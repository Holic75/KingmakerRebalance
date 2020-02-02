using CallOfTheWild.NewMechanics.EnchantmentMechanics;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
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
using UnityEngine;

namespace CallOfTheWild
{
    public class MetamagicFeats
    {
        [Flags]
        public enum MetamagicExtender
        {
            Intensified = 0x40000000,
            Dazing = 0x20000000,
            Persistent = 0x10000000,
            Rime = 0x08000000,
            Toppling = 0x04000000,
            Selective = 0x02000000,
            ElementalFire = 0x01000000,
            ElementalCold = 0x00800000,
            ElementalElectricity = 0x00400000,
            ElementalAcid = 0x00200000,
            Elemental = ElementalFire | ElementalCold | ElementalElectricity | ElementalAcid,
            BloodIntensity = 0x00100000,
            IntensifiedGeneral = BloodIntensity | Intensified
        }

        static public bool test_mode = false;
        static LibraryScriptableObject library => Main.library;

        static public BlueprintFeature intensified_metamagic;
        static public BlueprintFeature dazing_metamagic;
        static public BlueprintFeature toppling_metamagic;
        static public BlueprintFeature rime_metamagic;
        static public BlueprintFeature persistent_metamagic;

        public static void load()
        {
            createIntensifiedSpell();
            createTopplingSpell();
            createRimeSpell();
            createDazingSpell();
            createPersistentSpell();
        }




        static void AddMetamagicToFeatSelection(BlueprintFeature feat)
        {
            var selections = new BlueprintFeatureSelection[]{library.Get<BlueprintFeatureSelection>("3a60f0c0442acfb419b0c03b584e1394"),
                                                            library.Get<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899"),
                                                           };

            foreach (var s in selections)
            {
                s.AllFeatures = s.AllFeatures.AddToArray(feat);
            }
        }


        static void createPersistentSpell()
        {
            persistent_metamagic = library.CopyAndAdd<BlueprintFeature>("a1de1e4f92195b442adb946f0e2b9d4e", "PersistentSpellFeature", "");
            persistent_metamagic.SetNameDescriptionIcon("Metamagic (Persistent Spell)",
                                                         "Whenever a creature targeted by a persistent spell or within its area succeeds on its saving throw against the spell, it must make another saving throw against the effect. If a creature fails this second saving throw, it suffers the full effects of the spell, as if it had failed its first saving throw.\n"
                                                         + "Level Increase: +2 (a persistent spell uses up a spell slot two levels higher than the spell’s actual level.)\n"
                                                         + "Spells that do not require a saving throw to resist or lessen the spell’s effect do not benefit from this feat.",
                                                         LoadIcons.Image2Sprite.Create(@"FeatIcons/PersistentSpell.png")
                                                        );

            persistent_metamagic.ReplaceComponent<AddMetamagicFeat>(a => a.Metamagic = (Metamagic)MetamagicExtender.Persistent);
            library.AddFeats(persistent_metamagic);
            AddMetamagicToFeatSelection(persistent_metamagic);

            var spells = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(b => b.IsSpell && b.LocalizedSavingThrow.ToString() != Helpers.savingThrowNone.ToString() && !b.LocalizedSavingThrow.ToString().Empty()).Cast<BlueprintAbility>().ToArray();
            foreach (var s in spells)
            {
                s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Persistent;
                if (s.Parent != null)
                {
                    s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Persistent;
                }
            }
        }


        static void createDazingSpell()
        {
            var dazed = library.Get<BlueprintBuff>("9934fedff1b14994ea90205d189c8759");
            

            dazing_metamagic = library.CopyAndAdd<BlueprintFeature>("a1de1e4f92195b442adb946f0e2b9d4e", "DazingSpellFeature", "");
            dazing_metamagic.SetNameDescriptionIcon("Metamagic (Dazing Spell)",
                                                         "You can modify a spell to daze a creature damaged by the spell. When a creature takes damage from this spell, they become dazed for a number of rounds equal to the original level of the spell. If the spell allows a saving throw, a successful save negates the daze effect. If the spell does not allow a save, the target can make a Will save to negate the daze effect. \n"
                                                         + "Level Increase: +3 (a dazing spell uses up a spell slot three levels higher than the spell’s actual level.)\n"
                                                         + "Spells that do not inflict damage do not benefit from this feat.",
                                                         LoadIcons.Image2Sprite.Create(@"FeatIcons/DazingSpell.png")
                                                        );
            dazing_metamagic.AddComponent(Helpers.Create<NewMechanics.ApplyBuffOnSpellDamageIfHasMetamagic>(a =>
                                                                                            {
                                                                                                a.descriptor = SpellDescriptor.None;
                                                                                                a.save_type = SavingThrowType.Will;
                                                                                                a.buff = dazed;
                                                                                                a.use_existing_save = true;
                                                                                                a.metamagic = (Metamagic)MetamagicExtender.Dazing;
                                                                                            }
                                                                                            )
                                         );


            dazing_metamagic.ReplaceComponent<AddMetamagicFeat>(a => a.Metamagic = (Metamagic)MetamagicExtender.Dazing);
            library.AddFeats(dazing_metamagic);
            AddMetamagicToFeatSelection(dazing_metamagic);

            var spells = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(b => b.IsSpell && b.EffectOnEnemy == AbilityEffectOnUnit.Harmful && ((b.AvailableMetamagic & Metamagic.Maximize) != 0)).Cast<BlueprintAbility>().ToArray();
            foreach (var s in spells)
            {
                s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Dazing;
                if (s.Parent != null)
                {
                    s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Dazing;
                }
            }
        }


