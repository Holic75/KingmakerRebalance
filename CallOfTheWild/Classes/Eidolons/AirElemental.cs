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
        static void fillAirElementalProgression()
        {
            var feature1 = Helpers.CreateFeature("AirElementalEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, air elemental eidolons gain immunity to paralysis and sleep and the slam and immunity (electricity) evolutions.",
                                                  "",
                                                  air_elemental_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AirElementalEidolonLevel1AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                                Common.createAddConditionImmunity(UnitCondition.Sleeping)
                                                                                ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.slam_biped),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[2])
                                                  );

            var feature4 = Helpers.CreateFeature("AirElementalEidolonLevel4Feature",
                                                  "Evolution Pool Increase",
                                                  "At 4th level, all elemental eidolons add 1 point to their evolution pools.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(i => i.amount = 1)
                                                  );

            var feature8 = Helpers.CreateFeature("AirElementalEidolonLevel8Feature",
                                                  "Flight",
                                                  "At 8th level, air elemental eidolons gain flight evolution.",
                                                  "",
                                                  Evolutions.flight.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.flight)
                                                  );

            var feature12 = Helpers.CreateFeature("AirElementalEidolonLevel12Feature",
                                                  "Immunity",
                                                  "At 12th level, all elemental eidolons gain immunity to bleed, poison, and stun. In addition, they can no longer be flanked.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AirElementalEidolonLevel12AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createAddConditionImmunity(UnitCondition.Stunned),
                                                                                Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.CannotBeFlanked)
                                                                                )
                                                  );

            var feature16 = Helpers.CreateFeature("AirElementalEidolonLevel16Feature",
                                                  "Immunity II",
                                                  "At 16th level, all elemental eidolons gain immunity to critical hits and do not take additional damage from precision-based attacks, such as sneak attack.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AirElementalEidolonLevel16AddFeature",
                                                                                Helpers.Create<AddImmunityToPrecisionDamage>(),
                                                                                Helpers.Create<AddImmunityToCriticalHits>()
                                                                                )
                                                  );

            var feature20 = Helpers.CreateFeature("AirElementalEidolonLevel20Feature",
                                                  "Whirlwind",
                                                  "At 20th level, air elemental eidolons gain the whirlwind ability as large air elementals.",
                                                  "",
                                                  Helpers.GetIcon("2c746396b6ce0ab47821ad960ec44df6"), //whirlwind
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("AirElementalEidolonLevel20AddFeature",
                                                                                Helpers.CreateAddFact(library.Get<BlueprintActivatableAbility>("2c746396b6ce0ab47821ad960ec44df6"))
                                                                                )
                                                  ); 

            air_elemental_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            air_elemental_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);
        }
    }
}
