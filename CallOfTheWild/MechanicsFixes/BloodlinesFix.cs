using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class BloodlinesFix
    {
        static LibraryScriptableObject library => Main.library;
        static BlueprintCharacterClass magus = library.Get<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
        static BlueprintCharacterClass sorcerer = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
        static BlueprintCharacterClass dragon_disciple = library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
        static BlueprintArchetype eldritch_scion = library.Get<BlueprintArchetype>("d078b2ef073f2814c9e338a789d97b73");

        static public BlueprintFeature blood_havoc;
        static public BlueprintFeature blood_intensity;
        static public BlueprintFeature blood_piercing;
        static public BlueprintFeatureSelection bloodline_familiar;

        static public BlueprintFeatureSelection eldritch_heritage;
        static public BlueprintFeatureSelection improved_eldritch_heritage;
        static public BlueprintFeatureSelection greater_eldritch_heritage;
        static internal void load()
        {
            fixSorcBloodlineProgression();
            fixBloodlinesUI();
            fixArcaneBloodline();
            createEldritchHeritage();

            createBloodlineFamiliar();
            createBloodHavoc();
            createBloodIntensity();
            createBloodPiercing();
            addBloodlineMutations();

            //fixDragonDisciplePrerequisites();
            fixBloodlineSpells();
        }


        static void createEldritchHeritage()
        {
            var bloodlines = library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914").AllFeatures;

            eldritch_heritage = Helpers.CreateFeatureSelection("EldritchHeritageFeatureSelection",
                                                       "Eldritch Heritage",
                                                       "Select one sorcerer bloodline. You must have Skill focus in the class skill that bloodline grants to a sorcerer at 1st level (for example, Heal for the celestial bloodline). This bloodline cannot be a bloodline you already have. You gain the first-level bloodline power for the selected bloodline. For purposes of using that power, treat your sorcerer level as equal to your character level – 2, even if you have levels in sorcerer. You do not gain any of the other bloodline abilities.",
                                                       "",
                                                       null,
                                                       FeatureGroup.Feat,
                                                       Helpers.PrerequisiteStatValue(Kingmaker.EntitySystem.Stats.StatType.Charisma, 13),
                                                       Helpers.PrerequisiteCharacterLevel(3)
                                                       );
            eldritch_heritage.AddComponent(Helpers.PrerequisiteNoFeature(eldritch_heritage));
            eldritch_heritage.ReapplyOnLevelUp = true;
            improved_eldritch_heritage = Helpers.CreateFeatureSelection("ImprovedEldritchHeritageFeatureSelection",
                                                                       "Improved Eldritch Heritage",
                                                                       "You gain either the 3rd-level or the 9th-level power (your choice) of the bloodline you selected with the Eldritch Heritage feat. For purposes of using that power, treat your sorcerer level as equal to your character level – 2, even if you have levels in sorcerer. You do not gain any of the other bloodline abilities.\n"
                                                                       + "Special: You may select this feat multiple times. Its effects do not stack. Each time you select the feat, it applies to a different bloodline power for that bloodline available at sorcerer level 3 or 9.",
                                                                       "",
                                                                       null,
                                                                       FeatureGroup.Feat,
                                                                       Helpers.PrerequisiteStatValue(Kingmaker.EntitySystem.Stats.StatType.Charisma, 15),
                                                                       Helpers.PrerequisiteCharacterLevel(11),
                                                                       Helpers.PrerequisiteFeature(eldritch_heritage)
                                                                       );


            greater_eldritch_heritage = Helpers.CreateFeatureSelection("GreaterEldritchHeritageFeatureSelection",
                                                                       "Greater Eldritch Heritage",
                                                                       "ou gain an additional power from the bloodline you selected with the Eldritch Heritage feat. You gain a 15th-level sorcerer bloodline power. For purposes of using that power, treat your character level as your sorcerer level for all your sorcerer bloodline powers granted by this feat, Eldritch Heritage, and Improved Eldritch Heritage.",
                                                                       "",
                                                                       null,
                                                                       FeatureGroup.Feat,
                                                                       Helpers.PrerequisiteStatValue(Kingmaker.EntitySystem.Stats.StatType.Charisma, 17),
                                                                       Helpers.PrerequisiteCharacterLevel(17),
                                                                       Helpers.PrerequisiteFeature(improved_eldritch_heritage)
                                                                       );

            var skill_foci_map = Common.getSkillFociMap();

            var fake_sorcerer = library.CopyAndAdd(sorcerer, "FakeSorcererClass", "");
            eldritch_heritage.AddComponents(Helpers.Create<FakeClassLevelMechanics.AddFakeClassLevel>(a =>
            {
                a.fake_class = fake_sorcerer;
                a.value = Helpers.CreateContextValue(AbilityRankType.Default);
            }
                                            ),
                                            Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.BonusValue,
                                                                            stepLevel: -2)
                                           );

            greater_eldritch_heritage.AddComponent(Helpers.Create<FakeClassLevelMechanics.AddFakeClassLevel>(a =>
            {
                a.fake_class = fake_sorcerer;
                a.value = 2;
            }
                                                    )
                                                  );

            var infernal_bloodline = library.Get<BlueprintProgression>("e76a774cacfb092498177e6ca706064d");
            foreach (var b in bloodlines)
            {
                List<StatType> bloodline_skills = new List<StatType>();
                var bp = b as BlueprintProgression;
                var skill_features = bp.LevelEntries[0].Features.Where(f => f.name.Contains("ClassSkill")).ToArray();
                if (skill_features[0] is BlueprintFeatureSelection)
                {
                    skill_features = (skill_features[0] as BlueprintFeatureSelection).AllFeatures;
                }

                var level1_feature = bp.LevelEntries.Where(l => l.Level == 1).FirstOrDefault().Features.Where(f => !f.name.Contains("ClassSkill") && !f.Name.Contains("Arcana")).FirstOrDefault() as BlueprintFeature;
                if (level1_feature is BlueprintProgression)
                {
                    Main.logger.Log("Eldritch Heritage: Skipping " + b.Name + ". " + "Level 1 feature " + level1_feature.Name + " is progression");
                    continue;
                }

                var level3_feature = bp.LevelEntries.Where(l => l.Level == 3).FirstOrDefault().Features.Where(f => !f.name.Contains("SpellLevel")).FirstOrDefault() as BlueprintFeature;
                var level9_feature = bp.LevelEntries.Where(l => l.Level == 9).FirstOrDefault().Features.Where(f => !f.name.Contains("SpellLevel")).FirstOrDefault() as BlueprintFeature;
                var level15_feature = bp.LevelEntries.Where(l => l.Level == 15).FirstOrDefault().Features.Where(f => !f.name.Contains("SpellLevel")).FirstOrDefault() as BlueprintFeature;


                var eldritch_heritage_lvl1 = level1_feature is BlueprintFeatureSelection ? library.CopyAndAdd(level1_feature, "EldritchHeritage" + bp.name + level1_feature.name, "") : Common.featureToFeature(level1_feature, false, prefix: "EldritchHeritage" + bp.name);
                eldritch_heritage_lvl1.Groups = new FeatureGroup[] { FeatureGroup.Feat };
                eldritch_heritage_lvl1.SetNameDescription(eldritch_heritage.Name + ": " + bp.Name + $" ({eldritch_heritage_lvl1.Name})",
                                                          eldritch_heritage.Description + "\n" + level1_feature.Name + ": " + level1_feature.Description
                                                          );
                foreach (var sf in skill_features)
                {
                    var skill = sf.GetComponent<AddClassSkill>().Skill;
                    eldritch_heritage_lvl1.AddComponent(Helpers.PrerequisiteFeature(skill_foci_map[skill], any: skill_features.Length > 1));
                }
                eldritch_heritage.AllFeatures = eldritch_heritage.AllFeatures.AddToArray(eldritch_heritage_lvl1);
                ClassToProgression.addClassToFact(fake_sorcerer, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.NoSpells, eldritch_heritage_lvl1, sorcerer);

                if (!(level3_feature is BlueprintProgression))
                {
                    var eldritch_heritage_lvl3 = level3_feature is BlueprintFeatureSelection ? library.CopyAndAdd(level3_feature, "ImprovedEldritchHeritage3" + bp.name + level3_feature.name, "") : Common.featureToFeature(level3_feature, false, prefix: "ImprovedEldritchHeritage3" + bp.name);
                    eldritch_heritage_lvl3.Groups = new FeatureGroup[] { FeatureGroup.Feat };
                    eldritch_heritage_lvl3.SetNameDescription(improved_eldritch_heritage.Name + ": " + bp.Name + $" ({eldritch_heritage_lvl3.Name})",
                                                              improved_eldritch_heritage.Description + "\n" + level3_feature.Name + ": " + level3_feature.Description
                                                              );
                    eldritch_heritage_lvl3.AddComponent(Helpers.PrerequisiteFeature(eldritch_heritage_lvl1));
                    improved_eldritch_heritage.AllFeatures = improved_eldritch_heritage.AllFeatures.AddToArray(eldritch_heritage_lvl3);
                    ClassToProgression.addClassToFact(fake_sorcerer, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.NoSpells, eldritch_heritage_lvl3, sorcerer);
                }
                else
                {
                    Main.logger.Log("Eldritch Heritage: Skipping " + b.Name + " " + "Level 3 feature " + level3_feature.Name + " (progression)");
                }


                if (!(level9_feature is BlueprintProgression))
                {
                    var eldritch_heritage_lvl9 = level9_feature is BlueprintFeatureSelection ? library.CopyAndAdd(level9_feature, "ImprovedEldritchHeritage9" + bp.name + level9_feature.name, "") : Common.featureToFeature(level9_feature, false, prefix: "ImprovedEldritchHeritage9" + bp.name);
                    eldritch_heritage_lvl9.Groups = new FeatureGroup[] { FeatureGroup.Feat };
                    eldritch_heritage_lvl9.SetNameDescription(improved_eldritch_heritage.Name + ": " + bp.Name + $" ({eldritch_heritage_lvl9.Name})",
                                                              improved_eldritch_heritage.Description + "\n" + level9_feature.Name + ": " + level9_feature.Description
                                                              );
                    eldritch_heritage_lvl9.AddComponent(Helpers.PrerequisiteFeature(eldritch_heritage_lvl1));
                    improved_eldritch_heritage.AllFeatures = improved_eldritch_heritage.AllFeatures.AddToArray(eldritch_heritage_lvl9);
                    ClassToProgression.addClassToFact(fake_sorcerer, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.NoSpells, eldritch_heritage_lvl9, sorcerer);
                }
                else
                {
                    Main.logger.Log("Eldritch Heritage: Skipping " + b.Name + " " + "Level 9 feature " + level9_feature.Name + " (progression)");
                }


                var eldritch_heritage_lvl15 = level15_feature is BlueprintFeatureSelection ? library.CopyAndAdd(level15_feature, "ImprovedEldritchHeritage15" + bp.name + level15_feature.name, "") : Common.featureToFeature(level15_feature, false, prefix: "GreaterEldritchHeritage" + bp.name);
                eldritch_heritage_lvl15.Groups = new FeatureGroup[] { FeatureGroup.Feat };
                eldritch_heritage_lvl15.SetNameDescription(greater_eldritch_heritage.Name + ": " + bp.Name + $" ({eldritch_heritage_lvl15.Name})",
                                                          greater_eldritch_heritage.Description + "\n" + level15_feature.Name + ": " + level15_feature.Description
                                                          );
                eldritch_heritage_lvl15.AddComponent(Helpers.PrerequisiteFeature(eldritch_heritage_lvl1));
                greater_eldritch_heritage.AllFeatures = greater_eldritch_heritage.AllFeatures.AddToArray(eldritch_heritage_lvl15);
                ClassToProgression.addClassToFact(fake_sorcerer, new BlueprintArchetype[0], ClassToProgression.DomainSpellsType.NoSpells, eldritch_heritage_lvl15, sorcerer);

                eldritch_heritage.AddComponent(Helpers.PrerequisiteNoFeature(b));
                b.AddComponent(Helpers.PrerequisiteNoFeature(eldritch_heritage));

                if (b == infernal_bloodline)
                {
                    eldritch_heritage.AddComponent(Common.prerequisiteNoArchetype(Summoner.devil_binder));
                }
            }

            sorcerer.AddComponent(Helpers.PrerequisiteNoFeature(eldritch_heritage));
            dragon_disciple.AddComponent(Helpers.PrerequisiteNoFeature(eldritch_heritage));


            library.AddFeats(eldritch_heritage, improved_eldritch_heritage, greater_eldritch_heritage);
        }

        static void fixBloodlineSpells()
        {
            var draconic = library.Get<BlueprintProgression>("7bd143ead2d6c3a409aad6ee22effe34"); //black
            Common.replaceSorcererBloodlineSpell(draconic, NewSpells.fly, 3);
            var arcane = library.Get<BlueprintProgression>("4d491cf9631f7e9429444f4aed629791");
            Common.replaceSorcererBloodlineSpell(arcane, NewSpells.overland_flight, 5);
            var fey = library.Get<BlueprintProgression>("e8445256abbdc45488c2d90373f7dae8");
            Common.replaceSorcererBloodlineSpell(fey, NewSpells.irresistible_dance, 8);
            Common.replaceSorcererBloodlineSpell(fey, Wildshape.shapechange, 9);
            var infernal = library.Get<BlueprintProgression>("e76a774cacfb092498177e6ca706064d");
            Common.replaceSorcererBloodlineSpell(infernal, library.Get<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61"), 5); //dominate person
            Common.replaceSorcererBloodlineSpell(infernal, NewSpells.meteor_swarm, 9);
            var serpentine = library.Get<BlueprintProgression>("739c1e842bf77994baf963f4ad964379");
            Common.replaceSorcererBloodlineSpell(serpentine, NewSpells.irresistible_dance, 8);
            Common.replaceSorcererBloodlineSpell(serpentine, library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757"), 9); //dominate monster
        }


        static void createBloodlineFamiliar()
        {
            bloodline_familiar = library.CopyAndAdd<BlueprintFeatureSelection>("363cab72f77c47745bf3a8807074d183", "BloodlineFamiliarFeatureSelection", "");
            bloodline_familiar.SetNameDescription("Bloodline Familiar", "Those with an inherent connection to magic often attract creatures who feel a similar instinctive pull toward magical forces. At 1st level, a sorcerer, bloodrager or any other class with bloodline powers can choose to gain a bloodline familiar instead of 1st-level bloodline power.");
            bloodline_familiar.Groups = new FeatureGroup[0];
            bloodline_familiar.ComponentsArray = new BlueprintComponent[0];
        }


        static void fixDragonDisciplePrerequisites()
        {
            var dragon_disiciple = library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");

            var no_sorc = dragon_disciple.GetComponent<PrerequisiteNoFeature>();
            var allowed_bloodlines = dragon_disciple.GetComponent<PrerequisiteFeaturesFromList>();

            no_sorc.Group = Prerequisite.GroupType.All;
            allowed_bloodlines.Group = Prerequisite.GroupType.All;
            dragon_disciple.RemoveComponent(no_sorc);
            dragon_disciple.RemoveComponent(allowed_bloodlines);
            dragon_disciple.AddComponent(Helpers.Create<PrerequisiteMechanics.PrerequsiteOrAlternative>(p => { p.base_prerequsite = no_sorc; p.alternative_prerequsite = allowed_bloodlines; }));

        }


        static void fixArcaneBloodline()
        {
            var resource = Helpers.CreateAbilityResource("ArcaneBloodlineMetamagicAdeptResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(0, 3, 1, 4, 1, 0, 0.0f, new BlueprintCharacterClass[] { sorcerer, magus}, new BlueprintArchetype[] { eldritch_scion });

            var metamagic_adept = library.Get<BlueprintFeature>("7aa83ee3526a946419561d8d1aa09e75");
            var icon = library.Get<BlueprintAbility>("92681f181b507b34ea87018e8f7a528a").Icon;
            metamagic_adept.SetNameDescriptionIcon("Metamagic Adept",
                                                   "At 3rd level, you can apply any one metamagic feat you know to a spell you are about to cast without increasing the casting time. You must still expend a higher-level spell slot to cast this spell. You can use this ability once per day at 3rd level and one additional time per day for every four sorcerer levels you possess beyond 3rd, up to five times per day at 19th level. At 20th level, this ability is replaced by arcane apotheosis.",
                                                   icon);

            var buff = Helpers.CreateBuff("MetamagicAdeptBuff",
                                          metamagic_adept.Name,
                                          metamagic_adept.Description,
                                          "",
                                          metamagic_adept.Icon,
                                          null,
                                          Helpers.Create<NewMechanics.MetamagicAdept>(m => m.resource = resource),
                                          Helpers.Create<SpellManipulationMechanics.NoSpontnaeousMetamagicCastingTimeIncreaseIfLessMetamagic>(n => n.max_metamagics = 1)
                                          );
            var ability = Helpers.CreateActivatableAbility("MetamagicAdeptToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;

            metamagic_adept.ComponentsArray = new BlueprintComponent[] {Helpers.CreateAddAbilityResource(resource),
                                                                        Helpers.CreateAddFact(ability)};

            var arcane_apotheosis = library.Get<BlueprintFeature>("2086d8c0d40e35b40b86d47e47fb17e4");
            arcane_apotheosis.SetDescription("At 20th level, your body surges with arcane power. You can add any metamagic feats that you know to your spells without increasing their casting time, although you must still expend higher-level spell slots.");
            arcane_apotheosis.SetComponents(Helpers.Create<SpellManipulationMechanics.NoSpontnaeousMetamagicCastingTimeIncreaseIfLessMetamagic>(n => n.max_metamagics = 100),
                                            Common.createRemoveFeatureOnApply(metamagic_adept));
        }



        static void createBloodIntensity()
        {
            var resource = Helpers.CreateAbilityResource("BloodIntensityResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(0, 3, 1, 4, 1, 0, 0.0f, new BlueprintCharacterClass[] { sorcerer, magus, Bloodrager.bloodrager_class }, new BlueprintArchetype[] { eldritch_scion });

            var buff = Helpers.CreateBuff("BloodIntensityBuff",
                                          "Blood Intensity",
                                          "Whenever you cast a bloodrager or sorcerer spell that deals damage, you can increase its maximum number of damage dice by an amount equal to your Strength or Charisma modifier, whichever is higher. This otherwise functions as —and does not stack with—the Intensified Spell feat. You can use this ability once per day at 3rd level and one additional time per day for every 4 caster levels you have beyond 3rd, up to five times per day at 19th level.",
                                          "",
                                          MetamagicFeats.intensified_metamagic.Icon,
                                          null,
                                          //Helpers.Create<SpellManipulationMechanics.NoSpontnaeousMetamagicCastingTimeIncreaseIfLessMetamagic>(n => n.max_metamagics = 0), //blood intensity does not increase casting time
                                          Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(m =>
                                                                                                                      {
                                                                                                                          m.amount = 1;
                                                                                                                          m.resource = resource;
                                                                                                                          m.spell_descriptor = SpellDescriptor.None;
                                                                                                                          m.Metamagic = (Metamagic)MetamagicFeats.MetamagicExtender.BloodIntensity;
                                                                                                                      })
                                          );

            var ability = Helpers.CreateActivatableAbility("BloodIntensityToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;
                                          
            blood_intensity = Helpers.CreateFeature("BloodIntensityFeature",
                                                "Blood Intensity",
                                                "Whenever you cast a bloodrager or sorcerer spell that deals damage, you can increase its maximum number of damage dice by an amount equal to your Strength or Charisma modifier, whichever is higher. This otherwise functions as —and does not stack with—the Intensified Spell feat. You can use this ability once per day at 3rd level and one additional time per day for every 4 caster levels you have beyond 3rd, up to five times per day at 19th level.",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Helpers.CreateAddAbilityResource(resource),
                                                Helpers.CreateAddFact(ability),
                                                Helpers.PrerequisiteClassLevel(sorcerer, 3, any: true),
                                                Helpers.PrerequisiteClassLevel(Bloodrager.bloodrager_class, 8, any: true)//,
                                                //Common.createPrerequisiteArchetypeLevel(magus, eldritch_scion, 3, any: true)
                                                );
        }


        static void createBloodPiercing()
        {
            var resource = Helpers.CreateAbilityResource("BloodPiercingResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(0, 4, 1, 5, 1, 0, 0.0f, new BlueprintCharacterClass[] { sorcerer, magus, Bloodrager.bloodrager_class }, new BlueprintArchetype[] { eldritch_scion });

            var buff = Helpers.CreateBuff("BloodPiercingBuff",
                                          "Blood Piercing",
                                          "When you cast a bloodrager or sorcerer spell that deals damage, creatures affected by the spell reduce their energy resistance and spell resistance against the spell’s effects by an amount equal to your Strength or Charisma modifier, whichever is higher. You can use this ability once per day at 4th level and one additional time per day for every 5 caster levels you have beyond 3rd, up to four times per day at 18th level.",
                                          "",
                                          MetamagicFeats.piercing_metamagic.Icon,
                                          null,
                                          //Helpers.Create<SpellManipulationMechanics.NoSpontnaeousMetamagicCastingTimeIncreaseIfLessMetamagic>(n => n.max_metamagics = 0), //blood intensity does not increase casting time
                                          Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(m =>
                                          {
                                              m.amount = 1;
                                              m.resource = resource;
                                              m.spell_descriptor = SpellDescriptor.None;
                                              m.Metamagic = (Metamagic)MetamagicFeats.MetamagicExtender.BloodPiercing;
                                          })
                                          );

            var ability = Helpers.CreateActivatableAbility("BloodPiercingToggleAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                           Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(resource, Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;

            blood_piercing = Helpers.CreateFeature("BloodPiercingFeature",
                                                "Blood Piericng",
                                                buff.Description,
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Helpers.CreateAddAbilityResource(resource),
                                                Helpers.CreateAddFact(ability),
                                                Helpers.PrerequisiteClassLevel(sorcerer, 9, any: true),
                                                Helpers.PrerequisiteClassLevel(Bloodrager.bloodrager_class, 4, any: true)                                                 
                                                );
        }


        static void createBloodHavoc()
        {
            Dictionary<string, List<BlueprintFeature>> spell_asset_id_feature_map = new Dictionary<string, List<BlueprintFeature>>();

            var bloodlines = library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914").AllFeatures.Cast<BlueprintProgression>().ToList();

            foreach (var b in bloodlines)
            {
                foreach (var le in b.LevelEntries)
                {
                    foreach (var f in le.Features)
                    {
                        if (f.name.Contains("SpellLevel"))
                        {
                            var spell = f.GetComponent<AddKnownSpell>()?.Spell;
                            if (spell == null)
                            {
                                continue;
                            }
                            
                            if (!spell_asset_id_feature_map.ContainsKey(spell.AssetGuid))
                            {
                                spell_asset_id_feature_map[spell.AssetGuid] = new List<BlueprintFeature>();
                            }
                            //Main.logger.Log("Added: " + spell.name + " --- " + f.name);
                            spell_asset_id_feature_map[spell.AssetGuid].Add(f as BlueprintFeature);
                        }
                    }
                }
            }
            NewMechanics.BloodHavoc.spell_asset_id_feature_map = spell_asset_id_feature_map;
            blood_havoc = Helpers.CreateFeature("BloodHavocFeature",
                                                "Blood Havoc",
                                                $"Whenever you cast a bloodrager or sorcerer spell that deals damage, add 1 point of damage per die rolled. This benefit applies only to damaging spells {(Main.settings.balance_fixes ? "" : "that belong to schools you have selected with Spell Focus or ")}that are bloodline spells for your bloodline.",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Helpers.Create<NewMechanics.BloodHavoc>(b => b.feature = Main.settings.balance_fixes ? null : library.Get<BlueprintParametrizedFeature>("16fa59cc9a72a6043b566b49184f53fe")),
                                                Helpers.PrerequisiteClassLevel(sorcerer, 1, any: true),
                                                Helpers.PrerequisiteClassLevel(Bloodrager.bloodrager_class, 4, any: true)//,
                                                //Common.createPrerequisiteArchetypeLevel(magus, eldritch_scion, 1, any: true)
                                                );
        }


        static void addBloodlineMutations()
        {
            var feats_selections = new BlueprintFeatureSelection[] {library.Get<BlueprintFeatureSelection>("3a60f0c0442acfb419b0c03b584e1394"), //sorcerer
                                                                    Bloodrager.bloodline_feat_selection,
                                                                   };

            foreach (var fs in feats_selections)
            {
                fs.AllFeatures = fs.AllFeatures.AddToArray(blood_havoc, blood_intensity, blood_piercing);
            }

            var bloodlines = library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914").AllFeatures.Cast<BlueprintProgression>().ToList();

            foreach (var b in bloodlines)
            {
                var ability1 = b.LevelEntries[0].Features.Where(f => !f.name.Contains("Arcana") && !f.name.Contains("ClassSkill")).FirstOrDefault() as BlueprintFeature;
                var ability3 = b.LevelEntries[1].Features.Where(f => !f.name.Contains("SpellLevel1")).FirstOrDefault() as BlueprintFeature;
                var ability9 = b.LevelEntries[4].Features.Where(f => !f.name.Contains("SpellLevel4")).FirstOrDefault() as BlueprintFeature;

                var selection = Helpers.CreateFeatureSelection(ability1.name + "Selection",
                                                               ability1.Name,
                                                               ability1.Description,
                                                               "",
                                                               ability1.Icon,
                                                               FeatureGroup.None);
                selection.AllFeatures = new BlueprintFeature[] { ability1, blood_havoc, bloodline_familiar };

                var selection3 = Helpers.CreateFeatureSelection(ability3.name + "Selection",
                                               ability3.Name,
                                               ability3.Description,
                                               "",
                                               ability3.Icon,
                                               FeatureGroup.None);
                selection3.AllFeatures = new BlueprintFeature[] { ability3, blood_intensity };
                var selection9 = Helpers.CreateFeatureSelection(ability9.name + "Selection",
                               ability9.Name,
                               ability9.Description,
                               "",
                               ability9.Icon,
                               FeatureGroup.None);
                selection9.AllFeatures = new BlueprintFeature[] { ability9, blood_piercing };

                b.UIGroups[0].Features.Remove(ability1);
                b.UIGroups[0].Features.Add(selection);
                b.UIGroups[0].Features.Remove(ability3);
                b.UIGroups[0].Features.Add(selection3);
                b.UIGroups[0].Features.Add(selection9);
                b.LevelEntries[0].Features.Remove(ability1);
                b.LevelEntries[0].Features.Add(selection);
                b.LevelEntries[1].Features.Remove(ability3);
                b.LevelEntries[1].Features.Add(selection3);
                b.LevelEntries[4].Features.Remove(ability9);
                b.LevelEntries[4].Features.Add(selection9);
            }

            foreach (var b in Bloodrager.bloodlines.Values)
            {
                var ability0 = b.progression.LevelEntries[0].Features[0] as BlueprintFeature;
                var ability1 = b.progression.LevelEntries[1].Features[0] as BlueprintFeature;
                var ability2 = b.progression.LevelEntries[3].Features[0] as BlueprintFeature;

                var es_b = Bloodrager.bloodrager_eldritch_scion_bloodlines_map[b.progression];

                var selection0 = Helpers.CreateFeatureSelection(ability0.name + "Selection",
                                               ability0.Name,
                                               ability0.Description,
                                               "",
                                               ability0.Icon,
                                               FeatureGroup.None);
                selection0.AllFeatures = new BlueprintFeature[] { ability0, bloodline_familiar };
                b.progression.UIGroups[0].Features.Remove(ability0);
                b.progression.UIGroups[0].Features.Add(selection0);
                b.progression.LevelEntries[0].Features.Remove(ability0);
                b.progression.LevelEntries[0].Features.Add(selection0);
                es_b.UIGroups[0].Features.Remove(ability0);
                es_b.UIGroups[0].Features.Add(selection0);
                es_b.LevelEntries[0].Features.Remove(ability0);
                es_b.LevelEntries[0].Features.Add(selection0);

                var selection = Helpers.CreateFeatureSelection(ability1.name + "Selection",
                                                               ability1.Name,
                                                               ability1.Description,
                                                               "",
                                                               ability1.Icon,
                                                               FeatureGroup.None);
                selection.AllFeatures = new BlueprintFeature[] { ability1, blood_havoc, blood_piercing };
                b.progression.UIGroups[0].Features.Remove(ability1);
                b.progression.UIGroups[0].Features.Add(selection);
                b.progression.LevelEntries[1].Features.Remove(ability1);
                b.progression.LevelEntries[1].Features.Add(selection);
                es_b.UIGroups[0].Features.Remove(ability1);
                es_b.UIGroups[0].Features.Add(selection);
                es_b.LevelEntries[1].Features.Remove(ability1);
                es_b.LevelEntries[1].Features.Add(selection);

                var selection2 = Helpers.CreateFeatureSelection(ability2.name + "Selection",
                                               ability2.Name,
                                               ability2.Description,
                                               "",
                                               ability2.Icon,
                                               FeatureGroup.None);
                selection2.AllFeatures = new BlueprintFeature[] { ability2, blood_intensity};
                b.progression.UIGroups[0].Features.Remove(ability2);
                b.progression.UIGroups[0].Features.Add(selection2);
                b.progression.LevelEntries[3].Features.Remove(ability2);
                b.progression.LevelEntries[3].Features.Add(selection2);
                es_b.UIGroups[0].Features.Remove(ability2);
                es_b.UIGroups[0].Features.Add(selection2);
                es_b.LevelEntries[3].Features.Remove(ability2);
                es_b.LevelEntries[3].Features.Add(selection2);
            }
        }

        static void fixBloodlinesUI()
        {
            var bloodlines = library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914").AllFeatures.Cast<BlueprintProgression>().ToList();
            bloodlines.Add(library.Get<BlueprintProgression>("a46d4bd93601427409d034a997673ece")); //sylvan
            bloodlines.Add(library.Get<BlueprintProgression>("7d990675841a7354c957689a6707c6c2")); //sage
            bloodlines.Add(library.Get<BlueprintProgression>("8a95d80a3162d274896d50c2f18bb6b1")); //empyreal

            foreach (var b in bloodlines)
            {
                List<BlueprintFeature> spells = new List<BlueprintFeature>();
                List<BlueprintFeature> abilities = new List<BlueprintFeature>();

                foreach (var le in b.LevelEntries)
                {
                    foreach (var f in le.Features)
                    {
                        if (f.name.Contains("SpellLevel"))
                        {
                            spells.Add(f as BlueprintFeature);
                        }
                        else if (!f.name.Contains("ClassSkill") && !(le.Level == 1 && f.name.Contains("Arcana")))
                        {
                            abilities.Add(f as BlueprintFeature);
                        }
                    }
                }
                b.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(abilities.ToArray()), Helpers.CreateUIGroup(spells.ToArray()) };
            }
        }


        static void fixSorcBloodlineProgression()
        {
            //put draconic claws and breath weapon in one first level feat
            var draconic_progression = library.GetAllBlueprints().Where(b => (b is BlueprintProgression) && b.name.Contains("BloodlineDraconic") && !b.name.Contains("Bloodrager")).Cast<BlueprintProgression>().ToArray();

            foreach (var d in draconic_progression)
            {
                convolveBloodlineFeatures(d, "ClawsFeatureAddLevel1", "ClawsFeatureAddLevel", sorcerer, magus, dragon_disciple);
                convolveBloodlineFeatures(d, "BreathWeaponBaseFeature", "BreathWeaponExtraUse", sorcerer, magus, dragon_disciple);
                convolveBloodlineFeatures(d, "ResistancesAbilityAddLevel1", "ResistancesAbilityAddLevel", sorcerer, magus, dragon_disciple);
                d.LevelEntries[4].Features[0].HideInUI = false;
            }

            var serpentine_bloodline = library.Get<BlueprintProgression>("739c1e842bf77994baf963f4ad964379");
            convolveBloodlineFeatures(serpentine_bloodline, "SerpentsFangBiteFeatureAddLevel1", "SerpentsFangBiteFeatureAddLevel", sorcerer, magus);
            convolveBloodlineFeatures(serpentine_bloodline, "SnakeskinFeatureAddLevel1", "SnakeskinFeatureAddLevel", sorcerer, magus);

            var infernal_bloodline = library.Get<BlueprintProgression>("e76a774cacfb092498177e6ca706064d");
            convolveBloodlineFeatures(infernal_bloodline, "HellfireFeature", "HellfireExtraUse", sorcerer, magus);

            var undead_bloodline = library.Get<BlueprintProgression>("a1a8bf61cadaa4143b2d4966f2d1142e");
            convolveBloodlineFeatures(undead_bloodline, "BloodlineUndeadGraspOfTheDeadFeature", "GraspOfTheDeadExtraUse", sorcerer, magus);
            var deaths_gift = library.Get<BlueprintFeature>("fd5f14d44e82f464196fdf0ea82347cc");
            replaceRepeatingFeatureWithProgression(undead_bloodline, deaths_gift);

            //fix elemental bloodlines
            var elemental_progression = library.GetAllBlueprints().Where(b => (b is BlueprintProgression) && b.name.Contains("BloodlineElemental") && !b.name.Contains("Bloodrager")).Cast<BlueprintProgression>().ToArray();

            foreach (var e in elemental_progression)
            {
                convolveBloodlineFeatures(e, "ElementalBlastFeature", "ElementalBlastExtraUse", sorcerer, magus);
            }

            var water_elemental_movement = library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8");
            removeExtraEntries(library.Get<BlueprintProgression>("7c692e90592257a4e901d12ae6ec1e41"), water_elemental_movement); //second water elemental movement entry
            replaceFeature(library.Get<BlueprintProgression>("32393034410fb2f4d9c8beaa5c8c8ab7"), library.Get<BlueprintFeature>("277d9c2a2392e8940a452888aa67f32e"), water_elemental_movement); //earth elemental movement

            //celestial or empyreal remove wings at level 15

            var celestial_bloodline = library.Get<BlueprintProgression>("aa79c65fa0e11464d9d100b038c50796");
            var empyreal_bloodline = library.Get<BlueprintProgression>("8a95d80a3162d274896d50c2f18bb6b1");
            var aura = library.Get<BlueprintFeature>("2768c719ee7338c49932358c2c581bba");
            var celestial_wings = library.Get<BlueprintFeature>("894d21ff523481d49a51ab1750fca3a0");

            replaceFeature(celestial_bloodline, celestial_wings, null);
            replaceFeature(empyreal_bloodline, celestial_wings, null);
            replaceFeature(celestial_bloodline, aura, celestial_wings);

            var abyssal_bloodline = library.Get<BlueprintProgression>("d3a4cb7be97a6694290f0dcfbd147113");
            var demon_wings = library.Get<BlueprintFeature>("36db25d9e0848f04da604ff9e3d931af");
            replaceFeature(abyssal_bloodline, demon_wings, null);
            convolveBloodlineFeatures(abyssal_bloodline, "ClawsFeatureAddLevel1", "ClawsFeatureAddLevel", sorcerer, magus);
            convolveBloodlineFeatures(abyssal_bloodline, "StrengthAbilityAddLevel1", "StrengthAbilityAddLevel", sorcerer, magus);

            var arcane_bloodline = library.Get<BlueprintProgression>("4d491cf9631f7e9429444f4aed629791");
            var sage_bloodline = library.Get<BlueprintProgression>("7d990675841a7354c957689a6707c6c2");
            convolveBloodlineFeatures(arcane_bloodline, "CombatCastingAdeptFeatureAddLevel1", "CombatCastingAdeptFeatureAddLevel", sorcerer, magus);
            

            var new_arcana = library.Get<BlueprintFeature>("20a2435574bdd7f4e947f405df2b25ce");
            var arcana_progression =  replaceRepeatingFeatureWithProgression(arcane_bloodline, new_arcana);
            removeExtraEntries(sage_bloodline, new_arcana);
            replaceFeature(sage_bloodline, new_arcana, arcana_progression);
            replaceFeature(sage_bloodline, library.Get<BlueprintFeature>("3d7b19c8a1d03464aafeb306342be000"), null);
        }






        static void convolveBloodlineFeatures(BlueprintProgression bloodline, string primary_feature_id, string secondary_feature_id, params BlueprintCharacterClass[] classes)
        {
            BlueprintFeature primary_feature = null;
            for (int i = 0; i < bloodline.LevelEntries.Length; i++)
            {
                foreach (var f in bloodline.LevelEntries[i].Features.ToArray())
                {
                    if (f.name.Contains(primary_feature_id))
                    {
                        primary_feature = f as BlueprintFeature;
                    }
                    else if (f.name.Contains(secondary_feature_id))
                    {
                        primary_feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(f as BlueprintFeature, bloodline.LevelEntries[i].Level,
                                                                                        classes,
                                                                                        new BlueprintArchetype[] { eldritch_scion }));
                        bloodline.LevelEntries[i].Features.Remove(f);
                    }
                }
            }
        }


        static void removeExtraEntries(BlueprintProgression bloodline, BlueprintFeature feature)
        {
            bool found = false;
            for (int i = 0; i < bloodline.LevelEntries.Length; i++)
            {
                foreach (var f in bloodline.LevelEntries[i].Features.ToArray())
                {
                    if (f == feature && !found)
                    {
                        found = true;
                    }
                    else if (f == feature)
                    {
                        bloodline.LevelEntries[i].Features.Remove(f);
                    }
                }
            }
        }


        static void replaceFeature(BlueprintProgression bloodline, BlueprintFeature feature, BlueprintFeature replacement)
        {

            for (int i = 0; i < bloodline.LevelEntries.Length; i++)
            {
                if (replacement == null)
                {
                    bloodline.LevelEntries[i].Features.RemoveAll(f => f == feature);
                }
                else
                {
                    for (int j = 0; j < bloodline.LevelEntries[i].Features.Count; j++)
                    {
                        if (bloodline.LevelEntries[i].Features[j] == feature )
                        {
                            bloodline.LevelEntries[i].Features[j] = replacement;
                        }
                    }
                }
            }
        }


        static BlueprintProgression replaceRepeatingFeatureWithProgression(BlueprintProgression bloodline, BlueprintFeature feature)
        {
            List<LevelEntry> level_entries = new List<LevelEntry>();

            var feature_progression = Helpers.CreateProgression(feature.name + "Progression",
                                                                feature.Name,
                                                                feature.Description,
                                                                "",
                                                                feature.Icon,
                                                                FeatureGroup.None);
            feature_progression.Classes = bloodline.Classes;
            feature_progression.Archetypes = bloodline.Archetypes;
            feature_progression.HideInCharacterSheetAndLevelUp = feature.HideInCharacterSheetAndLevelUp;

            for (int i = 0; i < bloodline.LevelEntries.Length; i++)
            {
                foreach (var f in bloodline.LevelEntries[i].Features.ToArray())
                {
                    if (f == feature)
                    {
                        if (level_entries.Count == 0)
                        {
                            bloodline.LevelEntries[i].Features.Add(feature_progression);
                        }
                        level_entries.Add(Helpers.LevelEntry(bloodline.LevelEntries[i].Level, feature));
                        bloodline.LevelEntries[i].Features.Remove(f);
                    }
                }
            }
            feature_progression.LevelEntries = level_entries.ToArray();
            return feature_progression;
        }
    }



}
