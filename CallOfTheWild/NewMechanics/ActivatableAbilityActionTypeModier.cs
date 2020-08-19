using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ActivatableAbilityActionTypeModierMechanics
{


    public class UnitPartActivatableAbilityActionTypeModifier : UnitPart
    {
        class Entry
        {
            public readonly Fact buff;
            public readonly ActivatableAbilityGroup group;
            public readonly UnitCommand.CommandType action;

            public Entry(Fact new_buff, ActivatableAbilityGroup new_group, UnitCommand.CommandType new_action)
            {
                buff = new_buff;
                group = new_group;
                action = new_action;
            }
        }
        [JsonProperty]
        private List<Entry> entries = new List<Entry>();

        public void addEntry(Fact new_buff, ActivatableAbilityGroup new_group, UnitCommand.CommandType new_action)
        {
            if (!entries.Any(e => e.buff == new_buff))
            {
                entries.Add(new Entry(new_buff, new_group, new_action));
            }
        }


        public void removeBuff(Fact buff)
        {
            entries.RemoveAll(e => e.buff == buff);
        }


        public UnitCommand.CommandType getCommandType(BlueprintActivatableAbility activatable_ability)
        {
            if (activatable_ability.Group == ActivatableAbilityGroup.None)
            {
                return activatable_ability.ActivateWithUnitCommandType;
            }

            var entry = entries.Find(e => e.group == activatable_ability.Group);
            if (entry == null)
            {
                return activatable_ability.ActivateWithUnitCommandType;
            }

            return entry.action;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ModifyActivatableAbilityGroupActionType : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public ActivatableAbilityGroup group;
        public UnitCommand.CommandType action;

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartActivatableAbilityActionTypeModifier>().addEntry(this.Fact, group, action);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartActivatableAbilityActionTypeModifier>().removeBuff(this.Fact);
        }
    }



    [Harmony12.HarmonyPatch(typeof(UnitActivateAbility))]
    [Harmony12.HarmonyPatch("GetCommandType", Harmony12.MethodType.Normal)]
    class UnitActivateAbility__GetCommandType__Patch
    {
        static bool Prefix(ActivatableAbility ability, ref UnitCommand.CommandType __result)
        {
            Main.TraceLog();
            if (ability.Blueprint.Group != ActivatableAbilityGroup.BardicPerformance)
            {
                __result = ability.Owner.Ensure< UnitPartActivatableAbilityActionTypeModifier>().getCommandType(ability.Blueprint);
            }
            else if ((bool)ability.Owner.State.Features.QuickenPerformance2 || ability.Blueprint.ActivateWithUnitCommandType == UnitCommand.CommandType.Swift)
            {
                __result = !(bool)ability.Owner.State.Features.SingingSteel ? UnitCommand.CommandType.Swift : UnitCommand.CommandType.Free;
            }
            else if ((bool)ability.Owner.State.Features.QuickenPerformance1 || ability.Blueprint.ActivateWithUnitCommandType == UnitCommand.CommandType.Move)
            {
                __result = !(bool)ability.Owner.State.Features.SingingSteel ? UnitCommand.CommandType.Move : UnitCommand.CommandType.Swift;
            }
            else if (ability.Blueprint.ActivateWithUnitCommandType == UnitCommand.CommandType.Standard)
            {
                __result = !(bool)ability.Owner.State.Features.SingingSteel ? UnitCommand.CommandType.Standard : UnitCommand.CommandType.Move;
            }
            return false;
        }
    }
}
