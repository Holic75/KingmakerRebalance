using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
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
        static void createQuadrupedFireElementalUnit()
        {
            var fx_buff = Helpers.CreateBuff("QuadrupedFireElementalEidolonFxBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             Common.createPrefabLink("f5eaec10b715dbb46a78890db41fa6a0"));
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);

            var fx_feature = Helpers.CreateFeature("QuadrupedFireElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Common.createAuraFeatureComponent(fx_buff));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var fire_elemental = library.Get<BlueprintUnit>("37b3eb7ca48264247b3247c732007aef");
            var fire_elemental_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "QuadrupedFireElementalEidolonUnit", "");
            fire_elemental_unit.Color = fire_elemental.Color;

            fire_elemental_unit.Visual = fire_elemental.Visual;
            fire_elemental_unit.LocalizedName = fire_elemental_unit.LocalizedName.CreateCopy();
            fire_elemental_unit.LocalizedName.String = Helpers.CreateString(fire_elemental_unit.name + ".Name", "Elemental Eidolon (Fire)");

            fire_elemental_unit.Strength = 14;
            fire_elemental_unit.Dexterity = 14;
            fire_elemental_unit.Constitution = 13;
            fire_elemental_unit.Intelligence = 7;
            fire_elemental_unit.Wisdom = 10;
            fire_elemental_unit.Charisma = 11;
            fire_elemental_unit.Speed = 40.Feet();
            fire_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            fire_elemental_unit.Body = fire_elemental_unit.Body.CloneObject();
            fire_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            fire_elemental_unit.Body.PrimaryHand = library.Get<BlueprintItemWeapon>("a000716f88c969c499a535dadcf09286"); //bite 1d6
            fire_elemental_unit.Body.SecondaryHand = null;
            fire_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            fire_elemental_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[] { quadruped_archetype};
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillStealth, StatType.SkillLoreReligion};
                a.DoNotApplyAutomatically = true;
                a.Selections = new SelectionEntry[0];
            });
            fire_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(fire_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonQuadrupedFireElementalProtrait", "FireElementalQuadruped", ""));

            fire_quadruped_eidolon = Helpers.CreateProgression("QuadrupedFireElementalEidolonProgression",
                                                        "Elemental Eidolon (Fire, Quadruped)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            fire_quadruped_eidolon.IsClassFeature = true;
            fire_quadruped_eidolon.ReapplyOnLevelUp = true;
            fire_quadruped_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            fire_quadruped_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            fire_quadruped_eidolon.ReplaceComponent<AddPet>(a => a.Pet = fire_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(fire_quadruped_eidolon);
        }


        static void createFireElementalUnit()
        {
            var fx_buff = Helpers.CreateBuff("FireElementalEidolonFxBuff",
                                             "",
                                             "",
                                             "",
                                             null,
                                             Common.createPrefabLink("f5eaec10b715dbb46a78890db41fa6a0"));
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);

            var fx_feature = Helpers.CreateFeature("FireElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("7cc1c50366f08814eb5a5e7c47c71a2a")),
                                                   Common.createAuraFeatureComponent(fx_buff));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var fire_elemental = library.Get<BlueprintUnit>("37b3eb7ca48264247b3247c732007aef");
            var fire_elemental_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "FireElementalEidolonUnit", "");
            fire_elemental_unit.Color = fire_elemental.Color;

            fire_elemental_unit.Visual = fire_elemental.Visual;
            fire_elemental_unit.LocalizedName = fire_elemental_unit.LocalizedName.CreateCopy();
            fire_elemental_unit.LocalizedName.String = Helpers.CreateString(fire_elemental_unit.name + ".Name", "Elemental Eidolon (Fire)");

            fire_elemental_unit.Strength = 16;
            fire_elemental_unit.Dexterity = 12;
            fire_elemental_unit.Constitution = 13;
            fire_elemental_unit.Intelligence = 7;
            fire_elemental_unit.Wisdom = 10;
            fire_elemental_unit.Charisma = 11;
            fire_elemental_unit.Speed = 30.Feet();
            fire_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            fire_elemental_unit.Body = fire_elemental_unit.Body.CloneObject();
            fire_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            fire_elemental_unit.Body.PrimaryHand = null;
            fire_elemental_unit.Body.SecondaryHand = null;
            fire_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            fire_elemental_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillPerception, StatType.SkillLoreReligion, StatType.SkillStealth };
                a.DoNotApplyAutomatically = true;
                a.Selections = new SelectionEntry[0];
            });
            fire_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>());


            Helpers.SetField(fire_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonFireElementalProtrait", "FireElemental", ""));

            fire_elemental_eidolon = Helpers.CreateProgression("FireElementalEidolonProgression",
                                                        "Elemental Eidolon (Fire)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            fire_elemental_eidolon.IsClassFeature = true;
            fire_elemental_eidolon.ReapplyOnLevelUp = true;
            fire_elemental_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            fire_elemental_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            fire_elemental_eidolon.ReplaceComponent<AddPet>(a => a.Pet = fire_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(fire_elemental_eidolon);
        }


        static void fillFireElementalProgression()
        {
            var feature1 = Helpers.CreateFeature("FireElementalEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, fire elemental eidolons gain immunity to paralysis and sleep and the slam and immunity (fire) evolutions.",
                                                  "",
                                                  fire_elemental_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("QuadrupedFireElementalEidolonLevel1AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                                Common.createAddConditionImmunity(UnitCondition.Sleeping)
                                                                                ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.slam_biped),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[3])
                                                  );

            var feature1q = Helpers.CreateFeature("QuadrupedFireElementalEidolonLevel1Feature",
                                      "Base Evolutions",
                                      "At 1st level, fire elemental eidolons gain immunity to paralysis and sleep and the bite and immunity (fire) evolutions.",
                                      "",
                                      fire_elemental_eidolon.Icon,
                                      FeatureGroup.None,
                                      addTransferableFeatToEidolon("FireElementalEidolonLevel1AddFeature",
                                                                    Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                    Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                    Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                    Common.createAddConditionImmunity(UnitCondition.Sleeping)
                                                                    ),
                                      Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = Evolutions.bite),
                                      Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[3])
                                      );

            var feature4 = Helpers.CreateFeature("FireElementalEidolonLevel4Feature",
                                                  "Evolution Pool Increase",
                                                  "At 4th level, all elemental eidolons add 1 point to their evolution pools.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(i => i.amount = 1)
                                                  );

            var feature8 = Helpers.CreateFeature("FireElementalEidolonLevel8Feature",
                                                  "Speed Increase",
                                                  "At 8th level, fire elemental eidolons increase their speed by 20 feet.",
                                                  "",
                                                  Helpers.GetIcon("486eaff58293f6441a5c2759c4872f98"), //haste
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("FireElementalEidolonLevel8AddFeature",
                                                                                Helpers.CreateAddStatBonus(StatType.Speed, 20, ModifierDescriptor.UntypedStackable)
                                                                                )
                                                  );

            var feature12 = Helpers.CreateFeature("FireElementalEidolonLevel12Feature",
                                                  "Immunity",
                                                  "At 12th level, all elemental eidolons gain immunity to bleed, poison, and stun. In addition, they can no longer be flanked.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("FireElementalEidolonLevel12AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createAddConditionImmunity(UnitCondition.Stunned),
                                                                                Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.CannotBeFlanked)
                                                                                )
                                                  );

            var feature16 = Helpers.CreateFeature("FireElementalEidolonLevel16Feature",
                                                  "Immunity II",
                                                  "At 16th level, all elemental eidolons gain immunity to critical hits and do not take additional damage from precision-based attacks, such as sneak attack.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("FireElementalEidolonLevel16AddFeature",
                                                                                Helpers.Create<AddImmunityToPrecisionDamage>(),
                                                                                Helpers.Create<AddImmunityToCriticalHits>()
                                                                                )
                                                  );
           
            transferable_abilities.Add(library.Get<BlueprintFeature>("3b423b497934aeb48a3676cca64b5b55"));
            var feature20 = Helpers.CreateFeature("FireElementalEidolonLevel20Feature",
                                                  "Burn",
                                                  "At 20th level, fire elemental eidolons gain the energy attacks (fire) evolution and the burn ability as large fire elementals.",
                                                  "",
                                                  Helpers.GetIcon("c419858ccdf548c46aeed236051776b4"), //burn buff
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.energy_attacks[3]),
                                                  Helpers.Create<AddFeatureToCompanion>(a => a.Feature = library.Get<BlueprintFeature>("3b423b497934aeb48a3676cca64b5b55"))
                                                  );

            fire_elemental_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            fire_elemental_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);

            fire_quadruped_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1q),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            fire_quadruped_eidolon.UIGroups = Helpers.CreateUIGroups(feature1q, feature4, feature8, feature12, feature16, feature20);
        }
    }
}
