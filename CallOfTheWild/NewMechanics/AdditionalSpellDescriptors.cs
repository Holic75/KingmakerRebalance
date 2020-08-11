using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.AdditionalSpellDescriptors
{
    [Flags]
    public enum ExtraSpellDescriptor : long
    {
        HolyVindicatorShield = 0x0004000000000000,
        Wind = 0x0008000000000000,
        Water = 0x0010000000000000
    }



    static class UIUtilityTexts_GetSpellDescriptor_Patch
    {
        static void Postfix(SpellDescriptor spellDescriptor, ref string __result)
        {

            if (spellDescriptor.Intersects((SpellDescriptor)ExtraSpellDescriptor.Water))
            {

                __result = maybeAddSeparator(__result) + "Water";
            }
            if (spellDescriptor.Intersects((SpellDescriptor)ExtraSpellDescriptor.Wind))
            {
                __result = maybeAddSeparator(__result) + "Wind";
            }
        }


        static string maybeAddSeparator(string input)
        {
            if (input.Empty())
            {
                return input;
            }
            else
            {
                return input + ", ";
            }
        }
    }
}
