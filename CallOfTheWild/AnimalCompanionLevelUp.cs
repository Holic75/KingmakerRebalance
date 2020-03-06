using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
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
            if (!(bool)((UnityEngine.Object)component))
                return false;
            var tr = Harmony12.Traverse.Create(__instance);

            int pet_level = tr.Method("GetPetLevel").GetValue<int>();
            //Main.logger.Log("Pet level: " + __instance.SpawnedPet.Descriptor.Progression.CharacterLevel.ToString());
            //Main.logger.Log("Should be: " + pet_level.ToString());

            if (pet_level > __instance.SpawnedPet.Descriptor.Progression.CharacterLevel)
            {
                if (__instance.SpawnedPet.Descriptor.Progression.CharacterLevel == 0)
                {
                    component.LevelUp(__instance.SpawnedPet.Descriptor, 1);
                }
                var exp = Game.Instance.BlueprintRoot.Progression.XPTable.GetBonus(pet_level)
                                                                        - Game.Instance.BlueprintRoot.Progression.XPTable.GetBonus(__instance.SpawnedPet.Descriptor.Progression.CharacterLevel);
                Harmony12.Traverse.Create(__instance.SpawnedPet.Descriptor.Progression).Property("Experience").SetValue(__instance.SpawnedPet.Descriptor.Progression.Experience + exp);
                EventBus.RaiseEvent<IUnitGainExperienceHandler>((Action<IUnitGainExperienceHandler>)(h => h.HandleUnitGainExperience(__instance.SpawnedPet.Descriptor, exp)));
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
