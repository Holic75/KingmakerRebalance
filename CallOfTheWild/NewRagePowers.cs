using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    class NewRagePowers
    {
        static LibraryScriptableObject library => Main.library;
        static BlueprintFeatureSelection rage_powers_selection => Main.library.Get<BlueprintFeatureSelection>("28710502f46848d48b3f0d6132817c4e");
        static BlueprintFeatureSelection extra_rage_power_selection => Main.library.Get<BlueprintFeatureSelection>("0c7f01fbbe687bb4baff8195cb02fe6a");
        static BlueprintBuff rage_buff => library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
        static BlueprintActivatableAbility reckless_stance => library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");
        static BlueprintCharacterClass barbarian_class => ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

        static BlueprintFeature taunting_stance;



        static internal void load()
        {
            createTauntingStance();
        }


        static void addToSelection(BlueprintFeature rage_power)
        {
            extra_rage_power_selection.AllFeatures = extra_rage_power_selection.AllFeatures.AddToArray(rage_power);
            rage_powers_selection.AllFeatures = rage_powers_selection.AllFeatures.AddToArray(rage_power);
        }


        static void createTauntingStance()
        {
            var shout = library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162");



            var buff = Helpers.CreateBuff("TauntingStanceEffectBuff",
                                          "Taunting Stance",
                                          "The barbarian can leave herself open to attacks while preparing devastating counterattacks. Enemies gain a +4 bonus on attack and damage rolls against the barbarian while she’s in this stance, but every attack against the barbarian provokes an attack of opportunity from her. This is a stance rage power.",
                                          "",
                                          shout.Icon,
                                          null,
                                          Common.createComeAndGetMe()
                                          );

            taunting_stance = Common.createSwitchActivatableAbilityBuff("TauntingStance", "", "", "",
                                                      buff, rage_buff,
                                                      reckless_stance.ActivateWithUnitAnimation,
                                                      ActivatableAbilityGroup.BarbarianStance,
                                                      command_type: CommandType.Standard);

            //taunting_stance.AddComponent(Helpers.PrerequisiteClassLevel(barbarian_class, 12));

            addToSelection(taunting_stance);
        }
    }
}
