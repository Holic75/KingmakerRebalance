using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
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
using Kingmaker.UnitLogic.Class.Kineticist.Properties;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
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
    public class Subdomains
    {
        static LibraryScriptableObject library => Main.library;

        static BlueprintCharacterClass cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
        static BlueprintCharacterClass inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
        static BlueprintCharacterClass druid_class = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");

        static BlueprintFeatureSelection cleric_domain_selection = library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");
        static BlueprintFeatureSelection cleric_secondary_domain_selection = library.Get<BlueprintFeatureSelection>("43281c3d7fe18cc4d91928395837cd1e");
        static BlueprintFeatureSelection druid_domain_selection = library.Get<BlueprintFeatureSelection>("5edfe84c93823d04f8c40ca2b4e0f039");
        static BlueprintFeatureSelection blight_druid_domain_selection = library.Get<BlueprintFeatureSelection>("096fc02f6cc817a43991c4b437e12b8e");

        static public BlueprintProgression storm_domain;
        static public BlueprintProgression storm_domain_secondary;
        static public BlueprintProgression storm_domain_druid;


        static public BlueprintProgression lightning_domain;
        static public BlueprintProgression lightning_domain_secondary;
        static public BlueprintProgression lightning_domain_druid;

        public static void load()
        {
            createStormsDomain();
            createLightningDomain();
        }


        static void createStormsDomain()
        {
            var difficult_terrain_buff = library.CopyAndAdd<BlueprintBuff>("762f5da59182e9b4b90c62ed3142b732", "GaleAuraEffectBuff", "");

            var area_buff = Common.createBuffAreaEffect(difficult_terrain_buff, 20.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>(), Common.createContextConditionHasFact(NewSpells.immunity_to_wind, false)));
            //area_buff.GetComponent<AddAreaEffect>().AreaEffect.Fx = Common.createPrefabLink("9f9ebe136ce5a9345b5b016f011c5aa6");

            var buff = Helpers.CreateBuff("GaleAuraBuff",
                                             "Gale Aura",
                                             "At 6th level, as a standard action, you can create a 20-foot aura of gale-like winds that slows the progress of enemies. Creatures in the aura cannot take a 5-foot step. Enemies in the aura treat each square that brings them closer to you as difficult terrain. They can move normally in any other direction. You can use this ability for a number of rounds per day equal to your cleric level. The rounds do not need to be consecutive.",
                                             "",
                                             Helpers.GetIcon("093ed1d67a539ad4c939d9d05cfe192c"),
                                             Common.createPrefabLink("9f9ebe136ce5a9345b5b016f011c5aa6")
                                             );


            var resource = Helpers.CreateAbilityResource("GaleAuraResource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, new BlueprintCharacterClass[] { cleric_class, druid_class, inquisitor_class });

            var ability = Helpers.CreateActivatableAbility("GaleAuraToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.WithUnitCommand,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                           null,
                                                           resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.NewRound)
                                                           );

            var feature = Common.ActivatableAbilityToFeature(ability, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            var weather_domain = library.Get<BlueprintProgression>("c18a821ee662db0439fb873165da25be");
            var weather_domain_domain_secondary = library.Get<BlueprintProgression>("d124d29c7c96fc345943dd17e24990e8");
            var weather_domain_domain_druid = library.Get<BlueprintProgression>("4a3516fdc4cda764ebd1279b22d10205");

            var weather_domain_greater = library.Get<BlueprintFeature>("8e44306af595c8d44aad2f1260fd7be2");

            var spell_list = library.CopyAndAdd<BlueprintSpellList>("eba577470b8ee8443bb4552433451990", "StormsSubdomainSpellList", "700ec962456941d79fc50144be87c97a");
            Common.excludeSpellsFromList(spell_list, a => false);
            storm_domain = createSubdomain("StormsSubdomain", "Storms Subdomain",
                                               "With power over storm and sky, you can call down the wrath of the gods upon the world below.\nStorm Burst: As a standard action, you can create a storm burst targeting any foe within 30 feet as a ranged touch attack. The storm burst deals 1d6 points of damage + 1 point for every two levels you possess in the class that gave you access to this domain. In addition, the target is buffeted by winds and rain, causing it to take a –2 penalty on attack rolls for 1 round. You can use this ability a number of times per day equal to 3 + your Wisdom modifier.\n"
                                               + ability.Name + ": " + ability.Description + "\n"
                                               + "Domain Spells: Obscuring Mist, Summon Small Elemental, Call Lightning, Sleet Storm, Call Lightning Storm, Sirocco, Scouring Winds, Sunburst, Tsunami.",
                                               weather_domain,
                                               new BlueprintFeature[] { weather_domain_greater },
                                               new BlueprintFeature[] { feature },
                                               spell_list
                                               );
            Common.replaceDomainSpell(storm_domain, library.Get<BlueprintAbility>("cad052ef098f9f247ab73ae4c37ac687"), 5);
            storm_domain.AddComponent(Helpers.PrerequisiteNoFeature(weather_domain));

            storm_domain.LevelEntries[1].Level = 6;

            storm_domain_secondary = library.CopyAndAdd(storm_domain, "StormSubdomainSecondaryProgression", "");
            storm_domain_secondary.RemoveComponents<LearnSpellList>();

            storm_domain_secondary.AddComponents(Helpers.PrerequisiteNoFeature(storm_domain),
                                                 Helpers.PrerequisiteNoFeature(weather_domain),
                                                 Helpers.PrerequisiteNoFeature(weather_domain_domain_secondary));
            storm_domain.AddComponents(Helpers.PrerequisiteNoFeature(storm_domain_secondary));
            storm_domain_druid = library.CopyAndAdd(storm_domain_secondary, "StormSubdomainDruidProgression", "");
            storm_domain_druid.Classes = new BlueprintCharacterClass[] { druid_class };
            storm_domain_druid.ComponentsArray = new BlueprintComponent[] { Helpers.PrerequisiteNoFeature(weather_domain_domain_druid), Helpers.PrerequisiteClassLevel(druid_class, 1) };
            weather_domain_domain_druid.AddComponent(Helpers.PrerequisiteNoFeature(storm_domain_druid));

            var weather_domain_base = library.Get<BlueprintFeature>("1c37869ee06ca33459f16f23f4969e7d");
            weather_domain_base.SetNameDescription(library.Get<BlueprintAbility>("f166325c271dd29449ba9f98d11542d9"));

            var spells_feature_druid = library.CopyAndAdd<BlueprintFeature>("01c9f3756c9d2e1488b6a2d29dd9d37f", "StormsDomainSpellListDruidFeature", "");
            spells_feature_druid.ReplaceComponent<AddSpecialSpellList>(a => a.SpellList = spell_list);
            storm_domain_druid.LevelEntries = new LevelEntry[] { Helpers.LevelEntry(1, storm_domain_druid.LevelEntries[0].Features.ToArray().AddToArray(spells_feature_druid)),
                                                                 storm_domain_druid.LevelEntries[1] };

            cleric_domain_selection.AllFeatures = cleric_domain_selection.AllFeatures.AddToArray(storm_domain);
            cleric_secondary_domain_selection.AllFeatures = cleric_secondary_domain_selection.AllFeatures.AddToArray(storm_domain_secondary);
            druid_domain_selection.AllFeatures = druid_domain_selection.AllFeatures.AddToArray(storm_domain_druid);
            blight_druid_domain_selection.AllFeatures = blight_druid_domain_selection.AllFeatures.AddToArray(storm_domain_druid);
        }


        static void createLightningDomain()
        {
            var buff = Helpers.CreateBuff("LightningRodBuff",
                                             "Lightning Rod",
                                             "As a swift action when you cast a spell with the electricity descriptor, you can designate one creature within line of sight. The spell’s damage against that creature increases by 50%, as if affected by the Empower Spell feat. This additional damage results from divine power that is not subject to being reduced by electricity resistance, and you take an equal amount of electricity damage immediately after you cast the spell.\n"
                                             + "The spell can deal this additional damage only once, even if it could affect the target multiple times.\n"
                                             + "You can use this ability once per day at 8th level and one additional time per day for every 4 cleric levels you have beyond 8th.",
                                             "",
                                             Helpers.GetIcon("d2cff9243a7ee804cb6d5be47af30c73"),
                                             null,
                                             Helpers.Create<SpellManipulationMechanics.DamageBoostAndReflection>(d =>
                                             {
                                                 d.change_damage_type = true;
                                                 d.change_reflect_damage_type = true;
                                                 d.reflect_damage = true;
                                                 d.remove_self_on_apply = true;
                                                 d.damage_type = Kingmaker.Enums.Damage.DamageEnergyType.Holy;
                                                 d.reflect_damage_type = Kingmaker.Enums.Damage.DamageEnergyType.Electricity;
                                                 d.descriptor = SpellDescriptor.Electricity;
                                                 d.only_from_caster = true;
                                                 d.only_spells = true;
                                             }),
                                             Helpers.Create<UniqueBuff>()
                                             );
            buff.Stacking = StackingType.Stack;
            var resource = Helpers.CreateAbilityResource("LightningRodResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 12, 1, 4, 1, 0, 0.0f, new BlueprintCharacterClass[] { cleric_class, druid_class, inquisitor_class });

            var ability = Helpers.CreateAbility("LightningRodAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                                                AbilityRange.Long,
                                                Helpers.oneRoundDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            ability.CanTargetSelf = false;

            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));
            
            var air_domain = library.Get<BlueprintProgression>("750bfcd133cd52f42acbd4f7bc9cc365");
            var air_domain_secondary = library.Get<BlueprintProgression>("d7169e8978d9e9d418398eab946c49e5");
            var air_domain_druid = library.Get<BlueprintProgression>("3aef017b78329db4fa53fe8560069886");

            var air_domain_greater = library.Get<BlueprintFeature>("8a8e3f80abc04c34b98324823d14b057");
            var air_domain_capstone = library.Get<BlueprintFeature>("d5a54e5a3876f5a498abad99b1984cb5");

            var spell_list = library.CopyAndAdd<BlueprintSpellList>("9678d121f669f864d9da5dabf1ca1ce0", "LightningSubdomainSpellList", "");
            Common.excludeSpellsFromList(spell_list, a => false);

            lightning_domain = createSubdomain("LightningSubdomain", "Lightning Subdomain",
                                               "You can manipulate lightning, mist, and wind, and are resistant to electricity damage.\nLightning Arc: As a standard action, you can unleash an arc of electricity targeting any foe within 30 feet as a ranged touch attack. This arc of electricity deals 1d6 points of electricity damage + 1 point for every two levels you possess in the class that gave you access to this domain. You can use this ability a number of times per day equal to 3 + your Wisdom modifier.\n"
                                               + ability.Name + ": " + ability.Description + "\n"
                                               + "Domain Spells: Shocking Grasp, Flame Blade (deals electricity damage and gains the electricity descriptor instead of fire), Lightning Bolt, Air Walk, Cloudkill, Chain Lightning, Elemental Body IV (Air), Shout, Greater, Elemental Swarm (Air).",
                                               air_domain,
                                               new BlueprintFeature[] { air_domain_greater, air_domain_greater, air_domain_capstone },
                                               new BlueprintFeature[] { feature, null, null },
                                               spell_list
                                               );

            Common.replaceDomainSpell(lightning_domain, library.Get<BlueprintAbility>("ab395d2335d3f384e99dddee8562978f"), 1);
            Common.replaceDomainSpell(lightning_domain, CallOfTheWild.NewSpells.flame_blade_electric, 2);
            lightning_domain.AddComponent(Helpers.PrerequisiteNoFeature(air_domain));

            lightning_domain.LevelEntries[1].Level = 8;

            lightning_domain_secondary = library.CopyAndAdd(lightning_domain, "LightningSubdomainSecondaryProgression", "");
            lightning_domain_secondary.RemoveComponents<LearnSpellList>();

            lightning_domain_secondary.AddComponent(Helpers.PrerequisiteNoFeature(lightning_domain));

            lightning_domain_druid = library.CopyAndAdd(lightning_domain_secondary, "LightningSubdomainDruidProgression", "");
            lightning_domain_druid.Classes = new BlueprintCharacterClass[] { druid_class };
            lightning_domain_druid.ComponentsArray = new BlueprintComponent[] { Helpers.PrerequisiteNoFeature(air_domain_druid), Helpers.PrerequisiteClassLevel(druid_class, 1) };                                                              
            air_domain_druid.AddComponent(Helpers.PrerequisiteNoFeature(lightning_domain_druid));

            var air_domain_base = library.Get<BlueprintFeature>("39b0c7db785560041b436b558c9df2bb");
            air_domain_base.SetNameDescription(library.Get<BlueprintAbility>("b3494639791901e4db3eda6117ad878f"));

            var spells_feature_druid = library.CopyAndAdd<BlueprintFeature>("01c9f3756c9d2e1488b6a2d29dd9d37f", "LightningDomainSpellListDruidFeature", "");
            spells_feature_druid.ReplaceComponent<AddSpecialSpellList>(a => a.SpellList = spell_list);
            lightning_domain_druid.LevelEntries = new LevelEntry[] { Helpers.LevelEntry(1, lightning_domain_druid.LevelEntries[0].Features.ToArray().AddToArray(spells_feature_druid)),
                                                                 lightning_domain_druid.LevelEntries[1] };

            cleric_domain_selection.AllFeatures = cleric_domain_selection.AllFeatures.AddToArray(lightning_domain);
            cleric_secondary_domain_selection.AllFeatures = cleric_secondary_domain_selection.AllFeatures.AddToArray(lightning_domain_secondary);
            druid_domain_selection.AllFeatures = druid_domain_selection.AllFeatures.AddToArray(lightning_domain_druid);
            blight_druid_domain_selection.AllFeatures = blight_druid_domain_selection.AllFeatures.AddToArray(lightning_domain_druid);
        }



        static BlueprintProgression createSubdomain(string name,
                                                    string display_name,
                                                    string description,
                                                    BlueprintProgression base_progression,
                                                    BlueprintFeature[] old_features,
                                                    BlueprintFeature[] new_features,
                                                    BlueprintSpellList spell_list = null )
        {
            bool[] features_replaced = new bool[old_features.Length];
            List<LevelEntry> new_level_entries = new List<LevelEntry>();
            var progression = library.CopyAndAdd(base_progression, name + "Progression", "");
            progression.SetNameDescription(display_name, description);

            foreach (var le in base_progression.LevelEntries)
            {
                var features = le.Features.ToArray();
                
                for (int i = 0; i < old_features.Length; i++ )
                {
                    if (!features_replaced[i])
                    {
                        if (features.Contains(old_features[i]))
                        {
                            features_replaced[i] = true;
                            features = features.RemoveFromArray(old_features[i]);
                            if (new_features[i] != null)
                            {
                                features = features.AddToArray(new_features[i]);
                            }
                        }
                    }
                }

                if (!features.Empty())
                {
                    new_level_entries.Add( Helpers.LevelEntry(le.Level, features));
                }
            }

            progression.LevelEntries = new_level_entries.ToArray();

            if (spell_list != null)
            {
                progression.ReplaceComponent<LearnSpellList>(l => l.SpellList = spell_list);
            }

            features_replaced = new bool[old_features.Length];
            List<UIGroup> ui_groups = new List<UIGroup>();
            foreach (var uig in base_progression.UIGroups)
            {
                var features = uig.Features.ToArray();

                for (int i = 0; i < old_features.Length; i++)
                {
                    if (!features_replaced[i])
                    {
                        if (features.Contains(old_features[i]))
                        {
                            features_replaced[i] = true;
                            features = features.RemoveFromArray(old_features[i]);
                            if (new_features[i] != null)
                            {
                                features = features.AddToArray(new_features[i]);
                            }
                        }
                    }
                }

                if (!features.Empty())
                {
                    ui_groups.Add(Helpers.CreateUIGroup(features));
                }
            }

            progression.UIGroups = ui_groups.ToArray();

            return progression;
        }
    }
}
