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
using Kingmaker.UnitLogic.Class.Kineticist;
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
    public class OverwhelmingSoul
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature mind_over_matter;
        static public BlueprintFeature mental_prowess;
        static public BlueprintFeature overwhelming_power;

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var kineticist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "OverhelmingSoulArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Overwhelming Soul");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Some kineticists have such a powerful personality that they can seize control of their element with their minds alone, without endangering their bodies.");
            });
            Helpers.SetField(archetype, "m_ParentClass", kineticist_class);
            library.AddAsset(archetype, "");

            var burn_feature = library.Get<BlueprintFeature>("57e3577a0eb53294e9d7cc649d5239a3");
            var overflow = library.Get<BlueprintProgression>("86beb0391653faf43aec60d5ec05b538");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, burn_feature, overflow),
                                                          Helpers.LevelEntry(6, KineticistFix.internal_buffer)
                                                        };

            createMindOverMatter();
            createMentalProwess();
            createOverwhelmingPower();

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, mind_over_matter, mental_prowess),
                                                       Helpers.LevelEntry(3, overwhelming_power)
                                                    };

            archetype.OverrideAttributeRecommendations = true;
            archetype.RecommendedAttributes = new StatType[] { StatType.Charisma, StatType.Dexterity };
            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills = kineticist_class.ClassSkills.AddToArray(StatType.SkillPersuasion);
            kineticist_class.Progression.UIDeterminatorsGroup = kineticist_class.Progression.UIDeterminatorsGroup.AddToArray(mind_over_matter);
            kineticist_class.Progression.UIGroups = kineticist_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(mental_prowess, overwhelming_power));
            kineticist_class.Archetypes = kineticist_class.Archetypes.AddToArray(archetype);
        }


        static void createOverwhelmingPower()
        {
            overwhelming_power = library.CopyAndAdd<BlueprintFeature>("2496916d8465dbb4b9ddeafdf28c67d8", "OverwhelmingPowerOverwhelmingSoulFeature", "");

            overwhelming_power.SetNameDescription("Overwhelming Power",
                                                  "At 3rd level, an overwhelming soul gains a +1 bonus on attack rolls and damage rolls with her kinetic blasts. The damage bonus doesn’t apply to kinetic blade, kinetic whip, or other infusions that don’t apply the damage bonus from elemental overflow.\n"
                                                  + "This bonus increases by 1 at 6th level and every 3 levels thereafter."
                                                  );
            overwhelming_power.RemoveComponents<RecalculateOnStatChange>();
            overwhelming_power.RemoveComponents<ContextRankConfig>();
            overwhelming_power.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { archetype.GetParentClass() },
                                                                            progression: ContextRankProgression.StartPlusDivStep, startLevel: 3, stepLevel: 3));
        }


        static void createMindOverMatter()
        {
            mind_over_matter = library.CopyAndAdd<BlueprintFeature>("2fa48527ba627254ba9bf4556330a4d4", "MindOverMatterBurnFeature", "");
            var burn_resource = Helpers.CreateAbilityResource("OverwhelmingSoulBurnResource", "", "", "", null);
            burn_resource.SetIncreasedByLevel(0, 1, new BlueprintCharacterClass[] { archetype.GetParentClass() });

            var burn_per_round_resource = Helpers.CreateAbilityResource("OverwhelmingSoulBurnPerRoundResource", "", "", "", null);
            burn_per_round_resource.SetFixedResource(1);

            mind_over_matter.ReplaceComponent<AddAbilityResources>(a => a.Resource = burn_resource);
            mind_over_matter.ReplaceComponent<AddKineticistPart>(a =>
                                                                {
                                                                    a.MainStat = StatType.Charisma;
                                                                    a.MaxBurn = burn_resource;
                                                                    a.MaxBurnPerRound = burn_per_round_resource;
                                                                }
                                                                );
            mind_over_matter.RemoveComponents<AddKineticistBurnValueChangedTrigger>();
            mind_over_matter.SetNameDescription("Mind Over Matter",
                                                "An overwhelming soul uses her Charisma modifier instead of her Constitution modifier to determine her damage with wild talents, the DCs of Constitutionbased wild talents, the duration of wild talents with a Constitution-based duration, her bonus on concentration checks for wild talents, and the other Constitutionbased effects of all her wild talents.");
            mind_over_matter.AddComponents(burn_per_round_resource.CreateAddAbilityResource(),
                                           burn_resource.CreateAddAbilityResource()
                                           );
        }


        static void createMentalProwess()
        {
            var icon = Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"); //mind fog
            mental_prowess = Helpers.CreateFeature("OverwhelmingSoulMentalProwessBuff",
                                          "Mental Prowess",
                                          "An overwhelming soul’s mind is strong enough to protect her body from the stress of channeling the elements. However, in exchange, she is unable to push her limits quite as far as other kineticists.\n"
                                          + "She can’t choose to accept burn, though she can use wild talents with a burn cost if she can reduce that cost to 0 points with abilities such as gather power and infusion specialization.\n"
                                          + "A number of times per day equal to her kineticist level, she can reduce the total burn cost of any wild talent by 1.",
                                          "",
                                          icon,
                                          FeatureGroup.None
                                          );      
        }
    }
}
