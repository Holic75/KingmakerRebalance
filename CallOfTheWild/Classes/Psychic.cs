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
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
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
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
    class Psychic
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass psychic_class;
        static public BlueprintProgression psychic_progression;

        static public BlueprintFeature psychic_proficiencies;
        static public BlueprintFeatureSelection psychic_discipline;

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
        static public BlueprintArchetype mutation_mind;
        static public BlueprintArchetype psychic_marauder;
        static public BlueprintArchetype terror_weaver;
        static public BlueprintArchetype amnesiac;
        static public BlueprintFeature phrenic_mastery;


        static Dictionary<string, BlueprintProgression> psychic_disiciplines_map = new Dictionary<string, BlueprintProgression>();


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
            psychic_class.PrimaryColor = sorceror_class.PrimaryColor;
            psychic_class.SecondaryColor = sorceror_class.SecondaryColor;
            psychic_class.RecommendedAttributes = wizard_class.RecommendedAttributes;
            psychic_class.NotRecommendedAttributes = wizard_class.NotRecommendedAttributes;
            psychic_class.EquipmentEntities = sorceror_class.EquipmentEntities;
            psychic_class.MaleEquipmentEntities = sorceror_class.MaleEquipmentEntities;
            psychic_class.FemaleEquipmentEntities = sorceror_class.FemaleEquipmentEntities;
            psychic_class.ComponentsArray = wizard_class.ComponentsArray;
            psychic_class.StartingItems = new Kingmaker.Blueprints.Items.BlueprintItem[] {library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("511c97c1ea111444aa186b1a58496664"), //crossbow
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("ada85dae8d12eda4bbe6747bb8b5883c"), // quarterstaff
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("63caf94a780472b448f50d0bc183c38f"), //s. magic missile
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("63caf94a780472b448f50d0bc183c38f"), //s. magic missile
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("e8308a74821762e49bc3211358e81016"), //s. mage armor
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("f79c3fd5012a3534c8ab36dc18e85fb1") //s. sleep
                                                                                       };
            createPsychicProgression();
            psychic_class.Progression = psychic_progression;
           
            psychic_class.Archetypes = new BlueprintArchetype[] { };
            Helpers.RegisterClass(psychic_class);
            //createPsychicFeats
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

            psychic_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { psychic_spellcasting, psychic_proficiencies, psychic_discipline};
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
            createAbominationDiscipline();
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
                                         bleed1d6.GetComponent<AddHealTrigger>()
                                         );

            var buff = Helpers.CreateBuff("DarkHalfBuff",
                                          "Dark Half",
                                          "By allowing the dark forces to overcome you, You can enter a state of instinctual cruelty as a swift action.\n"
                                          + "While you’re manifesting your dark half, you increase the DCs of your psychic spells by 1, gain a +2 morale bonus on Will saves, and become immune to fear effects. Whenever you cast a spell that deals damage while manifesting your dark half, you can cause any creature that took damage from the spell to also take 1 point of bleed damage. The amount of bleed damage increases to 2 points at 5th level and to 3 points at 13th level. While manifesting your dark half, You can’t use any Charisma-, Dexterity-, or Intelligence-based skills (except Mobility and Intimidate) or any ability that requires patience or concentration other than casting spells using psychic magic, using phrenic amplifications, or attempting to return to normal. You can attempt to return to your normal self as a free action, but must succeed at an Intelligence check with a DC equal to 10. If you fail, you continue to manifest your dark half and can’t attempt to change back for 1 round.\n"
                                          + "You can manifest your dark half for a number of rounds per day equal to 3 + 1/2 your psychic level + your Charisma modifier; when these rounds are expended, you return to your normal self without requiring a concentration check.",
                                          "",
                                          Helpers.GetIcon("da8ce41ac3cd74742b80984ccc3c9613"), //rage
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
                                          Helpers.CreateAddFactContextActions(activated: new GameAction[] {Common.createContextActionRandomize(apply_morphic_form_buff1, apply_morphic_form_buff2, apply_morphic_form_buff3),
                                                                                                           apply_psychic_safeguard_buff},
                                                                              newRound: new GameAction[] {Helpers.CreateConditional(Helpers.Create<BuffConditionCheckRoundNumber>(b => b.RoundNumber = 1),
                                                                                                                                    null,
                                                                                                                                    Helpers.CreateConditional(Helpers.Create<ResourceMechanics.ContextConditionTargetHasEnoughResource>(c => c.resource = resource),
                                                                                                                                                              Helpers.Create<NewMechanics.ContextActionSpendResource>(c => c.resource = resource),
                                                                                                                                                              Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                                                              )
                                                                                                                                    )
                                                                                                          }
                                                                              ),
                                          resource.CreateResourceLogic()
                                          );

            var deactivate_buff = Helpers.CreateBuff("DeactivateDarkHalfBuff",
                                                     "Deactivate Dark Half Cooldown",
                                                     buff.Description,
                                                     "",
                                                     Helpers.GetIcon("d316d3d94d20c674db2c24d7de96f6a7"), //serenity
                                                     null);

            var activate_dark_half = Helpers.CreateAbility("DarkHalfAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           AbilityType.Supernatural,
                                                           CommandType.Swift,
                                                           AbilityRange.Personal,
                                                           "",
                                                           "",
                                                           Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)),
                                                           Common.createAbilityCasterHasNoFacts(buff, deactivate_buff)
                                                           );
            activate_dark_half.setMiscAbilityParametersSelfOnly();

            var deactivate_dark_half = Helpers.CreateAbility("DeactivateDarkHalfAbility",
                                                             "Deactivate Dark Half",
                                                               buff.Description,
                                                               "",
                                                               deactivate_buff.Icon,
                                                               AbilityType.Supernatural,
                                                               CommandType.Free,
                                                               AbilityRange.Personal,
                                                               "",
                                                               "",
                                                               Helpers.CreateRunActions(Common.createContextActionSkillCheck(StatType.Intelligence, 
                                                                                                                             Common.createContextActionRemoveBuff(buff),
                                                                                                                             Common.createContextActionApplyBuff(deactivate_buff, Helpers.CreateContextDuration(1, DurationRate.Rounds), dispellable: false),
                                                                                                                             10
                                                                                                                             )
                                                                                        ),
                                                               Common.createAbilityCasterHasNoFacts(deactivate_buff),
                                                               Common.createAbilityCasterHasFacts(buff)
                                                               );
            deactivate_dark_half.setMiscAbilityParametersSelfOnly();

            var wrapper = Common.createVariantWrapper("DarkHalfWrapperAbility", "", activate_dark_half, deactivate_dark_half);

            var dark_half = Common.AbilityToFeature(wrapper, false);

            createPsychicDiscipline("Abomination",
                                    "Abomination",
                                    "Your mind is impure, tainted by outside forces. These might be monstrous ancestors whose blood still flows within you, or powerful and unknowable psychic forces that intrude upon your mind. Like a psychic disease, this influence consumes part of your brain, creating a dark counterpart to your normal self. Every time you call forth a psychic spell, You’re drawing on this dangerous force—and potentially giving it a greater hold on you. This malign influence might stem from creatures like rakshasas and aboleths, or perhaps malign entities that dwell in the voids between the stars.",
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
                                                   FeatureGroup.None,
                                                   Helpers.CreateAddAbilityResource(phrenic_pool_resource)
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
                new Common.SpellId( "39a602aa80cc96f4597778b6d4d49c0a", 1), //flare burst
                new Common.SpellId( "88367310478c10b47903463c5d0152b0", 1), //hypnotism
                new Common.SpellId( Witch.ill_omen.AssetGuid, 1),
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
                new Common.SpellId( "f0455c9295b53904f9e02fc571dd2ce1", 2), //owl's wisdom
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( "1724061e89c667045a6891179ee2e8e7", 2), //summon monster 2
                new Common.SpellId( NewSpells.thought_shield[0].AssetGuid, 2),

                new Common.SpellId( "0a2f7c6aa81bc6548ac7780d8b70bcbc", 3), //battering blast (it seems it should be on the list since all force spells are there)
                new Common.SpellId( NewSpells.countless_eyes.AssetGuid, 3),
                new Common.SpellId( "7658b74f626c56a49939d9c20580885e", 3), //deep slumber
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //displacement
                new Common.SpellId( NewSpells.fly.AssetGuid, 3),
                new Common.SpellId( "486eaff58293f6441a5c2759c4872f98", 3), //haste
                new Common.SpellId( "5ab0d42fb68c9e34abae4921822b9d63", 3), //heroism
                new Common.SpellId( NewSpells.mental_barrier[1].AssetGuid, 3),
                new Common.SpellId( NewSpells.mind_thrust[2].AssetGuid, 3),
                new Common.SpellId( "96c9d98b6a9a7c249b6c4572e4977157", 3), //protection from arrows communal
                new Common.SpellId( "d2f116cfe05fcdd4a94e80143b67046f", 3), //protection from energy
                new Common.SpellId( "97b991256e43bb140b263c326f690ce2", 3), //rage
                new Common.SpellId( "7bb0c402f7f789d4d9fae8ca87b4c7e2", 3), //resist energy communal
                new Common.SpellId( NewSpells.resinous_skin.AssetGuid, 3),
                new Common.SpellId( NewSpells.sands_of_time.AssetGuid, 3),
                new Common.SpellId( "f492622e473d34747806bdb39356eb89", 3), //slow
                new Common.SpellId( NewSpells.stunning_barrier_greater.AssetGuid, 3),
                new Common.SpellId( NewSpells.synesthesia.AssetGuid, 3),
                new Common.SpellId( NewSpells.thought_shield[1].AssetGuid, 3),
                new Common.SpellId( "8a28a811ca5d20d49a863e832c31cce1", 3), //vampyric touch
                new Common.SpellId( NewSpells.wall_of_nausea.AssetGuid, 3),
                new Common.SpellId( "5d61dde0020bbf54ba1521f7ca0229dc", 3), //summon monster 3

                new Common.SpellId( NewSpells.aura_of_doom.AssetGuid, 4),
                new Common.SpellId( "7792da00c85b9e042a0fdfc2b66ec9a8", 4), //break enchantment
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 4), //confusion
                new Common.SpellId( "4baf4109145de4345861fe0f2209d903", 4), //crushing despair
                new Common.SpellId( "4a648b57935a59547b7a2ee86fb4f26a", 4), //dimensions door
                new Common.SpellId( "754c478a2aa9bb54d809e648c3f7ac0e", 4), //dominate animal
                new Common.SpellId( "66dc49bf154863148bd217287079245e", 4), //enlarge person mass
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life, greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( NewSpells.fleshworm_infestation.AssetGuid, 4),
                new Common.SpellId( "4c349361d720e844e846ad8c19959b1e", 4), //freedom of movement
                new Common.SpellId( NewSpells.intellect_fortress.AssetGuid, 4),
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
                new Common.SpellId( "630c8b85d9f07a64f917d79cb5905741", 5), //summon monster 5
                new Common.SpellId( NewSpells.mental_barrier[3].AssetGuid, 5),
                new Common.SpellId( NewSpells.psychic_crush[0].AssetGuid, 5),
                new Common.SpellId( "4cf3d0fae3239ec478f51e86f49161cb", 5), //true seeing
                new Common.SpellId( NewSpells.suffocation.AssetGuid, 5),
                new Common.SpellId( NewSpells.synapse_overload.AssetGuid, 5),
                new Common.SpellId( "8878d0c46dfbd564e9d5756349d5e439", 5), //waves of fatigue
                
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
                new Common.SpellId( "e15e5e7045fda2244b98c8f010adfe31", 6), //heroism greater
                new Common.SpellId( NewSpells.inflict_pain_mass.AssetGuid, 6),
                new Common.SpellId( "15a04c40f84545949abeedef7279751a", 6), //joyful rapture
                new Common.SpellId( NewSpells.mental_barrier[4].AssetGuid, 6),
                new Common.SpellId( NewSpells.mind_thrust[5].AssetGuid, 6),
                new Common.SpellId( "9f5ada581af3db4419b54db77f44e430", 6), //owls wisdom mass    
                new Common.SpellId( "07d577a74441a3a44890e3006efcf604", 6), //primal regression
                new Common.SpellId( NewSpells.psychic_crush[1].AssetGuid, 6),
                new Common.SpellId( NewSpells.psychic_surgery.AssetGuid, 6),
                new Common.SpellId( "e740afbab0147944dab35d83faa0ae1c", 6), //summon monster 6
                new Common.SpellId( "27203d62eb3d4184c9aced94f22e1806", 6), //transformation     


                new Common.SpellId( "d361391f645db984bbf58907711a146a", 7), //banishment
                new Common.SpellId( "6f1dcf6cfa92d1948a740195707c0dbe", 7), //finger of death
                new Common.SpellId( NewSpells.fly_mass.AssetGuid, 7),
                new Common.SpellId( NewSpells.hold_person_mass.AssetGuid, 7),
                new Common.SpellId( "2b044152b3620c841badb090e01ed9de", 7), //insanity
                new Common.SpellId( "98310a099009bbd4dbdf66bcef58b4cd", 7), //invisibility mass
                new Common.SpellId( "5c8cde7f0dcec4e49bfa2632dfe2ecc0", 7), //ki shout
                new Common.SpellId( "df2a0ba6b6dcecf429cbb80a56fee5cf", 7), //mind blank
                new Common.SpellId( NewSpells.particulate_form.AssetGuid, 7),
                new Common.SpellId( "261e1788bfc5ac1419eec68b1d485dbc", 7), //power word blind
                new Common.SpellId( NewSpells.psychic_crush[2].AssetGuid, 7),
                new Common.SpellId( "df7d13c967bce6a40bec3ba7c9f0e64c", 7), //resonating word
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
                new Common.SpellId( "fd0d3840c48cafb44bb29e8eb74df204", 8), //shout greater
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
                                             + "Psychic spells are not subject to arcane spell failure due to armor, but they require a more significant effort, compared to classic magic and thus the DC of all concentration checks required as a part of casting a psychic spell is increased by 10, additionaly psychic magic can not be used at all if caster is under the influence of fear or negative emotion effects.",
                                             "",
                                             null,
                                             FeatureGroup.None);

            psychic_spellcasting.AddComponent(Helpers.Create<SpellFailureMechanics.PsychicSpellbook>(p => p.spellbook = psychic_spellbook));
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
            progression.LevelEntries = progression.LevelEntries.AddToArray(Helpers.LevelEntry(1, feature1), Helpers.LevelEntry(5, feature5), Helpers.LevelEntry(13, feature13));
            progression.UIGroups = new UIGroup[]{Helpers.CreateUIGroup(feature1, feature5, feature13),
                                                 Helpers.CreateUIGroup(learn_spell_features) };
            progression.UIDeterminatorsGroup = new BlueprintFeatureBase[0];
            progression.Classes = getPsychicArray();

            psychic_discipline.AllFeatures = psychic_discipline.AllFeatures.AddToArray(progression);
            psychic_disiciplines_map.Add(name, progression);
        }
    }
}
