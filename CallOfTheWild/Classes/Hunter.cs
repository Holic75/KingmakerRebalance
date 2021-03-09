using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;



namespace CallOfTheWild
{
    public class Hunter
    {
        static internal readonly Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup AnimalFocusGroup = ActivatableAbilityGroupExtension.AnimalFocus.ToActivatableAbilityGroup();//Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.Judgment;
        public static BlueprintCharacterClass hunter_class;
        public static BlueprintProgression hunter_progression;
        static  LibraryScriptableObject library => Main.library;
        static public BlueprintFeature animal_focus;
        static public BlueprintFeature animal_focus_ac;
        static public BlueprintFeature animal_focus_additional_use;
        static public BlueprintFeature animal_focus_additional_use_ac;
        static public BlueprintFeature animal_focus_additional_use2;
        static public BlueprintFeature animal_focus_additional_use_ac2;
        static public BlueprintFeatureSelection hunter_animal_companion;
        static public BlueprintFeatureSelection precise_companion;
        static public BlueprintFeatureSelection hunter_teamwork_feat;
        static public BlueprintFeature hunter_tactics;
        static public BlueprintFeature hunter_woodland_stride;
        static public BlueprintArchetype forester_archetype;
        static public BlueprintArchetype feykiller_archetype;
        static public BlueprintArchetype divine_hunter_archetype;
        static public BlueprintFeatureSelection hunter_otherwordly_companion;
        static public BlueprintArchetype primal_companion_hunter;

        static public BlueprintFeature ac_smite_good_feature;
        static public BlueprintFeature ac_smite_evil_feature;
        static public BlueprintFeature ac_smite_law_feature;
        static public BlueprintFeature ac_smite_chaos_feature;
        static public BlueprintFeature fiendish_template;
        static public BlueprintFeature celestial_template;
        static public BlueprintFeature entropic_template;
        static public BlueprintFeature resolute_template;

        static public BlueprintFeature forester_tactician;
        static public BlueprintAbility tactician_ability;

        static private AnimalFocusEngine animal_focus_engine;
        static public BlueprintFeature planar_focus;

        static public BlueprintFeatureSelection trick_selection;
        static public BlueprintAbilityResource trick_resource;

        static public BlueprintFeature animal_focus_feykiller;
        static public BlueprintFeature animal_focus_feykiller_ac;
        static public BlueprintFeature iron_talons_ac;

        static public BlueprintAbilityResource raise_companion_resource;
        static public BlueprintFeature raise_animal_companion;
        static public BlueprintFeature forester_breath_of_life;

        static public BlueprintArchetype feral_hunter;
        static public BlueprintFeature[] wild_shape;
        static public BlueprintFeature[] summon_nature_ally;
        static public BlueprintSummonPool summon_nature_ally_pool;
        static public BlueprintAbilityResource wildshape_resource;
        static public BlueprintFeature extra_wildshape;
        static public BlueprintFeatureSelection precise_nature_ally;
        




        internal static void createHunterClass()
        {
            var ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");

            hunter_class = Helpers.Create<BlueprintCharacterClass>();
            hunter_class.name = "HunterClass";
            library.AddAsset(hunter_class, "32486dcfda61462fbfd66b5644786b39");

            var inquistor_class = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            var sacred_huntsmaster_archetype = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012");
            animal_focus_engine = new AnimalFocusEngine();
            animal_focus_engine.initialize(new BlueprintCharacterClass[] { hunter_class, inquistor_class }, sacred_huntsmaster_archetype, 0, "");
            createAnimalFocusFeat();

            hunter_class.LocalizedName = Helpers.CreateString("Hunter.Name", "Hunter");
            hunter_class.LocalizedDescription = Helpers.CreateString("Hunter.Description",
                "Hunters are warriors of the wilds that have forged close bonds with trusted animal companions.They focus their tactics on fighting alongside their companion animals as a formidable team of two.Able to cast a wide variety of nature spells and take on the abilities and attributes of beasts, hunters magically improve both themselves and their animal companions.\n"
                + "Role: Hunters can adapt their tactics to many kinds of opponents, and cherish their highly trained animal companions.As a team, the hunter and her companion can react to danger with incredible speed, making them excellent scouts, explorers, and saboteurs."
                );
            hunter_class.m_Icon = ranger_class.Icon;
            hunter_class.SkillPoints = ranger_class.SkillPoints;
            hunter_class.HitDie = DiceType.D8;
            hunter_class.BaseAttackBonus = druid_class.BaseAttackBonus;
            hunter_class.FortitudeSave = ranger_class.FortitudeSave;
            hunter_class.ReflexSave = ranger_class.ReflexSave;
            hunter_class.WillSave = ranger_class.WillSave;
            hunter_class.Spellbook = createHunterSpellbook();
            hunter_class.ClassSkills = ranger_class.ClassSkills;
            hunter_class.IsDivineCaster = true;
            hunter_class.IsArcaneCaster = false;
            hunter_class.StartingGold = ranger_class.StartingGold;
            hunter_class.PrimaryColor = ranger_class.PrimaryColor;
            hunter_class.SecondaryColor = ranger_class.SecondaryColor;
            hunter_class.RecommendedAttributes = ranger_class.RecommendedAttributes;
            hunter_class.NotRecommendedAttributes = ranger_class.NotRecommendedAttributes;
            hunter_class.EquipmentEntities = ranger_class.EquipmentEntities;
            hunter_class.MaleEquipmentEntities = ranger_class.MaleEquipmentEntities;
            hunter_class.FemaleEquipmentEntities = ranger_class.FemaleEquipmentEntities;
            hunter_class.ComponentsArray = ranger_class.ComponentsArray;
            hunter_class.StartingItems = ranger_class.StartingItems;

            createHunterProgression();
            hunter_class.Progression = hunter_progression;
            createDivineHunterArchetype();
            createForesterArchetype();
            createFeykillerArchetype();
            createFeralHunterArchetype();
            createPrimalCompanionHunterArchetype();
            hunter_class.Archetypes = new BlueprintArchetype[] {divine_hunter_archetype, forester_archetype, feykiller_archetype, primal_companion_hunter, feral_hunter };
            Helpers.RegisterClass(hunter_class);

            Common.addMTDivineSpellbookProgression(hunter_class, hunter_class.Spellbook, "MysticTheurgeHunter",
                                         Common.createPrerequisiteClassSpellLevel(hunter_class, 2));


            createPlanarFocus();
        }


