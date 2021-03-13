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
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
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
    public class Oracle
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass oracle_class;
        static public BlueprintProgression oracle_progression;
        static public BlueprintFeature oracle_proficiencies;
        static public BlueprintFeature oracle_orisons;
        static public BlueprintFeatureSelection oracle_curses;
        static public BlueprintFeatureSelection oracle_mysteries;
        static public BlueprintFeatureSelection revelation_selection;
        static public BlueprintFeatureSelection cure_inflict_spells_selection;
        static public BlueprintFeature mystery_skills;

        static public BlueprintProgression clouded_vision;
        static public BlueprintFeature clouded_vision_minor;
        static public BlueprintProgression blackened;
        static public BlueprintFeature blackened_minor;
        static public BlueprintProgression deaf;
        static public BlueprintFeature deaf_minor;
        static public BlueprintProgression lame;
        static public BlueprintFeature lame_minor;
        static public BlueprintProgression wasting;
        static public BlueprintFeature wasting_minor;
        static public BlueprintProgression pranked;
        static public BlueprintFeature pranked_minor;
        static public BlueprintProgression plagued;
        static public BlueprintFeature plagued_minor;
        static public BlueprintProgression vampirism;
        static public BlueprintFeature vampirism_minor;
        static public BlueprintProgression lich;
        static public BlueprintFeature lich_minor;
        static public BlueprintProgression wolf_scarred_face;
        static public BlueprintFeature wolf_scarred_face_minor;
        static public BlueprintProgression powerless;
        static public BlueprintFeature powerless_minor;
        static public BlueprintProgression reclusive;
        static public BlueprintFeature reclusive_minor;

        static MysteryEngine mystery_engine;

        static public BlueprintProgression time_mystery;
        static public BlueprintProgression ancestor_mystery;
        static public BlueprintProgression flame_mystery;
        static public BlueprintProgression battle_mystery;
        static public BlueprintProgression life_mystery;
        static public BlueprintProgression wind_mystery;
        static public List<BlueprintProgression> dragon_mysteries = new List<BlueprintProgression>();
        static public BlueprintProgression waves_mystery;
        static public BlueprintProgression nature_mystery;
        static public BlueprintFeatureSelection animal_companion;
        static public BlueprintProgression bones_mystery;

        static Dictionary<BlueprintProgression, BlueprintAbility[]> mystery_spells_map = new Dictionary<BlueprintProgression, BlueprintAbility[]>();
        static public BlueprintArchetype seeker_archetype;
        static public BlueprintFeature tinkering, seeker_lore, seeker_magic;
        static public BlueprintArchetype warsighted_archetype;
        static public BlueprintFeatureSelection fighter_feat;
        static public BlueprintArchetype spirit_guide_archetype;
        static public BlueprintFeatureSelection spirit_guide_spirit_selection;

        static public BlueprintArchetype divine_herbalist_archetype;
        static public BlueprintFeature master_herbalist;
        static public BlueprintFeature healers_way;
        static public BlueprintFeature master_healing_technique;
        static public BlueprintAbilityResource healers_way_resource;

        static public BlueprintArchetype dual_cursed_archetype;
        static public BlueprintFeatureSelection dual_cursed_oracle_mysteries;
        static public BlueprintFeatureSelection minor_curse_selection;
        static public Dictionary<int, BlueprintFeature> dual_cursed_bonus_spell_features = new Dictionary<int, BlueprintFeature>();
        static public Dictionary<int, BlueprintAbility> dual_cursed_bonus_spells = new Dictionary<int, BlueprintAbility>();
        static public BlueprintAbility oracles_burden;
        static public BlueprintFeature fortune_revelation;
        static public BlueprintFeature misfortune_revelation;
        static public BlueprintFeature extra_healers_way;

        static public BlueprintArchetype hermit;
        static public BlueprintFeature recluses_stride;
        static public BlueprintFeature fade_from_memory;
        static public BlueprintFeature reclusive_curse;
        static public Dictionary<int, BlueprintFeature> hermit_bonus_spell_features = new Dictionary<int, BlueprintFeature>();
        static public Dictionary<int, BlueprintAbility> hermit_bonus_spells = new Dictionary<int, BlueprintAbility>();
        static public BlueprintFeatureSelection hermit_mysteries;

        static public Dictionary<BlueprintFeature, BlueprintFeature> curse_to_minor_map = new Dictionary<BlueprintFeature, BlueprintFeature>();
        static public Dictionary<BlueprintFeature, BlueprintFeature> curse_to_hindrance_map = new Dictionary<BlueprintFeature, BlueprintFeature>();

        static public Dictionary<BlueprintFeature, BlueprintFeature> oracle_ravener_hunter_mysteries_map = new Dictionary<BlueprintFeature, BlueprintFeature>();

        public class Spirit
        {
            public BlueprintProgression progression;
            public BlueprintAbility[] spells;
            public BlueprintFeatureSelection hex_selection;

            public Spirit(string name, string display_name, string description, UnityEngine.Sprite icon, string guid, 
                           BlueprintFeature spirit_ability, BlueprintFeature greater_spirit_ability, BlueprintAbility[] spells, BlueprintFeature[] hexes)
            {
                hex_selection = Helpers.CreateFeatureSelection(name + "SpiritGuideHexFeatureSelection",
                                                               "Hex",
                                                               "At 3rd level, spirit guide gains one hex of her choice from the list of hexes available from that spirit. She uses her oracle level as her shaman level, and she switches Wisdom for Charisma and vice versa for the purpose of determining the hex’s effects.",
                                                               "",
                                                               null,
                                                               FeatureGroup.None);
                hex_selection.AllFeatures = hexes;

                string spells_description = display_name + "bonded spirit grants spirit guide the following spells: ";
             
                for (int i = 0; i < spells.Length; i++)
                {
                    spells_description += spells[i].Name + ((i == (spells.Length - 1)) ? "" : ", ");
                }
                spells_description += ".";

                var learn_spells = Helpers.CreateFeature(name + "SpiritGuideSpellsFeature",
                                                         "Bonded Spirit Spells",
                                                         spells_description,
                                                         "",
                                                         Helpers.GetIcon("a8e7e315b5a241b47ad526771eee19b7"), //destruction judgement
                                                         FeatureGroup.None,
                                                         (new Common.ExtraSpellList(spells)).createLearnSpellList(name + "SpirirtGuideSpellList", "", oracle_class)
                                                         );



                var entries = new LevelEntry[] { Helpers.LevelEntry(3, hex_selection),
                                                 Helpers.LevelEntry(4, learn_spells),
                                                 Helpers.LevelEntry(7, spirit_ability),
                                                 Helpers.LevelEntry(15, greater_spirit_ability)
                                               };


                progression = Helpers.CreateProgression(name + "SpiritGuideProgression",
                                                        display_name + " Spirit",
                                                        description,
                                                        "",
                                                        icon,
                                                        FeatureGroup.None
                                                        );
                progression.LevelEntries = entries.ToArray();
                progression.UIGroups = Helpers.CreateUIGroups(hex_selection, learn_spells, spirit_ability, greater_spirit_ability);
                progression.Classes = getOracleArray(); 
            }
        }

        public class DragonInfo
        {
            public string name;
            public BlueprintAbility breath_weapon_prototype;
            public UnityEngine.Sprite resist_icon;
            public string[] dragon_form_id;
            public string breath_weapon_area;
            public DamageEnergyType energy;
            public BlueprintActivatableAbility wings;

            public DragonInfo(string dragon_name, string breath_area, DamageEnergyType damage_energy)
            {
                name = dragon_name;
                breath_weapon_prototype = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(a => a.name == ("BloodlineDraconic" + name + "BreathWeaponAbility")).FirstOrDefault();
                wings = library.GetAllBlueprints().OfType<BlueprintActivatableAbility>().Where(a => a.name == ("AbilityWingsDraconic" + name)).FirstOrDefault();
                resist_icon = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(a => a.name == ("Resist" + damage_energy.ToString())).FirstOrDefault().Icon;
                dragon_form_id = new string[3];
                dragon_form_id[0] = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(a => a.name == ("FormOfTheDragonI" + name)).FirstOrDefault().AssetGuid;
                dragon_form_id[1] = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(a => a.name == ("FormOfTheDragonII" + name)).FirstOrDefault().AssetGuid;
                dragon_form_id[2] = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(a => a.name == ("FormOfTheDragonIII" + name)).FirstOrDefault().AssetGuid;
                energy = damage_energy;
                breath_weapon_area = breath_area;
            }
        }

        static BlueprintCharacterClass[] getOracleArray()
        {
            return new BlueprintCharacterClass[] { oracle_class };
        }



        internal static void createOracleClass()
        {
            Main.logger.Log("Oracle class test mode: " + test_mode.ToString());
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");

            oracle_class = Helpers.Create<BlueprintCharacterClass>();
            oracle_class.name = "OracleClass";
            library.AddAsset(oracle_class, "32c02466b2364c8a906e6e4761175099");


            oracle_class.LocalizedName = Helpers.CreateString("Oracle.Name", "Oracle");
            oracle_class.LocalizedDescription = Helpers.CreateString("Oracle.Description",
                "Although the gods work through many agents, perhaps none is more mysterious than the oracle. These divine vessels are granted power without their choice, selected by providence to wield powers that even they do not fully understand. Unlike a cleric, who draws her magic through devotion to a deity, oracles garner strength and power from many sources, namely those patron deities who support their ideals. Instead of worshiping a single source, oracles tend to venerate all of the gods that share their beliefs. While some see the powers of the oracle as a gift, others view them as a curse, changing the life of the chosen in unforeseen ways.\n"
                + "Role: Oracles do not usually associate with any one church or temple, instead preferring to strike out on their own, or with a small group of like-minded individuals. Oracles typically use their spells and revelations to further their understanding of their mystery, be it through fighting mighty battles or tending to the poor and sick."
                );
            oracle_class.m_Icon = oracle_class.Icon;
            oracle_class.SkillPoints = druid_class.SkillPoints;
            oracle_class.HitDie = DiceType.D8;
            oracle_class.BaseAttackBonus = cleric_class.BaseAttackBonus;
            oracle_class.FortitudeSave = cleric_class.ReflexSave;
            oracle_class.ReflexSave = cleric_class.ReflexSave;
            oracle_class.WillSave = cleric_class.WillSave;
            oracle_class.Spellbook = createOracleSpellbook();
            oracle_class.ClassSkills = new StatType[] { StatType.SkillKnowledgeArcana, StatType.SkillLoreReligion, StatType.SkillPersuasion, StatType.SkillKnowledgeWorld };
            oracle_class.IsDivineCaster = true;
            oracle_class.IsArcaneCaster = false;
            oracle_class.StartingGold = cleric_class.StartingGold;
            oracle_class.PrimaryColor = cleric_class.PrimaryColor;
            oracle_class.SecondaryColor = cleric_class.SecondaryColor;
            oracle_class.RecommendedAttributes = new StatType[] { StatType.Charisma};
            oracle_class.NotRecommendedAttributes = new StatType[0];
            oracle_class.EquipmentEntities = cleric_class.EquipmentEntities;
            oracle_class.MaleEquipmentEntities = cleric_class.MaleEquipmentEntities;
            oracle_class.FemaleEquipmentEntities = cleric_class.FemaleEquipmentEntities;
            oracle_class.ComponentsArray = new BlueprintComponent[] { cleric_class.ComponentsArray[0] };
            oracle_class.StartingItems = cleric_class.StartingItems;

            createOracleProgression();
            oracle_class.Progression = oracle_progression;
            createSeeker();
            createWarsighted();
            createSpiritGuide();
            createDivineHerbalist();
            createDualCursed();
            createHermit();

            oracle_class.Archetypes = new BlueprintArchetype[] {seeker_archetype, warsighted_archetype, spirit_guide_archetype, divine_herbalist_archetype, dual_cursed_archetype, hermit};
            Helpers.RegisterClass(oracle_class);
            createExtraRevelationFeat();

            Common.addMTDivineSpellbookProgression(oracle_class, oracle_class.Spellbook, "MysticTheurgeOracle",
                                                     Common.createPrerequisiteClassSpellLevel(oracle_class, 2));
        }


        static void createHermit()
        {
            hermit = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "HermitOracleArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Hermit");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A hermit is a recluse who gained her oracular powers from isolation in a deep desert, on a mountain peak, or in another secluded location. A connection to untraveled places gives the hermit powers to evade his enemies.");
            });
            Helpers.SetField(hermit, "m_ParentClass", oracle_class);
            library.AddAsset(hermit, "");

            createReclusesStride();
            createFadeFromMemory();

            hermit.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, oracle_curses, oracle_mysteries, revelation_selection),
                                                       Helpers.LevelEntry(7, revelation_selection) };

            hermit.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, reclusive, hermit_mysteries, recluses_stride),
                                                    Helpers.LevelEntry(7, fade_from_memory)
                                                  };
            oracle_class.Progression.UIDeterminatorsGroup = oracle_class.Progression.UIDeterminatorsGroup.AddToArray(hermit_mysteries);
            oracle_class.Progression.UIGroups = oracle_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(recluses_stride, fade_from_memory));
        }


        static void createFadeFromMemory()
        {
            var buff20 = library.CopyAndAdd<BlueprintBuff>("49786ccc94a5ee848a5637b4145b2092", "FadeFromMemoryPartialBuff", "");
            buff20.RemoveComponents<AddStatBonus>();
            buff20.ReplaceComponent<AddConcealment>(a => { a.OnlyForAttacks = true; a.DistanceGreater = 10.Feet(); });
            buff20.SetNameDescription("Fade from Memory",
                                      "At 7th level, you can gain 20% concealment from creatures more than 10 feet away until the beginning of your next turn as a free action. At 14th level, you instead gain 50% concealment until the beginning of your next turn. You can use this ability a number of times per day equal to your oracle level."
                                      );

            var buff50 = library.CopyAndAdd(buff20, "FadeFromMemoryTotalBuff", "");
            buff50.ReplaceComponent<AddConcealment>(a => a.Concealment = Concealment.Total);

            var resource = Helpers.CreateAbilityResource("FadeFromMemoryResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, getOracleArray());

            var toggle20 = Common.buffToToggle(buff20, CommandType.Free, false, resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound));
            var toggle50 = Common.buffToToggle(buff50, CommandType.Free, false, resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound));
            toggle20.DeactivateIfCombatEnded = true;
            toggle50.DeactivateIfCombatEnded = true;

            fade_from_memory = Helpers.CreateFeature("FadeFromMeoryFeature",
                                                     toggle20.Name,
                                                     toggle20.Description,
                                                     "",
                                                     toggle20.Icon,
                                                     FeatureGroup.None,
                                                     resource.CreateAddAbilityResource(),
                                                     Helpers.CreateAddFeatureOnClassLevel(Common.ActivatableAbilityToFeature(toggle20), 14, getOracleArray(), before: true),
                                                     Helpers.CreateAddFeatureOnClassLevel(Common.ActivatableAbilityToFeature(toggle50), 14, getOracleArray())
                                                     );
        }


        static void createReclusesStride()
        {
            recluses_stride = Helpers.CreateFeature("ReclusesStrideFeature",
                                                    "Recluse’s Stride",
                                                    "Your base speed increases by 10 feet. At 5th level, once per round when leaving a square, you can treat the square as though it isn’t threatened by any opponents that you can see. At 10th level, you can teleport to a point within medium distance (as per dimension door) as a move action, provided that there are no other creatures within 10 feet of you when you use this ability and no other creatures within 10 feet of your destination. You can teleport a number of times per day equal to 3 + your Charisma modifier.",
                                                    "",
                                                    Helpers.GetIcon("4f8181e7a7f1d904fbaea64220e83379"),  //expeditious retreat
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddStatBonus(StatType.Speed, 10, ModifierDescriptor.UntypedStackable)
                                                    );

            var buff5 = Helpers.CreateBuff("ReclusesStrideBuff5",
                                           recluses_stride.Name,
                                           recluses_stride.Description,
                                           "",
                                           recluses_stride.Icon,
                                           Common.createPrefabLink("534da216ed0a6ff4781aa4628ec2513e"),
                                           Helpers.Create<AooMechanics.NoAooOnDisengage>()
                                           );
            buff5.AddComponent(Helpers.Create<UnitMoveMechanics.ActionOnUnitMoved>(a =>
            {
                a.min_distance_moved = 10.Feet().Meters;
                a.actions = Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff5));
            }));

            var cooldown = Helpers.CreateBuff("ReclusesStrideBuff5cooldown",
                                           recluses_stride.Name +": Cooldown",
                                           recluses_stride.Description,
                                           "",
                                           recluses_stride.Icon,
                                           null
                                           );

            var ability = Helpers.CreateAbility("ReclusesStrideAbility5",
                                                buff5.Name,
                                                buff5.Description,
                                                "",
                                                buff5.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Free,
                                                AbilityRange.Personal,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff5, Helpers.CreateContextDuration(1), dispellable: false),
                                                                         Common.createContextActionApplyBuff(cooldown, Helpers.CreateContextDuration(1), dispellable: false)
                                                                         ),
                                                library.Get<BlueprintAbility>("4f8181e7a7f1d904fbaea64220e83379").GetComponent<AbilitySpawnFx>(),
                                                Common.createAbilityCasterHasNoFacts(cooldown)
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            var teleport = library.CopyAndAdd<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2", "ReclusesStrideDimensionDoorAbility", "");
            teleport.Type = AbilityType.Supernatural;
            teleport.Range = AbilityRange.Medium;

            teleport.ActionType = CommandType.Move;
            teleport.Parent = null;
            teleport.SetNameDescription(recluses_stride.Name + ": Dimension Door", recluses_stride.Description);

            teleport.AddComponents(Helpers.Create<NewMechanics.AbilityCasterNoUnitsAround>(a => a.distance = 10.Feet().Meters),
                                   Helpers.Create<NewMechanics.AbilityTargetPointHasNoUnitsAround>(a => a.distance = 10.Feet().Meters)
                                   );
            var resource = Helpers.CreateAbilityResource("ReclusesStrideResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Charisma);
            teleport.AddComponent(resource.CreateResourceLogic());

            var feature5 = Common.AbilityToFeature(ability);
            var feature10 = Common.AbilityToFeature(teleport);
            feature10.AddComponent(resource.CreateAddAbilityResource());
            recluses_stride.AddComponent(Helpers.CreateAddFeatureOnClassLevel(feature5, 5, getOracleArray()));
            recluses_stride.AddComponent(Helpers.CreateAddFeatureOnClassLevel(feature10, 10, getOracleArray()));
        }


        static void createDualCursed()
        {
            dual_cursed_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DualCursedOracleArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Dual-Cursed Oracle");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "All oracles are cursed to some degree, but some oracles bear an even heavier burden. Though doubly afflicted with supernatural or physical hindrances, a dual-cursed oracle can manipulate fortune and gains greater insights into her mystery.");
            });
            Helpers.SetField(dual_cursed_archetype, "m_ParentClass", oracle_class);
            library.AddAsset(dual_cursed_archetype, "");

            createFortuneRevelation();
            createMisfortuneRevelation();

            dual_cursed_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, mystery_skills, oracle_mysteries) };
            dual_cursed_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, minor_curse_selection, dual_cursed_oracle_mysteries),
                                                                        Helpers.LevelEntry(5, fortune_revelation),
                                                                        Helpers.LevelEntry(13, misfortune_revelation),
                                                                      };

            oracle_class.Progression.UIDeterminatorsGroup = oracle_class.Progression.UIDeterminatorsGroup.AddToArray(minor_curse_selection, dual_cursed_oracle_mysteries);
            oracle_class.Progression.UIGroups = oracle_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(fortune_revelation, misfortune_revelation));
        }


        static void createFortuneRevelation()
        {
            var resource = Helpers.CreateAbilityResource("FortuneRevelationResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 10, 1, 5, 1, 0, 0.0f, getOracleArray());

            var ability = library.CopyAndAdd<BlueprintAbility>("9af0b584f6f754045a0a79293d100ab3", "FortuneRevelationAbility", "");
            ability.RemoveComponents<ReplaceAbilitiesStat>();
            ability.RemoveComponents<SpellComponent>();
            ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            ability.ActionType = CommandType.Swift;
            ability.Range = AbilityRange.Personal;
            ability.Type = AbilityType.Extraordinary;
            ability.setMiscAbilityParametersSelfOnly();
            ability.SetNameDescription("Fortune", 
                                        "At 5th level, as swift action, you can grant yourself an ability to make a second roll and take the best value every time you roll d20 until the end of the round. You can use this ability once per day at 5th level, and one additional time per day for every six oracle levels beyond 5th.");

            fortune_revelation = Common.AbilityToFeature(ability, false);
            fortune_revelation.AddComponent(resource.CreateAddAbilityResource());
        }


        static void createMisfortuneRevelation()
        {
            var ability = library.CopyAndAdd<BlueprintAbility>("ca1a4cd28737ae544a0a7e5415c79d9b", "MisfortuneRevelationAbility", "");
            ability.RemoveComponents<AbilityResourceLogic>();
            ability.RemoveComponents<SpellComponent>();
            ability.RemoveComponents<AbilityDeliverTouch>();
            ability.ActionType = CommandType.Swift;
            ability.Range = AbilityRange.Close;
            ability.Type = AbilityType.Extraordinary;
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            ability.SetNameDescription("Misfortune", 
                                       "At 13th level as a swift action, you can curse a creature within close range with misfortune. For the next round, anytime the target rolls a d20, they must roll twice and take the less favorable result.  Once a creature has suffered from your misfortune, it cannot be the target of this revelation again for 1 day.");

            var buff = Helpers.CreateBuff("MisfortuneRevelationCooldownBuff",
                                          ability.Name + " Cooldown",
                                          ability.Description,
                                          "",
                                          ability.Icon,
                                          null);
            var apply_cooldown = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);
            buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            buff.Stacking = StackingType.Stack;
            ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(apply_cooldown)));
            ability.AddComponent(Common.createAbilityTargetHasNoFactUnlessBuffsFromCaster(new BlueprintBuff[] { buff }));

            misfortune_revelation = Common.AbilityToFeature(ability, false);
        }

        static void createDivineHerbalist()
        {
            divine_herbalist_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DivineHerbalistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Divine Herbalist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Divine herbalists practice an obscure alchemical art which involves medicinal and restorative techniques that transcend ordinary alchemy, allowing them to blur the line between alchemical medicine and divine miracles.");
            });
            Helpers.SetField(divine_herbalist_archetype, "m_ParentClass", oracle_class);
            library.AddAsset(divine_herbalist_archetype, "");


            master_herbalist = Helpers.CreateFeature("MasterHerbalistFeature",
                                                     "Master Herbalist",
                                                     "A divine herbalist gains competence bonus on Lore (Nature) checks equal to 1/2 her oracle level (minimum 1), and can use her Charisma modifier in place of her Wisdom modifier when attempting Lore (Nature) checks. Lore (Nature) is a class skill for divine herbalist.",
                                                     "",
                                                     Helpers.GetIcon("d797007a142a6c0409a74b064065a15e"),
                                                     FeatureGroup.Domain,
                                                     Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillLoreNature),
                                                     Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s =>
                                                                                                                             {
                                                                                                                                s.StatTypeToReplaceBastStatFor = StatType.SkillLoreNature;
                                                                                                                                s.NewBaseStatType = StatType.Charisma;
                                                                                                                             }),
                                                     Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Charisma),
                                                     Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom),
                                                     Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.Competence),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getOracleArray(), progression: ContextRankProgression.Div2, min: 1)
                                                     );
            createHealersWayAndMasterHealingTechnique();

            divine_herbalist_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, mystery_skills, revelation_selection), Helpers.LevelEntry(7, revelation_selection) };
            divine_herbalist_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, master_herbalist, healers_way), Helpers.LevelEntry(7, master_healing_technique) };

            oracle_class.Progression.UIDeterminatorsGroup = oracle_class.Progression.UIDeterminatorsGroup.AddToArray(master_herbalist);
            oracle_class.Progression.UIGroups = oracle_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(healers_way, master_healing_technique));
        }


        static void createHealersWayAndMasterHealingTechnique()
        {
            List<BlueprintBuff> buffs = new List<BlueprintBuff>();

            healers_way_resource = Helpers.CreateAbilityResource("HealersWayResource", "", "", "", null);
            healers_way_resource.SetIncreasedByStat(1, StatType.Charisma);

            var ability = library.CopyAndAdd<BlueprintAbility>("caae1dc6fcf7b37408686971ee27db13", "HealesWayOthersAbility", "");

            ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = healers_way_resource);
            ability.RemoveComponents<AbilityCasterAlignment>();
            ability.ReplaceComponent<ContextRankConfig>(c => { Helpers.SetField(c, "m_Class", getOracleArray()); Helpers.SetField(c, "m_Min", 1); Helpers.SetField(c, "m_UseMin", true); });
            ability.AddComponent(Helpers.Create<UndeadMechanics.AbilityTargetHasNegativeEnegyAffinity>(a => a.Inverted = true));

            ability.SetNameDescription("Healer's Way — Others",
                                       $"A divine herbalist combines alchemy, acupuncture, and divine magic to heal wounds by touch. She can use this ability a number of times per day equal to 1 + her Charisma modifier. With one use of this ability, she uses positive energy to heal the target of 1d{BalanceFixes.getDamageDieString(DiceType.D6)} hit points for every 2 oracle levels she has. Using this ability is a standard action unless the oracle targets herself, in which case it is a swift action. Using this ability requires only one free hand. This ability counts as a paladin’s lay on hands ability for the purposes of feats, spells, and effects that work with that class feature when it is used for healing purposes. Unlike lay on hands, this ability cannot be used to harm undead.");


            var actions = ability.GetComponent<AbilityEffectRunAction>().Actions.Actions;

            var descriptor_dc_map = new Dictionary<SpellDescriptor, int>();
            descriptor_dc_map.Add(SpellDescriptor.Fatigue, 25);
            descriptor_dc_map.Add(SpellDescriptor.Shaken, 25);
            descriptor_dc_map.Add(SpellDescriptor.Sickened, 25);
            descriptor_dc_map.Add(SpellDescriptor.Daze, 30);
            descriptor_dc_map.Add(SpellDescriptor.Staggered, 30);
            descriptor_dc_map.Add(SpellDescriptor.Exhausted, 35);
            descriptor_dc_map.Add(SpellDescriptor.Confusion, 35);
            descriptor_dc_map.Add(SpellDescriptor.Frightened, 35);
            descriptor_dc_map.Add(SpellDescriptor.Nauseated, 35);
            descriptor_dc_map.Add(SpellDescriptor.Blindness, 40);
            descriptor_dc_map.Add(SpellDescriptor.Paralysis, 40);
            descriptor_dc_map.Add(SpellDescriptor.Stun, 40);


            var nauseted = library.Get<BlueprintBuff>("956331dba5125ef48afe41875a00ca0e");
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");

            var apply_nauseted = Helpers.CreateActionList(Common.createContextActionApplyBuff(nauseted, Helpers.CreateContextDuration(1), dispellable: false));
            var apply_sickened = Helpers.CreateActionList(Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(1), dispellable: false));
            
            actions = Common.changeAction<Conditional>(actions, c =>
                                                                {
                                                                    var condition = c.ConditionsChecker.Conditions[0] as ContextConditionCasterHasFact;
                                                                    if (condition == null || condition.Not)
                                                                    {
                                                                        return;
                                                                    }
                                                                    var fact = condition.Fact;
                                                                    var action = c.IfTrue.Actions.Length == 0 ? null : c.IfTrue.Actions[0] as ContextActionDispelMagic;
                                                                    if (action == null)
                                                                    {
                                                                        return;
                                                                    }
                                                                    var descriptor = action.Descriptor.Value;
                                                                    if (!descriptor_dc_map.ContainsKey(descriptor))
                                                                    {
                                                                        c.IfTrue = Helpers.CreateActionList();
                                                                        return;
                                                                    }
                                                                    var buff = Helpers.CreateBuff(descriptor.ToString() + "MasterHealingTechniqueBuff",
                                                                                                  "Master Healing Technique: " + descriptor.ToString(),
                                                                                                  "",
                                                                                                  "",
                                                                                                  fact.Icon,
                                                                                                  null);
                                                                    buffs.Add(buff);
                                                                    c.ConditionsChecker = Helpers.CreateConditionsCheckerAnd(Common.createContextConditionCasterHasFact(buff),
                                                                                                                             Helpers.Create<NewMechanics.ContextConditionHasCondtionImmunity>(cc => { cc.Not = true; cc.condition = UnitCondition.Nauseated; }),
                                                                                                                             Helpers.Create<NewMechanics.ContextConditionHasCondtionImmunity>(cc => { cc.Not = true; cc.condition = UnitCondition.Sickened; })
                                                                                                                             );
                                                                    var if_true = Helpers.Create<SkillMechanics.ContextActionSkillCheckWithFailures>();
                                                                    if_true.custom_dc = descriptor_dc_map[descriptor];
                                                                    if_true.Failure10 = apply_nauseted;
                                                                    if_true.Failure5 = apply_sickened;
                                                                    if_true.Stat = StatType.SkillLoreNature;
                                                                    if_true.use_custom_dc = true;
                                                                    if_true.on_caster = true;
                                                                    if_true.Success = Helpers.CreateActionList(Common.createContextActionRemoveBuffsByDescriptor(descriptor));
                                                                    c.IfTrue = Helpers.CreateActionList(if_true);
                                                                }
                                                                );
            
            ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(actions));
            var self_ability = library.CopyAndAdd(ability, "HealesWaySelfAbility", "");
            self_ability.Range = AbilityRange.Personal;
            self_ability.setMiscAbilityParametersSelfOnly();
            self_ability.SetNameDescriptionIcon("Healer's Way — Self", ability.Description, Helpers.GetIcon("8d6073201e5395d458b8251386d72df1"));
            self_ability.ActionType = CommandType.Swift;

            healers_way = Helpers.CreateFeature("HealersWayFeature",
                                                "Healer's way",
                                                ability.Description,
                                                "",
                                                ability.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFacts(ability, self_ability),
                                                healers_way_resource.CreateAddAbilityResource()
                                                );

            var toggles = new List<BlueprintActivatableAbility>();

            foreach (var b in buffs)
            {
                var toggle = Helpers.CreateActivatableAbility(b.name + "ToggleAbility",
                                                              b.Name,
                                                              "At 7th level, whenever a divine herbalist heals a living creature with her healer’s way ability, as a free action she can attempt a Lore (Nature) check to remove a condition from the target, with each condition having an accompanying Lore (Nature) DC (see the list below). Failure by 5 or more causes the target to become sickened for 1 round (if the divine herbalist is attempting to remove the sickened condition, this extends it by 1 round). Similarly, failure by 10 or more instead causes the target to become nauseated for 1 round or to have its existing nauseated condition extended by 1 round. A creature that cannot be sickened or nauseated cannot have conditions removed by this ability.\n"
                                                              + "Minor Conditions (DC 25): Fatigued, shaken, and sickened.\n"
                                                              + "Major Conditions (DC 30): Dazed and staggered.\n"
                                                              + "Severe Conditions (DC 35): Confused, exhausted, frightened, and nauseated.\n"
                                                              + "Dire Conditions (DC 40): Blinded, deafened, paralyzed, and stunned.\n",
                                                              "",
                                                              b.Icon,
                                                              b,
                                                              AbilityActivationType.Immediately,
                                                              CommandType.Free,
                                                              null);
                toggle.DeactivateImmediately = true;
                toggle.Group = ActivatableAbilityGroupExtension.HelaersWay.ToActivatableAbilityGroup();

                toggles.Add(toggle);
            }


            master_healing_technique = Helpers.CreateFeature("MasterHEalingTechniqueFeature",
                                                            "Master Healing Technique",
                                                            toggles[0].Description,
                                                            "",
                                                            Helpers.GetIcon("ff8f1534f66559c478448723e16b6624"), //heal
                                                            FeatureGroup.None,
                                                            Helpers.CreateAddFacts(toggles.ToArray())
                                                            );

            extra_healers_way = library.CopyAndAdd<BlueprintFeature>("a2b2f20dfb4d3ed40b9198e22be82030", "ExtraHealersWay", "");
            extra_healers_way.SetNameDescription("Extra Healer's Way",
                                                 "You can use your healer's way ability two additional times per day.\nSpecial: You can gain Extra Healer's Way multiple times. Its effects stack.");
            extra_healers_way.ReplaceComponent<IncreaseResourceAmount>(i => i.Resource = healers_way_resource);
            extra_healers_way.ReplaceComponent<PrerequisiteFeature>(p => p.Feature = healers_way);
            library.AddFeats(extra_healers_way);
        }


        static void createSpiritGuide()
        {
            spirit_guide_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SpirirtGuideArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Spirit Guide");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Through her exploration of the universe’s mysteries, a spirit guide opens connections to the spirit world and forms bonds with the entities that inhabit it.");
            });
            Helpers.SetField(spirit_guide_archetype, "m_ParentClass", oracle_class);
            library.AddAsset(spirit_guide_archetype, "");


            var class_skills = Helpers.CreateFeature("ClassSkillsSpiritGuideFeature",
                                                     "Class Skills",
                                                     "A spirit guide gains all Knowledge skills as class skills. This replaces the bonus class skills gained from the oracle’s mystery.",
                                                     "",
                                                     null,
                                                     FeatureGroup.Domain,
                                                     Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillLoreNature)
                                                     );
            createSpiritGuideSpiritSelection();

            spirit_guide_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, mystery_skills), Helpers.LevelEntry(3, revelation_selection), Helpers.LevelEntry(7, revelation_selection), Helpers.LevelEntry(15, revelation_selection) };
            spirit_guide_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, class_skills), Helpers.LevelEntry(3, spirit_guide_spirit_selection)};
            oracle_class.Progression.UIGroups = oracle_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(class_skills, spirit_guide_spirit_selection));
        }


        static void createSpiritGuideSpiritSelection()
        {
            var hex_engine = new HexEngine(new BlueprintCharacterClass[] { oracle_class }, StatType.Charisma, StatType.Wisdom, spirit_guide_archetype);

            bool test_mode = false;
            var spirits_engine = new SpiritsEngine(hex_engine);

            var battle_spirit = new SpiritsEngine.BattleSpirit();
            var bones_spirit = new SpiritsEngine.BonesSpirit();
            var flame_spirit = new SpiritsEngine.FlameSpirit();
            var heavens_spirit = new SpiritsEngine.HeavensSpirit();
            var life_spirit = new SpiritsEngine.LifeSpirit();
            var lore_spirit = new SpiritsEngine.LoreSpirit();
            var nature_spirit = new SpiritsEngine.NatureSpirit();
            var stone_spirit = new SpiritsEngine.StoneSpirit();
            var waves_spirit = new SpiritsEngine.WavesSpirit();
            var wind_spirit = new SpiritsEngine.WindSpirit();

            List<Spirit> spirits = new List<Spirit>();


            spirits.Add(battle_spirit.createOracleSpirit(hex_engine, "OracleSpiritGuide", test_mode));
            spirits.Add(bones_spirit.createOracleSpirit(hex_engine, "OracleSpiritGuide", "OracleSpiritGuide", test_mode));
            spirits.Add(flame_spirit.createOracleSpirit(hex_engine, "OracleSpiritGuide", test_mode));
            spirits.Add(stone_spirit.createOracleSpirit(hex_engine, "OracleSpiritGuide", test_mode));
            spirits.Add(waves_spirit.createOracleSpirit(hex_engine, "OracleSpiritGuide", test_mode));
            spirits.Add(wind_spirit.createOracleSpirit(hex_engine, "OracleSpiritGuide", test_mode));
            spirits.Add(nature_spirit.createOracleSpirit(hex_engine, "OracleSpiritGuide", test_mode));
            spirits.Add(lore_spirit.createOracleSpirit(hex_engine, "OracleSpiritGuide", "OracleSpiritGuide", test_mode));
            spirits.Add(heavens_spirit.createOracleSpirit(hex_engine, "OracleSpiritGuide", test_mode));
            spirits.Add(life_spirit.createOracleSpirit(hex_engine, "OracleSpiritGuide", "OracleSpiritGuide", "Extra Channel (Spirit Guide Life Spirit)", test_mode));

            spirit_guide_spirit_selection = Helpers.CreateFeatureSelection("SpiritGuideBondedSpirit",
                                               "Bonded Spirit",
                                               "At 3rd level, a spirit guide can form a bond with a spirit, as the shaman’s wandering spirit class feature.\n"
                                               + "A spirit guide gains one hex of her choice from the list of hexes available from that spirit. She uses her oracle level as her shaman level, and she switches Wisdom for Charisma and vice versa for the purpose of determining the hex’s effects.\n"
                                               + "At 4th level, she adds the bonded spirit’s spirit magic spells to her oracle spells known. At 7th level, she gains the spirit ability of her bonded spirit. At 15th level, she gains the greater spirit ability of her bonded spirit.",
                                               "",
                                               LoadIcons.Image2Sprite.Create(@"AbilityIcons/SpiritCall.png"),
                                               FeatureGroup.None);

            foreach (var s in spirits)
            {
                spirit_guide_spirit_selection.AllFeatures = spirit_guide_spirit_selection.AllFeatures.AddToArray(s.progression);
            }
        }


        static void createWarsighted()
        {
            warsighted_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "WarsightedArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Warsighted");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A warsighted’s unique gifts are not in strange magical revelations, but in her ability to adapt in the midst of a battle with new fighting techniques. The warsighted is a master of combat, as dedicated as a fighter and as flexible as a brawler.");
            });
            Helpers.SetField(warsighted_archetype, "m_ParentClass", oracle_class);
            library.AddAsset(warsighted_archetype, "");

            fighter_feat = library.CopyAndAdd<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f", "WarsightedBonusFeat", "");
            fighter_feat.SetDescription("At 1st, 7th, 11th and 15th a warsighted gains a bonus feat in addition to those gained from normal advancement. These bonus feats must be selected from those listed as combat feats. ");

            warsighted_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, revelation_selection), Helpers.LevelEntry(7, revelation_selection), Helpers.LevelEntry(11, revelation_selection), Helpers.LevelEntry(15, revelation_selection) };
            warsighted_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, fighter_feat), Helpers.LevelEntry(7, fighter_feat), Helpers.LevelEntry(11, fighter_feat), Helpers.LevelEntry(15, fighter_feat)};
        }


        static void createSeeker()
        {
            seeker_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SeekerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Seeker");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Oracles gain their magical powers through strange and mysterious ways, be they chosen by fate or blood. While most might be content with their strange powers, some oracles join the Pathfinders specifically to find out more about their mysteries and determine the genesis and history of their eldritch talents. These spellcasters are known among the Spells as seekers, after their obsession with researching ancient texts and obscure ruins for any clues they can find about their heritage and histories.");
            });
            Helpers.SetField(seeker_archetype, "m_ParentClass", oracle_class);
            library.AddAsset(seeker_archetype, "");

            createTinkering();
            createSeekerLore();
            createSeekerMagic();

            seeker_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, mystery_skills), Helpers.LevelEntry(3, revelation_selection), Helpers.LevelEntry(15, revelation_selection) };
            seeker_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, tinkering), Helpers.LevelEntry(3, seeker_lore), Helpers.LevelEntry(15, seeker_magic) };

            oracle_class.Progression.UIGroups = oracle_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(tinkering, seeker_lore, seeker_magic));
        }


        static void createSeekerMagic()
        {
            seeker_magic = Helpers.CreateFeature("SeekerMagicFeature",
                                                 "Seeker Magic",
                                                 "At 15th level, a seeker becomes skilled at modifying his mystery spells with metamagic. When a seeker applies a metamagic feat to any bonus spells granted by his mystery, he reduces the metamagic feat’s spell level adjustment by 1. Thus, applying a Metamagic feat like Still Spell to a spell does not change its effective spell level at all, while applying Quicken Spell only increases the spell’s effective spell level by 3 instead of by 4. This reduction to the spell level adjustment for Metamagic feats does not stack with similar reductions from other abilities.",
                                                 "",
                                                 Helpers.GetIcon("3524a71d57d99bb4b835ad20582cf613"),
                                                 FeatureGroup.None);

            foreach (var kv in mystery_spells_map)
            {
                var feature = Helpers.CreateFeature(kv.Key.name + "SeekerMagicFeature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.Create<NewMechanics.MetamagicMechanics.ReduceMetamagicCostForSpecifiedSpells>(r => { r.reduction = 1; r.spells = kv.Value; })
                                                    );
                feature.HideInCharacterSheetAndLevelUp = true;
                seeker_magic.AddComponent(Common.createAddFeatureIfHasFact(kv.Key, feature));
            }
        }


        static void createTinkering()
        {
            tinkering = library.CopyAndAdd<BlueprintFeature>("dbb6b3bffe6db3547b31c3711653838e", "SeekerTinkeringFeature", "");
            tinkering.SetNameDescription("Tinkering",
                                         "Seekers often look to ancient devices, old tomes, and strange magical items in order to learn more about their oracle mysteries. As a result of this curiosity and thanks to an innate knack at deciphering the strange and weird, a seeker gains Trickery as a class skill. In addition, at 1st level, a seeker adds half his oracle level on Perception checks made to locate traps and on all Trickery skill checks (minimum +1). A seeker can use Disable Device to disarm magical traps. If the seeker also possesses levels in rogue or another class that provides the trapfinding ability, those levels stack with his oracle levels for determining his overall bonus on these skill checks.\n"
                                         + "This ability replaces all of the bonus class skills he would otherwise normally gain from his mystery."
                                         );
            tinkering.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getOracleArray()));
            tinkering.AddComponent(Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillThievery));
        }


        static void createSeekerLore()
        {
            seeker_lore = Helpers.CreateFeature("SeekerLoreFeature",
                                                "Seeker Lore",
                                                "By 3rd level, a seeker has already learned much about his mystery, and is more comfortable using the bonus spells gained by that mystery. He gains a +4 bonus on all concentration checks and on caster level checks made to overcome spell resistance.",
                                                "",
                                                null,
                                                FeatureGroup.None);
            foreach (var kv in mystery_spells_map)
            {
                var feature = Helpers.CreateFeature(kv.Key.name + "SeekerLoreFeature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.Create<NewMechanics.CasterLevelChecksBonusForSpecifiedSpells>(c => { c.value = 4; c.spells = kv.Value; })
                                                    );
                feature.HideInCharacterSheetAndLevelUp = true;
                seeker_lore.AddComponent(Common.createAddFeatureIfHasFact(kv.Key, feature));
            }
        }


        static void createExtraRevelationFeat()
        {
            var extra_revelation = Helpers.CreateFeatureSelection("ExtraRevelationOracleFeat",
                                                                  "Extra Revelation",
                                                                  "You gain one additional revelation. You must meet all of the prerequisites for this revelation.\n"
                                                                  + "Special: You can gain Extra Revelation up to 2 times.",
                                                                  "",
                                                                  null,
                                                                  FeatureGroup.Feat,
                                                                  Helpers.PrerequisiteFeature(revelation_selection)
                                                                  );
            extra_revelation.AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.PrerequisiteFeatureFullRank>(p =>
                                                                                                                {
                                                                                                                    p.divisor = 3;
                                                                                                                    p.Feature = extra_revelation;
                                                                                                                    p.checked_feature = extra_revelation;
                                                                                                                    p.not = true;
                                                                                                                }
                                                                                                                )
                                         );
            extra_revelation.AllFeatures = revelation_selection.AllFeatures;
            extra_revelation.Ranks = 2;
            library.AddFeats(extra_revelation);
        }


        static BlueprintSpellbook createOracleSpellbook()
        {
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var sorcerer_class = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
            var oracle_spellbook = Helpers.Create<BlueprintSpellbook>();
            oracle_spellbook.Name = oracle_class.LocalizedName;
            oracle_spellbook.name = "OracleSpellbook";
            library.AddAsset(oracle_spellbook, "");
            oracle_spellbook.Name = oracle_class.LocalizedName;
            oracle_spellbook.SpellsPerDay = sorcerer_class.Spellbook.SpellsPerDay;
            oracle_spellbook.SpellsKnown = sorcerer_class.Spellbook.SpellsKnown;
            oracle_spellbook.Spontaneous = true;
            oracle_spellbook.IsArcane = false;
            oracle_spellbook.AllSpellsKnown = false;
            oracle_spellbook.CanCopyScrolls = false;
            oracle_spellbook.CastingAttribute = StatType.Charisma;
            oracle_spellbook.CharacterClass = oracle_class;
            oracle_spellbook.CasterLevelModifier = 0;
            oracle_spellbook.CantripsType = CantripsType.Orisions;
            oracle_spellbook.SpellsPerLevel = cleric_class.Spellbook.SpellsPerLevel;
            oracle_spellbook.SpellList = Common.createSpellList("OracleSpellList", "", cleric_class.Spellbook.SpellList, 9);
            return oracle_spellbook;
        }


        static void createOracleProgression()
        {
            createOracleOrisons();
            createOracleProficiencies();
            createCureInflictSpells();
            createCurses();
            createOracleBurdenSpell();
            dual_cursed_bonus_spells = new Dictionary<int, BlueprintAbility>
            {
                {1, Witch.ill_omen },
                {2, oracles_burden },
                {3, library.Get<BlueprintAbility>("989ab5c44240907489aba0a8568d0603") }
            };

            hermit_bonus_spells = new Dictionary<int, BlueprintAbility>
            {
                {2, SpellDuplicates.addDuplicateSpell("46fd02ad56c35224c9c91c88cd457791", "HermitBlindessSpellAbility", "") },
                {4, library.Get<BlueprintAbility>("4baf4109145de4345861fe0f2209d903") },
                {6, library.Get<BlueprintAbility>("36c8971e91f1745418cc3ffdfac17b74") }
            };

            createMysteries();
            
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            oracle_progression = Helpers.CreateProgression("OracleProgression",
                                                               oracle_class.Name,
                                                               oracle_class.Description,
                                                               "",
                                                               oracle_class.Icon,
                                                               FeatureGroup.None);
            oracle_progression.Classes = getOracleArray();

            oracle_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, oracle_proficiencies, detect_magic, oracle_orisons,
                                                                                        mystery_skills,
                                                                                        oracle_curses,
                                                                                        oracle_mysteries,
                                                                                        cure_inflict_spells_selection,
                                                                                        revelation_selection,
                                                                                        library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee"), //inside the storm
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), //ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")), //touch calculate feature};
                                                                    Helpers.LevelEntry(2),
                                                                    Helpers.LevelEntry(3, revelation_selection),
                                                                    Helpers.LevelEntry(4),
                                                                    Helpers.LevelEntry(5),
                                                                    Helpers.LevelEntry(6),
                                                                    Helpers.LevelEntry(7, revelation_selection),
                                                                    Helpers.LevelEntry(8),
                                                                    Helpers.LevelEntry(9),
                                                                    Helpers.LevelEntry(10),
                                                                    Helpers.LevelEntry(11, revelation_selection),
                                                                    Helpers.LevelEntry(12),
                                                                    Helpers.LevelEntry(13),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, revelation_selection),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18),
                                                                    Helpers.LevelEntry(19, revelation_selection),
                                                                    Helpers.LevelEntry(20)
                                                                    };

            oracle_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { oracle_proficiencies, detect_magic, oracle_orisons, oracle_curses, cure_inflict_spells_selection, oracle_mysteries, mystery_skills };

            oracle_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(revelation_selection)};
        }


        static void createOracleBurdenSpell()
        {
            var actions = new List<GameAction>();

            foreach(var kv in curse_to_minor_map)
            {
                var buff = Helpers.CreateBuff(kv.Key.name + "OraclesBurdenBuff",
                                              "Oracle's Burden: " + kv.Key.Name,
                                              "",
                                              "",
                                              null,
                                              null,
                                              Helpers.CreateAddFact(curse_to_hindrance_map[kv.Key])
                                              );
                var action = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionCasterHasFacts>(c => c.Facts = new BlueprintUnitFact[] { kv.Key, kv.Value }),
                                                      Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes))
                                                      );
                actions.Add(action);
            }


            var ability = Helpers.CreateAbility("OraclesBurdenAbility",
                                                   "Oracle's Burden",
                                                   "You entreat the forces of fate to bestow your oracle’s curse upon another creature. The target creature suffers all the hindrances and none of the benefits of your oracle’s curse class feature. You still suffer all effects of your oracle’s curse.",
                                                   "",
                                                   Helpers.GetIcon("4baf4109145de4345861fe0f2209d903"), //crushing despair
                                                   AbilityType.Spell,
                                                   CommandType.Standard,
                                                   AbilityRange.Touch,
                                                   Helpers.minutesPerLevelDuration,
                                                   Helpers.willNegates,
                                                   Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(new GameAction[0], actions.ToArray())),
                                                   Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                                   Helpers.CreateSpellDescriptor(SpellDescriptor.Curse),
                                                   Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                   Helpers.CreateDeliverTouch(),
                                                   Helpers.CreateContextRankConfig()
                                                   );
            ability.setMiscAbilityParametersTouchHarmful();
            ability.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend | Metamagic.Reach | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)MetamagicFeats.MetamagicExtender.Piercing;
            oracles_burden = Helpers.CreateTouchSpellCast(ability);

            oracles_burden.AddToSpellList(oracle_class.Spellbook.SpellList, 2);
        }

        static void createMysteries()
        {

            mystery_skills = Helpers.CreateFeature("OracleMysterySkills",
                                                   "Additional Class Skilss",
                                                   "Oracle gains additional class skills based on his mystery.",
                                                   "",
                                                   null,
                                                   FeatureGroup.None);
            //mystery_skills.HideInCharacterSheetAndLevelUp = true;
            
            mystery_engine = new MysteryEngine(new BlueprintCharacterClass[] { oracle_class, Archetypes.RavenerHunter.archetype.GetParentClass() }, StatType.Charisma, Archetypes.RavenerHunter.archetype);

            oracle_mysteries = Helpers.CreateFeatureSelection("OracleMysteriesSelection",
                                                              "Mystery",
                                                              "Each oracle draws upon a divine mystery to grant her spells and powers. This mystery also grants additional class skills and other special abilities. This mystery can represent a devotion to one ideal, prayers to deities that support the concept, or a natural calling to champion a cause. For example, an oracle with the waves mystery might have been born at sea and found a natural calling to worship the gods of the oceans, rivers, and lakes, be they benign or malevolent. Regardless of its source, the mystery manifests in a number of ways as the oracle gains levels. An oracle must pick one mystery upon taking her first level of oracle. Once made, this choice cannot be changed.\n"
                                                              + "At 2nd level, and every two levels thereafter, an oracle learns an additional spell derived from her mystery.",
                                                              "",
                                                              null,
                                                              FeatureGroup.Domain);

            dual_cursed_oracle_mysteries = library.CopyAndAdd(oracle_mysteries, "DualCursedOracleMysteries", "");
            hermit_mysteries = library.CopyAndAdd(oracle_mysteries, "HermitOracleMysteries", "");
            hermit_mysteries.SetDescription("Hermit must choose either Ancestors, Life, Nature, Waves or Wind mystery.");

            revelation_selection = Helpers.CreateFeatureSelection("OracleRevelationSelection",
                                                                  "Revelation",
                                                                  "At 1st level, 3rd level, and every four levels thereafter (7th, 11th, and so on), an oracle uncovers a new secret about her mystery that grants her powers and abilities. The oracle must select a revelation from the list of revelations available to her mystery (see FAQ at right). If a revelation is chosen at a later level, the oracle gains all of the abilities and bonuses granted by that revelation based on her current level. Unless otherwise noted, activating the power of a revelation is a standard action.\n"
                                                                  + "Unless otherwise noted, the DC to save against these revelations is equal to 10 + 1/2 the oracle’s level + the oracle’s Charisma modifier.",
                                                                  "",
                                                                  null,
                                                                  FeatureGroup.None);


            createTimeMystery();
            createAncestorsMystery();
            createFlameMystery();
            createBattleMystery();
            createLifeMystery();
            createWindMystery();
            createDragonMysteries();
            createWavesMystery();
            createNatureMystery();
            createBonesMystery();

            oracle_mysteries.AllFeatures = new BlueprintFeature[] { time_mystery, ancestor_mystery, flame_mystery, battle_mystery, life_mystery, wind_mystery, waves_mystery, nature_mystery, bones_mystery};
            oracle_mysteries.AllFeatures = oracle_mysteries.AllFeatures.AddToArray(dragon_mysteries);


            foreach (var m in oracle_mysteries.AllFeatures)
            {
                foreach (var kv in oracle_ravener_hunter_mysteries_map)
                {
                    if (kv.Key != m)
                    {
                        m.AddComponent(Helpers.PrerequisiteNoFeature(kv.Value));
                        kv.Value.AddComponent(Helpers.PrerequisiteNoFeature(m));
                    }
                }
            }
        }


        static void createBonesMystery()
        {
            var spells = new BlueprintAbility[9]
            {
                library.Get<BlueprintAbility>("bd81a3931aa285a4f9844585b5d97e51"), //cause fear
                library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762"), //false life
                library.Get<BlueprintAbility>("4b76d32feb089ad4499c3a1ce8e1ac27"), //animate dead
                library.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0"), //fear
                library.Get<BlueprintAbility>("4fbd47525382517419c66fb548fe9a67"), //slay living
                library.Get<BlueprintAbility>("a89dcbbab8f40e44e920cc60636097cf"), //circle of death
                library.Get<BlueprintAbility>("76a11b460be25e44ca85904d6806e5a3"), //create undead
                library.Get<BlueprintAbility>("08323922485f7e246acb3d2276515526"), //horrid witling
                library.Get<BlueprintAbility>("b24583190f36a8442b212e45226c54fc"), //wail of banshee
            };


            var armor_of_bones = mystery_engine.createArmorOfBones("ArmorOfBonesOracleRevelation",
                                                                   "Armor of Bones ",
                                                                   "You can conjure armor made of bones that grants you a +4 armor bonus. At 7th level, and every four levels thereafter, this bonus increases by +2. At 13th level, this armor grants you Damage Reduction 5/bludgeoning. You can use this armor for 1 hour per day per oracle level. This duration does not need to be consecutive, but it must be spent in 1-hour increments.");
            var bleeding_wounds = mystery_engine.createBleedingWounds("BleedingWoundsOracleRevelation",
                                                                      "Bleeding Wounds ",
                                                                      "Whenever a creature takes damage from one of your spells or effects that causes negative energy damage (such as inflict light wounds or the death’s touch revelation), it begins to bleed, taking 1 point of damage each round. At 5th level, and every five levels thereafter, this damage increases by 1. The bleeding can be stopped by a DC 15 Heal check or any effect that heals damage.");
            var deaths_touch = mystery_engine.createDeathsTouch("DeathsTouchOracleRevelation",
                                                                "Death’s Touch",
                                                                Main.settings.balance_fixes
                                                                ? "You can cause terrible wounds to appear on a creature with a melee touch attack. This attack deals 1d6 points of negative energy damage plus 1d6 points for every two oracle levels you possess beyond first. If used against an undead creature, it heals damage and grants a +2 channel resistance for 1 minute. You can use this ability a number of times per day equal to 3 + your Charisma modifier."
                                                                : "You can cause terrible wounds to appear on a creature with a melee touch attack. This attack deals 1d6 points of negative energy damage +1 point for every two oracle levels you possess. If used against an undead creature, it heals damage and grants a +2 channel resistance for 1 minute. You can use this ability a number of times per day equal to 3 + your Charisma modifier.");
            var near_death = mystery_engine.createNearDeath("NearDeathOracleRevelation",
                                                            "Near Death",
                                                            "You gain a +2 insight bonus on saves against diseases, mind-affecting effects, and poisons. At 7th level, this bonus also applies on saves against death effects, sleep effects, and stunning. At 11th level, the bonus increases to +4.");
            var raise_the_dead = mystery_engine.createRaiseTheDead("RaiseTheDeadOracleRevelation",
                                                                   "Raise the Dead",
                                                                   "As a standard action, you can summon a single skeletal champion to fight for you. The undead creature's power scales with your oracle level. It remains for a number of rounds equal to your Charisma modifier. At 15th level, you can summon one grave knight or living armor instead. You can use this ability once per day plus one additional time per day at 10th level.");
            var resist_life = mystery_engine.createResistLife("ResistLifeOracleRevelation",
                                                              "Resist Life",
                                                              "You are treated as an undead creature when you are targeted by positive or negative energy. You are not subject to Turn Undead or Command Undead (or any other effect that specifically targets undead), unless you are actually an undead creature. At 7th level, you receive channel resistance +2. This bonus increases by +2 at 11th and 15th level.");
            var soul_siphon = mystery_engine.createSoulSiphon("SoulSiphonOrcalRevelation",
                                                              "Soul Siphon",
                                                              "As a ranged touch attack, you can unleash a ray that causes a target to gain one negative level. The ray has a range of 25 feet. This negative level lasts for a number of minutes equal to your Charisma modifier. Whenever this ability gives a target a negative level, you heal a number of hit points equal to your oracle level. You can use this ability once per day, plus one additional time at 11th level and every four levels thereafter. You must be at least 7th level to select this revelation.");
            var undead_servitude = mystery_engine.createUndeadServitude("UndeadServitudeOracleRevelation",
                                                                        "Undead Servitude",
                                                                        "You can control undead, as per the spell using your oracle level as caster level. You can use this ability a number of times per day equal to 3 + your Charisma modifier.");

            var animate_dead = library.CopyAndAdd<BlueprintAbility>("4b76d32feb089ad4499c3a1ce8e1ac27", "BonesManifestationAnimateDeadAbility", "");
            animate_dead.Type = AbilityType.Supernatural;
            animate_dead.RemoveComponents<SpellListComponent>();
            animate_dead.RemoveComponents<SpellComponent>();
            animate_dead.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getOracleArray()));
            animate_dead.MaterialComponent = library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3").MaterialComponent;//fireball
            var power_word_kill = library.CopyAndAdd<BlueprintAbility>("2f8a67c483dfa0f439b293e094ca9e3c", "BonesOracleFinalRevelationPowerWordKillAbility", "");
            power_word_kill.RemoveComponents<SpellListComponent>();
            power_word_kill.RemoveComponents<SpellComponent>();
            power_word_kill.Type = AbilityType.Supernatural;
            power_word_kill.SetDescription(power_word_kill.Description.Replace("101", "151"));
            power_word_kill.ReplaceComponent<AbilityTargetHPCondition>(a => a.CurrentHPLessThan = 151);
            var new_actions = Common.changeAction<Conditional>(power_word_kill.GetComponent<AbilityEffectRunAction>().Actions.Actions,
                                                               c => c.ConditionsChecker = Helpers.CreateConditionsCheckerOr(Helpers.Create<ContextConditionCompareTargetHP>(h => h.Value = 151))
                                                               );
            power_word_kill.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(new_actions));
            var resource = Helpers.CreateAbilityResource("BonesOracleFinalRevelationResource", "", "", "", null);
            resource.SetFixedResource(1);
            power_word_kill.AddComponent(Helpers.CreateResourceLogic(resource));

            var final_revelation = Helpers.CreateFeature("BonesFinalRevelationFeature",
                                                  "Final Revelation",
                                                  "Upon reaching 20th level, you become a master of death. You can cast animate dead at will without paying a material component cost, although you are still subject to the usual Hit Dice control limit. Once per day, you can cast power word kill, but the spell can target a creature with 150 hit points or fewer.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFacts(animate_dead, power_word_kill),
                                                  Helpers.CreateAddAbilityResource(resource)
                                                  );

            bones_mystery = createMystery("BonesOracleMystery", "Bones", Helpers.GetIcon("a1a8bf61cadaa4143b2d4966f2d1142e"), //undead bloodline
                             final_revelation,
                             new StatType[] { StatType.SkillStealth},
                             spells,
                             false, false,
                             armor_of_bones, bleeding_wounds, deaths_touch, near_death,
                             raise_the_dead, resist_life, soul_siphon, undead_servitude
                             );
        }


        static void createNatureMystery()
        {
            var spells = new BlueprintAbility[9]
            {
                    library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33"), //Magic Fang
                    library.Get<BlueprintAbility>("5b77d7cc65b8ab74688e74a37fc2f553"), //barkskin
                    library.Get<BlueprintAbility>("754c478a2aa9bb54d809e648c3f7ac0e"), //dominate animal
                    library.Get<BlueprintAbility>("e418c20c8ce362943a8025d82c865c1c"), //cape of wasps
                    library.Get<BlueprintAbility>("6d1d48a939ce475409f06e1b376bc386"), //vinetrap
                    library.Get<BlueprintAbility>("7c5d556b9a5883048bf030e20daebe31"), //stone skin communal
                    library.Get<BlueprintAbility>("b974af13e45639a41a04843ce1c9aa12"), //creeping doom
                    library.Get<BlueprintAbility>("7cfbefe0931257344b2cb7ddc4cdff6f"), //stormbolts
                    library.Get<BlueprintAbility>("d8144161e352ca846a73cf90e85bf9ac"), //tsunami
            };


            animal_companion = mystery_engine.createAnimalCompanion("AnimalCompanionOracleRevelation",
                                                                    "Animal Companion",
                                                                    "You gain the service of a faithful animal. This animal functions as a druid’s animal companion, using your oracle level as your effective druid level.");
            var erosion_touch = mystery_engine.createErosionTouch("ErosionTouchOracleRevelation",
                                                                  "Erosion Touch",
                                                                  $"As a melee touch attack, you can deal 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage per level to construct. You can use this ability once per day, plus one time per day for every three levels you possess.");
            var friend_to_animals = mystery_engine.createFriendToAnimals("FriendToAnimalsOracleRevelation",
                                                                         "Friend to Animals",
                                                                         "Animals within 30 feet of you receive a bonus on all saving throws equal to your Charisma modifier.");
            var life_leach = mystery_engine.createLifeLich("LifeLichOracleRevelation",
                                                           "Life Lich",
                                                           $"You can draw life force from the bodies of enemies and channel it into yourself. As a standard action, you can drain the life essence from one living target within 30 feet. The target takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage per two levels you possess (maximum 10d{BalanceFixes.getDamageDieString(DiceType.D6)} ). You gain temporary hit points equal to the damage you deal. You can’t gain more than the target’s current hit points + the target’s Constitution score (which is enough to kill the subject). The temporary hit points last a number of hours equal to your Charisma modifier. The target receives a Fortitude save to halve the damage (and the temporary hit points you gain). You may use this ability once per day at 7th level, plus one additional time per day for every 4 levels you possess beyond 7th. You must be at least 7th level before selecting this revelation.");
            var natures_whispers = mystery_engine.createNaturesWhispers("NatureWhispersOracleRevelation",
                                                                        "Nature’s Whispers",
                                                                        "You have become so attuned to the whispers of the natural world, from the croaking of frogs to the groaning of great boulders, that your surroundings constantly keep you preternaturally aware of danger. You may add your Charisma modifier, instead of your Dexterity modifier, to your Armor Class and CMD. Any condition that would cause you to lose your Dexterity modifier to your Armor Class instead causes you to lose your Charisma modifier to your Armor Class.");
            var spirit_of_nature = mystery_engine.createSpiritOfNature("SpiritOfNatureOracleRevelation",
                                                                       "Spirit of Nature",
                                                                       "Whenever the oracle is reduced to below 25% hit points, she gains fast healing 1 for 1d4 rounds. At 15th level, this increases to fast healing 3.");
            var form_of_the_beast = mystery_engine.createFormOfTheBeast("FormOfTheBeastOracleRevelation",
                                                                        "Form of the Beast",
                                                                        "As a standard action, you can assume the form of an animal, as beast shape I. At 9th level, it works as beast shape II. At 11th level, as beast shape III. At 13th level, you can assume a form of magical beast as beast shape IV. You can use this ability once per day, but the duration is 1 hour/level. You must be at least 7th level to select this revelation.");
            var gift_of_claw_and_horn = mystery_engine.createGiftOfClawAndHorn("GiftOfClawAndHornOracleRevelation",
                                                                               "Gift of Claw and Horn",
                                                                               "As a swift action, you gain a natural weapon. The natural weapon lasts for a number of rounds equal to half your oracle level(minimum 1).You must choose a bite, claw, or gore attack. These attacks deal the normal damage for a creature of your size.At 5th level, your natural weapon gains a + 1 enhancement bonus.This bonus increases by + 1 at 10th, 15th, and 20th level.At 11th level, you gain two natural weapons at a time. You can use this ability a number of times per day equal to 3 + your Charisma modifier.");




            var resource = Helpers.CreateAbilityResource("NatureOracleFinalRevelationResource", "", "", "", null);
            resource.SetFixedResource(1);
            var friend_to_animals_buff = friend_to_animals.GetComponent<AuraFeatureComponent>().Buff.GetComponent<AddAreaEffect>().AreaEffect.GetComponent<AbilityAreaEffectBuff>().Buff;
            var apply_friend_to_animals = Common.createContextActionApplyBuff(friend_to_animals_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);

            var animal_fact = library.Get<BlueprintFeature>("a95311b3dc996964cbaa30ff9965aaf6");
            var plant_fact = library.Get<BlueprintFeature>("706e61781d692a042b35941f14bc41c5");

            var animal_buff = Helpers.CreateBuff("NatureOracleFinalRevelationAnimalBuff",
                                                 "Nature Final Revelation: Animal",
                                                 "At 20th level, you have discovered the intrinsic secrets of life itself, granting you incredible control over your own body. Once per day, she can change her type to plant, animal, or humanoid, and gain superficial physical characteristics of the chosen type as appropriate. She must choose a type that is different from her current type. This change doesn’t alter her Hit Dice, hit points, saving throws, skill ranks, class skills, or proficiencies.",
                                                 "",
                                                 library.Get<BlueprintAbility>("de7a025d48ad5da4991e7d3c682cf69d").Icon, //cat's grace
                                                 null,
                                                 Helpers.CreateAddFact(animal_fact),
                                                 Helpers.CreateAddFactContextActions(Helpers.CreateConditional(Helpers.CreateConditionCasterHasFact(friend_to_animals), apply_friend_to_animals),
                                                                                     Common.createContextActionRemoveBuff(friend_to_animals_buff))
                                                 );
            var apply_animal = Common.createContextActionApplyBuff(animal_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var plant_buff = Helpers.CreateBuff("NatureOracleFinalRevelationPlantBuff",
                                                 "Nature Final Revelation: Plant",
                                                 animal_buff.Description,
                                                 "",
                                                 library.Get<BlueprintAbility>("0fd00984a2c0e0a429cf1a911b4ec5ca").Icon, //entnagle
                                                 null,
                                                 Helpers.CreateAddFact(plant_fact)
                                                 );
            var apply_plant = Common.createContextActionApplyBuff(plant_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var precast_actions = Common.createContextActionOnContextCaster(Common.createContextActionRemoveBuff(animal_buff), Common.createContextActionRemoveBuff(plant_buff));


            var manifestation_animal = Helpers.CreateAbility("NatureOracleFinaleRevelationAnimalAbility",
                                                            animal_buff.Name,
                                                            animal_buff.Description,
                                                            "",
                                                            animal_buff.Icon,
                                                            AbilityType.Supernatural,
                                                            CommandType.Standard,
                                                            AbilityRange.Personal,
                                                            "Permanent",
                                                            "",
                                                            Helpers.CreateRunActions(apply_animal),
                                                            Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(precast_actions)),
                                                            Common.createAbilitySpawnFx("b5fc8209a9e75ff47acfd132540e0ba6", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                            Common.createAbilityCasterHasNoFacts(animal_fact),
                                                            Helpers.CreateResourceLogic(resource)
                                                            );
            Common.setAsFullRoundAction(manifestation_animal);
            var manifestation_plant = Helpers.CreateAbility("NatureOracleFinalRevelationPlantAbility",
                                                            plant_buff.Name,
                                                            plant_buff.Description,
                                                            "",
                                                            plant_buff.Icon,
                                                            AbilityType.Supernatural,
                                                            CommandType.Standard,
                                                            AbilityRange.Personal,
                                                            "Permanent",
                                                            "",
                                                            Helpers.CreateRunActions(apply_plant),
                                                            Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(precast_actions)),
                                                            Common.createAbilitySpawnFx("814caa282b28ef04e8b651551c782a88", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                            Common.createAbilityCasterHasNoFacts(plant_fact),
                                                            Helpers.CreateResourceLogic(resource)
                                                            );
            Common.setAsFullRoundAction(manifestation_plant);
            var manifestation_humanoid = Helpers.CreateAbility("NatureOracleFinalRevelationHumanoidAbility",
                                                                "Nature Final Revelation: Humanoid",
                                                                plant_buff.Description,
                                                                "",
                                                                library.Get<BlueprintAbility>("bd09b025ee2a82f46afab922c4decca9").Icon, //turn back
                                                                AbilityType.Supernatural,
                                                                CommandType.Standard,
                                                                AbilityRange.Personal,
                                                                "Permanent",
                                                                "",
                                                                Helpers.CreateRunActions(precast_actions),
                                                                Common.createAbilityCasterHasFacts(plant_fact, animal_fact),
                                                                Helpers.CreateResourceLogic(resource)
                                                                );
            Common.setAsFullRoundAction(manifestation_humanoid);

            var wrapper = Common.createVariantWrapper("NatureOracleFinalRevelationAbility", "", manifestation_animal, manifestation_humanoid, manifestation_plant);
            wrapper.SetName("Nature Final Revelation");
            wrapper.SetIcon(manifestation_plant.Icon);

            var final_revelation = Helpers.CreateFeature("NatureFinalRevelationFeature",
                                                          wrapper.Name,
                                                          wrapper.Description,
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFact(wrapper),
                                                          Helpers.CreateAddAbilityResource(resource)
                                                          );

            nature_mystery = createMystery("NatureOracleMystery", "Nature", wrapper.Icon, //charged blast
                                         final_revelation,
                                         new StatType[] { StatType.SkillLoreNature, StatType.SkillMobility },
                                         spells,
                                         true, true,
                                         animal_companion, erosion_touch, friend_to_animals, life_leach,
                                         natures_whispers, spirit_of_nature, form_of_the_beast, gift_of_claw_and_horn
                                         );
        }


        static void createWavesMystery()
        {
            var spells = new BlueprintAbility[9]
            {
                    NewSpells.frost_bite,
                    library.Get<BlueprintAbility>("b6010dda6333bcf4093ce20f0063cd41"), //frigid touch
                    NewSpells.sleet_storm,
                    library.Get<BlueprintAbility>("fcb028205a71ee64d98175ff39a0abf9"), //ice storm
                    library.Get<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931"), //icy prison
                    NewSpells.fluid_form,
                    NewSpells.ice_body,
                    library.Get<BlueprintAbility>("7ef49f184922063499b8f1346fb7f521"), //seamantle
                    library.Get<BlueprintAbility>("d8144161e352ca846a73cf90e85bf9ac"), //tsunami
            };

            var blizzard = mystery_engine.createBlizzard("BlizzardOracleRevelation",
                                                             "Blizzard",
                                                             $"As a standard action, you can create a blizzard of snow and ice. You can create 20-foot radius blizzard storm within close range. Any creature caught in the blizzard takes 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of cold damage per oracle level, with a Reflex save resulting in half damage. The storm lasts for a number of rounds equal to your Charisma modifier; the ground remains icy (+5 to Acrobatics DCs) as long as local conditions permit. The blizzard obscures sight beyond 5 feet, providing total concealment. A creature within 5 feet has concealment. You can use this ability once per day. You must be 11th level to select this revelation.");
            var fluid_nature = mystery_engine.createFluidNature("FluidNatureOracleRevelation",
                                                             "Fluid Nature",
                                                             "You receive a +4 bonus to your Combat Maneuver Defense against bull rush, grapple, and trip attempts. A creature trying to confirm a critical hit against you has a –4 penalty on its confirmation roll. At 5th level, you gain Dodge as a bonus feat. You do not need to meet the prerequisite to gain this feat.");
            var freezing_spells = mystery_engine.createFreezingSpells("FreezingSpellsOracleRevelation",
                                                               "Freezing Spells",
                                                               "Whenever a creature fails a saving throw and takes cold damage from one of your spells, it is slowed (as the slow spell) for 1 round. Spells that do not allow a save do not slow creatures. At 11th level, the duration increases to 1d4 rounds.");
            var ice_armor = mystery_engine.createIceArmor("IceArmorOracleRevelation",
                                                          "Ice Armor",
                                                          "You can conjure armor of ice that grants you a +4 armor bonus. At 7th level, and every four levels thereafter, this bonus increases by +2. At 13th level, this armor grants you DR 5/piercing. You can use this armor for 1 hour per day per oracle level. This duration does not need to be consecutive, but it must be spent in 1-hour increments.");
            var icy_skin = mystery_engine.createIcySkin("IcySkinOracleRevelation",
                                                       "Icy Skin",
                                                       "You gain resist cold 5. This resistance increases to 10 at 5th level and 20 at 11th level. At 17th level, you gain immunity to cold.");
            var water_form = mystery_engine.createWaterForm("WaterFormOracleRevelation",
                                                            "Water Form",
                                                            "As a standard action, you can assume the form of a Small water elemental, as elemental body I. At 9th level, you can assume the form of a Medium water elemental, as elemental body II. At 11th level, you can assume the form of a Large water elemental, as elemental body III. At 13th level, you can assume the form of a Huge water elemental, as elemental body IV. You can use this ability once per day, but the duration is 1 hour/level. You must be at least 7th level to select this revelation.");
            var wintry_touch = mystery_engine.createWintryTouch("WintryTouchOracleRevelation",
                                                            "Wintry Touch",
                                                            Main.settings.balance_fixes
                                                            ? $"As a standard action, you can perform a melee touch attack that deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of cold damage plus 1d{BalanceFixes.getDamageDieString(DiceType.D6)} point for every two oracle levels you possess beyond first. You can use the wintry touch ability a number of times per day equal to 3 + your Charisma modifier. At 11th level, any weapon that you wield is treated as a frost weapon."
                                                            : "As a standard action, you can perform a melee touch attack that deals 1d6 points of cold damage + 1 point for every two oracle levels you possess. You can use the wintry touch ability a number of times per day equal to 3 + your Charisma modifier. At 11th level, any weapon that you wield is treated as a frost weapon.");
            var punitive_transformation = mystery_engine.createPunitiveTransformation("PunitiveTransformationOracleRevelation",
                                                            "Punitive Transformation",
                                                            "You can transform an opponent into a harmless animal as if using baleful polymorph. This transformation lasts 1 round per oracle level. Transforming another creature causes the first to immediately revert to normal. You may use this ability a number of times per day equal to your Charisma modifier. You must be at least 7th level before selecting this revelation.");

            var final_revelation = Helpers.CreateFeature("FinalRevelationWavesMystery",
                                                         "Final Revelation",
                                                         "Upon reaching 20th level, you become a master of cold. You can apply any one of the following feats to any cold or water spell without increasing the level or casting time: Reach Spell or Extend Spell.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None);

            var extend = Common.CreateMetamagicAbility(final_revelation, "Extend", "Extend Spell (Cold)", Kingmaker.UnitLogic.Abilities.Metamagic.Extend, SpellDescriptor.Cold | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water, "", "", library.Get<BlueprintAbility>("40681ea748d98f54ba7f5dc704507f39").Icon);
            extend.Group = ActivatableAbilityGroupExtension.ShamanFlamesMetamagic.ToActivatableAbilityGroup();
            var reach = Common.CreateMetamagicAbility(final_revelation, "Reach", "Reach Spell (Cold)", Kingmaker.UnitLogic.Abilities.Metamagic.Reach, SpellDescriptor.Cold | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water, "", "", library.Get<BlueprintAbility>("40681ea748d98f54ba7f5dc704507f39").Icon);
            reach.Group = ActivatableAbilityGroupExtension.ShamanFlamesMetamagic.ToActivatableAbilityGroup();
            final_revelation.AddComponent(Helpers.CreateAddFacts(extend, reach));

            waves_mystery = createMystery("WavesOracleMystery", "Waves", library.Get<BlueprintAbility>("40681ea748d98f54ba7f5dc704507f39").Icon, //charged blast
                                         final_revelation,
                                         new StatType[] { StatType.SkillLoreNature, StatType.SkillMobility},
                                         spells,
                                         true, true,
                                         blizzard, fluid_nature, freezing_spells, ice_armor,
                                         icy_skin, water_form, wintry_touch, punitive_transformation
                                         );
        }


        static void createDragonMysteries()
        {
            List<DragonInfo> dragon_info = new List<DragonInfo>();
            dragon_info.Add(new DragonInfo("Black", "60-foot line", DamageEnergyType.Acid));
            dragon_info.Add(new DragonInfo("Blue",  "60-foot line", DamageEnergyType.Electricity));
            dragon_info.Add(new DragonInfo("Brass", "60-foot line", DamageEnergyType.Fire));
            dragon_info.Add(new DragonInfo("Bronze", "60-foot line", DamageEnergyType.Electricity));
            dragon_info.Add(new DragonInfo("Copper", "60-foot line", DamageEnergyType.Acid));
            dragon_info.Add(new DragonInfo("Gold", "30-foot cone", DamageEnergyType.Fire));
            dragon_info.Add(new DragonInfo("Green", "30-foot cone", DamageEnergyType.Acid));
            dragon_info.Add(new DragonInfo("Red", "30-foot cone", DamageEnergyType.Fire));
            dragon_info.Add(new DragonInfo("Silver", "30-foot cone", DamageEnergyType.Cold));
            dragon_info.Add(new DragonInfo("White", "30-foot cone", DamageEnergyType.Cold));


            var spells = new BlueprintAbility[9]
            {
                library.Get<BlueprintAbility>("bd81a3931aa285a4f9844585b5d97e51"), //cause fear
                library.Get<BlueprintAbility>("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                NewSpells.fly,
                library.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0"), //fear
                library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889"), //spell resistance
                library.Get<BlueprintAbility>("f0f761b808dc4b149b08eaf44b99f633"), //dispel magic greater
                library.Get<BlueprintAbility>("4cf3d0fae3239ec478f51e86f49161cb"), //true seeing
                library.Get<BlueprintAbility>("1cdc4ad4c208246419b98a35539eafa6"), //form of the dragon 3
                library.Get<BlueprintAbility>("41cf93453b027b94886901dbfc680cb9") //overwhelming presence
            };


            var dragon_magic = mystery_engine.createDragonMagic("DragonMagicOracleRevelation",
                                                                    "Dragon Magic",
                                                                    "Your draconic power grants you a limited form of access to arcane magic. Add one spell to your spells known from the sorcerer/wizard spell list that is 2 levels lower than the highest-level spell you can cast.");
            var dragon_senses = mystery_engine.createDragonSenses("DragonSensesOracleRevelation",
                                                                  "Dragon Senses",
                                                                  "Your senses take on a keen draconic edge. You gain a +2 bonus on Perception checks. At 5th level this bonus increases to +4. At 11th level, you gain blindsense with a range of 30 feet. At the 15th level, your blindsense range increases to 60 feet.");
            var presence_of_dragon = mystery_engine.createPresenceOfDragons("PresenceOfDragonsOracleRevelation",
                                                                            "Presence of Dragons",
                                                                            "Those who would oppose you must overcome their fear of dragons or be struck with terror at your draconic majesty. As a swift action, you can manifest an aura of draconic might around yourself. Enemies within 30 feet who can see you when you activate this ability must attempt a Will save. Success means that the creature is immune to this ability for the following 24 hours. On a failed save, the opponent is shaken for 2d6 rounds. This is a mind-affecting fear effect. You can use this ability once per day at 1st level, plus one additional time per day at 5th level and for every 5 levels beyond 5th.");
            var scaled_toughness = mystery_engine.createScaledToughness("ScaledToughnessOracleRevelation",
                                                                        "Scaled Toughness",
                                                                        "You can manifest the scaly toughness of dragonkind. Once per day as a swift action, you can harden your skin, giving it a scaly appearance and granting you DR 10/magic. During this time, you are also immune to paralysis and sleep effects. This effect lasts for a number of rounds equal to your oracle level. At 13th level, you can use this ability twice per day. You must be at least 7th level to select this revelation.");

            foreach (var di in dragon_info)
            {
                var breath_weapon = mystery_engine.createBreathWeapon($"BreathWeapon{di.name}OracleRevelation",
                                                                      "Breath Weapon",
                                                                      $"The primal power of dragonkind seethes within you. You gain a breath weapon. This breath weapon deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage of your energy type per 2 oracle levels you have (minimum 1d{BalanceFixes.getDamageDieString(DiceType.D6)} ; Reflex half ). The shape of the breath weapon is either a 30-foot cone or a 60- foot line, depending on the dragon type. You can use this ability once per day at 1st level, plus one additional time at 5th level and one additional time per day for every 5 levels beyond 5th.",
                                                                      di.breath_weapon_prototype);

                var draconic_resistances = mystery_engine.createDraconicResistance($"DraconicResistances{di.name}OracleRevelation",
                                                                                  "Draconic Resistances",
                                                                                  "Like the great dragons, you are not easily harmed by common means of attack. You gain resistance 5 against your chosen energy type and a +1 natural armor bonus. At 9th level, your energy resistance increases to 10 and your natural armor bonus increases to +2. At 15th level, your energy resistance increases to 20 and your natural armor bonus increases to +4.",
                                                                                  di.energy,
                                                                                  di.resist_icon);
                var form_of_the_dragon = mystery_engine.createFormOfTheDragon($"FormOfTheDragon{di.name}OracleRevelation",
                                                                              "Form of the Dragon",
                                                                              "Your kinship with dragonkind allows you to take on the form of a dragon. As a standard action, you can assume the form of a Medium dragon, as per form of the dragon I. At 15th level, you can assume the form of a Large dragon, as per form of the dragon II. At 19th level, you can assume the form of a Huge dragon, as per form of the dragon III. You can use this ability once per day, but the duration is 10 minutes per oracle level. If you are at least 15th level and choose to have this ability function as per form of the dragon I, the duration is instead 1 hour per oracle level. You must be at least 11th level to select this revelation.",
                                                                              di.dragon_form_id[0], di.dragon_form_id[1], di.dragon_form_id[2]);
                var wings_of_the_dragon = mystery_engine.createWingsOfDragon($"WingsOfTheDragon{di.name}OracleRevelation",
                                                                              di.wings);

                var final_revelation = Helpers.CreateFeature($"FinalRevelationDragon{di.name}Mystery",
                                                            "Final Revelation",
                                                            "Upon reaching 20th level, your draconic destiny unfolds. You gain immunity to paralysis, sleep, and damage of your energy type. You count as a dragon for the purposes of spells and magical effects. If you have the breath weapon revelation, you can use your breath weapon an unlimited number of times per day, though no more often than once every 1d4+1 rounds.",
                                                            "",
                                                            null,
                                                            FeatureGroup.None,
                                                            Helpers.CreateAddFact(Common.dragon),
                                                            Helpers.Create<AddEnergyDamageImmunity>(a => a.EnergyType = di.energy)
                                                            );
                var dragon_mystery = createMystery("Dragon" + di.name + "OracleMystery",$"Dragon ({di.name})", Helpers.GetIcon(di.dragon_form_id[0]),
                                                 final_revelation,
                                                 new StatType[] { StatType.SkillPerception },
                                                 spells,
                                                 false, false,
                                                 dragon_magic, dragon_senses, presence_of_dragon, scaled_toughness,
                                                 breath_weapon, draconic_resistances, form_of_the_dragon, wings_of_the_dragon
                                                 );
                dragon_mystery.SetDescription("Energy type: " + di.energy.ToString().ToLower() + ".\n"
                                              + "Breath weapon shape: " + di.breath_weapon_area + ".\n"
                                              + dragon_mystery.Description);
                dragon_mysteries.Add(dragon_mystery);
            }
        }


        static void createWindMystery()
        {
            var spells = new BlueprintAbility[9]
            {
                library.Get<BlueprintAbility>("ab395d2335d3f384e99dddee8562978f"), //shocking grasp
                library.Get<BlueprintAbility>("c28de1f98a3f432448e52e5d47c73208"), //protection from arrows
                library.Get<BlueprintAbility>("d2cff9243a7ee804cb6d5be47af30c73"), //lightning bolt
                NewSpells.river_of_wind,
                library.Get<BlueprintAbility>("16fff43f034133a4a86e914a523e021f"), //summon elemental large air
                library.Get<BlueprintAbility>("093ed1d67a539ad4c939d9d05cfe192c"), //sirocco
                NewSpells.scouring_winds,
                library.Get<BlueprintAbility>("333efbf776ab61c4da53e9622751d95f"), //summon elemental elder air
                NewSpells.winds_of_vengeance
            };

            var air_barrier = mystery_engine.createSpiritShield("AirBarrierOracleRevelation",
                                                                "Air Barrier",
                                                                "You can create an invisible shell of air that grants you a +4 armor bonus. At 7th level, and every four levels thereafter, this bonus increases by +2. At 13th level, this barrier causes incoming arrows, rays, and other ranged attacks requiring an attack roll against you to have a 50% miss chance. You can use this barrier for 1 hour per day per oracle level. This duration does not need to be consecutive, but it must be spent in 1-hour increments.");
            var invisibility = mystery_engine.createInvisibility("InvisibilityOracleRevelation",
                                                                "Invisibility",
                                                                "As a standard action, you can become invisible (as per the invisibility spell). You can remain invisible for 1 minute per day per oracle level. This duration does not need to be consecutive, but it must be spent in 1-minute increments. Starting at 9th level, each time you activate this ability you can treat it as greater invisibility, though each round spent this way counts as 1 minute of your normal invisibility duration. You must be at least 3rd level to select this revelation.");
            var lightning_breath = mystery_engine.createLightningBreath("LightningBreathOracleRevelation",
                                                               "Lightning Breath",
                                                               $"As a standard action, you can breathe a 30-foot line of electricity. This line deals 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of electricity damage per oracle level. A Reflex save halves this damage. You can use this ability once per day, plus one additional time per day at 5th level and every five levels thereafter.");
            var thunderburst = mystery_engine.createThunderburst("ThunderBurstOracleRevelation",
                                                                 "Thunderburst",
                                                                 $"As a standard action, you can create a blast of air accompanied by a loud peal of thunder. The blast has a medium range and has a 20-foot radius. Creatures in the area take 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of bludgeoning damage per oracle level and are deafened for 1 hour, with a Fortitude save resulting in half damage and no deafness. You must be at least 7th level to select this revelation. You can use this ability once per day, plus one additional time per day at 11th level and every four levels thereafter.");
            var touch_of_electricity = mystery_engine.createTouchOfElectricity("TouchOfElectricityOracleRevelation",
                                                                             "Touch of Electricity",
                                                                             Main.settings.balance_fixes
                                                                             ? $"As a standard action, you can perform a melee touch attack that deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of electricity damage plus {BalanceFixes.getDamageDieString(DiceType.D6)} points for every two oracle levels you possess beyond first. You can use this ability a number of times per day equal to 3 + your Charisma modifier. At 11th level, any weapon that you wield is treated as a shock weapon."
                                                                             : "As a standard action, you can perform a melee touch attack that deals 1d6 points of electricity damage +1 point for every two oracle levels you possess. You can use this ability a number of times per day equal to 3 + your Charisma modifier. At 11th level, any weapon that you wield is treated as a shock weapon.");
            var vortex_spells = mystery_engine.createVortexSpells("VortexSpellsOracleRevelation",
                                                                 "Vortex Spells",
                                                                 "Whenever you score a critical hit against an opponent with an attack spell, the target is staggered for 1 round. At 11th level, the duration increases to 1d4 rounds.");
            var spark_skin = mystery_engine.createSparkSkin("SparkSkinOracleRevelation",
                                                            "Spark Skin",
                                                            "You gain resist electricity 5. This resistance increases to 10 at 5th level and 20 at 11th level. At 17th level, you gain immunity to electricity.");
            var wings_of_air = mystery_engine.CreateWingsOfAir("WingsOfAirOracleRevelation",
                                                                "Wings of Air",
                                                                "You gain a pair of wings that grant a +3 dodge bonus to AC against melee attacks and an immunity to ground based effects, such as difficult terrain. You must be at least 7th level to select this revelation.");

            var final_revelation = Helpers.CreateFeature("FinalRevelationWindMystery",
                                                         "Final Revelation",
                                                          "Upon reaching 20th level, you become a master of air and electricity. You can apply any one of the following feats to any air or electricity spell without increasing the level or casting time: Reach Spell or Extend Spell.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None);

            var extend = Common.CreateMetamagicAbility(final_revelation, "Extend", "Extend Spell (Electricity)", Kingmaker.UnitLogic.Abilities.Metamagic.Extend, SpellDescriptor.Electricity | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air, "", "", library.Get<BlueprintAbility>("093ed1d67a539ad4c939d9d05cfe192c").Icon);
            extend.Group = ActivatableAbilityGroupExtension.ShamanFlamesMetamagic.ToActivatableAbilityGroup();
            var reach = Common.CreateMetamagicAbility(final_revelation, "Reach", "Reach Spell (Electricity)", Kingmaker.UnitLogic.Abilities.Metamagic.Reach, SpellDescriptor.Electricity | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air, "", "", library.Get<BlueprintAbility>("093ed1d67a539ad4c939d9d05cfe192c").Icon);
            reach.Group = ActivatableAbilityGroupExtension.ShamanFlamesMetamagic.ToActivatableAbilityGroup();
            final_revelation.AddComponent(Helpers.CreateAddFacts(extend, reach));

            wind_mystery = createMystery("WindOracleMystery", "Wind", library.Get<BlueprintAbility>("093ed1d67a539ad4c939d9d05cfe192c").Icon, //sirocco
                                         final_revelation,
                                         new StatType[] { StatType.SkillLoreNature },
                                         spells,
                                         true, true,
                                         air_barrier, invisibility, lightning_breath, thunderburst,
                                         touch_of_electricity, vortex_spells, spark_skin, wings_of_air
                                         );
        }


        static void createLifeMystery()
        {
            var spells = new BlueprintAbility[9]
            {
                library.Get<BlueprintAbility>("f6f95242abdfac346befd6f4f6222140"), //remove sickness
                library.Get<BlueprintAbility>("e84fc922ccf952943b5240293669b171"), //lesser restoration
                SpellDuplicates.addDuplicateSpell("e7240516af4241b42b2cd819929ea9da", "ShamanLifeSpiritNeutralizePoison", ""), //neutralzie poison
                library.Get<BlueprintAbility>("f2115ac1148256b4ba20788f7e966830"), //restoration
                library.Get<BlueprintAbility>("d5847cad0b0e54c4d82d6c59a3cda6b0"), //breath of life
                library.Get<BlueprintAbility>("5da172c4c89f9eb4cbb614f3a67357d3"), //heal
                library.Get<BlueprintAbility>("fafd77c6bfa85c04ba31fdc1c962c914"), //greater restoration
                SpellDuplicates.addDuplicateSpell("867524328b54f25488d371214eea0d90", "shamanLifeSpiritMassHeal", ""), //mass heal
                SpellDuplicates.addDuplicateSpell("80a1a388ee938aa4e90d427ce9a7a3e9", "shamanLifeSpiritResurrection", ""), //resurrection
            };

            var channel = mystery_engine.createChannel("ChannelOracleRevelation",
                                                           "Channel",
                                                           "You can channel positive energy like a cleric, using your oracle level as your effective cleric level when determining the amount of damage healed (or caused to undead) and the DC. You can use this ability a number of times per day equal to 1 + your Charisma modifier.");
            var combat_healer = mystery_engine.createCombatHealer("CombatHealerLifeMysteryOracleRevelation",
                                                                   "Combat Healer",
                                                                   "Whenever you cast a cure spell (a spell with “cure” in its name), you can cast it as a swift action, as if using the Quicken Spell feat, by expending two spell slots. This does not increase the level of the spell. You can use this ability once per day at 7th level and one additional time per day for every four levels beyond 7th. You must be at least 7th level to select this revelation (as the battle mystery revelation.)");
            var energy_body = mystery_engine.createEnergyBody("EnergyOracleRevelation",
                                                       "Energy Body",
                                                       $"As a standard action, you can transform your body into pure life energy, resembling a golden-white fire elemental. In this form, you gain the elemental subtype. Any undead creature striking you with its body or a handheld weapon deals normal damage, but at the same time the attacker takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of positive energy damage + 1 point per oracle level. Creatures wielding melee weapons with reach are not subject to this damage if they attack you. At the beginning of your turn your heal all allied adjacent creatures for 1d{BalanceFixes.getDamageDieString(DiceType.D6)} hit points + 1 per oracle level. You may return to your normal form as a free action. You may remain in energy body form for a number of rounds per day equal to your oracle level.");

            var enhanced_cures = mystery_engine.createEnchancedCures("EnhancedCuresOracleRevelation",
                                                                     "Enhanced Cures",
                                                                     $"Whenever you cast a cure spell, the maximum number of hit points healed is based on your oracle level, not the limit based on the spell. For example, an 11th-level oracle of life with this revelation may cast cure light wounds to heal 1d{BalanceFixes.getDamageDieString(DiceType.D8)}+11 hit points instead of the normal 1d{BalanceFixes.getDamageDieString(DiceType.D8)}+5 maximum.");
            var healing_hands = mystery_engine.createHealingHands("HealingHandsOracleRevelation",
                                                         "Healing Hands",
                                                         "You gain a +3 bonus to religion skill.");
            var life_link = mystery_engine.createLifeLink("LifeLinkOracleRevelation",
                                                          "Life Link",
                                                          "As a standard action, you may create a bond between yourself and another creature within 30 feet.  Each round at the start of your turn, if the bonded creature is wounded for 5 or more hit points below its maximum hit points, it heals 5 hit points and you take 5 hit points of damage. You may have one bond active per oracle level. This bond continues until the bonded creature dies, you die or you end it as a free action.");
            var safe_curing = mystery_engine.createSafeCuring("SafeCuringOracleRevelation",
                                             "Safe Curing",
                                             "Whenever you cast a spell that cures the target of hit point damage, you do not provoke attacks of opportunity for spellcasting.");
            var spirit_boost = mystery_engine.createSpiritBoost("SpiritBoostOracleRevelation",
                                                                 "Spirit Boost",
                                                                 "Whenever your healing spells heal a target up to its maximum hit points, any excess points persist for 1 round per level as temporary hit points (up to a maximum number of temporary hit points equal to your oracle level).");

            var conditions = SpellDescriptor.Bleed | SpellDescriptor.Death | SpellDescriptor.Exhausted | SpellDescriptor.Fatigue | SpellDescriptor.Nauseated | SpellDescriptor.Sickened;

            var final_revelation = Helpers.CreateFeature("FinalRevelationLifeMystery",
                                                         "Final Revelation",
                                                          "Upon reaching 20th level, you become a perfect channel for life energy. You become immune to bleed, death attacks, exhaustion, fatigue, nausea effects, negative levels, and sickened effects. Ability damage and drain cannot reduce you below 1 in any ability score. You also receive diehard feat for free.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Common.createBuffDescriptorImmunity(conditions),
                                                          Common.createSpellImmunityToSpellDescriptor(conditions),
                                                          UnitCondition.Exhausted.CreateImmunity(),
                                                          UnitCondition.Fatigued.CreateImmunity(),
                                                          UnitCondition.Nauseated.CreateImmunity(),
                                                          UnitCondition.Paralyzed.CreateImmunity(),
                                                          UnitCondition.Sickened.CreateImmunity(),
                                                          Helpers.Create<AddImmunityToEnergyDrain>(),
                                                          Helpers.Create<HealingMechanics.StatsCannotBeReducedBelow1>(),
                                                          Common.createAddFeatureIfHasFact(library.Get<BlueprintFeature>("c99f3405d1ef79049bd90678a666e1d7"), //diehard
                                                                                           library.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad"),
                                                                                           not: true)
                                                          );

            life_mystery = createMystery("LifeOracleMystery", "Life", Helpers.GetIcon("be2062d6d85f4634ea4f26e9e858c3b8"), // cleanse
                                         final_revelation,
                                         new StatType[] { StatType.SkillLoreNature },
                                         spells,
                                         true, true,
                                         channel, combat_healer, energy_body, enhanced_cures,
                                         healing_hands, life_link, safe_curing, spirit_boost
                                         );
        }


        static void createBattleMystery()
        {
            var spells = new BlueprintAbility[9]
            {
                library.Get<BlueprintAbility>("c60969e7f264e6d4b84a1499fdcf9039"), //enlarge person
                library.Get<BlueprintAbility>("c28de1f98a3f432448e52e5d47c73208"), //protection from arrows
                library.Get<BlueprintAbility>("2d4263d80f5136b4296d6eb43a221d7d"), //magical vestment,
                NewSpells.wall_of_fire,
                library.Get<BlueprintAbility>("90810e5cf53bf854293cbd5ea1066252"), //righteous might
                library.Get<BlueprintAbility>("6a234c6dcde7ae94e94e9c36fd1163a7"), //bulls strength mass
                library.Get<BlueprintAbility>("da1b292d91ba37948893cdbe9ea89e28"), //legendary proportions
                library.Get<BlueprintAbility>("7cfbefe0931257344b2cb7ddc4cdff6f"), //stormbolts
                library.Get<BlueprintAbility>("01300baad090d634cb1a1b2defe068d6"), //clashing rocks
            };

            var battlecry = mystery_engine.createBattleCry("BattlecryOracleRevelation",
                                                           "Battlecry",
                                                           "As a standard action, you can unleash an inspiring battlecry. All allies within 50 feet who hear your cry gain a +1 morale bonus on attack rolls, skill checks, and saving throws for a number of rounds equal to your Charisma modifier. At 10th level, this bonus increases to +2. You can use this ability once per day, plus one additional time per day at 5th level and for every five levels thereafter.");
            var combat_healer = mystery_engine.createCombatHealer("CombatHealerOracleRevelation",
                                                                  "Combat Healer",
                                                                  "Whenever you cast a cure spell (a spell with “cure” in its name), you can cast it as a swift action, as if using the Quicken Spell feat, by expending two spell slots. This does not increase the level of the spell. You can use this ability once per day at 7th level and one additional time per day for every four levels beyond 7th. You must be at least 7th level to select this revelation.");
            var iron_skin = mystery_engine.createIronSkin("IronSkinOracleRevelation",
                                                          "Iron Skin",
                                                          "Once per day, your skin hardens and takes on the appearance of iron, granting you DR 10/adamantine. This functions as stoneskin, using your oracle level as the caster level. At 15th level, you can use this ability twice per day. You must be at least 11th level to select this revelation.");
            var maneuver_mastery = mystery_engine.createManeuverMastery("ManeuverMasteryOracleRevelation",
                                              "Maneuver Mastery",
                                              "Select one type of combat maneuver. When performing the selected maneuver, you treat your oracle level as your base attack bonus when determining your CMB. At 7th level, you gain the Improved feat (such as Improved Trip) that grants you a bonus when performing that maneuver. At 11th level, you gain the Greater feat (such as Greater Trip) that grants you a bonus when performing that maneuver. You do not need to meet the prerequisites to receive these feats.");
            var skill_at_arms = mystery_engine.createSkillAtArms("SkillAtArmsOracleRevelation",
                                                                 "Skill at Arms",
                                                                 "You gain proficiency in all martial weapons and heavy armor.");
            var surprising_charge = mystery_engine.createSurprisingCharge("SurprisingChargeOracleRevelation",
                                                                          "Surprising Charge",
                                                                          "Once per day you can spend a swift action to gain pounce ability for one round. You can use this ability one additional time per day at 7th level and 15th level.");
            var war_sight = mystery_engine.createTemporalCelerity("WarSightOracleRevelation",
                                                                  "War Sight",
                                                                  "Whenever you roll for initiative, you can roll twice and take either result. At 7th level, you can always act in the surprise round, but if you fail to notice the ambush, you act last, regardless of your initiative result (you act in the normal order in following rounds). At 11th level, you can roll for initiative three times and take any one of the results.");
            var weapon_mastery = mystery_engine.createWeaponMastery("WeaponMasteryOracleRevelation",
                                                                  "Weapon Mastery",
                                                                  "Select one weapon with which you are proficient. You gain Weapon Focus with that weapon. At 8th level, you gain Improved Critical with that weapon. At 12th level, you gain Greater Weapon Focus with that weapon. You do not need to meet the prerequisites to receive these feats.");

            var final_revelation = Helpers.CreateFeature("FinalRevelationBattleMystery",
                                                          "Final Revelation",
                                                          "Upon reaching 20th level, you become an avatar of battle. You gain pounce ability and diehard feat. Whenever you score a critical hit, the attack ignores damage reduction. You gain a +4 insight bonus to AC for the purposes of confirming critical hits against you.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Common.createCriticalConfirmationACBonus(4),
                                                          library.Get<BlueprintFeature>("1a8149c09e0bdfc48a305ee6ac3729a8").GetComponent<AddMechanicsFeature>(), //pounce
                                                          Helpers.Create<IgnoreDamageReductionOnCriticalHit>(),
                                                          Common.createAddFeatureIfHasFact(library.Get<BlueprintFeature>("c99f3405d1ef79049bd90678a666e1d7"), //diehard
                                                                                           library.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad"),
                                                                                           not: true)
                                                          );
            battle_mystery = createMystery("BattleOracleMystery", "Battle", Helpers.GetIcon("c78506dd0e14f7c45a599990e4e65038"), //charge
                             final_revelation,
                             new StatType[] { StatType.SkillAthletics },
                             spells,
                             true, false,
                             battlecry, combat_healer, iron_skin, maneuver_mastery,
                             skill_at_arms, surprising_charge, war_sight, weapon_mastery
                             );
        }

        static void createFlameMystery()
        {
            var spells = new BlueprintAbility[9]
            {
                    library.Get<BlueprintAbility>("4783c3709a74a794dbe7c8e7e0b1b038"), //burning hands
                    library.Get<BlueprintAbility>("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                    library.Get<BlueprintAbility>("2d81362af43aeac4387a3d4fced489c3"), //fireball
                    NewSpells.wall_of_fire,
                    library.Get<BlueprintAbility>("b3a203742191449458d2544b3f442194"), //summon elemental large fire
                    NewSpells.fire_seeds,
                    SpellDuplicates.addDuplicateSpell("e3d0dfe1c8527934294f241e0ae96a8d", "FireStormShamanFlameSpiritAbility"),
                    NewSpells.incendiary_cloud,
                    library.Get<BlueprintAbility>("08ccad78cac525040919d51963f9ac39"), //fiery body
            };

            var burning_magic = mystery_engine.createBurningMagic("BurningMagicOracleRevelation",
                                                                  "Burning Magic",
                                                                  "Whenever a creature fails a saving throw and takes fire damage from one of your spells, it catches on fire. This fire deals 1 point of fire damage per spell level at the beginning of the burning creature’s turn. The fire lasts for 1d4 rounds, but it can be extinguished as a move action if the creature succeeds at a Reflex save (using the spell’s DC). Spells that do not grant a save do not cause a creature to catch on fire.");
            var cinder_dance = mystery_engine.createCinderDance("CinderDanceOracleRevelation",
                                                                "Cinder Dance",
                                                                "Your base speed increases by 10 feet. At 10th level, you can ignore difficult terrain when moving.");
            var fire_breath = mystery_engine.createFireBreath("FireBreathOracleRevelation",
                                                              "Fire Breath",
                                                              $"As a standard action, you can unleash a 15-foot cone of flame from your mouth. This flame deals 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of fire damage per level. A Reflex save halves this damage. You can use this ability once per day, plus one additional time per day at 5th level and every five levels thereafter. The save DC is Charisma-based.");
            var firestorm = mystery_engine.CreateFirestorm("FirestormOracleRevelation",
                                                           "Firestorm",
                                                           $"As a standard action, you can cause fire to erupt in 20-foot radius burst in any point within close range.  Any creature caught in these flames takes 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of fire damage per oracle level, with a Reflex save resulting in half damage. This fire lasts for a number of rounds equal to your Charisma modifier. You can use this ability once per day. You must be at least 11th level to select this revelation.");
            var form_of_flame = mystery_engine.createFormOfFlame("FormOfFlameOracleRevelation",
                                                                "Form of Flame",
                                                                "As a standard action, you can assume the form of a Small fire elemental, as elemental body I. At 9th level, you can assume the form of a Medium fire elemental, as elemental body II. At 11th level, you can assume the form of a Large fire elemental, as elemental body III. At 13th level, you can assume the form of a Huge fire elemental, as elemental body IV. You can use this ability once per day, but the duration is 1 hour/level. You must be at least 7th level to select this revelation.");
            var heat_aura = mystery_engine.createHeatAura("HeatAuraOracleRevelation",
                                                        "Heat Aura",
                                                        $"As a swift action, you can cause waves of heat to radiate from your body. This heat deals 1d{BalanceFixes.getDamageDieString(DiceType.D4)} points of fire damage per two oracle levels (minimum 1d4) to all creatures within 10 feet. A Reflex save halves the damage. In addition, your form wavers and blurs, granting you 20% concealment until your next turn. You can use this ability once per day, plus one additional time per day at 5th level and every five levels thereafter.");
            var touch_of_flame = mystery_engine.createTouchOfFlame("TouchOfFlameOracleRevelation",
                                                            "Touch of Flame",
                                                            Main.settings.balance_fixes
                                                            ? $"As a standard action, you can perform a melee touch attack that deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of fire damage plus 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points for every two oracle levels you possess beyond first. You can use this ability a number of times per day equal to 3 + your Charisma modifier. At 11th level, any weapon that you wield is treated as a flaming weapon."
                                                            : "As a standard action, you can perform a melee touch attack that deals 1d6 points of fire damage +1 point for every two oracle levels you possess. You can use this ability a number of times per day equal to 3 + your Charisma modifier. At 11th level, any weapon that you wield is treated as a flaming weapon.");
            var molten_skin = mystery_engine.createMoltenSkin("MoltenSkinOracleRevelation",
                                                                     "Molten Skin",
                                                                     "You gain resist fire 5. This resistance increases to 10 at 5th level and 20 at 11th level. At 17th level, you gain immunity to fire.");

            var final_revelation = Helpers.CreateFeature("FinalRevelationFlameMystery",
                                                  "Final Revelation",
                                                  "Upon reaching 20th level, you become a master of fire. You can apply any one of the following feats to any fire spell you cast without increasing the spell’s level or casting time: Reach Spell, Extend Spell. You do not need to possess these feats to use this ability.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None);

            var extend = Common.CreateMetamagicAbility(final_revelation, "Extend", "Extend Spell (Fire)", Kingmaker.UnitLogic.Abilities.Metamagic.Extend, SpellDescriptor.Fire, "", "", NewSpells.wall_of_fire.Icon);
            extend.Group = ActivatableAbilityGroupExtension.ShamanFlamesMetamagic.ToActivatableAbilityGroup();
            var reach = Common.CreateMetamagicAbility(final_revelation, "Reach", "Reach Spell (Fire)", Kingmaker.UnitLogic.Abilities.Metamagic.Reach, SpellDescriptor.Fire, "", "", NewSpells.wall_of_fire.Icon);
            reach.Group = ActivatableAbilityGroupExtension.ShamanFlamesMetamagic.ToActivatableAbilityGroup();
            final_revelation.AddComponent(Helpers.CreateAddFacts(extend, reach));

            flame_mystery = createMystery("FlameOracleMystery", "Flame", NewSpells.wall_of_fire.Icon, final_revelation,
                                         new StatType[] { StatType.SkillAthletics, StatType.SkillMobility },
                                         spells,
                                         true, false,
                                         burning_magic, cinder_dance, fire_breath, firestorm,
                                         form_of_flame, heat_aura, touch_of_flame, molten_skin
                                         );
        }


        static void createTimeMystery()
        {
            var spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("14c90900b690cac429b229efdf416127"), //longstrider
                library.Get<BlueprintAbility>("464a7193519429f48b4d190acb753cf0"), //grace
                NewSpells.sands_of_time,
                NewSpells.threefold_aspect,
                library.Get<BlueprintAbility>("41e8a952da7a5c247b3ec1c2dbb73018"), //hold monster
                NewSpells.contingency,
                library.Get<BlueprintAbility>("4aa7942c3e62a164387a73184bca3fc1"), //disintegrate
                NewSpells.temporal_stasis,
                NewSpells.time_stop,
            };

            var aging_touch = mystery_engine.createAgingTouch("AgingTouchOracleRevelation",
                                                              "Aging Touch",
                                                              $"Your touch ages living creatures and objects. As a melee touch attack, you can deal 1 point of Strength damage for every two oracle levels you possess to living creatures. Against constructs, you can deal 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage per oracle level. You can use this ability once per day, plus one additional time per day for every five oracle levels you possess.");
            var rewind_time = mystery_engine.createRewindTime("RewindTimeOracleRevelation",
                                                              "Rewind Time",
                                                              "Once per day, you can reroll any one failed d20. At 11th level, and every four levels thereafter, you can use this ability an additional time per day. You must be at least 7th level to select this revelation.");          
            var speed_or_slow_time = mystery_engine.createSpeedOrSlowTime("SpeedOrSlowTimeOracleRevelation",
                                                                          "Speed or Slow Time",
                                                                          "As a standard action, you can speed up or slow down time, as either the haste or slow spell. You can use this ability once per day, plus one additional time per day at 12th level and 17th level. You must be at least 7th level before selecting this revelation.");                     
            var temporal_celerity = mystery_engine.createTemporalCelerity("TemporalCelerityOracleRevelation",
                                                                          "Temporal Celerity",
                                                                          "Whenever you roll for initiative, you can roll twice and take either result. At 7th level, you can always act in the surprise round, but if you fail to notice the ambush, you act last, regardless of your initiative result (you act in the normal order in following rounds). At 11th level, you can roll for initiative three times and take any one of the results.");
            var time_flicker = mystery_engine.createTimeFlicker("TimeFlickerOracleRevelation",
                                                                "Time Flicker",
                                                                "As a standard action, you can flicker in and out of time, gaining concealment (as the blur spell). You can use this ability for 1 minute per oracle level that you possess per day. This duration does not need to be consecutive, but it must be spent in 1-minute increments. At 7th level, each time you activate this ability, you can treat it as the displacement spell, though each round spent this way counts as 1 minute of your normal time flicker duration. You must be at least 3rd level to select this revelation.");
            var time_hop = mystery_engine.createTimeHop("TimeHopOracleRevelation",
                                                        "Time Hop",
                                                        "As a move action, you can teleport up to 50 feet per 3 oracle levels, as the dimension door spell. This movement does not provoke attacks of opportunity. You must have line of sight to your destination to use this ability. You can bring other willing creatures with you, but you must expend 2 uses of this ability. You must be at least 7th level before selecting this revelation.");
            var time_sight = mystery_engine.createTimeSight("TimeSightOracleRevelation",
                                                            "Time Sight",
                                                            "You can peer through the mists of time to see things as they truly are, as if using the true seeing spell.\n" +
                                                            "At 18th level, this functions like foresight. You can use this ability for a number of minutes per day equal to your oracle level, but these minutes do not need to be consecutive. You must be at least 11th level to select this revelation.");
            var erase_from_time = mystery_engine.createEraseFromTime("EraseFromTimeOracleRevelation",
                                                                     "Erase From Time",
                                                                     "As a melee touch attack, you can temporarily remove a creature from time altogether. The target creature must make a Fortitude save or vanish completely for a number of rounds equal to 1/2 your oracle level (minimum 1 round). No magic or divinations can detect the creature during this time, as it exists outside of time and space—in effect, the creature ceases to exist for the duration of this ability. At the end of the duration, the creature reappears unharmed in the space it last occupied (or the nearest possible space, if the original space is now occupied). You can use this ability once per day, plus one additional time per day at 11th level.");

            var time_stop = Common.convertToSpellLike(NewSpells.time_stop, "OracleTimeMystery", getOracleArray(), StatType.Charisma);
            var final_revelation = Helpers.CreateFeature("FinalRevelationTimeMystery",
                                                         "Final Revelation",
                                                         "Upon reaching 20th level, you become a true master of time and stop aging. You receive +2 competence bonus to your mental stats. In addition, you can cast time stop once per day as a spell-like ability.",
                                                         "",
                                                         null,
                                                         FeatureGroup.None,
                                                         Helpers.CreateAddStatBonus(StatType.Wisdom, 2, ModifierDescriptor.Competence),
                                                         Helpers.CreateAddStatBonus(StatType.Intelligence, 2, ModifierDescriptor.Competence),
                                                         Helpers.CreateAddStatBonus(StatType.Charisma, 2, ModifierDescriptor.Competence),
                                                         Helpers.CreateAddFact(time_stop),
                                                         Helpers.CreateAddAbilityResource(time_stop.GetComponent<AbilityResourceLogic>().RequiredResource)
                                                         );

            time_mystery = createMystery("TimeOracleMystery", "Time", time_stop.Icon, final_revelation,
                                         new StatType[] { StatType.SkillMobility, StatType.SkillPerception, StatType.SkillUseMagicDevice },
                                         spells,
                                         true, false,
                                         aging_touch, rewind_time, speed_or_slow_time, temporal_celerity,
                                         time_flicker, time_hop, time_sight, erase_from_time
                                         );
        }


        static void createAncestorsMystery()
        {
            var spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00"), //true strike
                SpiritualWeapons.spiritual_weapon,
                library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63"), //heroism
                SpiritualWeapons.spiritual_ally,
                library.Get<BlueprintAbility>("90810e5cf53bf854293cbd5ea1066252"), //righteous might
                library.Get<BlueprintAbility>("e15e5e7045fda2244b98c8f010adfe31"), //heroism greater
                library.Get<BlueprintAbility>("98310a099009bbd4dbdf66bcef58b4cd"), //mass invisibility
                library.Get<BlueprintAbility>("0e67fa8f011662c43934d486acc50253"), //prediction of failure
                library.Get<BlueprintAbility>("43740dab07286fe4aa00a6ee104ce7c1"), //heroic invocation
            };


            var blood_of_heroes = mystery_engine.createBloodOfHeroes("BloodOfHeroesOracleRevelation",
                                                         "Blood of Heroes",
                                                         "As a move action, you can call upon your ancestors to grant you extra bravery in battle. You gain a +1 morale bonus on attack rolls, damage rolls, and Will saves against fear for a number of rounds equal to your Charisma bonus. At 7th level, this bonus increases to +2, and at 14th level this bonus increases to +3. You can use this ability once per day, plus one additional time per day at 5th level, and every five levels thereafter.");
            var phantom_touch = mystery_engine.createPhantomTouch("PhantomTouchOracleRevelation",
                                                                  "Phantom Touch",
                                                                  "As a standard action, you can perform a melee touch attack that causes a living creature to become shaken. This ability lasts for a number of rounds equal to 1/2 your oracle level (minimum 1 round). You can use this ability a number of times per day equal to 3 + your Charisma modifier.");
            var sacred_council = mystery_engine.createSacredCouncil("SacredCouncilOracleRevelation",
                                                                    "Sacred Council",
                                                                     "As a move action, you can call upon your ancestors to provide council. This advice grants you a +2 bonus on any one kind of d20 rolls. This effect lasts for 1 round. You can use this ability a number of times per day equal to your Charisma bonus.");
            var spirit_of_the_warrior = mystery_engine.createSpiritOfTheWarrior("SpiritOfTheWarriorOracleRevelation",
                                                                                "Spirit of the Warrior",
                                                                                "You can summon the spirit of a great warrior ancestor and allow it to possess you, becoming a mighty warrior yourself. You gain a +4 enhancement bonus to Strength, Dexterity, and Constitution, and a +4 natural armor bonus to AC. Your base attack bonus while possessed equals your oracle level (which may give you additional attacks), all weapons you are holding receive keen enchantment. You can use this ability for 1 round for every 2 oracle levels you possess. This duration does not need to be consecutive, but it must be spent in 1-round increments. You must be at least 11th level to select this revelation.");
           
            var spirit_shield = mystery_engine.createSpiritShield("SpiritShieldOracleRevelation",
                                                                  "Spirit Shield",
                                                                  "You can call upon the spirits of your ancestors to form a shield around you that blocks incoming attacks and grants you a +4 armor bonus. At 7th level, and every four levels thereafter, this bonus increases by +2. At 13th level, this shield causes arrows, rays, and other ranged attacks requiring an attack roll against you to have a 50% miss chance. You can use this shield for 1 hour per day per oracle level. This duration does not need to be consecutive, but it must be spent in 1-hour increments.");
            var storm_of_souls = mystery_engine.createStormOfSouls("StormOfSoulsOracleRevelation",
                                                                   "Storm of Souls",
                                                                   $"You can summon the spirits of your ancestors to attack in a ghostly barrage—their fury creates physical wounds on creatures in the area. The storm has a range of 100 feet and is a 20-foot-radius burst. Objects and creatures in the area take 1d{BalanceFixes.getDamageDieString(DiceType.D8)} hit points of damage for every two oracle levels you possess. Undead creatures in the area take 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage for every oracle level you possess. A successful Fortitude save reduces the damage to half. You must be at least 7th level to select this revelation. You can use this ability once per day, plus one additional time per day at 11th level and every four levels thereafter.");
            var spirit_walk = mystery_engine.createSpiritWalk("SpiritWalkOracleRevelation",
                                                                   "Spirit Walk",
                                                                   "You can become incorporeal and invisible. You can take no action other than to move while in this form. You remain in this form for a number of rounds equal to twice your oracle level, but these rounds need not be consecutive. You must be at least 11th level to select this revelation.");           
            var ancestral_weapon = mystery_engine.createAncestralWeapon("AncestralWeaponOracleRevelation",
                                                                     "Ancestral Weapon",
                                                                     "The weapon you hold gains ghost touch weapon property. You can use this ability for a number of minutes per day equal to your oracle level. This duration does not need to be consecutive, but it must be used in 1-minute increments. The weapon loses ghost touch property if it leaves your grasp.");

            var heroic_invocation = Common.convertToSpellLike(library.Get<BlueprintAbility>("43740dab07286fe4aa00a6ee104ce7c1"), "OracleAncestorMystery", getOracleArray(), StatType.Charisma);
            var final_revelation = Helpers.CreateFeature("FinalRevelationAncestorMystery",
                                                         "Final Revelation",
                                                         "Upon reaching 20th level, you become one with the spirits of your ancestors. You gain a bonus on Will saving throws equal to your Charisma modifier, blindsense out to a range of 60 feet, and a +4 bonus on your caster level for all divination spells. You can cast heroic invocation as a spell-like ability once per day.",
                                                         "",
                                                         null,
                                                         FeatureGroup.None,
                                                         Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.UntypedStackable),
                                                         Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma),
                                                         Helpers.Create<IncreaseSpellSchoolCasterLevel>(i => { i.School = SpellSchool.Divination; i.BonusLevel = 4; }),
                                                         Helpers.CreateAddFact(heroic_invocation),
                                                         Helpers.CreateAddAbilityResource(heroic_invocation.GetComponent<AbilityResourceLogic>().RequiredResource),
                                                         Common.createBlindsense(60)
                                                         );
            ancestor_mystery = createMystery("AncestorOracleMystery", "Ancestor", library.Get<BlueprintAbility>("6717dbaef00c0eb4897a1c908a75dfe5").Icon, final_revelation,
                             new StatType[] { StatType.SkillLoreNature },
                             spells,
                             true, true,
                             blood_of_heroes, phantom_touch, sacred_council, spirit_of_the_warrior,
                             spirit_shield , storm_of_souls, spirit_walk, ancestral_weapon
                             );
        }


        static BlueprintProgression createMystery(string name, string display_name, UnityEngine.Sprite icon, 
                                                  BlueprintFeature final_revelation, StatType[] class_skills, BlueprintAbility[] spells, 
                                                  bool is_ravener,
                                                  bool is_hermit,
                                                  params BlueprintFeature[] revelations)
        {
            string description = $"An oracle with the {display_name.ToLower()} mystery adds ";

            for (int i = 0; i < class_skills.Length; i++)
            {
                description += LocalizedTexts.Instance.Stats.GetText(class_skills[i]);
                if (i + 2 < class_skills.Length)
                {
                    description += ", ";
                }
                else if (i + 1 < class_skills.Length)
                {
                    description += " and ";
                }       
            }

            description += " to her list of class skills.";

            var mystery = Helpers.CreateProgression(name + "Progression",
                                                    display_name,
                                                    description,
                                                    "",
                                                    icon,
                                                    FeatureGroup.Domain);

            var skills_feature = Helpers.CreateFeature(name + "SkillsFeature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.Domain);
            skills_feature.HideInCharacterSheetAndLevelUp = true;
            foreach (var s in class_skills)
            {
                skills_feature.AddComponent(Helpers.Create<AddClassSkill>(a => a.Skill = s));
            }

            mystery.AddComponent(Common.createAddFeatureIfHasFact(mystery_skills, skills_feature));


            mystery.Classes = getOracleArray();
            mystery.LevelEntries = new LevelEntry[spells.Length + 1];
            mystery.UIGroups = Helpers.CreateUIGroups();
            for (int i = 0; i < spells.Length; i++)
            {
                var feat = Helpers.CreateFeature(name + spells[i].name,
                                                 spells[i].Name,
                                                 "At 2nd level, and every two levels thereafter, an oracle learns an additional spell derived from her mystery.\n"
                                                 + spells[i].Name + ": " + spells[i].Description,
                                                 "",
                                                 spells[i].Icon,
                                                 FeatureGroup.None,
                                                 spells[i].CreateAddKnownSpell(oracle_class, i + 1)
                                                 );
                mystery.LevelEntries[i] = Helpers.LevelEntry(2 * (i + 1), feat);
                mystery.UIGroups[0].Features.Add(feat);
            }

            mystery.LevelEntries[spells.Length] = Helpers.LevelEntry(20, final_revelation);
            mystery.UIGroups[0].Features.Add(final_revelation);

            var mystery_revelation_selection = Helpers.CreateFeatureSelection(name + "RevelationSelection",
                                                                              display_name,
                                                                              revelation_selection.Description,
                                                                              "",
                                                                              icon,
                                                                              FeatureGroup.None,
                                                                              Helpers.PrerequisiteFeature(mystery, any: true));

            mystery_revelation_selection.AllFeatures = revelations;

            revelation_selection.AllFeatures = revelation_selection.AllFeatures.AddToArray(mystery_revelation_selection);

            mystery_spells_map.Add(mystery, spells);

            if (is_ravener)
            {
                var ravener_mystery = Helpers.CreateFeature(name + "RavenerHunterProgression",
                                                                display_name,
                                                                Archetypes.RavenerHunter.charged_by_nature.Description,
                                                                "",
                                                                icon,
                                                                FeatureGroup.Domain);
                mystery_revelation_selection.AddComponent(Helpers.PrerequisiteFeature(ravener_mystery, any: true));
                oracle_ravener_hunter_mysteries_map.Add(mystery, ravener_mystery);
                Archetypes.RavenerHunter.charged_by_nature.AllFeatures = Archetypes.RavenerHunter.charged_by_nature.AllFeatures.AddToArray(ravener_mystery);
                Archetypes.RavenerHunter.revelation_selection.AllFeatures = Archetypes.RavenerHunter.revelation_selection.AllFeatures.AddToArray(mystery_revelation_selection);
            }

            var dual_cursed_mystery = replaceSpellsForMystery(mystery, "DualCursed", "DualCuresedBonusSpell", dual_cursed_bonus_spells, dual_cursed_bonus_spell_features);
            dual_cursed_oracle_mysteries.AllFeatures = dual_cursed_oracle_mysteries.AllFeatures.AddToArray(dual_cursed_mystery);

            if (is_hermit)
            {
                var hermit_mystery = replaceSpellsForMystery(mystery, "Hermit", "HermitBonusSpell", hermit_bonus_spells, hermit_bonus_spell_features);
                hermit_mysteries.AllFeatures = hermit_mysteries.AllFeatures.AddToArray(hermit_mystery);
            }


            return mystery;
        }


        static BlueprintProgression replaceSpellsForMystery(BlueprintProgression mystery, string prefix, string spell_prefix, Dictionary<int, BlueprintAbility> bonus_spells, Dictionary<int, BlueprintFeature> spell_features)
        {
            var new_mystery = library.CopyAndAdd(mystery, prefix + mystery.name, "");
            new_mystery.LevelEntries = new LevelEntry[mystery.LevelEntries.Length];

            for (int i = 0; i < mystery.LevelEntries.Length; i++)
            {
                var sl = mystery.LevelEntries[i].Level / 2;
                if (bonus_spells.ContainsKey(sl))
                {
                    if (!spell_features.ContainsKey(sl))
                    {
                        var spell = bonus_spells[sl];
                        spell_features[sl] = Helpers.CreateFeature(spell_prefix + spell.name,
                                                                                             spell.Name,
                                                                                             "At 2nd level, and every two levels thereafter, an oracle learns an additional spell derived from her mystery.\n"
                                                                                             + spell.Name + ": " + spell.Description,
                                                                                             "",
                                                                                             spell.Icon,
                                                                                             FeatureGroup.None,
                                                                                             spell.CreateAddKnownSpell(oracle_class, sl)
                                                                                             );
                    }

                    new_mystery.UIGroups[0].Features.Add(spell_features[sl]);
                    new_mystery.LevelEntries[i] = Helpers.LevelEntry(mystery.LevelEntries[i].Level, spell_features[sl]);
                }
                else
                {
                    new_mystery.LevelEntries[i] = mystery.LevelEntries[i];
                }
            }
            mystery.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = new_mystery));
            return new_mystery;
        }


        static void createCurses()
        {
            createCloudedVision();
            createBlackened();
            createDeaf();
            createLame();
            createWasting();
            createPranked();
            createPlagued();
            createWolfScarredFace();
            createLich();
            createVampirism();
            createPowerless();
            createReclusive();

            vampirism_minor.AddComponent(Helpers.PrerequisiteNoFeature(lich));
            lich_minor.AddComponent(Helpers.PrerequisiteNoFeature(vampirism));
            oracle_curses = Helpers.CreateFeatureSelection("OracleCurseSelection",
                                                           "Oracle's Curse",
                                                           "Each oracle is cursed, but this curse comes with a benefit as well as a hindrance. This choice is made at 1st level, and once made, it cannot be changed. The oracle’s curse cannot be removed or dispelled without the aid of a deity. An oracle’s curse is based on her oracle level.",
                                                           "",
                                                           null,
                                                           FeatureGroup.Domain,
                                                           Helpers.Create<NoSelectionIfAlreadyHasFeature>(n => { n.AnyFeatureFromSelection = true; n.Features = new BlueprintFeature[0]; })
                                                           );

            oracle_curses.AllFeatures = new BlueprintFeature[] { clouded_vision, blackened, deaf, lame, wasting, pranked, plagued, wolf_scarred_face, lich, vampirism, powerless, reclusive };

            minor_curse_selection = Helpers.CreateFeatureSelection("OracleMinorCurseSelection",
                                               "Second Curse",
                                               "A dual-cursed oracle must choose a second curse at 1st level. This curse never changes its abilities as the oracle gains levels.",
                                               "",
                                               null,
                                               FeatureGroup.Domain);

            minor_curse_selection.AllFeatures = new BlueprintFeature[] { clouded_vision_minor, blackened_minor, deaf_minor, lame_minor, wasting_minor, pranked_minor, plagued_minor, wolf_scarred_face_minor, lich_minor, vampirism_minor, powerless_minor };
        }


        static void createReclusive()
        {
            var curse = Helpers.CreateFeature("OracleCurseReclusive",
                                              "Reclusive",
                                              "You must attempt saving throws to resist all spells cast by anyone other than yourself, even those cast by allies. Spells you cast only on yourself affect you as though your caster level were 1 higher.",
                                              "",
                                              NewSpells.barrow_haze.Icon,
                                              FeatureGroup.None,
                                              Helpers.Create<HarmlessSaves.SaveAgainstHarmlessSpells>(),
                                              Helpers.Create<NewMechanics.MetamagicMechanics.ApplyMetamagicToPersonalSpell>(a => a.caster_level_increase = 1)
                                              );

            reclusive_minor = library.CopyAndAdd(curse, "OracleCurseReclusiveMinor", "");

            var hindrance = library.CopyAndAdd(powerless_minor, "OracleCurseReclusiveHindranceFeature", "");
            hindrance.RemoveComponents<NewMechanics.MetamagicMechanics.ApplyMetamagicToPersonalSpell>();
            curse_to_hindrance_map.Add(curse, hindrance);
            curse_to_minor_map.Add(curse, reclusive_minor);

            var curse5 = Helpers.CreateFeature("OracleCurse5Reclusive",
                                               "Reclusive Prophecy",
                                               "At 5th level, any spells you cast only on yourself affect you as if they were modified by the Extend Spell feat. This does not increase their level or casting time.",
                                               "",
                                               Helpers.GetIcon("f180e72e4a9cbaa4da8be9bc958132ef"), //extend spell
                                               FeatureGroup.None,
                                               Helpers.Create<NewMechanics.MetamagicMechanics.ApplyMetamagicToPersonalSpell>(a => a.metamagic = Metamagic.Extend)
                                               );

            var curse10 = Helpers.CreateFeature("OracleCurse10Reclusive",
                                                "Reclusive Prophecy",
                                                "At 10th level, you are immune to charm spells and spell-like abilities.",
                                                "",
                                                Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"),
                                                FeatureGroup.None,
                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Charm),
                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Charm)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Reclusive",
                                                "Reclusive Prophecy",
                                                "At 15th level, you gain spell resistance equal to 10 + your oracle level.",
                                                "",
                                                Helpers.GetIcon("0a5ddfbcfb3989543ac7c936fc256889"), //spell resistance
                                                FeatureGroup.None,
                                                Helpers.Create<AddSpellResistance>(s => s.Value = Helpers.CreateContextValue(AbilityRankType.Default)),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.BonusValue,
                                                                                classes: getOracleArray(), stepLevel: 10)                              
                                                );

            reclusive = createOracleCurseProgression("OracleReclusiveCurseProgression", "Reclusive Prophecy",
                                                    "You are reclusive and paranoid to the point that your allies cannot easily help you in times of stress or unease.",
                                                    curse, curse5, curse10, curse15);

            reclusive_minor.AddComponent(Helpers.PrerequisiteNoFeature(reclusive));
        }


        static void createPowerless()
        {
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var apply_staggered = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1), dispellable: false);

            var uncanny_dodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            var improved_uncanny_dodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            var vampiric_shadow_shield = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");
            var curse = Helpers.CreateFeature("OracleCursePowerless",
                                              "Powerless Prophecy",
                                              "You gain uncanny dodge, as the rogue class feature. However, you are always staggered for the entire first round of combat.",
                                              "",
                                              staggered.Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddFact(uncanny_dodge),
                                              Helpers.Create<NewMechanics.RunActionOnCombatStart>(r => r.actions = Helpers.CreateActionList(apply_staggered))
                                              );

            powerless_minor = library.CopyAndAdd(curse, "OracleCursePowerlessMinor", "");

            var hindrance = library.CopyAndAdd(powerless_minor, "OracleCursePowerlessHindranceFeature", "");
            hindrance.RemoveComponents<AddFacts>();
            curse_to_hindrance_map.Add(curse, hindrance);
            curse_to_minor_map.Add(curse, vampirism_minor);

            var curse5 = Helpers.CreateFeature("OracleCurse5Powerless",
                                               "Powerless Prophecy",
                                               "At 5th level, you gain a +4 insight bonus on initiative checks.",
                                               "",
                                               Helpers.GetIcon("797f25d709f559546b29e7bcb181cc74"), //improved initiative
                                               FeatureGroup.None,
                                               Helpers.CreateAddStatBonus(StatType.Initiative, 4, ModifierDescriptor.Insight)
                                               );

            var curse10 = Helpers.CreateFeature("OracleCurse10Powerless",
                                                "Powerless Prophecy",
                                                "At 10th level, you gain improved uncanny dodge as the rogue ability, using your oracle level as your rogue level.",
                                                "",
                                                improved_uncanny_dodge.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFact(improved_uncanny_dodge)
                                                );

            var buff15 = Helpers.CreateBuff("OracleCurse15VPowerlessBuff",
                                            "Powerless Prophecy",
                                            "At 15th level, you gain a +4 insight bonus on all your saving throws and to your AC during surprise round.",
                                            "",
                                            Helpers.GetIcon("1f01a098d737ec6419aedc4e7ad61fdd"), //foresight
                                            null,
                                            Helpers.CreateAddStatBonus(StatType.AC, 4, ModifierDescriptor.Insight),
                                            Helpers.CreateAddStatBonus(StatType.SaveWill, 4, ModifierDescriptor.Insight),
                                            Helpers.CreateAddStatBonus(StatType.SaveReflex, 4, ModifierDescriptor.Insight),
                                            Helpers.CreateAddStatBonus(StatType.SaveFortitude, 4, ModifierDescriptor.Insight)
                                            );
            buff15.Stacking = StackingType.Ignore;
            var curse15 = Helpers.CreateFeature("OracleCurse15VPowerless",
                                                buff15.Name,
                                                buff15.Description,
                                                "",
                                                buff15.Icon,
                                                FeatureGroup.None,
                                                Helpers.Create<InitiativeMechanics.ActionInSurpriseRound>(a => a.actions = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff15, Helpers.CreateContextDuration(1), dispellable:false)))
                                                );

            powerless = createOracleCurseProgression("OraclePowerlessCurseProgression", "Powerless Prophecy",
                                                    "You are forewarned of danger but can’t act to prevent it.",
                                                    curse, curse5, curse10, curse15);

            powerless_minor.AddComponent(Helpers.PrerequisiteNoFeature(powerless));
        }


        static void createVampirism()
        {
            var vampiric_touch = library.Get<BlueprintAbility>("8a28a811ca5d20d49a863e832c31cce1");
            var vampiric_shadow_shield = library.Get<BlueprintAbility>("a34921035f2a6714e9be5ca76c5e34b5");
            var curse = Helpers.CreateFeature("OracleCurseVampirism",
                                              "Vampirism",
                                              "You take damage from positive energy and heal from negative energy as if you were undead.",
                                              "",
                                              NewSpells.savage_maw.Icon,
                                              FeatureGroup.None,
                                              Helpers.Create<UndeadMechanics.ConsiderUndeadForHealing>(),
                                              Helpers.Create<AddEnergyImmunity>(a => a.Type = DamageEnergyType.NegativeEnergy)
                                              );

            vampirism_minor = library.CopyAndAdd(curse, "OracleCurseVampirismMinor", "");
            var hindrance = library.CopyAndAdd(vampirism_minor, "OracleCurseVampirismHindranceFeature", "");
            curse_to_hindrance_map.Add(curse, hindrance);
            curse_to_minor_map.Add(curse, vampirism_minor);

            var curse5 = Helpers.CreateFeature("OracleCurse5Vampirism",
                                               "Channel Resistance",
                                               "At 5th level, you gain channel resistance +4.",
                                               "",
                                               Helpers.GetIcon("89df18039ef22174b81052e2e419c728"),
                                               FeatureGroup.None,
                                               Helpers.Create<SavingThrowBonusAgainstSpecificSpells>(c =>
                                                                                                        {
                                                                                                            c.Spells = new BlueprintAbility[0];
                                                                                                            c.Value = 4;
                                                                                                            c.BypassFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("3d8e38c9ed54931469281ab0cec506e9") }; //sun domain
                                                                                                        }
                                                                                                    )
                                               );
            ChannelEnergyEngine.addChannelResitance(curse5);

            var curse10 = Helpers.CreateFeature("OracleCurse10Vampirism",
                                                "Vampirism",
                                                "At 10th level, you add vampiric touch to your list of 3rd-level oracle spells known and undead anatomy II to your list of 5th-level oracle spells known",
                                                "",
                                                vampiric_touch.Icon,
                                                FeatureGroup.None,
                                                vampiric_touch.CreateAddKnownSpell(oracle_class, 3),
                                                Wildshape.undead_anatomyII.CreateAddKnownSpell(oracle_class, 5)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Vampirism",
                                                "Damage Reduction",
                                                "At 15th level, you gain damage reduction 5/magic.",
                                                "",
                                                vampiric_shadow_shield.Icon,
                                                FeatureGroup.None,
                                                Common.createMagicDR(5)
                                                );

            vampirism = createOracleCurseProgression("OracleVampirismCurseProgression", "Vampirism",
                                                    "You crave the taste of fresh, warm blood.",
                                                    curse, curse5, curse10, curse15);

            vampirism_minor.AddComponent(Helpers.PrerequisiteNoFeature(vampirism));
        }

        static void createLich()
        {
            var false_life = library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762");
            var banshee_blast = library.Get<BlueprintAbility>("d42c6d3f29e07b6409d670792d72bc82");
            var curse = Helpers.CreateFeature("OracleCurseLich",
                                              "Lich",
                                              "You have (unknowingly) fulfilled most (but not all) of the ritualistic components to achieve lichdom. You have yet to turn into an undead creature, but you are close. You take damage from positive energy and heal from negative energy as if you were undead.",
                                              "",
                                              Helpers.GetIcon("a1a8bf61cadaa4143b2d4966f2d1142e"), // undead bloodline
                                              FeatureGroup.None,
                                              Helpers.Create<UndeadMechanics.ConsiderUndeadForHealing>(),
                                              Helpers.Create<AddEnergyImmunity>(a => a.Type = DamageEnergyType.NegativeEnergy)
                                              );

            lich_minor = library.CopyAndAdd(curse, "OracleCurseLichMinor", "");
            var hindrance = library.CopyAndAdd(lich_minor, "OracleCurseLichHindranceFeature", "");
            curse_to_hindrance_map.Add(curse, hindrance);
            curse_to_minor_map.Add(curse, lich_minor);

            var curse5 = Helpers.CreateFeature("OracleCurse5Lich",
                                               "Ghoul Touch",
                                               "At 5th level, you add ghoul touch to your list of 2nd-level oracle spells known.",
                                               "",
                                               NewSpells.ghoul_touch.Icon,
                                               FeatureGroup.None,
                                               NewSpells.ghoul_touch.CreateAddKnownSpell(oracle_class, 2));


            var curse10 = Helpers.CreateFeature("OracleCurse10Lich",
                                                "Lich",
                                                "At 10th level, add undead anatomy I to your list of 3rd-level oracle spells known and undead anatomy II to your list of 5th-level oracle spells known",
                                                "",
                                                banshee_blast.Icon,
                                                FeatureGroup.None,
                                                Wildshape.undead_anatomyI.CreateAddKnownSpell(oracle_class, 3),
                                                Wildshape.undead_anatomyII.CreateAddKnownSpell(oracle_class, 5)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Lich",
                                                "Immunity To Death Effects",
                                                "At 15th level, you are immune to death effects.",
                                                "",
                                                Helpers.GetIcon("0413915f355a38146bc6ad40cdf27b3f"), // death ward
                                                FeatureGroup.None,
                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Death),
                                                SpellDescriptor.Death.CreateBuffImmunity());

            lich = createOracleCurseProgression("OracleLichCurseProgression", "Lich",
                                                    "Every living spellcaster hides a secret in their flesh—a unique, personalized set of conditions that, when all are fulfilled in the correct order, can trigger the transformation into a lich. Normally, one must expend years and tens of thousands of gold pieces to research this deeply personalized method of attaining immortality. Yet, in a rare few cases, chance and ill fortune can conspire against an unsuspecting spellcaster.",
                                                    curse, curse5, curse10, curse15);

            lich_minor.AddComponent(Helpers.PrerequisiteNoFeature(lich));
        }


        static void createWolfScarredFace()
        {
            var magic_fang = library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33");
            var magic_fang_greater = library.Get<BlueprintAbility>("f1100650705a69c4384d3edd88ba0f52");

            var bite1d4 = Helpers.CreateFeature("OracleWolfScarred1d4BiteFeature",
                                    "",
                                    "",
                                    "",
                                    null,
                                    FeatureGroup.None,
                                    Common.createAddAdditionalLimb(library.Get<BlueprintItemWeapon>("35dfad6517f401145af54111be04d6cf"))
                                    );
            bite1d4.HideInUI = true;

            var bite1d6 = Helpers.CreateFeature("OracleWolfScarred1d6BiteFeature",
                                                "",
                                                "",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Common.createAddAdditionalLimb(library.Get<BlueprintItemWeapon>("a000716f88c969c499a535dadcf09286"))
                                                );
            bite1d6.HideInUI = true;
            var bite1d8 = Helpers.CreateFeature("OracleWolfScarred1d8BiteFeature",
                                    "",
                                    "",
                                    "",
                                    null,
                                    FeatureGroup.None,
                                    Common.createAddAdditionalLimb(library.Get<BlueprintItemWeapon>("61bc14eca5f8c1040900215000cfc218"))
                                    );
            bite1d8.HideInUI = true;

            var curse = Helpers.CreateFeature("OracleCurseWolfScarredFace",
                                              "Wolf-scarred Face",
                                              "You have a severe speech impediment, and any spells you cast have a 20% chance of failing, wasting your action but not expending the spell. You gain a natural bite attack that deals 1d4 points of damage if you are a Medium creature or 1d3 points of damage if you are Small.",
                                              "",
                                              Helpers.GetIcon("de7a025d48ad5da4991e7d3c682cf69d"), // cats grace
                                              FeatureGroup.None,
                                              Helpers.Create<SpellFailureMechanics.SpellFailureChance>(s => { s.chance = 20; s.do_not_spend_slot_if_failed = true; }),
                                              Helpers.CreateAddFeatureOnClassLevel(bite1d4, 5, getOracleArray(), before: true)
                                              );

            wolf_scarred_face_minor = Helpers.CreateFeature("OracleCurseWolfScarredFaceMinorFeature",
                                              "Wolf-scarred Face",
                                              "You have a severe speech impediment, and any spells you cast have a 20% chance of failing, wasting your action but not expending the spell. You gain a natural bite attack that deals 1d4 points of damage if you are a Medium creature or 1d3 points of damage if you are Small.",
                                              "",
                                              Helpers.GetIcon("de7a025d48ad5da4991e7d3c682cf69d"), // cats grace
                                              FeatureGroup.None,
                                              Helpers.Create<SpellFailureMechanics.SpellFailureChance>(s => s.chance = 20),
                                              Common.createAddAdditionalLimb(library.Get<BlueprintItemWeapon>("35dfad6517f401145af54111be04d6cf"))
                                              );

            var hindrance = library.CopyAndAdd(wolf_scarred_face_minor, "OracleCurseWolfScarredFaceHindranceFeature", "");
            hindrance.RemoveComponents<AddAdditionalLimb>();
            curse_to_hindrance_map.Add(curse, hindrance);
            curse_to_minor_map.Add(curse, wolf_scarred_face_minor);

            var curse5 = Helpers.CreateFeature("OracleCurse5WolfScarredFace",
                                               "Wolf-scarred Face",
                                               "At 5th level, you add magic fang to your list of known spells and your bite damage increases to 1d6.",
                                               "",
                                               magic_fang.Icon,
                                               FeatureGroup.None,
                                               magic_fang.CreateAddKnownSpell(oracle_class, 1),
                                               Helpers.CreateAddFeatureOnClassLevel(bite1d6, 10, getOracleArray(), before: true)
                                               );

            var curse10 = Helpers.CreateFeature("OracleCurse10WolfScarredFace",
                                                "Wolf-scarred Face",
                                                "At 10th level, the damage dealt by your bite attack increases to 1d8.",
                                                "",
                                                Helpers.GetIcon("75de4ded3e731dc4f84d978fe947dc67"), // acid maw
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(bite1d8, 15, getOracleArray(), before: true)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15WolfScarredFace",
                                                "Wolf-scarred Face",
                                                "At 15th level, you add greater magic fang to your list of known spells and the damage dealt by your bite attack increases to 2d6.",
                                                "",
                                                magic_fang_greater.Icon,
                                                FeatureGroup.None,
                                                magic_fang_greater.CreateAddKnownSpell(oracle_class, 3),
                                                Common.createAddAdditionalLimb(library.Get<BlueprintItemWeapon>("2abc1dc6172759c42971bd04b8c115cb")) //bite 2d6
                                                );

            wolf_scarred_face = createOracleCurseProgression("OracleWolfScarredFaceCurseProgression", "Wolf-scarred Face",
                                                            "Your face is deformed, as though you were born with a wolf’s muzzle instead of an ordinary nose and jaw. Many mistake you for a werewolf, and in areas plagued by lycanthropes, you must take pains to hide your face.",
                                                            curse, curse5, curse10, curse15);

            wolf_scarred_face_minor.AddComponent(Helpers.PrerequisiteNoFeature(wolf_scarred_face));
        }

        static void createPlagued()
        {
            var pox_pustules = library.Get<BlueprintAbility>("bc153808ef4884a4594bc9bec2299b69");
            var curse = Helpers.CreateFeature("OracleCursePlagued",
                                  "Plagued",
                                  "You take a –1 penalty on all saving throws against disease effects, but you are immune to the sickened condition.",
                                  "",
                                  Helpers.GetIcon("4e42460798665fd4cb9173ffa7ada323"), // sickened
                                  FeatureGroup.None,
                                  Helpers.Create<SavingThrowBonusAgainstDescriptor>(s => { s.Bonus = -1; s.ModifierDescriptor = ModifierDescriptor.UntypedStackable; s.SpellDescriptor = SpellDescriptor.Disease; })
                                  );
            plagued_minor = library.CopyAndAdd(curse, "OracleCursePlaguedMinorFeature", "");
            var hindrance = library.CopyAndAdd(plagued_minor, "OracleCursePlaguedHindranceFeature", "");
            curse_to_hindrance_map.Add(curse, hindrance);
            curse_to_minor_map.Add(curse, plagued_minor);

            var curse5 = Helpers.CreateFeature("OracleCurse5Plagued",
                                               "Pox Pustules",
                                               "At 5th level, you add pox pustules to your list of 2nd-level oracle spells known.",
                                               "",
                                               pox_pustules.Icon,
                                               FeatureGroup.None,
                                               pox_pustules.CreateAddKnownSpell(oracle_class, 2));

            var curse10 = Helpers.CreateFeature("OracleCurse10Plagued",
                                                "Plagued",
                                                "At 10th level, increase the save DC of any disease effect you create by +2.",
                                                "",
                                                Helpers.GetIcon("82a5b848c05e3f342b893dedb1f9b446"), // plague storm
                                                FeatureGroup.None,
                                                Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellsDC>(i => { i.Value = 2; i.Descriptor = SpellDescriptor.Disease; })
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Plagued",
                                                "Immune to Disease",
                                                "At 15th level, you are immune to the effects of disease.",
                                                "",
                                                Helpers.GetIcon("4093d5a0eb5cae94e909eb1e0e1a6b36"), // remove disease
                                                FeatureGroup.None,
                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Disease),
                                                SpellDescriptor.Disease.CreateBuffImmunity());

            plagued = createOracleCurseProgression("OraclePlaguedCurseProgression", "Plagued",
                                                    "You suffer from minor ailments and sicknesses. While you struggle to resist new diseases, you have grown accustomed to the many inconveniences of sickness.",
                                                    curse, curse5, curse10, curse15);

            plagued_minor.AddComponent(Helpers.PrerequisiteNoFeature(plagued));
        }

        static void createPranked()
        {
            var faerie_fire = library.Get<BlueprintAbility>("4d9bf81b7939b304185d58a09960f589");
            var vanish = library.Get<BlueprintAbility>("f001c73999fb5a543a199f890108d936");
            var glitterdust = library.Get<BlueprintAbility>("ce7dad2b25acf85429b6c9550787b2d9");
            var hideous_laughter = library.Get<BlueprintAbility>("fd4d9fd7f87575d47aafe2a64a6e2d8d");
            var confusion = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949");
            var mass_invisibility = library.Get<BlueprintAbility>("98310a099009bbd4dbdf66bcef58b4cd");
            var curse = Helpers.CreateFeature("OracleCursePranked",
                                              "Pranked",
                                              "You take a –4 penalty on initiative checks. Furthermore, whenever you attempt to cast a spell using an item, there’s a 25% chance that you fail. You add faerie fire and vanish to your list of spells known.",
                                              "",
                                              hideous_laughter.Icon,
                                              FeatureGroup.None,
                                              Helpers.CreateAddStatBonus(StatType.Initiative, -4, ModifierDescriptor.Penalty),
                                              Helpers.Create<SpellFailureMechanics.ItemUseFailure>(s => s.chance = 25),
                                              faerie_fire.CreateAddKnownSpell(oracle_class, 1),
                                              vanish.CreateAddKnownSpell(oracle_class, 1)
                                              );
            pranked_minor = library.CopyAndAdd(curse, "OracleCursePrankedMinorFeature", "");
            var hindrance = library.CopyAndAdd(pranked_minor, "OracleCursePrankedHindranceFeature", "");
            hindrance.RemoveComponents<AddKnownSpell>();
            curse_to_hindrance_map.Add(curse, hindrance);
            curse_to_minor_map.Add(curse, pranked_minor);

            var curse5 = Helpers.CreateFeature("OracleCurse5Pranked",
                                               "Pranked",
                                               "At 5th level, you add glitterdust and hideous laughter to your list of spells known.",
                                               "",
                                               glitterdust.Icon,
                                               FeatureGroup.None,
                                               glitterdust.CreateAddKnownSpell(oracle_class, 2),
                                               hideous_laughter.CreateAddKnownSpell(oracle_class, 2)
                                               );

            var curse10 = Helpers.CreateFeature("OracleCurse10Pranked",
                                                "Pranked",
                                                "At 10th level, you add confusion to your list of spells known as a 5th-level spell.",
                                                "",
                                                confusion.Icon,
                                                FeatureGroup.None,
                                                confusion.CreateAddKnownSpell(oracle_class, 5));

            var curse15 = Helpers.CreateFeature("OracleCurse15Pranked",
                                                "Pranked",
                                                "At 15th level, you add invisibility, mass to your list of spells known.",
                                                "",
                                                mass_invisibility.Icon,
                                                FeatureGroup.None,
                                                mass_invisibility.CreateAddKnownSpell(oracle_class, 7));

            pranked = createOracleCurseProgression("OraclePrankedCurseProgression", "Pranked",
                                                    "Capricious fey constantly bedevil you, playing pranks on you such as tying your shoelaces together, hiding your gear, making inappropriate noises or smells at formal events, and mimicking your voice to tell embarrassing lies.",
                                                    curse, curse5, curse10, curse15);

            pranked_minor.AddComponent(Helpers.PrerequisiteNoFeature(pranked));
        }


        static void createWasting()
        {
            var curse = Helpers.CreateFeature("OracleCurseWasting", 
                                              "Wasting",
                                              "You take a –4 penalty on Charisma-based skill checks, except for Intimidate. You gain a +4 competence bonus on saves made against disease.",
                                              "",
                                              NewSpells.fleshworm_infestation.Icon, // sickened
                                              FeatureGroup.None,
                                              Helpers.CreateAddStatBonus(StatType.CheckDiplomacy, -4, ModifierDescriptor.Penalty),
                                              Helpers.CreateAddStatBonus(StatType.CheckBluff, -4, ModifierDescriptor.Penalty),
                                              Helpers.CreateAddStatBonus(StatType.SkillUseMagicDevice, -4, ModifierDescriptor.Penalty),
                                              Helpers.Create<SavingThrowBonusAgainstDescriptor>(s =>
                                                                                                {
                                                                                                    s.Value = 4;
                                                                                                    s.SpellDescriptor = SpellDescriptor.Disease;
                                                                                                    s.ModifierDescriptor = ModifierDescriptor.Competence;
                                                                                                }
                                                                                                )
                                              );
            wasting_minor = library.CopyAndAdd(curse, "OracleCurseWastingMinorFeature", "");
            var hindrance = library.CopyAndAdd(wasting_minor, "OracleCurseWastingHindranceFeature", "");
            hindrance.RemoveComponents<SavingThrowBonusAgainstDescriptor>();
            curse_to_hindrance_map.Add(curse, hindrance);
            curse_to_minor_map.Add(curse, wasting_minor);

            var curse5 = Helpers.CreateFeature("OracleCurse5Wasting", 
                                               "Immune to Sickened",
                                               "At 5th level, you are immune to the sickened condition (but not nauseated).",
                                               "",
                                               Helpers.GetIcon("7ee2ef06226a4884f80b7647a2aa2dee"), // mercy sickened
                                               FeatureGroup.None,
                                               UnitCondition.Sickened.CreateImmunity(),
                                               SpellDescriptor.Sickened.CreateBuffImmunity());

            var curse10 = Helpers.CreateFeature("OracleCurse10Wasting", 
                                                "Immune to Disease",
                                                "At 10th level, you gain immunity to disease.",
                                                "",
                                                Helpers.GetIcon("3990a92ce97efa3439e55c160412ce14"), // mercy diseased
                                                FeatureGroup.None,
                                                SpellDescriptor.Disease.CreateSpellImmunity(),
                                                SpellDescriptor.Disease.CreateBuffImmunity());

            var curse15 = Helpers.CreateFeature("OracleCurse15Wasting",
                                                "Immune to Nauseated",
                                                "At 15th level, you are immune to the nauseated condition.",
                                                "",
                                                Helpers.GetIcon("a0cacf71d872d2a42ae3deb6bf977962"), // mercy nauseated
                                                FeatureGroup.None,
                                                UnitCondition.Nauseated.CreateImmunity(),
                                                SpellDescriptor.Nauseated.CreateBuffImmunity());

            wasting = createOracleCurseProgression("OracleWastingCurseProgression", "Wasting",
                                                    "Your body is slowly rotting away.",
                                                    curse, curse5, curse10, curse15);

            wasting_minor.AddComponent(Helpers.PrerequisiteNoFeature(wasting));
        }


        static void createLame()
        {
            var curse = Helpers.CreateFeature("OracleCurseLame",
                                              "Lame",
                                              "One of your legs is permanently wounded, reducing your base land speed by 10 feet if your base speed is 30 feet or more. If your base speed is less than 30 feet, your speed is reduced by 5 feet. Your speed is never reduced due to encumbrance.",
                                              "",
                                              Helpers.GetIcon("f492622e473d34747806bdb39356eb89"), // slow
                                              FeatureGroup.None,
                                              Helpers.Create<EncumbranceMechanics.IgnoreEncumbrence>(),
                                              Helpers.Create<NewMechanics.AddSpeedBonusBasedOnRaceSize>(a => { a.small_race_speed_bonus = -5; a.normal_race_speed_bonus = -10; })
                                              );
            lame_minor = library.CopyAndAdd(curse, "OracleCurseLameMinorFeature", "");
            var hindrance = library.CopyAndAdd(lame_minor, "OracleCurseLameHindranceFeature", "");
            hindrance.RemoveComponents<EncumbranceMechanics.IgnoreEncumbrence>();
            curse_to_hindrance_map.Add(curse, hindrance);
            curse_to_minor_map.Add(curse, lame_minor);

            var curse5 = Helpers.CreateFeature("OracleCurse5Lame", 
                                               "Immune to Fatigue",
                                               "At 5th level, you are immune to the fatigued condition (but not exhaustion).",
                                               "",
                                               Helpers.GetIcon("e5aa306af9b91974a9b2f2cbe702f562"), // mercy fatigue
                                               FeatureGroup.None,
                                               UnitCondition.Fatigued.CreateImmunity(),
                                               SpellDescriptor.Fatigue.CreateBuffImmunity());

            var curse10 = Helpers.CreateFeature("OracleCurse10Lame", 
                                                "Effortless Armor",
                                                "At 10th level, your speed is never reduced by armor.",
                                                "",
                                                Helpers.GetIcon("e1291272c8f48c14ab212a599ad17aac"), // effortless armor
                                                FeatureGroup.None,
                                                // Conceptually similar to ArmorSpeedPenaltyRemoval, but doesn't need 2 ranks in the feat to work.
                                                AddMechanicsFeature.MechanicsFeatureType.ImmunToMediumArmorSpeedPenalty.CreateAddMechanics(),
                                                AddMechanicsFeature.MechanicsFeatureType.ImmunToArmorSpeedPenalty.CreateAddMechanics());

            var curse15 = Helpers.CreateFeature("OracleCurse15Lame", "Immune to Exhausted",
                                                "At 15th level, you are immune to the exhausted condition.",
                                                "be45e9251c134ac9baee97e1e3ffc30a",
                                                Helpers.GetIcon("25641bda25467224e930e8c70eaf9a83"), // mercy exhausted
                                                FeatureGroup.None,
                                                UnitCondition.Exhausted.CreateImmunity(),
                                                SpellDescriptor.Exhausted.CreateBuffImmunity());

            lame = createOracleCurseProgression("OracleLameCurseProgression", "Lame",
                                                "",
                                                curse, curse5, curse10, curse15);
            lame_minor.AddComponent(Helpers.PrerequisiteNoFeature(lame));
        }


        static void createBlackened()
        {
            var burning_hands = library.Get<BlueprintAbility>("4783c3709a74a794dbe7c8e7e0b1b038");
            var scorching_ray = library.Get<BlueprintAbility>("cdb106d53c65bbc4086183d54c3b97c7");
            var burning_arc = library.Get<BlueprintAbility>("eaac3d36e0336cb479209a6f65e25e7c");
            var firebrand = library.Get<BlueprintAbility>("98734a2665c18cd4db71878b0532024a");
            var curse = Helpers.CreateFeature("OracleCurseBlackened",
                                              "Blackened",
                                              "You take a –4 penalty on weapon attack rolls, but you add burning hands to your list of spells known.",
                                              "",
                                              burning_hands.Icon,
                                              FeatureGroup.None,
                                              burning_hands.CreateAddKnownSpell(oracle_class, 1),
                                              Helpers.Create<NewMechanics.WeaponsOnlyAttackBonus>(w => w.value = -4)
                                              );
            curse.ReapplyOnLevelUp = true;

            blackened_minor = Helpers.CreateFeature("OracleCurseBlackenedMinorFeature",
                                                      "Blackened",
                                                      "You take a –4 penalty on weapon attack rolls, but you add burning hands to your list of spells known.",
                                                      "",
                                                      burning_hands.Icon,
                                                      FeatureGroup.None,
                                                      burning_hands.CreateAddKnownSpell(oracle_class, 1),
                                                      Helpers.Create<NewMechanics.WeaponsOnlyAttackBonus>(w => w.value = -4)
                                                      );
            curse_to_minor_map.Add(curse, blackened_minor);
            var hindrance = library.CopyAndAdd(blackened_minor, "OracleCurseBlackenedHindranceFeature", "");
            hindrance.RemoveComponents<AddKnownSpell>();
            curse_to_hindrance_map.Add(curse, hindrance);

            var curse5 = Helpers.CreateFeature("OracleCurse5Blackened", 
                                               curse.Name,
                                               "At 5th level, add scorching ray and burning arc to your list of spells known.",
                                               "",
                                               scorching_ray.Icon,
                                               FeatureGroup.None,
                                               scorching_ray.CreateAddKnownSpell(oracle_class, 2),
                                               burning_arc.CreateAddKnownSpell(oracle_class, 2)
                                               );

            var curse10 = Helpers.CreateFeature("OracleCurse10Blackened",
                                                curse.Name,
                                                "At 10th level, add wall of fire to your list of spells known and your penalty on weapon attack rolls is reduced to –2.",
                                                "",
                                                NewSpells.wall_of_fire.Icon,
                                                FeatureGroup.None,
                                                Helpers.Create<NewMechanics.WeaponsOnlyAttackBonus>(w => w.value = 2),
                                                NewSpells.wall_of_fire.CreateAddKnownSpell(oracle_class, 4)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Blackened", 
                                                curse.Name,
                                                "At 15th level, add firebrand to your list of spells known.",
                                                "",
                                                firebrand.Icon,
                                                FeatureGroup.None,
                                                firebrand.CreateAddKnownSpell(oracle_class, 7));

            blackened = createOracleCurseProgression("OracleBlackenedCurseProgression", "Blackened", 
                                                     "Your hands and forearms are shriveled and blackened, as if you had plunged your arms into a blazing fire, and your thin, papery skin is sensitive to the touch.",
                                                     curse, curse5, curse10, curse15);
            blackened_minor.AddComponent(Helpers.PrerequisiteNoFeature(blackened));
        }


        static void createDeaf()
        {
            var curse = Helpers.CreateFeature("OracleCurseDeaf",
                                              "Deaf",
                                              "You cannot hear and suffer all of the usual penalties for being deafened: -4 penalty on initiative and -4 perception. It does not affect your ability to cast spells.",
                                              "",
                                              Helpers.GetIcon("c3893092a333b93499fd0a21845aa265"),
                                              FeatureGroup.None,
                                              Helpers.CreateAddStatBonus(StatType.Initiative, -4, ModifierDescriptor.UntypedStackable),
                                              Helpers.CreateAddStatBonus(StatType.SkillPerception, -4, ModifierDescriptor.UntypedStackable),   
                                              Common.createSpellImmunityToSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent),
                                              Helpers.Create<SpecificBuffImmunity>(s => s.Buff = Common.deafened),
                                              Helpers.Create<SpecificBuffImmunity>(s => s.Buff = NewSpells.silence_buff),
                                              Helpers.Create<SpecificBuffImmunity>(s => s.Buff = library.Get<BlueprintBuff>("cbfd2f5279f5946439fe82570fd61df2"))//echolocation
                                              );
            curse.ReapplyOnLevelUp = true;

            deaf_minor = Helpers.CreateFeature("OracleCurseDeafMinorFeature",
                                              "Deaf",
                                              "You cannot hear and suffer all of the usual penalties for being deafened: -4 penalty on initiative and -4 perception. It does not affect your ability to cast spells.",
                                              "",
                                              Helpers.GetIcon("c3893092a333b93499fd0a21845aa265"),
                                              FeatureGroup.None,
                                              Helpers.CreateAddStatBonus(StatType.SkillPerception, -4, ModifierDescriptor.UntypedStackable),
                                              Helpers.CreateAddStatBonus(StatType.Initiative, -4, ModifierDescriptor.UntypedStackable),
                                              Common.createSpellImmunityToSpellDescriptor((SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent),
                                              Helpers.Create<SpecificBuffImmunity>(s => s.Buff = Common.deafened),
                                              Helpers.Create<SpecificBuffImmunity>(s => s.Buff = NewSpells.silence_buff),
                                              Helpers.Create<SpecificBuffImmunity>(s => s.Buff = library.Get<BlueprintBuff>("cbfd2f5279f5946439fe82570fd61df2"))//echolocation
                                              );

            curse_to_minor_map.Add(curse, deaf_minor);
            var hindrance = library.CopyAndAdd(deaf_minor, "OracleCurseDeafHindranceFeature", "");
            curse_to_hindrance_map.Add(curse, hindrance);
            var curse5 = Helpers.CreateFeature("OracleCurse5Deaf", 
                                               curse.Name,
                                               "At 5th level, you no longer receive a penalty on Perception checks, and the initiative penalty for being deaf is reduced to –2.",
                                               "",
                                               curse.Icon, FeatureGroup.None,
                                               Helpers.CreateAddStatBonus(StatType.Initiative, 2, ModifierDescriptor.UntypedStackable)
                                               );

            var curse10 = Helpers.CreateFeature("OracleCurse10Deaf",
                                                curse.Name,
                                                "At 10th level, you receive a +3 competence bonus on Perception checks, and you do not suffer any penalty on initiative checks due to being deaf.",
                                                "",
                                                Helpers.GetIcon("c927a8b0cd3f5174f8c0b67cdbfde539"), // remove blindness
                                                FeatureGroup.None,
                                                Helpers.CreateAddStatBonus(StatType.SkillPerception, 3, ModifierDescriptor.Competence),
                                                Helpers.CreateAddStatBonus(StatType.Initiative, 2, ModifierDescriptor.UntypedStackable)
                                                );

            var curse15 = Helpers.CreateFeature("OracleCurse15Deaf",
                                                "Tremorsense",
                                                "At 15th level, you gain tremorsense out to a range of 30 feet.",
                                                "",
                                                Helpers.GetIcon("30e5dc243f937fc4b95d2f8f4e1b7ff3"), // see invisible
                                                FeatureGroup.None,
                                                Helpers.Create<Blindsense>(b => b.Range = 30.Feet()));
            deaf = createOracleCurseProgression("OracleDeafCurseProgression", "Deaf", "", curse, curse5, curse10, curse15);
            deaf_minor.AddComponent(Helpers.PrerequisiteNoFeature(deaf));
        }


        static void createCloudedVision()
        {
            var curse = Helpers.CreateProgression("OracleCurseCloudedVision",
                                                  "Clouded Vision",
                                                  "Your eyes are obscured, making it difficult for you to see.\n"+
                                                  "You cannot see anything beyond 20 feet. Targets beyond this range have concealment, and you cannot target any point past that range.",
                                                  "",
                                                  Helpers.GetIcon("46fd02ad56c35224c9c91c88cd457791"), // blindness
                                                  FeatureGroup.None,
                                                  Helpers.Create<ConcealementMechanics.SetVisibilityLimit>(s => s.visibility_limit = 20.Feet()),
                                                  Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(a => 
                                                                                                              { a.CheckDistance = true;
                                                                                                                a.Descriptor = ConcealmentDescriptor.InitiatorIsBlind;
                                                                                                                a.DistanceGreater = 20.Feet();
                                                                                                                a.Concealment = Concealment.Total;
                                                                                                              }
                                                                                                              )
                                                  );
            clouded_vision_minor = library.CopyAndAdd(curse, "OracleCurseCloudedVisionMinorFeature", "");
            var hindrance = library.CopyAndAdd(clouded_vision_minor, "OracleCurseCloudedVisionHindranceFeature", "");
            curse_to_hindrance_map.Add(curse, hindrance);
            curse_to_minor_map.Add(curse, clouded_vision);

            var curse5 = Helpers.CreateProgression("OracleCurse5CloudedVision",
                                      curse.Name,
                                      "At 5th level, your vision distance increases to 30 feet.",
                                      "",
                                      curse.Icon,
                                      FeatureGroup.None,
                                      Common.createRemoveFeatureOnApply(curse),
                                      Helpers.Create<ConcealementMechanics.SetVisibilityLimit>(s => s.visibility_limit = 30.Feet()),
                                      Helpers.Create<ConcealementMechanics.AddOutgoingConcealment>(a =>
                                                                                                  {
                                                                                                      a.CheckDistance = true;
                                                                                                      a.Descriptor = ConcealmentDescriptor.InitiatorIsBlind;
                                                                                                      a.DistanceGreater = 30.Feet();
                                                                                                      a.Concealment = Concealment.Total;
                                                                                                  }
                                                                                                  )
                                      );
            var curse10 = Helpers.CreateFeature("OracleCurse10CloudedVision", 
                                                "Blindsense",
                                                "At 10th level, you gain blindsense out to a range of 30 feet.",
                                                "",
                                                Helpers.GetIcon("30e5dc243f937fc4b95d2f8f4e1b7ff3"), // see invisible
                                                FeatureGroup.None,
                                                Helpers.Create<Blindsense>(b => b.Range = 30.Feet()));
            var curse15 = Helpers.CreateFeature("OracleCurse15CloudedVision", 
                                                "Blindsight",
                                                "At 15th level, you gain blindsight out to a range of 15 feet.",
                                                "",
                                                Helpers.GetIcon("4cf3d0fae3239ec478f51e86f49161cb"), // true seeing
                                                FeatureGroup.None,
                                                Common.createBuffDescriptorImmunity(SpellDescriptor.GazeAttack),
                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.GazeAttack),
                                                Helpers.Create<Blindsense>(b => { b.Range = 15.Feet(); b.Blindsight = true; }));
            clouded_vision = createOracleCurseProgression("OracleCloudedVisionCurseProgression", "Clouded Vision", "",
                                                          curse, curse5, curse10, curse15);
            clouded_vision_minor.AddComponent(Helpers.PrerequisiteNoFeature(clouded_vision));
        }


        static BlueprintProgression createOracleCurseProgression(string name, string display_name, string base_description, params BlueprintFeature[] features)
        {
            var curse_description = base_description;
            foreach (var f in features)
            {
                if (!curse_description.Empty())
                {
                    curse_description += "\n";
                }
                curse_description += f.Description;
            }
            var curse = Helpers.CreateProgression(name,
                                                  display_name,
                                                  curse_description,
                                                  "",
                                                  features[0].Icon,
                                                  FeatureGroup.None);

            curse.LevelEntries = new LevelEntry[] { Helpers.LevelEntry(1, features[0]), Helpers.LevelEntry(5, features[1]), Helpers.LevelEntry(10, features[2]), Helpers.LevelEntry(15, features[3]) };
            curse.UIGroups = Helpers.CreateUIGroups(features);
            curse.Classes = getOracleArray();

            Summoner.addCurseProgressionToPactboundCurse(curse);
            return curse;
        }


        static void createCureInflictSpells()
        {
            var cure_spells = new BlueprintAbility[]
                {
                library.Get<BlueprintAbility>("5590652e1c2225c4ca30c4a699ab3649"),
                library.Get<BlueprintAbility>("6b90c773a6543dc49b2505858ce33db5"),
                library.Get<BlueprintAbility>("3361c5df793b4c8448756146a88026ad"),
                library.Get<BlueprintAbility>("41c9016596fe1de4faf67425ed691203"),
                library.Get<BlueprintAbility>("5d3d689392e4ff740a761ef346815074"),
                library.Get<BlueprintAbility>("571221cc141bc21449ae96b3944652aa"),
                library.Get<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397"),
                library.Get<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449"),
                };
            var inflict_spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("e5af3674bb241f14b9a9f6b0c7dc3d27"),
                library.Get<BlueprintAbility>("65f0b63c45ea82a4f8b8325768a3832d"),
                library.Get<BlueprintAbility>("bd5da98859cf2b3418f6d68ea66cabbe"),
                library.Get<BlueprintAbility>("651110ed4f117a948b41c05c5c7624c0"),
                library.Get<BlueprintAbility>("9da37873d79ef0a468f969e4e5116ad2"),
                library.Get<BlueprintAbility>("03944622fbe04824684ec29ff2cec6a7"),
                library.Get<BlueprintAbility>("820170444d4d2a14abc480fcbdb49535"),
                library.Get<BlueprintAbility>("5ee395a2423808c4baf342a4f8395b19"),
            };


            BlueprintComponent[] add_cure_spells = new BlueprintComponent[cure_spells.Length];
            
            for (int i = 0; i < cure_spells.Length; i++)
            {
                add_cure_spells[i] = Helpers.CreateAddKnownSpell(cure_spells[i], oracle_class, i + 1);
            }
            var free_cure_spells = Helpers.CreateFeature("OracleFreeCureSpells",
                                                                "Cure Spells",
                                                                "In addition to the spells gained by oracles as they gain levels, each oracle also adds all of either the cure spells or the inflict spells to her list of spells known (cure spells include all spells with “cure” in the name, inflict spells include all spells with “inflict” in the name). These spells are added as soon as the oracle is capable of casting them. This choice is made when the oracle gains her first level and cannot be changed.",
                                                                "",
                                                                cure_spells[0].Icon,
                                                                FeatureGroup.None,
                                                                add_cure_spells);
            for (int i = 0; i < cure_spells.Length; i++)
            {
                cure_spells[i].AddRecommendNoFeature(free_cure_spells);
            }

            BlueprintComponent[] add_inflict_spells = new BlueprintComponent[inflict_spells.Length];

            for (int i = 0; i < inflict_spells.Length; i++)
            {
                add_inflict_spells[i] = Helpers.CreateAddKnownSpell(inflict_spells[i], oracle_class, i + 1);
            }
            var free_inflict_spells = Helpers.CreateFeature("OracleFreeInflictSpells",
                                                                "Inflict Spells",
                                                                "In addition to the spells gained by oracles as they gain levels, each oracle also adds all of either the cure spells or the inflict spells to her list of spells known (cure spells include all spells with “cure” in the name, inflict spells include all spells with “inflict” in the name). These spells are added as soon as the oracle is capable of casting them. This choice is made when the oracle gains her first level and cannot be changed.",
                                                                "",
                                                                inflict_spells[0].Icon,
                                                                FeatureGroup.None,
                                                                add_inflict_spells);
            for (int i = 0; i < inflict_spells.Length; i++)
            {
                inflict_spells[i].AddRecommendNoFeature(free_inflict_spells);
            }


            cure_inflict_spells_selection = Helpers.CreateFeatureSelection("OracleBonusSpellsFeature",
                                                                           "Bonus Spells",
                                                                           free_cure_spells.Description,
                                                                           "",
                                                                           null,
                                                                           FeatureGroup.None);
            cure_inflict_spells_selection.AllFeatures = new BlueprintFeature[] { free_cure_spells, free_inflict_spells };
        }


        static void createOracleOrisons()
        {
            oracle_orisons = library.CopyAndAdd<BlueprintFeature>(
                 "e62f392949c24eb4b8fb2bc9db4345e3", // cleric orisions
                 "OracleOrisonsFeature",
                 "");
            oracle_orisons.SetDescription("Oracles learn a number of orisons, or 0-level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            oracle_orisons.ReplaceComponent<BindAbilitiesToClass>(c => c.CharacterClass = oracle_class);
        }


        static void createOracleProficiencies()
        {
            oracle_proficiencies = library.CopyAndAdd<BlueprintFeature>("8c971173613282844888dc20d572cfc9", //cleric proficiencies
                                                                "OracleProficiencies",
                                                                "");
            oracle_proficiencies.SetName("Oracle Proficiencies");
            oracle_proficiencies.SetDescription("Oracles are proficient with all simple weapons, light armor, medium armor, and shields (except tower shields). Some oracle revelations grant additional proficiencies.");
        }
    }
}
