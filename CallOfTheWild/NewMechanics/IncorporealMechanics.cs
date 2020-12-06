using Kingmaker.Blueprints;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.IncorporealMechanics
{
    class UnitPartGhostbaneDirge : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }


    public class GhostbaneDirge : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartGhostbaneDirge>().addBuff(this.Fact);

        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartGhostbaneDirge>().removeBuff(this.Fact);
        }
    }



    [Harmony12.HarmonyPatch(typeof(AddIncorporealDamageDivisor), "OnEventAboutToTrigger")]
    static class AddIncorporealDamageDivisor_OnEventAboutToTrigger_Patch
    {
        internal static bool Prefix(AddIncorporealDamageDivisor __instance, RuleCalculateDamage evt)
        {
            if (!(evt.Target.Get<UnitPartGhostbaneDirge>()?.active()).GetValueOrDefault())
            {
                return true;
            }

            foreach (BaseDamage baseDamage in evt.DamageBundle)
            {
                if (baseDamage.Reality != DamageRealityType.Ghost)
                {
                    PhysicalDamage physicalDamage = baseDamage as PhysicalDamage;
                    if (physicalDamage != null)
                    {
                        if (physicalDamage.EnchantmentTotal == 0)
                            baseDamage.Half = true;
                    }
                }
            }

            return false;
        }
    }
}
