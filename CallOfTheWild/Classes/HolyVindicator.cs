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
    public class HolyVindicator
    {
        static internal bool test_mode;
        static LibraryScriptableObject library => Main.library;

        static public BlueprintCharacterClass holy_vindicator_class;
        static public BlueprintProgression holy_vindicator_progression;

        static public BlueprintFeature holy_vindicator_proficiencies;
        static public BlueprintFeatureSelection spellbook_selection;
        static public BlueprintFeature vindicator_shield;
        static public BlueprintFeature channel_energy_progression;
        static public BlueprintFeature stigmata;
        static public BlueprintFeature stigmata_move;
        static public BlueprintFeature stigmata_swift;

        static public Dictionary<ModifierDescriptor, List<BlueprintActivatableAbility>> stigmata_abilities = new Dictionary<ModifierDescriptor, List<BlueprintActivatableAbility>>();
        static public Dictionary<ModifierDescriptor, List<BlueprintBuff>> stigmata_buffs = new Dictionary<ModifierDescriptor, List<BlueprintBuff>>();
        static public BlueprintFeature faith_healing;
        static public BlueprintFeature divine_wrath;
        static public BlueprintFeature divine_retribution;
        static public BlueprintFeature versatile_channel;
        static public BlueprintFeature bloodfire;
        static public BlueprintFeature bloodrain;



        internal static void createHolyVindicatorClass()
        {
            Main.logger.Log("Holy Vindicator class test mode: " + test_mode.ToString());
            var paladin_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");

            var savesPrestigeLow = library.Get<BlueprintStatProgression>("dc5257e1100ad0d48b8f3b9798421c72");
            var savesPrestigeHigh = library.Get<BlueprintStatProgression>("1f309006cd2855e4e91a6c3707f3f700");

            holy_vindicator_class = Helpers.Create<BlueprintCharacterClass>();
            holy_vindicator_class.name = "HolyVindicatorClass";
            library.AddAsset(holy_vindicator_class, "");

            holy_vindicator_class.LocalizedName = Helpers.CreateString("HolyVindicator.Name", "Holy Vindicator");
            holy_vindicator_class.LocalizedDescription = Helpers.CreateString("Holy Vindicator",
                "Many faiths have within their membership an order of the church militant, be they holy knights or dark warriors, who put their lives and immortal souls on the line for their faith. They are paragons of battle, eschewing sermons for steel. These men and women are living conduits of divine power, down to their very blood, which they happily shed in a moment if it brings greater glory to their deity or judgment upon heretics, infidels, and all enemies of the faith. Holy vindicators are usually clerics or fighter/clerics, though many paladins (or even paladin/clerics) are drawn to this class as well. In all cases, the class offers a further opportunity to fuse and refine their martial and ministerial powers and role. "
                + "The holy vindicator has substantial spellcasting ability, though not so much as a focused cleric or paladin. His combat skills are considerable and his healing powers prodigious, and those whose religious views align well with the vindicator will find a ready ally."
                );

            holy_vindicator_class.m_Icon = paladin_class.Icon;
            holy_vindicator_class.SkillPoints = paladin_class.SkillPoints;
            holy_vindicator_class.HitDie = DiceType.D10;
            holy_vindicator_class.BaseAttackBonus = paladin_class.BaseAttackBonus;
            holy_vindicator_class.FortitudeSave = savesPrestigeHigh;
            holy_vindicator_class.ReflexSave = savesPrestigeLow;
            holy_vindicator_class.WillSave = savesPrestigeHigh;
            holy_vindicator_class.ClassSkills = new StatType[] { StatType.SkillAthletics, StatType.SkillLoreReligion, StatType.SkillKnowledgeArcana, StatType.SkillPersuasion };
            holy_vindicator_class.IsDivineCaster = true;
            holy_vindicator_class.IsArcaneCaster = false;
            holy_vindicator_class.PrestigeClass = true;
            holy_vindicator_class.StartingGold = paladin_class.StartingGold;
            holy_vindicator_class.PrimaryColor = paladin_class.PrimaryColor;
            holy_vindicator_class.SecondaryColor = paladin_class.SecondaryColor;
            holy_vindicator_class.RecommendedAttributes = new StatType[] { StatType.Strength };
            holy_vindicator_class.EquipmentEntities = paladin_class.EquipmentEntities;
            holy_vindicator_class.MaleEquipmentEntities = paladin_class.MaleEquipmentEntities;
            holy_vindicator_class.FemaleEquipmentEntities = paladin_class.FemaleEquipmentEntities;
            holy_vindicator_class.StartingItems = paladin_class.StartingItems;

            holy_vindicator_class.ComponentsArray = new BlueprintComponent[]{paladin_class.ComponentsArray[0], paladin_class.ComponentsArray[2] };//no atheism
            holy_vindicator_class.AddComponent(Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 5));
            holy_vindicator_class.AddComponent(Helpers.PrerequisiteStatValue(StatType.SkillLoreReligion, 5));
            holy_vindicator_class.AddComponent(Helpers.PrerequisiteFeature(ChannelEnergyEngine.improved_channel)); //instead of channel energy and Alignment/Elemental channel which are not in the game and imho do not worth implementing 
            holy_vindicator_class.AddComponent(Helpers.Create<SpellbookMechanics.PrerequisiteDivineCasterTypeSpellLevel>(p => p.RequiredSpellLevel = 1));

            holy_vindicator_class.AddComponent(Helpers.Create<SkipLevelsForSpellProgression>(s => s.Levels = new int[] { 5, 9 }));

            createHolyVindicatorProgression();
            holy_vindicator_class.Progression = holy_vindicator_progression;

            Helpers.RegisterClass(holy_vindicator_class);
        }

        static void createHolyVindicatorProgression()
        {
            createHolyVindicatorProficiencies();
            createVindicatorShield();
            createChannelEnergyProgression();
            createSpellbookSelection();

            createStigmata();
            createFaithHealing();
            createBloodfireAndBloodrain();
            createVersatileChannel();

            var channel_smite = Helpers.CreateFeature("HolyVindicatorChannelSmite",
                                                      "Channel Smite",
                                                      "At 5th level, a vindicator gains Channel Smite as a bonus feat.",
                                                      "",
                                                      ChannelEnergyEngine.channel_smite.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(ChannelEnergyEngine.channel_smite)
                                                      );

            holy_vindicator_progression = Helpers.CreateProgression("HolyVindicatorProgression",
                                                   holy_vindicator_class.Name,
                                                   holy_vindicator_class.Description,
                                                   "",
                                                   holy_vindicator_class.Icon,
                                                   FeatureGroup.None);
            holy_vindicator_progression.Classes = getHolyVindicatorArray();

            holy_vindicator_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, channel_energy_progression, holy_vindicator_proficiencies, vindicator_shield),
                                                                          Helpers.LevelEntry(2, spellbook_selection, stigmata),
                                                                          Helpers.LevelEntry(3, faith_healing),
                                                                          Helpers.LevelEntry(4),
                                                                          Helpers.LevelEntry(5, bloodfire, channel_smite),
                                                                          Helpers.LevelEntry(6, versatile_channel, stigmata_move),
                                                                          Helpers.LevelEntry(7),
                                                                          Helpers.LevelEntry(8),
                                                                          Helpers.LevelEntry(9, bloodrain),
                                                                          Helpers.LevelEntry(10, stigmata_swift)
                                                                        };

            holy_vindicator_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { holy_vindicator_proficiencies };
            holy_vindicator_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(channel_energy_progression, spellbook_selection, channel_smite, versatile_channel),
                                                                   Helpers.CreateUIGroup(stigmata, stigmata_move, stigmata_swift),
                                                                   Helpers.CreateUIGroup(vindicator_shield, faith_healing, bloodfire, bloodrain),
                                                                  };
            /* not sure if it is worth implementing these features (they are not very useful and interface would be clunky)
            createDivineWrath();
            createDivineRetribution();
            */
        }


        static BlueprintCharacterClass[] getHolyVindicatorArray()
        {
            return new BlueprintCharacterClass[] { holy_vindicator_class };
        }


        static void createHolyVindicatorProficiencies()
        {
            holy_vindicator_proficiencies = library.CopyAndAdd<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb", "HolyVindicatorProficiencies", "");//paladin
            holy_vindicator_proficiencies.SetName("Holy Vindicator Proficiencies");
            holy_vindicator_proficiencies.SetDescription("A vindicator is proficient with all simple and martial weapons and all armor and shields (except tower shields).");
        }


        static void createSpellbookSelection()
        {
            spellbook_selection = Helpers.CreateFeatureSelection("HolyVindicatorSpellbookSelection",
                                                                 "Holy Vindicator Spellbook Selection",
                                                                 "At 2nd level, and at every level thereafter, with an exception for 5th and 9th levels, a vindicator gains new spells per day as if he had also gained a level in divine spellcasting class he belonged to before adding the prestige class. He does not, however, gain any other benefit a character of that class would have gained, except for additional spells per day, spells known (if he is a spontaneous spellcaster), and an increased effective level of spellcasting. If a character had more than one spellcasting class before becoming a holy vindicator, he must decide to which class he adds the new level for purposes of determining spells per day.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.EldritchKnightSpellbook);
            spellbook_selection.Obligatory = true;
            Common.addSpellbooksToSpellSelection("HolyVindicator", 1, spellbook_selection, arcane: false, alchemist: false);
        }


        static void createVindicatorShield()
        {
            ChannelEnergyEngine.createHolyVindicatorShield();
            vindicator_shield = ChannelEnergyEngine.holy_vindicator_shield;
        }


        static void createChannelEnergyProgression()
        {
            ChannelEnergyEngine.addClassToChannelEnergyProgression(holy_vindicator_class);

            channel_energy_progression = Helpers.CreateFeature("ChannelEnergyHolyVindicatorProgression",
                                                               "Channel Energy",
                                                               "The vindicator’s class level stacks with levels in any other class that grants the channel energy ability.",
                                                               "",
                                                               null,
                                                               FeatureGroup.None);
        }


        static void createStigmata()
        {
            ModifierDescriptor[] bonus_descriptors = new ModifierDescriptor[] { ModifierDescriptor.Sacred, ModifierDescriptor.Profane };
            StatType[] stats = new StatType[] {StatType.AC, StatType.AdditionalAttackBonus, StatType.AdditionalDamage};
            string [] stats_description = new string[] { "Armor Class Bonus", "Attack Bonus", "Damage Bonus" };

            var icon_profane = library.Get<BlueprintAbility>("a6e59e74cba46a44093babf6aec250fc").Icon;//slay living
            var icon_sacred = library.Get<BlueprintAbility>("f6f95242abdfac346befd6f4f6222140").Icon;//slay living

            stigmata = Helpers.CreateFeature("HolyVindicatorStigmata",
                                             "Stigmata",
                                             "A vindicator willingly gives his blood in service to his faith, and is marked by scarified wounds appropriate to his deity. At 2nd level, he may stop or start the flow of blood by force of will as a standard action; at 6th level it becomes a move action, and at 10th level it becomes a swift action. Activating stigmata causes holy or unholy damage equal to half the vindicator’s class level every round. While the stigmata are bleeding, the vindicator gains a sacred bonus (if he channels positive energy) or profane bonus (if he channels negative energy) equal to half his class level. Each time he activates his stigmata, the vindicator decides if the bonus applies to attack rolls, weapon damage rolls, Armor Class, saving throws or spell penetration checks; to change what the bonus applies to, the vindicator must deactivate and reactivate his stigmata. While his stigmata are burning, the vindicator ignores blood drain and bleed damage from any other source.",
                                             "",
                                             icon_sacred,
                                             FeatureGroup.None);

            stigmata_move = library.CopyAndAdd<BlueprintFeature>(stigmata.AssetGuid, "HolyVindicatorStigmataMove", "");
            stigmata_move.SetName("Stigmata: Move Action");
            stigmata_move.AddComponent(Helpers.Create<ActivatableAbilityActionTypeModierMechanics.ModifyActivatableAbilityGroupActionType>(m => 
                                                                                                            { m.group = ActivatableAbilityGroupExtension.Stigmata.ToActivatableAbilityGroup();
                                                                                                                m.action = CommandType.Move;
                                                                                                            }
                                                                                                            )
                                      );

            stigmata_swift = library.CopyAndAdd<BlueprintFeature>(stigmata.AssetGuid, "HolyVindicatorStigmataSwift", "");
            stigmata_swift.AddComponent(Common.createRemoveFeatureOnApply(stigmata_move));
            stigmata_swift.SetName("Stigmata: Swift Action");
            stigmata_swift.AddComponent(Helpers.Create<ActivatableAbilityActionTypeModierMechanics.ModifyActivatableAbilityGroupActionType>(m =>
                                                                                                        {
                                                                                                            m.group = ActivatableAbilityGroupExtension.Stigmata.ToActivatableAbilityGroup();
                                                                                                            m.action = CommandType.Swift;
                                                                                                        }
                                                                                                        )
                                      );




            var bleed_immunity = Common.createBuffDescriptorImmunity(SpellDescriptor.Bleed);

            foreach (var bonus_descriptor in bonus_descriptors)
            {
                var icon = bonus_descriptor == ModifierDescriptor.Sacred ? icon_sacred : icon_profane;
                var buffs = new List<BlueprintBuff>();
                var dmg_type = bonus_descriptor == ModifierDescriptor.Sacred ? DamageEnergyType.Holy : DamageEnergyType.Unholy;
                var add_context_actions = Helpers.CreateAddFactContextActions(Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Bleed),
                                                                        null,
                                                                        Helpers.CreateActionDealDamage(dmg_type,
                                                                                                       Helpers.CreateContextDiceValue(DiceType.Zero, bonus: Helpers.CreateContextValue(AbilityRankType.StatBonus))
                                                                                                       )
                                                                        );  
                var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getHolyVindicatorArray(),
                                                                 type: AbilityRankType.StatBonus, progression: ContextRankProgression.Div2);

                for (int i = 0; i < stats.Length; i++)
                {
                    var buff = Helpers.CreateBuff(bonus_descriptor.ToString() + "Stigmata" + stats[i].ToString() + "Buff",
                                                  $"{bonus_descriptor.ToString()} Stigmata: {stats_description[i]}",
                                                  stigmata.Description,
                                                  "",
                                                  icon,
                                                  null,
                                                  add_context_actions,
                                                  bleed_immunity,
                                                  Helpers.CreateAddContextStatBonus(stats[i], bonus_descriptor, rankType: AbilityRankType.StatBonus),
                                                  context_rank_config
                                                  );
                    buffs.Add(buff);
                }

                var saves_buff = Helpers.CreateBuff(bonus_descriptor.ToString() + "Stigmata" + "SavesBuff",
                                                    $"{bonus_descriptor.ToString()} Stigmata: Saving Throws Bonus",
                                                    buffs[0].Description,
                                                    "",
                                                    icon,
                                                    null,
                                                    Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, bonus_descriptor, rankType: AbilityRankType.StatBonus),
                                                    Helpers.CreateAddContextStatBonus(StatType.SaveReflex, bonus_descriptor, rankType: AbilityRankType.StatBonus),
                                                    Helpers.CreateAddContextStatBonus(StatType.SaveWill, bonus_descriptor, rankType: AbilityRankType.StatBonus),
                                                    context_rank_config
                                                    );
                buffs.Add(saves_buff);
                var spell_penetration_buff = Helpers.CreateBuff(bonus_descriptor.ToString() + "Stigmata" + "SpellPenetrationBuff",
                                    $"{bonus_descriptor.ToString()} Stigmata: Caster Level Check Bonus",
                                    buffs[0].Description,
                                    "",
                                    icon,
                                    null,
                                    Helpers.Create<NewMechanics.CasterLevelCheckBonus>(s => s.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                                    context_rank_config
                                    );
                buffs.Add(spell_penetration_buff);

                stigmata_buffs.Add(bonus_descriptor, buffs);

                stigmata_abilities.Add(bonus_descriptor, new List<BlueprintActivatableAbility>());
                foreach (var b in buffs)
                {
                    var ability = Helpers.CreateActivatableAbility(b.name.Replace("Buff", "ActivatableAbility"),
                                                                   b.Name,
                                                                   b.Description,
                                                                   "",
                                                                   icon,
                                                                   b,
                                                                   AbilityActivationType.WithUnitCommand,
                                                                   CommandType.Standard,
                                                                   null
                                                                   );

                    ability.Group = ActivatableAbilityGroupExtension.Stigmata.ToActivatableAbilityGroup();
                    ability.DeactivateIfCombatEnded = !test_mode;
                    ability.DeactivateIfOwnerDisabled = true;
                    ability.DeactivateIfOwnerUnconscious = true;
                    stigmata_abilities[bonus_descriptor].Add(ability);
                }

                var add_stigmata = Helpers.CreateFeature($"HolyVindicator{bonus_descriptor.ToString()}StigmataFeature",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddFacts(stigmata_abilities[bonus_descriptor].ToArray())
                                                        );
                add_stigmata.HideInUI = true;

                stigmata.AddComponent(Helpers.Create<NewMechanics.AddFeatureIfHasFactsFromList>(a => { a.Feature = add_stigmata; a.CheckedFacts = new BlueprintUnitFact[0]; }));
            }

            ChannelEnergyEngine.registerStigmata(stigmata);
        }


        static void createFaithHealing()
        {
            var healers_blessing = library.Get<BlueprintFeature>("b9ea4eb16ded8b146868540e711f81c8");
            var healing_spells = new BlueprintAbility[]
            {   //cure
                library.Get<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f"),
                library.Get<BlueprintAbility>("1c1ebf5370939a9418da93176cc44cd9"),
                library.Get<BlueprintAbility>("6e81a6679a0889a429dec9cedcf3729c"),
                library.Get<BlueprintAbility>("0d657aa811b310e4bbd8586e60156a2d"),
                library.Get<BlueprintAbility>("5d3d689392e4ff740a761ef346815074"),
                library.Get<BlueprintAbility>("571221cc141bc21449ae96b3944652aa"),
                library.Get<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397"),
                library.Get<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449"),
                //inflict
                library.Get<BlueprintAbility>("e5cb4c4459e437e49a4cd73fde6b9063"),
                library.Get<BlueprintAbility>("14d749ecacca90a42b6bf1c3f580bb0c"),
                library.Get<BlueprintAbility>("3cf05ef7606f06446ad357845cb4d430"),
                library.Get<BlueprintAbility>("b0b8a04a3d74e03489862b03f4e467a6"),
                library.Get<BlueprintAbility>("9da37873d79ef0a468f969e4e5116ad2"),
                library.Get<BlueprintAbility>("03944622fbe04824684ec29ff2cec6a7"),
                library.Get<BlueprintAbility>("820170444d4d2a14abc480fcbdb49535"),
                library.Get<BlueprintAbility>("5ee395a2423808c4baf342a4f8395b19"),
            };
            var faith_healing_empower = Helpers.CreateFeature("HolyVindicatorFaithHealingEmpowerFeature",
                                                              "Faith Healing: Empower",
                                                              "At 3rd level, any cure wounds spells a vindicator casts on himself are automatically empowered as if by the Empower Spell feat, except they do not use higher spell level slots or an increased casting time. At 8th level, these healing spells are maximized rather than empowered.",
                                                              "",
                                                              healers_blessing.Icon,
                                                              FeatureGroup.None,
                                                              Helpers.Create<HealingMechanics.SelfHealingMetamagic>(m =>
                                                                                                                    {
                                                                                                                        m.spells = healing_spells.ToArray();
                                                                                                                        m.empower = true;
                                                                                                                    }
                                                                                                                    )
                                                             );

            var faith_healing_maximize = Helpers.CreateFeature("HolyVindicatorFaithHealingMaximizeFeature",
                                                  "Faith Healing: Maximize",
                                                  faith_healing_empower.Description,
                                                  "",
                                                  faith_healing_empower.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<HealingMechanics.SelfHealingMetamagic>(m =>
                                                                                                        {
                                                                                                            m.spells = healing_spells.ToArray();
                                                                                                            m.maximize = true;
                                                                                                        }
                                                                                                        )
                                                 );

            faith_healing = Helpers.CreateFeature("HolyVindicatorFaithHealingFeature",
                                                  "Faith Healing",
                                                  faith_healing_empower.Description,
                                                  "",
                                                  faith_healing_empower.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFeatureOnClassLevel(faith_healing_empower, 8, getHolyVindicatorArray(), true),
                                                  Helpers.CreateAddFeatureOnClassLevel(faith_healing_maximize, 8, getHolyVindicatorArray())
                                                  );
        }


        static void createBloodfireAndBloodrain()
        {
            bloodfire = Helpers.CreateFeature("BloodFireHolyVindicatorFeature",
                                              "Bloodfire",
                                              $"At 5th level, while a vindicator’s stigmata are bleeding, his blood runs down his weapons like sacred or profane liquid energy; when he uses Channel Smite, the damage increases by 1d{BalanceFixes.getDamageDieString(DiceType.D6)}, and if the target fails its save, it is sickened and takes 1d6 points of bleed damage each round on its turn. The target can attempt a new save every round to end the sickened and bleed effects.",
                                              "",
                                              null,
                                              FeatureGroup.None);

            var bloodfire_buff = Helpers.CreateBuff("BloodFireHolyVindicatorBuff",
                                                      bloodfire.Name,
                                                      bloodfire.Description,
                                                      "",
                                                      null,
                                                      null);
            bloodfire_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            bloodrain = Helpers.CreateFeature("BloodRainHolyVindicatorFeature",
                                  "Bloodrain",
                                  $"At 9th level, while his stigmata are bleeding, the vindicator’s harmful channeled energy is accompanied by a burst of sacred or profane liquid energy, increasing the damage by 1d{BalanceFixes.getDamageDieString(DiceType.D6)}. Creatures failing their saves against the channeled energy become sickened and take 1d6 points of bleed damage each round. Affected creatures can attempt a new save every round to end the sickened and bleed effects.",
                                  "",
                                  null,
                                  FeatureGroup.None);

            var bloodrain_buff = Helpers.CreateBuff("BloodRainHolyVindicatorBuff",
                                                      bloodrain.Name,
                                                      bloodrain.Description,
                                                      "",
                                                      null,
                                                      null);
            bloodrain_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            foreach (var buff_list in stigmata_buffs.Values)
            {
                foreach (var buff in buff_list)
                {
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, bloodfire_buff, bloodfire);
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, bloodrain_buff, bloodrain);
                }
            }

            var bleed_buff = library.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562");
            var sickened_buff = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");

            var blood_buff = Helpers.CreateBuff("HolyVindicatorBloodBuff",
                                                "",
                                                "",
                                                "",
                                                null,
                                                null,
                                                Helpers.CreateAddFactContextActions(new GameAction[]{Common.createContextActionApplyBuff(bleed_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false),
                                                                                                     Common.createContextActionApplyBuff(sickened_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)
                                                                                                    }),
                                                 Helpers.Create<NewMechanics.BuffRemoveOnSave>(b => b.SaveType = SavingThrowType.Will)
                                                );
            blood_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var save_failed_action = Common.createContextSavedApplyBuff(blood_buff, DurationRate.Rounds, is_permanent: true, is_dispellable: false);
            /*var save_failed_action = Common.createContextActionSavingThrow(SavingThrowType.Will, 
                                                                           Helpers.CreateActionList(Common.createContextSavedApplyBuff(sickened_buff, DurationRate.Rounds, is_permanent: true, is_dispellable: false))
            ;
                                                                           );*/

            var positive_damage = Helpers.CreateActionDealDamage(DamageEnergyType.PositiveEnergy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 1), IgnoreCritical: true);
            var negative_damage = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 1), IgnoreCritical: true);

            var smite_positive_action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(bloodfire_buff), new GameAction[] { positive_damage, save_failed_action });
            var smite_negative_action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(bloodfire_buff), new GameAction[] { negative_damage, save_failed_action });
            var positive_action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(bloodrain_buff), new GameAction[] { positive_damage, save_failed_action });
            var negative_action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(bloodrain_buff), new GameAction[] { negative_damage, save_failed_action });

            ChannelEnergyEngine.addBloodfireAndBloodrainActions(positive_action, negative_action, smite_positive_action, smite_negative_action );
        }


        static void createVersatileChannel()
        {
            versatile_channel = Helpers.CreateFeature("VersatileChannelHolyVindicator",
                                                      "Versatile Channel",
                                                      "At 6th level, a vindicator’s channel energy can instead affect a 30-foot cone or a 120-foot line.",
                                                      "",
                                                      null,
                                                      FeatureGroup.None);

            ChannelEnergyEngine.addVersatileChannel(versatile_channel);
        }

    }
}
