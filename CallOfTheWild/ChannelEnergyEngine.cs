using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class ChannelEnergyEngine
    {
        [Flags]
        public enum ChannelType
        {
            None = 0,
            PositiveHeal = 1,
            PositiveHarm = 2,
            NegativeHarm = 4,
            NegativeHeal = 8,
            PositiveSmite = 16,
            NegativeSmite = 32,
            BackToTheGrave = 64,
            All = ~None,
            NonSmite = PositiveHeal | PositiveHarm | NegativeHarm | NegativeHeal,
            Negative = NegativeHarm | NegativeHeal | NegativeSmite,
            Positive = PositiveHeal | PositiveHarm | PositiveSmite,
            Heal = PositiveHeal | NegativeHeal,
            Harm = PositiveHarm | NegativeHarm
        }

        public class ChannelEntry
        {
            public readonly BlueprintAbility ability;
            public readonly BlueprintFeature parent_feature;
            public readonly ChannelType channel_type;

            public ChannelEntry(string ability_guid, string parent_feature_guid, ChannelType type)
            {
                ability = library.Get<BlueprintAbility>(ability_guid);
                parent_feature = library.Get<BlueprintFeature>(parent_feature_guid);
                channel_type = type;
            }


            public ChannelEntry(BlueprintAbility channel_ability, BlueprintFeature channel_parent_feature, ChannelType type)
            {
                ability = channel_ability;
                parent_feature = channel_parent_feature;
                channel_type = type;
            }

            public bool scalesWithClass(BlueprintCharacterClass character_class)
            {
                var scaling = ability.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>();
                if (scaling == null)
                {
                    return false;
                }
                else
                {
                    return scaling.CharacterClasses.Contains(character_class);
                }
            }
        }



        static internal LibraryScriptableObject library => Main.library;
        static List<ChannelEntry> positive_heal = new List<ChannelEntry>{new ChannelEntry("574cf074e8b65e84d9b69a8c6f1af27b","7d49d7f590dc9a948b3bd1c8b7979854", ChannelType.PositiveHeal), //empyreal heal
                                                                         new ChannelEntry("6670f0f21a1d7f04db2b8b115e8e6abf", "cb6d55dda5ab906459d18a435994a760", ChannelType.PositiveHeal), //paladin heal
                                                                         new ChannelEntry("0c0cf7fcb356d2448b7d57f2c4db3c0c", "a9ab1bbc79ecb174d9a04699986ce8d5", ChannelType.PositiveHeal), //hospitalier heal
                                                                         new ChannelEntry("f5fc9a1a2a3c1a946a31b320d1dd31b2", "a79013ff4bcd4864cb669622a29ddafb", ChannelType.PositiveHeal) }; //cleric heal

        static List<ChannelEntry> positive_harm = new List<ChannelEntry>{new ChannelEntry("e1536ee240c5d4141bf9f9485a665128","7d49d7f590dc9a948b3bd1c8b7979854", ChannelType.PositiveHarm), //empyreal_harm
                                                                         new ChannelEntry("4937473d1cfd7774a979b625fb833b47", "cb6d55dda5ab906459d18a435994a760", ChannelType.PositiveHarm), //paladin harm
                                                                         new ChannelEntry("cc17243b2185f814aa909ac6b6599eaa", "a9ab1bbc79ecb174d9a04699986ce8d5", ChannelType.PositiveHarm), //hospitalier harm
                                                                         new ChannelEntry("279447a6bf2d3544d93a0a39c3b8e91d", "a79013ff4bcd4864cb669622a29ddafb", ChannelType.PositiveHarm) }; //cleric harm
        
        static List<ChannelEntry> negative_heal = new List<ChannelEntry> { new ChannelEntry("9be3aa47a13d5654cbcb8dbd40c325f2", "3adb2c906e031ee41a01bfc1d5fb7eea", ChannelType.NegativeHeal) };
        static List<ChannelEntry> negative_harm = new List<ChannelEntry> { new ChannelEntry("89df18039ef22174b81052e2e419c728", "3adb2c906e031ee41a01bfc1d5fb7eea", ChannelType.NegativeHarm) };

        static List<ChannelEntry> negative_smite = new List<ChannelEntry>();
        static List<ChannelEntry> positive_smite = new List<ChannelEntry>();
        static List<ChannelEntry> back_to_the_grave = new List<ChannelEntry>();

        static BlueprintFeature selective_channel = library.Get<BlueprintFeature>("fd30c69417b434d47b6b03b9c1f568ff");

        static Dictionary<string, string> normal_quick_channel_map = new Dictionary<string, string>();
        static Dictionary<string, string> normal_smite_map = new Dictionary<string, string>();


        static public BlueprintFeature quick_channel = null;
        static public BlueprintFeature channel_smite = null;
        static public BlueprintFeature improved_channel = null;
        static public BlueprintFeature channeling_scourge = null;

        static BlueprintBuff back_to_the_grave_buff = null;
        static BlueprintFeature back_to_the_grave_feature = null;

        static BlueprintFeature witch_channel_negative;
        static BlueprintFeature witch_channel_positive;




        public static List<ChannelEntry> getAllChannels(ChannelType type_mask = ChannelType.All)
        {
           
            List<ChannelEntry> entries = new List<ChannelEntry>();

            if ((type_mask & ChannelType.PositiveHeal) != ChannelType.None)
            {
                entries.AddRange(positive_heal);
            }

            if ((type_mask & ChannelType.PositiveHarm) != ChannelType.None)
            {
                entries.AddRange(positive_harm);
            }

            if ((type_mask & ChannelType.NegativeHeal) != ChannelType.None)
            {
                entries.AddRange(negative_heal);
            }

            if ((type_mask & ChannelType.NegativeHarm) != ChannelType.None)
            {
                entries.AddRange(negative_harm);
            }


            if ((type_mask & ChannelType.NegativeSmite) != ChannelType.None)
            {
                entries.AddRange(negative_smite);
            }


            if ((type_mask & ChannelType.PositiveSmite) != ChannelType.None)
            {
                entries.AddRange(positive_smite);
            }

            return entries;
        }


        public static List<BlueprintAbility> getChannelsByClass(ChannelType type_mask, BlueprintCharacterClass character_class)
        {
            var channels = getAllChannels(type_mask);

            List<BlueprintAbility> abilities = new List<BlueprintAbility>();

            foreach (var c in channels)
            {
                if (c.scalesWithClass(character_class))
                {
                    abilities.Add(c.ability);
                }
            }

            return abilities;
        }


        internal static void setWitchImprovedChannelHex(BlueprintFeature positive_channel, BlueprintFeature negative_channel)
        {
            witch_channel_negative = negative_channel;
            witch_channel_positive = positive_channel;

            var channels = getAllChannels();

            foreach (var c in channels)
            {
                addToWitchImprovedChannelHexScaling(c);
            }
        }

        static void addToWitchImprovedChannelHexScaling(ChannelEntry entry)
        {
            if (witch_channel_negative == null)
            {
                return;
            }
            if (!entry.scalesWithClass(Witch.witch_class))
            {
                return;
            }
            if ((entry.channel_type & ChannelType.Negative) != ChannelType.None)
            {
                var comp = witch_channel_negative.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
                comp.spells = comp.spells.AddToArray(entry.ability);
            }
            if ((entry.channel_type & ChannelType.Positive) != ChannelType.None)
            {
                var comp = witch_channel_positive.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
                comp.spells = comp.spells.AddToArray(entry.ability);
            }
        }

        internal static void addBackToTheGrave(BlueprintFeature feature, BlueprintBuff buff, BlueprintAbility prototype_ability)
        {
            back_to_the_grave_feature = feature;
            back_to_the_grave_buff = buff;

            back_to_the_grave_buff.AddComponent(Helpers.CreateAddFacts());

            var channels = getChannelsByClass(ChannelType.PositiveHeal, Warpriest.warpriest_class);
            var harm_undead_warpriest = getChannelsByClass(ChannelType.PositiveHarm, Warpriest.warpriest_class);
            ChannelEnergyEngine.updateItemsForChannelDerivative(harm_undead_warpriest[0], prototype_ability);

            foreach (var c in channels)
            {
                addBackToTheGraveEffect(c);
            }

            storeChannel(prototype_ability, feature, ChannelType.BackToTheGrave);
        }

        static void addToBackToTheGrave(BlueprintAbility ability)
        {
            if (back_to_the_grave_feature == null)
            {
                return;
            }

            var component = back_to_the_grave_buff.GetComponent<AddFacts>();
            component.Facts = component.Facts.AddToArray(ability);
        }


        static void addBackToTheGraveEffect(BlueprintAbility ability)
        {
            if (back_to_the_grave_feature == null)
            {
                return;
            }

            var caster_action = Helpers.Create<ContextActionOnContextCaster>(c =>
                                {
                                    var apply_buff = Common.createContextActionApplyBuff(back_to_the_grave_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1)), dispellable: false);

                                    c.Actions = Helpers.CreateActionList(Helpers.CreateConditional(Common.createContextConditionCasterHasFact(back_to_the_grave_feature), apply_buff));
                                }
                                                                            );
            ability.ReplaceComponent<AbilityEffectRunAction>(c => c.Actions = Helpers.CreateActionList(c.Actions.Actions.AddToArray(caster_action)));
        }


        internal static void createChannelingScourge()
        {
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var inquisitor = library.Get<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            var cleric_channel = library.Get<BlueprintFeatureSelection>("d332c1748445e8f4f9e92763123e31bd");

            var harm_undead = library.Get<BlueprintAbility>("279447a6bf2d3544d93a0a39c3b8e91d");
            var harm_living = library.Get<BlueprintAbility>("89df18039ef22174b81052e2e419c728");


            var channels = getChannelsByClass(ChannelType.Harm, cleric);

            channeling_scourge = Helpers.CreateFeature("ChannelingScourgeFeature",
                                                       "Channeling Scourge",
                                                       "When you use channel energy to deal damage, your inquisitor levels count as cleric levels for determining the number of damage dice and the saving throw DC",
                                                       "",
                                                       null,
                                                       FeatureGroup.Feat,
                                                        Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c =>
                                                        {
                                                            c.spells = channels.ToArray();
                                                            c.value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
                                                        }
                                                       ),
                                                        Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c =>
                                                        {
                                                            c.spells = channels.ToArray();
                                                            c.value = Helpers.CreateContextValue(AbilityRankType.DamageBonus);
                                                            c.multiplier = -1;
                                                        }
                                                       ),
                                                       Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                       classes: new BlueprintCharacterClass[] { inquisitor, cleric },
                                                                                       type: AbilityRankType.StatBonus
                                                                                       ),
                                                       Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                       classes: new BlueprintCharacterClass[] { cleric },
                                                                                       type: AbilityRankType.DamageBonus
                                                                                       ),
                                                       Helpers.PrerequisiteClassLevel(cleric, 1),
                                                       Helpers.PrerequisiteClassLevel(inquisitor, 1),
                                                       Helpers.PrerequisiteFeature(cleric_channel)
                                                       );
            library.AddFeats(channeling_scourge);
        }


        static void addToChannelingScourge(ChannelEntry c)
        {
            if (channeling_scourge == null)
            {
                return;
            }
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            if ((c.channel_type & ChannelType.Harm) != ChannelType.None && c.scalesWithClass(cleric))
            {
                var components = channeling_scourge.GetComponents<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
                foreach (var component in components)
                {
                    component.spells = component.spells.AddToArray(c.ability);
                }
            }
        }


        static void addToImprovedChannel(BlueprintAbility ability, BlueprintFeature parent_feature)
        {
            if (improved_channel == null)
            {
                return;
            }
            var prereq = improved_channel.GetComponent<PrerequisiteFeaturesFromList>();
            if (!prereq.Features.Contains(parent_feature))
            {
                prereq.Features = prereq.Features.AddToArray(parent_feature);
            }

            var abilities = improved_channel.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();

            if (!abilities.spells.Contains(ability))
            {
                abilities.spells = abilities.spells.AddToArray(ability);
            }
        }


        public static void createImprovedChannel()
        {
            improved_channel = Helpers.CreateFeature("ImprovedChannelFeature",
                                                     "Improved Channel",
                                                     "Add 2 to the DC of saving throws made to resist the effects of your channel energy ability.",
                                                     "",
                                                     null,
                                                     FeatureGroup.Feat,
                                                     Helpers.PrerequisiteFeaturesFromList(),
                                                     Helpers.Create<NewMechanics.IncreaseSpecifiedSpellsDC>(c => { c.BonusDC = 2; c.spells = new BlueprintAbility[0]; })
                                                     );

            var channels = getAllChannels();

            foreach (var c in channels)
            {
                addToImprovedChannel(c.ability, c.parent_feature);
            }

            library.AddFeats(improved_channel);
        }

        public static void createChannelSmite()
        {
            var resounding_blow = library.Get<BlueprintAbility>("9047cb1797639924487ec0ad566a3fea");
            channel_smite = Helpers.CreateFeature("ChannelSmiteFeature",
                                      "Channel Smite",
                                      "Before you make a melee attack roll, you can choose to spend one use of your channel energy ability as a swift action. If you channel positive energy and you hit an undead creature, that creature takes an amount of additional damage equal to the damage dealt by your channel positive energy ability. If you channel negative energy and you hit a living creature, that creature takes an amount of additional damage equal to the damage dealt by your channel negative energy ability. Your target can make a Will save, as normal, to halve this additional damage. If your attack misses, the channel energy ability is still expended with no effect.",
                                      "",
                                      resounding_blow.Icon,
                                      FeatureGroup.Feat);
            channel_smite.Groups = channel_smite.Groups.AddToArray(FeatureGroup.CombatFeat);

            var channels = getAllChannels(ChannelType.PositiveHarm | ChannelType.NegativeHarm);

            foreach (var c in channels)
            {
                addToChannelSmite(c.ability, c.parent_feature, c.channel_type);
            }


            library.AddCombatFeats(channel_smite);
        }

        static void addToChannelSmite(BlueprintAbility channel, BlueprintFeature parent_feature, ChannelType channel_type)
        {
            if (channel_smite == null)
            {
                return;
            }

            if (!(channel_type == ChannelType.NegativeHarm || channel_type == ChannelType.PositiveHarm))
            {
                return;
            }

            if (channel.ActionType != Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard)
            {
                return;// do not add smite on quick channel
            }

            Common.addFeaturePrerequisiteOr(channel_smite, parent_feature);

            var resounding_blow = library.Get<BlueprintAbility>("9047cb1797639924487ec0ad566a3fea");
            var smite_evil = library.Get<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec");
            var buff = Helpers.CreateBuff("ChannelSmite" + channel.name + "Buff",
                                          $"Channel Smite ({channel.Name})",
                                          channel_smite.Description,
                                          Helpers.MergeIds(channel.AssetGuid, "0d406cf592524c85b796216ed4ee3ab3"),
                                          resounding_blow.Icon,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(channel.GetComponent<AbilityEffectRunAction>().Actions,
                                                                                           check_weapon_range_type: true),
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()),
                                                                                           check_weapon_range_type: true,
                                                                                           only_hit: false,
                                                                                           on_initiator: true),
                                           channel.GetComponent<ContextRankConfig>()
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1), Kingmaker.UnitLogic.Mechanics.DurationRate.Rounds),
                                                                 dispellable: false
                                                                 );

            var ability = Helpers.CreateAbility("ChannelSmite" + channel.name,
                                                buff.Name,
                                                buff.Description,
                                                Helpers.MergeIds(channel.AssetGuid, "81e5fc81f1a644d5898a9fdbda752e95"),
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneRoundDuration,
                                                channel.LocalizedSavingThrow,
                                                smite_evil.GetComponent<AbilitySpawnFx>(),
                                                channel.GetComponent<AbilityResourceLogic>(),
                                                Helpers.CreateRunActions(apply_buff),
                                                channel.GetComponent<ContextRankConfig>(),
                                                channel.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>()
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            updateItemsForChannelDerivative(channel, ability);

            var caster_alignment = channel.GetComponent<AbilityCasterAlignment>();
            if (caster_alignment != null)
            {
                ability.AddComponent(caster_alignment);
            }

            var smite_feature = Common.AbilityToFeature(ability, guid: Helpers.MergeIds(ability.AssetGuid, parent_feature.AssetGuid));

            channel_smite.AddComponent(Common.createAddFeatureIfHasFact(parent_feature, smite_feature));
            parent_feature.AddComponent(Common.createAddFeatureIfHasFact(channel_smite, smite_feature));

            normal_smite_map.Add(channel.AssetGuid, ability.AssetGuid);

            storeChannel(ability, smite_feature, channel_type == ChannelType.NegativeHarm ? ChannelType.NegativeSmite : ChannelType.PositiveSmite);
        }



        public static void createQuickChannel()
        {
            quick_channel = Helpers.CreateFeature("QuickChannelFeature",
                                                  "Quick Channel",
                                                  "You may channel energy as a move action by spending 2 daily uses of that ability.",
                                                  "",
                                                  LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Channel_Quick.png"),
                                                  FeatureGroup.Feat,
                                                  Helpers.PrerequisiteStatValue(Kingmaker.EntitySystem.Stats.StatType.SkillLoreReligion, 5));

            var channels = getAllChannels(ChannelType.NonSmite);

            foreach (var c in channels)
            {
                addToQuickChannel(c.ability, c.parent_feature, c.channel_type);
            }

            library.AddFeats(quick_channel);
        }


        static void addToQuickChannel(BlueprintAbility channel, BlueprintFeature parent_feature, ChannelType channel_type)
        {
            if (quick_channel == null)
            {
                return;
            }

            if (channel.ActionType != Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard)
            {
                return;
            }

            Common.addFeaturePrerequisiteOr(quick_channel, parent_feature);

            var quicken_ability = library.CopyAndAdd<BlueprintAbility>(channel.AssetGuid, "Quick" + channel.name, channel.AssetGuid, "e936d73a1dfe42efb1765b980c80e113");
            quicken_ability.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Move;
            quicken_ability.SetName(quicken_ability.Name + $" ({quick_channel.Name})");
            var resource_logic = quicken_ability.GetComponent<AbilityResourceLogic>();
            var amount = resource_logic.Amount;
            quicken_ability.ReplaceComponent<AbilityResourceLogic>(c => { c.Amount = amount * 2;});
            updateItemsForChannelDerivative(channel, quicken_ability);

            var quicken_feature = Common.AbilityToFeature(quicken_ability, guid: Helpers.MergeIds(quicken_ability.AssetGuid, parent_feature.AssetGuid));
            quick_channel.AddComponent(Common.createAddFeatureIfHasFact(parent_feature, quicken_feature));
            parent_feature.AddComponent(Common.createAddFeatureIfHasFact(quick_channel, quicken_feature));

            storeChannel(quicken_ability, quicken_feature, channel_type);

            normal_quick_channel_map.Add(channel.AssetGuid, quicken_ability.AssetGuid);
        }


        public static BlueprintAbility getQuickChannelVariant(BlueprintAbility normal_channel_ability)
        {
            if (quick_channel == null)
            {
                return null;
            }
            return library.Get<BlueprintAbility>(normal_quick_channel_map[normal_channel_ability.AssetGuid]);
        }


        public static BlueprintAbility getChannelSmiteVariant(BlueprintAbility normal_channel_ability)
        {
            if (channel_smite == null)
            {
                return null;
            }
            return library.Get<BlueprintAbility>(normal_smite_map[normal_channel_ability.AssetGuid]);
        }

        public static BlueprintAbility createChannelEnergy(ChannelType channel_type, string name, string dispaly_name, string description, string guid, BlueprintFeature parent_feature, 
                                                           ContextRankConfig rank_config, NewMechanics.ContextCalculateAbilityParamsBasedOnClasses dc_scaling,
                                                           AbilityResourceLogic resource_logic, bool update_items = false)
        {
            string original_guid = "";
            BlueprintAbility prototype = null;
            switch (channel_type)
            {
                case ChannelType.PositiveHeal:
                    original_guid = "f5fc9a1a2a3c1a946a31b320d1dd31b2";
                    prototype = positive_heal[0].ability;
                    break;
                case ChannelType.PositiveHarm:
                    original_guid = "279447a6bf2d3544d93a0a39c3b8e91d";
                    prototype = positive_harm[0].ability;
                    break;
                case ChannelType.NegativeHarm:
                    original_guid = "89df18039ef22174b81052e2e419c728";
                    prototype = negative_harm[0].ability;
                    break;
                case ChannelType.NegativeHeal:
                    original_guid = "9be3aa47a13d5654cbcb8dbd40c325f2";
                    prototype = negative_heal[0].ability;
                    break;
            }

            var ability = library.CopyAndAdd<BlueprintAbility>(original_guid, name, guid);
            if (dispaly_name != "")
            {
                ability.SetName(dispaly_name);
            }

            if (description != "")
            {
                ability.SetDescription(description);
            }

            ability.ReplaceComponent<ContextRankConfig>(rank_config);
            ability.ReplaceComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(dc_scaling);

            if (resource_logic != null)
            {
                ability.ReplaceComponent<AbilityResourceLogic>(resource_logic);
            }
            else
            {
                ability.RemoveComponents<AbilityResourceLogic>();
            }



            if (update_items)
            {
                updateItemsForChannelDerivative(prototype, ability);
            }


            storeChannel(ability, parent_feature, channel_type);
            addToChannelSmite(ability, parent_feature, channel_type);
            addToQuickChannel(ability, parent_feature, channel_type);
            addToSelectiveChannel(parent_feature);

            return ability;
        }



        static void storeChannel(BlueprintAbility ability, BlueprintFeature parent_feature, ChannelType channel_type)
        {
            var entry = new ChannelEntry(ability, parent_feature, channel_type);
            switch (channel_type)
            {
                case ChannelType.PositiveHeal:
                    positive_heal.Add(entry);
                    break;
                case ChannelType.PositiveHarm:
                    positive_harm.Add(entry);
                    break;
                case ChannelType.NegativeHarm:
                    negative_harm.Add(entry);
                    break;
                case ChannelType.NegativeHeal:
                    negative_heal.Add(entry);
                    break;
                case ChannelType.NegativeSmite:
                    negative_smite.Add(entry);
                    break;
                case ChannelType.PositiveSmite:
                    positive_smite.Add(entry);
                    break;
                case ChannelType.BackToTheGrave:
                    back_to_the_grave.Add(entry);
                    addToBackToTheGrave(entry.ability);
                    break;
            }

            addToImprovedChannel(ability, parent_feature);
            addToChannelingScourge(entry);
            addToWitchImprovedChannelHexScaling(entry);

            if (entry.scalesWithClass(Warpriest.warpriest_class) && entry.channel_type == ChannelType.PositiveHeal)
            {
                addBackToTheGraveEffect(entry.ability);
            }
        }


        static void addToSelectiveChannel(BlueprintFeature parent_feature)
        {
            selective_channel.AddComponent(Helpers.PrerequisiteFeature(parent_feature, true));
        }


        public static BlueprintFeature createExtraChannelFeat(BlueprintAbility ability, BlueprintFeature parent_feature, string name, string display_name, string guid)
        {
            var extra_channel = library.CopyAndAdd<BlueprintFeature>("cd9f19775bd9d3343a31a065e93f0c47", name, guid);
            extra_channel.ReplaceComponent<Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteFeature>(Helpers.PrerequisiteFeature(parent_feature));
            extra_channel.SetName(display_name);

            var resource_logic = ability.GetComponent<AbilityResourceLogic>();
            var resource = resource_logic.RequiredResource;
            var amount = resource_logic.Amount;
            extra_channel.ReplaceComponent<IncreaseResourceAmount>(c => { c.Value = amount * 2; c.Resource = resource; });
            extra_channel.ReplaceComponent<PrerequisiteFeature>(c => { c.Feature = parent_feature; });

            //update items
            var ring_of_energy_source_feature = library.Get<BlueprintFeature>("c372cd498006fcf4ab4c9ed6b92515a9");
            ring_of_energy_source_feature.AddComponent(extra_channel.GetComponent<IncreaseResourceAmount>());

            library.AddFeats(extra_channel);
            return extra_channel;
        }


        static public void updateItemsFeature(ChannelType channel_type, BlueprintFeature feature)
        {
            //phylacteries bonuses
            var negative_bonus1 = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("60f06749fa4729c49bc3eb2eb7e3b316");
            var positive_bonus1 = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("f5d0bf8c1b4574848acb8d1fbb544807");
            var negative_bonus2 = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("cb4a39044b59f5e47ad5bc08ff9d6669");
            var positive_bonus2 = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("e988cf802d403d941b2ed8b6016de68f");

            var linnorm_buff = library.Get<BlueprintBuff>("b5ebb94df76531c4ca4f13bfd91efd4e");

            if ((channel_type | ChannelType.PositiveHeal) >0 || (channel_type | ChannelType.PositiveHarm) >0)
            {
                Common.addFeatureToEnchantment(positive_bonus1, feature);
                Common.addFeatureToEnchantment(positive_bonus2, feature);
                Common.addFeatureToEnchantment(positive_bonus2, feature);
                linnorm_buff.AddComponent(Helpers.CreateAddFact(feature));
                linnorm_buff.AddComponent(Helpers.CreateAddFact(feature));
            }
            else if ((channel_type | ChannelType.NegativeHarm) > 0 || (channel_type | ChannelType.NegativeHeal) > 0)
            {
                Common.addFeatureToEnchantment(negative_bonus1, feature);
                Common.addFeatureToEnchantment(negative_bonus2, feature);
                Common.addFeatureToEnchantment(negative_bonus2, feature);
            }
        }


        


        static internal void updateItemsForChannelDerivative(BlueprintAbility original_ability, BlueprintAbility derived_ability)
        {
            var config = derived_ability.GetComponent<ContextRankConfig>();

            ContextRankProgression progression = Helpers.GetField<ContextRankProgression>(config, "m_Progression");
            int step = Helpers.GetField<int>(config, "m_StepLevel");
            int level_scale = (progression == ContextRankProgression.OnePlusDivStep || progression == ContextRankProgression.DivStep || progression == ContextRankProgression.StartPlusDivStep)
                                    ? step : 2;
            //phylacteries bonuses
            BlueprintEquipmentEnchantment[] enchants = new BlueprintEquipmentEnchantment[]{library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("60f06749fa4729c49bc3eb2eb7e3b316"),
                                                                                  library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("f5d0bf8c1b4574848acb8d1fbb544807"),
                                                                                  library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("cb4a39044b59f5e47ad5bc08ff9d6669"),
                                                                                  library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("e988cf802d403d941b2ed8b6016de68f"),
                                                                                 };
            
            foreach (var e in enchants)
            {
                var boni = e.GetComponents<AddCasterLevelEquipment>().ToArray();
                foreach (var b in boni)
                {
                    if (b.Spell == original_ability)
                    {
                        var b2 = b.CreateCopy();
                        b2.Spell = derived_ability;
                        b2.Bonus = boni[0].Bonus / 2 * level_scale;
                        e.AddComponent(b2);
                    }
                }
            }


            BlueprintBuff[] buffs = new BlueprintBuff[] { library.Get<BlueprintBuff>("b5ebb94df76531c4ca4f13bfd91efd4e") };// camp dish buff

            foreach (var buff in buffs)
            {
                var boni = buff.GetComponents<AddCasterLevelForAbility>().ToArray();
                foreach (var b in boni)
                {
                    if (b.Spell == original_ability)
                    {
                        var b2 = b.CreateCopy();
                        b2.Spell = derived_ability;
                        b2.Bonus = boni[0].Bonus / 2 * level_scale;
                        buff.AddComponent(b2);
                    }
                }
            }

        }

    }
}
