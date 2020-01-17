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
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System.Linq;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.RuleSystem;

namespace CallOfTheWild
{
    public class Rebalance
    {
        static LibraryScriptableObject library => Main.library;



        internal static void fixSpellDescriptors()
        {
            //fiery body
            library.Get<BlueprintAbility>("08ccad78cac525040919d51963f9ac39").GetComponent<SpellDescriptorComponent>().Descriptor = SpellDescriptor.Fire;
        }

        internal static void fixAnimalCompanion()
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


        internal static void fixLegendaryProportionsAC()
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


        internal static void removeJudgement19FormSHandMS()
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


        internal static void fixSkillPoints()
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
            class_skill_map.Add(new KeyValuePair<string, int>("c75e0971973957d4dbad24bc7957e4fb", 2));//slayer
            class_skill_map.Add(new KeyValuePair<string, int>("90e4d7da3ccd1a8478411e07e91d5750", 2));//swordlord
            foreach (var class_skill in class_skill_map)
            {
                var current_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>(class_skill.Key);
                current_class.SkillPoints = class_skill.Value;
            }


            var skilled_human = library.Get<BlueprintFeature>("3adf9274a210b164cb68f472dc1e4544");
            var skilled_half_orc = library.Get<BlueprintFeature>("4e8fe10f42f314e4fa7c7918bcfadbd5");
            skilled_human.ReplaceComponent<AddSkillPoint>(Helpers.Create<NewMechanics.AddSkillPointOnEvenLevels>());
            skilled_human.SetDescription("Humans gain an additional skill rank at every even level.");

            skilled_half_orc.ReplaceComponent<AddSkillPoint>(Helpers.Create<NewMechanics.AddSkillPointOnEvenLevels>());
            skilled_half_orc.SetDescription("Half-orcs gain an additional skill rank at every even level.");
        }

        internal static void fixCompanions()
        {
            //change stats of certain companions
            //Valerie 
            var valerie_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("54be53f0b35bf3c4592a97ae335fe765");
            valerie_companion.Strength = 16;//+2
            valerie_companion.Dexterity = 10;
            valerie_companion.Constitution = 14;
            valerie_companion.Intelligence = 13;
            valerie_companion.Wisdom = 10;
            valerie_companion.Charisma = 15;
            var valerie1_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("912444657701e2d4ab2634c3d1e130ad");
            var valerie_class_level = valerie1_feature.GetComponent<AddClassLevels>();
            valerie_class_level.CharacterClass = VindicativeBastard.vindicative_bastard_class;
            valerie_class_level.Archetypes = new BlueprintArchetype[0];
            valerie_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Strength;
            valerie_class_level.Selections[0].Features[1] = valerie_class_level.Selections[0].Features[2];
            valerie_class_level.Skills = new StatType[] { StatType.SkillPersuasion, StatType.SkillLoreReligion, StatType.SkillAthletics };
            valerie_companion.Body.PrimaryHand = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("571c56d11dafbb04094cbaae659974b5");//longsword
            valerie_companion.Body.SecondaryHand = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Shields.BlueprintItemShield>("f4cef3ba1a15b0f4fa7fd66b602ff32b");//shield
            valerie1_feature.GetComponent<AddFacts>().Facts = valerie1_feature.GetComponent<AddFacts>().Facts.Skip(1).ToArray();
            //change amiri stats
            var amiri_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b3f29faef0a82b941af04f08ceb47fa2");
            amiri_companion.Strength = 17;//+2
            amiri_companion.Dexterity = 12;
            amiri_companion.Constitution = 16;
            amiri_companion.Intelligence = 10;
            amiri_companion.Wisdom = 12;
            amiri_companion.Charisma = 8;
            var amiri1_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("df943986ee329e84a94360f2398ae6e6");
            var amiri_class_level = amiri1_feature.GetComponent<AddClassLevels>();
            amiri_class_level.Archetypes = new BlueprintArchetype[] { library.Get<BlueprintArchetype>("a2ccb759dc6f1f94d9aae8061509bf87") };
            amiri_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Strength;
            amiri_class_level.Selections[0].Features[1] = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
            //amiri_class_level.Selections[0].Features[1] = NewFeats.furious_focus;
            amiri_class_level.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillAthletics, StatType.SkillMobility };
            //change tristian stats
            var tristian_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f6c23e93512e1b54dba11560446a9e02");
            tristian_companion.Strength = 10;
            tristian_companion.Dexterity = 14;
            tristian_companion.Constitution = 12;
            tristian_companion.Intelligence = 10;
            tristian_companion.Wisdom = 17;
            tristian_companion.Charisma = 14;
            var tristian_level = tristian_companion.GetComponent<AddClassLevels>();
            tristian_level.Selections[2].Features[0] = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35");//fire as primary
            tristian_level.Selections[4].Features[1] = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");//improved initiative
            tristian_level.Selections[4].Features[2] = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("16fa59cc9a72a6043b566b49184f53fe");//spell focus
            tristian_level.Selections[5].ParamSpellSchool = SpellSchool.Evocation;
            tristian_level.Selections[6].ParamSpellSchool = SpellSchool.Evocation;

            var harrim_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("aab03d0ab5262da498b32daa6a99b507");
            harrim_companion.Strength = 17;
            harrim_companion.Constitution = 12;
            harrim_companion.Charisma = 10;
            harrim_companion.Wisdom = 16;
            harrim_companion.Dexterity = 10;
            harrim_companion.Body.PrimaryHandAlternative1 = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("7f7c8e1e4fdd99e438b30ed9622e9e3f");//heavy flail


