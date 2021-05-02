using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public enum CombatManeuverTypeExtender
    {
        None,
        AwesomeBlow = 100
    }

    public static partial class Extensions
    {
        public static CombatManeuver ToCombatManeuverType(this CombatManeuverTypeExtender extra_maneuver)
        {
            int value = (int)extra_maneuver;
            return (CombatManeuver)value;
        }
    }

    namespace CombatManeuverMechanics
    {
        [Harmony12.HarmonyPatch(typeof(RuleCombatManeuver))]
        [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
        class RuleCombatManeuver__OnTrigger__Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var create_exception_string = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Ldstr && x.operand.ToString().Contains("Unsupported"));
                var ret = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Ret);

                codes.InsertRange(create_exception_string + 1, new Harmony12.CodeInstruction[]
                    {
                        new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0),
                        new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                           new Func<RuleCombatManeuver, bool>(RuleCombatManeuver__OnTrigger__Patch.checkExtraManeuvers).Method
                                                                           ),
                        new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Brfalse_S, codes[ret].labels[0])
                    }
                );
                return codes.AsEnumerable();
            }

            static bool checkExtraManeuvers(RuleCombatManeuver evt)
            {
                switch (evt.Type)
                {
                    case (CombatManeuver)CombatManeuverTypeExtender.AwesomeBlow:
                        {
                            if (evt.Target.CanBeKnockedOff())
                            {
                                evt.Target.Descriptor.State.Prone.ShouldBeActive = true;
                                EventBus.RaiseEvent<IKnockOffHandler>((Action<IKnockOffHandler>)(h => h.HandleKnockOff(evt.Initiator, evt.Target)));
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unsupported combat maneuver type: " + (object)evt.Type);
                }
                return true;
            }
        }


        public class UnitPartFakeSizeBonus : AdditiveUnitPart
        {
            public Size getEffectiveSize()
            {
                var unit_size = this.Owner.State.Size;
                if (buffs.Empty())
                {
                    return unit_size;
                }

                int bonus = buffs[0].Blueprint.GetComponent<FakeSizeBonus>().bonus;
                for (int i = 1; i < buffs.Count; i++)
                {
                    var c = buffs[i].Blueprint.GetComponent<FakeSizeBonus>();

                    if (c.bonus > bonus)
                    {
                        bonus = c.bonus;
                    }
                }

                return unit_size.Shift(bonus);
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class CMBBonusAgainstFlanked : RuleInitiatorLogicComponent<RuleCombatManeuver>
        {
            public ContextValue Value;

            private MechanicsContext Context
            {
                get
                {
                    MechanicsContext context = (this.Fact as Buff)?.Context;
                    if (context != null)
                        return context;
                    return (this.Fact as Feature)?.Context;
                }
            }

            public override void OnEventAboutToTrigger(RuleCombatManeuver evt)
            {
                if (!evt.Target.CombatState.IsFlanked || !evt.Initiator.CombatState.IsEngage(evt.Target))
                {
                    return;
                }
                evt.AddBonus(this.Value.Calculate(this.Context), this.Fact);
            }

            public override void OnEventDidTrigger(RuleCombatManeuver evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class FakeSizeBonus : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber, IInitiatorRulebookHandler<RuleCalculateBaseCMB>, IInitiatorRulebookHandler<RuleCalculateBaseCMD>
        {
            public int bonus;

            public void OnEventAboutToTrigger(RuleCalculateBaseCMB evt)
            {
                var old_size = this.Owner.State.Size;
                var new_size = old_size.Shift(bonus);
                if (old_size == new_size)
                {
                    return;
                }

                evt.AddBonus(new_size.GetModifiers().CMDAndCMD - old_size.GetModifiers().CMDAndCMD + new_size.GetModifiers().AttackAndAC - old_size.GetModifiers().AttackAndAC, this.Fact);
            }

            public void OnEventAboutToTrigger(RuleCalculateBaseCMD evt)
            {
                var old_size = this.Owner.State.Size;
                var new_size = old_size.Shift(bonus);
                if (old_size == new_size)
                {
                    return;
                }

                evt.AddBonus(new_size.GetModifiers().CMDAndCMD - old_size.GetModifiers().CMDAndCMD + new_size.GetModifiers().AttackAndAC - old_size.GetModifiers().AttackAndAC, this.Fact);
            }

            public void OnEventDidTrigger(RuleCalculateBaseCMB evt)
            {
            }

            public void OnEventDidTrigger(RuleCalculateBaseCMD evt)
            {
            }

            public override void OnTurnOn()
            {
                this.Owner.Ensure<UnitPartFakeSizeBonus>().addBuff(this.Fact);
            }


            public override void OnTurnOff()
            {
                this.Owner.Ensure<UnitPartFakeSizeBonus>().removeBuff(this.Fact);
            }
        }


        public class ContextConditionCasterSizeGreater : ContextCondition
        {
            public int size_delta;

            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                if (Target == null)
                {
                    return false;
                }

                return (int)this.Context.MaybeCaster.Ensure<UnitPartFakeSizeBonus>().getEffectiveSize() - (int)this.Target.Unit.Ensure<UnitPartFakeSizeBonus>().getEffectiveSize() >= size_delta;
            }
        }


        public class ContextConditionTargetSizeLessOrEqual : ContextCondition
        {
            public Size target_size;

            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                if (Target == null)
                {
                    return false;
                }

                return this.Target.Unit.Ensure<UnitPartFakeSizeBonus>().getEffectiveSize() <= target_size;
            }
        }


        public class AbilityTargetNotBiggerUnlesFact : BlueprintComponent, IAbilityTargetChecker
        {
            public BlueprintUnitFact fact;
            public bool Not;

            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                var unit = target.Unit;
                if (unit == null || caster == null)
                {
                    return false;
                }

                return ((unit.Descriptor.State.Size <= caster.Descriptor.State.Size) != Not) || (fact != null && caster.Descriptor.HasFact(fact));
            }
        }


        public class ContextConditionCasterBuffRankLess : ContextCondition
        {
            public BlueprintBuff buff;
            public int rank;

            protected override string GetConditionCaption()
            {
                return string.Empty;
            }

            protected override bool CheckCondition()
            {
                var caster = this.Context.MaybeCaster;
                if (caster == null)
                {
                    return false;
                }
                return (caster.Buffs.Enumerable.Where(b => b.Blueprint == buff).Count() < rank);
            }
        }


        public class ContextActionSpitOut : ContextAction
        {

            public override string GetCaption()
            {
                return "Spit out";
            }

            public override void RunAction()
            {
                var caster = this.Context.MaybeCaster;

                if (caster == null)
                {
                    return;
                };
                caster.Ensure<UnitPartSwallowWhole>().SpitOut(true);
            }
        }


        public class ContextActionForceMove : ContextAction
        {
            public bool provoke_aoo = false;
            public ContextDiceValue distance_dice;

            public override string GetCaption()
            {
                return "Force Move";
            }

            public override void RunAction()
            {
                var caster = this.Context.MaybeCaster;

                var unit = this.Target.Unit;

                if (unit == null || caster == null)
                {
                    return;
                }
                var distance = distance_dice.Calculate(this.Context);
                unit.Ensure<UnitPartForceMove>().Push((unit.Position - caster.Position).normalized, distance.Feet().Meters, provoke_aoo);
            }
        }



        [AllowedOn(typeof(BlueprintUnitFact))]
        public class CombatManeuverBonus : RuleInitiatorLogicComponent<RuleCalculateCMB>
        {
            public ContextValue Value;

            private MechanicsContext Context
            {
                get
                {
                    MechanicsContext context = (this.Fact as Buff)?.Context;
                    if (context != null)
                        return context;
                    return (this.Fact as Feature)?.Context;
                }
            }

            public override void OnEventAboutToTrigger(RuleCalculateCMB evt)
            {
                evt.AddBonus(this.Value.Calculate(this.Context), this.Fact);
            }

            public override void OnEventDidTrigger(RuleCalculateCMB evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ViciousStomp : OwnedGameLogicComponent<UnitDescriptor>, IKnockOffHandler, IGlobalSubscriber
        {
            public void HandleKnockOff(UnitEntityData initiator, UnitEntityData target)
            {
                if (target == initiator || initiator == null || target == null)
                {
                    return;
                }

                if (!this.Owner.Unit.CombatState.IsEngage(target))
                {
                    return;
                }

                if (this.Owner.Body.PrimaryHand?.MaybeWeapon == null)
                {
                    return;
                }

                if (this.Owner.Body.PrimaryHand.Weapon.Blueprint.IsUnarmed
                    || FeralCombatTraining.checkHasFeralCombat(this.Owner.Unit, this.Owner.Body.PrimaryHand.Weapon))
                {
                    Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(Owner.Unit, target);
                }
            }
        }



        [ComponentName("Maneuver Defence Bonus")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ContextManeuverDefenceBonus : RuleTargetLogicComponent<RuleCalculateCMB>
        {
            public ContextValue Bonus;

            public override void OnEventAboutToTrigger(RuleCalculateCMB evt)
            {
                evt.AddBonus(this.Bonus.Calculate(this.Fact.MaybeContext), this.Fact);
            }

            public override void OnEventDidTrigger(RuleCalculateCMB evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SpecificCombatManeuverBonus : RuleInitiatorLogicComponent<RuleCalculateCMB>
        {
            public ContextValue Value;
            public CombatManeuver maneuver_type;

            private MechanicsContext Context
            {
                get
                {
                    MechanicsContext context = (this.Fact as Buff)?.Context;
                    if (context != null)
                        return context;
                    return (this.Fact as Feature)?.Context;
                }
            }

            public override void OnEventAboutToTrigger(RuleCalculateCMB evt)
            {
                if (evt.Type == maneuver_type)
                {
                    evt.AddBonus(this.Value.Calculate(this.Context), this.Fact);
                }
            }

            public override void OnEventDidTrigger(RuleCalculateCMB evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SpecificCombatManeuverBonusUnlessHasFacts : RuleInitiatorLogicComponent<RuleCalculateCMB>
        {
            public ContextValue Value;
            public CombatManeuver maneuver_type;
            public static List<BlueprintUnitFact> facts = new List<BlueprintUnitFact>();

            private MechanicsContext Context
            {
                get
                {
                    MechanicsContext context = (this.Fact as Buff)?.Context;
                    if (context != null)
                        return context;
                    return (this.Fact as Feature)?.Context;
                }
            }

            public override void OnEventAboutToTrigger(RuleCalculateCMB evt)
            {
                if (evt.Type == maneuver_type)
                {
                    var bab = evt.Initiator.Stats.BaseAttackBonus.ModifiedValue;
                    var lvl = evt.Initiator.Descriptor.Progression.CharacterLevel;
                    var bonus = Math.Min(this.Value.Calculate(this.Context), Math.Max(lvl - bab, 0));
                    evt.AddBonus(bonus, this.Fact);
                }
            }

            public override void OnEventDidTrigger(RuleCalculateCMB evt)
            {
            }
        }


        public class ApplyBuffOnHit : GameLogicComponent, ITargetRulebookHandler<RuleAttackWithWeapon>, IRulebookHandler<RuleAttackWithWeapon>, IInitiatorRulebookSubscriber
        {
            public bool apply_to_ranged = false;
            public bool apply_to_natural = false;

            public ActionList main_hand_action = null;
            public ActionList off_hand_action = null;
            public ActionList other_action = null;


            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {

            }

            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {

                if (evt.Weapon.Blueprint.IsRanged && !apply_to_ranged)
                {
                    return;
                }

                if ((evt.Weapon.Blueprint.IsNatural || evt.Weapon.Blueprint.IsUnarmed) && !apply_to_natural)
                {
                    return;
                }

                if (evt.Weapon.HoldingSlot == evt.Initiator.Body.PrimaryHand)
                {
                    if (main_hand_action != null)
                    {
                        using (new ContextAttackData(evt.AttackRoll, (Projectile)null))
                            (this.Fact as IFactContextOwner)?.RunActionInContext(this.main_hand_action, (TargetWrapper)evt.Initiator);
                    }
                }
                else if (evt.Weapon.HoldingSlot == evt.Initiator.Body.SecondaryHand)
                {
                    if (off_hand_action != null)
                    {
                        using (new ContextAttackData(evt.AttackRoll, (Projectile)null))
                            (this.Fact as IFactContextOwner)?.RunActionInContext(this.off_hand_action, (TargetWrapper)evt.Initiator);
                    }
                }
                else
                {
                    if (other_action != null)
                    {
                        using (new ContextAttackData(evt.AttackRoll, (Projectile)null))
                            (this.Fact as IFactContextOwner)?.RunActionInContext(this.other_action, (TargetWrapper)evt.Initiator);
                    }
                }
            }
        }


        public class ContextActionBreakFreeFromSpellGrapple : ContextAction
        {
            public ActionList Success;
            public ActionList Failure;

            public override void RunAction()
            {
                if (this.Target.Unit == null)
                {
                    UberDebug.LogError((object)"Target unit is missing", (object[])Array.Empty<object>());
                }
                else
                {

                    if (tryBreakFree(this.Target.Unit, this.Context.MaybeCaster, this.Context))
                        this.Success.Run();
                    else
                        this.Failure.Run();
                }
            }

            private static bool tryBreakFree(UnitEntityData target, UnitEntityData grappler, MechanicsContext context)
            {
                int dc = 0;
                if (context != null)
                {
                    dc = context.Params.DC;
                }
                RuleCalculateCMB rule_calcualte_cmb1 = new RuleCalculateCMB(target, grappler, CombatManeuver.Grapple);
                RuleCalculateCMB rule_calcualte_cmb2 = context?.TriggerRule<RuleCalculateCMB>(rule_calcualte_cmb1) ?? Rulebook.Trigger<RuleCalculateCMB>(rule_calcualte_cmb1);
                if (Math.Max((int)(target.Stats.SkillMobility), (int)(target.Stats.SkillAthletics)) < rule_calcualte_cmb2.Result)
                {
                    RuleSkillCheck rule_skill_check = new RuleSkillCheck(target, StatType.AdditionalCMB, dc);
                    rule_skill_check.Bonus.AddModifier(rule_calcualte_cmb2.Result - target.Stats.AdditionalCMB.ModifiedValue, null, ModifierDescriptor.UntypedStackable);
                    return (context?.TriggerRule<RuleSkillCheck>(rule_skill_check) ?? Rulebook.Trigger<RuleSkillCheck>(rule_skill_check)).IsPassed;
                }
                else
                {
                    StatType stat = (int)(target.Stats.SkillMobility) > (int)(target.Stats.SkillAthletics) ? StatType.SkillMobility : StatType.SkillAthletics;
                    RuleSkillCheck rule_skill_check = new RuleSkillCheck(target, stat, dc);
                    return (context?.TriggerRule<RuleSkillCheck>(rule_skill_check) ?? Rulebook.Trigger<RuleSkillCheck>(rule_skill_check)).IsPassed;
                }
            }

            public override string GetCaption()
            {
                return "Check to break free from entangle or grapple";
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class ManeuverOnAttackWithBonus : RuleInitiatorLogicComponent<RuleAttackWithWeapon>
        {
            public WeaponCategory[] categories = new WeaponCategory[0];
            public CombatManeuver maneuver;
            public ContextValue bonus;
            public bool use_swift_action = false;

            public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
            }

            public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (evt.Weapon == null)
                    return;

                if (use_swift_action && evt.Initiator.CombatState.Cooldown.SwiftAction > 0.0f)
                {
                    return;
                }


                if (!categories.Empty() && !categories.Contains(evt.Weapon.Blueprint.Category))
                {
                    return;
                }

                if (!evt.AttackRoll.IsHit)
                {
                    return;
                }

                if (this.maneuver == CombatManeuver.Trip && (evt.Target.Descriptor.State.Prone.Active || (bool)(evt.Target.View) && evt.Target.View.IsGetUp))
                {
                    return;
                }

                var rule = new RuleCombatManeuver(this.Owner.Unit, evt.Target, this.maneuver);
                rule.AddBonus(this.bonus.Calculate(this.Fact.MaybeContext), this.Fact);
                Rulebook.Trigger<RuleCombatManeuver>(new RuleCombatManeuver(this.Owner.Unit, evt.Target, this.maneuver));

                if (use_swift_action)
                {
                    evt.Initiator.CombatState.Cooldown.SwiftAction = 6.0f;
                }
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ReplaceCombatManeuverStatForSpecificManeuver : RuleInitiatorLogicComponent<RuleCalculateCMB>
        {
            public StatType StatType;
            public CombatManeuver[] maneuvers;

            public override void OnEventAboutToTrigger(RuleCalculateCMB evt)
            {
                if (!maneuvers.Contains(evt.Type))
                {
                    return;
                }
                evt.ReplaceStrength = new StatType?(this.StatType);
            }

            public override void OnEventDidTrigger(RuleCalculateCMB evt)
            {
            }
        }


        [AllowMultipleComponents]
        public class AddInitiatorManeuverWithWeaponTrigger : GameLogicComponent, IInitiatorRulebookHandler<RuleCombatManeuver>
        {
            public bool OnlySuccess = true;
            public BlueprintWeaponType WeaponType = null;
            public bool CheckWeaponCategory;
            [ShowIf("CheckWeaponCategory")]
            public WeaponCategory Category;
            public bool CheckWeaponRangeType;
            [ShowIf("CheckWeaponRangeType")]
            public AttackTypeAttackBonus.WeaponRangeType RangeType;
            public bool ActionsOnInitiator;
            public bool CheckDistance;
            [ShowIf("CheckDistance")]
            public Feet DistanceLessEqual;
            public bool AllNaturalAndUnarmed;
            public bool DuelistWeapon;
            public ActionList Action;

            public CombatManeuver[] Maneuvers = new CombatManeuver[0];


            private void TryRunActions(RuleAttackRoll rule)
            {
                if (!this.CheckCondition(rule))
                    return;
                if (!this.ActionsOnInitiator)
                {
                    using (new ContextAttackData(rule, (Projectile)null))
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.Action, (TargetWrapper)rule.Target);
                }
                else
                {
                    using (new ContextAttackData(rule, (Projectile)null))
                        (this.Fact as IFactContextOwner)?.RunActionInContext(this.Action, (TargetWrapper)rule.Initiator);
                }
            }

            private bool CheckCondition(RuleAttackRoll evt)
            {
                Main.logger.Log(evt.Weapon.Blueprint.name);
                ItemEntity owner = (this.Fact as ItemEnchantment)?.Owner;

                if (owner != null && owner != evt.Weapon
                    || (this.WeaponType != null && this.WeaponType != evt.Weapon.Blueprint.Type || this.CheckWeaponCategory && this.Category != evt.Weapon.Blueprint.Category)
                    || (this.CheckWeaponRangeType && !AttackTypeAttackBonus.CheckRangeType(evt.Weapon.Blueprint, this.RangeType))
                    || this.AllNaturalAndUnarmed && !evt.Weapon.Blueprint.Type.IsNatural && !evt.Weapon.Blueprint.Type.IsUnarmed)
                    return false;

                if (this.CheckDistance && (double)evt.Target.DistanceTo(evt.Initiator) > (double)this.DistanceLessEqual.Meters)
                    return false;
                bool flag = evt.Weapon.Blueprint.Category.HasSubCategory(WeaponSubCategory.Light) || evt.Weapon.Blueprint.Category.HasSubCategory(WeaponSubCategory.OneHandedPiercing) || (bool)evt.Initiator.Descriptor.State.Features.DuelingMastery && evt.Weapon.Blueprint.Category == WeaponCategory.DuelingSword || evt.Initiator.Descriptor.Ensure<DamageGracePart>().HasEntry(evt.Weapon.Blueprint.Category);
                return !this.DuelistWeapon || flag;
            }

            public void OnEventAboutToTrigger(RuleCombatManeuver evt)
            {

            }

            public void OnEventDidTrigger(RuleCombatManeuver evt)
            {
                if (!Maneuvers.Empty() && !Maneuvers.Contains(evt.Type))
                {
                    return;
                }
                if (!evt.Success && OnlySuccess)
                {
                    return;
                }

                var attack_rule = Rulebook.CurrentContext.AllEvents.LastOfType<RuleAttackRoll>();

                if (attack_rule == null)
                {
                    return;
                }

                TryRunActions(attack_rule);
            }
        }


        //fix Success to return false in case of autofailure or immunity to combat maneuver
        [Harmony12.HarmonyPatch(typeof(RuleCombatManeuver))]
        [Harmony12.HarmonyPatch("Success", Harmony12.MethodType.Getter)]
        class RuleCombatManeuver__Success__Patch
        {
            static void Postfix(RuleCombatManeuver __instance, ref bool __result)
            {
                if (__instance.AutoFailure //already includes immunity to specific maneuvers 
                    || __instance.Target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers)
                    || (__instance.Target.Descriptor.State.HasConditionImmunity(UnitCondition.Prone)
                        && (__instance.Type == CombatManeuver.Trip || __instance.Type == CombatManeuverTypeExtender.AwesomeBlow.ToCombatManeuverType())
                        )
                    )
                {
                    //TODO: should probably add checks for validity of other maneuvers but currently it is only used in greater trip, greater bull rush, so it looks sufficient
                    __result = false;
                }
            }
        }


        [Harmony12.HarmonyPatch(typeof(ContextActionCombatManeuver))]
        [Harmony12.HarmonyPatch("RunAction", Harmony12.MethodType.Normal)]
        class ContextActionCombatManeuver__RunAction__Patch
        {
            static bool Prefix(ContextActionCombatManeuver __instance)
            {
                var tr = Harmony12.Traverse.Create(__instance);
                var target = tr.Property("Target").GetValue<TargetWrapper>();
                var unit = target?.Unit;
                if (unit == null)
                {
                    return true;
                }

                if (__instance.Type == CombatManeuverTypeExtender.AwesomeBlow.ToCombatManeuverType() 
                    && (unit.Descriptor.State.Prone.Active || unit.View.IsGetUp))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
