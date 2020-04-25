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
        static public BlueprintFeature infectious_charms;
        static public BlueprintFeature time_stutter;
        //creative destruction
        //alchemical affinity
        //forests blessing
        //idealize
        //knowledge is power
        //opposition research
        //resilent illusions
        //staff like wand ?

        static LibraryScriptableObject library => Main.library;

        static BlueprintCharacterClass wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");


        static public void create()
        {
            createInfectiousCharms();
            createTimeStutter();
        }


        static void createTimeStutter()
        {
            var resource = Helpers.CreateAbilityResource("TimeStutterResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 15, 1, 5, 1, 0, 0.0f, new BlueprintCharacterClass[] { wizard });

            var ability = library.CopyAndAdd<BlueprintAbility>(NewSpells.time_stop, "TimeStutter" + NewSpells.time_stop.name, "");
            ability.AvailableMetamagic = 0;
            ability.LocalizedDuration = Helpers.CreateString("${ability.name}.Duration", Helpers.oneRoundDuration);
            ability.ReplaceComponent<ContextCalculateSharedValue>(Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.Zero, 0, 1), sharedValue: AbilitySharedValue.Duration));
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
            feature.Groups = feature.Groups.AddToArray(FeatureGroup.WizardFeat);
            library.AddFeats(feature);

            var wizard_feat = library.Get<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899");
            wizard_feat.AllFeatures = wizard_feat.AllFeatures.AddToArray(feature);
        }

        static void createInfectiousCharms()
        {
            //hideous laughter
            //hold person
            //overwhelming grief
            //dominate
            //feeblemind
            //hold monster
            //insanity
            //power word blind
            //power word stun
            //dominate monster 
            //power word kill

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
                library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757"), //dominate monster
                library.Get<BlueprintAbility>("2f8a67c483dfa0f439b293e094ca9e3c"), //power word kill
            };

            infectious_charms = Helpers.CreateFeature("InfectiousCharmsArcaneDiscoveryFeature",
                                                      "Infectious Charms",
                                                      "Anytime you target and successfully affect a single creature with a charm or compulsion spell, as a swift action immediately after affecting a creature with a charm or compulsion spell, you can cause the spell to carry over to another creature. The spell behaves in all ways as though its new target were the original target of the spell.",
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

                swift_abilites.Add(swift_ability);
            }

            var wrapper = Common.createVariantWrapper("InfectiousCharmsBaseAbility", "", swift_abilites.ToArray());
            wrapper.SetNameDescriptionIcon(infectious_charms.Name, infectious_charms.Description, infectious_charms.Icon);

            infectious_charms.AddComponent(Helpers.CreateAddFact(wrapper));

            addWizardDiscovery(infectious_charms);
        }
    }
}
