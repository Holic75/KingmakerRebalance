using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.EncumbranceMechanics
{
    public class UnitPartIgnoreEncumbrance : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitPartEncumbrance), "GetSpeedPenalty", typeof(UnitDescriptor), typeof(Encumbrance))]
    static class UnitPartEncumbrance_GetSpeedPenalty_Patch
    {
        static void Postfix(UnitDescriptor owner, Encumbrance encumbrance, ref int __result)
        {
            Main.TraceLog();
            if (__result < 0 && owner.Get<UnitPartIgnoreEncumbrance>()?.active() == true)
            {
                __result = 0;
            }
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class IgnoreEncumbrence : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public Feet visibility_limit;
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartIgnoreEncumbrance>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartIgnoreEncumbrance>().removeBuff(this.Fact);
        }
    }
}
