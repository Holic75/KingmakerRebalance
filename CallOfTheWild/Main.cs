using UnityModManagerNet;
using System;
using System.Reflection;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;


public class Main
{
    public class Settings
    {
        public bool update_companions  { get;}
        public bool nerf_animal_companion { get; }
        public bool reduce_skill_points { get; }
        public bool sacred_huntsmaster_animal_focus { get;  }

        public Settings()
        {
               
            using (StreamReader settings_file = File.OpenText("Mods/CallOfTheWild/settings.json"))
            using (JsonTextReader reader = new JsonTextReader(settings_file))
            {
                JObject jo = (JObject)JToken.ReadFrom(reader);
                update_companions = (bool)jo["update_companions"];
                nerf_animal_companion = (bool)jo["nerf_animal_companion"];
                reduce_skill_points = (bool)jo["reduce_skill_points"];
                sacred_huntsmaster_animal_focus = (bool)jo["sacred_huntsmaster_animal_focus"];
            }
        }
    }

    static public Settings settings = new Settings();
    public static UnityModManagerNet.UnityModManager.ModEntry.ModLogger logger;
    internal static LibraryScriptableObject library;

    static readonly Dictionary<Type, bool> typesPatched = new Dictionary<Type, bool>();
    static readonly List<String> failedPatches = new List<String>();
    static readonly List<String> failedLoading = new List<String>();

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
        }
        catch (Exception ex)
        {
            DebugError(ex);
            throw ex;   
        }
        return true;
    }
    [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
    [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
    static class LibraryScriptableObject_LoadDictionary_Patch
    {
        static void Postfix(LibraryScriptableObject __instance)
        {
            var self = __instance;
            if (Main.library != null) return;
            Main.library = self;
            try
            {
                Main.DebugLog("Loading Call of the Wild");

                CallOfTheWild.Helpers.GuidStorage.load(CallOfTheWild.Properties.Resources.blueprints);
                CallOfTheWild.Helpers.Load();
                CallOfTheWild.ArmorEnchantments.initialize();

                if (settings.nerf_animal_companion)
                {
                    Main.logger.Log("Upating animal companion bonuses.");
                    CallOfTheWild.Rebalance.fixAnimalCompanion();
                }

                if (settings.reduce_skill_points)
                {
                    Main.logger.Log("Reducing class skillpoints to 1/2 of pnp value.");
                    CallOfTheWild.Rebalance.fixSkillPoints();
                }

                if (settings.update_companions)
                {
                    Main.logger.Log("Updating companion stats.");
                    CallOfTheWild.Rebalance.fixCompanions();
                }

                CallOfTheWild.Rebalance.fixLegendaryProportionsAC();
                CallOfTheWild.Rebalance.removeJudgement19FormSHandMS();
                CallOfTheWild.Rebalance.fixDomains();
                CallOfTheWild.Rebalance.fixBarbarianRageAC();
                CallOfTheWild.Rebalance.fixInspiredFerocity();

                CallOfTheWild.Rebalance.fixMagicVestment();
                CallOfTheWild.Rebalance.fixDragonDiscipleBonusFeat();
                CallOfTheWild.Rebalance.fixAnimalGrowth();
                CallOfTheWild.Rebalance.fixIncreasedDamageReduction();
                //CallOfTheWild.Rebalance.fixNaturalACStacking();

                CallOfTheWild.Wildshape.load();
                CallOfTheWild.NewRagePowers.load();
                CallOfTheWild.NewSpells.load();

#if DEBUG
                CallOfTheWild.HexEngine.test_mode = true;
                CallOfTheWild.Bloodrager.test_mode = true;
                CallOfTheWild.Skald.test_mode = true;
                CallOfTheWild.Warpriest.test_mode = true;
#endif
                CallOfTheWild.Hunter.createHunterClass();
                if (settings.sacred_huntsmaster_animal_focus)
                {
                    Main.logger.Log("Replacing Sacred Huntsmaster favored enemy with animal focus.");
                    CallOfTheWild.Hunter.addAnimalFocusSH();
                }
                CallOfTheWild.HexEngine.Initialize();
                CallOfTheWild.Witch.createWitchClass();
                CallOfTheWild.Skald.createSkaldClass();
                CallOfTheWild.Bloodrager.createBloodragerClass();


                CallOfTheWild.SharedSpells.load();
                CallOfTheWild.NewFeats.load();

                CallOfTheWild.Warpriest.createWarpriestClass();

                CallOfTheWild.CleanUp.processRage();


#if DEBUG
                    string guid_file_name = @"C:\Repositories\KingmakerRebalance\CallOfTheWild\blueprints.txt";
                    CallOfTheWild.Helpers.GuidStorage.dump(guid_file_name);
#endif
                    CallOfTheWild.Helpers.GuidStorage.dump(@"Mods\CallOfTheWild\loaded_blueprints.txt");
                }
                catch (Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }

        internal static Exception Error(String message)
        {
            logger?.Log(message);
            return new InvalidOperationException(message);
        }
}
