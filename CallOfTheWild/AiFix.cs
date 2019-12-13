using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class AiFix
    {
        static LibraryScriptableObject library => Main.library;

        internal static void load()
        {
            var support_caster_consideration = library.Get<TargetClassConsideration>("60bfe8583de6d3f45818c641b459386e");
            var arcane_caster_consideration = library.Get<TargetClassConsideration>("a10afa800940a9143bda381d487fcd5c");
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");

            support_caster_consideration.FirstPriorityClasses = support_caster_consideration.FirstPriorityClasses.AddToArray(Skald.skald_class);
            support_caster_consideration.SecondPriorityClasses = support_caster_consideration.SecondPriorityClasses.AddToArray(Shaman.shaman_class, Witch.witch_class, druid);

            arcane_caster_consideration.FirstPriorityClasses = support_caster_consideration.FirstPriorityClasses.AddToArray(Shaman.shaman_class, Witch.witch_class, druid);
            arcane_caster_consideration.SecondPriorityClasses = support_caster_consideration.SecondPriorityClasses.AddToArray(Skald.skald_class);


            //update channel energy (since it has variants now)
            var channel_guid = new string[]{ "18e219ba6c2becb45883ff9643e1f4ea",
                                         "dc13644c3dc3bb244a89e3237bc3ad94",
                                        "cb9b4be1466401c468e9b157b4973330",
                                        "2fc0766638579a94ba19779197f509ad",
                                        "622292b9f2e9e1d4c8bac7bba485cbe2",
                                        "48c10b81b79617f4ca709e6e76535f15",
                                        "41041e39dfbb80046ae4262dfd64b677",
                                        "bcb4e562747371e4884d1f12fbfdbfbb",
                                         "4abca3d5053b0094094aebfcb1d6cbfd"};

            foreach (var c_guid in channel_guid)
            {
                BlueprintAiCastSpell action = library.Get<BlueprintAiCastSpell>(c_guid);
                action.Variant = action.Ability;
                action.Ability = action.Ability.Parent;
            }
        }
    }
}
