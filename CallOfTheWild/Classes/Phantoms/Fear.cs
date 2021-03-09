using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
        static void createFear()
        {
            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var frightened = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");

            var apply_shakened = Common.createContextActionApplyBuff(shaken, Helpers.CreateContextDuration(diceCount: 1, diceType: DiceType.D4), dispellable: false);
            var apply_frighted = Common.createContextActionApplyBuff(frightened, Helpers.CreateContextDuration(diceCount: 1, diceType: DiceType.D4), dispellable: false);


            var frightful_attack_buff = Helpers.CreateBuff("FearPhantomFrightfulAttackBuff",
                                                             "Frightful Attack",
                                                             "When the spiritualist reaches 12th level, if the phantom hits with a slam attack, it can frighten those it hits instead of causing them to be shaken (the phantom chooses when it makes the attack). This is a mind-affecting fear effect.",
                                                             "",
                                                             frightened.Icon,
                                                             null
                                                             );
            var frightful_attack_toggle = Helpers.CreateActivatableAbility("FearPhantomFrightfulAttackToggleAbility",
                                                                           frightful_attack_buff.Name,
                                                                           frightful_attack_buff.Description,
                                                                           "",
                                                                           frightful_attack_buff.Icon,
                                                                           frightful_attack_buff,
                                                                           AbilityActivationType.Immediately,
                                                                           UnitCommand.CommandType.Free,
                                                                           null);

            var frightful_attack = Common.ActivatableAbilityToFeature(frightful_attack_toggle, false);
            var apply_effect = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(frightful_attack_buff),
                                                              Helpers.CreateActionSavingThrow(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, apply_frighted)),
                                                              Helpers.CreateActionSavingThrow(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, apply_shakened)));

            var horryfying_strike = Helpers.CreateFeature("FearPhantomHorrifyingStrikeFeature",
                                                         "Horrifying Strike",
                                                         "If the phantom hits a creature with a slam attack, that creature must succeed at a Will saving throw (DC = 10 + 1/2 the phantom’s Hit Dice + the phantom’s Charisma modifier) or be shaken for 1d4 rounds. Multiple attacks against the same creature do not cause the creature to become frightened. This is a mind-affecting fear effect.",
                                                         "",
                                                         shaken.Icon,
                                                         FeatureGroup.None,
                                                         Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(apply_effect),
                                                                                                                      wait_for_attack_to_resolve: true,
                                                                                                                      weapon_category: WeaponCategory.OtherNaturalWeapons
                                                                                                                      ),
                                                         Common.createContextCalculateAbilityParamsBasedOnClasses(getPhantomSpiritualistArray(), StatType.Charisma),
                                                         Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Fear | SpellDescriptor.Shaken)
                                                         );

            var horryfying_strike_exciter = Helpers.CreateFeature("FearPhantomHorrifyingStrikeExciterFeature",
                                                                 "Horrifying Strike",
                                                                 "If the phantom hits a creature with a slam attack, that creature must succeed at a Will saving throw (DC = 10 + 1/2 the phantom’s Hit Dice + the phantom’s Charisma modifier) or be shaken for 1d4 rounds. Multiple attacks against the same creature do not cause the creature to become frightened. This is a mind-affecting fear effect.",
                                                                 "",
                                                                 shaken.Icon,
                                                                 FeatureGroup.None,
                                                                 Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(apply_effect),
                                                                                                                              wait_for_attack_to_resolve: true
                                                                                                                              ),
                                                                 Common.createContextCalculateAbilityParamsBasedOnClasses(getPhantomSpiritualistArray(), StatType.Charisma),
                                                                 Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Fear | SpellDescriptor.Shaken)
                                                                 );



            var immunity_to_fear_buff = library.CopyAndAdd<BlueprintBuff>("c5c86809a1c834e42a2eb33133e90a28", "FearPhantomShelterAlliesBuff", "");
            immunity_to_fear_buff.ComponentsArray = new BlueprintComponent[]
            {
                Common.createAddConditionImmunity(Kingmaker.UnitLogic.UnitCondition.Shaken),
                Common.createAddConditionImmunity(Kingmaker.UnitLogic.UnitCondition.Frightened),
                Common.createBuffDescriptorImmunity(SpellDescriptor.Shaken | SpellDescriptor.Frightened),
                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Shaken | SpellDescriptor.Frightened)
            };

            immunity_to_fear_buff.SetNameDescription("Shelter Allies",
                                                     "When the spiritualist reaches 17th level, the phantom protects its allies from terror. Allies within the phantom’s increase fear aura are immune to fear as long as they are within the aura."
                                                     );

            var shelter_allies = Helpers.CreateFeature("FearPhantomShelterAlliesFeature",
                                                       immunity_to_fear_buff.Name,
                                                       immunity_to_fear_buff.Description,
                                                       "",
                                                       immunity_to_fear_buff.Icon,
                                                       FeatureGroup.None);


            var aura_effect_enemy = Helpers.CreateActionSavingThrow(SavingThrowType.Will,
                                                                    Helpers.CreateConditionalSaved(null,
                                                                                                   Common.createContextActionApplyBuff(frightened, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true)
                                                                                                  )
                                                                    );
            var aura_effect_ally = Common.createContextActionApplyBuff(immunity_to_fear_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);

            var aura_effect = Helpers.CreateConditional(new Condition[] { Common.createContextConditionHasFact(shaken), Helpers.Create<ContextConditionIsEnemy>(), Common.createContextConditionHasFact(frightened, false) },
                                                                         aura_effect_enemy,
                                                                         Helpers.CreateConditional(new Condition[] { Common.createContextConditionCasterHasFact(shelter_allies), Helpers.Create<ContextConditionIsAlly>() },
                                                                                                   aura_effect_ally
                                                                                                   )
                                                        );
            var remove_effect = Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(),
                                                             Helpers.Create<ContextActionRemoveBuffSingleStack>(c => c.TargetBuff = frightened),
                                                             Helpers.Create<ContextActionRemoveBuffSingleStack>(c => c.TargetBuff = immunity_to_fear_buff)
                                            );
            var aura_component = Helpers.CreateAreaEffectRunAction(unitEnter: aura_effect,
                                              unitExit: remove_effect,
                                              round: aura_effect
                                              );

                         
            var toggle = Common.createToggleAreaEffect("FearPhantomIncreaseFear",
                                                      "Increase Fear",
                                                      "When the spiritualist reaches 7th level, as a swift action, the phantom can emit a 20-footradius aura that amplifies the fear conditions of enemies within it if they fail their Will saving throws (DC = 10 + 1/2 the phantom’s Hit Dice + the phantom’s Charisma modifier). A shaken enemy in the aura becomes frightened, a frightened creature becomes panicked, and a panicked creature cowers. This effect lasts either as long as the enemy stays within the aura and is under the condition of the initial fear effect, or for a number of rounds after it leaves the aura equal to 1/2 the phantom’s Hit Dice, or until the end of the original fear effect’s duration, whichever comes first. A creature that succeeds at the saving throw is immune to this effect from the same phantom for 24 hours. This is a mind-affecting fear effect.",
                                                      Helpers.GetIcon("08cb5f4c3b2695e44971bf5c45205df0"), //scare
                                                      20.Feet(), 
                                                      AbilityActivationType.WithUnitCommand,
                                                      UnitCommand.CommandType.Swift,
                                                      Common.createPrefabLink("8a80d991f3d68e84293e098a6faa7620"), //unholy
                                                      null,
                                                      new BlueprintComponent[]
                                                      {
                                                          aura_component,
                                                          Common.createContextCalculateAbilityParamsBasedOnClasses(getPhantomSpiritualistArray(), StatType.Charisma),
                                                          frightened.GetComponent<SpellDescriptorComponent>()
                                                      }
                                                      );
            toggle.DeactivateIfOwnerDisabled = true;
            var increase_fear = Common.ActivatableAbilityToFeature(toggle, false);

            var stealthy = library.Get<BlueprintFeature>("c7e1d5ef809325943af97f093e149c4f");

            var stealthy_phantom = Common.featureToFeature(stealthy, false);
            stealthy_phantom.SetDescription("The phantom gains Stealthy as a bonus feat.");

            var fear_archetype = createPhantomArchetype("FearPhantomArchetype",
                                                         "Fear",
                                                         false,
                                                         true,
                                                         true,
                                                         new StatType[] { StatType.SkillPersuasion, StatType.SkillStealth },
                                                         new LevelEntry[] { Helpers.LevelEntry(1, stealthy_phantom) },
                                                         new LevelEntry[] { Helpers.LevelEntry(1) }
                                                         );

            //bane, scare, fear, phantasmal killer, phantasmal web, frightful aspect
            createPhantom("Fear",
                          "Fear",
                          "A phantom with this emotional focus suffered from overwhelming fear in life. As a phantom, it is able to channel that fear into a terrifying weapon. Fear phantoms are often horrifying to behold. Sometimes they appear as ghostly figures in tattered funeral garb or wrapped in chains or other bindings. Their features may be distorted into gaunt and haunting forms, making them seem more like ghosts or spectres. A miasma of livid gray swirling motes often surrounds their forms as they stalk their surroundings, seeking to bestow their terror on others.\n"
                          + "Skills: The phantom gains a number of ranks in Persuasion and Stealth equal to its number of Hit Dice. While confined in the spiritualist’s consciousness, such a phantom grants the spiritualist Skill Focus in each of these skills.\n"
                          + "Good Saves: Reflex and Will.\n"
                          + "Stealthy: The phantom gains Stealthy as a bonus feat.",
                          frightened.Icon,
                          fear_archetype,
                          horryfying_strike, increase_fear, frightful_attack, shelter_allies,
                          new StatType[] { StatType.SkillPersuasion, StatType.SkillStealth },
                          12, 14,
                          new BlueprintAbility[]
                          {
                              library.Get<BlueprintAbility>("8bc64d869456b004b9db255cdd1ea734"), //bane
                              NewSpells.savage_maw,
                              library.Get<BlueprintAbility>("8a28a811ca5d20d49a863e832c31cce1"), //vampiryc touch
                              library.Get<BlueprintAbility>("6717dbaef00c0eb4897a1c908a75dfe5") //phantasmal killer
                          },
                          horryfying_strike_exciter,
                          increase_fear,
                          emotion_conduit_spells: new BlueprintAbility[]
                          {
                              library.Get<BlueprintAbility>("8bc64d869456b004b9db255cdd1ea734"), //bane
                              library.Get<BlueprintAbility>("08cb5f4c3b2695e44971bf5c45205df0"), //scare
                              SpellDuplicates.addDuplicateSpell(library.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0"), "EmotionConduitFearSpell", ""),
                              library.Get<BlueprintAbility>("6717dbaef00c0eb4897a1c908a75dfe5"), //phantasmal killer
                              library.Get<BlueprintAbility>("12fb4a4c22549c74d949e2916a2f0b6a"), //phantasmal web
                              library.Get<BlueprintAbility>("e788b02f8d21014488067bdd3ba7b325"), //frightful aspect
                          }
                          );
        }
    }
}
