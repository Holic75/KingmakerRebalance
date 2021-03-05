using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class VersatilePerformance
    {
        static LibraryScriptableObject library => Main.library;
        static List<BlueprintFeature> versatile_performances = new List<BlueprintFeature>();
        static BlueprintFeatureSelection martial_performance;

        static public BlueprintFeatureSelection masterpiece_selection;

        static public BlueprintFeature clamor_of_hevens; //blind/deafen evil,  stun/stagger undead//
        static public BlueprintFeature dance_of_23_steps; //dodge ac bonus //
        //static public BlueprintFeature dumbshow_of_garroc; //damage plant ooze
        static public BlueprintFeature symphony_of_elysian_heart; //freedom of movement //
        static public BlueprintFeature triple_time; //+10 feet speed bonus for 1 hour
        static public BlueprintFeature banshees_requiem; // +2 negative levels //
        static public BlueprintFeature blazing_rondo; //haste + fatigue //
        static public BlueprintBuff blazing_rondo_buff;
        static public BlueprintBuff symphony_of_elysian_heart_buff;

        static BlueprintCharacterClass bard_class = library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

        static BlueprintAbilityResource performance_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
        static public BlueprintFeatureSelection rogue_talents;

        static internal void create()
        {
            createMartialPerformance();
            createVersatilePerformanceAndExpandedVersality();
            createMasterpieces();
            var versatile_perfrormance = library.Get<BlueprintFeatureSelection>("94e2cd84bf3a8e04f8609fe502892f4f");
            versatile_perfrormance.AllFeatures = versatile_performances.ToArray().AddToArray(martial_performance).AddToArray(masterpiece_selection);
            versatile_perfrormance.SetNameDescription("Versatile Performance",
                                                      versatile_performances[0].Description);

            Skald.versatile_performance.SetNameDescription(versatile_perfrormance.Name, versatile_perfrormance.Description);
            Skald.versatile_performance.AllFeatures = versatile_perfrormance.AllFeatures;

            fixArchaelogist();
        }


        static void createMasterpieces()
        {
            masterpiece_selection = Helpers.CreateFeatureSelection("BardicMasterpiecesFeatureSelection",
                                                                   "Bardic Masterpieces",
                                                                   "Talented bards can learn or create masterpieces, unusual applications of the bardic performance ability requiring special training.",
                                                                   "",
                                                                   null,
                                                                   FeatureGroup.Feat,
                                                                   Helpers.PrerequisiteClassLevel(bard_class, 3, any: true),
                                                                   Helpers.PrerequisiteClassLevel(Skald.skald_class, 3, any: true));
            createClamorOfHeavens();
            createDanceOf23Steps();
            createSymphonyOfElysianHeart();
            createBansheesRequiem();
            createBlazingRondo();
            createTripleTime();

            masterpiece_selection.AllFeatures = masterpiece_selection.AllFeatures.AddToArray(clamor_of_hevens, dance_of_23_steps, symphony_of_elysian_heart, banshees_requiem, blazing_rondo, triple_time);

            library.AddFeats(masterpiece_selection);
        }


        static void createTripleTime()
        {
            var buff = library.Get<BlueprintBuff>("035ed56eb973f0e469a288ff5991c9ff"); //longstrider

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Hours), dispellable: false);

            var ability = Helpers.CreateAbility("TripleTimeAbility",
                                                "Triple Time",
                                                "Effect: This bright and spritely tune mimics the sound of human feet, slowly building to a steady, ground-eating pace. When you complete this performance, you affect one ally in hearing range per bard level. This masterpiece increases the affected target’s base land speed by 10 feet for 1 hour.",
                                                "",
                                                buff.Icon,
                                                AbilityType.Extraordinary,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                "1 hour",
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Ally),
                                                performance_resource.CreateResourceLogic(),
                                                Common.createAbilitySpawnFx("20d09f919accddf41bde3820341d08b7", anchor: Kingmaker.UnitLogic.Abilities.Components.Base.AbilitySpawnFxAnchor.SelectedTarget),
                                                Common.createAbilityCasterHasNoFacts(NewSpells.silence_buff),
                                                Common.createAbilityTargetHasFact(true, NewSpells.silence_buff)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(ability);

            triple_time = Common.AbilityToFeature(ability, false);
            triple_time.Groups = new FeatureGroup[] { FeatureGroup.Feat };
            triple_time.AddComponents(Helpers.PrerequisiteClassLevel(bard_class, 3, any: true),
                                            Helpers.PrerequisiteClassLevel(Skald.skald_class, 3, any: true)); ;
        }


        static void createBlazingRondo()
        {
            var haste_buff = library.Get<BlueprintBuff>("03464790f40c3c24aa684b57155f3280"); //haste
            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var apply_haste = Common.createContextActionApplyBuff(haste_buff, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true);
            var apply_fatigue = Common.createContextActionApplyBuff(fatigued, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilitySharedValue.Duration)), dispellable: false);
            var fatigue_saved = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_fatigue));

            var effect_buff = Helpers.CreateBuff("BlazingRondoEffectBuff",
                                    "Blazing Rondo",
                                    "You and your allies gain the benefits of haste while you maintain this masterpiece, except the bonus to AC and on attack rolls and Reflex saves is one-fifth of your bard level. Allies must be within 50 feet of you to receive this benefit. When you cease performing this masterpiece, any creature that received this benefit must succeed at a Fortitude save at this masterpiece’s DC or be fatigued for twice as many rounds as they were affected.\n"
                                    + "Use: 1 round of bardic performance per round.",
                                    "",
                                    haste_buff.Icon,
                                    null,
                                    Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable),
                                    Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Dodge),
                                    Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.UntypedStackable),
                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] {bard_class, Skald.skald_class},
                                                                    progression: ContextRankProgression.DelayedStartPlusDivStep, 
                                                                    startLevel: 10, stepLevel: 5),
                                    Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { bard_class, Skald.skald_class }, StatType.Charisma),
                                    Helpers.CreateAddFactContextActions(activated: apply_haste,
                                                                        newRound: Helpers.Create<ContextActionChangeSharedValue>(c => { c.AddValue = 2; c.Type = SharedValueChangeType.Add; c.SharedValue = AbilitySharedValue.Duration; }),
                                                                        deactivated: fatigue_saved)
                                    );
            var toggle = Common.createToggleAreaEffect(effect_buff, 50.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()),
                                          AbilityActivationType.WithUnitCommand,
                                          UnitCommand.CommandType.Standard,
                                          Common.createPrefabLink("5d4308fa344af0243b2dd3b1e500b2cc"), //inspire courage
                                          Common.createPrefabLink("9353083f430e5a44a8e9d6e26faec248")
                                          );
            toggle.Group = ActivatableAbilityGroup.BardicPerformance;
            toggle.AddComponent(performance_resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.NewRound));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            blazing_rondo = Common.ActivatableAbilityToFeature(toggle, false);
            blazing_rondo.Groups = new FeatureGroup[] { FeatureGroup.Feat };
            blazing_rondo.AddComponents(Helpers.PrerequisiteClassLevel(bard_class, 7, any: true),
                                            Helpers.PrerequisiteClassLevel(Skald.skald_class, 7, any: true));
            blazing_rondo_buff = effect_buff;
        }


        static void createBansheesRequiem()
        {
            var effect = Helpers.CreateActionEnergyDrain(Helpers.CreateContextDiceValue(DiceType.Zero, 0, 2), Helpers.CreateContextDuration(1, DurationRate.Hours), EnergyDrainType.Permanent);

            var effect_saved = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, effect));
            var effect_buff = Helpers.CreateBuff("BansheesRequiemBuff",
                                                 "Banshee’s Requiem",
                                                 "Effect: All living creatures you select within 30 feet at the start of your turn each round gain 2 negative levels unless they succeed at a Fortitude saving throw (DC = 10 + 1/2 your bard level + your Charisma bonus). This is a death effect and a sonic effect. This performance has audible components.\n"
                                                 + "Use: 3 rounds of bardic performance per round.",
                                                 "",
                                                 Helpers.GetIcon("b24583190f36a8442b212e45226c54fc"), //banshee blast
                                                 null,
                                                 Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { bard_class, Skald.skald_class }, StatType.Charisma),
                                                 Helpers.CreateAddFactContextActions(activated: effect_saved, newRound: effect_saved),
                                                 Helpers.CreateSpellDescriptor(SpellDescriptor.Death | SpellDescriptor.Sonic)
                                                 );

            var toggle = Common.createToggleAreaEffect(effect_buff, 30.Feet(), Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFact(Common.undead, has: false),
                                                                                                                 Common.createContextConditionHasFact(Common.elemental, has: false),
                                                                                                                 Common.createContextConditionHasFact(Common.construct, has: false),
                                                                                                                 Helpers.Create<ContextConditionIsEnemy>()),
                                                      AbilityActivationType.WithUnitCommand,
                                                      UnitCommand.CommandType.Standard,
                                                      Common.createPrefabLink("20caf000cd4c3434da00a74f4a49dccc"), //dirge of doom
                                                      Common.createPrefabLink("39da71647ad4747468d41920d0edd721") //dirge of doom
                                                      );
            toggle.Group = ActivatableAbilityGroup.BardicPerformance;
            toggle.Buff.AddComponent(Helpers.CreateAddFactContextActions(activated: Helpers.Create<ResourceMechanics.ContextActionSpendResourceFromCaster>(a =>
            {
                a.resource = performance_resource;
                a.amount = 3;
            })));
            toggle.AddComponent(performance_resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.NewRound));
            toggle.AddComponent(Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = performance_resource; r.amount = 3; }));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            banshees_requiem = Common.ActivatableAbilityToFeature(toggle, false);
            banshees_requiem.Groups = new FeatureGroup[] { FeatureGroup.Feat };
            banshees_requiem.AddComponents(Helpers.PrerequisiteClassLevel(bard_class, 17, any: true),
                                            Helpers.PrerequisiteClassLevel(Skald.skald_class, 17, any: true));
        }


        static void createSymphonyOfElysianHeart()
        {
            var buff = library.CopyAndAdd<BlueprintBuff>("1533e782fca42b84ea370fc1dcbf4fc1", "SymphonyOfElysianHeartEffectBuff", ""); //freedom of movement

            var toggle = Common.createToggleAreaEffect(buff, 30.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()),
                                          AbilityActivationType.WithUnitCommand,
                                          UnitCommand.CommandType.Standard,
                                          Common.createPrefabLink("79665f3d500fdf44083feccf4cbfc00a"), //inspire competence area
                                          Common.createPrefabLink("9353083f430e5a44a8e9d6e26faec248")
                                          );
            toggle.Group = ActivatableAbilityGroup.BardicPerformance;
            toggle.SetNameDescription("Symphony of the Elysian Heart",
                                      "Effect: The complex arpeggios in this piece follow each other so quickly that the music can sound jumbled and disjointed at first. As the piece progresses, however, distinct phrases emerge, creating a wild but harmonious piece that inspires feelings of unfettered freedom. You and your allies within 30 feet who can hear you can move and attack normally for the duration of your performance, even if under the influence of magic that usually impedes movement. This effect is identical to that of freedom of movement, except that this masterpiece does not allow subjects to move and attack normally while underwater unless these creatures would already be able to do so, and only lasts as long as you continue the performance.\n"
                                      + "Use: 1 bardic performance round per round."
                                      );

            toggle.AddComponent(performance_resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.NewRound));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            symphony_of_elysian_heart = Common.ActivatableAbilityToFeature(toggle, false);
            symphony_of_elysian_heart.Groups = new FeatureGroup[] { FeatureGroup.Feat };
            symphony_of_elysian_heart.AddComponents(Helpers.PrerequisiteClassLevel(bard_class, 7, any: true),
                                            Helpers.PrerequisiteClassLevel(Skald.skald_class, 7, any: true));

            symphony_of_elysian_heart_buff = buff;
        }


        static void createDanceOf23Steps()
        {
            var bard_class = library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var buff = Helpers.CreateBuff("DanceOf23StepsBuff",
                                     "The Dance of 23 Steps",
                                     "Effect: The shuffling steps, bends, and leaps of this intricate dance make you a difficult target to hit, but also make it more difficult for you to perform other actions. When using this masterpiece, you take a –2 penalty on melee attack rolls and combat maneuver checks, and you must make a concentration check to cast any spell (DC 15 + the spell’s level), but you gain a +2 dodge bonus to your Armor Class. When you have 8 bard levels, and every 4 levels thereafter, the penalty increases by –1 and the dodge bonus increases by +1. You can combine this masterpiece with fighting defensively and Combat Expertise, but not total defense. When you use this masterpiece, it lasts until the start of your next turn. Abilities that extend the duration of a bardic performance (such as Lingering Performance) affect this masterpiece; this allows you to get multiple rounds of its benefit (and its penalties) at the cost of only 1 round of bardic performance."
                                     +"Use: 1 bardic performance round. Starting this performance is free action.",
                                     "",
                                     NewSpells.irresistible_dance.Icon,
                                     Common.createPrefabLink("91ef30ab58fa0d3449d4d2ccc20cb0f8"),
                                     Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.None, multiplier: -1),
                                     Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Dodge),
                                     Common.createAddCondition(UnitCondition.SpellCastingIsDifficult),
                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.OnePlusDivStep,
                                                                      classes: new BlueprintCharacterClass[] {bard_class, Skald.skald_class},
                                                                      min: 2, stepLevel: 4)
                                     );

            var toggle = Helpers.CreateActivatableAbility(buff.name + "ToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          UnitCommand.CommandType.Free,
                                                          null);
            toggle.Group = ActivatableAbilityGroup.BardicPerformance;
            toggle.AddComponent(performance_resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.NewRound));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            dance_of_23_steps = Common.ActivatableAbilityToFeature(toggle, false);
            dance_of_23_steps.Groups = new FeatureGroup[] { FeatureGroup.Feat };
            dance_of_23_steps.AddComponents(Helpers.PrerequisiteClassLevel(bard_class, 4, any: true),
                                            Helpers.PrerequisiteClassLevel(Skald.skald_class, 4, any: true));
        }


        static void createClamorOfHeavens()
        {
            var bard_class = library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var undead_stunned = library.CopyAndAdd<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3", "UndeadStunnedBuff", "");
            undead_stunned.RemoveComponents<SpellDescriptorComponent>();

            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var blinded = library.Get<BlueprintBuff>("0ec36e7596a4928489d2049e1e1c76a7");
            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");

            var apply_stun = Common.createContextActionApplyBuff(undead_stunned, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true, is_child: true);
            var apply_stagger = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true, is_child: true);
            var apply_blind = Common.createContextActionApplyBuff(blinded, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true, is_child: true);
            var apply_shaken = Common.createContextActionApplyBuff(shaken, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true, is_child: true);

            var effect = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, Common.undead, Common.outsider),
                                                   Helpers.CreateConditionalSaved(apply_stagger, apply_stun),
                                                   Helpers.CreateConditionalSaved(apply_shaken, apply_blind)
                                                   );
            var effect_saved = Helpers.CreateActionSavingThrow(SavingThrowType.Will, effect);
            var effect_buff = Helpers.CreateBuff("ClamorOfHeavensEffectBuff",
                                                 "Clamor Of the Heavens",
                                                 "Effect: Evil creatures that hear the performance and fail a Will save against the effect are blinded and deafened for the duration. On a successful save, they are shaken instead. Undead or creatures with the evil subtype that fail their saves are stunned for the duration, while those that succeed are staggered.\n"
                                                 + "Use: 3 bardic performance rounds, +1 round per additional round of duration.",
                                                 "",
                                                 Helpers.GetIcon("574cf074e8b65e84d9b69a8c6f1af27b"),
                                                 null,
                                                 Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { bard_class, Skald.skald_class }, StatType.Charisma),
                                                 Helpers.CreateAddFactContextActions(activated: effect_saved)
                                                 );

            var toggle = Common.createToggleAreaEffect(effect_buff, 30.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.CreateContextConditionAlignment(AlignmentComponent.Evil)),
                                                      AbilityActivationType.WithUnitCommand,
                                                      UnitCommand.CommandType.Standard,
                                                      Common.createPrefabLink("dfc59904273f7ee49ab00e5278d86e16"),
                                                      Common.createPrefabLink("9353083f430e5a44a8e9d6e26faec248")
                                                      );
            toggle.Group = ActivatableAbilityGroup.BardicPerformance;
            toggle.Buff.AddComponent(Helpers.CreateAddFactContextActions(activated: Helpers.Create<ResourceMechanics.ContextActionSpendResourceFromCaster>(a =>
                                                                                                                                                           {
                                                                                                                                                               a.resource = performance_resource;
                                                                                                                                                               a.amount = 2;
                                                                                                                                                           })));
            toggle.AddComponent(performance_resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.NewRound));
            toggle.AddComponent(Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = performance_resource; r.amount = 3; }));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            clamor_of_hevens = Common.ActivatableAbilityToFeature(toggle, false);
            clamor_of_hevens.Groups = new FeatureGroup[] { FeatureGroup.Feat };
            clamor_of_hevens.AddComponents(Helpers.PrerequisiteClassLevel(bard_class, 10, any: true),
                                           Helpers.PrerequisiteClassLevel(Skald.skald_class, 10, any: true) );
        }


        static void fixArchaelogist()
        {
            var versatile_perfrormance = library.Get<BlueprintFeatureSelection>("94e2cd84bf3a8e04f8609fe502892f4f");
            var archaelogist = library.Get<BlueprintArchetype>("38384e0c1e99c2e42ac6ed70a04aca46");
            List<int> remove_vt_levels = new int[] { 2, 6, 10, 14, 18 }.ToList();
            List<int> add_rogue_talent = new int[] { 4, 8, 12, 16, 20 }.ToList();

            rogue_talents = library.CopyAndAdd<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93", "ArchaelogistRogueTalent", "");
            rogue_talents.SetDescription("At 4th level, an archaeologist gains a rogue talent. He gains an additional rogue talent for every four levels of archaeologist gained after 4th level. Otherwise, this works as the rogue’s rogue talent ability.");


            foreach (var le in archaelogist.RemoveFeatures)
            {
                if (remove_vt_levels.Contains(le.Level))
                {
                    remove_vt_levels.Remove(le.Level);
                    le.Features.Add(versatile_perfrormance);
                }
            }

            foreach (var l in remove_vt_levels)
            {
                archaelogist.RemoveFeatures = archaelogist.RemoveFeatures.AddToArray(Helpers.LevelEntry(l, versatile_perfrormance));
            }

            foreach (var le in archaelogist.AddFeatures)
            {
                if (add_rogue_talent.Contains(le.Level))
                {
                    add_rogue_talent.Remove(le.Level);
                    le.Features.Add(rogue_talents);
                }
            }


            foreach (var l in add_rogue_talent)
            {
                archaelogist.AddFeatures = archaelogist.AddFeatures.AddToArray(Helpers.LevelEntry(l, rogue_talents));
            }
        }


        static void createMartialPerformance()
        {
            var bard_class = library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            martial_performance = Helpers.CreateFeatureSelection("MartialPerformanceFeatureSelection",
                                                    "Martial Performance",
                                                    "The bard or skald selects a weapon she is proficient with. She receives a weapon focus with associated weapon category and treats her bard or skald level as half fighter level for the purpose of qualifying for combat feats.",
                                                    "",
                                                    library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e").Icon,
                                                    FeatureGroup.None,
                                                    Common.createClassLevelsForPrerequisites(fighter, bard_class, 0.5),
                                                    Common.createClassLevelsForPrerequisites(fighter, Skald.skald_class, 0.5),
                                                    Helpers.PrerequisiteClassLevel(bard_class, 6, any: true),
                                                    Helpers.PrerequisiteClassLevel(Skald.skald_class, 6, any: true)
                                                    );



            foreach (var category in Enum.GetValues(typeof(WeaponCategory)).Cast<WeaponCategory>())
            {
                var feature = Helpers.CreateFeature(category.ToString() + "MartialPerformanceFeature",
                                                    martial_performance.Name + $" ({LocalizedTexts.Instance.Stats.GetText(category)})",
                                                    martial_performance.Description,
                                                    "",
                                                    martial_performance.Icon,
                                                    FeatureGroup.None,
                                                    Common.createAddParametrizedFeatures(library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"), category),
                                                    Helpers.Create<PrerequisiteProficiency>(p =>
                                                    {
                                                        p.WeaponProficiencies = new WeaponCategory[] { category };
                                                        p.ArmorProficiencies = new ArmorProficiencyGroup[0];
                                                    }));
                martial_performance.AllFeatures = martial_performance.AllFeatures.AddToArray(feature);
            }       
        }

        static void createVersatilePerformanceAndExpandedVersality()
        {
            var bard_class = library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            var skill_foci = library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a").AllFeatures;
            var skills = new StatType[] { StatType.SkillMobility, StatType.SkillStealth, StatType.SkillPersuasion, StatType.SkillPerception, StatType.SkillUseMagicDevice };

            for (int i = 0; i < skill_foci.Length; i++)
            {
                StatType stat = skill_foci[i].GetComponent<AddContextStatBonus>().Stat;
                if (!skills.Contains(stat))
                {
                    continue;
                }

                string name = LocalizedTexts.Instance.Stats.GetText(stat);

                var feature = Helpers.CreateFeature(stat.ToString() + "VersatilePerformanceFeature",
                                                   "Versatile Performance: " + name,
                                                   "At 2nd level a bard or skald selects one of the following skills: Mobility, Stealth, Perception, Persuation or Use Magic Device. She receives a number of ranks in this skill equal to half her bard or skald level. In addition she uses charisma modifier in place of the ability modifier the skill would normally use. The bard or skald can immediately retrain all of her ranks in excess in the selected skill at no additional cost in money or time.",
                                                   "",
                                                   skill_foci[i].Icon,
                                                   FeatureGroup.None,
                                                   Helpers.Create<SkillMechanics.SetSkillRankToValue>(ss => { ss.skill = stat; ss.value = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                                   Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(r => { r.only_if_greater = false; r.NewBaseStatType = StatType.Charisma; r.StatTypeToReplaceBastStatFor = stat; }),
                                                   Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,progression: ContextRankProgression.OnePlusDiv2,
                                                                                   classes: new BlueprintCharacterClass[] {bard_class, Skald.skald_class}
                                                                                   )
                                                   );

                var feature2 = Helpers.CreateFeature(stat.ToString() + "ExpandedVersalityFeature",
                                                       "Expanded Versality: " + name,
                                                       "A bard or skald can select a skill for which she already selected versatile performance and instead gain a number of ranks in this skill equal to her bard or skald level. The bard or skald can immediately retrain all of her ranks in excess in the selected skill at no additional cost in money or time.",
                                                       "",
                                                       skill_foci[i].Icon,
                                                       FeatureGroup.None,
                                                       Helpers.Create<SkillMechanics.SetSkillRankToValue>(ss => { ss.skill = stat; ss.value = Helpers.CreateContextValue(AbilityRankType.Default); ss.increase_by1_on_apply = true; }),
                                                       Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                                                       classes: new BlueprintCharacterClass[] { bard_class, Skald.skald_class }
                                                                                       ),
                                                       Helpers.PrerequisiteFeature(feature)
                                                       );
                versatile_performances.Add(feature);
                versatile_performances.Add(feature2);
                if (stat == StatType.SkillPersuasion)
                {
                    Skald.great_orator.AllFeatures = new BlueprintFeature[] { feature };
                }
            }
        }
    }
}
