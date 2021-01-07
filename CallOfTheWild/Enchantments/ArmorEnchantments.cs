using System;
using Kingmaker.Blueprints;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Enums;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities;

namespace CallOfTheWild
{
    public static class ArmorEnchantments
    {
        static LibraryScriptableObject library => Main.library;
        public static BlueprintArmorEnchantment[] temporary_shield_enchantments;
        public static BlueprintArmorEnchantment[] temporary_armor_enchantments;
        public static BlueprintArmorEnchantment[] fortification_enchantments;
        public static BlueprintArmorEnchantment[] spell_resistance_enchantments;
        public static Dictionary<DamageEnergyType, BlueprintArmorEnchantment[]> energy_resistance_enchantments;
        public static BlueprintArmorEnchantment spell_storing;


        static internal void initialize()
        {
            createTemporaryArmorEnchantments();
            createTemporaryShieldEnchantmnets();
            createFortificationEnchantments();
            createSpellResistanceEnchantments();
            createEnergyResistanceEnchantments();
            createSpellStoring();
        }


        static void createSpellStoring()
        {
            var icon = Helpers.GetIcon("76d4885a395976547a13c5d6bf95b482"); //armor focus
            var feature = Helpers.CreateFeature("ArmorSpellStoringFeature",
                                                "Spell Storing Armor",
                                                "This armor allows a spellcaster to store a single touch spell of up to 3rd level in it. Anytime a creature hits the wearer with a melee attack or melee touch attack, the armor can cast the spell on that creature as a swift immediate action if the wearer desires. Once the spell has been cast from the armor, a spellcaster can cast any other targeted touch spell of up to 3rd level into it. The armor magically imparts to the wielder the name of the spell currently stored within it.",
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.Create<SpellManipulationMechanics.FactStoreSpell>(a => a.always_hit = true));

            var release_buff = Helpers.CreateBuff("ArmorSpellStoringToggleBuff",
                                                  feature.Name + ": Release",
                                                  feature.Description,
                                                  "",
                                                  feature.Icon,
                                                  null,
                                                  Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = feature));

