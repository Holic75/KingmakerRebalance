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
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace KingmakerRebalance
{
    class Bloodrager
    {
        static internal LibraryScriptableObject library => Main.library;
        static internal BlueprintCharacterClass bloodrager_class;
        static internal BlueprintProgression bloodrager_progression;
        static internal BlueprintBuff bloodrage_buff;

        //aberrant - reach?
        //bloodlines
        //abbyssal - everything seems to be doable
        //arcane - diruptive ?, caster's scourge?
        //celestial - conviction?
        //draconic  - ok
        //elemental - power of the elements?
        //fey - leaping charge
        //infernal - ok
        //undead - frightfull charge? 

        internal static void createBloodragerClass()
        {
            var barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

            bloodrager_class = Helpers.Create<BlueprintCharacterClass>();
            bloodrager_class.name = "BloodragerClass";
            library.AddAsset(bloodrager_class, "cf217eb4f8504d67aad37464cee966f8");

            bloodrager_class.LocalizedName = Helpers.CreateString("Bloodrager.Name", "Bloodrager");
            bloodrager_class.LocalizedDescription = Helpers.CreateString("Bloodrager.Description",
                                                                         "While many ferocious combatants can tap into a deep reservoir of buried rage, bloodragers have an intrinsic power that seethes within.Like sorcerers, bloodragers’ veins surge with arcane power.While sorcerers use this power for spellcasting, bloodragers enter an altered state in which their bloodline becomes manifest, where the echoes of their strange ancestry lash out with devastating power. In these states, bloodragers can cast some arcane spells instinctively.The bloodrager’s magic is as fast, violent, and seemingly unstoppable as their physical prowess.\n"
                                                                         + "Role: Masters of the battlefield, bloodragers unleash fearful carnage on their enemies using their bloodlines and combat prowess.The bloodrager’s place is on the front lines, right in his enemies’ faces, supplying tremendous martial force bolstered by a trace of arcane magic."
                                                                         );
            bloodrager_class.m_Icon = barbarian_class.Icon;
            bloodrager_class.SkillPoints = barbarian_class.SkillPoints;
            bloodrager_class.HitDie = DiceType.D10;
            bloodrager_class.BaseAttackBonus = barbarian_class.BaseAttackBonus;
            bloodrager_class.FortitudeSave = barbarian_class.FortitudeSave;
            bloodrager_class.ReflexSave = barbarian_class.ReflexSave;
            bloodrager_class.WillSave = barbarian_class.WillSave;
            bloodrager_class.Spellbook = createBloodragerSpellbook();
            bloodrager_class.ClassSkills = new StatType[] {StatType.SkillKnowledgeArcana, StatType.SkillMobility, StatType.SkillLoreNature, StatType.SkillPerception, StatType.SkillAthletics,
                                                      StatType.SkillPersuasion};
            bloodrager_class.IsDivineCaster = false;
            bloodrager_class.IsArcaneCaster = true;
            bloodrager_class.StartingGold = barbarian_class.StartingGold;
            bloodrager_class.PrimaryColor = barbarian_class.PrimaryColor;
            bloodrager_class.SecondaryColor = barbarian_class.SecondaryColor;
            bloodrager_class.RecommendedAttributes = new StatType[] { StatType.Strength, StatType.Charisma, StatType.Constitution };
            bloodrager_class.NotRecommendedAttributes = new StatType[] { StatType.Intelligence };
            bloodrager_class.EquipmentEntities = barbarian_class.EquipmentEntities;
            bloodrager_class.MaleEquipmentEntities = barbarian_class.MaleEquipmentEntities;
            bloodrager_class.FemaleEquipmentEntities = barbarian_class.FemaleEquipmentEntities;
            bloodrager_class.ComponentsArray = barbarian_class.ComponentsArray;
            bloodrager_class.StartingItems = barbarian_class.StartingItems;
            createBloodragerProgression();
            bloodrager_class.Progression = bloodrager_progression;

            bloodrager_class.Archetypes = new BlueprintArchetype[] {};
            Helpers.RegisterClass(bloodrager_class);
        }


        static BlueprintCharacterClass[] getBloodragerArray()
        {
            return new BlueprintCharacterClass[] { bloodrager_class };
        }


        static void createBloodragerProgression()
        {
            bloodrager_progression = Helpers.CreateProgression("BloodragerProgression",
                           bloodrager_class.Name,
                           bloodrager_class.Description,
                           "ca69bcd88a6a4fd685f6a1a9744ed518",
                           bloodrager_class.Icon,
                           FeatureGroup.None);
            bloodrager_progression.Classes = getBloodragerArray();

            var bloodrager_proficiencies = library.CopyAndAdd<BlueprintFeature>("acc15a2d19f13864e8cce3ba133a1979", //barbarian proficiencies
                                                                            "BloodragerProficiencies",
                                                                            "207950c35a6a48bb895325d4dab02b75");
            bloodrager_proficiencies.AddComponent(Common.createArcaneArmorProficiency(Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Buckler,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.LightShield,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.HeavyShield,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.TowerShield,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Light,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Medium)
                                                                                      );
            bloodrager_proficiencies.SetName("Bloodrager Proficiencies");
            bloodrager_proficiencies.SetDescription("Bloodragers are proficient with all simple and martial weapons, light armor, medium armor, and shields (except tower shields). A bloodrager can cast bloodrager spells while wearing light armor or medium armor without incurring the normal arcane spell failure chance. This does not affect the arcane spell failure chance for arcane spells received from other classes. Like other arcane spellcasters, a bloodrager wearing heavy armor or wielding a shield incurs a chance of arcane spell failure if the spell in question has somatic components.");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            var fast_movement = library.Get<BlueprintFeature>("d294a5dddd0120046aae7d4eb6cbc4fc");
            var uncanny_dodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            var improved_uncanny_dodge = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");
            var damage_reduction = library.Get<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8");
            var indomitable_will = library.Get<BlueprintFeature>("e9ae7276574c170468937b617d993357");

            var rage = library.Get<BlueprintFeature>("2479395977cfeeb46b482bc3385f4647");
            var greater_rage = library.Get<BlueprintFeature>("ce49c579fe0bcc647a32c96929fae982");
            var tireless_rage = library.Get<BlueprintFeature>("ca9343d75a83a2745a22fa11c383153a");
            var mighty_rage = library.Get<BlueprintFeature>("06a7e5b60020ad947aed107d82d1f897");

            bloodrager_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, bloodrager_proficiencies, rage, fast_movement, detect_magic,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, uncanny_dodge),
                                                                    Helpers.LevelEntry(5, improved_uncanny_dodge),
                                                                    Helpers.LevelEntry(7, damage_reduction),
                                                                    Helpers.LevelEntry(10, damage_reduction),
                                                                    Helpers.LevelEntry(11, greater_rage),
                                                                    Helpers.LevelEntry(13, damage_reduction),
                                                                    Helpers.LevelEntry(14, indomitable_will),
                                                                    Helpers.LevelEntry(16, damage_reduction),
                                                                    Helpers.LevelEntry(17, tireless_rage),
                                                                    Helpers.LevelEntry(19, damage_reduction),
                                                                    Helpers.LevelEntry(20, mighty_rage)
                                                                    };

            bloodrager_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { bloodrager_proficiencies, detect_magic};
            bloodrager_progression.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(rage, greater_rage, tireless_rage, mighty_rage) };

            //fix rage resource to work for bloodrager
            var rage_resource = library.Get<Kingmaker.Blueprints.BlueprintAbilityResource>("24353fcf8096ea54684a72bf58dedbc9");
            var amount = Helpers.GetField(rage_resource, "m_MaxAmount");
            BlueprintCharacterClass[] classes = (BlueprintCharacterClass[])Helpers.GetField(amount, "Class");
            classes = classes.AddToArray(bloodrager_class);
            Helpers.SetField(amount, "Class", classes);
            Helpers.SetField(rage_resource, "m_MaxAmount", amount);

            //allow to cast spells while in rage
            //It is possible to make a separate rage buff for bloodrager different from standard rage,
            //but it is apparently impossible to allow casting only bloodrager spells while under the effect of such buff
            bloodrage_buff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            bloodrage_buff.RemoveComponent(bloodrage_buff.GetComponent<Kingmaker.UnitLogic.FactLogic.ForbidSpellCasting>());
        }


        static BlueprintSpellbook createBloodragerSpellbook()
        {
            var bloodrager_spellbook = Helpers.Create<BlueprintSpellbook>();
            bloodrager_spellbook.name = "BloodragerSpellbook";
            library.AddAsset(bloodrager_spellbook, "e3a80ac9beb441af8a7285eaf99a3a8b");
            bloodrager_spellbook.Name = bloodrager_class.LocalizedName;
            bloodrager_spellbook.SpellsPerDay = Common.createSpontaneousHalfCasterSpellsPerDay("BloodragerSpellsPerDayTable", "48e4349ce7dd4d399a9ebaf5aed21372");

            bloodrager_spellbook.SpellsKnown = Common.createSpontaneousHalfCasterSpellsKnown("BloodragerSpellsKnown", "f0110eac4ecc4da08d28fa73ed514e59");
            bloodrager_spellbook.Spontaneous = true;
            bloodrager_spellbook.IsArcane = true;
            bloodrager_spellbook.AllSpellsKnown = false;
            bloodrager_spellbook.CanCopyScrolls = false;
            bloodrager_spellbook.CastingAttribute = StatType.Charisma;
            bloodrager_spellbook.CharacterClass = bloodrager_class;
            bloodrager_spellbook.CasterLevelModifier = 0;
            bloodrager_spellbook.CantripsType = CantripsType.Cantrips;
            bloodrager_spellbook.SpellsPerLevel = 0;

            bloodrager_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            bloodrager_spellbook.SpellList.name = "BloodragerSpellList";
            library.AddAsset(bloodrager_spellbook.SpellList, "e93c0b4113f2498f8f206b3fe02f7964");
            bloodrager_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < bloodrager_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                bloodrager_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);

            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "4783c3709a74a794dbe7c8e7e0b1b038", 1), //burning hands
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( "91da41b9793a4624797921f221db653c", 1), //color sparay
                new Common.SpellId( "1a40fc88aeac9da4aa2fbdbb88335f5d", 1), //corrosive touch
                new Common.SpellId( "8e7cfa5f213a90549aadd18f8f6f4664", 1), //ear piercing scream
                new Common.SpellId( "c60969e7f264e6d4b84a1499fdcf9039", 1), //enlarge person
                new Common.SpellId( "b065231094a21d14dbf1c3832f776871", 1), //fire belly
                new Common.SpellId( "39a602aa80cc96f4597778b6d4d49c0a", 1), //flare burst
                new Common.SpellId( "9e1ad5d6f87d19e4d8883d63a6e35568", 1), //mage armor
                new Common.SpellId( "4ac47ddb9fa1eaf43a1b6809980cfbd2", 1), //magic missile
                new Common.SpellId( "ef768022b0785eb43a18969903c537c4", 1), //mage shield
                new Common.SpellId( "433b1faf4d02cc34abb0ade5ceda47c4", 1), //protection from alignment
                new Common.SpellId( "450af0402422b0b4980d9c2175869612", 1), //ray of enfeeblement
                new Common.SpellId( "fa3078b9976a5b24caf92e20ee9c0f54", 1), //ray of sickening
                new Common.SpellId( "4e0e9aba6447d514f88eff1464cc4763", 1), //reduce person
                new Common.SpellId( "ab395d2335d3f384e99dddee8562978f", 1), //shocking grasp
                new Common.SpellId( "9f10909f0be1f5141bf1c102041f93d9", 1), //snowball
                new Common.SpellId( "85067a04a97416949b5d1dbf986d93f3", 1), //stone fist
                new Common.SpellId( "5d38c80a819e8084ba19b29a865312c2", 1), //touch of gracelessness
                new Common.SpellId( "2c38da66e5a599347ac95b3294acbe00", 1), //true strike

                new Common.SpellId( "9a46dfd390f943647ab4395fc997936d", 2), //acid arrow
                new Common.SpellId( "a900628aea19aa74aad0ece0e65d091a", 2), //bear's endurance
                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 2), //blindness
                new Common.SpellId( "de7a025d48ad5da4991e7d3c682cf69d", 2), //cats grace
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagles splendor
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( "c83447189aabc72489164dfc246f3a36", 2), //frigid touch
                new Common.SpellId( "ce7dad2b25acf85429b6c9550787b2d9", 2), //glitterdust
                new Common.SpellId( "42a65895ba0cb3a42b6019039dd2bff1", 2), //molten orb
                new Common.SpellId( "3e4ab69ada402d145a5e0ad3ad4b8564", 2), //mirror image
                new Common.SpellId( "cdb106d53c65bbc4086183d54c3b97c7", 2), //scorching ray
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( "5181c2ed0190fc34b8a1162783af5bf4", 2), //stone call

                new Common.SpellId( "61a7ed778dd93f344a5dacdbad324cc9", 3), //beast shape 1
                new Common.SpellId( "2d81362af43aeac4387a3d4fced489c3", 3), //fireball
                new Common.SpellId( "486eaff58293f6441a5c2759c4872f98", 3), //haste
                new Common.SpellId( "5ab0d42fb68c9e34abae4921822b9d63", 3), //heroism
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 3), //hold person
                new Common.SpellId( "d2cff9243a7ee804cb6d5be47af30c73", 3), //lightning bolt
                new Common.SpellId( "d2f116cfe05fcdd4a94e80143b67046f", 3), //protection from energy
                new Common.SpellId( "97b991256e43bb140b263c326f690ce2", 3), //rage
                new Common.SpellId( "68a9e6d7256f1354289a39003a46d826", 3), //stinking cloud
                new Common.SpellId( "6cbb040023868574b992677885390f92", 3), //vampyric touch

                new Common.SpellId( "5d4028eb28a106d4691ed1b92bbb1915", 4), //beast shape 2
                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 4), //bestow curse
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 4), //confusion
                new Common.SpellId( "4baf4109145de4345861fe0f2209d903", 4), //crushing despair
                new Common.SpellId( "f72f8f03bf0136c4180cd1d70eb773a5", 4), //controlled fireball
                new Common.SpellId( "5e826bcdfde7f82468776b55315b2403", 4), //dragon breath
                new Common.SpellId( "690c90a82bf2e58449c6b541cb8ea004", 4), //elemental body 1
                new Common.SpellId( "f34fb78eaaec141469079af124bcfa0f", 4), //enervation
                new Common.SpellId( "66dc49bf154863148bd217287079245e", 4), //enlarge person mass
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life, greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( "fcb028205a71ee64d98175ff39a0abf9", 4), //ice storm
                new Common.SpellId( "2427f2e3ca22ae54ea7337bbab555b16", 4), //reduce person mass
                new Common.SpellId( "f09453607e683784c8fca646eec49162", 4), //shout
                new Common.SpellId( "1e481e03d9cf1564bae6b4f63aed2d1a", 4), //touch of slime
                new Common.SpellId( "16ce660837fb2544e96c3b7eaad73c63", 4), //volcanic storm
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(bloodrager_spellbook.SpellList, spell_id.level);
            }

            return bloodrager_spellbook;
        }

    }
}
