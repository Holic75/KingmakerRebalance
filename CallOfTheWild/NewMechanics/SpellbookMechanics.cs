using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SpellbookMechanics
{

    [AllowedOn(typeof(BlueprintSpellbook))]
    public class NoSpellsPerDaySacaling : BlueprintComponent
    {

    }

    //fix for spellbooks with fixed number of spells per day iindependent of casting stat
    [Harmony12.HarmonyPatch(typeof(Spellbook))]
    [Harmony12.HarmonyPatch("GetSpellsPerDay", Harmony12.MethodType.Normal)]
    class Spellbook__GetSpellsPerDay__Patch
    {
        static bool Prefix(Spellbook __instance, int spellLevel, ref int __result)
        {
            if (__instance.Blueprint.GetComponent<NoSpellsPerDaySacaling>() == null)
            {
                return true;
            }

            ModifiableValueAttributeStat stat = __instance.Owner.Stats.GetStat(__instance.Blueprint.CastingAttribute) as ModifiableValueAttributeStat;
            if (stat == null || stat.ModifiedValue < 10 + spellLevel)
            {
                __result = 0;
                return false;
            }

            int? count = __instance.Blueprint.SpellsPerDay.GetCount(__instance.CasterLevel, spellLevel);
            if (!count.HasValue)
            {
                __result =  0;
            }
            else
            {
                __result = count.Value;
            }
            return false;
        }
    }
}
