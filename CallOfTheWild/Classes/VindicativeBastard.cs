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
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Conditions;

namespace CallOfTheWild
{
    public class VindicativeBastard
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;
        static public BlueprintCharacterClass vindicative_bastard_class;
        static public BlueprintProgression vindicative_bastard_progression;

        static public BlueprintBuff vindicative_smite_buff;
        static public BlueprintAbility vindicative_smite_ability;
        static public BlueprintFeature vindicative_smite;
        static public BlueprintFeature add_vindicative_smite_use;
        static public BlueprintAbilityResource smite_resource;

        static public BlueprintAbility gang_up_ability;
        static public BlueprintAbility swift_gang_up_ability;
        static public BlueprintFeature gang_up;
        static public BlueprintFeature swift_justice;

        static public BlueprintFeatureSelection faded_grace;
        static public BlueprintFeature solo_tactics;
        static public BlueprintAbilityResource solo_tactics_resource;
        static public BlueprintFeatureSelection teamwork_feat;
        static public BlueprintFeature spiteful_tenacity;
        static public BlueprintFeature stalwart;

        static public BlueprintFeature aura_of_self_righteousness;
        static public BlueprintFeature ultimate_vindication;

        static public BlueprintSpellbook vindicative_bastard_spellbook;


        static internal void createClass()
        {
            Main.logger.Log("Vindictive Bastard class test mode: " + test_mode.ToString());
            var paladin_class = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            vindicative_bastard_class = Helpers.Create<BlueprintCharacterClass>();
            vindicative_bastard_class.name = "VindicativeBastardClass";
            library.AddAsset(vindicative_bastard_class, "");


            vindicative_bastard_class.LocalizedName = Helpers.CreateString("VindicativeBastard.Name", "Vindictive Bastard");
            vindicative_bastard_class.LocalizedDescription = Helpers.CreateString("VindicativeBastard.Description",
                                                                        "While paladins often collaborate with less righteous adventurers in order to further their causes, those who spend too much time around companions with particularly loose morals run the risk of adopting those same unscrupulous ideologies and methods. Such a vindictive bastard, as these fallen paladins are known, strikes out for retribution and revenge, far more interested in tearing down those who have harmed her or her companions than furthering a distant deity’s cause."
                                                                        );

            vindicative_bastard_class.m_Icon = paladin_class.Icon;
            vindicative_bastard_class.SkillPoints = paladin_class.SkillPoints;
            vindicative_bastard_class.HitDie = paladin_class.HitDie;
            vindicative_bastard_class.BaseAttackBonus = paladin_class.BaseAttackBonus;
            vindicative_bastard_class.FortitudeSave = paladin_class.FortitudeSave;
            vindicative_bastard_class.ReflexSave = paladin_class.ReflexSave;
            vindicative_bastard_class.WillSave = paladin_class.WillSave;
            createVindicativeBastardSpellbook();
            vindicative_bastard_class.Spellbook = vindicative_bastard_spellbook;
            vindicative_bastard_class.ClassSkills = paladin_class.ClassSkills;
            vindicative_bastard_class.IsDivineCaster = true;
            vindicative_bastard_class.IsArcaneCaster = false;
            vindicative_bastard_class.StartingGold = paladin_class.StartingGold;
            vindicative_bastard_class.PrimaryColor = paladin_class.PrimaryColor;
            vindicative_bastard_class.SecondaryColor = paladin_class.SecondaryColor;
            vindicative_bastard_class.RecommendedAttributes = paladin_class.RecommendedAttributes;
            vindicative_bastard_class.NotRecommendedAttributes = paladin_class.NotRecommendedAttributes;
            vindicative_bastard_class.EquipmentEntities = paladin_class.EquipmentEntities;
            vindicative_bastard_class.MaleEquipmentEntities = paladin_class.MaleEquipmentEntities;
            vindicative_bastard_class.FemaleEquipmentEntities = paladin_class.FemaleEquipmentEntities;
            vindicative_bastard_class.ComponentsArray = new BlueprintComponent[]{ paladin_class.ComponentsArray[0],
                                                                                  Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = paladin_class)
                                                                                };
            paladin_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = vindicative_bastard_class));
            vindicative_bastard_class.StartingItems = paladin_class.StartingItems;

            createVindicativeBastardProgression();
            vindicative_bastard_class.Progression = vindicative_bastard_progression;

            vindicative_bastard_class.Archetypes = new BlueprintArchetype[0];
            Helpers.RegisterClass(vindicative_bastard_class);

            Common.addMTDivineSpellbookProgression(vindicative_bastard_class, vindicative_bastard_class.Spellbook, "MysticTheurgeVindicativeBastard",
                                                     Common.createPrerequisiteClassSpellLevel(vindicative_bastard_class, 2));
        }


        static void createVindicativeBastardSpellbook()
        {
            vindicative_bastard_spellbook = library.CopyAndAdd<BlueprintSpellbook>("bce4989b070ce924b986bf346f59e885", "VindicativeBastardSpellbook", "");
            vindicative_bastard_spellbook.Name = vindicative_bastard_class.LocalizedName;
            vindicative_bastard_spellbook.CharacterClass = vindicative_bastard_class;
            vindicative_bastard_spellbook.CasterLevelModifier = -3;
        }

        static BlueprintCharacterClass[] getVindicativeBastardArray()
        {
            return new BlueprintCharacterClass[] { vindicative_bastard_class };
        }

        static void createVindicativeBastardProgression()
        {
            createVindicativeSmite();
            createFadedGrace();
            createSoloTactics();
            createSpitefulTenacity();
            createTeamworkFeat();
            createGangUp();
            createStalwart();
            createAuraOfSelfRighteousness();
            createUltimateVindication();

            var proficiencies = library.CopyAndAdd<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb", "VindicativeBastardProficiencies", "");
            proficiencies.SetName("Vindicative Bastard Proficiencies");
            proficiencies.SetDescription("Vindictive Bastards are proficient with all simple and martial weapons, with all types of armor (heavy, medium, and light), and with shields (except tower shields).");

            vindicative_bastard_progression = Helpers.CreateProgression("VindicativeBastardProgression",
                                                   vindicative_bastard_class.Name,
                                                   vindicative_bastard_class.Description,
                                                   "",
                                                   vindicative_bastard_class.Icon,
                                                   FeatureGroup.None);
            vindicative_bastard_progression.Classes = getVindicativeBastardArray();
            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            vindicative_bastard_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, proficiencies, detect_magic, vindicative_smite,
                                                                                                    library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                                    library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, faded_grace, solo_tactics),
                                                                    Helpers.LevelEntry(3, spiteful_tenacity, teamwork_feat),
                                                                    Helpers.LevelEntry(4, add_vindicative_smite_use),
                                                                    Helpers.LevelEntry(5, gang_up),
                                                                    Helpers.LevelEntry(6, teamwork_feat),
                                                                    Helpers.LevelEntry(7, add_vindicative_smite_use),
                                                                    Helpers.LevelEntry(8),
                                                                    Helpers.LevelEntry(9, teamwork_feat),
                                                                    Helpers.LevelEntry(10, add_vindicative_smite_use),
                                                                    Helpers.LevelEntry(11, swift_justice),
                                                                    Helpers.LevelEntry(12, teamwork_feat),
                                                                    Helpers.LevelEntry(13, add_vindicative_smite_use),
                                                                    Helpers.LevelEntry(14, stalwart),
                                                                    Helpers.LevelEntry(15, teamwork_feat),
                                                                    Helpers.LevelEntry(16, add_vindicative_smite_use),
                                                                    Helpers.LevelEntry(17, aura_of_self_righteousness),
                                                                    Helpers.LevelEntry(18, teamwork_feat),
                                                                    Helpers.LevelEntry(19, add_vindicative_smite_use),
                                                                    Helpers.LevelEntry(20, ultimate_vindication)
                                                                    };

            vindicative_bastard_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] {proficiencies, detect_magic};
            vindicative_bastard_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(vindicative_smite, add_vindicative_smite_use, add_vindicative_smite_use, add_vindicative_smite_use, add_vindicative_smite_use, add_vindicative_smite_use, add_vindicative_smite_use ),
                                                         Helpers.CreateUIGroup(faded_grace, spiteful_tenacity, gang_up, swift_justice, stalwart, aura_of_self_righteousness, ultimate_vindication),
                                                         Helpers.CreateUIGroup(solo_tactics, teamwork_feat, teamwork_feat, teamwork_feat)
                                                        };
        }


        static void createVindicativeSmite()
        {
            vindicative_smite_buff = library.CopyAndAdd<BlueprintBuff>("b6570b8cbb32eaf4ca8255d0ec3310b0", "VindicativeSmiteBuff", "");
            vindicative_smite_buff.SetName("Vindictive Smite");
            vindicative_smite_buff.RemoveComponents<IgnoreTargetDR>();

            Common.addConditionToResoundingBlow(Common.createContextConditionHasBuffFromCaster(vindicative_smite_buff));


            var vindicative_smite_allowed = Helpers.CreateBuff("VindicativeSmiteAllowedBuff",
                                                               "Vindictive Smite Allowed",
                                                               "",
                                                               "",
                                                               vindicative_smite_buff.Icon,
                                                               null);
            vindicative_smite_allowed.SetBuffFlags(BuffFlags.RemoveOnRest);


            vindicative_smite_ability = library.CopyAndAdd<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec", "VindicativeSmiteAbility", "");
            vindicative_smite_ability.SetName(vindicative_smite_buff.Name);
            vindicative_smite_ability.SetDescription("A vindictive bastard is particularly ruthless against those who have harmed her or her allies. Once per day as a swift action, she can smite one target within sight who has dealt hit point damage to her or an ally. She adds her Charisma modifier to her attack rolls and adds her paladin level to damage rolls against the target of her smite. In addition, while vindictive smite is in effect, the vindictive bastard gains a deflection bonus equal to her Charisma bonus (if any) to her AC against attacks by the target of the smite.\n"
                                             + "The vindictive smite effect remains until the target of the smite is dead or the next time the vindictive bastard rests and regains her uses of this ability. At 4th level and every 3 levels thereafter, the vindictive bastard can invoke her vindictive smite one additional time per day, to a maximum of seven times per day at 19th level.");

            vindicative_smite_ability.ReplaceComponent<AbilityCasterAlignment>(Common.createAbilityTargetHasFact(false, vindicative_smite_allowed));
            vindicative_smite_ability.AddComponent(Common.createAbilityTargetHasFact(true, vindicative_smite_buff));

            var apply_buff = Common.createContextActionApplyBuff(vindicative_smite_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            vindicative_smite_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(apply_buff));

            var config = vindicative_smite_ability.GetComponents<ContextRankConfig>().Where(c => c.IsBasedOnClassLevel).FirstOrDefault();
            var new_config = config.CreateCopy();
            Helpers.SetField(new_config, "m_Class", getVindicativeBastardArray());
            vindicative_smite_ability.ReplaceComponent(config, new_config);

            var apply_allowed = Common.createContextActionApplyBuff(vindicative_smite_allowed, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);

            GameAction trigger = apply_allowed;

            if (!test_mode)
            {
                trigger = Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), trigger);
            }

            var vindicative_smite_trigger = Helpers.CreateBuff("VindicativeSmiteTriggerBuff",
                                                                  "",
                                                                  "",
                                                                  "",
                                                                  null,
                                                                  null,
                                                                  Helpers.Create<NewMechanics.AddIncomingDamageTriggerOnAttacker>(c => c.Actions = Helpers.CreateActionList(trigger))
                                                                  );
            vindicative_smite_trigger.SetBuffFlags(BuffFlags.HiddenInUi);

            var vindicative_smite_trigger_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", "VindicativeSmiteTriggerArea", "");
            vindicative_smite_trigger_area.Size = 100.Feet();
            vindicative_smite_trigger_area.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = vindicative_smite_trigger);

            var vindicative_smite_area_buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", "VindicativeSmiteTriggerAreaBuff", "");
            vindicative_smite_area_buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = vindicative_smite_trigger_area);
            smite_resource = library.Get<BlueprintAbilityResource>("b4274c5bb0bf2ad4190eb7c44859048b");//smite_evil_resource

            vindicative_smite = Helpers.CreateFeature("VindicativeSmiteFeature",
                                                       vindicative_smite_ability.Name,
                                                       vindicative_smite_ability.Description,
                                                       "",
                                                       vindicative_smite_ability.Icon,
                                                       FeatureGroup.None,
                                                       Common.createAuraFeatureComponent(vindicative_smite_area_buff),
                                                       Helpers.CreateAddAbilityResource(smite_resource),
                                                       Helpers.CreateAddFact(vindicative_smite_ability)
                                                       );

            add_vindicative_smite_use = library.CopyAndAdd<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137", "VindicativeSmiteAdditionalUse", "");
            add_vindicative_smite_use.SetName("Vindicative Smite - Additional Use");
            add_vindicative_smite_use.SetDescription(vindicative_smite.Description);
        }


        static void createFadedGrace()
        {
            faded_grace = Helpers.CreateFeatureSelection("FadedGraceFeatureSelection",
                                                         "Faded Grace",
                                                         "At 2nd level, a vindictive bastard gains one of the following as a bonus feat: Great Fortitude, Iron Will, or Lightning Reflexes.",
                                                         "",
                                                         null,
                                                         FeatureGroup.None);

            faded_grace.AllFeatures = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334"),
                library.Get<BlueprintFeature>("15e7da6645a7f3d41bdad7c8c4b9de1e"),
                library.Get<BlueprintFeature>("79042cb55f030614ea29956177977c52")
            };
        }


        static void createSoloTactics()
        {
            var inquisitor_solo_tactics = library.Get<BlueprintFeature>("5602845cd22683840a6f28ec46331051");
            var swift_tactician = library.Get<BlueprintFeature>("4ca47c023f1c158428bd55deb44c735f");
            var solo_tactics_buff = Helpers.CreateBuff("VindicativeBastardSoloTacticsBuff",
                                                       "Solo Tactics",
                                                       "At 2nd level, a vindictive bastard gains solo tactics, as per the inquisitor class feature. She can activate this ability as a swift action and gains the benefits of it for 1 round. She can use this ability a number of rounds per day equal to half her paladin level + her Charisma modifier.",
                                                       "",
                                                       swift_tactician.Icon,
                                                       null,
                                                       inquisitor_solo_tactics.ComponentsArray[0]);

            solo_tactics_resource = Helpers.CreateAbilityResource("HolyVindicatorSoloTacticsResource", "", "", "", null);
            solo_tactics_resource.SetIncreasedByStat(0, StatType.Charisma);
            solo_tactics_resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, getVindicativeBastardArray());

            var solo_tactics_ability = Helpers.CreateActivatableAbility("VindicativeBastardSoloTacticsActivatableAbility",
                                                                        solo_tactics_buff.Name,
                                                                        solo_tactics_buff.Description,
                                                                        "",
                                                                        solo_tactics_buff.Icon,
                                                                        solo_tactics_buff,
                                                                        AbilityActivationType.Immediately,
                                                                        CommandType.Free,
                                                                        null,
                                                                        Helpers.CreateActivatableResourceLogic(solo_tactics_resource, ResourceSpendType.NewRound)
                                                                        );
            if (!test_mode)
            {
                Helpers.SetField(solo_tactics_ability, "m_ActivateWithUnitCommand", CommandType.Swift);
                solo_tactics_ability.DeactivateIfCombatEnded = true;
                solo_tactics_ability.AddComponent(Common.createActivatableAbilityUnitCommand(CommandType.Swift));
            }

            solo_tactics = Common.ActivatableAbilityToFeature(solo_tactics_ability, false);
            solo_tactics.AddComponent(Helpers.CreateAddAbilityResource(solo_tactics_resource));
        }


        static void createTeamworkFeat()
        {
            teamwork_feat = library.CopyAndAdd<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb", "VindicativeBastardTeamworkFeatSelection", "");
            teamwork_feat.SetDescription("At 3rd level and every 3 levels thereafter, the vindictive bastard gains a bonus feat in addition to those gained from normal advancement. These bonus feats must be selected from those listed as teamwork feats.\n"
                                         +"The vindictive bastard must meet the prerequisites of the selected bonus feat.");
        }


        static void createSpitefulTenacity()
        {
            var diehard = library.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad");
            spiteful_tenacity = Helpers.CreateFeature("SpitefulTenacityFeature",
                                                       "Spiteful Tenacity",
                                                       "At 3rd level vindictive bastard receives diehard feat for free.",
                                                       "",
                                                       diehard.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(diehard)
                                                       );
        }

        static void createGangUp()
        {
            var freebooter_bane_buff = library.Get<BlueprintBuff>("76dabd40a1c1c644c86ce30e41ad5cab");
            var gang_up_buff = library.CopyAndAdd<BlueprintBuff>(vindicative_smite_buff.AssetGuid, "GangUpBuff", "");
            gang_up_buff.SetName("Gang Up");
            gang_up_buff.SetIcon(freebooter_bane_buff.Icon);
            gang_up_buff.ReplaceComponent<ACBonusAgainstTarget>(a => { a.CheckCaster = false; a.CheckCasterFriend = true; });
            gang_up_buff.ReplaceComponent<AttackBonusAgainstTarget>(a => { a.CheckCaster = false; a.CheckCasterFriend = true; });
            gang_up_buff.ReplaceComponent<DamageBonusAgainstTarget>(a => { a.CheckCaster = false; a.CheckCasterFriend = true; });


            gang_up_ability = library.CopyAndAdd<BlueprintAbility>(vindicative_smite_ability.AssetGuid, "GangUpAbility", "");
            gang_up_ability.RemoveComponents<AbilityTargetHasFact>();
            gang_up_ability.RemoveComponents<AbilityResourceLogic>();
            gang_up_ability.AddComponent(Common.createAbilityTargetHasFact(false, vindicative_smite_buff));
            gang_up_ability.ActionType = CommandType.Move;

            foreach (var c in gang_up_ability.GetComponents<ContextRankConfig>().ToArray())
            {
                var new_c = c.CreateCopy();
                Helpers.SetField(new_c, "m_Min", 1);
                Helpers.SetField(new_c, "m_UseMin", true);
                Helpers.SetField(new_c, "m_Progression", ContextRankProgression.Div2);
                gang_up_ability.ReplaceComponent(c, new_c);
            }

            var apply_buff = Common.createContextActionApplyBuff(gang_up_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.SpeedBonus)), dispellable: false);
            gang_up_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(apply_buff));
            gang_up_ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, min: 1, type: AbilityRankType.SpeedBonus, stat: StatType.Charisma));
            gang_up_ability.SetName(gang_up_buff.Name);
            gang_up_ability.SetIcon(gang_up_buff.Icon);
            gang_up_ability.SetDescription("At 5th level, a vindictive bastard forms a close bond with her companions. This allows her to spend a move action to grant half her vindictive smite bonus against a single target to all allies who can see and hear her. This bonus lasts for a number of rounds equal to the vindictive bastard’s Charisma modifier (minimum 1).");

            swift_gang_up_ability = library.CopyAndAdd<BlueprintAbility>(gang_up_ability.AssetGuid, "SwiftGangUpAbility", "");
            swift_gang_up_ability.ActionType = CommandType.Swift;
            swift_gang_up_ability.SetName("Swift Justice");
            swift_gang_up_ability.SetDescription("At 11th level, a vindictive bastard can activate her gang up ability as a swift action.");

            gang_up = Common.AbilityToFeature(gang_up_ability, false, "");
            swift_justice = Common.AbilityToFeature(swift_gang_up_ability, false, "");
        }


        static void createStalwart()
        {
            stalwart = library.Get<BlueprintFeature>("ec9dbc9a5fa26e446a54fe5df6779088");
            stalwart.SetDescription("A character can use mental and physical resiliency to avoid certain attacks. If she makes a Fortitude or Will saving throw against an attack that has a halved damage on a successful save, she instead avoids the damage entirely.");
        }


        static void createAuraOfSelfRighteousness()
        {
            var effect_buff = library.CopyAndAdd<BlueprintBuff>("44939bb018ccac24f8e055f3eddc16f2", "AuraOfSelfRighteousnessEffectBuff", "");
            effect_buff.SetName("Aura of Self-Righteousness");
            effect_buff.SetDescription("At 17th level, a vindictive bastard gains DR 5/lawful or good and immunity to compulsion spells and spell-like abilities. Each ally within 10 feet of her gains a +4 morale bonus on saving throws against compulsion effects. Aura of self-righteousness functions only while the vindictive bastard is conscious, not if she is unconscious or dead.");
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("0c523f9b68ded1a4ca152f3169066a0f", "AuraOfSelfRighteousnessArea", "");
            area.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = effect_buff);
            var buff = library.CopyAndAdd<BlueprintBuff>("bacdf633f8ffdfd4b92bc7f2de43a1c5", "AuraOfSelfRighteousnessBuff", "");
            buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);

            aura_of_self_righteousness = library.CopyAndAdd<BlueprintFeature>("6bd4a71232014254e80726f3a3756962", "AuraOfSelfRighteousnessFeature", "");
            aura_of_self_righteousness.ReplaceComponent<AuraFeatureComponent>(a => a.Buff = buff);
            aura_of_self_righteousness.ReplaceComponent<AddDamageResistancePhysical>(a => a.Alignment = DamageAlignment.Good | DamageAlignment.Lawful);
            aura_of_self_righteousness.SetName(effect_buff.Name);
            aura_of_self_righteousness.SetDescription(effect_buff.Description);
        }



        static void createUltimateVindication()
        {
            var disintegrate = library.CopyAndAdd<BlueprintAbility>("4aa7942c3e62a164387a73184bca3fc1", "UltimateVindicationDisintegrateAbility", "");
            disintegrate.RemoveComponents<SpellListComponent>();
            disintegrate.RemoveComponents<AbilityDeliverProjectile>();
            disintegrate.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClass(vindicative_bastard_class, StatType.Charisma));
            disintegrate.Type = AbilityType.Supernatural;

            var buff_cooldown = Helpers.CreateBuff("UltimateVindicationDisintegrateCooldownBuff",
                                       "Ultimate Vindication Cooldown",
                                       "",
                                       "",
                                       disintegrate.Icon,
                                       null);

            var apply_cooldown = Common.createContextActionApplyBuff(buff_cooldown, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);
            disintegrate.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(apply_cooldown)));


            var buff_allowed = Helpers.CreateBuff("UltimateVindicationDisintegrateAllowedBuff",
                                                   "Ultimate Vindication Allowed",
                                                   "",
                                                   "",
                                                   disintegrate.Icon,
                                                   null);

            var hit_effect = Helpers.CreateConditional(new Condition[] { Helpers.CreateConditionHasBuff(buff_allowed), Common.createContextConditionHasBuffFromCaster(buff_cooldown, true) },
                                                       Helpers.Create<ContextActionCastSpell>(c => c.Spell = disintegrate)
                                                      );

            ultimate_vindication = Helpers.CreateFeature("UltimateVindicationFeature",
                                                         "Ultimate Vindication",
                                                         "At 20th level, if a foe knocks vindictive bastard or one of a vindictive bastard’s allies unconscious, the vindictive bastard musters a vindictive fury. The next time she hits that foe within 1 minute, the vindictive bastard can channel the effects of a disintegrate spell through her weapon, using her paladin level as her effective caster level.\n"
                                                         + " Whether or not the target succeeds at its save against the disintegrate effect, it is immune to this ability for the next 24 hours.",
                                                         "",
                                                         disintegrate.Icon,
                                                         FeatureGroup.None,
                                                         Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(hit_effect))
                                                         );


            var apply_allowed = Common.createContextActionApplyBuff(buff_allowed, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);

            GameAction trigger = apply_allowed;
            if (!test_mode)
            {
                trigger = Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(), trigger);
            }

            var ultimate_vindication_trigger = Helpers.CreateBuff("UltimateVindicationTriggerBuff",
                                                                  "",
                                                                  "",
                                                                  "",
                                                                  null,
                                                                  null,
                                                                  Helpers.Create<NewMechanics.AddIncomingDamageTriggerOnAttacker>(c => 
                                                                                                                    { c.Actions = Helpers.CreateActionList(trigger); c.reduce_below0 = true; })
                                                                  );
            ultimate_vindication_trigger.SetBuffFlags(BuffFlags.HiddenInUi);

            var ultimate_vindication_trigger_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("7ced0efa297bd5142ab749f6e33b112b", "UltimateVindicationTriggerArea", "");
            ultimate_vindication_trigger_area.Size = 100.Feet();
            ultimate_vindication_trigger_area.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = ultimate_vindication_trigger);

            var ultimate_vindication_area_buff = library.CopyAndAdd<BlueprintBuff>("c96380f6dcac83c45acdb698ae70ffc4", "UltimateVindicationTriggerAreaBuff", "");
            ultimate_vindication_area_buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = ultimate_vindication_trigger_area);

            ultimate_vindication.AddComponent(Common.createAuraFeatureComponent(ultimate_vindication_area_buff));
        }

    }
}
