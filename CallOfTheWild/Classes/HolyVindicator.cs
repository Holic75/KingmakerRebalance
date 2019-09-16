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
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
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
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
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

namespace CallOfTheWild
{
    public class HolyVindicator
    {
        static LibraryScriptableObject library => Main.library;
        internal static bool test_mode = false;

        static public BlueprintCharacterClass holy_vindicator_class;
        static public BlueprintProgression holy_vindicator_progression;

        static public BlueprintFeature holy_vindicator_proficiencies;
        static public BlueprintFeatureSelection spellbook_selection;
        static public BlueprintFeature vindicator_shield;
        static public BlueprintFeature channel_energy_progression;
        static public BlueprintFeature stigmata;
        static public Dictionary<ModifierDescriptor, List<BlueprintActivatableAbility>> stigmata_abilities = new Dictionary<ModifierDescriptor, List<BlueprintActivatableAbility>>();
        static public Dictionary<ModifierDescriptor, List<BlueprintBuff>> stigmata_buffs = new Dictionary<ModifierDescriptor, List<BlueprintBuff>>();
        static public BlueprintFeature faith_healing;
        static public BlueprintFeature divine_wrath;
        static public BlueprintFeature divine_retribution;
        static public BlueprintFeature versatile_channel;
        static public BlueprintFeature bloodfire;
        static public BlueprintFeature bloodrain;



        internal static void createholy_vindicatorClass()
        {
            var paladin_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");

            var savesPrestigeLow = library.Get<BlueprintStatProgression>("dc5257e1100ad0d48b8f3b9798421c72");
            var savesPrestigeHigh = library.Get<BlueprintStatProgression>("1f309006cd2855e4e91a6c3707f3f700");

            holy_vindicator_class = Helpers.Create<BlueprintCharacterClass>();
            holy_vindicator_class.name = "HolyVindicatorClass";
            library.AddAsset(holy_vindicator_class, "");

            holy_vindicator_class.LocalizedName = Helpers.CreateString("HolyVindicator.Name", "Holy Vindicator");
            holy_vindicator_class.LocalizedDescription = Helpers.CreateString("Holy Vindicator",
                "Many faiths have within their membership an order of the church militant, be they holy knights or dark warriors, who put their lives and immortal souls on the line for their faith. They are paragons of battle, eschewing sermons for steel. These men and women are living conduits of divine power, down to their very blood, which they happily shed in a moment if it brings greater glory to their deity or judgment upon heretics, infidels, and all enemies of the faith. Holy vindicators are usually clerics or fighter/clerics, though many paladins (or even paladin/clerics) are drawn to this class as well. In all cases, the class offers a further opportunity to fuse and refine their martial and ministerial powers and role."
                + "The holy vindicator has substantial spellcasting ability, though not so much as a focused cleric or paladin.His combat skills are considerable and his healing powers prodigious, and those whose religious views align well with the vindicator will find a ready ally."
                );

            holy_vindicator_class.m_Icon = paladin_class.Icon;
            holy_vindicator_class.SkillPoints = paladin_class.SkillPoints;
            holy_vindicator_class.HitDie = DiceType.D10;
            holy_vindicator_class.BaseAttackBonus = paladin_class.BaseAttackBonus;
            holy_vindicator_class.FortitudeSave = savesPrestigeHigh;
            holy_vindicator_class.ReflexSave = savesPrestigeLow;
            holy_vindicator_class.WillSave = savesPrestigeHigh;
            holy_vindicator_class.ClassSkills = new StatType[] { StatType.SkillAthletics, StatType.SkillLoreReligion, StatType.SkillKnowledgeArcana, StatType.SkillPersuasion };
            holy_vindicator_class.IsDivineCaster = true;
            holy_vindicator_class.IsArcaneCaster = false;
            holy_vindicator_class.PrestigeClass = true;
            holy_vindicator_class.StartingGold = paladin_class.StartingGold;
            holy_vindicator_class.PrimaryColor = paladin_class.PrimaryColor;
            holy_vindicator_class.SecondaryColor = paladin_class.SecondaryColor;
            holy_vindicator_class.RecommendedAttributes = new StatType[] { StatType.Strength };
            holy_vindicator_class.EquipmentEntities = paladin_class.EquipmentEntities;
            holy_vindicator_class.MaleEquipmentEntities = paladin_class.MaleEquipmentEntities;
            holy_vindicator_class.FemaleEquipmentEntities = paladin_class.FemaleEquipmentEntities;
            holy_vindicator_class.StartingItems = paladin_class.StartingItems;

            holy_vindicator_class.ComponentsArray = paladin_class.ComponentsArray;
            holy_vindicator_class.AddComponent(Helpers.PrerequisiteStatValue(StatType.BaseAttackBonus, 5));
            holy_vindicator_class.AddComponent(Helpers.PrerequisiteStatValue(StatType.SkillLoreReligion, 5));
            holy_vindicator_class.AddComponent(Helpers.PrerequisiteFeature(ChannelEnergyEngine.improved_channel)); //instead of channel energy and Alignment/Elemental channel which are not in the game and imho do not worth implementing 
            holy_vindicator_class.AddComponent(Common.createPrerequisiteCasterTypeSpellLevel(false, 1));

            holy_vindicator_class.AddComponent(Helpers.Create<SkipLevelsForSpellProgression>(s => s.Levels = new int[] { 5, 9 }));

            createHolyVindicatorProgression();
            holy_vindicator_class.Progression = holy_vindicator_progression;

            Helpers.RegisterClass(holy_vindicator_class);
        }

