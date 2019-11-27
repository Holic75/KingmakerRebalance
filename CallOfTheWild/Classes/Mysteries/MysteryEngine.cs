using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public partial class MysteryEngine
    {
        static LibraryScriptableObject library => Main.library;
        BlueprintCharacterClass[] classes;
        StatType stat;

        public MysteryEngine(BlueprintCharacterClass[] mystery_classes, StatType scaling_stat)
        {
            stat = scaling_stat;
            classes = mystery_classes;
        }
    }
}
