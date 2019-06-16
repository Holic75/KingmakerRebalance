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

namespace CallOfTheWild
{
    public class Main
    {
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
            } catch(Exception ex)
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
                    Main.DebugLog("Loading Kingmaker Rebalance");


                    CallOfTheWild.Helpers.GuidStorage.load(Properties.Resources.blueprints);
                    CallOfTheWild.Helpers.Load();
                    CallOfTheWild.Rebalance.fixAnimalCompanion();
                    CallOfTheWild.Rebalance.fixLegendaryProportionsAC();
                    CallOfTheWild.Rebalance.fixSkillPoints();
                    CallOfTheWild.Rebalance.fixCompanions();
                    CallOfTheWild.Rebalance.removeJudgement19FormSHandMS();
                    CallOfTheWild.Rebalance.fixDomains();
                    CallOfTheWild.Rebalance.fixBarbarianRageAC();
                    CallOfTheWild.Wildshape.fixBeastShape();
                    CallOfTheWild.Rebalance.fixMagicVestment();

                    CallOfTheWild.Hunter.createHunterClass();
                    CallOfTheWild.Hunter.addAnimalFocusSH();
                    CallOfTheWild.Witch.createWitchClass();
                    CallOfTheWild.Bloodrager.test_mode = false;
                    CallOfTheWild.Bloodrager.createBloodragerClass();

#if DEBUG
                    string guid_file_name = @"C:\Repositories\KingmakerRebalance\CallOfTheWild\blueprints.txt";
                    CallOfTheWild.Helpers.GuidStorage.dump(guid_file_name);
#endif
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
}
