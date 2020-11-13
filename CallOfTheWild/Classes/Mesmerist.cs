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
    class Mesmerist
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass mesmerist_class;
        static public BlueprintProgression mesmerist_progression;

        static public BlueprintFeature mesmerist_proficiencies;
        static public BlueprintFeature mesmerist_spellcasting;

        internal static void createMesmeristClass()
        {
            Main.logger.Log("Mesmerist class test mode: " + test_mode.ToString());
            var slayer_class = library.TryGet<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");
            var bard_class = library.TryGet<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            mesmerist_class = Helpers.Create<BlueprintCharacterClass>();
            mesmerist_class.name = "MesmeristClass";
            library.AddAsset(mesmerist_class, "");

            mesmerist_class.LocalizedName = Helpers.CreateString("Mesmerist.Name", "Mesmerist");
            mesmerist_class.LocalizedDescription = Helpers.CreateString("Mesmerist.Description",
                                                                         "Experts at charm and deceit, mesmerists compel others to heed their words and bend to their will. Psychic powers, primarily those of enchantment and illusion, give mesmerists the tools they need to manipulate others—usually for their own personal gain. The very gaze of a mesmerist can hypnotize someone into following his whims. Mesmerists frequently form cults of personality around themselves, and they develop skills and contingency plans in case their ploys are discovered.\n"
                                                                         + "They draw their magic from the Astral Plane, and many consider their minds to be conduits to enigmatic spaces others can’t comprehend.\n"
                                                                         + "Role: Mesmerists wield power over lesser minds, suppressing foes’ wills to weaken them. Priding themselves on their trickery and inventiveness, they also support their allies—and often themselves—with magical tricks, most of which offer protection. Their limited healing ability primarily provides temporary hit points, so mesmerists aren’t the strongest primary healers, but they can easily remove conditions that typically affect the mind."
                                                                         );
            mesmerist_class.m_Icon = slayer_class.Icon;
            mesmerist_class.SkillPoints = bard_class.SkillPoints;
            mesmerist_class.HitDie = DiceType.D8;
            mesmerist_class.BaseAttackBonus = bard_class.BaseAttackBonus;
            mesmerist_class.FortitudeSave = bard_class.FortitudeSave;
            mesmerist_class.ReflexSave = bard_class.ReflexSave;
            mesmerist_class.WillSave = bard_class.WillSave;
            mesmerist_class.Spellbook = createMesmeristSpellbook();
            mesmerist_class.ClassSkills = new StatType[] {StatType.SkillAthletics,  StatType.SkillMobility, StatType.SkillStealth, StatType.SkillThievery,
                                                      StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion,
                                                      StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice};
            mesmerist_class.IsDivineCaster = false;
            mesmerist_class.IsArcaneCaster = false;
            mesmerist_class.StartingGold = bard_class.StartingGold;
            mesmerist_class.PrimaryColor = bard_class.PrimaryColor;
            mesmerist_class.SecondaryColor = bard_class.SecondaryColor;
            mesmerist_class.RecommendedAttributes = new StatType[] { StatType.Charisma };
            mesmerist_class.NotRecommendedAttributes = new StatType[0];
            mesmerist_class.EquipmentEntities = slayer_class.EquipmentEntities;
            mesmerist_class.MaleEquipmentEntities = slayer_class.MaleEquipmentEntities;
            mesmerist_class.FemaleEquipmentEntities = slayer_class.FemaleEquipmentEntities;
            mesmerist_class.ComponentsArray = slayer_class.ComponentsArray;
            mesmerist_class.StartingItems = new BlueprintItem[]
            {
                library.Get<BlueprintItemArmor>("afbe88d27a0eb544583e00fa78ffb2c7"), //studded leather
                library.Get<BlueprintItemWeapon>("ada85dae8d12eda4bbe6747bb8b5883c"), //quarterstaff
                library.Get<BlueprintItemWeapon>("511c97c1ea111444aa186b1a58496664"), //light crossbow
                library.Get<BlueprintItemEquipmentUsable>("bb7ecad2d3d2c8247a38f44855c99061"), //sleep scroll
                library.Get<BlueprintItemEquipmentUsable>("fe244c39bdd5cb64eae65af23c6759de"), //cause fear
                library.Get<BlueprintItemEquipmentUsable>("f001c73999fb5a543a199f890108d936") //vanish
            };

            //createMesmeristProgression();
            mesmerist_class.Progression = mesmerist_progression;

            Helpers.RegisterClass(mesmerist_class);
        }

        static BlueprintSpellbook createMesmeristSpellbook()
        {
            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var mesmerist_spellbook = Helpers.Create<BlueprintSpellbook>();
            mesmerist_spellbook.name = "MesmeristSpellbook";
            library.AddAsset(mesmerist_spellbook, "");
            mesmerist_spellbook.Name = mesmerist_class.LocalizedName;
            mesmerist_spellbook.SpellsPerDay = bard_class.Spellbook.SpellsPerDay;
            mesmerist_spellbook.SpellsKnown = bard_class.Spellbook.SpellsKnown;
            mesmerist_spellbook.Spontaneous = true;
            mesmerist_spellbook.IsArcane = false;
            mesmerist_spellbook.AllSpellsKnown = false;
            mesmerist_spellbook.CanCopyScrolls = false;
            mesmerist_spellbook.CastingAttribute = StatType.Charisma;
            mesmerist_spellbook.CharacterClass = mesmerist_class;
            mesmerist_spellbook.CasterLevelModifier = 0;
            mesmerist_spellbook.CantripsType = CantripsType.Cantrips;
            mesmerist_spellbook.SpellsPerLevel = bard_class.Spellbook.SpellsPerLevel;

            mesmerist_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            mesmerist_spellbook.SpellList.name = "MesmeristSpellList";
            library.AddAsset(mesmerist_spellbook.SpellList, "");
            mesmerist_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < mesmerist_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                mesmerist_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "55f14bc84d7c85446b07a1b5dd6b2b4c", 0), //daze
                new Common.SpellId( "f0f8e5b9808f44e4eadd22b138131d52", 0), //flare
                new Common.SpellId( "95f206566c5261c42aa5b3e7e0d1e36c", 0), //mage light
                new Common.SpellId( "5bf3315ce1ed4d94e8805706820ef64d", 0), //touch of fatigue

                new Common.SpellId( "8bc64d869456b004b9db255cdd1ea734", 1),//bane
                new Common.SpellId( NewSpells.burst_of_adrenaline.AssetGuid, 1),
                new Common.SpellId( NewSpells.burst_of_insight.AssetGuid, 1),
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( "91da41b9793a4624797921f221db653c", 1), //color spray
                new Common.SpellId( "fbdd8c455ac4cde4a9a3e18c84af9485", 1), //doom
                new Common.SpellId( "4f8181e7a7f1d904fbaea64220e83379", 1), //expeditious retreat
                new Common.SpellId( "4d9bf81b7939b304185d58a09960f589", 1), //faerie fire
                new Common.SpellId( "95851f6e85fe87d4190675db0419d112", 1), //grease
                new Common.SpellId( "fd4d9fd7f87575d47aafe2a64a6e2d8d", 1), //hideous laughter
                new Common.SpellId( "fd4d9fd7f87575d47aafe2a64a6e2d8d", 1), //hypnotism
                new Common.SpellId( Witch.ill_omen.AssetGuid, 1),
                new Common.SpellId( NewSpells.invigorate.AssetGuid, 1),
                new Common.SpellId( NewSpells.obscuring_mist.AssetGuid, 1),
                new Common.SpellId( "450af0402422b0b4980d9c2175869612", 1), //ray of enfeeblement
                new Common.SpellId( "fa3078b9976a5b24caf92e20ee9c0f54", 1), //ray of sickening
                new Common.SpellId( "55a037e514c0ee14a8e3ed14b47061de", 1), //remove fear
                new Common.SpellId( "f6f95242abdfac346befd6f4f6222140", 1), //remove sickness
                new Common.SpellId( "bb7ecad2d3d2c8247a38f44855c99061", 1), //sleep
                new Common.SpellId( "ad10bfec6d7ae8b47870e3a545cc8900", 1), //tocuh of gracelessness
                new Common.SpellId( "8fd74eddd9b6c224693d9ab241f25e84", 1), //summon monster 1
                new Common.SpellId( "f001c73999fb5a543a199f890108d936", 1), //vanish

                new Common.SpellId( NewSpells.babble.AssetGuid, 2),
                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 2), //blindness
                new Common.SpellId( NewSpells.blistering_invective.AssetGuid, 2),
                new Common.SpellId( "14ec7a4e52e90fa47a4c8d63c69fd5c1", 2), //blur
                new Common.SpellId( "ce4c4e52c53473549ae033e2bb44b51a", 2), //castigate
                new Common.SpellId( NewSpells.bone_fists.AssetGuid, 2),
                new Common.SpellId( "de7a025d48ad5da4991e7d3c682cf69d", 2), //cat's grace
                new Common.SpellId( "b48b4c5ffb4eab0469feba27fc86a023", 2), //delay poison
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagle's splendor
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( "ce7dad2b25acf85429b6c9550787b2d9", 2), //glitterdust
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 2), //hold person
                new Common.SpellId( "41bab342089c0254ca222eb918e98cd4", 2), //hold animal
                new Common.SpellId( NewSpells.howling_agony.AssetGuid, 2),
                new Common.SpellId( NewSpells.hypnotic_pattern.AssetGuid, 2),
                new Common.SpellId( NewSpells.inflict_pain.AssetGuid, 2),
                new Common.SpellId( "89940cde01689fb46946b2f8cd7b66b7", 2), //invisibility
                new Common.SpellId( "3e4ab69ada402d145a5e0ad3ad4b8564", 2), //mirror image
                new Common.SpellId( "97b991256e43bb140b263c326f690ce2", 2), //rage
                new Common.SpellId( "e84fc922ccf952943b5240293669b171", 2), //restoration lesser
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( NewSpells.silence.AssetGuid, 2),
                new Common.SpellId( NewSpells.stricken_heart.AssetGuid, 2),

                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 3), //bestow curse
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 3), //confusion
                new Common.SpellId( "4baf4109145de4345861fe0f2209d903", 3), //crushing despair
                new Common.SpellId( "7658b74f626c56a49939d9c20580885e", 3), //deep slumber
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //displacement
                new Common.SpellId( "754c478a2aa9bb54d809e648c3f7ac0e", 3), //dominate animal
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 3), //false life, greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 3), //fear
                new Common.SpellId( NewSpells.invigorate_mass.AssetGuid, 3),
                new Common.SpellId( "dd2918e4a77c50044acba1ac93494c36", 3), //overwhelming grief
                new Common.SpellId( NewSpells.ray_of_exhaustion.AssetGuid, 3),
                new Common.SpellId( "c927a8b0cd3f5174f8c0b67cdbfde539", 3), //remove blindness
                new Common.SpellId( "b48674cef2bff5e478a007cf57d8345b", 3), //remove curse
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 3), //see invisibility
                new Common.SpellId( NewSpells.spite.AssetGuid, 3),
                new Common.SpellId( NewSpells.synesthesia.AssetGuid, 3),
                new Common.SpellId( "8a28a811ca5d20d49a863e832c31cce1", 3), //vampyric touch
                new Common.SpellId( NewSpells.synaptic_pulse.AssetGuid, 3),

                new Common.SpellId( "7792da00c85b9e042a0fdfc2b66ec9a8", 4), //break enchantment
                new Common.SpellId( NewSpells.command_greater.AssetGuid, 4),
                new Common.SpellId( NewSpells.daze_mass.AssetGuid, 4),
                new Common.SpellId( NewSpells.curse_major.AssetGuid, 4),
                new Common.SpellId( "4a648b57935a59547b7a2ee86fb4f26a", 4), //dimensions door
                new Common.SpellId( "d7cbd2004ce66a042aeab2e95a3c5c61", 4), //dominate person
                new Common.SpellId( "f34fb78eaaec141469079af124bcfa0f", 4), //enervation
                new Common.SpellId( "4c349361d720e844e846ad8c19959b1e", 4), //freedom of movement
                new Common.SpellId( "41e8a952da7a5c247b3ec1c2dbb73018", 4), //hold monster            
                new Common.SpellId( "ecaa0def35b38f949bd1976a6c9539e0", 4), //invisibility greater
                new Common.SpellId( "6717dbaef00c0eb4897a1c908a75dfe5", 4), //phantasmal killer
                new Common.SpellId( "2a6eda8ef30379142a4b75448fb214a3", 4), //poison
                new Common.SpellId( "f2115ac1148256b4ba20788f7e966830", 4), //restoration
                new Common.SpellId( "d316d3d94d20c674db2c24d7de96f6a7", 4), //serenity
                new Common.SpellId( NewSpells.shadow_conjuration.AssetGuid, 4),
                new Common.SpellId( NewSpells.solid_fog.AssetGuid, 4),
                new Common.SpellId( NewSpells.synapse_overload.AssetGuid, 4),
                new Common.SpellId( NewSpells.synaptic_pulse_greater.AssetGuid, 4),
               
                new Common.SpellId( "41236cf0e476d7043bc16a33a9f449bd", 5), //castigate
                new Common.SpellId( "7f71a70d822af94458dc1a235507e972", 5), //cloak of dreams
                new Common.SpellId( NewSpells.inflict_pain_mass.AssetGuid, 5),
                new Common.SpellId( "444eed6e26f773a40ab6e4d160c67faa", 5), //feeblemind
                new Common.SpellId( "15a04c40f84545949abeedef7279751a", 5), //joyful rapture
                new Common.SpellId( "eabf94e4edc6e714cabd96aa69f8b207", 5), //mind fog
                new Common.SpellId( "12fb4a4c22549c74d949e2916a2f0b6a", 5), //phantasmal web
                new Common.SpellId( "1f2e6019ece86d64baa5effa15e81ecc", 5), //phantasmal putrefaction
                new Common.SpellId( "07d577a74441a3a44890e3006efcf604", 5), //primal regression
                new Common.SpellId( NewSpells.psychic_surgery.AssetGuid, 5),
                new Common.SpellId( "237427308e48c3341b3d532b9d3a001f", 5), //shadow evocation
                new Common.SpellId( "8878d0c46dfbd564e9d5756349d5e439", 5), //waves of fatigue
                                                      
                new Common.SpellId( "740d943e42b60f64a8de74926ba6ddf7", 6), //euphoric tranquility
                new Common.SpellId( "3167d30dd3c622c46b0c0cb242061642", 6), //eyebite
                new Common.SpellId( NewSpells.hold_person_mass.AssetGuid, 6),
                new Common.SpellId( "2b044152b3620c841badb090e01ed9de", 6), //insanity
                new Common.SpellId( "98310a099009bbd4dbdf66bcef58b4cd", 6), //invisibility mass
                new Common.SpellId( NewSpells.irresistible_dance.AssetGuid, 6),
                new Common.SpellId( "41cf93453b027b94886901dbfc680cb9", 6), //overwhelming presence
                new Common.SpellId( "261e1788bfc5ac1419eec68b1d485dbc", 6), //power word blind
                new Common.SpellId( NewSpells.song_of_discord_greater.AssetGuid, 6),
                new Common.SpellId( "4cf3d0fae3239ec478f51e86f49161cb", 6), //true seeing
                new Common.SpellId( "1e2d1489781b10a45a3b70192bba9be3", 6), //waves of ectasy
                new Common.SpellId( "3e4d3b9a5bd03734d9b053b9067c2f38", 6), //waves of exhaustion               
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(mesmerist_spellbook.SpellList, spell_id.level);
            }

            mesmerist_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.PsychicSpellbook>());

            mesmerist_spellcasting = Helpers.CreateFeature("MesmeristSpellCasting",
                                             "Mesmerist Spellcasting",
                                             "A mesmerist casts psychic spells drawn from the mesmerist spell list. He can cast any spell he knows without preparing it ahead of time. To learn or cast a spell, a mesmerist must have a Charisma score equal to at least 10 + the spell’s level. The saving throw DC against a mesmerist’s spell is 10 + the spell’s level + the mesmerist’s Charisma modifier.\n"
                                             + "Like other spellcasters, a mesmerist can cast only a certain number of spells of each spell level per day. In addition, he receives bonus spells per day if he has a high Charisma score.\n"
                                             + "The mesmerist’s selection of spells is limited. A mesmerist begins play knowing four 0-level spells and two 1st-level spells of the mesmerist’s choice. At each new mesmerist level, he learns one or more new spells.",
                                             "",
                                             null,
                                             FeatureGroup.None);

            mesmerist_spellcasting.AddComponents(Helpers.Create<SpellFailureMechanics.PsychicSpellbook>(p => p.spellbook = mesmerist_spellbook),
                                               Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.NaturalSpell));
            mesmerist_spellcasting.AddComponent(Helpers.Create<SpellbookMechanics.AddUndercastSpells>(p => p.spellbook = mesmerist_spellbook));
            mesmerist_spellcasting.AddComponent(Helpers.CreateAddFact(Investigator.center_self));
            mesmerist_spellcasting.AddComponents(Common.createCantrips(mesmerist_class, StatType.Charisma, mesmerist_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
            mesmerist_spellcasting.AddComponents(Helpers.CreateAddFacts(mesmerist_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));

            return mesmerist_spellbook;
        }
    }
}
