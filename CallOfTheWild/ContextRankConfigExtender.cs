using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
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
        MasterFeatureRank,
        SummClassLevelWithArchetypes,
        MaxClassLevelWithArchetypes,
        ClassLevelPlusStatValue,
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
    [Harmony12.HarmonyPatch("IsBasedOnClassLevel", Harmony12.MethodType.Getter)]
    class ContextRankConfig__IsBasedOnClassLevel__Patch
    {
        static void Postfix(ContextRankConfig __instance, ContextRankBaseValueType ___m_BaseValueType,  ref bool __result)
        {
            if (!__result)
            {
                __result = ___m_BaseValueType == ContextRankBaseValueTypeExtender.ClassLevelPlusStatValue.ToContextRankBaseValueType()
                          || ___m_BaseValueType == ContextRankBaseValueTypeExtender.MasterClassLevel.ToContextRankBaseValueType()
                          || ___m_BaseValueType == ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType()
                          || ___m_BaseValueType == ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType();
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(ContextRankConfig))]
    [Harmony12.HarmonyPatch("GetBaseValue", Harmony12.MethodType.Normal)]
    class ContextRankConfig__GetBaseValue__Patch
    {
        static int getClassLevelRank(UnitDescriptor unit, ContextRankConfig rank_config)
        {
            Main.TraceLog();
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
                if (!m_ExceptClasses)
                {
                    rankBonus += FakeClassLevelMechanics.Helpers.calculateFakeClassLevel(unit, m_Class, new BlueprintArchetype[0]);
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
                if (!m_ExceptClasses)
                {
                    rankBonus = Math.Max(FakeClassLevelMechanics.Helpers.calculateFakeClassLevel(unit, m_Class, new BlueprintArchetype[0]), rankBonus);
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

        static bool Prefix(ContextRankConfig __instance, MechanicsContext context, ContextRankBaseValueType ___m_BaseValueType, bool ___m_ExceptClasses, StatType ___m_Stat,
                           BlueprintFeature ___m_Feature, BlueprintCharacterClass[] ___m_Class, BlueprintArchetype ___Archetype,  ref int __result)
        {
            int rankBonus1 = context.Params.RankBonus;
            if (___m_BaseValueType == ContextRankBaseValueTypeExtender.MasterClassLevel.ToContextRankBaseValueType()
                || ___m_BaseValueType == ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType())
            {
                
                var caster = context.MaybeCaster;

                __result = rankBonus1 + getMasterRank(caster.Descriptor, __instance);
                return false;
            }
            else if (___m_BaseValueType == ContextRankBaseValueTypeExtender.MasterFeatureRank.ToContextRankBaseValueType())
            {
                if (context.MaybeCaster.Descriptor.IsPet)
                {
                    __result =  context.MaybeCaster.Descriptor.Master.Value.Descriptor.Progression.Features.GetRank(___m_Feature);
                }
                else
                {
                    __result = context.MaybeCaster.Descriptor.Progression.Features.GetRank(___m_Feature);
                }
                return false;
            }
            else if (___m_BaseValueType == ContextRankBaseValueTypeExtender.ClassLevelPlusStatValue.ToContextRankBaseValueType())
            {
                foreach (Kingmaker.UnitLogic.ClassData classData in context.MaybeCaster.Descriptor.Progression.Classes)
                {
                    if (___m_ExceptClasses && !((IList<BlueprintCharacterClass>)___m_Class).HasItem<BlueprintCharacterClass>(classData.CharacterClass) || !___m_ExceptClasses && ((IList<BlueprintCharacterClass>)___m_Class).HasItem<BlueprintCharacterClass>(classData.CharacterClass))
                        rankBonus1 += classData.Level;
                }
                int? bonus = context.MaybeCaster.Descriptor.Stats.GetStat<ModifiableValueAttributeStat>(___m_Stat)?.Bonus;
                rankBonus1 =  !bonus.HasValue ? rankBonus1 : rankBonus1 + bonus.Value;
                __result = rankBonus1;
                if (!___m_ExceptClasses)
                {
                    __result += FakeClassLevelMechanics.Helpers.calculateFakeClassLevel(context.MaybeCaster.Descriptor, ___m_Class, new BlueprintArchetype[0]);
                }
                return false;
            }
            else if (___m_BaseValueType == ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType())
            {
                __result = 0;
                var archetypes_list = ___m_Feature.GetComponent<ContextRankConfigArchetypeList>().archetypes;
                if (___Archetype != null)
                {
                    archetypes_list = archetypes_list.AddToArray(___Archetype);
                }
                foreach (var c in context.MaybeCaster.Descriptor.Progression.Classes)
                {
                    if (!___m_Class.Contains(c.CharacterClass))
                    {
                        continue;
                    }
                    var class_archetypes = archetypes_list.Where(a => a.GetParentClass() == c.CharacterClass).ToArray();
                    if (class_archetypes.Empty())
                    {
                        __result += c.Level;
                    }
                    else
                    {
                        foreach (var a in c.Archetypes)
                        {
                            if (class_archetypes.Contains(a))
                            {
                                __result += c.Level;
                                break;
                            }
                        }
                    }
                }
                __result += FakeClassLevelMechanics.Helpers.calculateFakeClassLevel(context.MaybeCaster.Descriptor, ___m_Class, archetypes_list) + rankBonus1;
                return false;
            }
            else if (___m_BaseValueType == ContextRankBaseValueTypeExtender.MaxClassLevelWithArchetypes.ToContextRankBaseValueType())
            {
                __result = 0;
                var archetypes_list = ___m_Feature.GetComponent<ContextRankConfigArchetypeList>().archetypes;
                if (___Archetype != null)
                {
                    archetypes_list = archetypes_list.AddToArray(___Archetype);
                }
                foreach (var c in context.MaybeCaster.Descriptor.Progression.Classes)
                {
                    if (!___m_Class.Contains(c.CharacterClass))
                    {
                        continue;
                    }
                    var class_archetypes = archetypes_list.Where(a => a.GetParentClass() == c.CharacterClass).ToArray();
                    if (class_archetypes.Empty())
                    {
                        __result = Math.Max(c.Level, __result);
                    }
                    else
                    {
                        foreach (var a in c.Archetypes)
                        {
                            if (class_archetypes.Contains(a))
                            {
                                __result = Math.Max(c.Level, __result);
                                break;
                            }
                        }
                    }
                }
                __result = Math.Max(FakeClassLevelMechanics.Helpers.calculateFakeClassLevel(context.MaybeCaster.Descriptor, ___m_Class, archetypes_list), __result) + rankBonus1;
                return false;
            }
            else
            {
                return true;
            }
        }
    }


    public class ContextRankConfigArchetypeList: BlueprintComponent
    {
        public BlueprintArchetype[] archetypes;
    }

}
