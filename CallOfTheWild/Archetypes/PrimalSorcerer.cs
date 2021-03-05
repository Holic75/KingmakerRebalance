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
    public class PrimalSorcerer
    {
        static BlueprintArchetype archetype;
        static public BlueprintFeatureSelection primal_bloodline_selection;

        static LibraryScriptableObject library => Main.library;


        public static void create()
        {
            var sorcerer = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
            createPrimalBloodlines();
          
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "PrimalSorcererArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Primal Sorcerer");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Your powers are attuned to the concentrated core of the elemental plane.");
            });
            Helpers.SetField(archetype, "m_ParentClass", sorcerer);
            library.AddAsset(archetype, "");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, library.Get<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914")), //original bloodline

                                                       };
            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, primal_bloodline_selection)};

            sorcerer.Archetypes = sorcerer.Archetypes.AddToArray(archetype);

            //fix dragon disciple
            var dragon_disicple = library.Get<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
            dragon_disicple.AddComponent(Common.prerequisiteNoArchetype(sorcerer, archetype));
        }


        static void createPrimalBloodlines()
        {
            primal_bloodline_selection = library.CopyAndAdd<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914", "PrimalBloodlineSelection", "");
            primal_bloodline_selection.AllFeatures = new BlueprintFeature[0];
            primal_bloodline_selection.SetName("Primal Bloodline");

            var sorcerer = library.Get<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
            var bloodline_feats = library.Get<BlueprintFeature>("d2a4b74ee7e43a648b51d0f36db2aa34");

            var core_bloodline_id = new string[]
                                                {
                                                    "cd788df497c6f10439c7025e87864ee4", //air
                                                    "32393034410fb2f4d9c8beaa5c8c8ab7", //earth
                                                    "17cc794d47408bc4986c55265475c06f", //fire
                                                    "7c692e90592257a4e901d12ae6ec1e41", //water
                                                };

            var arcana_prototype_id = new string[]
                                                {
                                                    "0f0cb88a2ccc0814aa64c41fd251e84e", //blue dragon
                                                    "caebe2fa3b5a94d4bbc19ccca86d1d6f", //green dragon
                                                    "a8baee8eb681d53438cc17bd1d125890", //red dragon
                                                    "456e305ebfec3204683b72a45467d87c", //white
                                                };

            DamageEnergyType[] energy = new DamageEnergyType[] { DamageEnergyType.Electricity, DamageEnergyType.Acid, DamageEnergyType.Fire, DamageEnergyType.Cold };

            for (int i = 0; i < core_bloodline_id.Length; i++)
            {
                var prototype = library.Get<BlueprintProgression>(core_bloodline_id[i]);
                var progression = library.CopyAndAdd<BlueprintProgression>(core_bloodline_id[i], "Primal" + prototype.name, "");
                progression.SetName("Primal " + progression.Name);

                var arcana = library.CopyAndAdd<BlueprintFeature>(arcana_prototype_id[i], energy[i].ToString() + "PrimalElementalArcanaFeature", "");
                arcana.SetName("Primal Bloodline Arcana");

                progression.Classes = new BlueprintCharacterClass[] { sorcerer };

                var action = Helpers.CreateActionDealDamage(energy[i], Helpers.CreateContextDiceValue(DiceType.D6, 1, 0), IgnoreCritical: true);
                var on_hit = Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(action));
                on_hit.AllNaturalAndUnarmed = true;
                var buff = Helpers.CreateBuff(energy[i].ToString() + "PrimalElementalistSummoningBuff",
                                             "Elementalist Summoning",
                                             "At 9th level, whenever you summon a creature, it gains energy resistance 10 against the energy type that matches your elemental bloodline, and its natural attacks deal an additional 1d6 points of damage of the same energy type. ",
                                             "",
                                             library.Get<BlueprintAbility>("333efbf776ab61c4da53e9622751d95f").Icon,
                                             null,
                                             Common.createEnergyDR(10, energy[i]),
                                             on_hit);

                var feature = Helpers.CreateFeature(energy[i].ToString() + "PrimalElementalistSummoningFeature",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    FeatureGroup.None);
                feature.AddComponent(Helpers.Create<OnSpawnBuff>(o => { o.IfHaveFact = feature; o.buff = buff; o.CheckDescriptor = true; o.SpellDescriptor = SpellDescriptor.Summoning; o.IsInfinity = true; }));

                var level_entries = new List<LevelEntry>();

                foreach (var le in prototype.LevelEntries)
                {
                    var new_le = Helpers.LevelEntry(le.Level);

                    foreach (var f in le.Features)
                    {
                        if (f.name.Contains("Arcana"))
                        {
                            new_le.Features.Add(arcana);
                        }
                        else if (f.name.Contains("ElementalBlastFeature"))
                        {
                            new_le.Features.Add(feature);
                        }
                        else
                        {
                            new_le.Features.Add(f);
                        }
                    }
                    level_entries.Add(new_le);
                }
                   
                progression.LevelEntries = level_entries.ToArray();
                //progression.UIGroups[0].Features.Add(feature);

                var feat_prereq = bloodline_feats.GetComponent<PrerequisiteFeaturesFromList>();
                feat_prereq.Features = feat_prereq.Features.AddToArray(progression);

                primal_bloodline_selection.AllFeatures = primal_bloodline_selection.AllFeatures.AddToArray(progression);
            }
        }

    }
}