        static void createRimeSpell()
        {
            var entangled = library.Get<BlueprintBuff>("f7f6330726121cf4b90a6086b05d2e38");

            rime_metamagic = library.CopyAndAdd<BlueprintFeature>("a1de1e4f92195b442adb946f0e2b9d4e", "RimeSpellFeature", "");
            rime_metamagic.SetNameDescriptionIcon("Metamagic (Rime Spell)",
                                                         "The frost of your cold spell clings to the target, impeding it for a short time. A rime spell causes creatures that takes cold damage from the spell to become entangled for a number of rounds equal to the original level of the spell.\n"
                                                         + "Level Increase: +1 (a rime spell uses up a spell slot one level higher than the spell’s actual level.)\n"
                                                         + "This feat only affects spells with the cold descriptor.",
                                                         LoadIcons.Image2Sprite.Create(@"FeatIcons/RimeSpell.png")
                                                        );
            rime_metamagic.AddComponent(Helpers.Create<NewMechanics.ApplyBuffOnSpellDamageIfHasMetamagic>(a =>
                                                                                            {
                                                                                                a.descriptor = SpellDescriptor.Cold;
                                                                                                a.save_type = SavingThrowType.Unknown;
                                                                                                a.buff = entangled;
                                                                                                a.metamagic = (Metamagic)MetamagicExtender.Rime;
                                                                                            }
                                                                                            )
                                         );


            rime_metamagic.ReplaceComponent<AddMetamagicFeat>(a => a.Metamagic = (Metamagic)MetamagicExtender.Rime);
            library.AddFeats(rime_metamagic);
            AddMetamagicToFeatSelection(rime_metamagic);

            var spells = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(b => b.IsSpell 
                                                                                     && b.EffectOnEnemy == AbilityEffectOnUnit.Harmful 
                                                                                     && ((b.AvailableMetamagic & Metamagic.Maximize) != 0) 
                                                                                     && (b.SpellDescriptor & (SpellDescriptor.Fire | SpellDescriptor.Cold | SpellDescriptor.Electricity | SpellDescriptor.Acid)) != 0 ).Cast<BlueprintAbility>().ToArray();
            foreach (var s in spells)
            {
                s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Rime;
                if (s.Parent != null)
                {
                    s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Rime;
                }
            }
        }


        static void createTopplingSpell()
        {

            var action = Helpers.Create<ContextActionCombatManeuver>(a => { a.Type = Kingmaker.RuleSystem.Rules.CombatManeuver.Trip; a.UseCastingStat = true; a.UseCasterLevelAsBaseAttack = true; });

            toppling_metamagic = library.CopyAndAdd<BlueprintFeature>("a1de1e4f92195b442adb946f0e2b9d4e", "TopplingSpellFeature", "");
            toppling_metamagic.SetNameDescriptionIcon("Metamagic (Toppling Spell)",
                                                         "The impact of your force spell is strong enough to knock the target prone. If the target takes damage, fails its saving throw, or is moved by your force spell, make a trip check against the target, using your caster level plus your casting ability score bonus (Wisdom for clerics, Intelligence for wizards, and so on). \n"
                                                         + "Level Increase: +1 (a toppling spell uses up a spell slot one level higher than the spell’s actual level.)\n"
                                                         + "A toppling spell only affects spells with the force descriptor.",
                                                         LoadIcons.Image2Sprite.Create(@"FeatIcons/TopplingSpell.png")
                                                        );
            toppling_metamagic.AddComponent(Helpers.Create<NewMechanics.ActionOnSpellDamage>(a =>
                                                                                            {
                                                                                                a.descriptor = SpellDescriptor.None;
                                                                                                a.save_type = SavingThrowType.Unknown;
                                                                                                a.action = Helpers.CreateActionList(action);
                                                                                            }
                                                                                            )
                                         );


            toppling_metamagic.ReplaceComponent<AddMetamagicFeat>(a => a.Metamagic = (Metamagic)MetamagicExtender.Toppling);
            library.AddFeats(toppling_metamagic);
            AddMetamagicToFeatSelection(toppling_metamagic);

            var spells = new BlueprintAbility[]
                                                {
                                                    library.Get<BlueprintAbility>("4ac47ddb9fa1eaf43a1b6809980cfbd2"), //magic missile
                                                    library.Get<BlueprintAbility>("0a2f7c6aa81bc6548ac7780d8b70bcbc"), //battering blast
                                                };
            foreach (var s in spells)
            {
                s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Toppling;
                if (s.Parent != null)
                {
                    s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Toppling;
                }
            }
        }


