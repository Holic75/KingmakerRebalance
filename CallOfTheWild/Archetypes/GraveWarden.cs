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
    public class GraveWarden
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature holy_water_sprinkler;
        static public BlueprintFeature death_ward;
        static public BlueprintFeature dustbringer;

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var slayer_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "GraveWardenArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Grave Warden");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While paladins and inquisitors use their connection with the divine to fight undead hordes and other horrors of the night, a grave warden relies on knowledge, skill with weapons, and tenacity to put an end to these night-born terrors.");
            });
            Helpers.SetField(archetype, "m_ParentClass", slayer_class);
            library.AddAsset(archetype, "");

            var slayer_talent2 = library.Get<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118");
            var slayer_talent10 = library.Get<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(2, slayer_talent2),
                                                          Helpers.LevelEntry(10, slayer_talent10)
                                                       };

            createHolyWaterSprinkerAndDeathWard();
            createDustBringer();

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(2, holy_water_sprinkler),
                                                       Helpers.LevelEntry(7, death_ward),
                                                       Helpers.LevelEntry(10, dustbringer)
                                                    };

            slayer_class.Progression.UIGroups = slayer_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(holy_water_sprinkler, death_ward, dustbringer));
            slayer_class.Archetypes = slayer_class.Archetypes.AddToArray(archetype);
        }


        static void createDustBringer()
        {
            
            var slayer_class = library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");

            var icon = Helpers.GetIcon("a9a52760290591844a96d0109e30e04d"); //undeath to death
            var assasinate_buff = Helpers.CreateBuff("GraveWardenDustbringerBuff",
                                                     "Dustbringer Target",
                                                     "At 10th level, a grave warden can slay a studied undead opponent. This functions as the assassinate slayer talent, except it requires a successful Will saving throw instead of a successful Fortitude save, affects only undead, and destroys the target instead of killing it. If the target succeeds at its saving throw, it becomes immune to that grave warden’s dustbringer ability for 24 hours.\n"
                                                     + "Assasiante: " + RogueTalents.assasinate.Description,
                                                     "",
                                                     icon,
                                                     null);
            assasinate_buff.Stacking = StackingType.Stack;

            var assasinate_cooldown = Helpers.CreateBuff("GraveWardenDustbringerCooldownBuff",
                                         "Dustbringer Target Cooldown",
                                         assasinate_buff.Description,
                                         "",
                                         icon,
                                         null);
            assasinate_cooldown.Stacking = StackingType.Stack;
            var apply_cooldown = Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(assasinate_cooldown), null,
                                                           Common.createContextActionApplyBuff(assasinate_cooldown, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false));
            var apply_buff = Common.createContextActionApplyBuff(assasinate_buff, Helpers.CreateContextDuration(1, DurationRate.Rounds), dispellable: false);

            var ability = Helpers.CreateAbility("GraveWardenDustbringerAbility",
                                                "Dustbringer",
                                                assasinate_buff.Description,
                                                "",
                                                assasinate_buff.Icon,
                                                AbilityType.Extraordinary,
                                                UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.oneRoundDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Common.createAbilityTargetHasNoFactUnlessBuffsFromCaster(new BlueprintBuff[] { assasinate_cooldown, assasinate_buff }),
                                                Common.createAbilityTargetHasFact(false, Common.undead)
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful();
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            //Common.setAsFullRoundAction(ability);

            var attempt_assasinate = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsFlatFooted>(), Common.createContextConditionHasBuffFromCaster(assasinate_buff)),
                                                               Helpers.CreateActionSavingThrow(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, Helpers.Create<ContextActionKillTarget>()))
                                                               );
            dustbringer = Helpers.CreateFeature("GraveWardenDustbringerFeature",
                                               ability.Name,
                                               ability.Description,
                                               "",
                                               ability.Icon,
                                               FeatureGroup.None,
                                               Helpers.CreateAddFact(ability),
                                               Helpers.Create<AddInitiatorAttackRollTrigger>(a =>
                                               {
                                                   a.OnlyHit = true;
                                                   a.SneakAttack = true;
                                                   a.Action = Helpers.CreateActionList(attempt_assasinate);
                                               }
                                                                                            ),
                                               Helpers.Create<AddInitiatorAttackRollTrigger>(a =>
                                               {
                                                   a.OnlyHit = false;
                                                   a.Action = Helpers.CreateActionList(apply_cooldown, Common.createContextActionRemoveBuffFromCaster(assasinate_buff));
                                               }
                                                                                            ),
                                               Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { slayer_class }, StatType.Intelligence)
                                               );

            Common.addToSlayerStudiedTargetDC(dustbringer);
        }


        static void createHolyWaterSprinkerAndDeathWard()
        {
            var action = Helpers.CreateConditional(Common.createContextConditionHasFact(Common.undead), Helpers.CreateActionDealDamage(DamageEnergyType.Holy, Helpers.CreateContextDiceValue(DiceType.D4, 2)));
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/HolyWaterJet.png");
            var buff = Helpers.CreateBuff("HolyWaterSprinklerBuff",
                                         "Holy Water Sprinkler",
                                         "As a swift action, a grave warden can open a flask of holy water and pour it onto a held or adjacent melee weapon. If the weapon successfully hits an undead creature before the end of the grave warden’s next turn, the undead takes damage as if it took a direct hit from the holy water, taking 2d4 points of damage in addition to the damage from the weapon, if any.\n"
                                         + "A grave warden can use this ability once per day per grave warden level.",
                                         "",
                                         icon,
                                         null,
                                         Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(action), check_weapon_range_type: true, wait_for_attack_to_resolve: true)
                                         );
            var resource = Helpers.CreateAbilityResource("GraveWardenHolyWaterResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, new BlueprintCharacterClass[] { archetype.GetParentClass() });
            var ability = Helpers.CreateActivatableAbility("HolyWaterSprinklerAbility",
                                                 buff.Name,
                                                 buff.Description,
                                                 "",
                                                 buff.Icon,
                                                 buff,
                                                 AbilityActivationType.Immediately,
                                                 CommandType.Swift,
                                                 null,
                                                 resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound),
                                                 Common.createActivatableAbilityUnitCommand(CommandType.Swift)
                                                 );

            holy_water_sprinkler = Common.ActivatableAbilityToFeature(ability, false);
            holy_water_sprinkler.AddComponent(resource.CreateAddAbilityResource());

            var death_ward_ability = Common.convertToSpellLike(library.Get<BlueprintAbility>("0413915f355a38146bc6ad40cdf27b3f"),
                                                               "GraveWarden",
                                                               new BlueprintCharacterClass[] { archetype.GetParentClass() },
                                                               StatType.Intelligence,
                                                               no_resource: true
                                                               );
            death_ward_ability.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(death_ward_ability);
            death_ward_ability.Range = AbilityRange.Personal;

            death_ward_ability.AddComponent(resource.CreateResourceLogic(amount: 4));

            death_ward = Common.AbilityToFeature(death_ward_ability, false);
            death_ward.SetDescription("At 7th level, a grave warden learns to perform a short ritual that grants the benefits of death ward, using his slayer level as his caster level. Performing this ritual requires a full-round action and consumes 4 uses of Holy Water Sprinkler ability. The grave warden can protect only himself with this ability.");

        }
    }
}
