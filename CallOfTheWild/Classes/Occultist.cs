using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Buffs.Conditions;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    class Occultist
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass occultist_class;
        static public BlueprintProgression occultist_progression;

        static public BlueprintFeature occultist_proficiencies;
        static public BlueprintFeature occultist_spellcasting;


        static public BlueprintAbilityResource generic_focus_resource;
        static public Dictionary<SpellSchool, BlueprintAbilityResource> implement_focus;

        static public BlueprintFeatureSelection focus_power_selection;
        static public BlueprintFeatureSelection implement_base_selection;
        static public BlueprintFeatureSelection implement_selection;
        static public BlueprintFeature repower_construct; //1 round -> 2 rounds -> 1 minute

        internal static void createMesmeristClass()
        {
            Main.logger.Log("Occultist class test mode: " + test_mode.ToString());
            var alchemsit_class = library.TryGet<BlueprintCharacterClass>("0937bec61c0dabc468428f496580c721");
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            occultist_class = Helpers.Create<BlueprintCharacterClass>();
            occultist_class.name = "OccultistClass";
            library.AddAsset(occultist_class, "");

            occultist_class.LocalizedName = Helpers.CreateString("Occultist.Name", "Occultist");
            occultist_class.LocalizedDescription = Helpers.CreateString("Occultist.Description",
                                                                         "The occultist focuses on the world around him, grounded in the powers that flow throughout his environment. He studies the magic that infuses everything, from psychic resonances left in everyday items to powerful incantations that fuel the mightiest spells.\n"
                                                                         + "The occultist channels his psychic might through implements—items that allow him to focus his power and produce incredible effects. For him, implements are more than simple tools. They are a repository of history and a tie to the events of the past. The occultist uses these implements to influence and change the present, adding his legend to theirs. Though some of these implements might be magic items in their own right, most of them are merely of historical or personal significance to the occultist.The occultist channels his psychic might through implements—items that allow him to focus his power and produce incredible effects. For him, implements are more than simple tools. They are a repository of history and a tie to the events of the past. The occultist uses these implements to influence and change the present, adding his legend to theirs. Though some of these implements might be magic items in their own right, most of them are merely of historical or personal significance to the occultist.\n"
                                                                         + "Role: Occultists are always eager to travel in the company of adventurers, explorers, and archaeologists, as those three groups of people have a knack for finding items with rich histories and great significance."
                                                                         );
            occultist_class.m_Icon = alchemsit_class.Icon;
            occultist_class.SkillPoints = alchemsit_class.SkillPoints;
            occultist_class.HitDie = DiceType.D8;
            occultist_class.BaseAttackBonus = alchemsit_class.BaseAttackBonus;
            occultist_class.FortitudeSave = alchemsit_class.FortitudeSave;
            occultist_class.ReflexSave = alchemsit_class.WillSave;
            occultist_class.WillSave = alchemsit_class.FortitudeSave;
            occultist_class.Spellbook = createOccultistSpellbook();
            occultist_class.ClassSkills = new StatType[] { StatType.SkillStealth, StatType.SkillThievery,
                                                      StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreReligion,
                                                      StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice};
            occultist_class.IsDivineCaster = false;
            occultist_class.IsArcaneCaster = false;
            occultist_class.StartingGold = alchemsit_class.StartingGold;
            occultist_class.PrimaryColor = alchemsit_class.PrimaryColor;
            occultist_class.SecondaryColor = alchemsit_class.SecondaryColor;
            occultist_class.RecommendedAttributes = new StatType[] { StatType.Intelligence };
            occultist_class.NotRecommendedAttributes = new StatType[0];
            occultist_class.EquipmentEntities = alchemsit_class.EquipmentEntities;
            occultist_class.MaleEquipmentEntities = alchemsit_class.MaleEquipmentEntities;
            occultist_class.FemaleEquipmentEntities = alchemsit_class.FemaleEquipmentEntities;
            occultist_class.ComponentsArray = alchemsit_class.ComponentsArray;
            occultist_class.StartingItems = new BlueprintItem[]
            {
                library.Get<BlueprintItemArmor>("d7963e1fcf260c148877afd3252dbc91"), //scalemail
                library.Get<BlueprintItemWeapon>("6fd0a849531617844b195f452661b2cd"), //longsword
                library.Get<BlueprintItemWeapon>("f4cef3ba1a15b0f4fa7fd66b602ff32b"), //shield
                library.Get<BlueprintItemEquipmentUsable>("201f6150321e09048bd59e9b7f558cb0"), //longbow
                library.Get<BlueprintItemEquipmentUsable>("d52566ae8cbe8dc4dae977ef51c27d91"), //potion of cure light wounds
                library.Get<BlueprintItemEquipmentUsable>("f79c3fd5012a3534c8ab36dc18e85fb1"), //sleep
                library.Get<BlueprintItemEquipmentUsable>("fe244c39bdd5cb64eae65af23c6759de") //cause fear
            };

            createOccultistProgression();
            occultist_class.Progression = occultist_progression;
            occultist_class.Archetypes = new BlueprintArchetype[] { };//battle host, silksworn, reliquarian, haunt collector, panoply savant? necrocultist?
            Helpers.RegisterClass(occultist_class);
        }


        static void createOccultistProgression()
        {
            /*abjuration: warding talisman -> mind barrier x
                aegis (as swift action for 2 pts ?)
                energy shield
                globe of negation ?
                unravelling
            conjuration: casting focus -> servitor x
                flesh mend
                psychic fog
                purge corruption
                side step
            divination: third eye -> sudden insight x
                danger sight
                mind eye ?
                in accordance with prophecy
            enchantment: glorious presence -> cloud mind x
                binding pattern
                inspired assault ?(1 + 1/4 levels)
                obey 
            evocation: intense focus -> energy ray
                energy blast: (5 + 1d6/2 levels)
                energy ward
                radiance
                wall of power
            illusion: distortion (concealement until hit 20 -> 50%) -> color beam (1d4 rounds, will)              
                shadow beast
                unseen - greater invisibility for 1 minute
                bedeveling aura (1 round/2 levels)
            necromancy: necromantic focus - > mind fear
                flesh rot
                necromantic servant
                soulbound puppet
                pain wave
            transmuation: physical enchancement -> legacy weapon
                mind over gravity (fly + 30 speed bonus)
                philosopher's touch
                quickness (haste + 1/6 levels)
                size alteration (enlarge reduce without limitation)
                sudden speed 

            trappings of the warrior: martial skill -> combat feat (?) 
                counterstrike
                warrior's resilence (?)
                shield ally
            mage's paraphernalia: scholarly knowledge -> inspiration (convert any spell to wizard divination/evocation/necromancy spell of that level for 1 pt)
                metamagic knowledge
                metamagic master
                spell power
            */

            //repower construct to replace binding circles

            //createKnacks();
            //createProficiencies();
            //createImplements();
            //createRepowerConstruct();
        }


        static BlueprintSpellbook createOccultistSpellbook()
        {
            var alchemist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("027d37761f3804042afa96fe3e9086cc");
            var occultist_spellbook = Helpers.Create<BlueprintSpellbook>();
            occultist_spellbook.name = "OccultistSpellbook";
            library.AddAsset(occultist_spellbook, "");
            occultist_spellbook.Name = occultist_class.LocalizedName;
            occultist_spellbook.SpellsPerDay = alchemist_class.Spellbook.SpellsPerDay;
            occultist_spellbook.SpellsKnown = alchemist_class.Spellbook.SpellsKnown;
            occultist_spellbook.Spontaneous = true;
            occultist_spellbook.IsArcane = false;
            occultist_spellbook.AllSpellsKnown = false;
            occultist_spellbook.CanCopyScrolls = false;
            occultist_spellbook.CastingAttribute = StatType.Intelligence;
            occultist_spellbook.CharacterClass = occultist_class;
            occultist_spellbook.CasterLevelModifier = 0;
            occultist_spellbook.CantripsType = CantripsType.Cantrips;
            occultist_spellbook.SpellsPerLevel = 0;

            occultist_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            occultist_spellbook.SpellList.name = "OccultistSpellList";
            library.AddAsset(occultist_spellbook.SpellList, "");
            occultist_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < occultist_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                occultist_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "55f14bc84d7c85446b07a1b5dd6b2b4c", 0), //daze
                new Common.SpellId( "f0f8e5b9808f44e4eadd22b138131d52", 0), //flare
                new Common.SpellId( "c3a8f31778c3980498d8f00c980be5f5", 0), //guidance
                new Common.SpellId( "95f206566c5261c42aa5b3e7e0d1e36c", 0), //mage light
                new Common.SpellId( "7bc8e27cba24f0e43ae64ed201ad5785", 0), //resistance
                new Common.SpellId( "5bf3315ce1ed4d94e8805706820ef64d", 0), //touch of fatigue

                new Common.SpellId( "4783c3709a74a794dbe7c8e7e0b1b038", 1),//burning hands                
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( NewSpells.command.AssetGuid, 1),
                new Common.SpellId( "5590652e1c2225c4ca30c4a699ab3649", 1), //cure light wounds
                new Common.SpellId( "c60969e7f264e6d4b84a1499fdcf9039", 1), //enlarge person
                new Common.SpellId( "4f8181e7a7f1d904fbaea64220e83379", 1), //expeditious retreat
                new Common.SpellId( "3e9d1119d43d07c4c8ba9ebfd1671952", 1), //gravity bow
                new Common.SpellId( "fd4d9fd7f87575d47aafe2a64a6e2d8d", 1), //hypnotism
                new Common.SpellId( "e5af3674bb241f14b9a9f6b0c7dc3d27", 1), //inflict light wounds
                new Common.SpellId( "779179912e6c6fe458fa4cfb90d96e10", 1), //lead blades
                new Common.SpellId( "9e1ad5d6f87d19e4d8883d63a6e35568", 1), //mage armor
                new Common.SpellId( NewSpells.magic_weapon.AssetGuid, 1),
                new Common.SpellId( "4e0e9aba6447d514f88eff1464cc4763", 1), //reduce person
                new Common.SpellId( "ef768022b0785eb43a18969903c537c4", 1), //shield
                new Common.SpellId( "ab395d2335d3f384e99dddee8562978f", 1), //shocking grasp
                new Common.SpellId( "bb7ecad2d3d2c8247a38f44855c99061", 1), //sleep
                new Common.SpellId( "f001c73999fb5a543a199f890108d936", 1), //vanish

                new Common.SpellId( NewSpells.animate_dead_lesser.AssetGuid, 2),
                //alied cloak
                new Common.SpellId( "14ec7a4e52e90fa47a4c8d63c69fd5c1", 2), //blur
                new Common.SpellId( NewSpells.blade_tutor.AssetGuid, 2), //should probablyalso be there due to flavor
                new Common.SpellId( "6b90c773a6543dc49b2505858ce33db5", 2), //cure moderate wounds
                new Common.SpellId( "b48b4c5ffb4eab0469feba27fc86a023", 2), //delay poison
                new Common.SpellId( "e1291272c8f48c14ab212a599ad17aac", 2), //effortless armor
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( "4709274b2080b6444a3c11c6ebbe2404", 2), //find traps
                new Common.SpellId( NewSpells.force_sword.AssetGuid, 2),
                //frost fall
                new Common.SpellId( NewSpells.ghostbane_dirge.AssetGuid, 2),
                new Common.SpellId( "ce7dad2b25acf85429b6c9550787b2d9", 2), //glitterdust
                new Common.SpellId( "65f0b63c45ea82a4f8b8325768a3832d", 2), //inflict moderate wounds
                new Common.SpellId( NewSpells.inflict_pain.AssetGuid, 2),
                new Common.SpellId( "89940cde01689fb46946b2f8cd7b66b7", 2), //invisibility
                new Common.SpellId( "dee3074b2fbfb064b80b973f9b56319e", 2), //pernicious poison
                new Common.SpellId( "21ffef7791ce73f468b6fca4d9371e8b", 2), //resist energy
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( "c3893092a333b93499fd0a21845aa265", 2), //sound burst
                //tactical acumen

                new Common.SpellId( "4b76d32feb089ad4499c3a1ce8e1ac27", 3), //animate dead
                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 3), //bestow curse
                new Common.SpellId( "2a9ef0e0b5822a24d88b16673a267456", 3), //call lightning
                new Common.SpellId( NewSpells.cloak_of_winds.AssetGuid, 3),
                new Common.SpellId( NewSpells.countless_eyes.AssetGuid, 3),
                new Common.SpellId( "3361c5df793b4c8448756146a88026ad", 3), //cure serious wounds
                new Common.SpellId( "7658b74f626c56a49939d9c20580885e", 3), //deep slumber
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //displacement
                new Common.SpellId( "2d81362af43aeac4387a3d4fced489c3", 3), //fireball
                new Common.SpellId( NewSpells.flame_arrow.AssetGuid, 3),
                new Common.SpellId( NewSpells.fly.AssetGuid, 3),
                new Common.SpellId( "486eaff58293f6441a5c2759c4872f98", 3), //haste
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 3), //hold person
                new Common.SpellId( "bd5da98859cf2b3418f6d68ea66cabbe", 3), //inflict serious wounds
                new Common.SpellId( NewSpells.keen_edge.AssetGuid, 3),
                new Common.SpellId( "d2cff9243a7ee804cb6d5be47af30c73", 3), //lightning bolt
                new Common.SpellId( "2d4263d80f5136b4296d6eb43a221d7d", 3), //magic vestment
                new Common.SpellId( NewSpells.magic_weapon_greater.AssetGuid, 3),
                new Common.SpellId( "d2f116cfe05fcdd4a94e80143b67046f", 3), //protection from energy
                new Common.SpellId( "7bb0c402f7f789d4d9fae8ca87b4c7e2", 3), //resist energy communal
                new Common.SpellId( NewSpells.sands_of_time.AssetGuid, 3),
                new Common.SpellId( "f492622e473d34747806bdb39356eb89", 3), //slow
                new Common.SpellId( SpiritualWeapons.twilight_knife.AssetGuid, 3),


                new Common.SpellId( NewSpells.air_walk.AssetGuid, 4),
                new Common.SpellId( "7792da00c85b9e042a0fdfc2b66ec9a8", 4), //break enchantment
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 4), //confusion
                new Common.SpellId( "f72f8f03bf0136c4180cd1d70eb773a5", 4), //controlled blast fireball
                new Common.SpellId( "41c9016596fe1de4faf67425ed691203", 4), //cure critical wounds
                new Common.SpellId( NewSpells.daze_mass.AssetGuid, 4),
                new Common.SpellId( "e9cc9378fd6841f48ad59384e79e9953", 4), //death ward
                new Common.SpellId( "4a648b57935a59547b7a2ee86fb4f26a", 4), //dimensions door
                new Common.SpellId( "95f7cdcec94e293489a85afdf5af1fd7", 4), //dismissal
                new Common.SpellId( "20b548bf09bb3ea4bafea78dcb4f3db6", 4), //echolocation
                //etheric shards
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( NewSpells.fire_shield.AssetGuid, 4),
                new Common.SpellId( "0087fc2d64b6095478bc7b8d7d512caf", 4), //freedom of movement
                new Common.SpellId( "41e8a952da7a5c247b3ec1c2dbb73018", 4), //hold monster
                new Common.SpellId( "fcb028205a71ee64d98175ff39a0abf9", 4), //ice storm
                new Common.SpellId( "651110ed4f117a948b41c05c5c7624c0", 4), //inflcit critical wounds
                new Common.SpellId( "ecaa0def35b38f949bd1976a6c9539e0", 4), //invisibility greater
                new Common.SpellId( "2a6eda8ef30379142a4b75448fb214a3", 4), //poison
                new Common.SpellId( NewSpells.rigor_mortis.AssetGuid, 4),
                new Common.SpellId( NewSpells.river_of_wind.AssetGuid, 4),
                new Common.SpellId( NewSpells.spirit_bound_blade.AssetGuid, 4),
                new Common.SpellId( "f09453607e683784c8fca646eec49162", 4), //shout
                new Common.SpellId( "c66e86905f7606c4eaa5c774f0357b2b", 4), //stoneskin
                new Common.SpellId( NewSpells.wall_of_fire.AssetGuid, 4),


                new Common.SpellId( NewSpells.air_walk_communal.AssetGuid, 5),
                new Common.SpellId( "d5a36a7ee8177be4f848b953d1c53c84", 5), //call lightning storm
                new Common.SpellId( NewSpells.command_greater.AssetGuid, 5),
                new Common.SpellId( "e7c530f8137630f4d9d7ee1aa7b1edc0", 5), //cone of cold     
                new Common.SpellId( "5d3d689392e4ff740a761ef346815074", 5), //cure light wounds mass
                new Common.SpellId( NewSpells.curse_major.AssetGuid, 5),
                new Common.SpellId( "f0f761b808dc4b149b08eaf44b99f633", 5), //dispel magic greater               
                new Common.SpellId( "d7cbd2004ce66a042aeab2e95a3c5c61", 5), //dominate person
                new Common.SpellId( "ebade19998e1f8542a1b55bd4da766b3", 5), //fire snake
                new Common.SpellId( NewSpells.ghostbane_dirge_mass.AssetGuid, 5),
                new Common.SpellId( "9da37873d79ef0a468f969e4e5116ad2", 5), //inflcit light wounds mass
                new Common.SpellId( NewSpells.inflict_pain_mass.AssetGuid, 5),
                new Common.SpellId( "eabf94e4edc6e714cabd96aa69f8b207", 5), //mind fog
                new Common.SpellId( NewSpells.overland_flight.AssetGuid, 5),
                new Common.SpellId( NewSpells.particulate_form.AssetGuid, 5),
                new Common.SpellId( "0a5ddfbcfb3989543ac7c936fc256889", 5), //spell resistance
                new Common.SpellId( "7c5d556b9a5883048bf030e20daebe31", 5), //stoneskin mass
                new Common.SpellId( NewSpells.suffocation.AssetGuid, 5),
                new Common.SpellId( "4cf3d0fae3239ec478f51e86f49161cb", 5), //true seeing

                new Common.SpellId( "36c8971e91f1745418cc3ffdfac17b74", 6), //blade barrier
                new Common.SpellId( "645558d63604747428d55f0dd3a4cb58", 6), //chain lightning
                new Common.SpellId( "7f71a70d822af94458dc1a235507e972", 6), //cloak of dreams
                new Common.SpellId( "5ef85d426783a5347b420546f91a677b", 6), //cold ice strike
                new Common.SpellId( NewSpells.contingency.AssetGuid, 6),
                new Common.SpellId( NewSpells.control_construct.AssetGuid, 6),
                new Common.SpellId( "571221cc141bc21449ae96b3944652aa", 6), //cure moderate wounds
                new Common.SpellId( "4aa7942c3e62a164387a73184bca3fc1", 6), //disintegrate
                new Common.SpellId( NewSpells.freezing_sphere.AssetGuid, 6),
                new Common.SpellId( "cc09224ecc9af79449816c45bc5be218", 6), //harm
                new Common.SpellId( "5da172c4c89f9eb4cbb614f3a67357d3", 6), //heal
                new Common.SpellId( "03944622fbe04824684ec29ff2cec6a7", 6), //inflict moderate wounds mass
                new Common.SpellId( "0e67fa8f011662c43934d486acc50253", 6), //prediction of failure
                new Common.SpellId( "093ed1d67a539ad4c939d9d05cfe192c", 6), //sirocco
                new Common.SpellId( "27203d62eb3d4184c9aced94f22e1806", 6), //transformation
                new Common.SpellId( "474ed0aa656cc38499cc9a073d113716", 6), //umbral strike

 
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(occultist_spellbook.SpellList, spell_id.level);
            }

            occultist_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.PsychicSpellbook>());

            occultist_spellcasting = Helpers.CreateFeature("OccultistSpellCasting",
                                             "Occultist Spellcasting",
                                             "An occultist casts psychic spells drawn from the occultist spell list, limited by the implement groups he knows.\n"
                                             + "He can cast any spell he knows without preparing it ahead of time. Every occultist spell has an implement component. To learn or cast a spell, an occultist must have an Intelligence score equal to at least 10 + the spell level. The Difficulty Class for a saving throw against an occultist’s spell equals 10 + the spell level + the occultist’s Intelligence modifier.\n"
                                             + "An occultist can cast only a certain number of spells of each spell level per day. In addition, he gains bonus spells per day if he has a high Intelligence score.\n"
                                             + "The occultist’s selection of spells is limited. For each implement school he learns to use, he can add 6 spells of any level he can cast to his list of spells known, chosen from that school’s spell list. If he selects the same implement school multiple times, he adds 6 more spells from that school’s list for each time he has selected that school.\n"
                                             + "An occultist need not prepare his spells in advance. He can cast any spell he knows at any time, assuming he has not yet used up his allotment of spells per day for the spell’s level.",
                                             "",
                                             null,
                                             FeatureGroup.None);

            occultist_spellcasting.AddComponents(Helpers.Create<SpellFailureMechanics.PsychicSpellbook>(p => p.spellbook = occultist_spellbook),
                                                 Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.NaturalSpell));
            occultist_spellcasting.AddComponent(Helpers.Create<SpellbookMechanics.AddUndercastSpells>(p => p.spellbook = occultist_spellbook));
            occultist_spellcasting.AddComponent(Helpers.CreateAddFact(Investigator.center_self));
            occultist_spellcasting.AddComponents(Common.createCantrips(occultist_class, StatType.Charisma, occultist_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
            occultist_spellcasting.AddComponents(Helpers.CreateAddFacts(occultist_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));

            return occultist_spellbook;
        }
    }
}
