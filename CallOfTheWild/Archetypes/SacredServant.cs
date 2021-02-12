using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
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
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
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
    public class SacredServant
    {
        static public BlueprintArchetype archetype;

        static public BlueprintFeatureSelection domain_selection;
        static public BlueprintFeature[] divine_bond = new BlueprintFeature[6];
        static public BlueprintAbility divine_bond_ability;
        static public BlueprintBuff divine_bond_buff;
        static public BlueprintFeature call_celestial_ally;
        static public BlueprintFeatureSelection paladin_deity;

        static LibraryScriptableObject library => Main.library;
       


        internal static void create()
        {
            var paladin_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");


            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SacredServantArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Sacred Servant");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Paladins as a general rule, venerate the gods of good and purity, but some take this a step further, dedicating themselves to a specific deity and furthering the cause of the faith. These sacred servants are rewarded for their devotion with additional spells and powerful allies.");
            });
            Helpers.SetField(archetype, "m_ParentClass", paladin_class);
            library.AddAsset(archetype, "");

            createDomainSelection();
            createDivineBond();
            createCallCelestialAlly();

            var smite_evil_extra = library.Get<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137");
            var weapon_bond_feature = library.Get<BlueprintFeature>("1c7cdc1605554954f838d85bbdd22d90");
            var weapon_bond_feature2 = library.Get<BlueprintFeature>("c8db0772b7059ec4eabe55b7e0e79824");
            var weapon_bond_feature3 = library.Get<BlueprintFeature>("d2f45a2034d4f7643ba1a450bc5c4c06");
            var weapon_bond_feature4 = library.Get<BlueprintFeature>("6d73f49b602e29a43a6faa2ea1e4a425");
            var weapon_bond_feature5 = library.Get<BlueprintFeature>("f17c3ba33bb44d44782cb3851d823011");
            var weapon_bond_feature6 = library.Get<BlueprintFeature>("b936ee90c070edb46bd76025dc1c5936");
            var weapon_bond_extra_use = library.Get<BlueprintFeature>("5a64de5435667da4eae2e4c95ec87917");
            var aura_of_resolve = library.Get<BlueprintFeature>("a28693b24cc412c478b8b85877f2dad2");

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, paladin_deity),
                                                          Helpers.LevelEntry(4, smite_evil_extra),
                                                          Helpers.LevelEntry(5,  weapon_bond_feature),
                                                          Helpers.LevelEntry(8, aura_of_resolve, weapon_bond_feature2),
                                                          Helpers.LevelEntry(9, weapon_bond_extra_use),
                                                          Helpers.LevelEntry(10, smite_evil_extra),
                                                          Helpers.LevelEntry(11,  weapon_bond_feature3),
                                                          Helpers.LevelEntry(13, weapon_bond_extra_use),
                                                          Helpers.LevelEntry(14,  weapon_bond_feature4),
                                                          Helpers.LevelEntry(16, smite_evil_extra),
                                                          Helpers.LevelEntry(17,  weapon_bond_feature5, weapon_bond_extra_use),
                                                          Helpers.LevelEntry(20,  weapon_bond_feature6),
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, paladin_deity, domain_selection),
                                                       Helpers.LevelEntry(5, divine_bond[0]),
                                                       Helpers.LevelEntry(8, divine_bond[1], call_celestial_ally),
                                                       Helpers.LevelEntry(11, divine_bond[2]),
                                                       Helpers.LevelEntry(14, divine_bond[3]),
                                                       Helpers.LevelEntry(17, divine_bond[4]),
                                                       Helpers.LevelEntry(20, divine_bond[5]),
                                                     };

            paladin_class.Progression.UIDeterminatorsGroup = paladin_class.Progression.UIDeterminatorsGroup.AddToArray(domain_selection);
            paladin_class.Progression.UIGroups[0].Features.AddRange(divine_bond);
            paladin_class.Progression.UIGroups[2].Features.Add(call_celestial_ally);
            paladin_class.Archetypes = paladin_class.Archetypes.AddToArray(archetype);
        }





        static void createDomainSelection()
        {
            paladin_deity = library.Get<BlueprintFeatureSelection>("a7c8b73528d34c2479b4bd638503da1d");
            paladin_deity.Group = FeatureGroup.Deities;
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var cleric_domain = library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");
            domain_selection = library.CopyAndAdd(cleric_domain, "SacredServantDomainSelection", "");
            ClassToProgression.addClassToDomains(archetype.GetParentClass(), new BlueprintArchetype[] { archetype }, ClassToProgression.DomainSpellsType.SpecialList, domain_selection, cleric);
            domain_selection.SetDescription("Sacred Servant chooses one domain associated with her deity. At 4th level she also gains one domain spell slot for each level of paladin spells she can cast. Every day she must prepare the domain spell from her chosen domain in that spell slot.");
            archetype.GetParentClass().Spellbook.CantripsType = CantripsType.Orisions; //to properly show domain slots
        }


        static void createCallCelestialAlly()
        {
            var resource = Helpers.CreateAbilityResource("SacredServantCelestialAllyResource", "", "", "", null);
            resource.SetFixedResource(1);
            var summons = new BlueprintAbility[]
            {
             library.Get<BlueprintAbility>("0964bf88b582bed41b74e79596c4f6d9"),//sm v
             library.Get<BlueprintAbility>("eb6df7ddfc0669d4fb3fc9af4bd34bca"),//sm vii
             library.Get<BlueprintAbility>("e96593e67d206ab49ad1b567327d1e75")//sm ix
            };

            var summon_actions = new List<ActionList>();

            foreach (var s in summons)
            {
                var sa = s.GetComponent<AbilityEffectRunAction>().Actions.Actions;
                sa = Common.changeAction<ContextActionSpawnMonster>(sa, c => c.DurationValue = Helpers.CreateContextDuration(c.DurationValue.BonusValue, DurationRate.Minutes, c.DurationValue.DiceType, c.DurationValue.DiceCountValue));
                summon_actions.Add(Helpers.CreateActionList(sa));
            }


            var ability = Helpers.CreateAbility("SacredServantCallCelestialAllyAbility",
                                                "Call Celestial Ally",
                                                "At 8th level, a sacred servant can call upon her deity for aid, in the form of a powerful servant. This allows the sacred servant to bralani azata once per day as a spell-like ability for 1 minute per sacred servant level. At 12th level, she can summon movanic deva instead. Finally, at 16th level, a sacred servant can summon ghaelle azata.",
                                                "",
                                                Helpers.GetIcon("b1c7576bd06812b42bda3f09ab202f14"),
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Close,
                                                Helpers.minutesPerLevelDuration,
                                                "",
                                                resource.CreateResourceLogic(),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { archetype.GetParentClass() }),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.DelayedStartPlusDivStep, AbilityRankType.SpeedBonus,
                                                                                classes: new BlueprintCharacterClass[] { archetype.GetParentClass() }, startLevel: 8, stepLevel: 4),
                                                Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Summoning),
                                                Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.SpeedBonus),
                                                                                                                        summon_actions.ToArray()
                                                                                                                        )
                                                                        )
                                                );
            Common.setAsFullRoundAction(ability);
            ability.setMiscAbilityParametersRangedDirectional();
            call_celestial_ally = Common.AbilityToFeature(ability, false);
            call_celestial_ally.AddComponent(resource.CreateAddAbilityResource());
        }


        static void createDivineBond()
        {
            var loh_resource = library.Get<BlueprintAbilityResource>("9dedf41d995ff4446a181f143c3db98c");
            var resource = Helpers.CreateAbilityResource("SacredServantDivineBondResource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 9, 1, 4, 1, 0, 0.0f, new BlueprintCharacterClass[] { archetype.GetParentClass() });
            divine_bond_buff = Helpers.CreateBuff("SacredServantDivineBondBuff",
                                                    "Divine Bond",
                                                    "At 5th level, instead of forming a divine bond with her weapon or a mount, a sacred servant forms a bond with her holy symbol.\n"
                                                    + "At 5th level, the spirit grants one bonus. For every three levels beyond 5th, the spirit grants one additional bonus. These bonuses can be spent in a number of ways to grant the paladin enhanced abilities to channel positive energy and to cast spells.\n"
                                                    + "Each bonus can be used to grant one of the following enhancements:\n"
                                                    + "+1 caster level to any paladin spell cast,\n"
                                                    + "+1 to the DC to halve the damage of channel positive energy when used to harm undead,\n"
                                                    + $"+1d{BalanceFixes.getDamageDieString(DiceType.D6)} to channel positive energy,\n"
                                                    + "restore one use/day of lay on hands.\n"
                                                    + "These enhancements stack and can be selected multiple times. The enhancements granted by the spirit are determined when the spirit is called and cannot be changed until the spirit is called again. If the sacred servant increases her number of uses of lay on hands per day in this way, that choice is set for the rest of the day, and once used, these additional uses are not restored (even if the spirit is called again that day). The celestial spirit imparts no enhancements if the holy symbol is held by anyone other than the sacred servant, but resumes giving enhancements if returned to the sacred servant. A sacred servant can use this ability once per day at 5th level, and one additional time per day for every four levels beyond 5th, to a total of four times per day at 17th level.",
                                                    "",
                                                    LoadIcons.Image2Sprite.Create(@"AbilityIcons/Wish.png"),
                                                    null);

            divine_bond_ability = library.CopyAndAdd<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4", "SacredServantDivineBondAbility", "");
            divine_bond_ability.SetNameDescriptionIcon(divine_bond_buff);
            var apply_buff = Common.createContextActionApplyBuff(divine_bond_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false);
            divine_bond_ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = resource);
            divine_bond_ability.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(apply_buff,
                                                                                                                   Helpers.Create<ResourceMechanics.RestoreResourceAmountEqualToRemainingGroupSize>(r => { r.resource = loh_resource; r.group = ActivatableAbilityGroup.DivineWeaponProperty; }))
                                                                );
            divine_bond_ability.setMiscAbilityParametersSelfOnly();

            var cl_icon = Helpers.GetIcon("1bc83efec9f8c4b42a46162d72cbf494");//burst of glory
            var channel_dice_icon = Helpers.GetIcon("0d657aa811b310e4bbd8586e60156a2d");//cure critical wounds
            var channel_dc_icon = Helpers.GetIcon("a9a52760290591844a96d0109e30e04d");//undeath to death

            var cl_bonuses = new BlueprintActivatableAbility[6];
            var channel_dc_bonuses = new BlueprintActivatableAbility[6];
            var channel_dice_bonuses = new BlueprintActivatableAbility[6];



            cl_bonuses = createSacredBondAbility("SacredServantDivineBondCLBonus",
                                                 "Caster Level Bonus",
                                                 divine_bond_buff.Description,
                                                 cl_icon,
                                                 Helpers.Create<NewMechanics.IncreaseAllSpellsCLForSpecificSpellbook>(cl => { cl.spellbook = archetype.GetParentClass().Spellbook; cl.Value = 1; }),
                                                 Helpers.Create<NewMechanics.IncreaseAllSpellsCLForSpecificSpellbook>(cl => { cl.spellbook = archetype.GetParentClass().Spellbook; cl.Value = 2; }),
                                                 Helpers.Create<NewMechanics.IncreaseAllSpellsCLForSpecificSpellbook>(cl => { cl.spellbook = archetype.GetParentClass().Spellbook; cl.Value = 3; }),
                                                 Helpers.Create<NewMechanics.IncreaseAllSpellsCLForSpecificSpellbook>(cl => { cl.spellbook = archetype.GetParentClass().Spellbook; cl.Value = 4; }),
                                                 Helpers.Create<NewMechanics.IncreaseAllSpellsCLForSpecificSpellbook>(cl => { cl.spellbook = archetype.GetParentClass().Spellbook; cl.Value = 5; }),
                                                 Helpers.Create<NewMechanics.IncreaseAllSpellsCLForSpecificSpellbook>(cl => { cl.spellbook = archetype.GetParentClass().Spellbook; cl.Value = 6; })
                                                 );

            var channels = ChannelEnergyEngine.getChannelAbilities(e => e.scalesWithClass(archetype.GetParentClass())).ToArray();
            channel_dc_bonuses = createSacredBondAbility("SacredServantDivineBondChannelDCBonus",
                                                         "Channel Energy DC Bonus",
                                                         divine_bond_buff.Description,
                                                         channel_dc_icon,
                                                         Helpers.Create<NewMechanics.ContextIncreaseAbilitiesDC>(c => { c.abilities = channels; c.Value = 1; }),
                                                         Helpers.Create<NewMechanics.ContextIncreaseAbilitiesDC>(c => { c.abilities = channels; c.Value = 2; }),
                                                         Helpers.Create<NewMechanics.ContextIncreaseAbilitiesDC>(c => { c.abilities = channels; c.Value = 3; }),
                                                         Helpers.Create<NewMechanics.ContextIncreaseAbilitiesDC>(c => { c.abilities = channels; c.Value = 4; }),
                                                         Helpers.Create<NewMechanics.ContextIncreaseAbilitiesDC>(c => { c.abilities = channels; c.Value = 5; }),
                                                         Helpers.Create<NewMechanics.ContextIncreaseAbilitiesDC>(c => { c.abilities = channels; c.Value = 6; })
                                                         );

            channel_dice_bonuses = createSacredBondAbility("SacredServantDivineBondChannelDiceBonus",
                                                         "Channel Energy Dice Bonus",
                                                         divine_bond_buff.Description,
                                                         channel_dice_icon,
                                                         Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => { c.spells = channels; c.value = 2; }),
                                                         Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => { c.spells = channels; c.value = 4; }),
                                                         Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => { c.spells = channels; c.value = 6; }),
                                                         Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => { c.spells = channels; c.value = 8; }),
                                                         Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => { c.spells = channels; c.value = 10; }),
                                                         Helpers.Create<NewMechanics.ContextIncreaseCasterLevelForSelectedSpells>(c => { c.spells = channels; c.value = 12; })
                                                         );

            for (int i = 0; i < 6; i++)
            {
                divine_bond[i] = Helpers.CreateFeature($"SacredServantDivineBond{i+1}Feature",
                                                       divine_bond_ability.Name,
                                                       divine_bond_ability.Description,
                                                       "",
                                                       divine_bond_ability.Icon,
                                                       FeatureGroup.None);
                if (i == 0)
                {
                    divine_bond[i].AddComponent(resource.CreateAddAbilityResource());
                    divine_bond[i].AddComponent(Helpers.CreateAddFact(divine_bond_ability));
                }
                else
                {
                    divine_bond[i].AddComponent(Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty));
                }
                divine_bond[i].AddComponent(Helpers.CreateAddFacts(cl_bonuses[i], channel_dc_bonuses[i], channel_dice_bonuses[i]));
            }
        }


        static BlueprintActivatableAbility[] createSacredBondAbility(string name, string display_name, string description, UnityEngine.Sprite icon, params BlueprintComponent[] components)
        {
            var toggles = new BlueprintActivatableAbility[6];
            var switch_buffs = new BlueprintBuff[6];
            for (int i = 0; i < 6; i++)
            {
                var buff = Helpers.CreateBuff($"{name}Buff{i + 1}",
                                              $"Divine Bond ({display_name} +{i+1})",
                                              description,
                                              "",
                                              icon,
                                              null,
                                              components[i]
                                              );
                switch_buffs[i] = Helpers.CreateBuff($"{name}SwitchBuff{i + 1}",
                                                      display_name,
                                                      description,
                                                      "",
                                                      icon,
                                                      null
                                                      );
                switch_buffs[i].SetBuffFlags(BuffFlags.HiddenInUi);

                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(divine_bond_buff, buff, switch_buffs[i]);

                toggles[i] = Helpers.CreateActivatableAbility($"{name}ToggleAbility{i + 1}",
                                                              buff.Name,
                                                              description,
                                                              "",
                                                              icon,
                                                              switch_buffs[i],
                                                              AbilityActivationType.Immediately,
                                                              CommandType.Free,
                                                              null);
                toggles[i].Group = ActivatableAbilityGroup.DivineWeaponProperty;
                toggles[i].WeightInGroup = i + 1;
                toggles[i].DeactivateImmediately = true;                                         
            }

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (j != i)
                    {
                        toggles[i].AddComponent(Helpers.Create<RestrictionHasFact>(r => { r.Feature = switch_buffs[j]; r.Not = true; }));
                    }
                }
            }
            return toggles;
        }

    }
}
