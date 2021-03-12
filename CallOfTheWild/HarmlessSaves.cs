using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace CallOfTheWild.HarmlessSaves
{
    public class UnitPartSaveAgainstHarmlessSpells : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SaveAgainstHarmlessSpells : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartSaveAgainstHarmlessSpells>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartSaveAgainstHarmlessSpells>().removeBuff(this.Fact);
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    public class HarmlessSpell : BlueprintComponent
    {
        public SavingThrowType save_type = SavingThrowType.Will;
    }


    [AllowedOn(typeof(BlueprintAbility))]
    public class HarmlessHealSpell : BlueprintComponent
    {
        public SavingThrowType save_type = SavingThrowType.Will;
    }


    public class HarmlessSaves
    {
        static public string fort_harmless = "Fortitude negates (harmless)";
        static public string will_harmless = "Will negates (harmless)";
        static internal void init()
        {
            var will_saves_guids = Helpers.readStringsfromFile(UnityModManager.modsPath + @"/CallOfTheWild/HarmlessSpells/harmless_will_saves.txt", ' ');
            var will_saves_heal_guids = Helpers.readStringsfromFile(UnityModManager.modsPath + @"/CallOfTheWild/HarmlessSpells/heal_will_saves.txt", ' ');
            var fort_saves_guids = Helpers.readStringsfromFile(UnityModManager.modsPath + @"/CallOfTheWild/HarmlessSpells/harmless_fort_saves.txt", ' ');
            foreach (var w in will_saves_guids)
            {
                var s = Main.library.Get<BlueprintAbility>(w);
                s.AddComponent(Helpers.Create<HarmlessSpell>(h => h.save_type = SavingThrowType.Will));
                if (s.LocalizedSavingThrow.IsEmpty() || s.LocalizedSavingThrow.ToString() == Helpers.savingThrowNone)
                {
                    s.LocalizedSavingThrow = Helpers.CreateString(s.name + "LocalizedSavingThrow", will_harmless);
                }
            }

            foreach (var f in fort_saves_guids)
            {
                var s = Main.library.Get<BlueprintAbility>(f);
                s.AddComponent(Helpers.Create<HarmlessSpell>(h => h.save_type = SavingThrowType.Fortitude));
                if (s.LocalizedSavingThrow.IsEmpty() || s.LocalizedSavingThrow.ToString() == Helpers.savingThrowNone)
                {
                    s.LocalizedSavingThrow = Helpers.CreateString(s.name + "LocalizedSavingThrow", fort_harmless);
                }
            }


            foreach (var w in will_saves_heal_guids)
            {
                var s = Main.library.Get<BlueprintAbility>(w);
                s.AddComponent(Helpers.Create<HarmlessHealSpell>(h => h.save_type = SavingThrowType.Will));
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(AbilityEffectRunAction), "Apply", typeof(AbilityExecutionContext), typeof(TargetWrapper))]
    static class AbilityEffectRunAction_Apply_Patch
    {
        internal static bool Prefix(AbilityExecutionContext context, TargetWrapper target)
        {
            if (!target.IsUnit)
            {
                return true;
            }
            if (context?.Params == null || context.MaybeCaster == null)
            {
                return true;
            }

            if (!(target.Unit.Get<UnitPartSaveAgainstHarmlessSpells>()?.active()).GetValueOrDefault())
            {
                return true;
            }

            if (target.Unit == context.MaybeCaster || !target.Unit.IsAlly(context.MaybeCaster))
            {
                return true;
            }

            if (context.AbilityBlueprint?.GetComponent<HarmlessSpell>() == null || !context.AbilityBlueprint.IsSpell)
            {
                return true;
            }

            RuleSavingThrow ruleSavingThrow = new RuleSavingThrow(target.Unit, context.AbilityBlueprint.GetComponent<HarmlessSpell>().save_type, context.Params.DC);
            ruleSavingThrow.Reason = (RuleReason)((MechanicsContext)context);
            if (context.TriggerRule<RuleSavingThrow>(ruleSavingThrow).IsPassed)
            {
                return false;
            }

            return true;
        }
    }
}
