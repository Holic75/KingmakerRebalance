using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Prerequisites;

namespace CallOfTheWild
{
    public partial class Phantom
    {
        static void createKindness()
        {
            var benevolent_phantom = Helpers.CreateFeature("KindnessPhantomBaseFeature",
                                                             "Benevolent",
                                                             "When the phantom uses the aid another action, the bonus granted by a successful check increases by 2.",
                                                             "",
                                                             Helpers.GetIcon("55a037e514c0ee14a8e3ed14b47061de"), // remove fear
                                                             FeatureGroup.None
                                                             );

            Helpers.SetField(Rebalance.aid_another_config, "m_FeatureList",
                 Helpers.GetField<BlueprintFeature[]>(Rebalance.aid_another_config, "m_FeatureList").AddToArray(benevolent_phantom, benevolent_phantom));

            var opening_strike_buff_target = Helpers.CreateBuff("KindnessPhantomOpeningStrikeBuff",
                                                         "Opening Strike Target",
                                                         "As a standard action, the phantom can make a melee attack against a foe. If this attack hits, the spiritualist designates one ally threatening that enemy. This ally can make a single attack at his full base attack bonus against that enemy as a swift action.",
                                                         "",
                                                         Helpers.GetIcon("9d5d2d3ffdd73c648af3eb3e585b1113"),
                                                         null);

            var opening_strike_ally_ability = Helpers.CreateAbility("KindnessPhantomOpeningStrikeAllyAbility",
                                                                    "Opening Strike",
                                                                    opening_strike_buff_target.Description,
                                                                    "",
                                                                    opening_strike_buff_target.Icon,
                                                                    AbilityType.Special,
                                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                                    AbilityRange.Weapon,
                                                                    "",
                                                                    "",
                                                                    Helpers.CreateRunActions(Common.createContextActionAttack(Helpers.CreateActionList(Common.createContextActionRemoveBuff(opening_strike_buff_target)),
                                                                                                                              Helpers.CreateActionList(Common.createContextActionRemoveBuff(opening_strike_buff_target))
                                                                                                                              )
                                                                                                                              ),
                                                                    Helpers.Create<NewMechanics.AttackAnimation>(),
                                                                    Common.createAbilityTargetHasFact(opening_strike_buff_target)
                                                                    );
            opening_strike_ally_ability.setMiscAbilityParametersTouchHarmful();
            opening_strike_ally_ability.NeedEquipWeapons = true;

            var opening_strike_buff_ally = Helpers.CreateBuff("KindnessPhantomOpeningStrikeAllyBuff",
                                                         "Opening Strike",
                                                         opening_strike_buff_target.Description,
                                                         "",
                                                         opening_strike_buff_target.Icon,
                                                         null,
                                                         Helpers.CreateAddFact(opening_strike_ally_ability));

            var effect = Helpers.Create<TeamworkMechanics.ContextActionOnUnitsEngagingTarget>(c =>
            {
                c.actions = Helpers.CreateActionList(Common.createContextActionApplyBuff(opening_strike_buff_ally, Helpers.CreateContextDuration(1), dispellable: false));
                c.ignore_caster = true;
            });
            var opening_strike_ability = Helpers.CreateAbility("KindnessPhantomOpeningStrikeAbility",
                                                        opening_strike_buff_ally.Name,
                                                        opening_strike_buff_target.Description,
                                                        "",
                                                        opening_strike_buff_target.Icon,
                                                        AbilityType.Special,
                                                        Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                        AbilityRange.Weapon,
                                                        "",
                                                        "",
                                                        Helpers.CreateRunActions(Common.createContextActionAttack(Helpers.CreateActionList(effect, 
                                                                                                                                           Common.createContextActionApplyBuff(opening_strike_buff_target, Helpers.CreateContextDuration(1), dispellable: false)))),
                                                        Helpers.Create<NewMechanics.AttackAnimation>()
                                                        );
            opening_strike_ability.setMiscAbilityParametersTouchHarmful();
            opening_strike_ability.NeedEquipWeapons = true;
            var opening_strike = Common.AbilityToFeature(opening_strike_ability, false);

            //etheric healing
            
            var etheric_healing_resource = Helpers.CreateAbilityResource("EthericHealingResource", "", "", "", null);
            etheric_healing_resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, getPhantomArray());
            etheric_healing_resource.SetIncreasedByStat(0, StatType.Charisma);

            var lay_on_hands_self = library.CopyAndAdd<BlueprintAbility>("8d6073201e5395d458b8251386d72df1", "KindnessPhantomEthericHealingSelf", "");
            var lay_on_hands = library.CopyAndAdd<BlueprintAbility>("caae1dc6fcf7b37408686971ee27db13", "KindnessPhantomEthericHealing", "");

