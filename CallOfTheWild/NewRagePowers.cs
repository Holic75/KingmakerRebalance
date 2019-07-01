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

        static BlueprintFeature come_and_get_me;



        static internal void load()
        {
            createComeAndGetMe();
        }


        static void addToSelection(BlueprintFeature rage_power)
        {
            extra_rage_power_selection.AllFeatures = extra_rage_power_selection.AllFeatures.AddToArray(rage_power);
            rage_powers_selection.AllFeatures = rage_powers_selection.AllFeatures.AddToArray(rage_power);
        }


        static void createComeAndGetMe()
        {
            var shout = library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162");

            var enemy_buff = Helpers.CreateBuff("ComeAndGetMeEnemyBuff",
                                                "",
                                                "",
                                                "",
                                                null,
                                                null,
                                                Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.AdditionalDamage, 4, Kingmaker.Enums.ModifierDescriptor.UntypedStackable),
                                                Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.AdditionalAttackBonus, 4, Kingmaker.Enums.ModifierDescriptor.UntypedStackable)
                                                );

            var buff = Helpers.CreateBuff("ComeAndGetMeEfectBuff",
                                          "Come and Get Me",
                                          "While raging, as a free action the barbarian may leave herself open to attack while preparing devastating counterattacks. Enemies gain a +4 bonus on attack and damage rolls against the barbarian, but every attack against the barbarian provokes an attack of opportunity from her, which is resolved prior to resolving each enemy attack.",
                                          "",
                                          shout.Icon,
                                          null,
                                          Common.createComeAndGetMe(enemy_buff)
                                          );

            come_and_get_me = Common.createSwitchActivatableAbilityBuff("ComeAndGetMeFeature", "", "", "",
                                                      buff, rage_buff,
                                                      reckless_stance.ActivateWithUnitAnimation);

            //come_and_get_me.AddComponent(Helpers.PrerequisiteClassLevel(barbarian_class, 8));

            addToSelection(come_and_get_me);
        }
    }
}
