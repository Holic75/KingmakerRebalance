using System;
using Kingmaker.Blueprints;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Enums;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System.Linq;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Harmony12;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Common;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Class.Kineticist;

namespace CallOfTheWild
{
    public class Rebalance
    {
        static LibraryScriptableObject library => Main.library;

        static public BlueprintBuff[] aid_another_buffs = new BlueprintBuff[2];
        static public ContextRankConfig aid_another_config;
        static public BlueprintAbility aid_another;
        static public BlueprintAbility aid_self_free;

        static internal void createAidAnother()
        {
            var remove_fear = library.Get<BlueprintAbility>("55a037e514c0ee14a8e3ed14b47061de");
            var remove_self_action = Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>());

            aid_another_config = Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList, progression: ContextRankProgression.BonusValue,
                                                                                                featureList: new BlueprintFeature[0], stepLevel: 2);
            aid_another_buffs[0] = Helpers.CreateBuff("WarpriestCommunityBlessingAidAnother1Buff",
                                                                "Aid Another (Attack Bonus)",
                                                                "In melee combat, you can help a friend attack or defend by distracting or interfering with an opponents. If you’re in position to make a melee attack on an opponent that is engaging a friend in melee combat, you can attempt to aid your friend as a standard action. Your friend gains either a +2 bonus on his next attack roll or a +2 bonus to AC against next attack (your choice), as long as that attack comes before the beginning of your next turn. Multiple characters can aid the same friend, and similar bonuses stack.",
                                                                "",
                                                                remove_fear.Icon,
                                                                null,
                                                                Common.createAttackTypeAttackBonus(Helpers.CreateContextValue(AbilityRankType.Default), AttackTypeAttackBonus.WeaponRangeType.Melee, ModifierDescriptor.UntypedStackable),
                                                                Common.createAddInitiatorAttackWithWeaponTrigger(remove_self_action, check_weapon_range_type: true, wait_for_attack_to_resolve: true, on_initiator: true, only_hit: false),
                                                                aid_another_config
                                                                );
            aid_another_buffs[0].Stacking = StackingType.Stack;

            aid_another_buffs[1] = Helpers.CreateBuff("WarpriestCommunityBlessingAidAnother2Buff",
                                                    "Aid Another (AC Bonus)",
                                                    aid_another_buffs[0].Description,
                                                    "",
                                                    remove_fear.Icon,
                                                    null,
                                                    Helpers.Create<ACBonusAgainstAttacks>(a => { a.Value = Helpers.CreateContextValue(AbilityRankType.Default); a.Descriptor = ModifierDescriptor.UntypedStackable; a.AgainstMeleeOnly = true; }),
                                                    Helpers.Create<AddTargetAttackWithWeaponTrigger>(a =>
                                                    {
                                                        a.ActionOnSelf = remove_self_action;
                                                        a.WaitForAttackResolve = true;
                                                        a.OnlyMelee = true;
                                                        a.OnlyHit = false;
                                                    }),
                                                    aid_another_config
                                                    );
            aid_another_buffs[1].Stacking = StackingType.Stack;

            BlueprintAbility[] aid_another_abilities = new BlueprintAbility[aid_another_buffs.Length];
            BlueprintAbility[] aid_self_abilities = new BlueprintAbility[aid_another_buffs.Length];
            var cooldown = Helpers.CreateBuff("AlliedCloakAidAnotherCooldownBuff",
                                              "Allied Cloak (Cooldown)",
                                              aid_another_buffs[0].Description,
                                              "",
                                              Helpers.GetIcon("e418c20c8ce362943a8025d82c865c1c"), //cape of wasps
                                              null
                                             );
            var apply_cooldown = Common.createContextActionApplyBuff(cooldown, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false);

            for (int i = 0; i < aid_another_buffs.Length; i++)
            {
                var apply_buff = Common.createContextActionApplyBuff(aid_another_buffs[i], Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false);
                aid_another_abilities[i] = Helpers.CreateAbility($"WarpriestCommunityBlessingAidAnother{i + 1}Ability",
                                                                 aid_another_buffs[i].Name,
                                                                 aid_another_buffs[i].Description,
                                                                 "",
                                                                 aid_another_buffs[i].Icon,
                                                                 AbilityType.Special,
                                                                 UnitCommand.CommandType.Standard,
                                                                 AbilityRange.Touch,
                                                                 Helpers.oneRoundDuration,
                                                                 "",
                                                                 Helpers.CreateRunActions(Common.createContextActionRemoveBuffFromCaster(aid_another_buffs[i]), apply_buff)
                                                                 );
                aid_another_abilities[i].setMiscAbilityParametersTouchFriendly(works_on_self: false);

                aid_self_abilities[i] = Helpers.CreateAbility($"AlliedCloak{i + 1}Ability",
                                                                "Allied Cloak: " + aid_another_buffs[i].Name,
                                                                aid_another_buffs[i].Description,
                                                                "",
                                                                cooldown.Icon,
                                                                AbilityType.Special,
                                                                UnitCommand.CommandType.Free,
                                                                AbilityRange.Personal,
                                                                Helpers.oneRoundDuration,
                                                                "",
                                                                Helpers.CreateRunActions(Common.createContextActionRemoveBuffFromCaster(aid_another_buffs[i]), apply_buff, apply_cooldown),
                                                                Common.createAbilityCasterHasNoFacts(cooldown)
                                                             );
                aid_self_abilities[i].setMiscAbilityParametersSelfOnly();
            }

            var wrapper = Common.createVariantWrapper("AidAnotherAbilityBase", "", aid_another_abilities);
            wrapper.SetName("Aid Another");
            var feature = Helpers.CreateFeature("AidAnotherFeature",
                                                "Aid Another",
                                                aid_another_buffs[0].Description,
                                                "",
                                                aid_another_buffs[0].Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFacts(wrapper)
                                                );
            feature.HideInCharacterSheetAndLevelUp = true;
            var basic_feat_progression = library.Get<BlueprintProgression>("5b72dd2ca2cb73b49903806ee8986325");
            basic_feat_progression.LevelEntries[0].Features.Add(feature);
            aid_another = wrapper;

            aid_self_free = Common.createVariantWrapper("AidSelfFreeAbilityBase", "", aid_self_abilities);
            aid_self_free.SetName("Allied Cloak");

            Action<UnitDescriptor> save_game_action = delegate (UnitDescriptor u)
            {
                if (!u.HasFact(feature))
                {
                    u.AddFact(feature);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_action);
        }


        static internal void removeDescriptionsFromMonkACFeatures()
        {
            var monk_ac = library.Get<BlueprintFeature>("e241bdfd6333b9843a7bfd674d607ac4");
            var scaled_fist_ac = library.Get<BlueprintFeature>("3929bfd1beeeed243970c9fc0cf333f8");
            monk_ac.SetDescription("");
            scaled_fist_ac.SetDescription("");
        }


        internal static void addFatigueBuffRestrictionsToRage()
        {
            //add explicit fatigue/exhausted buff restrictions to prevent tired character under invigorate effect from entering rage
            var rage_ability = library.Get<BlueprintActivatableAbility>("df6a2cce8e3a9bd4592fb1968b83f730");

            rage_ability.AddComponents(Helpers.Create<RestrictionHasFact>(r => { r.Feature = BlueprintRoot.Instance.SystemMechanics.FatigueBuff; r.Not = true; }),
                                       Helpers.Create<RestrictionHasFact>(r => { r.Feature = BlueprintRoot.Instance.SystemMechanics.ExhaustedBuff; r.Not = true; })
                                       );
        }

        internal static void removePowerOfWyrmsBuffImmunity()
        {
            //power of wyrms in pf:km incorrectly gives corresponding elemental descriptor buff immunity, which interfers with elemental defensive spells, like flame shield or fiery body.
            //Also by pnp rules (and according to in-game description) it should only give immunity to elemental damage.
            var powers = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(f => f.name.Contains("PowerOfWyrms")).ToArray();

            var elemental_descriptors = SpellDescriptor.Fire | SpellDescriptor.Acid | SpellDescriptor.Electricity | SpellDescriptor.Cold;
            foreach (var p in powers)
            {
                p.RemoveComponents<BuffDescriptorImmunity>(s => (s.Descriptor & elemental_descriptors) > 0);
            }
        }

