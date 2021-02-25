using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class SpellDuplicates
    {
        static LibraryScriptableObject library => Main.library;
        static Dictionary<string, List<BlueprintAbility>> duplicate_spells = new Dictionary<string, List<BlueprintAbility>>();

        public static BlueprintAbility addDuplicateSpell(string prototype_id, string name, string guid = "")
        {
            if (duplicate_spells.ContainsKey(prototype_id))
            {
                var spell = duplicate_spells[prototype_id].Find(a => a.name == name);
                if (spell != null)
                {
                    return spell;
                }
            }
            return addDuplicateSpell(library.Get<BlueprintAbility>(prototype_id), name, guid);
        }

        public static BlueprintAbility addDuplicateSpell(BlueprintAbility prototype, string name, string guid = "")
        {
            if (duplicate_spells.ContainsKey(prototype.AssetGuid))
            {
                var spell = duplicate_spells[prototype.AssetGuid].Find(a => a.name == name);
                if (spell != null)
                {
                    return spell;
                }
            }

            BlueprintAbility new_spell = library.CopyAndAdd<BlueprintAbility>(prototype, name, guid);
            if (!duplicate_spells.ContainsKey(prototype.AssetGuid))
            {
                duplicate_spells.Add(prototype.AssetGuid, new BlueprintAbility[] { prototype }.ToList());
            }
            var list = duplicate_spells[prototype.AssetGuid];
            list.Add(new_spell);

            for (int i = 1; i < list.Count - 1; i++)
            {
                duplicate_spells[list[i].AssetGuid].Add(new_spell);
            }

            var new_list = new List<BlueprintAbility>();
            new_list.Add(new_spell);
            for (int i = 0; i < list.Count - 1; i++)
            {
                new_list.Add(list[i]);
            }
            duplicate_spells.Add(new_spell.AssetGuid, new_list);

            if (new_spell.Type == AbilityType.Spell)
            {
                Helpers.AddSpell(new_spell);
            }

            return new_spell;
        }


        public static bool isDuplicate(BlueprintAbility original, BlueprintAbility duplicate)
        {
            if (original == null || duplicate == null)
            {
                return false;
            }
            return original == duplicate
                   || (duplicate_spells.ContainsKey(original.AssetGuid) && duplicate_spells[original.AssetGuid].Contains(duplicate));
        }


        public static bool isDuplicateOrParent(BlueprintAbility original, BlueprintAbility duplicate)
        {
            if (original == null || duplicate == null)
            {
                return false;
            }
            return isDuplicate(original, duplicate) || isDuplicate(original, duplicate.Parent);
        }


        public static bool containsDuplicateOrParent(BlueprintAbility[] originals, BlueprintAbility duplicate)
        {
            foreach (var o in originals)
            {
                if (isDuplicateOrParent(o, duplicate))
                {
                    return true;
                }
            }
            return false;
        }



        public static BlueprintAbility[] getDuplicates(BlueprintAbility original)
        {
            if (original == null)
            {
                return new BlueprintAbility[0];
            }
            if (duplicate_spells.ContainsKey(original.AssetGuid))
            {
                return duplicate_spells[original.AssetGuid].ToArray();
            }
            else
            {
                return new BlueprintAbility[] { original };
            }
        }


        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SavingThrowBonusAgainstSpecificSpellsOrDuplicates : RuleInitiatorLogicComponent<RuleSavingThrow>
        {
            public BlueprintAbility[] Spells;
            public ModifierDescriptor ModifierDescriptor;
            public ContextValue Value;

            private MechanicsContext Context
            {
                get
                {
                    MechanicsContext context = (this.Fact as Buff)?.Context;
                    if (context != null)
                        return context;
                    return (this.Fact as Feature)?.Context;
                }
            }

            public override void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                BlueprintAbility sourceAbility = evt.Reason.Context?.SourceAbility;

                int bonus = this.Value.Calculate(this.Context);
                foreach (var s in Spells)
                {
                    if (SpellDuplicates.isDuplicate(s, evt.Reason.Context?.SourceAbility))
                    {
                        evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(bonus, (GameLogicComponent)this, this.ModifierDescriptor));
                        evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(bonus, (GameLogicComponent)this, this.ModifierDescriptor));
                        evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(bonus, (GameLogicComponent)this, this.ModifierDescriptor));
                        return;
                    }
                }


            }
            public override void OnEventDidTrigger(RuleSavingThrow evt)
            {
            }
        }

        [AllowedOn(typeof(BlueprintUnitFact))]
        public class ConcentrationBonusForSpellsOrDuplicates : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>, IConcentrationBonusProvider
        {
            public BlueprintAbility[] Spells;
            public ContextValue Value;


            private MechanicsContext Context
            {
                get
                {
                    MechanicsContext context = (this.Fact as Buff)?.Context;
                    if (context != null)
                        return context;
                    return (this.Fact as Feature)?.Context;
                }
            }

            public int GetStaticConcentrationBonus()
            {
                return 0;
            }

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                int bonus = this.Value.Calculate(this.Context);
                foreach (var s in Spells)
                {
                    if (SpellDuplicates.isDuplicate(s, evt.Spell))
                    {
                        evt.AddBonusConcentration(bonus);
                        return;
                    }
                }

            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class AddCasterLevelForSpellsOrDuplicates : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public BlueprintAbility[] Spells;
            public ContextValue Value;

            private MechanicsContext Context
            {
                get
                {
                    MechanicsContext context = (this.Fact as Buff)?.Context;
                    if (context != null)
                        return context;
                    return (this.Fact as Feature)?.Context;
                }
            }

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                int bonus = this.Value.Calculate(this.Context);
                foreach (var s in Spells)
                {
                    if (SpellDuplicates.isDuplicate(s, evt.Spell))
                    {
                        evt.AddBonusCasterLevel(bonus);
                        return;
                    }
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }

        [AllowedOn(typeof(BlueprintUnitFact))]
        public class SpellPenetrationBonusForSpellsOrDuplicates : RuleInitiatorLogicComponent<RuleSpellResistanceCheck>
        {
            public ContextValue Value;
            public BlueprintAbility[] Spells;

            private MechanicsContext Context
            {
                get
                {
                    MechanicsContext context = (this.Fact as Buff)?.Context;
                    if (context != null)
                        return context;
                    return (this.Fact as Feature)?.Context;
                }
            }

            public override void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
            {
                int bonus = this.Value.Calculate(this.Context);
                foreach (var s in Spells)
                {
                    if (SpellDuplicates.isDuplicate(s, evt.Ability))
                    {
                        evt.AdditionalSpellPenetration += bonus;
                        return;
                    }
                }
                
            }

            public override void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
            }
        }


        [AllowMultipleComponents]
        [AllowedOn(typeof(BlueprintUnitFact))]
        public class IncreaseDCForSpellsOrDuplicates : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            public BlueprintAbility[] Spells;
            public ContextValue Value;

            private MechanicsContext Context
            {
                get
                {
                    MechanicsContext context = (this.Fact as Buff)?.Context;
                    if (context != null)
                        return context;
                    return (this.Fact as Feature)?.Context;
                }
            }

            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                int bonus = this.Value.Calculate(this.Context);
                foreach (var s in Spells)
                {
                    if (SpellDuplicates.isDuplicate(s, evt.Spell))
                    {
                        evt.AddBonusDC(bonus);
                        return;
                    }
                }
            }

            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }

        [AllowedOn(typeof(BlueprintParametrizedFeature))]
        public class ClBonusParametrized : ParametrizedFeatureComponent, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public int bonus;
            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                var spell = (this.Fact as Feature)?.Param.Blueprint as BlueprintAbility;
                if (SpellDuplicates.isDuplicate(spell, evt.Spell) || SpellDuplicates.isDuplicate(spell, evt.Spell?.Parent))
                {
                    evt.AddBonusCasterLevel(bonus);
                }
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }


        [AllowedOn(typeof(BlueprintParametrizedFeature))]
        public class SignatureSpellBonusParametrized : ParametrizedFeatureComponent, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
        {
            public ContextValue dc_bonus;
            public ContextValue concentration_bonus;
            public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                var spell = (this.Fact as Feature)?.Param.Blueprint as BlueprintAbility;
                if (SpellDuplicates.isDuplicate(spell, evt.Spell) || SpellDuplicates.isDuplicate(spell, evt.Spell?.Parent))
                {
                    evt.AddBonusDC(dc_bonus.Calculate(this.Fact.MaybeContext));
                    evt.AddBonusConcentration(concentration_bonus.Calculate(this.Fact.MaybeContext));
                }
            }

            public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }
        }
    }



    [Harmony12.HarmonyPatch(typeof(SpellSpecializationParametrized))]
    [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
    public class SpellSpecializationParametrized_OnEventAboutToTrigger_Patch
    {
        static bool Prefix(SpellSpecializationParametrized __instance, RuleCalculateAbilityParams evt)
        {
            Main.TraceLog();
            var spell = (__instance.Fact as Feature)?.Param.Blueprint as BlueprintAbility;
            if (SpellDuplicates.isDuplicate(spell, evt.Spell) || SpellDuplicates.isDuplicate(spell, evt.Spell?.Parent))
            {
                evt.AddBonusCasterLevel(2);
            }

            return false;
        }
    }



}
