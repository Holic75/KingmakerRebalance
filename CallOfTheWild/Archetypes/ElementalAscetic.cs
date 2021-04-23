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
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class ElementalAscetic
    {
        static public BlueprintArchetype archetype;
        static public BlueprintFeature elemental_flurry;
        static public BlueprintFeatureSelection elemental_focus;
        static public BlueprintFeature unlock_ac_bonus;
        static public BlueprintFeature proficiencies;
        static public BlueprintFeature flurry11;

        static LibraryScriptableObject library => Main.library;

        internal static void create()
        {
            var kineticist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ElementalAsceticArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Elemental Ascetic");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Combining the elemental powers of a kineticist with the rigid physical discipline of a monk, an elemental ascetic channels his powers through his body to enhance himself in combat.");
            });
            Helpers.SetField(archetype, "m_ParentClass", kineticist_class);
            library.AddAsset(archetype, "");

            var element_selection = library.Get<BlueprintFeatureSelection>("1f3a15a3ae8a5524ab8b97f469bf4e3d");
            var infusion_selection = library.Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var proficiencies = library.Get<BlueprintFeature>("31ad04e4c767f5d4b96c13a71fd7ff15");
            var improved_unarmed_strike = Common.featureToFeature(library.Get<BlueprintFeature>("7812ad3672a4b9a4fb894ea402095167"), false, prefix: "ElementalAscetic");
            improved_unarmed_strike.SetDescription("At 1st level, an elemental ascetic gains Improved Unarmed Strike as a bonus feat.");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, element_selection, proficiencies, infusion_selection),
                                                          Helpers.LevelEntry(5, infusion_selection),
                                                          Helpers.LevelEntry(11, infusion_selection),
                                                          Helpers.LevelEntry(17, infusion_selection)
                                                        };
            createElementalFocus();
            createElementalFlurry();
            createAcBonus();

            var elemental_ascetic_proficiencies = library.CopyAndAdd<BlueprintFeature>("31ad04e4c767f5d4b96c13a71fd7ff15", "ElementalAsceticProficienciesFeature", "");
            elemental_ascetic_proficiencies.ReplaceComponent<AddFacts>(a => a.Facts = a.Facts.RemoveFromArray(library.Get<BlueprintFeature>("6d3728d4e9c9898458fe5e9532951132")));
            elemental_ascetic_proficiencies.SetNameDescription("Elemental Ascetic Proficiencies", "Elemental ascetic is not proficient with any armor.");
            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, KineticistFix.kinetic_fist, elemental_focus, elemental_ascetic_proficiencies, unlock_ac_bonus, elemental_flurry, improved_unarmed_strike),
                                                       Helpers.LevelEntry(5, KineticistFix.powerful_fist[0]),
                                                       Helpers.LevelEntry(11, KineticistFix.powerful_fist[1], flurry11),
                                                       Helpers.LevelEntry(17, KineticistFix.powerful_fist[2]),
                                                    };

            archetype.OverrideAttributeRecommendations = true;
            kineticist_class.Progression.UIGroups[1].Features.Add(elemental_focus);
            kineticist_class.Progression.UIDeterminatorsGroup = kineticist_class.Progression.UIDeterminatorsGroup.AddToArray(proficiencies, improved_unarmed_strike, unlock_ac_bonus);
            kineticist_class.Progression.UIGroups = kineticist_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(elemental_flurry, flurry11));
            kineticist_class.Progression.UIGroups = kineticist_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(KineticistFix.powerful_fist.AddToArray(KineticistFix.kinetic_fist)));          
            kineticist_class.Archetypes = kineticist_class.Archetypes.AddToArray(archetype);

            //remove leather armor
            archetype.ReplaceStartingEquipment = true;
            archetype.StartingItems = archetype.GetParentClass().StartingItems.Skip(1).ToArray(); 
        }


        static void createAcBonus()
        {
            var monk = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");
            var ac_bonus = library.Get<BlueprintFeature>("e241bdfd6333b9843a7bfd674d607ac4");
            ac_bonus.Ranks++;
            foreach (var c in ac_bonus.GetComponents<ContextRankConfig>().ToArray())
            {
                if (c.IsBasedOnClassLevel)
                {
                    ClassToProgression.addClassToContextRankConfig(archetype.GetParentClass(), new BlueprintArchetype[] { archetype }, c, "ElementalAscetic", monk);
                }
                if (c.IsBasedOnCustomProperty) //for balance fixes (class level limiter on wisdom)
                {
                    var property = Helpers.GetField<BlueprintUnitProperty>(c, "m_CustomProperty");
                    var cfg = property.GetComponent<NewMechanics.ContextValueWithLimitProperty>().max_value;
                    ClassToProgression.addClassToContextRankConfig(archetype.GetParentClass(), new BlueprintArchetype[] { archetype }, cfg, "ElementalAscetic", monk);
                }
            }

            unlock_ac_bonus = library.CopyAndAdd<BlueprintFeature>("2615c5f87b3d72b42ac0e73b56d895e0", "ElementalAsceticACBonusUnlockFeature", "");
            unlock_ac_bonus.ReplaceComponent<MonkNoArmorFeatureUnlock>(m => m.NewFact = ac_bonus);
            unlock_ac_bonus.SetDescription($"When unarmored and unencumbered, an elemental ascetic adds his Wisdom bonus (if any{(Main.settings.balance_fixes_monk_ac ? ", up to his elemental ascetic level" : "")}) to his AC and CMD. In addition, an elemental ascetic gains a +1 bonus to AC and CMD at 4th level. This bonus increases by 1 for every four elemental ascetic levels thereafter, up to a maximum of +5 at 20th level.\n"
                                           + "An elemental ascetic does not receive his element’s defensive wild talent and can never take the expanded defense utility wild talent.");
        }

        static void createElementalFlurry()
        {
            var flurry_feature1 = library.Get<BlueprintFeature>("332362f3bd39ebe46a740a36960fdcb4");
            var flurry_feature11 = library.Get<BlueprintFeature>("de25523acc24b1448aa90f74d6512a08");
            var full_bab = Helpers.CreateFeature("ElementalAsceticFullBabFeature",
                                                 "",
                                                 "",
                                                 "",
                                                 null,
                                                 FeatureGroup.None,
                                                 Helpers.Create<RaiseBAB>(r => r.TargetValue = Helpers.CreateContextValue(AbilityRankType.Default)),
                                                 Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { archetype.GetParentClass() })
                                                 );

            var burn_abilities = KineticistFix.kinetic_fist.GetComponents<AddFeatureIfHasFact>().Select(f =>
                (f.Feature as BlueprintFeature).GetComponents<AddFeatureIfHasFact>().ToArray()[1].Feature as BlueprintAbility).ToArray();

            elemental_flurry = Helpers.CreateFeature("ElementalFlurryFeature",
                                                     "Elemental Flurry",
                                                     "At 1st level, an elemental ascetic gains the kinetic fist form infusion and it costs 0 points of burn instead of 1 point of burn. When using the kinetic fist form infusion with a full attack, he can make a flurry of blows as the monk class feature treating his kineticist class level as his Base Attack Bonus. He must use only his fists to make this flurry, no matter what other abilities he possesses.\n"
                                                     + "Like a monk, he can use this ability only when unarmored, not using a shield, and unencumbered. He can’t use his kinetic blast without a form infusion, nor can he ever use his kinetic blast with energize weapon, extended range or kinetic blade infusions or with any other infusions listing them as prerequisites.",
                                                     "",
                                                     KineticistFix.kinetic_fist.Icon,
                                                     FeatureGroup.None,
                                                     Helpers.Create<FeralCombatTraining.MonkNoArmorAndUnarmedFeatureUnlock>(m => m.NewFact = flurry_feature1),
                                                     Helpers.Create<FeralCombatTraining.MonkNoArmorAndUnarmedFeatureUnlock>(m => m.NewFact = full_bab),
                                                     //Helpers.CreateAddFact(KineticistFix.kinetic_fist),
                                                     Helpers.Create<AddKineticistBurnModifier>(a =>
                                                     {
                                                         a.BurnType = KineticistBurnType.Infusion;
                                                         a.Value = -1;
                                                         a.AppliableTo = burn_abilities;
                                                     }
                                                     )
                                                     );

            var pummeling_style = library.Get<BlueprintFeature>("c36562b8e7ae12d408487ba8b532d966");
            pummeling_style.AddComponent(Helpers.PrerequisiteFeature(elemental_flurry, any: true));

            flurry11 = library.CopyAndAdd(elemental_flurry, "ElementalFlurry11Feature", "");
            flurry11.ComponentsArray = new BlueprintComponent[] { Helpers.Create<FeralCombatTraining.MonkNoArmorAndUnarmedFeatureUnlock>(m => m.NewFact = flurry_feature11) };

            var forbidden_features_id = new string[]
            {
                "cb2d9e6355dd33940b2bef49e544b0bf", //extended range
                "9ff81732daddb174aa8138ad1297c787", //kinetic blade
                "bb0de2047c448bd46aff120be3b39b7a", //enveloping winds
                "a275b35f282601944a97e694f6bc79f8", //flesh of stone
                "8ad77685e64842c45a6f5b19f9086c6c", //searing flesh
                "29ec36fa2a5b8b94ebce170bd369083a", //shroud of water
                KineticistFix.energize_weapon.AssetGuid
            };

            foreach (var id in forbidden_features_id)
            {
                library.Get<BlueprintFeature>(id).AddComponent(Common.prerequisiteNoArchetype(archetype));
            }

        }


        static void createElementalFocus()
        {
            var kineticist = library.Get<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");
            var elemental_foci = new List<BlueprintFeature>();
            var elemental_focus_standard = library.Get<BlueprintFeatureSelection>("1f3a15a3ae8a5524ab8b97f469bf4e3d");

            foreach (var efs in elemental_focus_standard.AllFeatures)
            {
                var progression = efs as BlueprintProgression;

                var focus = Helpers.CreateProgression("ElementalAscetic" + progression.name,
                                                  progression.Name,
                                                  progression.Description,
                                                  "",
                                                  progression.Icon,
                                                  progression.Groups[0]);
                focus.LevelEntries = new LevelEntry[] { progression.LevelEntries[0] };
                elemental_foci.Add(focus);
                efs.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = focus)); //to work correctly with 2nd, 3rd element
            }

            elemental_focus = library.CopyAndAdd(elemental_focus_standard, "ElementalAscetic" + elemental_focus_standard.name, "");

            elemental_focus.AllFeatures = elemental_foci.ToArray();
            elemental_focus.RemoveComponents<NewMechanics.FeatureReplacement>();
            elemental_focus_standard.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = elemental_focus));

            //we need to hide all basic base blasts from elemental ascetic
            var kientic_blade_infusion = library.Get<BlueprintFeature>("9ff81732daddb174aa8138ad1297c787");

            foreach (var c in kientic_blade_infusion.GetComponents<AddFeatureIfHasFact>())
            {
                var blast_base = c.CheckedFact as BlueprintAbility;

                var standard_blast = blast_base.Variants[0];
                standard_blast.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfCasterHasNoFact>(s => s.UnitFact = elemental_focus));
            }

            //done after havocker, so should no longer have AbilityShowIfCasterHasFact but only AbilityShowIfCasterHasFactsFromList
            var abilities = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(a =>
            {
                var comp = a.GetComponent<NewMechanics.AbilityShowIfCasterHasFactsFromList>();
                return comp != null && comp.UnitFacts.Contains(elemental_focus_standard);
            }
                                                                            );
            foreach (var a in abilities)
            {
                var comp = a.GetComponent<NewMechanics.AbilityShowIfCasterHasFactsFromList>();
                comp.UnitFacts = comp.UnitFacts.AddToArray(elemental_focus);
            }


        }
    }
}
