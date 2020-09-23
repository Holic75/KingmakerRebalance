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
        Earth = 0x0004000000000000, //from earth blast        
         Air = 0x0008000000000000, //should be added to all air blasts (?)
        Water = 0x0010000000000000, //from kineticist water blast
                                    //cold blast have wrong descriptor 20000000000004 - i.e cold + something ?, should be just cold
                                    //magma blast is missing earth descriptor (only fire)
                                    //sandstorm only has earth (missing air)
                                    //mud blast has descriptor of 1000000000000000 while it should be earth + water
                                    //blizzard blast has 40000000000004 - while it should be cold + air
                                    //steam is only fire - should be water + fire
                                    //metal is correct
                                    //thunderstorm only has electricity - probably correct
                                    //plasma only has fire - should be air +fire
        Shadow20 = 0x0200000000000000,
        Shadow60 = 0x0400000000000000,
        Shadow = Shadow20 | Shadow60,

        LanguageDependent = 0x2000000000000000,
        HolyVindicatorShield = 0x4000000000000000,
    }



    static class UIUtilityTexts_GetSpellDescriptor_Patch
    {
        static void Postfix(SpellDescriptor spellDescriptor, ref string __result)
        {
            if (spellDescriptor.Intersects((SpellDescriptor)ExtraSpellDescriptor.Earth))
            {
                __result = maybeAddSeparator(__result) + "Earth";
            }
            if (spellDescriptor.Intersects((SpellDescriptor)ExtraSpellDescriptor.Water))
            {
                __result = maybeAddSeparator(__result) + "Water";
            }
            if (spellDescriptor.Intersects((SpellDescriptor)ExtraSpellDescriptor.Air))
            {
                __result = maybeAddSeparator(__result) + "Air";
            }
            if (spellDescriptor.Intersects((SpellDescriptor)ExtraSpellDescriptor.Shadow))
            {
                __result = maybeAddSeparator(__result) + "Shadow";
            }
            if (spellDescriptor.Intersects((SpellDescriptor)ExtraSpellDescriptor.LanguageDependent))
            {
                __result = maybeAddSeparator(__result) + "Language-Dependent";
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
