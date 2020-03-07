using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.LevelUpMechanics
{
    public class addSelection : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        [JsonProperty]
        bool applied;
        public BlueprintFeatureSelection selection;

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
        }

        public override void OnFactActivate()
        {
            try
            {
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {
                    int index = levelUp.State.Selections.Count<FeatureSelectionState>((Func<FeatureSelectionState, bool>)(s => s.Selection == selection));
                    FeatureSelectionState featureSelectionState = new FeatureSelectionState(null, null, selection, index, 0);
                    levelUp.State.Selections.Add(featureSelectionState);
                }
                applied = true;
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }
}
