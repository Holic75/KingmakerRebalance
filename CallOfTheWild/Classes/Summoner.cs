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
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Recommendations;
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
    public class Summoner
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass summoner_class;
        static public BlueprintProgression summoner_progression;
        static public BlueprintFeature summoner_proficiencies;
        static public BlueprintFeature summoner_cantrips;
        static public BlueprintFeatureSelection eidolon_selection;
        static List<BlueprintFeature> evolution_distribution = new List<BlueprintFeature>();

        static public BlueprintFeature link;
        static public BlueprintFeature life_link;
        static public BlueprintFeature life_bond;
        static public BlueprintFeature shield_ally;
        static public BlueprintFeature greater_shield_ally;
        static public BlueprintFeature merge_forms;

        static public BlueprintAbilityResource makers_call_resource;
        static public BlueprintFeature makers_call;
        static public BlueprintFeature transposition;
        static public BlueprintFeatureSelection aspect;
        static public BlueprintFeature[] aspect_extra;
        static public BlueprintFeatureSelection greater_aspect;
        static public BlueprintFeature[] greater_aspect_extra;
        static public BlueprintFeature twin_eidolon;
        static public BlueprintFeature[] summon_monster = new BlueprintFeature[9];
        static public BlueprintSummonPool summon_pool;
        static public BlueprintAbilityResource summon_resource;
        static public BlueprintArchetype devil_binder;
        static public BlueprintFeature[] infernal_affinity;
        static public BlueprintFeature[] infernal_arcana;
        static public BlueprintFeature devil_binder_smite_chaos;
        static public BlueprintFeature devil_binder_charisma_bonus;

        static BlueprintArchetype fey_caller;
        static public BlueprintFeature[] summon_nature_ally = new BlueprintFeature[9];
        static BlueprintArchetype naturalist;
        static AnimalFocusEngine animal_focus_engine;


        static public BlueprintArchetype master_summoner;
        static public BlueprintFeatureSelection lesser_eidolon_selection;
        static public BlueprintFeatureSelection twinned_eidolon_selection;
        static public BlueprintFeatureSelection fey_eidolon_selection;
        static public BlueprintFeatureSelection infernal_eidolon_selection;
        static List<BlueprintFeature> lesser_evolution_distribution = new List<BlueprintFeature>();
        static public BlueprintSummonPool master_summoner_free_pool;
        static public BlueprintFeature[] master_summoner_summon_monster = new BlueprintFeature[9];
        static public BlueprintAbilityResource master_summon_resource;


        static public BlueprintArchetype twinned_summoner;
        static public BlueprintAbility makers_call_ability;
        static public BlueprintFeature teamwork_feat_share;
        static public BlueprintFeatureSelection teamwork_feat;


        static public BlueprintArchetype construct_caller;
        static public BlueprintFeature[] planar_tinkering = new BlueprintFeature[5];
        static public BlueprintFeatureSelection construct_eidolon_selection;

        static public BlueprintArchetype soulbound_summoner;
        static public BlueprintFeature[] soulbound_evolution = new BlueprintFeature[5];
        static public BlueprintFeature soulbound_life_link;
        static public BlueprintFeatureSelection pactbound_curse;

        static public BlueprintAbility summon_companion_ability;
        static public BlueprintAbility summon_call_ability;



        internal static void createSummonerClass()
        {
            Main.logger.Log("Summoner class test mode: " + test_mode.ToString());
            var magus_class = library.TryGet<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");

            summoner_class = Helpers.Create<BlueprintCharacterClass>();
            summoner_class.name = "SummonerClass";
            library.AddAsset(summoner_class, "");

            summoner_class.LocalizedName = Helpers.CreateString("Summoner.Name", "Summoner");
            summoner_class.LocalizedDescription = Helpers.CreateString("Summoner.Description",
                                                                         "There are those who take a different path when pursuing the arcane arts, reaching across the boundaries of the world to the far-flung planes to call forth all manner of creatures to do their bidding. Known as summoners, these arcane practitioners form close bonds with particular outsiders, known as eidolons, which increase in power along with their callers. In the end, summoners and their eidolons become linked, sharing shards of the same souls.\n"
                                                                         + "Role: Summoners spend much of their time exploring the arcane arts alongside their eidolons. While their power comes from within, they rely heavily on their eidolon companions in dangerous situations. While a summoner and his eidolon function as individuals, their true power lies in what they can accomplish together."
                                                                         );
            summoner_class.m_Icon = magus_class.Icon;
            summoner_class.SkillPoints = magus_class.SkillPoints;
            summoner_class.HitDie = DiceType.D8;
            summoner_class.BaseAttackBonus = magus_class.BaseAttackBonus;
            summoner_class.FortitudeSave = magus_class.ReflexSave;
            summoner_class.ReflexSave = magus_class.ReflexSave;
            summoner_class.WillSave = magus_class.WillSave;
            summoner_class.Spellbook = createSummonerSpellbook();
            summoner_class.ClassSkills = new StatType[] { StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion,
                                                         StatType.SkillUseMagicDevice};
            summoner_class.IsDivineCaster = false;
            summoner_class.IsArcaneCaster = true;
            summoner_class.StartingGold = magus_class.StartingGold;
            summoner_class.PrimaryColor = magus_class.PrimaryColor;
            summoner_class.SecondaryColor = magus_class.SecondaryColor;
            summoner_class.RecommendedAttributes = new StatType[] { StatType.Charisma };
            summoner_class.NotRecommendedAttributes = new StatType[0];
            summoner_class.EquipmentEntities = magus_class.EquipmentEntities;
            summoner_class.MaleEquipmentEntities = magus_class.MaleEquipmentEntities;
            summoner_class.FemaleEquipmentEntities = magus_class.FemaleEquipmentEntities;
            summoner_class.ComponentsArray = magus_class.ComponentsArray;
            summoner_class.StartingItems = new BlueprintItem[]
            {
                library.Get<BlueprintItemArmor>("afbe88d27a0eb544583e00fa78ffb2c7"), //studded leather
                library.Get<BlueprintItemWeapon>("ada85dae8d12eda4bbe6747bb8b5883c"), //quarterstaff
                library.Get<BlueprintItemWeapon>("511c97c1ea111444aa186b1a58496664"), //light crossbow
                library.Get<BlueprintItemEquipmentUsable>("807763fd874989e4d96eb2d8e234139e"), //shield scroll
                library.Get<BlueprintItemEquipmentUsable>("affbadb670f599b4084029e5ad784cb7"), //enlarge scroll
                library.Get<BlueprintItemEquipmentUsable>("a4fbba95ffa58144ca7189bc350ed622") //grease scroll
            };

            createSummonerProgression();
            summoner_class.Progression = summoner_progression;
            createDevilBinder();
            createFeyCaller();
            createNaturalist();
            createMasterSummoner();
            createTwinnedSummoner();
            //createConstructCaller();
            createSoulboundSummoner();
            summoner_class.Archetypes = new BlueprintArchetype[] {devil_binder, fey_caller, naturalist, master_summoner, twinned_summoner, soulbound_summoner /*, construct_caller*/ };
            Helpers.RegisterClass(summoner_class);

            Evolutions.addClassToExtraEvalution(summoner_class);

            createSummonerSpells();
            addToPrestigeClasses();
        }


        static void addToPrestigeClasses()
        {
            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, summoner_class.Spellbook, "EldritchKnightSummoner",
                                        Common.createPrerequisiteClassSpellLevel(summoner_class, 3));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, summoner_class.Spellbook, "ArcaneTricksterSummoner",
                                        Common.createPrerequisiteClassSpellLevel(summoner_class, 2));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, summoner_class.Spellbook, "MysticTheurgeSummoner",
                                        Common.createPrerequisiteClassSpellLevel(summoner_class, 2));
            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, summoner_class.Spellbook, "DragonDiscipleSummoner",
                                        Common.createPrerequisiteClassSpellLevel(summoner_class, 1));
        }


        static void createSoulboundSummoner()
        {
            soulbound_summoner = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SoulboundSummonerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Soulbound Summoner");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Not everyone who becomes a summoner is an intentional dabbler in the arcane arts. Soulbound summoners, as a rule, never set out to bind their soul to a dangerous and enigmatic power. Rather, they have found themselves unintentional masters of eidolons through incredible circumstances. Most common are those who forged their pacts with an outsider out of a mutual desire for self-preservation. Other soulbound summoners never contacted a true outsider at all, instead manifesting an eidolon from their minds in response to mental or magical trauma. The events that create such a summoner result in the eidolon fusing entirely to the summoner’s psyche. These accidental summoners lack the practiced skill at reaching across planes that most summoners have, but their intense bond with their eidolon grants both summoner and outsider unusual power.");
            });
            Helpers.SetField(soulbound_summoner, "m_ParentClass", summoner_class);
            library.AddAsset(soulbound_summoner, "");


            //create planar tinkering
            for (int i = 0; i < soulbound_evolution.Length; i++)
            {
                soulbound_evolution[i] = Helpers.CreateFeature($"SoulboundEvolution{i}Feature",
                                                          "Soulbound Evolution",
                                                          "A soulbound summoner’s eidolon gains power from its unusually strong bond with its summoner.\n"
                                                          + "At 3rd level, and every 4 levels thereafter, the eidolon adds 1 point to its evolution pool.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(ep => ep.amount = 1)
                                                          );
            }

            createSoulboundLifeLink();
            createPactboundCurse();

            soulbound_summoner.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, life_link, summon_monster[0]),
                                                             Helpers.LevelEntry(3, summon_monster[1]),
                                                             Helpers.LevelEntry(5, summon_monster[2]),
                                                             Helpers.LevelEntry(7, summon_monster[3]),
                                                             Helpers.LevelEntry(9, summon_monster[4]),
                                                             Helpers.LevelEntry(11, summon_monster[5]),
                                                             Helpers.LevelEntry(13, summon_monster[6]),
                                                             Helpers.LevelEntry(15, summon_monster[7]),
                                                             Helpers.LevelEntry(17, summon_monster[8])
                                                           };
            soulbound_summoner.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, soulbound_life_link, pactbound_curse),
                                                             Helpers.LevelEntry(3, soulbound_evolution[0]),
                                                             Helpers.LevelEntry(7, soulbound_evolution[1]),
                                                             Helpers.LevelEntry(11, soulbound_evolution[2]),
                                                             Helpers.LevelEntry(15, soulbound_evolution[3]),
                                                             Helpers.LevelEntry(19, soulbound_evolution[4])
                                                        };

            summoner_class.Progression.UIDeterminatorsGroup = summoner_class.Progression.UIDeterminatorsGroup.AddToArray(pactbound_curse);
            summoner_class.Progression.UIGroups = summoner_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(soulbound_evolution));
        }


        static void createPactboundCurse()
        {
            pactbound_curse = Helpers.CreateFeatureSelection("PactboundCurseSelection",
                                                             "Pactbond Curse",
                                                             "A soulbound summoner’s metaphysical connection with his eidolon has profound effects on both of them. The eidolon’s alignment always matches that of the soulbound summoner, regardless of its subtype. In addition, at 1st level, the summoner must choose an oracle curse, using his summoner level as his oracle level for determining the curse’s effects. Once this choice is made, it cannot be changed. A summoner that gains spells for his list of spells known as a result of his curse must be able to cast spells of the appropriate level in order to cast the learned spell.",
                                                             "",
                                                             null,
                                                             FeatureGroup.Domain,
                                                             Helpers.Create<NoSelectionIfAlreadyHasFeature>(n => { n.AnyFeatureFromSelection = true; n.Features = new BlueprintFeature[0]; })
                                                             );
            pactbound_curse.AllFeatures = new BlueprintFeature[0];
            //curses will be added from Oracle class
        }


        public static void addCurseProgressionToPactboundCurse(BlueprintProgression oracle_curse)
        {
            ClassToProgression.addClassToProgression(summoner_class, new BlueprintArchetype[] { soulbound_summoner }, ClassToProgression.DomainSpellsType.NormalList,
                                                     oracle_curse, Oracle.oracle_class);

            pactbound_curse.AllFeatures = pactbound_curse.AllFeatures.AddToArray(oracle_curse);
            
        }


        static void createSoulboundLifeLink()
        {
            int[] hp_amount = new int[] { 1, 5, 10, 25 };
            var abilities = new BlueprintAbility[hp_amount.Length];


            for (int i = 0; i < hp_amount.Length; i++)
            {
                var heal = Common.createContextActionHealTargetNoBonus(Helpers.CreateContextDiceValue(DiceType.Zero, 0, hp_amount[i]));
                var breath_of_life = Helpers.Create<ContextActionBreathOfLife>(b => b.Value = Helpers.CreateContextDiceValue(DiceType.Zero, 0, hp_amount[i]));
                var eidolon_action = Helpers.CreateConditional(Helpers.Create<ContextConditionAlive>(), heal, breath_of_life);

                var damage = Helpers.CreateActionDealDamage(DamageEnergyType.Holy, Helpers.CreateContextDiceValue(DiceType.Zero, 0, hp_amount[i]));
                damage.DamageType.Type = Kingmaker.RuleSystem.Rules.Damage.DamageType.Direct;

                var ability = Helpers.CreateAbility($"SoulboundLifeLink{hp_amount[i]}Ability",
                                                    $"Soulbound Life Link ({hp_amount[i]} HP)",
                                                    "The essence of a soulbound summoner’s eidolon resides within the summoner’s mind and soul instead of a home plane, and he can use this connection to restore his eidolon with his own vitality. The summoner can use his life link ability to sacrifice any number of his hit points without using an action. Each hit point sacrificed in this way heals the eidolon for 1 point of damage. The soulbound summoner can use this ability even after the eidolon has been killed and sent back to its summoner’s mind; if the eidolon is healed enough that its hit point total is above 0, it can be summoned again as normal.",
                                                    "",
                                                    Helpers.GetIcon("41c9016596fe1de4faf67425ed691203"), //cure critical wounds
                                                    AbilityType.Supernatural,
                                                    CommandType.Free,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(damage, Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList((eidolon_action)))),
                                                    Common.createAbilitySpawnFx("9a38d742801be084d89bd34318c600e8", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                    Helpers.Create<CompanionMechanics.AbilityCasterCompanionUnsummoned>(a => a.not = true),
                                                    Helpers.Create<NewMechanics.AbilityCasterHpCondition>(a => { a.Inverted = true; a.CurrentHPLessThan = hp_amount[i] + 1; })
                                                    );
                ability.setMiscAbilityParametersSelfOnly();
                abilities[i] = ability;
            }

            var wrapper = Common.createVariantWrapper("SoulboundLifeLinkBase", "", abilities);
            wrapper.SetName("Soulbound Link");

            soulbound_life_link = Common.AbilityToFeature(wrapper, false);
        }

        static void createConstructCaller()
        {
            construct_caller = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ConstructCallerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Construct Caller");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Construct callers use their mechanical and planar understandings to work planar energies as a tinkerer works parts. They use this understanding to hand-craft unique, constructed eidolons.");
            });
            Helpers.SetField(construct_caller, "m_ParentClass", summoner_class);
            library.AddAsset(construct_caller, "");


            //create planar tinkering
            for (int i = 0; i < planar_tinkering.Length; i++)
            {
                planar_tinkering[i] = Helpers.CreateFeature($"PlanarTinkering{i}Feature",
                                                          "Planar Tinkering",
                                                          "At 3rd level, a construct caller learns to better shape planar energy to improve her eidolon. The number of points in her eidolon’s evolution pool increases by 1. The number of points in the eidolon’s evolution pool increases by an additional 1 at 7th level and every 4 levels thereafter, for a total of 5 additional points at 19th level.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(ep => ep.amount = 1)
                                                          );
            }
            construct_eidolon_selection = library.CopyAndAdd(eidolon_selection, "ConstructEidolonSelection", "d0ccc547a64347668cb33da79f13de7a");
            construct_eidolon_selection.SetNameDescriptionIcon(Eidolon.construct_eidolon);
            construct_eidolon_selection.AllFeatures = new BlueprintFeature[] { Eidolon.construct_eidolon };
            construct_eidolon_selection.HideInCharacterSheetAndLevelUp = true;

            construct_caller.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, eidolon_selection, summon_monster[0]),
                                                             Helpers.LevelEntry(3, summon_monster[1]),
                                                             Helpers.LevelEntry(5, summon_monster[2]),
                                                             Helpers.LevelEntry(7, summon_monster[3]),
                                                             Helpers.LevelEntry(9, summon_monster[4]),
                                                             Helpers.LevelEntry(11, summon_monster[5]),
                                                             Helpers.LevelEntry(13, summon_monster[6]),
                                                             Helpers.LevelEntry(15, summon_monster[7]),
                                                             Helpers.LevelEntry(17, summon_monster[8])
                                                           };
            construct_caller.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, construct_eidolon_selection),
                                                             Helpers.LevelEntry(3, planar_tinkering[0]),
                                                             Helpers.LevelEntry(7, planar_tinkering[1]),
                                                             Helpers.LevelEntry(11, planar_tinkering[2]),
                                                             Helpers.LevelEntry(15, planar_tinkering[3]),
                                                             Helpers.LevelEntry(19, planar_tinkering[4])
                                                        };

            summoner_class.Progression.UIDeterminatorsGroup = summoner_class.Progression.UIDeterminatorsGroup.AddToArray(construct_eidolon_selection);
            summoner_class.Progression.UIGroups = summoner_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(planar_tinkering));
        }


        static void createTwinnedSummoner()
        {
            var dimension_door = library.Get<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2");
            var ability_normal = Helpers.CreateAbility("SummonerTwinnedTranspositionAbility",
                                                      "Twinned Transpostion",
                                                      "At 6th level, as a standard action, a twinned summoner can swap locations with his eidolon. The twinned summoner can use this ability once per day at 6th level, plus one additional time per day for every 4 levels beyond 6th. At 8th level, he can use this ability as a swift action.",
                                                      "",
                                                      dimension_door.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      "",
                                                      "",
                                                      Helpers.CreateRunActions(Helpers.Create<ContextActionCastSpell>(c => c.Spell = summon_call_ability),
                                                                               Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList(Helpers.Create<ContextActionCastSpell>(ca => ca.Spell = dimension_door)))
                                                                               ),
                                                      makers_call_resource.CreateResourceLogic()
                                                     );
            ability_normal.AddComponent(Helpers.Create<NewMechanics.AbilityCasterCompanionDead>(a => a.not = true));
            var twinned_transposition = Common.AbilityToFeature(ability_normal, false);
            twinned_transposition.AddComponent(makers_call_resource.CreateAddAbilityResource());

            var ability_swift = library.CopyAndAdd(ability_normal, "SummonerTwinnedTranspositionSwiftAbility", "");
            ability_swift.ActionType = CommandType.Swift;
            ability_swift.SetName(ability_normal.Name + " (Swift)");
            var swift_twinned_transposition = Common.AbilityToFeature(ability_swift, false);

            teamwork_feat_share = library.CopyAndAdd<BlueprintFeature>("e1f437048db80164792155102375b62c", "TwinnedSummonerTeamworkFeatShareFeature", "");
            teamwork_feat_share.SetNameDescription("Teamwork Feat Sharing", "At 4th level and at 12th level, a twinned summoner gains a bonus teamwork feat. He must meet the prerequisites of the selected teamwork feat. The twinned summoner automatically grants all of his teamwork feats to his eidolon. The eidolon doesn’t need to meet the prerequisites of these teamwork feats.");

            teamwork_feat = library.CopyAndAdd<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb", "TwinnedSummonerTeamworkFeatFeatureSelection", "");
            teamwork_feat.SetDescription(teamwork_feat_share.Description);


            twinned_summoner = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "TwinnedSummonerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Twinned Summoner");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Reflecting their connected nature, a twinned summoner and his eidolon appear identical save for any obvious differences, such as evolutions or equipment.");
            });
            Helpers.SetField(twinned_summoner, "m_ParentClass", summoner_class);
            library.AddAsset(twinned_summoner, "");

            twinned_eidolon_selection = library.CopyAndAdd(eidolon_selection, "TwinnedEidolonFeatureSelection", "07464c5f4c5e4dd79120db73a3cfd597");
            twinned_eidolon_selection.SetNameDescriptionIcon(Eidolon.twinned_eidolon);
            twinned_eidolon_selection.AllFeatures = new BlueprintFeature[] { Eidolon.twinned_eidolon, Eidolon.twinned_eidolon_small };
            twinned_eidolon_selection.AddComponent(Helpers.Create<CompanionMechanics.ChangeCompanionAlignmentToMasters>());

            twinned_summoner.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, eidolon_selection),
                                                             Helpers.LevelEntry(4, shield_ally),
                                                             Helpers.LevelEntry(6, makers_call),
                                                             Helpers.LevelEntry(8, transposition),
                                                             Helpers.LevelEntry(12, greater_shield_ally),
                                                           };

            twinned_summoner.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, twinned_eidolon_selection),
                                                             Helpers.LevelEntry(4, teamwork_feat_share, teamwork_feat),
                                                             Helpers.LevelEntry(6, twinned_transposition),
                                                             Helpers.LevelEntry(8, swift_twinned_transposition),
                                                             Helpers.LevelEntry(12, teamwork_feat),
                                                        };

            summoner_progression.UIDeterminatorsGroup = summoner_progression.UIDeterminatorsGroup.AddToArray(twinned_eidolon_selection);
            summoner_progression.UIGroups[0].Features.Add(teamwork_feat_share);
            summoner_progression.UIGroups[0].Features.Add(twinned_transposition);
            summoner_progression.UIGroups[0].Features.Add(swift_twinned_transposition);


        }

        static void createMasterSummoner()
        {
            master_summoner = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MasterSummonerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Master Summoner");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Most summoners forge a bond with a single, powerful eidolon, but some summoners seek to control a variety of creatures. The master summoner sacrifices the power of his eidolon in favor of summoning a plethora of otherworldly creatures to aid him.");
            });
            Helpers.SetField(master_summoner, "m_ParentClass", summoner_class);
            library.AddAsset(master_summoner, "");

            createLesserEidolon();
            createMasterSummons();


            master_summoner.RemoveFeatures = new LevelEntry[20];
            master_summoner.RemoveFeatures[0] = Helpers.LevelEntry(1, eidolon_selection);
            master_summoner.RemoveFeatures[3] = Helpers.LevelEntry(4, shield_ally);
            master_summoner.RemoveFeatures[11] = Helpers.LevelEntry(12, greater_shield_ally);
            master_summoner.AddFeatures = new LevelEntry[20];

            for (int i = 0; i < 20; i++)
            {
                if (master_summoner.RemoveFeatures[i] == null)
                {
                    master_summoner.RemoveFeatures[i] = Helpers.LevelEntry(i + 1);
                }
                master_summoner.RemoveFeatures[i].Features.Add(evolution_distribution[i]);
                if (i == 0)
                {
                    master_summoner.AddFeatures[i] = Helpers.LevelEntry(i + 1, lesser_eidolon_selection, lesser_evolution_distribution[i]);
                }
                else
                {
                    master_summoner.AddFeatures[i] = Helpers.LevelEntry(i + 1, lesser_evolution_distribution[i]);
                }
            }

            for (int i = 0; i < 9; i++)
            {
                master_summoner.RemoveFeatures[i * 2].Features.Add(summon_monster[i]);
                master_summoner.AddFeatures[i * 2].Features.Add(master_summoner_summon_monster[i]);
            }

            var augment_summoning = library.Get<BlueprintFeature>("38155ca9e4055bb48a89240a2055dcc3");
            var master_augment_summoning = Helpers.CreateFeature("MasterSummonerAugmentSummoning",
                                                                 augment_summoning.Name,
                                                                 "At 2nd level, a master summoner gains Augment Summoning as a bonus feat. He does not have to meet any requirements for this feat.",
                                                                 "",
                                                                 augment_summoning.Icon,
                                                                 FeatureGroup.None,
                                                                 Helpers.CreateAddFact(augment_summoning)
                                                                 );

            master_summoner.AddFeatures[1].Features.Add(master_augment_summoning);

            summoner_class.Progression.UIGroups[0].Features.Add(master_augment_summoning);
            summoner_class.Progression.UIGroups = summoner_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(master_summoner_summon_monster));
            summoner_class.Progression.UIDeterminatorsGroup = summoner_class.Progression.UIDeterminatorsGroup.AddToArray(lesser_eidolon_selection);
        }


        static void createMasterSummons()
        {
            var mt_feats = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("c19f6f34ed0bc364cbdec88b49a54f67"),
                library.Get<BlueprintFeature>("45e466127a8961d40bb3030816ed245b"),
                library.Get<BlueprintFeature>("ea26b3a3acb98074fa34f80fcc4e497d"),
                library.Get<BlueprintFeature>("03168f4f13ff26f429d912085e88baba"),
                library.Get<BlueprintFeature>("00fda605a917fcc4e89612dd31683bdd"),
                library.Get<BlueprintFeature>("9b14b05456142914888a48354a0eec17"),
                library.Get<BlueprintFeature>("667fd017406abd548b89292edd7dbfb7"),
                library.Get<BlueprintFeature>("20d72612311ba914aaba5cc8a4cf312c"),
                library.Get<BlueprintFeature>("f63d23b4e41b3264fa6aa2be8079d28d")
            };

            master_summon_resource = Helpers.CreateAbilityResource("MasterSummonnerSummonResource", "", "", "", null);
            master_summon_resource.SetIncreasedByStat(5, StatType.Charisma);
            var description = "Starting at 1st level, a master summoner can cast summon monster I as a spell-like ability a number of times per day equal to 5 + his Charisma modifier. The summoner can use this ability when his eidolon is summoned. Only one summon monster spell may be in effect while the eidolon is summoned. If the summoner’s eidolon is not summoned, the number of creatures that can be summoned with this ability is only limited by its uses per day. This ability otherwise functions as the summoner’s normal summon monster I ability. Other than these restrictions, there is no limit to how many summon monster spells the summoner can have active at one time.";
            master_summoner_free_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", "MasterSummonerSummonPool", "");

            for (int i = 0; i < mt_feats.Length; i++)
            {
                List<BlueprintAbility> summon_spells = new List<BlueprintAbility>();
                foreach (var f in mt_feats[i].GetComponent<AddFacts>().Facts)
                {
                    var ability = library.CopyAndAdd<BlueprintAbility>(f.AssetGuid, f.name.Replace("MonsterTactician", "MasterSummoner"), "");
                    ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = master_summon_resource);
                    foreach (var c in ability.GetComponents<ContextRankConfig>())
                    {
                        if (c.IsBasedOnClassLevel)
                        {
                            var new_c = c.CreateCopy(crc => Helpers.SetField(crc, "m_Class", getSummonerArray()));
                            ability.ReplaceComponent(c, new_c);
                        }
                    }

                    var clear_action = Helpers.CreateConditional(Helpers.Create<CompanionMechanics.ContextActionPetIsAlive>(),
                                                                 Helpers.Create<ContextActionClearSummonPool>(c => c.SummonPool = summon_pool));
                    var new_actions = Common.replaceActions<ContextActionClearSummonPool>(ability.GetComponent<AbilityEffectRunAction>().Actions.Actions, clear_action);

                    new_actions = Common.replaceActions<ContextActionSpawnMonster>(new_actions, s =>
                    {
                        if (s.SummonPool == summon_pool || s.SummonPool == master_summoner_free_pool)
                        {
                            return s;
                        }
                        var summon_free = s.CreateCopy(sm => sm.SummonPool = master_summoner_free_pool);
                        var summon_normal = s.CreateCopy(sm => sm.SummonPool = summon_pool);
                        var summon2 = Helpers.CreateConditional(Helpers.Create<NewMechanics.HasUnitsInSummonPool>(h => h.SummonPool = master_summoner_free_pool),
                                                                summon_normal, summon_free);
                        return summon2;
                    }
                    );
                                                                                                      
                    ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(new_actions));
                    ability.AddComponent(Helpers.Create<CompanionMechanics.AbilityCasterCompanionDeadOrSummonPoolEmpty>(a => a.SummonPool = master_summoner_free_pool));
                    summon_spells.Add(ability);
                }

                BlueprintAbility summon_base = null;
                if (summon_spells.Count == 1)
                {
                    summon_base = summon_spells[0];
                }
                else
                {
                    summon_base = Common.createVariantWrapper($"MasterSummonerSummon{i + 1}Base", "", summon_spells.ToArray());
                    summon_base.SetNameDescription("Summon Monster " + Common.roman_id[i + 1], description);
                }

                master_summoner_summon_monster[i] = Helpers.CreateFeature($"MasterSummonerSummonMonster{i + 1}Feature",
                                                                          "Summon Monster " + Common.roman_id[i + 1],
                                                                          description,
                                                                          "",
                                                                          summon_spells[0].Icon,
                                                                          FeatureGroup.None,
                                                                          Helpers.CreateAddFact(summon_base)
                                                                          );
                if (i == 0)
                {
                    master_summoner_summon_monster[i].AddComponent(master_summon_resource.CreateAddAbilityResource());
                }
            }
        }


        static void createLesserEidolon()
        {
            var summoner_eidolon_rank_progression = library.CopyAndAdd<BlueprintProgression>("125af359f8bc9a145968b5d8fd8159b8", "SummonerLesserEidolonProgression", "");
            summoner_eidolon_rank_progression.Classes = getSummonerArray();
            summoner_eidolon_rank_progression.LevelEntries = summoner_eidolon_rank_progression.LevelEntries.Where(x => x.Level > 2 && (x.Level % 2 == 0)).ToArray();
            lesser_eidolon_selection = Helpers.CreateFeatureSelection("LesserEidolonFeatureSelection",
                                                                  "Lesser Eidolon",
                                                                  "A master summoner’s class level is halved (minimum 1) for the purposes of determining his eidolon’s abilities, Hit Dice, evolution pool, and so on. The eidolon otherwise functions as normal.",
                                                                  "",
                                                                  null,
                                                                  FeatureGroup.AnimalCompanion,
                                                                  Helpers.Create<AddFeatureOnApply>(a => a.Feature = summoner_eidolon_rank_progression),
                                                                  Helpers.Create<AddFeatureOnApply>(a => a.Feature = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d"))
                                                                  );
            lesser_eidolon_selection.AllFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("472091361cf118049a2b4339c4ea836a") }; //empty companion
            lesser_eidolon_selection.AllFeatures = lesser_eidolon_selection.AllFeatures.AddToArray(Eidolon.getLesserEidolons());
            lesser_eidolon_selection.AddComponents(eidolon_selection.GetComponents<AddFacts>().ToArray());

            for (int lvl = 1; lvl <= 20; lvl++)
            {
                var feature = Helpers.CreateFeature($"SummonerLesserEvolutionDistribution{lvl}Feature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None);
                feature.AddComponent(Helpers.Create<EvolutionMechanics.RefreshEvolutionsOnLevelUp>());
                int half_level = Math.Max(1, lvl / 2);
                int prev_half_level = lvl == 2 ? 1 : (lvl - 1) / 2;
                int bonus_ep = Eidolon.EidolonComponent.rank_to_level[half_level] - Eidolon.EidolonComponent.rank_to_level[prev_half_level];
                if (bonus_ep > 0)
                {
                    feature.AddComponent(Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(n => n.amount = bonus_ep));
                }
                feature.AddComponent(Helpers.Create<EvolutionMechanics.addEvolutionSelection>(a => a.selection = Evolutions.evolution_selection));

                feature.HideInCharacterSheetAndLevelUp = true;
                feature.HideInUI = true;

                lesser_evolution_distribution.Add(feature);
            }
        }


        static void createNaturalist()
        {
            var acid_maw = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67");
            naturalist = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "NaturalistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Naturalist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A naturalist is a summoner who is in tune with the natural world, using his magic like a lens to focus various animal aspects onto his eidolon. More akin to a hunter than to other arcane spellcasters, a naturalist instinctively masters the power of such creatures as the bear, wolf, mouse, or tiger to make his exotic eidolon the perfect living tool for battle or stealth, and he eventually discovers how to apply these transformations to himself as well.");
            });
            Helpers.SetField(naturalist, "m_ParentClass", summoner_class);
            library.AddAsset(naturalist, "");


            animal_focus_engine = new AnimalFocusEngine();
            animal_focus_engine.initialize(getSummonerArray(), naturalist, 2, "Naturalist");

            var animal_focus_additional_use_component = Helpers.Create<Kingmaker.UnitLogic.FactLogic.IncreaseActivatableAbilityGroupSize>();
            animal_focus_additional_use_component.Group = ActivatableAbilityGroupExtension.AnimalFocus.ToActivatableAbilityGroup();

            var animal_focus_additional_use = Helpers.CreateFeature("NaturalistAdditionalAnimalFocusFeature",
                                                             "Second Animal Focus",
                                                             "At 10th level, whenever a naturalist uses animal focus, he may apply two different animal aspects to his eidolon. The eidolon’s form gains superficial physical charges appropriate to the chosen aspect. This replaces the aspect summoner class ability.",
                                                             "",
                                                             acid_maw.Icon,
                                                             FeatureGroup.None,
                                                             animal_focus_additional_use_component
                                                            );
            animal_focus_additional_use.Ranks = 1;
            var animal_focus_additional_use2 = library.CopyAndAdd<BlueprintFeature>(animal_focus_additional_use.AssetGuid, "NaturalistAdditionalAnimalFocus2Feature", "");
            animal_focus_additional_use2.SetNameDescription("Third Animal Focus", "At 18th level, whenever a naturalist uses his animal focus ability, he can apply three different animal aspects to his eidolon");

            var animal_focus = animal_focus_engine.createAnimalFocus();
            var animal_focus_ac = Helpers.CreateFeature("NaturalistAnimalFocusAcFeature",
                                                        "Animal Focus",
                                                        "At 4th level, as a swift action a naturalist can enhance his eidolon with the aspect of an animal. Each time he uses this ability, he can select a hunter’s animal aspect and apply it to his eidolon. His hunter level for this ability is equal to his summoner level – 2. He does not gain the ability to add an animal aspect to himself (see Shared Focus, below). This effect lasts until the eidolon is dismissed or sent back to its home plane.",
                                                        "",
                                                        acid_maw.Icon,
                                                        FeatureGroup.None,
                                                        Common.createAddFeatToAnimalCompanion(animal_focus)
                                                        );
            var animal_focus_additional_use_ac = Helpers.CreateFeature("NaturalistAdditonalAnimalFocusAcFeature",
                                                                        animal_focus_additional_use.Name,
                                                                        animal_focus_additional_use.Description,
                                                                        "",
                                                                        acid_maw.Icon,
                                                                        FeatureGroup.None,
                                                                        Common.createAddFeatToAnimalCompanion(animal_focus_additional_use)
                                                                        );

           var animal_focus_additional_use_ac2 = Helpers.CreateFeature("NaturalistAdditonalAnimalFocusAc2Feature",
                                                                        animal_focus_additional_use2.Name,
                                                                        animal_focus_additional_use2.Description,
                                                                        "",
                                                                        acid_maw.Icon,
                                                                        FeatureGroup.None,
                                                                        Common.createAddFeatToAnimalCompanion(animal_focus_additional_use2)
                                                                        );

            var shared_focus = library.CopyAndAdd<BlueprintFeature>(animal_focus, "NaturalistSharedFocus", "");
            shared_focus.SetNameDescription("Shared Focus", "At 14th level, the naturalist begins to take on some of the feral nature of his eidolon. He can apply one animal focus to himself.");

            naturalist.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, summon_monster[0]),
                                                             Helpers.LevelEntry(3, summon_monster[1]),
                                                             Helpers.LevelEntry(4, shield_ally),
                                                             Helpers.LevelEntry(5, summon_monster[2]),
                                                             Helpers.LevelEntry(7, summon_monster[3]),
                                                             Helpers.LevelEntry(9, summon_monster[4]),
                                                             Helpers.LevelEntry(10, aspect_extra[0]),
                                                             Helpers.LevelEntry(11, summon_monster[5], aspect_extra[1]),
                                                             Helpers.LevelEntry(12, greater_shield_ally, aspect_extra[2]),
                                                             Helpers.LevelEntry(13, summon_monster[6], aspect_extra[3]),
                                                             Helpers.LevelEntry(14, life_bond, aspect_extra[4]),
                                                             Helpers.LevelEntry(15, summon_monster[7], aspect_extra[5]),
                                                             Helpers.LevelEntry(16, aspect_extra[6]),
                                                             Helpers.LevelEntry(17, summon_monster[8], aspect_extra[7]),
                                                             Helpers.LevelEntry(18, greater_aspect_extra[0]),
                                                             Helpers.LevelEntry(19, greater_aspect_extra[1]),
                                                             Helpers.LevelEntry(20, greater_aspect_extra[2])
                                                           };

            naturalist.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, summon_nature_ally[0]),
                                                             Helpers.LevelEntry(3, summon_nature_ally[1]),
                                                             Helpers.LevelEntry(4, animal_focus_ac),
                                                             Helpers.LevelEntry(5, summon_nature_ally[2]),
                                                             Helpers.LevelEntry(7, summon_nature_ally[3]),
                                                             Helpers.LevelEntry(9, summon_nature_ally[4]),
                                                             Helpers.LevelEntry(10, animal_focus_additional_use_ac),
                                                             Helpers.LevelEntry(11, summon_nature_ally[5]),
                                                             Helpers.LevelEntry(13, summon_nature_ally[6]),
                                                             Helpers.LevelEntry(14, shared_focus),
                                                             Helpers.LevelEntry(15, summon_nature_ally[7]),
                                                             Helpers.LevelEntry(17, summon_nature_ally[8]),
                                                             Helpers.LevelEntry(18, animal_focus_additional_use_ac2),
                                                        };


            summoner_class.Progression.UIGroups = summoner_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(animal_focus_ac, animal_focus_additional_use_ac, shared_focus, animal_focus_additional_use_ac2));

            var planar_focus = animal_focus_engine.createPlanarFocus("Naturalist", animal_focus_ac);
            planar_focus.AddComponents(Common.createPrerequisiteArchetypeLevel(summoner_class, naturalist, 14));

        }


        static void createFeyCaller()
        {
            fey_caller = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "FeyCallerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Fey Caller");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The strange creatures known as fey hail from a variety of places, including the natural world and their own domains on other planes. Fey callers are summoners whose voices reach out into those strange faerie realms and call forth the fey who dwell there. The touch of the fey makes the fey caller different from those summoners who call other outsiders to their sides as eidolons.");
            });
            Helpers.SetField(fey_caller, "m_ParentClass", summoner_class);
            library.AddAsset(fey_caller, "");

            createSummonNatureAlly();

            fey_caller.ReplaceClassSkills = true;
            fey_caller.ClassSkills = new StatType[] { StatType.SkillLoreNature, StatType.SkillLoreReligion, StatType.SkillPersuasion, StatType.SkillUseMagicDevice };

            fey_eidolon_selection = library.CopyAndAdd(eidolon_selection, "FeyEidolonSelection", "e054d599ec2f4b7e99a43c739a44b1fd");
            fey_eidolon_selection.SetNameDescriptionIcon(Eidolon.fey_eidolon);
            fey_eidolon_selection.AllFeatures = new BlueprintFeature[] { Eidolon.fey_eidolon };
            fey_eidolon_selection.HideInCharacterSheetAndLevelUp = true;

            fey_caller.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, eidolon_selection, summon_monster[0]),
                                                             Helpers.LevelEntry(3, summon_monster[1]),
                                                             Helpers.LevelEntry(5, summon_monster[2]),
                                                             Helpers.LevelEntry(7, summon_monster[3]),
                                                             Helpers.LevelEntry(9, summon_monster[4]),
                                                             Helpers.LevelEntry(11, summon_monster[5]),
                                                             Helpers.LevelEntry(13, summon_monster[6]),
                                                             Helpers.LevelEntry(15, summon_monster[7]),
                                                             Helpers.LevelEntry(17, summon_monster[8])
                                                           };
            fey_caller.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, fey_eidolon_selection, summon_nature_ally[0]),
                                                             Helpers.LevelEntry(3, summon_nature_ally[1]),
                                                             Helpers.LevelEntry(5, summon_nature_ally[2]),
                                                             Helpers.LevelEntry(7, summon_nature_ally[3]),
                                                             Helpers.LevelEntry(9, summon_nature_ally[4]),
                                                             Helpers.LevelEntry(11, summon_nature_ally[5]),
                                                             Helpers.LevelEntry(13, summon_nature_ally[6]),
                                                             Helpers.LevelEntry(15, summon_nature_ally[7]),
                                                             Helpers.LevelEntry(17, summon_nature_ally[8])
                                                        };

            summoner_class.Progression.UIDeterminatorsGroup = summoner_class.Progression.UIDeterminatorsGroup.AddToArray(fey_eidolon_selection);
            summoner_class.Progression.UIGroups = summoner_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(summon_nature_ally));
        }


        static void createSummonNatureAlly()
        {
            var spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("c6147854641924442a3bb736080cfeb6"),
                library.Get<BlueprintAbility>("298148133cdc3fd42889b99c82711986"),
                library.Get<BlueprintAbility>("fdcf7e57ec44f704591f11b45f4acf61"),
                library.Get<BlueprintAbility>("c83db50513abdf74ca103651931fac4b"),
                library.Get<BlueprintAbility>("8f98a22f35ca6684a983363d32e51bfe"),
                library.Get<BlueprintAbility>("55bbce9b3e76d4a4a8c8e0698d29002c"),
                library.Get<BlueprintAbility>("051b979e7d7f8ec41b9fa35d04746b33"),
                library.Get<BlueprintAbility>("ea78c04f0bd13d049a1cce5daf8d83e0"),
                library.Get<BlueprintAbility>("a7469ef84ba50ac4cbf3d145e3173f8e")
            };

            var description = "Instead of summoning creatures from the summon monster list, the naturalist's and fey caller’s summoning spell-like ability summons creatures from the list for the summon nature’s ally spell of the same level. It still follows the other rules and restrictions for the summoner’s summon monster spell-like ability.";


            for (int i = 0; i < spells.Length; i++)
            {
                List<BlueprintAbility> summon_spells = new List<BlueprintAbility>();
                List<BlueprintAbility> sna_spells = new List<BlueprintAbility>();
                if (spells[i].HasVariants)
                {
                    sna_spells = spells[i].Variants.ToList();
                }
                else
                {
                    sna_spells.Add(spells[i]);
                }

                foreach (var s in sna_spells)
                {
                    var ability = library.CopyAndAdd<BlueprintAbility>(s.AssetGuid, "FeyCaller" + s.name, "");
                    ability.RemoveComponents<SpellListComponent>();
                    ability.AddComponent(summon_resource.CreateResourceLogic());
                    foreach (var c in ability.GetComponents<ContextRankConfig>())
                    {
                        if (!c.IsFeatureList)
                        {
                            var new_c = c.CreateCopy(crc => { Helpers.SetField(crc, "m_Class", getSummonerArray()); Helpers.SetField(crc, "m_BaseValueType", ContextRankBaseValueType.ClassLevel); });
                            ability.ReplaceComponent(c, new_c);
                        }
                    }
                    var new_actions = Common.changeAction<ContextActionSpawnMonster>(ability.GetComponent<AbilityEffectRunAction>().Actions.Actions, a => 
                                                                                        {
                                                                                            a.SummonPool = summon_pool;
                                                                                            a.DurationValue = Helpers.CreateContextDuration(a.DurationValue.BonusValue, DurationRate.Minutes);
                                                                                        }
                                                                                    );
                    new_actions = new GameAction[] { Helpers.Create<ContextActionClearSummonPool>(c => c.SummonPool = summon_pool) }.AddToArray(new_actions);
                    ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(new_actions));
                    ability.AddComponent(Helpers.Create<NewMechanics.AbilityCasterCompanionDead>());
                    ability.LocalizedDuration = Helpers.minutesPerLevelDuration;
                    summon_spells.Add(ability);
                    Common.unsetAsFullRoundAction(ability);
                }

                BlueprintAbility summon_base = null;
                if (summon_spells.Count == 1)
                {
                    summon_base = summon_spells[0];
                }
                else
                {
                    summon_base = Common.createVariantWrapper($"SummonerSummonNaturesAlly{i + 1}Base", "", summon_spells.ToArray());
                    summon_base.SetNameDescription("Summon Nature's Ally " + Common.roman_id[i + 1], description);
                }

                summon_nature_ally[i] = Helpers.CreateFeature($"SummonerSummonNaturesAlly{i + 1}Feature",
                                                          "Summon Nature's Ally " + Common.roman_id[i + 1],
                                                          description,
                                                          "",
                                                          summon_spells[0].Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFact(summon_base)
                                                          );
                if (i == 0)
                {
                    summon_nature_ally[i].AddComponent(summon_resource.CreateAddAbilityResource());
                }
            }
        }



        static void createDevilBinder()
        {
            devil_binder = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DevilBinderArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Devil Binder");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While many assume Hellknights are worshipers or agents of devil-kind or, at best, duped puppets of the infernal realm-most Hellknights see Hell and its native devils as a challenge to be conquered. If one can dominate these beings of pure (albeit cruel) law, then the power of that law is theirs by right. While the Test undertaken by those who seek to become full Hellknights is one method by which this power is earned, the devil binder takes this philosophy a step further, forcing a devil into service.\n"
                                                                                        + "Unlike those of most summoners, a devil binder’s eidolon does not willingly serve its master due to a similar philosophical bent.While some devil binders (especially those who do serve Hell) seek more amicable relationships with these infernal servants, others treat their eidolons as mere tools, remorselessly ordering the devils into battle. The nature of the summoner’s magical bond is irrevocably altered by their style of invocation. A devil binder borrows extraplanar quintessence from her eidolon, gaining strange abilities for herself and evolving her eidolon’s magical powers.");
            });
            Helpers.SetField(devil_binder, "m_ParentClass", summoner_class);
            library.AddAsset(devil_binder, "");

            createInfernalAffinity();
            createInfernalArcana();
            createSmiteChaos();
            createDevilBinderCharismaBonus();

            infernal_eidolon_selection = library.CopyAndAdd(eidolon_selection, "InfernalEidolonSelection", "0b7e1736275948cb805b58451a0a3715");
            infernal_eidolon_selection.SetNameDescriptionIcon(Eidolon.infernal_eidolon);
            infernal_eidolon_selection.HideInCharacterSheetAndLevelUp = true;
            infernal_eidolon_selection.AllFeatures = new BlueprintFeature[] { Eidolon.infernal_eidolon };

            devil_binder.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, eidolon_selection, summon_monster[0]),
                                                             Helpers.LevelEntry(3, summon_monster[1]),
                                                             Helpers.LevelEntry(4, shield_ally),
                                                             Helpers.LevelEntry(5, summon_monster[2]),
                                                             Helpers.LevelEntry(6, makers_call),
                                                             Helpers.LevelEntry(7, summon_monster[3]),
                                                             Helpers.LevelEntry(8, transposition),
                                                             Helpers.LevelEntry(9, summon_monster[4]),
                                                             Helpers.LevelEntry(11, summon_monster[5]),
                                                             Helpers.LevelEntry(12, greater_shield_ally),
                                                             Helpers.LevelEntry(13, summon_monster[6]),
                                                             Helpers.LevelEntry(15, summon_monster[7]),
                                                             Helpers.LevelEntry(17, summon_monster[8]),
                                                             Helpers.LevelEntry(20, twin_eidolon)
                                                           };
            devil_binder.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, infernal_eidolon_selection, infernal_arcana[0]),
                                                          Helpers.LevelEntry(3, infernal_arcana[1]),
                                                          Helpers.LevelEntry(4, infernal_affinity[0], devil_binder_charisma_bonus),
                                                          Helpers.LevelEntry(5, infernal_arcana[2]),
                                                          Helpers.LevelEntry(6, devil_binder_smite_chaos),
                                                          Helpers.LevelEntry(7, infernal_arcana[3]),
                                                          Helpers.LevelEntry(8, infernal_affinity[1]),
                                                          Helpers.LevelEntry(9, infernal_arcana[4]),
                                                          Helpers.LevelEntry(11, infernal_arcana[5]),
                                                          Helpers.LevelEntry(12, infernal_affinity[2]),
                                                          Helpers.LevelEntry(13, infernal_arcana[6]),
                                                          Helpers.LevelEntry(15, infernal_arcana[7]),
                                                          Helpers.LevelEntry(16, infernal_affinity[3]),
                                                          Helpers.LevelEntry(17, infernal_arcana[8]),
                                                          Helpers.LevelEntry(19, infernal_arcana[9]),
                                                          Helpers.LevelEntry(20, infernal_affinity[4])
                                                        };

            summoner_class.Progression.UIDeterminatorsGroup = summoner_class.Progression.UIDeterminatorsGroup.AddToArray(infernal_eidolon_selection);
            summoner_class.Progression.UIGroups = summoner_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(infernal_affinity));
            summoner_class.Progression.UIGroups = summoner_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(infernal_arcana));
            summoner_class.Progression.UIGroups = summoner_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(devil_binder_charisma_bonus, devil_binder_smite_chaos));
        }


        static void createDevilBinderCharismaBonus()
        {
            var charisma_bonus = Helpers.CreateFeature("InfernalEidolonCharismaBonus",
                                                                "Eidolon Charisma Bonus",
                                                                "At 4th level and every 4 levels thereafter, the eidolon’s Charisma score increases by 2.",
                                                                "",
                                                                null,
                                                                FeatureGroup.None,
                                                                Helpers.CreateAddContextStatBonus(StatType.Charisma, ModifierDescriptor.UntypedStackable, multiplier: 2),
                                                                Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterClassLevel.ToContextRankBaseValueType(),
                                                                                                classes: getSummonerArray(), progression: ContextRankProgression.StartPlusDivStep,
                                                                                                startLevel: 4, stepLevel: 4)
                                                               );
            devil_binder_charisma_bonus = Common.createAddFeatToAnimalCompanion(charisma_bonus, "");
        }


        static void createSmiteChaos()
        {
            var smite_chaos = Common.createSmite("DevilBinderSmiteChaos",
                                     "Smite Chaos",
                                     "At 6th level, the summoner’s eidolon gains the ability to smite chaos as a Hellknight of its Hit Dice.  As a swift action, the eidolon chooses one target within sight to smite. If this target is of chaotic alignment, the eidolon adds its Charisma bonus (if any) to its attack rolls and adds its HD to all damage rolls made against the target of its smite. Smite attacks automatically bypass any DR the target possesses.\n"
                                     + "In addition, while smite is in effect, the eidolon gains a deflection bonus equal to its Charisma modifier (if any) to its AC against attacks made by the target of the smite. If the smite targets a creature that’s not of the chaotic alignment, the smite is wasted with no effect.\n"
                                     + "The smite effect remains until the target of the smite is dead or the next time the eidolon rests. The eidolon can use this ability once per day, plus one additional time per day for every 4 levels beyond 6th (to a maximum of five times per day at 18th level).",
                                     "",
                                     "",
                                     Helpers.GetIcon("474ed0aa656cc38499cc9a073d113716"),
                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { Eidolon.eidolon_class }),
                                     AlignmentComponent.Chaotic);
            var smite_resource = Helpers.CreateAbilityResource("SmiteResource", "", "", "", null);
            smite_resource.SetFixedResource(1);
            var smite_ability = smite_chaos.GetComponent<AddFacts>().Facts.First() as BlueprintAbility;
            smite_chaos.ReplaceComponent<AddAbilityResources>(c => c.Resource = smite_resource);
            smite_ability.ReplaceComponent<AbilityResourceLogic>(c => c.RequiredResource = smite_resource);
            smite_chaos.AddComponents(Helpers.Create<IncreaseResourceAmountBySharedValue>(i => { i.Resource = smite_resource; i.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                     Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterClassLevel.ToContextRankBaseValueType(), classes: getSummonerArray(),
                                                                     progression: ContextRankProgression.DelayedStartPlusDivStep, startLevel: 10, stepLevel: 4)
                                    );
            devil_binder_smite_chaos = Common.createAddFeatToAnimalCompanion(smite_chaos, "");
        }

        static void createInfernalAffinity()
        {
            var corrupting_touch = library.CopyAndAdd<BlueprintAbility>("ec78fc025b797b1459786f012b92e1ad", "DevilBinderCorruptingTouch", "");
            corrupting_touch.ReplaceComponent<ContextRankConfig>(c => { Helpers.SetField(c, "m_Class", getSummonerArray()); Helpers.SetField(c, "Archetype", devil_binder); });
            var corrupting_touch_resource = library.Get<BlueprintAbilityResource>("1c55aba4475e93440ade11d7076703d4");
            corrupting_touch.RemoveComponents<AbilityResourceLogic>();
            var corrupting_touch_cast = Helpers.CreateTouchSpellCast(corrupting_touch);
            corrupting_touch_cast.AddComponent(corrupting_touch_resource.CreateResourceLogic());
            var corrupting_touch_feature = library.CopyAndAdd<BlueprintFeature>("de7fc4b905e3c434a8f8eaac80195b64", "DevilBinderCorruptingTouchFeature", "");
            corrupting_touch_feature.ReplaceComponent<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { corrupting_touch_cast });
            corrupting_touch.SetDescription("You can cause a creature to become shaken as a melee touch attack. This effect persists for a number of rounds equal to 1/2 your summoner level (minimum 1). Multiple touches do not stack, but they do add to the duration. You can use this ability a number of times per day equal to 3 + your Charisma modifier.");
            corrupting_touch_feature.SetDescription(corrupting_touch.Description);
            var infernal_resistances = Helpers.CreateFeature("DevilBinderInfernalResistances",
                                                             "Infernal Resistances",
                                                             "You gain resist fire 5 and a +2 bonus on saving throws made against poison. At 9th level, your resistance to fire increases to 10 and your bonus on poison saving throws increases to +4.",
                                                             "",
                                                             Helpers.GetIcon("ddfb4ac970225f34dbff98a10a4a8844"),
                                                             FeatureGroup.None,
                                                             Common.createEnergyDRContextRank(DamageEnergyType.Fire, AbilityRankType.Default, multiplier: 5),
                                                             Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilitySharedValue.StatBonus), ModifierDescriptor.UntypedStackable, SpellDescriptor.Poison),
                                                             Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getSummonerArray(),
                                                                                             progression: ContextRankProgression.OnePlusDivStep, stepLevel: 9,
                                                                                             max: 2),
                                                             Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.One, Helpers.CreateContextValue(AbilityRankType.Default), Helpers.CreateContextValue(AbilityRankType.Default)), AbilitySharedValue.StatBonus)
                                                             );
            var hellfire_resource = Helpers.CreateAbilityResource("DevilBinderHellfireResource", "", "", "", null);
            hellfire_resource.SetIncreasedByLevelStartPlusDivStep(1, 17, 1, 3, 1, 0, 0.0f, getSummonerArray());
            var hellfire_ability = library.CopyAndAdd<BlueprintAbility>("87e837a180a12db448a6d78e58e1b0a6", "DevinlBinderHellfireAbility", "");
            hellfire_ability.SetDescription($"You can call down a column of hellfire. This 10 - foot - radius burst does 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of fire damage per sorcerer level. Those caught in the area of your blast receive a Reflex save for half damage. Good creatures that fail their saves are shaken for a number of rounds equal to your sorcerer level. The DC of this save is equal to 10 + 1/2 your summoner level + your Charisma modifier. At 12th level, you can use this ability once per day. At 17th level, you can use this ability twice per day. At 20th level, you can use this ability three times per day. This power has a range of 60 feet.");

            hellfire_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = hellfire_resource);
            hellfire_ability.ReplaceComponent<ContextRankConfig>(c => { Helpers.SetField(c, "m_Class", getSummonerArray()); Helpers.SetField(c, "Archetype", devil_binder); });
            hellfire_ability.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClass(summoner_class, StatType.Charisma));
            var hellfire = Common.AbilityToFeature(hellfire_ability, false);
            hellfire.AddComponent(hellfire_resource.CreateAddAbilityResource());

            var on_dark_wings = library.Get<BlueprintFeature>("85243106886a0384380d5b5f04046909");
            var power_of_the_pit = library.Get<BlueprintFeature>("b6afdc50876e08149b1f9fdcdb2a308c");


            infernal_affinity = new BlueprintFeature[] { corrupting_touch_feature, infernal_resistances, hellfire, on_dark_wings, power_of_the_pit };
        }


        static void createInfernalArcana()
        {          
            var spells = new BlueprintAbility[] {library.Get<BlueprintAbility>("1eaf1020e82028d4db55e6e464269e00"), //protection chaos
                                                 library.Get<BlueprintAbility>("cdb106d53c65bbc4086183d54c3b97c7"), //scorching ray
                                                 library.Get<BlueprintAbility>("c7104f7526c4c524f91474614054547e"), //hold person
                                                 library.Get<BlueprintAbility>("4baf4109145de4345861fe0f2209d903"), //crushing despair
                                                 library.Get<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61"), //dominate person
                                                 library.Get<BlueprintAbility>("02de4dd8add69aa42a3d1330b573e2ab"), //summon soul eater 
                                                 library.Get<BlueprintAbility>("98734a2665c18cd4db71878b0532024a"), //firebrand
                                                 library.Get<BlueprintAbility>("f958ef62eea5050418fb92dfa944c631"), //power word stun
                                                 NewSpells.meteor_swarm};

            infernal_arcana = new BlueprintFeature[spells.Length + 1];
            var resource_bonus = Helpers.CreateFeature("InfernalArcanaExtraUseFeature",
                                                       "Infernal Arcana Extra Use",
                                                       "At 19th level, the devil binder’s eidolon can use each of its infernal arcana spell-like abilities three times per day.",
                                                       "",
                                                       null,
                                                       FeatureGroup.None);

            for (int i = 0; i < spells.Length; i++)
            {
                var resource = Helpers.CreateAbilityResource("InfernalArcana" + spells[i].name + "Resource", "", "", "", null);
                resource.SetFixedResource(1);
                var spell_like = Common.convertToSpellLike(spells[i], "InfernalArcana", new BlueprintCharacterClass[] { Eidolon.eidolon_class }, StatType.Charisma,
                                                           resource, no_scaling: true);
                spell_like.AddComponent(Helpers.Create<ContextCalculateAbilityParams>(c => { /*c.ReplaceCasterLevel = true; c.CasterLevel = Helpers.CreateContextValue(AbilityRankType.SpeedBonus);*/ c.StatType = StatType.Charisma; c.ReplaceSpellLevel = true; c.SpellLevel = i + 1; }));

                var spell_feature = Common.AbilityToFeature(spell_like, false);
                spell_feature.AddComponent(Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => { c.spells = new BlueprintAbility[] { spell_like }; c.correct_dc = true; c.value = Helpers.CreateContextValue(AbilityRankType.Default);}));
                spell_feature.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterClassLevel.ToContextRankBaseValueType(), 
                                            classes: getSummonerArray(),
                                            progression: ContextRankProgression.DivStep, stepLevel: 4));
                spell_feature.SetNameDescription("Infernal Arcana: " + spell_feature.Name,
                                                 $"The devil binder’s eidolon gains {spell_feature.Name} spell - like ability.\n"
                                                 + "This ability is usable once per day, and the eidolon’s caster level is equal to the devil binder’s summoner level.\n"
                                                 + spell_feature.Name + ": " + spell_feature.Description);
                spell_feature.AddComponent(resource.CreateAddAbilityResource());
                infernal_arcana[i] = Common.createAddFeatToAnimalCompanion(spell_feature, "");
                resource_bonus.AddComponent(Helpers.Create<IncreaseResourceAmount>(r => { r.Resource = resource; r.Value = 2; }));
            }
            infernal_arcana[spells.Length] = Common.createAddFeatToAnimalCompanion(resource_bonus, "");
        }

        static public BlueprintCharacterClass[] getSummonerArray()
        {
            return new BlueprintCharacterClass[] { summoner_class }; 
        }


        static void createSummonerProgression()
        {
            createSummonerProficiencies();
            createSummonerCantrips();
            createSummonMonster();
            createEidolon();
            createLink();
            createLifeLink();
            createLifeBond();
            createShieldAllyAndGreaterSheildAlly();
            createMakersCallAndTranspostion();
            createAspect();
            createGreaterAspect();            
            createMergeForms();
            createTwinEidolon();

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            summoner_progression = Helpers.CreateProgression("SummonerProgression",
                                                              summoner_class.Name,
                                                              summoner_class.Description,
                                                              "",
                                                              summoner_class.Icon,
                                                              FeatureGroup.None);
            summoner_progression.Classes = getSummonerArray();

            summoner_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, summoner_proficiencies, detect_magic, summoner_cantrips, eidolon_selection, life_link, link,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")), // touch calculate feature                                                                                      
                                                                    Helpers.LevelEntry(2),
                                                                    Helpers.LevelEntry(3),
                                                                    Helpers.LevelEntry(4, shield_ally),
                                                                    Helpers.LevelEntry(5),
                                                                    Helpers.LevelEntry(6, makers_call),
                                                                    Helpers.LevelEntry(7),
                                                                    Helpers.LevelEntry(8, transposition),
                                                                    Helpers.LevelEntry(9),
                                                                    Helpers.LevelEntry(10, aspect_extra[0]),
                                                                    Helpers.LevelEntry(11, aspect_extra[1]),
                                                                    Helpers.LevelEntry(12, greater_shield_ally, aspect_extra[2]),
                                                                    Helpers.LevelEntry(13, aspect_extra[3]),
                                                                    Helpers.LevelEntry(14, life_bond, aspect_extra[4]),
                                                                    Helpers.LevelEntry(15, aspect_extra[5]),
                                                                    Helpers.LevelEntry(16, aspect_extra[6]),
                                                                    Helpers.LevelEntry(17, aspect_extra[7]),
                                                                    Helpers.LevelEntry(18, greater_aspect_extra[0]),
                                                                    Helpers.LevelEntry(19, greater_aspect_extra[1]),
                                                                    Helpers.LevelEntry(20, greater_aspect_extra[2], twin_eidolon)
                                                                    };
            for (int i = 0; i < 20; i++)
            {
                summoner_progression.LevelEntries[i].Features.Add(evolution_distribution[i]);
            }

            for (int i = 0; i < 9; i++)
            {
                summoner_progression.LevelEntries[i*2].Features.Add(summon_monster[i]);
            }

            summoner_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { summoner_proficiencies, summoner_cantrips, eidolon_selection };
            summoner_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(life_link, shield_ally, makers_call, transposition, life_bond, greater_shield_ally, twin_eidolon),
                                                            Helpers.CreateUIGroup(link, aspect_extra[0], greater_aspect_extra[0]),
                                                            Helpers.CreateUIGroup(summon_monster)
                                                           };
        }

        static void createSummonMonster()
        {
            var mt_feats = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("c19f6f34ed0bc364cbdec88b49a54f67"),
                library.Get<BlueprintFeature>("45e466127a8961d40bb3030816ed245b"),
                library.Get<BlueprintFeature>("ea26b3a3acb98074fa34f80fcc4e497d"),
                library.Get<BlueprintFeature>("03168f4f13ff26f429d912085e88baba"),
                library.Get<BlueprintFeature>("00fda605a917fcc4e89612dd31683bdd"),
                library.Get<BlueprintFeature>("9b14b05456142914888a48354a0eec17"),
                library.Get<BlueprintFeature>("667fd017406abd548b89292edd7dbfb7"),
                library.Get<BlueprintFeature>("20d72612311ba914aaba5cc8a4cf312c"),
                library.Get<BlueprintFeature>("f63d23b4e41b3264fa6aa2be8079d28d")
            };

            summon_resource = Helpers.CreateAbilityResource("SummonnerSummonResource", "", "", "", null);
            summon_resource.SetIncreasedByStat(3, StatType.Charisma);
            var description = "At 1st level, a summoner can cast summon monster I as a spell-like ability a number of times per day equal to 3 + his Charisma modifier. Drawing on this ability uses up the same power that the summoner uses to call his eidolon. As a result, he can use this ability only when his eidolon is not summoned. He can cast this spell as a standard action, and the creatures remain for 1 minute per level (instead of 1 round per level). At 3rd level, and every 2 levels thereafter, the power of this ability increases by 1 spell level, allowing him to summon more powerful creatures (to a maximum of summon monster IX at 17th level). A summoner cannot have more than one summon monster or gate spell active in this way at one time. If this ability is used again, any existing summon monster or gate from this spell-like ability immediately ends. These summon spells are considered to be part of the summoner’s spell list for the purposes of spell trigger and spell completion items.";
            summon_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", "SummonerSummonPool", "");

            for (int i = 0; i < mt_feats.Length; i++)
            {
                List<BlueprintAbility> summon_spells = new List<BlueprintAbility>();
                foreach (var f in mt_feats[i].GetComponent<AddFacts>().Facts)
                {
                    var ability = library.CopyAndAdd<BlueprintAbility>(f.AssetGuid, f.name.Replace("MonsterTactician", "Summoner"), "");
                    ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = summon_resource);
                    foreach (var c in ability.GetComponents<ContextRankConfig>())
                    {
                        if (c.IsBasedOnClassLevel)
                        {
                            var new_c = c.CreateCopy(crc => Helpers.SetField(crc, "m_Class", getSummonerArray()));
                            ability.ReplaceComponent(c, new_c);
                        }
                    }
                    var new_actions = Common.changeAction<ContextActionClearSummonPool>(ability.GetComponent<AbilityEffectRunAction>().Actions.Actions, a => a.SummonPool = summon_pool);
                    new_actions = Common.changeAction<ContextActionSpawnMonster>(new_actions, a => a.SummonPool = summon_pool);
                    ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(new_actions));
                    ability.AddComponent(Helpers.Create<NewMechanics.AbilityCasterCompanionDead>());
                    summon_spells.Add(ability);
                }

                BlueprintAbility summon_base = null;
                if (summon_spells.Count == 1)
                {
                    summon_base = summon_spells[0];
                }
                else
                {
                    summon_base = Common.createVariantWrapper($"SummonerSummon{i + 1}Base", "", summon_spells.ToArray());
                    summon_base.SetNameDescription("Summon Monster " + Common.roman_id[i + 1], description);
                }

                summon_monster[i] = Helpers.CreateFeature($"SummonerSummonMonster{i + 1}Feature",
                                                          "Summon Monster " + Common.roman_id[i + 1],
                                                          description,
                                                          "",
                                                          summon_spells[0].Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFact(summon_base)
                                                          );
                if (i == 0)
                {
                    summon_monster[i].AddComponent(summon_resource.CreateAddAbilityResource());
                }
            }
        }


        static void createMergeForms()
        {
            var resource = Helpers.CreateAbilityResource("MergeFormsResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, getSummonerArray());
            var icon = Helpers.GetIcon("4aa7942c3e62a164387a73184bca3fc1");
            var buff = Helpers.CreateBuff("MergeFormsBuff",
                                          "Merge Forms",
                                          "At 16th level, as a full-round action, a summoner can touch his eidolon and the two can merge forms. This transformation includes all of the summoner’s gear. While merged in this way, the summoner is protected from harm and cannot be the target of spells or effects. All effects and spells currently targeting the summoner are suspended until the summoner emerges from the eidolon (although durations continue to expire).\n"
                                          + "The summoner can cast spells while inside the eidolon by taking control of the eidolon for the duration of the casting. Any material components used for these spells are taken from the summoner’s gear, even though they are otherwise inaccessible. The summoner can direct all of the eidolon’s actions while merged, can perceive through its senses, and can speak through its voice.\n"
                                          + "The summoner can use this ability for a number of rounds per day equal to his summoner level. He can end this effect at any time as a swift action. The summoner emerges in a square adjacent to the eidolon if able. If the eidolon is returned to its home plane while the summoner is merged with it, the summoner is immediately ejected, takes 4d6 points of damage, and is stunned for 1 round.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<EvolutionMechanics.ShareSpellbooksWithCompanion>()
                                          );
            var toggle = Helpers.CreateActivatableAbility("MergeFormsActivatableAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.WithUnitCommand,
                                                          CommandType.Standard,
                                                          null,
                                                          resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound)
                                                          );
            merge_forms = Common.ActivatableAbilityToFeature(toggle, false);
            merge_forms.AddComponent(resource.CreateAddAbilityResource());
        }


        static void createTwinEidolon()
        {
            var icon = Helpers.GetIcon("1bc83efec9f8c4b42a46162d72cbf494"); //burst of glory
            var twinned_eidolon_evolution = Helpers.CreateFeature("TwinnedEidolonEvolution",
                                                                    "",
                                                                    "",
                                                                    "",
                                                                    null,
                                                                    FeatureGroup.None,
                                                                    Helpers.Create<CompanionMechanics.SetPhysicalStatsToAnimalCompanionStats>(),
                                                                    Helpers.Create<CompanionMechanics.GrabFeaturesFromCompanion>(g => g.Features = Evolutions.evolutions_list.ToArray().AddToArray(Eidolon.transferable_abilities))
                                                                    );
            twinned_eidolon_evolution.HideInCharacterSheetAndLevelUp = true;

            var buff = Helpers.CreateBuff("TwinnedEidolonBuff",
                                          "Twin Eidolon",
                                          "At 20th level, a summoner and his eidolon share a true connection. As a standard action, the summoner can assume the shape of his eidolon, copying all of its evolutions, form, and abilities. His Strength, Dexterity, and Constitution scores change to match the base scores of his eidolon. He can choose to have any gear that he carries become absorbed by his new form, as with spells from the polymorph subschool. Items with continuous effects continue to function while absorbed in this way. The summoner loses his natural attacks and all racial traits (except bonus feats, skills, and languages) in favor of the abilities granted by his eidolon’s evolutions. The summoner retains all of his class features. The summoner can keep this form for a number of minutes per day equal to his summoner level. This duration does not need to be consecutive, but it must be spent in 1-minute increments.",
                                          "",
                                          icon,
                                          null,
                                          //Helpers.Create<EvolutionMechanics.AddShortDurationSelfEvolution>(a => { a.Feature = twinned_eidolon_evolution; }),
                                          Helpers.Create<CompanionMechanics.SetPhysicalStatsToAnimalCompanionStats>(),
                                          Helpers.Create<CompanionMechanics.GrabFeaturesFromCompanion>(g => g.Features = Evolutions.evolutions_list.ToArray().AddToArray(Eidolon.transferable_abilities)),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Polymorph)
                                          );

            var resource = Helpers.CreateAbilityResource("TwinnedEidolonResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, getSummonerArray());
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
            var ability = Helpers.CreateAbility("TwinnedEidolonAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                resource.CreateResourceLogic(),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Polymorph),
                                                Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Polymorph))),
                                                Common.createAbilitySpawnFx("352469f228a3b1f4cb269c7ab0409b8e", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            twin_eidolon = Common.AbilityToFeature(ability, false);
            twin_eidolon.AddComponent(resource.CreateAddAbilityResource());
            //stats set to eidolons
            //all evolutions are copied
        }


        static void createAspect()
        {            
            BlueprintFeature[] aspect_selection = new BlueprintFeature[3];

            for (int i = 0; i < 3; i++)
            {
                var aspect_i = Helpers.CreateFeature($"SummonerAspect{i}EvolutionFeature",
                                                   $"Aspect ({i} E.P.)",
                                                   "At 10th level, a summoner can divert up to 2 points from his eidolon’s evolution pool to add evolutions to himself. He cannot select any evolution that the eidolon could not possess, and he must be able to meet the requirements as well (except for subtype requirements, so long as his eidolon meets the subtype requirement). He cannot select the ability increase evolution through this ability. Any points spent in this way are taken from the eidolon’s evolution pool (reducing the total number available to the eidolon). The summoner can change the evolutions granted by these points anytime he can change the eidolon’s evolutions.",
                                                   "",
                                                   Helpers.GetIcon("489c8c4a53a111d4094d239054b26e32"), //abyssal strength
                                                   FeatureGroup.None,
                                                   Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(ep => ep.amount = -i)
                                                   );
                var aspect_entry = Helpers.CreateFeature($"SummonerAspect{i}Feature",
                                                         aspect_i.Name,
                                                         aspect_i.Description,
                                                         "",
                                                         aspect_i.Icon,
                                                         FeatureGroup.None,
                                                         Helpers.Create<EvolutionMechanics.AddTemporarySelfEvolution>(a => { a.cost = -i; a.Feature = aspect_i; })
                                                         );
                if (i > 0)
                {
                    aspect_entry.AddComponent(Helpers.Create<EvolutionMechanics.addSelfEvolutionSelection>(a => a.selection = Evolutions.self_evolution_selection));
                    aspect_entry.AddComponent(Helpers.Create<EvolutionMechanics.PrerequisiteEnoughEvolutionPoints>(p => p.amount = i));
                }
                aspect_entry.HideInCharacterSheetAndLevelUp = true;
                aspect_selection[i] = aspect_entry;
            }

            aspect = Helpers.CreateFeatureSelection("SummonerAspectFeatureSelection",
                                                    "Aspect",
                                                    aspect_selection[0].Description,
                                                    "",
                                                    aspect_selection[0].Icon,
                                                    FeatureGroup.None
                                                    );
            aspect.AllFeatures = aspect_selection;


            aspect_extra = new BlueprintFeature[8];
            for (int i = 0; i < aspect_extra.Length; i++)
            {
                aspect_extra[i] = Helpers.CreateFeature($"SummonerAspectExtraFeature{i}",
                                                        aspect.Name,
                                                        aspect.Description,
                                                        "",
                                                        aspect.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.Create<EvolutionMechanics.RefreshSelfEvolutionsOnLevelUp>(),
                                                        Helpers.Create<EvolutionMechanics.addSelection>(a => a.selection = aspect)
                                                        );

                if (i > 0)
                {
                    aspect_extra[i].HideInCharacterSheetAndLevelUp = true;
                    aspect_extra[i].HideInUI = true;
                }
            }
        }


        static void createGreaterAspect()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/GreaterAspect.png");
            BlueprintFeature[] aspect_selection = new BlueprintFeature[4];

            for (int i = 0; i < 4; i++)
            {
                var aspect_i = Helpers.CreateFeature($"SummonerGreaterAspect{i}EvolutionFeature",
                                                   $"Greater Aspect ({i} E.P.)",
                                                   "At 18th level, a summoner can divert more of his eidolon’s evolutions to himself. This ability functions as the aspect ability, but the maximum number of evolution points the summoner can divert increases to 6. In addition, the eidolon loses 1 point from its evolution pool for every 2 points (or fraction thereof ) diverted to the summoner instead of losing 1 point from the evolution pool for each point diverted.",
                                                   "",
                                                   icon,
                                                   FeatureGroup.None,
                                                   Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(ep => ep.amount = -i)
                                                   );
                var aspect_entry = Helpers.CreateFeature($"SummoneGreaterAspect{i}Feature",
                                                         aspect_i.Name,
                                                         aspect_i.Description,
                                                         "",
                                                         aspect_i.Icon,
                                                         FeatureGroup.None,
                                                         Helpers.Create<EvolutionMechanics.AddTemporarySelfEvolution>(a => { a.cost = -2*i; a.Feature = aspect_i; })
                                                         );
                if (i > 0)
                {
                    aspect_entry.AddComponent(Helpers.Create<EvolutionMechanics.addSelfEvolutionSelection>(a => a.selection = Evolutions.self_evolution_selection));
                    aspect_entry.AddComponent(Helpers.Create<EvolutionMechanics.PrerequisiteEnoughEvolutionPoints>(p => p.amount = i));
                }
                aspect_entry.HideInCharacterSheetAndLevelUp = true;
                aspect_selection[i] = aspect_entry;
            }

            greater_aspect = Helpers.CreateFeatureSelection("SummonerGreaterAspectFeatureSelection",
                                                    "Greater Aspect",
                                                    aspect_selection[0].Description,
                                                    "",
                                                    aspect_selection[0].Icon,
                                                    FeatureGroup.None
                                                    );
            greater_aspect.AllFeatures = aspect_selection;
            greater_aspect_extra = new BlueprintFeature[3];
            for (int i = 0; i < greater_aspect_extra.Length; i++)
            {
                greater_aspect_extra[i] = Helpers.CreateFeature($"SummonerGreaterAspectExtraFeature{i}",
                                                        greater_aspect.Name,
                                                        greater_aspect.Description,
                                                        "",
                                                        greater_aspect.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.Create<EvolutionMechanics.RefreshSelfEvolutionsOnLevelUp>(),
                                                        Helpers.Create<EvolutionMechanics.addSelection>(a => a.selection = greater_aspect)
                                                        );

                if (i > 0)
                {
                    greater_aspect_extra[i].HideInCharacterSheetAndLevelUp = true;
                    greater_aspect_extra[i].HideInUI = true;
                }
            }
        }


        static void createMakersCallAndTranspostion()
        {
            makers_call_resource = Helpers.CreateAbilityResource("MakersCallResource", "", "", "", null);
            makers_call_resource.SetIncreasedByLevelStartPlusDivStep(0, 6, 1, 4, 1, 0, 0.0f, getSummonerArray());


            var ability = library.CopyAndAdd<BlueprintAbility>("5bdc37e4acfa209408334326076a43bc", "MakersCallAbility", "");

            ability.Type = AbilityType.Supernatural;
            ability.Parent = null;
            ability.Range = AbilityRange.Personal;
            ability.RemoveComponents<SpellComponent>();
            ability.RemoveComponents<SpellListComponent>();
            ability.RemoveComponents<RecommendationNoFeatFromGroup>();
            ability.SetNameDescriptionIcon("Maker’s Call",
                                           "At 6th level, as a standard action, a summoner can call his eidolon to his side. This functions as dimension door, using the summoner’s caster level. When this ability is used, the eidolon appears adjacent to the summoner (or as close as possible if all adjacent spaces are occupied). If the eidolon is out of range, the ability is wasted. The summoner can use this ability once per day at 6th level, plus one additional time per day for every four levels beyond 6th.",
                                           Helpers.GetIcon("a5ec7892fb1c2f74598b3a82f3fd679f")); //stunning barrier

            var dimension_door_component = ability.GetComponent<AbilityCustomDimensionDoor>();

            var dimension_door_call = Helpers.Create<NewMechanics.CustomAbilities.AbilityCustomMoveCompanionToTarget>(a =>
            {
                a.CasterAppearFx = dimension_door_component.CasterAppearFx;
                a.CasterAppearProjectile = dimension_door_component.CasterAppearProjectile;
                a.CasterDisappearFx = dimension_door_component.CasterDisappearFx;
                a.CasterDisappearProjectile = dimension_door_component.CasterDisappearProjectile;
                a.PortalBone = dimension_door_component.PortalBone;
                a.PortalFromPrefab = dimension_door_component.PortalFromPrefab;
                a.Radius = 10.Feet();
                a.SideAppearFx = dimension_door_component.SideAppearFx;
                a.SideAppearProjectile = dimension_door_component.SideAppearProjectile;
                a.SideDisappearFx = dimension_door_component.SideDisappearFx;
                a.SideDisappearProjectile = dimension_door_component.SideDisappearProjectile;
            }
            );
            ability.ReplaceComponent(dimension_door_component, dimension_door_call);
            ability.AddComponent(makers_call_resource.CreateResourceLogic());
            ability.setMiscAbilityParametersSelfOnly();
            ability.AddComponent(Helpers.Create<NewMechanics.AbilityCasterCompanionDead>(a => a.not = true));

            makers_call = Common.AbilityToFeature(ability, false);
            makers_call.AddComponent(Helpers.CreateAddAbilityResource(makers_call_resource));
            makers_call_ability = ability;

            summon_call_ability = library.CopyAndAdd(ability, "SummonCallAbility", "6a8ce66ed50c431b9825a3554491c904");
            summon_call_ability.SetNameDescriptionIcon("Call Eidolon", "", null);
            summon_call_ability.RemoveComponents<AbilityResourceLogic>();
            summon_call_ability.Parent = null;
            summon_companion_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(Helpers.Create<ContextActionCastSpell>(c => c.Spell = summon_call_ability))));



            var dimension_door = library.Get<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2");
            var ability2 = Helpers.CreateAbility("SummonerTranspositionAbility",
                                                      "Transpostion",
                                                      "At 8th level, a summoner can use his maker’s call ability to swap locations with his eidolon. If the eidolon occupies more squares than the summoner, the summoner can appear in any square occupied by the eidolon. The eidolon must occupy the square that was occupied by the summoner if able, or as close as possible if it is not able.",
                                                      "",
                                                      dimension_door.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      "",
                                                      "",
                                                      Helpers.CreateRunActions(Helpers.Create<ContextActionCastSpell>(c => c.Spell = summon_call_ability),
                                                                               Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList(Helpers.Create<ContextActionCastSpell>(ca => ca.Spell = dimension_door)))
                                                                               ),
                                                      makers_call_resource.CreateResourceLogic()
                                                     );
            ability2.AddComponent(Helpers.Create<NewMechanics.AbilityCasterCompanionDead>(a => a.not = true));
            transposition = Common.AbilityToFeature(ability2, false);
        }


        static void createShieldAllyAndGreaterSheildAlly()
        {
            var buff1 = Helpers.CreateBuff("SummonerShieldAllyBuff",
                                              "",
                                              "",
                                              "",
                                              null,
                                              null,
                                              Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.Shield),
                                              Helpers.CreateAddStatBonus(StatType.SaveFortitude, 2, ModifierDescriptor.Circumstance),
                                              Helpers.CreateAddStatBonus(StatType.SaveReflex, 2, ModifierDescriptor.Circumstance),
                                              Helpers.CreateAddStatBonus(StatType.SaveWill, 2, ModifierDescriptor.Circumstance)
                                              );
            var buff11 = library.CopyAndAdd<BlueprintBuff>(buff1, "SummonerGreaterShieldAllyAllyBuff", "");
            var buff2 = Helpers.CreateBuff("SummonerGreaterShieldAllyBuff",
                                  "",
                                  "",
                                  "",
                                  null,
                                  null, //shield spell
                                  Helpers.CreateAddStatBonus(StatType.AC, 4, ModifierDescriptor.Shield),
                                  Helpers.CreateAddStatBonus(StatType.SaveFortitude, 4, ModifierDescriptor.Circumstance),
                                  Helpers.CreateAddStatBonus(StatType.SaveReflex, 4, ModifierDescriptor.Circumstance),
                                  Helpers.CreateAddStatBonus(StatType.SaveWill, 4, ModifierDescriptor.Circumstance)
                                  );

            var shield_ally_eidolon = Common.createAuraEffectFeature("Shield Ally",
                                                                     "At 4th level, whenever a summoner is within his eidolon’s reach, the summoner gains a +2 shield bonus to his Armor Class and a +2 circumstance bonus on his saving throws. This bonus does not apply if the eidolon is grappled, helpless, paralyzed, stunned, or unconscious.",
                                                                     Helpers.GetIcon("ef768022b0785eb43a18969903c537c4"),
                                                                     buff1,
                                                                     10.Feet(),
                                                                     Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionIsMaster>())
                                                                     );

            var add_shield_ally = Common.createAddFeatToAnimalCompanion(shield_ally_eidolon, "");
            add_shield_ally.HideInCharacterSheetAndLevelUp = true;

            shield_ally = Helpers.CreateFeature("SummonerShieldAllyFeature",
                                                shield_ally_eidolon.Name,
                                                shield_ally_eidolon.Description,
                                                "",
                                                shield_ally_eidolon.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(add_shield_ally, 12, getSummonerArray(), before: true)
                                                );

            var greater_shield_ally_eidolon = Common.createAuraEffectFeature("Greater Shield Ally",
                                                         "At 12th level, whenever an ally is within reach of the summoner’s eidolon, the ally gains a +2 shield bonus to its Armor Class and a +2 circumstance bonus on its saving throws. If this ally is the summoner, these bonuses increase to +4. This bonus does not apply if the eidolon is grappled, helpless, paralyzed, stunned, or unconscious.",
                                                         Helpers.GetIcon("ef768022b0785eb43a18969903c537c4"),
                                                         buff11,
                                                         10.Feet(),
                                                         Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>(), Helpers.Create<NewMechanics.ContextConditionIsMaster>(c => c.Not = true), Helpers.Create<ContextConditionIsCaster>(c => c.Not = true))
                                                         );
            greater_shield_ally_eidolon.AddComponent(Common.createAuraEffectFeatureComponentCustom(buff2, 
                                                                                                   10.Feet(),
                                                                                                   Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionIsMaster>())));

            greater_shield_ally = Helpers.CreateFeature("SummonerGreaterShieldAllyFeature",
                                                        greater_shield_ally_eidolon.Name,
                                                        greater_shield_ally_eidolon.Description,
                                                        "",
                                                        greater_shield_ally_eidolon.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.Create<AddFeatureToCompanion>(a => a.Feature = greater_shield_ally_eidolon)
                                                        );
        }


        static void createLink()
        {
            link = Helpers.CreateFeature("SummonerLink",
                                         "Link",
                                         "A summoner and his eidolon share a mental link that allows for communication across any distance (as long as they are on the same plane). This communication is a free action, allowing the summoner to give orders to his eidolon at any time. In addition, magic items interfere with the summoner’s connection to his eidolon. As a result, the summoner and his eidolon share magic item slots. For example, if the summoner is wearing a ring, his eidolon can wear no more than one ring. In case of a conflict, the items worn by the summoner remain active, and those used by the eidolon become dormant. The eidolon must possess the appropriate appendages to use a magic item.",
                                         "",
                                         null,
                                         FeatureGroup.None,
                                         Helpers.Create<CompanionMechanics.SharedSlots>()
                                         );
        }


        static void createLifeLink()
        {
            var life_link_eidolon_feature = Helpers.CreateFeature("SummonerLifeLinkEidolonFeature",
                                                                  "Life Link",
                                                                  "At 1st level, a summoner forms a close bond with his eidolon. Whenever the eidolon takes enough damage to send it back to its home plane, as a reaction to the damage, the summoner can sacrifice any number of hit points he has without using an action. Each hit point sacrificed in this way prevents 1 point of damage dealt to the eidolon. This can prevent the eidolon from being sent back to its home plane.",
                                                                  "",
                                                                  Helpers.GetIcon("d5847cad0b0e54c4d82d6c59a3cda6b0"), //breath of life
                                                                  FeatureGroup.None,
                                                                  Helpers.Create<CompanionMechanics.TransferDamageToMaster>()
                                                                  );


            var buff = Helpers.CreateBuff("SummonerLifeLinkBuff",
                                          life_link_eidolon_feature.Name,
                                          life_link_eidolon_feature.Description,
                                          "",
                                          life_link_eidolon_feature.Icon,
                                          null,
                                          Helpers.Create<AddFeatureToCompanion>(a => a.Feature = life_link_eidolon_feature)
                                          );

            var toggle = Helpers.CreateActivatableAbility("SummonerLifeLinkToggleAbility",
                                                          life_link_eidolon_feature.Name,
                                                          life_link_eidolon_feature.Description,
                                                          "",
                                                          life_link_eidolon_feature.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          CommandType.Free,
                                                          null);
            toggle.DeactivateImmediately = true;
            toggle.Group = ActivatableAbilityGroupExtension.EidolonLifeLink.ToActivatableAbilityGroup();
            life_link = Common.ActivatableAbilityToFeature(toggle, false);
        }


        static void createLifeBond()
        {
            var buff = Helpers.CreateBuff("SummonerLifeBondBuff",
                                          "Life Bond",
                                          "At 14th level, the summoner’s life becomes linked to his eidolon’s. As long as the eidolon has 1 or more hit points, the summoner is protected from harm. Damage in excess of that which would reduce the summoner to 0 hit points is instead transferred to the eidolon. This damage is transferred 1 point at a time, meaning that as soon as the eidolon is reduced to a number of negative hit points equal to its Constitution score, all excess damage remains with the summoner. Effects that cause death but don’t deal damage are unaffected by this ability. This ability does not affect spells such as baleful polymorph, flesh to stone, imprisonment, or other spells that don’t deal damage.",
                                          "",
                                          Helpers.GetIcon("7792da00c85b9e042a0fdfc2b66ec9a8"), //break enchantment
                                          null,
                                          Helpers.Create<CompanionMechanics.TransferDamageAfterThresholdToPet>(a => a.threshold = 1)
                                          );

            var toggle = Helpers.CreateActivatableAbility("SummonerLifeBondToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          CommandType.Free,
                                                          null);
            toggle.DeactivateImmediately = true;
            toggle.Group = ActivatableAbilityGroupExtension.EidolonLifeLink.ToActivatableAbilityGroup();
            life_bond = Common.ActivatableAbilityToFeature(toggle, false);
        }


        static void createEidolon()
        {
            var summoner_eidolon_rank_progression = library.CopyAndAdd<BlueprintProgression>("125af359f8bc9a145968b5d8fd8159b8", "SummonerEidolonProgression", "");
            summoner_eidolon_rank_progression.Classes = getSummonerArray();
            eidolon_selection = Helpers.CreateFeatureSelection("EidolonFeatureSelection",
                                                                  "Eidolon",
                                                                  "A summoner begins play with the ability to summon to his side a powerful outsider called an eidolon. The eidolon forms a link with the summoner, who forever after summons an aspect of the same creature. Each eidolon has a subtype, chosen when the eidolon is first summoned, that determines its origin and many of its abilities. An eidolon must be within one alignment step of the summoner who calls it (so a neutral good summoner can call a neutral, lawful good, or chaotic good eidolon) and can speak all of his languages. An eidolon is treated as a summoned creature, except it is not sent back to its home plane until reduced to a number of negative hit points equal to or greater than its Constitution score. In addition, due to its tie to its summoner, an eidolon can touch and attack creatures warded by protection from evil and similar effects that prevent contact with summoned creatures.\n"
                                                                  + "The eidolon takes a form shaped by the summoner’s desires. The eidolon’s Hit Dice, saving throws, skills, feats, and abilities are tied to the summoner’s class level and increase as the summoner gains levels. In addition, each eidolon gains a pool of evolution points based on the summoner’s class level that can be used to give the eidolon different abilities and powers. Whenever the summoner gains a level, he must decide how these points are spent, and they are set until he gains another level of summoner.\n"
                                                                  + "The eidolon’s physical appearance is up to the summoner, but it always appears as some sort of fantastical creature appropriate to its subtype. This control is not fine enough to make the eidolon appear like a specific creature.",
                                                                  "",
                                                                  null,
                                                                  FeatureGroup.AnimalCompanion,
                                                                  Helpers.Create<AddFeatureOnApply>(a => a.Feature = summoner_eidolon_rank_progression),
                                                                  Helpers.Create<AddFeatureOnApply>(a => a.Feature = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d"))
                                                                  );
            eidolon_selection.AllFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("472091361cf118049a2b4339c4ea836a") }; //empty companion
            Eidolon.create();
            for (int lvl = 1; lvl <= 20; lvl++)
            {
                var feature = Helpers.CreateFeature($"SummonerEvolutionDistribution{lvl}Feature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None);
                feature.AddComponent(Helpers.Create<EvolutionMechanics.RefreshEvolutionsOnLevelUp>());
                int bonus_ep = Eidolon.EidolonComponent.rank_to_level[lvl] - Eidolon.EidolonComponent.rank_to_level[lvl - 1];
                if (bonus_ep > 0)
                {
                    feature.AddComponent(Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(n => n.amount = bonus_ep));
                }
                feature.AddComponent(Helpers.Create<EvolutionMechanics.addEvolutionSelection>(a => a.selection = Evolutions.evolution_selection));

                feature.HideInCharacterSheetAndLevelUp = true;
                feature.HideInUI = true;

                evolution_distribution.Add(feature);
            }


            var unsummon_buff = Helpers.CreateBuff("EidolonUnsummonedBuff",
                                                   "Eidolon Unsummoned",
                                                   "Your eidolon is unsummoned.",
                                                   "",
                                                   Helpers.GetIcon("4aa7942c3e62a164387a73184bca3fc1"), //disintegrate
                                                   null,
                                                   Helpers.CreateAddFactContextActions(activated: new GameAction[] { Helpers.Create<CompanionMechanics.ContextActionUnsummonCompanion>() },
                                                       deactivated: new GameAction[] { Helpers.Create<CompanionMechanics.ContextActionSummonCompanion>(),
                                                                                       Helpers.Create<ContextActionClearSummonPool>(c => c.SummonPool = summon_pool)
                                                                                     })
                                                  );
           unsummon_buff.SetBuffFlags(BuffFlags.RemoveOnRest);

           var unsummon_companion = Helpers.CreateAbility("EidolonUnsummonAbility",
                                                           "Unsummon Eidolon",
                                                           "Unsummon your eidolon.",
                                                           "",
                                                           unsummon_buff.Icon,
                                                           AbilityType.Supernatural,
                                                           CommandType.Standard,
                                                           AbilityRange.Personal,
                                                           "",
                                                           "",
                                                           Helpers.CreateRunActions(Common.createContextActionApplyBuff(unsummon_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)),
                                                           Helpers.Create<NewMechanics.AbilityCasterCompanionDead>(a => a.not = true),
                                                           Helpers.Create<CompanionMechanics.AbilityCasterCompanionCanBeUnsummoned>());

            unsummon_companion.setMiscAbilityParametersSelfOnly();
            var summon_companion = Helpers.CreateAbility("EidolonSummonAbility",
                                                           "Summon Eidolon",
                                                           "Summons previously unsummoned eidolon.",
                                                           "",
                                                           unsummon_companion.Icon,
                                                           AbilityType.Supernatural,
                                                           CommandType.Standard,
                                                           AbilityRange.Personal,
                                                           "",
                                                           "",
                                                           Helpers.CreateRunActions(Helpers.Create<ContextActionRemoveBuff>(c => c.Buff = unsummon_buff)),
                                                           Helpers.Create<CompanionMechanics.AbilityCasterCompanionUnsummoned>());
            Common.setAsFullRoundAction(summon_companion);
            summon_companion.setMiscAbilityParametersSelfOnly();
            eidolon_selection.AddComponent(Helpers.CreateAddFacts(summon_companion, unsummon_companion));
            summon_companion_ability = summon_companion;
        }


        static void createSummonerProficiencies()
        {
            summoner_proficiencies = library.CopyAndAdd<BlueprintFeature>("25c97697236ccf2479d0c6a4185eae7f", //sorcerer proficiencies
                                                                "SummonerProficiencies",
                                                                "de926c4e29de4550931b959bcd41e280");
            summoner_proficiencies.SetName("Summoner Proficiencies");
            summoner_proficiencies.SetDescription("Summoners are proficient with all simple weapons and light armor. A summoner can cast summoner spells while wearing light armor without incurring the normal arcane spell failure chance. Like any other arcane spellcaster, a summoner wearing medium or heavy armor, or using a shield, incurs a chance of arcane spell failure if the spell in question has a somatic component.");
            summoner_proficiencies.ReplaceComponent<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { a.Facts[0], library.Get<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132") });
            summoner_proficiencies.AddComponent(Common.createArcaneArmorProficiency(ArmorProficiencyGroup.Light));
        }


        static void createSummonerCantrips()
        {
            var daze = library.Get<BlueprintAbility>("55f14bc84d7c85446b07a1b5dd6b2b4c");
            summoner_cantrips = Common.createCantrips("SummonerCantripsFeature",
                                                   "Cantrips",
                                                   "Summoners can cast a number of cantrips, or 0-level spells. These spells are cast like any other spell, but they are not expended when cast and may be used again.",
                                                   daze.Icon,
                                                   "98914f0079234c06b9a3c5064e06665b",
                                                   summoner_class,
                                                   StatType.Charisma,
                                                   summoner_class.Spellbook.SpellList.SpellsByLevel[0].Spells.ToArray());
        }


        static BlueprintSpellbook createSummonerSpellbook()
        {
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var summoner_spellbook = Helpers.Create<BlueprintSpellbook>();
            summoner_spellbook.name = "SummonerSpellbook";
            library.AddAsset(summoner_spellbook, "20f4a4b890204302ae9895fdc45bb20c");
            summoner_spellbook.Name = summoner_class.LocalizedName;
            summoner_spellbook.SpellsPerDay = bard_class.Spellbook.SpellsPerDay;
            summoner_spellbook.SpellsKnown = bard_class.Spellbook.SpellsKnown;
            summoner_spellbook.Spontaneous = true;
            summoner_spellbook.IsArcane = true;
            summoner_spellbook.AllSpellsKnown = false;
            summoner_spellbook.CanCopyScrolls = false;
            summoner_spellbook.CastingAttribute = StatType.Charisma;
            summoner_spellbook.CharacterClass = summoner_class;
            summoner_spellbook.CasterLevelModifier = 0;
            summoner_spellbook.CantripsType = CantripsType.Cantrips;
            summoner_spellbook.SpellsPerLevel = bard_class.Spellbook.SpellsPerLevel;

            summoner_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            summoner_spellbook.SpellList.name = "SummonerSpellList";
            library.AddAsset(summoner_spellbook.SpellList, "972048af37924e59b174653974b255a5");
            summoner_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < summoner_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                summoner_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);

            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "0c852a2405dd9f14a8bbcfaf245ff823", 0), //acid_splash
                new Common.SpellId( "55f14bc84d7c85446b07a1b5dd6b2b4c", 0), //daze
                new Common.SpellId( "c3a8f31778c3980498d8f00c980be5f5", 0), //guidance
                new Common.SpellId( "95f206566c5261c42aa5b3e7e0d1e36c", 0), //mage light
                new Common.SpellId( "7bc8e27cba24f0e43ae64ed201ad5785", 0), //resistance

                new Common.SpellId( "95810d2829895724f950c8c4086056e7", 1), //corrosive touch
                new Common.SpellId( "c60969e7f264e6d4b84a1499fdcf9039", 1), //enlarge person
                new Common.SpellId( "4f8181e7a7f1d904fbaea64220e83379", 1), //expeditious retreat
                new Common.SpellId( "95851f6e85fe87d4190675db0419d112", 1), //grease
                new Common.SpellId( NewSpells.long_arm.AssetGuid, 1),
                //life conduit
                new Common.SpellId( "9e1ad5d6f87d19e4d8883d63a6e35568", 1), //mage armor
                new Common.SpellId( "403cf599412299a4f9d5d925c7b9fb33", 1), //magic fang
                new Common.SpellId( NewSpells.obscuring_mist.AssetGuid, 1),
                new Common.SpellId( "433b1faf4d02cc34abb0ade5ceda47c4", 1), //protection from alignment
                new Common.SpellId( "fa3078b9976a5b24caf92e20ee9c0f54", 1), //ray of sickening
                new Common.SpellId( "4e0e9aba6447d514f88eff1464cc4763", 1), //reduce person
                //rejuvenate eidolon, lesser
                new Common.SpellId( "ef768022b0785eb43a18969903c537c4", 1), //shield
                new Common.SpellId( "9f10909f0be1f5141bf1c102041f93d9", 1), //snowball
                new Common.SpellId( "8fd74eddd9b6c224693d9ab241f25e84", 1), //summon monster 1

                new Common.SpellId( "5b77d7cc65b8ab74688e74a37fc2f553", 2), //barksin
                new Common.SpellId( "a900628aea19aa74aad0ece0e65d091a", 2), //bear's endurance
                new Common.SpellId( NewSpells.blade_tutor.AssetGuid, 2),
                new Common.SpellId( NewSpells.blood_armor.AssetGuid, 2),
                new Common.SpellId( "14ec7a4e52e90fa47a4c8d63c69fd5c1", 2), //blur
                new Common.SpellId( "4c3d08935262b6544ae97599b3a9556d", 2), //bull's strength
                new Common.SpellId( "de7a025d48ad5da4991e7d3c682cf69d", 2), //cat's grace
                new Common.SpellId( "29ccc62632178d344ad0be0865fd3113", 2), //create pit
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagle's splendor
                //evolution surge, lesser
                new Common.SpellId( "ae4d3ad6a8fda1542acf2e9bbc13d113", 2), //fox cunning
                new Common.SpellId( "ce7dad2b25acf85429b6c9550787b2d9", 2), //glitterdust
                new Common.SpellId( "89940cde01689fb46946b2f8cd7b66b7", 2), //invisibility
                new Common.SpellId( "f0455c9295b53904f9e02fc571dd2ce1", 2), //owl's wisdom
                new Common.SpellId( "2cadf6c6350e4684baa109d067277a45", 2), //protection from alignment
                new Common.SpellId( "c28de1f98a3f432448e52e5d47c73208", 2), //protection from arrows
                new Common.SpellId( "21ffef7791ce73f468b6fca4d9371e8b", 2), //resist energy
                //restore eidolon, lesser
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( "970c6db48ff0c6f43afc9dbb48780d03", 2), //summon small elemental
                new Common.SpellId( "1724061e89c667045a6891179ee2e8e7", 2), //summon monster 2
                new Common.SpellId( NewSpells.warding_weapon.AssetGuid, 2),

                new Common.SpellId( NewSpells.allied_cloak.AssetGuid, 3),
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //displacement
                //evoution surge
                new Common.SpellId( NewSpells.fly.AssetGuid, 3),
                new Common.SpellId( "486eaff58293f6441a5c2759c4872f98", 3), //haste
                new Common.SpellId( "5ab0d42fb68c9e34abae4921822b9d63", 3), //heroism
                //life conduit improved
                new Common.SpellId( "f1100650705a69c4384d3edd88ba0f52", 3), //magic fang greater
                 new Common.SpellId( "96c9d98b6a9a7c249b6c4572e4977157", 3), //protection from arrows communal
                new Common.SpellId( "d2f116cfe05fcdd4a94e80143b67046f", 3), //protection from energy
                new Common.SpellId( "97b991256e43bb140b263c326f690ce2", 3), //rage
                //rejuvenate eidolon
                //restore eidolon
                new Common.SpellId( "7bb0c402f7f789d4d9fae8ca87b4c7e2", 3), //resist energy communal

                new Common.SpellId( "f492622e473d34747806bdb39356eb89", 3), //slow
                new Common.SpellId( "46097f610219ac445b4d6403fc596b9f", 3), //spiked pit
                new Common.SpellId( "68a9e6d7256f1354289a39003a46d826", 3), //stinking cloud
                new Common.SpellId( "e42b1dbff4262c6469a9ff0a6ce730e3", 3), //summon medium elemental
                new Common.SpellId( "7ed74a3ec8c458d4fb50b192fd7be6ef", 3), //summon monster 4

                
                new Common.SpellId( "1407fb5054d087d47a4c40134c809f12", 4), //acid pit
                new Common.SpellId( NewSpells.daze_mass.AssetGuid, 4),
                new Common.SpellId( "4a648b57935a59547b7a2ee86fb4f26a", 4), //dimension door
                new Common.SpellId( "66dc49bf154863148bd217287079245e", 4), //enlarge person mass
                //evolution surge greater
                new Common.SpellId( NewSpells.fire_shield.AssetGuid, 4),
                new Common.SpellId( "ecaa0def35b38f949bd1976a6c9539e0", 4), //invisibility greater
                new Common.SpellId( "e48638596c955a74c8a32dbc90b518c1", 4), //obsidian flow          
                new Common.SpellId( "76a629d019275b94184a1a8733cac45e", 4), //protection from energy communal
                new Common.SpellId( "2427f2e3ca22ae54ea7337bbab555b16", 4), //reduce person mass
                new Common.SpellId( NewSpells.solid_fog.AssetGuid, 4),
                new Common.SpellId( "c66e86905f7606c4eaa5c774f0357b2b", 4), //stoneskin
                new Common.SpellId( "89404dd71edc1aa42962824b44156fe5", 4), //summon large elemental
                new Common.SpellId( "630c8b85d9f07a64f917d79cb5905741", 4), //summon monster 5
                new Common.SpellId( NewSpells.wall_of_fire.AssetGuid, 4),

                new Common.SpellId( "3105d6e9febdc3f41a08d2b7dda1fe74", 5), //baleful polymorph
                new Common.SpellId( "548d339ba87ee56459c98e80167bdf10", 5), //cloudkill
                new Common.SpellId( "95f7cdcec94e293489a85afdf5af1fd7", 5), //dismissal
                new Common.SpellId( "f0f761b808dc4b149b08eaf44b99f633", 5), //dispel magic greater
                new Common.SpellId( "41e8a952da7a5c247b3ec1c2dbb73018", 5), //hold monster
                new Common.SpellId( "f63f4d1806b78604a952b3958892ce1c", 5), //hungry pit
                //life conduit, greater
                new Common.SpellId( NewSpells.overland_flight.AssetGuid, 5),
                //rejuvenate eidolon greater
                new Common.SpellId( "7c5d556b9a5883048bf030e20daebe31", 5), //stoneskin communal
                new Common.SpellId( "766ec978fa993034f86a372c8eb1fc10", 5), //summon huge elemental
                new Common.SpellId( "e740afbab0147944dab35d83faa0ae1c", 5), //summon monster 6
              

                new Common.SpellId( "dbf99b00cd35d0a4491c6cc9e771b487", 6), //acid fog
                new Common.SpellId( "f6bcea6db14f0814d99b54856e918b92", 6), //bears endurance mass
                new Common.SpellId( "6a234c6dcde7ae94e94e9c36fd1163a7", 6), //bull strength mass
                new Common.SpellId( "1f6c94d56f178b84ead4c02f1b1e1c48", 6), //cat grace mass
                new Common.SpellId( "b974af13e45639a41a04843ce1c9aa12", 6), //creeping doom
                new Common.SpellId( "2caa607eadda4ab44934c5c9875e01bc", 6), //eagles splendor mass
                new Common.SpellId( "2b24159ad9907a8499c2313ba9c0f615", 6), //fox cunning mass
                new Common.SpellId( "e15e5e7045fda2244b98c8f010adfe31", 6), //heroism greater
                new Common.SpellId( "98310a099009bbd4dbdf66bcef58b4cd", 6), //invisibility mass
                new Common.SpellId( "9f5ada581af3db4419b54db77f44e430", 6), //owls wisdom mass 
                new Common.SpellId( "8eb769e3b583f594faabe1cfdb0bb696", 6), //summon greater elemental
                new Common.SpellId( "ab167fd8203c1314bac6568932f1752f", 6), //summon monster 7
                new Common.SpellId( "7d700cdf260d36e48bb7af3a8ca5031f", 6), //tar pool
                new Common.SpellId( "4cf3d0fae3239ec478f51e86f49161cb", 6), //true seeing
            
                
                
                
                

                new Common.SpellId( "8a7f8c1223bda1541b42fd0320cdbe2b", 8), //summon elder elemental
                new Common.SpellId( "5d61dde0020bbf54ba1521f7ca0229dc", 7), //summon monster 3
                new Common.SpellId( "d3ac756a229830243a72e84f3ab050d0", 7), //summon monster 8
                new Common.SpellId( "52b5df2a97df18242aec67610616ded0", 7), //summon monster 9              
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(summoner_spellbook.SpellList, spell_id.level);
            }

            return summoner_spellbook;
        }


        static void createSummonerSpells()
        {
            createEvolutionSurge();
            createRejuvenateEidolon();
            //createRestoreEidolon();
            createLifeConduit();
        }


        static void createEvolutionSurge()
        {
            var icon = Helpers.GetIcon("93d9d74dac46b9b458d4d2ea7f4b1911"); //polymoph

            var spell_levels = new int[] { 2, 3, 4 };
            var max_evolution_cost = new int[] { 2, 4, 6};
            var names = new string[] { "Evolution Surge, Lesser", "Evolution Surge", "Evolution Surge, Greater" };

            for (int i = 0; i < spell_levels.Length; i++)
            {
                var descripton = "This spell causes your eidolon to take on new characteristics.\n"
                                                                           + $"You can grant the eidolon any evolution whose total cost does not exceed {max_evolution_cost[i]} evolution points. You may only grant one evolution with this spell, even if that evolution can be taken multiple times.\n"
                                                                           + "You can grant an evolution that allows you to spend additional evolution points to upgrade that evolution. This spell cannot be used to grant an upgrade to an evolution that the eidolon already possesses. The eidolon must meet any prerequisites of the selected evolution.";
                if (i + 1 == spell_levels.Length)
                {
                    descripton = "This spell causes your eidolon to take on new characteristics.\n"
                                 + $"You can grant the eidolon up to 2 evolutions whose total cost do not exceed {max_evolution_cost[i]} evolution points.\n"
                                 + "You can grant an evolution that allows you to spend additional evolution points to upgrade that evolution. This spell cannot be used to grant an upgrade to an evolution that the eidolon already possesses. The eidolon must meet any prerequisites of the selected evolution.";
                }
                var ability = Evolutions.getGrantTemporaryEvolutionAbility(max_evolution_cost[i], true,
                                                                           names[i].Replace(" ", "").Replace(",", ""),
                                                                           names[i],
                                                                           "This spell causes your eidolon to take on new characteristics.\n"
                                                                           + $"You can grant the eidolon any evolution whose total cost does not exceed {max_evolution_cost[i]} evolution points. You may only grant one evolution with this spell, even if that evolution can be taken multiple times.\n"
                                                                           + "You can grant an evolution that allows you to spend additional evolution points to upgrade that evolution. This spell cannot be used to grant an upgrade to an evolution that the eidolon already possesses. The eidolon must meet any prerequisites of the selected evolution.",
                                                                           icon,
                                                                           AbilityType.Spell,
                                                                           CommandType.Standard,
                                                                           Helpers.minutesPerLevelDuration,
                                                                           i + 1 == spell_levels.Length,
                                                                           Helpers.CreateContextRankConfig(),
                                                                           Helpers.CreateSpellComponent(SpellSchool.Transmutation));
                ability.AddToSpellList(summoner_class.Spellbook.SpellList, spell_levels[i]);
                ability.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Transmutation));


                Helpers.AddSpell(ability);
            }
        }


        static void createRejuvenateEidolon()
        {
            var icons = new UnityEngine.Sprite[]{Helpers.GetIcon("47808d23c67033d4bbab86a1070fd62f"), //cure light wounds
                                                 Helpers.GetIcon("6e81a6679a0889a429dec9cedcf3729c"), //cure serious wounds
                                                 Helpers.GetIcon("0d657aa811b310e4bbd8586e60156a2d") //cure critical wounds
                                                };
            var prefabs = new string[] { "61602c5b0ac793d489c008e9cb58f631", "d95f8959c2e167d4b899de4eea00b60c", "474cb5ac1a1048d4a9ee6ae62474d196" };
            var max_hp = new int[] { 5, 10, 20 };
            var names = new string[] { "Rejuvenate Eidolon, Lesser", "Rejuvenate Eidolon", "Rejuvenate Eidolon, Greater" };
            for (int i = 0; i < 3; i++)
            {
                var ability = Helpers.CreateAbility(names[i].Replace(" ", "").Replace(",", "") + "Ability",
                                                    names[i],
                                                    $"By laying your hand upon an eidolon, you cause its wounds to close and its form to solidify. This spell cures {1 + 2 * i}d10 points of damage +1 point per caster level (maximum +{max_hp[i]}).",
                                                    "",
                                                    icons[i],
                                                    AbilityType.Spell,
                                                    CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(DiceType.D10, 2 * i + 1, Helpers.CreateContextValue(AbilityRankType.Default)))),
                                                    Common.createAbilitySpawnFxDestroyOnCast(prefabs[i], anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                    Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.RestoreHP | SpellDescriptor.Cure),
                                                    Helpers.Create<NewMechanics.AbilityTargetIsPet>(),
                                                    Helpers.CreateDeliverTouch(),
                                                    Helpers.CreateContextRankConfig(max: max_hp[i])
                                                    );
                ability.setMiscAbilityParametersTouchFriendly();
                ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Heighten | Metamagic.Quicken | Metamagic.Maximize | Metamagic.Reach;
                var spell = ability.CreateTouchSpellCast();
                spell.AddComponent(Helpers.Create<NewMechanics.AbilityTargetIsPet>());
                spell.AddToSpellList(summoner_class.Spellbook.SpellList, 2 * i + 1);
                Helpers.AddSpell(spell);
            }
        }


        static void createLifeConduit()
        {
            var icon = Helpers.GetIcon("017afe6934e10c3489176e759a5f01b0"); //touch of good
            var prefabs = new string[] { "61602c5b0ac793d489c008e9cb58f631", "d95f8959c2e167d4b899de4eea00b60c", "474cb5ac1a1048d4a9ee6ae62474d196" };
            var names = new string[] { "Life Conduit", "Life Conduit, Improved", "Life Conduit, Greater" };

            for (int i = 0; i <3; i++)
            {
                var heal = Common.createContextActionHealTargetNoBonus(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilitySharedValue.Heal)));
                var damage = Helpers.CreateActionDealDamage(DamageEnergyType.Holy, Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilitySharedValue.Heal)));
                damage.DamageType.Type = Kingmaker.RuleSystem.Rules.Damage.DamageType.Direct;
                var config = Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.D6, i + 1, 0), AbilitySharedValue.Heal);
                var heal_master = Helpers.CreateAbility(names[i].Replace(" ", "").Replace(",", "") + "MasterAbility",
                                                        names[i],
                                                        $"You utilize life conduit to share hit points with your eidolon. While this spell is active, you can spend a swift action to transfer {i + 1}d6 hit points between you and your eidolon, either taking damage yourself and healing your eidolon or healing yourself and damaging your eidolon.",
                                                        "",
                                                        icon,
                                                        AbilityType.SpellLike,
                                                        CommandType.Swift,
                                                        AbilityRange.Personal,
                                                        "",
                                                        "",
                                                        Helpers.Create<NewMechanics.AbilityCasterPetIsAlive>(),
                                                        Helpers.CreateRunActions(heal, Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList(damage))),
                                                        Common.createAbilitySpawnFxDestroyOnCast(prefabs[i], anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                        Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.RestoreHP | SpellDescriptor.Cure),
                                                        config
                                                        );
                heal_master.setMiscAbilityParametersSelfOnly();
                heal_master.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Empower;


                var damage_master = Helpers.CreateAbility(names[i].Replace(" ", "").Replace(",", "") + "DamageMasterAbility",
                                        names[i] + "( Transfer HP to Eidolon)",
                                        $"You utilize life conduit to share hit points with your eidolon. While this spell is active, you can spend a swift action to transfer {i + 1}d6 hit points between you and your eidolon, either taking damage yourself and healing your eidolon or healing yourself and damaging your eidolon.",
                                        "",
                                        icon,
                                        AbilityType.SpellLike,
                                        CommandType.Swift,
                                        AbilityRange.Personal,
                                        "",
                                        "",
                                        Helpers.Create<NewMechanics.AbilityCasterPetIsAlive>(),
                                        Helpers.CreateRunActions(damage, Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList(heal))),
                                        Common.createAbilitySpawnFxDestroyOnCast(prefabs[i], anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                        Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                        Helpers.CreateSpellDescriptor(SpellDescriptor.RestoreHP | SpellDescriptor.Cure),
                                        config
                                        );
                heal_master.setMiscAbilityParametersSelfOnly();
                heal_master.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Empower;

                var heal_pet = Helpers.CreateAbility(names[i].Replace(" ", "").Replace(",", "") + "PetAbility",
                                        names[i],
                                        $"You utilize life conduit to share hit points with your eidolon. While this spell is active, you can spend a swift action to transfer {i + 1}d6 hit points between you and your eidolon, either taking damage yourself and healing your eidolon or healing yourself and damaging your eidolon.",
                                        "",
                                        icon,
                                        AbilityType.SpellLike,
                                        CommandType.Swift,
                                        AbilityRange.Personal,
                                        "",
                                        "",
                                        Helpers.Create<NewMechanics.AbilityCasterMasterIsAlive>(),
                                        Helpers.CreateRunActions(heal, Helpers.Create<NewMechanics.ContextActionsOnMaster>(c => c.Actions = Helpers.CreateActionList(damage))),
                                        Common.createAbilitySpawnFxDestroyOnCast(prefabs[i], anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                        Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                        Helpers.CreateSpellDescriptor(SpellDescriptor.RestoreHP | SpellDescriptor.Cure),
                                        config
                                        );
                heal_pet.setMiscAbilityParametersSelfOnly();
                heal_pet.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Empower;

                var master_buff = Helpers.CreateBuff(names[i].Replace(" ", "").Replace(",", "") + "MasterBuff",
                                                     heal_master.Name,
                                                     heal_master.Description,
                                                     "",
                                                     heal_master.Icon,
                                                     null,
                                                     Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = heal_master),
                                                     Helpers.CreateAddFact(heal_master),
                                                     Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = damage_master),
                                                     Helpers.CreateAddFact(damage_master)
                                                     );

                var pet_buff = Helpers.CreateBuff(names[i].Replace(" ", "").Replace(",", "") + "PetBuff",
                                     heal_pet.Name,
                                     heal_pet.Description,
                                     "",
                                     heal_pet.Icon,
                                     null,
                                     Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = heal_pet),
                                     Helpers.CreateAddFact(heal_pet)
                                     );

                var apply_master_buff = Common.createContextActionApplyBuff(master_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_from_spell: true);
                var apply_pet_buff = Common.createContextActionApplyBuff(pet_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));

                var ability = Helpers.CreateAbility(names[i].Replace(" ", "").Replace(",", "") + "Ability",
                                                    heal_master.Name,
                                                    heal_master.Description,
                                                    "",
                                                    heal_master.Icon,
                                                    AbilityType.Spell,
                                                    CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    Helpers.roundsPerLevelDuration,
                                                    "",
                                                    Helpers.CreateRunActions(apply_master_buff),
                                                    //Helpers.CreateRunActions(apply_master_buff, Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList(apply_pet_buff))),
                                                    Common.createAbilitySpawnFx("930c1a4aa129b8344a40c8c401d99a04", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                    Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                    Helpers.CreateSpellDescriptor(SpellDescriptor.RestoreHP | SpellDescriptor.Cure),
                                                    Helpers.CreateContextRankConfig(),
                                                    Helpers.Create<SharedSpells.CannotBeShared>()
                                                    );
                ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Maximize | Metamagic.Empower | Metamagic.Extend;
                ability.setMiscAbilityParametersSelfOnly();
                ability.AddToSpellList(summoner_class.Spellbook.SpellList, 2 * i + 1);
                Helpers.AddSpell(ability);
            }
        }
    }


}
