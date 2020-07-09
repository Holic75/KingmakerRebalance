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

namespace CallOfTheWild
{
    class AnimalFocusEngine
    {
        static LibraryScriptableObject library => Main.library;
        AddFeatureOnClassLevel[] mouse_focus;
        BlueprintActivatableAbility bull_focus;
        BlueprintActivatableAbility bear_focus;
        BlueprintActivatableAbility tiger_focus;
        BlueprintActivatableAbility monkey_focus;
        BlueprintActivatableAbility stag_focus;
        BlueprintActivatableAbility frog_focus;
        BlueprintActivatableAbility owl_focus;
        BlueprintActivatableAbility falcon_focus;
        BlueprintActivatableAbility snake_focus;

        BlueprintActivatableAbility crow_focus;
        BlueprintActivatableAbility shark_focus;
        BlueprintActivatableAbility turtle_focus;
        BlueprintActivatableAbility goat_focus;
        BlueprintActivatableAbility moongoose_focus;

        BlueprintFeature planar_focus;
        BlueprintActivatableAbility planar_focus_fire;
        BlueprintActivatableAbility planar_focus_cold;
        BlueprintActivatableAbility planar_focus_air;
        BlueprintActivatableAbility planar_focus_earth;
        BlueprintActivatableAbility planar_focus_water;
        BlueprintActivatableAbility planar_focus_shadow;
        BlueprintActivatableAbility planar_focus_chaos;
        BlueprintActivatableAbility planar_focus_law;
        BlueprintActivatableAbility planar_focus_good;
        BlueprintActivatableAbility planar_focus_evil;


        BlueprintCharacterClass[] allowed_classes;
        BlueprintArchetype allowed_archetype;
        int delay;
        string prefix;

        public void initialize(BlueprintCharacterClass[] character_classes, BlueprintArchetype character_archetype, int lvl_delay, string name_prefix)
        {
            allowed_classes = character_classes;
            allowed_archetype = character_archetype;
            delay = lvl_delay;
            prefix = name_prefix;
        }


        public BlueprintFeature createPlanarFocus(string ext, BlueprintFeature prerequisite_to_share_with_ac)
        {
            var outsider = library.Get<BlueprintFeature>("9054d3988d491d944ac144e27b6bc318");

            var airborne = library.Get<BlueprintFeature>("70cffb448c132fa409e49156d013b175");
            planar_focus_air = createToggleFocus(prefix + "PlanarFocusAir",
                                                 "Planar Focus: Air",
                                                 "You gain limited levitation ability that gives you immunity to difficult terrain and ground based effects.",
                                                 "",
                                                 "",
                                                 library.Get<BlueprintFeature>("f48c7d56a8a13af4d8e1cc9aae579b01").Icon, //elemental movement
                                                 Helpers.Create<AddConditionImmunity>(a => a.Condition = UnitCondition.DifficultTerrain),
                                                 Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Ground),
                                                 Common.createBuffDescriptorImmunity(SpellDescriptor.Ground)
                                               );

            planar_focus_chaos = createToggleFocus(prefix + "PlanarFocusChaos",
                                                     "Planar Focus: Chaos",
                                                     "Your form shifts subtly, making it difficult for others to aim precise attacks against you. You gain a 25% chance to negate extra damage from critical hits and precision damage from attacks made against you (such as from sneak attacks). Only chaotic characters can use this planar focus.",
                                                     "",
                                                     "",
                                                     library.Get<BlueprintAbility>("1eaf1020e82028d4db55e6e464269e00").Icon, //protection from chaos
                                                     Common.createAddFortification(25)
                                                   );
            planar_focus_chaos.AddComponent(Helpers.Create<NewMechanics.ActivatableAbilityAlignmentRestriction>(c => c.Alignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Chaotic));

