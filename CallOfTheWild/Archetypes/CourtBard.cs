using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony12;
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
using Kingmaker.Controllers;
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
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
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
    public class CourtBard
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature satire;
        static public BlueprintFeature mockery;
        static public BlueprintFeature glorious_epic;
        static public BlueprintFeature scandal;

        static public BlueprintFeature heraldic_expertise;
        static LibraryScriptableObject library => Main.library;

        static public BlueprintAbilityResource performance_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");

        internal static void create()
        {
            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "CourtBardArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Court Bard");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Spending years studying all of the finer points of erudition and etiquette, the court bard takes up the role of resplendent proclaimer and artist-in-residence at the hand of nobility, royalty, and the well-moneyed elite who aspire to join their ranks.");
            });
            Helpers.SetField(archetype, "m_ParentClass", bard_class);

            library.AddAsset(archetype, "");

            var inspire_courage = library.Get<BlueprintFeature>("acb4df34b25ca9043a6aba1a4c92bc69");
            var bardic_knowledge = library.Get<BlueprintFeature>("65cff8410a336654486c98fd3bacd8c5");
            var inspire_competence = library.Get<BlueprintFeature>("6d3fcfab6d935754c918eb0e004b5ef7");
            var dirge_of_doom = library.Get<BlueprintFeature>("1d48ab2bded57a74dad8af3da07d313a");
            var frightening_tune = library.Get<BlueprintFeature>("cfd8940869a304f4aa9077415f93febe");
            var jack_of_all_trades = library.Get<BlueprintFeature>("21fbafd5dc42d4d488c4d6caed46bc99");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, bardic_knowledge, inspire_courage), 
                                                          Helpers.LevelEntry(3, inspire_competence),
                                                          Helpers.LevelEntry(5, inspire_courage), 
                                                          Helpers.LevelEntry(7, inspire_competence),
                                                          Helpers.LevelEntry(8, dirge_of_doom),
                                                          Helpers.LevelEntry(10, jack_of_all_trades),
                                                          Helpers.LevelEntry(11, inspire_courage, inspire_competence),
                                                          Helpers.LevelEntry(14, frightening_tune),
                                                          Helpers.LevelEntry(15, inspire_competence),
                                                          Helpers.LevelEntry(17, inspire_courage),
                                                          Helpers.LevelEntry(19, inspire_competence),
                                                       };

            createSatire();
            createMockery();
            createGloriousEpic();
            createScandal();
            createHeraldicExpertise();
            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, satire, heraldic_expertise),
                                                       Helpers.LevelEntry(3, mockery),
                                                       Helpers.LevelEntry(8, glorious_epic),
                                                       Helpers.LevelEntry(14, scandal),
                                                     };

            bard_class.Progression.UIGroups = bard_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(satire, mockery, glorious_epic, scandal));
            bard_class.Archetypes = bard_class.Archetypes.AddToArray(archetype);     
        }


        static void createHeraldicExpertise()
        {
            var resource = Helpers.CreateAbilityResource("HeraldicExpertiseResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 10, 1, 5, 1, 0, 0.0f, new BlueprintCharacterClass[] { archetype.GetParentClass() });
            heraldic_expertise = Helpers.CreateFeature("HeraldicExpertiseFeature",
                                                       "Heraldic Expertise",
                                                       "A court bard gains a bonus equal to half his bard level on Persuasion and Knowledge (World) checks (minimum +1). Once per day, the court bard can also reroll a check against one of these skills, though he must take the result of the second roll even if it is worse. He can reroll one additional time per day at 5th level and every five levels thereafter.",
                                                       "",
                                                       Helpers.GetIcon("446f7bf201dc1934f96ac0a26e324803"), //eagles splendor
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddContextStatBonus(StatType.SkillPersuasion, ModifierDescriptor.UntypedStackable),
                                                       Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeWorld, ModifierDescriptor.UntypedStackable),
                                                       Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] {archetype.GetParentClass() },
                                                                                       progression: ContextRankProgression.Div2, min: 1),   
                                                       resource.CreateAddAbilityResource()
                                                       );

            var buff = Helpers.CreateBuff("HeraldicExpertiseBuff",
                                          heraldic_expertise.Name,
                                          heraldic_expertise.Description,
                                          "",
                                          heraldic_expertise.Icon,
                                          null,
                                          Helpers.Create<NewMechanics.ModifyD20WithActions>(m => {
                                              m.SpecificSkill = true;
                                              m.Skill = new StatType[] { StatType.CheckDiplomacy, StatType.CheckIntimidate, StatType.CheckBluff, StatType.SkillKnowledgeWorld };
                                              m.RollsAmount = 1;
                                              m.TakeBest = true;
                                              m.Rule = NewMechanics.ModifyD20WithActions.RuleType.SkillCheck;
                                              m.RerollOnlyIfFailed = true;
                                                              //m.DispellOnRerollFinished = true;
                                                              m.required_resource = resource;
                                              m.actions = Helpers.CreateActionList(Common.createContextActionSpendResource(resource, 1));
                                          })

                                          );

            var ability = Helpers.CreateActivatableAbility("HeraldicExpertiseToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;

            heraldic_expertise.AddComponent(Helpers.CreateAddFact(ability));
        }


        static void createSatire()
        {
            var buff = Helpers.CreateBuff("SatireEffectBuff",
                                          "Satire",
                                          "A court bard can use performance to undermine the confidence of enemies who hear it, causing them to take a –1 penalty on attack and damage rolls (minimum 1) and a –1 penalty on saves against fear and charm effects as long as the bard continues performing. This penalty increases by –1 at 5th level and every six levels thereafter. Satire is a language-dependent, mind-affecting ability that uses audible components.",
                                          "",
                                          Helpers.GetIcon("6b81d1245d2b45b49bb98a8e7d32c64d"), //overwhelming grief
                                          Common.createPrefabLink("6e01d9f56e260ea4088836571d0e6404"),
                                          Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable),
                                          Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.UntypedStackable, SpellDescriptor.Fear | SpellDescriptor.Shaken | SpellDescriptor.Charm),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Custom, classes: new BlueprintCharacterClass[] {archetype.GetParentClass() },
                                                                          customProgression: new (int, int)[] { (4, -1), (10, -2), (16, -3), (20, -4) }),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Sonic | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent)
                                          );

            var toggle = Common.createToggleAreaEffect(buff, 50.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()),
                                          AbilityActivationType.WithUnitCommand,
                                          UnitCommand.CommandType.Standard,
                                          Common.createPrefabLink("2f93a2909cb766f4d961aee34a3c84c2"),
                                          Common.createPrefabLink("9353083f430e5a44a8e9d6e26faec248")
                                          );
            toggle.Group = ActivatableAbilityGroup.BardicPerformance;
            toggle.AddComponent(performance_resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            toggle.Buff.SetBuffFlags(BuffFlags.HiddenInUi);
            satire = Common.ActivatableAbilityToFeature(toggle, false);
        }


        static void createMockery()
        {
            var buff = Helpers.CreateBuff("CourtBardMockeryEffectBuff",
                                          "Mockery",
                                          "A court bard of 3rd level or higher can subtly ridicule and defame a specific individual. The bard selects one target who can hear his performance. That individual takes a –2 penalty on Charisma checks and Charisma-related skill checks as long as the bard continues performing. This penalty increases by –1 every four levels after 3rd. Mockery is a language-dependent, mind-affecting ability that relies on audible components.",
                                          "",
                                          Helpers.GetIcon("4b1f07a71a982824988d7f48cd49f3f8"), //hideous laughter
                                          Common.createPrefabLink("ff26ee73fa464a44ca8bf20e858dc3bc"),
                                          Helpers.CreateAddContextStatBonus(StatType.SkillPersuasion, ModifierDescriptor.UntypedStackable),
                                          Helpers.CreateAddContextStatBonus(StatType.SkillUseMagicDevice, ModifierDescriptor.UntypedStackable),
                                          Common.createAbilityScoreCheckBonus(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.UntypedStackable, StatType.Charisma),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Custom, classes: new BlueprintCharacterClass[] { archetype.GetParentClass() },
                                                                          customProgression: new (int, int)[] { (6, -2), (10, -3), (14, -4), (18, -5) }),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Sonic | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent)
                                          );

            var toggle = Common.createToggleAreaEffect(buff, 30.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()),
                                          AbilityActivationType.WithUnitCommand,
                                          UnitCommand.CommandType.Standard,
                                          Common.createPrefabLink("79665f3d500fdf44083feccf4cbfc00a"),
                                          Common.createPrefabLink("c87c798cd0a410c419ee4bafd4adb68f")
                                          );
            toggle.Group = ActivatableAbilityGroup.BardicPerformance;
            toggle.AddComponent(performance_resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            toggle.Buff.SetBuffFlags(BuffFlags.HiddenInUi);

            mockery = Common.ActivatableAbilityToFeature(toggle, false);
        }


        static void createGloriousEpic()
        {
            var buff = Helpers.CreateBuff("CourtBardGloriousEpicEffectBuff",
                              "Glorious Epic",
                              "A court bard of 8th level or higher can weave captivating tales that engross those who hear them. Enemies within 30 feet become flat-footed unless they succeed at a Will save (DC 10 + 1/2 the bard’s level + the bard’s Cha modifier). A save renders them immune to this ability for 24 hours. Glorious epic is a language-dependent, mind-affecting ability that uses audible components.",
                              "",
                              Helpers.GetIcon("ce7dad2b25acf85429b6c9550787b2d9"), //glitterdust
                              Common.createPrefabLink("6e01d9f56e260ea4088836571d0e6404"),
                              Helpers.Create<FlatFootedMechanics.TargetFlatfooted>(),
                              Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Sonic | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent)
                              );
            buff.SetBuffFlags(BuffFlags.RemoveOnRest);

            var cooldown_buff = Helpers.CreateBuff("GloriousEpicCooldownBuff",
                                                   "Glorious Epic: Cooldown",
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   null
                                                   );
            cooldown_buff.SetBuffFlags(BuffFlags.RemoveOnRest);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);
            var effect = Helpers.CreateActionSavingThrow(SavingThrowType.Will, Helpers.CreateConditionalSaved(apply_cooldown, apply_buff));
            var area_components = new BlueprintComponent[]
            {
                Helpers.CreateAreaEffectRunAction(unitEnter: Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(),
                                                                                       Helpers.CreateConditional(Common.createContextConditionHasFact(cooldown_buff), null, effect)
                                                                                       ),
                                                  unitExit: Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(),
                                                                                      Common.createContextActionRemoveBuffFromCaster(buff)
                                                                                      )
                                                  ),
                Common.createContextCalculateAbilityParamsBasedOnClass(archetype.GetParentClass(), StatType.Charisma),
                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Sonic | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent)
            };


            var toggle = Common.createToggleAreaEffect("CourtBardGloriousEpic", 
                                                       buff.Name,
                                                       buff.Description,
                                                       buff.Icon,
                                                       30.Feet(), 
                                          AbilityActivationType.WithUnitCommand,
                                          UnitCommand.CommandType.Standard,
                                          Common.createPrefabLink("20caf000cd4c3434da00a74f4a49dccc"),
                                          Common.createPrefabLink("39da71647ad4747468d41920d0edd721"),
                                          area_components
                                          );
            toggle.Group = ActivatableAbilityGroup.BardicPerformance;
            toggle.AddComponent(performance_resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            toggle.Buff.SetBuffFlags(BuffFlags.HiddenInUi);

            glorious_epic = Common.ActivatableAbilityToFeature(toggle, false);
        }


        static void createScandal()
        {
            var buff = library.CopyAndAdd<BlueprintBuff>("2e1646c2449c88a4188e58043455a43a", "ScandalEffectBuff", ""); //song of discord buff
            buff.SetNameDescriptionIcon("Scandal",
                                        "court bard of 14th level or higher can combine salacious gossip and biting calumny to incite a riot. Each enemy within 50 feet is affected as if by a song of discord for as long as it can hear the performance. A successful Will save (DC 10 + 1/2 the bard’s level + the bard’s Cha modifier) negates the effect, and that creature is immune to this ability for 24 hours. Scandal is a language-dependent, mind-affecting ability that uses audible components.",
                                        buff.Icon);
            buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            buff.RemoveComponents<SpellDescriptorComponent>();
            buff.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Sonic | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent));

            var cooldown_buff = Helpers.CreateBuff("ScandalCooldownBuff",
                                                   "Scandal: Cooldown",
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   null
                                                   );
            cooldown_buff.SetBuffFlags(BuffFlags.RemoveOnRest);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
            var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false);
            var effect = Helpers.CreateActionSavingThrow(SavingThrowType.Will, Helpers.CreateConditionalSaved(apply_cooldown, apply_buff));
            var area_components = new BlueprintComponent[]
            {
                Helpers.CreateAreaEffectRunAction(unitEnter: Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(),
                                                                                       Helpers.CreateConditional(Common.createContextConditionHasFact(cooldown_buff), null, effect)
                                                                                       ),
                                                  unitExit: Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(),
                                                                                      Common.createContextActionRemoveBuffFromCaster(buff)
                                                                                      )
                                                  ),
                Common.createContextCalculateAbilityParamsBasedOnClass(archetype.GetParentClass(), StatType.Charisma),
                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Sonic | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.LanguageDependent)
            };


            var toggle = Common.createToggleAreaEffect("CourtBardScandal",
                                                       buff.Name,
                                                       buff.Description,
                                                       buff.Icon,
                                                       50.Feet(),
                                          AbilityActivationType.WithUnitCommand,
                                          UnitCommand.CommandType.Standard,
                                          Common.createPrefabLink("5d4308fa344af0243b2dd3b1e500b2cc"), //inspire courage area (to make it 50 feet)
                                          Common.createPrefabLink("c87c798cd0a410c419ee4bafd4adb68f"),
                                          area_components
                                          );
            toggle.Group = ActivatableAbilityGroup.BardicPerformance;
            toggle.AddComponent(performance_resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            toggle.Buff.SetBuffFlags(BuffFlags.HiddenInUi);

            scandal = Common.ActivatableAbilityToFeature(toggle, false);
        }

    }
}
