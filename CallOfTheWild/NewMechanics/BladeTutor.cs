using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.BladeTutor
{
    class BladeTutorUnitPart: AdditiveUnitPart
    {
        public (int, Fact) getValue()
        {
            int val = 0;
            Fact best_fact = null;
            foreach (var b in buffs)
            {
                if (b.Blueprint.GetComponent<BladeTutor>() != null)
                {
                    int val1 = 0;
                    b.CallComponents<BladeTutor>(bt => val1 = bt.getValue());
                    if (val1 > val)
                    {
                        val = val1;
                        best_fact = b;
                    }
                }
            }
            return (val, best_fact);
        }
    }


    public class BladeTutor : OwnedGameLogicComponent<UnitDescriptor>
    {
        public ContextValue value;

        public override void OnFactActivate()
        {
            this.Owner.Ensure<BladeTutorUnitPart>().addBuff(this.Fact);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<BladeTutorUnitPart>().removeBuff(this.Fact);
        }

        public int getValue()
        {
            return value.Calculate(this.Fact.MaybeContext);
        }
    }



    [Harmony12.HarmonyPatch(typeof(RuleCalculateAttackBonusWithoutTarget))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    static class RuleCalculateAttackBonusWithoutTarget_OnTrigger_Patch
    {
        public static HashSet<BlueprintUnitFact> facts = new HashSet<BlueprintUnitFact>
        {
            Main.library.Get<BlueprintBuff>("5898bcf75a0942449a5dc16adc97b279"), //power attack - rule
            Main.library.Get<BlueprintBuff>("8af258b1dd322874ba6047b0c24660c7"), //piranha strike - rule
            //add furious focus
            Main.library.Get<BlueprintBuff>("e81cd772a7311554090e413ea28ceea1"), //combat expertise - stat
            Main.library.Get<BlueprintBuff>("6ffd93355fb3bcf4592a5d976b1d32a9"), //fight defensively - stat
            //add same from stalwart
            Main.library.Get<BlueprintFeature>("6948b379c0562714d9f6d58ccbfa8faa"), //twf - rule
        };

        internal static bool Prefix(RuleCalculateAttackBonusWithoutTarget __instance, RulebookEventContext context)
        {
            if (__instance.Weapon == null || !__instance.Weapon.Blueprint.IsMelee)
            {
                return true;
            }
            var blade_tutor_part = __instance?.Initiator?.Get<BladeTutorUnitPart>();
            if (blade_tutor_part == null)
            {
                return true;
            }

            var res = blade_tutor_part.getValue();
            if (res.Item2 == null || res.Item1 == 0)
            {
                return true;
            }

            int total_penalty = 0;
            var attack_bonus = __instance.Initiator.Stats.GetStat(StatType.AdditionalAttackBonus);

            foreach (var m in attack_bonus.Modifiers)
            {
                if (m.Source?.Blueprint != null && facts.Contains(m.Source.Blueprint))
                {
                    total_penalty -= m.ModValue;
                }
            }

            foreach (var m in __instance.BonusSources)
            {
                if (m.Source?.Blueprint != null && facts.Contains(m.Source.Blueprint))
                {
                    total_penalty -= m.Bonus;
                }
            }

            int bonus = Math.Min(total_penalty, res.Item1);

            __instance.AddBonus(bonus, res.Item2);


            return true;
        }
    }

}
