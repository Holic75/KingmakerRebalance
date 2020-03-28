using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
                                                "Dismiss Area Effect",
                                                "This ability allows caster to dismiss and area effect created by herself at the target location.",
                                                "",
                                                Helpers.GetIcon("92681f181b507b34ea87018e8f7a528a"),
                                                Kingmaker.UnitLogic.Abilities.Blueprints.AbilityType.Extraordinary,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                Kingmaker.UnitLogic.Abilities.Blueprints.AbilityRange.Medium,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(Helpers.Create<ContextActionDismissSpell>())
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





}
