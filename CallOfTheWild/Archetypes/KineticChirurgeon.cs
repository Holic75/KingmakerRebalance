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
    public class KineticChirurgeion
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeatureSelection metahealer_wild_talent;
        static public BlueprintFeature healing_buffer;
        static public BlueprintFeature empowered_healing;
        static public BlueprintFeature dual_healing;
        static public BlueprintFeature swift_healing;
        static public BlueprintFeature kinetic_chirurgery;

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var kineticist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "KineticChirurgeonArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Kinetic Chirurgeon");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While any hydrokineticist can learn the rudiments of healing, some kineticists are virtuosos of the curative arts.");
            });
            Helpers.SetField(archetype, "m_ParentClass", kineticist_class);
            library.AddAsset(archetype, "");

            var element_selection = library.Get<BlueprintFeatureSelection>("1f3a15a3ae8a5524ab8b97f469bf4e3d");
            var infusion_selection = library.Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var wild_talent_selection = library.Get<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, element_selection, infusion_selection),
                                                          Helpers.LevelEntry(2, wild_talent_selection),
                                                          Helpers.LevelEntry(4, wild_talent_selection),
                                                          Helpers.LevelEntry(6, wild_talent_selection, KineticistFix.internal_buffer),
                                                          Helpers.LevelEntry(8, wild_talent_selection),
                                                          Helpers.LevelEntry(10, wild_talent_selection),
                                                          Helpers.LevelEntry(12, wild_talent_selection),
                                                          Helpers.LevelEntry(14, wild_talent_selection),
                                                          Helpers.LevelEntry(16, wild_talent_selection),
                                                          Helpers.LevelEntry(18, wild_talent_selection),
                                                          Helpers.LevelEntry(20, wild_talent_selection)
                                                        };
            createKineticChirurgery();
            createHealingBuffer();
            createMetahealerTalent();

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, kinetic_chirurgery),
                                                       Helpers.LevelEntry(2, metahealer_wild_talent),
                                                       Helpers.LevelEntry(4, metahealer_wild_talent),
                                                       Helpers.LevelEntry(6, healing_buffer, metahealer_wild_talent),
                                                       Helpers.LevelEntry(8, metahealer_wild_talent),
                                                       Helpers.LevelEntry(10, metahealer_wild_talent),
                                                       Helpers.LevelEntry(12, metahealer_wild_talent),
                                                       Helpers.LevelEntry(14, metahealer_wild_talent),
                                                       Helpers.LevelEntry(16, metahealer_wild_talent),
                                                       Helpers.LevelEntry(18, metahealer_wild_talent),
                                                       Helpers.LevelEntry(20, metahealer_wild_talent)
                                                    };

            archetype.OverrideAttributeRecommendations = true;
            kineticist_class.Progression.UIDeterminatorsGroup = kineticist_class.Progression.UIDeterminatorsGroup.AddToArray(kinetic_chirurgery);
            kineticist_class.Archetypes = kineticist_class.Archetypes.AddToArray(archetype);
        }


        static void createKineticChirurgery()
        {
            var water = library.Get<BlueprintFeature>("7ab8947ce2e19c44a9edcf5fd1466686");
            var kinetic_healer = library.Get<BlueprintFeature>("3ef666973adfa8f40af6c0679bd98ba5");
            kinetic_chirurgery = Helpers.CreateFeature("KineticChirurgeryFeature",
                                                       "Kinetic Chirurgery",
                                                       "A kinetic chirurgeon must select water as her primary element. She gains kinetic healer as a bonus wild talent at 1st level.",
                                                       "",
                                                       Helpers.GetIcon("3ef666973adfa8f40af6c0679bd98ba5"),
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFacts(water, kinetic_healer)
                                                       );
        }


        static void createMetahealerTalent()
        {
            metahealer_wild_talent = library.CopyAndAdd<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459", "MetahealerTalent", "");
            metahealer_wild_talent.SetNameDescription("Metahealer Talent",
                                                      "A kinetic chirurgeon can select a metahealing talent or any wild talent that has kinetic healer as prerequsite. Alternatively the kinetic chirurgeon can select any one paladin mercy that a paladin of that level could select. Each time she uses kinetic healer, she can apply one of these mercies to the target of the healing.");
            metahealer_wild_talent.AllFeatures = new BlueprintFeature[0];
            var mercies_selection = library.Get<BlueprintFeatureSelection>("02b187038a8dce545bb34bbfb346428d");
            metahealer_wild_talent.AllFeatures = metahealer_wild_talent.AllFeatures.AddToArray(mercies_selection.AllFeatures);

            //create mercy effects
            createMercies();
          
            createEmpoweredHealing();
            createDualHealing();
            createSwiftHealing();
            metahealer_wild_talent.AllFeatures = metahealer_wild_talent.AllFeatures.AddToArray(empowered_healing, dual_healing, swift_healing);
        }


        static void createMercies()
        {
            var lay_on_hands = library.Get<BlueprintAbility>("caae1dc6fcf7b37408686971ee27db13");

            var actions = lay_on_hands.GetComponent<AbilityEffectRunAction>().Actions.Actions;
            var mercies_selection = library.Get<BlueprintFeatureSelection>("02b187038a8dce545bb34bbfb346428d");


            var mercy_effects = Common.extractActions<Conditional>(actions).Where(c => c.ConditionsChecker.Conditions.Count() == 1
                                                                   && mercies_selection.AllFeatures.Contains((c.ConditionsChecker.Conditions[0] as ContextConditionCasterHasFact)?.Fact));

            var mercy_condition_map = new Dictionary<BlueprintFeature, SpellDescriptor>();
            foreach (var me in mercy_effects)
            {
                var feature = (me.ConditionsChecker.Conditions[0] as ContextConditionCasterHasFact).Fact as BlueprintFeature;
                var descriptor = (me.IfTrue.Actions[0] as ContextActionDispelMagic).Descriptor;
                mercy_condition_map.Add(feature, descriptor);
            }

            Main.logger.Log("MercyOk");
            var kinetic_healer = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c"); //kinetic healer
            var heal_varinats = kinetic_healer.GetComponent<AbilityVariants>();
            var mercy_actions = new List<GameAction>();

            foreach (var kv in mercy_condition_map)
            {
                var buff = Helpers.CreateBuff(kv.Key.name + "Buff",
                                              kv.Key.Name,
                                              "The kinetic chirurgeon can select any one paladin mercy that a paladin of that level could select instead of a wild talent. Each time she uses kinetic healer, she can apply one of these mercies to the target of the healing.",
                                              "",
                                              kv.Key.Icon,
                                              null);
                var toggle = Helpers.CreateActivatableAbility(kv.Key.name + "ToggleAbility",
                                                              buff.Name,
                                                              buff.Description,
                                                              "",
                                                              buff.Icon,
                                                              buff,
                                                              AbilityActivationType.Immediately,
                                                              CommandType.Free,
                                                              null);
                toggle.DeactivateImmediately = true;
                toggle.Group = ActivatableAbilityGroupExtension.KineticChirurgeonMercy.ToActivatableAbilityGroup();

                var mercy_action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(buff),
                                             Common.createContextActionRemoveBuffsByDescriptor(kv.Value)
                                             );
                mercy_actions.Add(mercy_action);

                var prereq = kv.Key.GetComponent<PrerequisiteClassLevel>();
                kv.Key.ReapplyOnLevelUp = true;

                if (prereq == null)
                {
                    continue;
                }
                prereq.Group = Prerequisite.GroupType.Any;
                kv.Key.AddComponent(Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, prereq.Level));
                kv.Key.AddComponent(Helpers.Create<AddFeatureOnClassLevel>(a =>
                {
                    a.Feature = Common.ActivatableAbilityToFeature(toggle);
                    a.Class = archetype.GetParentClass();
                    a.Level = 1;
                    a.Archetypes = new BlueprintArchetype[] { archetype };
                }));
                
            }

            foreach (var v in heal_varinats.Variants)
            {
                v.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(mercy_actions.ToArray())));
            }
        }


        static void createSwiftHealing()
        {
            swift_healing = Helpers.CreateFeature("SwiftHealingKineticHealerFeatureFeature",
                                                   "Swift Healing",
                                                   "A kinetic chirurgeon can choose to use kinetic healer on herself as a swift action.",
                                                   "",
                                                   Helpers.GetIcon("ef7ece7bb5bb66a41b256976b27f424e"),
                                                   FeatureGroup.KineticWildTalent,
                                                   Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, 13)
                                                   );

            var kinetic_healer = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c"); //kinetic healer
            var heal_varinats = kinetic_healer.GetComponent<AbilityVariants>();
            var ability = library.CopyAndAdd(heal_varinats.Variants[0], "Swift" + heal_varinats.Variants[0].name, "");
            ability.ActionType = CommandType.Swift;
            ability.setMiscAbilityParametersSelfOnly();
            ability.AddComponent(Common.createAbilityShowIfCasterHasFact(swift_healing));
            ability.SetNameDescriptionIcon(heal_varinats.Variants[0].Name + " (Swift Healing)", swift_healing.Description, swift_healing.Icon);

            heal_varinats.Variants = heal_varinats.Variants.AddToArray(ability);
        }


        static void createDualHealing()
        {
            dual_healing = Helpers.CreateFeature("DualHealingKineticHealerFeatureFeature",
                                                   "Dual Healing",
                                                   "A kinetic chirurgeon can choose to heal both herself and another target with the same use of kinetic healer, although in that case, both she and her target must each accept 1 point of burn, instead of only one of them needing to do so as normal for kinetic healer.",
                                                   "",
                                                   Helpers.GetIcon("867524328b54f25488d371214eea0d90"),
                                                   FeatureGroup.KineticWildTalent,
                                                   Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, 17)
                                                   );

            var kinetic_healer = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c"); //kinetic healer
            
            var heal_varinats = kinetic_healer.GetComponent<AbilityVariants>();
            var normal_heal = heal_varinats.Variants[0];
            var normal_actions = normal_heal.GetComponent<AbilityEffectRunAction>().Actions.Actions;

            var ability = library.CopyAndAdd(heal_varinats.Variants[1], "Dual" + heal_varinats.Variants[0].name, "");
            ability.ReplaceComponent<AbilityKineticist>(a => a.WildTalentBurnCost = 1);
            ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(Common.createContextActionOnContextCaster(normal_actions))));

            ability.AddComponent(Common.createAbilityShowIfCasterHasFact(dual_healing));
            ability.SetNameDescriptionIcon(heal_varinats.Variants[0].Name + " (Dual Healing)", dual_healing.Description, dual_healing.Icon);

            heal_varinats.Variants = heal_varinats.Variants.AddToArray(ability);
        }


        static void createEmpoweredHealing()
        {
            empowered_healing = Helpers.CreateFeature("EmpoweredHealingKineticHealerFeatureFeature",
                                                   "Empowered Healing",
                                                   "A kinetic chirurgeon can choose to roll one additional die when using kinetic healer. This increases to two additional dice at 11th level, and to three additional dice at 17th level. ",
                                                   "",
                                                   Helpers.GetIcon("a1de1e4f92195b442adb946f0e2b9d4e"),
                                                   FeatureGroup.KineticWildTalent,
                                                   Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, 5)
                                                   );

            var kinetic_healer = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c"); //kinetic healer

            var heal_varinats = kinetic_healer.GetComponent<AbilityVariants>();

            for (int i = 0; i < 1; i++)
            {
                var ability = library.CopyAndAdd(heal_varinats.Variants[i], "Empowered" + heal_varinats.Variants[i].name, "");
                var shared_config = ability.GetComponents<ContextCalculateSharedValue>().Where(a => a.ValueType == AbilitySharedValue.Duration).FirstOrDefault();

                var new_shared_config = Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.One,
                                                                                                          Helpers.CreateContextValue(AbilityRankType.ProjectilesCount),
                                                                                                          Helpers.CreateContextValue(AbilityRankType.DamageDice)),
                                                                           AbilitySharedValue.Heal);
                shared_config.Value.DiceCountValue = Helpers.CreateContextValue(AbilitySharedValue.Heal);
                ability.AddComponents(new_shared_config,
                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.MaxClassLevelWithArchetype, ContextRankProgression.DelayedStartPlusDivStep, AbilityRankType.ProjectilesCount,
                                                                      startLevel: 5, stepLevel: 6, classes: new BlueprintCharacterClass[] { archetype.GetParentClass() },
                                                                      archetype: archetype)
                                     );
                ability.AddComponent(Common.createAbilityShowIfCasterHasFact(empowered_healing));
                ability.SetNameDescriptionIcon(heal_varinats.Variants[i].Name + " (Empowered Healing)", empowered_healing.Description, empowered_healing.Icon);
                heal_varinats.Variants = heal_varinats.Variants.AddToArray(ability);
            }
            
        }


        static void createHealingBuffer()
        {
            var kinetic_healer = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c"); //kinetic healer
            var heal_varinats = kinetic_healer.GetComponent<AbilityVariants>();

            var buffer_resource = Helpers.CreateAbilityResource("KineticistHelearBufferResource", "", "", "", null);
            buffer_resource.SetIncreasedByLevelStartPlusDivStep(0, 6, 2, 5, 2, 0, 0.0f, new BlueprintCharacterClass[] { archetype.GetParentClass() });

            var icon = Helpers.GetIcon("be2062d6d85f4634ea4f26e9e858c3b8"); //cleanse
            var spend_resource = Helpers.Create<NewMechanics.ContextActionSpendResource>(s => s.resource = buffer_resource);
            var buff = Helpers.CreateBuff("HealerBufferBuff",
                                          "Healer Buffer",
                                          "At 6th level, a kinetic chirurgeon’s internal buffer has double the usual maximum size, and she can use it only when she would accept points of burn for the kinetic healer wild talent.\n"
                                          +"Internal Buffer: At 6th level, a kineticist’s study of her body and the elemental forces that course through it allow her to form an internal buffer to store extra energy.\n" +
                                          "When she would otherwise accept burn, a kineticist can spend energy from her buffer to avoid accepting 1 point of burn. She can do it once per day. Kineticist gets an additional use of this ability at levels 11 and 16.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<KineticistMechanics.DecreaseWildTalentCostForSpecificTalents>(a =>
                                                                                                                      {
                                                                                                                          a.actions = Helpers.CreateActionList(spend_resource);
                                                                                                                          a.abilities = heal_varinats.Variants.Where(v => v.GetComponent<AbilityKineticist>().WildTalentBurnCost > 0).ToArray();
                                                                                                                      }
                                                                                                                      )
                                          );

            var ability = Helpers.CreateActivatableAbility("HealerBufferActivatableAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(buffer_resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;

            healing_buffer = Common.ActivatableAbilityToFeature(ability, hide: false);
            healing_buffer.AddComponent(Helpers.CreateAddAbilityResource(buffer_resource));
        }



    }
}
