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
        static public BlueprintAbilityResource healing_buffer_resource;

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

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, infusion_selection),
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
            createMetahealerTalent();
            createHealingBuffer();

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

            //fix expanded element bonus
            wild_talent_selection.AddComponent(Common.prerequisiteNoArchetype(archetype));
            metahealer_wild_talent.AddComponent(Common.createPrerequisiteArchetypeLevel(archetype, 1));


            KineticistFix.expanded_element_bonus_talent_selection.AllFeatures = KineticistFix.expanded_element_bonus_talent_selection.AllFeatures.AddToArray(metahealer_wild_talent);
        }


        static void createKineticChirurgery()
        {
            var elemental_focus = library.Get<BlueprintFeatureSelection>("1f3a15a3ae8a5524ab8b97f469bf4e3d"); //to be able to pick talents
            var kinetic_knight_elemental_focus = library.Get<BlueprintFeatureSelection>("b1f296f0bd16bc242ae35d0638df82eb"); //to be able to pick talents
            var water = library.Get<BlueprintFeature>("7ab8947ce2e19c44a9edcf5fd1466686");
            var kinetic_knight_water = library.Get<BlueprintFeature>("5e839c743c6da6649a43cdeb70b6018f");
            var kinetic_healer = library.Get<BlueprintFeature>("3ef666973adfa8f40af6c0679bd98ba5");
            kinetic_chirurgery = Helpers.CreateFeature("KineticChirurgeryFeature",
                                                       "Kinetic Chirurgery",
                                                       "A kinetic chirurgeon must select water as her primary element. She gains kinetic healer as a bonus wild talent at 1st level.",
                                                       "",
                                                       Helpers.GetIcon("3ef666973adfa8f40af6c0679bd98ba5"),
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFacts(kinetic_healer)
                                                       );

            foreach (var e in elemental_focus.AllFeatures)
            {
                if (e == water)
                {
                    continue;
                }
                e.AddComponent(Common.prerequisiteNoArchetype(archetype.GetParentClass(), archetype));
            }


            foreach (var e in kinetic_knight_elemental_focus.AllFeatures)
            {
                if (e == kinetic_knight_water)
                {
                    continue;
                }
                e.AddComponent(Common.prerequisiteNoArchetype(archetype.GetParentClass(), archetype));
            }
        }


        static void createMetahealerTalent()
        {
            metahealer_wild_talent = library.CopyAndAdd<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459", "MetahealerTalent", "");
            metahealer_wild_talent.SetNameDescription("Metahealer Talent",
                                                      "A kinetic chirurgeon can select a metahealing talent or any talent altering kinetic healer wild talent. Alternatively the kinetic chirurgeon can select any one paladin mercy that a paladin of that level could select. Each time she uses kinetic healer, she can apply one of these mercies to the target of the healing.");
            metahealer_wild_talent.AllFeatures = new BlueprintFeature[0];
            var mercies_selection = library.Get<BlueprintFeatureSelection>("02b187038a8dce545bb34bbfb346428d");
            metahealer_wild_talent.AllFeatures = metahealer_wild_talent.AllFeatures.AddToArray(mercies_selection.AllFeatures);

            //create mercy effects
            createMercies();
          
            createDualHealing();
            createSwiftHealing();
            createEmpoweredHealing();
            var kinetic_revivification = library.Get<BlueprintFeature>("0377fcf4c10871f4187809d273af7f5d");
            var kinetic_restoration = library.Get<BlueprintFeature>("ed01d50910ae67b4dadc050f16d93bdf");
            var healing_burst = library.Get<BlueprintFeature>("c73b37aaa2b82b44686c56db8ce14e7f");
            metahealer_wild_talent.AllFeatures = metahealer_wild_talent.AllFeatures.AddToArray(empowered_healing, dual_healing, swift_healing, kinetic_restoration, kinetic_revivification, healing_burst);
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
                //kv.Key.ReapplyOnLevelUp = true;

                if (prereq != null)
                {
                    prereq.Group = Prerequisite.GroupType.Any;
                    kv.Key.AddComponent(Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, prereq.Level, any: true));
                }
                kv.Key.AddComponent(Helpers.Create<AddFeatureOnClassLevel>(a =>
                {
                    a.Feature = Common.ActivatableAbilityToFeature(toggle);
                    a.Class = archetype.GetParentClass();
                    a.Level = 1;
                    a.Archetypes = new BlueprintArchetype[] { archetype };
                    a.AdditionalClasses = new BlueprintCharacterClass[0];
                    a.BeforeThisLevel = false;
                }));
                
            }

            foreach (var v in heal_varinats.Variants)
            {
                v.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(mercy_actions.ToArray())));
            }
        }


        static void createSwiftHealing()
        {
            var sigil_of_creation_feature = library.Get<BlueprintFeature>("3e354532d3e41b548b883f7a67f27acf");
            swift_healing = Helpers.CreateFeature("SwiftHealingKineticHealerFeatureFeature",
                                                   "Swift Healing",
                                                   "A kinetic chirurgeon can choose to use kinetic healer on herself as a swift action.",
                                                   "",
                                                   Helpers.GetIcon("ef7ece7bb5bb66a41b256976b27f424e"),
                                                   FeatureGroup.KineticWildTalent,
                                                   Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, 13)
                                                   );

            var kinetic_healer = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c"); //kinetic healer
            var heal_variants = kinetic_healer.GetComponent<AbilityVariants>();
            var ability = library.CopyAndAdd(heal_variants.Variants[0], "Swift" + heal_variants.Variants[0].name, "");
            ability.ActionType = CommandType.Swift;
            ability.setMiscAbilityParametersSelfOnly();
            ability.Range = AbilityRange.Personal;
            ability.AddComponent(Common.createAbilityShowIfCasterHasFact(swift_healing));
            ability.SetNameDescriptionIcon(heal_variants.Variants[0].Name + " (Swift Healing)", swift_healing.Description, swift_healing.Icon);

            heal_variants.Variants = heal_variants.Variants.AddToArray(ability);
            sigil_of_creation_feature.GetComponent<AutoMetamagic>().Abilities.Add(ability);
        }


        static void createDualHealing()
        {
            var sigil_of_creation_feature = library.Get<BlueprintFeature>("3e354532d3e41b548b883f7a67f27acf");
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
            sigil_of_creation_feature.GetComponent<AutoMetamagic>().Abilities.Add(ability);
        }


        static void createEmpoweredHealing()
        {
            var sigil_of_creation_feature = library.Get<BlueprintFeature>("3e354532d3e41b548b883f7a67f27acf");
            empowered_healing = Helpers.CreateFeature("EmpoweredHealingKineticHealerFeatureFeature",
                                                   "Greater Healing",
                                                   "A kinetic chirurgeon can choose to roll one additional die when using kinetic healer. This increases to two additional dice at 11th level, and to three additional dice at 17th level. ",
                                                   "",
                                                   Helpers.GetIcon("a1de1e4f92195b442adb946f0e2b9d4e"),
                                                   FeatureGroup.KineticWildTalent,
                                                   Common.createPrerequisiteArchetypeLevel(archetype.GetParentClass(), archetype, 5)
                                                   );

            var kinetic_healer = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c"); //kinetic healer

            var heal_varinats = kinetic_healer.GetComponent<AbilityVariants>();

            for (int i = 0; i < 2; i++)
            {
                var ability = library.CopyAndAdd(heal_varinats.Variants[i], "Empowered" + heal_varinats.Variants[i].name, "");
                var actions = ability.GetComponent<AbilityEffectRunAction>().Actions.Actions;
                actions = Common.changeAction<ContextActionHealTarget>(actions, c => c.Value = Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.ProjectilesCount), c.Value.BonusValue));
                ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(actions));
                ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.MaxClassLevelWithArchetype, ContextRankProgression.DelayedStartPlusDivStep, AbilityRankType.ProjectilesCount,
                                                                      startLevel: 5, stepLevel: 6, classes: new BlueprintCharacterClass[] { archetype.GetParentClass() },
                                                                      archetype: archetype)
                                     );
                ability.AddComponent(Common.createAbilityShowIfCasterHasFact(empowered_healing));
                ability.SetNameDescriptionIcon(heal_varinats.Variants[i].Name + " (Greater Healing)", empowered_healing.Description, empowered_healing.Icon);
                heal_varinats.Variants = heal_varinats.Variants.AddToArray(ability);
                sigil_of_creation_feature.GetComponent<AutoMetamagic>().Abilities.Add(ability);
            }
        }


        static void createHealingBuffer()
        {
            var healing_burst = library.Get<BlueprintAbility>("db611ffeefb8f1e4f88e7d5393fc651d");
            var kinetic_healer = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c"); //kinetic healer
            var heal_varinats = kinetic_healer.GetComponent<AbilityVariants>();

            var buffer_resource = Helpers.CreateAbilityResource("KineticistHelearBufferResource", "", "", "", null);
            buffer_resource.SetFixedResource(1);

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
                                                                                                                          a.abilities = heal_varinats.Variants.Where(v => v.GetComponent<AbilityKineticist>().WildTalentBurnCost > 0).ToArray().AddToArray(healing_burst);
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
            healing_buffer.AddComponents(Helpers.CreateAddAbilityResource(buffer_resource),
                                         Helpers.Create<ResourceMechanics.ContextIncreaseResourceAmount>(c => { c.Resource = buffer_resource; c.Value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                         Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.StartPlusDoubleDivStep, startLevel: 6, stepLevel: 5,
                                                                          classes: new BlueprintCharacterClass[] { archetype.GetParentClass() }
                                                                          )
                                        );
            healing_buffer.ReapplyOnLevelUp = true;

            healing_buffer_resource = buffer_resource;
        }



    }
}
