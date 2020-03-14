using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class Eidolon
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintCharacterClass eidolon_class;
        static public bool test_mode = false;


        static public BlueprintUnit fire_elemental_eidolon;


        static public BlueprintProgression eidolon_progression;


        static public void create()
        {
            createEidolonClass();

            createFireElemental();
        }


        static void createFireElemental()
        {
            var fx_buff = Helpers.CreateBuff("FireElementalEidolonFxBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             Common.createPrefabLink("f5eaec10b715dbb46a78890db41fa6a0"));
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);

            var fx_feature = Helpers.CreateFeature("FireElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Common.createAuraFeatureComponent(fx_buff)/*,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitViewOnApply>(r => r.prefab = Common.createUnitViewLink("7cc1c50366f08814eb5a5e7c47c71a2a"))*/);
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var fire_elemental = library.Get<BlueprintUnit>("37b3eb7ca48264247b3247c732007aef");
            fire_elemental_eidolon = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "FireElementalEidolonUnit", "");
            fire_elemental_eidolon.Color = fire_elemental.Color;

           // var dryad = ResourcesLibrary.TryGetResource<UnitEntityView>("7cc1c50366f08814eb5a5e7c47c71a2a");
            
           // var octavia = ResourcesLibrary.TryGetResource<UnitEntityView>("77db32ef997a1274485750825a17c1aa");

           // GetCopyOf(dryad.gameObject.AddComponent<Character>(), octavia.GetComponentInChildren<Character>());

            //fire_elemental_eidolon.Prefab = Common.createUnitViewLink("7cc1c50366f08814eb5a5e7c47c71a2a");
            fire_elemental_eidolon.Visual = fire_elemental.Visual;
            fire_elemental_eidolon.LocalizedName = fire_elemental_eidolon.LocalizedName.CreateCopy();
            fire_elemental_eidolon.LocalizedName.String = Helpers.CreateString(fire_elemental_eidolon.name + ".Name", "Fire Elemental Eidolon");

            fire_elemental_eidolon.Strength = 16;
            fire_elemental_eidolon.Dexterity = 12;
            fire_elemental_eidolon.Constitution = 13;
            fire_elemental_eidolon.Intelligence = 7;
            fire_elemental_eidolon.Wisdom = 10;
            fire_elemental_eidolon.Charisma = 11;
            fire_elemental_eidolon.Speed = 30.Feet();
            fire_elemental_eidolon.AddFacts = new BlueprintUnitFact[] { natural_armor2, library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629"), fx_feature}; // { natural_armor2, fx_feature };
            fire_elemental_eidolon.Body = fire_elemental_eidolon.Body.CloneObject();
            fire_elemental_eidolon.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("5ea80d97dcfc81f46a1b9b2f256340f2"); //slam 1d8
            fire_elemental_eidolon.Body.PrimaryHand = null;
            fire_elemental_eidolon.Body.SecondaryHand = null;
            fire_elemental_eidolon.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            fire_elemental_eidolon.ReplaceComponent<AddClassLevels>(a => 
                                                                    { a.Archetypes = new BlueprintArchetype[0];
                                                                      a.CharacterClass = eidolon_class;
                                                                      a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
                                                                    });
            fire_elemental_eidolon.AddComponent(Helpers.Create<EidolonComponent>());


            Helpers.SetField(fire_elemental_eidolon, "m_Portrait", Helpers.createPortrait("FireElemental", ""));

            BlueprintFeature fire_elemental_eidolon_feature = library.CopyAndAdd<BlueprintFeature>("126712ef923ab204983d6f107629c895", "FireElementalEidolonProgression", "");
            fire_elemental_eidolon_feature.ReplaceComponent<AddPet>(a => a.Pet = fire_elemental_eidolon);
            fire_elemental_eidolon_feature.SetNameDescriptionIcon("Fire Elemental", "Test", Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"));
            Hunter.hunter_animal_companion.AllFeatures = Hunter.hunter_animal_companion.AllFeatures.AddToArray(fire_elemental_eidolon_feature);
        }


        static void createEidolonClass()
        {
            Main.logger.Log("Eidolon class test mode: " + test_mode.ToString());
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var fighter_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var animal_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");

            eidolon_class = Helpers.Create<BlueprintCharacterClass>();
            eidolon_class.name = "EidolonClass";
            library.AddAsset(eidolon_class, "e3b3ad6decb14cdba2e7e14982d90035");


            eidolon_class.LocalizedName = Helpers.CreateString("Eidolon.Name", "Eidolon");
            eidolon_class.LocalizedDescription = Helpers.CreateString("Eidolon.Description",
                "The eidolon takes a form shaped by the summoner’s desires. The eidolon’s Hit Dice, saving throws, skills, feats, and abilities are tied to the summoner’s class level and increase as the summoner gains levels. In addition, each eidolon gains a pool of evolution points based on the summoner’s class level that can be used to give the eidolon different abilities and powers. Whenever the summoner gains a level, he must decide how these points are spent, and they are set until he gains another level of summoner.\n"
                + "The eidolon’s physical appearance is up to the summoner, but it always appears as some sort of fantastical creature appropriate to its subtype. This control is not fine enough to make the eidolon appear like a specific creature."
                );
            eidolon_class.m_Icon = druid_class.Icon;
            eidolon_class.SkillPoints = druid_class.SkillPoints;
            eidolon_class.HitDie = DiceType.D10;
            eidolon_class.BaseAttackBonus = fighter_class.BaseAttackBonus;
            eidolon_class.FortitudeSave = druid_class.ReflexSave;
            eidolon_class.ReflexSave = druid_class.ReflexSave;
            eidolon_class.WillSave = druid_class.WillSave;
            eidolon_class.Spellbook = null;
            eidolon_class.ClassSkills = new StatType[] { StatType.SkillPersuasion, StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
            eidolon_class.IsDivineCaster = false;
            eidolon_class.IsArcaneCaster = false;
            eidolon_class.StartingGold = fighter_class.StartingGold;
            eidolon_class.PrimaryColor = fighter_class.PrimaryColor;
            eidolon_class.SecondaryColor = fighter_class.SecondaryColor;
            eidolon_class.RecommendedAttributes = new StatType[0];
            eidolon_class.NotRecommendedAttributes = new StatType[0];
            eidolon_class.EquipmentEntities = animal_class.EquipmentEntities;
            eidolon_class.MaleEquipmentEntities = animal_class.MaleEquipmentEntities;
            eidolon_class.FemaleEquipmentEntities = animal_class.FemaleEquipmentEntities;
            eidolon_class.ComponentsArray = new BlueprintComponent[] { Helpers.PrerequisiteClassLevel(eidolon_class, 1)};
            eidolon_class.StartingItems = animal_class.StartingItems;
            createEidolonProgression();
            eidolon_class.Progression = eidolon_progression;

            eidolon_class.Archetypes = new BlueprintArchetype[] {};
            Helpers.RegisterClass(eidolon_class);
        }


        static void createEidolonProgression()
        {
            //devotion
            //evasion
            //natural armor
            //str/dex increase
            //improved evasion

            var devotion = library.CopyAndAdd<BlueprintFeature>("226f939b7dfd47b4697ec52f79799012", "EidolonDevotionFeature", "");
            devotion.SetDescription("An eidolon gains a +4 morale bonus on Will saves against enchantment spells and effects.");
            var evasion = library.CopyAndAdd<BlueprintFeature>("815bec596247f9947abca891ef7f2ca8", "EidolonEvasionFeature", "");
            evasion.SetDescription("If the eidolon is subjected to an attack that normally allows a Reflex save for half damage, it takes no damage if it succeeds at its saving throw.");
            var improved_evasion = library.CopyAndAdd<BlueprintFeature>("bcb37922402e40d4684e7fb7e001d110", "EidolonImprovedEvasionFeature", "");
            improved_evasion.SetDescription("When subjected to an attack that allows a Reflex saving throw for half damage, an eidolon takes no damage if it succeeds at its saving throw and only half damage if it fails.");

            var natural_armor = library.CopyAndAdd<BlueprintFeature>("0d20d88abb7c33a47902bd99019f2ed1", "EidolonNaturalArmorFeature", "");
            natural_armor.SetNameDescription("Armor Bonus",
                                             "Eidolon receives bonuses to their natural armor. An eidolon cannot wear armor of any kind, as the armor interferes with the summoner’s connection to the eidolon.");
            var str_dex_bonus = library.CopyAndAdd<BlueprintFeature>("0c80276018694f24fbaf59ec7b841f2b", "EidolonStrDexIncreaseFeature", "");
            str_dex_bonus.SetNameDescription("Physical Prowess", "Eidolon receives +1 bonus to their Strength and Dexterity.");

            eidolon_progression = Helpers.CreateProgression("EidolonProgression",
                                                   eidolon_class.Name,
                                                   eidolon_class.Description,
                                                   "",
                                                   eidolon_class.Icon,
                                                   FeatureGroup.None);
            eidolon_progression.Classes = new BlueprintCharacterClass[] { eidolon_class };

            eidolon_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                       library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa"),  // touch calculate feature
                                                                                       library.Get<BlueprintFeature>("0aeba56961779e54a8a0f6dedef081ee")), //inside the storm
                                                                    Helpers.LevelEntry(2, natural_armor, evasion, str_dex_bonus),
                                                                    Helpers.LevelEntry(3),
                                                                    Helpers.LevelEntry(4, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(5, devotion),
                                                                    Helpers.LevelEntry(6, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(7),
                                                                    Helpers.LevelEntry(8, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(9, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(10),
                                                                    Helpers.LevelEntry(11, improved_evasion),
                                                                    Helpers.LevelEntry(12, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(13, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(14),
                                                                    Helpers.LevelEntry(15, natural_armor, str_dex_bonus),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17),
                                                                    Helpers.LevelEntry(18),
                                                                    Helpers.LevelEntry(19),
                                                                    Helpers.LevelEntry(20)
                                                                    };
            eidolon_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(evasion, devotion, improved_evasion),
                                                           Helpers.CreateUIGroup(natural_armor),
                                                           Helpers.CreateUIGroup(str_dex_bonus),
                                                        };
        }




        public class EidolonComponent : BlueprintComponent
        {

        }
        public static T GetCopyOf<T>(Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            /*PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }*/
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }
    }




}
