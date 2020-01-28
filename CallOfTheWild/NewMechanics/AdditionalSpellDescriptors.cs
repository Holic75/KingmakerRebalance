using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.AdditionalSpellDescriptors
{
    [Flags]
    public enum SpellDescriptor : long
    {
        HolyVindicatorShield = 0x0004000000000000,
        Wind = 0x0008000000000000
    }
}
