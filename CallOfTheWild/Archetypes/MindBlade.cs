using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
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
using System.Collections.Generic;
using System.Linq;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class MindBlade
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature psychic_spellcasting;
        static public BlueprintFeature psychic_pool;
        static public BlueprintFeature dual_weapons;
        static public BlueprintFeature dual_manifest;
        static public BlueprintFeature rapid_manifest;
        static public BlueprintFeature mindblade_spellbook;
        static public BlueprintFeatureSelection psychic_access;
        static public BlueprintAbility ability_light, ability_1h, ability_2h, ability_double, ability_light2, ability_1h2;
        static public List<BlueprintAbility> manifestation_abilities = new List<BlueprintAbility>();
        static public BlueprintAbilityResource psychic_pool_resource;

        static public BlueprintFeature[] psychic_pool_update = new BlueprintFeature[6];
        static public BlueprintFeature psychic_pool_enchantments;
        static List<BlueprintBuff> mind_blade_weapon_buffs = new List<BlueprintBuff>();
        static List<BlueprintBuff> mind_blade_2weapon_buffs = new List<BlueprintBuff>();
        static BlueprintWeaponEnchantment psychic_weapon_enchant;

        static public BlueprintSpellbook spellbook;
        static public BlueprintSpellList extra_psychic_spell_list;


        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var magus_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MindBladeMagusArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Mindblade");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A mindblade blends psychic talent and martial skill to lethal effect. By forming weapons with her mind, she always has the right tool for any situation.");
            });
            Helpers.SetField(archetype, "m_ParentClass", magus_class);
            library.AddAsset(archetype, "");

            archetype.ChangeCasterType = true;
            archetype.IsArcaneCaster = false;

            
            createPsychicSpellcasting();
            createPsychicAccess();
            createPsychicPoolAndDualWeapons();
            createPsychicPoolEnchants();
            createRapidManifest();
            createDualManifest();

            var arcane_pool_feature = library.Get<BlueprintFeature>("3ce9bb90749c21249adc639031d5eed1");
            var magus_arcana = library.Get<BlueprintFeatureSelection>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            var improved_spell_combat = library.Get<BlueprintFeature>("836879fcd5b29754eb664a090bd6c22f");
            var greater_spell_combat = library.Get<BlueprintFeature>("379887a82a7248946bbf6d0158663b5e");
            var medium_armor = library.Get<BlueprintFeature>("b24897e082896654c8dd64c8fb677363");
            var heavy_armor = library.Get<BlueprintFeature>("447ca91389e5c9246acb2c640d63f4da");

            var arcane_weapon5 = library.Get<BlueprintFeature>("36b609a6946733c42930c55ac540416b");
            var arcane_weapon9 = library.Get<BlueprintFeature>("70be888059f99a245a79d6d61b90edc5");
            var arcane_weapon13 = library.Get<BlueprintFeature>("1804187264121cd439d70a96234d4ddb");
            var arcane_weapon17 = library.Get<BlueprintFeature>("3cbe3e308342b3247ba2f4fbaf5e6307");
            var spell_recall = library.Get<BlueprintFeature>("61fc0521e9992624e9c518060bf89c0f");
            var improved_spell_recall = library.Get<BlueprintFeature>("0ef6ec1c2fdfc204fbd3bff9f1609490");
            var greater_spell_access = library.Get<BlueprintFeature>("de18c849c41dbfa44801d812376c707d");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, arcane_pool_feature),
                                                          Helpers.LevelEntry(4, spell_recall),
                                                          Helpers.LevelEntry(5, arcane_weapon5),
                                                          Helpers.LevelEntry(7, medium_armor),
                                                          Helpers.LevelEntry(8, improved_spell_combat),
                                                          Helpers.LevelEntry(9, arcane_weapon9),
                                                          Helpers.LevelEntry(11, improved_spell_recall),
                                                          Helpers.LevelEntry(13, arcane_weapon13, heavy_armor),
                                                          Helpers.LevelEntry(14, greater_spell_combat),
                                                          Helpers.LevelEntry(17, arcane_weapon17),
                                                          Helpers.LevelEntry(19, greater_spell_access)
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, psychic_spellcasting, psychic_pool),
                                                          Helpers.LevelEntry(3, psychic_pool_update[0]),
                                                          Helpers.LevelEntry(4, psychic_access),
                                                          Helpers.LevelEntry(5, psychic_pool_enchantments),
                                                          Helpers.LevelEntry(6, psychic_pool_update[1]),
                                                          Helpers.LevelEntry(7, psychic_access, dual_weapons),
                                                          Helpers.LevelEntry(8, rapid_manifest),
                                                          Helpers.LevelEntry(9, psychic_pool_update[2]),
                                                          Helpers.LevelEntry(11, psychic_access),
                                                          Helpers.LevelEntry(12, psychic_pool_update[3]),
                                                          Helpers.LevelEntry(13, dual_manifest),
                                                          Helpers.LevelEntry(14, psychic_access),
                                                          Helpers.LevelEntry(15, psychic_pool_update[4]),
                                                          Helpers.LevelEntry(18, psychic_pool_update[5]),
                                                          Helpers.LevelEntry(19, psychic_access),
                                                       };
            magus_class.Archetypes = magus_class.Archetypes.AddToArray(archetype);

            magus_class.Progression.UIGroups = magus_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(psychic_pool_update.AddToArray(psychic_pool)));
            magus_class.Progression.UIGroups = magus_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(dual_weapons, rapid_manifest, dual_manifest));
            magus_class.Progression.UIDeterminatorsGroup = magus_class.Progression.UIDeterminatorsGroup.AddToArray(psychic_spellcasting);


            var restricted_arcanas_ids = new string[]
            {
                "a2e0691dcfda2374e84d8bbf480e06a0",//bane blade
                "4be0bb10e110a35419e406da767bd1e3",//devoted blade
                "8896f327c59569c4eaf129bf35b96c1f",//ghost blade
                "7a73bf165e8eda6478b4419f857d1ab5",//enduring blade
            };

            foreach (var id in restricted_arcanas_ids)
            {
                var feature = library.Get<BlueprintFeature>(id);
                feature.AddComponent(Common.prerequisiteNoArchetype(magus_class, archetype));
            }
            archetype.ReplaceSpellbook = spellbook;
        }


        static void createPsychicSpellcasting()
        {
            spellbook = library.CopyAndAdd<BlueprintSpellbook>("e2763fbfdb91920458c4686c3e7ed085", "MindbladeSpellbook", "");
            spellbook.IsArcane = false;
            spellbook.CastingAttribute = Kingmaker.EntitySystem.Stats.StatType.Intelligence;
            spellbook.ComponentsArray = new BlueprintComponent[0];

            psychic_spellcasting = Helpers.CreateFeature("MindBladePsychicSpellCasting",
                                                         "Psychic Spell Casting",
                                                         "A mindblade casts psychic spells from magus spell list as psychic spells. To learn or cast a spell, a mindblade must have an Intelligence score equal to at least 10 + the spell’s level. The saving throw DC against a mindblade’s spell is 10 + the spell’s level + the mindblade’s Intelligence modifier.\n"
                                                         + "A mindblade need not prepare her spells in advance.\nShe can cast any magus spell she knows at any time, assuming she has not yet used up her allotment of spells per day for the spell’s level."
                                                         + "Mindblade spells are not subject to arcane spell failure due to armor, but they require a more significant effort, compared to classic magic and thus the DC of all concentration checks required as a part of casting a psychic spell is increased by 10, additionaly psychic magic can not be used at all if caster is under the influence of fear or negative emotion effects.",
                                                         "",
                                                         Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"),//mind fog
                                                         FeatureGroup.None,
                                                         Helpers.Create<SpellFailureMechanics.PsychicSpellbook>(p => p.spellbook = spellbook),
                                                         Helpers.Create<SpellbookMechanics.AddUndercastSpells>(p => p.spellbook = spellbook),
                                                         Helpers.CreateAddFact(Investigator.center_self),
                                                         Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.NaturalSpell)
                                                         );
            //disallow prestige classes
            var prestige_spellbooks = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("9a399ad2c35d4ff449789fcd7f02943b"),//ek
                library.Get<BlueprintFeature>("f10f1ddf993f6224b8b744ac8c89b82d"),//arcane trickster
                library.Get<BlueprintFeature>("2803ed6f49730cf4a9616880e0979269")//mt
            };
           
            foreach (var ps in prestige_spellbooks)
            {
                ps.AddComponent(Common.prerequisiteNoArchetype(archetype));
            }
            spellbook.AddComponent(Helpers.Create<SpellbookMechanics.PsychicSpellbook>());
        }


        static void createPsychicAccess()
        {
            extra_psychic_spell_list = Common.combineSpellLists("MindBladePsychicSpellList", Investigator.psychic_detective_spellbook.SpellList);
            var find_traps = library.Get<BlueprintAbility>("4709274b2080b6444a3c11c6ebbe2404");
            var banishment = library.Get<BlueprintAbility>("d361391f645db984bbf58907711a146a");
            Common.excludeSpellsFromList(extra_psychic_spell_list, find_traps, banishment);
            Common.addSpellToSpellList(extra_psychic_spell_list, find_traps, 2);
            Common.excludeSpellsFromList(extra_psychic_spell_list, archetype.GetParentClass().Spellbook.SpellList);

            psychic_access = Helpers.CreateFeatureSelection("PsychicAccessFeatureSelection",
                                                            "Psychic Access",
                                                            "At 4th level, the mindblade can add a psychic spell to the list of her spells known. These must be a spell of level the mindblade is able to cast. At 7th, 11th, 14th, and 19th levels, she adds one more psychic class spell to her list of spells known, following the same restrictions.\n"
                                                            + "When a mindblade takes the spell blending arcana, she gains spells from the psychic class spell list instead of the wizard spell list.",
                                                            "",
                                                            null,
                                                            FeatureGroup.None);
            psychic_access.Ranks = 5;

            for (int i = 1; i <= 6; i++)
            {
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956", $"PsychicAccess{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = archetype.GetParentClass();
                learn_spell.SpellList = extra_psychic_spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = extra_psychic_spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = archetype.GetParentClass(); });
                learn_spell.AddComponents(
                                          Common.createPrerequisiteClassSpellLevel(archetype.GetParentClass(), i)
                                          );
                learn_spell.SetName(Helpers.CreateString( $"PsychicAccessParametrizedFeature{i + 1}.Name", "Psychic Access " + $"(Level {i})"));
                learn_spell.SetDescription(psychic_access.Description);
                learn_spell.SetIcon(psychic_access.Icon);

                psychic_access.AllFeatures = psychic_access.AllFeatures.AddToArray(learn_spell);
            }


        }


        static void createPsychicPoolEnchants()
        {
            var brilliant_energy = createEnchantmentAbility("MindBladeWeaponEnchancementBrilliantEnergy",
                                                            "Psychic Weapon - Brilliant Energy",
                                                            "A mindblade can add the brilliant energy property to her psychic weapon, but this consumes 4 points of enhancement bonus granted to this weapon.\nA brilliant energy weapon ignores nonliving matter.Armor and shield bonuses to AC (including any enhancement bonuses to that armor) do not count against it because the weapon passes through armor. (Dexterity, deflection, dodge, natural armor, and other such bonuses still apply.) A brilliant energy weapon cannot harm undead, constructs, or objects.",
                                                            library.Get<BlueprintActivatableAbility>("f1eec5cc68099384cbfc6964049b24fa").Icon,
                                                            library.Get<BlueprintWeaponEnchantment>("66e9e299c9002ea4bb65b6f300e43770"),
                                                            4, ActivatableAbilityGroup.ArcaneWeaponProperty);
            
            var flaming = createEnchantmentAbility("MindBladeWeaponEnchancementFlaming",
                                                    "Psychic Weapon - Flaming",
                                                    "A mindblade can add the flaming property to her psychic weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                    library.Get<BlueprintActivatableAbility>("05b7cbe45b1444a4f8bf4570fb2c0208").Icon,
                                                    library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                    1, ActivatableAbilityGroup.ArcaneWeaponProperty);
            var speed = createEnchantmentAbility("MindBladeWeaponEnchancementSpeed",
                                                    "Psychic Weapon - Speed",
                                                    "A mindblade can add the speed property to her psychic weapon, but this consumes 3 points of enhancement bonus granted to this weapon.\nWhen making a full attack, the wielder of a speed weapon may make one extra attack with it. The attack uses the wielder's full base attack bonus, plus any modifiers appropriate to the situation. (This benefit is not cumulative with similar effects, such as a haste spell.)",
                                                    library.Get<BlueprintActivatableAbility>("85742dd6788c6914f96ddc4628b23932").Icon,
                                                    library.Get<BlueprintWeaponEnchantment>("f1c0c50108025d546b2554674ea1c006"),
                                                    3, ActivatableAbilityGroup.ArcaneWeaponProperty);

            var frost = createEnchantmentAbility("MindBladeWeaponEnchancementFrost",
                                                "Psychic Weapon - Frost",
                                                "A mindblade can add the frost property to her psychic weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA frost weapon is sheathed in a terrible, icy cold that deals an extra 1d6 points of cold damage on a successful hit. The cold does not harm the wielder.",
                                                library.Get<BlueprintActivatableAbility>("b338e43a8f81a2f43a73a4ae676353a5").Icon,
                                                library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b"),
                                                1, ActivatableAbilityGroup.ArcaneWeaponProperty);

            var shock = createEnchantmentAbility("MindBladeWeaponEnchancementShock",
                                                "Psychic Weapon - Shock",
                                                "A mindblade can add the shock property to her psychic weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA shock weapon is sheathed in crackling electricity that deals an extra 1d6 points of electricity damage on a successful hit. The electricity does not harm the wielder.",
                                                library.Get<BlueprintActivatableAbility>("a3a9e9a2f909cd74e9aee7788a7ec0c6").Icon,
                                                library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658"),
                                                1, ActivatableAbilityGroup.ArcaneWeaponProperty);

            var ghost_touch = createEnchantmentAbility("MindBladeWeaponEnchancementGhostTouch",
                                                        "Psychic Weapon - Ghost Touch",
                                                        "A mindblade can add the ghost touch property to her psychic weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA ghost touch weapon deals damage normally against incorporeal creatures, regardless of its bonus. An incorporeal creature's 50% reduction in damage from corporeal sources does not apply to attacks made against it with ghost touch weapons.",
                                                        library.Get<BlueprintActivatableAbility>("688d42200cbb2334c8e27191c123d18f").Icon,
                                                        library.Get<BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f"),
                                                        1, ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                        AlignmentMaskType.TrueNeutral);

            var keen = createEnchantmentAbility("MindBladeWeaponEnchancementKeen",
                                                "Psychic Weapon - Keen",
                                                "A mindblade can add the keen property to her psychic weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                1, ActivatableAbilityGroup.ArcaneWeaponProperty);

            var holy = createEnchantmentAbility("MindBladeWeaponEnchancementHoly",
                                                "Psychic Weapon - Holy",
                                                "A mindblade can add the holy property to her psychic weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA holy weapon is imbued with holy power. This power makes the weapon good-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of evil alignment.",
                                                library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8").Icon,
                                                library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140"),
                                                2, ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                AlignmentMaskType.Good);

            var unholy = createEnchantmentAbility("MindBladeWeaponEnchancementUnholy",
                                                    "Psychic Weapon - Unholy",
                                                    "A mindblade can add the unholy property to her psychic weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn unholy weapon is imbued with unholy power. This power makes the weapon evil-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of good alignment.",
                                                    library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce").Icon,
                                                    library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"),
                                                    2, ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                    AlignmentMaskType.Evil);

            var axiomatic = createEnchantmentAbility("MindBladeWeaponEnchancementAxiomatic",
                                                    "Psychic Weapon - Axiomatic",
                                                    "A mindblade can add the axiomatic property to her psychic weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn axiomatic weapon is infused with lawful power. It makes the weapon lawful-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against chaotic creatures.",
                                                    library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,
                                                    library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4"),
                                                    2, ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                    AlignmentMaskType.Lawful);

            var anarchic = createEnchantmentAbility("MindBladeWeaponEnchancementAnarchic",
                                                "Psychic Weapon - Anarchic",
                                                "A mindblade can add the anarchic property to her psychic weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn anarchic weapon is infused with the power of chaos. It makes the weapon chaotic-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of lawful alignment.",
                                                library.Get<BlueprintActivatableAbility>("8ed07b0cc56223c46953348f849f3309").Icon,
                                                library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"),
                                                2, ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                AlignmentMaskType.Chaotic);
            var bane = createEnchantmentAbility("MindBladeWeaponEnchancementBane",
                                                "Psychic Weapon - Bane",
                                                "A mindblade can add the bane property to her psychic weapon(s), but she needs to pay 1 extra point from her psychic pool per manifested weapon (double weapons count as two weapons). These extra points are not recovered upon dismissing manifested weapons.\nA bane weapon excels against certain foes. Against a designated foe, the weapon's enhancement bonus is +2 better than its actual bonus. It also deals an extra 2d6 points of damage against such foes.",
                                                library.Get<BlueprintActivatableAbility>("3a909d1effa3bbc4084f2b5ac95f5306").Icon,
                                                library.Get<BlueprintWeaponEnchantment>("1a93ab9c46e48f3488178733be29342a"), //bane everything
                                                1, ActivatableAbilityGroup.None,
                                                AlignmentMaskType.Chaotic);

            for (int i = 0; i < 6; i++)
            {
                psychic_pool_update[i] = Helpers.CreateFeature($"MindBladeWeaponEnhancement{i+1}Feature",
                                                                "Psychic Pool",
                                                                psychic_pool.Description,
                                                                "",
                                                                psychic_pool.Icon,
                                                                FeatureGroup.None,
                                                                Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.ArcaneWeaponProperty)
                                                                );
            }
            psychic_pool_enchantments = Helpers.CreateFeature($"MindBladeWeaponEnhancementsFeature",
                                                                "Psychic Pool",
                                                                psychic_pool.Description,
                                                                "",
                                                                psychic_pool.Icon,
                                                                FeatureGroup.None,
                                                                Helpers.CreateAddFacts(flaming, frost, shock, keen)
                                                                );
            psychic_pool_enchantments.HideInUI = true;
            psychic_pool_enchantments.HideInCharacterSheetAndLevelUp = true;
            psychic_pool_update[2].AddComponent(Helpers.CreateAddFacts(speed));

            var devoted_blade = library.CopyAndAdd<BlueprintFeature>("4be0bb10e110a35419e406da767bd1e3", "MindBladeDevotedBladeFeature", "");
            devoted_blade.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAddFacts(anarchic, axiomatic, holy, unholy),
                Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, 12)
            };


            var ghost_blade = library.CopyAndAdd<BlueprintFeature>("8896f327c59569c4eaf129bf35b96c1f", "MindBladeGhostBladeFeature", "");
            ghost_blade.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAddFacts(ghost_blade, brilliant_energy),
                Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, 9)
            };


            var bane_blade = library.CopyAndAdd<BlueprintFeature>("a2e0691dcfda2374e84d8bbf480e06a0", "MindBladeBaneBladeFeature", "");
            bane_blade.SetDescription(bane.Description);
            bane_blade.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAddFacts(bane),
                Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, 15)
            };
            var arcana_selection = library.Get<BlueprintFeatureSelection>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            arcana_selection.AllFeatures = arcana_selection.AllFeatures.AddToArray(devoted_blade, ghost_blade, bane_blade);


            foreach (var a in manifestation_abilities)
            {
                a.GetComponent<AbilityResourceLogic>().CostIsCustom = true;
                if (ability_1h2.Variants.Contains(a) || ability_light2.Variants.Contains(a) || ability_double.Variants.Contains(a))
                {
                    a.AddComponent(Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_increasing_facts = new Kingmaker.Blueprints.Facts.BlueprintFact[] { bane.Buff, bane.Buff }));
                }
                else
                {
                    a.AddComponent(Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_increasing_facts = new Kingmaker.Blueprints.Facts.BlueprintFact[] { bane.Buff }));
                }
            }



        }


        static public BlueprintActivatableAbility createEnchantmentAbility(string name_prefix, string display_name, string description, UnityEngine.Sprite icon,
                                                            BlueprintWeaponEnchantment enchantment, int group_size, ActivatableAbilityGroup group,
                                                            AlignmentMaskType alignment = AlignmentMaskType.Any)
        {
            //create buff
            //create activatable ability that gives buff
            //on main buff in activate add corresponding enchantment
            //create feature that gives activatable ability

            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                            display_name,
                                            description,
                                            "",
                                            icon,
                                            null,
                                            Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, false,
                                                                                            (BlueprintWeaponEnchantment)enchantment)
                                                                                            );

            var buff2 = Helpers.CreateBuff(name_prefix + "BothHandsBuff",
                                display_name,
                                description,
                                "",
                                icon,
                                null,
                                Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, false,
                                                                                enchantment),
                                Common.createBuffContextEnchantSecondaryHandWeapon(Common.createSimpleContextValue(1), false, false,
                                                                                enchantment)
                                                                                );


            buff.SetBuffFlags(BuffFlags.HiddenInUi);
            buff2.SetBuffFlags(BuffFlags.HiddenInUi);
            var switch_buff = Helpers.CreateBuff(name_prefix + "SwitchBuff",
                                  display_name,
                                  description,
                                  "",
                                  icon,
                                  null);
            switch_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            foreach (var bb in mind_blade_weapon_buffs)
            {
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bb, buff, switch_buff);
            }
            foreach (var bb in mind_blade_2weapon_buffs)
            {
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bb, buff2, switch_buff);
            }

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
            ability.Group = group;
            ability.DeactivateImmediately = true;

            if (alignment != AlignmentMaskType.Any)
            {
                ability.AddComponent(Helpers.Create<NewMechanics.ActivatableAbilityAlignmentRestriction>(c => c.Alignment = alignment));
            }
            return ability;
        }


        static void createPsychicPoolAndDualWeapons()
        { 
            var abilities = new List<BlueprintAbility>();
            var brilliant_energy = library.Get<BlueprintWeaponEnchantment>("66e9e299c9002ea4bb65b6f300e43770");
            psychic_weapon_enchant = Common.createWeaponEnchantment("ManifestedWeaponEnchant",
                                                          "Manifested",
                                                          "This is a weapon manifested by the power of your will.",
                                                          "Manifested",
                                                          "",
                                                          "",
                                                          0,
                                                          brilliant_energy.WeaponFxPrefab
                                                          );

            string description = "A mindblade gains a psychic pool, similar to a normal magus’s arcane pool. At 1st level, a mindblade can expend 1 point from her psychic pool as a standard action to manifest a light melee weapon of her choice, formed from psychic energy. By spending 2 points, the mindblade can manifest a one-handed melee weapon, and by spending 3 points, she can manifest a two-handed melee weapon (but not a double weapon). This psychic weapon can last indefinitely, but it vanishes if it leaves the mindblade’s hand. The mindblade can dismiss a held psychic weapon as a free action. When a psychic weapon vanishes, the mindblade regains the psychic energy used to create it. She can maintain only one weapon at a time.\n"
                                 + "At 1st level, a psychic weapon counts as a magic weapon of whatever type the mindblade selected, with a +1 enhancement bonus. At 3rd level and every 3 levels thereafter, the weapon’s enhancement bonus increases by 1, up to maximum of +5 at 12th level. Starting at 5th level, the mindblade can add any of the weapon special abilities listed in the arcane pool class feature in place of these bonuses. At 15th and 18th levels, the weapon gains an additional +1 enhancement bonus, which the mindblade can spend only on weapon special abilities.";
            var abilities_light = new List<BlueprintAbility>();
            var abilities_1h = new List<BlueprintAbility>();
            var abilities_2h = new List<BlueprintAbility>();
            var abilities_1h2 = new List<BlueprintAbility>();
            var abilities_light2 = new List<BlueprintAbility>();
            var abilities_double = new List<BlueprintAbility>();


            var allow_two_weapons_buff = Helpers.CreateBuff("AllowTwoPsychicWeaponsBuff",
                                                            "Dual Weapons",
                                                            "At 7th level, a mindblade can maintain two psychic weapons at a time or a psychic double weapon, though each weapon (or each end of a double weapon) has an enhancement bonus 1 lower than normal. Starting at 12th level, each of the two psychic weapons (or each end of a double weapon) instead has an enhancement bonus 2 lower than normal. When using two-weapon fighting with two psychic weapons or a psychic double weapon, the mindblade can use her spell combat ability as though she had a hand free.",
                                                            "",
                                                            Helpers.GetIcon("ac8aaf29054f5b74eb18f2af950e752d"),
                                                            null);
            allow_two_weapons_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var toggle_2_weapons = Helpers.CreateActivatableAbility("AllowTwoPsychicWeaponsToggleAbility",
                                                                    allow_two_weapons_buff.Name,
                                                                    allow_two_weapons_buff.Description,
                                                                    "",
                                                                    allow_two_weapons_buff.Icon,
                                                                    allow_two_weapons_buff,
                                                                    AbilityActivationType.Immediately,
                                                                    CommandType.Free,
                                                                    null);
            toggle_2_weapons.Group = ActivatableAbilityGroup.ArcaneWeaponProperty;
            toggle_2_weapons.DeactivateImmediately = true;
            var toggle_2_weapons2 = library.CopyAndAdd(toggle_2_weapons, "AllowTwoPsychicWeapons2ToggleAbility", "");
            toggle_2_weapons2.WeightInGroup = 2;                                       

            
            var weapon_categories = EnumUtils.GetValues<WeaponCategory>().ToArray();

            var enchants = WeaponEnchantments.temporary_enchants;
            var resource = library.Get<BlueprintAbilityResource>("effc3e386331f864e9e06d19dc218b37");
            foreach (var wc in weapon_categories)
            {
                var weapon = Game.Instance.BlueprintRoot.Progression.CategoryDefaults.Entries.Where(e => e.Key == wc && e.DefaultWeapon != null).FirstOrDefault()?.DefaultWeapon;
                if (weapon == null || !weapon.IsMelee || weapon.IsNatural || weapon.IsUnarmed)
                {
                    continue;
                }

                weapon = library.CopyAndAdd(weapon, "Psychic" + weapon.name, "$" + Helpers.MergeIds(weapon.AssetGuid, "52a5f2eb09f84ea88aaa60c05685c56a"));
                Common.addEnchantment(weapon, WeaponEnchantments.summoned_weapon_enchant);
                Common.addEnchantment(weapon, psychic_weapon_enchant);
                if (weapon.Double)
                {
                    var second_weapon = library.CopyAndAdd(weapon.SecondWeapon, "Psychic" + weapon.name + "Second", "$" + Helpers.MergeIds(weapon.SecondWeapon.AssetGuid, "230f40f9318d4d449fc11ec07110b696"));
                    second_weapon.SecondWeapon = second_weapon;
                    weapon.SecondWeapon = second_weapon;
                    Common.addEnchantment(second_weapon, WeaponEnchantments.summoned_weapon_enchant);
                    Common.addEnchantment(second_weapon, psychic_weapon_enchant);

                }

                bool is_light = weapon.IsLight;
                bool is_2h = weapon.IsTwoHanded && (weapon.Category != WeaponCategory.BastardSword) && (weapon.Category != WeaponCategory.DwarvenWaraxe);
                bool is_double = weapon.Double;
                bool is_normal = !is_2h && !is_double && !is_light;
                bool requires_2h = is_2h || is_double;
                int resource_amount = 1;
                if (is_normal)
                {
                    resource_amount = 2;
                }
                else if (requires_2h)
                {
                    resource_amount = 3;
                }

                var buff = Helpers.CreateBuff(wc.ToString() + "ManifestWeaponBuff",
                                              "",
                                              "",
                                              "$" + Helpers.MergeIds(weapon.AssetGuid, "ad5498957e7f4f7c9f0b848dd2242aaf"),
                                              null,
                                              null,
                                              Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => c.weapon = weapon),
                                              Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                                                                              false, false, enchants),
                                              Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => { c.Resource = resource; c.amount = resource_amount; }))
                                              );
                buff.SetBuffFlags(BuffFlags.RemoveOnRest);
                if (is_double)
                {
                    buff.AddComponent(Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeaponOffHand(ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                                                                          false, false, enchants));
                    mind_blade_2weapon_buffs.Add(buff);
                }
                else
                {
                    mind_blade_weapon_buffs.Add(buff);
                }
                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
                var ability = Helpers.CreateAbility(wc.ToString() + "ManifestWeaponAbility",
                                                    "Manifest " + LocalizedTexts.Instance.Stats.GetText(wc),
                                                    description,
                                                    "$" + Helpers.MergeIds(weapon.AssetGuid, "5fbf184cb3504e87b52db6593acbe940"),
                                                    weapon.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(apply_buff),
                                                    resource.CreateResourceLogic(amount: resource_amount),
                                                    Helpers.Create<NewMechanics.AbilityCasterPrimaryHandFree>(a => a.for_2h_item = requires_2h),
                                                    Helpers.Create<NewMechanics.AbilityShowIfCasterProficientWithWeaponCategory>(a => { a.category = wc; a.require_full_proficiency = true; })
                                                    );
                abilities.Add(ability);
                if (!is_double)
                {
                    ability.AddComponent(Common.createAbilityCasterHasNoFacts(allow_two_weapons_buff));
                }
                else
                {
                    ability.AddComponent(Common.createAbilityCasterHasFacts(allow_two_weapons_buff));
                    
                }
                ability.setMiscAbilityParametersSelfOnly();

                if (is_2h)
                {
                    abilities_2h.Add(ability);
                }
                else if (is_light)
                {
                    abilities_light.Add(ability);
                }
                else if (is_normal)
                {
                    abilities_1h.Add(ability);
                }
                else if (is_double)
                {
                    abilities_double.Add(ability);
                }

                
                if (!requires_2h)
                {
                    var buff2 = Helpers.CreateBuff(wc.ToString() + "ManifestWeaponBothHandsBuff",
                                                  "",
                                                  "",
                                                  "$" + Helpers.MergeIds(weapon.AssetGuid, "e8082c7d2f50417aa8710002c094e043"),
                                                  null,
                                                  null,
                                                  Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => { c.weapon = weapon; }),
                                                  Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => { c.weapon = weapon; c.create_in_offhand = true; }),
                                                  Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                                                                              false, false, enchants),
                                                  Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeaponOffHand(ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                                                                              false, false, enchants),
                                                  Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => { c.Resource = resource; c.amount = 2*resource_amount; }))
                                                  );
                    buff2.SetBuffFlags(BuffFlags.RemoveOnRest);
                    var apply_buff2 = Common.createContextActionApplyBuff(buff2, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
                    var ability2 = Helpers.CreateAbility(wc.ToString() + "ManifestWeaponBothHandsAbility",
                                                        "Manifest " + LocalizedTexts.Instance.Stats.GetText(wc) + " (Both Hands)",
                                                        description,
                                                        "$" + Helpers.MergeIds(weapon.AssetGuid, "f466ab98270c4e2d9ba131cbbf7aace3"),
                                                        weapon.Icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Standard,
                                                        AbilityRange.Personal,
                                                        "",
                                                        "",
                                                        Helpers.CreateRunActions(apply_buff2),
                                                        resource.CreateResourceLogic(amount: resource_amount*2),
                                                        Common.createAbilityCasterHasFacts(allow_two_weapons_buff),
                                                        Helpers.Create<NewMechanics.AbilityCasterPrimaryHandFree>(a => a.for_2h_item = false),
                                                        Helpers.Create<NewMechanics.AbilityCasterSecondaryHandFree>(),
                                                        Helpers.Create<NewMechanics.AbilityShowIfCasterProficientWithWeaponCategory>(a => { a.category = wc; a.require_full_proficiency = true;})
                                                        );
                    abilities.Add(ability2);
                    //Common.setAsFullRoundAction(ability2);
                    ability2.setMiscAbilityParametersSelfOnly();
                    mind_blade_2weapon_buffs.Add(buff2);

                    if (is_light)
                    {
                        abilities_light2.Add(ability2);
                    }
                    else if (is_normal)
                    {
                        abilities_1h2.Add(ability2);
                    }
                }
            }


            ability_light = Common.createVariantWrapper("MindBladeManifestLightWeaponBase", "", abilities_light.ToArray());
            ability_light.SetName("Manifest Light Weapon");
            ability_1h = Common.createVariantWrapper("MindBladeManifest1hWeaponBase", "", abilities_1h.ToArray());
            ability_1h.SetName("Manifest One-Handed Weapon");
            ability_2h = Common.createVariantWrapper("MindBladeManifest2hWeaponBase", "", abilities_2h.ToArray());
            ability_2h.SetName("Manifest Two-Handed Weapon");
            ability_light2 = Common.createVariantWrapper("MindBladeManifestLightWeapon2Base", "", abilities_light2.ToArray());
            ability_light2.SetName("Manifest Two Light Weapons");
            ability_1h2 = Common.createVariantWrapper("MindBladeManifest1hWeapon2Base", "", abilities_1h2.ToArray());
            ability_1h2.SetName("Manifest Two One-Handed Weapons");
            ability_double = Common.createVariantWrapper("MindBladeManifestDoubleWeaponBase", "", abilities_double.ToArray());
            ability_double.SetName("Manifest Double Weapon");


            var remove_buffs_actions = new List<GameAction>();
            foreach (var b in mind_blade_weapon_buffs)
            {
                remove_buffs_actions.Add(Common.createContextActionRemoveBuffFromCaster(b));
            }
            foreach (var b in mind_blade_2weapon_buffs)
            {
                remove_buffs_actions.Add(Common.createContextActionRemoveBuffFromCaster(b));
            }

            foreach (var a in abilities)
            {
                a.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(remove_buffs_actions.ToArray())));
            }

            var deactivate = Helpers.CreateAbility("ManifestWeaponDeactivateAbility",
                                                   "Deactivate Manifested Weapon(s)",
                                                   "Dismiss manifested weapon(s) and recover spent psychic pool points.",
                                                   "",
                                                   Helpers.GetIcon("1d6364123e1f6a04c88313d83d3b70ee"),
                                                   AbilityType.Supernatural,
                                                   CommandType.Free,
                                                   AbilityRange.Personal,
                                                   "",
                                                   "",
                                                   Helpers.CreateRunActions(remove_buffs_actions.ToArray())
                                                   );
            deactivate.setMiscAbilityParametersSelfOnly();



            psychic_pool = Helpers.CreateFeature("PsychicPoolFeature",
                                                 "Psychic Pool",
                                                 description,
                                                 "",
                                                 null,
                                                 FeatureGroup.None,
                                                 resource.CreateAddAbilityResource(),
                                                 Helpers.CreateAddFacts(ability_light, ability_1h, ability_2h, deactivate)
                                                 );
                                                 
            dual_weapons = Helpers.CreateFeature("DualWeaponsFeature",
                                                 "Dual Weapons",
                                                 "At 7th level, a mindblade can maintain two psychic weapons at a time or a psychic double weapon, though each weapon (or each end of a double weapon) has an enhancement bonus 1 lower than normal. Starting at 12th level, each of the two psychic weapons (or each end of a double weapon) instead has an enhancement bonus 2 lower than normal. When using two-weapon fighting with two psychic weapons or a psychic double weapon, the mindblade can use her spell combat ability as though she had a hand free.",
                                                 "",
                                                 Helpers.GetIcon("464a7193519429f48b4d190acb753cf0"), //grace
                                                 FeatureGroup.None,
                                                 Helpers.CreateAddFacts(ability_light2, ability_1h2, ability_double),
                                                 Helpers.Create<HoldingItemsMechanics.UseSpellCombatWith2ManifestedWeapons>(u => u.required_enchant = psychic_weapon_enchant),
                                                 Helpers.CreateAddFeatureOnClassLevel(Common.ActivatableAbilityToFeature(toggle_2_weapons), 12, new BlueprintCharacterClass[] {archetype.GetParentClass()}, before: true),
                                                 Helpers.CreateAddFeatureOnClassLevel(Common.ActivatableAbilityToFeature(toggle_2_weapons2), 12, new BlueprintCharacterClass[] {archetype.GetParentClass() })
                                                 );
            psychic_pool_resource = resource;
            manifestation_abilities = abilities;
        }


        static void createRapidManifest()
        {
            rapid_manifest = Helpers.CreateFeature("MindBladeRapidManifestFeature",
                                                   "Rapid Manifest",
                                                   "At 8th level, a mindblade can manifest one psychic weapon as a swift action.",
                                                   "",
                                                   Helpers.GetIcon("f9e6281bffd7030499e2ab469e15f1a7"),
                                                   FeatureGroup.None,
                                                   Helpers.Create<TurnActionMechanics.UseAbilitiesAsSwiftAction>(a => a.abilities = new BlueprintAbility[] { ability_1h, ability_2h, ability_light })
                                                   );
        }


        static void createDualManifest()
        {
            dual_manifest = Helpers.CreateFeature("MindBladeDualManifestFeature",
                                                   "Dual Manifest",
                                                   "At 13th level, a mindblade can manifest two psychic weapons or one double weapon with a swift action. Also, when wielding a psychic weapon two-handed, she can use her spell combat ability as though she had a hand free.",
                                                   "",
                                                   Helpers.GetIcon("27d76f1afda08a64d897cc81201b5218"),
                                                   FeatureGroup.None,
                                                   Helpers.Create<HoldingItemsMechanics.UseSpellCombatWith2hManifestedWeapon>(u => u.required_enchant = psychic_weapon_enchant),
                                                   Helpers.Create<TurnActionMechanics.UseAbilitiesAsSwiftAction>(a => a.abilities = new BlueprintAbility[] { ability_1h2, ability_double, ability_light2 })
                                                   );
        }
    }
}
