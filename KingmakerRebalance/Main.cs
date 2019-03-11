using UnityModManagerNet;
using System;
using System.Reflection;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Designers.Mechanics.Buffs;
using System.Collections.Generic;

namespace KingmakerRebalance
{
    public class Main
    {
        public static UnityModManagerNet.UnityModManager.ModEntry.ModLogger logger;
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLog(string msg)
        {
            if (logger != null) logger.Log(msg);
        }
        public static void DebugError(Exception ex)
        {
            if (logger != null) logger.Log(ex.ToString() + "\n" + ex.StackTrace);
        }
        public static bool enabled;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;
                var harmony = Harmony12.HarmonyInstance.Create(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            } catch(Exception ex)
            {
                DebugError(ex);
                throw ex;   
            }
            return true;
        }
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        static class LibraryScriptableObject_LoadDictionary_Patch
        {
            static void Postfix()
            {
                try
                {
                    Main.DebugLog("Installing Kingmaker Rebalance");
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
                catch (Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }

    }
}
