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


    [AllowedOn(typeof(BlueprintSpellbook))]
    public class CompanionSpellbook : BlueprintComponent
    {
        public BlueprintSpellbook spellbook;
    }


    //add automatic update of companion spellbooks
    [Harmony12.HarmonyPatch(typeof(Spellbook))]
    [Harmony12.HarmonyPatch("AddCasterLevel", Harmony12.MethodType.Normal)]
    class Spellbook__AddCasterLevel__Patch
    {
        static void Postfix(Spellbook __instance)
        {
            foreach (var cs in __instance.Blueprint.GetComponents<CompanionSpellbook>())
            {
                __instance.Owner.DemandSpellbook(cs.spellbook).AddCasterLevel();
            }
        }
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
