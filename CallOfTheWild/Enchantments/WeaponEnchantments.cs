using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
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
        static public BlueprintWeaponEnchantment summoned_weapon_enchant;

        static public void initialize()
        {
            createMetamagicEnchantments();
            createSummonedWeaponEnchantment();
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
        }
    }
}
