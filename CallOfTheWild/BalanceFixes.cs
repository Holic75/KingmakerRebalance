using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
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
        static internal void fixSpellDamage(params string[]  guids_black_list)
        {
            black_list = new HashSet<string>();
            black_list.AddRange(guids_black_list);
            fixSpellDamageAbilities();
            fixFeatures();
            manualTextFixes();
            fixLowerLevelBlasts();
            manualFixes();
            //fix manually:
            //channel energy selection for clerics
            //lesser elemental/rune domain abilities
            //base kineticist blast description
            //base dragonkind I, II, III breath weapon description
            //base school abilities description
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
                    a.Value = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default), 0);
                });
                force_missile_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(actions));
                force_missile_ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { wizard },
                                                                                   progression: ContextRankProgression.OnePlusDiv2));
                force_missile_ability.SetDescription("As a standard action, you can unleash a force missile that automatically strikes a foe, as magic missile. The force missile deals 1d6 points of damage plus 1d6 points of damage per 2 wizard levels beyond the first, plus the damage from your intense spells evocation power.[LONGSTART] This is a force effect. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.[LONGEND]");
                evocation_base.SetDescription("Evokers revel in the raw power of magic, and can use it to create and destroy with shocking ease.\nIntense Spells: Whenever you cast an evocation spell that deals hit point damage, add 1/2 your wizard level to the damage (minimum +1). This bonus only applies once to a spell, not once per missile or ray, and cannot be split between multiple missiles or rays. This damage is of the same type as the spell. At 20th level, whenever you cast an evocation spell, you can roll twice to penetrate a creature's spell resistance and take the better result.\nForce Missile: As a standard action, you can unleash a force missile that automatically strikes a foe, as magic missile. The force missile deals 1d6 points of damage plus 1d6 points of damage per 2 wizard levels beyond the first, plus the damage from your intense spells evocation power. This is a force effect. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.\nElemental Wall: At 8th level, one per day you can create a wall of energy. You can choose fire, cold, acid or electricity damage. The elemental wall otherwise functions like wall of fire. At 12th level and every 4 levels thereafter, you can use this ability one additional time per day, to a maximum of 4 times at level 20.");
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
                    a.Value = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default), 0);
                });
                telekinetic_fist_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(actions));
                telekinetic_fist_ability.RemoveComponents<ContextRankConfig>();
                telekinetic_fist_ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { wizard },
                                                                                   progression: ContextRankProgression.OnePlusDiv2));
                telekinetic_fist_ability.SetDescription("As a standard action, you can strike with a telekinetic fist, targeting any foe within 30 feet as a ranged touch attack. The telekinetic fist deals 1d6 points of bludgeoning damage plus 1d6 points of damage for every two wizard levels you possess beyond the first.[LONGSTART] You can use this ability a number of times per day equal to 3 + your Intelligence modifier.[LONGEND]");
                telekinetic_fist_feature.SetDescription(telekinetic_fist_ability.Description);
                transmutation_base.SetDescription("Transmuters use magic to change the world around them.\nPhysical Enhancement: You gain a +1 enhancement bonus to one physical ability score (Strength, Dexterity, or Constitution). This bonus increases by +1 for every five wizard levels you possess to a maximum of +5 at 20th level. At 20th level, this bonus applies to two physical ability scores of your choice.\nTelekinetic Fist: As a standard action, you can strike with a telekinetic fist, targeting any foe within 30 feet as a ranged touch attack. The telekinetic fist deals 1d6 points of bludgeoning damage plus 1d6 points of damage for every two wizard levels you possess beyond the first. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.\nChange Shape: At 8th level, you can change your shape for a number of rounds per day equal to your wizard level. These rounds do not need to be consecutive. This ability otherwise functions like beast shape II or elemental body I. At 12th level, this ability functions like beast shape III or elemental body II.");
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
                    a.Value = Helpers.CreateContextDiceValue(DiceType.D8, Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.Default), 0);
                });
                acid_dart_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(actions));
                acid_dart_ability.RemoveComponents<ContextRankConfig>();
                acid_dart_ability.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { wizard },
                                                                                   progression: ContextRankProgression.OnePlusDiv2));
                acid_dart_ability.SetDescription("As a standard action, you can unleash an acid dart targeting any foe within 30 feet as a ranged touch attack. The acid dart deals 1d8 points of acid damage plus 1d8 points of damage for every two wizard levels you possess beyond the first. You can use this ability a number of times per day equal to 3 + your Intelligence modifier. This attack ignores spell resistance.");
                acid_dart_feature.SetDescription(acid_dart_ability.Description);
                conjuration_base.SetDescription("The conjurer focuses on the study of summoning monsters and magic alike to bend to his will.\nSummoner's Charm: Whenever you cast a conjuration (summoning) spell, increase the duration by a number of rounds equal to 1/2 your wizard level (minimum 1). This increase is not doubled by Extend Spell.\nAcid Dart: As a standard action, you can unleash an acid dart targeting any foe within 30 feet as a ranged touch attack. The acid dart deals 1d8 points of acid damage plus 1d8 points of damage for every two wizard levels you possess beyond the first. You can use this ability a number of times per day equal to 3 + your Intelligence modifier. This attack ignores spell resistance.\nDimensional Steps: At 8th level, you can use this ability to teleport up to 30 feet as a move action. You can use this ability a total number of times per day equal to your wizard level.");
                conjuration_progression.SetDescription(conjuration_base.Description);
            }

            //fix fire domain
            fixDomainAbility(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), library.Get<BlueprintProgression>("2bd6aa3c4979fd045bbbda8da586d3fb"),
                             library.Get<BlueprintProgression>("562567d7c244fae43ac61df3d55547ca"),
                             library.Get<BlueprintFeature>("42cc125d570c5334c89c6499b55fc0a3"), library.Get<BlueprintAbility>("4ecdf240d81533f47a5279f5075296b9"),
                             DiceType.D8);

            //fix healing domain
            fixDomainAbility(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), library.Get<BlueprintProgression>("2bd6aa3c4979fd045bbbda8da586d3fb"),
                             library.Get<BlueprintProgression>("562567d7c244fae43ac61df3d55547ca"),
                             library.Get<BlueprintFeature>("42cc125d570c5334c89c6499b55fc0a3"), library.Get<BlueprintAbility>("4ecdf240d81533f47a5279f5075296b9"),
                             DiceType.D8);

            //fix water domain
            fixDomainAbility(library.Get<BlueprintProgression>("e63d9133cebf2cf4788e61432a939084"), library.Get<BlueprintProgression>("f05fb4f417465f94fb1b4d6c48ea42cf"),
                             library.Get<BlueprintProgression>("e48425d6fdafdba449beec54fe158339"),
                             library.Get<BlueprintFeature>("4c21ad24f55f64d4fb722f40720d9ab0"), library.Get<BlueprintAbility>("5e1db2ef80ff361448549beeb7785791"),
                             DiceType.D8);

            //fix air domain
            fixDomainAbility(library.Get<BlueprintProgression>("750bfcd133cd52f42acbd4f7bc9cc365"), library.Get<BlueprintProgression>("d7169e8978d9e9d418398eab946c49e5"),
                             library.Get<BlueprintProgression>("3aef017b78329db4fa53fe8560069886"),
                             library.Get<BlueprintFeature>("39b0c7db785560041b436b558c9df2bb"), library.Get<BlueprintAbility>("b3494639791901e4db3eda6117ad878f"),
                             DiceType.D8);

            //fix earth domain
            fixDomainAbility(library.Get<BlueprintProgression>("08bbcbfe5eb84604481f384c517ac800"), library.Get<BlueprintProgression>("4132a011b835a36479d6bc19a1b962e6"),
                             library.Get<BlueprintProgression>("a3217dc55003b914aa296da7ada029bc"),
                             library.Get<BlueprintFeature>("828d82a0e8c5a944bbdb6b12f802ff02"), library.Get<BlueprintAbility>("3ff40918d33219942929f0dbfe5d1dee"),
                             DiceType.D8);

            //fix weather domain
            fixDomainAbility(library.Get<BlueprintProgression>("c18a821ee662db0439fb873165da25be"), library.Get<BlueprintProgression>("d124d29c7c96fc345943dd17e24990e8"),
                             library.Get<BlueprintProgression>("4a3516fdc4cda764ebd1279b22d10205"),
                             library.Get<BlueprintFeature>("1c37869ee06ca33459f16f23f4969e7d"), library.Get<BlueprintAbility>("f166325c271dd29449ba9f98d11542d9"),
                             DiceType.D8);

            //fix rune domain base ability


            

            //fix healing doamin
            fixDomainAbility(library.Get<BlueprintProgression>("b0a26ee984b6b6945b884467aa2f1baa"), library.Get<BlueprintProgression>("599fb0d60358c354d8c5c4304a73e19a"),
                             library.Get<BlueprintProgression>("599fb0d60358c354d8c5c4304a73e19a"),
                             library.Get<BlueprintFeature>("303cf1c933f343c4d91212f8f4953e3c"), library.Get<BlueprintAbility>("18f734e40dd7966438ab32086c3574e1"),
                             DiceType.D6, AbilityRankType.DamageBonus);

            //do not fix community domain, it is already powerful (just description)
            var community_domain_progressions = new BlueprintProgression[] { library.Get<BlueprintProgression>("b8bbe42616d61ac419971b7910d79fc8"), library.Get<BlueprintProgression>("3a397e27682edfd409cb73ff12de7c51") };
            foreach (var cdp in community_domain_progressions)
            {
                cdp.SetDescription(cdp.Description.Replace("d6", "d8"));
            }

            //fix darkness greater ability description
            var darkness_domain_progressions = new BlueprintProgression[] { library.Get<BlueprintProgression>("1e1b4128290b11a41ba55280ede90d7d"),
                                                                            library.Get<BlueprintProgression>("57e97f4cda643734eb4800972c53b960"),
                                                                            library.Get<BlueprintProgression>("bfdc224a1362f2b4688b57f70adcc26f") };
            foreach (var ddp in darkness_domain_progressions)
            {
                ddp.SetDescription(ddp.Description.Replace("d10", "d20"));
                ddp.SetDescription(ddp.Description.Replace("d8", "d12"));
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
            base_ability.SetDescription(base_ability.Description.Replace("+ 1 point for every two levels you possess in the class that gave you access to this domain", $"plus 1d{(int)dice} points of damage per two levels of the class that gave you access to this domain beyond the first"));
            base_feature.SetDescription(base_feature.Description.Replace("+ 1 point for every two levels you possess in the class that gave you access to this domain", $"plus 1d{(int)(dice)} points of damage per two levels of the class that gave you access to this domain beyond the first"));
            druid_progression.SetDescription(base_feature.Description);
            cleric_progression.SetDescription(druid_progression.Description);
            cleric_progression2.SetDescription(cleric_progression.Description);
        }


        static string replaceString(string s, params DiceType[] dice)
        {
            var new_s = s;
            foreach (var d in dices)
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
            dragon_form1.SetDescription(dragon_form1.Description.Replace("6d8", "6d12"));
            dragon_form1_feature.SetDescription(dragon_form1_feature.Description.Replace("6d8", "6d12"));

            var dragon_form2 = library.Get<BlueprintAbility>("666556ded3a32f34885e8c318c3a0ced");
            var dragon_form2_feature = library.Get<BlueprintFeature>("5b13008bdc18a0c4db321ae9f04ba77f");
            dragon_form2.SetDescription(dragon_form2.Description.Replace("8d8", "8d12"));
            dragon_form2_feature.SetDescription(dragon_form2_feature.Description.Replace("8d8", "8d12"));

            var dragon_form3 = library.Get<BlueprintAbility>("1cdc4ad4c208246419b98a35539eafa6");
            var dragon_form3_feature = library.Get<BlueprintFeature>("a5e3c6ee70f04c6489fd0bac3582db74");
            dragon_form3.SetDescription(dragon_form3.Description.Replace("12d8", "12d12"));
            dragon_form3_feature.SetDescription(dragon_form3_feature.Description.Replace("12d8", "12d12"));

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
            kinetic_diadem_lesser.SetDescription(replaceString(kinetic_diadem.Description, DiceType.D6));

            foreach (var p in phylacteries)
            {
                p.SetDescription(replaceString(p.Description, DiceType.D6));
            }

            var alchemist_bombs = library.Get<BlueprintFeature>("c59b2f256f5a70a4d896568658315b7d");
            alchemist_bombs.SetDescription(replaceString(alchemist_bombs.Description, DiceType.D6));
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
            Main.logger.Log("Processed:" + f.name);
            processed.AddRange(processed_local);
        }


        static void fixSpellDamageArea(BlueprintAbilityAreaEffect area, HashSet<DiceType> processed)
        {
            if (black_list.Contains(area.AssetGuid))
            {
                return;
            }
            if (area.GetComponent<AbilityAreaEffectRunAction>() == null)
            {
                return;
            }

            if (processed_areas.Contains(area))
            {
                return;
            }

           
            processed_areas.Add(area);
            HashSet<DiceType> processed_local = new HashSet<DiceType>();

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

            area.ReplaceComponent<AbilityAreaEffectRunAction>(af =>
            {
                af.Round.Actions = run_actions_array[0];
                af.UnitEnter.Actions = run_actions_array[1];
                af.UnitExit.Actions = run_actions_array[2];
                af.UnitMove.Actions = run_actions_array[2];
            }
            );
            processed.AddRange(processed_local);

            Main.logger.Log("Processed:" + area.name);
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
                    Main.logger.Log("Processed:" + a.name);
                }
                else
                {
                    fixSpellDamageAbility(a, processed);
                }
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
            if (run_actions == null)
            {
                return;
            }
            var new_actions = processActions(run_actions, ability.GetComponents<ContextCalculateSharedValue>().ToArray(), processed_local);

            if (processed_local.Empty())
            {
                return;
            }

            processed.AddRange(processed_local);

            ability.ReplaceComponent<AbilityEffectRunAction>(ra => ra.Actions = Helpers.CreateActionList(new_actions));

            foreach (var p in processed_local)
            {
                var old_dice = (int)p;
                var new_dice = (int)dices_shift[p];              
                ability.SetDescription(ability.Description.Replace("d" + old_dice.ToString(), "dxxx" + new_dice.ToString()));
            }
            ability.SetDescription(ability.Description.Replace("dxxx", "d"));

            Main.logger.Log("Processed:" + ability.name);
        }


        static void fixSpellDamageBuff(BlueprintBuff buff, HashSet<DiceType> processed)
        {
            if (buff == null)
            {
                return;
            }


            if (buff.GetComponent<AddFactContextActions>() == null)
            {
                return;
            }

            if (processed_buffs.Contains(buff))
            {
                return;
            }
            HashSet<DiceType> processed_local = new HashSet<DiceType>();

            processed_buffs.Add(buff);

            var run_actions_array = new GameAction[][] { buff.GetComponent<AddFactContextActions>()?.Activated?.Actions, buff.GetComponent<AddFactContextActions>()?.Deactivated?.Actions, buff.GetComponent<AddFactContextActions>()?.NewRound?.Actions };
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

            buff.ReplaceComponent<AddFactContextActions>(af =>
            {
                af.Activated.Actions = run_actions_array[0];
                af.Deactivated.Actions = run_actions_array[1];
                af.NewRound.Actions = run_actions_array[2];
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
            Main.logger.Log("Processed:" + buff.name);
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
}
