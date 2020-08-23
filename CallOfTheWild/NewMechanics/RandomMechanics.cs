using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.RandomMechanics
{
      class UnitPartSeedStorage: UnitPart
      {
        [JsonProperty]
        int seed;
        [JsonProperty]
        bool initialized;


        private void maybeInitialize()
        {
            if (initialized)
            {
                return;
            }

            seed = new Random().Next(0, 1000000);
            initialized = true;
        }

        public int getRandom(int a, int b)
        {
            maybeInitialize();
            int val = new Random(seed).Next(a, b + 1);
            int new_seed = seed;
            var rnd = new Random(seed);
            while (new_seed == seed)
            {
               new_seed = rnd.Next(0, 1000000);
            }
            seed = new_seed;

            return val;
        }
      }


    public class RunActionDependingOnD100 : ContextAction
    {
        public int[] thresholds;
        public ActionList[] actions;
        public ContextDiceValue in_combat_bonus;

        public override void RunAction()
        {
            if (this.Target.Unit == null || this.Context.MaybeCaster == null)
            {
                UberDebug.LogError((object)"Target unit is missing", (object[])Array.Empty<object>());
            }
            else
            {
                int value = this.Target.Unit.Ensure<UnitPartSeedStorage>().getRandom(1, 100);
                if (this.Target.Unit.IsInCombat && in_combat_bonus != null)
                {
                    value += in_combat_bonus.Calculate(this.Context);
                }
                //Main.logger.Log("Rolled: " + value.ToString());
                Common.AddBattleLogMessage(this.Target.Unit.CharacterName + " rolls " + value.ToString() + " on d100 for " + this.Context.Name);
                for (int i = 0; i < actions.Length; i++)
                {
                    if (thresholds[i] >= value)
                    {
                        if (actions[i] != null)
                        {
                            actions[i].Run();
                        }
                        return;
                    }
                }
            }
        }

        public override string GetCaption()
        {
            return string.Format("Run action based on d100");
        }
    }
}
