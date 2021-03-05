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
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Mechanics.Properties;

namespace CallOfTheWild
{
    public class Antipaladin
    {
        static internal LibraryScriptableObject library => Main.library;
        static internal bool test_mode = false;
        static public BlueprintCharacterClass antipaladin_class;
        static public BlueprintProgression antipaladin_progression;

        static public BlueprintFeatureSelection antipaladin_deity;
        static public BlueprintFeature antipaladin_proficiencies;
        static public BlueprintFeature touch_of_corruption;
        static public BlueprintAbility touch_of_corruption_base;
        static public BlueprintAbility ranged_channel_wrapper;
        static public BlueprintFeature smite_good;
        static public BlueprintFeature smite_good_extra_use;
        static public BlueprintFeature unholy_resilence;
        static public BlueprintFeature aura_of_cowardice;
        static public BlueprintFeature plague_bringer;
        static public BlueprintFeature channel_negative_energy;
        static public BlueprintFeature[] fiendish_boon;
        static public BlueprintFeature aura_of_despair;
        static public BlueprintFeature aura_of_vengeance;
        static public BlueprintFeature aura_of_sin;
        static public BlueprintFeature aura_of_deparvity;
        static public BlueprintFeature tip_of_spear;
        static public BlueprintFeature antipaladin_alignment;

        static public BlueprintAbilityResource smite_resource;
        static public BlueprintAbilityResource touch_of_corruption_resource;
        static public BlueprintAbilityResource fiendish_boon_resource;

        static public BlueprintFeature extra_touch_of_corruption;
        static public BlueprintFeature extra_channel;
        static public BlueprintAbilityResource extra_channel_resource;

        static public BlueprintFeatureSelection cruelty;

        static public BlueprintArchetype insinuator;
        static public BlueprintFeature invocation;
        static public BlueprintFeature smite_impudence;
        static public BlueprintFeature smite_impudence_extra_use;
        static public BlueprintFeature selfish_healing;
        static public BlueprintFeature aura_of_ego;
        static public BlueprintFeature stubborn_health;
        static public BlueprintFeatureSelection greeds;
        static public BlueprintFeature insinuator_channel_energy;
        static public BlueprintFeature channel_positive_energy;
        static public BlueprintFeature aura_of_ambition;
        static public BlueprintFeature aura_of_glory;
        static public BlueprintFeature aura_of_belief;
        static public BlueprintFeature aura_of_indomitability;
        static public BlueprintFeature personal_champion;
        static public BlueprintFeatureSelection insinuator_bonus_feat;


        static public BlueprintArchetype blighted_myrmidon;
        static public BlueprintFeature smite_nature;
        static public BlueprintFeature smite_nature_extra_use;
        static public BlueprintFeature aura_of_decay;

        static public BlueprintArchetype iron_tyrant;
        static public BlueprintFeature iron_fist;
        static public BlueprintFeatureSelection iron_tyrant_bonus_feat;
        static public BlueprintFeature unstoppable;
        static public BlueprintFeature[] fiendish_bond;


        static public BlueprintArchetype dread_vanguard;
        static public BlueprintFeature[] beacon_of_evil;
        static public BlueprintFeature[] dark_emissary;


        static public BlueprintFeature ability_focus_touch_of_corruption;



        internal class CrueltyEntry
        {
            static Dictionary<string, CrueltyEntry> cruelties_map = new Dictionary<string, CrueltyEntry>();
            static Dictionary<string, BlueprintFeature> features_map = new Dictionary<string, BlueprintFeature>();
            string dispaly_name;
            string description;
            BlueprintBuff[] buffs;
            string prerequisite_id;
            int level;
            int divisor;
            SavingThrowType save;

            internal static void addCruelty(string cruelty_name, string cruelty_description, int lvl, string prerequisite_name, int round_divisor, SavingThrowType save_type, params BlueprintBuff[] effects)
            {
                cruelties_map.Add(cruelty_name, new CrueltyEntry(cruelty_name, cruelty_description, lvl, prerequisite_name, round_divisor, save_type, effects));
            }


            CrueltyEntry(string cruelty_name, string cruelty_description, int lvl, string prerequisite_name, int round_divisor, SavingThrowType save_type, params BlueprintBuff[] effects)
            {
                buffs = effects;
                level = lvl;
                prerequisite_id = prerequisite_name;
                divisor = round_divisor;
                dispaly_name = cruelty_name;
                description = cruelty_description;
                save = save_type;
            }


            BlueprintFeature createCrueltyFeature(BlueprintAbility base_ability)
            {
                var abilities_touch = new List<BlueprintAbility>();
                var abilities_hit = new List<BlueprintAbility>();

                var feature = Helpers.CreateFeature(dispaly_name + "CrueltyFeature",
                                                    "Cruelty: " + dispaly_name,
                                                    description,
                                                    "",
                                                    buffs[0].Icon,
                                                    FeatureGroup.None);

                if (level > 0)
                {
                    feature.AddComponent(Helpers.PrerequisiteClassLevel(antipaladin_class, level));
                }
                if (!prerequisite_id.Empty())
                {
                    feature.AddComponent(Helpers.PrerequisiteFeature(features_map[prerequisite_id]));
                }
                foreach (var b in buffs)
                {
                    var touch_ability = base_ability.Variants[0].GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility;
                    var touch_cruelty = library.CopyAndAdd(touch_ability, b.name + touch_ability.name, "");
                    touch_cruelty.AvailableMetamagic = Metamagic.Reach; //for dread vanguard aura
                    touch_cruelty.SetNameDescriptionIcon("Cruelty: " + b.Name,
                                                         description +"\n" + b.Name + ": " + b.Description,
                                                         b.Icon);

                    var duration = Helpers.CreateContextDuration();
                    if (divisor < 0)
                    {
                        duration = Helpers.CreateContextDuration(-divisor);
                    }
                    else if (divisor > 0)
                    {
                        duration = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus));
                    }

                    var effect_action = Helpers.CreateActionSavingThrow(save,
                                                                        new GameAction[] { Helpers.CreateConditionalSaved(null, Common.createContextActionApplyBuff(b, duration, is_permanent: divisor == 0, dispellable: false)) }
                                                                        );

