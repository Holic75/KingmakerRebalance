using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    class NewRagePowers
    {
        static LibraryScriptableObject library => Main.library;
        static BlueprintFeatureSelection rage_powers_selection => Main.library.Get<BlueprintFeatureSelection>("28710502f46848d48b3f0d6132817c4e");
        static BlueprintFeatureSelection extra_rage_power_selection => Main.library.Get<BlueprintFeatureSelection>("0c7f01fbbe687bb4baff8195cb02fe6a");
        static BlueprintBuff rage_buff => library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
        static BlueprintActivatableAbility reckless_stance => library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");
        static BlueprintCharacterClass barbarian_class => ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

        static internal BlueprintBuff rage_marker_caster;

        static internal BlueprintFeature taunting_stance;
        static internal BlueprintFeature terrifying_howl_feature;
        static internal BlueprintAbility terrifying_howl_ability;
        static internal BlueprintFeature quick_reflexes_feature;

        static internal BlueprintFeature lesser_atavism_totem;
        static internal BlueprintFeature atavism_totem;
        static internal BlueprintFeature greater_atavism_totem;

        static internal BlueprintFeature unrestrained_rage_feature;



        static internal void load()
        {
            createRageMarker();
            createTauntingStance();
            createTerrefyingHowl();
            createQuickReflexes();
            createLesserAtavismTotem();
            createAtavismTotem();
            createGreaterAtavismTotem();
            createUnrestrainedRage();
        }


        static void createRageMarker()
        {
            rage_marker_caster = Helpers.CreateBuff("RageMarkerBuff",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     null);
            rage_marker_caster.SetBuffFlags(BuffFlags.HiddenInUi);
            var conditional_caster = Helpers.CreateConditional(Common.createContextConditionIsCaster(),
                                                               Common.createContextActionApplyBuff(rage_marker_caster, Helpers.CreateContextDuration(),
                                                                                                   is_child: true, dispellable: false, is_permanent: true)
                                                              );                                                                                                                          
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(rage_buff, rage_marker_caster, conditional_caster);
        }


        static void addToSelection(BlueprintFeature rage_power)
        {
            extra_rage_power_selection.AllFeatures = extra_rage_power_selection.AllFeatures.AddToArray(rage_power);
            rage_powers_selection.AllFeatures = rage_powers_selection.AllFeatures.AddToArray(rage_power);
        }


        static internal void createUnrestrainedRage()
        {
            var buff = Helpers.CreateBuff("UnrestrainedRageEffectBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createAddConditionImmunity(Kingmaker.UnitLogic.UnitCondition.Paralyzed),
                                          Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis)
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);
            unrestrained_rage_feature = Helpers.CreateFeature("UnrestrainedRageFeature",
                                                              "Unrestrained Rage",
                                                              "While raging, the barbarian is immune to paralysis.",
                                                              "",
                                                              null,
                                                              FeatureGroup.RagePower,
                                                              Helpers.PrerequisiteClassLevel(barbarian_class, 12)
                                                              );
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(rage_buff,buff, unrestrained_rage_feature);
            addToSelection(unrestrained_rage_feature);
        }


        static internal void createLesserAtavismTotem()
        {
            var animal_fury_buff = library.Get<BlueprintBuff>("a67b51a8074ae47438280be0a87b01b6");
            var animal_fury = library.Get<BlueprintFeature>("25954b1652bebc2409f9cb9d5728bceb");
            var acid_maw = library.Get<BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67");
            var lesser_beast_totem = library.Get<BlueprintFeature>("d99dfc9238a8a6646b32be09057c1729");

            var lesser_atavism_buff_size = Helpers.CreateBuff("LesserAtavismTotemBiteBuff",
                                           "",
                                           "",
                                           "",
                                           null,
                                           null,
                                           Common.createWeaponTypeSizeChange(1, library.Get<BlueprintWeaponType>("952e30e6cb40b454789a9db6e5f6dd09")) //animal fury bite
                                           );

            lesser_atavism_buff_size.SetBuffFlags(BuffFlags.HiddenInUi);
            lesser_atavism_totem = Helpers.CreateFeature("LesserAtavismTotemFeature",
                                                         "Atavism Totem, Lesser",
                                                         "The barbarian gains a bite attack; if she already has a bite attack, it deals damage as if the barbarian were one size larger."
                                                         + " Note: Totem rage powers grant powers related to a theme.A barbarian cannot select from more than one group of totem rage powers; for example, a barbarian who selects a beast totem rage power cannot later choose to gain any of the dragon totem rage powers(any rage power with \"dragon totem\" in its title)",
                                                         "",
                                                         acid_maw.Icon,
                                                         FeatureGroup.RagePower,
                                                         Helpers.PrerequisiteNoFeature(lesser_beast_totem));
            lesser_beast_totem.AddComponent(Helpers.PrerequisiteNoFeature(lesser_atavism_totem));

            var conditional_size = Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(animal_fury),
                                                                         Common.createContextConditionHasFact(lesser_atavism_totem) },
                                                        Common.createContextActionApplyBuff(lesser_atavism_buff_size, Helpers.CreateContextDuration(),
                                                                                             is_child: true, is_permanent: true, dispellable: false)
                                                       );
            var conditional_bite = Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(animal_fury, has: false),
                                                                         Common.createContextConditionHasFact(lesser_atavism_totem) },
                                            Common.createContextActionApplyBuff(animal_fury_buff, Helpers.CreateContextDuration(),
                                                                                 is_child: true, is_permanent: true, dispellable: false)
                                           );
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(rage_buff, lesser_atavism_buff_size, conditional_size);
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(rage_buff, animal_fury_buff, conditional_bite);

            addToSelection(lesser_atavism_totem);
        }


        static internal void createAtavismTotem()
        {
            var ferocity = library.Get<BlueprintUnitFact>("955e356c813de1743a98ab3485d5bc69");
            var atavism_totem_buff = Helpers.CreateBuff("AtavismTotemBuff",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        null,
                                                        Helpers.CreateAddFact(ferocity)
                                                        );
            atavism_totem_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            atavism_totem = Helpers.CreateFeature("AtavismTotemFeature",
                                                  "Atavism Totem",
                                                  "The barbarian gains ferocity.",
                                                  "",
                                                  lesser_atavism_totem.Icon,
                                                  FeatureGroup.RagePower,
                                                  Helpers.PrerequisiteClassLevel(barbarian_class, 6),
                                                  Helpers.PrerequisiteFeature(lesser_atavism_totem)
                                                  );

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(rage_buff, atavism_totem_buff, atavism_totem);
            addToSelection(atavism_totem);
        }


        static internal void createGreaterAtavismTotem()
        {
            var overrun = library.CopyAndAdd<BlueprintAbility>("1a3b471ecea51f7439a946b23577fd70", "GreaterAtavismTrample", "");
            var trample = library.Get<BlueprintFeature>("9292099e5fd70f84fb07fbb9b8b6a5a5");

            greater_atavism_totem = Helpers.CreateFeature("AtavismTotemGreaterFeature",
                                                  "Atavism Totem, Greater",
                                                  "The barbarian gains trample.",
                                                  "",
                                                  lesser_atavism_totem.Icon,
                                                  FeatureGroup.RagePower,
                                                  Helpers.CreateAddFact(overrun),
                                                  Helpers.CreateAddFact(trample),
                                                  Helpers.PrerequisiteClassLevel(barbarian_class, 10),
                                                  Helpers.PrerequisiteFeature(atavism_totem)
                                                  );

            overrun.AddComponent(Common.createAbilityCasterHasFacts(rage_marker_caster));
            addToSelection(greater_atavism_totem);
        }




        static void createQuickReflexes()
        {
            var quick_reflexes_buff = Helpers.CreateBuff("QuickReflexesEffectBuff",
                                                         "Quick Reflexes",
                                                         "While raging, the barbarian can make one additional attack of opportunity per round.",
                                                         "",
                                                         null,
                                                         null,
                                                         Helpers.CreateAddStatBonus(StatType.AttackOfOpportunityCount, 1, Kingmaker.Enums.ModifierDescriptor.UntypedStackable)
                                                         );
            quick_reflexes_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            
            quick_reflexes_feature = Helpers.CreateFeature("QuickReflexesEffectFeature",
                                                           quick_reflexes_buff.Name,
                                                           quick_reflexes_buff.Description,
                                                           "",
                                                           null,
                                                           FeatureGroup.RagePower);

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(rage_buff, quick_reflexes_buff, quick_reflexes_feature);
            addToSelection(quick_reflexes_feature);
        }


        static void createTerrefyingHowl()
        {
            var dazzling_display = library.Get<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb");
            terrifying_howl_ability = library.CopyAndAdd<BlueprintAbility>("08cb5f4c3b2695e44971bf5c45205df0", "TerrifyingHowlAbility", "");
            terrifying_howl_ability.SetName("Terrifying Howl");
            terrifying_howl_ability.SetDescription("The barbarian unleashes a terrifying howl as a standard action. All shaken enemies within 30 feet must make a Will save (DC equal to 10 + 1/2 the barbarian’s level + the barbarian’s Strength modifier) or be frightened for 1d4+1 rounds.\n"
                                            + "Once an enemy has made a save versus terrifying howl (successful or not), it is immune to this power for 24 hours.");
            terrifying_howl_ability.Type = AbilityType.Extraordinary;
            terrifying_howl_ability.RemoveComponents<SpellComponent>();
            terrifying_howl_ability.RemoveComponents<SpellListComponent>();
            terrifying_howl_ability.Range = AbilityRange.Personal;

            var frighteneed_buff = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");
            var shaken_buff = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var cooldown_buff = Helpers.CreateBuff("TerrifyingHowlCooldownBuff",
                                                   "Cooldown: Terrifying Howl",
                                                   terrifying_howl_ability.Description,
                                                   "",
                                                   terrifying_howl_ability.Icon,
                                                   null);
            cooldown_buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var on_failed_save = Common.createContextSavedApplyBuff(frighteneed_buff,
                                                                    Helpers.CreateContextDuration(Common.createSimpleContextValue(1),
                                                                                                  Kingmaker.UnitLogic.Mechanics.DurationRate.Rounds,
                                                                                                  Kingmaker.RuleSystem.DiceType.D4,
                                                                                                  Common.createSimpleContextValue(1)
                                                                                                  ),
                                                                    is_dispellable: false
                                                                    );

            var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1),
                                                                                                                  Kingmaker.UnitLogic.Mechanics.DurationRate.Days),
                                                                    dispellable: false
                                                                    );
            PrefabLink p = new PrefabLink();
            p.AssetId = "cbfe312cb8e63e240a859efaad8e467c";
            var fx = Common.createContextActionSpawnFx(p);


            var condition = Helpers.CreateConditional(new Condition[] { Helpers.CreateConditionHasBuffFromCaster(cooldown_buff, true),
                                                                        Helpers.CreateConditionHasFact(shaken_buff),
                                                                        Helpers.Create<ContextConditionIsEnemy>()
                                                                        },
                                                       Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(on_failed_save, apply_cooldown, fx)));
            condition.ConditionsChecker.Operation = Operation.And;

            terrifying_howl_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(condition));
            terrifying_howl_ability.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[]{barbarian_class}, StatType.Strength));
            terrifying_howl_ability.AddComponent(dazzling_display.GetComponent<AbilitySpawnFx>());
            terrifying_howl_ability.AddComponent(Common.createAbilityCasterHasFacts(rage_marker_caster)); // allow to use only on rage
            terrifying_howl_feature = Common.AbilityToFeature(terrifying_howl_ability, false);
            terrifying_howl_feature.Groups = new FeatureGroup[] { FeatureGroup.RagePower };
            terrifying_howl_feature.AddComponent(Helpers.PrerequisiteClassLevel(barbarian_class, 8));
            addToSelection(terrifying_howl_feature);
        }


        static void createTauntingStance()
        {
            var shout = library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162");



            var buff = Helpers.CreateBuff("TauntingStanceEffectBuff",
                                          "Taunting Stance",
                                          "The barbarian can leave herself open to attacks while preparing devastating counterattacks. Enemies gain a +4 bonus on attack and damage rolls against the barbarian while she’s in this stance, but every attack against the barbarian provokes an attack of opportunity from her. This is a stance rage power.",
                                          "",
                                          shout.Icon,
                                          null,
                                          Common.createComeAndGetMe()
                                          );

            taunting_stance = Common.createSwitchActivatableAbilityBuff("TauntingStance", "", "", "",
                                                      buff, rage_buff,
                                                      reckless_stance.ActivateWithUnitAnimation,
                                                      ActivatableAbilityGroup.BarbarianStance,
                                                      command_type: CommandType.Standard);

            taunting_stance.AddComponent(Helpers.PrerequisiteClassLevel(barbarian_class, 12));
            taunting_stance.Groups = new FeatureGroup[] { FeatureGroup.RagePower };
            addToSelection(taunting_stance);
        }
    }
}
