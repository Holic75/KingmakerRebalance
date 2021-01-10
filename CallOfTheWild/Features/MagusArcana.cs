using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
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
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class MagusArcana
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintFeatureSelection maneuver_mastery;
        static public BlueprintFeature familiar;
        static public BlueprintFeatureSelection spell_blending;
        static public BlueprintFeatureSelection spell_blending_mindblade;
        static public BlueprintFeature reach_spellstrike;

        static BlueprintCharacterClass magus = library.Get<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
        static BlueprintFeatureSelection magus_arcana = library.Get<BlueprintFeatureSelection>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
        static BlueprintFeatureSelection eldritch_magus_arcana = library.Get<BlueprintFeatureSelection>("d4b54d9db4932454ab2899f931c2042c");


        public static void load()
        {
            createManeuverMastery();
            createFamiliar();
            createSpellBlending();
            createReachSpellStrike();
        }


        static BlueprintCharacterClass[] getMagusArray()
        {
            return new BlueprintCharacterClass[] { magus };
        }


        static void createReachSpellStrike()
        {
            reach_spellstrike = Helpers.CreateFeature("ReachSpellStrikeMagusArcana",
                                                      "Reach Spellstrike",
                                                      "The magus can deliver spells with a range of touch with ranged spellstrike.",
                                                      "12ebb355c7e34596badbf3cf5b88c30f",
                                                      Helpers.GetIcon("3e9d1119d43d07c4c8ba9ebfd1671952"),
                                                      FeatureGroup.MagusArcana,
                                                      Helpers.Create<NewMechanics.MetamagicMechanics.ReachSpellStrike>(r => r.Metamagic = Metamagic.Reach),
                                                      Helpers.PrerequisiteClassLevel(magus, 9),
                                                      Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc")) //ranged spell strike
                                                      );
            magus_arcana.AllFeatures = magus_arcana.AllFeatures.AddToArray(reach_spellstrike);
        }


        static void createSpellBlending()
        {
            var icon = library.Get<BlueprintFeature>("55edf82380a1c8540af6c6037d34f322").Icon;
            spell_blending = Helpers.CreateFeatureSelection("SpellBlendingFeature",
                                                                          "Spell Blending",
                                                                          "When a magus selects this arcana, he must select one spell from the wizard spell list that is of a magus spell level he can cast. He adds this spell to his spellbook and list of magus spells known as a magus spell of its wizard spell level.\n"
                                                                          + "Special: A magus can select this magus arcana more than once.",
                                                                          "",
                                                                          icon,
                                                                          FeatureGroup.None,
                                                                          Common.prerequisiteNoArchetype(Archetypes.MindBlade.archetype));

            var wizard_spell_list = library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");
            for (int i = 1; i <= 6; i++)
            {
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956", $"SpellBlending{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = magus;
                learn_spell.SpellList = wizard_spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = wizard_spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = magus; });
                learn_spell.AddComponents(Helpers.Create<PrerequisiteClassSpellLevel>(p => { p.CharacterClass = magus; p.RequiredSpellLevel = i; })
                                          );
                learn_spell.SetName(Helpers.CreateString($"SpellBlending{i}ParametrizedFeature.Name", "Spell Blending " + $"(Level {i})"));
                learn_spell.SetDescription(spell_blending.Description);
                learn_spell.SetIcon(spell_blending.Icon);

                spell_blending.AllFeatures = spell_blending.AllFeatures.AddToArray(learn_spell);
            }

            magus_arcana.AllFeatures = magus_arcana.AllFeatures.AddToArray(spell_blending);
            eldritch_magus_arcana.AllFeatures = eldritch_magus_arcana.AllFeatures.AddToArray(spell_blending);



            spell_blending_mindblade = library.CopyAndAdd(spell_blending, "SpellBlendingMindbladeFeature", "");
            spell_blending_mindblade.ComponentsArray = new BlueprintComponent[] { Common.createPrerequisiteArchetypeLevel(Archetypes.MindBlade.archetype, 1) };
            spell_blending_mindblade.AllFeatures = new BlueprintFeature[0];

            for (int i = 1; i <= 6; i++)
            {
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956", $"SpellBlendingMindblade{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = magus;
                learn_spell.SpellList = Archetypes.MindBlade.extra_psychic_spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = Archetypes.MindBlade.extra_psychic_spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = magus; });
                learn_spell.AddComponents(Helpers.Create<PrerequisiteClassSpellLevel>(p => { p.CharacterClass = magus; p.RequiredSpellLevel = i; })
                                          );
                learn_spell.SetName(Helpers.CreateString($"SpellBlendingMindblade{i}ParametrizedFeature.Name", "Spell Blending " + $"(Level {i})"));
                learn_spell.SetDescription(spell_blending.Description);
                learn_spell.SetIcon(spell_blending.Icon);

                spell_blending_mindblade.AllFeatures = spell_blending_mindblade.AllFeatures.AddToArray(learn_spell);
            }

            magus_arcana.AllFeatures = magus_arcana.AllFeatures.AddToArray(spell_blending_mindblade);


        }


        static void createFamiliar()
        {
            familiar = library.CopyAndAdd<BlueprintFeatureSelection>("363cab72f77c47745bf3a8807074d183", "MagusFamiliarFeatureSelection", "");
            familiar.ComponentsArray = new BlueprintComponent[0];
            familiar.Groups = new FeatureGroup[] { FeatureGroup.MagusArcana };
            familiar.SetDescription("The magus gains a familiar.");
            familiar.AddComponent(Helpers.PrerequisiteNoFeature(familiar));

            magus_arcana.AllFeatures = magus_arcana.AllFeatures.AddToArray(familiar);
            eldritch_magus_arcana.AllFeatures = eldritch_magus_arcana.AllFeatures.AddToArray(familiar);
        }
        
        static void createManeuverMastery()
        {
            var maneuvers = new CombatManeuver[][] { new CombatManeuver[] { CombatManeuver.BullRush },
                                                     new CombatManeuver[] {CombatManeuver.Disarm },
                                                     new CombatManeuver[] {CombatManeuver.Trip },
                                                     new CombatManeuver[] {CombatManeuver.SunderArmor },
                                                     new CombatManeuver[] {CombatManeuver.DirtyTrickBlind, CombatManeuver.DirtyTrickEntangle, CombatManeuver.DirtyTrickSickened }
                                                   };

            var names = new string[] { "Bull Rush", "Disarm", "Trip", "Sunder", "Dirty Trick" };
            var icons = new UnityEngine.Sprite[]
            {
                library.Get<BlueprintFeature>("b3614622866fe7046b787a548bbd7f59").Icon,
                library.Get<BlueprintFeature>("25bc9c439ac44fd44ac3b1e58890916f").Icon,
                library.Get<BlueprintFeature>("0f15c6f70d8fb2b49aa6cc24239cc5fa").Icon,
                library.Get<BlueprintFeature>("9719015edcbf142409592e2cbaab7fe1").Icon,
                library.Get<BlueprintFeature>("ed699d64870044b43bb5a7fbe3f29494").Icon,
            };

            maneuver_mastery = Helpers.CreateFeatureSelection("MagusManeuverMasteryFeatureSelection",
                                                               "Maneuver Mastery",
                                                               "The magus has mastered one combat maneuver. He selects one maneuver when selecting this arcana. Whenever he is attempting the selected maneuver, he uses his magus level in place of his base attack bonus (in addition to any base attack bonus gained from other classes).\n"
                                                               + "A magus can select this magus arcana more than once. Its effects do not stack. Each time he selects this arcana, he selects another combat maneuver.",
                                                               "",
                                                               null,
                                                               FeatureGroup.MagusArcana);

            for (int i = 0; i < maneuvers.Length; i++)
            {
                var feat = Helpers.CreateFeature("MagusManeuverMastery" + maneuvers[i][0].ToString() + "Feature",
                                                 maneuver_mastery.Name + ": " + names[i],
                                                 maneuver_mastery.Description,
                                                 "",
                                                 icons[i],
                                                 FeatureGroup.MagusArcana,
                                                 Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                 classes: getMagusArray(),
                                                                                 progression: ContextRankProgression.StartPlusDivStep,
                                                                                 startLevel: 1,
                                                                                 stepLevel: 4)
                                                 );

                foreach (var maneuver in maneuvers[i])
                {
                    feat.AddComponent(Helpers.Create<CombatManeuverMechanics.SpecificCombatManeuverBonusUnlessHasFacts>(s => { s.maneuver_type = maneuver; s.Value = Helpers.CreateContextValue(AbilityRankType.Default); }));
                }
                maneuver_mastery.AllFeatures = maneuver_mastery.AllFeatures.AddToArray(feat);
            }

            CombatManeuverMechanics.SpecificCombatManeuverBonusUnlessHasFacts.facts.Add(library.Get<BlueprintBuff>("287682389d2011b41b5a65195d9cbc84"));  //transformation
            magus_arcana.AllFeatures = magus_arcana.AllFeatures.AddToArray(maneuver_mastery);
            eldritch_magus_arcana.AllFeatures = eldritch_magus_arcana.AllFeatures.AddToArray(maneuver_mastery);
        }




        //fix for reach spell strike
        [Harmony12.HarmonyPatch(typeof(UnitPartMagus))]
        [Harmony12.HarmonyPatch("IsSuitableForEldritchArcherSpellStrike", Harmony12.MethodType.Normal)]
        public class Patch_UnitPartMagus_IsSuitableForEldritchArcherSpellStrike_Patch
        {
            static void Postfix(UnitPartMagus __instance, AbilityData spell, ref bool __result)
            {
                Main.TraceLog();
                if (__result == true)
                {
                    return;
                }

                if (spell.Blueprint.GetComponent<AbilityDeliverTouch>() != null 
                    && (spell.HasMetamagic(Metamagic.Reach) || ((spell.Blueprint.AvailableMetamagic & Metamagic.Reach) != 0) && __instance.Owner.HasFact(reach_spellstrike))
                    )
                {
                    __result = __instance.IsSpellFromMagusSpellList(spell);
                }
            }
        }

    }
}
