using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class Eidolon
    {
        static void fillEarthElementalProgression()
        {
            var feature1 = Helpers.CreateFeature("EarthElementalEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, earth elemental eidolons gain immunity to paralysis and sleep and the slam and immunity (acid) evolutions.",
                                                  "",
                                                  fire_elemental_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("EarthElementalEidolonLevel1AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                                Common.createAddConditionImmunity(UnitCondition.Sleeping)
                                                                                ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.slam_biped),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[0])
                                                  );

            var feature4 = Helpers.CreateFeature("EarthElementalEidolonLevel4Feature",
                                                  "Evolution Pool Increase",
                                                  "At 4th level, all elemental eidolons add 1 point to their evolution pools.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(i => i.amount = 1)
                                                  );
            transferable_abilities.Add(library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8"));
            var feature8 = Helpers.CreateFeature("EarthElementalEidolonLevel8Feature",
                                                  "Combat Maneuver Immunity",
                                                  "At 8th level, earth elemental eidolons gain immunity to combat maneuvers.",
                                                  "",
                                                  Helpers.GetIcon("737ef897849327b45b88b83a797918c8"), //freedom of movement
                                                  FeatureGroup.None,
                                                  Helpers.Create<AddFeatureToCompanion>(a => a.Feature = library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8"))
                                                  );

            var feature12 = Helpers.CreateFeature("EarthElementalEidolonLevel12Feature",
                                                  "Immunity",
                                                  "At 12th level, all elemental eidolons gain immunity to bleed, poison, and stun. In addition, they can no longer be flanked.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("EarthElementalEidolonLevel12AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createAddConditionImmunity(UnitCondition.Stunned),
                                                                                Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.CannotBeFlanked)
                                                                                )
                                                  );

            var feature16 = Helpers.CreateFeature("EarthElementalEidolonLevel16Feature",
                                                  "Immunity II",
                                                  "At 16th level, all elemental eidolons gain immunity to critical hits and do not take additional damage from precision-based attacks, such as sneak attack.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("EarthElementalEidolonLevel16AddFeature",
                                                                                Helpers.Create<AddImmunityToPrecisionDamage>(),
                                                                                Helpers.Create<AddImmunityToCriticalHits>()
                                                                                )
                                                  );

            var feature20 = Helpers.CreateFeature("EarthElementalEidolonLevel20Feature",
                                                  "Damage Reduction",
                                                  "At 20th level, earth elemental eidolons gain DR 5/- and +1 bonus to its attack, damage rolls, and combat maneuvers.",
                                                  "",
                                                  Helpers.GetIcon("c66e86905f7606c4eaa5c774f0357b2b"),
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("EarthElementalEidolonLevel20AddFeature",
                                                                                Common.createContextPhysicalDR(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                                                progression: ContextRankProgression.MultiplyByModifier,
                                                                                                                stepLevel: 10,
                                                                                                                min: 5,
                                                                                                                featureList: new BlueprintFeature[] { Evolutions.damage_reduction }
                                                                                                                ),
                                                                                Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 1, ModifierDescriptor.UntypedStackable),
                                                                                Helpers.CreateAddStatBonus(StatType.AdditionalDamage, 1, ModifierDescriptor.UntypedStackable),
                                                                                Helpers.CreateAddStatBonus(StatType.AdditionalCMB, 1, ModifierDescriptor.UntypedStackable),
                                                                                Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] { Evolutions.damage_reduction })
                                                                                )
                                                  );

            earth_elemental_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            earth_elemental_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);
        }
    }
}
