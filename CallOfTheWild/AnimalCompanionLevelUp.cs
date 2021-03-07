using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
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
            Main.TraceLog();
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
            Main.TraceLog();
            if (__instance.Master == null || master == null || __instance.Master == (UnitEntityData)new UnitReference())
            {
                return true;
            }
            Harmony12.Traverse.Create(master.Descriptor).Field("m_Pet").SetValue(null);

            return true;
        }
    }


    [Harmony12.HarmonyPatch(typeof(AddPet), "GetPetLevel")]
    static class AddPet_GetPetLevel_Patch
    {
        internal static bool Prefix(AddPet __instance, ref int __result)
        {
            Main.TraceLog();
            var custom_level = __instance.Fact.Blueprint.GetComponent<CompanionMechanics.CustomLevelProgression>();
            if (custom_level == null)
            {
                return true;
            }

            __result = custom_level.getLevel(__instance);
            return false;
        }

    }



    [Harmony12.HarmonyPatch(typeof(AddPet), "TryLevelUpPet")]
    static class AddPet_TryLevelUpPet_Patch
    {
        static BlueprintCharacterClass[] manual_classes = new BlueprintCharacterClass[] { ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920"),
                                                                                         Eidolon.eidolon_class,
                                                                                         Phantom.phantom_class,
                                                                                         ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("01a754e7c1b7c5946ba895a5ff0faffc"),
                                                                                        };
        internal static bool Prefix(AddPet __instance)
        {
            Main.TraceLog();
            if (!__instance.Owner.IsPlayerFaction)
            {
                return true;
            }
            if (__instance.SpawnedPet == null)
                return false;

            AddClassLevels component = __instance.SpawnedPet.Blueprint.GetComponent<AddClassLevels>();

            
            if (component == null)
                return false;

            if (!manual_classes.Contains(component.CharacterClass))
            {//non animal and non eidolon companions will have automatic level up
                return true;
            }

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
            var dragon_calss = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("01a754e7c1b7c5946ba895a5ff0faffc");
            
            var slayer = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb");
            var kineticist = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");
            slayer.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = animal_calss));
            kineticist.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = animal_calss));
            Helpers.RegisterClass(animal_calss);
            animal_calss.HideIfRestricted = false;
            Helpers.RegisterClass(dragon_calss);
            dragon_calss.HideIfRestricted = false;


            foreach (var c in BlueprintRoot.Instance.Progression.CharacterClasses)
            {
                if (c != Eidolon.eidolon_class)
                {
                    c.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = Eidolon.eidolon_class));
                }
                if (c != Phantom.phantom_class)
                {
                    c.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = Phantom.phantom_class));
                }
                if (c != dragon_calss)
                {
                    c.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = dragon_calss));
                }
            }


            //fix feats
            var eidolon_restricted_feats = new BlueprintFeature[]
            {
                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132"), //light armor proficiency
                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("46f4fb320f35704488ba3d513397789d"), //medium armor proficiency
                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("1b0f68188dcc435429fb87a022239681"), //heavy armor proficiency
                NewFeats.animal_ally,
                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("8fc01f06eab4dd946baa5bc658cac556"), //boon companion
            };

            var animal_restricted_feats = eidolon_restricted_feats.AddToArray(new BlueprintFeature[]
                                                                            {
                                                                                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("e70ecf1ed95ca2f40b754f1adb22bbdd"), //simple weapon proficiency
                                                                                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629"), //medium armor proficiency
                                                                                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("9a01b6815d6c3684cb25f30b8bf20932"), //heavy armor proficiency
                                                                                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167"), //improved unarmed strike
                                                                                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("cb8686e7357a68c42bdd9d4e65334633"), //shields proficiency
                                                                                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("6105f450bb2acbd458d277e71e19d835"), //tower shield proficiency
                                                                                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ac8aaf29054f5b74eb18f2af950e752d"), //two weapon fighting
                                                                                NewFeats.animal_ally,
                                                                                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("8fc01f06eab4dd946baa5bc658cac556"), //boon companion
                                                                            });

            foreach (var f in eidolon_restricted_feats)
            {
                f.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = Eidolon.eidolon_class));
            }

            foreach (var f in animal_restricted_feats)
            {
                f.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = dragon_calss));
                f.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = Phantom.phantom_class));
                f.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = animal_calss));
                f.AddComponent(Common.prerequisiteNoArchetype(Eidolon.eidolon_class, Eidolon.quadruped_archetype));
                f.AddComponent(Common.prerequisiteNoArchetype(Eidolon.eidolon_class, Eidolon.serpentine_archetype));
            }


            var improved_unarmed_strike = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");
            var weapon_override = improved_unarmed_strike.GetComponent<EmptyHandWeaponOverride>();
            improved_unarmed_strike.ReplaceComponent(weapon_override, Helpers.Create<NewMechanics.EmptyHandWeaponOverrideIfNoWeapon>(e => e.Weapon = weapon_override.Weapon));
        }

    }
}
