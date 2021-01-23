using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
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
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Buffs.Conditions;
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
    public class Psychic
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass psychic_class;
        static public BlueprintProgression psychic_progression;

        static public BlueprintFeature psychic_proficiencies;
        static public BlueprintFeatureSelection psychic_discipline;
        static public BlueprintFeatureSelection psychic_discipline_no_spells;

        static public BlueprintFeature psychic_spellcasting;
        static public BlueprintFeature extra_phrenic_pool;
        static public BlueprintFeatureSelection extra_phrenic_amplification;
        static public BlueprintAbilityResource phrenic_pool_resource;

        static public BlueprintFeature focused_force;
        static public BlueprintFeature ongoing_defense;
        static public BlueprintFeature biokinetic_healing;
        static public BlueprintFeature conjured_armor;
        static public BlueprintFeature defensive_prognostication;
        static public BlueprintFeature minds_eye;
        static public BlueprintFeature overpowering_mind;
        static public BlueprintFeature will_of_the_dead;
        static public BlueprintFeature relentness_casting;
        static public BlueprintFeature undercast_surge;
        static public BlueprintFeature psychofeedback;
        static public BlueprintFeature synaptic_shock;
        static public BlueprintFeature space_rending_spell;
        static public BlueprintFeature[] mimic_metamagic;
        static public BlueprintFeature dual_amplification;

        static public BlueprintFeature phrenic_pool;
        static public BlueprintFeatureSelection phrenic_amplification;
        static public BlueprintFeature major_amplification;


        static public BlueprintArchetype magaambyan_telepath;
        static public BlueprintArchetype starseeker;
        static public BlueprintArchetype mutation_mind;
        static public BlueprintArchetype psychic_maraudeur;
        static public BlueprintArchetype amnesiac;

        static public BlueprintFeature phrenic_mastery;

        static public BlueprintFeature natures_command;
        static public BlueprintFeatureSelection[] primal_magic = new BlueprintFeatureSelection[9];

        static public BlueprintAbility written_in_stars_ability;
        static public BlueprintAbilityResource written_in_stars_resource;
        static public BlueprintFeature written_in_stars;

        static public BlueprintSpellbook amnesiac_spellbook;
        static public BlueprintFeature amnesiac_spell_casting;
        static public BlueprintFeature spell_recollection;

        static public BlueprintBuff physical_mutation_buff;
        static public BlueprintFeature physical_mutation;

        static public BlueprintFeature bodily_mutations;
        static public BlueprintFeature adhesive;
        static public BlueprintFeature bite_attack;
        static public BlueprintFeature claws;
        static public BlueprintFeature elongated_legs;
        static public BlueprintFeature elongated_torso;
        static public BlueprintFeature rubbery;
        static public BlueprintFeature slimy;

        static public BlueprintFeature improved_bodily_mutations;
        static public BlueprintFeature enlarged_body;
        static public BlueprintFeature multiple_eyes;
        static public BlueprintFeature recuperation;
        static public BlueprintFeature wings;

        static public BlueprintFeature[] aura_of_insanity = new BlueprintFeature[3];
        static public BlueprintFeature skewed_mentality;
        static public BlueprintFeature cracked_perspectives;
        static public BlueprintFeature unreal_understanding;


        static Dictionary<string, BlueprintProgression> psychic_disiciplines_map = new Dictionary<string, BlueprintProgression>();
        static Dictionary<string, BlueprintProgression> no_spells_psychic_disiciplines_map = new Dictionary<string, BlueprintProgression>();

        internal static void createPsychicClass()
        {
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            var sorceror_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");

            psychic_class = Helpers.Create<BlueprintCharacterClass>();
            psychic_class.name = "PsychicClass";
            library.AddAsset(psychic_class, "");

            psychic_class.LocalizedName = Helpers.CreateString("Psychic.Name", "Psychic");
            psychic_class.LocalizedDescription = Helpers.CreateString("Psychic.Description",
                "Within the mind of any sentient being lies power to rival that of the greatest magical artifact or holy site. By accessing these staggering vaults of mental energy, the psychic can shape the world around her, the minds of others, and pathways across the planes. No place or idea is too secret or remote for a psychic to access, and she can pull from every type of psychic magic. Many methods allow psychics to tap into their mental abilities, and the disciplines they follow affect their abilities.\n"
                + "Role: With a large suite of spells, psychics can handle many situations, but they excel at moving and manipulating objects, as well as reading and influencing thoughts."
                );
            psychic_class.m_Icon = sorceror_class.Icon;
            psychic_class.SkillPoints = wizard_class.SkillPoints;
            psychic_class.HitDie = DiceType.D6;
            psychic_class.BaseAttackBonus = wizard_class.BaseAttackBonus;
            psychic_class.FortitudeSave = wizard_class.FortitudeSave;
            psychic_class.ReflexSave = wizard_class.ReflexSave;
            psychic_class.WillSave = wizard_class.WillSave;
            psychic_class.Spellbook = createPsychicSpellbook();

            psychic_class.ClassSkills = new StatType[] {StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion, StatType.SkillPerception,
                                                      StatType.SkillPersuasion};
            psychic_class.IsDivineCaster = false;
            psychic_class.IsArcaneCaster = false;
            psychic_class.StartingGold = wizard_class.StartingGold;
            psychic_class.PrimaryColor = wizard_class.PrimaryColor;
            psychic_class.SecondaryColor = wizard_class.SecondaryColor;
            psychic_class.RecommendedAttributes = wizard_class.RecommendedAttributes;
            psychic_class.NotRecommendedAttributes = wizard_class.NotRecommendedAttributes;
            psychic_class.EquipmentEntities = wizard_class.EquipmentEntities;
            psychic_class.MaleEquipmentEntities = wizard_class.MaleEquipmentEntities;
            psychic_class.FemaleEquipmentEntities = wizard_class.FemaleEquipmentEntities;
            psychic_class.ComponentsArray = sorceror_class.ComponentsArray;
            psychic_class.StartingItems = new Kingmaker.Blueprints.Items.BlueprintItem[] {library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("511c97c1ea111444aa186b1a58496664"), //crossbow
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("ada85dae8d12eda4bbe6747bb8b5883c"), // quarterstaff
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("63caf94a780472b448f50d0bc183c38f"), //s. magic missile
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("63caf94a780472b448f50d0bc183c38f"), //s. magic missile
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("e8308a74821762e49bc3211358e81016"), //s. mage armor
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("f79c3fd5012a3534c8ab36dc18e85fb1") //s. sleep
                                                                                       };
            createPsychicProgression();
            psychic_class.Progression = psychic_progression;

            createMagaambyanTelepath();
            createEsotericStarseeker();
            createAmnesiac();
            createMutationMind();
            createPsychicMaraudeur();
            psychic_class.Archetypes = new BlueprintArchetype[] {amnesiac, magaambyan_telepath, starseeker, mutation_mind, psychic_maraudeur };
            Helpers.RegisterClass(psychic_class);
            createPsychicFeats();
        }


        static void createPsychicMaraudeur()
        {
            psychic_maraudeur = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "PsychicMaraudeurArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Psychic Marauder");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "When one encounters aberrant creatures, it can unlock psychic potential within a person, though this experience is traumatic for those who endure it. The psychic marauder’s powers come hand in hand with madness, either from exposure to alien psyches or from terrifying glimpses into the cosmos that the psychic can’t suppress. But she does not crumble under the strain of this insanity; instead, she cordons it off to use as a shield against other psychic intrusions and harnesses it to unleash it upon the world around her.");
            });
            Helpers.SetField(psychic_maraudeur, "m_ParentClass", psychic_class);
            library.AddAsset(psychic_maraudeur, "");

            createSkewedMentality();
            createAuraOfInsanity();
            createCrackedPerspectives();
            createUnrealUnderstanding();

            psychic_maraudeur.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(3, phrenic_amplification),
                                                              Helpers.LevelEntry(11, phrenic_amplification),
                                                              Helpers.LevelEntry(19, phrenic_amplification),
                                                              Helpers.LevelEntry(20, phrenic_mastery)};
            psychic_maraudeur.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(2, skewed_mentality),
                                                          Helpers.LevelEntry(3, aura_of_insanity[0]),
                                                          Helpers.LevelEntry(9, cracked_perspectives),
                                                          Helpers.LevelEntry(11, aura_of_insanity[1]),
                                                          Helpers.LevelEntry(19, aura_of_insanity[2]),
                                                          Helpers.LevelEntry(20, unreal_understanding),
                                                        };
            psychic_class.Progression.UIGroups = psychic_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(aura_of_insanity.AddToArray(skewed_mentality, cracked_perspectives, unreal_understanding)));
        }


        static void createAuraOfInsanity()
        {
            var confusion_buff = library.Get<BlueprintBuff>("886c7407dc629dc499b9f1465ff382df");

            aura_of_insanity[0] = Helpers.CreateFeature("AuraOfInsanityIFeature",
                                                        "Aura of Insanity I",
                                                        "At 3rd level, a psychic marauder’s madness begins to leak out. As a standard action, the psychic marauder can spend 1 point from her phrenic pool to manifest an aura of insanity, letting bits of her psyche penetrate the minds of those around her. The aura is centered on the psychic marauder and has a radius of 10 feet, increasing by 5 feet for every 3 class levels beyond 3rd. Any hostile creature starting its turn within the aura must succeed at a Will save (DC = 10 + half the psychic marauder’s level + the psychic marauder’s Intelligence modifier) or be confused for 1 round. The aura of insanity lasts for a number of rounds equal to the psychic marauder’s Charisma modifier. This is a mind-affecting effect.",
                                                        "",
                                                        confusion_buff.Icon,
                                                        FeatureGroup.None
                                                        );

            aura_of_insanity[1] = Helpers.CreateFeature("AuraOfInsanityIIFeature",
                                            "Aura of Insanity II",
                                            "At 11th level, the psychic can decide to spend 1 extra phrenic pool point when using aura of insanity to force affected creatures to always reroll \'acts normally\' confused condition.",
                                            "",
                                            confusion_buff.Icon,
                                            FeatureGroup.None
                                            );

            aura_of_insanity[2] = Helpers.CreateFeature("AuraOfInsanityIIIFeature",
                                                        "Aura of Insanity III",
                                                        "At 19th level, the psychic can decide to spend 2 extra phrenic pool points when using aura of insanity to select the result of the affected creatures’ confused condition without rolling.",
                                                        "",
                                                        confusion_buff.Icon,
                                                        FeatureGroup.None
                                                        );

            var confused_states = new ConfusionState[] { ConfusionState.AttackNearest, ConfusionState.DoNothing, ConfusionState.SelfHarm };
            var no_act_normally_buff = Helpers.CreateBuff("NoActNormallyConfusionBuff",
                                                          "Reroll \'Acts Normally\' Confused Condition",
                                                          "The creature under confusion effect will always reroll beahviour dice if it results in \'Acts normally\'.",
                                                          "",
                                                          confusion_buff.Icon,
                                                          null,
                                                          Helpers.Create<ConfusionControl.ControlConfusionBuff>(c => c.allowed_states = confused_states)
                                                          );
            var apply_specific_confusion_buffs = new List<GameAction>();
            var specific_confusion_buffs = new List<BlueprintBuff>();

            foreach (var cs in confused_states)
            {
                var state_string = Regex.Replace(cs.ToString(), "([a-z])([A-Z])", "$1 $2");
                var specific_buff = Helpers.CreateBuff(cs.ToString() + "ConfusionBuff",
                                                       $"Enforce \'{state_string}\' Condition",
                                                       "The creature under confusion effect will always roll enforced condition.",
                                                       "",
                                                       confusion_buff.Icon,
                                                       null,
                                                       Helpers.Create<ConfusionControl.ControlConfusionBuff>(c => c.allowed_states = new ConfusionState[] { cs })
                                                       );
                GameAction apply_confusion3 = Helpers.CreateActionSavingThrow(SavingThrowType.Will,
                                          Helpers.CreateConditionalSaved(new GameAction[0],
                                                                         new GameAction[]{Common.createContextActionApplyBuff(confusion_buff, Helpers.CreateContextDuration(1), dispellable: false),
                                                                                                      Common.createContextActionApplyBuff(specific_buff, Helpers.CreateContextDuration(1), dispellable: false)
                                                                                         }
                                                                        )
                                         );
                apply_confusion3 = Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), apply_confusion3);
                apply_specific_confusion_buffs.Add(apply_confusion3);
                specific_confusion_buffs.Add(specific_buff);
            }


            GameAction apply_confusion = Helpers.CreateActionSavingThrow(SavingThrowType.Will,
                                                                  Helpers.CreateConditionalSaved(null,
                                                                                                 Common.createContextActionApplyBuff(confusion_buff, Helpers.CreateContextDuration(1), dispellable: false)
                                                                                                )
                                                                 );
            apply_confusion = Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), apply_confusion);
            GameAction apply_confusion2 = Helpers.CreateActionSavingThrow(SavingThrowType.Will,
                                                      Helpers.CreateConditionalSaved(new GameAction[0],
                                                                                     new GameAction[]{Common.createContextActionApplyBuff(confusion_buff, Helpers.CreateContextDuration(1), dispellable: false),
                                                                                                      Common.createContextActionApplyBuff(no_act_normally_buff, Helpers.CreateContextDuration(1), dispellable: false)
                                                                                                     }
                                                                                    )
                                                     );
            apply_confusion2 = Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), apply_confusion2);

            for (int i = 0; i < 6; i++)
            {
                var area1 = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7fc8dbff8ba688d4b864b1c1be45fe97", $"AuraOfInsanityI{i+1}Area", "");
                area1.Size = (10 + i * 5).Feet();
                area1.ComponentsArray = new BlueprintComponent[]
                {
                    Helpers.CreateAreaEffectRunAction(round: apply_confusion),
                    Common.createContextCalculateAbilityParamsBasedOnClass(psychic_class, StatType.Intelligence),
                    Helpers.CreateSpellDescriptor(SpellDescriptor.Confusion | SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting)
                };

                var buff1 = Helpers.CreateBuff($"AuraOfInsanityI{i + 1}Buff",
                                               aura_of_insanity[0].Name,
                                               aura_of_insanity[0].Description,
                                               "",
                                               aura_of_insanity[0].Icon,
                                               null,
                                               Common.createAddAreaEffect(area1)
                                               );

                var ability1 = Helpers.CreateAbility($"AuraOfInsanityI{i + 1}Ability",
                                                       aura_of_insanity[0].Name,
                                                       aura_of_insanity[0].Description,
                                                       "",
                                                       aura_of_insanity[0].Icon,
                                                       AbilityType.Supernatural,
                                                       CommandType.Standard,
                                                       AbilityRange.Personal,
                                                       "Charisma modifier rounds",
                                                       "",
                                                       Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff1,
                                                                                                                    Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                                                    dispellable: false)
                                                                               ),
                                                       Common.createAbilitySpawnFx("c14a2f46018cb0e41bfeed61463510ff", anchor: AbilitySpawnFxAnchor.SelectedTarget, position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None),
                                                       phrenic_pool_resource.CreateResourceLogic(),
                                                       Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Charisma),
                                                       Common.createAbilityAoERadius((10 + i * 5).Feet(), TargetType.Enemy)
                                                       );
                ability1.setMiscAbilityParametersSelfOnly();

                var feature1 = Common.AbilityToFeature(ability1);

                var area2 = library.CopyAndAdd<BlueprintAbilityAreaEffect>(area1, $"AuraOfInsanityII{i + 1}Area", "");
                area2.ComponentsArray = new BlueprintComponent[]
                {
                    Helpers.CreateAreaEffectRunAction(round: apply_confusion2),
                    Common.createContextCalculateAbilityParamsBasedOnClass(psychic_class, StatType.Intelligence)
                };

                var buff2 = Helpers.CreateBuff($"AuraOfInsanityII{i + 1}Buff",
                               aura_of_insanity[1].Name,
                               aura_of_insanity[1].Description,
                               "",
                               aura_of_insanity[1].Icon,
                               null,
                               Common.createAddAreaEffect(area2)
                               );

                var ability2 = Helpers.CreateAbility($"AuraOfInsanityII{i + 1}Ability",
                                       aura_of_insanity[1].Name,
                                       aura_of_insanity[1].Description,
                                       "",
                                       aura_of_insanity[1].Icon,
                                       AbilityType.Supernatural,
                                       CommandType.Standard,
                                       AbilityRange.Personal,
                                       "Charisma modifier rounds",
                                       "",
                                       Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff2,
                                                                                                    Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                                    dispellable: false)
                                                               ),
                                       Common.createAbilitySpawnFx("c14a2f46018cb0e41bfeed61463510ff", anchor: AbilitySpawnFxAnchor.SelectedTarget, position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None),
                                       phrenic_pool_resource.CreateResourceLogic(amount: 2),
                                       Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Charisma),
                                       Common.createAbilityAoERadius((10 + i * 5).Feet(), TargetType.Enemy)
                                       );
                ability2.setMiscAbilityParametersSelfOnly();

                var feature2 = Common.AbilityToFeature(ability2);

                List<BlueprintAbility> abilities3 = new List<BlueprintAbility>();

                for (int j = 0; j < specific_confusion_buffs.Count; j++)
                {
                    var area3 = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7fc8dbff8ba688d4b864b1c1be45fe97", $"AuraOfInsanityIII{i + 1}{j+1}Area", "");
                    area3.Size = (10 + i * 5).Feet();
                    area3.ComponentsArray = new BlueprintComponent[]
                    {
                    Helpers.CreateAreaEffectRunAction(round: apply_specific_confusion_buffs[j]),
                    Common.createContextCalculateAbilityParamsBasedOnClass(psychic_class, StatType.Intelligence),
                    Helpers.CreateSpellDescriptor(SpellDescriptor.Confusion | SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting)
                    };

                    var buff3 = Helpers.CreateBuff($"AuraOfInsanityIII{i + 1}{j+1}Buff",
                                                   aura_of_insanity[2].Name + "(" + specific_confusion_buffs[j].Name + ")",
                                                   aura_of_insanity[2].Description,
                                                   "",
                                                   aura_of_insanity[2].Icon,
                                                   null,
                                                   Common.createAddAreaEffect(area3)
                                                   );

                    var ability3 = Helpers.CreateAbility($"AuraOfInsanityIII{i + 1}{j+1}Ability",
                                                           buff3.Name,
                                                           aura_of_insanity[2].Description,
                                                           "",
                                                           aura_of_insanity[2].Icon,
                                                           AbilityType.Supernatural,
                                                           CommandType.Standard,
                                                           AbilityRange.Personal,
                                                           "Charisma modifier rounds",
                                                           "",
                                                           Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff3,
                                                                                                                        Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                                                        dispellable: false)
                                                                                   ),
                                                           Common.createAbilitySpawnFx("c14a2f46018cb0e41bfeed61463510ff", anchor: AbilitySpawnFxAnchor.SelectedTarget, position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None),
                                                           phrenic_pool_resource.CreateResourceLogic(amount: 3),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Charisma),
                                                           Common.createAbilityAoERadius((10 + i * 5).Feet(), TargetType.Enemy)
                                                           );
                    ability3.setMiscAbilityParametersSelfOnly();
                    abilities3.Add(ability3);
                }

                var wrapper3 = Common.createVariantWrapper($"AuraOfInsanityIII{i + 1}AbilityBase", "",  abilities3.ToArray());
                wrapper3.SetName(aura_of_insanity[2].Name);

                var feature3 = Common.AbilityToFeature(wrapper3);

                aura_of_insanity[0].AddComponent(Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a =>
                {
                    a.classes = getPsychicArray();
                    a.Feature = feature1;
                    a.min_level = (i + 1) * 3;
                    a.max_level = i == 5 ? 100 : (i + 2) * 3 - 1;
                }));

                aura_of_insanity[1].AddComponent(Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a =>
                {
                    a.classes = getPsychicArray();
                    a.Feature = feature2;
                    a.min_level = (i + 1) * 3;
                    a.max_level = i == 5 ? 100 : (i + 2) * 3 - 1;
                }));

                aura_of_insanity[2].AddComponent(Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a =>
                {
                    a.classes = getPsychicArray();
                    a.Feature = feature3;
                    a.min_level = (i + 1) * 3;
                    a.max_level = i == 5 ? 100 : (i + 2) * 3 - 1;
                }));
            }

        }

        static void createSkewedMentality()
        {
            skewed_mentality = Helpers.CreateFeature("SkewedMentalityFeature",
                                                     "Skewed Mentality",
                                                     "At 2nd level, a psychic marauder becomes alienated from reality as others perceive it. The psychic marauder uses her Charisma modifier on Will saves instead of her Wisdom modifier.",
                                                     "",
                                                     null,
                                                     FeatureGroup.None,
                                                     Helpers.Create<StatReplacementMechanics.ReplaceStatForSavingthrow>(r =>
                                                     {
                                                         r.dependent_value = StatType.SaveWill;
                                                         r.stat = StatType.Charisma;
                                                     })
                                                     );
        }


        static void createCrackedPerspectives()
        {
            cracked_perspectives = Helpers.CreateFeature("CrackedPerspectivesFeature",
                                                     "Cracked Perspectives",
                                                     "At 9th level, a psychic marauder’s growing madness renders her immune to confusion and insanity effects.",
                                                     "",
                                                     Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"), //mind fog
                                                     FeatureGroup.None,
                                                     Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Confusion),
                                                     Common.createBuffDescriptorImmunity(SpellDescriptor.Confusion),
                                                     Common.createAddConditionImmunity(UnitCondition.Confusion)
                                                     );
        }


        static void createUnrealUnderstanding()
        {
            unreal_understanding = Helpers.CreateFeature("UnrealUnderstandingFeature",
                                                     "Unreal Understanding",
                                                     "At 20th level, a psychic marauder’s mindset becomes completely aberrant, to the point that no outside force can penetrate her psyche. Because of this, the psychic marauder becomes immune to all mind-affecting effects.",
                                                     "",
                                                     Helpers.GetIcon("b3da3fbee6a751d4197e446c7e852bcb"), //true seeing
                                                     FeatureGroup.None,
                                                     Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.MindAffecting),
                                                     Common.createBuffDescriptorImmunity(SpellDescriptor.MindAffecting)
                                                     );
        }




        static void createMutationMind()
        {
            mutation_mind = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MutationMindArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Mutation Mind");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Exposure to unintended spell effects, curses, or sources of radiation cause some to manifest psychic powers. When a mutation mind uses her psychic abilities, her physical body changes under the stress, and she risks losing control if she pushes too far.");
            });
            Helpers.SetField(mutation_mind, "m_ParentClass", psychic_class);
            library.AddAsset(mutation_mind, "");
            createPhysicalMutation();
            createBodilyMutations();
            createImprovedBodilyMutations();
            mutation_mind.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, phrenic_amplification) };
            mutation_mind.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, physical_mutation),
                                                          Helpers.LevelEntry(3, bodily_mutations),
                                                          Helpers.LevelEntry(11, improved_bodily_mutations)
                                                        };
            psychic_class.Progression.UIGroups = psychic_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(physical_mutation, bodily_mutations, improved_bodily_mutations));
        }


        static void createBodilyMutations()
        {
            bodily_mutations = Helpers.CreateFeature("BodilyMutationsFeature",
                                                     "Bodily Mutations",
                                                     "Starting at 3rd level, whenever a mutation mind gains a phrenic amplification, she can select either a phrenic amplification or a bodily mutation. A bodily mutation grants its benefit only when the mutation mind is affected by her physical mutation. Each time she activates her physical mutation, she can activate any number of bodily mutations she possesses.",
                                                     "",
                                                     Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"), //enlarge person
                                                     FeatureGroup.None);

            adhesive = createBodilyMutation("Adhesive",
                                             "Adhesive",
                                             "The mutation mind’s skin becomes sticky, granting her a +4 bonus on disarm and grapple combat maneuver checks.",
                                             Helpers.GetIcon("6b30813c3709fc44b92dc8fd8191f345"),
                                             null,
                                             false,
                                             Helpers.Create<ManeuverBonus>(m => { m.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.Disarm; m.Bonus = 4; }),
                                             Helpers.Create<ManeuverBonus>(m => { m.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.Grapple; m.Bonus = 4; })
                                             );

            bite_attack = createBodilyMutation("BiteAttack",
                                             "Bite Attack",
                                             "The mutation mind’s mouth fills with sharp teeth, allowing her to make a bite attack as part of a full-attack action at her highest base attack bonus. This attack deals 1d4 points of damage (1d3 if Small) and counts as a primary attack unless combined with manufactured weapon attacks, as normal.",
                                             Helpers.GetIcon("25954b1652bebc2409f9cb9d5728bceb"),
                                             null,
                                             false,
                                             Common.createAddAdditionalLimb(library.Get<BlueprintItemWeapon>("8c01e7fccbb829947bc5894f63fb389a"))
                                             );

            claws = createBodilyMutation("Claws",
                                         "Claws",
                                         "The mutation mind’s hands turn into claws, allowing her to make two claw attacks as a full-attack action at her highest base attack bonus. Each of these attacks deals 1d4 points of damage (1d3 if Small) and counts as a primary attack unless combined with manufactured weapon attacks, as normal.",
                                         Helpers.GetIcon("e17698ef77dc3174cbd0a42422c22441"),
                                         null,
                                         false,
                                         Common.createEmptyHandWeaponOverride(library.Get<BlueprintItemWeapon>("289c13ba102d0df43862a488dad8a5d5"))
                                         );

            elongated_legs = createBodilyMutation("ElongatedLegs",
                                         "Elongated Legs",
                                         "The mutation mind’s legs become long and spindly, increasing her base movement by 10 feet.",
                                         Helpers.GetIcon("486eaff58293f6441a5c2759c4872f98"),
                                         null,
                                         false,
                                         Helpers.CreateAddStatBonus(StatType.Speed, 10, ModifierDescriptor.UntypedStackable)
                                         );

            elongated_torso = createBodilyMutation("ElongatedTorso",
                                                 "Elongated Torso",
                                                 "The mutation mind’s torso enlarges disproportionately with the rest of her body, granting her a +2 natural armor bonus to AC.",
                                                 Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"),
                                                 null,
                                                 false,
                                                 Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.NaturalArmor)
                                                 );

            rubbery = createBodilyMutation("Rubbery",
                                     "Rubbery",
                                     "The mutation mind’s body becomes soft and rubbery, granting her DR 2/—.",
                                     NewSpells.resinous_skin.Icon,
                                     null,
                                     false,
                                     Common.createPhysicalDR(2)
                                     );

            slimy = createBodilyMutation("Slimy",
                                 "Slimy",
                                 "The mutation mind exudes a thin layer of oily slime over her body, granting her a +4 bonus to CMD.",
                                 LoadIcons.Image2Sprite.Create(@"AbilityIcons/Slimy.png"),
                                 null,
                                 false,
                                 Helpers.CreateAddStatBonus(StatType.AdditionalCMD, 4, ModifierDescriptor.UntypedStackable)
                                 );

        }


        static void createImprovedBodilyMutations()
        {
            improved_bodily_mutations = Helpers.CreateFeature("ImprovedBodilyMutationsFeature",
                                                     "Improved Bodily Mutations",
                                                     "At 11th level, a mutation mind can select one of the improved bodily mutations instead of a phrenic amplification.",
                                                     "",
                                                     Helpers.GetIcon("93d9d74dac46b9b458d4d2ea7f4b1911"), //polymorph
                                                     FeatureGroup.None);

            enlarged_body = createBodilyMutation("EnlargedBody",
                                                 "Enlarged Body",
                                                 "The mutation mind’s body swells and stretches, enlarging her as the enlarge person spell but by 2 size categories. Mutation mind receives + 4 size bonus to strength and -2 bonus to dexterity.",
                                                 Helpers.GetIcon("3dccdf27a8209af478ac71cded18a271"),
                                                 null,
                                                 true,
                                                 Helpers.CreateAddStatBonus(StatType.Strength, 4, ModifierDescriptor.Size),
                                                 Helpers.CreateAddStatBonus(StatType.Dexterity, -2, ModifierDescriptor.Size),
                                                 Helpers.Create<ChangeUnitSize>(c => c.SizeDelta  = 2)
                                                 );


            multiple_eyes = createBodilyMutation("MultipleEyes",
                                                 "Multiple Eyes",
                                                 "Multiple eyes erupt all over the mutation mind’s body. She can’t be flanked and gains a +4 bonus on Perception checks.",
                                                 NewSpells.countless_eyes.Icon,
                                                 null,
                                                 true,
                                                 Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.CannotBeFlanked),
                                                 Helpers.CreateAddStatBonus(StatType.SkillPerception, 4, ModifierDescriptor.UntypedStackable)
                                                 );

            recuperation = createBodilyMutation("Recuperation",
                                                 "Recuperation",
                                                 "The mutation mind heals quickly from physical wounds, gaining fast healing 5.",
                                                 Helpers.GetIcon("aad496a7c9bee224cbd01a3cf8e42061"),
                                                 null,
                                                 true,
                                                 Common.createAddContextEffectFastHealing(5)
                                                 );


            wings = createBodilyMutation("Wings",
                                     "Wings",
                                     "The mutation mind sprouts a pair of fleshy, batlike wings.",
                                     Helpers.GetIcon("78197196e096c6e4eaed5c62fa108b52"),
                                     Common.createPrefabLink("7662eda306be77a4ab8f57dbf1235cc9"),
                                     true,
                                     FixFlying.airborne.ComponentsArray
                                     );
        }


        static BlueprintFeature createBodilyMutation(string name, string display_name, string description, UnityEngine.Sprite icon,
                                         PrefabLink fx, bool is_improved, params BlueprintComponent[] components)
        {
            var feature = Helpers.CreateFeature(name + "BodilyMutationFeature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.PrerequisiteFeature(bodily_mutations));

            if (is_improved)
            {
                feature.AddComponent(Helpers.PrerequisiteFeature(improved_bodily_mutations));
            }
            phrenic_amplification.AllFeatures = phrenic_amplification.AllFeatures.AddToArray(feature);

            var buff = Helpers.CreateBuff(name + "BodilyMutationBuff",
                                          display_name,
                                          description,
                                          "",
                                          icon,
                                          fx,
                                          components);

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(physical_mutation_buff, buff, feature);

            return feature;
        }


        static void createPhysicalMutation()
        {
            var resource = Helpers.CreateAbilityResource("PhysicalMutationResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, getPsychicArray());
            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var exhausted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");

            physical_mutation_buff = Helpers.CreateBuff("PhysicalMutationBuff",
                                                        "Physical Mutation",
                                                        "A mutation mind’s psychic powers warp her body, allowing her to physically mutate herself as a swift action, which grants her a +4 alchemical bonus to Strength and a –2 penalty to Intelligence. At 12th level, the bonus to Strength increases to +6, and at 16th level to +8. A mutation mind can assume her physically mutated form for a number of minutes per day equal to her psychic level. The minutes don’t have to be used consecutively, but must be used in 1-minute increments. After the mutation ends, the mutation mind is fatigued and can’t activate a physical mutation again for 1 round.",
                                                        "",
                                                        Helpers.GetIcon("489c8c4a53a111d4094d239054b26e32"),
                                                        Common.createPrefabLink("3b42d149d55dfe5469d9967f22d7e020"), //bull strength
                                                        Helpers.CreateAddStatBonus(StatType.Intelligence, -2, ModifierDescriptor.UntypedStackable),
                                                        Helpers.CreateAddContextStatBonus(StatType.Strength, ModifierDescriptor.Alchemical),
                                                        Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getPsychicArray(),
                                                                                        progression: ContextRankProgression.Custom,
                                                                                        customProgression: new (int, int)[] { (11, 4), (15, 6), (20, 8) }),
                                                        Helpers.CreateAddFactContextActions(deactivated:
                                                                                            Common.createContextActionApplyBuff(fatigued, Helpers.CreateContextDuration(1), dispellable: false)
                                                                                            )
                                                        );

            var ability = Helpers.CreateAbility("PhysicalMutationAbility",
                                                physical_mutation_buff.Name,
                                                physical_mutation_buff.Description,
                                                "",
                                                physical_mutation_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(physical_mutation_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)),
                                                Common.createAbilityCasterHasNoFacts(physical_mutation_buff),
                                                Helpers.Create<NewMechanics.AbilityCasterHasCondition>(a => { a.Condition = UnitCondition.Fatigued; a.Not = true; }),
                                                Helpers.Create<NewMechanics.AbilityCasterHasCondition>(a => { a.Condition = UnitCondition.Exhausted; a.Not = true; }),
                                                Common.createAbilityCasterHasNoFacts(BlueprintRoot.Instance.SystemMechanics.FatigueBuff),
                                                Common.createAbilityCasterHasNoFacts(BlueprintRoot.Instance.SystemMechanics.ExhaustedBuff),
                                                resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            physical_mutation = Common.AbilityToFeature(ability, false);
            physical_mutation.AddComponent(resource.CreateAddAbilityResource());
        }


        static void createAmnesiac()
        {
            amnesiac = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "AmnesiacArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Amnesiac");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The amnesiac once possessed great psychic power, but mental blocks—resulting from either a traumatic event or intentional implantation—have caused her to forget what she knew before. The amnesiac’s struggle to control her psychic magic leads to wild and unpredictable results.");
            });
            Helpers.SetField(amnesiac, "m_ParentClass", psychic_class);
            library.AddAsset(amnesiac, "");
            createAmnesiacSpellbook();
            createAmnesiacSpellcasting();
            amnesiac.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, psychic_spellcasting)};
            amnesiac.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, amnesiac_spell_casting, spell_recollection)
                                                    };
            
            amnesiac.ReplaceSpellbook = amnesiac_spellbook;
            psychic_class.Progression.UIDeterminatorsGroup = psychic_class.Progression.UIDeterminatorsGroup.AddToArray(amnesiac_spell_casting, spell_recollection);
        }


        static void createAmnesiacSpellcasting()
        {
            amnesiac_spell_casting = Helpers.CreateFeature("AmnesiacSpellcastingFeature",
                                                           "Amnesiac Spellcasting",
                                                           "An amnesiac’s ability to cast psychic spells is the same as that of the psychic class, with the following exceptions.\n"
                                                           + "An amnesiac’s faulty memory makes remembering and casting spells difficult, but the increased flexibility can be a great benefit. Instead of choosing a number of spells known as psychic, an amnesiac accesses spells she knew the previous day from the recesses of her mind.\n"
                                                           + "For each spell level the amnesiac can cast, she retains a number of spells known equal to half the usual number for psychic spells known. The remainder of her spells known  become amnesia slots, which the amnesiac can use with her spell recollection ability.\n"
                                                           + "This change to spells doesn’t apply to knacks (0-level spells) or discipline spells, which function the same way they do for a normal psychic. When the amnesiac gains access to 3rd-level spells, she gains full recall of her 1st-level spells and no longer gains 1st-level amnesia slots, instead gaining the full number of 1st-level spells known and casting them as a normal psychic. When this happens, the amnesiac replaces all her corresponding amnesia slots with any level-appropriate spells from the psychic spell list as her spells known, even if they were never among the spells she prepared or recalled; once selected, these spells can no longer be changed, as with a normal psychic. ach time the psychic gains access to a new level of spells, she gains full memory of spells 2 levels lower in the same way.",
                                                           "",
                                                           null,
                                                           FeatureGroup.None);
            amnesiac_spell_casting.ComponentsArray = psychic_spellcasting.ComponentsArray.ToList().ToArray();
            amnesiac_spell_casting.ReplaceComponent<SpellFailureMechanics.PsychicSpellbook>(p => p.spellbook = amnesiac_spellbook);
            amnesiac_spell_casting.ReplaceComponent<SpellbookMechanics.AddUndercastSpells>(a => a.spellbook = amnesiac_spellbook);

            spell_recollection = Helpers.CreateFeature("SpellRecollectionFeature",
                                                       "Spell Recollection",
                                                       "Once per hour as a swift action, an amnesiac can attempt to remember any spell from the psychic spell list of her choice from either of the 2 highest spell levels she can cast. When she does, she rolls on d100 to determine the result. Because the mental stress of combat brings memories to the surface more easily, the amnesiac adds 1d10 to this roll’s result if she’s in combat when she attempts to recall a spell. Regardless of the result, the amnesiac expends an amnesia slot of the appropriate level for the spell she is attempting to remember.\n"
                                                       + "Once a spell has been remembered in this way, the amnesiac can cast it as one of her spells known for the rest of the day (even if she failed to cast the spell during the round in which she remembered it).\n"
                                                       + "Spell Recollection d100 effects:\n"
                                                       + "01-10: The amnesiac is unable to cast spells this round.\n"
                                                       + "11-35: The amnesiac can’t remember the new spell (but can still cast spells this round).\n"
                                                       + "36-95: The amnesiac remembers and can cast the new spell.\n"
                                                       + "96+  : As 36-95, but amnesia slot is not expended.",
                                                       "",
                                                       Helpers.GetIcon("0a5ddfbcfb3989543ac7c936fc256889"),
                                                       FeatureGroup.None);

            var forbid_spellcasting_buff = Helpers.CreateBuff("SpellRecollectionSpellcastingForbiddenBuff",
                                                              spell_recollection.Name + ": Unable to Cast Spells",
                                                              "You are unable to cast spells until the end of the round.",
                                                              "",
                                                              spell_recollection.Icon,
                                                              null
                                                              );

            var apply_spellcsting_forbidden_buff = Common.createContextActionApplyBuff(forbid_spellcasting_buff, Helpers.CreateContextDuration(1), dispellable: false);

            var amnesiac_cooldown_buff = Helpers.CreateBuff("SpellRecollectionCooldownBuff",
                                                            "Spell Recollection Cooldown",
                                                            spell_recollection.Description,
                                                            "6a8651389c6348bb819b41b8ecdfe9fe",
                                                            spell_recollection.Icon,
                                                            null);
            var apply_cooldown = Common.createContextActionApplyBuff(amnesiac_cooldown_buff, Helpers.CreateContextDuration(1, DurationRate.Hours), dispellable: false);
            for (int i = 1; i <= 9; i++)
            {
                var spell_recollection_ability = Helpers.CreateAbility($"SpellRecollection{i}BaseAbility",
                                                       spell_recollection.Name + " " + Common.roman_id[i],
                                                       spell_recollection.Description,
                                                       "",
                                                       spell_recollection.Icon,
                                                       AbilityType.Extraordinary,
                                                       CommandType.Swift,
                                                       AbilityRange.Personal,
                                                       "",
                                                       "");
                spell_recollection_ability.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAbilityVariants(spell_recollection_ability) };
                spell_recollection_ability.setMiscAbilityParametersSelfOnly();

                var resource = Helpers.CreateAbilityResource($"SpellRecollection{i}Resource", "", "", "", null);
                if (i == 1)
                {
                    resource.SetIncreasedByLevelStartPlusDivStep(1, 5, 1, 100, 0, 0, 0.0f, getPsychicArray());
                }
                else
                {
                    resource.SetFixedResource(1);
                }
                var allowed_feature = Helpers.CreateFeature($"SpellRecollection{i}Feature",
                                                            "", "", "", null, FeatureGroup.None,
                                                            resource.CreateAddAbilityResource(),
                                                            Helpers.CreateAddFact(spell_recollection_ability));
                allowed_feature.HideInCharacterSheetAndLevelUp = true;
                allowed_feature.HideInUI = true;
                
                var spells = psychic_class.Spellbook.SpellList.SpellsByLevel[i].Spells;

                foreach (var s in spells)
                {
                    var buff = Helpers.CreateBuff($"SpellRecollection{i}" + s.name + "Buff",
                                                  "",
                                                  "",
                                                  Helpers.GuidStorage.hasStoredGuid($"SpellRecollection{i}" + s.name + "Buff") ? "" : Helpers.MergeIds("63c05fe75c4f4d79b71e4ffde4fa9752", s.AssetGuid),
                                                  null,
                                                  null,
                                                  Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => c.Resource = resource)),
                                                  Helpers.Create<SpellbookMechanics.TemporaryAddKnownSpell>(t => { t.spell = s; t.spell_level = i; t.character_class = psychic_class; })
                                                 );
                    buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath);

                    var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);


                    var ability = Helpers.CreateAbility($"SpellRecollection{i}" + s.name,
                                                         spell_recollection_ability.Name + " (" + s.Name + ")",
                                                         s.Description,
                                                         Helpers.GuidStorage.hasStoredGuid($"SpellRecollection{i}" + s.name) ? "" : Helpers.MergeIds("b82a9118725941b8b48a34b9cfff93f0", s.AssetGuid),
                                                         s.Icon,
                                                         AbilityType.Extraordinary,
                                                         CommandType.Swift,
                                                         AbilityRange.Personal,
                                                         "",
                                                         "",
                                                         resource.CreateResourceLogic(),
                                                         Helpers.Create<NewMechanics.AbilityCasterKnowsSpell>(a => { a.spellbook = amnesiac_spellbook; a.spell = s; a.not = true; }),
                                                         Helpers.Create<NewMechanics.AbilityShowIfCasterKnowsSpell>(a => { a.spellbook = amnesiac_spellbook; a.spell = s; a.not = true; }),
                                                         Helpers.CreateRunActions(Helpers.Create<RandomMechanics.RunActionDependingOnD100>(r =>
                                                                                 {
                                                                                     r.actions = new ActionList[]
                                                                                     {
                                                                                         Helpers.CreateActionList(apply_spellcsting_forbidden_buff, apply_cooldown),
                                                                                          Helpers.CreateActionList(apply_cooldown),
                                                                                         Helpers.CreateActionList(apply_buff, apply_cooldown),
                                                                                         Helpers.CreateActionList(apply_buff,
                                                                                                                  Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => c.Resource = resource),
                                                                                                                  apply_cooldown
                                                                                                                 )
                                                                                     };
                                                                                     r.thresholds = new int[] { 10, 35, 95, 1000 };
                                                                                     r.in_combat_bonus = Helpers.CreateContextDiceValue(DiceType.D10, 1, 0);
                                                                                 })
                                                                                 ),
                                                         Common.createAbilityCasterHasNoFacts(amnesiac_cooldown_buff)
                                                         );

                    ability.setMiscAbilityParametersSelfOnly();
                    spell_recollection_ability.addToAbilityVariants(ability);
                }
                spell_recollection.AddComponent(Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a =>
                                                {
                                                    a.classes = getPsychicArray();
                                                    a.min_level = (i == 1) ? 0 : i * 2 + 1;
                                                    a.max_level = (i == 1) ? 5 : (i >= 8 ? 20 : i * 2 + 3);
                                                    a.Feature = allowed_feature;
                                                })
                );
            }
        }

       static void createAmnesiacSpellbook()
       {
            amnesiac_spellbook = library.CopyAndAdd(psychic_class.Spellbook, "AmnesiacSpellbook", "");
            amnesiac_spellbook.SpellsKnown = createAmnesiacSpellsKnownTable();
            amnesiac_spellbook.Name = amnesiac.LocalizedName;
       }

        static public Kingmaker.Blueprints.Classes.Spells.BlueprintSpellsTable createAmnesiacSpellsKnownTable()
        {
            return Common.createSpellsTable("AmnesiacSpellsKnownTable", "",
                                       Common.createSpellsLevelEntry(),  //0
                                       Common.createSpellsLevelEntry(0, 1/*+1*/),  //1
                                       Common.createSpellsLevelEntry(0, 1/*+1*/),  //2
                                       Common.createSpellsLevelEntry(0, 2/*+1*/),  //3
                                       Common.createSpellsLevelEntry(0, 2/*+1*/, 1),  //4
                                       Common.createSpellsLevelEntry(0, 2/*+2*/, 1/*+1*/), //5
                                       Common.createSpellsLevelEntry(0, 4,       1/*+1*/, 1), //6
                                       Common.createSpellsLevelEntry(0, 5,       2/*+1*/, 1/*+1*/), //7
                                       Common.createSpellsLevelEntry(0, 5,       3,       1/*+1*/, 1), //8
                                       Common.createSpellsLevelEntry(0, 5,       4,       2/*+1*/, 1/*+1*/), //9
                                       Common.createSpellsLevelEntry(0, 5,       4,       3,       1/*+1*/, 1), //10
                                       Common.createSpellsLevelEntry(0, 5,       5,       4,       2/*+1*/, 1/*+1*/), //11
                                       Common.createSpellsLevelEntry(0, 5,       5,       4,       3,       1/*+1*/, 1), //12
                                       Common.createSpellsLevelEntry(0, 5,       5,       4,       4,       2/*+1*/, 1/*+1*/), //13
                                       Common.createSpellsLevelEntry(0, 5,       5,       4,       4,       3,       1/*+1*/, 1), //14
                                       Common.createSpellsLevelEntry(0, 5,       5,       4,       4,       4,       2/*+1*/, 1/*+1*/), //15
                                       Common.createSpellsLevelEntry(0, 5,       5,       4,       4,       4,       3,       1/*+1*/, 1), //16
                                       Common.createSpellsLevelEntry(0, 5,       5,       4,       4,       4,       3,       2/*+1*/, 1/*+1*/), //17
                                       Common.createSpellsLevelEntry(0, 5,       5,       4,       4,       4,       3,       3,       1/*+1*/, 1), //18
                                       Common.createSpellsLevelEntry(0, 5,       5,       4,       4,       4,       3,       3,       2/*+1*/, 1/*+1*/), //19
                                       Common.createSpellsLevelEntry(0, 5,       5,       4,       4,       4,       3,       3,       2/*+1*/, 2/*+1*/) //20
                                       );
        }


        static void createMagaambyanTelepath()
        {
            //nature's command
            var buff = Helpers.CreateBuff("NaturesCommandBuff",
                                          "Nature’s Command",
                                          "The Magaambyan telepath’s mind-affecting spells can affect even plant creatures. When she casts a mind-affecting spell, the Magaambyan telepath can spend 2 points from her phrenic pool to overcome a plant creature’s immunity to mind-affecting effects for the purposes of that spell. This ability functions even on mindless plant creatures.\n"
                                          + "At 11th-level she can treat plant creatures as if they understood her language for purpose of language-dependent spells or abilities.",
                                          "",
                                          Helpers.GetIcon("0fd00984a2c0e0a429cf1a911b4ec5ca"),
                                          null,
                                            Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(m =>
                                            {
                                                m.Descriptor = SpellDescriptor.MindAffecting;
                                                m.amount = 2;
                                                m.resource = phrenic_pool_resource;
                                                m.Metamagic = (Metamagic)MetamagicFeats.MetamagicExtender.VerdantSpell;
                                                m.specific_class = psychic_class;
                                            })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             phrenic_pool_resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                             Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = phrenic_pool_resource; r.amount = 2; })
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            natures_command =  Common.ActivatableAbilityToFeature(toggle, false);
            natures_command.AddComponent(Helpers.CreateAddFeatureOnClassLevel(Common.plant_arcana_language_hidden, 11, getPsychicArray()));


            //primal spells
            var druid_spell_list = library.Get<BlueprintSpellList>("bad8638d40639d04fa2f80a1cac67d6b");
           
            var combined_spell_list = Common.combineSpellLists("MagaambyanTelepathPrimalMagicSpellList", druid_spell_list);
            Common.excludeSpellsFromList(combined_spell_list, psychic_class.Spellbook.SpellList);

            for (int i = 1; i <= 9; i++)
            {
                primal_magic[i - 1] = Helpers.CreateFeatureSelection($"MagaambyanTelepathPrimalMagic{i}FeatureSelection",
                                                "Primal Spells " + $"(Level {i})",
                                                "A Magaambyan telepath adds one 1st-level spell from the druid spell list to the psychic spell list and her spells known. Each time a Magaambyan telepath gains the ability to cast a new level of spell, she can add one spell of that level from the druid spell list to both the psychic spell list and her spells known. She casts these spells as psychic spells, and once the spells are selected these choices cannot be changed.\n"
                                                + "A Magaambyan telepath does not gain discipline spells.",
                                                "",
                                                null,
                                                FeatureGroup.None);
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956", $"MagaambyanTelepathPrimalMagic{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = psychic_class;
                learn_spell.SpellList = combined_spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = combined_spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = psychic_class; });
                learn_spell.SetName(Helpers.CreateString($"MagaambyanTelepathPrimalMagic{i}ParametrizedFeature.Name", "Primal Spells " + $"(level {i})"));
                learn_spell.SetDescription(primal_magic[i - 1].Description);
                learn_spell.SetIcon(primal_magic[i - 1].Icon);
                primal_magic[i - 1].AllFeatures = new BlueprintFeature[] { learn_spell };
            }


            magaambyan_telepath = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MagaambyanTelepathArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Magaambyan Telepath");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While most students of the Magaambya in the Mwangi Expanse focus on the blending of arcane and divine magic, followers of Old-Mage Jatembe’s more esoteric wisdom instead focus on the occult connection between all living creatures and the resonance between the natural and supernatural world. These Magaambyan telepaths hone their mental abilities to detect such spiritual and eldritch bonds, and attempt to commune with the mind of the jungle or the hearts of its wild inhabitants. Whether seeking information that only nature knows or attempting to impose their will upon the natural world, these psychics employ techniques that few outside of the Magaambya understand.");
            });
            Helpers.SetField(magaambyan_telepath, "m_ParentClass", psychic_class);
            library.AddAsset(magaambyan_telepath, "");

            magaambyan_telepath.RemoveFeatures = new LevelEntry[] {
                                                                   Helpers.LevelEntry(1, psychic_discipline, phrenic_amplification),
                                                                   Helpers.LevelEntry(11, phrenic_amplification)
                                                                };
            magaambyan_telepath.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, psychic_discipline_no_spells, natures_command, primal_magic[0]),
                                                               Helpers.LevelEntry(4, primal_magic[1]),
                                                               Helpers.LevelEntry(6, primal_magic[2]),
                                                               Helpers.LevelEntry(8, primal_magic[3]),
                                                               Helpers.LevelEntry(10, primal_magic[4]),
                                                               Helpers.LevelEntry(12, primal_magic[5]),
                                                               Helpers.LevelEntry(14, primal_magic[6]),
                                                               Helpers.LevelEntry(16, primal_magic[7]),
                                                               Helpers.LevelEntry(18, primal_magic[8])
                                                             };
            psychic_class.Progression.UIGroups = psychic_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(primal_magic));
        }

        static void createEsotericStarseeker()
        {
            createEsotericStarseekerWrittenInTheStars();
            starseeker = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "EsptericStarseekerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Esoteric Starseeker");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The celestial bodies travel through the sky in fixed patterns, and knowledgeable astrologers rely on these stellar constants to make accurate predictions, but the constellations can also be a source of power. An esoteric starseeker is a psychic who focuses her studies on uncovering the secrets of the sky and the constellations of the Cosmic Caravan, able to read the stars as well as she can read minds.");
            });
            Helpers.SetField(starseeker, "m_ParentClass", psychic_class);
            library.AddAsset(starseeker, "");

            starseeker.RemoveFeatures = new LevelEntry[] {
                                                            Helpers.LevelEntry(1, psychic_discipline, phrenic_amplification),
                                                            Helpers.LevelEntry(11, phrenic_amplification, major_amplification)
                                                         };
            starseeker.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, psychic_discipline_no_spells, written_in_stars),
                                                        Helpers.LevelEntry(15, major_amplification)
                                                      };
            psychic_class.Progression.UIDeterminatorsGroup = psychic_class.Progression.UIDeterminatorsGroup.AddToArray(written_in_stars);
        }


        static void createEsotericStarseekerWrittenInTheStars()
        {
            written_in_stars_resource = Helpers.CreateAbilityResource("WrittenInStarsResource", "", "", "", null);
            written_in_stars_resource.SetIncreasedByLevelStartPlusDivStep(1, 11, 1, 100, 0, 0, 0.0f, getPsychicArray());

            written_in_stars_ability = Helpers.CreateAbility("WrittenInStarsBaseAbility",
                                                             "Written in the Stars",
                                                             "An esoteric starseeker can attune herself to a constellation of the Cosmic Caravan, gaining knowledge of new spells from it. She gains one bonus constellation spell slot for each spell level she can cast, and she can prepare a spell associated with her attuned constellation into that slot. At 11th level, she can attune herself to two constellations, choosing between the spells offered by both constellations when she prepares her constellation spells. "
                                                             + "She can change her attuned constellations in the beggining of the day. The Cosmic Caravan and their associated spells are as follows:\n",
                                                             "",
                                                             LoadIcons.Image2Sprite.Create(@"AbilityIcons/Starburn.png"),
                                                             AbilityType.Supernatural,
                                                             CommandType.Standard,
                                                             AbilityRange.Personal,
                                                             "",
                                                             ""
                                                             );
            written_in_stars_ability.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAbilityVariants(written_in_stars_ability) };
            written_in_stars_ability.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(written_in_stars_ability);


            var lantern_bearer_spell_list = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("4d9bf81b7939b304185d58a09960f589"), //faerie fire
                library.Get<BlueprintAbility>("ce7dad2b25acf85429b6c9550787b2d9"), //glitterdust
                library.Get<BlueprintAbility>("bf0accce250381a44b857d4af6c8e10d"), //searing light
                library.Get<BlueprintAbility>("4b8265132f9c8174f87ce7fa6d0fe47b"), //rainbow pattern
                library.Get<BlueprintAbility>("4cf3d0fae3239ec478f51e86f49161cb"), //true seeing
                library.Get<BlueprintAbility>("f8cea58227f59c64399044a82c9735c4"), //chains of light
                library.Get<BlueprintAbility>("b22fd434bdb60fb4ba1068206402c4cf"), //prismatic spray
                library.Get<BlueprintAbility>("e96424f70ff884947b06f41a765b7658"), //sunburst
                NewSpells.meteor_swarm
            };

            var newlyweds_spell_list = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("88367310478c10b47903463c5d0152b0"), //hypnotism
                library.Get<BlueprintAbility>("c7104f7526c4c524f91474614054547e"), //hold person
                NewSpells.synesthesia,
                library.Get<BlueprintAbility>("7792da00c85b9e042a0fdfc2b66ec9a8"), //break enchantment
                library.Get<BlueprintAbility>("a0fc99f0933d01643b2b8fe570caa4c5"), //raise dead
                library.Get<BlueprintAbility>("15a04c40f84545949abeedef7279751a"), //joyful rapture
                NewSpells.synesthesia_mass,
                library.Get<BlueprintAbility>("f958ef62eea5050418fb92dfa944c631"), //power word stun
                library.Get<BlueprintAbility>("41cf93453b027b94886901dbfc680cb9"), //overwhelming presence
            };


            var daughter_spell_list = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638"), //bless
                NewSpells.hypnotic_pattern,
                library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63"), //heroism
                library.Get<BlueprintAbility>("0087fc2d64b6095478bc7b8d7d512caf"), //freedom of movement
                library.Get<BlueprintAbility>("d38aaf487e29c3d43a3bffa4a4a55f8f"), //song of discord
                library.Get<BlueprintAbility>("7f71a70d822af94458dc1a235507e972"), //cloak of dreams
                library.Get<BlueprintAbility>("1e2d1489781b10a45a3b70192bba9be3"), //waves of ecstasy
                NewSpells.irresistible_dance,
                library.Get<BlueprintAbility>("43740dab07286fe4aa00a6ee104ce7c1"), //heroic invocation
            };


            var rider_spell_list = new BlueprintAbility[]
            {
                NewSpells.burst_of_adrenaline,
                library.Get<BlueprintAbility>("4709274b2080b6444a3c11c6ebbe2404"), //find traps
                library.Get<BlueprintAbility>("2d4263d80f5136b4296d6eb43a221d7d"), //magic vestement
                library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162"), //shout
                library.Get<BlueprintAbility>("20b548bf09bb3ea4bafea78dcb4f3db6"), //echolocation
                library.Get<BlueprintAbility>("36c8971e91f1745418cc3ffdfac17b74"), //blade  barrier
                library.Get<BlueprintAbility>("da1b292d91ba37948893cdbe9ea89e28"), //legendary proprtions
                library.Get<BlueprintAbility>("e788b02f8d21014488067bdd3ba7b325"), //frightful aspect
                library.Get<BlueprintAbility>("1f01a098d737ec6419aedc4e7ad61fdd"), //foresight
            };


            var patriarch_spell_list = new BlueprintAbility[]
            {
                NewSpells.command,
                NewSpells.force_sword,
                library.Get<BlueprintAbility>("92681f181b507b34ea87018e8f7a528a"), //dispel magic
                library.Get<BlueprintAbility>("76a629d019275b94184a1a8733cac45e"), //protection from energy comminal
                NewSpells.burst_of_force,
                library.Get<BlueprintAbility>("f0f761b808dc4b149b08eaf44b99f633"), //dispel magic greater
                NewSpells.fly_mass,
                library.Get<BlueprintAbility>("a5c56f0f699daec44b7aedd8b273b08a"), //brilliant inspiration
                library.Get<BlueprintAbility>("52b5df2a97df18242aec67610616ded0"), //foresight
            };


            var wagon_spell_list = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("4f8181e7a7f1d904fbaea64220e83379"), //expeditious retreat
                library.Get<BlueprintAbility>("464a7193519429f48b4d190acb753cf0"), //grace
                library.Get<BlueprintAbility>("486eaff58293f6441a5c2759c4872f98"), //haste
                library.Get<BlueprintAbility>("4a648b57935a59547b7a2ee86fb4f26a"), //dimension door
                library.Get<BlueprintAbility>("41e8a952da7a5c247b3ec1c2dbb73018"), //hold monster
                NewSpells.fluid_form,
                NewSpells.particulate_form,
                NewSpells.temporal_stasis,
                NewSpells.time_stop
            };


            var pack_spell_list = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("7bdb6a9fb6b37614e96f155748ae50c6"), //aspect of the falcon
                library.Get<BlueprintAbility>("a4ad1b8fa11e7c347a608c004efce9d5"), //aspect of the bear
                library.Get<BlueprintAbility>("97b991256e43bb140b263c326f690ce2"), //rage
                library.Get<BlueprintAbility>("7ed74a3ec8c458d4fb50b192fd7be6ef"), //summon monster IV
                library.Get<BlueprintAbility>("630c8b85d9f07a64f917d79cb5905741"), //summon monster V
                library.Get<BlueprintAbility>("f6bcea6db14f0814d99b54856e918b92"), //mass bear endurance
                library.Get<BlueprintAbility>("ab167fd8203c1314bac6568932f1752f"), //summon monster VII
                library.Get<BlueprintAbility>("d3ac756a229830243a72e84f3ab050d0"), //summon monster VIII
                Wildshape.shapechange
            };


            var mother_spell_list = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                library.Get<BlueprintAbility>("03a9630394d10164a9410882d31572f0"), //aid
                NewSpells.invisibility_purge,
                library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b"), //stone skin
                library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889"), //spell resistance
                NewSpells.contingency,
                library.Get<BlueprintAbility>("df2a0ba6b6dcecf429cbb80a56fee5cf"), //mind blank
                library.Get<BlueprintAbility>("7ef49f184922063499b8f1346fb7f521"), //sea mantle
                NewSpells.akashic_form
            };


            var stranger_spell_list = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("f001c73999fb5a543a199f890108d936"), //vanish
                library.Get<BlueprintAbility>("89940cde01689fb46946b2f8cd7b66b7"), //invisibility
                library.Get<BlueprintAbility>("903092f6488f9ce45a80943923576ab3"), //displacement
                library.Get<BlueprintAbility>("ecaa0def35b38f949bd1976a6c9539e0"), //improved invisibility
                library.Get<BlueprintAbility>("12fb4a4c22549c74d949e2916a2f0b6a"), //phantasmal web
                library.Get<BlueprintAbility>("1f2e6019ece86d64baa5effa15e81ecc"), //phantasmal putrefaction
                library.Get<BlueprintAbility>("98310a099009bbd4dbdf66bcef58b4cd"), //invisibility mass
                library.Get<BlueprintAbility>("0e67fa8f011662c43934d486acc50253"), //prediciton of failure
                NewSpells.divide_mind
            };

            var follower_spell_list = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("fbdd8c455ac4cde4a9a3e18c84af9485"), //doom
                NewSpells.howling_agony,
                NewSpells.ray_of_exhaustion,
                library.Get<BlueprintAbility>("f34fb78eaaec141469079af124bcfa0f"), //enervation
                library.Get<BlueprintAbility>("8878d0c46dfbd564e9d5756349d5e439"), //faves of fatigue
                library.Get<BlueprintAbility>("a89dcbbab8f40e44e920cc60636097cf"), //circle of death
                library.Get<BlueprintAbility>("d361391f645db984bbf58907711a146a"), //banishment
                library.Get<BlueprintAbility>("3b646e1db3403b940bf620e01d2ce0c7"), //destruction
                library.Get<BlueprintAbility>("37302f72b06ced1408bf5bb965766d46"), //energy drain
            };

            addConstellationToWrittenInStarsAbility(lantern_bearer_spell_list, "LanternBearer", "The Lantern Bearer");
            addConstellationToWrittenInStarsAbility(newlyweds_spell_list, "Newlyweds", "The Newlyweds");
            addConstellationToWrittenInStarsAbility(daughter_spell_list, "Daughter", "The Daughter");
            addConstellationToWrittenInStarsAbility(rider_spell_list, "Rider", "The Rider");
            addConstellationToWrittenInStarsAbility(patriarch_spell_list, "Patriarch", "The Patriarch");
            addConstellationToWrittenInStarsAbility(wagon_spell_list, "Wagon", "The Wagon");
            addConstellationToWrittenInStarsAbility(pack_spell_list, "Pack", "The Pack");
            addConstellationToWrittenInStarsAbility(mother_spell_list, "Mother", "The Mother");
            addConstellationToWrittenInStarsAbility(stranger_spell_list, "Stranger", "The Stranger");
            addConstellationToWrittenInStarsAbility(follower_spell_list, "Follower", "The Follower");

            written_in_stars = Common.AbilityToFeature(written_in_stars_ability, false);
            written_in_stars.AddComponent(written_in_stars_resource.CreateAddAbilityResource());
            written_in_stars.SetDescription(written_in_stars.Description + "Esoteric Starseeker does not gain discipline spells.");
        }



        static void addConstellationToWrittenInStarsAbility(BlueprintAbility[] spells, string name, string display_name)
        {
            var spells_description = "";

            for (int i = 0; i < spells.Length; i++)
            {
                var s = spells[i];
                spells_description += s.Name + " (" + (i+1).ToString() + Common.getNumExtension(i+1) +")";
                if (s == spells.Last())
                {
                    spells_description += ".";
                }
                else
                {
                    spells_description += ", ";
                }
            }

            var buff = Helpers.CreateBuff("WrittenInStars" + name + "Buff",
                                          "Written in the Stars: " + display_name,
                                          "Constellation spells: " + spells_description,
                                          "",
                                          written_in_stars_ability.Icon,
                                          null,
                                          Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => c.Resource = written_in_stars_resource))
                                         );
            buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath);
            for (int i = 0; i < spells.Length; i++)
            {
                buff.AddComponent(Helpers.Create<SpellbookMechanics.TemporaryAddKnownSpell>(t => { t.spell = spells[i]; t.spell_level = i + 1; t.character_class = psychic_class; }));
            }

            var ability = Helpers.CreateAbility("WrittenInStars" + name + "Ability",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                "Unitl rest",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true)),
                                                written_in_stars_resource.CreateResourceLogic(),
                                                Common.createAbilityCasterHasNoFacts(buff)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(ability);

            written_in_stars_ability.addToAbilityVariants(ability);
            written_in_stars_ability.SetDescription(written_in_stars_ability.Description + display_name + ": " + spells_description + "\n");
        }


        static void createPsychicFeats()
        {
            extra_phrenic_pool = Helpers.CreateFeature("PsychicExtraPhrenicPoolFeature",
                                                       "Expanded Phrenic Pool",
                                                       "Your phrenic pool total increases by 2 points.",
                                                       "",
                                                       Helpers.GetIcon("42f96fc8d6c80784194262e51b0a1d25"), //extra arcana
                                                       FeatureGroup.Feat,
                                                       Helpers.Create<IncreaseResourceAmount>(i => { i.Resource = phrenic_pool_resource; i.Value = 2; }),
                                                       Helpers.PrerequisiteFeature(phrenic_pool)
                                                       );
            extra_phrenic_pool.Ranks = 10;
            extra_phrenic_amplification = Helpers.CreateFeatureSelection("PsychicExtraPhrenicAmplificationFeatureSelection",
                                                                           "Extra Phrenic Amplification (Psychic)",
                                                                           "You gain one additional phrenic amplification.",
                                                                           "",
                                                                           null,
                                                                           FeatureGroup.Feat,
                                                                           Helpers.PrerequisiteFeature(phrenic_amplification)
                                                                           );
            extra_phrenic_amplification.AllFeatures = phrenic_amplification.AllFeatures;

            library.AddFeats(extra_phrenic_pool, extra_phrenic_amplification);
        }


        static BlueprintCharacterClass[] getPsychicArray()
        {
            return new BlueprintCharacterClass[] { psychic_class };

        }


        static void createPsychicProgression()
        {
            createPsychicProficiencies();
            createPhrenicAmplification();
            createPsychicDisiciplines();

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            psychic_progression = Helpers.CreateProgression("PsychicProgression",
                                                            psychic_class.Name,
                                                            psychic_class.Description,
                                                            "",
                                                            psychic_class.Icon,
                                                            FeatureGroup.None);
            psychic_progression.Classes = getPsychicArray();

            var deity = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            psychic_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, psychic_proficiencies, detect_magic,
                                                                                        psychic_discipline,
                                                                                        phrenic_amplification,
                                                                                        psychic_spellcasting,
                                                                                        phrenic_pool,
                                                                                        library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee"), //inside the storm
                                                                                        library.Get<BlueprintFeature>("9fc9813f569e2e5448ddc435abf774b3"), //full caster feature
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2),
                                                                    Helpers.LevelEntry(3, phrenic_amplification),
                                                                    Helpers.LevelEntry(4),
                                                                    Helpers.LevelEntry(5),
                                                                    Helpers.LevelEntry(6),
                                                                    Helpers.LevelEntry(7, phrenic_amplification),
                                                                    Helpers.LevelEntry(8),
                                                                    Helpers.LevelEntry(9),
                                                                    Helpers.LevelEntry(10),
                                                                    Helpers.LevelEntry(11, major_amplification, phrenic_amplification),
                                                                    Helpers.LevelEntry(12),
                                                                    Helpers.LevelEntry(13),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, phrenic_amplification),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18),
                                                                    Helpers.LevelEntry(19, phrenic_amplification),
                                                                    Helpers.LevelEntry(20, phrenic_mastery)
                                                                    };

            psychic_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { psychic_spellcasting, psychic_proficiencies, psychic_discipline, psychic_discipline_no_spells };
            psychic_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(phrenic_pool, major_amplification, phrenic_mastery),
                                                          };
        }


        static void createPsychicDisiciplines()
        {
            psychic_discipline = Helpers.CreateFeatureSelection("PsychicDisciplineFeatureSelection",
                                                                "Psychic Discipline",
                                                                "Each psychic accesses and improves her mental powers through a particular method, such as rigorous study or attaining a particular mental state.\n"
                                                                + "This is called her psychic discipline. She gains additional spells known based on her selected discipline. The choice of discipline must be made at 1st level; once made, it can’t be changed. Each psychic discipline gives the psychic a number of discipline powers (at 1st, 5th, and 13th levels), and grants her additional spells known. In addition, the discipline determines which ability score the psychic uses for her phrenic pool and phrenic amplifications abilities. The DC of a saving throw against a psychic discipline ability equals 10 + 1/2 the psychic’s level + the psychic’s Intelligence modifier.\n"
                                                                + "At 1st level, a psychic learns an additional spell determined by her discipline. She learns another additional spell at 4th level and every 2 levels thereafter, until learning the final one at 18th level. These spells are in addition to the number of spells given. Spells learned from a discipline can’t be exchanged for different spells at higher levels.",
                                                                "",
                                                                null,
                                                                FeatureGroup.None);
            psychic_discipline_no_spells = library.CopyAndAdd(psychic_discipline, "NoSpells" + psychic_discipline.name, "");
            createAbominationDiscipline();
            createFaithDisicipline();
            createPsychedeliaDisicipline();
            createPainDiscipline();
            createPageantryDiscipline();
            createFerocityDiscipline();
        }


        static void createFerocityDiscipline()
        {
            var enhanced_senses_buff1 = Helpers.CreateBuff("EnhancedSenses1Buff",
                                                           "Enhanced Senses: Blindsense",
                                                           "Your mind can process sensory stimuli with exceptional efficiency, awakening you to a world most humanoids cannot comprehend. You can spend 1 point from your phrenic pool as a standard action to amplify your sense of smell, gaining blindsense with a range of 30 feet for 1 minute. At 11th level, your blindsense is continuously active, and you can spend 1 point from your phrenic pool as a standard action to gain blindsight with a range of 30 feet for 1 minute.",
                                                           "",
                                                           NewSpells.countless_eyes.Icon,
                                                           null,
                                                           Common.createBlindsense(30)
                                                           );

            var enhanced_senses1_ability = Helpers.CreateAbility("EnhancedSenses1Ability",
                                                        enhanced_senses_buff1.Name,
                                                        enhanced_senses_buff1.Description,
                                                        "",
                                                        enhanced_senses_buff1.Icon,
                                                        AbilityType.Extraordinary,
                                                        CommandType.Standard,
                                                        AbilityRange.Personal,
                                                        Helpers.oneMinuteDuration,
                                                        "",
                                                        Helpers.CreateRunActions(Common.createContextActionApplyBuff(enhanced_senses_buff1, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)),
                                                        Common.createAbilitySpawnFx("8de64fbe047abc243a9b4715f643739f", position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None),
                                                        phrenic_pool_resource.CreateResourceLogic()
                                                        );
            enhanced_senses1_ability.setMiscAbilityParametersSelfOnly();
            var enhanced_senses1_feature = Common.AbilityToFeature(enhanced_senses1_ability);

            var enhanced_senses_buff2 = Helpers.CreateBuff("EnhancedSenses2Buff",
                                               "Enhanced Senses: Blindsight",
                                               enhanced_senses_buff1.Description,
                                               "",
                                               NewSpells.countless_eyes.Icon,
                                               null,
                                               Common.createBlindsight(30)
                                               );
            
            var enhanced_senses2_ability = Helpers.CreateAbility("EnhancedSenses2Ability",
                                            enhanced_senses_buff2.Name,
                                            enhanced_senses_buff2.Description,
                                            "",
                                            enhanced_senses_buff2.Icon,
                                            AbilityType.Extraordinary,
                                            CommandType.Standard,
                                            AbilityRange.Personal,
                                            Helpers.oneMinuteDuration,
                                            "",
                                            Helpers.CreateRunActions(Common.createContextActionApplyBuff(enhanced_senses_buff2, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)),
                                            Common.createAbilitySpawnFx("8de64fbe047abc243a9b4715f643739f", position_anchor: AbilitySpawnFxAnchor.None, orientation_anchor: AbilitySpawnFxAnchor.None),
                                            phrenic_pool_resource.CreateResourceLogic()
                                            );
            enhanced_senses2_ability.setMiscAbilityParametersSelfOnly();
            var enhanced_senses2_feature = Common.AbilityToFeature(enhanced_senses2_ability);
            enhanced_senses2_feature.AddComponent(Common.createBlindsense(30));

            var enhanced_senses = Helpers.CreateFeature("EnhancedSensesFeature",
                                                        "Enhanced Senses",
                                                        enhanced_senses1_feature.Description,
                                                        "",
                                                        enhanced_senses1_feature.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddFeatureOnClassLevel(enhanced_senses1_feature, 11, getPsychicArray(), before: true),
                                                        Helpers.CreateAddFeatureOnClassLevel(enhanced_senses2_feature, 11, getPsychicArray())
                                                        );

            var ferocity_buff = Helpers.CreateBuff("FerocityAbilityBuff",
                                                   "Ferocity",
                                                   "At 5th level, you cling to life with increasing ferocity. You gain Diehard feat. Whenever you have less than 25% hit points, you gain a +4 morale bonus to your Strength and Dexterity, as well as a +2 morale bonus on Fortitude saves.",
                                                   "",
                                                   Helpers.GetIcon("97b991256e43bb140b263c326f690ce2"), //rage
                                                   null,
                                                   Helpers.CreateAddStatBonus(StatType.Strength, 4, ModifierDescriptor.Morale),
                                                   Helpers.CreateAddStatBonus(StatType.Dexterity, 4, ModifierDescriptor.Morale),
                                                   Helpers.CreateAddStatBonus(StatType.SaveFortitude, 2, ModifierDescriptor.Morale)
                                                   );

            var dmg_trigger = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => c.Value = 25),
                                                        Common.createContextActionApplyBuff(ferocity_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)
                                                        );
            var healing_trigger = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionCompareTargetHPPercent>(c => { c.Value = 25; c.Not = true; }),
                                                            Common.createContextActionRemoveBuff(ferocity_buff)
                                                           );

            var ferocity_feature = Helpers.CreateFeature("FerocityAbilityFeature",
                                                         ferocity_buff.Name,
                                                         ferocity_buff.Description,
                                                         "",
                                                         ferocity_buff.Icon,
                                                         FeatureGroup.None,
                                                         Common.createAddFeatureIfHasFact(library.Get<BlueprintFeature>("c99f3405d1ef79049bd90678a666e1d7"),
                                                                                          library.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad"),
                                                                                          not: true
                                                                                          ),
                                                         Common.createIncomingDamageTrigger(dmg_trigger),
                                                         Common.createHealingTrigger(healing_trigger)
                                                        );

            var primal_fury_resource = Helpers.CreateAbilityResource("PrimalFuryFerocityResource", "", "", "", null);
            primal_fury_resource.SetIncreasedByLevel(0, 1, getPsychicArray());
            var primal_fury_buff = library.CopyAndAdd<BlueprintBuff>("287682389d2011b41b5a65195d9cbc84", "PrimalFuryFerocityBuff", "");

            primal_fury_buff.SetNameDescription("Primal Fury",
                                                "At 13th level, you can unleash a more primal version of yourself as a free action. This functions as transformation, with the following exceptions. You can use this ability for a number of rounds per day equal to your psychic level. These rounds do not need to be consecutive. You can end the effect as a free action. After using this ability, you are fatigued for one minute. You cannot enter a new primal fury while fatigued or exhausted. If you fall unconscious, your primal fury immediately ends."
                                                );

            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var apply_fatigued = Common.createContextActionApplyBuff(fatigued, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
            primal_fury_buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: apply_fatigued));
            var primal_fury_toggle = Common.buffToToggle(primal_fury_buff, CommandType.Free, true,
                                                         Helpers.Create<RestrictionHasUnitCondition>(r => { r.Condition = UnitCondition.Fatigued; r.Invert = true; }),
                                                         Helpers.Create<RestrictionHasUnitCondition>(r => { r.Condition = UnitCondition.Exhausted; r.Invert = true; }),
                                                         Helpers.Create<RestrictionHasFact>(r => { r.Feature = BlueprintRoot.Instance.SystemMechanics.FatigueBuff; r.Not = true; }),
                                                         Helpers.Create<RestrictionHasFact>(r => { r.Feature = BlueprintRoot.Instance.SystemMechanics.ExhaustedBuff; r.Not = true; }),
                                                         primal_fury_resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound)
                                                         );
            primal_fury_toggle.DeactivateIfOwnerDisabled = true;
            primal_fury_toggle.DeactivateIfOwnerUnconscious = true;
            var primal_fury_feature = Common.ActivatableAbilityToFeature(primal_fury_toggle, false);
            primal_fury_feature.AddComponent(primal_fury_resource.CreateAddAbilityResource());

            createPsychicDiscipline("Ferocity",
                        "Ferocity",
                        "You are in touch with the most primal part of your subconscious. Your training allows you to unlock animalistic senses and powers.\n"
                        + "Discipline Powers: Through your psychic training, you can unleash the most animalistic portions of your subconscious, enhancing your senses and physical capabilities.",
                        ferocity_feature.Icon,
                        StatType.Wisdom,
                        new BlueprintAbility[]
                        {
                                                    library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00"), //true strike
                                                    library.Get<BlueprintAbility>("4c3d08935262b6544ae97599b3a9556d"), //bull's strength
                                                    library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63"), //heroism
                                                    library.Get<BlueprintAbility>("0087fc2d64b6095478bc7b8d7d512caf"), //freedom of movement
                                                    library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b"), //stoneskin
                                                    library.Get<BlueprintAbility>("27203d62eb3d4184c9aced94f22e1806"), //transformation
                                                    library.Get<BlueprintAbility>("df2a0ba6b6dcecf429cbb80a56fee5cf"), //mind blank
                                                    library.Get<BlueprintAbility>("e788b02f8d21014488067bdd3ba7b325"), //frightful aspect
                                                    library.Get<BlueprintAbility>("1f01a098d737ec6419aedc4e7ad61fdd"), //foresight
                        },
                        enhanced_senses,
                        ferocity_feature,
                        primal_fury_feature
                        );
        } 


        static void createPageantryDiscipline()
        {
            //ritual unity
            var skill_foci = library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a").AllFeatures;
            var abilities = new BlueprintAbility[skill_foci.Length];
            var ritual_unity_resource = Helpers.CreateAbilityResource("RitualUnityResource", "", "", "", null);
            ritual_unity_resource.SetIncreasedByStat(0, StatType.Charisma);
            for (int i = 0; i < skill_foci.Length; i++)
            {

                StatType stat = skill_foci[i].GetComponent<AddContextStatBonus>().Stat;
                string name = LocalizedTexts.Instance.Stats.GetText(stat);

                var buff = Helpers.CreateBuff(stat.ToString() + "RitualUnityBuff",
                                              "Ritual Unity: " + name,
                                              "You can use aid another action to assist an ally with a skill check. If you succeed at a DC 20 skill check, you impart a +4 bonus to your ally on his next skill check. When you successfully aid an ally in this way, you regain 1 point in your phrenic pool.\n"
                                              + "You can use this ability a number of times per day equal to your Charisma modifier.",
                                              "",
                                              skill_foci[i].Icon,
                                              null,
                                              Helpers.CreateAddStatBonus(stat, 4, ModifierDescriptor.UntypedStackable)
                                              );
                buff.AddComponents(Helpers.Create<AddInitiatorSkillRollTrigger>(a => { a.Skill = stat; a.Action = Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff)); }),
                                   Helpers.Create<AddInitiatorPartySkillRollTrigger>(a => { a.Skill = stat; a.Action = Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff)); })
                                   );
                if (stat == StatType.SkillPersuasion)
                {
                    foreach (var s in new StatType[] { StatType.CheckIntimidate, StatType.CheckDiplomacy, StatType.CheckBluff })
                    {
                        buff.AddComponents(Helpers.Create<AddInitiatorSkillRollTrigger>(a => { a.Skill = s; a.Action = Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff)); }),
                                           Helpers.Create<AddInitiatorPartySkillRollTrigger>(a => { a.Skill = s; a.Action = Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff)); })
                                           );
                    }
                }
                buff.SetBuffFlags(BuffFlags.RemoveOnRest);
                var check = Helpers.Create<SkillMechanics.ContextActionCasterSkillCheck>(c =>
                {
                    c.CustomDC = 20;
                    c.UseCustomDC = true;
                    c.Success = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                         Common.createContextActionOnContextCaster(Helpers.Create<ResourceMechanics.ContextRestoreResource>(r => r.Resource = phrenic_pool_resource)));
                    c.Stat = stat;
                });
                abilities[i] = Helpers.CreateAbility(stat.ToString() + "RitualUnityAbility",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Supernatural,
                                                   CommandType.Standard,
                                                   AbilityRange.Touch,
                                                   Helpers.oneRoundDuration,
                                                   "",
                                                   Helpers.CreateRunActions(check),
                                                   ritual_unity_resource.CreateResourceLogic()
                                                   );
                abilities[i].setMiscAbilityParametersTouchFriendly(works_on_self: false);
            }

            var ritual_unity_ability = Common.createVariantWrapper("RitualUnityBaseAbility", "", abilities);
            ritual_unity_ability.SetName("Ritual Unity");
            ritual_unity_ability.SetIcon(Helpers.GetIcon("c9629ef9eebb88b479b2fbc5e836656a"));//skill focus
            var ritual_unity = Common.AbilityToFeature(ritual_unity_ability, false);
            ritual_unity.AddComponent(ritual_unity_resource.CreateAddAbilityResource());

            //power from pageantry
            var power_from_pageantry_buff = Helpers.CreateBuff("PowerFromPageantryBuff",
                                                               "Power from Pageantry",
                                                               $"Starting from 5th level, you can spend 1 point from your phrenic pool as a move action to increase caster level and saving throw DC by {(Main.settings.balance_fixes ? 1 : 2)} for next spell you cast before the end of your turn.",
                                                               "",
                                                               Helpers.GetIcon("a5e23522eda32dc45801e32c05dc9f96"),//good hope
                                                               null,
                                                               Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s =>
                                                               {
                                                                   s.amount = 0;
                                                                   s.resource = phrenic_pool_resource;
                                                                   s.specific_class = psychic_class;
                                                                   s.remove_self = true;
                                                               }),
                                                               Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellsDC>(c =>
                                                               {
                                                                   c.specific_class = psychic_class;
                                                                   c.Value = Main.settings.balance_fixes ? 1 : 2;
                                                               }),
                                                               Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellLevel>(c =>
                                                               {
                                                                   c.specific_class = psychic_class;
                                                                   c.Value = Main.settings.balance_fixes ? 1 : 2;
                                                               })
                                                               );
            var power_from_pageantry_ability = Helpers.CreateAbility("PowerFromPageantry",
                                                                     power_from_pageantry_buff.Name,
                                                                     power_from_pageantry_buff.Description,
                                                                     "",
                                                                     power_from_pageantry_buff.Icon,
                                                                     AbilityType.Supernatural,
                                                                     CommandType.Move,
                                                                     AbilityRange.Personal,
                                                                     Helpers.oneRoundDuration,
                                                                     "",
                                                                     Helpers.CreateRunActions(Common.createContextActionApplyBuff(power_from_pageantry_buff, Helpers.CreateContextDuration(), dispellable: false, duration_seconds: 5)),
                                                                     phrenic_pool_resource.CreateResourceLogic()
                                                                     );
            power_from_pageantry_ability.setMiscAbilityParametersSelfOnly();
            var power_from_pageantry = Common.AbilityToFeature(power_from_pageantry_ability, false);

            var unrivalaed_focus_buff = Helpers.CreateBuff("UnrivaledFocusBuff",
                                                           "Unrivaled Focus",
                                                           "At 13th level, your powers of concentration become unmatched. Whenever you attempt a concentration check, you can spend 1 point from your phrenic pool to treat the result of your die roll as a 20. You must choose to use this ability before rolling the check.",
                                                           "",
                                                           Helpers.GetIcon("06964d468fde1dc4aa71a92ea04d930d"),//combat casting
                                                           null,
                                                           Helpers.Create<NewMechanics.ModifyD20WithActions>(m =>
                                                           {
                                                               m.Rule = NewMechanics.ModifyD20WithActions.RuleType.Concentration;
                                                               m.Roll = 20;
                                                               m.Replace = true;
                                                               m.actions = Helpers.CreateActionList(Helpers.Create<NewMechanics.ContextActionSpendResource>(c => c.resource = phrenic_pool_resource));
                                                           })
                                                           );

            var unrivaled_focus_toggle = Common.buffToToggle(unrivalaed_focus_buff, CommandType.Free, true,
                                                            phrenic_pool_resource.CreateActivatableResourceLogic(ResourceSpendType.Never)
                                                            );

            var unrivaled_focus = Common.ActivatableAbilityToFeature(unrivaled_focus_toggle, false);


            createPsychicDiscipline("Pageantry",
                                    "Pageantry",
                                    "The act of ritual, no matter how ostentatious, can be a route to mental strength. By engaging in esoteric and intricate routines, you have unlocked potent psychic power.",
                                    power_from_pageantry.Icon,
                                    StatType.Charisma,
                                    new BlueprintAbility[]
                                    {
                                                    library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638"), //bless
                                                    NewSpells.silence,
                                                    library.Get<BlueprintAbility>("f492622e473d34747806bdb39356eb89"), //slow
                                                    library.Get<BlueprintAbility>("4b8265132f9c8174f87ce7fa6d0fe47b"), //rainbow_pattern
                                                    NewSpells.command_greater,
                                                    library.Get<BlueprintAbility>("e15e5e7045fda2244b98c8f010adfe31"), //heroism greater
                                                    NewSpells.hold_person_mass,
                                                    library.Get<BlueprintAbility>("e788b02f8d21014488067bdd3ba7b325"), //frightful aspect
                                                    library.Get<BlueprintAbility>("43740dab07286fe4aa00a6ee104ce7c1"), //heroic invocation
                                    },
                                    ritual_unity,
                                    power_from_pageantry,
                                    unrivaled_focus
                                    );
        }


        static void createPainDiscipline()
        {
            var painful_reminder_resource = Helpers.CreateAbilityResource("PainfulReminderResource", "", "", "", null);
            painful_reminder_resource.SetIncreasedByStat(3, StatType.Charisma);
            var painful_reminder_buff = Helpers.CreateBuff("PainfulReminderBuff",
                                                           "Painful Reminder Allowed",
                                                           "As a swift action, you can cause an enemy to take 1d6 points of nonlethal damage if you dealt damage to that enemy with a spell since the start of your previous turn. You can use this ability a number of times per day equal to 3 + your Charisma modifier. This damage increases to 2d6 at 8th level and to 3d6 at 15th level.\n"
                                                           + "If your painful reminder deals at least 5 points of damage, you regain 1 point in your phrenic pool.",
                                                           "",
                                                           Helpers.GetIcon("55f14bc84d7c85446b07a1b5dd6b2b4c"), //daze
                                                           null);
            painful_reminder_buff.Stacking = StackingType.Stack;

            var painful_reminder_ability = Helpers.CreateAbility("PainfulReminderAbility",
                                                                 "Painful Reminder",
                                                                 painful_reminder_buff.Description,
                                                                 "",
                                                                 painful_reminder_buff.Icon,
                                                                 AbilityType.Supernatural,
                                                                 CommandType.Swift,
                                                                 AbilityRange.Unlimited,
                                                                 "",
                                                                 "",
                                                                 Common.createAbilityTargetHasFact(false, painful_reminder_buff),
                                                                 Helpers.CreateRunActions(Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValueFromSharedValue(AbilitySharedValue.Damage)),
                                                                                          Helpers.CreateConditional(Helpers.Create<ContextConditionSharedValueHigher>(c =>
                                                                                                                      {
                                                                                                                          c.SharedValue = AbilitySharedValue.Damage;
                                                                                                                          c.HigherOrEqual = 5;
                                                                                                                      }),
                                                                                                                      Common.createContextActionOnContextCaster(Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => c.Resource = phrenic_pool_resource))
                                                                                                                     )
                                                                                          ),
                                                                 Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default), 0)),
                                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getPsychicArray(), progression: ContextRankProgression.Custom,
                                                                                                 customProgression: new (int, int)[] { (7, 1), (14, 2), (20, 3) }),
                                                                 painful_reminder_resource.CreateResourceLogic()
                                                                 );
            painful_reminder_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            var painful_reminder_action = Helpers.CreateActionList(Common.createContextActionRemoveBuffFromCaster(painful_reminder_buff),
                                                                   Common.createContextActionApplyBuff(painful_reminder_buff, Helpers.CreateContextDuration(2), dispellable: false)
                                                                   );

            var painful_reminder = Helpers.CreateFeature("PainfulReminderFeature",
                                                         painful_reminder_ability.Name,
                                                         painful_reminder_ability.Description,
                                                         "",
                                                         painful_reminder_ability.Icon,
                                                         FeatureGroup.None,
                                                         Helpers.CreateAddFact(painful_reminder_ability),
                                                         painful_reminder_resource.CreateAddAbilityResource(),
                                                         Helpers.Create<NewMechanics.ActionOnSpellDamage>(a =>
                                                                                                         {
                                                                                                             a.action = painful_reminder_action;
                                                                                                         }
                                                                                                         )
                                                         );

            var live_on_resource = Helpers.CreateAbilityResource("LiveOnResource", "", "", "", null);
            live_on_resource.SetIncreasedByStat(0, StatType.Charisma);
            live_on_resource.SetIncreasedByLevelStartPlusDivStep(0, 3, 0, 2, 1, 0, 0.0f, getPsychicArray());
            var live_on_ability = library.CopyAndAdd<BlueprintAbility>("8d6073201e5395d458b8251386d72df1", "LiveOnAbility", "");
            live_on_ability.RemoveComponents<AbilityCasterAlignment>();
            live_on_ability.RemoveComponents<AbilityResourceLogic>();
            live_on_ability.RemoveComponents<ContextRankConfig>();
            live_on_ability.AddComponents(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getPsychicArray(),
                                                                         progression: ContextRankProgression.StartPlusDivStep, startLevel: 5, stepLevel: 2),
                                          live_on_resource.CreateResourceLogic());

            live_on_ability.SetNameDescription("Live On",
                                               "At 5th level, you can use lay on hands as though you were a paladin of 3 levels lower than your psychic level.\n"
                                               + "You also gain access to mercies as though you were a paladin of 3 levels lower than your psychic level. You can target only yourself with lay on hands or mercies gained from this discipline.");

            var live_on_progression = Helpers.CreateProgression("LiveOnProgression",
                                                                live_on_ability.Name,
                                                                live_on_ability.Description,
                                                                "",
                                                                live_on_ability.Icon,
                                                                FeatureGroup.None,
                                                                live_on_resource.CreateAddAbilityResource(),
                                                                Helpers.CreateAddFact(live_on_ability),
                                                                Helpers.Create<ReplaceCasterLevelOfAbility>(r =>
                                                                                                            {
                                                                                                                r.Class = psychic_class;
                                                                                                                r.Spell = live_on_ability;
                                                                                                            }
                                                                                                            )
                                                                );

            live_on_progression.Classes = getPsychicArray();

            var mercy_selection = library.CopyAndAdd<BlueprintFeatureSelection>("02b187038a8dce545bb34bbfb346428d", "LiveOnMercyFeatureSelection", "");
            mercy_selection.SetDescription(live_on_ability.Description);

            foreach (var f in mercy_selection.AllFeatures)
            {
                var comp = f.GetComponent<PrerequisiteClassLevel>();
                if (comp == null)
                {
                    continue;
                }
                comp.Group = Prerequisite.GroupType.Any;
                f.AddComponent(Helpers.Create<PrerequisiteMechanics.CompoundPrerequisite>(c =>
                                                                                            {
                                                                                                c.prerequisite1 = Helpers.PrerequisiteClassLevel(psychic_class, comp.Level + 3);
                                                                                                c.prerequisite2 = Helpers.PrerequisiteFeature(live_on_progression);
                                                                                                c.Group = Prerequisite.GroupType.Any;
                                                                                            }
                                                                                          )
                              );
            }
            live_on_progression.LevelEntries = new LevelEntry[0];
            for (int i = 6; i <= 20; i+=3)
            {
                live_on_progression.LevelEntries = live_on_progression.LevelEntries.AddToArray(Helpers.LevelEntry(i, mercy_selection));
            }

            var agonizing_wound_resource = Helpers.CreateAbilityResource("AgonizingWoundResource", "", "", "", null);
            agonizing_wound_resource.SetIncreasedByStat(3, StatType.Charisma);
            var agonizing_wound = Helpers.CreateFeature("AgonizingWoundFeature",
                                                        "Agonizing Wound",
                                                        "At 13th level, whenever you cast a spell that deals damage to a creature, you can also make that creature frightened or sickened (your choice) for one round. If you expend two uses of this ability, you can instead have the creature become dazed, nauseated, or panicked for 1 round. The creature can attempt a Will saving throw to negate this effect. You can use this ability a number of times per day equal to 3 + your Charisma modifier. This is a mind-affecting pain effect.",
                                                        "",
                                                        Helpers.GetIcon("137af566f68fd9b428e2e12da43c1482"),//harm
                                                        FeatureGroup.None,
                                                        agonizing_wound_resource.CreateAddAbilityResource());

            var agonizing_wound_buff = Helpers.CreateBuff("AgonizingWoundSelectBuff",
                                                          agonizing_wound.Name + " Target",
                                                          agonizing_wound.Description,
                                                          "",
                                                          agonizing_wound.Icon,
                                                          null,
                                                          Helpers.Create<BuffMechanics.StoreBuff>(),
                                                          Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting)
                                                          );
            agonizing_wound_buff.Stacking = StackingType.Stack;

            var agonizng_wound_ability = Helpers.CreateAbility("AgonizingWoundSelectAbility",
                                                               agonizing_wound_buff.Name,
                                                               agonizing_wound_buff.Description,
                                                               "",
                                                               agonizing_wound.Icon,
                                                               AbilityType.Supernatural,
                                                               CommandType.Free,
                                                               AbilityRange.Unlimited,
                                                               Helpers.oneRoundDuration,
                                                               "",
                                                               Helpers.CreateRunActions(Common.createContextActionRemoveBuffFromCaster(agonizing_wound_buff),
                                                                                         Common.createContextActionApplyBuff(agonizing_wound_buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                               Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting)
                                                               );

            agonizng_wound_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            agonizing_wound.AddComponent(Helpers.CreateAddFact(agonizng_wound_ability));

            var buff_cost_pairs = new (BlueprintBuff, int)[]
            {
                (library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323"), 1),
                (library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf"), 1),
                (NewSpells.nauseted_non_poison, 2),
                (Common.dazed_non_mind_affecting, 2),
            };

            foreach (var bcp in buff_cost_pairs)
            {
                var action_on_dmg = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasBuffFromCaster(agonizing_wound_buff),
                                                                                                 Helpers.Create<ResourceMechanics.ContextConditionTargetHasEnoughResource>(c =>
                                                                                                                                                                         {
                                                                                                                                                                             c.resource = agonizing_wound_resource;
                                                                                                                                                                             c.amount = bcp.Item2;
                                                                                                                                                                             c.on_caster = true;
                                                                                                                                                                         })
                                                                                                 ),
                                                                                                 new GameAction[]{Common.createContextActionApplyBuff(bcp.Item1, Helpers.CreateContextDuration(1), dispellable: false),
                                                                                                                  Helpers.Create<ResourceMechanics.ContextActionSpendResourceFromCaster>(c => {c.resource = agonizing_wound_resource; c.amount = bcp.Item2; }),
                                                                                                                  Common.createContextActionRemoveBuffFromCaster(agonizing_wound_buff)
                                                                                                                 }
                                                            );
                var buff = Helpers.CreateBuff("AgonizingWound" + bcp.Item1.name + "Buff",
                                              "Agonizing Wound: " + bcp.Item1.Name,
                                              agonizing_wound.Description,
                                              "",
                                              bcp.Item1.Icon,
                                              null,
                                              Helpers.Create<NewMechanics.ActionOnSpellDamage>(a =>
                                                                                                  {
                                                                                                      a.save_type = SavingThrowType.Will;
                                                                                                      a.action = Helpers.CreateActionList(action_on_dmg);
                                                                                                  }
                                                                                               ),
                                              Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<BuffMechanics.RemoveStoredBuffs>(r => r.buff = agonizing_wound_buff))
                                              );

                var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                                 agonizing_wound_resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                                 Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = agonizing_wound_resource; r.amount = bcp.Item2; })
                                                );
                toggle.Group = ActivatableAbilityGroupExtension.AgonizingWound.ToActivatableAbilityGroup();
                agonizing_wound.AddComponent(Helpers.CreateAddFact(toggle));
            }


            createPsychicDiscipline("Pain",
                                    "Pain",
                                    "Mental blocks prevent your immense inborn psychic energies from flowing freely. They are unleashed only when you suffer pain.\n"
                                    + "Discipline Powers: Your powers allow you to cause and endure pain.",
                                    agonizing_wound.Icon,
                                    StatType.Charisma,
                                    new BlueprintAbility[]
                                    {
                                        library.Get<BlueprintAbility>("fa3078b9976a5b24caf92e20ee9c0f54"), //ray of sicikening
                                        NewSpells.pain_strike,
                                        library.Get<BlueprintAbility>("8a28a811ca5d20d49a863e832c31cce1"), //vampyric touch
                                        NewSpells.pain_strike_mass,
                                        NewSpells.synapse_overload,
                                        NewSpells.inflict_pain_mass,
                                        library.Get<BlueprintAbility>("3e4d3b9a5bd03734d9b053b9067c2f38"), //waves of exhaustion
                                        library.Get<BlueprintAbility>("08323922485f7e246acb3d2276515526"), //horrid wilting
                                        NewSpells.mass_suffocation
                                    },
                                    painful_reminder,
                                    live_on_progression,
                                    agonizing_wound
                                    );
        }


        static void createPsychedeliaDisicipline()
        {
            var cognatogen_alchemist = library.Get<BlueprintFeature>("e3f460ea61fcc504183c7d6818bbbf7a");
            var cognatogen = Helpers.CreateFeature("PsychedeliaCognatogen",
                                                   "Cognatogen",
                                                   "Once per day, you can create a cognatogen, a mutagen-like mixture that heightens one mental ability score at the expense of a physical ability score. When you imbibe a cognatogen, you gain a +2 natural armor bonus and a +4 alchemical bonus to the selected ability score for 1 minute per psychic level. In addition, while the cognatogen is in effect, you take a –2 penalty to one of your physical ability scores. If the cognatogen enhances your Intelligence, it applies a penalty to your Strength. If it enhances your Wisdom, it applies a penalty to your Dexterity.\n"
                                                   + "If it enhances your Charisma, it applies a penalty to your Constitution. Otherwise, this ability works just like the alchemist’s mutagen ability. When the effect of the cognatogen ends, you take 2 points of ability damage to the ability score penalized by the cognatogen. If you have both alchemist and psychic levels, these levels stack to determine the duration of your cognatogen and the DC of the save a non-alchemist must attempt if he drinks your cognatogen. If you gain discoveries, you can take the grand cognatogen and greater cognatogen discoveries to improve your cognatogen. The infuse mutagen discovery and the persistent mutagen class ability apply to cognatogens. However, even if you have alchemist levels, the duration of your cognatogen remains 1 minute per level (instead of 10 minutes per level).",
                                                   "",
                                                   cognatogen_alchemist.Icon,
                                                   FeatureGroup.None,
                                                   Helpers.CreateAddFact(cognatogen_alchemist),
                                                   Helpers.CreateAddAbilityResource(library.Get<BlueprintAbilityResource>("3b163587f010382408142fc8a97852b6"))
                                                   );
            
            var spell_level_comps = cognatogen_alchemist.GetComponents<SpellLevelByClassLevel>().ToArray();
            cognatogen_alchemist.RemoveComponents<SpellLevelByClassLevel>();
            foreach (var slc in spell_level_comps)
            {
                cognatogen_alchemist.AddComponent(Helpers.Create<NewMechanics.SpellLevelByClassLevel>(s =>
                {
                    s.Ability = slc.Ability;
                    s.Class = slc.Class;
                    s.ExtraClass = psychic_class;
                    s.ExtraFeatureToCheck = cognatogen;
                }));

                var buff = Common.extractActions<ContextActionApplyBuff>(slc.Ability.GetComponent<AbilityEffectRunAction>().Actions.Actions).FirstOrDefault().Buff;
                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
                var actions = Common.extractActions<Conditional>(slc.Ability.GetComponent<AbilityEffectRunAction>().Actions.Actions).FirstOrDefault();
                actions.IfFalse = Helpers.CreateActionList(Helpers.CreateConditional(Common.createContextConditionCasterHasFact(cognatogen),
                                                                                     new GameAction[] { apply_buff },
                                                                                     actions.IfFalse.Actions));
            }


            var apply_nauseted = Helpers.CreateActionSavingThrow(SavingThrowType.Will,
                                                                 Helpers.CreateConditionalSaved(null,
                                                                                                Common.createContextActionApplyBuff(NewSpells.nauseted_non_poison, Helpers.CreateContextDuration(1), dispellable: false)
                                                                                               )
                                                                );
            var wrapped_brain = Helpers.CreateFeature("WarpedBrainFeature",
                                                      "Warped Brain",
                                                      "At 5th level, your mind becomes difficult to comprehend. When another creature uses a mind-affecting spell or ability against you, that creature must attempt a Will save. If it fails, it becomes nauseated for 1 round. This ability triggers even if you succeed at your save (or are otherwise unaffected by the spell or ability), but doesn’t apply if you’re a willing subject of the spell. This is a mind-affecting effect.",
                                                      "",
                                                      Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"),
                                                      FeatureGroup.None,
                                                      Common.createContextCalculateAbilityParamsBasedOnClass(psychic_class, StatType.Intelligence),
                                                      Helpers.Create<SpellManipulationMechanics.ExtraEffectOnSpellApplyOnSpellCaster>(a =>
                                                                                                      {
                                                                                                          a.descriptor = SpellDescriptor.MindAffecting;
                                                                                                          a.only_enemies = !test_mode;
                                                                                                          a.actions = Helpers.CreateActionList(apply_nauseted);
                                                                                                      }
                                                                                                     )
                                                      );

            var confused = library.Get<BlueprintBuff>("886c7407dc629dc499b9f1465ff382df");
            var cooldown = Helpers.CreateBuff("HallucinogenicAuraCooldownBuff",
                                               "Hallucinogenic Aura Cooldown",
                                               "At 13th level, a mental field emanates from you, touching the minds of those nearby.\n"
                                               + "Any creature within 20 feet of you must succeed at a Will save or be confused for 1d4 rounds. A creature that succeeds at its saving throw is immune to your hallucinogenic aura for 24 hours. A creature that fails its save doesn’t need to continue making saves while it’s confused by this aura, and becomes immune for 24 hours once its confusion ends.\n"
                                               + "This is a mind-affecting effect. You’re immune to your own hallucinogenic aura, as well as your allies.\n",
                                               "",
                                               confused.Icon,
                                               null);
            cooldown.Stacking = StackingType.Stack;
            var apply_confused = Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(cooldown),
                                                          null,
                                                          new GameAction[]{ Helpers.CreateActionSavingThrow(SavingThrowType.Will,
                                                                                                            Helpers.CreateConditionalSaved(null,
                                                                                                                                           new GameAction[]{Common.createContextActionApplyBuff(confused, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1), dispellable: false),
                                                                                                                                                            Common.createContextActionApplyBuff(cooldown, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false)
                                                                                                                                                           }
                                                                                                                                           )
                                                                                                           ),
                                                                            Common.createContextActionApplyBuff(cooldown, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false)
                                                                           }
                                                          );
            apply_confused = Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), apply_confused);
            var area_effect = Helpers.CreateAreaEffectRunAction(unitEnter: apply_confused);

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("a70dc66c3059b7a4cb5b2a2e8ac37762", "HallucinogenicAuraArea", "");
            area.Size = 20.Feet();
            area.SpellResistance = false;
            area.ComponentsArray = new BlueprintComponent[] {area_effect,
                                                             Common.createContextCalculateAbilityParamsBasedOnClass(psychic_class, StatType.Intelligence),
                                                             Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting)
                                                            };

            var aura_buff = Helpers.CreateBuff("HallucinogenicAuraAreaBuff",
                                               "Hallucinogenic Aura",
                                               cooldown.Description,
                                               "",
                                               cooldown.Icon,
                                               null,
                                               Common.createAddAreaEffect(area),
                                               Common.createContextCalculateAbilityParamsBasedOnClass(psychic_class, StatType.Intelligence)
                                               );

            var hallucinogenic_aura_feature = Helpers.CreateFeature("HallucinogenicAuraFeature",
                                                                    aura_buff.Name,
                                                                    aura_buff.Description,
                                                                    "",
                                                                    aura_buff.Icon,
                                                                    FeatureGroup.None,
                                                                    Common.createAuraFeatureComponent(aura_buff)
                                                                    );


            createPsychicDiscipline("Psychedelia",
                                    "Psychedelia",
                                    "You ingest hallucinogens to expand your mind. Experimentation and study show you which ones will have the greatest effect. Your psychedelic forays put you into a different mental space from others, and normal people don’t really understand you.\n"
                                    + "Discipline Powers: You distort your own mind and perceptions, and can impress your altered states onto others.",
                                    cognatogen.Icon,
                                    StatType.Wisdom,
                                    new BlueprintAbility[]
                                    {
                                                        library.Get<BlueprintAbility>("40ec382849b60504d88946df46a10f2d"), //haze of dreams
                                                        library.Get<BlueprintAbility>("fd4d9fd7f87575d47aafe2a64a6e2d8d"), //hideous laughter
                                                        NewSpells.synesthesia,
                                                        library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949"), //confusion
                                                        library.Get<BlueprintAbility>("12fb4a4c22549c74d949e2916a2f0b6a"), //phantasmal web
                                                        library.Get<BlueprintAbility>("15a04c40f84545949abeedef7279751a"), //joyful rapture
                                                        library.Get<BlueprintAbility>("1e2d1489781b10a45a3b70192bba9be3"), //waves of ecstasy
                                                        library.Get<BlueprintAbility>("740d943e42b60f64a8de74926ba6ddf7"), //euphoric tranquility
                                                        NewSpells.divide_mind
                                    },
                                    cognatogen,
                                    wrapped_brain,
                                    hallucinogenic_aura_feature
                                    );
        }


        static void createFaithDisicipline()
        {
            var cleric_spontnaeous_inflict = library.Get<BlueprintFeature>("5ba6b9cc18acafd45b6293d1e03221ac");
            var cleric_spontaneous_cure = library.Get<BlueprintFeature>("5e4620cea099c9345a9207c11d7bc916");
            var inflict_spells = cleric_spontnaeous_inflict.GetComponent<SpontaneousSpellConversion>().SpellsByLevel.ToArray();
            var cure_spells = cleric_spontaneous_cure.GetComponent<SpontaneousSpellConversion>().SpellsByLevel.ToArray();

            var divine_energy_resource = Helpers.CreateAbilityResource("PsychicDivineEnergyResource", "", "", "", null);
            divine_energy_resource.SetIncreasedByStat(0, StatType.Wisdom);
            var restore_phrenic_pool = Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => { c.Resource = phrenic_pool_resource; c.amount = 1; });
            var effect_on_spell_cast = Helpers.CreateActionList(restore_phrenic_pool);

            var divine_energy_cure = Helpers.CreateFeature("FaithDivineEnergyCureFeature",
                                                           "Spontaneous Healing",
                                                           "You can channel spell energy into cure or inflict spells. This ability functions similarly to the cleric’s ability to spontaneously cast cure or inflict spells, and the type of spells you can convert depends on your alignment in the same way. The cure or inflict spells don’t count as being on your psychic spell list for the purposes of any other effects. Each time you use this ability to convert a spell, you regain 1 point in your phrenic pool.\n"
                                                           + "You can use this ability a number of times per day equal to your Wisdom modifier.",
                                                           "",
                                                           cleric_spontaneous_cure.Icon,
                                                           FeatureGroup.None,
                                                           divine_energy_resource.CreateAddAbilityResource(),
                                                           Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("8c769102f3996684fb6e09a2c4e7e5b9"))); //channel positive allowed


            for (int i = 1; i < cure_spells.Length; i++)
            {
                var duplicate_spell = library.CopyAndAdd(cure_spells[i], $"DivineEnrgyConversionCureSpell{i + 1}Ability", "");
                cure_spells[i] = duplicate_spell;
                duplicate_spell.AddComponent(divine_energy_resource.CreateResourceLogic());
            }

            divine_energy_cure.AddComponents(Common.createSpontaneousSpellConversion(psychic_class, cure_spells),
                                              Helpers.Create<OnCastMechanics.RunActionAfterSpellCastBasedOnLevel>(r =>
                                                                                                                {
                                                                                                                    r.actions = new ActionList[] { effect_on_spell_cast };
                                                                                                                    r.allow_sticky_touch = true;
                                                                                                                    r.specific_abilities = cure_spells;
                                                                                                                    r.specific_class = psychic_class;
                                                                                                                })
                                           );


            var divine_energy_inflict = Helpers.CreateFeature("FaithDivineEnergyInflictFeature",
                                               "Spontaneous Wounding",
                                               divine_energy_cure.Description,
                                               "",
                                               cleric_spontnaeous_inflict.Icon,
                                               FeatureGroup.None,
                                               divine_energy_resource.CreateAddAbilityResource(),
                                               Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("dab5255d809f77c4395afc2b713e9cd6"))); //channel negative allowed


            for (int i = 1; i < inflict_spells.Length; i++)
            {
                var duplicate_spell = library.CopyAndAdd(inflict_spells[i], $"DivineEnrgyConversionInflictSpell{i + 1}Ability", "");
                inflict_spells[i] = duplicate_spell;
                duplicate_spell.AddComponent(divine_energy_resource.CreateResourceLogic());
            }

            divine_energy_inflict.AddComponents(Common.createSpontaneousSpellConversion(psychic_class, inflict_spells),
                                                Helpers.Create<OnCastMechanics.RunActionAfterSpellCastBasedOnLevel>(r =>
                                                                                                                {
                                                                                                                    r.actions = new ActionList[] { effect_on_spell_cast };
                                                                                                                    r.allow_sticky_touch = true;
                                                                                                                    r.specific_abilities = inflict_spells;
                                                                                                                    r.specific_class = psychic_class;
                                                                                                                })
                                           );

            var divine_energy = Helpers.CreateFeatureSelection("DivineEnergyPsychicFeatureSelection",
                                                               "Divine Energy",
                                                               divine_energy_cure.Description,
                                                               "",
                                                               null,
                                                               FeatureGroup.None
                                                               );
            divine_energy.AllFeatures = new BlueprintFeature[] { divine_energy_cure, divine_energy_inflict };


            var resilence_of_the_faithful = Helpers.CreateFeature("ResilenceOfTheFaithfulFeature",
                                                                  "Resilience of the Faithful",
                                                                  "At 5th level, you gain a +2 resistance bonus on all saving throws. This bonus increases by 1 for every 5 levels you possess beyond 5th.",
                                                                  "",
                                                                  Helpers.GetIcon("a05a8959c594daa40a1c5add79566566"),
                                                                  FeatureGroup.None,
                                                                  Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Resistance),
                                                                  Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Resistance),
                                                                  Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Resistance),
                                                                  Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getPsychicArray(),
                                                                                                  progression: ContextRankProgression.OnePlusDivStep,
                                                                                                  stepLevel: 5)
                                                                  );
            resilence_of_the_faithful.ReapplyOnLevelUp = true;


            var prayer_buff = library.Get<BlueprintBuff>("789bae3802e7b6b4c8097aaf566a1cf5");
            var prayer_debuff = library.Get<BlueprintBuff>("890182fa30a5f724c86ce41f237cf95f");
            var prayer_debuff2 = library.CopyAndAdd(prayer_debuff, "PrayerAuraAlignmentDifferenceDebuff", "");
            prayer_debuff2.SetName("Prayer(Enemies, Alignment Difference)");

            var area_effect = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", "PrayerAuraAreaEffect", "");
            area_effect.Size = 30.Feet();
            area_effect.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.Create<AbilityAreaEffectBuff>(a =>
                {
                    a.Buff = prayer_buff;
                    a.Condition = Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>());
                }
                ),
                Helpers.Create<AbilityAreaEffectBuff>(a =>
                {
                    a.Buff = prayer_debuff;
                    a.Condition = Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>());
                }
                ),
                Helpers.Create<AbilityAreaEffectBuff>(a =>
                {
                    a.Buff = prayer_debuff2;
                    a.Condition = Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>(), Helpers.Create<NewMechanics.ContextConditionStrictAlignmentDifference>());
                }
                )
            };
            area_effect.Fx = Common.createPrefabLink("6b75812d8c3b0d34f9bc204d6babc2a1");//enchantment aoe
            var area_buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", "AreaPrayerAuraBuff", "");
            area_buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area_effect);

            area_buff.SetNameDescriptionIcon("Prayer Aura",
                                             "At 13th level, as a free action, you can extend an aura around you to bolster your allies and make your enemies less effective. You can use this aura for a number of rounds per day equal to your psychic level. These rounds don’t need to be consecutive. This functions as the prayer spell, granting your allies a +1 luck bonus on attack rolls, weapon damage rolls, saves, and skill checks and imposing a –1 penalty on your enemies’ rolls of those types. If you are chaotic, lawful, good, or evil, the penalty from your aura changes to –2 against creatures of an opposing alignment. The penalty doesn’t change further for a creature that opposes you on two alignment axes (such as a chaotic evil creature fighting a lawful good psychic).",
                                             prayer_buff.Icon);

            var prayer_aura_resource = Helpers.CreateAbilityResource("PsychicPrayerAuraResource", "", "", "", null);
            prayer_aura_resource.SetIncreasedByLevel(0, 1, getPsychicArray());

            var prayer_aura_toggle = Common.buffToToggle(area_buff, UnitCommand.CommandType.Free, true,
                                             prayer_aura_resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.NewRound)
                                             );
            prayer_aura_toggle.DeactivateIfCombatEnded = !test_mode;
            var prayer_aura_feature = Common.ActivatableAbilityToFeature(prayer_aura_toggle, false);

            prayer_aura_toggle.AddComponent(prayer_aura_resource.CreateAddAbilityResource());

            createPsychicDiscipline("Faith",
                                    "Faith",
                                    "Your belief in a higher power fuels your psychic abilities. Whether your mental abilities truly come to you as a divine gift or are simply enhanced by the power of your belief, none can say. In many ways, you resemble a divine caster, and prayers often factor into your casting of psychic spells.\n"
                                    + "Discipline Powers: Your powers serve to protect or cure you and your allies.",
                                    prayer_buff.Icon,
                                    StatType.Wisdom,
                                    new BlueprintAbility[]
                                    {
                                            library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638"), //bless
                                            NewSpells.force_sword,
                                            library.Get<BlueprintAbility>("2d4263d80f5136b4296d6eb43a221d7d"), //magical vestement
                                            library.Get<BlueprintAbility>("f2115ac1148256b4ba20788f7e966830"), //restoration
                                            library.Get<BlueprintAbility>("1bc83efec9f8c4b42a46162d72cbf494"), //burst of glory
                                            NewSpells.psychic_surgery,
                                            library.Get<BlueprintAbility>("fafd77c6bfa85c04ba31fdc1c962c914"), //restoration greater
                                            library.Get<BlueprintAbility>("d3ac756a229830243a72e84f3ab050d0"), //summon monster VIII
                                            library.Get<BlueprintAbility>("867524328b54f25488d371214eea0d90"), //heal mass
                                    },
                                    divine_energy,
                                    resilence_of_the_faithful,
                                    prayer_aura_feature
                                    );
            var atheism = library.Get<BlueprintFeature>("92c0d2da0a836ce418a267093c09ca54");
            psychic_disiciplines_map["Faith"].AddComponent(Helpers.PrerequisiteNoFeature(atheism));
            no_spells_psychic_disiciplines_map["Faith"].AddComponent(Helpers.PrerequisiteNoFeature(atheism));
        }


        static void createAbominationDiscipline()
        {
            var morphic_form = Helpers.CreateFeature("MorphicForFeature",
                                                     "Morphic Form",
                                                     "At 5th level, while manifesting your dark half, you gain DR 5. This damage reduction can be overcome by a random type of damage each time you manifest your dark half (either bludgeoning, cold iron or magic).",
                                                     "",
                                                     Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                     FeatureGroup.None
                                                     );

            var morphic_form_buff1 = Helpers.CreateBuff("MorphicFormBludgeoningBuff",
                                                         morphic_form.Name + ": DR 5/Bludgeoning",
                                                        morphic_form.Description,
                                                         "",
                                                         Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                         null,
                                                         Common.createContextFormDR(5, PhysicalDamageForm.Bludgeoning)
                                                         );
            var morphic_form_buff2 = Helpers.CreateBuff("MorphicFormColdIronBuff",
                                             morphic_form.Name + ": DR 5/Cold Iron",
                                             morphic_form.Description,
                                             "",
                                             Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                             null,
                                             Common.createMatrerialDR(5, PhysicalDamageMaterial.ColdIron)
                                             );
            var morphic_form_buff3 = Helpers.CreateBuff("MorphicFormMagicBuff",
                                             morphic_form.Name + ": DR 5/Magic",
                                             morphic_form.Description,
                                             "",
                                             Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                             null,
                                             Common.createMagicDR(5)
                                             );
            var apply_morphic_form_buff1 = Common.createContextActionApplyBuff(morphic_form_buff1, Helpers.CreateContextDuration(), dispellable: false, is_child: true, is_permanent: true);
            var apply_morphic_form_buff2 = Common.createContextActionApplyBuff(morphic_form_buff2, Helpers.CreateContextDuration(), dispellable: false, is_child: true, is_permanent: true);
            var apply_morphic_form_buff3 = Common.createContextActionApplyBuff(morphic_form_buff3, Helpers.CreateContextDuration(), dispellable: false, is_child: true, is_permanent: true);

            var psychic_safeguard = Helpers.CreateFeature("PsychicSafeGuardFeature",
                                                          "Psychic Safeguard",
                                                          "At 13th level, you project constant mental defenses, gaining spell resistance equal to 8 + your caster level. While manifesting your dark half, this spell resistance increases to 16 + your caster level.",
                                                          "",
                                                          Helpers.GetIcon("0a5ddfbcfb3989543ac7c936fc256889"),
                                                          FeatureGroup.None,
                                                          Helpers.Create<AddSpellResistance>(a => a.Value = Helpers.CreateContextValue(AbilityRankType.Default)),
                                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getPsychicArray(),
                                                                                          progression: ContextRankProgression.BonusValue,
                                                                                          stepLevel: 8)
                                                          );
            psychic_safeguard.ReapplyOnLevelUp = true;
            var psychic_safeguard_buff = Helpers.CreateBuff("PsychicSafeGuardBuff",
                                                              psychic_safeguard.Name,
                                                              psychic_safeguard.Description,
                                                              "",
                                                              psychic_safeguard.Icon,
                                                              null,
                                                              Helpers.Create<AddSpellResistance>(a => a.Value = Helpers.CreateContextValue(AbilityRankType.Default)),
                                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getPsychicArray(),
                                                                                              progression: ContextRankProgression.BonusValue,
                                                                                              stepLevel: 16)
                                                              );

            var apply_psychic_safeguard_buff = Common.createContextActionApplyBuff(psychic_safeguard_buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true, dispellable: false);
            var resource = Helpers.CreateAbilityResource("DarkHalfResource", "", "", "", null);
            resource.SetIncreasedByStat(0, StatType.Charisma);
            resource.SetIncreasedByLevelStartPlusDivStep(3, 2, 1, 2, 1, 0, 0.0f, getPsychicArray());


            var bleed1d6 = library.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562");

            var bleed_buff = Helpers.CreateBuff("DarkHalfBleedBuff",
                                         "Dark Half: Bleed",
                                         "This creature takes 1 point of bleed damage per turn. The amount of bleed damage increases to 2 points at 5th level and to 3 points at 13th level. Bleeding can be stopped through the application of any spell that cures hit point damage (even if the bleed is ability damage).",
                                         "",
                                         bleed1d6.Icon,
                                         null,
                                         Helpers.Create<BleedMechanics.BleedBuff>(b => b.dice_value = Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default))),
                                         Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getPsychicArray(),
                                                                         progression: ContextRankProgression.Custom,
                                                                         customProgression: new (int, int)[] { (4, 1), (12, 2), (20, 3) }
                                                                         ),
                                         bleed1d6.GetComponent<CombatStateTrigger>(),
                                         bleed1d6.GetComponent<AddHealTrigger>(),
                                         Helpers.CreateSpellDescriptor(SpellDescriptor.Bleed)
                                         );

            var spend_resource = Helpers.CreateConditional(Helpers.Create<ResourceMechanics.ContextConditionTargetHasEnoughResource>(c => c.resource = resource),
                                                                                                                                    Helpers.Create<NewMechanics.ContextActionSpendResource>(c => { c.resource = resource; c.amount = 1; }),
                                                                                                                                    Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                                    );
            var buff = Helpers.CreateBuff("DarkHalfBuff",
                                          "Dark Half",
                                          "By allowing the dark forces to overcome you, you can enter a state of instinctual cruelty as a swift action.\n"
                                          + "While you’re manifesting your dark half, you increase the DCs of your psychic spells by 1, gain a +2 morale bonus on Will saves, and become immune to fear effects. Whenever you cast a spell that deals damage while manifesting your dark half, you can cause any creature that took damage from the spell to also take 1 point of bleed damage. The amount of bleed damage increases to 2 points at 5th level and to 3 points at 13th level. While manifesting your dark half, you can’t use any Charisma-, Dexterity-, or Intelligence-based skills (except Mobility and Intimidate) or any ability that requires patience or concentration other than casting spells using psychic magic, using phrenic amplifications, or attempting to return to normal. You can attempt to return to your normal self as a free action, but must succeed at an Intelligence check with a DC equal to 10. If you fail, you continue to manifest your dark half and can’t attempt to change back for 1 round.\n"
                                          + "You can manifest your dark half for a number of rounds per day equal to 3 + 1/2 your psychic level + your Charisma modifier; when these rounds are expended, you return to your normal self without requiring a concentration check.",
                                          "",
                                          Helpers.GetIcon("1d48ab2bded57a74dad8af3da07d313a"), //dirge of doom
                                          Common.createPrefabLink("53c86872d2be80b48afc218af1b204d7"), //rage
                                          Helpers.CreateAddStatBonus(StatType.SaveWill, 2, ModifierDescriptor.Morale),
                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Fear | SpellDescriptor.Shaken),
                                          Common.createBuffDescriptorImmunity(SpellDescriptor.Fear | SpellDescriptor.Shaken),
                                          Helpers.Create<NewMechanics.IncreaseAllSpellsDCForSpecificSpellbook>(a => { a.specific_class = psychic_class; a.Value = 1; }),
                                          Helpers.CreateAddStatBonus(StatType.SkillThievery, -20, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.SkillUseMagicDevice, -20, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.CheckDiplomacy, -20, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.CheckBluff, -20, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, -20, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, -20, ModifierDescriptor.UntypedStackable),
                                          Helpers.Create<NewMechanics.ActionOnSpellDamage>(a =>
                                            {
                                                a.descriptor = SpellDescriptor.None;
                                                a.use_energy = false;
                                                a.action = Helpers.CreateActionList(Common.createContextActionApplyBuff(bleed_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true));
                                            }),
                                          Helpers.CreateAddFactContextActions(activated: new GameAction[] {Helpers.CreateConditional(Common.createContextConditionCasterHasFact(morphic_form),
                                                                                                                                     Common.createContextActionRandomize(apply_morphic_form_buff1, 
                                                                                                                                                                         apply_morphic_form_buff2, 
                                                                                                                                                                         apply_morphic_form_buff3)
                                                                                                                                     ),
                                                                                                           Helpers.CreateConditional(Common.createContextConditionCasterHasFact(psychic_safeguard),
                                                                                                                                     apply_psychic_safeguard_buff),
                                                                                                           spend_resource,
                                                                                                           Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Fear | SpellDescriptor.Shaken)
                                                                                                           },
                                                                              newRound: new GameAction[] {spend_resource
                                                                                                          }
                                                                              ),
                                          resource.CreateResourceLogic()
                                          );

            var remove_action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(buff),
                                                          Common.createContextActionSkillCheck(StatType.Intelligence,
                                                                                               Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff),
                                                                                                                        Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                       ),
                                                                                               custom_dc: 10
                                                                                              ),
                                                          Helpers.Create<ContextActionRemoveSelf>()
                                                          );

            var deactivate_buff = Helpers.CreateBuff("DeactivateDarkHalfBuff",
                                                     "Dark Half Deactivate Attempt",
                                                     buff.Description,
                                                     "",
                                                     Helpers.GetIcon("d316d3d94d20c674db2c24d7de96f6a7"), //serenity
                                                     null,
                                                     Helpers.CreateAddFactContextActions(activated: remove_action, newRound: remove_action)
                                                     );

            var toggle_buff = Helpers.CreateBuff("DarkHalfToggleBuff",
                                                 "",
                                                 "",
                                                 "",
                                                 null,
                                                 null,
                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                                                     deactivated: Common.createContextActionApplyBuff(deactivate_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true)
                                                                                     )
                                                 );
            toggle_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var dark_half_toggle = Common.buffToToggle(toggle_buff, test_mode ? CommandType.Free : CommandType.Swift, true,
                                                 resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                                 Helpers.Create<RestrictionHasFact>(a => { a.Feature = deactivate_buff; a.Not = true; })
                                                 );
            dark_half_toggle.SetNameDescriptionIcon(buff.Name, buff.Description, buff.Icon);
            

            var dark_half = Common.ActivatableAbilityToFeature(dark_half_toggle, false);
            dark_half.AddComponent(resource.CreateAddAbilityResource());

            createPsychicDiscipline("Abomination",
                                    "Abomination",
                                    "Your mind is impure, tainted by outside forces. These might be monstrous ancestors whose blood still flows within you, or powerful and unknowable psychic forces that intrude upon your mind. Like a psychic disease, this influence consumes part of your brain, creating a dark counterpart to your normal self. Every time you call forth a psychic spell, you’re drawing on this dangerous force—and potentially giving it a greater hold on you. This malign influence might stem from creatures like rakshasas and aboleths, or perhaps malign entities that dwell in the voids between the stars.\n"
                                    + "Discipline Powers: Your powers allow the dark influences to take over, and it can be difficult to come back from the brink.",
                                    dark_half.Icon,
                                    StatType.Charisma,
                                    new BlueprintAbility[]
                                    {
                                        library.Get<BlueprintAbility>("450af0402422b0b4980d9c2175869612"), //ray of enfeeblement
                                        library.Get<BlueprintAbility>("14ec7a4e52e90fa47a4c8d63c69fd5c1"), //blur
                                        NewSpells.ray_of_exhaustion,
                                        library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949"), //confusion
                                        NewSpells.burst_of_force,
                                        NewSpells.mental_barrier[4],
                                        library.Get<BlueprintAbility>("2b044152b3620c841badb090e01ed9de"), //insanity
                                        NewSpells.orb_of_the_void,
                                        NewSpells.telekinetic_storm
                                    },
                                    dark_half,
                                    morphic_form,
                                    psychic_safeguard
                                    );
        }


        static void createPsychicProficiencies()
        {
            psychic_proficiencies = library.CopyAndAdd<BlueprintFeature>("25c97697236ccf2479d0c6a4185eae7f", //sorcerer proficiencies
                                                                            "PsychicProficiencies",
                                                                            "");
            psychic_proficiencies.SetName("Psychic Proficiencies");
            psychic_proficiencies.SetDescription("A psychic is proficient with all simple weapons, but not with any type of armor or shield.");
        }


        static void createPhrenicAmplification()
        {
            phrenic_pool_resource = Helpers.CreateAbilityResource("PsychicPhrenicPoolResource", "", "", "", null);
            phrenic_pool_resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, getPsychicArray());

            phrenic_pool = Helpers.CreateFeature("PhrenicPoolFeature",
                                                           "Phrenic Pool",
                                                           "A psychic has a pool of supernatural mental energy that she can draw upon to manipulate psychic spells as she casts them. The maximum number of points in a psychic’s phrenic pool is equal to 1/2 her psychic level + her Wisdom or Charisma modifier, as determined by her psychic discipline. The phrenic pool is replenished each morning after 8 hours of rest or meditation; these hours don’t need to be consecutive. The psychic might be able to recharge points in her phrenic pool in additional circumstances dictated by her psychic discipline. Points gained in excess of the pool’s maximum are lost.",
                                                           "",
                                                           Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"), //mind fog
                                                           FeatureGroup.None,
                                                           Helpers.CreateAddAbilityResource(phrenic_pool_resource)
                                                           );

            phrenic_amplification = Helpers.CreateFeatureSelection("PhrenicAmplificationsFeatureSelection",
                                                                   "Phrenic Amplifications",
                                                                   "A psychic develops particular techniques to empower her spellcasting, called phrenic amplifications. The psychic can activate a phrenic amplification only while casting a spell using psychic magic, and the amplification modifies either the spell’s effects or the process of casting it. The spell being cast is called the linked spell. The psychic can activate only one amplification each time she casts a spell, and doing so is part of the action used to cast the spell. She can use any amplification she knows with any psychic spell, unless the amplification’s description states that it can be linked only to certain types of spells. A psychic learns one phrenic amplification at 1st level, selected from the list below. At 3rd level and every 4 levels thereafter, the psychic learns a new phrenic amplification.",
                                                                   "",
                                                                   null,
                                                                   FeatureGroup.None
                                                                   );

            major_amplification = Helpers.CreateFeature("MajorAmplificationFeature",
                                                   "Major Amplifications",
                                                   "Starting from 11th level, psychic can choose a major amplificaiton instead of a phrenic amplification.",
                                                   "",
                                                   LoadIcons.Image2Sprite.Create(@"AbilityIcons/Metamixing.png"),
                                                   FeatureGroup.None
                                                   );





            var phrenic_amplifications_engine = new PhrenicAmplificationsEngine(phrenic_pool_resource, psychic_class.Spellbook, psychic_class, "Psychic");

            focused_force = phrenic_amplifications_engine.createFocusedForce();
            biokinetic_healing = phrenic_amplifications_engine.createBiokineticHealing();
            conjured_armor = phrenic_amplifications_engine.createConjuredArmor();
            defensive_prognostication = phrenic_amplifications_engine.createDefensivePrognostication();
            minds_eye = phrenic_amplifications_engine.createMindsEye();
            overpowering_mind = phrenic_amplifications_engine.createOverpoweringMind();
            will_of_the_dead = phrenic_amplifications_engine.createWillOfTheDead();
            ongoing_defense = phrenic_amplifications_engine.createOngoingDefense();
            relentness_casting = phrenic_amplifications_engine.createRelentnessCasting();
            undercast_surge = phrenic_amplifications_engine.createUndercastSurge();
            psychofeedback = phrenic_amplifications_engine.createPsychofeedback();

            synaptic_shock = phrenic_amplifications_engine.createSynapticShock();
            space_rending_spell = phrenic_amplifications_engine.createSpaceRendingSpell();
            dual_amplification = phrenic_amplifications_engine.createDualAmplification();
            mimic_metamagic = phrenic_amplifications_engine.createMimicMetamagic();

            synaptic_shock.AddComponent(Helpers.PrerequisiteFeature(major_amplification));
            space_rending_spell.AddComponent(Helpers.PrerequisiteFeature(major_amplification));
            dual_amplification.AddComponent(Helpers.PrerequisiteFeature(major_amplification));

            foreach (var mm in mimic_metamagic)
            {
                mm.AddComponent(Helpers.PrerequisiteFeature(major_amplification));
            }
            phrenic_amplification.AllFeatures = new BlueprintFeature[]
            {
                biokinetic_healing,
                conjured_armor,
                defensive_prognostication,
                focused_force,
                minds_eye,
                overpowering_mind,
                will_of_the_dead,
                ongoing_defense,
                relentness_casting,
                undercast_surge,
                psychofeedback,
                synaptic_shock,
                space_rending_spell,
                dual_amplification
            }.AddToArray(mimic_metamagic);

            phrenic_mastery = Helpers.CreateFeature("PhrenicMasteryFeature",
                                                    "Phrenic Mastery",
                                                    "At 20th level, the psychic’s mind is a legendary weapon in its own right. The psychic’s phrenic pool increases by 6, and she gains two new phrenic amplifications.",
                                                    "",
                                                    Helpers.GetIcon("fafd77c6bfa85c04ba31fdc1c962c914"), //greater restoration
                                                    FeatureGroup.None,
                                                    Helpers.Create<IncreaseResourceAmount>(i => { i.Resource = phrenic_pool_resource; i.Value = 6; }),
                                                    Helpers.Create<EvolutionMechanics.addSelection>(a => a.selection = phrenic_amplification),
                                                    Helpers.Create<EvolutionMechanics.addSelection>(a => a.selection = phrenic_amplification)
                                                    );
        }


        static BlueprintSpellbook createPsychicSpellbook()
        {
            var sorcerer_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
            var psychic_spellbook = Helpers.Create<BlueprintSpellbook>();
            psychic_spellbook.name = "PsychicSpellbook";
            library.AddAsset(psychic_spellbook, "");
            psychic_spellbook.Name = psychic_class.LocalizedName;
            psychic_spellbook.SpellsPerDay = sorcerer_class.Spellbook.SpellsPerDay;
            psychic_spellbook.SpellsKnown = sorcerer_class.Spellbook.SpellsKnown;
            psychic_spellbook.Spontaneous = true;
            psychic_spellbook.IsArcane = false;
            psychic_spellbook.AllSpellsKnown = false;
            psychic_spellbook.CanCopyScrolls = false;
            psychic_spellbook.CastingAttribute = StatType.Intelligence;
            psychic_spellbook.CharacterClass = psychic_class;
            psychic_spellbook.CasterLevelModifier = 0;
            psychic_spellbook.CantripsType = CantripsType.Cantrips;
            psychic_spellbook.SpellsPerLevel = sorcerer_class.Spellbook.SpellsPerLevel;

            psychic_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            psychic_spellbook.SpellList.name = "PsychicSpellList";
            library.AddAsset(psychic_spellbook.SpellList, "");
            psychic_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < psychic_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                psychic_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);

            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "55f14bc84d7c85446b07a1b5dd6b2b4c", 0), //daze
                new Common.SpellId( "f0f8e5b9808f44e4eadd22b138131d52", 0), //flare
                new Common.SpellId( "95f206566c5261c42aa5b3e7e0d1e36c", 0), //mage light
                new Common.SpellId( "7bc8e27cba24f0e43ae64ed201ad5785", 0), //resistance
                new Common.SpellId( "d3a852385ba4cd740992d1970170301a", 0), //virtue

                new Common.SpellId( NewSpells.burst_of_adrenaline.AssetGuid, 1),
                new Common.SpellId( NewSpells.burst_of_insight.AssetGuid, 1),
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( "91da41b9793a4624797921f221db653c", 1), //color spray
                new Common.SpellId( NewSpells.command.AssetGuid, 1),
                new Common.SpellId( "8e7cfa5f213a90549aadd18f8f6f4664", 1), //ear-piercing scream
                new Common.SpellId( "c60969e7f264e6d4b84a1499fdcf9039", 1), //enlarge person
                new Common.SpellId( "4f8181e7a7f1d904fbaea64220e83379", 1), //expeditious retreat
                new Common.SpellId( "39a602aa80cc96f4597778b6d4d49c0a", 1), //flare burst
                new Common.SpellId( Witch.hermean_potential.AssetGuid, 1),
                new Common.SpellId( "88367310478c10b47903463c5d0152b0", 1), //hypnotism
                new Common.SpellId( Witch.ill_omen.AssetGuid, 1),
                new Common.SpellId( NewSpells.invigorate.AssetGuid, 1),
                new Common.SpellId( NewSpells.long_arm.AssetGuid, 1),
                new Common.SpellId( "9e1ad5d6f87d19e4d8883d63a6e35568", 1), //mage armor
                new Common.SpellId( "4ac47ddb9fa1eaf43a1b6809980cfbd2", 1), //magic missile
                new Common.SpellId( NewSpells.mind_thrust[0].AssetGuid, 1),
                new Common.SpellId( "4e0e9aba6447d514f88eff1464cc4763", 1), //reduce person
                new Common.SpellId( "55a037e514c0ee14a8e3ed14b47061de", 1), //remove fear
                new Common.SpellId( "ef768022b0785eb43a18969903c537c4", 1), //shield
                new Common.SpellId( "bb7ecad2d3d2c8247a38f44855c99061", 1), //sleep
                new Common.SpellId( "a5ec7892fb1c2f74598b3a82f3fd679f", 1), //stunning barrier
                new Common.SpellId( "8fd74eddd9b6c224693d9ab241f25e84", 1), //summon monster 1
                new Common.SpellId( "2c38da66e5a599347ac95b3294acbe00", 1), //true strike
                new Common.SpellId( "f001c73999fb5a543a199f890108d936", 1), //vanish

                new Common.SpellId( "a900628aea19aa74aad0ece0e65d091a", 2), //bear's endurance
                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 2), //blindness
                new Common.SpellId( NewSpells.blood_armor.AssetGuid, 2),
                new Common.SpellId( "14ec7a4e52e90fa47a4c8d63c69fd5c1", 2), //blur
                new Common.SpellId( NewSpells.bone_fists.AssetGuid, 2),
                new Common.SpellId( "4c3d08935262b6544ae97599b3a9556d", 2), //bull's strength
                new Common.SpellId( "de7a025d48ad5da4991e7d3c682cf69d", 2), //cat's grace
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagle's splendor
                new Common.SpellId( "e1291272c8f48c14ab212a599ad17aac", 2), //effortless armor
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( "4709274b2080b6444a3c11c6ebbe2404", 2), //find traps
                new Common.SpellId( NewSpells.force_sword.AssetGuid, 2),
                new Common.SpellId( "ae4d3ad6a8fda1542acf2e9bbc13d113", 2), //fox cunning
                new Common.SpellId( "fd4d9fd7f87575d47aafe2a64a6e2d8d", 2), //hideous laughter
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 2), //hold person
                new Common.SpellId( "41bab342089c0254ca222eb918e98cd4", 2), //hold animal
                new Common.SpellId( NewSpells.howling_agony.AssetGuid, 2),
                new Common.SpellId( NewSpells.hypnotic_pattern.AssetGuid, 2),
                new Common.SpellId( NewSpells.inflict_pain.AssetGuid, 2),
                new Common.SpellId( "89940cde01689fb46946b2f8cd7b66b7", 2), //invisibility
                new Common.SpellId( NewSpells.mental_barrier[0].AssetGuid, 2),
                new Common.SpellId( NewSpells.mind_thrust[1].AssetGuid, 2),
                new Common.SpellId( "3e4ab69ada402d145a5e0ad3ad4b8564", 2), //mirror image
                new Common.SpellId( NewSpells.pain_strike.AssetGuid, 2),
                new Common.SpellId( "c28de1f98a3f432448e52e5d47c73208", 2), //protection from arrows
                new Common.SpellId( "21ffef7791ce73f468b6fca4d9371e8b", 2), //resist energy
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( NewSpells.savage_maw.AssetGuid, 2),
                new Common.SpellId( NewSpells.silence.AssetGuid, 2),
                new Common.SpellId( "f0455c9295b53904f9e02fc571dd2ce1", 2), //owl's wisdom
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( "970c6db48ff0c6f43afc9dbb48780d03", 2), //summon small elemental
                new Common.SpellId( "1724061e89c667045a6891179ee2e8e7", 2), //summon monster 2
                new Common.SpellId( NewSpells.telekinetic_strikes.AssetGuid, 2),
                new Common.SpellId( NewSpells.thought_shield[0].AssetGuid, 2),
                new Common.SpellId( NewSpells.warding_weapon.AssetGuid, 2),

                new Common.SpellId( NewSpells.babble.AssetGuid, 3),
                new Common.SpellId( "0a2f7c6aa81bc6548ac7780d8b70bcbc", 3), //battering blast (it seems it should be on the list since all force spells are there)
                new Common.SpellId( NewSpells.countless_eyes.AssetGuid, 3),
                new Common.SpellId( NewSpells.daze_mass.AssetGuid, 3),
                new Common.SpellId( "7658b74f626c56a49939d9c20580885e", 3), //deep slumber
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //displacement
                new Common.SpellId( NewSpells.fly.AssetGuid, 3),
                new Common.SpellId( "486eaff58293f6441a5c2759c4872f98", 3), //haste
                new Common.SpellId( "5ab0d42fb68c9e34abae4921822b9d63", 3), //heroism
                new Common.SpellId( NewSpells.locate_weakness.AssetGuid, 3),
                new Common.SpellId( NewSpells.mental_barrier[1].AssetGuid, 3),
                new Common.SpellId( NewSpells.mind_thrust[2].AssetGuid, 3),
                new Common.SpellId( "96c9d98b6a9a7c249b6c4572e4977157", 3), //protection from arrows communal
                new Common.SpellId( "d2f116cfe05fcdd4a94e80143b67046f", 3), //protection from energy
                new Common.SpellId( "97b991256e43bb140b263c326f690ce2", 3), //rage
                new Common.SpellId( "7bb0c402f7f789d4d9fae8ca87b4c7e2", 3), //resist energy communal
                new Common.SpellId( NewSpells.resinous_skin.AssetGuid, 3),
                new Common.SpellId( NewSpells.sands_of_time.AssetGuid, 3),
                new Common.SpellId( NewSpells.shadow_enchantment.AssetGuid, 3),
                new Common.SpellId( "f492622e473d34747806bdb39356eb89", 3), //slow
                new Common.SpellId( NewSpells.stunning_barrier_greater.AssetGuid, 3),
                new Common.SpellId( NewSpells.synaptic_pulse.AssetGuid, 3),
                new Common.SpellId( NewSpells.synesthesia.AssetGuid, 3),
                new Common.SpellId( NewSpells.thought_shield[1].AssetGuid, 3),
                new Common.SpellId( SpiritualWeapons.twilight_knife.AssetGuid, 3),
                new Common.SpellId( "8a28a811ca5d20d49a863e832c31cce1", 3), //vampyric touch
                new Common.SpellId( NewSpells.wall_of_nausea.AssetGuid, 3),
                new Common.SpellId( "5d61dde0020bbf54ba1521f7ca0229dc", 3), //summon monster 3

                new Common.SpellId( NewSpells.aura_of_doom.AssetGuid, 4),
                new Common.SpellId( "7792da00c85b9e042a0fdfc2b66ec9a8", 4), //break enchantment
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 4), //confusion
                new Common.SpellId( "4baf4109145de4345861fe0f2209d903", 4), //crushing despair
                new Common.SpellId( NewSpells.debilitating_portent.AssetGuid, 4),
                new Common.SpellId( "4a648b57935a59547b7a2ee86fb4f26a", 4), //dimensions door
                new Common.SpellId( "754c478a2aa9bb54d809e648c3f7ac0e", 4), //dominate animal
                new Common.SpellId( "66dc49bf154863148bd217287079245e", 4), //enlarge person mass
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life, greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( NewSpells.fleshworm_infestation.AssetGuid, 4),
                new Common.SpellId( "0087fc2d64b6095478bc7b8d7d512caf", 4), //freedom of movement
                new Common.SpellId( NewSpells.globe_of_invulnerability_lesser.AssetGuid, 4),
                new Common.SpellId( NewSpells.intellect_fortress.AssetGuid, 4),
                new Common.SpellId( NewSpells.invigorate_mass.AssetGuid, 4),
                new Common.SpellId( "ecaa0def35b38f949bd1976a6c9539e0", 4), //invisibility greater
                new Common.SpellId( NewSpells.mental_barrier[2].AssetGuid, 4),
                new Common.SpellId( NewSpells.mind_thrust[3].AssetGuid, 4),
                new Common.SpellId( "dd2918e4a77c50044acba1ac93494c36", 4), //overwhelming grief
                new Common.SpellId( NewSpells.pain_strike_mass.AssetGuid, 4),
                new Common.SpellId( "6717dbaef00c0eb4897a1c908a75dfe5", 4), //phantasmal killer
                new Common.SpellId( "76a629d019275b94184a1a8733cac45e", 4), //protection from energy communal
                new Common.SpellId( "4b8265132f9c8174f87ce7fa6d0fe47b", 4), //rainbow pattern
                new Common.SpellId( "2427f2e3ca22ae54ea7337bbab555b16", 4), //reduce person mass  
                new Common.SpellId( "f09453607e683784c8fca646eec49162", 4), //shout
                new Common.SpellId( "c66e86905f7606c4eaa5c774f0357b2b", 4), //stoneskin
                new Common.SpellId( "e42b1dbff4262c6469a9ff0a6ce730e3", 4), //summon medium elemental
                new Common.SpellId( "7ed74a3ec8c458d4fb50b192fd7be6ef", 4), //summon monster 4
                new Common.SpellId( NewSpells.thought_shield[2].AssetGuid, 4),
                new Common.SpellId( NewSpells.wall_of_blindness.AssetGuid, 4),

                new Common.SpellId( NewSpells.burst_of_force.AssetGuid, 5),
                new Common.SpellId( NewSpells.command_greater.AssetGuid, 5),
                new Common.SpellId( "95f7cdcec94e293489a85afdf5af1fd7", 5), //dismissal
                new Common.SpellId( "d7cbd2004ce66a042aeab2e95a3c5c61", 5), //dominate person
                new Common.SpellId( "20b548bf09bb3ea4bafea78dcb4f3db6", 5), //echolocation
                new Common.SpellId( "444eed6e26f773a40ab6e4d160c67faa", 5), //feeblemind
                new Common.SpellId( "41e8a952da7a5c247b3ec1c2dbb73018", 5), //hold monster
                new Common.SpellId( "eabf94e4edc6e714cabd96aa69f8b207", 5), //mind fog
                new Common.SpellId( NewSpells.mind_thrust[4].AssetGuid, 5),
                new Common.SpellId( NewSpells.overland_flight.AssetGuid, 5),
                new Common.SpellId( "12fb4a4c22549c74d949e2916a2f0b6a", 5), //phantasmal web
                new Common.SpellId( "d316d3d94d20c674db2c24d7de96f6a7", 5), //serenity
                new Common.SpellId( "d38aaf487e29c3d43a3bffa4a4a55f8f", 5), //song of discord
                new Common.SpellId( "0a5ddfbcfb3989543ac7c936fc256889", 5), //spell resistance
                new Common.SpellId( "7c5d556b9a5883048bf030e20daebe31", 5), //stoneskin communal
                new Common.SpellId( "89404dd71edc1aa42962824b44156fe5", 5), //summon large elemental
                new Common.SpellId( "630c8b85d9f07a64f917d79cb5905741", 5), //summon monster 5
                new Common.SpellId( NewSpells.mental_barrier[3].AssetGuid, 5),
                new Common.SpellId( NewSpells.psychic_crush[0].AssetGuid, 5),
                new Common.SpellId( "4cf3d0fae3239ec478f51e86f49161cb", 5), //true seeing
                new Common.SpellId( NewSpells.suffocation.AssetGuid, 5),
                new Common.SpellId( NewSpells.synaptic_pulse_greater.AssetGuid, 5),
                new Common.SpellId( NewSpells.synapse_overload.AssetGuid, 5),
                new Common.SpellId( "8878d0c46dfbd564e9d5756349d5e439", 5), //waves of fatigue
                
                new Common.SpellId( "d42c6d3f29e07b6409d670792d72bc82", 6), //banshee blast ? (probably should be since wail of banshee is also on the list)
                new Common.SpellId( "f6bcea6db14f0814d99b54856e918b92", 6), //bears endurance mass
                new Common.SpellId( "36c8971e91f1745418cc3ffdfac17b74", 6), //blade barrier
                new Common.SpellId( "6a234c6dcde7ae94e94e9c36fd1163a7", 6), //bull strength mass
                new Common.SpellId( "1f6c94d56f178b84ead4c02f1b1e1c48", 6), //cat grace mass
                new Common.SpellId( "7f71a70d822af94458dc1a235507e972", 6), //cloak of dreams
                new Common.SpellId( NewSpells.contingency.AssetGuid, 6),
                new Common.SpellId( NewSpells.curse_major.AssetGuid, 6),
                new Common.SpellId( "f0f761b808dc4b149b08eaf44b99f633", 6), //dispel magic, greater
                new Common.SpellId( "4aa7942c3e62a164387a73184bca3fc1", 6), //disintegrate
                new Common.SpellId( "2caa607eadda4ab44934c5c9875e01bc", 6), //eagles splendor mass
                new Common.SpellId( NewSpells.fluid_form.AssetGuid, 6),
                new Common.SpellId( "2b24159ad9907a8499c2313ba9c0f615", 6), //fox cunning mass
                new Common.SpellId( NewSpells.globe_of_invulnerability.AssetGuid, 6),
                new Common.SpellId( "e15e5e7045fda2244b98c8f010adfe31", 6), //heroism greater
                new Common.SpellId( NewSpells.inflict_pain_mass.AssetGuid, 6),
                new Common.SpellId( "15a04c40f84545949abeedef7279751a", 6), //joyful rapture
                new Common.SpellId( NewSpells.mental_barrier[4].AssetGuid, 6),
                new Common.SpellId( NewSpells.mind_thrust[5].AssetGuid, 6),
                new Common.SpellId( "9f5ada581af3db4419b54db77f44e430", 6), //owls wisdom mass    
                new Common.SpellId( NewSpells.phantom_limbs.AssetGuid, 6),
                new Common.SpellId( "07d577a74441a3a44890e3006efcf604", 6), //primal regression
                new Common.SpellId( NewSpells.psychic_crush[1].AssetGuid, 6),
                new Common.SpellId( NewSpells.psychic_surgery.AssetGuid, 6),
                new Common.SpellId( NewSpells.shadow_enchantment_greater.AssetGuid, 6),
                new Common.SpellId( "766ec978fa993034f86a372c8eb1fc10", 6), //summon huge elemental
                new Common.SpellId( "e740afbab0147944dab35d83faa0ae1c", 6), //summon monster 6
                new Common.SpellId( "27203d62eb3d4184c9aced94f22e1806", 6), //transformation     


                new Common.SpellId( "d361391f645db984bbf58907711a146a", 7), //banishment
                new Common.SpellId( "6f1dcf6cfa92d1948a740195707c0dbe", 7), //finger of death
                new Common.SpellId( NewSpells.fly_mass.AssetGuid, 7),
                new Common.SpellId( NewSpells.hold_person_mass.AssetGuid, 7),
                new Common.SpellId( "2b044152b3620c841badb090e01ed9de", 7), //insanity
                new Common.SpellId( "98310a099009bbd4dbdf66bcef58b4cd", 7), //invisibility mass
                new Common.SpellId( "5c8cde7f0dcec4e49bfa2632dfe2ecc0", 7), //ki shout
                new Common.SpellId( SpiritualWeapons.mages_sword.AssetGuid, 7),
                new Common.SpellId( "df2a0ba6b6dcecf429cbb80a56fee5cf", 7), //mind blank
                new Common.SpellId( NewSpells.particulate_form.AssetGuid, 7),
                new Common.SpellId( "261e1788bfc5ac1419eec68b1d485dbc", 7), //power word blind
                new Common.SpellId( NewSpells.psychic_crush[2].AssetGuid, 7),
                new Common.SpellId( "df7d13c967bce6a40bec3ba7c9f0e64c", 7), //resonating word
                new Common.SpellId( "8eb769e3b583f594faabe1cfdb0bb696", 7), //summon greater elemental
                new Common.SpellId( "ab167fd8203c1314bac6568932f1752f", 7), //sm 7
                new Common.SpellId( NewSpells.synesthesia_mass.AssetGuid, 7),
                new Common.SpellId( "1e2d1489781b10a45a3b70192bba9be3", 7), //waves of ectasy
                new Common.SpellId( "3e4d3b9a5bd03734d9b053b9067c2f38", 7), //waves of exhaustion

                //bilocation ?
                new Common.SpellId( "a5c56f0f699daec44b7aedd8b273b08a", 8), //brilliant inspiration
                new Common.SpellId( "c3d2294a6740bc147870fff652f3ced5", 8), //death clutch
                new Common.SpellId( "740d943e42b60f64a8de74926ba6ddf7", 8), //euphoric tranquility
                new Common.SpellId( "e788b02f8d21014488067bdd3ba7b325", 8), //frightful aspect
                //glimpse of the akashic ?
                new Common.SpellId( NewSpells.iron_body.AssetGuid, 8),
                new Common.SpellId( NewSpells.irresistible_dance.AssetGuid, 8),
                new Common.SpellId( "87a29febd010993419f2a4a9bee11cfc", 8), //mind blank communal
                new Common.SpellId( NewSpells.orb_of_the_void.AssetGuid, 8),
                new Common.SpellId( "f958ef62eea5050418fb92dfa944c631", 8), //power word stun
                new Common.SpellId( "0e67fa8f011662c43934d486acc50253", 8), //prediction of failure
                new Common.SpellId( "42aa71adc7343714fa92e471baa98d42", 8), //protection from spells
                new Common.SpellId( NewSpells.psychic_crush[3].AssetGuid, 8),
                new Common.SpellId( NewSpells.song_of_discord_greater.AssetGuid, 8),
                new Common.SpellId( "fd0d3840c48cafb44bb29e8eb74df204", 8), //shout greater
                new Common.SpellId( "8a7f8c1223bda1541b42fd0320cdbe2b", 8), //summon elder elemental
                new Common.SpellId( "d3ac756a229830243a72e84f3ab050d0", 8), //sm 8
                new Common.SpellId( NewSpells.temporal_stasis.AssetGuid, 8),

                new Common.SpellId( NewSpells.akashic_form.AssetGuid, 9), 
                new Common.SpellId( NewSpells.divide_mind.AssetGuid, 9), //divide mind
                new Common.SpellId( "3c17035ec4717674cae2e841a190e757", 9), //dominate monster
                new Common.SpellId( "1f01a098d737ec6419aedc4e7ad61fdd", 9), //foresight
                new Common.SpellId( "43740dab07286fe4aa00a6ee104ce7c1", 9), //heroic invocation
                new Common.SpellId( NewSpells.hold_monster_mass.AssetGuid, 9),
                new Common.SpellId( "41cf93453b027b94886901dbfc680cb9", 9), //overwhelming presence
                new Common.SpellId( "2f8a67c483dfa0f439b293e094ca9e3c", 9), //power word kill
                new Common.SpellId( NewSpells.psychic_crush[4].AssetGuid, 9),
                new Common.SpellId( NewSpells.mass_suffocation.AssetGuid, 9),
                new Common.SpellId( "52b5df2a97df18242aec67610616ded0", 9), //sm9
                new Common.SpellId( NewSpells.telekinetic_storm.AssetGuid, 9),
                new Common.SpellId( NewSpells.time_stop.AssetGuid, 9),
                new Common.SpellId( "b24583190f36a8442b212e45226c54fc", 9), //wail of banshee
                new Common.SpellId( "870af83be6572594d84d276d7fc583e0", 9), //weird

            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(psychic_spellbook.SpellList, spell_id.level);
            }

            psychic_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.PsychicSpellbook>());

            psychic_spellcasting = Helpers.CreateFeature("PsychichSpellCasting",
                                             "Psychic Magic",
                                             "A psychic can cast any spell she knows without preparing it ahead of time. To learn or cast a spell, a psychic must have an Intelligence score equal to at least 10 + the spell’s level. The saving throw DC against a psychic’s spell is 10 + the spell’s level + the psychic detective’s Intelligence modifier.\n"
                                             + "Psychic spells are not subject to arcane spell failure due to armor, but they require a more significant effort, compared to classic magic and thus the DC of all concentration checks required as a part of casting a psychic spell is increased by 10, additionaly psychic magic can not be used at all if caster is under the influence of fear or negative emotion effects.\n"
                                             + "Some psychic spells can be undercast. This means that the spellcaster can cast the spell at the level that he knows, or as any lower-level version of that spell, using the appropriate spell slot. When a spellcaster undercasts a spell, it is treated exactly like the lower-level version, including when determining its effect, saving throw, and other variables. For example, a psychic spellcaster who adds ego mind trhust III to his list of spells known can cast it as mind thrust I, II, or III. If he casts it as mind thrust I, it is treated in all ways as that spell; it uses the text and the saving throw DC for that spell, and requires him to expend a 1st-level spell slot.\n"
                                             + "Whenever a spontaneous spellcaster adds a spell to his list of spells known that can be undercast, he can immediately learn a spell in place of each lower-level version of that spell he knows. In essence, he loses each earlier version and can replace it with another spell of the same level that is on his spell list.",
                                             "",
                                             null,
                                             FeatureGroup.None);

            psychic_spellcasting.AddComponents(Helpers.Create<SpellFailureMechanics.PsychicSpellbook>(p => p.spellbook = psychic_spellbook), 
                                               Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.NaturalSpell));
            psychic_spellcasting.AddComponent(Helpers.Create<SpellbookMechanics.AddUndercastSpells>(p => p.spellbook = psychic_spellbook));
            psychic_spellcasting.AddComponent(Helpers.CreateAddFact(Investigator.center_self));
            psychic_spellcasting.AddComponents(Common.createCantrips(psychic_class, StatType.Intelligence, psychic_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
            psychic_spellcasting.AddComponents(Helpers.CreateAddFacts(psychic_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));

            return psychic_spellbook;
        }


        static void createPsychicDiscipline(string name, string display_name, string description, UnityEngine.Sprite icon, StatType stat, BlueprintAbility[] spells,
                                            BlueprintFeature feature1, BlueprintFeature feature5, BlueprintFeature feature13)
        {
            var progression = Helpers.CreateProgression(name + "Progression",
                                                         display_name,
                                                         description + "\nPhrenic Pool Ability: " + stat.ToString(),
                                                         "",
                                                         icon,
                                                         FeatureGroup.None,
                                                         Helpers.Create<IncreaseResourceAmountBySharedValue>(i => { i.Resource = phrenic_pool_resource; i.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                                         Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: stat),
                                                         Helpers.Create<RecalculateOnStatChange>(r => r.Stat = stat)
                                                         );

            var extr_spell_list = new Common.ExtraSpellList(spells);
            
            progression.LevelEntries = extr_spell_list.createLearnSpellLevelEntries(name + "DisiciplineSpell",
                                                                                "At 1st level, a psychic learns an additional spell determined by her discipline. She learns another additional spell at 4th level and every 2 levels thereafter, until learning the final one at 18th level.",
                                                                                progression.AssetGuid,
                                                                                new int[] { 1, 4, 6, 8, 10, 12, 14, 16, 18 },
                                                                                psychic_class);
            var learn_spell_features = progression.LevelEntries.Select(le => le.Features[0]).ToArray();
            progression.LevelEntries[0].Features.Add(feature1);
            progression.LevelEntries = progression.LevelEntries.AddToArray(Helpers.LevelEntry(5, feature5), Helpers.LevelEntry(13, feature13));
            progression.UIGroups = new UIGroup[]{Helpers.CreateUIGroup(feature1, feature5, feature13),
                                                 Helpers.CreateUIGroup(learn_spell_features) };
            progression.UIDeterminatorsGroup = new BlueprintFeatureBase[0];
            progression.Classes = getPsychicArray();

            var progression_no_spells = library.CopyAndAdd(progression, "NoSpells" + progression.name, "");
            progression_no_spells.LevelEntries = new LevelEntry[] { Helpers.LevelEntry(1, feature1), Helpers.LevelEntry(5, feature5), Helpers.LevelEntry(13, feature13) };

            psychic_discipline.AllFeatures = psychic_discipline.AllFeatures.AddToArray(progression);
            psychic_discipline_no_spells.AllFeatures = psychic_discipline_no_spells.AllFeatures.AddToArray(progression_no_spells);
            psychic_disiciplines_map.Add(name, progression);
            no_spells_psychic_disiciplines_map.Add(name, progression_no_spells);
        }
    }
}
