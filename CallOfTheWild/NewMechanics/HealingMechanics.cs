using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.HealingMechanics
{
    public class UnitPartReceiveBonusCasterLevelHealing : UnitPart
    {
        [JsonProperty]
        private List<Fact> buffs = new List<Fact>();

        public void addBuff(Fact buff)
        {
            buffs.Add(buff);
        }


        public void removeBuff(Fact buff)
        {
            buffs.Remove(buff);
        }


        public bool active()
        {
            return !buffs.Empty();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ReceiveBonusCasterLevelHealing : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartReceiveBonusCasterLevelHealing>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartReceiveBonusCasterLevelHealing>().removeBuff(this.Fact);
        }
    }


    [Harmony12.HarmonyPatch(typeof(ContextActionHealTarget))]
    [Harmony12.HarmonyPatch("RunAction", Harmony12.MethodType.Normal)]
    class Patch_ContextActionHealTarget_RunAction_Postfix
    {

        static public void Postfix(ContextActionHealTarget __instance)
        {
            var tr = Harmony12.Traverse.Create(__instance);
            var context = tr.Property("Context").GetValue<MechanicsContext>();
            var target = tr.Property("Target").GetValue<TargetWrapper>().Unit;
            if (target == null || context.MaybeCaster == null)
                return;


            if (context.SourceAbilityContext?.Ability?.SourceItem != null)
            {
                return;
            }

            if (target.Descriptor.Ensure<UnitPartReceiveBonusCasterLevelHealing>().active() && context.SourceAbility != null && __instance.Value != null)
            {
                int dice_count = __instance.Value.DiceCountValue.Calculate(context);
                var dice = __instance.Value.DiceType;
                if (dice_count == 0 || dice == DiceType.Zero)
                {
                    return;
                }
                int bonus = context.Params.CasterLevel;
                context.TriggerRule<RuleHealDamage>(new RuleHealDamage(target, target, DiceFormula.Zero, bonus));
            }
        }
    }
}
