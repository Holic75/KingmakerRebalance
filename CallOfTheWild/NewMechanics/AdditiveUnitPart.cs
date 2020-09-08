using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class AdditiveUnitPart : UnitPart
    {
        [JsonProperty]
        protected List<Fact> buffs = new List<Fact>();

        public virtual void addBuff(Fact buff)
        {
            if (!buffs.Contains(buff))
            {
                buffs.Add(buff);
            }
        }

        public virtual void removeBuff(Fact buff)
        {
            buffs.Remove(buff);
        }



    }



    public class AdditiveUnitPartWithCheckLock : AdditiveUnitPart
    {
        Dictionary<Fact, bool> lock_map = new Dictionary<Fact, bool>();

        public override void addBuff(Fact buff)
        {
            if (!buffs.Contains(buff))
            {
                buffs.Add(buff);
                lock_map[buff] = false;
            }
        }

        public override void removeBuff(Fact buff)
        {
            buffs.Remove(buff);
            lock_map.Remove(buff);
        }


        protected bool check<T>(Fact buff, Predicate<T> pred) where T : BlueprintComponent
        {
            if (!buffs.Contains(buff))
            {
                return false;
            }
            if (lock_map[buff])
            {
                return false;
            }
            lock_map[buff] = true;

            bool res = false;
            buff.CallComponents<T>(c => res = pred(c));
            lock_map[buff] = false;
            return res;
        }
    }
}
