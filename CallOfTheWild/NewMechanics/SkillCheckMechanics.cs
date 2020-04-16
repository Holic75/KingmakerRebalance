using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SkillMechanics
{

    //make dialog skill checks account for temporary skill bonuses if possible
    [Harmony12.HarmonyPatch(typeof(RulePartySkillCheck))]
    [Harmony12.HarmonyPatch("Calculate", Harmony12.MethodType.Normal)]
    class Patch_RulePartySkillCheck_Calculate_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_d20_index = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("D20"));

            codes[check_d20_index] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                   new Func<RuleSkillCheck, int>(getD20Value).Method);
            return codes.AsEnumerable();
        }


        static private int getD20Value(RuleSkillCheck evt)
        {
            if (evt.IsTriggererd)
            {
                return evt.D20 + evt.Bonus;
            }
            else
            {
                return evt.D20;
            }
        }
    }


    class SetSkillRankToValue : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler
    {
        public StatType skill;
        public ContextValue value;
        [JsonProperty]
        private int previous_value = 0;

        public override void OnFactActivate()
        {
            try
            {
                if (previous_value <= 0)
                {
                    var LevelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                    if (base.Owner == LevelUp.Preview || base.Owner == LevelUp.Unit)
                    {
                        var @class = LevelUp.State.SelectedClass;
                        //allow retraining
                        LevelUp.State.ExtraSkillPoints += base.Owner.Stats.GetStat(skill).BaseValue;

                        HandleUnitGainLevel(this.Owner, @class);
                    }
                    ModifiableValueSkill stat = this.Owner.Stats.GetStat<ModifiableValueSkill>(skill);
                    stat?.ClassSkill.Retain();
                    stat?.UpdateValue();
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }

        public override void OnFactDeactivate()
        {
            if (previous_value > 0)
            {
                base.Owner.Stats.GetStat(skill).BaseValue = 0;
                previous_value = 0;
            }
            ModifiableValueSkill stat = this.Owner.Stats.GetStat<ModifiableValueSkill>(skill);
            stat?.ClassSkill.Release();
            stat?.UpdateValue();
            base.OnFactDeactivate();
        }

        public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
        {
            int old_value = previous_value;
            int new_value = this.value.Calculate(this.Fact.MaybeContext);

            base.Owner.Stats.GetStat(skill).BaseValue += (new_value - old_value);
            previous_value = new_value;
        }
    }
}
