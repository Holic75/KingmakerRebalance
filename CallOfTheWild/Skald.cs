using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using static Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility;

namespace CallOfTheWild
{
   
    class Skald
    {
        static internal bool test_mode = false;
        static internal LibraryScriptableObject library => Main.library;

        static internal BlueprintCharacterClass skald_class;
        static internal BlueprintProgression skald_progression;

        static internal BlueprintFeature skald_proficiencies;
        static internal BlueprintFeature damage_reduction;
        static internal BlueprintFeature uncanny_dodge;
        static internal BlueprintFeature improved_uncanny_dodge;
        static internal BlueprintFeature fast_movement;
        static internal BlueprintFeatureSelection versatile_performance;
        static internal BlueprintFeature well_versed;
        static internal BlueprintAbilityResource performance_resource;
        static internal BlueprintFeature give_performance_resource;

        static internal BlueprintFeature inspired_rage_feature;
        static internal BlueprintActivatableAbility inspired_rage;
        static internal BlueprintBuff inspired_rage_effect_buff;
        static internal BlueprintFeature master_skald;
        static internal BlueprintBuff master_skald_buff;
        static internal BlueprintFeatureSelection skald_rage_powers;
        static internal BlueprintFeature song_of_marching;
        static internal BlueprintFeature song_of_strength;
        static internal BlueprintFeature dirge_of_doom;
        static internal BlueprintFeature song_of_the_fallen;
        static internal BlueprintFeature lore_master;
        static internal BlueprintFeature spell_kenning;
        static internal BlueprintFeature spell_kenning_extra_use;

        internal static void createSkaldClass()
        {
            Main.logger.Log("Skald class test mode: " + test_mode.ToString());
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

            skald_class = Helpers.Create<BlueprintCharacterClass>();
            skald_class.name = "SkaldClass";
            library.AddAsset(skald_class, "");

            skald_class.LocalizedName = Helpers.CreateString("Skald.Name", "Skald");
            skald_class.LocalizedDescription = Helpers.CreateString("Skald.Description",
                                                                         "Skalds are poets, historians, and keepers of lore who use their gifts for oration and song to inspire allies into a frenzied rage. They balance a violent spirit with the veneer of civilization, recording events such as heroic battles and the deeds of great leaders, enhancing these stories in the retelling to earn bloodier victories in combat. A skald’s poetry is nuanced and often has multiple overlapping meanings, and he applies similar talents to emulate magic from other spellcasters.\n"
                                                                         + "Role: A skald inspires his allies, and often presses forward to fight enemies in melee. Outside of combat, he’s useful as a healer and scholar, less versatile but more durable than a bard."
                                                                         );
            skald_class.m_Icon = bard_class.Icon;
            skald_class.SkillPoints = barbarian_class.SkillPoints;
            skald_class.HitDie = DiceType.D8;
            skald_class.BaseAttackBonus = bard_class.BaseAttackBonus;
            skald_class.FortitudeSave = barbarian_class.FortitudeSave;
            skald_class.ReflexSave = barbarian_class.ReflexSave;
            skald_class.WillSave = bard_class.WillSave;
            skald_class.Spellbook = createSkaldSpellbook();
            skald_class.ClassSkills = new StatType[] {StatType.SkillAthletics, StatType.SkillMobility,
                                                      StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion,
                                                      StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice}; //everything except stealth and trickety
            skald_class.IsDivineCaster = false;
            skald_class.IsArcaneCaster = true;
            skald_class.StartingGold = bard_class.StartingGold;
            skald_class.PrimaryColor = bard_class.PrimaryColor;
            skald_class.SecondaryColor = bard_class.SecondaryColor;
            skald_class.RecommendedAttributes = new StatType[] { StatType.Strength, StatType.Charisma, StatType.Constitution };
            skald_class.NotRecommendedAttributes = new StatType[0];
            skald_class.EquipmentEntities = bard_class.EquipmentEntities;
            skald_class.MaleEquipmentEntities = bard_class.MaleEquipmentEntities;
            skald_class.FemaleEquipmentEntities = bard_class.FemaleEquipmentEntities;
            skald_class.ComponentsArray =  bard_class.ComponentsArray;
            skald_class.StartingItems = bard_class.StartingItems;
            createSkaldProgression();
            skald_class.Progression = skald_progression;

          
            skald_class.Archetypes = new BlueprintArchetype[] {}; //battle scion, spellwarrior?, court poet ? urban skald? herald_of_the_horn?
            Helpers.RegisterClass(skald_class);
            //addToPrestigeClasses(); //to at, mt, ek, dd
            //fixExtraPerformance
        }


        static BlueprintCharacterClass[] getSkaldArray()
        {
            return new BlueprintCharacterClass[] { skald_class };
        }


