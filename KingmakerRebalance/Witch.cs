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
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace KingmakerRebalance
{
    class Witch
    {
        static internal LibraryScriptableObject library => Main.library;
        static internal BlueprintCharacterClass witch_class;
        static internal BlueprintProgression witch_progression;
        static internal BlueprintFeatureSelection witch_patrons;
        static internal BlueprintFeatureSelection hex_selection;
        //hexes
        static internal BlueprintFeature healing_hex;
        static internal BlueprintFeature beast_of_ill_omen;
        static internal BlueprintFeature slumber_hex;
        static internal BlueprintFeature misfortune_hex;
        static internal BlueprintFeature fortune_hex;
        static internal BlueprintFeature iceplant_hex;
        static internal BlueprintFeature murksight_hex;





        internal static void createWitchClass()
        {
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

            witch_class = Helpers.Create<BlueprintCharacterClass>();
            witch_class.name = "WitcherClass";
            library.AddAsset(witch_class, "0df441ec2aa2407384ddd14e54a50d22");

            witch_class.LocalizedName = Helpers.CreateString("Witch.Name", "Witch");
            witch_class.LocalizedDescription = Helpers.CreateString("Witch.Description",
                "Some gain power through study, some through devotion, others through blood, but the witch gains power from her communion with the unknown.Generally feared and misunderstood, the witch draws her magic from a pact made with an otherworldly power.Communing with that source, using her familiar as a conduit, the witch gains not only a host of spells, but a number of strange abilities known as hexes.As a witch grows in power, she might learn about the source of her magic, but some remain blissfully unaware.Some are even afraid of that source, fearful of what it might be or where its true purposes lie."                + "Role: Hunters can adapt their tactics to many kinds of opponents, and cherish their highly trained animal companions.As a team, the hunter and her companion can react to danger with incredible speed, making them excellent scouts, explorers, and saboteurs."
                + "Role: While many witches are recluses, living on the edge of civilization, some live within society, openly or in hiding.The blend of witches’ spells makes them adept at filling a number of different roles, from seer to healer, and their hexes grant them a number of abilities that are useful in a fight.Some witches travel about, seeking greater knowledge and better understanding of the mysterious powers that guide them."
                );
            witch_class.m_Icon = wizard_class.Icon;
            witch_class.SkillPoints = wizard_class.SkillPoints;
            witch_class.HitDie = DiceType.D6;
            witch_class.BaseAttackBonus = wizard_class.BaseAttackBonus;
            witch_class.FortitudeSave = wizard_class.FortitudeSave;
            witch_class.ReflexSave = wizard_class.ReflexSave;
            witch_class.WillSave = wizard_class.WillSave;
            witch_class.Spellbook = createWitchSpellbook();
            witch_class.ClassSkills = new StatType[] {StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion, StatType.SkillUseMagicDevice,
                                                      StatType.SkillPersuasion};
            witch_class.IsDivineCaster = false;
            witch_class.IsArcaneCaster = true;
            witch_class.StartingGold = wizard_class.StartingGold;
            witch_class.PrimaryColor = wizard_class.PrimaryColor;
            witch_class.SecondaryColor = wizard_class.SecondaryColor;
            witch_class.RecommendedAttributes = wizard_class.RecommendedAttributes;
            witch_class.NotRecommendedAttributes = wizard_class.NotRecommendedAttributes;
            witch_class.EquipmentEntities = wizard_class.EquipmentEntities;
            witch_class.MaleEquipmentEntities = wizard_class.MaleEquipmentEntities;
            witch_class.FemaleEquipmentEntities = wizard_class.FemaleEquipmentEntities;
            witch_class.ComponentsArray = wizard_class.ComponentsArray;
            witch_class.StartingItems = new Kingmaker.Blueprints.Items.BlueprintItem[] {library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("511c97c1ea111444aa186b1a58496664"), //crossbow
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("ada85dae8d12eda4bbe6747bb8b5883c"), // quarterstaff
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("cd635d5720937b044a354dba17abad8d"), //s. cure light wounds
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("cd635d5720937b044a354dba17abad8d"), //s. cure light wounds
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("e8308a74821762e49bc3211358e81016"), //s. mage armor
                                                                                        library.Get<Kingmaker.Blueprints.Items.BlueprintItem>("3c56e535129756e449af6c0e67fd937f") //s. burning hands
                                                                                       };
            createWitchProgression();
            witch_class.Progression = witch_progression;
            witch_class.Archetypes = new BlueprintArchetype[] { };
            Helpers.RegisterClass(witch_class);
        }


        static void createWitchProgression()
        {
            createWitchPatrons();

        }


        static void createWitchPatrons()
        {
            BlueprintFeature[] patrons = new BlueprintFeature[]
            {
                createWitchPatronFeature("Agility", "f7a4a115c138439c8e5f4ee8adacfca0","9b43dc766cae40b7891c035b8fa43522",
                                         "4f8181e7a7f1d904fbaea64220e83379", //expeditious retreat
                                         "de7a025d48ad5da4991e7d3c682cf69d", //cats grace
                                         "486eaff58293f6441a5c2759c4872f98", //haste
                                         "4c349361d720e844e846ad8c19959b1e", //freedom of movement
                                         "93d9d74dac46b9b458d4d2ea7f4b1911", //polymorph
                                         "1f6c94d56f178b84ead4c02f1b1e1c48", //cats grace mass
                                         "a9fc28e147dbb364ea4a3c1831e7e55f", //polymorph greater
                                         "9155dbc8268da1c49a7fc4834fa1a4b1", //cloak of chaos
                                         "08ccad78cac525040919d51963f9ac39" //fiery body
                                         ),
                createWitchPatronFeature("Ancestors", "7e72537869dc455781780147823fc6a5", "a5dd1fcfdff84b2396c21c7705faf3fc",
                                         "90e59f4a4ada87243b7b3535a06d0638", //bless
                                         "03a9630394d10164a9410882d31572f0", //aid
                                         "faabd2cc67efa4646ac58c7bb3e40fcc", //prayer
                                         "0413915f355a38146bc6ad40cdf27b3f", //death ward
                                         "1bc83efec9f8c4b42a46162d72cbf494", //burst of glory
                                         "e15e5e7045fda2244b98c8f010adfe31", //heroism, greater
                                         "3cf5b2bd093a36a468b4ece38ad4d5fa", //bestow grace of the champion
                                         "cbf3bafa8375340498b86a3313a11e2f", //euphoric tranquility
                                         "870af83be6572594d84d276d7fc583e0" //weird
                                        ),
                createWitchPatronFeature("Animal", "aee09b4c9ae843c09829ef220241affe", "1eb5c7c9254044b5bad7bdf0eca1764a",
                                         "403cf599412299a4f9d5d925c7b9fb33", //magic fang
                                         "4c3d08935262b6544ae97599b3a9556d", //bull strength
                                         "754c478a2aa9bb54d809e648c3f7ac0e", //dominate animal
                                         "c83db50513abdf74ca103651931fac4b", //summon nature ally 4
                                         "56923211d2ac95e43b8ac5031bab74d8", //animal growth
                                         "9b93040dad242eb43ac7de6bb6547030", //beast shape 3
                                         "940a545a665194b48b722c1f9dd78d53", //beast shape 4
                                         "ea78c04f0bd13d049a1cce5daf8d83e0", //natures ally 8
                                         "a7469ef84ba50ac4cbf3d145e3173f8e"  //natures ally 9
                                        ),
                createWitchPatronFeature("Automn", "c3c1f806206546628b7b9d44cc1141ad", "beaddc15271240d992b8149babb79fed",
                                         "450af0402422b0b4980d9c2175869612", //ray of enfeeblement
                                         "29ccc62632178d344ad0be0865fd3113", //create pit
                                         "1a36c8b9ed655c249a9f9e8d4731f001", //soothing mud
                                         "6b30813c3709fc44b92dc8fd8191f345", //slowing mud
                                         "6d1d48a939ce475409f06e1b376bc386", //vinetrap
                                         "dbf99b00cd35d0a4491c6cc9e771b487", //acid fog
                                         "8c29e953190cc67429dc9c701b16b7c2", //caustic eruption
                                         "08323922485f7e246acb3d2276515526", //horrid witling
                                         "b24583190f36a8442b212e45226c54fc", //wail of banshee
                                        ),
                createWitchPatronFeature("Devotion", "faa76cf5cacd447caa2c18965ca9c3cb", "89bd5b848cfc4d6eacb51cf8e018e8d4",
                                         "9d5d2d3ffdd73c648af3eb3e585b1113", //divine favor
                                         "042aaa117e89c4d4b8cb41478dd3fca3", //grace
                                         "2d4263d80f5136b4296d6eb43a221d7d", //magic vestment
                                         "a26c23a887a6f154491dc2cefdad2c35",  //crusader's edge
                                         "f9910c76efc34af41b6e43d5d8752f0f", //flameStrike
                                         "6a234c6dcde7ae94e94e9c36fd1163a7", //bulls strength mass
                                         "3cf5b2bd093a36a468b4ece38ad4d5fa", //bestow grace of the champion
                                         "808ab74c12df8784ab4eeaf6a107dbea", //holy aura
                                         "867524328b54f25488d371214eea0d90" //heal mass
                                        ),
                createWitchPatronFeature("Elements", "1c19b61a30e34682b40261d29acd6ac3", "4f51c6e364be432f8e285893b4611b22",
                                         "ab395d2335d3f384e99dddee8562978f", //shocking grasp
                                         "cdb106d53c65bbc4086183d54c3b97c7", //scorching ray
                                         "2d81362af43aeac4387a3d4fced489c3", //fireball
                                         "690c90a82bf2e58449c6b541cb8ea004", //elemental body 1
                                         "f9910c76efc34af41b6e43d5d8752f0f", //flamestrike
                                         "6303b404df12b0f4793fa0763b21dd2c", //elemental assesor
                                         "8eb769e3b583f594faabe1cfdb0bb696", //summon greater elemental
                                         "e3d0dfe1c8527934294f241e0ae96a8d", //firestorm
                                         "d8144161e352ca846a73cf90e85bf9ac" //tsunami
                                        ),
                createWitchPatronFeature("Endurance", "16e0d62dac7947aab226dcded5bc5177", "79301d011ec5490ea32dcd42a8daa366",
                                         "b065231094a21d14dbf1c3832f776871", //fire belly
                                         "a900628aea19aa74aad0ece0e65d091a", //bears endurance
                                         "d2f116cfe05fcdd4a94e80143b67046f", //protection from energy
                                         "76a629d019275b94184a1a8733cac45e", //protection from energy communal
                                         "0a5ddfbcfb3989543ac7c936fc256889", //spell resistance
                                         "f6bcea6db14f0814d99b54856e918b92", //bears endurance mass
                                         "fafd77c6bfa85c04ba31fdc1c962c914", //restoration greater
                                         "b1c7576bd06812b42bda3f09ab202f14", //angelic aspect greater
                                         "867524328b54f25488d371214eea0d90"  //heal mass
                                         ),
                createWitchPatronFeature("Healing", "84286f03a92c4d27bf13484dee22c990", "b6c6c3ee2e904c689d9cb0eca78ce7c1",
                                         "55a037e514c0ee14a8e3ed14b47061de", //remove fear
                                         "e84fc922ccf952943b5240293669b171", //restoration lesser
                                         "4093d5a0eb5cae94e909eb1e0e1a6b36", //remove disiease
                                         "f2115ac1148256b4ba20788f7e966830", //restoration
                                         "be2062d6d85f4634ea4f26e9e858c3b8", //cleanse
                                         "788d72e7713cf90418ee1f38449416dc", //inspiring recovery
                                         "fafd77c6bfa85c04ba31fdc1c962c914", //restoration greater
                                         "1f173a16120359e41a20fc75bb53d449", //cure critical wounds mass
                                         "867524328b54f25488d371214eea0d90" // heal mass
                                         ),
                createWitchPatronFeature("Light", "8676966b3e0f4595be33f9cd2efc061c", "c248bad0e267442890c292c2079bcd2b",
                                         "91da41b9793a4624797921f221db653c", //color spray
                                         "ce7dad2b25acf85429b6c9550787b2d9", //glitterdust
                                         "c927a8b0cd3f5174f8c0b67cdbfde539", //remove blindness
                                         "4b8265132f9c8174f87ce7fa6d0fe47b", //rainbow pattern
                                         "ebade19998e1f8542a1b55bd4da766b3", //fire snake
                                         "093ed1d67a539ad4c939d9d05cfe192c", //sirocco
                                         "1fca0ba2fdfe2994a8c8bc1f0f2fc5b1", //sunbeam
                                         "e96424f70ff884947b06f41a765b7658", //sunburst
                                         "08ccad78cac525040919d51963f9ac39" //fiery body
                                        ),
                createWitchPatronFeature("Mercy", "88ce8b8dc64b4343984fefd31054a913","b7b7f46389544a99b0b4846ab99ed042",
                                         "47808d23c67033d4bbab86a1070fd62f", //cure light wounds
                                         "446f7bf201dc1934f96ac0a26e324803", //eagles splendor
                                         "b48674cef2bff5e478a007cf57d8345b", //remove curse
                                         "f2115ac1148256b4ba20788f7e966830", //resoration
                                         "be2062d6d85f4634ea4f26e9e858c3b8", //cleanse
                                         "2caa607eadda4ab44934c5c9875e01bc", //eagles splendor mass
                                         "15a04c40f84545949abeedef7279751a", //joyfull rupture
                                         "cbf3bafa8375340498b86a3313a11e2f", //euphoric tranquility
                                         "867524328b54f25488d371214eea0d90" //mass heal
                                        ),
                createWitchPatronFeature("Mountain", "2147870b66c643978c6c6aeae6f6c6a6", "cdcc5439ee0a4266aeb41e29f0677bf9",
                                         "85067a04a97416949b5d1dbf986d93f3", //stone fist
                                         "5181c2ed0190fc34b8a1162783af5bf4", //stone call
                                         "0a2f7c6aa81bc6548ac7780d8b70bcbc", //battering blast
                                         "d1afa8bc28c99104da7d784115552de5", //spike stones
                                         "7c5d556b9a5883048bf030e20daebe31", //stoneskin communal
                                         "e243740dfdb17a246b116b334ed0b165", //stone to flesh
                                         "3ecd589cf1a55df42a3b66940ee93ea4", //summon earth elemental greater
                                         "65254c7a2cf18944287207e1de3e44e8", //summon earth elemental elder
                                         "01300baad090d634cb1a1b2defe068d6" //clashing rocks
                                        ),
                createWitchPatronFeature("Plague", "ad6dcbca48394d0890565eef09c0dc19", "7339a5c3c56d4403860790af1118959a",
                                         "fa3078b9976a5b24caf92e20ee9c0f54", //ray of sickening
                                         "dee3074b2fbfb064b80b973f9b56319e", //pernicious poison
                                         "48e2744846ed04b4580be1a3343a5d3d", //contagion
                                         "4b76d32feb089ad4499c3a1ce8e1ac27", //animate dead
                                         "548d339ba87ee56459c98e80167bdf10", //cloudkill
                                         "76a11b460be25e44ca85904d6806e5a3", //create undead
                                         "b974af13e45639a41a04843ce1c9aa12", //creeping doom
                                         "08323922485f7e246acb3d2276515526", //horrid witlin
                                         "37302f72b06ced1408bf5bb965766d46" //energy drain
                                        ),
                createWitchPatronFeature("Protection", "fbda94ae0c184d06a9f1bc8b56e68267", "da5be607b885402eae26f954c47b47e4",
                                         "183d5bb91dea3a1489a6db6c9cb64445", //shield of faith
                                         "21ffef7791ce73f468b6fca4d9371e8b", //resist energy
                                         "d2f116cfe05fcdd4a94e80143b67046f", //protection from energy
                                         "c66e86905f7606c4eaa5c774f0357b2b", //stoneskin
                                         "7c5d556b9a5883048bf030e20daebe31", //stoneskin communal
                                         "fafd77c6bfa85c04ba31fdc1c962c914", //restoration greater
                                         "42aa71adc7343714fa92e471baa98d42", //protection from spells
                                         "87a29febd010993419f2a4a9bee11cfc" //mindblank communal
                                         ),
                createWitchPatronFeature("Spring", "fa8155faef214069a896e65a9073458c", "e9f7f92bf7724ec788fcd6374bfd2e82",
                                         "f3c0b267dd17a2a45a40805e31fe3cd1", //feather step
                                         "6c7467f0344004d48848a43d8c078bf8", //sickening entanglement
                                         "d219494150ac1f24f9ce14a3d4f66d26", //feather step mass
                                         "a5e23522eda32dc45801e32c05dc9f96", //good hope
                                         "3fce8e988a51a2a4ea366324d6153001", //constricting coils
                                         "645558d63604747428d55f0dd3a4cb58", //chain lightning
                                         "26be70c4664d07446bdfe83504c1d757", //change staff
                                         "7cfbefe0931257344b2cb7ddc4cdff6f", //stormbolts
                                         "d8144161e352ca846a73cf90e85bf9ac" //tsunami
                                        ),
                createWitchPatronFeature("Summer", "abd76216790240a6b3a0ddda236d1f19", "31e196f567fb4426b0199350dcbb2ac4",
                                         "b065231094a21d14dbf1c3832f776871", //firebelly
                                         "cdb106d53c65bbc4086183d54c3b97c7", //scorching ray
                                         "bf0accce250381a44b857d4af6c8e10d", //searing light
                                         "f72f8f03bf0136c4180cd1d70eb773a5", //controlled fireball
                                         "f9910c76efc34af41b6e43d5d8752f0f", //flame strike
                                         "093ed1d67a539ad4c939d9d05cfe192c", //sirocco
                                         "1fca0ba2fdfe2994a8c8bc1f0f2fc5b1", //sunbeam
                                         "e96424f70ff884947b06f41a765b7658", //sunburst
                                         "08ccad78cac525040919d51963f9ac39" //fiery body
                                         ),
                createWitchPatronFeature("Transformation", "6af899b9496d42258619ceebe08b1e4a" , "e71d75fcdc24439ab6a7fddbfbd92d34",
                                         "14c90900b690cac429b229efdf416127", //longstrider
                                         "a900628aea19aa74aad0ece0e65d091a", //bears endurance
                                         "61a7ed778dd93f344a5dacdbad324cc9", //beast shape 1
                                         "5d4028eb28a106d4691ed1b92bbb1915", //beast shape 2
                                         "9b93040dad242eb43ac7de6bb6547030", //beast shape 3
                                         "f767399367df54645ac620ef7b2062bb", //form of dragon 1
                                         "666556ded3a32f34885e8c318c3a0ced", //form of dragon 2
                                         "1cdc4ad4c208246419b98a35539eafa6", //form of dragon 3
                                         "08ccad78cac525040919d51963f9ac39", //fiery body
                                         ),
                createWitchPatronFeature("Winter", "073328f3df07436eb27bcc731fca6bb1", "3d5f30725ae24f589747b1164fc44228",
                                         "9f10909f0be1f5141bf1c102041f93d9", //snowball
                                         "c83447189aabc72489164dfc246f3a36", //frigid touch
                                         "fcb028205a71ee64d98175ff39a0abf9", //ice storm
                                         "65e8d23aef5e7784dbeb27b1fca40931", //icy prison
                                         "e7c530f8137630f4d9d7ee1aa7b1edc0", //cone of cold
                                         "5ef85d426783a5347b420546f91a677b", //cold ice strike
                                         "3e4d3b9a5bd03734d9b053b9067c2f38", //waves of exhaustion
                                         "17696c144a0194c478cbe402b496cb23", //polar ray
                                         "ba48abb52b142164eba309fd09898856" // polar midnight
                                         )

            };
            var diety_selection = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            witch_patrons = Helpers.CreateFeatureSelection("WitchPatronSelection",
                                                           "Patron",
                                                           "At 1st level, when a witch gains her familiar, she must also select a patron. This patron is a vague and mysterious force, granting the witch power for reasons that she might not entirely understand. While these forces need not be named, they typically hold influence over one of the following forces.\n"
                                                           + "At 2nd level, and every two levels thereafter, a witch’s patron adds new spells to a witch’s list of spells known. These spells are also automatically added to the list of spells stored by the familiar.The spells gained depend upon the patron chosen.",
                                                           "30f2f38633a144028aebecdd03391470",
                                                           diety_selection.Icon,
                                                           FeatureGroup.None);
            witch_patrons.AllFeatures = patrons;
        }


        static BlueprintSpellbook createWitchSpellbook()
        {
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            var witch_spellbook = Helpers.Create<BlueprintSpellbook>();
            witch_spellbook.name = "WitchSpellbook";
            library.AddAsset(witch_spellbook, "be5817bb59c14526a99877f8a7f15d31");
            witch_spellbook.Name = witch_class.LocalizedName;
            witch_spellbook.SpellsPerDay = wizard_class.Spellbook.SpellsPerDay;
            witch_spellbook.SpellsKnown = wizard_class.Spellbook.SpellsKnown;
            witch_spellbook.Spontaneous = false;
            witch_spellbook.IsArcane = true;
            witch_spellbook.AllSpellsKnown = false;
            witch_spellbook.CanCopyScrolls = true;
            witch_spellbook.CastingAttribute = StatType.Intelligence;
            witch_spellbook.CharacterClass = witch_class;
            witch_spellbook.CasterLevelModifier = 0;
            witch_spellbook.CantripsType = CantripsType.Cantrips;
          
            witch_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            witch_spellbook.SpellList.name = "WizardSpellList";
            library.AddAsset(witch_spellbook.SpellList, "422490cf62744e16a3e131efd94cf290");
            witch_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < witch_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                witch_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);

            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "55f14bc84d7c85446b07a1b5dd6b2b4c", 0), //daze
                new Common.SpellId( "c3a8f31778c3980498d8f00c980be5f5", 0), //guidance
                new Common.SpellId( "95f206566c5261c42aa5b3e7e0d1e36c", 0), //mage light
                new Common.SpellId( "7bc8e27cba24f0e43ae64ed201ad5785", 0), //resistance
                new Common.SpellId( "5bf3315ce1ed4d94e8805706820ef64d", 0), //touch of fatigue

                new Common.SpellId( "4783c3709a74a794dbe7c8e7e0b1b038", 1), //burning hands
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( "1af9d5995090e5a4185a30decf0959ad", 1), //charm person
                new Common.SpellId( "47808d23c67033d4bbab86a1070fd62f", 1), //cure light wounds
                new Common.SpellId( "8e7cfa5f213a90549aadd18f8f6f4664", 1), //ear piercing scream
                new Common.SpellId( "c60969e7f264e6d4b84a1499fdcf9039", 1), //enlarge person
                new Common.SpellId( "88367310478c10b47903463c5d0152b0", 1), //hypnotism
                new Common.SpellId( "e5cb4c4459e437e49a4cd73fde6b9063", 1), //inflict light wounds
                new Common.SpellId( "9e1ad5d6f87d19e4d8883d63a6e35568", 1), //mage armor
                new Common.SpellId( "450af0402422b0b4980d9c2175869612", 1), //ray of enfeeblement
                new Common.SpellId( "fa3078b9976a5b24caf92e20ee9c0f54", 1), //ray of sickening
                new Common.SpellId( "4e0e9aba6447d514f88eff1464cc4763", 1), //reduce person
                new Common.SpellId( "f6f95242abdfac346befd6f4f6222140", 1), //remove sickness
                new Common.SpellId( "bb7ecad2d3d2c8247a38f44855c99061", 1), //sleep
                new Common.SpellId( "9f10909f0be1f5141bf1c102041f93d9", 1), //snowball
                new Common.SpellId( "8fd74eddd9b6c224693d9ab241f25e84", 1), //summon monster 1
                new Common.SpellId( "dd38f33c56ad00a4da386c1afaa49967", 1), //ubreakable heart

                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 2), //blindness
                new Common.SpellId( "b7731c2b4fa1c9844a092329177be4c3", 2), //boneshaker
                new Common.SpellId( "1c1ebf5370939a9418da93176cc44cd9", 2), //cure moderate wounds
                new Common.SpellId( "b48b4c5ffb4eab0469feba27fc86a023", 2), //delay poison
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( "2dbe271c979d9104c8e2e6b42e208e32", 2), //fester
                new Common.SpellId( "ce7dad2b25acf85429b6c9550787b2d9", 2), //glitterdust
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 2), //hold person
                new Common.SpellId( "14d749ecacca90a42b6bf1c3f580bb0c", 2), //inflict moderate wounds
                new Common.SpellId( "42a65895ba0cb3a42b6019039dd2bff1", 2), //molten orb
                new Common.SpellId( "bc153808ef4884a4594bc9bec2299b69", 2), //pox postules
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( "1724061e89c667045a6891179ee2e8e7", 2), //summon monster 2
                new Common.SpellId( "134cb6d492269aa4f8662700ef57449f", 2), //web

                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 3), //bestow curse
                new Common.SpellId( "7658b74f626c56a49939d9c20580885e", 3), //deep slumber
                new Common.SpellId( "04e820e1ce3a66f47a50ad5074d3ae40", 3), //delay posion communal
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "5ab0d42fb68c9e34abae4921822b9d63", 3), //heroism
                new Common.SpellId( "d2cff9243a7ee804cb6d5be47af30c73", 3), //lightning bolt
                new Common.SpellId( "97b991256e43bb140b263c326f690ce2", 3), //rage
                new Common.SpellId( "c927a8b0cd3f5174f8c0b67cdbfde539", 3), //remove blindness
                new Common.SpellId( "b48674cef2bff5e478a007cf57d8345b", 3), //remove curse
                new Common.SpellId( "4093d5a0eb5cae94e909eb1e0e1a6b36", 3), //remove disease
                new Common.SpellId( "68a9e6d7256f1354289a39003a46d826", 3), //stinking cloud
                new Common.SpellId( "5d61dde0020bbf54ba1521f7ca0229dc", 3), //summon monster 3
                new Common.SpellId( "6cbb040023868574b992677885390f92", 3), //vampyric touch

                new Common.SpellId( "e418c20c8ce362943a8025d82c865c1c", 4), //cape of vasps
                new Common.SpellId( "4baf4109145de4345861fe0f2209d903", 4), //crushing despair
                new Common.SpellId( "6e81a6679a0889a429dec9cedcf3729c", 4), //cure serious wounds
                new Common.SpellId( "0413915f355a38146bc6ad40cdf27b3f", 4), //death ward
                new Common.SpellId( "f34fb78eaaec141469079af124bcfa0f", 4), //enervation
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life, greater
                new Common.SpellId( "fcb028205a71ee64d98175ff39a0abf9", 4), //ice storm
                new Common.SpellId( "3cf05ef7606f06446ad357845cb4d430", 4), //inflict serious wounds
                new Common.SpellId( "6717dbaef00c0eb4897a1c908a75dfe5", 4), //phantasmal killer
                new Common.SpellId( "d797007a142a6c0409a74b064065a15e", 4), //poison
                new Common.SpellId( "7ed74a3ec8c458d4fb50b192fd7be6ef", 4), //summon monster 4
                new Common.SpellId( "1e481e03d9cf1564bae6b4f63aed2d1a", 4), //touch of slime
                new Common.SpellId( "16ce660837fb2544e96c3b7eaad73c63", 4), //volcanic storm

                new Common.SpellId( "3105d6e9febdc3f41a08d2b7dda1fe74", 5), //baleful polymorph
                new Common.SpellId( "bacba2ff48d498b46b86384053945e83", 5), //cave fangs
                new Common.SpellId( "548d339ba87ee56459c98e80167bdf10", 5), //cloudkill
                new Common.SpellId( "0d657aa811b310e4bbd8586e60156a2d", 5), //cure critical wounds
                new Common.SpellId( "d7cbd2004ce66a042aeab2e95a3c5c61", 5), //dominate person
                new Common.SpellId( "41e8a952da7a5c247b3ec1c2dbb73018", 5), //hold monster
                new Common.SpellId( "3cf05ef7606f06446ad357845cb4d430", 5), //inflict critical wounds
                new Common.SpellId( "630c8b85d9f07a64f917d79cb5905741", 5), //summon monster 5
                new Common.SpellId( "8878d0c46dfbd564e9d5756349d5e439", 5), //waves of fatigue

                new Common.SpellId( "d42c6d3f29e07b6409d670792d72bc82", 6), //banshee blast
                new Common.SpellId( "7f71a70d822af94458dc1a235507e972", 6), //cloak of dreams
                new Common.SpellId( "e7c530f8137630f4d9d7ee1aa7b1edc0", 6), //cone of cold
                new Common.SpellId( "5d3d689392e4ff740a761ef346815074", 6), //cure light wounds mass
                new Common.SpellId( "f0f761b808dc4b149b08eaf44b99f633", 6), //dispel magic greater
                new Common.SpellId( "3167d30dd3c622c46b0c0cb242061642", 6), //eyebyte
                new Common.SpellId( "52b8b14360a87104482b2735c7fc8606", 6), //fester mass
                new Common.SpellId( "e15e5e7045fda2244b98c8f010adfe31", 6), //heroism greater
                new Common.SpellId( "03944622fbe04824684ec29ff2cec6a7", 6), //inflict moderate wounds mass
                new Common.SpellId( "1f2e6019ece86d64baa5effa15e81ecc", 6), //phantasmal putrefecation
                new Common.SpellId( "a0fc99f0933d01643b2b8fe570caa4c5", 6), //raise dead
                new Common.SpellId( "a6e59e74cba46a44093babf6aec250fc", 6), //slay living
                new Common.SpellId( "e243740dfdb17a246b116b334ed0b165", 6), //stone to flash
                new Common.SpellId( "e740afbab0147944dab35d83faa0ae1c", 6), //summon monster 6
                new Common.SpellId( "27203d62eb3d4184c9aced94f22e1806", 6), //transformation
                new Common.SpellId( "b3da3fbee6a751d4197e446c7e852bcb", 6), //true seeing

                new Common.SpellId( "645558d63604747428d55f0dd3a4cb58", 7), //chain lightning
                new Common.SpellId( "571221cc141bc21449ae96b3944652aa", 7), //cure moderate wounds mass
                new Common.SpellId( "137af566f68fd9b428e2e12da43c1482", 7), //harm
                new Common.SpellId( "ff8f1534f66559c478448723e16b6624", 7), //heal
                new Common.SpellId( "03944622fbe04824684ec29ff2cec6a7", 7), //inflict moderate wounds mass
                new Common.SpellId( "2b044152b3620c841badb090e01ed9de", 7), //insanity
                new Common.SpellId( "da1b292d91ba37948893cdbe9ea89e28", 7), //legendary proportions
                new Common.SpellId( "261e1788bfc5ac1419eec68b1d485dbc", 7), //power word blind
                new Common.SpellId( "ab167fd8203c1314bac6568932f1752f", 7), //summon monster 7
                new Common.SpellId( "474ed0aa656cc38499cc9a073d113716", 7), //umbral strike
                new Common.SpellId( "1e2d1489781b10a45a3b70192bba9be3", 7), //waves of Ectasy
                new Common.SpellId( "3e4d3b9a5bd03734d9b053b9067c2f38", 7), //waves of exhaustion

                new Common.SpellId( "0cea35de4d553cc439ae80b3a8724397", 8), //cure serious wounds mass
                new Common.SpellId( "c3d2294a6740bc147870fff652f3ced5", 8), //death clutch
                new Common.SpellId( "e788b02f8d21014488067bdd3ba7b325", 8), //frightfull aspect
                new Common.SpellId( "08323922485f7e246acb3d2276515526", 8), //horrid wilting
                new Common.SpellId( "820170444d4d2a14abc480fcbdb49535", 8), //inflict serious wounds mass
                new Common.SpellId( "f958ef62eea5050418fb92dfa944c631", 8), //power word stun
                new Common.SpellId( "0e67fa8f011662c43934d486acc50253", 8), //prediction of failure
                new Common.SpellId( "80a1a388ee938aa4e90d427ce9a7a3e9", 8), //ressurection
                new Common.SpellId( "7cfbefe0931257344b2cb7ddc4cdff6f", 8), //stormbolts
                new Common.SpellId( "d3ac756a229830243a72e84f3ab050d0", 8), //summon monster 8
                new Common.SpellId( "df2a0ba6b6dcecf429cbb80a56fee5cf", 8), //mind blank

                new Common.SpellId( "1f173a16120359e41a20fc75bb53d449", 9), //cure critical wounds mass
                new Common.SpellId( "3c17035ec4717674cae2e841a190e757", 9), //dominate monster
                new Common.SpellId( "43740dab07286fe4aa00a6ee104ce7c1", 9), //heroic invocation
                new Common.SpellId( "0340fe43f35e7a448981b646c638c83d", 9), //elemental swarm
                new Common.SpellId( "5ee395a2423808c4baf342a4f8395b19", 9), //inflict critical wounds mass
                new Common.SpellId( "87a29febd010993419f2a4a9bee11cfc", 9), //mind blank communal
                new Common.SpellId( "ba48abb52b142164eba309fd09898856", 9), //polar midnight
                new Common.SpellId( "2f8a67c483dfa0f439b293e094ca9e3c", 9), //power word kill
                new Common.SpellId( "52b5df2a97df18242aec67610616ded0", 9), //summon monster 9
                new Common.SpellId( "b24583190f36a8442b212e45226c54fc", 9) //wail of banshee
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(witch_spellbook.SpellList, spell_id.level);
            }

            return witch_spellbook;
        }


        static BlueprintFeature createWitchPatronFeature(string name, string spell_list_guid, string feature_guid, params string[] spell_guids)
        {
            var extra_spell_list = new Common.ExtraSpellList(spell_guids);
            var learn_spell_list = extra_spell_list.createLearnSpellList("Witch" + name + "PatronSpellList", spell_list_guid, witch_class);
            string description = name + " patron grants witch the following spells: ";
            for (int i = 1; i < 9; i++)
            {
                description += learn_spell_list.SpellList.SpellsByLevel[i].Spells[0].Name + ", ";
            }
            description += ".";

            return Helpers.CreateFeature("Witch" + name + "PatronFeature",
                                  name + " Patron",
                                  description,
                                  feature_guid,
                                  null,
                                  FeatureGroup.None,
                                  learn_spell_list);
        }


        static void addWitchHexCooldownScaling(BlueprintAbility ability, BlueprintBuff hex_cooldown)
        {
            ability.Type = AbilityType.Supernatural;
            ability.ReplaceComponent<AbilityEffectRunAction>(ability.GetComponent<AbilityEffectRunAction>());
            var cooldown_action = new Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff();
            cooldown_action.Buff = hex_cooldown;
            cooldown_action.AsChild = true;
            var duration = Helpers.CreateContextValue(AbilityRankType.Default);
            duration.ValueType = ContextValueType.Simple;
            duration.Value = 1;
            cooldown_action.DurationValue = Helpers.CreateContextDuration(bonus: duration,
                                                                            rate: DurationRate.Days);
            var action = new Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction();
            action.addAction(cooldown_action);
            ability.AddComponent(action);
            var target_checker = new Kingmaker.UnitLogic.Abilities.Components.TargetCheckers.AbilityTargetHasFact();
            target_checker.CheckedFacts = new BlueprintUnitFact[] { hex_cooldown };
            target_checker.Inverted = true;
            ability.AddComponent(target_checker);
            var scaling = new Kingmaker.UnitLogic.Mechanics.Components.ContextCalculateAbilityParamsBasedOnClass();
            scaling.CharacterClass = witch_class;
            scaling.StatType = StatType.Intelligence;
            scaling.UseKineticistMainStat = false;
            ability.AddComponent(scaling);
            var spell_list_components = ability.GetComponents<Kingmaker.Blueprints.Classes.Spells.SpellListComponent>();
            foreach (var c in spell_list_components)
            {
                ability.RemoveComponent(c);
            }
            ability.Type = AbilityType.Supernatural;

        }


        static BlueprintBuff addWitchHexCooldownScaling(BlueprintAbility ability, string buff_guid)
        {
            var hex_cooldown = Helpers.CreateBuff(ability.name + "CooldownBuff",
                                                                     ability.Name,
                                                                     ability.Description,
                                                                     buff_guid,
                                                                     ability.Icon,
                                                                     null);
            hex_cooldown.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath);
            addWitchHexCooldownScaling(ability, hex_cooldown);
            return hex_cooldown;
        }

        static BlueprintCharacterClass[] getWitchArray()
        {
            return new BlueprintCharacterClass[] { witch_class };
        }


        static void createHealingHex()
        {
            var cure_light_wounds_hex_ability = library.CopyAndAdd<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f", "HealingHex1Ability", "a9f6f1aa9d46452aa5720c472b8926e2");
            cure_light_wounds_hex_ability.SetName("Healing Hex");
            cure_light_wounds_hex_ability.SetDescription("A witch can soothe the wounds of those she touches.\n"
                                                          + "Effect: This acts as a cure light wounds spell, using the witch’s caster level.Once a creature has benefited from the healing hex, it cannot benefit from it again for 24 hours.At 5th level, this hex acts like cure moderate wounds.");

            cure_light_wounds_hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, 
                                                                                                                                                       classes: new BlueprintCharacterClass[] { witch_class },
                                                                                                                                                       max: 5));
            var hex_cooldown = addWitchHexCooldownScaling(cure_light_wounds_hex_ability, "67e655e5a20640519d387e08298de728");


            var cure_mod_wounds_hex_ability = library.CopyAndAdd<BlueprintAbility>("1c1ebf5370939a9418da93176cc44cd9", "HealingHex2Ability", "8ea243ac42aa4959ba131cbd5ff0118b");
            cure_mod_wounds_hex_ability.SetName(cure_light_wounds_hex_ability.Name);
            cure_mod_wounds_hex_ability.SetDescription(cure_light_wounds_hex_ability.Description);


            cure_mod_wounds_hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                                                                       classes: getWitchArray(),
                                                                                                                                                       max: 10));
            addWitchHexCooldownScaling(cure_mod_wounds_hex_ability, hex_cooldown);

            var healing_hex1_feature = Helpers.CreateFeature("HealingHex1Feature", cure_light_wounds_hex_ability.Name, cure_light_wounds_hex_ability.Description,
                                                             "a9d436988d044916b7bf61a58725725b",
                                                             cure_light_wounds_hex_ability.Icon,
                                                             FeatureGroup.None,
                                                             Helpers.CreateAddFact(cure_light_wounds_hex_ability));

            var healing_hex2_feature = Helpers.CreateFeature("HealingHex2Feature", cure_light_wounds_hex_ability.Name, cure_light_wounds_hex_ability.Description,
                                                 "6fe1054367f149939edc7f576d157bfa",
                                                 cure_light_wounds_hex_ability.Icon,
                                                 FeatureGroup.None,
                                                 Helpers.CreateAddFact(cure_mod_wounds_hex_ability));

            healing_hex = Helpers.CreateFeature("HealingHexFeature",
                                                cure_light_wounds_hex_ability.Name,
                                                cure_light_wounds_hex_ability.Description,
                                                "abec18ed55414a52a6d09457b734a5ca",
                                                cure_light_wounds_hex_ability.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(healing_hex1_feature, 5, getWitchArray(), new BlueprintArchetype[0], true),
                                                Helpers.CreateAddFeatureOnClassLevel(healing_hex2_feature, 5, getWitchArray(), new BlueprintArchetype[0], false)
                                                );
        }


        static void createBeastOfIllOmen()
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("8bc64d869456b004b9db255cdd1ea734", "BeastOfIllOmenHexAbility", "c19d55421e6f436580423fffc78c11bd");
            hex_ability.SetName("Beast of Ill-Omen");
            hex_ability.SetDescription("The witch imbues her familiar with strange magic, putting a minor curse upon the next enemy to see it.\n"
                                        + "Effect: The enemy must make a Will save or be affected by bane(caster level equal to the witch’s level).The witch can use this hex on her familiar at a range of up to 60 feet.The affected enemy must be no more than 60 feet from the familiar to trigger the effect; seeing the familiar from a greater distance has no effect(though if the enemy and familiar approach to within 60 feet of each other, the hex takes effect). The bane affects the closest creature to the familiar(ties affect the creature with the highest initiative score)\n"
                                        + " Whether or not the target’s save is successful, the creature cannot be the target of the bane effect for 1 day(later uses of this hex ignore that creature when determining who is affected).");
            hex_ability.Range = AbilityRange.Medium;
            hex_ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            hex_ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint;
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityTargetsAround>());
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>());

            var hex_cooldown = addWitchHexCooldownScaling(hex_ability, "ef6b3d4ad22644628aacfd3eaa4783e9");
            beast_of_ill_omen = Helpers.CreateFeature("BeastOfIllOmenFeature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      "fb3278a3b552414faaecb4189818b32e",
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
        }


        static void createSlumber()
        {
            var sleep_spell = library.Get<BlueprintAbility>("bb7ecad2d3d2c8247a38f44855c99061");
            var sleep_buff = library.Get<BlueprintBuff>("c9937d7846aa9ae46991e9f298be644a");
            var hex_ability = Helpers.CreateAbility("SlumberHexAbility",
                                                    "Slumber",
                                                    "Effect: A witch can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "This hex can affect a creature of any HD.The creature will not wake due to noise or light, but others can rouse it with a standard action.This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.",
                                                    "31f0fa4235ad435e95ebc89d8549c2ce",
                                                    sleep_buff.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Standard,
                                                    AbilityRange.Close,
                                                    sleep_spell.LocalizedDuration,
                                                    sleep_spell.LocalizedSavingThrow);
            hex_ability.CanTargetPoint = false;
            hex_ability.CanTargetEnemies = true;
            hex_ability.CanTargetFriends = true;
            hex_ability.CanTargetSelf = false;
            hex_ability.SpellResistance = true;
            hex_ability.AvailableMetamagic = sleep_spell.AvailableMetamagic;
            hex_ability.MaterialComponent = sleep_spell.MaterialComponent;
            hex_ability.ResourceAssetIds = sleep_spell.ResourceAssetIds;
            hex_ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            hex_ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint;
            hex_ability.AddComponent(sleep_spell.GetComponent<Kingmaker.Blueprints.Classes.Spells.SpellComponent>());
            var target_checker = new Kingmaker.UnitLogic.Abilities.Components.TargetCheckers.AbilityTargetHasFact();
            target_checker.CheckedFacts = new BlueprintUnitFact[] { library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f"), //construct
                                                                    library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33") //undead
                                                                  };
            target_checker.Inverted = true;
            hex_ability.AddComponent(target_checker);
            hex_ability.AddComponent(sleep_spell.GetComponents<Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFx>().Where(a => a.Time == Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFxTime.OnApplyEffect).ElementAt(0));

            var action = new Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction();
            action.SavingThrowType = SavingThrowType.Will;
            action.addAction(Common.createContextSavedApplyBuff(sleep_buff, DurationRate.Rounds));

            hex_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getWitchArray()));          
            var hex_cooldown = addWitchHexCooldownScaling(hex_ability, "0ccdbefa7f304a5788c4369b0a988e21");
            slumber_hex = Helpers.CreateFeature("SlumberHexFeature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      "c086eeb69a4442df9c4bb8469a2c362d",
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
        }


        static void createMisfortune()
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("ca1a4cd28737ae544a0a7e5415c79d9b", "MisfortuneHexAbility", "08b6595f503f4d3c973424c217f7610e"); //touch of chaos as base
            hex_ability.SetName("Misfortune");
            hex_ability.SetDescription("Effect: The witch can cause a creature within 30 feet to suffer grave misfortune for 1 round. Anytime the creature makes an ability check, attack roll, saving throw, or skill check, it must roll twice and take the worse result. A Will save negates this hex. At 8th level and 16th level, the duration of this hex is extended by 1 round. This hex affects all rolls the target must make while it lasts. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");
            hex_ability.Range = AbilityRange.Close;
            hex_ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            hex_ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint;
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityDeliverTouch>());
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityResourceLogic>());
            var action = new Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction();
            action.SavingThrowType = SavingThrowType.Will;
            action.addAction(Common.createContextSavedApplyBuff(library.Get<BlueprintBuff>("96bbd279e0bed0f4fb208a1761f566b5"), DurationRate.Rounds));
            hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>(action);
            hex_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                     min: 1,
                                                                     startLevel: 1,
                                                                     stepLevel: 8,
                                                                     classes: getWitchArray()));
            var hex_cooldown = addWitchHexCooldownScaling(hex_ability, "3c8c06c506cd45d29e35e2e6507c659a");
            misfortune_hex = Helpers.CreateFeature("MisfortuneHexFeature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      "d7d51941f7684c4c92eb2232a5dd600f",
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
        }


        static void createFortuneHex()
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("9af0b584f6f754045a0a79293d100ab3", "FortuneHexAbility", "9af0b584f6f754045a0a79293d100ab3"); //bit of luck
            hex_ability.SetName("Fortune");
            hex_ability.SetDescription("he witch can grant a creature within 30 feet a bit of good luck for 1 round. The target can call upon this good luck once per round, allowing him to reroll any ability check, attack roll, saving throw, or skill check, taking the better result. He must decide to use this ability before the first roll is made. At 8th level and 16th level, the duration of this hex is extended by 1 round. Once a creature has benefited from the fortune hex, it cannot benefit from it again for 24 hours.");
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.Designers.Mechanics.Facts.ReplaceAbilitiesStat>());
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityResourceLogic>());
            var action = new Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction();
            var apply_buff = (Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff)hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>().Actions.Actions[0].CreateCopy();
            var bonus_value = Helpers.CreateContextValue(AbilityRankType.Default);
            bonus_value.Value = 1;
            bonus_value.ValueType = ContextValueType.Rank;
            apply_buff.DurationValue = Helpers.CreateContextDuration(bonus: bonus_value);
            action.Actions = Helpers.CreateActionList(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>().Actions.Actions[0].CreateCopy());
            action.Actions.Actions = new Kingmaker.ElementsSystem.GameAction[] { apply_buff };
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>());
            var hex_cooldown = addWitchHexCooldownScaling(hex_ability, "ffaf306fa3aa41e183dba5866bee9210");
            fortune_hex = Helpers.CreateFeature("FortuneHexFeature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      "b308ce2d429d4c1ea048fb7a69f65002",
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
        }


        static void createIceplantHex()
        {
            var frigid_touch = library.Get<BlueprintAbility>("c83447189aabc72489164dfc246f3a36");
            iceplant_hex = Helpers.CreateFeature("IceplantHexFeature",
                                                 "Iceplant",
                                                  "This hex grants the witch and her familiar a +2 natural armor bonus and the constant effects of endure elements.\n"
                                                  + "The effect leaves the witch’s skin thick and stiff to the touch.",
                                                  "9828e12570414e6eb4cf42e00f303eab",
                                                  frigid_touch.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.NaturalArmor)
                                                  );
        }

        static void createMurksightHex()
        {
            var dracon_disciple_blindsense = library.Get<BlueprintFeature>("bb19b581f5a47b44abfe00164f1fcade");
            murksight_hex = Helpers.CreateFeature("MurksightHexFeature",
                                  "Murksight",
                                  "The witch receives blindsense up to 15 feet.",
                                  "e860bd889e494cd583b59bc5df42e7ef",
                                   dracon_disciple_blindsense.Icon, 
                                   FeatureGroup.None,
                                   Helpers.Create<Blindsense>(b => b.Range = 15.Feet()));
        }
    }
}