                    touch_cruelty.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(effect_action)));
                    if (divisor > 0)
                    {
                        touch_cruelty.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.DivStep, AbilityRankType.StatBonus,
                                                                                   stepLevel: divisor, classes: getAntipaladinArray()));
                    }
                    var spell_descriptor_component = b.GetComponent<SpellDescriptorComponent>();

                    var cast_cruelty = Helpers.CreateTouchSpellCast(touch_cruelty);
                    cast_cruelty.AvailableMetamagic = Metamagic.Reach; //for dread vanguard aura
                    cast_cruelty.AddComponent(base_ability.Variants[0].GetComponent<AbilityResourceLogic>());
                    cast_cruelty.AddComponent(Common.createAbilityShowIfCasterHasFact(feature));
                    cast_cruelty.AddComponents(Common.createAbilityTargetHasFact(true, Common.construct), Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
                    cast_cruelty.Parent = base_ability;
                    cast_cruelty.Parent.addToAbilityVariants(cast_cruelty);
                    if (spell_descriptor_component != null)
                    {
                        touch_cruelty.AddComponent(spell_descriptor_component);
                        cast_cruelty.AddComponent(spell_descriptor_component);
                    }
                }
                return feature;
            }


            internal static BlueprintFeature[] createCruelties(BlueprintAbility base_ability)
            {
                foreach (var kv in cruelties_map)
                {
                    features_map.Add(kv.Key, kv.Value.createCrueltyFeature(base_ability));
                }

                return features_map.Values.ToArray();
            }
        }

        internal static void creatAntipaldinClass()
        {
            Main.logger.Log("Antipaladin class test mode: " + test_mode.ToString());
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var paladin_class = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            var inquisitor_class = library.Get<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");

            antipaladin_class = Helpers.Create<BlueprintCharacterClass>();
            antipaladin_class.name = "AntipaladinClass";
            library.AddAsset(antipaladin_class, "");

            antipaladin_class.LocalizedName = Helpers.CreateString("Antipaladin.Name", "Antipaladin");
            antipaladin_class.LocalizedDescription = Helpers.CreateString("Antipaladin.Description",
                                                                         "Although it is a rare occurrence, paladins do sometimes stray from the path of righteousness. Most of these wayward holy warriors seek out redemption and forgiveness for their misdeeds, regaining their powers through piety, charity, and powerful magic. Yet there are others, the dark and disturbed few, who turn actively to evil, courting the dark powers they once railed against in order to take vengeance on their former brothers. It’s said that those who climb the farthest have the farthest to fall, and antipaladins are living proof of this fact, their pride and hatred blinding them to the glory of their forsaken patrons.\n"
                                                                         + "Antipaladins become the antithesis of their former selves. They make pacts with fiends, take the lives of the innocent, and put nothing ahead of their personal power and wealth. Champions of evil, they often lead armies of evil creatures and work with other villains to bring ruin to the holy and tyranny to the weak. Not surprisingly, paladins stop at nothing to put an end to such nefarious antiheroes.\n"
                                                                         + "Role: Antipaladins are villains at their most dangerous. They care nothing for the lives of others and actively seek to bring death and destruction to ordered society. They rarely travel with those that they do not subjugate, unless as part of a ruse to bring ruin from within."
                                                                         );
            antipaladin_class.m_Icon = fighter_class.Icon;
            antipaladin_class.SkillPoints = paladin_class.SkillPoints;
            antipaladin_class.HitDie = DiceType.D10;
            antipaladin_class.BaseAttackBonus = paladin_class.BaseAttackBonus;
            antipaladin_class.FortitudeSave = paladin_class.FortitudeSave;
            antipaladin_class.ReflexSave = paladin_class.ReflexSave;
            antipaladin_class.WillSave = paladin_class.WillSave;
            antipaladin_class.Spellbook = createAntipaladinSpellbook();
            antipaladin_class.ClassSkills = paladin_class.ClassSkills;
            antipaladin_class.IsDivineCaster = true;
            antipaladin_class.IsArcaneCaster = false;
            antipaladin_class.StartingGold = paladin_class.StartingGold;
            antipaladin_class.PrimaryColor = inquisitor_class.PrimaryColor;
            antipaladin_class.SecondaryColor = inquisitor_class.SecondaryColor;
            antipaladin_class.RecommendedAttributes = paladin_class.RecommendedAttributes;
            antipaladin_class.NotRecommendedAttributes = paladin_class.NotRecommendedAttributes;
            antipaladin_class.EquipmentEntities = paladin_class.EquipmentEntities;
            antipaladin_class.MaleEquipmentEntities = paladin_class.MaleEquipmentEntities;
            antipaladin_class.FemaleEquipmentEntities = paladin_class.FemaleEquipmentEntities;
            antipaladin_class.ComponentsArray = paladin_class.ComponentsArray.ToArray();
            antipaladin_class.ReplaceComponent<PrerequisiteAlignment>(p => p.Alignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Evil);
            antipaladin_class.StartingItems = paladin_class.StartingItems;
            createAntipaladinProgression();
            antipaladin_class.Progression = antipaladin_progression;

            createInsinuator();
            createBlightedMyrmidon();
            createIronTyrant();
            createDreadVanguard();
            antipaladin_class.Archetypes = new BlueprintArchetype[] {insinuator, blighted_myrmidon, iron_tyrant, dread_vanguard }; //blighted myrmidon, insinuator, iron tyrant, dread vanguard, seal breaker
            Helpers.RegisterClass(antipaladin_class);
            fixAntipaladinFeats();

            antipaladin_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = paladin_class));
            antipaladin_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = VindicativeBastard.vindicative_bastard_class));
            paladin_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = antipaladin_class));
            VindicativeBastard.vindicative_bastard_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = antipaladin_class));

            Common.addMTDivineSpellbookProgression(antipaladin_class, antipaladin_class.Spellbook, "MysticTheurgeAntipaladin",
                                                   Common.createPrerequisiteClassSpellLevel(antipaladin_class, 2));
        }


        static void createDreadVanguard()
        {
            dread_vanguard = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DreadVanguardArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Dread Vanguard");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some antipaladins serve or ally themselves with villains who are bent on earthly conquest. They care nothing for the intricacies of divine spellcasting, but malevolent energy still surrounds them. Whether alone or at the head of a marauding host, these cruel warriors bring suffering and death—but their presence also heralds the coming of a greater evil.");
            });
            Helpers.SetField(dread_vanguard, "m_ParentClass", antipaladin_class);
            library.AddAsset(dread_vanguard, "");

            createBeaconOfEvil();
            createDarkEmissary();

            dread_vanguard.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(2, touch_of_corruption),
                                                              Helpers.LevelEntry(14, aura_of_sin),
                                                             };

            dread_vanguard.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(2, touch_of_corruption),
                                                       Helpers.LevelEntry(4, beacon_of_evil[0]),
                                                       Helpers.LevelEntry(8, beacon_of_evil[1]),
                                                       Helpers.LevelEntry(12, beacon_of_evil[2]),
                                                       Helpers.LevelEntry(14, dark_emissary[0]),
                                                       Helpers.LevelEntry(16, beacon_of_evil[3]),
                                                       Helpers.LevelEntry(17, dark_emissary[1]),
                                                       Helpers.LevelEntry(20, beacon_of_evil[4], dark_emissary[2]),
                                                      };

            antipaladin_progression.UIGroups = antipaladin_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(beacon_of_evil));
            antipaladin_progression.UIGroups = antipaladin_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(dark_emissary));
            dread_vanguard.RemoveSpellbook = true;
            dread_vanguard.ChangeCasterType = true;
        }


        static void createDarkEmissary()
        {
            var resource = Helpers.CreateAbilityResource("DarkEmissaryResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(2, 17, 2, 3, 2, 0, 0.0f, getAntipaladinArray());

            var crushing_despair_buff = library.Get<BlueprintBuff>("36897b36a4bd63146b1d8509a17fc5ad");
            var pain_buff = library.CopyAndAdd<BlueprintBuff>("caae9592917719a41b601b678a8e6ddf", "SymbolofPainBuff", "");
            pain_buff.RemoveComponents<BuffAllSavesBonus>();
            pain_buff.RemoveComponents<SpellDescriptorComponent>();
            pain_buff.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Death));
            pain_buff.SetNameDescription("Pain", "The subject suffers a –4 penalty on attack rolls, ability checks, and skill checks.");

            var icon = Helpers.GetIcon("237427308e48c3341b3d532b9d3a001f");
            var immunity_buff = Helpers.CreateBuff("DarkEmissaryImmunityBuff",
                                                   "Dark Emissary Immunity",
                                                   "The creature can no longer be affected by Dark Emissary ability.",
                                                   "",
                                                   null,
                                                   null
                                                   );
            immunity_buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            dark_emissary = new BlueprintFeature[3];
            for (int i = 0; i < dark_emissary.Length; i++)
            {
                dark_emissary[i] = Helpers.CreateFeature($"DarkEmissary{(i + 1)}Feature",
                                                          "Dark Emissary",
                                                          "At 14th level, a dread vanguard becomes a true messenger of the forces of darkness he serves. Once per day, the dread vanguard can expend two uses of his touch of corruption ability to mark one location within medium range with the stain of evil. This location can be any point in space, but the ability works best if placed on an altar, shrine, or other site important to a community.\n"
                                                          + "The location is affected as if by a desecrate spell. Creatures approaching within 20 feet of the site must succeed at a Will save or suffer the effects of crushing despair.\n"
                                                          + "At 17th level, the dread vanguard can also mark the site with a symbol of pain, and at 20th level, he adds a symbol of weakness. If available, all three of these effects overlap.\n"
                                                          + "A location remains marked in this way for up to 1 minute per antipaladin level.\n"
                                                          + "Allies or evil creatures who serve the same power or organization as the dread vanguard are immune to the crushing despair and symbol effects, and automatically know that the location has been marked for their masters.\n"
                                                          + " At 17th and 20th levels, antipaladin can use this ability one additional time per day.",
                                                          "",
                                                          icon,
                                                          FeatureGroup.None
                                                          );
            }


            var area = library.CopyAndAdd(NewSpells.desecrate_area, "DarkEmissaryArea", "");
            //area.Size = 20.Feet();
            //area.Fx = Common.createPrefabLink("baa268c6db5723b4fa43c1b65f99bf0f"); //unholy aoe 30 feet

            var apply_despair = Common.createContextActionApplyBuff(crushing_despair_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);
            var apply_pain = Common.createContextActionApplyBuff(pain_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);
            var apply_weakness = Helpers.CreateActionDealDamage(StatType.Strength, Helpers.CreateContextDiceValue(DiceType.D6, 3));

            var actions = new GameAction[] {apply_despair, apply_pain, apply_weakness};

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i] = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(dark_emissary[i]),
                                                       Helpers.CreateActionSavingThrow(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, actions[i]))
                                                      );
            }
            actions = actions.AddToArray(Common.createContextActionApplyBuff(immunity_buff, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false));
            var effect = Helpers.CreateConditional(new Condition[] { /*Helpers.Create<ContextConditionIsEnemy>(),*/ Common.createContextConditionHasFact(immunity_buff, false) }, actions);


            area.AddComponent(Helpers.CreateAreaEffectRunAction(unitEnter: effect));
            area.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClass(antipaladin_class, StatType.Charisma));
            area.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray()));

            var ability = Helpers.CreateAbility("DarkEmissaryAbility",
                                                dark_emissary[0].Name,
                                                dark_emissary[0].Description,
                                                "",
                                                dark_emissary[0].Icon,
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Medium,
                                                Helpers.minutesPerLevelDuration,
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray()),
                                                Common.createContextCalculateAbilityParamsBasedOnClass(antipaladin_class, StatType.Charisma),
                                                resource.CreateResourceLogic(amount: 2),
                                                Helpers.Create<NewMechanics.AbilityCasterHasResource>(ab => { ab.resource = touch_of_corruption_resource; ab.amount = 2; }),
                                                Common.createAbilityAoERadius(20.Feet(), TargetType.Enemy)
                                                );
            ability.setMiscAbilityParametersRangedDirectional();

            dark_emissary[0].AddComponents(Helpers.CreateAddFact(ability),
                                           resource.CreateAddAbilityResource(),
                                           Helpers.Create<ResourceMechanics.ConnectResource>(c => { c.base_resource = resource; c.connected_resources = new BlueprintAbilityResource[] { touch_of_corruption_resource }; })
                                           );
        }


        static void createBeaconOfEvil()
        {
            var stats_buff = Helpers.CreateBuff("AuraOfBeaconStatsBuff",
                                                "Beacon of Evil",
                                                "At 4th level and every 4 level thereafter, a dread vanguard gains one additional use of his touch of corruption ability per day. As a standard action, he can spend a use of his touch of corruption ability to manifest the darkness in his soul as an area of flickering shadows with a 30-foot radius centered on him. These shadows don’t affect visibility. The antipaladin and all allies in the area gain a +1 profane bonus to AC and on attack rolls, damage rolls, and saving throws against fear. This lasts for 1 minute, as long as the dread vanguard is conscious.\n"
                                                + "At 8th level, the aura grants fast healing 3 to the dread vanguard as well as to his allies while they remain within it. Additionally, while this aura is active, the antipaladin can use his touch of corruption ability against any targets within its radius by making a ranged touch attack.\n"
                                                + "At 12th level the profane bonus granted to AC and on attack rolls, damage rolls, and saving throws against fear increases to +2.\n"
                                                + "At 16th level, the fast healing granted by this ability increases to 5. Additionally, the antipaladin’s weapons and those of his allies within the aura’s radius are considered evil for the purpose of overcoming damage reduction.\n"
                                                + "At 20th level, the profane bonus granted to AC and on attack rolls, damage rolls, and saving throws against fear increases to +4. Lastly, attacks made by the dread vanguard and his allies within the aura’s radius are infused with pure unholy power, and deal an additional 1d6 points of damage.",
                                                "",
                                                NewSpells.rebuke.Icon,
                                                null,
                                                Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Profane),
                                                Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.Profane),
                                                Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.Profane),
                                                Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.Profane, SpellDescriptor.Shaken | SpellDescriptor.Fear),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray(),
                                                                                progression: ContextRankProgression.Custom, customProgression: new (int, int)[] {(11, 1), (19, 2), (20, 4) })
                                                );

            var buff8 = Helpers.CreateBuff("AuraOfBeacon8Buff",
                                           "",
                                           "",
                                           "",
                                           null,
                                           null,
                                           Common.createAddContextEffectFastHealing(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray(),
                                                                                                                    progression: ContextRankProgression.Custom,
                                                                                                                    customProgression: new (int, int)[] { (15, 3), (20, 5) })
                                           );
            buff8.SetBuffFlags(BuffFlags.HiddenInUi);
            var buff16 = Helpers.CreateBuff("AuraOfBeacon16Buff",
                                               "",
                                               "",
                                               "",
                                               null,
                                               null,
                                               Common.createAddOutgoingAlignment(DamageAlignment.Evil)
                                               );
            buff16.SetBuffFlags(BuffFlags.HiddenInUi);
            var buff20 = Helpers.CreateBuff("AuraOfBeacon20Buff",
                                           "",
                                           "",
                                           "",
                                           null,
                                           null,
                                           Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(DiceType.D6, 1, 0), IgnoreCritical: true)))
                                           );
            buff20.SetBuffFlags(BuffFlags.HiddenInUi);

            beacon_of_evil = new BlueprintFeature[5];
            for (int i = 0; i < beacon_of_evil.Length; i++)
            {
                beacon_of_evil[i] = Helpers.CreateFeature($"BeaconOfEvil{4 * (i + 1)}Feature",
                                                          stats_buff.Name,
                                                          stats_buff.Description,
                                                          "",
                                                          stats_buff.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateIncreaseResourceAmount(touch_of_corruption_resource, 1)
                                                          );
            }

            Common.addContextActionApplyBuffOnCasterFactsToActivatedAbilityBuffNoRemove(stats_buff, buff8, beacon_of_evil[1]);
            Common.addContextActionApplyBuffOnCasterFactsToActivatedAbilityBuffNoRemove(stats_buff, buff16, beacon_of_evil[3]);
            Common.addContextActionApplyBuffOnCasterFactsToActivatedAbilityBuffNoRemove(stats_buff, buff20, beacon_of_evil[4]);

            var reach_touch_of_corruption_buff = Helpers.CreateBuff("TouchOfCorruptionReachBuff",
                                                                    "Ranged Touch of Corruption",
                                                                    "You can use touch of corruption as a ranged touch attack within the area of Beacon of Evil.",
                                                                    "",
                                                                    Helpers.GetIcon("450af0402422b0b4980d9c2175869612"), //ray of enfeeblement
                                                                    null,
                                                                    Common.autoMetamagicOnAbilities(Metamagic.Reach, touch_of_corruption_base.Variants.AddToArray(touch_of_corruption_base.Variants.Select(v => v.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility)))
                                                                    );

            var reach_touch_of_corruption_toggle = Common.buffToToggle(reach_touch_of_corruption_buff, CommandType.Free, true,
                                                                       Common.createActivatableAbilityRestrictionHasFact(buff8));

            foreach (var a in ranged_channel_wrapper.Variants)
            {
                a.AddComponent(Common.createAbilityCasterHasFacts(buff8));
            }

            beacon_of_evil[1].AddComponent(Helpers.CreateAddFacts(reach_touch_of_corruption_toggle, ranged_channel_wrapper));

            var aura_buff = Common.createAuraEffectBuff(stats_buff, 30.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()));
            aura_buff.SetNameDescriptionIcon(stats_buff);
            aura_buff.SetBuffFlags(0);
            var area = aura_buff.GetComponent<AuraFeatureComponent>().Buff.GetComponent<AddAreaEffect>().AreaEffect.Fx = Common.createPrefabLink("dfadb7fa26de0384d9d9a6dabb0bea72"/*"79cd602c3311fda459f1e7c62d7ec9a1"*/);
            var ability = Helpers.CreateAbility("BeaconOfEvilAbility",
                                                stats_buff.Name,
                                                stats_buff.Description,
                                                "",
                                                stats_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(aura_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)),
                                                touch_of_corruption_resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            beacon_of_evil[0].AddComponent(Helpers.CreateAddFact(ability));

        }


        static void createIronTyrant()
        {
            iron_tyrant = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "IronTyrantArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Iron Tyrant");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Covered from head to toe in blackened armor decorated with grotesque shapes and bristling with spikes, iron tyrants make an unmistakable impression on the battlefield. These antipaladins’ armor is an outward symbol of their inner power, and they are rarely seen without it. Iron tyrants seek the strength to rule over domains as unfettered despots, and depend on their armor as protection against those they have not yet cowed.");
            });
            Helpers.SetField(iron_tyrant, "m_ParentClass", antipaladin_class);
            library.AddAsset(iron_tyrant, "");

            createIronFist();
            createIronTyrantBonusFeats();
            createUnstoppable();
            createFiendishBond();

            iron_tyrant.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(2, touch_of_corruption),
                                                          Helpers.LevelEntry(3, cruelty),
                                                          Helpers.LevelEntry(4, channel_negative_energy),
                                                          Helpers.LevelEntry(5, fiendish_boon[0]),
                                                          Helpers.LevelEntry(6, cruelty),
                                                          Helpers.LevelEntry(8, fiendish_boon[1]),
                                                          Helpers.LevelEntry(9, cruelty),
                                                          Helpers.LevelEntry(11, fiendish_boon[2]),
                                                          Helpers.LevelEntry(12, cruelty),
                                                          Helpers.LevelEntry(14, fiendish_boon[3]),
                                                          Helpers.LevelEntry(15, cruelty),
                                                          Helpers.LevelEntry(17, fiendish_boon[4]),
                                                          Helpers.LevelEntry(18, cruelty),
                                                          Helpers.LevelEntry(20, fiendish_boon[5]),
                                                         };

            iron_tyrant.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(2, iron_fist),
                                                       Helpers.LevelEntry(3, iron_tyrant_bonus_feat),
                                                       Helpers.LevelEntry(4, unstoppable),
                                                       Helpers.LevelEntry(5, fiendish_bond[0]),
                                                       Helpers.LevelEntry(6, iron_tyrant_bonus_feat),
                                                       Helpers.LevelEntry(8, fiendish_bond[1]),
                                                       Helpers.LevelEntry(9, iron_tyrant_bonus_feat),
                                                       Helpers.LevelEntry(11, fiendish_bond[2]),
                                                       Helpers.LevelEntry(12, iron_tyrant_bonus_feat),
                                                       Helpers.LevelEntry(14, fiendish_bond[3]),
                                                       Helpers.LevelEntry(15, iron_tyrant_bonus_feat),
                                                       Helpers.LevelEntry(17, fiendish_bond[4]),
                                                       Helpers.LevelEntry(18, iron_tyrant_bonus_feat),
                                                       Helpers.LevelEntry(20, fiendish_bond[5]),
                                                      };

            antipaladin_progression.UIGroups[3].Features.Add(iron_fist);
            antipaladin_progression.UIGroups[3].Features.Add(unstoppable);
            foreach (var fb in fiendish_bond)
            {
                antipaladin_progression.UIGroups[3].Features.Add(fb);
            }
            antipaladin_progression.UIGroups = antipaladin_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(iron_tyrant_bonus_feat));
            antipaladin_progression.UIDeterminatorsGroup = antipaladin_progression.UIDeterminatorsGroup.AddToArray(invocation);
        }


        static void createFiendishBond()
        {
            var mage_armor = library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568");
            var enchants = ArmorEnchantments.temporary_armor_enchantments;
            var enhancement_buff = Helpers.CreateBuff("IronTyrantArmorEnchancementBaseBuff",
                                            "",
                                            "",
                                            "",
                                            null,
                                            null,
                                            Common.createBuffRemainingGroupSizetEnchantArmor(ActivatableAbilityGroup.DivineWeaponProperty,
                                                                                            false, true, enchants
                                                                                            )
                                            );
            enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var fiendish_bond_enhancement_buff = Helpers.CreateBuff("IronTyrantArmorEnhancementSwitchBuff",
                                                                    "Fiendish Bond",
                                                                    "At 5th level, instead of forming a fiendish bond with his weapon, an iron tyrant can form a bond with his armor. As a standard action, an iron tyrant can enhance his armor by calling upon a fiendish spirit’s aid. This bond lasts for 1 minute per antipaladin level.\n"
                                                                    + "At 5th level, the spirit grants the armor a +1 enhancement bonus. For every 3 antipaladin levels beyond 5th, the armor gains another +1 enhancement bonus, to a maximum of +6 at 20th level.\n"
                                                                    + "These bonuses stack with existing armor enhancement bonuses to a maximum of +5, or they can be used to add any of the following armor special abilities: fortification (heavy, light, or medium) or spell resistance (13, 17, 21, and 25). Adding these special abilities consumes an amount of bonus equal to the special ability’s base price modifier.",
                                                                    "",
                                                                    mage_armor.Icon,
                                                                    null,
                                                                    Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, dispellable: false, is_permanent: true)
                                                                                                        )
                                                                    );
            //fortification - light (25% +1), medium(50% +3), heavy (75% +5)
            //spell resistance (+13 +2), (+17 +3), (+21 +4), (+25, +5)

            var sr_icon = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889").Icon;
            var fortification_icon = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b").Icon; //stoneskin

            List<BlueprintActivatableAbility>[] enchant_abilities = new List<BlueprintActivatableAbility>[6];

            for (int i = 0; i < enchant_abilities.Length; i++)
            {
                enchant_abilities[i] = new List<BlueprintActivatableAbility>();
            }

            foreach (var e in ArmorEnchantments.spell_resistance_enchantments)
            {
                int cost = e.EnchantmentCost;
                var a = Common.createEnchantmentAbility("IrontTyrantFiendishBond" + e.name, "Fiendish Bond - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of armor enhancement bonus.", sr_icon, fiendish_bond_enhancement_buff, e, cost, ActivatableAbilityGroup.DivineWeaponProperty);
                enchant_abilities[cost - 1].Add(a);
            }


            foreach (var e in ArmorEnchantments.fortification_enchantments)
            {
                int cost = e.EnchantmentCost;
                var a = Common.createEnchantmentAbility("IrontTyrantFiendishBond" + e.name, "Fiendish Bond - " + e.Name, e.Description + $"\nThis consumes {cost} point(s) of armor enhancement bonus.", fortification_icon, fiendish_bond_enhancement_buff, e, cost, ActivatableAbilityGroup.DivineWeaponProperty);
                enchant_abilities[cost - 1].Add(a);
            }

            var apply_buff = Common.createContextActionApplyBuff(fiendish_bond_enhancement_buff,
                                                                 Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes),
                                                                 dispellable: false);
            var ability = Helpers.CreateAbility("IronTyrantFiendishBondAbility",
                                                fiendish_bond_enhancement_buff.Name,
                                                fiendish_bond_enhancement_buff.Description,
                                                "",
                                                fiendish_bond_enhancement_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.minutesPerLevelDuration,
                                                Helpers.savingThrowNone,
                                                mage_armor.GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray()),
                                                Helpers.CreateResourceLogic(fiendish_boon_resource),
                                                Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>(),
                                                Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            fiendish_bond = new BlueprintFeature[6];
            for (int i = 0; i < fiendish_bond.Length; i++)
            {
                fiendish_bond[i] = Helpers.CreateFeature($"IronTyrantFiendishBond{i+1}Feature",
                                                        $"Fiendish Bond +{i+1}",
                                                        ability.Description,
                                                        "",
                                                        ability.Icon,
                                                        FeatureGroup.None
                                                        );
                if (i == 0)
                {
                    fiendish_bond[i].AddComponent(fiendish_boon_resource.CreateAddAbilityResource());
                    fiendish_bond[i].AddComponent(Helpers.CreateAddFact(ability));
                }
                if (i > 0)
                {
                    fiendish_bond[i].AddComponent(Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty));
                }
                if (enchant_abilities[i].Count > 0)
                {
                    fiendish_bond[i].AddComponent(Helpers.CreateAddFacts(enchant_abilities[i].ToArray()));
                }
            }
        }


        static void createIronTyrantBonusFeats()
        {
            iron_tyrant_bonus_feat = library.CopyAndAdd<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f", "IronTyrantBonusFeat", "");
            iron_tyrant_bonus_feat.SetNameDescription("Bonus Feat",
                                                      "At 3rd level and every 3 antipaladin levels thereafter, an iron tyrant gains a bonus feat in addition to those gained from normal advancement. This feat must be a combat feat that relates to the iron tyrant’s armor or shield, such as Shield Focus, or Armor Focus.");
            iron_tyrant_bonus_feat.AllFeatures = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("ac57069b6bf8c904086171683992a92a"), //shield focus
                library.Get<BlueprintFeature>("afd05ca5363036c44817c071189b67e1"), //greater shield focus
                library.Get<BlueprintFeature>("76d4885a395976547a13c5d6bf95b482"), //armor focus
                library.Get<BlueprintFeature>("121811173a614534e8720d7550aae253"), //shield bash
                library.Get<BlueprintFeature>("ac8aaf29054f5b74eb18f2af950e752d"), //twf
                library.Get<BlueprintFeature>("dbec636d84482944f87435bd31522fcc"), //shield master
                library.Get<BlueprintFeature>("0b442a7b4aa598d4e912a4ecee0500ff"), //bashing finish
                library.Get<BlueprintFeature>("8976de442862f82488a4b138a0a89907"), //shield wall
                library.Get<BlueprintFeature>("6105f450bb2acbd458d277e71e19d835"), //tower shield proficiency
            };
            //add itwf and gtwf only if blance fixes are not enabled
            if (!Main.settings.balance_fixes)
            {
                iron_tyrant_bonus_feat.AllFeatures = iron_tyrant_bonus_feat.AllFeatures.AddToArray(library.Get<BlueprintFeature>("9af88f3ed8a017b45a6837eab7437629"), library.Get<BlueprintFeature>("c126adbdf6ddd8245bda33694cd774e8"));
            }
            //other feats will be added upon creation
        }


        static void createIronFist()
        {
            DiceFormula[] diceFormulas = new DiceFormula[] {new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8)};

            iron_fist = Helpers.CreateFeature("IronFistFeature",
                                              "Iron Fist",
                                              "At 2nd level, an iron tyrant gains Improved Unarmed Strike as a bonus feat. In addition, whenever the iron tyrant makes a successful attack with a weapon from fighter close weapons group, the weapon damage is based on his level and not the weapon type, as per the warpriest’s sacred weapon ability.",
                                              "",
                                              Helpers.GetIcon("85067a04a97416949b5d1dbf986d93f3"), //stone fist
                                              FeatureGroup.None,
                                              Helpers.CreateAddFact(library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167")),
                                              Helpers.Create<NewMechanics.ContextWeaponDamageDiceReplacementWeaponCategory>(c =>
                                              {
                                                  c.dice_formulas = diceFormulas;
                                                  c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                  c.categories = new WeaponCategory[] {WeaponCategory.SpikedHeavyShield, WeaponCategory.SpikedLightShield,
                                                                                       WeaponCategory.WeaponLightShield, WeaponCategory.WeaponHeavyShield,
                                                                                       WeaponCategory.UnarmedStrike, WeaponCategory.PunchingDagger};
                                              }),
                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                type: AbilityRankType.Default,
                                                                                progression: ContextRankProgression.DivStep,
                                                                                stepLevel: 5,
                                                                                classes: getAntipaladinArray())
                                              );
        }

        static void createUnstoppable()
        {
            unstoppable = library.CopyAndAdd<BlueprintFeature>("11f4072ea766a5840a46e6660894527d", "UnstoppableIronTyrantFeature", "");
            unstoppable.SetName("Unstoppable");
        }


        static void createBlightedMyrmidon()
        {
            blighted_myrmidon = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "BlightedMyrmidonArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Blighted Myrmidon");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Blighted myrmidons carry the seed of rot in their black hearts and sap life from the natural world.");
            });
            Helpers.SetField(blighted_myrmidon, "m_ParentClass", antipaladin_class);
            library.AddAsset(blighted_myrmidon, "");

            createSmiteNature();
            createAuraOfDecay();


            blighted_myrmidon.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, smite_good),
                                                          Helpers.LevelEntry(4, smite_good_extra_use),
                                                          Helpers.LevelEntry(7, smite_good_extra_use),
                                                          Helpers.LevelEntry(10, smite_good_extra_use),
                                                          Helpers.LevelEntry(11, aura_of_vengeance),
                                                          Helpers.LevelEntry(13, smite_good_extra_use),
                                                          Helpers.LevelEntry(16, smite_good_extra_use),
                                                          Helpers.LevelEntry(19, smite_good_extra_use)
                                                         };

            blighted_myrmidon.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, smite_nature),
                                                              Helpers.LevelEntry(4, smite_nature_extra_use),
                                                              Helpers.LevelEntry(7, smite_nature_extra_use),
                                                              Helpers.LevelEntry(11, aura_of_decay),
                                                              Helpers.LevelEntry(13, smite_nature_extra_use),
                                                              Helpers.LevelEntry(16, smite_nature_extra_use),
                                                              Helpers.LevelEntry(19, smite_nature_extra_use),
                                                              };

            antipaladin_progression.UIGroups[1].Features.Add(smite_nature);
            antipaladin_progression.UIGroups[1].Features.Add(smite_nature_extra_use);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_decay);
        }


        static void createAuraOfDecay()
        {
            var deal_dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Unholy, Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilitySharedValue.Damage)), false, true);
            var effect = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionHasFactsOrClassLevelsUnlessCasterHasFact>(c =>
                                                    {
                                                        c.facts = new BlueprintUnitFact[] { Common.elemental, Common.fey };
                                                        c.classes = new BlueprintCharacterClass[] {library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96"), //druid
                                                                                                                                    library.Get<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6"), //ranger
                                                                                                                                    Hunter.hunter_class
                                                                                                                                };
                                                        c.caster_fact = null;
                                                    }),
                                                    Helpers.CreateConditionalSaved(Common.createContextActionOnContextCaster(Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilitySharedValue.StatBonus)))),
                                                                                   Common.createContextActionOnContextCaster(Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilitySharedValue.Heal))))
                                                                                   )
                                                    );
            var effect_actions = new GameAction[]{Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, new GameAction[]{deal_dmg, effect }),
                                                  
                                                  };
            var effect_buff = Helpers.CreateBuff("AuraOfDecayBuff",
                                      "Aura of Decay Target",
                                      $"At 11th level, as a free action, a blighted myrmidon can expend two uses of her smite nature ability to generate an aura of decay with a range of 10 feet for 1 minute. Living foes of the blighted myrmidon within the aura take 3d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage unless they succeed at a Fortitude save (DC = half the blighted myrmidon’s level + her Charisma modifier) for half damage. If an elemental, a fey, or a creature with levels in druid, hunter, or ranger takes damage from aura of decay, the blighted myrmidon regains a number of hit points equal to half the amount of damage the creature takes.",
                                      "",
                                      NewSpells.explosion_of_rot.Icon,
                                      Common.createPrefabLink("fbf39991ad3f5ef4cb81868bb9419bff"), //poison buff
                                      Helpers.CreateAddFactContextActions(activated: effect_actions, newRound: effect_actions),
                                      Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 3, 0), AbilitySharedValue.Damage),
                                      Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilitySharedValue.Damage)), AbilitySharedValue.Heal, 0.5),
                                      Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilitySharedValue.Damage)), AbilitySharedValue.StatBonus, 0.25),
                                      Common.createContextCalculateAbilityParamsBasedOnClass(antipaladin_class, StatType.Charisma)
                                     );

            var aura_buff = Common.createBuffAreaEffect(effect_buff, 13.Feet(),
                                                        Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>(),
                                                                                            Common.createContextConditionHasFact(Common.undead, false),
                                                                                            Common.createContextConditionHasFact(Common.construct, false)
                                                                                            )
                                                        );

            aura_buff.SetBuffFlags(0);
            aura_buff.SetNameDescriptionIcon("Aura of Decay", effect_buff.Description, effect_buff.Icon);

            var ability = Helpers.CreateAbility("AuraOfDecayAbility",
                                                aura_buff.Name,
                                                aura_buff.Description,
                                                "",
                                                aura_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Free,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "Fortitude partial",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(aura_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)),
                                                Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                smite_resource.CreateResourceLogic(amount: 2)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            aura_of_decay = Common.AbilityToFeature(ability, false);
        }


        static void createSmiteNature()
        {
            smite_nature = Common.createSmite("AntipaladinSmiteNature",
                                                "Smite Nature",
                                                "A blighted myrmidon can drain the life from a creature tied to nature. As a swift action, the blighted myrmidon chooses one target within sight to smite. Regardless of its alignment, if the target is an animal, elemental, fey, plant, or vermin, or has levels of druid, ranger or hunter, the blighted myrmidon adds her Charisma bonus (if any) on her attack rolls and adds her blighted myrmidon level on damage rolls against the target of her smite. This ability otherwise functions as smite good.",
                                                "",
                                                "",
                                                LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteNature.png"),
                                                getAntipaladinArray(),
                                                Helpers.Create<NewMechanics.ContextConditionHasFactsOrClassLevelsUnlessCasterHasFact>(c => 
                                                {
                                                    c.facts = new BlueprintUnitFact[] { Common.animal, Common.elemental, Common.fey, Common.plant, Common.vermin };
                                                    c.classes = new BlueprintCharacterClass[] {library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96"), //druid
                                                                                                library.Get<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6"), //ranger
                                                                                                Hunter.hunter_class
                                                                                            };
                                                    c.caster_fact = tip_of_spear;
                                                })
                                                );

            var smite_nature_ability = smite_nature.GetComponent<AddFacts>().Facts[0] as BlueprintAbility;
            smite_nature_ability.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));

            smite_nature_extra_use = library.CopyAndAdd<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137", "SmiteNatureAdditionalUse", "");
            smite_nature_extra_use.SetNameDescriptionIcon("Smite Nature — Additional Use",
                                              smite_nature.Description,
                                              smite_nature.Icon);
        }


        static void createInsinuator()
        {
            insinuator = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "InsinuatorArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Insinuator");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Between the selfless nobility of paladins and the chaotic menace of antipaladins, there exists a path of dedicated self-interest.\nShunning the ties that bind them to a single deity, insinuators embrace whatever forces help them achieve their own agenda and glory, borrowing power to emulate divine warriors.");
            });
            Helpers.SetField(insinuator, "m_ParentClass", antipaladin_class);
            library.AddAsset(insinuator, "");

            createInvocation(); //also aura of belief, aura of indomitobility, aura of ego, aura of ambition and personal champion (dr part)
            createSmiteImpudence(); //also personal champion smite part and aura of glory
            createSelfishHealing(); //and greeds
            createInsinuatorChannelEnergy(); 
            createBonusFeats();
            //ambitious bond(?)

            insinuator.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, smite_good, antipaladin_deity),
                                                          Helpers.LevelEntry(2, touch_of_corruption),
                                                          Helpers.LevelEntry(3, cruelty, aura_of_cowardice),
                                                          Helpers.LevelEntry(4, smite_good_extra_use, channel_negative_energy),
                                                          Helpers.LevelEntry(6, cruelty),
                                                          Helpers.LevelEntry(7, smite_good_extra_use),
                                                          Helpers.LevelEntry(8, aura_of_despair),
                                                          Helpers.LevelEntry(9, cruelty),
                                                          Helpers.LevelEntry(10, smite_good_extra_use),
                                                          Helpers.LevelEntry(11, aura_of_vengeance),
                                                          Helpers.LevelEntry(12, cruelty),
                                                          Helpers.LevelEntry(13, smite_good_extra_use),
                                                          Helpers.LevelEntry(14, aura_of_sin),
                                                          Helpers.LevelEntry(15, cruelty),
                                                          Helpers.LevelEntry(16, smite_good_extra_use),
                                                          Helpers.LevelEntry(17, aura_of_deparvity),
                                                          Helpers.LevelEntry(18, cruelty),
                                                          Helpers.LevelEntry(19, smite_good_extra_use),
                                                          Helpers.LevelEntry(20, tip_of_spear)
                                                         };

            insinuator.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, invocation, smite_impudence),
                                                       Helpers.LevelEntry(2, selfish_healing),
                                                       Helpers.LevelEntry(3, greeds, aura_of_ego),
                                                       Helpers.LevelEntry(4, insinuator_channel_energy, insinuator_bonus_feat, smite_impudence_extra_use),
                                                       Helpers.LevelEntry(6, greeds),
                                                       Helpers.LevelEntry(7, insinuator_bonus_feat, smite_impudence_extra_use),
                                                       Helpers.LevelEntry(8, aura_of_ambition),
                                                       Helpers.LevelEntry(9, greeds),
                                                       Helpers.LevelEntry(10, insinuator_bonus_feat, smite_impudence_extra_use),
                                                       Helpers.LevelEntry(11, aura_of_glory),
                                                       Helpers.LevelEntry(12, greeds),
                                                       Helpers.LevelEntry(13, insinuator_bonus_feat, smite_impudence_extra_use),
                                                       Helpers.LevelEntry(14, aura_of_belief),
                                                       Helpers.LevelEntry(15, greeds),
                                                       Helpers.LevelEntry(16, insinuator_bonus_feat, smite_impudence_extra_use),
                                                       Helpers.LevelEntry(17, aura_of_indomitability),
                                                       Helpers.LevelEntry(18, greeds),
                                                       Helpers.LevelEntry(19, insinuator_bonus_feat,smite_impudence_extra_use),
                                                       Helpers.LevelEntry(20, personal_champion),
                                                      };

            //antipaladin_progression.UIGroups[0].Features.Add(stubborn_health);
            antipaladin_progression.UIGroups[1].Features.Add(smite_impudence);
            antipaladin_progression.UIGroups[1].Features.Add(smite_impudence_extra_use);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_ego);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_ambition);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_glory);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_belief);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_indomitability);
            antipaladin_progression.UIGroups[2].Features.Add(personal_champion);
            antipaladin_progression.UIGroups[3].Features.Add(selfish_healing);
            antipaladin_progression.UIGroups[3].Features.Add(greeds);
            antipaladin_progression.UIGroups[3].Features.Add(insinuator_channel_energy);
            antipaladin_progression.UIGroups = antipaladin_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(insinuator_bonus_feat));
            antipaladin_progression.UIDeterminatorsGroup = antipaladin_progression.UIDeterminatorsGroup.AddToArray(invocation);

            insinuator.RemoveSpellbook = true;
            insinuator.ChangeCasterType = true;
        }

        static void createBonusFeats()
        {
            insinuator_bonus_feat = library.CopyAndAdd<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f", "InsinuatorBonusFeat", ""); //combat feat
            insinuator_bonus_feat.AllFeatures = insinuator_bonus_feat.AllFeatures.AddToArray(library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a"));
            insinuator_bonus_feat.SetNameDescription("Bonus Feat",
                                                     "At 4th level, an insinuator gains one bonus feat, which must be selected from the list of combat feats or Skill Focus. At 7th level and every 3 antipaladin levels thereafter, the insinuator gains one additional combat or Skill Focus feat.");
        }

        static void createInsinuatorChannelEnergy()
        {
            var positive_energy_feature = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var context_rank_config = Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray(), progression: ContextRankProgression.StartPlusDivStep, stepLevel: 4, startLevel: 2);
            var dc_scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(getAntipaladinArray(), StatType.Charisma);
            channel_positive_energy = Helpers.CreateFeature("AntipaladinChannelPositiveEnergyFeature",
                                                            "Channel Positive Energy",
                                                            "When an insinuator reaches 4th level, he gains the supernatural ability to channel positive energy like a cleric if she invokes a neutral outsider. Using this ability consumes two uses of his touch of corruption ability. An Insinuator uses half his level as his effective cleric level when channeling positive energy. This is a Charisma-based ability.",
                                                            "",
                                                            positive_energy_feature.Icon,
                                                            FeatureGroup.None);

            var heal_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                                      "AntipaladinChannelEnergyHealLiving",
                                                                      "",
                                                                      $"Channeling positive energy causes a burst that heals all living creatures in a 30-foot radius centered on the insinuator. The amount of damage healed is equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage plus 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage for every four insinuator levels beyond 2nd (2d{BalanceFixes.getDamageDieString(DiceType.D6)}  at 6th, 3d{BalanceFixes.getDamageDieString(DiceType.D6)}  at 10th, and so on).",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(touch_of_corruption_resource, amount: 2));
            var harm_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                                                        "AntipaladinChannelEnergyHarmUndead",
                                                                        "",
                                                                        $"Channeling positive energy causes a burst that damages all undead creatures in a 30-foot radius centered on the insinuator. The amount of damage inflicted is equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage plus 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage for every four insinuator levels beyond 2nd (2d{BalanceFixes.getDamageDieString(DiceType.D6)}  at 6th, 3d{BalanceFixes.getDamageDieString(DiceType.D6)}  at 10th, and so on). Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the insinuator's level + the insinuator's Charisma modifier.",
                                                                        "",
                                                                        context_rank_config,
                                                                        dc_scaling,
                                                                        Helpers.CreateResourceLogic(touch_of_corruption_resource, amount: 2));

            var heal_living_base = Common.createVariantWrapper("AntipaladinPositiveHealBase", "", heal_living);
            var harm_undead_base = Common.createVariantWrapper("AntipaladinPositiveHarmBase", "", harm_undead);
            heal_living.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_living.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInsinuatorAlignmentNeutral>());
            harm_undead.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            harm_undead.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInsinuatorAlignmentNeutral>());

            ChannelEnergyEngine.storeChannel(heal_living, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);

            channel_positive_energy.AddComponent(Helpers.CreateAddFacts(heal_living_base, harm_undead_base));

            var heal_living_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                          "AntipaladinChannelEnergyHealLivingExtra",
                                                          heal_living.Name + " (Extra)",
                                                          heal_living.Description,
                                                          "",
                                                          heal_living.GetComponent<ContextRankConfig>(),
                                                          heal_living.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                                          Helpers.CreateResourceLogic(extra_channel_resource, true, 1));

            var harm_undead_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                              "AntipaladinChannelEnergyHarmUndeadExtra",
                                              harm_undead.Name + " (Extra)",
                                              harm_undead.Description,
                                              "",
                                              harm_undead.GetComponent<ContextRankConfig>(),
                                              harm_undead.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                              Helpers.CreateResourceLogic(extra_channel_resource, true, 1));


            heal_living_extra.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_living_extra.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInsinuatorAlignmentNeutral>());
            harm_undead_extra.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            harm_undead_extra.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInsinuatorAlignmentNeutral>());

            heal_living_base.addToAbilityVariants(heal_living_extra);
            harm_undead_base.addToAbilityVariants(harm_undead_extra);
            ChannelEnergyEngine.storeChannel(heal_living_extra, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead_extra, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);


            insinuator_channel_energy = Helpers.CreateFeature("InsinuatorChannelEnergy",
                                                              "Channel Energy",
                                                              "At 4th level, an insinuator can channel negative energy, treating his antipaladin level as his effective cleric level. If he invokes a neutral outsider for the day, he may instead chose to channel positive energy, but treats his effective cleric level as half his antipaladin level. Using this ability consumes two uses of his selfish healing ability. This is a Charisma-based ability.",
                                                              "",
                                                              LoadIcons.Image2Sprite.Create(@"AbilityIcons/ChannelEnergy.png"),
                                                              FeatureGroup.None,
                                                              Helpers.CreateAddFacts(channel_negative_energy, channel_positive_energy)
                                                              );
        }


        static void createSelfishHealing()
        {
            var ability = library.CopyAndAdd<BlueprintAbility>("8d6073201e5395d458b8251386d72df1", "SelfishHealingAbility", "");
            ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getAntipaladinArray()));
            ability.SetNameDescription("Selfish Healing",
                                       "Beginning at 2nd level, an insinuator can heal his wounds by touch. This is treated exactly like the paladin’s lay on hands class feature, except it can be used only to heal the insinuator and cannot be used on other creatures."
                                       );

            ability.ReplaceComponent<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil);
            ability.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInvokedOutsider>());
            selfish_healing = Common.AbilityToFeature(ability, false);
            selfish_healing.AddComponent(touch_of_corruption_resource.CreateAddAbilityResource());

            greeds = library.CopyAndAdd<BlueprintFeatureSelection>("02b187038a8dce545bb34bbfb346428d", "GreedsSelection", "");
            greeds.SetNameDescription("Greeds",
                                      "Beginning at 3rd level, an insinuator can heal himself of certain conditions. This functions as the mercy paladin class ability, but these mercies can only be applied to the insinuator himself."
                                      );
            foreach (var f in greeds.AllFeatures)
            {
                var prereq = f.GetComponent<PrerequisiteClassLevel>();
                if (prereq == null)
                {
                    continue;
                }
                prereq.Group = Prerequisite.GroupType.Any;
                f.AddComponent(Common.createPrerequisiteArchetypeLevel(insinuator, prereq.Level, any: true));
            }
        }



        static void createSmiteImpudence()
        {
            var smite_target_buff = library.CopyAndAdd<BlueprintBuff>("b6570b8cbb32eaf4ca8255d0ec3310b0", "SmiteImpudenceBuff", "");
            smite_target_buff.RemoveComponents<ACBonusAgainstTarget>();
            var smite_target_buff2 = library.CopyAndAdd<BlueprintBuff>(smite_target_buff, "SmiteImpudence2Buff", "");
            smite_target_buff2.SetNameDescriptionIcon(personal_champion);

            var aura_of_glory_buff = library.CopyAndAdd<BlueprintBuff>("ac3c66782859eb84692a8782320ffd2c", "AuaOfGloryBuff", "");
            aura_of_glory_buff.RemoveComponents<ACBonusAgainstTarget>();
            var aura_of_glory_buff2 = library.CopyAndAdd<BlueprintBuff>(aura_of_glory_buff, "AuraOfGlory2Buff", "");
            aura_of_glory_buff2.SetNameDescriptionIcon(personal_champion);

            var hp_buff = Helpers.CreateBuff("SmiteImpudenceHpBuff",
                                                "",
                                                "",
                                                "",
                                                null,
                                                null,
                                                Helpers.Create<TemporaryHitPointsFromAbilityValue>(t => 
                                                { t.Descriptor = ModifierDescriptor.UntypedStackable;
                                                  t.RemoveWhenHitPointsEnd = true;
                                                  t.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                }),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray())
                                                );

            hp_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var smite_impudence_ability = library.CopyAndAdd<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec", "SmiteImpudenceAbility", "");
            smite_impudence_ability.ReplaceComponent<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil);
            smite_impudence_ability.ReplaceComponent<AbilityEffectRunAction>(a =>
            {
                a.Actions = Helpers.CreateActionList(Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(smite_target_buff, false),
                                                                                                Helpers.Create<InsinuatorMechanics.ContextConditionNonOutsiderAlignment>()
                                                                                               },
                                                                               new GameAction[] { Common.createContextActionApplyBuff(smite_target_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                                                                  Common.createContextActionApplyBuffToCaster(hp_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                                                                  Helpers.CreateConditional(Common.createContextConditionCasterHasFact(personal_champion),
                                                                                                                            Common.createContextActionApplyBuff(smite_target_buff2, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true) )
                                                                                                }
                                                                               )
                                                     );
            }
            );
            smite_impudence_ability.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInvokedOutsider>());
            smite_impudence_ability.SetNameDescriptionIcon("Smite Impudence",
                                                   "Once per day, an insinuator can beseech the forces empowering him to punish their shared enemies. As a swift action, the insinuator chooses one target within sight to smite.\n"
                                                   + "An insinuator cannot use smite against a target that shares an alignment with the outsider he has invoked for the day. The insinuator adds his Charisma bonus on his attack rolls and half his insinuator level on all damage rolls made against the target of his smite.\n"
                                                   + "Regardless of the target, the smite attack automatically bypasses any damage reduction the creature might possess. In addition, each time the insinuator declares a smite, he gains a number of temporary hit points equal to his antipaladin level.\n"
                                                   + "The smite effect remains until the target is defeated or the next time the insinuator rests and regains his uses of this ability.\n"
                                                   + "At 4th level and at every 3 levels thereafter, the insinuator can use smite one additional time per day, to a maximum of seven times per day at 19th level.",
                                                   LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteImpudence.png")
                                                   );

            var config_dmg = smite_impudence_ability.GetComponents<ContextRankConfig>().Where(c => c.IsBasedOnClassLevel).FirstOrDefault();
            smite_impudence_ability.ReplaceComponent(config_dmg, config_dmg.CreateCopy(c => { Helpers.SetField(c, "m_Progression", ContextRankProgression.Div2); Helpers.SetField(c, "m_Class", getAntipaladinArray()); }));


            smite_impudence = Common.AbilityToFeature(smite_impudence_ability, false);
            smite_impudence.AddComponent(smite_resource.CreateAddAbilityResource());

            smite_impudence_extra_use = library.CopyAndAdd<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137", "SmiteImpudenceAdditionalUse", "");
            smite_impudence_extra_use.SetNameDescriptionIcon("Smite Impudence — Additional Use", smite_impudence.Description, smite_impudence.Icon);




            var aura_of_glory_ability = library.CopyAndAdd<BlueprintAbility>("7a4f0c48829952e47bb1fd1e4e9da83a", "AuraOfGloryAbility", "");
            aura_of_glory_ability.ReplaceComponent<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil);
            aura_of_glory_ability.ReplaceComponent<AbilityEffectRunAction>(a =>
            {
                a.Actions = Helpers.CreateActionList(Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(aura_of_glory_buff, false),
                                                                                                Helpers.Create<InsinuatorMechanics.ContextConditionNonOutsiderAlignment>()
                                                                                               },
                                                                               new GameAction[] { Common.createContextActionApplyBuff(smite_target_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false),
                                                                                                  Common.createContextActionApplyBuff(aura_of_glory_buff,Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false),
                                                                                                  Helpers.Create<TeamworkMechanics.ContextActionOnUnitsWithinRadius>(c =>
                                                                                                  {
                                                                                                      c.around_caster = true;
                                                                                                      c.ignore_target = false;
                                                                                                      c.Radius = 13.Feet();
                                                                                                      c.actions = Helpers.CreateActionList(Helpers.CreateConditional(Helpers.Create<ContextConditionIsAlly>(),
                                                                                                                                                                     Common.createContextActionApplyBuff(hp_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)
                                                                                                                                                                    )
                                                                                                                                          );
                                                                                                  }
                                                                                                  ),                       
                                                                                                  Helpers.CreateConditional(Common.createContextConditionCasterHasFact(personal_champion),
                                                                                                                            Common.createContextActionApplyBuff(smite_target_buff2, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)),
                                                                                                  Helpers.CreateConditional(Common.createContextConditionCasterHasFact(personal_champion),
                                                                                                                            Common.createContextActionApplyBuff(aura_of_glory_buff2, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false) )
                                                                                                }
                                                                               )
                                                     );
            }
            );
            aura_of_glory_ability.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInvokedOutsider>());
            aura_of_glory_ability.SetNameDescriptionIcon("Aura of Glory",
                                                   "At 11th level, an insinuator can expend two uses of his smite impudence ability to grant the ability to smite impudence to all allies within 10 feet, using his bonuses. Allies must use this smite impudence ability before the start of the insinuator’s next turn, and the bonuses last for 1 minute. Using this ability is a free action.",
                                                   Helpers.GetIcon("7a4f0c48829952e47bb1fd1e4e9da83a")//aura of justice
                                                   );
            aura_of_glory = Common.AbilityToFeature(aura_of_glory_ability, false);
            config_dmg = aura_of_glory_ability.GetComponents<ContextRankConfig>().Where(c => c.IsBasedOnClassLevel).FirstOrDefault();
            aura_of_glory_ability.ReplaceComponent(config_dmg, config_dmg.CreateCopy(c => { Helpers.SetField(c, "m_Progression", ContextRankProgression.Div2); Helpers.SetField(c, "m_Class", getAntipaladinArray()); }));
        }


        static void createInvocation()
        {
            invocation = Helpers.CreateFeature("InsinuatorInvocationFeature",
                                               "Invocation",
                                               "At the start of each day, an insinuator can meditate to contact and barter with an outsider to empower him for a day. An insinuator can freely invoke an outsider of his own alignment. He can instead invoke an outsider within one step of his own alignment by succeeding at a Diplomacy or Knowledge (religion) skill check (DC = 15 + the insinuator’s antipaladin level). While invoking the power of an outsider, the insinuator radiates an alignment aura that matches that of the outsider’s, and becomes vulnerable to alignment-based effects that target that outsider’s alignment (such as smite evil or chaos hammer). None of an insinuator’s supernatural or spell-like class abilities function unless he has invoked the power of an outsider, and the alignment of the being invoked may affect how some abilities function.",
                                               "",
                                               Helpers.GetIcon("d3a4cb7be97a6694290f0dcfbd147113"),
                                               FeatureGroup.None
                                               );

            aura_of_ego = Helpers.CreateFeature("AuraOfEgoFeature",
                                               "Aura of Ego",
                                               "At 3rd level, an insinuator radiates an aura that bolsters allies and deters enemies. Each ally within 10 feet gains a +2 morale bonus on saving throws against fear effects. Enemies within 10 feet take a –2 penalty on saving throws against fear effects. This ability functions only while the insinuator is conscious, not if he is unconscious or dead.",
                                               "",
                                               Helpers.GetIcon("e45ab30f49215054e83b4ea12165409f"), //aura of courage
                                               FeatureGroup.None
                                               );

            var aura_of_ego_enemy_buff = Helpers.CreateBuff("AuraOfEgoEnemyBuff",
                                                            aura_of_ego.Name,
                                                            aura_of_ego.Description,
                                                            "",
                                                            aura_of_cowardice.Icon,
                                                            null,
                                                            Common.createSavingThrowBonusAgainstDescriptor(-2, ModifierDescriptor.UntypedStackable, SpellDescriptor.Fear | SpellDescriptor.Shaken)
                                                            );

            var aura_of_ego_ally_buff = Helpers.CreateBuff("AuraOfEgoAllyBuff",
                                                            aura_of_ego.Name,
                                                            aura_of_ego.Description,
                                                            "",
                                                            aura_of_ego.Icon,
                                                            null,
                                                            Common.createSavingThrowBonusAgainstDescriptor(2, ModifierDescriptor.Morale, SpellDescriptor.Fear | SpellDescriptor.Shaken)
                                                            );
            var aura_of_ego_enemy = Common.createAuraEffectBuff(aura_of_ego_enemy_buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()));
            var aura_of_ego_ally = Common.createAuraEffectBuff(aura_of_ego_ally_buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()));

            aura_of_ambition = Helpers.CreateFeature("AuraOfAmbitionFeature",
                                                   "Aura of Ambition",
                                                   "At 8th level, enemies within 10 feet of an insinuator take a –1 penalty on all saving throws. All allies within 10 feet gain a +1 bonus on all saving throws.\n"
                                                   + "This penalty does not stack with the penalty from aura of ego. This ability functions only while the insinuator is conscious, not if he is unconscious or dead.",
                                                   "",
                                                   LoadIcons.Image2Sprite.Create(@"AbilityIcons/AuraOfAmbition.png"),
                                                   FeatureGroup.None
                                                   );

            var aura_of_ambition_enemy_buff = Helpers.CreateBuff("AuraOfAmbitionEnemyBuff",
                                                aura_of_ambition.Name,
                                                aura_of_ambition.Description,
                                                "",
                                                aura_of_despair.Icon,
                                                null,
                                                Common.createSavingThrowBonusAgainstDescriptor(1, ModifierDescriptor.UntypedStackable, SpellDescriptor.Fear | SpellDescriptor.Shaken),
                                                Helpers.Create<BuffAllSavesBonus>(b => { b.Value = -1; b.Descriptor = ModifierDescriptor.UntypedStackable; })
                                                );

            var aura_of_ambition_ally_buff = Helpers.CreateBuff("AuraOfAmbitionAllyBuff",
                                                            aura_of_ambition.Name,
                                                            aura_of_ambition.Description,
                                                            "",
                                                            aura_of_ambition.Icon,
                                                            null,
                                                            Helpers.Create<BuffAllSavesBonus>(b => { b.Value = 1; b.Descriptor = ModifierDescriptor.UntypedStackable; })
                                                            );
            var aura_of_ambition_enemy = Common.createAuraEffectBuff(aura_of_ambition_enemy_buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()));
            var aura_of_ambition_ally = Common.createAuraEffectBuff(aura_of_ambition_ally_buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()));


            aura_of_belief = Helpers.CreateFeature("AuraOfBeliefFeature",
                                                   "Aura of Belief",
                                                   "At 14th level, an insinuator’s weapons are treated as chaos-aligned while he invokes a chaotic outsider, law-aligned when he invokes a lawful outsider, or evil-aligned while he invokes an evil outsider.",
                                                   "",
                                                   Helpers.GetIcon("90e59f4a4ada87243b7b3535a06d0638"), //bless
                                                   FeatureGroup.None
                                                   );

            aura_of_indomitability = Helpers.CreateFeature("AuraOfIndomitabilityFeature",
                                                           "Aura of Indomitability",
                                                           "At 17th level, an insinuator gains DR 10 that is bypassed by the alignment opposite of the outsider he has invoked for the day, or DR 5/— while invoking a neutral outsider.",
                                                           "",
                                                           Helpers.GetIcon("2a6a2f8e492ab174eb3f01acf5b7c90a"), //defensive stance
                                                           FeatureGroup.None
                                                           );



            personal_champion = Helpers.CreateFeature("PersonalChampionFeature",
                                                       "Personal Champion",
                                                       "At 20th level, an insinuator becomes a living embodiment of his selfish desires. His damage reduction from aura of indomitability increases to 15 (or 10 while invoking a neutral outsider). Whenever he uses smite impudence, he adds twice his full Charisma bonus to the attack roll and doubles his effective bonus damage gained from the smite. In addition, he can invoke a new outsider patron by meditating again.",
                                                       "",
                                                       LoadIcons.Image2Sprite.Create(@"AbilityIcons/PersonalChampion.png"),
                                                       FeatureGroup.None
                                                       );

            var alignments_map = new Dictionary<AlignmentMaskType, (AlignmentMaskType, AlignmentMaskType)>
            {
                {AlignmentMaskType.LawfulEvil, (AlignmentMaskType.NeutralEvil | AlignmentMaskType.LawfulEvil, AlignmentMaskType.Lawful | AlignmentMaskType.Evil) },
                {AlignmentMaskType.NeutralEvil, (AlignmentMaskType.Evil, AlignmentMaskType.Evil) },
                {AlignmentMaskType.ChaoticEvil, (AlignmentMaskType.NeutralEvil | AlignmentMaskType.ChaoticEvil, AlignmentMaskType.Chaotic | AlignmentMaskType.Evil) },
                {AlignmentMaskType.LawfulNeutral,  (AlignmentMaskType.LawfulEvil, AlignmentMaskType.Lawful) },
                {AlignmentMaskType.TrueNeutral,  (AlignmentMaskType.NeutralEvil, AlignmentMaskType.TrueNeutral) },
                {AlignmentMaskType.ChaoticNeutral, (AlignmentMaskType.ChaoticEvil, AlignmentMaskType.Chaotic) }
            };

            List<BlueprintAbility> abilities = new List<BlueprintAbility>();
            List<BlueprintBuff> buffs = new List<BlueprintBuff>();
            var remove_buffs = Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = new BlueprintBuff[0]);

            var cooldown_buff = Helpers.CreateBuff("InvocationCooldownBuff",
                                                   "Invocation Cooldown",
                                                   "Insinuator is no longer able to contact outsider to barter for power.",
                                                   "",
                                                   invocation.Icon,
                                                   null
                                                   );
            cooldown_buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.HiddenInUi);

            foreach (var kv in alignments_map)
            {
                var buff = Helpers.CreateBuff(kv.Key.ToString() + "InvocationBuff",
                                              invocation.Name + ": " + UIUtility.GetAlignmentText(kv.Key),
                                              invocation.Description,
                                              "",
                                              invocation.Icon,
                                              null,
                                              Helpers.Create<InsinuatorMechanics.InsinuatorOutsiderAlignment>(i => { i.alignment = kv.Key; i.shared_alignment = kv.Value.Item2; })
                                              );
                buff.SetBuffFlags(BuffFlags.RemoveOnRest);

                remove_buffs.Buffs = remove_buffs.Buffs.AddToArray(buff);

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
                var ability = Helpers.CreateAbility(kv.Key.ToString() + "InvocationAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Extraordinary,
                                                    CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(remove_buffs,
                                                                             Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionAlignmentStrict>(c => c.Alignment = kv.Key),
                                                                                                       apply_buff,
                                                                                                       Helpers.Create<SkillMechanics.ContextActionCasterSkillCheck>(c =>
                                                                                                       {
                                                                                                           c.CustomDC = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                           c.Success = Helpers.CreateActionList(apply_buff);
                                                                                                           c.Stat = StatType.CheckDiplomacy;
                                                                                                       })
                                                                                                       ),
                                                                             Helpers.CreateConditional(Common.createContextConditionHasFact(personal_champion, false),
                                                                                                       Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)
                                                                                                       )
                                                                             ),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.BonusValue, stepLevel: 15,
                                                                                    classes: getAntipaladinArray()
                                                                                    ),
                                                    Common.createAbilitySpawnFx("c4d861e816edd6f4eab73c55a18fdadd", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                    Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = kv.Value.Item1),
                                                    Helpers.Create<NewMechanics.AbilityShowIfCasterHasAlignment>(a => a.alignment = kv.Value.Item1),
                                                    Common.createAbilityCasterHasNoFacts(cooldown_buff)
                                                    );
                Common.setAsFullRoundAction(ability);
                ability.setMiscAbilityParametersSelfOnly();
                abilities.Add(ability);

                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_ego_ally, aura_of_ego);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_ego_enemy, aura_of_ego);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_ambition_ally, aura_of_ambition);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_ambition_enemy, aura_of_ambition);

                if (kv.Key != AlignmentMaskType.TrueNeutral)
                {
                    var aura_of_belief_buff = Helpers.CreateBuff(kv.Key.ToString() + "AuraOfBeliefBuff",
                                                     aura_of_belief.Name,
                                                     aura_of_belief.Description,
                                                     "",
                                                     aura_of_belief.Icon,
                                                     null,
                                                     Common.createAddOutgoingAlignmentFromAlignment(kv.Key)
                                                     );
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_belief_buff, aura_of_belief);
                }
                var aura_of_indomitability_buff = Helpers.CreateBuff(kv.Key.ToString() + "AuraOfIndomitabilityBuff",
                                                                     aura_of_indomitability.Name,
                                                                     aura_of_indomitability.Description,
                                                                     "",
                                                                     aura_of_indomitability.Icon,
                                                                     null,
                                                                     Common.createContextDRFromAlignment(Helpers.CreateContextValue(AbilityRankType.Default), kv.Key),
                                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureListRanks, ContextRankProgression.MultiplyByModifier, stepLevel: 5,
                                                                                                     featureList: Enumerable.Repeat(invocation, kv.Key == AlignmentMaskType.TrueNeutral ? 1 : 2).ToArray().AddToArray(personal_champion)
                                                                                                     )
                                                                     );
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_indomitability_buff, aura_of_indomitability);
            }

            var wrapper = Common.createVariantWrapper("InsinuatorInvocationBaseAbility", "", abilities.ToArray());

            invocation.AddComponent(Helpers.CreateAddFact(wrapper));
            wrapper.SetName(invocation.Name);
        }


        static void fixAntipaladinFeats()
        {
            extra_touch_of_corruption = library.CopyAndAdd<BlueprintFeature>("a2b2f20dfb4d3ed40b9198e22be82030", "ExtraTouchOfCorruption", "");
            extra_touch_of_corruption.ReplaceComponent<PrerequisiteFeature>(p => p.Feature = touch_of_corruption);
            extra_touch_of_corruption.SetNameDescription("Extra Touch of Corruption",
                                                         "You can use your touch of corruption two additional times per day.\nSpecial: You can gain Extra Touch of Corruption multiple times.Its effects stack.");
            library.AddFeats(extra_touch_of_corruption);
        }


        public static BlueprintCharacterClass[] getAntipaladinArray()
        {
            return new BlueprintCharacterClass[] { antipaladin_class };
        }

        static void createAntipaladinProgression()
        {
            antipaladin_proficiencies = library.CopyAndAdd<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb", "AntipaladinProficiencies", "");
            antipaladin_proficiencies.SetNameDescription("Antipaladin Proficiencies",
                                                         "Antipaladins are proficient with all simple and martial weapons, with all types of armor (heavy, medium, and light), and with shields (except tower shields).");
            unholy_resilence = library.CopyAndAdd<BlueprintFeature>("8a5b5e272e5c34e41aa8b4facbb746d3", "UnholyResilence", ""); //from divine grace
            unholy_resilence.SetNameDescription("Unholy Resilience",
                                                $"At 2nd level, an antipaladin gains a bonus equal to his Charisma bonus (if any{(Main.settings.balance_fixes ? ", up to his antipaladin level" : "")}) on all saving throws.");
            var paladin = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            if (unholy_resilence.GetComponent<ContextRankConfig>() != null)
            {
                var unholy_resilence_property = NewMechanics.ContextValueWithLimitProperty.createProperty("UnholyResilenceProperty", "39ab94d65a1a45dca3575c432f2a4163",
                                                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus,
                                                                                                      stat: Kingmaker.EntitySystem.Stats.StatType.Charisma,
                                                                                                      min: 0,
                                                                                                      type: AbilityRankType.Default),
                                                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                                      classes: new BlueprintCharacterClass[] { antipaladin_class },
                                                                                                      min: 0,
                                                                                                      type: AbilityRankType.DamageDiceAlternative),
                                                                       unholy_resilence
                                                                      );
                unholy_resilence.ReplaceComponent<ContextRankConfig>(a => Helpers.SetField(a, "m_CustomProperty", unholy_resilence_property));
            }

            plague_bringer = library.CopyAndAdd<BlueprintFeature>("41d1d0de15e672349bf4262a5acf06ce", "PlagueBearer", ""); //from divine health
            plague_bringer.SetNameDescription("Plague Bringer",
                                              "At 3rd level, the powers of darkness make an antipaladin a beacon of corruption and disease. An antipaladin does not take any damage or take any penalty from diseases. He can still contract diseases and spread them to others, but he is otherwise immune to their effects.");
            plague_bringer.ComponentsArray = new BlueprintComponent[] { Helpers.Create<BuffMechanics.SuppressBuffsCorrect>(s => s.Descriptor = SpellDescriptor.Disease) };

            antipaladin_alignment = library.CopyAndAdd<BlueprintFeature>("f8c91c0135d5fc3458fcc131c4b77e96", "AntipaladinAlignmentRestriction", "");
            antipaladin_alignment.SetIcon(NewSpells.aura_of_doom.Icon);
            antipaladin_alignment.SetDescription("An antipaladin who ceases to be evil loses all antipaladin spells and class features. She cannot thereafter gain levels as an antipaladin until she changes the alignment back.");
            antipaladin_alignment.ReplaceComponent<ForbidSpellbookOnAlignmentDeviation>(f =>
            {
                f.Alignment = AlignmentMaskType.Evil;
                f.Spellbooks = new BlueprintSpellbook[] { antipaladin_class.Spellbook };
            });

            createAntipaladinDeitySelection();
            createSmiteGood();
            createTouchOfCorruption();
            createChannelEnergy();
            createAuras();
            createFiendishBoon();

            antipaladin_progression = Helpers.CreateProgression("AntpladinProgression",
                                                              antipaladin_class.Name,
                                                              antipaladin_class.Description,
                                                              "",
                                                              antipaladin_class.Icon,
                                                              FeatureGroup.None);

            antipaladin_progression.Classes = getAntipaladinArray();



            antipaladin_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, antipaladin_proficiencies, antipaladin_deity, smite_good, antipaladin_alignment,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")), // touch calculate feature                                                                                      
                                                                    Helpers.LevelEntry(2, touch_of_corruption, unholy_resilence),
                                                                    Helpers.LevelEntry(3, aura_of_cowardice, plague_bringer, cruelty),
                                                                    Helpers.LevelEntry(4, smite_good_extra_use, channel_negative_energy),
                                                                    Helpers.LevelEntry(5, fiendish_boon[0]),
                                                                    Helpers.LevelEntry(6, cruelty),
                                                                    Helpers.LevelEntry(7, smite_good_extra_use),
                                                                    Helpers.LevelEntry(8, aura_of_despair, fiendish_boon[1]),
                                                                    Helpers.LevelEntry(9, cruelty),
                                                                    Helpers.LevelEntry(10, smite_good_extra_use),
                                                                    Helpers.LevelEntry(11, aura_of_vengeance, fiendish_boon[2]),
                                                                    Helpers.LevelEntry(12, cruelty),
                                                                    Helpers.LevelEntry(13, smite_good_extra_use),
                                                                    Helpers.LevelEntry(14, aura_of_sin, fiendish_boon[3]),
                                                                    Helpers.LevelEntry(15, cruelty),
                                                                    Helpers.LevelEntry(16, smite_good_extra_use),
                                                                    Helpers.LevelEntry(17, aura_of_deparvity, fiendish_boon[4]),
                                                                    Helpers.LevelEntry(18, cruelty),
                                                                    Helpers.LevelEntry(19, smite_good_extra_use),
                                                                    Helpers.LevelEntry(20, tip_of_spear, fiendish_boon[5])
                                                                    };

            antipaladin_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { antipaladin_proficiencies, antipaladin_deity, antipaladin_alignment};
            antipaladin_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(fiendish_boon.AddToArray(unholy_resilence, plague_bringer)),
                                                                Helpers.CreateUIGroup(smite_good, smite_good_extra_use),
                                                                Helpers.CreateUIGroup(aura_of_cowardice, aura_of_despair, aura_of_vengeance, aura_of_sin, aura_of_deparvity, tip_of_spear),
                                                                Helpers.CreateUIGroup(touch_of_corruption, channel_negative_energy, cruelty),
                                                           };
        }


        static void createAuras()
        {
            var cowardice_buff = Helpers.CreateBuff("AuraOfCowardiceEffectBuff",
                                                    "Aura of Cowardice",
                                                    "At 3rd level, an antipaladin radiates a palpably daunting aura that causes all enemies within 10 feet to take a –4 penalty on saving throws against fear effects. Creatures that are normally immune to fear lose that immunity while within 10 feet of an antipaladin with this ability. This ability functions only while the antipaladin remains conscious, not if he is unconscious or dead.",
                                                    "",
                                                    Helpers.GetIcon("08cb5f4c3b2695e44971bf5c45205df0"),
                                                    null,
                                                    Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), 
                                                                                                          ModifierDescriptor.UntypedStackable, SpellDescriptor.Shaken | SpellDescriptor.Fear)
                                                    );

            aura_of_cowardice = Common.createAuraEffectFeature(cowardice_buff.Name,
                                                               cowardice_buff.Description,
                                                               cowardice_buff.Icon,
                                                               cowardice_buff,
                                                               13.Feet(),
                                                               Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>())
                                                               );

            var despair_buff = Helpers.CreateBuff("AuraOfDespairEffectBuff",
                                        "Aura of Despair",
                                        "At 8th level, enemies within 10 feet of an antipaladin take a –2 penalty on all saving throws. This penalty does not stack with the penalty from aura of cowardice.\nThis ability functions only while the antipaladin is conscious, not if he is unconscious or dead.",
                                        "",
                                        Helpers.GetIcon("4baf4109145de4345861fe0f2209d903"), //crushing despair
                                        null,
                                        Helpers.Create<BuffAllSavesBonus>(b => { b.Value = -2; b.Descriptor = ModifierDescriptor.UntypedStackable; })
                                        );

            aura_of_despair = Common.createAuraEffectFeature(despair_buff.Name,
                                                             despair_buff.Description,
                                                             despair_buff.Icon,
                                                             despair_buff,
                                                             13.Feet(),
                                                             Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>())
                                                             );

            cowardice_buff.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList, ContextRankProgression.BonusValue,
                                        stepLevel: -4, featureList: new BlueprintFeature[] { aura_of_despair, aura_of_despair })
                                       );


            var deparvity_buff = Helpers.CreateBuff("AuraOfDeparvityEffectBuff",
                                                    "Aura of Deparvity",
                                                    "At 17th level, an antipaladin gains DR 5/good. Each enemy within 10 feet takes a –4 penalty on saving throws against compulsion effects. This ability functions only while the antipaladin is conscious, not if he is unconscious or dead.",
                                                    "",
                                                    Helpers.GetIcon("41cf93453b027b94886901dbfc680cb9"), //overwhelming presence
                                                    null,
                                                    Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                                          ModifierDescriptor.UntypedStackable, SpellDescriptor.Compulsion),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList, ContextRankProgression.BonusValue,
                                                                                    stepLevel: -4, featureList: new BlueprintFeature[] { aura_of_despair, aura_of_despair })
                                                    );

            aura_of_deparvity = Common.createAuraEffectFeature(deparvity_buff.Name,
                                                             deparvity_buff.Description,
                                                             deparvity_buff.Icon,
                                                             deparvity_buff,
                                                             13.Feet(),
                                                             Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>())
                                                             );

            aura_of_deparvity.AddComponent(Common.createAlignmentDR(5, DamageAlignment.Good));

            var sin_buff = Helpers.CreateBuff("AuraOfSinEffectBuff",
                                        "Aura of Sin",
                                        "At 14th level, an antipaladin’s weapons are treated as evil-aligned for the purposes of overcoming damage reduction. Any attack made against an enemy within 10 feet of him is treated as evil-aligned for the purposes of overcoming damage reduction. This ability functions only while the antipaladin is conscious, not if he is unconscious or dead.",
                                        "",
                                        Helpers.GetIcon("8bc64d869456b004b9db255cdd1ea734"), //bane
                                        null,
                                        library.Get<BlueprintBuff>("f84a39e55230f5e499588c5cd19548cd").GetComponent<AddIncomingDamageWeaponProperty>().CreateCopy(a => a.Alignment = DamageAlignment.Evil)
                                        );

            aura_of_sin = Common.createAuraEffectFeature(sin_buff.Name,
                                                         sin_buff.Description,
                                                         sin_buff.Icon,
                                                         sin_buff,
                                                         13.Feet(),
                                                         Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>())
                                                         );
            aura_of_sin.AddComponent(library.Get<BlueprintFeature>("0437f4af5ad49b544bccf48aa7a51319").GetComponent<AddOutgoingPhysicalDamageProperty>().CreateCopy(a => a.Alignment = DamageAlignment.Evil));
        }


        static void createFiendishBoon()
        {
            fiendish_boon_resource = Helpers.CreateAbilityResource("FiendishBoonResource", "", "", "", null);
            fiendish_boon_resource.SetIncreasedByLevelStartPlusDivStep(1, 9, 1, 4, 1, 0, 0.0f, getAntipaladinArray());

            var divine_weapon = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var enchants = WeaponEnchantments.temporary_enchants;

            var enhancement_buff = Helpers.CreateBuff("FiendishBondEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(ActivatableAbilityGroup.DivineWeaponProperty,
                                                                                                      false, true,
                                                                                                      enchants
                                                                                                      )
                                         );
            var fiendish_boon_enhancement_buff = Helpers.CreateBuff("FiendishBondEnchancementSwitchBuff",
                                                                 "Fiendish Boon",
                                                                 "Upon reaching 5th level, an antipaladin forms a divine bond with her weapon. As a standard action, she can call upon the aid of a fiendish spirit for 1 minute per antipaladin level.\nAt 5th level, this spirit grants the weapon a +1 enhancement bonus. For every three levels beyond 5th, the weapon gains another +1 enhancement bonus, to a maximum of +6 at 20th level. These bonuses can be added to the weapon, stacking with existing weapon bonuses to a maximum of +5.\nAlternatively, they can be used to add any of the following weapon properties: anarchic, axiomatic, flaming, keen, speed, unholy, vicious and vorpal. Adding these properties consumes an amount of bonus equal to the property's cost. These bonuses are added to any properties the weapon already has, but duplicate abilities do not stack.\nAn antipaladin can use this ability once per day at 5th level, and one additional time per day for every four levels beyond 5th, to a total of four times per day at 17th level.",
                                                                 "",
                                                                 NewSpells.magic_weapon_greater.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, is_permanent: true, dispellable: false)
                                                                                                     )
                                                                 );
            enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var vicious_enchant = library.Get<BlueprintWeaponEnchantment>("a1455a289da208144981e4b1ef92cc56");
            var vicious = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementVicious",
                                                                        "Fiendish Boon - Vicious",
                                                                        "An antipaladin can add the vicious property to a weapon enhanced with her fiendish boon, but this consumes 1 point of enhancement bonus granted to this weapon.\n" + vicious_enchant.Description,
                                                                        LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWVicious.png"),
                                                                        fiendish_boon_enhancement_buff,
                                                                        vicious_enchant,
                                                                        1, ActivatableAbilityGroup.DivineWeaponProperty);

            var vorpal = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementVorpal",
                                                            "Fiendish Boon - Vorpal",
                                                            "An antipaladin can add the vorpal property to a weapon enhanced with her fiendish boon, but this consumes 5 points of enhancement bonus granted to this weapon.\n" + WeaponEnchantments.vorpal.Description,
                                                            LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWVorpal.png"),
                                                            fiendish_boon_enhancement_buff,
                                                            WeaponEnchantments.vorpal,
                                                            5, ActivatableAbilityGroup.DivineWeaponProperty);

            var speed_enchant = library.Get<BlueprintWeaponEnchantment>("f1c0c50108025d546b2554674ea1c006");
            var speed = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementSpeed",
                                                "Fiendish Boon - Speed",
                                                "An antipaladin can add the speed property to a weapon enhanced with her fiendish boon, but this consumes 3 points of enhancement bonus granted to this weapon.\n" + speed_enchant.Description,
                                                library.Get<BlueprintActivatableAbility>("ed1ef581af9d9014fa1386216b31cdae").Icon, //speed
                                                fiendish_boon_enhancement_buff,
                                                speed_enchant,
                                                3, ActivatableAbilityGroup.DivineWeaponProperty);

            var flaming = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementFlaming",
                                                                "Fiendish Boon - Flaming",
                                                                "An antipaladin can add the flaming property to a weapon enhanced with her fiendish boon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                                library.Get<BlueprintActivatableAbility>("7902941ef70a0dc44bcfc174d6193386").Icon,
                                                                fiendish_boon_enhancement_buff,
                                                                library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                                1, ActivatableAbilityGroup.DivineWeaponProperty);

            var keen = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementKeen",
                                                            "Fiendish Boon - Keen",
                                                            "An antipaladin can add the keen property to a weapon enhanced with her fiendish boon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                            library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                            fiendish_boon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                            1, ActivatableAbilityGroup.DivineWeaponProperty);

            var unholy = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementUnholy",
                                                            "Fiendish Boon - Unholy",
                                                            "An antipaladin can add the unholy property to a weapon enhanced with her fiendish boon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn unholy weapon is imbued with unholy power. This power makes the weapon evil-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of good alignment.",
                                                            LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWUnholy.png"),
                                                            fiendish_boon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"),
                                                            2, ActivatableAbilityGroup.DivineWeaponProperty,
                                                            AlignmentMaskType.Evil);

            var axiomatic = Common.createEnchantmentAbility("FiendishBoonEnchancementAxiomatic",
                                                            "Fiendish Boon - Axiomatic",
                                                            "An antipaladin can add the axiomatic property to a weapon enhanced with her fiendish boon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn axiomatic weapon is infused with lawful power. It makes the weapon lawful-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against chaotic creatures.",
                                                            library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,
                                                            fiendish_boon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4"),
                                                            2, ActivatableAbilityGroup.DivineWeaponProperty,
                                                            AlignmentMaskType.Lawful);

            var anarchic = Common.createEnchantmentAbility("FiendishBoonEnchancementAnarchic",
                                                            "Fiendish Boon - Anarchic",
                                                            "An antipaladin can add the anarchic property to a weapon enhanced with her fiendish boon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn anarchic weapon is infused with the power of chaos. It makes the weapon chaotic-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of lawful alignment.",
                                                            LoadIcons.Image2Sprite.Create(@"AbilityIcons/HWAnarchic.png"),
                                                            fiendish_boon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"),
                                                            2, ActivatableAbilityGroup.DivineWeaponProperty,
                                                            AlignmentMaskType.Chaotic);

           
            var ability = Helpers.CreateAbility("FiendishBoonSwitchAbility",
                                                 fiendish_boon_enhancement_buff.Name,
                                                 fiendish_boon_enhancement_buff.Description,
                                                 "",
                                                 fiendish_boon_enhancement_buff.Icon,
                                                 AbilityType.Supernatural,
                                                 CommandType.Standard,
                                                 AbilityRange.Personal,
                                                 Helpers.minutesPerLevelDuration,
                                                 "",
                                                 Helpers.CreateRunActions(Common.createContextActionApplyBuff(fiendish_boon_enhancement_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false)),
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray()),
                                                 Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil),
                                                 fiendish_boon_resource.CreateResourceLogic(),
                                                 Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>()
                                                 );
            ability.setMiscAbilityParametersSelfOnly(Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon);
            ability.NeedEquipWeapons = true;
            ability.AddComponents(library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4").GetComponent<AbilitySpawnFx>());

            fiendish_boon = new BlueprintFeature[6];
            fiendish_boon[0] = Helpers.CreateFeature("FiendishBoonWeaponEnchancementFeature",
                                                    "Fiendish Boon +1",
                                                    fiendish_boon_enhancement_buff.Description,
                                                    "",
                                                    fiendish_boon_enhancement_buff.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddAbilityResource(fiendish_boon_resource),
                                                    Helpers.CreateAddFacts(ability, flaming, keen, vicious)
                                                    );

            fiendish_boon[1] = Helpers.CreateFeature("FiendishBoonWeaponEnchancement2Feature",
                                                    "Fiendish Boon +2",
                                                    fiendish_boon_enhancement_buff.Description,
                                                    "",
                                                    fiendish_boon_enhancement_buff.Icon,
                                                    FeatureGroup.None,
                                                    Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty),
                                                    Helpers.CreateAddFacts(anarchic, axiomatic, unholy)
                                                    );

            fiendish_boon[2] = Helpers.CreateFeature("FiendishBoonWeaponEnchancement3Feature",
                                                    "Fiendish Boon +3",
                                                    fiendish_boon_enhancement_buff.Description,
                                                    "",
                                                    fiendish_boon_enhancement_buff.Icon,
                                                    FeatureGroup.None,
                                                    Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty),
                                                    Helpers.CreateAddFacts(speed)
                                                    );

            fiendish_boon[3] = Helpers.CreateFeature("FiendishBoonWeaponEnchancement4Feature",
                                                    "Fiendish Boon +4",
                                                    fiendish_boon_enhancement_buff.Description,
                                                    "",
                                                    fiendish_boon_enhancement_buff.Icon,
                                                    FeatureGroup.None,
                                                    Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty)
                                                    );

            fiendish_boon[4] = Helpers.CreateFeature("FiendishBoonWeaponEnchancement5Feature",
                                                    "Fiendish Boon +5",
                                                    fiendish_boon_enhancement_buff.Description,
                                                    "",
                                                    fiendish_boon_enhancement_buff.Icon,
                                                    FeatureGroup.None,
                                                    Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty),
                                                    Helpers.CreateAddFacts(vorpal)
                                                    );

            fiendish_boon[5] = Helpers.CreateFeature("FiendishBoonWeaponEnchancement6Feature",
                                                        "Fiendish Boon +6",
                                                        fiendish_boon_enhancement_buff.Description,
                                                        "",
                                                        fiendish_boon_enhancement_buff.Icon,
                                                        FeatureGroup.None,
                                                        Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty)
                                                        );
        }


        static void createTouchOfCorruption()
        {
            var bestow_curse = library.Get<BlueprintAbility>("989ab5c44240907489aba0a8568d0603");
            var contagion = library.Get<BlueprintAbility>("48e2744846ed04b4580be1a3343a5d3d");

            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            //6
            var dazed = Common.dazed_non_mind_affecting;
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var diseased = contagion.Variants.Select(b => Common.extractActions<ContextActionApplyBuff>(b.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility.GetComponent<AbilityEffectRunAction>().Actions.Actions).First().Buff).ToArray();
            //9
            //cursed
            var cursed = bestow_curse.Variants.Select(b => Common.extractActions<ContextActionApplyBuff>(b.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility.GetComponent<AbilityEffectRunAction>().Actions.Actions).First().Buff).ToArray();
            var exahusted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");
            var frightened = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");
            var nauseated = library.Get<BlueprintBuff>("956331dba5125ef48afe41875a00ca0e");
            var poisoned = library.Get<BlueprintBuff>("ba1ae42c58e228c4da28328ea6b4ae34");

            //12
            var blinded = library.Get<BlueprintBuff>("0ec36e7596a4928489d2049e1e1c76a7");
            var deafened = Common.deafened;
            var paralyzed = library.Get<BlueprintBuff>("af1e2d232ebbb334aaf25e2a46a92591");
            var stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");

            CrueltyEntry.addCruelty("Fatigued", "The target is fatigued if Fortitude save is failed.", 0, "", 0, SavingThrowType.Fortitude, fatigued);
            CrueltyEntry.addCruelty("Shaken", "The target is shaken for 1 round per level of the antipaladin if Will save is failed.", 0, "", 1, SavingThrowType.Will, shaken);
            CrueltyEntry.addCruelty("Sickened", "The target is sickened for 1 round per level of the antipaladin if Fortitude save is failed.", 0, "", 1, SavingThrowType.Fortitude, sickened);
            CrueltyEntry.addCruelty("Diseased", "The target contracts a disease, as if the antipaladin had cast contagion, using his antipaladin level as his caster level if Fortitude save is failed.", 6, "", 0, SavingThrowType.Fortitude, diseased);
            CrueltyEntry.addCruelty("Dazed", "The target is dazed for 1 round if Will save is failed.", 6, "", -1, SavingThrowType.Will, dazed);
            CrueltyEntry.addCruelty("Staggered", "The target is staggered for 1 round per two levels of the antipaladin if Fortitude save is failed.", 6, "", 2, SavingThrowType.Fortitude, staggered);
            CrueltyEntry.addCruelty("Cursed", "The target is cursed, as if the antipaladin had cast bestow curse, using his antipaladin level as his caster level if Will save is failed.", 9, "", 0, SavingThrowType.Will, cursed);
            CrueltyEntry.addCruelty("Exhausted", "The target is exhausted if Fortitude save is failed.", 9, "Fatigued", 9, SavingThrowType.Fortitude, exahusted);
            CrueltyEntry.addCruelty("Frightened", "The target is frightened for 1 round per three levels of the antipaladin if Will save is failed.", 9, "Shaken", 2, SavingThrowType.Will, frightened);
            CrueltyEntry.addCruelty("Nauseated", "The target is nauseated for 1 round per three levels of the antipaladin if Fortitude save is failed.", 9, "Sickened", 3, SavingThrowType.Fortitude, nauseated);
            CrueltyEntry.addCruelty("Poisoned", "The target is poisoned, as if the antipaladin had cast poison, using the antipaladin’s level as the caster level if Fortitude save is failed.", 9, "", 0, SavingThrowType.Fortitude, poisoned);
            CrueltyEntry.addCruelty("Blinded", "The target is blinded for 1 round per level of the antipaladin if Fortitude save is failed.", 12, "", 1, SavingThrowType.Fortitude, blinded);
            CrueltyEntry.addCruelty("Deafened", "The target is deafened for 1 round per level of the antipaladin if Fortitude save is failed.", 12, "", 1, SavingThrowType.Fortitude, deafened);
            CrueltyEntry.addCruelty("Paralyzed", " The target is paralyzed for 1 round if Will save is failed.", 12, "", -1, SavingThrowType.Will, paralyzed);
            CrueltyEntry.addCruelty("Stunned", "The target is stunned for 1 round per four levels of the antipaladin if Fortitude save is failed.", 12, "", 1, SavingThrowType.Fortitude, stunned);


            var paladin = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            touch_of_corruption_resource = library.Get<BlueprintAbilityResource>("9dedf41d995ff4446a181f143c3db98c");
            ClassToProgression.addClassToResource(antipaladin_class, new BlueprintArchetype[0], touch_of_corruption_resource, paladin);

            var dice = Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.DamageDice), 0);
            var heal_action = Common.createContextActionHealTarget(dice);
            var damage_undead_action = Helpers.CreateActionDealDamage(DamageEnergyType.PositiveEnergy, dice);
            var damage_living_action = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, dice);
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                          type: AbilityRankType.DamageDice, classes: getAntipaladinArray());

            var deaths_embrace_living = library.Get<BlueprintFeature>("fd7c08ccd3c7773458eb9613db3e93ad");

            var inflict_light_wounds = library.Get<BlueprintAbility>("244a214d3b0188e4eb43d3a72108b67b");
            var ability = Helpers.CreateAbility("TouchOfCorruptionAbility",
                                                "Touch of Corruption",
                                                $"Beginning at 2nd level, an antipaladin surrounds his hand with a fiendish flame, causing terrible wounds to open on those he touches. Each day he can use this ability a number of times equal to 1/2 his antipaladin level + his Charisma modifier. As a touch attack, an antipaladin can cause 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of damage for every two antipaladin levels he possesses. Using this ability is a standard action that does not provoke attacks of opportunity.\n"
                                                + "An antipaladin can also chose to channel corruption into a melee weapon by spending 2 uses of this ability as a swift action. The next enemy struck with this weapon will suffer the effects of this ability.\n"
                                                + $"Alternatively, an antipaladin can use this power to heal undead creatures, restoring 1d{BalanceFixes.getDamageDieString(DiceType.D6)} hit points for every two levels the antipaladin possesses. This ability is modified by any feat, spell, or effect that specifically works with the lay on hands paladin class feature. For example, the Extra Lay On Hands feat grants an antipaladin 2 additional uses of the touch of corruption class feature.",
                                                "",
                                                Helpers.GetIcon("989ab5c44240907489aba0a8568d0603"), //bestow curse
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "",
                                                Helpers.savingThrowNone,
                                                Helpers.CreateRunActions(Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(),
                                                                                                                                        Common.createContextConditionHasFact(deaths_embrace_living)
                                                                                                                                    ),
                                                                                                    heal_action,
                                                                                                    damage_living_action)),
                                                inflict_light_wounds.GetComponent<AbilityDeliverTouch>(),
                                                inflict_light_wounds.GetComponent<AbilitySpawnFx>(),
                                                context_rank_config,
                                                Helpers.Create<AbilityUseOnRest>(c => c.Type = AbilityUseOnRestType.HealUndead),
                                                Common.createContextCalculateAbilityParamsBasedOnClass(antipaladin_class, StatType.Charisma)
                                                );
            ability.AvailableMetamagic = Metamagic.Reach; //for dread vanguard aura
            ability.setMiscAbilityParametersTouchHarmful();
            var ability_cast = Helpers.CreateTouchSpellCast(ability, touch_of_corruption_resource);

            ability_cast.AddComponents(Common.createAbilityTargetHasFact(true, Common.construct),
                                       Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil),
                                       Helpers.CreateResourceLogic(touch_of_corruption_resource));
            ability_cast.AvailableMetamagic = Metamagic.Reach; //for dread vanguard aura
            var wrapper = Common.createVariantWrapper("AntipladinCrueltyBaseAbility", "", ability_cast);

            touch_of_corruption = Common.AbilityToFeature(wrapper, false);
            touch_of_corruption.AddComponent(touch_of_corruption_resource.CreateAddAbilityResource());

            cruelty = Helpers.CreateFeatureSelection("CrueltiesFeatureSelection",
                                                     "Cruelty",
                                                     "At 3rd level, and every three levels thereafter, an antipaladin can select one cruelty. Each cruelty adds an effect to the antipaladin’s touch of corruption ability. Whenever the antipaladin uses touch of corruption to deal damage to one target, the target also receives the additional effect from one of the cruelties possessed by the antipaladin. This choice is made when the touch is used. The target receives a save to avoid this cruelty. If the save is successful, the target takes the damage as normal, but not the effects of the cruelty. The DC of this save is equal to 10 + 1/2 the antipaladin’s level + the antipaladin’s Charisma modifier.",
                                                     "",
                                                     null,
                                                     FeatureGroup.None);

            cruelty.AllFeatures = CrueltyEntry.createCruelties(wrapper);
            touch_of_corruption_base = wrapper;

            //create channel corruption
            var channels = new List<BlueprintAbility>();
            var ranged_channels = new List<BlueprintAbility>();
            var remove_buffs = Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = new BlueprintBuff[0]);
            
            foreach (var a in wrapper.Variants)
            {
                for (int i = 0; i < 2; i++)
                {
                    var touch_ability = a.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility;
                    var actions = new GameAction[] { Helpers.Create<ContextActionCastSpell>(c => c.Spell = touch_ability), Helpers.Create<ContextActionRemoveSelf>() };
                    var buff = Helpers.CreateBuff("Channel" + (i == 0 ? "" : "Ranged") + touch_ability.name + "Buff",
                                                  "Channel " + touch_ability.Name + (i == 0 ? "" : " (Ranged)"),
                                                  touch_ability.Description,
                                                  "",
                                                  touch_ability.Icon,
                                                  null,
                                                  Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(actions), check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Melee)
                                                  );
                    if (i == 0)
                    {
                        buff.AddComponent(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(actions), check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Melee));
                    }
                    else
                    {
                        buff.AddComponents(Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(actions[0]), check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged),
                                           Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(actions[1]), only_hit: false, check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged)
                                           );
                    }
                    remove_buffs.Buffs = remove_buffs.Buffs.AddToArray(buff);

                    var channel = Helpers.CreateAbility("Channel" + (i == 0 ? "" : "Ranged") + touch_ability.name,
                                                        buff.Name,
                                                        buff.Description,
                                                        "",
                                                        buff.Icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Swift,
                                                        AbilityRange.Personal,
                                                        Helpers.oneMinuteDuration,
                                                        "",
                                                        Helpers.CreateRunActions(remove_buffs,
                                                                                 Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)),
                                                        touch_of_corruption_resource.CreateResourceLogic(amount: 2),
                                                        touch_ability.GetComponent<ContextCalculateAbilityParamsBasedOnClass>()
                                                        );
                    var requirement = a.GetComponent<AbilityShowIfCasterHasFact>();
                    if (requirement != null)
                    {
                        channel.AddComponent(requirement);
                    }
                    channel.setMiscAbilityParametersSelfOnly();
                    if (i == 0)
                    {
                        channel.AddComponent(Helpers.Create<AbilityCasterMainWeaponIsMelee>());
                        channels.Add(channel);
                    }
                    else
                    {
                        channel.AddComponent(Helpers.Create<NewMechanics.AbilityCasterMainWeaponIsRanged>());
                        ranged_channels.Add(channel);
                    }
                }
            }

            var channel_wrapper = Common.createVariantWrapper("ChannelTouchOfCorruptionBase", "", channels.ToArray());
            channel_wrapper.SetIcon(LoadIcons.Image2Sprite.Create(@"AbilityIcons/WeaponEvil.png"));

            ranged_channel_wrapper = Common.createVariantWrapper("RangedChannelTouchOfCorruptionBase", "", ranged_channels.ToArray());
            ranged_channel_wrapper.SetIcon(LoadIcons.Image2Sprite.Create(@"AbilityIcons/TipOfTheSpear.png"));
            touch_of_corruption.AddComponent(Helpers.CreateAddFact(channel_wrapper));


            ability_focus_touch_of_corruption = Helpers.CreateFeature("TouchOfCorruptionFocusFeature",
                                                                    "Ability Focus: Cruelty",
                                                                    "The DC to resist effects of antipaladin cruelties is increased by 2.",
                                                                    "",
                                                                    null,
                                                                    FeatureGroup.Feat,
                                                                    Helpers.PrerequisiteFeature(cruelty),
                                                                    Helpers.Create<NewMechanics.IncreaseSpellDCForBlueprints>(i => 
                                                                    {
                                                                        i.value = 2;
                                                                        i.blueprints = touch_of_corruption_base.Variants.Skip(1).Select(t => t.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility).ToArray();
                                                                    })
                                                                    );
            library.AddFeats(ability_focus_touch_of_corruption);
        }


        static void createChannelEnergy()
        {
            var negative_energy_feature = library.Get<BlueprintFeature>("3adb2c906e031ee41a01bfc1d5fb7eea");
            var context_rank_config = Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray(), progression: ContextRankProgression.OnePlusDiv2);
            var dc_scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(getAntipaladinArray(), StatType.Charisma);
            channel_negative_energy = Helpers.CreateFeature("AntipaladinChannelNegativeEnergyFeature",
                                                            "Channel Negative Energy",
                                                            "When an antipaladin reaches 4th level, he gains the supernatural ability to channel negative energy like a cleric. Using this ability consumes two uses of his touch of corruption ability. An antipaladin uses his level as his effective cleric level when channeling negative energy. This is a Charisma-based ability.",
                                                            "",
                                                            negative_energy_feature.Icon,
                                                            FeatureGroup.None);

            var harm_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHarm,
                                                                      "AntipaladinChannelEnergyHarmLiving",
                                                                      "",
                                                                      $"Channeling negative energy causes a burst that damages all living creatures in a 30 - foot radius centered on the antipaladin. The amount of damage inflicted is equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage plus 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage for every two antipaladin levels beyond 1st (3d{BalanceFixes.getDamageDieString(DiceType.D6)}  at 5th, and so on). Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1 / 2 the antipaladin's level + the antipaladin's Charisma modifier.",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(touch_of_corruption_resource, amount: 2));
            var heal_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHeal,
                                                                        "AntipaladinChannelEnergyHealUndead",
                                                                        "",
                                                                        $"Channeling negative energy causes a burst that heals all undead creatures in a 30-foot radius centered on the antipaladin. The amount of damage healed is equal to 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage plus 1d{BalanceFixes.getDamageDieString(DiceType.D6)}  points of damage for every two antipaladin levels beyond 1st (3d{BalanceFixes.getDamageDieString(DiceType.D6)}  at 5th, and so on).",
                                                                        "",
                                                                        context_rank_config,
                                                                        dc_scaling,
                                                                        Helpers.CreateResourceLogic(touch_of_corruption_resource, amount: 2));

            var harm_living_base = Common.createVariantWrapper("AntipaladinNegativeHarmBase", "", harm_living);
            var heal_undead_base = Common.createVariantWrapper("AntipaladinNegativeHealBase", "", heal_undead);
            harm_living.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_undead.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_undead.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>());
            harm_living.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>());

            ChannelEnergyEngine.storeChannel(harm_living, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHarm);
            ChannelEnergyEngine.storeChannel(heal_undead, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHeal);

            channel_negative_energy.AddComponent(Helpers.CreateAddFacts(harm_living_base, heal_undead_base));

            //add extra channel
            extra_channel_resource = Helpers.CreateAbilityResource("AntipaladinExtraChannelResource", "", "", "", null);
            extra_channel_resource.SetFixedResource(0);


            var harm_living_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHarm,
                                                          "AntipaladinChannelEnergyHarmLivingExtra",
                                                          harm_living.Name + " (Extra)",
                                                          harm_living.Description,
                                                          "",
                                                          harm_living.GetComponent<ContextRankConfig>(),
                                                          harm_living.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                                          Helpers.CreateResourceLogic(extra_channel_resource, true, 1));

            var heal_undead_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHeal,
                                              "AntipaladinChannelEnergyHealUndeadExtra",
                                              heal_undead.Name + " (Extra)",
                                              heal_undead.Description,
                                              "",
                                              heal_undead.GetComponent<ContextRankConfig>(),
                                              heal_undead.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                              Helpers.CreateResourceLogic(extra_channel_resource, true, 1));


            harm_living_extra.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_undead_extra.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_undead_extra.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>());
            harm_living_extra.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>());

            harm_living_base.addToAbilityVariants(harm_living_extra);
            heal_undead_base.addToAbilityVariants(heal_undead_extra);
            ChannelEnergyEngine.storeChannel(harm_living_extra, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHarm);
            ChannelEnergyEngine.storeChannel(heal_undead_extra, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHeal);

            channel_negative_energy.AddComponent(Helpers.CreateAddAbilityResource(extra_channel_resource));
            extra_channel = ChannelEnergyEngine.createExtraChannelFeat(harm_living_extra, channel_negative_energy, "ExtraChannelAntipaladin", "Extra Channel (Antipaladin)", "");
        }

        static void createSmiteGood()
        {
            var resource = library.Get<BlueprintAbilityResource>("b4274c5bb0bf2ad4190eb7c44859048b");
            tip_of_spear = Helpers.CreateFeature("TipOfTheSpearFeature",
                                                 "Tip of the Spear",
                                                 "At 20th level, the antipaladin tears through heroes and rival villains alike. The antipaladin gains three additional uses of smite good per day and can smite foes regardless of their alignment.",
                                                 "",
                                                 LoadIcons.Image2Sprite.Create(@"AbilityIcons/TipOfTheSpear.png"),
                                                 FeatureGroup.None,
                                                 resource.CreateIncreaseResourceAmount(3)
                                                 );

            smite_good = Common.createSmite("AntipaladinSmiteGood",
                                            "Smite Good",
                                            "Once per day, an antipaladin can call out to the dark powers to crush the forces of good. As a swift action, the antipaladin chooses one target within sight to smite. If this target is good, the antipaladin adds his Charisma bonus (if any) on his attack rolls and adds his antipaladin level on all damage rolls made against the target of his smite, smite good attacks automatically bypass any DR the creature might possess.\n"
                                            + "In addition, while smite good is in effect, the antipaladin gains a deflection bonus equal to his Charisma modifier (if any) to his AC against attacks made by the target of the smite. If the antipaladin targets a creature that is not good, the smite is wasted with no effect.\n"
                                            + "The smite evil lasts until the target dies or the paladin selects a new target. At 4th level, and at every three levels thereafter, the paladin may smite evil one additional time per day.",
                                            "",
                                            "",
                                            LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteGood.png"),
                                            getAntipaladinArray(),
                                            Helpers.Create<NewMechanics.ContextConditionAlignmentUnlessCasterHasFact>(c => { c.Alignment = AlignmentComponent.Good; c.fact = tip_of_spear; })
                                            );

            smite_good_extra_use = library.CopyAndAdd<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137", "SmiteGoodAdditionalUse", "");
            smite_good_extra_use.SetNameDescriptionIcon("Smite Good — Additional Use",
                                              smite_good.Description,
                                              smite_good.Icon);

            smite_resource = resource;

            var smite_good_ability = smite_good.GetComponent<AddFacts>().Facts[0] as BlueprintAbility;
            smite_good_ability.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));


            aura_of_vengeance = Common.createSmiteForAllies("AntipaladinAuraOfVengeance",
                                                            "Aura of Vengeance",
                                                            "At 11th level, an antipaladin can expend two uses of his smite good ability to grant the ability to smite good to all allies within 10 feet, using his bonuses. Allies must use this smite good ability by the start of the antipaladin’s next turn and the bonuses last for 1 minute. Using this ability is a free action.",
                                                            "",
                                                            "",
                                                            NewSpells.command.Icon,
                                                            getAntipaladinArray(),
                                                            Helpers.Create<NewMechanics.ContextConditionAlignmentUnlessCasterHasFact>(c => { c.Alignment = AlignmentComponent.Good; c.fact = tip_of_spear; })
                                                            );

            var aura_of_vengeance_ability = aura_of_vengeance.GetComponent<AddFacts>().Facts[0] as BlueprintAbility;
            aura_of_vengeance_ability.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
        }


        static void createAntipaladinDeitySelection()
        {
            antipaladin_deity = library.CopyAndAdd<BlueprintFeatureSelection>("a7c8b73528d34c2479b4bd638503da1d", "AntipaladinDeitySelection", "");
            antipaladin_deity.AllFeatures = new BlueprintFeature[0];
            antipaladin_deity.Group = FeatureGroup.Deities;

            var deities = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4").AllFeatures;

            var allow_good = library.Get<BlueprintFeature>("882521af8012fc749930b03dc18a69de");
            var groetus = library.Get<BlueprintFeature>("c3e4d5681906d5246ab8b0637b98cbfe");

            antipaladin_deity.AllFeatures = deities.Where(d => d.GetComponents<AddFacts>().Aggregate(true, (val, af) => val = val && !af.Facts.Contains(allow_good))).ToArray().RemoveFromArray(groetus);
            antipaladin_deity.Features = antipaladin_deity.AllFeatures;
        }


        static BlueprintSpellbook createAntipaladinSpellbook()
        {
            var paladin_spellook = library.Get<BlueprintSpellbook>("bce4989b070ce924b986bf346f59e885");
            var antipaladin_spellbook = library.CopyAndAdd(paladin_spellook, "AntipaladinSpellbook", "");
            antipaladin_spellbook.Name = antipaladin_class.LocalizedName;
            antipaladin_spellbook.CharacterClass = antipaladin_class;          
            antipaladin_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            antipaladin_spellbook.SpellList.name = "AntipaladinSpelllist";
            library.AddAsset(antipaladin_spellbook.SpellList, "");
            antipaladin_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < antipaladin_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                antipaladin_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "8bc64d869456b004b9db255cdd1ea734", 1), //bane
                new Common.SpellId( "b7731c2b4fa1c9844a092329177be4c3", 1), //boneshaker
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( NewSpells.command.AssetGuid, 1),
                new Common.SpellId( "fbdd8c455ac4cde4a9a3e18c84af9485", 1), //doom
                new Common.SpellId( "e5af3674bb241f14b9a9f6b0c7dc3d27", 1), //inflict light wounds
                new Common.SpellId( NewSpells.magic_weapon.AssetGuid, 1),
                new Common.SpellId( "433b1faf4d02cc34abb0ade5ceda47c4", 1), //protection from alignment
                new Common.SpellId( NewSpells.savage_maw.AssetGuid, 1),
                new Common.SpellId( NewSpells.shadow_claws.AssetGuid, 1),
                //touch of blindness
                new Common.SpellId( "8fd74eddd9b6c224693d9ab241f25e84", 1), //summon monster I
                
                new Common.SpellId( NewSpells.blade_tutor.AssetGuid, 2), //acid arrow
                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 2), //blindness
                new Common.SpellId( "4c3d08935262b6544ae97599b3a9556d", 2), //bulls's strength
                new Common.SpellId( NewSpells.desecrate.AssetGuid, 2),
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagles splendor
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 2), //hold person
                new Common.SpellId( NewSpells.inflict_pain.AssetGuid, 2),
                new Common.SpellId( "89940cde01689fb46946b2f8cd7b66b7", 2), //invisibility
                new Common.SpellId( "c9198d9dfd2515d4ba98335b57bb66c7", 2), //litany of eloqeunce
                new Common.SpellId( "16f7754287811724abe1e0ead88f74ca", 2), //litany of entanglement
                new Common.SpellId( "dee3074b2fbfb064b80b973f9b56319e", 2), //pernicious poison
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( "82962a820ebc0e7408b8582fdc3f4c0c", 2), //sense vitals
                new Common.SpellId( NewSpells.silence.AssetGuid, 2),
                new Common.SpellId( "1724061e89c667045a6891179ee2e8e7", 2), //summon monster 2
                new Common.SpellId( NewSpells.touch_of_blood_letting.AssetGuid, 2),              
                new Common.SpellId( NewSpells.vine_strike.AssetGuid, 2),
                
                new Common.SpellId( NewSpells.accursed_glare.AssetGuid, 3), 
                new Common.SpellId( "4b76d32feb089ad4499c3a1ce8e1ac27", 3), //animate dead
                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 3), //bestow curse
                new Common.SpellId( "48e2744846ed04b4580be1a3343a5d3d", 3), //contagion
                new Common.SpellId( NewSpells.deadly_juggernaut.AssetGuid, 3),
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //diplacement instead of shield of darkness
                new Common.SpellId( "65f0b63c45ea82a4f8b8325768a3832d", 3), //inflict moderate wounds        
                new Common.SpellId( NewSpells.magic_weapon_greater.AssetGuid, 3),
                new Common.SpellId( NewSpells.second_wind.AssetGuid, 3),
                new Common.SpellId( "5d61dde0020bbf54ba1521f7ca0229dc", 3), //summon monster 3
                new Common.SpellId( "8a28a811ca5d20d49a863e832c31cce1", 3), //vampiric touch

                //banishing blade        
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( NewSpells.inflict_pain_mass.AssetGuid, 4),
                new Common.SpellId( "bd5da98859cf2b3418f6d68ea66cabbe", 4), //inflict serious wounds
                new Common.SpellId( "ecaa0def35b38f949bd1976a6c9539e0", 4), //invisibility greater
                new Common.SpellId( "435e73bcff18f304293484f9511b4672", 4), //lithany of madness
                new Common.SpellId( "4fbd47525382517419c66fb548fe9a67", 4), //slay living
                new Common.SpellId( "b56521d58f996cd4299dab3f38d5fe31", 4), //profane nimbus
                new Common.SpellId( "9047cb1797639924487ec0ad566a3fea", 4), //resounding blow
                new Common.SpellId( "7ed74a3ec8c458d4fb50b192fd7be6ef", 4), //summon monster 4
                //unholy sword
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(antipaladin_spellbook.SpellList, spell_id.level);
            }

            return antipaladin_spellbook;
        }
    }
}
