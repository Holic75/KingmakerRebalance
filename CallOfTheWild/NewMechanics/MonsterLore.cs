using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Inspect;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.NewMechanics.MonsterLore
{
    public class ContextMonsterLoreCheckUsingClassAndStat : ContextAction
    {
        public int bonus;
        public ContextValue value;

        public override string GetCaption()
        {
            return "Monster Lore Check";
        }

        public override void RunAction()
        {
            var target = this.Target.Unit;
            var initiator = this.Context.MaybeCaster;
            var result = bonus + value.Calculate(this.Context);
            if (!InspectUnitsHelper.IsInspectAllow(target))
                return;

            BlueprintUnit blueprintForInspection = target.Descriptor.BlueprintForInspection;
            InspectUnitsManager.UnitInfo info = Game.Instance.Player.InspectUnitsManager.GetInfo(blueprintForInspection);

            if (info == null)
                return;



            int dc = info.DC;
            Common.AddBattleLogMessage($"{initiator.CharacterName} forced DC {dc} monster lore check: {result}");
            if (info.KnownPartsCount < 4)
            {
                info.SetCheck(result, initiator);
                EventBus.RaiseEvent<IKnowledgeHandler>((Action<IKnowledgeHandler>)(h => h.HandleKnowledgeUpdated(info)));
            }
        }
    }


    public class ContextMonsterLoreCheck : ContextAction
    {
        public ContextValue value;
        public ModifierDescriptor descriptor;
        public ActionList action_on_success = null;

        public override string GetCaption()
        {
            return "Monster Lore Check";
        }

        public override void RunAction()
        {
            var target = this.Target.Unit;
            var initiator = this.Context.MaybeCaster;
            if (!InspectUnitsHelper.IsInspectAllow(target))
                return;
            

            BlueprintUnit blueprintForInspection = target.Descriptor.BlueprintForInspection;
            InspectUnitsManager.UnitInfo info = Game.Instance.Player.InspectUnitsManager.GetInfo(blueprintForInspection);
            StatType statType = !(bool)((UnityEngine.Object)info.Blueprint.Type) ? StatType.SkillLoreNature : info.Blueprint.Type.KnowledgeStat;


            if (info == null)
                return;

            ModifiableValueSkill stat = initiator.Stats.GetStat<ModifiableValueSkill>(statType);
            int? nullable1;
            int? nullable2;
            if (stat == null)
            {
                nullable1 = new int?();
                nullable2 = nullable1;
            }
            else
                nullable2 = new int?(stat.BaseValue);
            nullable1 = nullable2;

            int dc = info.DC;
            int bonus = value.Calculate(this.Context);
            if ((!nullable1.HasValue ? 0 : nullable1.Value) > 0 || (bool)initiator.Descriptor.State.Features.MakeKnowledgeCheckUntrained)
            {
                var skill_check = new RuleSkillCheck(initiator, statType, dc);
                skill_check.AddTemporaryModifier(initiator.Stats.GetStat(statType).AddModifier(bonus, null, descriptor));
                skill_check.IgnoreDifficultyBonusToDC = true;
                int rollResult = Rulebook.Trigger<RuleSkillCheck>(skill_check).RollResult;
                Common.AddBattleLogMessage($"{initiator.CharacterName} DC {dc} monster lore check: {rollResult}");

                if (dc <= rollResult && action_on_success != null)
                {
                    action_on_success.Run();
                }
                if (info.KnownPartsCount < 4)
                {
                    info.SetCheck(rollResult, initiator);                   
                    EventBus.RaiseEvent<IKnowledgeHandler>((Action<IKnowledgeHandler>)(h => h.HandleKnowledgeUpdated(info)));
                }                
            }          
        }
    }


    [ComponentName("Checks if monster can be inspected")]
    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityTargetCanBeInspected : BlueprintComponent, IAbilityTargetChecker
    {
        public bool allow_reinspect = false;
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target.Unit;
            BlueprintUnit blueprintForInspection = unit.BlueprintForInspection;
            InspectUnitsManager.UnitInfo info = Game.Instance.Player.InspectUnitsManager.GetInfo(blueprintForInspection);
            if (info == null)
                return false;
            return info.KnownPartsCount < 4 || allow_reinspect;
        }
    }


    [ComponentName("Checks if monster is inspected")]
    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityTargetInspected : BlueprintComponent, IAbilityTargetChecker
    {
        public int min_inspection_level = 1;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target.Unit;
            BlueprintUnit blueprintForInspection = unit.BlueprintForInspection;
            InspectUnitsManager.UnitInfo info = Game.Instance.Player.InspectUnitsManager.GetInfo(blueprintForInspection);
            if (info == null)
                return false;
            return info.KnownPartsCount >= min_inspection_level;
        }
    }
}
