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

        public void addBuff(Fact buff)
        {
            if (!buffs.Contains(buff))
            {
                buffs.Add(buff);
            }
        }

        public void removeBuff(Fact buff)
        {
            buffs.Remove(buff);
        }
    }
}
