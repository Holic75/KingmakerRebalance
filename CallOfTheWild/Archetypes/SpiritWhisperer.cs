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
    public class SpiritWhisperer
    {
        static public BlueprintArchetype archetype;

        static SpiritsEngine spirits_engine;
        static HexEngine hex_engine;
        static public SpiritsEngine.BattleSpirit battle_spirit;
        static public SpiritsEngine.BonesSpirit bones_spirit;
        static public SpiritsEngine.FlameSpirit flame_spirit;
        static public SpiritsEngine.LifeSpirit life_spirit;
        static public SpiritsEngine.HeavensSpirit heavens_spirit;
        static public SpiritsEngine.LoreSpirit lore_spirit;
        static public SpiritsEngine.NatureSpirit nature_spirit;
        static public SpiritsEngine.StoneSpirit stone_spirit;
        static public SpiritsEngine.WavesSpirit waves_spirit;
        static public SpiritsEngine.WindSpirit wind_spirit;

        static public BlueprintFeatureSelection spirit_selection;
        static public BlueprintFeatureSelection spirit_hex_selection;

        static LibraryScriptableObject library => Main.library;
        static public BlueprintFeatureSelection shaman_familiar;

        public class Spirit
        {
            public BlueprintProgression progression;
            public BlueprintFeatureSelection hex_selection;

            public Spirit(string name, string display_name, string description, UnityEngine.Sprite icon, string guid,
                BlueprintFeature spirit_ability, BlueprintFeature greater_spirit_ability, BlueprintFeature manifestation, BlueprintFeature[] hexes)
            {

                var entries = new LevelEntry[] { Helpers.LevelEntry(1, spirit_ability),
                                                 Helpers.LevelEntry(8, greater_spirit_ability),
                                                 Helpers.LevelEntry(20, manifestation)
                                               };


                progression = Helpers.CreateProgression(name + "SpiritWhispererProgression",
                                                        display_name + " Spirit",
                                                        description,
                                                        "",
                                                        icon,
                                                        FeatureGroup.None
                                                        );
                progression.LevelEntries = entries.ToArray();
                progression.UIGroups = Helpers.CreateUIGroups(spirit_ability, greater_spirit_ability, manifestation);

                this.hex_selection = Helpers.CreateFeatureSelection(name + "SpiritWhispirerSpiritHexSelection",
                               display_name + " Spirit Hex",
                               $"You can select one of the hexes granted by {display_name} spirit.",
                               "",
                               icon,
                               FeatureGroup.None,
                               Helpers.PrerequisiteFeature(progression)
                               );
                this.hex_selection.AllFeatures = hexes;
                progression.Classes = new BlueprintCharacterClass[] { archetype.GetParentClass() };
            }
        }

        static public Dictionary<string, Spirit> spirits = new Dictionary<string, Spirit>();

        public static void create()
        {
            var wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SpiritWhispererArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Spirit Whisperer");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Spirit whisperers are a breed apart among wizards, and are often mistaken for witches. While spirit whisperers do gain and store their spells by communing with familiars, the spirits they gain guidance from are somewhat closer to the world and more direct than the powers with which witches typically traffic. These wizards treat such spirits as mentors and friends, conversing with them rather than appeasing them in the effort to gain and use arcane knowledge.");
            });
            Helpers.SetField(archetype, "m_ParentClass", wizard);
            library.AddAsset(archetype, "");
            createSpiritSelection();
            createHexSelection();

            shaman_familiar = library.CopyAndAdd<BlueprintFeatureSelection>("363cab72f77c47745bf3a8807074d183", "SpiritWhispererSpiritAnimal", "");
            shaman_familiar.DlcType = Kingmaker.Blueprints.Root.DlcType.None;
            shaman_familiar.ComponentsArray = new BlueprintComponent[0];
            shaman_familiar.SetNameDescriptionIcon("Arcane Bond",
                                                   "When a spirit whisperer chooses an arcane bond, he must choose the familiar arcane bond.",
                                                   Helpers.GetIcon("1194129055923ae46a6b876ff6a30358"));

            var school_selection = library.Get<BlueprintFeatureSelection>("5f838049069f1ac4d804ce0862ab5110");
            var arcane_bond = library.Get<BlueprintFeatureSelection>("03a1781486ba98043afddaabf6b7d8ff");
            var wizard_feat = library.Get<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, school_selection, arcane_bond),
                                                          Helpers.LevelEntry(5, wizard_feat),
                                                          Helpers.LevelEntry(10, wizard_feat),
                                                          Helpers.LevelEntry(15, wizard_feat),
                                                          Helpers.LevelEntry(20, wizard_feat)
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, spirit_selection, shaman_familiar),
                                                       Helpers.LevelEntry(5, spirit_hex_selection),
                                                       Helpers.LevelEntry(10, spirit_hex_selection),
                                                       Helpers.LevelEntry(15, spirit_hex_selection)};

            wizard.Progression.UIDeterminatorsGroup = wizard.Progression.UIDeterminatorsGroup.AddToArray(spirit_selection);
            wizard.Progression.UIGroups = wizard.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(shaman_familiar, spirit_hex_selection, spirit_hex_selection, spirit_hex_selection));
            wizard.Archetypes = wizard.Archetypes.AddToArray(archetype);
        }


        static void createSpiritSelection()
        {
            var wizard = library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            hex_engine = new HexEngine(new BlueprintCharacterClass[] { wizard }, StatType.Intelligence, StatType.Charisma, archetype);

            bool test_mode = false;
            spirits_engine = new SpiritsEngine(hex_engine);

            battle_spirit = new SpiritsEngine.BattleSpirit();
            bones_spirit = new SpiritsEngine.BonesSpirit();
            flame_spirit = new SpiritsEngine.FlameSpirit();
            heavens_spirit = new SpiritsEngine.HeavensSpirit();
            life_spirit = new SpiritsEngine.LifeSpirit();
            lore_spirit = new SpiritsEngine.LoreSpirit();
            nature_spirit = new SpiritsEngine.NatureSpirit();
            stone_spirit = new SpiritsEngine.StoneSpirit();
            waves_spirit = new SpiritsEngine.WavesSpirit();
            wind_spirit = new SpiritsEngine.WindSpirit();


            spirits.Add("Battle", battle_spirit.createSpiritWhispererSpirit(hex_engine, "SpiritWhisperer", "SpiritWhisperer", test_mode));
            spirits.Add("Bones", bones_spirit.createSpiritWhispererSpirit(hex_engine, "SpiritWhisperer", "SpiritWhisperer", test_mode));
            spirits.Add("Flame", flame_spirit.createSpiritWhispererSpirit(hex_engine, "SpiritWhisperer", test_mode));
            spirits.Add("Stone", stone_spirit.createSpiritWhispererSpirit(hex_engine, "SpiritWhisperer", test_mode));
            spirits.Add("Waves", waves_spirit.createSpiritWhispererSpirit(hex_engine, "SpiritWhisperer", test_mode));
            spirits.Add("Wind", wind_spirit.createSpiritWhispererSpirit(hex_engine, "SpiritWhisperer", test_mode));
            spirits.Add("Nature", nature_spirit.createSpiritWhispererSpirit(hex_engine, "SpiritWhisperer", test_mode));
            spirits.Add("Lore", lore_spirit.createSpiritWhispererSpirit(hex_engine, "SpiritWhisperer", test_mode));
            spirits.Add("Heavens", heavens_spirit.createSpiritWhispererSpirit(hex_engine, "SpiritWhisperer", test_mode));
            spirits.Add("Life", life_spirit.createSpiritWhispererSpirit(hex_engine, "SpiritWhisperer", "SpiritWhisperer", "Extra Channel (Spirit Whisperer Life Spirit)",  test_mode));

            spirit_selection = Helpers.CreateFeatureSelection("SpiritWhispererSpiritLink",
                                               "Spirit Link",
                                               "At 1st level, a spirit whisperer forms a mystical bond with a spirit. The spirit whisperer picks a spirit from the shaman’s list of spirits. At 1st level, he gains a spirit ability granted by that spirit. At 8th level, he gains the greater spirit ability granted by that spirit (or true spirit ability for Lore spirit). At 20th level, the spirit whisperer gains the manifestation ability granted by the spirit. He uses his wizard level as his shaman level for determining the effects and DCs of abilities granted by the spirit. In addition, he uses his Intelligence modifier in place of his Wisdom modifier for these abilities. He does not gain hexes, spirit magic spells, or the true spirit ability typically granted to a shaman by these spirits.",
                                               "",
                                               null,
                                               FeatureGroup.None);

            foreach (var s in spirits)
            {
                spirit_selection.AllFeatures = spirit_selection.AllFeatures.AddToArray(s.Value.progression);
            }
        }


        static void createHexSelection()
        {
            var wizard_feat = library.Get<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899");
            spirit_hex_selection = Helpers.CreateFeatureSelection("SpiritHexSpirirtWhispererSelection",
                                                                   "Spirit Hex",
                                                                   "At 5th level, a spirit whisperer can select one hex from the list of those granted by his chosen spirit. He uses his wizard level as his shaman level when determining the effects and DC of this hex. In addition, he uses his Intelligence modifier in place of his Wisdom modifier for these hexes. At 10th and 15th level, he can select another hex from those granted by his spirit. Each hex selected in this way replaces the bonus feat gained at that level.",
                                                                   "",
                                                                   null,
                                                                   FeatureGroup.None);
            foreach (var s in spirits)
            {
                spirit_hex_selection.AllFeatures = spirit_hex_selection.AllFeatures.AddToArray(s.Value.hex_selection);
            }

            spirit_hex_selection.AllFeatures = spirit_hex_selection.AllFeatures.AddToArray(wizard_feat);
        }
    }
}