        static void createIntensifiedSpell()
        {

            intensified_metamagic = library.CopyAndAdd<BlueprintFeature>("a1de1e4f92195b442adb946f0e2b9d4e", "IntensifiedSpellFeature", "");
            intensified_metamagic.SetNameDescriptionIcon("Metamagic (Intensified Spell)",
                                                         "An intensified spell increases the maximum number of damage dice by 5 levels. You must actually have sufficient caster levels to surpass the maximum in order to benefit from this feat. No other variables of the spell are affected, and spells that inflict damage that is not modified by caster level are not affected by this feat.\n"
                                                         + "Level Increase: +1 (an intensified spell uses up a spell slot one level higher than the spell’s actual level.)",
                                                         LoadIcons.Image2Sprite.Create(@"FeatIcons/IntensifiedSpell.png")
                                                         );
            intensified_metamagic.ReplaceComponent<AddMetamagicFeat>(a => a.Metamagic = (Metamagic)MetamagicExtender.Intensified);
            library.AddFeats(intensified_metamagic);
            AddMetamagicToFeatSelection(intensified_metamagic);


           var spells = library.GetAllBlueprints().Where(b => (b is BlueprintAbility) && (b as BlueprintAbility).IsSpell).Cast<BlueprintAbility>().ToArray();

            foreach (var s in spells)
            {
                var run_action = s.GetComponent<AbilityEffectRunAction>()?.Actions;
                if (run_action == null)
                {
                    continue;
                }
                var damage = Common.extractActions<ContextActionDealDamage>(run_action.Actions).Where(d => d.Value.DiceCountValue.ValueType == ContextValueType.Rank).ToArray();
                var context_rank_configs = s.GetComponents<ContextRankConfig>().Where(c =>                                                                                     
                                                                                          Helpers.GetField<ContextRankBaseValueType>(c, "m_BaseValueType") == ContextRankBaseValueType.CasterLevel
                                                                                          && Helpers.GetField<bool>(c, "m_UseMax") == true
                                                                                          && Helpers.GetField<int>(c, "m_Max") != Helpers.GetField<int>(c, "m_Min")                                                                                 
                                                                                      ).ToArray();
                if (damage.Empty() || context_rank_configs.Empty())
                {
                    continue;
                }

                foreach (var d in damage)
                {
                    var config = context_rank_configs.FirstOrDefault(c => Helpers.GetField<AbilityRankType>(c, "m_Type") == d.Value.DiceCountValue.ValueRank);
                    if (config != null)
                    {
                        Helpers.SetField(config, "m_Feature", intensified_metamagic);
                        s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.IntensifiedGeneral;
                        if (s.Parent != null)
                        {
                            s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.IntensifiedGeneral;
                        }
                    }

                   
                }
            }

            var acid_spray_buff = library.Get<BlueprintBuff>("ab9cfc0c9411e6441b738dcf5a3567ee");
            Helpers.SetField(acid_spray_buff.GetComponent<ContextRankConfig>(), "m_Feature", intensified_metamagic);
        }


        static int intensify_watcher = 0;


        [Harmony12.HarmonyPatch(typeof(ContextRankConfig))]
        [Harmony12.HarmonyPatch("GetValue", Harmony12.MethodType.Normal)]
        static class ContextRankConfig_GetValue_Patch
        {
            internal static bool Prefix(ContextRankConfig __instance, MechanicsContext context, ref int __result)
            {
                intensify_watcher = 0;
                if (context.HasMetamagic((Metamagic)MetamagicExtender.Intensified))
                {
                    intensify_watcher = 5;
                }

                if (context.HasMetamagic((Metamagic)MetamagicExtender.BloodIntensity))
                {
                    var value = Math.Max(context.MaybeCaster.Descriptor.Stats.Charisma.Bonus, context.MaybeCaster.Descriptor.Stats.Strength);
                    if (intensify_watcher < value)
                    {
                        intensify_watcher = value;
                    }
                }
                return true;
            }

