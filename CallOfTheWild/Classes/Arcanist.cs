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
using Kingmaker.Blueprints.Root;
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
    public class Arcanist
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass arcanist_class;
        static public BlueprintProgression arcanist_progression;
        static public BlueprintSpellbook memorization_spellbook;
        static public BlueprintSpellbook arcanist_spellbook;
        static public BlueprintFeature arcanist_proficiencies;
        static public BlueprintFeature arcanist_cantrips;

        static public BlueprintAbilityResource arcane_reservoir_resource;
        static public BlueprintFeature arcane_reservoir;
        static public BlueprintActivatableAbility arcane_reservoir_spell_dc_boost;
        static public BlueprintActivatableAbility arcane_reservoir_caster_level_boost;
        static public BlueprintFeature consume_spells;

        static public BlueprintFeatureSelection arcane_exploits;
        static public BlueprintFeature quick_study;


        internal static void createArcanistClass()
        {
            Main.logger.Log("Arcanist class test mode: " + test_mode.ToString());
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

            arcanist_class = Helpers.Create<BlueprintCharacterClass>();
            arcanist_class.name = "ArcanistClass";
            library.AddAsset(arcanist_class, "19c3cf3d51cf4cbf9a136a600c26585a");

            arcanist_class.LocalizedName = Helpers.CreateString("Arcanist.Name", "Arcanist");
            arcanist_class.LocalizedDescription = Helpers.CreateString("Arcanist.Description",
                "Some spellcasters seek the secrets of magic, pursuing the power to make the impossible possible. Others are born with magic in their blood, commanding unbelievable forces as effortlessly as they breathe. Yet still others seek to meld the science of arcane scholars with the natural might of innate casters. These arcanists seek to discover the mysterious laws of magic and through will and expertise bend those forces to their whims. Arcanists are the shapers and tinkers of the arcane world, and no magic can resist their control.\n"
                + "Role: Arcanists are scholars of all things magical. They constantly seek out new forms of magic to discover how they work, and in many cases, to collect the energy of such magic for their own uses. Many arcanists are seen as reckless, more concerned with the potency of magic than the ramifications of unleashing such power."
                );
            arcanist_class.m_Icon = wizard_class.Icon;
            arcanist_class.SkillPoints = wizard_class.SkillPoints;
            arcanist_class.HitDie = DiceType.D6;
            arcanist_class.BaseAttackBonus = wizard_class.BaseAttackBonus;
            arcanist_class.FortitudeSave = wizard_class.ReflexSave;
            arcanist_class.ReflexSave = wizard_class.ReflexSave;
            arcanist_class.WillSave = wizard_class.WillSave;
            arcanist_class.Spellbook = createArcanistSpellbook();
            arcanist_class.ClassSkills = wizard_class.ClassSkills.AddToArray(StatType.SkillUseMagicDevice);
            arcanist_class.IsDivineCaster = false;
            arcanist_class.IsArcaneCaster = true;
            arcanist_class.StartingGold = wizard_class.StartingGold;
            arcanist_class.PrimaryColor = wizard_class.PrimaryColor;
            arcanist_class.SecondaryColor = wizard_class.SecondaryColor;
            arcanist_class.RecommendedAttributes = new StatType[] { StatType.Intelligence, StatType.Charisma };
            arcanist_class.NotRecommendedAttributes = new StatType[0];
            arcanist_class.EquipmentEntities = wizard_class.EquipmentEntities;
            arcanist_class.MaleEquipmentEntities = wizard_class.MaleEquipmentEntities;
            arcanist_class.FemaleEquipmentEntities = wizard_class.FemaleEquipmentEntities;
            arcanist_class.ComponentsArray = wizard_class.ComponentsArray;
            arcanist_class.StartingItems = wizard_class.StartingItems;

            createArcanistProgression();
            arcanist_class.Progression = arcanist_progression;

            Helpers.RegisterClass(arcanist_class);
        }


        static void createConsumeSpells()
        {
            var icon = library.Get<BlueprintFeature>("bfbaa0dd74b9909459e462cd8b091177").Icon;

            List<BlueprintAbility> consume_abilities = new List<BlueprintAbility>();
            consume_abilities.Add(null);

            for (int i = 1; i <= 9; i++)
            {
                var ability = Helpers.CreateAbility($"ConsumeSpells{i}Ability",
                                                    "Consume Spells " + Common.roman_id[i],
                                                    "At 1st level, an arcanist can expend an available arcanist spell slot as a move action, making it unavailable for the rest of the day, just as if she had used it to cast a spell. She can use this ability a number of times per day equal to her Charisma modifier (minimum 1). Doing this adds a number of points to her arcane reservoir equal to the level of the spell slot consumed. She cannot consume cantrips (0 level spells) in this way. Points gained in excess of the reservoir’s maximum are lost.",
                                                    "",
                                                    icon,
                                                    AbilityType.Special,
                                                    CommandType.Move,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => { c.amount = 1; c.Resource = arcane_reservoir_resource; }))
                                                    );
                ability.setMiscAbilityParametersSelfOnly();
                consume_abilities.Add(ability);
            }

            consume_spells = Helpers.CreateFeature("ConsumeSpellsFeature",
                                                   "Consume Spells",
                                                   consume_abilities[1].Description,
                                                   "",
                                                   icon,
                                                   FeatureGroup.None,
                                                   Helpers.Create<NewMechanics.SpontaneousSpellConversionForSpellbook>(s => { s.spellbook = arcanist_spellbook; s.SpellsByLevel = consume_abilities.ToArray(); })
                                                   );
        }


        static void createArcaneReservoir()
        {
            arcane_reservoir_resource = Helpers.CreateAbilityResource("ArcaneReservoirFullResource", "", "", "", null);
            arcane_reservoir_resource.SetIncreasedByStat(0, StatType.Charisma);
            arcane_reservoir_resource.SetIncreasedByLevel(3, 1, getArcanistArray());


            var arcane_reservoir_partial_resource = Helpers.CreateAbilityResource("ArcaneReservoirPartialResource", "", "", "", null);
            arcane_reservoir_partial_resource.SetIncreasedByStat(0, StatType.Charisma);
            arcane_reservoir_partial_resource.SetIncreasedByLevelStartPlusDivStep(3, 2, 1, 2,1, 0, 0.0f, getArcanistArray());
            arcane_reservoir_resource.AddComponent(Helpers.Create<ResourceMechanics.FakeResourceAmountFullRestore>(f => f.fake_resource = arcane_reservoir_partial_resource));

            var icon = library.Get<BlueprintFeature>("55edf82380a1c8540af6c6037d34f322").Icon; //elven magic

            var dc_buff = Helpers.CreateBuff("ArcaneReservoirSpellDCBuff",
                                             "Spell DC Increase",
                                             "The arcanist can expend 1 point from her arcane reservoir as a free action whenever she casts an arcanist spell. If she does, she can choose to increase the caster level by 1 or increase the spell’s DC by 1. She can expend no more than 1 point from her reservoir on a given spell in this way.",
                                             "",
                                             icon,
                                             null,
                                             Helpers.Create<NewMechanics.IncreaseAllSpellsDCForSpecificSpellbook>(i => { i.spellbook = arcanist_spellbook; i.Value = 1; }),
                                             Helpers.Create<NewMechanics.SpendResourceOnSpellCast>(s => { s.spellbook = arcanist_spellbook; s.resource = arcane_reservoir_resource; })
                                             );
            arcane_reservoir_spell_dc_boost = Helpers.CreateActivatableAbility("ArcaneReservoirSpellDCToggleAbility",
                                                                               dc_buff.Name,
                                                                               dc_buff.Description,
                                                                               "",
                                                                               dc_buff.Icon,
                                                                               dc_buff,
                                                                               AbilityActivationType.Immediately,
                                                                               CommandType.Free,
                                                                               null,
                                                                               Helpers.CreateActivatableResourceLogic(arcane_reservoir_resource, ResourceSpendType.Never)
                                                                               );
            arcane_reservoir_spell_dc_boost.Group = ActivatableAbilityGroupExtension.ArcanistArcaneReservoirSpellboost.ToActivatableAbilityGroup();
            arcane_reservoir_spell_dc_boost.DeactivateImmediately = true;


            var cl_buff = Helpers.CreateBuff("ArcaneReservoirSpellCLBuff",
                                 "Spell Caster Level Increase",
                                 "The arcanist can expend 1 point from her arcane reservoir as a free action whenever she casts an arcanist spell. If she does, she can choose to increase the caster level by 1 or increase the spell’s DC by 1. She can expend no more than 1 point from her reservoir on a given spell in this way.",
                                 "",
                                 icon,
                                 null,
                                 Helpers.Create<NewMechanics.IncreaseAllSpellsCLForSpecificSpellbook>(i => { i.spellbook = arcanist_spellbook; i.Value = 1; }),
                                 Helpers.Create<NewMechanics.SpendResourceOnSpellCast>(s => { s.spellbook = arcanist_spellbook; s.resource = arcane_reservoir_resource; })
                                 );

            arcane_reservoir_caster_level_boost = Helpers.CreateActivatableAbility("ArcaneReservoirSpellCLToggleAbility",
                                                                                   cl_buff.Name,
                                                                                   cl_buff.Description,
                                                                                   "",
                                                                                   cl_buff.Icon,
                                                                                   cl_buff,
                                                                                   AbilityActivationType.Immediately,
                                                                                   CommandType.Free,
                                                                                   null,
                                                                                   Helpers.CreateActivatableResourceLogic(arcane_reservoir_resource, ResourceSpendType.Never)
                                                                                   );
            arcane_reservoir_caster_level_boost.Group = ActivatableAbilityGroupExtension.ArcanistArcaneReservoirSpellboost.ToActivatableAbilityGroup();
            arcane_reservoir_caster_level_boost.DeactivateImmediately = true;

            arcane_reservoir = Helpers.CreateFeature("ArcaneReservoirFeature",
                                                     "Arcane Reservoir",
                                                     "An arcanist has an innate pool of magical energy that she can draw upon to fuel her arcanist exploits and enhance her spells. The arcanist’s arcane reservoir can hold a maximum amount of magical energy equal to 3 + the arcanist’s level. Each day, when preparing spells, the arcanist’s arcane reservoir fills with raw magical energy, gaining a number of points equal to 3 + 1/2 her arcanist level. Any points she had from the previous day are lost. She can also regain these points through the consume spells class feature and some arcanist exploits. The arcane reservoir can never hold more points than the maximum amount noted above; points gained in excess of this total are lost.\n"
                                                     + "Points from the arcanist reservoir are used to fuel many of the arcanist’s powers. In addition, the arcanist can expend 1 point from her arcane reservoir as a free action whenever she casts an arcanist spell. If she does, she can choose to increase the caster level by 1 or increase the spell’s DC by 1. She can expend no more than 1 point from her reservoir on a given spell in this way.",
                                                     "",
                                                     icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddAbilityResource(arcane_reservoir_resource),
                                                     Helpers.CreateAddFacts(arcane_reservoir_spell_dc_boost, arcane_reservoir_caster_level_boost)
                                                     );
        }


        static BlueprintCharacterClass[] getArcanistArray()
        {
            return new BlueprintCharacterClass[] { arcanist_class };
        }


        static void createArcanistProgression()
        {
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            arcanist_progression = Helpers.CreateProgression("ArcanistProgression",
                                                               arcanist_class.Name,
                                                               arcanist_class.Description,
                                                               "",
                                                               arcanist_class.Icon,
                                                               FeatureGroup.None,
                                                               Helpers.Create<SpellManipulationMechanics.ArcanistPreparedMetamagicNoSpellCastingTimeIncrease>(a => a.spellbook = arcanist_spellbook));
            arcanist_progression.Classes = getArcanistArray();

            arcanist_proficiencies = library.CopyAndAdd<BlueprintFeature>("25c97697236ccf2479d0c6a4185eae7f", "ArcanistProficiencies", "");
            arcanist_proficiencies.SetNameDescription("Arcanist Proficiencies",
                                                      "Arcanists are proficient with all simple weapons. They are not proficient with any type of armor or shield. Armor interferes with an arcanist’s gestures, which can cause her spells with somatic components to fail.");
            arcanist_cantrips = library.CopyAndAdd<BlueprintFeature>("c58b36ec3f759c84089c67611d1bcc21", "ArcanistCantrips", "");
            arcanist_cantrips.SetNameDescription("Arcanist Cantrips",
                                                 "Arcanists can cast a number of cantrips, or 0-level spells. These spells are cast like any other spell, but they are not expended when cast and may be used again.");
            arcanist_cantrips.ReplaceComponent<LearnSpells>(l => l.CharacterClass = arcanist_class);
            arcanist_cantrips.ReplaceComponent<BindAbilitiesToClass>(b => { b.CharacterClass = arcanist_class; b.Stat = StatType.Intelligence; });

            createArcaneReservoir();
            createConsumeSpells();

            arcanist_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, arcanist_proficiencies, detect_magic, arcanist_cantrips,
                                                                                        arcane_reservoir,
                                                                                        consume_spells,
                                                                                        library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee"), //inside the storm
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),//touch calculate feature};
                                                                    Helpers.LevelEntry(2),
                                                                    Helpers.LevelEntry(3),
                                                                    Helpers.LevelEntry(4),
                                                                    Helpers.LevelEntry(5),
                                                                    Helpers.LevelEntry(6),
                                                                    Helpers.LevelEntry(7),
                                                                    Helpers.LevelEntry(8),
                                                                    Helpers.LevelEntry(9),
                                                                    Helpers.LevelEntry(10),
                                                                    Helpers.LevelEntry(11),
                                                                    Helpers.LevelEntry(12),
                                                                    Helpers.LevelEntry(13),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18),
                                                                    Helpers.LevelEntry(19),
                                                                    Helpers.LevelEntry(20)
                                                                    };

            arcanist_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { arcanist_proficiencies, detect_magic, arcanist_cantrips };

            arcanist_progression.UIGroups = new UIGroup[]  {
                                                           };
        }


        static BlueprintSpellbook createArcanistSpellbook()
        {
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

            memorization_spellbook = Helpers.Create<BlueprintSpellbook>();
            memorization_spellbook.Name = Helpers.CreateString("ArcanistMemorizationSpellbook.Name", "Arcanist (Prepared)");
            memorization_spellbook.name = "ArcanistMemorizationSpellbook";
            library.AddAsset(memorization_spellbook, "ab76417567444a6cb87d9d53e9752955");

            memorization_spellbook.Spontaneous = false;
            memorization_spellbook.IsArcane = true;
            memorization_spellbook.AllSpellsKnown = false;
            memorization_spellbook.CanCopyScrolls = true;
            memorization_spellbook.CastingAttribute = StatType.Intelligence;
            memorization_spellbook.CharacterClass = arcanist_class;
            memorization_spellbook.CasterLevelModifier = 0;
            memorization_spellbook.CantripsType = CantripsType.Cantrips;
            memorization_spellbook.SpellsPerLevel = 2;
            memorization_spellbook.SpellsPerDay =  Common.createSpellsTable("ArcanistSpellsKnown", "",
                                                                       Common.createSpellsLevelEntry(),  //0
                                                                       Common.createSpellsLevelEntry(0, 2),  //1
                                                                       Common.createSpellsLevelEntry(0, 2),  //2
                                                                       Common.createSpellsLevelEntry(0, 3),  //3
                                                                       Common.createSpellsLevelEntry(0, 3, 1), //4
                                                                       Common.createSpellsLevelEntry(0, 4, 2), //5
                                                                       Common.createSpellsLevelEntry(0, 4, 2, 1), //6
                                                                       Common.createSpellsLevelEntry(0, 5, 3, 2), //7
                                                                       Common.createSpellsLevelEntry(0, 5, 3, 2, 1), //8
                                                                       Common.createSpellsLevelEntry(0, 5, 4, 3, 2), //9
                                                                       Common.createSpellsLevelEntry(0, 5, 4, 3, 2, 1), //10
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 3, 2), //11
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 3, 2, 1), //12
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 3, 2), //13
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 3, 2, 1), //14
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 2), //15
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 2, 1), //16
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 3, 2), //17
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 3, 2, 1), //18
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 3, 2, 2), //19
                                                                       Common.createSpellsLevelEntry(0, 5, 5, 4, 4, 4, 3, 3, 3, 3) //20
                                                                       );

            memorization_spellbook.SpellList = wizard_class.Spellbook.SpellList;
            memorization_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.NoSpellsPerDaySacaling>());
            memorization_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.CanNotUseSpells>());

            arcanist_spellbook = Helpers.Create<BlueprintSpellbook>();
            arcanist_spellbook.Name = Helpers.CreateString("ArcanistSpellbook.Name", "Arcanist (Spontaneous)");
            arcanist_spellbook.name = "ArcanistSpellbook";
            library.AddAsset(arcanist_spellbook, "0c21cfcab6ce4395bd4df330ab3cf715");

            arcanist_spellbook.Spontaneous = true;
            arcanist_spellbook.IsArcane = true;
            arcanist_spellbook.AllSpellsKnown = false;
            arcanist_spellbook.CanCopyScrolls = false;
            arcanist_spellbook.CastingAttribute = StatType.Intelligence;
            arcanist_spellbook.CharacterClass = arcanist_class;
            arcanist_spellbook.CasterLevelModifier = 0;
            arcanist_spellbook.CantripsType = CantripsType.Cantrips;
            arcanist_spellbook.SpellsPerLevel = 0;
            arcanist_spellbook.SpellsKnown = Common.createEmptySpellTable("ArcanistKnownPerDaySpellTable", "");
            arcanist_spellbook.SpellsPerDay = Common.createSpellsTable("ArcanistSpellsPerDay", "",
                                                                       Common.createSpellsLevelEntry(),  //0
                                                                       Common.createSpellsLevelEntry(0, 2),  //1
                                                                       Common.createSpellsLevelEntry(0, 3),  //2
                                                                       Common.createSpellsLevelEntry(0, 4),  //3
                                                                       Common.createSpellsLevelEntry(0, 4, 2), //4
                                                                       Common.createSpellsLevelEntry(0, 4, 3), //5
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 2), //6
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 3), //7
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 2), //8
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 3), //9
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 2), //10
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 3), //11
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 2), //12
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 3), //13
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 2), //14
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 3), //15
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 4, 2), //16
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 4, 3), //17
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 4, 4, 2), //18
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 4, 4, 3), //19
                                                                       Common.createSpellsLevelEntry(0, 4, 4, 4, 4, 4, 4, 4, 4, 4) //20
                                                                       );

            arcanist_spellbook.SpellList = wizard_class.Spellbook.SpellList;
            memorization_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.CompanionSpellbook>(c => c.spellbook = arcanist_spellbook));
            arcanist_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.GetKnownSpellsFromMemorizationSpellbook>(c => c.spellbook = memorization_spellbook));

            return memorization_spellbook;
        }
    }
}
