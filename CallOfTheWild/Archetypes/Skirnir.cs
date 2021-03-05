using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
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
using System;
using System.Collections.Generic;
using System.Linq;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class Skirnir
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature diminished_spellcasting;
        static public BlueprintFeature bonded_item;
        static public BlueprintFeature[] shield_arcane_pool_enchantment = new BlueprintFeature[5];
        static public BlueprintFeature sorcerous_shield;
        static public BlueprintFeature shielded_spellstrike;
        static public BlueprintFeature spell_shield;
        static public BlueprintFeature shielded_spell_combat;
        static public BlueprintFeature greater_shielded_spellcombat;
        static public BlueprintFeature greater_spellshield;

        static public BlueprintSpellbook spellbook;

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var magus_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SkirnirArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Skirnir");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Sometimes called a shield-vassal or shieldmaiden, the skirnir has learned to infuse his power into his shield.");
            });
            Helpers.SetField(archetype, "m_ParentClass", magus_class);
            library.AddAsset(archetype, "");

            createDiminishedSpellCasting();
            createBondedItem();
            createShieldArcanePool();
            createSorcerousShield();
            createShieldedSpellstrikeAndSpellCombat();
            createSpellShield();

            var spell_combat = library.Get<BlueprintFeature>("2464ba53317c7fc4d88f383fac2b45f9");
            var spellstrike = library.Get<BlueprintFeature>("be50f4e97fff8a24ba92561f1694a945");
            var arcane_pool_feature = library.Get<BlueprintFeature>("3ce9bb90749c21249adc639031d5eed1");
            var magus_arcana = library.Get<BlueprintFeatureSelection>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            var improved_spell_combat = library.Get<BlueprintFeature>("836879fcd5b29754eb664a090bd6c22f");
            var greater_spell_combat = library.Get<BlueprintFeature>("379887a82a7248946bbf6d0158663b5e");
            var greater_spell_access = library.Get<BlueprintFeature>("de18c849c41dbfa44801d812376c707d");
            var counterstrike = library.Get<BlueprintFeature>("cd96b7275c206da4899c69ae127ffda6");

            var spell_recall = library.Get<BlueprintFeature>("61fc0521e9992624e9c518060bf89c0f");
            var improved_spell_recall = library.Get<BlueprintFeature>("0ef6ec1c2fdfc204fbd3bff9f1609490");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, spell_combat),
                                                          Helpers.LevelEntry(2, spellstrike),
                                                          Helpers.LevelEntry(4, spell_recall),
                                                          Helpers.LevelEntry(8, improved_spell_combat),
                                                          Helpers.LevelEntry(11, improved_spell_recall),
                                                          Helpers.LevelEntry(14, greater_spell_combat),
                                                          Helpers.LevelEntry(16, counterstrike),
                                                          Helpers.LevelEntry(19, greater_spell_access),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, shielded_spellstrike, shield_arcane_pool_enchantment[0], sorcerous_shield, bonded_item, diminished_spellcasting),
                                                          Helpers.LevelEntry(5, shield_arcane_pool_enchantment[1]),
                                                          Helpers.LevelEntry(7, spell_shield),
                                                          Helpers.LevelEntry(8, shielded_spell_combat),
                                                          Helpers.LevelEntry(9, shield_arcane_pool_enchantment[2]),
                                                          Helpers.LevelEntry(13, shield_arcane_pool_enchantment[3]),
                                                          Helpers.LevelEntry(14, improved_spell_combat),
                                                          Helpers.LevelEntry(16, greater_spellshield),
                                                          Helpers.LevelEntry(17, shield_arcane_pool_enchantment[4]),
                                                          Helpers.LevelEntry(19, greater_shielded_spellcombat),
                                                       };
            magus_class.Archetypes = magus_class.Archetypes.AddToArray(archetype);

            magus_class.Progression.UIGroups[2].Features.AddRange(new BlueprintFeature[] { shielded_spellstrike, shielded_spell_combat, greater_shielded_spellcombat });
            magus_class.Progression.UIGroups = magus_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(spell_shield, greater_spellshield));
            magus_class.Progression.UIGroups = magus_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(shield_arcane_pool_enchantment));
            magus_class.Progression.UIDeterminatorsGroup = magus_class.Progression.UIDeterminatorsGroup.AddToArray(bonded_item, diminished_spellcasting, sorcerous_shield);

            archetype.ReplaceSpellbook = spellbook;
            archetype.ReplaceStartingEquipment = true;
            //add shield
            archetype.StartingItems = archetype.GetParentClass().StartingItems.AddToArray(library.Get<BlueprintItemShield>("f4cef3ba1a15b0f4fa7fd66b602ff32b"));
        }


        static void createSpellShield()
        {
            var arcane_pool_resource = library.Get<BlueprintAbilityResource>("effc3e386331f864e9e06d19dc218b37");
            spell_shield = Helpers.CreateFeature("SpellShieldFeature",
                                    "Spellshield",
                                    "At 7th level, as a standard action, a skirnir may store a magus spell in his shield by spending 1 point from his arcane pool per level of the spell. This functions as the spell storing weapon special ability, but activates only on a successful shield bash by the skirnir and is not limited to spells of 3rd level or less. ",
                                    "",
                                    Helpers.GetIcon("ef768022b0785eb43a18969903c537c4"), //mage shield
                                    FeatureGroup.None,
                                    Helpers.Create<SpellManipulationMechanics.FactStoreSpell>(f => f.always_hit = true));

            var release_buff = Helpers.CreateBuff("SpellShieldToggleBuff",
                                                  spell_shield.Name + ": Release",
                                                  spell_shield.Description,
                                                  "",
                                                  spell_shield.Icon,
                                                  null,
                                                  Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = spell_shield));

            var spell_shield_activatable_ability = Helpers.CreateActivatableAbility("SpellShieldToggleAbility",
                                                                             spell_shield.Name + ": Release",
                                                                             spell_shield.Description,
                                                                             "",
                                                                             spell_shield.Icon,
                                                                             release_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.Create<NewMechanics.ActivatableAbilityHasShieldRestriction>(),
                                                                             Helpers.Create<SpellManipulationMechanics.ActivatableAbilitySpellStoredInFactRestriction>(a => a.fact = spell_shield));
            spell_shield_activatable_ability.DeactivateImmediately = true;

            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = spell_shield);
            var release_on_condition = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(release_buff),
                                                                 release_action
                                                                 );
            var on_attack_action = Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(release_on_condition), weapon_category: WeaponCategory.SpikedHeavyShield);

            spell_shield.AddComponents(Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(release_on_condition), weapon_category: WeaponCategory.SpikedHeavyShield),
                                       Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(release_on_condition), weapon_category: WeaponCategory.SpikedLightShield),
                                       Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(release_on_condition), weapon_category: WeaponCategory.WeaponHeavyShield),
                                       Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(release_on_condition), weapon_category: WeaponCategory.WeaponLightShield));
            spell_shield.AddComponent(Helpers.CreateAddFact(spell_shield_activatable_ability));

            int max_variants = 10; //due to ui limitation
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return spell.Spellbook?.Blueprint == archetype.ReplaceSpellbook
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
                var ability = Helpers.CreateAbility($"SpellShield{i + 1}Ability",
                                                        spell_shield.Name,
                                                        spell_shield.Description,
                                                        "",
                                                        spell_shield.Icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Standard,
                                                        AbilityRange.Personal,
                                                        "",
                                                        "",
                                                        Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s => { s.fact = spell_shield; s.check_slot_predicate = check_slot_predicate; s.variant = i; }),
                                                        Helpers.CreateResourceLogic(arcane_pool_resource, cost_is_custom: true),
                                                        library.Get<BlueprintAbility>("1bd76e00b6e056d42a8ecc1031dd43b4").GetComponent<AbilityMagusSpellRecallCostCalculator>()
                                                        );
                ability.setMiscAbilityParametersSelfOnly();
                spell_shield.AddComponent(Helpers.CreateAddFact(ability));
            }

            greater_spellshield = Helpers.CreateFeature("GreaterSpellshieldFeature",
                                                      "Greater Spellshield",
                                                      "At 16th level, a skirnir may activate a stored spell as a swift action after being struck in combat.",
                                                      "",
                                                      Helpers.GetIcon("183d5bb91dea3a1489a6db6c9cb64445"), //shield of faith
                                                      FeatureGroup.None);

            var release_buff2 = Helpers.CreateBuff("GreaterSpellshieldToggleBuff",
                                                  greater_spellshield.Name + ": Release",
                                                  greater_spellshield.Description,
                                                  "",
                                                  greater_spellshield.Icon,
                                                  null,
                                                  Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = spell_shield));

            var greater_spell_shield_ability = Helpers.CreateActivatableAbility("GreaterSpellshieldToggleAbility",
                                                                             greater_spellshield.Name + ": Release",
                                                                             greater_spellshield.Description,
                                                                             "",
                                                                             greater_spellshield.Icon,
                                                                             release_buff2,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.Create<NewMechanics.ActivatableAbilityLightOrNoArmor>(),
                                                                             Helpers.Create<SpellManipulationMechanics.ActivatableAbilitySpellStoredInFactRestriction>(a => a.fact = spell_shield));
            greater_spell_shield_ability.DeactivateImmediately = true;

            var release_on_condition_swift = Helpers.CreateConditional(new Condition[]{Common.createContextConditionCasterHasFact(release_buff2),
                                                                                 Helpers.Create<TurnActionMechanics.ContextConditionHasAction>(c => {c.has_swift = true; c.check_caster = true; })
                                                                                },
                                                     new GameAction[]{release_action,
                                                                                  Helpers.Create<TurnActionMechanics.ConsumeAction>(c => {c.consume_swift = true; c.from_caster = true; })
                                                                     }
                                                     );
            var on_attacked_action = Common.createAddTargetAttackWithWeaponTrigger(action_attacker: Helpers.CreateActionList(release_on_condition_swift),
                                                                                  not_reach: false, only_melee: true,
                                                                                   wait_for_attack_to_resolve: true);
            greater_spellshield.AddComponent(on_attacked_action);
            greater_spellshield.AddComponent(Helpers.CreateAddFact(greater_spell_shield_ability));
        }


        static void createShieldedSpellstrikeAndSpellCombat()
        {
            var add_spell_combat = Helpers.Create<AddMagusMechanicPart>(); //needed for unit_part magus creation (no actual ability though)
            Helpers.SetField(add_spell_combat, "m_Feature", 1);
            Helpers.SetField(add_spell_combat, "m_MagusClass", archetype.GetParentClass());
            //it should not be used since it sets spell combat to be always active by default (ok on magus though since the corresponding toggle is always active by default)

            var add_magus_part = Helpers.Create<NewMechanics.ActivateUnitPartMagus>(a => a.magus_class = archetype.GetParentClass());

            var add_spellstrike = Helpers.Create<AddMagusMechanicPart>();
            Helpers.SetField(add_spellstrike, "m_Feature", 2);

            var spellstrike = library.CopyAndAdd<BlueprintActivatableAbility>("e958891ef90f7e142a941c06c811181e", "SkirnirSpellstrikeActivatableAbility", "");
            spellstrike.SetDescription("At 1st level, a skirnir may use this ability with a weapon or shield bash attack.");

            shielded_spellstrike = Common.ActivatableAbilityToFeature(spellstrike, false);
            shielded_spellstrike.AddComponents(add_magus_part, add_spellstrike);

            var spell_combat = library.CopyAndAdd<BlueprintActivatableAbility>("8898a573e8a8a184b8186dbc3a26da74", "SkirnirSpellCombatACtivatableAbility", "");
            spell_combat.SetNameDescription("Shield Spell Combat",
                                            "At 8th level, a skirnir gains the spell combat ability, but only when wielding his bonded shield. A skirnir may use his shield hand to perform somatic components for magus spells, forfeiting the shield’s bonus to AC until the beginning of his next turn; if the bonded shield is a buckler, he retains its bonus to AC. At 14th level, he gains the benefits of improved spell combat.\n"
                                            + "At 19th level, he retains his shield’s bonus to AC when using any type of shield with spell combat."
                                            );
            spell_combat.AddComponent(Helpers.Create<NewMechanics.ActivatableAbilityHasShieldRestriction>());
            shielded_spell_combat = Common.ActivatableAbilityToFeature(spell_combat, false);
            shielded_spell_combat.AddComponent(library.Get<BlueprintFeature>("2464ba53317c7fc4d88f383fac2b45f9").GetComponent<AddMagusMechanicPart>());
            shielded_spell_combat.AddComponent(Helpers.Create<HoldingItemsMechanics.UseSpellCombatWithShield>());

            //TODO: need to remove shield ac when using spell combat if skirnir lvl < 19 and it is not a buckler
            //Simply subtracting shield / shield enhancement and shield focus AC looks tempting but it will likely conflict with brilliant energy and pinpoint targeting
            var spell_combat_penalty = library.Get<BlueprintBuff>("7b4cf64d3a49e3d45b1dbd2385f4eb6d");
            greater_shielded_spellcombat = Helpers.CreateFeature("GreaterSpellCombatFeature",
                                                                 "",
                                                                 "",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.None
                                                                 );
            greater_shielded_spellcombat.HideInCharacterSheetAndLevelUp = true;
            greater_shielded_spellcombat.HideInUI = true;

            var penalty_buff = Helpers.CreateBuff("ShieldedSpellCombatPenalty",
                                                  "Shielded Spell Combat Penalty",
                                                  shielded_spell_combat.Description,
                                                  "",
                                                  shielded_spell_combat.Icon,
                                                  null,
                                                  Helpers.Create<RemoveShieldACIfHasShield>(r =>
                                                  {
                                                      r.only_if_has_feature = shielded_spell_combat;
                                                      r.unless_feature = greater_shielded_spellcombat;
                                                      r.proficiency_groups = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.HeavyShield,
                                                                                                         ArmorProficiencyGroup.LightShield,
                                                                                                         ArmorProficiencyGroup.TowerShield };
                                                  })
                                                  );
            var penalty_action = Helpers.CreateConditional(Common.createContextConditionHasFact(shielded_spell_combat), Common.createContextActionApplyBuff(penalty_buff, Helpers.CreateContextDuration(1), dispellable: false));
            spell_combat_penalty.AddComponent(Helpers.CreateAddFactContextActions(activated: penalty_action));
        }


        static void createSorcerousShield()
        {
            sorcerous_shield = Helpers.CreateFeature("SorcerousShieldFeature",
                                                     "Sorcerous Shield",
                                                     "At 1st level, skirnirs are proficient with all types of shields, including tower shields, and do not suffer an arcane spell failure chance when casting magus spells while using a shield.",
                                                     "",
                                                     Helpers.GetIcon("cb8686e7357a68c42bdd9d4e65334633"), //shield proficiency
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFacts(library.Get<BlueprintFeature>("cb8686e7357a68c42bdd9d4e65334633"), //shield proficiency
                                                                            library.Get<BlueprintFeature>("6105f450bb2acbd458d277e71e19d835")
                                                                            ),//tower shield proficiency  
                                                     Common.createArcaneArmorProficiency(ArmorProficiencyGroup.Buckler, ArmorProficiencyGroup.LightShield, ArmorProficiencyGroup.HeavyShield, ArmorProficiencyGroup.TowerShield)
                                                     );
        }


        static void createShieldArcanePool()
        {
            var enduring_blade_buff = library.Get<BlueprintBuff>("3c2fe8e0374d28d4185355121f4c4544");
            var resource = library.Get<BlueprintAbilityResource>("effc3e386331f864e9e06d19dc218b37");
            var group = ActivatableAbilityGroupExtension.SkirnirArcaneShieldEnchantment.ToActivatableAbilityGroup();
            var enchants = ArmorEnchantments.temporary_armor_enchantments;
            var enhancement_buff = Helpers.CreateBuff("SkirnirArcaneShieldEnchancementBaseBuff",
                                            "",
                                            "",
                                            "",
                                            null,
                                            null,
                                            Common.createBuffRemainingGroupSizetEnchantShield(group, false, true, enchants)
                                            );
            enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var bond_enhancement_buff = Helpers.CreateBuff("SkirnirArcaneShieldEnhancementSwitchBuff",
                                                            "Arcane Shield Enchantment",
                                                            "At 1st level, a skirnir can use his arcane pool to grant an enhancement bonus to a weapon as normal, as well as to his shield, paying the arcane pool cost separately for each. At 5th level and above, he can also add the following shield special abilities: fortification (any), spell resistance (any).",
                                                            "",
                                                            Helpers.GetIcon("68666566c506d344bad1e30bc3194fed"), //tss touch ac
                                                            null,
                                                            Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                        is_child: true, dispellable: false, is_permanent: true)
                                                                                                )
                                                            );
            //fortification - light (25% +1), medium(50% +3), heavy (+75%)

            var fortification_icon = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b").Icon; //stoneskin
            var sr_icon = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889").Icon;

            List<BlueprintActivatableAbility>[] enchant_abilities = new List<BlueprintActivatableAbility>[5];

            for (int i = 0; i < enchant_abilities.Length; i++)
            {
                enchant_abilities[i] = new List<BlueprintActivatableAbility>();
            }

            foreach (var e in ArmorEnchantments.fortification_enchantments)
            {
                int cost = e.EnchantmentCost;
                var a = Common.createShieldEnchantmentAbility("SkirnirArcaneShield" + e.name, "Arcane Shield Enchantment - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of shield enhancement bonus.", fortification_icon, new BlueprintBuff[] { bond_enhancement_buff }, e, cost, group);
                enchant_abilities[cost - 1].Add(a);
            }

            foreach (var e in ArmorEnchantments.spell_resistance_enchantments)
            {
                int cost = e.EnchantmentCost;
                var a = Common.createShieldEnchantmentAbility("SkirnirArcaneShield" + e.name, "Arcane Shield Enchantment - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of shield enhancement bonus.", sr_icon, new BlueprintBuff[] { bond_enhancement_buff }, e, cost, group);
                enchant_abilities[cost - 1].Add(a);
            }

            var apply_buff = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(enduring_blade_buff),
                                                      Common.createContextActionApplyBuff(bond_enhancement_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false),
                                                      Common.createContextActionApplyBuff(bond_enhancement_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)
                                                      );
            var ability = Helpers.CreateAbility("SkirnirArcaneShieldAbility",
                                                bond_enhancement_buff.Name,
                                                bond_enhancement_buff.Description,
                                                "",
                                                bond_enhancement_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                Helpers.savingThrowNone,
                                                library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4").GetComponent<AbilitySpawnFx>(), //shield spell fx
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateResourceLogic(resource, cost_is_custom: true),
                                                Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_increasing_facts = new Kingmaker.Blueprints.Facts.BlueprintFact[] { enduring_blade_buff }),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] {archetype.GetParentClass()}),
                                                Helpers.Create<NewMechanics.AbilityCasterHasShield>()
                                                );
            ability.setMiscAbilityParametersSelfOnly();
          
            for (int i = 0; i < shield_arcane_pool_enchantment.Length; i++)
            {
                shield_arcane_pool_enchantment[i] = Helpers.CreateFeature($"SkirnirArcaneShield{i + 1}Feature",
                                                        i == 0 ? "Arcane Shield" : $"Arcane Shield (+{i + 1})",
                                                        ability.Description,
                                                        "",
                                                        ability.Icon,
                                                        FeatureGroup.None
                                                        );
                if (i == 0)
                {
                    shield_arcane_pool_enchantment[i].AddComponent(Helpers.CreateAddFacts(ability));
                    continue;
                }
                if (i > 0)
                {
                    shield_arcane_pool_enchantment[i].AddComponent(Common.createIncreaseActivatableAbilityGroupSize(group));
                }
                if (enchant_abilities[i].Count > 0)
                {
                    shield_arcane_pool_enchantment[i].AddComponent(Helpers.CreateAddFacts(enchant_abilities[i].ToArray()));
                    if (i == 1)
                    {
                        shield_arcane_pool_enchantment[i].AddComponent(Helpers.CreateAddFacts(enchant_abilities[0].ToArray()));
                    }
                }
            }
        }


        static void createBondedItem()
        {
            var item_bond_ability = library.CopyAndAdd<BlueprintAbility>("e5dcf71e02e08fc448d9745653845df1", "SkirnirItemBond", "");
            item_bond_ability.SetDescription("At 1st level, a skirnir gains a shield (except for a tower shield) as an arcane bond item. This is identical to the wizard class ability, but the skirnir may only bond with a shield, not a familiar or other item.");
            item_bond_ability.AddComponent(Helpers.Create<NewMechanics.AbilityCasterHasShield>());

            bonded_item = Common.AbilityToFeature(item_bond_ability, false);
            bonded_item.AddComponent(item_bond_ability.GetComponent<AbilityResourceLogic>().RequiredResource.CreateAddAbilityResource());
        }


        static void createDiminishedSpellCasting()
        {
            spellbook = library.CopyAndAdd<BlueprintSpellbook>("682545e11e5306c45b14ca78bcbe3e62", "SkirnirSpellbook", "");
            spellbook.Name = Helpers.CreateString(spellbook.name + ".Name", archetype.Name);

            diminished_spellcasting = Helpers.CreateFeature("DiminishedSpellcastingSkirnirFeature",
                                                            "Diminished Spellcasting",
                                                            "A skirnir casts one fewer spell of each level than normal. If this reduces the number to 0, he may cast spells of that level only if his Intelligence allows bonus spells of that level.",
                                                            "",
                                                            null,
                                                            FeatureGroup.None
                                                            );

            var magus_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");

            //add check against arrowsong minstrel archetype for exisitng bard spellbooks
            var selections_to_fix = new BlueprintFeatureSelection[] {Common.EldritchKnightSpellbookSelection,
                                                                     Common.ArcaneTricksterSelection,
                                                                     Common.MysticTheurgeArcaneSpellbookSelection
                                                                    };
            foreach (var s in selections_to_fix)
            {
                foreach (var f in s.AllFeatures)
                {
                    if (f.GetComponents<PrerequisiteClassSpellLevel>().Where(c => c.CharacterClass == magus_class).Count() > 0)
                    {
                        f.AddComponent(Common.prerequisiteNoArchetype(magus_class, archetype));
                    }
                }
            }

            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, spellbook, "EldritchKnightArrowsongSkirnir",
                                       Common.createPrerequisiteClassSpellLevel(magus_class, 3),
                                       Common.createPrerequisiteArchetypeLevel(magus_class, archetype, 1));

            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, spellbook, "ArcaneTricksterArrowsongSkirinir",
                                       Common.createPrerequisiteClassSpellLevel(magus_class, 2),
                                       Common.createPrerequisiteArchetypeLevel(magus_class, archetype, 1));

            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, spellbook, "MysticTheurgeArrowosngSkirinir",
                                       Common.createPrerequisiteClassSpellLevel(magus_class, 2),
                                       Common.createPrerequisiteArchetypeLevel(magus_class, archetype, 1));
        }



        [ComponentName("remove shield ac")]
        public class RemoveShieldACIfHasShield : RuleTargetLogicComponent<RuleCalculateAC>
        { 
            public BlueprintFeature only_if_has_feature;
            public BlueprintFeature unless_feature;
            public ArmorProficiencyGroup[] proficiency_groups;

            public override void OnEventAboutToTrigger(RuleCalculateAC evt)
            {
                if (evt.AttackType.IsTouch())
                {
                    return;
                }

                if (evt.BrilliantEnergy != null)
                {
                    return;
                }

                if (unless_feature != null && evt.Target.Descriptor.HasFact(unless_feature))
                {
                    return;
                }
                if (only_if_has_feature != null && !evt.Target.Descriptor.HasFact(only_if_has_feature))
                {
                    return;
                }
                var shield = evt.Target?.Body?.SecondaryHand?.MaybeShield?.ArmorComponent;
                if (shield == null || !proficiency_groups.Contains(shield.Blueprint.ProficiencyGroup))
                {
                    return;
                }
                int shield_ac = 0;
                foreach (ModifiableValue.Modifier modifier in evt.Target.Stats.AC.Modifiers)
                {
                    shield_ac += modifier.ModDescriptor == ModifierDescriptor.Shield ? modifier.ModValue : 0;
                    shield_ac += modifier.ModDescriptor == ModifierDescriptor.ShieldEnhancement ? modifier.ModValue : 0;
                    shield_ac += modifier.ModDescriptor == ModifierDescriptor.ShieldFocus ? modifier.ModValue : 0;
                }
                evt.AddBonus(-shield_ac, this.Fact);
            }

            public override void OnEventDidTrigger(RuleCalculateAC evt)
            {
            }
        }
    }
}

