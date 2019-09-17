using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Alignments;
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

    public static partial class Extensions
    {
        public static bool isOf(this ChannelEnergyEngine.ChannelType this_type, ChannelEnergyEngine.ChannelType channel_type)
        {
            return (this_type & channel_type) != 0;
        }


        public static bool isNotOf(this ChannelEnergyEngine.ChannelType this_type, ChannelEnergyEngine.ChannelType channel_type)
        {
            return (this_type & channel_type) == 0;
        }


        public static bool isOnly(this ChannelEnergyEngine.ChannelType this_type, ChannelEnergyEngine.ChannelType channel_type)
        {
            return (this_type & ~channel_type) == 0;
        }


        public static bool isBase(this ChannelEnergyEngine.ChannelType this_type)
        {

            return this_type == ChannelEnergyEngine.ChannelType.PositiveHeal || this_type == ChannelEnergyEngine.ChannelType.NegativeHeal
                   || this_type == ChannelEnergyEngine.ChannelType.PositiveHarm || this_type == ChannelEnergyEngine.ChannelType.NegativeHarm;
        }
    }

    public static class ChannelEnergyEngine
    {
        [Flags]
        public enum ChannelType
        {
            None = 0,
            Positive = 1,
            Negative = 2,
            Heal = 4,
            Harm = 8,
            Smite = 16,
            Quick = 32,
            BackToTheGrave = 64,
            Cone = 128,
            Line = 256,
            HolyVindicatorShield = 512,
            Form = Cone | Line,
            PositiveHeal = Positive | Heal,
            PositiveHarm = Positive | Harm,
            NegativeHeal = Negative | Heal,
            NegativeHarm = Negative | Harm,
            All = ~None
        }

        public class ChannelEntry
        {
            public readonly BlueprintAbility ability;
            public readonly BlueprintAbility base_ability;
            public readonly BlueprintFeature parent_feature;
            public readonly ChannelType channel_type;

            public ChannelEntry(string ability_guid, string parent_feature_guid, ChannelType type)
            {
                ability = library.Get<BlueprintAbility>(ability_guid);
                parent_feature = library.Get<BlueprintFeature>(parent_feature_guid);
                base_ability = ability.Parent;
                channel_type = type;
            }


            public ChannelEntry(BlueprintAbility channel_ability, BlueprintFeature channel_parent_feature, ChannelType type)
            {
                ability = channel_ability;
                parent_feature = channel_parent_feature;
                base_ability = ability.Parent;
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
        static List<ChannelEntry> channel_entires = new List<ChannelEntry>();

        static BlueprintFeature selective_channel = library.Get<BlueprintFeature>("fd30c69417b434d47b6b03b9c1f568ff");

        static Dictionary<string, string> normal_quick_channel_map = new Dictionary<string, string>();
        static Dictionary<string, string> normal_smite_map = new Dictionary<string, string>();


        static public BlueprintFeature quick_channel = null;
        static public BlueprintFeature channel_smite = null;
        static public BlueprintFeature improved_channel = null;
        static public BlueprintFeature channeling_scourge = null;

        static BlueprintFeature witch_channel_negative;
        static BlueprintFeature witch_channel_positive;

        static BlueprintAbility back_to_the_grave_base = null;
        static public BlueprintFeature versatile_channeler = null;
        static internal BlueprintFeature versatile_channeler_positive = null;
        static internal BlueprintFeature versatile_channeler_negative = null;
        static internal BlueprintFeature versatile_channeler_positive_warpriest = null;
        static internal BlueprintFeature versatile_channeler_negative_warpriest = null;

        static internal BlueprintFeature holy_vindicator_shield = null;
        static readonly public SpellDescriptor holy_vindicator_shield_descriptor = (SpellDescriptor)0x0004000000000000;


        static BlueprintFeature stigmata_feature = null;


        internal static void init()
        {
            var empyreal_sorc_channel_feature = library.Get<BlueprintFeature>("7d49d7f590dc9a948b3bd1c8b7979854");
            var cleric_negative_channel_feature = library.Get<BlueprintFeature>("3adb2c906e031ee41a01bfc1d5fb7eea");
            var cleric_positve_channel_feature = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var paladin_channel_feature = library.Get<BlueprintFeature>("cb6d55dda5ab906459d18a435994a760");
            var hospitalier_channel_feature = library.Get<BlueprintFeature>("a9ab1bbc79ecb174d9a04699986ce8d5");

            var empyreal_sorc_positive_heal = library.Get<BlueprintAbility>("574cf074e8b65e84d9b69a8c6f1af27b");
            var empyreal_sorc_positive_harm = library.Get<BlueprintAbility>("e1536ee240c5d4141bf9f9485a665128");
            var cleric_positive_heal = library.Get<BlueprintAbility>("f5fc9a1a2a3c1a946a31b320d1dd31b2");
            var cleric_positive_harm = library.Get<BlueprintAbility>("279447a6bf2d3544d93a0a39c3b8e91d");
            var cleric_negative_heal = library.Get<BlueprintAbility>("9be3aa47a13d5654cbcb8dbd40c325f2");
            var cleric_negative_harm = library.Get<BlueprintAbility>("89df18039ef22174b81052e2e419c728");

            var paladin_positive_heal = library.Get<BlueprintAbility>("6670f0f21a1d7f04db2b8b115e8e6abf");
            var paladin_positive_harm = library.Get<BlueprintAbility>("4937473d1cfd7774a979b625fb833b47");
            var hospitalier_positive_heal = library.Get<BlueprintAbility>("0c0cf7fcb356d2448b7d57f2c4db3c0c");
            var hospitalier_positive_harm = library.Get<BlueprintAbility>("cc17243b2185f814aa909ac6b6599eaa");

            var empyreal_sorc_positive_heal_base = Common.createVariantWrapper("EmpyrealSorcerorPositiveChannelHealBase", "", empyreal_sorc_positive_heal);
            var empyreal_sorc_positive_harm_base = Common.createVariantWrapper("EmpyrealSorcerorPositiveChannelHarmBase", "", empyreal_sorc_positive_harm);

            var cleric_positive_heal_base = Common.createVariantWrapper("ClericPositiveChannelHealBase", "", cleric_positive_heal);
            var cleric_positive_harm_base = Common.createVariantWrapper("ClericPositiveChannelHarmBase", "", cleric_positive_harm);
            var cleric_negative_heal_base = Common.createVariantWrapper("ClericNegativeChannelHealBase", "", cleric_negative_heal);
            var cleric_negative_harm_base = Common.createVariantWrapper("ClericNegativeChannelHarmBase", "", cleric_negative_harm);

            var paladin_positive_heal_base = Common.createVariantWrapper("PaladinPositiveChannelHealBase", "", paladin_positive_heal);
            var paladin_positive_harm_base = Common.createVariantWrapper("PaladinPositiveChannelHarmBase", "", paladin_positive_harm);

            var hospitalier_positive_heal_base = Common.createVariantWrapper("HospitalierPositiveChannelHealBase", "", hospitalier_positive_heal);
            var hospitalier_positive_harm_base = Common.createVariantWrapper("HospitalierPositiveChannelHarmBase", "", hospitalier_positive_harm);


            empyreal_sorc_channel_feature.GetComponent<AddFacts>().Facts = new BlueprintUnitFact[] { empyreal_sorc_positive_heal_base, empyreal_sorc_positive_harm_base };
            paladin_channel_feature.GetComponent<AddFacts>().Facts = new BlueprintUnitFact[] { paladin_positive_heal_base, paladin_positive_harm_base };
            hospitalier_channel_feature.GetComponent<AddFacts>().Facts = new BlueprintUnitFact[] { hospitalier_positive_heal_base, hospitalier_positive_harm_base };

            cleric_positve_channel_feature.GetComponent<AddFacts>().Facts[1] = cleric_positive_heal_base;
            cleric_positve_channel_feature.GetComponent<AddFacts>().Facts[2] = cleric_positive_harm_base;
            cleric_negative_channel_feature.GetComponent<AddFacts>().Facts[1] = cleric_negative_heal_base;
            cleric_negative_channel_feature.GetComponent<AddFacts>().Facts[2] = cleric_negative_harm_base;

            storeChannel(empyreal_sorc_positive_heal, empyreal_sorc_channel_feature, ChannelType.PositiveHeal);
            storeChannel(empyreal_sorc_positive_harm, empyreal_sorc_channel_feature, ChannelType.PositiveHarm);

            storeChannel(paladin_positive_heal, paladin_channel_feature, ChannelType.PositiveHeal);
            storeChannel(paladin_positive_harm, paladin_channel_feature, ChannelType.PositiveHarm);

            storeChannel(hospitalier_positive_heal, hospitalier_channel_feature, ChannelType.PositiveHeal);
            storeChannel(hospitalier_positive_harm, hospitalier_channel_feature, ChannelType.PositiveHarm);

            storeChannel(cleric_positive_heal, cleric_positve_channel_feature, ChannelType.PositiveHeal);
            storeChannel(cleric_positive_harm, cleric_positve_channel_feature, ChannelType.PositiveHarm);
            storeChannel(cleric_negative_heal, cleric_negative_channel_feature, ChannelType.NegativeHeal);
            storeChannel(cleric_negative_harm, cleric_negative_channel_feature, ChannelType.NegativeHarm);

        }


        static internal void addHolyVindicatorChannelEnergyProgression()
        {
            foreach (var c in channel_entires)
            {
                addHolyVindicatorChannelEnergyProgressionToChannel(c);
            }
        }


        static void addHolyVindicatorChannelEnergyProgressionToChannel(ChannelEntry entry)
        {
            if (HolyVindicator.holy_vindicator_class == null)
            {
                return;
            }

            if (entry.scalesWithClass(HolyVindicator.holy_vindicator_class))
            {
                return;
            }

            var context_rank_config = entry.ability.GetComponent<ContextRankConfig>();
            if (context_rank_config != null)
            {
                context_rank_config = context_rank_config.CreateCopy();
            }

            var classes = Helpers.GetField<BlueprintCharacterClass[]>(context_rank_config, "m_Class");
            if (classes.Length != 0 && classes[0] != null)
            {
                classes = classes.AddToArray(HolyVindicator.holy_vindicator_class);
                Helpers.SetField(context_rank_config, "m_Class", classes);
            }

            var scaling = entry.ability.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>().CreateCopy();
            {
                scaling.CharacterClasses = scaling.CharacterClasses.AddToArray(HolyVindicator.holy_vindicator_class);
            }

            if (entry.channel_type.isOf(ChannelType.HolyVindicatorShield | ChannelType.Smite))
            {
                //look for buff
                var buff = (entry.ability.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).Buff;

                if (context_rank_config != null)
                {
                    buff.ReplaceComponent<ContextRankConfig>(context_rank_config);
                }

                buff.ReplaceComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(scaling);
            }
        }

        static internal void createHolyVindicatorShield()
        {
            var sacred_numbus = library.Get<BlueprintAbility>("bf74b3b54c21a9344afe9947546e036f");
            holy_vindicator_shield = Helpers.CreateFeature("HolyVindicatorShieldFeature",
                                                            "Vindicator's Shield",
                                                            "A vindicator can channel energy into his shield as a standard action; when worn, the shield gives the vindicator a sacred bonus (if positive energy) or profane bonus (if negative energy) to his Armor Class equal to the number of dice of the vindicator’s channel energy. This bonus lasts for 24 hours or until the vindicator is struck in combat, whichever comes first. The shield does not provide this bonus to any other wielder, but the vindicator does not need to be holding the shield for it to retain this power.",
                                                            "",
                                                            sacred_numbus.Icon,
                                                            FeatureGroup.None);
            foreach (var c in channel_entires)
            {
                addToHolyVindicatorShield(c);
            }
        }


        static void addToHolyVindicatorShield(ChannelEntry entry)
        {
            if (holy_vindicator_shield == null)
            {
                return;
            }

            if (!entry.channel_type.isBase())
            {
                return;
            }

            ModifierDescriptor bonus_descriptor = ModifierDescriptor.Sacred;
            var icon = holy_vindicator_shield.Icon;
            var fx_buff = library.Get<BlueprintBuff>("57b1c6a69c53f4d4ea9baec7d0a3a93a").FxOnStart; //sacred nimbus

            if (entry.channel_type.isOf(ChannelType.Negative))
            {
                bonus_descriptor = ModifierDescriptor.Profane;
                icon = library.Get<BlueprintAbility>("b56521d58f996cd4299dab3f38d5fe31").Icon; //profane nimbus
                fx_buff = library.Get<BlueprintBuff>("bb08ad05d0b4505488775090954c2317").FxOnStart;//profane imbus
            }

            var buff = Helpers.CreateBuff(entry.ability.name + "HolyVindicatorShieldBuff",
                                         $"{bonus_descriptor.ToString()} Vindicator's Shield ({entry.ability.Name})",
                                         holy_vindicator_shield.Description,
                                         Helpers.MergeIds(entry.ability.AssetGuid, "ea0d0d4343124c58905d444c3c6278e9"),
                                         icon,
                                         fx_buff,
                                         Helpers.Create<NewMechanics.AddStatBonusIfHasShield>(a =>
                                                                                             {
                                                                                                 a.descriptor = bonus_descriptor;
                                                                                                 a.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                 a.stat = Kingmaker.EntitySystem.Stats.StatType.AC;
                                                                                             }
                                                                                             ),
                                         Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, bonus_descriptor),
                                         entry.ability.GetComponent<ContextRankConfig>(),
                                         entry.ability.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                         Common.createAddTargetAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()),
                                                                                       Helpers.CreateActionList(), not_reach: false, only_melee: false
                                                                                      ),
                                         Helpers.CreateSpellDescriptor(holy_vindicator_shield_descriptor)
                                        );

            var apply_buff_action = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), Kingmaker.UnitLogic.Mechanics.DurationRate.Days),
                                                                        dispellable: false);

            var ability = Helpers.CreateAbility(entry.ability.name + "HolyVindicatorShieldAbility",
                                               buff.Name,
                                               buff.Description,
                                               Helpers.MergeIds(entry.ability.AssetGuid, "fda02a5dfc834f38be65c9f368e8ef62"),
                                               buff.Icon,
                                               AbilityType.Supernatural,
                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                               AbilityRange.Personal,
                                               "24 hours",
                                               "",
                                               Helpers.CreateRunActions(apply_buff_action),
                                               Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionRemoveBuffsByDescriptor(holy_vindicator_shield_descriptor)))
                                               );

            ability.setMiscAbilityParametersSelfOnly();

            entry.base_ability.addToAbilityVariants(ability);
            //updateItemsForChannelDerivative(entry.ability, ability);  ? shield is not updated

            var caster_alignment = entry.ability.GetComponent<AbilityCasterAlignment>();
            if (caster_alignment != null)
            {
                ability.AddComponent(caster_alignment);
            }

            storeChannel(ability, entry.parent_feature, entry.channel_type | ChannelType.HolyVindicatorShield);
        }


        static internal void createVersatileChanneler()
        {
            var channel_positive = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var channel_negative = library.Get<BlueprintFeature>("3adb2c906e031ee41a01bfc1d5fb7eea");

            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var allow_positive = channel_positive.GetComponent<PrerequisiteFeature>().Feature;
            var allow_negative = channel_negative.GetComponent<PrerequisiteFeature>().Feature;
            allow_positive.SetName("Deity Allows Channeling Positive Energy");
            allow_negative.SetName("Deity Allows Channeling Negative Energy");
            versatile_channeler_negative = Helpers.CreateFeature("VersatileChannelerNegativeFeature",
                                                                     "",
                                                                     "",
                                                                     "",
                                                                     null,
                                                                     FeatureGroup.None,
                                                                     Helpers.CreateAddFacts(channel_negative),
                                                                     Common.createRemoveFeatureOnApply(channel_negative.GetComponent<AddFacts>().Facts[3] as BlueprintFeature),
                                                                     Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => 
                                                                                                                                             { c.spells = new BlueprintAbility[0];
                                                                                                                                               c.value = Common.createSimpleContextValue(-2);
                                                                                                                                             })
                                                                     );
           versatile_channeler_negative.HideInCharacterSheetAndLevelUp = true;

           versatile_channeler_positive = Helpers.CreateFeature("VersatileChannelerPositiveFeature",
                                                         "",
                                                         "",
                                                         "",
                                                         null,
                                                         FeatureGroup.None,
                                                         Helpers.CreateAddFacts(channel_positive),
                                                         Common.createRemoveFeatureOnApply(channel_positive.GetComponent<AddFacts>().Facts[3] as BlueprintFeature),
                                                         Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => 
                                                                                                                                 { c.spells = new BlueprintAbility[0];
                                                                                                                                   c.value = Common.createSimpleContextValue(-2);
                                                                                                                                 }
                                                                                                                                 )
                                                         );
            versatile_channeler_positive.HideInCharacterSheetAndLevelUp = true;

            versatile_channeler_positive_warpriest = Helpers.CreateFeature("VersatileChannelerPositiveWarpriestFeature",
                                                                              "",
                                                                              "",
                                                                              "",
                                                                              null,
                                                                              FeatureGroup.None,
                                                                              Helpers.CreateAddFacts(Warpriest.channel_positive_energy),
                                                                              Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c =>
                                                                              {
                                                                                  c.spells = new BlueprintAbility[0];
                                                                                  c.value = Common.createSimpleContextValue(-2);
                                                                              }                                                                     )
                                                                              );
            versatile_channeler_positive_warpriest.HideInCharacterSheetAndLevelUp = true;


            versatile_channeler_negative_warpriest = Helpers.CreateFeature("VersatileChannelerNegativeWarpriestFeature",
                                                                  "",
                                                                  "",
                                                                  "",
                                                                  null,
                                                                  FeatureGroup.None,
                                                                  Helpers.CreateAddFacts(Warpriest.channel_negative_energy),
                                                                  Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c =>
                                                                  {
                                                                      c.spells = new BlueprintAbility[0];
                                                                      c.value = Common.createSimpleContextValue(-2);
                                                                  })
                                                                  );
            versatile_channeler_negative_warpriest.HideInCharacterSheetAndLevelUp = true;


            versatile_channeler = Helpers.CreateFeature("VersatileChannelerFeature",
                                                        "Versatile Channeler",
                                                        "If you normally channel positive energy, you may choose to channel negative energy as if your effective cleric level were 2 levels lower than normal. If you normally channel negative energy, you may choose to channel positive energy as if your effective cleric level were 2 levels lower than normal.\n"
                                                        + "Note: This feat only applies to neutral clerics or warpriests who worship neutral deities — characters who have the channel energy class ability and have to make a choice to channel positive or negative energy at 1st level. Characters whose alignment or deity makes this choice for them cannot select this feat.",
                                                        "",
                                                        null,
                                                        FeatureGroup.Feat,
                                                        Common.createAddFeatureIfHasFact(channel_negative, versatile_channeler_positive),
                                                        Common.createAddFeatureIfHasFact(channel_positive, versatile_channeler_negative),
                                                        Common.createAddFeatureIfHasFact(Warpriest.channel_positive_energy, versatile_channeler_negative_warpriest),
                                                        Common.createAddFeatureIfHasFact(Warpriest.channel_negative_energy, versatile_channeler_positive_warpriest),
                                                        channel_positive.GetComponent<PrerequisiteFeature>(),
                                                        channel_negative.GetComponent<PrerequisiteFeature>(),
                                                        Helpers.PrerequisiteFeaturesFromList(channel_negative, channel_positive, Warpriest.channel_positive_energy, Warpriest.channel_negative_energy),
                                                        Helpers.PrerequisiteClassLevel(cleric, 1, true),
                                                        Helpers.PrerequisiteClassLevel(Warpriest.warpriest_class, 4, true),
                                                        Common.createPrerequisiteAlignment(AlignmentMaskType.ChaoticNeutral | AlignmentMaskType.LawfulNeutral | AlignmentMaskType.TrueNeutral)
                                                        );
            library.AddFeats(versatile_channeler);

            foreach (var c in channel_entires)
            {
                addToVersatileChanneler(c);
            }
        }


        static void addToVersatileChanneler(ChannelEntry entry)
        {
            if (versatile_channeler == null)
            {
                return;
            }
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");

            if (entry.scalesWithClass(cleric))
            {
                if (entry.channel_type.isOf(ChannelType.Positive))
                {
                    var comp = versatile_channeler_positive.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
                    comp.spells = comp.spells.AddToArray(entry.ability);
                }
                else if (entry.channel_type.isOf(ChannelType.Negative))
                {
                    var comp = versatile_channeler_negative.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
                    comp.spells = comp.spells.AddToArray(entry.ability);
                }
            }

            if (entry.scalesWithClass(Warpriest.warpriest_class))
            {
                if (entry.channel_type.isOf(ChannelType.Positive))
                {
                    var comp = versatile_channeler_positive_warpriest.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
                    comp.spells = comp.spells.AddToArray(entry.ability);
                }
                else if (entry.channel_type.isOf(ChannelType.Negative))
                {
                    var comp = versatile_channeler_negative_warpriest.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
                    comp.spells = comp.spells.AddToArray(entry.ability);
                }
            }
        }

        //this function will only create ability without storing it
        //user will need to put it inside wrapper and then call storeChannel
        public static BlueprintAbility createChannelEnergy(ChannelType channel_type, string name, string dispaly_name, string description, string guid,
                                                           ContextRankConfig rank_config, NewMechanics.ContextCalculateAbilityParamsBasedOnClasses dc_scaling,
                                                           AbilityResourceLogic resource_logic)
        {
            string original_guid = "";
            switch (channel_type)
            {
                case ChannelType.PositiveHeal:
                    original_guid = "f5fc9a1a2a3c1a946a31b320d1dd31b2";
                    break;
                case ChannelType.PositiveHarm:
                    original_guid = "279447a6bf2d3544d93a0a39c3b8e91d";
                    break;
                case ChannelType.NegativeHarm:
                    original_guid = "89df18039ef22174b81052e2e419c728";
                    break;
                case ChannelType.NegativeHeal:
                    original_guid = "9be3aa47a13d5654cbcb8dbd40c325f2";
                    break;
                default:
                    throw Main.Error("Only base channel abilities can be created");
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


            updateItemsForChannelDerivative(library.Get<BlueprintAbility>(original_guid), ability);

            return ability;
        }


        public static void storeChannel(BlueprintAbility ability, BlueprintFeature parent_feature, ChannelType channel_type)
        {
            var entry = new ChannelEntry(ability, parent_feature, channel_type);
            channel_entires.Add(entry);

            addToImprovedChannel(entry);
            addToChannelingScourge(entry);

            addToWitchImprovedChannelHexScaling(entry);
            addToQuickChannel(entry);

            addToChannelSmite(entry);

            addToImprovedChannel(entry);

            Common.addFeaturePrerequisiteOr(selective_channel, parent_feature);

            addToBackToTheGrave(entry);
            addToVersatileChanneler(entry);
            addToHolyVindicatorShield(entry);
            addHolyVindicatorChannelEnergyProgressionToChannel(entry);
            addToStigmataPrerequisites(entry);
        }


        internal static void registerStigmata(BlueprintFeature stigmata)
        {
            stigmata_feature = stigmata;
        }


        static void addToStigmataPrerequisites(ChannelEntry entry)
        {
            if (!entry.channel_type.isBase())
            {
                return;
            }

            if (stigmata_feature == null)
            {
                return;
            }

            var sacred_stigmata_prerequisites = stigmata_feature.GetComponents<NewMechanics.AddFeatureIfHasFactsFromList>().ElementAt(0);
            if (entry.channel_type.isOf(ChannelType.Positive) && !sacred_stigmata_prerequisites.CheckedFacts.Contains(entry.parent_feature))
            {
                sacred_stigmata_prerequisites.CheckedFacts = sacred_stigmata_prerequisites.CheckedFacts.AddToArray(entry.parent_feature);
            }


            var profane_stigmata_prerequisites = stigmata_feature.GetComponents<NewMechanics.AddFeatureIfHasFactsFromList>().ElementAt(1);
            if (entry.channel_type.isOf(ChannelType.Negative) && !profane_stigmata_prerequisites.CheckedFacts.Contains(entry.parent_feature))
            {
                profane_stigmata_prerequisites.CheckedFacts = profane_stigmata_prerequisites.CheckedFacts.AddToArray(entry.parent_feature);
            }
        }

        public static void addToBackToTheGrave(ChannelEntry entry)
        {
            if (entry.channel_type.isOf(ChannelType.BackToTheGrave))
            {
                if (back_to_the_grave_base == null)
                {
                    back_to_the_grave_base = entry.base_ability;
                }
                else
                {
                    back_to_the_grave_base.addToAbilityVariants(entry.ability);
                }
            }
        }


        public static List<ChannelEntry> getChannels(Predicate<ChannelEntry> p)
        {
            return channel_entires.Where(c => p(c)).ToList();
        }


        public static List<BlueprintAbility> getChannelAbilities(Predicate<ChannelEntry> p)
        {
            List<BlueprintAbility> abilities = new List<BlueprintAbility>();

            foreach(var c in channel_entires)
            {
                if (p(c))
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

            foreach (var c in channel_entires)
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
            if (entry.channel_type.isOf(ChannelType.Negative))
            {
                var comp = witch_channel_negative.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
                comp.spells = comp.spells.AddToArray(entry.ability);
            }
            if (entry.channel_type.isOf(ChannelType.Positive))
            {
                var comp = witch_channel_positive.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
                comp.spells = comp.spells.AddToArray(entry.ability);
            }
        }


        internal static void createChannelingScourge()
        {
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var inquisitor = library.Get<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            var cleric_channel = library.Get<BlueprintFeatureSelection>("d332c1748445e8f4f9e92763123e31bd");

            var harm_undead = library.Get<BlueprintAbility>("279447a6bf2d3544d93a0a39c3b8e91d");
            var harm_living = library.Get<BlueprintAbility>("89df18039ef22174b81052e2e419c728");


            var channels = getChannelAbilities(c => c.channel_type.isOf(ChannelType.Harm | ChannelType.Smite) && c.channel_type.isNotOf(ChannelType.BackToTheGrave) && c.scalesWithClass(cleric));

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
            if (c.channel_type.isOf(ChannelType.Harm) && c.channel_type.isNotOf(ChannelType.BackToTheGrave)  && c.scalesWithClass(cleric))
            {
                var components = channeling_scourge.GetComponents<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
                foreach (var component in components)
                {
                    component.spells = component.spells.AddToArray(c.ability);
                }
            }
        }


        static void addToImprovedChannel(ChannelEntry c)
        {
            if (improved_channel == null)
            {
                return;
            }
            var prereq = improved_channel.GetComponent<PrerequisiteFeaturesFromList>();
            if (!prereq.Features.Contains(c.parent_feature))
            {
                prereq.Features = prereq.Features.AddToArray(c.parent_feature);
            }

            var abilities = improved_channel.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();

            if (!abilities.spells.Contains(c.ability))
            {
                abilities.spells = abilities.spells.AddToArray(c.ability);
            }
        }


        internal static void createImprovedChannel()
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

            foreach (var c in channel_entires)
            {
                addToImprovedChannel(c);
            }

            library.AddFeats(improved_channel);
        }


        internal static void createChannelSmite()
        {
            var resounding_blow = library.Get<BlueprintAbility>("9047cb1797639924487ec0ad566a3fea");
            channel_smite = Helpers.CreateFeature("ChannelSmiteFeature",
                                      "Channel Smite",
                                      "Before you make a melee attack roll, you can choose to spend one use of your channel energy ability as a swift action. If you channel positive energy and you hit an undead creature, that creature takes an amount of additional damage equal to the damage dealt by your channel positive energy ability. If you channel negative energy and you hit a living creature, that creature takes an amount of additional damage equal to the damage dealt by your channel negative energy ability. Your target can make a Will save, as normal, to halve this additional damage. If your attack misses, the channel energy ability is still expended with no effect.",
                                      "",
                                      resounding_blow.Icon,
                                      FeatureGroup.Feat);
            channel_smite.Groups = channel_smite.Groups.AddToArray(FeatureGroup.CombatFeat);

            foreach (var c in channel_entires.ToArray())
            {
                addToChannelSmite(c);
            }

            library.AddCombatFeats(channel_smite);
        }

        static void addToChannelSmite(ChannelEntry c)
        {
            if (channel_smite == null)
            {
                return;
            }

            if (!(c.channel_type.isOf(ChannelType.Harm) && c.channel_type.isBase()))
            {
                return;
            }

            Common.addFeaturePrerequisiteOr(channel_smite, c.parent_feature);

            var resounding_blow = library.Get<BlueprintAbility>("9047cb1797639924487ec0ad566a3fea");
            var smite_evil = library.Get<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec");
            var buff = Helpers.CreateBuff("ChannelSmite" + c.ability.name + "Buff",
                                          $"Channel Smite ({c.ability.Name})",
                                          channel_smite.Description,
                                          Helpers.MergeIds(c.ability.AssetGuid, "0d406cf592524c85b796216ed4ee3ab3"),
                                          resounding_blow.Icon,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(c.ability.GetComponent<AbilityEffectRunAction>().Actions,
                                                                                           check_weapon_range_type: true),
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()),
                                                                                           check_weapon_range_type: true,
                                                                                           only_hit: false,
                                                                                           on_initiator: true),
                                           c.ability.GetComponent<ContextRankConfig>()
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                                 Helpers.CreateContextDuration(Common.createSimpleContextValue(1), Kingmaker.UnitLogic.Mechanics.DurationRate.Rounds),
                                                                 dispellable: false
                                                                 );

            var ability = Helpers.CreateAbility("ChannelSmite" + c.ability.name,
                                                buff.Name,
                                                buff.Description,
                                                Helpers.MergeIds(c.ability.AssetGuid, "81e5fc81f1a644d5898a9fdbda752e95"),
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneRoundDuration,
                                                c.ability.LocalizedSavingThrow,
                                                smite_evil.GetComponent<AbilitySpawnFx>(),
                                                c.ability.GetComponent<AbilityResourceLogic>(),
                                                Helpers.CreateRunActions(apply_buff),
                                                c.ability.GetComponent<ContextRankConfig>(),
                                                c.ability.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                                Common.createAbilityShowIfCasterHasFact(channel_smite)
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            c.base_ability.addToAbilityVariants(ability);
            updateItemsForChannelDerivative(c.ability, ability);

            var caster_alignment = c.ability.GetComponent<AbilityCasterAlignment>();
            if (caster_alignment != null)
            {
                ability.AddComponent(caster_alignment);
            }

            var smite_feature = Common.AbilityToFeature(ability, guid: Helpers.MergeIds(ability.AssetGuid, c.parent_feature.AssetGuid));
            smite_feature.ComponentsArray = new BlueprintComponent[0];

            normal_smite_map.Add(c.ability.AssetGuid, ability.AssetGuid);

            storeChannel(ability, c.parent_feature, c.channel_type | ChannelType.Smite);
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

        
            foreach (var c in channel_entires.ToArray())
            {
                addToQuickChannel(c);
            }

            library.AddFeats(quick_channel);
        }


        static void addToQuickChannel(ChannelEntry entry)
        {
            if (quick_channel == null)
            {
                return;
            }

            if (entry.ability.ActionType != Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard)
            {
                return;
            }

            if (entry.channel_type.isOf(ChannelType.HolyVindicatorShield))
            {// no quick for vindicator shield
                return;
            }

            Common.addFeaturePrerequisiteOr(quick_channel, entry.parent_feature);

            var quicken_ability = library.CopyAndAdd<BlueprintAbility>(entry.ability.AssetGuid, "Quick" + entry.ability.name, entry.ability.AssetGuid, "e936d73a1dfe42efb1765b980c80e113");
            quicken_ability.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Move;
            quicken_ability.SetName(quicken_ability.Name + $" ({quick_channel.Name})");
            var resource_logic = quicken_ability.GetComponent<AbilityResourceLogic>();
            var amount = resource_logic.Amount;
            quicken_ability.ReplaceComponent<AbilityResourceLogic>(c => { c.Amount = amount * 2;});

            quicken_ability.AddComponent(Common.createAbilityShowIfCasterHasFact(quick_channel));
            entry.base_ability.addToAbilityVariants(quicken_ability);

            var caster_alignment = entry.ability.GetComponent<AbilityCasterAlignment>();
            if (caster_alignment != null)
            {
                quicken_ability.AddComponent(caster_alignment);
            }

            updateItemsForChannelDerivative(entry.ability, quicken_ability);

            var quicken_feature = Common.AbilityToFeature(quicken_ability, guid: Helpers.MergeIds(quicken_ability.AssetGuid, entry.parent_feature.AssetGuid));
            quicken_feature.ComponentsArray = new BlueprintComponent[0];

            storeChannel(quicken_ability, entry.parent_feature, entry.channel_type | ChannelType.Quick);

            normal_quick_channel_map.Add(entry.ability.AssetGuid, quicken_ability.AssetGuid);
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
