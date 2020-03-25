using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class Eidolon
    {
        static void createProteanUnit()
        {
            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var tatzlwyrm = library.Get<BlueprintUnit>("4dd913232eaf3894890b2bfaabcd8282");
            var protean_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "ProteanEidolonUnit", "");

            protean_unit.Color = tatzlwyrm.Color;
            protean_unit.Visual = tatzlwyrm.Visual;
            
            protean_unit.LocalizedName = protean_unit.LocalizedName.CreateCopy();
            protean_unit.LocalizedName.String = Helpers.CreateString(protean_unit.name + ".Name", "Protean Eidolon");

            protean_unit.Prefab = tatzlwyrm.Prefab;
            protean_unit.Alignment = Alignment.ChaoticNeutral;
            protean_unit.Strength = 12;
            protean_unit.Dexterity = 16;
            protean_unit.Constitution = 13;
            protean_unit.Intelligence = 7;
            protean_unit.Wisdom = 10;
            protean_unit.Charisma = 11;
            protean_unit.Speed = 20.Feet();
            protean_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2 }; // { natural_armor2, fx_feature };
            protean_unit.Body = protean_unit.Body.CloneObject();
            protean_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            protean_unit.Body.PrimaryHand = library.Get<BlueprintItemWeapon>("a000716f88c969c499a535dadcf09286"); //bite 1d6
            protean_unit.Body.SecondaryHand = library.Get<BlueprintItemWeapon>("b21cd5b03fbb0f542815580e66f85915"); //tail 1d6
            protean_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            protean_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[] { serpentine_archetype };
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillStealth, StatType.SkillLoreReligion };
                a.Selections = new SelectionEntry[0];
            });
            protean_unit.AddComponents(Helpers.Create<EidolonComponent>());
            Helpers.SetField(protean_unit, "m_Portrait", Helpers.createPortrait("EidolonProteanProtrait", "Protean", ""));


            protean_eidolon = Helpers.CreateProgression("ProteanEidolonProgression",
                                        "Protean Eidolon",
                                        "Serpentine beings of pure chaos, proteans seek to reshape reality. Protean eidolons appreciate creative summoners who often rebuild their forms. Beyond that, protean eidolons are happy to work with their summoners for any purpose, though they are quick to remind their summoners that while they have a mutually beneficial relationship, they are not servants.",
                                        "",
                                        Helpers.GetIcon("403cf599412299a4f9d5d925c7b9fb33"), //magic fang
                                        FeatureGroup.AnimalCompanion,
                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                        );
            protean_eidolon.IsClassFeature = true;
            protean_eidolon.ReapplyOnLevelUp = true;
            protean_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            protean_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Chaotic | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            protean_eidolon.ReplaceComponent<AddPet>(a => a.Pet = protean_unit);

            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(protean_eidolon);
        }


        static void fillProteanProgression()
        {
            var base_evolutions = Helpers.CreateFeature("ProteanEidolonBaseEvolutionsFeature",
                                            "",
                                            "",
                                            "",
                                            null,
                                            FeatureGroup.None,
                                            Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.resistance[0])
                                            );
            base_evolutions.HideInCharacterSheetAndLevelUp = true;

            var feature1 = Helpers.CreateFeature("ProteanEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, protean eidolons gain the bite, tail slap and resistance (acid) evolutions.",
                                                  "",
                                                  protean_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = Evolutions.bite),
                                                  Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = Evolutions.tail_slap),
                                                  Helpers.CreateAddFeatureOnClassLevel(base_evolutions, 16, Summoner.getSummonerArray(), before: true)
                                                  );

            var feature4 = Helpers.CreateFeature("ProteanEidolonLevel4Feature",
                                                  "Resistance",
                                                 " At 4th level, protean eidolons gain electricity resistance 10 and sonic resistance 10.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("ProteanEidolonLevel4AddFeature",
                                                                                Common.createEnergyDR(10, DamageEnergyType.Electricity),
                                                                                Common.createEnergyDR(10, DamageEnergyType.Sonic))
                                                  );

            var feature8 = Helpers.CreateFeature("ProteanEidolonLevel8Feature",
                                                  "Constrict",
                                                  "At 8th level, protean eidolons gain the constrict evolution.",
                                                  "",
                                                  Evolutions.constrict.Icon,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.constrict)
                                                  );

            var feature12 = Helpers.CreateFeature("ProteanEidolonLevel12Feature",
                                                  "Damage Reduction",
                                                  "At 12th level, protean eidolons gain DR 5/lawful. They also gain the blindsense and flight evolutions.",
                                                  "",
                                                  Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //mage armor
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("ProteanEidolonLevel12AddFeature",
                                                                                Common.createContextAlignmentDR(Helpers.CreateContextValue(AbilityRankType.Default), DamageAlignment.Lawful),
                                                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                                                progression: ContextRankProgression.MultiplyByModifier,
                                                                                                                stepLevel: 10,
                                                                                                                min: 5,
                                                                                                                featureList: new BlueprintFeature[] { Evolutions.damage_reduction }
                                                                                                                ),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Sleep),
                                                                                Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] { Evolutions.damage_reduction })
                                                                                ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.blindsense),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.flight)
                                                  );

            var amorphous_anatomy = Helpers.CreateFeature("ProteanAmorphousFeature",
                                                          "Amorphous Anatomy",
                                                          "A protean’s vital organs shift and change shape and position constantly. This grants it a 50 % chance to ignore additional damage caused by critical hits and sneak attacks, and grants it immunity to polymorph effects(unless the protean is a willing target).A protean automatically recovers from physical blindness or deafness after 1 round by growing new sensory organs to replace those that were compromised.",
                                                          "",
                                                          Evolutions.amorphous.Icon,
                                                          FeatureGroup.None,
                                                          Common.createAddFortification(50),
                                                          Common.createSpecificBuffImmunity(library.Get<BlueprintBuff>("0a52d8761bfd125429842103aed48b90")), //baleful polymorph
                                                          Helpers.Create<ProteanBuffRemovalAfter1Round>()
                                                          );
            transferable_abilities.Add(amorphous_anatomy);


            var feature16 = Helpers.CreateFeature("ProteanEidolonLevel16Feature",
                                                    "Immunity",
                                                    "At 16th level, protean eidolons lose the resistance (acid) evolution and instead gain the immunity (acid) evolution. They also gain the amorphous anatomy ability.\n"
                                                    + "Amorphous Anatomy: A protean’s vital organs shift and change shape and position constantly. This grants it a 50 % chance to ignore additional damage caused by critical hits and sneak attacks, and grants it immunity to polymorph effects(unless the protean is a willing target).A protean automatically recovers from physical blindness or deafness after 1 round by growing new sensory organs to replace those that were compromised.",
                                                    "",
                                                    Evolutions.amorphous.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[0]),
                                                    Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.amorphous),
                                                    Common.createAddFeatToAnimalCompanion(amorphous_anatomy)
                                                    );


            var heal = library.CopyAndAdd<BlueprintAbility>("ff8f1534f66559c478448723e16b6624", "ProteanHealAbility", "");
            heal.RemoveComponents<AbilityDeliverTouch>();
            heal.ReplaceComponent<ContextRankConfig>(c => { Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueType.ClassLevel); Helpers.SetField(c, "m_Class", new BlueprintCharacterClass[] { Summoner.summoner_class, Eidolon.eidolon_class }); });

            var cast_heal = Helpers.Create<ContextActionCastSpell>(c => c.Spell = heal);
            var polymorph_greater = library.Get<BlueprintAbility>("a9fc28e147dbb364ea4a3c1831e7e55f");
            List<BlueprintAbility> polymorph_variants = new List<BlueprintAbility>();
            var polymorph_resource = Helpers.CreateAbilityResource("ProteanChangeShapeResource", "", "", "", null);
            polymorph_resource.SetFixedResource(1);

            foreach (var v in polymorph_greater.Variants)
            {
                var ability = Common.convertToSuperNatural(v, "Protean", new BlueprintCharacterClass[] { Summoner.summoner_class, Eidolon.eidolon_class }, StatType.Charisma, polymorph_resource);
                ability.Type = AbilityType.Supernatural;
                ability.Range = AbilityRange.Personal;
                ability.setMiscAbilityParametersSelfOnly();
                var actions = ability.GetComponent<AbilityEffectRunAction>().Actions.Actions;

                var new_actions = Common.changeAction<ContextActionApplyBuff>(actions, c =>
                                                                                {
                                                                                    var buff = library.CopyAndAdd<BlueprintBuff>(c.Buff, "Protean" + c.Buff.name, "");
                                                                                    buff.AddComponent(Helpers.CreateAddFactContextActions(deactivated: cast_heal));
                                                                                }
                                                                                );
                polymorph_variants.Add(ability);
            }
            var change_shape = Common.createVariantWrapper("ProteanChangeShapeAbility", "", polymorph_variants.ToArray());
            change_shape.SetNameDescription("Change Shape",
                                            "A protean’s form is not fixed. Once per day as a standard action, a protean may change shape into any as per greater polymorph spell. A protean can resume its true form as a free action, and when it does so, it gains the effects of a heal spell (CL equal to the protean’s HD).");




            var freedom_of_movement_buff = library.Get<BlueprintBuff>("1533e782fca42b84ea370fc1dcbf4fc1");
            var feature20 = Helpers.CreateFeature("ProteanEidolonLevel20Feature",
                                                  "Change Shape",
                                                  "At 20th level, protean eidolons gain constant freedom of movement and the protean version of the change shape (greater polymorph) ability.",
                                                  "",
                                                  freedom_of_movement_buff.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("ProteanEidolonLevel20AddFeature",
                                                                                Common.createAuraFeatureComponent(freedom_of_movement_buff),
                                                                                Helpers.CreateAddFact(change_shape),
                                                                                polymorph_resource.CreateAddAbilityResource()
                                                                                )
                                                  );

            protean_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            protean_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);
        }
    }



    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ProteanBuffRemovalAfter1Round : RuleInitiatorLogicComponent<RuleApplyBuff>
    {
        public override void OnEventAboutToTrigger(RuleApplyBuff evt)
        {
            TimeSpan round = 6.Seconds();
            if ((evt.AppliedBuff.MaybeContext.SpellDescriptor & SpellDescriptor.Blindness) > 0
                || evt.Blueprint == Common.deafened)
            {
                Harmony12.Traverse.Create(evt).Property("Duration").SetValue(round);
            }
        }

        public override void OnEventDidTrigger(RuleApplyBuff evt)
        {

        }
    }


}

