using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.BuffMechanics
{
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
