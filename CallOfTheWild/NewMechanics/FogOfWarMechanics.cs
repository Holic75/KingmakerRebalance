using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.FogOfWarMechanics
{
    public class UnitPartFogOfWarRevealer : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class FogOfWarRevealer : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartFogOfWarRevealer>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartFogOfWarRevealer>().removeBuff(this.Fact);
        }
    }

    [Harmony12.HarmonyPatch(typeof(FogOfWarController))]
    [Harmony12.HarmonyPatch("Tick", Harmony12.MethodType.Normal)]
    public class MagusController_HandleUnitRunCommand_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var get_party_idx = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("Party"));

            codes[get_party_idx] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                    new Func<List<UnitEntityData>>(getPartyRevealars).Method
                                                                    );

            return codes.AsEnumerable();
        }


        internal static List<UnitEntityData> getPartyRevealars()
        {
            var party = Game.Instance.Player.Party.ToList();
            var revealers = Game.Instance.State.Units.Where(u => (u.Descriptor.Get<UnitPartFogOfWarRevealer>()?.active()).GetValueOrDefault());
            party.AddRange(revealers);

            return party;
        }
    }

}
