using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
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

        public class UnitPartFullProficiency: AdditiveUnitPart
        {
            public bool hasFullProficiency(WeaponCategory category)
            {
                bool is_ok = false;
                foreach (var b in buffs)
                {
                    b.CallComponents<FullProficiency>(c => is_ok = c.hasProficiency(category));
                    if (is_ok)
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        public class FullProficiency: OwnedGameLogicComponent<UnitDescriptor>
        {
            public WeaponCategory category;
            public BlueprintRace race;

            public override void OnFactActivate()
            {
                this.Owner.Ensure<UnitPartFullProficiency>().addBuff(this.Fact);
            }

            public override void OnFactDeactivate()
            {
                this.Owner.Ensure<UnitPartFullProficiency>().removeBuff(this.Fact);
            }

            public bool hasProficiency(WeaponCategory required_category)
            {
                if (race == null || this.Owner.Progression.Race == race)
                {
                    return category == required_category;
                }
                return false;
            }
        }




        static LibraryScriptableObject library => Main.library;


        static internal void load()
        {
            fixMartialExoticWeapons();
        }

        static void fixMartialExoticWeapons()
        {
            var bastard_sword_type = library.Get<BlueprintWeaponType>("d2fe2c5516b56f04da1d5ea51ae3ddfe");
            Helpers.SetField(bastard_sword_type, "m_IsTwoHanded", true);
            WeaponVisualParameters bs_visuals = Helpers.GetField<WeaponVisualParameters>(bastard_sword_type, "m_VisualParameters");
            Helpers.SetField(bs_visuals, "m_WeaponAnimationStyle", WeaponAnimationStyle.SlashingTwoHanded);
            fixFullWeaponCategory(WeaponCategory.BastardSword, WeaponCategory.Greatsword,
                                  Helpers.Create<HoldingItemsMechanics.CanHoldIn1Hand>(c =>
                                  {
                                      c.category = WeaponCategory.BastardSword;
                                      c.require_full_proficiency = true;
                                      c.except_types = new BlueprintWeaponType[]
                                      {
                                        library.Get<BlueprintWeaponType>("1e460a2dc830cf546939b2723aa875e7"), //oversized bastard sword
                                        library.Get<BlueprintWeaponType>("58dcff8d1dd5d094eb345e8eeb2e4626"), //oversized bastard sword no penalty
                                      };
                                  }));

            var dwarven_waraxe_type = library.Get<BlueprintWeaponType>("a6925f5f897801449a648d865637e5a0");
            Helpers.SetField(dwarven_waraxe_type, "m_IsTwoHanded", true);
            WeaponVisualParameters dw_visuals = Helpers.GetField<WeaponVisualParameters>(dwarven_waraxe_type, "m_VisualParameters");
            Helpers.SetField(dw_visuals, "m_WeaponAnimationStyle", WeaponAnimationStyle.SlashingTwoHanded);
            fixFullWeaponCategory(WeaponCategory.DwarvenWaraxe, WeaponCategory.Battleaxe,
                                  Helpers.Create<HoldingItemsMechanics.CanHoldIn1Hand>(c => { c.category = WeaponCategory.DwarvenWaraxe; c.require_full_proficiency = true;}));




            var bastard_sword_proficiency = library.Get<BlueprintFeature>("57299a78b2256604dadf1ab9a42e2873");
            bastard_sword_proficiency.SetDescription(bastard_sword_proficiency.Description + "\nA character can use a bastard sword two-handed as a martial weapon, without exotic weapon proficiency.");

            var dwarven_waraxe_proficiency = library.Get<BlueprintFeature>("bd0d7feca087d2247b12965c1467790c");
            dwarven_waraxe_proficiency.SetDescription(dwarven_waraxe_proficiency.Description + "\nA character can use a swarven waraxe two-handed as a martial weapon, without exotic weapon proficiency.");

            var duelling_sword_proficiency = library.Get<BlueprintFeature>("9c37279588fd9e34e9c4cb234857492c");
            fixFullWeaponCategory(WeaponCategory.DuelingSword, WeaponCategory.Longsword,
                                  Helpers.Create<NewMechanics.ConsiderAsFinessable>(c => c.category = WeaponCategory.DuelingSword));          
            duelling_sword_proficiency.SetDescription(duelling_sword_proficiency.Description + "\nAn dueling sword can be used as a martial weapon (in which case it functions as a longsword), but if you have the feat Exotic Weapon Proficiency (dueling sword), you can use the Weapon Finesse feat to apply your Dexterity modifier instead of your Strength modifier to attack rolls with a dueling sword sized for you, even though it isn’t a light weapon.");

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


        static void fixFullWeaponCategory(WeaponCategory base_category, WeaponCategory like_category, params BlueprintComponent[] components)
        {
            var feats = library.GetAllBlueprints().OfType<BlueprintFeature>();
            foreach (var f in feats)
            {
                bool found = false;
                BlueprintRace race = null;
                foreach (var c in f.GetComponents<AddProficiencies>().ToArray())
                {
                    if (c.WeaponProficiencies.Contains(base_category))
                    {
                        found = true;
                        race = c.RaceRestriction;
                        break;
                    }

                    if (c.WeaponProficiencies.Contains(like_category) && !c.WeaponProficiencies.Contains(base_category))
                    {
                        c.WeaponProficiencies = c.WeaponProficiencies.AddToArray(base_category);
                    }
                }

                if (found && !f.GetComponents<FullProficiency>().Any(fp => fp.category == base_category))
                {
                    f.AddComponents(components);
                    f.AddComponent(Helpers.Create<FullProficiency>(fp => { fp.category = base_category; fp.race = race; }));
                }

                found = false;
                foreach (var c in f.GetComponents<PrerequisiteProficiency>().ToArray())
                {
                    if (c.WeaponProficiencies.Contains(base_category))
                    {
                        c.WeaponProficiencies = c.WeaponProficiencies.RemoveFromArray(base_category);
                        found = true;
                    }
                    if (c.WeaponProficiencies.Empty() && c.ArmorProficiencies.Empty())
                    {
                        f.RemoveComponent(c);
                    }
                }

                if (found && !f.GetComponents<PrerequisiteFullExoticProficiency>().Any(fp => fp.category == base_category && !fp.not))
                {
                    f.AddComponent(Helpers.Create<PrerequisiteFullExoticProficiency>(fp => fp.category = base_category));
                }

                found = false;
                foreach (var c in f.GetComponents<PrerequisiteNotProficient>().ToArray())
                {
                    if (c.WeaponProficiencies.Contains(base_category))
                    {
                        c.WeaponProficiencies = c.WeaponProficiencies.RemoveFromArray(base_category);
                        found = true;
                        c.Group = Prerequisite.GroupType.Any;
                    }
                    if (c.WeaponProficiencies.Empty() && c.ArmorProficiencies.Empty())
                    {
                        f.RemoveComponent(c);
                    }
                }

                if (found && !f.GetComponents<PrerequisiteFullExoticProficiency>().Any(fp => fp.category == base_category && fp.not))
                {
                    f.AddComponent(Helpers.Create<PrerequisiteFullExoticProficiency>(fp => { fp.category = base_category; fp.not = true; fp.Group = Prerequisite.GroupType.Any; }));
                }
            }
        }


        public class PrerequisiteFullExoticProficiency : Prerequisite
        {
            public WeaponCategory category;
            public bool not;

            public override bool Check(
              FeatureSelectionState selectionState,
              UnitDescriptor unit,
              LevelUpState state)
            {
                return (unit.Get<UnitPartFullProficiency>()?.hasFullProficiency(category)).GetValueOrDefault() != not;
            }

            public override string GetUIText()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(string.Format("{0}:\n", not ? UIStrings.Instance.Tooltips.NoProficiencies : UIStrings.Instance.Tooltips.HasProficiencies));

                stringBuilder.Append(LocalizedTexts.Instance.Stats.GetText(this.category));
                stringBuilder.Append(" (Exotic)\n");

                return stringBuilder.ToString();
            }
        }
    }



    
}
