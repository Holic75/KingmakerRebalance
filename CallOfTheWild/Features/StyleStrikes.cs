using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class StyleStrikes
    {
        static LibraryScriptableObject library = Main.library;
        static public BlueprintFeature flying_kick;

        static public void load()
        {
            createFlyingKick();
        }


        static void addToStyleStrikes(BlueprintFeature feature)
        {
            var style_strikes = library.Get<BlueprintFeatureSelection>("7bc6a93f6e48eff49be5b0cde83c9450");
            style_strikes.AllFeatures = style_strikes.AllFeatures.AddToArray(feature);
        }

        static void createFlyingKick()
        {
            var buff = Helpers.CreateBuff("MonkFlyingKickBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Helpers.CreateAddMechanics(Kingmaker.UnitLogic.FactLogic.AddMechanicsFeature.MechanicsFeatureType.Pounce)
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
                
            var toggle = Common.buffToToggle(buff, Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free, false);
            toggle.Group = Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.StyleStrike;
            toggle.SetNameDescriptionIcon("Flying Kick",
                                          "The monk leaps through the air to strike a foe with a kick. The monk can make a full-attack at the end of charge action.",
                                          NewSpells.fly.Icon);

            flying_kick = Common.ActivatableAbilityToFeature(toggle, false);
            addToStyleStrikes(flying_kick);
        }
    }
}
