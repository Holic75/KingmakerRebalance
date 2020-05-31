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
using Kingmaker.UnitLogic.Class.LevelUp;

namespace CallOfTheWild
{
    public class Main
    {
        internal class Settings
        {
            internal bool update_companions { get; }
            internal bool nerf_animal_companion { get; }
            internal bool reduce_skill_points { get; }
            internal bool sacred_huntsmaster_animal_focus { get; }
            internal bool use_armor_in_wildshape { get; }
            internal bool x3_crit_multiplier_for_flails { get; }
            internal bool swap_weapon_sets_as_move_action { get; }
            internal bool allow_spellcasting_in_elemental_form { get; }
            internal bool fix_teamwork_feats { get; }
            internal bool fix_ecclesitheurge_class { get; }
            internal bool advanced_fighter_options { get; }
            internal bool wizard_discoveries { get; }
            internal Settings()
            {

                using (StreamReader settings_file = File.OpenText(UnityModManager.modsPath + @"/CallOfTheWild/settings.json"))
                using (JsonTextReader reader = new JsonTextReader(settings_file))
                {
                    JObject jo = (JObject)JToken.ReadFrom(reader);
                    update_companions = (bool)jo["update_companions"];
                    //nerf_animal_companion = (bool)jo["nerf_animal_companion"];
                    reduce_skill_points = (bool)jo["reduce_skill_points"];
                    sacred_huntsmaster_animal_focus = (bool)jo["sacred_huntsmaster_animal_focus"];
                    use_armor_in_wildshape = (bool)jo["use_armor_in_wildshape"];
                    x3_crit_multiplier_for_flails = (bool)jo["x3_crit_multiplier_for_flails"];
                    swap_weapon_sets_as_move_action = (bool)jo["swap_weapon_sets_as_move_action"];
                    allow_spellcasting_in_elemental_form = (bool)jo["allow_spellcasting_in_elemental_form"];
                    fix_teamwork_feats = (bool)jo["fix_teamwork_feats"];
                    fix_ecclesitheurge_class = (bool)jo["fix_ecclesitheurge_class"];
                    advanced_fighter_options = (bool)jo["advanced_fighter_options"];
                    wizard_discoveries = (bool)jo["wizard_discoveries"];
                }
            }
        }

        static internal Settings settings = new Settings();
        internal static UnityModManagerNet.UnityModManager.ModEntry.ModLogger logger;
        internal static Harmony12.HarmonyInstance harmony;
        public static LibraryScriptableObject library;

