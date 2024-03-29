﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SpellFailureMechanics
{
    class UnitPartUnableToUseAbilities : AdditiveUnitPart
    {
        public bool active()
        {
            return !this.buffs.Empty();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class UnableToUseAbilities : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartUnableToUseAbilities>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartUnableToUseAbilities>().removeBuff(this.Fact);
        }
    }


    class UnitPartSilence : AdditiveUnitPart
    {
        public bool active()
        {
            return !this.buffs.Empty();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class Silence : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartSilence>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartSilence>().removeBuff(this.Fact);
        }
    }


    public class NonSilencedRestriciton : ActivatableAbilityRestriction
    {
        public override bool IsAvailable()
        {
            return !(this.Owner.Get<UnitPartSilence>()?.active()).GetValueOrDefault();
        }
    }


    class UnitPartCenterSelf: AdditiveUnitPart
    {
        public bool active()
        {
            return !this.buffs.Empty();
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class CenterSelf : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartCenterSelf>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartCenterSelf>().removeBuff(this.Fact);
        }
    }


    class ItemUseFailure: OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, IInitiatorRulebookSubscriber
    {
        public int chance;

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
            //Main.logger.Log("Checking " + evt.Spell.Name);
            if ((evt.Spell?.SourceItemUsableBlueprint == null) || (evt.Spell.StickyTouch != null))
                return;                   
            evt.SpellFailureChance = Math.Max(evt.SpellFailureChance, this.chance);
            //Main.logger.Log("Ok " + evt.SpellFailureChance.ToString());
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {

        }
    }


    class SpellFailureChance : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, IInitiatorRulebookSubscriber
    {
        public ContextValue chance;
        public bool do_not_spend_slot_if_failed = false;
        public bool ignore_psychic = false;

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
            if (!evt.Spell.Blueprint.IsSpell || evt.Spell.Spellbook == null || (evt.Spell.StickyTouch != null))
                return;

            if (ignore_psychic && evt.Spell.Spellbook.Blueprint.GetComponent<SpellbookMechanics.PsychicSpellbook>() != null)
            {
                return;
            }
            int threshold = this.chance.Calculate(this.Fact.MaybeContext);
            int d100 = RulebookEvent.Dice.D100;
            //Main.logger.Log($"Failure: {d100}/{threshold}");
            if (d100 > threshold)
            {
                return;
            }
            evt.SpellFailureChance = 200;
            if (do_not_spend_slot_if_failed)
            {
                evt.Spell.Caster.Ensure<SpellbookMechanics.UnitPartDoNotSpendNextSpell>().active = true;
            }
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {

        }
    }


    class PsychicSpellbook : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>,  IInitiatorRulebookSubscriber
    {
        public BlueprintSpellbook spellbook;
        void IRulebookHandler<RuleCalculateAbilityParams>.OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt.Spellbook?.Blueprint != spellbook)
            {
                return;
            }

            var center_self = evt.Initiator.Get<UnitPartCenterSelf>();
            if (center_self != null && center_self.active())
            {
                return;
            }
            evt.AddBonusConcentration(-10);
        }

        void IRulebookHandler<RuleCalculateAbilityParams>.OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
           
        }
    }


    [Harmony12.HarmonyPatch(typeof(ActivatableAbility))]
    [Harmony12.HarmonyPatch("IsAvailable", Harmony12.MethodType.Getter)]
    class ActivatableAbility__IsAvailable__Patch
    {
        static void Postfix(ActivatableAbility __instance, ref bool __result)
        {
            if (__result == false)
            {
                return;
            }

            __result = !(__instance.Owner.Get<UnitPartUnableToUseAbilities>()?.active()).GetValueOrDefault();
        }
    }


    [Harmony12.HarmonyPatch(typeof(AbilityData))]
    [Harmony12.HarmonyPatch("IsAvailableForCast", Harmony12.MethodType.Getter)]
    class AbilityData__IsAvailableForCast__Patch
    {
        static void Postfix(AbilityData __instance, ref bool __result)
        {
            if (__result == false)
            {
                return;
            }

            if ((__instance.Caster.Get<UnitPartUnableToUseAbilities>()?.active()).GetValueOrDefault())
            {
                __result = false;
                return;
            }

            bool is_psychic = __instance.Spellbook?.Blueprint.GetComponent<SpellbookMechanics.PsychicSpellbook>() != null;

            if (is_psychic)
            {
                if (!__instance.HasMetamagic((Metamagic)MetamagicFeats.MetamagicExtender.Logical))
                {
                    foreach (var b in __instance.Caster.Buffs)
                    {
                        if (!b.IsSuppressed && (b.Context.SpellDescriptor & (SpellDescriptor.NegativeEmotion | SpellDescriptor.Fear | SpellDescriptor.Shaken)) != 0)
                        {
                            __result = false;
                            return;
                        }
                    }
                }
            }
            else if ((__instance.Caster.Get<UnitPartSilence>()?.active()).GetValueOrDefault()
                    && __instance.Blueprint.IsSpell)
            {
                __result = false;
                return;
            }
        }
    }



    [Harmony12.HarmonyPatch(typeof(AbilityData))]
    [Harmony12.HarmonyPatch("CalcSpellSource", Harmony12.MethodType.Normal)]
    class AbilityData__CalcSpellSource__Patch
    {
        static bool Prefix(AbilityData __instance, ref SpellSource __result)
        {
            SpellTypeOverride component = __instance.Blueprint.GetComponent<SpellTypeOverride>();
            if ((bool)((UnityEngine.Object)component))
            {
                __result = component.Type;
                return false;
            }
            if ((UnityEngine.Object)__instance.SourceItemUsableBlueprint != (UnityEngine.Object)null && __instance.SourceItemUsableBlueprint.Type != UsableItemType.Scroll || __instance.Blueprint.Type != AbilityType.Spell && __instance.Blueprint.Type != AbilityType.SpellLike)
            {
                __result = SpellSource.None;
                return false;
            }
            if (__instance.Spellbook != null)
            {
                __result = __instance.Spellbook.Blueprint.IsArcane ? SpellSource.Arcane : SpellSource.Divine;
                return false;
            }

            Kingmaker.UnitLogic.Feature feature = __instance.Caster.Progression.Features.Enumerable.FirstOrDefault<Kingmaker.UnitLogic.Feature>((Func<Kingmaker.UnitLogic.Feature, bool>)(f => f.Blueprint.GetComponents<AddFacts>().Any<AddFacts>((Func<AddFacts, bool>)(af => ((IEnumerable<BlueprintUnitFact>)af.Facts).Contains<BlueprintUnitFact>((BlueprintUnitFact)__instance.Blueprint)))));
            BlueprintCharacterClass blueprintCharacterClass1;
            if (feature == null)
            {
                blueprintCharacterClass1 = (BlueprintCharacterClass)null;
            }
            else
            {
                BlueprintProgression sourceProgression = feature.SourceProgression;
                blueprintCharacterClass1 = sourceProgression != null ? ((IList<BlueprintCharacterClass>)sourceProgression.Classes).FirstOrDefault<BlueprintCharacterClass>() : (BlueprintCharacterClass)null;
            }
            BlueprintCharacterClass blueprintCharacterClass2 = blueprintCharacterClass1;
            if (!(bool)((UnityEngine.Object)blueprintCharacterClass2))
            {
                __result = SpellSource.Unknown;
                return false;
            }
            var base_spellbook = __instance.Caster?.GetSpellbook(blueprintCharacterClass2);
            if (base_spellbook == null)
            {
                __result = blueprintCharacterClass2.IsDivineCaster ? SpellSource.Divine : (blueprintCharacterClass2.IsArcaneCaster ? SpellSource.Arcane : SpellSource.None);
                return false;
            }
            __result = base_spellbook.Blueprint.IsArcane ? SpellSource.Arcane : (base_spellbook.Blueprint.IsAlchemist ? SpellSource.None : SpellSource.Divine);
            return false;
        }
    }
}
