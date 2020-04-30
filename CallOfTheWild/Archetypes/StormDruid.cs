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
    public class StormDruid
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintArchetype archetype;
        static public List<BlueprintProgression> domain_secondary_progressions = new List<BlueprintProgression>();
        static public List<BlueprintProgression> domain_primary_progressions = new List<BlueprintProgression>();

        static public BlueprintFeature spontaneous_casting;

        static public BlueprintFeatureSelection nature_bond;
        static public BlueprintFeatureSelection wind_lord;
        static public BlueprintFeature storm_walker;
        static public BlueprintFeature eyes_of_the_storm;
        static public BlueprintFeature storm_lord;
        

        static public void create()
        {
            var druid = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "StormDruidArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Storm Druid");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While most druids focus their attention upon the rich earth and the bounty of nature that springs forth from it, the storm druid’s eyes have ever been cast to the skies and the endless expanse of blue, channeling the most raw and untamed aspects of nature.");
            });
            Helpers.SetField(archetype, "m_ParentClass", druid);
            library.AddAsset(archetype, "");
            createNatureBondAndWindLord();
            createMiscAbilities();


            var druid_bond = library.Get<BlueprintFeatureSelection>("3830f3630a33eba49b60f511b4c8f2a8");
            var spontaneous_summon = library.Get<BlueprintFeature>("b296531ffe013c8499ad712f8ae97f6b");
            var resis_nature_lure = library.Get<BlueprintFeature>("ad6a5b0e1a65c3540986cf9a7b006388");
            var venom_immunity = library.Get<BlueprintFeature>("5078622eb5cecaf4683fa16a9b948c2c");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, druid_bond, spontaneous_summon),
                                                          Helpers.LevelEntry(4, resis_nature_lure),
                                                          Helpers.LevelEntry(9, venom_immunity)
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, nature_bond, spontaneous_casting),
                                                       Helpers.LevelEntry(2, storm_walker),
                                                       Helpers.LevelEntry(4, eyes_of_the_storm),
                                                       Helpers.LevelEntry(9, wind_lord),
                                                       Helpers.LevelEntry(13, storm_lord)};

            druid.Progression.UIDeterminatorsGroup = druid.Progression.UIDeterminatorsGroup.AddToArray(nature_bond, spontaneous_casting);
            druid.Progression.UIGroups = druid.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(storm_walker, eyes_of_the_storm, wind_lord, storm_lord));
            druid.Archetypes = druid.Archetypes.AddToArray(archetype);
        }


        static void createMiscAbilities()
        {
            storm_walker = Helpers.CreateFeature("StormWalkerStormLordFeature",
                                                 "Storm Walker",
                                                 "At 2nd level, storm druid is no longer slowed by weather effects.",
                                                 "",
                                                 LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormStep.png"),
                                                 FeatureGroup.None,
                                                 Helpers.Create<WeatherMechanics.IgnoreWhetherMovementEffects>()
                                                 );

            eyes_of_the_storm = Helpers.CreateFeature("EyesOfTheStormStormLordFeature",
                                                     "Eyes of the Storm",
                                                     "At 4th level, a storm druid can see through magical fog, mist, gas, wind, rain, or similar inclement weather conditions, ignoring any concealment it might grant.",
                                                     "",
                                                     library.Get<BlueprintAbility>("582009cf6013790469d6e98e5210477a").Icon, //eyebite
                                                     FeatureGroup.None,
                                                     Helpers.Create<ConcealementMechanics.IgnoreFogConcelement>()
                                                     );

            storm_lord = Helpers.CreateFeature("StormLordStormLordFeature",
                                             "Storm Lord",
                                             "At 13th level, a storm druid is unaffected by natural and magical wind effects. She also gains +2 bonus on saving throws against sonic effects.",
                                             "",
                                             library.Get<BlueprintAbility>("d2cff9243a7ee804cb6d5be47af30c73").Icon, //lightning bolt
                                             FeatureGroup.None,
                                             Helpers.CreateAddFact(NewSpells.immunity_to_wind),
                                             Common.createSavingThrowBonusAgainstDescriptor(2, ModifierDescriptor.UntypedStackable, SpellDescriptor.Sonic)
                                             );
        }


        static void createNatureBondAndWindLord()
        {
            var domains = new BlueprintProgression[]{library.Get<BlueprintProgression>("4a3516fdc4cda764ebd1279b22d10205"),
                                                     library.Get<BlueprintProgression>("3aef017b78329db4fa53fe8560069886")
                                                    };
            foreach (var d in domains)
            {
                var spell_list = d.LevelEntries[0].Features[1].GetComponent<AddSpecialSpellList>().SpellList;
                var primary_domain = library.CopyAndAdd<BlueprintProgression>(d.AssetGuid, "StormLord" + d.name, "");

                var spells = Common.getSpellsFromSpellList(spell_list);
                var spells_array = Common.createSpelllistsForSpontaneousConversion(spells);

                for (int i = 0; i < spells_array.Length; i++)
                {
                    primary_domain.AddComponent(Common.createSpontaneousSpellConversion(archetype.GetParentClass(), spells_array[i].ToArray()));
                }
                var secondary_domain = library.CopyAndAdd<BlueprintProgression>(primary_domain.AssetGuid, "StormLordSecondary" + d.name, "");

                List<LevelEntry> secondary_level_entries = new List<LevelEntry>();
                secondary_level_entries.Add(Helpers.LevelEntry(9));

                foreach (var level_entry in d.LevelEntries)
                {
                    if (level_entry.Level <= 9)
                    {
                        secondary_level_entries[0].Features.AddRange(level_entry.Features);
                    }
                    else
                    {
                        secondary_level_entries.Add(level_entry);
                    }
                }
                secondary_domain.LevelEntries = secondary_level_entries.ToArray();
                secondary_domain.AddComponent(Helpers.PrerequisiteNoFeature(primary_domain));
                domain_primary_progressions.Add(primary_domain);
                domain_secondary_progressions.Add(secondary_domain);
            }

            nature_bond = Helpers.CreateFeatureSelection("NatureBondStormLordFeatureSelection",
                                                        "Nature Bond",
                                                        "A storm druid may not choose an animal companion. A storm druid must choose the Air or Weather domain.",
                                                        "",
                                                        library.Get<BlueprintAbility>("7cfbefe0931257344b2cb7ddc4cdff6f").Icon, //storm bolts
                                                        FeatureGroup.DruidDomain);
            nature_bond.AllFeatures = domain_primary_progressions.ToArray();

            wind_lord = Helpers.CreateFeatureSelection("WindLordStormLordFeatureSelection",
                                                        "Wind Lord",
                                                        "At 9th level, a storm druid can select another domain or subdomain from those available to her through her nature bond.",
                                                        "",
                                                        library.Get<BlueprintFeature>("f2fa7541f18b8af4896fbaf9f2a21dfe").Icon, //cyclone
                                                        FeatureGroup.DruidDomain);
            wind_lord.AllFeatures = domain_secondary_progressions.ToArray();

            spontaneous_casting = Helpers.CreateFeature("SpontaneousCastingStormLord",
                                                        "Spontaneous Domain Casting",
                                                        "A storm druid can channel stored spell energy into domain spells that she has not prepared ahead of time. She can “lose” a prepared spell in order to cast any domain spell of the same level or lower.",
                                                        "",
                                                        library.Get<BlueprintAbility>("0bd54216d38852947930320f6269a9d7").Icon,
                                                        FeatureGroup.None
                                                        );
        }


    }
}
