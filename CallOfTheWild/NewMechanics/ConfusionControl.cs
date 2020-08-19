using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.Visual.HitSystem;
using System;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Newtonsoft.Json;
using Kingmaker.Utility;
using Kingmaker.UI.GenericSlot;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.EntitySystem.Entities;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.Blueprints.Validation;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.Controllers;
using Kingmaker;
using static Kingmaker.UnitLogic.Abilities.Components.AbilityCustomMeleeAttack;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.EntitySystem;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.EntitySystem.Persistence.Versioning;
using JetBrains.Annotations;
using Kingmaker.Enums.Damage;
using Kingmaker.Inspect;


namespace CallOfTheWild
{
    namespace ConfusionControl
    {

        public class UnitPartConfusionControl : UnitPart
        {
            [JsonProperty]
            private List<Buff> buffs = new List<Buff>();

            public void addBuff(Buff buff)
            {
                if (!buffs.Contains(buff))
                {
                    buffs.Add(buff);
                }
            }


            public void removeBuff(Buff buff)
            {
                buffs.Remove(buff);
            }


            public ConfusionState[] allowedConfusionStates()
            {
                if (buffs.Empty())
                {
                    return new ConfusionState[] { ConfusionState.ActNormally, ConfusionState.DoNothing, ConfusionState.AttackNearest, ConfusionState.SelfHarm };
                }

                return buffs.Last().Get<ControlConfusionBuff>().allowed_states;
            }

        }

        [ComponentName("BuffMechanics/Confusion Control")]
        [AllowedOn(typeof(BlueprintUnitFact))]
        [AllowMultipleComponents]
        public class ControlConfusionBuff : BuffLogic
        {
            public override void OnTurnOn()
            {
                this.Owner.Ensure<UnitPartConfusionControl>().addBuff(this.Buff);
            }


            public override void OnTurnOff()
            {
                this.Owner.Ensure<UnitPartConfusionControl>().removeBuff(this.Buff);
            }

            public ConfusionState[] allowed_states; 
        }


        [Harmony12.HarmonyPatch(typeof(UnitConfusionController))]
        [Harmony12.HarmonyPatch("TickOnUnit", Harmony12.MethodType.Normal)]
        class UnitConfusionController__TickOnUnit__Patch
        {
            static bool Prefix(UnitConfusionController __instance, UnitEntityData unit)
            {
                Main.TraceLog();
                var allowed_states = new ConfusionState[0];
                if (unit.Descriptor.State.HasCondition(UnitCondition.AttackNearest))
                {
                    allowed_states = new ConfusionState[] { ConfusionState.AttackNearest };
                }
                else
                {
                    allowed_states = unit.Ensure<UnitPartConfusionControl>().allowedConfusionStates();
                }
                if (unit.Descriptor.State.HasCondition(UnitCondition.Confusion) || unit.Descriptor.State.HasCondition(UnitCondition.AttackNearest))
                {
                    var tr = Harmony12.Traverse.Create<UnitConfusionController>();
                    UnitPartConfusion part = unit.Ensure<UnitPartConfusion>();
                    bool flag = !unit.CombatState.HasCooldownForCommand(UnitCommand.CommandType.Standard);
                    if (Game.Instance.TimeController.GameTime - part.RoundStartTime > tr.Field("RoundDuration").GetValue<TimeSpan>() && flag)
                    {
                        do
                        {
                            RuleRollDice ruleRollDice = Rulebook.Trigger<RuleRollDice>(new RuleRollDice(unit, new DiceFormula(1, DiceType.D100)));
                            int num = ruleRollDice.Result;
                            part.State = num >= 26 ? (num >= 51 ? (num >= 76 ? ConfusionState.AttackNearest : ConfusionState.SelfHarm) : ConfusionState.DoNothing) : ConfusionState.ActNormally;
                        } while (!allowed_states.Contains(part.State));
                        if (part.State == ConfusionState.ActNormally)
                            part.ReleaseControl();
                        else
                            part.RetainControl();
                        part.RoundStartTime = Game.Instance.TimeController.GameTime;
                        part.Cmd?.Interrupt();
                        part.Cmd = (UnitCommand)null;
                    }
                    if (part.Cmd != null || !unit.Descriptor.State.CanAct || part.State == ConfusionState.ActNormally)
                        return false;
                    if (flag)
                    {
                        switch (part.State)
                        {
                            case ConfusionState.DoNothing:
                                part.Cmd = tr.Method("DoNothing", part).GetValue<UnitCommand>();
                                break;
                            case ConfusionState.SelfHarm:
                                part.Cmd = tr.Method("SelfHarm", part).GetValue<UnitCommand>();
                                break;
                            case ConfusionState.AttackNearest:
                                part.Cmd = tr.Method("AttackNearest", part).GetValue<UnitCommand>();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                        part.Cmd = tr.Method("DoNothing", part).GetValue<UnitCommand>();
                    if (part.Cmd == null)
                        return false;
                    part.Owner.Unit.Commands.Run(part.Cmd);
                }
                else
                    unit.Remove<UnitPartConfusion>();

                return false;
            }
        }
    }



}
