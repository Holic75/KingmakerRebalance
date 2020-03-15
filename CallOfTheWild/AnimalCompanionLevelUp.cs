using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CallOfTheWild.AnimalCompanionLevelUp
{

    [Harmony12.HarmonyPatch(typeof(UnitDescriptor), "Dispose")]
    static class UnitDescriptor_Dispose_Patch
    {
        internal static bool Prefix(UnitDescriptor __instance, UnitPartsManager ___m_Parts, Dictionary<BlueprintSpellbook, Spellbook> ___m_Spellbooks)
        {
            __instance.Abilities.Dispose();
            __instance.ActivatableAbilities.Dispose();
            __instance.Logic.Dispose();
            __instance.Buffs.Dispose();
            __instance.Body.Dispose();
            __instance.Progression.Dispose();
            foreach (Spellbook spellbook in ___m_Spellbooks.Values)
                spellbook.Dispose();
            if (!__instance.IsPlayerFaction)
                __instance.Inventory.Dispose();
            /*if (__instance.Master.Value != null)
                __instance.Master.Value.Descriptor.m_Pet = (UnitReference)((UnitEntityData)null);*/ //not sure if it will not break anything ???
            ___m_Parts.Dispose();

            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitDescriptor), "SetMaster")]
    static class UnitDescriptor_SetMaster_Patch
    {
        internal static bool Prefix(UnitDescriptor __instance, UnitEntityData master, ref UnitReference ___m_Pet)
        {
            if (__instance.Master == null || master == null || __instance.Master == (UnitEntityData)new UnitReference())
            {
                return true;
            }
            Harmony12.Traverse.Create(master.Descriptor).Field("m_Pet").SetValue(null);

            return true;
        }
    }



    [Harmony12.HarmonyPatch(typeof(AddPet), "TryLevelUpPet")]
    static class AddPet_TryLevelUpPet_Patch
    {
        internal static bool Prefix(AddPet __instance)
        {
            if (!__instance.Owner.IsPlayerFaction)
            {
                return true;
            }
            if (__instance.SpawnedPet == null)
                return false;
            AddClassLevels component = __instance.SpawnedPet.Blueprint.GetComponent<AddClassLevels>();
            if (component == null)
                return false;

            int pet_level = 0;
            var eidolon = __instance.SpawnedPet.Blueprint.GetComponent<Eidolon.EidolonComponent>();
            if (eidolon == null)
            {
                var tr = Harmony12.Traverse.Create(__instance);
                pet_level = tr.Method("GetPetLevel").GetValue<int>();
            }
            else
            {
                pet_level = eidolon.getEidolonLevel(__instance);
            }
            //Main.logger.Log("Pet level: " + __instance.SpawnedPet.Descriptor.Progression.CharacterLevel.ToString());
            //Main.logger.Log("Should be: " + pet_level.ToString());

            if (pet_level > __instance.SpawnedPet.Descriptor.Progression.CharacterLevel)
            {
                if (__instance.SpawnedPet.Descriptor.Progression.CharacterLevel == 0)
                {
                    component.LevelUp(__instance.SpawnedPet.Descriptor, 1);
                }
                var exp = Game.Instance.BlueprintRoot.Progression.XPTable.GetBonus(pet_level);
                Harmony12.Traverse.Create(__instance.SpawnedPet.Descriptor.Progression).Property("Experience").SetValue(exp);
                EventBus.RaiseEvent<IUnitGainExperienceHandler>((Action<IUnitGainExperienceHandler>)(h => h.HandleUnitGainExperience(__instance.SpawnedPet.Descriptor, exp)));
                //Main.logger.Log("Pet level now: " + __instance.SpawnedPet.Descriptor.Progression.CharacterLevel.ToString());
            }
            
            if (eidolon != null)
            {//no upgrade for eidolon, since they are performed through summoner
                return false;
            }
            int? rank = __instance.Owner.GetFact((BlueprintUnitFact)__instance.LevelRank)?.GetRank();
            if (Mathf.Min(20, !rank.HasValue ? 0 : rank.Value) < __instance.UpgradeLevel)
                return false;
            __instance.SpawnedPet.Descriptor.Progression.Features.AddFeature(__instance.UpgradeFeature, (MechanicsContext)null);
            return false;
        }


        static internal void init()
        {
            var animal_calss = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");
            var slayer = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");
            var kineticist = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");
            slayer.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = animal_calss));
            kineticist.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = animal_calss));
            Helpers.RegisterClass(animal_calss);
            animal_calss.HideIfRestricted = false;
        }
    }
}
