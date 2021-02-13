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
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Blueprints.Items;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UI.Log;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker;
using UnityEngine;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Blueprints.Items.Ecnchantments;

namespace CallOfTheWild
{
    class CleanUp
    {
        internal static void run()
        {
            fixWallAbilitiesAoeVIsualization();
            processRage();
            fixPolymorphSizeChangesStacking();
            fixBardicPerformanceSilenced();

            var domain_selection = Main.library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");
            var cleric = Main.library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var druid = Main.library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            fixDomainSpells(Archetypes.SacredServant.archetype.GetParentClass().Spellbook.SpellList, domain_selection, 4);
            fixDomainSpells(cleric.Spellbook.SpellList, domain_selection, 9);
            fixDomainSpells(druid.Spellbook.SpellList, domain_selection, 9);
            fixDomainSpells(Hunter.hunter_class.Spellbook.SpellList, domain_selection, 6);
            fixDomainSpells(Occultist.occultist_class.Spellbook.SpellList, domain_selection, 6);

            if (Main.settings.deity_for_everyone)
            {
                Main.logger.Log("Enabling deity selection for everyone.");
                addDietySelectionToEveryone();
            }
        }


        static void fixBardicPerformanceSilenced()
        {
            var abilities = Main.library.GetAllBlueprints().OfType<BlueprintActivatableAbility>().Where(a => a.Group == ActivatableAbilityGroup.BardicPerformance);
            foreach (var a in abilities)
            {
                Common.addSpellDescriptor(a.Buff, SpellDescriptor.Sonic);
            }
        }


        static void addDietySelectionToEveryone()
        {
            var atheism = Main.library.Get<BlueprintFeature>("92c0d2da0a836ce418a267093c09ca54");
            var deity = Main.library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");

            deity.AllFeatures = deity.AllFeatures.AddToArray(atheism);

            var forbidden_classes = new BlueprintCharacterClass[]
            {
                Main.library.Get<BlueprintCharacterClass>("f5b8c63b141b2f44cbb8c2d7579c34f5"), //eldritch scion
                Main.library.Get<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920"),
                Eidolon.eidolon_class
            };

            var classes = Main.library.Root.Progression.CharacterClasses.Where(c => !forbidden_classes.Contains(c) && !c.Archetypes.Empty() || c == VindicativeBastard.vindicative_bastard_class).ToList();
            foreach (var c in classes)
            {
                if (hasDietySelection(c.Progression.LevelEntries))
                {//already has diety
                    atheism.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = c));
                    continue;
                }

                c.Progression.LevelEntries[0].Features.Add(deity);

