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
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
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

namespace CallOfTheWild.Archetypes
{
    public class SageCounselor
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature[] cunning_fist;
        static public BlueprintFeature deceptive_ki;
        static public BlueprintFeature feinting_flurry;

        static LibraryScriptableObject library => Main.library;

        static BlueprintCharacterClass[] getMonkArray()
        {
            var monk_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            return new BlueprintCharacterClass[] { monk_class };
        }

        static public void create()
        {
            var monk_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SageCounselorArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Sage Counselor");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Sage counselors are ascetics and mystics who leave the confines of the monastery walls to advise secular people about spiritual truths and to seek knowledge of the outside world. They often find work as mentors teaching religion and martial arts, and some of them even become counselors for people in high places. Sage counselors often speak in metaphors, knowing that indirect persuasion is more effective than speaking plainly, and they use indirect tactics in combat as well.");
            });
            Helpers.SetField(archetype, "m_ParentClass", monk_class);
            library.AddAsset(archetype, "");

            var ki_power = library.Get<BlueprintFeature>("e9590244effb4be4f830b1e3fffced13");
            var monk_feat1 = library.Get<BlueprintFeatureSelection>("ac3b7f5c11bce4e44aeb332f66b75bab");
            var monk_feat6 = library.Get<BlueprintFeatureSelection>("b993f42cb119b4f40ac423ae76394374");
            var monk_feat10 = library.Get<BlueprintFeatureSelection>("1051170c612d5b844bfaa817d6f4cfff");

            var ki_power_selection = library.Get<BlueprintFeatureSelection>("3049386713ff04245a38b32483362551");

            createCunningFist();
            createDeceptiveKi();
            createFeintingFlurry();

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, monk_feat1),
                                                          Helpers.LevelEntry(2, monk_feat1),
                                                          Helpers.LevelEntry(3, ki_power),
                                                          Helpers.LevelEntry(4, ki_power_selection),
                                                          Helpers.LevelEntry(6, monk_feat6),
                                                          Helpers.LevelEntry(10, monk_feat10),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, cunning_fist[0]),
                                                       Helpers.LevelEntry(2, cunning_fist[1]),
                                                       Helpers.LevelEntry(3, deceptive_ki),
                                                       Helpers.LevelEntry(6, cunning_fist[2]),
                                                       Helpers.LevelEntry(10, feinting_flurry),
                                                      };

            monk_class.Progression.UIGroups = monk_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(cunning_fist));
            monk_class.Progression.UIGroups = monk_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(deceptive_ki, feinting_flurry));
            monk_class.Archetypes = monk_class.Archetypes.AddToArray(archetype);
        }


        static void createCunningFist()
        {
            var feats = new BlueprintFeature[] {library.Get<BlueprintFeature>("4c44724ffa8844f4d9bedb5bb27d144a"), //combat expertise
                                                 NewFeats.improved_feint,
                                                 NewFeats.greater_feint };

            cunning_fist = new BlueprintFeature[feats.Length];

            for (int i = 0; i < feats.Length; i++)
            {
                cunning_fist[i] = Helpers.CreateFeature("CunningFist" + feats[i].name,
                                                        "Cunning Fist",
                                                        "A sage counselor gains Combat Expertise as a bonus feat, even if he doesn’t meet the prerequisites, and he can ignore the Intelligence prerequisite on feats that have Combat Expertise as a prerequisite. At 2nd level, he gains Improved Feint, and at 6th level, he gains Greater Feint, even if he doesn’t meet the prerequisites.",
                                                        "",
                                                        feats[i].Icon,
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddFact(feats[i])
                                                        );
            }

            string[] features_guids = new string[] { "0f15c6f70d8fb2b49aa6cc24239cc5fa", //improved trip
                                                    "4cc71ae82bdd85b40b3cfe6697bb7949", //greater trip
                                                    "25bc9c439ac44fd44ac3b1e58890916f", //improved disarm
                                                    "ed699d64870044b43bb5a7fbe3f29494", //improved dirty trick
                                                    "52c6b07a68940af41b270b3710682dc7", //greater Dirty trick
                                                    "63d8e3a9ab4d72e4081a7862d7246a79", //greater disarm
                                                    NewFeats.felling_smash.AssetGuid,
                                                    NewFeats.improved_feint.AssetGuid,
                                                    NewFeats.greater_feint.AssetGuid,
                                                    NewFeats.ranged_feint.AssetGuid,
                                                    NewFeats.two_weapon_feint.AssetGuid,
                                                    NewFeats.improved_two_weapon_feint.AssetGuid,
                                                    NewFeats.improved_combat_expertise.AssetGuid,
                                                    NewFeats.swordplay_style.AssetGuid,
                                                    NewFeats.swordplay_upset.AssetGuid
                                                    };

            var cunning_fist_prereq = CallOfTheWild.Helpers.PrerequisiteFeature(cunning_fist[0]);
            foreach (var guid in features_guids)
            {
                var f = library.Get<BlueprintFeature>(guid);
                var stat_reqs = f.GetComponents<PrerequisiteStatValue>().Where(p => p.Stat == StatType.Intelligence).ToArray();
              
                foreach (var sr in stat_reqs)
                {
                    f.ReplaceComponent(sr, CallOfTheWild.Helpers.Create<CallOfTheWild.PrerequisiteMechanics.CompoundPrerequisites>(pa => { pa.prerequisites = new Prerequisite[] { sr, cunning_fist_prereq }; pa.Group = sr.Group; pa.any = true; }));
                }
            }
        }


        static void createDeceptiveKi()
        {
            deceptive_ki = library.CopyAndAdd<BlueprintFeature>("e9590244effb4be4f830b1e3fffced13", "DeceptiveKiFeature", "");
            deceptive_ki.RemoveComponents<AddFacts>();

            var resource = deceptive_ki.GetComponent<AddAbilityResources>().Resource;

            var ki_feint = library.CopyAndAdd(NewFeats.improved_feint_ability, "KiFeintAbility", "");
            ki_feint.SetNameDescriptionIcon("Ki Feint",
                                            "A sage counselor can spend 1 ki point while performing a flurry of blows to feint an opponent as a swift action, but he can’t spend 1 ki point to make an additional attack when making a flurry of blows.",
                                            LoadIcons.Image2Sprite.Create(@"FeatIcons/FeintingFlurry.png"));
            ki_feint.ActionType = CommandType.Swift;
            ki_feint.AddComponent(resource.CreateResourceLogic());



            var deceptive_ki_buff = Helpers.CreateBuff("DeceptiveKiBuff",
                                                       "Deceptive Ki",
                                                       "At 3rd level, the sage counselor can spend 1 point from his ki pool as a swift action to give himself a +4 insight bonus on his next Bluff check.",
                                                       "",
                                                       Helpers.GetIcon("b3da3fbee6a751d4197e446c7e852bcb"), //true seeing
                                                       null,
                                                       Helpers.CreateAddStatBonus(StatType.CheckBluff, 4, ModifierDescriptor.Insight)
                                                       );
            deceptive_ki_buff.AddComponents(Helpers.Create<AddInitiatorSkillRollTrigger>(a => { a.Skill = StatType.CheckBluff; a.Action = Helpers.CreateActionList(Common.createContextActionRemoveBuff(deceptive_ki_buff)); }),
                                            Helpers.Create<AddInitiatorPartySkillRollTrigger>(a => { a.Skill = StatType.CheckBluff; a.Action = Helpers.CreateActionList(Common.createContextActionRemoveBuff(deceptive_ki_buff)); })
                                            );

            var ability = Helpers.CreateAbility("DeceptiveKiAbility",
                                                deceptive_ki_buff.Name,
                                                deceptive_ki_buff.Description,
                                                "",
                                                deceptive_ki_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(deceptive_ki_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)),
                                                resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            deceptive_ki.AddComponents(Helpers.CreateAddFacts(ki_feint, ability));
            deceptive_ki.SetNameDescription("Deceptive Ki",
                                            "A sage counselor can spend 1 ki point while performing a flurry of blows to feint an opponent as a swift action, but he can’t spend 1 ki point to make an additional attack when making a flurry of blows.\n"
                                            + "In addition, the sage counselor can spend 1 point from his ki pool as a swift action to give himself a +4 insight bonus on his next Bluff check. The sage counselor does not gain the ability to spend ki to increase his speed by 20 feet for 1 round."
                                            );
        }


        static void createFeintingFlurry()
        {
            var feint_action = Common.createContextActionApplyBuff(CallOfTheWild.NewFeats.greater_feint_buff, Helpers.CreateContextDuration(), dispellable: false, duration_seconds: 6); //you already have all feint feats
            var feint_check_action = Helpers.Create<CallOfTheWild.SkillMechanics.ContextFeintSkillCheck>(c => c.Success = Helpers.CreateActionList(feint_action));
            var feint_action_list = Helpers.CreateActionList(feint_check_action);
            var feint_buff = Helpers.CreateBuff("FeintingFlurryFeature",
                                                "Feinting Flurry",
                                                "At 10th level, sage counselor can choose to replace his first attack during a flurry of blows with a feint check.",
                                                "",
                                                LoadIcons.Image2Sprite.Create(@"FeatIcons/FeintingFlurry.png"),
                                                null,
                                                Helpers.Create<CallOfTheWild.AttackReplacementMechanics.ReplaceAttackWithActionOnFullAttack>(f => f.action = feint_action_list)
                                                );

            var toggle = Common.buffToToggle(feint_buff, CommandType.Free, true,
                                             Helpers.Create<NewMechanics.FlurryOfBlowsRestriciton>());
            feinting_flurry = Common.ActivatableAbilityToFeature(toggle, false);
        }
    }
}

