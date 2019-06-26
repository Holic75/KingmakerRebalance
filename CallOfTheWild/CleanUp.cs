using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Blueprints.Items;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UI.Log;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker;
using UnityEngine;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;

namespace CallOfTheWild
{
    class CleanUp
    {
        static internal void processRage()
        {
            BlueprintBuff[] rage_buffs = new BlueprintBuff[] { Bloodrager.bloodrage_buff, //same as barabrian rage
                                                               Skald.inspired_rage_effect_buff,
                                                               Skald.controlled_rage_str_buff,
                                                               Skald.controlled_rage_dex_buff,
                                                               Skald.controlled_rage_con_buff };
            //to ensure coexistance of different rages we need to do the following:
            //on every condition on activated add check that target has no buff to avoid replacing existing buffs,
            //then we need to add all activated conditional actions to new round (to reapply buffs if they dissapear)
            //then in deactivated on remove buff will need to be replaced with remove buff from caster (to avoid removing buffs that are not hours?)


            foreach (var buff in rage_buffs)
            {
                var context_actions = buff.GetComponent<AddFactContextActions>();
                var activated_actions = context_actions.Activated.Actions;
                var new_round_actions = context_actions.NewRound;
                List<GameAction> actions_for_new_round = new List<GameAction>();

                foreach (var action in activated_actions)
                {
                    if (!(action is Conditional))
                    {
                        continue;
                    }

                    var conditional_action = (Conditional)action;
                    if (conditional_action.IfTrue.Actions.Length != 1 || !(conditional_action.IfTrue.Actions[0] is ContextActionApplyBuff))
                    {
                        continue;
                    }

                    var applied_buff = ((ContextActionApplyBuff)conditional_action.IfTrue.Actions[0]).Buff;
                    conditional_action.ConditionsChecker.Conditions = conditional_action.ConditionsChecker.Conditions.AddToArray(Common.createContextConditionHasFact(applied_buff, false));
                    conditional_action.ConditionsChecker.Operation = Operation.And;
                    actions_for_new_round.Add(action);


                }
                new_round_actions.Actions = actions_for_new_round.ToArray().AddToArray(new_round_actions.Actions);

                ;

                var deactivated_actions = context_actions.Deactivated;

                for (int i = 0; i < deactivated_actions.Actions.Length; i++)
                {
                    if (!(deactivated_actions.Actions[i] is ContextActionRemoveBuff))
                    {
                        continue;
                    }
                    var buff_to_remove = ((ContextActionRemoveBuff)deactivated_actions.Actions[i]).Buff;
                    deactivated_actions.Actions[i] = Common.createContextActionRemoveBuffFromCaster(buff_to_remove);
                }

            }
        }

    }
}
