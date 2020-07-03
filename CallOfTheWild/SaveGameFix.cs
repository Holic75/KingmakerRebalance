using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public class SaveGameFix
    {

        static public List<Action<UnitDescriptor>> save_game_actions = new List<Action<UnitDescriptor>>(); 

        static public BlueprintAbility createDummyAbility(string name, string guid)
        {
            return Helpers.CreateAbility(name, "", "", guid, null, AbilityType.Special, CommandType.Free, AbilityRange.Close, "", "");
        }


        static public BlueprintActivatableAbility createDummyActivatableAbility(string name, string guid)
        {
            return Helpers.CreateActivatableAbility(name, "", "", guid, null, null, AbilityActivationType.Immediately, CommandType.Free, null);
        }


        static public BlueprintFeature createDummyFeature(string name, string guid)
        {
            return Helpers.CreateFeature(name, "", "", guid, null, FeatureGroup.None);
        }


        static public BlueprintFeatureSelection createDummyFeatureSelection(string name, string guid)
        {
            return Helpers.CreateFeatureSelection(name, "", "", guid, null, FeatureGroup.None);
        }

        static internal void FixMissingAssets()
        {
            List<BlueprintUnitFact> missing_facts = new List<BlueprintUnitFact>();
            //broken channels that were removed in 1.29
            missing_facts.Add(createDummyAbility("ChannelSmiteQuickChannelEnergyEmpyrealHarm",   "89804559ac9dd22e23053d0b8c939eae"));
            missing_facts.Add(createDummyAbility("ChannelSmiteQuickChannelEnergyPaladinHarm",    "21e46c86f0a5714e918572662d76f4c1"));
            missing_facts.Add(createDummyAbility("ChannelSmiteQuickChannelEnergyHospitalerHarm", "a4c40f80cdddfe2e926c5e8560ac512c"));
            missing_facts.Add(createDummyAbility("ChannelSmiteQuickChannelPositiveHarm", "4f476c1d5375337ee1c6ce7a154d269b"));
            missing_facts.Add(createDummyAbility("ChannelSmiteQuickWitchPostiveHarm", "247078e91cb34f9ea7538e53811892e8"));
            missing_facts.Add(createDummyAbility("ChannelSmiteQuickChannelNegativeEnergy", "e10c33b872aa274e80ec96a132ec08ae"));
            missing_facts.Add(createDummyAbility("ChannelSmiteQuickWitchNegativeHarm", "d2479ab66de342adb092948d4bb85948"));

            missing_facts.Add(createDummyFeature("PositiveChanneling1Feature", "fc168f08cbeb4a5e90f1d5a4cfe82f42"));
            missing_facts.Add(createDummyFeature("PositiveChanneling2Feature", "56f12dafd5bf495fabf9b2f938a03d63"));
            
            missing_facts.Add(createDummyFeatureSelection("BloodlineUndeadSpellLevel1Selection", "cacde9103efa44979349a36e104c5b71"));

            Action<UnitDescriptor> fix_action = delegate (UnitDescriptor u)
            {
                foreach (var missing_fact in missing_facts)
                {
                    if (u.HasFact(missing_fact))
                    {
                        u.RemoveFact(missing_fact);
                    }
                }
            };

            //remove channels explicit abilities that should now all be under drop-down menu
            Action<UnitDescriptor> fix_channel = delegate (UnitDescriptor u)
            {
                var channels = ChannelEnergyEngine.getChannelAbilities(p => true);
                foreach (var channel in channels)
                {
                    if (u.HasFact(channel))
                    {
                        u.RemoveFact(channel);
                    }
                }
            };
            save_game_actions.Add(fix_action);
            save_game_actions.Add(fix_channel);
        }
    }



    [Harmony12.HarmonyPatch(typeof(UnitEntityData))]
    [Harmony12.HarmonyPatch("PostLoad", Harmony12.MethodType.Normal)]
    class UnitDescriptor__PostLoad__Patch
    {
        static LibraryScriptableObject library = Main.library;

        [Harmony12.HarmonyPostfix]
        static void Postfix(UnitEntityData __instance)
        {
            foreach (var action in SaveGameFix.save_game_actions)
            {
                action(__instance.Descriptor);
            }
        }
    }
}
