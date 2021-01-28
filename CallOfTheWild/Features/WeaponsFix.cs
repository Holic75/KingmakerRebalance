using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Kingmaker.View.Animation;
using Newtonsoft.Json;
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
        public class UnitPartFullProficiency : AdditiveUnitPart
        {
        }
    }

    public class WeaponsFix
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

            public bool hasBuff(Fact fact)
            {
                return buffs.Contains(fact);
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


            public override void PostLoad()
            {
                base.PostLoad();
                if (!this.Owner.Ensure<UnitPartFullProficiency>().hasBuff(this.Fact))
                {
                    this.OnFactActivate();
                }
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


        [AllowedOn(typeof(BlueprintParametrizedFeature))]
        public class AddFullWeaponProficiencyWeaponCategory : ParametrizedFeatureComponent
        {
            static public Dictionary<WeaponCategory, BlueprintFeature> category_feature_map;
            [JsonProperty]
            private Fact m_applied_fact = null;

            public override void OnFactActivate()
            {
                if (this.Param.WeaponCategory.HasValue && category_feature_map.ContainsKey(this.Param.WeaponCategory.Value)
                    && !this.Owner.HasFact(category_feature_map[this.Param.WeaponCategory.Value]))
                {
                    m_applied_fact = this.Owner.AddFact(category_feature_map[this.Param.WeaponCategory.Value], null, null);
                }
            }

            public override void OnFactDeactivate()
            {
                if (m_applied_fact != null)
                {
                    this.Owner.RemoveFact(m_applied_fact);
                }
            }

            public override void PostLoad()
            {
                base.PostLoad();
                this.OnFactActivate();
            }
        }




        static LibraryScriptableObject library => Main.library;


        static internal void load()
        {
            fixMartialExoticWeapons();
            addWeaponProperties();
        }


        static void addWeaponProperties()
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
            var sunder_enchant = Common.createWeaponEnchantment("SunderEnchantment", sunder.Name, sunder.Description, "", "", "", 0, null,
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
            var trip_enchant = Common.createWeaponEnchantment("TripEnchantment", trip.Name, trip.Description, "", "", "", 0, null,
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

            var disarm_enchant = Common.createWeaponEnchantment("DisarmEnchantment", disarm.Name, disarm.Description, "", "", "", 0, null,
                   Helpers.Create<AddUnitFeatureEquipment>(a => a.Feature = disarm));

            WeaponCategory[] disarm_weapons = new WeaponCategory[]
            {
                WeaponCategory.Flail,
                WeaponCategory.HeavyFlail,
                WeaponCategory.Nunchaku,
                WeaponCategory.Sai,
            };
            WeaponCategory[] sunder_weapons = new WeaponCategory[]
            {
            };
            WeaponCategory[] trip_weapons = new WeaponCategory[]
            {
                WeaponCategory.Sickle,
                WeaponCategory.Flail,
                WeaponCategory.HeavyFlail,
                WeaponCategory.Scythe,
                WeaponCategory.Kama,
                WeaponCategory.Fauchard,
                WeaponCategory.HookedHammer
            };

            var weapon_types = library.GetAllBlueprints().OfType<BlueprintWeaponType>();
            foreach (var wt in weapon_types)
            {
                if (disarm_weapons.Contains(wt.Category))
                {
                    Common.addEnchantment(wt, disarm_enchant);
                }
                if (sunder_weapons.Contains(wt.Category))
                {
                    Common.addEnchantment(wt, sunder_enchant);
                }
                if (trip_weapons.Contains(wt.Category))
                {
                    Common.addEnchantment(wt, trip_enchant);
                }
            }
        }

        static void fixMartialExoticWeapons()
        {
            var oversized_bastard_sword_types = new BlueprintWeaponType[]
                                              {
                                                library.Get<BlueprintWeaponType>("1e460a2dc830cf546939b2723aa875e7"), //oversized bastard sword
                                                library.Get<BlueprintWeaponType>("58dcff8d1dd5d094eb345e8eeb2e4626"), //oversized bastard sword no penalty
                                              };
            var bastard_sword_type = library.Get<BlueprintWeaponType>("d2fe2c5516b56f04da1d5ea51ae3ddfe");
            Helpers.SetField(bastard_sword_type, "m_IsTwoHanded", true);
            WeaponVisualParameters bs_visuals = Helpers.GetField<WeaponVisualParameters>(bastard_sword_type, "m_VisualParameters");
            Helpers.SetField(bs_visuals, "m_WeaponAnimationStyle", WeaponAnimationStyle.SlashingTwoHanded);
            fixFullWeaponCategory(WeaponCategory.BastardSword, WeaponCategory.Greatsword,
                                  Helpers.Create<HoldingItemsMechanics.CanHoldIn1Hand>(c =>
                                  {
                                      c.category = WeaponCategory.BastardSword;
                                      c.require_full_proficiency = true;
                                      c.except_types = oversized_bastard_sword_types;
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
            dwarven_waraxe_proficiency.SetDescription(dwarven_waraxe_proficiency.Description + "\nA character can use a dwarven waraxe two-handed as a martial weapon, without exotic weapon proficiency.");

            var duelling_sword_proficiency = library.Get<BlueprintFeature>("9c37279588fd9e34e9c4cb234857492c");
            fixFullWeaponCategory(WeaponCategory.DuelingSword, WeaponCategory.Longsword,
                                  Helpers.Create<NewMechanics.ConsiderAsFinessable>(c => c.category = WeaponCategory.DuelingSword));          
            duelling_sword_proficiency.SetDescription(duelling_sword_proficiency.Description + "\nA dueling sword can be used as a martial weapon (in which case it functions as a longsword), but if you have the feat Exotic Weapon Proficiency (dueling sword), you can use the Weapon Finesse feat to apply your Dexterity modifier instead of your Strength modifier to attack rolls with a dueling sword sized for you, even though it isn’t a light weapon.");

            var estoc_type = library.Get<BlueprintWeaponType>("d516765b3c2904e4a939749526a52a9a");
            Helpers.SetField(estoc_type, "m_IsTwoHanded", true);

            var estoc_proficiency = library.Get<BlueprintFeature>("9dc64f0b9161a354c9471a631318e16c");
            fixFullWeaponCategory(WeaponCategory.Estoc, WeaponCategory.Greatsword,
                                  Helpers.Create<HoldingItemsMechanics.CanHoldIn1Hand>(c => { c.category = WeaponCategory.Estoc; c.require_full_proficiency = true; }),
                                  Helpers.Create<NewMechanics.ConsiderAsFinessable>(c => c.category = WeaponCategory.Estoc));
            estoc_proficiency.SetDescription(estoc_proficiency.Description + "\nLike the bastard sword, an estoc requires special training to use it one handed, but it can also be wielded as a two-handed martial weapon.\nWhen you wield an estoc with one hand, treat it as a one-handed weapon; when you wield an estoc with two hands, treat it as a two-handed weapon. If you can use the estoc proficiently with one hand, you can also use the Weapon Finesse feat to apply your Dexterity modifier instead of your Strength modifier on attack rolls when wielding an estoc sized for you with one or two hands, even though it isn’t a light weapon.");

            //remove duelling sword and estoc from list of finessable weapons
            var info = typeof(WeaponCategoryExtension).GetField("Data", BindingFlags.NonPublic | BindingFlags.Static);
            var data = (Array)info.GetValue(null);
            
            foreach (var dd in data)
            {
                var category = Helpers.GetField<WeaponCategory>(dd, "Category");
                if (category == WeaponCategory.DuelingSword || category == WeaponCategory.Estoc)
                {
                    var subcategories = Helpers.GetField<WeaponSubCategory[]>(dd, "SubCategories");
                    subcategories = subcategories.RemoveFromArray(WeaponSubCategory.Finessable);
                    Helpers.SetField(dd, "SubCategories", subcategories);
                    break;
                }
            }

            //fix kensai choosen weapon to give proficiency
            var kensai_choosen_weapon = library.Get<BlueprintParametrizedFeature>("c0b4ec0175e3ff940a45fc21f318a39a");
            kensai_choosen_weapon.AddComponent(Helpers.Create<AddFullWeaponProficiencyWeaponCategory>());
            AddFullWeaponProficiencyWeaponCategory.category_feature_map = new Dictionary<WeaponCategory, BlueprintFeature>()
                {
                    {WeaponCategory.BastardSword, library.Get<BlueprintFeature>("57299a78b2256604dadf1ab9a42e2873") },
                    {WeaponCategory.DwarvenWaraxe, library.Get<BlueprintFeature>("bd0d7feca087d2247b12965c1467790c") },
                    {WeaponCategory.DuelingSword, library.Get<BlueprintFeature>("9c37279588fd9e34e9c4cb234857492c") },
                    {WeaponCategory.Estoc, library.Get<BlueprintFeature>("9dc64f0b9161a354c9471a631318e16c") }
                };


            //fix units
            var hendrick_bandit = library.Get<BlueprintUnit>("5d7122078d52ec34aa5cc34df345ed51");
            hendrick_bandit.Body.PrimaryHand = library.Get<BlueprintItemWeapon>("0ce4b3f4a3d70fa45851313a7f3d233f"); //battle axe +1 instead of bastard sword +1 since he does not have proficiency

            //fix all oversized bastard swords to require exotic weapon proficiency
            var oversized_bastard_swords = library.GetAllBlueprints().OfType<BlueprintItemWeapon>().Where(w => oversized_bastard_sword_types.Contains(w.Type)).ToArray();
            foreach (var obs in oversized_bastard_swords)
            {
                obs.AddComponent(Helpers.Create<NewMechanics.EquipmentRestrictionFeature>(e => e.feature = bastard_sword_proficiency));
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
