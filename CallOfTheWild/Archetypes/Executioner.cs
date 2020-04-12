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
    class Executioner
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature focused_killer;
        static public BlueprintFeature pain_strike;
        static public BlueprintFeature assasinate;
        static public BlueprintFeatureSelection bloodstained_hands; 

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var slayer_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ExecutionerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Executioner");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "These professional killers are trained and used almost exclusively by the various crime families. Streetwise and ruthless, they operate independently, often making their talents available to the highest bidder. Though a few unscrupulous executioners exploit this arrangement and force families to pay higher rates for their services, most of these assassins follow a strict personal code and maintain absolute transparency with their employers.");
            });
            Helpers.SetField(archetype, "m_ParentClass", slayer_class);
            library.AddAsset(archetype, "");

            var slayer_talent4 = library.Get<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118");
            var slayer_talent10 = library.Get<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66");

            var quarry = library.Get<BlueprintFeature>("385260ca07d5f1b4e907ba22a02944fc");
            var improved_quarry = library.Get<BlueprintFeature>("25e009b7e53f86141adee3a1213af5af");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(4, slayer_talent4),
                                                          Helpers.LevelEntry(10, slayer_talent10),
                                                          Helpers.LevelEntry(14, quarry),
                                                          Helpers.LevelEntry(19, improved_quarry),
                                                       };

            createBloodstainedHands();
            createFocusedKiller();
            createPainStrike();

            assasinate = Helpers.CreateFeature("ExecutionerAssasinateFeature",
                                               RogueTalents.assasinate.Name,
                                               "At 10th level, an executioner must select the assassinate advanced slayer talent.",
                                               "",
                                               RogueTalents.assasinate.Icon,
                                               FeatureGroup.None,
                                               Helpers.CreateAddFact(RogueTalents.assasinate)
                                               );

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, focused_killer, bloodstained_hands),
                                                       Helpers.LevelEntry(4, pain_strike),
                                                       Helpers.LevelEntry(10, assasinate),
                                                       Helpers.LevelEntry(14, RogueTalents.swift_death),
                                                    };

            slayer_class.Progression.UIDeterminatorsGroup = slayer_class.Progression.UIDeterminatorsGroup.AddToArray(bloodstained_hands);
            slayer_class.Progression.UIGroups = slayer_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(focused_killer, pain_strike, assasinate, RogueTalents.swift_death));
            slayer_class.Archetypes = slayer_class.Archetypes.AddToArray(archetype);
        }


        static void createBloodstainedHands()
        {
            var bloodstained_hands1 = Helpers.CreateFeature("BloodstainedHandsFeature",
                                                            "Bloodstained Hands",
                                                            "An executioner must be evil.",
                                                            "",
                                                            null,
                                                            FeatureGroup.None,
                                                            Common.createPrerequisiteAlignment(AlignmentMaskType.Evil));

            bloodstained_hands = Helpers.CreateFeatureSelection("BloodstainedHandsFeatureSelection",
                                                                bloodstained_hands1.Name,
                                                                bloodstained_hands1.Description,
                                                                "",
                                                                bloodstained_hands1.Icon,
                                                                FeatureGroup.None);
            bloodstained_hands.AllFeatures = bloodstained_hands.AllFeatures.AddToArray(bloodstained_hands1);
        }


        static void createPainStrike()
        {
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");          

            var apply_buff = Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(0, DurationRate.Rounds, DiceType.D4, 1), dispellable: false);
            var apply_buff_saved = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_buff));
            var buff = Helpers.CreateBuff("ExecutionerPainStrikeBuff",
                                          "Painful Strike",
                                          "Executioners are trained to cause excruciating pain when striking targets, often leaving them reeling in agony or completely incapacitated as they slowly bleed out. A creature that takes sneak attack damage from an executioner must make a successful a Fortitude save (DC = 10 + 1/2 the executioner’s class level + his Intelligence modifier) or become sickened for 1d4 rounds.",
                                          "",
                                          sickened.Icon,
                                          null,
                                          Helpers.Create<AddInitiatorAttackRollTrigger>(a =>
                                              {
                                                    a.OnlyHit = true;
                                                    a.SneakAttack = true;
                                                    a.Action = Helpers.CreateActionList(apply_buff_saved);
                                              }
                                          ),
                                          Common.createContextCalculateAbilityParamsBasedOnClass(archetype.GetParentClass(), StatType.Intelligence)
                                          );
            Common.addToSlayerStudiedTargetDC(buff);

            var toggle = Helpers.CreateActivatableAbility("ExecutionerPainStrikeToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          UnitCommand.CommandType.Free,
                                                          null
                                                          );
            toggle.Group = ActivatableAbilityGroupExtension.SneakAttack.ToActivatableAbilityGroup();
            toggle.DeactivateImmediately = true;

            pain_strike = Common.ActivatableAbilityToFeature(toggle, false);
            pain_strike.SetDescription("At 4th level, an executioner automatically gains this talent. " + toggle.Description);
        }


        static void createFocusedKiller()
        {
            var non_humanoids = new BlueprintUnitFact[] {Common.dragon, Common.undead, Common.elemental, Common.magical_beast, Common.animal, Common.monstrous_humanoid,
                                                         Common.aberration, Common.plant};


            var studied_target_buff = library.Get<BlueprintBuff>("45548967b714e254aa83f23354f174b0");

            focused_killer = Helpers.CreateFeature("FocusedKillerFeature",
                                                   "Focused Killer",
                                                   "At 1st level, an executioner’s studied target bonuses increase by 1 against humanoid opponents, but decrease by 1 against non-humanoid targets.",
                                                   "",
                                                   RogueTalents.bleeding_attack.Icon,
                                                   FeatureGroup.None);

            var focused_killer_action = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(focused_killer),
                                                                  Helpers.CreateConditional(Common.createContextConditionHasFacts(false, non_humanoids),
                                                                                              Helpers.Create<ContextActionChangeSharedValue>(c =>
                                                                                              {
                                                                                                  c.Type = SharedValueChangeType.Add;
                                                                                                  c.AddValue = -1;
                                                                                                  c.SharedValue = AbilitySharedValue.DamageBonus;
                                                                                              }),
                                                                                              Helpers.Create<ContextActionChangeSharedValue>(c =>
                                                                                              {
                                                                                                  c.Type = SharedValueChangeType.Add;
                                                                                                  c.AddValue = 1;
                                                                                                  c.SharedValue = AbilitySharedValue.DamageBonus;
                                                                                              })
                                                                                            )
                                                                   );
            studied_target_buff.ReplaceComponent<AddFactContextActions>(a => a.Activated = Helpers.CreateActionList(a.Activated.Actions.AddToArray(focused_killer_action)));
        }
    }
}
