using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
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
    internal class BalanceFixes
    {
        static LibraryScriptableObject library => Main.library;
        static DiceType[] dices = new DiceType[] { DiceType.D3, DiceType.D4, DiceType.D6, DiceType.D8, DiceType.D10};
        static Dictionary<DiceType, DiceType> dices_shift = new Dictionary<DiceType, DiceType> { { DiceType.D3, DiceType.D4 },
                                                                                                 { DiceType.D4, DiceType.D6 },
                                                                                                 { DiceType.D6, DiceType.D8 },
                                                                                                 { DiceType.D8, DiceType.D12},
                                                                                                 { DiceType.D10, DiceType.D20}
                                                                                               };

        static Dictionary<BlueprintFeature, HashSet<DiceType>> processed_facts = new Dictionary<BlueprintFeature, HashSet<DiceType>>();
        static HashSet<BlueprintAbilityAreaEffect> processed_areas = new HashSet<BlueprintAbilityAreaEffect>();
        static Dictionary<BlueprintAbility, HashSet<DiceType>> processed_abilities = new Dictionary<BlueprintAbility, HashSet<DiceType>>();
        static HashSet<BlueprintBuff> processed_buffs = new HashSet<BlueprintBuff>();
        static HashSet<string> black_list;
        static internal void load(params string[]  guids_black_list)
        {
            black_list = new HashSet<string>();
            black_list.AddRange(guids_black_list);
            fixSpellDamageAbilities();
            fixFeatures();
            manualTextFixes();
            fixLowerLevelBlasts();
            manualFixes();

            fixSorcererBloodlineArcana();
            fixFeats();
            if (Main.settings.balance_fixes_monk_ac)
            {
                fixMonkAc();
            }
            fixPaladinSaves();
            fixTwf();
            fixVitalStrike();
            fixCleave();
        }


        static void fixCleave()
        {
            var greate_cleave = library.Get<BlueprintFeature>("cc9c862ef2e03af4f89be5088851ea35");
            var cleave = library.Get<BlueprintFeature>("d809b6c4ff2aaff4fa70d712a70f7d7b");
            var cleave_ability = library.Get<BlueprintAbility>("6447d104a2222c14d9c9b8a36e4eb242");

            cleave.SetDescription(greate_cleave.Description);
            cleave_ability.SetDescription(greate_cleave.Description);
            cleave_ability.GetComponent<AbilityCustomCleave>().GreaterFeature = cleave;

            var cleaving_finish_greater = library.Get<BlueprintFeature>("ffa1b373190af4f4db7a5501904a1983");
            cleaving_finish_greater.RemoveComponents<Prerequisite>();
            cleaving_finish_greater.HideInUI = true;
            cleaving_finish_greater.HideInCharacterSheetAndLevelUp = true;

            var cleaving_finish = library.Get<BlueprintFeature>("59bd93899149fa44687ff4121389b3a9");
            cleaving_finish.AddComponent(Helpers.CreateAddFact(cleaving_finish_greater));
            cleaving_finish.SetDescription("If you make a melee attack, and your target drops to 0 or fewer hit points as a result of your attack, you can make another melee attack using your highest base attack bonus against another opponent within reach.");


            var selections = library.GetAllBlueprints().OfType<BlueprintFeatureSelection>();
            foreach (var s in selections)
            {
                s.AllFeatures = s.AllFeatures.RemoveFromArray(greate_cleave);
                s.Features = s.Features.RemoveFromArray(greate_cleave);
                s.AllFeatures = s.AllFeatures.RemoveFromArray(cleaving_finish_greater);
                s.Features = s.Features.RemoveFromArray(cleaving_finish_greater);
            }
        }


        static void fixVitalStrike()
        {
            var vital_strike = library.Get<BlueprintFeature>("14a1fc1356df9f146900e1e42142fc9d");
            var improved_vital_strike = library.Get<BlueprintFeature>("52913092cd018da47845f36e6fbe240f");
            var greater_vital_strike = library.Get<BlueprintFeature>("e2d1fa11f6b095e4fb2fd1dcf5e36eb3");
            var vital_strike_ability = library.Get<BlueprintAbility>("efc60c91b8e64f244b95c66b270dbd7c");

            var selections = library.GetAllBlueprints().OfType<BlueprintFeatureSelection>();

            foreach (var s in selections)
            {
                s.AllFeatures = s.AllFeatures.RemoveFromArray(improved_vital_strike).RemoveFromArray(greater_vital_strike);
                s.Features = s.Features.RemoveFromArray(improved_vital_strike).RemoveFromArray(greater_vital_strike);
            }

            vital_strike.SetDescription("You make a single attack that deals significantly more damage than normal.\nBenefit: As a standard action, you can make one attack at your highest base attack bonus that deals additional damage. Roll the weapon's damage dice for the attack twice (three times once your BAB reaches 11, four times once your BAB reaches 16) and add the results together before adding bonuses from Strength, weapon abilities (such as flaming), precision-based damage, and other damage bonuses. These extra weapon damage dice are not multiplied on a critical hit, but are added to the total.");
            vital_strike_ability.SetDescription(vital_strike.Description);
        }


        static void fixTwf()
        {
            //two weapon fighting now allows you to make up to 3 three iterative attacks if your bab allows it
            var twf = library.Get<BlueprintFeature>("ac8aaf29054f5b74eb18f2af950e752d");
            var itwf = library.Get<BlueprintFeature>("c126adbdf6ddd8245bda33694cd774e8");
            var gtwf = library.Get<BlueprintFeature>("9af88f3ed8a017b45a6837eab7437629");

            //remove itwf and gtwf from selections
            var selections = library.GetAllBlueprints().OfType<BlueprintFeatureSelection>();

            foreach (var s in selections)
            {
                s.AllFeatures = s.AllFeatures.RemoveFromArray(itwf).RemoveFromArray(gtwf);
                s.Features = s.Features.RemoveFromArray(itwf).RemoveFromArray(gtwf);
            }

            var twf_basic_mechanics = library.Get<BlueprintFeature>("6948b379c0562714d9f6d58ccbfa8faa");
            twf_basic_mechanics.ReplaceComponent<TwoWeaponFightingAttacks>(Helpers.Create<IterativeTwoWeaponFightingAttacks>());
        }

        static void fixMonkAc()
        {
            //fix monk ac bonuses
            var monk = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var monk_ac_unlock = library.Get<BlueprintFeature>("2615c5f87b3d72b42ac0e73b56d895e0");
            monk_ac_unlock.SetDescription("When unarmored and unencumbered, the monk adds his Wisdom bonus (if any, up to his monk level) to his AC and CMD. In addition, a monk gains a + 1 bonus to AC and CMD at 4th level. This bonus increases by 1 for every four monk levels thereafter, up to a maximum of + 5 at 20th level.\nThese bonuses to AC apply even against touch attacks or when the monk is flat - footed. He loses these bonuses when he is immobilized or helpless, when he wears any armor, when he carries a shield, or when he carries a medium or heavy load.");
            var monk_ac = library.Get<BlueprintFeature>("e241bdfd6333b9843a7bfd674d607ac4");
            var monk_ac_property = NewMechanics.ContextValueWithLimitProperty.createProperty("MonkAcProperty", "b8ba561529dc4143b014994ea3f234fe",
                                                                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus,
                                                                                                                              stat: Kingmaker.EntitySystem.Stats.StatType.Wisdom,
                                                                                                                              min: 0,
                                                                                                                              type: AbilityRankType.Default),
                                                                                              Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                                                              classes: new BlueprintCharacterClass[] { monk },
                                                                                                                              min: 0,
                                                                                                                              type: AbilityRankType.DamageDiceAlternative),
                                                                                              monk_ac
                                                                                              );

            foreach (var c in monk_ac.GetComponents<ContextRankConfig>().ToArray())
            {
                if (c.IsBasedOnStatBonus)
                {
                    var new_c = Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, customProperty: monk_ac_property, type: c.Type);
                    monk_ac.ReplaceComponent(c, new_c);
                }
            }

            var scaled_fist = library.Get<BlueprintArchetype>("5868fc82eb11a4244926363983897279");
            var scaled_fist_ac_unlock = library.Get<BlueprintFeature>("2a8922e28b3eba54fa7a244f7b05bd9e");
            scaled_fist_ac_unlock.SetDescription("When unarmored and unencumbered, the monk adds his Charisma bonus (if any, up to his monk level) to his AC and CMD. In addition, a monk gains a +1 bonus to AC and CMD at 4th level. This bonus increases by 1 for every four monk levels thereafter, up to a maximum of +5 at 20th level.\nThese bonuses to AC apply even against touch attacks or when the monk is flat-footed. He loses these bonuses when he is immobilized or helpless, when he wears any armor, when he carries a shield, or when he carries a medium or heavy load.");
            var scaled_fist_ac = library.Get<BlueprintFeature>("3929bfd1beeeed243970c9fc0cf333f8");
            var sf_ac_property = NewMechanics.ContextValueWithLimitProperty.createProperty("ScaledFistAcProperty", "d58a65251b2c4ce9829ae82ca896861d",
                                                                                  Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus,
                                                                                                                  stat: Kingmaker.EntitySystem.Stats.StatType.Charisma,
                                                                                                                  min: 0,
                                                                                                                  type: AbilityRankType.Default),
                                                                                  Helpers.CreateContextRankConfig(ContextRankBaseValueType.SummClassLevelWithArchetype,
                                                                                                                  archetype: scaled_fist,
                                                                                                                  classes: new BlueprintCharacterClass[] { monk },
                                                                                                                  min: 0,
                                                                                                                  type: AbilityRankType.DamageDiceAlternative),
                                                                                   scaled_fist_ac
                                                                                  );

            foreach (var c in scaled_fist_ac.GetComponents<ContextRankConfig>().ToArray())
            {
                if (c.IsBasedOnStatBonus)
                {
                    var new_c = Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, customProperty: sf_ac_property, type: c.Type);
                    scaled_fist_ac.ReplaceComponent(c, new_c);
                }
            }

            scaled_fist_ac.SetDescription("");
        }


        static void fixPaladinSaves()
        {
            //fix paladin saves
            var divine_grace = library.Get<BlueprintFeature>("8a5b5e272e5c34e41aa8b4facbb746d3");
            var paladin = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            var paladin_saves_property = NewMechanics.ContextValueWithLimitProperty.createProperty("DivineGraceSavesProperty", "97e5940097ed4f2f989d531bda77df2f",
                                                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus,
                                                                                                      stat: Kingmaker.EntitySystem.Stats.StatType.Charisma,
                                                                                                      min: 0,
                                                                                                      type: AbilityRankType.DamageDice),
                                                                      Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                                      classes: new BlueprintCharacterClass[] { paladin },
                                                                                                      min: 0,
                                                                                                      type: AbilityRankType.DamageDiceAlternative),
                                                                      divine_grace
                                                                      );

            divine_grace.RemoveComponents<DerivativeStatBonus>();
            divine_grace.AddComponents(Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.SaveFortitude, ModifierDescriptor.UntypedStackable),
                                       Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.SaveReflex, ModifierDescriptor.UntypedStackable),
                                       Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.SaveWill, ModifierDescriptor.UntypedStackable),
                                       Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, customProperty: paladin_saves_property)
                                       );
            divine_grace.SetDescription("At 2nd level, a paladin gains a bonus equal to her Charisma bonus (if any, up to her paladin level) on all saving throws.");
        }


        static void fixSorcererBloodlineArcana()
        {
            var sorcerer = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
            var eldritch_scion = library.Get<BlueprintArchetype>("d078b2ef073f2814c9e338a789d97b73");

            var guids = new string[]
            {
                "5515ae09c952ae2449410ab3680462ed", //black
                "0f0cb88a2ccc0814aa64c41fd251e84e", //blue
                "153e9b6b5b0f34d45ae8e815838aca80", //brass
                "677ae97f60d26474bbc24a50520f9424", //bronze
                "2a8ed839d57f31a4983041645f5832e2", //copper
                "ac04aa27a6fd8b4409b024a6544c4928", //gold
                "caebe2fa3b5a94d4bbc19ccca86d1d6f", //green
                "a8baee8eb681d53438cc17bd1d125890", //red
                "1af96d3ab792e3048b5e0ca47f3a524b", //silver
                "456e305ebfec3204683b72a45467d87c", //white
            };

            var bloodline_classes = new BlueprintCharacterClass[] { library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf"), //sorcerer
                                                                   library.Get<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780")}; //magus

            foreach (var guid in guids)
            {
                var f = library.Get<BlueprintFeature>(guid);
                var arcana = f.GetComponent<DraconicBloodlineArcana>();
                f.ReplaceComponent(arcana,
                                   Helpers.Create<DraconicBloodlineArcanaClassSpells>(d =>
                                   {
                                       d.SpellDescriptor = arcana.SpellDescriptor;
                                       d.classes = bloodline_classes;
                                   }));
            }

            var arance_bloodline_arcana = library.Get<BlueprintFeature>("e8e4f56618dd8b04490aa6a0b75ac24f");
            arance_bloodline_arcana.ComponentsArray = new BlueprintComponent[] { Helpers.Create<ArcaneBloodlineArcanaOnSpecificClass>(a => { a.classes = bloodline_classes; }) };


            //reduce DC bonuses from fey and infernal bloodline to 1 and make them work only on sorcerer spells
            var arcanas = new BlueprintFeature[]{library.Get<BlueprintFeature>("67f151cdb9f0c9d4092ccfd5a9ccc6c9"),
                                                 library.Get<BlueprintFeature>("6fe1c3f9437d88749a07e294c08799b9")
                                                };

            foreach (var a in arcanas)
            {
                var increase_spell_descriptor = a.GetComponent<IncreaseSpellDescriptorDC>();
                a.ReplaceComponent(increase_spell_descriptor,
                                   Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellsDC>(c =>
                                                                                                   {
                                                                                                       c.specific_class = sorcerer;
                                                                                                       c.Value = 1;
                                                                                                       c.Descriptor = increase_spell_descriptor.Descriptor;
                                                                                                   }
                                                                                                 )
                                   );
                a.AddComponent(Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellsDC>(c =>
                                                                                                {
                                                                                                    c.spellbook = eldritch_scion.ReplaceSpellbook;
                                                                                                    c.Value = 1;
                                                                                                    c.Descriptor = increase_spell_descriptor.Descriptor;
                                                                                                }
                                                                                                 )
                                                                                            );
                a.SetDescription(a.Description.Replace("2", "1"));
            }


            var school_power_selection = library.Get<BlueprintFeatureSelection>("3524a71d57d99bb4b835ad20582cf613");
            var school_power_feature = library.Get<BlueprintParametrizedFeature>("8891303b67eb273428f1691864b08cf8");
            school_power_feature.GetComponent<SpellFocusParametrized>().BonusDC = 1;
            school_power_selection.SetDescription(school_power_selection.Description.Replace("2", "1"));
            school_power_feature.SetDescription(school_power_feature.Description.Replace("2", "1"));
        }

        static internal void fixFeats()
        {
            //remove pbs from precise shot feat
            var precise_shot = library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665");
            precise_shot.RemoveComponents<PrerequisiteFeature>();
        }


        static void manualFixes()
        {
            var buff_asset_ids = new string[]
            {//elemental assesor ids
                "19e4cb7ff0bef034e819ee27649cf8f8",
                "8f57c7d7bdf4d8e43a8efb51a7ffd306",
                "7389f35292798914cab1238d0f10e821",
                "8a34bbb5f5117924687a3c887478ad9f"
            };

            foreach (var s in buff_asset_ids)
            {
                var buff = library.Get<BlueprintBuff>(s);
                HashSet<DiceType> processed_local = new HashSet<DiceType>();
                fixSpellDamageBuff(buff, processed_local);
            }


            {//fix serenity
                HashSet<DiceType> processed_local = new HashSet<DiceType>();
                var serenity_buff = library.Get<BlueprintBuff>("0a90088dca676524ead26b1f7a368e62");
                serenity_buff.ReplaceComponent<AddInitiatorAttackRollTrigger>(a => a.Action = Helpers.CreateActionList(processActions(a.Action.Actions, new ContextCalculateSharedValue[0], processed_local)));
                var serenity = library.Get<BlueprintAbility>("d316d3d94d20c674db2c24d7de96f6a7");
                foreach (var p in processed_local)
                {
                    var old_dice = (int)p;
                    var new_dice = (int)dices_shift[p];
                    serenity.SetDescription(serenity.Description.Replace("d" + old_dice.ToString(), "dxxx" + new_dice.ToString()));
                }
                serenity.SetDescription(serenity.Description.Replace("dxxx", "d"));
            }

            //fix phoenix_blast
            {
                var area = library.Get<BlueprintAbilityAreaEffect>("f6d511dda2d97b84783df21af423c878");
                var feature = library.Get<BlueprintFeature>("9a1fc45d57567e946a20926f891453ed");

                HashSet<DiceType> processed_local = new HashSet<DiceType>();
                fixSpellDamageArea(area, processed_local);
                foreach (var p in processed_local)
                {
                    var old_dice = (int)p;
                    var new_dice = (int)dices_shift[p];
                    feature.SetDescription(feature.Description.Replace("d" + old_dice.ToString(), "dxxx" + new_dice.ToString()));
                }
                feature.SetDescription(feature.Description.Replace("dxxx", "d"));
            }
        }

        static public DiceType getDamageDie(DiceType dice)
        {
            if (dice < dices[0] || dice > dices.Last() || !Main.settings.balance_fixes)
            {
                return dice;
            }
            return dices_shift[dice];
        }

        static public string getDamageDieString(DiceType dice)
        {
            return ((int)getDamageDie(dice)).ToString();
        }



        static void fixLowerLevelBlasts()
        {
            var wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            //fix evocation 
            {
                var evocation_progression = library.Get<BlueprintProgression>("f8019b7724d72a241a97157bc37f1c3b");
                var evocation_base = library.Get<BlueprintFeature>("c46512b796216b64899f26301241e4e6");
                var force_missile_feature = library.Get<BlueprintFeature>("f9501f0df27af5446b705b5da255469a");
                var force_missile_ability = library.Get<BlueprintAbility>("3d55cc710cc497843bb51788057cd93f");

                var actions = force_missile_ability.GetComponent<AbilityEffectRunAction>().Actions.Actions;
                actions = Common.changeAction<ContextActionDealDamage>(actions, a =>
                {
                    a.Value = Helpers.CreateContextDiceValue(getDamageDie(DiceType.D4), Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default), 0);
                });
                force_missile_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(actions));
                force_missile_ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { wizard },
                                                                                   progression: ContextRankProgression.OnePlusDiv2));
                force_missile_ability.SetDescription($"As a standard action, you can unleash a force missile that automatically strikes a foe, as magic missile. The force missile deals 1d{getDamageDieString(DiceType.D4)} points of damage plus 1d{getDamageDieString(DiceType.D4)} points of damage per 2 wizard levels beyond first, plus the damage from your intense spells evocation power.[LONGSTART] This is a force effect. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.[LONGEND]");
                evocation_base.SetDescription($"Evokers revel in the raw power of magic, and can use it to create and destroy with shocking ease.\nIntense Spells: Whenever you cast an evocation spell that deals hit point damage, add 1/2 your wizard level to the damage (minimum +1). This bonus only applies once to a spell, not once per missile or ray, and cannot be split between multiple missiles or rays. This damage is of the same type as the spell. At 20th level, whenever you cast an evocation spell, you can roll twice to penetrate a creature's spell resistance and take the better result.\nForce Missile: As a standard action, you can unleash a force missile that automatically strikes a foe, as magic missile. The force missile deals 1d{getDamageDieString(DiceType.D4)} points of damage plus 1d{getDamageDieString(DiceType.D4)} points of damage per 2 wizard levels beyond first, plus the damage from your intense spells evocation power. This is a force effect. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.\nElemental Wall: At 8th level, one per day you can create a wall of energy. You can choose fire, cold, acid or electricity damage. The elemental wall otherwise functions like wall of fire. At 12th level and every 4 levels thereafter, you can use this ability one additional time per day, to a maximum of 4 times at level 20.");
                evocation_progression.SetDescription(evocation_base.Description);
            }

            //fix transmutation
            {
                var transmutation_progression = library.Get<BlueprintProgression>("b6a604dab356ac34788abf4ad79449ec");
                var transmutation_base = library.Get<BlueprintFeature>("c459c8200e666ef4c990873d3e501b91");
                var telekinetic_fist_feature = library.Get<BlueprintFeature>("e015d05ca18b5c24183cd062486fd46b");
                var telekinetic_fist_ability = library.Get<BlueprintAbility>("810992c76efdde84db707a0444cf9a1c");

                var actions = telekinetic_fist_ability.GetComponent<AbilityEffectRunAction>().Actions.Actions;
                actions = Common.changeAction<ContextActionDealDamage>(actions, a =>
                {
                    a.Value = Helpers.CreateContextDiceValue(getDamageDie(DiceType.D4), Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default), 0);
                });
                telekinetic_fist_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(actions));
                telekinetic_fist_ability.RemoveComponents<ContextRankConfig>();
                telekinetic_fist_ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { wizard },
                                                                                   progression: ContextRankProgression.OnePlusDiv2));
                telekinetic_fist_ability.SetDescription($"As a standard action, you can strike with a telekinetic fist, targeting any foe within 30 feet as a ranged touch attack. The telekinetic fist deals 1d{getDamageDieString(DiceType.D4)} points of bludgeoning damage plus 1d{getDamageDieString(DiceType.D4)} points of damage for every two wizard levels you possess beyond first.[LONGSTART] You can use this ability a number of times per day equal to 3 + your Intelligence modifier.[LONGEND]");
                telekinetic_fist_feature.SetDescription(telekinetic_fist_ability.Description);
                transmutation_base.SetDescription($"Transmuters use magic to change the world around them.\nPhysical Enhancement: You gain a +1 enhancement bonus to one physical ability score (Strength, Dexterity, or Constitution). This bonus increases by +1 for every five wizard levels you possess to a maximum of +5 at 20th level. At 20th level, this bonus applies to two physical ability scores of your choice.\nTelekinetic Fist: As a standard action, you can strike with a telekinetic fist, targeting any foe within 30 feet as a ranged touch attack. The telekinetic fist deals 1d{getDamageDieString(DiceType.D4)} points of bludgeoning damage plus 1d{getDamageDieString(DiceType.D4)} points of damage for every two wizard levels you possess beyond first. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.\nChange Shape: At 8th level, you can change your shape for a number of rounds per day equal to your wizard level. These rounds do not need to be consecutive. This ability otherwise functions like beast shape II or elemental body I. At 12th level, this ability functions like beast shape III or elemental body II.");
                transmutation_progression.SetDescription(transmutation_base.Description);
            }

            //fix conjuration
            {
                var conjuration_progression = library.Get<BlueprintProgression>("567801abe990faf4080df566fadcd038");
                var conjuration_base = library.Get<BlueprintFeature>("cee0f7edbd874a042952ee150f878b84");
                var acid_dart_feature = library.Get<BlueprintFeature>("b4b575d14c3bab6489aafa17ac2c909e");
                var acid_dart_ability = library.Get<BlueprintAbility>("697291ff99d3fbb448be5b60b5f2a30c");

                var actions = acid_dart_ability.GetComponent<AbilityEffectRunAction>().Actions.Actions;
                actions = Common.changeAction<ContextActionDealDamage>(actions, a =>
                {
                    a.Value = Helpers.CreateContextDiceValue(getDamageDie(DiceType.D6), Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default), 0);
                });
                acid_dart_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(actions));
                acid_dart_ability.RemoveComponents<ContextRankConfig>();
                acid_dart_ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { wizard },
                                                                                   progression: ContextRankProgression.OnePlusDiv2));
                acid_dart_ability.SetDescription($"As a standard action, you can unleash an acid dart targeting any foe within 30 feet as a ranged touch attack. The acid dart deals 1d{getDamageDieString(DiceType.D6)} points of acid damage plus 1d{getDamageDieString(DiceType.D6)} points of damage for every two wizard levels you possess beyond first. You can use this ability a number of times per day equal to 3 + your Intelligence modifier. This attack ignores spell resistance.");
                acid_dart_feature.SetDescription(acid_dart_ability.Description);
                conjuration_base.SetDescription($"The conjurer focuses on the study of summoning monsters and magic alike to bend to his will.\nSummoner's Charm: Whenever you cast a conjuration (summoning) spell, increase the duration by a number of rounds equal to 1/2 your wizard level (minimum 1). This increase is not doubled by Extend Spell.\nAcid Dart: As a standard action, you can unleash an acid dart targeting any foe within 30 feet as a ranged touch attack. The acid dart deals 1d{getDamageDieString(DiceType.D6)} points of acid damage plus 1d{getDamageDieString(DiceType.D6)} points of damage for every two wizard levels you possess beyond first. You can use this ability a number of times per day equal to 3 + your Intelligence modifier. This attack ignores spell resistance.\nDimensional Steps: At 8th level, you can use this ability to teleport up to 30 feet as a move action. You can use this ability a total number of times per day equal to your wizard level.");
                conjuration_progression.SetDescription(conjuration_base.Description);
            }

            //fix fire domain
            fixDomainAbility(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), library.Get<BlueprintProgression>("2bd6aa3c4979fd045bbbda8da586d3fb"),
                             library.Get<BlueprintProgression>("562567d7c244fae43ac61df3d55547ca"),
                             library.Get<BlueprintFeature>("42cc125d570c5334c89c6499b55fc0a3"), library.Get<BlueprintAbility>("4ecdf240d81533f47a5279f5075296b9"),
                             getDamageDie(DiceType.D6));

            //fix healing domain
            fixDomainAbility(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), library.Get<BlueprintProgression>("2bd6aa3c4979fd045bbbda8da586d3fb"),
                             library.Get<BlueprintProgression>("562567d7c244fae43ac61df3d55547ca"),
                             library.Get<BlueprintFeature>("42cc125d570c5334c89c6499b55fc0a3"), library.Get<BlueprintAbility>("4ecdf240d81533f47a5279f5075296b9"),
                             getDamageDie(DiceType.D6));

            //fix water domain
            fixDomainAbility(library.Get<BlueprintProgression>("e63d9133cebf2cf4788e61432a939084"), library.Get<BlueprintProgression>("f05fb4f417465f94fb1b4d6c48ea42cf"),
                             library.Get<BlueprintProgression>("e48425d6fdafdba449beec54fe158339"),
                             library.Get<BlueprintFeature>("4c21ad24f55f64d4fb722f40720d9ab0"), library.Get<BlueprintAbility>("5e1db2ef80ff361448549beeb7785791"),
                             DiceType.D8);

            //fix air domain
            fixDomainAbility(library.Get<BlueprintProgression>("750bfcd133cd52f42acbd4f7bc9cc365"), library.Get<BlueprintProgression>("d7169e8978d9e9d418398eab946c49e5"),
                             library.Get<BlueprintProgression>("3aef017b78329db4fa53fe8560069886"),
                             library.Get<BlueprintFeature>("39b0c7db785560041b436b558c9df2bb"), library.Get<BlueprintAbility>("b3494639791901e4db3eda6117ad878f"),
                             getDamageDie(DiceType.D6));

            //fix earth domain
            fixDomainAbility(library.Get<BlueprintProgression>("08bbcbfe5eb84604481f384c517ac800"), library.Get<BlueprintProgression>("4132a011b835a36479d6bc19a1b962e6"),
                             library.Get<BlueprintProgression>("a3217dc55003b914aa296da7ada029bc"),
                             library.Get<BlueprintFeature>("828d82a0e8c5a944bbdb6b12f802ff02"), library.Get<BlueprintAbility>("3ff40918d33219942929f0dbfe5d1dee"),
                             getDamageDie(DiceType.D6));

            //fix weather domain
            fixDomainAbility(library.Get<BlueprintProgression>("c18a821ee662db0439fb873165da25be"), library.Get<BlueprintProgression>("d124d29c7c96fc345943dd17e24990e8"),
                             library.Get<BlueprintProgression>("4a3516fdc4cda764ebd1279b22d10205"),
                             library.Get<BlueprintFeature>("1c37869ee06ca33459f16f23f4969e7d"), library.Get<BlueprintAbility>("f166325c271dd29449ba9f98d11542d9"),
                             getDamageDie(DiceType.D6));

            //fix rune domain base ability


            

            //fix healing doamin
            fixDomainAbility(library.Get<BlueprintProgression>("b0a26ee984b6b6945b884467aa2f1baa"), library.Get<BlueprintProgression>("599fb0d60358c354d8c5c4304a73e19a"),
                             library.Get<BlueprintProgression>("599fb0d60358c354d8c5c4304a73e19a"),
                             library.Get<BlueprintFeature>("303cf1c933f343c4d91212f8f4953e3c"), library.Get<BlueprintAbility>("18f734e40dd7966438ab32086c3574e1"),
                             getDamageDie(DiceType.D4), AbilityRankType.DamageBonus);

            //do not fix community domain, it is already powerful (just description)
            var community_domain_progressions = new BlueprintProgression[] { library.Get<BlueprintProgression>("b8bbe42616d61ac419971b7910d79fc8"), library.Get<BlueprintProgression>("3a397e27682edfd409cb73ff12de7c51") };
            foreach (var cdp in community_domain_progressions)
            {
                cdp.SetDescription(cdp.Description.Replace($"d6", $"d{getDamageDieString(DiceType.D6)}"));
            }

            //fix darkness greater ability description
            var darkness_domain_progressions = new BlueprintProgression[] { library.Get<BlueprintProgression>("1e1b4128290b11a41ba55280ede90d7d"),
                                                                            library.Get<BlueprintProgression>("57e97f4cda643734eb4800972c53b960"),
                                                                            library.Get<BlueprintProgression>("bfdc224a1362f2b4688b57f70adcc26f") };
            foreach (var ddp in darkness_domain_progressions)
            {
                ddp.SetDescription(ddp.Description.Replace("d10", $"d{getDamageDieString(DiceType.D10)}"));
                ddp.SetDescription(ddp.Description.Replace("d8", $"d{getDamageDieString(DiceType.D8)}"));
            }



            //fix runes
            {
                var base_ability = library.Get<BlueprintAbility>("56ad05dedfd9df84996f62108125eed5");
                base_ability.SetDescription(base_ability.Description.Replace("+ 1 point for every two levels you possess in the class that gave you access to this domain", $"plus 1d{getDamageDieString(DiceType.D6)} points of damage per two levels of the class that gave you access to this domain beyond first"));

                List<BlueprintAbilityAreaEffect> areas = new List<BlueprintAbilityAreaEffect>();
                foreach (var v in base_ability.Variants)
                {
                    v.SetDescription(base_ability.Description);
                    var area = (v.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnAreaEffect).AreaEffect;
                    area.ReplaceComponent<AbilityAreaEffectRunAction>(a => a.UnitEnter.Actions = Common.changeAction<ContextActionDealDamage>(a.UnitEnter.Actions,
                                                                                                                                              ca => ca.Value = Helpers.CreateContextDiceValue(getDamageDie(DiceType.D6), Helpers.CreateContextValue(AbilityRankType.DamageBonus), 0))
                                                                                                                                        );
                    area.AddComponent(v.GetComponent<ContextRankConfig>().CreateCopy());
                    areas.Add(area);
                }

                foreach (var v in base_ability.Variants)
                {
                    foreach (var a in areas)
                    {
                        v.AddComponent(Helpers.Create<NewMechanics.AbilityTargetPointDoesNotContainAreaEffect>(atp => atp.area_effect = a));
                    }
                }

                var base_feature = library.Get<BlueprintFeature>("b74c64a0152c7ee46b13ecdd72dda6f3");
                var domain = library.Get<BlueprintProgression>("6d4dac497c182754d8b1f49071cca3fd");
                var domain2 = library.Get<BlueprintProgression>("8d176f8fe5616a64ca37835be7c2ccfe");
                base_feature.SetDescription(base_feature.Description.Replace("+ 1 point for every two levels you possess in the class that gave you access to this domain", $"plus 1d{getDamageDieString(DiceType.D6)} points of damage per two levels of the class that gave you access to this domain beyond first"));
                domain.SetDescription(base_feature.Description);
                domain2.SetDescription(base_feature.Description);
            }
        }


        static void fixDomainAbility(BlueprintProgression cleric_progression,
                                     BlueprintProgression cleric_progression2,
                                     BlueprintProgression druid_progression,
                                     BlueprintFeature base_feature,
                                     BlueprintAbility base_ability,
                                     DiceType dice, AbilityRankType rank = AbilityRankType.Default)
        {
            var domain_classes = new BlueprintCharacterClass[]{library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0"), //cleric
                                                               library.Get<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce"), //inquisitor
                                                               library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96") //druid
            };

            var actions = base_ability.GetComponent<AbilityEffectRunAction>().Actions.Actions;
            actions = Common.changeAction<ContextActionDealDamage>(actions, a =>
            {
                a.Value = Helpers.CreateContextDiceValue(dice, Helpers.CreateContextValue(rank), 0);
            });

            actions = Common.changeAction<ContextActionHealTarget>(actions, a =>
            {
                a.Value = Helpers.CreateContextDiceValue(dice, Helpers.CreateContextValue(rank), 0);
            });

            base_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(actions));
            base_ability.RemoveComponents<ContextRankConfig>();
            base_ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: domain_classes, type: rank,
                                                                               progression: ContextRankProgression.OnePlusDiv2));
            base_ability.SetDescription(base_ability.Description.Replace("+ 1 point for every two levels you possess in the class that gave you access to this domain", $"plus 1d{(int)dice} points of damage per two levels of the class that gave you access to this domain beyond first"));
            base_feature.SetDescription(base_feature.Description.Replace("+ 1 point for every two levels you possess in the class that gave you access to this domain", $"plus 1d{(int)(dice)} points of damage per two levels of the class that gave you access to this domain beyond first"));
            druid_progression.SetDescription(base_feature.Description);
            cleric_progression.SetDescription(druid_progression.Description);
            cleric_progression2.SetDescription(cleric_progression.Description);
        }


        static string replaceString(string s, params DiceType[] dice)
        {
            var new_s = s;
            foreach (var d in dice)
            {
                var old_dice = (int)d;
                var new_dice = (int)dices_shift[d];
                new_s = new_s.Replace("d" + old_dice.ToString(), "dxxx" + new_dice.ToString());
            }
            return new_s.Replace("dxxx", "d");
        }

        static void manualTextFixes()
        {
            //channel energy selection for clerics
            //base kineticist blast description
            //base dragonkind I, II, III breath weapon description
            //kineticist diadems and icons
            var channel_energy_selection = library.Get<BlueprintFeatureSelection>("d332c1748445e8f4f9e92763123e31bd");
            channel_energy_selection.SetDescription(replaceString(channel_energy_selection.Description, DiceType.D6));

            var dragon_form1 = library.Get<BlueprintAbility>("f767399367df54645ac620ef7b2062bb");
            var dragon_form1_feature = library.Get<BlueprintFeature>("b10c54491148de84b858763588664a8a");
            dragon_form1.SetDescription(dragon_form1.Description.Replace("6d8", $"1d{getDamageDieString(DiceType.D8)}"));
            dragon_form1_feature.SetDescription(dragon_form1_feature.Description.Replace("6d8", $"6d{getDamageDieString(DiceType.D8)}"));

            var dragon_form2 = library.Get<BlueprintAbility>("666556ded3a32f34885e8c318c3a0ced");
            var dragon_form2_feature = library.Get<BlueprintFeature>("5b13008bdc18a0c4db321ae9f04ba77f");
            dragon_form2.SetDescription(dragon_form2.Description.Replace("8d8", $"8d{getDamageDieString(DiceType.D8)}"));
            dragon_form2_feature.SetDescription(dragon_form2_feature.Description.Replace("8d8", $"8d{getDamageDieString(DiceType.D8)}"));

            var dragon_form3 = library.Get<BlueprintAbility>("1cdc4ad4c208246419b98a35539eafa6");
            var dragon_form3_feature = library.Get<BlueprintFeature>("a5e3c6ee70f04c6489fd0bac3582db74");
            dragon_form3.SetDescription(dragon_form3.Description.Replace("12d8", $"12d{getDamageDieString(DiceType.D8)}"));
            dragon_form3_feature.SetDescription(dragon_form3_feature.Description.Replace("12d8", $"12d{getDamageDieString(DiceType.D8)}"));

            var kinetic_blast_feature = library.Get<BlueprintFeature>("93efbde2764b5504e98e6824cab3d27c");
            kinetic_blast_feature.SetDescription(replaceString(kinetic_blast_feature.Description, DiceType.D6));

            var kinetic_blast_progression = library.Get<BlueprintProgression>("30a5b8cf728bd4a4d8d90fc4953e322e");
            kinetic_blast_progression.SetDescription(replaceString(kinetic_blast_progression.Description, DiceType.D6));


            var kinetic_diadem_lesser = library.Get<BlueprintItemEquipmentHead>("2c46e4062d47c5243922e60eb5ab3df1");
            var kinetic_diadem = library.Get<BlueprintItemEquipmentHead>("cd6b1dda38c5b614cb428969770bb08d");
            var phylacteries = new BlueprintItem[]
            {
                library.Get<BlueprintItemEquipmentHead>("7ccc1833353d9a64cb671c7bc747bade"),
                library.Get<BlueprintItemEquipmentHead>("9751f7b073f740b458ab170cc81f5f1b"),
                library.Get<BlueprintItemEquipmentHead>("01bc7670bd0225745a66957f21f1ba23"),
                library.Get<BlueprintItemEquipmentHead>("6b50bf738b4e6964b916539efc582a9d"),
            };

            kinetic_diadem.SetDescription(replaceString(kinetic_diadem.Description, DiceType.D6));
            kinetic_diadem_lesser.SetDescription(replaceString(kinetic_diadem_lesser.Description, DiceType.D6));

            foreach (var p in phylacteries)
            {
                p.SetDescription(replaceString(p.Description, DiceType.D6));
            }

            var alchemist_bombs = library.Get<BlueprintFeature>("c59b2f256f5a70a4d896568658315b7d");
            alchemist_bombs.SetDescription(replaceString(alchemist_bombs.Description, DiceType.D6));

            var firebelly = library.Get<BlueprintAbility>("b065231094a21d14dbf1c3832f776871");
            var firebelly_buff = library.Get<BlueprintBuff>("7c33de68880aa444bbb916271b653016");
            firebelly.SetDescription(firebelly.Description.Replace("1d4", $"1d{getDamageDieString(DiceType.D4)}"));
            firebelly_buff.SetDescription(firebelly_buff.Description.Replace("1d4", $"1d{getDamageDieString(DiceType.D4)}"));

            var arcane_bomber_bomb_selection = library.Get<BlueprintFeatureSelection>("0c7a9899e27cae6449ee1fb3049f128d");
            arcane_bomber_bomb_selection.SetDescription(replaceString(arcane_bomber_bomb_selection.Description, DiceType.D6));

            var cave_fangs = library.Get<BlueprintAbility>("bacba2ff48d498b46b86384053945e83");
            var cave_fangs_abilities = new BlueprintAbility[] { cave_fangs }.AddToArray(cave_fangs.Variants);

            foreach (var ca in cave_fangs_abilities)
            {
                ca.SetDescription(replaceString(ca.Description, DiceType.D8));
            }


            //firebrand
            var firebrand_buff = library.Get<BlueprintBuff>("c6cc1c5356db4674dbd2be20ea205c86");
            var firebrand_ability = library.Get<BlueprintAbility>("98734a2665c18cd4db71878b0532024a");
            var firebrand_bonus_spell = library.Get<BlueprintFeature>("f10e60b5fa889e44585e4e48d0256a42");
            firebrand_buff.SetDescription(replaceString(firebrand_buff.Description, DiceType.D6));
            firebrand_ability.SetDescription(replaceString(firebrand_ability.Description, DiceType.D6));
            firebrand_bonus_spell.SetDescription(replaceString(firebrand_bonus_spell.Description, DiceType.D6));

            //scaled fist draconic heritage 
            var sf_draconic_heritage = library.Get<BlueprintFeatureSelection>("f9042eed12dac2745a2eb7a9a936906b");
            sf_draconic_heritage.SetDescription(replaceString(sf_draconic_heritage.Description, DiceType.D6));

            //acerbic ring
            var acerbic_ring = library.Get<BlueprintItemEquipmentRing>("1f34a6b309907a44681c689709976bff");
            acerbic_ring.SetDescription(replaceString(acerbic_ring.Description, DiceType.D6));

            var acid_splash = library.Get<BlueprintAbility>("0c852a2405dd9f14a8bbcfaf245ff823");
            acid_splash.SetDescription(acid_splash.Description.Replace("3", getDamageDieString(DiceType.D3)));

            //fix changed dexterity damage description
            var polar_midnight = library.Get<BlueprintAbility>("ba48abb52b142164eba309fd09898856");
            polar_midnight.SetDescription(polar_midnight.Description.Replace($"1d{getDamageDieString(DiceType.D6)}", "1d6"));
        }

       

        static void fixFeatures()
        {
            var features = library.GetAllBlueprints().OfType<BlueprintFeature>();
            foreach (var f in features)
            {
                HashSet<DiceType> processed_local = new HashSet<DiceType>();
                processFeature(f, processed_local);
            }
        }


        static void processFeature(BlueprintFeature f, HashSet<DiceType> processed)
        {
            if (processed_facts.ContainsKey(f))
            {
                processed.AddRange(processed_facts[f]);
                return;
            }
            HashSet<DiceType> processed_local = new HashSet<DiceType>();
            var learn_spell = f.GetComponents<AddKnownSpell>();
            foreach (var ls in learn_spell)
            {
                if (processed_abilities.ContainsKey(ls.Spell))
                {
                    processed_local.AddRange(processed_abilities[ls.Spell]);
                }
            }

            var add_facts = f.GetComponents<AddFacts>();
            foreach (var af in add_facts)
            {
                foreach (var ff in af.Facts.OfType<BlueprintAbility>())
                {
                    if (processed_abilities.ContainsKey(ff))
                    {
                        processed_local.AddRange(processed_abilities[ff]);
                    }
                }
            }

            var add_feature_if_has_fact = f.GetComponents<AddFeatureIfHasFact>();
            foreach (var af in add_feature_if_has_fact)
            {
                var fff = af.Feature as BlueprintFeature;
                if (fff != null)
                {
                    processFeature(fff, processed_local);
                }
            }

            processed_facts.Add(f, processed_local);

            if (processed_local.Empty())
            {
                return;
            }

            foreach (var p in processed_local)
            {
                var old_dice = (int)p;
                var new_dice = (int)dices_shift[p];
                f.SetDescription(f.Description.Replace("d" + old_dice.ToString(), "dxxx" + new_dice.ToString()));
            }
            f.SetDescription(f.Description.Replace("dxxx", "d"));
            Main.logger.Log("Fixed damage for: " + f.name);
            processed.AddRange(processed_local);
        }


        static void fixSpellDamageArea(BlueprintAbilityAreaEffect area, HashSet<DiceType> processed)
        {
            if (black_list.Contains(area.AssetGuid))
            {
                return;
            }

            if (processed_areas.Contains(area))
            {
                return;
            }

            var area_effect_buff = area.GetComponent<AbilityAreaEffectBuff>();

            if (area.GetComponent<AbilityAreaEffectRunAction>() == null && area_effect_buff == null)
            {
                return;
            }

            processed_areas.Add(area);
            HashSet<DiceType> processed_local = new HashSet<DiceType>();

            if (area_effect_buff?.Buff != null)
            {
                fixSpellDamageBuff(area_effect_buff.Buff, processed_local);
            }

            var run_actions_array = new GameAction[][] { area.GetComponent<AbilityAreaEffectRunAction>()?.Round?.Actions, area.GetComponent<AbilityAreaEffectRunAction>()?.UnitEnter?.Actions, area.GetComponent<AbilityAreaEffectRunAction>()?.UnitExit?.Actions, area.GetComponent<AbilityAreaEffectRunAction>()?.UnitMove?.Actions };
            for (int i = 0; i < run_actions_array.Length; i++)
            {
                var run_actions = run_actions_array[i];
                if (run_actions == null)
                {
                    continue;
                }
                var new_actions = processActions(run_actions, area.GetComponents<ContextCalculateSharedValue>().ToArray(), processed_local);
                run_actions_array[i] = new_actions;
            }

            if (processed_local.Empty())
            {
                return;
            }

            area.MaybeReplaceComponent<AbilityAreaEffectRunAction>(af =>
            {
                af.Round.Actions = run_actions_array[0];
                af.UnitEnter.Actions = run_actions_array[1];
                af.UnitExit.Actions = run_actions_array[2];
                af.UnitMove.Actions = run_actions_array[3];
            }
            );
            processed.AddRange(processed_local);

            Main.logger.Log("Fixed damage for: " + area.name);
        }

        static void fixSpellDamageAbilities()
        {
            var abilities = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(a => !a.HasVariants);
                 
            foreach (var a in abilities)
            {
                HashSet<DiceType> processed = new HashSet<DiceType>();
                if (processed_abilities.ContainsKey(a))
                {
                    continue;
                }

                if (a.StickyTouch != null )
                {
                    if (!processed_abilities.ContainsKey(a.StickyTouch.TouchDeliveryAbility))
                    {
                        fixSpellDamageAbility(a.StickyTouch.TouchDeliveryAbility, processed);
                    }


                    processed = processed_abilities[a.StickyTouch.TouchDeliveryAbility];
                    processed_abilities.Add(a, processed);
                    if (processed.Empty())
                    {
                        continue;
                    }
                    foreach (var p in processed)
                    {
                        var old_dice = (int)p;
                        var new_dice = (int)dices_shift[p];
                        a.SetDescription(a.Description.Replace("d" + old_dice.ToString(), "dxxx" + new_dice.ToString()));
                    }
                    a.SetDescription(a.Description.Replace("dxxx", "d"));
                    Main.logger.Log("Fixed damage for: " + a.name);
                }
                else
                {
                    fixSpellDamageAbility(a, processed);
                }
            }

            var abilities_wit_variants = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(a => a.HasVariants);
            foreach (var a in abilities_wit_variants)
            {
                HashSet<DiceType> processed = new HashSet<DiceType>();
                foreach (var v in a.Variants)
                {
                    if (processed_abilities.ContainsKey(v))
                    {
                        processed.AddRange(processed_abilities[v]);
                    }
                }
                if (processed.Empty())
                {
                    continue;
                }

                foreach (var p in processed)
                {
                    var old_dice = (int)p;
                    var new_dice = (int)dices_shift[p];
                    a.SetDescription(a.Description.Replace("d" + old_dice.ToString(), "dxxx" + new_dice.ToString()));
                }
                a.SetDescription(a.Description.Replace("dxxx", "d"));
                Main.logger.Log("Fixed damage for: " + a.name);
            }
        }


        static void fixSpellDamageAbility(BlueprintAbility ability, HashSet<DiceType> processed)
        {           
            if (black_list.Contains(ability.AssetGuid))
            {
                return;
            }

            if (processed_abilities.ContainsKey(ability))
            {
                return;
            }
            processed_abilities.Add(ability, processed);

            HashSet<DiceType> processed_local = new HashSet<DiceType>();

            var ability_kineticist = ability.GetComponent<AbilityKineticist>();
            if (ability_kineticist != null)
            {
                foreach (var cdi in ability_kineticist.CachedDamageInfo)
                {
                    var dice_value = Helpers.GetField<ContextDiceValue>(cdi, "Value");

                    if (dice_value.DiceType < dices[0] || dice_value.DiceType > dices.Last())
                    {
                        continue;
                    }
                    processed_local.Add(dice_value.DiceType);
                    dice_value.DiceType = dices_shift[dice_value.DiceType];
                }
            }
            
            var run_actions = ability.GetComponent<AbilityEffectRunAction>()?.Actions?.Actions;
            var clicked_actions = ability.GetComponent<AbilityEffectRunActionOnClickedTarget>()?.Action?.Actions;
            if (run_actions == null && clicked_actions == null)
            {
                return;
            }
            if (run_actions != null)
            {
                run_actions = processActions(run_actions, ability.GetComponents<ContextCalculateSharedValue>().ToArray(), processed_local);
            }
            if (clicked_actions != null)
            {
                clicked_actions = processActions(clicked_actions, ability.GetComponents<ContextCalculateSharedValue>().ToArray(), processed_local);
            }

            if (processed_local.Empty())
            {
                return;
            }

            processed.AddRange(processed_local);

            ability.MaybeReplaceComponent<AbilityEffectRunAction>(ra => ra.Actions = Helpers.CreateActionList(run_actions));
            ability.MaybeReplaceComponent<AbilityEffectRunActionOnClickedTarget>(ra => ra.Action = Helpers.CreateActionList(clicked_actions));

            foreach (var p in processed_local)
            {
                var old_dice = (int)p;
                var new_dice = (int)dices_shift[p];              
                ability.SetDescription(ability.Description.Replace("d" + old_dice.ToString(), "dxxx" + new_dice.ToString()));
            }
            ability.SetDescription(ability.Description.Replace("dxxx", "d"));

            Main.logger.Log("Fixed damage for: " + ability.name);
        }


        static void fixSpellDamageBuff(BlueprintBuff buff, HashSet<DiceType> processed)
        {
            if (buff == null)
            {
                return;
            }

            if (buff.GetComponent<AddFactContextActions>() == null && buff.GetComponent<AddFallProneTrigger>() == null && buff.GetComponent<AddOffensiveActionTrigger>() == null)
            {
                return;
            }

            if (processed_buffs.Contains(buff))
            {
                return;
            }
            HashSet<DiceType> processed_local = new HashSet<DiceType>();

            processed_buffs.Add(buff);

            var run_actions_array = new GameAction[][] { buff.GetComponent<AddFactContextActions>()?.Activated?.Actions,
                                                         buff.GetComponent<AddFactContextActions>()?.Deactivated?.Actions,
                                                         buff.GetComponent<AddFactContextActions>()?.NewRound?.Actions,
                                                         buff.GetComponent<AddFallProneTrigger>()?.Action?.Actions,
                                                         buff.GetComponent<AddOffensiveActionTrigger>()?.Action?.Actions};
            for (int i = 0;  i < run_actions_array.Length; i++)
            {
                var run_actions = run_actions_array[i];
                if (run_actions == null)
                {
                    continue;
                }
                var new_actions = processActions(run_actions, buff.GetComponents<ContextCalculateSharedValue>().ToArray(), processed_local);
                run_actions_array[i] = new_actions;
            }

            if (processed_local.Empty())
            {
                return;
            }

            buff.MaybeReplaceComponent<AddFactContextActions>(af =>
            {
                af.Activated.Actions = run_actions_array[0];
                af.Deactivated.Actions = run_actions_array[1];
                af.NewRound.Actions = run_actions_array[2];
            }
            );

            buff.MaybeReplaceComponent<AddFallProneTrigger>(af =>
            {
                af.Action = Helpers.CreateActionList(run_actions_array[3]);
            }
            );

            buff.MaybeReplaceComponent<AddOffensiveActionTrigger>(af =>
            {
                af.Action = Helpers.CreateActionList(run_actions_array[4]);
            }
);

            processed.AddRange(processed_local);
            foreach (var p in processed_local)
            {
                var old_dice = (int)p;
                var new_dice = (int)dices_shift[p];
                buff.SetDescription(buff.Description.Replace("d" + old_dice.ToString(), "dxxx" + new_dice.ToString()));                    
            }
            buff.SetDescription(buff.Description.Replace("dxxx", "d"));
            Main.logger.Log("Fixed damage for: " + buff.name);
        }



        static GameAction[] processActions(GameAction[] run_actions, ContextCalculateSharedValue[] shared_values, HashSet<DiceType> processed)
        {
            if (run_actions == null)
            {
                return run_actions;
            }
            var shared_values2 = shared_values.ToList();
            var new_actions = Common.changeAction<ContextActionDealDamage>(run_actions, c =>
            {
                if (Helpers.GetField<int>(c, "m_Type") != 0)
                {
                    return;
                }

                var dice_value = new ContextDiceValue[] { c.Value };
                if (c.Value.BonusValue.IsValueShared)
                {
                    var sh2 = shared_values2.Where(cc => cc.ValueType == c.Value.BonusValue.ValueShared).FirstOrDefault();
                    if (sh2 != null)
                    {
                        dice_value = dice_value.AddToArray(sh2.Value);
                        shared_values2.Remove(sh2);
                    }
                }
                foreach (var v in dice_value)
                {
                    if (v.DiceCountValue == null || (v.DiceCountValue.Value == 0 && v.DiceCountValue.IsValueSimple))
                    {
                        continue;
                    }

                    if (v.DiceType > DiceType.D8 || v.DiceType < DiceType.D3)
                    {
                        continue;
                    }

                    processed.Add(v.DiceType);
                    v.DiceType = dices_shift[v.DiceType];
                }

            }
            );


            new_actions = Common.changeAction<ContextActionHealTarget>(new_actions, c =>
            {

                if (c.Value.DiceCountValue == null || (c.Value.DiceCountValue.Value == 0 && c.Value.DiceCountValue.IsValueSimple))
                {
                    return;
                }

                if (c.Value.DiceType >= DiceType.D10 || c.Value.DiceType < DiceType.D3)
                {
                    return;
                }

                processed.Add(c.Value.DiceType);
                c.Value.DiceType = dices_shift[c.Value.DiceType];
            }
            );


            new_actions = Common.changeAction<ContextActionApplyBuff>(new_actions, c =>
            {
                fixSpellDamageBuff(c.Buff, processed);
            }
            );

            new_actions = Common.changeAction<ContextActionSpawnAreaEffect>(new_actions, c =>
            {
                fixSpellDamageArea(c.AreaEffect, processed);
            }
            );


            return new_actions;
        }
    }


    public class DraconicBloodlineArcanaClassSpells : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber
    {
        public SpellDescriptorWrapper SpellDescriptor;
        public BlueprintCharacterClass[] classes;

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            MechanicsContext context = evt.Reason.Context;
            if ((Object)context?.SourceAbility == (Object)null || !context.SpellDescriptor.HasAnyFlag((Kingmaker.Blueprints.Classes.Spells.SpellDescriptor)this.SpellDescriptor) || !context.SourceAbility.IsSpell )
                return;

            var spellbook = context.SourceAbilityContext?.Ability?.Spellbook;
            if (spellbook == null)
            {
                return;
            }
            spellbook = SpellbookMechanics.Helpers.getClassSpellbook(spellbook, this.Owner);
            bool spellbook_ok = false;

            foreach (var c in classes)
            {
                var sb = this.Owner.GetSpellbook(c);
                if (sb == spellbook)
                {
                    spellbook_ok = true;
                    break;
                }
            }


            if (!spellbook_ok)
            {
                return;
            }
            foreach (BaseDamage baseDamage in evt.DamageBundle)
                baseDamage.AddBonus(baseDamage.Dice.Rolls);
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
        }
    }


    public class IterativeTwoWeaponFightingAttacks : RuleInitiatorLogicComponent<RuleCalculateAttacksCount>
    {        
        public override void OnEventAboutToTrigger(RuleCalculateAttacksCount evt)
        {
            if (!evt.Initiator.Body.PrimaryHand.HasWeapon || !evt.Initiator.Body.SecondaryHand.HasWeapon || (evt.Initiator.Body.PrimaryHand.Weapon.Blueprint.IsNatural || evt.Initiator.Body.SecondaryHand.Weapon.Blueprint.IsNatural) || (evt.Initiator.Body.PrimaryHand.Weapon == evt.Initiator.Body.EmptyHandWeapon || evt.Initiator.Body.SecondaryHand.Weapon == evt.Initiator.Body.EmptyHandWeapon))
                return;

            var bab = this.Owner.Stats.BaseAttackBonus.ModifiedValue;
            if (bab > 5)
                ++evt.SecondaryHand.PenalizedAttacks;
            if (bab > 10)
                ++evt.SecondaryHand.PenalizedAttacks;
        }

        public override void OnEventDidTrigger(RuleCalculateAttacksCount evt)
        {
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ArcaneBloodlineArcanaOnSpecificClass : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public BlueprintCharacterClass[] classes;

        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (!((Object)evt.Spell != (Object)null) || evt.Spellbook == null || evt.Spell.Type != AbilityType.Spell)
                return;

            var spellbook = SpellbookMechanics.Helpers.getClassSpellbook(evt.Spellbook, this.Owner);
            bool spellbook_ok = false;

            foreach (var c in classes)
            {
                var sb = this.Owner.GetSpellbook(c);
                if (sb == spellbook)
                {
                    spellbook_ok = true;
                    break;
                }
            }
            if (!spellbook_ok)
            {
                return;
            }

            AbilityData abilityData = evt.AbilityData;
            if (((object)abilityData != null ? abilityData.MetamagicData : (MetamagicData)null) == null || !evt.AbilityData.MetamagicData.NotEmpty || evt.AbilityData.HasMetamagic(Metamagic.Heighten))
                return;
            evt.AddBonusDC(1);
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
