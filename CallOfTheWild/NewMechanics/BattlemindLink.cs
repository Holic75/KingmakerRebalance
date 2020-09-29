using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.BattlemindLink
{
    class UnitPartBattlemindLink: AdditiveUnitPart
    {
        public bool isLinkedTo(UnitEntityData unit)
        {
            bool val = false;

            foreach (var b in buffs)
            {
                b.CallComponents<BattlemindLinkBuff>(c => val = c.isLinkedTo(unit));
                if (val)
                {
                    return true;
                }
            }

            return false;
        }
    }


    class UnitPartBattlemindLinkMark : AdditiveUnitPart
    {
        public bool isLinkedTo(UnitEntityData unit)
        {
            bool val = false;

            foreach (var b in buffs)
            {
                b.CallComponents<BattlemindLinkBuff>(c => val = c.isLinkedTo(unit));
                if (val)
                {
                    return true;
                }
            }

            return false;
        }
    }





    class BattlemindLinkBuff: BuffLogic
    {
        public bool to_caster;

        public override void OnFactActivate()
        {
            if (to_caster)
            {
                this.Fact.MaybeContext.MaybeCaster?.Ensure<UnitPartBattlemindLink>().addBuff(this.Fact);
            }
            else
            {
                this.Fact.MaybeContext.MainTarget.Unit?.Ensure<UnitPartBattlemindLink>().addBuff(this.Fact);
            }
        }


        public override void OnFactDeactivate()
        {
            if (to_caster)
            {
                this.Fact.MaybeContext.MaybeCaster?.Ensure<UnitPartBattlemindLink>().removeBuff(this.Fact);
            }
            else
            {
                this.Fact.MaybeContext.MainTarget.Unit?.Ensure<UnitPartBattlemindLink>().removeBuff(this.Fact);
            }
        }


        public bool isLinkedTo(UnitEntityData unit)
        {
            if (to_caster)
            {
                return unit == this.Context.MainTarget.Unit;
            }
            else
            {
                return unit == this.Context.MaybeCaster;
            }
        }
    }
}
