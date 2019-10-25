using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class SpellDuplicates
    {
        static LibraryScriptableObject library => Main.library;
        static Dictionary<string, List<BlueprintAbility>> duplicate_spells = new Dictionary<string, List<BlueprintAbility>>();

        public static BlueprintAbility addDuplicateSpell(string prototype_id, string name, string guid = "")
        {
            return addDuplicateSpell(library.Get<BlueprintAbility>(prototype_id), name, guid);
        }

        public static BlueprintAbility addDuplicateSpell(BlueprintAbility prototype, string name, string guid = "")
        {
            BlueprintAbility new_spell = library.CopyAndAdd<BlueprintAbility>(prototype, name, guid);
            if (!duplicate_spells.ContainsKey(prototype.AssetGuid))
            {
                duplicate_spells.Add(prototype.AssetGuid, new BlueprintAbility[] { prototype }.ToList());
            }
            var list = duplicate_spells[prototype.AssetGuid];
            list.Add(new_spell);

            for (int i = 1; i < list.Count - 1; i++)
            {
                duplicate_spells[list[i].AssetGuid].Add(new_spell);
            }

            var new_list = new List<BlueprintAbility>();
            new_list.Add(new_spell);
            for (int i = 0; i < list.Count - 1; i++)
            {
                new_list.Add(list[i]);
            }
            duplicate_spells.Add(new_spell.AssetGuid, new_list);

            return new_spell;
        }


        public static bool isDuplicate(BlueprintAbility original, BlueprintAbility duplicate)
        {
            if (original == null || duplicate == null)
            {
                return false;
            }
            return original == duplicate
                   || (duplicate_spells.ContainsKey(original.AssetGuid) && duplicate_spells[original.AssetGuid].Contains(duplicate));
        }

    }



    [Harmony12.HarmonyPatch(typeof(SpellSpecializationParametrized))]
    [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
    public class SpellSpecializationParametrized_OnEventAboutToTrigger_Patch
    {
        static bool Prefix(SpellSpecializationParametrized __instance, RuleCalculateAbilityParams evt)
        {
            var spell = (__instance.Fact as Feature)?.Param.Blueprint as BlueprintAbility;
            if (SpellDuplicates.isDuplicate(spell, evt.Spell) || SpellDuplicates.isDuplicate(spell, evt.Spell?.Parent))
            {
                evt.AddBonusCasterLevel(2);
            }

            return false;
        }
    }



}
