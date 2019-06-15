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

namespace KingmakerRebalance
{
public class Rebalance
{
    public static void fixAnimalCompanion()
    {
        //animal companion rebalance
        //set natural ac as per pnp
        var natural_armor = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0d20d88abb7c33a47902bd99019f2ed1");
        var natural_armor_value = natural_armor.GetComponent<AddStatBonus>();
        natural_armor_value.Value = 2;
        //set stat bonus to str and dex as per pnp, set con to the same values to compensate for bonus ability points
        var stat_bonuses = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0c80276018694f24fbaf59ec7b841f2b");
        var stat_bonuses_value = stat_bonuses.GetComponents<AddStatBonus>();
        foreach (var stat_bonus_value in stat_bonuses_value)
        {
            stat_bonus_value.Value = 1;
        }
        //remove enchanced attacks
        var enchanced_attacks_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("71d6955fe81a9a34b97390fef1104362");
        enchanced_attacks_feature.HideInCharacterSheetAndLevelUp = true;
        var enchanced_attack_bonus = enchanced_attacks_feature.GetComponent<AllAttacksEnhancement>();
        enchanced_attack_bonus.Bonus = 0;
        var enchanced_attack_saves_bonus = enchanced_attacks_feature.GetComponent<BuffAllSavesBonus>();
        enchanced_attack_saves_bonus.Value = 0;

        //fix progression
        var animal_companion_progression = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("9f8a232fbe435a9458bf64c3024d7bee");
        var animal_companion_level_entires = animal_companion_progression.AddFeatures;
        foreach (var lvl_entry in animal_companion_level_entires)
        {
            lvl_entry.Features.Remove(enchanced_attacks_feature);
        }
    }


    public static void fixLegendaryProportionsAC()
    {
        //fix natural armor bonus for animal growth and legendary proportions
        var buff_ids = new string[] {"3fca5d38053677044a7ffd9a872d3a0a", //animal growth
                                            "4ce640f9800d444418779a214598d0a3" //legendary proportions
                                        };
        foreach (var buff_id in buff_ids)
        {
            var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(buff_id);
            var boni = buff.GetComponents<AddContextStatBonus>();
            foreach (var bonus in boni)
            {
                if (bonus.Stat == Kingmaker.EntitySystem.Stats.StatType.AC)
                {
                    bonus.Descriptor = Kingmaker.Enums.ModifierDescriptor.NaturalArmor;
                }
            }
        }
    }


