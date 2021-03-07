using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
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
using Kingmaker.UnitLogic.Mechanics.Conditions;
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

        public static bool isStrictlyOf(this ChannelEnergyEngine.ChannelType this_type, ChannelEnergyEngine.ChannelType channel_type)
        {
            return (~this_type & channel_type) == 0;
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
        public enum ChannelProperty
        {
            None = 0,
            Bloodfire = 1,
            Bloodrain = 2,
            All = ~None
        }

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
            SwiftPositiveChannel = 1024,
            All = ~None
        }

        public class ChannelEntry
        {
            public readonly BlueprintAbility ability;
            public readonly BlueprintAbility base_ability;
            public readonly BlueprintFeature parent_feature;
            public readonly ChannelType channel_type;
            internal ChannelProperty properties;
            public readonly BlueprintUnitFact[] required_facts = new BlueprintUnitFact[0];

            public ChannelEntry(string ability_guid, string parent_feature_guid, ChannelType type, ChannelProperty property, params BlueprintUnitFact[] req_facts)
            {
                ability = library.Get<BlueprintAbility>(ability_guid);
                parent_feature = library.Get<BlueprintFeature>(parent_feature_guid);
                base_ability = ability.Parent;
                channel_type = type;
                properties = property;
                required_facts = req_facts;
            }


            public ChannelEntry(BlueprintAbility channel_ability, BlueprintFeature channel_parent_feature, ChannelType type, ChannelProperty property, params BlueprintUnitFact[] req_facts)
            {
                ability = channel_ability;
                parent_feature = channel_parent_feature;
                base_ability = ability.Parent;
                channel_type = type;
                properties = property;
                required_facts = req_facts;
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
        static public BlueprintFeature sacred_conduit = null;
        static public BlueprintFeature xavorns_cross_feature = null;
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
        static readonly public SpellDescriptor holy_vindicator_shield_descriptor = (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.HolyVindicatorShield;


        static BlueprintFeature stigmata_feature = null;
        static GameAction blood_positive_action = null;
        static GameAction blood_negative_action = null;
        static GameAction blood_smite_positive_action = null;
        static GameAction blood_smite_negative_action = null;

        static BlueprintFeature versatile_channel;
        static AbilityDeliverProjectile negative_line_projectile;
        static AbilityDeliverProjectile positive_line_projectile;

        static AbilityDeliverProjectile negative_cone_projectile;
        static AbilityDeliverProjectile positive_cone_projectile;

        static BlueprintUnitFact[] channel_resistances = new BlueprintFeature[] { library.Get<BlueprintFeature>("a9ac84c6f48b491438f91bb237bc9212") };

        static public BlueprintFeature swift_positive_channel;
        static public BlueprintAbilityResource swift_positve_channel_resource;

        static public BlueprintBuff desecrate_buff, consecrate_buff;
        static List<BlueprintCharacterClass> extra_progressing_classes = new List<BlueprintCharacterClass>();

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

            empyreal_sorc_channel_feature.GetComponent<AddFacts>().Facts = new BlueprintUnitFact[] { empyreal_sorc_positive_harm_base, empyreal_sorc_positive_heal_base };
            paladin_channel_feature.GetComponent<AddFacts>().Facts = new BlueprintUnitFact[] { paladin_positive_harm_base, paladin_positive_heal_base };
            hospitalier_channel_feature.GetComponent<AddFacts>().Facts = new BlueprintUnitFact[] { hospitalier_positive_harm_base, hospitalier_positive_heal_base };

            cleric_positve_channel_feature.GetComponent<AddFacts>().Facts[1] = cleric_positive_heal_base;
            cleric_positve_channel_feature.GetComponent<AddFacts>().Facts[2] = cleric_positive_harm_base;
            cleric_negative_channel_feature.GetComponent<AddFacts>().Facts[1] = cleric_negative_harm_base;
            cleric_negative_channel_feature.GetComponent<AddFacts>().Facts[2] = cleric_negative_heal_base;

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

            //replace undead check to work easier
            var channels_to_fix = new string[]
            {
                "cbd03c874e39e6c4795fe0093544f2a2", //BreathOfLifeTouch
                "9c7ebca48b7340242a761a9f53e2f010", //FmaewardenEmbersTouch
                "788d72e7713cf90418ee1f38449416dc", //InspiringRecovery (Shouldn't resurrect dhampirs)
                "0e370822d9e0ff54f897e7fdf24cffb8", //KineticRevificationAbility (Shouldn't resurrect dhampirs)

                "f5fc9a1a2a3c1a946a31b320d1dd31b2", //ChannelEnergy
                "e1536ee240c5d4141bf9f9485a665128", //ChannelEnergyEmpyrealHarm
                "574cf074e8b65e84d9b69a8c6f1af27b", //ChannelEnergyEmpyrealHeal
                "cc17243b2185f814aa909ac6b6599eaa", //ChannelEnergyHospitalerHarm
                "0c0cf7fcb356d2448b7d57f2c4db3c0c", //ChannelEnergyHospitalerHeal
                "4937473d1cfd7774a979b625fb833b47", //ChannelEnergyPaladinHarm
                "6670f0f21a1d7f04db2b8b115e8e6abf", //ChannelEnergyPaladinHeal
                "89df18039ef22174b81052e2e419c728", //ChannelNegativeEnergy
                "9be3aa47a13d5654cbcb8dbd40c325f2", //ChannelNegativeHeal
                "90db8c09596a6734fa3b281399d672f7", //ChannelOnCritAbility
                "279447a6bf2d3544d93a0a39c3b8e91d", //ChannelPositiveHarm
                "0d657aa811b310e4bbd8586e60156a2d", //CureCriticalWounds
                "1f173a16120359e41a20fc75bb53d449", //CureCriticalWoundsMass
                "0e41945b6701d7643b4f19c145d7d9e1", //CureCriticalWoundsPretendPotion
                "47808d23c67033d4bbab86a1070fd62f", //CureLightWounds
                "5d3d689392e4ff740a761ef346815074", //CureLightWoundsMass
                "1c1ebf5370939a9418da93176cc44cd9", //CureModerateWounds
                "571221cc141bc21449ae96b3944652aa", //CureModerateWoundsMass
                "6e81a6679a0889a429dec9cedcf3729c", //CureSeriousWounds
                "0cea35de4d553cc439ae80b3a8724397", //CureSeriousWoundsMass
                "c8feb35c95f9f83438655f93552b900b", //DivineHunterDistantMercyAbility
                "ff8f1534f66559c478448723e16b6624", //Heal
                "caae1dc6fcf7b37408686971ee27db13", //LayOnHandsOthers
                "8d6073201e5395d458b8251386d72df1", //LayOnHandsSelf
                "8337cea04c8afd1428aad69defbfc365", //LayOnHandsSelfOrTroth
                "a8666d26bbbd9b640958284e0eee3602", //LifeBlast
                "02a98da52a022534b94604dfb06e6fe9", //VeilOfPositiveEnergySwift

                "a44adb55cfca17d488faefeffd321b07", //FriendlyInflictCriticalWounds
                "137af566f68fd9b428e2e12da43c1482", //Harm
                "db611ffeefb8f1e4f88e7d5393fc651d", //HealingBurstAbility
                "867524328b54f25488d371214eea0d90", //HealMass
                "3cf05ef7606f06446ad357845cb4d430", //InflictCriticalWounds
                "5ee395a2423808c4baf342a4f8395b19", //InflictCriticalWoundsMass
                "244a214d3b0188e4eb43d3a72108b67b", //InflictLightWounds_PrologueTrap
                "e5cb4c4459e437e49a4cd73fde6b9063", //InflictLightWounds
                "9da37873d79ef0a468f969e4e5116ad2", //InflictLightWoundsMass
                "14d749ecacca90a42b6bf1c3f580bb0c", //InflictModerateWounds
                "03944622fbe04824684ec29ff2cec6a7", //InflictModerateWoundsMass
                "b0b8a04a3d74e03489862b03f4e467a6", //InflictSeriousWounds
                "820170444d4d2a14abc480fcbdb49535", //InflictSeriousWoundsMass
                "5c24b3e2633a8f74f8419492eda5bf11", //ZombieLairChannelNegativeHarm
                "ab32492b5b46dea4199fe724efa5f800", //ZombieLairChannelNegativeHeal
            };

            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            foreach (var c_guid in channels_to_fix)
            {
                var c = library.Get<BlueprintAbility>(c_guid);
                var new_actions = Common.changeAction<Conditional>(c.GetComponent<AbilityEffectRunAction>().Actions.Actions,
                                                 a =>
                                                 {    
                                                     var checker = Helpers.CreateConditionsCheckerOr(a.ConditionsChecker.Conditions);
                                                     checker.Operation = a.ConditionsChecker.Operation;

                                                     for (int i = 0; i < checker.Conditions.Length; i++)
                                                     {
                                                         var has_fact = checker.Conditions[i] as ContextConditionHasFact;
                                                         if (has_fact != null && has_fact.Fact == undead)
                                                         {
                                                             checker.Conditions[i] = Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(e => e.Not = has_fact.Not);
                                                         }
                                                     }
                                                     a.ConditionsChecker = checker;
                                                 }
                                                 );
                c.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(new_actions));
            }
            //create favored class bonuses for favored class mod

            var feature_favored_all = Helpers.CreateFeature("ChannelFavoredClassBonusFeature",
                                                            "Channel Energy Bonus",
                                                            "Gain a +1/3 bonus on the damage dealt or healed with channel energy ability.",
                                                            "59c3b881e1b549f79a76c1f28dea54f6",
                                                             cleric_positive_heal.Icon,
                                                             FeatureGroup.None);
            feature_favored_all.Ranks = 6;

            var feature_favored_undead = Helpers.CreateFeature("HarmUndeadFavoredClassBonusFeature",
                                                                "Harm Undead Bonus",
                                                                "Add +1/2 to damage when using positive energy against undead.",
                                                                "8994cd3223f741248e64685248e715a3",
                                                                 cleric_positive_harm.Icon,
                                                                 FeatureGroup.None);
            feature_favored_undead.Ranks = 10;

            var cleric_channels = new BlueprintAbility[] { cleric_negative_heal, cleric_positive_harm, cleric_positive_heal, cleric_negative_harm };

            foreach (var ch in cleric_channels)
            {
                var actions = ch.GetComponent<AbilityEffectRunAction>().Actions.Actions;
                actions = CallOfTheWild.Common.changeAction<ContextActionHealTarget>(actions, c => c.Value.BonusValue = Helpers.CreateContextValue(AbilityRankType.DamageDiceAlternative));
                actions = CallOfTheWild.Common.changeAction<ContextActionDealDamage>(actions, c => c.Value.BonusValue = Helpers.CreateContextValue(AbilityRankType.DamageDiceAlternative));

                ch.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureRank, feature: feature_favored_all, type: AbilityRankType.DamageDiceAlternative));
                if (ch == cleric_positive_harm)
                {
                    ch.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks,
                                                                     featureList: new BlueprintFeature[] { feature_favored_undead, feature_favored_all },
                                                                     type: AbilityRankType.DamageDiceAlternative));
                }
                else
                {
                    ch.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureRank,
                                                                     feature: feature_favored_all,
                                                                     type: AbilityRankType.DamageDiceAlternative));
                }

                ch.ReplaceComponent<AbilityEffectRunAction>(CallOfTheWild.Helpers.CreateRunActions(actions));
            }

            ChannelEnergyEngine.createImprovedChannel();
            ChannelEnergyEngine.createQuickChannel();
            ChannelEnergyEngine.createChannelSmite();
        }




        static public void addChannelResitance(BlueprintUnitFact new_cr)
        {
            channel_resistances = channel_resistances.AddToArray(new_cr);
            foreach (var c in channel_entires)
            {
                addToSpecificChannelResistance(c, new_cr);
            }
        }


        static internal void addVersatileChannel(BlueprintFeature versatile_channel_feat)
        {
            versatile_channel = versatile_channel_feat;
            var negative_line_proj = library.Get<BlueprintProjectile>("3d13c52359bcd6645988dd88a120d7c0"); //umbral strike
            var positive_line_proj = library.Get<BlueprintProjectile>("1288a9729b18d3e4682d0f784e5fbd55"); //plasma
            var negative_cone_proj = library.Get<BlueprintProjectile>("4d601ab51e167c04a8c6883260e872ee"); //necromancy cone
            var positive_cone_proj = library.Get<BlueprintProjectile>("8c3c664b2bd74654e82fc70d3457e10d"); //enchantment_cone

            var lightning_bolt = library.Get<BlueprintAbility>("d2cff9243a7ee804cb6d5be47af30c73");
            var waves_of_fatigue = library.Get<BlueprintAbility>("8878d0c46dfbd564e9d5756349d5e439");

            negative_line_projectile = lightning_bolt.GetComponent<AbilityDeliverProjectile>().CreateCopy();
            negative_line_projectile.Projectiles = new BlueprintProjectile[] { negative_line_proj };

            positive_line_projectile = lightning_bolt.GetComponent<AbilityDeliverProjectile>().CreateCopy();
            positive_line_projectile.Projectiles = new BlueprintProjectile[] { positive_line_proj };

            negative_cone_projectile = waves_of_fatigue.GetComponent<AbilityDeliverProjectile>().CreateCopy();
            negative_cone_projectile.Projectiles = new BlueprintProjectile[] { negative_cone_proj };

            positive_cone_projectile = waves_of_fatigue.GetComponent<AbilityDeliverProjectile>().CreateCopy();
            positive_cone_projectile.Projectiles = new BlueprintProjectile[] { positive_cone_proj };

            foreach (var entry in channel_entires.ToArray())
            {
                addToVersatileChannel(entry);
            }
        }


        static void addToVersatileChannel(ChannelEntry entry)
        {
            if (versatile_channel == null)
            {
                return;
            }

            if (entry.channel_type.isOf(ChannelType.Form) || entry.channel_type.isOf(ChannelType.Smite) || entry.channel_type.isOf(ChannelType.HolyVindicatorShield))
            {
                return;
            }

            var spray_form = library.Get<BlueprintAbility>("a240a6d61e1aee040bf7d132bfe1dc07");//fan of flames
            var torrent_form = library.Get<BlueprintFeature>("2aad85320d0751340a0786de073ee3d5");
            var cone_ability = library.CopyAndAdd<BlueprintAbility>(entry.ability.AssetGuid, "Cone" + entry.ability.name, entry.ability.AssetGuid, "0217ce91d3384dafa3a3b85d00fc9f95");
            var line_ability = library.CopyAndAdd<BlueprintAbility>(entry.ability.AssetGuid, "Line" + entry.ability.name, entry.ability.AssetGuid, "4fc2127aa79542188f739bb1516aff5d");

            cone_ability.SetName($"{cone_ability.Name}, 30-Foot Cone");
            cone_ability.SetIcon(spray_form.Icon);
            line_ability.SetName($"{line_ability.Name}, 120-Foot Line");
            line_ability.SetIcon(torrent_form.Icon);
            cone_ability.Range = AbilityRange.Projectile;
            line_ability.Range = AbilityRange.Projectile;
            cone_ability.setMiscAbilityParametersRangedDirectional();
            line_ability.setMiscAbilityParametersRangedDirectional();
            if (entry.channel_type.isOf(ChannelType.Positive))
            {
                cone_ability.ReplaceComponent<AbilityTargetsAround>(positive_cone_projectile);
                line_ability.ReplaceComponent<AbilityTargetsAround>(positive_line_projectile);
            }
            else
            {
                cone_ability.ReplaceComponent<AbilityTargetsAround>(negative_cone_projectile);
                line_ability.ReplaceComponent<AbilityTargetsAround>(negative_line_projectile);
            }

            line_ability.RemoveComponents<AbilitySpawnFx>();
            cone_ability.RemoveComponents<AbilitySpawnFx>();

            line_ability.AddComponent(Common.createAbilityShowIfCasterHasFacts(entry.required_facts.AddToArray(versatile_channel)));
            cone_ability.AddComponent(Common.createAbilityShowIfCasterHasFacts(entry.required_facts.AddToArray(versatile_channel)));
            entry.base_ability.addToAbilityVariants(line_ability, cone_ability);

            updateItemsForChannelDerivative(entry.ability, line_ability);
            updateItemsForChannelDerivative(entry.ability, cone_ability);

            storeChannel(line_ability, entry.parent_feature, entry.channel_type | ChannelType.Line, entry.properties);
            storeChannel(cone_ability, entry.parent_feature, entry.channel_type | ChannelType.Cone, entry.properties);
        }


        static internal void addBloodfireAndBloodrainActions(GameAction positive_action, GameAction negative_action, GameAction smite_positive_action, GameAction smite_negative_action)
        {
            blood_positive_action = positive_action;
            blood_negative_action = negative_action;
            blood_smite_positive_action = smite_positive_action;
            blood_smite_negative_action = smite_negative_action;

            foreach (var entry in channel_entires)
            {
                addToBloodfireAndBloodrain(entry);
            }
        }


        static void addToBloodfireAndBloodrain(ChannelEntry entry)
        {
            if (blood_negative_action == null)
            {
                return;
            }

            if (entry.channel_type.isOf(ChannelType.Smite) && !entry.properties.HasFlag(ChannelProperty.Bloodfire))
            {
                var buff = (entry.ability.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).Buff;

                if (entry.channel_type.isOf(ChannelType.Positive))
                {
                    buff.ReplaceComponent<AddInitiatorAttackWithWeaponTrigger>(a => a.Action =
                                                 Helpers.CreateActionList(Common.addMatchingAction<ContextActionDealDamage>(a.Action.Actions, blood_smite_positive_action)));
                }
                else
                {
                    buff.ReplaceComponent<AddInitiatorAttackWithWeaponTrigger>(a => a.Action =
                                                 Helpers.CreateActionList(Common.addMatchingAction<ContextActionDealDamage>(a.Action.Actions, blood_smite_negative_action)));
                }
                entry.properties = entry.properties | ChannelProperty.Bloodfire;
            }
            else if (entry.channel_type.isNotOf(ChannelType.HolyVindicatorShield) && entry.channel_type.isOf(ChannelType.Harm) && !entry.properties.HasFlag(ChannelProperty.Bloodrain))
            {
                if (entry.channel_type.isOf(ChannelType.Positive))
                {
                    entry.ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions =
                                                 Helpers.CreateActionList(Common.addMatchingAction<ContextActionDealDamage>(a.Actions.Actions, blood_positive_action)));
                }
                else
                {
                    entry.ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions =
                                                 Helpers.CreateActionList(Common.addMatchingAction<ContextActionDealDamage>(a.Actions.Actions, blood_negative_action)));
                }
                entry.properties = entry.properties | ChannelProperty.Bloodrain;
            }
        }


        static public void addClassToChannelEnergyProgression(BlueprintCharacterClass cls)
        {
            if (extra_progressing_classes.Contains(cls))
            {
                return;
            }

            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var blessing_of_the_faithful_long = library.Get<BlueprintAbility>("3ef665bb337d96946bcf98a11103f32f");
            ClassToProgression.addClassToAbility(cls, new BlueprintArchetype[0], blessing_of_the_faithful_long, cleric);

            foreach (var c in channel_entires)
            {
                addClassChannelEnergyProgressionToChannel(c, cls);
            }
        }      


        static void addClassChannelEnergyProgressionToChannel(ChannelEntry entry, BlueprintCharacterClass cls)
        {
            if (cls == null)
            {
                return;
            }

            if (entry.scalesWithClass(cls))
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
                classes = classes.AddToArray(cls);
                Helpers.SetField(context_rank_config, "m_Class", classes);
            }
            entry.ability.ReplaceComponent<ContextRankConfig>(context_rank_config);

            var scaling = entry.ability.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>().CreateCopy();
            {
                scaling.CharacterClasses = scaling.CharacterClasses.AddToArray(cls);
            }
            entry.ability.ReplaceComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(scaling);

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
            foreach (var c in channel_entires.ToArray())
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
                                               entry.ability.GetComponent<ContextRankConfig>(),
                                               entry.ability.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                               Common.createAbilityShowIfCasterHasFacts(entry.required_facts.AddToArray(holy_vindicator_shield)),
                                               Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionRemoveBuffsByDescriptor(holy_vindicator_shield_descriptor))),
                                               Helpers.Create<NewMechanics.AbilityCasterHasShield>()
                                               );

            ability.setMiscAbilityParametersSelfOnly();

            entry.base_ability.addToAbilityVariants(ability);

            foreach (var checker in entry.ability.GetComponents<IAbilityCasterChecker>())
            {
                ability.AddComponent(checker as BlueprintComponent);
            }

            storeChannel(ability, entry.parent_feature, entry.channel_type | ChannelType.HolyVindicatorShield, entry.properties);
        }


        static internal void createVersatileChanneler()
        {
            var channel_positive = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var channel_negative = library.Get<BlueprintFeature>("3adb2c906e031ee41a01bfc1d5fb7eea");

            //separate spontaneous cure/harm from channel
            var spontaneous_heal = channel_positive.GetComponent<AddFacts>().Facts[3];
            var spontaneous_harm = channel_negative.GetComponent<AddFacts>().Facts[3];

            channel_positive.GetComponent<AddFacts>().Facts = channel_positive.GetComponent<AddFacts>().Facts.Take(3).ToArray();
            channel_positive.AddComponent(Common.createAddFeatureIfHasFact(spontaneous_harm, spontaneous_heal, not: true));
            channel_negative.GetComponent<AddFacts>().Facts = channel_negative.GetComponent<AddFacts>().Facts.Take(3).ToArray();
            channel_negative.AddComponent(Common.createAddFeatureIfHasFact(spontaneous_heal, spontaneous_harm, not: true));


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



            var versatile_channeler_negative_evangelist = Helpers.CreateFeature("VersatileChannelerEvangelistNegativeFeature",
                                             "",
                                             "",
                                             "",
                                             null,
                                             FeatureGroup.None,
                                             Helpers.CreateAddFacts(Archetypes.Evangelist.channel_negative_energy),
                                             versatile_channeler_negative.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>()
                                             );
            versatile_channeler_negative_evangelist.HideInCharacterSheetAndLevelUp = true;

            var versatile_channeler_positive_evangelist = Helpers.CreateFeature("VersatileChannelerPositiveEvangelistFeature",
                                                          "",
                                                          "",
                                                          "",
                                                          null,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFacts(Archetypes.Evangelist.channel_positive_energy),
                                                          ChannelEnergyEngine.versatile_channeler_positive.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>()
                                                          );
            versatile_channeler_positive_evangelist.HideInCharacterSheetAndLevelUp = true;


            versatile_channeler = Helpers.CreateFeature("VersatileChannelerFeature",
                                                        "Versatile Channeler",
                                                        "If you normally channel positive energy, you may choose to channel negative energy as if your effective cleric level were 2 levels lower than normal. If you normally channel negative energy, you may choose to channel positive energy as if your effective cleric level were 2 levels lower than normal.\n"
                                                        + "Note: This feat only applies to neutral clerics or warpriests who worship neutral deities — characters who have the channel energy class ability and have to make a choice to channel positive or negative energy at 1st level. Characters whose alignment or deity makes this choice for them cannot select this feat.",
                                                        "",
                                                        null,
                                                        FeatureGroup.Feat,
                                                        Common.createAddFeatureIfHasFactAndNotHasFact(channel_negative, channel_positive, versatile_channeler_positive),
                                                        Common.createAddFeatureIfHasFactAndNotHasFact(channel_positive, channel_negative, versatile_channeler_negative),
                                                        Common.createAddFeatureIfHasFactAndNotHasFact(Warpriest.channel_positive_energy, Warpriest.channel_negative_energy, versatile_channeler_negative_warpriest),
                                                        Common.createAddFeatureIfHasFactAndNotHasFact(Warpriest.channel_negative_energy, Warpriest.channel_negative_energy,versatile_channeler_positive_warpriest),
                                                        Common.createAddFeatureIfHasFactAndNotHasFact(Archetypes.Evangelist.channel_negative_energy, Archetypes.Evangelist.channel_positive_energy, versatile_channeler_positive_evangelist),
                                                        Common.createAddFeatureIfHasFactAndNotHasFact(Archetypes.Evangelist.channel_positive_energy, Archetypes.Evangelist.channel_negative_energy, versatile_channeler_negative_evangelist),
                                                        channel_positive.GetComponent<PrerequisiteFeature>(),
                                                        channel_negative.GetComponent<PrerequisiteFeature>(),
                                                        Helpers.PrerequisiteFeaturesFromList(channel_negative, channel_positive, Warpriest.channel_positive_energy, Warpriest.channel_negative_energy,
                                                                                             Archetypes.Evangelist.channel_positive_energy, Archetypes.Evangelist.channel_negative_energy),
                                                        Helpers.PrerequisiteClassLevel(cleric, 3, true),
                                                        Helpers.PrerequisiteClassLevel(Warpriest.warpriest_class, 5, true),
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


        public static void storeChannel(BlueprintAbility ability, BlueprintFeature parent_feature, ChannelType channel_type, ChannelProperty property = ChannelProperty.None)
        {
            var entry = new ChannelEntry(ability, parent_feature, channel_type, property);
            channel_entires.Add(entry);

            addToImprovedChannel(entry);
            addToChannelingScourge(entry);

            addToWitchImprovedChannelHexScaling(entry);
            if (!channel_type.isOf(ChannelType.Form))
            { //form channel will be created from quick and not vice versa
                addToQuickChannel(entry);
                addToSwiftPositiveChannel(entry);
            }
            addToVersatileChannel(entry);

            addToChannelSmite(entry);

            addToImprovedChannel(entry);

            Common.addFeaturePrerequisiteAny(selective_channel, parent_feature);

            addToBackToTheGrave(entry);
            addToVersatileChanneler(entry);
            addToHolyVindicatorShield(entry);
            foreach (var cls in extra_progressing_classes)
            {
                addClassChannelEnergyProgressionToChannel(entry, cls);
            }
            addToStigmataPrerequisites(entry);


            //should be last
            addToBloodfireAndBloodrain(entry);
            addToChannelResistance(entry);


            //ecclicitheurge scaling (no bonus at lvl 3)
            var bonded = library.Get<BlueprintFeature>("aa34ca4f3cd5e5d49b2475fcfdf56b24");
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var penalty = bonded.GetComponent<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>();
            if (penalty != null && entry.scalesWithClass(cleric))
            {
                penalty.spells = penalty.spells.AddToArray(entry.ability);
            }

            addIncreasedSpellDamageFromSunDomain(entry);

            addToDesecreate(entry);
            addToConsecrate(entry);
        }


        static void addToDesecreate(ChannelEntry entry)
        {
            if (desecrate_buff == null)
            {
                return;
            }
            if (entry.channel_type.isOf(ChannelType.NegativeHarm))
            {
                var dc_bonus = desecrate_buff.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();
                dc_bonus.spells = dc_bonus.spells.AddToArray(entry.ability);
            }
        }


        static void addToConsecrate(ChannelEntry entry)
        {
            if (consecrate_buff == null)
            {
                return;
            }
            if (entry.channel_type.isOf(ChannelType.PositiveHarm))
            {
                var dc_bonus = consecrate_buff.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();
                dc_bonus.spells = dc_bonus.spells.AddToArray(entry.ability);
            }
        }

        internal static void addIncreasedSpellDamageFromSunDomain(ChannelEntry entry)
        {
            if (!entry.channel_type.isOf(ChannelType.PositiveHarm))
            {
                return;
            }

            var sun_domain_dmg_bonus = library.Get<BlueprintFeature>("3d8e38c9ed54931469281ab0cec506e9");
            var component = sun_domain_dmg_bonus.GetComponent<IncreaseSpellDamageByClassLevel>();

            if (component.Spells.Contains(entry.ability))
            {
                return;
            }

            component.Spells = component.Spells.AddToArray(entry.ability);
        }

        internal static void addToSpecificChannelResistance(ChannelEntry entry, BlueprintUnitFact cr)
        {
            if (!entry.channel_type.isStrictlyOf(ChannelType.Positive | ChannelType.Harm))
            {
                return;
            }

            var comp = cr.GetComponent<SavingThrowBonusAgainstSpecificSpells>();
            if (comp != null && !comp.Spells.Contains(entry.ability))
            {
                comp.Spells = comp.Spells.AddToArray(entry.ability);
            }

            var comp2 = cr.GetComponent<NewMechanics.ContextSavingThrowBonusAgainstSpecificSpells>();
            if (comp2 != null && !comp2.Spells.Contains(entry.ability))
            {
                comp2.Spells = comp2.Spells.AddToArray(entry.ability);
            }
        }

        static void addToChannelResistance(ChannelEntry entry)
        {
            foreach (var cr in channel_resistances)
            {
                addToSpecificChannelResistance(entry, cr);
            }
        }


        internal static void registerStigmata(BlueprintFeature stigmata)
        {
            stigmata_feature = stigmata;

            foreach (var c in channel_entires)
            {
                addToStigmataPrerequisites(c);
            }
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
                                                       Helpers.PrerequisiteFeature(cleric_channel, any: true),
                                                       Helpers.PrerequisiteFeature(Archetypes.Evangelist.channel_energy, any: true)
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



        public static void addToImprovedChannel(BlueprintAbility ability, BlueprintFeature feature, bool not_negative = false)
        {
            if (improved_channel == null)
            {
                return;
            }


            var prereq = improved_channel.GetComponent<PrerequisiteFeaturesFromList>();
            if (!prereq.Features.Contains(feature))
            {
                prereq.Features = prereq.Features.AddToArray(feature);
            }

            var abilities = improved_channel.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();

            if (!abilities.spells.Contains(ability))
            {
                abilities.spells = abilities.spells.AddToArray(ability);
            }


            if (sacred_conduit == null)
            {
                return;
            }

            abilities = sacred_conduit.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();

            if (!abilities.spells.Contains(ability))
            {
                abilities.spells = abilities.spells.AddToArray(ability);
            }


            if (xavorns_cross_feature == null || not_negative)
            {
                return;
            }

            abilities = xavorns_cross_feature.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();

            if (!abilities.spells.Contains(ability))
            {
                abilities.spells = abilities.spells.AddToArray(ability);
            }
        }




        static void addToImprovedChannel(ChannelEntry c)
        {
            if (improved_channel == null)
            {
                return;
            }


            var prereq = improved_channel.GetComponent<PrerequisiteFeaturesFromList>();
            if (!prereq.Features.Contains(c.parent_feature) && !c.channel_type.isOf(ChannelType.BackToTheGrave))
            {
                prereq.Features = prereq.Features.AddToArray(c.parent_feature);
            }

            var abilities = improved_channel.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();

            if (!abilities.spells.Contains(c.ability))
            {
                abilities.spells = abilities.spells.AddToArray(c.ability);
            }


            if (sacred_conduit == null)
            {
                return;
            }

            abilities = sacred_conduit.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();

            if (!abilities.spells.Contains(c.ability))
            {
                abilities.spells = abilities.spells.AddToArray(c.ability);
            }

            if (xavorns_cross_feature == null || !c.channel_type.isOf(ChannelType.Negative))
            {
                return;
            }

            abilities = xavorns_cross_feature.GetComponent<NewMechanics.IncreaseSpecifiedSpellsDC>();

            if (!abilities.spells.Contains(c.ability))
            {
                abilities.spells = abilities.spells.AddToArray(c.ability);
            }
        }


        internal static void registerDesecreate(BlueprintBuff buff)
        {
            desecrate_buff = buff;
            buff.AddComponent(Helpers.Create<NewMechanics.IncreaseSpecifiedSpellsDC>(i => { i.BonusDC = 3; i.spells = new BlueprintAbility[0]; }));
            foreach (var c in channel_entires)
            {
                addToDesecreate(c);
            }
        }


        internal static void registerConsecrate(BlueprintBuff buff)
        {
            consecrate_buff = buff;

            buff.AddComponent(Helpers.Create<NewMechanics.IncreaseSpecifiedSpellsDC>(i => { i.BonusDC = 3; i.spells = new BlueprintAbility[0]; }));
            foreach (var c in channel_entires)
            {
                addToConsecrate(c);
            }
        }




        internal static void createImprovedChannel()
        {
            var turn_undead = library.Get<BlueprintAbility>("71b8898b1d26d654b9a3eeac87e3e2f8");
            var necromancy_school = library.Get<BlueprintFeature>("927707dce06627d4f880c90b5575125f");
            improved_channel = Helpers.CreateFeature("ImprovedChannelFeature",
                                                     "Improved Channel",
                                                     "Add 2 to the DC of saving throws made to resist the effects of your channel energy, command undead or turn undead ability.",
                                                     "",
                                                     null,
                                                     FeatureGroup.Feat,
                                                     Helpers.Create<NewMechanics.IncreaseSpecifiedSpellsDC>(c => { c.BonusDC = 2; c.spells = new BlueprintAbility[] {turn_undead }; }),
                                                     Helpers.PrerequisiteFeaturesFromList(necromancy_school)
                                                     );

            sacred_conduit = Helpers.CreateFeature("SacredConduitTrait",
                                                  "Sacred Conduit",
                                                  "Your birth was particularly painful and difficult for your mother, who needed potent divine magic to ensure that you survived (your mother may or may not have survived). In any event, that magic infused you from an early age, and you now channel divine energy with greater ease than most.\n"
                                                  + "Benefit: Whenever you channel energy, you gain a +1 trait bonus to the save DC of your channeled energy.",
                                                  "",
                                                  ChannelEnergyEngine.improved_channel.Icon,
                                                  FeatureGroup.Trait,
                                                  Helpers.Create<NewMechanics.IncreaseSpecifiedSpellsDC>(c => { c.BonusDC = 1; c.spells = new BlueprintAbility[] { turn_undead }; })
                                                 );

            xavorns_cross_feature = library.Get<BlueprintFeature>("35f473c44b5a94b42898be80f3248ca0");
            xavorns_cross_feature.RemoveComponents<IncreaseSpellDC>();
            xavorns_cross_feature.AddComponent(Helpers.Create<NewMechanics.IncreaseSpecifiedSpellsDC>(c => { c.BonusDC = 2; c.spells = new BlueprintAbility[] { turn_undead }; }));

            foreach (var c in channel_entires)
            {
                addToImprovedChannel(c);
            }

            library.AddFeats(improved_channel);
        }


        internal static BlueprintFeature createCommandUndead(string name_prefix, string display_name, string description,
                                                             StatType stat, BlueprintCharacterClass[] classes, BlueprintAbilityResource resource,
                                                             BlueprintArchetype[] archetypes = null)
        {
            var ability = Common.convertToSuperNatural(NewSpells.control_undead, name_prefix, classes, stat, resource, archetypes: archetypes);
            ability.SetName(display_name);

            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(resource.CreateAddAbilityResource());
            feature.SetDescription(description);

            ChannelEnergyEngine.addToImprovedChannel(ability, feature);

            return feature;
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

            var new_actions = Common.changeAction<ContextActionDealDamage>(c.ability.GetComponent<AbilityEffectRunAction>().Actions.Actions, 
                                                                        cad => cad.IgnoreCritical = true);
            var resounding_blow = library.Get<BlueprintAbility>("9047cb1797639924487ec0ad566a3fea");
            var smite_evil = library.Get<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec");
            var buff = Helpers.CreateBuff("ChannelSmite" + c.ability.name + "Buff",
                                          $"Channel Smite ({c.ability.Name})",
                                          channel_smite.Description,
                                          Helpers.MergeIds(c.ability.AssetGuid, "0d406cf592524c85b796216ed4ee3ab3"),
                                          resounding_blow.Icon,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger( Helpers.CreateActionList(new_actions),
                                                                                           check_weapon_range_type: true),
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()),
                                                                                           check_weapon_range_type: true,
                                                                                           only_hit: false,
                                                                                           on_initiator: true)
                                          );
            buff.AddComponents(c.ability.GetComponents<ContextRankConfig>());

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
                                                Common.createAbilityShowIfCasterHasFacts(c.required_facts.AddToArray(channel_smite))
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            ability.NeedEquipWeapons = true;
            ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon;

            c.base_ability.addToAbilityVariants(ability);
            updateItemsForChannelDerivative(c.ability, ability);

            foreach (var checker in c.ability.GetComponents<IAbilityCasterChecker>())
            {
                ability.AddComponent(checker as BlueprintComponent);
            }

            var smite_feature = Common.AbilityToFeature(ability, guid: Helpers.MergeIds(ability.AssetGuid, c.parent_feature.AssetGuid));
            smite_feature.ComponentsArray = new BlueprintComponent[0];

            normal_smite_map.Add(c.ability.AssetGuid, ability.AssetGuid);

            storeChannel(ability, c.parent_feature, c.channel_type | ChannelType.Smite, c.properties);
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


        public static void createSwiftPositiveChannel()
        {
            if (swift_positve_channel_resource != null)
            {
                return;
            }
            swift_positve_channel_resource = Helpers.CreateAbilityResource("SwiftPositveChannelResource", "", "", "", null);
            swift_positve_channel_resource.SetIncreasedByStat(0, Kingmaker.EntitySystem.Stats.StatType.Charisma);
            swift_positive_channel = Helpers.CreateFeature("SwiftPositiveChannelFeature",
                                                  "Swift Channel",
                                                  "",
                                                  "",
                                                  LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Channel_Quick.png"),
                                                  FeatureGroup.Feat);
            swift_positive_channel.HideInCharacterSheetAndLevelUp = true;

            foreach (var c in channel_entires.ToArray())
            {
                addToSwiftPositiveChannel(c);
            }
        }


        static void addToSwiftPositiveChannel(ChannelEntry entry)
        {
            if (swift_positive_channel == null)
            {
                return;
            }

            if (entry.ability.ActionType != Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard)
            {
                return;
            }

            if (entry.channel_type.isOf(ChannelType.HolyVindicatorShield | ChannelType.Negative))
            {// no swift for vindicator shield and negative energy
                return;
            }

            var quicken_ability = library.CopyAndAdd<BlueprintAbility>(entry.ability.AssetGuid, "Swift" + entry.ability.name, entry.ability.AssetGuid, "5d0149c1d031437c846f3930ecc923c0");
            quicken_ability.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift;
            quicken_ability.SetName(quicken_ability.Name + $" ({swift_positive_channel.Name})");
            var spend_resource = Helpers.Create<ContextActionOnContextCaster>(c => c.Actions = Helpers.CreateActionList(Common.createContextActionSpendResource(swift_positve_channel_resource, 1)));
            quicken_ability.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(spend_resource)));
            quicken_ability.AddComponent(Helpers.Create<NewMechanics.AbilityCasterHasResource>(c =>
                                                                                                {
                                                                                                    c.resource = swift_positve_channel_resource;
                                                                                                    c.amount = 1;
                                                                                                }
                                                                                               )
                                        );
            quicken_ability.AddComponent(Common.createAbilityShowIfCasterHasFacts(entry.required_facts.AddToArray(swift_positive_channel)));

            entry.base_ability.addToAbilityVariants(quicken_ability);

            updateItemsForChannelDerivative(entry.ability, quicken_ability);

            storeChannel(quicken_ability, entry.parent_feature, entry.channel_type | ChannelType.SwiftPositiveChannel, entry.properties);
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

            quicken_ability.AddComponent(Common.createAbilityShowIfCasterHasFacts(entry.required_facts.AddToArray(quick_channel)));
            entry.base_ability.addToAbilityVariants(quicken_ability);


            updateItemsForChannelDerivative(entry.ability, quicken_ability);

            var quicken_feature = Common.AbilityToFeature(quicken_ability, guid: Helpers.MergeIds(quicken_ability.AssetGuid, entry.parent_feature.AssetGuid));
            quicken_feature.ComponentsArray = new BlueprintComponent[0];

            storeChannel(quicken_ability, entry.parent_feature, entry.channel_type | ChannelType.Quick, entry.properties);

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

            var xavorns_cross_enchant = library.Get<BlueprintItemEnchantment>("249dd13b0e3f811448d796b01215e484");
            xavorns_cross_enchant.AddComponent(extra_channel.GetComponent<IncreaseResourceAmount>().CreateCopy(e => e.Value = 1));

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