            var major_activatable_ability = Helpers.CreateActivatableAbility("ArmorSpellStoringToggleAbility",
                                                                             feature.Name + ": Release",
                                                                             feature.Description,
                                                                             "",
                                                                             feature.Icon,
                                                                             release_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.Create<SpellManipulationMechanics.ActivatableAbilitySpellStoredInFactRestriction>(a => a.fact = feature));
            major_activatable_ability.DeactivateImmediately = true;

            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = feature);
            var release_on_condition = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(release_buff), release_action);
            feature.AddComponent(Common.createAddTargetAttackWithWeaponTrigger(action_attacker: Helpers.CreateActionList(release_on_condition), not_reach: false));
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
                var ability = Helpers.CreateAbility($"ArmorSpellStoring{i + 1}Ability",
                                                          feature.Name,
                                                          feature.Description,
                                                          "",
                                                          feature.Icon,
                                                          AbilityType.Supernatural,
                                                          CommandType.Standard,
                                                          AbilityRange.Personal,
                                                          "",
                                                          "",
                                                          Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s => { s.fact = feature; s.check_slot_predicate = check_slot_predicate; s.variant = i; })
                                                          );
                ability.setMiscAbilityParametersSelfOnly();
                feature.AddComponent(Helpers.CreateAddFact(ability));
            }

            spell_storing = Common.createArmorEnchantment("SpellStoringArmorEnchantment",
                                                                  "Spell Storing",
                                                                  feature.Description,
                                                                  "",
                                                                  "",
                                                                  "",
                                                                  0,
                                                                  1,
                                                                  Helpers.Create<AddUnitFeatureEquipment>(a => a.Feature = feature)
                                                                  );
        }



        static void createTemporaryArmorEnchantments()
        {
            BlueprintArmorEnchantment[] armor_enchantments = {Main.library.Get< BlueprintArmorEnchantment>("a9ea95c5e02f9b7468447bc1010fe152"), //+1
                                                              Main.library.Get< BlueprintArmorEnchantment>("758b77a97640fd747abf149f5bf538d0"), //+2
                                                              Main.library.Get< BlueprintArmorEnchantment>("9448d3026111d6d49b31fc85e7f3745a"), //+3 
                                                              Main.library.Get< BlueprintArmorEnchantment>("eaeb89df5be2b784c96181552414ae5a"), //+4
                                                              Main.library.Get< BlueprintArmorEnchantment>("6628f9d77fd07b54c911cd8930c0d531")  //+5
                                                             };
            temporary_armor_enchantments = new BlueprintArmorEnchantment[armor_enchantments.Length];

            for (int i = 0; i < armor_enchantments.Length; i++)
            {
                temporary_armor_enchantments[i] = Main.library.CopyAndAdd<BlueprintArmorEnchantment>(armor_enchantments[i].AssetGuid, $"TemporaryArmorEnchantment{i + 1}", "");
                Helpers.SetField(temporary_armor_enchantments[i], "m_EnchantName", Helpers.CreateString($"TemporaryArmorEnchantment{i + 1}.Name", $"Temporary Enhancement + {i + 1}"));
            }
        }


        static void createTemporaryShieldEnchantmnets()
        {
            BlueprintArmorEnchantment[] shield_enchantments = {Main.library.Get< BlueprintArmorEnchantment>("e90c252e08035294eba39bafce76c119"), //+1
                                                              Main.library.Get< BlueprintArmorEnchantment>("7b9f2f78a83577d49927c78be0f7fbc1"), //+2
                                                              Main.library.Get< BlueprintArmorEnchantment>("ac2e3a582b5faa74aab66e0a31c935a9"), //+3 
                                                              Main.library.Get< BlueprintArmorEnchantment>("a5d27d73859bd19469a6dde3b49750ff"), //+4
                                                              Main.library.Get< BlueprintArmorEnchantment>("84d191a748edef84ba30c13b8ab83bd9")  //+5
                                                             };

            temporary_shield_enchantments = new BlueprintArmorEnchantment[shield_enchantments.Length];

            for (int i = 0; i < temporary_shield_enchantments.Length; i++)
            {
                temporary_shield_enchantments[i] = Main.library.CopyAndAdd<BlueprintArmorEnchantment>(shield_enchantments[i].AssetGuid, $"TemporaryShieldEnchantment{i + 1}", "");
                Helpers.SetField(temporary_shield_enchantments[i], "m_EnchantName", Helpers.CreateString($"TemporaryShieldEnchantment{i + 1}.Name", $"Temporary Enhancement + {i + 1}"));
            }
        }


        static void createFortificationEnchantments()
        {
            int[] fortificaiton_values = new int[] { 25, 50, 75 };
            int[] enchantment_cost = new int[]{1, 3, 5};
            string[] prefix = new string[] { "Light", "Moderate", "Heavy" };
            fortification_enchantments = new BlueprintArmorEnchantment[fortificaiton_values.Length];

            for (int i = 0; i < fortificaiton_values.Length; i++)
            {
                var feature = Helpers.CreateFeature($"Fortifiaction{fortificaiton_values[i]}EncahntmentFeature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Common.createAddFortification(fortificaiton_values[i])
                                                    );
                feature.HideInCharacterSheetAndLevelUp = true;

                fortification_enchantments[i] = Common.createArmorEnchantment($"Fortification{fortificaiton_values[i]}Enchantment",
                                                                              $"{prefix[i]} Fortification",
                                                                              $"This suit of armor or shield produces a magical force that protects vital areas of the wearer more effectively. When a critical hit or sneak attack is scored on the wearer, there is a {fortificaiton_values[i]}% chance that the critical hit or sneak attack is negated and damage is instead rolled normally.",
                                                                              "",
                                                                              "",
                                                                              "",
                                                                              0,
                                                                              enchantment_cost[i],
                                                                              Helpers.Create<AddUnitFeatureEquipment>(a => a.Feature = feature)
                                                                              );
            }
        }


        static void createSpellResistanceEnchantments()
        {
            int[] sr_values = new int[] { 13, 17, 21, 25 };
            int[] enchantment_cost = new int[] { 2, 3, 4, 5 };
           
            spell_resistance_enchantments = new BlueprintArmorEnchantment[sr_values.Length];

            for (int i = 0; i < sr_values.Length; i++)
            {
                var feature = Helpers.CreateFeature($"SpellResistance{sr_values[i]}EncahntmentFeature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.Create<AddSpellResistance>(a => a.Value = Common.createSimpleContextValue(sr_values[i]))
                                                    );
                feature.HideInCharacterSheetAndLevelUp = true;

                spell_resistance_enchantments[i] = Common.createArmorEnchantment($"SpellResistance{sr_values[i]}Enchantment",
                                                                              $"Spell Resistance {sr_values[i]}",
                                                                              $"This special ability grants the armor or shield’s wearer spell resistance {sr_values[i]} while the armor or shield is worn.",
                                                                              "",
                                                                              "",
                                                                              "",
                                                                              0,
                                                                              enchantment_cost[i],
                                                                              Helpers.Create<AddUnitFeatureEquipment>(a => a.Feature = feature)
                                                                              );
            }
        }


        static void createEnergyResistanceEnchantments()
        {
            energy_resistance_enchantments = new Dictionary<DamageEnergyType, BlueprintArmorEnchantment[]>();
            DamageEnergyType[] energies = new DamageEnergyType[] { DamageEnergyType.Fire, DamageEnergyType.Cold, DamageEnergyType.Electricity, DamageEnergyType.Acid};
            int[] dr_values = new int[] { 10, 20, 30 };
            int[] enchantment_cost = new int[] { 1, 2, 4 };
            string[] prefix = new string[]{ " ", "Improved ", "Greater " };

            foreach (var e in energies)
            {
                var enchants = new BlueprintArmorEnchantment[dr_values.Length];
                var energy_string = e.ToString();
                for (int i = 0; i < dr_values.Length; i++)
                {

                    var feature = Helpers.CreateFeature($"{energy_string}Resistance{dr_values[i]}EncahntmentFeature",
                                    "",
                                    "",
                                    "",
                                    null,
                                    FeatureGroup.None,
                                    Common.createEnergyDR(dr_values[i], e)
                                    );
                    feature.HideInCharacterSheetAndLevelUp = true;

                    enchants[i] = Common.createArmorEnchantment($"{energy_string}esistance{dr_values[i]}Enchantment",
                                                                                  $"{prefix[i]}{energy_string} Resistance {dr_values[i]}",
                                                                                  $"A suit of armor or a shield with this special ability protects against {energy_string.ToLower()}. The armor absorbs the first {dr_values[i]} points of {energy_string.ToLower()} energy damage per attack that the wearer would normally take.",
                                                                                  "",
                                                                                  "",
                                                                                  "",
                                                                                  0,
                                                                                  enchantment_cost[i],
                                                                                  Helpers.Create<AddUnitFeatureEquipment>(a => a.Feature = feature)
                                                                                  );
                }
                energy_resistance_enchantments.Add(e, enchants);
            }

        }

    }
}
