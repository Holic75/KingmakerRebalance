using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class MonkKiPowers
    {
        static LibraryScriptableObject library = Main.library;
        static public BlueprintAbilityResource wis_resource = library.Get<BlueprintAbilityResource>("9d9c90a9a1f52d04799294bf91c80a82");
        static public BlueprintAbilityResource cha_resource = library.Get<BlueprintAbilityResource>("7d002c1025fbfe2458f1509bf7a89ce1");
        static public BlueprintFeatureSelection monk_ki_power_selection = library.Get<BlueprintFeatureSelection>("3049386713ff04245a38b32483362551");
        static public BlueprintFeatureSelection scaled_fist_ki_power_selection = library.Get<BlueprintFeatureSelection>("4694f6ac27eaed34abb7d09ab67b4541");
        static public BlueprintCharacterClass monk = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");

        static BlueprintFeature sensei_mystic_powers = library.Get<BlueprintFeature>("d5f7bcde6e7e5ed498f430ebf5c29837");
        static BlueprintFeature sensei_mystic_powers_mass = library.Get<BlueprintFeature>("a316044187ec61344ba33535f42f6a4d");

        public static void load()
        {
            createMonkKiPowerFromSpell(NewSpells.burst_of_adrenaline, 4, false, 1);
            createMonkKiPowerFromSpell(NewSpells.burst_of_insight, 4, false, 1);
            createMonkKiPowerFromSpell(NewSpells.cloak_of_winds, 6, true, 2);
            createMonkKiPowerFromSpell(NewSpells.deadly_juggernaut, 8, false, 2);
            createMonkKiPowerFromSpell(NewSpells.akashic_form, 20, false, 3);
            createMonkKiPowerFromSpell(library.Get<BlueprintAbility>("e788b02f8d21014488067bdd3ba7b325"), 20, false, 3); //frightful aspect
        }

        public static void createMonkKiPowerFromSpell(BlueprintAbility spell, int required_level, bool self_only, int cost = 1)
        {
            string action_type = "standard";
            if (spell.ActionType == UnitCommand.CommandType.Swift)
            {
                action_type = "swift";
            }
            else if (spell.ActionType == UnitCommand.CommandType.Move)
            {
                action_type = "move";
            }
            else if (spell.IsFullRoundAction)
            {
                action_type = "full-round";
            }

            var description = $"A monk with this ki power can spend {cost} point{(cost != 1 ? "s" : "")} from his ki pool to apply effect of the {spell.Name} spell to himself as a {action_type} action.\n"
                  + spell.Name + ": " + spell.Description;
            var name = "Ki Power: " + spell.Name;

            var scaled_fist_ability = Common.convertToSpellLikeVariants(spell, "ScaledFistKiPower", new BlueprintCharacterClass[] {monk }, StatType.Charisma,
                                                                           cha_resource, self_only: self_only, cost: cost);
            scaled_fist_ability.SetNameDescription(name, description);
            var scaled_fist_feature = Common.AbilityToFeature(scaled_fist_ability, false);

            var monk_ability = Common.convertToSpellLikeVariants(spell, "KiPower", new BlueprintCharacterClass[] { monk }, StatType.Wisdom,
                                                               wis_resource, self_only: self_only, cost: cost);
            monk_ability.SetNameDescription(name, description);

            var monk_feature = Common.AbilityToFeature(monk_ability, false);
            if (monk_ability.HasVariants)
            {
                foreach (var v in monk_ability.Variants)
                {
                    v.SetName("Ki Power: " + v.Name);
                }
            }
            if (scaled_fist_ability.HasVariants)
            {
                foreach (var v in scaled_fist_ability.Variants)
                {
                    v.SetName("Ki Power: " + v.Name);
                }
            }

            monk_feature.AddComponent(Helpers.PrerequisiteClassLevel(monk, required_level));
            scaled_fist_feature.AddComponent(Helpers.PrerequisiteClassLevel(monk, required_level));

            monk_ki_power_selection.AllFeatures = monk_ki_power_selection.AllFeatures.AddToArray(monk_feature);
            scaled_fist_ki_power_selection.AllFeatures = scaled_fist_ki_power_selection.AllFeatures.AddToArray(scaled_fist_feature);

            var mystic_wisdom_ability = library.CopyAndAdd(monk_ability, "SenseiAdvice" + monk_ability.name, "");
            mystic_wisdom_ability.Range = AbilityRange.Close;
            mystic_wisdom_ability.setMiscAbilityParametersSingleTargetRangedFriendly();
            mystic_wisdom_ability.SetName(mystic_wisdom_ability.Name.Replace("Ki Power: ", "Sensei Advice: "));
            mystic_wisdom_ability.MaybeReplaceComponent<AbilityResourceLogic>(a => a.Amount = a.Amount + 1);
            if (mystic_wisdom_ability.HasVariants)
            {
                var variants = mystic_wisdom_ability.Variants.ToList().ToArray();

                for (int i = 0; i < variants.Length; i++)
                {
                    variants[i] = library.CopyAndAdd(variants[i], "SenseiAdvice" + variants[i].name, "");
                    variants[i].SetName(variants[i].Name.Replace("Ki Power: ", "Sensei Advice: "));
                    variants[i].Range = AbilityRange.Close;
                    variants[i].setMiscAbilityParametersSingleTargetRangedFriendly();
                    variants[i].Parent = mystic_wisdom_ability;
                    variants[i].ReplaceComponent<AbilityResourceLogic>(a => a.Amount = a.Amount + 1);
                }
                mystic_wisdom_ability.ReplaceComponent<AbilityVariants>(a => a.Variants = variants);
            }

            sensei_mystic_powers.AddComponent(Common.createAddFeatureIfHasFact(monk_feature, mystic_wisdom_ability));


            var mystic_wisdom_ability_mass = library.CopyAndAdd(monk_ability, "SenseiAdviceMass" + monk_ability.name, "");
            mystic_wisdom_ability_mass.SetName(mystic_wisdom_ability.Name.Replace("Ki Power: ", "Sensei Advice: Mass "));
            mystic_wisdom_ability_mass.MaybeReplaceComponent<AbilityResourceLogic>(a => a.Amount = a.Amount + 2);
            if (mystic_wisdom_ability_mass.HasVariants)
            {
                var variants = mystic_wisdom_ability_mass.Variants.ToList().ToArray();

                for (int i = 0; i < variants.Length; i++)
                {
                    variants[i] = library.CopyAndAdd(variants[i], "SenseiAdviceMass" + variants[i].name, "");
                    variants[i].SetName(variants[i].Name.Replace("Ki Power: ", "Sensei Advice: Mass "));
                    variants[i].Parent = mystic_wisdom_ability_mass;
                    variants[i].AddComponent(Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally));
                    variants[i].ReplaceComponent<AbilityResourceLogic>(a => a.Amount = a.Amount + 2);
                }
                mystic_wisdom_ability_mass.ReplaceComponent<AbilityVariants>(a => a.Variants = variants);
            }
            else
            {
                mystic_wisdom_ability_mass.AddComponent(Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally));
            }

            sensei_mystic_powers_mass.AddComponent(Common.createAddFeatureIfHasFact(monk_feature, mystic_wisdom_ability_mass));
        }


    }
}
