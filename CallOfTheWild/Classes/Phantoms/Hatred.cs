using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public partial class Phantom
    {
        static void createHatred()
        {
            var weapon_finesse = library.Get<BlueprintFeature>("90e54424d682d104ab36436bd527af09");
            var sneak_attack_feature = library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87");
            var hated_target_buff = Helpers.CreateBuff("HatredPhantomHatedTargetBuff",
                                                       "Hated Target",
                                                       "The phantom can take a move action to designate one creature within its line of sight as a hated target. The phantom gains a +2 bonus on attack rolls against its hated target, and a bonus on damage rolls equal to 1/2 the phantom’s Hit Dice (minimum 1). The phantom can maintain these bonuses against only one target at a time, and these bonuses remain in effect until either the hated opponent is dead or it has been out of the phantom’s line of sight for at least 1 minute. When the spiritualist reaches 7th level, the phantom can use this ability as a swift action.",
                                                       "",
                                                       NewSpells.howling_agony.Icon,
                                                       Common.createPrefabLink("8de64fbe047abc243a9b4715f643739f"),
                                                       Helpers.Create<AttackBonusAgainstTarget>(a => { a.Value = 2; a.CheckCaster = true; }),
                                                       Helpers.Create<DamageBonusAgainstTarget>(a => { a.Value = Helpers.CreateContextValue(AbilityRankType.Default); a.CheckCaster = true; }),
                                                       Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getPhantomArray(), progression: ContextRankProgression.Div2,
                                                                                       min: 1),
                                                       Helpers.Create<UniqueBuff>()
                                                       );
            hated_target_buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var hated_target_move = Helpers.CreateAbility("HatredPhantomHatedTargetAbility",
                                                          hated_target_buff.Name,
                                                          hated_target_buff.Description,
                                                          "",
                                                          hated_target_buff.Icon,
                                                          AbilityType.Supernatural,
                                                          UnitCommand.CommandType.Move,
                                                          AbilityRange.Medium,
                                                          "",
                                                          "",
                                                          Helpers.CreateRunActions(Common.createContextActionApplyBuff(hated_target_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false))
                                                          );
            hated_target_move.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            var hated_target = Common.AbilityToFeature(hated_target_move);
            var hated_target_swift = library.CopyAndAdd(hated_target_move, "HatredPhantomHatedTargetSwiftAbility", "");
            hated_target_swift.ActionType = UnitCommand.CommandType.Swift;
            hated_target_swift.SetName(hated_target_buff.Name + " (Swift)");

            var dmg = Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default)), IgnoreCritical: true);
            var hateful_aura_effect_buff = Helpers.CreateBuff("HatredPhantomHatefulAuraEffectBuff",
                                                              "Hateful Aura",
                                                              "When the spiritualist reaches 7th level, as a swift action, the phantom can emit a 20-foot-radius aura that inflicts pain upon the minds of those who dare damage it or its master. Enemies within the aura that deal damage to the spiritualist or the phantom take an amount of damage equal to the phantom’s Charisma bonus. This is a mind-affecting pain effect.",
                                                              "",
                                                              Helpers.GetIcon("b48674cef2bff5e478a007cf57d8345b"), //remove curse
                                                              null,
                                                              Helpers.Create<CompanionMechanics.AddOutgoingDamageTriggerOnAttackerOfPetOrMaster>(a =>
                                                              {
                                                                  a.Actions = Helpers.CreateActionList(dmg);
                                                              }),
                                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Charisma),
                                                              Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Death)
                                                              );

            var toggle = Common.createToggleAreaEffect(hateful_aura_effect_buff, 20.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()),
                                                      AbilityActivationType.WithUnitCommand,
                                                      UnitCommand.CommandType.Swift,
                                                      Common.createPrefabLink("8a80d991f3d68e84293e098a6faa7620"), //unholy
                                                      null
                                                      );
            toggle.DeactivateIfOwnerDisabled = true;
            var hateful_aura = Common.ActivatableAbilityToFeature(toggle, false);
            hateful_aura.AddComponent(Helpers.CreateAddFact(hated_target_swift));




            var sneak_attack = Helpers.CreateFeature("HatredPhantomSneakAttackFeature",
                                                     "Sneak Attack",
                                                     "When the spiritualist reaches 12th level, the phantom gains sneak attack +3d6, but only against its hated enemy. At 18th level, the sneak attack damage increases to +5d6.",
                                                     "",
                                                     sneak_attack_feature.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddContextStatBonus(StatType.SneakAttack, ModifierDescriptor.Feat),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getPhantomArray(), 
                                                                                     progression: ContextRankProgression.Custom,
                                                                                     customProgression: new (int, int)[] {(13, 3), (20, 5) }
                                                                                     )
                                                     );



            var shared_hatred_buff = Helpers.CreateBuff("HatredPhantomSharedHatredBuff",
                                           "Shared Hatred",
                                           "When the spiritualist reaches 17th level, the phantom can grant its hatred to others. When the phantom designates a hated enemy, its allies also gain a +2 bonus on attack rolls and a +4 bonus on damage rolls against that enemy. ",
                                           "",
                                           Helpers.GetIcon("08cb5f4c3b2695e44971bf5c45205df0"), //scare
                                           null,
                                           Helpers.Create<AttackBonusAgainstTarget>(a => { a.Value = 2; a.CheckCasterFriend = true; }),
                                           Helpers.Create<DamageBonusAgainstTarget>(a => { a.Value = 4; a.CheckCasterFriend = true; })
                                           );

            var shared_hatred = Helpers.CreateFeature("HatredPhantomSharedHatredFeature",
                                         shared_hatred_buff.Name,
                                         shared_hatred_buff.Description,
                                         "",
                                         shared_hatred_buff.Icon,
                                         FeatureGroup.None
                                         );

            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(shared_hatred_buff,
                                                                                      Helpers.CreateConditional(Common.createContextConditionCasterHasFact(shared_hatred),
                                                                                                                Common.createContextActionApplyBuff(shared_hatred_buff, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true))
                                                                                      );
            var add_weapon_finesse = Common.featureToFeature(weapon_finesse, false);
            add_weapon_finesse.SetDescription("The phantom gains Weapon Finesse as a bonus feat.");
            var hatred_archetype = createPhantomArchetype("HatredPhantomArchetype",
                                                         "Hatred",
                                                         true,
                                                         true,
                                                         false,
                                                         new StatType[] { StatType.SkillMobility, StatType.SkillPerception },
                                                         new LevelEntry[] { Helpers.LevelEntry(1, add_weapon_finesse)},
                                                         new LevelEntry[] { Helpers.LevelEntry(1) }
                                                         );
            //ill omen, inflcit pain, bestow curse, ?debilitating portent?, inflict pain mass, harm
            createPhantom("Hatred",
                          "Hatred",
                          "Few things draw a spirit toward the Negative Material Plane like the emotion of hatred. Phantoms with this focus often are blinded by—and blind others with—this powerful emotion. These phantoms are frequently darker and more foreboding than all but those phantoms with the fear emotional focus. Many of them manifest as dark and dreadful knights, their armor bristling with spikes and their hands seeming to grasp barbed and terrible weapons. Other times they appear as tall, gaunt figures staring down arrogantly at those who approach.\n"
                          + "These phantoms typically spew a string of curses at their foes, often profane, sometimes poetic. Their auras are pulsating and pitch black, and thrum violently when these horrible phantoms attack.\n"
                          + "Skills: The phantom gains a number of ranks in Mobility and Perception equal to its number of Hit Dice. While confined in the spiritualist’s consciousness, the phantom grants the spiritualist Skill Focus in each of these skills.\n"
                          + "Good Saves: Fortitude and Reflex.\n"
                          + "Weapon Finesse: The phantom gains Weapon Finesse as a bonus feat.",
                          shared_hatred.Icon,
                          hatred_archetype,
                          hated_target, hateful_aura, sneak_attack, shared_hatred,
                          new StatType[] { StatType.SkillMobility, StatType.SkillPerception },
                          12, 14,
                          new BlueprintAbility[]
                          {
                              library.Get<BlueprintAbility>("fbdd8c455ac4cde4a9a3e18c84af9485"), //doom
                              NewSpells.inflict_pain,
                              library.Get<BlueprintAbility>("989ab5c44240907489aba0a8568d0603"), //bestow curse,
                              library.Get<BlueprintAbility>("f34fb78eaaec141469079af124bcfa0f") //enervation,
                          }
                          );
        }
    }
}
