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
    class Witch
    {
        static internal LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static internal BlueprintCharacterClass witch_class;
        static internal BlueprintProgression witch_progression;
        static internal BlueprintFeatureSelection witch_patrons;
        static internal BlueprintFeatureSelection hex_selection;
        static internal BlueprintFeatureSelection witch_familiar;
        static internal BlueprintFeature witch_cantrips;
        //hexes
        static internal BlueprintFeature healing;
        static internal BlueprintFeature beast_of_ill_omen;
        static internal BlueprintFeature slumber_hex;
        static internal BlueprintFeature misfortune_hex;
        static internal BlueprintFeature fortune_hex;
        static internal BlueprintFeature iceplant_hex;
        static internal BlueprintFeature murksight_hex;
        static internal BlueprintFeature ameliorating;
        static internal BlueprintFeature evil_eye;
        static internal BlueprintFeature summer_heat;
        static internal BlueprintFeature cackle;
        //major hexes
        static internal BlueprintFeature major_ameliorating;
        static internal BlueprintFeature major_healing;
        static internal BlueprintFeature animal_skin;
        static internal BlueprintFeature agony;
        static internal BlueprintFeature beast_gift;
        static internal BlueprintFeature harrowing_curse;
        static internal BlueprintFeature ice_tomb;
        static internal BlueprintFeature regenerative_sinew;
        static internal BlueprintFeature retribution;
        // restless slumber? withering ?
        // grand hexes
        static internal BlueprintFeature animal_servant;
        static internal BlueprintFeature death_curse;
        static internal BlueprintFeature lay_to_rest;
        static internal BlueprintFeature life_giver;
        static internal BlueprintFeature eternal_slumber;
        //death interupted ? - breath of life 1/creature/24 hours?
        static internal BlueprintFeature extra_hex_feat;

        static internal BlueprintArchetype ley_line_guardian_archetype;
        static internal BlueprintArchetype hedge_witch_archetype;
        static internal BlueprintArchetype hex_channeler_archetype;

        static internal BlueprintFeatureSelection hex_channeler_channel_energy_selection;
        static internal BlueprintFeature improved_channel_hex_positive;
        static internal BlueprintFeature improved_channel_hex_negative;
        static internal BlueprintFeature witch_channel_negative;
        static internal BlueprintFeature witch_channel_positive;
        static internal BlueprintFeature conduit_surge;

        static HexEngine hex_engine;


      
        internal static void createWitchClass()
        {
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

            witch_class = Helpers.Create<BlueprintCharacterClass>();
            witch_class.name = "WitcherClass";
            library.AddAsset(witch_class, "0df441ec2aa2407384ddd14e54a50d22");

            hex_engine = new HexEngine(getWitchArray(), StatType.Intelligence);

            witch_class.LocalizedName = Helpers.CreateString("Witch.Name", "Witch");
            witch_class.LocalizedDescription = Helpers.CreateString("Witch.Description",
                "Some gain power through study, some through devotion, others through blood, but the witch gains power from her communion with the unknown.Generally feared and misunderstood, the witch draws her magic from a pact made with an otherworldly power.Communing with that source, using her familiar as a conduit, the witch gains not only a host of spells, but a number of strange abilities known as hexes.As a witch grows in power, she might learn about the source of her magic, but some remain blissfully unaware.Some are even afraid of that source, fearful of what it might be or where its true purposes lie.\n"
                + "Role: While many witches are recluses, living on the edge of civilization, some live within society, openly or in hiding.The blend of witches’ spells makes them adept at filling a number of different roles, from seer to healer, and their hexes grant them a number of abilities that are useful in a fight.Some witches travel about, seeking greater knowledge and better understanding of the mysterious powers that guide them."
                );
            witch_class.m_Icon = wizard_class.Icon;
            witch_class.SkillPoints = wizard_class.SkillPoints;
            witch_class.HitDie = DiceType.D6;
            witch_class.BaseAttackBonus = wizard_class.BaseAttackBonus;
            witch_class.FortitudeSave = wizard_class.FortitudeSave;
            witch_class.ReflexSave = wizard_class.ReflexSave;
            witch_class.WillSave = wizard_class.WillSave;
            witch_class.Spellbook = createWitchSpellbook();
            witch_class.ClassSkills = new StatType[] {StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion, StatType.SkillUseMagicDevice,
                                                      StatType.SkillPersuasion};
            witch_class.IsDivineCaster = false;
            witch_class.IsArcaneCaster = true;
            witch_class.StartingGold = wizard_class.StartingGold;
            witch_class.PrimaryColor = wizard_class.PrimaryColor;
            witch_class.SecondaryColor = wizard_class.SecondaryColor;
            witch_class.RecommendedAttributes = wizard_class.RecommendedAttributes;
            witch_class.NotRecommendedAttributes = wizard_class.NotRecommendedAttributes;
            witch_class.EquipmentEntities = wizard_class.EquipmentEntities;
            witch_class.MaleEquipmentEntities = wizard_class.MaleEquipmentEntities;
            witch_class.FemaleEquipmentEntities = wizard_class.FemaleEquipmentEntities;
            witch_class.ComponentsArray = wizard_class.ComponentsArray;
            witch_class.StartingItems = new Kingmaker.Blueprints.Items.BlueprintItem[] {library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("511c97c1ea111444aa186b1a58496664"), //crossbow
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("ada85dae8d12eda4bbe6747bb8b5883c"), // quarterstaff
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("cd635d5720937b044a354dba17abad8d"), //s. cure light wounds
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("cd635d5720937b044a354dba17abad8d"), //s. cure light wounds
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("e8308a74821762e49bc3211358e81016"), //s. mage armor
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("3c56e535129756e449af6c0e67fd937f") //s. burning hands
                                                                                       };
            createWitchProgression();
            witch_class.Progression = witch_progression;
            createLeyLineGuardian();
            createHedgeWitch();
            createHexChanneler();
            witch_class.Archetypes = new BlueprintArchetype[] {ley_line_guardian_archetype, hedge_witch_archetype, hex_channeler_archetype};
            Helpers.RegisterClass(witch_class);
            createExtraHexFeat();

            addToPrestigeClasses();
        }


        static void addToPrestigeClasses()
        {
            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, witch_class.Spellbook, "EldritchKnightWitch",
                                       Common.createPrerequisiteClassSpellLevel(witch_class, 3),
                                       Common.prerequisiteNoArchetype(witch_class, ley_line_guardian_archetype));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, witch_class.Spellbook, "ArcaneTricksterWitch",
                           Common.createPrerequisiteClassSpellLevel(witch_class, 2),
                           Common.prerequisiteNoArchetype(witch_class, ley_line_guardian_archetype));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, witch_class.Spellbook, "MysticTheurgeWitch",
                           Common.createPrerequisiteClassSpellLevel(witch_class, 2),
                           Common.prerequisiteNoArchetype(witch_class, ley_line_guardian_archetype));

            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, ley_line_guardian_archetype.ReplaceSpellbook, "EldritchKnightLeyLineGuardian",
                           Common.createPrerequisiteClassSpellLevel(witch_class, 3),
                           Common.createPrerequisiteArchetypeLevel(witch_class, ley_line_guardian_archetype, 1));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, ley_line_guardian_archetype.ReplaceSpellbook, "ArcaneTricksterLeyLineGuardian",
                           Common.createPrerequisiteClassSpellLevel(witch_class, 2),
                           Common.createPrerequisiteArchetypeLevel(witch_class, ley_line_guardian_archetype, 1));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, ley_line_guardian_archetype.ReplaceSpellbook, "MysticTheurgeLeyLineGuardian",
                           Common.createPrerequisiteClassSpellLevel(witch_class, 2),
                           Common.createPrerequisiteArchetypeLevel(witch_class, ley_line_guardian_archetype,1));

            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, ley_line_guardian_archetype.ReplaceSpellbook, "DragonDiscipleLeyLineGuardian",
                           Common.createPrerequisiteClassSpellLevel(witch_class, 1),
                           Common.createPrerequisiteArchetypeLevel(witch_class, ley_line_guardian_archetype, 1));
        }


        static void createLeyLineGuardian()
        {
            ley_line_guardian_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "LeyLineGuardianArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Ley Line Guardian");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some witches tap into the power of their patrons not through a special connection with a familiar, but rather directly through the vast network of ley lines that crosses the planes. These witches can harness the latent powers of ley lines without even needing to be near one of the points where ley lines’ powers are accessible to mortal spellcasters.\n"
                                                                                       + "Instead of preparing her spells, a ley line guardian draws the power casting spells directly from ley lines. A ley line guardian is a spontaneous spellcaster. She knows the same number of spells and receives the same number of spell slots per day as a sorcerer of her witch level. Bonus spells granted by a ley line guardian’s patron are added to the ley line guardian’s total spells known at the appropriate levels.\n"
                                                                                       + "Note: Ley Line Guardian uses INTELLIGENCE as primary casting attribute!");
            });
            Helpers.SetField(ley_line_guardian_archetype, "m_ParentClass", witch_class);
            library.AddAsset(ley_line_guardian_archetype, "d4a3aa7c1cf84e14ae532c92e675927f");
            createConduitSurge();
            ley_line_guardian_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, witch_familiar), Helpers.LevelEntry(1, hex_selection), Helpers.LevelEntry(8, hex_selection) };
            ley_line_guardian_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, conduit_surge) };
            var sorcerer_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");

            var spellbook = library.CopyAndAdd<BlueprintSpellbook>(witch_class.Spellbook, "LeyLineGuardianSpellbook", "a3a86b1efe31479cb8543c76bd522147");
            
            spellbook.CanCopyScrolls = false;
            spellbook.Spontaneous = true;
            spellbook.SpellsKnown = sorcerer_class.Spellbook.SpellsKnown;
            spellbook.SpellsPerDay = sorcerer_class.Spellbook.SpellsPerDay;
            spellbook.SpellsPerLevel = sorcerer_class.Spellbook.SpellsPerLevel;
            spellbook.Name = ley_line_guardian_archetype.LocalizedName;
            ley_line_guardian_archetype.ReplaceSpellbook = spellbook;
        }


        static void createConduitSurge()
        {

            var resource = Helpers.CreateAbilityResource("LeyLineGuardianConduitSurgeResource",
                                             "",
                                             "",
                                             "",
                                             null
                                             );
            resource.SetIncreasedByStat(3, StatType.Charisma);

            var surge = Helpers.Create<NewMechanics.ConduitSurge>();
            surge.buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3"); //stagerred
            surge.save_type = SavingThrowType.Fortitude;
            surge.rate = DurationRate.Minutes;
            surge.dice_value = Helpers.CreateContextDiceValue(DiceType.D4, Common.createSimpleContextValue(1), Helpers.CreateContextValue(AbilityRankType.DamageBonus));
            surge.resource = resource;

            var shadow_evocation = library.Get<BlueprintAbility>("237427308e48c3341b3d532b9d3a001f");
            var buff = Helpers.CreateBuff("LeyLineGuardianConduitSurgeBuff",
                              "Conduit Surge",
                              "At 1st level, a ley line guardian is adept at channeling energy from ley lines to enhance her own spells. As a swift action, she can increase her effective caster level for the next spell she casts in that round by 1d4–1 levels. After performing a conduit surge, the ley line guardian must succeed at a Fortitude save (DC = 10 + level of spell cast + number of additional caster levels granted) or become staggered for a number of minutes equal to the level of the spell cast. At 8th level, the caster level increase becomes 1d4. She can use this ability a number of times per day equal to 3 + her Charisma modifier.",
                              "",
                              shadow_evocation.Icon,
                              null,
                              surge,
                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWitchArray(),
                                                              progression: ContextRankProgression.Custom, type: AbilityRankType.DamageBonus,
                                                              customProgression: new (int, int)[] {
                                                                            (7, -1),
                                                                            (20, 0)
                                                                })
                              );


            var ability = Helpers.CreateActivatableAbility("LeyLineGuardianConduitSurgeAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, ResourceSpendType.Never)
                                                           );
          
            conduit_surge = Helpers.CreateFeature("LeyLineGuardianConduitSurgeFeature",
                                             ability.Name,
                                             ability.Description,
                                             "",
                                             ability.Icon,
                                             FeatureGroup.None,
                                             Helpers.CreateAddFact(ability),
                                             Helpers.CreateAddAbilityResource(resource)
                                             );
        }


        static void createHedgeWitch()
        {
            hedge_witch_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "HedgeWitchArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Hedge Witch");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Among witches, there are those who devote themselves to the care of others and restrict their practices to the healing arts. They often take the place of clerics in rural communities and may wander the countryside servicing the needs of several small communities.");
            });
            Helpers.SetField(hedge_witch_archetype, "m_ParentClass", witch_class);
            library.AddAsset(hedge_witch_archetype, "721173ec8def432ea01dd024d53e8fb8");
            hedge_witch_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(4, hex_selection),
                                                                      Helpers.LevelEntry(8, hex_selection)};

            var witch_spontaneous_cure = library.CopyAndAdd<BlueprintFeature>("5e4620cea099c9345a9207c11d7bc916", "WitchSpontaneousCure", "ba941f7c2096461bbda75578b52b792cf");
            witch_spontaneous_cure.SetName("Spontaneous Healing");
            witch_spontaneous_cure.SetDescription("A hedge witch can channel stored spell energy into healing spells that she did not prepare ahead of time. The witch can “lose” any prepared spell that is not an orison in order to cast any cure spell of the same spell level or lower, even if she doesn’t know that cure spell.");
            witch_spontaneous_cure.ReplaceComponent<Kingmaker.UnitLogic.FactLogic.SpontaneousSpellConversion>(Common.createSpontaneousSpellConversion(witch_class,
                                                                                                                                                      null,
                                                                                                                                                      library.Get<BlueprintAbility>("5590652e1c2225c4ca30c4a699ab3649"),
                                                                                                                                                      library.Get<BlueprintAbility>("6b90c773a6543dc49b2505858ce33db5"),
                                                                                                                                                      library.Get<BlueprintAbility>("6b90c773a6543dc49b2505858ce33db5"),
                                                                                                                                                      library.Get<BlueprintAbility>("3361c5df793b4c8448756146a88026ad"),
                                                                                                                                                      library.Get<BlueprintAbility>("41c9016596fe1de4faf67425ed691203"),
                                                                                                                                                      library.Get<BlueprintAbility>("5d3d689392e4ff740a761ef346815074"),
                                                                                                                                                      library.Get<BlueprintAbility>("571221cc141bc21449ae96b3944652aa"),
                                                                                                                                                      library.Get<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397"),
                                                                                                                                                      library.Get<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449")
                                                                                                                                                     )
                                                                                                             );
            hedge_witch_archetype.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, witch_spontaneous_cure)};
        }


        static internal void createHexChanneler()
        {
            hex_channeler_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "HexChannelerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Hex Channeler");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A hex channeler is a witch who devotes herself to either life—healing the wounded and destroying the undead—or death, slaying the living and aiding undead.");
            });
            Helpers.SetField(hex_channeler_archetype, "m_ParentClass", witch_class);
            library.AddAsset(hex_channeler_archetype, "6fb0c184122e42e686d23d9d473d621e");
            hex_channeler_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(2, hex_selection)};

            createHexChannelerChannelEnergySelection();
            hex_channeler_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(2, hex_channeler_channel_energy_selection) };

            //replace improved_channel_hex_positive for negative energy channeler 
            Action<UnitDescriptor> save_game_fix = delegate (UnitDescriptor u)
            {
                if (u.HasFact(witch_channel_negative))
                {
                    while (u.Progression.Features.HasFact(improved_channel_hex_positive))
                    {
                        u.Progression.ReplaceFeature(improved_channel_hex_positive, improved_channel_hex_negative);
                    }
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_fix);
        }


        static void createHexChannelerChannelEnergySelection()
        {
            var bless_spell = library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638");

            var select_positive = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var select_negative = library.Get<BlueprintFeature>("3adb2c906e031ee41a01bfc1d5fb7eea");

            witch_channel_positive = Helpers.CreateFeature("WitchChannelPositive",
                                                               select_positive.Name,
                                                               select_negative.Description,
                                                               "2cca6a04afd64ebd84ee6aad6d1cea5f",
                                                               select_positive.Icon,
                                                               FeatureGroup.ChannelEnergy,
                                                               Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Good
                                                                                                  | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral
                                                                                                  | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                                  | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                                  )
                                                             );

            witch_channel_negative = Helpers.CreateFeature("WitchChannelNegative",
                                                                select_negative.Name,
                                                                select_negative.Description,
                                                                "bffcdc859c954a08bbbbe1eddb4b2115",
                                                                select_negative.Icon,
                                                                FeatureGroup.ChannelEnergy,
                                                                Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Evil
                                                                                                    | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral
                                                                                                    | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                                    | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                                    )
                                                               );
            var channel_energy_fact = library.Get<BlueprintUnitFact>("93f062bc0bf70e84ebae436e325e30e8");
            improved_channel_hex_positive = Helpers.CreateFeature("WitchImprovedChannelHex",
                                             "Increase Channel Energy Amount",
                                             "Every time the hex channeler is able to learn a new hex (including major or grand hexes, but not hexes gained through the Extra Hex feat), she can instead increase her channel energy amount by 1d6.",
                                             "6638ec10b97b4e5bad312f58b80db844",
                                             bless_spell.Icon,
                                             FeatureGroup.None,
                                             Common.createPrerequisiteArchetypeLevel(witch_class, hex_channeler_archetype, 2),
                                             Helpers.PrerequisiteFeature(witch_channel_positive));
            improved_channel_hex_positive.Ranks = 20;
            improved_channel_hex_negative = Helpers.CreateFeature("WitchImprovedChannelHexNegative",
                                 "Increase Channel Energy Amount",
                                 "Every time the hex channeler is able to learn a new hex (including major or grand hexes, but not hexes gained through the Extra Hex feat), she can instead increase her channel energy amount by 1d6.",
                                 "6e5bbc077cc34af0ab934a2b84760807",
                                 bless_spell.Icon,
                                 FeatureGroup.None,
                                 Common.createPrerequisiteArchetypeLevel(witch_class, hex_channeler_archetype, 2),
                                 Helpers.PrerequisiteFeature(witch_channel_negative)
                                 );
            improved_channel_hex_negative.Ranks = 20;
            var context_rank_config_positive = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, progression: ContextRankProgression.AsIs,
                                                          featureList: new BlueprintFeature[] { witch_channel_positive,
                                                                                                improved_channel_hex_positive});

            var context_rank_config_negative = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, progression: ContextRankProgression.AsIs,
                                              featureList: new BlueprintFeature[] { witch_channel_negative,
                                                                                   improved_channel_hex_negative });

            var positive_heal = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal, "WitchPostiveHeal", "b305df2f8ec34684867db7402677388b",
                                                                        witch_channel_positive, context_rank_config_positive);
            var positive_harm = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm, "WitchPostiveHarm", "4ca35352f0eb49a49faf4a1057ed5d6e",
                                                                        witch_channel_positive, context_rank_config_positive);
            var negative_heal = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHeal, "WitchNegativeHeal", "a39b06c274c843f19fa10cc6b7be5f39",
                                                                        witch_channel_negative, context_rank_config_negative);
            var negative_harm = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHarm, "WitchNegativeHarm", "ba94b10d81bb4497886e50ce9d4d96ce",
                                                                        witch_channel_negative, context_rank_config_negative);
            witch_channel_positive.AddComponent(Helpers.CreateAddFacts(channel_energy_fact, positive_heal, positive_harm));
            witch_channel_negative.AddComponent(Helpers.CreateAddFacts(channel_energy_fact, negative_heal, negative_harm));


            hex_channeler_channel_energy_selection = Helpers.CreateFeatureSelection("WitchChannelEnergySelection",
                                                                                    "Channel Energy",
                                                                                    "At 2nd level, a hex channeler can call upon her patron to release a wave of energy from herself or her familiar.A good witch channels positive energy(like a good cleric), and an evil witch channels negative energy(like an evil cleric).A witch who is neither good nor evil must choose whether she channels positive or negative energy; once this choice is made, it cannot be reversed.\n"
                                                                                    + "Channeling energy causes a burst that affects all creatures of one type(either undead or living) in a 30 - foot radius centered on the witch. The witch can channel energy a number of times per day equal to 3 + her Charisma modifier(minimum 1). This otherwise functions as a cleric using channel energy, except the witch does not require a holy symbol to use this ability.The hex channeler uses her witch level as her cleric level for all other effects dependent upon channel energy(except increasing the amount of damage healed or dealt).\n"
                                                                                    + "This burst heals or deals 1d6 points of damage.Every time the hex channeler is able to learn a new hex (including major or grand hexes, but not hexes gained through the Extra Hex feat), she can instead increase her channel energy amount by 1d6.",
                                                                                    "d33b4095dbfa47588ed1f07b5af30e2c",
                                                                                    bless_spell.Icon,
                                                                                    FeatureGroup.None);
            hex_channeler_channel_energy_selection.Features = new BlueprintFeature[] { witch_channel_positive, witch_channel_negative };
            hex_channeler_channel_energy_selection.AllFeatures = hex_channeler_channel_energy_selection.Features;


            hex_selection.Features = hex_selection.Features.AddToArray(improved_channel_hex_positive, improved_channel_hex_negative);
            hex_selection.AllFeatures = hex_selection.AllFeatures.AddToArray(improved_channel_hex_positive, improved_channel_hex_negative);

            ChannelEnergyEngine.updateItemsFeature(ChannelEnergyEngine.ChannelType.PositiveHeal | ChannelEnergyEngine.ChannelType.PositiveHarm, improved_channel_hex_positive);
            ChannelEnergyEngine.updateItemsFeature(ChannelEnergyEngine.ChannelType.NegativeHeal | ChannelEnergyEngine.ChannelType.NegativeHarm, improved_channel_hex_negative);

            ChannelEnergyEngine.createExtraChannelFeat(positive_heal, hex_channeler_channel_energy_selection, "ExtraChannelWitch", "Extra Channel (Hex Channeler)",
                                                       "9c90fbbe75dc4bd0951e6d5be6da5627");
        }


        static void createHexSelection()
        {
            createBeastOfIllOmen();
            createHealing();
            createIceplantHex();
            createFortuneHex();
            createMisfortune();
            createMurksightHex();
            createSlumber();
            createAmeliorating();
            createEvilEye();
            createSummerHeat();

            createMajorHealing();
            createMajorAmeliorating();
            createAnimalSkin();
            createAgony();
            createBeastGift();
            createHarrowingCurse();
            createIceTomb();
            createRegenerativeSinew();
            createRetribution();

            createAnimalServant();
            createDeathCurse();
            createLayToRest();
            createLifeGiver();
            createEternalSlumber();
            createCackle();

            hex_selection = Helpers.CreateFeatureSelection("WitchHexSelection",
                                                           "Hex",
                                                           "Witches learn a number of magic tricks, called hexes, that grant them powers or weaken foes. At 1st level, a witch gains one hex of her choice. She gains an additional hex at 2nd level and for every 2 levels attained after 2nd level. A witch cannot select an individual hex more than once.\n" +
                                                           "Unless otherwise noted, using a hex is a standard action that does not provoke an attack of opportunity. The save to resist a hex is equal to 10 + 1 / 2 the witch’s level + the witch’s Intelligence modifier.",
                                                           "68bd6449147e4234b6d9a80564ba17ae",
                                                           null,
                                                           FeatureGroup.None);
            hex_selection.Features = new BlueprintFeature[] { ameliorating, healing, beast_of_ill_omen, slumber_hex, misfortune_hex, fortune_hex, iceplant_hex, murksight_hex, evil_eye, summer_heat, cackle,
                                                              major_healing,  major_ameliorating, animal_skin, agony, beast_gift, harrowing_curse, ice_tomb, regenerative_sinew, retribution,
                                                              animal_servant, death_curse, lay_to_rest, life_giver, eternal_slumber};
            hex_selection.AllFeatures = hex_selection.Features;
        }


        static void createWitchCantrips()
        {
            var daze = library.Get<BlueprintAbility>("55f14bc84d7c85446b07a1b5dd6b2b4c");
            witch_cantrips = Common.createCantrips("WitchCantripsFeature",
                                                   "Cantrips",
                                                   "Witches can cast a number of cantrips, or 0-level spells. These spells are cast like any other spell, but they are not expended when cast and may be used again.",
                                                   daze.Icon,
                                                   "86501dda312a4f548d579632c4a06c0f",
                                                   witch_class,
                                                   StatType.Intelligence,
                                                   witch_class.Spellbook.SpellList.SpellsByLevel[0].Spells.ToArray());
        }


        static void createWitchProgression()
        {
            createWitchPatrons();
            createWitchCantrips();
            createHexSelection();
            witch_progression = Helpers.CreateProgression("WitchProgression",
                                       witch_class.Name,
                                       witch_class.Description,
                                       "6973ab6ad02f4265888c3fdbe7e12921",
                                       witch_class.Icon,
                                       FeatureGroup.None);
            witch_progression.Classes = getWitchArray();

            var witch_proficiencies = library.CopyAndAdd<BlueprintFeature>("25c97697236ccf2479d0c6a4185eae7f", //sorcerer proficiencies
                                                                            "WitchProficiencies",
                                                                            "a042a27a76f94a2ca5e0997d5f432a33");
            witch_proficiencies.SetName("Witch Proficiencies");
            witch_proficiencies.SetDescription("Witches are proficient with all simple weapons. They are not proficient with any type of armor or shield. Armor interferes with a witch’s gestures, which can cause her spells with somatic components to fail.");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            witch_familiar = library.CopyAndAdd<BlueprintFeatureSelection>("363cab72f77c47745bf3a8807074d183", "WitchFamiliar", "a07215bde92a4cc986ee3059cb8b7350");
            witch_familiar.DlcType = Kingmaker.Blueprints.Root.DlcType.None;
            witch_familiar.ComponentsArray = new BlueprintComponent[0];
            witch_familiar.SetDescription("At 1st level, a witch forms a close bond with a familiar, a creature that teaches her magic and helps to guide her along her path. Familiars also aid a witch by granting her skill bonuses, additional spells, and help with some types of magic.");

            var entries = new List<LevelEntry>();
            entries.Add(Helpers.LevelEntry(1, witch_proficiencies, witch_cantrips, detect_magic, witch_patrons, witch_familiar, hex_selection,
                                                           library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                           library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa"),  // touch calculate feature
                                                           library.Get<BlueprintFeature>("9fc9813f569e2e5448ddc435abf774b3") //full caster feature
                                                         ));
            witch_progression.UIGroups = new UIGroup[1] { Helpers.CreateUIGroup(hex_selection) };
            for (int i = 2; i<= 20; i++)
            {
                if (i % 2 == 0)
                {
                    entries.Add(Helpers.LevelEntry(i, hex_selection));
                    witch_progression.UIGroups[0].Features.Add(hex_selection);
                }
                else
                {
                    entries.Add(Helpers.LevelEntry(i));
                }
            }
            
            
            witch_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { witch_familiar, witch_proficiencies, witch_cantrips, detect_magic };
            witch_progression.LevelEntries = entries.ToArray();
        }



        static void createWitchPatrons()
        {
            BlueprintFeature[] patrons = new BlueprintFeature[]
            {
                createWitchPatronFeature("Agility", "f7a4a115c138439c8e5f4ee8adacfca0","9b43dc766cae40b7891c035b8fa43522",
                                         "4f8181e7a7f1d904fbaea64220e83379", //expeditious retreat
                                         "de7a025d48ad5da4991e7d3c682cf69d", //cats grace
                                         "486eaff58293f6441a5c2759c4872f98", //haste
                                         "4c349361d720e844e846ad8c19959b1e", //freedom of movement
                                         "93d9d74dac46b9b458d4d2ea7f4b1911", //polymorph
                                         "1f6c94d56f178b84ead4c02f1b1e1c48", //cats grace mass
                                         "a9fc28e147dbb364ea4a3c1831e7e55f", //polymorph greater
                                         "9155dbc8268da1c49a7fc4834fa1a4b1", //cloak of chaos
                                         Wildshape.shapechange.AssetGuid //shapechange
                                         ),
                createWitchPatronFeature("Ancestors", "7e72537869dc455781780147823fc6a5", "a5dd1fcfdff84b2396c21c7705faf3fc",
                                         "90e59f4a4ada87243b7b3535a06d0638", //bless
                                         "03a9630394d10164a9410882d31572f0", //aid
                                         "faabd2cc67efa4646ac58c7bb3e40fcc", //prayer
                                         "0413915f355a38146bc6ad40cdf27b3f", //death ward
                                         "1bc83efec9f8c4b42a46162d72cbf494", //burst of glory
                                         "e15e5e7045fda2244b98c8f010adfe31", //heroism, greater
                                         "3cf5b2bd093a36a468b4ece38ad4d5fa", //bestow grace of the champion
                                         "cbf3bafa8375340498b86a3313a11e2f", //euphoric tranquility
                                         "870af83be6572594d84d276d7fc583e0" //weird
                                        ),
                createWitchPatronFeature("Animal", "aee09b4c9ae843c09829ef220241affe", "1eb5c7c9254044b5bad7bdf0eca1764a",
                                         "403cf599412299a4f9d5d925c7b9fb33", //magic fang
                                         "4c3d08935262b6544ae97599b3a9556d", //bull strength
                                         "754c478a2aa9bb54d809e648c3f7ac0e", //dominate animal
                                         "c83db50513abdf74ca103651931fac4b", //summon nature ally 4
                                         "56923211d2ac95e43b8ac5031bab74d8", //animal growth
                                         "9b93040dad242eb43ac7de6bb6547030", //beast shape 3
                                         "940a545a665194b48b722c1f9dd78d53", //beast shape 4
                                         "ea78c04f0bd13d049a1cce5daf8d83e0", //natures ally 8
                                         "a7469ef84ba50ac4cbf3d145e3173f8e"  //natures ally 9
                                        ),
                createWitchPatronFeature("Autumn", "c3c1f806206546628b7b9d44cc1141ad", "beaddc15271240d992b8149babb79fed",
                                         "450af0402422b0b4980d9c2175869612", //ray of enfeeblement
                                         "29ccc62632178d344ad0be0865fd3113", //create pit
                                         "1a36c8b9ed655c249a9f9e8d4731f001", //soothing mud
                                         "6b30813c3709fc44b92dc8fd8191f345", //slowing mud
                                         "6d1d48a939ce475409f06e1b376bc386", //vinetrap
                                         "dbf99b00cd35d0a4491c6cc9e771b487", //acid fog
                                         "8c29e953190cc67429dc9c701b16b7c2", //caustic eruption
                                         "08323922485f7e246acb3d2276515526", //horrid witling
                                         "b24583190f36a8442b212e45226c54fc" //wail of banshee
                                        ),
                createWitchPatronFeature("Devotion", "faa76cf5cacd447caa2c18965ca9c3cb", "89bd5b848cfc4d6eacb51cf8e018e8d4",
                                         "9d5d2d3ffdd73c648af3eb3e585b1113", //divine favor
                                         "042aaa117e89c4d4b8cb41478dd3fca3", //grace
                                         "2d4263d80f5136b4296d6eb43a221d7d", //magic vestment
                                         "a26c23a887a6f154491dc2cefdad2c35",  //crusader's edge
                                         "f9910c76efc34af41b6e43d5d8752f0f", //flameStrike
                                         "6a234c6dcde7ae94e94e9c36fd1163a7", //bulls strength mass
                                         "3cf5b2bd093a36a468b4ece38ad4d5fa", //bestow grace of the champion
                                         "808ab74c12df8784ab4eeaf6a107dbea", //holy aura
                                         "867524328b54f25488d371214eea0d90" //heal mass
                                        ),
                createWitchPatronFeature("Elements", "1c19b61a30e34682b40261d29acd6ac3", "4f51c6e364be432f8e285893b4611b22",
                                         "ab395d2335d3f384e99dddee8562978f", //shocking grasp
                                         "cdb106d53c65bbc4086183d54c3b97c7", //scorching ray
                                         "2d81362af43aeac4387a3d4fced489c3", //fireball
                                         "690c90a82bf2e58449c6b541cb8ea004", //elemental body 1
                                         "f9910c76efc34af41b6e43d5d8752f0f", //flamestrike
                                         "6303b404df12b0f4793fa0763b21dd2c", //elemental assesor
                                         "8eb769e3b583f594faabe1cfdb0bb696", //summon greater elemental
                                         "e3d0dfe1c8527934294f241e0ae96a8d", //firestorm
                                         "d8144161e352ca846a73cf90e85bf9ac" //tsunami
                                        ),
                createWitchPatronFeature("Endurance", "16e0d62dac7947aab226dcded5bc5177", "79301d011ec5490ea32dcd42a8daa366",
                                         "b065231094a21d14dbf1c3832f776871", //fire belly
                                         "a900628aea19aa74aad0ece0e65d091a", //bears endurance
                                         "d2f116cfe05fcdd4a94e80143b67046f", //protection from energy
                                         "76a629d019275b94184a1a8733cac45e", //protection from energy communal
                                         "0a5ddfbcfb3989543ac7c936fc256889", //spell resistance
                                         "f6bcea6db14f0814d99b54856e918b92", //bears endurance mass
                                         "fafd77c6bfa85c04ba31fdc1c962c914", //restoration greater
                                         "b1c7576bd06812b42bda3f09ab202f14", //angelic aspect greater
                                         "867524328b54f25488d371214eea0d90"  //heal mass
                                         ),
                createWitchPatronFeature("Healing", "84286f03a92c4d27bf13484dee22c990", "b6c6c3ee2e904c689d9cb0eca78ce7c1",
                                         "55a037e514c0ee14a8e3ed14b47061de", //remove fear
                                         "e84fc922ccf952943b5240293669b171", //restoration lesser
                                         "4093d5a0eb5cae94e909eb1e0e1a6b36", //remove disiease
                                         "f2115ac1148256b4ba20788f7e966830", //restoration
                                         "be2062d6d85f4634ea4f26e9e858c3b8", //cleanse
                                         "788d72e7713cf90418ee1f38449416dc", //inspiring recovery
                                         "fafd77c6bfa85c04ba31fdc1c962c914", //restoration greater
                                         library.CopyAndAdd<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449", "WitchHealingPatronCureCriticalWoundsMassAbility", "").AssetGuid, 
                                         //cure critical wounds mass
                                         "867524328b54f25488d371214eea0d90" // heal mass
                                         ),
                createWitchPatronFeature("Light", "8676966b3e0f4595be33f9cd2efc061c", "c248bad0e267442890c292c2079bcd2b",
                                         "91da41b9793a4624797921f221db653c", //color spray
                                         "ce7dad2b25acf85429b6c9550787b2d9", //glitterdust
                                         "c927a8b0cd3f5174f8c0b67cdbfde539", //remove blindness
                                         "4b8265132f9c8174f87ce7fa6d0fe47b", //rainbow pattern
                                         "ebade19998e1f8542a1b55bd4da766b3", //fire snake
                                         "093ed1d67a539ad4c939d9d05cfe192c", //sirocco
                                         "1fca0ba2fdfe2994a8c8bc1f0f2fc5b1", //sunbeam
                                         "e96424f70ff884947b06f41a765b7658", //sunburst
                                         "08ccad78cac525040919d51963f9ac39" //fiery body
                                        ),
                createWitchPatronFeature("Mercy", "88ce8b8dc64b4343984fefd31054a913","b7b7f46389544a99b0b4846ab99ed042",
                                         "5590652e1c2225c4ca30c4a699ab3649", //cure light wounds
                                         "446f7bf201dc1934f96ac0a26e324803", //eagles splendor
                                         "b48674cef2bff5e478a007cf57d8345b", //remove curse
                                         "f2115ac1148256b4ba20788f7e966830", //resoration
                                         "be2062d6d85f4634ea4f26e9e858c3b8", //cleanse
                                         "2caa607eadda4ab44934c5c9875e01bc", //eagles splendor mass
                                         "15a04c40f84545949abeedef7279751a", //joyfull rupture
                                         "cbf3bafa8375340498b86a3313a11e2f", //euphoric tranquility
                                         "867524328b54f25488d371214eea0d90" //mass heal
                                        ),
                createWitchPatronFeature("Mountain", "2147870b66c643978c6c6aeae6f6c6a6", "cdcc5439ee0a4266aeb41e29f0677bf9",
                                         "85067a04a97416949b5d1dbf986d93f3", //stone fist
                                         "5181c2ed0190fc34b8a1162783af5bf4", //stone call
                                         "0a2f7c6aa81bc6548ac7780d8b70bcbc", //battering blast
                                         "d1afa8bc28c99104da7d784115552de5", //spike stones
                                         "7c5d556b9a5883048bf030e20daebe31", //stoneskin communal
                                         "e243740dfdb17a246b116b334ed0b165", //stone to flesh
                                         "3ecd589cf1a55df42a3b66940ee93ea4", //summon earth elemental greater
                                         "65254c7a2cf18944287207e1de3e44e8", //summon earth elemental elder
                                         "01300baad090d634cb1a1b2defe068d6" //clashing rocks
                                        ),
                createWitchPatronFeature("Plague", "ad6dcbca48394d0890565eef09c0dc19", "7339a5c3c56d4403860790af1118959a",
                                         "fa3078b9976a5b24caf92e20ee9c0f54", //ray of sickening
                                         "dee3074b2fbfb064b80b973f9b56319e", //pernicious poison
                                         "48e2744846ed04b4580be1a3343a5d3d", //contagion
                                         "4b76d32feb089ad4499c3a1ce8e1ac27", //animate dead
                                         "548d339ba87ee56459c98e80167bdf10", //cloudkill
                                         "76a11b460be25e44ca85904d6806e5a3", //create undead
                                         "b974af13e45639a41a04843ce1c9aa12", //creeping doom
                                         "08323922485f7e246acb3d2276515526", //horrid witlin
                                         "37302f72b06ced1408bf5bb965766d46" //energy drain
                                        ),
                createWitchPatronFeature("Protection", "fbda94ae0c184d06a9f1bc8b56e68267", "da5be607b885402eae26f954c47b47e4",
                                         NewSpells.sanctuary.AssetGuid, //sanctuary
                                         "21ffef7791ce73f468b6fca4d9371e8b", //resist energy
                                         "d2f116cfe05fcdd4a94e80143b67046f", //protection from energy
                                         "c66e86905f7606c4eaa5c774f0357b2b", //stoneskin
                                         "7c5d556b9a5883048bf030e20daebe31", //stoneskin communal
                                         "fafd77c6bfa85c04ba31fdc1c962c914", //restoration greater
                                         "42aa71adc7343714fa92e471baa98d42", //protection from spells
                                         "7ef49f184922063499b8f1346fb7f521", //seamantle
                                         "87a29febd010993419f2a4a9bee11cfc" //mindblank communal
                                         ),
                createWitchPatronFeature("Spring", "fa8155faef214069a896e65a9073458c", "e9f7f92bf7724ec788fcd6374bfd2e82",
                                         "f3c0b267dd17a2a45a40805e31fe3cd1", //feather step
                                         "6c7467f0344004d48848a43d8c078bf8", //sickening entanglement
                                         "d219494150ac1f24f9ce14a3d4f66d26", //feather step mass
                                         "a5e23522eda32dc45801e32c05dc9f96", //good hope
                                         "3fce8e988a51a2a4ea366324d6153001", //constricting coils
                                         library.CopyAndAdd<BlueprintAbility>("645558d63604747428d55f0dd3a4cb58", "WitchSpringPatronChainLightningAbility", "").AssetGuid, //chain lightning
                                         "26be70c4664d07446bdfe83504c1d757", //change staff
                                         "7cfbefe0931257344b2cb7ddc4cdff6f", //stormbolts
                                         "d8144161e352ca846a73cf90e85bf9ac" //tsunami
                                        ),
                createWitchPatronFeature("Strength", "6f859ba938f94132920eeb63a8c9af50", "8125ff1edafc4c5489ad2739a85d5386",
                                         "9d5d2d3ffdd73c648af3eb3e585b1113", //divine favor
                                         "4c3d08935262b6544ae97599b3a9556d", //bulls strength
                                         "2d4263d80f5136b4296d6eb43a221d7d", //magical vestment
                                         "ef16771cb05d1344989519e87f25b3c5", //divine power
                                         "90810e5cf53bf854293cbd5ea1066252", //righteous magic
                                         "6a234c6dcde7ae94e94e9c36fd1163a7", //bulls strength mass
                                         Wildshape.giant_formI.AssetGuid, //giant form I
                                         Wildshape.giant_formII.AssetGuid, //giant form II
                                         Wildshape.shapechange.AssetGuid //shapechange
                                        ),
                createWitchPatronFeature("Summer", "abd76216790240a6b3a0ddda236d1f19", "31e196f567fb4426b0199350dcbb2ac4",
                                         "b065231094a21d14dbf1c3832f776871", //firebelly
                                         "cdb106d53c65bbc4086183d54c3b97c7", //scorching ray
                                         "bf0accce250381a44b857d4af6c8e10d", //searing light
                                         "f72f8f03bf0136c4180cd1d70eb773a5", //controlled fireball
                                         "f9910c76efc34af41b6e43d5d8752f0f", //flame strike
                                         "093ed1d67a539ad4c939d9d05cfe192c", //sirocco
                                         "1fca0ba2fdfe2994a8c8bc1f0f2fc5b1", //sunbeam
                                         "e96424f70ff884947b06f41a765b7658", //sunburst
                                         "08ccad78cac525040919d51963f9ac39" //fiery body
                                         ),
                createWitchPatronFeature("Transformation", "6af899b9496d42258619ceebe08b1e4a" , "e71d75fcdc24439ab6a7fddbfbd92d34",
                                         "14c90900b690cac429b229efdf416127", //longstrider
                                         "a900628aea19aa74aad0ece0e65d091a", //bears endurance
                                         "61a7ed778dd93f344a5dacdbad324cc9", //beast shape 1
                                         "5d4028eb28a106d4691ed1b92bbb1915", //beast shape 2
                                         "9b93040dad242eb43ac7de6bb6547030", //beast shape 3
                                         "f767399367df54645ac620ef7b2062bb", //form of dragon 1
                                         "666556ded3a32f34885e8c318c3a0ced", //form of dragon 2
                                         "1cdc4ad4c208246419b98a35539eafa6", //form of dragon 3
                                         Wildshape.shapechange.AssetGuid //shapechange
                                         ),
                createWitchPatronFeature("Winter", "073328f3df07436eb27bcc731fca6bb1", "3d5f30725ae24f589747b1164fc44228",
                                         "9f10909f0be1f5141bf1c102041f93d9", //snowball
                                         "c83447189aabc72489164dfc246f3a36", //frigid touch
                                         library.CopyAndAdd<BlueprintAbility>("fcb028205a71ee64d98175ff39a0abf9", "WitchWinterPatronIceStormAbility", "").AssetGuid, //ice storm
                                         "65e8d23aef5e7784dbeb27b1fca40931", //icy prison
                                         library.CopyAndAdd<BlueprintAbility>("e7c530f8137630f4d9d7ee1aa7b1edc0", "WinterPatronConeOfColdABility","").AssetGuid, //cone of cold
                                         "5ef85d426783a5347b420546f91a677b", //cold ice strike
                                         "3e4d3b9a5bd03734d9b053b9067c2f38", //waves of exhaustion
                                         "17696c144a0194c478cbe402b496cb23", //polar ray
                                         "ba48abb52b142164eba309fd09898856" // polar midnight
                                         )

            };
            var diety_selection = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            witch_patrons = Helpers.CreateFeatureSelection("WitchPatronSelection",
                                                           "Patron",
                                                           "At 1st level, when a witch gains her familiar, she must also select a patron. This patron is a vague and mysterious force, granting the witch power for reasons that she might not entirely understand. While these forces need not be named, they typically hold influence over one of the following forces.\n"
                                                           + "A witch’s patron adds new spells to a witch’s list of spells known. These spells are also automatically added to the list of spells stored by the familiar. The spells gained depend upon the patron chosen.",
                                                           "30f2f38633a144028aebecdd03391470",
                                                           diety_selection.Icon,
                                                           FeatureGroup.None);
            witch_patrons.AllFeatures = patrons;
            witch_patrons.Features = patrons;

        }


        static BlueprintSpellbook createWitchSpellbook()
        {
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            var witch_spellbook = Helpers.Create<BlueprintSpellbook>();
            witch_spellbook.Name = witch_class.LocalizedName;
            witch_spellbook.name = "WitchSpellbook";
            library.AddAsset(witch_spellbook, "be5817bb59c14526a99877f8a7f15d31");
            witch_spellbook.Name = witch_class.LocalizedName;
            witch_spellbook.SpellsPerDay = wizard_class.Spellbook.SpellsPerDay;
            witch_spellbook.SpellsKnown = wizard_class.Spellbook.SpellsKnown;
            witch_spellbook.Spontaneous = false;
            witch_spellbook.IsArcane = true;
            witch_spellbook.AllSpellsKnown = false;
            witch_spellbook.CanCopyScrolls = true;
            witch_spellbook.CastingAttribute = StatType.Intelligence;
            witch_spellbook.CharacterClass = witch_class;
            witch_spellbook.CasterLevelModifier = 0;
            witch_spellbook.CantripsType = CantripsType.Cantrips;
            witch_spellbook.SpellsPerLevel = wizard_class.Spellbook.SpellsPerLevel;
          
            witch_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            witch_spellbook.SpellList.name = "WitchSpellList";
            library.AddAsset(witch_spellbook.SpellList, "422490cf62744e16a3e131efd94cf290");
            witch_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < witch_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                witch_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);

            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "55f14bc84d7c85446b07a1b5dd6b2b4c", 0), //daze
                new Common.SpellId( "c3a8f31778c3980498d8f00c980be5f5", 0), //guidance
                new Common.SpellId( "95f206566c5261c42aa5b3e7e0d1e36c", 0), //mage light
                new Common.SpellId( "7bc8e27cba24f0e43ae64ed201ad5785", 0), //resistance
                new Common.SpellId( "5bf3315ce1ed4d94e8805706820ef64d", 0), //touch of fatigue

                new Common.SpellId( "4783c3709a74a794dbe7c8e7e0b1b038", 1), //burning hands
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( "5590652e1c2225c4ca30c4a699ab3649", 1), //cure light wounds
                new Common.SpellId( "8e7cfa5f213a90549aadd18f8f6f4664", 1), //ear piercing scream
                new Common.SpellId( "c60969e7f264e6d4b84a1499fdcf9039", 1), //enlarge person
                new Common.SpellId( "88367310478c10b47903463c5d0152b0", 1), //hypnotism
                new Common.SpellId( "e5af3674bb241f14b9a9f6b0c7dc3d27", 1), //inflict light wounds
                new Common.SpellId( "9e1ad5d6f87d19e4d8883d63a6e35568", 1), //mage armor
                new Common.SpellId( "450af0402422b0b4980d9c2175869612", 1), //ray of enfeeblement
                new Common.SpellId( "fa3078b9976a5b24caf92e20ee9c0f54", 1), //ray of sickening
                new Common.SpellId( "4e0e9aba6447d514f88eff1464cc4763", 1), //reduce person
                new Common.SpellId( "f6f95242abdfac346befd6f4f6222140", 1), //remove sickness
                new Common.SpellId( "bb7ecad2d3d2c8247a38f44855c99061", 1), //sleep
                new Common.SpellId( "9f10909f0be1f5141bf1c102041f93d9", 1), //snowball
                new Common.SpellId( "8fd74eddd9b6c224693d9ab241f25e84", 1), //summon monster 1
                new Common.SpellId( "dd38f33c56ad00a4da386c1afaa49967", 1), //ubreakable heart
                new Common.SpellId( HexEngine.hex_vulnerability_spell.AssetGuid, 1), //hex vilnerability
                new Common.SpellId( NewSpells.command.AssetGuid, 1), //command

                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 2), //blindness
                new Common.SpellId( "b7731c2b4fa1c9844a092329177be4c3", 2), //boneshaker
                new Common.SpellId( "6b90c773a6543dc49b2505858ce33db5", 2), //cure moderate wounds
                new Common.SpellId( "b48b4c5ffb4eab0469feba27fc86a023", 2), //delay poison
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( "2dbe271c979d9104c8e2e6b42e208e32", 2), //fester
                new Common.SpellId( "ce7dad2b25acf85429b6c9550787b2d9", 2), //glitterdust
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 2), //hold person
                new Common.SpellId( "65f0b63c45ea82a4f8b8325768a3832d", 2), //inflict moderate wounds
                new Common.SpellId( "42a65895ba0cb3a42b6019039dd2bff1", 2), //molten orb
                new Common.SpellId( "dee3074b2fbfb064b80b973f9b56319e", 2), //pernicious poison"
                new Common.SpellId( "bc153808ef4884a4594bc9bec2299b69", 2), //pox postules
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( "1724061e89c667045a6891179ee2e8e7", 2), //summon monster 2
                new Common.SpellId( "134cb6d492269aa4f8662700ef57449f", 2), //web

                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 3), //bestow curse
                new Common.SpellId( "7658b74f626c56a49939d9c20580885e", 3), //deep slumber
                new Common.SpellId( "04e820e1ce3a66f47a50ad5074d3ae40", 3), //delay posion communal
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "5ab0d42fb68c9e34abae4921822b9d63", 3), //heroism
                new Common.SpellId( "d2cff9243a7ee804cb6d5be47af30c73", 3), //lightning bolt
                new Common.SpellId( "97b991256e43bb140b263c326f690ce2", 3), //rage
                new Common.SpellId( "c927a8b0cd3f5174f8c0b67cdbfde539", 3), //remove blindness
                new Common.SpellId( "b48674cef2bff5e478a007cf57d8345b", 3), //remove curse
                new Common.SpellId( "4093d5a0eb5cae94e909eb1e0e1a6b36", 3), //remove disease
                new Common.SpellId( "9779c8578acd919419f563c33d7b2af5", 3), //spit venom
                new Common.SpellId( "68a9e6d7256f1354289a39003a46d826", 3), //stinking cloud
                new Common.SpellId( "5d61dde0020bbf54ba1521f7ca0229dc", 3), //summon monster 3
                new Common.SpellId( "6cbb040023868574b992677885390f92", 3), //vampyric touch

                new Common.SpellId( "e418c20c8ce362943a8025d82c865c1c", 4), //cape of vasps
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 4), //confusion
                new Common.SpellId( "4baf4109145de4345861fe0f2209d903", 4), //crushing despair
                new Common.SpellId( "3361c5df793b4c8448756146a88026ad", 4), //cure serious wounds
                new Common.SpellId( "0413915f355a38146bc6ad40cdf27b3f", 4), //death ward
                new Common.SpellId( "4a648b57935a59547b7a2ee86fb4f26a", 4), //dimension door 
                new Common.SpellId( "f34fb78eaaec141469079af124bcfa0f", 4), //enervation
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life, greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( "fcb028205a71ee64d98175ff39a0abf9", 4), //ice storm
                new Common.SpellId( "651110ed4f117a948b41c05c5c7624c0", 4), //inflict serious wounds
                new Common.SpellId( "e7240516af4241b42b2cd819929ea9da", 4), //neutralize poison
                new Common.SpellId( "6717dbaef00c0eb4897a1c908a75dfe5", 4), //phantasmal killer
                new Common.SpellId( "d797007a142a6c0409a74b064065a15e", 4), //poison
                new Common.SpellId( "7ed74a3ec8c458d4fb50b192fd7be6ef", 4), //summon monster 4
                new Common.SpellId( "1e481e03d9cf1564bae6b4f63aed2d1a", 4), //touch of slime
                new Common.SpellId( "16ce660837fb2544e96c3b7eaad73c63", 4), //volcanic storm

                new Common.SpellId( "3105d6e9febdc3f41a08d2b7dda1fe74", 5), //baleful polymorph
                new Common.SpellId( "7792da00c85b9e042a0fdfc2b66ec9a8", 5), //break enchantment
                new Common.SpellId( "bacba2ff48d498b46b86384053945e83", 5), //cave fangs
                new Common.SpellId( "548d339ba87ee56459c98e80167bdf10", 5), //cloudkill
                new Common.SpellId( "41c9016596fe1de4faf67425ed691203", 5), //cure critical wounds
                new Common.SpellId( "d7cbd2004ce66a042aeab2e95a3c5c61", 5), //dominate person
                new Common.SpellId( "444eed6e26f773a40ab6e4d160c67faa", 5), //feeblemind
                new Common.SpellId( "41e8a952da7a5c247b3ec1c2dbb73018", 5), //hold monster
                new Common.SpellId( "651110ed4f117a948b41c05c5c7624c0", 5), //inflict critical wounds
                new Common.SpellId( "eabf94e4edc6e714cabd96aa69f8b207", 5), //mind fog
                new Common.SpellId( "630c8b85d9f07a64f917d79cb5905741", 5), //summon monster 5
                new Common.SpellId( "8878d0c46dfbd564e9d5756349d5e439", 5), //waves of fatigue

                new Common.SpellId( "d42c6d3f29e07b6409d670792d72bc82", 6), //banshee blast
                new Common.SpellId( "7f71a70d822af94458dc1a235507e972", 6), //cloak of dreams
                new Common.SpellId( "e7c530f8137630f4d9d7ee1aa7b1edc0", 6), //cone of cold
                new Common.SpellId( "5d3d689392e4ff740a761ef346815074", 6), //cure light wounds mass
                new Common.SpellId( "f0f761b808dc4b149b08eaf44b99f633", 6), //dispel magic greater
                new Common.SpellId( "3167d30dd3c622c46b0c0cb242061642", 6), //eyebyte
                new Common.SpellId( "52b8b14360a87104482b2735c7fc8606", 6), //fester mass
                new Common.SpellId( "700cfcbd0cb2975419bcab7dbb8c6210", 6), //hellfire ray
                new Common.SpellId( "e15e5e7045fda2244b98c8f010adfe31", 6), //heroism greater
                new Common.SpellId( "9da37873d79ef0a468f969e4e5116ad2", 6), //inflict light wounds mass
                new Common.SpellId( "1f2e6019ece86d64baa5effa15e81ecc", 6), //phantasmal putrefecation
                new Common.SpellId( "a0fc99f0933d01643b2b8fe570caa4c5", 6), //raise dead
                new Common.SpellId( "a6e59e74cba46a44093babf6aec250fc", 6), //slay living
                new Common.SpellId( "e243740dfdb17a246b116b334ed0b165", 6), //stone to flash
                new Common.SpellId( "e740afbab0147944dab35d83faa0ae1c", 6), //summon monster 6
                new Common.SpellId( "27203d62eb3d4184c9aced94f22e1806", 6), //transformation
                new Common.SpellId( "b3da3fbee6a751d4197e446c7e852bcb", 6), //true seeing

                new Common.SpellId( "645558d63604747428d55f0dd3a4cb58", 7), //chain lightning
                new Common.SpellId( "571221cc141bc21449ae96b3944652aa", 7), //cure moderate wounds mass
                new Common.SpellId( "137af566f68fd9b428e2e12da43c1482", 7), //harm
                new Common.SpellId( "ff8f1534f66559c478448723e16b6624", 7), //heal
                new Common.SpellId( "03944622fbe04824684ec29ff2cec6a7", 7), //inflict moderate wounds mass
                new Common.SpellId( "2b044152b3620c841badb090e01ed9de", 7), //insanity
                new Common.SpellId( "da1b292d91ba37948893cdbe9ea89e28", 7), //legendary proportions
                new Common.SpellId( "261e1788bfc5ac1419eec68b1d485dbc", 7), //power word blind
                new Common.SpellId( "ab167fd8203c1314bac6568932f1752f", 7), //summon monster 7
                new Common.SpellId( "474ed0aa656cc38499cc9a073d113716", 7), //umbral strike
                new Common.SpellId( "1e2d1489781b10a45a3b70192bba9be3", 7), //waves of Ectasy
                new Common.SpellId( "3e4d3b9a5bd03734d9b053b9067c2f38", 7), //waves of exhaustion

                new Common.SpellId( "0cea35de4d553cc439ae80b3a8724397", 8), //cure serious wounds mass
                new Common.SpellId( "c3d2294a6740bc147870fff652f3ced5", 8), //death clutch
                new Common.SpellId( "e788b02f8d21014488067bdd3ba7b325", 8), //frightfull aspect
                new Common.SpellId( "08323922485f7e246acb3d2276515526", 8), //horrid wilting
                new Common.SpellId( "820170444d4d2a14abc480fcbdb49535", 8), //inflict serious wounds mass
                new Common.SpellId( "f958ef62eea5050418fb92dfa944c631", 8), //power word stun
                new Common.SpellId( "0e67fa8f011662c43934d486acc50253", 8), //prediction of failure
                new Common.SpellId( "80a1a388ee938aa4e90d427ce9a7a3e9", 8), //ressurection
                new Common.SpellId( "7cfbefe0931257344b2cb7ddc4cdff6f", 8), //stormbolts
                new Common.SpellId( "d3ac756a229830243a72e84f3ab050d0", 8), //summon monster 8
                new Common.SpellId( "df2a0ba6b6dcecf429cbb80a56fee5cf", 8), //mind blank

                new Common.SpellId( "1f173a16120359e41a20fc75bb53d449", 9), //cure critical wounds mass
                new Common.SpellId( "3c17035ec4717674cae2e841a190e757", 9), //dominate monster
                new Common.SpellId( "43740dab07286fe4aa00a6ee104ce7c1", 9), //heroic invocation
                new Common.SpellId( "0340fe43f35e7a448981b646c638c83d", 9), //elemental swarm
                new Common.SpellId( "5ee395a2423808c4baf342a4f8395b19", 9), //inflict critical wounds mass
                new Common.SpellId( "1f01a098d737ec6419aedc4e7ad61fdd", 9), //foresight
                new Common.SpellId( "87a29febd010993419f2a4a9bee11cfc", 9), //mind blank communal
                new Common.SpellId( "ba48abb52b142164eba309fd09898856", 9), //polar midnight
                new Common.SpellId( "2f8a67c483dfa0f439b293e094ca9e3c", 9), //power word kill
                new Common.SpellId( "52b5df2a97df18242aec67610616ded0", 9), //summon monster 9
                new Common.SpellId( "b24583190f36a8442b212e45226c54fc", 9) //wail of banshee
            };
            
            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(witch_spellbook.SpellList, spell_id.level);
            }

            HexEngine.hex_vulnerability_spell.AddSpellAndScroll("e236e280f8be487428dcc09fe44dd5fd"); //hold person
            return witch_spellbook;
        }


        static BlueprintFeature createWitchPatronFeature(string name, string spell_list_guid, string feature_guid, params string[] spell_guids)
        {
            var extra_spell_list = new Common.ExtraSpellList(spell_guids);
            var learn_spell_list = extra_spell_list.createLearnSpellList("Witch" + name + "PatronSpellList", spell_list_guid, witch_class);
            string description = name + " patron grants witch the following spells: ";
            for (int i = 1; i <= 9; i++)
            {
                description += learn_spell_list.SpellList.SpellsByLevel[i].Spells[0].Name + ((i == 9) ? "" :", ");
            }
            description += ".";

            return Helpers.CreateFeature("Witch" + name + "PatronFeature",
                                  name + " Patron",
                                  description,
                                  feature_guid,
                                  null,
                                  FeatureGroup.None,
                                  learn_spell_list);
        }

        static BlueprintCharacterClass[] getWitchArray()
        {
            return new BlueprintCharacterClass[] { witch_class };
        }


        static void createHealing()
        {
            healing = hex_engine.createHealing("WitchHealing", "Healing",
                                        "A witch can soothe the wounds of those she touches.\n"
                                         + "Effect: This acts as a cure light wounds spell, using the witch’s caster level.Once a creature has benefited from the healing hex, it cannot benefit from it again for 24 hours.At 5th level, this hex acts like cure moderate wounds.",
                                        "a9f6f1aa9d46452aa5720c472b8926e2",
                                        "8ea243ac42aa4959ba131cbd5ff0118b",
                                        "a9d436988d044916b7bf61a58725725b",
                                        "6fe1054367f149939edc7f576d157bfa",
                                        "abec18ed55414a52a6d09457b734a5ca",
                                        "67e655e5a20640519d387e08298de728");
        }


        static void createBeastOfIllOmen()
        {
            beast_of_ill_omen = hex_engine.createBeastOfIllOmen("BeastOfIllOmen", "Beast of Ill-Omen",
                                                                "The witch imbues her familiar with strange magic, putting a minor curse upon the next enemy to see it.\n"
                                                                + "Effect: The target enemy must make a Will save or be affected by bane (caster level equal to the witch’s level).\n"
                                                                + " Whether or not the target’s save is successful, the creature cannot be the target of the bane effect for 1 day(later uses of this hex ignore that creature when determining who is affected).",
                                                                "c19d55421e6f436580423fffc78c11bd",
                                                                "fb3278a3b552414faaecb4189818b32e",
                                                                "ef6b3d4ad22644628aacfd3eaa4783e9"
                                                                );
        }


        static void createSlumber()
        {
            slumber_hex = hex_engine.createSlumber("Slumber", "Slumber",
                                                   "Effect: A witch can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "This hex can affect a creature of any HD. The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.",
                                                   "31f0fa4235ad435e95ebc89d8549c2ce",
                                                   "c086eeb69a4442df9c4bb8469a2c362d",
                                                   "0ccdbefa7f304a5788c4369b0a988e21");
        }


        static void createMisfortune()
        {
            misfortune_hex = hex_engine.createMisfortune("Misfortune", "Misfortune",
                                                         "Effect: The witch can cause a creature within 30 feet to suffer grave misfortune for 1 round. Anytime the creature makes an ability check, attack roll, saving throw, or skill check, it must roll twice and take the worse result. A Will save negates this hex. At 8th level and 16th level, the duration of this hex is extended by 1 round. This hex affects all rolls the target must make while it lasts. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.",
                                                         "08b6595f503f4d3c973424c217f7610e",
                                                         "",
                                                         "d7d51941f7684c4c92eb2232a5dd600f",
                                                         "3c8c06c506cd45d29e35e2e6507c659a");
        }


        static void createFortuneHex()
        {
            fortune_hex = hex_engine.createFortuneHex("Fortune", "Fortune",
                                                      "The witch can grant a creature within 30 feet a bit of good luck for 1 round. The target can call upon this good luck once per round, allowing him to reroll any ability check, attack roll, saving throw, or skill check, taking the better result. He must decide to use this ability before the first roll is made. At 8th level and 16th level, the duration of this hex is extended by 1 round. Once a creature has benefited from the fortune hex, it cannot benefit from it again for 24 hours.",
                                                      "986017ba2747495cb29cd37432140391",
                                                      "",
                                                      "b308ce2d429d4c1ea048fb7a69f65002",
                                                      "ffaf306fa3aa41e183dba5866bee9210");
        }


        static void createIceplantHex()
        {
            iceplant_hex = hex_engine.createIceplantHex("Iceplant", "Iceplant",
                                                        "This hex grants the witch and her familiar a +2 natural armor bonus and the constant effects of endure elements.\n"
                                                        + "The effect leaves the witch’s skin thick and stiff to the touch.",
                                                        "9828e12570414e6eb4cf42e00f303eab");
        }


        static void createMurksightHex()
        {
            murksight_hex = hex_engine.createMurksightHex("Murksight", "Murksight",
                                                          "The witch receives blindsight up to 15 feet.",
                                                          "e860bd889e494cd583b59bc5df42e7ef");
        }


        static void createAmeliorating()
        {
            ameliorating = hex_engine.createAmeliorating("Ameliorating", "Ameliorating",
                                                         "The witch can touch a creature to suppress or protect it from negative conditions. The hex effects the following conditions: dazzled, fatigued, shaken, or sickened. If the target is afflicted with these conditions, they are suppressed for a number of minutes equal to the witch’s level. Alternatively, the witch can grant her target a +4 circumstance bonus on saving throws against effects that cause any of the listed conditions for 24 hours. Once a creature has benefited from this hex, it cannot benefit from the hex again for 24 hours.",
                                                         "414d6c8fd5fc46c5a83b596a9bcf3322",
                                                         "a88b55579cd44dd5b78a27dcc6862ae1",
                                                         "09e35a74b871478eb1fb9480eae1f773",
                                                         "e7861746a94347cabec03c43b13f1223",
                                                         "cdf8ea7144b0454786273b68c97f3cfe",
                                                         "b58d93d94e834ada903a91d8cf46d650");
        }


        static void createEvilEye()
        {
            evil_eye = hex_engine.createEvilEye("EvilEye", "Evil Eye",
                                                "The witch can cause doubt to creep into the mind of a foe within 30 feet that she can see.\n"
                                                + "Effect: The target takes a –2 penalty on one of the following(witch’s choice): AC, attack rolls or saving throws. This hex lasts for a number of rounds equal to 3 + the witch’s Intelligence modifier.A Will save reduces this to just 1 round.\n"
                                                + "This is a mind - affecting effect. At 8th level the penalty increases to –4.",
                                                "1c8855dc3c9846a8addb4db4375eafe8", "ad14718b3f65491183dd97c4b9f57246", "cb406009170b447489b32d5b43d88f3f",
                                                "7a23bee9c3b04801bcd08ab8ef369341",
                                                "4a8fcd47dc9244f7ac6deb8dd9b741e2", "a11119164c7d430f81f6f3ec15e56e44", "2598782fac2a44b6a607f5d0d2a059bb");
        }


        static void createSummerHeat()
        {
            summer_heat = hex_engine.createSummerHeat("WitchSummerHeat", "Summer's Heat",
                                                      "Effect: The witch surrounds her target with oppressive heat, dealing a number of points of nonlethal damage equal to her witch level and causing the target to become fatigued. The target can attempt a Fortitude save to reduce this nonlethal damage by half and negate the fatigued condition. Whether or not the target succeeds at this save, it can’t be the target of this hex again for 1 day.",
                                                       "008a70774dbf48058810c565dad93fce",
                                                       "0c2bce51244647329e7e753b07548d10",
                                                       "383672cfa7804b20b3c27d20cf62bc73",
                                                       "7e5ed75385894376833e5dda98b12f8d",
                                                       "4ade4bcdcef14344b7f09c99fb60a672");
        }


        static void createMajorHealing()
        {
            major_healing = hex_engine.createMajorHealing("WitchMajorHealing", "Major Healing",
                                                         "By calling upon eerie powers, the witch’s touch can mend even the most terrible wounds of those she touches.\n"
                                                                        + "Effect: This hex acts as cure serious wounds, using the witch’s caster level.Once a creature has benefited from the major healing hex, it cannot benefit from it again for 24 hours.At 15th level, this hex acts like cure critical wounds.",
                                                         "8edb4d0c196a45c3a19a9b15272690ed",
                                                         "20fd1e6465a74433b9da13ef44a9c1bf",
                                                         "b247577f3d1b4abba1b412c3b694c741",
                                                         "2cffb51f4dcf4eec90d70cef7157ceca",
                                                         "b5614740fd784ec5b8e68bc9fbd4f0bd",
                                                         "8850ad9fb25148bb8732ca19076b15ec");
        }


        static void createMajorAmeliorating()
        {
            major_ameliorating = hex_engine.createMajorAmeliorating("MajorAmeliorating", "Major Ameliorating",
                                                                    "The witch can touch a creature to suppress or protect it from more debilitating negative conditions. The witch can remove blinded, curse, disease, or poison condition. The witch must succeed on caster level check (1d20 + caster level) against the DC of each condition to remove it. Alternatively, for 24 hours the witch can grant her target a +4 circumstance bonus on saving throws against effects that cause any of the above conditions or effects. Once a creature has benefited from this hex, it cannot benefit from it again for 24 hours.",
                                                                    "6c9027c8518b4e5abb5e6910b1bfd929",
                                                                    "b9e4db8a1dad4f3a86576a31c665fc93",
                                                                    "2c4d412a48844be184f2bd2ecc587ea6",
                                                                    "0d1b469f355b44129ef4236d0feabfce",
                                                                    "c580d3007f4948b38150aa347b9d9a9f");
        }


        static void createAnimalSkin()
        {
            animal_skin = hex_engine.createAnimalSkin("WitchAnimalSkin", "Animal Skin",
                                                      "The witch can become any animal of a size from Medium to Large. This ability is similar to beast shape II.",
                                                      "db93b8d7ae754858a81da32c121036a4",
                                                      "",
                                                      "",
                                                      "3c7752fbba7949dfb8ff07044c4db11d");
        }


        static void createAgony()
        {
            agony = hex_engine.createAgony("WitchAgony", "Agony",
                                            "Effect: With a quick incantation, a witch can place this hex on one creature within 60 feet, causing them to suffer intense pain. The target is nauseated for a number of rounds equal to the witch’s level. A Fortitude save negates this effect. If the saving throw is failed, the target can attempt a new save each round to end the effect. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.",
                                            "0229165c289947968a20550817524590",
                                            "824364df0dc3420d98c699f95dc250c0",
                                            "77d75c4d05a94141a461b29198a59f0b",
                                            "512e6a19428047038a4abd8ee368dc99");
        }


        static void createBeastGift()
        {
            beast_gift = hex_engine.createBeastGift("WitchBeastGift", "Beast’s Gift",
                                                    "Effect: The witch can use her magic to grant her allies ferocious animal abilities.The witch can partially transform a willing ally, granting him one bite attack dealing 1d8 points of damage for a number of minutes equal to the witch’s level. Once a creature has benefited from this hex, it cannot benefit from it again for 24 hours.",
                                                    "b87be54ce5ef465786f74d89efa53678",
                                                    "9007fc6505104436b01dfc7f989e82c4",
                                                    "fb4abd4dec1f4a029ebbc39c28139e8c",
                                                    "0dde3620e1394115a8785454170d8108");
        }


        static void createHarrowingCurse()
        {
            harrowing_curse = hex_engine.createHarrowingCurse("WitchHarrowingCurse", "Harrowing Curse",
                                                               "Effect: The witch can curse a target creature by touching it with a card randomly drawn from a harrow deck she owns. The target is affected as if by the spell bestow curse using the witch’s caster level, except that the witch can decrease only the ability score that corresponds to the suit of the card drawn. Whether or not the save is successful, a creature cannot be targeted by this hex more than once in 24 hours.",
                                                               "39cd09cc131e40b49ca20213094d1190",
                                                               "5ca58e3854444a869808889a3cbf20d9",
                                                               "b706bf0a946d4d1d9a49040efb3595c9");
        }


        static void createRetribution()
        {
            retribution = hex_engine.createRetribution("WitchRetribution", "Retribution",
                                                       "Effect: A witch can place a retribution hex on a creature within 60 feet, causing terrible wounds to open across the target’s flesh whenever it deals damage to another creature in melee.Immediately after the hexed creature deals damage in melee, it takes half that damage(round down).This damage bypasses any resistances, immunities, or damage reduction the creature possesses.This effect lasts for a number of rounds equal to the witch’s Intelligence modifier.A Will save negates this effect.",
                                                       "",
                                                       "",
                                                       "");
        }


        static void createIceTomb()
        {
            ice_tomb = hex_engine.createIceTomb("WitchIceTomb", "Ice Tomb",
                                                "Effect: A storm of ice and freezing wind envelops the target, which takes 3d8 points of cold damage (Fortitude half). If the target fails its save, it is paralyzed and unconscious but does not need to eat or breathe while the ice lasts. Destroying the ice frees the creature, which is staggered for 1d4 rounds after being released. Whether or not the target’s saving throw is successful, it cannot be the target of this hex again for 1 day.",
                                                "a680c3c5fd7646499f1b7e8d95b0f5df",
                                                "7e75ef2ae6984d80a98f47f7a5a2a8a8",
                                                "f983760c33db49b5ae58e3c60ad0014b",
                                                "99e48705252147f6a8c395394ba77faa");
        }


        static void createRegenerativeSinew()
        {
            regenerative_sinew = hex_engine.createRegenerativeSinew("WitchRegenerativeSinew", "Regenerative Sinew",
                                                                     "The witch can cause the debilitating wounds of a creature she touches to quickly close, helping it heal rapidly.\n"
                                                                     + "The target either gains fast healing 5 for a number of rounds equal to 1 / 2 the witch’s class level or it heals ability score damage for one ability score.\n"
                                                                     + "Once a creature has benefited from this hex, it cannot benefit from it again for 24 hours.",
                                                                     "6fab21ff649245babdb6f1155579f6b0",
                                                                     "03963bcf8dd64abea3757311c1e8a79c",
                                                                     "f6de4a3f23ec4e319f574e3900002919",
                                                                     "b7f1e7aee027492e8f474a1f5e419194",
                                                                     "a367881ee1704b1ab509758f5799369d");
        }

        static void createAnimalServant()
        {
            animal_servant = hex_engine.createAnimalServant("WitchAnimalServant", "Animal Servant",
                                                            "Effect: The witch can use this hex to turn a humanoid enemy into an animal and rob it of its free will.\n"
                                                            + "The transformation works as beast shape II and is negated by a successful Will save. The transformed creature retains its Intelligence score and known languages, if any, but the witch controls its mind. This effect functions as dominate monster, except the creature does not receive further saving throws to resist the hex.The effect can be removed only with wish or similar magic, although slaying the witch also ends the effect. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.",
                                                            "583e661fe4244a319672bc6ccdc51294",
                                                            "32b4b11964724f59a9034e61014dbb3c",
                                                            "92859cd9f42a4fae95462e27e3a940fb",
                                                            "73b1287891e7441bbfc663c5f937083c");
        }


        static void createDeathCurse()
        {
            death_curse = hex_engine.createDeathCurse("WitchDeathCurse", "Death Curse",
                                                      "This powerful hex seizes a creature’s heart, causing death within just a few moments.\n"
                                                      + "Effect: This hex has a range of 30 feet. The hexed creature receives a Will save to negate the effect. If this save is failed, the creature becomes fatigued the first round of the hex. On the second round of the hex, the creature becomes exhausted. On the third round, the creature dies unless it succeeds at a Fort save. Creatures that fail the first save but succeed at the second remain exhausted and take 4d6 points of damage + 1 point of damage per level of the witch. Slaying the witch that hexed the creature ends the effect, but any fatigue or exhaustion remains. Whether or not the saves are successful, a creature cannot be the target of this hex again for 1 day.",
                                                      "6913bcf974004951a0542e906b4c201c",
                                                      "617290b83ca04f01adc23e0416758dfb",
                                                      "cf27f36d30cd4ce8baaa3a52cf9e08f1",
                                                      "f172135df37a40e8aa7cb7be29d2a72d");
        }


        static void createLayToRest()
        {
            lay_to_rest = hex_engine.createLayToRest("WitchLayToRest", "Lay to Rest",
                                                     "Effect: The witch may target a single undead creature with this hex as if with an undeath to death spell. A Will save negates this effect. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.",
                                                     "948b588bb57d4ef1bf96940c0bba95c9",
                                                     "3fcdc34afbb74b15a4236740d299afaf",
                                                     "8a0733cfc0844297b4db7f3351714744");
        }


        static void createLifeGiver()
        {
            life_giver = hex_engine.createLifeGiver("WitchLifeGiver", "Life Giver",
                                                    "Effect: Once per day the witch can, as a full round action, touch a dead creature and bring it back to life. This functions as resurrection, but it does not require a material component.",
                                                    "7924a8779bc442c2b5cfd472f9ba028f",
                                                    "272749f543954f77a4180370207e1159",
                                                    "34e471a5196247e4b2daecf9bc38c105");
        }


        static void createEternalSlumber()
        {
            eternal_slumber = hex_engine.createEternalSlumber("WitchEternalSlumber", "Eternal Slumber",
                                                               "The witch can touch a creature, causing it to drift off into a permanent slumber.\n"
                                                               + "Effect: The creature receives a Will save to negate this effect. If the save fails, the creature falls asleep and cannot be woken. The effect can only be removed with a wish or similar magic, although slaying the witch ends the effect. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.",
                                                               "b03f4347c1974e38acff99a2af092461",
                                                               "0a2763d71f274a25b053647ea5053b40",
                                                               "8e7292d4fb9346a3bc71f653d539d0ca",
                                                               "2214659c18824be4af8a662485b6f341");
        }


        static void createCackle()
        {
            cackle = hex_engine.createCackle("WitchCackle", "Cackle",
                                             "Effect: A witch can cackle madly as a move action. Any creature that is within 30 feet that is under the effects of an agony hex, charm hex, evil eye hex, fortune hex, or misfortune hex caused by the witch has the duration of that hex extended by 1 round.",
                                             "",
                                             "",
                                             "",
                                             "",
                                             "CreateCackleToggleAbility");
        }


        static void createExtraHexFeat()
        {
            var extra_hex_feat_selection = Helpers.CreateFeatureSelection("ExtraHexFeat",
                                                            "Extra Hex",
                                                            "You gain one additional hex. You must meet the prerequisites for this hex. If you are a shaman, it must be a hex granted by your spirit rather than one from a wandering spirit.\n"
                                                            + "Special: You can take this feat multiple times. Each time you do, you gain another hex.",
                                                            "5d3b5b72afb940d4b9aab740d8925b53",
                                                            null,
                                                            FeatureGroup.Feat,
                                                            Helpers.PrerequisiteClassLevel(witch_class, 1));
            extra_hex_feat_selection.AllFeatures = hex_selection.Features;
            extra_hex_feat_selection.Features = hex_selection.Features;
            extra_hex_feat = extra_hex_feat_selection;
            extra_hex_feat.Ranks = 10;
            extra_hex_feat.Groups = new FeatureGroup[] { FeatureGroup.Feat };
            library.AddFeats(extra_hex_feat);
        }



    }
}
