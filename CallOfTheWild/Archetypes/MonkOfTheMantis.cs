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
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class MonkOfTheMantis
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature[] debilitating_blows = new BlueprintFeature[3];

        static LibraryScriptableObject library => Main.library;

        static BlueprintCharacterClass[] getMonkArray()
        {
            var monk_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            return new BlueprintCharacterClass[] { monk_class };
        }

        static public void create()
        {
            var monk_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MonkOfTheMantisArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Monk of the Mantis");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A body contains many points where the flesh, mind, and spirit coincide. A monk of the mantis is skilled at manipulating these points. With precise strikes, she temporarily disrupts a body’s connection with the rest of the self.");
            });
            Helpers.SetField(archetype, "m_ParentClass", monk_class);
            library.AddAsset(archetype, "");


            var monk_feat1 = library.Get<BlueprintFeatureSelection>("ac3b7f5c11bce4e44aeb332f66b75bab");
            var monk_feat6 = library.Get<BlueprintFeatureSelection>("b993f42cb119b4f40ac423ae76394374");
            var monk_feat10 = library.Get<BlueprintFeatureSelection>("1051170c612d5b844bfaa817d6f4cfff");
            var ki_power = library.Get<BlueprintFeatureSelection>("3049386713ff04245a38b32483362551");

            var sneak_attack = library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87");
            createDebilitatingBlows();


            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(2, monk_feat1),
                                                          Helpers.LevelEntry(6, monk_feat6, ki_power),
                                                          Helpers.LevelEntry(10, monk_feat10, ki_power),
                                                          Helpers.LevelEntry(12, ki_power),
                                                          Helpers.LevelEntry(14, monk_feat10),
                                                          Helpers.LevelEntry(18, monk_feat10)
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(2, sneak_attack),
                                                          Helpers.LevelEntry(6, sneak_attack, debilitating_blows[0]),
                                                          Helpers.LevelEntry(10, sneak_attack, debilitating_blows[1]),
                                                          Helpers.LevelEntry(12, debilitating_blows[2]),
                                                          Helpers.LevelEntry(14, sneak_attack),
                                                          Helpers.LevelEntry(18, sneak_attack)
                                                       };


            monk_class.Progression.UIGroups = monk_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(sneak_attack));
            monk_class.Progression.UIGroups = monk_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(debilitating_blows));

            monk_class.Archetypes = monk_class.Archetypes.AddToArray(archetype);
        }

        static void createDebilitatingBlows()
        {
            var stunning_fist_buff = library.Get<BlueprintBuff>("d9eaeba5690a7704da8bbf626456a50e");
            var entangled = library.Get<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38");
            var exhausted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");

            debilitating_blows[0] = Helpers.CreateFeature("MonkOfTheMantisDebilitatingBlowsFeature",
                                                          "Debilitating Blows",
                                                          "At 6th level, if a monk of the mantis stuns a target with Stunning Fist, the target is also entangled for 1 round after the Stunning Fist effect ends. At 10th level, if the monk of the mantis stuns a target with Stunning Fist, the target is also exhausted for 1 round after the Stunning Fist effect ends.\n"
                                                          + "At 12th level, the monk can spend 1 point from her ki pool as part of her Stunning Fist attack to increase the duration of these additional effects to a number of rounds equal to her Wisdom bonus (minimum 2 rounds).",
                                                          "",
                                                          Helpers.GetIcon("74179cb9dbaba2b47b25535fe12fea1d"), //brutal beating
                                                          FeatureGroup.None);
            debilitating_blows[1] = library.CopyAndAdd(debilitating_blows[0], "MonkOfTheMantisDebilitatingBlows2Feature", "");
            debilitating_blows[2] = library.CopyAndAdd(debilitating_blows[0], "MonkOfTheMantisDebilitatingBlows3Feature", "");

            var ki_resource = library.Get<BlueprintAbilityResource>("9d9c90a9a1f52d04799294bf91c80a82");
            var debilitating_blows_duration_buff = Helpers.CreateBuff("DebilitatingBlowsDurationBuff",
                                                                      "Debilitating Blows: (Duration Increase)",
                                                                      debilitating_blows[0].Description,
                                                                      "",
                                                                      debilitating_blows[0].Icon,
                                                                      null);
            var toggle = Helpers.CreateActivatableAbility("DebilitatingBlowsDurationToggleAbility",
                                                          debilitating_blows_duration_buff.Name,
                                                          debilitating_blows_duration_buff.Description,
                                                          "",
                                                          debilitating_blows_duration_buff.Icon,
                                                          debilitating_blows_duration_buff,
                                                          AbilityActivationType.Immediately,
                                                          CommandType.Free,
                                                          null,
                                                          ki_resource.CreateActivatableResourceLogic(ResourceSpendType.Never)
                                                          );
            toggle.DeactivateImmediately = true;

            var stunned = library.CopyAndAdd<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3", "MonkStunningFistEffectBuff", "");

            var effect = Helpers.CreateConditional(new Condition[] { Common.createContextConditionCasterHasFact(debilitating_blows_duration_buff) },
                                                  new GameAction[]{ Common.createContextActionApplyBuff(entangled, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false),
                                                                    Common.createContextActionApplyBuff(exhausted, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false),
                                                                    Helpers.Create<ResourceMechanics.ContextActionSpendResourceFromCaster>(c => {c.resource = ki_resource; c.amount = 1; })
                                                                   },
                                                  new GameAction[] { Helpers.CreateConditional(Common.createContextConditionCasterHasFact(debilitating_blows[0]), Common.createContextActionApplyBuff(entangled, Helpers.CreateContextDuration(1), dispellable: false)),
                                                                     Helpers.CreateConditional(Common.createContextConditionCasterHasFact(debilitating_blows[1]), Common.createContextActionApplyBuff(exhausted, Helpers.CreateContextDuration(1), dispellable: false))
                                                                   }
                                                  );

            stunned.AddComponents(Helpers.CreateAddFactContextActions(deactivated: effect),
                                  Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Wisdom, min: 2)
                                  );

            var old_stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");

            var attack_trigger = stunning_fist_buff.GetComponent<FeralCombatTraining.AddInitiatorAttackWithWeaponTriggerOrFeralTraining>();
            var actions = attack_trigger.Action.Actions;
            actions = Common.changeAction<ContextActionApplyBuff>(actions, c =>
                                                                            {
                                                                                if (c.Buff == old_stunned)
                                                                                {
                                                                                    c.Buff = stunned;
                                                                                }
                                                                            }
                                                                   );
            stunning_fist_buff.ReplaceComponent<FeralCombatTraining.AddInitiatorAttackWithWeaponTriggerOrFeralTraining>(a => a.Action = Helpers.CreateActionList(actions));


            debilitating_blows[2].AddComponent(Helpers.CreateAddFact(toggle));
        }
    }
}
