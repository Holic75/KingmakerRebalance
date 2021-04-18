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
using Kingmaker.UnitLogic.Mechanics.Components;
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
        private Dictionary<SpellSchool, int> invested_focus_bonus = new Dictionary<SpellSchool, int>();
        [JsonProperty]
        private bool focus_locked;

        public int getInvestedFocusAmount(params SpellSchool[] schools)
        {
            int amount = 0;

            foreach (var s in schools)
            {
                amount += invested_focus[s] + invested_focus_bonus[s];
            }
            return amount;
        }

        public void investFocus(SpellSchool school, int amount = 1)
        {
            invested_focus[school] += amount;
            invested_focus[SpellSchool.Universalist] -= amount;
        }

        public void addinvestedFocusBonus(SpellSchool school, int amount = 1)
        {
            invested_focus_bonus[school] += amount;
        }

        public void removeinvestedFocusBonus(SpellSchool school, int amount = 1)
        {
            invested_focus_bonus[school] -= amount;
        }

        public void reset()
        {
            foreach (SpellSchool school in Enum.GetValues(typeof(SpellSchool)))
            {
                invested_focus[school] = 0;
            }
        }

        public void init()
        {
            reset();
            foreach (SpellSchool school in Enum.GetValues(typeof(SpellSchool)))
            {
                invested_focus_bonus[school] = 0;
            }
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

    public class ContextConditionInvestedFocusAmount : ContextCondition
    {
        public SpellSchool[] schools;
        public int amount;
        public bool locked_focus = true;

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


            if (locked_focus && !unit_part.isLocked())
            {
                return false;
            }

            foreach (var s in schools)
            {
                if (unit_part.getInvestedFocusAmount(s) < amount)
                {
                    return false;
                }
            }

            return true;
        }
    }


    public class FocusLocked : ContextCondition
    {
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

            return unit_part.isLocked();
        }
    }


    public class ContextActionLockFocus : ContextAction
    {
        public override string GetCaption() => "Lock focus";

        public override void RunAction()
        {
            var unit = this.Target?.Unit;
            if (unit == null)
            {
                UberDebug.LogError("Target is missing");
                return;
            }

            var unit_part_focus = unit.Get<UnitPartImplements>();
            if (unit_part_focus == null || unit_part_focus.isLocked())
            {
                return;
            }

            unit_part_focus.lockFocus();
        }
    }


    public class ContextActionResetFocus : ContextAction
    {
        public override string GetCaption() => "Reset focus";

        public override void RunAction()
        {
            var unit = this.Target?.Unit;
            if (unit == null)
            {
                UberDebug.LogError("Target is missing");
                return;
            }

            var unit_part_focus = unit.Get<UnitPartImplements>();
            if (unit_part_focus == null || unit_part_focus.isLocked())
            {
                return;
            }

            unit_part_focus.reset();
        }
    }


    public class ContextActionUnlockFocus : ContextAction
    {
        public override string GetCaption() => "Unlock focus";

        public override void RunAction()
        {
            var unit = this.Target?.Unit;
            if (unit == null)
            {
                UberDebug.LogError("Target is missing");
                return;
            }

            var unit_part_focus = unit.Get<UnitPartImplements>();
            if (unit_part_focus == null || !unit_part_focus.isLocked())
            {
                return;
            }

            unit_part_focus.unlockFocus();
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

            var unit_part_focus = unit.Get<UnitPartImplements>();
            if (unit_part_focus != null && !unit_part_focus.isLocked())
            {
                unit_part_focus.investFocus(school, val);
            }

           
            if (resource != null)
            {
                unit.Descriptor.Resources.Restore(resource, val);
            }
        }
    }


    public class RestrictionInvestedFocus : ActivatableAbilityRestriction
    {
        public SpellSchool school;
        public int amount = 1;
        public bool locked_focus = true;

        public override bool IsAvailable()
        {
            var unit_part = this.Owner.Get<UnitPartImplements>();
            if (unit_part == null)
            {
                return false;
            }

            if (locked_focus && !unit_part.isLocked())
            {
                return false;
            }

            return unit_part.getInvestedFocusAmount(school) >= amount;
        }
    }

    public class AddImplements : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartImplements>().init();

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Remove<UnitPartImplements>();
        }
    }


    public class BonusInvestedFocusPoints : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public int value;
        public SpellSchool school;
        public BlueprintAbilityResource resource;

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartImplements>().addinvestedFocusBonus(school, value);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartImplements>().removeinvestedFocusBonus(school, value);
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
        public bool locked_focus = true;

        public bool CorrectCaster(UnitEntityData caster)
        {
            var unit_part = caster?.Get<UnitPartImplements>();
            if (unit_part == null)
            {
                return false;
            }

            if (locked_focus && !unit_part.isLocked())
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
        public ContextRankConfig max_value = null;
        public bool locked_focus;

        public static BlueprintUnitProperty createProperty(string name, string guid, ContextRankConfig max_value,  bool locked_focus, params SpellSchool[] schools)
        {
            var p = Helpers.Create<BlueprintUnitProperty>();
            p.name = name;
            Main.library.AddAsset(p, guid);
            p.SetComponents(Helpers.Create<InvestedImplementFocusAmountProperty>(a => { a.schools = schools; a.max_value = max_value; a.locked_focus = locked_focus; }));

            return p;
        }

        public override int GetInt(UnitEntityData unit)
        {        
            var unit_part = unit.Get<UnitPartImplements>();
            if (unit_part == null)
            {
                return 0;
            }


            if (locked_focus && !unit_part.isLocked())
            {
                return 0;
            }

            int val = unit_part.getInvestedFocusAmount(schools);
            if (max_value != null)
            {
                var context = Helpers.GetMechanicsContext();
                if (context != null)
                {
                    var max_val = max_value.GetValue(context);
                    val = Math.Min(max_val, val);
                }
            }
            return val;
        }
    }

    [AllowMultipleComponents]
    public class IncreaseResourceAmountBasedOnInvestedFocus : OwnedGameLogicComponent<UnitDescriptor>, IResourceAmountBonusHandler, IUnitSubscriber
    {
        public SpellSchool school;
        public BlueprintAbilityResource resource;
        public bool locked_focus = false;

        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            if (!this.Fact.Active || ((Object)resource != (Object)this.resource))
                return;
            var unit_part = this.Owner.Get<UnitPartImplements>();
            if (unit_part == null)
            {
                return;
            }

            if (locked_focus && !unit_part.isLocked())
            {
                return;
            }
            bonus += unit_part.getInvestedFocusAmount(school);
        }
    }
}
