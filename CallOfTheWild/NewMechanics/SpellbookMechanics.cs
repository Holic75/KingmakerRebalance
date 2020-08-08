using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SpellbookMechanics
{
    public class UnitPartDoNotSpendNextSpell : AdditiveUnitPart
    {
        public bool active;

    }

    [AllowedOn(typeof(BlueprintSpellbook))]
    public class NoSpellsPerDaySacaling : BlueprintComponent
    {

    }


    [AllowedOn(typeof(BlueprintSpellbook))]
    public class CompanionSpellbook : BlueprintComponent
    {
        public BlueprintSpellbook spellbook;
    }


    [AllowedOn(typeof(BlueprintSpellbook))]
    public class CanNotUseSpells : BlueprintComponent
    {

    }

    [AllowedOn(typeof(BlueprintSpellbook))]
    public class GetKnownSpellsFromMemorizationSpellbook : BlueprintComponent
    {
        public BlueprintSpellbook spellbook;
    }


    //make fake spell book return zero memorized spells
    [Harmony12.HarmonyPatch(typeof(Spellbook))]
    [Harmony12.HarmonyPatch("Rest", Harmony12.MethodType.Normal)]
    class Spellbook__Rest__Patch
    {
        static void Postfix(Spellbook __instance)
        {
            if (!__instance.Blueprint.Spontaneous)
            {
                return;
            }
            var memorization_spellbook_blueprint = __instance.Blueprint.GetComponent<GetKnownSpellsFromMemorizationSpellbook>()?.spellbook;
            if (memorization_spellbook_blueprint == null || memorization_spellbook_blueprint.Spontaneous)
            {
                return;
            }

            var memorization_spellbook = __instance.Owner?.Spellbooks.Where(s => s.Blueprint == memorization_spellbook_blueprint).FirstOrDefault();
            if (memorization_spellbook == null)
            {
                return;
            }

            var unit_part_arcanist = __instance.Owner.Get<SpellManipulationMechanics.UnitPartArcanistPreparedMetamagic>();
            if (unit_part_arcanist == null)
            {
                return;
            }

            for (int i = 1; i <= __instance.MaxSpellLevel; i++)
            {
                unit_part_arcanist.refreshSpellLevel(i);
            }
            /*foreach (var s in __instance.GetAllKnownSpells().ToArray())
            {
                __instance.RemoveSpell(s.Blueprint);
            }

            __instance.Owner.Ensure<SpellManipulationMechanics.UnitPartArcanistPreparedMetamagic>().clear();
            var m_known_spells = Helpers.GetField<List<AbilityData>[]>(__instance, "m_KnownSpells");
            var m_known_SpellLevels = Helpers.GetField<Dictionary<BlueprintAbility, int>>(__instance, "m_KnownSpellLevels");
            foreach (var slot in memorization_spellbook.GetAllMemorizedSpells())
            {
                if (slot.Spell != null)
                {
                    var new_ability = new AbilityData(slot.Spell.Blueprint, __instance);
                    if (slot.Spell.MetamagicData != null)
                    {
                        new_ability.MetamagicData = slot.Spell.MetamagicData.Clone();
                        __instance.Owner.Ensure<SpellManipulationMechanics.UnitPartArcanistPreparedMetamagic>().add(new_ability.Blueprint, new_ability.MetamagicData.MetamagicMask);
                    }
                    else
                    {
                        __instance.Owner.Ensure<SpellManipulationMechanics.UnitPartArcanistPreparedMetamagic>().add(new_ability.Blueprint, (Metamagic)0);
                    }
                    var spell_level = memorization_spellbook.GetSpellLevel(slot.Spell.Blueprint);
                    new_ability.DecorationBorderNumber = slot.Spell.DecorationBorderNumber;
                    new_ability.DecorationColorNumber = slot.Spell.DecorationColorNumber;
                    
                    m_known_spells[slot.Spell.SpellLevel].Add(new_ability);
                    m_known_SpellLevels[slot.Spell.Blueprint] = spell_level;
                    EventBus.RaiseEvent<ISpellBookCustomSpell>((Action<ISpellBookCustomSpell>)(h => h.AddSpellHandler(new_ability)));
                }
            }*/
        }
    }





    //make fake spell book return zero memorized spells
    [Harmony12.HarmonyPatch(typeof(Spellbook))]
    [Harmony12.HarmonyPatch("GetMemorizedSpells", Harmony12.MethodType.Normal)]
    class Spellbook__GetMemorizedSpells__Patch
    {
        static void Postfix(Spellbook __instance, int spellLevel, ref IEnumerable<SpellSlot> __result)
        {
            if (__instance.Blueprint.GetComponent<CanNotUseSpells>() != null)
            {
                __result = new SpellSlot[0];
            }
        }
    }

    [Harmony12.HarmonyPatch(typeof(Spellbook))]
    [Harmony12.HarmonyPatch("Spend", Harmony12.MethodType.Normal)]
    class Spellbook__Spends__Patch
    {
        static bool Prefix(Spellbook __instance, AbilityData spell, bool excludeSpecial, ref bool __result)
        {
            if (__instance.Owner.Ensure<UnitPartDoNotSpendNextSpell>().active)
            {
                __instance.Owner.Ensure<UnitPartDoNotSpendNextSpell>().active = false;
                __result = true;
                return false;
            }
            return true;
        }
    }


    //add automatic update of companion spellbooks
    [Harmony12.HarmonyPatch(typeof(Spellbook))]
    [Harmony12.HarmonyPatch("AddCasterLevel", Harmony12.MethodType.Normal)]
    class Spellbook__AddCasterLevel__Patch
    {
        static void Postfix(Spellbook __instance)
        {
            foreach (var cs in __instance.Blueprint.GetComponents<CompanionSpellbook>())
            {
                __instance.Owner.DemandSpellbook(cs.spellbook).AddCasterLevel();
            }
        }
    }


    //fix loading spell on arcanist
    [Harmony12.HarmonyPatch(typeof(Spellbook))]
    [Harmony12.HarmonyPatch("PostLoad", Harmony12.MethodType.Normal)]
    class Spellbook__PostLoad__Patch
    {
        static void Postfix(Spellbook __instance, List<AbilityData>[] ___m_KnownSpells, Dictionary<BlueprintAbility, int> ___m_KnownSpellLevels)
        {
            for (int index = 0; index < ___m_KnownSpells.Length; ++index)
            {
                foreach (AbilityData abilityData in ___m_KnownSpells[index])
                {
                    if (abilityData.MetamagicData != null)
                    {
                        ___m_KnownSpellLevels[abilityData.Blueprint] = index - abilityData.MetamagicData.SpellLevelCost;
                    }
                }
            }
        }
    }

    //fix for spellbooks with fixed number of spells per day iindependent of casting stat
    [Harmony12.HarmonyPatch(typeof(Spellbook))]
    [Harmony12.HarmonyPatch("GetSpellsPerDay", Harmony12.MethodType.Normal)]
    class Spellbook__GetSpellsPerDay__Patch
    {
        static bool Prefix(Spellbook __instance, int spellLevel, ref int __result)
        {
            if (__instance.Blueprint.GetComponent<NoSpellsPerDaySacaling>() == null)
            {
                return true;
            }

            ModifiableValueAttributeStat stat = __instance.Owner.Stats.GetStat(__instance.Blueprint.CastingAttribute) as ModifiableValueAttributeStat;
            if (stat == null || stat.ModifiedValue < 10 + spellLevel)
            {
                __result = 0;
                return false;
            }

            int? count = __instance.Blueprint.SpellsPerDay.GetCount(__instance.CasterLevel, spellLevel);
            if (!count.HasValue)
            {
                __result = 0;
            }
            else
            {
                __result = count.Value;
            }
            return false;
        }
    }


    //disallow removing metamgic from arcanist prepared metamagic spells
    [Harmony12.HarmonyPatch(typeof(SpellBookMetamagicMixer))]
    [Harmony12.HarmonyPatch("HandleRemoveFromMixer", Harmony12.MethodType.Normal)]
    class SpellBookMetamagicMixer__HandleRemoveFromMixer__Patch
    {
        static bool Prefix(SpellBookMetamagicMixer __instance, Kingmaker.UnitLogic.Feature feature, MetamagicBuilder ___m_MetamagicBuilder)
        {
            var unit = ___m_MetamagicBuilder?.Spellbook?.Owner;
            if (unit == null)
            {
                return true;
            }

            var arcanist_part = unit.Get<SpellManipulationMechanics.UnitPartArcanistPreparedMetamagic>();

            if (arcanist_part == null)
            {
                return true;
            }

            if (arcanist_part.spellbook != ___m_MetamagicBuilder.Spellbook.Blueprint)
            {
                return true;
            }

            var current_metamagic_data = __instance.CurrentTemporarySpell?.MetamagicData;
            if (current_metamagic_data == null)
            {
                return true;
            }

            var metamagic = feature.Get<AddMetamagicFeat>().Metamagic;
            var metamagic_after_removal = current_metamagic_data.MetamagicMask & ~metamagic;

            if (!arcanist_part.authorisedMetamagic(__instance.CurrentTemporarySpell.Blueprint, metamagic_after_removal))
            {
                return false;
            }

            return true;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class CasterLevelBonusBounded : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public ContextValue value;
        public ContextValue bound;
        public BlueprintCharacterClass character_class;

        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            evt.AddBonusCasterLevel(getBonus(evt.Spellbook));
        }



        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }


        private int getBonus(Spellbook spellbook)
        {
            if (spellbook == null)
            {
                return 0;
            }
            var class_spellbook = this.Owner.GetSpellbook(character_class);
            if (class_spellbook == null)
            {
                return 0;
            }

            if (class_spellbook.Blueprint != spellbook.Blueprint && class_spellbook.Blueprint.GetComponent<CompanionSpellbook>()?.spellbook != spellbook.Blueprint)
            {
                return 0;
            }

            int bonus = value.Calculate(this.Fact.MaybeContext);
            bonus = Math.Min(bonus, bound.Calculate(this.Fact.MaybeContext) - spellbook.CasterLevel);

            return Math.Max(0, bonus);
        }
    }

    public class PsychicSpellbook: BlueprintComponent
    {
    
    }


    [AllowMultipleComponents]
    public class PrerequisitePsychicCasterTypeSpellLevel : Prerequisite
    {
        public bool OnlySpontaneous;
        public int RequiredSpellLevel;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            foreach (ClassData classData in unit.Progression.Classes)
            {
                BlueprintSpellbook spellbook = classData.Spellbook;
                if (spellbook != null
                    && (!this.OnlySpontaneous || spellbook.Spontaneous)
                    && spellbook.GetComponent<PsychicSpellbook>() != null
                    && unit.DemandSpellbook(classData.CharacterClass).MaxSpellLevel >= this.RequiredSpellLevel)
                    return true;
            }
            return false;
        }


        public override string GetUIText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.OnlySpontaneous)
                stringBuilder.Append("Ability to cast psychic spontaneous spells");
            else
                stringBuilder.Append("Ability to cast psychic spells");
            stringBuilder.Append(" ");
            stringBuilder.Append((string)UIStrings.Instance.Tooltips.SpellsOfLevel[this.RequiredSpellLevel]);
            return stringBuilder.ToString();
        }
    }



    [AllowMultipleComponents]
    public class PrerequisiteDivineCasterTypeSpellLevel : Prerequisite
    {
        public bool OnlySpontaneous;
        public int RequiredSpellLevel;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            foreach (ClassData classData in unit.Progression.Classes)
            {
                BlueprintSpellbook spellbook = classData.Spellbook;
                if (spellbook != null
                    && !spellbook.IsAlchemist
                    && !spellbook.IsArcane
                    && (!this.OnlySpontaneous || spellbook.Spontaneous)
                    && spellbook.GetComponent<PsychicSpellbook>() == null
                    && unit.DemandSpellbook(classData.CharacterClass).MaxSpellLevel >= this.RequiredSpellLevel)
                    return true;
            }
            return false;
        }


        public override string GetUIText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.OnlySpontaneous)
                stringBuilder.Append((string)(UIStrings.Instance.Tooltips.CanCastDivineSpontaneousSpellsOfLevel));
            else
                stringBuilder.Append((string)(UIStrings.Instance.Tooltips.CanCastDivineSpellsOfLevel));
            stringBuilder.Append(" ");
            stringBuilder.Append((string)UIStrings.Instance.Tooltips.SpellsOfLevel[this.RequiredSpellLevel]);
            return stringBuilder.ToString();
        }
    }


    public static class Helpers
    {
        //used to extract arcanist spontaneous spellbook from his memorization spellbook, for any other kind of spellbooks return spellbook argument
        public static Spellbook getCastingSpellbook(Spellbook spellbook, UnitDescriptor unit)
        {
            var sb1 = spellbook?.Blueprint.GetComponent<CompanionSpellbook>()?.spellbook?.GetComponent<GetKnownSpellsFromMemorizationSpellbook>()?.spellbook;

            if (sb1 == null)
            {
                return spellbook;
            }
            else
            {
                return unit.GetSpellbook(sb1);
            }
        }
    }

}
