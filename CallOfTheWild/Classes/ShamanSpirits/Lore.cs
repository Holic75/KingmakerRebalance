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
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
    partial class SpiritsEngine
    {
        public class LoreSpirit
        {
            public BlueprintFeature spirit_ability;
            public BlueprintFeature greater_spirit_ability;
            public BlueprintFeature greater_spirit_ability_wandering;
            public BlueprintFeature true_spirit_ability;
            public BlueprintFeature manifestation;
            public BlueprintFeature benefit_of_wisdom;
            public BlueprintFeature brain_drain;
            public BlueprintFeature confusion_curse;
            public BlueprintFeature mental_acuity;
            public BlueprintAbility[] spells;
            public BlueprintFeature[] hexes;
            public StatType primary_stat;
            public StatType secondary_stat;

            HexEngine hex_engine;
            string prefix;
            string spell_list_prefix;
            bool test_mode;


            public Oracle.Spirit createOracleSpirit(HexEngine associated_hex_engine, string asset_prefix, string spell_list_asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                spell_list_prefix = spell_list_asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_stat;

                createHexes();
                createSpiritAbility();
                createGreaterSpiritAbility(true, 15);

                spells = new BlueprintAbility[9]
                {
                    library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00"), //true strike
                    library.Get<BlueprintAbility>("f0455c9295b53904f9e02fc571dd2ce1"), //owls wisdom
                    library.Get<BlueprintAbility>("1a045f845778dc54db1c2be33a8c3c0a"), //see invisibility communal
                    library.Get<BlueprintAbility>("e9cc9378fd6841f48ad59384e79e9953"), //death ward
                    library.Get<BlueprintAbility>("4cf3d0fae3239ec478f51e86f49161cb"), //true seeing
                    library.Get<BlueprintAbility>("9f5ada581af3db4419b54db77f44e430"), //owls wisdom mass
                    library.Get<BlueprintAbility>("fafd77c6bfa85c04ba31fdc1c962c914"), //greater restoration
                    library.Get<BlueprintAbility>("0e67fa8f011662c43934d486acc50253"), //prediction of failure
                    NewSpells.time_stop
                };

                return new Oracle.Spirit("Lore",
                                         "Lore",
                                         "A shaman who selects the lore spirit appears far wiser and knowing that her age would suggest. Though she can seem unassuming, her eyes give the impression she is peering deep into all she looks at, seeing the secrets of the essential merely by concentrating.",
                                         LoadIcons.Image2Sprite.Create(@"AbilityIcons/Wish.png"),
                                         "",
                                         spirit_ability,
                                         greater_spirit_ability,
                                         spells,
                                         hexes
                                         );
            }


            public Archetypes.SpiritWhisperer.Spirit createSpiritWhispererSpirit(HexEngine associated_hex_engine, string asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                spell_list_prefix = "";
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_secondary_stat;

                createHexes();
                createSpiritAbility();
                createTrueSpiritAbility();
                createManifestation();

                return new Archetypes.SpiritWhisperer.Spirit("Lore",
                                                          "Lore",
                                                          "A shaman who selects the lore spirit appears far wiser and knowing that her age would suggest. Though she can seem unassuming, her eyes give the impression she is peering deep into all she looks at, seeing the secrets of the essential merely by concentrating.",
                                                          manifestation.Icon,
                                                          "",
                                                          spirit_ability,
                                                          true_spirit_ability,
                                                          manifestation,
                                                          hexes);
            }

            public Shaman.Spirit createShamanSpirit(HexEngine associated_hex_engine, string asset_prefix, string spell_list_asset_prefix, bool test = false)
            {
                test_mode = test;
                hex_engine = associated_hex_engine;
                prefix = asset_prefix;
                spell_list_prefix = spell_list_asset_prefix;
                primary_stat = hex_engine.hex_stat;
                secondary_stat = hex_engine.hex_secondary_stat;

                createHexes();
                createSpiritAbility();
                createGreaterSpiritAbility(true);
                createTrueSpiritAbility();
                createManifestation();

                spells = new BlueprintAbility[9]
                {
                    library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00"), //true strike
                    library.Get<BlueprintAbility>("f0455c9295b53904f9e02fc571dd2ce1"), //owls wisdom
                    library.Get<BlueprintAbility>("1a045f845778dc54db1c2be33a8c3c0a"), //see invisibility communal
                    library.Get<BlueprintAbility>("e9cc9378fd6841f48ad59384e79e9953"), //death ward
                    library.Get<BlueprintAbility>("4cf3d0fae3239ec478f51e86f49161cb"), //true seeing
                    library.Get<BlueprintAbility>("9f5ada581af3db4419b54db77f44e430"), //owls wisdom mass
                    library.Get<BlueprintAbility>("fafd77c6bfa85c04ba31fdc1c962c914"), //greater restoration
                    library.Get<BlueprintAbility>("0e67fa8f011662c43934d486acc50253"), //prediction of failure
                    NewSpells.time_stop
                };

                return new Shaman.Spirit("Lore",
                                  "Lore",
                                  "A shaman who selects the lore spirit appears far wiser and knowing that her age would suggest. Though she can seem unassuming, her eyes give the impression she is peering deep into all she looks at, seeing the secrets of the essential merely by concentrating.",
                                  manifestation.Icon,
                                  "",
                                  new BlueprintFeature[] {spirit_ability, spirit_ability },
                                  new BlueprintFeature[] { greater_spirit_ability, greater_spirit_ability_wandering },
                                  new BlueprintFeature[] { true_spirit_ability, true_spirit_ability },
                                  manifestation,
                                  hexes,
                                  spells);
            }

            void createHexes()
            {
                benefit_of_wisdom = hex_engine.createBenefitOfWisdom(prefix + "BenefitOfWisdom",
                                                                      "Benefit of Wisdom",
                                                                      "The shaman relies on wisdom rather than intellect to gain and retain knowledge. She can use her Wisdom modifier instead of her Intelligence modifier on all Intelligence-based skill checks."
                                                                      );
                brain_drain = hex_engine.createBrainDrain(prefix + "BrainDrain",
                                                          "Brain Drain",
                                                          $"As a standard action, the shaman violently probes the mind of a single intelligent enemy within 30 feet. The target can attempt a Will saving throw to negate the effect. If it succeeds, it immediately knows the source of the mental prying; otherwise, it’s wracked with pain and takes 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of damage for every 2 levels the shaman possesses.\n"
                                                          + "This is a mind-affecting effect. Once she successfully affects a creature, she cannot use this hex on that creature again for 24 hours."
                                                          );

                confusion_curse = hex_engine.createConfusionCurse(prefix + "ConfusionCurse",
                                                                  "Confusion Curse ",
                                                                  "The shaman’s command of lore can cause weaker minds to become mired in confusion. The shaman chooses a single intelligent target within 30 feet. That creature must succeed at a Will saving throw or become confused for a number of rounds equal to the shaman’s Charisma modifier (minimum 1). Once affected by this hex, the creature cannot be the target of this hex again for 24 hours."
                                                                  );

                mental_acuity = hex_engine.createMentalAcuity(prefix + "MentalAcuity",
                                                              "Mental Acuity",
                                                              "Shaman receives +1 inherent bonus to her intelligence. This bonus increases by 1 for every 5 shaman levels."
                                                             );

                hexes = new BlueprintFeature[]
                {
                    benefit_of_wisdom,
                    brain_drain,
                    confusion_curse,
                    mental_acuity,
                };
            }


            void createSpiritAbility()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "MonstrousInsightResource", "", "", "", null);
                resource.SetIncreasedByStat(3, secondary_stat);
                var icon = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e").Icon; //detect magic

                var lore_action = Helpers.Create<NewMechanics.MonsterLore.ContextMonsterLoreCheck>(c =>
                                                                                                    {
                                                                                                        c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                        c.descriptor = ModifierDescriptor.Insight;
                                                                                                    }
                                                                                                  );

                var buff = Helpers.CreateBuff(prefix + "MonstrousInsightBuff",
                                                "Monstrous Insight",
                                                "The shaman can identify creatures and gain insight into their strengths and weaknesses. As a standard action, the shaman can attempt a Knowledge skill check to identify a creature and its abilities (using the appropriate skill for the monster’s type) with an insight bonus equal to her shaman level. Whether or not the check is successful, she also gains a +2 insight bonus for 1 minute on attack rolls made against that creature and a +2 insight bonus to her AC against attacks made by that creature. The shaman can use this ability a number of times per day equal to 3 + her Charisma modifier.",
                                                "",
                                                icon,
                                                null,
                                                Helpers.Create<AttackBonusAgainstTarget>(c => { c.Value = Common.createSimpleContextValue(2); c.CheckCaster = true; }),
                                                Helpers.Create<ACBonusAgainstTarget>(c => { c.Value = Common.createSimpleContextValue(2); c.CheckCaster = true; c.Descriptor = ModifierDescriptor.Insight; })
                                                );
                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
                var ability = Helpers.CreateAbility(prefix + "MonstrousInsightAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    Helpers.oneMinuteDuration,
                                                    Helpers.savingThrowNone,
                                                    Helpers.CreateDeliverTouch(),
                                                    Helpers.CreateRunActions(lore_action, apply_buff),
                                                    Helpers.Create<NewMechanics.MonsterLore.AbilityTargetCanBeInspected>(a => a.allow_reinspect = true),
                                                    Common.createAbilitySpawnFx("749bb96fb50ee5b4685645472d718465", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_engine.hex_classes)
                                                                                    
                                                    );
                ability.setMiscAbilityParametersTouchHarmful();
                var sticky_touch = Helpers.CreateTouchSpellCast(ability, resource);

                sticky_touch.AddComponent(Helpers.Create<NewMechanics.MonsterLore.AbilityTargetCanBeInspected>(a => a.allow_reinspect = true));

                spirit_ability = Common.AbilityToFeature(sticky_touch, hide: false);
                spirit_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));
            }


            void createGreaterSpiritAbility(bool need_secondary, int start_level = 8)
            {
                var icon = library.Get<BlueprintAbility>("f2115ac1148256b4ba20788f7e966830").Icon; //restoration
                BlueprintFeatureSelection learn_selection = Helpers.CreateFeatureSelection(prefix + "ArcaneEnlightenmentFeatureSelection",
                                                                                          "Arcane Enlightenment",
                                                                                          "The shaman’s native intelligence grants her the ability to tap into arcane lore. The shaman can add a spell from the sorcerer/wizard spell list to the list of shaman spells she can prepare.\n"
                                                                                          + "To select a spell she needs to have both Intelligence and Charisma scores equal to 10 + the spell's level. She can add an additional spell every two levels thereafter.",
                                                                                          "",
                                                                                          icon,
                                                                                          FeatureGroup.None);

                var wizard_spell_list = Common.combineSpellLists(spell_list_prefix + "LoreSpiritWizardSpellList", library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89"));
                Common.excludeSpellsFromList(wizard_spell_list, hex_engine.hex_classes[0].Spellbook.SpellList);
                for (int i = 1; i <= 9; i++)
                {
                    var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956", prefix + $"ArcaneEnlightenment{i}ParametrizedFeature", "");
                    learn_spell.SpellLevel = i;
                    learn_spell.SpecificSpellLevel = true;
                    learn_spell.SpellLevelPenalty = 0;
                    learn_spell.SpellcasterClass = hex_engine.hex_classes[0];
                    learn_spell.SpellList = wizard_spell_list;
                    learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = wizard_spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = hex_engine.hex_classes[0]; });
                    learn_spell.AddComponents(Helpers.PrerequisiteStatValue(StatType.Intelligence, 10 + i),
                                              Helpers.PrerequisiteStatValue(secondary_stat, 10 + i),
                                              Common.createPrerequisiteClassSpellLevel(hex_engine.hex_classes[0], i)
                                              );
                    learn_spell.SetName(Helpers.CreateString(prefix + $"ArcaneEnlightenmentParametrizedFeature{i + 1}.Name", "Arcane Enlightenment " + $"(Level {i})"));
                    learn_spell.SetDescription(learn_selection.Description);
                    learn_spell.SetIcon(learn_selection.Icon);

                    learn_selection.AllFeatures = learn_selection.AllFeatures.AddToArray(learn_spell);
                }

                greater_spirit_ability = Helpers.CreateProgression(prefix + "ArcaneEnlightenmentProgression",
                                                                   learn_selection.Name,
                                                                   learn_selection.Description,
                                                                   "",
                                                                   learn_selection.Icon,
                                                                   FeatureGroup.None);
        
                var entries = new List<LevelEntry>();
                for (int i = start_level; i <= 20; i = i + 2)
                {
                    entries.Add(Helpers.LevelEntry(i, learn_selection));
                }
                (greater_spirit_ability as BlueprintProgression).LevelEntries = entries.ToArray();

                entries = new List<LevelEntry>();
                for (int i = start_level + 4; i <= 20; i = i + 2)
                {
                    entries.Add(Helpers.LevelEntry(i, learn_selection));
                }

                if (need_secondary)
                {
                    greater_spirit_ability_wandering = library.CopyAndAdd<BlueprintProgression>(greater_spirit_ability.AssetGuid, prefix + "ArcaneEnlightenmentWanderingFeatureSelection", "");
                    (greater_spirit_ability_wandering as BlueprintProgression).LevelEntries = entries.ToArray();
                }
            }


            void createTrueSpiritAbility()
            {
                var icon = library.Get<BlueprintActivatableAbility>("1f7e326d3a88fd84985a60e416388c27").Icon; //judgement resilence
                true_spirit_ability = Helpers.CreateFeature(prefix + "PerfectKnowledgeFeature",
                                                            "Perfect Knowledge",
                                                            "Shaman gains a +10 competence bonus on all Lore and Knowledge checks.",
                                                            "",
                                                            icon,
                                                            FeatureGroup.None,
                                                            Helpers.CreateAddStatBonus(StatType.SkillLoreNature, 10, ModifierDescriptor.Competence),
                                                            Helpers.CreateAddStatBonus(StatType.SkillLoreReligion, 10, ModifierDescriptor.Competence),
                                                            Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, 10, ModifierDescriptor.Competence),
                                                            Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, 10, ModifierDescriptor.Competence)
                                                            );
            }


            void createManifestation()
            {
                var resource = Helpers.CreateAbilityResource(prefix +"LoreManifestationResource", "", "", "", null);
                resource.SetFixedResource(1);
                var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/Wish.png");
                var skills = new StatType[] { StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion };

                var wish_variants = NewSpells.createWishSpellLevelVariants(prefix + "LoreManifestationWish",
                                                                           "Manifestation: Wish",
                                                                           "This ability allows you to cast any sorcerer/wizard spell of 8th level or lower, or any other spell of 7th level or lower. You can not cast spells requiring expensive material components.",
                                                                           icon,
                                                                           library.Get<BlueprintSpellbook>("5a38c9ac8607890409fcb8f6342da6f4"), //wizard
                                                                           CommandType.Standard,
                                                                           AbilityType.SpellLike,
                                                                           allow_metamagic: false,
                                                                           allow_spells_with_material_components: false,
                                                                           resource: resource,
                                                                           additional_components: new BlueprintComponent[] {Common.createContextCalculateAbilityParamsBasedOnClasses(hex_engine.hex_classes, primary_stat)}
                                                                           );

                manifestation = Helpers.CreateFeature(prefix + "LoreManifestationFeature",
                                                      "Manifestation",
                                                      "Upon reaching 20th level, the shaman becomes an unending font of knowledge and lore. She can take 20 on all Knowledge skill checks, including those she isn’t trained in. Her understanding of the fundamental underpinnings of reality has also become so advanced that she can cast wish once per day. This doesn’t require a material component, but the wish cannot be used to grant ability score bonuses or replicate spells with expensive material components.",
                                                      "",
                                                      icon,
                                                      FeatureGroup.None,
                                                      Helpers.Create<ModifyD20>(m => { m.Replace = true; m.SpecificSkill = true; m.Rule = RuleType.SkillCheck; m.Skill = skills; m.Roll = 20; }),
                                                      Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.MakeKnowledgeCheckUntrained),
                                                      Helpers.CreateAddFacts(wish_variants),
                                                      Helpers.CreateAddAbilityResource(resource)
                                                      );
            }

        }
    }
}
