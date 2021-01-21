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
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.ElementsSystem;

namespace CallOfTheWild
{
    public class Bloodrager
    {
        static internal LibraryScriptableObject library => Main.library;
        static internal bool test_mode = false;
        static public BlueprintCharacterClass bloodrager_class;
        static public BlueprintProgression bloodrager_progression;
        static public BlueprintFeatureSelection bloodline_selection;

        static public BlueprintFeature bloodrage;
        static public BlueprintActivatableAbility bloodrage_ability;
        static public BlueprintFeature greater_bloodrage;
        static public BlueprintFeature mighty_bloodrage;
        static public BlueprintFeature tireless_bloodrage;
        static public BlueprintBuff bloodrage_buff;
        static public BlueprintAbilityResource bloodrage_resource;
        static public BlueprintFeature damage_reduction;
        static public BlueprintFeature uncanny_dodge;
        static public BlueprintFeature improved_uncanny_dodge;
        static public BlueprintFeature fast_movement;
        static public BlueprintFeature indomitable_will;
        static public BlueprintFeature bloodrager_proficiencies;
        static public BlueprintFeature blood_sanctuary;

        static public BlueprintArchetype metamagic_rager_archetype;
        static public BlueprintArchetype steelblood_archetype;
        static public BlueprintArchetype spelleater_archetype;

        static public BlueprintFeature metarage;
        static public BlueprintFeature blood_of_life;
        static public BlueprintFeature spell_eating;
        static public BlueprintFeature steelblood_proficiencies;
        static public BlueprintFeature blood_deflection;
        static public BlueprintFeature blood_deflection_bonus;
        static public BlueprintFeature armor_training;
        static public BlueprintFeatureSelection bloodline_feat_selection;

        static public BlueprintArchetype urban_bloodrager;
        static public BlueprintFeature urban_bloodrage;
        static public BlueprintBuff[] urban_bloodrage_buffs = new BlueprintBuff[3];
        static public BlueprintFeature restrained_magic;
        static public BlueprintFeature urban_bloodrager_proficiencies;

        static public ActivatableAbilityGroup metarage_group = ActivatableAbilityGroupExtension.MetaRage.ToActivatableAbilityGroup();
        static public BlueprintFeature blood_casting;
        
        static public BlueprintFeatureSelection adopted_magic;
        static public BlueprintArchetype eldritch_scion_bloodrager;


        static public BlueprintArchetype blood_conduit;
        static public BlueprintFeatureSelection contact_specialist;
        static public BlueprintFeature spell_conduit;
        static public BlueprintFeature reflexive_conduit;

        static public BlueprintBuff eldritch_scion_buff;
        static public BlueprintFeature mystical_focus;

        static public Dictionary<BlueprintProgression, BlueprintProgression> bloodrager_eldritch_scion_bloodlines_map = new Dictionary<BlueprintProgression, BlueprintProgression>();
        public class BloodlineInfo
        {
            public BlueprintProgression progression;
            public BlueprintFeatureSelection bonus_feats;

            public BloodlineInfo(BlueprintProgression bloodline_progression, BlueprintFeatureSelection bloodline_feats)
            {
                progression = bloodline_progression;
                bonus_feats = bloodline_feats;
            }
        }

        static public Dictionary<String, BloodlineInfo> bloodlines = new Dictionary<String, BloodlineInfo>();

        internal static void createBloodragerClass()
        {
            Main.logger.Log("Bloodrager class test mode: " + test_mode.ToString());
            var barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

            bloodrager_class = Helpers.Create<BlueprintCharacterClass>();
            bloodrager_class.name = "BloodragerClass";
            library.AddAsset(bloodrager_class, "cf217eb4f8504d67aad37464cee966f8");

            bloodrager_class.LocalizedName = Helpers.CreateString("Bloodrager.Name", "Bloodrager");
            bloodrager_class.LocalizedDescription = Helpers.CreateString("Bloodrager.Description",
                                                                         "While many ferocious combatants can tap into a deep reservoir of buried rage, bloodragers have an intrinsic power that seethes within.Like sorcerers, bloodragers’ veins surge with arcane power.While sorcerers use this power for spellcasting, bloodragers enter an altered state in which their bloodline becomes manifest, where the echoes of their strange ancestry lash out with devastating power. In these states, bloodragers can cast some arcane spells instinctively.The bloodrager’s magic is as fast, violent, and seemingly unstoppable as their physical prowess.\n"
                                                                         + "Role: Masters of the battlefield, bloodragers unleash fearful carnage on their enemies using their bloodlines and combat prowess.The bloodrager’s place is on the front lines, right in his enemies’ faces, supplying tremendous martial force bolstered by a trace of arcane magic."
                                                                         );
            bloodrager_class.m_Icon = barbarian_class.Icon;
            bloodrager_class.SkillPoints = barbarian_class.SkillPoints;
            bloodrager_class.HitDie = DiceType.D10;
            bloodrager_class.BaseAttackBonus = barbarian_class.BaseAttackBonus;
            bloodrager_class.FortitudeSave = barbarian_class.FortitudeSave;
            bloodrager_class.ReflexSave = barbarian_class.ReflexSave;
            bloodrager_class.WillSave = barbarian_class.WillSave;
            bloodrager_class.Spellbook = createBloodragerSpellbook();
            bloodrager_class.ClassSkills = new StatType[] {StatType.SkillKnowledgeArcana, StatType.SkillMobility, StatType.SkillLoreNature, StatType.SkillPerception, StatType.SkillAthletics,
                                                      StatType.SkillPersuasion};
            bloodrager_class.IsDivineCaster = false;
            bloodrager_class.IsArcaneCaster = true;
            bloodrager_class.StartingGold = barbarian_class.StartingGold;
            bloodrager_class.PrimaryColor = barbarian_class.PrimaryColor;
            bloodrager_class.SecondaryColor = barbarian_class.SecondaryColor;
            bloodrager_class.RecommendedAttributes = new StatType[] { StatType.Strength, StatType.Charisma, StatType.Constitution };
            bloodrager_class.NotRecommendedAttributes = new StatType[] { StatType.Intelligence };
            bloodrager_class.EquipmentEntities = barbarian_class.EquipmentEntities;
            bloodrager_class.MaleEquipmentEntities = barbarian_class.MaleEquipmentEntities;
            bloodrager_class.FemaleEquipmentEntities = barbarian_class.FemaleEquipmentEntities;
            bloodrager_class.ComponentsArray = new BlueprintComponent[] { barbarian_class.ComponentsArray[0] }; //no animal class, probably should be no sorcerer or eldrich scion  or dragon disciple
            bloodrager_class.StartingItems = barbarian_class.StartingItems;
            createBloodragerProgression();
            bloodrager_class.Progression = bloodrager_progression;

            createEldritchScionMysticalFocus();
            createMetarager();
            createSpellEater();
            createSteelblood();
            createUrbanBloodrager();
            createBloodConduit();
            bloodrager_class.Archetypes = new BlueprintArchetype[] { metamagic_rager_archetype, spelleater_archetype, steelblood_archetype, urban_bloodrager, blood_conduit }; //steelblood, spell eater, metamagic rager
            Helpers.RegisterClass(bloodrager_class);
            createRageCastingFeat();
            addToPrestigeClasses();
        }


