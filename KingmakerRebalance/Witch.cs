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
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace KingmakerRebalance
{
    class Witch
    {
        static internal LibraryScriptableObject library => Main.library;
        static internal BlueprintCharacterClass witch_class;
        static internal BlueprintFeatureSelection hex_selection;
        //hexes
        static internal BlueprintFeature healing_hex;
        static internal BlueprintFeature beast_of_ill_omen;


        static void addWitchHexCooldownScaling(BlueprintAbility ability, BlueprintBuff hex_cooldown)
        {
            ability.ReplaceComponent<AbilityEffectRunAction>(ability.GetComponent<AbilityEffectRunAction>());
            var cooldown_action = new Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff();
            cooldown_action.Buff = hex_cooldown;
            cooldown_action.AsChild = true;
            var duration = Helpers.CreateContextValue(AbilityRankType.Default);
            duration.ValueType = ContextValueType.Simple;
            duration.Value = 1;
            cooldown_action.DurationValue = Helpers.CreateContextDuration(bonus: duration,
                                                                            rate: DurationRate.Days);
            ability.GetComponent<AbilityEffectRunAction>().addAction(cooldown_action);
            var target_checker = new Kingmaker.UnitLogic.Abilities.Components.TargetCheckers.AbilityTargetHasFact();
            target_checker.CheckedFacts = new BlueprintUnitFact[] { hex_cooldown };
            target_checker.Inverted = true;
            ability.AddComponent(target_checker);
            var scaling = new Kingmaker.UnitLogic.Mechanics.Components.ContextCalculateAbilityParamsBasedOnClass();
            scaling.CharacterClass = witch_class;
            scaling.StatType = StatType.Intelligence;
            scaling.UseKineticistMainStat = false;
            ability.AddComponent(scaling);
            var spell_list_components = ability.GetComponents<Kingmaker.Blueprints.Classes.Spells.SpellListComponent>();
            foreach (var c in spell_list_components)
            {
                ability.RemoveComponent(c);
            }
        }

        static BlueprintBuff addWitchHexCooldownScaling(BlueprintAbility ability, string buff_guid)
        {
            var hex_cooldown = Helpers.CreateBuff(ability.name + "CooldownBuff",
                                                                     ability.Name,
                                                                     ability.Description,
                                                                     buff_guid,
                                                                     ability.Icon,
                                                                     null);
            hex_cooldown.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.StayOnDeath);
            addWitchHexCooldownScaling(ability, hex_cooldown);
            return hex_cooldown;
        }

        static BlueprintCharacterClass[] getWitchArray()
        {
            return new BlueprintCharacterClass[] { witch_class };
        }


        static void createHealingHex()
        {
            var cure_light_wounds_hex_ability = library.CopyAndAdd<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f", "HealingHex1Ability", "a9f6f1aa9d46452aa5720c472b8926e2");
            cure_light_wounds_hex_ability.SetName("Healing Hex");
            cure_light_wounds_hex_ability.SetDescription("A witch can soothe the wounds of those she touches.\n"
                                                          + "Effect: This acts as a cure light wounds spell, using the witch’s caster level.Once a creature has benefited from the healing hex, it cannot benefit from it again for 24 hours.At 5th level, this hex acts like cure moderate wounds.");

            cure_light_wounds_hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, 
                                                                                                                                                       classes: new BlueprintCharacterClass[] { witch_class },
                                                                                                                                                       max: 5));
            var hex_cooldown = addWitchHexCooldownScaling(cure_light_wounds_hex_ability, "67e655e5a20640519d387e08298de728");


            var cure_mod_wounds_hex_ability = library.CopyAndAdd<BlueprintAbility>("1c1ebf5370939a9418da93176cc44cd9", "HealingHex2Ability", "8ea243ac42aa4959ba131cbd5ff0118b");
            cure_mod_wounds_hex_ability.SetName(cure_light_wounds_hex_ability.Name);
            cure_mod_wounds_hex_ability.SetDescription(cure_light_wounds_hex_ability.Description);


            cure_mod_wounds_hex_ability.ReplaceComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                                                                       classes: getWitchArray(),
                                                                                                                                                       max: 10));
            addWitchHexCooldownScaling(cure_mod_wounds_hex_ability, hex_cooldown);

            var healing_hex1_feature = Helpers.CreateFeature("HealingHex1Feature", cure_light_wounds_hex_ability.Name, cure_light_wounds_hex_ability.Description,
                                                             "a9d436988d044916b7bf61a58725725b",
                                                             cure_light_wounds_hex_ability.Icon,
                                                             FeatureGroup.None,
                                                             Helpers.CreateAddFact(cure_light_wounds_hex_ability));

            var healing_hex2_feature = Helpers.CreateFeature("HealingHex2Feature", cure_light_wounds_hex_ability.Name, cure_light_wounds_hex_ability.Description,
                                                 "6fe1054367f149939edc7f576d157bfa",
                                                 cure_light_wounds_hex_ability.Icon,
                                                 FeatureGroup.None,
                                                 Helpers.CreateAddFact(cure_mod_wounds_hex_ability));

            healing_hex = Helpers.CreateFeature("HealingHexFeature",
                                                cure_light_wounds_hex_ability.Name,
                                                cure_light_wounds_hex_ability.Description,
                                                "abec18ed55414a52a6d09457b734a5ca",
                                                cure_light_wounds_hex_ability.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(healing_hex1_feature, 5, getWitchArray(), new BlueprintArchetype[0], true),
                                                Helpers.CreateAddFeatureOnClassLevel(healing_hex2_feature, 5, getWitchArray(), new BlueprintArchetype[0], false)
                                                );
        }

        static void createBeastOfIllOmen()
        {
            var hex_ability = library.CopyAndAdd<BlueprintAbility>("8bc64d869456b004b9db255cdd1ea734", "BeastOfIllOmenAbility", "c19d55421e6f436580423fffc78c11bd");
            hex_ability.SetName("Beast of Ill-Omen");
            hex_ability.SetDescription("The witch imbues her familiar with strange magic, putting a minor curse upon the next enemy to see it.\n"
                                        + "Effect: The enemy must make a Will save or be affected by bane(caster level equal to the witch’s level).The witch can use this hex on her familiar at a range of up to 60 feet.The affected enemy must be no more than 60 feet from the familiar to trigger the effect; seeing the familiar from a greater distance has no effect(though if the enemy and familiar approach to within 60 feet of each other, the hex takes effect). The bane affects the closest creature to the familiar(ties affect the creature with the highest initiative score)\n"
                                        + " Whether or not the target’s save is successful, the creature cannot be the target of the bane effect for 1 day(later uses of this hex ignore that creature when determining who is affected).");
            hex_ability.Range = AbilityRange.Medium;
            hex_ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            hex_ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionPoint;
            hex_ability.RemoveComponent(hex_ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityTargetsAround>());
            
            var hex_cooldown = addWitchHexCooldownScaling(hex_ability, "ef6b3d4ad22644628aacfd3eaa4783e9");
            beast_of_ill_omen = Helpers.CreateFeature("BeastOfIllOmenFeature",
                                                      hex_ability.Name,
                                                      hex_ability.Description,
                                                      "fb3278a3b552414faaecb4189818b32e",
                                                      hex_ability.Icon,
                                                      FeatureGroup.None,
                                                      Helpers.CreateAddFact(hex_ability));
        }
    }
}
