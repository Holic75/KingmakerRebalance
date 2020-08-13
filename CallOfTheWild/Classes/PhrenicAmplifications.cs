using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class PhrenicAmplificationsEngine
    {
        static LibraryScriptableObject library => Main.library;

        BlueprintAbilityResource resource;
        BlueprintSpellbook spellbook;
        BlueprintCharacterClass character_class;
        static BlueprintFeature dual_amplification;
        string name_prefix;
        
        //subordinate spell
        //dispelling pulse

        public PhrenicAmplificationsEngine(BlueprintAbilityResource pool_resource, BlueprintSpellbook linked_spellbook, BlueprintCharacterClass linked_class, string asset_prefix)
        {
            resource = pool_resource;
            spellbook = linked_spellbook;
            character_class = linked_class;
            name_prefix = asset_prefix;
        }


        public BlueprintFeature[] createMimicMetamagic()
        {
            var metamagics = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(b => b.Groups.Contains(FeatureGroup.WizardFeat) && (b.GetComponent<AddMetamagicFeat>() != null) && b.AssetGuid != "2f5d1e705c7967546b72ad8218ccf99c").ToArray();

            var metamagic_selection = Helpers.CreateFeatureSelection(name_prefix + "MimicMetamagicSelection",
                                                                     "Mimic Metamagic",
                                                                     "When the psychic gains this amplification, she chooses two metamagic feats; she need not have these feats to select them. When she casts a spell, she can spend points from her phrenic pool to apply one of the chosen feats to the linked spell without increasing the spell’s level or casting time. She must spend a number of points equal to double the number of levels by which the feat normally increases a spell’s level (minimum 2 points). If the metamagic feat alters the spell’s casting time in a different way than the standard rules for a spontaneous caster using a metamagic feat (as in the case of Quicken Spell), it changes the casting time accordingly. The psychic can still apply metamagic feats she knows to the spell while using this amplification, increasing the casting time and spell level as normal. This amplification can be applied only to a spell that the chosen metamagic feat could normally affect, and only if the spellcaster can cast spells of a high enough level that she would be able to apply the metamagic feat in question to the linked spell. For example, an 11th-level psychic could spend 8 points to quicken a 1st-level spell, but couldn’t quicken a 2nd-level spell because she’s unable to cast 6th-level spells. This ability doesn’t require her to have any free spell slots in the relevant level, however, so the psychic in the example could quicken a 1st-level spell even if she had cast all her 5th-level spells for the day. A psychic can select this amplification multiple times, adding two additional options to the list of metamagic feats she can apply using this amplification each time.",
                                                                     "",
                                                                     null,
                                                                     FeatureGroup.None);

            foreach (var m in metamagics)
            {
                var metamagic_enum = m.GetComponent<AddMetamagicFeat>().Metamagic;
                var feature = Helpers.CreateFeature(name_prefix + m.name,
                                                   "Mimic Metamagic: " + m.Name,
                                                   metamagic_selection.Description + "\n" + m.Name + ": " + m.Description,
                                                   "",
                                                   m.Icon,
                                                   FeatureGroup.None);

                var buff = Helpers.CreateBuff(name_prefix + m.name + "Buff",
                                              feature.Name,
                                              feature.Description,
                                              "",
                                              feature.Icon,
                                              null,
                                              Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(mm =>
                                              {
                                                  mm.Metamagic = metamagic_enum;
                                                  mm.limit_spell_level = true;
                                                  mm.resource = resource;
                                                  mm.amount = metamagic_enum.DefaultCost() * 2;
                                                  mm.spellbook = spellbook;
                                              })
                                              );
                var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                                 resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                 );
                toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
                toggle.AddComponent(Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = metamagic_enum.DefaultCost() * 2; }));
                feature.AddComponent(Helpers.CreateAddFact(toggle));
                metamagic_selection.AllFeatures = metamagic_selection.AllFeatures.AddToArray(feature);
            }


            var amplifications = new BlueprintFeature[metamagics.Length / 2];

            for (int i = 0; i < amplifications.Length; i++)
            {
                amplifications[i] = Helpers.CreateFeature(name_prefix + $"{i + 1}MimicMetamagicFeature",
                                                          metamagic_selection.Name + " " + Common.roman_id[i],
                                                          metamagic_selection.Description,
                                                          "",
                                                          metamagic_selection.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.Create<EvolutionMechanics.addSelection>(a => a.selection = metamagic_selection)
                                                          );
                if (i > 0)
                {
                    amplifications[i].AddComponent(Helpers.PrerequisiteFeature(amplifications[i - 1]));
                }
            }

            return amplifications;
        }

        public BlueprintFeature createDualAmplification()
        {
            if (dual_amplification != null)
            {
                return dual_amplification;
            }

            dual_amplification = Helpers.CreateFeature(name_prefix + "DualAmplificationFeature",
                                                       "DualAmplification",
                                                       "When the psychic uses this major amplification, she chooses two other amplifications or major amplifications she knows to apply to the same linked spell. She can’t apply the same amplification to the linked spell more than once, even if she can use that amplification for multiple different effects.",
                                                       "",
                                                       null,
                                                       FeatureGroup.None,
                                                       Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup())
                                                       );
            return dual_amplification;
        }


        public BlueprintFeature createSpaceRendingSpell()
        {
            var dimension_door = library.Get<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2");

            var actions = new ActionList[10];
            var buffs = new BlueprintBuff[9];
            var abilities = new BlueprintAbility[9];

            for (int i = 0; i< 9; i++)
            {
                buffs[i] = Helpers.CreateBuff(name_prefix + $"{i + 1}SpaceRendingSpellBuff",
                                            "Space-rending Spell " + Common.roman_id[i+1],
                                            "The psychic can warp space with her mind, teleporting herself as she casts her linked spell. She teleports herself 10 feet per point she spends from her phrenic pool (as dimension door). The maximum number of points she can spend in this way is equal to the linked spell’s level.",
                                            "",
                                            dimension_door.Icon,
                                            null);

                abilities[i] = Helpers.CreateAbility(name_prefix + $"{i + 1}SpaceRendingSpellAbility",
                                                     buffs[i].Name,
                                                     buffs[i].Description,
                                                     "",
                                                     buffs[i].Icon,
                                                     AbilityType.Supernatural,
                                                     UnitCommand.CommandType.Free,
                                                     AbilityRange.Custom,
                                                     "",
                                                     "",
                                                     resource.CreateResourceLogic(amount: i + 1),
                                                     Helpers.CreateRunActions(Helpers.Create<ContextActionCastSpell>(c => c.Spell = dimension_door))
                                                     );
                abilities[i].CustomRange = ((i + 1) * 10).Feet();
                abilities[i].setMiscAbilityParametersRangedDirectional();
                actions[i+1] = Helpers.CreateActionList(Common.createContextActionApplyBuff(buffs[i], Helpers.CreateContextDuration(1), dispellable: false));
            }

            for (int i = 0; i < 9; i++)
            {
                abilities[i].AddComponent(Common.createAbilityCasterHasFacts(buffs.Take(9 - i).ToArray()));
                var remove_buffs = Common.createContextActionOnContextCaster(Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = buffs));
                abilities[i].ReplaceComponent<AbilityEffectRunAction>(a => a.Actions.Actions = a.Actions.Actions.AddToArray(remove_buffs));
            }

            var ability = Common.createVariantWrapper(name_prefix + "SpaceRendingSpellAbility", "", abilities);
            ability.SetName("Space-rending Spell");

            var buff = Helpers.CreateBuff(name_prefix + "SpaceRendingSpellBuff",
                                                ability.Name,
                                                ability.Description,
                                                "",
                                                ability.Icon,
                                                null,
                                                Helpers.Create<OnCastMechanics.RunActionAfterSpellCastBasedOnLevel>(r => { r.spellbook = spellbook; r.actions = actions; })                                                     
                                                );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            var feature = Common.ActivatableAbilityToFeature(toggle, false);
            feature.AddComponent(Helpers.CreateAddFact(ability));
            return feature;
        }


        public BlueprintFeature createSynapticShock()
        {
            var target_buff = Helpers.CreateBuff(name_prefix + "SynapticShockTargetBuff",
                                                 "Synaptic Shock Target",
                                                 "The psychic manipulates an enemy’s mind with brute force, causing mental harm beyond that done by her linked spell. She can spend 1 point from her phrenic pool when casting a mind-affecting linked spell to select one of the spell’s targets. If the target is affected by the linked spell, that target is confused for 1 round. This amplification can be applied only to a mind-affecting spell that can affect a number of targets or that has an area. It has no effect on creatures that are immune to mind-affecting effects, unless the linked spell is able to affect such creatures (such as a spell that has both will of the dead and synaptic shock applied to it via dual amplification).",
                                                 "",
                                                 Helpers.GetIcon("cf6c901fb7acc904e85c63b342e9c949"), //confusion
                                                 null,
                                                 Helpers.Create<UniqueBuff>()
                                                 );
            target_buff.Stacking = StackingType.Stack;
            var ability = Helpers.CreateAbility(name_prefix + "SynapticShockTargetAbility",
                                                target_buff.Name,
                                                target_buff.Description,
                                                "",
                                                target_buff.Icon,
                                                AbilityType.Supernatural,
                                                UnitCommand.CommandType.Free,
                                                AbilityRange.Long,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(target_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true))
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            var confusion_buff = library.Get<BlueprintBuff>("886c7407dc629dc499b9f1465ff382df");
            var action = Helpers.CreateConditional(Helpers.Create<ResourceMechanics.ContextConditionTargetHasEnoughResource>(c => { c.resource = resource; c.on_caster = true; }),
                                                   new GameAction[]{Helpers.Create<ResourceMechanics.ContextActionSpendResourceFromCaster>(c => c.resource = resource),
                                                                    Common.createContextActionApplyBuff(confusion_buff, Helpers.CreateContextDuration(1)),
                                                                    Common.createContextActionRemoveBuffFromCaster(target_buff)
                                                                   }
                                                  );

            var cond_action = Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(target_buff), action);
            var buff = Helpers.CreateBuff(name_prefix + "SynapticShockCasterBuff",
                                                "Synaptic Shock",
                                                target_buff.Description,
                                                "",
                                                target_buff.Icon,
                                                null,
                                                Helpers.CreateAddFactContextActions(deactivated: Helpers.Create<BuffMechanics.RemoveUniqueBuff>(r => r.buff = target_buff)),
                                                Helpers.Create<SpellManipulationMechanics.ExtraEffectOnSpellApplyTarget>(a =>
                                                                                                        {
                                                                                                            a.descriptor = SpellDescriptor.MindAffecting;
                                                                                                            a.check_caster = true;
                                                                                                            a.actions = Helpers.CreateActionList(cond_action);
                                                                                                        }
                                                                                                        )
                                                );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            var feature = Common.ActivatableAbilityToFeature(toggle, false);
            feature.AddComponent(Helpers.CreateAddFact(ability));
            return feature;
        }

        public BlueprintFeature createFocusedForce()
        {
            var buff = Helpers.CreateBuff(name_prefix + "FocusedForceBuff",
                                          "Focused Force",
                                          "When casting a force spell, the psychic can increase the spell’s damage by spending 1 point from her phrenic pool. Increase the die size for the spell’s damage by one step (from 1d4 to 1d6, 1d6 to 1d8, 1d8 to 1d10, or 1d10 to 1d12). This increases the size of each die rolled, so a spell that dealt 4d6+3 points of force damage would deal 4d8+3 points of force damage instead. This amplification can be linked only to spells that deal force damage, and only if that damage includes a die value. A spell that already uses d12s for damage can’t be amplified in this way.",
                                          "",
                                          Helpers.GetIcon("0a2f7c6aa81bc6548ac7780d8b70bcbc"),
                                          null,
                                          Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(m =>
                                            {
                                                m.amount = 1;
                                                m.resource = resource;
                                                m.spell_descriptor = SpellDescriptor.Force;
                                                m.Metamagic = (Metamagic)MetamagicFeats.MetamagicExtender.ForceFocus;
                                                m.spellbook = spellbook;
                                            })                                        
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            var feature =  Common.ActivatableAbilityToFeature(toggle, false);
            feature.AddComponent(Helpers.Create<OnCastMechanics.ForceFocusSpellDamageDiceIncrease>(s => { s.spellbook = spellbook; s.SpellDescriptor = SpellDescriptor.Force; }));
            return feature;
        }


        public BlueprintFeature createPsychofeedback()
        {
            var icons = new UnityEngine.Sprite[]
            {
                Helpers.GetIcon("3553bda4d6dfe6344ad89b25f7be939a"), //dexterity
                Helpers.GetIcon("99cf556b967c2074ca284e127d815711"), //constitution
                Helpers.GetIcon("c7773d1b408fea24dbbb0f7bf3eb864e") //strength
            };

            var stats = new StatType[]
            {
                StatType.Dexterity,
                StatType.Constitution,
                StatType.Strength
            };
            var spell_arrays = new BlueprintAbility[][]
            {
                new BlueprintAbility[10],
                new BlueprintAbility[10],
                new BlueprintAbility[10]
            };

            var description = "The psychic can spend 2 points from her phrenic pool to sacrifice a linked spell of 2nd level or higher. Doing so grants the psychic a +1 enhancement bonus to Strength, Dexterity, or Constitution per level of the sacrificed spell. This bonus lasts for 1 minute per psychic level.";
            for (int i = 2; i <= 9; i++)
            {
                for (int j = 0; j < spell_arrays.Length; j++)
                {
                    var buff = Helpers.CreateBuff(name_prefix + stats[j].ToString() + $"{i}PsychoFeedbackBuff",
                                                  $"Psychofeedback (+{i} {stats[j].ToString()})",
                                                  description,
                                                  "",
                                                  icons[j],
                                                  null,
                                                  Helpers.CreateAddStatBonus(stats[j], i, ModifierDescriptor.Enhancement)
                                                  );

                    var apply_buff = Common.createContextActionApplyBuff(buff,
                                                                        Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), 
                                                                        dispellable: false);
                    spell_arrays[j][i] = Helpers.CreateAbility(name_prefix + stats[j].ToString() + $"{i}PsychoFeedbackAbility",
                                                            buff.Name,
                                                            buff.Description,
                                                            "",
                                                            buff.Icon,
                                                            AbilityType.Supernatural,
                                                            UnitCommand.CommandType.Standard,
                                                            AbilityRange.Personal,
                                                            Helpers.minutesPerLevelDuration,
                                                            "",
                                                            Helpers.CreateRunActions(apply_buff),
                                                            Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] {character_class}),
                                                            resource.CreateResourceLogic(amount: 2)
                                                            );
                    spell_arrays[j][i].setMiscAbilityParametersSelfOnly();
                }
            }

            var feature = Helpers.CreateFeature(name_prefix + "PsychofeedbackFeature",
                                                "Psychofeedback",
                                                description,
                                                "",
                                                icons[0],
                                                FeatureGroup.None,
                                                Common.createSpontaneousSpellConversion(character_class, spell_arrays[0]),
                                                Common.createSpontaneousSpellConversion(character_class, spell_arrays[1]),
                                                Common.createSpontaneousSpellConversion(character_class, spell_arrays[2])
                                                );

            return feature;
        }


        public BlueprintFeature createRelentnessCasting()
        {
            var buff = Helpers.CreateBuff(name_prefix + "RelentnessCastingBuff",
                                          "Relentness Casting",
                                          "The psychic can spend 1 point from her phrenic pool to roll twice on any caster level checks to overcome spell resistance required for the linked spell and take the better result. Because she must decide to spend points from her phrenic pool when she starts casting a spell, the psychic must decide to use this ability before the GM calls for the caster level check.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/Metamixing.png"),
                                          null,
                                            Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(m =>
                                            {
                                                m.amount = 1;
                                                m.resource = resource;
                                                m.Metamagic = (Metamagic)MetamagicFeats.MetamagicExtender.RollSpellResistanceTwice;
                                                m.spellbook = spellbook;
                                            })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                             );
            toggle.AddComponent(Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 1; }));
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            var feature = Common.ActivatableAbilityToFeature(toggle, false);
            return feature;
        }


        public BlueprintFeature createOngoingDefense()
        {
            var buff = Helpers.CreateBuff(name_prefix + "OngoingDefenseBuff",
                                          "Ongoing Defense",
                                          "The psychic can increase the duration of spells that improve her psychic defenses. She can spend 1 point from her phrenic pool when she casts any intellect fortress, mental barrier or thought shield spell to extend the spell’s duration by 1 round.",
                                          "",
                                          Helpers.GetIcon("62888999171921e4dafb46de83f4d67d"), //shield of dawn
                                          null,
                                            Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(m =>
                                            {
                                                m.amount = 1;
                                                m.resource = resource;
                                                m.Abilities = NewSpells.mental_barrier.AddToArray(NewSpells.thought_shield).AddToArray(NewSpells.intellect_fortress).ToList();
                                                m.Metamagic = (Metamagic)MetamagicFeats.MetamagicExtender.ExtraRoundDuration;
                                                m.spellbook = spellbook;
                                            })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            var feature = Common.ActivatableAbilityToFeature(toggle, false);
            feature.AddComponent(Helpers.Create<OnCastMechanics.IncreaseDurationBy1RoundIfMetamagic>(s => { s.spellbook = spellbook;}));
            return feature;
        }


        public BlueprintFeature createWillOfTheDead()
        {
            var will_of_the_dead = library.Get<BlueprintFeature>("1a5e7191279e7cd479b17a6ca438498c");
            var buff = Helpers.CreateBuff(name_prefix + "WillOfTheDeadBuff",
                                          "Will of the Dead",
                                          "Even undead creatures can be affected by the psychic’s mind-affecting spells. The psychic can spend 2 points from her phrenic pool to overcome an undead creature’s immunity to mind-affecting effects for the purposes of the linked spell. This ability functions even on mindless undead, but has no effect on creatures that aren’t undead. This amplification can be linked only to spells that have the mind-affecting descriptor.",
                                          "",
                                          will_of_the_dead.Icon,
                                          null,
                                            Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(m =>
                                            {
                                                m.Descriptor = SpellDescriptor.MindAffecting;
                                                m.amount = 2;
                                                m.resource = resource;
                                                m.Metamagic = (Metamagic)MetamagicFeats.MetamagicExtender.ThrenodicSpell;
                                                m.spellbook = spellbook;
                                            })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                             Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 2; })
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            return Common.ActivatableAbilityToFeature(toggle, false);
        }


        public BlueprintFeature createMindsEye()
        {         
            var buff = Helpers.CreateBuff(name_prefix + "MindsEyeBuff",
                                          "Mind's Eye",
                                          "Some psychics train their visual and psychic senses, binding them together into a unified focus to better guide their ranged spells and place them with uncanny precision. While casting a spell that requires a ranged attack roll, the psychic can spend 2 points from her phrenic pool and gain a +4 insight bonus on the attack roll.",
                                          "",
                                          Helpers.GetIcon("3c08d842e802c3e4eb19d15496145709"), //uncanny dodge
                                          null,
                                            Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(m =>
                                            {
                                                m.amount = 2;
                                                m.resource = resource;
                                                m.Metamagic = (Metamagic)MetamagicFeats.MetamagicExtender.RangedAttackRollBonus;
                                                m.spellbook = spellbook;
                                            })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                             Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 2; })
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            var feature =  Common.ActivatableAbilityToFeature(toggle, false);
            feature.AddComponent(Helpers.Create<OnCastMechanics.RangedSpellAttackRollBonusRangeAttackRollMetamagic>(s => { s.spellbook = spellbook; s.bonus = 4; s.descriptor = ModifierDescriptor.UntypedStackable; }));
            return feature;
        }


        public BlueprintFeature createConjuredArmor()
        {
            var effect_buff = Helpers.CreateBuff(name_prefix + "ConjuredArmorEffectBuff",
                                          "Conjured Armor",
                                          "By spending 1 point from her phrenic pool, the psychic grants any creature she conjures or summons with the linked spell a +2 deflection bonus to AC. This bonus lasts for 1 round per caster level or until the creature disappears, whichever comes first. This amplification can be linked only to conjuration (calling) or conjuration (summoning) spells. The bonus increases to +3 at 8th level and to +4 at 15th level.",
                                          "",
                                          Helpers.GetIcon("38155ca9e4055bb48a89240a2055dcc3"), //augmented summoning
                                          null,
                                          Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, Kingmaker.Enums.ModifierDescriptor.Deflection, ContextValueType.Rank, AbilityRankType.Default),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Custom, AbilityRankType.Default,
                                                                          customProgression: new (int, int)[] { (7, 2), (14, 3), (20, 4) },
                                                                          classes: new BlueprintCharacterClass[] {character_class}
                                                                          )
                                          );

            var buff = Helpers.CreateBuff(name_prefix + "ConjuredArmorBuff",
                                          effect_buff.Name,
                                          effect_buff.Description,
                                          "",
                                          Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //armor
                                          null,
                                          Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.resource = resource; s.spellbook = spellbook; s.school = SpellSchool.Conjuration; s.spell_descriptor = SpellDescriptor.Summoning; s.amount = 1; }),
                                          Helpers.Create<OnCastMechanics.OnSpawnBuff>(s => { s.spellbook = spellbook; s.school = SpellSchool.Conjuration; s.spell_descriptor = SpellDescriptor.Summoning; s.buff = effect_buff; s.duration_value = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)); }),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { character_class })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            return Common.ActivatableAbilityToFeature(toggle, false);
        }


        public BlueprintFeature createBiokineticHealing()
        {
            var buff = Helpers.CreateBuff(name_prefix + "BiokineticHealingBuff",
                                          "Biokinetic Healing",
                                          "When the psychic casts a linked spell from the transmutation school, she can spend 1 point from her phrenic pool to regain 2 hit points per level of the linked spell.",
                                          "",
                                          Helpers.GetIcon("8d6073201e5395d458b8251386d72df1"), //lay on hands self
                                          null,
                                          Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.resource = resource; s.spellbook = spellbook; s.school = SpellSchool.Transmutation; s.amount = 1; }),
                                          Helpers.Create<OnCastMechanics.HealAfterSpellCast>(s => { s.spellbook = spellbook; s.school = SpellSchool.Transmutation; s.multiplier = 2; })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            return Common.ActivatableAbilityToFeature(toggle, false);          
        }


        public BlueprintFeature createUndercastSurge()
        {
            var toggles = new BlueprintActivatableAbility[5];

            for (int i = 0; i < toggles.Length; i++)
            {
                var buff = Helpers.CreateBuff(name_prefix + $"UndercastSurge{i + 1}Buff",
                                                      "Undercast Surge " + Common.roman_id[i + 1],
                                                      "When the psychic undercasts a spell, she can spend points from her phrenic pool to increase the spell’s effective level, essentially using up a lower-level spell slot to cast a higher-level version of the spell. This costs 2 points per spell level increased. She can’t use this ability to cast a version higher than the version she knows. For instance, a psychic who knows mind thrust III but not mind thrust IV could cast mind thrust II and spend 2 points to treat it as mind thrust III, but couldn’t spend 4 points to treat it as mind thrust IV. This amplification can be linked only to spells that can be undercast.",
                                                      "",
                                                      Helpers.GetIcon("c3a8f31778c3980498d8f00c980be5f5"), //guidance
                                                      null,
                                                      Helpers.Create<SpellManipulationMechanics.SpendLowerLevelSpellSlot>(s =>
                                                                                                                          {
                                                                                                                              s.amount = (i + 1) * 2;
                                                                                                                              s.spell_slot_decrease = (i + 1);
                                                                                                                              s.undercast_only = true;
                                                                                                                              s.resource = resource;
                                                                                                                          }
                                                                                                                        )
                                                      );

                var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                                 resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                 );
                toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
                toggle.AddComponent(Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 2 * (i + 1); }));
                toggles[i] = toggle;
            }
            var feature = Helpers.CreateFeature(name_prefix + "UndercastSurgeFeature",
                                                "Undercast Surge",
                                                toggles[0].Description,
                                                "",
                                                toggles[0].Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFacts(toggles)
                                                );
            for (int i = 0; i < toggles.Length; i++)
            {
                for (int j = 0; j < toggles.Length; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }
                    toggles[i].AddComponent(Helpers.Create<RestrictionHasFact>(r => { r.Feature = toggles[j].Buff; r.Not = true; }));
                }
            }

            return feature;
        }





        public BlueprintFeature createDefensivePrognostication()
        {
            var toggles = new BlueprintActivatableAbility[2];

            for (int i = 0; i < 2; i++)
            {
                var effect_buff = Helpers.CreateBuff(name_prefix + $"DefensivePrognostication{i + 1}EffectBuff",
                                                      "Defensive Prognostication " + Common.roman_id[i + 1],
                                                      "When casting a divination spell, the psychic sees a glimmer of her future. By spending 1 point from her phrenic pool as she casts a divination spell, she gains a +2 insight bonus to AC for a number of rounds equal to the linked spell’s level. She can instead spend 2 points to increase the bonus to +4. This amplification can be linked only to divination spells.",
                                                      "",
                                                      Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //armor
                                                      null,
                                                      Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, (i+1)*2, Kingmaker.Enums.ModifierDescriptor.Insight)
                                                      );

                var buff = Helpers.CreateBuff(name_prefix + $"DefensivePrognostication{i+1}Buff",
                                              effect_buff.Name,
                                              effect_buff.Description,
                                              "",
                                              Helpers.GetIcon("ef768022b0785eb43a18969903c537c4"), //shield
                                              null,
                                              Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.resource = resource; s.spellbook = spellbook; s.school = SpellSchool.Divination; s.amount = i+1; }),
                                              Helpers.Create<OnCastMechanics.ApplyBuffAfterSpellCast>(s => { s.spellbook = spellbook; s.school = SpellSchool.Divination; s.buff = effect_buff; })
                                              );

                var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                                 resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                 );
                toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
                if (i > 0)
                {
                    toggle.AddComponent(Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 2; }));
                }
                toggles[i] = toggle;
            }


            for (int i = 0; i < toggles.Length; i++)
            {
                for (int j = 0; j < toggles.Length; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }
                    toggles[i].AddComponent(Helpers.Create<RestrictionHasFact>(r => { r.Feature = toggles[j].Buff; r.Not = true; }));
                }
            }
            var feature = Helpers.CreateFeature(name_prefix + "DefensivePrognosticationFeature",
                                                "Defensive Prognostication",
                                                toggles[0].Description,
                                                "",
                                                toggles[0].Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFacts(toggles)
                                                );
            return feature;
        }


        public BlueprintFeature createOverpoweringMind()
        {
            var feature = Helpers.CreateFeature(name_prefix + "Overpowering MindFeature",
                              "Overpowering Mind",
                              "The psychic can spend 2 points from her phrenic pool to increase the save DC of the linked spell by 1. At 8th level, she can choose to instead spend 4 points to increase the DC by 2. At 15th level, she can choose to instead spend 6 points to increase the DC by 3. This amplification can be linked only to spells that have the mind - affecting descriptor.",
                              "",
                              Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"), //mind fog
                              FeatureGroup.None
                              );
            for (int i = 0; i < 3; i++)
            {
                var buff = Helpers.CreateBuff(name_prefix + $"OverpoweringMind{i+1}Buff",
                                              "Overpowering Mind " + Common.roman_id[i+1],
                                              feature.Description,
                                              "",
                                              feature.Icon,
                                              null,
                                              Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.resource = resource; s.spellbook = spellbook; s.spell_descriptor = SpellDescriptor.MindAffecting; s.amount = 2*(i+1); }),
                                              Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellsDC>(c => { c.spellbook = spellbook; c.Descriptor = SpellDescriptor.MindAffecting; c.Value = i + 1; })
                                              );

                var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                                 resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                 );
                toggle.AddComponent(Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 2 * (i+1); }));
                toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();

                if (i == 0)
                {
                    feature.AddComponent(Helpers.CreateAddFact(toggle));
                }
                else
                {
                    feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(Common.ActivatableAbilityToFeature(toggle), 1 + i*7, new BlueprintCharacterClass[] { character_class }));
                }
            }
            return feature;
        }
    }
}
