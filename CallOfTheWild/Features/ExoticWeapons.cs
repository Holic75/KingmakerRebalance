using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Kingmaker.View.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    
    public class ExoticWeapons
    {
        static LibraryScriptableObject library => Main.library;

        static internal void load()
        {
            fixMartialExoticWeapons();
        }



        static void fixMartialExoticWeapons()
        {
            var martial_weapon_prof = library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629");
            var profs = martial_weapon_prof.GetComponent<AddProficiencies>();
            profs.WeaponProficiencies = profs.WeaponProficiencies.AddToArray(WeaponCategory.BastardSword,
                                                                             WeaponCategory.DwarvenWaraxe,
                                                                             WeaponCategory.DuelingSword);

            var bastard_sword_type = library.Get<BlueprintWeaponType>("d2fe2c5516b56f04da1d5ea51ae3ddfe");
            Helpers.SetField(bastard_sword_type, "m_IsTwoHanded", true);
            WeaponVisualParameters bs_visuals = Helpers.GetField<WeaponVisualParameters>(bastard_sword_type, "m_VisualParameters");
            Helpers.SetField(bs_visuals, "m_WeaponAnimationStyle", WeaponAnimationStyle.SlashingTwoHanded);

            var dwarven_waraxe_type = library.Get<BlueprintWeaponType>("a6925f5f897801449a648d865637e5a0");
            Helpers.SetField(dwarven_waraxe_type, "m_IsTwoHanded", true);
            WeaponVisualParameters dw_visuals = Helpers.GetField<WeaponVisualParameters>(dwarven_waraxe_type, "m_VisualParameters");
            Helpers.SetField(dw_visuals, "m_WeaponAnimationStyle", WeaponAnimationStyle.SlashingTwoHanded);


            var bastard_sword_proficiency = library.Get<BlueprintFeature>("57299a78b2256604dadf1ab9a42e2873");
            bastard_sword_proficiency.RemoveComponents<PrerequisiteNotProficient>();
            bastard_sword_proficiency.AddComponent(Helpers.Create<HoldingItemsMechanics.PrerequisiteNoExoticProficiency>(p => p.category = WeaponCategory.BastardSword));
            var bs_profs = bastard_sword_proficiency.GetComponent<AddProficiencies>();
            bs_profs.WeaponProficiencies = bs_profs.WeaponProficiencies.AddToArray(WeaponCategory.BastardSword);
            bastard_sword_proficiency.AddComponent(Helpers.Create<HoldingItemsMechanics.CanHoldIn1Hand>(c =>
            {
                c.category = WeaponCategory.BastardSword;
                c.except_types = new BlueprintWeaponType[]
                {
                    library.Get<BlueprintWeaponType>("1e460a2dc830cf546939b2723aa875e7"), //oversized bastard sword
                    library.Get<BlueprintWeaponType>("58dcff8d1dd5d094eb345e8eeb2e4626"), //oversized bastard sword no penalty
                };

            }));

            var dwarven_waraxe_proficiency = library.Get<BlueprintFeature>("bd0d7feca087d2247b12965c1467790c");
            dwarven_waraxe_proficiency.RemoveComponents<PrerequisiteNotProficient>();
            dwarven_waraxe_proficiency.AddComponent(Helpers.Create<HoldingItemsMechanics.PrerequisiteNoExoticProficiency>(p => p.category = WeaponCategory.DwarvenWaraxe));
            var dw_profs = dwarven_waraxe_proficiency.GetComponent<AddProficiencies>();
            dw_profs.WeaponProficiencies = dw_profs.WeaponProficiencies.AddToArray(WeaponCategory.DwarvenWaraxe);
            dwarven_waraxe_proficiency.AddComponent(Helpers.Create<HoldingItemsMechanics.CanHoldIn1Hand>(c => c.category = WeaponCategory.DwarvenWaraxe));

            var duelling_sword_proficiency = library.Get<BlueprintFeature>("9c37279588fd9e34e9c4cb234857492c");
            duelling_sword_proficiency.RemoveComponents<PrerequisiteNotProficient>();
            duelling_sword_proficiency.AddComponent(Helpers.Create<HoldingItemsMechanics.PrerequisiteNoExoticProficiency>(p => p.category = WeaponCategory.DuelingSword));
            var ds_profs = duelling_sword_proficiency.GetComponent<AddProficiencies>();
            ds_profs.WeaponProficiencies = ds_profs.WeaponProficiencies.AddToArray(WeaponCategory.DuelingSword);
            duelling_sword_proficiency.AddComponent(Helpers.Create<NewMechanics.ConsiderAsFinessable>(c => c.category = WeaponCategory.DuelingSword));

            //remove duelling sword from list of finessable weapons
            var info = typeof(WeaponCategoryExtension).GetField("Data", BindingFlags.NonPublic | BindingFlags.Static);
            var data = (Array)info.GetValue(null);
            
            foreach (var dd in data)
            {
                var category = Helpers.GetField<WeaponCategory>(dd, "Category");
                if (category == WeaponCategory.DuelingSword)
                {
                    var subcategories = Helpers.GetField<WeaponSubCategory[]>(dd, "SubCategories");
                    subcategories = subcategories.RemoveFromArray(WeaponSubCategory.Finessable);
                    Helpers.SetField(dd, "SubCategories", subcategories);
                    break;
                }
            }

        }


        public static int getProficiencyRank(UnitDescriptor unit, WeaponCategory category)
        {
            if (!unit.Proficiencies.Contains(category))
            {
                return 0;
            }
            MultiSet<WeaponCategory> multiset = Helpers.GetField<MultiSet<WeaponCategory>>(unit.Proficiencies, "m_WeaponProficiencies");
            Dictionary<WeaponCategory, int> dict = Helpers.GetField<Dictionary<WeaponCategory, int>>(multiset, "m_Data");
            return dict[category];
        }
    }



    
}