                foreach (var a in c.Archetypes)
                {
                    if (!hasDietySelection(a.AddFeatures))
                    {
                        continue;
                    }
                    atheism.AddComponent(Common.prerequisiteNoArchetype(a));
                    if (a.RemoveFeatures.Empty() || a.RemoveFeatures[0].Level != 1)
                    {
                        a.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, deity) }.AddToArray(a.RemoveFeatures);
                    }
                    else
                    {
                        a.RemoveFeatures[0].Features.Add(deity);
                    }
                }
            }
        }


        static bool hasDietySelection(LevelEntry[] level_entries)
        {
            foreach (var le in level_entries)
            {
                if (le.Features.Any(f =>
                {
                    var selection = (f as BlueprintFeatureSelection);
                    return selection != null && selection.Group == FeatureGroup.Deities;
                }
                                    ))
                {
                    return true;
                }
            }
            return false;
        }


        static void fixWallAbilitiesAoeVIsualization()
        {
            AoeMechanics.AbilityWallRange.load();
            var abilities = Main.library.GetAllBlueprints().OfType<BlueprintAbility>();

            foreach (var a in abilities)
            {
                var run_actions = a.GetComponent<AbilityEffectRunAction>();
                if (run_actions?.Actions?.Actions == null)
                {
                    continue;
                }
                var area = Common.extractActions<ContextActionSpawnAreaEffect>(run_actions.Actions.Actions).FirstOrDefault()?.AreaEffect;
                if (area == null)
                {
                    continue;
                }
                if (area.Shape == AreaEffectShape.Wall)
                {
                    a.CanTargetEnemies = true;
                    a.CanTargetFriends = true;
                    a.AddComponent(Common.createAbilityAoERadius(area.Size / 2, TargetType.Any));
                    a.AddComponent(Helpers.Create<AoeMechanics.AbilityRectangularAoeVisualizer>(ab => { ab.target_type = TargetType.Any; ab.length = area.Size; ab.width = 5.Feet(); }));
                }
            }
        }

        static void processRage()
        {
            BlueprintBuff[] rage_buffs = new BlueprintBuff[] { Main.library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613"), //standart rage
                                                               Bloodrager.bloodrage_buff, //same as all other bloodrages
                                                               Spiritualist.rapture_str_con_buff, //same as rapture_dex_cha_buff
                                                               Skald.inspired_rage_effect_buff,
                                                               Skald.controlled_rage_str_buff,
                                                               Skald.controlled_rage_dex_buff,
                                                               Skald.controlled_rage_con_buff,
                                                               Skald.insightful_contemplation_buff,
                                                               NewRagePowers.greater_ferocious_beast_buff};
            //rage_buffs = rage_buffs.AddToArray(Bloodrager.urban_bloodrage_buffs); //same AddFactContextActions as in normal rage only bonuses are different
            //to ensure coexistance of different rages we need to do the following:
            //on every condition on activated add check that target has no buff to avoid replacing existing buffs,
            //then we need to add all activated conditional actions to new round (to reapply buffs if they dissapear)
            //then in deactivated on remove buff will need to be replaced with remove buff from caster (to avoid removing buffs that are not ours?)


            foreach (var buff in rage_buffs)
            {
                var context_actions = buff.GetComponent<AddFactContextActions>();
                var activated_actions = context_actions.Activated.Actions;
                var new_round_actions = context_actions.NewRound;
                List<GameAction> actions_for_new_round = new List<GameAction>();

                foreach (var action in activated_actions)
                {
                    if (!(action is Conditional))
                    {
                        continue;
                    }

                    var conditional_action = (Conditional)action;
                    if (conditional_action.IfTrue.Actions.Length != 1 || !(conditional_action.IfTrue.Actions[0] is ContextActionApplyBuff))
                    {
                        continue;
                    }

                    var applied_buff = ((ContextActionApplyBuff)conditional_action.IfTrue.Actions[0]).Buff;
                    conditional_action.ConditionsChecker.Conditions = conditional_action.ConditionsChecker.Conditions.AddToArray(Common.createContextConditionHasFact(applied_buff, false));
                    conditional_action.ConditionsChecker.Operation = Operation.And;
                    actions_for_new_round.Add(action);
                }
                new_round_actions.Actions = actions_for_new_round.ToArray().AddToArray(new_round_actions.Actions);

                var deactivated_actions = context_actions.Deactivated;

                for (int i = 0; i < deactivated_actions.Actions.Length; i++)
                {
                    if (!(deactivated_actions.Actions[i] is ContextActionRemoveBuff))
                    {
                        continue;
                    }
                    var buff_to_remove = ((ContextActionRemoveBuff)deactivated_actions.Actions[i]).Buff;
                    deactivated_actions.Actions[i] = Common.createContextActionRemoveBuffFromCaster(buff_to_remove);
                }
            }
            addRageRejection();

            //fix furious
            var furious = Main.library.Get<BlueprintWeaponEnchantment>("b606a3f5daa76cc40add055613970d2a");
            furious.ReplaceComponent<WeaponConditionalEnhancementBonus>(w =>
            {
                w.Conditions = Helpers.CreateConditionsCheckerOr(Common.createContextConditionHasFacts(false, Main.library.Get<BlueprintBuff>("6928adfa56f0dcc468162efde545786b"), NewRagePowers.rage_marker_caster,
                                                                                                               Skald.inspired_rage_effect_buff,
                                                                                                               Skald.controlled_rage_str_buff,
                                                                                                               Skald.controlled_rage_dex_buff,
                                                                                                               Skald.controlled_rage_con_buff,
                                                                                                               Skald.insightful_contemplation_buff,
                                                                                                               NewRagePowers.greater_ferocious_beast_buff));
            });
        }


        static void addRageRejection()
        {
            //adds activatable ability which allows to reject raging songs of skalds and rage spell
            var basic_feat_progression = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("5b72dd2ca2cb73b49903806ee8986325");

            (BlueprintBuff, bool)[] rage_buffs = new (BlueprintBuff, bool)[] { (Skald.inspired_rage_effect_buff, true),
                                                                               (Skald.controlled_rage_str_buff, true),
                                                                               (Skald.controlled_rage_dex_buff, true),
                                                                               (Skald.controlled_rage_con_buff, true),
                                                                               (Skald.insightful_contemplation_buff, true),
                                                                               (Spiritualist.rapture_share_str_con_buff, true),
                                                                               (Spiritualist.rapture_share_dex_cha_buff, true),
                                                                               (ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("6928adfa56f0dcc468162efde545786b"), false), //rage spell
                                                                               (ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("9ec69854596674a4ba40802e6337894d"), false) //inspire ferocity
                                                                             };

            var mind_blank = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("35f3724d4e8877845af488d167cb8a89");
            BlueprintBuff rejection_buff = Helpers.CreateBuff("RejectRageBuff",
                                                             "Reject Rage",
                                                             "When this ability is activated, character will reject rage from external sources (like rage spell or skald raging songs).",
                                                             "",
                                                             LoadIcons.Image2Sprite.Create(@"AbilityIcons/RejectRage.png"),
                                                             null);
            foreach (var b in rage_buffs)
            {
                rejection_buff.AddComponent(Common.createSpecificBuffImmunityExceptCaster(b.Item1, b.Item2));
            }

            var reject_rage_ability = Helpers.CreateActivatableAbility("RejectRageActivatableAbility",
                                                                       rejection_buff.Name,
                                                                       rejection_buff.Description,
                                                                       "",
                                                                       rejection_buff.Icon,
                                                                       rejection_buff,
                                                                       AbilityActivationType.Immediately,
                                                                       CommandType.Free,
                                                                       null);
            reject_rage_ability.DeactivateImmediately = true;
            var reject_rage_feature = Helpers.CreateFeature("RejectRageFeature",
                                                        rejection_buff.Name,
                                                        rejection_buff.Description,
                                                        "",
                                                        rejection_buff.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddFact(reject_rage_ability)
                                                        );
            reject_rage_feature.HideInUI = true;
            reject_rage_feature.HideInCharacterSheetAndLevelUp = true;


            basic_feat_progression.LevelEntries[0].Features.Add(reject_rage_feature);

            Action<UnitDescriptor> save_game_action = delegate (UnitDescriptor u)
            {
                if (!u.HasFact(reject_rage_feature))
                {
                    u.AddFact(reject_rage_feature);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_action);
        }



        static void fixPolymorphSizeChangesStacking()
        {
            var size_buffs = new BlueprintBuff[]
            {
               Main.library.Get<BlueprintBuff>("4f139d125bb602f48bfaec3d3e1937cb"),//enlarge
               Main.library.Get<BlueprintBuff>("b0793973c61a19744a8630468e8f4174"),//reduce
               Main.library.Get<BlueprintBuff>("c84fbb4414925f344b894e9511626296"),//righteous might evil
               Main.library.Get<BlueprintBuff>("17206974f2a2c164db26d1af7fac57d5"),//righteous might good
               Main.library.Get<BlueprintBuff>("3fca5d38053677044a7ffd9a872d3a0a"),//animal growth
               Main.library.Get<BlueprintBuff>("4ce640f9800d444418779a214598d0a3"),//legendary proportions
               Main.library.Get<BlueprintBuff>("6ba82f2c8a7146e6b4880cbe7f8534e8"),//enlarged body mutation mind mutation
            };

            var frightful_aspect = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("e788b02f8d21014488067bdd3ba7b325");
            frightful_aspect.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Polymorph))));

            var polymorphs = Main.library.GetAllBlueprints().OfType<BlueprintBuff>().Where(b =>
                                                                        {
                                                                            if (b.GetComponent<Polymorph>() != null)
                                                                            {
                                                                                return true;
                                                                            }

                                                                            var spell_descriptor = b.GetComponent<SpellDescriptorComponent>();
                                                                            return spell_descriptor != null && ((spell_descriptor.Descriptor.Value & SpellDescriptor.Polymorph) > 0);
                                                                        }
                                                                        );

            foreach (var szb in size_buffs)
            {
                Common.addSpellDescriptor(szb, SpellDescriptor.Polymorph, add_to_area: false);
                foreach (var p in polymorphs)
                {
                    p.AddComponent(Common.createSpecificBuffImmunity(szb));
                }
            }
        }



        static void fixDomainSpells(BlueprintSpellList base_spell_list, BlueprintFeatureSelection domain_selection, int max_level)
        {
            //to avoid bug with different level domain and spellbook spells
            var spells_map = new Dictionary<string, int>();
            for (int i = 1; i <= max_level; i++)
            {
                foreach (var s in base_spell_list.SpellsByLevel[i].Spells)
                {
                    spells_map.Add(s.AssetGuid, i);
                }
            }

            foreach (var d in domain_selection.AllFeatures)
            {
                var spell_list = d.GetComponent<LearnSpellList>()?.SpellList;
                if (spell_list == null)
                {
                    continue;
                }

                for (int i = 1; i <= max_level; i++)
                {
                    foreach (var s in spell_list.SpellsByLevel[i].Spells.ToArray())
                    {
                        if (spells_map.ContainsKey(s.AssetGuid) && spells_map[s.AssetGuid] != i)
                        {
                            Common.replaceDomainSpell(d as BlueprintProgression, SpellDuplicates.addDuplicateSpell(s, "Domain" + s.name, Helpers.MergeIds(s.AssetGuid, "cb8e527ae3db4f81bcac7d03753d5c40")), i);
                        }
                    }
                }
            }
        }
    }
}