            var deal_cold_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Cold,
                                                                  Helpers.CreateContextDiceValue(DiceType.D4, Helpers.CreateContextValue(AbilityRankType.DamageDice)),
                                                                  IgnoreCritical: true);
            planar_focus_cold = createToggleFocus(prefix + "PlanarFocusCold",
                                                 "Planar Focus: Cold",
                                                 "Creatures that attack you with natural attacks or melee weapons take 1d4 points of cold damage for every 4 class levels you possess.",
                                                 "",
                                                 "",
                                                 library.Get<BlueprintAbility>("021d39c8e0eec384ba69140f4875e166").Icon, //protection from cold
                                                 Common.createAddTargetAttackWithWeaponTrigger(Helpers.CreateActionList(),
                                                                                               Helpers.CreateActionList(deal_cold_damage)
                                                                                               ),
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType(),
                                                                                 ContextRankProgression.StartPlusDivStep,
                                                                                 AbilityRankType.DamageDice,
                                                                                 startLevel: delay,
                                                                                 stepLevel: 4,
                                                                                 classes: allowed_classes, archetype: allowed_archetype)
                                                 );


            planar_focus_earth = createToggleFocus(prefix + "PlanarFocusEarth",
                                                     "Planar Focus: Earth",
                                                     "You gain +2 bonus to CMB when performing bull rush maneuver, and a +2 bonus to CMD when defending against it. You also receive +2 enhancement bonus to your natural armor.",
                                                     "",
                                                     "",
                                                     library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b").Icon, //stone skin
                                                     Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.BullRush, 2),
                                                     Common.createManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.BullRush, 2),
                                                     Common.createAddGenericStatBonus(2, ModifierDescriptor.NaturalArmorEnhancement, StatType.AC)
                                                     );

            planar_focus_evil = createToggleFocus(prefix + "PlanarFocusEvil",
                                                     "Planar Focus: Evil",
                                                     $"You gain a +1 profane bonus to AC and on saves against attacks made and effects created by good outsiders. This bonus increases to +2 at {10+delay}th level. Only evil characters can use this planar focus.",
                                                     "",
                                                     "",
                                                     library.Get<BlueprintAbility>("b56521d58f996cd4299dab3f38d5fe31").Icon, //profane nimbus
                                                     Common.createContextACBonusAgainstFactOwner(outsider, AlignmentComponent.Good, Helpers.CreateContextValue(AbilityRankType.StatBonus), ModifierDescriptor.Profane),
                                                     Common.createContextSavingThrowBonusAgainstFact(outsider, AlignmentComponent.Good, Helpers.CreateContextValue(AbilityRankType.StatBonus), ModifierDescriptor.Profane),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType(),
                                                                                 ContextRankProgression.StartPlusDivStep,
                                                                                 AbilityRankType.StatBonus,
                                                                                 startLevel: delay,
                                                                                 stepLevel: 10,
                                                                                 max: 2,
                                                                                 classes: allowed_classes, archetype: allowed_archetype)
                                                     );
            planar_focus_evil.AddComponent(Helpers.Create<NewMechanics.ActivatableAbilityAlignmentRestriction>(c => c.Alignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Evil));


            planar_focus_fire = createToggleFocus(prefix + "PlanarFocusFire",
                                                 "Planar Focus: Fire",
                                                 "Your natural attacks and melee weapons deal 1d3 points of fire damage for every 4 class levels you possess.",
                                                 "",
                                                 "",
                                                 library.Get<BlueprintActivatableAbility>("7902941ef70a0dc44bcfc174d6193386").Icon, //weapon bond flaming
                                                 Common.createAddWeaponEnergyDamageDiceBuff(Helpers.CreateContextDiceValue(DiceType.D3, Helpers.CreateContextValue(AbilityRankType.DamageDice)),
                                                                                            DamageEnergyType.Fire,
                                                                                            AttackType.Melee, AttackType.Touch),
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType(),
                                                                                 ContextRankProgression.StartPlusDivStep,
                                                                                 AbilityRankType.DamageDice,
                                                                                 startLevel: delay + 4,
                                                                                 stepLevel: 4,
                                                                                 classes: allowed_classes, archetype: allowed_archetype)
                                               );

            planar_focus_good = createToggleFocus(prefix + "PlanarFocusGood",
                                         "Planar Focus: Good",
                                         $"You gain a +1 sacred bonus to AC and on saves against attacks made or effects created by evil outsiders. This bonus increases to +2 at {10+delay}th level. Only good characters can use this planar focus.",
                                         "",
                                         "",
                                         library.Get<BlueprintAbility>("bf74b3b54c21a9344afe9947546e036f").Icon, //sacred nimbus
                                         Common.createContextACBonusAgainstFactOwner(outsider, AlignmentComponent.Evil, Helpers.CreateContextValue(AbilityRankType.StatBonus), ModifierDescriptor.Sacred),
                                         Common.createContextSavingThrowBonusAgainstFact(outsider, AlignmentComponent.Evil, Helpers.CreateContextValue(AbilityRankType.StatBonus), ModifierDescriptor.Sacred),
                                         Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType(),
                                                                     ContextRankProgression.StartPlusDivStep,
                                                                     AbilityRankType.StatBonus,
                                                                     startLevel: delay,
                                                                     stepLevel: 10,
                                                                     max: 2,
                                                                     classes: allowed_classes, archetype: allowed_archetype)
                                         );
            planar_focus_good.AddComponent(Helpers.Create<NewMechanics.ActivatableAbilityAlignmentRestriction>(c => c.Alignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Good));

            planar_focus_law = createToggleFocus(prefix + "PlanarFocusLaw",
                                                 "Planar Focus: Law",
                                                 "You gain immunity to polymorph spells.",
                                                 "",
                                                 "",
                                                 library.Get<BlueprintAbility>("c3aafbbb6e8fc754fb8c82ede3280051").Icon, //protection from law
                                                 Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Polymorph)
                                                 );
            planar_focus_law.AddComponent(Helpers.Create<NewMechanics.ActivatableAbilityAlignmentRestriction>(c => c.Alignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Lawful));

            planar_focus_water = createToggleFocus(prefix + "PlanarFocusWater",
                                                     "Planar Focus: Water",
                                                     "You gain immunity to combat maneuvers.",
                                                     "",
                                                     "",
                                                     library.Get<BlueprintAbility>("3e4ab69ada402d145a5e0ad3ad4b8564").Icon, //mirror image
                                                     Helpers.Create<AddCondition>(c => c.Condition = UnitCondition.ImmuneToCombatManeuvers)
                                                     );


            planar_focus_shadow = createToggleFocus(prefix + "PlanarFocusShadow",
                         "Planar Focus: Shadow",
                         "You gain a +5 bonus on Stealth and Trickery checks.",
                         "",
                         "",
                         library.Get<BlueprintAbility>("f001c73999fb5a543a199f890108d936").Icon, //vanish
                         Helpers.CreateAddStatBonus(StatType.SkillStealth, 5, ModifierDescriptor.UntypedStackable),
                         Helpers.CreateAddStatBonus(StatType.SkillThievery, 5, ModifierDescriptor.UntypedStackable)
                         );


            BlueprintActivatableAbility[] foci = new BlueprintActivatableAbility[] {planar_focus_air, planar_focus_chaos, planar_focus_cold, planar_focus_earth,
                                                                                    planar_focus_evil, planar_focus_fire, planar_focus_good, planar_focus_law, planar_focus_shadow};

            string description = "When you use your animal focus class feature, you can choose any of the following new aspects unless they conflict with your alignment.";

            foreach (var f in foci)
            {
                description += "\n" + f.Name + " - " + f.Description;
            }

            var planar_focus_ac = Helpers.CreateFeature(prefix + "PlanarFocusAcFeature",
                                      "",
                                      "",
                                      "",
                                      null,
                                      FeatureGroup.None,
                                      Helpers.CreateAddFacts(foci)
                                      );

            planar_focus = Helpers.CreateFeature(prefix + "PlanarFocusFeature",
                                                 $"Planar Focus ({ext})",
                                                  description,
                                                  "",
                                                  null,
                                                  FeatureGroup.Feat,
                                                  Helpers.CreateAddFacts(foci),
                                                  Helpers.PrerequisiteStatValue(StatType.SkillLoreReligion, 5)
                                                  );
            //planar_focus.AddComponent(Common.createAddFeatToAnimalCompanion(planar_focus));
            planar_focus.AddComponent(Common.createAddFeatureIfHasFact(prerequisite_to_share_with_ac, Common.createAddFeatToAnimalCompanion(planar_focus, "")));

            library.AddFeats(planar_focus);

            return planar_focus;
        }




        public BlueprintFeature createFeykillerAnimalFocus()
       {
            //remove bear, frog, monkey, mouse
            //add crow (+arcana), goat (+saves vs enchantment), shark (+lore nature), turtle (+ natural ac)

            var eagle_splendor = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("446f7bf201dc1934f96ac0a26e324803"); //crow
            var devil_spawn = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintFeature>("02a2c984494a9734ba8b01927dcf96e2"); // goat
            var magic_fang = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33"); //shark
            var resistance = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("7bc8e27cba24f0e43ae64ed201ad5785"); //turtle
            var delay_poison = library.Get<BlueprintAbility>("b48b4c5ffb4eab0469feba27fc86a023"); //moongoose

            (int, int)[] progressionStat = new (int, int)[3] { (7 + delay, 2), (14 + delay, 4), (20, 6) };
            (int, int)[] progressionSkill = new (int, int)[3] { (7 + delay, 4), (14 + delay, 6), (20, 8) };           

            turtle_focus = createScaledFocus(prefix + "TurtleFocus",
                                            "Animal Focus: Turtle",
                                            $"The character gains a +2 enhancement bonus to Natural Armor Class. This bonus increases to +4 at {8+delay}th level and +6 at {15+delay}th level.",
                                            "",
                                            "",
                                            resistance.Icon,
                                            Kingmaker.Enums.ModifierDescriptor.NaturalArmorEnhancement,
                                            Kingmaker.EntitySystem.Stats.StatType.AC,
                                            progressionStat,
                                            allowed_classes,
                                            allowed_archetype);

            var goat_bonus = Helpers.Create<SavingThrowBonusAgainstSchoolAbilityValue>();
            goat_bonus.School = SpellSchool.Enchantment;
            goat_bonus.Value = 0;
            goat_bonus.Bonus = Helpers.CreateContextValueRank(AbilityRankType.Default);
            goat_bonus.ModifierDescriptor = ModifierDescriptor.Competence;

            goat_focus = createScaledFocus(prefix + "GoatFocus",
                                            "Animal Focus: Goat",
                                            $"The creature gains a +2 competence bonus on saving throws against enchantment spells and spell-like abilities. This bonus increases to +4 at {8+delay}th level and +6 at {15+delay}th level.",
                                            "",
                                            "",
                                            devil_spawn.Icon,
                                            goat_bonus,
                                            progressionStat,
                                            allowed_classes,
                                            allowed_archetype);
            crow_focus = createScaledFocus(prefix + "CrowFocus",
                                               "Animal Focus: Crow",
                                               $"The character gains a +4 competence bonus on Knowledge Arcana checks. This bonus increases to +6 at {8+prefix}th level and +8 at {15+prefix}th level.",
                                               "",
                                               "",
                                               eagle_splendor.Icon,
                                               Kingmaker.Enums.ModifierDescriptor.Competence,
                                               Kingmaker.EntitySystem.Stats.StatType.SkillKnowledgeArcana,
                                               progressionSkill,
                                               allowed_classes,
                                               allowed_archetype);
            shark_focus = createScaledFocus(prefix + "SharkFocus",
                                               "Animal Focus: Shark",
                                               $"The character gains a +4 competence bonus on Lore Nature checks. This bonus increases to +6 at {8+delay}th level and +8 at {15+delay}th level.",
                                               "",
                                               "",
                                               magic_fang.Icon,
                                               Kingmaker.Enums.ModifierDescriptor.Competence,
                                               Kingmaker.EntitySystem.Stats.StatType.SkillLoreNature,
                                               progressionSkill,
                                               allowed_classes,
                                               allowed_archetype);

            moongoose_focus = createToggleFocus(prefix + "MoongooseFocus",
                                               "Animal Focus: Moongoose",
                                               $"The creature gains a +2 competence bonus on grapple combat maneuver CMD and on saving throws against poison. These bonuses increase to +4 at {8+delay}th level and +6 at {15+delay}th level.",
                                               "",
                                               "",
                                               delay_poison.Icon,
                                               Common.createContextManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Grapple, Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                                               Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.StatBonus), ModifierDescriptor.Circumstance, SpellDescriptor.Poison),
                                               Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType(),
                                                                                 ContextRankProgression.Custom,
                                                                                 AbilityRankType.StatBonus,
                                                                                 customProgression: progressionStat,
                                                                                 classes: allowed_classes, archetype: allowed_archetype)
                                               );


            BlueprintComponent animal_foci = CallOfTheWild.Helpers.CreateAddFacts(bull_focus,
                                                                                       tiger_focus,
                                                                                       falcon_focus,
                                                                                       owl_focus,
                                                                                       stag_focus,
                                                                                       turtle_focus,
                                                                                       crow_focus,
                                                                                       shark_focus,
                                                                                       goat_focus,
                                                                                       moongoose_focus);



            var inflict_light_wounds = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("e5cb4c4459e437e49a4cd73fde6b9063");
            var feat = CallOfTheWild.Helpers.CreateFeature(prefix + "FeykillerAnimalFocusFeature",
                                                            "Feykiller Animal Focus",
                                                            "A feykiller emulates animals that grant her the ability to unmask fey trickery. She adds crow, goat, shark, moongoose and turtle to her animal focus ability instead of the bear, frog, monkey, mouse and snake choices.",
                                                            "",
                                                            inflict_light_wounds.Icon,
                                                            FeatureGroup.None,
                                                            animal_foci);
            return feat;
        }


        public BlueprintFeature createAnimalFocus()
        {
            var bull_strength = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("4c3d08935262b6544ae97599b3a9556d");
            var bear_endurance = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("a900628aea19aa74aad0ece0e65d091a");
            var cat_grace = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("de7a025d48ad5da4991e7d3c682cf69d");
            var aspect_falcon = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("7bdb6a9fb6b37614e96f155748ae50c6");
            var owl_wisdom = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("f0455c9295b53904f9e02fc571dd2ce1");
            var heroism = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63"); //for monkey
            var feather_step = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("f3c0b267dd17a2a45a40805e31fe3cd1"); //for frog
            var longstrider = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("14c90900b690cac429b229efdf416127"); //for stag
            var summon_monster1 = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("8fd74eddd9b6c224693d9ab241f25e84"); //for mouse
            var summon_monster3 = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>("5d61dde0020bbf54ba1521f7ca0229dc"); //for snake
            (int, int)[] progressionStat = new (int, int)[3] { (7 + delay, 2), (14 + delay, 4), (20, 6) };
            (int, int)[] progressionSkill = new (int, int)[3] { (7 + delay, 4), (14 + delay, 6), (20, 8) };
            (int, int)[] progressionSpeed = new (int, int)[3] { (7 + delay, 5), (14 + delay, 10), (20, 20) };

            mouse_focus = createMouseFocus(summon_monster1.Icon, allowed_classes, allowed_archetype, 12 + delay);
            bull_focus = createScaledFocus(prefix + "BullFocus",
                                            "Animal Focus: Bull",
                                            $"The character gains a +2 enhancement bonus to Strength. This bonus increases to +4 at {8+delay}th level and +6 at {15+delay}th level",
                                            "",
                                            "",
                                            bull_strength.Icon,
                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                            Kingmaker.EntitySystem.Stats.StatType.Strength,
                                            progressionStat,
                                            allowed_classes,
                                            allowed_archetype);
            bear_focus = createScaledFocus(prefix + "BearFocus",
                                            "Animal Focus: Bear",
                                            $"The character gains a +2 enhancement bonus to Constitution. This bonus increases to +4 at {8 + delay}th level and +6 at {15 + delay}th level.",
                                            "",
                                            "",
                                            bear_endurance.Icon,
                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                            Kingmaker.EntitySystem.Stats.StatType.Constitution,
                                            progressionStat,
                                            allowed_classes,
                                            allowed_archetype);
            tiger_focus = createScaledFocus(prefix + "TigerFocus",
                                            "Animal Focus: Tiger",
                                            $"The character gains a +2 enhancement bonus to Dexterity. This bonus increases to +4 at {8+delay}th level and +6 at {15 + delay}th level.",
                                            "",
                                            "",
                                            cat_grace.Icon,
                                            Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                            Kingmaker.EntitySystem.Stats.StatType.Dexterity,
                                            progressionStat,
                                            allowed_classes,
                                            allowed_archetype);
            falcon_focus = createScaledFocus(prefix + "FalconFocus",
                                               "Animal Focus: Falcon",
                                               $"The character gains a +4 competence bonus on Perception checks. This bonus increases to +6 at {8 + delay}th level and +8 at {15+delay}th level.",
                                               "",
                                               "",
                                               aspect_falcon.Icon,
                                               Kingmaker.Enums.ModifierDescriptor.Competence,
                                               Kingmaker.EntitySystem.Stats.StatType.SkillPerception,
                                               progressionSkill,
                                               allowed_classes,
                                               allowed_archetype);

            monkey_focus = createScaledFocus(prefix + "MonkeyFocus",
                                               "Animal Focus: Monkey",
                                               $"The character gains a +4 competence bonus on Athletics checks. This bonus increases to +6 at {8+delay}th level and +8 at {15+delay}th level.",
                                               "",
                                               "",
                                               heroism.Icon,
                                               Kingmaker.Enums.ModifierDescriptor.Competence,
                                               Kingmaker.EntitySystem.Stats.StatType.SkillAthletics,
                                               progressionSkill,
                                               allowed_classes,
                                               allowed_archetype);
            owl_focus = createScaledFocus(prefix + "OwlFocus",
                                            "Animal Focus: Owl",
                                            $"The character gains a +4 competence bonus on Stealth checks. This bonus increases to +6 at {8+delay}th level and +8 at {15+delay}th level.",
                                            "",
                                            "",
                                            owl_wisdom.Icon,
                                            Kingmaker.Enums.ModifierDescriptor.Competence,
                                            Kingmaker.EntitySystem.Stats.StatType.SkillStealth,
                                            progressionSkill,
                                            allowed_classes,
                                            allowed_archetype);
            frog_focus = createScaledFocus(prefix + "FrogFocus",
                                            "Animal Focus: Frog",
                                            $"The character gains a +4 competence bonus on Mobility checks. This bonus increases to +6 at {8+delay}th level and +8 at {15+delay}th level.",
                                            "",
                                            "",
                                            feather_step.Icon,
                                            Kingmaker.Enums.ModifierDescriptor.Competence,
                                            Kingmaker.EntitySystem.Stats.StatType.SkillMobility,
                                            progressionSkill,
                                            allowed_classes,
                                            allowed_archetype);
            stag_focus = createScaledFocus(prefix + "StagFocus",
                                             "Animal Focus: Stag",
                                             $"The character gains a 5-foot enhancement bonus to its base land speed. This bonus increases to 10 feet at {8+delay}th level and 20 feet at {15+delay}th level.",
                                             "",
                                             "",
                                             longstrider.Icon,
                                             Kingmaker.Enums.ModifierDescriptor.Enhancement,
                                             Kingmaker.EntitySystem.Stats.StatType.Speed,
                                             progressionSpeed,
                                             allowed_classes,
                                             allowed_archetype);

            snake_focus = createToggleFocus(prefix + "SnakeFocus",
                                             "Animal Focus: Snake",
                                             $"The creature gains a +2 bonus on attack rolls when making attacks of opportunity and a +2 dodge bonus to AC against attacks of opportunity. These bonuses increase to +4 at {8+delay}th level and +6 at {15+delay}th level.",
                                             "",
                                             "",
                                             summon_monster3.Icon,
                                             Common.createACBonussOnAttacksOfOpportunity(Helpers.CreateContextValue(AbilityRankType.StatBonus), ModifierDescriptor.UntypedStackable),
                                             Common.createAttackBonusOnAttacksOfOpportunity(Helpers.CreateContextValue(AbilityRankType.StatBonus), ModifierDescriptor.Dodge),
                                             Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType(),
                                                                            ContextRankProgression.Custom,
                                                                            AbilityRankType.StatBonus,
                                                                            customProgression: progressionStat,
                                                                            classes: allowed_classes, archetype: allowed_archetype));


            BlueprintComponent animal_foci = CallOfTheWild.Helpers.CreateAddFacts(bull_focus,
                                                                                       bear_focus,
                                                                                       tiger_focus,
                                                                                       falcon_focus,
                                                                                       monkey_focus,
                                                                                       owl_focus,
                                                                                       frog_focus,
                                                                                       stag_focus,
                                                                                       snake_focus);

            var wildshape_wolf = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintFeature>("19bb148cb92db224abb431642d10efeb");
            var feat = CallOfTheWild.Helpers.CreateFeature(prefix + "AnimalFocusFeature",
                                                            "Animal Focus",
                                                            "Character can take the focus of an animal as a swift action. She must select one type of animal to emulate, gaining a bonus or special ability based on the type of animal emulated and her level.",
                                                            "",
                                                            wildshape_wolf.Icon,
                                                            FeatureGroup.None,
                                                            animal_foci, mouse_focus[0], mouse_focus[1]);
            return feat;
        }


        Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel[] createMouseFocus(UnityEngine.Sprite icon, BlueprintCharacterClass[] allowed_classes,
                                                                                                      BlueprintArchetype archetype, int update_lvl)
        {
            var evasion = Helpers.Create<Kingmaker.Designers.Mechanics.Facts.Evasion>();
            var improved_evasion = Helpers.Create<Kingmaker.Designers.Mechanics.Facts.ImprovedEvasion>();
            improved_evasion.SavingThrow = SavingThrowType.Reflex;
            evasion.SavingThrow = SavingThrowType.Reflex;
            var mouse_focus1 = createToggleFocus(prefix + "MouseFocus1",
                                            "Mouse Focus",
                                            $"The creature gains evasion, as the rogue class feature. At {12+delay}th level, this increases to improved evasion, as the rogue advanced talent.",
                                            "",
                                            "",
                                            icon,
                                            evasion);
            var mouse_focus2 = createToggleFocus(prefix + "MouseFocus2",
                                           "Mouse Focus",
                                           $"The creature gains evasion, as the rogue class feature. At {12+delay}th level, this increases to improved evasion, as the rogue advanced talent.",
                                           "",
                                           "",
                                           icon,
                                           evasion, improved_evasion);

            var mouse_focus1f = Helpers.CreateFeature(prefix + "MouseFocus1Feature",
                                                       "",
                                                       "",
                                                       "",
                                                       icon,
                                                       FeatureGroup.None,
                                                       CallOfTheWild.Helpers.CreateAddFact(mouse_focus1));
            mouse_focus1f.HideInUI = true;
            mouse_focus1f.HideNotAvailibleInUI = true;
            var mouse_focus2f = Helpers.CreateFeature(prefix + "MouseFocus2Feature",
                                           "",
                                           "",
                                           "",
                                           icon,
                                           FeatureGroup.None,
                                           CallOfTheWild.Helpers.CreateAddFact(mouse_focus2));
            mouse_focus2f.HideInUI = true;
            mouse_focus2f.HideInCharacterSheetAndLevelUp = true;

            var animal_class = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");
            Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel[] mouse_focus = new Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel[] {
                                                Helpers.CreateAddFeatureOnClassLevel(mouse_focus1f, update_lvl, allowed_classes.AddToArray(animal_class), new BlueprintArchetype[] { archetype }, true),
                                                Helpers.CreateAddFeatureOnClassLevel(mouse_focus2f, update_lvl, allowed_classes.AddToArray(animal_class), new BlueprintArchetype[] { archetype }, false)
                                                };
            return mouse_focus;
        }


        Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility createScaledFocus(string name, string display_name, string description,
                                                                                                      string buff_guid, string ability_guid,
                                                                                                      UnityEngine.Sprite icon,
                                                                                                      Kingmaker.Enums.ModifierDescriptor descriptor,
                                                                                                      Kingmaker.EntitySystem.Stats.StatType stat_type,
                                                                                                      (int, int)[] progression,
                                                                                                      BlueprintCharacterClass[] allowed_classes,
                                                                                                      BlueprintArchetype archetype)
        {
            BlueprintComponent[] components = new BlueprintComponent[2]{ CallOfTheWild.Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType(),
                                                                                                                      ContextRankProgression.Custom,
                                                                                                                      classes: allowed_classes,
                                                                                                                      archetype: archetype,
                                                                                                                      customProgression: progression),
                                                                       CallOfTheWild.Helpers.CreateAddContextStatBonus(stat_type,
                                                                                                                        descriptor,
                                                                                                                        Kingmaker.UnitLogic.Mechanics.ContextValueType.Rank)
                                                                     };
            var focus = createToggleFocus(name,
                                          display_name,
                                          description,
                                          buff_guid,
                                          ability_guid,
                                          icon,
                                          components);
            return focus;
        }


        Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility createScaledFocus(string name, string display_name, string description,
                                                                                              string buff_guid, string ability_guid,
                                                                                              UnityEngine.Sprite icon,
                                                                                              BlueprintComponent comp,
                                                                                              (int, int)[] progression,
                                                                                              BlueprintCharacterClass[] allowed_classes,
                                                                                              BlueprintArchetype archetype)
        {
            BlueprintComponent[] components = new BlueprintComponent[2]{ CallOfTheWild.Helpers.CreateContextRankConfig(ContextRankBaseValueType.MaxClassLevelWithArchetype,
                                                                                                                      ContextRankProgression.Custom,
                                                                                                                      classes: allowed_classes,
                                                                                                                      archetype: archetype,
                                                                                                                      customProgression: progression),
                                                                                                                      comp
                                                                     };
            var focus = createToggleFocus(name,
                                          display_name,
                                          description,
                                          buff_guid,
                                          ability_guid,
                                          icon,
                                          components);
            return focus;
        }



        Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility createToggleFocus(string name, string display_name, string description,
                                                                                                      string buff_guid, string ability_guid,
                                                                                                      UnityEngine.Sprite icon, params BlueprintComponent[] components)
        {
            var buff = CallOfTheWild.Helpers.CreateBuff(name + "Buff",
                                                         display_name,
                                                         description,
                                                         buff_guid,
                                                         icon,
                                                         null,
                                                         components);

            var Focus = CallOfTheWild.Helpers.CreateActivatableAbility(name,
                                                                         display_name,
                                                                         description,
                                                                         ability_guid,
                                                                         icon,
                                                                         buff,
                                                                         Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                                                                         Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                                         null);
            Focus.Group = ActivatableAbilityGroupExtension.AnimalFocus.ToActivatableAbilityGroup();
            Focus.WeightInGroup = 1;
            Focus.DeactivateImmediately = true;
            return Focus;
        }
    }
}
