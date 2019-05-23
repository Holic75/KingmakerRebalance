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
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace KingmakerRebalance
{
    class Common
    {
        static internal LibraryScriptableObject library => Main.library;

        public enum DomainSpellsType
        {
            NoSpells = 1,
            SpecialList = 2,
            NormalList = 3
        }


        internal static PrerequisiteNoArchetype prerequisiteNoArchetype(BlueprintCharacterClass character_class, BlueprintArchetype archetype)
        {
            var p = new PrerequisiteNoArchetype();
            p.Archetype = archetype;
            p.CharacterClass = character_class;
            return p;
        }

        internal static BlueprintFeature createSpellResistance(string name, string display_name, string description, string guid, BlueprintCharacterClass character_class, int start_value)
        {
            var spell_resistance = library.CopyAndAdd<BlueprintFeature>("01182bcee8cb41640b7fa1b1ad772421", //monk diamond soul
                                                                        name,
                                                                        guid);
            spell_resistance.SetName(display_name);
            spell_resistance.SetDescription(description);
            spell_resistance.Groups = new FeatureGroup[0];
            spell_resistance.RemoveComponent(spell_resistance.GetComponent<Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteClassLevel>());
            var context_rank_config = spell_resistance.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>();
            Helpers.SetField(context_rank_config, "m_StepLevel", start_value);
            Helpers.SetField(context_rank_config, "m_Class", new BlueprintCharacterClass[] { character_class });

            return spell_resistance;
        }





        internal static Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical createAlignmentDR(int dr_value, DamageAlignment alignment)
        {
            var feat = new Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical();
            feat.Alignment = alignment;
            feat.BypassedByAlignment = true;
            feat.Value.ValueType = ContextValueType.Simple;
            feat.Value.Value = dr_value;

            return feat;
        }


        internal static Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical createAlignmentDRContextRank(DamageAlignment alignment)
        {
            var feat = new Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical();
            feat.Alignment = alignment;
            feat.BypassedByAlignment = true;
            feat.Value = Helpers.CreateContextValueRank(AbilityRankType.StatBonus);
            return feat;
        }


        internal static Kingmaker.UnitLogic.FactLogic.AddDamageResistanceEnergy createEnergyDR(int dr_value, DamageEnergyType energy)
        {
            var feat = new Kingmaker.UnitLogic.FactLogic.AddDamageResistanceEnergy();
            feat.Type = energy;
            feat.Value.ValueType = ContextValueType.Simple;
            feat.Value.Value = dr_value;

            return feat;
        }


        internal static Kingmaker.UnitLogic.FactLogic.AddDamageResistanceEnergy createEnergyDRContextRank(DamageEnergyType energy)
        {
            var feat = new Kingmaker.UnitLogic.FactLogic.AddDamageResistanceEnergy();
            feat.Type = energy;
            feat.Value = Helpers.CreateContextValueRank(AbilityRankType.StatBonus);
            return feat;
        }


        internal static void addClassToDomains(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, DomainSpellsType spells_type, BlueprintFeatureSelection domain_selection)
        {
            var domains = domain_selection.AllFeatures;
            foreach (var domain_feature in domains)
            {
                
                BlueprintProgression domain = (BlueprintProgression)domain_feature;
                domain.Classes = domain.Classes.AddToArray(class_to_add);
                domain.Archetypes = domain.Archetypes.AddToArray(archetypes_to_add);
                Main.logger.Log("Processing " + domain.Name);

                foreach (var entry in domain.LevelEntries)
                {
                    foreach (var feat in entry.Features)
                    {
                        addClassToFeat(class_to_add, archetypes_to_add, spells_type, feat);
                    }
                }

                if (spells_type == DomainSpellsType.NormalList)
                {
                    
                    var spell_list = domain.GetComponent<Kingmaker.UnitLogic.FactLogic.LearnSpellList>().SpellList;

                    if (archetypes_to_add.Empty())
                    {
                        var learn_spells_fact = Helpers.Create<Kingmaker.UnitLogic.FactLogic.LearnSpellList>();
                        learn_spells_fact.SpellList = spell_list;
                        learn_spells_fact.CharacterClass = class_to_add;
                        domain.AddComponent(learn_spells_fact);
                        
                    }
                    else
                    {
                        foreach (var ar_type in archetypes_to_add)
                        {
                            var learn_spells_fact = Helpers.Create<Kingmaker.UnitLogic.FactLogic.LearnSpellList>();
                            learn_spells_fact.SpellList = spell_list;
                            learn_spells_fact.CharacterClass = class_to_add;
                            learn_spells_fact.Archetype = ar_type;
                            domain.AddComponent(learn_spells_fact);
                        }
                    }
                }
            }
        }


        static void addClassToContextRankConfig(BlueprintCharacterClass class_to_add, ContextRankConfig c)
        {
            BlueprintCharacterClass cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var classes = Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class");
            if (classes.Contains(cleric_class))
            {
                classes = classes.AddToArray(class_to_add);
                Helpers.SetField(c, "m_Class", classes);
            }
        }


        static void addClassToBuff(BlueprintCharacterClass class_to_add ,BlueprintBuff b)
        {
            {
                var context_rank_configs = b.GetComponents<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>();
                foreach (var c in context_rank_configs)
                {
                    addClassToContextRankConfig(class_to_add, c);
                }
            }
            var area_effects = b.GetComponents<Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect>();
            foreach (var c in area_effects)
            {
                var context_rank_configs = c.AreaEffect.GetComponents<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>();
                foreach (var c2 in context_rank_configs)
                {
                    addClassToContextRankConfig(class_to_add, c2);
                }
            }
        }


        static void addClassToAbility(BlueprintCharacterClass class_to_add, BlueprintAbility a)
        {
            var components = a.ComponentsArray;
            foreach (var c in components)
            {
                if (c.GetType() == new Kingmaker.UnitLogic.Abilities.Components.AbilityVariants().GetType())
                {
                    var c_typed = (Kingmaker.UnitLogic.Abilities.Components.AbilityVariants)c;
                    foreach (var v in c_typed.Variants)
                    {
                        addClassToAbility(class_to_add, v);
                    }
                }
                else if (c.GetType() == new Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig().GetType())
                {
                    var c_typed = (Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig)c;
                    addClassToContextRankConfig(class_to_add, c_typed);
                }
                else if (c.GetType() == new AbilityEffectRunAction().GetType())
                {
                    var c_typed = (AbilityEffectRunAction)c;
                    foreach (var aa in c_typed.Actions.Actions)
                    {
                        if (aa == null)
                        {
                            continue;
                        }
                        if (aa.GetType() == new Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff().GetType())
                        {
                            var aa_typed = (Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff)aa;
                            addClassToBuff(class_to_add, aa_typed.Buff);
                        }
                    }
                }

            }
        }


        static void addClassToFact(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, DomainSpellsType spells_type, BlueprintUnitFact f)
        {
            if (f.GetType() == new Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility().GetType())
            {
                var f_typed = (Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility)f;
                addClassToAbility(class_to_add, f_typed);
            }
            else if (f.GetType() == new Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility().GetType())
            {
                var f_typed = (Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility)f;
                addClassToBuff(class_to_add, f_typed.Buff);
            }
        }


        static void addClassToResource(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, BlueprintAbilityResource rsc)
        {
            BlueprintCharacterClass cleric_class = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var amount = ExtensionMethods.getMaxAmount(rsc);
            var classes = Helpers.GetField<BlueprintCharacterClass[]>(amount, "Class");
            var archetypes = Helpers.GetField<BlueprintArchetype[]>(amount, "Archetypes");

            if (classes.Contains(cleric_class))
            {
                classes = classes.AddToArray(class_to_add);
                archetypes = archetypes.AddToArray(archetypes_to_add);
                Helpers.SetField(amount, "Class", classes);
                Helpers.SetField(amount, "Archetypes", archetypes);
                ExtensionMethods.setMaxAmount(rsc, amount);
            }

        }


        static void addClassToFeat(BlueprintCharacterClass class_to_add, BlueprintArchetype[] archetypes_to_add, DomainSpellsType spells_type, BlueprintFeatureBase feat)
        {
            foreach (var c in feat.ComponentsArray)
            {
                if (c.GetType() == new Kingmaker.Designers.Mechanics.Buffs.IncreaseSpellDamageByClassLevel().GetType())
                {
                    var c_typed = (Kingmaker.Designers.Mechanics.Buffs.IncreaseSpellDamageByClassLevel)c;
                    c_typed.AdditionalClasses = c_typed.AdditionalClasses.AddToArray(class_to_add);
                    c_typed.Archetypes = c_typed.Archetypes.AddToArray(archetypes_to_add);
                }
                else if (c.GetType() == new Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel().GetType())
                {
                    var c_typed = (Kingmaker.Designers.Mechanics.Facts.AddFeatureOnClassLevel)c;
                    if (c_typed.Feature.ComponentsArray.Length > 0 
                          && c_typed.Feature.ComponentsArray[0].GetType() == new Kingmaker.UnitLogic.FactLogic.AddSpecialSpellList().GetType())
                    {
                        if (spells_type == DomainSpellsType.SpecialList)
                        {
                            //TODO: will need to make a copy of feature and replace CharacterClass in component with class_to_add
                        }
                        else
                        {
                            continue;
                        }
                    }
                    c_typed.AdditionalClasses = c_typed.AdditionalClasses.AddToArray(class_to_add);
                    c_typed.Archetypes = c_typed.Archetypes.AddToArray(archetypes_to_add);
                    addClassToFeat(class_to_add, archetypes_to_add, spells_type, c_typed.Feature);
                }
                else if (c.GetType() == new Kingmaker.UnitLogic.FactLogic.AddSpecialSpellList().GetType() && spells_type == DomainSpellsType.SpecialList)
                {
                    /*var c_typed = (Kingmaker.UnitLogic.FactLogic.AddSpecialSpellList)c;
                    if (c_typed.CharacterClass != class_to_add)
                    {
                        var c2 = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddSpecialSpellList>();
                        c2.CharacterClass = class_to_add;
                        c2.SpellList = c_typed.SpellList;
                        feat.AddComponent(c2);
                    }*/
                }
                else if (c.GetType() == new Kingmaker.UnitLogic.FactLogic.AddFacts().GetType())
                {
                    var c_typed = (Kingmaker.UnitLogic.FactLogic.AddFacts)c;
                    foreach (var f in c_typed.Facts)
                    {
                        addClassToFact(class_to_add, archetypes_to_add, spells_type, f);
                    }
                }
                else if (c.GetType() == new Kingmaker.Designers.Mechanics.Facts.AddAbilityResources().GetType())
                {
                    var c_typed = (Kingmaker.Designers.Mechanics.Facts.AddAbilityResources)c;
                    addClassToResource(class_to_add, archetypes_to_add, c_typed.Resource);
                }
                else if (c.GetType() == new Kingmaker.Designers.Mechanics.Facts.FactSinglify().GetType())
                {
                    var c_typed = (Kingmaker.Designers.Mechanics.Facts.FactSinglify)c;
                    foreach (var f in c_typed.NewFacts)
                    {
                        addClassToFact(class_to_add, archetypes_to_add, spells_type, f);
                    }
                }


            }
        }
    }
}