    public static void removeJudgement19FormSHandMS()
    {
       BlueprintArchetype[] inquisitor_archetypes = new BlueprintArchetype[2] {ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012"),//sacred huntsmaster
                                                                               ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("cdaabf4b146c9ba42ab7d05abe3b48c4")//monster tactician
                                                                              };
       Kingmaker.Blueprints.Classes.LevelEntry remove_entry = new Kingmaker.Blueprints.Classes.LevelEntry();
       remove_entry.Level = 19;
       remove_entry.Features.Add(ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ee50875819478774b8968701893b52f5"));//judgement additional use
       foreach (var inquisitor_archetype in inquisitor_archetypes)
       {
           inquisitor_archetype.RemoveFeatures = inquisitor_archetype.RemoveFeatures.AddToArray(remove_entry);
       }
    }


    public static void fixSkillPoints()
    {
        //update skillpoints
        var class_skill_map = new List<KeyValuePair<string, int>>();
        class_skill_map.Add(new KeyValuePair<string, int>("0937bec61c0dabc468428f496580c721", 2));//alchemist
        class_skill_map.Add(new KeyValuePair<string, int>("9c935a076d4fe4d4999fd48d853e3cf3", 2));//arcane trickster
        class_skill_map.Add(new KeyValuePair<string, int>("f7d7eb166b3dd594fb330d085df41853", 2));//barbarian
        class_skill_map.Add(new KeyValuePair<string, int>("772c83a25e2268e448e841dcd548235f", 3));//bard
        class_skill_map.Add(new KeyValuePair<string, int>("67819271767a9dd4fbfd4ae700befea0", 1));//cleric
        class_skill_map.Add(new KeyValuePair<string, int>("72051275b1dbb2d42ba9118237794f7c", 1));//dragon disciple                   
        class_skill_map.Add(new KeyValuePair<string, int>("610d836f3a3a9ed42a4349b62f002e96", 2));//druid
        class_skill_map.Add(new KeyValuePair<string, int>("4e0ea99612ae87a499c7fb0588e31828", 2));//duelist
        class_skill_map.Add(new KeyValuePair<string, int>("de52b73972f0ed74c87f8f6a8e20b542", 1));//eldrich knight
        class_skill_map.Add(new KeyValuePair<string, int>("f5b8c63b141b2f44cbb8c2d7579c34f5", 1));//eldrich scion
        class_skill_map.Add(new KeyValuePair<string, int>("48ac8db94d5de7645906c7d0ad3bcfbd", 1));//fighter
        class_skill_map.Add(new KeyValuePair<string, int>("f1a70d9e1b0b41e49874e1fa9052a1ce", 3));//inquisitor
        class_skill_map.Add(new KeyValuePair<string, int>("42a455d9ec1ad924d889272429eb8391", 2));//kineticist
        class_skill_map.Add(new KeyValuePair<string, int>("45a4607686d96a1498891b3286121780", 1));//magus
        class_skill_map.Add(new KeyValuePair<string, int>("e8f21e5b58e0569468e420ebea456124", 2));//monk
        class_skill_map.Add(new KeyValuePair<string, int>("0920ea7e4fd7a404282e3d8b0ac41838", 1));//mystic theurge
        class_skill_map.Add(new KeyValuePair<string, int>("bfa11238e7ae3544bbeb4d0b92e897ec", 1));//paladin
        class_skill_map.Add(new KeyValuePair<string, int>("cda0615668a6df14eb36ba19ee881af6", 3));//ranger
        class_skill_map.Add(new KeyValuePair<string, int>("299aa766dee3cbf4790da4efb8c72484", 4));//rogue
        class_skill_map.Add(new KeyValuePair<string, int>("b3a505fb61437dc4097f43c3f8f9a4cf", 1));//sorcerer
        class_skill_map.Add(new KeyValuePair<string, int>("d5917881586ff1d4d96d5b7cebda9464", 1));//stalwart defender
        class_skill_map.Add(new KeyValuePair<string, int>("90e4d7da3ccd1a8478411e07e91d5750", 2));//aldori swordlord
        class_skill_map.Add(new KeyValuePair<string, int>("ba34257984f4c41408ce1dc2004e342e", 1));//wizard
        foreach (var class_skill in class_skill_map)
        {
            var current_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>(class_skill.Key);
            current_class.SkillPoints = class_skill.Value;
        }
    }

    public static void fixCompanions()
    {
        //change stats of certain companions
        //Valerie 
        var valerie_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("54be53f0b35bf3c4592a97ae335fe765");
        valerie_companion.Strength = 14;//+2
        valerie_companion.Dexterity = 15;
        valerie_companion.Constitution = 14;
        valerie_companion.Intelligence = 13;
        valerie_companion.Wisdom = 8;
        valerie_companion.Charisma = 15;
        var valerie1_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("912444657701e2d4ab2634c3d1e130ad");
        var valerie_class_level = valerie1_feature.GetComponent<AddClassLevels>();
        valerie_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Strength;
        valerie_class_level.Selections[1].Features[0] = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ac57069b6bf8c904086171683992a92a"); //shield focus instead of bastard sword
        valerie_companion.Body.PrimaryHand = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("571c56d11dafbb04094cbaae659974b5");//longsword
        valerie_companion.Body.Armor = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Armors.BlueprintItemArmor>("9809987cc12d94545a64ff20e6fdb216");//breastplate
                                                                                                                                                                    //change amiri stats
        var amiri_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b3f29faef0a82b941af04f08ceb47fa2");
        amiri_companion.Strength = 16;//+2
        amiri_companion.Dexterity = 14;
        amiri_companion.Constitution = 16;
        amiri_companion.Intelligence = 10;
        amiri_companion.Wisdom = 12;
        amiri_companion.Charisma = 8;
        var amiri1_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("df943986ee329e84a94360f2398ae6e6");
        var amiri_class_level = amiri1_feature.GetComponent<AddClassLevels>();
        amiri_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Strength;
        //change tristian stats
        var tristian_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f6c23e93512e1b54dba11560446a9e02");
        tristian_companion.Strength = 8;
        tristian_companion.Dexterity = 16;
        tristian_companion.Constitution = 12;
        tristian_companion.Intelligence = 10;
        tristian_companion.Wisdom = 16;
        tristian_companion.Charisma = 14;
        var tristian_level = tristian_companion.GetComponent<AddClassLevels>();
        tristian_level.Selections[2].Features = new BlueprintFeature[2];
        tristian_level.Selections[2].Features[0] = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35");//fire as primary
        tristian_level.Selections[2].Features[1] = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("243ab3e7a86d30243bdfe79c83e6adb4");//good as secondary
        tristian_level.Selections[3].Features[0] = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("243ab3e7a86d30243bdfe79c83e6adb4");//good as secondary
        tristian_level.Selections[4].Features[2] = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab");//point blank shot instead of extend spell
        //change harrim stats
        var harrim_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("aab03d0ab5262da498b32daa6a99b507");
        harrim_companion.Strength = 16;
        var harrim_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("8910febae2a7b9f4ba5eca4dde1e9649");
        var harrim_class_level = harrim_feature.GetComponent<AddClassLevels>();
        harrim_class_level.Selections[3].Features[0] = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("9ebe166b9b901c746b1858029f13a2c5"); //madness domain instead of chaos

        //change linzi
        var linzi_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("77c11edb92ce0fd408ad96b40fd27121");
        linzi_companion.Dexterity = 16;
        //change octavia
        var octavia_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f9161aa0b3f519c47acbce01f53ee217");
        octavia_companion.Dexterity = 16;
        octavia_companion.Intelligence = 16;
        octavia_companion.Constitution = 10;

        //change regognar
        var regognar_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b090918d7e9010a45b96465de7a104c3");
        regognar_companion.Dexterity = 12;
        //change ekun
        var ekun_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("d5bc1d94cd3e5be4bbc03f3366f67afc");
        ekun_companion.Strength = 14;
        ekun_companion.Constitution = 10;
        ekun_companion.Dexterity = 18;
        ekun_companion.Wisdom = 14;
        ekun_companion.Charisma = 8;
        var ekun_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0bc6dc9b6648a744899752508addae8c");
        var ekun_class_level = ekun_feature.GetComponent<AddClassLevels>();
        ekun_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Dexterity;

        //change jubilost
        var jubilost_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("3f5777b51d301524c9b912812955ee1e");
        jubilost_companion.Dexterity = 16;
        //change nok-nok
        
        var noknok_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f9417988783876044b76f918f8636455");
        noknok_companion.Constitution = 14;
        noknok_companion.Wisdom = 10;
    }


    public static void fixDomains()
    {
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            var domain_classes = new BlueprintCharacterClass[] { cleric_class, inquisitor_class };
            //protection domain
            var protection_bonus_context_rank = Helpers.CreateContextRankConfig(progression: ContextRankProgression.OnePlusDivStep,
                                                                                             startLevel: 1,
                                                                                             stepLevel: 5,
                                                                                             min: 1,
                                                                                             classes: domain_classes
                                                                                             );
            var protection_domain_remove_save_bonus = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>("74a4fb45f23705d4db2784d16eb93138"); //resistant touch self
            protection_domain_remove_save_bonus.AddComponent(protection_bonus_context_rank);

            var protection_domain_save_bonus = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>("2ddb4cfc3cfd04c46a66c6cd26df1c06"); //resitant touch bonus
            protection_domain_save_bonus.ReplaceComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>(protection_bonus_context_rank);



        }


        public static void fixBarbarianRageAC()
        {
            var rage = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            var components = rage.GetComponents<Kingmaker.UnitLogic.FactLogic.AddContextStatBonus>();
            foreach (var c in components)
            {
                if (c.Stat == Kingmaker.EntitySystem.Stats.StatType.AC)
                {
                    c.Value = Common.createSimpleContextValue(2);
                }
            }
        }


        public static void fixMagicVestment()
        {
            fixMagicVestmentArmor();
            fixMagicVestmentShield();
        }

        static void fixMagicVestmentArmor()
        {
            BlueprintArmorEnchantment[] enchantments = {Main.library.Get< BlueprintArmorEnchantment>("a9ea95c5e02f9b7468447bc1010fe152"), //+1
                                                              Main.library.Get< BlueprintArmorEnchantment>("758b77a97640fd747abf149f5bf538d0"), //+2
                                                              Main.library.Get< BlueprintArmorEnchantment>("9448d3026111d6d49b31fc85e7f3745a"), //+3 
                                                              Main.library.Get< BlueprintArmorEnchantment>("eaeb89df5be2b784c96181552414ae5a"), //+4
                                                              Main.library.Get< BlueprintArmorEnchantment>("6628f9d77fd07b54c911cd8930c0d531")  //+5
                                                             };

            BlueprintArmorEnchantment[] temporary_enchantments = new BlueprintArmorEnchantment[enchantments.Length];

            for (int i = 0; i < enchantments.Length; i++)
            {
                temporary_enchantments[i] = Main.library.CopyAndAdd<BlueprintArmorEnchantment>(enchantments[i].AssetGuid, $"TemporaryArmorEnchantment{i + 1}", "");
                Helpers.SetField(temporary_enchantments[i], "m_EnchantName", Helpers.CreateString($"TemporaryArmorEnchantment{i + 1}.Name", $"Temporary Enhancement + {i + 1}"));
            }

            var magic_vestement_armor_buff = Main.library.Get<BlueprintBuff>("9e265139cf6c07c4fb8298cb8b646de9");
            var armor_enchant = new NewMechanics.BuffContextEnchantArmor();
            armor_enchant.value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
            armor_enchant.enchantments = temporary_enchantments;

            magic_vestement_armor_buff.ComponentsArray = new BlueprintComponent[] {armor_enchant,
                                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel,
                                                                                   progression: ContextRankProgression.DivStep, startLevel: 4, min:1, stepLevel: 4, max: 5, type: AbilityRankType.StatBonus)
                                                                                  };
            magic_vestement_armor_buff.Stacking = StackingType.Replace;
        }


