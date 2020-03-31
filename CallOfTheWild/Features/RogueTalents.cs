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

namespace CallOfTheWild
{
    public class RogueTalents
    {
        static public BlueprintFeatureSelection minor_magic;
        static public BlueprintFeatureSelection major_magic;
        static LibraryScriptableObject library => Main.library;

        static public void load()
        {
            createMinorMagic();
            createMajorMagic();
        }


        static void addToTalentSelection(BlueprintFeature f)
        {
            var selections =
                new BlueprintFeatureSelection[]
                {
                    library.Get<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93"), //rogue talent
                    library.Get<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118"), //slayer talent2
                    library.Get<BlueprintFeatureSelection>("43d1b15873e926848be2abf0ea3ad9a8"), //slayer talent6
                    library.Get<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66"), //slayerTalent10
                    Investigator.investigator_talent_selection
                };

            foreach (var s in selections)
            {
                s.AllFeatures = s.AllFeatures.AddToArray(f);
            }
        }


        static void createMinorMagic()
        {
            var spells = Helpers.wizardSpellList.SpellsByLevel[0].Spells;

            minor_magic = Helpers.CreateFeatureSelection("MinorMagicRogueTalent",
                                                         "Minor Magic",
                                                         "A rogue with this talent gains the ability to cast a 0-level spell from the sorcerer/wizard spell list. This spell can be cast three times a day as a spell-like ability. The caster level for this ability is equal to the rogue’s level. The save DC for this spell is 10 + the rogue’s Intelligence modifier.",
                                                         "",
                                                         Helpers.GetIcon("16e23c7a8ae53cc42a93066d19766404"), //jolt
                                                         FeatureGroup.RogueTalent,
                                                         Helpers.PrerequisiteStatValue(StatType.Intelligence, 10)
                                                         );

            var classes = new BlueprintCharacterClass[] {library.Get<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484"), //rogue
                                                         library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb"), //slayer
                                                         Investigator.investigator_class };


            foreach (var s in spells)
            {
                BlueprintFeature feature = null;
                if (!s.HasVariants)
                {
                    var spell_like = Common.convertToSpellLike(s, "MinorMagic", classes, StatType.Intelligence, no_resource: true, no_scaling: true,
                                                               guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid));                   
                    feature = Common.AbilityToFeature(spell_like, false);
                    feature.AddComponent(Helpers.Create<BindAbilitiesToClass>(b =>
                                                                                {
                                                                                    b.Abilites = new BlueprintAbility[] { spell_like };
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = true;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                }
                                                                                )
                                                                            );
                }
                else
                {
                    List<BlueprintAbility> spell_likes = new List<BlueprintAbility>();
                    foreach (var v in s.Variants)
                    {
                        spell_likes.Add(Common.convertToSpellLike(v, "MinorMagic", classes, StatType.Intelligence, no_resource: true, no_scaling: true,
                                                                  guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", v.AssetGuid)));
                    }
                    var wrapper = Common.createVariantWrapper("MinorMagic" + s.name, Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid), spell_likes.ToArray());
                    wrapper.SetNameDescriptionIcon(s.Name, s.Description, s.Icon);
                    feature = Common.AbilityToFeature(wrapper, false);
                    feature.AddComponent(Helpers.Create<BindAbilitiesToClass>(b =>
                                                                                {
                                                                                    b.Abilites = spell_likes.ToArray();
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = true;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                }
                                                                                )
                                                                             );
                }
                feature.SetName("Minor Magic: " + feature.Name);
                feature.Groups = new FeatureGroup[] { FeatureGroup.RogueTalent };
                minor_magic.AllFeatures = minor_magic.AllFeatures.AddToArray(feature);
            }
            addToTalentSelection(minor_magic);                                                       
        }


        static void createMajorMagic()
        {
            var spells = Helpers.wizardSpellList.SpellsByLevel[1].Spells;

            major_magic = Helpers.CreateFeatureSelection("MajorMagicRogueTalent",
                                                         "Major Magic",
                                                         "A rogue with this talent gains the ability to cast a 1st-level spell from the sorcerer/wizard spell list once per day as a spell-like ability for every 2 rogue levels she possesses. The rogue’s caster level for this ability is equal to her rogue level. The save DC for this spell is 11 + the rogue’s Intelligence modifier. A rogue must have the minor magic rogue talent and an Intelligence score of at least 11 to select this talent.",
                                                         "",
                                                         Helpers.GetIcon("4ac47ddb9fa1eaf43a1b6809980cfbd2"), //magic missile
                                                         FeatureGroup.RogueTalent,
                                                         Helpers.PrerequisiteStatValue(StatType.Intelligence, 11),
                                                         Helpers.PrerequisiteFeature(minor_magic)
                                                         );

            var classes = new BlueprintCharacterClass[] {library.Get<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484"), //rogue
                                                         library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb"),
                                                         Investigator.investigator_class };


            foreach (var s in spells)
            {
                var resource = Helpers.CreateAbilityResource("MajorMagic" + s.name + "Resource", "", "", Helpers.MergeIds("27fea41a99cd46609f8ab2283d1afce0", s.AssetGuid), null);
                resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, classes);
                BlueprintFeature feature = null;
                if (!s.HasVariants)
                {
                    var spell_like = Common.convertToSpellLike(s, "MajorMagic", classes, StatType.Intelligence, resource, no_scaling: true,
                                                               guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid));
                    feature = Common.AbilityToFeature(spell_like, false);
                    spell_like.AddComponent(Helpers.Create<NewMechanics.BindAbilitiesToClassFixedLevel>(b =>
                                                                                {
                                                                                    b.Abilites = new BlueprintAbility[] { spell_like };
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = false;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                    b.fixed_level = 1;
                                                                                }
                                                                                )
                                                                            );
                }
                else
                {
                    List<BlueprintAbility> spell_likes = new List<BlueprintAbility>();
                    foreach (var v in s.Variants)
                    {
                        spell_likes.Add(Common.convertToSpellLike(v, "MajorMagic", classes, StatType.Intelligence, resource, no_scaling: true,
                                                                  guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", v.AssetGuid)));
                    }
                    var wrapper = Common.createVariantWrapper("MajorMagic" + s.name, guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid), spell_likes.ToArray());
                    wrapper.SetNameDescriptionIcon(s.Name, s.Description, s.Icon);
                    feature = Common.AbilityToFeature(wrapper, false);
                    feature.AddComponent(Helpers.Create<NewMechanics.BindAbilitiesToClassFixedLevel>(b =>
                                                                                {
                                                                                    b.Abilites = spell_likes.ToArray();
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = false;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                    b.fixed_level = 1;
                                                                                }
                                                                                )
                                                                             );
                }
                feature.SetName("Major Magic: " + feature.Name);
                feature.Groups = new FeatureGroup[] { FeatureGroup.RogueTalent };
                feature.AddComponent(resource.CreateAddAbilityResource());
                major_magic.AllFeatures = major_magic.AllFeatures.AddToArray(feature);
            }
            addToTalentSelection(major_magic);
        }




    }
}