            var harrim_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("8910febae2a7b9f4ba5eca4dde1e9649");
            harrim_feature.GetComponent<AddFacts>().Facts = harrim_feature.GetComponent<AddFacts>().Facts.Skip(1).ToArray();

            var harrim_class_level = harrim_feature.GetComponent<AddClassLevels>();
            harrim_class_level.CharacterClass = Warpriest.warpriest_class;
            harrim_class_level.Selections[0].Features = new BlueprintFeature[] { NewFeats.weapon_of_the_chosen,
                                                                                 NewFeats.improved_weapon_of_the_chosen,
                                                                                 NewFeats.greater_weapon_of_the_chosen,
                                                                                 library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5"), //pa
                                                                                 NewFeats.furious_focus,
                                                                                 library.Get<BlueprintFeature>("31470b17e8446ae4ea0dacd6c5817d86"), //ws
                                                                                 library.Get<BlueprintParametrizedFeature>("7cf5edc65e785a24f9cf93af987d66b3"), //gws
                                                                                 library.Get<BlueprintParametrizedFeature>("09c9e82965fb4334b984a1e9df3bd088"), //gwf
                                                                                 library.Get<BlueprintFeature>("afd05ca5363036c44817c071189b67e1"), //gsf
                                                                                 Warpriest.extra_channel
                                                                                 };
            harrim_class_level.Selections[2].Selection = Warpriest.warpriest_energy_selection;
            harrim_class_level.Selections[2].Features = new BlueprintFeature[] { Warpriest.warpriest_spontaneous_heal };
            harrim_class_level.Selections[3].Selection = Warpriest.warpriest_blessings;
            harrim_class_level.Selections[3].Features = new BlueprintFeature[] { Warpriest.blessings_map["WarpriestBlessingChaos"], Warpriest.blessings_map["WarpriestBlessingDestruction"] };
            harrim_class_level.Selections[4].Selection = Warpriest.fighter_feat;
            harrim_class_level.Selections[4].Features = new BlueprintFeature[] {  NewFeats.weapon_of_the_chosen,
                                                                                 NewFeats.improved_weapon_of_the_chosen,
                                                                                 NewFeats.greater_weapon_of_the_chosen,
                                                                                 library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5"), //pa
                                                                                 NewFeats.furious_focus,
                                                                                 library.Get<BlueprintFeature>("31470b17e8446ae4ea0dacd6c5817d86"), //ws
                                                                                 library.Get<BlueprintParametrizedFeature>("7cf5edc65e785a24f9cf93af987d66b3"), //gws
                                                                                 library.Get<BlueprintParametrizedFeature>("09c9e82965fb4334b984a1e9df3bd088"), //gwf
                                                                                 library.Get<BlueprintFeature>("afd05ca5363036c44817c071189b67e1"), //gsf
                                                                                };
            harrim_class_level.Selections[5].IsParametrizedFeature = false;
            harrim_class_level.Selections[5].Selection = Warpriest.weapon_focus_selection;
            harrim_class_level.Selections[5].Features = new BlueprintFeature[] { library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e") }; //wf
            harrim_class_level.Selections[6].ParametrizedFeature = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"); //wf
            harrim_class_level.Selections[6].ParamWeaponCategory = WeaponCategory.HeavyFlail;
            harrim_class_level.Selections[7].ParametrizedFeature = library.Get<BlueprintParametrizedFeature>("31470b17e8446ae4ea0dacd6c5817d86"); //ws
            harrim_class_level.Selections[7].ParamWeaponCategory = WeaponCategory.HeavyFlail;
            harrim_class_level.Selections[8].IsParametrizedFeature = false;
            harrim_class_level.Selections[8].Selection = library.Get<BlueprintFeatureSelection>("76d4885a395976547a13c5d6bf95b482"); //af
            harrim_class_level.Selections[8].Features = new BlueprintFeature[] { library.Get<BlueprintFeature>("c27e6d2b0d33d42439f512c6d9a6a601") }; //heavy
            harrim_class_level.Selections[9].ParametrizedFeature = library.Get<BlueprintParametrizedFeature>("09c9e82965fb4334b984a1e9df3bd088"); //gwf
            harrim_class_level.Selections[9].ParamWeaponCategory = WeaponCategory.HeavyFlail;

            harrim_feature.GetComponent<AddFacts>().Facts =  harrim_feature.GetComponent<AddFacts>().Facts.Take(1).ToArray();
            //harrim_class_level.Selections[3].Features[0] = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("9ebe166b9b901c746b1858029f13a2c5"); //madness domain instead of chaos

            //change linzi
            var linzi_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("77c11edb92ce0fd408ad96b40fd27121");
            linzi_companion.Dexterity = 15;
            linzi_companion.Charisma = 16;
            linzi_companion.Constitution = 12;
            linzi_companion.Strength = 11;
            //change octavia
            var octavia_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f9161aa0b3f519c47acbce01f53ee217");
            octavia_companion.Dexterity = 16;
            octavia_companion.Intelligence = 16;
            octavia_companion.Constitution = 10;

            //change regongar
            var regognar_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b090918d7e9010a45b96465de7a104c3");
            regognar_companion.Dexterity = 12;
            ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("12ee53c9e546719408db257f489ec366").GetComponent<AddClassLevels>().Levels = 1;
            //change ekun
            var ekun_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("d5bc1d94cd3e5be4bbc03f3366f67afc");
            ekun_companion.Strength = 14;
            ekun_companion.Constitution = 12;
            ekun_companion.Dexterity = 17;
            ekun_companion.Wisdom = 14;
            ekun_companion.Charisma = 10;
            var ekun_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0bc6dc9b6648a744899752508addae8c");
            var ekun_class_level = ekun_feature.GetComponent<AddClassLevels>();
            ekun_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Dexterity;
            //change jubilost
            var jubilost_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("3f5777b51d301524c9b912812955ee1e");
            jubilost_companion.Dexterity = 16;
            jubilost_companion.Wisdom = 10;
            jubilost_companion.Intelligence = 17;
            jubilost_companion.Constitution = 12;
            //change nok-nok
            var noknok_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f9417988783876044b76f918f8636455");
            noknok_companion.Strength = 11;
            noknok_companion.Constitution = 14;
            noknok_companion.Wisdom = 10;
            noknok_companion.GetComponent<AddClassLevels>().Levels = 1;
            noknok_companion.GetComponent<AddClassLevels>().Skills = new StatType[] { StatType.SkillMobility, StatType.SkillThievery, StatType.SkillPerception, StatType.SkillStealth, StatType.SkillUseMagicDevice, StatType.SkillLoreNature, StatType.SkillAthletics };
            //change jaethal
            var jaethal_feature_list = library.Get<BlueprintFeature>("34280596dd550074ca55bd15285451b3");
            var jaethal_selections = jaethal_feature_list.GetComponent<AddClassLevels>();
            jaethal_selections.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillMobility, StatType.SkillLoreReligion, StatType.SkillAthletics };
            jaethal_selections.Selections[1].Features = jaethal_selections.Selections[1].Features.Skip(1).ToArray();

        }


        internal static void fixMissingSlamProficiency()
        {

            Action<UnitDescriptor> add_slam_proficiency = delegate (UnitDescriptor u)
            {
                if (!u.Proficiencies.Contains(WeaponCategory.OtherNaturalWeapons))
                {
                    u.Proficiencies.Add(WeaponCategory.OtherNaturalWeapons);
                }
            };

            SaveGameFix.save_game_actions.Add(add_slam_proficiency);
        }

        internal static void fixWebSchool()
        {
            var web = library.Get<BlueprintAbility>("134cb6d492269aa4f8662700ef57449f");
            web.GetComponent<SpellComponent>().School = SpellSchool.Conjuration;

            library.Get<BlueprintSpellList>("ac551db78c1baa34eb8edca088be13cb").SpellsByLevel[2].Spells.Add(web); //add to lust
            library.Get<BlueprintSpellList>("17c0bfe5b7c8ac3449da655cdcaed4e7").SpellsByLevel[2].Spells.Remove(web); //remove from wrath

            library.Get<BlueprintSpellList>("69a6eba12bc77ea4191f573d63c9df12").SpellsByLevel[2].Spells.Add(web); //add to conjuration
            library.Get<BlueprintSpellList>("becbcfeca9624b6469319209c2a6b7f1").SpellsByLevel[2].Spells.Remove(web);//remove from conjuration
        }


        internal static void fixJudgments()
        {
            //fix smiting buffs
            library.Get<BlueprintActivatableAbility>("72fe16312b4479145afc6cc6c87cd08f").Buff = library.Get<BlueprintBuff>("481b03bc6cbc5af448b1f6cb70d88859");//alignment
            library.Get<BlueprintActivatableAbility>("2c448ab4135c7c741b6f0f223901f9fa").Buff = library.Get<BlueprintBuff>("2e3f01df36b508b4e9186bab7a337dfa");//adamantite
        }

        internal static void fixDomains()
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


        internal static void fixBarbarianRageAC()
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


        internal static void fixMagicVestment()
        {
            fixMagicVestmentArmor();
            fixMagicVestmentShield();
        }

        static void fixMagicVestmentArmor()
        {
            var magic_vestement_armor_buff = Main.library.Get<BlueprintBuff>("9e265139cf6c07c4fb8298cb8b646de9");
            var armor_enchant = Helpers.Create<NewMechanics.EnchantmentMechanics.BuffContextEnchantArmor>();
            armor_enchant.value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
            armor_enchant.enchantments = ArmorEnchantments.temporary_armor_enchantments;

            magic_vestement_armor_buff.ComponentsArray = new BlueprintComponent[] {armor_enchant,
                                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel,
                                                                                   progression: ContextRankProgression.DivStep, startLevel: 4, min:1, stepLevel: 4, max: 5, type: AbilityRankType.StatBonus)
                                                                                  };
            magic_vestement_armor_buff.Stacking = StackingType.Replace;

            library.Get<BlueprintAbility>("956309af83352714aa7ee89fb4ecf201").AddComponent(Helpers.Create<NewMechanics.AbilitTargetHasArmor>());
        }


        static void fixMagicVestmentShield()
        {
            var magic_vestement_shield_buff = Main.library.Get<BlueprintBuff>("2e8446f820936a44f951b50d70a82b16");
            var shield_enchant = Helpers.Create<NewMechanics.EnchantmentMechanics.BuffContextEnchantShield>();
            shield_enchant.value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
            shield_enchant.enchantments = ArmorEnchantments.temporary_shield_enchantments;

            magic_vestement_shield_buff.ComponentsArray = new BlueprintComponent[] {shield_enchant,
                                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel,
                                                                                   progression: ContextRankProgression.DivStep, startLevel: 4, min:1, stepLevel: 4, max: 5, type: AbilityRankType.StatBonus)
                                                                                  };
            magic_vestement_shield_buff.Stacking = StackingType.Replace;
            library.Get<BlueprintAbility>("adcda176d1756eb45bd5ec9592073b09").AddComponent(Helpers.Create<NewMechanics.AbilitTargetHasShield>());
        }

        internal static BlueprintFeatureSelection dd_feat_subselection;
        internal static void fixDragonDiscipleBonusFeat()
        {
            //to allow select feats from other bloodline classes (bloodrager for example)
            var dd_feat_selection = Main.library.Get<BlueprintFeatureSelection>("f4b011d090e8ae543b1441bd594c7bf7");
            dd_feat_subselection = Main.library.CopyAndAdd<BlueprintFeatureSelection>("f4b011d090e8ae543b1441bd594c7bf7", "DragonDiscipleDraconicFeatSubselection", "");

            dd_feat_subselection.Features = new BlueprintFeature[] { dd_feat_selection };
            dd_feat_subselection.AllFeatures = dd_feat_subselection.Features;
            dd_feat_subselection.SetDescription("Upon reaching 2nd level, and every three levels thereafter, a dragon disciple receives one bonus feat, chosen from the draconic bloodline’s bonus feat list.");

            var dragon_disciple_progression = Main.library.Get<BlueprintProgression>("69fc2bad2eb331346a6c777423e0d0f7");
            foreach (var le in dragon_disciple_progression.LevelEntries)
            {
                for (int i = 0; i < le.Features.Count; i++)
                {
                    if (le.Features[i] == dd_feat_selection)
                    {
                        le.Features[i] = dd_feat_subselection;
                    }
                }
            }

        }


        static internal void fixNaturalACStacking()
        {
            ModifiableValue.DefaultStackingDescriptors.Remove(ModifierDescriptor.NaturalArmor);
            //replace natural armor on dd to racial to allow it to stack
            ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("aa4f9fd22a07ddb49982500deaed88f9").GetComponent<AddStatBonus>().Descriptor = ModifierDescriptor.Racial;
        }


        static internal void fixInspiredFerocity()
        {
            var reckless_stance_switch = Main.library.Get<BlueprintBuff>("c52e4fdad5df5d047b7ab077a9907937");
            var inspire_ferocity_rage_buff = Main.library.Get<BlueprintBuff>("9a8a16f5734eec7439c5c77000316742");
            var inspire_ferocity_switch_buff = Main.library.Get<BlueprintBuff>("4b3fb3c9473a00f4fa526f4bd3fc8b7a");
            var c = reckless_stance_switch.GetComponent<AddFactContextActions>();
            c.Deactivated.Actions = c.Deactivated.Actions.AddToArray(Common.createContextActionRemoveBuff(inspire_ferocity_rage_buff)); //remove inspired ferocity when reckless stance is deactivated
            var condition_on = (Conditional)c.Activated.Actions[0];
            var apply_ferocity = Common.createContextActionApplyBuff(inspire_ferocity_rage_buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true);
            condition_on.IfTrue.Actions = condition_on.IfTrue.Actions.AddToArray(Helpers.CreateConditional(Common.createContextConditionCasterHasFact(inspire_ferocity_switch_buff),
                                                                                 apply_ferocity, null)
                                                                                 );
        }


        static internal void fixItemBondForSpontnaeousCasters()
        {
            var item_bond_spontaneous = library.CopyAndAdd<BlueprintAbility>("e5dcf71e02e08fc448d9745653845df1","ItemBondSpontaneousAbility" ,"");
            item_bond_spontaneous.ReplaceComponent<AbilityRestoreSpellSlot>(Helpers.Create<AbilityRestoreSpontaneousSpell>(a => a.SpellLevel = 10));

            var item_bond_feature = library.Get<BlueprintFeature>("2fb5e65bd57caa943b45ee32d825e9b9");
            var add_facts = item_bond_feature.GetComponent<AddFacts>();
            if (add_facts.Facts.Length == 1)
            {
                add_facts.Facts = add_facts.Facts.AddToArray(item_bond_spontaneous);
            }
        }


        static internal void fixAnimalGrowth()
        {
            //fix animal growth to increase size on upgraded animal companions
            var animal_growth = library.Get<BlueprintAbility>("56923211d2ac95e43b8ac5031bab74d8");
            var animal_growth_buff = library.Get<BlueprintBuff>("3fca5d38053677044a7ffd9a872d3a0a");
            var animal_growth_buff2 = library.CopyAndAdd(animal_growth_buff, "AnimalGrowth2Buff", "");
            animal_growth_buff2.ReplaceComponent<ChangeUnitSize>(c => c.SizeDelta = 2);

            BlueprintFeatureSelection animal_companion_selection = library.Get<BlueprintFeatureSelection>("2ecd6c64683b59944a7fe544033bb533"); //select ac from domain

            List<BlueprintFeature> upgrade_features = new List<BlueprintFeature>();

            foreach (var ac in animal_companion_selection.AllFeatures)
            {
                var upgrade_feature = ac.GetComponent<AddPet>()?.UpgradeFeature;

                if (upgrade_feature != null)
                {
                    upgrade_features.Add(upgrade_feature);
                }
            }

            var animal_growth_run_action = animal_growth.GetComponent<AbilityEffectRunAction>();
            var apply_buff_action = animal_growth_run_action.Actions.Actions[0] as ContextActionApplyBuff;
            var apply_buff2_action = apply_buff_action.CreateCopy();
            apply_buff2_action.Buff = animal_growth_buff2;

            var new_run_action = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, upgrade_features.ToArray()),
                                                           apply_buff2_action,
                                                           apply_buff_action);
            animal_growth_run_action.Actions = Helpers.CreateActionList(new_run_action);
        }


        static internal void fixTeamworkFeats()
        {
            var back_to_back = library.Get<BlueprintFeature>("c920f2cd2244d284aa69a146aeefcb2c");
            back_to_back.GetComponent<BackToBack>().Radius = 2;
            var shield_wall = library.Get<BlueprintFeature>("8976de442862f82488a4b138a0a89907");
            shield_wall.GetComponent<ShieldWall>().Radius = 2;
            var shake_it_off = library.Get<BlueprintFeature>("6337b37f2a7c11b4ab0831d6780bce2a");
            shake_it_off.GetComponent<ShakeItOff>().Radius = 2;
            var allied_spell_caster = library.Get<BlueprintFeature>("9093ceeefe9b84746a5993d619d7c86f");
            allied_spell_caster.GetComponent<AlliedSpellcaster>().Radius = 2;
            allied_spell_caster.AddComponent(Helpers.Create<TeamworkMechanics.AlliedSpellcasterSameSpellBonus>(a => { a.Radius = 2; a.AlliedSpellcasterFact = allied_spell_caster; }));
            allied_spell_caster.SetDescription("Whenever you are adjacent to an ally who also has this feat, you receive a +2 competence bonus on level checks made to overcome spell resistance. If your ally has the same spell prepared (or known with a slot available if they are spontaneous spellcasters), this bonus increases to +4 and you receive a +1 bonus to the caster level for all level-dependent variables, such as duration, range, and effect.");
            var shielded_caster = library.Get<BlueprintFeature>("0b707584fc2ea724aa72c396c2230dc7");
            shielded_caster.GetComponent<ShieldedCaster>().Radius = 2;
        }


        static internal void fixSpellRanges()
        {
            library.Get<BlueprintAbility>("b24583190f36a8442b212e45226c54fc").Range = AbilityRange.Medium; //change range of wail of banshee to medium since by the time you can cast it will be 25 + 17/2 * 5 = 65
            library.Get<BlueprintAbility>("ba48abb52b142164eba309fd09898856").Range = AbilityRange.Medium; //change range of polar midnight to medium since by the time you can cast it will be 25 + 17/2 * 5 = 65
        }

        static internal void fixEcclesitheurge()
        {
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var archetype = library.Get<BlueprintArchetype>("472af8cb3de628f4a805dc4a038971bc");
            archetype.AddSkillPoints = 0;

            var bonded = library.Get<BlueprintFeature>("aa34ca4f3cd5e5d49b2475fcfdf56b24");

            bonded.AddComponent(Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => { c.value = -2; c.spells = new BlueprintAbility[0]; }));
            bonded.SetDescription(bonded.Description + "\nThis ability replaces the increase to channel energy gained at 3rd level.");

            var long_blessing = library.Get<BlueprintAbility>("3ef665bb337d96946bcf98a11103f32f");
            long_blessing.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                             classes: new BlueprintCharacterClass[] { cleric_class }, stepLevel: 2, startLevel: 3));
        }


        static internal void fixIncreasedDamageReduction()
        {
            var drs = new BlueprintFeature[] {library.Get<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8"), //barbarian
                                              library.Get<BlueprintFeature>("e71bd204a2579b1438ebdfbf75aeefae"), //invulnerable rager
                                              library.Get<BlueprintFeature>("2edbf059fd033974bbff67960f15974d"), //mad dog
                                              library.Get<BlueprintFeature>("427b4a34432389042861b8db4cbe3d99"), //invulnerable rager extreme endurance
                                             };

            var increased_dr = library.Get<BlueprintFeature>("ddaee203ee4dcb24c880d633fbd77db6");
            var increased_dr_stalwart = library.Get<BlueprintFeature>("d10496e92d0799a40bb3930b8f4fda0d");

            foreach (var dr in drs)
            {
                var context_rank_config = dr.GetComponent<ContextRankConfig>();
                var feature_list = Helpers.GetField<BlueprintFeature[]>(context_rank_config,"m_FeatureList").ToList();
                
                while (feature_list.Contains(increased_dr))
                {
                    feature_list.Remove(increased_dr);
                }
                Helpers.SetField(context_rank_config, "m_FeatureList", feature_list.ToArray());
            }


            var dr_buff = Helpers.CreateBuff("BarbarianRageIncreasedDrBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             null,
                                             Common.createContextPhysicalDR(Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                                             Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, type: AbilityRankType.StatBonus,
                                                                             featureList: new BlueprintFeature[] { increased_dr, increased_dr })
                                            );
            dr_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var barbarian_rage_buff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(barbarian_rage_buff, dr_buff, increased_dr);


            var dr_buff_stalwart = Helpers.CreateBuff("StalwartDefenderIncreasedDrBuff",
                                 "",
                                 "",
                                 "",
                                 null,
                                 null,
                                 increased_dr_stalwart.ComponentsArray
                                );
            dr_buff_stalwart.SetBuffFlags(BuffFlags.HiddenInUi);
            increased_dr_stalwart.ComponentsArray = new BlueprintComponent[0];
            //fix it to give dr2 as for unchained barb
            increased_dr_stalwart.SetDescription("The stalwart defender's damage reduction from this class increases by 2/—. This increase is always active while the stalwart defender is in a defensive stance. He can select this power up to two times. Its effects stack. The stalwart defender must be at least 6th level before selecting this defensive power.");
            dr_buff_stalwart.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, featureList: new BlueprintFeature[] { increased_dr_stalwart, increased_dr_stalwart }));


            var defensive_stance_buff = library.Get<BlueprintBuff>("3dccdf27a8209af478ac71cded18a271");
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(defensive_stance_buff, dr_buff_stalwart, increased_dr_stalwart);


            //fix resist energy for invulnerable rager
            var resists = drs[3].GetComponents<ResistEnergyContext>();
            foreach (var r in resists)
            {
                r.UseValueMultiplier = false;
                r.ValueMultiplier = Common.createSimpleContextValue(1);
                r.Value = Common.createSimpleContextValue(1);
            }
            drs[3].RemoveComponents<ContextRankConfig>();

        }


        internal static void fixChannelNegativeEnergyHeal()
        {
            //in vanilla it uses shared value for unknown reason, to make it uniform with other abilities we replace heal amount with context value
            var negative_heal = library.Get<BlueprintAbility>("9be3aa47a13d5654cbcb8dbd40c325f2");

            var new_actions = Common.changeAction<ContextActionHealTarget>(negative_heal.GetComponent<AbilityEffectRunAction>().Actions.Actions,
                                                                           c => c.Value = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.Default), 0));

            negative_heal.ReplaceComponent<AbilityEffectRunAction>(c => c.Actions = Helpers.CreateActionList(new_actions));
        }

        internal static void fixVitalStrike()
        {
            BlueprintAbility[] vital_strikes = new BlueprintAbility[] {library.Get<BlueprintAbility>("efc60c91b8e64f244b95c66b270dbd7c"),
                                                                       library.Get<BlueprintAbility>("c714cd636700ac24a91ca3df43326b00"),
                                                                       library.Get<BlueprintAbility>("11f971b6453f74d4594c538e3c88d499")
                                                                      };
            foreach (var a in vital_strikes)
            {
                Helpers.SetField(a, "m_IsFullRoundAction", false);
            }
        }


        internal static void fixArcheologistLuck()
        {
            var archaeologist_luck = library.Get<BlueprintActivatableAbility>("12dc796147c42e04487fcad3aaa40cea");
            archaeologist_luck.Group = ActivatableAbilityGroup.BardicPerformance;
        }


        internal static void addRangerImprovedFavoredTerrain()
        {
            var improved_favored_terrain = library.CopyAndAdd<BlueprintFeatureSelection>("a6ea422d7308c0d428a541562faedefd", "ImprovedFavoredTerrain", "");
            improved_favored_terrain.Mode = SelectionMode.OnlyRankUp;
            improved_favored_terrain.SetName("Improved Favored Terrain");

            foreach (var f in improved_favored_terrain.AllFeatures)
            {
                f.Ranks = 10;
            }

            var ranger_progression = library.Get<BlueprintProgression>("97261d609529d834eba4fd4da1bc44dc");
            ranger_progression.LevelEntries[7].Features.Add(improved_favored_terrain);
            ranger_progression.LevelEntries[12].Features.Add(improved_favored_terrain);
            ranger_progression.LevelEntries[17].Features.Add(improved_favored_terrain);
            ranger_progression.UIGroups = ranger_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(improved_favored_terrain, improved_favored_terrain, improved_favored_terrain));
        }


        //forbid bard song overlap on bardic performance
        [Harmony12.HarmonyPatch(typeof(ActivatableAbility))]
        [Harmony12.HarmonyPatch("OnTurnOn", Harmony12.MethodType.Normal)]
        class ActivatableAbilityy__OnTurnOn__Patch
        {
            static void Postfix( ActivatableAbility __instance)
            {
                if (__instance.Blueprint.Group != ActivatableAbilityGroup.BardicPerformance)
                {
                    return;
                }
         
                var activated_performances =  __instance.Owner.ActivatableAbilities.Enumerable.Where(a => __instance.Owner.Buffs.HasFact(a.Blueprint.Buff) && !a.IsOn
                                                                                                          && a.Blueprint.Group == ActivatableAbilityGroup.BardicPerformance);         
                foreach (var a in activated_performances)
                {
                    if (a != __instance)
                    {
                        (__instance.Owner.Buffs.GetFact(a.Blueprint.Buff) as Buff).Remove();
                    }
                }
            }
        }

        internal static void fixElementalMovementWater()
        {
            var feature = library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8");
            feature.ReplaceComponent<AbilityTargetHasCondition>(Helpers.Create<AddCondition>(c => c.Condition = Kingmaker.UnitLogic.UnitCondition.ImmuneToCombatManeuvers));
        }


        internal static void fixDazzlingDisplay()
        {
            //require holding weapon with weapon focus
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            var dazzling_display = library.Get<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb");
            dazzling_display.AddComponent(Helpers.Create<NewMechanics.AbilityCasterMainWeaponCheckHasParametrizedFeature>(a => a.feature = weapon_focus));
        }


        internal static void fixChannelEnergySaclaing()
        {
            var empyreal_resource = library.Get<BlueprintAbilityResource>("f9af9354fb8a79649a6e512569387dc5");
            empyreal_resource.SetIncreasedByStat(1, StatType.Wisdom);

            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var paladin = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            var sorceror = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");

            string[] cleric_channel_ids = new string[] {"f5fc9a1a2a3c1a946a31b320d1dd31b2",
                                                      "279447a6bf2d3544d93a0a39c3b8e91d",
                                                      "9be3aa47a13d5654cbcb8dbd40c325f2",
                                                      "89df18039ef22174b81052e2e419c728"};

            

            string[] paladin_channel_ids = new string[] { "6670f0f21a1d7f04db2b8b115e8e6abf",
                                                          "0c0cf7fcb356d2448b7d57f2c4db3c0c",
                                                          "4937473d1cfd7774a979b625fb833b47",
                                                          "cc17243b2185f814aa909ac6b6599eaa" };

            string[] empyreal_channel_ids = new string[] { "574cf074e8b65e84d9b69a8c6f1af27b", "e1536ee240c5d4141bf9f9485a665128" };

            foreach (var id in cleric_channel_ids)
            {
                var channel = library.Get<BlueprintAbility>(id);
                channel.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { cleric }, StatType.Charisma));
            }

            foreach (var id in paladin_channel_ids)
            {
                var channel = library.Get<BlueprintAbility>(id);
                channel.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { paladin }, StatType.Charisma));
            }

            foreach (var id in empyreal_channel_ids)
            {
                var channel = library.Get<BlueprintAbility>(id);
                channel.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { sorceror }, StatType.Charisma));
            }
        }

        internal static void fixCaveFangs()
        {
            var cave_fangs_stalagmites_ability = library.Get<BlueprintAbility>("8ec73d388f0875640af8df799f7f16b5");
            var cave_fangs_stalactites_ability = library.Get<BlueprintAbility>("039681ca00c74f24eb302f340f8c6be7");

            var cave_fangs_stalagmites_area = library.Get<BlueprintAbilityAreaEffect>("104bb16f7c3717f44859d0aea97251ce");
            var cave_fangs_stalactites_area = library.Get<BlueprintAbilityAreaEffect>("b8a7c68b040695a40b3a87b9676f7b50");

            var cave_fangs_stalagmites_area2 = library.Get<BlueprintAbilityAreaEffect>("8b4ea698ae053c541beed4e050f32dc3");
            var cave_fangs_stalactites_area2 = library.Get<BlueprintAbilityAreaEffect>("34fc4df95571a2a4f81460cce0c2ea93");

            var dummy_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>(cave_fangs_stalagmites_area.AssetGuid, "CaveFangsNoCastArea", "");
            dummy_area.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            dummy_area.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAreaEffectRunAction(new GameAction[0]) };

            var dummy_spawn_action = Helpers.Create<ContextActionSpawnAreaEffect>(c =>
                                                                                   {
                                                                                       c.AreaEffect = dummy_area;
                                                                                       c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Rounds);
                                                                                   }
                                                                                );
            cave_fangs_stalagmites_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(dummy_spawn_action)));
            cave_fangs_stalactites_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(dummy_spawn_action)));

            cave_fangs_stalagmites_ability.AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(a => a.area_effect = cave_fangs_stalagmites_area));
            cave_fangs_stalagmites_ability.AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(a => a.area_effect = dummy_area));

            cave_fangs_stalactites_ability.AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(a => a.area_effect = cave_fangs_stalactites_area));
            cave_fangs_stalactites_ability.AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(a => a.area_effect = dummy_area));
        }


        internal static void fixAnimalCompanionFeats()
        {
            //remove weapon focus from ac
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            var animal_companion_selection = library.Get<BlueprintFeatureSelection>("571f8434d98560c43935e132df65fe76");

            foreach (var f in animal_companion_selection.AllFeatures)
            {
                var u = f.GetComponent<AddPet>()?.Pet;
                if (u == null)
                {
                    continue;
                }
                var feats = u.GetComponent<AddClassLevels>().Selections[0].Features;
                u.GetComponent<AddClassLevels>().Selections[0].Features = feats.RemoveFromArray(weapon_focus);
            }
            
        }
        

        internal static void fixFlailCritMultiplier()
        {
            BlueprintWeaponType[] flails = new BlueprintWeaponType[] {library.Get<BlueprintWeaponType>("bf1e53f7442ed0c43bf52d3abe55e16a"),
                                                                      library.Get<BlueprintWeaponType>("8fefb7e0da38b06408f185e29372c703")
                                                                     };
            foreach (var f in flails)
            {
                Helpers.SetField(f, "m_CriticalModifier", 3);
            }
        }


        internal static void fixNeclaceOfDoubleCrosses()
        {
            var neclace_feature = library.Get<BlueprintFeature>("64d5a59feeb292e49a6c459fe37c3953");
            var sneak = neclace_feature.GetComponent<AdditionalSneakDamageOnHit>();
            Helpers.SetField(sneak, "m_Weapon", 1);
        }


        internal static void fixDomainSpells()
        {
            Common.replaceSpellFromListWithDuplicate(library.Get<BlueprintSpellList>("eba577470b8ee8443bb4552433451990"), 5, "WeatherDomain"); //ice storm
            Common.replaceSpellFromListWithDuplicate(library.Get<BlueprintSpellList>("eba577470b8ee8443bb4552433451990"), 7, "WeatherDomain"); //fire storm

            Common.replaceSpellFromListWithDuplicate(library.Get<BlueprintSpellList>("35e15cd1b353e2d47b507c445d2f8c6f"), 5, "WaterDomain"); //ice storm
            Common.replaceSpellFromListWithDuplicate(library.Get<BlueprintSpellList>("df3bc5bda7deb9d46b0f177db3bb7876"), 6, "EarthDomain"); //stoneskin
        }

        internal static void fixStalwartDefender()
        {
            var stalwart_defender = library.Get<BlueprintCharacterClass>("d5917881586ff1d4d96d5b7cebda9464");
            var progression = library.Get<BlueprintProgression>("e93eabf4f9b48914c9d880dd41c06385");

            //ad armor proficiency prerequisites
            var light_armor_proficiency = library.Get<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132");
            var medium_armor_proficiency = library.Get<BlueprintFeature>("46f4fb320f35704488ba3d513397789d");
            stalwart_defender.AddComponent(Helpers.PrerequisiteFeature(light_armor_proficiency));
            stalwart_defender.AddComponent(Helpers.PrerequisiteFeature(medium_armor_proficiency));

            var proficiency = library.CopyAndAdd<BlueprintFeature>("a23591cc77086494ba20880f87e73970", "StalwartDefenderProficiency", ""); //from fighter
            proficiency.SetNameDescription("Stalwart Defender Proficiencies",
                                           "A stalwart defender is proficient with all simple and martial weapons, all types of armor, and shields (including tower shields)."
                                           );
            progression.LevelEntries[0].Features.Add(proficiency);
            //give it retroactively
            Action<UnitDescriptor> save_game_fix = delegate (UnitDescriptor unit)
            {
                if (unit.Progression.GetClassLevel(stalwart_defender) >=1 && !unit.Progression.Features.HasFact(proficiency))
                {
                    unit.Progression.Features.AddFeature(proficiency);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_fix);


            var internal_fortitude = library.Get<BlueprintFeature>("727fcc6ef87e568479ab9dc3a8a5dc6c");
            internal_fortitude.ComponentsArray = new BlueprintComponent[0];
            var internal_fortitude_buff = library.CopyAndAdd<BlueprintBuff>("21ff8159995fe194b81d89b1c83f33a3", "StalwartDefenderInternalFortitudeBuff", ""); //from rage

            var defensive_stance_buff = library.Get<BlueprintBuff>("3dccdf27a8209af478ac71cded18a271");
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(defensive_stance_buff, internal_fortitude_buff, internal_fortitude);

            //remove second improved uncanny dodge

            var new_level_entries = new List<LevelEntry>();

            foreach (var level_entry in progression.LevelEntries)
            {
                if (level_entry.Level != 9)
                {
                    new_level_entries.Add(level_entry);
                }
            }
            progression.LevelEntries = new_level_entries.ToArray();
        }


        //fix range
        [Harmony12.HarmonyPatch(typeof(BlueprintAbility))]
        [Harmony12.HarmonyPatch("GetRange", Harmony12.MethodType.Normal)]
        class BlueprintAbility_GetRange
        {
            static void Postfix(BlueprintAbility __instance, bool reach, ref Feet __result)
            {
                AbilityRange range = __instance.Range;
                if (!(range == AbilityRange.Touch || range == AbilityRange.Close || range == AbilityRange.Medium || range == AbilityRange.Long))
                {
                    return;
                }

                if (reach && range != AbilityRange.Long)
                {
                    ++range;
                }

                if (range == AbilityRange.Medium)
                {
                    __result =  60.Feet();
                }
                else if (range == AbilityRange.Long)
                {
                    __result = 100.Feet();
                }
            }
        }


        internal static void fixAlchemistFastBombs()
        {
            var fast_bombs = library.Get<BlueprintFeature>("128c5fccec5ca724281a4907b1f0ac83");
            var fast_bombs_ability = fast_bombs.GetComponent<AddFacts>().Facts[0] as BlueprintActivatableAbility;
            var fast_bombs_buff = fast_bombs_ability.Buff;

            var bombs = fast_bombs_buff.GetComponent<FastBombs>().Abilities;

            var new_buff = Helpers.CreateBuff("FastBombs2Buff",
                                              fast_bombs_buff.Name,
                                              fast_bombs.Description,
                                              "",
                                              fast_bombs.Icon,
                                              null,
                                              Helpers.Create<TurnActionMechanics.IterativeAttacksWithAbilities>(i => i.abilities = bombs)
                                              );

            var new_ability = Helpers.CreateAbility("FastBombs2Ability",
                                                    new_buff.Name,
                                                    new_buff.Description,
                                                    "",
                                                    new_buff.Icon,
                                                    AbilityType.Special,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    Helpers.oneRoundDuration,
                                                    "",
                                                    Helpers.CreateRunActions(Common.createContextActionApplyBuff(new_buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                    Common.createAbilityCasterHasNoFacts(fast_bombs_buff)
                                                    );
            fast_bombs_ability.AddComponent(Helpers.Create<RestrictionHasFact>(r => { r.Feature = new_buff; r.Not = true; }));
            new_ability.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(new_ability);

            //imitate full attack action for bombs
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            fast_bombs_buff.AddComponent(Common.createAddCondition(UnitCondition.Staggered));
            fast_bombs_ability.AddComponent(Helpers.Create<RestrictionHasFact>(r => { r.Feature = staggered; r.Not = true; }));
            

            //fast_bombs_buff.AddComponent(Helpers.Create<FreeActionAbilityUseMechanics.ForceFullRoundOnAbilities>(f => f.abilities = bombs));
            fast_bombs.AddComponent(Helpers.CreateAddFact(new_ability));
            //Helpers.SetField(fast_bombs_ability, "m_ActivateWithUnitCommand", UnitCommand.CommandType.Move);
            //fast_bombs_ability.DeactivateAfterFirstRound = true;
           // fast_bombs_ability.ActivationType = AbilityActivationType.WithUnitCommand;
           // fast_bombs_ability.DeactivateImmediately = false;

        }
    }


    //allow inherent modifiers to be considered as permanent
    [Harmony12.HarmonyPatch(typeof(ModifiableValue.Modifier))]
    [Harmony12.HarmonyPatch("IsPermanent", Harmony12.MethodType.Normal)]
    class ModifiableValue_IsPermanent
    {
        static void Postfix(ModifiableValue.Modifier __instance,  ref bool __result)
        {
            __result = __result || __instance.ModDescriptor == ModifierDescriptor.Inherent;
        }
    }
}
