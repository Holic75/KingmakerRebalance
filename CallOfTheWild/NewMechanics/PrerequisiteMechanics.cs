using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root.Strings;
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
    public class PrerequisiteMatchingParametrizedFeatures : Prerequisite
    {
        public BlueprintParametrizedFeature base_feature;
        public BlueprintParametrizedFeature dependent_feature;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            if (selectionState != (FeatureSelectionState)null && selectionState.IsSelectedInChildren((IFeatureSelectionItem)this.base_feature))
                return false;
            return unit.Progression.Features.Enumerable.Where(p => p.Blueprint == this.base_feature).Any(this.CheckFeature);
        }

        public bool CheckFeature(Kingmaker.UnitLogic.Feature feature)
        {
            return feature.Owner.Progression.Features.Enumerable.Where(p => p.Blueprint == this.dependent_feature).Any(f => f.Param == feature.Param);
        }

        public override string GetUIText()
        {
            return base_feature.Name + " with " + dependent_feature.Name + ".";
        }
    }

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


    [AllowMultipleComponents]
    public class CompoundPrerequisite : Prerequisite
    {
        public Prerequisite prerequisite1;
        public Prerequisite prerequisite2;
        

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            return this.prerequisite1.Check(selectionState, unit, state) && this.prerequisite2.Check(selectionState, unit, state);
        }

        public override string GetUIText()
        {
            return prerequisite1.GetUIText() + " and " + prerequisite2.GetUIText();
        }
    }


    [AllowMultipleComponents]
    public class CompoundPrerequisites : Prerequisite
    {
        public Prerequisite[] prerequisites;
        public bool any = false;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            foreach (var p in prerequisites)
            {
                if (!p.Check(selectionState, unit, state))
                {
                    if (!any)
                    {
                        return false;
                    }
                }
                else if (any)
                {
                    return true;
                }
            }
            return !any;
        }

        public override string GetUIText()
        {
            var text = $"Meets {(any ? "one of " :"")}the following requirements::";
            for (int i = 0; i < prerequisites.Length; i++)
            {
                text += "\n" + prerequisites[i].GetUIText();
            }
            return text;
        }
    }

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

    [AllowMultipleComponents]
    public class PrerequisiteNoFeatures : Prerequisite
    {
        public BlueprintFeature[] Features;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            foreach (var f in Features)
            {
                if (unit.Logic.HasFact(f))
                {
                    return false;
                }
                if (unit.Progression.Features.HasFact(f))
                {
                    return false;
                }
            }
            return true;
        }

        public override string GetUIText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("{0}:\n", (object)UIStrings.Instance.Tooltips.NoFeature));
            for (int index = 0; index < this.Features.Length; ++index)
            {
                stringBuilder.Append(this.Features[index].Name);
                if (index < this.Features.Length - 1)
                    stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }
    }
}
