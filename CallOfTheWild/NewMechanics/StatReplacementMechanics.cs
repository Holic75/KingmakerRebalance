using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Actions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.StatReplacementMechanics
{

    public class UnitPartRplaceACStat : AdditiveUnitPart
    {
        public bool active()
        {
            return !buffs.Empty();
        }


        public StatType GetACStat()
        {
            var stat = StatType.Dexterity;

            foreach (var b in buffs)
            {
                var buff_stat = b.Blueprint.GetComponent<ReplaceACStat>().stat;

                if (this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(stat).Bonus < this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(buff_stat).Bonus)
                {
                    stat = buff_stat;
                }
            }

            return stat;
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ReplaceACStat : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public StatType stat;

        public override void OnTurnOn()
        {
            this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(stat)?.AddDependentValue(this.Owner.Stats.AC);
            this.Owner.Ensure<UnitPartRplaceACStat>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(stat)?.RemoveDependentValue(this.Owner.Stats.AC);
            this.Owner.Ensure<UnitPartRplaceACStat>().removeBuff(this.Fact);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ReplaceStatForSavingthrow : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {
        public StatType dependent_value;
        public StatType stat;
        [JsonProperty]
        private StatType _oldStat;

        public override void OnTurnOn()
        {
            this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(stat)?.AddDependentValue(this.Owner.Stats.GetStat(dependent_value));
            _oldStat = this.Owner.Stats.GetStat<ModifiableValueSavingThrow>(dependent_value).BaseStat.Type;
            Helpers.SetField(this.Owner.Stats.GetStat<ModifiableValueSavingThrow>(dependent_value), "BaseStat",  this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(stat));
        }


        public override void OnTurnOff()
        {
            this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(stat)?.RemoveDependentValue(this.Owner.Stats.GetStat(dependent_value));
            Helpers.SetField(this.Owner.Stats.GetStat<ModifiableValueSavingThrow>(dependent_value), "BaseStat", this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(_oldStat));
        }
    }


    public class ReplaceBaseStatForStatTypeLogic : OwnedGameLogicComponent<UnitDescriptor>
    {
        public StatType StatTypeToReplaceBastStatFor;
        public StatType NewBaseStatType;
        public bool only_if_greater = true;
        [JsonProperty]
        private StatType? _oldStatType = null;

        public override void OnTurnOn()
        {
            ModifiableValue value = base.Owner.Stats.GetStat(StatTypeToReplaceBastStatFor);
            if (value.GetType() == typeof(ModifiableValueSkill))
            {
                if (_oldStatType == null)
                {
                    _oldStatType = (value as ModifiableValueSkill)?.BaseStat.Type;
                }

                ModifiableValueAttributeStat oldStat = base.Owner.Stats.GetStat<ModifiableValueAttributeStat>((StatType)_oldStatType);
                ModifiableValueAttributeStat newStat = base.Owner.Stats.GetStat<ModifiableValueAttributeStat>(NewBaseStatType);

                if (newStat.Bonus < oldStat.Bonus && only_if_greater)
                {
                    return;
                }

                Traverse traverse = Traverse.Create(value);
                traverse.Field("BaseStat").SetValue(newStat);
                newStat.AddDependentValue(value);
                oldStat.RemoveDependentValue(value);
                value.UpdateValue();
            }
        }

        public override void OnTurnOff()
        {
            ModifiableValue value = base.Owner.Stats.GetStat(StatTypeToReplaceBastStatFor);
            if (value.GetType() == typeof(ModifiableValueSkill) && _oldStatType != null)
            {
                ModifiableValue oldStat = base.Owner.Stats.GetStat((StatType)_oldStatType);
                ModifiableValue newStat = base.Owner.Stats.GetStat(NewBaseStatType);

                Traverse traverse = Traverse.Create(value);
                traverse.Field("BaseStat").SetValue(oldStat);
                oldStat.AddDependentValue(value);
                newStat.RemoveDependentValue(value);
                value.UpdateValue();
            }
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ReplaceCMDStat : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateBaseCMD>, IUnitSubscriber
    {
        public StatType stat;

        public void OnEventAboutToTrigger(RuleCalculateBaseCMD evt)
        {
           if (this.Owner.Stats.GetStat<ModifiableValueAttributeStat>(stat).Bonus > this.Owner.Stats.Dexterity.Bonus)
            {
                evt.ReplaceDexterity = stat;
            }
        }

        public void OnEventDidTrigger(RuleCalculateBaseCMD evt)
        {
            
        }
    }


    [Harmony12.HarmonyPatch(typeof(ModifiableValueArmorClass))]
    [Harmony12.HarmonyPatch("UpdateInternalModifiers", Harmony12.MethodType.Normal)]
    class ModifiableValueArmorClass_UpdateInternalModifiers_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_dex_index = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("get_Bonus"));

            codes[check_dex_index - 1] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
                                                                           new Func<CharacterStats, ModifiableValueAttributeStat>(getAcStat).Method
                                                                  );

            return codes.AsEnumerable();
        }


        static private ModifiableValueAttributeStat getAcStat(CharacterStats character_stats)
        {
            Main.TraceLog();
            var repalce_ac_part = character_stats.Owner?.Get<UnitPartRplaceACStat>();

            if (repalce_ac_part == null || !repalce_ac_part.active())
            {
                return character_stats.Dexterity;
            }
            else
            {
                return character_stats.GetStat<ModifiableValueAttributeStat>(repalce_ac_part.GetACStat());
            }
        }
    }
}

