using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class Familiars
    {
        static LibraryScriptableObject library => Main.library;
        public static void load()
        {
            var familiars = library.Get<BlueprintFeatureSelection>("363cab72f77c47745bf3a8807074d183").AllFeatures;

            var buff = Helpers.CreateBuff("FamiliarFreeItemUseBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.Create<TurnActionMechanics.FamiliarFreeItemUse>());
            buff.SetBuffFlags(BuffFlags.HiddenInUi);

            foreach (var f in familiars)
            {
                f.AddComponent(Common.createAuraFeatureComponent(buff));
                f.SetDescription(f.Description + "\nOnce per round instead of using an activatable item, like wand, scroll or potion, you can make your familiar activate this item, instead of spending an action yourself."
                    + "If activating an item requires to make a skill check or an attack roll, you make the corresponding check in place of your familiar.\nNote: Your familiar does not have a spellbook, so a UMD check is always required if it uses a scroll or a wand.");
                var toggle = f.GetComponent<AddFacts>().Facts[0] as BlueprintActivatableAbility;
                toggle.AddComponent(Helpers.Create<TurnActionMechanics.RestrictionCanUseFamiliarFreeCast>());
                toggle.Buff.AddComponent(Helpers.Create<TurnActionMechanics.ActivateFamiliarFreeCast>());
                toggle.Buff.SetDescription(f.Description);
                toggle.Group = ActivatableAbilityGroupExtension.Familiar.ToActivatableAbilityGroup();
            }
        }
    }
}