        static void createSkaldProgression()
        {

            createVersatilePerformance();
            createWellVersed();
            createPerfromanceResource();
            createMasterSkald();
            createInspiredRage();
            createSkaldRagePowersFeature();
            createSkaldDamageReduction();
            createSongOfMarching();
            createSongOfStrength();
            createDirgeOfDoom();
            createSongOfTheFallen();
            createLoreMaster();
            createSpellKenning();


            skald_progression = Helpers.CreateProgression("SkaldProgression",
                           skald_class.Name,
                           skald_class.Description,
                           "",
                           skald_class.Icon,
                           FeatureGroup.None);
            skald_progression.Classes = getSkaldArray();

            skald_proficiencies = library.CopyAndAdd<BlueprintFeature>(Bloodrager.bloodrager_proficiencies.AssetGuid,
                                                                            "SkaldProficiencies",
                                                                            "");
            skald_proficiencies.SetName("Skald Proficiencies");
            skald_proficiencies.SetDescription("A skald is proficient with all simple and martial weapons, light and medium armor, and shields (except tower shields). A skald can cast skald spells while wearing light or medium armor and even using a shield without incurring the normal arcane spell failure chance. ");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            fast_movement = Bloodrager.fast_movement;
            uncanny_dodge = Bloodrager.uncanny_dodge;
            improved_uncanny_dodge = Bloodrager.improved_uncanny_dodge;

            var cantrips = library.CopyAndAdd<BlueprintFeature>("4f422e8490ec7d94592a8069cce47f98", //bard cantrips
                                                                     "SkaldCantripsFeature",
                                                                     ""
                                                                     );
            cantrips.SetDescription("Skalds learn a number of cantrips, or 0-level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            cantrips.ReplaceComponent<BindAbilitiesToClass>(c => { c.CharacterClass = skald_class; });
            cantrips.ReplaceComponent<LearnSpells>(c => { c.CharacterClass = skald_class; });




            skald_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, skald_proficiencies, fast_movement, detect_magic, cantrips,
                                                                                        give_performance_resource, inspired_rage_feature,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, versatile_performance, well_versed),
                                                                    Helpers.LevelEntry(3, skald_rage_powers, song_of_marching),
                                                                    Helpers.LevelEntry(4, uncanny_dodge),
                                                                    Helpers.LevelEntry(5, spell_kenning),
                                                                    Helpers.LevelEntry(6, skald_rage_powers, song_of_strength),
                                                                    Helpers.LevelEntry(7, versatile_performance, lore_master),
                                                                    Helpers.LevelEntry(8, improved_uncanny_dodge),
                                                                    Helpers.LevelEntry(9, skald_rage_powers, damage_reduction), 
                                                                    Helpers.LevelEntry(10, dirge_of_doom),
                                                                    Helpers.LevelEntry(11, spell_kenning_extra_use),
                                                                    Helpers.LevelEntry(12, versatile_performance, skald_rage_powers),
                                                                    Helpers.LevelEntry(13),
                                                                    Helpers.LevelEntry(14, damage_reduction, song_of_the_fallen), 
                                                                    Helpers.LevelEntry(15, skald_rage_powers),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17, versatile_performance, spell_kenning_extra_use),
                                                                    Helpers.LevelEntry(18, skald_rage_powers), 
                                                                    Helpers.LevelEntry(19, damage_reduction),
                                                                    Helpers.LevelEntry(20, master_skald)
                                                                    };

