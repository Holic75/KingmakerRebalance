using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CallOfTheWild.NewMechanics;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
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
using Kingmaker.RuleSystem.Rules.Abilities;
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
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
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

namespace CallOfTheWild.Archetypes
{
    public class Evangelist
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeatureSelection channel_energy;

        static public BlueprintFeature spontaneous_casting;
        static public BlueprintFeature inspire_courage;
        static public BlueprintFeature inspire_competence;
        static public BlueprintFeature inspire_greatness;
        static public BlueprintFeature inspire_heroics;
        static public BlueprintFeature fascinate;
        static public BlueprintFeature weapon_proficiency;
        static public BlueprintFeature performance_resource;
        static public BlueprintFeature channel_positive_energy, channel_negative_energy;
        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "EvangelistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Evangelist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The evangelist is the voice of her religion in the world. Where others nurture the faith among believers, an evangelist proclaims the coming glory of her deific patron and issues the clarion call to all around to heed the truth, or obey the call to war and crusade against the enemies of the church.");
            });
            Helpers.SetField(archetype, "m_ParentClass", cleric_class);
            library.AddAsset(archetype, "");

            createChannelEnergy();
            createSpontaneousCasting();
            createPerformance();
            createWeponProficiency();

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, library.Get<BlueprintFeature>("8c971173613282844888dc20d572cfc9"), //cleric proficiencies
                                                                                library.Get<BlueprintFeature>("d332c1748445e8f4f9e92763123e31bd"), //channel energy
                                                                                library.Get<BlueprintFeature>("43281c3d7fe18cc4d91928395837cd1e")), //second domain
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, weapon_proficiency, spontaneous_casting, performance_resource, inspire_courage),
                                                       Helpers.LevelEntry(3, inspire_competence, channel_energy),
                                                       Helpers.LevelEntry(6, fascinate),
                                                       Helpers.LevelEntry(9, inspire_greatness),
                                                       Helpers.LevelEntry(15, inspire_heroics)};

            cleric_class.Progression.UIDeterminatorsGroup = cleric_class.Progression.UIDeterminatorsGroup.AddToArray(weapon_proficiency, spontaneous_casting);
            cleric_class.Progression.UIGroups = cleric_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(inspire_courage, inspire_competence, fascinate, inspire_greatness, inspire_heroics));
            cleric_class.Archetypes = cleric_class.Archetypes.AddToArray(archetype);


            archetype.ReplaceStartingEquipment = true;
            var starting_items = cleric_class.StartingItems.RemoveFromArray(library.Get<BlueprintItemShield>("a85d51d0fb905f940b951eec60388bac")); //from cleric
        }

        static void createWeponProficiency()
        {
            weapon_proficiency = Helpers.CreateFeature("EvangelistProficiency",
                                                       "Single-Minded",
                                                       "An evangelist focuses her skills and learning on proclamation rather than the fine details of the church’s deeper mysteries or martial training. Thus, she may select only one domain and does not gain medium armor proficiency or shield proficiency.",
                                                       "",
                                                       null,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFacts(library.Get<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132"), //light armor
                                                                              library.Get<BlueprintFeature>("e70ecf1ed95ca2f40b754f1adb22bbdd") //simple weapon
                                                                              )
                                                      );
        }         


        static void createSpontaneousCasting()
        {
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            BlueprintAbility[] spells = new BlueprintAbility[]
            {
                NewSpells.command,
                NewSpells.hypnotic_pattern,
                library.Get<BlueprintAbility>("faabd2cc67efa4646ac58c7bb3e40fcc"), //prayer
                library.Get<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61"), //domiante person
                NewSpells.command_greater,
                library.Get<BlueprintAbility>("d316d3d94d20c674db2c24d7de96f6a7"), //serenity
                library.Get<BlueprintAbility>("7f71a70d822af94458dc1a235507e972"), //cloak of dreams
                library.Get<BlueprintAbility>("cbf3bafa8375340498b86a3313a11e2f"), //euphoric tranquility
                library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757") //dominate monster
            };

            var description = "An evangelist does not gain the ability to spontaneously cast cure or inflict spells by sacrificing prepared spells.However, an evangelist can spontaneously cast the following splls, by sacrificing a prepared spell of the noted level: ";
            for (int i = 0; i < spells.Length; i++)
            {
                description += spells[i].Name + $" ({i+1}{Common.getNumExtension(i+1)})" +   ((i + 1) == spells.Length ? "." : ", ");
            }
            spontaneous_casting = Helpers.CreateFeature("EvangelistSpontaneousCasting",
                                                         "Spontaneous Casting",
                                                         description,
                                                         "",
                                                         library.Get<BlueprintAbility>("d316d3d94d20c674db2c24d7de96f6a7").Icon,
                                                         FeatureGroup.None
                                                         );

            var spells_array = Common.createSpelllistsForSpontaneousConversion(spells);

            for (int i = 0; i < spells_array.Length; i++)
            {
                spontaneous_casting.AddComponent(Common.createSpontaneousSpellConversion(cleric, spells_array[i].ToArray()));
            }
        }


        static void createPerformance()
        {
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");

            var amount = Helpers.GetField(resource, "m_MaxAmount");
            BlueprintCharacterClass[] classes = (BlueprintCharacterClass[])Helpers.GetField(amount, "Class");
            classes = classes.AddToArray(cleric);
            Helpers.SetField(amount, "Class", classes);
            Helpers.SetField(amount, "Archetypes", new BlueprintArchetype[] { archetype });
            Helpers.SetField(resource, "m_MaxAmount", amount);


            var archatype_list_feature = Helpers.CreateFeature("BardicPerformanceArchetypeExtensionFeature",
                                                               "",
                                                               "",
                                                               "",
                                                               null,
                                                               FeatureGroup.None);
            archatype_list_feature.AddComponent(Helpers.Create<ContextRankConfigArchetypeList>(c => c.archetypes = new BlueprintArchetype[] { archetype }));
            archatype_list_feature.HideInCharacterSheetAndLevelUp = true;

            var inspire_courage_ability = library.CopyAndAdd<BlueprintActivatableAbility>("70274c5aa9124424c984217b62dabee8", "EvangelistInspireCourageToggleAbility", "");
            inspire_courage_ability.SetDescription("A 1st level evangelist can use his performance to inspire courage in his allies (including himself), bolstering them against fear and improving their combat abilities. To be affected, an ally must be able to perceive the evangelist's performance. An affected ally receives a +1 morale bonus on saving throws against charm and fear effects and a +1 competence bonus on attack and weapon damage rolls. At 5th level, and every six evangelist levels thereafter, this bonus increases by +1, to a maximum of +4 at 17th level.");
            var inspire_courage_buff = library.Get<BlueprintBuff>("6d6d9e06b76f5204a8b7856c78607d5d");
            inspire_courage_buff.ReplaceComponent<ContextRankConfig>(c =>
                                                                    {
                                                                        Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueTypeExtender.MaxClassLevelWithArchetypes.ToContextRankBaseValueType());
                                                                        Helpers.SetField(c, "m_Feature", archatype_list_feature);
                                                                        Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(cleric));
                                                                    }
                                                                    );

            var inspire_competence_ability = library.CopyAndAdd<BlueprintActivatableAbility>("430ab3bb57f2cfc46b7b3a68afd4f74e", "EvangelistInspireCompetenceToggleAbility", "");
            inspire_competence_ability.SetDescription("An evangelist of 3rd level or higher can use his performance to help allies succeed at a task. They must be within 30 feet and able to see and hear the evangelist. They get a +2 competence bonus on all skill checks as long as they continue to hear the evangelist's performance. This bonus increases by +1 for every four levels the evangelist has attained beyond 3rd (+3 at 7th, +4 at 11th, +5 at 15th, and +6 at 19th).");
            var inspire_competence_buff = library.Get<BlueprintBuff>("1fa5f733fa1d77743bf54f5f3da5a6b1");
            inspire_competence_buff.ReplaceComponent<ContextRankConfig>(c =>
                                                                {
                                                                    Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueTypeExtender.MaxClassLevelWithArchetypes.ToContextRankBaseValueType());
                                                                    Helpers.SetField(c, "m_Feature", archatype_list_feature);
                                                                    Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(cleric));
                                                                }
                                                                );

            var inspire_greatness_ability = library.CopyAndAdd<BlueprintActivatableAbility>("be36959e44ac33641ba9e0204f3d227b", "EvangelistInspireGreatnessToggleAbility", "");
            inspire_greatness_ability.SetDescription("An evangelist of 9th level or higher can use his performance to inspire greatness in all allies within 30 feet, granting extra fighting capability. A creature inspired with greatness gains 2 bonus Hit Dice (d10s), the commensurate number of temporary hit points (apply the target's Constitution modifier, if any, to these bonus Hit Dice), a +2 competence bonus on attack rolls, and a +1 competence bonus on Fortitude saves.");

            var inspire_heroics_ability = library.CopyAndAdd<BlueprintActivatableAbility>("a4ce06371f09f504fa86fcf6d0e021e4", "EvangelistInspireHeroicsToggleAbility", "");
            inspire_heroics_ability.SetDescription("An evangelist of 15th level or higher can inspire tremendous heroism in all allies within 30 feet. Inspired creatures gain a +4 morale bonus on saving throws and a +4 dodge bonus to AC. The effect lasts for as long as the targets are able to witness the performance.");

            var fascinate_ability = library.CopyAndAdd<BlueprintActivatableAbility>("993908ad3fb81f34ba0ed168b7c61f58", "EvangelistFascinateToggleAbility", "");
            fascinate_ability.SetDescription("At 6th level, an evangelist can use his performance to cause one or more creatures to become fascinated with him. Each creature to be fascinated must be within 30 feet.\nEach creature within range receives a Will save (DC 10 + 1/2 the evangelist's level + the evangelist's Cha modifier) to negate the effect. If a creature's saving throw succeeds, the evangelist cannot attempt to fascinate that creature again for 24 hours. If its saving throw fails, the creature stands quietly and observes the performance for as long as the bard continues to maintain it. Any damage to the target automatically breaks the effect.");

            var fasciante_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("a4fc1c0798359974e99e1d790935501d", "EvangelistFascianteArea", "");
            fasciante_area.ReplaceComponent<ContextCalculateAbilityParamsBasedOnClass>(c => c.CharacterClass = cleric);
            var fascinate_buff = library.CopyAndAdd<BlueprintBuff>("555930f121b364a4e82670b433028728", "EvangelistFascianteBuff", "");
            fascinate_buff.SetDescription(fascinate_ability.Description);
            fascinate_buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = fasciante_area);
            fascinate_ability.Buff = fascinate_buff;


            inspire_courage = Common.ActivatableAbilityToFeature(inspire_courage_ability, false);
            inspire_competence = Common.ActivatableAbilityToFeature(inspire_competence_ability, false);
            inspire_heroics = Common.ActivatableAbilityToFeature(inspire_heroics_ability, false);
            inspire_greatness = Common.ActivatableAbilityToFeature(inspire_greatness_ability, false);
            fascinate = Common.ActivatableAbilityToFeature(fascinate_ability, false);

            performance_resource = library.CopyAndAdd<BlueprintFeature>("b92bfc201c6a79e49afd0b5cfbfc269f", "EvangelistPerformanceResource", "");
            performance_resource.ReplaceComponent<IncreaseResourcesByClass>(i => i.CharacterClass = cleric);
        }


        static void createChannelEnergy()
        {
            var resource = library.CopyAndAdd<BlueprintAbilityResource>("5e2bba3e07c37be42909a12945c27de7", "EvangelistChannelEnergyResource", "");
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");

            var positive_energy_feature = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var negative_energy_feature = library.Get<BlueprintFeature>("3adb2c906e031ee41a01bfc1d5fb7eea");
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Custom,
                                                                                  type: AbilityRankType.Default, classes: new BlueprintCharacterClass[] { cleric },
                                                                                  customProgression: new (int, int)[] {(4,1),
                                                                                                                       (6,2),
                                                                                                                       (10, 3),
                                                                                                                       (12, 4),
                                                                                                                       (16, 5),
                                                                                                                       (18, 6),
                                                                                                                       (20, 7)}
                                                                                  );
            var dc_scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { cleric }, StatType.Charisma);
            channel_positive_energy = Helpers.CreateFeature("EvangelistChannelPositiveEnergyFeature",
                                                                "Channel Positive Energy",
                                                                "An evangelist (or a neutral evangelist who worships a good deity) channels positive energy and can choose to deal damage to undead creatures or to heal living creatures.\n"
                                                                + $"Channeling energy causes a burst that either heals all living creatures or damages all undead creatures in a 30-foot radius centered on the evangelist. The amount of damage dealt or healed is equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)} at level 3 and increases by 1d{BalanceFixes.getDamageDieString(DiceType.D6)} every 2 levels except at levels 9 and 15. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the evangelist's level + the evangelist's Charisma modifier. Creatures healed by channel energy cannot exceed their maximum hit point total—all excess healing is lost. This is a standard action that does not provoke an attack of opportunity.",
                                                                "",
                                                                positive_energy_feature.Icon,
                                                                FeatureGroup.None,
                                                                positive_energy_feature.GetComponent<PrerequisiteFeature>(),
                                                                Helpers.CreateAddAbilityResource(resource));

            var heal_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                                      "EvangelistChannelEnergyHealLiving",
                                                                      "",
                                                                      $"Channeling positive energy causes a burst that heals all living creatures in a 30 - foot radius centered on the evangelist. The amount of damage healed is equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)} at level 3 and increases by 1d{BalanceFixes.getDamageDieString(DiceType.D6)} every 2 levels except at levels 9 and 15.",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(resource, true, 1));
            var harm_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                                                      "EvangelistChannelEnergyHarmUndead",
                                                                      "",
                                                                      "Channeling energy causes a burst that damages all undead creatures in a 30 - foot radius centered on the evangelist. The amount of damage dealt is equal to 1d6 at level 3 and increases by 1d6 every 2 levels except at levels 9 and 15. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the evangelist's level + the evangelist's Charisma modifier.",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(resource, true, 1));

            var heal_living_base = Common.createVariantWrapper("EvangelistPositiveHealBase", "", heal_living);
            var harm_undead_base = Common.createVariantWrapper("EvangelistPositiveHarmBase", "", harm_undead);

            ChannelEnergyEngine.storeChannel(heal_living, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);

            channel_positive_energy.AddComponent(Helpers.CreateAddFacts(heal_living_base, harm_undead_base));

            channel_negative_energy = Helpers.CreateFeature("EvangelistChannelNegativeEnergyFeature",
                                                                "Channel Negative Energy",
                                                                "An evil evangelist (or a neutral evangelist who worships an evil deity) channels negative energy and can choose to deal damage to living creatures or to heal undead creatures.\n"
                                                                + "Channeling energy causes a burst that either heals all undead creatures or damages all living creatures in a 30-foot radius centered on the evangelist. The amount of damage dealt or healed is equal to 1d6 at level 3 and increases by 1d6 every 2 levels except at levels 9 and 15. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the evangelist's level + the evangelist's Charisam modifier. Creatures healed by channel energy cannot exceed their maximum hit point total—all excess healing is lost. This is a standard action that does not provoke an attack of opportunity.",
                                                                "",
                                                                negative_energy_feature.Icon,
                                                                FeatureGroup.None,
                                                                negative_energy_feature.GetComponent<PrerequisiteFeature>(),
                                                                Helpers.CreateAddAbilityResource(resource));

            var harm_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHarm,
                                                          "EvangelsitChannelEnergyHarmLiving",
                                                          "",
                                                          $"Channeling energy causes a burst that damages all living creatures in a 30 - foot radius centered on the evangelist. The amount of damage dealt is equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)} at level 3 and increases by 1d{BalanceFixes.getDamageDieString(DiceType.D6)} every 2 levels except at levels 9 and 15. Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the evangelist's level + the evangelist's Charisma modifier. This is a standard action that does not provoke an attack of opportunity.",
                                                          "",
                                                          context_rank_config,
                                                          dc_scaling,
                                                          Helpers.CreateResourceLogic(resource, true, 1));

            var heal_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHeal,
                                              "EvangelistChannelEnergyHealUndead",
                                              "",
                                              $"Channeling positive energy causes a burst that heals all undead creatures in a 30 - foot radius centered on the evangelist. The amount of damage healed is equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)} at level 3 and increases by 1d{BalanceFixes.getDamageDieString(DiceType.D6)} every 2 levels except at levels 9 and 15.",
                                              "",
                                              context_rank_config,
                                              dc_scaling,
                                              Helpers.CreateResourceLogic(resource, true, 1));


            var harm_living_base = Common.createVariantWrapper("EvangelisttNegativeHarmBase", "", harm_living);
            var heal_undead_base = Common.createVariantWrapper("EvangelistNegativeHealBase", "", heal_undead);

            ChannelEnergyEngine.storeChannel(harm_living, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHarm);
            ChannelEnergyEngine.storeChannel(heal_undead, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHeal);

            channel_negative_energy.AddComponent(Helpers.CreateAddFacts(harm_living_base, heal_undead_base));
            channel_energy = Helpers.CreateFeatureSelection("EvangelistChannelEnergyFeature",
                                                             "Channel energy",
                                                             "Starting at 3rd level, an evangelist can release a wave of energy by channeling the power of his faith through his holy (or unholy) symbol. This energy can be used to deal or heal damage, depending on the type of energy channeled and the creatures targeted. Using this ability is a standard action and doesn’t provoke an attack of opportunity. The evangelist must present a holy (or unholy) symbol to use this ability. A good evangelist (or one who worships a good deity) channels positive energy and can choose to heal living creatures or to deal damage to undead creatures. An evil evangelist (or one who worships an evil deity) channels negative energy and can choose to deal damage to living creatures or heal undead creatures. A neutral evangelist who worships a neutral deity (or one who is not devoted to a particular deity) can select the type of energy she channels.\n"
                                                             + $"Channeling energy causes a burst that affects all creatures of one type (either undead or living) in a 30 - foot radius centered on the evangelist. The amount of damage dealt or healed is equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)} at level 3 and increases by 1d{BalanceFixes.getDamageDieString(DiceType.D6)} every 2 levels except at levels 9 and 15. Creatures that take damage from channeled energy must succeed at a Will saving throw to halve the damage. The save DC is 10 + 1/2 the evangelist’s level + the evangelist’s Charisma modifier. Creatures healed by channeled energy cannot exceed their maximum hit point total—all excess healing is lost. An evangelist can choose whether or not to include himself in this effect.",
                                                             "",
                                                             null,
                                                             FeatureGroup.None
                                                             );
            channel_energy.AllFeatures = new BlueprintFeature[] { channel_positive_energy, channel_negative_energy };
            var extra_channel = ChannelEnergyEngine.createExtraChannelFeat(heal_living, channel_energy, "ExtraChannelEvangelist", "Extra Channel (Evangelist)", "");         
        }
    }
}
