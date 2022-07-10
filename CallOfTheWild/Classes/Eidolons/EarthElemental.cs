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
        static void createQuadrupedEarthElementalUnit()
        {
            var fx_buff = Helpers.CreateBuff("QuadrupedEarthElementalEidolonFxBuff",
                                 "",
                                 "",
                                 "",
                                 null,
                                 Common.createPrefabLink("9d0ceb7173b10774db39bafb14042710"));
            fx_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var fx_feature = Helpers.CreateFeature("QuadrupedEarthElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<SizeMechanics.PermanentSizeOverride>(p => p.size = Size.Medium));
                                                  // Common.createAuraFeatureComponent(fx_buff));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var earth_elemental_unit = library.CopyAndAdd<BlueprintUnit>("eab864d9ca3415644a792792fd81bf87", "QuadrupedEarthElementalEidolonUnit", ""); //wolf

            earth_elemental_unit.Prefab = Common.createUnitViewLink("46b4f98ae7d8ddf40878914741a42129");
            earth_elemental_unit.LocalizedName = earth_elemental_unit.LocalizedName.CreateCopy();
            earth_elemental_unit.LocalizedName.String = Helpers.CreateString(earth_elemental_unit.name + ".Name", "Elemental Eidolon (Earth)");
            earth_elemental_unit.Size = Size.Huge;
            earth_elemental_unit.Strength = 14;
            earth_elemental_unit.Dexterity = 14;
            earth_elemental_unit.Constitution = 13;
            earth_elemental_unit.Intelligence = 7;
            earth_elemental_unit.Wisdom = 10;
            earth_elemental_unit.Charisma = 11;
            earth_elemental_unit.Speed = 30.Feet();
            earth_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature }; // { natural_armor2, fx_feature };
            earth_elemental_unit.Body = earth_elemental_unit.Body.CloneObject();
            earth_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            earth_elemental_unit.Body.PrimaryHand = library.Get<BlueprintItemWeapon>("a000716f88c969c499a535dadcf09286"); //bite 1d6
            earth_elemental_unit.Body.SecondaryHand = null;
            earth_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            earth_elemental_unit.Body.AdditionalSecondaryLimbs = new BlueprintItemWeapon[0];
            earth_elemental_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[] {Eidolon.quadruped_archetype};
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillStealth };
                a.DoNotApplyAutomatically = true;
                a.Selections = new SelectionEntry[0];
            });
            earth_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>());

            Helpers.SetField(earth_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonQuadrupedEarthElementalProtrait", "EarthElementalQuadruped", ""));

            earth_quadruped_eidolon = Helpers.CreateProgression("QuadrupedEarthElementalEidolonProgression",
                                                        "Elemental Eidolon (Earth, Quadruped)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            earth_quadruped_eidolon.IsClassFeature = true;
            earth_quadruped_eidolon.ReapplyOnLevelUp = true;
            earth_quadruped_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            earth_quadruped_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            earth_quadruped_eidolon.ReplaceComponent<AddPet>(a => a.Pet = earth_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(earth_quadruped_eidolon);;
            addLesserEidolon(earth_quadruped_eidolon);
        }


        static void createEarthElementalUnit()
        {
            var fx_feature = Helpers.CreateFeature("EarthElementalEidolonFxFeature",
                                                   "",
                                                   "",
                                                   "",
                                                   null,
                                                   FeatureGroup.None,
                                                   Helpers.Create<UnitViewMechanics.ReplaceUnitView>(r => r.prefab = Common.createUnitViewLink("f59e51021e055b1459b0260a76cc4e54")), //stone golem
                                                   Helpers.Create<SizeMechanics.PermanentSizeOverride>(p => p.size = Size.Medium));
            fx_feature.HideInCharacterSheetAndLevelUp = true;
            fx_feature.HideInUI = true;

            var natural_armor2 = library.Get<BlueprintUnitFact>("45a52ce762f637f4c80cc741c91f58b7");
            var earth_elemental = library.Get<BlueprintUnit>("11d8e4b048acc0e4c8e42e76b8ab869d");
            var earth_elemental_unit = library.CopyAndAdd<BlueprintUnit>("8a6986e17799d7d4b90f0c158b31c5b9", "EarthElementalEidolonUnit", "");
            var visual = library.Get<BlueprintBuff>("40aaecaebce743e48a1d35957583fcc6");
            visual.SetBuffFlags(visual.GetBuffFlags() | BuffFlags.HiddenInUi);
            earth_elemental_unit.Color = earth_elemental.Color;

            earth_elemental_unit.Visual = earth_elemental.Visual;
            earth_elemental_unit.LocalizedName = earth_elemental_unit.LocalizedName.CreateCopy();
            earth_elemental_unit.LocalizedName.String = Helpers.CreateString(earth_elemental_unit.name + ".Name", "Elemental Eidolon (Earth)");

            earth_elemental_unit.Strength = 16;
            earth_elemental_unit.Dexterity = 12;
            earth_elemental_unit.Constitution = 13;
            earth_elemental_unit.Intelligence = 7;
            earth_elemental_unit.Wisdom = 10;
            earth_elemental_unit.Charisma = 11;
            earth_elemental_unit.Speed = 30.Feet();
            earth_elemental_unit.AddFacts = new BlueprintUnitFact[] { natural_armor2, fx_feature, visual }; // { natural_armor2, fx_feature };
            earth_elemental_unit.Body = earth_elemental_unit.Body.CloneObject();
            earth_elemental_unit.Body.EmptyHandWeapon = library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            earth_elemental_unit.Body.PrimaryHand = null;
            earth_elemental_unit.Body.SecondaryHand = null;
            earth_elemental_unit.Body.AdditionalLimbs = new BlueprintItemWeapon[0];
            earth_elemental_unit.Body.AdditionalSecondaryLimbs = new BlueprintItemWeapon[0];
            earth_elemental_unit.ReplaceComponent<AddClassLevels>(a =>
            {
                a.Archetypes = new BlueprintArchetype[0];
                a.CharacterClass = eidolon_class;
                a.Skills = new StatType[] { StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillStealth };
                a.DoNotApplyAutomatically = true;
                a.Selections = new SelectionEntry[0];
            });
            earth_elemental_unit.AddComponents(Helpers.Create<EidolonComponent>(),
                                               Helpers.Create<UnitViewMechanics.ChangeUnitScaleForInventory>(c => c.scale_factor = 1.51f));
            earth_elemental_unit.Size = Size.Large;

            Helpers.SetField(earth_elemental_unit, "m_Portrait", Helpers.createPortrait("EidolonEarthElementalProtrait", "EarthElemental", ""));

            earth_elemental_eidolon = Helpers.CreateProgression("EarthElementalEidolonProgression",
                                                        "Elemental Eidolon (Earth)",
                                                        "Pulled in from one of the four elemental planes, these eidolons are linked to one of the four elements: air, earth, fire, or water. Generally, an elemental eidolon appears as a creature made entirely of one element, but there is some variation. Elemental eidolons are decidedly moderate in their views and actions. They tend to avoid the conflicts of others when they can and seek to maintain balance. The only exception is when facing off against emissaries of their opposing elements, which they hate utterly.",
                                                        "",
                                                        Helpers.GetIcon("650f8c91aaa5b114db83f541addd66d6"), //summon elemental
                                                        FeatureGroup.AnimalCompanion,
                                                        library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
                                                        );
            earth_elemental_eidolon.IsClassFeature = true;
            earth_elemental_eidolon.ReapplyOnLevelUp = true;
            earth_elemental_eidolon.Classes = new BlueprintCharacterClass[] { Summoner.summoner_class };
            earth_elemental_eidolon.AddComponent(Common.createPrerequisiteAlignment(Kingmaker.UnitLogic.Alignments.AlignmentMaskType.ChaoticNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.LawfulNeutral
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralEvil
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.NeutralGood
                                                                                   | Kingmaker.UnitLogic.Alignments.AlignmentMaskType.TrueNeutral));
            earth_elemental_eidolon.ReplaceComponent<AddPet>(a => a.Pet = earth_elemental_unit);
            Summoner.eidolon_selection.AllFeatures = Summoner.eidolon_selection.AllFeatures.AddToArray(earth_elemental_eidolon);
            addLesserEidolon(earth_elemental_eidolon);
        }


        static void fillEarthElementalProgression()
        {
            var feature1 = Helpers.CreateFeature("EarthElementalEidolonLevel1Feature",
                                                  "Base Evolutions",
                                                  "At 1st level, earth elemental eidolons gain immunity to paralysis and sleep and the slam and immunity (acid) evolutions.",
                                                  "",
                                                  earth_elemental_eidolon.Icon,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("EarthElementalEidolonLevel1AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                                Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                                Common.createAddConditionImmunity(UnitCondition.Sleeping)
                                                                                ),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.slam_biped),
                                                  Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[0])
                                                  );

            var feature1q = Helpers.CreateFeature("QuadrupedEarthElementalEidolonLevel1Feature",
                                      "Base Evolutions",
                                      "At 1st level, earth elemental eidolons gain immunity to paralysis and sleep and the bite and immunity (acid) evolutions.",
                                      "",
                                      earth_quadruped_eidolon.Icon,
                                      FeatureGroup.None,
                                      addTransferableFeatToEidolon("QuadrupedEarthElementalEidolonLevel1AddFeature",
                                                                    Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                    Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis | SpellDescriptor.Sleep),
                                                                    Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                                                                    Common.createAddConditionImmunity(UnitCondition.Sleeping)
                                                                    ),
                                      Helpers.Create<EvolutionMechanics.AddFakeEvolution>(a => a.Feature = Evolutions.bite),
                                      Helpers.Create<EvolutionMechanics.AddPermanentEvolution>(a => a.Feature = Evolutions.immunity[0])
                                      );

            var feature4 = Helpers.CreateFeature("EarthElementalEidolonLevel4Feature",
                                                  "Evolution Pool Increase",
                                                  "At 4th level, all elemental eidolons add 1 point to their evolution pools.",
                                                  "",
                                                  null,
                                                  FeatureGroup.None,
                                                  Helpers.Create<EvolutionMechanics.IncreaseEvolutionPool>(i => i.amount = 1)
                                                  );
            transferable_abilities.Add(library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8"));
            var feature8 = Helpers.CreateFeature("EarthElementalEidolonLevel8Feature",
                                                  "Combat Maneuver Immunity",
                                                  "At 8th level, earth elemental eidolons gain immunity to combat maneuvers.",
                                                  "",
                                                  Helpers.GetIcon("737ef897849327b45b88b83a797918c8"), //freedom of movement
                                                  FeatureGroup.None,
                                                  Helpers.Create<AddFeatureToCompanion>(a => a.Feature = library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8"))
                                                  );

            var feature12 = Helpers.CreateFeature("EarthElementalEidolonLevel12Feature",
                                                  "Immunity",
                                                  "At 12th level, all elemental eidolons gain immunity to bleed, poison, and stun. In addition, they can no longer be flanked.",
                                                  "",
                                                  Helpers.GetIcon("21ffef7791ce73f468b6fca4d9371e8b"), //resist energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("EarthElementalEidolonLevel12AddFeature",
                                                                                Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createBuffDescriptorImmunity(SpellDescriptor.Poison | SpellDescriptor.Bleed | SpellDescriptor.Stun),
                                                                                Common.createAddConditionImmunity(UnitCondition.Stunned),
                                                                                Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.CannotBeFlanked)
                                                                                )
                                                  );

            var feature16 = Helpers.CreateFeature("EarthElementalEidolonLevel16Feature",
                                                  "Immunity II",
                                                  "At 16th level, all elemental eidolons gain immunity to critical hits and do not take additional damage from precision-based attacks, such as sneak attack.",
                                                  "",
                                                  Helpers.GetIcon("d2f116cfe05fcdd4a94e80143b67046f"), //protection from energy,
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("EarthElementalEidolonLevel16AddFeature",
                                                                                Helpers.Create<AddImmunityToPrecisionDamage>(),
                                                                                Helpers.Create<AddImmunityToCriticalHits>()
                                                                                )
                                                  );

            var feature20 = Helpers.CreateFeature("EarthElementalEidolonLevel20Feature",
                                                  "Damage Reduction",
                                                  "At 20th level, earth elemental eidolons gain DR 5/- and +1 bonus to its attack, damage rolls, and combat maneuvers.",
                                                  "",
                                                  Helpers.GetIcon("c66e86905f7606c4eaa5c774f0357b2b"),
                                                  FeatureGroup.None,
                                                  addTransferableFeatToEidolon("EarthElementalEidolonLevel20AddFeature",
                                                                                Common.createContextPhysicalDR(Helpers.CreateContextValue(AbilityRankType.Default)),
                                                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureList,
                                                                                                                progression: ContextRankProgression.MultiplyByModifier,
                                                                                                                stepLevel: 10,
                                                                                                                min: 5,
                                                                                                                featureList: new BlueprintFeature[] { Evolutions.damage_reduction }
                                                                                                                ),
                                                                                Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 1, ModifierDescriptor.UntypedStackable),
                                                                                Helpers.CreateAddStatBonus(StatType.AdditionalDamage, 1, ModifierDescriptor.UntypedStackable),
                                                                                Helpers.CreateAddStatBonus(StatType.AdditionalCMB, 1, ModifierDescriptor.UntypedStackable),
                                                                                Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[] { Evolutions.damage_reduction })
                                                                                )
                                                  );

            earth_elemental_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            earth_elemental_eidolon.UIGroups = Helpers.CreateUIGroups(feature1, feature4, feature8, feature12, feature16, feature20);

            earth_quadruped_eidolon.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, feature1q),
                                                           Helpers.LevelEntry(4, feature4),
                                                           Helpers.LevelEntry(8, feature8),
                                                           Helpers.LevelEntry(12, feature12),
                                                           Helpers.LevelEntry(16, feature16),
                                                           Helpers.LevelEntry(20, feature20)
                                                           };
            earth_quadruped_eidolon.UIGroups = Helpers.CreateUIGroups(feature1q, feature4, feature8, feature12, feature16, feature20);
            setLesserEidolonProgression(earth_elemental_eidolon);
            setLesserEidolonProgression(earth_quadruped_eidolon);
        }
    }
}
