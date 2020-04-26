using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
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
    class WizardDiscoveries
    {
        static bool disabled = false;
        static public BlueprintFeature infectious_charms; //
        static public BlueprintFeature time_stutter;//
        static public BlueprintFeature forests_blessing; //
        static public BlueprintFeature alchemical_affinity; //
        static public BlueprintFeature knowledge_is_power; //
        static public BlueprintFeature resilent_illusions;//
        static public BlueprintFeature idealize; //
        static public BlueprintFeatureSelection opposition_research; //

        //creative destruction
        //staff like wand ?

        static LibraryScriptableObject library => Main.library;

        static BlueprintCharacterClass wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

        static public void create(bool disable = false)
        {
            disabled = disable;
            if (!disabled)
            {
                Main.logger.Log("Enabling Wizard Discoveries");
            }
            createInfectiousCharms();
            createTimeStutter();
            createForestsBlessing();
            createAlchemicalAffinity();
            createKnowledgeIsPower();
            createResilentIllusions();
            createIdealize();
            createOppositionResearch();
        }


        static void createOppositionResearch()
        {
            var opposition_schools = library.Get<BlueprintFeatureSelection>("6c29030e9fea36949877c43a6f94ff31").AllFeatures;
            var icon = Helpers.GetIcon("68a23a419b330de45b4c3789649b5b41");

            opposition_research = Helpers.CreateFeatureSelection("OppositionResearchWizardDiscoveryFeature",
                                                                 "Opposition Research",
                                                                 "Select one Wizard opposition school; preparing spells of this school now only requires one spell slot of the appropriate level instead of two.",
                                                                 "",
                                                                 icon,
                                                                 FeatureGroup.Feat,
                                                                 Helpers.PrerequisiteFeaturesFromList(opposition_schools),
                                                                 Helpers.PrerequisiteClassLevel(wizard, 9)
                                                                 );
            opposition_research.AddComponent(Helpers.PrerequisiteNoFeature(opposition_research));

            foreach (BlueprintFeature opposition_school in opposition_schools)
            {
                SpellSchool school = opposition_school.GetComponent<AddOppositionSchool>().School;
                var feature = Helpers.CreateFeature(school.ToString() + "OppositionResearchWizardDiscoveryFeature",
                                                   "Opposition Research: " + school.ToString(),
                                                   opposition_research.Description,
                                                   "",
                                                   opposition_school.Icon,
                                                   FeatureGroup.WizardFeat,
                                                   Helpers.Create<RemoveFeatureOnApply>(f => f.Feature = opposition_school),
                                                   Helpers.Create<RemoveOppositionSchool>(s => s.school = school),
                                                   Helpers.PrerequisiteFeature(opposition_school),
                                                   Helpers.PrerequisiteClassLevel(wizard, 9)
                                                   );
                opposition_research.AllFeatures = opposition_research.AllFeatures.AddToArray(feature);
            }

            addWizardDiscovery(opposition_research);
        }


        static void createResilentIllusions()
        {
            resilent_illusions = Helpers.CreateFeature("ResilentIllusionsWizardDiscoveryFeature",
                                                       "Resilient Illusions",
                                                       "Anytime a creature tries to disbelieve one of your illusion effects, make a caster level check. Treat the illusion’s save DC as its normal DC or the result of the caster level check, whichever is higher.",
                                                       "",
                                                       Helpers.GetIcon("6717dbaef00c0eb4897a1c908a75dfe5"), //phantasmal killer
                                                       FeatureGroup.Feat,
                                                       Helpers.Create<SpellManipulationMechanics.ReplaceDCWithCasterLevelCheckForSchool>(r => { r.school = SpellSchool.Illusion; r.save_type = SavingThrowType.Will; }),
                                                       Helpers.PrerequisiteClassLevel(wizard, 8)
                                                       );
            addWizardDiscovery(resilent_illusions);
        }


        static void createIdealize()
        {
            idealize = Helpers.CreateFeature("IdealizeWizardDiscoveryFeature",
                                           "Idealize",
                                           "When a transmutation spell you cast grants an enhancement bonus to an ability score, that bonus increases by 2. At 20th level, the bonus increases by 4.",
                                           "",
                                           Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"), //enlarge
                                           FeatureGroup.Feat,
                                           Helpers.Create<SpellManipulationMechanics.ModifierBonusForSchool>(r =>
                                                                                                           { r.school = SpellSchool.Transmutation;
                                                                                                               r.descriptor = ModifierDescriptor.Enhancement;
                                                                                                               r.bonus = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                           }),
                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { wizard },
                                                                           progression: ContextRankProgression.Custom,
                                                                           customProgression: new (int, int)[] { (19, 2), (20, 4) }
                                                                           ),
                                           Helpers.PrerequisiteClassLevel(wizard, 10)
                                           );
            idealize.ReapplyOnLevelUp = true;
            addWizardDiscovery(idealize);
        }


        static void createKnowledgeIsPower()
        {
            knowledge_is_power = Helpers.CreateFeature("KnowledgeIsPowerWizardDiscoveryFeature",
                                         "Knowledge is Power ",
                                         "Your understanding of physical forces gives you power over them. You add your Intelligence modifier on combat maneuver checks and to your CMD. You also add your Intelligence modifier on all Strength checks.",
                                         "",
                                         Helpers.GetIcon("810992c76efdde84db707a0444cf9a1c"), //telekinetic fist
                                         FeatureGroup.Feat,
                                         Helpers.CreateAddContextStatBonus(StatType.AdditionalCMB, ModifierDescriptor.UntypedStackable),
                                         Common.createAbilityScoreCheckBonus(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                                                                 ModifierDescriptor.UntypedStackable, StatType.Strength),
                                         Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Intelligence, min: 0),
                                         Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence)
                                         );

            addWizardDiscovery(knowledge_is_power);
        }


        static void createAlchemicalAffinity()
        {
            var alchemist_spells = library.Get<BlueprintSpellList>("f60d0cd93edc65c42ad31e34a905fb2f");

            alchemical_affinity = Helpers.CreateFeature("AlchemicalAffinityWizardDiscoveryFeature",
                                                     "Alchemical Affinity",
                                                     "Whenever you cast a spell that appears on both the wizard and alchemist spell lists, you treat your caster level as 1 higher than normal and the save DC of such spells increases by 1.",
                                                     "",
                                                     Helpers.GetIcon("2a6eda8ef30379142a4b75448fb214a3"), //poison
                                                     FeatureGroup.Feat,
                                                     Helpers.Create<NewMechanics.SpellListAffinity>(s => { s.base_spell_list = wizard.Spellbook.SpellList; s.second_spell_list = alchemist_spells; s.bonus = 1; }),
                                                     Helpers.PrerequisiteClassLevel(wizard, 5)
                                                     );
            addWizardDiscovery(alchemical_affinity);
        }


        static void createForestsBlessing()
        {
            var druid_spells = library.Get<BlueprintSpellList>("bad8638d40639d04fa2f80a1cac67d6b");

            forests_blessing = Helpers.CreateFeature("ForestsBlessingWizardDiscoveryFeature",
                                                     "Forest’s Blessing",
                                                     "You cast any spells that appear on both the wizard and druid spell lists at +1 caster level and with +1 to the save DC.",
                                                     "",
                                                     Helpers.GetIcon("0fd00984a2c0e0a429cf1a911b4ec5ca"), //entangle
                                                     FeatureGroup.Feat,
                                                     Helpers.Create<NewMechanics.SpellListAffinity>(s => { s.base_spell_list = wizard.Spellbook.SpellList; s.second_spell_list = druid_spells; s.bonus = 1; }),
                                                     Helpers.PrerequisiteClassLevel(wizard, 5)
                                                     );
            addWizardDiscovery(forests_blessing);
        }


        static void createTimeStutter()
        {
            var resource = Helpers.CreateAbilityResource("TimeStutterResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 15, 1, 5, 1, 0, 0.0f, new BlueprintCharacterClass[] { wizard });

            var ability = library.CopyAndAdd<BlueprintAbility>(NewSpells.time_stop, "TimeStutter" + NewSpells.time_stop.name, "");
            ability.AvailableMetamagic = 0;
            ability.LocalizedDuration = Helpers.CreateString("${ability.name}.Duration", Helpers.oneRoundDuration);
            ability.ReplaceComponent<ContextCalculateSharedValue>(Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.Zero, 0, 2), sharedValue: AbilitySharedValue.Duration));
            ability.AddComponent(resource.CreateResourceLogic());
            ability.Type = AbilityType.SpellLike;

            
            time_stutter = Common.AbilityToFeature(ability, false);
            time_stutter.SetName("Time Stutter");
            time_stutter.AddComponent(resource.CreateAddAbilityResource());
            time_stutter.SetDescription("You can briefly step out of time, pausing the world around you. This ability acts as the time stop spell, except that you gain only 1 round of apparent time. You can use this ability once per day plus one additional time for every 5 wizard levels you possess beyond 10th.");
            time_stutter.AddComponent(Helpers.PrerequisiteClassLevel(wizard, 10));
            addWizardDiscovery(time_stutter);
        }


        static void addWizardDiscovery(BlueprintFeature feature)
        {
            if (disabled)
            {
                return;
            }
            feature.Groups = feature.Groups.AddToArray(FeatureGroup.WizardFeat);
            library.AddFeats(feature);

            var wizard_feat = library.Get<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899");
            wizard_feat.AllFeatures = wizard_feat.AllFeatures.AddToArray(feature);
        }

        static void createInfectiousCharms()
        {


            var spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("fd4d9fd7f87575d47aafe2a64a6e2d8d"), //hideous laughter
                library.Get<BlueprintAbility>("c7104f7526c4c524f91474614054547e"), //hold person
                library.Get<BlueprintAbility>("dd2918e4a77c50044acba1ac93494c36"), //ovewhelming grief
                library.Get<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61"), //dominate
                library.Get<BlueprintAbility>("444eed6e26f773a40ab6e4d160c67faa"), //feeblemind
                library.Get<BlueprintAbility>("41e8a952da7a5c247b3ec1c2dbb73018"), //hold monster
                library.Get<BlueprintAbility>("2b044152b3620c841badb090e01ed9de"), //insanity
                library.Get<BlueprintAbility>("261e1788bfc5ac1419eec68b1d485dbc"), //power word blind
                library.Get<BlueprintAbility>("f958ef62eea5050418fb92dfa944c631"), //power word stun
                library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757"), //dominate monster
                library.Get<BlueprintAbility>("2f8a67c483dfa0f439b293e094ca9e3c"), //power word kill
                library.Get<BlueprintAbility>("cbf3bafa8375340498b86a3313a11e2f"), //euphoric tranquility effect
                NewSpells.irresistible_dance.StickyTouch.TouchDeliveryAbility,
            };

            infectious_charms = Helpers.CreateFeature("InfectiousCharmsArcaneDiscoveryFeature",
                                                      "Infectious Charms",
                                                      "Anytime you target and successfully affect a single creature with a charm or compulsion spell, as a swift action immediately after affecting a creature with a charm or compulsion spell, you can cause the spell to carry over to another creature within close range. The spell behaves in all ways as though its new target were the original target of the spell.",
                                                      "",
                                                      Helpers.GetIcon("d7cbd2004ce66a042aeab2e95a3c5c61"),
                                                      FeatureGroup.Feat,
                                                      Helpers.PrerequisiteClassLevel(wizard, 11)
                                                      );

            var swift_abilites = new List<BlueprintAbility>();


            foreach (var spell in spells)
            {
                var buff = Helpers.CreateBuff("InfectiousCharms" + spell.name + "Buff",
                                              infectious_charms.Name,
                                              infectious_charms.Description,
                                              "",
                                              null,
                                              null);

                var apply_buff = Common.createContextActionApplyBuffToCaster(buff, Helpers.CreateContextDuration(1), dispellable: false);

                var swift_ability = library.CopyAndAdd<BlueprintAbility>(spell, "InfectiousCharms" + spell.name, "");
                swift_ability.ActionType = UnitCommand.CommandType.Swift;
                swift_ability.AddComponent(Common.createAbilityCasterHasFacts(buff));
                swift_ability.RemoveComponents<AbilityDeliverTouch>();

                swift_ability.Range = AbilityRange.Close;


                var remove_buff = Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(Helpers.Create<ContextActionRemoveBuff>(r => r.Buff = buff))));
                swift_ability.AddComponent(remove_buff);

                bool found = false;

                var new_actions = spell.GetComponent<AbilityEffectRunAction>().Actions.Actions;

                new_actions = Common.changeAction<ContextActionConditionalSaved>(new_actions,
                                                                                 c =>
                                                                                 {
                                                                                     c.Failed = Helpers.CreateActionList(c.Failed.Actions.AddToArray(apply_buff));
                                                                                     found = true;
                                                                                 });
                if (!found)
                {
                    new_actions = new_actions.AddToArray(apply_buff);
                }

                spell.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(new_actions));

                buff.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = swift_ability));
                swift_abilites.Add(swift_ability);
            }

            var wrapper = Common.createVariantWrapper("InfectiousCharmsBaseAbility", "", swift_abilites.ToArray());
            wrapper.SetNameDescriptionIcon(infectious_charms.Name, infectious_charms.Description, infectious_charms.Icon);

            infectious_charms.AddComponent(Helpers.CreateAddFact(wrapper));

            addWizardDiscovery(infectious_charms);
        }


        public class RemoveOppositionSchool : OwnedGameLogicComponent<UnitDescriptor>
        {
            public SpellSchool school;
            public override void OnFactActivate()
            {
                base.Owner.DemandSpellbook(wizard).OppositionSchools.Remove(this.school);
            }
        }
    }
}
