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

namespace KingmakerRebalance
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


                    KingmakerRebalance.Helpers.GuidStorage.load(Properties.Resources.blueprints);
                    KingmakerRebalance.Helpers.Load();
                    KingmakerRebalance.Rebalance.fixAnimalCompanion();
                    KingmakerRebalance.Rebalance.fixLegendaryProportionsAC();
                    KingmakerRebalance.Rebalance.fixSkillPoints();
                    KingmakerRebalance.Rebalance.fixCompanions();
                    KingmakerRebalance.Rebalance.removeJudgement19FormSHandMS();
                    KingmakerRebalance.Rebalance.fixDomains();
                    KingmakerRebalance.Rebalance.fixBarbarianRageAC();
                    KingmakerRebalance.Wildshape.fixBeastShape();
                    KingmakerRebalance.Rebalance.fixMagicVestment();

                    KingmakerRebalance.Hunter.createHunterClass();
                    KingmakerRebalance.Hunter.addAnimalFocusSH();
                    KingmakerRebalance.Witch.createWitchClass();
                    KingmakerRebalance.Bloodrager.test_mode = false;
                    KingmakerRebalance.Bloodrager.createBloodragerClass();

#if DEBUG
                    string guid_file_name = @"C:\Repositories\KingmakerRebalance\KingmakerRebalance\blueprints.txt";
                    KingmakerRebalance.Helpers.GuidStorage.dump(guid_file_name);
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
