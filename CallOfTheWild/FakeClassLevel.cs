using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
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

namespace CallOfTheWild.FakeClassLevelMechanics
{
    public class UnitPartFakeClassLevel : AdditiveUnitPart
    {
        public int getFakeClassLevel(BlueprintCharacterClass character_class)
        {
            int lvl = 0;
            foreach (var b in buffs)
            {
                b.CallComponents<FakeClassLevelsBase>(f => lvl += f.getFakeClassLevel(character_class));
            }
            return lvl;
        }
    }


    public class FakeClass: BlueprintComponent
    {

    }


    public abstract class FakeClassLevelsBase : OwnedGameLogicComponent<UnitDescriptor>
    {
        public abstract int getFakeClassLevel(BlueprintCharacterClass character_class);

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartFakeClassLevel>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartFakeClassLevel>().removeBuff(this.Fact);
        }
    }


    public class AddFakeClassLevel : FakeClassLevelsBase
    {
        public BlueprintCharacterClass fake_class;
        public ContextValue value;

        public override int getFakeClassLevel(BlueprintCharacterClass character_class)
        {
            if (character_class != fake_class)
            {
                return 0;
            }

            this.Fact.MaybeContext.Recalculate();
            return value.Calculate(this.Fact.MaybeContext);
        }
    }


    public class Helpers
    {
        static public int calculateFakeClassLevel(UnitDescriptor descriptor, BlueprintCharacterClass[] classes, BlueprintArchetype[] archetypes)
        {
            var unit_part_fake_class = descriptor?.Get<UnitPartFakeClassLevel>();
            if (unit_part_fake_class == null)
            {
                return 0;
            }
            int lvl = 0;
            foreach (var c in classes)
            {
                if (archetypes.Any(a => a.GetParentClass() == c))
                {//fake classes should not have archetypes
                    continue;
                }
                lvl += unit_part_fake_class.getFakeClassLevel(c);
            }
            return lvl;
        }
    }


    [Harmony12.HarmonyPatch(typeof(ContextRankConfig))]
    [Harmony12.HarmonyPatch("GetBaseValue", Harmony12.MethodType.Normal)]
    class ContextRankConfig__GetBaseValue__Patch
    {
        static void Postfix(ContextRankConfig __instance, MechanicsContext context, ContextRankBaseValueType ___m_BaseValueType, bool ___m_ExceptClasses, StatType ___m_Stat,
                   BlueprintFeature ___m_Feature, BlueprintCharacterClass[] ___m_Class, BlueprintArchetype ___Archetype, ref int __result)
        {
            switch (___m_BaseValueType)
            {
                case ContextRankBaseValueType.ClassLevel:
                    __result += FakeClassLevelMechanics.Helpers.calculateFakeClassLevel(context?.MaybeCaster?.Descriptor, ___m_Class, new BlueprintArchetype[0]);
                    return;
                case ContextRankBaseValueType.MaxClassLevelWithArchetype:
                    if (___m_ExceptClasses)
                    {
                        return;
                    }
                    __result = Math.Max(FakeClassLevelMechanics.Helpers.calculateFakeClassLevel(context.MaybeCaster.Descriptor, ___m_Class, new BlueprintArchetype[] { ___Archetype }) + context.Params.RankBonus, __result);
                    return;
                case ContextRankBaseValueType.SummClassLevelWithArchetype:
                    int num3 = context.Params.RankBonus;
                    foreach (Kingmaker.UnitLogic.ClassData classData in context.MaybeCaster.Descriptor.Progression.Classes)
                    {
                        if ((___m_ExceptClasses && !(___m_Class).HasItem<BlueprintCharacterClass>(classData.CharacterClass) || !___m_ExceptClasses && (___m_Class).HasItem<BlueprintCharacterClass>(classData.CharacterClass)) && (!((IEnumerable<BlueprintArchetype>)classData.CharacterClass.Archetypes).Contains<BlueprintArchetype>(___Archetype) || classData.Archetypes.Contains(___Archetype)))
                            num3 += classData.Level + context.Params.RankBonus;
                    }
                    num3 += FakeClassLevelMechanics.Helpers.calculateFakeClassLevel(context?.MaybeCaster?.Descriptor, ___m_Class, new BlueprintArchetype[] { ___Archetype });
                    __result = num3;
                    return;
                case ContextRankBaseValueType.OwnerSummClassLevelWithArchetype:
                    UnitEntityData maybeOwner = context.MaybeOwner;
                    if (maybeOwner == null)
                        return;
                    int num4 = context.Params.RankBonus;
                    foreach (Kingmaker.UnitLogic.ClassData classData in maybeOwner.Descriptor.Progression.Classes)
                    {
                        if ((___m_ExceptClasses && !(___m_Class).HasItem<BlueprintCharacterClass>(classData.CharacterClass) || !___m_ExceptClasses && (___m_Class).HasItem<BlueprintCharacterClass>(classData.CharacterClass)) && (!((IEnumerable<BlueprintArchetype>)classData.CharacterClass.Archetypes).Contains<BlueprintArchetype>(___Archetype) || classData.Archetypes.Contains(___Archetype)))
                            num4 += classData.Level;
                    }
                    num4 += FakeClassLevelMechanics.Helpers.calculateFakeClassLevel(maybeOwner.Descriptor, ___m_Class, new BlueprintArchetype[] { ___Archetype });
                    __result = num4;
                    return;
                default:
                    return;
            }
        }
    }

    [Harmony12.HarmonyPatch(typeof(UnitProgressionData))]
    [Harmony12.HarmonyPatch("GetClassLevel", Harmony12.MethodType.Normal)]
    [Harmony12.HarmonyPatch(new Type[] { typeof(BlueprintCharacterClass) })]
    class UnitProgressionData__GetClassLevel__Patch
    {
        static void Postfix(UnitProgressionData __instance, BlueprintCharacterClass characterClass, ref int __result)
        {
            var unit_fake_class_part = __instance.Owner.Get<UnitPartFakeClassLevel>();
            if (__result == 0 && unit_fake_class_part != null)
            {
                __result += unit_fake_class_part.getFakeClassLevel(characterClass);
            }
        }
    }
}
