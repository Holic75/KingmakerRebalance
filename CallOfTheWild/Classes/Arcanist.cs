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
        static public BlueprintFeature arcanist_spellcasting;

        static public BlueprintAbilityResource arcane_reservoir_resource;
        static public BlueprintFeature arcane_reservoir;
        static public BlueprintActivatableAbility arcane_reservoir_spell_dc_boost;
        static public BlueprintActivatableAbility arcane_reservoir_caster_level_boost;
        static public BlueprintFeature consume_spells;

        static public BlueprintFeatureSelection arcane_exploits;
        static public BlueprintFeature greater_arcane_exploits;
        static public BlueprintFeature quick_study;
        static public BlueprintFeature acid_jet;
        static public BlueprintFeature lingering_acid;
        static public BlueprintFeature arcane_barrier;
        
        static public BlueprintFeature arcane_weapon;
        static public ActivatableAbilityGroup arcane_weapon_group = ActivatableAbilityGroupExtension.ArcanistArcaneWeapon.ToActivatableAbilityGroup();
        static public BlueprintFeature energy_shield;
        static public BlueprintFeature energy_absorption;
        static public Dictionary<DamageEnergyType, BlueprintBuff> energy_absorption_buffs = new Dictionary<DamageEnergyType, BlueprintBuff>();


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
            var resource = Helpers.CreateAbilityResource("ConsumeSpellsResource", "", "", "", null);
            resource.AddComponent(Helpers.Create<ResourceMechanics.MinResourceAmount>(m => m.value = 1));
            resource.SetIncreasedByStat(0, StatType.Charisma);

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
                                                    Helpers.CreateRunActions(Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => { c.amount = 1; c.Resource = arcane_reservoir_resource; })),
                                                    Helpers.CreateResourceLogic(resource)
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
                                                   Helpers.CreateAddAbilityResource(resource),
                                                   Helpers.Create<NewMechanics.SpontaneousSpellConversionForSpellbook>(s => { s.spellbook = arcanist_spellbook; s.SpellsByLevel = consume_abilities.ToArray(); })
                                                   );
        }


        static void createArcaneReservoir()
        {
            arcane_reservoir_resource = Helpers.CreateAbilityResource("ArcaneReservoirFullResource", "", "", "", null);
            arcane_reservoir_resource.SetIncreasedByLevel(3, 1, getArcanistArray());


            var arcane_reservoir_partial_resource = Helpers.CreateAbilityResource("ArcaneReservoirPartialResource", "", "", "", null);
            arcane_reservoir_partial_resource.SetIncreasedByLevelStartPlusDivStep(3, 2, 1, 2, 1, 0, 0.0f, getArcanistArray());
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
                                                               Helpers.Create<SpellManipulationMechanics.ArcanistPreparedMetamagicNoSpellCastingTimeIncrease>());
            arcanist_progression.Classes = getArcanistArray();

            arcanist_proficiencies = library.CopyAndAdd<BlueprintFeature>("25c97697236ccf2479d0c6a4185eae7f", "ArcanistProficiencies", "");
            arcanist_proficiencies.SetNameDescription("Arcanist Proficiencies",
                                                      "Arcanists are proficient with all simple weapons. They are not proficient with any type of armor or shield. Armor interferes with an arcanist’s gestures, which can cause her spells with somatic components to fail.");
            arcanist_cantrips = library.CopyAndAdd<BlueprintFeature>("c58b36ec3f759c84089c67611d1bcc21", "ArcanistCantrips", "");
            arcanist_cantrips.SetNameDescription("Arcanist Cantrips",
                                                 "Arcanists can cast a number of cantrips, or 0-level spells. These spells are cast like any other spell, but they are not expended when cast and may be used again.");
            arcanist_cantrips.ReplaceComponent<LearnSpells>(l => l.CharacterClass = arcanist_class);
            arcanist_cantrips.ReplaceComponent<BindAbilitiesToClass>(b => { b.CharacterClass = arcanist_class; b.Stat = StatType.Intelligence; });

            createArcanistSpellCasting();
            createArcaneReservoir();
            createConsumeSpells();
            createArcaneExploits();

            arcanist_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, arcanist_proficiencies, detect_magic, arcanist_cantrips, 
                                                                                        arcanist_spellcasting,
                                                                                        arcane_reservoir,
                                                                                        consume_spells,
                                                                                        arcane_exploits,
                                                                                        library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee"), //inside the storm
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),//touch calculate feature};
                                                                    Helpers.LevelEntry(2),
                                                                    Helpers.LevelEntry(3, arcane_exploits),
                                                                    Helpers.LevelEntry(4),
                                                                    Helpers.LevelEntry(5, arcane_exploits),
                                                                    Helpers.LevelEntry(6),
                                                                    Helpers.LevelEntry(7, arcane_exploits),
                                                                    Helpers.LevelEntry(8),
                                                                    Helpers.LevelEntry(9, arcane_exploits),
                                                                    Helpers.LevelEntry(10),
                                                                    Helpers.LevelEntry(11, greater_arcane_exploits, arcane_exploits),
                                                                    Helpers.LevelEntry(12),
                                                                    Helpers.LevelEntry(13, arcane_exploits),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, arcane_exploits),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17, arcane_exploits),
                                                                    Helpers.LevelEntry(18),
                                                                    Helpers.LevelEntry(19, arcane_exploits),
                                                                    Helpers.LevelEntry(20)
                                                                    };

            arcanist_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { arcanist_proficiencies, detect_magic, arcanist_cantrips, arcanist_spellcasting };

            arcanist_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(arcane_reservoir, greater_arcane_exploits),
                                                            Helpers.CreateUIGroup(arcane_exploits)
                                                           };
        }


        static void createArcanistSpellCasting()
        {
            arcanist_spellcasting = Helpers.CreateFeature("ArcanistSpellCastingFeature",
                                                          "Arcanist Spell Casting",
                                                          "An arcanist casts arcane spells drawn from the sorcerer/wizard spell list. An arcanist must prepare her spells ahead of time, but unlike a wizard, her spells are not expended when they’re cast. Instead, she can cast any spell that she has prepared consuming a spell slot of the appropriate level, assuming she hasn’t yet used up her spell slots per day for that level.\n"
                                                          + "To learn, prepare, or cast a spell, the arcanist must have an Intelligence score equal to at least 10 + the spell’s level. The saving throw DC against an arcanist’s spell is 10 + the spell’s level + the arcanist’s Intelligence modifier.\n"
                                                          + "An arcanist can only cast a certain number of spells of each spell level per day. In addition, she receives bonus spells per day if she has a high Intelligence score.\n"
                                                          + "An arcanist may know any number of spells, but the number she can prepare each day is limited. Unlike the number of spells she can cast per day, the number of spells an arcanist can prepare each day is not affected by her Intelligence score. Feats and other effects that modify the number of spells known by a spellcaster instead affect the number of spells an arcanist can prepare.\n"
                                                          + "Like a sorcerer, an arcanist can choose to apply any metamagic feats she knows to a prepared spell as she casts it, with the same increase in casting time (see Spontaneous Casting and Metamagic Feats). However, she may also prepare a spell with any metamagic feats she knows and cast it without increasing casting time like a wizard. She cannot combine these options—a spell prepared with metamagic feats cannot be further modified with another metamagic feat at the time of casting (unless she has the metamixing arcanist exploit).",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Helpers.Create<SpellManipulationMechanics.InitializeArcanistPart>(i => i.spellbook = arcanist_spellbook)
                                                          );
        }


        static void createArcaneExploits()
        {
            var icon = library.Get<BlueprintFeature>("30f20e6f850519b48aa59e8c0ff66ae9").Icon;
            var greater_icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/ArcaneExploit.png");
            arcane_exploits = Helpers.CreateFeatureSelection("ArcaneExploitsFeatureSelection",
                                                             "Arcanist Exploits",
                                                             "By bending and sometimes even breaking the rules of magic, the arcanist learns to exploit gaps and exceptions in the laws of magic. Some of these exploits allow her to break down various forms of magic, adding their essence to her arcane reservoir. At 1st level and every 2 levels thereafter, the arcanist learns a new arcane exploit selected from the following list. An arcanist exploit cannot be selected more than once. Once an arcanist exploit has been selected, it cannot be changed. Most arcanist exploits require the arcanist to expend points from her arcane reservoir to function. Unless otherwise noted, the saving throw DC for an arcanist exploit is equal to 10 + 1/2 the arcanist’s level + the arcanist’s Charisma modifier.",
                                                             "",
                                                             icon,
                                                             FeatureGroup.None);
            greater_arcane_exploits = Helpers.CreateFeatureSelection("GreateArcaneExploitsFeature",
                                                             "Greater Exploits",
                                                             "At 11th level and every 2 levels thereafter, an arcanist can choose one of the greater exploits in place of an arcanist exploit.",
                                                             "",
                                                             greater_icon,
                                                             FeatureGroup.None);
            createQuickStudy();           
            createArcaneBarrier();
            createArcaneWeapon();
            createEnergyShieldAndEnergyAbsorption();

            createAcidJetAndLingeringAcid();


            arcane_exploits.AllFeatures = new BlueprintFeature[] { quick_study, arcane_barrier, arcane_weapon, acid_jet, energy_shield,
                                                                   energy_absorption, lingering_acid};
        }


        static void createAcidJetAndLingeringAcid()
        {
            var acid_arrow = library.Get<BlueprintAbility>("9a46dfd390f943647ab4395fc997936d");
            var base_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Acid, Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default), Helpers.CreateContextValue(AbilityRankType.DamageBonus)));
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var apply_sickened = Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1), dispellable: false);                                                                                                    
            var extra_effect = Helpers.CreateConditionalSaved(null, new GameAction[] { apply_sickened});

            var acid_jet_ability = Helpers.CreateAbility("AcidJetExploitAbility",
                                                        "Acid Jet",
                                                        "The arcanist can unleash a jet of acid by expending 1 point from her arcane reservoir and making a ranged touch attack against any one target within close range. If the attack hits, it deals 1d6 points of acid damage + the arcanist’s Charisma modifier, plus an additional 1d6 points of acid damage for every 2 levels beyond 1st (to a maximum of 10d6 at 19th level). The target is also sickened for 1d4 rounds. It can attempt a Fortitude saving throw to negate the sickened condition.",
                                                        "",
                                                        acid_arrow.Icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Standard,
                                                        AbilityRange.Close,
                                                        "",
                                                        "Fortitude partial",
                                                        Helpers.CreateResourceLogic(arcane_reservoir_resource, cost_is_custom: true),
                                                        Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_reducing_facts = new BlueprintFact[] {energy_absorption_buffs[DamageEnergyType.Acid]}),
                                                        acid_arrow.GetComponent<AbilityDeliverProjectile>(),
                                                        Helpers.CreateSpellDescriptor(SpellDescriptor.Acid),
                                                        Helpers.CreateRunActions(SavingThrowType.Fortitude, base_damage, extra_effect),
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getArcanistArray(), progression: ContextRankProgression.OnePlusDiv2), //base damage
                                                        Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 0), //extra damage
                                                        Common.createContextCalculateAbilityParamsBasedOnClasses(getArcanistArray(), StatType.Charisma)
                                                        );
            acid_jet_ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            acid_jet = Common.AbilityToFeature(acid_jet_ability);

            //lingering buffs 5d6 -> 2d6 -> 1d6
            //                4d6 -> 2d6 -> 1d6
            //                3d6 -> 1d6
            //                2d6 -> 1d6
            //                1d6


            lingering_acid = Helpers.CreateFeature("LingeringAcidExploitFeature",
                                                   "Lingering Acid",
                                                   "Whenever the arcanist uses the acid jet exploit, she can expend 2 points from her arcane reservoir instead of one. If she does, the target takes additional damage on the following rounds if it fails its saving throw. The target takes 1d6 points of acid damage on the following round for every 2d6 points of acid damage dealt by the initial attack. On subsequent rounds, the target continues to take 1d6 points of acid damage for every 2d6 points of acid damage dealt on the previous round. The damage continues until the amount of acid damage dealt on the previous round by this effect is 1d6. For example, a 9th level arcanist would deal 5d6 points of acid damage + the arcanist’s Charisma modifier, 2d6 points of acid damage on the following round, and 1d6 points of acid damage on the third and final round. The arcanist must have the acid jet exploit to select this exploit.",
                                                   "",
                                                   LoadIcons.Image2Sprite.Create(@"AbilityIcons/LingeringAcid.png"),
                                                   FeatureGroup.None,
                                                   Helpers.PrerequisiteFeature(greater_arcane_exploits),
                                                   Helpers.PrerequisiteFeature(acid_jet)
                                                   );

            var damage_buffs = new BlueprintBuff[5];
            var apply_extra_damage_buff_actions = new ActionList[5];

            for (int i = 0; i < 5; i++)
            {
                damage_buffs[i] = Helpers.CreateBuff($"LingeringAcidExploit{i + 1}Buff",
                                                     lingering_acid.Name + $" ({i + 1}d6)",
                                                     lingering_acid.Description,
                                                     "",
                                                     lingering_acid.Icon,
                                                     null,
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Acid)
                                                     );
                if ((i + 1) / 2 > 0)
                {
                    var apply_extra_buff = Common.createContextActionApplyBuff(damage_buffs[(i + 1) / 2 - 1], Helpers.CreateContextDuration(1), dispellable: false);
                    Helpers.CreateAddFactContextActions(deactivated: new GameAction[] { Helpers.CreateActionDealDamage(DamageEnergyType.Acid, Helpers.CreateContextDiceValue(DiceType.D6, i + 1, 0)),
                                                                                        apply_extra_buff });
                }
                else
                {
                    Helpers.CreateAddFactContextActions(deactivated: Helpers.CreateActionDealDamage(DamageEnergyType.Acid, Helpers.CreateContextDiceValue(DiceType.D6, i + 1, 0)));
                }
                apply_extra_damage_buff_actions[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(damage_buffs[i], Helpers.CreateContextDuration(1), dispellable: false));
            }

            var lingering_effect = Common.createRunActionsDependingOnContextValueIgnoreNegative(Helpers.CreateContextValue(AbilityRankType.DamageDiceAlternative), apply_extra_damage_buff_actions);
            var extra_lingering = Helpers.CreateConditionalSaved(null, new GameAction[] { apply_sickened, lingering_effect });

            var lingering_acid_ability = library.CopyAndAdd<BlueprintAbility>(acid_jet_ability.AssetGuid, "LingeringAcidExploitAbility", "");
            lingering_acid_ability.SetNameDescriptionIcon(lingering_acid.Name, lingering_acid.Description, lingering_acid.Icon);
            lingering_acid_ability.AddComponent(Helpers.CreateContextRankConfig(type: AbilityRankType.DamageDiceAlternative, baseValueType: ContextRankBaseValueType.ClassLevel, classes: getArcanistArray(), progression: ContextRankProgression.DelayedStartPlusDivStep, startLevel: 3, stepLevel: 4));
            lingering_acid_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(SavingThrowType.Fortitude, base_damage, extra_lingering));
            lingering_acid_ability.ReplaceComponent<AbilityResourceLogic>(Helpers.CreateResourceLogic(arcane_reservoir_resource, amount: 2));

            lingering_acid.AddComponent(Helpers.CreateAddFact(lingering_acid));
        }


        static void createEnergyShieldAndEnergyAbsorption()
        {
            energy_absorption = Helpers.CreateFeature("EnergyAbsorptionExploitFeature",
                                                      "Energy Absorption",
                                                      "Whenever the arcanist is using the energy shield exploit, and the shield prevents 10 or more points of damage, she can absorb a portion of that energy and use it to fuel her exploits. After absorbing the damage, she can use any exploit that deals the same type of energy damage as the type her shield absorbed, reducing the cost to her arcane reservoir by 1 point. She must use this energy within 1 minute or it is lost. The arcanist does not gain more than one such use of energy per round, and she cannot store more than one use of this energy at a time. The arcanist must have the energy shield exploit to select this exploit.",
                                                      "",
                                                      library.Get<BlueprintActivatableAbility>("1452fb3e0e3e2f6488bee09050097b6f").Icon,
                                                      FeatureGroup.None,
                                                      Helpers.PrerequisiteFeature(greater_arcane_exploits)
                                                      );

            var resist_energy_base = library.Get<BlueprintAbility>("21ffef7791ce73f468b6fca4d9371e8b");

            var resist_variants = resist_energy_base.Variants;
            var energy = new DamageEnergyType[] { DamageEnergyType.Acid, DamageEnergyType.Cold, DamageEnergyType.Electricity, DamageEnergyType.Fire, DamageEnergyType.Sonic };

            BlueprintAbility[] energy_shield_variants = new BlueprintAbility[energy.Length];

            for (int i = 0; i < energy_shield_variants.Length; i++)
            {
                var absorption_buff = Helpers.CreateBuff("EnergyAbsorptionExploit" + energy[i].ToString() + "Buff",
                                                         "Energy Absorption: " + energy[i].ToString(),
                                                         energy_absorption.Description,
                                                         "",
                                                         energy_absorption.Icon,
                                                         null);
                energy_absorption_buffs.Add(energy[i], absorption_buff);
                var apply_absorption_buff = Common.createContextActionApplyBuff(absorption_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);

                var buff = Helpers.CreateBuff("EnergyShieldExploit" + energy[i].ToString() + "Buff",
                                              "Energy Shield: " + energy[i].ToString(),
                                              "The arcanist can protect herself from energy damage as a standard action by expending 1 point from her arcane reservoir. She must pick one energy type and gains resistance 10 against that energy type for 1 minute per arcanist level. This protection increases by 5 for every 5 levels the arcanist possesses (up to a maximum of 30 at 20th level).",
                                              "",
                                              resist_variants[i].Icon,
                                              null,
                                              Common.createEnergyDRContextRank(energy[i], AbilityRankType.Default, 5),
                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                               startLevel: -5, stepLevel: 5, max: 6, classes: getArcanistArray()),
                                              Helpers.Create<NewMechanics.ActionOnDamageAbsorbed>(a =>
                                              {
                                                  a.min_dmg = 10;
                                                  a.energy = energy[i];
                                                  a.action = Helpers.CreateActionList(Helpers.CreateConditional(Common.createContextConditionHasFact(energy_absorption), apply_absorption_buff));
                                              })
                                              );

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);
                energy_shield_variants[i] = Helpers.CreateAbility("EnergyShieldExploit" + energy[i].ToString() + "Ability",
                                                                  buff.Name,
                                                                  buff.Description,
                                                                  "",
                                                                  buff.Icon,
                                                                  AbilityType.Supernatural,
                                                                  CommandType.Standard,
                                                                  AbilityRange.Personal,
                                                                  "1 minute/ arcanist level",
                                                                  "",
                                                                  Helpers.CreateRunActions(apply_buff),
                                                                  Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getArcanistArray()),
                                                                  resist_variants[i].GetComponent<AbilitySpawnFx>(),
                                                                  Helpers.CreateResourceLogic(arcane_reservoir_resource)
                                                                  );
                energy_shield_variants[i].setMiscAbilityParametersSelfOnly();
            }

            var energy_shield_wrapper = Common.createVariantWrapper("EnergyShieldExploitAbility", "", energy_shield_variants);
            energy_shield_wrapper.SetName("Energy Shield");

            energy_shield = Common.AbilityToFeature(energy_shield_wrapper);
            energy_absorption.AddComponent(Helpers.PrerequisiteFeature(energy_shield));
        }



        static void createArcaneWeapon()
        {
            var arcane_weapon_magus = library.Get<BlueprintAbility>("3c89dfc82c2a3f646808ea250eb91b91");
            var enchants = WeaponEnchantments.temporary_enchants;

            var enhancement_buff = Helpers.CreateBuff("ArcanistArcaneWeaponEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(arcane_weapon_group,
                                                                                                      false, true,
                                                                                                      enchants
                                                                                                      )
                                         );
            var arcane_weapon_enhancement_buff = Helpers.CreateBuff("ArcanistArcaneWeaponEnchancementSwitchBuff",
                                                                 "Arcane Weapon",
                                                                 "As a standard action, the arcanist can expend 1 point from her arcane reservoir to enhance her weapon. The weapon gains a +1 enhancement bonus, which increases by 1 for every 4 levels beyond 5th (to a maximum of +4 at 17th level). These bonuses can be added to the weapon, stacking with existing weapon bonuses to a maximum of +5. An arcanist can also use this exploit to add one of the following weapon special abilities: flaming, frost, keen, shock and speed. Adding these special abilities replaces an amount of enhancement bonus equal to the ability’s cost. Duplicate special abilities do not stack. The benefits are decided upon when the exploit is used, and they cannot be changed unless the exploit is used again. These benefits only apply to weapons wielded by the arcanist; if another creature attempts to wield the weapon, it loses these benefits, though they resume if the arcanist regains possession of the weapon. The arcanist cannot have more than one use of this ability active at a time. This effect lasts for a number of minutes equal to the arcanist’s Charisma modifier (minimum 1).",
                                                                 "",
                                                                 arcane_weapon_magus.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, is_permanent: true, dispellable: false)
                                                                                                     )
                                                                 );
            arcane_weapon_enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var flaming = Common.createEnchantmentAbility("ArcanistArcaneWeaponEnchancementFlaming",
                                                                "Arcane Weapon - Flaming",
                                                                "An arcanist can add the flaming property to her arcane weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                                library.Get<BlueprintActivatableAbility>("05b7cbe45b1444a4f8bf4570fb2c0208").Icon,
                                                                arcane_weapon_enhancement_buff,
                                                                library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                                1, arcane_weapon_group);

            var frost = Common.createEnchantmentAbility("ArcanistArcaneWeaponEnchancementFrost",
                                                            "Arcane Weapon - Frost",
                                                            "An arcanist can add the frost property to her arcane weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA frost weapon is sheathed in a terrible, icy cold that deals an extra 1d6 points of cold damage on a successful hit. The cold does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("b338e43a8f81a2f43a73a4ae676353a5").Icon,
                                                            arcane_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b"),
                                                            1, arcane_weapon_group);

            var shock = Common.createEnchantmentAbility("ArcanistArcaneWeaponEnchancemenShock",
                                                            "Arcane Weapon - Shock",
                                                            "An arcanist can add the shock property to her arcane weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA shock weapon is sheathed in crackling electricity that deals an extra 1d6 points of electricity damage on a successful hit. The electricity does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("a3a9e9a2f909cd74e9aee7788a7ec0c6").Icon,
                                                            arcane_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658"),
                                                            1, arcane_weapon_group);

            var keen = Common.createEnchantmentAbility("ArcanistArcaneWeaponEnchancementKeen",
                                                            "Arcane Weapon - Keen",
                                                            "An arcanist can add the keen property to her arcane weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                            library.Get<BlueprintActivatableAbility>("24fe1f546e07987418557837b0e0f8f5").Icon,
                                                            arcane_weapon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                            1, arcane_weapon_group);

            var speed = Common.createEnchantmentAbility("ArcanistArcaneWeaponEnchancementSpeed",
                                                                    "Arcane Weapon - Disruption",
                                                                    "An arcanist can add the speed property to her arcane weapon, but this consumes 3 points of enhancement bonus granted to this weapon.\nWhen making a full attack, the wielder of a speed weapon may make one extra attack with it. The attack uses the wielder's full base attack bonus, plus any modifiers appropriate to the situation. (This benefit is not cumulative with similar effects, such as a Haste spell.)",
                                                                    library.Get<BlueprintActivatableAbility>("85742dd6788c6914f96ddc4628b23932").Icon,
                                                                    arcane_weapon_enhancement_buff,
                                                                    library.Get<BlueprintWeaponEnchantment>("f1c0c50108025d546b2554674ea1c006"),
                                                                    3, arcane_weapon_group);

            var apply_buff = Common.createContextActionApplyBuff(arcane_weapon_enhancement_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);
            var arcane_weapon_ability = Helpers.CreateAbility("ArcanistArcaneWeponAbility",
                                                                         arcane_weapon_enhancement_buff.Name,
                                                                         arcane_weapon_enhancement_buff.Description,
                                                                         "",
                                                                         arcane_weapon_enhancement_buff.Icon,
                                                                         AbilityType.Supernatural,
                                                                         CommandType.Standard,
                                                                         AbilityRange.Personal,
                                                                         "Charisma modifier minutes",
                                                                         "",
                                                                         Helpers.CreateResourceLogic(arcane_reservoir_resource),
                                                                         Helpers.Create<NewMechanics.AbilityCasterPrimaryHandFree>(a => a.not = true),
                                                                         Helpers.CreateRunActions(apply_buff),
                                                                         arcane_weapon_magus.GetComponent<AbilitySpawnFx>(),
                                                                         Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, min: 1),
                                                                         Helpers.PrerequisiteClassLevel(arcanist_class, 5)
                                                                         );
            arcane_weapon_ability.setMiscAbilityParametersSelfOnly(Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon);
            arcane_weapon_ability.NeedEquipWeapons = true;


            arcane_weapon = Helpers.CreateFeature("ArcanistArcaneWeaponEnchancementFeature",
                                                  arcane_weapon_ability.name,
                                                  arcane_weapon_ability.Description,
                                                  "",
                                                   arcane_weapon_ability.Icon,
                                                   FeatureGroup.None,
                                                   Helpers.CreateAddFacts(arcane_weapon_ability, flaming, frost, shock, keen)
                                                   );

            var arcane_weapon2 = Helpers.CreateFeature("ArcanistArcaneWeaponEnchancement2Feature",
                                                        "Arcane Weapon +2",
                                                        arcane_weapon_ability.Description,
                                                        "",
                                                        arcane_weapon_ability.Icon,
                                                        FeatureGroup.None,
                                                        Common.createIncreaseActivatableAbilityGroupSize(arcane_weapon_group)
                                                        );
            var arcane_weapon3 = Helpers.CreateFeature("ArcanistArcaneWeaponEnchancement3Feature",
                                            "Arcane Weapon +3",
                                            arcane_weapon_ability.Description,
                                            "",
                                            arcane_weapon_ability.Icon,
                                            FeatureGroup.None,
                                            Helpers.CreateAddFacts(speed),
                                            Common.createIncreaseActivatableAbilityGroupSize(arcane_weapon_group)
                                            );

            var arcane_weapon4 = Helpers.CreateFeature("ArcanistArcaneWeaponEnchancement4Feature",
                                "Arcane Weapon +4",
                                arcane_weapon_ability.Description,
                                "",
                                arcane_weapon_ability.Icon,
                                FeatureGroup.None,
                                Common.createIncreaseActivatableAbilityGroupSize(arcane_weapon_group)
                                );

            arcane_weapon_ability.AddComponent(Helpers.CreateAddFeatureOnClassLevel(arcane_weapon2, 9, getArcanistArray()));
            arcane_weapon_ability.AddComponent(Helpers.CreateAddFeatureOnClassLevel(arcane_weapon3, 13, getArcanistArray()));
            arcane_weapon_ability.AddComponent(Helpers.CreateAddFeatureOnClassLevel(arcane_weapon4, 17, getArcanistArray()));
        }

        static void createArcaneBarrier()
        {
            var arcane_barrier_cost_buff = Helpers.CreateBuff("ArcaneBarrierCostBuff",
                                                               "",
                                                               "",
                                                               "",
                                                               null,
                                                               null
                                                               );
            arcane_barrier_cost_buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.HiddenInUi);
            arcane_barrier_cost_buff.Stacking = StackingType.Stack;
            var apply_cost_buff = Common.createContextActionApplyBuff(arcane_barrier_cost_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var icon = library.Get<BlueprintAbility>("183d5bb91dea3a1489a6db6c9cb64445").Icon; //shield of faith
            var buff = Helpers.CreateBuff("ArcaneBarrierBuff",
                                          "Arcane Barrier",
                                          "As a swift action, the arcanist can expend 1 point from her arcane reservoir to create a barrier of magic that protects her from harm. This barrier grants the arcanist a number of temporary hit points equal to her arcanist level + her Charisma modifier, and lasts for 1 minute per arcanist level or until all the temporary hit points have been lost. Each additional time per day the arcanist uses this ability, the number of arcane reservoir points she must spend to activate it increases by 1 (so the second time it is used, the arcanist must expend 2 points from her arcane reservoir, 3 points for the third time, and so on). The temporary hit points from this ability do not stack with themselves, but additional uses do cause the total number of temporary hit points and the duration to reset.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<TemporaryHitPointsFromAbilityValue>(t => { t.Value = Helpers.CreateContextValue(AbilityRankType.Default); t.RemoveWhenHitPointsEnd = true; }),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueTypeExtender.ClassLevelPlusStatValue.ToContextRankBaseValueType(),
                                                                          classes: getArcanistArray(), stat: StatType.Charisma, min: 0)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);

            var ability = Helpers.CreateAbility("ArcaneBarrierAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff, apply_cost_buff),
                                                library.Get<BlueprintAbility>("183d5bb91dea3a1489a6db6c9cb64445").GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateResourceLogic(arcane_reservoir_resource, cost_is_custom: true),
                                                Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => { r.cost_increasing_facts = new BlueprintFact[] { arcane_barrier_cost_buff }; }),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getArcanistArray())
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            arcane_barrier = Common.AbilityToFeature(ability, false);
        }


        static void createQuickStudy()
        {
            var icon = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e").Icon;//detect magic

            List<BlueprintAbility> study_abilities = new List<BlueprintAbility>();

            for (int i = 1; i <= 9; i++)
            {
                var ability = Helpers.CreateAbility($"QuickStudy{i}Ability",
                                                    "Quick Study " + Common.roman_id[i],
                                                    "The arcanist can prepare a new all spells of the specified level in place of existing ones by expending 1 point from her arcane reservoir. Using this ability is a full-round action that provokes an attack of opportunity. The arcanist must be able to reference her spellbook when using this ability.",
                                                    "",
                                                    icon,
                                                    AbilityType.SpellLike,
                                                    CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(Helpers.Create<SpellManipulationMechanics.RefreshArcanistSpellLevel>(c => { c.spell_level = i; })),
                                                    Helpers.CreateResourceLogic(arcane_reservoir_resource),
                                                    Helpers.Create<NewMechanics.AbilityShowIfHasClassLevel>(a => { a.character_class = arcanist_class; a.level = (i - 1) * 2; })
                                                    );
                Common.setAsFullRoundAction(ability);
                ability.setMiscAbilityParametersSelfOnly();
                study_abilities.Add(ability);
            }
            var quick_study_ability = Common.createVariantWrapper("QuickStudyAbility", "", study_abilities.ToArray());
            quick_study_ability.SetName("Quick Study");

            quick_study = Helpers.CreateFeature("QuickStudyFeature",
                                                quick_study_ability.Name,
                                                quick_study_ability.Description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFact(quick_study_ability)
                                                );
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