        static void fixMagicVestmentShield()
        {
            BlueprintArmorEnchantment[] enchantments = {Main.library.Get< BlueprintArmorEnchantment>("e90c252e08035294eba39bafce76c119"), //+1
                                                              Main.library.Get< BlueprintArmorEnchantment>("7b9f2f78a83577d49927c78be0f7fbc1"), //+2
                                                              Main.library.Get< BlueprintArmorEnchantment>("ac2e3a582b5faa74aab66e0a31c935a9"), //+3 
                                                              Main.library.Get< BlueprintArmorEnchantment>("a5d27d73859bd19469a6dde3b49750ff"), //+4
                                                              Main.library.Get< BlueprintArmorEnchantment>("84d191a748edef84ba30c13b8ab83bd9")  //+5
                                                             };

            BlueprintArmorEnchantment[] temporary_enchantments = new BlueprintArmorEnchantment[enchantments.Length];

            for (int i = 0; i < enchantments.Length; i++)
            {
                temporary_enchantments[i] = Main.library.CopyAndAdd<BlueprintArmorEnchantment>(enchantments[i].AssetGuid, $"TemporaryShieldEnchantment{i + 1}", "");
                Helpers.SetField(temporary_enchantments[i], "m_EnchantName", Helpers.CreateString($"TemporaryShieldEnchantment{i + 1}.Name", $"Temporary Enhancement + {i + 1}"));
            }

            var magic_vestement_shield_buff = Main.library.Get<BlueprintBuff>("2e8446f820936a44f951b50d70a82b16");
            var shield_enchant = new NewMechanics.BuffContextEnchantShield();
            shield_enchant.value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
            shield_enchant.enchantments = temporary_enchantments;

            magic_vestement_shield_buff.ComponentsArray = new BlueprintComponent[] {shield_enchant,
                                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel,
                                                                                   progression: ContextRankProgression.DivStep, startLevel: 4, min:1, stepLevel: 4, max: 5, type: AbilityRankType.StatBonus)
                                                                                  };
            magic_vestement_shield_buff.Stacking = StackingType.Replace;
        }


    }
}
