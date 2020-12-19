using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Assets.UnitLogic.Mechanics.Actions;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs;
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
    class Spiritualist
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass spiritualist_class;
        static public BlueprintProgression spiritualist_progression;
        static public BlueprintFeature spiritualist_proficiencies;
        static public BlueprintFeature spiritualist_knacks;
        static public BlueprintFeatureSelection emotional_focus_selection;
        static public BlueprintFeature spiritualist_spellcasting;
        static public BlueprintFeature bonded_manifestation;

        static public BlueprintFeature link;
        static public BlueprintFeature etheric_tether;
        static public BlueprintFeature spiritual_bond;
        static public BlueprintFeature spiritual_inference;
        static public BlueprintFeature greater_spiritual_inference;

        static public BlueprintAbilityResource phantom_recall_resource;
        static public BlueprintFeature phantom_recall;
        static public BlueprintAbility phantom_recall_ability;
        static public BlueprintFeature fused_consciousness;
        static public BlueprintFeature dual_bond;
        static public BlueprintFeatureSelection potent_phantom;
        static public BlueprintFeature masters_alignment;

        static public BlueprintFeature shared_consciousness;
        static public BlueprintBuff unsummon_buff;
        static public BlueprintAbility summon_companion_ability;
        static public BlueprintAbility summon_call_ability;

        static public BlueprintArchetype hag_haunted;
        static public BlueprintFeature hag_phantom;
        static public BlueprintFeature hag_shared_consciousness;
        static public BlueprintFeature hag_fused_consciousness;
        static public BlueprintFeature hag_fused_consciousness12;
        static public BlueprintFeature hag_spell_casting;
        static public BlueprintFeature hag_curse7;
        static public BlueprintFeature hag_curse13;
        static public BlueprintSpellbook hag_haunted_spellbook;

        static public BlueprintArchetype onmyoji;
        static public BlueprintSpellbook onmyoji_spellbook;
        static public BlueprintFeature onmyoji_spellcasting;
        static public BlueprintFeatureSelection divine_teachings;

        static public BlueprintArchetype scourge;
        static public BlueprintFeature spell_scourge;
        static public BlueprintFeature ectoplasmic_swarm;

        static public BlueprintFeature charisma_spellcasting;
        static public BlueprintSpellbook fractured_mind_spellbook;
        static public BlueprintArchetype fractured_mind;

        static public BlueprintArchetype exciter;
        static public BlueprintFeatureSelection exciter_phantom_selection;
        static public BlueprintFeature rapture;
        static public BlueprintFeatureSelection exciter_potent_phantom;
        static public BlueprintFeatureSelection rapturous_rage;
        static public BlueprintFeature greater_rapture;
        static public BlueprintFeature overwhelming_excitement;
        static public BlueprintAbilityResource rapture_resource;
        static public BlueprintFeature perfect_passion;
        static public BlueprintFeature exciter_shared_consciousness;
        static public BlueprintBuff rapture_str_con_buff;
        static public BlueprintBuff rapture_dex_cha_buff;
        static public BlueprintBuff rapture_share_str_con_buff;
        static public BlueprintBuff rapture_share_dex_cha_buff;
        static public BlueprintBuff prevent_shared_conciousness_fact;
        static public BlueprintFeatureSelection emotional_conduit;

        //necrologist, exciter, priest of the fallen

        internal static void createSpiritualistClass()
        {
            Main.logger.Log("Spiritualist class test mode: " + test_mode.ToString());
            var inquisitor_class = library.TryGet<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");

            spiritualist_class = Helpers.Create<BlueprintCharacterClass>();
            spiritualist_class.name = "SpiritualistClass";
            library.AddAsset(spiritualist_class, "");

            spiritualist_class.LocalizedName = Helpers.CreateString("Spiritualist.Name", "Spiritualist");
            spiritualist_class.LocalizedDescription = Helpers.CreateString("Spiritualsit.Description",
                                                                         "Becoming a spiritualist is not a calling — it’s a phenomenon.\n"
                                                                         + "When a creature dies, its spirit flees its body and begins the next stage of its existence. Debilitating emotional attachments during life and other psychic corruptions cause some spirits to drift into the Ethereal Plane and descend toward the Negative Energy Plane. Some of these spirits are able to escape the pull of undeath and make their way back to the Material Plane, seeking refuge in a psychically attuned mind. Such a fusing of consciousnesses creates a spiritualist—the master of a single powerful phantom whom the spiritualist can manifest to do her bidding.\n"
                                                                         + "Role: The spiritualist seeks the occult and esoteric truth about life, death, and the passage beyond, using her phantom as a guide and tool. The connection with her phantom allows her to harness the powers of life and death, thought and nightmare, shadow and revelation."
                                                                         );
            spiritualist_class.m_Icon = inquisitor_class.Icon;
            spiritualist_class.SkillPoints = inquisitor_class.SkillPoints - 1;
            spiritualist_class.HitDie = DiceType.D8;
            spiritualist_class.BaseAttackBonus = inquisitor_class.BaseAttackBonus;
            spiritualist_class.FortitudeSave = inquisitor_class.FortitudeSave;
            spiritualist_class.ReflexSave = inquisitor_class.ReflexSave;
            spiritualist_class.WillSave = inquisitor_class.WillSave;
            spiritualist_class.Spellbook = createSpiritualistSpellbook();
            spiritualist_class.ClassSkills = new StatType[] { StatType.SkillPersuasion, StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion, StatType.SkillPerception,
                                                         StatType.SkillUseMagicDevice};
            spiritualist_class.IsDivineCaster = false;
            spiritualist_class.IsArcaneCaster = false;
            spiritualist_class.StartingGold = inquisitor_class.StartingGold;
            spiritualist_class.PrimaryColor = inquisitor_class.PrimaryColor;
            spiritualist_class.SecondaryColor = inquisitor_class.SecondaryColor;
            spiritualist_class.RecommendedAttributes = new StatType[] { StatType.Wisdom };
            spiritualist_class.NotRecommendedAttributes = new StatType[0];
            spiritualist_class.EquipmentEntities = inquisitor_class.EquipmentEntities;
            spiritualist_class.MaleEquipmentEntities = inquisitor_class.MaleEquipmentEntities;
            spiritualist_class.FemaleEquipmentEntities = inquisitor_class.FemaleEquipmentEntities;
            spiritualist_class.ComponentsArray = inquisitor_class.ComponentsArray;
            spiritualist_class.StartingItems = new BlueprintItem[]
            {
                library.Get<BlueprintItemArmor>("afbe88d27a0eb544583e00fa78ffb2c7"), //studded leather
                library.Get<BlueprintItemWeapon>("1052a1f7128861942aa0c2ee6078531e"), //scythe
                library.Get<BlueprintItemWeapon>("511c97c1ea111444aa186b1a58496664"), //light crossbow
                library.Get<BlueprintItemEquipmentUsable>("807763fd874989e4d96eb2d8e234139e"), //shield scroll
                library.Get<BlueprintItemEquipmentUsable>("fe244c39bdd5cb64eae65af23c6759de"), //cause fear
                library.Get<BlueprintItemEquipmentUsable>("cd635d5720937b044a354dba17abad8d") //cure light wounds
            };

            createSpiritualistProgression();
            spiritualist_class.Progression = spiritualist_progression;
            
            Helpers.RegisterClass(spiritualist_class);

            createHagHaunted();
            createOnmyoji();
            createScourge();
            createFracturedMind();
            createExciter();
            spiritualist_class.Archetypes = new BlueprintArchetype[] {hag_haunted, onmyoji, scourge, fractured_mind, exciter};
            createEmotionalConduit();
        }


        static void createEmotionalConduit()
        {
            emotional_conduit = Helpers.CreateFeatureSelection("EmotionaConduitFeatureSelection",
                                                               "Emotional Conduit",
                                                               "You gain familiarity with a number of additional spells based on the emotional focus of your phantom. These spells are added to both your class spell list (if not already on that list) and your list of spells known; they are in addition to the normal number of spells known for your level.",
                                                               "",
                                                               null,
                                                               FeatureGroup.Feat,
                                                               Helpers.PrerequisiteClassLevel(spiritualist_class, 1)
                                                               );

            foreach (var kv in Phantom.emotion_conduit_spells_map)
            {
                string spell_string = "You gain the following spells:";
                foreach (var s in kv.Value)
                {
                    spell_string += " " + s.Name +  (s == kv.Value.Last() ? "." : ",");
                }
                var feature = Helpers.CreateFeature(kv.Key + "EmotionaConduitFeature",
                                                    emotional_conduit.Name + ": " + Phantom.phantom_name_map[kv.Key],
                                                    emotional_conduit.Description + "\n" + spell_string,
                                                    "",
                                                    Phantom.phantom_progressions[kv.Key].Icon,
                                                    FeatureGroup.Feat);

                for (int i = 0; i < kv.Value.Length; i++)
                {
                    feature.AddComponent(Helpers.CreateAddKnownSpell(kv.Value[i], spiritualist_class, i + 1));
                }

                if (Phantom.exciter_progressions.ContainsKey(kv.Key))
                {
                    feature.AddComponents(Helpers.PrerequisiteFeaturesFromList(Phantom.phantom_progressions[kv.Key], Phantom.exciter_progressions[kv.Key]));
                }
                else
                {
                    feature.AddComponent(Helpers.PrerequisiteFeature(Phantom.phantom_progressions[kv.Key]));
                }
                emotional_conduit.AllFeatures = emotional_conduit.AllFeatures.AddToArray(feature);
            }

            library.AddFeats(emotional_conduit);
        }


        static void createExciter()
        {
            exciter = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ExciterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Exciter");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Psychic magic draws upon the twin and sometimes opposed powers of thought and emotion, but an exciter cares little for rationality. The phantom that accompanies him fills him with unbridled exultation, as he lets feeling and passion rule and sharpen his mind and body into a glorious fusion.");
            });
            Helpers.SetField(exciter, "m_ParentClass", spiritualist_class);
            library.AddAsset(exciter, "");

            createMergedPhantom();
            createExciterSharedConciousness();
            createRapture();
            createOverwhelmingExcitement();


            var fast_movement = library.Get<BlueprintFeature>("d294a5dddd0120046aae7d4eb6cbc4fc");


            exciter.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, emotional_focus_selection, shared_consciousness, etheric_tether, masters_alignment),
                                                            Helpers.LevelEntry(4, spiritual_inference),
                                                            Helpers.LevelEntry(6, phantom_recall),
                                                            Helpers.LevelEntry(10, fused_consciousness),
                                                            Helpers.LevelEntry(12, greater_spiritual_inference),
                                                            Helpers.LevelEntry(14, spiritual_bond),
                                                            Helpers.LevelEntry(20, potent_phantom)};
            exciter.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, exciter_phantom_selection, rapture, fast_movement, exciter_shared_consciousness),
                                                         Helpers.LevelEntry(4, perfect_passion),
                                                         Helpers.LevelEntry(10, overwhelming_excitement, rapturous_rage),
                                                         Helpers.LevelEntry(12, greater_rapture),
                                                         Helpers.LevelEntry(14, rapturous_rage),
                                                         Helpers.LevelEntry(18, rapturous_rage),
                                                         Helpers.LevelEntry(20, exciter_potent_phantom),
                                                         };

            spiritualist_progression.UIGroups = spiritualist_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(rapture, rapturous_rage, greater_rapture, rapturous_rage, rapturous_rage));
            spiritualist_progression.UIGroups = spiritualist_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(fast_movement, perfect_passion, overwhelming_excitement));
            spiritualist_progression.UIGroups[1].Features.Add(exciter_shared_consciousness);
            spiritualist_progression.UIGroups[1].Features.Add(exciter_potent_phantom);
            spiritualist_progression.UIDeterminatorsGroup = spiritualist_progression.UIDeterminatorsGroup.AddToArray(exciter_phantom_selection);
        }


        static void createExciterSharedConciousness()
        {
            prevent_shared_conciousness_fact = Helpers.CreateBuff("PreventExciterSharedConciousnessFeature",
                                                                        "",
                                                                        "",
                                                                        "",
                                                                        null,
                                                                        null
                                                                      );
            prevent_shared_conciousness_fact.SetBuffFlags(BuffFlags.HiddenInUi);


            exciter_shared_consciousness = Helpers.CreateFeature("ExciterSharedConsciousnessFeature",
                                             shared_consciousness.Name,
                                             shared_consciousness.Description,
                                             "",
                                             shared_consciousness.Icon,
                                             FeatureGroup.None,
                                             Helpers.Create<RecalculateOnFactsChange>(a => a.CheckedFacts = new BlueprintUnitFact[] {prevent_shared_conciousness_fact} )
                                             );

            foreach (var kv in Phantom.exciter_progressions)
            {
                var exciter_skill_foci_feature = Helpers.CreateFeature("Exciter" + kv.Key + "SharedConsciousnessFeature",
                                                                       shared_consciousness.Name + " (" + Phantom.exciter_progressions[kv.Key].Name + ")",
                                                                       shared_consciousness.Description,
                                                                       "",
                                                                       shared_consciousness.Icon,
                                                                       FeatureGroup.None,
                                                                       Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.UntypedStackable, SpellDescriptor.MindAffecting),
                                                                       Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getSpiritualistArray(),
                                                                                                      progression: ContextRankProgression.Custom,
                                                                                                      customProgression: new (int, int)[] { (11, 4), (20, 8) })
                                                                      );
                exciter_skill_foci_feature.HideInCharacterSheetAndLevelUp = true;
                foreach (var sf in Phantom.phantom_skill_foci[kv.Key])
                {
                    exciter_skill_foci_feature.AddComponents(Common.createAddFeatureIfHasFactAndNotHasFact(Phantom.exciter_progressions[kv.Key], sf, sf));
                }

                exciter_shared_consciousness.AddComponent(Common.createAddFeatureIfHasFactAndNotHasFactDynamic(Phantom.exciter_progressions[kv.Key], prevent_shared_conciousness_fact,  exciter_skill_foci_feature));
            }
        }


        static void createOverwhelmingExcitement()
        {
            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var exhausted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");
            var str_con_effect_buff = Helpers.CreateBuff("OverwhelmingExcitementStrConEffectBuff",
                                                 "Overwhelming Excitement",
                                                 "At 10th level, an exciter can share the effects of his rapture with willing allies within 10 feet. He must expend 1 additional round of his rapture each round. The exciter’s allies share all effects of the rapture except the rage powers; each ally must end its turn within 10 feet of the exciter, or the effects of the rapture end for that ally and it becomes fatigued.",
                                                 "",
                                                 Helpers.GetIcon("1bc83efec9f8c4b42a46162d72cbf494"), //burst of glory
                                                 Common.createPrefabLink("53c86872d2be80b48afc218af1b204d7"), //from rage
                                                 Helpers.CreateAddContextStatBonus(StatType.Constitution, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus, multiplier: 2),
                                                 Helpers.CreateAddContextStatBonus(StatType.Strength, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus, multiplier: 2),
                                                 Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                                 Helpers.CreateAddStatBonus(StatType.AC, -2, ModifierDescriptor.UntypedStackable),
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList, ContextRankProgression.BonusValue, stepLevel: 2, type: AbilityRankType.StatBonus,
                                                                              featureList: new BlueprintFeature[] { greater_rapture }),
                                                 Helpers.Create<ForbidSpellCasting>(a => a.ForbidMagicItems = true)
                                               );

            var dex_cha_effect_buff = library.CopyAndAdd(str_con_effect_buff, "OverwhelmingExcitementDexChaEffectBuff", "");
            dex_cha_effect_buff.RemoveComponents<AddContextStatBonus>();
            dex_cha_effect_buff.AddComponents(Helpers.CreateAddContextStatBonus(StatType.Dexterity, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus, multiplier: 2),
                                              Helpers.CreateAddContextStatBonus(StatType.Charisma, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus, multiplier: 2),
                                              Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus)
                                             );

            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", "OverwhelmingExcitementAura", "");
            area.Fx = Common.createPrefabLink("cda35ba5c34a61b499f5858eabcedec7"); //abjuration aoe

            var in_area_effect = Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Helpers.Create<ContextConditionIsCaster>(), 
                                                                                             Helpers.CreateConditionHasFact(fatigued),
                                                                                             Helpers.CreateConditionHasFact(exhausted),
                                                                                             Helpers.Create<ContextConditionIsEnemy>()
                                                                                             ),
                                                           null,
                                                           Helpers.CreateConditional(Common.createContextConditionCasterHasFact(rapture_str_con_buff),
                                                                                     Common.createContextActionApplyBuff(str_con_effect_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false),
                                                                                     Helpers.CreateConditional(Common.createContextConditionCasterHasFact(rapture_dex_cha_buff),
                                                                                                               Common.createContextActionApplyBuff(dex_cha_effect_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)
                                                                                                               )
                                                                                    )
                                                          );
            area.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAreaEffectRunAction(unitEnter: new GameAction[]{in_area_effect },
                                                  round: new GameAction[]{in_area_effect },
                                                  unitExit: new GameAction[]{Helpers.CreateConditional(new Condition[]{Common.createContextConditionHasBuffFromCaster(str_con_effect_buff) },
                                                                                                       new GameAction[]{ Common.createContextActionApplyBuff(fatigued, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false),
                                                                                                                         Common.createContextActionRemoveBuffFromCaster(str_con_effect_buff)
                                                                                                                       }
                                                                                                       ),
                                                                             Helpers.CreateConditional(new Condition[]{Common.createContextConditionHasBuffFromCaster(dex_cha_effect_buff) },
                                                                                                       new GameAction[]{ Common.createContextActionApplyBuff(fatigued, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false),
                                                                                                                         Common.createContextActionRemoveBuffFromCaster(dex_cha_effect_buff)
                                                                                                                       }
                                                                                                       ),                                                                             
                                                                            }
                                                 )
            };

            var overwhelming_excitement_buff = Helpers.CreateBuff("OverwhelmingExcitementBuff",
                                                                   str_con_effect_buff.Name,
                                                                   str_con_effect_buff.Description,
                                                                   "",
                                                                   str_con_effect_buff.Icon,
                                                                   null,
                                                                   Common.createAddAreaEffect(area),
                                                                   Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(prevent_shared_conciousness_fact, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true))
                                                                   );

            var toggle = Common.buffToToggle(overwhelming_excitement_buff, CommandType.Free, false,
                                             rapture_resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound),
                                             Helpers.Create<NewMechanics.RestrictionHasFacts>(r =>  r.features = new BlueprintUnitFact[] { rapture_str_con_buff, rapture_dex_cha_buff })
                                             );
            
            overwhelming_excitement = Common.ActivatableAbilityToFeature(toggle, false);

            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(rapture_str_con_buff,
                                                                          Helpers.CreateConditional(Common.createContextConditionHasFact(overwhelming_excitement),
                                                                                                    null,
                                                                                                    Common.createContextActionApplyBuff(prevent_shared_conciousness_fact, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true)
                                                                                                    )
                                                                          );
            rapture_share_str_con_buff = str_con_effect_buff;
            rapture_share_dex_cha_buff = dex_cha_effect_buff;
        }

        static void createRapture()
        {
            rapture_resource = Helpers.CreateAbilityResource("RaptureResource", "", "", "", null);
            rapture_resource.SetIncreasedByLevel(0, 2, getSpiritualistArray());
            rapture_resource.SetIncreasedByStat(2, StatType.Constitution);

            perfect_passion = Helpers.CreateFeature("PerfectPassionFeature",
                                                    "Perfect Passion",
                                                    "At 4th level, an exciter can cast psychic spells even while in his rapture. He can cast these spells defensively and attempt concentration checks for these spells without impairment, despite being in a rapture.",
                                                    "",
                                                    Helpers.GetIcon("ce7dad2b25acf85429b6c9550787b2d9"),
                                                    FeatureGroup.None
                                                    );


            var rapture_casting_allowed_buff = Helpers.CreateBuff("RaptureCastingAllowedBuff",
                                        "",
                                        "",
                                        "",
                                        null,
                                        null);
            rapture_casting_allowed_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_allow_blood_casting = Common.createContextActionApplyBuff(rapture_casting_allowed_buff, Helpers.CreateContextDuration(1), is_child: true, dispellable: false);

            var cast_only_on_self = Common.createContextActionApplyBuff(SharedSpells.can_only_target_self_buff, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true);
            var rapture_casting_buff = Helpers.CreateBuff("RaptureCastingBuff",
                                                        "Rapture Casting",
                                                        "Uupon entering a rapture, exiter can apply the effects of a single spiritualist spell he knows to himself. The spell must have a range of touch or personal, and it must be a 1st- or 2nd-level spell. For every 3 spiritualist levels he has beyond 12th, the maximum spell level of this spell increases by 1.",
                                                        "",
                                                        library.Get<BlueprintAbility>("92681f181b507b34ea87018e8f7a528a").Icon,
                                                        null,
                                                        Helpers.Create<TurnActionMechanics.FreeTouchOrPersonalSpellUseFromSpellbook>(f =>
                                                        {
                                                            f.allowed_spellbook = spiritualist_class.Spellbook;
                                                            f.max_spell_level = Helpers.CreateContextValue(AbilityRankType.Default);
                                                            f.control_buff = rapture_casting_allowed_buff;
                                                        }
                                                        ),
                                                        Helpers.Create<TurnActionMechanics.FreeTouchOrPersonalSpellUseFromSpellbook>(f =>
                                                        {
                                                            f.allowed_spellbook = fractured_mind_spellbook;
                                                            f.max_spell_level = Helpers.CreateContextValue(AbilityRankType.Default);
                                                            f.control_buff = rapture_casting_allowed_buff;
                                                        }                                                                           
                                                        ),
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                        classes: getSpiritualistArray(),
                                                                                        progression: ContextRankProgression.Custom,
                                                                                        customProgression: new (int, int)[] { (14, 2), (17, 3), (20, 4) }
                                                                                        ),
                                                        Helpers.CreateAddFactContextActions(cast_only_on_self)
                                                        );

            var rapture_casting_ability = Helpers.CreateActivatableAbility("RaptureCastingToggleAbility",
                                                                         rapture_casting_buff.Name,
                                                                         rapture_casting_buff.Description,
                                                                         "",
                                                                         rapture_casting_buff.Icon,
                                                                         rapture_casting_buff,
                                                                         AbilityActivationType.Immediately,
                                                                         CommandType.Free,
                                                                         null,
                                                                         Helpers.Create<RestrictionHasFact>(r => r.Feature = rapture_casting_allowed_buff));
            rapture_casting_ability.DeactivateImmediately = true;

            greater_rapture = Helpers.CreateFeature("GreaterRaptureFeature",
                                        "Greater Rapture",
                                        "At 12th level, an exciter increases the morale bonus his rapture grants to each applicable ability score by 2 and the morale bonus he gains on Will saves by 1.\n"
                                        + "In addition, upon entering a rapture, he can apply the effects of a single spiritualist spell he knows to himself. The spell must have a range of touch or personal, and it must be a 1st- or 2nd-level spell. For every 3 spiritualist levels he has beyond 12th, the maximum spell level of this spell increases by 1.",
                                        "",
                                        Helpers.GetIcon("df6a2cce8e3a9bd4592fb1968b83f730"), //rage
                                        FeatureGroup.None,
                                        Helpers.CreateAddFact(rapture_casting_ability)
                                        );

            
            var rapture_str_con_ability = library.CopyAndAdd<BlueprintActivatableAbility>("df6a2cce8e3a9bd4592fb1968b83f730", "RaptureStrConToggleAbility", "");
            rapture_str_con_ability.SetNameDescription("Rapture (Strength and Constitution)",
                                               "An exciter gains the ability to enter an ecstatic state in which he’s consumed and overwhelmed by his passions and driven into a fighting fury. This functions similarly to a barbarian’s rage, treating his spiritualist level as his barbarian level, though he doesn’t qualify for feats or other elements that require rage or bloodrage. When entering a rapture, the exciter loses all other benefits from having his phantom confined in his consciousness (such as the Skill Focus feats and bonus against mind-affecting effects). While in rapture Exciter receives +4 morale bonus to Strength, Constitution and +2 morale bonus on Will saves instead of usual barbarian rage bonuses, but he can choose to exchange the normal +4 morale bonus to his Strength and Constitution scores for a +4 morale bonus to his Dexterity and Charisma."
                                              );
            rapture_str_con_ability.Group = ActivatableAbilityGroupExtension.Rage.ToActivatableAbilityGroup();
            rapture_str_con_buff = library.CopyAndAdd<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613", "RaptureStrConBuff", "");
            rapture_str_con_buff.RemoveComponent(rapture_str_con_buff.GetComponent<Kingmaker.UnitLogic.FactLogic.ForbidSpellCasting>());
            rapture_str_con_buff.AddComponent(Helpers.Create<NewMechanics.ForbidSpellCastingUnlessHasFacts>(f => { f.ForbidMagicItems = true; f.allowed_facts = new BlueprintUnitFact[] {perfect_passion}; }));
            rapture_str_con_buff.SetNameDescription(rapture_str_con_ability);
            rapture_str_con_ability.Buff.RemoveComponents<NewMechanics.FeatureReplacement>();
            rapture_str_con_ability.Buff = rapture_str_con_buff;
            rapture_str_con_ability.ReplaceComponent<ActivatableAbilityResourceLogic>(a => a.RequiredResource = rapture_resource);
            rapture_str_con_buff.ReplaceComponent<AddFactContextActions>(a =>
            {
                a.Activated = Helpers.CreateActionList(a.Activated.Actions.ToList().ToArray().AddToArray(apply_allow_blood_casting));
                a.NewRound = Helpers.CreateActionList(a.NewRound.Actions.ToList().ToArray());
                a.Deactivated = Helpers.CreateActionList(a.Deactivated.Actions.ToList().ToArray());
            });
            rapture_str_con_buff.RemoveComponents<TemporaryHitPointsPerLevel>();
            rapture_str_con_buff.RemoveComponents<ContextRankConfig>();
            rapture_str_con_buff.RemoveComponents<AddContextStatBonus>();
            rapture_str_con_buff.RemoveComponents<WeaponAttackTypeDamageBonus>();
            rapture_str_con_buff.RemoveComponents<SpellDescriptorComponent>();
            rapture_str_con_buff.RemoveComponents<WeaponGroupDamageBonus>();
            rapture_str_con_buff.RemoveComponents<AttackTypeAttackBonus>();
            rapture_str_con_buff.AddComponents(Helpers.CreateAddContextStatBonus(StatType.Constitution, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus, multiplier: 2),
                                               Helpers.CreateAddContextStatBonus(StatType.Strength, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus, multiplier: 2),
                                               Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                               Helpers.CreateAddStatBonus(StatType.AC, -2, ModifierDescriptor.UntypedStackable),
                                               Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureList, ContextRankProgression.BonusValue, stepLevel: 2, type: AbilityRankType.StatBonus,
                                                                              featureList: new BlueprintFeature[] { greater_rapture })
                                               );

            var barbarian = library.Get<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");
            rapturous_rage = library.CopyAndAdd<BlueprintFeatureSelection>("28710502f46848d48b3f0d6132817c4e", "RapturousRageFeatureSelection", "");
            rapturous_rage.SetNameDescription("Rapturous Rage",
                                              "At 10th level and every 4 spiritualist levels thereafter, an exciter can select one rage power for which he qualifies, treating his spiritualist level as his barbarian level for all purposes relating to that particular rage power (he still doesn’t qualify for the Extra Rage Power feat or any similar abilities).");

            ClassToProgression.addClassToBuff(spiritualist_class, new BlueprintArchetype[] {exciter }, rapture_str_con_buff, barbarian);
            foreach (var f in rapturous_rage.AllFeatures)
            {
                ClassToProgression.addClassToFeat(spiritualist_class, new BlueprintArchetype[] {exciter}, ClassToProgression.DomainSpellsType.NoSpells, f, barbarian);
            }

            if (test_mode)
            {
                // allow to use rage out of combat for debug purposes
                rapture_str_con_ability.IsOnByDefault = false;
                rapture_str_con_ability.DeactivateIfCombatEnded = false;
                rapture_str_con_ability.IsOnByDefault = false;
                rapture_str_con_ability.DeactivateIfCombatEnded = false;
            }

            rapture_dex_cha_buff = library.CopyAndAdd(rapture_str_con_buff, "RaptureDexChaBuff", "");
            rapture_dex_cha_buff.SetIcon(Helpers.GetIcon("3553bda4d6dfe6344ad89b25f7be939a")); //transmutation physical ench dex
            rapture_dex_cha_buff.SetName("Rapture (Dexterity and Charisma)");
            rapture_dex_cha_buff.RemoveComponents<AddContextStatBonus>();
            rapture_dex_cha_buff.AddComponents(Helpers.CreateAddContextStatBonus(StatType.Charisma, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus, multiplier: 2),
                                   Helpers.CreateAddContextStatBonus(StatType.Dexterity, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus, multiplier: 2),
                                   Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus, multiplier: 2)
                                   );
            var rapture_dex_cha_ability = library.CopyAndAdd(rapture_str_con_ability, "RaptureDexChaAbility", "");
            rapture_dex_cha_ability.SetName(rapture_dex_cha_buff.Name);
            rapture_dex_cha_ability.SetIcon(rapture_dex_cha_buff.Icon);
            rapture_dex_cha_ability.Buff = rapture_dex_cha_buff;

            rapture = Helpers.CreateFeature("RaptureFeature",
                                            "Rapture",
                                            rapture_str_con_ability.Description,
                                            "",
                                            rapture_str_con_ability.Icon,
                                            FeatureGroup.None,
                                            rapture_resource.CreateAddAbilityResource(),
                                            Helpers.CreateAddFacts(rapture_str_con_ability, rapture_dex_cha_ability),
                                            Common.createClassLevelsForPrerequisites(barbarian, spiritualist_class)
                                            );
        }


        static void createMergedPhantom()
        {
            exciter_phantom_selection = Helpers.CreateFeatureSelection("ExciterPhantomFeatureSelection",
                                                      "Merged Phantom",
                                                      "An exciter internalizes his phantom and merges it completely within his mind. He cannot fully manifest his phantom outside of his own body in incorporeal or ectoplasmic form. Emotional focus abilities that affect or require a manifested phantom are lost, except for phantom ability gained at first level and aura gained by the phantom at 7th level; if the phantom gains an aura at 7th level, this aura functions despite the phantom not being manifested, and is centered on the exciter. If phantom ability augments only a phantom's slam attack it augments all exiter's melee attacks instead.",
                                                      "",
                                                      null,
                                                      FeatureGroup.AnimalCompanion
                                                      );

            foreach (var kv in Phantom.exciter_progressions)
            {
                exciter_phantom_selection.AllFeatures = exciter_phantom_selection.AllFeatures.AddToArray(kv.Value);
            }


            exciter_potent_phantom = Helpers.CreateFeatureSelection("ExciterPotentPhantomFeatureSelection",
                                                "Potent Phantom",
                                                "At 20th level, the spiritualist’s phantom grows ever more complex and sophisticated in its manifestation. The phantom gains a second emotional focus. This does not change the phantom’s saving throws, but the phantom otherwise grants all the skills and powers of the focus.",
                                                "",
                                                Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"),
                                                FeatureGroup.None);

            foreach (var kv in Phantom.potent_exciter_progressions)
            {
                exciter_potent_phantom.AllFeatures = exciter_potent_phantom.AllFeatures.AddToArray(kv.Value);
            }
        }


        static void createOnmyoji()
        {
            onmyoji = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "OnmyojiArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Onmyoji");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Though most spiritualists are chosen by their phantoms, others deliberately call phantoms to them through years of careful preparation and study in obscure divine traditions.\n"
                                                                                       + "These spiritualists, known as onmyoji, form close bonds with their phantoms, as any other spiritualist does, but see the phantoms as partners and tools in their work. Onmyoji serve as emissaries between the mundane world and the spiritual one, either working to ensure that troubles in the spiritual world do not spill over into the world of mortals, or stirring up spiritual trouble in order to achieve their ends among the living.");
            });
            Helpers.SetField(onmyoji, "m_ParentClass", spiritualist_class);
            library.AddAsset(onmyoji, "");

            createOnmyojiSpellcasting();
            createDivineTeachings();


            onmyoji.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, spiritualist_spellcasting, shared_consciousness),
                                                            Helpers.LevelEntry(4, spiritual_inference),
                                                            Helpers.LevelEntry(10, fused_consciousness),
                                                            Helpers.LevelEntry(12, greater_spiritual_inference),};
            onmyoji.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, onmyoji_spellcasting),
                                                         Helpers.LevelEntry(4, divine_teachings),
                                                         Helpers.LevelEntry(7, divine_teachings),
                                                         Helpers.LevelEntry(10, divine_teachings),
                                                         Helpers.LevelEntry(13, divine_teachings),
                                                         Helpers.LevelEntry(16, divine_teachings),
                                                         Helpers.LevelEntry(19, divine_teachings),
                                                         };

            onmyoji.ReplaceSpellbook = onmyoji_spellbook;
            onmyoji.ChangeCasterType = true;
            onmyoji.IsDivineCaster = true;
            spiritualist_progression.UIDeterminatorsGroup = spiritualist_progression.UIDeterminatorsGroup.AddToArray(onmyoji_spellcasting);
            Common.addMTDivineSpellbookProgression(spiritualist_class, onmyoji_spellbook, "MysticTheurgeOnmyojiProgression", 
                                                   Common.createPrerequisiteArchetypeLevel(onmyoji, 1),
                                                   Common.createPrerequisiteClassSpellLevel(spiritualist_class, 2)
                                                   );
        }


        static void createScourge()
        {
            scourge = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ScourgeArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Scourge");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Scourges are students of pain and have a rare connection to tormented and wracked spirits. Scourges seek to share their phantoms’ miseries with all around them, using the spirit’s pain as a weapon. A scourge’s phantom is a broken and wretched creature, and the torments it suffered in life are reflected in its ghostly or ectoplasmic appearance as wounds, scars, grotesque malformations, and tattered garments.");
            });
            Helpers.SetField(scourge, "m_ParentClass", spiritualist_class);
            library.AddAsset(scourge, "");

            createSpellScourge();
            createEctoplasmicSwarm();


            scourge.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(4, spiritual_inference),
                                                       Helpers.LevelEntry(12, greater_spiritual_inference),};
            scourge.AddFeatures = new LevelEntry[] {     
                                                         Helpers.LevelEntry(4, spell_scourge),
                                                         Helpers.LevelEntry(12, ectoplasmic_swarm),
                                                   };

            spiritualist_progression.UIGroups = spiritualist_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(spell_scourge, ectoplasmic_swarm));

            //fix phantoms
            foreach (var f in emotional_focus_selection.AllFeatures)
            {
                f.AddComponent(Common.prerequisiteNoArchetype(scourge));
            }

            foreach (var kv in Phantom.pain_phantom_progressions)
            {
                kv.Value.AddComponent(Common.createPrerequisiteArchetypeLevel(scourge, 1));
                kv.Value.SetDescription(kv.Value.Description + "\n" + Phantom.endure_torment.Name + ": " + Phantom.endure_torment.Description + " This ability replaces devotion.");
                emotional_focus_selection.AllFeatures = emotional_focus_selection.AllFeatures.AddToArray(kv.Value);
            }
        }


        static void createEctoplasmicSwarm()
        {
            var spider_swarm_damage_effect_immunity = library.Get<BlueprintBuff>("1c63c2b0ea1f44940a63211fef462b98");
            var nauseted = library.Get<BlueprintBuff>("956331dba5125ef48afe41875a00ca0e");
            var apply_nauseted = Common.createContextActionApplyBuff(nauseted, Helpers.CreateContextDuration(1));
            var dmg_phantom = Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                             Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.D6, 1, 0)),
                                                                             Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.D8, 1, 0)),
                                                                             Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.D10, 1, 0)),
                                                                             Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.D6, 2, 0)),
                                                                             Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.D8, 2, 0))
                                                                             );

            var dmg_phantom_large = Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.Default),
                                                                 Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.D8, 1, 0)),
                                                                 Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.D6, 2, 0)),
                                                                 Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.D8, 2, 0)),
                                                                 Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.D6, 3, 0)),
                                                                 Helpers.CreateActionDealDirectDamage(Helpers.CreateContextDiceValue(DiceType.D8, 3, 0))
                                                                 );

            var dmg = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(Phantom.slam_damage_large),
                                                dmg_phantom_large,
                                                dmg_phantom);

            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                type: AbilityRankType.Default,
                                                                                progression: ContextRankProgression.StartPlusDivStep,
                                                                                startLevel: 1,
                                                                                stepLevel: 3,
                                                                                classes: Phantom.getPhantomArray());

            var activated_actions = Helpers.CreateActionList(Helpers.Create<ContextActionSwarmTarget>(),
                                                             Common.createContextActionApplyBuff(spider_swarm_damage_effect_immunity, Helpers.CreateContextDuration(1)),
                                                             dmg,
                                                             Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_nauseted))
                                                             );


            var in_swarm_buff = Helpers.CreateBuff("EctoplasmicSwarmDamageEffectBuff",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    Common.createPrefabLink("4b31ab00d34d50845aa087a2a8bed63d"),
                                                    Helpers.CreateAddFactContextActions(activated: activated_actions.Actions,
                                                                                        deactivated: new GameAction[] { Helpers.Create<ContextActionSwarmTarget>(c => c.Remove = true) }
                                                                                        ),
                                                    context_rank_config
                                                   );
            in_swarm_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var swarm_buff = Common.createBuffAreaEffect(in_swarm_buff, 5.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()));
            swarm_buff.SetBuffFlags(BuffFlags.HiddenInUi);

          
            var swarm_attack = Helpers.CreateConditional(new Condition[] { Common.createContextConditionHasBuffFromCaster(spider_swarm_damage_effect_immunity) },
                                                                     null,
                                                                     new GameAction[]{dmg,
                                                                                      Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_nauseted))
                                                                                     }
                                                        );

            var attack_in_swarm = Helpers.Create<ContextActionSwarmAttack>(c => c.AttackActions = Helpers.CreateActionList(Helpers.Create<ContextActionOnSwarmTargets>(a => a.Actions = Helpers.CreateActionList(swarm_attack))));
            var swarm_attack_conditional = Helpers.CreateConditional(new Condition[] { Helpers.Create<ContextSwarmHasEnemiesInInnerCircle>() },
                                                                     new GameAction[] { attack_in_swarm,
                                                                                        Helpers.Create<ContextActionPlaySound>(c => c.SoundName = "SwarmMandragora_Attack_Voice")
                                                                                      }
                                                                     );

            swarm_buff.AddComponents(Helpers.CreateAddFactContextActions(newRound: swarm_attack_conditional),
                                    context_rank_config);

            var swarm_feature = Helpers.CreateFeature("EctoplasmicSwarmDamageFeature",
                                                      "",
                                                      "",
                                                      "",
                                                      null,
                                                      FeatureGroup.None,
                                                      Common.createAuraFeatureComponent(swarm_buff),
                                                      Common.createContextCalculateAbilityParamsBasedOnClass(Phantom.phantom_class, StatType.Constitution)
                                                      );
            swarm_feature.HideInUI = true;
            swarm_feature.HideInCharacterSheetAndLevelUp = true;

            var polymorph_component = library.Get<BlueprintBuff>("6ba1229b016317041b17f75e7b0fc686").GetComponent<Polymorph>().CreateCopy();
            polymorph_component.DexterityBonus = 8;
            polymorph_component.StrengthBonus = -10;
            polymorph_component.ConstitutionBonus = -2;
            polymorph_component.Size = Size.Diminutive;
            polymorph_component.Prefab = Common.createUnitViewLink("99a0fbddd76b4b147b1831d315a581cb");//mandragora swarm
            polymorph_component.MainHand = null;
            polymorph_component.OffHand = null;
            polymorph_component.NaturalArmor = 0;
            polymorph_component.AdditionalLimbs = new BlueprintItemWeapon[0];
            polymorph_component.SecondaryAdditionalLimbs = new BlueprintItemWeapon[0];
            polymorph_component.Facts = new BlueprintUnitFact[]
            {
                swarm_feature,
                library.Get<BlueprintFeature>("2e3e840ab458ce04c92064489f87ecc2") //diminutive swarm
            };

            var ectoplasmic_swarm_buff = Helpers.CreateBuff("EctoplasmicSwarmBuff",
                                                            "Ectoplasmic Swarm",
                                                            "At 12th level, as a standard action, a scourge with a phantom manifested in ectoplasmic form can command it to break apart in a gruesome display of gore and agony, transforming it into a swarm of Diminutive ectoplasmic organs and viscera. The phantom gains the swarm subtype, dealing its unmodified slam damage die as damage for its swarm attack. Its distraction DC is equal to 10 + 1/2 the phantom’s Hit Dice + its Constitution modifier. In this form, the phantom is too diffuse to use any of its abilities from emotional focus (even passive abilities).",
                                                            "",
                                                            LoadIcons.Image2Sprite.Create(@"AbilityIcons/EctoplasmicSwarm.png"),
                                                            null,
                                                            polymorph_component,
                                                            Helpers.CreateSpellDescriptor(SpellDescriptor.Polymorph),
                                                            Helpers.Create<AddCondition>(a => a.Condition = UnitCondition.CanNotAttack),
                                                            Helpers.Create<SpellFailureMechanics.UnableToUseAbilities>()
                                                            );
            ectoplasmic_swarm_buff.SetBuffFlags(BuffFlags.RemoveOnRest);

            var apply_polymorph = Common.createContextActionApplyBuff(ectoplasmic_swarm_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var precast_action = Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList(Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Polymorph)));
            var ectoplasmic_swarm_ability = Helpers.CreateAbility("EctoplasmicSwarmAbility",
                                                                  ectoplasmic_swarm_buff.Name,
                                                                  ectoplasmic_swarm_buff.Description,
                                                                  "",
                                                                  ectoplasmic_swarm_buff.Icon,
                                                                  AbilityType.Supernatural,
                                                                  CommandType.Standard,
                                                                  AbilityRange.Personal,
                                                                  "",
                                                                  "",
                                                                  Helpers.CreateRunActions(Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList(apply_polymorph))),
                                                                  Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(precast_action)),
                                                                  Helpers.Create<NewMechanics.AbilityCasterCompanionDead>(a => a.not = true),
                                                                  Helpers.Create<CompanionMechanics.AbilityCasterCompanionHasFact>(a => { a.fact = ectoplasmic_swarm_buff; a.not = true; })
                                                                  );
            ectoplasmic_swarm_ability.setMiscAbilityParametersSelfOnly();

            var ectoplasmic_swarm_ability_remove = Helpers.CreateAbility("EctoplasmicSwarmRemoveAbility",
                                                      "Deactivate " + ectoplasmic_swarm_buff.Name,
                                                      ectoplasmic_swarm_buff.Description,
                                                      "",
                                                      ectoplasmic_swarm_buff.Icon,
                                                      AbilityType.Supernatural,
                                                      CommandType.Standard,
                                                      AbilityRange.Personal,
                                                      "",
                                                      "",
                                                      Helpers.CreateRunActions(Helpers.Create<ContextActionsOnPet>(c => c.Actions = Helpers.CreateActionList(Common.createContextActionRemoveBuff(ectoplasmic_swarm_buff)))),
                                                      Helpers.Create<CompanionMechanics.AbilityCasterCompanionHasFact>(a => a.fact = ectoplasmic_swarm_buff)
                                                      );

            var wrapper = Common.createVariantWrapper("EctoplasmicswarmAbilityBase", "", ectoplasmic_swarm_ability, ectoplasmic_swarm_ability_remove);
            ectoplasmic_swarm = Common.AbilityToFeature(wrapper, false);
        }


        static void createSpellScourge()
        {
            var spell_scourge_attack_buff = Helpers.CreateBuff("SpellScourgePhantomAttackBuff",
                                                              "Spell Scourge",
                                                              "At 4th level, when a scourge’s phantom damages a creature, it causes severe pain, requiring that creature to succeed at a concentration check (DC = 15 + spell level) to use spells, spell-like abilities, and other abilities that require concentration for 1 round.\n"
                                                              + "The phantom gains a +2 bonus on attack and damage rolls when making attacks of opportunity. Any creature threatened by the scourge’s phantom takes a –5 penalty on concentration checks.",
                                                              "",
                                                              NewSpells.fleshworm_infestation.Icon,
                                                              null,
                                                              Helpers.Create<AddCondition>(a => a.Condition = UnitCondition.SpellCastingIsDifficult)
                                                              );

            var spell_scourge_aura_buff = Helpers.CreateBuff("SpellScourgeAuraEffectBuff",
                                                  "Spell Scourge Concentration Penalty",
                                                  spell_scourge_attack_buff.Description,
                                                  "",
                                                  spell_scourge_attack_buff.Icon,
                                                  null,
                                                  Helpers.Create<ConcentrationBonus>(c => c.Value = -5)
                                                  );

            var spell_scourge_phantom = Common.createAuraEffectFeature(spell_scourge_attack_buff.Name, spell_scourge_attack_buff.Description, spell_scourge_attack_buff.Icon,
                                                                       spell_scourge_aura_buff, 10.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>())
                                                                       );
            spell_scourge_phantom.AddComponents(Common.createAttackBonusOnAttacksOfOpportunity(2, ModifierDescriptor.UntypedStackable),
                                                Helpers.Create<NewMechanics.AttackOfOpportunityDamgeBonus>(a => { a.value = 2; a.descriptor = ModifierDescriptor.UntypedStackable; }),
                                                Helpers.Create<AddOutgoingDamageTrigger>(a =>
                                                {
                                                    a.NotZeroDamage = true;
                                                    a.ApplyToAreaEffectDamage = true;
                                                    a.Actions = Helpers.CreateActionList(Common.createContextActionApplyBuff(spell_scourge_attack_buff, Helpers.CreateContextDuration(1), dispellable: false));
                                                }
                                                )
                                               );
            spell_scourge = Common.createAddFeatToAnimalCompanion(spell_scourge_phantom, "");
        }


        static void createFracturedMind()
        {
            fractured_mind = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "FracturedMindArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Fractured Mind");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Most spiritualists harbor the spirits of the deceased in their psyches, but a small number of them—known as fractured minds—draw their powers instead from a fraction of their own souls that resonates with extremely powerful emotions. These spiritualists’ phantoms are not spiritual allies, but rather extensions of the fractured minds’ own inner thoughts and emotions.");
            });
            Helpers.SetField(fractured_mind, "m_ParentClass", spiritualist_class);
            library.AddAsset(fractured_mind, "");

            createFracturedMindSpellcasting();


            fractured_mind.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, spiritualist_spellcasting)};
            fractured_mind.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, charisma_spellcasting) };
            fractured_mind.ReplaceSpellbook = fractured_mind_spellbook;

            spiritualist_progression.UIDeterminatorsGroup = spiritualist_progression.UIDeterminatorsGroup.AddToArray(charisma_spellcasting);
        }

        static void createOnmyojiSpellcasting()
        {
            onmyoji_spellbook = library.CopyAndAdd(spiritualist_class.Spellbook, "OnmyojiSpellbook", "");
            onmyoji_spellbook.IsArcane = false;
            onmyoji_spellbook.Name = onmyoji.LocalizedName;
            onmyoji_spellbook.CantripsType = CantripsType.Orisions;
            onmyoji_spellbook.RemoveComponents<SpellbookMechanics.PsychicSpellbook>();

            onmyoji_spellcasting = Helpers.CreateFeature("OnmyojiSpellcastingFeature",
                                                      "Divine Spellcasting",
                                                      "An onmyoji’s spellcasting ability comes from divine rather than psychic power. As a divine caster, the onmyoji’s spells use verbal components instead of thought components, and somatic components instead of emotional components, and she uses an ofuda as a divine focus. Ofudas are scrolls with holy writings written on parchment, cloth, or wood (having the same cost as a wooden holy symbol) or metal (having the same cost as a silver holy symbol).",
                                                      "",
                                                      Helpers.GetIcon("90e59f4a4ada87243b7b3535a06d0638"), //bless
                                                      FeatureGroup.None,
                                                      Helpers.Create<SpellbookMechanics.AddUndercastSpells>(p => p.spellbook = onmyoji_spellbook)
                                                      );
          
            onmyoji_spellcasting.AddComponents(Common.createCantrips(spiritualist_class, StatType.Wisdom, onmyoji_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
            onmyoji_spellcasting.AddComponent(Helpers.CreateAddFacts(onmyoji_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
        }


        static void createFracturedMindSpellcasting()
        {
            fractured_mind_spellbook = library.CopyAndAdd(spiritualist_class.Spellbook, "FracturedMindSpellbook", "");
            fractured_mind_spellbook.CastingAttribute = StatType.Charisma;
            fractured_mind_spellbook.Name = fractured_mind.LocalizedName;

            charisma_spellcasting.AddComponent(Helpers.Create<SpellbookMechanics.AddUndercastSpells>(p => p.spellbook = fractured_mind_spellbook));

            charisma_spellcasting.AddComponents(Common.createCantrips(spiritualist_class, StatType.Charisma, fractured_mind_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
            charisma_spellcasting.AddComponent(Helpers.CreateAddFacts(fractured_mind_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
        }





        static void createDivineTeachings()
        {
            var icon = library.Get<BlueprintAbility>("f2115ac1148256b4ba20788f7e966830").Icon; //restoration
            divine_teachings = Helpers.CreateFeatureSelection("DivineTeachingsFeatureSelection",
                                                            "Divine Teachings",
                                                            "An onmyoji gains the ability to call upon her phantom to help her cast spells she normally couldn’t. At 4th level and every 3 levels thereafter, an onmyoji can choose a single spell from the cleric spell list with a spell level she is currently able to cast, and add that spell to her list of spells known, at the same spell level as it appears on the cleric spell list.",
                                                            "",
                                                            icon,
                                                            FeatureGroup.None);

            var cleric_spell_list = Common.combineSpellLists("DivineTeachingsSpellList", library.Get<BlueprintSpellList>("8443ce803d2d31347897a3d85cc32f53"));
            Common.excludeSpellsFromList(cleric_spell_list, spiritualist_class.Spellbook.SpellList);
            for (int i = 1; i <= 6; i++)
            {
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956",  $"DivineTeachings{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = spiritualist_class;
                learn_spell.SpellList = cleric_spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = cleric_spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = spiritualist_class; });
                learn_spell.AddComponent(Common.createPrerequisiteClassSpellLevel(spiritualist_class, i));
                learn_spell.SetName(Helpers.CreateString( $"DivineTeachingsParametrizedFeature{i}.Name", "Divine Teachings " + $"(level {i})"));
                learn_spell.SetDescription(divine_teachings.Description);
                learn_spell.SetIcon(divine_teachings.Icon);

                divine_teachings.AllFeatures = divine_teachings.AllFeatures.AddToArray(learn_spell);
            }
        }



        static void createHagHaunted()
        {
            hag_haunted = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "HagHauntedArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Hag-Haunted");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Hags—those of flesh and blood, at any rate—die like any other mortals, and their souls normally depart to the Outer Planes for judgment. A hag who dies with a curse on her breath is often anchored to the Ethereal Plane by the power of her hatred —similar to vile and angry mortal souls—and some even claw their way back to the living world through the souls of those they despised or ruined… or those unfortunate souls they birthed. Hag-haunted spiritualists are tethered to these spiteful spirits, anchoring them once again in the world of the living. While this partnership imparts powerful magic, they run the constant risk of serving as little more than mounts for their overwhelming phantoms.\n" 
                                                                                       + "Hag-haunted spiritualists are rarely the masters in their relationship with their phantoms, and the only tool at their disposal to control their wicked minion is to dismiss them back to the Ethereal Plane. In the best scenarios, the relationship is one of mutual competition and constant bargaining, but just as often the hag phantom dominates and abuses her spiritualist.");
            });
            Helpers.SetField(hag_haunted, "m_ParentClass", spiritualist_class);
            library.AddAsset(hag_haunted, "");

            createHagSpellcasting();
            createHagPhantom();
            createHagSharedConsciousnessAndFusedConsciousness();
            createHagDeathCurse();

            hag_haunted.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, spiritualist_spellcasting, shared_consciousness, masters_alignment),
                                                            Helpers.LevelEntry(4, spiritual_inference),
                                                            Helpers.LevelEntry(10, fused_consciousness),
                                                            Helpers.LevelEntry(12, greater_spiritual_inference),};
            hag_haunted.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, hag_spell_casting, hag_phantom, hag_shared_consciousness),
                                                         Helpers.LevelEntry(7, hag_curse7),
                                                         Helpers.LevelEntry(10, hag_fused_consciousness),
                                                         Helpers.LevelEntry(12, hag_fused_consciousness12),
                                                         Helpers.LevelEntry(13, hag_curse13),
                                                         };

            hag_haunted.ReplaceSpellbook = hag_haunted_spellbook;
            hag_haunted.ChangeCasterType = true;
            hag_haunted.IsArcaneCaster = true;
            spiritualist_progression.UIDeterminatorsGroup = spiritualist_progression.UIDeterminatorsGroup.AddToArray(hag_spell_casting, hag_phantom);
            spiritualist_progression.UIGroups = spiritualist_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(hag_shared_consciousness, hag_curse7, hag_fused_consciousness, hag_curse13));

            //add to prestige classes
            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, hag_haunted_spellbook, "EldritchKnightHagHaunted",
                          Common.createPrerequisiteArchetypeLevel(hag_haunted, 1),
                          Common.createPrerequisiteClassSpellLevel(spiritualist_class, 3));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, hag_haunted_spellbook, "ArcaneTricksterHagHaunted",
                                        Common.createPrerequisiteArchetypeLevel(hag_haunted, 1),
                                        Common.createPrerequisiteClassSpellLevel(spiritualist_class, 2));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, hag_haunted_spellbook, "MysticTheurgeHagHaunted",
                                        Common.createPrerequisiteArchetypeLevel(hag_haunted, 1),
                                        Common.createPrerequisiteClassSpellLevel(spiritualist_class, 2));
            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, hag_haunted_spellbook, "DragonDiscipleHagHaunted",
                                       Common.createPrerequisiteArchetypeLevel(hag_haunted, 1),
                                       Common.createPrerequisiteClassSpellLevel(spiritualist_class, 1));
        }


        static void createHagDeathCurse()
        {
            var bestow_curse = library.Get<BlueprintAbility>("989ab5c44240907489aba0a8568d0603");
            hag_curse7 = Helpers.CreateFeature("HagDeathCurse7Feature",
                                               "Death Curse",
                                               "At 7th level, the spiritualist gains bestow curse as an extra 3rd-level spell known.",
                                               "",
                                               bestow_curse.Icon,
                                               FeatureGroup.None,
                                               Helpers.CreateAddKnownSpell(bestow_curse, spiritualist_class, 3)
                                               );

            hag_curse13 = Helpers.CreateFeature("HagDeathCurse16Feature",
                                   "Death Curse II",
                                   "At 13th level, the spiritualist gains major curse as an additional 5th-level spell known.",
                                   "",
                                   bestow_curse.Icon,
                                   FeatureGroup.None,
                                   Helpers.CreateAddKnownSpell(NewSpells.curse_major, spiritualist_class, 5)
                                   );

        }


        static void createHagSpellcasting()
        {
            hag_haunted_spellbook = library.CopyAndAdd(spiritualist_class.Spellbook, "HagHauntedSpellbook", "");
            hag_haunted_spellbook.IsArcane = true;
            hag_haunted_spellbook.Name = hag_haunted.LocalizedName;
            hag_haunted_spellbook.CantripsType = CantripsType.Cantrips;
            hag_haunted_spellbook.RemoveComponents<SpellbookMechanics.PsychicSpellbook>();

            hag_spell_casting = Helpers.CreateFeature("HagSpellcastingFeature",
                                                      "Hag Spellcasting",
                                                      "A hag-haunted spiritualist’s spells come from her connection to her hag phantom. Her spells are considered arcane rather than psychic, and they use verbal and somatic components instead of thought and emotion components. She still selects her spells known from the spiritualist class list.\n"
                                                      + "A hag-haunted can cast spiritualist spells while wearing light armor without incurring the normal arcane spell failure chance. Like any other arcane spellcaster, a hag-haunted wearing medium or heavy armor incurs a chance of arcane spell failure.",
                                                      "",
                                                      Helpers.GetIcon("55edf82380a1c8540af6c6037d34f322"),
                                                      FeatureGroup.None,
                                                      Common.createArcaneArmorProficiency(ArmorProficiencyGroup.Light),
                                                      Helpers.Create<SpellbookMechanics.AddUndercastSpells>(p => p.spellbook = hag_haunted_spellbook)
                                                      );
            hag_spell_casting.AddComponents(Common.createCantrips(spiritualist_class, StatType.Wisdom, hag_haunted_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
            hag_spell_casting.AddComponent(Helpers.CreateAddFacts(hag_haunted_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
        }


        static void createHagSharedConsciousnessAndFusedConsciousness()
        {
            var spell_focus = library.Get<BlueprintParametrizedFeature>("16fa59cc9a72a6043b566b49184f53fe");
            var spell_focus_greater = library.Get<BlueprintParametrizedFeature>("5b04b45b228461c43bad768eb0f7c7bf");
            var skill_focus_persuasion = library.Get<BlueprintFeature>("1621be43793c5bb43be55493e9c45924");
            hag_shared_consciousness = Helpers.CreateFeature("HagSharedConsciousnessFeature",
                                             "Shared Consciousness",
                                             "When in the spiritualist’s consciousness, the hag phantom can grant the hag-haunted Spell Focus (necromancy) and Skill Focus (persuasion), but she often revokes them if the spiritualist banishes her there as a punishment, and she might use them as leverage to get what she wants.",
                                             "",
                                             Helpers.GetIcon("b48674cef2bff5e478a007cf57d8345b"),
                                             FeatureGroup.None);

            hag_fused_consciousness = Helpers.CreateFeature("HagFusedConsciousnessFeature",
                                             "Fused Consciousness",
                                             "When the spiritualist reaches 10th level, the hag can also grant Spell Focus (necromancy) and Skill Focus (persuasion) while manifested, and when the spiritualist reaches 12th level, she can also grant Greater Spell Focus (necromancy).",
                                             "",
                                             Helpers.GetIcon("b48674cef2bff5e478a007cf57d8345b"),
                                             FeatureGroup.None,
                                             Common.createAddParametrizedFeatures(spell_focus, SpellSchool.Necromancy),
                                             Common.createAddFeatureIfHasFact(skill_focus_persuasion, skill_focus_persuasion, not: true),
                                             Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.UntypedStackable, SpellDescriptor.MindAffecting),
                                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getSpiritualistArray(),
                                                                                      progression: ContextRankProgression.Custom,
                                                                                      customProgression: new (int, int)[] { (11, 4), (20, 8) })

                                            );

            var shared_consciousness_buff = Helpers.CreateBuff("HagSharedConsciousnessBuff",
                                                   hag_shared_consciousness.Name,
                                                   hag_shared_consciousness.Description,
                                                   "",
                                                   hag_shared_consciousness.Icon,
                                                   null,
                                                   hag_fused_consciousness.ComponentsArray
                                                   );

            hag_fused_consciousness12 = Helpers.CreateFeature("HagFusedConsciousness12Feature",
                                                              "",
                                                              "",
                                                              "",
                                                              null,
                                                              FeatureGroup.None,
                                                              Common.createAddParametrizedFeatures(spell_focus_greater, SpellSchool.Necromancy)
                                                              );
            hag_fused_consciousness12.HideInCharacterSheetAndLevelUp = true;
            hag_fused_consciousness12.HideInUI = true;
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(unsummon_buff,
                                                                                      Helpers.CreateConditional(Common.createContextConditionHasFact(hag_fused_consciousness, has: false),
                                                                                                                Helpers.CreateConditional(Common.createContextConditionHasFact(hag_shared_consciousness, has: true),
                                                                                                                Common.createContextActionApplyBuff(shared_consciousness_buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true, dispellable: false)
                                                                                                                )
                                                                                                                )
                                                                                                                );
        }


        static void createHagPhantom()
        {
            var hag_phantom_feature = Helpers.CreateFeature("HagPhantomFeature",
                                                            "Hag Phantom",
                                                            "A hag phantom forms from the soul of a deceased hag. She always has an evil alignment, rather than matching the spiritualist’s alignment, and must select one of the following emotional focuses: anger, hatred, or zeal. The hag phantom starts with a +2 bonus to Strength and Intelligence and has her own agenda—usually contrary to the spiritualist’s—though she recognizes that the spiritualist can unmanifest her, and therefore she typically hides suspicious actions from her spiritualist.",
                                                            "",
                                                            NewSpells.howling_agony.Icon,
                                                            FeatureGroup.None,
                                                            Helpers.CreateAddStatBonus(StatType.Strength, 2, ModifierDescriptor.Racial),
                                                            Helpers.CreateAddStatBonus(StatType.Intelligence, 2, ModifierDescriptor.Racial)
                                                            );

            hag_phantom = Common.createAddFeatToAnimalCompanion("Spiritualist", hag_phantom_feature, "");
            hag_phantom.AddComponent(Helpers.Create<CompanionMechanics.ChangeCompanionAlignment>(c => c.alignment = Alignment.NeutralEvil));

            var allowed_foci = new string[] { "Anger", "Hatred", "Zeal" };

            foreach (var kv in Phantom.phantom_progressions)
            {
                if (!allowed_foci.Contains(kv.Key))
                {
                    kv.Value.AddComponent(Common.prerequisiteNoArchetype(hag_haunted));
                    Phantom.potent_phantom[kv.Key].AddComponent(Common.prerequisiteNoArchetype(hag_haunted));
                }
            }
            
        }



        public static BlueprintCharacterClass[] getSpiritualistArray()
        {
            return new BlueprintCharacterClass[] { spiritualist_class };
        }


        static void createSpiritualistProgression()
        {
            charisma_spellcasting = Helpers.CreateFeature("SpiritualistCharismaSpellcasting",
                                                          "Emotional Spellcasting",
                                                          "A fractured mind’s ability to cast spells is tied to the force of her own spirit rather than her connection to the spirit world. As a result, she uses her Charisma score rather than her Wisdom score to determine the highest spell level she can cast, the saving throw DCs of spells and spell-like abilities she casts, and her bonus spells per day.",
                                                          "",
                                                          null,
                                                          FeatureGroup.None
                                                          );



            createSpiritualistProficiencies();
            createSpiritualistKnacks();
            createPhantom(); //add fractured mind emotional power instead of spell like abilities
            createSummonUnsummonPhantom();
            //createLink(); //not sure if this one is needed since phantom are already weeker than 
            createEthericTether();
            createSpiritualBond();
            createSharedAndFusedConsciousness();
            createBondedManifestationAndDualBond();
            createSpiritualInferenceAndGreaterSpiritualInference();
            createPhantomRecall();
            createPotentPhantom();

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            spiritualist_progression = Helpers.CreateProgression("SpiritualistProgression",
                                                              spiritualist_class.Name,
                                                              spiritualist_class.Description,
                                                              "",
                                                              spiritualist_class.Icon,
                                                              FeatureGroup.None);
            spiritualist_progression.Classes = getSpiritualistArray();

            spiritualist_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, spiritualist_proficiencies, spiritualist_spellcasting, detect_magic,
                                                                                            spiritualist_knacks, emotional_focus_selection, etheric_tether, shared_consciousness, /*link,*/
                                                                                            masters_alignment,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")), // touch calculate feature                                                                                      
                                                                    Helpers.LevelEntry(2),
                                                                    Helpers.LevelEntry(3, bonded_manifestation),
                                                                    Helpers.LevelEntry(4, spiritual_inference),
                                                                    Helpers.LevelEntry(5),
                                                                    Helpers.LevelEntry(6, phantom_recall),
                                                                    Helpers.LevelEntry(7),
                                                                    Helpers.LevelEntry(8),
                                                                    Helpers.LevelEntry(9),
                                                                    Helpers.LevelEntry(10, fused_consciousness),
                                                                    Helpers.LevelEntry(11),
                                                                    Helpers.LevelEntry(12, greater_spiritual_inference),
                                                                    Helpers.LevelEntry(13),
                                                                    Helpers.LevelEntry(14, spiritual_bond),
                                                                    Helpers.LevelEntry(15),
                                                                    Helpers.LevelEntry(16),
                                                                    Helpers.LevelEntry(17, dual_bond),
                                                                    Helpers.LevelEntry(18),
                                                                    Helpers.LevelEntry(19),
                                                                    Helpers.LevelEntry(20, potent_phantom)
                                                                    };

            spiritualist_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { spiritualist_proficiencies, spiritualist_spellcasting, detect_magic,
                                                                                            spiritualist_knacks, emotional_focus_selection };
            spiritualist_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(etheric_tether, spiritual_inference, phantom_recall, spiritual_bond, greater_spiritual_inference, potent_phantom),
                                                            Helpers.CreateUIGroup(shared_consciousness, bonded_manifestation, fused_consciousness, dual_bond)
                                                           };
        }


        static void createBondedManifestationAndDualBond()
        {
            var bonded_manifestation_ac_fcb = Helpers.CreateFeature("BondedManifestationShiledAcFavoredClassBonusFeature",
                                                "Bonded Manifestation Shield AC Bonus",
                                                "Add 1/6 to the shield bonus granted by the 3rd-level bonded manifestation ability.",
                                                "9aaf6f154e3e4022884715cce4e665f7",
                                                 NewSpells.barrow_haze.Icon,
                                                 FeatureGroup.None);
            bonded_manifestation_ac_fcb.Ranks = 3;


            var bonded_manifestation_buff = Helpers.CreateBuff("BondedManifestationBuff",
                                                               "Bonded Manifestation",
                                                               "At 3rd level, as a swift action, a spiritualist can pull on the consciousness of her phantom and the substance of the Ethereal Plane to partially manifest aspects of both in her own body. When she does, she uses this bonded manifestation to enhance her own abilities while the phantom is still bound to her consciousness.\n"
                                                               + "For the spiritualist to use this ability, the phantom must be confined in the spiritualist’s consciousness; it can’t be manifested in any other way.\n"
                                                               + "During a bonded manifestation, the phantom can’t be damaged, dismissed, or banished. A spiritualist can use bonded manifestation a number of rounds per day equal to 3 + her spiritualist level. The rounds need not be consecutive. She can dismiss the effects of a bonded manifestation as a free action, but even if she dismisses a bonded manifestation on the same round that she used it, it counts as 1 round of use.\n"
                                                               + "Spiritualist gains an ectoplasmic shield that protects her without restricting her movement or actions. She gains a +4 shield bonus to Armor Class; this bonus applies to incorporeal touch attacks.\n"
                                                               + "The ectoplasmic shield has no armor check penalty or arcane spell failure chance. At 8th level, the spiritualist also sprouts a pair of ectoplasmic tendrils from her body. As a swift action, the spiritualist can use these tendrils to attack creatures within her melee reach (using the damage dice of her manifested phantom).\n"
                                                               + "At 13th level, the bonus from ectoplasmic shield increases to +6. At 18th level, the spiritualist can take a full-round action to attack all creatures within her melee reach with her tendrils (using the damage dice of her manifested phantom). When she does, she rolls the attack roll twice and takes the better of the two results.",
                                                               "",
                                                               NewSpells.barrow_haze.Icon,
                                                               Common.createPrefabLink("e9a8af06810719e4d9885c10c827b131"), //from ghost form
                                                               Helpers.Create<AddContextStatBonus>(a =>
                                                               {
                                                                   a.Descriptor = ModifierDescriptor.Shield;
                                                                   a.Stat = StatType.AC;
                                                                   a.Value = Helpers.CreateContextValue(AbilitySharedValue.StatBonus);
                                                                   a.Multiplier = 1;
                                                               }
                                                               ),
                                                               Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.One, Helpers.CreateContextValue(AbilityRankType.StatBonus), Helpers.CreateContextValue(AbilityRankType.Default)), AbilitySharedValue.StatBonus),
                                                               Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getSpiritualistArray(),
                                                                                               progression: ContextRankProgression.Custom,
                                                                                               customProgression: new (int, int)[] { (12, 4), (20, 6) }
                                                                                               ),
                                                               Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.StatBonus,
                                                                                               feature: bonded_manifestation_ac_fcb
                                                                                               ),
                                                               Helpers.Create<NewMechanics.TransferDescriptorBonusToTouchAC>(t =>
                                                               {
                                                                   t.value = Helpers.CreateContextValue(AbilitySharedValue.StatBonus);
                                                                   t.Descriptor = ModifierDescriptor.Sacred;
                                                                   t.required_target_fact = library.Get<BlueprintFeature>("c4a7f98d743bc784c9d4cf2105852c39");
                                                               }
                                                               )
                                                               );

            DiceFormula[] diceFormulas = new DiceFormula[] {new DiceFormula(1, DiceType.D6),
                                                            new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(1, DiceType.D10),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8)};

            var slam = library.CopyAndAdd<BlueprintItemWeapon>("767e6932882a99c4b8ca95c88d823137", "BondedManifestationSlam", "b115f320141a43e4b3ac9b076e7bc49b");
            slam.AddComponent(Helpers.Create<ItemMechanics.ForcePrimary>());
            var bonded_manifestation_buff8_1 = Helpers.CreateBuff("BondedManifestation81Buff",
                                                    "",
                                                    "",
                                                    "",
                                                    null,
                                                    null,
                                                    Common.createAddAdditionalLimb(slam),
                                                    Helpers.Create<NewMechanics.ContextWeaponDamageDiceReplacementForSpecificCategory>(c =>
                                                                                                   {
                                                                                                       c.category = WeaponCategory.OtherNaturalWeapons;
                                                                                                       c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                       c.dice_formulas = diceFormulas;
                                                                                                   }),
                                                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                type: AbilityRankType.Default,
                                                                                progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                                startLevel: 5,
                                                                                stepLevel: 4,
                                                                                classes: getSpiritualistArray())
                                                   );
            bonded_manifestation_buff8_1.SetBuffFlags(BuffFlags.HiddenInUi);

            DiceFormula[] diceFormulas2 = new DiceFormula[] {new DiceFormula(1, DiceType.D8),
                                                            new DiceFormula(2, DiceType.D6),
                                                            new DiceFormula(2, DiceType.D8),
                                                            new DiceFormula(3, DiceType.D6),
                                                            new DiceFormula(3, DiceType.D8)};
            
            var bonded_manifestation_buff8_2 = Helpers.CreateBuff("BondedManifestation82Buff",
                                        "",
                                        "",
                                        "",
                                        null,
                                        null,
                                        Common.createAddAdditionalLimb(slam),
                                        Helpers.Create<NewMechanics.ContextWeaponDamageDiceReplacementForSpecificCategory>(c =>
                                        {
                                            c.category = WeaponCategory.OtherNaturalWeapons;
                                            c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                            c.dice_formulas = diceFormulas2;
                                        }),
                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                    type: AbilityRankType.Default,
                                                                    progression: ContextRankProgression.DelayedStartPlusDivStep,
                                                                    startLevel: 5,
                                                                    stepLevel: 4,
                                                                    classes: getSpiritualistArray())
                                       );
            bonded_manifestation_buff8_2.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_slams = Common.createContextActionOnContextCaster(Helpers.CreateConditional(Common.createContextConditionCasterHasFact(Phantom.phantom_progressions["Anger"]),
                                                                                        Common.createContextActionApplyBuff(bonded_manifestation_buff8_2, Helpers.CreateContextDuration(1)),
                                                                                        Common.createContextActionApplyBuff(bonded_manifestation_buff8_1, Helpers.CreateContextDuration(1))
                                                                                        ));
            var remove_slams = Common.createContextActionOnContextCaster(Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(r => r.Buffs = new BlueprintBuff[] { bonded_manifestation_buff8_2, bonded_manifestation_buff8_1 }));
            var attack8 = Common.createContextActionAttackWithSpecificWeapon(slam,
                                                                             Helpers.CreateActionList(apply_slams),
                                                                             Helpers.CreateActionList(remove_slams),
                                                                             Helpers.CreateActionList(remove_slams));
            var bonded_manifesation8_ability = Helpers.CreateAbility("BondedManifestation8Ability",
                                                                     "Tendrils Attack",
                                                                     "At 8th level, the spiritualist also sprouts a pair of ectoplasmic tendrils from her body. As a swift action, the spiritualist can use these tendrils to attack creatures within her melee reach (using the damage dice of her manifested phantom).",
                                                                     "",
                                                                     Helpers.GetIcon("4ac47ddb9fa1eaf43a1b6809980cfbd2"),
                                                                     AbilityType.Special,
                                                                     CommandType.Swift,
                                                                     AbilityRange.Touch,
                                                                     "",
                                                                     "",
                                                                     Helpers.Create<AbilityDeliverProjectile>(a =>
                                                                     {
                                                                         a.LineWidth = 5.Feet();
                                                                         a.Projectiles = new BlueprintProjectile[]
                                                                         {
                                                                             library.Get<BlueprintProjectile>("2e3992d1695960347a7f9bdf8122966f"),
                                                                             library.Get<BlueprintProjectile>("741743ccd287a854fbb68ce70f75fa05"),
                                                                         };
                                                                     }
                                                                     ),
                                                                     Helpers.CreateRunActions(attack8),
                                                                     Common.createAbilityCasterHasFacts(bonded_manifestation_buff)
                                                                     );
            bonded_manifesation8_ability.setMiscAbilityParametersTouchHarmful(true);



            var bonded_manifestaion8_feature = Common.AbilityToFeature(bonded_manifesation8_ability);

            var buff_reroll_attacks = Helpers.CreateBuff("BondedManifestation13RerollAttacksBuff",
                                                         "",
                                                         "",
                                                         "",
                                                         null,
                                                         null,
                                                         Helpers.Create<ModifyD20>(m =>
                                                         {
                                                             m.Rule = RuleType.AttackRoll;
                                                             m.RollsAmount = 1;
                                                             m.TakeBest = true;
                                                         })
                                                         );
            buff_reroll_attacks.SetBuffFlags(BuffFlags.HiddenInUi);
            var apply_slams18 = Common.createContextActionOnContextCaster(Common.createContextActionApplyBuff(buff_reroll_attacks, Helpers.CreateContextDuration(1)),
                                                                   Helpers.CreateConditional(Common.createContextConditionCasterHasFact(Phantom.phantom_progressions["Anger"]),
                                                                            Common.createContextActionApplyBuff(bonded_manifestation_buff8_2, Helpers.CreateContextDuration(1)),
                                                                            Common.createContextActionApplyBuff(bonded_manifestation_buff8_1, Helpers.CreateContextDuration(1))
                                                                            ));
            var remove_slams18 = Common.createContextActionOnContextCaster(Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(r => r.Buffs = new BlueprintBuff[] { buff_reroll_attacks, bonded_manifestation_buff8_2, bonded_manifestation_buff8_1 }));

            var attack18 = Common.createContextActionAttackWithSpecificWeapon(slam,
                                                                 Helpers.CreateActionList(apply_slams18),
                                                                 Helpers.CreateActionList(remove_slams18),
                                                                 Helpers.CreateActionList(remove_slams18));
            var attack = Helpers.CreateAbility("BondedManifestationCleaveAbility",
                                               "Bonded Manifestation Attack II",
                                                "At 18th level, while using bonded manifestation, the spiritualist can take a full-round action to attack all creatures within her melee reach with her tendrils (using the damage dice of her manifested phantom). When she does, she rolls the attack roll twice, takes the better of the two results, and uses that as her attack roll result against all creatures within her melee reach.",
                                                "",
                                                LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormOfSouls.png"),
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                "",
                                                "",
                                               Helpers.CreateRunActions(attack18),
                                               Common.createAbilityCasterHasFacts(bonded_manifestation_buff),
                                               Helpers.CreateAbilityTargetsAround(7.Feet(), TargetType.Enemy),
                                               Common.createAbilitySpawnFxDestroyOnCast("859a6d74aedf5f349a470ab14afb47d3", anchor: AbilitySpawnFxAnchor.SelectedTarget, position_anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                               );
            attack.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(attack);
            var bonded_manifestaion18_feature = Common.AbilityToFeature(attack);
            
            var bonded_manifestation_resource = Helpers.CreateAbilityResource("BondedManigfestationResource", "", "", "", null);
            bonded_manifestation_resource.SetIncreasedByLevel(3, 1, getSpiritualistArray());
            var toggle = Common.buffToToggle(bonded_manifestation_buff, CommandType.Swift, false,
                                             bonded_manifestation_resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound),
                                             Helpers.Create<CompanionMechanics.ActivatableAbilityCompanionUnsummonedOrNoFeature>(a => a.feature = emotional_focus_selection)
                                             );

            bonded_manifestation = Common.ActivatableAbilityToFeature(toggle, false);
            bonded_manifestation.AddComponents(bonded_manifestation_resource.CreateAddAbilityResource(),
                                               Helpers.CreateAddFeatureOnClassLevel(bonded_manifestaion8_feature, 8, getSpiritualistArray()),
                                               Helpers.CreateAddFeatureOnClassLevel(bonded_manifestaion18_feature, 18, getSpiritualistArray())
                                              );

            dual_bond = Helpers.CreateFeature("DualBondSpiritualistFeature",
                                              "Dual Bond",
                                              "At 17th level, the spiritualist can use her bonded manifestation ability a number of rounds per day equal to 3 + twice her spiritualist level.",
                                              "",
                                              bonded_manifestation.Icon,
                                              FeatureGroup.None,
                                              Helpers.Create<IncreaseResourceAmountBySharedValue>(i => { i.Resource = bonded_manifestation_resource; i.Value = Helpers.CreateContextValue(AbilityRankType.Default);}),
                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getSpiritualistArray())
                                              );
            dual_bond.ReapplyOnLevelUp = true;
        }


        static void createPhantom()
        {
            var phantom_rank_progression = library.CopyAndAdd<BlueprintProgression>("125af359f8bc9a145968b5d8fd8159b8", "SpiritualistPhantomProgression", "");
            phantom_rank_progression.Classes = getSpiritualistArray();
            emotional_focus_selection = Helpers.CreateFeatureSelection("PhantomFeatureSelection",
                                                                  "Phantom",
                                                                  "A spiritualist begins play with the aid of a powerful and versatile spirit entity called a phantom. The phantom forms a link with the spiritualist, who forever after can either harbor the creature within her consciousness or manifest it as an ectoplasmic entity. A phantom has the same alignment as the spiritualist, and it can speak all the languages its master can. A spiritualist can harbor her phantom in her consciousness (see the shared consciousness class feature), manifest it partially (see the bonded manifestation class feature), or fully manifest it. A fully manifested phantom is treated as a summoned creature from the Ethereal Plane, except it is not sent back to the Ethereal Plane until it is reduced to a negative amount of hit points equal to or greater than its Constitution score.\n"
                                                                  + "The phantom does not heal naturally, and can be healed only with magic or by being tended to with the Heal skill while fully manifested in ectoplasmic form. The phantom stays fully manifested until it is either returned to the spiritualist’s consciousness (a standard action) or banished to the Ethereal Plane. If the phantom is banished to the Ethereal Plane, it can’t return to the spiritualist’s consciousness or manifest again for 24 hours.\n"
                                                                  + "Fully manifested phantoms can use magic items (though not wield weapons) appropriate to their forms.\n"
                                                                  + "A fully manifested phantom’s abilities, feats, Hit Dice, saving throws, and skills are tied to the spiritualist’s class level and increase as the spiritualist gains levels.\n"
                                                                  + "Each phantom has an emotional focus—a powerful emotion based on some experience in life that keeps it tethered to the Material and Ethereal planes. This emotional focus also grants the phantom abilities that it can use while manifested. The type of each ability and its power are determined by the spiritualist’s level.\n"
                                                                  + "The emotional focus determines which bonus skill ranks the phantom gains, as well as the skills in which its spiritualist master gains Skill Focus. It also determines the good saving throws of the manifested phantom and the special abilities the phantom gains as it increases in level.\n"
                                                                  + "While phantoms tend to appear much as they did in life—at least as they did at the time of death— each emotional focus twists a phantom’s visage, mannerisms, and even personality in its own way. Unlike with most creatures, a phantom’s emotion aura often manifests for all to see, even those without the benefit of spells or abilities.\n"
                                                                  + "Often phantoms manifest these emotion auras in unique ways, some of which are described in individual emotional focus descriptions.",
                                                                  "",
                                                                  null,
                                                                  FeatureGroup.AnimalCompanion,
                                                                  Helpers.Create<AddFeatureOnApply>(a => a.Feature = phantom_rank_progression),
                                                                  Helpers.Create<AddFeatureOnApply>(a => a.Feature = library.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d"))                                                                 
                                                                  );

            masters_alignment = Helpers.CreateFeature("PhantomMastersAlignment",
                                                          "",
                                                          "",
                                                          "5b6cb21f9bd34e2c90a2ef72a70dce8e",
                                                          null,
                                                          FeatureGroup.None,
                                                          Helpers.Create<CompanionMechanics.ChangeCompanionAlignmentToMasters>()
                                                          );
            masters_alignment.HideInCharacterSheetAndLevelUp = true;
            masters_alignment.HideInUI = true;

            Phantom.create();
            emotional_focus_selection.AllFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("472091361cf118049a2b4339c4ea836a") }; //empty companion
            foreach (var kv in Phantom.phantom_progressions)
            {
                emotional_focus_selection.AllFeatures = emotional_focus_selection.AllFeatures.AddToArray(kv.Value);
            }
        }

        static void createPhantomRecall()
        {
            phantom_recall_resource = Helpers.CreateAbilityResource("PhantomRecallResource", "", "", "", null);
            phantom_recall_resource.SetIncreasedByLevelStartPlusDivStep(0, 6, 1, 4, 1, 0, 0.0f, getSpiritualistArray());


            var ability = library.CopyAndAdd<BlueprintAbility>("5bdc37e4acfa209408334326076a43bc", "PhantomRecallAbility", "");

            ability.Type = AbilityType.Supernatural;
            ability.Parent = null;
            ability.Range = AbilityRange.Personal;
            ability.RemoveComponents<SpellComponent>();
            ability.RemoveComponents<SpellListComponent>();
            ability.RemoveComponents<RecommendationNoFeatFromGroup>();
            ability.SetNameDescriptionIcon("Phantom Recall",
                                           "At 6th level, as a swift action, a spiritualist can call her manifested phantom to her side. This functions as dimension door, using the spiritualist’s caster level. When this ability is used, the phantom appears adjacent to the spiritualist (or as close as possible if all adjacent spaces are occupied). The spiritualist can use this ability once per day at 6th level, plus one additional time per day for every four levels beyond 6th.",
                                           Helpers.GetIcon("a5ec7892fb1c2f74598b3a82f3fd679f")); //stunning barrier

            var dimension_door_component = ability.GetComponent<AbilityCustomDimensionDoor>();

            var dimension_door_call = Helpers.Create<NewMechanics.CustomAbilities.AbilityCustomMoveCompanionToTarget>(a =>
            {
                a.CasterAppearFx = dimension_door_component.CasterAppearFx;
                a.CasterAppearProjectile = dimension_door_component.CasterAppearProjectile;
                a.CasterDisappearFx = dimension_door_component.CasterDisappearFx;
                a.CasterDisappearProjectile = dimension_door_component.CasterDisappearProjectile;
                a.PortalBone = dimension_door_component.PortalBone;
                a.PortalFromPrefab = dimension_door_component.PortalFromPrefab;
                a.Radius = 10.Feet();
                a.SideAppearFx = dimension_door_component.SideAppearFx;
                a.SideAppearProjectile = dimension_door_component.SideAppearProjectile;
                a.SideDisappearFx = dimension_door_component.SideDisappearFx;
                a.SideDisappearProjectile = dimension_door_component.SideDisappearProjectile;
            }
            );
            ability.ReplaceComponent(dimension_door_component, dimension_door_call);
            ability.AddComponent(phantom_recall_resource.CreateResourceLogic());
            ability.setMiscAbilityParametersSelfOnly();
            ability.AddComponent(Helpers.Create<NewMechanics.AbilityCasterCompanionDead>(a => a.not = true));

            phantom_recall = Common.AbilityToFeature(ability, false);
            phantom_recall.AddComponent(Helpers.CreateAddAbilityResource(phantom_recall_resource));
            phantom_recall_ability = ability;

            summon_call_ability = library.CopyAndAdd(ability, "PhantomSummonCallAbility", "");
            summon_call_ability.SetNameDescriptionIcon("Call Phantom", "", null);
            summon_call_ability.RemoveComponents<AbilityResourceLogic>();
            summon_call_ability.Parent = null;
            summon_companion_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(Helpers.Create<ContextActionCastSpell>(c => c.Spell = summon_call_ability))));
        }


        static void createPotentPhantom()
        {
            potent_phantom = Helpers.CreateFeatureSelection("PotentPhantomFeatureSelection",
                                                            "Potent Phantom",
                                                            "At 20th level, the spiritualist’s phantom grows ever more complex and sophisticated in its manifestation. The phantom gains a second emotional focus. This does not change the phantom’s saving throws, but the phantom otherwise grants all the skills and powers of the focus.",
                                                            "",
                                                            Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"),
                                                            FeatureGroup.None);

            foreach (var kv in Phantom.potent_phantom)
            {
                potent_phantom.AllFeatures = potent_phantom.AllFeatures.AddToArray(kv.Value);
            }
        }


        static void createSharedAndFusedConsciousness()
        {
            shared_consciousness = Helpers.CreateFeature("SharedConsciousnessFeature",
                                                         "Shared Consciousness",
                                                         "At 1st level, while a phantom is confined in a spiritualist’s consciousness (but not while it’s fully manifested or banished to the Ethereal Plane), it grants the spiritualist the Skill Focus feat in two skills determined by the phantom’s emotional focus, unless the spiritualist already has Skill Focus in those skills. It also grants a +4 bonus on saving throws against all mind - affecting effects; at 12th level, this bonus increases to + 8.",
                                                         "",
                                                         Helpers.GetIcon("b48674cef2bff5e478a007cf57d8345b"),
                                                         FeatureGroup.None);

            fused_consciousness = Helpers.CreateFeature("FusedConsciousnessFeature",
                                             "Fused Consciousness",
                                             "At 10th level, a spiritualist always gains the benefits of Shared Consciousness, even when her phantom is manifested.",
                                             "",
                                             Helpers.GetIcon("b48674cef2bff5e478a007cf57d8345b"),
                                             FeatureGroup.None,
                                             Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.UntypedStackable, SpellDescriptor.MindAffecting),
                                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getSpiritualistArray(),
                                                                                      progression: ContextRankProgression.Custom,
                                                                                      customProgression: new (int, int)[] { (11, 4), (20, 8) })

                                            );

            foreach (var kv in Phantom.phantom_skill_foci)
            {
                foreach (var sf in kv.Value)
                {
                    fused_consciousness.AddComponents(Common.createAddFeatureIfHasFactAndNotHasFact(Phantom.phantom_progressions[kv.Key], sf, sf));  
                }
            }

            var shared_consciousness_buff = Helpers.CreateBuff("SharedConsciousnessBuff",
                                                   shared_consciousness.Name,
                                                   shared_consciousness.Description,
                                                   "",
                                                   shared_consciousness.Icon,
                                                   null,
                                                   fused_consciousness.ComponentsArray
                                                   );

            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(unsummon_buff,
                                                                                      Helpers.CreateConditional(Common.createContextConditionHasFact(fused_consciousness, has: false),
                                                                                                                Helpers.CreateConditional(Common.createContextConditionHasFact(shared_consciousness, has: true),
                                                                                                                Common.createContextActionApplyBuff(shared_consciousness_buff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true, dispellable: false)
                                                                                                                )
                                                                                                                )
                                                                                      );
        }

        static void createSummonUnsummonPhantom()
        {
            unsummon_buff = Helpers.CreateBuff("PhantomUnsummonedBuff",
                                       "Phantom Confined",
                                       "Your phantom is confined in your conciousness.",
                                       "",
                                       Helpers.GetIcon("4aa7942c3e62a164387a73184bca3fc1"), //disintegrate
                                       null,
                                       Helpers.CreateAddFactContextActions(activated: new GameAction[] { Helpers.Create<CompanionMechanics.ContextActionUnsummonCompanion>() },
                                                                           deactivated: new GameAction[] { Helpers.Create<CompanionMechanics.ContextActionSummonCompanion>()
                                                                           }
                                                                           )
                                      );
            unsummon_buff.SetBuffFlags(BuffFlags.RemoveOnRest);

            var unsummon_companion = Helpers.CreateAbility("SpiritUnsummonAbility",
                                                            "Confine Phantom",
                                                            "Confine your phantom in your consciousness.",
                                                            "",
                                                            unsummon_buff.Icon,
                                                            AbilityType.Supernatural,
                                                            CommandType.Standard,
                                                            AbilityRange.Personal,
                                                            "",
                                                            "",
                                                            Helpers.CreateRunActions(Common.createContextActionApplyBuff(unsummon_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)),
                                                            Helpers.Create<NewMechanics.AbilityCasterCompanionDead>(a => a.not = true),
                                                            Helpers.Create<CompanionMechanics.AbilityCasterCompanionCanBeUnsummoned>()
                                                            );

            unsummon_companion.setMiscAbilityParametersSelfOnly();
            var summon_companion = Helpers.CreateAbility("ManifestSpiritAbility",
                                                           "Manifest Phantom",
                                                           "Fully manifest your phantom.",
                                                           "",
                                                           unsummon_companion.Icon,
                                                           AbilityType.Supernatural,
                                                           CommandType.Standard,
                                                           AbilityRange.Personal,
                                                           "",
                                                           "",
                                                           Helpers.CreateRunActions(Helpers.Create<ContextActionRemoveBuff>(c => c.Buff = unsummon_buff)),
                                                           Helpers.Create<CompanionMechanics.AbilityCasterCompanionUnsummoned>());
            Common.setAsFullRoundAction(summon_companion);
            summon_companion.setMiscAbilityParametersSelfOnly();
            emotional_focus_selection.AddComponent(Helpers.CreateAddFacts(summon_companion, unsummon_companion));
            summon_companion_ability = summon_companion;
        }


        static void createSpiritualInferenceAndGreaterSpiritualInference()
        {
            var spiritual_inference_ac_fcb = Helpers.CreateFeature("SpiritualInferenceShiledAcFavoredClassBonusFeature",
                                                                    "Spiritual Inference Shield AC Bonus",
                                                                    "Add 1/6 to the shield bonus granted to the spiritualist while under the effects of either spiritual interference or greater spiritual interference.",
                                                                    "ce01ec2935e24fcca153863c98295d67",
                                                                     Helpers.GetIcon("ef768022b0785eb43a18969903c537c4"),//shield
                                                                     FeatureGroup.None);
            spiritual_inference_ac_fcb.Ranks = 3;

            var buff1 = Helpers.CreateBuff("SpiritualInferenceBuff",
                                              "",
                                              "",
                                              "",
                                              null,
                                              null,
                                              Helpers.CreateAddStatBonus(StatType.SaveFortitude, 2, ModifierDescriptor.Circumstance),
                                              Helpers.CreateAddStatBonus(StatType.SaveReflex, 2, ModifierDescriptor.Circumstance),
                                              Helpers.CreateAddStatBonus(StatType.SaveWill, 2, ModifierDescriptor.Circumstance)
                                              );
            var buff11 = library.CopyAndAdd<BlueprintBuff>(buff1, "GreaterSpiritualInferenceAllyBuff", "");
            buff1.AddComponents(Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Shield),
                                Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterFeatureRank.ToContextRankBaseValueType(), progression: ContextRankProgression.BonusValue,
                                                                stepLevel: 2,
                                                                feature: spiritual_inference_ac_fcb
                                                                ));
            buff11.AddComponent(Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.Shield));
            var buff2 = Helpers.CreateBuff("GreaterSpiritualInferenceBuff",
                                  "",
                                  "",
                                  "",
                                  null,
                                  null, //shield spell
                                  Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Shield),
                                  Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterFeatureRank.ToContextRankBaseValueType(), progression: ContextRankProgression.BonusValue,
                                                                    stepLevel: 4,
                                                                    feature: spiritual_inference_ac_fcb
                                                                 ),
                                  Helpers.CreateAddStatBonus(StatType.SaveFortitude, 4, ModifierDescriptor.Circumstance),
                                  Helpers.CreateAddStatBonus(StatType.SaveReflex, 4, ModifierDescriptor.Circumstance),
                                  Helpers.CreateAddStatBonus(StatType.SaveWill, 4, ModifierDescriptor.Circumstance)
                                  );

            var shield_ally_eidolon = Common.createAuraEffectFeature("Spiritual Inference",
                                                                     "At 4th level, whenever a spiritualist is within the reach of her ectoplasmic manifested phantom, she gains a +2 shield bonus to her Armor Class and a +2 circumstance bonus on her saving throws. She doesn’t gain these bonuses when the ectoplasmic manifested phantom is grappled, helpless, or unconscious.",
                                                                     Helpers.GetIcon("ef768022b0785eb43a18969903c537c4"),
                                                                     buff1,
                                                                     10.Feet(),
                                                                     Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionIsMaster>())
                                                                     );

            var add_shield_ally = Common.createAddFeatToAnimalCompanion(shield_ally_eidolon, "");
            add_shield_ally.HideInCharacterSheetAndLevelUp = true;

            spiritual_inference = Helpers.CreateFeature("SpiirtualInferenceFeature",
                                                shield_ally_eidolon.Name,
                                                shield_ally_eidolon.Description,
                                                "",
                                                shield_ally_eidolon.Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(add_shield_ally, 12, getSpiritualistArray(), before: true)
                                                );

            var greater_shield_ally_eidolon = Common.createAuraEffectFeature("Greater Spiritual Inference",
                                                         "At 12th level, Whenever allies are within the phantom’s reach, as long as the manifested phantom is in ectoplasmic form, each ally gains a +2 shield bonus to its Armor Class and a +2 circumstance bonus on its saving throws. For the spiritualist, these bonuses increase to +4. The spiritualist and allies within range don’t gain this bonus if the manifested phantom is grappled, helpless, or unconscious. For the spiritualist, this bonus increases to +4. This bonus doesn’t apply if the phantom is unconscious.",
                                                         Helpers.GetIcon("ef768022b0785eb43a18969903c537c4"),
                                                         buff11,
                                                         10.Feet(),
                                                         Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>(), Helpers.Create<NewMechanics.ContextConditionIsMaster>(c => c.Not = true), Helpers.Create<ContextConditionIsCaster>(c => c.Not = true))
                                                         );
            greater_shield_ally_eidolon.AddComponent(Common.createAuraEffectFeatureComponentCustom(buff2,
                                                                                                   10.Feet(),
                                                                                                   Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.ContextConditionIsMaster>())));

            greater_spiritual_inference = Helpers.CreateFeature("GreaterSpiritualInferenceFeature",
                                                        greater_shield_ally_eidolon.Name,
                                                        greater_shield_ally_eidolon.Description,
                                                        "",
                                                        greater_shield_ally_eidolon.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.Create<AddFeatureToCompanion>(a => a.Feature = greater_shield_ally_eidolon)
                                                        );
        }


        static void createEthericTether()
        {
            var etheric_tether_feature = Helpers.CreateFeature("SpiritualistEthericTetherFeature",
                                                                  "Etheric Tether",
                                                                  "Whenever her manifested phantom takes enough damage to send it back to the Ethereal Plane, as a reaction to the damage, the spiritualist can sacrifice any number of her hit points without using an action. Each hit point sacrificed in this way prevents 1 point of damage dealt to the phantom. This can prevent the phantom from being sent back to the Ethereal Plane.",
                                                                  "",
                                                                  Helpers.GetIcon("d5847cad0b0e54c4d82d6c59a3cda6b0"), //breath of life
                                                                  FeatureGroup.None,
                                                                  Helpers.Create<CompanionMechanics.TransferDamageToMaster>()
                                                                  );


            var buff = Helpers.CreateBuff("EthericTetherBuff",
                                          etheric_tether_feature.Name,
                                          etheric_tether_feature.Description,
                                          "",
                                          etheric_tether_feature.Icon,
                                          null,
                                          Helpers.Create<AddFeatureToCompanion>(a => a.Feature = etheric_tether_feature)
                                          );

            var toggle = Helpers.CreateActivatableAbility("SpiritualistEthericTetherAbility",
                                                          etheric_tether_feature.Name,
                                                          etheric_tether_feature.Description,
                                                          "",
                                                          etheric_tether_feature.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          CommandType.Free,
                                                          null);
            toggle.DeactivateImmediately = true;
            toggle.Group = ActivatableAbilityGroupExtension.EidolonLifeLink.ToActivatableAbilityGroup();
            etheric_tether = Common.ActivatableAbilityToFeature(toggle, false);
        }


        static void createSpiritualBond()
        {
            var buff = Helpers.CreateBuff("SpiritualBondBuff",
                                          "Spiritual Bond",
                                          "At 14th level, a spiritualist’s life force becomes intrinsically linked with the phantom’s spiritual essence. As long as the phantom has 1 or more hit points, when the spiritualist takes damage that would reduce her to fewer than 0 hit points, those points of damage are transferred to the phantom instead. This transfer stops after the phantom takes all the points of damage or the phantom is reduced to a negative amount of hit points equal to its Constitution score. In the latter case, points of damage dealt in excess of this limit are dealt to the spiritualist. This ability affects only effects that deal hit point damage.",
                                          "",
                                          Helpers.GetIcon("7792da00c85b9e042a0fdfc2b66ec9a8"), //break enchantment
                                          null,
                                          Helpers.Create<CompanionMechanics.TransferDamageAfterThresholdToPet>(a => a.threshold = 1)
                                          );

            var toggle = Helpers.CreateActivatableAbility("SpiritualBondToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          CommandType.Free,
                                                          null);
            toggle.DeactivateImmediately = true;
            toggle.Group = ActivatableAbilityGroupExtension.EidolonLifeLink.ToActivatableAbilityGroup();
            spiritual_bond = Common.ActivatableAbilityToFeature(toggle, false);
        }


        static void createSpiritualistProficiencies()
        {
            spiritualist_proficiencies = library.CopyAndAdd<BlueprintFeature>("25c97697236ccf2479d0c6a4185eae7f", //sorcerer proficiencies
                                                                "SpiritualistProficiencies",
                                                                "");
            spiritualist_proficiencies.SetName("Spiritualist Proficiencies");
            spiritualist_proficiencies.SetDescription("A spiritualist is proficient with all simple weapons, kukris and scythes, as well as with light armor.");
            spiritualist_proficiencies.ReplaceComponent<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { a.Facts[0],
                                                                                                           library.Get<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132"),//light
                                                                                                           library.Get<BlueprintFeature>("96c174b0ebca7b246b82d4bc4aac4574"),//scythe
                                                                                                           Deities.kukri_proficiency});
        }


        static void createSpiritualistKnacks()
        {
            var daze = library.Get<BlueprintAbility>("55f14bc84d7c85446b07a1b5dd6b2b4c");
            spiritualist_knacks = Common.createCantrips("SpiritualistKnacksFeature",
                                                   "Knacks",
                                                   "A spiritualist learns a number of knacks, or 0-level psychic spells. These spells are cast like any other spell, but they are not expended when cast and may be used again.",
                                                   daze.Icon,
                                                   "",
                                                   spiritualist_class,
                                                   StatType.Wisdom,
                                                   spiritualist_class.Spellbook.SpellList.SpellsByLevel[0].Spells.ToArray());

            spiritualist_knacks.ComponentsArray = new BlueprintComponent[0];
        }


        static BlueprintSpellbook createSpiritualistSpellbook()
        {
            var inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            var spiritualist_spellbook = Helpers.Create<BlueprintSpellbook>();
            spiritualist_spellbook.name = "SpiritualistSpellbook";
            library.AddAsset(spiritualist_spellbook, "");
            spiritualist_spellbook.Name = spiritualist_class.LocalizedName;
            spiritualist_spellbook.SpellsPerDay = inquisitor_class.Spellbook.SpellsPerDay;
            spiritualist_spellbook.SpellsKnown = inquisitor_class.Spellbook.SpellsKnown;
            spiritualist_spellbook.Spontaneous = true;
            spiritualist_spellbook.IsArcane = false;
            spiritualist_spellbook.AllSpellsKnown = false;
            spiritualist_spellbook.CanCopyScrolls = false;
            spiritualist_spellbook.CastingAttribute = StatType.Wisdom;
            spiritualist_spellbook.CharacterClass = spiritualist_class;
            spiritualist_spellbook.CasterLevelModifier = 0;
            spiritualist_spellbook.CantripsType = CantripsType.Cantrips;
            spiritualist_spellbook.SpellsPerLevel = inquisitor_class.Spellbook.SpellsPerLevel;

            spiritualist_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            spiritualist_spellbook.SpellList.name = "SpiritualistSpellList";
            library.AddAsset(spiritualist_spellbook.SpellList, "");
            spiritualist_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < spiritualist_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                spiritualist_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);

            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "55f14bc84d7c85446b07a1b5dd6b2b4c", 0), //daze
                new Common.SpellId( "c3a8f31778c3980498d8f00c980be5f5", 0), //guidance
                new Common.SpellId( "95f206566c5261c42aa5b3e7e0d1e36c", 0), //mage light
                new Common.SpellId( "7bc8e27cba24f0e43ae64ed201ad5785", 0), //resistance
                new Common.SpellId( "5bf3315ce1ed4d94e8805706820ef64d", 0), //touch of fatigue
                new Common.SpellId( "d3a852385ba4cd740992d1970170301a", 0), //virtue

                new Common.SpellId( NewSpells.burst_of_adrenaline.AssetGuid, 1),
                new Common.SpellId( NewSpells.burst_of_insight.AssetGuid, 1),
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( NewSpells.chill_touch.AssetGuid, 1), 
                new Common.SpellId( "5590652e1c2225c4ca30c4a699ab3649", 1), //cure light wounds
                new Common.SpellId( "fbdd8c455ac4cde4a9a3e18c84af9485", 1), //doom
                new Common.SpellId( "4f8181e7a7f1d904fbaea64220e83379", 1), //expeditious retreat
                new Common.SpellId( "e5af3674bb241f14b9a9f6b0c7dc3d27", 1), //inflict light wounds
                new Common.SpellId( "9e1ad5d6f87d19e4d8883d63a6e35568", 1), //mage armor
                new Common.SpellId( "403cf599412299a4f9d5d925c7b9fb33", 1), //magic fang
                new Common.SpellId( NewSpells.obscuring_mist.AssetGuid, 1), 
                new Common.SpellId( "433b1faf4d02cc34abb0ade5ceda47c4", 1), //protection from alignment
                new Common.SpellId( "55a037e514c0ee14a8e3ed14b47061de", 1), //remove fear
                new Common.SpellId( NewSpells.sanctuary.AssetGuid, 1),
                new Common.SpellId( "ef768022b0785eb43a18969903c537c4", 1), //shield
                new Common.SpellId( "8fd74eddd9b6c224693d9ab241f25e84", 1), //summon monster 1
                new Common.SpellId( "ad10bfec6d7ae8b47870e3a545cc8900", 1), //touch of gracelessness

                new Common.SpellId( "03a9630394d10164a9410882d31572f0", 2), //aid
                new Common.SpellId( NewSpells.animate_dead_lesser.AssetGuid, 2),
                new Common.SpellId( NewSpells.blade_tutor.AssetGuid, 2),
                new Common.SpellId( "14ec7a4e52e90fa47a4c8d63c69fd5c1", 2), //blur
                new Common.SpellId( "b7731c2b4fa1c9844a092329177be4c3", 2),//boneshaker
                new Common.SpellId( "6b90c773a6543dc49b2505858ce33db5", 2), //cure moderate wounds
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagle's splendor
                new Common.SpellId( "e1291272c8f48c14ab212a599ad17aac", 2), //effortless armor
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( NewSpells.force_sword.AssetGuid, 2),
                new Common.SpellId( NewSpells.ghoul_touch.AssetGuid, 2),
                new Common.SpellId( "65f0b63c45ea82a4f8b8325768a3832d", 2), //inflict moderate wounds
                new Common.SpellId( NewSpells.inflict_pain.AssetGuid, 2),
                new Common.SpellId( "89940cde01689fb46946b2f8cd7b66b7", 2), //invisibility
                new Common.SpellId( "c28de1f98a3f432448e52e5d47c73208", 2), //protection from arrows
                new Common.SpellId( "e84fc922ccf952943b5240293669b171", 2), //restoration, lesser
                new Common.SpellId( "21ffef7791ce73f468b6fca4d9371e8b", 2), //resist energy
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( NewSpells.shadow_claws.AssetGuid, 2),
                new Common.SpellId( NewSpells.stricken_heart.AssetGuid, 2),
                new Common.SpellId( "970c6db48ff0c6f43afc9dbb48780d03", 2), //summon small elemental
                new Common.SpellId( "1724061e89c667045a6891179ee2e8e7", 2), //summon monster 2
                
                new Common.SpellId( "4b76d32feb089ad4499c3a1ce8e1ac27", 3), //animate dead
                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 3), //bestow curse
                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 3), //blindness
                new Common.SpellId( NewSpells.cloak_of_winds.AssetGuid, 3),
                new Common.SpellId( "3361c5df793b4c8448756146a88026ad", 3), //cure serious wounds
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //displacement
                new Common.SpellId( NewSpells.fly.AssetGuid, 3),
                new Common.SpellId( "486eaff58293f6441a5c2759c4872f98", 3), //haste
                new Common.SpellId( "5ab0d42fb68c9e34abae4921822b9d63", 3), //heroism
                new Common.SpellId( NewSpells.howling_agony.AssetGuid, 3),
                new Common.SpellId( "bd5da98859cf2b3418f6d68ea66cabbe", 3), //inflict serious wounds
                new Common.SpellId( NewSpells.invisibility_purge.AssetGuid, 3),
                new Common.SpellId( "f1100650705a69c4384d3edd88ba0f52", 3), //magic fang, greater
                new Common.SpellId( NewSpells.pain_strike.AssetGuid, 3),
                new Common.SpellId( "d2f116cfe05fcdd4a94e80143b67046f", 3), //protection from energy
                new Common.SpellId( "c927a8b0cd3f5174f8c0b67cdbfde539", 3), //remove blindness
                new Common.SpellId( "4093d5a0eb5cae94e909eb1e0e1a6b36", 3), //remove disiese
                new Common.SpellId( NewSpells.ray_of_exhaustion.AssetGuid, 3),
                new Common.SpellId( NewSpells.rigor_mortis.AssetGuid, 3),
                new Common.SpellId( NewSpells.sands_of_time.AssetGuid, 3),
                new Common.SpellId( "f492622e473d34747806bdb39356eb89", 3), //slow
                new Common.SpellId( NewSpells.spirit_bound_blade.AssetGuid, 3),
                new Common.SpellId( Wildshape.undead_anatomyI.AssetGuid, 3),
                new Common.SpellId( "8a28a811ca5d20d49a863e832c31cce1", 3), //vampyric touch
                new Common.SpellId( "5d61dde0020bbf54ba1521f7ca0229dc", 3), //summon monster 3

                new Common.SpellId( NewSpells.aura_of_doom.AssetGuid, 4),
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 4), //confusion
                new Common.SpellId( "4baf4109145de4345861fe0f2209d903", 4), //crushing despair
                new Common.SpellId( "41c9016596fe1de4faf67425ed691203", 4), //cure critical wounds
                new Common.SpellId( "e9cc9378fd6841f48ad59384e79e9953", 4), //death ward
                new Common.SpellId( NewSpells.debilitating_portent.AssetGuid, 4),
                new Common.SpellId( "4a648b57935a59547b7a2ee86fb4f26a", 4), //dimensions door
                new Common.SpellId( "f34fb78eaaec141469079af124bcfa0f", 4), //enervation
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life, greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( "4c349361d720e844e846ad8c19959b1e", 4), //freedom of movement
                new Common.SpellId( "651110ed4f117a948b41c05c5c7624c0", 4), //inflict critical wounds
                new Common.SpellId( "ecaa0def35b38f949bd1976a6c9539e0", 4), //invisibility greater
                new Common.SpellId( "e7240516af4241b42b2cd819929ea9da", 4), //neutralize poison
                new Common.SpellId( "6717dbaef00c0eb4897a1c908a75dfe5", 4), //phantasmal killer
                new Common.SpellId( "b48674cef2bff5e478a007cf57d8345b", 4), //remove curse
                new Common.SpellId( "f2115ac1148256b4ba20788f7e966830", 4), //restoration
                new Common.SpellId( NewSpells.shadow_conjuration.AssetGuid, 4),
                new Common.SpellId( NewSpells.solid_fog.AssetGuid, 4),
                new Common.SpellId( "e42b1dbff4262c6469a9ff0a6ce730e3", 4), //summon medium elemental
                new Common.SpellId( "7ed74a3ec8c458d4fb50b192fd7be6ef", 4), //summon monster 4

                new Common.SpellId( "7792da00c85b9e042a0fdfc2b66ec9a8", 5), //break enchantment
                new Common.SpellId( "d5847cad0b0e54c4d82d6c59a3cda6b0", 5), //breath of life
                new Common.SpellId( "548d339ba87ee56459c98e80167bdf10", 5), //cloudkill
                new Common.SpellId( NewSpells.curse_major.AssetGuid, 5),
                new Common.SpellId( "95f7cdcec94e293489a85afdf5af1fd7", 5), //dismissal
                new Common.SpellId( "d7cbd2004ce66a042aeab2e95a3c5c61", 5), //dominate person
                new Common.SpellId( "444eed6e26f773a40ab6e4d160c67faa", 5), //feeblemind
                new Common.SpellId( NewSpells.fickle_winds.AssetGuid, 5),
                new Common.SpellId( NewSpells.inflict_pain_mass.AssetGuid, 5),
                new Common.SpellId( "eabf94e4edc6e714cabd96aa69f8b207", 5), //mind fog
                new Common.SpellId( NewSpells.overland_flight.AssetGuid, 5),
                new Common.SpellId( NewSpells.pain_strike_mass.AssetGuid, 5),
                new Common.SpellId( "12fb4a4c22549c74d949e2916a2f0b6a", 5), //phantasmal web
                new Common.SpellId( NewSpells.phantom_limbs.AssetGuid, 5),
                new Common.SpellId( "a0fc99f0933d01643b2b8fe570caa4c5", 5), //raise dead
                new Common.SpellId( "237427308e48c3341b3d532b9d3a001f", 5), //shadow evocation
                new Common.SpellId( "4fbd47525382517419c66fb548fe9a67", 5), //slay living
                new Common.SpellId( "0a5ddfbcfb3989543ac7c936fc256889", 5), //spell resistance
                new Common.SpellId( NewSpells.suffocation.AssetGuid, 5),
                new Common.SpellId( "89404dd71edc1aa42962824b44156fe5", 5), //summon large elemental
                new Common.SpellId( "630c8b85d9f07a64f917d79cb5905741", 5), //summon monster 5
                new Common.SpellId( Wildshape.undead_anatomyII.AssetGuid, 5),
                new Common.SpellId( "a34921035f2a6714e9be5ca76c5e34b5", 5), //vampiric shadow shield
                new Common.SpellId( "8878d0c46dfbd564e9d5756349d5e439", 5), //waves of fatigue
                
                new Common.SpellId( "d361391f645db984bbf58907711a146a", 6), //banishment
                new Common.SpellId( "d42c6d3f29e07b6409d670792d72bc82", 6), //banshee blast
                new Common.SpellId( "a89dcbbab8f40e44e920cc60636097cf", 6), //circle of death
                new Common.SpellId( "76a11b460be25e44ca85904d6806e5a3", 6), //create undead
                new Common.SpellId( "f0f761b808dc4b149b08eaf44b99f633", 6), //dispel magic, greater
                new Common.SpellId( "4aa7942c3e62a164387a73184bca3fc1", 6), //disintegrate
                new Common.SpellId( "3167d30dd3c622c46b0c0cb242061642", 6), //eyebite
                new Common.SpellId( "cc09224ecc9af79449816c45bc5be218", 6), //harm
                new Common.SpellId( "5da172c4c89f9eb4cbb614f3a67357d3", 6), //heal
                new Common.SpellId( "e15e5e7045fda2244b98c8f010adfe31", 6), //heroism greater
                new Common.SpellId( "766ec978fa993034f86a372c8eb1fc10", 6), //summon huge elemental
                new Common.SpellId( "e740afbab0147944dab35d83faa0ae1c", 6), //summon monster 6
                new Common.SpellId( "27203d62eb3d4184c9aced94f22e1806", 6), //transformation     
                new Common.SpellId( "4cf3d0fae3239ec478f51e86f49161cb", 6), //true seeing
                new Common.SpellId( "474ed0aa656cc38499cc9a073d113716", 6), //umbral strike
                new Common.SpellId( Wildshape.undead_anatomyIII.AssetGuid, 6),
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(spiritualist_spellbook.SpellList, spell_id.level);
            }

            spiritualist_spellbook.AddComponent(Helpers.Create<SpellbookMechanics.PsychicSpellbook>());

            spiritualist_spellcasting = Helpers.CreateFeature("SpiritualistSpellCasting",
                                             "Psychic Magic",
                                             "A spiritualist casts psychic spells drawn from the spiritualist spell list. She can cast any spell she knows without preparing it ahead of time, assuming she has not yet used up her allotment of spells per day for the spell’s level. To learn or cast a spell, a spiritualist must have a Wisdom score equal to at least 10 + the spell level. The Difficulty Class for a saving throw against a spiritualist’s spell equals 10 + the spell level + the spiritualist’s Wisdom modifier.\n"
                                             + "A spiritualist can cast only a certain number of spells of each spell level per day.",
                                             "",
                                             null,
                                             FeatureGroup.None);

            spiritualist_spellcasting.AddComponents(Helpers.Create<SpellFailureMechanics.PsychicSpellbook>(p => p.spellbook = spiritualist_spellbook),
                                               Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.NaturalSpell));
            spiritualist_spellcasting.AddComponent(Helpers.Create<SpellbookMechanics.AddUndercastSpells>(p => p.spellbook = spiritualist_spellbook));
            spiritualist_spellcasting.AddComponent(Helpers.CreateAddFact(Investigator.center_self));
            spiritualist_spellcasting.AddComponents(Common.createCantrips(spiritualist_class, StatType.Wisdom, spiritualist_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));
            spiritualist_spellcasting.AddComponents(Helpers.CreateAddFacts(spiritualist_spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()));

            return spiritualist_spellbook;
        }

    }
}
