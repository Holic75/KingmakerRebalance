using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class NewFeats
    {
        static  LibraryScriptableObject library => Main.library;
        static internal BlueprintFeature raging_brutality;
        static internal BlueprintFeature blooded_arcane_strike;
        static internal BlueprintFeature riving_strike;
        static internal BlueprintFeature coordinated_shot;
        
        static internal void load()
        {
            createRagingBrutality();
            createBloodedArcaneStrike();
            createRivingStrike();
            createCoordiantedShot();
            FeralCombatTraining.load();

        }


        static internal void createRagingBrutality()
        {
            var destructive_smite = library.Get<BlueprintActivatableAbility>("e69898f762453514780eb5e467694bdb");
            var power_attack_buff = library.Get<BlueprintBuff>("5898bcf75a0942449a5dc16adc97b279");
            var rage_resource = library.Get<BlueprintAbilityResource>("24353fcf8096ea54684a72bf58dedbc9");
            var power_attack_feature = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
            var rage_feature = library.Get<BlueprintFeature>("2479395977cfeeb46b482bc3385f4647");
            var buff = Helpers.CreateBuff("RagingBrutalityBuff",
                                          "Raging Brutality",
                                          "While raging and using Power Attack, you can spend 3 additional rounds of your rage as a swift action to add your Constitution bonus on damage rolls for melee attacks or thrown weapon attacks you make on your turn. If you are using the weapon two-handed, instead add 1-1/2 times your Constitution bonus.",
                                          "",
                                          destructive_smite.Icon,
                                          null,
                                          Common.createContextWeaponDamageBonus(Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Constitution, type: AbilityRankType.DamageBonus));

            var ability = Helpers.CreateAbility("RagingBrutalityAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneRoundDuration,
                                                Helpers.savingThrowNone,
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff,
                                                                                                             Helpers.CreateContextDuration(Common.createSimpleContextValue(1), rate: DurationRate.Rounds),
                                                                                                             dispellable: false
                                                                                                             )
                                                                        ),
                                                Common.createAbilityCasterHasFacts(NewRagePowers.rage_marker_caster),
                                                Common.createAbilityCasterHasFacts(power_attack_buff),
                                                Helpers.CreateResourceLogic(rage_resource, amount: 3)
                                                );

            raging_brutality = Helpers.CreateFeature("RagingBrutalityFeature",
                                                      buff.Name,
                                                      buff.Description,
                                                      "",
                                                      buff.Icon,
                                                      FeatureGroup.Feat,
                                                      Helpers.CreateAddFact(ability),
                                                      Helpers.PrerequisiteStatValue(StatType.Strength, 13),
                                                      Helpers.PrerequisiteFeature(power_attack_feature),
                                                      Helpers.PrerequisiteFeature(rage_feature),
                                                      Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 12)
                                                      );
            library.AddFeats(raging_brutality);
        }


        static void createBloodedArcaneStrike()
        {
            var arcane_strike_buff = library.Get<BlueprintBuff>("98ac795afd1b2014eb9fdf2b9820808f");
            var arcane_strike_feature = library.Get<BlueprintFeature>("0ab2f21a922feee4dab116238e3150b4");
            var arcane_strike_bonus = arcane_strike_buff.GetComponent<AddContextStatBonus>();
            var vital_strike_buff = Helpers.CreateBuff("BloodedArcaneStrikeVitalStrikeBuff",
                                                       "",
                                                       "",
                                                       "",
                                                       null,
                                                       null,
                                                       Common.createVitalStrikeScalingDamage(arcane_strike_bonus.Value, arcane_strike_bonus.Multiplier),
                                                       arcane_strike_buff.GetComponent<ContextRankConfig>()
                                                       );
            vital_strike_buff.SetBuffFlags(arcane_strike_buff.GetBuffFlags() | BuffFlags.HiddenInUi);

            blooded_arcane_strike = Helpers.CreateFeature("BloodedArcaneStrikeFeature",
                                                          "Blooded Arcane Strike",
                                                          "While you are bloodraging, you don’t need to spend a swift action to use your Arcane Strike—it is always in effect. When you use this ability with Vital Strike, Improved Vital Strike, or Greater Vital Strike, the bonus on damage rolls for Arcane Strike is multiplied by the number of times (two, three, or four) you roll damage dice for one of those feats.",
                                                          "",
                                                          arcane_strike_feature.Icon,
                                                          FeatureGroup.CombatFeat,
                                                          Helpers.PrerequisiteFeature(arcane_strike_feature),
                                                          Helpers.PrerequisiteClassLevel(Bloodrager.bloodrager_class, 1)
                                                          );

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(arcane_strike_buff, vital_strike_buff, blooded_arcane_strike);
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(Bloodrager.bloodrage_buff, arcane_strike_buff, blooded_arcane_strike);

            //library.AddFeats(blooded_arcane_strike);
            library.AddCombatFeats(blooded_arcane_strike);
            blooded_arcane_strike.Groups = blooded_arcane_strike.Groups.AddToArray(FeatureGroup.Feat);
        }


        static void createRivingStrike()
        {
            var arcane_strike_buff = library.Get<BlueprintBuff>("98ac795afd1b2014eb9fdf2b9820808f");
            var arcane_strike_feature = library.Get<BlueprintFeature>("0ab2f21a922feee4dab116238e3150b4");

            var debuff = Helpers.CreateBuff("RivingStrikeEnemyBuff",
                                            "Riving Strike Penalty",
                                            "Target receives -2 penalty to saving throws against spells and spell-like abilities",
                                            "",
                                            arcane_strike_feature.Icon,
                                            null,
                                            Common.createSavingThrowBonusAgainstAbilityType(-2, Common.createSimpleContextValue(0), AbilityType.Spell),
                                            Common.createSavingThrowBonusAgainstAbilityType(-2, Common.createSimpleContextValue(0), AbilityType.SpellLike)
                                            );
            debuff.Stacking = StackingType.Stack;

            var debuff_action = Common.createContextActionApplyBuff(debuff,
                                                             Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds),
                                                             dispellable: false
                                                             );
            var buff = Helpers.CreateBuff("RivingStrikeBuff",
                                          "Riving Strike",
                                          "If you have a weapon that is augmented by your Arcane Strike feat, when you damage a creature with an attack made with that weapon, that creature takes a –2 penalty on saving throws against spells and spell-like abilities. This effect lasts for 1 round.",
                                          "",
                                          arcane_strike_feature.Icon,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(debuff_action))
                                          );

            riving_strike = Helpers.CreateFeature("RivingStrikeFeature",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  FeatureGroup.CombatFeat,
                                                  Helpers.PrerequisiteFeature(arcane_strike_feature));
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(arcane_strike_buff, buff, riving_strike);
            library.AddCombatFeats(riving_strike);
            riving_strike.Groups = riving_strike.Groups.AddToArray(FeatureGroup.Feat);
        }


        static void createCoordiantedShot()
        {
            var point_blank_shot = library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab");
            coordinated_shot = Helpers.CreateFeature("CoordinatedShotFeature",
                                                     "Coordinated Shot",
                                                     "If your ally with this feat is threatening an opponent and is not providing cover to that opponent against your ranged attacks, you gain a +1 bonus on ranged attacks against that opponent. If your ally with this feat is flanking that opponent with another ally (even if that other ally doesn’t have this feat), this bonus increases to +2.",
                                                     "",
                                                     point_blank_shot.Icon,
                                                     FeatureGroup.Feat,
                                                     Helpers.PrerequisiteFeature(point_blank_shot));

            coordinated_shot.AddComponent(Helpers.Create<NewMechanics.CoordinatedShotAttackBonus>(c =>
                                                                                                 {
                                                                                                     c.AttackBonus = 1;
                                                                                                     c.AdditionalFlankBonus = 1;
                                                                                                     c.CoordinatedShotFact = coordinated_shot;
                                                                                                 })
                                         );
            coordinated_shot.Groups = coordinated_shot.Groups.AddToArray(FeatureGroup.CombatFeat, FeatureGroup.TeamworkFeat);
            library.AddCombatFeats(coordinated_shot);
            Common.addTemworkFeats(coordinated_shot);
        }

    }
}
