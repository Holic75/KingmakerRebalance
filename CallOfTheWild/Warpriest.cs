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
using Kingmaker.UnitLogic.Alignments;
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
    class Warpriest
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static internal BlueprintCharacterClass warpriest_class;
        static internal BlueprintProgression warpriest_progression;

        static internal BlueprintFeature warpriest_fighter_feat_prerequisite_replacement;
        static internal BlueprintFeature warpriest_orisons;
        static internal BlueprintFeatureSelection blessing_selection;
        static internal BlueprintFeature warpriest_proficiencies;

        static internal BlueprintFeature warpriest_sacred_armor;
        static internal BlueprintFeature warpriest_sacred_armor_ability;
        static internal BlueprintFeature warpriest_sacred_armor2;
        static internal BlueprintFeature warpriest_sacred_armor3;
        static internal BlueprintFeature warpriest_sacred_armor4;
        static internal BlueprintFeature warpriest_sacred_armor5;
        static internal BlueprintAbilityResource sacred_armor_resource;

        static internal BlueprintFeature warpriest_sacred_weapon_damage;
        static internal BlueprintFeature warpriest_sacred_weapon_enhancement;
        static internal BlueprintBuff sacred_weapon_enchancement_buff;
        static internal BlueprintFeature warpriest_sacred_weapon_enhancement2;
        static internal BlueprintFeature warpriest_sacred_weapon_enhancement3;
        static internal BlueprintFeature warpriest_sacred_weapon_enhancement4;
        static internal BlueprintFeature warpriest_sacred_weapon_enhancement5;
        static internal BlueprintAbilityResource sacred_weapon_resource;

        static internal BlueprintFeature warpriest_fervor;
        static internal BlueprintFeatureSelection warpriest_channel_energy;
        static internal BlueprintFeature warpriest_aspect_of_war;
        static internal ActivatableAbilityGroup sacred_weapon_enchancement_group = ActivatableAbilityGroup.TrueMagus;
        static internal ActivatableAbilityGroup sacred_armor_enchancement_group = ActivatableAbilityGroup.ArcaneWeaponProperty;


        internal static void createWarpriestClass()
        {
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");

            warpriest_class = Helpers.Create<BlueprintCharacterClass>();
            warpriest_class.name = "WarpriestClass";
            library.AddAsset(warpriest_class, "");


            warpriest_class.LocalizedName = Helpers.CreateString("Warpriest.Name", "Warpriest");
            warpriest_class.LocalizedDescription = Helpers.CreateString("Warpriest.Description",
                                                                        "Capable of calling upon the power of the gods in the form of blessings and spells, warpriests blend divine magic with martial skill.They are unflinching bastions of their faith, shouting gospel as they pummel foes into submission, and never shy away from a challenge to their beliefs.While clerics might be subtle and use diplomacy to accomplish their aims, warpriests aren’t above using violence whenever the situation warrants it. In many faiths, warpriests form the core of the church’s martial forces—reclaiming lost relics, rescuing captured clergy, and defending the church’s tenets from all challenges.\n"
                                                                        + "Role: Warpriests can serve as capable healers or spellcasters, calling upon their divine powers from the center of the fight, where their armor and martial skills are put to the test.\n"
                                                                        + "Alignment: A warpriest’s alignment must be within one step of his deity’s, along either the law/ chaos axis or the good/ evil axis."
                                                                        );

            warpriest_class.m_Icon = cleric_class.Icon;
            warpriest_class.SkillPoints = cleric_class.SkillPoints;
            warpriest_class.HitDie = DiceType.D8;
            warpriest_class.BaseAttackBonus = cleric_class.BaseAttackBonus;
            warpriest_class.FortitudeSave = cleric_class.FortitudeSave;
            warpriest_class.ReflexSave = cleric_class.ReflexSave;
            warpriest_class.WillSave = cleric_class.WillSave;
            warpriest_class.Spellbook = createWarpriestSpellbook();
            warpriest_class.ClassSkills = cleric_class.ClassSkills;
            warpriest_class.IsDivineCaster = true;
            warpriest_class.IsArcaneCaster = false;
            warpriest_class.StartingGold = cleric_class.StartingGold;
            warpriest_class.PrimaryColor = cleric_class.PrimaryColor;
            warpriest_class.SecondaryColor = cleric_class.SecondaryColor;
            warpriest_class.RecommendedAttributes = cleric_class.RecommendedAttributes;
            warpriest_class.NotRecommendedAttributes = cleric_class.NotRecommendedAttributes;
            warpriest_class.EquipmentEntities = cleric_class.EquipmentEntities;
            warpriest_class.MaleEquipmentEntities = cleric_class.MaleEquipmentEntities;
            warpriest_class.FemaleEquipmentEntities = cleric_class.FemaleEquipmentEntities;
            warpriest_class.ComponentsArray = cleric_class.ComponentsArray;
            warpriest_class.StartingItems = cleric_class.StartingItems;

            createWarpriestProgression();
            warpriest_class.Progression = warpriest_progression;
            //createSacredFist();
            //createChampionOfTheFaith();
            //createCultLeader();
            warpriest_class.Archetypes = new BlueprintArchetype[] { }; // { sacred_fist_archetype, champion_of_the_faith_archetype, cult_leader_archetype };
            Helpers.RegisterClass(warpriest_class);

            //addToPrestigeClasses(); //mt
        }

        static BlueprintCharacterClass[] getWarpriestArray()
        {
            return new BlueprintCharacterClass[] { warpriest_class };
        }


        static BlueprintSpellbook createWarpriestSpellbook()
        {
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var magus_class = library.Get<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
            var warpriest_spellbook = Helpers.Create<BlueprintSpellbook>();
            warpriest_spellbook.Name = warpriest_class.LocalizedName;
            warpriest_spellbook.name = "WarpriestSpellbook";
            library.AddAsset(warpriest_spellbook, "");
            warpriest_spellbook.Name = warpriest_class.LocalizedName;
            warpriest_spellbook.SpellsPerDay = magus_class.Spellbook.SpellsPerDay;
            warpriest_spellbook.SpellsKnown = magus_class.Spellbook.SpellsKnown;
            warpriest_spellbook.Spontaneous = false;
            warpriest_spellbook.IsArcane = false;
            warpriest_spellbook.AllSpellsKnown = true;
            warpriest_spellbook.CanCopyScrolls = false;
            warpriest_spellbook.CastingAttribute = StatType.Wisdom;
            warpriest_spellbook.CharacterClass = warpriest_class;
            warpriest_spellbook.CasterLevelModifier = 0;
            warpriest_spellbook.CantripsType = CantripsType.Orisions;
            warpriest_spellbook.SpellsPerLevel = cleric_class.Spellbook.SpellsPerLevel;
            warpriest_spellbook.SpellList = cleric_class.Spellbook.SpellList;
            return warpriest_spellbook;
        }


        static void createWarpriestProgression()
        {
            createWarpriestProficiencies();
            createWarpriestFighterFeatPrerequisiteReplacement();
            createWarpriestOrisions();
            //createBlessingSelection();
            createSacredWeaponDamage();
            createSacredWeaponEnhancement();
            //createFervor();
            //createChannelEnergy();
            //createSacredArmor();
            //createAspectOfWar();
        }


        static void createWarpriestProficiencies()
        {
            warpriest_proficiencies = library.CopyAndAdd<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb", "WarpriestProficiencies", "");
            warpriest_proficiencies.SetName("Warpriest Proficiencies");
            warpriest_proficiencies.SetDescription("A warpriest is proficient with all simple and martial weapons, as well as the favored weapon of his deity, and with all armor (heavy, light, and medium) and shields (except tower shields). If the warpriest worships a deity with unarmed strike as its favored weapon, the warpriest gains Improved Unarmed Strike as a bonus feat.");
        }


        static void createWarpriestOrisions()
        {
            warpriest_orisons = library.CopyAndAdd<BlueprintFeature>(
                 "e62f392949c24eb4b8fb2bc9db4345e3", // cleric orisions
                 "WarpriestOrisonsFeature",
                 "");
            warpriest_orisons.SetDescription("Warpriests learn a number of orisons, or 0-level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            warpriest_orisons.ReplaceComponent<BindAbilitiesToClass>(c => c.CharacterClass = warpriest_class);
        }


        static void createWarpriestFighterFeatPrerequisiteReplacement()
        {
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var fighter_training = library.Get<BlueprintFeature>("2b636b9e8dd7df94cbd372c52237eebf");
            warpriest_fighter_feat_prerequisite_replacement = Helpers.CreateFeature("WarpriestFighterFeatPrerequisiteReplacement",
                                                                                    "Fighter Training",
                                                                                    "Warpriest treats his warpriest level as his base attack bonus (in addition to base attack bonuses gained from other classes and Hit Dice) for the purpose of qualifying for these feats.\n"
                                                                                    + "Finally, for the purposes of these feats, the warpriest can select feats that have a minimum number of fighter levels as a prerequisite, treating his warpriest level as his fighter level.",
                                                                                    "",
                                                                                    fighter_training.Icon,
                                                                                    FeatureGroup.None,
                                                                                    Common.createClassLevelsForPrerequisites(fighter_class, warpriest_class),
                                                                                    Common.createReplace34BabWithClassLevel(warpriest_class)
                                                                                    );
        }


        static void createSacredWeaponDamage()
        {
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            DiceFormula[] diceFormulas = new DiceFormula[] {new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8)};
            warpriest_sacred_weapon_damage = Helpers.CreateFeature("WarpriestSacredWeaponDamage",
                                                                   "Sacred Weapon",
                                                                   "At 1st level, weapons wielded by a warpriest are charged with the power of his faith. In addition to the favored weapon of his deity, the warpriest can designate a weapon as a sacred weapon by selecting that weapon with the Weapon Focus feat; if he has multiple Weapon Focus feats, this ability applies to all of them. Whenever the warpriest hits with his sacred weapon, the weapon damage is based on his level and not the weapon type. The warpriest can decide to use the weapon’s base damage instead of the sacred weapon damage—this must be declared before the attack roll is made. (If the weapon’s base damage exceeds the sacred weapon damage, its damage is unchanged.) This increase in damage does not affect any other aspect of the weapon, and doesn’t apply to alchemical items, bombs, or other weapons that only deal energy damage.\n"
                                                                   + "The damage dealt by medium warpriest with her sacred weapon is 1d6 at levels 1-4, 1d8 at levels 5-9, 1d10 at levels 10 - 14, 2d6 at levels 15-19 and finally 2d8 at level 20.",
                                                                   "",
                                                                   bless_weapon.Icon,
                                                                   FeatureGroup.None,
                                                                   Common.createContextWeaponDamageDiceReplacement(weapon_focus,
                                                                                                                   Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                                                   diceFormulas),
                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                   type: AbilityRankType.Default,
                                                                                                   progression: ContextRankProgression.DivStep,
                                                                                                   stepLevel: 5,
                                                                                                   classes: getWarpriestArray())
                                                                  );
        }


        static void createSacredWeaponEnhancement()
        {
            var divine_weapon = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var enchants = divine_weapon.GetComponent<ContextActionWeaponEnchantPool>().DefaultEnchantments;
            var enhancement_buff = Helpers.CreateBuff("WarpriestSacredWeaponEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(sacred_armor_enchancement_group,
                                                                                                      false, true,
                                                                                                      enchants.Cast<BlueprintWeaponEnchantment>().ToArray()
                                                                                                      )
                                         );

            sacred_weapon_enchancement_buff = Helpers.CreateBuff("WarpriestSacredWeaponEnchancementSwitchBuff",
                                                                 "Sacred Weapon Enhancement",
                                                                 "At 4th level, the warpriest gains the ability to enhance one of his sacred weapons with divine power as a swift action. This power grants the weapon a +1 enhancement bonus. For every 4 levels beyond 4th, this bonus increases by 1 (to a maximum of +5 at 20th level). The warpriest can use this ability a number of rounds per day equal to his warpriest level, but these rounds need not be consecutive.\n"
                                                                 + "These bonuses stack with any existing bonuses the weapon might have, to a maximum of + 5. The warpriest can enhance a weapon with any of the following weapon special abilities: brilliant energy, disruption, flaming, frost, keen, and shock. In addition, if the warpriest is chaotic, he can add anarchic. If he is evil, he can add unholy. If he is good, he can add holy. If he is lawful, he can add axiomatic. If he is neutral, he can add ghost touch. Adding any of these special abilities replaces an amount of bonus equal to the special ability’s base cost. Duplicate abilities do not stack.",
                                                                 "",
                                                                 divine_weapon.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, dispellable: false)
                                                                                                     )
                                                                 );
            sacred_weapon_enchancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var brilliant_energy = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementBrilliantEnergy",
                                                                        "Sacred Weapon - Brilliant Energy",
                                                                        "A warpriest can add the brilliant energy property to her sacred weapon, but this consumes 4 points of enhancement bonus granted to this weapon.\nA brilliant energy weapon ignores nonliving matter.Armor and shield bonuses to AC(including any enhancement bonuses to that armor) do not count against it because the weapon passes through armor. (Dexterity, deflection, dodge, natural armor, and other such bonuses still apply.) A brilliant energy weapon cannot harm undead, constructs, or objects.",
                                                                        library.Get<BlueprintActivatableAbility>("f1eec5cc68099384cbfc6964049b24fa").Icon,
                                                                        library.Get<BlueprintWeaponEnchantment>("66e9e299c9002ea4bb65b6f300e43770"),
                                                                        4);

            var flaming = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementFlaming",
                                                                "Sacred Weapon - Flaming",
                                                                "A warpriest can add the flaming property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                                library.Get<BlueprintActivatableAbility>("7902941ef70a0dc44bcfc174d6193386").Icon,
                                                                library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                                1);

            var frost = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementFrost",
                                                            "Sacred Weapon - Frost",
                                                            "A warpriest can add the frost property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA frost weapon is sheathed in a terrible, icy cold that deals an extra 1d6 points of cold damage on a successful hit. The cold does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("b338e43a8f81a2f43a73a4ae676353a5").Icon,
                                                            library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b"),
                                                            1);

            var shock = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementShock",
                                                            "Sacred Weapon - Shock",
                                                            "A warpriest can add the shock property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA shock weapon is sheathed in crackling electricity that deals an extra 1d6 points of electricity damage on a successful hit. The electricity does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("a3a9e9a2f909cd74e9aee7788a7ec0c6").Icon,
                                                            library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658"),
                                                            1);

            var ghost_touch = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementGhostTouch",
                                                                    "Sacred Weapon - Ghost Touch",
                                                                    "A warpriest can add the ghost touch property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA ghost touch weapon deals damage normally against incorporeal creatures, regardless of its bonus. An incorporeal creature's 50% reduction in damage from corporeal sources does not apply to attacks made against it with ghost touch weapons.",
                                                                    library.Get<BlueprintActivatableAbility>("688d42200cbb2334c8e27191c123d18f").Icon,
                                                                    library.Get<BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f"),
                                                                    1,
                                                                    AlignmentMaskType.LawfulNeutral | AlignmentMaskType.NeutralEvil | AlignmentMaskType.NeutralGood | AlignmentMaskType.ChaoticNeutral | AlignmentMaskType.TrueNeutral);

            var keen = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementKeen",
                                                            "Sacred Weapon - Keen",
                                                            "A warpriest can add the keen property to her sacred weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                            library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                            library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                            1);

            var disruption = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementDisruption",
                                                                    "Sacred Weapon - Disruption",
                                                                    "A warpriest can add the disruption property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA disruption weapon is the bane of all undead. Any undead creature struck in combat must succeed on a DC 14 Will save or be destroyed. A disruption weapon must be a bludgeoning melee weapon.",
                                                                    library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                                    library.Get<BlueprintWeaponEnchantment>("0f20d79b7049c0f4ca54ca3d1ea44baa"),
                                                                    2);

            var holy = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementHoly",
                                                            "Sacred Weapon - Holy",
                                                            "A warpriest can add the holy property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA holy weapon is imbued with holy power. This power makes the weapon good-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of evil alignment.",
                                                            library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8").Icon,
                                                            library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140"),
                                                            2,
                                                            AlignmentMaskType.Good);

            var unholy = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementUnholy",
                                                            "Sacred Weapon - Unholy",
                                                            "A warpriest can add the unholy property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn unholy weapon is imbued with unholy power. This power makes the weapon evil-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of good alignment.",
                                                            library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce").Icon,
                                                            library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"),
                                                            2,
                                                            AlignmentMaskType.Evil);

            var axiomatic = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementAxiomatic",
                                                            "Sacred Weapon - Axiomatic",
                                                            "A warpriest can add the axiomatic property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn axiomatic weapon is infused with lawful power. It makes the weapon lawful-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against chaotic creatures.",
                                                            library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,
                                                            library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4"),
                                                            2,
                                                            AlignmentMaskType.Lawful);

            var anarchic = createSacredWeaponEnhancementFeature("WarpriestSacredWeaponEnchancementAnarchic",
                                                "Sacred Weapon - Anarchic",
                                                "A warpriest can add the anarchic property to her sacred weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn anarchic weapon is infused with the power of chaos. It makes the weapon chaotic-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of lawful alignment.",
                                                library.Get<BlueprintActivatableAbility>("8ed07b0cc56223c46953348f849f3309").Icon,
                                                library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"),
                                                2,
                                                AlignmentMaskType.Chaotic);


            sacred_weapon_resource = Helpers.CreateAbilityResource("WarpriestSacredWeaponResource", "", "", "", null);
            sacred_weapon_resource.SetIncreasedByLevel(0, 1, getWarpriestArray());

            var sacred_weapon_ability = Helpers.CreateActivatableAbility("WarpriestSacredWeaponEnchantmentToggleAbility",
                                                                         sacred_weapon_enchancement_buff.Name,
                                                                         sacred_weapon_enchancement_buff.Description,
                                                                         "",
                                                                         sacred_weapon_enchancement_buff.Icon,
                                                                         sacred_weapon_enchancement_buff,
                                                                         AbilityActivationType.Immediately,
                                                                         CommandType.Swift,
                                                                         null,
                                                                         Helpers.CreateActivatableResourceLogic(sacred_weapon_resource, ResourceSpendType.NewRound));

            warpriest_sacred_weapon_enhancement = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement",
                                                                        sacred_weapon_ability.Name,
                                                                        sacred_weapon_ability.Description,
                                                                        "",
                                                                        sacred_weapon_ability.Icon,
                                                                        FeatureGroup.None,
                                                                        Helpers.CreateAddAbilityResource(sacred_weapon_resource),
                                                                        Helpers.CreateAddFacts(flaming, frost, shock, ghost_touch, keen)
                                                                        );

            warpriest_sacred_weapon_enhancement2 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement2",
                                                            sacred_weapon_ability.Name,
                                                            sacred_weapon_ability.Description,
                                                            "",
                                                            sacred_weapon_ability.Icon,
                                                            FeatureGroup.None,
                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group),
                                                            Helpers.CreateAddFacts(disruption, holy, unholy, axiomatic, anarchic)
                                                            );

            warpriest_sacred_weapon_enhancement3 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement3",
                                                                            sacred_weapon_ability.Name,
                                                                            sacred_weapon_ability.Description,
                                                                            "",
                                                                            sacred_weapon_ability.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group)
                                                                            );

            warpriest_sacred_weapon_enhancement4 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement4",
                                                                            sacred_weapon_ability.Name,
                                                                            sacred_weapon_ability.Description,
                                                                            "",
                                                                            sacred_weapon_ability.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group),
                                                                            Helpers.CreateAddFact(brilliant_energy)
                                                                            );

            warpriest_sacred_weapon_enhancement5 = Helpers.CreateFeature("WarpriestSacredWeaponEnchancement5",
                                                                            sacred_weapon_ability.Name,
                                                                            sacred_weapon_ability.Description,
                                                                            "",
                                                                            sacred_weapon_ability.Icon,
                                                                            FeatureGroup.None,
                                                                            Common.createIncreaseActivatableAbilityGroupSize(sacred_weapon_enchancement_group)
                                                                            );
        }


        static BlueprintActivatableAbility createSacredWeaponEnhancementFeature(string name_prefix, string display_name, string description, UnityEngine.Sprite icon,
                                                                           BlueprintWeaponEnchantment enchantment, int group_size, AlignmentMaskType alignment = AlignmentMaskType.Any)
        {
            //create buff
            //create activatable ability that gives buff
            //on main buff in activate add corresponding enchantment
            //remove it in deactivate
            //create feature that gives activatable ability
            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          icon,
                                          null,
                                          Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, true,
                                                                                           new Kingmaker.Blueprints.Items.Weapons.BlueprintWeaponType[0],
                                                                                           enchantment)
                                                                                           );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var switch_buff = Helpers.CreateBuff(name_prefix + "SwitchBuff",
                                  display_name,
                                  description,
                                  "",
                                  icon,
                                  null);

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(sacred_weapon_enchancement_buff, buff, switch_buff);

            var ability = Helpers.CreateActivatableAbility(name_prefix + "ToggleAbility",
                                                                        display_name,
                                                                        description,
                                                                        "",
                                                                        icon,
                                                                        switch_buff,
                                                                        AbilityActivationType.Immediately,
                                                                        CommandType.Free,
                                                                        null
                                                                        );
            ability.WeightInGroup = group_size;
            ability.Group = sacred_armor_enchancement_group;

            if (alignment != AlignmentMaskType.Any)
            {
                ability.AddComponent(Helpers.Create<NewMechanics.ActivatableAbilityAlignmentRestriction>(c => c.Alignment = alignment));
            }
            return ability;
        }


        static void createSacredArmor()
        {
            //energy resistance - normal (10 +2), improved (20 +4), greater (+5)  - fire, cold, shock, acid = 12 //pick existing
            //fortification - light (25% +1), medium(50% +3), heavy (75% +5) //create
            //spell resistance (+13 +2), (+15 +3), (+17 +4), (+19, +5) //create
        }
    }
}
