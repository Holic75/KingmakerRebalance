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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public partial class Phantom
    {
        static void createAnger()
        {
            var powerful_strikes = Helpers.CreateFeature("AngerPhantomPowerfulStrikesFeature",
                                                         "Powerful Strikes",
                                                         "A phantom with this focus deals more damage with its slam attacks. It deals slam damage as a creature one size category larger than its current size.\n"
                                                         + "The phantom also gains Power Attack as a bonus feat.",
                                                         "",
                                                         Helpers.GetIcon("7812ad3672a4b9a4fb894ea402095167"),//improved unarmed strike
                                                         FeatureGroup.None
                                                         //Common.createWeaponTypeSizeChange(1, new BlueprintWeaponType[] { library.Get<BlueprintWeaponType>("f18cbcb39a1b35643a8d129b1ec4e716") })
                                                         //power attack and damage will be added in archetype
                                                         );

            var powerful_strikes_spiritualist = Helpers.CreateFeature("AngerPhantomPowerfulStrikesExciterFeature",
                                                         powerful_strikes.Name,
                                                         powerful_strikes.Description,
                                                         "",
                                                         powerful_strikes.Icon,
                                                         FeatureGroup.None,
                                                         Helpers.Create<MeleeWeaponSizeChange>(m => m.SizeCategoryChange = 1),
                                                         Helpers.CreateAddFact(library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5"))
                                                         );

            var aura_of_fury_effect_buff = Helpers.CreateBuff("AngerPhantomAuraOfFuryEffectBuff",
                                                       "Aura of Fury",
                                                       "When the spiritualist reaches 7th level, as a swift action, the phantom can emit a 20-foot-radius aura of fury. Creatures within the aura gain a +2 bonus on melee attack rolls but take a –2 penalty to AC. Ending the aura is a free action.",
                                                       "",
                                                       Helpers.GetIcon("97b991256e43bb140b263c326f690ce2"), //rage
                                                       null,
                                                       Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, -2, Kingmaker.Enums.ModifierDescriptor.UntypedStackable),
                                                       Common.createAttackTypeAttackBonus(2, AttackTypeAttackBonus.WeaponRangeType.Melee, ModifierDescriptor.UntypedStackable)
                                                       );

            var toggle = Common.createToggleAreaEffect(aura_of_fury_effect_buff, 20.Feet(), Helpers.CreateConditionsCheckerAnd(),
                                                      AbilityActivationType.WithUnitCommand,
                                                      UnitCommand.CommandType.Swift,
                                                      Common.createPrefabLink("b3acbaa70e97c3649992e8f1e4bfe8dd"), //anarchic
                                                      null
                                                      );
            toggle.DeactivateIfOwnerDisabled = true;
            var aura_of_fury = Common.ActivatableAbilityToFeature(toggle, false);

            var ferocious_mien_buff = library.CopyAndAdd<BlueprintBuff>("4f139d125bb602f48bfaec3d3e1937cb", "AngerPhantomFerociousMienBuff", "");
            ferocious_mien_buff.AddComponents(library.Get<BlueprintBuff>("a1ffec0ce7c167a40aaea13dc49b757b").ComponentsArray);
            ferocious_mien_buff.FxOnStart = library.Get<BlueprintBuff>("a1ffec0ce7c167a40aaea13dc49b757b").FxOnStart;

            var ferocious_mien_resource = Helpers.CreateAbilityResource("AngeerPhantomFerociousMienResource", "", "", "", null);
            ferocious_mien_resource.SetFixedResource(3);
            var ferocious_mien_ability = Helpers.CreateAbility("AngerPhantomFerociousMienAbility",
                                                           "Ferocious Mien",
                                                           "When the spiritualist reaches 12th level, three times per day as a swift action, a phantom in ectoplasmic form can grow more ferocious and frightening. It becomes one size category larger than its current size, as affected by an enlarge person spell, and grows fiercer in combat, as if affected by a rage spell. This effect lasts for 1 round per class level of the spiritualist.",
                                                           "",
                                                           ferocious_mien_buff.Icon,
                                                           AbilityType.Supernatural,
                                                           UnitCommand.CommandType.Swift,
                                                           AbilityRange.Personal,
                                                           Helpers.roundsPerLevelDuration,
                                                           "",
                                                           Helpers.CreateRunActions(Common.createContextActionApplyBuff(ferocious_mien_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false)),
                                                           library.Get<BlueprintAbility>("c60969e7f264e6d4b84a1499fdcf9039").GetComponent<AbilitySpawnFx>(),
                                                           ferocious_mien_resource.CreateResourceLogic()
                                                           );
            ferocious_mien_ability.setMiscAbilityParametersSelfOnly();
            var ferocious_mien = Common.AbilityToFeature(ferocious_mien_ability, false);
            ferocious_mien.AddComponent(ferocious_mien_resource.CreateAddAbilityResource());

            var wail_of_banshee = library.Get<BlueprintAbility>("b24583190f36a8442b212e45226c54fc");
            var furious_wail_resource = Helpers.CreateAbilityResource("AngerPhantomFuriousWailResource", "", "", "", null);
            furious_wail_resource.SetFixedResource(1);
            var furious_wail_ability = Common.convertToSuperNatural(wail_of_banshee, "AngerPhantom", getPhantomArray(), StatType.Charisma, furious_wail_resource);
            var furious_wail = Common.AbilityToFeature(furious_wail_ability);
            furious_wail.AddComponent(furious_wail_resource.CreateAddAbilityResource());


            var powerful_strikes_phantom = Helpers.CreateFeature("AngerPhantomBaseFeature",
                                                                 "Strength Focus",
                                                                 "The phantom gains a +2 bonus to Strength and a –2 penalty to Dexterity. Instead of the phantom gaining a bonus to Dexterity as the spiritualist gains levels, an anger-focused phantom gains a bonus to Strength instead.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.None,
                                                                 Helpers.CreateAddFact(library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5"))
                                                                 );


            var anger_archetype = createPhantomArchetype("AngerPhantomArchetype",
                                                         "Anger",
                                                         true,
                                                         false,
                                                         true,
                                                         new StatType[] { StatType.SkillPersuasion, StatType.SkillLoreNature },
                                                         new LevelEntry[] { Helpers.LevelEntry(1, slam_damage_large, powerful_strikes_phantom),
                                                                            Helpers.LevelEntry(2, str_cha_bonus),
                                                                            Helpers.LevelEntry(4, str_cha_bonus),
                                                                            Helpers.LevelEntry(6, str_cha_bonus),
                                                                            Helpers.LevelEntry(8, str_cha_bonus),
                                                                            Helpers.LevelEntry(9, str_cha_bonus),
                                                                            Helpers.LevelEntry(12, str_cha_bonus),
                                                                            Helpers.LevelEntry(13, str_cha_bonus),
                                                                            Helpers.LevelEntry(15, str_cha_bonus),
                                                         },
                                                         new LevelEntry[] { Helpers.LevelEntry(1, slam_damage),
                                                                            Helpers.LevelEntry(2, dex_cha_bonus),
                                                                            Helpers.LevelEntry(4, dex_cha_bonus),
                                                                            Helpers.LevelEntry(6, dex_cha_bonus),
                                                                            Helpers.LevelEntry(8, dex_cha_bonus),
                                                                            Helpers.LevelEntry(9, dex_cha_bonus),
                                                                            Helpers.LevelEntry(12, dex_cha_bonus),
                                                                            Helpers.LevelEntry(13, dex_cha_bonus),
                                                                            Helpers.LevelEntry(15, dex_cha_bonus),}
                                                         );

            //burst of adrenaline, rage, howling agony, shout, song of discord, transformation
            createPhantom("Anger",
                          "Anger",
                          "Phantoms with this emotional focus are filled with seething anger from events in their past lives. Phantoms with this focus often take the form of hulking brutes with furrowed brows or of frenzied creatures that always seem ready to strike down those who come too near. Many times, these phantoms exude a bright red aura, especially when they are engaged in combat, or they seem to breathe a red mist in shallow pants from behind clenched, phantasmal teeth.\n"
                          + "Skills: The phantom gains a number of ranks in Persuasion and Lore (Nature) equal to its number of Hit Dice. While confined in the spiritualist’s consciousness, the phantom grants the spiritualist Skill Focus in each of these skills.\n"
                          + "Good Saves: Fortitude and Will.\n"
                          + "Strength Focus: The phantom gains a +2 bonus to Strength and a –2 penalty to Dexterity. Instead of the phantom gaining a bonus to Dexterity as the spiritualist gains levels, an anger-focused phantom gains a bonus to Strength instead.",
                          aura_of_fury.Icon,
                          anger_archetype,
                          powerful_strikes, aura_of_fury, ferocious_mien, furious_wail,
                          new StatType[] { StatType.SkillPersuasion, StatType.SkillLoreNature },
                          14, 12,
                          new BlueprintAbility[]
                          {
                              library.Get<BlueprintAbility>("c60969e7f264e6d4b84a1499fdcf9039"), //enlarge person
                              library.Get<BlueprintAbility>("97b991256e43bb140b263c326f690ce2"), //rage
                              library.Get<BlueprintAbility>("e80a4d6c0efa5774cbd515e3e37095b0"), //longstrider greater
                              library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162") //shout
                          },
                          powerful_strikes_spiritualist,
                          aura_of_fury,
                          emotion_conduit_spells: new BlueprintAbility[]
                          {
                              NewSpells.burst_of_adrenaline,
                              SpellDuplicates.addDuplicateSpell(library.Get<BlueprintAbility>("97b991256e43bb140b263c326f690ce2"), "EmotionConduitRageSpell"), //rage
                              NewSpells.howling_agony,
                              library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162"), //shout
                              library.Get<BlueprintAbility>("d38aaf487e29c3d43a3bffa4a4a55f8f"), //song of discord
                              library.Get<BlueprintAbility>("27203d62eb3d4184c9aced94f22e1806"), //transformation
                          }
                          );
        }
    }
}
