﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public static class WeaponEnchantments
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintWeaponEnchantment empower_enchant;
        static public BlueprintWeaponEnchantment maximize_enchant;
        static public Dictionary<DamageEnergyType, BlueprintWeaponEnchantment> elemental_enchants = new Dictionary<DamageEnergyType, BlueprintWeaponEnchantment>();
        static public BlueprintWeaponEnchantment summoned_weapon_enchant;
        static public BlueprintWeaponEnchantment[] temporary_enchants = new BlueprintWeaponEnchantment[] {library.Get<BlueprintWeaponEnchantment>("d704f90f54f813043a525f304f6c0050"),
                                                                                                         library.Get<BlueprintWeaponEnchantment>("9e9bab3020ec5f64499e007880b37e52"),
                                                                                                         library.Get<BlueprintWeaponEnchantment>("d072b841ba0668846adeb007f623bd6c"),
                                                                                                         library.Get<BlueprintWeaponEnchantment>("6a6a0901d799ceb49b33d4851ff72132"),
                                                                                                         library.Get<BlueprintWeaponEnchantment>("746ee366e50611146821d61e391edf16") };

        static public BlueprintEquipmentEnchantment[] unarmed_enchants = new BlueprintEquipmentEnchantment[] {library.Get<BlueprintEquipmentEnchantment>("da7d830b3f75749458c2e51524805560"),
                                                                                                             library.Get<BlueprintEquipmentEnchantment>("49f9befa0e77cd5428ca3b28fd66a54e"),
                                                                                                             library.Get<BlueprintEquipmentEnchantment>("bae627dfb77c2b048900f154719ca07b"),
                                                                                                             library.Get<BlueprintEquipmentEnchantment>("a4016a5d78384a94581497d0d135d98b"),
                                                                                                             library.Get<BlueprintEquipmentEnchantment>("c3ad7f708c573b24082dde91b081ca5f") };

        static public BlueprintWeaponEnchantment[] standard_enchants = new BlueprintWeaponEnchantment[] {library.Get<BlueprintWeaponEnchantment>("d42fc23b92c640846ac137dc26e000d4"),
                                                                                                             library.Get<BlueprintWeaponEnchantment>("eb2faccc4c9487d43b3575d7e77ff3f5"),
                                                                                                             library.Get<BlueprintWeaponEnchantment>("80bb8a737579e35498177e1e3c75899b"),
                                                                                                             library.Get<BlueprintWeaponEnchantment>("783d7d496da6ac44f9511011fc5f1979"),
                                                                                                             library.Get<BlueprintWeaponEnchantment>("bdba267e951851449af552aa9f9e3992") };
        static public BlueprintWeaponEnchantment master_work = library.Get<BlueprintWeaponEnchantment>("6b38844e2bffbac48b63036b66e735be");
        static public BlueprintWeaponEnchantment[] static_enchants;

        static public BlueprintEquipmentEnchantment[] unarmed_bonus = new BlueprintEquipmentEnchantment[5];

        static public BlueprintWeaponEnchantment cruel;
        static public BlueprintWeaponEnchantment heartseeker;
        static public BlueprintWeaponEnchantment menacing;
        static public BlueprintWeaponEnchantment vorpal;

        static public BlueprintWeaponEnchantment thundering = library.Get<BlueprintWeaponEnchantment>("690e762f7704e1f4aa1ac69ef0ce6a96");
        static public BlueprintWeaponEnchantment vicious = library.Get<BlueprintWeaponEnchantment>("a1455a289da208144981e4b1ef92cc56");
        static public BlueprintWeaponEnchantment bane = library.Get<BlueprintWeaponEnchantment>("1a93ab9c46e48f3488178733be29342a");

        static public BlueprintWeaponEnchantment cold_iron = library.Get<BlueprintWeaponEnchantment>("e5990dc76d2a613409916071c898eee8");
        static public BlueprintWeaponEnchantment mithral = library.Get<BlueprintWeaponEnchantment>("0ae8fc9f2e255584faf4d14835224875");
        static public BlueprintWeaponEnchantment adamantine = library.Get<BlueprintWeaponEnchantment>("ab39e7d59dd12f4429ffef5dca88dc7b");

        static public BlueprintWeaponEnchantment dazzling_blade_fx_enchant;

        static public BlueprintWeaponEnchantment spell_storing;
        static public BlueprintFeature spell_storing_feature;

        static public BlueprintWeaponEnchantment disarm_enchant;
        static public BlueprintWeaponEnchantment sunder_enchant;
        static public BlueprintWeaponEnchantment trip_enchant;

        static public void initialize()
        {
            createManeuverEnchantments();
            createMetamagicEnchantments();
            createSummonedWeaponEnchantment();
            createStaticEnchants();
            //fix weapon enchants to be non cumulative
            fixEnchants();

            createCruelEnchant();
            createHeartSeekerEnchant();
            createMenacingEnchant();
            createVorpalEnchant();

            var brilliant_energy = library.Get<BlueprintWeaponEnchantment>("6cbb732b9d638724a960d784634dcdcf"); //plasma
            dazzling_blade_fx_enchant = Common.createWeaponEnchantment("DazzlingWeaponEnchant", "", "", "", "", "", 0, brilliant_energy.WeaponFxPrefab);

            addDamageOverrideToMaterialEnchants();

            createSpellStoring();
        }


        static void createManeuverEnchantments()
        {
            var sunder = Helpers.CreateFeature("SunderWeaponPropertyFeature",
                                   "Sunder",
                                   "When you use a sunder weapon, you get a +2 bonus on Combat Maneuver Checks to sunder attempts.",
                                   "",
                                   null,
                                   FeatureGroup.None,
                                   Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.SunderArmor, 2)
                                   );
            sunder.HideInCharacterSheetAndLevelUp = true;
            sunder_enchant = Common.createWeaponEnchantment("SunderEnchantment", sunder.Name, sunder.Description, "", "", "", 0, null,
                                           Helpers.Create<AddUnitFeatureEquipment>(a => a.Feature = sunder));

            var trip = Helpers.CreateFeature("TripWeaponPropertyFeature",
                                   "Trip",
                                   "When you use a trip weapon, you get a +2 bonus on Combat Maneuver Checks to trip attempts.",
                                   "",
                                   null,
                                   FeatureGroup.None,
                                   Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Trip, 2)
                                   );
            trip.HideInCharacterSheetAndLevelUp = true;
            trip_enchant = Common.createWeaponEnchantment("TripEnchantment", trip.Name, trip.Description, "", "", "", 0, null,
                               Helpers.Create<AddUnitFeatureEquipment>(a => a.Feature = trip));

            var disarm = Helpers.CreateFeature("DisarmWeaponPropertyFeature",
                                               "Disarm",
                                               "When you use a disarm weapon, you get a +2 bonus on Combat Maneuver Checks to disarm attempts.",
                                               "",
                                               null,
                                               FeatureGroup.None,
                                               Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Disarm, 2)
                                               );
            disarm.HideInCharacterSheetAndLevelUp = true;

            disarm_enchant = Common.createWeaponEnchantment("DisarmEnchantment", disarm.Name, disarm.Description, "", "", "", 0, null,
                   Helpers.Create<AddUnitFeatureEquipment>(a => a.Feature = disarm));
        }


        static void createSpellStoring()
        {
            var store_feature = Helpers.CreateFeature("CharacterWeaponSpellStroingFeature",
                                                      "",
                                                      "",
                                                      "",
                                                      null,
                                                      Kingmaker.Blueprints.Classes.FeatureGroup.None,
                                                      Helpers.Create<SpellManipulationMechanics.FactStoreSpell>(f => f.link_type = SpellManipulationMechanics.FactStoreSpell.LinkType.Weapon)
                                                      );
            store_feature.HideInCharacterSheetAndLevelUp = true;
            store_feature.HideInUI = true;

            var basic_feat_progression = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("5b72dd2ca2cb73b49903806ee8986325");
            basic_feat_progression.LevelEntries[0].Features.Add(store_feature);

            Action<UnitDescriptor> save_game_action = delegate (UnitDescriptor u)
            {
                if (!u.HasFact(store_feature))
                {
                    u.AddFact(store_feature);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_action);
            spell_storing_feature = store_feature;
            var icon = Helpers.GetIcon("a26c23a887a6f154491dc2cefdad2c35"); //crusader's edge
            var feature = Helpers.CreateFeature("WeaponSpellStoringFeature",
                                                "Spell Storing Weapon",
                                                "A spell storing weapon allows a spellcaster to store a single targeted spell of up to 3rd level in the weapon. (The spell must have a casting time of 1 standard action.) Anytime the weapon strikes a creature and the creature takes damage from it, the weapon can immediately cast the spell on that creature as a free action if the wielder desires. (This special ability is an exception to the general rule that casting a spell from an item takes at least as long as casting that spell normally.) Once the spell has been cast from the weapon, a spellcaster can cast any other targeted spell of up to 3rd level into it.",
                                                "",
                                                icon,
                                                Kingmaker.Blueprints.Classes.FeatureGroup.None);

            var release_buff = Helpers.CreateBuff("WeaponSpellStoringToggleBuff",
                                                  feature.Name + ": Release",
                                                  feature.Description,
                                                  "",
                                                  feature.Icon,
                                                  null,
                                                  Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = store_feature));

            var major_activatable_ability = Helpers.CreateActivatableAbility("WeaponSpellStoringToggleAbility",
                                                                             feature.Name + ": Release",
                                                                             feature.Description,
                                                                             "",
                                                                             feature.Icon,
                                                                             release_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.Create<SpellManipulationMechanics.ActivatableAbilitySpellStoredInFactRestriction>(a => a.fact = store_feature));
            major_activatable_ability.DeactivateImmediately = true;

            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = store_feature);
            var release_on_condition = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(release_buff), release_action);
            feature.AddComponent(Common.createAddInitiatorAttackWithWeaponTrigger(action: Helpers.CreateActionList(release_on_condition), wait_for_attack_to_resolve: true));
            feature.AddComponent(Helpers.CreateAddFact(major_activatable_ability));

            int max_variants = 10; //due to ui limitation
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return spell.SpellLevel <= 3
                        && spell.Blueprint.EffectOnEnemy == AbilityEffectOnUnit.Harmful
                        && spell.Blueprint.Range != AbilityRange.Personal
                        && spell.Blueprint.CanTargetEnemies
                        && !spell.Blueprint.CanTargetPoint
                        && !spell.Blueprint.IsFullRoundAction
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < max_variants)
                        && !spell.Blueprint.HasAreaEffect()
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent);
            };

            for (int i = 0; i < max_variants; i++)
            {
                var ability = Helpers.CreateAbility($"WeaponSpellStoring{i + 1}Ability",
                                                          feature.Name,
                                                          feature.Description,
                                                          "",
                                                          feature.Icon,
                                                          AbilityType.Supernatural,
                                                          CommandType.Standard,
                                                          AbilityRange.Personal,
                                                          "",
                                                          "",
                                                          Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s => { s.fact = store_feature; s.check_slot_predicate = check_slot_predicate; s.variant = i; })
                                                          );
                ability.setMiscAbilityParametersSelfOnly();
                feature.AddComponent(Helpers.CreateAddFact(ability));
            }

            spell_storing = Common.createWeaponEnchantment("SpellStoringWeaponEnchantment",
                                                            "Spell Storing",
                                                            feature.Description,
                                                            "",
                                                            "",
                                                            "",
                                                            0,
                                                            1,
                                                            null,
                                                            Helpers.Create<AddUnitFeatureEquipment>(a => a.Feature = feature)
                                                            );
        }


        static void addDamageOverrideToMaterialEnchants()
        {
            cold_iron.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.AddWeaponDamageMaterial>(a => a.material = PhysicalDamageMaterial.ColdIron));
            mithral.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.AddWeaponDamageMaterial>(a => a.material = PhysicalDamageMaterial.Silver));
            adamantine.AddComponent(Helpers.Create<NewMechanics.EnchantmentMechanics.AddWeaponDamageMaterial>(a => a.material = PhysicalDamageMaterial.Adamantite));
        }


        static void createVorpalEnchant()
        {
            var action = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, Common.undead, Common.plant, Common.aberration, Common.construct, Common.elemental),
                                                   null,
                                                   Helpers.Create<ContextActionKillTarget>()
                                                   );
            
            vorpal = Common.createWeaponEnchantment("VorpalWeaponEnchantment",
                                          "Vorpal",
                                          "This potent and feared ability allows the weapon to sever the heads of those it strikes.\n"
                                          + "Upon a roll of natural 20 (followed by a successful roll to confirm the critical hit), the weapon severs the opponent’s head (if it has one) from its body. Some creatures, such as many aberrations and all oozes, have no heads. Others, such as golems and undead creatures other than vampires, are not affected by the loss of their heads. Most other creatures, however, die when their heads are cut off.",
                                           "",
                                           "",
                                           "",
                                           5,
                                           null,
                                           Common.createAddInitiatorAttackRollTrigger2(Helpers.CreateActionList(action), critical_hit: true, only_natural20: true)
                                          );
        }

        static void createMenacingEnchant()
        {
            var icon = Helpers.GetIcon("9b9eac6709e1c084cb18c3a366e0ec87");
            var menacing_buff = Helpers.CreateBuff("MenacingEncahntBuff",
                                                   "Menacing",
                                                   "",
                                                   "",
                                                   icon,
                                                   null,
                                                   Helpers.Create<NewMechanics.EnchantmentMechanics.Menaced>());
            menacing_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var aura = Common.createAuraEffectFeature("Menacing",
                                                      "A menacing weapon helps allies deal with flanked foes. When the wielder is adjacent to a creature that is being flanked by an ally, the flanking bonus on attack rolls for all flanking allies increases by +2. This ability works even if the wielder is not one of the characters flanking the creature.",
                                                      icon,
                                                      menacing_buff,
                                                      7.Feet(),
                                                      Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>())
                                                      );


                 

            menacing = Common.createWeaponEnchantment("MencaingWeaponEnchantment",
                                                      aura.Name,
                                                      aura.Description,
                                                       "",
                                                       "",
                                                       "",
                                                       5,
                                                       null,
                                                       Helpers.Create<AddUnitFeatureEquipment>(a => a.Feature = aura)
                                                      );
        }


        static void createHeartSeekerEnchant()
        {
            heartseeker = Common.createWeaponEnchantment("HeartSeekerWeaponEnchantment",
                                                       "Heartseeker",
                                                       "A heartseeker weapon is drawn unerringly toward beating hearts. A heartseeker weapon ignores the miss chance for concealment against most living targets, though the attack must still target the proper square. This special ability does not apply against aberrations, oozes, plants, outsiders with the elemental subtype, or any creature specifically noted to lack a heart.",
                                                       "",
                                                       "",
                                                       "",
                                                       5,
                                                       null,
                                                       Helpers.Create<NewMechanics.EnchantmentMechanics.HeartSeekerEnchantment>()
                                                      );
        }


        static void createCruelEnchant()
        {
            var description = "When the wielder strikes a creature that is frightened, shaken, or panicked with a cruel weapon, that creature becomes sickened for 1 round. When the wielder uses the weapon to knock unconscious or kill a creature, he gains 5 temporary hit points that last for 10 minutes.";
            var temp_hp_buff = Helpers.CreateBuff("CruelTemporaryHpBuff",
                                                  "Cruel Enchantmnet Temporary HP Bonus",
                                                  description,
                                                  "",
                                                  Helpers.GetIcon("e5cb4c4459e437e49a4cd73fde6b9063"), //inflict light wounds
                                                  null,
                                                  Helpers.Create<TemporaryHitPointsFromAbilityValue>(t => { t.Value = 5; t.RemoveWhenHitPointsEnd = true; })
                                                  );


            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var apply_tmp_hp = Common.createContextActionApplyBuff(temp_hp_buff, Helpers.CreateContextDuration(10, Kingmaker.UnitLogic.Mechanics.DurationRate.Minutes), dispellable: false);
            var apply_sickened = Helpers.CreateConditionalOr(new Condition[]{Helpers.Create<NewMechanics.ContextConditionHasCondition>(c => c.condition = UnitCondition.Frightened),
                                                                             Helpers.Create<NewMechanics.ContextConditionHasCondition>(c => c.condition = UnitCondition.Shaken)
                                                                            },
                                                             Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(1), dispellable: false)
                                                             );


            cruel = Common.createWeaponEnchantment("CruelWeaponEnchantment",
                                           "Cruel",
                                           description,
                                           "",
                                           "",
                                           "",
                                           5,
                                           null,
                                           Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_sickened), wait_for_attack_to_resolve: true),
                                           Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_tmp_hp), wait_for_attack_to_resolve: true,
                                                                                            on_initiator: true, reduce_hp_to_zero: true)
                                          );
        }


        static void createStaticEnchants()
        {
            static_enchants = new BlueprintWeaponEnchantment[5];

            for (int i = 0; i < static_enchants.Length; i++)
            {
                static_enchants[i] = Common.createWeaponEnchantment($"StaticTemporaryEnhancement{i + 1}",
                                                                    "Static " + temporary_enchants[i].Name,
                                                                    $"Attacks with this weapon get +{i + 1} enhancement bonus on both attack and damage rolls.",
                                                                    temporary_enchants[i].Prefix,
                                                                    temporary_enchants[i].Suffix,
                                                                    "",
                                                                    temporary_enchants[i].IdentifyDC,
                                                                    temporary_enchants[i].WeaponFxPrefab,
                                                                    Helpers.Create<NewMechanics.EnchantmentMechanics.StaticWeaponEnhancementBonus>(s => s.EnhancementBonus = i + 1)
                                                                    );
            }
        }


        static void fixEnchants()
        {
            for (int i = 0; i < unarmed_enchants.Length; i++)
            {
                unarmed_enchants[i].ComponentsArray[0] = Helpers.Create<NewMechanics.EnchantmentMechanics.StaticEquipmentWeaponTypeEnhancement>(s => { s.Enhancement = i + 1; s.AllNaturalAndUnarmed = true; });
                unarmed_bonus[i] = library.CopyAndAdd<BlueprintEquipmentEnchantment>(unarmed_enchants[i], $"UnarmedBonusEnhancement{i + 1}", "");
                unarmed_bonus[i].ComponentsArray = new BlueprintComponent[] { Helpers.Create<NewMechanics.EnchantmentMechanics.EquipmentWeaponTypeEnhancement>(s => { s.Enhancement = i + 1; s.AllNaturalAndUnarmed = true; }) };
            }

            /*for (int i = 0; i < standard_enchants.Length; i++)
            {
                standard_enchants[i].ComponentsArray[0] = Helpers.Create<NewMechanics.EnchantmentMechanics.StaticWeaponEnhancementBonus>(s => { s.EnhancementBonus = i + 1; });
            }*/

            //fix magic fang
            var magic_fang = library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33");
            var greater_magic_fang = library.Get<BlueprintAbility>("f1100650705a69c4384d3edd88ba0f52");

            (magic_fang.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as MagicFang).Enchantment[0] = static_enchants[0];
            (greater_magic_fang.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as MagicFang).Enchantment = static_enchants;


            //fix monk robes
            var robes = new BlueprintItemArmor[]{ library.Get<BlueprintItemArmor>("70c13c319f3d18841bb4c4e5c27125c9"), //protector
                                                  library.Get<BlueprintItemArmor>("d2eb319bc6b399a48b1b1d0a73238ac4"), //arbiter
                                                  library.Get<BlueprintItemArmor>("c18a1d22dca8b684195633d685855d0c") //enforcer
                                                 };

            for(int i = 0; i < robes.Length; i++)
            {
                var robe_enchants = Helpers.GetField<BlueprintEquipmentEnchantment[]>(robes[i], "m_Enchantments");
                robe_enchants[1] = unarmed_bonus[2 * i];
                Helpers.SetField(robes[i], "m_Enchantments", robe_enchants);
            }
        }


        static void createSummonedWeaponEnchantment()
        {
            summoned_weapon_enchant = Common.createWeaponEnchantment("SummonedWeaponEnchant",
                                                                      "Summoned",
                                                                      "This is a summoned weapon.",
                                                                      "",
                                                                      "",
                                                                      "",
                                                                      0,
                                                                      null
                                                                      );

            BlueprintWeaponType[] kinetic_blade_types = new BlueprintWeaponType[] {library.Get<BlueprintWeaponType>("b05a206f6c1133a469b2f7e30dc970ef"),
                                                                                   library.Get<BlueprintWeaponType>("a15b2fb1d5dc4f247882a7148d50afb0")
                                                                                  };
            //add it to kinetic blades
            var kinetic_blades = library.GetAllBlueprints().OfType<BlueprintItemWeapon>().Where(w => kinetic_blade_types.Contains(w.Type));
            foreach (var kb in kinetic_blades)
            {
                Common.addEnchantment(kb, summoned_weapon_enchant);
            }
        }


        static void createMetamagicEnchantments()
        {
            empower_enchant = Common.createWeaponEnchantment("EmpowerWeaponEnchantment",
                                              "Empowered",
                                              "All variable, numeric effects of an empowered spell are increased by half including bonuses to those dice rolls.",
                                              "",
                                              "",
                                              "",
                                              0,
                                              null,
                                              Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponMetamagicDamage>(w => w.empower = true)
                                              );
            maximize_enchant = Common.createWeaponEnchantment("MaximizeWeaponEnchantment",
                                                         "Maximized",
                                                         "All variable, numeric effects of a spell are maximized.",
                                                         "",
                                                         "",
                                                         "",
                                                         0,
                                                         null,
                                                         Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponMetamagicDamage>(w => w.maximize = true)
                                                         );

            DamageEnergyType[] elements = new DamageEnergyType[] { DamageEnergyType.Cold, DamageEnergyType.Acid, DamageEnergyType.Electricity, DamageEnergyType.Fire };

            foreach (var elt  in elements)
            {
                var enchant = Common.createWeaponEnchantment(elt.ToString() + "ElementalWeaponEnchantment",
                                             elt.ToString(),
                                             "Weapon damage type is changed to " + elt.ToString().ToLower() + ".",
                                             "",
                                             "",
                                             "",
                                             0,
                                             null,
                                             Helpers.Create<NewMechanics.EnchantmentMechanics.ReplaceEnergyDamage>(r => r.energy_descriptor = elt)
                                             );
                elemental_enchants.Add(elt, enchant);
            }
        }


        static public BlueprintWeaponEnchantment createRimeEnchantment(string name, BlueprintBuff context_buff)
        {
            BlueprintBuff entangled = library.Get<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38");
            var enchant = Common.createWeaponEnchantment(name,
                             "Rime",
                             MetamagicFeats.rime_metamagic.Description,
                             "",
                             "",
                             "",
                             0,
                             null,
                             Helpers.Create<NewMechanics.EnchantmentMechanics.ApplyBuffDamageDealtWithDCFromSpecifiedBuff>(a =>
                                                                                                                             {
                                                                                                                                 a.use_damage_energy_type = true;
                                                                                                                                 a.energy_descriptor = DamageEnergyType.Cold;
                                                                                                                                 a.context_buff = context_buff;
                                                                                                                                 a.effect_buff = entangled;
                                                                                                                             }
                                                                                                                             )
                             );

            return enchant;
        }


        static public BlueprintWeaponEnchantment createDazingEnchantment(string name, BlueprintBuff context_buff)
        {
            BlueprintBuff dazed = library.Get<BlueprintBuff>("9934fedff1b14994ea90205d189c8759");
            var enchant = Common.createWeaponEnchantment(name,
                             "Dazing",
                             MetamagicFeats.dazing_metamagic.Description,
                             "",
                             "",
                             "",
                             0,
                             null,
                             Helpers.Create<NewMechanics.EnchantmentMechanics.ApplyBuffDamageDealtWithDCFromSpecifiedBuff>(a =>
                                                                                                                             {
                                                                                                                                 a.context_buff = context_buff;
                                                                                                                                 a.effect_buff = dazed;
                                                                                                                                 a.save_type = Kingmaker.EntitySystem.Stats.SavingThrowType.Will;
                                                                                                                             }
                                                                                                                          )
                             );

            return enchant;
        }
    }
}
