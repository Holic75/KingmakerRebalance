﻿using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Class.Kineticist.Actions;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class KineticistFix
    {
        static LibraryScriptableObject library => Main.library;
        static Dictionary<string, (BlueprintActivatableAbility, BlueprintAbility)> blast_kinetic_blades_burn_map = new Dictionary<string, (BlueprintActivatableAbility, BlueprintAbility)>();
        static List<BlueprintActivatableAbility> substance_infusions = new List<BlueprintActivatableAbility>();

        public static BlueprintFeature blade_rush;
        public static BlueprintFeature blade_rush_swift;
        public static BlueprintFeature kinetic_whip;
        public static BlueprintBuff kinetic_whip_buff;

        //public static BlueprintFeature suffocate;
        public static BlueprintFeature wings_of_air;
        public static BlueprintFeature spark_of_life;

        public static BlueprintFeature whip_hurricane;

        public static BlueprintFeature internal_buffer;
        public static BlueprintAbilityResource internal_buffer_resource;

        static BlueprintCharacterClass kineticist_class = library.Get<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");
        static BlueprintFeature kinetic_blade_infusion = library.Get<BlueprintFeature>("9ff81732daddb174aa8138ad1297c787");
        static BlueprintFeature whirlwind_infusion = library.Get<BlueprintFeature>("80fdf049d396c33408a805d9e21a42e1");

        static BlueprintFeatureSelection infusion_selection = library.Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
        static BlueprintArchetype kinetic_knight = library.Get<BlueprintArchetype>("7d61d9b2250260a45b18c5634524a8fb");
        static BlueprintProgression kineticist_progression = library.Get<BlueprintProgression>("b79e92dd495edd64e90fb483c504b8df");


        static BlueprintAbility blade_whirlwind_ability = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");
        static BlueprintAbility kinetic_whip_ability;
        static BlueprintAbility whip_hurricane_ability;
        static BlueprintAbility blade_rush_ability;
        static BlueprintAbility blade_rush_swift_ability;
        static public BlueprintBuff blade_rush_buff;

        static public BlueprintFeature pushing_infusion;

        internal static void load(bool update_archetypes)
        {
            var kinetic_blade_infusion = library.Get<BlueprintFeature>("9ff81732daddb174aa8138ad1297c787");
            foreach (var c in kinetic_blade_infusion.GetComponents<AddFeatureIfHasFact>())
            {
                var add_facts = c.Feature.GetComponents<AddFeatureIfHasFact>().ToArray();
                blast_kinetic_blades_burn_map.Add(c.CheckedFact.AssetGuid, (add_facts[0].Feature as BlueprintActivatableAbility, add_facts[1].Feature as BlueprintAbility));
            }

            foreach (var infusion in infusion_selection.AllFeatures)
            {
                var comp = infusion.GetComponent<AddFacts>();
                if (comp != null)
                {
                    var ability = comp.Facts[0] as BlueprintActivatableAbility;
                    if (ability == null)
                    {
                        continue;
                    }
                    if (ability.Group == ActivatableAbilityGroup.SubstanceInfusion)
                    {
                        substance_infusions.Add(ability);
                    }
                }
            }

            fixDescriptors();
            //addWhirlwindInfusionToKineticistSelection();
            restoreKineticKnightinfusions();
            whirlwind_infusion.HideInUI = false;
            fixKineticBladeCost();
            fixKineticBladeCostForKineticKnight();
            fixBladeWhirlwindCost();
            fixKineticHealer();
            fixBladeWhirlwindRange();

            fixShroudOfWaterForKineticKnight();

            createBladeRush();
            createKineticWhip();
            createWhipHurricane();
            createSparkOfLife();
            createInternalBuffer();
            fixKineticistAbilitiesToBeSpelllike();
            Witch.infusion.AllFeatures = infusion_selection.AllFeatures;

            if (update_archetypes)
            {
                Main.logger.Log("Updating base kineticist archetypes");
                updateKineticistArchetypes();
            }
        }


        static void updateKineticistArchetypes()
        {
            //make dark elementalist recover 2 points of burn per soul power use
            var soul_power = library.Get<BlueprintAbility>("31a1e5b27cdb78f4094630610519981c");
            soul_power.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionHealBurn>(a.Actions.Actions, c => c.Value = 3)));
            soul_power.SetDescription("A dark elementalist uses the souls of others to protect herself from the dangers of burn. She can't choose to accept burn if doing so would raise her total number of points of burn above 3. However, a number of times per day equal to her Intelligence modifier, as a full-round action she can gather up the souls of dead creatures with HD sum equal to or higher than her character level. When she does, all of her existing burn is unloaded into the departing souls, racking it with unspeakable torment, but reducing her current burn total to zero.\nA dark elementalist gains attack and damage bonuses from elemental overflow based on how many times that day she has successfully used soul power to rack souls, rather than based on her current burn total. For instance, a 9th-level dark elementalist who had used soul power three or more times during the course of the day would add a +3 bonus on attack rolls and a +6 bonus on damage rolls. A dark elementalist does not gain size bonuses to physical ability scores or a chance to ignore critical hits and sneak attacks from elemental overflow.\nThis ability alters burn and elemental overflow and replaces internal buffer.");

            var soul_power_feature = library.Get<BlueprintFeature>("42c5a9a8661db2f47aedf87fb8b27aaf");
            soul_power_feature.SetDescription(soul_power.Description);

            //make psychockineticist burn give only -1 to skills/saves per burn value
            var psychokineticist_burn = library.Get<BlueprintBuff>("a9e3e785ea41449499b6b5d3d22a0856");
            var context_rank_config = psychokineticist_burn.GetComponent<ContextRankConfig>();
            Helpers.SetField(context_rank_config, "m_StepLevel", 1);
            var psychikineticist_burn_feature = library.Get<BlueprintFeature>("f53404854de9fd04a9ff1959385edb71");
            psychikineticist_burn_feature.SetDescription("A psychokineticist's mind strains when he overtaxes himself. He takes a –1 penalty on Will saves, Wisdom checks, and Wisdom-based skill checks for each point of burn he has accepted, rather than taking nonlethal damage from burn. He can accept an amount of burn equal to his Wisdom modifier (rather than 3 + his Wisdom modifier). Otherwise, his burn works just like a normal kineticist's.\nThis ability alters burn.");
        }


        static void fixDescriptors()
        {
            var blast_descriptor_map = new Dictionary<BlueprintAbility, SpellDescriptor>
            {
                {library.Get<BlueprintAbility>("d663a8d40be1e57478f34d6477a67270"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water }, //water blast
                {library.Get<BlueprintAbility>("4e2e066dd4dc8de4d8281ed5b3f4acb6"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water | SpellDescriptor.Electricity }, //charged water
                {library.Get<BlueprintAbility>("7980e876b0749fc47ac49b9552e259c1"), SpellDescriptor.Cold }, //cold blast
                {library.Get<BlueprintAbility>("3baf01649a92ae640927b0f633db7c11"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water | SpellDescriptor.Fire }, //steam blast
                {library.Get<BlueprintAbility>("b93e1f0540a4fa3478a6b47ae3816f32"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth |  (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air}, //sandstorm
                {library.Get<BlueprintAbility>("9afdc3eeca49c594aa7bf00e8e9803ac"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air | SpellDescriptor.Fire }, //plasma
                {library.Get<BlueprintAbility>("e2610c88664e07343b4f3fb6336f210c"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth }, //mud
                {library.Get<BlueprintAbility>("8c25f52fce5113a4491229fd1265fc3c"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth | SpellDescriptor.Fire }, //magma blast
                {library.Get<BlueprintAbility>("403bcf42f08ca70498432cf62abee434"), SpellDescriptor.Cold | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water}, //ice blast
                {library.Get<BlueprintAbility>("16617b8c20688e4438a803effeeee8a6"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water | SpellDescriptor.Cold }, //blizzard blast
                {library.Get<BlueprintAbility>("0ab1552e2ebdacf44bb7b20f5393366d"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air }, //air blast
            };

            foreach (var kv in blast_descriptor_map)
            {
                Common.replaceSpellDescriptor(kv.Key, kv.Value);

                foreach (var v in kv.Key.Variants)
                {
                    Common.replaceSpellDescriptor(v, kv.Value);
                }
            }
        }

        static void fixKineticistAbilitiesToBeSpelllike()
        {
            var abilities = library.GetAllBlueprints().Where<BlueprintScriptableObject>(a => a is BlueprintAbility).ToArray().Cast<BlueprintAbility>().Where(b => b.GetComponent<AbilityKineticist>() != null).ToArray();

            var combat_abilities = new BlueprintAbility[] { blade_rush_ability, blade_rush_swift_ability, whip_hurricane_ability, blade_whirlwind_ability, kinetic_whip_ability };

            foreach (var ability in abilities)
            {
                if (!combat_abilities.Contains(ability))
                {
                    ability.Type = AbilityType.SpellLike;
                }
            }
        }





        static void createInternalBuffer()
        {
            internal_buffer_resource = Helpers.CreateAbilityResource("KineticistInternalBufferResource", "", "", "", null);
            internal_buffer_resource.SetIncreasedByLevelStartPlusDivStep(0, 6, 1, 5, 1, 0, 0.0f, new BlueprintCharacterClass[] { kineticist_class });

            var icon = library.Get<BlueprintActivatableAbility>("00b6d36e31548dc4ab0ac9d15e64a980").Icon; //healing judgment
            var spend_resource = Helpers.Create<NewMechanics.ContextActionSpendResource>(s => s.resource = internal_buffer_resource);
            var buff = Helpers.CreateBuff("InternalBufferBuff",
                                          "Internal Buffer",
                                          "At 6th level, a kineticist’s study of her body and the elemental forces that course through it allow her to form an internal buffer to store extra energy.\n" +
                                          "When she would otherwise accept burn, a kineticist can spend energy from her buffer to avoid accepting 1 point of burn. She can do it once per day. Kineticist gets an additional use of this ability at levels 11 and 16.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<KineticistMechanics.DecreaseWildTalentCostWithActionOnBurn>(a => a.actions = Helpers.CreateActionList(spend_resource))
                                          );

            var ability = Helpers.CreateActivatableAbility("InternalBufferActivatableAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(internal_buffer_resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;

            internal_buffer = Common.ActivatableAbilityToFeature(ability, hide: false);
            internal_buffer.AddComponent(Helpers.CreateAddAbilityResource(internal_buffer_resource));
            kineticist_progression.LevelEntries[5].Features.Add(internal_buffer);

            var dark_elementalist_archetype = library.Get<BlueprintArchetype>("f12f18ae8842425489d29f302e69134c");
            dark_elementalist_archetype.RemoveFeatures = dark_elementalist_archetype.RemoveFeatures.AddToArray(Helpers.LevelEntry(6, internal_buffer));

            //Witch.infusion.AllFeatures = infusion_selection.AllFeatures;
        }

        static void fixShroudOfWaterForKineticKnight()
        {
            //make shroud of water give enchancement bonus to armor or shield for burn (as of now it is not capped unlike in pnp)
            var abilities = new BlueprintAbility[] { library.Get<BlueprintAbility>("d2603c237cf8d9e41b95b71e4cf0e692"), //armor
                                                     library.Get<BlueprintAbility>("78926d1c7c01f5245974c5da015d0641") }; //shield

            var buffs = new BlueprintBuff[] { library.Get<BlueprintBuff>("04d22f8c690781d4c8f61f0437cb91ef"), //armor
                                                     library.Get<BlueprintBuff>("1f1657d95529d8945964515ca44473aa") }; //shield

            var armor_feature = library.CopyAndAdd<BlueprintFeature>("1ff803cb49f63ea4185490fae2c43ca7", "ShroudOfWaterArmorKineticKnightEffectFeature", "");
            var shield_feature = library.CopyAndAdd<BlueprintFeature>("4d8feca11d6e29a499ae761b90eacdba", "ShroudOfWaterShieldKineticKnightEffectFeature", "");

            armor_feature.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: new  BlueprintCharacterClass[]{ kineticist_class },
                                                progression: ContextRankProgression.StartPlusDivStep, startLevel: - 10, stepLevel: 4, min: 4),
                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureRank, feature: armor_feature, type: Kingmaker.Enums.AbilityRankType.DamageDice),
                Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, Kingmaker.Enums.ModifierDescriptor.Armor),
                Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, Kingmaker.Enums.ModifierDescriptor.ArmorEnhancement, rankType: Kingmaker.Enums.AbilityRankType.DamageDice),
                Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[]{armor_feature })
            };


            shield_feature.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: new  BlueprintCharacterClass[]{ kineticist_class },
                                                progression: ContextRankProgression.StartPlusDivStep, startLevel: - 2, stepLevel: 4, min: 2),
                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureRank, feature: shield_feature, type: Kingmaker.Enums.AbilityRankType.DamageDice),
                Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, Kingmaker.Enums.ModifierDescriptor.Shield),
                Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, Kingmaker.Enums.ModifierDescriptor.ShieldEnhancement, rankType: Kingmaker.Enums.AbilityRankType.DamageDice),
                Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[]{shield_feature })
            };

            var features = new BlueprintFeature[] { armor_feature, shield_feature };

            for (int i = 0; i < abilities.Length; i++)
            {
                var apply_buff = abilities[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff;
                var base_buff = apply_buff.Buff;

                var new_buff = library.CopyAndAdd<BlueprintBuff>(base_buff.AssetGuid, "KineticKnight" + base_buff.name, "");
                new_buff.ReplaceComponent<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { features[i] });
                var apply_new_buff = apply_buff.CreateCopy(a => a.Buff = new_buff);

                var action = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionHasArchetype>(c => c.archetype = kinetic_knight),
                                                        apply_new_buff,
                                                        apply_buff
                                                      );
                abilities[i].ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
                var remove = Common.createContextActionRemoveBuff(new_buff);
                buffs[i].GetComponent<AddFactContextActions>().Deactivated.Actions = buffs[i].GetComponent<AddFactContextActions>().Deactivated.Actions.AddToArray(remove);
            }

            //update description
            var elemental_bastion = library.Get<BlueprintFeature>("82fbdd5eb5ac73b498c572cc71bda48f");
            elemental_bastion.SetDescription(elemental_bastion.Description + "\nNote:  If she has the shroud of water defense wild talent, whenever its bonus would be increased by accepting burn, she instead increases the enhancement bonus of her armor or shield by an equal amount.");
        }

        static void fixBladeWhirlwindRange()
        {
            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");
            //var range = blade_whirlwind.GetComponent<AbilityTargetsAround>();
            // Helpers.SetField(range, "m_Radius", 15.Feet());
            //var attack_in_range = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionEngagedByCaster>(), blade_whirlwind.GetComponent<AbilityEffectRunAction>().Actions.Actions);
            // blade_whirlwind.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(attack_in_range));
            var attack_engaged = Helpers.Create<NewMechanics.ContextActionOnTargetsInReach>(c => c.actions = blade_whirlwind.GetComponent<AbilityEffectRunAction>().Actions);
            blade_whirlwind.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(attack_engaged));
            blade_whirlwind.RemoveComponents<AbilityTargetsAround>();
            blade_whirlwind.NeedEquipWeapons = true;
            blade_whirlwind.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Special;
            blade_whirlwind.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSpecialAttack;
            //blade_whirlwind.AddComponent(CallOfTheWild.Helpers.Create<NewMechanics.AttackAnimation>());
        }


        static void restoreKineticKnightinfusions()
        {
            infusion_selection.Obligatory = false;
            var level_entries = new List<LevelEntry>();

            foreach (var rf in kinetic_knight.RemoveFeatures)
            {
                var features = new List<BlueprintFeatureBase>();
                foreach (var f in rf.Features)
                {
                    if (f != infusion_selection)
                    {
                        features.Add(f);
                    }
                }
                if (features.Count != 0)
                {
                    level_entries.Add(Helpers.LevelEntry(rf.Level, features.ToArray()));
                }
            }
            kinetic_knight.RemoveFeatures = level_entries.ToArray();
        }

        static void addWhirlwindInfusionToKineticistSelection()
        {
            infusion_selection.AllFeatures = infusion_selection.AllFeatures.AddToArray(whirlwind_infusion);
        }

        static void fixKineticHealer()
        {
            var sigil_of_creation_feature = library.Get<BlueprintFeature>("3e354532d3e41b548b883f7a67f27acf");
            //add option to offload burn to other target when using kinetic healer
            var healer_base = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c");
            healer_base.SetDescription("With a touch, you can heal a willing living creature of an amount of damage equal to your kinetic blast’s damage. Instead of paying the burn cost yourself, you can cause the recipient to take 1 point of burn. If you do so, the recipient takes 1 point of nonlethal damage per Hit Die kineticist possesses, as usual for burn; this damage can’t be healed by any means until the recipient takes a full night’s rest.");
            var healer_burn_self = library.CopyAndAdd<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c", "KineticHealerBurnSelfAbility", "");
            var healer_burn_other = library.CopyAndAdd<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c", "KineticHealerBurnOtherAbility", "");
            healer_burn_other.CanTargetSelf = false;
            healer_burn_other.ReplaceComponent<AbilityKineticist>(a => a.WildTalentBurnCost = 0);
            healer_burn_other.SetName(healer_burn_other.Name + ": Burn Offload");

            var burn_other_buff = Helpers.CreateBuff("BurnOtherBuff",
                                                     "Burn Offload",
                                                     "",
                                                     "",
                                                     healer_burn_other.Icon,
                                                     null,
                                                     Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.DamageNonLethal, Kingmaker.Enums.ModifierDescriptor.UntypedStackable),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CharacterLevel)
                                                     );
            burn_other_buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.HiddenInUi);
            burn_other_buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Stack;
            var apply_burn = Common.createContextActionApplyBuff(burn_other_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);

            healer_burn_other.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(apply_burn)));



            healer_base.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAbilityVariants(healer_base, healer_burn_self, healer_burn_other) };
            sigil_of_creation_feature.GetComponent<AutoMetamagic>().Abilities = (new BlueprintAbility[] { healer_burn_self, healer_burn_other }).ToList();
        }


        static BlueprintAbility[] getKineticBladesBurnArray()
        {
            var burns = new List<BlueprintAbility>();

            foreach (var c in blast_kinetic_blades_burn_map.Values)
            {
                burns.Add(c.Item2);
            }
            return burns.ToArray();
        }


        static void createKineticWhip()
        {
            var icon = library.Get<BlueprintAbility>("16e23c7a8ae53cc42a93066d19766404").Icon; //jolt
            var blade_enabled_buff = library.Get<BlueprintBuff>("426a9c079ee7ac34aa8e0054f2218074");
            var apply_blade = Common.createContextActionApplyBuff(blade_enabled_buff, Helpers.CreateContextDuration(), dispellable: false, is_child: true, is_permanent: true);
            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");

            kinetic_whip_buff = Helpers.CreateBuff("KineticWhipBuff",
                                                   "Kinetic Whip",
                                                   "You form a long tendril of energy or elemental matter. This functions as kinetic blade but counts as a reach weapon appropriate for your size. Unlike most reach weapons, the kinetic whip can also attack nearby creatures. The kinetic whip disappears at the beginning of your next turn, but in the intervening time, it threatens all squares within its reach, allowing you to make attacks of opportunity that deal the whip’s usual damage.",
                                                   "",
                                                   icon,
                                                   null,
                                                   Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.Reach, 4, Kingmaker.Enums.ModifierDescriptor.UntypedStackable),
                                                   Helpers.CreateAddFactContextActions(activated: apply_blade),
                                                   Helpers.Create<AddConditionImmunity>(a => a.Condition = Kingmaker.UnitLogic.UnitCondition.DisableAttacksOfOpportunity)
                                                   );

            var apply_whip = Common.createContextActionApplyBuff(kinetic_whip_buff, Helpers.CreateContextDuration(1), dispellable: false);
            kinetic_whip_ability = Helpers.CreateAbility("KineticWhipAbility",
                                                kinetic_whip_buff.Name,
                                                "Element: universal\nType: form infusion\nLevel: 3\nBurn: 2\nPrerequisites: kinetic blade\nAssociated Blasts: any\nSaving Throw: none\n" + kinetic_whip_buff.Description,
                                                "",
                                                icon,
                                                AbilityType.Special,
                                                UnitCommand.CommandType.Free,
                                                AbilityRange.Personal,
                                                Helpers.oneRoundDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_whip),
                                                blade_whirlwind.GetComponent<AbilityCasterHasFacts>(),
                                                Helpers.Create<AbilityKineticist>(a =>
                                                                                    {
                                                                                        a.InfusionBurnCost = 2;
                                                                                    }
                                                                                 ),
                                                Common.createAbilityCasterHasNoFacts(kinetic_whip_buff)
                                                );

            kinetic_whip_ability.setMiscAbilityParametersSelfOnly();

            addBladeInfusionCostIncrease(kinetic_whip_ability);


            kinetic_whip = Common.AbilityToFeature(kinetic_whip_ability, false);
            kinetic_whip.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 6));
            kinetic_whip.AddComponent(Helpers.PrerequisiteFeature(kinetic_blade_infusion));
            infusion_selection.AllFeatures = infusion_selection.AllFeatures.AddToArray(kinetic_whip);

            kinetic_knight.AddFeatures = kinetic_knight.AddFeatures.AddToArray(Helpers.LevelEntry(5, kinetic_whip));
            kineticist_progression.UIGroups.Last().Features.Add(kinetic_whip);

            //remove whip when using blade whirlwind, blade dash and swift blade dash
            var remove_whip = Common.createContextActionRemoveBuff(kinetic_whip_buff);
            blade_whirlwind_ability.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(remove_whip))));
            blade_rush_ability.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(remove_whip))));
            blade_rush_swift_ability.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(remove_whip))));
        }


        static void createSparkOfLife()
        {
            BlueprintAbility summon_elemental_medium = library.Get<BlueprintAbility>("e42b1dbff4262c6469a9ff0a6ce730e3");
            BlueprintAbility summon_elemental_large = library.Get<BlueprintAbility>("89404dd71edc1aa42962824b44156fe5");
            BlueprintAbility summon_elemental_huge = library.Get<BlueprintAbility>("766ec978fa993034f86a372c8eb1fc10");
            BlueprintAbility summon_elemental_greater = library.Get<BlueprintAbility>("8eb769e3b583f594faabe1cfdb0bb696");
            BlueprintAbility summon_elemental_elder = library.Get<BlueprintAbility>("8a7f8c1223bda1541b42fd0320cdbe2b");

            var elements = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("49e55e8f24e1ad24e910fefc0258adba"), //air
                library.Get<BlueprintFeature>("d945ac76fc6a06e44b890252824db30a"), //earth
                library.Get<BlueprintFeature>("fbed3ca8c0d89124ebb3299ccf68c439"), //fire
                library.Get<BlueprintFeature>("53a8c2f3543147b4d913c6de0c57c7e8"), //water
            };

            var summon_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", "SparkOfLifeSummonPool", "");
            var spark_of_life_ability = Helpers.CreateAbility("SparkOfLifeAbilityBase",
                                                              "Spark of Life",
                                                              "Element: universal\nType: utility\nLevel: 5\nBurn: 0\n"
                                                              + "You breathe a semblance of life into elemental matter, which takes the form of a Medium elemental of any of your elements as if summoned by summon monster IV with a caster level equal to your kineticist level, except the elemental gains the mindless trait. Each round on your turn, you must take a move action to guide the elemental or it collapses back into its component element. By accepting 1 point of burn, you can pour a bit of your own sentience into the elemental, removing the mindless quality and allowing it to persist for 1 round per kineticist level without requiring any further actions. At 12th level, you can choose to form a Large elemental as if by summon monster V; at 14th level, you can choose to form a Huge elemental as if by summon monster VI; at 16th level, you can choose to form a greater elemental as if by summon monster VII; and at 18th level, you can choose to form an elder elemental as if by summon monster VIII.",
                                                              "",
                                                              Helpers.GetIcon("e42b1dbff4262c6469a9ff0a6ce730e3"),
                                                              AbilityType.SpellLike,
                                                              UnitCommand.CommandType.Standard,
                                                              AbilityRange.Close,
                                                              "",
                                                              ""
                                                              );
            var variants = Helpers.CreateAbilityVariants(spark_of_life_ability);

            spark_of_life_ability.setMiscAbilityParametersRangedDirectional();


            var buff = Helpers.CreateBuff("SparkOfLifeBuff",
                                          "Spark of Life: Concentration",
                                          spark_of_life_ability.Description,
                                          "",
                                          spark_of_life_ability.Icon,
                                          null,
                                          Common.createAddCondition(UnitCondition.Staggered),
                                          Helpers.CreateAddFactContextActions(newRound: new GameAction[] {Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.HasUnitsInSummonPoolFromCaster>(h => {h.SummonPool = summon_pool; h.Not = true; }),
                                                                                                                                                                       Helpers.Create<NewMechanics.ContextConditionHasCondtionImmunity>(h => h.condition = UnitCondition.Staggered)
                                                                                                                                                                       ),
                                                                                                                                    Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                                    ),
                                                                                                         },
                                                                              deactivated: new GameAction[] { Helpers.Create<NewMechanics.ContextActionClearSummonPoolFromCaster>(c => c.SummonPool = summon_pool) }
                                                                              )
                                          );



            var names = new string[] { "Air", "Earth", "Fire", "Water" };


            for (int i = 0; i < 4; i++)
            {
                var action = Common.createRunActionsDependingOnContextValue(
                     Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.StatBonus),
                     summon_elemental_medium.Variants[i].GetComponent<AbilityEffectRunAction>().Actions,
                     summon_elemental_large.Variants[i].GetComponent<AbilityEffectRunAction>().Actions,
                     summon_elemental_huge.Variants[i].GetComponent<AbilityEffectRunAction>().Actions,
                     summon_elemental_greater.Variants[i].GetComponent<AbilityEffectRunAction>().Actions,
                     summon_elemental_elder.Variants[i].GetComponent<AbilityEffectRunAction>().Actions
                     );
                var normal_ability = Helpers.CreateAbility($"SparkOfLife{names[i]}Ability",
                                                          $"Spark of Life ({names[i]}, Burn)",
                                                           spark_of_life_ability.Description,
                                                           "",
                                                           spark_of_life_ability.Icon,
                                                           AbilityType.SpellLike,
                                                           UnitCommand.CommandType.Standard,
                                                           AbilityRange.Close,
                                                           Helpers.roundsPerLevelDuration,
                                                           "",
                                                           Helpers.CreateRunActions(action),
                                                           summon_elemental_medium.Variants[i].GetComponent<SpellDescriptorComponent>(),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { kineticist_class }),
                                                           Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = elements[i]),
                                                           Helpers.Create<AbilityKineticist>(a => { a.Amount = 1; a.WildTalentBurnCost = 1; }),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { kineticist_class },
                                                                                           progression: ContextRankProgression.DelayedStartPlusDivStep, type: Kingmaker.Enums.AbilityRankType.StatBonus,
                                                                                           startLevel: 10, stepLevel: 2, min: 1, max: 5)
                                                           );
                normal_ability.setMiscAbilityParametersRangedDirectional();
                var new_duration = Helpers.CreateContextDuration(1, Kingmaker.UnitLogic.Mechanics.DurationRate.Hours);
                var action2 = Common.createRunActionsDependingOnContextValue(
                             Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.StatBonus),
                             Helpers.CreateActionList((summon_elemental_medium.Variants[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).CreateCopy(sm => { sm.SummonPool = summon_pool; sm.DurationValue = new_duration; })),
                             Helpers.CreateActionList((summon_elemental_large.Variants[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).CreateCopy(sm => { sm.SummonPool = summon_pool; sm.DurationValue = new_duration; })),
                             Helpers.CreateActionList((summon_elemental_huge.Variants[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).CreateCopy(sm => { sm.SummonPool = summon_pool; sm.DurationValue = new_duration; })),
                             Helpers.CreateActionList((summon_elemental_greater.Variants[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).CreateCopy(sm => { sm.SummonPool = summon_pool; sm.DurationValue = new_duration; })),
                             Helpers.CreateActionList((summon_elemental_elder.Variants[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).CreateCopy(sm => { sm.SummonPool = summon_pool; sm.DurationValue = new_duration; }))
                             );

                var concentration_ability = Helpers.CreateAbility($"SparkOfLife{names[i]}ConcentrationAbility",
                                          $"Spark of Life ({names[i]}, Concentration)",
                                           spark_of_life_ability.Description,
                                           "",
                                           spark_of_life_ability.Icon,
                                           AbilityType.SpellLike,
                                           UnitCommand.CommandType.Standard,
                                           AbilityRange.Close,
                                           "Concentration",
                                           "",
                                           Helpers.CreateRunActions(action2,
                                                                    Common.createContextActionApplyBuffToCaster(buff, new_duration, dispellable: false)
                                                                    ),
                                           summon_elemental_medium.Variants[i].GetComponent<SpellDescriptorComponent>(),
                                           Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = elements[i]),
                                           Helpers.Create<AbilityKineticist>(a => a.Amount = 1),
                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { kineticist_class },
                                                                           progression: ContextRankProgression.DelayedStartPlusDivStep, type: Kingmaker.Enums.AbilityRankType.StatBonus,
                                                                           startLevel: 10, stepLevel: 2, min: 1, max: 5),
                                           Common.createAbilityCasterHasNoFacts(buff)
                                           );
                concentration_ability.setMiscAbilityParametersRangedDirectional();
                Common.setAsFullRoundAction(concentration_ability);
                variants.Variants = variants.Variants.AddToArray(normal_ability, concentration_ability);
                spark_of_life_ability.AddComponent(variants);
            }

            var stop_concentrating = Helpers.CreateAbility("SparkOfLifeStopConcentratingAbility",
                                                           "Spark of life: Stop Concentrating",
                                                           spark_of_life_ability.Description,
                                                           "",
                                                           LoadIcons.Image2Sprite.Create(@"AbilityIcons/Unsummon.png"),
                                                           AbilityType.Special,
                                                           UnitCommand.CommandType.Free,
                                                           AbilityRange.Personal,
                                                           "",
                                                           "",
                                                           Helpers.CreateRunActions(Common.createContextActionRemoveBuff(buff)),
                                                           Common.createAbilityCasterHasFacts(buff)
                                                           );
            stop_concentrating.setMiscAbilityParametersSelfOnly();
            variants.Variants = variants.Variants.AddToArray(stop_concentrating);

            spark_of_life = Common.AbilityToFeature(spark_of_life_ability, false);
            spark_of_life.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 10));
            var wild_talent_selection = library.Get<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459");
            wild_talent_selection.AllFeatures = wild_talent_selection.AllFeatures.AddToArray(spark_of_life);
        }


        static void createWhipHurricane()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/WhipHurricane.png");
            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");

            whip_hurricane_ability = library.CopyAndAdd<BlueprintAbility>(blade_whirlwind.AssetGuid, "WhipHurricaneAbility", "");
            whip_hurricane_ability.SetNameDescriptionIcon("Whip Hurricane",
                                                          "Element: universal\nType: form infusion\nLevel: 6\nBurn: 4\nPrerequisites: kinetic blade, kinetic whip, blade whirlwind\nAssociated Blasts: any\nSaving Throw: none\n"
                                                          + "As blade whirlwind, except it manifests a kinetic whip, and the whip lasts until the beginning of your next turn or until you use any form infusion that creates a blade or whip again, whichever comes first.",
                                                          icon);

            var add_whip = Common.createContextActionApplyBuff(kinetic_whip_buff, Helpers.CreateContextDuration(1), dispellable: false);
            whip_hurricane_ability.ReplaceComponent<AbilityExecuteActionOnCast>(a => a.Actions = Helpers.CreateActionList(Common.createContextActionOnContextCaster(add_whip)));
            whip_hurricane_ability.ReplaceComponent<AbilityKineticist>(a => a.InfusionBurnCost = 4);
            //whip_hurricane_ability.ReplaceComponent<AbilityTargetsAround>(a => Helpers.SetField(a, "m_Radius", 20.Feet()));
            addBladeInfusionCostIncrease(whip_hurricane_ability);


            whip_hurricane = Common.AbilityToFeature(whip_hurricane_ability, false);
            whip_hurricane.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 12));
            whip_hurricane.AddComponent(Helpers.PrerequisiteFeature(kinetic_blade_infusion));
            whip_hurricane.AddComponent(Helpers.PrerequisiteFeature(kinetic_whip));
            whip_hurricane.AddComponent(Helpers.PrerequisiteFeature(whirlwind_infusion));

            infusion_selection.AllFeatures = infusion_selection.AllFeatures.AddToArray(whip_hurricane);

            kinetic_knight.AddFeatures = kinetic_knight.AddFeatures.AddToArray(Helpers.LevelEntry(11, whip_hurricane));
            kineticist_progression.UIGroups.Last().Features.Add(whip_hurricane);
        }



        static void createBladeRush()
        {
            var icon = NewSpells.dimension_door_free.Icon; //freedom of movement

            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");
            blade_rush_buff = library.CopyAndAdd<BlueprintBuff>("f36da144a379d534cad8e21667079066", "BladeRushBuff", "9cf447963ba0408289d3d35e97a345a1");
            blade_rush_buff.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.AdditionalAttackBonus, 2, Kingmaker.Enums.ModifierDescriptor.None),
                Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, -2, Kingmaker.Enums.ModifierDescriptor.None),
            };
            blade_rush_ability = Helpers.CreateAbility("BladeRushAbility",
                                                            "Blade Rush",
                                                            "Element: universal\nType: form infusion\nLevel: 2\nBurn: 2\nPrerequisites: kinetic blade\nAssociated Blasts: any\nSaving Throw: none\nYou use your element’s power to instantly move 25 feet in any direction, manifest a kinetic blade, and attack once. You gain a +2 bonus on the attack roll and take a –2 penalty to your AC until the start of your next turn. The movement doesn’t provoke attacks of opportunity, though activating blade rush does. If you have the kinetic whip infusion, you can manifest a kinetic whip instead of a kinetic blade at the end of your movement by increasing the burn cost of this infusion by 1. The blade or whip vanishes instantly after the rush.",
                                                            "",
                                                            icon,
                                                            AbilityType.Special,
                                                            UnitCommand.CommandType.Standard,
                                                            AbilityRange.Close,
                                                            "",
                                                            "",
                                                            Helpers.CreateRunActions(Common.createContextActionOnContextCaster(Common.createContextActionApplyBuff(blade_rush_buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                                                     Helpers.Create<ContextActionCastSpell>(c => c.Spell = NewSpells.dimension_door_free),
                                                                                     Common.createContextActionAttack()
                                                                                     ),
                                                            blade_whirlwind.GetComponent<AbilityCasterHasFacts>(),
                                                            Helpers.Create<AbilityKineticist>(a =>
                                                                                                {
                                                                                                    a.InfusionBurnCost = 2;
                                                                                                }
                                                                                             )
                                                            );
            blade_rush_buff.SetNameDescriptionIcon(blade_rush_ability);
            blade_rush_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            blade_rush_ability.NeedEquipWeapons = true;

            addBladeInfusionCostIncrease(blade_rush_ability);

            blade_rush = Common.AbilityToFeature(blade_rush_ability, false);
            blade_rush.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 4));
            blade_rush.AddComponent(Helpers.PrerequisiteFeature(kinetic_blade_infusion));
            infusion_selection.AllFeatures = infusion_selection.AllFeatures.AddToArray(blade_rush);


            blade_rush_swift_ability = library.CopyAndAdd<BlueprintAbility>(blade_rush_ability.AssetGuid, "BladeRushSwift", "");
            blade_rush_swift_ability.ActionType = UnitCommand.CommandType.Swift;
            blade_rush_swift_ability.SetNameDescription("Blade Rush (Swift Action)",
                                                        "At 13th level as a swift action, she can accept 2 points of burn to unleash a kinetic blast with the blade rush infusion."
                                                        );
            addBladeInfusionCostIncrease(blade_rush_swift_ability);
            blade_rush_swift_ability.ReplaceComponent<AbilityKineticist>(a => a.WildTalentBurnCost = 2);

            blade_rush_swift = Common.AbilityToFeature(blade_rush_swift_ability, false, "");

            kinetic_knight.AddFeatures[2].Features.Add(blade_rush);
            kinetic_knight.AddFeatures = kinetic_knight.AddFeatures.AddToArray(Helpers.LevelEntry(13, blade_rush_swift));

            kineticist_progression.UIGroups[3].Features = kineticist_progression.UIGroups[3].Features.Take(4).ToList();
            kineticist_progression.UIGroups = kineticist_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(kinetic_blade_infusion, blade_rush, whirlwind_infusion, blade_rush_swift));
        }


        static void fixBladeWhirlwindCost()
        {
            //in vanilla blade whirlwind has cost which is independent of blast type and infusions (2)
            //it should be 3 + blast cost (either 0 or 2) + substance infusion cost
            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");

            blade_whirlwind.GetComponent<AbilityKineticist>().InfusionBurnCost = 3;

            addBladeInfusionCostIncrease(blade_whirlwind);
        }


        static void addBladeInfusionCostIncrease(BlueprintAbility blade_ability)
        {
            //add blade cost (if any)
            foreach (var blade_burn in blast_kinetic_blades_burn_map.Values)
            {
                var buff = blade_burn.Item1.Buff;
                var cost = blade_burn.Item2.GetComponent<AbilityKineticist>();
                if (cost.BlastBurnCost != 0)
                {
                    buff.AddComponent(Helpers.Create<AddKineticistBurnModifier>(a =>
                                                                                {
                                                                                    a.AppliableTo = new BlueprintAbility[] { blade_ability };
                                                                                    a.BurnType = KineticistBurnType.Blast;
                                                                                    a.Value = cost.BlastBurnCost;
                                                                                }
                                                                                                                        )
                                                                                );
                }
            }

            //add infusion cost and disallow using infusions with incompatible blades
            foreach (var infusion in substance_infusions)
            {
                //Main.logger.Log(infusion.name);
                var allowed_blasts = infusion.Buff.GetComponent<AddAreaDamageTrigger>()?.AbilityList;
                if (allowed_blasts == null)
                {
                    allowed_blasts = infusion.Buff.GetComponent<AddKineticistBurnModifier>().AppliableTo;
                }

                foreach (var blast_key in blast_kinetic_blades_burn_map.Keys)
                {
                    var blast_ability = library.Get<BlueprintAbility>(blast_key);
                    if (!allowed_blasts.Contains(blast_ability))
                    {
                        blast_kinetic_blades_burn_map[blast_key].Item1.AddComponent(Common.createActivatableAbilityRestrictionHasFact(infusion.Buff, not: true));
                    }
                }
                var infusion_burn = infusion.Buff.GetComponent<AddKineticistBurnModifier>();
                if (infusion_burn != null)
                {
                    infusion_burn.AppliableTo = infusion_burn.AppliableTo.AddToArray(blade_ability);
                }
            }
        }


        static void fixKineticBladeCostForKineticKnight()
        {
            var kinetic_elemental_blade = library.Get<BlueprintFeature>("22a6db57adc348b458cb224f3047b3b2");

            kinetic_elemental_blade.ReplaceComponent<AddKineticistBurnModifier>(a => a.AppliableTo = getKineticBladesBurnArray());
        }


        static void fixKineticBladeCost()
        {
            //apply infusions cost to compatible blades
            foreach (var infusion in substance_infusions)
            {
                var burn_modifier = infusion.Buff.GetComponent<AddKineticistBurnModifier>();
                if (burn_modifier != null)
                {
                    foreach (var blast in burn_modifier.AppliableTo)
                    {
                        var abilities_to_add = new List<BlueprintAbility>();
                        if (blast_kinetic_blades_burn_map.ContainsKey(blast.AssetGuid))
                        {
                            abilities_to_add.Add(blast_kinetic_blades_burn_map[blast.AssetGuid].Item2);
                        }
                        burn_modifier.AppliableTo = burn_modifier.AppliableTo.AddToArray(abilities_to_add.ToArray());
                    }
                }
            }

            //composite blade blasts should cost 3 (blade (1)  + composite blast (2))
            foreach (var burn in getKineticBladesBurnArray())
            {
                var cost = burn.GetComponent<AbilityKineticist>();
                if (cost.InfusionBurnCost == 2)
                {
                    cost.InfusionBurnCost = 1;
                    cost.BlastBurnCost = 2;
                }
            }

        }
    }

    //do not remove kinetic blade if we have kinetic whip active
    [Harmony12.HarmonyPatch(typeof(UnitPartKineticist))]
    [Harmony12.HarmonyPatch("RemoveBladeActivatedBuff", Harmony12.MethodType.Normal)]
    class UnitPartKineticist__RemoveBladeActivatedBuff__Patch
    {
        static bool Prefix(UnitPartKineticist __instance, ref AddKineticistPart ___m_Settings)
        {
            Main.TraceLog();
            if (__instance.Owner.Buffs.HasFact(KineticistFix.kinetic_whip_buff))
            {
                return false;
            }

            /*var blade_enabled_buff = __instance.Owner.Buffs.GetBuff(___m_Settings.BladeActivatedBuff);

            if (Main.settings.kinetic_blade_refresh_for_tb && __instance.Owner.Unit.IsInCombat && blade_enabled_buff != null)
            {//remove after 5 seconds to avoid paying cost for next round in the previous one, will not properly work in rt
                blade_enabled_buff.RemoveAfterDelay(new TimeSpan(0, 0, 5));
            }
            else
            {
                __instance.Owner.Buffs.RemoveFact(___m_Settings.BladeActivatedBuff);
            }*/

            __instance.Owner.Buffs.RemoveFact(___m_Settings.BladeActivatedBuff);
            return false;
        }
    }


    //avoid getting burn on the queued command if it has cooldown
    [Harmony12.HarmonyPatch(typeof(KineticistController))]
    [Harmony12.HarmonyPatch("TryRunKineticBladeActivationAction", Harmony12.MethodType.Normal)]
    class Patch_KineticistController_TryRunKineticBladeActivationAction_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_blade_activated = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("IsActivatingBladeNow"));

            codes[check_blade_activated] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1); //cmd
            codes.Insert(check_blade_activated + 1, new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<UnitPartKineticist, UnitCommand, bool>(shouldReturnToQueue).Method));

            return codes.AsEnumerable();
        }

        private static bool shouldReturnToQueue(UnitPartKineticist kineticist, UnitCommand cmd)
        {
            Main.TraceLog();
            return kineticist.IsActivatingBladeNow || kineticist.Owner.Unit.CombatState.HasCooldownForCommand(cmd);
        }
    }


    //fix concentration checks for kineticist
    [Harmony12.HarmonyPatch(typeof(RuleCheckConcentration))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RuleCheckConcentration__OnTrigger__Patch
    {
        static bool Prefix(RuleCheckConcentration __instance, RulebookEventContext context)
        {
            if (__instance.Spell.Blueprint.GetComponent<AbilityKineticist>() == null)
            {
                return true;
            }
            var kineticist_part = __instance.Initiator.Get<UnitPartKineticist>();
            if (kineticist_part == null)
            {
                return true;
            }

            var tr = Harmony12.Traverse.Create(__instance);
            var rule = Rulebook.Trigger<RuleCalculateAbilityParams>(new RuleCalculateAbilityParams(__instance.Initiator, __instance.Spell));
            var ability_params = rule.Result;

            tr.Property("DC").SetValue(__instance.Damage == null ? 15 + ability_params.SpellLevel : 10 + ability_params.SpellLevel + __instance.Damage.Damage / 2);

            var bonus_concentration = Helpers.GetField<int>(rule, "m_BonusConcentration");
            tr.Property("Concentration").SetValue(bonus_concentration + kineticist_part.ClassLevel + kineticist_part.MainStatBonus);
            tr.Property("ResultRollRaw").SetValue(RulebookEvent.Dice.D20);
            return false;
        }
    }


    //fix concentration checks for kineticist
    [Harmony12.HarmonyPatch(typeof(RuleCheckCastingDefensively))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RuleCheckCastingDefensively__OnTrigger__Patch
    {
        static bool Prefix(RuleCheckCastingDefensively __instance, RulebookEventContext context)
        {
            if (__instance.Spell.Blueprint.GetComponent<AbilityKineticist>() == null)
            {
                return true;
            }
            var kineticist_part = __instance.Initiator.Get<UnitPartKineticist>();
            if (kineticist_part == null)
            {
                return true;
            }

            var tr = Harmony12.Traverse.Create(__instance);
            var rule = Rulebook.Trigger<RuleCalculateAbilityParams>(new RuleCalculateAbilityParams(__instance.Initiator, __instance.Spell));
            var ability_params = rule.Result;

            tr.Property("DC").SetValue(15 + ability_params.SpellLevel * 2);

            var bonus_concentration = Helpers.GetField<int>(rule, "m_BonusConcentration");
            tr.Property("Concentration").SetValue(bonus_concentration + kineticist_part.ClassLevel + kineticist_part.MainStatBonus);
            tr.Property("ResultRollRaw").SetValue(RulebookEvent.Dice.D20);
            return false;
        }
    }


    //fix addAreaDamageTrigger to account for descriptors of ability that provoked it (for (greater) elemental fcous feat)
    [Harmony12.HarmonyPatch(typeof(AddAreaDamageTrigger))]
    [Harmony12.HarmonyPatch("RunAction", Harmony12.MethodType.Normal)]
    class AddAreaDamageTrigger__RunAction__Patch
    {
        static bool Prefix(AddAreaDamageTrigger __instance, UnitEntityData target, ref TimeSpan ___m_LastFrameTime, ref HashSet<UnitEntityData> ___m_AffectedThisFrame)
        {
            var ability_context = Helpers.GetMechanicsContext()?.SourceAbilityContext;      
            var original_blueprint = __instance.Fact.MaybeContext?.AssociatedBlueprint;

            TimeSpan gameTime = Game.Instance.TimeController.GameTime;
            if (gameTime != ___m_LastFrameTime)
            {
                ___m_LastFrameTime = gameTime;
                ___m_AffectedThisFrame.Clear();
            }
            if (!___m_AffectedThisFrame.Add(target) || !__instance.Actions.HasActions)
                return false;
            if (__instance.OwnerArea != null)
            {
                ability_context = __instance.OwnerArea.Context?.SourceAbilityContext;
            }

            if (ability_context != null)
            {

            }
            else
            {
                Main.logger.Log("Context is null");
            }

            Helpers.SetField(__instance.Fact.MaybeContext, "AssociatedBlueprint", ability_context?.AssociatedBlueprint);
            //__instance.Fact.MaybeContext?.RecalculateAbilityParams(); //will trigger RuleCalculate ability params since it normally has ContextAbilityParamsCalculator component

            (__instance.Fact as IFactContextOwner)?.RunActionInContext(__instance.Actions, (TargetWrapper)target);
            Helpers.SetField(__instance.Fact.MaybeContext, "AssociatedBlueprint", original_blueprint);
            //__instance.Fact.MaybeContext?.RecalculateAbilityParams();

            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(IncreaseSpellDescriptorDC))]
    [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
    class IncreaseSpellDescriptorDC__OnEventAboutToTrigger__Patch
    {
        static bool Prefix(IncreaseSpellDescriptorDC __instance, RuleCalculateAbilityParams evt)
        {
            var ability_context = Helpers.GetMechanicsContext()?.SourceAbilityContext;
            SpellDescriptorComponent component = ability_context.AssociatedBlueprint?.GetComponent<SpellDescriptorComponent>();
            if ((component != null ? (component.Descriptor.HasAnyFlag((SpellDescriptor)__instance.Descriptor) ? 1 : 0) : 0) == 0)
                return false;
            evt.AddBonusDC(__instance.BonusDC);
            return false;
        }
    }

}
