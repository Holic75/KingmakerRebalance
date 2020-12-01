using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
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


        public static void load()
        {
            createMonkKiPowerFromSpell(NewSpells.burst_of_adrenaline, 4, false, 1);
            createMonkKiPowerFromSpell(NewSpells.burst_of_insight, 4, false, 1);
            createMonkKiPowerFromSpell(NewSpells.cloak_of_winds, 6, true, 2);
            createMonkKiPowerFromSpell(NewSpells.deadly_juggernaut, 8, false, 2);
            createMonkKiPowerFromSpell(NewSpells.akashic_form, 20, false, 3);
            createMonkKiPowerFromSpell(library.Get<BlueprintAbility>("e788b02f8d21014488067bdd3ba7b325"), 20, false, 3);
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

            monk_feature.AddComponent(Helpers.PrerequisiteClassLevel(monk, required_level));
            scaled_fist_feature.AddComponent(Helpers.PrerequisiteClassLevel(monk, required_level));

            monk_ki_power_selection.AllFeatures = monk_ki_power_selection.AllFeatures.AddToArray(monk_feature);
            scaled_fist_ki_power_selection.AllFeatures = scaled_fist_ki_power_selection.AllFeatures.AddToArray(scaled_fist_feature);
        }


    }
}
