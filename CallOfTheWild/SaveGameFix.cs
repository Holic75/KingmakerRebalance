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

        static BlueprintAbility createDummyAbility(string name, string guid)
        {
            return Helpers.CreateAbility(name, "", "", guid, null, AbilityType.Special, CommandType.Free, AbilityRange.Close, "", "");
        }

        static internal void FixMissingAssets()
        {
            List<BlueprintAbility> missing_abilities = new List<BlueprintAbility>();
            //broken channels that were removed in 1.29
            missing_abilities.Add(createDummyAbility("ChannelSmiteQuickChannelEnergyEmpyrealHarm",   "89804559ac9dd22e23053d0b8c939eae"));
            missing_abilities.Add(createDummyAbility("ChannelSmiteQuickChannelEnergyPaladinHarm",    "21e46c86f0a5714e918572662d76f4c1"));
            missing_abilities.Add(createDummyAbility("ChannelSmiteQuickChannelEnergyHospitalerHarm", "a4c40f80cdddfe2e926c5e8560ac512c"));
            missing_abilities.Add(createDummyAbility("ChannelSmiteQuickChannelPositiveHarm", "4f476c1d5375337ee1c6ce7a154d269b"));
            missing_abilities.Add(createDummyAbility("ChannelSmiteQuickWitchPostiveHarm", "247078e91cb34f9ea7538e53811892e8"));
            missing_abilities.Add(createDummyAbility("ChannelSmiteQuickChannelNegativeEnergy", "e10c33b872aa274e80ec96a132ec08ae"));
            missing_abilities.Add(createDummyAbility("ChannelSmiteQuickWitchNegativeHarm", "d2479ab66de342adb092948d4bb85948"));


            Action<UnitDescriptor> fix_action = delegate (UnitDescriptor u)
            {
                foreach (var missing_ability in missing_abilities)
                {
                    if (u.HasFact(missing_ability))
                    {
                        u.RemoveFact(missing_ability);
                    }
                }
            };

            save_game_actions.Add(fix_action);
        }
    }



    [Harmony12.HarmonyPatch(typeof(UnitDescriptor))]
    [Harmony12.HarmonyPatch("PostLoad", Harmony12.MethodType.Normal)]
    class UnitDescriptor__PostLoad__Patch
    {
        static LibraryScriptableObject library = Main.library;

        [Harmony12.HarmonyPostfix]
        static void Postfix(UnitDescriptor __instance)
        {
            foreach (var action in SaveGameFix.save_game_actions)
            {
                action(__instance);
            }
        }
    }
}
