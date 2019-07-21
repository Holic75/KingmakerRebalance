using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
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
            PositiveHeal = 1,
            PositiveHarm = 2,
            NegativeHarm = 4,
            NegativeHeal = 8
        }

        public class ChannelEntry
        {
            public BlueprintAbility ability;
            public BlueprintFeature parent_feature;

            public ChannelEntry(string ability_guid, string parent_feature_guid)
            {
                ability = library.Get<BlueprintAbility>(ability_guid);
                parent_feature = library.Get<BlueprintFeature>(parent_feature_guid);
            }


            public ChannelEntry(BlueprintAbility channel_ability, BlueprintFeature channel_parent_feature)
            {
                ability = channel_ability;
                parent_feature = channel_parent_feature;
            }
        }



        static internal LibraryScriptableObject library => Main.library;
        static List<ChannelEntry> positive_heal = new List<ChannelEntry>{new ChannelEntry("574cf074e8b65e84d9b69a8c6f1af27b","7d49d7f590dc9a948b3bd1c8b7979854"), //empyreal heal
                                                                         new ChannelEntry("6670f0f21a1d7f04db2b8b115e8e6abf", "cb6d55dda5ab906459d18a435994a760"), //paladin heal
                                                                         new ChannelEntry("0c0cf7fcb356d2448b7d57f2c4db3c0c", "a9ab1bbc79ecb174d9a04699986ce8d5"), //hospitalier heal
                                                                         new ChannelEntry("f5fc9a1a2a3c1a946a31b320d1dd31b2", "a79013ff4bcd4864cb669622a29ddafb") }; //cleric heal

        static List<ChannelEntry> positive_harm = new List<ChannelEntry>{new ChannelEntry("e1536ee240c5d4141bf9f9485a665128","7d49d7f590dc9a948b3bd1c8b7979854"), //empyreal_harm
                                                                         new ChannelEntry("4937473d1cfd7774a979b625fb833b47", "cb6d55dda5ab906459d18a435994a760"), //paladin harm
                                                                         new ChannelEntry("cc17243b2185f814aa909ac6b6599eaa", "a9ab1bbc79ecb174d9a04699986ce8d5"), //hospitalier harm
                                                                         new ChannelEntry("279447a6bf2d3544d93a0a39c3b8e91d", "a79013ff4bcd4864cb669622a29ddafb") }; //cleric harm
        
        static List<ChannelEntry> negative_heal = new List<ChannelEntry> { new ChannelEntry("9be3aa47a13d5654cbcb8dbd40c325f2", "a79013ff4bcd4864cb669622a29ddafb") };
        static List<ChannelEntry> negative_harm = new List<ChannelEntry> { new ChannelEntry("89df18039ef22174b81052e2e419c728", "a79013ff4bcd4864cb669622a29ddafb") };

        static BlueprintFeature selective_channel = library.Get<BlueprintFeature>("fd30c69417b434d47b6b03b9c1f568ff");


        static BlueprintFeature quick_channel = null;


        public static void createQuickChannel()
        {
            quick_channel = Helpers.CreateFeature("QuickChannelFeature",
                                                  "Quick Channel",
                                                  "You may channel energy as a move action by spending 2 daily uses of that ability.",
                                                  "",
                                                  null,
                                                  FeatureGroup.Feat,
                                                  Helpers.PrerequisiteStatValue(Kingmaker.EntitySystem.Stats.StatType.SkillLoreReligion, 5));
            foreach (var e in positive_heal.ToArray())
            {
                addToQuickChannel(e.ability, e.parent_feature, ChannelType.PositiveHeal);
            }

            foreach (var e in positive_harm.ToArray())
            {
                addToQuickChannel(e.ability, e.parent_feature, ChannelType.PositiveHarm);
            }

            foreach (var e in negative_harm.ToArray())
            {
                addToQuickChannel(e.ability, e.parent_feature, ChannelType.NegativeHarm);
            }

            foreach (var e in negative_heal.ToArray())
            {
                addToQuickChannel(e.ability, e.parent_feature, ChannelType.NegativeHeal);
            }

            library.AddFeats(quick_channel);
        }


        static void addToQuickChannel(BlueprintAbility channel, BlueprintFeature parent_feature, ChannelType channel_type)
        {
            if (quick_channel == null)
            {
                return;
            }

            var features_from_list = quick_channel.GetComponent<PrerequisiteFeaturesFromList>();
            if (features_from_list == null)
            {
                features_from_list = Helpers.PrerequisiteFeaturesFromList(parent_feature);
                quick_channel.AddComponent(features_from_list);
            }

            if (!features_from_list.Features.Contains(parent_feature))
            {
                features_from_list.Features = features_from_list.Features.AddToArray(parent_feature);
            }


            var quicken_ability = library.CopyAndAdd<BlueprintAbility>(channel.AssetGuid, "Quick" + channel.name, channel.AssetGuid, "e936d73a1dfe42efb1765b980c80e113");
            quicken_ability.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Move;
            quicken_ability.SetName(quicken_ability.Name + $" ({quick_channel.Name})");
            var resource_logic = quicken_ability.GetComponent<AbilityResourceLogic>();
            var amount = resource_logic.Amount;
            quicken_ability.ReplaceComponent<AbilityResourceLogic>(c => { c.Amount = amount * 2;});
            updateItemsForQuick(channel, quicken_ability);

            var quicken_feature = Common.AbilityToFeature(quicken_ability, guid: Helpers.MergeIds(quicken_ability.AssetGuid, parent_feature.AssetGuid));
            quick_channel.AddComponent(Common.createAddFeatureIfHasFact(parent_feature, quicken_feature));
            parent_feature.AddComponent(Common.createAddFeatureIfHasFact(quick_channel, quicken_feature));

            storeChannel(quicken_ability, parent_feature, channel_type);
        }

        public static BlueprintAbility createChannelEnergy(ChannelType channel_type, string name, string guid, BlueprintFeature parent_feature, 
                                                           ContextRankConfig rank_config = null, AbilityResourceLogic resource_logic = null, bool update_items = false)
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

            if (rank_config != null)
            {
                ability.ReplaceComponent<ContextRankConfig>(rank_config);
            }

            if (resource_logic != null)
            {
                ability.ReplaceComponent<AbilityResourceLogic>(resource_logic);
            }

            if (update_items)
            {
                updateItemsForQuick(ability, prototype);
            }


            storeChannel(ability, parent_feature, channel_type);
            addToQuickChannel(ability, parent_feature, channel_type);
            addToSelectiveChannel(parent_feature);

            return ability;
        }


        static void storeChannel(BlueprintAbility ability, BlueprintFeature parent_feature, ChannelType channel_type)
        {
            switch (channel_type)
            {
                case ChannelType.PositiveHeal:
                    positive_heal.Add(new ChannelEntry(ability, parent_feature));
                    break;
                case ChannelType.PositiveHarm:
                    positive_harm.Add(new ChannelEntry(ability, parent_feature));
                    break;
                case ChannelType.NegativeHarm:
                    negative_harm.Add(new ChannelEntry(ability, parent_feature));
                    break;
                case ChannelType.NegativeHeal:
                    negative_heal.Add(new ChannelEntry(ability, parent_feature));
                    break;
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

            library.AddFeats(extra_channel);
            return extra_channel;
        }


        static public void updateItems(ChannelType channel_type, params BlueprintComponent[] components_to_add)
        {
            //phylacteries bonuses
            var negative_bonus1 = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("60f06749fa4729c49bc3eb2eb7e3b316");
            var positive_bonus1 = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("f5d0bf8c1b4574848acb8d1fbb544807");
            var negative_bonus2 = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("cb4a39044b59f5e47ad5bc08ff9d6669");
            var positive_bonus2 = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintEquipmentEnchantment>("e988cf802d403d941b2ed8b6016de68f");

            foreach (var c in components_to_add)
            {
                if ((channel_type | ChannelType.PositiveHeal) >0 || (channel_type | ChannelType.PositiveHarm) >0)
                {
                    positive_bonus1.AddComponent(c);
                    positive_bonus2.AddComponent(c);
                    positive_bonus2.AddComponent(c);
                }
                else if ((channel_type | ChannelType.NegativeHarm) > 0 || (channel_type | ChannelType.NegativeHeal) > 0)
                {
                    negative_bonus1.AddComponent(c);
                    negative_bonus2.AddComponent(c);
                    negative_bonus2.AddComponent(c);
                }
            }
        }


        static void updateItemsForQuick(BlueprintAbility original_ability, BlueprintAbility quicken_ability)
        {
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
                        b2.Spell = quicken_ability;
                        e.AddComponent(b2);
                    }
                }
            }
        }

    }
}
