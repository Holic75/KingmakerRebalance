using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.TravelMap
{
    class UnitPartOverrideTravelSpeedMultiplier : AdditiveUnitPart
    {
        public float getTravelSpeedMultiplier()
        {
            float max_speed_multiplier = 0.0f;
            foreach (var b in buffs)
            {
                float speed_multiplier = 0.0f;
                b.CallComponents<OverrideGlobalMapTravelMultiplier>(c => speed_multiplier = c.getTravelSpeedMultiplier());

                if (max_speed_multiplier < speed_multiplier)
                {
                    max_speed_multiplier = speed_multiplier;
                }
            }

            return max_speed_multiplier;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class OverrideGlobalMapTravelMultiplier : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public ContextValue travel_map_multiplier;

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartOverrideTravelSpeedMultiplier>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartOverrideTravelSpeedMultiplier>().removeBuff(this.Fact);
        }


        public float getTravelSpeedMultiplier()
        {
            return travel_map_multiplier.Calculate(this.Fact.MaybeContext);
        }
    }


    [Harmony12.HarmonyPatch(typeof(MapMovementController))]
    [Harmony12.HarmonyPatch("CalcSpeedModifiers", Harmony12.MethodType.Normal)]
    class Spellbook__AddCasterLevel__Patch
    {
        static void Postfix(ref float __result)
        {
            Main.TraceLog();
            float max_speed_multiplier = 0.0f;
            foreach (var u in Game.Instance.Player.PartyCharacters)
            {
                var override_speed = u.Value.Get<UnitPartOverrideTravelSpeedMultiplier>();
                if (override_speed != null)
                {
                    max_speed_multiplier = Math.Max(max_speed_multiplier, override_speed.getTravelSpeedMultiplier());
                }
            }

            if (max_speed_multiplier >= 0.001f)
            {
                __result = max_speed_multiplier;
            }
        }
    }
}
