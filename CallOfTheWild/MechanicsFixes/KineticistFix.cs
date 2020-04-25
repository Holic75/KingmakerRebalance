using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
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

        internal static void load()
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
            createInternalBuffer();
            fixKineticistAbilitiesToBeSpelllike();
        }


        static void fixKineticistAbilitiesToBeSpelllike()
        {
            var abilities = library.GetAllBlueprints().Where<BlueprintScriptableObject>(a => a is BlueprintAbility).ToArray().Cast<BlueprintAbility>().Where(b => b.GetComponent< AbilityKineticist>() != null).ToArray();

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

            //fix previous saves without buffer
            Action<UnitDescriptor> save_game_fix = delegate (UnitDescriptor unit)
            {
                if (unit.Progression.GetClassLevel(kineticist_class) >= 6 && !unit.Progression.Features.HasFact(internal_buffer)
                    && !unit.Progression.IsArchetype(Archetypes.OverwhelmingSoul.archetype))
                {
                    unit.Progression.Features.AddFeature(internal_buffer);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_fix);
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
            var level_entries = new List<LevelEntry>();

            foreach (var rf in kinetic_knight.RemoveFeatures)
            {
                var features = new List<BlueprintFeatureBase>();
                foreach (var f in rf.Features)
                {
                    if (f != infusion_selection || rf.Level <= 3)
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
            //add option to offload burn to other target when using kinetic healer
            var healer_base = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c");
            healer_base.SetDescription("With a touch, you can heal a willing living creature of an amount of damage equal to your kinetic blast’s damage. Instead of paying the burn cost yourself, you can cause the recipient to take 1 point of burn. If you do so, the recipient takes 1 point of nonlethal damage per Hit Die kineticist possesses, as usual for burn; this damage can’t be healed by any means until the recipient takes a full night’s rest.");
            var healer_burn_self = library.CopyAndAdd<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c", "KineticHealerBurnSelfAbility", "");
            var healer_burn_other = library.CopyAndAdd<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c", "KineticHealerBurnOtherAbility", "");
            healer_burn_other.CanTargetSelf = false;
            healer_burn_other.ReplaceComponent<AbilityKineticist>(a => a.WildTalentBurnCost = 0);
            healer_burn_other.SetName(healer_burn_other.Name + " (Burn Offload)");

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


            
            healer_base.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAbilityVariants(healer_base, healer_burn_self, healer_burn_other)};
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
            var charge_buff = library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066");
            
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
                                                            Helpers.CreateRunActions(Common.createContextActionOnContextCaster(Common.createContextActionApplyBuff(charge_buff, Helpers.CreateContextDuration(1), dispellable: false)),
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
            codes.Insert(check_blade_activated + 1, new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,  new Func<UnitPartKineticist, UnitCommand, bool>(shouldReturnToQueue).Method));

            return codes.AsEnumerable();
        }

        private static bool shouldReturnToQueue(UnitPartKineticist kineticist, UnitCommand cmd)
        {
            return kineticist.IsActivatingBladeNow || kineticist.Owner.Unit.CombatState.HasCooldownForCommand(cmd);
        }
    }

    }