        static readonly Dictionary<Type, bool> typesPatched = new Dictionary<Type, bool>();
        static readonly List<String> failedPatches = new List<String>();
        static readonly List<String> failedLoading = new List<String>();

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void DebugLog(string msg)
        {
            if (logger != null) logger.Log(msg);
        }
        internal static void DebugError(Exception ex)
        {
            if (logger != null) logger.Log(ex.ToString() + "\n" + ex.StackTrace);
        }
        internal static bool enabled;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;
                harmony = Harmony12.HarmonyInstance.Create(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                if (settings.swap_weapon_sets_as_move_action)
                {
                    Main.logger.Log("Changing weapons will take move action.");
                    NewMechanics.WeaponSetSwapPatch.Run();
                }
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

                    CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = UnityModManager.modsPath + @"/CallOfTheWild/Icons/";
#if DEBUG                
                    bool allow_guid_generation = true;
#else
                    bool allow_guid_generation = false; //no guids should be ever generated in release
#endif
                    CallOfTheWild.Helpers.GuidStorage.load(CallOfTheWild.Properties.Resources.blueprints, allow_guid_generation);
                    CallOfTheWild.Helpers.Load();
                    CallOfTheWild.ArmorEnchantments.initialize();
                    CallOfTheWild.WeaponEnchantments.initialize();

                    CallOfTheWild.Rebalance.fixAnimalCompanion();

                    if (settings.reduce_skill_points)
                    {
                        Main.logger.Log("Reducing class skillpoints to 1/2 of pnp value.");
                        CallOfTheWild.Rebalance.fixSkillPoints();
                    }

                    if (settings.x3_crit_multiplier_for_flails)
                    {
                        Main.logger.Log("Increasing flails crit multiplier to x3.");
                        CallOfTheWild.Rebalance.fixFlailCritMultiplier();
                    }


                    if (settings.fix_teamwork_feats)
                    {
                        Main.logger.Log("Fixing teamwork feats.");
                        CallOfTheWild.Rebalance.fixTeamworkFeats();
                    }

                    if (settings.fix_ecclesitheurge_class)
                    {
                        Main.logger.Log("Fixing Ecclesitheurge");
                        CallOfTheWild.Rebalance.fixEcclesitheurge();
                    }

                    CallOfTheWild.Common.initialize();
                    CallOfTheWild.Rebalance.fixTransmutionSchoolPhysicalEnhancement();
                    CallOfTheWild.Rebalance.fixSylvanSorcerorAnimalCompanion();
                    CallOfTheWild.Rebalance.fixLegendaryProportionsAC();
                    CallOfTheWild.Rebalance.removeJudgement19FormSHandMS();
                    CallOfTheWild.Rebalance.fixDomains();
                    CallOfTheWild.Rebalance.fixBarbarianRageAC();
                    CallOfTheWild.Rebalance.fixInspiredFerocity();
                    CallOfTheWild.Rebalance.fixWebSchool();
                    CallOfTheWild.Rebalance.fixMagicVestment();
                    CallOfTheWild.Rebalance.fixDragonDiscipleBonusFeat();
                    CallOfTheWild.Rebalance.fixAnimalSizeChange();
                    CallOfTheWild.Rebalance.fixIncreasedDamageReduction();
                    CallOfTheWild.Rebalance.fixItemBondForSpontnaeousCasters();
                    CallOfTheWild.FixFlying.load();
                    CallOfTheWild.Rebalance.fixVitalStrike();
                    CallOfTheWild.Rebalance.fixArcheologistLuck();
                    CallOfTheWild.Rebalance.fixElementalMovementWater();
                    CallOfTheWild.Rebalance.addRangerImprovedFavoredTerrain();
                    CallOfTheWild.Rebalance.fixChannelEnergySaclaing();
                    CallOfTheWild.Rebalance.fixCaveFangs();
                    CallOfTheWild.Rebalance.fixDazzlingDisplay();
                    CallOfTheWild.Rebalance.fixSpellDescriptors();
                    CallOfTheWild.Rebalance.fixSpellRanges();
                    CallOfTheWild.Rebalance.fixJudgments();
                    CallOfTheWild.Rebalance.fixMissingSlamProficiency();
                    CallOfTheWild.Rebalance.fixNeclaceOfDoubleCrosses();
                    CallOfTheWild.Rebalance.fixStalwartDefender();
                    CallOfTheWild.Rebalance.fixDomainSpells();
                    CallOfTheWild.Rebalance.fixAnimalCompanionFeats();
                    CallOfTheWild.Rebalance.fixAlchemistFastBombs();
                    CallOfTheWild.Rebalance.fixChannelEnergyHeal();
                    CallOfTheWild.Rebalance.fixElementalWallsToAvoidDealingDamageTwiceOnTheFirstRound();
                    CallOfTheWild.Rebalance.fixArchonsAuraToEffectOnlyEnemies();
                    CallOfTheWild.Rebalance.fixDruidDomainUi();
                    CallOfTheWild.Rebalance.fixLethalStance();
                    CallOfTheWild.Rebalance.disallowMultipleFamiliars();
                    CallOfTheWild.Rebalance.fixTrapfinding();
                    CallOfTheWild.Rebalance.fixPhysicalDrBypassToApplyToAllPhysicalDamage();
                    CallOfTheWild.Rebalance.fixUndeadImmunity();
                    CallOfTheWild.Rebalance.fixBleed();
                    CallOfTheWild.Rebalance.fixDispellingStrikeCL();
                    CallOfTheWild.Rebalance.addMobilityToMonkBonusFeats();
                    CallOfTheWild.Rebalance.fixGrappleSpells();
                    CallOfTheWild.Rebalance.fixDruidWoodlandStride();
                    CallOfTheWild.Rebalance.fixTandemTripPrerequisite();
                    CallOfTheWild.Rebalance.fixRangerAnimalCompanion();
                    CallOfTheWild.VitalStrikeMechanics.VitalStrikeRangedAttackPatch.Run();
                    CallOfTheWild.Rebalance.fixFlameDancer();
                    CallOfTheWild.Rebalance.fixSerpentineBloodlineSerpentfriend();
                    CallOfTheWild.MonkStunningFists.create();

                    //CallOfTheWild.Rebalance.fixNaturalACStacking();

                    CallOfTheWild.ChannelEnergyEngine.init();
                    if (settings.allow_spellcasting_in_elemental_form)
                    {
                        CallOfTheWild.Wildshape.allowElementalsToCast();
                    }
                    CallOfTheWild.Wildshape.load();
                    if (settings.use_armor_in_wildshape)
                    {
                        CallOfTheWild.Wildshape.allowToUseArmorInWildshape();
                    }
                    CallOfTheWild.MetamagicFeats.load();
                    CallOfTheWild.NewRagePowers.load();
                    CallOfTheWild.NewSpells.load();
                    CallOfTheWild.NewFeats.createDeityFavoredWeapon();
                    

                    bool inquisitions_test = false;
#if DEBUG
                    
                    CallOfTheWild.HexEngine.test_mode = true;
                    CallOfTheWild.Bloodrager.test_mode = true;
                    CallOfTheWild.Skald.test_mode = true;
                    CallOfTheWild.Warpriest.test_mode = true;
                    CallOfTheWild.Shaman.test_mode = true;
                    CallOfTheWild.Arcanist.test_mode = true;
                    CallOfTheWild.HolyVindicator.test_mode = true;
                    CallOfTheWild.NewFeats.test_mode = true;
                    CallOfTheWild.VindicativeBastard.test_mode = true;
                    inquisitions_test = true;
                    CallOfTheWild.RogueTalents.test_mode = true;
#endif
                    CallOfTheWild.Archetypes.UndeadLord.create();
                    CallOfTheWild.Summoner.createSummonerClass();
                    CallOfTheWild.Inquisitions.create(inquisitions_test);
                    CallOfTheWild.VindicativeBastard.createClass();
                    CallOfTheWild.Hunter.createHunterClass();
                    if (settings.sacred_huntsmaster_animal_focus)
                    {
                        Main.logger.Log("Replacing Sacred Huntsmaster favored enemy with animal focus.");
                        CallOfTheWild.Hunter.addAnimalFocusSH();
                    }
                    CallOfTheWild.HexEngine.Initialize();
                    CallOfTheWild.Witch.createWitchClass();
                    CallOfTheWild.Skald.createSkaldClass();
                    CallOfTheWild.Oracle.createOracleClass();
                    CallOfTheWild.Investigator.createInvestigatorClass();

                    CallOfTheWild.Archetypes.StormDruid.create();
                    CallOfTheWild.Shaman.createShamanClass();                   
                    CallOfTheWild.Bloodrager.createBloodragerClass();
                    CallOfTheWild.BloodlinesFix.load();
                    CallOfTheWild.Archetypes.PrimalSorcerer.create();
                    CallOfTheWild.Arcanist.createArcanistClass();

                    CallOfTheWild.SharedSpells.load();
                    

                    CallOfTheWild.Archetypes.DivineTracker.create(); // blessings will be filled in warpriest part
                    CallOfTheWild.Warpriest.createWarpriestClass();

                    CallOfTheWild.Archetypes.MindBlade.create();
                    CallOfTheWild.Archetypes.Evangelist.create();
                    CallOfTheWild.NewFeats.load();
                    CallOfTheWild.MagusArcana.load();
                    

                    CallOfTheWild.VersatilePerformance.create();
                    if (settings.advanced_fighter_options)
                    {
                        CallOfTheWild.AdvancedFighterOptions.load();
                    }
                    else
                    {
                        CallOfTheWild.AdvancedFighterOptions.prepareLookupData();
                    }

                    CallOfTheWild.Archetypes.ArrowsongMinstrel.create();
                    CallOfTheWild.Archetypes.DirgeBard.create();
                    CallOfTheWild.Archetypes.SpiritWhisperer.create();
                    CallOfTheWild.Archetypes.UntamedRager.create();
                    CallOfTheWild.Archetypes.NatureBondedMagus.create();                    
                    CallOfTheWild.Archetypes.ZenArcher.create();
                    CallOfTheWild.Archetypes.SanctifiedSlayer.create();
                    CallOfTheWild.Archetypes.LoreWarden.create();
                    CallOfTheWild.Archetypes.Preservationist.create();
                    CallOfTheWild.Archetypes.NatureFang.create();
                    CallOfTheWild.RogueTalents.load();
                    CallOfTheWild.Archetypes.Executioner.create();
                    CallOfTheWild.Archetypes.Ninja.create();

                    CallOfTheWild.Archetypes.Seeker.create();

                    CallOfTheWild.Hinterlander.createHinterlanderClass();
                    CallOfTheWild.HolyVindicator.createHolyVindicatorClass();

                    CallOfTheWild.KineticistFix.load();
                    CallOfTheWild.Archetypes.OverwhelmingSoul.create();
                    CallOfTheWild.Archetypes.KineticChirurgeion.create();
                    CallOfTheWild.Archetypes.SacredServant.create();
                    CallOfTheWild.Archetypes.MonkOfTheMantis.create();
                    CallOfTheWild.Archetypes.BeastkinBerserker.create();
                    CallOfTheWild.MysticTheurgeFix.load();
                    CallOfTheWild.AnimalCompanionLevelUp.AddPet_TryLevelUpPet_Patch.init();

                    CallOfTheWild.WizardDiscoveries.create(!settings.wizard_discoveries);
                    CallOfTheWild.CleanUp.processRage();
                    CallOfTheWild.CleanUp.fixWallAbilitiesAoeVIsualization();
                    CallOfTheWild.CleanUp.fixPolymorphSizeChangesStacking();
                    CallOfTheWild.DismissSpells.Dismiss.create();
                    CallOfTheWild.SaveGameFix.FixMissingAssets();
                    CallOfTheWild.AiFix.load();


                    if (settings.update_companions)
                    {
                        Main.logger.Log("Updating companion stats.");
                        CallOfTheWild.Rebalance.fixCompanions();
                    }


#if DEBUG
                    string guid_file_name = @"C:\Repositories\KingmakerRebalance\CallOfTheWild\blueprints.txt";
                    CallOfTheWild.Helpers.GuidStorage.dump(guid_file_name);
#endif
                    CallOfTheWild.Helpers.GuidStorage.dump(UnityModManager.modsPath + @"/CallOfTheWild/loaded_blueprints.txt");
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
