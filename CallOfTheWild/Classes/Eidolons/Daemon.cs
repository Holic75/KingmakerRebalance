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
using Kingmaker.UnitLogic.Abilities;
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
        static void fillDaemonProgression()
        {
            var base_evolutions = Helpers.CreateFeature("DaemonEidolonBaseEvolutionsFeature",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.resistance[0])
                                                        );
            base_evolutions.HideInCharacterSheetAndLevelUp = true;
            var feature1 = Helpers.CreateFeature("DaemonEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "Starting at 1st level, daemon eidolons gain the claws, resistance (acid) evolution as well as a +4 bonus on saving throws against death effects, disease, and poison.",
                                                  "",
                                                  daemon_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("DaemonEidolonLevel1AddFeature",
                                                                                                  Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.Other, SpellDescriptor.Poison | SpellDescriptor.Death | SpellDescriptor.Disease)),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.claws_biped),
                                                  Helpers.CreateAddFeatureOnClassLevel(base_evolutions, 16, Summoner.getSummonerArray(), before: true)
                                                  );

            var feature4 = Helpers.CreateFeature("DaemonEidolonLevel4Feature",
                                                  "Resistance",
                                                  "At 4th level, daemon eidolons gain cold resistance 10, electricity resistance 10, and fire resistance 10.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("DaemonEidolonLevel4AddFeature",
                                                                                                  Common.createEnergyDR(10, DamageEnergyType.Fire),
                                                                                                  Common.createEnergyDR(10, DamageEnergyType.Electricity),
                                                                                                  Common.createEnergyDR(10, DamageEnergyType.Cold)
                                                                                                  )
                                                  );

            var feature8 = Helpers.CreateFeature("DaemonEidolonLevel8Feature",
                                                  "Evolution Pool Increase",
                                                  "At 8th level, daemon eidolons add 1 point to their evolution pools.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(i => i.amount = 1)
                                                  );

            var feature12 = Helpers.CreateFeature("DaemonEidolonLevel12Feature",
                                                    "Damage Reduction and Immunity",
                                                    "At 12th level, daemon eidolons gain DR 5/good. They also gain immunity to death effects, disease, and poison.",
                                                    "",
                                                    Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy,
                                                    FeatureGroup.None,
                                                    Common.createAddFeatComponentsToAnimalCompanion("DaemonEidolonLevel12AddFeature",
                                                                                                    Common.createAlignmentDR(5, DamageAlignment.Good),
                                                                                                    Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Poison | SpellDescriptor.Death | SpellDescriptor.Disease),
                                                                                                    Common.createBuffDescriptorImmunity(SpellDescriptor.Poison | SpellDescriptor.Death | SpellDescriptor.Disease)
                                                                                                    )
                                                    );


            var feature16 = Helpers.CreateFeature("DaemonEidolonLevel16Feature",
                                                  "Acid Immunity",
                                                  "At 16th level, daemon eidolons lose the resistance(acid) evolution, and instead gain the immunity(acid) evolution.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[0])
                                                  );

            var devour_icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/SavageMaw.png");

            var buff = Helpers.CreateBuff("DaemonEidolonLevel20Buff",
                                          "Devour",
                                          "At 20th level, as a standard action, a daemon eidolon can devour a portion of the soul of a dying creature or a creature that died no earlier than 1 round ago. This kills the creature and provides the daemon eidolon a profane bonus on attack rolls, saving throws, and skill checks for 24 hours. The bonus is equal to +1 per 5 Hit Dice the slain creature possessed. A creature whose soul was devoured in this way requires resurrection or more powerful magic to return from the dead.",
                                          "",
                                          devour_icon,
                                          null,
                                          Helpers.Create<BuffAllSkillsBonus>(b => { b.Value = 1; b.Descriptor = ModifierDescriptor.Profane; b.Multiplier = Helpers.CreateContextValue(AbilitySharedValue.StatBonus); }),
                                          Helpers.Create<AddContextStatBonus>(a => { a.Value = Helpers.CreateContextValue(AbilitySharedValue.StatBonus); a.Stat = StatType.SaveFortitude; a.Descriptor = ModifierDescriptor.Profane; }),
                                          Helpers.Create<AddContextStatBonus>(a => { a.Value = Helpers.CreateContextValue(AbilitySharedValue.StatBonus); a.Stat = StatType.SaveReflex; a.Descriptor = ModifierDescriptor.Profane; }),
                                          Helpers.Create<AddContextStatBonus>(a => { a.Value = Helpers.CreateContextValue(AbilitySharedValue.StatBonus); a.Stat = StatType.SaveFortitude; a.Descriptor = ModifierDescriptor.Profane; }),
                                          Helpers.Create<AddContextStatBonus>(a => { a.Value = Helpers.CreateContextValue(AbilitySharedValue.StatBonus); a.Stat = StatType.AdditionalAttackBonus; a.Descriptor = ModifierDescriptor.Profane; })
                                         );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);

            var ability = Helpers.CreateAbility("DaemonEidolonLevel20Ability",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "24 hours",
                                                "",
                                                Helpers.CreateRunActions(Helpers.Create<NewMechanics.WriteTargetHDtoSharedValue>(w => { w.divisor = 5; w.shared_value = AbilitySharedValue.StatBonus; }), apply_buff),
                                                Helpers.Create<NewMechanics.AbilityTargetRecentlyDead>(a => a.RecentlyDeadBuff = library.Get<BlueprintBuff>("38bb8a5d773243843bbaaa2c340cc19c")),
                                                Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                );
            ability.setMiscAbilityParametersTouchHarmful();


            var feature20 = Helpers.CreateFeature("DaemonEidolonLevel20Feature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Common.createAddFeatComponentsToAnimalCompanion("DaemonEidolonLevel20AddFeature",
                                                                                                  Helpers.CreateAddFact(ability)
                                                                                                  )
                                                  );

            daemon_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            daemon_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);
        }
    }
}
