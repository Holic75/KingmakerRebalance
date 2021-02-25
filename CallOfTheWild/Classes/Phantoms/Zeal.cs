using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using CallOfTheWild.NewMechanics;

namespace CallOfTheWild
{
    public partial class Phantom
    {
        static void createZeal()
        {
            var ruthless_combatant = Helpers.CreateFeature("ZealPhantomRuthlessCombatantFeature",
                                                         "Ruthless Combatant",
                                                         "The phantom threatens a critical hit with its slam attacks on a roll of 19–20. When the spiritualist reaches 11th level, the phantom’s critical modifier with slam attacks increases to ×3. This doesn’t stack with Improved Critical or similar effects.",
                                                         "",
                                                         Helpers.GetIcon("7812ad3672a4b9a4fb894ea402095167"),//improved unarmed strike
                                                         FeatureGroup.None,
                                                         Helpers.Create<WeaponTypeCriticalEdgeIncrease>(w => w.WeaponType = library.Get<BlueprintWeaponType>("f18cbcb39a1b35643a8d129b1ec4e716"))
                                                         );

            var ruthless_combatant_spiritualist = Helpers.CreateFeature("ZealPhantomExciterRuthlessCombatantFeature",
                                             "Ruthless Combatant",
                                             "The phantom threatens a critical hit with its slam attacks on a roll of 19–20. When the spiritualist reaches 11th level, the phantom’s critical modifier with slam attacks increases to ×3. This doesn’t stack with Improved Critical or similar effects.",
                                             "",
                                             Helpers.GetIcon("7812ad3672a4b9a4fb894ea402095167"),//improved unarmed strike
                                             FeatureGroup.None,
                                             Helpers.Create<NewMechanics.AttackTypeCriticalEdgeIncrease>(w => w.Type = AttackTypeAttackBonus.WeaponRangeType.Melee)
                                             );

            var ruthless_combatant2 = Helpers.CreateFeature("ZealPhantomRuthlessCombatantFeature2",
                                             "",
                                             "",
                                             "",
                                             Helpers.GetIcon("7812ad3672a4b9a4fb894ea402095167"),//improved unarmed strike
                                             FeatureGroup.None,
                                             Helpers.Create<WeaponTypeCriticalMultiplierIncrease>(w => { w.WeaponType = library.Get<BlueprintWeaponType>("f18cbcb39a1b35643a8d129b1ec4e716"); w.AdditionalMultiplier = 1; })
                                             );
            ruthless_combatant2.HideInCharacterSheetAndLevelUp = true;
            ruthless_combatant2.HideInUI = true;
            ruthless_combatant.AddComponent(ruthless_combatant2.CreateAddFeatureOnClassLevel(9, getPhantomArray()));

            var ruthless_combatant2_spiritualist = Helpers.CreateFeature("ZealPhantomExciterRuthlessCombatantFeature2",
                                 "",
                                 "",
                                 "",
                                 Helpers.GetIcon("7812ad3672a4b9a4fb894ea402095167"),//improved unarmed strike
                                 FeatureGroup.None,
                                 Helpers.Create<AttackTypeCriticalMultiplierIncrease>(w => { w.Type = AttackTypeAttackBonus.WeaponRangeType.Melee; w.AdditionalMultiplier = 1; })
                                 );

            ruthless_combatant2_spiritualist.HideInCharacterSheetAndLevelUp = true;
            ruthless_combatant2_spiritualist.HideInUI = true;
            ruthless_combatant_spiritualist.AddComponent(ruthless_combatant2_spiritualist.CreateAddFeatureOnClassLevel(9, Spiritualist.getSpiritualistArray()));

            var determination_aura_buff = Helpers.CreateBuff("ZealPhantomDeterminationAuraEffectBuff",
                                                       "Determination Aura",
                                                       "When the spiritualist reaches 7th level, as a swift action, the phantom can emit a 20-footradius aura that grants its zeal to nearby allies. Allies within the aura gain a +2 competence bonus on attack rolls and saving throws.",
                                                       "",
                                                       Helpers.GetIcon("87ab2fed7feaaff47b62a3320a57ad8d"), //heroism
                                                       null,
                                                       Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 2, Kingmaker.Enums.ModifierDescriptor.Competence),
                                                       Helpers.CreateAddStatBonus(StatType.SaveFortitude, 2, Kingmaker.Enums.ModifierDescriptor.Competence),
                                                       Helpers.CreateAddStatBonus(StatType.SaveReflex, 2, Kingmaker.Enums.ModifierDescriptor.Competence),
                                                       Helpers.CreateAddStatBonus(StatType.SaveWill, 2, Kingmaker.Enums.ModifierDescriptor.Competence)
                                                       );