        static void createHolyVindicatorProgression()
        {
            createHolyVindicatorProficiencies();
            createVindicatorShield();
            createChannelEnergyProgression();
            createSpellbookSelection();

            createStigmata();
            /*createFaithHealing();
            createDivineWrath();
            createBloodfire();
            createVersatileChannel();
            createBloodrain();
            createDivineRetribution();*/
        }


        static BlueprintCharacterClass[] getHolyVindicatorArray()
        {
            return new BlueprintCharacterClass[] { holy_vindicator_class };
        }


        static void createHolyVindicatorProficiencies()
        {
            holy_vindicator_proficiencies = library.CopyAndAdd<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb", "HolyVindicatorProficiencies", "");//paladin
            holy_vindicator_proficiencies.SetName("Holy Vindicator Proficiencies");
            holy_vindicator_proficiencies.SetDescription("A vindicator is proficient with all simple and martial weapons and all armor and shields (except tower shields).");
        }


        static void createSpellbookSelection()
        {
            spellbook_selection = Helpers.CreateFeatureSelection("HolyVindicatorSpellbookSelection",
                                                                 "Holy Vindicator Spellbook Selection",
                                                                 "At 2nd level, and at every level thereafter, with an exception for 5th and 9th levels, a vindicator gains new spells per day as if he had also gained a level in divine spellcasting class he belonged to before adding the prestige class. He does not, however, gain any other benefit a character of that class would have gained, except for additional spells per day, spells known (if he is a spontaneous spellcaster), and an increased effective level of spellcasting. If a character had more than one spellcasting class before becoming a hinterlandert, he must decide to which class he adds the new level for purposes of determining spells per day.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.EldritchKnightSpellbook);
            Common.addSpellbooksToSpellSelection("HolyVindicator", 1, spellbook_selection, arcane: false, alchemist: false);
        }


        static void createVindicatorShield()
        {
            ChannelEnergyEngine.createHolyVindicatorShield();
            vindicator_shield = ChannelEnergyEngine.holy_vindicator_shield;
        }


        static void createChannelEnergyProgression()
        {
            ChannelEnergyEngine.addHolyVindicatorChannelEnergyProgression();

            channel_energy_progression = Helpers.CreateFeature("ChannelEnergyHolyVindicatorProgression",
                                                               "Channel Energy",
                                                               "The vindicator’s class level stacks with levels in any other class that grants the channel energy ability.",
                                                               "",
                                                               null,
                                                               FeatureGroup.None);
        }


        static void createStigmata()
        {
            ModifierDescriptor[] bonus_descriptors = new ModifierDescriptor[] { ModifierDescriptor.Sacred, ModifierDescriptor.Profane };
            StatType[] stats = new StatType[] {StatType.AC, StatType.AdditionalAttackBonus, StatType.AdditionalDamage};

            var icon = library.Get<BlueprintAbility>("a6e59e74cba46a44093babf6aec250fc").Icon;//slay living

            foreach (var bonus_descriptor in bonus_descriptors)
            {
                var buffs = new List<BlueprintBuff>();
                var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: getHolyVindicatorArray(),
                                                             type: AbilityRankType.StatBonus, progression: ContextRankProgression.Div2);
                foreach (var s in stats)
                {
                    var buff = Helpers.CreateBuff(bonus_descriptor.ToString() + "Stigmata" + s.ToString() + "Buff",
                                                  $"{bonus_descriptor.ToString()} Stigmata: {s.ToString()}",
                                                  "A vindicator willingly gives his blood in service to his faith, and is marked by scarified wounds appropriate to his deity. At 2nd level, he may stop or start the flow of blood by force of will as a standard action; at 6th level it becomes a move action, and at 10th level it becomes a swift action. Activating stigmata causes holy or unholy damage equal to half the vindicator’s class level every round. While the stigmata are bleeding, the vindicator gains a sacred bonus (if he channels positive energy) or profane bonus (if he channels negative energy) equal to half his class level. Each time he activates his stigmata, the vindicator decides if the bonus applies to attack rolls, weapon damage rolls, Armor Class, or saving throws; to change what the bonus applies to, the vindicator must deactivate and reactivate his stigmata. While his stigmata are bleeding, the vindicator ignores blood drain and bleed damage from any other source.",
                                                  "",
                                                  icon,
                                                  null,
                                                  Helpers.CreateAddContextStatBonus(s, bonus_descriptor, rankType: AbilityRankType.StatBonus),
                                                  context_rank_config
                                                  );
                    buffs.Add(buff);
                }

                var saves_buff = Helpers.CreateBuff(bonus_descriptor.ToString() + "Stigmata" + "SavesBuff",
                                                    $"{bonus_descriptor.ToString()} Stigmata: Savingthrows",
                                                    buffs[0].Description,
                                                    "",
                                                    icon,
                                                    null,
                                                    Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, bonus_descriptor, rankType: AbilityRankType.StatBonus),
                                                    Helpers.CreateAddContextStatBonus(StatType.SaveReflex, bonus_descriptor, rankType: AbilityRankType.StatBonus),
                                                    Helpers.CreateAddContextStatBonus(StatType.SaveWill, bonus_descriptor, rankType: AbilityRankType.StatBonus),
                                                    context_rank_config
                                                    );
                buffs.Add(saves_buff);
                stigmata_buffs.Add(bonus_descriptor, buffs);

                stigmata_abilities.Add(bonus_descriptor, new List<BlueprintActivatableAbility>());
                foreach (var b in buffs)
                {
                    var ability = Helpers.CreateActivatableAbility(b.name.Replace("Buff", "ActivatableAbility"),
                                                                   b.Name,
                                                                   b.Description,
                                                                   "",
                                                                   icon,
                                                                   b,
                                                                   AbilityActivationType.Immediately,
                                                                   CommandType.Standard,
                                                                   null
                                                                   );

                    ability.Group = ActivatableAbilityGroupExtension.Stigmata.ToActivatableAbilityGroup();
                    stigmata_abilities[bonus_descriptor].Add(ability);
                }
            }
        }

    }
}
