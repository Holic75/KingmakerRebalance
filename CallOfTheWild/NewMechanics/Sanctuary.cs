using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SanctuaryMechanics
{

    public class UnitPartSanctuary : UnitPart
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


        public bool canBeAttackedBy(UnitEntityData unit)
        {
            if (buffs.Empty())
            {
                return true;
            }

            foreach (var b in buffs)
            {
                if (b.Get<Sanctuary>().canBeAttackedBy(unit))
                {
                    return true;
                }
            }

            return false;
        }

    }

    public enum OffensiveActionEffect
    {
        REMOVE_FROM_OWNER,
        REMOVE_FROM_TARGET
    }

    [ComponentName("Sanctuary")]
    [AllowedOn(typeof(BlueprintBuff))]
    public class Sanctuary : BuffLogic, IUnitMakeOffensiveActionHandler, IUnitSubscriber
    {
        private List<UnitEntityData> can_not_attack = new List<UnitEntityData>();

        private List<UnitEntityData> can_attack = new List<UnitEntityData>();
        public SavingThrowType save_type;

        public OffensiveActionEffect offensive_action_effect;

        public override void  OnTurnOn()
        {
            this.Owner.Ensure<UnitPartSanctuary>().addBuff(this.Buff);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartSanctuary>().removeBuff(this.Buff);
        }

        public bool canBeAttackedBy(UnitEntityData unit)
        {
            if (can_attack.Contains(unit))
            {
                return true;
            }
            if (can_not_attack.Contains(unit))
            {
                return false;
            }
            var spell_descriptor = this.Buff.Blueprint.GetComponent<SpellDescriptorComponent>();
            if (spell_descriptor != null
                && (UnitDescriptionHelper.GetDescription(unit.Blueprint).Immunities.SpellDescriptorImmunity.Value & spell_descriptor.Descriptor.Value) != 0)
            {
                Common.AddBattleLogMessage($"{unit.CharacterName} is immune to {this.Fact.Name} of {Owner.CharacterName}");
                can_not_attack.Add(unit);
                return false;
            }
            RuleSavingThrow ruleSavingThrow = this.Context.TriggerRule<RuleSavingThrow>(new RuleSavingThrow(unit, save_type, this.Context.Params.DC));
            if (ruleSavingThrow.IsPassed)
            {
                can_attack.Add(unit);
                Common.AddBattleLogMessage($"{unit.CharacterName} successfuly overcomes {this.Fact.Name} of {Owner.CharacterName}");
                return true;
            }
            else
            {
                Common.AddBattleLogMessage($"{unit.CharacterName} fails to overcome {this.Fact.Name} of {Owner.CharacterName}");
                can_not_attack.Add(unit);
                return false;
            }
        }

        public void HandleUnitMakeOffensiveAction(UnitEntityData target)
        {
            if (offensive_action_effect == OffensiveActionEffect.REMOVE_FROM_OWNER)
            {
                this.Buff.Remove();
            }
            else if (offensive_action_effect == OffensiveActionEffect.REMOVE_FROM_TARGET && !can_attack.Contains(target))
            {
                can_attack.Add(target);
                can_not_attack.Remove(target);
                Common.AddBattleLogMessage($"{Owner.CharacterName} invalidates {this.Fact.Name} against {target.CharacterName}");
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitCommand))]
    [Harmony12.HarmonyPatch("CommandTargetUntargetable", Harmony12.MethodType.Normal)]
    class UnitCommand__CommandTargetUntargetable__Patch
    {
        static void Postfix(EntityDataBase sourceEntity, UnitEntityData targetUnit, RulebookEvent evt, ref bool __result)
        {
            Main.TraceLog();
            if (__result)
            {
                return;
            }

            UnitEntityData unit = sourceEntity as UnitEntityData;
            if (unit == null || unit.IsAlly(targetUnit) || evt != null)
            {
                return;
            }

            __result = !targetUnit.Ensure<UnitPartSanctuary>().canBeAttackedBy(unit);
        }
    }
}
