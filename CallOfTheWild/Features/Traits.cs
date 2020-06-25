using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class Traits
    {
        static LibraryScriptableObject library => Main.library;
        //COMBAT TRAITS
        static public BlueprintFeature anatomist;
        static public BlueprintFeature armor_expert;
        static public BlueprintFeature berserker_of_the_society;
        static public BlueprintFeature blade_of_the_society;
        static public BlueprintFeature defender_of_the_society;
        static public BlueprintFeature deft_dodger;
        static public BlueprintFeature dirty_fighter;
        static public BlueprintFeature reactionary;
        static public BlueprintFeature resilent;
        static public BlueprintFeature slippery;


        //FAITH TRAITS
        static public BlueprintFeature birthmark;
        static public BlueprintFeature caretaker;
        static public BlueprintFeature devotee_of_the_green;
        static public BlueprintFeature ease_of_faith;
        static public BlueprintFeature exalted_of_the_society;
        static public BlueprintFeature indomitable_faith;
        static public BlueprintFeature sacred_conduit;
        static public BlueprintFeature scholar_of_the_greate_beyond;
        static public BlueprintFeature fates_favored;
        static public BlueprintFeature omen;

        //SOCIAL TRAITS
        //adopted
        static public BlueprintFeature bully;
        static public BlueprintFeature fast_talker;
        static public BlueprintFeature maestro_of_the_society;
        static public BlueprintFeature poverty_stricken;
        static public BlueprintFeature student_of_philosophy;
        static public BlueprintFeature bruising_intellect;
        static public BlueprintFeature clever_wordplay;
        static public BlueprintFeature child_of_streets;

        //MAGIC TRAITS
        static public BlueprintFeature classically_schooled;
        static public BlueprintFeature dangerously_curious;
        static public BlueprintFeature focused_mind;
        static public BlueprintParametrizedFeature gifted_adept;
        static public BlueprintFeature magical_knack;
        static public BlueprintParametrizedFeature magical_lineage;
        static public BlueprintFeature pragmatic_activator;
        static public BlueprintFeature transmuter_of_korada;
        static public BlueprintFeature strength_of_the_land;

        static void createMagicalTraits()
        {
            classically_schooled = Helpers.CreateFeature("ClassicallySchooledTrait",
                                         "Classically Schooled",
                                         "Your apprenticeship or early education was particularly focused on the direct application of magic."
                                         + "Benefits: You gain a +1 trait bonus on Knowledge Arcana checks, and Knowledge Arcana is always a class skill for you.",
                                         "",
                                         Helpers.GetIcon("cad1b9175e8c0e64583432a22134d33c"), // sf arcana
                                         FeatureGroup.Trait,
                                         Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, 1, ModifierDescriptor.Trait),
                                         Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillKnowledgeArcana)
                                         );

            dangerously_curious = Helpers.CreateFeature("DangerouslyCuriousTrait",
                                                         "Dangerously Curious",
                                                         "You have always been intrigued by magic, possibly because you were the child of a magician or priest. You often snuck into your parent’s laboratory or shrine to tinker with spell components and magic devices, and frequently caused quite a bit of damage and headaches for your parent as a result."
                                                         + "Benefits: You gain a +1 bonus on Use Magic Device checks, and Use Magic Device is always a class skill for you.",
                                                         "",
                                                         Helpers.GetIcon("f43ffc8e3f8ad8a43be2d44ad6e27914"), // sf umd
                                                         FeatureGroup.Trait,
                                                         Helpers.CreateAddStatBonus(StatType.SkillUseMagicDevice, 1, ModifierDescriptor.Trait),
                                                         Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillUseMagicDevice)
                                                         );

            focused_mind = Helpers.CreateFeature("FocusedMindTrait",
                                             "Focused Mind",
                                             "Your childhood was dominated either by lessons of some sort (whether musical, academic, or other) or by a horrible home life that encouraged your ability to block out distractions and focus on the immediate task at hand."
                                             + "Benefit: You gain a +2 trait bonus on concentration checks.",
                                             "",
                                             Helpers.GetIcon("06964d468fde1dc4aa71a92ea04d930d"), // sf umd
                                             FeatureGroup.Trait,
                                             Helpers.Create<ConcentrationBonus>(c => c.Value = 4)
                                             );

            gifted_adept = library.CopyAndAdd<BlueprintParametrizedFeature>("e69a85f633ae8ca4398abeb6fa11b1fe", "GiftedAdeptParametrizedTrait", "");
            gifted_adept.SetNameDescription("Gifted Adept",
                                            "Your interest in magic was inspired by witnessing a spell being cast in a particularly dramatic method, perhaps even one that affected you physically or spiritually. This early exposure to magic has made it easier for you to work similar magic on your own."
                                            + "Benefit: Pick one spell when you choose this trait—from this point on, whenever you cast that spell, its effects manifest at +1 caster level.");
            gifted_adept.ComponentsArray = new BlueprintComponent[] { Helpers.Create<SpellDuplicates.ClBonusParametrized>(c => c.bonus = 1),
                                                                      Helpers.Create<NewMechanics.ParametrizedFeatureSelection.MaxLearneableSpellLevelLimiter>(m => m.max_lvl = 3) };
            gifted_adept.ParameterType = (FeatureParameterType) NewMechanics.ParametrizedFeatureSelection.FeatureParameterTypeExtender.AllLearnableSpells;

            magical_lineage = library.CopyAndAdd<BlueprintParametrizedFeature>("e69a85f633ae8ca4398abeb6fa11b1fe", "MagicalLineageParametrizedTrait", "");
            magical_lineage.SetNameDescription("Magical Lineage",
                                            "One of your parents was a gifted spellcaster who not only used metamagic often, but also developed many magical items and perhaps even a new spell or two—and you have inherited a fragment of this greatness."
                                            + "Benefit: Pick one spell when you choose this trait. When you apply metamagic feats to this spell that add at least 1 level to the spell, treat its actual level as 1 lower for determining the spell’s final adjusted level.");
            magical_lineage.ComponentsArray = new BlueprintComponent[] { Helpers.Create<NewMechanics.MetamagicMechanics.ReduceMetamagicCostForSpellParametrized>(r => r.reduction = 1),
                                                                      Helpers.Create<NewMechanics.ParametrizedFeatureSelection.MaxLearneableSpellLevelLimiter>(m => m.max_lvl = 3) };
            magical_lineage.ParameterType = (FeatureParameterType)NewMechanics.ParametrizedFeatureSelection.FeatureParameterTypeExtender.AllLearnableSpells;


            transmuter_of_korada = Helpers.CreateFeature("TransmuterOfKoradaTrait",
                                                         "Transmuter of Korada",
                                                         "You learned the secrets of transmutation from a follower of the empyreal lord Korada.\n"
                                                         + "Whenever you cast a spell from the transmutation school, its effects manifest at +1 caster level. Additionally, once per day you can double the duration of one of the following spells: bear’s endurance, bull’s strength, cat’s grace, eagle’s splendor, fox’s cunning, or owl’s wisdom. A spell affected by this trait cannot be modified further by the Extend Spell metamagic feat or similar abilities.",
                                                         "",
                                                         Helpers.GetIcon("b6a604dab356ac34788abf4ad79449ec"), // transmutation
                                                         FeatureGroup.Trait,
                                                         Helpers.Create<IncreaseSpellSchoolCasterLevel>(i => { i.BonusLevel = 1; i.School = SpellSchool.Transmutation; })
                                                         );
            var transmuter_of_korada_resource = Helpers.CreateAbilityResource("TransmuterOfKoradaResource", "", "", "", null);
            transmuter_of_korada_resource.SetFixedResource(1);

            var transmuter_of_korada_buff = Helpers.CreateBuff("TransmuterOfKoradaBuff",
                                                               transmuter_of_korada.Name,
                                                               transmuter_of_korada.Description,
                                                               "",
                                                               transmuter_of_korada.Icon,
                                                               null,
                                                               Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellList>(m =>
                                                               {
                                                                   m.resource = transmuter_of_korada_resource;
                                                                   m.amount = 1;
                                                                   m.Metamagic = Metamagic.Extend;
                                                                   m.spell_list = new BlueprintAbility[]
                                                                   {
                                                                       library.Get<BlueprintAbility>("4c3d08935262b6544ae97599b3a9556d"), //bulls strength
                                                                       library.Get<BlueprintAbility>("de7a025d48ad5da4991e7d3c682cf69d"), //cats grace
                                                                       library.Get<BlueprintAbility>("a900628aea19aa74aad0ece0e65d091a"), //bears endurance
                                                                       library.Get<BlueprintAbility>("ae4d3ad6a8fda1542acf2e9bbc13d113"), //foxs cunning
                                                                       library.Get<BlueprintAbility>("f0455c9295b53904f9e02fc571dd2ce1"), //owls wisdom
                                                                       library.Get<BlueprintAbility>("446f7bf201dc1934f96ac0a26e324803"), //eagles spledor
                                                                   };
                                                               })
                                                               );
            var transmuter_of_korada_toggle = Helpers.CreateActivatableAbility("TransmuterOfKoradaActivatableAbility",
                                                                               transmuter_of_korada.Name,
                                                                               transmuter_of_korada.Description,
                                                                               "",
                                                                               transmuter_of_korada.Icon,
                                                                               transmuter_of_korada_buff,
                                                                               AbilityActivationType.Immediately,
                                                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                                               null,
                                                                               transmuter_of_korada_resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                                               );
            transmuter_of_korada_toggle.DeactivateImmediately = true;
            transmuter_of_korada.AddComponents(transmuter_of_korada_resource.CreateAddAbilityResource(),
                                              Helpers.CreateAddFact(transmuter_of_korada_toggle)
                                              );

            pragmatic_activator = Helpers.CreateFeature("PragmaticActivatorTrait",
                                             "Pragmatic Activator",
                                             "While some figure out how to use magical devices with stubborn resolve, your approach is more pragmatic."
                                             + "Benefit: You may use your Intelligence modifier when making Use Magic Device checks instead of your Charisma modifier.",
                                             "",
                                             Helpers.GetIcon("f43ffc8e3f8ad8a43be2d44ad6e27914"), // sf umd
                                             FeatureGroup.Trait,
                                             Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s => {
                                                  s.StatTypeToReplaceBastStatFor = StatType.SkillUseMagicDevice;
                                                  s.NewBaseStatType = StatType.Intelligence;
                                              }),
                                             Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Charisma),
                                             Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence)
                                             );
            strength_of_the_land = Helpers.CreateFeature("StrengthOfTheLandTrait",
                                                         "Strength of the Land",
                                                         "You are able to tap into the living energy of the world to shatter lesser magic. "
                                                         + "Benefit: You gain a +1 trait bonus on caster level checks while touching the ground or unworked stone. This includes dispel checks and checks to overcome spell resistance.",
                                                         "",
                                                         Helpers.GetIcon("ee7dc126939e4d9438357fbd5980d459"), // spell penetration
                                                         FeatureGroup.Trait,
                                                         Helpers.Create<SpellPenetrationBonus>(s => s.Value = 1),
                                                         Helpers.Create<DispelCasterLevelCheckBonus>(d => d.Value = 1),
                                                         Helpers.Create<PrerequisiteMechanics.PrerequisiteRace>(p => p.race = library.Get<BlueprintRace>("c4faf439f0e70bd40b5e36ee80d06be7"))//dwarf
                                                         );
        }


        //RELIGION TRAITS
        //1. deadeye bowman
        //2. shield trained
        //3. Wisdom in Flesh
        //4. Opportunistic
        //5. erastil's speaker
        //6. child of nature
        //7. purity of faith
        //8. underlying principles
        //9. secret knowledge
        //10. spirit guide
        //11. illuminator
        //12. defensive strategist


        //REGIONAL TRAITS
        //1. honeyed tongue
        //2. militia
        //3. freed slave
        //4. aspiring hellknight
        //5. sece revolutionary
        //6. glory of old
        //7. spiritual forester
        //8. viking blood
        //9. river rat
        //10. Quan martial artist
        //11. Superstitious
        //12. Sargavan guard
        //13. Hermean paragon
        //14. wayang spell hunter
        //15. Valashmai Veteran
        //16. Rice Runner
        //17. Precocious Spellcaster 
        //18. Sound of Mind
        //19. Xa Hoi Soldier

        //RACE TRAITS
        //1. auspicious
        //2. big ears
        //3. bred for war
        //4. brute
        //5. elven reflexes
        //6. fanatic
        //7. forlorn
        //8. freed slave
        //9. grounded
        //10. latent psion
        //11. outcast
        //12. ruthless
        //13. superstitious
        //14. warrior of old
        //15. dirty_fighter
        //16. finish the fight
        //17. mother's teeth
        //18. unbreakable hate
        //19. Varisian tattoo
        //20. shield bearer


        static void createSocialTraits()
        {
            bully = Helpers.CreateFeature("BullyTrait",
                                 "Bully",
                                 "You grew up in an environment where the meek were ignored and you often had to resort to threats or violence to be heard.\n"
                                 + "Benefits: You gain a +1 trait bonus on intimidate checks, and Persuation is always considered a class skill for you for purpose of intimidate checks.",
                                 "",
                                 Helpers.GetIcon("d76497bfc48516e45a0831628f767a0f"), // intimidating prowess
                                 FeatureGroup.Trait,
                                 Helpers.CreateAddStatBonus(StatType.CheckIntimidate, 1, ModifierDescriptor.Trait),
                                 Helpers.Create<NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = StatType.CheckIntimidate; })
                                 );


            fast_talker = Helpers.CreateFeature("FastTalkerTrait",
                                                 "Bully",
                                                 "You had a knack for getting yourself into trouble as a child, and as a result developed a silver tongue at an early age.\n"
                                                 + "Benefits: You gain a +1 trait bonus on bluff checks, and Persuation is always considered a class skill for you for purpose of bluff checks.",
                                                 "",
                                                 Helpers.GetIcon("231a37321e26551489503e4e1d99e681"), // deceitful
                                                 FeatureGroup.Trait,
                                                 Helpers.CreateAddStatBonus(StatType.CheckBluff, 1, ModifierDescriptor.Trait),
                                                 Helpers.Create<NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = StatType.CheckBluff; })
                                                 );

            var bard = library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var perfromance_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            maestro_of_the_society = Helpers.CreateFeature("MaestroOfTheSocietyTrait",
                                                           "Maestro of the Society",
                                                           "The skills of the greatest musicians are at your fingertips, thanks to the vast treasure trove of musical knowledge in the vaults you have access to.\nBenefit: You may use bardic performance 3 additional rounds per day.",
                                                           "",
                                                           Helpers.GetIcon("0d3651b2cb0d89448b112e23214e744e"),
                                                           FeatureGroup.None,
                                                           Helpers.Create<IncreaseResourceAmount>(i => { i.Resource = perfromance_resource; i.Value = 3; }),
                                                           Helpers.PrerequisiteClassLevel(bard, 1)
                                                           );

            poverty_stricken = Helpers.CreateFeature("PovertyStrickenTrait",
                                                     "Poverty Stricken",
                                                     "Your childhood was tough, and your parents always had to make every copper piece count. Hunger was your constant companion, and you often had to live off the land or sleep in the wild."
                                                     + "Benefits: You gain a +1 bonus on Lore Nature checks, and Lore Nature is always a class skill for you.",
                                                     "",
                                                     Helpers.GetIcon("6507d2da389ed55448e0e1e5b871c013"), // sf nature
                                                     FeatureGroup.Trait,
                                                     Helpers.CreateAddStatBonus(StatType.SkillLoreNature, 1, ModifierDescriptor.Trait),
                                                     Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillLoreNature)
                                                     );

            student_of_philosophy = Helpers.CreateFeature("StudentOfPhilosophyTrait",
                                                         "Student of Philosophy",
                                                         "You were trained in a now-defunct philosophical tradition—such as that of the now-destroyed magic universities or astrologers—and learned to use logic and reason to persuade others."
                                                         + "Benefit: You can use your Intelligence modifier in place of your Charisma modifier on Diplomacy checks.",
                                                         "",
                                                         Helpers.GetIcon("1621be43793c5bb43be55493e9c45924"), // sf diplomacy
                                                         FeatureGroup.Trait,
                                                         Helpers.Create<SkillMechanics.DependentAbilityScoreCheckStatReplacement>(s =>
                                                         {
                                                             s.stat = StatType.CheckDiplomacy;
                                                             s.old_stat = StatType.Charisma;
                                                             s.new_stat = StatType.Intelligence;
                                                         })
                                                         );

            bruising_intellect = Helpers.CreateFeature("BruisingIntellectTrait",
                                                         "Bruising Intellect",
                                                         "Your sharp intellect and rapier-like wit bruise egos."
                                                         + "Benefit: Persuation is considered a class skill for you for the purpose of Intimidate checks, and you may use your Intelligence modifier when making Intimidate checks instead of your Charisma modifier. ",
                                                         "",
                                                         Helpers.GetIcon("1621be43793c5bb43be55493e9c45924"), // sf diplomacy
                                                         FeatureGroup.Trait,
                                                         Helpers.Create<SkillMechanics.DependentAbilityScoreCheckStatReplacement>(s =>
                                                         {
                                                             s.stat = StatType.CheckIntimidate;
                                                             s.old_stat = StatType.Charisma;
                                                             s.new_stat = StatType.Intelligence;
                                                         }),
                                                         Helpers.Create<NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = StatType.CheckBluff; a.value = 3; })
                                                         );

            clever_wordplay = Helpers.CreateFeature("CleverWordpalyTrait",
                                             "Clever Wordpaly",
                                             "Your cunning and logic are more than a match for another’s confidence and poise."
                                             + "Benefit: You may use your Intelligence modifier when making Bluff checks instead of your Charisma modifier. ",
                                             "",
                                             Helpers.GetIcon("1621be43793c5bb43be55493e9c45924"), // sf diplomacy
                                             FeatureGroup.Trait,
                                             Helpers.Create<SkillMechanics.DependentAbilityScoreCheckStatReplacement>(s =>
                                             {
                                                 s.stat = StatType.CheckBluff;
                                                 s.old_stat = StatType.Charisma;
                                                 s.new_stat = StatType.Intelligence;
                                             })
                                             );


            child_of_streets = Helpers.CreateFeature("ChildOfStreetsTrait",
                                         "Child of Streets",
                                         "You grew up on the streets of a large city, and as a result you have developed a knack for picking pockets and hiding small objects on your person."
                                         + "Benefits: You gain a +1 trait bonus on Trickery checks, and Trckery is always a class skill for you.",
                                         "",
                                         Helpers.GetIcon("7feda1b98f0c169418aa9af78a85953b"), // sf nature
                                         FeatureGroup.Trait,
                                         Helpers.CreateAddStatBonus(StatType.SkillThievery, 1, ModifierDescriptor.Trait),
                                         Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillThievery)
                                         );
        }


        static void createFaithTraits()
        {
            birthmark = Helpers.CreateFeature("BirthmarkTrait", 
                                              "Birthmark",
                                              "You were born with a strange birthmark that looks very similar to the holy symbol of the god you chose to worship later in life.\nBenefits: This birthmark increases your devotion to your god. You gain a +2 trait bonus on all saving throws against charm and compulsion effects.",
                                              "",
                                              Helpers.GetIcon("2483a523984f44944a7cf157b21bf79c"), // Elven Immunities
                                              FeatureGroup.Trait,
                                              Helpers.Create<SavingThrowBonusAgainstSchool>(a =>
                                                {
                                                    a.School = SpellSchool.Enchantment;
                                                    a.Value = 2;
                                                    a.ModifierDescriptor = ModifierDescriptor.Trait;
                                                }));

            caretaker = Helpers.CreateFeature("CaretakerTrait",
                                             "Caretaker",
                                             "Your faith in the natural world or one of the gods of nature makes it easy for you to pick up on related concepts."
                                             + "Benefits: You gain a +1 trait bonus on Lore Nature checks, and Lore Nature is always a class skill for you.",
                                             "",
                                             Helpers.GetIcon("6507d2da389ed55448e0e1e5b871c013"), // lore nature
                                             FeatureGroup.Trait,
                                             Helpers.CreateAddStatBonus(StatType.SkillLoreNature, 1, ModifierDescriptor.Trait),
                                             Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillLoreNature)
                                             );


            devotee_of_the_green = Helpers.CreateFeature("DevoteeOfTheGreenTrait",
                                 "Devotee of the Green",
                                 "As the child of an herbalist or an assistant in a temple infirmary, you often had to assist in tending to the sick and wounded.\nBenefits: You gain a +1 trait bonus on Lore Religion checks, and Lore Religion is always a class skill for you.",
                                 "",
                                 Helpers.GetIcon("f6f95242abdfac346befd6f4f6222140"), // remove sickness
                                 FeatureGroup.Trait,
                                 Helpers.CreateAddStatBonus(StatType.SkillLoreReligion, 1, ModifierDescriptor.Trait),
                                 Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillLoreReligion)
                                 );


            ease_of_faith = Helpers.CreateFeature("EaseOfFaithTrait",
                                             "Ease of Faith",
                                             "Your mentor, the person who invested your faith in you from an early age, took steps to ensure you understood that what powers your divine magic is no different from that which powers the magic of other religions. This philosophy makes it easier for you to interact with others who may not share your views.\n"
                                             + "Benefits: You gain a +1 bonus on Diplomacy checks, and consider Persutaion skill as class skill for purpose of persuation checks.",
                                             "",
                                             Helpers.GetIcon("1621be43793c5bb43be55493e9c45924"), // skill focus diplomacy
                                             FeatureGroup.Trait,
                                             Helpers.CreateAddStatBonus(StatType.CheckDiplomacy, 1, ModifierDescriptor.Trait),
                                             Helpers.Create<NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = StatType.CheckDiplomacy; })
                                             );

            var channel_resource = library.Get<BlueprintAbilityResource>("5e2bba3e07c37be42909a12945c27de7");

            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            exalted_of_the_society = Helpers.CreateFeature("ExaltedOfTheSocietyTrait", 
                                                            "Exalted of the Society",
                                                            "The vaults of the great city contain many secrets of the divine powers of the gods, and you have studied your god extensively.\nBenefit: You may channel energy 1 additional time per day.",
                                                            "",
                                                            Helpers.GetIcon("cd9f19775bd9d3343a31a065e93f0c47"), // Extra Channel
                                                            FeatureGroup.Trait,
                                                            channel_resource.CreateIncreaseResourceAmount(1),
                                                            Helpers.PrerequisiteClassLevel(cleric, 1)
                                                            );

            indomitable_faith = Helpers.CreateFeature("IndomitableFaithTrait",
                                                      "Indomitable Faith",
                                                      "You were born in a region where your faith was not popular, but you still have never abandoned it. Your constant struggle to maintain your own faith has bolstered your drive.\nBenefit: You gain a +1 trait bonus on Will saves.",
                                                      "",
                                                      Helpers.GetIcon("175d1577bb6c9a04baf88eec99c66334"), // Iron Will
                                                      FeatureGroup.Trait,
                                                      Helpers.CreateAddStatBonus(StatType.SaveWill, 1, ModifierDescriptor.Trait)
                                                      );

            sacred_conduit = ChannelEnergyEngine.sacred_conduit;


            scholar_of_the_greate_beyond = Helpers.CreateFeature("ScholarOfTheGreatBeyondTrait",
                                                                 "Scholar of the Great Beyond",
                                                                 "Your greatest interests as a child did not lie with current events or the mundane—you have always felt out of place, as if you were born in the wrong era. You take to philosophical discussions of the Great Beyond and of historical events with ease."
                                                                 + "You gain a +1 trait bonus on Knowledge World checks, and Knowledge World is always a class skill for you.",
                                                                 "",
                                                                 Helpers.GetIcon("611e863120c0f9a4cab2d099f1eb20b4"), // sf world
                                                                 FeatureGroup.Trait,
                                                                 Helpers.CreateAddStatBonus(StatType.SkillKnowledgeWorld, 1, ModifierDescriptor.Trait),
                                                                 Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillKnowledgeWorld)
                                                                 );




            omen = Helpers.CreateFeature("OmenTrait",
                                 "Omen",
                                 "You are the harbinger of some future event. Whether this event bodes good or ill, you exude an ominous presence.\n"
                                 + "Benefits: You gain a +1 trait bonus on Intimidate checks, and Persuation is always a class skill for you for the purpose of intimidate checks. Once per day, you may attempt to demoralize an opponent as a swift action.",
                                 "",
                                 Helpers.GetIcon("d2aeac47450c76347aebbc02e4f463e0"), // fear
                                 FeatureGroup.Trait,
                                 Helpers.CreateAddStatBonus(StatType.CheckIntimidate, 1, ModifierDescriptor.Trait),
                                 Helpers.Create<NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = StatType.CheckIntimidate; })
                                 );

            var omen_demoralize = library.CopyAndAdd<BlueprintAbility>("7d2233c3b7a0b984ba058a83b736e6ac", "OmenDemoralizeAbility", "");
            omen_demoralize.SetNameDescriptionIcon("Omen (Swift Action Demoralize)", omen.Description, omen.Icon);
            omen_demoralize.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift;
            var omen_resource = Helpers.CreateAbilityResource("OmenTraitResource", "", "", "", null);
            omen_resource.SetFixedResource(1);
            omen_demoralize.AddComponent(omen_resource.CreateResourceLogic());
            omen.AddComponents(omen_resource.CreateAddAbilityResource(),
                               Helpers.CreateAddFact(omen_demoralize));

            fates_favored =  Helpers.CreateFeature("FatesFavoredTrait",
                                                  "Fate's Favored",
                                                  "Whenever you are under the effect of a luck bonus of any kind, that bonus increases by 1.",
                                                  "",
                                                  Helpers.GetIcon("9a7e3cd1323dfe347a6dcce357844769"), // blessing luck & resolve
                                                  FeatureGroup.Trait,
                                                  Helpers.Create<SpellManipulationMechanics.TargetDescriptorModifierBonus>(t => { t.descriptor = ModifierDescriptor.Luck; t.bonus = 1; })
                                                  );

        }


        static void createCombatTratits()
        {
            anatomist = Helpers.CreateFeature("AnatomistTrait",
                                              "Anatomist",
                                              "You have studied the workings of anatomy, either as a student at university or as an apprentice mortician or necromancer. You know where to aim your blows to strike vital organs.\nBenefit: You gain a +1 trait bonus on all rolls made to confirm critical hits.",
                                              "",
                                              Helpers.GetIcon("f4201c85a991369408740c6888362e20"), // Improved Critical
                                              FeatureGroup.Trait,
                                              Helpers.Create<CriticalConfirmationBonus>(a => { a.Bonus = 1; a.Value = 0; })
                                              );

            armor_expert = Helpers.CreateFeature("ArmorExpertTrait",
                                                 "Armor Expert",
                                                "You have worn armor as long as you can remember, either as part of your training to become a knight’s squire or simply because you were seeking to emulate a hero. Your childhood armor wasn’t the real thing as far as protection, but it did encumber you as much as real armor would have, and you’ve grown used to moving in such suits with relative grace.\nBenefit: When you wear armor of any sort, reduce that suit’s armor check penalty by 1, to a minimum check penalty of 0.",
                                                "",
                                                Helpers.GetIcon("3bc6e1d2b44b5bb4d92e6ba59577cf62"), // Armor Focus (light)
                                                FeatureGroup.Trait,
                                                Helpers.Create<ArmorCheckPenaltyIncrease>(a => { a.Bonus = 1; a.CheckCategory = true; a.Category = ArmorProficiencyGroup.Light; }),
                                                Helpers.Create<ArmorCheckPenaltyIncrease>(a => { a.Bonus = 1; a.CheckCategory = true; a.Category = ArmorProficiencyGroup.Medium; }),
                                                Helpers.Create<ArmorCheckPenaltyIncrease>(a => { a.Bonus = 1; a.CheckCategory = true; a.Category = ArmorProficiencyGroup.Heavy; })
                                                );

            var rage_resource = library.Get<BlueprintAbilityResource>("24353fcf8096ea54684a72bf58dedbc9");
            berserker_of_the_society = Helpers.CreateFeature("BerserkerOfTheSocietyTrait",
                                                             "Berserker of the Society",
                                                             "Your time spent as a society member has taught you new truths about the origins of the your rage ability.\nBenefit: You may use your rage ability for 3 additional rounds per day.",
                                                             "",
                                                             Helpers.GetIcon("1a54bbbafab728348a015cf9ffcf50a7"), // Extra Rage
                                                             FeatureGroup.Trait,
                                                             rage_resource.CreateIncreaseResourceAmount(3),
                                                             Helpers.PrerequisiteClassLevel(library.Get<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853"), 1));

            blade_of_the_society = Helpers.CreateFeature("BladeOfTheSocietyTrait",
                                                         "Blade of the Society",
                                                         "You have studied and learned the weak spots of many humanoids and monsters.\nBenefit: You gain a +1 trait bonus to damage rolls from sneak attacks.",
                                                         "",
                                                         Helpers.GetIcon("9f0187869dc23744292c0e5bb364464e"), // Accomplished Sneak Attacker
                                                         FeatureGroup.Trait,
                                                         Helpers.Create<NewMechanics.SneakAttackDamageBonus>(a => a.value = 1)
                                                         );

            defender_of_the_society = Helpers.CreateFeature("DefenderOfTheSocietyTrait",
                                                            "Defender of the Society",
                                                            "Your time spent fighting and studying the greatest warriors of the society has taught you new defensive skills while wearing armor.\nBenefit: You gain a +1 trait bonus to Armor Class when wearing medium or heavy armor.",
                                                            "",
                                                            Helpers.GetIcon("7dc004879037638489b64d5016997d12"), // Armor Focus Medium
                                                            FeatureGroup.Trait,
                                                            Helpers.Create<NewMechanics.ArmorCategoryAcBonus>(a => { a.category = ArmorProficiencyGroup.Medium; a.descriptor = ModifierDescriptor.Trait; a.value = 1; }),
                                                            Helpers.Create<NewMechanics.ArmorCategoryAcBonus>(a => { a.category = ArmorProficiencyGroup.Heavy; a.descriptor = ModifierDescriptor.Trait; a.value = 1; }),
                                                            Helpers.PrerequisiteClassLevel(library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd"), 1)
                                                            );

            deft_dodger = Helpers.CreateFeature("DeftDodgerTrait",
                                                "Deft Dodger",
                                                "Growing up in a rough neighborhood or a dangerous environment has honed your senses.\nBenefit: You gain a +1 trait bonus on Reflex saves.",
                                                "",
                                                Helpers.GetIcon("15e7da6645a7f3d41bdad7c8c4b9de1e"), // Lightning Reflexes
                                                FeatureGroup.Trait,
                                                Helpers.CreateAddStatBonus(StatType.SaveReflex, 1, ModifierDescriptor.Trait)
                                                );

            dirty_fighter = Helpers.CreateFeature("DirtyFighterTrait",
                                                 "Dirty Fighter",
                                                 "You wouldn’t have lived to make it out of childhood without the aid of a sibling, friend, or companion you could always count on to distract your enemies long enough for you to do a little bit more damage than normal. That companion may be another PC or an NPC (who may even be recently departed from your side).\n" +
                                                 "Benefit: When you hit a foe you are flanking, you deal 1 additional point of damage (this damage is added to your base damage, and is multiplied on a critical hit). This additional damage is a trait bonus.",
                                                 "",
                                                 Helpers.GetIcon("5662d1b793db90c4b9ba68037fd2a768"), // precise strike
                                                 FeatureGroup.Trait,
                                                 Helpers.Create<NewMechanics.DamageBonusAgainstFlankedTarget>(d => d.bonus = 1)
                                                 );

            reactionary = Helpers.CreateFeature("ReactionaryTrait",
                                                "Reactionary",
                                                "You were bullied often as a child, but never quite developed an offensive response. Instead, you became adept at anticipating sudden attacks and reacting to danger quickly.\nBenefit: You gain a +2 trait bonus on initiative checks.",
                                                "",
                                                Helpers.GetIcon("797f25d709f559546b29e7bcb181cc74"), // Improved Initiative
                                                FeatureGroup.Trait,
                                                Helpers.CreateAddStatBonus(StatType.Initiative, 2, ModifierDescriptor.Trait)
                                                );

            resilent = Helpers.CreateFeature("ResilientTrait",
                                             "Resilient",
                                             "Growing up in a poor neighborhood or in the unforgiving wilds often forced you to subsist on food and water from doubtful sources. You’ve built up your constitution as a result.\nBenefit: You gain a +1 trait bonus on Fortitude saves.",
                                             "",
                                             Helpers.GetIcon("79042cb55f030614ea29956177977c52"), // Great Fortitude
                                             FeatureGroup.Trait,
                                             Helpers.CreateAddStatBonus(StatType.SaveFortitude, 1, ModifierDescriptor.Trait));

            slippery = Helpers.CreateFeature("SlipperTrait",
                                             "Slippery",
                                             "You have escaped from so many dangerous situations in your life that you’ve gotten quite good at not getting caught.\nBenefit: You gain a +1 trait bonus on Stealth checks and Stealth is a class skill for you.",
                                             "",
                                             Helpers.GetIcon("97a6aa2b64dd21a4fac67658a91067d7"), // fast stealth
                                             FeatureGroup.Trait,
                                             Helpers.CreateAddStatBonus(StatType.SkillStealth, 1, ModifierDescriptor.Trait),
                                             Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillStealth)
                                             );      
        }



    }
}
