using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        public BlueprintBuff createTrickstersSkill()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "PerformersAccoutrementsFocusProperty", "",
                                                                                                  createClassScalingConfig(ContextRankProgression.HalfMore),
                                                                                                  false,
                                                                                                  SpellSchool.Enchantment, SpellSchool.Illusion);
            var buff = Helpers.CreateBuff(prefix + "PerformersAccoutrementsFocusBuff",
                                          "Trickster's Skill",
                                          "The panoply grants a +1 bonus on Stealth and Trickery checks for every 3 points of total mental focus invested in all of the associated implements, to a maximum bonus equal to half the occultist’s level.",
                                          "",
                                          Helpers.GetIcon("d99bab42783d29f48870274fb1e85bc4"),
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.SkillStealth, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddContextStatBonus(StatType.SkillThievery, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 3,
                                                                          customProperty: property)
                                          );
            return buff;
        }


        public BlueprintFeature createTrickstersEdge()
        {
            var ability = library.CopyAndAdd<BlueprintAbility>("fa12d155c229c134dbbbebf0d7b980f0", prefix + "TrickstersEdgeAbility", "");
            ability.ReplaceComponent<AbilityResourceLogic>(a => { a.RequiredResource = resource; a.Amount = 2; });
            ability.SetNameDescription("Trickster's Edge",
                                       "The occultist can expend 2 points of mental focus as a swift action, allowing him to anticipate his opponent's defenses. Enemies are denied their Dexterity bonus against the occultist's attacks until the end of the occultist's next turn.");

            addFocusInvestmentCheck(ability, SpellSchool.Enchantment, SpellSchool.Illusion);
            var feature = Common.AbilityToFeature(ability, false);
            return feature;
        }


        public BlueprintFeature createPuppetMaster()
        {
            var dominate = library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757");
            var ability = Common.convertToSuperNatural(dominate, prefix, classes, StatType.Intelligence, resource, archetypes: getArchetypeArray(), cost: 3);
            ability.Type = AbilityType.SpellLike;
            ability.AddComponent(createClassScalingConfig(progression: ContextRankProgression.Div2));
            ability.SetNameDescription("Puppet Master",
                                       "By taking a standard action and expending 3 points of mental focus, you can dominate the mind of a creature. This functions as per dominate monster (using your occultist level as the caster level), except that the saving throw DC is equal to 10 + half your occultist level + your Intelligence modifier and the duration is equal to 1 round per 2 occultist levels. You must be at least 9th level to select this focus power.");
            ability.LocalizedDuration = Helpers.CreateString(ability.name + ".Duration", "1 round/2 levels");
            addFocusInvestmentCheck(ability, SpellSchool.Enchantment, SpellSchool.Illusion);
            var feature = Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 9);
            return feature;
        }
    }
}
