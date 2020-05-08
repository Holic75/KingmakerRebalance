using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.DismissSpells
{
    public class Dismiss
    {
        public static BlueprintFeature dismiss_feature;

        public static void create()
        {
            var ability = Helpers.CreateAbility("DismissSpellAbility",
                                                "Dismiss Spell",
                                                "This ability allows caster to dismiss and area effect created by herself at the target location or dismiss a summoned or animated creature.",
                                                "",
                                                Helpers.GetIcon("92681f181b507b34ea87018e8f7a528a"),
                                                Kingmaker.UnitLogic.Abilities.Blueprints.AbilityType.Extraordinary,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                Kingmaker.UnitLogic.Abilities.Blueprints.AbilityRange.Medium,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(Helpers.Create<ContextActionDismissSpell>()),
                                                Helpers.Create<AbilityTargetCanDismiss>()
                                                );
            ability.setMiscAbilityParametersRangedDirectional();

            dismiss_feature = Common.AbilityToFeature(ability);

            var basic_feat_progression = Main.library.Get<BlueprintProgression>("5b72dd2ca2cb73b49903806ee8986325");

            basic_feat_progression.LevelEntries[0].Features.Add(dismiss_feature);

            Action<UnitDescriptor> save_game_action = delegate (UnitDescriptor u)
            {
                if (!u.HasFact(dismiss_feature))
                {
                    u.AddFact(dismiss_feature);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_action);
        }
    }

    public class ContextActionDismissSpell : ContextAction
    {
        public override string GetCaption()
        {
            return "Dismiss";
        }

        public override void RunAction()
        {
            var unit = GameHelper.GetTargetsAround(this.Target.Point, 1.Feet().Meters * 0.1f, false, false).FirstOrDefault();

            if (unit != null)
            {
                var summoner = unit.Get<UnitPartSummonedMonster>()?.Summoner;
                if (summoner == this.Context.MaybeCaster)
                {
                    unit.Descriptor.RemoveFact(Game.Instance.BlueprintRoot.SystemMechanics.SummonedUnitBuff);
                }
                return;
            }

            var area = Game.Instance.State.AreaEffects.Where(a => a.Context.SourceAbility != null && a.Context.MaybeCaster == this.Context.MaybeCaster 
                                                             && ((a.Context.AssociatedBlueprint as BlueprintBuff) == null)
                                                             && Helpers.GetField<TimeSpan?>(a, "m_Duration").HasValue
                                                             && a.View?.Shape != null && a.View.Shape.Contains(this.Target.Point, 0.0f)).FirstOrDefault();

            if (area != null)
            {
                Common.AddBattleLogMessage($"Dismissed {area.Context.SourceAbility.Name} area effect");
                area.ForceEnd();              
            }
        }
    }



    public class AbilityTargetCanDismiss : BlueprintComponent, IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            var unit = GameHelper.GetTargetsAround(target.Point, 1.Feet().Meters*0.1f, false, false).FirstOrDefault();
            if (unit != null)
            {
                var summoner = unit.Get<UnitPartSummonedMonster>()?.Summoner;
                return summoner == caster;
            }

            var area = Game.Instance.State.AreaEffects.Where(a => a.Context.SourceAbility != null && a.Context.MaybeCaster == caster
                                                             && ((a.Context.AssociatedBlueprint as BlueprintBuff) == null)
                                                             && Helpers.GetField<TimeSpan?>(a, "m_Duration").HasValue
                                                             && a.View?.Shape != null && a.View.Shape.Contains(target.Point, 0.0f)).FirstOrDefault();

            return area != null;
        }
    }





}
