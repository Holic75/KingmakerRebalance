using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.PrerequisiteMechanics
{

    [AllowMultipleComponents]
    public class PrerequsiteOrAlternative : Prerequisite
    {        
        public Prerequisite base_prerequsite, alternative_prerequsite;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            return this.base_prerequsite.Check(selectionState, unit, state) || this.alternative_prerequsite.Check(selectionState, unit, state);
        }

        public override string GetUIText()
        {
            return base_prerequsite.GetUIText() + " or " + this.alternative_prerequsite.GetUIText();
        }
    }
}
