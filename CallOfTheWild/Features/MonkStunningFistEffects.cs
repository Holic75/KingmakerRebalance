using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Enums;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using UnityEngine;

namespace CallOfTheWild
{
    public class MonkStunningFists
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintFeature stunning_fist_staggered, stunning_fist_blind, stunning_fist_paralyzed;
        static public BlueprintBuff stunning_fist_staggered_buff, stunning_fist_blind_buff, stunning_fist_paralyzed_buff;
        static public BlueprintAbility stunning_fist_staggered_ability, stunning_fist_blind_ability, stunning_fist_paralyzed_ability;

        static internal void create()
        {
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var blind = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");
            var paralyzed = library.Get<BlueprintBuff>("af1e2d232ebbb334aaf25e2a46a92591");

            var stunning_fist_ability = library.Get<BlueprintAbility>("732ae7773baf15447a6737ae6547fc1e");
            var stunning_fist_buff = library.Get<BlueprintBuff>("d9eaeba5690a7704da8bbf626456a50e");
            var sickened_fist_buff = library.Get<BlueprintBuff>("4d7da6df5cb3b3940a9d96311a2dc311");
            var sickened_ability = library.Get<BlueprintAbility>("c81906c75821cbe4c897fa11bdaeee01");
            var fatigued_buff = library.Get<BlueprintBuff>("696b29374599d4141be64e46a91bd09b");
            var fatigued_ability = library.Get<BlueprintAbility>("32f92fea1ab81c843a436a49f522bfa1");

            stunning_fist_ability.GetComponent<AbilityCasterHasNoFacts>().Facts = stunning_fist_ability.GetComponent<AbilityCasterHasNoFacts>().Facts.AddToArray(sickened_fist_buff, fatigued_buff);
            sickened_ability.GetComponent<AbilityCasterHasNoFacts>().Facts = sickened_ability.GetComponent<AbilityCasterHasNoFacts>().Facts.AddToArray(stunning_fist_buff, fatigued_buff);
            fatigued_ability.GetComponent<AbilityCasterHasNoFacts>().Facts = fatigued_ability.GetComponent<AbilityCasterHasNoFacts>().Facts.AddToArray(sickened_fist_buff, stunning_fist_buff);

            var staggered_tuple = createStunningFistVariant("StunningFistStaggered",
                                                           "Stunning Fist: Stagger",
                                                           "This ability works as Stunning Fist, but it makes the target staggered for 1d6+1 rounds on a failed save instead of stunning for 1 round.",
                                                           staggered,
                                                           Helpers.CreateContextDuration(1, DurationRate.Rounds, Kingmaker.RuleSystem.DiceType.D6, 1),
                                                           12);

            var blind_tuple = createStunningFistVariant("StunningFistBlind",
                                                       "Stunning Fist: Blind",
                                                       "This ability works as Stunning Fist, but it makes the target permanently blind on a failed save instead of stunning for 1 round.",
                                                       blind,
                                                       null,
                                                       16);

            var paralyzed_tuple = createStunningFistVariant("StunningFistParalyzed",
                                                           "Stunning Fist: Paralyze",
                                                           "This ability works as Stunning Fist, but it makes the target paralyzed for 1d6+1 rounds on a failed save instead of stunning for 1 round.",
                                                           paralyzed,
                                                           Helpers.CreateContextDuration(1, DurationRate.Rounds, Kingmaker.RuleSystem.DiceType.D6, 1),
                                                           20);

            stunning_fist_staggered = staggered_tuple.Item1;
            stunning_fist_blind = blind_tuple.Item1;
            stunning_fist_paralyzed = paralyzed_tuple.Item1;


            stunning_fist_staggered_ability = staggered_tuple.Item3;
            stunning_fist_blind_ability = blind_tuple.Item3;
            stunning_fist_paralyzed_ability = paralyzed_tuple.Item3;

            stunning_fist_staggered_buff = staggered_tuple.Item4;
            stunning_fist_blind_buff = blind_tuple.Item4;
            stunning_fist_paralyzed_buff = paralyzed_tuple.Item4;
        }


