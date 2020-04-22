using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SizeMechanics
{
    public class UnitPartSizeOverride : AdditiveUnitPart
    {
        public new void addBuff(Fact buff)
        {
            base.addBuff(buff);
            this.Owner.Ensure<UnitPartSizeModifier>().Remove(null);
        }


        public new void removeBuff(Fact buff)
        {
            base.removeBuff(buff);
            this.Owner.Ensure<UnitPartSizeModifier>().Remove(null);
        }


        public Size getSize()
        {
            if (buffs.Empty())
            {
                return this.Owner.OriginalSize;
            }
            else
            {                
                return buffs.Last().Blueprint.GetComponent<PermanentSizeOverride>().size;
            }
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class PermanentSizeOverride : OwnedGameLogicComponent<UnitDescriptor>
    {
        public Size size;
        public override void OnFactActivate()
        {
            //Main.logger.Log("Activated: " + this.Fact.Name);
            this.Owner.Ensure<UnitPartSizeOverride>().addBuff(this.Fact);
            //this.Owner.State.Size = this.Owner.Ensure<UnitPartSizeOverride>().getSize();
        }

        /*public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartSizeOverride>().addBuff(this.Fact);
            //this.Owner.State.Size = this.Owner.Ensure<UnitPartSizeOverride>().getSize();
        }*/


        /*public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartSizeOverride>().removeBuff(this.Fact);
            //this.Owner.State.Size = this.Owner.Ensure<UnitPartSizeOverride>().getSize();
        }*/

        public override void OnFactDeactivate()
        {
            //Main.logger.Log("Deactivated: " + this.Fact.Name);
            this.Owner.Ensure<UnitPartSizeOverride>().removeBuff(this.Fact);
            //this.Owner.State.Size = this.Owner.Ensure<UnitPartSizeOverride>().getSize();
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitPartSizeModifier), "UpdateSize")]
    class UnitPartSizeModifier_UnitPartSizeModifier_Patch
    {
        static void Postfix(UnitPartSizeModifier __instance, List<Fact> ___m_SizeChangeFacts)
        {
            Fact fact = ___m_SizeChangeFacts.LastItem<Fact>();
            if (fact == null)
            {
                __instance.Owner.State.Size = __instance.Owner.Ensure<UnitPartSizeOverride>().getSize();
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(ChangeUnitSize), "GetSize")]
    class ChangeUnitSizer_GetSize_Patch
    {
        static void Postfix(ChangeUnitSize __instance, ref Size __result)
        {
            var change_type = Helpers.GetField<int>(__instance, "m_Type");
            if (change_type == 0)
            {
                __result = __instance.Owner.Ensure<UnitPartSizeOverride>().getSize().Shift(__instance.SizeDelta);
            }
        }
    }


}
