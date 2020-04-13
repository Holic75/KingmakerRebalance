using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SpellFailureMechanics
{
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
        public int chance;

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
            if (!evt.Spell.Blueprint.IsSpell || (evt.Spell.StickyTouch != null))
                return;
            evt.SpellFailureChance = Math.Max(evt.SpellFailureChance, this.chance);
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {

        }
    }


    class PsychicSpellbook : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>,  IInitiatorRulebookSubscriber
    {
        public BlueprintSpellbook spellbook;

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
            if (evt.Spell?.Spellbook?.Blueprint != spellbook)
            {
                return;
            }

            foreach (var b in evt.Initiator.Buffs)
            {
                if ((b.Context.SpellDescriptor & (SpellDescriptor.NegativeEmotion | SpellDescriptor.Fear | SpellDescriptor.Shaken)) != 0)
                {
                    evt.SpellFailureChance = Math.Max(evt.SpellFailureChance, 100);
                    break;
                }
            }

            
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {

        }


        void IRulebookHandler<RuleCalculateAbilityParams>.OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt.Spellbook?.Blueprint != spellbook)
            {
                return;
            }
            evt.AddBonusConcentration(-10);
        }

        void IRulebookHandler<RuleCalculateAbilityParams>.OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
           
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