            var toggle = Common.createToggleAreaEffect(determination_aura_buff, 20.Feet(), Helpers.CreateConditionsCheckerAnd(),
                                                      AbilityActivationType.WithUnitCommand,
                                                      UnitCommand.CommandType.Swift,
                                                      Common.createPrefabLink("63f322580ec0e7c4c96fc62ecabad40f"), //axiomatic
                                                      null
                                                      );
            toggle.DeactivateIfOwnerDisabled = true;
            var determination_aura = Common.ActivatableAbilityToFeature(toggle, false);

            var haste = library.Get<BlueprintAbility>("486eaff58293f6441a5c2759c4872f98");
            var haste_resource = Helpers.CreateAbilityResource("ZealPhantomHasteResource", "", "", "", null);
            haste_resource.SetFixedResource(3);
            var haste_spell_like = Common.convertToSpellLike(haste, "ZealPhantom", getPhantomArray(), StatType.Charisma, haste_resource, no_scaling: true);
            var zeal_haste = Common.AbilityToFeature(haste_spell_like, false);
            zeal_haste.AddComponent(haste_resource.CreateAddAbilityResource());
            zeal_haste.SetNameDescription("Haste", "When the spiritualist reaches 12th level, three times per day as a standard action, the phantom can use haste as spell-like ability using his Hit Dice as caster level.");



            var zeals_resolve_resource = Helpers.CreateAbilityResource("ZealPhantomZealsResolveResource", "", "", "", null);
            zeals_resolve_resource.SetFixedResource(3);

            var rule_types = new ModifyD20WithActions.RuleType[] { ModifyD20WithActions.RuleType.AttackRoll,
                                                                   ModifyD20WithActions.RuleType.SavingThrow};
            var names = new string[] { "Attack Roll", "Saving Throw" };
            var abilities = new List<BlueprintActivatableAbility>();
            for (int i = 0; i < rule_types.Length; i++)
            {
                var buff2 = library.CopyAndAdd<BlueprintBuff>("3bc40c9cbf9a0db4b8b43d8eedf2e6ec", rule_types[i].ToString() + "ZealPhantomZealsResolveBuff", "");
                buff2.SetNameDescription("Zeal’s Resolve" + ": " + names[i],
                                         "When the spiritualist reaches 17th level, three times per day as a free action, when the phantom misses with an attack roll or fails a saving throw, it can reroll the failed attack or saving throw. It must take the new result, even if that result is worse.");
                buff2.RemoveComponents<ModifyD20>();
                buff2.AddComponent(Helpers.Create<NewMechanics.ModifyD20WithActions>(m =>
                {
                    m.Rule = rule_types[i];
                    m.RollsAmount = 1;
                    m.TakeBest = true;
                    m.RerollOnlyIfFailed = true;
                    m.actions = Helpers.CreateActionList(Common.createContextActionSpendResource(zeals_resolve_resource, 1));
                    m.required_resource = zeals_resolve_resource;
                })
                                                                                    );
                var ability2 = Helpers.CreateActivatableAbility(rule_types[i].ToString() + "ZealPhantomZealsResolve" + "ToggleAbility",
                                                               buff2.Name,
                                                               buff2.Description,
                                                               "",
                                                               buff2.Icon,
                                                               buff2,
                                                               AbilityActivationType.Immediately,
                                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                               null,
                                                               Helpers.CreateActivatableResourceLogic(zeals_resolve_resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                               );
                ability2.DeactivateImmediately = true;
                abilities.Add(ability2);
            }


            var zeals_resolve = Helpers.CreateFeature("ZealPhantomZealsResolveFeature",
                                                    "Zeal’s Resolve",
                                                    abilities[0].Description,
                                                    "",
                                                    abilities[0].Icon,
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddFacts(abilities.ToArray()),
                                                    zeals_resolve_resource.CreateAddAbilityResource()
                                                    );

            var skill_phantom = Helpers.CreateFeature("ZealPhantomBaseFeature",
                                                                 "Tracking",
                                                                 "The phantom adds half its number of Hit Dice (minimum 1) to Lore (Nature) checks.",
                                                                 "",
                                                                 Helpers.GetIcon("6507d2da389ed55448e0e1e5b871c013"),
                                                                 FeatureGroup.None,
                                                                 Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.UntypedStackable),
                                                                 Helpers.CreateContextRankConfig(Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.ClassLevel,
                                                                                                 classes: getPhantomArray(),
                                                                                                 progression: Kingmaker.UnitLogic.Mechanics.Components.ContextRankProgression.Div2,
                                                                                                 min: 1
                                                                                                 )
                                                                 );


