using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CallOfTheWild.NewMechanics;
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
    class PactWizard
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeatureSelection patron_spells;
        static public BlueprintFeatureSelection curse_selection;
        static public BlueprintFeature reroll_feature;
        static public BlueprintFeature patron_metamagic;
        static public BlueprintFeature reroll_bonus;


        static LibraryScriptableObject library => Main.library;


        public static void create()
        {
            var wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "PactWizardArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Pact Wizard");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While the art of wizardry is usually a scholar’s pursuit, there are those who seek mastery of arcane power without tedious study and monotonous research. Motivated by foolish ambition, such individuals turn to the greatest enigmas of the cosmos in the hopes of attaining greater power. Though few successfully attract the attention of these forces, those who do receive phenomenal arcane power for their efforts, but become the dutiful playthings and servants of the forces with which they consort.");
            });
            Helpers.SetField(archetype, "m_ParentClass", wizard);
            library.AddAsset(archetype, "");
            createPatronSpellsAndMetamagic();
            createCurseSelection();
            createRerollsAndRerollsBonus();



            var wizard_feat = library.Get<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, wizard_feat),
                                                          Helpers.LevelEntry(5, wizard_feat),
                                                          Helpers.LevelEntry(10, wizard_feat),
                                                          Helpers.LevelEntry(15, wizard_feat),
                                                          Helpers.LevelEntry(20, wizard_feat)
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, patron_spells),
                                                       Helpers.LevelEntry(5, curse_selection),
                                                       Helpers.LevelEntry(10, reroll_feature),
                                                       Helpers.LevelEntry(15, patron_metamagic),
                                                       Helpers.LevelEntry(20, reroll_bonus),
            };

            wizard.Progression.UIDeterminatorsGroup = wizard.Progression.UIDeterminatorsGroup.AddToArray(patron_spells);
            wizard.Progression.UIGroups = wizard.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(curse_selection, reroll_feature, patron_metamagic, reroll_bonus));
            wizard.Archetypes = wizard.Archetypes.AddToArray(archetype);
        }


        static void createRerollsAndRerollsBonus()
        {

            var resource = Helpers.CreateAbilityResource("PactWizardRerollResource", "", "", "", null);
            resource.SetIncreasedByStat(0, StatType.Intelligence);


            reroll_feature = Helpers.CreateFeature("PactWizardRerollsFeature",
                                                   "Great Power, Greater Expense",
                                                   "At 10th level, the pact wizard can invoke his patron’s power to roll twice and take the better result when attempting any caster level check, concentration check, initiative check, or saving throw. He can activate this ability as a free action before attempting the check, even if it isn’t his turn. He can use this ability a number of times per day equal to his Intelligence modifier.",
                                                   "",
                                                   Helpers.GetIcon("9af0b584f6f754045a0a79293d100ab3"), //luck
                                                   FeatureGroup.None,
                                                   resource.CreateAddAbilityResource()
                                                   );

            reroll_bonus = Helpers.CreateFeature("PactWizardRerollsBonusFeature",
                                       "Great Power, Greater Expense",
                                       "At 20th level, when the pact wizard invokes his patron’s power to roll twice on a check, he adds his Intelligence bonus to the result as an insight bonus.",
                                       "",
                                       Helpers.GetIcon("9af0b584f6f754045a0a79293d100ab3"), //luck
                                       FeatureGroup.None
                                       );

            var rule_types = new NewMechanics.ModifyD20WithActions.RuleType[] { ModifyD20WithActions.RuleType.Concentration,
                                                                   ModifyD20WithActions.RuleType.Intiative,
                                                                   ModifyD20WithActions.RuleType.SavingThrow,
                                                                   ModifyD20WithActions.RuleType.SpellResistance };
            var names = new string[] { "Concentration", "Initiative", "Saving Throw", "Spell Resistance" };
            var stats = new BlueprintComponent[][] { new BlueprintComponent[] {Helpers.Create<ConcentrationBonus>(c => c.Value = Helpers.CreateContextValue(AbilityRankType.Default)) },
                                                     new BlueprintComponent[] {Helpers.CreateAddContextStatBonus(StatType.Initiative, ModifierDescriptor.Insight)},
                                                     new BlueprintComponent[] {Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Insight),
                                                                               Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Insight),
                                                                               Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Insight),
                                                                              },
                                                     new BlueprintComponent[] {Helpers.Create<SpellPenetrationBonus>(c => c.Value = Helpers.CreateContextValue(AbilityRankType.Default)) }
                                                   };
            var abilities = new List<BlueprintActivatableAbility>();
            for (int i = 0; i < rule_types.Length; i++)
            {
                var buff2 = library.CopyAndAdd<BlueprintBuff>("3bc40c9cbf9a0db4b8b43d8eedf2e6ec", rule_types[i].ToString() + "PactWizardRerollBuff", "");
                buff2.SetNameDescription(reroll_feature.Name + ": " + names[i], reroll_feature.Description);
                buff2.RemoveComponents<ModifyD20>();
                buff2.AddComponent(Helpers.Create<NewMechanics.ModifyD20WithActions>(m =>
                {
                    m.Rule = rule_types[i];
                    m.RollsAmount = 1;
                    m.TakeBest = true;
                    m.RerollOnlyIfFailed = false; //to avoid applying bonus before roll and also make it work per description
                    m.actions = Helpers.CreateActionList(Common.createContextActionSpendResource(resource, 1));
                    m.required_resource = resource;
                })
                                                                                    );
                var ability2 = Helpers.CreateActivatableAbility(rule_types[i].ToString() + "PactWizardReroll" + "ToggleAbility",
                                                               buff2.Name,
                                                               buff2.Description,
                                                               "",
                                                               buff2.Icon,
                                                               buff2,
                                                               AbilityActivationType.Immediately,
                                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                               null,
                                                               Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                               );

                var buff = Helpers.CreateBuff(rule_types[i].ToString() + "PactWizardRerollBonusBuff",
                                              buff2.Name,
                                              buff2.Description,
                                              "",
                                              buff2.Icon,
                                              null,
                                              stats[i]);
                buff.SetBuffFlags(BuffFlags.HiddenInUi);
                buff.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Intelligence));
                Common.addContextActionApplyBuffOnCasterFactsToActivatedAbilityBuffNoRemove(buff2, buff, reroll_bonus);
                ability2.DeactivateImmediately = true;
                abilities.Add(ability2);
            }


            reroll_feature.AddComponent(Helpers.CreateAddFacts(abilities.ToArray()));
        }


        static void createCurseSelection()
        {
            curse_selection = Helpers.CreateFeatureSelection("PactWizardCurseSelection",
                                               "Great Power, Greater Expense",
                                               "As a pact wizard grows in power, his choice of patron begins to affect his physical body.\n"
                                               + "At 5th level, the pact wizard chooses one oracle curse, using 1/2 his character level as his effective oracle level when determining the effects of this curse.\n"
                                               + "If an oracle curse would add spells to the oracle’s list of spells known, the pact wizard instead add those spells to the wizard’s spell list as well as to his spellbook.",
                                               "",
                                               null,
                                               FeatureGroup.Domain);

            foreach (var c in Oracle.oracle_curses.AllFeatures)
            {
                curse_selection.AllFeatures = curse_selection.AllFeatures.AddToArray(createCurseProgression(c as BlueprintProgression));
            }
        }


        static BlueprintProgression createCurseProgression(BlueprintProgression oracle_curse)
        {
            var features = new BlueprintFeature[] {oracle_curse.LevelEntries[0].Features[0] as BlueprintFeature, //1 -> 5
                                                   oracle_curse.LevelEntries[1].Features[0] as BlueprintFeature, //5 -> 10, 
                                                   oracle_curse.LevelEntries[2].Features[0] as BlueprintFeature, //10 -> 20
                                                  };

            var curse = Helpers.CreateProgression("PactWizard" + oracle_curse.name,
                                                  oracle_curse.Name,
                                                  oracle_curse.Description,
                                                  "",
                                                  features[0].Icon,
                                                  FeatureGroup.None);
            List<BlueprintAbility> curse_spells = new List<BlueprintAbility>();
            for (int i = 0; i < features.Length; i++)
            {
                
                features[i] = library.CopyAndAdd(features[i], "PactWizard" + features[i].name, "");
                foreach (var af in features[i].GetComponents<AddFeatureOnClassLevel>())
                {
                    features[i].ReplaceComponent(af, af.CreateCopy(c => { c.Class = archetype.GetParentClass(); c.Level = c.Level * 2; }));
                }

                foreach (var aks in features[i].GetComponents<AddKnownSpell>())
                {
                    features[i].ReplaceComponent(aks, aks.CreateCopy(c =>
                                                {
                                                    c.CharacterClass = archetype.GetParentClass();
                                                    if (archetype.GetParentClass().Spellbook.SpellList.Contains(c.Spell) 
                                                        && archetype.GetParentClass().Spellbook.SpellList.GetLevel(c.Spell) != c.SpellLevel)
                                                    {
                                                        c.Spell = SpellDuplicates.addDuplicateSpell(c.Spell, "PactWizard" + c.Spell.name, "");
                                                    }
                                                    curse_spells.Add(c.Spell);
                                                }
                                                )
                    );
                }
            }

            if (!curse_spells.Empty())
            {
                var feature2 = Helpers.CreateFeature(oracle_curse.name + "CurseMetamagicFeature",
                                "",
                                "",
                                "",
                                null,
                                FeatureGroup.None,
                                Helpers.Create<NewMechanics.MetamagicMechanics.ReduceMetamagicCostForSpecifiedSpells>(r => { r.reduction = 1; r.spells = curse_spells.ToArray(); })
                                );
                feature2.HideInCharacterSheetAndLevelUp = true;
                patron_metamagic.AddComponent(Common.createAddFeatureIfHasFact(curse, feature2));
            }

            curse.LevelEntries = new LevelEntry[] { Helpers.LevelEntry(5, features[0]), Helpers.LevelEntry(10, features[1]), Helpers.LevelEntry(20, features[2]) };
            curse.UIGroups = Helpers.CreateUIGroups(features);
            curse.Classes = new BlueprintCharacterClass[] {archetype.GetParentClass() };
            return curse;
        }


        static void createPatronSpellsAndMetamagic()
        {
            var diety_selection = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            patron_spells = Helpers.CreateFeatureSelection("PactWizardPatronSelection",
                                               "Patron Spells",
                                               "At 1st level, a pact wizard must select a patron. This functions like the witch class ability of the same name, except the pact wizard automatically adds his patron’s spells to his spellbook instead of to his familiar.\n"
                                               + "In addition, the pact wizard can expend any prepared spell that isn’t a spell prepared using the additional spell slot the wizard receives from his arcane school in order to spontaneously cast one of his patron’s spells of the same level.",
                                               "",
                                               diety_selection.Icon,
                                               FeatureGroup.None);

            patron_metamagic = Helpers.CreateFeature("PactWizardPatronMetamagicFeature",
                                                       "Great Power, Greater Expense",
                                                       "At 15th level, when pact wizard applies metamagic feats to any spells he learned via his patron or curse, he treats that spell’s final effective level as 1 lower (to a minimum level equal to the spell’s original level).",
                                                       "",
                                                       Helpers.GetIcon("3524a71d57d99bb4b835ad20582cf613"),
                                                       FeatureGroup.None);

            foreach (var kv in Witch.patron_spelllist_map)
            {
                var learn_spell_list = kv.Value.createLearnSpellList("PactWizard" + kv.Key + "PatronSpellList", "", archetype.GetParentClass());
                string description = kv.Key + " patron grants pact wizard the following spells: ";
                for (int i = 1; i <= 9; i++)
                {
                    description += learn_spell_list.SpellList.SpellsByLevel[i].Spells[0].Name + ((i == 9) ? "" : ", ");
                }
                description += ".";

                var patron = Helpers.CreateFeature("PactWizard" + kv.Key + "PatronFeature",
                                                  kv.Key + " Patron",
                                                  description,
                                                  "",
                                                  null,
                                                  FeatureGroup.None,
                                                  learn_spell_list
                                                  );
                var spells = Common.getSpellsFromSpellList(learn_spell_list.SpellList);
                var spells_array = Common.createSpelllistsForSpontaneousConversion(spells);

                for (int i = 0; i < spells_array.Length; i++)
                {
                    patron.AddComponent(Common.createSpontaneousSpellConversion(archetype.GetParentClass(), spells_array[i].ToArray()));
                }

                patron_spells.AllFeatures = patron_spells.AllFeatures.AddToArray(patron);


                var feature2 = Helpers.CreateFeature(kv.Key + "PatronMetamagicFeature",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    FeatureGroup.None,
                                                    Helpers.Create<NewMechanics.MetamagicMechanics.ReduceMetamagicCostForSpecifiedSpells>(r => { r.reduction = 1; r.spells = spells.ToArray(); })
                                                    );
                feature2.HideInCharacterSheetAndLevelUp = true;
                patron_metamagic.AddComponent(Common.createAddFeatureIfHasFact(patron, feature2));
            }
        }
    }
}
