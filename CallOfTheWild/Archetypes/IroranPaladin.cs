using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
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
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class IroranPaladin
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature confident_defense;
        static public BlueprintFeature unarmed_strike;
        static public BlueprintFeature personal_trial;
        static public BlueprintFeature personal_trial_extra_use;
        static public BlueprintFeature aura_of_excellence;
        static public BlueprintFeature ki_pool;
        static public BlueprintFeature ki_strike_magic;
        static public BlueprintFeature ki_strike_cold_iron_silver;
        static public BlueprintFeature ki_strike_lawful;
        static public BlueprintFeature ki_strike_adamantine;
        static public BlueprintFeature divine_body;
        static public BlueprintFeature aura_of_perfection;
        static public BlueprintFeature irori;

        static public BlueprintBuff personal_trial_buff;

        static LibraryScriptableObject library => Main.library;



        internal static void create()
        {
            var paladin_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "IroranPaladinArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Iroran Paladin");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Iroran paladins meditate on self-perfection and train relentlessly, knowing that their example can inspire others to excel. Irori offers no universal paladin code— each paladin in his service creates his own code as part of his spiritual journey, seeing the adherence to such a self-formulated creed as one of the many tests one must face to reach perfection.");
            });
            Helpers.SetField(archetype, "m_ParentClass", paladin_class);
            library.AddAsset(archetype, "");

            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills =  archetype.GetParentClass().ClassSkills.AddToArray(StatType.SkillAthletics, StatType.SkillMobility);

            createConfidentDefense();
            createUnarmedStrike();
            createPersonalTrial();
            createDivineBody();
            createKiPool();
            createAuraOfExcellence();
            createAuraOfPerfection();


            var paladin_proficiencies = library.Get<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb");
            var smite_evil = library.Get<BlueprintFeature>("3a6db57fce75b0244a6a5819528ddf26");
            var smite_evil_extra = library.Get<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137");
            var weapon_bond_feature = library.Get<BlueprintFeature>("1c7cdc1605554954f838d85bbdd22d90");
            var paladin_deity = library.Get<BlueprintFeatureSelection>("a7c8b73528d34c2479b4bd638503da1d");
            var aura_of_courage = library.Get<BlueprintFeature>("e45ab30f49215054e83b4ea12165409f");
            var weapon_bond_extra_use = library.Get<BlueprintFeature>("5a64de5435667da4eae2e4c95ec87917");
            var aura_of_justice = library.Get<BlueprintFeature>("9f13fdd044ccb8a439f27417481cb00e");
            var channel_energy = library.Get<BlueprintFeature>("cb6d55dda5ab906459d18a435994a760");

            irori = Common.featureToFeature(library.Get<BlueprintFeature>("23a77a5985de08349820429ce1b5a234"), false, "", "IroranPaladin");
            irori.SetDescription("Iroran paladin must worship Irori.");

            var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, paladin_proficiencies, paladin_deity, smite_evil),
                                                          Helpers.LevelEntry(3, aura_of_courage),
                                                          Helpers.LevelEntry(4, smite_evil_extra, channel_energy),
                                                          Helpers.LevelEntry(5, weapon_bond_feature),
                                                          Helpers.LevelEntry(7, smite_evil_extra),
                                                          Helpers.LevelEntry(10, smite_evil_extra),
                                                          Helpers.LevelEntry(11, aura_of_justice),
                                                          Helpers.LevelEntry(13, smite_evil_extra),
                                                          Helpers.LevelEntry(16, smite_evil_extra),
                                                          Helpers.LevelEntry(19, smite_evil_extra),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, irori, personal_trial, confident_defense, unarmed_strike, improved_unarmed_strike),
                                                       Helpers.LevelEntry(3, aura_of_excellence),
                                                       Helpers.LevelEntry(4, ki_pool, ki_strike_magic, personal_trial_extra_use),
                                                       Helpers.LevelEntry(5, divine_body),
                                                       Helpers.LevelEntry(7, ki_strike_cold_iron_silver),
                                                       Helpers.LevelEntry(8, personal_trial_extra_use),
                                                       Helpers.LevelEntry(10, ki_strike_lawful),
                                                       Helpers.LevelEntry(11, aura_of_perfection),
                                                       Helpers.LevelEntry(12, personal_trial_extra_use),
                                                       Helpers.LevelEntry(16, ki_strike_adamantine, personal_trial_extra_use),
                                                       Helpers.LevelEntry(20, personal_trial_extra_use),
                                                     };

            paladin_class.Progression.UIDeterminatorsGroup = paladin_class.Progression.UIDeterminatorsGroup.AddToArray(irori, confident_defense);
            paladin_class.Progression.UIGroups[0].Features.Add(divine_body);
            paladin_class.Progression.UIGroups[1].Features.Add(personal_trial);
            paladin_class.Progression.UIGroups[1].Features.Add(personal_trial_extra_use);
            paladin_class.Progression.UIGroups[2].Features.Add(aura_of_excellence);
            paladin_class.Progression.UIGroups[2].Features.Add(aura_of_perfection);
            paladin_class.Progression.UIGroups[2].Features.Add(ki_pool);
            paladin_class.Progression.UIGroups = paladin_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(ki_strike_magic, ki_strike_cold_iron_silver, ki_strike_lawful, ki_strike_adamantine));
            paladin_class.Archetypes = paladin_class.Archetypes.AddToArray(archetype);
        }


        static void createAuraOfPerfection()
        {
            var ally_buff = Helpers.CreateBuff("AuraOfPerfectionBuff",
                                               "Aura of Perfection",
                                               "At 11th level, whenever an Iroran paladin or ally within 10 feet would reroll a die and take the best result, he can roll an additional die as part of the reroll and use the higher of the three as the result of the roll. This ability functions only while the Iroran paladin is conscious.",
                                               "",
                                               Helpers.GetIcon("9af0b584f6f754045a0a79293d100ab3"),
                                               null,
                                               Helpers.Create<RerollsMechanics.ExtraTakeBestReroll>()
                                               );

            aura_of_perfection = Common.createAuraEffectFeature(ally_buff.Name, ally_buff.Description, ally_buff.Icon, ally_buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()));
        }


        static void createAuraOfExcellence()
        {
            var ally_buff = Helpers.CreateBuff("AuraOfExcellenceBuff",
                                               "Aura of Excellence",
                                               "At 3rd level, an Iroran paladin is immune to any effect that would force him to reroll a die against his will or roll twice and take the lower result. Whenever an ally within 10 feet of him would reroll a die against her will, she can roll twice and take the higher result. Whenever an ally within 10 feet would be forced to roll twice and take the lower result, she can instead roll three times and take the second-lowest result. This ability functions only while the Iroran paladin is conscious.",
                                               "",
                                               Helpers.GetIcon("d316d3d94d20c674db2c24d7de96f6a7"),
                                               null,
                                               Helpers.Create<RerollsMechanics.ExtraGoodRerollOnTakeWorst>()
                                               );

            aura_of_excellence = Common.createAuraEffectFeature(ally_buff.Name, ally_buff.Description, ally_buff.Icon, ally_buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()));
            aura_of_excellence.AddComponent(Helpers.Create<RerollsMechanics.IgnoreTakeWorstRerolls>());
        }


        static void createKiPool()
        {
            var scaled_fist_archetype = library.Get<BlueprintArchetype>("5868fc82eb11a4244926363983897279");
            var scaled_fist_ki_pool = library.Get<BlueprintFeature>("ae98ab7bda409ef4bb39149a212d6732");
            var resource = library.Get<BlueprintAbilityResource>("7d002c1025fbfe2458f1509bf7a89ce1");

            var amount = CallOfTheWild.ExtensionMethods.getMaxAmount(resource);
            Helpers.SetField(amount, "ArchetypesDiv", new BlueprintArchetype[] {scaled_fist_archetype});

            ClassToProgression.addClassToResource(archetype.GetParentClass(), new BlueprintArchetype[] { archetype }, resource, scaled_fist_archetype.GetParentClass());

            ki_pool = Common.featureToFeature(scaled_fist_ki_pool, false, "", "IroranPaladin");


            var ki_ignore_dr_buff = Helpers.CreateBuff("IroranPaladinKiIgnoreDRBuff",
                                                       "Ki Power: Ignore DR",
                                                       "By spending 1 point from his ki pool, an iroran paladin can ignore any damage reduction possessed by the target of his personal trial ability for 1 round.",
                                                       "",
                                                       Helpers.GetIcon("2c38da66e5a599347ac95b3294acbe00"),
                                                       null,
                                                       Helpers.Create<NewMechanics.IgnoreDamageReductionIfTargetHasFact>(i =>
                                                       {
                                                           i.fact = personal_trial_buff;
                                                           i.from_caster = true;
                                                       })
                                                       );

            var ability = library.CopyAndAdd<BlueprintAbility>("ca948bb4ce1a2014fbf4d8d44b553074", "IroranPaladinKiPowerIgnoreDRBuff", "");
            ability.SetNameDescriptionIcon(ki_ignore_dr_buff);
            ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(Common.createContextActionApplyBuff(ki_ignore_dr_buff, Helpers.CreateContextDuration(1), dispellable: false)));

            ki_pool.AddComponent(Helpers.CreateAddFacts(ability));


            ki_strike_magic = library.CopyAndAdd<BlueprintFeature>("1188005ee3160f84f8bed8b19a7d46cf", "IroranPaladinKiStrikeMagic", "");
            ki_strike_magic.SetDescription("At 4th level, ki strike allows the iroran paladin's unarmed attacks to be treated as magic weapons for the purpose of overcoming damage reduction.");

            ki_strike_cold_iron_silver = library.CopyAndAdd<BlueprintFeature>("7b657938fde78b14cae10fc0d3dcb991", "IroranPaladinKiStrikeColdIronSilver", "");
            ki_strike_cold_iron_silver.SetDescription("At 7th level, the iroran paladin's unarmed attacks are treated as cold iron and silver for the purpose of overcoming damage reduction.");

            ki_strike_lawful = library.CopyAndAdd<BlueprintFeature>("34439e527a8f5fb4588024e71960dd42", "IroranPaladinKiStrikeLawful", "");
            ki_strike_lawful.SetDescription("At 10th level, the iroran paladin's unarmed attacks are treated as lawful weapons for the purpose of overcoming damage reduction.");

            ki_strike_adamantine = library.CopyAndAdd<BlueprintFeature>("ddc10a3463bd4d54dbcbe993655cf64e", "IroranPaladinKiStrikeAdamantine", "");
            ki_strike_adamantine.SetDescription("At 16th level, the iroran paladin's unarmed attacks are treated as adamantine weapons for the purpose of overcoming damage reduction and bypassing hardness.");
        }


        static void createDivineBody()
        {
            var ability = library.CopyAndAdd<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4", "DivineBodyIroranPaladinAbility", "");
            ability.SetNameDescriptionIcon("Divine Body",
                                           "Upon reaching 5th level, an Iroran paladin must form a bond with a weapon, except he can only choose to enhance his unarmed strike. This ability otherwise functions as and replaces divine bond.",
                                           Helpers.GetIcon("1bc83efec9f8c4b42a46162d72cbf494") //burst of glory
                                           );
            ability.AddComponent(Common.createAbilityCasterMainWeaponCheck(WeaponCategory.UnarmedStrike));
            divine_body = library.CopyAndAdd<BlueprintFeature>("1c7cdc1605554954f838d85bbdd22d90", "DivineBodyIroranPaladinFeature", "");
            divine_body.SetNameDescriptionIcon(ability);
            divine_body.ReplaceComponent<AddFacts>(a => a.Facts = a.Facts.Skip(1).ToArray().AddToArray(ability));
        }


        static void createPersonalTrial()
        {
            var icon = Helpers.GetIcon("c3a8f31778c3980498d8f00c980be5f5"); //guidance

            var config = Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { archetype.GetParentClass() },
                                                         progression: ContextRankProgression.OnePlusDivStep, stepLevel: 4);
            var buff = Helpers.CreateBuff("PersonalTrialBuff",
                                          "",
                                          "",
                                          "",
                                          icon,
                                          Common.createPrefabLink("5b4cdc22715305949a1bd80fab08302b"),
                                          Helpers.Create<UniqueBuff>(),
                                          Helpers.Create<ACBonusAgainstTarget>(a =>
                                          {
                                              a.CheckCaster = true;
                                              a.Descriptor = ModifierDescriptor.Insight;
                                              a.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                          }
                                          ),
                                          Helpers.Create<DamageBonusAgainstTarget>(a =>
                                          {
                                              a.CheckCaster = true;
                                              a.ApplyToSpellDamage = true;
                                              a.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                          }
                                          ),
                                          config
                                          );
            buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var ability = library.CopyAndAdd<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec", "PersonalTrialAbility", "");
            ability.RemoveComponents<AbilityEffectRunAction>();
            ability.RemoveComponents<ContextRankConfig>();

            ability.AddComponent(Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true)));
            ability.SetNameDescriptionIcon("Personal Trial",
                                           " Once per day, an Iroran paladin can as a swift action declare one target within line of sight as his personal trial. The Iroran paladin gains a +1 insight bonus on attack rolls and damage rolls against that creature, to his AC against attacks made by the target, and on saving throws against the target’s spells and special abilities. This bonus increases by 1 at 4th level and every 4 levels thereafter, to a maximum bonus of +6 at 20th level. The personal trial effect remains until the target of the trial is dead or the next time the paladin rests and regains daily uses of this ability. At 4th level and every three levels thereafter, the Iroran paladin can use personal trial one additional time per day.",
                                           icon);

            personal_trial = Common.AbilityToFeature(ability, false);
            personal_trial.AddComponent(ability.GetComponent<AbilityResourceLogic>().RequiredResource.CreateAddAbilityResource());

            personal_trial.AddComponents(Helpers.Create<NewMechanics.SavingThrowBonusAgainstFactFromCaster>(s =>
                                        {
                                            s.CheckedFact = buff;
                                            s.Descriptor = ModifierDescriptor.Insight;
                                            s.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                        }),
                                        Helpers.Create<NewMechanics.AttackBonusAgainstFactsOwner>(s =>
                                        {
                                            s.CheckedFacts = new BlueprintUnitFact[] { buff };
                                            s.Descriptor = ModifierDescriptor.Insight;
                                            s.Bonus = Helpers.CreateContextValue(AbilityRankType.Default);
                                            s.only_from_caster = true;
                                            s.attack_types = new AttackType[] { AttackType.Melee, AttackType.Touch, AttackType.RangedTouch, AttackType.Ranged };
                                        }),
                                        config
                                        );
            personal_trial.ReapplyOnLevelUp = true;
            personal_trial_extra_use = library.CopyAndAdd<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137", "PersonalTrialExtraUse", "");
            personal_trial_extra_use.SetNameDescriptionIcon(personal_trial);


            personal_trial_buff = buff;
        }


        static void createUnarmedStrike()
        {
            var monk = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var fist1d6_monk = library.Get<BlueprintFeature>("c3fbeb2ffebaaa64aa38ce7a0bb18fb0");
            ClassToProgression.addClassToFeat(archetype.GetParentClass(), new BlueprintArchetype[] { archetype }, ClassToProgression.DomainSpellsType.NoSpells, fist1d6_monk, monk);
            unarmed_strike = library.CopyAndAdd(fist1d6_monk, "IroranPaladin1d6Feature", "");
            unarmed_strike.SetDescription("At 1st level, an Iroran paladin gains Improved Unarmed Strike as a bonus feat. In addition, he gains the unarmed strike monk ability, treating his monk level as his paladin level for calculating his unarmed strike damage.");
        }


        static void createConfidentDefense()
        {
            var monk = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var paladin_proficiencies = library.Get<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb");
            var simple_proficiency = library.Get<BlueprintFeature>("e70ecf1ed95ca2f40b754f1adb22bbdd");
            var martial_proficiency = library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629");
            var light_armor_proficiency = library.Get<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132");
            confident_defense = library.CopyAndAdd(paladin_proficiencies, "IroranPaladinConfidentDefenseFeature", "");
            confident_defense.ReplaceComponent<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { light_armor_proficiency, martial_proficiency, simple_proficiency });


            var scaled_fist_ac = library.Get<BlueprintFeature>("3929bfd1beeeed243970c9fc0cf333f8");
            scaled_fist_ac.SetDescription("");
            scaled_fist_ac.Ranks++;
            foreach (var c in scaled_fist_ac.GetComponents<ContextRankConfig>().ToArray())
            {
                if (c.IsBasedOnClassLevel)
                {
                    //fix it to not to work with normal monk levels
                    var new_c = Helpers.CreateContextRankConfig(ContextRankBaseValueType.SummClassLevelWithArchetype,
                                                                    classes: new BlueprintCharacterClass[] { monk },
                                                                    type: c.Type,
                                                                    archetype: library.Get<BlueprintArchetype>("5868fc82eb11a4244926363983897279"),
                                                                    progression: ContextRankProgression.DivStep,
                                                                    stepLevel: 4);
                    scaled_fist_ac.ReplaceComponent(c, new_c);
                }
                if (c.IsBasedOnCustomProperty) //for balance fixes (class level limiter on charisma)
                {
                    var property = Helpers.GetField<BlueprintUnitProperty>(c, "m_CustomProperty");
                    var cfg = property.GetComponent<NewMechanics.ContextValueWithLimitProperty>().max_value;
                    ClassToProgression.addClassToContextRankConfig(archetype.GetParentClass(), new BlueprintArchetype[] { archetype }, cfg, "IroranPaladin", monk);
                }
            }

            confident_defense.AddComponents(Helpers.Create<MonkNoArmorAndMonkWeaponFeatureUnlock>(c => c.NewFact = scaled_fist_ac));
            confident_defense.AddComponents(Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a => 
            {
                a.feature = scaled_fist_ac;
                a.required_armor = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Light, ArmorProficiencyGroup.None };
                a.forbidden_armor = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Buckler, ArmorProficiencyGroup.LightShield, ArmorProficiencyGroup.HeavyShield, ArmorProficiencyGroup.TowerShield };
            }));
            confident_defense.SetNameDescription("Confident Defense",
                                                 $"At 1st level, when wearing light or no armor and not using a shield, an Iroran paladin adds 1 point of his Charisma bonus (if any){(Main.settings.balance_fixes_monk_ac ? " per class level" : "")} as a dodge bonus to his Armor Class. If he is caught flat-footed or otherwise denied his Dexterity bonus, he also loses this bonus. This ability replaces his proficiency with medium armor, heavy armor, and shields.");
        }
    }
}