            var zeal_archetype = createPhantomArchetype("ZealPhantomArchetype",
                                                         "Zeal",
                                                         true,
                                                         true,
                                                         false,
                                                         new StatType[] { StatType.SkillMobility, StatType.SkillLoreNature },
                                                         new LevelEntry[] { Helpers.LevelEntry(1, skill_phantom)},
                                                         new LevelEntry[] { Helpers.LevelEntry(1)}
                                                         );

            //bless, false life, heroism, freedom of movement, joyful rapture, heroism greater
            createPhantom("Zeal",
                          "Zeal",
                          "A phantom with this emotional focus fixates on every task given as if it were the phantom’s last. The most basic commands are treated as life-and-death situations, and the truly dangerous ones are faced with a resolve and tenacity that sometimes defies common sense. Zeal phantoms take the form of steadfast protectors or daring and manic creatures looking for the next challenge or opportunity to prove itself to either its master or itself. Often very prideful creatures, these phantoms display an orange aura upon completing their tasks. They are prone to boast about their accomplishments and chastise those around them for not accomplishing more.\n"
                          + "Skills: The phantom gains a number of ranks in Mobility and Lore (Nature) equal to its number of Hit Dice. While confined in the spiritualist’s consciousness, the phantom grants the spiritualist Skill Focus in each of these skills.\n"
                          + "Good Saves: Fortitude and Will.\n"
                          + "Tracking: The phantom adds half its number of Hit Dice (minimum 1) to Lore (Nature) checks.",
                          determination_aura.Icon,
                          zeal_archetype,
                          ruthless_combatant, determination_aura, zeal_haste, zeals_resolve,
                          new StatType[] { StatType.SkillMobility, StatType.SkillLoreNature },
                          12, 14,
                          new BlueprintAbility[]
                          {
                              library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638"), //bless
                              NewSpells.force_sword,
                              library.Get<BlueprintAbility>("9d5d2d3ffdd73c648af3eb3e585b1113"), //divine favor
                              library.Get<BlueprintAbility>("0087fc2d64b6095478bc7b8d7d512caf") //freedom of movement
                          },
                          ruthless_combatant_spiritualist,
                          determination_aura,
                          emotion_conduit_spells: new BlueprintAbility[]
                          {
                              library.Get<BlueprintAbility>("90e59f4a4ada87243b7b3535a06d0638"), //bless
                              library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762"), //false life
                              library.Get<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63"), //heroism
                              library.Get<BlueprintAbility>("0087fc2d64b6095478bc7b8d7d512caf"), //freedom of movement
                              library.Get<BlueprintAbility>("15a04c40f84545949abeedef7279751a"), //joyful rapture
                              library.Get<BlueprintAbility>("e15e5e7045fda2244b98c8f010adfe31") //heroism greater
                          }
                          );
        }
    }
}