        static void createFeralHunterArchetype()
        {
            feral_hunter = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "FeralHunterHunterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Feral Hunter");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A feral hunter has forged a bond with nature that’s so strong that she doesn’t merely channel the aspects of animals—she actually becomes an animal herself. Though she lacks an animal companion, a feral hunter is in tune with the beast lurking within her flesh and spirit, and lives in a near-wild state of being. A feral hunter often resembles a lycanthrope, but her power comes from her own nature and is not influenced by moonlight or silver.");

            });
            Helpers.SetField(feral_hunter, "m_ParentClass", hunter_class);
            library.AddAsset(feral_hunter, "");
            feral_hunter.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, hunter_animal_companion, animal_focus_ac),
                                                                  Helpers.LevelEntry(2, precise_companion),
                                                                  Helpers.LevelEntry(3, hunter_tactics),
                                                                  Helpers.LevelEntry(6, hunter_teamwork_feat),
                                                                  Helpers.LevelEntry(7, trick_selection),
                                                                  Helpers.LevelEntry(8, animal_focus_additional_use_ac),
                                                                  Helpers.LevelEntry(9, hunter_teamwork_feat),
                                                                  Helpers.LevelEntry(10, raise_animal_companion),
                                                                  Helpers.LevelEntry(12, hunter_teamwork_feat),
                                                                  Helpers.LevelEntry(13, trick_selection),
                                                                  Helpers.LevelEntry(15, hunter_teamwork_feat),
                                                                  Helpers.LevelEntry(18, hunter_teamwork_feat),
                                                                  Helpers.LevelEntry(19, trick_selection),
                                                                  Helpers.LevelEntry(20, animal_focus_additional_use_ac2) };

            createFeralHunterWildshape();
            createSummonNatureAlly();
            createPreciseNatureAlly();
            var feral_hunter_animal_focus_additional_use = library.CopyAndAdd<BlueprintFeature>(animal_focus_additional_use, "FeralHunterAnimalFocusFeature", "");

            var wildshape_progression = Wildshape.addWildshapeProgression("FeralHuntterWildshapeProgression",
                                                              new BlueprintCharacterClass[] { hunter_class },
                                                              new BlueprintArchetype[0],
                                                              new LevelEntry[]
                                                              {
                                                                             Helpers.LevelEntry(4, wild_shape[0], wild_shape[1]), //wolf, leopard
                                                                             Helpers.LevelEntry(6, wild_shape[2], wild_shape[3], extra_wildshape), //bear, dire wolf
                                                                             Helpers.LevelEntry(8, wild_shape[4], wild_shape[5], extra_wildshape), //smilodon, mastodon
                                                                             Helpers.LevelEntry(10, extra_wildshape),
                                                                             Helpers.LevelEntry(12, extra_wildshape),
                                                                             Helpers.LevelEntry(14, extra_wildshape),
                                                                             Helpers.LevelEntry(16, extra_wildshape),
                                                                             Helpers.LevelEntry(18, extra_wildshape),
                                                                             Helpers.LevelEntry(20, extra_wildshape),
                                                              }
                                                              );

            feral_hunter_animal_focus_additional_use.SetNameDescription("Additional Animal Focus", "The feral hunter can apply additional animal focus to herself.");
            feral_hunter.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, /*feral_hunter_animal_focus_additional_use,*/ summon_nature_ally[0], wildshape_progression),
                                                                Helpers.LevelEntry(2, precise_nature_ally),
                                                                Helpers.LevelEntry(3, summon_nature_ally[1]),
                                                                Helpers.LevelEntry(5, summon_nature_ally[2]),
                                                                Helpers.LevelEntry(7, summon_nature_ally[3]),
                                                                Helpers.LevelEntry(9, summon_nature_ally[4]),
                                                                Helpers.LevelEntry(11, summon_nature_ally[5]),
                                                                Helpers.LevelEntry(13, summon_nature_ally[6]),
                                                                Helpers.LevelEntry(15, summon_nature_ally[7]),
                                                                Helpers.LevelEntry(17, summon_nature_ally[8]),
                                                             };


            hunter_progression.UIGroups = hunter_progression.UIGroups.AddToArray(Helpers.CreateUIGroups(summon_nature_ally));
            hunter_progression.UIGroups = hunter_progression.UIGroups.AddToArray(Helpers.CreateUIGroups(wild_shape[0], wild_shape[2], wild_shape[4]));
            hunter_progression.UIGroups = hunter_progression.UIGroups.AddToArray(Helpers.CreateUIGroups(wild_shape[1], wild_shape[3], wild_shape[5]));
        }



        static void createPreciseNatureAlly()
        {
            precise_nature_ally = library.CopyAndAdd<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb",
                                          "HunterPreciseNatureAlly",
                                          "");
            precise_nature_ally.SetName("Precise Nature's Ally");
            precise_nature_ally.SetDescription("At 2nd level, a hunter chooses either Precise Shot or Outflank as a bonus feat. She does not need to meet the prerequisites for this feat. In addtion the feral hunter grants all her teamwork feats to all creatures she summons with summon nature’s ally.");

            var outflank = library.TryGet<Kingmaker.Blueprints.Classes.BlueprintFeature>("422dab7309e1ad343935f33a4d6e9f11");

            precise_nature_ally.AllFeatures = new BlueprintFeature[] {library.TryGet< Kingmaker.Blueprints.Classes.BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665"), //precise shot
                                                                           outflank };
            precise_nature_ally.Features = precise_companion.AllFeatures;
            precise_nature_ally.IgnorePrerequisites = true;

            precise_nature_ally.AddComponent(library.Get<BlueprintFeature>("c3abcce19f9f80640a867c9e75f880b2").GetComponent<OnSpawnBuff>()); //from monster tactics
            precise_nature_ally.ReplaceComponent<OnSpawnBuff>(o => o.IfHaveFact = precise_nature_ally);
            library.Get<BlueprintBuff>("81ddc40b935042844a0b5fb052eeca73").SetDescription("This summoned creature receives all teamwork feats that its summoner possesses.");
        }


        static void createSummonNatureAlly()
        {
            var summon_resource = Helpers.CreateAbilityResource("FeralHunterSummonResource", "", "", "", null);
            summon_resource.SetIncreasedByStat(3, StatType.Wisdom);
            
            var summon_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", "FeralHunterSummonPool", "");
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

            var description = "Starting at 1st level, a feral hunter can cast summon nature's ally I as a spell-like ability a number of times per day equal to 3 + her Wisdom modifier. She can cast this spell as a standard action, and the creatures remain for 1 minute per level (instead of 1 round per level). At 3rd level and every 2 hunter levels thereafter, the power of this ability increases by 1 spell level, allowing her to summon more powerful creatures (to a maximum of summon nature's ally IX at 17th level). A feral hunter cannot have more than one summon nature's ally spell active in this way at a time; if she uses another, any existing summon nature's ally immediately ends.";

            summon_nature_ally = new BlueprintFeature[9];
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
                    var ability = library.CopyAndAdd<BlueprintAbility>(s.AssetGuid, "FeralHunter" + s.name, "");
                    ability.RemoveComponents<SpellListComponent>();
                    ability.AddComponent(summon_resource.CreateResourceLogic());
                    foreach (var c in ability.GetComponents<ContextRankConfig>())
                    {
                        if (!c.IsFeatureList)
                        {
                            var new_c = c.CreateCopy(crc => { Helpers.SetField(crc, "m_Class", new BlueprintCharacterClass[] { hunter_class }); Helpers.SetField(crc, "m_BaseValueType", ContextRankBaseValueType.ClassLevel); });
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
                    summon_base = Common.createVariantWrapper($"FeralHunterSummonNaturesAlly{i + 1}Base", "", summon_spells.ToArray());
                    summon_base.SetNameDescription("Summon Nature's Ally " + Common.roman_id[i + 1], description);
                }

                summon_nature_ally[i] = Helpers.CreateFeature($"FeralHunterSummonNaturesAlly{i + 1}Feature",
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


        static void createFeralHunterWildshape()
        {
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            //fix scaling
            foreach (var s in Wildshape.animal_wildshapes)
            {
                ClassToProgression.addClassToAbility(hunter_class, new BlueprintArchetype[] { feral_hunter }, s, druid);
                /*s.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(ContextRankBaseValueType.SummClassLevelWithArchetype,
                                                                                       classes: new BlueprintCharacterClass[] { druid, warpriest_class },
                                                                                       archetype: feral_champion)
                                                     );*/
            }

            wildshape_resource = library.Get<BlueprintAbilityResource>("ae6af4d58b70a754d868324d1a05eda4");
            extra_wildshape = library.Get<BlueprintFeature>("f78260b9a089ccc44b55f0fed08b1752");

            string description = "At 4th level, a feral hunter gains the ability to change shape. This ability functions like the druid wild shape ability, except the hunter can take only animal forms (not elemental or plant forms). The hunter’s effective druid level is equal to her class level. A feral hunter can use wild shape once per day at 4th level and one additional time per day every 2 levels thereafter.";
            wild_shape = new BlueprintFeature[Wildshape.animal_wildshapes.Count];

            for (int i = 0; i < wild_shape.Length; i++)
            {

                wild_shape[i] = Helpers.CreateFeature("FeralHunter" + Wildshape.animal_wildshapes[i].name + "Feature",
                                                      Wildshape.animal_wildshapes[i].Name,
                                                      description,
                                                      "",
                                                      Wildshape.animal_wildshapes[i].Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(Wildshape.animal_wildshapes[i])
                                                      );
                if (i == 0)
                {
                    wild_shape[i].AddComponents(wildshape_resource.CreateAddAbilityResource(),
                                                Helpers.CreateAddFact(Wildshape.first_wildshape_form)
                                                );
                }
            }
        }


        static void createPrimalCompanionHunterArchetype()
        {
            primal_companion_hunter = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "PrimalCOmpanionHunterHunterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Primal Companion Hunter");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Most hunters are skilled at awakening the primal beasts inside themselves. However, some can instead activate the primal essence within their animal companion. These primal companion hunters bestow upon their companions the ability to suddenly manifest new and terrifying powers—throwbacks to long-extinct beasts, bizarre mutations from extreme environments, or new abilities crafted from generations of selective breeding."
                                                              );

            });
            Helpers.SetField(primal_companion_hunter, "m_ParentClass", hunter_class);
            library.AddAsset(primal_companion_hunter, "008d810f24624b8d88da439715416204");

            primal_companion_hunter.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, animal_focus, animal_focus_ac),
                                                                        Helpers.LevelEntry(8, animal_focus_additional_use, animal_focus_additional_use_ac),
                                                                        Helpers.LevelEntry(20, animal_focus_additional_use2, animal_focus_additional_use_ac2)
                                                                       };


            var resource = Helpers.CreateAbilityResource("PrimalSurgePrimalCompanionResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 20, 1, 20, 0, 0, 0, new BlueprintCharacterClass[] { hunter_class });

            var primal_surge_ability = Evolutions.getGrantTemporaryEvolutionAbility(4, false,
                                                                                    "PrimalSurgePrimalCompanion",
                                                                                    "Primal Surge",
                                                                                    "At 8th level, once per day as a swift action, a primal companion hunter can touch her animal companion and grant it one evolution that costs up to 4 evolution points. The companion must meet the prerequisites of the selected evolution. Unlike the evolutions from primal transformation, this evolution is not set; it can be changed each time the hunter uses this ability.\n"
                                                                                    + "This ability can grant an evolution that allows additional evolution points to be spent to upgrade that evolution, and any points left over can be spent on such upgrades. This ability cannot be used to grant an upgrade to an evolution that the companion already possesses.",
                                                                                    Helpers.GetIcon("7bdb6a9fb6b37614e96f155748ae50c6"), //aspect of the falcon
                                                                                    AbilityType.Supernatural,
                                                                                    CommandType.Swift,
                                                                                    Helpers.minutesPerLevelDuration,
                                                                                    false,
                                                                                    Helpers.CreateResourceLogic(resource),
                                                                                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                                    classes: new BlueprintCharacterClass[] { hunter_class }
                                                                                                                    )
                                                                                    );

            var primal_surge = Common.AbilityToFeature(primal_surge_ability, false);
            primal_surge_ability.AddComponent(Helpers.CreateAddAbilityResource(resource));

            var primal_master = Helpers.CreateFeature("PrimalMasterFeature",
                                                      "Primal Master",
                                                      "At 20th level, a primal companion hunter becomes in tune with his primal nature. She can use primal surge as a free action up to 2 times per day.",
                                                      "",
                                                      Helpers.GetIcon("d99dfc9238a8a6646b32be09057c1729"), //beast totem
                                                      FeatureGroup.None,
                                                      Helpers.Create<TurnActionMechanics.UseAbilitiesAsFreeAction>(u => u.abilities = new BlueprintAbility[] { primal_surge_ability })
                                                      );

            LevelEntry[] level_entries = new LevelEntry[20];
            List<BlueprintFeature> primal_transformations = new List<BlueprintFeature>();
            for (int lvl = 1; lvl <= 20; lvl++)
            {
                var feature = Helpers.CreateFeature($"PrimalTransformation{lvl}Feature",
                                                    "Primal Transformation",
                                                    "At first level, a primal companion hunter awakens a primal creature from within his animal companion. The animal companion gains a pool of 2 evolution points that can be used to give the companion evolutions as if it were an eidolon. A primal companion hunter uses her hunter level to determine her effective summoner level for the purpose of qualifying for evolutions and determining their effects. At 8th level, the number of evolution points in her pool increases to 4, and at 15th level, it increases to 6.\n"
                                                    + "Whenever Primal Companion Hunter gains a level, she may redistribute evolution points spent previously.",
                                                    "",
                                                    Helpers.GetIcon("56923211d2ac95e43b8ac5031bab74d8"),
                                                    FeatureGroup.None);
                if (lvl == 1 || lvl == 8 || lvl == 15)
                {
                    feature.AddComponent(Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(n => n.amount = 2));
                }
                feature.AddComponent(Helpers.Create<EvolutionMechanics.RefreshEvolutionsOnLevelUp>());
                feature.AddComponent(Helpers.Create<EvolutionMechanics.addEvolutionSelection>(a => a.selection = Evolutions.evolution_selection));

                if (lvl == 8)
                {
                    level_entries[lvl - 1] = Helpers.LevelEntry(lvl, feature, primal_surge);
                }
                else if (lvl == 20)
                {
                    level_entries[lvl - 1] = Helpers.LevelEntry(lvl, feature, primal_master);
                }
                else
                {
                    level_entries[lvl - 1] = Helpers.LevelEntry(lvl, feature);
                }

                if (lvl != 1)
                {
                    feature.HideInCharacterSheetAndLevelUp = true;
                    feature.HideInUI = true;
                }
                primal_transformations.Add(feature);
            }


            primal_companion_hunter.AddFeatures = level_entries;
            //hunter_class.Progression.UIGroups = hunter_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(primal_transformations.ToArray()));
            hunter_class.Progression.UIGroups = hunter_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(primal_transformations[0], primal_surge, primal_master));

            Evolutions.addClassToExtraEvalution(hunter_class, primal_companion_hunter);
        }




        static void createPlanarFocus()
        {
            var inquistor_class = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");          
            var sacred_huntsmaster_archetype = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012");
            planar_focus = animal_focus_engine.createPlanarFocus("Hunter, Sacred Huntsmaster", animal_focus_ac);

            
            planar_focus.AddComponents(Helpers.PrerequisiteClassLevel(hunter_class, 1, true),
                                       Common.createPrerequisiteArchetypeLevel(inquistor_class, sacred_huntsmaster_archetype, 4, true)
                                       );
        }


        static void createFeykillerArchetype()
        {
            //Since most of feykiller abilities replace the ones that can not be implemented in the game
            //in order to compensate I decided to remove additional animal focus uses at levels 8 and, 20 and give only one only at level 14
            createFeykillerAnimalFocusFeat();
            var resist_nature_lure = library.Get<BlueprintFeature>("ad6a5b0e1a65c3540986cf9a7b006388");
            resist_nature_lure.SetDescription("Starting at 4th level, a character gains a +4 bonus on saving throws against the spell-like and supernatural abilities of fey");
            var grounded = library.CopyAndAdd<BlueprintFeature>("c532e8f7a393d0c4580f017d225d4fe2", "ForesterGroundedFeature", "6a62762fb54a4671aa58d79490631822"); //from beguiling immunity
            grounded.SetDescription("At 17th level, a feykiller gains a +4 insight bonus on saving throws against illusion and enchantment effects, and she is immune to illusion and enchantment effects created by fey.");
            grounded.SetName("Grounded");

            var illusion_save_bonus = Helpers.Create<SavingThrowBonusAgainstSchool>();
            illusion_save_bonus.Value = 4;
            illusion_save_bonus.School = SpellSchool.Illusion;
            illusion_save_bonus.ModifierDescriptor = ModifierDescriptor.Insight;

            var enchancement_save_bonus = Helpers.Create<SavingThrowBonusAgainstSchool>();
            enchancement_save_bonus.Value = 4;
            enchancement_save_bonus.School = SpellSchool.Enchantment;
            enchancement_save_bonus.ModifierDescriptor = ModifierDescriptor.Insight;

            grounded.AddComponent(illusion_save_bonus);
            grounded.AddComponent(enchancement_save_bonus);
            var iron_talons = library.CopyAndAdd<BlueprintFeature>("7d62b8531749ea74292b0d39b4b7fc19","FeykillerIronTalonsFeature", "66697558d03540bb87989ec1573d57fb"); //from feybane
            iron_talons.RemoveComponent(iron_talons.GetComponent <Kingmaker.Designers.Mechanics.Facts.SpellPenetrationBonus >());
            iron_talons.SetName("Iron Talons");
            iron_talons.SetDescription("Through training and prayer, a feykiller imbues her animal companion with an enhanced ability to fight fey. At 7th level, all of her animal companion’s natural attacks are treated as cold iron.");

            iron_talons_ac = Helpers.CreateFeature("FeykillerIronTalonsAC",
                                            iron_talons.Name,
                                            iron_talons.Description,
                                            "5dc1f960b2494fdcbe9420eaeea5b81f",
                                            iron_talons.Icon,
                                            FeatureGroup.None,
                                            Common.createAddFeatToAnimalCompanion(iron_talons)
                                            );

            feykiller_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "FeykillerHunterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Feykiller");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some hunters in fey-plagued regions are dedicated to tracking down and eradicating these threats. They use their connection to the natural world to ground their senses and fight corrupted intrusions.");

            });
            Helpers.SetField(feykiller_archetype, "m_ParentClass", hunter_class);
            library.AddAsset(feykiller_archetype, "4165bde18ad94688b1eab678ccda5f17");
            feykiller_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, animal_focus, animal_focus_ac),
                                                                  Helpers.LevelEntry(7, trick_selection),
                                                                  Helpers.LevelEntry(8, animal_focus_additional_use, animal_focus_additional_use_ac),
                                                                  Helpers.LevelEntry(13, trick_selection),
                                                                  Helpers.LevelEntry(19, trick_selection),
                                                                  Helpers.LevelEntry(20, animal_focus_additional_use2, animal_focus_additional_use_ac2) };

            feykiller_archetype.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, animal_focus_feykiller, animal_focus_feykiller_ac),
                                                                Helpers.LevelEntry(4, resist_nature_lure),
                                                                Helpers.LevelEntry(7, iron_talons_ac),
                                                                Helpers.LevelEntry(8, animal_focus_additional_use, animal_focus_additional_use_ac),
                                                                Helpers.LevelEntry(17, grounded),
                                                                Helpers.LevelEntry(20, animal_focus_additional_use2, animal_focus_additional_use_ac2),
                                                              };

            hunter_progression.UIGroups[1].Features.Add(animal_focus_feykiller);
            hunter_progression.UIGroups[2].Features.Add(animal_focus_feykiller_ac);
            hunter_progression.UIGroups = hunter_progression.UIGroups.AddToArray(Helpers.CreateUIGroups(resist_nature_lure, iron_talons_ac, grounded));
        }


        static BlueprintFeature createFeykillerAnimalFocus()
        {
            return animal_focus_engine.createFeykillerAnimalFocus();
        }


        static void createFeykillerAnimalFocusFeat()
        {
            var animal_growth = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("56923211d2ac95e43b8ac5031bab74d8");
            animal_focus_feykiller = createFeykillerAnimalFocus();
            animal_focus_feykiller_ac = Helpers.CreateFeature("FeykillerAnimalFocusAc",
                                                        "Feykiller Animal Focus (Animal Companion)",
                                                        "The character can apply animal focus to her animal companion.",
                                                        "23accdec89ac4ea8a3547d0ca0b5719a",
                                                        animal_growth.Icon,
                                                        FeatureGroup.None,
                                                        Common.createAddFeatToAnimalCompanion(animal_focus_feykiller)
                                                        );
        }

        static void createForesterArchetype()
        {
            forester_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ForesterHunterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Forester");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While all hunters have a bond with the natural world, a forester has a stronger tie to her environment than to the animals within it. While most foresters feel strong bonds with woodland regions, the archetype functions well in other terrains as well. In such cases, a forester might refer to herself by a different name that more accurately reflects her chosen terrain. For example, a forester who favors bogs and marshes might call herself a “swamper,” while one who favors frozen regions might call herself a “glacier guardian.” As foresters gain levels and take on new favored terrains, they often eschew such titles completely, viewing them as unnecessary, and merely refer to themselves as guardians of the wild or champions of nature’s will—but regardless of the name, their devotion remains.");

            });
            Helpers.SetField(forester_archetype, "m_ParentClass", hunter_class);
            library.AddAsset(forester_archetype, "fabce7959e2f44119cc9ef8a778e9ebd");
            forester_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, hunter_animal_companion, animal_focus_ac),
                                                                  Helpers.LevelEntry(2, precise_companion),
                                                                  Helpers.LevelEntry(3, hunter_tactics),
                                                                  Helpers.LevelEntry(7, trick_selection),
                                                                  Helpers.LevelEntry(8, animal_focus_additional_use_ac),
                                                                  Helpers.LevelEntry(10, raise_animal_companion),
                                                                  Helpers.LevelEntry(13, trick_selection),
                                                                  Helpers.LevelEntry(19, trick_selection),
                                                                  Helpers.LevelEntry(20, animal_focus_additional_use_ac2) };

            var evasion = library.Get<BlueprintFeature>("576933720c440aa4d8d42b0c54b77e80");
            var improved_evasion = library.Get<BlueprintFeature>("ce96af454a6137d47b9c6a1e02e66803");
            var camouflage = library.CopyAndAdd<BlueprintFeature>("ff1b5aa8dcc7d7d4d9aa85e1cb3f9e88", "ForesterCamouflageFeature", "204bfa0c3232403db8105d56d8bda1be");
            var camouflage_ability = library.CopyAndAdd<BlueprintAbility>("b26a123a009d4a141ac9c19355913285", "ForesterCamouflageAbility","990d21cb474946e6bdf293d4fe9009e0");
            camouflage_ability.SetDescription("At 7th level, a forester can use the Stealth skill to hide in any of her favored terrains, even if the terrain doesn’t grant cover or concealment.");
            camouflage.SetComponents(Helpers.CreateAddFact(camouflage_ability));
            camouflage.SetDescription(camouflage_ability.Description);

            var forester_favored_terrain_selection = Common.copyRenameSelection("a6ea422d7308c0d428a541562faedefd",
                                                                                 "Forester",
                                                                                 "A forester gains the ranger’s favored terrain ability. She gains her first favored terrain at 5th level and a new favored terrain every 4 levels thereafter. The forester gains a +2 bonus on initiative checks and Lore (Nature), Perception, and Stealth skill checks when he is in this terrain. In addition, at each such interval, the bonuses on initiative checks and skill checks in one favored terrain (including the one just selected, if so desired) increase by 2.",
                                                                                 "e36b551be4684d4388242b405f7a8732");

            var forester_improved_favored_terrain_selection = library.CopyAndAdd<BlueprintFeatureSelection>(forester_favored_terrain_selection.AssetGuid, "ImprovedForesterFavoredTerrainSelection", "");
            forester_improved_favored_terrain_selection.Mode = SelectionMode.OnlyRankUp;
            forester_improved_favored_terrain_selection.SetName("Improved Favored Terrain");


            var forester_favored_enemy_selection = Common.copyRenameSelection("16cc2c937ea8d714193017780e7d4fc6",
                                                                     "Forester",
                                                                     "At 6th level, a forester selects a creature type from the ranger favored enemies list. He gets a + 2 bonus on weapon attack and damage rolls against them.\nAt 10th level and every four levels thereafter( 14th, and 18th level), the hunter may select an additional favored enemy.\nIf the forester chooses humanoids or outsiders as a favored enemy, he must also choose an associated subtype, as indicated on the table below. If a specific creature falls into more than one category of favored enemy, the forester's bonuses do not stack; he simply uses whichever bonus is higher.",
                                                                     "c8fec1d3bf354c06bc9a3d356453767f");

            var bonus_feat_selection = library.CopyAndAdd<Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f", "ForesterBonusFeatSelection", "eaa6fe284ea8461493ad95e406b74e41");
            bonus_feat_selection.SetDescription("At 2nd level, a forester gains one bonus combat feat. She must meet the prerequisites for this feat as normal. She gains an additional bonus combat feat at 7th, 13th, and 19th levels.");

            createForesterTactician();
            createBreathOfLife();

            var forester_animal_focus_additional_use = library.CopyAndAdd<BlueprintFeature>(animal_focus_additional_use, "ForesterAnimalFocusFeature", "");
            
            forester_animal_focus_additional_use.SetNameDescription("Additional Animal Focus", "The forester can apply additional animal focus to herself.");
            forester_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, forester_animal_focus_additional_use),
                                                                Helpers.LevelEntry(2, bonus_feat_selection),
                                                                Helpers.LevelEntry(3, forester_tactician),
                                                                Helpers.LevelEntry(4, evasion),
                                                                Helpers.LevelEntry(5, forester_favored_terrain_selection),
                                                                Helpers.LevelEntry(6, forester_favored_enemy_selection),
                                                                Helpers.LevelEntry(7, camouflage, bonus_feat_selection),
                                                                Helpers.LevelEntry(9, forester_favored_terrain_selection, forester_improved_favored_terrain_selection),
                                                                Helpers.LevelEntry(10, forester_favored_enemy_selection, forester_breath_of_life),
                                                                Helpers.LevelEntry(11, improved_evasion),
                                                                Helpers.LevelEntry(13, forester_favored_terrain_selection, bonus_feat_selection, forester_improved_favored_terrain_selection),
                                                                Helpers.LevelEntry(14, forester_favored_enemy_selection),
                                                                Helpers.LevelEntry(17, forester_favored_terrain_selection, forester_improved_favored_terrain_selection),
                                                                Helpers.LevelEntry(18, forester_favored_enemy_selection),
                                                                Helpers.LevelEntry(19, bonus_feat_selection)
                                                             };


           hunter_progression.UIGroups = hunter_progression.UIGroups.AddToArray(Helpers.CreateUIGroups(bonus_feat_selection, bonus_feat_selection, bonus_feat_selection, bonus_feat_selection));
           hunter_progression.UIGroups = hunter_progression.UIGroups.AddToArray(Helpers.CreateUIGroups(forester_favored_terrain_selection, forester_favored_terrain_selection, forester_favored_terrain_selection, forester_favored_terrain_selection));
           hunter_progression.UIGroups = hunter_progression.UIGroups.AddToArray(Helpers.CreateUIGroups(forester_improved_favored_terrain_selection, forester_improved_favored_terrain_selection, forester_improved_favored_terrain_selection, forester_improved_favored_terrain_selection));
           hunter_progression.UIGroups = hunter_progression.UIGroups.AddToArray(Helpers.CreateUIGroups(forester_animal_focus_additional_use, forester_tactician, forester_breath_of_life, evasion, camouflage, improved_evasion));
        }


        static void createForesterTactician()
        {
            tactician_ability = library.CopyAndAdd<BlueprintAbility>("00af3b5f43aa7ae4c87bcfe4e129f6e8", "ForesterTacticianAbility", "d63901c064b146eaa9a0bc4144e26f29"); //vanguard tactician
            tactician_ability.SetName("Tactician");
            tactician_ability.SetDescription("At 3rd level as a standard action, a forester can grant the benefits of one teamwork feat to all allies within 30 feet who can see and hear her. Allies retain the use of this bonus feat for 3 rounds plus 1 round for every 2 levels the forester has. Allies do not need to meet the prerequisites of this bonus feat. The forester can use this ability once per day at 3rd level, plus one additional time per day at 7th level and every 5 levels thereafter.");

            var tactician_resource = Helpers.Create<BlueprintAbilityResource>();
            tactician_resource.name = "ForesterTacticianResource";
            tactician_resource.SetIncreasedByLevelStartPlusDivStep(1, 2, 0, 5, 1, 0, 0.0f, new BlueprintCharacterClass[] { hunter_class });
            library.AddAsset(tactician_resource, "46f1e4647ab948a0b12accc0e23e6849");

            tactician_ability.ReplaceComponent<AbilityResourceLogic>(Helpers.CreateResourceLogic(tactician_resource));

            var abilities = tactician_ability.Variants;
            var new_abilities = new List<BlueprintAbility>();

            foreach (var a in abilities)
            {
                var new_ability = library.CopyAndAdd(a, a.name.Replace("Vanguard", "Forester"), "");
                new_ability.ReplaceComponent<AbilityResourceLogic>(Helpers.CreateResourceLogic(tactician_resource));
                new_ability.Parent = tactician_ability;
                new_ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                    progression: ContextRankProgression.StartPlusDivStep,
                                                                                                    startLevel: -1,
                                                                                                    stepLevel: 2,
                                                                                                    classes: new BlueprintCharacterClass[] { hunter_class }
                                                                                                    )
                                                                    );

                var buff = Common.extractActions<ContextActionApplyBuff>(new_ability.GetComponent<AbilityEffectRunAction>().Actions.Actions).FirstOrDefault().Buff;
                var teamwork_feat_name = buff.GetComponent<AddFactsFromCaster>().Facts[0].Name;
                new_ability.SetName("Tactician — " + teamwork_feat_name);
                new_abilities.Add(new_ability);
                //change buffs to pick name from parent ability
                Common.extractActions<ContextActionApplyBuff>(a.GetComponent<AbilityEffectRunAction>().Actions.Actions).FirstOrDefault().Buff.SetName("");
            }

            tactician_ability.ReplaceComponent<AbilityVariants>(a => a.Variants = new_abilities.ToArray());

            forester_tactician = Helpers.CreateFeature("ForesterTacticianFeature", tactician_ability.Name, tactician_ability.Description,
                                                       "33aaac96f43e4077aca97f59eaf4b724",
                                                       tactician_ability.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(tactician_ability),
                                                       Helpers.CreateAddAbilityResource(tactician_resource));
        }



        static void createDivineHunterArchetype()
        {
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            //Since it is a bit tricky to provide favored terran damage bonuses as per RAW
            //I decided to replace this feature with 4 favored enemies at levels 6, 10, 14 and 18 (without rank increases).
            divine_hunter_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DivineHunterHunterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Divine Hunter");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While most hunters heed the call of nature and fight to protect its bounty, some are inspired to serve a higher power. These divine hunters use faith to aid them in their struggles, and their faith infuses their animal companions, making these companions champions of their deities.\n"
                                                              + "Alignment: A divine hunter’s alignment must be within one step of her deity’s, along either the law / chaos axis or the good / evil axis. A divine hunter can otherwise be of any alignment.");

            });
            Helpers.SetField(divine_hunter_archetype, "m_ParentClass", hunter_class);
            library.AddAsset(divine_hunter_archetype, "bd650995013f4cb2b98b014b0639a46c");

            divine_hunter_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(3, hunter_tactics, hunter_teamwork_feat),
                                                                        Helpers.LevelEntry(6, hunter_teamwork_feat),
                                                                        Helpers.LevelEntry(9, hunter_teamwork_feat),
                                                                        Helpers.LevelEntry(12, hunter_teamwork_feat),
                                                                        Helpers.LevelEntry(15, hunter_teamwork_feat),
                                                                        Helpers.LevelEntry(18, hunter_teamwork_feat)
                                                                       };
            var diety_selection = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            var domain_selection = library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");
            //add divine_hunter to all domains
            //this will make hunter receive all domain bonuses starting from level 1 which will be a bit stronger than pnp version, but way simpler to implement
            ClassToProgression.addClassToDomains(hunter_class, new BlueprintArchetype[] { divine_hunter_archetype }, ClassToProgression.DomainSpellsType.NormalList, domain_selection, cleric);
            createOtherWordlyCompanion();

            divine_hunter_archetype.ClassSkills = hunter_class.ClassSkills.AddToArray(StatType.SkillLoreReligion);
            divine_hunter_archetype.ReplaceClassSkills = true;
            divine_hunter_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, diety_selection, domain_selection),
                                                                     Helpers.LevelEntry(3, hunter_otherwordly_companion)
                                                                   };
            hunter_progression.UIDeterminatorsGroup = hunter_progression.UIDeterminatorsGroup.AddToArray(diety_selection, domain_selection);

            var animal_domain = library.Get<BlueprintProgression>("23d2f87aa54c89f418e68e790dba11e0");
            animal_domain.AddComponent(Common.prerequisiteNoArchetype(hunter_class, divine_hunter_archetype));

            divine_hunter_archetype.AddComponent(Helpers.PrerequisiteNoFeature(library.Get<BlueprintFeature>("92c0d2da0a836ce418a267093c09ca54")));//no atheism
        }


        static void createOtherWordlyCompanion()
        {
            createSmiteGoodEvilAC();

            var celestial_bloodline = library.Get<Kingmaker.Blueprints.Classes.BlueprintProgression>("aa79c65fa0e11464d9d100b038c50796");
            var abbysal_bloodline = library.Get<Kingmaker.Blueprints.Classes.BlueprintProgression>("d3a4cb7be97a6694290f0dcfbd147113");

            var demonic_might = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("5c1c2ed7fe5f99649ab00605610b775b");
            var damage_resistance = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("8cbf303d479cf0d42a8e36092c76fa7c");
            var aura_of_heaven = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("2768c719ee7338c49932358c2c581bba");

            var ac_dr_evil = Helpers.CreateFeature("AnimalCompanionCelestialDRFeature",
                                                "Celestial Damage Reduction",
                                                "Animal Companion gains damage reduction 5/Evil at level 5. It increases to damage reduction 10/Evil at level 11.",
                                                "368bc4311f7f4ba9af3752ff4418d0a8",
                                                aura_of_heaven.Icon,
                                                FeatureGroup.None,
                                                Common.createAlignmentDRContextRank(DamageAlignment.Evil),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel,
                                                                                ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                                                customProgression: new (int, int)[] {
                                                                                    (4, 0),
                                                                                    (10, 5),
                                                                                    (20, 10)
                                                                                })
                                                );



            var ac_dr_good = Helpers.CreateFeature("AnimalCompanionFiendishDRFeature",
                                               "Fiendish Damage Reduction",
                                               "Animal Companion gains damage reduction 5/Good at level 5. It increases to damage reduction 10/Good at level 11.",
                                               "a203d617f8d547459e1f25790f886b6e",
                                               aura_of_heaven.Icon,
                                               FeatureGroup.None,
                                               Common.createAlignmentDRContextRank(DamageAlignment.Good),
                                               Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel,
                                                                                ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                                                customProgression: new (int, int)[] {
                                                                                    (4, 0),
                                                                                    (10, 5),
                                                                                    (20, 10)
                                                                                })
                                              );


            var ac_dr_law = Helpers.CreateFeature("AnimalCompanionEntropicDRFeature",
                                   "Entropic Damage Reduction",
                                   "Animal Companion gains damage reduction 5/Lawful at level 5. It increases to damage reduction 10/Lawful at level 11.",
                                   "",
                                   aura_of_heaven.Icon,
                                   FeatureGroup.None,
                                   Common.createAlignmentDRContextRank(DamageAlignment.Lawful),
                                   Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel,
                                                                    ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                                    customProgression: new (int, int)[] {
                                                                                    (4, 0),
                                                                                    (10, 5),
                                                                                    (20, 10)
                                                                    })
                                  );

            var ac_dr_chaos = Helpers.CreateFeature("AnimalCompanionResoluteDRFeature",
                                                   "Resolute Damage Reduction",
                                                   "Animal Companion gains damage reduction 5/Chaotic at level 5. It increases to damage reduction 10/Chaotic at level 11.",
                                                   "",
                                                   aura_of_heaven.Icon,
                                                   FeatureGroup.None,
                                                   Common.createAlignmentDRContextRank(DamageAlignment.Chaotic),
                                                   Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel,
                                                                                    ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                                                    customProgression: new (int, int)[] {
                                                                                                                (4, 0),
                                                                                                                (10, 5),
                                                                                                                (20, 10)
                                                                                    })
                                                  );

            var ac_resist_cae = Helpers.CreateFeature("AnimalCompanionCelestialResistFeature",
                        "Celestial Resistance",
                        "Animal commanpanion gains resist acid 5, resist cold 5 and resist electricity 5. At 5th level these resistances increase to 10, at 11th level to 15.",
                        "46a19a521e0d40f792d8b4f64931be8a",
                        damage_resistance.Icon,
                        FeatureGroup.None,
                        Common.createEnergyDRContextRank(DamageEnergyType.Acid),
                        Common.createEnergyDRContextRank(DamageEnergyType.Cold),
                        Common.createEnergyDRContextRank(DamageEnergyType.Electricity),
                        Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel,
                                ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                customProgression: new (int, int)[] {
                                    (4, 5),
                                    (10, 10),
                                    (20, 15)
                                })
                        );


            var ac_resist_af = Helpers.CreateFeature("AnimalCompanionEntropicResistFeature",
                                                    "Entropic Resistance",
                                                    "Animal commanpanion gains resist acid 5, and resist fire 5. At 5th level these resistances increase to 10, at 11th level to 15.",
                                                    "",
                                                    damage_resistance.Icon,
                                                    FeatureGroup.None,
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Acid),
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Fire),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel,
                                                            ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                            customProgression: new (int, int)[] {
                                                                            (4, 5),
                                                                            (10, 10),
                                                                            (20, 15)
                                                            })
                                                    );


            var ac_resist_acf = Helpers.CreateFeature("AnimalCompanionResoluteResistFeature",
                                        "Resolute Resistance",
                                        "Animal commanpanion gains resist acid 5, resist cold 5, and resist fire 5. At 5th level these resistances increase to 10, at 11th level to 15.",
                                        "",
                                        damage_resistance.Icon,
                                        FeatureGroup.None,
                                        Common.createEnergyDRContextRank(DamageEnergyType.Acid),
                                        Common.createEnergyDRContextRank(DamageEnergyType.Fire),
                                        Common.createEnergyDRContextRank(DamageEnergyType.Cold),
                                        Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel,
                                                ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                customProgression: new (int, int)[] {
                                                                            (4, 5),
                                                                            (10, 10),
                                                                            (20, 15)
                                                })
                                        );


            var ac_resist_cf = Helpers.CreateFeature("AnimalCompanionFiendishResistFeature",
                        "Fiendish Resistance",
                        "Animal commanpanion gains reist resist cold 5 and resist fire 5. At 5th level these resistances increase to 10, at 11th level to 15.",
                        "4170f7f5874a4e45bc7050a53727452f",
                        damage_resistance.Icon,
                        FeatureGroup.None,
                        Common.createEnergyDRContextRank(DamageEnergyType.Fire),
                        Common.createEnergyDRContextRank(DamageEnergyType.Cold),
                        Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel,
                        ContextRankProgression.Custom, AbilityRankType.StatBonus,
                        customProgression: new (int, int)[] {
                            (4, 5),
                            (10, 10),
                            (20, 15)
                        })
                        );


            var spell_resistance = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889");
            var ac_spell_resistance = Helpers.CreateFeature("AnimalCompanionSpellResistanceFeature",
                                                            "Spell Resistance",
                                                            "Animal Companion gains spell resistance equal to its level + 6.",
                                                            "0e7481a8ceb041129a692bf59f24d057",
                                                            spell_resistance.Icon,
                                                            FeatureGroup.None,
                                                            Helpers.Create<AddSpellResistance>(s => s.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CharacterLevel, progression: ContextRankProgression.BonusValue,
                                                                                            type: AbilityRankType.StatBonus, stepLevel: 6)
                                                               );

            celestial_template = Helpers.CreateFeature("CelestialTemplateFeauture",
                                                  "Celestial Template",
                                                  "Celestial creatures dwell in the higher planes, but can be summoned using spells such as summon monster and planar ally. Celestial creatures may use Smite Evil once per day, gain energy resistance 5 to acid, cold and fire, which increases to 10 at level 5 and to 15 at level 11. They also gain spell resistance equal to their level + 6. Starting from level 5 they also gain damage reduction 5/Evil which further increases to  10/Evil at level 11.",
                                                  "69f0d7d1077f492f8237952f8219a270",
                                                  celestial_bloodline.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFacts(ac_smite_evil_feature, ac_spell_resistance, ac_resist_cae, ac_dr_evil));

            fiendish_template = Helpers.CreateFeature("FiendishTemplateFeature",
                                                   "Fiendish Template",
                                                   "Creatures with the fiendish template live in the Lower Planes, such as the Abyss and Hell, but can be summoned using spells such as summon monster and planar ally. Fiendish creatures may use Smite Good once per day, gain energy resistance 5 to cold and fire, which increases to 10 at level 5 and to 15 at level 11. They also gain spell resistance equal to their level + 6. Starting from level 5 they also gain damage reduction 5/Good which further increases to  10/Good at level 11.",
                                                   "3e33af2ab5974859bdaa92c32987b3e0",
                                                   abbysal_bloodline.Icon,
                                                   FeatureGroup.None,
                                                   Helpers.CreateAddFacts(ac_smite_good_feature, ac_spell_resistance, ac_resist_cf, ac_dr_good)
                                                  );

            resolute_template = Helpers.CreateFeature("ResoluteTemplateFeature",
                                       "Resolute Template",
                                       "Creatures with the resolute template live in planes where law is paramount. They can be summoned using spells such as summon monster and planar ally. Resolute creatures may use Smite Chaos once per day, gain energy resistance 5 to acid, cold and fire, which increases to 10 at level 5 and to 15 at level 11. They also gain spell resistance equal to their level + 6. Starting from level 5 they also gain damage reduction 5/Chaotic which further increases to  10/Chaotic at level 11.",
                                       "",
                                       Helpers.GetIcon("ddffa896d4605a44f95baa6d0d350828"), //smite justice
                                       FeatureGroup.None,
                                       Helpers.CreateAddFacts(ac_smite_chaos_feature, ac_spell_resistance, ac_resist_acf, ac_dr_chaos)
                                      );

            entropic_template = Helpers.CreateFeature("EntropicTemplateFeature",
                                                       "Entropic Template",
                                                       "Creatures with the entropic template live in planes where chaos is paramount. They can be summoned using spells such as summon monster and planar ally. Entropic creatures may use Smite Law once per day, gain energy resistance 5 to acid and fire, which increases to 10 at level 5 and to 15 at level 11. They also gain spell resistance equal to their level + 6. Starting from level 5 they also gain damage reduction 5/Lawful which further increases to  10/Lawful at level 11.",
                                                       "",
                                                       Helpers.GetIcon("ddffa896d4605a44f95baa6d0d350828"), //smite justice
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFacts(ac_smite_law_feature, ac_spell_resistance, ac_resist_af, ac_dr_law)
                                                      );


            hunter_otherwordly_companion = Helpers.CreateFeatureSelection("AnimalCompanionTemplateSelection",
                                           "Otherworldly Companion",
                                           "At 3rd level, a hunter’s companion takes on otherworldly features. If the divine hunter worships a good deity, the animal companion gains the celestial template. If the hunter worships an evil deity, the animal companion gains the fiendish template. If the hunter worships a lawful deity, the animal companion gains the resolute template. If the hunter worships a chotic deity, the animal companion gains the chaotic template.  If the hunter worships a neutral deity, she can choose any template; once this choice is made, it cannot be changed.",
                                           "1936995e234b4d2e8dbddc935e731254",
                                           null,
                                           FeatureGroup.None,
                                           Helpers.Create<NoSelectionIfAlreadyHasFeature>(n => { n.AnyFeatureFromSelection = true; n.Features = new BlueprintFeature[0]; })
                                           );

            var evil_allowed = library.Get<BlueprintFeature>("351235ac5fc2b7e47801f63d117b656c");
            var good_allowed = library.Get<BlueprintFeature>("882521af8012fc749930b03dc18a69de");
            var chaos_allowed = library.Get<BlueprintFeature>("8c7d778bc39fec642befc1435b00f613");
            var law_allowed = library.Get<BlueprintFeature>("092714336606cfc45a37d2ab39fabfa8");

            var prereq_neutral = Helpers.Create<PrerequisiteMechanics.PrerequisiteNoFeatures>(p =>
            {
                p.Features = new BlueprintFeature[]
                {
                   evil_allowed, good_allowed, law_allowed, chaos_allowed
                };
                p.Group = Prerequisite.GroupType.Any;
            }
            );

            var channel_positive_allowed = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("8c769102f3996684fb6e09a2c4e7e5b9");
            var channel_negative_allowed = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("dab5255d809f77c4395afc2b713e9cd6");

            hunter_otherwordly_companion.AllFeatures = new BlueprintFeature[] {Helpers.CreateFeature("CelestialCompanionTemplateFeature",
                                                                                                      "Celestial Companion",
                                                                                                      celestial_template.Description,
                                                                                                      "4eff84c8f4a740b28f18587cdeb0c41d",
                                                                                                      celestial_template.Icon,
                                                                                                      FeatureGroup.None,
                                                                                                      Common.createAddFeatToAnimalCompanion(celestial_template),
                                                                                                      Helpers.PrerequisiteFeature(good_allowed, any: true),
                                                                                                      prereq_neutral
                                                                                                      ),
                                                                                Helpers.CreateFeature("FiendishCompanionTemplateFeature",
                                                                                                        "Fiendish Companion",
                                                                                                        fiendish_template.Description,
                                                                                                        "76784350237247aab40ebdcc6107794d",
                                                                                                        fiendish_template.Icon,
                                                                                                        FeatureGroup.None,
                                                                                                        Common.createAddFeatToAnimalCompanion(fiendish_template),
                                                                                                        Helpers.PrerequisiteFeature(evil_allowed, any: true),
                                                                                                        prereq_neutral
                                                                                                        ),
                                                                                Helpers.CreateFeature("ResoluteCompanionTemplateFeature",
                                                                                                        "Resolute Companion",
                                                                                                        resolute_template.Description,
                                                                                                        "",
                                                                                                        resolute_template.Icon,
                                                                                                        FeatureGroup.None,
                                                                                                        Common.createAddFeatToAnimalCompanion(resolute_template),
                                                                                                        Helpers.PrerequisiteFeature(law_allowed, any: true),
                                                                                                        prereq_neutral
                                                                                                        ),
                                                                               Helpers.CreateFeature("EntropicCompanionTemplateFeature",
                                                                                                        "Entropic Companion",
                                                                                                        entropic_template.Description,
                                                                                                        "",
                                                                                                        entropic_template.Icon,
                                                                                                        FeatureGroup.None,
                                                                                                        Common.createAddFeatToAnimalCompanion(entropic_template),
                                                                                                        Helpers.PrerequisiteFeature(chaos_allowed, any: true),
                                                                                                        prereq_neutral
                                                                                                        )
                                                                                };
        }


        static void createSmiteGoodEvilAC()
        {
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CharacterLevel);
            var smite_evil = library.Get<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec");

            ac_smite_evil_feature = Common.createSmite("SmiteEvilAC",
                                                       "Smite Evil",
                                                       "A character can call out to the powers of good to aid her in her struggle against evil. As a swift action, the character chooses one target within sight to smite. If this target is evil, the character adds her Charisma bonus (if any) to her attack rolls and adds her character level to all damage rolls made against the target of her smite, smite evil attacks automatically bypass any DR the creature might possess.\nIn addition, while smite evil is in effect, the character gains a deflection bonus equal to her Charisma bonus (if any) to her AC against attacks made by the target of the smite. If the character targets a creature that is not evil, the smite is wasted with no effect. The smite evil lasts until the target dies or the character selects a new target.",
                                                       "bf0882a6d254407bb259356f1aa66392",
                                                       "f009c072167c4b53a37c1071a2251c3f",
                                                       smite_evil.Icon,
                                                       context_rank_config,
                                                       AlignmentComponent.Evil);

            ac_smite_good_feature = Common.createSmite("SmiteGoodAC",
                                           "Smite Good",
                                           "A character can call out to the powers of evil to aid her in her struggle against good. As a swift action, the character chooses one target within sight to smite. If this target is good, the character adds her Cha bonus (if any) to her attack rolls and adds her class level to all damage rolls made against the target of her smite, smite good attacks automatically bypass any DR the creature might possess.\nIn addition, while smite good is in effect, the character gains a deflection bonus equal to her Charisma modifier (if any) to her AC against attacks made by the target of the smite. If the character targets a creature that is not good, the smite is wasted with no effect.\nThe smite good lasts until the target dies or the character selects a new target.",
                                           "a432066702694b2590260b58426fee28",
                                           "320b92730bd54842b9707931a5dbab18",
                                           LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteGood.png"),
                                           context_rank_config,
                                           AlignmentComponent.Good);


            ac_smite_law_feature = Common.createSmite("SmiteLawAC",
                                                       "Smite Law",
                                                       "A character can call out to the powers of chaos to aid her in her struggle against law. As a swift action, the character chooses one target within sight to smite. If this target is lawful, the character adds her Cha bonus (if any) to her attack rolls and adds her class level to all damage rolls made against the target of her smite, smite law attacks automatically bypass any DR the creature might possess.\nIn addition, while smite law is in effect, the character gains a deflection bonus equal to her Charisma modifier (if any) to her AC against attacks made by the target of the smite. If the character targets a creature that is not lawful, the smite is wasted with no effect.\nThe smite law lasts until the target dies or the character selects a new target.",
                                                       "",
                                                       "",
                                                       LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteNature.png"),
                                                       context_rank_config,
                                                       AlignmentComponent.Lawful);

            ac_smite_chaos_feature = Common.createSmite("SmiteChaosAC",
                                                       "Smite Chaos",
                                                       "A character can call out to the powers of law to aid her in her struggle against chaos. As a swift action, the character chooses one target within sight to smite. If this target is chaotic, the character adds her Cha bonus (if any) to her attack rolls and adds her class level to all damage rolls made against the target of her smite, smite chaos attacks automatically bypass any DR the creature might possess.\nIn addition, while smite chaos is in effect, the character gains a deflection bonus equal to her Charisma modifier (if any) to her AC against attacks made by the target of the smite. If the character targets a creature that is not chaotic, the smite is wasted with no effect.\nThe smite chaos lasts until the target dies or the character selects a new target.",
                                                       "",
                                                       "",
                                                       LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteImpudence.png"),
                                                       context_rank_config,
                                                       AlignmentComponent.Chaotic);
        }


        static BlueprintSpellbook createHunterSpellbook()
        {
            var ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
 
            var hunter_spellbook = Helpers.Create<BlueprintSpellbook>();
            hunter_spellbook.name = "HunterSpellbook";
            library.AddAsset(hunter_spellbook, "e1e06d8905884a1faaadd77a3cb5f87a");
            hunter_spellbook.Name = hunter_class.LocalizedName;
            hunter_spellbook.SpellsPerDay = inquisitor_class.Spellbook.SpellsPerDay;
            hunter_spellbook.SpellsKnown = inquisitor_class.Spellbook.SpellsKnown;
            hunter_spellbook.Spontaneous = true;
            hunter_spellbook.IsArcane = false;
            hunter_spellbook.AllSpellsKnown = false;
            hunter_spellbook.CanCopyScrolls = false;
            hunter_spellbook.CastingAttribute = StatType.Wisdom;
            hunter_spellbook.CharacterClass = hunter_class;
            hunter_spellbook.CasterLevelModifier = 0;
            hunter_spellbook.CantripsType = CantripsType.Orisions;
            //hunter knows all spells of ranger and 1-6 level spells of druid
            hunter_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            hunter_spellbook.SpellList.name = "HunterSpellList";
            library.AddAsset(hunter_spellbook.SpellList, "b161506e0b8f4116806a243f6838ae01");
            hunter_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < hunter_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                hunter_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }
            hunter_spellbook.SpellList.SpellsByLevel[0].SpellLevel = 0;
            /* hunter_spellbook.SpellList = library.CopyAndAdd<BlueprintSpellList>("29f3c338532390546bc5347826a655c4", //ranger spelllist
                                                                              "HunterSpellList",
                                                                              "3f0cbe75afe142478facc64fef816b28");*/
            //add ranger spells      
            foreach (var spell_level_list in ranger_class.Spellbook.SpellList.SpellsByLevel)
            {
                int sp_level = spell_level_list.SpellLevel;              
                foreach (var spell in spell_level_list.Spells)
                {
                    if (!spell.IsInSpellList(hunter_spellbook.SpellList))
                    {
                        ExtensionMethods.AddToSpellList(spell, hunter_spellbook.SpellList, sp_level);
                    }
                }
            }
            //add druid spells      
            foreach (var spell_level_list in druid_class.Spellbook.SpellList.SpellsByLevel)
            {
                int sp_level = spell_level_list.SpellLevel;
                if (sp_level > 6)
                {
                    continue;
                }
                foreach (var spell in spell_level_list.Spells)
                {
                    if (!spell.IsInSpellList(hunter_spellbook.SpellList))
                    {
                        ExtensionMethods.AddToSpellList(spell, hunter_spellbook.SpellList, sp_level);
                    }
                }
            }
           
            return hunter_spellbook;
        }


        static void createHunterProgression()
        {
            hunter_progression = Helpers.CreateProgression("HunterProgression",
                                                   hunter_class.Name,
                                                   hunter_class.Description,
                                                   "110347af180c477982894a74885466a4",
                                                   hunter_class.Icon,
                                                   FeatureGroup.None);
            hunter_progression.Classes = new BlueprintCharacterClass[1] { hunter_class };

            var hunter_orisons = library.CopyAndAdd<BlueprintFeature>("f2ed91cc202bd344691eef91eb6d5d1a", //druid orisons
                                                                       "HunterOrisionsFeature",
                                                                       "5838ceedcf344dfe8d3e0538c67a7884");
            hunter_orisons.SetDescription("Hunters learn a number of orisons, or 0 - level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            hunter_orisons.ReplaceComponent<BindAbilitiesToClass>(c => {c.CharacterClass = hunter_class;});
            hunter_orisons.RemoveComponents<AddFeatureOnClassLevel>();

            var hunter_proficiencies = library.CopyAndAdd<BlueprintFeature>("c5e479367d07d62428f2fe92f39c0341",
                                                                            "HunterProficiencies",
                                                                            "e92350a79aa84304a4d2837e4a248537");
            hunter_proficiencies.SetName("Hunter Proficiencies");
            hunter_proficiencies.SetDescription("A hunter is proficient with all simple and martial weapons and with light armor, medium armor, and shields (except tower shields)");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            hunter_tactics = library.CopyAndAdd<BlueprintFeature>("e1f437048db80164792155102375b62c",
                                                                      "HunterTactics",
                                                                      "3bd652e743f346ccbc25e298954e9805");
            hunter_tactics.SetDescription("At 3rd level, a hunter automatically grants her teamwork feats to her animal companion. The companion doesn't need to meet the prerequisites of these teamwork feats.");

            hunter_teamwork_feat = library.CopyAndAdd<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb",
                                                                              "HunterTeamworkFeat",
                                                                              "93a91b20e57845b19804de9c57e28bb3");
            hunter_teamwork_feat.SetDescription("At 3rd level, and every three levels thereafter, the hunter gains a bonus feat in addition to those gained from normal advancement. These bonus feats must be selected from those listed as teamwork feats. The hunter must meet the prerequisites of the selected bonus feat.");
 
            var bonus_hunter_spells = createFreeSummonNatureAllySpells();
            hunter_animal_companion = createHunterAnimalCompanion();

            hunter_woodland_stride = library.CopyAndAdd<BlueprintFeature>("11f4072ea766a5840a46e6660894527d",
                                                                         "HunterWooldlandStride",
                                                                         "07f67ae4a1614ca6b0d09df6a317630c");
            hunter_woodland_stride.SetDescription("At 5th level, you and your animal companion can move through any sort difficult terrain at your normal speed and without taking damage or suffering any other impairment.");
            hunter_woodland_stride.AddComponent(Common.createAddFeatToAnimalCompanion(hunter_woodland_stride));
            var entries = new List<LevelEntry>();
            entries.Add(Helpers.LevelEntry(1, hunter_proficiencies, hunter_orisons, detect_magic, bonus_hunter_spells, hunter_animal_companion, animal_focus, animal_focus_ac,
                                                           library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee"), //inside the storm
                                                           library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                           library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")  // touch calculate feature
                                                           )) ;

            createPreciseCompanion();
            createTrickSelection();
            createRaiseAnimalCompanion();
            entries.Add(Helpers.LevelEntry(2, precise_companion));
            entries.Add(Helpers.LevelEntry(3, hunter_tactics, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(4));
            entries.Add(Helpers.LevelEntry(5, hunter_woodland_stride));
            entries.Add(Helpers.LevelEntry(6, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(7, trick_selection));
            entries.Add(Helpers.LevelEntry(8, animal_focus_additional_use, animal_focus_additional_use_ac));
            entries.Add(Helpers.LevelEntry(9, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(10, raise_animal_companion));
            entries.Add(Helpers.LevelEntry(11));
            entries.Add(Helpers.LevelEntry(12, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(13, trick_selection));
            entries.Add(Helpers.LevelEntry(14));
            entries.Add(Helpers.LevelEntry(15, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(16));
            entries.Add(Helpers.LevelEntry(17));
            entries.Add(Helpers.LevelEntry(18, hunter_teamwork_feat));
            entries.Add(Helpers.LevelEntry(19, trick_selection));
            entries.Add(Helpers.LevelEntry(20, animal_focus_additional_use2, animal_focus_additional_use_ac2));
            hunter_progression.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(precise_companion, hunter_teamwork_feat, hunter_teamwork_feat, hunter_teamwork_feat, 
                                                                                    hunter_teamwork_feat, hunter_teamwork_feat, hunter_teamwork_feat),
                                                          Helpers.CreateUIGroup(animal_focus, animal_focus_additional_use, animal_focus_additional_use2),
                                                          Helpers.CreateUIGroup(animal_focus_ac, animal_focus_additional_use_ac, animal_focus_additional_use_ac2),
                                                          Helpers.CreateUIGroup(bonus_hunter_spells, hunter_tactics, hunter_woodland_stride, raise_animal_companion),
                                                          Helpers.CreateUIGroup(trick_selection, trick_selection, trick_selection)};
            hunter_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { hunter_animal_companion, hunter_proficiencies, hunter_orisons, detect_magic};
            hunter_progression.LevelEntries = entries.ToArray();

        }


        static void createRaiseAnimalCompanion()
        {
            raise_companion_resource = Helpers.CreateAbilityResource("HunterRaiseAnimalCompanionResource", "", "", "", null);
            raise_companion_resource.SetFixedResource(1);
            var spell = library.CopyAndAdd<BlueprintAbility>("9288a1e0a4704b54984fd8155de38d4f", "HunterRaiseAnimalCompanion", "");
            spell.RemoveComponents<AbilityTargetIsDeadCompanion>();
            spell.AddComponent(Helpers.Create<NewMechanics.AbilityCasterCompanionDead>());
            spell.AddComponent(Helpers.Create<CompanionMechanics.AbilityCasterCompanionUnsummoned>(a => a.not = true));
            spell.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(Helpers.Create<ContextActionsOnPet>(a => a.Actions = spell.GetComponent<AbilityEffectRunAction>().Actions)));
            spell.ReplaceComponent<AbilityTargetIsDeadCompanion>(Helpers.Create<NewMechanics.AbilityTargetIsDead>());
            spell.Range = AbilityRange.Personal;
            spell.setMiscAbilityParametersSelfOnly();
            spell.Type = AbilityType.SpellLike;
            spell.AddComponent(raise_companion_resource.CreateResourceLogic());

            raise_animal_companion = Helpers.CreateFeature("HunterRaiseAnimalCompanionFeature",
                                                           "Raise Animal Companion",
                                                           "At 10th level, a hunter gains raise animal companion as a spell-like ability.",
                                                           "",
                                                           spell.Icon,
                                                           FeatureGroup.None,
                                                           Helpers.CreateAddFact(spell),
                                                           raise_companion_resource.CreateAddAbilityResource()
                                                           );
            //raise_animal_companion.ReapplyOnLevelUp = true;
        }


        static void createBreathOfLife()
        {
            var spell = library.Get<BlueprintAbility>("d5847cad0b0e54c4d82d6c59a3cda6b0");
            
            var ability = Common.convertToSpellLike(spell, "Forester", new BlueprintCharacterClass[] { hunter_class },
                                                     StatType.Wisdom, raise_companion_resource);

            forester_breath_of_life = Helpers.CreateFeature("ForesterBreathOfLifeFeature",
                                                           ability.Name,
                                                           "At 10th level, a forester can cast breath of life once per day as a spell-like ability. ",
                                                           "",
                                                           ability.Icon,
                                                           FeatureGroup.None,
                                                           Helpers.CreateAddFact(ability),
                                                           raise_companion_resource.CreateAddAbilityResource()
                                                           );
        }


        static void createTrickSelection()
        {
            //aiding attack: + 2 bonus on next attack roll
            //distracting attack: -2 on attack rolls for 1 round
            //rattling strike: target is shaken fo 1d4 rounds
            //tangling attack: target entangled for 1 round
            //Upending Strike: trip combat maneuver
            var animal_class = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");
            trick_resource = Helpers.CreateAbilityResource("TrickResource", "", "", "", null);
            trick_resource.SetIncreasedByLevelStartPlusDivStep(0, 0, 0, 2, 1, 0, 0.0f, new BlueprintCharacterClass[] { animal_class});
            trick_resource.SetIncreasedByStat(0, StatType.Wisdom);

            var bewildering_injury = library.Get<BlueprintBuff>("22b1d98502050cb4cbdb3679ac53115e");
            var aiding_attack_target_buff = Helpers.CreateBuff("AidingAttackTargetBuff",
                                                               "Aiding Attack",
                                                               "The character can use this trick as a free action when he hits a creature with an attack. The next ally who makes an attack against the target creature before the start of the character’s next turn gains a + 2 bonus on that attack roll.",
                                                               "",
                                                               bewildering_injury.Icon,
                                                               bewildering_injury.FxOnStart,
                                                               Helpers.Create<AttackBonusAgainstTarget>(a =>
                                                                                                       {
                                                                                                           a.CheckCaster = true;
                                                                                                           a.CheckCasterFriend = true;
                                                                                                           a.Value = Common.createSimpleContextValue(2);
                                                                                                       }
                                                                                                       ),
                                                               Common.createAddTargetAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()),
                                                                                                             Helpers.CreateActionList(),
                                                                                                             only_hit: false,
                                                                                                             not_reach: false,
                                                                                                             only_melee: false,
                                                                                                             wait_for_attack_to_resolve: true
                                                                                                            )
                                                              );

            var aiding_attack = createStrikeTrick("AidingAttack", aiding_attack_target_buff.Name, aiding_attack_target_buff.Description, aiding_attack_target_buff.Icon,
                                                     Common.createContextActionApplyBuff(aiding_attack_target_buff, Helpers.CreateContextDuration(), dispellable: false, duration_seconds: 9)
                                                     );

            var disorientating_injury = library.Get<BlueprintBuff>("1f1e42f8c06d7dc4bb70cc12c73dfb38");
            var distracting_attack_buff = Helpers.CreateBuff("DistractingAttackEffectBuff",
                                                             "Distracting Attack",
                                                             "The character can use this trick as a free action before he makes an attack. If the attack hits, the target takes a –2 penalty on all attack rolls for 1 round.",
                                                             "",
                                                             disorientating_injury.Icon,
                                                             disorientating_injury.FxOnStart,
                                                             Common.createAddGenericStatBonus(-2, ModifierDescriptor.UntypedStackable, StatType.AdditionalAttackBonus)
                                                             );
            var distracting_attack = createStrikeTrick("DistractingAttack", distracting_attack_buff.Name, distracting_attack_buff.Description, distracting_attack_buff.Icon,
                                                        Common.createContextActionApplyBuff(distracting_attack_buff, Helpers.CreateContextDuration(), dispellable: false, duration_seconds: 6)
                                                        );

            var shaken_buff = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");

            var rattling_strike = createStrikeTrick("RattlingStrike", 
                                                    "Rattling Strike",
                                                    "The character can use this trick as a free action before he makes a melee attack. If the attack hits, the target is shaken for 1d4 rounds.",
                                                    shaken_buff.Icon,
                                                    Common.createContextActionApplyBuff(shaken_buff, Helpers.CreateContextDuration(diceType: DiceType.D4, diceCount: Common.createSimpleContextValue(1)), dispellable: false)
                                                    );

            var entangled_buff = library.Get<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38");
            var tangling_attack = createStrikeTrick("TanglingAtttack",
                                        "Tangling Attack",
                                        "The character can use this attack as a free action when he makes an attack. If the attack hits, the target is entangled for 1 round.",
                                        entangled_buff.Icon,
                                        Common.createContextActionApplyBuff(entangled_buff, Helpers.CreateContextDuration(), dispellable: false, duration_seconds: 6)
                                        );

            var grease = library.Get<BlueprintAbility>("95851f6e85fe87d4190675db0419d112");
            var upending_strike = createStrikeTrick("UpendingStrike",
                                                    "Upending Strike",
                                                    "The character can use this trick as a free action just before he makes a melee attack. If the attack hits, he may make a free trip combat maneuver against the target.",
                                                    grease.Icon,
                                                    Helpers.Create<ContextActionCombatManeuver>(c => { c.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.Trip; c.OnSuccess = Helpers.CreateActionList(); })
                                                    );


            trick_selection = Helpers.CreateFeatureSelection("AnimalCompanionTrickSelection",
                                                             "Animal Companion Trick",
                                                             "At 7th level and every 6 levels thereafter, a hunter’s animal companion learns a trick.\n"
                                                             + "An animal companion can use these tricks a number of times per day equal to half its Hit Dice plus its Wisdom modifier",
                                                             "",
                                                             null,
                                                             FeatureGroup.None
                                                             );
            trick_selection.AllFeatures = new BlueprintFeature[] { aiding_attack, distracting_attack, rattling_strike, tangling_attack, upending_strike };


        }


        static BlueprintFeature createStrikeTrick(string name, string display_name,  string description, UnityEngine.Sprite icon, GameAction action)
        {
            var buff = Helpers.CreateBuff(name + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          icon,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(action)),
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()), only_hit: false, on_initiator: true)
                                          );
            var act_ability = Helpers.CreateActivatableAbility(name + "ActivatableAbility",
                                                           display_name,
                                                           description,
                                                           "",
                                                           icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(trick_resource, ResourceSpendType.Attack)
                                                           );
            act_ability.DeactivateImmediately = true;
            act_ability.Group = ActivatableAbilityGroupExtension.HunterTrick.ToActivatableAbilityGroup();

            var feature = Common.ActivatableAbilityToFeature(act_ability);
            feature.AddComponent(Helpers.CreateAddAbilityResource(trick_resource));

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), true, false, true, false);
            var ability = Helpers.CreateAbility(name + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                AbilityType.Extraordinary,
                                                CommandType.Free,
                                                AbilityRange.Personal,
                                                "Next Attack",
                                                Helpers.savingThrowNone,
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateResourceLogic(trick_resource),
                                                Common.createAbilityCasterHasNoFacts(buff)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            feature.ReplaceComponent<AddFacts>(a => a.Facts[0] = ability);

            var give_feature = Helpers.CreateFeature(name + "GiveToAcFeature",
                                                     display_name,
                                                     description,
                                                     "",
                                                     icon,
                                                     FeatureGroup.None,
                                                     Common.createAddFeatToAnimalCompanion(feature)
                                                     );

            return give_feature;
        }


        static void createPreciseCompanion()
        {
            precise_companion = library.CopyAndAdd<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb",
                                                      "HunterPreciseCompanion",
                                                      "7904ae839f71415abc930d4afb37f006");
            precise_companion.SetName("Precise Companion");
            precise_companion.SetDescription("At 2nd level, a hunter chooses either Precise Shot or Outflank as a bonus feat. She does not need to meet the prerequisites for this feat. If she chooses Outflank, she automatically grants this feat to her animal companion as well.");

            var outflank = library.TryGet<Kingmaker.Blueprints.Classes.BlueprintFeature>("422dab7309e1ad343935f33a4d6e9f11");
            var precise_companion_outflank = Helpers.CreateFeature("PreciseCompanionOutflankFeature",
                                                                   outflank.Name,
                                                                   outflank.Description,
                                                                   "789857381caf4d87bb41df1af721a078",
                                                                   outflank.Icon,
                                                                   FeatureGroup.None,
                                                                   Helpers.CreateAddFact(outflank),
                                                                   Common.createAddFeatToAnimalCompanion(outflank));
            precise_companion_outflank.HideInCharacterSheetAndLevelUp = true;
            precise_companion_outflank.HideInUI = true;

            precise_companion.AllFeatures = new BlueprintFeature[2] {library.TryGet< Kingmaker.Blueprints.Classes.BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665"), //precise shot
                                                                           precise_companion_outflank };//outflank
            precise_companion.Features = precise_companion.AllFeatures;
            precise_companion.IgnorePrerequisites = true;
        }


        static BlueprintFeature createFreeSummonNatureAllySpells()
        {
            BlueprintAbility[] summon_nature_ally    = new BlueprintAbility[6]{library.TryGet<BlueprintAbility>("c6147854641924442a3bb736080cfeb6"),
                                                                             library.TryGet<BlueprintAbility>("298148133cdc3fd42889b99c82711986"),
                                                                             library.TryGet<BlueprintAbility>("fdcf7e57ec44f704591f11b45f4acf61"),
                                                                             library.TryGet<BlueprintAbility>("c83db50513abdf74ca103651931fac4b"),
                                                                             library.TryGet<BlueprintAbility>("8f98a22f35ca6684a983363d32e51bfe"),
                                                                             library.TryGet<BlueprintAbility>("55bbce9b3e76d4a4a8c8e0698d29002c")
                                                                            };

            BlueprintComponent[] add_summon_nature_ally = new BlueprintComponent[summon_nature_ally.Length];

            for (int i = 0; i <add_summon_nature_ally.Length; i++)
            {
                add_summon_nature_ally[i] = Helpers.CreateAddKnownSpell(summon_nature_ally[i], hunter_class, i + 1);           
            }


            var free_summon_nature_ally = Helpers.CreateFeature("HunterFreeSummonNatureAllyFeat",
                                                                "Bonus Hunter Spells",
                                                                "In addition to the spells gained by hunters as they gain levels, each hunter also automatically adds all summon nature’s ally spells to her list of spells known. These spells are added as soon as the hunter is capable of casting them.",
                                                                "6fdb0275f34a4a0184a215fed24cb1cd",
                                                                summon_nature_ally[0].Icon,
                                                                FeatureGroup.None,
                                                                add_summon_nature_ally);

            for (int i = 0; i < add_summon_nature_ally.Length; i++)
            {
                summon_nature_ally[i].AddRecommendNoFeature(free_summon_nature_ally);
            }

            return free_summon_nature_ally;
        }


        static Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection createHunterAnimalCompanion()
        {
            var animal_companion_progression = library.CopyAndAdd<BlueprintProgression>("924fb4b659dcb4f4f906404ba694b690",
                                                                          "HunterAnimalCompanionProgression",
                                                                          "d96334e7101f4dc5b1f5666c52bba0a6");
            animal_companion_progression.Classes = new BlueprintCharacterClass[1] { hunter_class};

            var animal_companion_selection = library.CopyAndAdd<BlueprintFeatureSelection>("2995b36659b9ad3408fd26f137ee2c67",
                                                                                            "AnimalCompanionSelectionHunter",
                                                                                            "cf9f8d9910db4beba174f4e2b7c1bb2a");
            animal_companion_selection.SetDescription("At 1st level, a hunter forms a bond with an animal companion. A hunter may begin play with any of the animals available to a druid. This animal is a loyal companion that accompanies the hunter on her adventures. This ability functions like the druid animal companion ability (which is part of the nature bond class feature). The hunter’s effective druid level is equal to her hunter level. If a character receives an animal companion from more than one source, her effective druid levels stack for the purposes of determining the companion’s statistics and abilities.");
            var add_progression = Helpers.Create<AddFeatureOnApply>();
            add_progression.Feature = animal_companion_progression;
            animal_companion_selection.ComponentsArray[0] = add_progression;


            return animal_companion_selection;
        }


        static void createAnimalFocusFeat()
        {
            var wildshape_wolf = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintFeature>("19bb148cb92db224abb431642d10efeb");
            var acid_maw = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67");
            var animal_focus_additional_use_component = Helpers.Create<Kingmaker.UnitLogic.FactLogic.IncreaseActivatableAbilityGroupSize>();
            animal_focus_additional_use_component.Group = AnimalFocusGroup;

            animal_focus_additional_use = Helpers.CreateFeature("AdditionalAnimalFocusFeature",
                                                             "Second Animal Focus",
                                                             "The character can apply additional animal focus to herself.",
                                                             "896f036314b049bfa723b74b0e509a89",
                                                             wildshape_wolf.Icon,
                                                             FeatureGroup.None,
                                                             animal_focus_additional_use_component
                                                            );
            animal_focus_additional_use.Ranks = 1;
            animal_focus_additional_use2 = library.CopyAndAdd<BlueprintFeature>(animal_focus_additional_use.AssetGuid, "AdditionalAnimalFocus2Feature", "74e98c7274754ab98c9dc698e7f37e0e");
            animal_focus_additional_use2.SetName("Third Animal Focus");

            animal_focus = animal_focus_engine.createAnimalFocus();

            animal_focus_ac = Helpers.CreateFeature("AnimalFocusAc",
                                                        "Animal Focus (Animal Companion)",
                                                        "The character can apply animal focus to her animal companion.",
                                                        "5eea1e98d11c4acbafc1f9b4abf6cae6",
                                                        acid_maw.Icon,
                                                        FeatureGroup.None,
                                                        Common.createAddFeatToAnimalCompanion(animal_focus)
                                                        );
            animal_focus_additional_use_ac = Helpers.CreateFeature("AdditonalAnimalFocusAc",
                                                        "Second Animal Focus (Animal Companion)",
                                                        "The character can apply one more animal focus to her animal companion.",
                                                        "06bd293935354563be67cb5d2679a9bf",
                                                        acid_maw.Icon,
                                                        FeatureGroup.None,
                                                        Common.createAddFeatToAnimalCompanion(animal_focus_additional_use)
                                                        );

            animal_focus_additional_use_ac2 = Helpers.CreateFeature("AdditonalAnimalFocusAc2",
                                            "Third Animal Focus (Animal Companion)",
                                            "The character can apply one more animal focus to her animal companion.",
                                            "db9c791a010f4401be344fe627b0a9f5",
                                            acid_maw.Icon,
                                            FeatureGroup.None,
                                            Common.createAddFeatToAnimalCompanion(animal_focus_additional_use2)
                                            );


        }


        internal static void addAnimalFocusSH()
        {
            Kingmaker.Blueprints.Classes.LevelEntry initial_entry = new Kingmaker.Blueprints.Classes.LevelEntry();
            Kingmaker.Blueprints.Classes.LevelEntry add_animal_focus_use_entry = new Kingmaker.Blueprints.Classes.LevelEntry();
           
            initial_entry.Level = 4;
            initial_entry.Features.Add(animal_focus);
            initial_entry.Features.Add(animal_focus_ac);
            add_animal_focus_use_entry.Level = 17;
            add_animal_focus_use_entry.Features.Add(animal_focus_additional_use);
            add_animal_focus_use_entry.Features.Add(animal_focus_additional_use_ac);

            var sacred_huntsmaster_archetype = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012");
            var additional_level_entry_features = new LevelEntry[2] { initial_entry, add_animal_focus_use_entry };
            var new_features = additional_level_entry_features.Concat(sacred_huntsmaster_archetype.AddFeatures).ToArray();
            Array.Sort(new_features, (x, y) => x.Level.CompareTo(y.Level));
            sacred_huntsmaster_archetype.AddFeatures = new_features;

            var inquisitor_progression = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("4e945c2fe5e252f4ea61eee7fb560017");
            Kingmaker.Blueprints.Classes.UIGroup animal_focus_ui_group = new Kingmaker.Blueprints.Classes.UIGroup();
            animal_focus_ui_group.Features.Add(animal_focus);
            animal_focus_ui_group.Features.Add(animal_focus_additional_use);
            inquisitor_progression.UIGroups = inquisitor_progression.UIGroups.AddToArray(animal_focus_ui_group);

            Kingmaker.Blueprints.Classes.UIGroup animal_focus_ac_ui_group = new Kingmaker.Blueprints.Classes.UIGroup();
            animal_focus_ac_ui_group.Features.Add(animal_focus_ac);
            animal_focus_ac_ui_group.Features.Add(animal_focus_additional_use_ac);
            inquisitor_progression.UIGroups = inquisitor_progression.UIGroups.AddToArray(animal_focus_ac_ui_group);

            //remove racial enemies on sacred huntsmaster
            var racial_enemy_feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("16cc2c937ea8d714193017780e7d4fc6");
            var racial_enemy_rankup_feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c1be13839472aad46b152cf10cf46179");
            foreach (var lvl_entry in sacred_huntsmaster_archetype.AddFeatures)
            {
                lvl_entry.Features.Remove(racial_enemy_feat);
                lvl_entry.Features.Remove(racial_enemy_rankup_feat);
            }
        }


 



    }
}
