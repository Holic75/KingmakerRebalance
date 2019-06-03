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
        static internal BlueprintFeatureSelection bloodline_selection;

        static internal BlueprintFeature bloodrage;
        static internal BlueprintFeature greater_bloodrage;
        static internal BlueprintFeature mighty_bloodrage;
        static internal BlueprintFeature tireless_bloodrage;
        static internal BlueprintBuff bloodrage_buff;
        static internal BlueprintFeature damage_reduction;

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

            bloodrager_class.Archetypes = new BlueprintArchetype[] { };
            Helpers.RegisterClass(bloodrager_class);
        }


        static BlueprintCharacterClass[] getBloodragerArray()
        {
            return new BlueprintCharacterClass[] { bloodrager_class };
        }


        static void createBloodragerProgression()
        {

            createBloodrage();
            creatBloodlineSelection();

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

            var indomitable_will = library.Get<BlueprintFeature>("e9ae7276574c170468937b617d993357");


            bloodrager_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, bloodrager_proficiencies, bloodrage, bloodline_selection, fast_movement, detect_magic,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, uncanny_dodge),
                                                                    Helpers.LevelEntry(5, improved_uncanny_dodge),
                                                                    Helpers.LevelEntry(7, damage_reduction),
                                                                    Helpers.LevelEntry(10, damage_reduction),
                                                                    Helpers.LevelEntry(11, greater_bloodrage),
                                                                    Helpers.LevelEntry(13, damage_reduction),
                                                                    Helpers.LevelEntry(14, indomitable_will),
                                                                    Helpers.LevelEntry(16, damage_reduction),
                                                                    Helpers.LevelEntry(17, tireless_bloodrage),
                                                                    Helpers.LevelEntry(19, damage_reduction),
                                                                    Helpers.LevelEntry(20, mighty_bloodrage)
                                                                    };

            bloodrager_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { bloodrager_proficiencies, detect_magic, bloodline_selection };
            bloodrager_progression.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(bloodrage, greater_bloodrage, tireless_bloodrage, mighty_bloodrage) };
        }


        static void createBloodrage()
        {
            //we are going to use barbarian rage as a bloodrage, at least for the time being
            bloodrage = library.Get<BlueprintFeature>("2479395977cfeeb46b482bc3385f4647");//barbarian rage feature
            greater_bloodrage = library.Get<BlueprintFeature>("ce49c579fe0bcc647a32c96929fae982");
            tireless_bloodrage = library.Get<BlueprintFeature>("ca9343d75a83a2745a22fa11c383153a");
            mighty_bloodrage = library.Get<BlueprintFeature>("06a7e5b60020ad947aed107d82d1f897");
            //fix rage resource to work for bloodrager
            var rage_resource = library.Get<Kingmaker.Blueprints.BlueprintAbilityResource>("24353fcf8096ea54684a72bf58dedbc9");
            var amount = Helpers.GetField(rage_resource, "m_MaxAmount");
            BlueprintCharacterClass[] classes = (BlueprintCharacterClass[])Helpers.GetField(amount, "Class");
            classes = classes.AddToArray(bloodrager_class);
            Helpers.SetField(amount, "Class", classes);
            Helpers.SetField(rage_resource, "m_MaxAmount", amount);

            //allow to cast spells while in rage
            //It is possible to make a separate rage buff for bloodrager, different from standard rage,
            //but it is apparently impossible to allow casting only bloodrager spells while under the effect of such buff
            bloodrage_buff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            bloodrage_buff.RemoveComponent(bloodrage_buff.GetComponent<Kingmaker.UnitLogic.FactLogic.ForbidSpellCasting>());
            // all to use rage out of combat for debug purposes
            var rage = library.Get<BlueprintActivatableAbility>("df6a2cce8e3a9bd4592fb1968b83f730");
            rage.IsOnByDefault = false;
            rage.DeactivateIfCombatEnded = false;

            //we will use damage reduction of barbarian
            damage_reduction = library.Get<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8");
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


        static void creatBloodlineSelection()
        {
            AberrantBloodline.create();
            AbyssalBloodline.create();

            bloodline_selection = Helpers.CreateFeatureSelection("BloodragerBloodlineSelection",
                                                                     "Bloodline",
                                                                     "Each bloodrager has a source of magic somewhere in his heritage that empowers his bloodrages, bonus feats, and bonu spells. Sometimes this source reflects a distant blood relationship to a powerful being, or is due to an extreme event involving such a creature somewhere in his family’s past. Regardless of the source, this influence manifests in a number of ways. A bloodrager must pick one bloodline upon taking his first level of bloodrager. Once made, this choice cannot be changed.\n"
                                                                     + "When choosing a bloodline, the bloodrager’s alignment doesn’t restrict his choices.A good bloodrager could come from an abyssal bloodline, a celestial bloodline could beget an evil bloodrager generations later, a bloodrager from an infernal bloodline could be chaotic, and so on.Though his bloodline empowers him, it doesn’t dictate or limit his thoughts and behavior.\n"
                                                                     + "The bloodrager gains bloodline powers at 1st level, 4th level, and every 4 levels thereafter.The bloodline powers a bloodrager gains are described in his chosen bloodline.For all spell - like bloodline powers, treat the character’s bloodrager level as the caster level.\n"
                                                                     + "At 6th level and every 3 levels thereafter, a bloodrager receives one bonus feat chosen from a list specific to each bloodline.The bloodrager must meet the prerequisites for these bonus feats.At 7th, 10th, 13th, and 16th levels, a bloodrager learns an additional spell derived from his bloodline.",
                                                                     "6eed80b1bfa9425e90c5981fb87dedf2",
                                                                     null,
                                                                     FeatureGroup.None);
            bloodline_selection.AllFeatures = new BlueprintFeature[] { AberrantBloodline.progression, AbyssalBloodline.progression };
            bloodline_selection.Features = bloodline_selection.AllFeatures;
        }


        class AbyssalBloodline
        {

            static internal BlueprintProgression progression;
            static internal BlueprintFeature claws;
            static internal BlueprintFeature demonic_bulk;
            static internal BlueprintFeature demonic_resistances;
            static internal BlueprintFeature abyssal_bloodrage;
            static internal BlueprintFeature demonic_aura;
            static internal BlueprintFeature demonic_immunities;

            static internal void create()
            {
                createClaws();
                createDemonicBulk();
                createDemonicResistances();
                createAbyssalBloodrage();
                createDemonicAura();
                createDemonicImmunities();

                var ray_of_enfeeblement = library.Get<BlueprintAbility>("450af0402422b0b4980d9c2175869612");
                var bulls_strength = library.Get<BlueprintAbility>("4c3d08935262b6544ae97599b3a9556d");
                var rage = library.Get<BlueprintAbility>("97b991256e43bb140b263c326f690ce2");
                var stoneskin = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b");

                var cleave = library.Get<BlueprintFeature>("d809b6c4ff2aaff4fa70d712a70f7d7b");
                var great_fortitude = library.Get<BlueprintFeature>("79042cb55f030614ea29956177977c52");
                var improved_bull_rush = library.Get<BlueprintFeature>("b3614622866fe7046b787a548bbd7f59");
                var improved_sunder = library.Get<BlueprintFeature>("9719015edcbf142409592e2cbaab7fe1");
                var intimidating_prowess = library.Get<BlueprintFeature>("d76497bfc48516e45a0831628f767a0f");
                var power_attack = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
                var toughness = library.Get<BlueprintFeature>("d09b20029e9abfe4480b356c92095623");


                progression = createBloodragerBloodline("Abyssal",
                                                              "Generations ago, a demon spread its filth into the essence of your bloodline. While it doesn’t manifest in all of your kin, in those moments when you’re bloodraging, you embody its terrifying presence.\n"
                                                               + "Bonus Feats: Cleave, Great Fortitude, Improved Bull Rush, Improved Sunder, Intimidating Prowess, Power Attack, Toughness.\n"
                                                               + "Bonus Spells: Ray of enfeeblement(7th), bull’s strength(10th), rage(13th), stoneskin(16th).",
                                                              library.Get<BlueprintProgression>("d3a4cb7be97a6694290f0dcfbd147113").Icon, //sorceror bloodline
                                                              new BlueprintAbility[] { ray_of_enfeeblement, bulls_strength, rage, stoneskin },
                                                              new BlueprintFeature[] { cleave, great_fortitude, improved_bull_rush, improved_sunder, intimidating_prowess, power_attack, toughness },
                                                              new BlueprintFeature[] { claws, demonic_bulk, demonic_resistances, abyssal_bloodrage, demonic_aura, demonic_immunities },
                                                              "8a06875a5d1b4f3e93d4f7e1b54e3356",
                                                              new string[] { "1ebaa0951c4c43b69462fb6ccf0dde3f", "bbe03ed8293e435782dbfa6bc0ad10bf", "6e26e5974ab4481fa6127783bbac301c", "b7b57562edac4e01955dfa63e956b01b" },
                                                              "ccd945bba7784ae981ac66e8138439d5"
                                                              );

            }


            static void createClaws()
            {
                //claws
                var claw1d6 = library.Get<BlueprintItemWeapon>("d40ae466ba750bf4495a174e399d85ce"); //from sorc abbys bloodline
                var claw1d8 = library.CopyAndAdd<BlueprintItemWeapon>("d40ae466ba750bf4495a174e399d85ce", "BloodlineAbyssalClaw1d8", "350d22105211463ebcb998e9740d00a1");
                Helpers.SetField(claw1d8, "m_DamageDice", new Kingmaker.RuleSystem.DiceFormula(1, DiceType.D8));
                var claw1d8Fire = library.CopyAndAdd<BlueprintItemWeapon>("6e2487c8fb0501841b508e5918b36cb9", "BloodlineAbyssalClaw1d8Fire", "9f147636d23c4a04a2a575e6eb601bfa");
                Helpers.SetField(claw1d8Fire, "m_DamageDice", new Kingmaker.RuleSystem.DiceFormula(1, DiceType.D8));

                var claw_buff1 = library.CopyAndAdd<BlueprintBuff>("4a51dca9d9456214e9a382b9e47385b3", "BloodragerBloodlineAbyssalClaw1Buff", "6786313f39c044ad9348bdfc163cf45f");
                claw_buff1.ReplaceComponent<Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride>(Common.createEmptyHandWeaponOverride(claw1d6));
                var claw_buff2 = library.Get<BlueprintBuff>("cec6fcd5be2175f4e888f7c79ce68db6"); //from sorcerer bloodline
                var claw_buff3 = library.CopyAndAdd<BlueprintBuff>("cec6fcd5be2175f4e888f7c79ce68db6", "BloodragerBloodlineAbyssalClaw3Buff", "9af5c8658f3f42c99cc61e58b1cae7ac");
                claw_buff3.ReplaceComponent<Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride>(Common.createEmptyHandWeaponOverride(claw1d8));
                var claw_buff4 = library.CopyAndAdd<BlueprintBuff>("cec6fcd5be2175f4e888f7c79ce68db6", "BloodragerBloodlineAbyssalClaw4Buff", "d255c953ad424b1f8e73e98b318e0d64");
                claw_buff4.ReplaceComponent<Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride>(Common.createEmptyHandWeaponOverride(claw1d8Fire));

                var claws1_feature = Helpers.CreateFeature("BloodragerBloodlineAbyssalClaws1Feature", "Claws",
                                                          "At 1st level, you grow claws while bloodraging. These claws are treated as natural weapons, allowing you to make two claw attacks as a full attack, using your full base attack bonus. These attacks deal 1d6 points of damage each (1d4 if you are Small) plus your Strength modifier. At 4th level, these claws are considered magic weapons for the purpose of overcoming damage resistance. At 8th level, the damage increases to 1d8 points (1d6 if you are Small). At 12th level, these claws become flaming weapons, which deal an additional 1d6 points of fire damage on a hit.",
                                                          "beb37be3879744b08b1def72cb4fb2d9",
                                                          claw1d6.Icon,
                                                          FeatureGroup.None);
                claws1_feature.HideInCharacterSheetAndLevelUp = true;
                var claws2_feature = Helpers.CreateFeature("BloodragerBloodlineAbyssalClaws2Feature", "Claws",
                                                          claws1_feature.Description,
                                                          "d5d7cfbbefc740d2b1047dd8ae4a9651",
                                                          claws1_feature.Icon,
                                                          FeatureGroup.None,
                                                          Common.createRemoveFeatureOnApply(claws1_feature));
                claws2_feature.HideInCharacterSheetAndLevelUp = true;
                var claws3_feature = Helpers.CreateFeature("BloodragerBloodlineAbyssalClaws3Feature", "Claws",
                                                          claws1_feature.Description,
                                                          "98cb5e2a4a7240dfa8f8de58136bc738",
                                                          claws1_feature.Icon,
                                                          FeatureGroup.None,
                                                          Common.createRemoveFeatureOnApply(claws2_feature),
                                                          Common.createRemoveFeatureOnApply(claws1_feature));
                claws3_feature.HideInCharacterSheetAndLevelUp = true;
                var claws4_feature = Helpers.CreateFeature("BloodragerBloodlineAbyssalClaws3Feature", "Claws",
                                                          claws1_feature.Description,
                                                          "e8d6d66c04bd495ba24824feaf537cf6",
                                                          claws1_feature.Icon,
                                                          FeatureGroup.None,
                                                          Common.createRemoveFeatureOnApply(claws3_feature),
                                                          Common.createRemoveFeatureOnApply(claws2_feature),
                                                          Common.createRemoveFeatureOnApply(claws1_feature));
                claws4_feature.HideInCharacterSheetAndLevelUp = true;
                claws = Helpers.CreateFeature("BloodragerBloodlineAbyssalClawsFeature", "Claws",
                                                          claws1_feature.Description,
                                                          "6897f6702f0144f4b468cd2ce6c22390",
                                                          claws1_feature.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFeatureOnClassLevel(claws1_feature, 1, getBloodragerArray(), null),
                                                          Helpers.CreateAddFeatureOnClassLevel(claws2_feature, 4, getBloodragerArray(), null),
                                                          Helpers.CreateAddFeatureOnClassLevel(claws3_feature, 8, getBloodragerArray(), null),
                                                          Helpers.CreateAddFeatureOnClassLevel(claws4_feature, 12, getBloodragerArray(), null)
                                                          );

                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, claw_buff1, claws1_feature);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, claw_buff2, claws2_feature);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, claw_buff3, claws3_feature);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, claw_buff4, claws4_feature);
            }


            static void createDemonicBulk()
            {
                var reckless_stance = library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");
                var enlarge_buff = library.Get<BlueprintBuff>("4f139d125bb602f48bfaec3d3e1937cb");
                var demonic_bulk_switch_buff = Helpers.CreateBuff("BloodragerBloodlineAbyssalDemonicBulkSwitchBuff",
                                                                  "Demonic Bulk",
                                                                  "At 4th level, when entering a bloodrage, you can choose to grow one size category larger than your base size (as enlarge person) even if you aren’t humanoid.",
                                                                  "00c460abf3e94f7fa7a69c6da5cb2741",
                                                                  enlarge_buff.Icon,
                                                                  null,
                                                                  Helpers.CreateEmptyAddFactContextActions());
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(demonic_bulk_switch_buff, enlarge_buff, bloodrage_buff);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, enlarge_buff, demonic_bulk_switch_buff);

                var demonic_bulk_ability = Helpers.CreateActivatableAbility("BloodragerBloodlineAbyssalDemonicBulkToggleAbility",
                                                                            demonic_bulk_switch_buff.Name,
                                                                            demonic_bulk_switch_buff.Description,
                                                                            "d6dd7b8642624f50997b2d580b8e18fa",
                                                                            enlarge_buff.Icon,
                                                                            demonic_bulk_switch_buff,
                                                                            AbilityActivationType.Immediately,
                                                                            CommandType.Standard,
                                                                            reckless_stance.ActivateWithUnitAnimation);
                demonic_bulk = Helpers.CreateFeature("BloodragerBloodlineAbyssalDemonicBulkFeature",
                                                                        demonic_bulk_switch_buff.Name,
                                                                        demonic_bulk_switch_buff.Description,
                                                                        "4f2896b7dcda42dd89a44550381b7173",
                                                                        enlarge_buff.Icon,
                                                                        FeatureGroup.None,
                                                                        KingmakerRebalance.Helpers.CreateAddFact(demonic_bulk_ability));
            }


            static void createDemonicResistances()
            {
                var damage_resistance = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("8cbf303d479cf0d42a8e36092c76fa7c");
                var resist_caf = Helpers.CreateBuff("BloodragerAbyssalBloodlineDemonicResistancesBuff",
                                                    "Demon Resistances",
                                                    "At 8th level, you gain resistance 5 to acid, cold, and fire. At 16th level, these resistances increase to 10.",
                                                    "c888c8d801dc49ceac1cf022734afaaf",
                                                    damage_resistance.Icon,
                                                    null,
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Acid),
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Fire),
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Cold),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                    ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                    classes: getBloodragerArray(),
                                                    customProgression: new (int, int)[] {
                                                                (15, 5),
                                                                (20, 10)
                                                    })
                                                    );
                demonic_resistances = Helpers.CreateFeature("BloodragerAbyssalBloodlineDemonicResistancesFeature",
                                                                               resist_caf.Name,
                                                                               resist_caf.Description,
                                                                               "f5926c928da34a3ebaf0c9183742fb91",
                                                                               resist_caf.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, resist_caf, demonic_resistances);
            }

            static void createAbyssalBloodrage()
            {
                var abyssal_strength = library.Get<BlueprintFeature>("489c8c4a53a111d4094d239054b26e32");
                var buff = Helpers.CreateBuff("BloodragerAbyssalBloodlineAbyssalBloodrage",
                                                    "Abyssal Bloodrage",
                                                    "At 12th level, while bloodraging,  you receive +2 morale bonus to Strength, but the penalty to AC becomes –4 instead of –2. At 16th level, this bonus increases by 4 instead. At 20th level, it increases by 6 instead.",
                                                    "9f57b65210a34032aef5da55a7b7fa18",
                                                    abyssal_strength.Icon,
                                                    null,
                                                    Helpers.CreateAddStatBonus(StatType.AC, -2, ModifierDescriptor.UntypedStackable),
                                                    Helpers.CreateAddContextStatBonus(StatType.Strength, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                    ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                    classes: getBloodragerArray(),
                                                    customProgression: new (int, int)[] {
                                                                (15, 2),
                                                                (19, 4),
                                                                (20, 6)
                                                    })
                                                    );
                abyssal_bloodrage = Helpers.CreateFeature("BloodragerAbyssalBloodlineDemonicResistancesFeature",
                                                                               buff.Name,
                                                                               buff.Description,
                                                                               "9167d9245c9140ae94f834186c98b700",
                                                                               buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, buff, abyssal_bloodrage);
            }


            static void createDemonicAura()
            {
                var area_effect = new Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect();
                area_effect.name = "BloodragerAbyssalBloodlineDemonicAura";
                area_effect.AffectEnemies = true;
                area_effect.AggroEnemies = true;
                area_effect.Size = 5.Feet();
                area_effect.Shape = AreaEffectShape.Cylinder;
                var damage = Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(2), Helpers.CreateContextValue(AbilityRankType.DamageBonus));
                var damage_action = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, damage, isAoE: true);
                var conditional_damage = Helpers.CreateConditional(new Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionIsMainTarget(),
                                                                    null,
                                                                    damage_action);
                area_effect.AddComponent(Helpers.CreateAreaEffectRunAction(round: conditional_damage));
                area_effect.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus,
                                                                         stat: StatType.Constitution,
                                                                         classes: getBloodragerArray(),
                                                                         type: AbilityRankType.DamageBonus
                                                                         )
                                        );
                area_effect.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Fire));
                area_effect.Fx = new Kingmaker.ResourceLinks.PrefabLink();
                library.AddAsset(area_effect, "2a03a03228fb479f9f0810e87cfbbe95");

                var reckless_stance = library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");
                var firebelly = library.Get<BlueprintAbility>("b065231094a21d14dbf1c3832f776871");

                var demonic_aura_buff = Helpers.CreateBuff("BloodragerBloodlineAbyssalDemonicAuraBuff",
                                                                              "Demonic Aura",
                                                                              "At 16th level, when entering a bloodrage you can choose to exude an aura of fire. The aura is a 5-foot burst centered on you, and deals 2d6 + your Constitution modifier points of fire damage to creatures that end their turns within it.",
                                                                              "44d877ef2428424082761e94dd3d55b3",
                                                                              firebelly.Icon,
                                                                              null,
                                                                              Common.createAddAreaEffect(area_effect));

                var demonic_aura_switch_buff = Helpers.CreateBuff("BloodragerBloodlineAbyssalDemonicAuraSwitchBuff",
                                                                  demonic_aura_buff.Name,
                                                                  demonic_aura_buff.Description,
                                                                  "5680b4fb4da04366a651e9da1b7943a1",
                                                                  firebelly.Icon,
                                                                  null,
                                                                  Helpers.CreateEmptyAddFactContextActions());

                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(demonic_aura_switch_buff, demonic_aura_buff, bloodrage_buff);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, demonic_aura_buff, demonic_aura_switch_buff);

                var demonic_aura_ability = Helpers.CreateActivatableAbility("BloodragerBloodlineAbyssalDemonicAuraToggleAbility",
                                                                            demonic_aura_switch_buff.Name,
                                                                            demonic_aura_switch_buff.Description,
                                                                            "610e508618df41e8bb32aff78c326e9f",
                                                                            firebelly.Icon,
                                                                            demonic_aura_switch_buff,
                                                                            AbilityActivationType.Immediately,
                                                                            CommandType.Standard,
                                                                            reckless_stance.ActivateWithUnitAnimation);
                demonic_aura = Helpers.CreateFeature("BloodragerBloodlineAbyssalDemonicBulkFeature",
                                                                        demonic_aura_switch_buff.Name,
                                                                        demonic_aura_switch_buff.Description,
                                                                        "b6df9a3984114c14881bf4b2d5aa5a47",
                                                                        firebelly.Icon,
                                                                        FeatureGroup.None,
                                                                        KingmakerRebalance.Helpers.CreateAddFact(demonic_aura_ability));

            }


            static void createDemonicImmunities()
            {
                demonic_immunities = library.CopyAndAdd<BlueprintFeature>("5c1c2ed7fe5f99649ab00605610b775b", //from sorceror bloodline
                                                                                            "BloodragerAbyssalBloodlineDemonicImmunitiesFeature",
                                                                                            "9f558f25202e451eb5b29c721a906a97");
                var resistances = demonic_immunities.GetComponents<Kingmaker.UnitLogic.FactLogic.AddDamageResistanceEnergy>().ToArray();

                foreach (var r in resistances)
                {
                    demonic_immunities.RemoveComponent(r);
                }
                demonic_immunities.SetName("Demonic Immunities");
                demonic_immunities.SetDescription("At 20th level, you’re immune to electricity and poison. You have this benefit constantly, even while not bloodraging.");
            }
        }


        class AberrantBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature staggering_strike;
            static internal BlueprintFeature abnormal_reach;
            static internal BlueprintFeature aberrant_fortitude;
            static internal BlueprintFeature unusual_anatomy;
            static internal BlueprintFeature aberrant_resistance;
            static internal BlueprintFeature aberrant_form;
            static string prefix = "BloodragerBloodlineAberrant";



            static internal void create()
            {
                createStaggeringStrike();
                createAbnormalReach();
                createAberrantFortitude();
                createUnusualAnatomy();
                createAberrantResistance();
                createAberrantForm();

                var enlarge_person = library.Get<BlueprintAbility>("c60969e7f264e6d4b84a1499fdcf9039");
                var see_invisibility = library.Get<BlueprintAbility>("30e5dc243f937fc4b95d2f8f4e1b7ff3");
                var displacement = library.Get<BlueprintAbility>("903092f6488f9ce45a80943923576ab3");
                var confusion = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949");

                var combat_reflexes = library.Get<BlueprintFeature>("0f8939ae6f220984e8fb568abbdfba95");
                var great_fortitude = library.Get<BlueprintFeature>("79042cb55f030614ea29956177977c52");
                var improved_disarm = library.Get<BlueprintFeature>("25bc9c439ac44fd44ac3b1e58890916f");
                var lightning_reflexes = library.Get<BlueprintFeature>("15e7da6645a7f3d41bdad7c8c4b9de1e");
                var improved_initiative = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
                var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");
                var iron_will = library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334");


                progression = createBloodragerBloodline("Aberrant",
                                                              "There is a taint in your blood that is both alien and bizarre. When you bloodrage, this manifests in peculiar and terrifying ways.\n"
                                                               + "Bonus Feats: Combat Reflexes, Great Fortitude, Improved Disarm, Improved Initiative, Improved Unarmed Strike, Iron Will, Lightning Reflexes.\n"
                                                               + "Bonus Spells: Enlarge person (7th), see invisibility (10th), displacement (13th), confusion (16th).",
                                                              library.Get<BlueprintAbility>("14ec7a4e52e90fa47a4c8d63c69fd5c1").Icon, //blur
                                                              new BlueprintAbility[] { enlarge_person, see_invisibility, displacement, confusion },
                                                              new BlueprintFeature[] { combat_reflexes, great_fortitude, improved_disarm, improved_initiative, improved_unarmed_strike, iron_will, lightning_reflexes },
                                                              new BlueprintFeature[] { staggering_strike, abnormal_reach, aberrant_fortitude, unusual_anatomy, aberrant_resistance, aberrant_form },
                                                              "b42a51e28d06456bad786a0405c3a892",
                                                              new string[] { "81d09d66ac3e4faf8e58f570e1551621", "feeb29835ff34bb2a7f692d465372565", "e96cd140a81b480aa6b52ac8bdb50813", "bdf50388d3d545ae8ea5d443c21b3388" },
                                                              "cd00b25671db470182daa9c129949991"
                                                              );
            }

            static void createStaggeringStrike()
            {
                var blade_barrier = library.Get<BlueprintAbility>("36c8971e91f1745418cc3ffdfac17b74");
                var stagerred_buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");

                var effect_action = Helpers.CreateActionList(Common.createContextSavedApplyBuff(stagerred_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1))));
                var action = Helpers.CreateActionList(Common.createContextActionSavingThrow(SavingThrowType.Fortitude, effect_action));
                var staggering_strike_buff = Helpers.CreateBuff(prefix + "StaggeringStrikeBuff",
                                                                "Staggering Strike",
                                                                "At 1st level, when you confirm a critical hit the target must succeed at a Fortitude saving throw or be staggered for 1 round. The DC of this save is equal to 10 + 1/2 your bloodrager level + your Constitution modifier. These effects stack with the Staggering Critical feat; the target must save against each effect individually.",
                                                                "58fb04c603b14382b23f2c7600691277",
                                                                blade_barrier.Icon,
                                                                null,
                                                                Common.createAddInitiatorAttackWithWeaponTrigger(action, critical_hit: true),
                                                                Common.createContextCalculateAbilityParamsBasedOnClass(bloodrager_class, StatType.Constitution)
                                                                );
                staggering_strike = Helpers.CreateFeature(prefix + "StaggeringStrikeFeature",
                                                               staggering_strike_buff.Name,
                                                               staggering_strike_buff.Description,
                                                               "a225ba5a080041b2b78f33b8a704e821",
                                                               staggering_strike_buff.Icon,
                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, staggering_strike_buff, staggering_strike);
            }


            static void createAbnormalReach()
            {
                var stone_fist = library.Get<BlueprintAbility>("85067a04a97416949b5d1dbf986d93f3");
                var abnormal_reach_buff = Helpers.CreateBuff(prefix + "AbnormalReachBuff",
                                                             "Abnormal Reach",
                                                             "At 4th level, your limbs elongate; your reach increases by 5 feet.",
                                                             "a2594e07ae5b489aaca41c6375a6d847",
                                                             stone_fist.Icon,
                                                             null,
                                                             Helpers.CreateAddStatBonus(StatType.Reach, 5, ModifierDescriptor.None)
                                                             );
                abnormal_reach = Helpers.CreateFeature(prefix + "AbnormalReachFeature",
                                               abnormal_reach_buff.Name,
                                               abnormal_reach_buff.Description,
                                               "19b74174db5a42c4b1d90ae9e54e8fe8",
                                               abnormal_reach_buff.Icon,
                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, abnormal_reach_buff, abnormal_reach);
            }


            static void createAberrantFortitude()
            {
                var defensive_stance = library.Get<BlueprintActivatableAbility>("be68c660b41bc9247bcab727b10d2cd1");
                var aberrant_fortitude_buff = Helpers.CreateBuff(prefix + "AberrantFortitudeBuff",
                                                                 "Aberrant Fortitude",
                                                                 "At 8th level, you become immune to the sickened and nauseated conditions.",
                                                                 "2f7c24016fa34388a3fa191609e75856",
                                                                 defensive_stance.Icon,
                                                                 null,
                                                                 Common.createAddConditionImmunity(UnitCondition.Nauseated),
                                                                 Common.createAddConditionImmunity(UnitCondition.Sickened),
                                                                 Common.createBuffDescriptorImmunity(SpellDescriptor.Sickened | SpellDescriptor.Nauseated)
                                                                 );
                aberrant_fortitude = Helpers.CreateFeature(prefix + "AberrantFortitudeFeature",
                                               aberrant_fortitude_buff.Name,
                                               aberrant_fortitude_buff.Description,
                                               "c3c215fb034e4b5b9f929f1d0c4e51a7",
                                               aberrant_fortitude_buff.Icon,
                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, aberrant_fortitude_buff, aberrant_fortitude);
            }


            static void createUnusualAnatomy()
            {
                var remove_disease = library.Get<BlueprintAbility>("4093d5a0eb5cae94e909eb1e0e1a6b36");
                var unusual_anatomy_buff = Helpers.CreateBuff(prefix + "UnusualAnatomyBuff",
                                                                 "Unusual Anatomy",
                                                                 "At 12th level, your internal anatomy shifts and changes, giving you a 50% chance to negate any critical hit or sneak attack that hits you. The damage is instead rolled normally.",
                                                                 "7cb52c6018ff4d7b9560341ba7240218",
                                                                 remove_disease.Icon,
                                                                 null,
                                                                 Common.createAddFortification(bonus: 50)
                                                                 );
                unusual_anatomy = Helpers.CreateFeature(prefix + "UnusualAnatomyFeature",
                                               unusual_anatomy_buff.Name,
                                               unusual_anatomy_buff.Description,
                                               "f8a61a0dd8bc49fbb7ae43b5bd1b4387",
                                               unusual_anatomy_buff.Icon,
                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, unusual_anatomy_buff, unusual_anatomy);
            }


            static void createAberrantResistance()
            {
                var spell_resistance = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889");
                var aberrant_resistance_buff = Helpers.CreateBuff(prefix + "AberrantResistanceBuff",
                                                                 "Aberrant Resistance",
                                                                 "At 16th level, you are immune to disease, exhaustion, fatigue, and poison, and to the staggered condition.",
                                                                 "2a916917494e4c188ce017aaa26c40e2",
                                                                 spell_resistance.Icon,
                                                                 null,
                                                                 Common.createAddConditionImmunity(UnitCondition.Exhausted),
                                                                 Common.createAddConditionImmunity(UnitCondition.Fatigued),
                                                                 Common.createAddConditionImmunity(UnitCondition.Staggered),
                                                                 Common.createBuffDescriptorImmunity(SpellDescriptor.Poison | SpellDescriptor.Disease | SpellDescriptor.Exhausted | SpellDescriptor.Fatigue | SpellDescriptor.Staggered)
                                                                 );
                aberrant_resistance = Helpers.CreateFeature(prefix + "AberrantResistanceFeature",
                                               aberrant_resistance_buff.Name,
                                               aberrant_resistance_buff.Description,
                                               "c35f914518a047c6b49ba024588357ef",
                                               aberrant_resistance_buff.Icon,
                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, aberrant_resistance_buff, aberrant_resistance);
            }


            static void createAberrantForm()
            {
                var constricting_coils = library.Get<BlueprintAbility>("3fce8e988a51a2a4ea366324d6153001");
                aberrant_form = Helpers.CreateFeature(prefix + "AberrantFormFeature",
                                                          "Aberrant Form",
                                                          "At 20th level, your body becomes truly unnatural. You are immune to critical hits and sneak attacks. In addition, you gain blindsight with a range of 60 feet and your bloodrager damage reduction increases by 1. You have these benefits constantly, even while not bloodraging.",
                                                          "aee46d11d5454482805ea2f6b5d7524b",
                                                          constricting_coils.Icon,
                                                          FeatureGroup.None,
                                                          Common.createBlindsight(60),
                                                          new AddImmunityToCriticalHits(),
                                                          new AddImmunityToPrecisionDamage()
                                                          );
                //add rank to dr
                var rank_config = damage_reduction.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>();
                var feature_list = Helpers.GetField<BlueprintFeature[]>(rank_config, "m_FeatureList");
                feature_list = feature_list.AddToArray(aberrant_form);
                Helpers.SetField(rank_config, "m_FeatureList", feature_list);
            }





        }



        static BlueprintProgression createBloodragerBloodline(string name, string description, UnityEngine.Sprite icon, 
                                                    BlueprintAbility[] bonus_spells, BlueprintFeature[] bonus_feats, BlueprintFeature[] powers,
                                                    string feat_selection_guid, string[] spell_guids, string progression_guid)
        {
            //spells at 7, 10, 13, 16
            //powers at 1, 4, 8, 12, 16, 20
            //feats at levels = 6, 9, 12, 15, 18
            var progression = Helpers.CreateProgression("Bloodrager" + name + "BloodlineProgression",
                                            name + " Bloodline",
                                            description,
                                            progression_guid,
                                            icon,
                                            FeatureGroup.BloodLine);


            var feat_selection = Helpers.CreateFeatureSelection("Bloodrager" + name + "BloodlineBonusFeatSelection",
                                                                "Bloodline Feat",
                                                                "At 6th level and every 3 levels thereafter, a bloodrager receives one bonus feat chosen from a list specific to each bloodline. The bloodrager must meet the prerequisites for these bonus feats.",
                                                                feat_selection_guid,
                                                                null,
                                                                FeatureGroup.None
                                                                );
            feat_selection.AllFeatures = bonus_feats;
            feat_selection.Features = bonus_feats;

            BlueprintFeature[] bloodline_spells = new BlueprintFeature[bonus_spells.Length];

            for (int i = 0; i < bloodline_spells.Length; i++)
            {
                bloodline_spells[i] = Helpers.CreateFeature("Bloodrager" + name + "BloodlineBonusSpell" + (i + 1).ToString(),
                                                            bonus_spells[i].Name,
                                                            "At 7th, 10th, 13th, and 16th levels, a bloodrager learns an additional spell derived from his bloodline.\n"
                                                            + bonus_spells[i].Description,
                                                            spell_guids[i],
                                                            bonus_spells[i].Icon,
                                                            FeatureGroup.None,
                                                            Helpers.CreateAddKnownSpell(bonus_spells[i], bloodrager_class, i + 1)
                                                            );
                bonus_spells[i].AddRecommendNoFeature(progression);
            }


            progression.Classes = getBloodragerArray();
            progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, powers[0]),
                                                         Helpers.LevelEntry(4, powers[1]),
                                                         Helpers.LevelEntry(6, feat_selection),
                                                         Helpers.LevelEntry(7, bloodline_spells[0]),
                                                         Helpers.LevelEntry(8, powers[2]),
                                                         Helpers.LevelEntry(9, feat_selection),
                                                         Helpers.LevelEntry(10, bloodline_spells[1]),
                                                         Helpers.LevelEntry(12, feat_selection, powers[3]),
                                                         Helpers.LevelEntry(13, bloodline_spells[2]),
                                                         Helpers.LevelEntry(15, feat_selection),
                                                         Helpers.LevelEntry(16, powers[4], bloodline_spells[3]),
                                                         Helpers.LevelEntry(18, feat_selection),
                                                         Helpers.LevelEntry(20, powers[5])
                                                        };
            progression.UIGroups = new UIGroup[] {Helpers.CreateUIGroup(powers),
                                                  Helpers.CreateUIGroup(bloodline_spells),
                                                  Helpers.CreateUIGroup(feat_selection, feat_selection, feat_selection, feat_selection, feat_selection)
                                                 };
       
            return progression;
        }


        
    }
}
