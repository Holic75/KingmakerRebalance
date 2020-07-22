using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
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



    [ComponentName("Add feature on class level range")]
    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddFeatureOnClassLevelRange : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
    {
        public int min_level = 0;
        public int max_level = 100;
        public BlueprintFeature Feature;
        public BlueprintCharacterClass[] classes;
        public BlueprintArchetype[] archetypes = new BlueprintArchetype[0];
        public int[] class_bonuses = new int[0];
        [JsonProperty]
        private Fact m_AppliedFact;

        public override void OnFactActivate()
        {
            this.Apply();
        }

        public override void OnFactDeactivate()
        {
            this.Owner.RemoveFact(this.m_AppliedFact);
        }

        public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
        {
            this.Apply();
        }

        private void Apply()
        {
            if (this.IsFeatureShouldBeApplied())
            {
                if (this.m_AppliedFact != null)
                    return;
                this.m_AppliedFact = this.Owner.AddFact((BlueprintUnitFact)this.Feature, (MechanicsContext)null, (FeatureParam)null);
            }
            else
            {
                if (this.m_AppliedFact == null)
                    return;
                this.Owner.RemoveFact(this.m_AppliedFact);
                this.m_AppliedFact = (Fact)null;
            }
        }

        private bool IsFeatureShouldBeApplied()
        {
            int class_level = ReplaceCasterLevelOfAbility.CalculateClassLevel(this.classes[0], this.classes.Skip(1).ToArray(), this.Owner, this.archetypes);
            if (!class_bonuses.Empty())
            {
                for (int i = 0; i < class_bonuses.Length; i++)
                {
                    if (this.Owner.Progression.Classes.Any(cd => cd.CharacterClass == classes[i]))
                    {
                        class_level += class_bonuses[i];
                    }
                }
            }

            if (class_level < min_level || class_level > max_level)
            {
                return false;
            }

            return true;
        }

        public override void PostLoad()
        {
            base.PostLoad();
            bool flag = this.m_AppliedFact != null && !this.Owner.HasFact(this.m_AppliedFact);
            if (flag)
            {
                this.m_AppliedFact.Dispose();
                this.m_AppliedFact = (Fact)null;
            }
            if (!flag || !((IList<BlueprintFeatureBase>)BlueprintRoot.Instance.PlayerUpgradeActions.AllowedForRestoreFeatures).HasItem<BlueprintFeatureBase>((BlueprintFeatureBase)this.Feature))
                return;
            this.Apply();
        }

    }
}
