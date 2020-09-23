using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class SkillUnlocks
    {
        static public Kingmaker.Blueprints.LibraryScriptableObject library => Main.library;
        
        //perception + sense motive (+2 insigt bonus as swift action)
        
        //stealth
        //knowledge (+ attack bonuses/cl/saves)

        public static BlueprintFeature heal_unlock;
        public static BlueprintFeature intimidate_unlock;
        public static BlueprintFeature mobility_unlock;
        public static BlueprintFeatureSelection signature_skill;

        internal static void load()
        {
            signature_skill = Helpers.CreateFeatureSelection("SignatureSkillFeatureSelection",
                                           "Signature Skill",
                                           "Choose one skill. You gain a special ability associated to this skill.",
                                           "",
                                           null,
                                           FeatureGroup.Feat
                                           );

            library.AddFeats(signature_skill);
            createHealUnlock();
            createIntimidateUnlock();
            createMobilityUnlock();
        }

        static void createIntimidateUnlock()
        {
            var icon = Helpers.GetIcon("1621be43793c5bb43be55493e9c45924"); //skill focus persuasion
            intimidate_unlock = Helpers.CreateFeature("SignatureSkillIntimidateFeature",
                                                      "Signature Skill: Intimidate",
                                                      "If you exceed the DC to demoralize a target by at least 10, it is frightened for 1 round in addition to being shaken. A Will save (DC = 10 + your number of ranks in Intimidate) negates the frightened condition, but the target is still shaken, even if it has the stalwart ability.\n"
                                                      + "When you earn 15 ranks in Persuasion, the duration of frightened state is increased to 1d4 rounds.",
                                                      "",
                                                      icon,
                                                      FeatureGroup.Feat,
                                                      Helpers.Create<DemoralizeMechanics.IntimidateSkillUnlock>(),
                                                      Helpers.PrerequisiteStatValue(StatType.SkillPersuasion, 5)
                                                      );
            signature_skill.AllFeatures = signature_skill.AllFeatures.AddToArray(intimidate_unlock);
        }


        static void createMobilityUnlock()
        {
            var icon = Helpers.GetIcon("52dd89af385466c499338b7297896ded"); //skill focus mobility
            mobility_unlock = Helpers.CreateFeature("SignatureSkillMobilityFeature",
                                                      "Signature Skill: Mobility",
                                                      "You do not provoke attacks of opportunity when standing up from prone.",
                                                      "",
                                                      icon,
                                                      FeatureGroup.Feat,
                                                      Helpers.CreateAddMechanics(Kingmaker.UnitLogic.FactLogic.AddMechanicsFeature.MechanicsFeatureType.GetUpWithoutAttackOfOpportunity),
                                                      Helpers.PrerequisiteStatValue(StatType.SkillMobility, 5)
                                                      );
            signature_skill.AllFeatures = signature_skill.AllFeatures.AddToArray(mobility_unlock);
        }


        static void createHealUnlock()
        {
            var icon = Helpers.GetIcon("c541f80af8d0af4498e1abb6025780c7"); //skill focus religion

            var heal_ability = library.Get<BlueprintAbility>("4843cb4c23951f54290c5149a4907f54");
            heal_ability.Range = AbilityRange.Touch;

            var treat_deadly_wounds_cooldown = Helpers.CreateBuff("TreatDeadlyWoundsCooldown",
                                                                     "Treat Deadly Wounds Cooldown",
                                                                     "As a full-round action you can treat your ally's wounds if you succeed at DC 20 Lore (Religion) check. When treating deadly wounds, you can restore hit points to a damaged creature. Treating deadly wounds restores 1 hit point per level of the creature. If you exceed the DC by 5 or more, add your Wisdom modifier (if positive) to this amount. A single creature can not benefit from this ability more than once per day.",
                                                                     "",
                                                                     Helpers.GetIcon("f6f95242abdfac346befd6f4f6222140"),
                                                                     null
                                                                     );
            treat_deadly_wounds_cooldown.SetBuffFlags(BuffFlags.RemoveOnRest);


            heal_unlock = Helpers.CreateFeature("SignatureSkillHealFeature",
                                                "Signature Skill: Heal",
                                                "The amount of healing you can provide with Treat Deadly Wounds ability increases as you gain more skill ranks:\n"
                                                + "05 Ranks: 2 hit points per target level, and 2 points of ability damage for each damaged ability;\n"
                                                + "10 Ranks: 4 hit points per target level, and 4 points of ability damage for each damaged ability;\n"
                                                + "15 Ranks: 6 hit points per target level, and 6 points of ability damage for each damaged ability;\n"
                                                + "20 Ranks: 9 hit points per target level, and 9 points of ability damage for each damaged ability.",
                                                "",
                                                icon,
                                                FeatureGroup.Feat,
                                                Helpers.PrerequisiteStatValue(StatType.SkillLoreReligion, 5)
                                                );

            var heal_wis_value = Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default);

            var heal_unlock_action = Helpers.CreateConditional(Helpers.Create<SkillMechanics.ContextSkillRanks>(c => { c.check_caster = true; c.skill = StatType.SkillLoreReligion; c.value = 20; }),
                                                               Helpers.Create<HealingMechanics.ContextActionTreatDeadlyWounds>(c => c.Value = 9),
                                                               Helpers.CreateConditional(Helpers.Create<SkillMechanics.ContextSkillRanks>(c => { c.check_caster = true; c.skill = StatType.SkillLoreReligion; c.value = 15; }),
                                                                                         Helpers.Create<HealingMechanics.ContextActionTreatDeadlyWounds>(c => c.Value = 6),
                                                                                         Helpers.CreateConditional(Helpers.Create<SkillMechanics.ContextSkillRanks>(c => { c.check_caster = true; c.skill = StatType.SkillLoreReligion; c.value = 10; }),
                                                                                                                   Helpers.Create<HealingMechanics.ContextActionTreatDeadlyWounds>(c => c.Value = 4),
                                                                                                                   Helpers.Create<HealingMechanics.ContextActionTreatDeadlyWounds>(c => c.Value = 2)
                                                                                                                   )
                                                                                         )
                                                              );

            var heal_action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(heal_unlock),
                                                        heal_unlock_action,
                                                        Helpers.Create<HealingMechanics.ContextActionTreatDeadlyWounds>(c => { c.Value = 1; c.stats_to_heal = new StatType[0]; })
                                                        );

            var wis_heal_action = Helpers.Create<HealingMechanics.ContextActionTreatDeadlyWounds>(c => { c.Value = heal_wis_value; c.stats_to_heal = new StatType[0]; c.multiply_by_hd = false; });
            var effect = Helpers.Create<SkillMechanics.ContextActionSkillCheckWithFailures>(c =>
            {
                c.Stat = StatType.SkillLoreReligion;
                c.on_caster = true;
                c.use_custom_dc = true;
                c.custom_dc = 20;
                c.Bypass5 = Helpers.CreateActionList(wis_heal_action);
                c.Bypass10 = Helpers.CreateActionList(wis_heal_action);
                c.Success = Helpers.CreateActionList(heal_action);
            });

            var treat_deadly_wounds_ability = library.CopyAndAdd(heal_ability, "TreatDeadlyWoundsHealSkillAbility", "");
            treat_deadly_wounds_ability.SetNameDescriptionIcon("Treat Deadly Wounds", treat_deadly_wounds_cooldown.Description, treat_deadly_wounds_cooldown.Icon);
            Common.setAsFullRoundAction(treat_deadly_wounds_ability);
            treat_deadly_wounds_ability.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateRunActions(effect, Common.createContextActionApplyBuff(treat_deadly_wounds_cooldown, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false)),
                Common.createAbilityTargetHasFact(inverted: true, treat_deadly_wounds_cooldown)
            };
            treat_deadly_wounds_ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Wisdom, min: 0));

            var heal_ability_copy = library.CopyAndAdd(heal_ability, heal_ability.name + "Child", "");
            heal_ability.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAbilityVariants(heal_ability, treat_deadly_wounds_ability, heal_ability_copy)
            };
            heal_ability.SetNameDescription("Use Heal Skill", "You can use your healing skills to treat afflictions or deadly wounds.");

            signature_skill.AllFeatures = signature_skill.AllFeatures.AddToArray(heal_unlock);
        } 
    }
}
