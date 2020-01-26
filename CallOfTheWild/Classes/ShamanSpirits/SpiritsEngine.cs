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
    public partial class SpiritsEngine
    {
        static LibraryScriptableObject library => Main.library;
        HexEngine hex_engine;

        public SpiritsEngine(HexEngine associated_hex_engine)
        {
            hex_engine = associated_hex_engine;
        }
    }
}
