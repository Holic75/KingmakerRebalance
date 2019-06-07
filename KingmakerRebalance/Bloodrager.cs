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
        static internal bool test_mode = false;
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

            if (test_mode)
            {
                // allow to use rage out of combat for debug purposes
                var rage = library.Get<BlueprintActivatableAbility>("df6a2cce8e3a9bd4592fb1968b83f730");
                rage.IsOnByDefault = false;
                rage.DeactivateIfCombatEnded = false;
                // allow to use charge on allies
                var charge = library.Get<BlueprintAbility>("c78506dd0e14f7c45a599990e4e65038");
                charge.CanTargetFriends = true;
            }

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
                new Common.SpellId( "6cbb040023868574b992677885390f92", 3), //vampiric touch

                new Common.SpellId( "5d4028eb28a106d4691ed1b92bbb1915", 4), //beast shape 2
                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 4), //bestow curse
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 4), //confusion
                new Common.SpellId( "4baf4109145de4345861fe0f2209d903", 4), //crushing despair
                new Common.SpellId( "48e2744846ed04b4580be1a3343a5d3d", 4), //contagion
                new Common.SpellId( "f72f8f03bf0136c4180cd1d70eb773a5", 4), //controlled fireball
                new Common.SpellId( "5e826bcdfde7f82468776b55315b2403", 4), //dragon breath
                new Common.SpellId( "690c90a82bf2e58449c6b541cb8ea004", 4), //elemental body 1
                new Common.SpellId( "f34fb78eaaec141469079af124bcfa0f", 4), //enervation
                new Common.SpellId( "66dc49bf154863148bd217287079245e", 4), //enlarge person mass
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life, greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( "fcb028205a71ee64d98175ff39a0abf9", 4), //ice storm
                new Common.SpellId( "6717dbaef00c0eb4897a1c908a75dfe5", 4), //phantasmal killer
                new Common.SpellId( "2427f2e3ca22ae54ea7337bbab555b16", 4), //reduce person mass
                new Common.SpellId( "f09453607e683784c8fca646eec49162", 4), //shout
                new Common.SpellId( "c66e86905f7606c4eaa5c774f0357b2b", 4), //stoneskin
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
            CelestialBloodline.create();
            FeyBloodline.create();
            InfernalBloodline.create();
            UndeadBloodline.create();

            bloodline_selection = Helpers.CreateFeatureSelection("BloodragerBloodlineSelection",
                                                                     "Bloodline",
                                                                     "Each bloodrager has a source of magic somewhere in his heritage that empowers his bloodrages, bonus feats, and bonu spells. Sometimes this source reflects a distant blood relationship to a powerful being, or is due to an extreme event involving such a creature somewhere in his family’s past. Regardless of the source, this influence manifests in a number of ways. A bloodrager must pick one bloodline upon taking his first level of bloodrager. Once made, this choice cannot be changed.\n"
                                                                     + "When choosing a bloodline, the bloodrager’s alignment doesn’t restrict his choices.A good bloodrager could come from an abyssal bloodline, a celestial bloodline could beget an evil bloodrager generations later, a bloodrager from an infernal bloodline could be chaotic, and so on.Though his bloodline empowers him, it doesn’t dictate or limit his thoughts and behavior.\n"
                                                                     + "The bloodrager gains bloodline powers at 1st level, 4th level, and every 4 levels thereafter.The bloodline powers a bloodrager gains are described in his chosen bloodline.For all spell - like bloodline powers, treat the character’s bloodrager level as the caster level.\n"
                                                                     + "At 6th level and every 3 levels thereafter, a bloodrager receives one bonus feat chosen from a list specific to each bloodline.The bloodrager must meet the prerequisites for these bonus feats.At 7th, 10th, 13th, and 16th levels, a bloodrager learns an additional spell derived from his bloodline.",
                                                                     "6eed80b1bfa9425e90c5981fb87dedf2",
                                                                     null,
                                                                     FeatureGroup.None);
            bloodline_selection.AllFeatures = new BlueprintFeature[] { AberrantBloodline.progression, AbyssalBloodline.progression, CelestialBloodline.progression, FeyBloodline.progression,
                                                                       InfernalBloodline.progression, UndeadBloodline.progression};
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


        class CelestialBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature angelic_attacks;
            static internal BlueprintFeature celestial_resistances;
            static internal BlueprintFeature conviction;
            static internal BlueprintFeature wings_of_heaven;
            static internal BlueprintFeature angelic_protection;
            static internal BlueprintFeature ascension;
            static string prefix = "BloodragerBloodlineCelestial";


            static internal void create()
            {
                createAngelicAttacks();
                createCelestialResistances();
                createConviction();
                createWingsOfHeaven();
                createAngelicProtection();
                createAscension();

                var bless = library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638");
                var resist_energy = library.Get<BlueprintAbility>("21ffef7791ce73f468b6fca4d9371e8b");
                var heroism = library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63");
                var flamestrike = library.Get<BlueprintAbility>("f9910c76efc34af41b6e43d5d8752f0f");

                var dodge = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
                var mobility = library.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb");
                var dazzling_display = library.Get<BlueprintFeature>("bcbd674ec70ff6f4894bb5f07b6f4095");
                var cornugon_smash = library.Get<BlueprintFeature>("ceea53555d83f2547ae5fc47e0399e14");
                var improved_initiative = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
                var weapon_focus = library.Get<BlueprintFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
                var iron_will = library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334");


                progression = createBloodragerBloodline("Celestial",
                                                              "By way of a celestial ancestor or divine intervention, the blood of angels fills your body with a holy potency, granting you a majestic visage and angelic powers when you enter your bloodrage.\n"
                                                               + "Bonus Feats: Dodge, Mobility, Dazzling Display, Cornugon Smash, Improved Initiative, Iron Will, Weapon Focus.\n"
                                                               + "Bonus Spells: Bless (7th), resist energy (10th), heroism (13th), flamestrike (16th).",
                                                              library.Get<BlueprintAbility>("75a10d5a635986641bfbcceceec87217").Icon, //angelic aspect
                                                              new BlueprintAbility[] { bless, resist_energy, heroism, flamestrike },
                                                              new BlueprintFeature[] { dodge, mobility, dazzling_display, cornugon_smash, improved_initiative, iron_will, weapon_focus},
                                                              new BlueprintFeature[] { angelic_attacks, celestial_resistances, conviction, wings_of_heaven, angelic_protection, ascension},
                                                              "356f9a6169d8480da772f02a13e2da29",
                                                              new string[] { "0921bf94bf174c8b8e0361057761ba7a", "ca818409470d4a56b8619c3604af345b", "c7dfde63fb8a41afa3f0b1fc0020bcac", "9d196e52c01c43ddbcca7b8a941144e7" },
                                                              "205d951fa55a4d9ca10f50592f1868b3"
                                                              );
            }

            static void createAngelicAttacks()
            {
                var crusaders_edge = library.Get<BlueprintAbility>("a26c23a887a6f154491dc2cefdad2c35");
                var resounding_blow_buff = library.Get<BlueprintBuff>("06173a778d7067a439acffe9004916e9");

                var outsider_feature = library.Get<BlueprintFeature>("9054d3988d491d944ac144e27b6bc318");
                var evil_feature = library.Get<BlueprintFeature>("5279fc8380dd9ba419b4471018ffadd1");

                var bonus_damage = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Helpers.CreateConditionHasFact(outsider_feature), Helpers.CreateConditionHasFact(evil_feature)),
                                                             Helpers.CreateActionDealDamage(DamageEnergyType.Holy, Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)))
                                                            );
                var bonus_damage_action = Helpers.CreateActionList(bonus_damage);
                var good_weapon = Common.createAddOutgoingAlignment(DamageAlignment.Good, check_range: true, is_ranged: false);
                var angelic_attacks_buff = Helpers.CreateBuff(prefix + "AngelicAttacksBuff",
                                                             "Angelic Attacks",
                                                             "At 1st level, your melee attacks are considered good-aligned weapons for the purpose of bypassing damage reduction. Furthermore, when you deal damage with a melee attack to an evil outsider, you deal an additional 1d6 points of damage. This additional damage stacks with effects such as align weapon and those granted by a weapon with the holy weapon special ability.",
                                                             "fcba2a8679de4213b31ecdbc9ab7a9ad",
                                                             crusaders_edge.Icon,
                                                             resounding_blow_buff.FxOnStart,
                                                             good_weapon,
                                                             Common.createAddInitiatorAttackWithWeaponTrigger(bonus_damage_action, only_hit: true, check_weapon_range_type: true, 
                                                                                                              range_type: AttackTypeAttackBonus.WeaponRangeType.Melee)
                                                             );
                angelic_attacks = Helpers.CreateFeature(prefix + "AngelicAttacksFeature",
                                                               angelic_attacks_buff.Name,
                                                               angelic_attacks_buff.Description,
                                                               "300f0d5f0a9446078465bafc693a17ef",
                                                               angelic_attacks_buff.Icon,
                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, angelic_attacks_buff, angelic_attacks);
            }


            static void createCelestialResistances()
            {
                var damage_resistance = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("8cbf303d479cf0d42a8e36092c76fa7c");
                var resist_ca_buff  = Helpers.CreateBuff(prefix + "CelestialResistancesBuff",
                                                    "Celestial Resistances",
                                                    "At 4th level, you gain resistance 5 to acid and cold. At 12th level, these resistances increase to 10.",
                                                    "2f932f4a8b32446993708c3e60e3aa54",
                                                    damage_resistance.Icon,
                                                    null,
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Acid),
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Cold),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                    ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                    classes: getBloodragerArray(),
                                                    customProgression: new (int, int)[] {
                                                                (11, 5),
                                                                (20, 10)
                                                    })
                                                    );
                celestial_resistances = Helpers.CreateFeature(prefix + "CelestialResistancesFeature",
                                                                               resist_ca_buff.Name,
                                                                               resist_ca_buff.Description,
                                                                               "4f59478befb0468081599205fbbce42a",
                                                                               resist_ca_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, resist_ca_buff, celestial_resistances);
            }


            static void createConviction()
            {
                var spell_resistance = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889");
                var conviction_buff = Helpers.CreateBuff(prefix + "ConvictionBuff",
                                                    "Conviction",
                                                    "At 8th level, you take half damage from magical attacks with the evil descriptor. If such an attack allows a Reflex save for half damage, you take no damage on a successful saving throw.",
                                                    "accbfca27afe4dadbc1b02f603e32a1a",
                                                    spell_resistance.Icon,
                                                    null,
                                                    new Kingmaker.UnitLogic.FactLogic.AddNimbusDamageDivisor()
                                                    );
                conviction = Helpers.CreateFeature(prefix + "ConvictionFeature",
                                                                               conviction_buff.Name,
                                                                               conviction_buff.Description,
                                                                               "223df27c2390449594a4d0fb78e9019e",
                                                                               conviction_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, conviction_buff, celestial_resistances);
            }


            static void createWingsOfHeaven()
            {
                var angelic_wings_buff = library.Get<BlueprintBuff>("25699a90ed3299e438b6fd5548930809");
                wings_of_heaven = Helpers.CreateFeature(prefix + "WingsOfHeavenFeature",
                                                                               "Wings of Heaven",
                                                                               "At 12th level, you gain a pair of wings that grant a +3 dodge bonus to AC against melee attacks and an immunity to ground based effects, such as difficult terrain.",
                                                                               "b5d48e26779048499194ade26ff0a741",
                                                                               angelic_wings_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, angelic_wings_buff, wings_of_heaven);
            }


            static void createAngelicProtection()
            {
                var sacred_nimbus_buff = library.Get<BlueprintBuff>("57b1c6a69c53f4d4ea9baec7d0a3a93a");
                var angelic_protection_buff = library.CopyAndAdd<BlueprintBuff>("6ab366720f4b8ed4f83ada36994d0890", prefix + "AngelicAspectBuff", "095a348c65564461b11398e8814d2c12"); //angelic aspect greater aura
                angelic_protection_buff.SetName("Angelic Protection");
                angelic_protection_buff.SetDescription("At 16th level, you gain a +4 deflection bonus to AC and a +4 resistance bonus on saving throws against attacks made or effects created by evil creatures.");
                angelic_protection_buff.SetIcon(sacred_nimbus_buff.Icon);
                angelic_protection = Helpers.CreateFeature(prefix + "AngelicProtectionFeature",
                                                                               angelic_protection_buff.Name,
                                                                               angelic_protection_buff.Description,
                                                                               "4ae1b076c2984982a9b2474ff0b980e7",
                                                                               angelic_protection_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, angelic_protection_buff, angelic_protection);
            }


            static void createAscension()
            {
                ascension = library.CopyAndAdd<BlueprintFeature>("d85e0396d8f68e047b7b67a1290f8dbc", prefix + "AscensionFeature", "17f896a2f8164f26a6c04523aa7ce708"); //from sorceror bloodline
                ascension.SetDescription("At 20th level, you become infused with the power of the heavens. You gain immunity to acid, cold, and petrification. You also gain resistance 10 to electricity and fire, as well as a +4 racial bonus on saving throws against poison. You have these benefits constantly, even while not bloodraging.");
            }

        }


        class FeyBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature confusing_critical;
            static internal BlueprintFeature woodland_stride;
            static internal BlueprintFeature blurring_movement;
            static internal BlueprintFeature quickling_bloodrage;
            static internal BlueprintFeature fleeting_glance;
            static internal BlueprintFeature fury_of_the_fey;
            static string prefix = "BloodragerBloodlineFey";


            static internal void create()
            {
                createConfusingCritical();
                createWoodlandStride();
                createBlurringMovement();
                createQuicklingBloodrage();
                createFleetingGlance();
                createFuryOfTheFey();

                var entangle = library.Get<BlueprintAbility>("0fd00984a2c0e0a429cf1a911b4ec5ca");
                var hideous_laughter = library.Get<BlueprintAbility>("fd4d9fd7f87575d47aafe2a64a6e2d8d");
                var haste = library.Get<BlueprintAbility>("486eaff58293f6441a5c2759c4872f98");
                var confusion = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949");

                var combat_reflexes = library.Get<BlueprintFeature>("0f8939ae6f220984e8fb568abbdfba95");
                var lightning_reflexes = library.Get<BlueprintFeature>("15e7da6645a7f3d41bdad7c8c4b9de1e");
                var dodge = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
                var mobility = library.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb");
                var improved_initiative = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
                var intimidating_prowess = library.Get<BlueprintFeature>("d76497bfc48516e45a0831628f767a0f");
                var hammer_the_gap = library.Get<BlueprintFeature>("7b64641c76ff4a744a2bce7f91a20f9a");


                progression = createBloodragerBloodline("Fey",
                                                              "One of your ancestors was fey, or the fey realm somehow intermixed with your bloodline. It affects your bloodrage in tricky and surprising ways.\n"
                                                               + "Bonus Feats: Dodge, Mobility, Combat Reflexes, Lightning Reflexes, Improved Initiative, Intimidating Prowess, Hammer the Gap.\n"
                                                               + "Bonus Spells: Entangle (7th), hideous laughter (10th), haste (13th), confusion (16th).",
                                                              library.Get<BlueprintProgression>("e8445256abbdc45488c2d90373f7dae8").Icon, // sorcerer fey bloodline
                                                              new BlueprintAbility[] { entangle, hideous_laughter, haste, confusion },
                                                              new BlueprintFeature[] { dodge, mobility, combat_reflexes, lightning_reflexes, improved_initiative, intimidating_prowess, hammer_the_gap },
                                                              new BlueprintFeature[] { confusing_critical, woodland_stride, blurring_movement, quickling_bloodrage, fleeting_glance, fury_of_the_fey  },
                                                              "fe056d23f80c4c64b4ee3a8e6f2973b2",
                                                              new string[] { "78b0900bd71c454baeb5886494fb0ae0", "2dbe75eadf684985a252608ae6f86259", "bcf1a280d91e435087c1180e3188ea41", "75c37819a8f74d02a3da486c46bf4e0c" },
                                                              "30fb198d84af4b2498fa1b3f35668494"
                                                              );
            }


            static void createConfusingCritical()
            {
                var blade_barrier = library.Get<BlueprintAbility>("36c8971e91f1745418cc3ffdfac17b74");
                var confusion_buff = library.Get<BlueprintBuff>("886c7407dc629dc499b9f1465ff382df");

                var effect_action = Helpers.CreateActionList(Common.createContextSavedApplyBuff(confusion_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1))));
                var action = Helpers.CreateActionList(Common.createContextActionSavingThrow(SavingThrowType.Will, effect_action));
                var confusing_critical_buff = Helpers.CreateBuff(prefix + "ConfusingCriticalBuff",
                                                                "Confusing Critical",
                                                                "At 1st level, fey power courses through your attacks. Each time you confirm a critical hit, the target must succeed at a Will saving throw or be confused for 1 round. The DC of this save is equal to 10 + 1/2 your bloodrager level + your Constitution modifier. This is a mind-affecting compulsion effect.",
                                                                "7a54fe2bbf624e60b10121356218a3a5",
                                                                blade_barrier.Icon,
                                                                null,
                                                                Common.createAddInitiatorAttackWithWeaponTrigger(action, critical_hit: true),
                                                                Common.createContextCalculateAbilityParamsBasedOnClass(bloodrager_class, StatType.Constitution)
                                                                );
                confusing_critical = Helpers.CreateFeature(prefix + "ConfusingCriticalFeature",
                                                               confusing_critical_buff.Name,
                                                               confusing_critical_buff.Description,
                                                               "54d034a7d7de4da083464496ab4aba61",
                                                               confusing_critical_buff.Icon,
                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, confusing_critical_buff, confusing_critical);
            }


            static void createWoodlandStride()
            {
                woodland_stride = library.CopyAndAdd<BlueprintFeature>("11f4072ea766a5840a46e6660894527d",
                                                             prefix + "WooldlandStrideFeature",
                                                             "c017c81495ee40d68dacbe5b794ef2a4");
                woodland_stride.SetDescription("At 4th level, you can move through any sort difficult terrain at your normal speed and without taking damage or suffering any other impairment.");
            }


            static void createBlurringMovement()
            {
                var blur_buff = library.Get<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674");
                blurring_movement = Helpers.CreateFeature(prefix + "BlurringMovementFeature",
                                                                               "Blurring Movement",
                                                                               "At 8th level, you become a blur of motion when you bloodrage. You gain effect of blur spell while in bloodrage.",
                                                                               "c7027b9ddd534fc09025ae807e5af38a",
                                                                               blur_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, blur_buff, blurring_movement);
            }


            static void createQuicklingBloodrage()
            {
                var haste_buff0 = library.Get<BlueprintBuff>("8d20b0a6129bd814eb0146041879f38a");
                var haste_buff = library.Get<BlueprintBuff>("03464790f40c3c24aa684b57155f3280");
                haste_buff.SetName(haste_buff0.Name);
                haste_buff.SetDescription(haste_buff0.Description);
                quickling_bloodrage = Helpers.CreateFeature(prefix + "QuicklingBloodrageFeature",
                                                                               "Quickling Bloodrage",
                                                                               "At 12th level, while bloodraging you’re treated as if you are under the effects of haste.",
                                                                               "f08420c83d6443068038b9160406438a",
                                                                               haste_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, haste_buff, quickling_bloodrage);
            }


            static void createFleetingGlance()
            {
                var resource = library.Get<BlueprintAbilityResource>("cb4518f1e176db44cbab819bb29c9b49"); //from sorc bloodline
                var amount = Helpers.GetField(resource, "m_MaxAmount");
                BlueprintCharacterClass[] classes = (BlueprintCharacterClass[])Helpers.GetField(amount, "Class");
                classes = classes.AddToArray(bloodrager_class);
                Helpers.SetField(amount, "Class", classes);
                Helpers.SetField(resource, "m_MaxAmount", amount);

                var fleeting_glance_ability = library.CopyAndAdd<BlueprintActivatableAbility>("ba5ead16619d2c64697968224163280b", prefix + "FleetingGlanceActivatableAbility", "625503853eea46f78e049841c0d27d4c");
                fleeting_glance_ability.SetDescription("At 12th level, you can turn invisible for a number of rounds per day equal to your bloodrager level. This ability functions as greater invisibility. These rounds need not be consecutive. You can use this ability even when not bloodraging.\nGreater Invisibility: This spell functions like invisibility, except that it doesn't end if the subject attacks.\nInvisibility: The creature becomes invisible. If a check is required, an invisible creature has a +20 bonus on its Stealth checks. The spell ends if the subject attacks any creature. For purposes of this spell, an attack includes any spell targeting a foe or whose area or effect includes a foe. Exactly who is a foe depends on the invisible character's perceptions. Actions directed at unattended objects do not break the spell. Causing harm indirectly is not an attack. Thus, an invisible being can open doors, talk, eat, climb stairs, summon monsters and have them attack, cut the ropes holding a rope bridge while enemies are on the bridge, remotely trigger traps, open a portcullis to release attack dogs, and so forth. If the subject attacks directly, however, it immediately becomes visible along with all its gear. Spells such as bless that specifically affect allies but not foes are not attacks for this purpose, even when they include foes in their area.");
                fleeting_glance_ability.ReplaceComponent<ActivatableAbilityResourceLogic>(Helpers.CreateActivatableResourceLogic(resource, ResourceSpendType.NewRound));

                fleeting_glance = Helpers.CreateFeature(prefix + "FleetingGlanceFeature",
                                                        fleeting_glance_ability.Name,
                                                        fleeting_glance_ability.Description,
                                                        "09b86c3330854d4cb1f303d6e3fea473",
                                                        fleeting_glance_ability.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddAbilityResource(resource),
                                                        Helpers.CreateAddFact(fleeting_glance_ability)
                                                       );
            }


            static void createFuryOfTheFey()
            {
                var bane_weapon_buff = library.CopyAndAdd<BlueprintBuff>("be190d2dd5433dd41a4aa00e1abc9a5b", prefix + "FuryOfTheFeyBuff", "51e32fabc97549a6ad56bd5d414a7d65");
                bane_weapon_buff.SetName("Fury of the Fey");
                bane_weapon_buff.SetBuffFlags(BuffFlags.StayOnDeath);
                bane_weapon_buff.SetDescription("At 20th level, when entering a bloodrage you can choose one type of creature (and subtype for humanoids or outsiders) that can be affected by the bane weapon special ability. All of your melee attacks are considered to have bane against that type. This ability doesn’t stack with other forms of bane.");
                fury_of_the_fey = Helpers.CreateFeature(prefix + "FuryOfTheFeyFeature",
                                                                               bane_weapon_buff.Name,
                                                                               bane_weapon_buff.Description,
                                                                               "9234d31a673840f6954d1baad879bbb8",
                                                                               bane_weapon_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, bane_weapon_buff, fury_of_the_fey);
            }

        }


        class InfernalBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature hellfire_strike;
            static internal BlueprintFeature infernal_resistance;
            static internal BlueprintFeature diabolical_arrogance;
            static internal BlueprintFeature dark_wings;
            static internal BlueprintFeature hellfire_charge;
            static internal BlueprintFeature fiend_of_the_pit;
            static string prefix = "BloodragerBloodlineInfernal";


            static internal void create()
            {
                createHellfireStrike();
                createInfernalResistance();
                createDiabolicalArrogance();
                createDarkWings();
                createHellfireCharge();
                createFiendOfThePit();

                var protection_from_good = library.Get<BlueprintAbility>("2ac7637daeb2aa143a3bae860095b63e");
                var scorching_ray = library.Get<BlueprintAbility>("cdb106d53c65bbc4086183d54c3b97c7");
                var hold_person = library.Get<BlueprintAbility>("c7104f7526c4c524f91474614054547e");
                var shield_of_dawn = library.Get<BlueprintAbility>("62888999171921e4dafb46de83f4d67d");

                var combat_reflexes = library.Get<BlueprintFeature>("0f8939ae6f220984e8fb568abbdfba95");
                var deceitful = library.Get<BlueprintFeature>("231a37321e26551489503e4e1d99e681");
                var blindfight = library.Get<BlueprintFeature>("4e219f5894ad0ea4daa0699e28c37b1d");
                var improved_sunder = library.Get<BlueprintFeature>("9719015edcbf142409592e2cbaab7fe1");
                var improved_disarm = library.Get<BlueprintFeature>("25bc9c439ac44fd44ac3b1e58890916f");
                var intimidating_prowess = library.Get<BlueprintFeature>("d76497bfc48516e45a0831628f767a0f");
                var iron_will = library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334");


                progression = createBloodragerBloodline("Infernal",
                                                              "The Pit lives in your blood. Maybe one of your ancestors was seduced by the powers of Hell or made a deal with a devil. Either way, its corruption seethes within your lineage.\n"
                                                               + "Bonus Feats: Blind - Fight, Combat Reflexes, Deceitful, Improved Disarm, Improved Sunder, Intimidating Prowess, Iron Will.\n"
                                                               + "Bonus Spells: Protection from good (7th), scorching ray(10th), hold preson(13th), shield of dawn(16th).",
                                                              library.Get<BlueprintProgression>("e76a774cacfb092498177e6ca706064d").Icon, // sorcerer infernal bloodline
                                                              new BlueprintAbility[] { protection_from_good, scorching_ray, hold_person, shield_of_dawn },
                                                              new BlueprintFeature[] { combat_reflexes, deceitful, blindfight, improved_disarm, improved_sunder, intimidating_prowess, iron_will },
                                                              new BlueprintFeature[] { hellfire_strike, infernal_resistance, diabolical_arrogance, dark_wings, hellfire_charge, fiend_of_the_pit },
                                                              "346f332ed9b843638a228257a59743b7",
                                                              new string[] { "02339061142647e8835f6f91c1485a76", "9a2c466398d04102b43c1a78ead78937", "592cf48fec304b4f8a2a5005a0939895", "11eab4cb7d9a4081b6788bfe9cb97fc4" },
                                                              "2d083855bf614669a8dd82ac2ff93207"
                                                              );
            }

            static void createHellfireStrike()
            {
                var burning_arc = library.Get<BlueprintAbility>("eaac3d36e0336cb479209a6f65e25e7c");

                var bonus_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageBonus)));
                var bonus_damage_action = Helpers.CreateActionList(bonus_damage);

                var hellfire_strike_buff = Helpers.CreateBuff(prefix + "HellfireStrikeBuff",
                                                             "Hellfire Strike",
                                                             "At 1st level, you deal additional 1d6 points of fire damage on critical hit. At 12thl level this damage increases to 2d6.",
                                                             "a97d9794ce2549709997a61f89ffcacb",
                                                             burning_arc.Icon,
                                                             null,
                                                             Common.createAddInitiatorAttackWithWeaponTrigger(bonus_damage_action, critical_hit: true, check_weapon_range_type: true,
                                                                                                              range_type: AttackTypeAttackBonus.WeaponRangeType.Melee),
                                                             Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Custom,
                                                                                             type: AbilityRankType.DamageBonus, classes: getBloodragerArray(),
                                                                                             customProgression: new (int, int)[] {
                                                                                                            (11, 1),
                                                                                                            (20, 2)
                                                                                             })
                                                             );
                hellfire_strike = Helpers.CreateFeature(prefix + "HellfireStirkeFeature",
                                                               hellfire_strike_buff.Name,
                                                               hellfire_strike_buff.Description,
                                                               "1740de7601f94a109bee9c7061c4085e",
                                                               hellfire_strike_buff.Icon,
                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, hellfire_strike_buff, hellfire_strike);
            }


            static void createInfernalResistance()
            {
                var damage_resistance = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("8cbf303d479cf0d42a8e36092c76fa7c");
                var resist_buff = Helpers.CreateBuff(prefix + "InfernalResistanceBuff",
                                                    "Infernal Resistance",
                                                    "At 4th level, you gain fire resistance 5, as well as a + 2 bonus on saving throws against poison.At 8th level, your fire resistance increases to 10, and the bonus on saving throws against poison increases to + 4.",
                                                    "befdf5624cd7418891b0dc62712c1456",
                                                    damage_resistance.Icon,
                                                    null,
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Fire, AbilityRankType.StatBonus),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                    ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                                                    classes: getBloodragerArray(),
                                                                                    customProgression: new (int, int)[] {
                                                                                                (7, 5),
                                                                                                (20, 10)
                                                                                                }
                                                                                    ),
                                                    Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.DamageDice), ModifierDescriptor.UntypedStackable,
                                                                                                          SpellDescriptor.Poison
                                                                                                          ),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                    ContextRankProgression.Custom, AbilityRankType.DamageDice,
                                                                                    classes: getBloodragerArray(),
                                                                                    customProgression: new (int, int)[] {
                                                                                                (7, 5),
                                                                                                (20, 10)
                                                                                                }
                                                                                    )
                                                    );
                infernal_resistance = Helpers.CreateFeature(prefix + "InfernalResistanceFeature",
                                                                               resist_buff.Name,
                                                                               resist_buff.Description,
                                                                               "b40f0fc02adb43ad9ffd8341154b1980",
                                                                               resist_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, resist_buff, infernal_resistance);
            }


            static void createDiabolicalArrogance()
            {
                var mind_blank = library.Get<BlueprintAbility>("df2a0ba6b6dcecf429cbb80a56fee5cf");

                var buff = Helpers.CreateBuff(prefix + "DiabolicalArroganceBuff",
                                              "Diabolical Arrogance",
                                              "At 8th level, you gain a +4 bonus on saving throws against enchantment and fear effects.",
                                              "abcff44aae1e475b855a9792a2d91f40",
                                              mind_blank.Icon,
                                              null,
                                              Common.createSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Fear),
                                              Common.createSavingThrowBonusAgainstSchool(4, ModifierDescriptor.UntypedStackable, SpellSchool.Enchantment)
                                             );
                diabolical_arrogance = Helpers.CreateFeature(prefix + "DiabolicalArroganceFeature",
                                                                               buff.Name,
                                                                               buff.Description,
                                                                               "487473aa12bb4829885341e889b5cb97",
                                                                               buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, buff, diabolical_arrogance);
            }


            static void createDarkWings()
            {
                var diabolic_wings_buff = library.Get<BlueprintBuff>("4113178a8d5bf4841b8f15b1b39e004f");
                dark_wings = Helpers.CreateFeature(prefix + "DarkWingsFeature",
                                                                               "Dark Wings",
                                                                               "At 12th level, you gain a pair of wings that grant a +3 dodge bonus to AC against melee attacks and an immunity to ground based effects, such as difficult terrain.",
                                                                               "9c451731dd7f4f96adec8e9621610841",
                                                                               diabolic_wings_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, diabolic_wings_buff, dark_wings);
            }


            static void createHellfireCharge()
            {
                var charge_buff = library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066");
                var flaming_burst = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>("3f032a3cd54e57649a0cdad0434bf221");
                var flaming = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");
                var weapon_enchant_buff = Helpers.CreateBuff(prefix + "HellfireChargeBuff",
                                                             "Hellfire Charge",
                                                             "At 16th level, when you charge the attack you make at the end of the charge is considered to be performed with weapon having flaming burst property.",
                                                             "b0439659723f4a8da680965c78a8fbf5",
                                                             charge_buff.Icon,
                                                             null,
                                                             Common.createBuffEnchantWornItem(flaming),
                                                             Common.createBuffEnchantWornItem(flaming_burst)
                                                             );
                hellfire_charge = Helpers.CreateFeature(prefix + "HellfireChargeFeature",
                                                                                weapon_enchant_buff.Name,
                                                                                weapon_enchant_buff.Description,
                                                                                "d26ca0ac64874157aad34ef664b116a9",
                                                                                weapon_enchant_buff.Icon,
                                                                                FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(charge_buff, weapon_enchant_buff, hellfire_charge, bloodrage_buff);
            }


            static void createFiendOfThePit()
            {
                var power_of_the_pit = library.Get<BlueprintFeature>("b6afdc50876e08149b1f9fdcdb2a308c");//from sorcerer infernal bloodline
                fiend_of_the_pit = Helpers.CreateFeature(prefix + "FiendOfThePitFeature",
                                                         "Fiend of the Pit",
                                                         "At 20th level, you gain immunity to fire and poison. You also gain resistance 10 to acid and cold, and gain blindsight within 60 feet. You have these benefits constantly, even while not bloodraging.",
                                                         "15b9af6383e7473eb8aa3a384ee9a78c",
                                                         power_of_the_pit.Icon,
                                                         FeatureGroup.None,
                                                         Common.createEnergyDR(10, DamageEnergyType.Cold),
                                                         Common.createEnergyDR(10, DamageEnergyType.Acid),
                                                         Common.createBlindsight(60),
                                                         Common.createBuffDescriptorImmunity(SpellDescriptor.Poison),
                                                         Common.createAddEnergyDamageImmunity(DamageEnergyType.Fire)
                                                         );
            }
        }


        class UndeadBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature frightful_charger;
            static internal BlueprintFeature ghost_strike;
            static internal BlueprintFeature deaths_gift;
            static internal BlueprintFeature frightening_strikes;
            static internal BlueprintFeature incorporeal_bloodrager;
            static internal BlueprintFeature one_foot_in_the_grave;
            static string prefix = "BloodragerBloodlineUndead";


            static internal void create()
            {
                createFrightfulCharger();
                createGhostStrike();
                createDeathsGift();
                createFrighteningStrikes();
                createIncorporealBloodrager();
                createOneFootInTheGrave();

                var ray_of_enfeeblement = library.Get<BlueprintAbility>("450af0402422b0b4980d9c2175869612");
                var false_life = library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762");
                var vampiric_touch = library.Get<BlueprintAbility>("6cbb040023868574b992677885390f92");
                var enervation = library.Get<BlueprintAbility>("f34fb78eaaec141469079af124bcfa0f");

                var endurance = library.Get<BlueprintFeature>("54ee847996c25cd4ba8773d7b8555174");
                var diehard = library.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad");
                var dodge = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
                var toughness = library.Get<BlueprintFeature>("d09b20029e9abfe4480b356c92095623");
                var mobility = library.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb");
                var intimidating_prowess = library.Get<BlueprintFeature>("d76497bfc48516e45a0831628f767a0f");
                var iron_will = library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334");


                progression = createBloodragerBloodline("Undead",
                                                              "The foul corruption of undeath is a part of you. Somewhere in the past, death became infused with your lineage. Your connection to the attributes of the undead bestows frightening power when your bloodrage.\n"
                                                               + "Bonus Feats: Diehard, Dodge, Endurance, Intimidating Prowess, Iron Will, Mobility, Toughness.\n"
                                                               + "Bonus Spells: Ray of enfeeblement (7th), false life (10th), vampiric touch (13th), enervation (16th).",
                                                              library.Get<BlueprintProgression>("a1a8bf61cadaa4143b2d4966f2d1142e").Icon, // sorcerer undead bloodline
                                                              new BlueprintAbility[] { ray_of_enfeeblement, false_life, vampiric_touch, enervation },
                                                              new BlueprintFeature[] { endurance, diehard, dodge, toughness, mobility, intimidating_prowess, iron_will },
                                                              new BlueprintFeature[] { frightful_charger, ghost_strike, deaths_gift, frightening_strikes, incorporeal_bloodrager, one_foot_in_the_grave },
                                                              "c9474ae9021543ff84633825b16b25f2",
                                                              new string[] { "290830ff3e8a470aaf1ebfd696dd1be5", "65d95f832c5649a9951d6d384085dcf2", "ca173aea1a5c41baa455f0146135ba18", "d83b97e1facf41ca9ef281a753536910" },
                                                              "e82330114e464cf193d521a47a273f5a"
                                                              );
            }


            static void createFrightfulCharger()
            {
                var shaken_buff = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
                var charge_buff = library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066");

                var action = Common.createContextActionApplyBuff(shaken_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.DamageBonus), DurationRate.Rounds));
                var effect_buff = Helpers.CreateBuff(prefix + "FrightfulChargerBuff",
                                                             "Frightful Charger",
                                                             "At 1st level, when you hit a creature with a charge attack, that creature becomes shaken for a number of rounds equal to 1/2 your bloodrager level (minimum 1). This effect does not cause an existing shaken or frightened condition (from this ability or another source) to turn into frightened or panicked. This is a mind-affecting fear effect.",
                                                             "61aff33f69d84391b49782fb976cf870",
                                                             charge_buff.Icon,
                                                             null,
                                                             Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(action)),
                                                             Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getBloodragerArray(),
                                                                                             type: AbilityRankType.DamageBonus, min: 1,  progression:ContextRankProgression.Div2)
                                                             );
                frightful_charger = Helpers.CreateFeature(prefix + "FrightfulChargerFeature",
                                                                                effect_buff.Name,
                                                                                effect_buff.Description,
                                                                                "2ba88b87439e456cb382392ba07ffa96",
                                                                                effect_buff.Icon,
                                                                                FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(charge_buff, effect_buff, frightful_charger, bloodrage_buff);
            }


            static void createGhostStrike()
            {
                var ghost_touch_magus = library.Get<BlueprintBuff>("9069fd94f3f5b8b448b28ce0a5ee8fd6");
                var ghost_touch = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f");
                var weapon_enchant_buff = Helpers.CreateBuff(prefix + "GhostStrikeBuff",
                                                             "Ghost Strike",
                                                             "At 4th level, your melee attacks are treated as if they have the ghost touch weapon special ability.",
                                                             "0f662d1b1432483190aa928ca7b9d355",
                                                             ghost_touch_magus.Icon,
                                                             null,
                                                             Common.createBuffEnchantWornItem(ghost_touch)
                                                             );
                ghost_strike = Helpers.CreateFeature(prefix + "GhostStrikeFeature",
                                                                                weapon_enchant_buff.Name,
                                                                                weapon_enchant_buff.Description,
                                                                                "85c240197d8d4f1f86b4f55f7bf51397",
                                                                                weapon_enchant_buff.Icon,
                                                                                FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, weapon_enchant_buff, ghost_strike);
            }


            static void createDeathsGift()
            {
                var sorc_deaths_gift = library.Get<BlueprintFeature>("fd5f14d44e82f464196fdf0ea82347cc");

                var buff = Helpers.CreateBuff(prefix + "DeathsGiftBuff",
                                              "Death's Gift",
                                              "At 8th level, you gain cold resistance 10, as well as DR 10/magic.",
                                              "f13abf4af63841e48f99c3005b163f75",
                                              sorc_deaths_gift.Icon,
                                              null,
                                              Common.createEnergyDR(10, DamageEnergyType.Cold),
                                              Common.createMagicDR(10)
                                              );
                deaths_gift = Helpers.CreateFeature(prefix + "DeathsGiftFeature",
                                                                                buff.Name,
                                                                                buff.Description,
                                                                                "19cdee308a0b4492a2b780adb115ab00",
                                                                                buff.Icon,
                                                                                FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, buff, deaths_gift);
            }


            static void createFrighteningStrikes()
            {
                var reckless_stance = library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");

                var frightened = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf"); //frightened
                var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220"); //shaken

                var fear_action = Helpers.CreateConditional(Helpers.CreateConditionHasBuff(shaken),
                                                       Common.createContextActionApplyBuff(frightened, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds)),
                                                       Common.createContextActionApplyBuff(shaken, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds))
                                                       );
                var save_action = Helpers.CreateConditionalSaved(new Kingmaker.ElementsSystem.GameAction[0], new Kingmaker.ElementsSystem.GameAction[] { fear_action });
                var action = Helpers.CreateActionList(Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(save_action)));

                var frightening_strikes_buff = Helpers.CreateBuff(prefix + "FrighteningStrikesBuff",
                                                                  "Frightening Strikes",
                                                                  "At 12th level, as a swift action during bloodrage you can empower your melee attacks with fear. For 1 round, creatures you hit with your melee attacks that fail a Will saving throw become shaken. Creatures who are already shaken become frightened. The DC of this save is equal to 10 + 1/2 your bloodrager level + your Constitution modifier. This is a mind-affecting fear effect.",
                                                                  "1a11f46ae4414b499d990d84b435fe81",
                                                                  frightened.Icon,
                                                                  null,
                                                                  Common.createAddInitiatorAttackWithWeaponTrigger(action, check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Melee),
                                                                  Common.createContextCalculateAbilityParamsBasedOnClass(character_class: bloodrager_class, stat: StatType.Constitution)
                                                                  );

                frightening_strikes = Common.createSwitchActivatableAbilityBuff(prefix + "FrighteningStrikes",
                                                                                "62c4051ff92f49d681a9d7fad040aca9", "4890b60e6cef4e4684b1c60e0a5c299a", "d32c7ed12d7e4cb09f5b4d9709dc7706",
                                                                                frightening_strikes_buff, bloodrage_buff, reckless_stance.ActivateWithUnitAnimation,
                                                                                command_type: CommandType.Swift,
                                                                                unit_command: CommandType.Swift
                                                                                );

            }


            static void createIncorporealBloodrager()
            {
                var ability = library.Get<BlueprintAbility>("853b5266404060f4f8afd9cf7859ef1f");
                var context_rank_config = ability.GetComponent<ContextRankConfig>();
                BlueprintCharacterClass[] classes = (BlueprintCharacterClass[])Helpers.GetField(context_rank_config, "m_Class");
                classes = classes.AddToArray(bloodrager_class);
                Helpers.SetField(context_rank_config, "m_Class", classes);
                ability.SetDescription("You can become incorporeal for 1 round per sorcerer level. While in this form, you gain the incorporeal subtype. You take no damage from non-magic weapons. You also take only half damage from any source not dealing ghost, holy, divine, or force damage. You can use this ability once per day.");

                incorporeal_bloodrager = library.CopyAndAdd<BlueprintFeature>("eafdc6762cbfa7d4d8220c6d6372973d", prefix + "IncorporealBloodragerFeature", "71742ba4b5434da6b5dfb9a0ccf79b19");
                incorporeal_bloodrager.SetName("Incorporeal Bloodrager");
                incorporeal_bloodrager.SetDescription("At 16th level, once per day you can choose to become incorporeal for 1 round per bloodrager level. You take only half damage from magic corporeal sources, and you take no damage from non-magic weapons and objects. You can use this ability even if not bloodraging.");
            }


            static void createOneFootInTheGrave()
            {
                one_foot_in_the_grave = library.CopyAndAdd<BlueprintFeature>("b3e403ebbdad8314386270fefc4b4cc8", prefix + "OneFootInTheGrave", "7ee1a466730f493f8957cbf65cbb9aa0");
                one_foot_in_the_grave.SetName("One foot in the Grave");
                one_foot_in_the_grave.SetDescription("At 20th level, you gain immunity to cold, paralysis, and sleep. The DR from your damage reduction ability increases to 8. You gain a +4 morale bonus on saving throws made against spells and spell-like abilities cast by undead. You have these benefits constantly, even while not bloodraging.");
                one_foot_in_the_grave.RemoveComponent(one_foot_in_the_grave.GetComponent<AddDamageResistancePhysical>());

                //add rank to dr
                var rank_config = damage_reduction.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>();
                var feature_list = Helpers.GetField<BlueprintFeature[]>(rank_config, "m_FeatureList");
                feature_list = feature_list.AddToArray(one_foot_in_the_grave, one_foot_in_the_grave, one_foot_in_the_grave);
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