            skald_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { skald_proficiencies, detect_magic, cantrips };
            skald_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(versatile_performance, versatile_performance, versatile_performance, versatile_performance),
                                                         Helpers.CreateUIGroup(fast_movement, uncanny_dodge, improved_uncanny_dodge),
                                                         Helpers.CreateUIGroup(inspired_rage_feature, song_of_marching, song_of_strength, dirge_of_doom, song_of_the_fallen, master_skald),
                                                         Helpers.CreateUIGroup(well_versed, lore_master, spell_kenning, spell_kenning_extra_use, spell_kenning_extra_use)
                                                        };
        }


        static void createSpellKenning()
        {
            var resource = Helpers.CreateAbilityResource("SkaldSpellKenningResource", "", "", "", null);
            resource.SetFixedResource(1);
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var wizard_class = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

            var classes = new (BlueprintCharacterClass, UnityEngine.Sprite)[] { (skald_class, library.Get<BlueprintFeature>("c459c8200e666ef4c990873d3e501b91").Icon), //transmutation
                                                                                (cleric_class, library.Get<BlueprintFeature>("30f20e6f850519b48aa59e8c0ff66ae9").Icon), //abjuration
                                                                                (wizard_class, library.Get<BlueprintFeature>("c46512b796216b64899f26301241e4e6").Icon)}; //evocation 
            const int max_level = 6;
            const int min_level = 1;
            const int max_variants_in_list = 12;
            List<BlueprintComponent> spell_kenning_base = new List<BlueprintComponent>();

            var icon = library.Get<BlueprintAbility>("da2447792e4ced74c80f35e29eb7a9e8").Icon; //secret of s
            for (int i = min_level; i <= max_level; i++)
            {
                var spell_kenning_base_i = Helpers.CreateAbility($"SkaldSpellKenning{i}BaseAbility",
                                                                 $"Spell Kenning {Common.roman_id[i]}",
                                                                 $"This ability allows to cast any Cleric, Skald or Wizard spell of level {i}.",
                                                                 "",
                                                                 icon,
                                                                 AbilityType.Spell,
                                                                 CommandType.Standard,
                                                                 AbilityRange.Personal,
                                                                 "",
                                                                 "");
                Helpers.SetField(spell_kenning_base_i, "m_IsFullRoundAction", true);
                List<BlueprintAbility> base_i_variants = new List<BlueprintAbility>();

                
                foreach (var c in classes)
                {
                    var variant_spells = Common.CreateAbilityVariantsReplace(null, "SkaldSpellKenning" + c.Item1.name,
                                                                                    s => {
                                                                                        s.ActionType = CommandType.Standard;
                                                                                        Helpers.SetField(s, "m_IsFullRoundAction", true);
                                                                                        if (s.GetComponent<AbilityResourceLogic>() == null)
                                                                                        {
                                                                                            s.AddComponent(Helpers.CreateResourceLogic(resource));
                                                                                        }
                                                                                    },
                                                                                  c.Item1.Spellbook.SpellList.SpellsByLevel[i].Spells.ToArray()
                                                                                  );
                    for (int j = 0; j < variant_spells.Length - 1; j+= max_variants_in_list)
                    {
                        int id = j / max_variants_in_list + 1;
                        var spell_kenning_class = Helpers.CreateAbility($"SkaldSpellKenning{i}Base{c.Item1.name}{id}Ability",
                                                     spell_kenning_base_i.Name + $" ({c.Item1.Name}, {id})",
                                                     $"This ability allows to cast any {c.Item1.Name} spell of level {i}.",
                                                     "",
                                                     c.Item2,
                                                     AbilityType.Spell,
                                                     CommandType.Standard,
                                                     AbilityRange.Close,
                                                     "",
                                                     "");
                        int num_spells = j + max_variants_in_list > variant_spells.Length - 1 ? variant_spells.Length - j : max_variants_in_list;
                        var variants_to_add = variant_spells.Skip(j).Take(num_spells).ToArray();
                        spell_kenning_class.AddComponent(Helpers.CreateAbilityVariants(spell_kenning_class, variants_to_add));
                        spell_kenning_base.Add(Helpers.CreateAddKnownSpell(spell_kenning_class, skald_class, i));
                    }         
                }
            }


            spell_kenning = Helpers.CreateFeature("SkaldSpellKenningFeature",
                                      "Spell Kenning",
                                      "At 5th level, a skald is learned in the magic of other spellcasters, and can use his own magic to duplicate those classes’ spells. Once per day, a skald can cast any spell on the bard, cleric, or sorcerer/wizard spell list as if it were one of his skald spells known, expending a skald spell slot of the same spell level to cast the desired spell. Casting a spell with spell kenning always has a minimum casting time of 1 full round, regardless of the casting time of the spell.\n"
                                       + ",At 11th level, a skald can use this ability twice per day.At 17th level, he can use this ability three times per day.",
                                      "",
                                      icon,
                                      FeatureGroup.None,
                                      spell_kenning_base.ToArray().AddToArray(Helpers.CreateAddAbilityResource(resource))
                                      );

            spell_kenning_extra_use = Helpers.CreateFeature("SkaldSpellKenningExtraUseFeature",
                          spell_kenning.Name,
                          spell_kenning.Description,
                          "",
                          icon,
                          FeatureGroup.None,
                          Helpers.CreateIncreaseResourceAmount(resource, 1)
                          );
        }


        static void createLoreMaster()
        {
            lore_master = Helpers.CreateFeature("SkaldLoreMasterFeature",
                                                "Lore Master",
                                                "At 7th level skald receives + 1 bonus to all lore and knowledge skills, this bonus increases by one every 6 levels thereafter.",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeArcana, ModifierDescriptor.UntypedStackable, ContextValueType.Rank),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeWorld, ModifierDescriptor.UntypedStackable, ContextValueType.Rank),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.UntypedStackable, ContextValueType.Rank),
                                                Helpers.CreateAddContextStatBonus(StatType.SkillLoreReligion, ModifierDescriptor.UntypedStackable, ContextValueType.Rank),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getSkaldArray(),
                                                                                progression: ContextRankProgression.StartPlusDivStep,
                                                                                startLevel: 7, stepLevel: 6)
                                               );
        }


        static void createSongOfTheFallen()
        {
            var song_of_the_fallen_ability = library.CopyAndAdd<BlueprintAbility>("80a1a388ee938aa4e90d427ce9a7a3e9", "SkaldSongOfTheFallen", "");
            song_of_the_fallen_ability.MaterialComponent = new MaterialComponentData();
            song_of_the_fallen_ability.SetName("Song of the fallen");
            song_of_the_fallen_ability.SetDescription("At 14th level, a skald can temporarily revive dead allies to continue fighting, with the same limitations as raise dead.The skald selects a dead ally within 30 feet and expends 6 rounds of raging song to bring that ally back to life for number of rounds equal to half of skald level. The revived ally is alive but staggered.");
            song_of_the_fallen_ability.Type = AbilityType.Supernatural;
            song_of_the_fallen_ability.Range = AbilityRange.Close;
            var staggered_buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var buff = Helpers.CreateBuff("SkaldSongOfTheFallenEffectBuff",
                              song_of_the_fallen_ability.Name,
                              song_of_the_fallen_ability.Description,
                              "",
                              song_of_the_fallen_ability.Icon,
                              null
                              );

            buff.SetBuffFlags(BuffFlags.StayOnDeath);
            
            buff.AddComponent(Helpers.CreateAddFactContextActions(newRound: Common.createContextActionApplyBuff(staggered_buff, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true),
                                                                  deactivated:Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionKillTarget>()
                                                                  )
                            );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);


            song_of_the_fallen_ability.ReplaceComponent<AbilityEffectRunAction>(c => c.Actions = Helpers.CreateActionList(c.Actions.Actions.AddToArray(apply_buff)));
            song_of_the_fallen_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getSkaldArray(),
                                                                                    progression: ContextRankProgression.Div2, type: AbilityRankType.Default)
                                                                                    );
            song_of_the_fallen_ability.AddComponent(Helpers.CreateResourceLogic(performance_resource, true, 6));


            song_of_the_fallen = Helpers.CreateFeature("SkaldSongOfTheFallenFeature",
                                                     song_of_the_fallen_ability.Name,
                                                     song_of_the_fallen_ability.Description,
                                                     "",
                                                     song_of_the_fallen_ability.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(song_of_the_fallen_ability)
                                                     );
        }


        static void createDirgeOfDoom()
        {
            var dirge_of_doom_ability = library.CopyAndAdd<BlueprintActivatableAbility>("d99d63f84e180d44e8f92b9a832c609d","SkaldDirgeOfDoomToggleAbility", "");
            dirge_of_doom_ability.SetDescription("At 10th level, a skald can create a sense of growing dread in his enemies, causing them to become shaken. This only affects enemies that are within 30 feet and able to hear the skald’s performance. The effect persists for as long as the enemy is within 30 feet and the skald continues his performance. This cannot cause a creature to become frightened or panicked, even if the targets are already shaken from another effect. This is a sonic mind-affecting fear effect, and relies on audible components.");
            dirge_of_doom = Helpers.CreateFeature("SkaldDirgeOfDoomFeature",
                                                  dirge_of_doom_ability.Name,
                                                  dirge_of_doom_ability.Description,
                                                  "",
                                                  dirge_of_doom_ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(dirge_of_doom_ability)
                                                  );
        }


        static void createSongOfStrength()
        {
            var strength_surge = library.Get<BlueprintAbility>("1d6364123e1f6a04c88313d83d3b70ee");
            var song_of_strength_buff = library.CopyAndAdd<BlueprintBuff>("1fa5f733fa1d77743bf54f5f3da5a6b1", "SkaldSongOfStrengthEffectBuff", "");
            song_of_strength_buff.SetName("Song of Strength");
            song_of_strength_buff.SetDescription("At 6th level, a skald can use raging song to inspire his allies to superhuman feats of strength. Once each round while the skald uses this performance, allies within 30 feet who can hear the skald may add 1/2 the skald’s level to a Strength check or Strength-based skill check.");
            song_of_strength_buff.SetIcon(strength_surge.Icon);
            song_of_strength_buff.ComponentsArray = new BlueprintComponent[]{Helpers.CreateAddContextStatBonus(StatType.SkillAthletics, ModifierDescriptor.UntypedStackable,
                                                                                                               ContextValueType.Rank, AbilityRankType.Default),
                                                                             Common.createAbilityScoreCheckBonus(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                                                 ModifierDescriptor.UntypedStackable, StatType.Strength),
                                                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                             progression: ContextRankProgression.Div2,
                                                                                                             type: AbilityRankType.Default,
                                                                                                             classes: getSkaldArray()
                                                                                                             )
                                                                             };
            var song_of_strength_ability = Common.convertPerformance(library.Get<BlueprintActivatableAbility>("430ab3bb57f2cfc46b7b3a68afd4f74e"), song_of_strength_buff, "SkaldSongOfStrength");
            song_of_strength_ability.DeactivateIfCombatEnded = !test_mode;

            song_of_strength = Helpers.CreateFeature("SkaldSongOfStrengthFeature",
                                                     song_of_strength_buff.Name,
                                                     song_of_strength_buff.Description,
                                                     "",
                                                     song_of_strength_buff.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(song_of_strength_ability)
                                                     );
        }


        static void createSongOfMarching()
        {
            var longstrider = library.Get<BlueprintAbility>("14c90900b690cac429b229efdf416127");
            var song_of_marching_ability = library.CopyAndAdd<BlueprintAbility>("9b7fa6cadc0349449829873c63cc5b0b", "SkaldSongOfMarchingAbility", "");
            song_of_marching_ability.SetName("Song of Marching");
            song_of_marching_ability.SetDescription("At 3rd level, a skald can use raging song to inspire his allies to move without suffering from fatigue. By expending 6 rounds of raging song, skalds extends the time he and his allies can go without a rest before becoming fatigued for 6 hours. The effect lasts for one day.") ;
            song_of_marching_ability.SetIcon(longstrider.Icon);
            song_of_marching_ability.ReplaceComponent<AbilityResourceLogic>(c => c.Amount = 6);
            song_of_marching_ability.ReplaceComponent<AbilitySpawnFx>(longstrider.GetComponent<AbilitySpawnFx>());

            var song_of_marching_buff = Helpers.CreateBuff("SkaldSongOfMarchingBuff",
                                                           song_of_marching_ability.Name,
                                                           song_of_marching_ability.Description,
                                                           "",
                                                           song_of_marching_ability.Icon,
                                                           null,
                                                           Common.createAddWearinessHours(-6)
                                                           );
            song_of_marching_buff.Stacking = StackingType.Stack;
            song_of_marching_buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var apply_buff = Common.createContextActionApplyBuff(song_of_marching_buff,
                                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(24), DurationRate.Hours),
                                                                 dispellable: false);
            song_of_marching_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(apply_buff));
            song_of_marching_ability.RemoveComponents<ContextRankConfig>();

            song_of_marching = Helpers.CreateFeature("SkaldSongOfMarchingFeature",
                                                     song_of_marching_ability.Name,
                                                     song_of_marching_ability.Description,
                                                      "",
                                                      song_of_marching_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(song_of_marching_ability)
                                                     );
        }


        static void createSkaldRagePowersFeature()
        {
            var barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");
            skald_rage_powers = library.CopyAndAdd<BlueprintFeatureSelection>("28710502f46848d48b3f0d6132817c4e", "SkaldRagePowerSelection", "");
            skald_rage_powers.SetDescription("At 3rd level and every 3 levels thereafter, a skald learns a rage power that affects the skald and any allies under the influence of his inspired rage. This cannot be a rage power that requires the creature to spend a standard action or rounds of rage to activate it, nor can it be a stance power.\n"
                                             + "When starting an inspired rage, the skald adds rage powers (if any) to the song, and all affected allies gain the benefit of these rage powers, using the skald’s level as their effective barbarian level. The skald uses his skald level as his barbarian level for the purpose of selecting rage powers that require a minimum barbarian level. If the rage power’s effects depend on the skald’s ability modifier(such as lesser spirit totem), affected allies use the skald’s ability modifier instead of their own for the purposes of this effect.") ;
            skald_rage_powers.AddComponent(Common.createClassLevelsForPrerequisites(barbarian_class, skald_class));

            //fix rage buffs to work with skald
            BlueprintBuff[] buffs_to_fix = new BlueprintBuff[]{library.Get<BlueprintBuff>("ec7db4946877f73439c4ee661f645452"), //beast totem ac buff
                                                               library.Get<BlueprintBuff>("3858dd3e9a94f0b41abdc58387d68ccf"), //guarded stance
                                                               library.Get<BlueprintBuff>("5b5e566167a3f2746b7d3a26bc8a50a6"), //guarded stance protect vitals
                                                               library.Get<BlueprintBuff>("b209f567dc78a1440aad52d4138c5f27"), //reflexive dodge
                                                               library.Get<BlueprintBuff>("0c6e198a78210954c9fe245a26b0c315"), //deadly accuracy
                                                               library.Get<BlueprintBuff>("9ec69854596674a4ba40802e6337894d"), //inspire ferocity buff
                                                               library.Get<BlueprintBuff>("c6271b3183c48d54b8defd272bea0665"), //lethal stance
                                                               library.Get<BlueprintBuff>("a8a733d2605c66548b652f312ea4dbf3"), //reckless stance
                                                              };
            foreach (var b in buffs_to_fix)
            {
                var c = b.GetComponent<ContextRankConfig>();
                BlueprintCharacterClass[] classes = Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class");
                classes = classes.AddToArray(skald_class);
                Helpers.SetField(c, "m_Class", classes);
                replaceContextConditionHasFactToContextConditionCasterHasFact(b);
            }
            var renewed_vigor = library.Get<BlueprintAbility>("5a25185dbf75a954580a1248dc694cfc");
            var context_rank_configs = renewed_vigor.GetComponents<ContextRankConfig>();
            foreach (var config in context_rank_configs)
            {
                var t = Helpers.GetField<ContextRankBaseValueType>(config, "m_BaseValueType");
                if (t == ContextRankBaseValueType.ClassLevel)
                {
                    BlueprintCharacterClass[] classes = Helpers.GetField<BlueprintCharacterClass[]>(config, "m_Class");
                    classes = classes.AddToArray(skald_class);
                    Helpers.SetField(config, "m_Class", classes);
                }
            }
        }


        static void createSkaldDamageReduction()
        {
            var damage_reduction_barbarian = library.Get<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8");
            damage_reduction = library.CopyAndAdd<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8", "SkaldDamageReduction", "");
            damage_reduction.SetDescription("At 9th level, a skald gains damage reduction.Subtract 1 from the damage the skald takes each time he is dealt damage from a weapon or a natural attack.At 14th and 19th levels, this damage reduction increases by 1.Damage Reduction can reduce damage to 0, but not below 0.Additionally, the skald grants this DR to all allies affected by his inspired rage.");

            var c = damage_reduction.GetComponent<ContextRankConfig>();
            BlueprintFeature[] feats = Helpers.GetField<BlueprintFeature[]>(c, "m_FeatureList");
            feats = feats.AddToArray(damage_reduction);
            Helpers.SetField(c, "m_FeatureList", feats);
            
            var increase_damage_reduction = library.Get<BlueprintFeature>("ddaee203ee4dcb24c880d633fbd77db6");
            //allow to dr share feats (only from skald)
            var dr_share_buff = Helpers.CreateBuff("SkaldSharedDr",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          damage_reduction.GetComponent<AddDamageResistancePhysical>(),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks,
                                                                          featureList: new BlueprintFeature[]{ damage_reduction, increase_damage_reduction, increase_damage_reduction})
                                          );
            dr_share_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var share_dr_condition = Helpers.CreateConditional(new Condition[] { Common.createContextConditionIsCaster(not: true) },
                                                             Common.createContextActionApplyBuff(dr_share_buff, Helpers.CreateContextDuration(), false, true, true),
                                                             null
                                                            );

            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(inspired_rage_effect_buff, dr_share_buff, share_dr_condition);
        }



        static void createVersatilePerformance()
        {
            versatile_performance = library.CopyAndAdd<BlueprintFeatureSelection>("94e2cd84bf3a8e04f8609fe502892f4f", "SkaldVersatilePerformance", "");
            versatile_performance.SetName("Skald Talent");
            versatile_performance.SetDescription("As a skald gains experience, she learns a number of talents that aid her and confound her foes. At 2nd level, a skald gains a rogue talent, as the rogue class feature of the same name. At 6th level and every 4 levels thereafter, the skald gains an additional rogue talent. A skald cannot select a rogue talent that modifies the sneak attack ability.");
        }

        static void createWellVersed()
        {
            well_versed = library.CopyAndAdd<BlueprintFeature>("8f4060852a4c8604290037365155662f", "SkaldWelLVersed", "");
            well_versed.SetDescription("At 2nd level, the skald becomes resistant to the bardic performance of others, and to sonic effects in general. The bard gains a +4 bonus on saving throws made against bardic performance, sonic, and language-dependent effects.");
        }


        static void createPerfromanceResource()
        {
            performance_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            var amount = Helpers.GetField(performance_resource, "m_MaxAmount");
            BlueprintCharacterClass[] classes = (BlueprintCharacterClass[])Helpers.GetField(amount, "Class");
            classes = classes.AddToArray(skald_class);
            Helpers.SetField(amount, "Class", classes);
            Helpers.SetField(performance_resource, "m_MaxAmount", amount);

            give_performance_resource = library.CopyAndAdd<BlueprintFeature>("b92bfc201c6a79e49afd0b5cfbfc269f", "SkaldPerformanceResourceFact", "");
            give_performance_resource.ReplaceComponent<IncreaseResourcesByClass>(c => { c.CharacterClass = skald_class;});

            //extra performance feat is fixed automatically
        }


        static BlueprintSpellbook createSkaldSpellbook()
        {
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var skald_spellbook = Helpers.Create<BlueprintSpellbook>();
            skald_spellbook.name = "SkaldSpellbook";
            library.AddAsset(skald_spellbook, "");
            skald_spellbook.Name = skald_class.LocalizedName;
            skald_spellbook.SpellsPerDay = bard_class.Spellbook.SpellsPerDay;

            skald_spellbook.SpellsKnown = bard_class.Spellbook.SpellsKnown;
            skald_spellbook.Spontaneous = true;
            skald_spellbook.IsArcane = true;
            skald_spellbook.AllSpellsKnown = false;
            skald_spellbook.CanCopyScrolls = false;
            skald_spellbook.CastingAttribute = StatType.Charisma;
            skald_spellbook.CharacterClass = skald_class;
            skald_spellbook.CasterLevelModifier = 0;
            skald_spellbook.CantripsType = CantripsType.Cantrips;
            skald_spellbook.SpellsPerLevel = 0;
            skald_spellbook.SpellList = bard_class.Spellbook.SpellList;
           
            return skald_spellbook;
        }


        static void createMasterSkald()
        {
            var inspire_greatness = library.Get<BlueprintFeature>("9ae0f32c72f8df84dab023d1b34641dc");
            master_skald = Helpers.CreateFeature("SkaldMasterSkald",
                                                 "Master Skald",
                                                 "At 20th level, a skald’s inspired rage no longer gives allies a penalty to AC, nor limits what skills or abilities they can use. Finally, when making a full attack, affected allies may make an additional attack each round (as if using a haste effect).",
                                                 "",
                                                 inspire_greatness.Icon,
                                                 FeatureGroup.None);

            master_skald_buff = Helpers.CreateBuff("SkaldMasterSkaldBuff",
                                                   master_skald.Name,
                                                   master_skald.Description,
                                                   "",
                                                   master_skald.Icon,
                                                   null,
                                                   Common.createBuffExtraAttack(1, true)
                                                   );
        }


        static void createInspiredRage()
        {
            createInspiredRageEffectBuff();
            var inspire_courage = library.Get<BlueprintActivatableAbility>("5250fe10c377fdb49be449dfe050ba70");

            inspired_rage_effect_buff.SetIcon(inspire_courage.Icon);
            inspired_rage_effect_buff.SetName("Inspired Rage");
            inspired_rage_effect_buff.SetDescription("At 1st level, affected allies gain a +2 morale bonus to Strength and Constitution and a +1 morale bonus on Will saving throws, but also take a –1 penalty to AC. While under the effects of inspired rage, allies other than the skald cannot use any ability that requires patience or concentration. At 4th level and every 4 levels thereafter, the song’s bonuses on Will saves increase by 1; the penalty to AC doesn’t change. At 8th and 16th levels, the song’s bonuses to Strength and Constitution increase by 2. (Unlike the barbarian’s rage ability, those affected are not fatigued after the song ends.)");
            inspired_rage = Common.convertPerformance(inspire_courage, inspired_rage_effect_buff, "SkaldInspiredRage");

            inspired_rage_feature = Helpers.CreateFeature("SkaldInspiredRageFeature",
                                                          inspired_rage.Name,
                                                          inspired_rage.Description,
                                                          "",
                                                          inspired_rage.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFact(inspired_rage)
                                                          );
        }   


        static internal void createInspiredRageEffectBuff()
        {
            inspired_rage_effect_buff = library.CopyAndAdd<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613", "SkaldInspiredRageEffectBuff", "");
            //in AddFactContextActions in Activated we will need to replace all ContextConditionHasFact with ContextConditionCasterHasFact
            //moreover if fact is a switch buff, then in its AddFactContextActions we will also need to add  inspired rage buff to StandardRage
            //remove all logic in NewRound (since we do not need to count number of rage rounds for after rage fatigue)
            //in Deactivated remove Conditional (to apply fatigue)
            //also add skald to all contexts
            var standard_rage_buff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            replaceContextConditionHasFactToContextConditionCasterHasFact(inspired_rage_effect_buff, standard_rage_buff, inspired_rage_effect_buff, "Skald");

            var component =  inspired_rage_effect_buff.GetComponent<AddFactContextActions>();
            component.NewRound = Helpers.CreateActionList();

            var deactivate_actions = component.Deactivated.Actions;
            component.Deactivated = Helpers.CreateActionList(deactivate_actions.RemoveFromArrayByType<Kingmaker.ElementsSystem.GameAction, Conditional>());

            //clear everything
            inspired_rage_effect_buff.RemoveComponents<TemporaryHitPointsPerLevel>();
            inspired_rage_effect_buff.RemoveComponents<AttackTypeAttackBonus>();
            inspired_rage_effect_buff.RemoveComponents<WeaponGroupDamageBonus>();
            inspired_rage_effect_buff.RemoveComponents<SpellDescriptorComponent>();
            inspired_rage_effect_buff.RemoveComponents<WeaponAttackTypeDamageBonus>();
            inspired_rage_effect_buff.RemoveComponents<ContextCalculateSharedValue>();
            inspired_rage_effect_buff.RemoveComponents<AddContextStatBonus>();
            inspired_rage_effect_buff.RemoveComponents<ContextRankConfig>();


            inspired_rage_effect_buff.AddComponent(Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.UntypedStackable, ContextValueType.Rank, AbilityRankType.DamageBonus));
            inspired_rage_effect_buff.AddComponent(Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.Default));
            inspired_rage_effect_buff.AddComponent(Helpers.CreateAddContextStatBonus(StatType.Strength, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.StatBonus, 2));
            inspired_rage_effect_buff.AddComponent(Helpers.CreateAddContextStatBonus(StatType.Constitution, ModifierDescriptor.Morale, ContextValueType.Rank, AbilityRankType.StatBonus, 2));
            var ac_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus,
                                                               baseValueType: ContextRankBaseValueType.FeatureListRanks,
                                                               progression: ContextRankProgression.BonusValue,
                                                               stepLevel: -1,
                                                               featureList: new BlueprintFeature[] {master_skald}
                                                               );
            var will_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.Default,
                                                                           baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                           classes: getSkaldArray(),
                                                                           progression: ContextRankProgression.OnePlusDivStep,
                                                                           stepLevel: 4);
            var stat_context_rank_config = Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus,
                                                                           baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                           classes: getSkaldArray(),
                                                                           progression: ContextRankProgression.OnePlusDivStep,
                                                                           stepLevel: 8);
            inspired_rage_effect_buff.AddComponent(will_context_rank_config);
            inspired_rage_effect_buff.AddComponent(stat_context_rank_config);
            inspired_rage_effect_buff.AddComponent(ac_context_rank_config);
            inspired_rage_effect_buff.RemoveComponents<ForbidSpellCasting>();


            var no_spell_casting_buff = Helpers.CreateBuff("SkaldForbidSpellCastingBuff",
                                                           "",
                                                           "",
                                                           "",
                                                           null,
                                                           null,
                                                           Helpers.Create<ForbidSpellCasting>()
                                                           );
            no_spell_casting_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var forbid_condition = Helpers.CreateConditional(new Condition[] { Common.createContextConditionIsCaster(not: true), Common.createContextConditionCasterHasFact(master_skald, has:false)},
                                                             Common.createContextActionApplyBuff(no_spell_casting_buff, Helpers.CreateContextDuration(), false, true, true),
                                                             null
                                                            );
            var master_skald_condition = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(master_skald, has: true),
                                                 Common.createContextActionApplyBuff(master_skald_buff, Helpers.CreateContextDuration(), false, true, true),
                                                 null
                                                );

            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(inspired_rage_effect_buff, no_spell_casting_buff, forbid_condition);
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(inspired_rage_effect_buff, master_skald_buff, master_skald_condition);
        }


        static void replaceContextConditionHasFactToContextConditionCasterHasFact(BlueprintBuff buff, BlueprintUnitFact inner_buff_to_locate = null,
                                                                                  BlueprintUnitFact inner_buff_to_add = null, string prefix = "")
        {
            var personal_buffs = new BlueprintUnitFact[] {library.Get<BlueprintBuff>("c52e4fdad5df5d047b7ab077a9907937"), //reckless stance
                                                     library.Get<BlueprintBuff>("16649b2e80602eb48bbeaad77f9f365f"), //lethal stance
                                                     library.Get<BlueprintBuff>("fd0fb6aef4000a443bdc45363410e377"), //guarded stance
                                                     library.Get<BlueprintBuff>("4b3fb3c9473a00f4fa526f4bd3fc8b7a") //inspire ferocity
                                                     };
            List<BlueprintUnitFact> patched_buffs = new List<BlueprintUnitFact>();
            var component = buff.GetComponent<AddFactContextActions>();
            if (component == null)
            {
                return;
            }
            component = component.CreateCopy();
            var activated_actions = component.Activated.Actions;

            for (int i = 0; i < activated_actions.Length; i++)
            {
                if (activated_actions[i] is Conditional)
                {
                    var new_a = (Conditional)activated_actions[i].CreateCopy();
                    new_a.ConditionsChecker = Helpers.CreateConditionsCheckerAnd(new_a.ConditionsChecker.Conditions);
                    bool is_personal = false;
                    for (int j = 0; j < new_a.ConditionsChecker.Conditions.Length; j++)
                    {
                        if (new_a.ConditionsChecker.Conditions[j] is ContextConditionHasFact)
                        {
                            var condition_entry = (ContextConditionHasFact)new_a.ConditionsChecker.Conditions[j];
                            var fact = condition_entry.Fact;

                            if (fact is BlueprintBuff && personal_buffs.Contains(fact))
                            {
                                is_personal = true;
                            }
                            if (fact is BlueprintBuff && inner_buff_to_locate != null && !patched_buffs.Contains(fact))
                            {
                                Common.addToFactInContextConditionHasFact((BlueprintBuff)(fact), inner_buff_to_locate, Common.createContextConditionCasterHasFact(inner_buff_to_add));
                                patched_buffs.Add(fact);
                            }
                            new_a.ConditionsChecker.Conditions[j] = Common.createContextConditionCasterHasFact(fact, !condition_entry.Not);
                        }
                    }
                    if (is_personal)
                    {
                        //personal buff
                        new_a.ConditionsChecker.Conditions = new_a.ConditionsChecker.Conditions.AddToArray(Common.createContextConditionIsCaster());
                    }
                    activated_actions[i] = new_a;
                }
            }
            component.Activated.Actions = activated_actions;
            buff.ReplaceComponent<AddFactContextActions>(component);
        }




    }
}
