using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony12;
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
using Kingmaker.Controllers;
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
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
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
    class RelicHunter
    {
        static public BlueprintArchetype archetype;
        static public BlueprintSpellbook spellbook;

        static public Dictionary<SpellSchool, BlueprintAbilityResource> deific_focus_resource = new Dictionary<SpellSchool, BlueprintAbilityResource>();
        static public Dictionary<SpellSchool, BlueprintAbility> invest_focus_abilities = new Dictionary<SpellSchool, BlueprintAbility>();
        static public Dictionary<SpellSchool, ImplementsEngine> implement_factories = new Dictionary<SpellSchool, ImplementsEngine>();
        static public Dictionary<SpellSchool, BlueprintFeature> base_implements = new Dictionary<SpellSchool, BlueprintFeature>();
        static public Dictionary<SpellSchool, BlueprintFeature> second_implements = new Dictionary<SpellSchool, BlueprintFeature>();

        static public BlueprintFeatureSelection focus_power_selection;
        static public BlueprintFeatureSelection first_implement_selection;
        static public BlueprintFeatureSelection implement_selection;
        static public BlueprintFeature deific_focus;
        static public BlueprintBuff locked_focus_buff;

        static LibraryScriptableObject library => Main.library;


        static BlueprintCharacterClass[] getOccultistArray()
        {
            return new BlueprintCharacterClass[] { archetype.GetParentClass() };
        }


        internal static void create()
        {
            var inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
           


            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "RelicHunterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Relic Hunter");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some inquisitors specialize in the use and recovery of long-lost relics of their faiths, drawing forth divine might from the recovered items in order to restore their sanctity and wield these artifacts against the enemies of their gods.");
            });
            Helpers.SetField(archetype, "m_ParentClass", inquisitor_class);
            library.AddAsset(archetype, "");

            spellbook = library.CopyAndAdd(inquisitor_class.Spellbook, "RelicHunterSpellBook", "");
            spellbook.SpellsKnown = Occultist.occultist_class.Spellbook.SpellsKnown;
            spellbook.AddComponent(Helpers.Create<SpellbookMechanics.OccultistSpellbook>());
            archetype.ReplaceSpellbook = spellbook;

            createDeificFocus();
            createImplements();
            createFocusPowers();

            var judgment = library.Get<BlueprintFeature>("981def910b98200499c0c8f85a78bde8");
            var judgment_extra_use = library.Get<BlueprintFeature>("ee50875819478774b8968701893b52f5");
            var bane = library.Get<BlueprintFeature>("7ddf7fbeecbe78342b83171d888028cf");
            var bane_greater = library.Get<BlueprintFeature>("6e694114b2f9e0e40a6da5d13736ff33");
            var judgment2 = library.Get<BlueprintFeature>("33bf0404b70d65f42acac989ec5295b2");
            var judgment3 = library.Get<BlueprintFeature>("490c7e92b22cc8a4bb4885a027b355db");
            var judgment_true = library.Get<BlueprintFeature>("f069b6557a2013544ac3636219186632");
            var domains = library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, judgment, domains),
                                                          Helpers.LevelEntry(4, judgment_extra_use),
                                                          Helpers.LevelEntry(5, bane),
                                                          Helpers.LevelEntry(7, judgment_extra_use),
                                                          Helpers.LevelEntry(8, judgment2),
                                                          Helpers.LevelEntry(10, judgment_extra_use),
                                                          Helpers.LevelEntry(12, bane_greater),
                                                          Helpers.LevelEntry(13, judgment_extra_use),
                                                          Helpers.LevelEntry(16, judgment_extra_use, judgment3),
                                                          Helpers.LevelEntry(19, judgment_extra_use),
                                                          Helpers.LevelEntry(20, judgment_true),
                                                       };
            archetype.ReplaceSpellbook = spellbook;
            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, deific_focus, first_implement_selection, implement_selection, focus_power_selection),
                                                       Helpers.LevelEntry(4, implement_selection, focus_power_selection),
                                                       Helpers.LevelEntry(7, implement_selection),
                                                       Helpers.LevelEntry(8, focus_power_selection),
                                                       Helpers.LevelEntry(10, implement_selection),
                                                       Helpers.LevelEntry(12, focus_power_selection),
                                                       Helpers.LevelEntry(13, implement_selection),
                                                       Helpers.LevelEntry(16, implement_selection, focus_power_selection),
                                                       Helpers.LevelEntry(19, implement_selection),
                                                       Helpers.LevelEntry(20, focus_power_selection),
                                                     };
            

            inquisitor_class.Progression.UIDeterminatorsGroup = inquisitor_class.Progression.UIDeterminatorsGroup.AddToArray(deific_focus);
            inquisitor_class.Progression.UIGroups = inquisitor_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(first_implement_selection, implement_selection));
            inquisitor_class.Progression.UIGroups = inquisitor_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(focus_power_selection));
            inquisitor_class.Archetypes = inquisitor_class.Archetypes.AddToArray(archetype);

            addToPrestigeClasses();
            Occultist.occultist_class.AddComponent(Common.prerequisiteNoArchetype(inquisitor_class, archetype));
        }


        static void addToPrestigeClasses()
        {
            var inquisitor_mt = library.Get<BlueprintProgression>("d21a104c204ed7348a51405e68387013");
            inquisitor_mt.AddComponent(Common.prerequisiteNoArchetype(archetype));

            Common.addMTDivineSpellbookProgression(archetype.GetParentClass(), archetype.ReplaceSpellbook, "MysticTheurgeRelicHunterProgression",
                                       Common.createPrerequisiteArchetypeLevel(archetype, 1),
                                       Common.createPrerequisiteClassSpellLevel(archetype.GetParentClass(), 2)
                                       );
        }

        static void createDeificFocus()
        {
            deific_focus = Helpers.CreateFeature("DeificFocusResource",
                                                 "Deific Focus",
                                                 "At 1st level, a relic hunter learns to invest divine power into her chosen relics. This acts like the occultist’s focus powers and mental focus class features, with the following adjustments.\n"
                                                 + "Each day, a relic hunter has a number of points of deific focus equal to her inquisitor level + her Wisdom modifier, and she must spend 1 hour in prayer with her relics to invest them with divine power. These points refresh at the start of each day.",
                                                 "",
                                                 Occultist.implement_icons[SpellSchool.Universalist],
                                                 FeatureGroup.None,
                                                 Helpers.Create<ImplementMechanics.AddImplements>()
                                                 );

            locked_focus_buff = Helpers.CreateBuff("RelicHunterLockedFocusBuff",
                                                   "Deific Focus Invested",
                                                   "You have invested deific focus into your implements.",
                                                   "",
                                                   Occultist.implement_icons[SpellSchool.Universalist],
                                                   null,
                                                   Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<ImplementMechanics.ContextActionUnlockFocus>())
                                                   );
            locked_focus_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath);

            var lock_focus_ability = Helpers.CreateAbility("RelicHunterLockFocusAbility",
                                                           "Lock Invested Deific Focus",
                                                           "Lock invested deific focus until you rest.",
                                                           "",
                                                           Occultist.implement_icons[SpellSchool.Universalist],
                                                           AbilityType.Special,
                                                           CommandType.Standard,
                                                           AbilityRange.Personal,
                                                           "",
                                                           "",
                                                           Helpers.CreateRunActions(Helpers.Create<ImplementMechanics.ContextActionLockFocus>(),
                                                                                    Common.createContextActionApplyBuff(locked_focus_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true)
                                                                                    ),
                                                           Helpers.Create<ImplementMechanics.AbilityCasterFocusLocked>(a => a.not = true)
                                                           );
            lock_focus_ability.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(lock_focus_ability);

            var reset_focus_ability = Helpers.CreateAbility("RelicHunterResetFocusAbility",
                                               "Reset Invested Deific Focus",
                                               "Reset deific focus you invested into your implements and redistribute it anew.",
                                               "",
                                               Occultist.implement_icons[SpellSchool.None],
                                               AbilityType.Special,
                                               CommandType.Free,
                                               AbilityRange.Personal,
                                               "",
                                               "",
                                               Helpers.CreateRunActions(Helpers.Create<ImplementMechanics.ContextActionResetFocus>()),
                                               Helpers.Create<ImplementMechanics.AbilityCasterFocusLocked>(a => a.not = true)
                                               );
            reset_focus_ability.setMiscAbilityParametersSelfOnly();

            List<BlueprintAbility> abilities = new List<BlueprintAbility>();
            abilities.Add(lock_focus_ability);
            abilities.Add(reset_focus_ability);

            foreach (SpellSchool school in Enum.GetValues(typeof(SpellSchool)))
            {
                if (school == SpellSchool.None)
                {
                    continue;
                }

                var resource = Helpers.CreateAbilityResource(school.ToString() + "DeificFocusResource", "", "", "", null);
                if (school == SpellSchool.Universalist)
                {
                    resource.SetIncreasedByLevel(0, 1, getOccultistArray());
                    resource.SetIncreasedByStat(0, StatType.Wisdom);
                }
                else
                {
                    resource.SetFixedResource(0);
                }
                deific_focus_resource[school] = resource;
                deific_focus.AddComponent(resource.CreateAddAbilityResource());
                var reset_action = reset_focus_ability.GetComponent<AbilityEffectRunAction>().Actions;
                reset_action.Actions = reset_action.Actions.AddToArray(Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => { c.full = true; c.Resource = resource; }));

                if (school == SpellSchool.Universalist)
                {
                    continue;
                }

                var invest_focus_ability = Helpers.CreateAbility(school.ToString() + "RelicHunterInvestFocusAbility",
                                                                 "Invest Deific Focus: " + school.ToString(),
                                                                 "Invest deific focus into specified implement.",
                                                                 "",
                                                                 Occultist.implement_icons[school],
                                                                 AbilityType.Special,
                                                                 CommandType.Free,
                                                                 AbilityRange.Personal,
                                                                 "",
                                                                 "",
                                                                 Helpers.CreateRunActions(Helpers.Create<ImplementMechanics.ContextActionInvestFocus>(c => { c.school = school; c.resource = deific_focus_resource[school]; })),
                                                                 resource.CreateResourceLogic()
                                                                 );
                invest_focus_ability.setMiscAbilityParametersSelfOnly();
                abilities.Add(invest_focus_ability);
                invest_focus_abilities[school] = invest_focus_ability;
            }

            foreach (var ability in abilities)
            {
                ability.MaybeReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = deific_focus_resource[SpellSchool.Universalist]);
            }

            var wrapper = Common.createVariantWrapper("DeificFocusAbilityBase", "", abilities.ToArray());
            wrapper.SetNameDescriptionIcon(deific_focus);

            deific_focus.AddComponent(Helpers.CreateAddFact(wrapper));
            deific_focus.AddComponents(Helpers.Create<ImplementMechanics.IncreaseResourceAmountBasedOnInvestedFocus>(r =>
            {
                r.resource = deific_focus_resource[SpellSchool.Universalist];
                r.school = SpellSchool.Universalist;
            })
                                        );

            foreach (var panoply in Enum.GetValues(typeof(Occultist.Panoply)))
            {
                var resource = Helpers.CreateAbilityResource(panoply.ToString() + "DeificFocusResource", "", "", "", null);
                resource.SetFixedResource(100);
                deific_focus_resource[(SpellSchool)panoply] = resource;
            }
        }


        static void createImplements()
        {
            first_implement_selection = Helpers.CreateFeatureSelection("RelicHunterBaseImplementSelection",
                                                                      "Relics",
                                                                      "At 1st level, a relic hunter gains the occultist’s implements class feature and learns to use two occultist implement schools as relic schools. At 4th level and every 3 levels thereafter, the relic hunter learns to use one additional relic school drawn from the same source, gaining access to that school’s resonant power and base focus power and opening up that school’s focus powers for her to select. Like an occultist, a relic hunter can select the same school twice, but it is far less useful for her to do so.\n"
                                                                      + "Relics do not need to be magic items, and non-magical relics do not take up a magic item slot. Relics that are not magic items are often of some religious significance to the relic hunter or her church, such as the battered shield of a saint, a bishop’s robe, or the finger bone of a church martyr.",
                                                                      "",
                                                                      null,
                                                                      FeatureGroup.None
                                                                      );
            //initialize implement engines
            var schools = new SpellSchool[] { SpellSchool.Abjuration, SpellSchool.Conjuration, SpellSchool.Divination, SpellSchool.Enchantment,
                                              SpellSchool.Evocation, SpellSchool.Illusion, SpellSchool.Necromancy, SpellSchool.Transmutation,
                                              (SpellSchool)Occultist.Panoply.TrappingOfTheWarrior, (SpellSchool)Occultist.Panoply.MagesParaphernalia,
                                              (SpellSchool)Occultist.Panoply.SaintsRegalia, (SpellSchool)Occultist.Panoply.PerformersAccoutrements};

            foreach (var s in schools)
            {
                if (!Occultist.isPanoply(s))
                {
                    implement_factories[s] = new ImplementsEngine("RelicHunter", deific_focus_resource[s],
                                                                  getOccultistArray(),
                                                                  StatType.Wisdom);
                }
                else
                {
                    implement_factories[s] = new ImplementsEngine("RelicHunter", deific_focus_resource[s],
                                                                  getOccultistArray(),
                                                                  StatType.Wisdom);
                }
            }

            Dictionary<SpellSchool, (string flavor, BlueprintFeature base_power, BlueprintBuff[] resonant_power_buffs)> implement_data
                = new Dictionary<SpellSchool, (string, BlueprintFeature, BlueprintBuff[])>
            {
                {SpellSchool.Abjuration, ("Abjuration implements are objects associated with protection and wards." ,
                                           implement_factories[SpellSchool.Abjuration].createMindBarrier(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Abjuration].createWardingTalisman()}
                                           )
                },
                {SpellSchool.Conjuration, ("Implements used in conjuration allow the relic hunter to perform magic that transports or calls creatures.",
                                           implement_factories[SpellSchool.Conjuration].createServitor(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Conjuration].createCastingFocus()}
                                           )
                },
                {SpellSchool.Divination, ("Implements of the divination school grant powers related to foresight and remote viewing.",
                                           implement_factories[SpellSchool.Divination].createSuddenInsight(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Divination].createThirdEye()}
                                           )
                },
                {SpellSchool.Enchantment, ("Enchantment implements allow the relic hunter to befuddle the mind and charm his foes.",
                                           implement_factories[SpellSchool.Enchantment].createCloudMind(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Enchantment].createGloriousPresence()}
                                           )
                },
                {SpellSchool.Evocation, ("Implements focused on evocation grant the ability to create and direct energy to protect and to destroy.",
                                           implement_factories[SpellSchool.Evocation].createEnergyRay(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Evocation].createIntenseFocus()}
                                           )
                },
                {SpellSchool.Illusion, ("Illusion implements allow the relic hunter to distort the senses and cloak creatures from sight.",
                                           implement_factories[SpellSchool.Illusion].createColorBeam(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Illusion].createDistortion()}
                                           )
                },
                {SpellSchool.Necromancy, ("Implements that draw power from necromancy can control undead and harm the living.",
                                           implement_factories[SpellSchool.Necromancy].createMindFear(),
                                           new BlueprintBuff[]{implement_factories[SpellSchool.Necromancy].createNecromanticFocus()}
                                           )
                },
                {SpellSchool.Transmutation,("Transmutation implements can alter the properties of both objects and creatures.",
                                           implement_factories[SpellSchool.Transmutation].createLegacyWeapon(),
                                           implement_factories[SpellSchool.Transmutation].createPhysicalEnhancement()
                                           )
                },
                {(SpellSchool)Occultist.Panoply.TrappingOfTheWarrior,("This panoply is associated with brave and stalwart warriors, martial skill, and the defense of one’s allies.\n"
                                                            +"Associated implement schools: Abjuration, Transmuation.",
                                           implement_factories[(SpellSchool)Occultist.Panoply.TrappingOfTheWarrior].createCounterStrike(),
                                           new BlueprintBuff[]{ implement_factories[(SpellSchool)Occultist.Panoply.TrappingOfTheWarrior].createMartialSkill()}
                                           )
                },
                {(SpellSchool)Occultist.Panoply.MagesParaphernalia, ("This panoply is associated with the arcane arts and the masters of manipulating magic.\n"
                                                          + "Associated implement schools: Divination, Evocation, Necromancy.",
                                           implement_factories[(SpellSchool)Occultist.Panoply.MagesParaphernalia].createSpellPower(archetype.GetParentClass()),
                                           new BlueprintBuff[]{ implement_factories[(SpellSchool)Occultist.Panoply.MagesParaphernalia].createScholarlyKnowledge()}
                                           )
                },
                {(SpellSchool)Occultist.Panoply.SaintsRegalia, ("This panoply is associated with devoted members of a good-aligned faith and the power of belief.\n"
                                                          + "Associated implement schools: Abjuration, Conjuration.",
                                           implement_factories[(SpellSchool)Occultist.Panoply.SaintsRegalia].createRestoringTouch(),
                                           new BlueprintBuff[]{ implement_factories[(SpellSchool)Occultist.Panoply.SaintsRegalia].createFontOfHealing()}
                                           )
                },
                {(SpellSchool)Occultist.Panoply.PerformersAccoutrements, ("This panoply is associated with those who deceive as well as those who entertain..\n"
                                                          + "Associated implement schools: Illusion, Enchantment.",
                                           implement_factories[(SpellSchool)Occultist.Panoply.PerformersAccoutrements].createTrickstersEdge(),
                                           new BlueprintBuff[]{ implement_factories[(SpellSchool)Occultist.Panoply.PerformersAccoutrements].createTrickstersSkill()}
                                           )
                },
            };

            foreach (var s in schools)
            {
                bool starts_with_vowel = "aeiou".IndexOf(s.ToString().ToLower()) >= 0;
                var data = implement_data[s];
                var description = "";
                if (!Occultist.isPanoply(s))
                {
                    description = data.flavor + "\n"
                    + $"Resonant Power: Each time the relic hunter invests deific focus into a{(starts_with_vowel ? "n" : "")} {s.ToString()} implement, the implement grants the following resonant power. The implement’s bearer gains the benefits of this power until the relic hunter refreshes his focus.\n"
                    + (data.resonant_power_buffs.Length > 1 ? "Physical Enhancement" : data.resonant_power_buffs[0].Name) + ": " + data.resonant_power_buffs[0].Description + "\n"
                    + $"Base Focus Power: All relic hunters who learn to use {s.ToString()} implements gain the following focus power.\n"
                    + data.base_power.Name + ": " + data.base_power.Description;
                }
                else
                {
                    description = data.flavor + "\n"
                    + $"Resonant Power: Each time the relic hunter invests deific focus into all of the associated implements, the panoply grants the following resonant power. The panoply’s bearer gains the benefits of this power until the relic hunter refreshes his focus.\n"
                    + (data.resonant_power_buffs.Length > 1 ? "Physical Enhancement" : data.resonant_power_buffs[0].Name) + ": " + data.resonant_power_buffs[0].Description + "\n"
                    + $"Base Focus Power: All relic hunters who learn to use this panoply gain the following focus power.\n"
                    + data.base_power.Name + ": " + data.base_power.Description;
                }

                base_implements[s] = Helpers.CreateFeature(Occultist.getString(s) + "RelicHunterImplementFeature",
                                                           Occultist.isPanoply(s) ? Occultist.getHumanString(s) : Occultist.getString(s) + " Implement",
                                                           description,
                                                           "",
                                                           Occultist.implement_icons[s],
                                                           FeatureGroup.Domain
                                                           );
                if (!Occultist.isPanoply(s))
                {
                    second_implements[s] = library.CopyAndAdd(base_implements[s], s.ToString() + "RelicHunterSecondImplementFeature", "");
                    second_implements[s].SetName(base_implements[s].Name + " (Extra Spells)");
                    second_implements[s].AddComponent(Helpers.PrerequisiteFeature(base_implements[s]));
                    invest_focus_abilities[s].AddComponent(Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = base_implements[s]));
                }
                else
                {
                    base_implements[s].AddComponent(Helpers.Create<ResourceMechanics.ConnectResource>(c =>
                    {
                        c.base_resource = deific_focus_resource[s];
                        c.connected_resources = deific_focus_resource.Where(r => Occultist.getCorrepsondingSpellSchools(s).Contains(r.Key)).Select(p => p.Value).ToArray();
                    })
                    );

                    base_implements[s].AddComponents(Occultist.getCorrepsondingSpellSchools(s).Select(a => Helpers.PrerequisiteFeature(base_implements[a])));
                    base_implements[s].AddComponent(deific_focus_resource[s].CreateAddAbilityResource());
                }
                base_implements[s].AddComponent(Helpers.CreateAddFact(data.base_power));


                if (s != SpellSchool.Transmutation)
                {
                    var conditions = new List<Condition>();
                    var focus_condition = Helpers.Create<ImplementMechanics.ContextConditionInvestedFocusAmount>(c =>
                    {
                        c.schools = Occultist.getCorrepsondingSpellSchools(s);
                        c.amount = 1;
                        c.locked_focus = false;
                    }
                                                                        );
                    conditions.Add(focus_condition);

                    if (Occultist.isPanoply(s))
                    {
                        conditions.Add(Common.createContextConditionHasFact(base_implements[s]));
                    }
                    Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(locked_focus_buff,
                                                                                              Helpers.CreateConditional(conditions.ToArray(),
                                                                                              Common.createContextActionApplyChildBuff(data.resonant_power_buffs[0])
                                                                                              )
                                                                                              );
                }
                else
                {
                    var toggles = new List<BlueprintActivatableAbility>();
                    foreach (var b in data.resonant_power_buffs)
                    {
                        var toggle_buff = library.CopyAndAdd(b, "Toggle" + b.name, "");
                        toggle_buff.SetBuffFlags(toggle_buff.GetBuffFlags() | BuffFlags.HiddenInUi);
                        toggle_buff.ComponentsArray = new BlueprintComponent[0];
                        var toggle = Common.buffToToggle(toggle_buff, CommandType.Free, true, Helpers.Create<ImplementMechanics.RestrictionInvestedFocus>(r => { r.amount = 3; r.school = s; r.locked_focus = false; }));
                        toggles.Add(toggle);
                        toggle.Group = ActivatableAbilityGroupExtension.PhysicalEnhancementResonantPower.ToActivatableAbilityGroup();
                        Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(locked_focus_buff, b, toggle_buff);
                    }
                    base_implements[s].AddComponent(Helpers.CreateAddFacts(toggles.ToArray()));
                }
                //var spell_selection = createSpellSelection(s);
                //var spells_to_pick = createCreateSpellSelectionArrays(getCorrepsondingSpellSchools(s));
                var spell_list = Common.combineSpellLists(s.ToString() + "RelicHunterSpellList",
                                          (spell, spelllist, lvl) =>
                                          {
                                              return Occultist.getCorrepsondingSpellSchools(s).Contains(spell.School);
                                          },
                                          archetype.GetParentClass().Spellbook.SpellList);
                for (int i = 0; i < 6; i++)
                {
                    base_implements[s].AddComponent(Helpers.Create<NewMechanics.addClassSpellChoice>(a =>
                    {
                        a.character_class = archetype.GetParentClass();
                        a.spell_level = i + 1;
                        a.spell_list = spell_list;
                    })
                    );
                    if (!Occultist.isPanoply(s))
                    {
                        second_implements[s].AddComponent(Helpers.Create<NewMechanics.addClassSpellChoice>(a =>
                        {
                            a.character_class = archetype.GetParentClass();
                            a.spell_level = i + 1;
                            a.spell_list = spell_list;
                        })
                        );
                    }
                }

                if (!Occultist.isPanoply(s))
                {
                    base_implements[s].AddComponents(Helpers.Create<ImplementMechanics.IncreaseResourceAmountBasedOnInvestedFocus>(r =>
                    {
                        r.resource = deific_focus_resource[s];
                        r.school = s;
                    })
                                                    );
                }
            }

            first_implement_selection.AllFeatures = base_implements.Values.ToArray().AddToArray(second_implements.Values.ToArray());
            implement_selection = library.CopyAndAdd(first_implement_selection, "RelicHunterImplementSelection", "");
        }


        static void createFocusPowers()
        {
            focus_power_selection = Helpers.CreateFeatureSelection("RelicHunterFocusPowerSelection",
                                                          "Focus Powers",
                                                          "At 1st level, the relic hunter learns the two base focus powers from her chosen relic schools and can select one more focus power from the list of those made available by her chosen schools. Whenever she gains a new relic school, she gains the base focus power of that school.\n"
                                                          + "In addition, at 4th level and every 4 levels thereafter, she learns a new focus power selected from all of the powers granted by all of the relic schools she knows.\n"
                                                          + "She can use these focus powers only by expending points of deific focus. Unless otherwise noted, the DC for any saving throws against a focus power is equal to 10 + 1/2 the inquisitor’s class level + the inquisitor’s Wisdom modifier. She cannot select a focus power more than once. She uses her inquisitor level in place of an occultist level to qualify for focus powers.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None
                                                          );

            Dictionary<SpellSchool, BlueprintFeature[]> implement_powers = new Dictionary<SpellSchool, BlueprintFeature[]>()
            {
                {SpellSchool.Abjuration, new BlueprintFeature[]{implement_factories[SpellSchool.Abjuration].createAegis(),
                                                                implement_factories[SpellSchool.Abjuration].createEnergyShield(),
                                                                implement_factories[SpellSchool.Abjuration].createUnraveling(),
                                                                implement_factories[SpellSchool.Abjuration].createGlobeOfNegation()
                                                               }
                },
                {SpellSchool.Conjuration, new BlueprintFeature[]{implement_factories[SpellSchool.Conjuration].createFleshMend(),
                                                                 implement_factories[SpellSchool.Conjuration].createPsychicFog(),
                                                                 implement_factories[SpellSchool.Conjuration].createPurgeCorruption(),
                                                                 implement_factories[SpellSchool.Conjuration].createSideStep()
                                                                }
                },
                {SpellSchool.Divination, new BlueprintFeature[]{implement_factories[SpellSchool.Divination].createDivinersFortune(),
                                                                implement_factories[SpellSchool.Divination].createDangerSight(),
                                                                implement_factories[SpellSchool.Divination].createInAccordanceWithProphecy(),
                                                                //mind eye
                                                               }
                },
                {SpellSchool.Enchantment, new BlueprintFeature[]{implement_factories[SpellSchool.Enchantment].createBindingPattern(),
                                                                implement_factories[SpellSchool.Enchantment].createInspiredAssault(),
                                                                implement_factories[SpellSchool.Enchantment].createObey(),
                                                                }
                },
                {SpellSchool.Evocation, new BlueprintFeature[]{implement_factories[SpellSchool.Evocation].createEnergyBlast(),
                                                                implement_factories[SpellSchool.Evocation].createRadiance(),
                                                                implement_factories[SpellSchool.Evocation].createWallOfPower(),
                                                                //light matrix
                                                                }
                },
                {SpellSchool.Illusion, new BlueprintFeature[]{implement_factories[SpellSchool.Illusion].createBedevelingAura(),
                                                                implement_factories[SpellSchool.Illusion].createShadowBeast(),
                                                                implement_factories[SpellSchool.Illusion].createTerror(),
                                                                implement_factories[SpellSchool.Illusion].createUnseen(),
                                                                }
                },
                {SpellSchool.Necromancy, new BlueprintFeature[]{implement_factories[SpellSchool.Necromancy].createFleshRot(),
                                                                implement_factories[SpellSchool.Necromancy].createNecromanticServant(),
                                                                implement_factories[SpellSchool.Necromancy].createPainWave(),
                                                                implement_factories[SpellSchool.Necromancy].createSoulboundPuppet(),
                                                                implement_factories[SpellSchool.Necromancy].createSpiritShroud(),
                                                                }
                },
                {SpellSchool.Transmutation, new BlueprintFeature[]{implement_factories[SpellSchool.Transmutation].createMindOverGravity(),
                                                                implement_factories[SpellSchool.Transmutation].createPhilosophersTouch(),
                                                                implement_factories[SpellSchool.Transmutation].createQuickness(),
                                                                implement_factories[SpellSchool.Transmutation].createSizeAlteration(),
                                                                implement_factories[SpellSchool.Transmutation].createSuddenSpeed(),
                                                                }
                },
                {(SpellSchool)Occultist.Panoply.TrappingOfTheWarrior, new BlueprintFeature[]{implement_factories[(SpellSchool)Occultist.Panoply.TrappingOfTheWarrior].createCombatTrick(deific_focus_resource[SpellSchool.Universalist]),
                                                                                  }
                },
                {(SpellSchool)Occultist.Panoply.MagesParaphernalia, new BlueprintFeature[]{implement_factories[(SpellSchool)Occultist.Panoply.MagesParaphernalia].createMetamagicKnowledge(deific_focus_resource[SpellSchool.Universalist]),
                                                                                 implement_factories[(SpellSchool)Occultist.Panoply.MagesParaphernalia].createMetamagicMaster(archetype.GetParentClass())
                                                                                  }
                },
                {(SpellSchool)Occultist.Panoply.SaintsRegalia, new BlueprintFeature[]{implement_factories[(SpellSchool)Occultist.Panoply.SaintsRegalia].createGuardianAura()
                                                                            }
                },
                {(SpellSchool)Occultist.Panoply.PerformersAccoutrements, new BlueprintFeature[]{implement_factories[(SpellSchool)Occultist.Panoply.PerformersAccoutrements].createPuppetMaster()
                                                                            }
                },
            };

            foreach (var kv in implement_powers)
            {
                foreach (var f in kv.Value)
                {
                    f.AddComponent(Helpers.PrerequisiteFeature(base_implements[kv.Key]));
                    focus_power_selection.AllFeatures = focus_power_selection.AllFeatures.AddToArray(f);
                }
            }
        }
    }
}
