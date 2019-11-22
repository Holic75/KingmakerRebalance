using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Commands.Base;
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
    class KineticistFix
    {
        static LibraryScriptableObject library => Main.library;
        static Dictionary<string, (BlueprintActivatableAbility, BlueprintAbility)> blast_kinetic_blades_burn_map = new Dictionary<string, (BlueprintActivatableAbility, BlueprintAbility)>();
        static List<BlueprintActivatableAbility> substance_infusions = new List<BlueprintActivatableAbility>();

        static BlueprintFeature blade_rush;
        static BlueprintFeature blade_rush_swift;
        static BlueprintCharacterClass kineticist_class = library.Get<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");
        static BlueprintFeature kinetic_blade_infusion = library.Get<BlueprintFeature>("9ff81732daddb174aa8138ad1297c787");
        static BlueprintFeature whirlwind_infusion = library.Get<BlueprintFeature>("80fdf049d396c33408a805d9e21a42e1");

        static BlueprintFeatureSelection infusion_selection = library.Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
        static BlueprintArchetype kinetic_knight = library.Get<BlueprintArchetype>("7d61d9b2250260a45b18c5634524a8fb");
        static BlueprintProgression kineticist_progression = library.Get<BlueprintProgression>("b79e92dd495edd64e90fb483c504b8df");

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
            //restoreKineticKnightinfusions();
            whirlwind_infusion.HideInUI = false;
            fixKineticBladeCost();
            fixKineticBladeCostForKineticKnight();
            fixBladeWhirlwindCost();
            fixKineticHealer();

            createBladeRush();
        }


        static void restoreKineticKnightinfusions()
        {
            var level_entries = new List<LevelEntry>();

            foreach (var rf in kinetic_knight.RemoveFeatures)
            {
                var features = new List<BlueprintFeatureBase>();
                foreach (var f in rf.Features)
                {
                    if (f != infusion_selection || rf.Level == 3)
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


        static void createBladeRush()
        {
            var icon = NewSpells.dimension_door_free.Icon; //freedom of movement

            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");
            var charge_buff = library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066");
            
            var blade_rush_ability = Helpers.CreateAbility("BladeRushAbility",
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


            var blade_rush_ability_swift = library.CopyAndAdd<BlueprintAbility>(blade_rush_ability.AssetGuid, "BladeRushSwift", "");
            blade_rush_ability_swift.ActionType = UnitCommand.CommandType.Swift;
            blade_rush_ability_swift.SetNameDescription("Blade Rush (Swift Action)",
                                                        "At 13th level as a swift action, she can accept 2 points of burn to unleash a kinetic blast with the blade rush infusion."
                                                        );
            addBladeInfusionCostIncrease(blade_rush_ability_swift);
            blade_rush_ability_swift.ReplaceComponent<AbilityKineticist>(a => a.WildTalentBurnCost = 2);

            blade_rush_swift = Common.AbilityToFeature(blade_rush_ability_swift, false, "");

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
}
