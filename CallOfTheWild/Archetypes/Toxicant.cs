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
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
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
    public class Toxicant
    {
        static public BlueprintArchetype archetype;

        static LibraryScriptableObject library => Main.library;
        static public BlueprintFeatureSelection poison_improvement;
        static public BlueprintFeature poison_secretion;

        static public BlueprintFeature sticky_posion;
        static public BlueprintFeature celestial_poison;
        static public BlueprintFeature concentrate_poison;
        static public BlueprintFeature poison_focus;
        static public BlueprintBuff poison_buff;
        static public BlueprintFeature poison_move_action;
        static public BlueprintFeature poison_swift_action;

        static public void create()
        {
            var alchemist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("0937bec61c0dabc468428f496580c721");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ToxicantArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Toxicant");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "In lands where poisons are legal and may be openly studied and sold, some alchemists obsess over the myriad ways that poisons and venoms can be applied and delivered. Known as toxicants, these deadly artists induce the production of deadly secretions within their own bodies in order to better inflict crippling conditions upon their foes.");
            });
            Helpers.SetField(archetype, "m_ParentClass", alchemist_class);
            library.AddAsset(archetype, "");

            createPoisonBuffAndPoisonImprovements();
            createToxicSecretionAndPoisonDiscoveries();

            var mutagen = library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea");
            var throw_anything = library.Get<BlueprintFeature>("65c538dcfd91930489ad3ab18ad9204b");
            var bombs = library.Get<BlueprintFeature>("c59b2f256f5a70a4d896568658315b7d");
            var discovery = library.Get<BlueprintFeatureSelection>("cd86c437488386f438dcc9ae727ea2a6");
            var sneak_attack = library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87");
            var medical_discovery = library.Get<BlueprintFeatureSelection>("67f499218a0e22944abab6fe1c9eaeee");
            var alchemist_discovery = library.Get<BlueprintFeatureSelection>("cd86c437488386f438dcc9ae727ea2a6");
            var advance_talents = library.Get<BlueprintFeature>("a33b99f95322d6741af83e9381b2391c");

            var poison_immunity = library.Get<BlueprintFeature>("202af59b918143a4ab7c33d72c8eb6d5");
            var persistent_mutagen = library.Get<BlueprintFeature>("75ba281feb2b96547a3bfb12ecaff052");


            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, mutagen),
                                                          Helpers.LevelEntry(14, persistent_mutagen)
                                                        };


            archetype.AddFeatures = new LevelEntry[] {    Helpers.LevelEntry(1, poison_secretion),
                                                          Helpers.LevelEntry(3, poison_improvement, poison_move_action),
                                                          Helpers.LevelEntry(6, poison_improvement, poison_swift_action),
                                                          Helpers.LevelEntry(9, poison_improvement),
                                                          Helpers.LevelEntry(12, poison_improvement),
                                                          Helpers.LevelEntry(15, poison_improvement),
                                                          Helpers.LevelEntry(18, poison_improvement),
                                                       };

            alchemist_class.Progression.UIGroups = alchemist_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(poison_secretion, poison_improvement));
            alchemist_class.Progression.UIGroups = alchemist_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(poison_move_action, poison_swift_action));
            alchemist_class.Archetypes = alchemist_class.Archetypes.AddToArray(archetype);

            medical_discovery.AllFeatures = medical_discovery.AllFeatures.AddToArray(sticky_posion, celestial_poison, concentrate_poison);
            alchemist_discovery.AllFeatures = alchemist_discovery.AllFeatures.AddToArray(sticky_posion, celestial_poison, concentrate_poison);

            var dispelling_attack = library.Get<BlueprintFeature>("1b92146b8a9830d4bb97ab694335fa7c");
            ClassToProgression.addClassToFeat(archetype.GetParentClass(), new BlueprintArchetype[] { archetype }, ClassToProgression.DomainSpellsType.NoSpells, dispelling_attack, archetype.GetParentClass());

            var greater_mutagen = library.Get<BlueprintFeature>("76c61966afdd82048911f3d63c6fe0bc");
            var cognatogen = library.Get<BlueprintFeature>("e3f460ea61fcc504183c7d6818bbbf7a");
            var feral_mutagen = library.Get<BlueprintFeature>("fd5f7b37ab4301c48a88cc196ee5f0ce");

            greater_mutagen.AddComponent(mutagen.PrerequisiteFeature());
            cognatogen.AddComponent(mutagen.PrerequisiteFeature());
            feral_mutagen.AddComponent(mutagen.PrerequisiteFeature());
        }


        static void createPoisonBuffAndPoisonImprovements()
        {
            var per_round_damage = Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default)), false, false, true);

            //poison buffs for every poison condition (and empty one)
            poison_buff = Helpers.CreateBuff("ToxicantPoisonBuff",
                                                 "Toxicant Poison",
                                                 "At 1st level, a toxicant has learned to mimic creatures with the ability to secrete harmful toxins through the skin. Once per day, in a process that takes 10 minutes, the toxicant can create and imbibe a tincture that causes her skin to secrete a mild toxin. The toxicant is immune to this secretion, but any creature that hits her with a natural attack or unarmed strike must succeed at a Fortitude save (DC = 10 + 1/2 the toxicant’s alchemist level + the toxicant’s Intelligence modifier). On a failed save, the target takes an amount of damage equal to the toxicant’s Intelligence modifier. At 4th level, a target that fails its save must succeed at a second save 1 round later or take the same amount of damage again. This effect repeats as long as the target continues to fail its saving throws, to a maximum number of rounds equal to 1 + 1 for every 4 alchemist levels the toxicant possesses (to a maximum of 6 rounds at 20th level).",
                                                 "",
                                                 Helpers.GetIcon("d797007a142a6c0409a74b064065a15e"),
                                                 Common.createPrefabLink("fbf39991ad3f5ef4cb81868bb9419bff"),
                                                 Helpers.CreateAddFactContextActions(activated: per_round_damage,
                                                                                     newRound: Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(Helpers.Create<ContextActionRemoveSelf>(), per_round_damage))
                                                                                     ),
                                                 Helpers.CreateSpellDescriptor(SpellDescriptor.Poison),
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Intelligence)
                                                 );
            //3
            var bleed1d6 = library.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562");
            var dazzled = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");
            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            //6
            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            //9
            var blinded = library.Get<BlueprintBuff>("0ec36e7596a4928489d2049e1e1c76a7"); //requires dazzled
            var dazed = Common.dazed_non_mind_affecting; //requires staggered
            //12
            var exhausted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");//requries fatigued
            //18
            var stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");//requries exhausted, staggerred

            var extra_effects = new (BlueprintBuff, int, BlueprintBuff[])[]
            {
                (bleed1d6, 3, new BlueprintBuff[0]),
                (dazzled, 3, new BlueprintBuff[]{blinded }),
                (fatigued, 3, new BlueprintBuff[]{exhausted }),
                (sickened, 3, new BlueprintBuff[0]),
                (shaken, 6, new BlueprintBuff[0]),
                (staggered, 6, new BlueprintBuff[]{dazed }),
                (blinded, 9, new BlueprintBuff[0]),
                (dazed, 9, new BlueprintBuff[]{stunned }),
                (exhausted, 12, new BlueprintBuff[]{stunned }),
                (stunned, 18, new BlueprintBuff[0]),
            };

            poison_improvement = Helpers.CreateFeatureSelection("ToxicantPoisonImprovementFeatureSelection",
                                                                "Toxic Secretion Improvement",
                                                                "At 3rd level and every 3 levels thereafter, the toxicant can choose a condition to have her toxin impose.\n"
                                                                + "Once this choice is made, it can’t be changed. A creature that fails its save against the toxic secretion also gains these conditions until it succeeds at a save against the secretion, or until the toxin’s duration ends.",
                                                                "",
                                                                null,
                                                                FeatureGroup.None);

            var buff_feature_map = new Dictionary<BlueprintBuff, BlueprintFeature>();
            foreach (var ee in extra_effects)
            {
                var feature = Helpers.CreateFeature(ee.Item1.name + "ToxicantPoisonBuff",
                                                    poison_improvement.Name + ": " + ee.Item1.Name,
                                                    poison_improvement.Description + "\n" + ee.Item1.Name + ": " + ee.Item1.Description,
                                                    "",
                                                    ee.Item1.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.PrerequisiteClassLevel(archetype.GetParentClass(), ee.Item2)
                                                    );
                buff_feature_map.Add(ee.Item1, feature);
                poison_improvement.AllFeatures = poison_improvement.AllFeatures.AddToArray(feature);
            }


            foreach (var ee in extra_effects)
            {
                foreach (var p in ee.Item3)
                {
                    buff_feature_map[p].AddComponent(Helpers.PrerequisiteFeature(buff_feature_map[ee.Item1]));
                }

                var condtions = new List<Condition>();

                condtions.Add(Common.createContextConditionCasterHasFact(buff_feature_map[ee.Item1]));

                foreach (var p in ee.Item3)
                {
                    condtions.Add(Common.createContextConditionCasterHasFact(buff_feature_map[p], false));
                }

                Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(poison_buff,
                                                                                          Helpers.CreateConditional(condtions.ToArray(),
                                                                                                                    Common.createContextActionApplyBuff(ee.Item1,
                                                                                                                                                        Helpers.CreateContextDuration(),
                                                                                                                                                        is_child: true, is_permanent: true, dispellable: false)
                                                                                                                   )
                                                                                         );
            }
        }


        static void createToxicSecretionAndPoisonDiscoveries()
        {
            //2 types of abilities: touch, apply to weapon
            //all selected effects will apply on poison (so everything is done through one buff)

            //malignant poison - full round action to apply, +4 dc ?
            //celestial poison (against undead) ?
            //sticky poison
            var resource = Helpers.CreateAbilityResource("ToxicantPoisonResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, new BlueprintCharacterClass[] { archetype.GetParentClass() });
            resource.SetIncreasedByStat(0, StatType.Intelligence);

            celestial_poison = Helpers.CreateFeature("CelestialPoisonDiscovery",
                                         "Celestial Poison",
                                         "The alchemist is able to infuse poisons with celestial power so they can affect evil creatures that are normally immune to poison. Any poison the alchemist administers to a weapon can affect undead and evil outsiders, bypassing their inherent immunities. Magical effects that negate poisons still apply. If a creature fails its save, the poison acts as normal, but may have no effect on the creature, depending on the effect of the poison (such as dealing Constitution damage to undead).",
                                         "",
                                         Helpers.GetIcon("e7240516af4241b42b2cd819929ea9da"), //neutralize poison
                                         FeatureGroup.Discovery,
                                         Common.createPrerequisiteArchetypeLevel(archetype, 8)
                                         );

            sticky_posion = Helpers.CreateFeature("StickyPoisonDiscovery",
                                         "Sticky Poison",
                                         "Any poison the alchemist creates is sticky—when the alchemist applies it to a weapon, the weapon remains poisoned for a number of strikes equal to the alchemist’s Intelligence modifier.",
                                         "",
                                         Helpers.GetIcon("0c852a2405dd9f14a8bbcfaf245ff823"), //acid spalsh
                                         FeatureGroup.Discovery,
                                         Common.createPrerequisiteArchetypeLevel(archetype, 6)
                                         );



            var celestial_poison_buff = library.CopyAndAdd(poison_buff, "Celestial" + "PoisonBuff", "");
            celestial_poison_buff.RemoveComponents<SpellDescriptorComponent>();

            var apply_poison = Common.createContextActionApplyBuff(poison_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));
            var apply_celestial_poison = Common.createContextActionApplyBuff(poison_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));
            var apply_on_condition = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(celestial_poison),
                                                         Helpers.CreateConditional(Common.createContextConditionHasFact(Common.undead), apply_celestial_poison,
                                                                                   Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(Common.outsider),
                                                                                                                              Helpers.CreateContextConditionAlignment(AlignmentComponent.Evil)},
                                                                                                              apply_celestial_poison,
                                                                                                              apply_poison)
                                                                                  ),
                                                          apply_poison
                                                         );
            var apply_effect = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_on_condition));

            var duration_config = Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { archetype.GetParentClass() },
                                                                  progression: ContextRankProgression.OnePlusDivStep, stepLevel: 4);
            var poison_touch = Helpers.CreateAbility("ToxicantPoisonTouchAbility",
                                                     "Poison Touch",
                                                     "A toxicant can collect and concentrate his secretion into a poison she can deliver as a touch attack or apply to a weapon. Targets of such attacks must attempt saving throws as if they had touched the toxicant’s toxic secretion. The toxicant can do this a number of times per day equal to her alchemist level + her Intelligence modifier.",
                                                     "",
                                                     poison_buff.Icon,
                                                     AbilityType.Extraordinary,
                                                     CommandType.Standard,
                                                     AbilityRange.Touch,
                                                     "",
                                                     Helpers.fortNegates,
                                                     Helpers.CreateRunActions(apply_effect),
                                                     Helpers.CreateDeliverTouch(),
                                                     Common.createAbilitySpawnFx("fbf39991ad3f5ef4cb81868bb9419bff", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                     duration_config,
                                                     Common.createContextCalculateAbilityParamsBasedOnClass(archetype.GetParentClass(), StatType.Intelligence)
                                                     );
            poison_touch.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend;
            poison_touch.setMiscAbilityParametersTouchHarmful();
            var poison_touch_cast = Helpers.CreateTouchSpellCast(poison_touch);
            

            var hit_resource = Helpers.CreateAbilityResource("ToxicantPoisonWeaponHitResource", "", "", "", null);
            hit_resource.SetFixedResource(30);

            var after_discharge = Helpers.CreateConditional(Helpers.Create<ResourceMechanics.ContextConditionTargetHasEnoughResource>(c => { c.resource = hit_resource; c.amount = 2; }),
                                                                                                      Helpers.Create<NewMechanics.ContextActionSpendResource>(c => c.resource = hit_resource),
                                                                                                      Helpers.Create<ContextActionRemoveSelf>()
                                                                                                      );

            var weapon_buff = Helpers.CreateBuff("ToxicantPoisonWeaponBuff",
                                                 "Poison Weapon",
                                                 poison_touch.Description,
                                                 "",
                                                 LoadIcons.Image2Sprite.Create(@"AbilityIcons/PoisonWeapon.png"),
                                                 null,
                                                 Helpers.CreateAddAbilityResourceNoRestore(hit_resource),
                                                 Helpers.CreateAddFactContextActions(activated: Helpers.Create<ResourceMechanics.ContextRestoreResource>(c =>
                                                                                                                             {
                                                                                                                                 c.Resource = hit_resource;
                                                                                                                                 c.amount = Helpers.CreateContextValue(AbilityRankType.DamageDice);
                                                                                                                             }
                                                                                                                             )
                                                                                     ),
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus,type: AbilityRankType.DamageDice,  stat: StatType.Intelligence, max: 30),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_effect),
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.RangedNormal),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()), only_hit: false, on_initiator: true,
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.RangedNormal),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_effect),
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.MeleeNormal),
                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Helpers.CreateConditional(Common.createContextConditionCasterHasFact(sticky_posion),
                                                                                                                                                    after_discharge,
                                                                                                                                                    Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                                                    )),
                                                                                                  on_initiator: true,
                                                                                                  check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Melee),
                                                 duration_config
                                                 );

            var poison_weapon = Helpers.CreateAbility("ToxicantPoisonWeaponAbility",
                                                     weapon_buff.Name,
                                                     weapon_buff.Description,
                                                     "",
                                                     weapon_buff.Icon,
                                                     AbilityType.Extraordinary,
                                                     CommandType.Standard,
                                                     AbilityRange.Touch,
                                                     Helpers.oneMinuteDuration,
                                                     Helpers.fortNegates,
                                                     Helpers.CreateRunActions(Common.createContextActionApplyBuff(weapon_buff, Helpers.CreateContextDurationNonExtandable(1, DurationRate.Minutes), dispellable: false)),
                                                     Common.createContextCalculateAbilityParamsBasedOnClass(archetype.GetParentClass(), StatType.Intelligence)
                                                     );
            poison_weapon.setMiscAbilityParametersTouchFriendly();
            poison_weapon.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend;

            poison_move_action = Helpers.CreateFeature("ToxicantPoisonMoveActionFeature",
                                                       "Swift Alchemy",
                                                       "A toxicant can apply poison to a weapon as a move action.",
                                                       "",
                                                       Helpers.GetIcon("697291ff99d3fbb448be5b60b5f2a30c"),
                                                       FeatureGroup.None,
                                                       Helpers.Create<TurnActionMechanics.MoveActionAbilityUse>(m => m.abilities = new BlueprintAbility[] { poison_weapon })
                                                       );

            poison_swift_action = Helpers.CreateFeature("ToxicantPoisonSwiftActionFeature",
                                           "Swift Poisoning",
                                           "At 6th level, an alchemist can apply a dose of poison to a weapon as a swift action.",
                                           "",
                                           Helpers.GetIcon("697291ff99d3fbb448be5b60b5f2a30c"),
                                           FeatureGroup.None,
                                           Helpers.Create<TurnActionMechanics.UseAbilitiesAsSwiftAction>(m => m.abilities = new BlueprintAbility[] { poison_weapon })
                                           );

            //fix previous saves without swift poison
            Action<UnitDescriptor> save_game_fix = delegate (UnitDescriptor unit)
            {
                if (unit.Progression.GetClassLevel(archetype.GetParentClass()) >= 3  && unit.Progression.IsArchetype(archetype) && !unit.Progression.Features.HasFact(poison_move_action))
                {
                    unit.Progression.Features.AddFeature(poison_move_action);
                }
                if (unit.Progression.GetClassLevel(archetype.GetParentClass()) >= 6 && unit.Progression.IsArchetype(archetype) && !unit.Progression.Features.HasFact(poison_swift_action))
                {
                    unit.Progression.Features.AddFeature(poison_swift_action);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_fix);

            poison_secretion = Helpers.CreateFeature("ToxicantPoisonSecreation",
                                                     "Toxic Secretion",
                                                     poison_buff.Description,
                                                     "",
                                                     poison_buff.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.Create<AddTargetAttackRollTrigger>(a =>
                                                     {
                                                         a.OnlyHit = true;
                                                         a.OnlyMelee = true;
                                                         a.NotReach = true;
                                                         a.CheckCategory = true;
                                                         a.Categories = new WeaponCategory[] { WeaponCategory.UnarmedStrike, WeaponCategory.Claw, WeaponCategory.Bite, WeaponCategory.Gore, WeaponCategory.Touch, WeaponCategory.OtherNaturalWeapons };
                                                         a.ActionsOnAttacker = Helpers.CreateActionList(Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), apply_effect));
                                                     }),
                                                     duration_config,
                                                     Common.createContextCalculateAbilityParamsBasedOnClass(archetype.GetParentClass(), StatType.Intelligence),
                                                     Helpers.CreateAddFacts(poison_weapon, poison_touch_cast),
                                                     resource.CreateAddAbilityResource()
                                                     );

            var extend = library.Get<BlueprintFeature>("4ca47c023f1c158428bd55deb44c735f").GetComponent<AutoMetamagic>().CreateCopy(a => { a.Abilities = new BlueprintAbility[] { poison_weapon, poison_touch }.ToList(); a.Metamagic = Metamagic.Extend; });
            var concentrate_poison_buff = Helpers.CreateBuff("ConcentratePoisonBuff",
                                                             "Concentrate Poison",
                                                             "The alchemist can combine two doses of the same poison to increase their effects. This requires two doses of the poison and 1 minute of concentration. When completed, the alchemist has one dose of poison. The poison’s duration is extended by 100% and the save DC increases by +2.",
                                                             "",
                                                             Helpers.GetIcon("fd101fbc4aacf5d48b76a65e3aa5db6d"),
                                                             null,
                                                             Helpers.Create<NewMechanics.IncreaseSpellDCForBlueprints>(i => { i.value = 2; i.blueprints = new BlueprintScriptableObject[] { poison_weapon, poison_touch }; }),
                                                             extend
                                                             );
            var concentrate_poison_toggle = Helpers.CreateActivatableAbility("ConcentratePoisonToggleAbility",
                                                                             concentrate_poison_buff.Name,
                                                                             concentrate_poison_buff.Description,
                                                                             "",
                                                                             concentrate_poison_buff.Icon,
                                                                             concentrate_poison_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null);
            concentrate_poison_toggle.DeactivateImmediately = true;

            poison_touch_cast.AddComponents(Helpers.CreateResourceLogic(resource, cost_is_custom: true),
                                            Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_increasing_facts = new BlueprintFact[] {concentrate_poison_buff}));
            poison_weapon.AddComponents(Helpers.CreateResourceLogic(resource, cost_is_custom: true),
                                        Helpers.Create<NewMechanics.ResourseCostCalculatorWithDecreasingFacts>(r => r.cost_increasing_facts = new BlueprintFact[] { concentrate_poison_buff }));

            concentrate_poison = Common.ActivatableAbilityToFeature(concentrate_poison_toggle, false);
            concentrate_poison.Groups = new FeatureGroup[] { FeatureGroup.Discovery };
            concentrate_poison.AddComponent(Common.createPrerequisiteArchetypeLevel(archetype, 1));

            poison_focus = Helpers.CreateFeature("PoisonFocusFeature",
                                        "Ability Focus: Toxic Secretion",
                                        "The DC of poison produced by toxicant toxic secretion ability is increased by 2.",
                                        "",
                                        null,
                                        FeatureGroup.Feat,
                                        Common.createPrerequisiteArchetypeLevel(archetype, 1),
                                        Helpers.Create<NewMechanics.IncreaseSpellDCForBlueprints>(i => { i.value = 2; i.blueprints = new BlueprintScriptableObject[] { poison_weapon, poison_touch, poison_secretion }; })
                                        );
            library.AddFeats(poison_focus);
        }
    }
}
