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
using Kingmaker.Blueprints.Root;
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
using Kingmaker.UnitLogic.Class.Kineticist;
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
    public class ElementalAnnihilator
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature devastating_infusion;
        static public BlueprintFeatureSelection dampened_versality;
        static public BlueprintFeature increased_range;
        static public BlueprintFeature blast_training;
        static public BlueprintFeature omnicide;

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var kineticist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ElementalAnnihilatorArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Elemental Annihilator");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "For some kineticists, nothing in life is as sweet as destruction and pain. Elemental annihilators pursue only uses of their powers that harm others.");
            });
            Helpers.SetField(archetype, "m_ParentClass", kineticist_class);
            library.AddAsset(archetype, "");

            var element_selection = library.Get<BlueprintFeatureSelection>("1f3a15a3ae8a5524ab8b97f469bf4e3d");
            var infusion_selection = library.Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var wild_talent_selection = library.Get<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, infusion_selection),
                                                          Helpers.LevelEntry(2, wild_talent_selection),
                                                          Helpers.LevelEntry(3, infusion_selection),
                                                          Helpers.LevelEntry(4, wild_talent_selection),
                                                          Helpers.LevelEntry(5, infusion_selection),
                                                          Helpers.LevelEntry(6, wild_talent_selection),
                                                          Helpers.LevelEntry(8, wild_talent_selection),
                                                          //Helpers.LevelEntry(9, infusion_selection), since no extreme range infusion
                                                          Helpers.LevelEntry(10, wild_talent_selection),
                                                          Helpers.LevelEntry(12, wild_talent_selection),
                                                          Helpers.LevelEntry(14, wild_talent_selection),
                                                          Helpers.LevelEntry(16, wild_talent_selection),
                                                          Helpers.LevelEntry(18, wild_talent_selection),
                                                          Helpers.LevelEntry(20, wild_talent_selection)
                                                        };
            createDevastatingInfusion();
           // creatBlastTraining();
           // createDampenedVersality();
           // createOmnicide();

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, devastating_infusion),
                                                      /* Helpers.LevelEntry(2, dampened_versality),
                                                       Helpers.LevelEntry(3, increased_range),
                                                       Helpers.LevelEntry(5, blast_training),
                                                       Helpers.LevelEntry(8, dampened_versality),
                                                       Helpers.LevelEntry(9, blast_training),
                                                       Helpers.LevelEntry(10, dampened_versality),
                                                       Helpers.LevelEntry(13, blast_training),
                                                       Helpers.LevelEntry(14, dampened_versality),
                                                       Helpers.LevelEntry(17, blast_training),
                                                       Helpers.LevelEntry(18, dampened_versality),
                                                       Helpers.LevelEntry(20, omnicide)*/
                                                    };

            archetype.OverrideAttributeRecommendations = true;
            kineticist_class.Progression.UIDeterminatorsGroup = kineticist_class.Progression.UIDeterminatorsGroup.AddToArray(devastating_infusion);
            kineticist_class.Progression.UIGroups = kineticist_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(increased_range, dampened_versality, omnicide));
            kineticist_class.Archetypes = kineticist_class.Archetypes.AddToArray(archetype);

            //fix expanded element bonus
            wild_talent_selection.AddComponent(Common.prerequisiteNoArchetype(archetype));
            kineticist_class.Archetypes = kineticist_class.Archetypes.AddToArray(archetype);
        }


        static void createDevastatingInfusion()
        {
            var kinetic_blade_feature = library.Get<BlueprintFeature>("9ff81732daddb174aa8138ad1297c787");
            //consits of AddFeatureIfHasFact (checkedFact == blast base ability, feature ==  corresponding kinetic blade feature)
            //corresponding kinetic blade feature adds blade toggle if it is not here and blade burn ability (through AddFeatureIfHasFact)
            //
            //blade burn ability - cost (can leave it at 1 ? + cost of infusions) + blade enabled buff
            //
            //blade toggle - restriction checking for no polymorph, empty primary hand, checks if enough burn can be taken;
            //blade toggle buff - adds kinetic blade item through AddKineticBladeItem - can be subclassed for primary/secondary hand and not disabling aoo
            //blade type should be overwritten to 1 handed, finessable ?
            //kinetic blade also contains kinetic blade enchant - with fx  + ?
            //has WeponKineticBladeComponent with burn and blast ability
            //damage ability contains
            //it does not make sense to use composite blasts since they will cost more for same damage,
            //so it is only physical (bludgeoning damage), and can be implemented as normal weapon

        }
    }
}
