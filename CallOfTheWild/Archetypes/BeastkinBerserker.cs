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
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
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

namespace CallOfTheWild.Archetypes
{
    public class BeastkinBerserker
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature savage_rapport;
        static public BlueprintFeature feral_transformation_wolf, feral_transformation_leopard, feral_transformation_bear, feral_transformation_dire_wolf, feral_transformation_mastodon, feral_transformation_smilodon;

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var barbarian_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "BeastkinBerserkerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Beastkin Berserker");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While some barbarians take on bestial aspects in their rages, the beastkin berserker descends so deeply into primal fury that she actually transforms into an animal. Berserkers of the surface world often associate with predators such as bears or wolves, and are sometimes mistaken for lycanthropes. Some barbarians shapechange into rampaging dinosaurs or megafauna, crafting their armor out of the hide and bones of their favored animals.");
            });
            Helpers.SetField(archetype, "m_ParentClass", barbarian_class);
            library.AddAsset(archetype, "");

            createSavageRapport();
            createFeralTransformation();

            var fast_movement = library.Get<BlueprintFeature>("d294a5dddd0120046aae7d4eb6cbc4fc");
            var rage_power = library.Get<BlueprintFeatureSelection>("28710502f46848d48b3f0d6132817c4e");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, fast_movement),
                                                          Helpers.LevelEntry(4, rage_power),
                                                          Helpers.LevelEntry(8, rage_power),
                                                          Helpers.LevelEntry(12, rage_power),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, savage_rapport),
                                                          Helpers.LevelEntry(4, feral_transformation_wolf, feral_transformation_leopard),
                                                          Helpers.LevelEntry(8, feral_transformation_bear, feral_transformation_dire_wolf),
                                                          Helpers.LevelEntry(12, feral_transformation_smilodon, feral_transformation_mastodon),
                                                       };

            barbarian_class.Progression.UIGroups = barbarian_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(feral_transformation_wolf, feral_transformation_dire_wolf, feral_transformation_mastodon));
            barbarian_class.Progression.UIGroups = barbarian_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(feral_transformation_leopard, feral_transformation_bear, feral_transformation_smilodon));
            barbarian_class.Archetypes = barbarian_class.Archetypes.AddToArray(archetype);
        }


        static void createSavageRapport()
        {
            savage_rapport = Helpers.CreateFeature("SavageRapportFeature",
                                                   "Savage Rapport",
                                                   "Beastkin berserker adds half her level to her Lore (Nature) skill.",
                                                   "",
                                                   Helpers.GetIcon("6507d2da389ed55448e0e1e5b871c013"),
                                                   FeatureGroup.None,
                                                   Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.UntypedStackable),
                                                   Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                                                   classes: new BlueprintCharacterClass[] { archetype.GetParentClass() })
                                                   );
            savage_rapport.ReapplyOnLevelUp = true;
        }


        static void createFeralTransformation()
        {
            var remove_polymorph = Common.createContextActionRemoveBuffsByDescriptor(SpellDescriptor.Polymorph);
            var rage_buff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            var spells = new BlueprintAbility[]
            {
                Wildshape.wolf_form_spell,
                Wildshape.leopard_form_spell,
                Wildshape.dire_wolf_form_spell,
                Wildshape.bear_form_spell,
                Wildshape.smilodon_form_spell,
                Wildshape.mastodon_form_spell
            };

            var names = new string[] { "Wolf", "Leopard", "Dire Wolf", "Bear", "Smilodon", "Mastodon" };

            var features = new List<BlueprintFeature>();

            var description = "At 4th level, when entering a rage, a beastkin berserker can take the form of a wolf or a leopard. This functions as beast shape I, except the duration is for as long as the beastkin berserker rages.\n"
                              + "At 8th level, the beastkin berserker can use feral transformation to take the form of a bear or dire wolf. Feral transformation now acts as beast shape II.\n"
                              + "At 12th level, a beastkin berserker is able to use feral transformation to take the form of a smilodon or a mastodon. Feral transformation now functions as beast shape III.";
            var turn_back = library.Get<BlueprintAbility>("bd09b025ee2a82f46afab922c4decca9");
            for (int i = 0; i <spells.Length; i++)
            {
                var spell_buff = (spells[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).Buff;
                var buff = library.CopyAndAdd(spell_buff, "BeastkinBerserker" + spell_buff.name, "");
                buff.SetNameDescription($"Feral Transformation ({names[i]})", description + "\n" + spells[i].Description);
                
                var polymorph = buff.GetComponent<Kingmaker.UnitLogic.Buffs.Polymorph>().CreateCopy();
                polymorph.Facts = polymorph.Facts.RemoveFromArray(turn_back);
                buff.ReplaceComponent<Kingmaker.UnitLogic.Buffs.Polymorph>(polymorph);

                var feature = Common.createSwitchActivatableAbilityBuff(buff.name.Replace("Buff", ""), "", "", "",
                                                                        buff,
                                                                        rage_buff,
                                                                        new Kingmaker.ElementsSystem.GameAction[] { remove_polymorph },
                                                                        null,
                                                                        group: ActivatableAbilityGroup.ChangeShape);
                buff.SetBuffFlags(BuffFlags.HiddenInUi | buff.GetBuffFlags());
                features.Add(feature);
            }

            feral_transformation_wolf = features[0];
            feral_transformation_leopard = features[1];
            feral_transformation_dire_wolf = features[2];
            feral_transformation_bear = features[3];
            feral_transformation_smilodon = features[4];
            feral_transformation_mastodon = features[5];
        }
    }
}
