using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CallOfTheWild.NewMechanics;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Experience;
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
    class UndeadLord
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeatureSelection undeath_subdomain_selection;
        static public BlueprintProgression undeath_subdomain;
        static public BlueprintFeature command_undead;
        static public BlueprintFeature unlife_healer_empowered;
        static public BlueprintFeature unlife_healer_maximized;
        static public BlueprintFeatureSelection bonus_feats;
        static public BlueprintFeature corpse_companion;
        static public BlueprintFeatureSelection corpse_companion_selection;
        static public BlueprintFeatureSelection deity_selection;
        static public BlueprintArchetype corpse_companion_archetype;

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "UndeadArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Undead Lord");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "An undead lord is a cleric focused on using necromancy to control undead. Her flock is the walking dead and her choir the keening spirits of the damned. This unliving congregation is the manifestation of her unceasing love affair with death.");
            });
            Helpers.SetField(archetype, "m_ParentClass", cleric_class);
            library.AddAsset(archetype, "");

            createDeitySelectionAndUndeadSubdomain();
            
            createCommandUndead();
            createUnlifeHealer();
            createBonusFeats();
            createCorpseCompanion();

            var channel_selection = library.Get<BlueprintFeatureSelection>("d332c1748445e8f4f9e92763123e31bd");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, library.Get<BlueprintFeature>("59e7a76987fe3b547b9cce045f4db3e4"), //deity selection
                                                                                library.Get<BlueprintFeature>("48525e5da45c9c243a343fc6545dbdb9"), //domain selection
                                                                                library.Get<BlueprintFeature>("43281c3d7fe18cc4d91928395837cd1e"), //second domain
                                                                                channel_selection)
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, deity_selection, undeath_subdomain_selection, channel_selection, command_undead, corpse_companion_selection),
                                                       Helpers.LevelEntry(8, unlife_healer_empowered),
                                                       Helpers.LevelEntry(10, bonus_feats),
                                                       Helpers.LevelEntry(16, unlife_healer_maximized),
                                                     };

            cleric_class.Progression.UIDeterminatorsGroup = cleric_class.Progression.UIDeterminatorsGroup.AddToArray(deity_selection, undeath_subdomain_selection, corpse_companion_selection);
            cleric_class.Progression.UIGroups = cleric_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(command_undead, unlife_healer_empowered, bonus_feats, unlife_healer_maximized));
            cleric_class.Archetypes = cleric_class.Archetypes.AddToArray(archetype);
        }


        static void createCorpseCompanion()
        {
            var undead_class = library.Get<BlueprintCharacterClass>("19a2d9e58d916d04db4cd7ad2c7a3ee2");

            corpse_companion_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "CorpseCompanionArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Corpse Companion");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", undead_class.Description);
            });
            Helpers.SetField(corpse_companion_archetype, "m_ParentClass", undead_class);
            library.AddAsset(corpse_companion_archetype, "");
            corpse_companion_archetype.RemoveFeatures = new LevelEntry[0];
            var companion_proficiency = library.CopyAndAdd<BlueprintFeature>("a23591cc77086494ba20880f87e73970", "CorpseCompanionProficiency", "");
            companion_proficiency.SetNameDescription("Proficiencies", "Your corpse companion is proficient with all simple and martial weapons and with all armor (heavy, light, and medium) and shields (including tower shields).");
            var str_dex_bonus = library.CopyAndAdd<BlueprintFeature>("0c80276018694f24fbaf59ec7b841f2b", "CorpseCompanionStrDexIncreaseFeature", "");
            str_dex_bonus.SetNameDescription("Physical Prowess", "Corpse companion receives +1 bonus to their Strength and Dexterity.");

            var size_increase1 = Helpers.CreateFeature("CorpseCompanionLargeFeature",
                                                       "Size Increase: Large",
                                                       "Corpse companion growth in size and receives +4 to its strength, -2 to dexterity and +10 feet bonus to speed.",
                                                       "",
                                                       Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"),
                                                       FeatureGroup.None,
                                                       Helpers.Create<SizeMechanics.PermanentSizeOverride>(a => a.size = Size.Large),
                                                       Helpers.CreateAddStatBonus(StatType.Strength, 4, ModifierDescriptor.Racial),
                                                       Helpers.CreateAddStatBonus(StatType.Dexterity, -2, ModifierDescriptor.Racial),
                                                       Helpers.CreateAddStatBonus(StatType.Speed, 10, ModifierDescriptor.Racial)
                                                       );

            var size_increase2 = Helpers.CreateFeature("CorpseCompanionHugeFeature",
                                                       "Size Increase: Huge",
                                                       "Corpse companion growth in size and receives +4 to its strength, -2 to dexterity, +1 bonus to natural armor and +10 feet bonus to speed..",
                                                       "",
                                                       Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"),
                                                       FeatureGroup.None,
                                                       Helpers.Create<SizeMechanics.PermanentSizeOverride>(a => a.size = Size.Huge),
                                                       Helpers.CreateAddStatBonus(StatType.Strength, 4, ModifierDescriptor.Racial),
                                                       Helpers.CreateAddStatBonus(StatType.Dexterity, -2, ModifierDescriptor.Racial),
                                                       Helpers.CreateAddStatBonus(StatType.AC, 1, ModifierDescriptor.NaturalArmor),
                                                       Helpers.CreateAddStatBonus(StatType.Speed, 10, ModifierDescriptor.Racial)
                                                       );

            corpse_companion_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, companion_proficiency),
                                                              Helpers.LevelEntry(2, str_dex_bonus),
                                                              Helpers.LevelEntry(4, str_dex_bonus),
                                                              Helpers.LevelEntry(6, str_dex_bonus),
                                                              Helpers.LevelEntry(7, size_increase1),
                                                              Helpers.LevelEntry(12, str_dex_bonus),
                                                              Helpers.LevelEntry(8, str_dex_bonus),
                                                              Helpers.LevelEntry(12, str_dex_bonus),
                                                              Helpers.LevelEntry(14, str_dex_bonus),
                                                              Helpers.LevelEntry(15, size_increase2),
                                                              Helpers.LevelEntry(16, str_dex_bonus),
                                                              Helpers.LevelEntry(18, str_dex_bonus),
                                                              Helpers.LevelEntry(20, str_dex_bonus)
                                                            };
            undead_class.Archetypes = undead_class.Archetypes.AddToArray(corpse_companion_archetype);

            var skeleton = library.CopyAndAdd<BlueprintUnit>("6c94133d39dea8544a591836c78eaaf3", "CorpseCompanionSkeletonUnit", "");
            skeleton.LocalizedName = skeleton.LocalizedName.CreateCopy();
            skeleton.LocalizedName.String = Helpers.CreateString(skeleton.name + ".Name", "Corpse Companion");
            skeleton.Body = skeleton.Body.CloneObject();
            skeleton.Body.PrimaryHand = null;
            skeleton.Body.Armor = null;
            skeleton.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("118fdd03e569a66459ab01a20af6811a");//claw 1d4
            skeleton.Faction = library.Get<BlueprintFaction>("d8de50cc80eb4dc409a983991e0b77ad"); //neutrals
            skeleton.RemoveComponents<Experience>();
            skeleton.RemoveComponents<AddTags>();
            skeleton.AddComponent(Helpers.Create<AllowDyingCondition>());
            skeleton.AddComponent(Helpers.Create<AddResurrectOnRest>());
            skeleton.AddComponent(Helpers.Create<Eidolon.CorpseCompanionComponent>());
            skeleton.ReplaceComponent<AddClassLevels>(a =>
                                                        {
                                                            a.DoNotApplyAutomatically = true;
                                                            a.Archetypes = new BlueprintArchetype[] { corpse_companion_archetype };
                                                            a.Levels = 0;
                                                            a.Skills = new StatType[0];
                                                        }
                                                     );
            Helpers.SetField(skeleton, "m_Portrait", Helpers.createPortrait("CorpseCompanionProtrait", "CorpseCompanion", ""));
            corpse_companion = Helpers.CreateFeature("CorpseCompanionFeature",
                                                         "Corpse Companion",
                                                         "With a ritual requiring 8 hours, an undead lord can animate a single skeleton whose Hit Dice do not exceed her cleric level. This corpse companion automatically follows her commands and does not need to be controlled by her. She cannot have more than one corpse companion at a time. It does not count against the number of Hit Dice of undead controlled by other methods.",
                                                        "",
                                                        Helpers.GetIcon("a1a8bf61cadaa4143b2d4966f2d1142e"), //undead bloodline
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            corpse_companion.IsClassFeature = true;
            corpse_companion.ReapplyOnLevelUp = true;
           

            corpse_companion.ReplaceComponent<AddPet>(a => { a.Pet = skeleton; a.UpgradeLevel = 100; });
            corpse_companion.AddComponent(Helpers.Create<CompanionMechanics.CustomLevelProgression>());

            var rank_profgression = library.CopyAndAdd<BlueprintProgression>("3853d5405ebfc0f4a86930bb7082b43b", "CorpseCOmpanionClericProgression", "");
            rank_profgression.Classes = new BlueprintCharacterClass[] { archetype.GetParentClass() };
            corpse_companion_selection = Helpers.CreateFeatureSelection("CorpseCompanionFeatureSelection",
                                                    corpse_companion.Name,
                                                    corpse_companion.Description,
                                                    "",
                                                    corpse_companion.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.Create<AddFeatureOnApply>(a => a.Feature = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d")),
                                                    Helpers.Create<AddFeatureOnApply>(a => a.Feature = rank_profgression)
                                                    );
            corpse_companion_selection.Group = FeatureGroup.AnimalCompanion;
            corpse_companion_selection.AllFeatures = corpse_companion_selection.AllFeatures.AddToArray(corpse_companion);

        }


        static void createBonusFeats()
        {
            bonus_feats = Helpers.CreateFeatureSelection("BonusFeatUndeadLordSelection",
                                                         "Bonus Feat",
                                                         "At 10th level, an undead lord may select one of the following as a bonus feat: Channel Smite, Extra Channel, Improved Channel or Quick Channel.",
                                                         "",
                                                         null,
                                                         FeatureGroup.None);
            bonus_feats.AllFeatures = new BlueprintFeature[]
                                                            {
                                                                library.Get<BlueprintFeature>("cd9f19775bd9d3343a31a065e93f0c47"),
                                                                ChannelEnergyEngine.improved_channel,
                                                                ChannelEnergyEngine.channel_smite,
                                                                ChannelEnergyEngine.quick_channel
                                                            };
        }



        static void createUnlifeHealer()
        {
            unlife_healer_empowered = Helpers.CreateFeature("UnlifeHealerEmpoweredFeature",
                                                            "Unlife Healer",
                                                            "At 8th level, the undead lord’s spells, spell-like abilities, and supernatural abilities used to heal undead heal an extra 50% damage. At 16th level, these effects automatically heal the maximum possible damage for the effect + the extra 50%. This does not stack with abilities or feats such as Empower Spell or Maximize Spell.",
                                                            "",
                                                            Helpers.GetIcon("89df18039ef22174b81052e2e419c728"),
                                                            FeatureGroup.None,
                                                            Helpers.Create<HealingMechanics.UndeadHealingMetamagic>(a => a.empower = true)
                                                            );

            unlife_healer_maximized = Helpers.CreateFeature("UnlifeHealerMAximziedFeature",
                                                            "Unlife Healer",
                                                            "At 8th level, the undead lord’s spells, spell-like abilities, and supernatural abilities used to heal undead heal an extra 50% damage. At 16th level, these effects automatically heal the maximum possible damage for the effect + the extra 50%. This does not stack with abilities or feats such as Empower Spell or Maximize Spell.",
                                                            "",
                                                            Helpers.GetIcon("89df18039ef22174b81052e2e419c728"),
                                                            FeatureGroup.None,
                                                            Helpers.Create<HealingMechanics.UndeadHealingMetamagic>(a => a.maximize = true)
                                                            );
        }



        static void createCommandUndead()
        {
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var resource = library.Get<BlueprintAbilityResource>("5e2bba3e07c37be42909a12945c27de7");

            command_undead = ChannelEnergyEngine.createCommandUndead("UndeadLord", 
                                                                     "Command Undead",
                                                                     "As a standard action, you can use one of your uses of channel negative energy to enslave one undead creature within close range. Undead receive a Will save to negate the effect. The DC for this Will save is equal to 10 + 1/2 your cleric level + your Charisma modifier. Undead that fail their saves fall under your control, obeying your commands to the best of their ability, as if under the effects of control undead.",
                                                                     StatType.Charisma, new BlueprintCharacterClass[] {cleric_class}, resource);
        }

        static void createDeitySelectionAndUndeadSubdomain()
        {
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            var druid_class = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var cleric_deity = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");

            var pharasma = library.Get<BlueprintFeature>("458750bc214ab2e44abdeae404ab22e9");
            var allow_death = library.Get<BlueprintFeature>("a099afe1b0b32554199b230699a69525");

            undeath_subdomain = library.CopyAndAdd<BlueprintProgression>("710d8c959e7036448b473ffa613cdeba", "UndeadSubdomain", "");

            var spell_list = library.CopyAndAdd<BlueprintSpellList>("436986e90d1e81b45a1accb6fa7261f0", "UndeadSubdomainSpellList", "");
            Common.excludeSpellsFromList(spell_list, a => false);

            var undead_domain_spelllist_feature = library.CopyAndAdd<BlueprintFeature>("b5f473955a944854f97c99600ef88130", "UndeadDomainSpellListFeature", "");
            undead_domain_spelllist_feature.ReplaceComponent< AddSpecialSpellList>(a => a.SpellList = spell_list);

            var base_feature = library.CopyAndAdd<BlueprintFeature>("9809efa15e5f9ad478594479af575a5d", "UndeadSubdomainBaseFeature", "");
            base_feature.ReplaceComponent<AddFeatureOnClassLevel>(a => a.Feature = undead_domain_spelllist_feature);

            var resource = Helpers.CreateAbilityResource("DeathsKissAbilityResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Wisdom);
            var buff = Helpers.CreateBuff("UndeadSubdomainBaseBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.Create<UndeadMechanics.ConsiderUndeadForHealing>()
                                          );
            var ability = Helpers.CreateAbility("UndeadSubdomainBaseAbility",
                                                "Death's Kiss",
                                                "You can cause a creature to take on some of the traits of the undead with a melee touch attack. Touched creatures are treated as undead for the purposes of effects that heal or cause damage based on positive and negative energy. This effect lasts for a number of rounds equal to 1/2 your cleric level (minimum 1). It does not apply to the Turn Undead or Command Undead feats. You can use this ability a number of times per day equal to 3 + your Wisdom modifier.",
                                                "",
                                                LoadIcons.Image2Sprite.Create(@"AbilityIcons/DeathsKiss.png"),
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "1 round/ 2 levels",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff,
                                                                                                             Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                                             dispellable: false)),
                                                Common.createAbilitySpawnFx("e93261ee4c3ea474e923f6a645a3384f", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                classes: new BlueprintCharacterClass[] { cleric_class, inquisitor_class, druid_class },
                                                                                progression: ContextRankProgression.Div2, min: 1
                                                                                ),
                                                Helpers.CreateDeliverTouch()
                                                );
            ability.setMiscAbilityParametersTouchHarmful();
            var touch_ability = Helpers.CreateTouchSpellCast(ability, resource);
            base_feature.ReplaceComponent<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { touch_ability });
            base_feature.ReplaceComponent<AddAbilityResources>(a => a.Resource = resource);
            base_feature.SetNameDescriptionIcon(ability.Name, ability.Description, ability.Icon);

            undeath_subdomain.LevelEntries = new LevelEntry[]
            {
                Helpers.LevelEntry(1, base_feature),
                undeath_subdomain.LevelEntries[1]
            };
            undeath_subdomain.UIGroups = Helpers.CreateUIGroups(base_feature, undeath_subdomain.UIGroups[0].Features[1]);
            undeath_subdomain.ReplaceComponent<LearnSpellList>(a => a.SpellList = spell_list);
            undeath_subdomain.AddComponent(Helpers.PrerequisiteNoFeature(pharasma));
            undeath_subdomain.SetNameDescriptionIcon("Undead Subdomain",
                                                     "You can grant the living some undead traits with a touch, and find comfort in the presence of the dead.\nDeath’s Kiss: You can cause a creature to take on some of the traits of the undead with a melee touch attack. Touched creatures are treated as undead for the purposes of effects that heal or cause damage based on positive and negative energy. This effect lasts for a number of rounds equal to 1/2 your cleric level (minimum 1). It does not apply to the Turn Undead or Command Undead feats. You can use this ability a number of times per day equal to 3 + your Wisdom modifier.\nDeath's Embrace: At 8th level, you heal damage instead of taking damage from channeled negative energy. If the channeled negative energy targets undead, you heal hit points just like undead in the area.\nIf you are undead, then you instead do not take damage from positive energy.\nDomain Spells: Cause Fear, Ghoul touch, Bestow Curse, Enervation, Slay Living, Circle of Death, Destruction, Horrid Wilting, Energy Drain.",
                                                     Helpers.GetIcon("4b76d32feb089ad4499c3a1ce8e1ac27")); //animate dead icon

            var undeath_subdomain_secondary = library.CopyAndAdd(undeath_subdomain, "UndeadSubdomainProgressionSecondary", "");
            var undeath_subdomain_blight = library.CopyAndAdd(undeath_subdomain, "UndeadSubdomainProgressionBligthDruid", "");
            undeath_subdomain_blight.RemoveComponents<PrerequisiteFeature>();
            undeath_subdomain_blight.RemoveComponents<PrerequisiteNoFeature>();
            undeath_subdomain_blight.AddComponent(Helpers.PrerequisiteNoFeature(pharasma));
            undeath_subdomain_secondary.RemoveComponents<LearnSpellList>();
            undeath_subdomain.AddComponent(Helpers.PrerequisiteNoFeature(undeath_subdomain_secondary));
            undeath_subdomain_secondary.AddComponent(Helpers.PrerequisiteNoFeature(undeath_subdomain));

           
            undeath_subdomain_blight.RemoveComponents<LearnSpellList>();
            undeath_subdomain_blight.AddComponent(Helpers.PrerequisiteClassLevel(druid_class, 1));
            undeath_subdomain_blight.Classes = new BlueprintCharacterClass[] { druid_class };
            var death_spells_feature_druid = library.CopyAndAdd<BlueprintFeature>("01c9f3756c9d2e1488b6a2d29dd9d37f", "UndeadDomainSpellListDruidFeature", "");
            death_spells_feature_druid.ReplaceComponent<AddSpecialSpellList>(a => a.SpellList = spell_list);
            undeath_subdomain_blight.LevelEntries = new LevelEntry[] { Helpers.LevelEntry(1, base_feature, death_spells_feature_druid), undeath_subdomain.LevelEntries[1] };
            
            var death_domain = library.Get<BlueprintFeature>("710d8c959e7036448b473ffa613cdeba");
            var death_domain2 = library.Get<BlueprintFeature>("023794a8386506c49aad142846700594");
            var death_domain_blight = library.Get<BlueprintFeature>("7d3f0f00b1f8ad54ba6abc6aeac84c05");
            death_domain.AddComponents(Helpers.PrerequisiteNoFeature(undeath_subdomain), Helpers.PrerequisiteNoFeature(undeath_subdomain_secondary));
            death_domain2.AddComponents(Helpers.PrerequisiteNoFeature(undeath_subdomain), Helpers.PrerequisiteNoFeature(undeath_subdomain_secondary));
            death_domain_blight.AddComponents(Helpers.PrerequisiteNoFeature(undeath_subdomain_blight));
            undeath_subdomain_blight.AddComponent(Helpers.PrerequisiteNoFeature(death_domain_blight));

            var domain_selection = library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");
            var blight_druid_domain = library.Get<BlueprintFeatureSelection>("096fc02f6cc817a43991c4b437e12b8e");
            var secondary_domain_selection = library.Get<BlueprintFeatureSelection>("43281c3d7fe18cc4d91928395837cd1e");
            domain_selection.AllFeatures = domain_selection.AllFeatures.AddToArray(undeath_subdomain);
            secondary_domain_selection.AllFeatures = secondary_domain_selection.AllFeatures.AddToArray(undeath_subdomain_secondary);
            blight_druid_domain.AllFeatures = blight_druid_domain.AllFeatures.AddToArray(undeath_subdomain_blight);

            Common.replaceDomainSpell(undeath_subdomain, NewSpells.ghoul_touch, 2);
            Common.replaceDomainSpell(undeath_subdomain, library.Get<BlueprintAbility>("f34fb78eaaec141469079af124bcfa0f"), 4); //enervation
            Common.replaceDomainSpell(undeath_subdomain, library.Get<BlueprintAbility>("37302f72b06ced1408bf5bb965766d46"), 9); //energy drain

            undeath_subdomain_selection = library.CopyAndAdd(domain_selection, "UndeadSubdomainSelection", "");
            undeath_subdomain_selection.AllFeatures = new BlueprintFeature[] { undeath_subdomain };
            undeath_subdomain_selection.SetNameDescriptionIcon("Death Magic", "An undead lord must select the Undead subdomain. She does not gain a second domain.", undeath_subdomain.Icon);

            deity_selection = library.CopyAndAdd(cleric_deity, "UndeadLordDietySelection", "");
            deity_selection.AllFeatures = new BlueprintFeature[0];
            deity_selection.SetNameDescription("Undead Lord Deity",
                                               "Undead lord deity’s portfolio must include the Death domain or a similar domain that promotes undeath.");

            foreach (var f in cleric_deity.AllFeatures)
            {
                if (f != pharasma && f.GetComponent<AddFacts>().Facts.Contains(allow_death))
                {
                    deity_selection.AllFeatures = deity_selection.AllFeatures.AddToArray(f);
                }
            }
        }
    }
}
