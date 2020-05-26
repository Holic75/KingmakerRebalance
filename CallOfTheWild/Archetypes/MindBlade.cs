using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
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
        static public BlueprintAbilityResource psychic_pool_resource;

        static public BlueprintFeature[] psychic_pool_update = new BlueprintFeature[6];
        static List<BlueprintBuff> mind_blade_weapon_buffs = new List<BlueprintBuff>();


        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var magus_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MindBladeMagusArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Mind Blade");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A mindblade blends psychic talent and martial skill to lethal effect. By forming weapons with her mind, she always has the right tool for any situation.");
            });
            Helpers.SetField(archetype, "m_ParentClass", magus_class);
            library.AddAsset(archetype, "");

            //createPsychicSpellcasting();
            //createPsychicAccess();
            createPsychicPoolAndDualWeapons();
            createPsychicPoolEnchants();
            //createRapidManifest();
            //createDualManifest();

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
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, psychic_spellcasting, psychic_pool),
                                                          Helpers.LevelEntry(3, psychic_pool_update[0]),
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
                "7a73bf165e8eda6478b4419f857d1ab5", //enduring blade
            };

            foreach (var id in restricted_arcanas_ids)
            {
                var feature = library.Get<BlueprintFeature>(id);
                feature.AddComponent(Common.prerequisiteNoArchetype(magus_class, archetype));
            }
        }


        static void createPsychicPoolEnchants()
        {
            var brilliant_energy = Common.createEnchantmentAbility("MindBladeWeaponEnchancementBrilliantEnergy",
                                                            "Psychic Weapon - Brilliant Energy",
                                                            "A mindblade can add the brilliant energy property to her psychic weapon, but this consumes 4 points of enhancement bonus granted to this weapon.\nA brilliant energy weapon ignores nonliving matter.Armor and shield bonuses to AC (including any enhancement bonuses to that armor) do not count against it because the weapon passes through armor. (Dexterity, deflection, dodge, natural armor, and other such bonuses still apply.) A brilliant energy weapon cannot harm undead, constructs, or objects.",
                                                            library.Get<BlueprintActivatableAbility>("f1eec5cc68099384cbfc6964049b24fa").Icon,
                                                            mind_blade_weapon_buffs.ToArray(),
                                                            library.Get<BlueprintWeaponEnchantment>("66e9e299c9002ea4bb65b6f300e43770"),
                                                            4, ActivatableAbilityGroup.ArcaneWeaponProperty);
            
            var flaming = Common.createEnchantmentAbility("MindBladeWeaponEnchancementFlaming",
                                                                "Psychic Weapon - Flaming",
                                                                "A mindblade can add the flaming property to her psychic weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                                library.Get<BlueprintActivatableAbility>("7902941ef70a0dc44bcfc174d6193386").Icon,
                                                                mind_blade_weapon_buffs.ToArray(),
                                                                library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                                1, ActivatableAbilityGroup.ArcaneWeaponProperty);
            var speed = Common.createEnchantmentAbility("MindBladeWeaponEnchancementSpeed",
                                                        "Psychic Weapon - Speed",
                                                        "A mindblade can add the speed property to her psychic weapon, but this consumes 3 points of enhancement bonus granted to this weapon.\nWhen making a full attack, the wielder of a speed weapon may make one extra attack with it. The attack uses the wielder's full base attack bonus, plus any modifiers appropriate to the situation. (This benefit is not cumulative with similar effects, such as a haste spell.)",
                                                        library.Get<BlueprintActivatableAbility>("85742dd6788c6914f96ddc4628b23932").Icon,
                                                        mind_blade_weapon_buffs.ToArray(),
                                                        library.Get<BlueprintWeaponEnchantment>("f1c0c50108025d546b2554674ea1c006"),
                                                        3, ActivatableAbilityGroup.ArcaneWeaponProperty);

            var frost = Common.createEnchantmentAbility("MindBladeWeaponEnchancementFrost",
                                                            "Psychic Weapon - Frost",
                                                            "A mindblade can add the frost property to her psychic weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA frost weapon is sheathed in a terrible, icy cold that deals an extra 1d6 points of cold damage on a successful hit. The cold does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("b338e43a8f81a2f43a73a4ae676353a5").Icon,
                                                            mind_blade_weapon_buffs.ToArray(),
                                                            library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b"),
                                                            1, ActivatableAbilityGroup.ArcaneWeaponProperty);

            var shock = Common.createEnchantmentAbility("MindBladeWeaponEnchancementShock",
                                                            "Psychic Weapon - Shock",
                                                            "A mindblade can add the shock property to her psychic weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA shock weapon is sheathed in crackling electricity that deals an extra 1d6 points of electricity damage on a successful hit. The electricity does not harm the wielder.",
                                                            library.Get<BlueprintActivatableAbility>("a3a9e9a2f909cd74e9aee7788a7ec0c6").Icon,
                                                            mind_blade_weapon_buffs.ToArray(),
                                                            library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658"),
                                                            1, ActivatableAbilityGroup.ArcaneWeaponProperty);

            var ghost_touch = Common.createEnchantmentAbility("MindBladeWeaponEnchancementGhostTouch",
                                                                    "Psychic Weapon - Ghost Touch",
                                                                    "A mindblade can add the ghost touch property to her psychic weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA ghost touch weapon deals damage normally against incorporeal creatures, regardless of its bonus. An incorporeal creature's 50% reduction in damage from corporeal sources does not apply to attacks made against it with ghost touch weapons.",
                                                                    library.Get<BlueprintActivatableAbility>("688d42200cbb2334c8e27191c123d18f").Icon,
                                                                    mind_blade_weapon_buffs.ToArray(),
                                                                    library.Get<BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f"),
                                                                    1, ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                                    AlignmentMaskType.TrueNeutral);

            var keen = Common.createEnchantmentAbility("MindBladeWeaponEnchancementKeen",
                                                            "Psychic Weapon - Keen",
                                                            "A mindblade can add the keen property to her psychic weapon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                            library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                            mind_blade_weapon_buffs.ToArray(),
                                                            library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                            1, ActivatableAbilityGroup.ArcaneWeaponProperty);

            var holy = Common.createEnchantmentAbility("MindBladeWeaponEnchancementHoly",
                                                            "Psychic Weapon - Holy",
                                                            "A mindblade can add the holy property to her psychic weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nA holy weapon is imbued with holy power. This power makes the weapon good-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of evil alignment.",
                                                            library.Get<BlueprintActivatableAbility>("ce0ece459ebed9941bb096f559f36fa8").Icon,
                                                            mind_blade_weapon_buffs.ToArray(),
                                                            library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140"),
                                                            2, ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                            AlignmentMaskType.Good);

            var unholy = Common.createEnchantmentAbility("MindBladeWeaponEnchancementUnholy",
                                                            "Psychic Weapon - Unholy",
                                                            "A mindblade can add the unholy property to her psychic weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn unholy weapon is imbued with unholy power. This power makes the weapon evil-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of good alignment.",
                                                            library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce").Icon,
                                                            mind_blade_weapon_buffs.ToArray(),
                                                            library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"),
                                                            2, ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                            AlignmentMaskType.Evil);

            var axiomatic = Common.createEnchantmentAbility("MindBladeWeaponEnchancementAxiomatic",
                                                            "Psychic Weapon - Axiomatic",
                                                            "A mindblade can add the axiomatic property to her psychic weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn axiomatic weapon is infused with lawful power. It makes the weapon lawful-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against chaotic creatures.",
                                                            library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,
                                                            mind_blade_weapon_buffs.ToArray(),
                                                            library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4"),
                                                            2, ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                            AlignmentMaskType.Lawful);

            var anarchic = Common.createEnchantmentAbility("MindBladeWeaponEnchancementAnarchic",
                                                "Psychic Weapon - Anarchic",
                                                "A mindblade can add the anarchic property to her psychic weapon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn anarchic weapon is infused with the power of chaos. It makes the weapon chaotic-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of lawful alignment.",
                                                library.Get<BlueprintActivatableAbility>("8ed07b0cc56223c46953348f849f3309").Icon,
                                                mind_blade_weapon_buffs.ToArray(),
                                                library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"),
                                                2, ActivatableAbilityGroup.ArcaneWeaponProperty,
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
            psychic_pool_update[0].AddComponent(Helpers.CreateAddFacts(flaming, frost, shock, keen));
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
            var arcana_selection = library.Get<BlueprintFeatureSelection>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            arcana_selection.AllFeatures = arcana_selection.AllFeatures.AddToArray(devoted_blade, ghost_blade);
        }


        static void createPsychicPoolAndDualWeapons()
        {
            var air_enchatment = library.Get<BlueprintWeaponEnchantment>("1d64abd0002b98043b199c0e3109d3ee");
            var psychic_weapon_enchant = Common.createWeaponEnchantment("ManifestedWeaponEnchant",
                                                          "Manifested",
                                                          "This is a weapon manifested by the power of your will.",
                                                          "",
                                                          "",
                                                          "",
                                                          0,
                                                          air_enchatment.WeaponFxPrefab
                                                          );

            string description = "A mindblade gains a psychic pool, similar to a normal magus’s arcane pool. At 1st level, a mindblade can expend 1 point from her psychic pool as a standard action to manifest a light melee weapon of her choice, formed from psychic energy. By spending 2 points, the mindblade can manifest a one-handed melee weapon, and by spending 3 points, she can manifest a two-handed melee weapon (but not a double weapon). This psychic weapon can last indefinitely, but it vanishes if it leaves the mindblade’s hand. The mindblade can dismiss a held psychic weapon as a free action. When a psychic weapon vanishes, the mindblade regains the psychic energy used to create it. She can maintain only one weapon at a time.\n"
                                 + "At 1st level, a psychic weapon counts as a magic weapon of whatever type the mindblade selected, with a +1 enhancement bonus. At 3rd level and every 3 levels thereafter, the weapon’s enhancement bonus increases by 1, up to maximum of +5 at 12th level. Starting at 5th level, the mindblade can add any of the weapon special abilities listed in the arcane pool class feature in place of these bonuses, although the weapon must maintain at least a +1 bonus to benefit from any weapon special abilities. At 15th and 18th levels, the weapon gains an additional +1 enhancement bonus, which the mindblade can spend only on weapon special abilities.";
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
            allow_two_weapons_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
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
                if (weapon == null || !weapon.IsMelee)
                {
                    continue;
                }
                weapon = library.CopyAndAdd(weapon, "Psychic" + weapon.name, "");
                Common.addEnchantment(weapon, WeaponEnchantments.summoned_weapon_enchant);
                Common.addEnchantment(weapon, psychic_weapon_enchant);
                if (weapon.Double)
                {
                    weapon.SecondWeapon = library.CopyAndAdd(weapon, "Psychic" + weapon.name + "Second", "");
                    Common.addEnchantment(weapon.SecondWeapon, WeaponEnchantments.summoned_weapon_enchant);
                    Common.addEnchantment(weapon.SecondWeapon, psychic_weapon_enchant);
                    weapon.SecondWeapon.SecondWeapon = weapon.SecondWeapon;
                }

                bool is_light = weapon.IsLight;
                bool is_2h = weapon.IsTwoHanded;
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
                                              "",
                                              null,
                                              null,
                                              Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => c.weapon = weapon),
                                              Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                                                                              false, true, enchants),
                                              Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => { c.Resource = resource; c.amount = resource_amount; }))
                                              );
                buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
                if (is_double)
                {
                    buff.AddComponent(Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeaponOffHand(ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                                                                          false, true, enchants));
                }
                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
                var ability = Helpers.CreateAbility(wc.ToString() + "ManifestWeaponAbility",
                                                    "Manifest " + LocalizedTexts.Instance.Stats.GetText(wc),
                                                    description,
                                                    "",
                                                    weapon.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(apply_buff),
                                                    resource.CreateResourceLogic(amount: resource_amount),
                                                    Helpers.Create<NewMechanics.AbilityCasterPrimaryHandFree>(a => a.for_2h_item = requires_2h)
                                                    );
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

                mind_blade_weapon_buffs.Add(buff);
                if (!requires_2h)
                {
                    var buff2 = Helpers.CreateBuff(wc.ToString() + "ManifestWeaponBothHandsBuff",
                                                  "",
                                                  "",
                                                  "",
                                                  null,
                                                  null,
                                                  Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => { c.weapon = weapon; }),
                                                  Helpers.Create<NewMechanics.EnchantmentMechanics.CreateWeapon>(c => { c.weapon = weapon; c.create_in_offhand = true; }),
                                                  Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                                                                              false, true, enchants),
                                                  Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeaponOffHand(ActivatableAbilityGroup.ArcaneWeaponProperty,
                                                                                                              false, true, enchants),
                                                  Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => { c.Resource = resource; c.amount = 2*resource_amount; }))
                                                  );

                    var apply_buff2 = Common.createContextActionApplyBuff(buff2, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
                    var ability2 = Helpers.CreateAbility(wc.ToString() + "ManifestWeaponBothHandsAbility",
                                                        "Manifest " + LocalizedTexts.Instance.Stats.GetText(wc) + " (Both Hands)",
                                                        description,
                                                        "",
                                                        weapon.Icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Standard,
                                                        AbilityRange.Personal,
                                                        "",
                                                        "",
                                                        Helpers.CreateRunActions(apply_buff2),
                                                        resource.CreateResourceLogic(amount: resource_amount*2),
                                                        Common.createAbilityCasterHasFacts(allow_two_weapons_buff)
                                                        );
                    //Common.setAsFullRoundAction(ability2);
                    ability2.setMiscAbilityParametersSelfOnly();
                    mind_blade_weapon_buffs.Add(buff2);

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
            ability_light2 = Common.createVariantWrapper("MindBladeManifestLightWeaponBase", "", abilities_light.ToArray());
            ability_light.SetName("Manifest Two Light Weapons");
            ability_1h2 = Common.createVariantWrapper("MindBladeManifest1hWeaponBase", "", abilities_1h.ToArray());
            ability_1h2.SetName("Manifest Two One-Handed Weapons");
            ability_double = Common.createVariantWrapper("MindBladeManifestDoubleWeaponBase", "", abilities_2h.ToArray());
            ability_double.SetName("Manifest Double Weapon");


            psychic_pool = Helpers.CreateFeature("PsychicPoolFeature",
                                                 "Psychic Pool",
                                                 description,
                                                 "",
                                                 null,
                                                 FeatureGroup.None,
                                                 resource.CreateAddAbilityResource(),
                                                 Helpers.CreateAddFacts(ability_light, ability_1h, ability_2h)
                                                 );
                                                 
            dual_weapons = Helpers.CreateFeature("DualWeaponsFeature",
                                                 "Dual Weapons",
                                                 "At 7th level, a mindblade can maintain two psychic weapons at a time or a psychic double weapon, though each weapon (or each end of a double weapon) has an enhancement bonus 1 lower than normal. Starting at 12th level, each of the two psychic weapons (or each end of a double weapon) instead has an enhancement bonus 2 lower than normal. When using two-weapon fighting with two psychic weapons or a psychic double weapon, the mindblade can use her spell combat ability as though she had a hand free.",
                                                 "",
                                                 null,
                                                 FeatureGroup.None,
                                                 Helpers.CreateAddFacts(ability_light2, ability_1h2, ability_double),
                                                 Helpers.CreateAddFeatureOnClassLevel(Common.ActivatableAbilityToFeature(toggle_2_weapons), 12, new BlueprintCharacterClass[] {archetype.GetParentClass()}, before: true),
                                                 Helpers.CreateAddFeatureOnClassLevel(Common.ActivatableAbilityToFeature(toggle_2_weapons2), 12, new BlueprintCharacterClass[] {archetype.GetParentClass() })
                                                 );
            psychic_pool_resource = resource;
        }
    }
}
