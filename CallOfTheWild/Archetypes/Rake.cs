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
    public class Rake
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeature rakes_smile;
        static public BlueprintFeature[] bravados_blade = new BlueprintFeature[5];

        static LibraryScriptableObject library => Main.library;


        static BlueprintCharacterClass[] getRogueArray()
        {
            var rogue_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");
            return new BlueprintCharacterClass[] { rogue_class };
        }

        static public void create()
        {
            var rogue_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "Rake";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Rake");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The rake is a rogue who is open about her skills and talents, often to the point of being boastful. Usually she has the protection of an important figure who finds her services useful, but sometimes her bravado is enough to keep enemies away. She is often used as a face for the group for diplomacy, gathering information, negotiations, or to gain the most lucrative contracts and quests from local authorities.");
            });
            Helpers.SetField(archetype, "m_ParentClass", rogue_class);
            library.AddAsset(archetype, "");


            var trapfinding = library.Get<BlueprintFeature>("dbb6b3bffe6db3547b31c3711653838e");
            var danger_sense = library.Get<BlueprintFeature>("0bcbe9e450b0e7b428f08f66c53c5136");


            createRakesSmile();
            createBravadosBlade();

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, trapfinding),
                                                          Helpers.LevelEntry(3, danger_sense),
                                                          Helpers.LevelEntry(6, danger_sense),
                                                          Helpers.LevelEntry(9, danger_sense),
                                                          Helpers.LevelEntry(12, danger_sense),
                                                          Helpers.LevelEntry(15, danger_sense),
                                                          Helpers.LevelEntry(18, danger_sense),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, bravados_blade[0]),
                                                       Helpers.LevelEntry(3, rakes_smile),
                                                       Helpers.LevelEntry(5, bravados_blade[1]),
                                                       Helpers.LevelEntry(6, rakes_smile),
                                                       Helpers.LevelEntry(9, bravados_blade[2], rakes_smile),
                                                       Helpers.LevelEntry(12, rakes_smile),
                                                       Helpers.LevelEntry(13, bravados_blade[3]),
                                                       Helpers.LevelEntry(15, rakes_smile),
                                                       Helpers.LevelEntry(17, bravados_blade[4]),
                                                       Helpers.LevelEntry(18, rakes_smile),
                                                       };

            rogue_class.Progression.UIGroups = rogue_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(bravados_blade));
            rogue_class.Archetypes = rogue_class.Archetypes.AddToArray(archetype);
        }

        static void createRakesSmile()
        {
            rakes_smile = Helpers.CreateFeature("RakesSmileFeature",
                                                "Rake’s Smile",
                                                "At 3rd level, a rake gains a +1 morale bonus on Bluff and Diplomacy checks. This bonus increases by +1 for every 3 levels beyond 3rd.",
                                                "",
                                                Helpers.GetIcon("fd4d9fd7f87575d47aafe2a64a6e2d8d"), //hideous laughter
                                                FeatureGroup.None,
                                                Helpers.CreateAddContextStatBonus(StatType.CheckDiplomacy, ModifierDescriptor.Morale),
                                                Helpers.CreateAddContextStatBonus(StatType.CheckBluff, ModifierDescriptor.Morale)
                                               );
            rakes_smile.Ranks = 6;
            rakes_smile.ReapplyOnLevelUp = true;
            rakes_smile.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, feature: rakes_smile));
        }


        static void createBravadosBlade()
        {
            
            var demoralize = library.Get<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb").GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as NewMechanics.DemoralizeWithAction;
            for (int i = 0; i < 5; i++)
            {
                var buff_sneak = Helpers.CreateBuff($"BravadosBladeReduceSneak{i}Buff",
                                                      "",
                                                      "",
                                                      "",
                                                      null,
                                                      null,
                                                      Helpers.CreateAddStatBonus(StatType.SneakAttack, -(i + 1), ModifierDescriptor.UntypedStackable)
                                                      );
                buff_sneak.SetBuffFlags(BuffFlags.HiddenInUi);
                var new_demoralize = demoralize.CreateCopy(d => d.bonus = i * 5);
                var action_on_hit = Helpers.CreateActionList(Common.createContextActionApplyBuffToCaster(buff_sneak, Helpers.CreateContextDuration(1), dispellable: false),
                                                             demoralize.CreateCopy(d => d.bonus = i * 5));

                var buff = Helpers.CreateBuff($"BravadosBlade{i}Buff",
                                              $"Bravado's Blade ({Common.roman_id[i + 1]})",
                                              "When a rake hits an opponent and deals sneak attack damage, she can forgo 1d6 points of that damage and make a free Intimidate check to demoralize the foe. Starting from level 5 she can decide to forgo 1 additional sneak attack die + 1 more die per 4 levels thereafter. For every additional 1d6 points of sneak attack damage she forgoes, she receives a +5 circumstance bonus on this check.",
                                              "",
                                              Helpers.GetIcon("bd81a3931aa285a4f9844585b5d97e51"),
                                              null,
                                              Common.createAddInitiatorAttackRollTrigger2(action_on_hit, sneak_attack: true),
                                              Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(Common.createContextActionRemoveBuff(buff_sneak)),
                                                                                               false,
                                                                                               wait_for_attack_to_resolve: true,
                                                                                               on_initiator: true
                                                                                               )
                                              );
                var toggle = Helpers.CreateActivatableAbility($"BravadosBlade{i}ToggleAbility",
                                                               buff.Name,
                                                               buff.Description,
                                                               "",
                                                               buff.Icon,
                                                               buff,
                                                               AbilityActivationType.Immediately,
                                                               CommandType.Free,
                                                               null);
                toggle.DeactivateImmediately = true;
                toggle.Group = ActivatableAbilityGroupExtension.BravadosBlade.ToActivatableAbilityGroup();

                bravados_blade[i] = Common.ActivatableAbilityToFeature(toggle, false);
            }
        }
    }

}