        static (BlueprintFeature, BlueprintFeature, BlueprintAbility, BlueprintBuff) createStunningFistVariant(string name, string display_name, string description, BlueprintBuff buff, ContextDurationValue duration, int level)
        {
            var stunning_fist_ability = library.Get<BlueprintAbility>("732ae7773baf15447a6737ae6547fc1e");
            var stunning_fist_buff = library.Get<BlueprintBuff>("d9eaeba5690a7704da8bbf626456a50e");

            var new_buff = library.CopyAndAdd(stunning_fist_buff, name + "OwnerBuff", "");
            new_buff.SetNameDescriptionIcon(display_name, description, buff.Icon);

            var old_stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");
            foreach (var attack_trigger in new_buff.GetComponents<AddInitiatorAttackWithWeaponTrigger>().ToArray())
            {
                var actionst = attack_trigger.Action.Actions;
                actionst = Common.changeAction<ContextActionApplyBuff>(actionst, c =>
                                                                            {
                                                                                if (c.Buff == old_stunned)
                                                                                {
                                                                                    c.Buff = buff;
                                                                                    if (duration == null)
                                                                                    {
                                                                                        c.Permanent = true;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        c.DurationValue = duration;
                                                                                    }
                                                                                }
                                                                            }
                                                                       );
                actionst = Common.changeAction<ContextActionRemoveBuff>(actionst, c =>
                                                                        {
                                                                            c.Buff = new_buff;
                                                                        }
                                                                      );
                new_buff.ReplaceComponent(attack_trigger, attack_trigger.CreateCopy(a => a.Action = Helpers.CreateActionList(actionst)));
            }

            var new_ability = library.CopyAndAdd(stunning_fist_ability, name + "Ability", "");
            new_ability.SetNameDescriptionIcon(new_buff);
            new_ability.GetComponent<AbilityCasterHasNoFacts>().Facts = new_ability.GetComponent<AbilityCasterHasNoFacts>().Facts.AddToArray(new_buff);

            var actions = new_ability.GetComponent<AbilityEffectRunAction>().Actions.Actions;
            actions = Common.changeAction<ContextActionApplyBuff>(actions, c => c.Buff = new_buff);

            new_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(actions));
            new_ability.ReplaceComponent<SpellDescriptorComponent>(buff.GetComponent<SpellDescriptorComponent>());

            var feature_monk = Common.AbilityToFeature(new_ability, false);
            feature_monk.AddComponent(Helpers.Create<ReplaceAbilityDC>(r => { r.Ability = new_ability; r.Stat = StatType.Wisdom; }));
            var scaled_fist_feature = library.CopyAndAdd<BlueprintFeature>(feature_monk, "ScaledFist" + feature_monk.name, "");
            scaled_fist_feature.ReplaceComponent<ReplaceAbilityDC>(r => r.Stat = StatType.Charisma);

            var monk_progression = library.Get<BlueprintProgression>("8a91753b978e3b34b9425419179aafd6");
            var scaled_fist = library.Get<BlueprintArchetype>("5868fc82eb11a4244926363983897279");

           
            foreach (var le in monk_progression.LevelEntries)
            {
                if (le.Level == level)
                {
                    le.Features.Add(feature_monk);
                }
            }
            bool processed = false;
            foreach (var le in scaled_fist.RemoveFeatures)
            {
                if (le.Level == level)
                {
                    le.Features.Add(feature_monk);
                    processed = true;
                    break;
                }
            }
            if (!processed)
            {
                scaled_fist.RemoveFeatures = scaled_fist.RemoveFeatures.AddToArray(Helpers.LevelEntry(level, feature_monk));
            }

            processed = false;
            foreach (var le in scaled_fist.AddFeatures)
            {
                if (le.Level == level)
                {
                    le.Features.Add(scaled_fist_feature);
                    processed = true;
                    break;
                }
            }
            if (!processed)
            {
                scaled_fist.AddFeatures = scaled_fist.AddFeatures.AddToArray(Helpers.LevelEntry(level, feature_monk));
            }

            monk_progression.UIGroups[9].Features.Add(scaled_fist_feature);
            monk_progression.UIGroups[9].Features.Add(feature_monk);

            return (feature_monk, scaled_fist_feature, new_ability, new_buff);
        }
    }
}
