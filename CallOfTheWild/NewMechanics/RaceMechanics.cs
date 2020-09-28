using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.RaceMechanics
{
    public class PrerequisiteRace : Prerequisite
    {
        public BlueprintRace race;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            return race == unit.Progression.Race;
        }

        public override string GetUIText()
        {
            return race.Name;
        }
    }
}
