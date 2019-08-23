using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Inspect;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
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
        public BlueprintCharacterClass character_class;
        public StatType stat_type;

        public override string GetCaption()
        {
            return "Monster Lore Check";
        }

        public override void RunAction()
        {
            var target = this.Target.Unit;
            var initiator = this.Context.MaybeCaster;
            var result = bonus + initiator.Descriptor.Progression.GetClassLevel(character_class) + initiator.Descriptor.Stats.GetStat<ModifiableValueAttributeStat>(stat_type).Bonus;
            if (!InspectUnitsHelper.IsInspectAllow(target))
                return;

            BlueprintUnit blueprintForInspection = target.Descriptor.BlueprintForInspection;
            InspectUnitsManager.UnitInfo info = Game.Instance.Player.InspectUnitsManager.GetInfo(blueprintForInspection);

            if (info == null)
                return;

            if (info.KnownPartsCount == 4)
                return;

            int dc = info.DC;
            Common.AddBattleLogMessage($"{initiator.CharacterName} forced DC {dc} monster lore check: {result}");
            info.SetCheck(result, initiator);
        }
    }


    [ComponentName("Checks if monster can be inspected")]
    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityTargetCanBeInspected : BlueprintComponent, IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target.Unit;
            BlueprintUnit blueprintForInspection = unit.BlueprintForInspection;
            InspectUnitsManager.UnitInfo info = Game.Instance.Player.InspectUnitsManager.GetInfo(blueprintForInspection);
            if (info == null)
                return false;
            return info.KnownPartsCount < 4;
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
