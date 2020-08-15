using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.BuffMechanics
{
    public class UnitPartStoreBuff : AdditiveUnitPart
    {
        public void removeAllBuffsByBlueprint(BlueprintBuff buff)
        {
            var found_buffs =  buffs.FindAll(b => b.Blueprint == buff).OfType<Buff>().ToArray();
            buffs.RemoveAll(b => b.Blueprint == buff);
            foreach (var b in found_buffs)
            {
                b.Remove();
            }
            
        }
    }


    public class StoreBuff : BuffLogic
    {
        public override void OnFactActivate()
        {
            this.Buff.Context.MaybeCaster?.Ensure<UnitPartStoreBuff>().addBuff(this.Buff);
        }

        public override void OnFactDeactivate()
        {
            this.Buff.Context.MaybeCaster?.Get<UnitPartStoreBuff>()?.removeBuff(this.Buff);
        }
    }




    public class RemoveStoredBuffs : ContextAction
    {
        public BlueprintBuff buff;
        public override string GetCaption()
        {
            return "Remove unique buff";
        }


        public override void RunAction()
        {
            var part_store_buff = this.Target.Unit?.Get<UnitPartStoreBuff>();

            if (part_store_buff == null)
            {
                return;
            }

            part_store_buff.removeAllBuffsByBlueprint(buff);
        }
    }



    public class RemoveUniqueBuff : ContextAction
    {
        public BlueprintBuff buff;
        public override string GetCaption()
        {
            return "Remove unique buff";
        }


        public override void RunAction()
        {
            var unit_part_unique_buff = this.Target.Unit?.Get<UnitPartUniqueBuffs>();
            if (unit_part_unique_buff == null)
            {
                return;
            }
            var buff_to_remove = unit_part_unique_buff.Buffs.Find(b => b.Blueprint == buff);
            if (buff_to_remove != null)
            {
                unit_part_unique_buff.RemoveBuff(buff_to_remove);
                buff_to_remove.Remove();
            }
        }
    }
}