            lay_on_hands.RemoveComponents<AbilityCasterAlignment>();
            lay_on_hands.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getPhantomArray()));
            lay_on_hands.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = etheric_healing_resource);

            lay_on_hands_self.Range = AbilityRange.Touch;
            lay_on_hands_self.RemoveComponents<AbilityCasterAlignment>();
            lay_on_hands_self.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getPhantomArray()));
            lay_on_hands_self.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = etheric_healing_resource);
            lay_on_hands_self.AddComponent(Helpers.Create<CompanionMechanics.AbilityTargetSelfOrMaster>());

            var etheric_healing = Helpers.CreateFeature("KindnessPhantomEthericHealingFeature",
                                                        "Etheric Healing",
                                                        "When the spiritualist reaches 7th level, the phantom gains the lay on hands ability with an effective paladin level equal to its Hit Dice. The phantom can use this ability on the spiritualist as a swift action.",
                                                        "",
                                                        lay_on_hands.Icon,
                                                        FeatureGroup.None);


            lay_on_hands_self.SetNameDescription("Etheric Healing: Self or Master",
                                                 etheric_healing.Description +"\n"
                                                 + "Lay On Hands: " + lay_on_hands.Description);
            lay_on_hands.SetNameDescription("Etheric Healing",
                                            etheric_healing.Description + "\n"
                                            + "Lay On Hands: " + lay_on_hands.Description);

            etheric_healing.AddComponents(Helpers.CreateAddFacts(lay_on_hands, lay_on_hands_self),
                                          etheric_healing_resource.CreateAddAbilityResource(),
                                          Helpers.Create<ReplaceCasterLevelOfAbility>(a => { a.Spell = lay_on_hands_self; a.Class = phantom_class; }),
                                          Helpers.Create<ReplaceCasterLevelOfAbility>(a => { a.Spell = lay_on_hands; a.Class = phantom_class; })
                                          );


            var kindness_archetype = createPhantomArchetype("KindnessPhantomArchetype",
                                                         "Kindness",
                                                         true,
                                                         false,
                                                         true,
                                                         new StatType[] { StatType.SkillPersuasion, StatType.SkillLoreReligion },
                                                         new LevelEntry[] { Helpers.LevelEntry(1, benevolent_phantom) },
                                                         new LevelEntry[] { Helpers.LevelEntry(1) }
                                                         );

            Dictionary<BlueprintFeature, BlueprintFeature> phantom_mercy_map = new Dictionary<BlueprintFeature, BlueprintFeature>();
            var mercy_selection = library.Get<BlueprintFeatureSelection>("02b187038a8dce545bb34bbfb346428d");
            var phantom_mercy_selection = library.CopyAndAdd<BlueprintFeatureSelection>("02b187038a8dce545bb34bbfb346428d", "KindnessPhantomMercySelection", "");
            phantom_mercy_selection.AllFeatures = new BlueprintFeature[0];
            foreach (var m in mercy_selection.AllFeatures)
            {
                var prereq = m.GetComponent<PrerequisiteClassLevel>();

                if (prereq == null || prereq.Level <= 12)
                {
                    phantom_mercy_map[m] = Common.createAddFeatToAnimalCompanion("AddKindnessPhantom", m, "");
                    phantom_mercy_map[m].AddComponents(m.ComponentsArray);
                }               
            }

            foreach (var kv in phantom_mercy_map)
            {
                kv.Value.RemoveComponents<PrerequisiteClassLevel>();
                kv.Value.RemoveComponents<PrerequisiteArchetypeLevel>();
                kv.Value.MaybeReplaceComponent<PrerequisiteFeature>(p => p.Feature = phantom_mercy_map[p.Feature]);
                phantom_mercy_selection.AllFeatures = phantom_mercy_selection.AllFeatures.AddToArray(kv.Value);
            }

            var expanded_aid = Helpers.CreateFeature("KindnessPhantomExpandedAid",
                                                     "Expanded Aid",
                                                     "When the spiritualist reaches 12th level, the phantom can use the aid another action as a move action. Additionally, the phantom selects four mercies available to a 12th-level paladin that it adds to its lay on hands ability.",
                                                     "",
                                                     Helpers.GetIcon("03a9630394d10164a9410882d31572f0"),
                                                     FeatureGroup.None,
                                                     Helpers.Create<TurnActionMechanics.MoveActionAbilityUse>(m => m.abilities = new BlueprintAbility[] { Rebalance.aid_another })
                                                     );


            var exceptional_aid = Helpers.CreateFeature("KindnessPhantomExceptionalAid",
                                         "Exceptional Aid",
                                         "When the spiritualist reaches 17th level, the phantom can use the aid another action as a swift action. Additionally, when designating an ally for an opening strike, that ally gains a bonus equal to the phantom’s Charisma modifier on damage rolls for the attack granted to him.",
                                         "",
                                         Helpers.GetIcon("76f8f23f6502def4dbefedffdc4d4c43"), //command
                                         FeatureGroup.None,
                                         Helpers.Create<TurnActionMechanics.UseAbilitiesAsSwiftAction>(m => m.abilities = new BlueprintAbility[] { Rebalance.aid_another })
                                         );

            var exceptional_aid_buff = Helpers.CreateBuff("KindnessPhantomExceptionalAidBuff",
                                                          exceptional_aid.Name,
                                                          exceptional_aid.Description,
                                                          "",
                                                          exceptional_aid.Icon,
                                                          null,
                                                          Helpers.Create<NewMechanics.DamageBonusForAbilities>(d =>
                                                          {
                                                              d.abilities = new BlueprintAbility[] { opening_strike_ally_ability };
                                                              d.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                          }),
                                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Charisma)
                                                          );

            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(opening_strike_buff_ally,
                                                                                      Helpers.CreateConditional(Common.createContextConditionCasterHasFact(exceptional_aid),
                                                                                                               Common.createContextActionApplyBuff(exceptional_aid_buff, Helpers.CreateContextDuration(), is_child: true, dispellable: false)
                                                                                                               )
                                                                                     );
            //bless, blessing of courage and life, good hope, joyful rapture, waves of ecstasy, greater heroism
            createPhantom("Kindness",
                          "Kindness",
                          "A phantom with this emotional focus was a being with a generous heart in life and continues to remain that way well after death. The phantom’s intense generosity compels it to remain a phantom and continue assisting the living, especially relatives or those who had been its good friends during its living years. Kindness phantoms have pleasant and gentle demeanors and speak with a melodic cadence, putting most who see them at ease. Their auras are bright emerald green with occasional fluctuating scarlet or golden hues.\n"
                          + "Skills: The phantom gains a number of ranks in Persuasion and Lore (Religion) equal to its number of Hit Dice. While confined in the spiritualist’s consciousness, the phantom grants the spiritualist Skill Focus in each of these skills.\n"
                          + "Good Saves: Fortitude and Will.\n"
                          + "Benevolent: When the phantom uses the aid another action, the bonus granted by a successful check increases by 2.",
                          etheric_healing.Icon,
                          kindness_archetype,
                          opening_strike, etheric_healing, expanded_aid, exceptional_aid,
                          new StatType[] { StatType.SkillPersuasion, StatType.SkillLoreReligion },
                          12, 14,
                          new BlueprintAbility[]
                          {
                              library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638"), //bless
                              library.Get<BlueprintAbility>("03a9630394d10164a9410882d31572f0"), //aid
                              library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63"), //heroism
                              library.Get<BlueprintAbility>("a5e23522eda32dc45801e32c05dc9f96") //good hope
                          },
                          emotion_conduit_spells: new BlueprintAbility[]
                          {
                              library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638"), //bless
                              library.Get<BlueprintAbility>("c36c1d11771b0584f8e100b92ee5475b"), //blessing of courage and life
                              library.Get<BlueprintAbility>("a5e23522eda32dc45801e32c05dc9f96"), //good hope
                              library.Get<BlueprintAbility>("15a04c40f84545949abeedef7279751a"), //joyful rapture
                              library.Get<BlueprintAbility>("1e2d1489781b10a45a3b70192bba9be3"), //waves of ecstasy
                              library.Get<BlueprintAbility>("e15e5e7045fda2244b98c8f010adfe31") //heroism greater
                          }
                          );

            var le12 = phantom_progressions["Kindness"].LevelEntries.Where(le => le.Level == 12).First();
            le12.Features[0].AddComponents(Helpers.Create<EvolutionMechanics.addSelection>(a => a.selection = phantom_mercy_selection),
                                           Helpers.Create<EvolutionMechanics.addSelection>(a => a.selection = phantom_mercy_selection),
                                           Helpers.Create<EvolutionMechanics.addSelection>(a => a.selection = phantom_mercy_selection),
                                           Helpers.Create<EvolutionMechanics.addSelection>(a => a.selection = phantom_mercy_selection));
        }
    }
}