        internal static void fixSpellDescriptors()
        {
            //fiery body
            library.Get<BlueprintAbility>("08ccad78cac525040919d51963f9ac39").GetComponent<SpellDescriptorComponent>().Descriptor = SpellDescriptor.Fire;
            //fire belly
            Common.addSpellDescriptor(library.Get<BlueprintAbility>("5e5b663f988ece84b9346f6d7d541e66"), SpellDescriptor.Fire);
            library.Get<BlueprintAbility>("08ccad78cac525040919d51963f9ac39").GetComponent<SpellDescriptorComponent>().Descriptor = SpellDescriptor.Fire;
            //force descriptors on battering blast and magic missile
            library.Get<BlueprintAbility>("4ac47ddb9fa1eaf43a1b6809980cfbd2").AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Force));
            library.Get<BlueprintAbility>("0a2f7c6aa81bc6548ac7780d8b70bcbc").AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Force));
            library.Get<BlueprintAbility>("740d943e42b60f64a8de74926ba6ddf7").ReplaceComponent<SpellDescriptorComponent>(s => s.Descriptor = s.Descriptor | SpellDescriptor.Compulsion);
            //descriptor to boggard terrifying croak
            Common.addSpellDescriptor(library.Get<BlueprintAbility>("d7ab3a110325b174e90ae6c7b4e96bb9"), SpellDescriptor.MindAffecting | SpellDescriptor.Fear | SpellDescriptor.Shaken | SpellDescriptor.Emotion);


            //water descriptor to certain spells
            var water_spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("d8144161e352ca846a73cf90e85bf9ac"), //tsunami
                library.Get<BlueprintAbility>("9f10909f0be1f5141bf1c102041f93d9"), //snowball
                library.Get<BlueprintAbility>("7ef49f184922063499b8f1346fb7f521"), //seamantle
                library.Get<BlueprintAbility>("d8d451ed3c919a4438cde74cd145b981") //tidal wave
            };

            foreach (var ws in water_spells)
            {
                Common.addSpellDescriptor(ws, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water);
            }
            var tsunami_area = library.Get<BlueprintAbilityAreaEffect>("800daf41c11463742ad24efd71ab1916");
            Common.addSpellDescriptor(tsunami_area, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water);


            //add descriptor text to spells 
            var original = Harmony12.AccessTools.Method(typeof(UIUtilityTexts), "GetSpellDescriptor");
            var patch = Harmony12.AccessTools.Method(typeof(AdditionalSpellDescriptors.UIUtilityTexts_GetSpellDescriptor_Patch), "Postfix");
            Main.harmony.Patch(original, postfix: new Harmony12.HarmonyMethod(patch));

            
            var sirocco = library.Get<BlueprintAbility>("093ed1d67a539ad4c939d9d05cfe192c");
            sirocco.ReplaceComponent<SpellDescriptorComponent>(s => s.Descriptor = s.Descriptor | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air);
            var sirocco_area = library.Get<BlueprintAbilityAreaEffect>("b21bc337e2beaa74b8823570cd45d6dd");
            sirocco_area.ReplaceComponent<SpellDescriptorComponent>(s => s.Descriptor = s.Descriptor | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air);

            //earth spells
            var earth_spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("bacba2ff48d498b46b86384053945e83"), //cave fangs
                library.Get<BlueprintAbility>("e48638596c955a74c8a32dbc90b518c1"), //obsidian flow
                library.Get<BlueprintAbility>("7d700cdf260d36e48bb7af3a8ca5031f"), //tar pool
                library.Get<BlueprintAbility>("01300baad090d634cb1a1b2defe068d6"), //clashing rocks
                library.Get<BlueprintAbility>("01300baad090d634cb1a1b2defe068d6"), //stone call
            };
            earth_spells = earth_spells.AddToArray(library.Get<BlueprintAbility>("bacba2ff48d498b46b86384053945e83").Variants);
            foreach (var es in earth_spells)
            {
                Common.addSpellDescriptor(es, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth);
            }

            var heroism = library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63");
            var heroism_greater = library.Get<BlueprintAbility>("e15e5e7045fda2244b98c8f010adfe31");
            var heroic_invocation = library.Get<BlueprintAbility>("43740dab07286fe4aa00a6ee104ce7c1");
            var good_hope = library.Get<BlueprintAbility>("a5e23522eda32dc45801e32c05dc9f96");
            var rage = library.Get<BlueprintAbility>("97b991256e43bb140b263c326f690ce2");
            var bless = library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638");
            var aid = library.Get<BlueprintAbility>("03a9630394d10164a9410882d31572f0");
            var prayer = library.Get<BlueprintAbility>("faabd2cc67efa4646ac58c7bb3e40fcc");
            var burst_of_glory = library.Get<BlueprintAbility>("1bc83efec9f8c4b42a46162d72cbf494");

            Common.addSpellDescriptor(heroism, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(heroism_greater, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(heroic_invocation, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(good_hope, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(rage, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting | SpellDescriptor.Emotion);
            Common.addSpellDescriptor(bless, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting | SpellDescriptor.Emotion);
            Common.addSpellDescriptor(aid, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(prayer, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(burst_of_glory, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);

            var good_hope_buff = library.Get<BlueprintBuff>("85af9f0c5d29e5e4fa2e75ca70442487");
            var heroism_buff = library.Get<BlueprintBuff>("87ab2fed7feaaff47b62a3320a57ad8d");
            var heroism_greater_buff = library.Get<BlueprintBuff>("b8da3ec045ec04845a126948e1f4fc1a");
            var heroic_invocation_buff = library.Get<BlueprintBuff>("fd8fb2c1d622556468a04bea949eb7da");
            var rage_buff = library.Get<BlueprintBuff>("6928adfa56f0dcc468162efde545786b");
            var bless_buff = library.Get<BlueprintBuff>("87b8c6270ea85c743afc734dfe99afee");
            var inspire_courage_effect_buff = library.Get<BlueprintBuff>("6d6d9e06b76f5204a8b7856c78607d5d");
            var inspire_greatness_effect_buff = library.Get<BlueprintBuff>("ec38c2e60d738584983415cb8a4f508d");
            var inspire_heroics_effect_buff = library.Get<BlueprintBuff>("31e1f369cf0e4904887c96e4ef97a9cb");
            var aid_buff = library.Get<BlueprintBuff>("319b4679f25779e4e9d04360381254e1");
            var inspiring_recovery_buff = library.Get<BlueprintBuff>("87cd09cdcde2856489a8dd44a55030dc");
            var prayer_buff = library.Get<BlueprintBuff>("789bae3802e7b6b4c8097aaf566a1cf5");
            var prayer_debuff = library.Get<BlueprintBuff>("890182fa30a5f724c86ce41f237cf95f");
            var burst_of_glory_buff = library.Get<BlueprintBuff>("81005a24695910f4cb9b7c8ab4d932e1");
            

            Common.addSpellDescriptor(heroism_buff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(heroism_greater_buff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(heroic_invocation_buff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(good_hope_buff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting | SpellDescriptor.Emotion);
            Common.addSpellDescriptor(rage_buff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting | SpellDescriptor.Emotion);
            Common.addSpellDescriptor(bless_buff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting | SpellDescriptor.Emotion);
            Common.addSpellDescriptor(inspire_courage_effect_buff, SpellDescriptor.MindAffecting | SpellDescriptor.Emotion);
            Common.addSpellDescriptor(inspire_heroics_effect_buff, SpellDescriptor.MindAffecting | SpellDescriptor.Emotion);
            Common.addSpellDescriptor(inspire_greatness_effect_buff, SpellDescriptor.MindAffecting | SpellDescriptor.Emotion);
            Common.addSpellDescriptor(aid_buff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(inspiring_recovery_buff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(prayer_buff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(prayer_debuff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);
            Common.addSpellDescriptor(burst_of_glory_buff, SpellDescriptor.Compulsion | SpellDescriptor.MindAffecting);


            //fix kingdom compulsion immunity buff
            var compulsion_immunity_buff = library.Get<BlueprintBuff>("868a0ad22d7fa4d4480deb50a9dca681");
            compulsion_immunity_buff.GetComponent<BuffDescriptorImmunity>().IgnoreFeature = compulsion_immunity_buff;
            compulsion_immunity_buff.GetComponent<SpellImmunityToSpellDescriptor>().CasterIgnoreImmunityFact = compulsion_immunity_buff;

            //language dependent tag to castigate and castigate mass
            var castigate = library.Get<BlueprintAbility>("ce4c4e52c53473549ae033e2bb44b51a");
            var castigate_mass = library.Get<BlueprintAbility>("41236cf0e476d7043bc16a33a9f449bd");
            var castigate_buff = library.Get<BlueprintBuff>("3a9033dd2a95c0145a54da45070727f3");
            Common.addSpellDescriptor(castigate, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent);
            Common.addSpellDescriptor(castigate_mass, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent);
            Common.addSpellDescriptor(castigate_buff, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent);
        }

        internal static void fixFeyStalkerSummonBuff()
        {
            //in vanilla it uses monster tactician shared teamwork feats buff instead off anti-fey moral bonuses
            var feystalker_master = library.Get<BlueprintFeature>("02357ba2802b8654bb3e824bae68f5c0");
            var feystalker_buff = library.Get<BlueprintBuff>("5a4b6a4be0c7efc4dbc7159152a21447");
            feystalker_master.ReplaceComponent<OnSpawnBuff>(o => o.buff = feystalker_buff);
        }





        public static void fixBeltsOfPerfectComponents()
        {
            var lesser_extend = library.Get<BlueprintFeature>("23de5684062b01f49a2f310103db5b60");
            var lesser_empower = library.Get<BlueprintFeature>("c54708f815850ea4f9a96e091bcbccac");

            var normal_extend = library.Get<BlueprintFeature>("0592284ca75c8f546be126c130726531");
            var normal_empower = library.Get<BlueprintFeature>("324defe6bf85dab4d9e1d85a63c1d35a");

            var greater_extend = library.Get<BlueprintFeature>("9dc99e47a71654e41be9a408fa3914de");
            var greater_empower = library.Get<BlueprintFeature>("ac32d1c08f04edc4fb99a3314fabb41b");


            lesser_extend.RemoveComponents<AutoMetamagic>();
            lesser_extend.AddComponent(Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicUpToSpellLevel>(m => { m.Metamagic = Metamagic.Extend; m.max_level = 1; }));
            normal_extend.RemoveComponents<AutoMetamagic>();
            normal_extend.AddComponent(Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicUpToSpellLevel>(m => { m.Metamagic = Metamagic.Extend; m.max_level = 2; }));
            greater_extend.RemoveComponents<AutoMetamagic>();
            greater_empower.AddComponent(Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicUpToSpellLevel>(m => { m.Metamagic = Metamagic.Extend; m.max_level = 3; }));

            lesser_empower.RemoveComponents<AutoMetamagic>();
            lesser_empower.AddComponent(Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicUpToSpellLevel>(m => { m.Metamagic = Metamagic.Empower; m.max_level = 1; }));
            normal_empower.RemoveComponents<AutoMetamagic>();
            normal_empower.AddComponent(Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicUpToSpellLevel>(m => { m.Metamagic = Metamagic.Empower; m.max_level = 2; }));
            greater_empower.RemoveComponents<AutoMetamagic>();
            greater_empower.AddComponent(Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicUpToSpellLevel>(m => { m.Metamagic = Metamagic.Empower; m.max_level = 3; }));
        }


        internal static void fixTransmutionSchoolPhysicalEnhancement()
        {
            var physical_ehancment_feature = library.Get<BlueprintFeature>("93919f8ce64dc5a4cbf058a486a44a1b");
            var wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            foreach (var f in physical_ehancment_feature.GetComponent<AddFacts>().Facts)
            {
                var toggle = f as BlueprintActivatableAbility;
                var buff = toggle.Buff;
                var comp = buff.GetComponent<AddStatBonusScaled>();
                var stat = comp.Stat;
                var descriptor = comp.Descriptor;

                buff.ComponentsArray = new BlueprintComponent[]
                {
                    Helpers.CreateAddContextStatBonus(stat, descriptor),
                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                    classes: new BlueprintCharacterClass[]{wizard}, stepLevel: 5)
                };
            }
        }

        internal static void fixAnimalCompanion()
        {
            //animal companion rebalance
            //set natural ac as per pnp
            var natural_armor = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0d20d88abb7c33a47902bd99019f2ed1");
            var natural_armor_value = natural_armor.GetComponent<AddStatBonus>();
            natural_armor_value.Value = 2;
            //set stat bonus to str and dex as per pnp, 
            var stat_bonuses = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0c80276018694f24fbaf59ec7b841f2b");
            var stat_bonuses_value = stat_bonuses.GetComponents<AddStatBonus>();
            foreach (var stat_bonus_value in stat_bonuses_value)
            {
                stat_bonus_value.Value = 1;
                stat_bonus_value.Descriptor = ModifierDescriptor.Feat;
                if (stat_bonus_value.Stat == StatType.Constitution)
                {
                    stat_bonus_value.Value = 0;
                }
            }
            stat_bonuses.SetDescription("Animal companions receive +1 bonus to their Strength and Dexterity.");
            //remove enchanced attacks
            var enchanced_attacks_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("71d6955fe81a9a34b97390fef1104362");
            enchanced_attacks_feature.HideInCharacterSheetAndLevelUp = true;
            var enchanced_attack_bonus = enchanced_attacks_feature.GetComponent<AllAttacksEnhancement>();
            enchanced_attack_bonus.Bonus = 0;
            var enchanced_attack_saves_bonus = enchanced_attacks_feature.GetComponent<BuffAllSavesBonus>();
            enchanced_attack_saves_bonus.Value = 0;

            //fix progression
            var animal_companion_progression = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("9f8a232fbe435a9458bf64c3024d7bee");
            var animal_companion_level_entires = animal_companion_progression.AddFeatures;
            foreach (var lvl_entry in animal_companion_level_entires)
            {
                lvl_entry.Features.Remove(enchanced_attacks_feature);
            }


            //fix size change bonus type
            var animals = library.Get<BlueprintFeatureSelection>("571f8434d98560c43935e132df65fe76"); //druid animal companion selection
            foreach (var f in animals.AllFeatures)
            {
                var pet = f.GetComponent<AddPet>()?.UpgradeFeature;
                if (pet == null)
                {
                    continue;
                }

                foreach (var s in pet.GetComponents<AddStatBonus>())
                {
                    s.Descriptor = ModifierDescriptor.Feat;
                }
            }
        }


        internal static void fixLegendaryProportionsAC()
        {
            //fix natural armor bonus for animal growth and legendary proportions
            var buff_ids = new string[] {"3fca5d38053677044a7ffd9a872d3a0a", //animal growth
                                            "4ce640f9800d444418779a214598d0a3" //legendary proportions
                                        };
            foreach (var buff_id in buff_ids)
            {
                var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(buff_id);
                var boni = buff.GetComponents<AddContextStatBonus>();
                foreach (var bonus in boni)
                {
                    if (bonus.Stat == Kingmaker.EntitySystem.Stats.StatType.AC)
                    {
                        bonus.Descriptor = Kingmaker.Enums.ModifierDescriptor.NaturalArmor;
                    }
                }
            }
        }


        internal static void removeJudgement19FormSHandMS()
        {
            BlueprintArchetype[] inquisitor_archetypes = new BlueprintArchetype[2] {ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012"),//sacred huntsmaster
                                                                               ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("cdaabf4b146c9ba42ab7d05abe3b48c4")//monster tactician
                                                                              };
            Kingmaker.Blueprints.Classes.LevelEntry remove_entry = new Kingmaker.Blueprints.Classes.LevelEntry();
            remove_entry.Level = 19;
            remove_entry.Features.Add(ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ee50875819478774b8968701893b52f5"));//judgement additional use
            foreach (var inquisitor_archetype in inquisitor_archetypes)
            {
                inquisitor_archetype.RemoveFeatures = inquisitor_archetype.RemoveFeatures.AddToArray(remove_entry);
            }
        }

        internal static void fixSerpentineBloodlineSerpentfriend()
        {
            var serpentfriend = library.Get<BlueprintFeature>("d0f97ba02e20e39419e2a29c135fc351");
            serpentfriend.SetNameDescription("Serpentfriend",
                                             "You gain a viper familiar.");
            var viper_familiar = library.Get<BlueprintFeature>("3c0b706c526e0654b8af90ded235a089");
            serpentfriend.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAddFact(viper_familiar) };
        }


        static internal void fixNecklaceOfDoubleCorses()
        {
            var neclace_feature = library.Get<BlueprintFeature>("64d5a59feeb292e49a6c459fe37c3953");
            var sneak = neclace_feature.GetComponent<AdditionalSneakDamageOnHit>();
            Helpers.SetField(sneak, "m_Weapon", 1);
            neclace_feature.AddComponent(Helpers.Create<AooMechanics.AooAgainstAllies>());
        }

        internal static void fixSkillPoints()
        {
            //update skillpoints
            var class_skill_map = new List<KeyValuePair<string, int>>();
            class_skill_map.Add(new KeyValuePair<string, int>("0937bec61c0dabc468428f496580c721", 2));//alchemist
            class_skill_map.Add(new KeyValuePair<string, int>("9c935a076d4fe4d4999fd48d853e3cf3", 2));//arcane trickster
            class_skill_map.Add(new KeyValuePair<string, int>("f7d7eb166b3dd594fb330d085df41853", 2));//barbarian
            class_skill_map.Add(new KeyValuePair<string, int>("772c83a25e2268e448e841dcd548235f", 3));//bard
            class_skill_map.Add(new KeyValuePair<string, int>("67819271767a9dd4fbfd4ae700befea0", 1));//cleric
            class_skill_map.Add(new KeyValuePair<string, int>("72051275b1dbb2d42ba9118237794f7c", 1));//dragon disciple                   
            class_skill_map.Add(new KeyValuePair<string, int>("610d836f3a3a9ed42a4349b62f002e96", 2));//druid
            class_skill_map.Add(new KeyValuePair<string, int>("4e0ea99612ae87a499c7fb0588e31828", 2));//duelist
            class_skill_map.Add(new KeyValuePair<string, int>("de52b73972f0ed74c87f8f6a8e20b542", 1));//eldrich knight
            class_skill_map.Add(new KeyValuePair<string, int>("f5b8c63b141b2f44cbb8c2d7579c34f5", 1));//eldrich scion
            class_skill_map.Add(new KeyValuePair<string, int>("48ac8db94d5de7645906c7d0ad3bcfbd", 1));//fighter
            class_skill_map.Add(new KeyValuePair<string, int>("f1a70d9e1b0b41e49874e1fa9052a1ce", 3));//inquisitor
            class_skill_map.Add(new KeyValuePair<string, int>("42a455d9ec1ad924d889272429eb8391", 2));//kineticist
            class_skill_map.Add(new KeyValuePair<string, int>("45a4607686d96a1498891b3286121780", 1));//magus
            class_skill_map.Add(new KeyValuePair<string, int>("e8f21e5b58e0569468e420ebea456124", 2));//monk
            class_skill_map.Add(new KeyValuePair<string, int>("0920ea7e4fd7a404282e3d8b0ac41838", 1));//mystic theurge
            class_skill_map.Add(new KeyValuePair<string, int>("bfa11238e7ae3544bbeb4d0b92e897ec", 1));//paladin
            class_skill_map.Add(new KeyValuePair<string, int>("cda0615668a6df14eb36ba19ee881af6", 3));//ranger
            class_skill_map.Add(new KeyValuePair<string, int>("299aa766dee3cbf4790da4efb8c72484", 4));//rogue
            class_skill_map.Add(new KeyValuePair<string, int>("b3a505fb61437dc4097f43c3f8f9a4cf", 1));//sorcerer
            class_skill_map.Add(new KeyValuePair<string, int>("d5917881586ff1d4d96d5b7cebda9464", 1));//stalwart defender
            class_skill_map.Add(new KeyValuePair<string, int>("90e4d7da3ccd1a8478411e07e91d5750", 2));//aldori swordlord
            class_skill_map.Add(new KeyValuePair<string, int>("ba34257984f4c41408ce1dc2004e342e", 1));//wizard
            class_skill_map.Add(new KeyValuePair<string, int>("c75e0971973957d4dbad24bc7957e4fb", 2));//slayer
            class_skill_map.Add(new KeyValuePair<string, int>("90e4d7da3ccd1a8478411e07e91d5750", 2));//swordlord
            class_skill_map.Add(new KeyValuePair<string, int>("4cd1757a0eea7694ba5c933729a53920", 3));//animal
            foreach (var class_skill in class_skill_map)
            {
                var current_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>(class_skill.Key);
                current_class.SkillPoints = class_skill.Value;
            }


            var skilled_human = library.Get<BlueprintFeature>("3adf9274a210b164cb68f472dc1e4544");
            var skilled_half_orc = library.Get<BlueprintFeature>("4e8fe10f42f314e4fa7c7918bcfadbd5");
            skilled_human.ReplaceComponent<AddSkillPoint>(Helpers.Create<NewMechanics.AddSkillPointOnEvenLevels>());
            skilled_human.SetDescription("Humans gain an additional skill rank at every even level.");

            skilled_half_orc.ReplaceComponent<AddSkillPoint>(Helpers.Create<NewMechanics.AddSkillPointOnEvenLevels>());
            skilled_half_orc.SetDescription("Half-orcs gain an additional skill rank at every even level.");
        }


        internal static void fixArchetypeKineticistGatherPowerWithShield()
        {
            var base_kineticist_feature = library.Get<BlueprintFeature>("57e3577a0eb53294e9d7cc649d5239a3");
            var buff = base_kineticist_feature.GetComponent<AddKineticistPart>().CanGatherPowerWithShieldBuff;

            var features = new BlueprintFeature[]{ library.Get<BlueprintFeature>("42c5a9a8661db2f47aedf87fb8b27aaf"), //dark elementalist
                                                    library.Get<BlueprintFeature>("2fa48527ba627254ba9bf4556330a4d4"), //psychikoneticits
                                                 };

            foreach (var f in features)
            {
                f.GetComponent<AddKineticistPart>().CanGatherPowerWithShieldBuff = buff;
            }
        }

        internal static void fixUniversalistMetamagicMastery()
        {
            BlueprintFeature[] metamagics = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(b => b.Groups.Contains(FeatureGroup.WizardFeat) && (b.GetComponent<AddMetamagicFeat>() != null) && b.AssetGuid != "2f5d1e705c7967546b72ad8218ccf99c").ToArray();

            var resource = library.Get<BlueprintAbilityResource>("42fd5b455f986f94293b15b13f38d6a5");
            var feature = Helpers.CreateFeature("UniversalistMetamagicFeature",
                                                "Metamagic Mastery",
                                                "At 8th level, you can apply any one metamagic feat that you know to a spell you are about to cast. This does not alter the level of the spell or the casting time. You can use this ability once per day at 8th level and one additional time per day for every two wizard levels you possess beyond 8th. Any time you use this ability to apply a metamagic feat that increases the spell level by more than 1, you must use an additional daily usage for each level above 1 that the feat adds to the spell. Even though this ability does not modify the spell’s actual level, you cannot use this ability to cast a spell whose modified spell level would be above the level of the highest-level spell that you are capable of casting.",
                                                "",
                                                Helpers.GetIcon("541bb8d595532ec419343b7a93cdb449"),
                                                FeatureGroup.None,
                                                resource.CreateAddAbilityResource()
                                                );

            var universalist_progression = library.Get<BlueprintProgression>("0933849149cfc9244ac05d6a5b57fd80");
            universalist_progression.LevelEntries = universalist_progression.LevelEntries.Where(le => le.Level < 8).ToArray();
            universalist_progression.LevelEntries = universalist_progression.LevelEntries.AddToArray(Helpers.LevelEntry(8, feature)
                                                                                                     );
            foreach (var mf in metamagics)
            {
                var metamagic_enum = mf.GetComponent<AddMetamagicFeat>().Metamagic;
                var cost = metamagic_enum.DefaultCost();
                var buff = Helpers.CreateBuff(mf.name + "MetamagicMasteryBuff",
                                              "Metamagic Mastery - " + mf.Name,
                                              feature.Description + "\n" + mf.Name + ": " + mf.Description,
                                              "",
                                              mf.Icon,
                                              null,
                                              Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(mm =>
                                              {
                                                  mm.Metamagic = metamagic_enum;
                                                  mm.limit_spell_level = true;
                                                  mm.resource = resource;
                                                  mm.amount = cost;
                                              })
                                              );
                var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                                 resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                                 Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = cost; }));
                toggle.Group = ActivatableAbilityGroupExtension.MetamagicMastery.ToActivatableAbilityGroup();
                var add_toggle = Common.ActivatableAbilityToFeature(toggle);
                var level = 2 * (cost - 1) + 8;

                mf.AddComponent(Common.createAddFeatureIfHasFactAndNotHasFact(feature, add_toggle, add_toggle));
                feature.AddComponent(Common.createAddFeatureIfHasFactAndNotHasFact(mf, add_toggle, add_toggle));
            }
            universalist_progression.SetDescription("Wizards who do not specialize (known as as universalists) have the most diversity of all arcane spellcasters.\nHand of the Apprentice: You cause your melee weapon to fly from your grasp and strike a foe before instantly returning to you. As a standard action, you can make a single attack using a melee weapon at a range of 30 feet. This attack is treated as a ranged attack with a thrown weapon, except that you add your Intelligence modifier on the attack roll instead of your Dexterity modifier (damage still relies on Strength). This ability cannot be used to perform a combat maneuver. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.\nMetamagic Mastery: " + feature.Description);
        }

        internal static void fixCompanions()
        {
            //change stats of certain companions
            //Valerie 
            var valerie_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("54be53f0b35bf3c4592a97ae335fe765");
            /* var valerie_scar_protrait = library.Get<BlueprintPortrait>("8134f34ef1cc67c498f1ae616995023d");
            Action<UnitDescriptor> fix_action_v = delegate (UnitDescriptor u)
            {
                if (u.Blueprint == valerie_companion)
                {
                    u.UISettings.SetPortrait(valerie_scar_protrait);
                }
            };
            SaveGameFix.save_game_actions.Add(fix_action_v);*/
            valerie_companion.Strength = 16;//+2
            valerie_companion.Dexterity = 10;
            valerie_companion.Constitution = 14;
            valerie_companion.Intelligence = 13;
            valerie_companion.Wisdom = 10;
            valerie_companion.Charisma = 15;
            var valerie1_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("912444657701e2d4ab2634c3d1e130ad");
            var valerie_class_level = valerie1_feature.GetComponent<AddClassLevels>();
            valerie_class_level.CharacterClass = VindicativeBastard.vindicative_bastard_class;
            valerie_class_level.Archetypes = new BlueprintArchetype[0];
            valerie_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Strength;
            valerie_class_level.Selections[0].Features[0] = library.Get<BlueprintFeature>("ac57069b6bf8c904086171683992a92a"); //shield focus
            valerie_class_level.Selections[0].Features[1] = library.Get<BlueprintFeature>("4c44724ffa8844f4d9bedb5bb27d144a"); //combat expertise
            valerie_class_level.Skills = new StatType[] { StatType.SkillPersuasion, StatType.SkillKnowledgeWorld, StatType.SkillLoreReligion };
            valerie_companion.Body.PrimaryHand = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("571c56d11dafbb04094cbaae659974b5");//longsword
            valerie_companion.Body.SecondaryHand = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Shields.BlueprintItemShield>("f4cef3ba1a15b0f4fa7fd66b602ff32b");//shield
            valerie1_feature.GetComponent<AddFacts>().Facts = valerie1_feature.GetComponent<AddFacts>().Facts.Skip(1).ToArray();

            //change amiri stats
            var amiri_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b3f29faef0a82b941af04f08ceb47fa2");
            amiri_companion.Strength = 17;//+2
            amiri_companion.Dexterity = 12;
            amiri_companion.Constitution = 16;
            amiri_companion.Intelligence = 10;
            amiri_companion.Wisdom = 12;
            amiri_companion.Charisma = 8;
            var amiri1_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("df943986ee329e84a94360f2398ae6e6");
            var amiri_class_level = amiri1_feature.GetComponent<AddClassLevels>();
            amiri_class_level.Archetypes = new BlueprintArchetype[] { library.Get<BlueprintArchetype>("a2ccb759dc6f1f94d9aae8061509bf87") };
            amiri_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Strength;
            amiri_class_level.Selections[0].Features[1] = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
            //amiri_class_level.Selections[0].Features[1] = NewFeats.furious_focus;
            amiri_class_level.Skills = new StatType[] { StatType.SkillPersuasion, StatType.SkillAthletics, StatType.SkillLoreNature };
            //change tristian stats
            var tristian_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f6c23e93512e1b54dba11560446a9e02");
            tristian_companion.Strength = 10;
            tristian_companion.Dexterity = 14;
            tristian_companion.Constitution = 12;
            tristian_companion.Intelligence = 10;
            tristian_companion.Wisdom = 17;
            tristian_companion.Charisma = 14;
            var tristian_level = tristian_companion.GetComponent<AddClassLevels>();
            tristian_level.Selections[2].Features[0] = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("c85c8791ee13d4c4ea10d93c97a19afc");//sun as primary
            tristian_level.Selections[3].Features[0] = Subdomains.restoration_domain_secondary;
            tristian_level.Selections[4].Features[1] = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");//improved initiative
            tristian_level.Selections[4].Features[2] = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("16fa59cc9a72a6043b566b49184f53fe");//spell focus
            tristian_level.Selections[5].ParamSpellSchool = SpellSchool.Evocation;
            tristian_level.Selections[6].ParamSpellSchool = SpellSchool.Evocation;
            tristian_level.Skills = new StatType[] { StatType.SkillLoreReligion, StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillLoreNature };
            tristian_level.Levels = 1;
            var harrim_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("aab03d0ab5262da498b32daa6a99b507");
            harrim_companion.Strength = 17;
            harrim_companion.Constitution = 12;
            harrim_companion.Intelligence = 10;
            harrim_companion.Charisma = 10;
            harrim_companion.Wisdom = 14;
            harrim_companion.Dexterity = 14;
            harrim_companion.Body.PrimaryHand = null;
            harrim_companion.Body.SecondaryHand = null;
            harrim_companion.Body.Armor = null;
            harrim_companion.Body.PrimaryHandAlternative1 = ResourcesLibrary.TryGetBlueprint<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>("7f7c8e1e4fdd99e438b30ed9622e9e3f");//heavy flail


            var harrim_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("8910febae2a7b9f4ba5eca4dde1e9649");
            harrim_feature.GetComponent<AddFacts>().Facts = harrim_feature.GetComponent<AddFacts>().Facts.Skip(1).ToArray();

            var harrim_class_level = harrim_feature.GetComponent<AddClassLevels>();
            harrim_class_level.CharacterClass = Warpriest.warpriest_class;
            harrim_class_level.Archetypes = new BlueprintArchetype[] { Warpriest.sacred_fist_archetype };
            harrim_class_level.Skills = new StatType[] { StatType.SkillLoreReligion, StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillLoreNature };
            harrim_class_level.Selections[0].Features = new BlueprintFeature[] { library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"),//wf
                                                                                 NewFeats.weapon_of_the_chosen,
                                                                                 NewFeats.improved_weapon_of_the_chosen,
                                                                                 NewFeats.greater_weapon_of_the_chosen,
                                                                                 NewFeats.furious_focus,
                                                                                 library.Get<BlueprintFeature>("31470b17e8446ae4ea0dacd6c5817d86"), //ws
                                                                                 library.Get<BlueprintParametrizedFeature>("7cf5edc65e785a24f9cf93af987d66b3"), //gws
                                                                                 library.Get<BlueprintParametrizedFeature>("09c9e82965fb4334b984a1e9df3bd088"), //gwf
                                                                                 library.Get<BlueprintFeature>("afd05ca5363036c44817c071189b67e1"), //gsf
                                                                                 Warpriest.extra_channel
                                                                                 };
            harrim_class_level.Selections[2].Selection = Warpriest.warpriest_energy_selection;
            harrim_class_level.Selections[2].Features = new BlueprintFeature[] { Warpriest.warpriest_spontaneous_heal };
            harrim_class_level.Selections[3].Selection = Warpriest.warpriest_blessings;
            harrim_class_level.Selections[3].Features = new BlueprintFeature[] { Warpriest.blessings_map["Chaos"], Warpriest.blessings_map["Destruction"] };
            harrim_class_level.Selections[4].Selection = Warpriest.fighter_feat;
            harrim_class_level.Selections[4].Features = new BlueprintFeature[] {NewFeats.weapon_of_the_chosen,
                                                                                 NewFeats.improved_weapon_of_the_chosen,
                                                                                 NewFeats.greater_weapon_of_the_chosen,
                                                                                 library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5"), //pa
                                                                                 NewFeats.furious_focus,
                                                                                 library.Get<BlueprintFeature>("31470b17e8446ae4ea0dacd6c5817d86"), //ws
                                                                                 library.Get<BlueprintParametrizedFeature>("7cf5edc65e785a24f9cf93af987d66b3"), //gws
                                                                                 library.Get<BlueprintParametrizedFeature>("09c9e82965fb4334b984a1e9df3bd088"), //gwf
                                                                                 library.Get<BlueprintFeature>("afd05ca5363036c44817c071189b67e1"), //gsf
                                                                                };
            harrim_class_level.Selections[5].IsParametrizedFeature = false;
            harrim_class_level.Selections[5].Selection = Warpriest.weapon_focus_selection;
            harrim_class_level.Selections[5].Features = new BlueprintFeature[] { library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e") }; //wf
            harrim_class_level.Selections[6].ParametrizedFeature = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"); //wf
            harrim_class_level.Selections[6].ParamWeaponCategory = WeaponCategory.UnarmedStrike;
            harrim_class_level.Selections[7].ParametrizedFeature = library.Get<BlueprintParametrizedFeature>("31470b17e8446ae4ea0dacd6c5817d86"); //ws
            harrim_class_level.Selections[7].ParamWeaponCategory = WeaponCategory.HeavyFlail;
            harrim_class_level.Selections[8].IsParametrizedFeature = false;
            harrim_class_level.Selections[8].Selection = library.Get<BlueprintFeatureSelection>("76d4885a395976547a13c5d6bf95b482"); //af
            harrim_class_level.Selections[8].Features = new BlueprintFeature[] { library.Get<BlueprintFeature>("c27e6d2b0d33d42439f512c6d9a6a601") }; //heavy
            harrim_class_level.Selections[9].ParametrizedFeature = library.Get<BlueprintParametrizedFeature>("09c9e82965fb4334b984a1e9df3bd088"); //gwf
            harrim_class_level.Selections[9].ParamWeaponCategory = WeaponCategory.HeavyFlail;

            harrim_feature.GetComponent<AddFacts>().Facts = harrim_feature.GetComponent<AddFacts>().Facts.Take(1).ToArray();
            //harrim_class_level.Selections[3].Features[0] = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("9ebe166b9b901c746b1858029f13a2c5"); //madness domain instead of chaos

            //change linzi
            var linzi_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("77c11edb92ce0fd408ad96b40fd27121");
            linzi_companion.Dexterity = 15;
            linzi_companion.Charisma = 16;
            linzi_companion.Constitution = 12;
            linzi_companion.Strength = 11;
            var linzi_feature = library.Get<BlueprintFeature>("920cb420385dbb34681b620b6c1b59e9");
            var linzi_class_levels = linzi_feature.GetComponent<AddClassLevels>();
            linzi_class_levels.Skills = new StatType[] { StatType.SkillPersuasion, StatType.SkillKnowledgeWorld, StatType.SkillUseMagicDevice, StatType.SkillThievery, StatType.SkillKnowledgeArcana, StatType.SkillMobility };
            linzi_class_levels.Selections[1].Features[0] = library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab");//point blank shot instead of extra performance
            if (Main.settings.balance_fixes)
            {
                linzi_class_levels.Selections[1].Features[0] = library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665");//precise shot
            }
            //change octavia
            var octavia_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f9161aa0b3f519c47acbce01f53ee217");
            octavia_companion.Dexterity = 16;
            octavia_companion.Intelligence = 17;
            octavia_companion.Constitution = 12;
            octavia_companion.Charisma = 12;
            octavia_companion.Strength = 8;
            //remove rogue level
            var octavia_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("200151a5a5c78a4439d0f6e9fb26620a");

            var octavia_acl = octavia_feature.GetComponent<AddClassLevels>();
            octavia_feature.RemoveComponents<AddClassLevels>(a => a != octavia_acl);
            //octavia_acl.CharacterClass = Arcanist.arcanist_class;
            octavia_acl.Archetypes = new BlueprintArchetype[] { Arcanist.exploiter_wizard_archetype };
            if (Main.settings.balance_fixes)
            {
                Common.addFeatureSelectionToAcl(octavia_acl, Arcanist.arcane_exploits_wizard, Arcanist.item_bond);
            }
            else
            {
                Common.addFeatureSelectionToAcl(octavia_acl, Arcanist.arcane_exploits_wizard, Arcanist.potent_magic);
            }
            //Common.addFeatureSelectionToAcl(octavia_acl, Arcanist.bloodline_selection, library.Get<BlueprintFeature>("4d491cf9631f7e9429444f4aed629791"));
            //Common.addFeatureSelectionToAcl(octavia_acl, library.Get<BlueprintFeatureSelection>("BloodlineArcaneArcaneBondFeature"), library.Get<BlueprintFeature>("97dff21a036e80948b07097ad3df2b30"));
            octavia_acl.Skills = new StatType[] { StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillUseMagicDevice, StatType.SkillMobility };
            octavia_acl.Selections[4].Features[0] = library.Get<BlueprintFeature>("97dff21a036e80948b07097ad3df2b30");// hare familiar
            octavia_acl.Selections[5].Features[0] = library.Get<BlueprintFeature>("f43ffc8e3f8ad8a43be2d44ad6e27914"); //umd
            octavia_companion.Dexterity = 14;
            octavia_companion.Charisma = 14;
            octavia_companion.Strength = 10;
            octavia_acl.Selections[0].Features[0] = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");//improved initiative
            octavia_acl.Selections[6].ParamSpellSchool = SpellSchool.Conjuration;
            //Common.addParametrizedFeatureSelectionToAcl(octavia_acl, library.Get<BlueprintParametrizedFeature>("16fa59cc9a72a6043b566b49184f53fe"), SpellSchool.Conjuration);
            /*octavia_acl.Skills = new StatType[] { StatType.SkillKnowledgeArcana, StatType.SkillThievery, StatType.SkillPersuasion, StatType.SkillMobility };
            //octavia_acl.Selections[1].Features[1] = library.Get<BlueprintFeature>("875fff6feb84f5240bf4375cb497e395"); //opposition enchantment, necromancy
            octavia_acl.Selections[2].Features[0] = Subschools.admixture;
            octavia_acl.Selections[4].Features[0] = library.Get<BlueprintFeature>("97dff21a036e80948b07097ad3df2b30");// hare familiar
            octavia_acl.Selections[5].Features[0] = library.Get<BlueprintFeature>("1621be43793c5bb43be55493e9c45924"); //adaptability persuation
            octavia_acl.Selections[6].ParamSpellSchool = SpellSchool.Conjuration;*/

            //remove dex buff if it is already activated
            Action<UnitDescriptor> fix_action = delegate (UnitDescriptor u)
            {
                var dex_buff = library.Get<BlueprintBuff>("b649a3d906a6ff44a9bb01f939ef1a6f");
                var buff = u.Buffs.GetBuff(dex_buff);
                if (buff == null)
                {
                    return;
                }
                var dex_toggle = library.Get<BlueprintActivatableAbility>("3553bda4d6dfe6344ad89b25f7be939a");
                if (!u.HasFact(dex_toggle))
                {
                    buff.Remove();
                }
            };
            SaveGameFix.save_game_actions.Add(fix_action);
            //change regongar and fix his portrait
            var reg_portrait = library.Get<BlueprintPortrait>("6e7302bb773adf04299dbe8832562d50").BackupPortrait = null;
            var regognar_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b090918d7e9010a45b96465de7a104c3");
            regognar_companion.Dexterity = 12;
            var regognar_levels = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("12ee53c9e546719408db257f489ec366").GetComponent<AddClassLevels>();
            regognar_levels.Levels = 1;
            regognar_levels.Selections = regognar_levels.Selections.AddToArray(new SelectionEntry()
            {
                Selection = library.Get<BlueprintFeatureSelection>("5294b338c6084396abbe63faab09049c"),
                Features = new BlueprintFeature[] { BloodlinesFix.bloodline_familiar }
            },
                                                                                new SelectionEntry()
                                                                                {
                                                                                    Selection = BloodlinesFix.bloodline_familiar,
                                                                                    Features = new BlueprintFeature[] { library.Get<BlueprintFeature>("61aeb92c176193e48b0c9c50294ab290") } //lizard
                                                                                }
                                                                              );

            //change ekun
            var ekun_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("d5bc1d94cd3e5be4bbc03f3366f67afc");
            ekun_companion.Strength = 14;
            ekun_companion.Constitution = 12;
            ekun_companion.Dexterity = 17;
            ekun_companion.Wisdom = 14;
            ekun_companion.Charisma = 8;
            ekun_companion.Intelligence = 12;
            var ekun_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0bc6dc9b6648a744899752508addae8c");
            var ekun_class_level = ekun_feature.GetComponent<AddClassLevels>();
            ekun_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Dexterity;
            //change jubilost
            var jubilost_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("3f5777b51d301524c9b912812955ee1e");
            jubilost_companion.Dexterity = 16;
            jubilost_companion.Wisdom = 10;
            jubilost_companion.Intelligence = 17;
            jubilost_companion.Constitution = 12;
            jubilost_companion.Strength = 12;
            jubilost_companion.Charisma = 8;
            var jubilost_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c9618e3c61e65114b994f3fabcae1d97");
            var jubilost_acl = jubilost_feature.GetComponent<AddClassLevels>();
            jubilost_acl.Levels = 1;
            jubilost_acl.Archetypes = jubilost_acl.Archetypes.AddToArray(Archetypes.Preservationist.archetype);
            jubilost_acl.Skills = new StatType[] { StatType.SkillKnowledgeWorld, StatType.SkillPersuasion, StatType.SkillThievery, StatType.SkillUseMagicDevice, StatType.SkillKnowledgeArcana, StatType.SkillPerception };
            //change nok-nok
            var noknok_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f9417988783876044b76f918f8636455");
            noknok_companion.Strength = 11;
            noknok_companion.Constitution = 14;
            noknok_companion.Wisdom = 10;
            noknok_companion.GetComponent<AddClassLevels>().Levels = 1;
            noknok_companion.GetComponent<AddClassLevels>().Skills = new StatType[] { StatType.SkillMobility, StatType.SkillThievery, StatType.SkillPerception, StatType.SkillStealth, StatType.SkillUseMagicDevice, StatType.SkillLoreNature, StatType.SkillAthletics };
            //change jaethal to archer
            var jaethal_feature_list = library.Get<BlueprintFeature>("34280596dd550074ca55bd15285451b3");
            var jaethal_selections = jaethal_feature_list.GetComponent<AddClassLevels>();
            jaethal_selections.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillMobility, StatType.SkillLoreReligion, StatType.SkillAthletics };
            //jaethal_selections.Selections[1].Features = jaethal_selections.Selections[1].Features.Skip(1).ToArray();
            jaethal_selections.Selections[1].Features[0] = library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab"); //point blank shot
            if (Main.settings.balance_fixes)
            {
                jaethal_selections.Selections[1].Features[0] = library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665"); //precise shot
            }
            jaethal_selections.Selections[2].Features[0] = Inquisitions.conversion;
            var jaethal_unit = library.Get<BlueprintUnit>("32d2801eddf236b499d42e4a7d34de23");
            jaethal_unit.Strength = 12;
            jaethal_unit.Dexterity = 17;
            jaethal_unit.Charisma = 10;
            jaethal_unit.Wisdom = 16;
            jaethal_unit.Intelligence = 8;
            jaethal_unit.Constitution = 12;
            jaethal_selections.LevelsStat = StatType.Dexterity;
            jaethal_unit.Body.PrimaryHandAlternative1 = library.Get<BlueprintItemWeapon>("7998cd1409fe1194583b64180df4f216"); //composite longbow

            var varn_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("e83a03d50fedd35449042ce73f1b6908");
            var varn_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("2babd2d4687b5ee428966322eccfe4b6");
            varn_companion.Dexterity = 18;
            varn_companion.Intelligence = 14;
            varn_companion.Wisdom = 8;
            varn_companion.Charisma = 12;
            var varn_class_levels = varn_feature.GetComponent<AddClassLevels>();
            varn_class_levels.Skills = new StatType[] { StatType.SkillMobility, StatType.SkillThievery, StatType.SkillPerception, StatType.SkillStealth, StatType.SkillPersuasion, StatType.SkillUseMagicDevice };

            var cephales_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("77c5eb949dffb9f45abcc7a78a2d281f");
            var cephales_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("d152b07305353474ba15d750015d99ee");
            cephales_companion.Strength = 8;
            cephales_companion.Constitution = 12;
            cephales_companion.Dexterity = 14;
            cephales_companion.Intelligence = 18;
            cephales_companion.Wisdom = 12;
            cephales_companion.Charisma = 12;
            var cephales_class_levels = cephales_feature.GetComponent<AddClassLevels>();
            cephales_class_levels.Selections[0].Features[0] = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74"); //improved initiative
            cephales_class_levels.Selections[0].Features[1] = library.Get<BlueprintParametrizedFeature>("16fa59cc9a72a6043b566b49184f53fe"); //spell focus (necromancy)
            cephales_class_levels.Selections = cephales_class_levels.Selections.AddToArray(new SelectionEntry()
            {
                Selection = library.Get<BlueprintFeatureSelection>("5294b338c6084396abbe63faab09049c"),
                Features = new BlueprintFeature[] { BloodlinesFix.blood_havoc },
                ParametrizedFeature = library.Get<BlueprintParametrizedFeature>("16fa59cc9a72a6043b566b49184f53fe"),
                ParamSpellSchool = SpellSchool.Necromancy,
                IsParametrizedFeature = true
            }
                                                                                            );
            cephales_class_levels.Selections[0].Features[3] = library.Get<BlueprintParametrizedFeature>("5b04b45b228461c43bad768eb0f7c7bf");
            cephales_class_levels.Selections[0].Features[4] = library.Get<BlueprintFeature>("f180e72e4a9cbaa4da8be9bc958132ef");


            //change kallike
            var kalikke_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("385e8d69b89992844b0992caf666a5fd");
            var kalikke_acl = kalikke_feature.GetComponent<AddClassLevels>();
            kalikke_acl.Levels = 1;
            kalikke_acl.Selections[0].Features[0] = library.Get<BlueprintFeature>("90e54424d682d104ab36436bd527af09"); //weapon finesse
            //kalikke_acl.Selections[4].Features = kalikke_acl.Selections[4].Features.Reverse().ToArray();
            kalikke_acl.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillMobility, StatType.SkillStealth, StatType.SkillLoreNature };
            var kalikke_companion = library.Get<BlueprintUnit>("c807d18a89f96c74f8bb48b31b616323");
            kalikke_companion.Strength = 9;
            kalikke_companion.Dexterity = 17;
            kalikke_companion.Intelligence = 10;
            kalikke_companion.Constitution = 16;
            kalikke_companion.Charisma = 8;
            kalikke_companion.Wisdom = 12;

            var elemental_focus = library.Get<BlueprintFeatureSelection>("bb24cc01319528849b09a3ae8eec0b31");
            var kanerah_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ccb52e235941e0442be0cb0ee5570f07");
            var kanerah_acl = kanerah_feature.GetComponent<AddClassLevels>();
            kanerah_acl.Levels = 1;
            kanerah_acl.Selections[0].Features[0] = elemental_focus; //elemental_focus
            kanerah_acl.Selections[12].Features[0] = library.Get<BlueprintFeature>("c82bc8134f3a6e24994b8ef70fb4014a"); //tiefling standard
            Common.addFeatureSelectionToAcl(kanerah_acl, elemental_focus, elemental_focus.AllFeatures.Last()); //fire
            kanerah_acl.Selections[4].Features = kalikke_acl.Selections[4].Features.Reverse().ToArray();
            kanerah_acl.Skills = new StatType[] { StatType.SkillStealth, StatType.SkillMobility, StatType.SkillUseMagicDevice, StatType.SkillKnowledgeWorld };
            var kanerah_companion = library.Get<BlueprintUnit>("f1c0b181a534f4940ae17f243a5968ec");
            kanerah_companion.Strength = 9;
            kanerah_companion.Dexterity = 17;
            kanerah_companion.Intelligence = 16;
            kanerah_companion.Constitution = 14;
            kanerah_companion.Charisma = 8;
            kanerah_companion.Wisdom = 10;
        }


        internal static void fixMissingSlamProficiency()
        {
            //add it to base abilities 
            var skill_use_ability = library.Get<BlueprintFeature>("e4c33ff99d638744686112e2a5f49856");
            skill_use_ability.AddComponent(Common.createAddWeaponProficiencies(WeaponCategory.OtherNaturalWeapons));
            Action<UnitDescriptor> add_slam_proficiency = delegate (UnitDescriptor u)
            {
                if (!u.Proficiencies.Contains(WeaponCategory.OtherNaturalWeapons))
                {
                    u.Proficiencies.Add(WeaponCategory.OtherNaturalWeapons);
                }
            };

            SaveGameFix.save_game_actions.Add(add_slam_proficiency);
        }

        internal static void fixWebSchool()
        {
            var web = library.Get<BlueprintAbility>("134cb6d492269aa4f8662700ef57449f");
            web.GetComponent<SpellComponent>().School = SpellSchool.Conjuration;

            library.Get<BlueprintSpellList>("ac551db78c1baa34eb8edca088be13cb").SpellsByLevel[2].Spells.Add(web); //add to lust
            library.Get<BlueprintSpellList>("17c0bfe5b7c8ac3449da655cdcaed4e7").SpellsByLevel[2].Spells.Remove(web); //remove from wrath

            library.Get<BlueprintSpellList>("69a6eba12bc77ea4191f573d63c9df12").SpellsByLevel[2].Spells.Add(web); //add to conjuration
            library.Get<BlueprintSpellList>("becbcfeca9624b6469319209c2a6b7f1").SpellsByLevel[2].Spells.Remove(web);//remove from conjuration
        }


        internal static void fixSpellRemoveFearBuff()
        {
            var fearless_rage_buff = library.Get<BlueprintBuff>("7f043b6980cdcbe42b52f0837a0e7361");
            var fearless_defensive_stance_buff = library.Get<BlueprintBuff>("993a5300cc84fde4bb4df441bf92d701");
            fearless_defensive_stance_buff.AddComponent(Helpers.CreateAddFactContextActions(activated: Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Shaken | SpellDescriptor.Fear)));
            fearless_rage_buff.AddComponent(Helpers.CreateAddFactContextActions(activated: Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Shaken | SpellDescriptor.Fear)));
            var buff = library.Get<BlueprintBuff>("c5c86809a1c834e42a2eb33133e90a28");
            buff.AddComponent(Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(fearless_rage_buff, Helpers.CreateContextDuration(), duration_seconds: 1)));
        }


        internal static void fixSpellUnbreakableHeartBuff()
        {
            var buff = library.Get<BlueprintBuff>("6603b27034f694e44a407a9cdf77c67e");
            var suppress = buff.GetComponent<SuppressBuffs>();
            buff.AddComponent(Helpers.CreateAddFactContextActions(activated: Common.createContextActionRemoveBuffsByDescriptor(suppress.Descriptor)));
            buff.RemoveComponent(suppress);
        }

        internal static void fixJudgments()
        {
            //fix smiting buffs
            library.Get<BlueprintActivatableAbility>("72fe16312b4479145afc6cc6c87cd08f").Buff = library.Get<BlueprintBuff>("481b03bc6cbc5af448b1f6cb70d88859");//alignment
            library.Get<BlueprintActivatableAbility>("2c448ab4135c7c741b6f0f223901f9fa").Buff = library.Get<BlueprintBuff>("2e3f01df36b508b4e9186bab7a337dfa");//adamantite
        }

        internal static void fixDomains()
        {
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            var druid_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var domain_classes_and_druid = new BlueprintCharacterClass[] { cleric_class, inquisitor_class, druid_class };
            var domain_classes = new BlueprintCharacterClass[] { cleric_class, inquisitor_class };

            //weather
            //add missing druid scaling to Storm Burst
            library.Get<BlueprintAbility>("f166325c271dd29449ba9f98d11542d9").ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", domain_classes_and_druid));

            //fix luck domain
            var luck_domain_greater_resource = library.Get<BlueprintAbilityResource>("b209ca75fbea5144c9d73ecb29055a08");
            var luck_domain_greater_ability = library.Get<BlueprintAbility>("0e0668a703fbfcf499d9aa9d918b71ea");

            if (luck_domain_greater_ability.GetComponent<AbilityResourceLogic>() == null)
            {
                luck_domain_greater_ability.AddComponent(luck_domain_greater_resource.CreateResourceLogic());
            }

            //fix strength surge to work on allies
            var strenght_surge = library.Get<BlueprintAbility>("6e3cbd10e50c6774e869ff8e20f2b352");
            strenght_surge.CanTargetEnemies = false;
            strenght_surge.CanTargetFriends = true;
            strenght_surge.StickyTouch.TouchDeliveryAbility.CanTargetEnemies = false;
            strenght_surge.StickyTouch.TouchDeliveryAbility.CanTargetFriends = true;
            //fix aura of madness to be a toggle instead of ability
            var madness_greater_resource = library.Get<BlueprintAbilityResource>("3289ee86c57f6134d81770865c315e8b");
            var madness_domain_buff = library.Get<BlueprintBuff>("73192f96dd97b634cb794ae42f92c2ff");
            var madness_area = library.Get<BlueprintAbilityAreaEffect>("19ee79b1da25ea049ba4fea92c2a4025");
            var madness_greater_feature = library.Get<BlueprintFeature>("9acc8ab2f313d0e49bb01e030c868e3f");

            madness_greater_resource.SetIncreasedByLevel(0, 1, new BlueprintCharacterClass[] { cleric_class, inquisitor_class });
            madness_area.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { cleric_class, inquisitor_class }, StatType.Wisdom));
            var toggle = Common.buffToToggle(madness_domain_buff, UnitCommand.CommandType.Standard, false,
                                             madness_greater_resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.NewRound));
            madness_greater_feature.RemoveComponents<ReplaceAbilitiesStat>();
            madness_greater_feature.ReplaceComponent<AddFacts>(r => r.Facts[0] = toggle);



            //protection domain
            /*var protection_bonus_context_rank = Helpers.CreateContextRankConfig(progression: ContextRankProgression.OnePlusDivStep,
                                                                                             startLevel: 1,
                                                                                             stepLevel: 5,
                                                                                             min: 1,
                                                                                             classes: domain_classes
                                                                                             );
            var protection_domain_remove_save_bonus = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>("74a4fb45f23705d4db2784d16eb93138"); //resistant touch self
            protection_domain_remove_save_bonus.AddComponent(protection_bonus_context_rank);

            var protection_domain_save_bonus = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>("2ddb4cfc3cfd04c46a66c6cd26df1c06"); //resitant touch bonus
            protection_domain_save_bonus.ReplaceComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>(protection_bonus_context_rank);*/
        }


        internal static void fixJaethalUndeadFeature()
        {
            var jaethal_immortality = library.Get<BlueprintFeature>("1ed5fac73a4dc054d8411f24cf09d703");
            jaethal_immortality.AddComponents(Helpers.CreateAddStatBonus(StatType.Strength, 2, ModifierDescriptor.Racial),
                                              Helpers.CreateAddStatBonus(StatType.Dexterity, 2, ModifierDescriptor.Racial),
                                              Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.NaturalArmor),
                                              Common.createContextFormDR(5, Kingmaker.Enums.Damage.PhysicalDamageForm.Slashing),
                                              Helpers.CreateAddFact(library.Get<BlueprintFeature>("a9ac84c6f48b491438f91bb237bc9212")) //channel resistance
                                              );
        }


        internal static void fixDispellingStrikeCL()
        {
            //var slayer = library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");
            var dispelling_attack = library.Get<BlueprintFeature>("1b92146b8a9830d4bb97ab694335fa7c");
           // dispelling_attack.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(slayer)));
            dispelling_attack.ReplaceComponent<ContextSetAbilityParams>(Helpers.Create<NewMechanics.ReplaceCasterLevelOfFactWithContextValue>(r =>
                                                                                                                                                    {
                                                                                                                                                        r.Feature = dispelling_attack;
                                                                                                                                                        r.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                                                    }
                                                                                                                                                    )
                                                                           );
        }


        internal static void fixBarbarianRageAC()
        {
            var rage = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            var components = rage.GetComponents<Kingmaker.UnitLogic.FactLogic.AddContextStatBonus>();
            foreach (var c in components)
            {
                if (c.Stat == Kingmaker.EntitySystem.Stats.StatType.AC)
                {
                    c.Value = Common.createSimpleContextValue(2);
                }
            }
        }


        internal static void fixMagicVestment()
        {
            fixMagicVestmentArmor();
            fixMagicVestmentShield();
        }

        static void fixMagicVestmentArmor()
        {
            var magic_vestement_armor_buff = Main.library.Get<BlueprintBuff>("9e265139cf6c07c4fb8298cb8b646de9");
            var armor_enchant = Helpers.Create<NewMechanics.EnchantmentMechanics.BuffContextEnchantArmor>();
            armor_enchant.value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
            armor_enchant.enchantments = ArmorEnchantments.temporary_armor_enchantments;

            magic_vestement_armor_buff.ComponentsArray = new BlueprintComponent[] {armor_enchant,
                                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel,
                                                                                   progression: ContextRankProgression.DivStep, startLevel: 4, min:1, stepLevel: 4, max: 5, type: AbilityRankType.StatBonus)
                                                                                  };
            magic_vestement_armor_buff.Stacking = StackingType.Replace;

            library.Get<BlueprintAbility>("956309af83352714aa7ee89fb4ecf201").AddComponent(Helpers.Create<NewMechanics.AbilitTargetHasArmor>());
        }


        static void fixMagicVestmentShield()
        {
            var magic_vestement_shield_buff = Main.library.Get<BlueprintBuff>("2e8446f820936a44f951b50d70a82b16");
            var shield_enchant = Helpers.Create<NewMechanics.EnchantmentMechanics.BuffContextEnchantShield>();
            shield_enchant.value = Helpers.CreateContextValue(AbilityRankType.StatBonus);
            shield_enchant.enchantments = ArmorEnchantments.temporary_shield_enchantments;

            magic_vestement_shield_buff.ComponentsArray = new BlueprintComponent[] {shield_enchant,
                                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel,
                                                                                   progression: ContextRankProgression.DivStep, startLevel: 4, min:1, stepLevel: 4, max: 5, type: AbilityRankType.StatBonus)
                                                                                  };
            magic_vestement_shield_buff.Stacking = StackingType.Replace;
            library.Get<BlueprintAbility>("adcda176d1756eb45bd5ec9592073b09").AddComponent(Helpers.Create<NewMechanics.AbilitTargetHasShield>());
        }

        internal static BlueprintFeatureSelection dd_feat_subselection;
        internal static void fixDragonDiscipleBonusFeat()
        {
            //to allow select feats from other bloodline classes (bloodrager for example)
            var dd_feat_selection = Main.library.Get<BlueprintFeatureSelection>("f4b011d090e8ae543b1441bd594c7bf7");
            dd_feat_subselection = Main.library.CopyAndAdd<BlueprintFeatureSelection>("f4b011d090e8ae543b1441bd594c7bf7", "DragonDiscipleDraconicFeatSubselection", "");

            dd_feat_subselection.Features = new BlueprintFeature[] { dd_feat_selection };
            dd_feat_subselection.AllFeatures = dd_feat_subselection.Features;
            dd_feat_subselection.SetDescription("Upon reaching 2nd level, and every three levels thereafter, a dragon disciple receives one bonus feat, chosen from the draconic bloodline’s bonus feat list.");

            var dragon_disciple_progression = Main.library.Get<BlueprintProgression>("69fc2bad2eb331346a6c777423e0d0f7");
            foreach (var le in dragon_disciple_progression.LevelEntries)
            {
                for (int i = 0; i < le.Features.Count; i++)
                {
                    if (le.Features[i] == dd_feat_selection)
                    {
                        le.Features[i] = dd_feat_subselection;
                    }
                }
            }

        }


        static internal void fixNaturalACStacking()
        {
            ModifiableValue.DefaultStackingDescriptors.Remove(ModifierDescriptor.NaturalArmor);
            //replace natural armor on dd to racial to allow it to stack
            ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("aa4f9fd22a07ddb49982500deaed88f9").GetComponent<AddStatBonus>().Descriptor = ModifierDescriptor.Racial;
        }


        static internal void fixInspiredFerocity()
        {
            var reckless_stance_switch = Main.library.Get<BlueprintBuff>("c52e4fdad5df5d047b7ab077a9907937");
            var inspire_ferocity_rage_buff = Main.library.Get<BlueprintBuff>("9a8a16f5734eec7439c5c77000316742");
            var inspire_ferocity_switch_buff = Main.library.Get<BlueprintBuff>("4b3fb3c9473a00f4fa526f4bd3fc8b7a");
            var c = reckless_stance_switch.GetComponent<AddFactContextActions>();
            c.Deactivated.Actions = c.Deactivated.Actions.AddToArray(Common.createContextActionRemoveBuff(inspire_ferocity_rage_buff)); //remove inspired ferocity when reckless stance is deactivated
            var condition_on = (Conditional)c.Activated.Actions[0];
            var apply_ferocity = Common.createContextActionApplyBuff(inspire_ferocity_rage_buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true);
            condition_on.IfTrue.Actions = condition_on.IfTrue.Actions.AddToArray(Helpers.CreateConditional(Common.createContextConditionCasterHasFact(inspire_ferocity_switch_buff),
                                                                                 apply_ferocity, null)
                                                                                 );
        }


        static internal void fixLethalStance()
        {
            //it should be competence bonus instead of untyped
            var buff = Main.library.Get<BlueprintBuff>("c6271b3183c48d54b8defd272bea0665");
            buff.GetComponent<AttackTypeAttackBonus>().Descriptor = ModifierDescriptor.Competence;
        }


        static internal void fixItemBondForSpontnaeousCasters()
        {
            var item_bond_spontaneous = library.CopyAndAdd<BlueprintAbility>("e5dcf71e02e08fc448d9745653845df1", "ItemBondSpontaneousAbility", "");
            item_bond_spontaneous.ReplaceComponent<AbilityRestoreSpellSlot>(Helpers.Create<AbilityRestoreSpontaneousSpell>(a => a.SpellLevel = 10));

            var item_bond_feature = library.Get<BlueprintFeature>("2fb5e65bd57caa943b45ee32d825e9b9");
            var add_facts = item_bond_feature.GetComponent<AddFacts>();
            if (add_facts.Facts.Length == 1)
            {
                add_facts.Facts = add_facts.Facts.AddToArray(item_bond_spontaneous);
            }
        }


        static internal void fixTrapfinding()
        {
            var trapfinding_rogue = library.Get<BlueprintFeature>("dbb6b3bffe6db3547b31c3711653838e");
            var trapfinding_slayer = library.Get<BlueprintFeature>("e3c12938c2f93544da89824fbe0933a5");

            trapfinding_rogue.AddComponent(Helpers.CreateAddContextStatBonus(StatType.SkillThievery, ModifierDescriptor.None));
            trapfinding_slayer.AddComponent(Helpers.CreateAddContextStatBonus(StatType.SkillThievery, ModifierDescriptor.None));
            trapfinding_rogue.SetDescription("A rogue adds 1/2 her level on Perception and Trickery checks.");
        }


        static internal void fixAnimalSizeChange()
        {
            //to make it compatible with size increasing efects
            string[] large_upgrades = new string[] {"abda5a76b8a5901478495ffdc5450c9e", //bear
                                                    "59f2a25bc27f1a2408721dc24f0589c5", //boar
                                                    "c938099ca0438b242b3edecfa9083e9f", //centiepede
                                                    "9763e77bfdcd32541848a9095ac53455", //dog
                                                    "70206f918cecc9440925dad944760928", //elk
                                                    "6a23d16a4476af644af89d91f9f96790", //mammoth
                                                    "f1e949c3d93fc234da255b94629c5b3a", //smilodon
                                                    "fb27e69b4ca4e904bac8e97833c4a12c", //wolf
                                                    };
            string[] medium_upgrades = new string[] {"beb608c45bb2aef42802e2afdf018a32", //monitor
                                                     "b8c98af302ee334499d30a926306327d", //leopard
                                                    };

            foreach (var id in large_upgrades)
            {
                var feature = library.Get<BlueprintFeature>(id);
                feature.ReplaceComponent<ChangeUnitSize>(Helpers.Create<SizeMechanics.PermanentSizeOverride>(p => p.size = Size.Large));
            }

            foreach (var id in medium_upgrades)
            {
                var feature = library.Get<BlueprintFeature>(id);
                feature.ReplaceComponent<ChangeUnitSize>(Helpers.Create<SizeMechanics.PermanentSizeOverride>(p => p.size = Size.Medium));
            }
        }


        static internal void fixTeamworkFeats()
        {
            int fix_range = 2;  //2 meters ~ 7 feet
            var back_to_back = library.Get<BlueprintFeature>("c920f2cd2244d284aa69a146aeefcb2c");
            back_to_back.GetComponent<BackToBack>().Radius = fix_range;
            var shield_wall = library.Get<BlueprintFeature>("8976de442862f82488a4b138a0a89907");
            shield_wall.GetComponent<ShieldWall>().Radius = fix_range;
            var shake_it_off = library.Get<BlueprintFeature>("6337b37f2a7c11b4ab0831d6780bce2a");
            shake_it_off.GetComponent<ShakeItOff>().Radius = fix_range;
            var allied_spell_caster = library.Get<BlueprintFeature>("9093ceeefe9b84746a5993d619d7c86f");
            allied_spell_caster.GetComponent<AlliedSpellcaster>().Radius = fix_range;
            allied_spell_caster.AddComponent(Helpers.Create<TeamworkMechanics.AlliedSpellcasterSameSpellBonus>(a => { a.Radius = fix_range; a.AlliedSpellcasterFact = allied_spell_caster; }));
            allied_spell_caster.SetDescription("Whenever you are adjacent to an ally who also has this feat, you receive a +2 competence bonus on level checks made to overcome spell resistance. If your ally has the same spell prepared (or known with a slot available if they are spontaneous spellcasters), this bonus increases to +4 and you receive a +1 bonus to the caster level for all level-dependent variables, such as duration, range, and effect.");
            var shielded_caster = library.Get<BlueprintFeature>("0b707584fc2ea724aa72c396c2230dc7");
            shielded_caster.GetComponent<ShieldedCaster>().Radius = fix_range;
            var coordinated_maneuvers = library.Get<BlueprintFeature>("b186cea78dce3a04aacff0a81786008c");
            coordinated_maneuvers.GetComponent<CoordinatedManeuvers>().Radius = fix_range;
        }


        static internal void fixSpellRanges()
        {
            library.Get<BlueprintAbility>("b24583190f36a8442b212e45226c54fc").Range = AbilityRange.Medium; //change range of wail of banshee to medium since by the time you can cast it will be 25 + 17/2 * 5 = 65
            library.Get<BlueprintAbility>("ba48abb52b142164eba309fd09898856").Range = AbilityRange.Medium; //change range of polar midnight to medium since by the time you can cast it will be 25 + 17/2 * 5 = 65
        }

        static internal void fixEcclesitheurge()
        {
            var cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var archetype = library.Get<BlueprintArchetype>("472af8cb3de628f4a805dc4a038971bc");
            archetype.AddSkillPoints = 0;

            var bonded = library.Get<BlueprintFeature>("aa34ca4f3cd5e5d49b2475fcfdf56b24");

            bonded.AddComponent(Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => { c.value = -2; c.spells = new BlueprintAbility[0]; }));
            bonded.SetDescription(bonded.Description + "\nThis ability replaces the increase to channel energy gained at 3rd level.");

            var long_blessing = library.Get<BlueprintAbility>("3ef665bb337d96946bcf98a11103f32f");
            long_blessing.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                             classes: new BlueprintCharacterClass[] { cleric_class }, stepLevel: 2, startLevel: 3));
        }


        static internal void fixIncreasedDamageReduction()
        {
            var drs = new BlueprintFeature[] {library.Get<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8"), //barbarian
                                              library.Get<BlueprintFeature>("e71bd204a2579b1438ebdfbf75aeefae"), //invulnerable rager
                                              library.Get<BlueprintFeature>("2edbf059fd033974bbff67960f15974d"), //mad dog
                                              library.Get<BlueprintFeature>("427b4a34432389042861b8db4cbe3d99"), //invulnerable rager extreme endurance
                                             };

            var increased_dr = library.Get<BlueprintFeature>("ddaee203ee4dcb24c880d633fbd77db6");
            var increased_dr_stalwart = library.Get<BlueprintFeature>("d10496e92d0799a40bb3930b8f4fda0d");

            foreach (var dr in drs)
            {
                var context_rank_config = dr.GetComponent<ContextRankConfig>();
                var feature_list = Helpers.GetField<BlueprintFeature[]>(context_rank_config, "m_FeatureList").ToList();

                while (feature_list.Contains(increased_dr))
                {
                    feature_list.Remove(increased_dr);
                }
                Helpers.SetField(context_rank_config, "m_FeatureList", feature_list.ToArray());
                //dr.AddComponent(Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] { dr }));
            }


            var dr_buff = Helpers.CreateBuff("BarbarianRageIncreasedDrBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             null,
                                             Common.createContextPhysicalDR(Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                                             Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, type: AbilityRankType.StatBonus,
                                                                             featureList: new BlueprintFeature[] { increased_dr, increased_dr })
                                            );
            dr_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var barbarian_rage_buff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(barbarian_rage_buff, dr_buff, increased_dr);


            var dr_buff_stalwart = Helpers.CreateBuff("StalwartDefenderIncreasedDrBuff",
                                 "",
                                 "",
                                 "",
                                 null,
                                 null,
                                 increased_dr_stalwart.ComponentsArray
                                );
            dr_buff_stalwart.SetBuffFlags(BuffFlags.HiddenInUi);
            increased_dr_stalwart.ComponentsArray = new BlueprintComponent[0];
            //fix it to give dr2 as for unchained barb
            increased_dr_stalwart.SetDescription("The stalwart defender's damage reduction from this class increases by 2/—. This increase is always active while the stalwart defender is in a defensive stance. He can select this power up to two times. Its effects stack. The stalwart defender must be at least 6th level before selecting this defensive power.");
            dr_buff_stalwart.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, featureList: new BlueprintFeature[] { increased_dr_stalwart, increased_dr_stalwart }));


            var defensive_stance_buff = library.Get<BlueprintBuff>("3dccdf27a8209af478ac71cded18a271");
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(defensive_stance_buff, dr_buff_stalwart, increased_dr_stalwart);


            //fix resist energy for invulnerable rager
            var resists = drs[3].GetComponents<ResistEnergyContext>();
            foreach (var r in resists)
            {
                r.UseValueMultiplier = false;
                r.ValueMultiplier = Common.createSimpleContextValue(1);
                r.Value = Common.createSimpleContextValue(1);
            }
            drs[3].RemoveComponents<ContextRankConfig>();

        }


        internal static void fixChannelEnergyHeal()
        {
            //in vanilla it uses shared value for healing same amount to everyone, to make it uniform with other abilities we replace heal amount with context value
            var heals = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("9be3aa47a13d5654cbcb8dbd40c325f2"), //negative cleric
                library.Get<BlueprintAbility>("f5fc9a1a2a3c1a946a31b320d1dd31b2"), //cleric
                library.Get<BlueprintAbility>("6670f0f21a1d7f04db2b8b115e8e6abf"), //paladin
                library.Get<BlueprintAbility>("0c0cf7fcb356d2448b7d57f2c4db3c0c"), //hospitalier
                library.Get<BlueprintAbility>("574cf074e8b65e84d9b69a8c6f1af27b"), //empyreal
            };

            foreach (var h in heals)
            {
                var new_actions = Common.changeAction<ContextActionHealTarget>(h.GetComponent<AbilityEffectRunAction>().Actions.Actions,
                                                               c => c.Value = Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default), 0));

                h.ReplaceComponent<AbilityEffectRunAction>(c => c.Actions = Helpers.CreateActionList(new_actions));
            }
        }

        internal static void fixVitalStrike()
        {
            BlueprintAbility[] vital_strikes = new BlueprintAbility[] {library.Get<BlueprintAbility>("efc60c91b8e64f244b95c66b270dbd7c"),
                                                                       library.Get<BlueprintAbility>("c714cd636700ac24a91ca3df43326b00"),
                                                                       library.Get<BlueprintAbility>("11f971b6453f74d4594c538e3c88d499")
                                                                      };
            foreach (var a in vital_strikes)
            {
                Helpers.SetField(a, "m_IsFullRoundAction", false);
            }
        }


        internal static void fixArcheologistLuck()
        {
            var archaeologist_luck = library.Get<BlueprintActivatableAbility>("12dc796147c42e04487fcad3aaa40cea");
            archaeologist_luck.Group = ActivatableAbilityGroup.BardicPerformance;
            Helpers.SetField(archaeologist_luck, "m_ActivateWithUnitCommand", UnitCommand.CommandType.Swift);
        }


        internal static void addRangerImprovedFavoredTerrain()
        {
            var improved_favored_terrain = library.CopyAndAdd<BlueprintFeatureSelection>("a6ea422d7308c0d428a541562faedefd", "ImprovedFavoredTerrain", "");
            improved_favored_terrain.Mode = SelectionMode.OnlyRankUp;
            improved_favored_terrain.SetName("Improved Favored Terrain");

            foreach (var f in improved_favored_terrain.AllFeatures)
            {
                f.Ranks = 10;
            }

            var ranger_progression = library.Get<BlueprintProgression>("97261d609529d834eba4fd4da1bc44dc");
            ranger_progression.LevelEntries[7].Features.Add(improved_favored_terrain);
            ranger_progression.LevelEntries[12].Features.Add(improved_favored_terrain);
            ranger_progression.LevelEntries[17].Features.Add(improved_favored_terrain);
            ranger_progression.UIGroups = ranger_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(improved_favored_terrain, improved_favored_terrain, improved_favored_terrain));
        }


        internal static void disallowMultipleFamiliars()
        {
            var familiar = library.Get<BlueprintFeatureSelection>("363cab72f77c47745bf3a8807074d183");

            foreach (var f in familiar.AllFeatures)
            {
                f.AddComponent(Helpers.Create<PrerequisiteMechanics.PrerequisiteNoFeatures>(p => p.Features = familiar.AllFeatures.RemoveFromArray(f)));
            }
        }

        internal static void fixSacredmasterHunterTactics()
        {
            //do not remove teamwork features that companion does not have
            var sh_teamwork_share = library.Get<BlueprintFeature>("e1f437048db80164792155102375b62c");
            var share_old = sh_teamwork_share.GetComponent<ShareFeaturesWithCompanion>();
            var share_new = Helpers.Create<CompanionMechanics.ShareFeaturesWithCompanion2>(s => s.Features = share_old.Features);
            sh_teamwork_share.ReplaceComponent(share_old, share_new);
        }

        internal static void refixBardicPerformanceOverlap()
        {
            //after 2.1.2 dev's fix
            var abilities = library.GetAllBlueprints().OfType<BlueprintActivatableAbility>().Where(a => a.Group == ActivatableAbilityGroup.BardicPerformance);
            foreach (var a in abilities)
            {
                a.Buff.RemoveComponents<AddFactContextActions>();
            }
        }

        //forbid bard song overlap on bardic performance
        [Harmony12.HarmonyPatch(typeof(ActivatableAbility))]
        [Harmony12.HarmonyPatch("OnTurnOn", Harmony12.MethodType.Normal)]
        class ActivatableAbilityy__OnTurnOn__Patch
        {
            static void Postfix(ActivatableAbility __instance)
            {
                Main.TraceLog();
                if (__instance.Blueprint.Group != ActivatableAbilityGroup.BardicPerformance)
                {
                    return;
                }

                var activated_performances = __instance.Owner.ActivatableAbilities.Enumerable.Where(a => __instance.Owner.Buffs.HasFact(a.Blueprint.Buff) && !a.IsOn
                                                                                                         && a.Blueprint.Group == ActivatableAbilityGroup.BardicPerformance);
                foreach (var a in activated_performances)
                {
                    if (a != __instance)
                    {
                        (__instance.Owner.Buffs.GetFact(a.Blueprint.Buff) as Buff).Remove();
                    }
                }
            }
        }

        internal static void fixElementalMovementWater()
        {
            var feature_water = library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8");
            feature_water.ReplaceComponent<AbilityTargetHasCondition>(Helpers.Create<AddCondition>(c => c.Condition = Kingmaker.UnitLogic.UnitCondition.ImmuneToCombatManeuvers));

            var airborne = library.Get<BlueprintFeature>("70cffb448c132fa409e49156d013b175");

            var feature_air = library.Get<BlueprintFeature>("1ae6835b8f568d44c8deb911f74762e4");
            feature_air.ComponentsArray = FixFlying.airborne.ComponentsArray;

            feature_air.SetDescription("At 15th level, you are able to fly. Yoy get immunity to difficult terrain and ground-based effects as well as +3 melee dodge AC bonus against non-flying creatures.");
        }


        internal static void fixDelayPoison()
        {
            var delay_poison_buff = library.Get<BlueprintBuff>("51ebd62ee464b1446bb01fa1e214942f");
            delay_poison_buff.RemoveComponents<BuffDescriptorImmunity>();
            delay_poison_buff.RemoveComponents<SuppressBuffs>();
            delay_poison_buff.AddComponent(Helpers.Create<BuffMechanics.SuppressBuffsCorrect>(s => s.Descriptor = SpellDescriptor.Poison));

            //also fix poisons to work correctly with delay:
            //make poison buff duration permanent, since they are anyway count number of ticks internally and dispel themselves once max number of ticks is reached;
            //Delay Poison will stop them from ticking
            var poisons = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(f => f.name.Contains("PoisonFeature")).ToArray();

            foreach (var p in poisons)
            {
                var attack_trigger = p.GetComponent<AddInitiatorAttackWithWeaponTrigger>();
                if (attack_trigger == null)
                {
                    continue;
                }
                attack_trigger.Action.Actions = Common.changeAction<ContextActionApplyBuff>(attack_trigger.Action.Actions, a => a.Permanent = true);
            }
        }


        internal static void fixSuppressBuffs()
        {
            var buffs = library.GetAllBlueprints().OfType<BlueprintBuff>();

            foreach (var b in buffs)
            {
                var c = b.GetComponent<SuppressBuffs>();
                if (c == null)
                {
                    continue;
                }
                var new_c = Helpers.Create<BuffMechanics.SuppressBuffsCorrect>(s =>
                {
                    s.Descriptor = c.Descriptor;
                    s.Schools = c.Schools;
                    s.Buffs = c.Buffs;
                }
                );
                b.ReplaceComponent(c, new_c);
            }
        }


        internal static void fixElementalWallsToAvoidDealingDamageTwiceOnTheFirstRound()
        {
            var areas = new BlueprintAbilityAreaEffect[]{library.Get<BlueprintAbilityAreaEffect>("ac8737ccddaf2f948adf796b5e74eee7"),
                                                         library.Get<BlueprintAbilityAreaEffect>("2a9cebe780b6130428f3bf4b18270021"),
                                                         library.Get<BlueprintAbilityAreaEffect>("608d84e25f42d6044ba9b96d9f60722a"),
                                                         library.Get<BlueprintAbilityAreaEffect>("2175d68215aa61644ad1d877d4915ece")
                                                        };

            foreach (var area in areas)
            {
                var old_run_action = area.GetComponent<AbilityAreaEffectRunAction>();
                var new_run_action = Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a => { a.UnitEnter = old_run_action.UnitEnter; a.FirstRound = old_run_action.Round; a.Round = old_run_action.Round; });

                area.ReplaceComponent(old_run_action, new_run_action);
            }

        }

        internal static void fixDazzlingDisplay()
        {
            //require holding weapon with weapon focus
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            var dazzling_display = library.Get<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb");
            dazzling_display.AddComponent(Helpers.Create<NewMechanics.AbilityCasterEquippedWeaponCheckHasParametrizedFeature>(a => { a.feature = weapon_focus; a.allow_kinetic_blast = true; }));
        }


        internal static void fixChannelEnergySaclaing()
        {
            var empyreal_resource = library.Get<BlueprintAbilityResource>("f9af9354fb8a79649a6e512569387dc5");
            empyreal_resource.SetIncreasedByStat(1, StatType.Wisdom);

            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var paladin = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            var sorceror = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");

            string[] cleric_channel_ids = new string[] {"f5fc9a1a2a3c1a946a31b320d1dd31b2",
                                                      "279447a6bf2d3544d93a0a39c3b8e91d",
                                                      "9be3aa47a13d5654cbcb8dbd40c325f2",
                                                      "89df18039ef22174b81052e2e419c728"};



            string[] paladin_channel_ids = new string[] { "6670f0f21a1d7f04db2b8b115e8e6abf",
                                                          "0c0cf7fcb356d2448b7d57f2c4db3c0c",
                                                          "4937473d1cfd7774a979b625fb833b47",
                                                          "cc17243b2185f814aa909ac6b6599eaa" };

            string[] empyreal_channel_ids = new string[] { "574cf074e8b65e84d9b69a8c6f1af27b", "e1536ee240c5d4141bf9f9485a665128" };

            foreach (var id in cleric_channel_ids)
            {
                var channel = library.Get<BlueprintAbility>(id);
                channel.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { cleric }, StatType.Charisma));
            }

            foreach (var id in paladin_channel_ids)
            {
                var channel = library.Get<BlueprintAbility>(id);
                channel.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { paladin }, StatType.Charisma));
            }

            foreach (var id in empyreal_channel_ids)
            {
                var channel = library.Get<BlueprintAbility>(id);
                channel.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { sorceror }, StatType.Charisma));
            }
        }

        internal static void fixCaveFangs()
        {
            var cave_fangs_stalagmites_ability = library.Get<BlueprintAbility>("8ec73d388f0875640af8df799f7f16b5");
            var cave_fangs_stalactites_ability = library.Get<BlueprintAbility>("039681ca00c74f24eb302f340f8c6be7");

            var cave_fangs_stalagmites_area = library.Get<BlueprintAbilityAreaEffect>("104bb16f7c3717f44859d0aea97251ce");
            var cave_fangs_stalactites_area = library.Get<BlueprintAbilityAreaEffect>("b8a7c68b040695a40b3a87b9676f7b50");

            var cave_fangs_stalagmites_area2 = library.Get<BlueprintAbilityAreaEffect>("8b4ea698ae053c541beed4e050f32dc3");
            var cave_fangs_stalactites_area2 = library.Get<BlueprintAbilityAreaEffect>("34fc4df95571a2a4f81460cce0c2ea93");

            var dummy_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>(cave_fangs_stalagmites_area.AssetGuid, "CaveFangsNoCastArea", "");
            dummy_area.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            dummy_area.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAreaEffectRunAction(new GameAction[0]) };

            var dummy_spawn_action = Helpers.Create<ContextActionSpawnAreaEffect>(c =>
                                                                                   {
                                                                                       c.AreaEffect = dummy_area;
                                                                                       c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Rounds);
                                                                                   }
                                                                                );
            cave_fangs_stalagmites_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(dummy_spawn_action)));
            cave_fangs_stalactites_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(dummy_spawn_action)));

            cave_fangs_stalagmites_ability.AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(a => a.area_effect = cave_fangs_stalagmites_area));
            cave_fangs_stalagmites_ability.AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(a => a.area_effect = dummy_area));

            cave_fangs_stalactites_ability.AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(a => a.area_effect = cave_fangs_stalactites_area));
            cave_fangs_stalactites_ability.AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(a => a.area_effect = dummy_area));
        }


        internal static void fixAnimalCompanionFeats()
        {
            //remove weapon focus from ac
            var weapon_focus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
            var animal_companion_selection = library.Get<BlueprintFeatureSelection>("571f8434d98560c43935e132df65fe76");

            foreach (var f in animal_companion_selection.AllFeatures)
            {
                var u = f.GetComponent<AddPet>()?.Pet;
                if (u == null)
                {
                    continue;
                }
                var feats = u.GetComponent<AddClassLevels>().Selections[0].Features;
                u.GetComponent<AddClassLevels>().Selections[0].Features = feats.RemoveFromArray(weapon_focus);
            }

        }


        internal static void fixFlailCritMultiplier()
        {
            BlueprintWeaponType[] flails = new BlueprintWeaponType[] {library.Get<BlueprintWeaponType>("bf1e53f7442ed0c43bf52d3abe55e16a"),
                                                                      library.Get<BlueprintWeaponType>("8fefb7e0da38b06408f185e29372c703")
                                                                     };
            foreach (var f in flails)
            {
                Helpers.SetField(f, "m_CriticalModifier", 3);
            }
        }




        public static void fixTristianAngelBuff()
        {
            // replace fire domain spells with sun domain
            //fix trisitian buff
            var trisitan_fire_maximize = library.Get<BlueprintBuff>("f16954c5c8cb0834baace64a167aa3cb");
            trisitan_fire_maximize.SetDescription("As a move action, Tristian can adopt his angelic form. All cure spells and spells from Sun domain he casts in this form are maximized, as if using Maximize Spell feat, and he gains immunity to fire. Also, in this form he has wings, which grant him immunity to ground based effects and +2 dodge bonus to AC against melee attacks.He can use this ability for up to 20 rounds per day. These rounds do not need to be consecutive.");

            var sun_domain_spells = library.Get<BlueprintSpellList>("600ffed45d0c3ec43a75dc76bb9377b6");
            var metamagic = trisitan_fire_maximize.GetComponent<AutoMetamagic>();

            var spells = Common.getSpellsFromSpellList(sun_domain_spells).AddToArray(metamagic.Abilities.Take(8)).ToList();

            foreach (var s in spells.ToArray())
            {
                spells.AddRange(SpellDuplicates.getDuplicates(s));
            }
            spells = spells.Distinct().ToList();
            metamagic.Abilities = spells;
        }

        internal static void fixDomainSpells()
        {
            //lvl 4 heling domain should be cure critical wounds
            Common.replaceSpellFromListWithDuplicate(library.Get<BlueprintSpellList>("eba577470b8ee8443bb4552433451990"), 5, "WeatherDomain"); //ice storm
            Common.replaceSpellFromListWithDuplicate(library.Get<BlueprintSpellList>("eba577470b8ee8443bb4552433451990"), 7, "WeatherDomain"); //fire storm

            Common.replaceSpellFromListWithDuplicate(library.Get<BlueprintSpellList>("35e15cd1b353e2d47b507c445d2f8c6f"), 5, "WaterDomain"); //ice storm
            Common.replaceSpellFromListWithDuplicate(library.Get<BlueprintSpellList>("df3bc5bda7deb9d46b0f177db3bb7876"), 6, "EarthDomain"); //stoneskin

            Common.replaceSpellFromListWithDuplicate(library.Get<BlueprintSpellList>("81bff1165d9468a44b2f815f7c26a373"), 6, "EvilDomain"); //create undead
            //lvl 4 death domain spell should be death ward
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("710d8c959e7036448b473ffa613cdeba"), library.Get<BlueprintAbility>("0413915f355a38146bc6ad40cdf27b3f"), 4);
            //lvl 4 helaing domain spell should be cure critical wounds
           // Common.replaceDomainSpell(library.Get<BlueprintProgression>("b0a26ee984b6b6945b884467aa2f1baa"), library.Get<BlueprintAbility>("41c9016596fe1de4faf67425ed691203"), 4);
        }

        internal static void fixStalwartDefender()
        {
            var stalwart_defender = library.Get<BlueprintCharacterClass>("d5917881586ff1d4d96d5b7cebda9464");
            var progression = library.Get<BlueprintProgression>("e93eabf4f9b48914c9d880dd41c06385");

            //ad armor proficiency prerequisites
            var light_armor_proficiency = library.Get<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132");
            var medium_armor_proficiency = library.Get<BlueprintFeature>("46f4fb320f35704488ba3d513397789d");
            stalwart_defender.AddComponent(Helpers.PrerequisiteFeature(light_armor_proficiency));
            stalwart_defender.AddComponent(Helpers.PrerequisiteFeature(medium_armor_proficiency));

            var proficiency = library.CopyAndAdd<BlueprintFeature>("a23591cc77086494ba20880f87e73970", "StalwartDefenderProficiency", ""); //from fighter
            proficiency.SetNameDescription("Stalwart Defender Proficiencies",
                                           "A stalwart defender is proficient with all simple and martial weapons, all types of armor, and shields (including tower shields)."
                                           );
            progression.LevelEntries[0].Features.Add(proficiency);
            //give it retroactively
            Action<UnitDescriptor> save_game_fix = delegate (UnitDescriptor unit)
            {
                if (unit.Progression.GetClassLevel(stalwart_defender) >= 1 && !unit.Progression.Features.HasFact(proficiency))
                {
                    unit.Progression.Features.AddFeature(proficiency);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_fix);


            var internal_fortitude = library.Get<BlueprintFeature>("727fcc6ef87e568479ab9dc3a8a5dc6c");
            internal_fortitude.ComponentsArray = new BlueprintComponent[0];
            var internal_fortitude_buff = library.CopyAndAdd<BlueprintBuff>("21ff8159995fe194b81d89b1c83f33a3", "StalwartDefenderInternalFortitudeBuff", ""); //from rage

            var defensive_stance_buff = library.Get<BlueprintBuff>("3dccdf27a8209af478ac71cded18a271");
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(defensive_stance_buff, internal_fortitude_buff, internal_fortitude);

            //remove second improved uncanny dodge

            var new_level_entries = new List<LevelEntry>();

            foreach (var level_entry in progression.LevelEntries)
            {
                if (level_entry.Level != 9)
                {
                    new_level_entries.Add(level_entry);
                }
            }
            progression.LevelEntries = new_level_entries.ToArray();
        }


        //fix range
        [Harmony12.HarmonyPatch(typeof(BlueprintAbility))]
        [Harmony12.HarmonyPatch("GetRange", Harmony12.MethodType.Normal)]
        class BlueprintAbility_GetRange
        {
            static void Postfix(BlueprintAbility __instance, bool reach, ref Feet __result)
            {
                Main.TraceLog();
                AbilityRange range = __instance.Range;
                if (!(range == AbilityRange.Touch || range == AbilityRange.Close || range == AbilityRange.Medium || range == AbilityRange.Long))
                {
                    return;
                }

                if (reach && range != AbilityRange.Long)
                {
                    ++range;
                }

                if (range == AbilityRange.Medium)
                {
                    __result = Common.medium_range_ft.Feet();
                }
                else if (range == AbilityRange.Long)
                {
                    __result = Common.long_range_ft.Feet();
                }
            }
        }


        internal static void fixAlchemistFastBombs()
        {
            var fast_bombs = library.Get<BlueprintFeature>("128c5fccec5ca724281a4907b1f0ac83");
            var fast_bombs_ability = fast_bombs.GetComponent<AddFacts>().Facts[0] as BlueprintActivatableAbility;
            var fast_bombs_buff = fast_bombs_ability.Buff;

            var bombs = fast_bombs_buff.GetComponent<FastBombs>().Abilities;

            var new_buff = Helpers.CreateBuff("FastBombs2Buff",
                                              fast_bombs_buff.Name,
                                              fast_bombs.Description,
                                              "",
                                              fast_bombs.Icon,
                                              null,
                                              Helpers.Create<TurnActionMechanics.IterativeAttacksWithAbilities>(i => i.abilities = bombs)
                                              );

            var new_ability = Helpers.CreateAbility("FastBombs2Ability",
                                                    new_buff.Name,
                                                    new_buff.Description,
                                                    "",
                                                    new_buff.Icon,
                                                    AbilityType.Special,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    Helpers.oneRoundDuration,
                                                    "",
                                                    Helpers.CreateRunActions(Common.createContextActionApplyBuff(new_buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                    Common.createAbilityCasterHasNoFacts(fast_bombs_buff)
                                                    );
            fast_bombs_ability.AddComponent(Helpers.Create<RestrictionHasFact>(r => { r.Feature = new_buff; r.Not = true; }));
            new_ability.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(new_ability);

            //imitate full attack action for bombs
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            //fast_bombs_buff.AddComponent(Helpers.CreateAddFactContextActions(activated: Common.apply_concnetration));
            //fast_bombs_ability.AddComponent(Helpers.Create<RestrictionHasFact>(r => { r.Feature = Common.concentration_buff; r.Not = true; }));
            Helpers.SetField(fast_bombs_ability, "m_ActivateWithUnitCommand", UnitCommand.CommandType.Move);
            //fast_bombs_ability.ActivationType = AbilityActivationType.WithUnitCommand;

            //fast_bombs_buff.AddComponent(Helpers.Create<FreeActionAbilityUseMechanics.ForceFullRoundOnAbilities>(f => f.abilities = bombs));
            fast_bombs.AddComponent(Helpers.CreateAddFact(new_ability));
            //Helpers.SetField(fast_bombs_ability, "m_ActivateWithUnitCommand", UnitCommand.CommandType.Move);
            //fast_bombs_ability.DeactivateAfterFirstRound = true;
            // fast_bombs_ability.ActivationType = AbilityActivationType.WithUnitCommand;
            // fast_bombs_ability.DeactivateImmediately = false;

        }

        static internal void fixArchonsAuraToEffectOnlyEnemiesAndDescription()
        {
            var area = ResourcesLibrary.TryGetBlueprint<BlueprintAbilityAreaEffect>("a70dc66c3059b7a4cb5b2a2e8ac37762");

            var run_actions = area.GetComponent<AbilityAreaEffectRunAction>();

            Common.changeAction<Conditional>(run_actions.UnitEnter.Actions, a =>
                                                                            {
                                                                                if (a.ConditionsChecker.Conditions.Length == 2 && (a.ConditionsChecker.Conditions[1] is ContextConditionIsAlly))
                                                                                {
                                                                                    a.ConditionsChecker.Conditions[1] = Helpers.Create<ContextConditionIsEnemy>();
                                                                                }
                                                                            }
                                            );
            var archons_aura = library.Get<BlueprintAbility>("e67efd8c84f69d24ab472c9f546fff7e").LocalizedSavingThrow = Helpers.willNegates;
        }


        static public void removeSoloTacticsFromSH()
        {
            var solo_tactics = library.Get<BlueprintFeature>("5602845cd22683840a6f28ec46331051");
            var sacred_huntsmaster = library.Get<BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012");
            if (sacred_huntsmaster.RemoveFeatures.Any(r => r.Level == 3))
            {
                return;
            }
            sacred_huntsmaster.RemoveFeatures = sacred_huntsmaster.RemoveFeatures.AddToArray(Helpers.LevelEntry(3, solo_tactics));
        }


        static internal void fixDruidDomainUi()
        {
            var druid_domains = library.Get<BlueprintFeatureSelection>("096fc02f6cc817a43991c4b437e12b8e").AllFeatures;

            foreach (var druid_domain in druid_domains)
            {
                var cleric_name = druid_domain.name.Replace("Druid", "Secondary");
                var cleric_domain = library.GetAllBlueprints().Where(a => a.name == cleric_name).First() as BlueprintProgression;

                (druid_domain as BlueprintProgression).UIGroups = cleric_domain.UIGroups;
            }
        }


        static internal void fixBleed()
        {
            var bleed1d4 = library.Get<BlueprintBuff>("5eb68bfe186d71a438d4f85579ce40c1");
            var bleed1d6 = library.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562");
            var bleed2d6 = library.Get<BlueprintBuff>("16249b8075ab8684ca105a78a047a5ef");
            var bleed1d6e = library.Get<BlueprintBuff>("dc9ed761b7721c64e98fab507e2a7755");

            bleed1d4.RemoveComponents<AddFactContextActions>();
            bleed1d4.AddComponent(Helpers.Create<BleedMechanics.BleedBuff>(b => b.dice_value = Helpers.CreateContextDiceValue(DiceType.D4, 1, 0)));
            bleed1d6.RemoveComponents<AddFactContextActions>();
            bleed1d6.AddComponent(Helpers.Create<BleedMechanics.BleedBuff>(b => b.dice_value = Helpers.CreateContextDiceValue(DiceType.D6, 1, 0)));
            bleed1d6e.RemoveComponents<AddFactContextActions>();
            bleed1d6e.AddComponent(Helpers.Create<BleedMechanics.BleedBuff>(b => b.dice_value = Helpers.CreateContextDiceValue(DiceType.D6, 1, 0)));
            bleed2d6.RemoveComponents<AddFactContextActions>();
            bleed2d6.AddComponent(Helpers.Create<BleedMechanics.BleedBuff>(b => b.dice_value = Helpers.CreateContextDiceValue(DiceType.D6, 2, 0)));
        }


        static internal void fixRangerAnimalCompanion()
        {
            var selection = library.Get<BlueprintFeatureSelection>("ee63330662126374e8785cc901941ac7");
            var share_fe_bonus = library.Get<BlueprintFeature>("cd7b831693c0f3947b019321c0510915");
            share_fe_bonus.AddComponent(Helpers.Create<CompanionMechanics.FavoredTerrainBonusFromMaster>());
            selection.AddComponent(Common.createAddFeatToAnimalCompanion(share_fe_bonus));
            selection.AllFeatures = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("472091361cf118049a2b4339c4ea836a"), //empty
                library.Get<BlueprintFeature>("f894e003d31461f48a02f5caec4e3359"), //dog
                library.Get<BlueprintFeature>("e992949eba096644784592dc7f51a5c7"), //ekun wolf
                library.Get<BlueprintFeature>("aa92fea676be33d4dafd176d699d7996"), //elk
                library.Get<BlueprintFeature>("2ee2ba60850dd064e8b98bf5c2c946ba"), //leopard
                library.Get<BlueprintFeature>("ece6bde3dfc76ba4791376428e70621a"), //monitor
                library.Get<BlueprintFeature>("67a9dc42b15d0954ca4689b13e8dedea"), //wolf
            };
            selection.SetDescription(selection.Description + "\nA ranger’s animal companion shares his favored enemy and favored terrain bonuses.");
        }


        static internal void fixRangerMasterHunter()
        {
            var master_hunter_cooldown_buff = library.Get<BlueprintBuff>("077f4430a10d3504b9078ab717334972");
            master_hunter_cooldown_buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var master_hunter = library.Get<BlueprintAbility>("8a57e1072da4f6f4faaa55b7b7dc633c");


            master_hunter.Range = AbilityRange.Weapon;
            master_hunter.setMiscAbilityParametersSingleTargetRangedHarmful();

            var effect = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude,
                                                         Helpers.CreateConditionalSaved(null,
                                                                                        Helpers.Create<ContextActionKillTarget>()
                                                                                        )
                                                        );

            master_hunter.ComponentsArray = new BlueprintComponent[]
            {
                master_hunter.GetComponent<AbilityResourceLogic>(),
                Helpers.CreateRunActions(Common.createContextActionApplyBuff(master_hunter_cooldown_buff, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false),
                                         Common.createContextActionAttack(Helpers.CreateActionList(effect))
                                        ),
                Helpers.Create<NewMechanics.AttackAnimation>(),
                Common.createAbilityTargetHasFact(true, master_hunter_cooldown_buff),
                Helpers.Create<FavoredEnemyMechanics.AbilityTargetIsFavoredEnemy>()
            };

            master_hunter.NeedEquipWeapons = true;
        }


        static internal void fixEaglesoul()
        {
            //fix it to be swift action rather than standard since it is how it is supposed to be due to its pnp version
            var eaglesoul = library.Get<BlueprintAbility>("332ad68273db9704ab0e92518f2efd1c");
            eaglesoul.ActionType = UnitCommand.CommandType.Swift;
        }


        static internal void fixGrease()
        {
            var grease_spell = library.Get<BlueprintAbility>("95851f6e85fe87d4190675db0419d112");
            var grease_area = library.Get<BlueprintAbilityAreaEffect>("d46313be45054b248a1f1656ddb38614");
            var grease_buff = library.Get<BlueprintBuff>("5f9910ccdd124294e905b391d01b4ade");
            grease_buff.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateSpellDescriptor(SpellDescriptor.Ground | SpellDescriptor.MovementImpairing),
                Common.createAddCondition(UnitCondition.DifficultTerrain)
            };

            var fall = Helpers.CreateActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateConditionalSaved(null, Helpers.Create<ContextActionKnockdownTarget>()));
            grease_spell.GetComponent<AbilityEffectRunAction>().Actions = Helpers.CreateActionList(fall);
            var spawn_area = Common.createContextActionSpawnAreaEffect(grease_area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
            grease_spell.AddComponent(Helpers.Create<AbilityEffectRunActionOnClickedTarget>(a => a.Action = Helpers.CreateActionList(spawn_area)));

            grease_spell.SetDescription("A grease spell covers a solid surface with a layer of slippery grease. Any creature in the area when the spell is cast must make a successful Reflex save or fall. A creature can walk within or through the area of grease at half normal speed with a DC 10 Mobility check, otherwise it falls. Creatures that do not move on their turn do not need to make this check and are not considered flat-footed.");
            var apply_prone = Helpers.Create<ContextActionKnockdownTarget>();
            var area_effect = Helpers.Create<NewMechanics.AbilityAreaEffectRunActionWithFirstRound>(a =>
            {
                a.UnitMove = Helpers.CreateActionList(Common.createContextActionSkillCheck(StatType.SkillMobility, failure: Helpers.CreateActionList(apply_prone), custom_dc: 10));
            }
            );
            grease_spell.RemoveComponents<AbilityAoERadius>();
            grease_spell.AddComponent(Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any));
            grease_area.AddComponents(area_effect);
        }


        public static void fixEldritchArcherPenalty()
        {
            Common.ignore_spell_combat_penalty = Helpers.CreateFeature("IgnoreSpellCombatPenaltyFeature",
                                                                       "",
                                                                       "",
                                                                       "",
                                                                       null,
                                                                       FeatureGroup.None
                                                                       );
            Common.ignore_spell_combat_penalty.HideInUI = true;
            Common.ignore_spell_combat_penalty.HideInCharacterSheetAndLevelUp = true;

            var spellcombat_penalty_buff = library.Get<BlueprintBuff>("7b4cf64d3a49e3d45b1dbd2385f4eb6d");
            spellcombat_penalty_buff.RemoveComponents<AttackTypeAttackBonus>();

            var cmp = Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable);
            spellcombat_penalty_buff.AddComponents(cmp,
                                                  Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList,
                                                                                  ContextRankProgression.BonusValue,
                                                                                  stepLevel: -2,
                                                                                  featureList: new BlueprintFeature[] {Common.ignore_spell_combat_penalty, Common.ignore_spell_combat_penalty })
                                                  
                                                  );
            BladeTutor.RuleCalculateAttackBonusWithoutTarget_OnTrigger_Patch.facts.Add(spellcombat_penalty_buff);
        }


        static internal void addMissingImmunities()
        {
            Common.plant_arcana_language_hidden = Helpers.CreateFeature("BypassPlantLanguageDependentImmunity",
                                        "",
                                        "",
                                        "",
                                        null,
                                        FeatureGroup.None);
            Common.plant_arcana_language_hidden.HideInCharacterSheetAndLevelUp = true;
            Common.plant_arcana_language_hidden.HideInUI = true;
            var serpentine_arcana = library.Get<BlueprintFeature>("02707231be1d3a74ba7e38a426c8df37");
            var language_dependent = (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent;
            Common.vermin.AddComponents(Helpers.Create<BuffDescriptorImmunity>(b => { b.Descriptor = language_dependent; b.IgnoreFeature = serpentine_arcana; }),
                                        Helpers.Create<SpellImmunityToSpellDescriptor>(b => { b.Descriptor = language_dependent; b.CasterIgnoreImmunityFact = serpentine_arcana; })
                                        );
            Common.plant.AddComponents(Helpers.Create<BuffDescriptorImmunity>(b => { b.Descriptor = language_dependent; b.IgnoreFeature = Common.plant_arcana_language_hidden; }),
                                        Helpers.Create<SpellImmunityToSpellDescriptor>(b => { b.Descriptor = language_dependent; b.CasterIgnoreImmunityFact = Common.plant_arcana_language_hidden; })
                                        );
            Common.animal.AddComponents(Helpers.Create<BuffDescriptorImmunity>(b => { b.Descriptor = language_dependent; b.IgnoreFeature = serpentine_arcana; }),
                                        Helpers.Create<SpellImmunityToSpellDescriptor>(b => { b.Descriptor = language_dependent; b.CasterIgnoreImmunityFact = serpentine_arcana; })
                                        );

            Common.magical_beast.AddComponents(Helpers.Create<BuffDescriptorImmunity>(b => { b.Descriptor = language_dependent; b.IgnoreFeature = serpentine_arcana; }),
                                                Helpers.Create<SpellImmunityToSpellDescriptor>(b => { b.Descriptor = language_dependent; b.CasterIgnoreImmunityFact = serpentine_arcana; })
                                                );

            /*Common.monstrous_humanoid.AddComponents(Helpers.Create<BuffDescriptorImmunity>(b => { b.Descriptor = language_dependent; b.IgnoreFeature = serpentine_arcana; }),
                                                Helpers.Create<SpellImmunityToSpellDescriptor>(b => { b.Descriptor = language_dependent; b.CasterIgnoreImmunityFact = serpentine_arcana; })
                                                );*/
        }

        static internal void fixUndeadImmunity()
        {
            Common.undead_arcana_hidden = Helpers.CreateFeature("BypassUndeadImmunity",
                                                                "",
                                                                "",
                                                                "",
                                                                null,
                                                                FeatureGroup.None);
            Common.undead_arcana_hidden.HideInCharacterSheetAndLevelUp = true;
            Common.undead_arcana_hidden.HideInUI = true;

            var undead_arcana = library.Get<BlueprintFeature>("1a5e7191279e7cd479b17a6ca438498c");
            undead_arcana.AddComponent(Helpers.CreateAddFact(Common.undead_arcana_hidden));

            //add missing immunity to stun, remove immunity to fear/shaken/charm/daze  and recalcualte fort saves on cha change
            var undead_immunity = library.Get<BlueprintFeature>("8a75eb16bfff86949a4ddcb3dd2f83ae");
            undead_immunity.RemoveComponents<BuffDescriptorImmunity>();
            undead_immunity.RemoveComponents<SpellImmunityToSpellDescriptor>();

            SpellDescriptor always_immune = SpellDescriptor.Poison | SpellDescriptor.Disease | SpellDescriptor.Sickened | SpellDescriptor.Paralysis
                                            | SpellDescriptor.Fatigue | SpellDescriptor.Exhausted | SpellDescriptor.Bleed
                                            | SpellDescriptor.VilderavnBleed | SpellDescriptor.Death | SpellDescriptor.Stun;


            undead_immunity.AddComponent(Helpers.Create<BuffDescriptorImmunity>(b => { b.Descriptor = always_immune; }));
            undead_immunity.AddComponent(Helpers.Create<BuffDescriptorImmunity>(b => { b.Descriptor = SpellDescriptor.MindAffecting; b.IgnoreFeature = Common.undead_arcana_hidden; }));
            undead_immunity.AddComponent(Helpers.Create<SpellImmunityToSpellDescriptor>(b => { b.Descriptor = always_immune; }));
            undead_immunity.AddComponent(Helpers.Create<SpellImmunityToSpellDescriptor>(b => { b.Descriptor = SpellDescriptor.MindAffecting; b.CasterIgnoreImmunityFact = Common.undead_arcana_hidden; }));

            undead_immunity.AddComponent(Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Charisma));


            var abilities = library.GetAllBlueprints().OfType<BlueprintAbility>();

            foreach (var a in abilities)
            {
                a.RemoveComponents<AbilityTargetHasNoFactUnless>(u => u.UnlessFact == undead_arcana);
                var actions = a.GetComponent<AbilityEffectRunAction>();
                if (actions?.Actions == null)
                {
                    continue;
                }
                var extracted_actions = Common.extractActions<Conditional>(actions.Actions.Actions);
                foreach (var ea in extracted_actions)
                {
                    if (ea.ConditionsChecker == null)
                    {
                        var cond_to_remove = ea.ConditionsChecker.Conditions.OfType<ContextConditionCasterHasFact>().Where(ccc => ccc.Fact == undead_arcana).ToArray();
                        foreach (var ctr in cond_to_remove)
                        {
                            ea.ConditionsChecker.Conditions = ea.ConditionsChecker.Conditions.RemoveFromArray(ctr);
                        }
                    }
                }
            }

            var mummification = library.Get<BlueprintFeature>("daf854d84d442e941aa3a2fdc041b37c");
            mummification.GetComponent<BuffDescriptorImmunity>().IgnoreFeature = null;

            //fix baleful polymorrph
            var baleful_polymorph = library.Get<BlueprintAbility>("3105d6e9febdc3f41a08d2b7dda1fe74");
            baleful_polymorph.ReplaceComponent<AbilityTargetHasFact>(a => a.CheckedFacts = a.CheckedFacts.AddToArray(Common.undead));
        }


        static internal void fixWidomCognatogen()
        {
            var wis_cognatogen = library.Get<BlueprintAbility>("84a9092b8430a1344a3c8b002cc68e7f");
            wis_cognatogen.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions.Actions = Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions, b => b.DurationValue.BonusValue = Helpers.CreateContextValue(AbilityRankType.Default)));
        }


        static internal void addMobilityToMonkBonusFeats()
        {
            var mobility = library.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb");

            var monk_feat_ids = new string[]
            {
                "b993f42cb119b4f40ac423ae76394374", //monk6
                "1051170c612d5b844bfaa817d6f4cfff", //monk10
                "92f7b37ef1cf5484db02a924592ceb74", //scaled fist bonus feats 6
                "c569fc66f22825445a7b7f3b5d6d208f", //scaled fist bonus feats 10
            };

            foreach (var id in monk_feat_ids)
            {
                var selection = library.Get<BlueprintFeatureSelection>(id);
                selection.AllFeatures = selection.AllFeatures.AddToArray(mobility);
                selection.Features = selection.Features.AddToArray(mobility);
            }
        }


        internal static void fixSylvanSorcerorAnimalCompanion()
        {
            //make it to be equal to level - 4 (min 1)
            var progression = library.Get<BlueprintProgression>("09c91f959fb737f4289d121e595c657c");
            progression.LevelEntries = progression.LevelEntries.Skip(3).ToArray();

            var sylvan_animal_companion = library.Get<BlueprintFeatureSelection>("a540d7dfe1e2a174a94198aba037274c");
            sylvan_animal_companion.SetDescription("At 1st level, you gain an animal companion. Your effective druid level for this ability is equal to your sorcerer level – 3 (minimum 1st).\n" + sylvan_animal_companion.Description);
        }


        static internal void fixPhysicalDrBypassToApplyToAllPhysicalDamage()
        {
            var features = library.GetAllBlueprints().OfType<BlueprintUnitFact>();

            foreach (var f in features)
            {
                var drs = f.GetComponents<AddOutgoingPhysicalDamageProperty>();
                foreach (var dr in drs)
                {
                    dr.AffectAnyPhysicalDamage = true;
                }
            }
        }

        static internal void fixTandemTripPrerequisite()
        {
            var tandem_trip = library.Get<BlueprintFeature>("d26eb8ab2aabd0e45a4d7eec0340bbce");
            tandem_trip.RemoveComponents<PrerequisiteFeature>();
        }


        static internal void fixAuraOfJustice()
        {
            //to make paladin also receive benefits
            var ability = library.Get<BlueprintAbility>("7a4f0c48829952e47bb1fd1e4e9da83a");
            var smite_buff = library.Get<BlueprintBuff>("b6570b8cbb32eaf4ca8255d0ec3310b0");

            var old_actions = ability.GetComponent<AbilityEffectRunAction>().Actions;
            old_actions.Actions = Common.addMatchingAction<ContextActionApplyBuff>(old_actions.Actions,
                                                                           Common.createContextActionApplyBuff(smite_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)
                                                                           );
        }


        static internal void fixDruidWoodlandStride()
        {
            var woodland_stride = library.Get<BlueprintFeature>("11f4072ea766a5840a46e6660894527d");
            var druid_progression = library.Get<BlueprintProgression>("01006f2ac8866764fb7af135e73be81c");

            druid_progression.LevelEntries[1].Features.Add(woodland_stride);
            druid_progression.UIGroups[0].Features.Add(woodland_stride);

            var ranger_progression = library.Get<BlueprintProgression>("97261d609529d834eba4fd4da1bc44dc");

            ranger_progression.LevelEntries[6].Features.Add(woodland_stride);
            ranger_progression.UIGroups[3].Features.Add(woodland_stride);
        }


        static internal void fixGrappleSpells()
        {
            var buffs = new BlueprintBuff[]
            {
                library.Get<BlueprintBuff>("a719abac0ea0ce346b401060754cc1c0"), //web
                library.Get<BlueprintBuff>("bf6c03b98af9a374c8d61988b5f3ba96"), //phantasmal web
                library.Get<BlueprintBuff>("5f0f235d30430e040829cf4b1bf1655b"), //grappling infuision (?)
            };

            foreach (var b in buffs)
            {
                var new_round_actions = b.GetComponent<AddFactContextActions>().NewRound;

                var new_actions = Common.replaceActions<ContextActionBreakFree>(new_round_actions.Actions,
                                                                                    a => Helpers.Create<CombatManeuverMechanics.ContextActionBreakFreeFromSpellGrapple>(c =>
                                                                                                                                                                        {
                                                                                                                                                                            c.Failure = a.Failure;
                                                                                                                                                                            c.Success = a.Success;
                                                                                                                                                                        }
                                                                                                                                                                        )
                                                                                 );
                new_round_actions.Actions = new_actions;
            }
        }


        internal static void nerfSmilodonRake()
        {
            var units = library.GetAllBlueprints().OfType<BlueprintUnit>().Where(c => c.name.Contains("Smilodon") || c.name.Contains("Dweomercat"));
            var buffs = library.GetAllBlueprints().OfType<BlueprintBuff>().Where(c => c.name.Contains("Smilodon"));

            foreach (var u in units)
            {
                int num_limbs = u.Body.AdditionalLimbs.Length;
                if (num_limbs < 3)
                {
                    continue;
                }
                u.Body.AdditionalSecondaryLimbs = u.Body.AdditionalLimbs.Skip(num_limbs - 2).ToArray();
                u.Body.AdditionalLimbs = u.Body.AdditionalLimbs.Take(num_limbs - 2).ToArray();
            }
            foreach (var b in buffs)
            {
                var polymorph = b.GetComponent<Polymorph>();
                if (polymorph == null)
                {
                    continue;
                }
                int num_limbs = polymorph.AdditionalLimbs.Length;
                if (num_limbs < 3)
                {
                    continue;
                }

                polymorph.SecondaryAdditionalLimbs = polymorph.AdditionalLimbs.Skip(num_limbs - 2).ToArray();
                polymorph.AdditionalLimbs = polymorph.AdditionalLimbs.Take(num_limbs - 2).ToArray();
            }
        }


        internal static void fixFeatsRequirements()
        {
            var manyshot = library.Get<BlueprintFeature>("adf54af2a681792489826f7fd1b62889");
            manyshot.AddComponent(Helpers.PrerequisiteStatValue(StatType.Dexterity, 17));
            manyshot.AddComponent(Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab"))); //point blank shot
        }

        internal static void fixFlameDancer()
        {
            //add song_of_fiery_faze gaze
            var buff = Helpers.CreateBuff("SongOfFeiryGazeEffectBuff",
                                          "Song of Fiery Gaze",
                                          "At 3rd level, a fire dancer can allow allies to see through flames without any distortion. Any ally within 30 feet of the bard who can hear the performance can see through fire, fog, and smoke without penalty as long as the light is sufficient to allow him to see normally, as with the base effect of the gaze of flames oracle revelation. Song of the fiery gaze relies on audible components.",
                                          "",
                                          Helpers.GetIcon("ee0b69e90bac14446a4cf9a050f87f2e"), //detect magic
                                          null,
                                          Helpers.Create<ConcealementMechanics.IgnoreFogConcelement>()
                                          );

            var toggle = Common.convertPerformance(library.Get<BlueprintActivatableAbility>("430ab3bb57f2cfc46b7b3a68afd4f74e"), buff, "SongOfFeiryGaze");
            var feature = Common.ActivatableAbilityToFeature(toggle, false);

            var flamedancer = library.Get<BlueprintArchetype>("e7914f2adcdb8fc46af5b65d1e06c539");
            flamedancer.AddFeatures[0].Features[0] = feature;

            var blueprint_facts = new BlueprintUnitFact[]
            {
                library.Get<BlueprintFeature>("3c10a0069e7f110499d2e810f4861a6e"),
                library.Get<BlueprintBuff>("bf9493f27bb23d74bb598fb1a7a9fe3a"),
                library.Get<BlueprintBuff>("6b6258335b08dd74fb12e89eddceed7a"),
                library.Get<BlueprintActivatableAbility>("1b28d456a5b1b4744a1d87cf24309ad1"),
            };


            foreach (var fact in blueprint_facts)
            {
                fact.SetDescription("At 6th level, a fire dancer’s performance can bend flames away from others. Any ally within 30 feet of the bard who can hear or see the bardic performance gains resist fire 20 as long as the performance is maintained. At 11th level, this resistance increases to 30.");
            }

            var progression = flamedancer.GetParentClass().Progression;
            progression.UIGroups = progression.UIGroups.AddToArray(Helpers.CreateUIGroup(library.Get<BlueprintFeature>("3c10a0069e7f110499d2e810f4861a6e"), feature));
        }


        static public void fixBlindingRay()
        {
            //dazzled should be applied for 1 round and not 1 round/level
            var blinding_ray = library.Get<BlueprintAbility>("9b4d07751dd104243a94b495c571c9dd");
            var dazzled = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");

            blinding_ray.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions.Actions = Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions, c =>
                                                                                                                                        {
                                                                                                                                            if (c.Buff == dazzled)
                                                                                                                                            {
                                                                                                                                                c.DurationValue = Helpers.CreateContextDuration(1);
                                                                                                                                            }
                                                                                                                                        })
                                                                  );
            blinding_ray.SetDescription("As a standard action, you can fire a shimmering ray at any foe within 30 feet as a ranged touch attack.The ray causes creatures to be blinded for 1 round.Creatures with more Hit Dice than your wizard level are dazzled for 1 round instead.You can use this ability a number of times per day equal to 3 + your Intelligence modifier.");
        }


        static internal void fixTactician()
        {
            var tactical_leader_tactician = library.Get<BlueprintFeature>("93e78cad499b1b54c859a970cbe4f585");
            var tactical_leader = library.Get<BlueprintArchetype>("639b74fd2f48d474e965c596b1649095");
            var teamwork_feats = library.Get<BlueprintBuff>("a603a90d24a636c41910b3868f434447").GetComponent<AddFactsFromCaster>().Facts.Cast<BlueprintFeature>().ToArray();
            var buff = library.Get<BlueprintBuff>("a603a90d24a636c41910b3868f434447");

            buff.SetNameDescription("", "");
            buff.ComponentsArray = new BlueprintComponent[] { Helpers.Create<TeamworkMechanics.AddFactsFromCasterIfHasBuffs>() };
            buff.Stacking = StackingType.Stack;
            foreach (var tw in teamwork_feats)
            {
                var choice_buff = Helpers.CreateBuff(tw.name + "ShareBuff",
                                          "Tactician: " + tw.Name,
                                          "You can grant this teamwork feat to all allies within 30 feet who can see and hear you, using your tactician ability.",
                                          Helpers.MergeIds("e47acc8f864543ca8055ace52233842a", tw.AssetGuid),
                                          tw.Icon,
                                          null
                                          );
                var toggle = Helpers.CreateActivatableAbility(tw.name + "ShareToggleAbility",
                                                              choice_buff.Name,
                                                              choice_buff.Description,
                                                              Helpers.MergeIds("ed966664711f48688cacf90e9bc798b8", tw.AssetGuid),
                                                              tw.Icon,
                                                              choice_buff,
                                                              AbilityActivationType.Immediately,
                                                              UnitCommand.CommandType.Free,
                                                              null
                                                              );
                toggle.DeactivateImmediately = true;
                toggle.Group = ActivatableAbilityGroupExtension.TacticianTeamworkFeatShare.ToActivatableAbilityGroup();
                toggle.WeightInGroup = 1;

                var feature = Common.ActivatableAbilityToFeature(toggle, true, Helpers.MergeIds("c9ca89f32d3b4e1b8add1bae23c73f4b", tw.AssetGuid));
                tw.AddComponent(Common.createAddFeatureIfHasFact(tactical_leader_tactician, feature));
                buff.GetComponent<TeamworkMechanics.AddFactsFromCasterIfHasBuffs>().facts.Add(tw);
                buff.GetComponent<TeamworkMechanics.AddFactsFromCasterIfHasBuffs>().prerequsites.Add(choice_buff);
            }
            var ability = library.Get<BlueprintAbility>("f1c8ec6179505714083ed9bd47599268");
            ability.SetNameDescription("Tactician",
                                       "At 3rd level, 9th level, and 18th level, a tactical leader gains a teamwork feat as a bonus feat. He must meet the prerequisites for this feat. As a standard action, the tactical leader can grant one of these feats to all allies within 30 feet who can see and hear him. Allies retain the use of this bonus feat for 3 rounds plus 1 round for every 2 inquisitor levels the tactical leader has. Allies do not need to meet the prerequisites of these bonus feats.\n"
                                       + "The tactical leader can use this ability once per day at 3rd level, plus one additional time per day at 6th, 9th, 15th, and 18th level.\n"
                                       + "At 12th level, a tactical leader can use the tactician ability as a swift action. At 18th level, whenever the tactical leader uses this ability, he grants any two teamwork feats that he knows. He can select from any of his teamwork feats, not just his bonus feats.");

            ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.createContextActionRemoveBuffFromCaster(buff), a.Actions.Actions[0]));


            var extra_tactician = Helpers.CreateFeature("TacticalLeaderExtraTactician",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroupExtension.TacticianTeamworkFeatShare.ToActivatableAbilityGroup())
                                                        );
            extra_tactician.HideInCharacterSheetAndLevelUp = true;
            extra_tactician.HideInUI = true;
            tactical_leader_tactician.AddComponent(Helpers.CreateAddFeatureOnClassLevel(extra_tactician, 18, new BlueprintCharacterClass[] { tactical_leader.GetParentClass() }, new BlueprintArchetype[] { tactical_leader }));
            tactical_leader_tactician.SetNameDescription(ability);
        }
    }


    //consider summons as allies
    [Harmony12.HarmonyPatch(typeof(UnitEntityData))]
    [Harmony12.HarmonyPatch("IsAlly", Harmony12.MethodType.Normal)]
    class UnitEntityData_IsAlly_Patch
    {
        static void Postfix(UnitEntityData __instance, UnitEntityData unit, ref bool __result)
        {
            Main.TraceLog();
            if (__result == true)
            {
                return;
            }

            var summoner = unit.Get<UnitPartSummonedMonster>()?.Summoner;
            if (summoner == unit)
            {
                return;
            }
            if (summoner != null)
            {
                __result = !__instance.IsEnemy(unit) && __instance.IsAlly(summoner);
            }
        }
    }






    //allow feat and inherent modifiers to be considered as permanent
    [Harmony12.HarmonyPatch(typeof(ModifiableValue.Modifier))]
    [Harmony12.HarmonyPatch("IsPermanent", Harmony12.MethodType.Normal)]
    class ModifiableValue_IsPermanent
    {
        static void Postfix(ModifiableValue.Modifier __instance,  ref bool __result)
        {
            Main.TraceLog();
            __result = __result || __instance.ModDescriptor == ModifierDescriptor.Inherent || __instance.ModDescriptor == ModifierDescriptor.Feat;
        }
    }


    //replace missing attack animation with "something"
    [Harmony12.HarmonyPatch(typeof(UnitAnimationManager), "GetAction", typeof(UnitAnimationSpecialAttackType))]
    class UnitAnimationManager_GetAction
    {
        static UnitAnimationSpecialAttackType[] attacks_to_try = new UnitAnimationSpecialAttackType[] { UnitAnimationSpecialAttackType.Slam,
                                                                                                        UnitAnimationSpecialAttackType.Bite,
                                                                                                        UnitAnimationSpecialAttackType.Gore}; 
        static void Postfix(UnitAnimationManager __instance, UnitAnimationSpecialAttackType type,  ref UnitAnimationAction __result)
        {
            if (__result != null)
            {
                return;
            }
            foreach (var att in attacks_to_try)
            {
                __result = (UnitAnimationAction)__instance.ActionSet.OfType<UnitAnimationActionSpecialAttack>().FirstOrDefault<UnitAnimationActionSpecialAttack>((Func<UnitAnimationActionSpecialAttack, bool>)(a => a.AttackType == att));
                if (__result != null)
                {
                    return;
                }
            }
        }
    }


    //fix holy/unholy/axiomatic and anarchic encahntments not to add damage on non-weapon attacks
    [Harmony12.HarmonyPatch(typeof(WeaponDamageAgainstAlignment), "OnEventAboutToTrigger", typeof(RulePrepareDamage))]
    class WeaponDamageAgainstAlignment_OnEventAboutToTrigger
    {
        static bool Prefix(WeaponDamageAgainstAlignment __instance, RulePrepareDamage evt)
        {
            return evt.DamageBundle?.WeaponDamage != null;
        }
    }



    //fixraise bab effect to take into account various bonuses
    [Harmony12.HarmonyPatch(typeof(RaiseBAB), "OnTurnOn")]
    class RaiseBABt_OnTurnOn
    {
        static bool Prefix(RaiseBAB __instance)
        {
            int num = __instance.TargetValue.Calculate(__instance.Fact.MaybeContext) - __instance.Owner.Stats.BaseAttackBonus.ModifiedValue;
            if (num <= 0)
                return false;
            Traverse.Create(__instance).Field("m_Modifier").SetValue(__instance.Owner.Stats.BaseAttackBonus.AddModifier(num, (GameLogicComponent)__instance, ModifierDescriptor.None));
            return false;
        }
    }
}
