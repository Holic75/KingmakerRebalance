using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.WeatherMechanics
{
    class UnitPartIgnoreWeatherMovementEffects : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitEntityData))]
    [Harmony12.HarmonyPatch("CalculateSpeedModifier", Harmony12.MethodType.Normal)]
    class Patch_UnitEntityData_CalculateSpeedModifier
    {
        static public void Postfix(UnitEntityData __instance, ref float __result)
        {
            if (Game.Instance.CurrentlyLoadedArea.IsCapital)
            {
                return;
            }

            var unit_part_ignore_wheather_movement = __instance.Get<UnitPartIgnoreWeatherMovementEffects>();
            if (unit_part_ignore_wheather_movement != null 
                && unit_part_ignore_wheather_movement.active()
                && Game.Instance.Player.Weather.ActualWeather >= BlueprintRoot.Instance.WeatherSettings.SlowdownBonusBeginsOn
                && __result <= 0.51f)
            {
                __result *= 2.0f;
            }
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class IgnoreWhetherMovementEffects : OwnedGameLogicComponent<UnitDescriptor>
    {

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartIgnoreWeatherMovementEffects>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartIgnoreWeatherMovementEffects>().removeBuff(this.Fact);
        }
    }

}