            internal static void Postfix(ContextRankConfig __instance, MechanicsContext context, ref int __result)
            {
                 intensify_watcher = 0;
            }
        }


        [Harmony12.HarmonyPatch(typeof(ContextRankConfig))]
        [Harmony12.HarmonyPatch("ApplyMinMax", Harmony12.MethodType.Normal)]
        static class ContextRankConfig_ApplyMinMax_Patch
        {
            internal static bool Prefix(ContextRankConfig __instance, int value, ref int __result)
            {
                bool intensified_allowed = Helpers.GetField<BlueprintFeature>(__instance, "m_Feature") == intensified_metamagic && intensify_watcher != 0;
                if (Helpers.GetField<bool>(__instance, "m_UseMin"))
                    __result = Math.Max(value, Helpers.GetField<int>(__instance, "m_Min"));
                if (Helpers.GetField<bool>(__instance, "m_UseMax"))
                    __result = Math.Min(value, Helpers.GetField<int>(__instance, "m_Max") + (intensified_allowed ? intensify_watcher : 0));

                return false;
            }
        }

        [Harmony12.HarmonyPatch(typeof(MetamagicHelper))]
        [Harmony12.HarmonyPatch("DefaultCost", Harmony12.MethodType.Normal)]
        static class MetamagicHelper_DefaultCost_Patch
        {
            internal static bool Prefix(Metamagic metamagic, ref int __result)
            {
                switch ((MetamagicExtender)metamagic)
                {
                    case MetamagicExtender.Dazing:
                        __result = 3;
                        return false;
                    case MetamagicExtender.Persistent:
                        __result = 2;
                        return false;
                    case MetamagicExtender.Rime:
                    case MetamagicExtender.Toppling:
                    case MetamagicExtender.Intensified:
                    case MetamagicExtender.ElementalFire:
                    case MetamagicExtender.ElementalCold:
                    case MetamagicExtender.ElementalElectricity:
                    case MetamagicExtender.ElementalAcid:
                    case MetamagicExtender.Selective:
                        __result = 1;
                        return false;
                }
                return true;
            }
        }

        [Harmony12.HarmonyPatch(typeof(MetamagicHelper))]
        [Harmony12.HarmonyPatch("SpellIcon", Harmony12.MethodType.Normal)]
        static class MetamagicHelper_SpellIcon_Patch
        {
            internal static bool Prefix(Metamagic metamagic, ref Sprite __result)
            {
                switch ((MetamagicExtender)metamagic)
                {
                    case MetamagicExtender.Intensified:
                        __result = UIRoot.Instance.SpellBookColors.MetamagicEmpower;
                        return false;
                    case MetamagicExtender.Dazing:
                    case MetamagicExtender.Toppling:
                        __result = UIRoot.Instance.SpellBookColors.MetamagicReach;
                        return false;
                    case MetamagicExtender.Rime:
                        __result = UIRoot.Instance.SpellBookColors.MetamagicHeighten;
                        return false;
                    case MetamagicExtender.Persistent:
                        __result = UIRoot.Instance.SpellBookColors.MetamagicMaximize;
                        return false;
                }
                
                return true;
            }
        }


        [Harmony12.HarmonyPatch(typeof(RuleSavingThrow))]
        [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
        static class RuleSavingThrow_OnTrigger_Patch
        {
            internal static void Postfix(RuleSavingThrow __instance, RulebookEventContext context)
            {
                if (__instance.Initiator.Descriptor.State.IsDead)
                    return;

                var ability = __instance.Reason?.Ability;

                if (ability == null)
                {
                    return;
                }

                if (!ability.HasMetamagic((Metamagic)MetamagicExtender.Persistent))
                {
                    return;
                }



                if (__instance.IsPassed)
                {
                    int old_value = __instance.D20;
                    Harmony12.Traverse.Create(__instance).Property("D20").SetValue(RulebookEvent.Dice.D20);
                    int new_value = __instance.D20;
                    Common.AddBattleLogMessage(__instance.Initiator.CharacterName + " rerolls saving throw due to persistent spell: " + $"{old_value}  >>  {new_value}");
                }
            }
        }
    }
}
