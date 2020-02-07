using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        static public void initialize()
        {
            createMetamagicEnchantments();
            createSummonedWeaponEnchantment();
            createStaticEnchants();
            //fix weapon enchants to be non cumulative
            fixEnchants();
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
