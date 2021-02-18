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
    public class DervishOfDawn
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature burst_of_speed;
        static public BlueprintFeature desert_stride;
        static public BlueprintFeature rapid_attack;
        static public BlueprintFeature lightning_strike;

        static BlueprintCharacterClass fighter_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DervishOfDawnArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Dawnflower Dervish");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "In Qadira, home of the whirlwind, the scorpion, and the djinni, no enemy is as feared as the dervishes of Sarenrae. While dervishes can be clerics, paladins, or rangers, zealous fighters join their ranks as well. These spinning warriors prefer light or no armor and wield scimitars with devastating consequences, moving swiftly over the treacherous desert sands to attack with lightning-fast strikes. They maneuver quickly among their enemies, relying on their speed and their skill to see them through the battle.");
            });
            Helpers.SetField(archetype, "m_ParentClass", fighter_class);
            library.AddAsset(archetype, "");

            createBurstOfSpeed();
            createDesertStride();
            createRapidAttack();
            createLightningStrike();

            var deity_selection = library.CopyAndAdd<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4", "DawnflowerDervishDeitySelection", "");
            deity_selection.AllFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("c1c4f7f64842e7e48849e5e67be11a1b") };//sarenrae
            deity_selection.SetNameDescription("Sarenrae", "A dawnflower dervish must be lawful good, neutral good, chaotic good or neutral, and must worship Sarenrae.");
            deity_selection.RemoveComponents<NoSelectionIfAlreadyHasFeature>();

            var armor_training = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");

            var advanced_armor_training = AdvancedFighterOptions.advanced_armor_training ?? armor_training;

            archetype.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(3, armor_training),
                                                         Helpers.LevelEntry(7, advanced_armor_training),
                                                         Helpers.LevelEntry(11, advanced_armor_training),
                                                         Helpers.LevelEntry(15, advanced_armor_training)
                                                       };

            archetype.AddFeatures = new LevelEntry[] {  Helpers.LevelEntry(1, deity_selection),
                                                          Helpers.LevelEntry(3, burst_of_speed),
                                                          Helpers.LevelEntry(7, desert_stride),
                                                          Helpers.LevelEntry(11, rapid_attack),
                                                          Helpers.LevelEntry(15, lightning_strike)
                                                       };

            fighter_class.Progression.UIGroups = fighter_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(burst_of_speed, desert_stride, rapid_attack, lightning_strike));
            fighter_class.Archetypes = fighter_class.Archetypes.AddToArray(archetype);
        }


        static void createBurstOfSpeed()
        {
            burst_of_speed = Helpers.CreateFeature("DawnflowerDervishBurstOfSpeedFeature",
                                                   "Burst of Speed",
                                                   "At 3rd level, a Dawnflower dervish takes only a –1 penalty to her AC after charging. At 7th level, the Dawnflower dervish can charge with no penalty.",
                                                   "",
                                                   Helpers.GetIcon("c78506dd0e14f7c45a599990e4e65038"), //charge
                                                   FeatureGroup.None);

            var charge_buff = library.Get<BlueprintBuff>("f36da144a379d534cad8e21667079066");

            charge_buff.AddComponent(Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.UntypedStackable));
            charge_buff.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.SummClassLevelWithArchetype, ContextRankProgression.DelayedStartPlusDivStep,
                                                                     classes: new BlueprintCharacterClass[] { archetype.GetParentClass() }, archetype: archetype,
                                                                     max: 2, stepLevel: 4, startLevel: 3)
                                                                     );
        }


        static void createDesertStride()
        {
            desert_stride = library.CopyAndAdd<BlueprintFeature>("11f4072ea766a5840a46e6660894527d", "DawnflowerDervishDesertStride", ""); //woodland stride
            desert_stride.SetNameDescription("Desert Stride", "At 7th level, a Dawnflower dervish can move through any sort difficult terrain at her normal speed and without taking damage or suffering any other impairment.");
        }


        static void createRapidAttack()
        {
            var buff = Helpers.CreateBuff("DawnflowerDervishRapidAttackBuff",
                                          "Rapid Attack",
                                          "At 11th level, a Dawnflower dervish can make a full attack after charge. She must forgo the attack at her highest bonus.",
                                          "",
                                          Helpers.GetIcon("85742dd6788c6914f96ddc4628b23932"), //arcane weapon speed
                                          null,
                                          Common.createBuffExtraAttack(-1, false),
                                          Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.Pounce)
                                          );

            var toggle = Common.buffToToggle(buff, CommandType.Free, deactivate_immediately: true);
            rapid_attack = Common.ActivatableAbilityToFeature(toggle, false);
        }


        static void createLightningStrike()
        {
            var buff = Helpers.CreateBuff("DawnflowerDervishLightningStrikeBuff",
                                          "Lightning Strike",
                                          "At 15th level, as part of a full attack, a Dawnflower dervish can make one additional attack. This attack is at the dervish’s highest base attack bonus, but each attack in the round (including the extra one) takes a –2 penalty.",
                                          "",
                                          Helpers.GetIcon("a3a9e9a2f909cd74e9aee7788a7ec0c6"), //arcane wepon shock
                                          null,
                                          Common.createBuffExtraAttack(1, false),
                                          Helpers.Create<WeaponParametersAttackBonus>(w => { w.Ranged = true; w.AttackBonus = -2; }),
                                          Helpers.Create<WeaponParametersAttackBonus>(w => { w.Ranged = false; w.AttackBonus = -2; })
                                          );

            var toggle = Common.buffToToggle(buff, CommandType.Free, deactivate_immediately: true);
            lightning_strike = Common.ActivatableAbilityToFeature(toggle, false);
        }
    }
}
