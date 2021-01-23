using CallOfTheWild.NewMechanics;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public partial class HexEngine
    {
        //general hexes:


        public BlueprintFeature CreateWingsAttackHex(string name_prefix, string display_name, string description)
        {
            var wing_weapon = library.Get<BlueprintItemWeapon>("864e29d3e07ad4a4f96d576b366b4a86");//wing 1d4

            var ability = library.CopyAndAdd<BlueprintActivatableAbility>("7679910a16368cc43b496cef2babe1cb", name_prefix + "HexActivatableAbility", ""); //silver dragon wings

            var buff = Helpers.CreateBuff(name_prefix + "HexBuff",
                                          display_name,
                                          description,
                                          "",
                                          ability.Icon,
                                          ability.Buff.FxOnStart,
                                          Common.createAddSecondaryAttacks(wing_weapon, wing_weapon)
                                          );

            ability.Buff = buff;
            ability.SetNameDescription(display_name, description);

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            return feature;
        }


        public BlueprintFeature CreateWingsHex(string name_prefix, string display_name, string description)
        {
            var ability = library.Get<BlueprintActivatableAbility>("7679910a16368cc43b496cef2babe1cb"); //silver dragon wings
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  display_name,
                                                  description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            return feature;
        }


        public BlueprintFeatureSelection createSecret(string name_prefix, string display_name, string description)
        {
            var metamagic_feats = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(b => b.Groups.Contains(FeatureGroup.WizardFeat) && (b.GetComponent<AddMetamagicFeat>() != null));

            var feature = Helpers.CreateFeatureSelection(name_prefix + "HexFeature",
                                      display_name,
                                      description,
                                      "",
                                      null,
                                      FeatureGroup.None);
            feature.IgnorePrerequisites = true;
            feature.Ranks = 1;
            feature.AddComponent(Helpers.PrerequisiteNoFeature(feature));
            feature.AllFeatures = metamagic_feats.ToArray();

            return feature;
        }


        public BlueprintFeature createIntimidatingDisplay(string name_prefix, string display_name, string description)
        {
            var dazzling_display_feature = library.Get<BlueprintFeature>("bcbd674ec70ff6f4894bb5f07b6f4095");
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                          display_name,
                          description,
                          "",
                          dazzling_display_feature.Icon,
                          FeatureGroup.None,
                          Helpers.CreateAddFact(dazzling_display_feature),
                          Helpers.PrerequisiteNoFeature(dazzling_display_feature));
            feature.Ranks = 1;

            
            var dazzling_display = library.Get<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb");
            dazzling_display.GetComponent<NewMechanics.AbilityCasterEquippedWeaponCheckHasParametrizedFeature>().alternative = feature;

            return feature;
        }


        public BlueprintFeature createShapeshiftHex(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "HexResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, hex_classes);
            BlueprintAbility[] shapes = new BlueprintAbility[] {Wildshape.wolf_form_spell, Wildshape.leopard_form_spell, Wildshape.bear_form_spell, Wildshape.dire_wolf_form_spell,
                                                               Wildshape.smilodon_form_spell, Wildshape.mastodon_form_spell, Wildshape.hodag_form_spell, Wildshape.winter_wolf_form_spell};
            int[] levels = new int[] { 8, 8, 12, 12, 16, 16, 20, 20 };

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                      display_name,
                                      description,
                                      "",
                                      Wildshape.wolf_form_spell.Icon,
                                      FeatureGroup.None,
                                      Helpers.CreateAddAbilityResource(resource));
            feature.Ranks = 1;
            var minute_duration = Helpers.CreateContextDuration(1, DurationRate.Minutes);
            for (int i = 0; i < shapes.Length; i++)
            {
                var ability_i = library.CopyAndAdd<BlueprintAbility>(shapes[i], name_prefix + shapes[i].name, "");
                ability_i.AddComponent(Helpers.CreateResourceLogic(resource));
                ability_i.Type = AbilityType.Supernatural;
                ability_i.LocalizedDuration = Helpers.CreateString(ability_i.name + ".Duration", Helpers.oneMinuteDuration);
                ability_i.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_classes));
                ability_i.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(hex_classes, hex_stat));
                ability_i.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionApplyBuff>(a.Actions.Actions, b => b.DurationValue = minute_duration)));
                var feature_i = Common.AbilityToFeature(ability_i);

                feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(feature_i, levels[i], hex_classes));
            }

            foreach (var c in hex_classes)
            {
                feature.AddComponent(Helpers.PrerequisiteClassLevel(c, 8, any: true));
            }

            return feature;
        }


        public BlueprintFeature createDraconicResilence(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("f767399367df54645ac620ef7b2062bb").Icon; //form of the dragon

            var buff1 = Helpers.CreateBuff(name_prefix + "1HexBuff",
                                           display_name,
                                           description,
                                           "",
                                           icon,
                                           null,
                                           Common.createBuffDescriptorImmunity(SpellDescriptor.Sleep),
                                           Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Sleep)
                                           );

            var buff2 = library.CopyAndAdd<BlueprintBuff>(buff1, name_prefix + "2HexBuff", "");
            buff2.AddComponents(Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis),
                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis)
                                );


            var apply1 = Common.createContextActionApplyBuff(buff1, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
            var apply7 = Common.createContextActionApplyBuff(buff2, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
          

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.roundsPerLevelDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                                        Helpers.CreateActionList(apply1),
                                                                                                                        Helpers.CreateActionList(apply7))
                                                                        ),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                           type: AbilityRankType.StatBonus, stepLevel: 7, min: 1, max: 2, classes: hex_classes),
                                                Common.createAbilitySpawnFx("c4d861e816edd6f4eab73c55a18fdadd", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedFriendly(true);
            addWitchHexCooldownScaling(ability, "");

            //addToAmplifyHex(ability);
            

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            return feature;
        }


        public BlueprintFeature createFury(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("97b991256e43bb140b263c326f690ce2").Icon; //rage

            var buff = Helpers.CreateBuff(name_prefix + "HexBuff",
                                           display_name,
                                           description,
                                           "",
                                           icon,
                                           null,

                                           Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                           Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Resistance, rankType: AbilityRankType.StatBonus),
                                           Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Resistance, rankType: AbilityRankType.StatBonus),
                                           Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Resistance, rankType: AbilityRankType.StatBonus),
                                           Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                           type: AbilityRankType.StatBonus, startLevel: -8, stepLevel: 8, classes: hex_classes)
                                           );
            cackle_buffs.Add(buff);
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.SpeedBonus)), dispellable: false);

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                "Variable",
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, progression: ContextRankProgression.AsIs,
                                                                           type: AbilityRankType.SpeedBonus, stat: hex_stat),
                                                Common.createAbilitySpawnFx("97b991256e43bb140b263c326f690ce2", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                               );

            ability.setMiscAbilityParametersSingleTargetRangedFriendly(true);
            addWitchHexCooldownScaling(ability, "");


            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            return feature;
        }



        //battle spirit hexes
        public BlueprintFeature createBattleWardHex(string name_prefix, string display_name, string description)
        {
            var shield_spell = library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4"); //shield spell

            var ac_buffs = new BlueprintBuff[5];
            var actions = new ActionList[5];

            for (int i = 0; i < ac_buffs.Length; i++)
            {
                var on_attack_action = i == 0 ? Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>()) 
                                              : Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>(), actions[i - 1].Actions[0]);
                ac_buffs[i] = Helpers.CreateBuff(name_prefix + $"{i+1}Buff",
                                                display_name,
                                                description + $" (+{i+1})",
                                                "",
                                                shield_spell.Icon,
                                                null,
                                                Helpers.CreateAddStatBonus(StatType.AC, (i + 1), ModifierDescriptor.Deflection),
                                                Helpers.Create<AddTargetAttackRollTrigger>(a => {
                                                    a.ActionsOnAttacker = Helpers.CreateActionList(); a.OnlyHit = false;
                                                    a.ActionOnSelf = on_attack_action;
                                                })                                                    
                                                );
                ac_buffs[i].SetBuffFlags(BuffFlags.RemoveOnRest);
                actions[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(ac_buffs[i], Helpers.CreateContextDuration(), dispellable: false, is_permanent: true));
            }

            var hex_ability = library.CopyAndAdd<BlueprintAbility>(shield_spell, name_prefix + "HexAbility", "");
            hex_ability.RemoveComponents<SpellListComponent>();
            hex_ability.RemoveComponents<SpellComponent>();
            hex_ability.Type = AbilityType.Supernatural;
            hex_ability.setMiscAbilityParametersTouchFriendly();
            var effect = Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), actions);
            hex_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(effect));
            hex_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                     type: AbilityRankType.StatBonus, startLevel: -16, stepLevel: 8, classes: hex_classes));
            hex_ability.SetIcon(shield_spell.Icon);
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);
            hex_ability.setMiscAbilityParametersTouchFriendly();
            hex_ability.Range = AbilityRange.Touch;
            addWitchHexCooldownScaling(hex_ability, "");

            var battle_ward = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  hex_ability.Name,
                                                  hex_ability.Description,
                                                  "",
                                                  hex_ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(hex_ability));
            battle_ward.Ranks = 1;
            addToRodOfAbruptHexes(hex_ability);
            return battle_ward;
        }


        public BlueprintFeature createHamperingHex(string name_prefix, string display_name, string description)
        {
            var haze_of_dreams = library.Get<BlueprintAbility>("40ec382849b60504d88946df46a10f2d");

            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          haze_of_dreams.Icon,
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.None, ContextValueType.Rank, AbilityRankType.StatBonus, -2),
                                          Helpers.CreateAddContextStatBonus(StatType.AdditionalCMD, ModifierDescriptor.None, ContextValueType.Rank, AbilityRankType.StatBonus, -2),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                           type: AbilityRankType.StatBonus, startLevel: 0, stepLevel: 8, min: 1, max: 2, classes: hex_classes)
                                                                          
                                          );

            var apply_saved = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDurationNonExtandable(1), dispellable: false);
            var apply_failed = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
            var action_save = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(Helpers.CreateConditionalSaved(apply_saved, apply_failed)));

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "Variable",
                                                "Will special",
                                                haze_of_dreams.GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateRunActions(action_save));
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);

            addWitchHexCooldownScaling(ability, "");

            addToAmplifyHex(ability);
            var hampering_hex = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            hampering_hex.Ranks = 1;
            addToSplitHex(ability, hampering_hex, true);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(hampering_hex, ability);
            return hampering_hex;
        }

              //also a life hex
        public BlueprintFeature createCurseOfSuffering(string name_prefix, string display_name, string description)
        {
            var forced_repentance = library.Get<BlueprintAbility>("cc0aeb74b35cb7147bff6c53538bbc76");

            BlueprintBuff[] bleed_buffs = new BlueprintBuff[]
            {
                library.Get<BlueprintBuff>("5eb68bfe186d71a438d4f85579ce40c1"),
                library.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562"),
                library.Get<BlueprintBuff>("16249b8075ab8684ca105a78a047a5ef"),
                library.Get<BlueprintBuff>("f80de2a32fc2a7141b23ec29bc36f395") //constitution
            };

            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          forced_repentance.Icon,
                                          null,
                                          Helpers.Create<HealingMechanics.IncomingHealingModifier>(i => i.ModifierPercents = 50),
                                          Helpers.Create<BleedMechanics.IncreaseBleed>(i => i.amount = 1),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Bleed | SpellDescriptor.Curse)
                                          );

            /*foreach (var bb in bleed_buffs)
            {
                var context_actions = bb.GetComponent<AddFactContextActions>();
                ContextActionDealDamage dmg_action = context_actions.NewRound.Actions.Where(a => a is ContextActionDealDamage).FirstOrDefault() as ContextActionDealDamage;
                dmg_action = dmg_action.CreateCopy();
                dmg_action.Value = Helpers.CreateContextDiceValue(Kingmaker.RuleSystem.DiceType.Zero, bonus: 1);

                context_actions.NewRound = Helpers.CreateActionList(new GameAction[] { Helpers.CreateConditional(Common.createContextConditionHasFact(buff), dmg_action) }.AddToArray(context_actions.NewRound.Actions));
            }*/
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.roundsPerLevelDuration,
                                                "Will special",
                                                forced_repentance.GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateRunActions(apply_buff));
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);

            addWitchHexCooldownScaling(ability, "");

            //addToAmplifyHex(ability);
            //addToSplitHex(ability, true);
            var curse_of_suffering = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            curse_of_suffering.Ranks = 1;
            addToSplitHex(ability, curse_of_suffering);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(curse_of_suffering, ability);
            return curse_of_suffering;
        }

        //bones spirit hexes
        public BlueprintFeature createBoneWard(string name_prefix, string display_name, string description)
        {
            var shield_spell = library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4"); //shield spell
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/BoneWard.png");

            var ac_buffs = new BlueprintBuff[3];

            for (int i = 0; i < ac_buffs.Length; i++)
            {
                ac_buffs[i] = Helpers.CreateBuff(name_prefix + $"{i + 1}Buff",
                                                display_name,
                                                description + $" (+{2 + i})",
                                                "",
                                                icon,
                                                null,
                                                Helpers.CreateAddStatBonus(StatType.AC, (i + 2), ModifierDescriptor.Deflection)
                                                );
            }

            var action1 = Common.createContextActionApplyBuff(ac_buffs[0], Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
            var action2 = Common.createContextActionApplyBuff(ac_buffs[1], Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
            var action3 = Common.createContextActionApplyBuff(ac_buffs[1], Helpers.CreateContextDuration(1, DurationRate.Hours), dispellable: false);

            var hex_ability = library.CopyAndAdd<BlueprintAbility>(shield_spell, name_prefix + "HexAbility", "");
            hex_ability.RemoveComponents<SpellListComponent>();
            hex_ability.RemoveComponents<SpellComponent>();
            hex_ability.Type = AbilityType.Supernatural;
            hex_ability.setMiscAbilityParametersTouchFriendly();
            hex_ability.Range = AbilityRange.Touch;

            var effect = Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                        Helpers.CreateActionList(action1),
                                                                        Helpers.CreateActionList(action2),
                                                                        Helpers.CreateActionList(action3)
                                                                        );
            hex_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(effect));
            hex_ability.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                     type: AbilityRankType.StatBonus, startLevel: 0, stepLevel: 8, classes: hex_classes));
            hex_ability.SetIcon(icon);
            hex_ability.SetName(display_name);
            hex_ability.SetDescription(description);
            addWitchHexCooldownScaling(hex_ability, "");

            var bone_ward = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  hex_ability.Name,
                                                  hex_ability.Description,
                                                  "",
                                                  hex_ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(hex_ability));
            bone_ward.Ranks = 1;

            addToRodOfAbruptHexes(hex_ability);
            addToRodOfInterminableHexes(hex_ability);
            return bone_ward;
        }


        public BlueprintFeature createFearfulGaze(string name_prefix, string display_name, string description)
        {
            var fear = library.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0");

            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var frightened = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");

            var apply_shaken = Common.createContextActionApplyBuff(shaken, Helpers.CreateContextDuration(1), dispellable: false);
            var apply_fear = Common.createContextActionApplyBuff(frightened, Helpers.CreateContextDuration(1), dispellable: false);
            var saved_action = Helpers.CreateConditionalSaved(null, Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                Helpers.CreateActionList(apply_shaken),
                                                                                                Helpers.CreateActionList(apply_fear)
                                                                                                )
                                                             );

            var action_save = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(saved_action));

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                fear.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.oneRoundDuration,
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(action_save),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                           type: AbilityRankType.StatBonus, startLevel: 0, stepLevel: 8, min: 1, max: 2, classes: hex_classes),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Fear | SpellDescriptor.Shaken | SpellDescriptor.MindAffecting)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");

            addToAmplifyHex(ability);
            var fearful_gaze = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            fearful_gaze.Ranks = 1;
            addToSplitHex(ability, fearful_gaze, true);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(fearful_gaze, ability);
            return fearful_gaze;
        }


        public BlueprintFeature createBoneLock(string name_prefix, string display_name, string description)
        {
            var boneshaker = library.Get<BlueprintAbility>("b7731c2b4fa1c9844a092329177be4c3");

            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var staggered_save = library.CopyAndAdd<BlueprintBuff>(staggered, name_prefix + "BoneLockStaggeredSaveEachRound", "");
            staggered_save.ReplaceComponent<BuffStatusCondition>(b => { b.SaveEachRound = true; b.SaveType = SavingThrowType.Fortitude;});

            var apply1 = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1), dispellable: false);
            var apply8 = Common.createContextActionApplyBuff(staggered_save, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
            var apply16 = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

            var saved_action = Helpers.CreateConditionalSaved(null, Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                    Helpers.CreateActionList(apply1),
                                                                                    Helpers.CreateActionList(apply8),
                                                                                    Helpers.CreateActionList(apply16)
                                                                                    )
                                                            );


            var action_save = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(saved_action));

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                boneshaker.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.oneRoundDuration,
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(action_save),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                           type: AbilityRankType.StatBonus, startLevel: 0, stepLevel: 8, min: 1, max: 3, classes: hex_classes),
                                                Common.createAbilityTargetHasFact(true, library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f")), //construct
                                                Common.createAbilityTargetHasFact(true, library.Get<BlueprintFeature>("3bec99efd9a363242a6c8d9957b75e91")), //aberration
                                                Common.createAbilityTargetHasFact(true, library.Get<BlueprintFeature>("706e61781d692a042b35941f14bc41c5")), //plant
                                                Common.createAbilityTargetHasFact(true, library.Get<BlueprintFeature>("198fd8924dabcb5478d0f78bd453c586")) //elemental
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");

            addToAmplifyHex(ability);
            var bone_lock = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            bone_lock.Ranks = 1;
            addToSplitHex(ability, bone_lock, true);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(bone_lock, ability);
            return bone_lock;
        }

        //flame spirit hexes
        //will need to add cinder dance
        public BlueprintFeature createFlameWardHex(string name_prefix, string display_name, string description)
        {
            var sacred_nimbus_buff = library.Get<BlueprintBuff>("57b1c6a69c53f4d4ea9baec7d0a3a93a"); //shield spell

            var dmg_buffs = new BlueprintBuff[3];
            var actions = new ActionList[3];

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Fire,
                                                      Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), 1, Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                                      IgnoreCritical: true);

            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          sacred_nimbus_buff.Icon,
                                          sacred_nimbus_buff.FxOnStart);

            GameAction[] remove_action = new GameAction[3];

            for (int i = 0; i < dmg_buffs.Length; i++)
            {
                var on_attack_action = i == 0 ? Helpers.CreateActionList(Helpers.Create<ContextActionRemoveBuff>(c => c.Buff = buff))
                                              : Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>(), actions[i - 1].Actions[0]);
                dmg_buffs[i] = Helpers.CreateBuff(name_prefix + $"{i + 1}Buff",
                                                display_name,
                                                description + $" (+{i + 1})",
                                                "",
                                                sacred_nimbus_buff.Icon,
                                                null,
                                                Common.createAddTargetAttackWithWeaponTrigger(on_attack_action, 
                                                                                              Helpers.CreateActionList(dmg),
                                                                                              not_reach: false),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                                           type: AbilityRankType.DamageBonus, classes: hex_classes)
                                                );
                dmg_buffs[i].SetBuffFlags(BuffFlags.HiddenInUi);
                actions[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(dmg_buffs[i], Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true));
                remove_action[i] = Common.createContextActionRemoveBuff(dmg_buffs[i]);
            }


            buff.AddComponents(Helpers.CreateAddFactContextActions(new GameAction[] { Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), actions) },
                                                                   remove_action),
                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                           type: AbilityRankType.StatBonus, stepLevel: 8, classes: hex_classes)
                                                                  );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
            var hex_ability = Helpers.CreateAbility(name_prefix + "HexAbility",
                                                    display_name,
                                                    description,
                                                    "",
                                                    sacred_nimbus_buff.Icon,
                                                    AbilityType.Supernatural,
                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(apply_buff)
                                                    );

            hex_ability.setMiscAbilityParametersTouchFriendly();
            
            addWitchHexCooldownScaling(hex_ability, "");

            var flame_ward = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  hex_ability.Name,
                                                  hex_ability.Description,
                                                  "",
                                                  hex_ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(hex_ability));
            flame_ward.Ranks = 1;
            addToRodOfAbruptHexes(hex_ability);
            addToRodOfInterminableHexes(hex_ability);
            return flame_ward;
        }


        public BlueprintFeature createFireNimbus(string name_prefix, string display_name, string description)
        {
            var buff = library.CopyAndAdd<BlueprintBuff>("cc383a9eaae4d2b45a925d442b367b54", name_prefix + "Buff", ""); //faery fire
            buff.AddComponent(Common.createContextSavingThrowBonusAgainstDescriptor(-2, ModifierDescriptor.UntypedStackable, SpellDescriptor.Fire));
            buff.SetNameDescription(display_name, description);
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);

            var save = Helpers.CreateConditionalSaved(null, apply_buff);
           
            var action_save = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(apply_buff));

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.roundsPerLevelDuration,
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(action_save)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");

            addToAmplifyHex(ability);
            //addToSplitHex(ability, true);
            var fire_nimbus = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            fire_nimbus.Ranks = 1;
            addToSplitHex(ability, fire_nimbus, true);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(fire_nimbus, ability);
            return fire_nimbus;
        }


        public BlueprintFeature createFlameCurse(string name_prefix, string display_name, string description)
        {
            var fire_belly = library.Get<BlueprintBuff>("7c33de68880aa444bbb916271b653016"); //fire belly

            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          fire_belly.Icon,
                                          Common.createPrefabLink("f00bbb092bd65a4468e72869b99f1d66"),
                                          Common.createAddEnergyVulnerability(DamageEnergyType.Fire));

            var apply_buff1 = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(2, DurationRate.Rounds), dispellable: false));
            var apply_buff2 = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(3, DurationRate.Rounds), dispellable: false, duration_seconds: 15));
            var apply_buff3 = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(4, DurationRate.Rounds), dispellable: false, duration_seconds: 21));

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "Variable",
                                                Helpers.savingThrowNone,
                                                Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                                        apply_buff1, apply_buff2, apply_buff3)),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                           type: AbilityRankType.StatBonus, stepLevel: 8, classes: hex_classes)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");

            //addToAmplifyHex(ability);
            //addToSplitHex(ability, true);
            var flame_curse = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            flame_curse.Ranks = 1;
            addToSplitHex(ability, flame_curse);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(flame_curse, ability);
            return flame_curse;
        }

        //waves spirit hexes
        //will need to add fluid magic     
        public BlueprintFeature createCrashingWaves(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("d8144161e352ca846a73cf90e85bf9ac").Icon;

            var caster_level_increase = Helpers.Create<NewMechanics.ContextIncreaseSpellDescriptorCasterLevel>();
            caster_level_increase.BonusCasterLevel = Helpers.CreateContextValue(AbilityRankType.Default);
            caster_level_increase.Descriptor = (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water;
            var caster_level_increase_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                               progression: ContextRankProgression.OnePlusDivStep,
                                                                               stepLevel: 8, max: 2, classes: hex_classes);

            var on_dmg_action = Helpers.CreateActionList(Helpers.Create<ContextActionKnockdownTarget>());
            var crashing_waves1 = Helpers.CreateFeature(name_prefix + "1HexFeature",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        FeatureGroup.None,
                                                        caster_level_increase,
                                                        caster_level_increase_config,
                                                        Helpers.Create<NewMechanics.ActionOnSpellDamage>(a =>
                                                                                                        {
                                                                                                            a.descriptor = (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water;
                                                                                                            a.save_type = SavingThrowType.Fortitude;
                                                                                                            a.action = on_dmg_action;
                                                                                                        }
                                                                                                        )
                                                        );
            crashing_waves1.HideInCharacterSheetAndLevelUp = true;
            var crashing_waves2 = Helpers.CreateFeature(name_prefix + "2HexFeature",
                                            "",
                                            "",
                                            "",
                                            null,
                                            FeatureGroup.None,
                                            caster_level_increase,
                                            caster_level_increase_config,
                                            Helpers.Create<NewMechanics.ActionOnSpellDamage>(a =>
                                                                                            {
                                                                                                a.descriptor = SpellDescriptor.None;
                                                                                                a.save_type = SavingThrowType.Fortitude;
                                                                                                a.action = on_dmg_action;
                                                                                            }
                                                                                            )
                                            );
            crashing_waves2.HideInCharacterSheetAndLevelUp = true;
            var crashing_waves = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                       display_name,
                                                       description,
                                                       "",
                                                       icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFeatureOnClassLevel(crashing_waves1, 16, hex_classes, before: true),
                                                       Helpers.CreateAddFeatureOnClassLevel(crashing_waves2, 16, hex_classes)
                                                       );
            return crashing_waves;
        }


        public  BlueprintFeature createBeckoningChill(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931").Icon;
            var entangle = library.CopyAndAdd<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38", name_prefix + "BeckoningChillEntangle", "");

            entangle.Stacking = StackingType.Prolong;
            entangle.FxOnStart = Common.createPrefabLink("21b65d177b9db1d4ca4961de15645d95");
            var apply_entangle = Common.createContextActionApplyBuff(entangle, Helpers.CreateContextDuration(1));
            
            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          icon,
                                          Common.createPrefabLink("f00bbb092bd65a4468e72869b99f1d66"),
                                          Helpers.Create<NewMechanics.AddIncomingDamageTriggerOnAttacker>(a =>
                                                                                                          {
                                                                                                              a.on_self = true;
                                                                                                              a.consider_damage_type = true;
                                                                                                              a.energy_types = new DamageEnergyType[] { DamageEnergyType.Cold };
                                                                                                              a.Actions = Helpers.CreateActionList(apply_entangle);
                                                                                                          })
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes));
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.oneMinuteDuration,
                                                Helpers.savingThrowNone,
                                                Helpers.CreateRunActions(apply_buff)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");

            //addToAmplifyHex(ability);
            //addToSplitHex(ability, true);
            var beckoning_chill = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            beckoning_chill.Ranks = 1;
            addToSplitHex(ability, beckoning_chill);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(beckoning_chill, ability);
            return beckoning_chill;
        }


        public BlueprintFeature createMistsShroud(string name_prefix, string display_name, string description)
        {
            var blur_buff = library.Get<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674"); 

            var concealement_buffs = new BlueprintBuff[3];
            var actions = new ActionList[3];



            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          blur_buff.Icon,
                                          blur_buff.FxOnStart);

            GameAction[] remove_action = new GameAction[3];

            for (int i = 0; i < concealement_buffs.Length; i++)
            {
                var on_attack_action = i == 0 ? Helpers.CreateActionList(Helpers.Create<ContextActionRemoveBuff>(c => c.Buff = buff))
                                              : Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>(), actions[i - 1].Actions[0]);
                concealement_buffs[i] = Helpers.CreateBuff(name_prefix + $"{i + 1}Buff",
                                                display_name,
                                                description + $" (+{i + 1})",
                                                "",
                                                blur_buff.Icon,
                                                null,
                                                blur_buff.GetComponent<AddConcealment>(),
                                                Helpers.Create<NewMechanics.AddTargetConcealmentRollTrigger>(a => { a.only_on_miss = true; a.actions = on_attack_action; })
                                                );
                concealement_buffs[i].SetBuffFlags(BuffFlags.HiddenInUi);
                actions[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(concealement_buffs[i], Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true));
                remove_action[i] = Common.createContextActionRemoveBuff(concealement_buffs[i]);
            }


            buff.AddComponents(Helpers.CreateAddFactContextActions(new GameAction[] { Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), actions) },
                                                                   remove_action),
                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                           type: AbilityRankType.StatBonus, stepLevel: 8, classes: hex_classes)
                                                                  );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
            var hex_ability = Helpers.CreateAbility(name_prefix + "HexAbility",
                                                    display_name,
                                                    description,
                                                    "",
                                                    blur_buff.Icon,
                                                    AbilityType.Supernatural,
                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(apply_buff)
                                                    );

            hex_ability.setMiscAbilityParametersTouchFriendly();

            addWitchHexCooldownScaling(hex_ability, "");

            var mists_shroud = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  hex_ability.Name,
                                                  hex_ability.Description,
                                                  "",
                                                  hex_ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(hex_ability));
            mists_shroud.Ranks = 1;

            addToRodOfAbruptHexes(hex_ability);
            addToRodOfInterminableHexes(hex_ability);
            return mists_shroud;
        }

        //life spirit hexes
        //curse of suffering from battle spirit
        public BlueprintFeature createEnchancedCures(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("0d657aa811b310e4bbd8586e60156a2d").Icon;

            var healing_spells = new BlueprintAbility[]
            {   //cure
                library.Get<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f"),
                library.Get<BlueprintAbility>("1c1ebf5370939a9418da93176cc44cd9"),
                library.Get<BlueprintAbility>("6e81a6679a0889a429dec9cedcf3729c"),
                library.Get<BlueprintAbility>("0d657aa811b310e4bbd8586e60156a2d"),
                library.Get<BlueprintAbility>("5d3d689392e4ff740a761ef346815074"),
                library.Get<BlueprintAbility>("571221cc141bc21449ae96b3944652aa"),
                library.Get<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397"),
                library.Get<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449")
            };

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  display_name,
                                                  description,
                                                  "",
                                                  icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<HealingMechanics.ExtendHpBonusToCasterLevel>(e => e.spells = healing_spells));
            feature.Ranks = 1;
            return feature;
        }


        public BlueprintFeature createLifeSight(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("b3da3fbee6a751d4197e446c7e852bcb").Icon; //true seeing
            var resource = Helpers.CreateAbilityResource(name_prefix + "HexResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, hex_classes);

            var buff = Helpers.CreateBuff(name_prefix + "HexBuff",
                                         display_name,
                                         description,
                                         "",
                                         icon,
                                         null,
                                         Common.createBlindsight(30),
                                         Common.createBuffDescriptorImmunity(SpellDescriptor.GazeAttack),
                                         Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.GazeAttack)
                                         );

            var ability = Helpers.CreateActivatableAbility(name_prefix + "HexActivatableAbility",
                                                           display_name,
                                                           description,
                                                           "",
                                                           icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.NewRound)
                                                           );


            var feature = Common.ActivatableAbilityToFeature(ability, hide: false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            foreach (var c in hex_classes)
            {
                feature.AddComponent(Helpers.PrerequisiteClassLevel(c, 12, any: true));
            }
            feature.Ranks = 1;
            return feature;
        }


        public BlueprintFeature createLifeLink(string name_prefix, string display_name, string description)
        {
            var clw = library.Get<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f");
            var resource = Helpers.CreateAbilityResource(name_prefix + "HexResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, hex_classes);

            var spawn_fx = Helpers.Create<ContextActionSpawnFx>(c => c.PrefabLink = clw.GetComponent<AbilitySpawnFx>().PrefabLink);
            var transfer_damage = Helpers.Create<HealingMechanics.ContextActionTransferDamageToCaster>(c => c.Value = 5);
            var new_round_action = Helpers.CreateConditional(Helpers.Create<ContextConditionDistanceToTarget>(c => c.DistanceGreater = Common.medium_range_ft.Feet()),
                                                                                                              Helpers.Create<ContextActionRemoveSelf>(),
                                                                                                              Helpers.CreateConditional(Helpers.Create<ContextConditionHasDamage>(),
                                                                                                                                        new GameAction[] { spawn_fx, transfer_damage }
                                                                                                                                        )
                                                            );

            var buff = Helpers.CreateBuff(name_prefix + "HexBuff",
                                          display_name,
                                          description,
                                          "",
                                          clw.Icon,
                                          null,
                                          Helpers.CreateAddFactContextActions(deactivated: Common.createContextActionOnContextCaster(Helpers.Create<ResourceMechanics.ContextRestoreResource>(c => c.Resource = resource)),
                                                                              newRound: new_round_action)
                                         );

            buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var ability = Helpers.CreateAbility(name_prefix + "Hexbility",
                                                           display_name,
                                                           description,
                                                           "",
                                                           buff.Icon,
                                                           AbilityType.Supernatural,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                           AbilityRange.Close,
                                                           "Permanent",
                                                           "",
                                                           Helpers.CreateRunActions(apply_buff),
                                                           Helpers.Create<NewMechanics.AbilityTargetHasBuffFromCaster>(a => { a.Buffs = new BlueprintBuff[] { buff }; a.not = true; }),
                                                           Helpers.CreateResourceLogic(resource)
                                                           );
            ability.setMiscAbilityParametersSingleTargetRangedFriendly();
            var dismiss = Helpers.CreateAbility(name_prefix + "DismissHexAbility",
                                                           "Dismiss: " + display_name,
                                                           description,
                                                           "",
                                                           buff.Icon,
                                                           AbilityType.Supernatural,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           AbilityRange.Medium,
                                                           "",
                                                           "",
                                                           Helpers.CreateRunActions(Common.createContextActionRemoveBuffFromCaster(buff)),
                                                           Helpers.Create<NewMechanics.AbilityTargetHasBuffFromCaster>(a => a.Buffs = new BlueprintBuff[] {buff})
                                                           );
            dismiss.setMiscAbilityParametersSingleTargetRangedFriendly();
            var feature = Common.AbilityToFeature(ability, hide: false);
            feature.AddComponent(Helpers.CreateAddFact(dismiss));
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));
            feature.Ranks = 1;
            return feature;
        }







        //lore spirit hexes
        public BlueprintFeature createBrainDrain(string name_prefix, string display_name, string description)
        {
            var mind_blank = library.Get<BlueprintAbility>("eabf94e4edc6e714cabd96aa69f8b207");

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Divine, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D4), Helpers.CreateContextValue(AbilityRankType.DamageBonus)));
            dmg.DamageType.Type = Kingmaker.RuleSystem.Rules.Damage.DamageType.Direct;
            var action = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, dmg)));
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                mind_blank.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "",
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(action),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                                                type: AbilityRankType.DamageBonus, min: 1, classes: hex_classes)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "", cooldown_only_on_success: true);

            addToAmplifyHex(ability);
            //addToSplitHex(ability, true);
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            addToSplitHex(ability, feature, true);
            addToRodOfAbruptHexes(ability);
            addToHexStrike(feature, ability);
            return feature;
        }


        public BlueprintFeature createConfusionCurse(string name_prefix, string display_name, string description)
        {
            var confused = library.Get<BlueprintBuff>("886c7407dc629dc499b9f1465ff382df");

            var apply_buff = Common.createContextActionApplyBuff(confused, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), dispellable: false);
            var action = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_buff)));
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                confused.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "",
                                                Helpers.willNegates,
                                                Helpers.CreateRunActions(action),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, progression: ContextRankProgression.AsIs,
                                                                                type: AbilityRankType.StatBonus, min: 1, stat: hex_secondary_stat)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "", cooldown_only_on_success: true);

            addToAmplifyHex(ability);
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            addToSplitHex(ability, feature, true);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(feature, ability);
            return feature;
        }


        public BlueprintFeature createMentalAcuity(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("ae4d3ad6a8fda1542acf2e9bbc13d113").Icon; //foxs cunning
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddContextStatBonus(StatType.Intelligence, ModifierDescriptor.Inherent),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                                stepLevel: 5, classes: hex_classes)
                                               );
            feature.Ranks = 1;
            return feature;
        }


        public BlueprintFeature createBenefitOfWisdom(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("f0455c9295b53904f9e02fc571dd2ce1").Icon; //owls wisdom
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None
                                               );

            if (hex_stat == StatType.Wisdom)
            {
                feature.AddComponents(Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s => {
                                                                                                                        s.StatTypeToReplaceBastStatFor = StatType.SkillKnowledgeArcana;
                                                                                                                        s.NewBaseStatType = StatType.Wisdom; }),
                                        Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s => {
                                                                                                                         s.StatTypeToReplaceBastStatFor = StatType.SkillKnowledgeWorld;
                                                                                                                         s.NewBaseStatType = StatType.Wisdom;
                                                                                                                    }),
                                      Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom),
                                      Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence)
                                     );
            }
            else if (hex_stat == StatType.Intelligence)
            {
                feature.AddComponents(Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s => {
                                                                                                                        s.StatTypeToReplaceBastStatFor = StatType.SkillLoreNature;
                                                                                                                        s.NewBaseStatType = StatType.Intelligence;
                                                                                                                    }),
                                      Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s => {
                                                                                                                        s.StatTypeToReplaceBastStatFor = StatType.SkillLoreReligion;
                                                                                                                        s.NewBaseStatType = StatType.Intelligence;
                                                                                                                    }),
                                      Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom),
                                      Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence)
                                     );
            }
            else if (hex_stat == StatType.Charisma)
            {
                feature.AddComponents(Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s => {
                                                                                                                        s.StatTypeToReplaceBastStatFor = StatType.SkillLoreNature;
                                                                                                                        s.NewBaseStatType = StatType.Charisma;
                                                                                                                    }),
                                      Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s => {
                                                                                                                        s.StatTypeToReplaceBastStatFor = StatType.SkillLoreReligion;
                                                                                                                        s.NewBaseStatType = StatType.Charisma;
                                                                                                                    }),
                                      Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s => {
                                                                                                                        s.StatTypeToReplaceBastStatFor = StatType.SkillKnowledgeArcana;
                                                                                                                        s.NewBaseStatType = StatType.Charisma;
                                                                                                                    }),
                                      Helpers.Create<StatReplacementMechanics.ReplaceBaseStatForStatTypeLogic>(s => {
                                                                                                                        s.StatTypeToReplaceBastStatFor = StatType.SkillKnowledgeWorld;
                                                                                                                        s.NewBaseStatType = StatType.Charisma;
                                                                                                                    }),
                                      Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Charisma),
                                      Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Wisdom),
                                      Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence)
                                     );
            }


            feature.Ranks = 1;
            return feature;
        }








        //nature spirit
        public BlueprintFeature createEntanglingCurse(string name_prefix, string display_name, string description)
        {
            var entangle = library.Get<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38");

            var apply_buff = Common.createContextActionApplyBuff(entangle, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), dispellable: false);
            var action = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_buff)));
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                entangle.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "",
                                                "Reflex Negates",
                                                Helpers.CreateRunActions(action),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, progression: ContextRankProgression.AsIs,
                                                                                type: AbilityRankType.StatBonus, min: 1, stat: hex_secondary_stat)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");

            addToAmplifyHex(ability);
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            addToSplitHex(ability, feature, true);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(feature, ability);
            return feature;
        }


        public BlueprintFeature createStormWalker(string name_prefix, string display_name, string description)
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormStep.png");


            var feature_concelement = Helpers.CreateFeature(name_prefix + "IgnoreConcelementHexFeature",
                                                              "",
                                                              "",
                                                              "",
                                                              null,
                                                              FeatureGroup.None,
                                                              Helpers.Create<ConcealementMechanics.IgnoreFogConcelement>()
                                                              );
            feature_concelement.HideInCharacterSheetAndLevelUp = true;

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  display_name,
                                                  description,
                                                  "",
                                                  icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<WeatherMechanics.IgnoreWhetherMovementEffects>(),
                                                  Helpers.CreateAddFeatureOnClassLevel(feature_concelement, 10, hex_classes) 
                                                  );


            feature.Ranks = 1;
            return feature;
        }


        public BlueprintFeature createFriendToAnimals(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintAbility>("c6147854641924442a3bb736080cfeb6").Icon; //change shape beast
            var spontnaeous_summon = library.Get<BlueprintFeature>("b296531ffe013c8499ad712f8ae97f6b");
            var animal = library.Get<BlueprintFeature>("a95311b3dc996964cbaa30ff9965aaf6");

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                      display_name,
                                      description,
                                      "",
                                      icon,
                                      FeatureGroup.None);
            feature.Ranks = 1;

            var buff = Helpers.CreateBuff(name_prefix + "HexBuff",
                                          display_name,
                                          description,
                                          "",
                                          icon,
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Sacred),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Sacred),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Sacred),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: hex_secondary_stat,
                                                                          min: 0),
                                          Helpers.CreateAddFactContextActions(newRound: Helpers.CreateConditional(Common.createContextConditionHasFact(animal), 
                                                                                                                  null,
                                                                                                                  Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                  )
                                                                             )
                                         );

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", name_prefix + "HexArea", "");
            area.Size = 30.Feet();
            
            area.ReplaceComponent<AbilityAreaEffectBuff>(a => { a.Buff = buff; a.Condition = Helpers.CreateConditionsCheckerOr(Common.createContextConditionHasFact(animal)); });

            var aura_buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", name_prefix + "HexAuraBuff", "");
            aura_buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);
            feature.AddComponent(Common.createAuraFeatureComponent(aura_buff));

            foreach (var c in hex_classes)
            {
                var conversion_feature = Helpers.CreateFeature(name_prefix + c.name + "HexSpontaneousConversionFeature",
                                                               "",
                                                               "",
                                                               Helpers.MergeIds(c.AssetGuid, "c897bdad78a7475e9d789f40cb9c2941"),
                                                               null,
                                                               FeatureGroup.None
                                                               );
                foreach (var sc in spontnaeous_summon.GetComponents<SpontaneousSpellConversion>())
                {
                    conversion_feature.AddComponent(Common.createSpontaneousSpellConversion(c, sc.SpellsByLevel));
                }
                conversion_feature.HideInCharacterSheetAndLevelUp = true;

                feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(conversion_feature, 1, new BlueprintCharacterClass[] { c }));
            }



            return feature;
        }





        public BlueprintFeature createErosionCurse(string name_prefix, string display_name, string description)
        {
            var touch_of_slime = library.Get<BlueprintAbility>("1e481e03d9cf1564bae6b4f63aed2d1a");

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Divine, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.DamageBonus)), 
                                                     halfIfSaved: true);
            dmg.DamageType.Type = Kingmaker.RuleSystem.Rules.Damage.DamageType.Direct;
            var action = Common.createContextActionSavingThrow(SavingThrowType.Reflex, Helpers.CreateActionList(dmg));
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                touch_of_slime.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "",
                                                Helpers.reflexHalfDamage,
                                                Helpers.CreateRunActions(action),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                Common.createAbilitySpawnFx("524f5d0fecac019469b9e58ce1b8402d", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                                                type: AbilityRankType.DamageBonus, min: 1, classes: hex_classes),
                                                Common.createAbilityTargetHasFact(inverted: false, library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f"))//construct
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");

            addToAmplifyHex(ability);
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            addToSplitHex(ability, feature, true);
            addToRodOfAbruptHexes(ability);
            addToHexStrike(feature, ability);
            return feature;
        }

        //stone spirit hexes

        public BlueprintFeature createStoneStability(string name_prefix, string display_name, string description)
        {
            var improved_trip = library.Get<BlueprintFeature>("0f15c6f70d8fb2b49aa6cc24239cc5fa");
            var greater_trip = library.Get<BlueprintFeature>("4cc71ae82bdd85b40b3cfe6697bb7949");

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                display_name,
                                                description,
                                                "",
                                                library.Get<BlueprintFeature>("2a6a2f8e492ab174eb3f01acf5b7c90a").Icon, //defensive stance
                                                FeatureGroup.None,
                                                Common.createManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Trip, 4),
                                                Common.createManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.BullRush, 4),
                                                Helpers.CreateAddFeatureOnClassLevel(improved_trip, 5, hex_classes),
                                                Helpers.CreateAddFeatureOnClassLevel(greater_trip, 10, hex_classes)
                                                );
            return feature;
        }


        public BlueprintFeature createMetalCurse(string name_prefix, string display_name, string description)
        {
            var magnetic_infusion = library.Get<BlueprintBuff>("07afee46a4533e74bbb2e962768864ad");
            var actions = new ActionList[3];
            for (int i = 0; i < actions.Length; i++)
            {
                var buff = Helpers.CreateBuff(name_prefix + $"{i + 1}HexBuff",
                                              display_name,
                                              description,
                                              "",
                                              magnetic_infusion.Icon,
                                              magnetic_infusion.FxOnStart,
                                              Helpers.Create<ACBonusAgainstWeaponSubcategory>(a => { a.ArmorClassBonus = -((i+1) * 2); a.SubCategory = WeaponSubCategory.Metal; })
                                              );
                actions[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(i + 1), dispellable: false));
            }

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                magnetic_infusion.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "",
                                                Helpers.savingThrowNone,
                                                Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                                        actions)
                                                                        ),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.DivStep,
                                                                                type: AbilityRankType.StatBonus, stepLevel: 8, classes: hex_classes)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");


            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            addToSplitHex(ability, feature);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(feature, ability);
            return feature;
        }


        public BlueprintFeature createWardOfStone(string name_prefix, string display_name, string description)
        {
            var stoneskin = library.Get<BlueprintBuff>("7aeaf147211349b40bb55c57fec8e28d");

            var dr_buffs = new BlueprintBuff[3];
            var actions = new ActionList[3];

            var dr = Common.createMaterialDR(5, PhysicalDamageMaterial.Adamantite);

            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          stoneskin.Icon,
                                          stoneskin.FxOnStart);

            GameAction[] remove_action = new GameAction[3];

            for (int i = 0; i < dr_buffs.Length; i++)
            {
                var on_attack_action = i == 0 ? Helpers.CreateActionList(Helpers.Create<ContextActionRemoveBuff>(c => c.Buff = buff))
                                              : Helpers.CreateActionList(Helpers.Create<ContextActionRemoveSelf>(), actions[i - 1].Actions[0]);
                dr_buffs[i] = Helpers.CreateBuff(name_prefix + $"{i + 1}Buff",
                                                display_name,
                                                description + $" (+{i + 1})",
                                                "",
                                                stoneskin.Icon,
                                                null,
                                                Common.createAddTargetAttackWithWeaponTrigger(on_attack_action,
                                                                                              null,
                                                                                              not_reach: false),
                                                dr
                                                );
                dr_buffs[i].SetBuffFlags(BuffFlags.HiddenInUi);
                actions[i] = Helpers.CreateActionList(Common.createContextActionApplyBuff(dr_buffs[i], Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true));
                remove_action[i] = Common.createContextActionRemoveBuff(dr_buffs[i]);
            }


            buff.AddComponents(Helpers.CreateAddFactContextActions(new GameAction[] { Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), actions) },
                                                                   remove_action),
                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                           type: AbilityRankType.StatBonus, stepLevel: 8, classes: hex_classes)
                                                                  );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
            var hex_ability = Helpers.CreateAbility(name_prefix + "HexAbility",
                                                    display_name,
                                                    description,
                                                    "",
                                                    stoneskin.Icon,
                                                    AbilityType.Supernatural,
                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(apply_buff)
                                                    );

            hex_ability.setMiscAbilityParametersTouchFriendly();

            addWitchHexCooldownScaling(hex_ability, "");

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  hex_ability.Name,
                                                  hex_ability.Description,
                                                  "",
                                                  hex_ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(hex_ability));
            feature.Ranks = 1;
            addToRodOfAbruptHexes(hex_ability);
            addToRodOfInterminableHexes(hex_ability);
            return feature;
        }

        //loadstone replace with slow effect
       public  BlueprintFeature createLoadStone(string name_prefix, string display_name, string description)
        {
            var load_stone = library.CopyAndAdd<BlueprintAbility>("f492622e473d34747806bdb39356eb89", name_prefix + "HexAbility", "");//slow

            load_stone.RemoveComponents<SpellListComponent>();
            load_stone.RemoveComponents<SpellComponent>();
            load_stone.RemoveComponents<AbilityTargetsAround>();
            load_stone.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            load_stone.AvailableMetamagic = 0;
            load_stone.SpellResistance = false;
            load_stone.SetNameDescription(display_name, description);
            load_stone.Range = AbilityRange.Close;
            addWitchHexCooldownScaling(load_stone, "");


            addToAmplifyHex(load_stone);
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  load_stone.Name,
                                                  load_stone.Description,
                                                  "",
                                                  load_stone.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(load_stone));
            feature.Ranks = 1;
            addToSplitHex(load_stone, feature, true);
            addToRodOfAbruptHexes(load_stone);
            addToRodOfInterminableHexes(load_stone);
            addToHexStrike(feature, load_stone);
            return feature;
        }


        //wind spirit hexes
        public BlueprintFeature createAirBarrier(string name_prefix, string display_name, string description)
        {
            var mage_armor = library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568");
            var buff = Helpers.CreateBuff(name_prefix + "HexBuff",
                                          display_name,
                                          description,
                                          "",
                                          mage_armor.Icon,
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Armor, rankType: AbilityRankType.Default, multiplier: 2),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                          classes: hex_classes, stepLevel: 4, startLevel: -1, min: 2)
                                         );
            var buff2 = Helpers.CreateBuff(name_prefix + "Hex2Buff",
                              display_name,
                              description,
                              "",
                              buff.Icon,
                              null,
                              Helpers.Create<AddConcealment>(c => { c.CheckWeaponRangeType = true;
                                                                    c.RangeType = AttackTypeAttackBonus.WeaponRangeType.Ranged;
                                                                    c.Concealment = Concealment.Total;
                                                                    c.Descriptor = ConcealmentDescriptor.Fog;
                                                                  }
                                                            )
                             );
            buff2.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Hours), dispellable: false);
            var apply_buff2 = Common.createContextActionApplyBuff(buff2, Helpers.CreateContextDuration(1, DurationRate.Hours), dispellable: false);
            var resource = Helpers.CreateAbilityResource(name_prefix + "HexResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, hex_classes);

            var ability = Helpers.CreateAbility(name_prefix + "HexAbility",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                "One hour",
                                                "",
                                                mage_armor.GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                                        Helpers.CreateActionList(apply_buff),
                                                                                                                        Helpers.CreateActionList(apply_buff, apply_buff2)
                                                                                                                        )
                                                                        ),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_classes,
                                                                                type: AbilityRankType.StatBonus, progression: ContextRankProgression.OnePlusDivStep,
                                                                                stepLevel: 13),
                                                Helpers.CreateResourceLogic(resource)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            var feature = Common.AbilityToFeature(ability);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            return feature;
        }


        public BlueprintFeature createVortexSpells(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintFeature>("f2fa7541f18b8af4896fbaf9f2a21dfe").Icon; //cyclone form infusion

            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var apply_staggered1 = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(1), dispellable: false);
            var apply_staggered1d4 = Common.createContextActionApplyBuff(staggered, Helpers.CreateContextDuration(0, diceType: Kingmaker.RuleSystem.DiceType.D4, diceCount: 1), dispellable: false);
            var action = Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                        Helpers.CreateActionList(apply_staggered1),
                                                                        Helpers.CreateActionList(apply_staggered1d4)
                                                                        );
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.Create<NewMechanics.ActionOnSpellDamage>(a => { a.only_critical = true; a.action = Helpers.CreateActionList(action); }),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_classes,
                                                                                progression: ContextRankProgression.OnePlusDivStep, stepLevel: 11)
                                               );
            return feature;
        }


        public BlueprintFeature createSparklingAura(string name_prefix, string display_name, string description)
        {
            var buff = library.CopyAndAdd<BlueprintBuff>("cc383a9eaae4d2b45a925d442b367b54", name_prefix + "Buff", ""); //faery fire           
            buff.SetNameDescriptionIcon(display_name, description, LoadIcons.Image2Sprite.Create(@"AbilityIcons/SparklingAura.png"));
            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity,
                                                     Helpers.CreateContextDiceValue(Kingmaker.RuleSystem.DiceType.Zero, bonus: Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                                     IgnoreCritical: true);
            var on_hit = Helpers.Create<NewMechanics.TargetWeaponSubCategoryAttackTrigger>(w =>
            {
                w.ActionOnSelf = Helpers.CreateActionList(dmg);
                w.ActionsOnAttacker = Helpers.CreateActionList();
                w.SubCategory = WeaponSubCategory.Metal;
            });
            buff.AddComponents(on_hit,
                               Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, progression: ContextRankProgression.AsIs,
                                                                           type: AbilityRankType.DamageBonus, stat: hex_secondary_stat)
                               );
            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), dispellable: false);
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "1 round / 2 levels",
                                                Helpers.savingThrowNone,
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                                           type: AbilityRankType.StatBonus, min: 1, classes: hex_classes)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            addToSplitHex(ability, feature);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(feature, ability);
            return feature;
        }


        public BlueprintFeature createWindWard(string name_prefix, string display_name, string description)
        {
            var buff1 = library.Get<BlueprintBuff>("49786ccc94a5ee848a5637b4145b2092");//chameleon stride
            var buff2 = library.CopyAndAdd<BlueprintBuff>("49786ccc94a5ee848a5637b4145b2092", name_prefix + "2HexBuff", "");
            buff2.ReplaceComponent<AddConcealment>(a => a.Concealment = Concealment.Total);


            var action1 = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff1, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), dispellable: false));
            var action2 = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff1, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false));
            var action3 = Helpers.CreateActionList(Common.createContextActionApplyBuff(buff2, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false));

            var hex_ability = Helpers.CreateAbility(name_prefix + "HexAbility",
                                                    display_name,
                                                    description,
                                                    "",
                                                    buff1.Icon,
                                                    AbilityType.Supernatural,
                                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                                            action1,
                                                                                                                            action2,
                                                                                                                            action3)
                                                                             ),
                                                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                           type: AbilityRankType.StatBonus, stepLevel: 8, classes: hex_classes)
                                                    );

            hex_ability.setMiscAbilityParametersTouchFriendly();

            addWitchHexCooldownScaling(hex_ability, "");

            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  hex_ability.Name,
                                                  hex_ability.Description,
                                                  "",
                                                  hex_ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(hex_ability));
            feature.Ranks = 1;

            addToRodOfAbruptHexes(hex_ability);
            addToRodOfInterminableHexes(hex_ability);
            return feature;
        }


        //heaven spirit hexes
        public BlueprintFeature createEnveloppingVoid(string name_prefix, string display_name, string description)
        {
            var blindness = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");

            var apply_buff = Common.createContextActionApplyBuff(blindness, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false);
            var action = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(Helpers.CreateConditionalSaved(null, apply_buff)));
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                blindness.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "",
                                                "Will Negates",
                                                Helpers.CreateRunActions(action),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_classes)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");

            addToAmplifyHex(ability);
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability));
            feature.Ranks = 1;
            addToSplitHex(ability, feature, true);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(feature, ability);
            return feature;
        }


        public BlueprintFeature createStarburn(string name_prefix, string display_name, string description)
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/Starburn.png");
            var resource = Helpers.CreateAbilityResource(name_prefix + "HexResource", "", "", "", null);
            resource.SetIncreasedByStat(1, hex_secondary_stat); //will make it 1 + charisma

            var faerie_fire_buff = library.Get<BlueprintBuff>("cc383a9eaae4d2b45a925d442b367b54");
   
            var apply_buff = Common.createContextActionApplyBuff(faerie_fire_buff, Helpers.CreateContextDuration(1), dispellable: false);
            var damage = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.Default)), halfIfSaved: true);

            var action = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(damage, Helpers.CreateConditionalSaved(null, apply_buff)));
            var cooldown_buff = Helpers.CreateBuff(name_prefix + "CooldownBuff",
                                                  "Cooldown: " + display_name,
                                                  description,
                                                  "",
                                                  icon,
                                                  null);
            var apply_cooldown = Common.createContextActionApplyBuffToCaster(cooldown_buff, Helpers.CreateContextDuration(0, DurationRate.Rounds, Kingmaker.RuleSystem.DiceType.D4, 1), dispellable: false);

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Close,
                                                "",
                                                "Fortitude half",
                                                Helpers.CreateRunActions(apply_cooldown, action),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: hex_classes, progression: ContextRankProgression.Div2, min: 1),
                                                Common.createAbilityCasterHasNoFacts(cooldown_buff),
                                                Helpers.CreateResourceLogic(resource)
                                               );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(test_mode);
            addWitchHexCooldownScaling(ability, "");

            addToAmplifyHex(ability);
            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability.Name,
                                                  ability.Description,
                                                  "",
                                                  ability.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFact(ability),
                                                  Helpers.CreateAddAbilityResource(resource));
            feature.Ranks = 1;
            addToSplitHex(ability, feature, true);
            addToRodOfAbruptHexes(ability);
            addToRodOfInterminableHexes(ability);
            addToHexStrike(feature, ability);
            return feature;
        }

        //lure of heavens is same as flight but will require level 10

        //heavens leap
        public BlueprintFeature createHeavensLeap(string name_prefix, string display_name, string description)
        {
            var dimension_door = library.Get<BlueprintAbility>("5bdc37e4acfa209408334326076a43bc"); //mass

            var mark_buff = Helpers.CreateBuff(name_prefix + "MarkBuff",
                                      display_name + " Target",
                                      description,
                                      "",
                                      dimension_door.Icon,
                                      null,
                                      Helpers.Create<UniqueBuff>());

            var apply_mark = Common.createContextActionApplyBuff(mark_buff, Helpers.CreateContextDuration(1), dispellable: false);


            var ability_mark = Helpers.CreateAbility(name_prefix + "MarkAbility",
                                    display_name + ": Select Target",
                                    description,
                                    "",
                                    dimension_door.Icon,
                                    AbilityType.Supernatural,
                                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                    AbilityRange.Close,
                                    "",
                                    "",
                                    Helpers.CreateRunActions(apply_mark)
                                   );
            ability_mark.setMiscAbilityParametersSingleTargetRangedFriendly(true);
            addWitchHexCooldownScaling(ability_mark, "");


            var ability_move = library.CopyAndAdd<BlueprintAbility>(dimension_door.AssetGuid, name_prefix + "MoveAbility", "");

            ability_move.Type = AbilityType.Supernatural;
            ability_move.Parent = null;
            ability_move.Range = AbilityRange.Close;
            ability_move.RemoveComponents<SpellComponent>();
            ability_move.RemoveComponents<SpellListComponent>();
            ability_move.RemoveComponents<RecommendationNoFeatFromGroup>();
            ability_move.SetNameDescription(display_name, description);

            var dimension_door_component = ability_move.GetComponent<AbilityCustomDimensionDoor>();

            var dimension_door_marked = Helpers.Create<NewMechanics.CustomAbilities.AbilityCustomDimensionDoorOnTargetWithBuffFromCaster>(a =>
            {
                a.buff = mark_buff;
                a.CasterAppearFx = dimension_door_component.CasterAppearFx;
                a.CasterAppearProjectile = dimension_door_component.CasterAppearProjectile;
                a.CasterDisappearFx = dimension_door_component.CasterDisappearFx;
                a.CasterDisappearProjectile = dimension_door_component.CasterDisappearProjectile;
                a.PortalBone = dimension_door_component.PortalBone;
                a.PortalFromPrefab = dimension_door_component.PortalFromPrefab;
                a.Radius = 30.Feet();
                a.SideAppearFx = dimension_door_component.SideAppearFx;
                a.SideAppearProjectile = dimension_door_component.SideAppearProjectile;
                a.SideDisappearFx = dimension_door_component.SideDisappearFx;
                a.SideDisappearProjectile = dimension_door_component.SideDisappearProjectile;
            }
            );
            ability_move.ReplaceComponent(dimension_door_component, dimension_door_marked);
          
            addToRodOfAbruptHexes(ability_move);


            var feature = Helpers.CreateFeature(name_prefix + "HexFeature",
                                                  ability_move.Name,
                                                  ability_move.Description,
                                                  "",
                                                  ability_move.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.CreateAddFacts(ability_mark, ability_move));
            feature.Ranks = 1;
            return feature;
        }
    }
}
