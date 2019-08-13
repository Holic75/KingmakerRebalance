using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public enum ContextRankBaseValueTypeExtender
    {
        None,
        MasterClassLevel, //ClassLevel or Master class level if pet
        MasterMaxClassLevelWithArchetype, //MaxClassLevelWithArchetype or Master class level if pet
    }

    public static partial class Extensions
    {
        public static ContextRankBaseValueType ToContextRankBaseValueType(this ContextRankBaseValueTypeExtender base_value)
        {
            int value = EnumUtils.GetMaxValue<ContextRankBaseValueType>() + (int)base_value;
            return (ContextRankBaseValueType)value;
        }


        public static ContextRankBaseValueTypeExtender ToContextRankBaseValueTypeExtender(this ContextRankBaseValueType base_value)
        {
            int value = (int)base_value - (int)ContextRankBaseValueTypeExtender.None.ToContextRankBaseValueType();
            if (value <= 0)
            {
                return 0;
            }
            return (ContextRankBaseValueTypeExtender)(value);
        }
    }


    [Harmony12.HarmonyPatch(typeof(ContextRankConfig))]
    [Harmony12.HarmonyPatch("GetBaseValue", Harmony12.MethodType.Normal)]
    class ContextRankConfig__GetBaseValue__Patch
    {
        static int getClassLevelRank(UnitDescriptor unit, ContextRankConfig rank_config)
        {
            if (unit == null)
            {
                return 0;
            }
            var tr = Harmony12.Traverse.Create(rank_config);
            bool m_ExceptClasses = tr.Field("m_ExceptClasses").GetValue<bool>();
            BlueprintCharacterClass[] m_Class = tr.Field("m_Class").GetValue<BlueprintCharacterClass[]>();
            ContextRankBaseValueType m_BaseValueType = tr.Field("m_BaseValueType").GetValue<ContextRankBaseValueType>();

            int rankBonus = 0;
            if (m_BaseValueType == ContextRankBaseValueTypeExtender.MasterClassLevel.ToContextRankBaseValueType())
            {
                foreach (Kingmaker.UnitLogic.ClassData classData in unit.Progression.Classes)
                {
                    if (m_ExceptClasses && !((IList<BlueprintCharacterClass>)m_Class).HasItem<BlueprintCharacterClass>(classData.CharacterClass) || !m_ExceptClasses && ((IList<BlueprintCharacterClass>)m_Class).HasItem<BlueprintCharacterClass>(classData.CharacterClass))
                        rankBonus += classData.Level;
                }
            }
            else if (m_BaseValueType == ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType())
            {
                BlueprintArchetype Archetype = tr.Field("Archetype").GetValue<BlueprintArchetype>();
                foreach (Kingmaker.UnitLogic.ClassData classData in unit.Progression.Classes)
                {
                    if ((m_ExceptClasses && !((IList<BlueprintCharacterClass>)m_Class).HasItem<BlueprintCharacterClass>(classData.CharacterClass) || !m_ExceptClasses && ((IList<BlueprintCharacterClass>)m_Class).HasItem<BlueprintCharacterClass>(classData.CharacterClass)) && (!((IEnumerable<BlueprintArchetype>)classData.CharacterClass.Archetypes).Contains<BlueprintArchetype>(Archetype) || classData.Archetypes.Contains(Archetype)))
                        rankBonus = Math.Max(rankBonus, classData.Level);
                }
            }
            return rankBonus;

        }

        static int getMasterRank(UnitDescriptor unit, ContextRankConfig rank_config)
        {
            if (unit == null)
            {
                return 0;
            }

            if (!unit.IsPet)
            {
                return getClassLevelRank(unit, rank_config);
            }
            else
            {
                return getClassLevelRank(unit.Master.Value.Descriptor, rank_config);
            }
        }

        static bool Prefix(ContextRankConfig __instance, MechanicsContext context, ContextRankBaseValueType ___m_BaseValueType, ref int __result)
        {
            if (___m_BaseValueType == ContextRankBaseValueTypeExtender.MasterClassLevel.ToContextRankBaseValueType()
                || ___m_BaseValueType == ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType())
            {
                int rankBonus1 = context.Params.RankBonus;
                var caster = context.MaybeCaster;

                __result = rankBonus1 + getMasterRank(caster.Descriptor, __instance);
                return false;
            }
            else
            {
                return true;
            }

        }
    }

}