        static void createEldritchScionMysticalFocus()
        {
            var eldritch_scion = library.Get<BlueprintArchetype>("d078b2ef073f2814c9e338a789d97b73");
            eldritch_scion_bloodrager = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "EldritchScionBloodragerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Eldritch Scion (Bloodrager)");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", eldritch_scion.Description);
            });
            Helpers.SetField(eldritch_scion_bloodrager, "m_ParentClass", eldritch_scion.GetParentClass());
            library.AddAsset(eldritch_scion_bloodrager, "");
            eldritch_scion_bloodrager.GetParentClass().Archetypes = eldritch_scion_bloodrager.GetParentClass().Archetypes.AddToArray(eldritch_scion_bloodrager);

            var rage_buff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");

            var add_fact_context_actions_rage = rage_buff.GetComponent<AddFactContextActions>();
            var add_fact_context_actions_bloodrage = bloodrage_buff.GetComponent<AddFactContextActions>();

            eldritch_scion_buff = Helpers.CreateBuff("EldritchScionMysticalFocusBuff",
                                                       "Mystical Focus",
                                                       "As a swift action, an eldritch scion can spend a point of eldritch energy to enter a state of mystical focus for 4 rounds. This allows him to use abilities from his bloodrager bloodline as though he were in a bloodrage, though he gains none of the other benefits or drawbacks of bloodraging.",
                                                       "",
                                                       bloodrage_buff.Icon,
                                                       bloodrage_buff.FxOnStart,
                                                       Helpers.CreateAddFactContextActions(activated: add_fact_context_actions_bloodrage.Activated.Actions.Except(add_fact_context_actions_rage.Activated.Actions).ToArray(),
                                                                                           deactivated: add_fact_context_actions_bloodrage.Deactivated.Actions.Except(add_fact_context_actions_rage.Deactivated.Actions).ToArray()
                                                                                           )
                                                       );
            bloodrage_buff.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = eldritch_scion_buff));

            
            ClassToProgression.addClassToFact(eldritch_scion_bloodrager.GetParentClass(), new BlueprintArchetype[] { eldritch_scion_bloodrager }, ClassToProgression.DomainSpellsType.NoSpells, eldritch_scion_buff, bloodrager_class);
            ClassToProgression.addClassToDomains(eldritch_scion_bloodrager.GetParentClass(), new BlueprintArchetype[] { eldritch_scion_bloodrager }, ClassToProgression.DomainSpellsType.NormalList, bloodline_selection, bloodrager_class);

            var eldritch_resource = library.Get<BlueprintAbilityResource>("17b6158d363e4844fa073483eb2655f8");

            var mystical_focus_ability = Helpers.CreateAbility("MysticalFocusAbility",
                                                       eldritch_scion_buff.Name,
                                                       eldritch_scion_buff.Description,
                                                       "",
                                                       eldritch_scion_buff.Icon,
                                                       AbilityType.Supernatural,
                                                       CommandType.Swift,
                                                       AbilityRange.Personal,
                                                       "4 rounds",
                                                       "",
                                                       Helpers.CreateRunActions(Common.createContextActionApplyBuff(eldritch_scion_buff, Helpers.CreateContextDuration(4, DurationRate.Rounds), dispellable: false)),
                                                       eldritch_resource.CreateResourceLogic()
                                                       );
            mystical_focus_ability.setMiscAbilityParametersSelfOnly();

            mystical_focus = Common.AbilityToFeature(mystical_focus_ability, false);

            var eldritch_scion_bloodrager_bloodlines = library.CopyAndAdd(bloodline_selection, "EldritchScionBloodlineSelection", "");
            eldritch_scion_bloodrager_bloodlines.SetDescription("An eldritch scion gains a bloodrager bloodline.The bloodline is selected at 1st level, and this choice cannot be changed. An eldritch scion’s effective bloodrager level for his bloodline abilities is equal to his eldritch scion level. He does not gain any bonus feats and he gains bonus spells from his bloodline 3 levels earlier than a bloodrager would. To use any ability that normally functions when in a bloodrage, an eldritch scion must spend a point from his eldritch pool (see mystical focus ability).");

            for (int i = 0; i < eldritch_scion_bloodrager_bloodlines.AllFeatures.Length; i++)
            {
                var f = library.CopyAndAdd(eldritch_scion_bloodrager_bloodlines.AllFeatures[i] as BlueprintProgression, "EldritchScion" + eldritch_scion_bloodrager_bloodlines.AllFeatures[i].name, "");

                f.ReplaceComponent<PrerequisiteClassLevel>(Common.createPrerequisiteArchetypeLevel(eldritch_scion_bloodrager, 1));
                eldritch_scion_bloodrager_bloodlines.AllFeatures[i].AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(a => a.replacement_feature = f));
                var powers = new BlueprintFeatureBase[]
                {
                    f.LevelEntries[0].Features[0],
                    f.LevelEntries[1].Features[0],
                    f.LevelEntries[3].Features[0],
                    f.LevelEntries[5].Features[0],
                    f.LevelEntries[7].Features[0],
                    f.LevelEntries[8].Features[0],
                };
                var spells = new BlueprintFeatureBase[]
                {
                    f.LevelEntries[2].Features[0],
                    f.LevelEntries[4].Features[0],
                    f.LevelEntries[6].Features[0],
                    f.LevelEntries[7].Features[1],
                };
                f.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, powers[0]),
                                                         Helpers.LevelEntry(4, powers[1], spells[0]),
                                                         Helpers.LevelEntry(7, spells[1]),
                                                         Helpers.LevelEntry(8, powers[2]),
                                                         Helpers.LevelEntry(10, spells[2]),
                                                         Helpers.LevelEntry(12, powers[3]),
                                                         Helpers.LevelEntry(13, spells[3]),
                                                         Helpers.LevelEntry(16, powers[4]),
                                                         Helpers.LevelEntry(20, powers[5])
                                                  };

                bloodrager_eldritch_scion_bloodlines_map.Add(eldritch_scion_bloodrager_bloodlines.AllFeatures[i] as BlueprintProgression, f);
                eldritch_scion_bloodrager_bloodlines.AllFeatures[i] = f;
                eldritch_scion_bloodrager_bloodlines.Features[i] = f;

            }

            bloodline_selection.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = eldritch_scion_bloodrager_bloodlines));

            eldritch_scion_bloodrager.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, eldritch_scion_bloodrager_bloodlines, mystical_focus, eldritch_scion.AddFeatures[0].Features[1], eldritch_scion.AddFeatures[0].Features[2]) };
            eldritch_scion_bloodrager.AddFeatures = eldritch_scion_bloodrager.AddFeatures.AddToArray(eldritch_scion.AddFeatures.Skip(1));
            //eldritch_scion_bloodrager.AddFeatures = eldritch_scion_bloodrager.AddFeatures.RemoveFromArray(eldritch_scion_bloodrager.AddFeatures.Where(le => le.Level == 19).FirstOrDefault());
            eldritch_scion_bloodrager.RemoveFeatures = eldritch_scion.RemoveFeatures;

            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");

            eldritch_scion_bloodrager.ReplaceSpellbook = library.CopyAndAdd(eldritch_scion.ReplaceSpellbook, "EldritchScionBloodragerSpellbook", "");
            //eldritch_scion_bloodrager.ReplaceSpellbook.RemoveComponents<AddCustomSpells>();
            eldritch_scion_bloodrager.OverrideAttributeRecommendations = true;
            eldritch_scion_bloodrager.RecommendedAttributes = eldritch_scion.RecommendedAttributes;
            eldritch_scion_bloodrager.NotRecommendedAttributes = eldritch_scion.NotRecommendedAttributes;
            //add check against new archetype for exisitng magus spellbooks
            var selections_to_fix = new BlueprintFeatureSelection[] {Common.EldritchKnightSpellbookSelection,
                                                                     Common.ArcaneTricksterSelection,
                                                                     Common.MysticTheurgeArcaneSpellbookSelection,
                                                                     Common.DragonDiscipleSpellbookSelection,
                                                                    };
            foreach (var s in selections_to_fix)
            {
                foreach (var f in s.AllFeatures)
                {
                    if (f.GetComponents<PrerequisiteClassSpellLevel>().Where(c => c.CharacterClass == eldritch_scion_bloodrager.GetParentClass()).Count() > 0)
                    {
                        f.AddComponent(Common.prerequisiteNoArchetype(eldritch_scion_bloodrager));
                    }
                }
            }

           
            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, eldritch_scion_bloodrager.ReplaceSpellbook, "EldritchKnightEldritchScionBloodrager",
                                       Common.createPrerequisiteClassSpellLevel(eldritch_scion_bloodrager.GetParentClass(), 3),
                                       Common.createPrerequisiteArchetypeLevel(eldritch_scion_bloodrager, 1));

            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, eldritch_scion_bloodrager.ReplaceSpellbook, "ArcaneTricksterEldritchScionBloodrager",
                                      Common.createPrerequisiteClassSpellLevel(eldritch_scion_bloodrager.GetParentClass(), 2),
                                       Common.createPrerequisiteArchetypeLevel(eldritch_scion_bloodrager, 1));

            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, eldritch_scion_bloodrager.ReplaceSpellbook, "MysticTheurgeEldritchScionBloodrager",
                                       Common.createPrerequisiteClassSpellLevel(eldritch_scion_bloodrager.GetParentClass(), 2),
                                       Common.createPrerequisiteArchetypeLevel(eldritch_scion_bloodrager, 1));

            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, eldritch_scion_bloodrager.ReplaceSpellbook, "DragonDiscipleEldritchScionBloodrager",
                                       Common.createPrerequisiteClassSpellLevel(eldritch_scion_bloodrager.GetParentClass(), 1),
                                       Common.createPrerequisiteArchetypeLevel(eldritch_scion_bloodrager, 1));

            var dd_breath = library.Get<BlueprintFeature>("0aadb51129cb0c147b5d2464c0db10b3");
            ClassToProgression.addClassToFeat(eldritch_scion_bloodrager.GetParentClass(), new BlueprintArchetype[] { eldritch_scion_bloodrager }, ClassToProgression.DomainSpellsType.NoSpells, dd_breath, bloodrager_class);
        }


        static void addToPrestigeClasses()
        {
            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, bloodrager_class.Spellbook, "EldritchKnightBloodrager",
                                       Common.createPrerequisiteClassSpellLevel(bloodrager_class, 3));
            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, bloodrager_class.Spellbook, "ArcaneTricksterBloodrager",
                           Common.createPrerequisiteClassSpellLevel(bloodrager_class, 2));
            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, bloodrager_class.Spellbook, "MysticTheurgeBloodrager",
                           Common.createPrerequisiteClassSpellLevel(bloodrager_class, 2));
            Common.addReplaceSpellbook(Common.DragonDiscipleSpellbookSelection, bloodrager_class.Spellbook, "DragonDiscipleBloodrager",
                                      Common.createPrerequisiteClassSpellLevel(bloodrager_class, 1));
        }


        static BlueprintCharacterClass[] getBloodragerArray()
        {
            return new BlueprintCharacterClass[] { bloodrager_class };
        }


        static BlueprintCharacterClass[] getDraconicArray()
        {
            return new BlueprintCharacterClass[] { bloodrager_class, library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c") };//+dragon disciple
        }


        static void removeUnauthorizedbloodlinesFromDragonDisciple()
        {
            var dragon_disciple = library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
            var non_dragon_bloodline = Helpers.CreateFeature("BloodragerNonDragonBloodline",
                                                             "Bloodrager Non-Draconic Bloodline",
                                                             "",
                                                             "",
                                                             null,
                                                             FeatureGroup.None);
            non_dragon_bloodline.HideInUI = true;
            non_dragon_bloodline.HideInCharacterSheetAndLevelUp = true;

            var allowed_features = dragon_disciple.GetComponent<PrerequisiteFeaturesFromList>();
            foreach (var b in bloodlines.Values)
            {
                if (!allowed_features.Features.Contains(b.progression))
                {
                    b.progression.LevelEntries[0].Features.Add(non_dragon_bloodline);
                }
            }
            dragon_disciple.AddComponent(Helpers.PrerequisiteNoFeature(non_dragon_bloodline));
            
        }

        static void createBloodragerProgression()
        {

            createBloodrage();
            createBloodlineFeatSelection();
            creatBloodlineSelection();
            removeUnauthorizedbloodlinesFromDragonDisciple();
            createBloodSanctuary();
            createBloodCasting();

            bloodrager_progression = Helpers.CreateProgression("BloodragerProgression",
                           bloodrager_class.Name,
                           bloodrager_class.Description,
                           "ca69bcd88a6a4fd685f6a1a9744ed518",
                           bloodrager_class.Icon,
                           FeatureGroup.None);
            bloodrager_progression.Classes = getBloodragerArray();

            bloodrager_proficiencies = library.CopyAndAdd<BlueprintFeature>("acc15a2d19f13864e8cce3ba133a1979", //barbarian proficiencies
                                                                            "BloodragerProficiencies",
                                                                            "207950c35a6a48bb895325d4dab02b75");
            bloodrager_proficiencies.AddComponent(Common.createArcaneArmorProficiency(Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Buckler,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.LightShield,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.HeavyShield,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.TowerShield,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Light,
                                                                                      Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Medium)
                                                                                      );
            bloodrager_proficiencies.SetName("Bloodrager Proficiencies");
            bloodrager_proficiencies.SetDescription("Bloodragers are proficient with all simple and martial weapons, light armor, medium armor, and shields (except tower shields). A bloodrager can cast bloodrager spells while wearing light armor or medium armor without incurring the normal arcane spell failure chance. This does not affect the arcane spell failure chance for arcane spells received from other classes. Like other arcane spellcasters, a bloodrager wearing heavy armor or wielding a shield incurs a chance of arcane spell failure if the spell in question has somatic components.");

            var detect_magic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");
            fast_movement = library.Get<BlueprintFeature>("d294a5dddd0120046aae7d4eb6cbc4fc");
            fast_movement.SetDescription("A character's land speed is faster than the norm for her race by +10 feet. This benefit applies only when she is wearing no armor, light armor, or medium armor, and not carrying a heavy load. Apply this bonus before modifying the character's speed because of any load carried or armor worn. This bonus stacks with any other bonuses to the character's land speed.");
            uncanny_dodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            improved_uncanny_dodge = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");

            indomitable_will = library.Get<BlueprintFeature>("e9ae7276574c170468937b617d993357");


            bloodrager_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, bloodrager_proficiencies, bloodrage, bloodline_selection, fast_movement, detect_magic,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")),  // touch calculate feature};
                                                                    Helpers.LevelEntry(2, uncanny_dodge),
                                                                    Helpers.LevelEntry(3, blood_sanctuary),
                                                                    Helpers.LevelEntry(4),
                                                                    Helpers.LevelEntry(5, improved_uncanny_dodge),
                                                                    Helpers.LevelEntry(6, bloodline_feat_selection),
                                                                    Helpers.LevelEntry(7, damage_reduction),
                                                                    Helpers.LevelEntry(8),
                                                                    Helpers.LevelEntry(9,  bloodline_feat_selection),
                                                                    Helpers.LevelEntry(10, damage_reduction),
                                                                    Helpers.LevelEntry(11, greater_bloodrage, blood_casting),
                                                                    Helpers.LevelEntry(12, bloodline_feat_selection),
                                                                    Helpers.LevelEntry(13, damage_reduction),
                                                                    Helpers.LevelEntry(14, indomitable_will),
                                                                    Helpers.LevelEntry(15, bloodline_feat_selection),
                                                                    Helpers.LevelEntry(16, damage_reduction),
                                                                    Helpers.LevelEntry(17, tireless_bloodrage),
                                                                    Helpers.LevelEntry(18, bloodline_feat_selection),
                                                                    Helpers.LevelEntry(19, damage_reduction),
                                                                    Helpers.LevelEntry(20, mighty_bloodrage)
                                                                    };

            bloodrager_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { bloodrager_proficiencies, detect_magic, bloodline_selection };
            bloodrager_progression.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(bloodrage, greater_bloodrage, tireless_bloodrage, mighty_bloodrage),
                                                              Helpers.CreateUIGroup(fast_movement, uncanny_dodge, blood_sanctuary, improved_uncanny_dodge, blood_casting, indomitable_will)};
        }


        static void createBloodlineFeatSelection()
        {
            bloodline_feat_selection = Helpers.CreateFeatureSelection("BloodragerBloodlineFeat",
                                                                      "Bloodline Feat",
                                                                      "At 6th level and every 3 levels thereafter, a bloodrager receives one bonus feat chosen from a list specific to each bloodline. The bloodrager must meet the prerequisites for these bonus feats.",
                                                                      "",
                                                                      null,
                                                                      FeatureGroup.None
                                                                      );
        }


        static void createBloodCasting()
        {
            var icon = library.Get<BlueprintAbility>("92681f181b507b34ea87018e8f7a528a").Icon;
            var blood_casting_allowed_buff = Helpers.CreateBuff("BloodCastingAllowedBuff",
                                                                "",
                                                                "",
                                                                "",
                                                                null,
                                                                null);
            blood_casting_allowed_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_allow_blood_casting = Common.createContextActionApplyBuff(blood_casting_allowed_buff, Helpers.CreateContextDuration(1), is_child: true, dispellable: false);
            var add_fact_context_actions = bloodrage_buff.GetComponent<AddFactContextActions>();
            add_fact_context_actions.Activated = Helpers.CreateActionList(add_fact_context_actions.Activated.Actions.AddToArray(apply_allow_blood_casting));

            var cast_only_on_self = Common.createContextActionApplyBuff(SharedSpells.can_only_target_self_buff, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true);
            var blood_casting_buff = Helpers.CreateBuff("BloodCastingBuff",
                                                        "Blood Casting",
                                                        "The bloodrager can apply the effects a bloodrager spell he knows of 2nd level or lower to himself the round he enters bloodrage. The spell must have a range of touch or personal. This use consumes a bloodrager spell slot, as if he had cast the spell; he must have the spell slot available to take advantage of this effect.\n"
                                                        + "At level 20, the spell he can apply to himself at the beginning of a bloodrage is no longer limited to only spells of 2nd level or lower.",
                                                        "",
                                                        icon,
                                                        null,
                                                        Helpers.Create<TurnActionMechanics.FreeTouchOrPersonalSpellUseFromSpellbook>(f =>
                                                                                                                                    {
                                                                                                                                        f.allowed_spellbook = bloodrager_class.Spellbook;
                                                                                                                                        f.max_spell_level = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                                                        f.control_buff = blood_casting_allowed_buff;
                                                                                                                                    }
                                                                                                                                    ),
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                        classes: getBloodragerArray(),
                                                                                        progression: ContextRankProgression.Custom,
                                                                                        customProgression: new (int, int)[] {(19, 2), (20, 4)}
                                                                                        ),
                                                        Helpers.CreateAddFactContextActions(cast_only_on_self)
                                                        );

            var blood_casting_ability = Helpers.CreateActivatableAbility("BloodCastingToggleAbility",
                                                                         blood_casting_buff.Name,
                                                                         blood_casting_buff.Description,
                                                                         "",
                                                                         blood_casting_buff.Icon,
                                                                         blood_casting_buff,
                                                                         AbilityActivationType.Immediately,
                                                                         CommandType.Free,
                                                                         null,
                                                                         Helpers.Create<RestrictionHasFact>(r => r.Feature = blood_casting_allowed_buff));
            blood_casting_ability.DeactivateImmediately = true;

            blood_casting = Common.ActivatableAbilityToFeature(blood_casting_ability, false);


            //fix previous saves without bloodcasting
            Action<UnitDescriptor> save_game_fix = delegate (UnitDescriptor unit)
            {
                if (unit.Progression.GetClassLevel(bloodrager_class) >= 11 && !unit.Progression.Features.HasFact(blood_casting))
                {
                    unit.Progression.Features.AddFeature(blood_casting);
                }
            };
            SaveGameFix.save_game_actions.Add(save_game_fix);
        }


        static void createBloodSanctuary()
        {
            var fact = Helpers.Create<NewMechanics.SavingThrowBonusAgainstAllies>();
            fact.Descriptor = ModifierDescriptor.UntypedStackable;
            fact.Value = 2;
            var spell_resistance = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889");
            blood_sanctuary = Helpers.CreateFeature("BloodragerBloodSanctury",
                                                     "Blood Sanctuary",
                                                     "At 3rd level, due to the power of his blood, a bloodrager can stand confidently amid the effects of spells cast by himself or his allies. He gains a +2 bonus on saving throws against spells that he or an ally casts.",
                                                     "",
                                                     spell_resistance.Icon,
                                                     FeatureGroup.None,
                                                     fact);
        }


        static void createBloodrage()
        {
            //normally bloodrage should allow to use rage powers if you have any since it is treated as normal rage
            bloodrage_ability = library.CopyAndAdd<BlueprintActivatableAbility>("df6a2cce8e3a9bd4592fb1968b83f730", "BloodrageToggleAbility", "");
            bloodrage_ability.SetNameDescription("Bloodrage",
                                            "The bloodrager’s source of internal power grants him the ability to bloodrage.\n"
                                            + "At 1st level, a bloodrager can rage for a number of rounds per day equal to 4 + her Constitution modifier. For each level after 1st she possesses, the bloodrager can rage for 2 additional rounds per day. Temporary increases to Constitution, such as that gained from bear's endurance, do not increase the total number of rounds that a bloodrager can rage per day. A bloodrager can enter a rage as a free action. The total number of rounds of rage per day is renewed after resting for 8 hours, although these hours need not be consecutive.\nWhile in a bloodrage, a bloodrager gains a +2 bonus on melee attack rolls, melee damage rolls, thrown weapon damage rolls, and Will saving throws. In addition, she takes a –2 penalty to Armor Class. She also gains 2 temporary hit points per Hit Die. These temporary hit points are lost first when a character takes damage, disappear when the rage ends, and are not replenished if the barbarian enters a rage again within 1 minute of her previous rage. While in a rage, a bloodrager can cast spells.\nA bloodrager can end her bloodrage as a free action, and is fatigued for 1 minute after a bloodrage ends. A bloodrager can't enter a new bloodrage while fatigued or exhausted, but can otherwise enter a rage multiple times per day. If a bloodrager falls unconscious, her rage immediately ends.\n"
                                            + "Bloodrage counts as the barbarian’s rage class feature for the purpose of feat prerequisites, feat abilities, magic item abilities, and spell effects."
                                            );
            bloodrage_ability.Group = ActivatableAbilityGroupExtension.Rage.ToActivatableAbilityGroup();
            bloodrage_buff = library.CopyAndAdd<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613", "BloodrageBuff", "");
            bloodrage_buff.RemoveComponent(bloodrage_buff.GetComponent<Kingmaker.UnitLogic.FactLogic.ForbidSpellCasting>());
            bloodrage_buff.AddComponent(Common.createForbidSpellCastingUnlessHasClass(false, getBloodragerArray()));
            bloodrage_buff.SetNameDescription(bloodrage_ability);
            bloodrage_ability.Buff.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = bloodrage_buff));
            bloodrage_ability.Buff = bloodrage_buff;
            bloodrage_buff.ReplaceComponent<AddFactContextActions>(a =>
            {
                a.Activated = Helpers.CreateActionList(a.Activated.Actions.ToList().ToArray());
                a.NewRound = Helpers.CreateActionList(a.NewRound.Actions.ToList().ToArray());
                a.Deactivated = Helpers.CreateActionList(a.Deactivated.Actions.ToList().ToArray());
            });

            var rage_feature = library.Get<BlueprintFeature>("2479395977cfeeb46b482bc3385f4647");
            bloodrage = library.CopyAndAdd<BlueprintFeature>(rage_feature, "BloodrageFeature", "");//barbarian rage feature
            bloodrage.SetNameDescription(bloodrage_ability);
            bloodrage.ReplaceComponent<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { a.Facts[1], bloodrage_ability });//keep standard rage resource

            greater_bloodrage = library.CopyAndAdd<BlueprintFeature>("ce49c579fe0bcc647a32c96929fae982", "GreaterBloodrageFeature", "");
            greater_bloodrage.SetNameDescription("Greater Bloodrage",
                                                  "At 11th level, a bloodrager's bonus on melee attack rolls, melee damage rolls, thrown weapon damage rolls, and Will saves while raging increases to +3. In addition, the amount of temporary hit points gained when entering a bloodrage increases to 3 per Hit Die.");
            tireless_bloodrage = library.CopyAndAdd<BlueprintFeature>("ca9343d75a83a2745a22fa11c383153a", "TirelessBloodrageFeature", "");
            tireless_bloodrage.SetNameDescription("Tireless Bloodrage", "At 17th level, a bloodrager no longer becomes fatigued at the end of his bloodrage.");
            library.Get<BlueprintFeature>("ca9343d75a83a2745a22fa11c383153a").AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = tireless_bloodrage));
            mighty_bloodrage = library.CopyAndAdd<BlueprintFeature>("06a7e5b60020ad947aed107d82d1f897", "MightyBloodrageFeature", "");
            mighty_bloodrage.SetNameDescription("Mighty Bloodrage",
                                                "At 20th level, a bloodrager's bonus on melee attack rolls, melee damage rolls, thrown weapon damage rolls, and Will saves while raging increases to +4. In addition, the amount of temporary hit points gained when entering a bloodrage increases to 4 per Hit Die."
                                                );
            //fix config for greater/tirelss rage
            var context_rank_config = bloodrage_buff.GetComponents<ContextRankConfig>().Where(a => a.IsFeatureList).FirstOrDefault();
            var features = Helpers.GetField<BlueprintFeature[]>(context_rank_config, "m_FeatureList");
            Helpers.SetField(context_rank_config, "m_FeatureList", features.AddToArray(mighty_bloodrage, greater_bloodrage));


            //fix rage resource to work for bloodrager
            var rage_resource = library.Get<Kingmaker.Blueprints.BlueprintAbilityResource>("24353fcf8096ea54684a72bf58dedbc9");
            ClassToProgression.addClassToResource(bloodrager_class, new BlueprintArchetype[0], rage_resource, library.Get<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853"));

            var extra_rage = library.Get<BlueprintFeature>("1a54bbbafab728348a015cf9ffcf50a7");
            extra_rage.ReplaceComponent<PrerequisiteFeature>(p => p.Group = Prerequisite.GroupType.Any);
            extra_rage.AddComponent(Helpers.PrerequisiteFeature(bloodrage, any: true));

            if (test_mode)
            {
                // allow to use rage out of combat for debug purposes
                var rage = library.Get<BlueprintActivatableAbility>("df6a2cce8e3a9bd4592fb1968b83f730");
                rage.IsOnByDefault = false;
                rage.DeactivateIfCombatEnded = false;
                bloodrage_ability.IsOnByDefault = false;
                bloodrage_ability.DeactivateIfCombatEnded = false;
                // allow to use charge on allies
                var charge = library.Get<BlueprintAbility>("c78506dd0e14f7c45a599990e4e65038");
                charge.CanTargetFriends = true;
            }

            //we will use damage reduction of barbarian
            damage_reduction = library.Get<BlueprintFeature>("cffb5cddefab30140ac133699d52a8f8");
            bloodrage_resource = rage_resource;
        }



        static BlueprintSpellbook createBloodragerSpellbook()
        {
            var bloodrager_spellbook = Helpers.Create<BlueprintSpellbook>();
            bloodrager_spellbook.name = "BloodragerSpellbook";
            library.AddAsset(bloodrager_spellbook, "e3a80ac9beb441af8a7285eaf99a3a8b");
            bloodrager_spellbook.Name = bloodrager_class.LocalizedName;
            bloodrager_spellbook.SpellsPerDay = Common.createSpontaneousHalfCasterSpellsPerDay("BloodragerSpellsPerDayTable", "48e4349ce7dd4d399a9ebaf5aed21372");

            bloodrager_spellbook.SpellsKnown = Common.createSpontaneousHalfCasterSpellsKnown("BloodragerSpellsKnown", "f0110eac4ecc4da08d28fa73ed514e59");
            bloodrager_spellbook.Spontaneous = true;
            bloodrager_spellbook.IsArcane = true;
            bloodrager_spellbook.AllSpellsKnown = false;
            bloodrager_spellbook.CanCopyScrolls = false;
            bloodrager_spellbook.CastingAttribute = StatType.Charisma;
            bloodrager_spellbook.CharacterClass = bloodrager_class;
            bloodrager_spellbook.CasterLevelModifier = 0;
            bloodrager_spellbook.CantripsType = CantripsType.Cantrips;
            bloodrager_spellbook.SpellsPerLevel = 0;

            bloodrager_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            bloodrager_spellbook.SpellList.name = "BloodragerSpellList";
            library.AddAsset(bloodrager_spellbook.SpellList, "e93c0b4113f2498f8f206b3fe02f7964");
            bloodrager_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < bloodrager_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                bloodrager_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);

            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( NewSpells.blade_lash.AssetGuid, 1),
                new Common.SpellId( "4783c3709a74a794dbe7c8e7e0b1b038", 1), //burning hands
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( NewSpells.chill_touch.AssetGuid, 1),
                new Common.SpellId( "91da41b9793a4624797921f221db653c", 1), //color sparay
                new Common.SpellId( "95810d2829895724f950c8c4086056e7", 1), //corrosive touch
                new Common.SpellId( "8e7cfa5f213a90549aadd18f8f6f4664", 1), //ear piercing scream
                new Common.SpellId( "c60969e7f264e6d4b84a1499fdcf9039", 1), //enlarge person
                new Common.SpellId( "b065231094a21d14dbf1c3832f776871", 1), //fire belly
                new Common.SpellId( "39a602aa80cc96f4597778b6d4d49c0a", 1), //flare burst
                new Common.SpellId( NewSpells.frost_bite.AssetGuid, 1),
                new Common.SpellId( NewSpells.long_arm.AssetGuid, 1),
                new Common.SpellId( "9e1ad5d6f87d19e4d8883d63a6e35568", 1), //mage armor
                new Common.SpellId( "4ac47ddb9fa1eaf43a1b6809980cfbd2", 1), //magic missile
                new Common.SpellId( NewSpells.magic_weapon.AssetGuid, 1),
                new Common.SpellId( "ef768022b0785eb43a18969903c537c4", 1), //mage shield
                new Common.SpellId( "433b1faf4d02cc34abb0ade5ceda47c4", 1), //protection from alignment
                new Common.SpellId( "450af0402422b0b4980d9c2175869612", 1), //ray of enfeeblement
                new Common.SpellId( "fa3078b9976a5b24caf92e20ee9c0f54", 1), //ray of sickening
                new Common.SpellId( "4e0e9aba6447d514f88eff1464cc4763", 1), //reduce person
                new Common.SpellId( "ab395d2335d3f384e99dddee8562978f", 1), //shocking grasp
                new Common.SpellId( "9f10909f0be1f5141bf1c102041f93d9", 1), //snowball
                new Common.SpellId( "85067a04a97416949b5d1dbf986d93f3", 1), //stone fist
                new Common.SpellId( NewSpells.touch_of_blood_letting.AssetGuid, 1),
                new Common.SpellId( "ad10bfec6d7ae8b47870e3a545cc8900", 1), //touch of gracelessness
                new Common.SpellId( "2c38da66e5a599347ac95b3294acbe00", 1), //true strike
                new Common.SpellId( NewSpells.warding_weapon.AssetGuid, 1),

                new Common.SpellId( "9a46dfd390f943647ab4395fc997936d", 2), //acid arrow
                new Common.SpellId( "a900628aea19aa74aad0ece0e65d091a", 2), //bear's endurance
                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 2), //blindness
                new Common.SpellId( "4c3d08935262b6544ae97599b3a9556d", 2), //bulls's strength
                new Common.SpellId( NewSpells.blood_armor.AssetGuid, 2),
                new Common.SpellId( NewSpells.bone_fists.AssetGuid, 2),
                new Common.SpellId( "de7a025d48ad5da4991e7d3c682cf69d", 2), //cats grace
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagles splendor
                new Common.SpellId( "7a5b5bf845779a941a67251539545762", 2), //false life
                new Common.SpellId( NewSpells.fiery_runes.AssetGuid, 2),
                new Common.SpellId( NewSpells.force_sword.AssetGuid, 2),
                new Common.SpellId( "b6010dda6333bcf4093ce20f0063cd41", 2), //frigid touch
                new Common.SpellId( NewSpells.ghoul_touch.AssetGuid, 2),
                new Common.SpellId( "ce7dad2b25acf85429b6c9550787b2d9", 2), //glitterdust
                new Common.SpellId( "42a65895ba0cb3a42b6019039dd2bff1", 2), //molten orb
                new Common.SpellId( "3e4ab69ada402d145a5e0ad3ad4b8564", 2), //mirror image
                new Common.SpellId( "c28de1f98a3f432448e52e5d47c73208", 2), //protection from arrows
                new Common.SpellId( "cdb106d53c65bbc4086183d54c3b97c7", 2), //scorching ray
                new Common.SpellId( "30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), //see invisibility
                new Common.SpellId( NewSpells.shadow_claws.AssetGuid, 2),
                new Common.SpellId( "5181c2ed0190fc34b8a1162783af5bf4", 2), //stone call
                new Common.SpellId( NewSpells.vine_strike.AssetGuid, 2),

                new Common.SpellId( NewSpells.allied_cloak.AssetGuid, 3),
                new Common.SpellId( "61a7ed778dd93f344a5dacdbad324cc9", 3), //beast shape 1
                new Common.SpellId( NewSpells.channel_vigor.AssetGuid, 3),
                new Common.SpellId( NewSpells.cloak_of_winds.AssetGuid, 3),
                new Common.SpellId( NewSpells.countless_eyes.AssetGuid, 3),
                new Common.SpellId( NewSpells.earth_tremor.AssetGuid, 3),
                new Common.SpellId( "2d81362af43aeac4387a3d4fced489c3", 3), //fireball
                new Common.SpellId( NewSpells.flame_arrow.AssetGuid, 3),
                new Common.SpellId( NewSpells.fly.AssetGuid, 3),
                new Common.SpellId( "486eaff58293f6441a5c2759c4872f98", 3), //haste
                new Common.SpellId( "5ab0d42fb68c9e34abae4921822b9d63", 3), //heroism
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 3), //hold person
                new Common.SpellId( NewSpells.howling_agony.AssetGuid, 3),
                new Common.SpellId( NewSpells.keen_edge.AssetGuid, 3),
                new Common.SpellId( "d2cff9243a7ee804cb6d5be47af30c73", 3), //lightning bolt
                new Common.SpellId( NewSpells.locate_weakness.AssetGuid, 3),
                new Common.SpellId( NewSpells.magic_weapon_greater.AssetGuid, 3),
                new Common.SpellId( NewSpells.pain_strike.AssetGuid, 3),
                new Common.SpellId( "d2f116cfe05fcdd4a94e80143b67046f", 3), //protection from energy
                new Common.SpellId( "97b991256e43bb140b263c326f690ce2", 3), //rage
                new Common.SpellId( NewSpells.ray_of_exhaustion.AssetGuid, 3),
                new Common.SpellId( NewSpells.resinous_skin.AssetGuid, 3),
                new Common.SpellId( NewSpells.sleet_storm.AssetGuid, 3),
                new Common.SpellId( "68a9e6d7256f1354289a39003a46d826", 3), //stinking cloud
                new Common.SpellId( SpiritualWeapons.twilight_knife.AssetGuid, 3),
                new Common.SpellId( Wildshape.undead_anatomyI.AssetGuid, 3),
                new Common.SpellId( "8a28a811ca5d20d49a863e832c31cce1", 3), //vampiric touch

                new Common.SpellId( "5d4028eb28a106d4691ed1b92bbb1915", 4), //beast shape 2
                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 4), //bestow curse
                new Common.SpellId( "cf6c901fb7acc904e85c63b342e9c949", 4), //confusion
                new Common.SpellId( "4baf4109145de4345861fe0f2209d903", 4), //crushing despair
                new Common.SpellId( "48e2744846ed04b4580be1a3343a5d3d", 4), //contagion
                new Common.SpellId( "f72f8f03bf0136c4180cd1d70eb773a5", 4), //controlled fireball
                new Common.SpellId( "5e826bcdfde7f82468776b55315b2403", 4), //dragon breath
                new Common.SpellId( "690c90a82bf2e58449c6b541cb8ea004", 4), //elemental body 1
                new Common.SpellId( "f34fb78eaaec141469079af124bcfa0f", 4), //enervation
                new Common.SpellId( "66dc49bf154863148bd217287079245e", 4), //enlarge person mass
                new Common.SpellId( "dc6af3b4fd149f841912d8a3ce0983de", 4), //false life, greater
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( "fcb028205a71ee64d98175ff39a0abf9", 4), //ice storm
                new Common.SpellId( "6717dbaef00c0eb4897a1c908a75dfe5", 4), //phantasmal killer
                new Common.SpellId( "2427f2e3ca22ae54ea7337bbab555b16", 4), //reduce person mass
                new Common.SpellId( NewSpells.river_of_wind.AssetGuid, 4), //river of wind
                new Common.SpellId( "f09453607e683784c8fca646eec49162", 4), //shout
                new Common.SpellId( "c66e86905f7606c4eaa5c774f0357b2b", 4), //stoneskin
                new Common.SpellId( "1e481e03d9cf1564bae6b4f63aed2d1a", 4), //touch of slime
                new Common.SpellId( "16ce660837fb2544e96c3b7eaad73c63", 4), //volcanic storm
                new Common.SpellId( NewSpells.fire_shield.AssetGuid, 4), //fire shield
                new Common.SpellId( NewSpells.wall_of_fire.AssetGuid, 4) //wall of fire
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(bloodrager_spellbook.SpellList, spell_id.level);
            }

            return bloodrager_spellbook;
        }


        static void creatBloodlineSelection()
        {
            AberrantBloodline.create();
            AbyssalBloodline.create();
            CelestialBloodline.create();
            FeyBloodline.create();
            InfernalBloodline.create();
            UndeadBloodline.create();
            DestinedBloodline.create();
            DraconicBloodlines.create();
            ElementalBloodlines.create();

            bloodline_selection = Helpers.CreateFeatureSelection("BloodragerBloodlineSelection",
                                                                     "Bloodline",
                                                                     "Each bloodrager has a source of magic somewhere in his heritage that empowers his bloodrages, bonus feats, and bonu spells. Sometimes this source reflects a distant blood relationship to a powerful being, or is due to an extreme event involving such a creature somewhere in his family’s past. Regardless of the source, this influence manifests in a number of ways. A bloodrager must pick one bloodline upon taking his first level of bloodrager. Once made, this choice cannot be changed.\n"
                                                                     + "When choosing a bloodline, the bloodrager’s alignment doesn’t restrict his choices.A good bloodrager could come from an abyssal bloodline, a celestial bloodline could beget an evil bloodrager generations later, a bloodrager from an infernal bloodline could be chaotic, and so on.Though his bloodline empowers him, it doesn’t dictate or limit his thoughts and behavior.\n"
                                                                     + "The bloodrager gains bloodline powers at 1st level, 4th level, and every 4 levels thereafter.The bloodline powers a bloodrager gains are described in his chosen bloodline.For all spell - like bloodline powers, treat the character’s bloodrager level as the caster level.\n"
                                                                     + "At 6th level and every 3 levels thereafter, a bloodrager receives one bonus feat chosen from a list specific to each bloodline.The bloodrager must meet the prerequisites for these bonus feats.At 7th, 10th, 13th, and 16th levels, a bloodrager learns an additional spell derived from his bloodline.\n"
                                                                     + "When a bloodrager enters a bloodrage, he often takes on a physical transformation influenced by his bloodline and powered by the magic that roils within him. Unless otherwise specified, he gains the effects of his bloodline powers only while in a bloodrage; once the bloodrage ends, all powers from his bloodline immediately cease, and any physical changes the bloodrager underwent revert, restoring him to normal.",
                                                                     "6eed80b1bfa9425e90c5981fb87dedf2",
                                                                     null,
                                                                     FeatureGroup.BloodLine);
            bloodline_selection.AllFeatures = new BlueprintFeature[] { AberrantBloodline.progression, AbyssalBloodline.progression, CelestialBloodline.progression, DestinedBloodline.progression,
                                                                       FeyBloodline.progression, InfernalBloodline.progression, UndeadBloodline.progression };
            bloodline_selection.AllFeatures = bloodline_selection.AllFeatures.AddToArray(DraconicBloodlines.progressions);
            bloodline_selection.AllFeatures = bloodline_selection.AllFeatures.AddToArray(ElementalBloodlines.progressions);
            bloodline_selection.Features = bloodline_selection.AllFeatures;
            bloodline_selection.AddComponent(Helpers.Create<NoSelectionIfAlreadyHasFeature>(n => { n.AnyFeatureFromSelection = true; n.Features = new BlueprintFeature[0]; }));
        }


        class AbyssalBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature claws;
            static internal BlueprintFeature demonic_bulk;
            static internal BlueprintFeature demonic_resistances;
            static internal BlueprintFeature abyssal_bloodrage;
            static internal BlueprintFeature demonic_aura;
            static internal BlueprintFeature demonic_immunities;

            static internal void create()
            {
                createClaws();
                createDemonicBulk();
                createDemonicResistances();
                createAbyssalBloodrage();
                createDemonicAura();
                createDemonicImmunities();

                var ray_of_enfeeblement = library.Get<BlueprintAbility>("450af0402422b0b4980d9c2175869612");
                var bulls_strength = library.Get<BlueprintAbility>("4c3d08935262b6544ae97599b3a9556d");
                var rage = library.Get<BlueprintAbility>("97b991256e43bb140b263c326f690ce2");
                var stoneskin = library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b");

                var cleave = library.Get<BlueprintFeature>("d809b6c4ff2aaff4fa70d712a70f7d7b");
                var great_fortitude = library.Get<BlueprintFeature>("79042cb55f030614ea29956177977c52");
                var improved_bull_rush = library.Get<BlueprintFeature>("b3614622866fe7046b787a548bbd7f59");
                var improved_sunder = library.Get<BlueprintFeature>("9719015edcbf142409592e2cbaab7fe1");
                var intimidating_prowess = library.Get<BlueprintFeature>("d76497bfc48516e45a0831628f767a0f");
                var power_attack = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
                var toughness = library.Get<BlueprintFeature>("d09b20029e9abfe4480b356c92095623");


                progression = createBloodragerBloodline("Abyssal",
                                                              "Generations ago, a demon spread its filth into the essence of your bloodline. While it doesn’t manifest in all of your kin, in those moments when you’re bloodraging, you embody its terrifying presence.\n"
                                                               + "Bonus Feats: Cleave, Great Fortitude, Improved Bull Rush, Improved Sunder, Intimidating Prowess, Power Attack, Toughness.\n"
                                                               + "Bonus Spells: Ray of enfeeblement(7th), bull’s strength(10th), rage(13th), stoneskin(16th).",
                                                              library.Get<BlueprintProgression>("d3a4cb7be97a6694290f0dcfbd147113").Icon, //sorceror bloodline
                                                              new BlueprintAbility[] { ray_of_enfeeblement, bulls_strength, rage, stoneskin },
                                                              new BlueprintFeature[] { cleave, great_fortitude, improved_bull_rush, improved_sunder, intimidating_prowess, power_attack, toughness },
                                                              new BlueprintFeature[] { claws, demonic_bulk, demonic_resistances, abyssal_bloodrage, demonic_aura, demonic_immunities },
                                                              "8a06875a5d1b4f3e93d4f7e1b54e3356",
                                                              new string[] { "1ebaa0951c4c43b69462fb6ccf0dde3f", "bbe03ed8293e435782dbfa6bc0ad10bf", "6e26e5974ab4481fa6127783bbac301c", "b7b57562edac4e01955dfa63e956b01b" },
                                                              "ccd945bba7784ae981ac66e8138439d5"
                                                              ).progression;

            }


            static void createClaws()
            {
                var claw1d6 = library.CopyAndAdd<BlueprintItemWeapon>("d40ae466ba750bf4495a174e399d85ce", "BloodragerBloodlineAbyssalClaw1d6", "37f8a9bd2fd04270a8d0d14916ed4c11"); //from sorc abbys bloodline
                var claw1d8 = library.CopyAndAdd<BlueprintItemWeapon>("d40ae466ba750bf4495a174e399d85ce", "BloodragerBloodlineAbyssalClaw1d8", "350d22105211463ebcb998e9740d00a1");
                Helpers.SetField(claw1d8, "m_DamageDice", new Kingmaker.RuleSystem.DiceFormula(1, DiceType.D8));
                var claw1d8Fire = library.CopyAndAdd<BlueprintItemWeapon>("6e2487c8fb0501841b508e5918b36cb9", "BloodragerBloodlineAbyssalClaw1d8Fire", "9f147636d23c4a04a2a575e6eb601bfa");
                Helpers.SetField(claw1d8Fire, "m_DamageDice", new Kingmaker.RuleSystem.DiceFormula(1, DiceType.D8));

                var claw_buff1 = library.CopyAndAdd<BlueprintBuff>("4a51dca9d9456214e9a382b9e47385b3", "BloodragerBloodlineAbyssalClaw1Buff", "6786313f39c044ad9348bdfc163cf45f");
                claw_buff1.ReplaceComponent<Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride>(Common.createEmptyHandWeaponOverride(claw1d6));
                var claw_buff2 = library.CopyAndAdd<BlueprintBuff>("cec6fcd5be2175f4e888f7c79ce68db6", "BloodragerBloodlineAbyssalClaw2Buff", "a4b808de88d44420aa0f2db0d64a1ce8"); //from sorcerer bloodline
                var claw_buff3 = library.CopyAndAdd<BlueprintBuff>("cec6fcd5be2175f4e888f7c79ce68db6", "BloodragerBloodlineAbyssalClaw3Buff", "9af5c8658f3f42c99cc61e58b1cae7ac");
                claw_buff3.ReplaceComponent<Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride>(Common.createEmptyHandWeaponOverride(claw1d8));
                var claw_buff4 = library.CopyAndAdd<BlueprintBuff>("cec6fcd5be2175f4e888f7c79ce68db6", "BloodragerBloodlineAbyssalClaw4Buff", "d255c953ad424b1f8e73e98b318e0d64");
                claw_buff4.ReplaceComponent<Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride>(Common.createEmptyHandWeaponOverride(claw1d8Fire));

                var claws1_feature = Helpers.CreateFeature("BloodragerBloodlineAbyssalClaws1Feature", "Claws",
                                                          "At 1st level, you grow claws while bloodraging. These claws are treated as natural weapons, allowing you to make two claw attacks as a full attack, using your full base attack bonus. These attacks deal 1d6 points of damage each (1d4 if you are Small) plus your Strength modifier. At 4th level, these claws are considered magic weapons for the purpose of overcoming damage resistance. At 8th level, the damage increases to 1d8 points (1d6 if you are Small). At 12th level, these claws become flaming weapons, which deal an additional 1d6 points of fire damage on a hit.",
                                                          "beb37be3879744b08b1def72cb4fb2d9",
                                                          claw1d6.Icon,
                                                          FeatureGroup.None);
                claws1_feature.HideInCharacterSheetAndLevelUp = true;
                var claws2_feature = Helpers.CreateFeature("BloodragerBloodlineAbyssalClaws2Feature", "Claws",
                                                          claws1_feature.Description,
                                                          "d5d7cfbbefc740d2b1047dd8ae4a9651",
                                                          claws1_feature.Icon,
                                                          FeatureGroup.None,
                                                          Common.createRemoveFeatureOnApply(claws1_feature));
                claws2_feature.HideInCharacterSheetAndLevelUp = true;
                var claws3_feature = Helpers.CreateFeature("BloodragerBloodlineAbyssalClaws3Feature", "Claws",
                                                          claws1_feature.Description,
                                                          "98cb5e2a4a7240dfa8f8de58136bc738",
                                                          claws1_feature.Icon,
                                                          FeatureGroup.None,
                                                          Common.createRemoveFeatureOnApply(claws2_feature),
                                                          Common.createRemoveFeatureOnApply(claws1_feature));
                claws3_feature.HideInCharacterSheetAndLevelUp = true;
                var claws4_feature = Helpers.CreateFeature("BloodragerBloodlineAbyssalClaws4Feature", "Claws",
                                                          claws1_feature.Description,
                                                          "e8d6d66c04bd495ba24824feaf537cf6",
                                                          claws1_feature.Icon,
                                                          FeatureGroup.None,
                                                          Common.createRemoveFeatureOnApply(claws3_feature),
                                                          Common.createRemoveFeatureOnApply(claws2_feature),
                                                          Common.createRemoveFeatureOnApply(claws1_feature));
                claws4_feature.HideInCharacterSheetAndLevelUp = true;
                claws = Helpers.CreateFeature("BloodragerBloodlineAbyssalClawsFeature", "Claws",
                                                          claws1_feature.Description,
                                                          "6897f6702f0144f4b468cd2ce6c22390",
                                                          claws1_feature.Icon,
                                                          FeatureGroup.None,
                                                          Helpers.CreateAddFeatureOnClassLevel(claws1_feature, 1, getBloodragerArray(), null),
                                                          Helpers.CreateAddFeatureOnClassLevel(claws2_feature, 4, getBloodragerArray(), null),
                                                          Helpers.CreateAddFeatureOnClassLevel(claws3_feature, 8, getBloodragerArray(), null),
                                                          Helpers.CreateAddFeatureOnClassLevel(claws4_feature, 12, getBloodragerArray(), null)
                                                          );

                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, claw_buff1, claws1_feature);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, claw_buff2, claws2_feature);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, claw_buff3, claws3_feature);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, claw_buff4, claws4_feature);
            }


            static void createDemonicBulk()
            {
                var reckless_stance = library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");
                var enlarge_buff = library.CopyAndAdd<BlueprintBuff>("4f139d125bb602f48bfaec3d3e1937cb", "BloodragerBloodlineAbyssalDemonicBulkSBuff", "b71ecf2aeb024508aa8d74ba96c9530b");
                var demonic_bulk_switch_buff = Helpers.CreateBuff("BloodragerBloodlineAbyssalDemonicBulkSwitchBuff",
                                                                  "Demonic Bulk",
                                                                  "At 4th level, when entering a bloodrage, you can choose to grow one size category larger than your base size (as enlarge person) even if you aren’t humanoid.",
                                                                  "00c460abf3e94f7fa7a69c6da5cb2741",
                                                                  enlarge_buff.Icon,
                                                                  null,
                                                                  Helpers.CreateEmptyAddFactContextActions());
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(demonic_bulk_switch_buff, enlarge_buff, bloodrage_buff);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, enlarge_buff, demonic_bulk_switch_buff);

                var demonic_bulk_ability = Helpers.CreateActivatableAbility("BloodragerBloodlineAbyssalDemonicBulkToggleAbility",
                                                                            demonic_bulk_switch_buff.Name,
                                                                            demonic_bulk_switch_buff.Description,
                                                                            "d6dd7b8642624f50997b2d580b8e18fa",
                                                                            enlarge_buff.Icon,
                                                                            demonic_bulk_switch_buff,
                                                                            AbilityActivationType.Immediately,
                                                                            CommandType.Free,
                                                                            reckless_stance.ActivateWithUnitAnimation);
                demonic_bulk = Helpers.CreateFeature("BloodragerBloodlineAbyssalDemonicBulkFeature",
                                                                        demonic_bulk_switch_buff.Name,
                                                                        demonic_bulk_switch_buff.Description,
                                                                        "4f2896b7dcda42dd89a44550381b7173",
                                                                        enlarge_buff.Icon,
                                                                        FeatureGroup.None,
                                                                        CallOfTheWild.Helpers.CreateAddFact(demonic_bulk_ability));
                enlarge_buff.SetBuffFlags(BuffFlags.HiddenInUi | enlarge_buff.GetBuffFlags());
            }


            static void createDemonicResistances()
            {
                var damage_resistance = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("8cbf303d479cf0d42a8e36092c76fa7c");
                var resist_caf = Helpers.CreateBuff("BloodragerAbyssalBloodlineDemonicResistancesBuff",
                                                    "Demon Resistances",
                                                    "At 8th level, you gain resistance 5 to acid, cold, and fire. At 16th level, these resistances increase to 10.",
                                                    "c888c8d801dc49ceac1cf022734afaaf",
                                                    damage_resistance.Icon,
                                                    null,
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Acid),
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Fire),
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Cold),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                    ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                    classes: getBloodragerArray(),
                                                    customProgression: new (int, int)[] {
                                                                (15, 5),
                                                                (20, 10)
                                                    })
                                                    );
                demonic_resistances = Helpers.CreateFeature("BloodragerAbyssalBloodlineDemonicResistancesFeature",
                                                                               resist_caf.Name,
                                                                               resist_caf.Description,
                                                                               "f5926c928da34a3ebaf0c9183742fb91",
                                                                               resist_caf.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, resist_caf, demonic_resistances);
            }


            static void createAbyssalBloodrage()
            {
                var abyssal_strength = library.Get<BlueprintFeature>("489c8c4a53a111d4094d239054b26e32");
                var buff = Helpers.CreateBuff("BloodragerAbyssalBloodlineAbyssalBloodrage",
                                                    "Abyssal Bloodrage",
                                                    "At 12th level, while bloodraging,  you receive +2 morale bonus to Strength, but the penalty to AC becomes –4 instead of –2. At 16th level, this bonus increases by 4 instead. At 20th level, it increases by 6 instead.",
                                                    "9f57b65210a34032aef5da55a7b7fa18",
                                                    abyssal_strength.Icon,
                                                    null,
                                                    Helpers.CreateAddStatBonus(StatType.AC, -2, ModifierDescriptor.UntypedStackable),
                                                    Helpers.CreateAddContextStatBonus(StatType.Strength, ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                    ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                    classes: getBloodragerArray(),
                                                    customProgression: new (int, int)[] {
                                                                (15, 2),
                                                                (19, 4),
                                                                (20, 6)
                                                    })
                                                    );
                abyssal_bloodrage = Helpers.CreateFeature("BloodragerAbyssalBloodlineAbyssalBloodrageFeature",
                                                                               buff.Name,
                                                                               buff.Description,
                                                                               "9167d9245c9140ae94f834186c98b700",
                                                                               buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, buff, abyssal_bloodrage);
            }


            static void createDemonicAura()
            {
                var area_effect = Helpers.Create<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect>();
                area_effect.name = "BloodragerAbyssalBloodlineDemonicAura";
                area_effect.AffectEnemies = true;
                area_effect.AggroEnemies = true;
                area_effect.Size = 5.Feet();
                area_effect.Shape = AreaEffectShape.Cylinder;
                var damage = Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D6), Common.createSimpleContextValue(2), Helpers.CreateContextValue(AbilityRankType.DamageBonus));
                var damage_action = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, damage, isAoE: true);
                var conditional_damage = Helpers.CreateConditional(Helpers.Create<Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionIsMainTarget>(),
                                                                    null,
                                                                    damage_action);
                area_effect.AddComponent(Helpers.CreateAreaEffectRunAction(round: conditional_damage));
                area_effect.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus,
                                                                         stat: StatType.Constitution,
                                                                         classes: getBloodragerArray(),
                                                                         type: AbilityRankType.DamageBonus
                                                                         )
                                        );
                area_effect.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Fire));
                area_effect.Fx = new Kingmaker.ResourceLinks.PrefabLink();
                library.AddAsset(area_effect, "2a03a03228fb479f9f0810e87cfbbe95");

                var reckless_stance = library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");
                var firebelly = library.Get<BlueprintAbility>("b065231094a21d14dbf1c3832f776871");

                var demonic_aura_buff = Helpers.CreateBuff("BloodragerBloodlineAbyssalDemonicAuraBuff",
                                                                              "Demonic Aura",
                                                                              $"At 16th level, when entering a bloodrage you can choose to exude an aura of fire. The aura is a 5-foot burst centered on you, and deals 2d{BalanceFixes.getDamageDieString(DiceType.D6)} + your Constitution modifier points of fire damage to creatures that end their turns within it.",
                                                                              "44d877ef2428424082761e94dd3d55b3",
                                                                              null,
                                                                              null,
                                                                              Common.createAddAreaEffect(area_effect));

                var demonic_aura_switch_buff = Helpers.CreateBuff("BloodragerBloodlineAbyssalDemonicAuraSwitchBuff",
                                                                  demonic_aura_buff.Name,
                                                                  demonic_aura_buff.Description,
                                                                  "5680b4fb4da04366a651e9da1b7943a1",
                                                                  firebelly.Icon,
                                                                  null,
                                                                  Helpers.CreateEmptyAddFactContextActions());

                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(demonic_aura_switch_buff, demonic_aura_buff, bloodrage_buff);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(bloodrage_buff, demonic_aura_buff, demonic_aura_switch_buff);

                var demonic_aura_ability = Helpers.CreateActivatableAbility("BloodragerBloodlineAbyssalDemonicAuraToggleAbility",
                                                                            demonic_aura_switch_buff.Name,
                                                                            demonic_aura_switch_buff.Description,
                                                                            "610e508618df41e8bb32aff78c326e9f",
                                                                            firebelly.Icon,
                                                                            demonic_aura_switch_buff,
                                                                            AbilityActivationType.Immediately,
                                                                            CommandType.Free,
                                                                            reckless_stance.ActivateWithUnitAnimation);
                demonic_aura = Helpers.CreateFeature("BloodragerBloodlineAbyssalDemonicAuraFeature",
                                                                        demonic_aura_switch_buff.Name,
                                                                        demonic_aura_switch_buff.Description,
                                                                        "b6df9a3984114c14881bf4b2d5aa5a47",
                                                                        firebelly.Icon,
                                                                        FeatureGroup.None,
                                                                        CallOfTheWild.Helpers.CreateAddFact(demonic_aura_ability));
                demonic_aura_buff.SetBuffFlags(BuffFlags.HiddenInUi | demonic_aura_buff.GetBuffFlags());
            }


            static void createDemonicImmunities()
            {
                demonic_immunities = library.CopyAndAdd<BlueprintFeature>("5c1c2ed7fe5f99649ab00605610b775b", //from sorceror bloodline
                                                                                            "BloodragerAbyssalBloodlineDemonicImmunitiesFeature",
                                                                                            "9f558f25202e451eb5b29c721a906a97");
                var resistances = demonic_immunities.GetComponents<Kingmaker.UnitLogic.FactLogic.AddDamageResistanceEnergy>().ToArray();

                foreach (var r in resistances)
                {
                    demonic_immunities.RemoveComponent(r);
                }
                demonic_immunities.SetName("Demonic Immunities");
                demonic_immunities.SetDescription("At 20th level, you’re immune to electricity and poison. You have this benefit constantly, even while not bloodraging.");
            }
        }


        class AberrantBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature staggering_strike;
            static internal BlueprintFeature abnormal_reach;
            static internal BlueprintFeature aberrant_fortitude;
            static internal BlueprintFeature unusual_anatomy;
            static internal BlueprintFeature aberrant_resistance;
            static internal BlueprintFeature aberrant_form;
            static string prefix = "BloodragerBloodlineAberrant";



            static internal void create()
            {
                createStaggeringStrike();
                createAbnormalReach();
                createAberrantFortitude();
                createUnusualAnatomy();
                createAberrantResistance();
                createAberrantForm();

                var enlarge_person = library.Get<BlueprintAbility>("c60969e7f264e6d4b84a1499fdcf9039");
                var see_invisibility = library.Get<BlueprintAbility>("30e5dc243f937fc4b95d2f8f4e1b7ff3");
                var displacement = library.Get<BlueprintAbility>("903092f6488f9ce45a80943923576ab3");
                var confusion = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949");

                var combat_reflexes = library.Get<BlueprintFeature>("0f8939ae6f220984e8fb568abbdfba95");
                var great_fortitude = library.Get<BlueprintFeature>("79042cb55f030614ea29956177977c52");
                var improved_disarm = library.Get<BlueprintFeature>("25bc9c439ac44fd44ac3b1e58890916f");
                var lightning_reflexes = library.Get<BlueprintFeature>("15e7da6645a7f3d41bdad7c8c4b9de1e");
                var improved_initiative = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
                var improved_unarmed_strike = library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167");
                var iron_will = library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334");


                progression = createBloodragerBloodline("Aberrant",
                                                              "There is a taint in your blood that is both alien and bizarre. When you bloodrage, this manifests in peculiar and terrifying ways.\n"
                                                               + "Bonus Feats: Combat Reflexes, Great Fortitude, Improved Disarm, Improved Initiative, Improved Unarmed Strike, Iron Will, Lightning Reflexes.\n"
                                                               + "Bonus Spells: Enlarge person (7th), see invisibility (10th), displacement (13th), confusion (16th).",
                                                              library.Get<BlueprintAbility>("14ec7a4e52e90fa47a4c8d63c69fd5c1").Icon, //blur
                                                              new BlueprintAbility[] { enlarge_person, see_invisibility, displacement, confusion },
                                                              new BlueprintFeature[] { combat_reflexes, great_fortitude, improved_disarm, improved_initiative, improved_unarmed_strike, iron_will, lightning_reflexes },
                                                              new BlueprintFeature[] { staggering_strike, abnormal_reach, aberrant_fortitude, unusual_anatomy, aberrant_resistance, aberrant_form },
                                                              "b42a51e28d06456bad786a0405c3a892",
                                                              new string[] { "81d09d66ac3e4faf8e58f570e1551621", "feeb29835ff34bb2a7f692d465372565", "e96cd140a81b480aa6b52ac8bdb50813", "bdf50388d3d545ae8ea5d443c21b3388" },
                                                              "cd00b25671db470182daa9c129949991"
                                                              ).progression;
            }

            static void createStaggeringStrike()
            {
                var blade_barrier = library.Get<BlueprintAbility>("36c8971e91f1745418cc3ffdfac17b74");
                var stagerred_buff = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");

                var effect_action = Helpers.CreateActionList(Common.createContextSavedApplyBuff(stagerred_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1))));
                var action = Helpers.CreateActionList(Common.createContextActionSavingThrow(SavingThrowType.Fortitude, effect_action));
                var staggering_strike_buff = Helpers.CreateBuff(prefix + "StaggeringStrikeBuff",
                                                                "Staggering Strike",
                                                                "At 1st level, when you confirm a critical hit the target must succeed at a Fortitude saving throw or be staggered for 1 round. The DC of this save is equal to 10 + 1/2 your bloodrager level + your Constitution modifier. These effects stack with the Staggering Critical feat; the target must save against each effect individually.",
                                                                "58fb04c603b14382b23f2c7600691277",
                                                                blade_barrier.Icon,
                                                                null,
                                                                Common.createAddInitiatorAttackRollTrigger2(action, critical_hit: true),
                                                                Common.createContextCalculateAbilityParamsBasedOnClass(bloodrager_class, StatType.Constitution)
                                                                );
                staggering_strike = Helpers.CreateFeature(prefix + "StaggeringStrikeFeature",
                                                               staggering_strike_buff.Name,
                                                               staggering_strike_buff.Description,
                                                               "a225ba5a080041b2b78f33b8a704e821",
                                                               staggering_strike_buff.Icon,
                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, staggering_strike_buff, staggering_strike);
            }


            static void createAbnormalReach()
            {
                var polymorph = library.Get<BlueprintAbility>("93d9d74dac46b9b458d4d2ea7f4b1911");
                var abnormal_reach_buff = Helpers.CreateBuff(prefix + "AbnormalReachBuff",
                                                             "Abnormal Reach",
                                                             "At 4th level, your limbs elongate; your reach increases by 5 feet.",
                                                             "a2594e07ae5b489aaca41c6375a6d847",
                                                             polymorph.Icon,
                                                             null,
                                                             Helpers.CreateAddStatBonus(StatType.Reach, 5, ModifierDescriptor.Enhancement)
                                                             );
                abnormal_reach = Helpers.CreateFeature(prefix + "AbnormalReachFeature",
                                               abnormal_reach_buff.Name,
                                               abnormal_reach_buff.Description,
                                               "19b74174db5a42c4b1d90ae9e54e8fe8",
                                               abnormal_reach_buff.Icon,
                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, abnormal_reach_buff, abnormal_reach);
            }


            static void createAberrantFortitude()
            {
                var defensive_stance = library.Get<BlueprintActivatableAbility>("be68c660b41bc9247bcab727b10d2cd1");
                var aberrant_fortitude_buff = Helpers.CreateBuff(prefix + "AberrantFortitudeBuff",
                                                                 "Aberrant Fortitude",
                                                                 "At 8th level, you become immune to the sickened and nauseated conditions.",
                                                                 "2f7c24016fa34388a3fa191609e75856",
                                                                 defensive_stance.Icon,
                                                                 null,
                                                                 Common.createAddConditionImmunity(UnitCondition.Nauseated),
                                                                 Common.createAddConditionImmunity(UnitCondition.Sickened),
                                                                 Common.createBuffDescriptorImmunity(SpellDescriptor.Sickened | SpellDescriptor.Nauseated)
                                                                 );
                aberrant_fortitude = Helpers.CreateFeature(prefix + "AberrantFortitudeFeature",
                                               aberrant_fortitude_buff.Name,
                                               aberrant_fortitude_buff.Description,
                                               "c3c215fb034e4b5b9f929f1d0c4e51a7",
                                               aberrant_fortitude_buff.Icon,
                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, aberrant_fortitude_buff, aberrant_fortitude);
            }


            static void createUnusualAnatomy()
            {
                var remove_disease = library.Get<BlueprintAbility>("4093d5a0eb5cae94e909eb1e0e1a6b36");
                var unusual_anatomy_buff = Helpers.CreateBuff(prefix + "UnusualAnatomyBuff",
                                                                 "Unusual Anatomy",
                                                                 "At 12th level, your internal anatomy shifts and changes, giving you a 50% chance to negate any critical hit or sneak attack that hits you. The damage is instead rolled normally.",
                                                                 "7cb52c6018ff4d7b9560341ba7240218",
                                                                 remove_disease.Icon,
                                                                 null,
                                                                 Common.createAddFortification(bonus: 50)
                                                                 );
                unusual_anatomy = Helpers.CreateFeature(prefix + "UnusualAnatomyFeature",
                                               unusual_anatomy_buff.Name,
                                               unusual_anatomy_buff.Description,
                                               "f8a61a0dd8bc49fbb7ae43b5bd1b4387",
                                               unusual_anatomy_buff.Icon,
                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, unusual_anatomy_buff, unusual_anatomy);
            }


            static void createAberrantResistance()
            {
                var spell_resistance = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889");
                var aberrant_resistance_buff = Helpers.CreateBuff(prefix + "AberrantResistanceBuff",
                                                                 "Aberrant Resistance",
                                                                 "At 16th level, you are immune to disease, exhaustion, fatigue, and poison, and to the staggered condition.",
                                                                 "2a916917494e4c188ce017aaa26c40e2",
                                                                 spell_resistance.Icon,
                                                                 null,
                                                                 Common.createAddConditionImmunity(UnitCondition.Exhausted),
                                                                 Common.createAddConditionImmunity(UnitCondition.Fatigued),
                                                                 Common.createAddConditionImmunity(UnitCondition.Staggered),
                                                                 Common.createBuffDescriptorImmunity(SpellDescriptor.Poison | SpellDescriptor.Disease | SpellDescriptor.Exhausted | SpellDescriptor.Fatigue | SpellDescriptor.Staggered)
                                                                 );
                aberrant_resistance = Helpers.CreateFeature(prefix + "AberrantResistanceFeature",
                                               aberrant_resistance_buff.Name,
                                               aberrant_resistance_buff.Description,
                                               "c35f914518a047c6b49ba024588357ef",
                                               aberrant_resistance_buff.Icon,
                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, aberrant_resistance_buff, aberrant_resistance);
            }


            static void createAberrantForm()
            {
                var constricting_coils = library.Get<BlueprintAbility>("3fce8e988a51a2a4ea366324d6153001");
                aberrant_form = Helpers.CreateFeature(prefix + "AberrantFormFeature",
                                                          "Aberrant Form",
                                                          "At 20th level, your body becomes truly unnatural. You are immune to critical hits and sneak attacks. In addition, you gain blindsight with a range of 60 feet and your bloodrager damage reduction increases by 1. You have these benefits constantly, even while not bloodraging.",
                                                          "aee46d11d5454482805ea2f6b5d7524b",
                                                          constricting_coils.Icon,
                                                          FeatureGroup.None,
                                                          Common.createBlindsight(60),
                                                          Common.createBuffDescriptorImmunity(SpellDescriptor.GazeAttack),
                                                          Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.GazeAttack),
                                                          Helpers.Create<AddImmunityToCriticalHits>(),
                                                          Helpers.Create<AddImmunityToPrecisionDamage>()
                                                          );
                //add rank to dr
                var rank_config = damage_reduction.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>();
                var feature_list = Helpers.GetField<BlueprintFeature[]>(rank_config, "m_FeatureList");
                feature_list = feature_list.AddToArray(aberrant_form);
                Helpers.SetField(rank_config, "m_FeatureList", feature_list);
            }
        }


        class CelestialBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature angelic_attacks;
            static internal BlueprintFeature celestial_resistances;
            static internal BlueprintFeature conviction;
            static internal BlueprintFeature wings_of_heaven;
            static internal BlueprintFeature angelic_protection;
            static internal BlueprintFeature ascension;
            static string prefix = "BloodragerBloodlineCelestial";


            static internal void create()
            {
                createAngelicAttacks();
                createCelestialResistances();
                createConviction();
                createWingsOfHeaven();
                createAngelicProtection();
                createAscension();

                var bless = library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638");
                var resist_energy = library.Get<BlueprintAbility>("21ffef7791ce73f468b6fca4d9371e8b");
                var heroism = library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63");
                var flamestrike = library.Get<BlueprintAbility>("f9910c76efc34af41b6e43d5d8752f0f");

                var dodge = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
                var mobility = library.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb");
                var dazzling_display = library.Get<BlueprintFeature>("bcbd674ec70ff6f4894bb5f07b6f4095");
                var cornugon_smash = library.Get<BlueprintFeature>("ceea53555d83f2547ae5fc47e0399e14");
                var improved_initiative = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
                var weapon_focus = library.Get<BlueprintFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
                var iron_will = library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334");


                progression = createBloodragerBloodline("Celestial",
                                                              "By way of a celestial ancestor or divine intervention, the blood of angels fills your body with a holy potency, granting you a majestic visage and angelic powers when you enter your bloodrage.\n"
                                                               + "Bonus Feats: Dodge, Mobility, Dazzling Display, Cornugon Smash, Improved Initiative, Iron Will, Weapon Focus.\n"
                                                               + "Bonus Spells: Bless (7th), resist energy (10th), heroism (13th), flamestrike (16th).",
                                                              library.Get<BlueprintAbility>("b1c7576bd06812b42bda3f09ab202f14").Icon, //angelic aspect greater
                                                              new BlueprintAbility[] { bless, resist_energy, heroism, flamestrike },
                                                              new BlueprintFeature[] { dodge, mobility, dazzling_display, cornugon_smash, improved_initiative, iron_will, weapon_focus },
                                                              new BlueprintFeature[] { angelic_attacks, celestial_resistances, conviction, wings_of_heaven, angelic_protection, ascension },
                                                              "356f9a6169d8480da772f02a13e2da29",
                                                              new string[] { "0921bf94bf174c8b8e0361057761ba7a", "ca818409470d4a56b8619c3604af345b", "c7dfde63fb8a41afa3f0b1fc0020bcac", "9d196e52c01c43ddbcca7b8a941144e7" },
                                                              "205d951fa55a4d9ca10f50592f1868b3"
                                                              ).progression;
            }

            static void createAngelicAttacks()
            {
                var crusaders_edge = library.Get<BlueprintAbility>("a26c23a887a6f154491dc2cefdad2c35");
                var resounding_blow_buff = library.Get<BlueprintBuff>("06173a778d7067a439acffe9004916e9");

                var outsider_feature = library.Get<BlueprintFeature>("9054d3988d491d944ac144e27b6bc318");
                var evil_feature = library.Get<BlueprintFeature>("5279fc8380dd9ba419b4471018ffadd1");

                var bonus_damage = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Helpers.CreateConditionHasFact(outsider_feature), Helpers.CreateConditionHasFact(evil_feature)),
                                                             Helpers.CreateActionDealDamage(DamageEnergyType.Holy, Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1)), IgnoreCritical: true)
                                                            );
                var bonus_damage_action = Helpers.CreateActionList(bonus_damage);
                var good_weapon = Common.createAddOutgoingAlignment(DamageAlignment.Good, check_range: true, is_ranged: false);
                var angelic_attacks_buff = Helpers.CreateBuff(prefix + "AngelicAttacksBuff",
                                                             "Angelic Attacks",
                                                             "At 1st level, your melee attacks are considered good-aligned weapons for the purpose of bypassing damage reduction. Furthermore, when you deal damage with a melee attack to an evil outsider, you deal an additional 1d6 points of damage. This additional damage stacks with effects such as align weapon and those granted by a weapon with the holy weapon special ability.",
                                                             "fcba2a8679de4213b31ecdbc9ab7a9ad",
                                                             crusaders_edge.Icon,
                                                             resounding_blow_buff.FxOnStart,
                                                             good_weapon,
                                                             Common.createAddInitiatorAttackWithWeaponTrigger(bonus_damage_action, only_hit: true, check_weapon_range_type: true,
                                                                                                              range_type: AttackTypeAttackBonus.WeaponRangeType.Melee)
                                                             );
                angelic_attacks = Helpers.CreateFeature(prefix + "AngelicAttacksFeature",
                                                               angelic_attacks_buff.Name,
                                                               angelic_attacks_buff.Description,
                                                               "300f0d5f0a9446078465bafc693a17ef",
                                                               angelic_attacks_buff.Icon,
                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, angelic_attacks_buff, angelic_attacks);
            }


            static void createCelestialResistances()
            {
                var damage_resistance = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("8cbf303d479cf0d42a8e36092c76fa7c");
                var resist_ca_buff = Helpers.CreateBuff(prefix + "CelestialResistancesBuff",
                                                    "Celestial Resistances",
                                                    "At 4th level, you gain resistance 5 to acid and cold. At 12th level, these resistances increase to 10.",
                                                    "2f932f4a8b32446993708c3e60e3aa54",
                                                    damage_resistance.Icon,
                                                    null,
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Acid),
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Cold),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                    ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                    classes: getBloodragerArray(),
                                                    customProgression: new (int, int)[] {
                                                                (11, 5),
                                                                (20, 10)
                                                    })
                                                    );
                celestial_resistances = Helpers.CreateFeature(prefix + "CelestialResistancesFeature",
                                                                               resist_ca_buff.Name,
                                                                               resist_ca_buff.Description,
                                                                               "4f59478befb0468081599205fbbce42a",
                                                                               resist_ca_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, resist_ca_buff, celestial_resistances);
            }


            static void createConviction()
            {
                var spell_resistance = library.Get<BlueprintAbility>("0a5ddfbcfb3989543ac7c936fc256889");
                var conviction_buff = Helpers.CreateBuff(prefix + "ConvictionBuff",
                                                    "Conviction",
                                                    "At 8th level, you take half damage from magical attacks with the evil descriptor. If such an attack allows a Reflex save for half damage, you take no damage on a successful saving throw.",
                                                    "accbfca27afe4dadbc1b02f603e32a1a",
                                                    spell_resistance.Icon,
                                                    null,
                                                     Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddNimbusDamageDivisor>()
                                                    );
                conviction = Helpers.CreateFeature(prefix + "ConvictionFeature",
                                                                               conviction_buff.Name,
                                                                               conviction_buff.Description,
                                                                               "223df27c2390449594a4d0fb78e9019e",
                                                                               conviction_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, conviction_buff, conviction);
            }


            static void createWingsOfHeaven()
            {
                //var angelic_wings_buff = library.CopyAndAdd<BlueprintBuff>("25699a90ed3299e438b6fd5548930809", prefix + "WingsOfHeavenBuff", "52bf0d70b8884943b83cdc96588a007f");
                var angelic_wings_buff = library.Get<BlueprintBuff>("25699a90ed3299e438b6fd5548930809");
                wings_of_heaven = Helpers.CreateFeature(prefix + "WingsOfHeavenFeature",
                                                                               "Wings of Heaven",
                                                                               "At 12th level, you gain a pair of wings that grant a +3 dodge bonus to AC against melee attacks and an immunity to ground based effects, such as difficult terrain.",
                                                                               "b5d48e26779048499194ade26ff0a741",
                                                                               angelic_wings_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, angelic_wings_buff, wings_of_heaven);
            }


            static void createAngelicProtection()
            {
                var sacred_nimbus_buff = library.Get<BlueprintBuff>("57b1c6a69c53f4d4ea9baec7d0a3a93a");
                var angelic_protection_buff = library.CopyAndAdd<BlueprintBuff>("6ab366720f4b8ed4f83ada36994d0890", prefix + "AngelicAspectBuff", "095a348c65564461b11398e8814d2c12"); //angelic aspect greater aura
                angelic_protection_buff.SetName("Angelic Protection");
                angelic_protection_buff.SetDescription("At 16th level, you gain a +4 deflection bonus to AC and a +4 resistance bonus on saving throws against attacks made or effects created by evil creatures.");
                angelic_protection_buff.SetIcon(sacred_nimbus_buff.Icon);
                angelic_protection = Helpers.CreateFeature(prefix + "AngelicProtectionFeature",
                                                                               angelic_protection_buff.Name,
                                                                               angelic_protection_buff.Description,
                                                                               "4ae1b076c2984982a9b2474ff0b980e7",
                                                                               angelic_protection_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, angelic_protection_buff, angelic_protection);
            }


            static void createAscension()
            {
                ascension = library.CopyAndAdd<BlueprintFeature>("d85e0396d8f68e047b7b67a1290f8dbc", prefix + "AscensionFeature", "17f896a2f8164f26a6c04523aa7ce708"); //from sorceror bloodline
                ascension.SetDescription("At 20th level, you become infused with the power of the heavens. You gain immunity to acid, cold, and petrification. You also gain resistance 10 to electricity and fire, as well as a +4 racial bonus on saving throws against poison. You have these benefits constantly, even while not bloodraging.");
            }

        }


        class FeyBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature confusing_critical;
            static internal BlueprintFeature woodland_stride;
            static internal BlueprintFeature blurring_movement;
            static internal BlueprintFeature quickling_bloodrage;
            static internal BlueprintFeature fleeting_glance;
            static internal BlueprintFeature fury_of_the_fey;
            static string prefix = "BloodragerBloodlineFey";


            static internal void create()
            {
                createConfusingCritical();
                createWoodlandStride();
                createBlurringMovement();
                createQuicklingBloodrage();
                createFleetingGlance();
                createFuryOfTheFey();

                var entangle = library.Get<BlueprintAbility>("0fd00984a2c0e0a429cf1a911b4ec5ca");
                var hideous_laughter = library.Get<BlueprintAbility>("fd4d9fd7f87575d47aafe2a64a6e2d8d");
                var haste = library.Get<BlueprintAbility>("486eaff58293f6441a5c2759c4872f98");
                var confusion = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949");

                var combat_reflexes = library.Get<BlueprintFeature>("0f8939ae6f220984e8fb568abbdfba95");
                var lightning_reflexes = library.Get<BlueprintFeature>("15e7da6645a7f3d41bdad7c8c4b9de1e");
                var dodge = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
                var mobility = library.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb");
                var improved_initiative = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
                var intimidating_prowess = library.Get<BlueprintFeature>("d76497bfc48516e45a0831628f767a0f");
                var hammer_the_gap = library.Get<BlueprintFeature>("7b64641c76ff4a744a2bce7f91a20f9a");


                progression = createBloodragerBloodline("Fey",
                                                              "One of your ancestors was fey, or the fey realm somehow intermixed with your bloodline. It affects your bloodrage in tricky and surprising ways.\n"
                                                               + "Bonus Feats: Dodge, Mobility, Combat Reflexes, Lightning Reflexes, Improved Initiative, Intimidating Prowess, Hammer the Gap.\n"
                                                               + "Bonus Spells: Entangle (7th), hideous laughter (10th), haste (13th), confusion (16th).",
                                                              library.Get<BlueprintProgression>("e8445256abbdc45488c2d90373f7dae8").Icon, // sorcerer fey bloodline
                                                              new BlueprintAbility[] { entangle, hideous_laughter, haste, confusion },
                                                              new BlueprintFeature[] { dodge, mobility, combat_reflexes, lightning_reflexes, improved_initiative, intimidating_prowess, hammer_the_gap },
                                                              new BlueprintFeature[] { confusing_critical, woodland_stride, blurring_movement, quickling_bloodrage, fleeting_glance, fury_of_the_fey },
                                                              "fe056d23f80c4c64b4ee3a8e6f2973b2",
                                                              new string[] { "78b0900bd71c454baeb5886494fb0ae0", "2dbe75eadf684985a252608ae6f86259", "bcf1a280d91e435087c1180e3188ea41", "75c37819a8f74d02a3da486c46bf4e0c" },
                                                              "30fb198d84af4b2498fa1b3f35668494"
                                                              ).progression;
            }


            static void createConfusingCritical()
            {
                var blade_barrier = library.Get<BlueprintAbility>("36c8971e91f1745418cc3ffdfac17b74");
                var confusion_buff = library.Get<BlueprintBuff>("886c7407dc629dc499b9f1465ff382df");

                var effect_action = Helpers.CreateActionList(Common.createContextSavedApplyBuff(confusion_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1))));
                var action = Helpers.CreateActionList(Common.createContextActionSavingThrow(SavingThrowType.Will, effect_action));
                var confusing_critical_buff = Helpers.CreateBuff(prefix + "ConfusingCriticalBuff",
                                                                "Confusing Critical",
                                                                "At 1st level, fey power courses through your attacks. Each time you confirm a critical hit, the target must succeed at a Will saving throw or be confused for 1 round. The DC of this save is equal to 10 + 1/2 your bloodrager level + your Constitution modifier. This is a mind-affecting compulsion effect.",
                                                                "7a54fe2bbf624e60b10121356218a3a5",
                                                                blade_barrier.Icon,
                                                                null,
                                                                Common.createAddInitiatorAttackRollTrigger2(action, critical_hit: true),
                                                                Common.createContextCalculateAbilityParamsBasedOnClass(bloodrager_class, StatType.Constitution)
                                                                );
                confusing_critical = Helpers.CreateFeature(prefix + "ConfusingCriticalFeature",
                                                               confusing_critical_buff.Name,
                                                               confusing_critical_buff.Description,
                                                               "54d034a7d7de4da083464496ab4aba61",
                                                               confusing_critical_buff.Icon,
                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, confusing_critical_buff, confusing_critical);
            }


            static void createWoodlandStride()
            {
                woodland_stride = library.CopyAndAdd<BlueprintFeature>("11f4072ea766a5840a46e6660894527d",
                                                             prefix + "WooldlandStrideFeature",
                                                             "c017c81495ee40d68dacbe5b794ef2a4");
                woodland_stride.SetDescription("At 4th level, you can move through any sort difficult terrain at your normal speed and without taking damage or suffering any other impairment.");
            }


            static void createBlurringMovement()
            {
                var blur_buff = library.CopyAndAdd<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674", prefix + "BlurringMovementBuff", "f2c369b10d9049c79ec6660ad0a0bece"); //for compatibility
                var blur_buff0 = library.Get<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674");
                blurring_movement = Helpers.CreateFeature(prefix + "BlurringMovementFeature",
                                                                               "Blurring Movement",
                                                                               "At 8th level, you become a blur of motion when you bloodrage. You gain effect of blur spell while in bloodrage.",
                                                                               "c7027b9ddd534fc09025ae807e5af38a",
                                                                               blur_buff0.Icon,
                                                                               FeatureGroup.None);
                blur_buff.SetName(blurring_movement.Name);
                blur_buff.SetDescription(blurring_movement.Description);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, blur_buff0, blurring_movement);
            }


            static void createQuicklingBloodrage()
            {
                var haste_spell = library.Get<BlueprintAbility>("486eaff58293f6441a5c2759c4872f98");
                var haste_buff0 = library.Get<BlueprintBuff>("03464790f40c3c24aa684b57155f3280");
                haste_buff0.SetName(haste_spell.Name);
                haste_buff0.SetDescription(haste_spell.Description);
                haste_buff0.SetIcon(haste_spell.Icon);
                var haste_buff = library.CopyAndAdd<BlueprintBuff>("03464790f40c3c24aa684b57155f3280", prefix + "QuicklingBloodrageBuff", "ff4c584a792149ff9d0246bc77cc2a85"); //for compatibility


                //change haste bonus types to enchancement to avoid stacking
                /*var haste_boni = haste_buff.GetComponents<Kingmaker.UnitLogic.FactLogic.AddStatBonus>().ToArray();
                foreach (var b in haste_boni)
                {
                    b.Descriptor = ModifierDescriptor.Enhancement;
                }*/


                quickling_bloodrage = Helpers.CreateFeature(prefix + "QuicklingBloodrageFeature",
                                                                               "Quickling Bloodrage",
                                                                               "At 12th level, while bloodraging you’re treated as if you are under the effects of haste.",
                                                                               "f08420c83d6443068038b9160406438a",
                                                                               haste_spell.Icon,
                                                                               FeatureGroup.None);

                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, haste_buff0, quickling_bloodrage);
            }


            static void createFleetingGlance()
            {
                var resource = library.Get<BlueprintAbilityResource>("cb4518f1e176db44cbab819bb29c9b49"); //from sorc bloodline
                var amount = Helpers.GetField(resource, "m_MaxAmount");
                BlueprintCharacterClass[] classes = (BlueprintCharacterClass[])Helpers.GetField(amount, "Class");
                classes = classes.AddToArray(bloodrager_class);
                Helpers.SetField(amount, "Class", classes);
                Helpers.SetField(resource, "m_MaxAmount", amount);

                var fleeting_glance_ability = library.CopyAndAdd<BlueprintActivatableAbility>("ba5ead16619d2c64697968224163280b", prefix + "FleetingGlanceActivatableAbility", "625503853eea46f78e049841c0d27d4c");
                fleeting_glance_ability.SetDescription("At 16th level, you can turn invisible for a number of rounds per day equal to your bloodrager level. This ability functions as greater invisibility. These rounds need not be consecutive. You can use this ability even when not bloodraging.\nGreater Invisibility: This spell functions like invisibility, except that it doesn't end if the subject attacks.\nInvisibility: The creature becomes invisible. If a check is required, an invisible creature has a +20 bonus on its Stealth checks. The spell ends if the subject attacks any creature. For purposes of this spell, an attack includes any spell targeting a foe or whose area or effect includes a foe. Exactly who is a foe depends on the invisible character's perceptions. Actions directed at unattended objects do not break the spell. Causing harm indirectly is not an attack. Thus, an invisible being can open doors, talk, eat, climb stairs, summon monsters and have them attack, cut the ropes holding a rope bridge while enemies are on the bridge, remotely trigger traps, open a portcullis to release attack dogs, and so forth. If the subject attacks directly, however, it immediately becomes visible along with all its gear. Spells such as bless that specifically affect allies but not foes are not attacks for this purpose, even when they include foes in their area.");
                fleeting_glance_ability.ReplaceComponent<ActivatableAbilityResourceLogic>(Helpers.CreateActivatableResourceLogic(resource, ResourceSpendType.NewRound));

                fleeting_glance = Helpers.CreateFeature(prefix + "FleetingGlanceFeature",
                                                        fleeting_glance_ability.Name,
                                                        fleeting_glance_ability.Description,
                                                        "09b86c3330854d4cb1f303d6e3fea473",
                                                        fleeting_glance_ability.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddAbilityResource(resource),
                                                        Helpers.CreateAddFact(fleeting_glance_ability)
                                                       );
            }


            static void createFuryOfTheFey()
            {
                var bane_weapon_buff = library.CopyAndAdd<BlueprintBuff>("be190d2dd5433dd41a4aa00e1abc9a5b", prefix + "FuryOfTheFeyBuff", "51e32fabc97549a6ad56bd5d414a7d65");
                bane_weapon_buff.SetName("Fury of the Fey");
                bane_weapon_buff.SetBuffFlags(BuffFlags.StayOnDeath);
                bane_weapon_buff.SetDescription("At 20th level, when entering a bloodrage you can choose one type of creature (and subtype for humanoids or outsiders) that can be affected by the bane weapon special ability. All of your melee attacks are considered to have bane against that type. This ability doesn’t stack with other forms of bane.");
                fury_of_the_fey = Helpers.CreateFeature(prefix + "FuryOfTheFeyFeature",
                                                                               bane_weapon_buff.Name,
                                                                               bane_weapon_buff.Description,
                                                                               "9234d31a673840f6954d1baad879bbb8",
                                                                               bane_weapon_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, bane_weapon_buff, fury_of_the_fey);
            }

        }


        class InfernalBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature hellfire_strike;
            static internal BlueprintFeature infernal_resistance;
            static internal BlueprintFeature diabolical_arrogance;
            static internal BlueprintFeature dark_wings;
            static internal BlueprintFeature hellfire_charge;
            static internal BlueprintFeature fiend_of_the_pit;
            static string prefix = "BloodragerBloodlineInfernal";


            static internal void create()
            {
                createHellfireStrike();
                createInfernalResistance();
                createDiabolicalArrogance();
                createDarkWings();
                createHellfireCharge();
                createFiendOfThePit();

                var protection_from_good = library.Get<BlueprintAbility>("2ac7637daeb2aa143a3bae860095b63e");
                var scorching_ray = library.Get<BlueprintAbility>("cdb106d53c65bbc4086183d54c3b97c7");
                var hold_person = library.Get<BlueprintAbility>("c7104f7526c4c524f91474614054547e");
                var shield_of_dawn = library.Get<BlueprintAbility>("62888999171921e4dafb46de83f4d67d");

                var combat_reflexes = library.Get<BlueprintFeature>("0f8939ae6f220984e8fb568abbdfba95");
                var deceitful = library.Get<BlueprintFeature>("231a37321e26551489503e4e1d99e681");
                var blindfight = library.Get<BlueprintFeature>("4e219f5894ad0ea4daa0699e28c37b1d");
                var improved_sunder = library.Get<BlueprintFeature>("9719015edcbf142409592e2cbaab7fe1");
                var improved_disarm = library.Get<BlueprintFeature>("25bc9c439ac44fd44ac3b1e58890916f");
                var intimidating_prowess = library.Get<BlueprintFeature>("d76497bfc48516e45a0831628f767a0f");
                var iron_will = library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334");


                progression = createBloodragerBloodline("Infernal",
                                                              "The Pit lives in your blood. Maybe one of your ancestors was seduced by the powers of Hell or made a deal with a devil. Either way, its corruption seethes within your lineage.\n"
                                                               + "Bonus Feats: Blind - Fight, Combat Reflexes, Deceitful, Improved Disarm, Improved Sunder, Intimidating Prowess, Iron Will.\n"
                                                               + "Bonus Spells: Protection from good (7th), scorching ray(10th), hold preson(13th), fire_shield(16th).",
                                                              library.Get<BlueprintProgression>("e76a774cacfb092498177e6ca706064d").Icon, // sorcerer infernal bloodline
                                                              new BlueprintAbility[] { protection_from_good, scorching_ray, hold_person, NewSpells.fire_shield },
                                                              new BlueprintFeature[] { combat_reflexes, deceitful, blindfight, improved_disarm, improved_sunder, intimidating_prowess, iron_will },
                                                              new BlueprintFeature[] { hellfire_strike, infernal_resistance, diabolical_arrogance, dark_wings, hellfire_charge, fiend_of_the_pit },
                                                              "346f332ed9b843638a228257a59743b7",
                                                              new string[] { "02339061142647e8835f6f91c1485a76", "9a2c466398d04102b43c1a78ead78937", "592cf48fec304b4f8a2a5005a0939895", "11eab4cb7d9a4081b6788bfe9cb97fc4" },
                                                              "2d083855bf614669a8dd82ac2ff93207"
                                                              ).progression;
            }

            static void createHellfireStrike()
            {
                var burning_arc = library.Get<BlueprintAbility>("eaac3d36e0336cb479209a6f65e25e7c");

                var bonus_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Fire, Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageBonus)), IgnoreCritical: true);
                var bonus_damage_action = Helpers.CreateActionList(bonus_damage);

                var hellfire_strike_buff = Helpers.CreateBuff(prefix + "HellfireStrikeBuff",
                                                             "Hellfire Strike",
                                                             "At 1st level, you deal additional 1d6 points of fire damage on critical hit. At 12th level this damage increases to 2d6.",
                                                             "a97d9794ce2549709997a61f89ffcacb",
                                                             burning_arc.Icon,
                                                             null,
                                                             Common.createAddInitiatorAttackWithWeaponTrigger(bonus_damage_action, critical_hit: true, check_weapon_range_type: true,
                                                                                                              range_type: AttackTypeAttackBonus.WeaponRangeType.Melee),
                                                             Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Custom,
                                                                                             type: AbilityRankType.DamageBonus, classes: getBloodragerArray(),
                                                                                             customProgression: new (int, int)[] {
                                                                                                            (11, 1),
                                                                                                            (20, 2)
                                                                                             })
                                                             );
                hellfire_strike = Helpers.CreateFeature(prefix + "HellfireStirkeFeature",
                                                               hellfire_strike_buff.Name,
                                                               hellfire_strike_buff.Description,
                                                               "1740de7601f94a109bee9c7061c4085e",
                                                               hellfire_strike_buff.Icon,
                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, hellfire_strike_buff, hellfire_strike);
            }


            static void createInfernalResistance()
            {
                var damage_resistance = library.Get<Kingmaker.Blueprints.Classes.BlueprintFeature>("8cbf303d479cf0d42a8e36092c76fa7c");
                var resist_buff = Helpers.CreateBuff(prefix + "InfernalResistanceBuff",
                                                    "Infernal Resistance",
                                                    "At 4th level, you gain fire resistance 5, as well as a + 2 bonus on saving throws against poison.At 8th level, your fire resistance increases to 10, and the bonus on saving throws against poison increases to + 4.",
                                                    "befdf5624cd7418891b0dc62712c1456",
                                                    damage_resistance.Icon,
                                                    null,
                                                    Common.createEnergyDRContextRank(DamageEnergyType.Fire, AbilityRankType.StatBonus),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                    ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                                                    classes: getBloodragerArray(),
                                                                                    customProgression: new (int, int)[] {
                                                                                                (7, 5),
                                                                                                (20, 10)
                                                                                                }
                                                                                    ),
                                                    Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.DamageDice), ModifierDescriptor.UntypedStackable,
                                                                                                          SpellDescriptor.Poison
                                                                                                          ),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                    ContextRankProgression.Custom, AbilityRankType.DamageDice,
                                                                                    classes: getBloodragerArray(),
                                                                                    customProgression: new (int, int)[] {
                                                                                                (7, 5),
                                                                                                (20, 10)
                                                                                                }
                                                                                    )
                                                    );
                infernal_resistance = Helpers.CreateFeature(prefix + "InfernalResistanceFeature",
                                                                               resist_buff.Name,
                                                                               resist_buff.Description,
                                                                               "b40f0fc02adb43ad9ffd8341154b1980",
                                                                               resist_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, resist_buff, infernal_resistance);
            }


            static void createDiabolicalArrogance()
            {
                var mind_blank = library.Get<BlueprintAbility>("df2a0ba6b6dcecf429cbb80a56fee5cf");

                var buff = Helpers.CreateBuff(prefix + "DiabolicalArroganceBuff",
                                              "Diabolical Arrogance",
                                              "At 8th level, you gain a +4 bonus on saving throws against enchantment and fear effects.",
                                              "abcff44aae1e475b855a9792a2d91f40",
                                              mind_blank.Icon,
                                              null,
                                              Common.createSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Fear),
                                              Common.createSavingThrowBonusAgainstSchool(4, ModifierDescriptor.UntypedStackable, SpellSchool.Enchantment)
                                             );
                diabolical_arrogance = Helpers.CreateFeature(prefix + "DiabolicalArroganceFeature",
                                                                               buff.Name,
                                                                               buff.Description,
                                                                               "487473aa12bb4829885341e889b5cb97",
                                                                               buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, buff, diabolical_arrogance);
            }


            static void createDarkWings()
            {
                //var diabolic_wings_buff = library.CopyAndAdd<BlueprintBuff>("4113178a8d5bf4841b8f15b1b39e004f", prefix + "DarkWingsBuff", "cd8e632e520d417cb20766b7e124909a");
                var diabolic_wings_buff = library.Get<BlueprintBuff>("4113178a8d5bf4841b8f15b1b39e004f");
                dark_wings = Helpers.CreateFeature(prefix + "DarkWingsFeature",
                                                                               "Dark Wings",
                                                                               "At 12th level, you gain a pair of wings that grant a +3 dodge bonus to AC against melee attacks and an immunity to ground based effects, such as difficult terrain.",
                                                                               "9c451731dd7f4f96adec8e9621610841",
                                                                               diabolic_wings_buff.Icon,
                                                                               FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, diabolic_wings_buff, dark_wings);
            }


            static void createHellfireCharge()
            {
                var charge_buff = library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066");
                var flaming_burst = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>("3f032a3cd54e57649a0cdad0434bf221");
                var flaming = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");
                var weapon_enchant_buff = Helpers.CreateBuff(prefix + "HellfireChargeBuff",
                                                             "Hellfire Charge",
                                                             "At 16th level, when you charge the attack you make at the end of the charge is considered to be performed with weapon having flaming burst property.",
                                                             "b0439659723f4a8da680965c78a8fbf5",
                                                             charge_buff.Icon,
                                                             null,
                                                             Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, false, flaming),
                                                             Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), false, false, flaming_burst)
                                                             );
                hellfire_charge = Helpers.CreateFeature(prefix + "HellfireChargeFeature",
                                                                                weapon_enchant_buff.Name,
                                                                                weapon_enchant_buff.Description,
                                                                                "d26ca0ac64874157aad34ef664b116a9",
                                                                                weapon_enchant_buff.Icon,
                                                                                FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(charge_buff, weapon_enchant_buff, hellfire_charge, bloodrage_buff);
            }


            static void createFiendOfThePit()
            {
                var power_of_the_pit = library.Get<BlueprintFeature>("b6afdc50876e08149b1f9fdcdb2a308c");//from sorcerer infernal bloodline
                fiend_of_the_pit = Helpers.CreateFeature(prefix + "FiendOfThePitFeature",
                                                         "Fiend of the Pit",
                                                         "At 20th level, you gain immunity to fire and poison. You also gain resistance 10 to acid and cold, and gain blindsense within 60 feet. You have these benefits constantly, even while not bloodraging.",
                                                         "15b9af6383e7473eb8aa3a384ee9a78c",
                                                         power_of_the_pit.Icon,
                                                         FeatureGroup.None,
                                                         Common.createEnergyDR(10, DamageEnergyType.Cold),
                                                         Common.createEnergyDR(10, DamageEnergyType.Acid),
                                                         Common.createBlindsense(60),
                                                         Common.createBuffDescriptorImmunity(SpellDescriptor.Poison),
                                                         Common.createAddEnergyDamageImmunity(DamageEnergyType.Fire)
                                                         );
            }
        }


        class UndeadBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature frightful_charger;
            static internal BlueprintFeature ghost_strike;
            static internal BlueprintFeature deaths_gift;
            static internal BlueprintFeature frightening_strikes;
            static internal BlueprintFeature incorporeal_bloodrager;
            static internal BlueprintFeature one_foot_in_the_grave;
            static string prefix = "BloodragerBloodlineUndead";


            static internal void create()
            {
                createFrightfulCharger();
                createGhostStrike();
                createDeathsGift();
                createFrighteningStrikes();
                createIncorporealBloodrager();
                createOneFootInTheGrave();

                var ray_of_enfeeblement = library.Get<BlueprintAbility>("450af0402422b0b4980d9c2175869612");
                var false_life = library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762");
                var vampiric_touch = library.Get<BlueprintAbility>("8a28a811ca5d20d49a863e832c31cce1");
                var enervation = library.Get<BlueprintAbility>("f34fb78eaaec141469079af124bcfa0f");

                var endurance = library.Get<BlueprintFeature>("54ee847996c25cd4ba8773d7b8555174");
                var diehard = library.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad");
                var dodge = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
                var toughness = library.Get<BlueprintFeature>("d09b20029e9abfe4480b356c92095623");
                var mobility = library.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb");
                var intimidating_prowess = library.Get<BlueprintFeature>("d76497bfc48516e45a0831628f767a0f");
                var iron_will = library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334");


                progression = createBloodragerBloodline("Undead",
                                                              "The foul corruption of undeath is a part of you. Somewhere in the past, death became infused with your lineage. Your connection to the attributes of the undead bestows frightening power when your bloodrage.\n"
                                                               + "Bonus Feats: Diehard, Dodge, Endurance, Intimidating Prowess, Iron Will, Mobility, Toughness.\n"
                                                               + "Bonus Spells: Ray of enfeeblement (7th), false life (10th), vampiric touch (13th), enervation (16th).",
                                                              library.Get<BlueprintProgression>("a1a8bf61cadaa4143b2d4966f2d1142e").Icon, // sorcerer undead bloodline
                                                              new BlueprintAbility[] { ray_of_enfeeblement, false_life, vampiric_touch, enervation },
                                                              new BlueprintFeature[] { endurance, diehard, dodge, toughness, mobility, intimidating_prowess, iron_will },
                                                              new BlueprintFeature[] { frightful_charger, ghost_strike, deaths_gift, frightening_strikes, incorporeal_bloodrager, one_foot_in_the_grave },
                                                              "c9474ae9021543ff84633825b16b25f2",
                                                              new string[] { "290830ff3e8a470aaf1ebfd696dd1be5", "65d95f832c5649a9951d6d384085dcf2", "ca173aea1a5c41baa455f0146135ba18", "d83b97e1facf41ca9ef281a753536910" },
                                                              "e82330114e464cf193d521a47a273f5a"
                                                              ).progression;
            }


            static void createFrightfulCharger()
            {
                var shaken_buff = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
                var charge_buff = library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066");

                var action = Common.createContextActionApplyBuff(shaken_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.DamageBonus), DurationRate.Rounds), is_child: true, dispellable: false);
                var effect_buff = Helpers.CreateBuff(prefix + "FrightfulChargerBuff",
                                                             "Frightful Charger",
                                                             "At 1st level, when you hit a creature with a charge attack, that creature becomes shaken for a number of rounds equal to 1/2 your bloodrager level (minimum 1). This effect does not cause an existing shaken or frightened condition (from this ability or another source) to turn into frightened or panicked. This is a mind-affecting fear effect.",
                                                             "61aff33f69d84391b49782fb976cf870",
                                                             charge_buff.Icon,
                                                             null,
                                                             Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(action)),
                                                             Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getBloodragerArray(),
                                                                                             type: AbilityRankType.DamageBonus, min: 1, progression: ContextRankProgression.Div2)
                                                             );
                frightful_charger = Helpers.CreateFeature(prefix + "FrightfulChargerFeature",
                                                                                effect_buff.Name,
                                                                                effect_buff.Description,
                                                                                "2ba88b87439e456cb382392ba07ffa96",
                                                                                effect_buff.Icon,
                                                                                FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(charge_buff, effect_buff, frightful_charger, bloodrage_buff);
            }


            static void createGhostStrike()
            {
                var ghost_touch_magus = library.Get<BlueprintBuff>("9069fd94f3f5b8b448b28ce0a5ee8fd6");
                var ghost_touch = library.Get<Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f");
                var weapon_enchant_buff = Helpers.CreateBuff(prefix + "GhostStrikeBuff",
                                                             "Ghost Strike",
                                                             "At 4th level, your melee attacks are treated as if they have the ghost touch weapon special ability.",
                                                             "0f662d1b1432483190aa928ca7b9d355",
                                                             ghost_touch_magus.Icon,
                                                             null,
                                                             Common.createBuffEnchantWornItem(ghost_touch)
                                                             );
                ghost_strike = Helpers.CreateFeature(prefix + "GhostStrikeFeature",
                                                                                weapon_enchant_buff.Name,
                                                                                weapon_enchant_buff.Description,
                                                                                "85c240197d8d4f1f86b4f55f7bf51397",
                                                                                weapon_enchant_buff.Icon,
                                                                                FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, weapon_enchant_buff, ghost_strike);
            }


            static void createDeathsGift()
            {
                var sorc_deaths_gift = library.Get<BlueprintFeature>("fd5f14d44e82f464196fdf0ea82347cc");

                var buff = Helpers.CreateBuff(prefix + "DeathsGiftBuff",
                                              "Death's Gift",
                                              "At 8th level, you gain cold resistance 10, as well as DR 10/magic.",
                                              "f13abf4af63841e48f99c3005b163f75",
                                              sorc_deaths_gift.Icon,
                                              null,
                                              Common.createEnergyDR(10, DamageEnergyType.Cold),
                                              Common.createMagicDR(10)
                                              );
                deaths_gift = Helpers.CreateFeature(prefix + "DeathsGiftFeature",
                                                                                buff.Name,
                                                                                buff.Description,
                                                                                "19cdee308a0b4492a2b780adb115ab00",
                                                                                buff.Icon,
                                                                                FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, buff, deaths_gift);
            }


            static void createFrighteningStrikes()
            {
                var reckless_stance = library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");

                var frightened = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf"); //frightened
                var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220"); //shaken

                var fear_action = Helpers.CreateConditional(Helpers.CreateConditionHasBuff(shaken),
                                                       Common.createContextActionApplyBuff(frightened, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false),
                                                       Common.createContextActionApplyBuff(shaken, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false)
                                                       );
                var save_action = Helpers.CreateConditionalSaved(new Kingmaker.ElementsSystem.GameAction[0], new Kingmaker.ElementsSystem.GameAction[] { fear_action });
                var action = Helpers.CreateActionList(Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(save_action)));

                var frightening_strikes_buff = Helpers.CreateBuff(prefix + "FrighteningStrikesBuff",
                                                                  "Frightening Strikes",
                                                                  "At 12th level, as a swift action during bloodrage you can empower your melee attacks with fear. For 1 round, creatures you hit with your melee attacks that fail a Will saving throw become shaken. Creatures who are already shaken become frightened. The DC of this save is equal to 10 + 1/2 your bloodrager level + your Constitution modifier. This is a mind-affecting fear effect.",
                                                                  "1a11f46ae4414b499d990d84b435fe81",
                                                                  frightened.Icon,
                                                                  null,
                                                                  Common.createAddInitiatorAttackWithWeaponTrigger(action, check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Melee),
                                                                  Common.createContextCalculateAbilityParamsBasedOnClass(character_class: bloodrager_class, stat: StatType.Constitution)
                                                                  );

                frightening_strikes = Common.createSwitchActivatableAbilityBuff(prefix + "FrighteningStrikes",
                                                                                "62c4051ff92f49d681a9d7fad040aca9", "4890b60e6cef4e4684b1c60e0a5c299a", "d32c7ed12d7e4cb09f5b4d9709dc7706",
                                                                                frightening_strikes_buff, bloodrage_buff, reckless_stance.ActivateWithUnitAnimation,
                                                                                command_type: CommandType.Swift,
                                                                                unit_command: CommandType.Swift
                                                                                );
                frightening_strikes_buff.SetBuffFlags(BuffFlags.HiddenInUi | frightening_strikes_buff.GetBuffFlags());

            }


            static void createIncorporealBloodrager()
            {
                var ability = library.Get<BlueprintAbility>("853b5266404060f4f8afd9cf7859ef1f");
                var context_rank_config = ability.GetComponent<ContextRankConfig>();
                BlueprintCharacterClass[] classes = (BlueprintCharacterClass[])Helpers.GetField(context_rank_config, "m_Class");
                classes = classes.AddToArray(bloodrager_class);
                Helpers.SetField(context_rank_config, "m_Class", classes);
                ability.SetDescription("You can become incorporeal for 1 round per sorcerer level. While in this form, you gain the incorporeal subtype. You take no damage from non-magic weapons. You also take only half damage from any source not dealing ghost, holy, divine, or force damage. You can use this ability once per day.");

                incorporeal_bloodrager = library.CopyAndAdd<BlueprintFeature>("eafdc6762cbfa7d4d8220c6d6372973d", prefix + "IncorporealBloodragerFeature", "71742ba4b5434da6b5dfb9a0ccf79b19");
                incorporeal_bloodrager.SetName("Incorporeal Bloodrager");
                incorporeal_bloodrager.SetDescription("At 16th level, once per day you can choose to become incorporeal for 1 round per bloodrager level. You take only half damage from magic corporeal sources, and you take no damage from non-magic weapons and objects. You can use this ability even if not bloodraging.");
            }


            static void createOneFootInTheGrave()
            {
                one_foot_in_the_grave = library.CopyAndAdd<BlueprintFeature>("b3e403ebbdad8314386270fefc4b4cc8", prefix + "OneFootInTheGrave", "7ee1a466730f493f8957cbf65cbb9aa0");
                one_foot_in_the_grave.SetName("One foot in the Grave");
                one_foot_in_the_grave.SetDescription("At 20th level, you gain immunity to cold, paralysis, and sleep. The DR from your damage reduction ability increases to 8. You gain a +4 morale bonus on saving throws made against spells and spell-like abilities cast by undead. You have these benefits constantly, even while not bloodraging.");
                one_foot_in_the_grave.RemoveComponent(one_foot_in_the_grave.GetComponent<AddDamageResistancePhysical>());

                //add rank to dr
                var rank_config = damage_reduction.GetComponent<Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig>();
                var feature_list = Helpers.GetField<BlueprintFeature[]>(rank_config, "m_FeatureList");
                feature_list = feature_list.AddToArray(one_foot_in_the_grave, one_foot_in_the_grave, one_foot_in_the_grave);
                Helpers.SetField(rank_config, "m_FeatureList", feature_list);
            }
        }

        class DestinedBloodline
        {
            static internal BlueprintProgression progression;
            static internal BlueprintFeature destined_strike;
            static internal BlueprintFeature fated_bloodrager;
            static internal BlueprintFeature certain_strike;
            static internal BlueprintFeature defy_death;
            static internal BlueprintFeature unstoppable;
            static internal BlueprintFeature victory_or_death;
            static string prefix = "BloodragerBloodlineDestined";


            static internal void create()
            {
                createDestinedStrike();
                createFatedBloodrager();
                createCertainStrike();
                createDefyDeath();
                createUnstoppable();
                createVictoryOrDeath();

                var shield = library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4");
                var blur = library.Get<BlueprintAbility>("14ec7a4e52e90fa47a4c8d63c69fd5c1");
                var protection_from_energy = library.Get<BlueprintAbility>("d2f116cfe05fcdd4a94e80143b67046f");
                var freedom_of_movement = library.Get<BlueprintAbility>("0087fc2d64b6095478bc7b8d7d512caf");

                var endurance = library.Get<BlueprintFeature>("54ee847996c25cd4ba8773d7b8555174");
                var diehard = library.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad");
                var improved_initiative = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
                var lightning_reflexes = library.Get<BlueprintFeature>("15e7da6645a7f3d41bdad7c8c4b9de1e");
                var dazzling_display = library.Get<BlueprintFeature>("bcbd674ec70ff6f4894bb5f07b6f4095");
                var intimidating_prowess = library.Get<BlueprintFeature>("d76497bfc48516e45a0831628f767a0f");
                var weapon_focus = library.Get<BlueprintFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");


                progression = createBloodragerBloodline("Destined",
                                                              "Your bloodline is destined for great things. When you bloodrage, you exude a greatness that makes all but the most legendary creatures seem lesser.\n"
                                                               + "Bonus Feats: Diehard, Endurance, Improved Initiative, Intimidating Prowess, Dazzling Display, Lightning Reflexes, Weapon Focus.\n"
                                                               + "Bonus Spells: shield (7th), blur (10th), protection from energy (13th), freedom of movement (16th).",
                                                              library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63").Icon, // heroism
                                                              new BlueprintAbility[] { shield, blur, protection_from_energy, freedom_of_movement },
                                                              new BlueprintFeature[] { endurance, diehard, dazzling_display, improved_initiative, weapon_focus, intimidating_prowess, lightning_reflexes },
                                                              new BlueprintFeature[] { destined_strike, fated_bloodrager, certain_strike, defy_death, unstoppable, victory_or_death },
                                                              "",
                                                              new string[] { "", "", "", "" },
                                                              ""
                                                              ).progression;
            }

            static void createDestinedStrike()
            {
                var true_strike = library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00");
                var destined_strike_buff = Helpers.CreateBuff(prefix + "DestinedStrikeBuff",
                                                              "Destined Strike",
                                                              "At 1st level, as a free action up to three times per day you can grant yourself an insight bonus equal to 1/2 your bloodrager level (minimum 1) on one melee attack. At 12th level, you can use this ability up to five times per day.",
                                                              "",
                                                              true_strike.Icon,
                                                              null,
                                                              Common.createAttackTypeAttackBonus(Helpers.CreateContextValue(AbilityRankType.DamageBonus), AttackTypeAttackBonus.WeaponRangeType.Melee,
                                                                                                 ModifierDescriptor.Insight),
                                                              Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                                                              min: 1, classes: getBloodragerArray(), type: AbilityRankType.DamageBonus)
                                                             );
                var resource = Helpers.CreateAbilityResource(prefix + "DestinedStrikeResource",
                                                             destined_strike_buff.Name,
                                                             "",
                                                             "",
                                                             null
                                                             );
                resource.SetFixedResource(3);
                var destined_strike_ability = Helpers.CreateActivatableAbility(prefix + "DestinedStrikeActivatableAbility",
                                                                               destined_strike_buff.Name,
                                                                               destined_strike_buff.Description,
                                                                               "",
                                                                               destined_strike_buff.Icon,
                                                                               destined_strike_buff,
                                                                               AbilityActivationType.Immediately,
                                                                               CommandType.Free,
                                                                               null,
                                                                               Helpers.CreateActivatableResourceLogic(resource, ResourceSpendType.Attack)
                                                                               );
                addBloodrageRestriction(destined_strike_ability);
                var destined_strike_additional_use = Helpers.CreateFeature(prefix + "DestinedStrikeAdditionalUsesFeature",
                                                                           "",
                                                                           "",
                                                                           "",
                                                                           destined_strike_buff.Icon,
                                                                           FeatureGroup.None,
                                                                           Helpers.CreateIncreaseResourceAmount(resource, 2)
                                                                           );
                destined_strike_additional_use.HideInCharacterSheetAndLevelUp = true;
                destined_strike = Helpers.CreateFeature(prefix + "DestinedStrikeFeature",
                                                        destined_strike_buff.Name,
                                                        destined_strike_buff.Description,
                                                        "",
                                                        destined_strike_buff.Icon,
                                                        FeatureGroup.None,
                                                        Helpers.CreateAddFact(destined_strike_ability),
                                                        Helpers.CreateAddAbilityResource(resource),
                                                        Helpers.CreateAddFeatureOnClassLevel(destined_strike_additional_use, 12, getBloodragerArray(), new BlueprintArchetype[0])
                                                        );
            }

            static void createFatedBloodrager()
            {
                var mirror_image = library.Get<BlueprintAbility>("3e4ab69ada402d145a5e0ad3ad4b8564");
                var fated_bloodrager_buff = Helpers.CreateBuff(prefix + "FatedBloodragerBuff",
                                                               "Fated Bloodrager",
                                                               "At 4th level, you gain a +1 luck bonus to AC and on saving throws. At 8th level and every 4 levels thereafter, this bonus increases by 1 (to a maximum of +5 at 20th level).",
                                                               "",
                                                               mirror_image.Icon,
                                                               null,
                                                               Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Luck, ContextValueType.Rank, AbilityRankType.Default),
                                                               Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Luck, ContextValueType.Rank, AbilityRankType.Default),
                                                               Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Luck, ContextValueType.Rank, AbilityRankType.Default),
                                                               Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Luck, ContextValueType.Rank, AbilityRankType.Default),
                                                               Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                                               startLevel: 4, stepLevel: 4, classes: getBloodragerArray()
                                                                                               )
                                                              );
                fated_bloodrager = Helpers.CreateFeature(prefix + "FatedBloodragerFeature",
                                                                                fated_bloodrager_buff.Name,
                                                                                fated_bloodrager_buff.Description,
                                                                                "",
                                                                                fated_bloodrager_buff.Icon,
                                                                                FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, fated_bloodrager_buff, fated_bloodrager);
            }


            static void createCertainStrike()
            {
                //replace with the following:
                //once every 4 levels allow to reroll one failed attack
                var knights_resolve = library.Get<BlueprintBuff>("f7bf9fc0d400d7243aaca01b14f8c935");
                var resource = Helpers.CreateAbilityResource(prefix + "CertainStrikeResource", "", "", "", knights_resolve.Icon);
                resource.SetIncreasedByLevelStartPlusDivStep(0, 8, 2, 4, 1, 0, 0.0f, getBloodragerArray());

                var reroll = Helpers.Create<NewMechanics.ModifyD20WithActions>();
                reroll.Rule = NewMechanics.ModifyD20WithActions.RuleType.AttackRoll;
                reroll.RollsAmount = 1;
                reroll.TakeBest = true;
                reroll.RerollOnlyIfFailed = true;
                reroll.actions = Helpers.CreateActionList();
                reroll.required_resource = resource;

                var certain_strike_buff = Helpers.CreateBuff(prefix + "CertainStrikeBuff",
                                                         "Certain Strike",
                                                         "At 8th level, you may decide to reroll a failed attack roll once per day. You can use this ability once per day per 4 levels of bloodrager.",
                                                         "",
                                                         knights_resolve.Icon,
                                                         null,
                                                         reroll);


                var certain_strike_ability = library.CopyAndAdd<BlueprintAbility>("6f9af630d43d4c2498a127ea84cb1c8a", prefix + "CertainStrikeAbility", "");
                certain_strike_ability.SetName(certain_strike_buff.Name);
                certain_strike_ability.SetDescription(certain_strike_buff.Description);
                certain_strike_ability.ActionType = CommandType.Free;
                certain_strike_ability.ComponentsArray = new BlueprintComponent[] {Helpers.CreateRunActions(Common.createContextActionApplyBuff(certain_strike_buff,
                                                                                                                                                Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds),
                                                                                                                                                is_child: false,
                                                                                                                                                dispellable: false
                                                                                                                                                )
                                                                                                           ),
                                                                                   Helpers.CreateResourceLogic(resource)
                                                                                  };
                certain_strike_ability.Type = AbilityType.Supernatural;

                var toggle = Helpers.CreateActivatableAbility(prefix + "CertainStrikeToggleAbility",
                                                              certain_strike_ability.Name,
                                                              certain_strike_ability.Description,
                                                              "44a41da87ea34b7ca8602ff7a3a76aaa",
                                                              certain_strike_ability.Icon,
                                                              certain_strike_buff,
                                                              AbilityActivationType.Immediately,
                                                              CommandType.Free,
                                                              null,
                                                              Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                                              Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => r.resource = resource));
                toggle.DeactivateImmediately = true;

                addBloodrageRestriction(certain_strike_ability);
                addBloodrageRestriction(toggle);
                certain_strike = Helpers.CreateFeature(prefix + "CertainStrikeFeature",
                                                       certain_strike_buff.Name,
                                                       certain_strike_buff.Description,
                                                       "",
                                                       certain_strike_buff.Icon,
                                                       FeatureGroup.None,
                                                       Helpers.CreateAddFact(toggle),
                                                       Helpers.CreateAddAbilityResource(resource)
                                                           );
            }


            static void createDefyDeath()
            {
                var raise_dead = library.Get<BlueprintAbility>("a0fc99f0933d01643b2b8fe570caa4c5");
                var resource = Helpers.CreateAbilityResource(prefix + "DefyDeathResource", "", "", "", raise_dead.Icon);
                resource.SetFixedResource(1);

                var death_check = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionSkillCheck>();
                death_check.CustomDC = Common.createSimpleContextValue(20);
                death_check.Stat = StatType.SaveFortitude;
                death_check.Failure = Helpers.CreateActionList();
                death_check.Success = Helpers.CreateActionList(Helpers.Create<ContextActionResurrect>());
                death_check.UseCustomDC = true;
                var defy_death_buff = Helpers.CreateBuff(prefix + "DefyDeathBuff",
                                   "Defy Death",
                                   "At 12th level, once per day when an attack or spell would result in your death, you can attempt a DC 20 Fortitude save. If you succeed, you are instead revived with 10% of health.",
                                   "",
                                   raise_dead.Icon,
                                   null,
                                   Common.createDeathActions(Helpers.CreateActionList(death_check), resource),
                                   Helpers.CreateAddAbilityResource(resource)
                                   );

                defy_death = Helpers.CreateFeature(prefix + "DefyDeathFeature",
                                   defy_death_buff.Name,
                                   defy_death_buff.Description,
                                   "",
                                   raise_dead.Icon,
                                   FeatureGroup.None,
                                   Helpers.CreateAddAbilityResource(resource)
                                   );
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, defy_death_buff, defy_death);
            }


            static void createUnstoppable()
            {
                var shield_of_faith = library.Get<BlueprintAbility>("183d5bb91dea3a1489a6db6c9cb64445");

                var unstoppable_buff = Helpers.CreateBuff(prefix + "UnstoppableBuff",
                                                          "Unstoppable",
                                                          "At 16th level, any critical threats you score are automatically confirmed. Any critical threats made against you confirm only if the second roll results in a natural 20 (or is automatically confirmed).",
                                                          "",
                                                          shield_of_faith.Icon,
                                                          null,
                                                          Common.createCriticalConfirmationACBonus(100),
                                                          Common.createCriticalConfirmationBonus(100)
                                                          );
                unstoppable = Helpers.CreateFeature(prefix + "UnstoppableFeature",
                                                                                unstoppable_buff.Name,
                                                                                unstoppable_buff.Description,
                                                                                "",
                                                                                unstoppable_buff.Icon,
                                                                                FeatureGroup.None);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, unstoppable_buff, unstoppable);
            }


            static void createVictoryOrDeath()
            {
                var confusion = library.Get<BlueprintAbility>("cf6c901fb7acc904e85c63b342e9c949");
                victory_or_death = Helpers.CreateFeature(prefix + "VictoryOrDeath",
                                                         "Victory or Death",
                                                         "At 20th level, you are immune to paralysis and petrification, as well as to the stunned, dazed, and staggered conditions. You have these benefits constantly, even while not bloodraging.",
                                                         "",
                                                         confusion.Icon,
                                                         FeatureGroup.None,
                                                         Common.createBuffDescriptorImmunity(SpellDescriptor.Stun),
                                                         Common.createBuffDescriptorImmunity(SpellDescriptor.Daze),
                                                         Common.createBuffDescriptorImmunity(SpellDescriptor.Petrified),
                                                         Common.createBuffDescriptorImmunity(SpellDescriptor.Staggered),
                                                         Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis),
                                                         Common.createAddConditionImmunity(UnitCondition.Staggered),
                                                         Common.createAddConditionImmunity(UnitCondition.Stunned),
                                                         Common.createAddConditionImmunity(UnitCondition.Dazed),
                                                         Common.createAddConditionImmunity(UnitCondition.Petrified),
                                                         Common.createAddConditionImmunity(UnitCondition.Paralyzed)
                                                         );
            }
        }



        class DraconicBloodlines
        {
            public static List<BlueprintProgression> progressions = new List<BlueprintProgression>();
            static List<BlueprintFeature> claws = new List<BlueprintFeature>();
            static List<BlueprintFeature> draconic_resistance = new List<BlueprintFeature>();
            static List<BlueprintFeature> breath_weapon = new List<BlueprintFeature>();
            static List<BlueprintFeature> breath_weapon_extra_use = new List<BlueprintFeature>();
            static List<BlueprintFeature> dragon_wings = new List<BlueprintFeature>();
            static List<BlueprintFeature> dragon_form = new List<BlueprintFeature>();
            static List<BlueprintFeature> power_of_the_wyrms = new List<BlueprintFeature>();

            static string prefix = "BloodragerBloodlineDraconic";

            struct DraconicBloodlineData
            {
                public UnityEngine.Sprite icon;
                public BlueprintBuff wings_prototype;
                public BlueprintAbility breath_weapon_prototype;
                public BlueprintBuff dragon_form_prototype;
                public DamageEnergyType energy_type;
                public Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment claws_enchantment;
                public BlueprintFeature power_of_the_wyrms;
                public UnityEngine.Sprite resistance_icon;
                public string prefix;
                public string name;

                public string energy_string;
                public string breath_area_string;
                public BlueprintProgression sorc_progression;

                public DraconicBloodlineData(UnityEngine.Sprite bloodline_icon, BlueprintBuff wings, BlueprintAbility breath_weapon, BlueprintBuff dragon_form, DamageEnergyType energy,
                                      BlueprintWeaponEnchantment enchantment, BlueprintFeature power_of_the_wyrms_feat, BlueprintProgression sorceror_progression, string bloodline_name, string breath_string)
                {
                    icon = bloodline_icon;
                    wings_prototype = wings;
                    breath_weapon_prototype = breath_weapon;
                    dragon_form_prototype = dragon_form;
                    energy_type = energy;
                    claws_enchantment = enchantment;
                    name = bloodline_name;
                    prefix = "BloodragerBloodlineDraconic" + bloodline_name;
                    energy_string = energy.ToString().ToLower();
                    breath_area_string = breath_string;
                    power_of_the_wyrms = power_of_the_wyrms_feat;
                    sorc_progression = sorceror_progression;

                    switch (energy)
                    {
                        case DamageEnergyType.Acid:
                            resistance_icon = library.Get<BlueprintAbility>("fedc77de9b7aad54ebcc43b4daf8decd").Icon;
                            break;
                        case DamageEnergyType.Cold:
                            resistance_icon = library.Get<BlueprintAbility>("5368cecec375e1845ae07f48cdc09dd1").Icon;
                            break;
                        case DamageEnergyType.Electricity:
                            resistance_icon = library.Get<BlueprintAbility>("90987584f54ab7a459c56c2d2f22cee2").Icon;
                            break;
                        default: //fire
                            resistance_icon = library.Get<BlueprintAbility>("ddfb4ac970225f34dbff98a10a4a8844").Icon;
                            break;
                    }
                }
            }

            static DraconicBloodlineData[] bloodlines;


            public static void create()
            {
                var corrosive_enchantment = library.Get<BlueprintWeaponEnchantment>("633b38ff1d11de64a91d490c683ab1c8");
                var flaming_enchantment = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");
                var shocking_enchantment = library.Get<BlueprintWeaponEnchantment>("7bda5277d36ad114f9f9fd21d0dab658");
                var frost_enchantment = library.Get<BlueprintWeaponEnchantment>("421e54078b7719d40915ce0672511d0b");
                bloodlines = new DraconicBloodlineData[]
                {
                    new DraconicBloodlineData(library.Get<BlueprintProgression>("7bd143ead2d6c3a409aad6ee22effe34").Icon, library.Get<BlueprintBuff>("ddfe6e85e1eed7a40aa911280373c228"),
                                              library.Get<BlueprintAbility>("1e65b0b2db777e24db96d8bc52cc9207"), library.Get<BlueprintBuff>("9eb5ba8c396d2c74c8bfabd3f5e91050"),
                                              DamageEnergyType.Acid, corrosive_enchantment, library.Get<BlueprintFeature>("25397838424fac04197e226a268ddce6"), 
                                              library.Get<BlueprintProgression>("7bd143ead2d6c3a409aad6ee22effe34"), "Black", "60-foot line"),
                    new DraconicBloodlineData(library.Get<BlueprintProgression>("8a7f100c02d0b254d8f5f3affc8ef386").Icon, library.Get<BlueprintBuff>("800cde038f9e6304d95365edc60ab0a4"),
                                              library.Get<BlueprintAbility>("60a3047f434f38544a2878c26955d3ad"), library.Get<BlueprintBuff>("cf8b4e861226e0545a6805036ab2a21b"),
                                              DamageEnergyType.Electricity, shocking_enchantment, library.Get<BlueprintFeature>("582b25fef37373c4591a7994f3da9c69"),
                                              library.Get<BlueprintProgression>("8a7f100c02d0b254d8f5f3affc8ef386"), "Blue", "60-foot line"),
                    new DraconicBloodlineData(library.Get<BlueprintProgression>("5f9ecbee67db8364985e9d0500eb25f1").Icon, library.Get<BlueprintBuff>("7f5acae38fc1e0f4c9325d8a4f4f81fc"),
                                              library.Get<BlueprintAbility>("531a57e0c19f80945b68bdb3e289279a"), library.Get<BlueprintBuff>("f7fdc15aa0219104a8b38c9891cac17b"),
                                              DamageEnergyType.Fire, flaming_enchantment, library.Get<BlueprintFeature>("8f10f507dde8d86488fda84cb136de47"),
                                              library.Get<BlueprintProgression>("5f9ecbee67db8364985e9d0500eb25f1"), "Brass", "60-foot line"),
                    new DraconicBloodlineData(library.Get<BlueprintProgression>("7e0f57d8d00464441974e303b84238ac").Icon, library.Get<BlueprintBuff>("482ee5d001527204bb86e34240e2ce65"),
                                              library.Get<BlueprintAbility>("732291d7ac20b0949aae002622e00b34"), library.Get<BlueprintBuff>("53e408cab2331bd48a3db846e531dfe8"),
                                              DamageEnergyType.Electricity, shocking_enchantment, library.Get<BlueprintFeature>("d81844b1f549df1418ccc40ccca3274a"),
                                              library.Get<BlueprintProgression>("7e0f57d8d00464441974e303b84238ac"), "Bronze", "60-foot line"),
                    new DraconicBloodlineData(library.Get<BlueprintProgression>("b522759a265897b4f8f7a1a180a692e4").Icon, library.Get<BlueprintBuff>("a25d6fc69cba80548832afc6c4787379"),
                                              library.Get<BlueprintAbility>("826ef8251d9243941b432f97d901e938"), library.Get<BlueprintBuff>("799c8b6ae43c7d741ac7887c984f2aa2"),
                                              DamageEnergyType.Acid, corrosive_enchantment, library.Get<BlueprintFeature>("6a885e6b343e4b44ab56e0dd47ff83fb"),
                                              library.Get<BlueprintProgression>("b522759a265897b4f8f7a1a180a692e4"), "Copper", "60-foot line"),
                    new DraconicBloodlineData(library.Get<BlueprintProgression>("6c67ef823db8d7d45bb0ef82f959743d").Icon, library.Get<BlueprintBuff>("984064a3dd0f25444ad143b8a33d7d92"),
                                              library.Get<BlueprintAbility>("598e33639b662784fb07c0e4c8978aa4"), library.Get<BlueprintBuff>("4300f60c00ecabc439deab11ce6d738a"),
                                              DamageEnergyType.Fire, flaming_enchantment, library.Get<BlueprintFeature>("3247396087a747148b17e1a0e37a3e67"),
                                              library.Get<BlueprintProgression>("6c67ef823db8d7d45bb0ef82f959743d"), "Gold", "30-foot cone"),
                    new DraconicBloodlineData(library.Get<BlueprintProgression>("7181be57d1cc3bc40bc4b552e4e4ce24").Icon, library.Get<BlueprintBuff>("a4ccc396e60a00f44907e95bc8bf463f"),
                                              library.Get<BlueprintAbility>("633b622267c097d4abe3ec6445c05152"), library.Get<BlueprintBuff>("070543328d3e9af49bb514641c56911d"),
                                              DamageEnergyType.Acid, corrosive_enchantment, library.Get<BlueprintFeature>("c593e3279e68cc649b976e685e5b8900"),
                                              library.Get<BlueprintProgression>("7181be57d1cc3bc40bc4b552e4e4ce24"), "Green", "30-foot cone"),
                    new DraconicBloodlineData(library.Get<BlueprintProgression>("8c6e5b3cf12f71e43949f52c41ae70a8").Icon, library.Get<BlueprintBuff>("08ae1c01155a2184db869e9ebedc758d"),
                                              library.Get<BlueprintAbility>("3f31704e595e78942b3640cdc9b95d8b"), library.Get<BlueprintBuff>("40a96969339f3c241b4d989910f255e1"),
                                              DamageEnergyType.Fire, flaming_enchantment, library.Get<BlueprintFeature>("a18ab74c10933e84daf76afdaabc28dd"),
                                              library.Get<BlueprintProgression>("8c6e5b3cf12f71e43949f52c41ae70a8"), "Red", "30-foot cone"),
                    new DraconicBloodlineData(library.Get<BlueprintProgression>("c7d2f393e6574874bb3fc728a69cc73a").Icon, library.Get<BlueprintBuff>("5a791c1b0bacee3459d7f5137fa0bd5f"),
                                              library.Get<BlueprintAbility>("11d03ebc508d6834cad5992056ad01a4"), library.Get<BlueprintBuff>("16857109dafc2b94eafd1e888552ef76"),
                                              DamageEnergyType.Cold, frost_enchantment, library.Get<BlueprintFeature>("a1d338a76b127b54eba7dc9a85532f3f"),
                                              library.Get<BlueprintProgression>("c7d2f393e6574874bb3fc728a69cc73a"), "Silver", "30-foot cone"),
                    new DraconicBloodlineData(library.Get<BlueprintProgression>("b0f79497a0d1f4f4b8293e82c8f8fa0c").Icon, library.Get<BlueprintBuff>("381a168acd79cd54baf87a17ca861d9b"),
                                              library.Get<BlueprintAbility>("84be529914c90664aa948d8266bb3fa6"), library.Get<BlueprintBuff>("2652c61dff50a24479520c84005ede8b"),
                                              DamageEnergyType.Cold, frost_enchantment, library.Get<BlueprintFeature>("c06fe7c8722ad5a42b241f11246d2679"),
                                              library.Get<BlueprintProgression>("b0f79497a0d1f4f4b8293e82c8f8fa0c"), "White", "30-foot cone")
                };

                createClaws();
                createDraconicResistance();
                createBreathWeapon();
                createDragonWings();
                createDragonForm();
                createPowerOfTheWyrms();

                var shield = library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4");
                var resist_energy = library.Get<BlueprintAbility>("21ffef7791ce73f468b6fca4d9371e8b");
                var dispel_magic = library.Get<BlueprintAbility>("92681f181b507b34ea87018e8f7a528a");
                var fear = library.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0");

                var blind_fight = library.Get<BlueprintFeature>("4e219f5894ad0ea4daa0699e28c37b1d");
                var cleave = library.Get<BlueprintFeature>("d809b6c4ff2aaff4fa70d712a70f7d7b");
                var improved_initiative = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
                var great_fortitude = library.Get<BlueprintFeature>("79042cb55f030614ea29956177977c52");
                var power_attack = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
                var skill_focus_mobility = library.Get<BlueprintFeature>("52dd89af385466c499338b7297896ded");
                var toughness = library.Get<BlueprintFeature>("d09b20029e9abfe4480b356c92095623");

                for (int i = 0; i < bloodlines.Length; i++)
                {
                    var bloodline_info = createBloodragerBloodline("Draconic",
                                                                  "At some point in your family’s history, a dragon interbred with your bloodline. Now, the sublime monster’s ancient power fuels your bloodrage.\n"
                                                                   + "Bonus Feats: Blind-Fight, Cleave, Great Fortitude, Improved Initiative, Power Attack, Skill Focus (Mobility), Toughness.\n"
                                                                   + "Bonus Spells: shield (7th), resist energy (10th), fly (13th), fear (16th).",
                                                                  bloodlines[i].icon,
                                                                  new BlueprintAbility[] { shield, resist_energy, NewSpells.fly, fear },
                                                                  new BlueprintFeature[] { blind_fight, cleave, great_fortitude, improved_initiative, power_attack, skill_focus_mobility, toughness },
                                                                  new BlueprintFeature[] { claws[i], draconic_resistance[i], breath_weapon[i], dragon_wings[i], dragon_form[i], power_of_the_wyrms[i] },
                                                                  "",
                                                                  new string[] { "", "", "", "" },
                                                                  "",
                                                                  bloodlines[i].name);
                    progressions.Add(bloodline_info.progression);
                    addToDragonDisciple(bloodlines[i].sorc_progression, bloodline_info, i);
                }
            }


            static void addToDragonDisciple(BlueprintProgression sorc_progression, BloodlineInfo bloodrager_bloodline_info, int bloodline_id)
            {
                var dragon_disciple_class = library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
                var dd_feat_selection = Rebalance.dd_feat_subselection;
                dd_feat_selection.AllFeatures = dd_feat_selection.AllFeatures.AddToArray(bloodrager_bloodline_info.bonus_feats);
                dd_feat_selection.Features = dd_feat_selection.Features.AddToArray(bloodrager_bloodline_info.bonus_feats);

                bloodrager_bloodline_info.progression.Classes = bloodrager_bloodline_info.progression.Classes.AddToArray(dragon_disciple_class);
                var blood_of_dragons_selection = library.Get<BlueprintFeatureSelection>("da48f9d7f697ae44ca891bfc50727988");

                var draconic_feature_prerequisites = dragon_disciple_class.GetComponent<PrerequisiteFeaturesFromList>();
                draconic_feature_prerequisites.Features = draconic_feature_prerequisites.Features.AddToArray(bloodrager_bloodline_info.progression);
                blood_of_dragons_selection.AllFeatures = blood_of_dragons_selection.AllFeatures.AddToArray(bloodrager_bloodline_info.progression);

                var dragon_wings = library.Get<BlueprintFeature>("aa36f82ab9a046c4a853dccf0cdbaf53");
                dragon_wings.SetName("Wings");
                dragon_wings.SetDescription("At 9th level, a dragon disciple gains the wings bloodline power, even if his level does not yet grant that power");
                var dragon_form_give = library.Get<BlueprintFeature>("0ee31c9fc504b51489d12d43168b5009");
                dragon_form_give.SetName("Dragon Form");
                dragon_form_give.SetDescription("At 7th level, a dragon disciple can assume the form of a dragon. This ability works like form of the dragon I. At 10th level, this ability functions as form of the dragon II and the dragon disciple can use this ability twice per day. His caster level for this effect is equal to his effective sorcerer levels for his draconic bloodline. Whenever he casts form of the dragon, he must assume the form of a dragon of the same type as his bloodline.");
                var dragon_form1 = library.Get<BlueprintFeature>("76dc9a65841190d46a8fc25dce00a242");
                dragon_form1.SetName(dragon_form_give.Name);
                dragon_form1.SetDescription(dragon_form_give.Description);
                var dragon_form2 = library.Get<BlueprintFeature>("717699191d106eb46a7d820a872ed24d");
                dragon_form2.SetName(dragon_form1.Name);
                dragon_form2.SetDescription(dragon_form1.Description);
                var breath_weapon_dd = library.Get<BlueprintFeature>("0aadb51129cb0c147b5d2464c0db10b3");
                BlueprintFeature[] dragon_features = new BlueprintFeature[] {library.Get<BlueprintFeature>("01971351119121d429ecf62c2ab94de3"), //elemental dmg bite
                                                                             breath_weapon_dd, //breath weapon
                                                                             dragon_wings, //wings
                                                                             dragon_form1, //dragon form I
                                                                             dragon_form2 //dragon form II
                                                                            };
                
                foreach (var f in dragon_features)
                {
                    var bloodline_entry = f.GetComponents<AddFeatureIfHasFact>().Where(h => h.CheckedFact == sorc_progression).ToArray()[0].CreateCopy();
                    bloodline_entry.CheckedFact = bloodrager_bloodline_info.progression;
                    if (f == breath_weapon_dd)
                    {
                        var feat = library.CopyAndAdd<BlueprintFeature>(breath_weapon[bloodline_id].AssetGuid, breath_weapon[bloodline_id].name + "Disciple", "");
                        bloodline_entry.Feature = feat;
                    }
                    f.AddComponent(bloodline_entry);
                }

            }


            static void createClaws()
            {
                var claw1d6 = library.CopyAndAdd<BlueprintItemWeapon>("18dc77b96c009804399c834e028d0552", prefix + "Claw1d6", ""); //from sorc draconic bloodline
                var claw1d8 = library.CopyAndAdd<BlueprintItemWeapon>("18dc77b96c009804399c834e028d0552", prefix + "Claw1d8", "");
                Helpers.SetField(claw1d8, "m_DamageDice", new Kingmaker.RuleSystem.DiceFormula(1, DiceType.D8));

                List<BlueprintItemWeapon> claws1d8Energy = new List<BlueprintItemWeapon>();

                foreach (var b in bloodlines)
                {
                    var claw_energy = library.CopyAndAdd<BlueprintItemWeapon>("18dc77b96c009804399c834e028d0552", b.prefix + "Claw1d8" + b.claws_enchantment.name, "");
                    Helpers.SetField(claw_energy, "m_DamageDice", new Kingmaker.RuleSystem.DiceFormula(1, DiceType.D8));
                    Helpers.SetField(claw_energy, "m_Enchantments", new BlueprintWeaponEnchantment[] { b.claws_enchantment });
                    claws1d8Energy.Add(claw_energy);
                }

                var claw_buff1 = library.CopyAndAdd<BlueprintBuff>("fe712a5237d918342936c0761cdc2d3e", prefix + "Claw1Buff", ""); //from sorcerer bloodline
                claw_buff1.ReplaceComponent<Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride>(Common.createEmptyHandWeaponOverride(claw1d6));
                var claw_buff2 = library.CopyAndAdd<BlueprintBuff>("4824413d436653546931aaddb9e71280", prefix + "Claw2Buff", ""); //from sorcerer bloodline
                var claw_buff3 = library.CopyAndAdd<BlueprintBuff>("4824413d436653546931aaddb9e71280", prefix + "Claw3Buff", "");
                claw_buff3.ReplaceComponent<Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride>(Common.createEmptyHandWeaponOverride(claw1d8));
                List<BlueprintBuff> claws4_buff_energy = new List<BlueprintBuff>();
                for (int i = 0; i < bloodlines.Length; i++)
                {
                    var claw_buff4 = library.CopyAndAdd<BlueprintBuff>("4824413d436653546931aaddb9e71280", bloodlines[i].prefix + "Claw4Buff", "");
                    claw_buff4.ReplaceComponent<Kingmaker.Designers.Mechanics.Buffs.EmptyHandWeaponOverride>(Common.createEmptyHandWeaponOverride(claws1d8Energy[i]));
                    claws4_buff_energy.Add(claw_buff4);
                }

                var claws1_feature = Helpers.CreateFeature(prefix + "Claws1Feature", "Claws",
                                                          "At 1st level, you grow claws. These claws are treated as natural weapons, allowing you to make two claw attacks as a full attack, using your full base attack bonus. These attacks deal 1d6 points of damage each (1d4 if you are Small) plus your Strength modifier. At 4th level, these claws are considered magic weapons for the purpose of overcoming damage reduction. At 8th level, the damage increases to 1d8 points (1d6 if you are Small). At 12th level, these claws deal an additional 1d6 points of damage of your energy type on a hit.",
                                                          "",
                                                          claw1d6.Icon,
                                                          FeatureGroup.None);
                claws1_feature.HideInCharacterSheetAndLevelUp = true;
                var claws2_feature = Helpers.CreateFeature(prefix + "Claws2Feature", "Claws",
                                                          claws1_feature.Description,
                                                          "",
                                                          claws1_feature.Icon,
                                                          FeatureGroup.None,
                                                          Common.createRemoveFeatureOnApply(claws1_feature));
                claws2_feature.HideInCharacterSheetAndLevelUp = true;
                var claws3_feature = Helpers.CreateFeature(prefix + "Claws3Feature", "Claws",
                                                          claws1_feature.Description,
                                                          "",
                                                          claws1_feature.Icon,
                                                          FeatureGroup.None,
                                                          Common.createRemoveFeatureOnApply(claws2_feature),
                                                          Common.createRemoveFeatureOnApply(claws1_feature));
                claws3_feature.HideInCharacterSheetAndLevelUp = true;

                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, claw_buff1, claws1_feature);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, claw_buff2, claws2_feature);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, claw_buff3, claws3_feature);

                for (int i = 0; i < bloodlines.Length; i++)
                {
                    var claws4_feature = Helpers.CreateFeature(bloodlines[i].prefix + "Claws4Feature", "Claws",
                                                              claws1_feature.Description,
                                                              "",
                                                              claws1_feature.Icon,
                                                              FeatureGroup.None,
                                                              Common.createRemoveFeatureOnApply(claws3_feature),
                                                              Common.createRemoveFeatureOnApply(claws2_feature),
                                                              Common.createRemoveFeatureOnApply(claws1_feature));
                    claws4_feature.HideInCharacterSheetAndLevelUp = true;
                    var claw = Helpers.CreateFeature(bloodlines[i].prefix + "ClawsFeature", "Claws",
                                                              claws1_feature.Description,
                                                              "",
                                                              claws1_feature.Icon,
                                                              FeatureGroup.None,
                                                              Helpers.CreateAddFeatureOnClassLevel(claws1_feature, 1, getDraconicArray(), null),
                                                              Helpers.CreateAddFeatureOnClassLevel(claws2_feature, 4, getDraconicArray(), null),
                                                              Helpers.CreateAddFeatureOnClassLevel(claws3_feature, 8, getDraconicArray(), null),
                                                              Helpers.CreateAddFeatureOnClassLevel(claws4_feature, 12, getDraconicArray(), null)
                                                              );
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, claws4_buff_energy[i], claws4_feature);
                    claws.Add(claw);
                }
            }


            static void createDraconicResistance()
            {
                foreach (var b in bloodlines)
                {
                    var buff = Helpers.CreateBuff(b.prefix + "DraconicResistancesBuff",
                                                        "Draconic Resistance",
                                                        $"At 4th level, you gain {b.energy_string} resistance 5  and a +1 natural armor bonus to AC. At 8th level, your {b.energy_string} resistance increases to 10 and your natural armor bonus increases to +2. At 16th level, your natural armor bonus increases to +4.",
                                                        "",
                                                        b.resistance_icon,
                                                        null,
                                                        Common.createEnergyDRContextRank(b.energy_type, AbilityRankType.StatBonus),
                                                                                         Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                         ContextRankProgression.Custom, AbilityRankType.StatBonus,
                                                                                         classes: getDraconicArray(),
                                                                                         customProgression: new (int, int)[] {
                                                                                                (7, 5),
                                                                                                (20, 10)
                                                                                         }),
                                                        Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.NaturalArmor, rankType: AbilityRankType.DamageBonus),
                                                        Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                                                                                        ContextRankProgression.Custom, AbilityRankType.DamageBonus,
                                                                                        classes: getDraconicArray(),
                                                                                        customProgression: new (int, int)[] {
                                                                                                (7, 1),
                                                                                                (15, 2),
                                                                                                (20, 4)
                                                                                        })
                                                        );
                    var feat = Helpers.CreateFeature(b.prefix + "DraconicResistancesFeature",
                                                     buff.Name,
                                                     buff.Description,
                                                     "",
                                                     buff.Icon,
                                                     FeatureGroup.None
                                                    );
                    draconic_resistance.Add(feat);
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, buff, feat);
                }
            }


            static void createBreathWeapon()
            {
                var resource = library.CopyAndAdd<BlueprintAbilityResource>("bebe2a97cc091934189fd8255e903b1f", prefix + "BreathWeaponResource", ""); //sorc bloodline resource
                resource.SetIncreasedByLevelStartPlusDivStep(1, 16, 1, 4, 1, 0, 0.0f, getDraconicArray());
                //var add_resource = library.Get<BlueprintFeature>("7459c25b2cc9cdd4d8367cb555f0fe5a");
                foreach (var b in bloodlines)
                {
                    var breath_ability = library.CopyAndAdd<BlueprintAbility>(b.breath_weapon_prototype.AssetGuid, b.prefix + "BreathWeaponAbility", "");
                    breath_ability.SetDescription($"At 8th level, you gain a breath weapon that you can use once per day. This breath weapon deals 1d{BalanceFixes.getDamageDieString(DiceType.D6)} points of {b.energy_string} damage per bloodrager level. Those caught in the area of the breath can attempt a Reflex saving throw for half damage. The DC of this save is equal to 10 + 1/2 your bloodrager level + your Constitution modifier. The shape of the breath weapon is a {b.breath_area_string}. At 16th level, you can use this ability twice per day. At 20th level, you can use this ability three times per day.");
                    var rank_config = breath_ability.GetComponent<ContextRankConfig>();
                    var classes = Helpers.GetField<BlueprintCharacterClass[]>(rank_config, "m_Class"); 
                    classes = classes.AddToArray(bloodrager_class);
                    Helpers.SetField(rank_config, "m_Class", classes); // will propagate to prototype to bloodrager o be combined with sorc or dd
                    breath_ability.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(getDraconicArray(), StatType.Constitution));
                    breath_ability.ReplaceComponent<AbilityResourceLogic>(Helpers.CreateResourceLogic(resource));
                    addBloodrageRestriction(breath_ability);

                    var breath_feature = Helpers.CreateFeature(b.prefix + "BreathWeaponFeature",
                                                               breath_ability.Name,
                                                               breath_ability.Description,
                                                               "",
                                                               breath_ability.Icon,
                                                               FeatureGroup.None,
                                                               Helpers.CreateAddFact(breath_ability),
                                                               Helpers.CreateAddAbilityResource(resource)
                                                               );
                    breath_feature.HideInCharacterSheetAndLevelUp = true;
                    var breath_extra_use = Helpers.CreateFeature(b.prefix + "BreathWeaponExtraUseFeature",
                                           breath_ability.Name,
                                           breath_ability.Description,
                                           "",
                                           breath_ability.Icon,
                                           FeatureGroup.None,
                                           Helpers.CreateIncreaseResourceAmount(resource,1)
                                           );
                    breath_extra_use.HideInCharacterSheetAndLevelUp = true;
                    var feat = Helpers.CreateFeature(b.prefix + "BreathWeaponBaseFeature",
                                                          breath_ability.Name,
                                                          breath_ability.Description,
                                                          "",
                                                          breath_ability.Icon,
                                                          FeatureGroup.None,
                                                          Common.createAddFeatureIfHasFact(breath_feature, breath_extra_use),
                                                          Common.createAddFeatureIfHasFact(breath_feature, breath_feature, true)
                                                          );
                    feat.Ranks = 5;
                    breath_weapon_extra_use.Add(breath_extra_use);
                    breath_weapon.Add(feat);
                }
            }


            static void createDragonWings()
            {
                foreach (var b in bloodlines)
                {
                    //var wings_buff = library.CopyAndAdd<BlueprintBuff>(b.wings_prototype.AssetGuid, b.prefix + "DragonWingsBuff", "");
                    var wings_feature = Helpers.CreateFeature(b.prefix + "DragonWings",
                                                                                   "Dragon Wings ",
                                                                                   "At 12th level, you gain a pair of leathery wings that grant a +3 dodge bonus to AC against melee attacks and an immunity to ground based effects, such as difficult terrain.",
                                                                                   "",
                                                                                   b.wings_prototype.Icon,
                                                                                   FeatureGroup.None);
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, b.wings_prototype, wings_feature);
                    dragon_wings.Add(wings_feature);
                }
            }


            static void createDragonForm()
            {
                var reckless_stance = library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");
                var remove_polymorph = Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Polymorph);
                foreach (var b in bloodlines)
                {
                    var buff = library.CopyAndAdd<BlueprintBuff>(b.dragon_form_prototype.AssetGuid, b.prefix + "DragonFormBuff", "");
                    buff.SetName("Dragon Form");
                    buff.SetDescription("At 16th level, when entering a bloodrage, you can choose to take the form of your chosen dragon type (as Dragonkind II).");
                    //remove turn back 
                    var polymorph = buff.GetComponent<Kingmaker.UnitLogic.Buffs.Polymorph>().CreateCopy();
                    polymorph.Facts = new BlueprintUnitFact[0];
                    buff.ReplaceComponent<Kingmaker.UnitLogic.Buffs.Polymorph>(polymorph);
                    var scaling = Helpers.Create<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>();
                    scaling.CharacterClasses = getDraconicArray();
                    scaling.StatType = StatType.Constitution;
                    buff.AddComponent(scaling);
                    buff.AddComponent(Helpers.CreateSuppressBuffs(b.wings_prototype));

                    var feature = Common.createSwitchActivatableAbilityBuff(b.prefix + "DragonForm", "", "", "",
                                                                            buff,
                                                                            bloodrage_buff,
                                                                            new Kingmaker.ElementsSystem.GameAction[] { remove_polymorph },
                                                                            reckless_stance.ActivateWithUnitAnimation);
                    buff.SetBuffFlags(BuffFlags.HiddenInUi | buff.GetBuffFlags());
                    dragon_form.Add(feature);
                }
            }


            static void createPowerOfTheWyrms()
            {
                foreach (var b in bloodlines)
                {
                    var feat = library.CopyAndAdd<BlueprintFeature>(b.power_of_the_wyrms.AssetGuid, b.prefix + "PowerOfTheWyrmsFeature", "");
                    feat.SetDescription(feat.Description + " You have this benefit constantly, even while not bloodraging.");
                    power_of_the_wyrms.Add(b.power_of_the_wyrms);
                }
            }
        }


        class ElementalBloodlines
        {
            public static List<BlueprintProgression> progressions = new List<BlueprintProgression>();
            static List<BlueprintFeature> elemental_strikes = new List<BlueprintFeature>(); //change duration to 1 round per 2 levels
            static List<BlueprintFeature> elemental_resistance = new List<BlueprintFeature>();
            static List<BlueprintFeature> elemental_movement = new List<BlueprintFeature>();
            static List<BlueprintFeature> power_of_the_elements = new List<BlueprintFeature>();//ignore resistance while using elemental strikes and raging on critical hit
            static List<BlueprintFeature> elemental_form = new List<BlueprintFeature>();
            static List<BlueprintFeature> elemental_body = new List<BlueprintFeature>();

            static string prefix = "BloodragerBloodlineElemental";

            struct ElementalBloodlineData
            {
                public UnityEngine.Sprite icon;
                public BlueprintFeature elemental_movement_prototype;
                public BlueprintBuff elemental_form_prototype;
                public DamageEnergyType energy_type;
                public BlueprintFeature elemental_body;
                public UnityEngine.Sprite resistance_icon;
                public BlueprintAbility spell1;
                public BlueprintAbility spell2;
                public string prefix;
                public string name;

                public string energy_string;

                public ElementalBloodlineData(UnityEngine.Sprite bloodline_icon, BlueprintFeature elemental_movement, BlueprintBuff elemental_form, DamageEnergyType energy,
                                      BlueprintAbility level1_spell, BlueprintAbility level2_spell,
                                      BlueprintFeature elemental_body_feature, string bloodline_name)
                {
                    icon = bloodline_icon;
                    elemental_movement_prototype = elemental_movement;
                    elemental_form_prototype = elemental_form;
                    elemental_body = elemental_body_feature;
                    spell1 = level1_spell;
                    spell2 = level2_spell;
                    energy_type = energy;
                    name = bloodline_name;

                    prefix = "BloodragerBloodlineElemental" + bloodline_name;
                    energy_string = energy.ToString().ToLower();

                    switch (energy)
                    {
                        case DamageEnergyType.Acid:
                            resistance_icon = library.Get<BlueprintAbility>("fedc77de9b7aad54ebcc43b4daf8decd").Icon;
                            break;
                        case DamageEnergyType.Cold:
                            resistance_icon = library.Get<BlueprintAbility>("5368cecec375e1845ae07f48cdc09dd1").Icon;
                            break;
                        case DamageEnergyType.Electricity:
                            resistance_icon = library.Get<BlueprintAbility>("90987584f54ab7a459c56c2d2f22cee2").Icon;
                            break;
                        default: //fire
                            resistance_icon = library.Get<BlueprintAbility>("ddfb4ac970225f34dbff98a10a4a8844").Icon;
                            break;
                    }
                }
            }
            static ElementalBloodlineData[] bloodlines;


            public static void create()
            {
                bloodlines = new ElementalBloodlineData[]
                {
                    new ElementalBloodlineData(library.Get<BlueprintProgression>("cd788df497c6f10439c7025e87864ee4").Icon, library.Get<BlueprintFeature>("1ae6835b8f568d44c8deb911f74762e4"),
                                               library.Get<BlueprintBuff>("ba06b8cff52da9e4d8432144ed6a6d19"), DamageEnergyType.Electricity,
                                               library.Get<BlueprintAbility>("728b3daffb1d9fd45958c6e60876b7a9"), library.Get<BlueprintAbility>("96ca3143601d6b242802655336620d91"),
                                               library.Get<BlueprintFeature>("7c3be22702ee39a418a5fba0e85e68de"), "Air"),
                    new ElementalBloodlineData(library.Get<BlueprintProgression>("32393034410fb2f4d9c8beaa5c8c8ab7").Icon, library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8"),
                                               library.Get<BlueprintBuff>("3c7c12df25d21b344b7cbe12a60038d8"), DamageEnergyType.Acid,
                                               library.Get<BlueprintAbility>("97d0a51ca60053047afb9aca900fb71b"), library.Get<BlueprintAbility>("435222be97067a447b2b40d3c58a058e"),
                                               library.Get<BlueprintFeature>("6541fc1423987a341b30ea68a54f0327"), "Earth"),
                    new ElementalBloodlineData(library.Get<BlueprintProgression>("17cc794d47408bc4986c55265475c06f").Icon, library.Get<BlueprintFeature>("f48c7d56a8a13af4d8e1cc9aae579b01"),
                                               library.Get<BlueprintBuff>("6be582eb1f6df4f41875c16d919e3b12"), DamageEnergyType.Fire,
                                               library.Get<BlueprintAbility>("4783c3709a74a794dbe7c8e7e0b1b038"), library.Get<BlueprintAbility>("cdb106d53c65bbc4086183d54c3b97c7"),
                                               library.Get<BlueprintFeature>("5d974328297021a479b4e3a1de749126"), "Fire"),
                    new ElementalBloodlineData(library.Get<BlueprintProgression>("7c692e90592257a4e901d12ae6ec1e41").Icon, library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8"),
                                               library.Get<BlueprintBuff>("f0abf98bb3bce4f4e877a8e8c2eccf41"), DamageEnergyType.Cold,
                                               library.Get<BlueprintAbility>("83ed16546af22bb43bd08734a8b51941"), library.Get<BlueprintAbility>("7ef096fdc8394e149a9e8dced7576fee"),
                                               library.Get<BlueprintFeature>("c459fcee6baabd149ac79acb0cb1d40e"), "Water")
                };

                createElementalStrikesAndPowerOfTheElements();
                createElementalResistance();
                createElementalMovement();
                createElementalForm();
                createElementalBody();

                var protection_from_energy = library.Get<BlueprintAbility>("d2f116cfe05fcdd4a94e80143b67046f");
                var elemental_body1 = library.Get<BlueprintAbility>("690c90a82bf2e58449c6b541cb8ea004");

                var dodge = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
                var cleave = library.Get<BlueprintFeature>("d809b6c4ff2aaff4fa70d712a70f7d7b");
                var improved_initiative = library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
                var great_fortitude = library.Get<BlueprintFeature>("79042cb55f030614ea29956177977c52");
                var power_attack = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
                var lightning_reflexes = library.Get<BlueprintFeature>("15e7da6645a7f3d41bdad7c8c4b9de1e");
                var weapon_focus = library.Get<BlueprintFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");

                for (int i = 0; i < bloodlines.Length; i++)
                {
                    var progression = createBloodragerBloodline("Elemental",
                                                                  "The power of the elements resides in you, and at times you can hardly control its fury.This influence comes either from an elemental outsider in your family history or from a moment when you or your ancestors were exposed to a powerful elemental force or cataclysm.\n"
                                                                   + "Cleave, Dodge, Great Fortitude, Improved Initiative, Lightning Reflexes, Power Attack, Weapon Focus.\n"
                                                                   + $"Bonus Spells: {bloodlines[i].spell1.Name} (7th), {bloodlines[i].spell2.Name.ToLower()} (10th), protection from energy (13th), elemental body I (16th).",
                                                                  bloodlines[i].icon,
                                                                  new BlueprintAbility[] { bloodlines[i].spell1, bloodlines[i].spell2, protection_from_energy, elemental_body1 },
                                                                  new BlueprintFeature[] { cleave, dodge, great_fortitude, improved_initiative, lightning_reflexes, power_attack, weapon_focus },
                                                                  new BlueprintFeature[] { elemental_strikes[i], elemental_resistance[i], elemental_movement[i], power_of_the_elements[i], elemental_form[i], elemental_body[i] },
                                                                  "",
                                                                  new string[] { "", "", "", "" },
                                                                  "",
                                                                  bloodlines[i].name).progression;
                    progressions.Add(progression);
                }
            }



            static void createElementalStrikesAndPowerOfTheElements()
            {
                var resource = Helpers.CreateAbilityResource(prefix + "ElementalStrikesResource", "", "", "", null);
                resource.SetIncreasedByLevelStartPlusDivStep(1, 8, 1, 8, 1, 0, 0.0f, getBloodragerArray());

                var divine_power = library.Get<BlueprintAbility>("ef16771cb05d1344989519e87f25b3c5"); //divine power
                var power_feat = Helpers.CreateFeature(prefix + "PowerOfTheElementsFeature",
                     "Power of the elements",
                     "At 12th level, the damage of your elemental strikes doubles.",
                     "",
                     divine_power.Icon,
                     FeatureGroup.None
                     );

                foreach (var b in bloodlines)
                {
                    //var bonus_damage = Helpers.CreateActionDealDamage(b.energy_type, Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageBonus)));
                    // var bonus_damage_action = Helpers.CreateActionList(bonus_damage);

                    var buff = Helpers.CreateBuff(b.prefix + "ElementalStrikesBuff",
                                                             "Elemental Strikes",
                                                             $"At 1st level, one time a day as a swift action you can imbue your melee attacks with elemental energy. For 1 round per 2 bloodrager levels (minimum 1), your melee attacks deal 1d6 points of {b.energy_string} damage. You gain an additional use of this ability at levels 8 and 16.",
                                                             "",
                                                             library.Get<BlueprintAbility>("9d5d2d3ffdd73c648af3eb3e585b1113").Icon, //divine favor
                                                             null,
                                                             Common.createAddWeaponEnergyDamageDiceBuff(Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageBonus)),
                                                                                                        b.energy_type,
                                                                                                        AttackType.Melee, AttackType.Touch)                                                 
                                                             //Common.createAddInitiatorAttackWithWeaponTrigger(bonus_damage_action, check_weapon_range_type: true,
                                                             //                                                 range_type: AttackTypeAttackBonus.WeaponRangeType.Melee)
                                                             );
                    var action = Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), dispellable: false));
                    var ability = Helpers.CreateAbility(b.prefix + "ElementalStrikesAbility",
                                                                   buff.Name,
                                                                   buff.Description,
                                                                   "",
                                                                   buff.Icon,
                                                                   AbilityType.Extraordinary,
                                                                   CommandType.Swift,
                                                                   AbilityRange.Personal,
                                                                   "1 round per 2 caster levels",
                                                                   Helpers.savingThrowNone,
                                                                   action,
                                                                   Helpers.CreateResourceLogic(resource),
                                                                   Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                                   progression: ContextRankProgression.Div2,
                                                                                                   min: 1,
                                                                                                   classes: getBloodragerArray(),
                                                                                                   type: AbilityRankType.StatBonus
                                                                                                   )
                                                                   );
                    ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Omni;
                    ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;
                    ability.Type = AbilityType.Supernatural;
                    addBloodrageRestriction(ability);
                    var feat = Helpers.CreateFeature(b.prefix + "ElementalStrikesFeature",
                                                     ability.Name,
                                                     ability.Description,
                                                     "",
                                                     ability.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.CreateAddFact(ability),
                                                     Helpers.CreateAddAbilityResource(resource)
                                                     );
                    elemental_strikes.Add(feat);
                    power_of_the_elements.Add(power_feat);
                    var rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, progression: ContextRankProgression.AsIs,
                                                                      type: AbilityRankType.DamageBonus, featureList: new BlueprintFeature[] { feat, power_feat });
                    buff.AddComponent(rank_config);
                }
            }


            static void createElementalResistance()
            {
                foreach (var b in bloodlines)
                {
                    var buff = Helpers.CreateBuff(b.prefix + "ElementalResistanceBuff",
                                                    "Elemental Resistance",
                                                    $"At 4th level, you gain {b.energy_string} resistance 10.",
                                                    "",
                                                    b.resistance_icon,
                                                    null,
                                                    Common.createEnergyDR(10, b.energy_type)
                                                  );
                    var feat = Helpers.CreateFeature(b.prefix + "ElementalResistanceFeature",
                                                     buff.Name,
                                                     buff.Description,
                                                     "",
                                                     buff.Icon,
                                                     FeatureGroup.None
                                                    );
                    elemental_resistance.Add(feat);
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, buff, feat);
                }
            }

            static void createElementalMovement()
            {
                foreach (var b in bloodlines)
                {
                    var buff = Helpers.CreateBuff(b.prefix + "ElementalMovementBuff",
                                                    "Elemental Movement",
                                                    b.elemental_movement_prototype.Description.Replace("15", "12"),
                                                    "",
                                                    b.elemental_movement_prototype.Icon,
                                                    null,
                                                    b.elemental_movement_prototype.ComponentsArray
                                                  );
                    var feat = Helpers.CreateFeature(b.prefix + "ElementalMovementFeature",
                                                     buff.Name,
                                                     buff.Description,
                                                     "",
                                                     buff.Icon,
                                                     FeatureGroup.None
                                                    );
                    elemental_movement.Add(feat);
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, buff, feat);
                }
            }


            static void createElementalForm()
            {
                var reckless_stance = library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");
                var remove_polymorph = Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Polymorph);
                foreach (var b in bloodlines)
                {
                    var buff = library.CopyAndAdd<BlueprintBuff>(b.elemental_form_prototype.AssetGuid, b.prefix + "ElementalFormBuff", "");
                    buff.SetName("Elemental Form");
                    buff.SetDescription("At 16th level, when entering a bloodrage, you can choose to take you can take an elemental form as elemental body IV.");
                    //remove turn back 
                    var polymorph = buff.GetComponent<Kingmaker.UnitLogic.Buffs.Polymorph>().CreateCopy();
                    polymorph.Facts = new BlueprintUnitFact[0];
                    buff.ReplaceComponent<Kingmaker.UnitLogic.Buffs.Polymorph>(polymorph);
                    var scaling = Helpers.Create<Kingmaker.UnitLogic.Mechanics.Components.ContextCalculateAbilityParamsBasedOnClass>();
                    scaling.CharacterClass = bloodrager_class;
                    scaling.StatType = StatType.Constitution;
                    scaling.UseKineticistMainStat = false;
                    buff.AddComponent(scaling);

                    var feature = Common.createSwitchActivatableAbilityBuff(b.prefix + "ElementalForm", "", "", "",
                                                                            buff,
                                                                            bloodrage_buff,
                                                                            new Kingmaker.ElementsSystem.GameAction[] { remove_polymorph },
                                                                            reckless_stance.ActivateWithUnitAnimation);
                    buff.SetBuffFlags(BuffFlags.HiddenInUi | buff.GetBuffFlags());
                    elemental_form.Add(feature);
                }
            }


            static void createElementalBody()
            {
                foreach (var b in bloodlines)
                {
                    var feat = library.CopyAndAdd<BlueprintFeature>(b.elemental_body.AssetGuid, b.prefix + "ElementalBodyFeature", "");
                    feat.SetDescription(feat.Description + " You have this benefit constantly, even while not bloodraging.");
                    elemental_body.Add(feat);
                }
            }


        }


        static BloodlineInfo createBloodragerBloodline(string name, string description, UnityEngine.Sprite icon,
                                                    BlueprintAbility[] bonus_spells, BlueprintFeature[] bonus_feats, BlueprintFeature[] powers,
                                                    string feat_selection_guid, string[] spell_guids, string progression_guid, string name_ext = "")
        {
            //spells at 7, 10, 13, 16
            //powers at 1, 4, 8, 12, 16, 20
            //feats at levels  6, 9, 12, 15, 18
            var progression = Helpers.CreateProgression("Bloodrager" + name + name_ext + "BloodlineProgression",
                                            name + " Bloodline" + ((name_ext == "") ? "" : (" — " + name_ext)),
                                            description,
                                            progression_guid,
                                            icon,
                                            FeatureGroup.BloodLine);


            var feat_selection = Helpers.CreateFeatureSelection("Bloodrager" + name + name_ext + "BloodlineBonusFeatSelection",
                                                                "Bloodline Feat",
                                                                "At 6th level and every 3 levels thereafter, a bloodrager receives one bonus feat chosen from a list specific to each bloodline. The bloodrager must meet the prerequisites for these bonus feats.",
                                                                feat_selection_guid,
                                                                null,
                                                                FeatureGroup.None
                                                                );
            feat_selection.AllFeatures = bonus_feats;
            feat_selection.Features = bonus_feats;

            bloodline_feat_selection.AllFeatures = bloodline_feat_selection.AllFeatures.AddToArray(feat_selection);
            bloodline_feat_selection.Features = bloodline_feat_selection.AllFeatures;


            BlueprintFeature[] bloodline_spells = new BlueprintFeature[bonus_spells.Length];

            for (int i = 0; i < bloodline_spells.Length; i++)
            {
                bloodline_spells[i] = Helpers.CreateFeature("Bloodrager" + name + name_ext + "BloodlineBonusSpell" + (i + 1).ToString(),
                                                            bonus_spells[i].Name,
                                                            "At 7th, 10th, 13th, and 16th levels, a bloodrager learns an additional spell derived from his bloodline.\n"
                                                            + bonus_spells[i].Description,
                                                            spell_guids[i],
                                                            bonus_spells[i].Icon,
                                                            FeatureGroup.None,
                                                            Helpers.CreateAddKnownSpell(bonus_spells[i], bloodrager_class, i + 1)
                                                            );
                bonus_spells[i].AddRecommendNoFeature(progression);
            }


            progression.Classes = getBloodragerArray();
            progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, powers[0]),
                                                         Helpers.LevelEntry(4, powers[1]),
                                                         Helpers.LevelEntry(7, bloodline_spells[0]),
                                                         Helpers.LevelEntry(8, powers[2]),
                                                         Helpers.LevelEntry(10, bloodline_spells[1]),
                                                         Helpers.LevelEntry(12, powers[3]),
                                                         Helpers.LevelEntry(13, bloodline_spells[2]),
                                                         Helpers.LevelEntry(16, powers[4], bloodline_spells[3]),
                                                         Helpers.LevelEntry(20, powers[5])
                                                        };
            progression.UIGroups = new UIGroup[] {Helpers.CreateUIGroup(powers),
                                                  Helpers.CreateUIGroup(bloodline_spells),
                                                  Helpers.CreateUIGroup(feat_selection, feat_selection, feat_selection, feat_selection, feat_selection)
                                                 };

            progression.AddComponent(Helpers.PrerequisiteClassLevel(bloodrager_class, 1, any: true)); //require level 1 bloodrager to not allow dd to pick bloodrager lines
            var bloodline_info = new BloodlineInfo(progression, feat_selection);
            bloodlines.Add(name + name_ext, bloodline_info);

            feat_selection.AddComponent(Helpers.PrerequisiteFeature(progression));

            return bloodline_info;
        }


        static void createRageCastingFeat()
        {
            //var contagion = library.Get<BlueprintAbility>("48e2744846ed04b4580be1a3343a5d3d"); //contagion
            var rage_casting = Helpers.Create<NewMechanics.RageCasting>();
            rage_casting.BonusDC = 4;
            var buff = Helpers.CreateBuff("RageCastingFeatBuff",
                                          "Rage Casting",
                                          "When you cast a bloodrager spell, as a swift action you can sacrifice some of your life force to augment the spell’s potency. You can opt to take 1d6 points of damage per spell level of the spell you are casting. You cannot overcome this damage in any way, and it cannot be taken from temporary hit points. For each of these damage dice you roll, the DC of the spell you are casting increases by 1.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Casting_Rage.png"),//contagion.Icon,
                                          null,
                                          rage_casting
                                          );
            var ability = Helpers.CreateActivatableAbility("RageCastingAFeatActivatableAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.WithUnitCommand,
                                                           test_mode ? CommandType.Free : CommandType.Swift,
                                                           null,
                                                           Common.createActivatableAbilityUnitCommand(CommandType.Swift)
                                                           );

            var feat = Helpers.CreateFeature("RageCastingFeat",
                                             ability.Name,
                                             ability.Description,
                                             "",
                                             buff.Icon,
                                             FeatureGroup.Feat,
                                             Helpers.CreateAddFact(ability),
                                             Helpers.PrerequisiteClassLevel(bloodrager_class, 4)
                                             );
            library.AddFeats(feat);
        }



        static void createMetarager()
        {
            metamagic_rager_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MetamagicRagerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Metamagic Rager");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While metamagic is difficult for many bloodragers to utilize, a talented few are able to channel their bloodrage in ways that push their spells to impressive ends.");
            });
            Helpers.SetField(metamagic_rager_archetype, "m_ParentClass", bloodrager_class);
            library.AddAsset(metamagic_rager_archetype, "");
            metamagic_rager_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(5, improved_uncanny_dodge) };
            createMetarage();
            metamagic_rager_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(5, metarage) };
            bloodrager_progression.UIGroups[1].Features.Add(metarage);
        }


        static void createMetarage()
        {
            BlueprintFeature[] metamagics = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(b => b.Groups.Contains(FeatureGroup.WizardFeat) && (b.GetComponent<AddMetamagicFeat>() != null) && b.AssetGuid != "2f5d1e705c7967546b72ad8218ccf99c").ToArray();

            BlueprintFeature[] metarager_metamagics = new BlueprintFeature[metamagics.Length];
            for (int i = 0; i < metamagics.Length; i++)
            {
                metarager_metamagics[i] = Helpers.CreateFeature("Metarager" + metamagics[i].name,
                                                                metamagics[i].Name,
                                                                metamagics[i].Description,
                                                                "",
                                                                metamagics[i].Icon,
                                                                FeatureGroup.None,
                                                                Helpers.CreateAddFact(metamagics[i]),
                                                                Helpers.PrerequisiteNoFeature(metamagics[i]),
                                                                Common.createPrerequisiteArchetypeLevel(bloodrager_class, metamagic_rager_archetype, 5)
                                                                );
                metarager_metamagics[i].HideInCharacterSheetAndLevelUp = true;
            }
            //add metamagic to feat selection
            foreach (var b in bloodlines.Values)
            {
                
                b.bonus_feats.AllFeatures = b.bonus_feats.AllFeatures.AddToArray(metarager_metamagics);
                b.bonus_feats.Features = b.bonus_feats.Features.AddToArray(metarager_metamagics);
            }

            foreach (var m in metamagics)
            {
                var metamagic = m.GetComponent<Kingmaker.UnitLogic.FactLogic.AddMetamagicFeat>().Metamagic;
                var mr = Helpers.Create<NewMechanics.MetamagicMechanics.MetaRage >();
                mr.Metamagic = metamagic;
                mr.resource = bloodrage_resource;
                var buff = Helpers.CreateBuff(m.name + "MetaRageBuff",
                                              "Meta - Rage: " + m.Name,
                                              m.Description,
                                              "",
                                              m.Icon,
                                              null,
                                              mr
                                              );
                var ability = Helpers.CreateActivatableAbility(m.name + "MetaRageAbility",
                                                               buff.Name,
                                                               buff.Description,
                                                               "",
                                                               buff.Icon,
                                                               buff,
                                                               AbilityActivationType.Immediately,
                                                               CommandType.Free,
                                                               null
                                                               );
                ability.Group = metarage_group;
                ability.WeightInGroup = 1;
                ability.DeactivateImmediately = true;
                var feat = Helpers.CreateFeature(m.name + "MetaRageFeat",
                                                 ability.Name,
                                                 ability.Description,
                                                 "",
                                                 ability.Icon,
                                                 FeatureGroup.Feat,
                                                 Helpers.CreateAddFact(ability)
                                                 );
                feat.HideInUI = true;
                feat.HideInCharacterSheetAndLevelUp = true;

                m.AddComponent(Helpers.CreateAddFeatureOnClassLevel(feat, 5, getBloodragerArray(), new BlueprintArchetype[] { metamagic_rager_archetype}));
            }
            var shadow_evocation = library.Get<BlueprintAbility>("237427308e48c3341b3d532b9d3a001f");
            metarage = Helpers.CreateFeature("BloodragerMetaRagerMetaRageFeat",
                                     "Meta-Rage",
                                     "At 5th level, a metamagic rager can sacrifice additional rounds of bloodrage to apply a metamagic feat he knows to a bloodrager spell. This costs a number of rounds of bloodrage equal to twice what the spell’s adjusted level would normally be with the metamagic feat applied (minimum 2 rounds). The metamagic rager does not have to be bloodraging to use this ability. The metamagic effect is applied without increasing the level of the spell slot expended, though the casting time is increased as normal. The metamagic rager can apply only one metamagic feat he knows in this manner with each casting. Additionally, when the metamagic rager takes a bloodline feat, he can choose to take a metamagic feat instead.",
                                     "",
                                     shadow_evocation.Icon,
                                     FeatureGroup.None
                                     );
        }


        static void createSpellEater()
        {
            spelleater_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SpelleaterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Spelleater");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Where other bloodragers learn to avoid or shrug off minor damage of all sorts, spelleaters tap into the power of their bloodline in order to heal damage as it comes, and can even cannibalize their own magical energy to heal more damage and continue taking the fight to the enemy.");
            });
            Helpers.SetField(spelleater_archetype, "m_ParentClass", bloodrager_class);
            library.AddAsset(spelleater_archetype, "");
            spelleater_archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(2, uncanny_dodge),
                                                                          Helpers.LevelEntry(5, improved_uncanny_dodge),
                                                                          Helpers.LevelEntry(7, damage_reduction),
                                                                          Helpers.LevelEntry(10, damage_reduction),
                                                                          Helpers.LevelEntry(13, damage_reduction),
                                                                          Helpers.LevelEntry(16, damage_reduction),
                                                                          Helpers.LevelEntry(19, damage_reduction)
                                                                        };
            createBloodOfLife();
            createSpellEating();
            spelleater_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(2, blood_of_life),
                                                                  Helpers.LevelEntry(5, spell_eating),
                                                                  Helpers.LevelEntry(7, blood_of_life),
                                                                  Helpers.LevelEntry(10, blood_of_life),
                                                                  Helpers.LevelEntry(13, blood_of_life),
                                                                  Helpers.LevelEntry(16, blood_of_life),
                                                                  Helpers.LevelEntry(19, blood_of_life),
                                                                };
            bloodrager_progression.UIGroups[1].Features.Add(spell_eating);
            bloodrager_progression.UIGroups = bloodrager_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(blood_of_life, blood_of_life, blood_of_life, blood_of_life, blood_of_life, blood_of_life));
        }


        static void createBloodOfLife()
        {
            var healing_judgement = library.Get<BlueprintActivatableAbility>("00b6d36e31548dc4ab0ac9d15e64a980");
            var blood_of_life_buff = Helpers.CreateBuff("SpelleaterBloodOfLifeBuff",
                                                           "Blood of Life",
                                                           "A spelleater’s blood empowers him to slowly recover from his wounds. At 2nd level, while bloodraging a spelleater gains fast healing 1. At 7th level and every 3 levels thereafter, this increases by 1(to a maximum of fast healing 6 at 19th level). If the spelleater gains an increase to damage reduction from a bloodline, he is considered to have an effective damage reduction of 0, and the increase is added as a bonus to fast healing.",
                                                           "",
                                                           healing_judgement.Icon,
                                                           null
                                                           );

            blood_of_life  = Helpers.CreateFeature("SpelleaterBloodOfLifeFeature",
                                                            blood_of_life_buff.Name,
                                                            blood_of_life_buff.Description,
                                                            "",
                                                            healing_judgement.Icon, //healing judgement
                                                            FeatureGroup.None
                                                          );
            blood_of_life.Ranks = 20;

            blood_of_life_buff.AddComponent(Common.createAddContextEffectFastHealing(Helpers.CreateContextValue(AbilityRankType.Default)));
            blood_of_life_buff.AddComponent(Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, type: AbilityRankType.Default,
                                                                            featureList: new BlueprintFeature[] {blood_of_life, AberrantBloodline.aberrant_form,
                                                                                                                 UndeadBloodline.one_foot_in_the_grave,
                                                                                                                 UndeadBloodline.one_foot_in_the_grave,
                                                                                                                 UndeadBloodline.one_foot_in_the_grave
                                                                                                                 }
                                                                            )
                                           );
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(bloodrage_buff, blood_of_life_buff, blood_of_life);
        }


        static void createSpellEating()
        {
            var renewed_vigor = library.Get<BlueprintAbility>("5a25185dbf75a954580a1248dc694cfc"); //for icon
            BlueprintAbility[] spell_eating_spells = new BlueprintAbility[4];
            string[] roman_id = new string[4] { "I", "II", "III", "IV" };
            for (int i = 0; i < spell_eating_spells.Length; i++)
            {
                var dice_value = Helpers.CreateContextDiceValue(dice: BalanceFixes.getDamageDie(DiceType.D8), diceCount: Common.createSimpleContextValue(i + 1));
                var spell = Helpers.CreateAbility("SpellEating" + (i + 1).ToString() + "Ability",
                                                  "Spell Eating " + roman_id[i],
                                                  $"As a swift action, the spelleater can consume one unused bloodrager spell slot to heal {i + 1}d{BalanceFixes.getDamageDieString(DiceType.D8)} damage.",
                                                  "",
                                                  renewed_vigor.Icon,
                                                  AbilityType.Special,
                                                  CommandType.Swift,
                                                  AbilityRange.Personal,
                                                  "",
                                                  "",
                                                  renewed_vigor.GetComponent<AbilitySpawnFx>(),
                                                  renewed_vigor.GetComponent<AbilityUseOnRest>(),
                                                  renewed_vigor.GetComponent<SpellDescriptorComponent>(),
                                                  Helpers.CreateRunActions(Common.createContextActionHealTarget(dice_value))
                                                 );
                spell.AnimationStyle = renewed_vigor.AnimationStyle;
                spell.Animation = renewed_vigor.Animation;
                spell.CanTargetSelf = true;
                spell_eating_spells[i] = spell;

            }
            spell_eating = library.CopyAndAdd<BlueprintFeature>("5e4620cea099c9345a9207c11d7bc916", "SpellEaterSpellEating", "");
            spell_eating.SetName("Spell Eating");
            spell_eating.SetIcon(renewed_vigor.Icon);
            spell_eating.SetDescription("At 5th level, a spelleater can consume spell slots for an extra dose of healing. As a swift action, the spelleater can consume one unused bloodrager spell slot to heal 1d8 damage for each level of the spell slot consumed.");
            spell_eating.ReplaceComponent<Kingmaker.UnitLogic.FactLogic.SpontaneousSpellConversion>(Common.createSpontaneousSpellConversion(bloodrager_class,
                                                                                                                                                        null,
                                                                                                                                                        spell_eating_spells[0],
                                                                                                                                                        spell_eating_spells[1],
                                                                                                                                                        spell_eating_spells[2],
                                                                                                                                                        spell_eating_spells[3],
                                                                                                                                                        null,
                                                                                                                                                        null,
                                                                                                                                                        null,
                                                                                                                                                        null,
                                                                                                                                                        null
                                                                                                                                                        )
                                                                                                                 );
        }


        static void createSteelblood()
        {
            steelblood_archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SteelbloodArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Steelblood");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Most bloodragers prefer light armor, but some learn the secret of using heavy armors. These steelbloods plod around the battlefield inspiring fear and delivering carnage from within a steel shell.");
            });
            Helpers.SetField(steelblood_archetype, "m_ParentClass", bloodrager_class);
            library.AddAsset(steelblood_archetype, "");
            steelblood_archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, fast_movement),
                                                                          Helpers.LevelEntry(2, uncanny_dodge),
                                                                          Helpers.LevelEntry(5, improved_uncanny_dodge),
                                                                          Helpers.LevelEntry(7, damage_reduction),
                                                                          Helpers.LevelEntry(10, damage_reduction),
                                                                          Helpers.LevelEntry(13, damage_reduction),
                                                                          Helpers.LevelEntry(16, damage_reduction),
                                                                          Helpers.LevelEntry(19, damage_reduction)
                                                                        };
            createSteelbloodProficiencies();
            var indomitable_stance = library.CopyAndAdd<BlueprintFeature>("74c59090138e28f4687c8a3400030763", "SteelbloodIndomitableStance","");
            indomitable_stance.SetDescription("At 1st level, a steelblood gains a +1 bonus on combat maneuver checks, to CMD against overrun combat maneuvers, and on Reflex saving throws against trample attacks. He also gains a +1 bonus to his AC against charge attacks and on attack and damage rolls against charging creatures.");

            var armored_swiftness = library.CopyAndAdd<BlueprintFeature>("f95f4f3a10917114c82bcbebc4d0fd36", "SteelbloodArmoredSwiftness", "");
            armored_swiftness.SetDescription("At 2nd level, a steelblood moves faster in medium and heavy armor. When wearing medium or heavy armor, a steelblood can move 5 feet faster than normal in that armor, to a maximum of his unencumbered speed.");

            armor_training = library.CopyAndAdd<BlueprintFeature>("3c380607706f209499d951b29d3c44f3", "SteelbloodArmorTraining", "");
            //armor_training.RemoveComponent(armor_training.GetComponent<Kingmaker.Designers.Mechanics.Facts.ArmorSpeedPenaltyRemoval>());
            armor_training.SetDescription("At 5th level, a steelblood learns to be more maneuverable while wearing armor. Whenever he is wearing armor, he reduces the armor check penalty by 1 (to a maximum of 0) and increases the maximum Dexterity bonus allowed by his armor by 1. Every 4 levels thereafter (9th, 13th, and 17th), these bonuses increase by 1, to a maximum 4-point reduction of the armor check penalty and a +4 increase of the maximum Dexterity bonus. This ability stacks with the fighter class feature of the same name.");

            createBloodDeflection();
            steelblood_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, steelblood_proficiencies),
                                                                  Helpers.LevelEntry(2, armored_swiftness),
                                                                  Helpers.LevelEntry(5, armor_training),
                                                                  Helpers.LevelEntry(7, blood_deflection),
                                                                  Helpers.LevelEntry(9, armor_training),
                                                                  Helpers.LevelEntry(13, armor_training),
                                                                  Helpers.LevelEntry(17, armor_training),
                                                                };
            bloodrager_progression.UIGroups[1].Features.Add(armored_swiftness);
            bloodrager_progression.UIDeterminatorsGroup = bloodrager_progression.UIDeterminatorsGroup.AddToArray(steelblood_proficiencies);
            bloodrager_progression.UIGroups[1].Features.Add(blood_deflection);
            bloodrager_progression.UIGroups = bloodrager_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(armor_training, armor_training, armor_training, armor_training));
        }


        static internal void createSteelbloodProficiencies()
        {
            steelblood_proficiencies = Helpers.CreateFeature("BloodragerSteelBloodProficiencies",
                                                             "Steelblood Proficiencies",
                                                             "A steelblood gains proficiency in heavy armor. A steelblood can cast bloodrager spells while wearing heavy armor without incurring an arcane spell failure chance. This replaces the bloodrager’s armor proficiency.",
                                                             "",
                                                             null,
                                                             FeatureGroup.None,
                                                             Common.createAddArmorProficiencies(ArmorProficiencyGroup.Heavy),
                                                             Common.createArcaneArmorProficiency(ArmorProficiencyGroup.Heavy)
                                                            );
        }


        static void createBloodDeflection()
        {
            blood_deflection_bonus = Helpers.CreateFeature("SteelbloodBloodDeflectionBonus",
                                                           "",
                                                           "",
                                                           "",
                                                           null,
                                                           FeatureGroup.None);
            blood_deflection_bonus.HideInUI = true;
            blood_deflection_bonus.HideInCharacterSheetAndLevelUp = true;
            blood_deflection_bonus.Ranks = 20;

            var shield_of_faith = library.Get<BlueprintAbility>("183d5bb91dea3a1489a6db6c9cb64445");

            BlueprintAbility[] blood_deflection_spells = new BlueprintAbility[4];
            string[] roman_id = new string[4] { "I", "II", "III", "IV" };
            int base_ac = 2;
            for (int i = 0; i < blood_deflection_spells.Length; i++)
            {
                var buff = Helpers.CreateBuff($"SteelbloodBloodDeflection{i + 1}Buff",
                                              "Blood Deflection " + roman_id[i],
                                              $"Steelblood can sacrifice a spell slot to gain a deflection bonus to AC equal to {1 + base_ac + i}.",
                                              "",
                                              shield_of_faith.Icon,
                                              null,
                                              Helpers.CreateAddStatBonus(StatType.AC, i + 3, ModifierDescriptor.Deflection)
                                              );

                var action = Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, 
                                                                                          Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), 
                                                                                          is_from_spell: true,
                                                                                          dispellable: false
                                                                                          )
                                                     );

                var spell = Helpers.CreateAbility($"SteelbloodBloodDeflection{i + 1}Ability",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  shield_of_faith.Icon,
                                                  AbilityType.Supernatural,
                                                  CommandType.Standard,
                                                  AbilityRange.Personal,
                                                  "Variable",
                                                  Helpers.savingThrowNone,
                                                  action,
                                                  shield_of_faith.GetComponent<AbilitySpawnFx>(),
                                                  Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureListRanks, progression: ContextRankProgression.AsIs,
                                                                                  type: AbilityRankType.Default, 
                                                                                  featureList: new BlueprintFeature[] { blood_deflection_bonus, AberrantBloodline.aberrant_form,
                                                                                                                        UndeadBloodline.one_foot_in_the_grave,
                                                                                                                        UndeadBloodline.one_foot_in_the_grave,
                                                                                                                        UndeadBloodline.one_foot_in_the_grave
                                                                                                                      }
                                                                                  )
                                                  );
                spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Self;
                spell.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSelf;
                blood_deflection_spells[i] = spell;
            }
            blood_deflection = library.CopyAndAdd<BlueprintFeature>("5e4620cea099c9345a9207c11d7bc916", "SteelbloodBloodDeflection", "");
            blood_deflection.SetName("Blood Deflection");
            blood_deflection.SetIcon(shield_of_faith.Icon);
            blood_deflection.SetDescription($"At 7th level, as a standard action a steelblood can sacrifice a bloodrager spell slot to gain a deflection bonus to AC equal to {base_ac} + the level of the spell sacrificed. The deflection bonus lasts one minute. Duration of the effect is increased by 1 minute at level 10 and every 3 levels thereafter. If the steelblood gains an increase to damage reduction from a bloodline he is considered to have an effective damage reduction of 0, and the increase is added to the duration of the effect.");
            blood_deflection.ReplaceComponent<Kingmaker.UnitLogic.FactLogic.SpontaneousSpellConversion>(Common.createSpontaneousSpellConversion(bloodrager_class,
                                                                                                                                                        null,
                                                                                                                                                        blood_deflection_spells[0],
                                                                                                                                                        blood_deflection_spells[1],
                                                                                                                                                        blood_deflection_spells[2],
                                                                                                                                                        blood_deflection_spells[3],
                                                                                                                                                        null,
                                                                                                                                                        null,
                                                                                                                                                        null,
                                                                                                                                                        null,
                                                                                                                                                        null
                                                                                                                                                        )
                                                                                                        );
            blood_deflection.AddComponent(Helpers.CreateAddFeatureOnClassLevel(blood_deflection_bonus,  7, getBloodragerArray(), new BlueprintArchetype[] { steelblood_archetype }));
            blood_deflection.AddComponent(Helpers.CreateAddFeatureOnClassLevel(blood_deflection_bonus, 10, getBloodragerArray(), new BlueprintArchetype[] { steelblood_archetype }));
            blood_deflection.AddComponent(Helpers.CreateAddFeatureOnClassLevel(blood_deflection_bonus, 13, getBloodragerArray(), new BlueprintArchetype[] { steelblood_archetype }));
            blood_deflection.AddComponent(Helpers.CreateAddFeatureOnClassLevel(blood_deflection_bonus, 16, getBloodragerArray(), new BlueprintArchetype[] { steelblood_archetype }));
            blood_deflection.AddComponent(Helpers.CreateAddFeatureOnClassLevel(blood_deflection_bonus, 19, getBloodragerArray(), new BlueprintArchetype[] { steelblood_archetype }));
        }


        static void addBloodrageRestriction(BlueprintAbility ability)
        {
            ability.AddComponent(Common.createAbilityCasterHasFacts(bloodrage_buff));
        }


        static void addBloodrageRestriction(BlueprintActivatableAbility ability)
        {
            ability.AddComponent(Common.createActivatableAbilityRestrictionHasFact(bloodrage_buff));
        }


        static void createUrbanBloodrager()
        {
            urban_bloodrager = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "UrbanBloodragerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Urban Bloodrager");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The urban bloodrager has learned to control her rage in so-called polite society. Though she lacks the untamed resilience of her wilder fellows, she’s an expert at keeping her rage from causing collateral damage in crowds.");
            });
            Helpers.SetField(urban_bloodrager, "m_ParentClass", bloodrager_class);
            library.AddAsset(urban_bloodrager, "");
            urban_bloodrager.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, bloodrage, bloodrager_proficiencies),
                                                                Helpers.LevelEntry(3, blood_sanctuary),
                                                                Helpers.LevelEntry(7, damage_reduction),
                                                                Helpers.LevelEntry(10, damage_reduction),
                                                                Helpers.LevelEntry(13, damage_reduction),
                                                                Helpers.LevelEntry(16, damage_reduction),
                                                                Helpers.LevelEntry(19, damage_reduction)
                                                               };
            createUrbanBloodragerProficiencies();
            createUrbanBloodrage();
            createRestrainedMagic();
            createAdoptedMagic();

            urban_bloodrager.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, urban_bloodrage, urban_bloodrager_proficiencies),
                                                                  Helpers.LevelEntry(3, restrained_magic),
                                                                  Helpers.LevelEntry(7, adopted_magic),
                                                                  Helpers.LevelEntry(10, adopted_magic),
                                                                  Helpers.LevelEntry(13, adopted_magic),
                                                                  Helpers.LevelEntry(16, adopted_magic),
                                                                  Helpers.LevelEntry(19, adopted_magic)
                                                            };

            urban_bloodrager.ReplaceClassSkills = true;
            urban_bloodrager.ClassSkills = bloodrager_class.ClassSkills = new StatType[] {StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillMobility, StatType.SkillPerception, StatType.SkillAthletics,
                                                      StatType.SkillPersuasion};

            bloodrager_progression.UIGroups[0].Features.Add(urban_bloodrage);
            bloodrager_progression.UIGroups[1].Features.Add(restrained_magic);
            bloodrager_progression.UIDeterminatorsGroup = bloodrager_progression.UIDeterminatorsGroup.AddToArray(urban_bloodrager_proficiencies);
            bloodrager_progression.UIGroups = bloodrager_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(adopted_magic, adopted_magic, adopted_magic, adopted_magic, adopted_magic));
        }


        static void createUrbanBloodragerProficiencies()
        {
            urban_bloodrager_proficiencies = library.CopyAndAdd(bloodrager_proficiencies, "UrbanBloodragerProficiencies", "");
            urban_bloodrager_proficiencies.ReplaceComponent<AddFacts>(a => a.Facts = a.Facts.Take(a.Facts.Length - 1).ToArray());
            urban_bloodrager_proficiencies.ReplaceComponent<ArcaneArmorProficiency>(Common.createArcaneArmorProficiency(
                                                                                                                        Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Light,
                                                                                                                        Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup.Medium
                                                                                                                        )
                                                                                   );

            urban_bloodrager_proficiencies.SetNameDescriptionIcon("Urban Bloodrager Proficiencies",
                                                                  "An urban bloodrager isn’t proficient with shields.",
                                                                  Helpers.GetIcon("8c971173613282844888dc20d572cfc9")
                                                                  );
        }


        static void createUrbanBloodrage()
        {
            urban_bloodrage = Helpers.CreateFeature("UrbanBloodragerControlledBloodrageFeature",
                                                    "Controlled Bloodrage",
                                                    "When an urban bloodrager rages, she does not gain the normal benefits.Instead, she can apply a + 4 morale bonus to her Constitution, Dexterity, or Strength.This bonus increases to + 6 when she gains greater bloodrage and to + 8 when she gains mighty bloodrage.When using a controlled bloodrage, an urban bloodrager gains no bonus on Will saves, takes no penalties to AC, and can still use Charisma -, Dexterity -, and Intelligence-based skills. A controlled bloodrage still counts as a bloodrage for the purposes of any spells, feats, and other effects.",
                                                    "",
                                                    Helpers.GetIcon("c7773d1b408fea24dbbb0f7bf3eb864e"),
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddFact(library.Get<BlueprintUnitFact>("4b1f3dd0f61946249a654941fc417a89"))
                                                    );


            var stats = new StatType[] { StatType.Strength, StatType.Dexterity, StatType.Constitution };
            var icons = new UnityEngine.Sprite[]
            {
                Helpers.GetIcon("c7773d1b408fea24dbbb0f7bf3eb864e"),
                Helpers.GetIcon("3553bda4d6dfe6344ad89b25f7be939a"),
                Helpers.GetIcon("99cf556b967c2074ca284e127d815711"),
            };
            
            for (int i = 0; i < stats.Length; i++)
            {
                urban_bloodrage_buffs[i] = library.CopyAndAdd(bloodrage_buff, "UrbanBloodragerControlledRage" + stats[i].ToString() + "Buff", "");
                urban_bloodrage_buffs[i].SetNameDescriptionIcon("Controlled Bloodrage: " + stats[i].ToString(),
                                                                urban_bloodrage.Description,
                                                                icons[i]
                                                                );

                urban_bloodrage_buffs[i].RemoveComponents<AttackTypeAttackBonus>();
                urban_bloodrage_buffs[i].RemoveComponents<WeaponGroupDamageBonus>();
                urban_bloodrage_buffs[i].RemoveComponents<WeaponAttackTypeDamageBonus>();
                urban_bloodrage_buffs[i].RemoveComponents<SpellDescriptorComponent>();
                urban_bloodrage_buffs[i].RemoveComponents<AddContextStatBonus>();
                urban_bloodrage_buffs[i].RemoveComponents<TemporaryHitPointsPerLevel>();
                urban_bloodrage_buffs[i].AddComponent(Helpers.CreateAddContextStatBonus(stats[i], ModifierDescriptor.Morale, rankType: AbilityRankType.StatBonus, multiplier: 2));

                var activatable_ability = library.CopyAndAdd(bloodrage_ability, "UrbanBloodragerControlledRage" + stats[i].ToString() + "Ability", "");
                activatable_ability.Buff = urban_bloodrage_buffs[i];
                activatable_ability.SetNameDescriptionIcon(urban_bloodrage_buffs[i]);
                urban_bloodrage.GetComponent<AddFacts>().Facts = urban_bloodrage.GetComponent<AddFacts>().Facts.AddToArray(activatable_ability);
                bloodrage_buff.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = urban_bloodrage_buffs[i]));
                NewRagePowers.rage_buff.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = urban_bloodrage_buffs[i]));
            }


            var extra_rage = library.Get<BlueprintFeature>("1a54bbbafab728348a015cf9ffcf50a7");
            extra_rage.ReplaceComponent<PrerequisiteFeature>(a => a.Group = Prerequisite.GroupType.Any);
            extra_rage.AddComponent(Helpers.PrerequisiteFeature(urban_bloodrage, any: true));
        }


        static void createRestrainedMagic()
        {
            var buff = Helpers.CreateBuff("UrbanBloodragerRestrainedMagicBuff",
                                          "Restrained Magic",
                                          "At 3rd level, an urban bloodrager can attune her spells so they are less likely to impact her allies or innocent bystanders. When the bloodrager casts a spell, she can grant a +2 bonus on the saving throw against that spell to any creatures she is aware of that are targeted by the spell or within the spell’s area.",
                                          "",
                                          Helpers.GetIcon("76a629d019275b94184a1a8733cac45e"),
                                          null,
                                          Helpers.Create<NewMechanics.SavingThrowBonusAgainstCaster>(s => { s.Value = 2; s.Descriptor = ModifierDescriptor.UntypedStackable; })
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);
            restrained_magic = Common.createAuraEffectFeature(buff.Name,
                                                             buff.Description,
                                                             buff.Icon,
                                                             buff,
                                                             60.Feet(),
                                                             Helpers.CreateConditionsCheckerOr(Helpers.Create<ContextConditionIsEnemy>(c => c.Not = true))
                                                             );
        }


        static void createAdoptedMagic()
        {
            var bard_spell_list = library.Get<BlueprintSpellList>("25a5013493bdcf74bb2424532214d0c8");
            var magus_spell_list = library.Get<BlueprintSpellList>("4d72e1e7bd6bc4f4caaea7aa43a14639");
            var combined_spell_list = Common.combineSpellLists("AdoptedMagicSpellList", bard_spell_list, magus_spell_list);
            Common.excludeSpellsFromList(combined_spell_list, bloodrager_class.Spellbook.SpellList);

            adopted_magic = Helpers.CreateFeatureSelection("AdoptedMagicFeatureSelection",
                                                            "Adopted Magic",
                                                            "At 7th level, an urban bloodrager learns some of the secrets of other magical traditions from other denizens of the city. She can select from the bard or magus spell list any spell of a level she can cast, and add it to her bloodrager spell list and to her bloodrager spells known.\n"
                                                            + "At 10th level and every 3 levels thereafter, the urban bloodrager can add another such spell to her spell list and spells known.",
                                                            "",
                                                            Helpers.GetIcon("55edf82380a1c8540af6c6037d34f322"),
                                                            FeatureGroup.None);
            for (int i = 1; i <= 4; i++)
            {
                var learn_spell = library.CopyAndAdd<BlueprintParametrizedFeature>("bcd757ac2aeef3c49b77e5af4e510956",  $"AdoptedMagic{i}ParametrizedFeature", "");
                learn_spell.SpellLevel = i;
                learn_spell.SpecificSpellLevel = true;
                learn_spell.SpellLevelPenalty = 0;
                learn_spell.SpellcasterClass = bloodrager_class;
                learn_spell.SpellList = combined_spell_list;
                learn_spell.ReplaceComponent<LearnSpellParametrized>(l => { l.SpellList = combined_spell_list; l.SpecificSpellLevel = true; l.SpellLevel = i; l.SpellcasterClass = bloodrager_class; });
                learn_spell.AddComponents(Common.createPrerequisiteClassSpellLevel(bloodrager_class, i));
                learn_spell.SetName(Helpers.CreateString($"AdoptedMagic{i}ParametrizedFeature.Name", "Adopted Magic " + $"(Level {i})"));
                learn_spell.SetDescription(adopted_magic.Description);
                learn_spell.SetIcon(adopted_magic.Icon);

                adopted_magic.AllFeatures = adopted_magic.AllFeatures.AddToArray(learn_spell);
            }
        }


        static void createBloodConduit()
        {
            blood_conduit = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "BloodConduitBloodragerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Blood Conduit");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Blood conduits learn to channel their arcane might directly through their flesh, without the need for mystical words or gestures.");
            });
            Helpers.SetField(blood_conduit, "m_ParentClass", bloodrager_class);
            library.AddAsset(blood_conduit, "");
            blood_conduit.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, fast_movement),
                                                                Helpers.LevelEntry(2, uncanny_dodge),
                                                                Helpers.LevelEntry(5, improved_uncanny_dodge),
                                                                Helpers.LevelEntry(14, indomitable_will)
                                                               };
            createContactSpecialist();
            createSpellConduitAndReflexiveConduit();

            blood_conduit.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, contact_specialist),
                                                                  Helpers.LevelEntry(5, spell_conduit),
                                                                  Helpers.LevelEntry(14, reflexive_conduit)
                                                            };

            bloodrager_progression.UIGroups[1].Features.Add(contact_specialist);
            bloodrager_progression.UIGroups[1].Features.Add(spell_conduit);
            bloodrager_progression.UIGroups[1].Features.Add(reflexive_conduit);
        }


        static void createContactSpecialist()
        {
            contact_specialist = Helpers.CreateFeatureSelection("ContactSpecialistFeatureSelection",
                                                                "Contact Specialist",
                                                                "At 1st level, a blood conduit selects a bonus feat from the following: Improved Bull Rush, Improved Trip, and Improved Unarmed Strike. He does not need to meet the prerequisites to take this feat. He also adds those feats to his list of bloodline feats.",
                                                                "",
                                                                null,
                                                                FeatureGroup.None,
                                                                Common.createPrerequisiteArchetypeLevel(blood_conduit, 1));
            contact_specialist.IgnorePrerequisites = true;
            contact_specialist.AllFeatures = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("b3614622866fe7046b787a548bbd7f59"), //bull rush
                library.Get<BlueprintFeature>("0f15c6f70d8fb2b49aa6cc24239cc5fa"), //trip
                library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167"), //improved unarmed strike
            };
            bloodline_feat_selection.AllFeatures = bloodline_feat_selection.AllFeatures.AddToArray(contact_specialist);
        }


        static void createSpellConduitAndReflexiveConduit()
        {
            spell_conduit = Helpers.CreateFeature("SpellConduitFeature",
                                    "Spell Conduit",
                                    "At 5th level, a blood conduit can channel a bloodrager spell with a range of touch into his blood as a free action. As long as he is wearing light or no armor, he can deliver this spell through bodily contact as a swift action. When he succeeds at a trip or bull rush combat maneuver or an attack with a natural weapon or an unarmed strike against an enemy, he can release a touch spell on the creature.",
                                    "",
                                    Helpers.GetIcon("1d6364123e1f6a04c88313d83d3b70ee"), //strength surge
                                    FeatureGroup.None,
                                    Helpers.Create<SpellManipulationMechanics.FactStoreSpell>(f => f.always_hit = true));

            var release_buff = Helpers.CreateBuff("SpellConduitToggleBuff",
                                                  spell_conduit.Name + ": Release",
                                                  spell_conduit.Description,
                                                  "",
                                                  spell_conduit.Icon,
                                                  null,
                                                  Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = spell_conduit));

            var conduit_activatable_ability = Helpers.CreateActivatableAbility("SpellConduitToggleAbility",
                                                                             spell_conduit.Name + ": Release",
                                                                             spell_conduit.Description,
                                                                             "",
                                                                             spell_conduit.Icon,
                                                                             release_buff,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.Create<NewMechanics.ActivatableAbilityLightOrNoArmor>(),
                                                                             Helpers.Create<SpellManipulationMechanics.ActivatableAbilitySpellStoredInFactRestriction>(a => a.fact = spell_conduit));
            conduit_activatable_ability.DeactivateImmediately = true;

            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = spell_conduit);
            var release_on_condition = Helpers.CreateConditional(new Condition[]{Common.createContextConditionCasterHasFact(release_buff),
                                                                                 Helpers.Create<TurnActionMechanics.ContextConditionHasAction>(c => {c.has_swift = true; c.check_caster = true; })
                                                                                },
                                                                 new GameAction[]{release_action,
                                                                                  Helpers.Create<TurnActionMechanics.ConsumeAction>(c => {c.consume_swift = true; c.from_caster = true; })
                                                                                 }
                                                                 );
            var on_attack_action = Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(release_on_condition));
            var on_maneuver_action = Helpers.Create<CombatManeuverMechanics.AddInitiatorManeuverWithWeaponTrigger>(a =>
            {
                a.ActionsOnInitiator = false;
                a.Action = Helpers.CreateActionList(release_on_condition);
                a.AllNaturalAndUnarmed = true;
                a.Maneuvers = new Kingmaker.RuleSystem.Rules.CombatManeuver[] { Kingmaker.RuleSystem.Rules.CombatManeuver.BullRush, Kingmaker.RuleSystem.Rules.CombatManeuver.Trip };
                a.OnlySuccess = true;
            });
            on_attack_action.AllNaturalAndUnarmed = true;
            spell_conduit.AddComponent(on_attack_action);
            spell_conduit.AddComponent(on_maneuver_action);
            spell_conduit.AddComponent(Helpers.CreateAddFact(conduit_activatable_ability));

            int max_variants = 10; //due to ui limitation
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return spell.Spellbook?.Blueprint == bloodrager_class.Spellbook
                        && spell.Blueprint.StickyTouch != null
                        && spell.Blueprint.CanTargetEnemies;
            };

            for (int i = 0; i < max_variants; i++)
            {
                var ability = Helpers.CreateAbility($"SpellConduit{i + 1}Ability",
                                                        spell_conduit.Name,
                                                        spell_conduit.Description,
                                                        "",
                                                        spell_conduit.Icon,
                                                        AbilityType.Supernatural,
                                                        CommandType.Free,
                                                        AbilityRange.Personal,
                                                        "",
                                                        "",
                                                        Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s => { s.fact = spell_conduit; s.check_slot_predicate = check_slot_predicate; s.variant = i; })
                                                        );
                ability.setMiscAbilityParametersSelfOnly();
                spell_conduit.AddComponent(Helpers.CreateAddFact(ability));
            }

            reflexive_conduit = Helpers.CreateFeature("ReflexiveConduitFeature",
                                                      "Reflexive Conduit",
                                                      "At 14th level, a blood conduit can discharge his power into foes that attempt bodily contact with him. While wearing light or no armor, when the blood conduit is subject to a natural weapon or unarmed attack he can release a spell channeled into his blood using spell conduit ability on his attacker as a swift action.",
                                                      "",
                                                      Helpers.GetIcon("3dccdf27a8209af478ac71cded18a271"), //defensive stance
                                                      FeatureGroup.None);

            var release_buff2 = Helpers.CreateBuff("ReflexiveConduitToggleBuff",
                                                  reflexive_conduit.Name + ": Release",
                                                  reflexive_conduit.Description,
                                                  "",
                                                  reflexive_conduit.Icon,
                                                  null,
                                                  Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = spell_conduit));

            var conduit_activatable_ability2 = Helpers.CreateActivatableAbility("ReflexiveDodgeToggleAbility",
                                                                             reflexive_conduit.Name + ": Release",
                                                                             reflexive_conduit.Description,
                                                                             "",
                                                                             reflexive_conduit.Icon,
                                                                             release_buff2,
                                                                             AbilityActivationType.Immediately,
                                                                             CommandType.Free,
                                                                             null,
                                                                             Helpers.Create<NewMechanics.ActivatableAbilityLightOrNoArmor>(),
                                                                             Helpers.Create<SpellManipulationMechanics.ActivatableAbilitySpellStoredInFactRestriction>(a => a.fact = spell_conduit));
            conduit_activatable_ability2.DeactivateImmediately = true;

            var release_on_condition2 = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(release_buff2), release_action);
            var on_attacked_action = Common.createAddTargetAttackWithWeaponTrigger(action_attacker: Helpers.CreateActionList(release_on_condition),
                                                                                   categories: new WeaponCategory[] { WeaponCategory.UnarmedStrike, WeaponCategory.Claw, WeaponCategory.Bite, WeaponCategory.Gore, WeaponCategory.OtherNaturalWeapons },
                                                                                   wait_for_attack_to_resolve: true);
            reflexive_conduit.AddComponent(on_attacked_action);
            reflexive_conduit.AddComponent(Helpers.CreateAddFact(conduit_activatable_ability2));
        }
    }
}
