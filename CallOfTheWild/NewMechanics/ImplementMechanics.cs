using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.ImplementMechanics
{
    class UnitPartImplements : UnitPart
    {
        [JsonProperty]
        private Dictionary<SpellSchool, int> invested_focus = new Dictionary<SpellSchool, int>();
        [JsonProperty]
        private bool focus_locked;

        public int getInvestedFocusAmount(SpellSchool school)
        {
            return invested_focus[school];
        }

        public int getInvestedFocusAmount(SpellSchool[] schools)
        {
            int amount = 0;
            if (!isLocked())
            {
                return amount;
            }

            foreach (var s in schools)
            {
                amount += invested_focus[s];
            }
            return amount;
        }

        public void investFocus(SpellSchool school, int amount = 1)
        {
            invested_focus[school] += amount;
        }

        public void reset()
        {
            invested_focus = new Dictionary<SpellSchool, int>();
        }

        public void lockFocus()
        {
            focus_locked = true;
        }

        public void unlockFocus()
        {
            focus_locked = false;
        }

        public bool isLocked()
        {
            return focus_locked;
        }
    }

    public class HasInvestedFocusAmount : ContextCondition
    {
        public SpellSchool[] schools;
        public int amount;

        protected override string GetConditionCaption()
        {
            return "";
        }

        protected override bool CheckCondition()
        {
            var unit_part = this.Context.MaybeCaster?.Get<UnitPartImplements>();
            if (unit_part == null)
            {
                return false;
            }

            return unit_part.getInvestedFocusAmount(schools) >= amount;
        }
    }


    public class ContextActionInvestFocus : ContextAction
    {
        public BlueprintAbilityResource resource;
        public ContextValue amount = 1;
        public SpellSchool school;

        public override string GetCaption() => "Invest focus";

        public override void RunAction()
        {
            var unit = this.Target?.Unit;
            if (unit == null)
            {
                UberDebug.LogError("Target is missing");
                return;
            }

            int val = amount.Calculate(this.Context);
            if (resource != null)
            {
                unit.Descriptor.Resources.Restore(resource, val);
            }

            var unit_part_focus = unit.Get<UnitPartImplements>();
            if (unit_part_focus == null || unit_part_focus.isLocked())
            {
                return;
            }

            unit_part_focus.investFocus(school, val);
        }
    }


    public class RestrictionInvestedFocus : ActivatableAbilityRestriction
    {
        public SpellSchool school;
        public int amount = 1;

        public override bool IsAvailable()
        {
            var unit_part = this.Owner.Get<UnitPartImplements>();
            if (unit_part == null)
            {
                return false;
            }

            return unit_part.getInvestedFocusAmount(school) >= amount;
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityCasterFocusLocked : BlueprintComponent, IAbilityCasterChecker
    {
        public bool not;
        public bool CorrectCaster(UnitEntityData caster)
        {
            var unit_part = caster?.Get<UnitPartImplements>();
            if (unit_part == null)
            {
                return false;
            }
            return unit_part.isLocked() != not;
        }

        public string GetReason()
        {
            return not ? "You have already invested your focus" : "You need to invest focus into your implements";
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityCasterInvestedFocus : BlueprintComponent, IAbilityCasterChecker
    {
        public SpellSchool school;
        public int amount = 1;

        public bool CorrectCaster(UnitEntityData caster)
        {
            var unit_part = caster?.Get<UnitPartImplements>();
            if (unit_part == null)
            {
                return false;
            }
            return unit_part.getInvestedFocusAmount(school) >= amount;
        }

        public string GetReason()
        {
            return "Invest more focus";
        }
    }


    class InvestedImplementFocusAmountProperty : PropertyValueGetter
    {
        public SpellSchool[] schools;
        public static BlueprintUnitProperty createProperty(string name, string guid, params SpellSchool[] schools)
        {
            var p = Helpers.Create<BlueprintUnitProperty>();
            p.name = name;
            Main.library.AddAsset(p, guid);
            p.SetComponents(Helpers.Create<InvestedImplementFocusAmountProperty>(a => a.schools = schools));
            return p;
        }

        public override int GetInt(UnitEntityData unit)
        {
            var unit_part = unit.Get<UnitPartImplements>();
            if (unit_part == null)
            {
                return 0;
            }

            return unit_part.getInvestedFocusAmount(schools);
        }
    }
}
