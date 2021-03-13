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
        public class Settings
        {
            public bool update_companions { get; }
            public bool nerf_animal_companion { get; }
            public bool reduce_skill_points { get; }
            public bool sacred_huntsmaster_animal_focus { get; }
            public bool swap_weapon_sets_as_move_action { get; }
            public bool allow_spellcasting_in_elemental_form { get; }
            public bool fix_teamwork_feats { get; }
            public bool fix_ecclesitheurge_class { get; }
            public bool advanced_fighter_options { get; }
            public bool wizard_discoveries { get; }
            public bool deity_for_everyone { get; }
            public bool secondary_rake_attacks { get; }
            public bool one_sneak_attack_per_target_per_spell { get; }
            public bool metamagic_for_spontaneous_spell_conversion { get; }
            public bool remove_solo_tactics_from_sacred_huntsmaster { get; }
            public bool update_kineticist_archetypes { get; }
            public bool balance_fixes { get; }
            public bool balance_fixes_monk_ac { get; }
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
                    swap_weapon_sets_as_move_action = (bool)jo["swap_weapon_sets_as_move_action"];
                    allow_spellcasting_in_elemental_form = (bool)jo["allow_spellcasting_in_elemental_form"];
                    fix_teamwork_feats = (bool)jo["fix_teamwork_feats"];
                    fix_ecclesitheurge_class = (bool)jo["fix_ecclesitheurge_class"];
                    advanced_fighter_options = (bool)jo["advanced_fighter_options"];
                    wizard_discoveries = (bool)jo["wizard_discoveries"];
                    secondary_rake_attacks = (bool)jo["secondary_rake_attacks"];
                    one_sneak_attack_per_target_per_spell = (bool)jo["one_sneak_attack_per_target_per_spell"];
                    metamagic_for_spontaneous_spell_conversion = (bool)jo["metamagic_for_spontaneous_spell_conversion"];
                    remove_solo_tactics_from_sacred_huntsmaster = (bool)jo["remove_solo_tactics_from_sacred_huntsmaster"];
                    update_kineticist_archetypes = (bool)jo["update_kineticist_archetypes"];
                    balance_fixes = (bool)jo["balance_fixes"];
                    balance_fixes_monk_ac = (bool)jo["balance_fixes_monk_ac"];
                }
            }
        }

        static public Settings settings = new Settings();
        internal static UnityModManagerNet.UnityModManager.ModEntry.ModLogger logger;
        internal static Harmony12.HarmonyInstance harmony;
        public static LibraryScriptableObject library;

        static readonly Dictionary<Type, bool> typesPatched = new Dictionary<Type, bool>();
        static readonly List<String> failedPatches = new List<String>();
        static readonly List<String> failedLoading = new List<String>();

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void DebugLog(string msg)
        {
            if (logger != null)
                logger.Log(msg);
        }


        [System.Diagnostics.Conditional("DEBUG")]
        internal static void TraceLog()
        {
            /*if (logger != null)
            {
                logger.Log("StackTrace:" + Environment.StackTrace);
            }*/
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

                /*if (settings.swap_weapon_sets_as_move_action)
                {
                    Main.logger.Log("Changing weapons will take move action.");
                    NewMechanics.WeaponSetSwapPatch.Run();
                }*/
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
                    CallOfTheWild.HarmlessSaves.HarmlessSaves.init();
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
                    CallOfTheWild.Deities.create();

                    CallOfTheWild.Rebalance.fixAnimalCompanion();

                    if (settings.reduce_skill_points)
                    {
                        Main.logger.Log("Reducing class skillpoints to 1/2 of pnp value.");
                        CallOfTheWild.Rebalance.fixSkillPoints();
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


                    if (settings.remove_solo_tactics_from_sacred_huntsmaster)
                    {
                        Main.logger.Log("Removing Solo Tactics from Sacred Huntsmaster");
                        CallOfTheWild.Rebalance.removeSoloTacticsFromSH();
                    }

                    CallOfTheWild.Common.initialize();
                    CallOfTheWild.Rebalance.removeDescriptionsFromMonkACFeatures();
                    CallOfTheWild.Rebalance.refixBardicPerformanceOverlap();
                    CallOfTheWild.Rebalance.removePowerOfWyrmsBuffImmunity();
                    CallOfTheWild.Rebalance.fixWidomCognatogen();
                    CallOfTheWild.Rebalance.fixTransmutionSchoolPhysicalEnhancement();
                    CallOfTheWild.Rebalance.fixSylvanSorcererAnimalCompanion();
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
                    CallOfTheWild.Rebalance.fixStalwartDefender();
                    CallOfTheWild.Rebalance.fixChannelEnergyHeal();
                    CallOfTheWild.Rebalance.condenseMonkUnarmedDamage();
                    if (settings.balance_fixes)
                    {
                        Main.logger.Log("Applying balance changes");
                        CallOfTheWild.BalanceFixes.load("979f63920af22344d81da5099c9ec32e", //death domain bleed
                                                        "ad9a6a7ee08ce73469dff703a17f8934", //medium elemental burn
                                                        "7d0f50b37b787ea4d8f5a09dd2f30a4e" //mirrow bow damage 
                                                         );
                    }
                    CallOfTheWild.Rebalance.fixDomainSpells();
                    CallOfTheWild.Rebalance.fixAnimalCompanionFeats();
                    CallOfTheWild.Rebalance.fixAlchemistFastBombs();
                    
                    CallOfTheWild.Rebalance.fixElementalWallsToAvoidDealingDamageTwiceOnTheFirstRound();
                    CallOfTheWild.Rebalance.fixArchonsAuraToEffectOnlyEnemiesAndDescription();
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
                    CallOfTheWild.Rebalance.fixRangerMasterHunter();
                    CallOfTheWild.Rebalance.fixEaglesoul();
                    CallOfTheWild.Rebalance.fixGrease();
                    CallOfTheWild.Rebalance.fixEldritchArcherPenalty();
                    CallOfTheWild.Rebalance.fixSpellRemoveFearBuff();
                    CallOfTheWild.Rebalance.fixSpellUnbreakableHeartBuff();
                    CallOfTheWild.MonkStunningFists.create();
                    CallOfTheWild.Rebalance.fixTactician();
                    CallOfTheWild.Rebalance.fixFeatsRequirements();
                    CallOfTheWild.Rebalance.createAidAnother();
                    CallOfTheWild.Rebalance.fixFeyStalkerSummonBuff();
                    CallOfTheWild.Rebalance.fixBeltsOfPerfectComponents();
                    CallOfTheWild.Rebalance.addMissingImmunities();
                    CallOfTheWild.Rebalance.fixJaethalUndeadFeature();
                    CallOfTheWild.Rebalance.addFatigueBuffRestrictionsToRage();
                    CallOfTheWild.Rebalance.fixNecklaceOfDoubleCorses();
                    CallOfTheWild.Rebalance.fixDelayPoison();
                    CallOfTheWild.Rebalance.fixSacredmasterHunterTactics();
                    CallOfTheWild.Rebalance.fixAuraOfJustice();
                    CallOfTheWild.Rebalance.fixArchetypeKineticistGatherPowerWithShield();
                    CallOfTheWild.Rebalance.fixSuppressBuffs();
                    CallOfTheWild.Rebalance.fixBlindingRay();
                    CallOfTheWild.Rebalance.fixElementalArcana();

                    if (settings.secondary_rake_attacks)
                    {
                        Main.logger.Log("Applying -5 penalty to rake attacks.");
                        CallOfTheWild.Rebalance.nerfSmilodonRake();
                    }

                    CallOfTheWild.ShadowSpells.ShadowSpells.init();
                    //CallOfTheWild.Rebalance.fixNaturalACStacking();
                    CallOfTheWild.Familiars.load();
                    CallOfTheWild.ChannelEnergyEngine.init();
                    if (settings.allow_spellcasting_in_elemental_form)
                    {
                        CallOfTheWild.Wildshape.allowElementalsToCast();
                    }
                    CallOfTheWild.Wildshape.load();

                    CallOfTheWild.MetamagicFeats.load();
                    CallOfTheWild.Rebalance.fixUniversalistMetamagicMastery();
                    CallOfTheWild.NewSpells.load();
                    CallOfTheWild.Rebalance.fixFlameWardenSpells();
                    CallOfTheWild.NewFeats.createDisruptive();
                    CallOfTheWild.NewFeats.createSpellbreaker();
                    CallOfTheWild.NewRagePowers.load();   
                    CallOfTheWild.Subdomains.load();
                    CallOfTheWild.NewFeats.createDeityFavoredWeapon();
                    CallOfTheWild.Subschools.load();
                    CallOfTheWild.WeaponsFix.load();
                    CallOfTheWild.SharedSpells.preload();

                    bool inquisitions_test = false;
#if DEBUG
                    CallOfTheWild.Spiritualist.test_mode = true;
                    CallOfTheWild.HexEngine.test_mode = true;
                    CallOfTheWild.Bloodrager.test_mode = true;
                    CallOfTheWild.Skald.test_mode = true;
                    CallOfTheWild.Warpriest.test_mode = true;
                    CallOfTheWild.Shaman.test_mode = true;
                    CallOfTheWild.Arcanist.test_mode = true;
                    CallOfTheWild.HolyVindicator.test_mode = true;
                    CallOfTheWild.NewFeats.test_mode = true;
                    CallOfTheWild.VindicativeBastard.test_mode = true;
                    CallOfTheWild.Occultist.test_mode = true;
                    inquisitions_test = true;
                    CallOfTheWild.RogueTalents.test_mode = true;
#endif
                    CallOfTheWild.Archetypes.UndeadLord.create();
                    CallOfTheWild.Summoner.createSummonerClass();
                    CallOfTheWild.Inquisitions.create(inquisitions_test);
                    CallOfTheWild.Hunter.createHunterClass();
                    CallOfTheWild.VindicativeBastard.createClass();
                    CallOfTheWild.Archetypes.IroranPaladin.create();
                    CallOfTheWild.Antipaladin.creatAntipaldinClass();
                    if (settings.sacred_huntsmaster_animal_focus)
                    {
                        Main.logger.Log("Replacing Sacred Huntsmaster favored enemy with animal focus.");
                        CallOfTheWild.Hunter.addAnimalFocusSH();
                    }
                    CallOfTheWild.HexEngine.Initialize();
                    CallOfTheWild.Witch.createWitchClass();
                    CallOfTheWild.Skald.createSkaldClass();
                    CallOfTheWild.Archetypes.RavenerHunter.create();
                    CallOfTheWild.Oracle.createOracleClass();
                    
                    CallOfTheWild.Investigator.createInvestigatorClass();
                    CallOfTheWild.Spiritualist.createSpiritualistClass();

                    CallOfTheWild.Archetypes.StormDruid.create();
                    CallOfTheWild.Shaman.createShamanClass();
                    CallOfTheWild.Archetypes.DraconicDruid.create();
                    CallOfTheWild.Psychic.createPsychicClass();
                    CallOfTheWild.Occultist.createOccultistClass();
                    CallOfTheWild.Archetypes.RelicHunter.create();
                    CallOfTheWild.Bloodrager.createBloodragerClass();
                    CallOfTheWild.BloodlinesFix.load(); //sorcerer archetypes with alternate bloodlines are created inside

                    CallOfTheWild.Arcanist.createArcanistClass();                    
                    CallOfTheWild.Archetypes.DrillSergeant.create();
                    CallOfTheWild.Archetypes.PackRager.create();
                    CallOfTheWild.Archetypes.DivineScourge.create();
                                      
                    CallOfTheWild.Archetypes.DivineTracker.create(); // blessings will be filled in warpriest part
                    CallOfTheWild.Warpriest.createWarpriestClass();

                    CallOfTheWild.SharedSpells.load();

                    CallOfTheWild.Archetypes.MindBlade.create();
                    CallOfTheWild.Archetypes.Skirnir.create();
                    CallOfTheWild.Archetypes.Evangelist.create();
                    CallOfTheWild.Archetypes.ArrowsongMinstrel.create();
                    CallOfTheWild.Archetypes.DirgeBard.create();
                    CallOfTheWild.Archetypes.CourtBard.create();
                    //Note: archetypes with new performances should be created before NewFeats to allow discordant voice to pick relevant toggles
                    CallOfTheWild.NewFeats.load();
                    CallOfTheWild.MagusArcana.load();
                    CallOfTheWild.SkillUnlocks.load();

                    CallOfTheWild.VersatilePerformance.create();
                    if (settings.advanced_fighter_options)
                    {
                        CallOfTheWild.AdvancedFighterOptions.load();
                    }
                    else
                    {
                        CallOfTheWild.AdvancedFighterOptions.prepareLookupData();
                    }
                    CallOfTheWild.MonkKiPowers.load();

                    CallOfTheWild.Archetypes.SpiritWhisperer.create();
                    CallOfTheWild.Archetypes.UntamedRager.create();
                    CallOfTheWild.Archetypes.NatureBondedMagus.create();                    
                    CallOfTheWild.Archetypes.ZenArcher.create();
                    CallOfTheWild.Archetypes.SageCounselor.create();
                    CallOfTheWild.Archetypes.SanctifiedSlayer.create();
                    CallOfTheWild.Archetypes.LoreWarden.create();
                    CallOfTheWild.Archetypes.DervishOfDawn.create();
                    CallOfTheWild.Archetypes.Preservationist.create();
                    CallOfTheWild.Archetypes.NatureFang.create();
                    CallOfTheWild.RogueTalents.load();
                    CallOfTheWild.Archetypes.Executioner.create();
                    CallOfTheWild.Archetypes.Ninja.create();
                    CallOfTheWild.Archetypes.Seeker.create();
                    CallOfTheWild.Archetypes.Bloodhunter.create();
                    CallOfTheWild.Archetypes.StygianSlayer.create();

                    CallOfTheWild.Hinterlander.createHinterlanderClass();
                    CallOfTheWild.HolyVindicator.createHolyVindicatorClass();
                    CallOfTheWild.DawnflowerAnchorite.createDawnflowerAnchoriteClass();

                    CallOfTheWild.KineticistFix.load(Main.settings.update_kineticist_archetypes);
                    CallOfTheWild.Archetypes.Rake.create();
                    CallOfTheWild.Archetypes.OverwhelmingSoul.create();
                    CallOfTheWild.Archetypes.KineticChirurgeion.create();
                    CallOfTheWild.Archetypes.SacredServant.create();
                    CallOfTheWild.Archetypes.MonkOfTheMantis.create();
                    CallOfTheWild.Archetypes.BeastkinBerserker.create();
                    CallOfTheWild.Archetypes.GraveWarden.create();
                    CallOfTheWild.Archetypes.Toxicant.create();
                    CallOfTheWild.Archetypes.Swashbuckler.create();
                    CallOfTheWild.MysticTheurgeFix.load();
                    CallOfTheWild.AnimalCompanionLevelUp.AddPet_TryLevelUpPet_Patch.init();

                    CallOfTheWild.WizardDiscoveries.create(!settings.wizard_discoveries);
                    CallOfTheWild.NewFeats.createPreferredSpell();
                    CallOfTheWild.MetamagicFeats.setMetamagicFlags();
                    CallOfTheWild.NewSpells.fixShadowSpells();
                    CallOfTheWild.Archetypes.PactWizard.create();
                    CallOfTheWild.CleanUp.run();
                    CallOfTheWild.DismissSpells.Dismiss.create();
                    CallOfTheWild.Rebalance.fixTristianAngelBuff();
                    
                    CallOfTheWild.SaveGameFix.FixMissingAssets();
                    CallOfTheWild.AiFix.load();


                    if (settings.update_companions)
                    {
                        Main.logger.Log("Updating companion stats.");
                        CallOfTheWild.Rebalance.fixCompanions();
                    }

                    Main.logger.Log("metamagic_for_spontaneous_spell_conversion:" +settings.metamagic_for_spontaneous_spell_conversion.ToString());
                    Main.logger.Log("one_sneak_attack_per_target_per_spell:" + settings.one_sneak_attack_per_target_per_spell.ToString());



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
