using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
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
        static void createDespair()
        {
            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var frightened = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");
            var crushing_despair_buff = library.Get<BlueprintBuff>("36897b36a4bd63146b1d8509a17fc5ad");
            var miserable_strikes_buff = Helpers.CreateBuff("DespairPhantomMiserableStrikesBuff",
                                                                    "Miserable Strikes",
                                                                    "If the phantom hits a creature with a slam attack, that creature must succeed at a Will Saving Throw (DC = 10 + 1/2 the phantom’s Hit Dice + the phantom’s Charisma modifier) or take a –2 penalty on attack and damage rolls for 1 round. This is a mind-affecting fear and emotion effect. Penalties from multiple hits don’t stack with themselves.",
                                                                    "",
                                                                    Helpers.GetIcon("e6f2fc5d73d88064583cb828801212f4"),
                                                                    null,
                                                                    Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, -2, ModifierDescriptor.UntypedStackable),
                                                                    Helpers.CreateAddStatBonus(StatType.AdditionalDamage, -2, ModifierDescriptor.UntypedStackable),
                                                                    Helpers.CreateSpellDescriptor(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.MindAffecting | Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Fear)
                                                                    );
            var apply_miserable_strikes = Common.createContextActionApplyBuff(miserable_strikes_buff, Helpers.CreateContextDuration(1), dispellable: false);
            
            var inescapable_despair = Helpers.CreateFeature("DespairPhantomInescapableDespairFeature",
                                             "Inescapable Despair",
                                             "When the spiritualist reaches 17th level, if the phantom hits with its slam attack, the creature hit doesn’t get a save to resist the effects of miserable strike.",
                                             "",
                                             miserable_strikes_buff.Icon,
                                             FeatureGroup.None
                                             );

            var apply_conditional = Helpers.CreateConditional(Common.createContextConditionCasterHasFact(inescapable_despair),
                                                              apply_miserable_strikes,
                                                              Helpers.CreateActionSavingThrow(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, apply_miserable_strikes))
                                                              );
            var miserable_strike = Helpers.CreateFeature("DespairPhantomMiserableStrikesFeature",
                                                         miserable_strikes_buff.Name,
                                                         miserable_strikes_buff.Description,
                                                         "",
                                                         miserable_strikes_buff.Icon,
                                                         FeatureGroup.None,
                                                         Common.createAddInitiatorAttackWithWeaponTriggerWithCategory(Helpers.CreateActionList(apply_conditional),
                                                                                                                      wait_for_attack_to_resolve: true,
                                                                                                                      weapon_category: WeaponCategory.OtherNaturalWeapons
                                                                                                                      ),
                                                         Common.createContextCalculateAbilityParamsBasedOnClass(phantom_class, StatType.Charisma),
                                                         Helpers.CreateSpellDescriptor(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.MindAffecting | Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Fear)
                                                         );

            var aura_of_despair_effect_buff = Helpers.CreateBuff("DespairPhantomAuraOfDespairEffectBuff",
                                                       "Aura of Despair",
                                                       "When the spiritualist reaches 7th level, as a swift action, the phantom can emit a 20-foot aura of despair. Enemies within the aura take a –2 penalty on all saving throws. This is a fear effect. Deactivating the aura is a free action.",
                                                       "",
                                                       Helpers.GetIcon("dd2918e4a77c50044acba1ac93494c36"), //overwhelming grief
                                                       null,
                                                       Helpers.CreateAddStatBonus(StatType.SaveFortitude, -2, Kingmaker.Enums.ModifierDescriptor.UntypedStackable),
                                                       Helpers.CreateAddStatBonus(StatType.SaveReflex, -2, Kingmaker.Enums.ModifierDescriptor.UntypedStackable),
                                                       Helpers.CreateAddStatBonus(StatType.SaveWill, -2, Kingmaker.Enums.ModifierDescriptor.UntypedStackable)
                                                       );

            var toggle = Common.createToggleAreaEffect(aura_of_despair_effect_buff, 20.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()),
                                                      AbilityActivationType.WithUnitCommand,
                                                      UnitCommand.CommandType.Swift,
                                                      Common.createPrefabLink("b3acbaa70e97c3649992e8f1e4bfe8dd"), //anarchic
                                                      null
                                                      );
            toggle.DeactivateIfOwnerDisabled = true;
            var aura_of_despair = Common.ActivatableAbilityToFeature(toggle, false);

           
            var crushing_despair = library.Get<BlueprintAbility>("4baf4109145de4345861fe0f2209d903");
            var despairing_shout_resource = Helpers.CreateAbilityResource("DespairPhantomDespairingShoutResource", "", "", "", null);
            despairing_shout_resource.SetFixedResource(3);
            var crushing_despair_ability = Common.convertToSuperNatural(crushing_despair, "DespairPhantom", getPhantomArray(), StatType.Charisma, despairing_shout_resource);
            var despairing_shout = Common.AbilityToFeature(crushing_despair_ability);
            despairing_shout.AddComponent(despairing_shout_resource.CreateAddAbilityResource());
            despairing_shout.SetNameDescription("Despairing Shout",
                                                "When the spiritualist reaches 12th level, three times per day as a standard action, the phantom can emit a shout that acts as crushing despair. The phantom uses its Hit Dice as its caster level for the effect, and the DC of the effect equals 10 + 1/2 the phantom’s Hit Dice + the phantom’s Charisma modifier. The phantom can use this ability in either ectoplasmic or incorporeal form.");

            var powerful_from_despair_phantom = Helpers.CreateFeature("DespairPhantomBaseFeature",
                                                                 "Power from Despair",
                                                                 "The phantom gains a +2 bonus on attack and damage rolls against creatures that are shaken, frightened, panicked, cowering, or subject to effects such as aura of despair or crushing despair.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.None,
                                                                 Helpers.Create<NewMechanics.AttackBonusAgainstFactsOwner>(a =>
                                                                     {
                                                                         a.attack_types = new Kingmaker.RuleSystem.AttackType[] { AttackType.Melee, AttackType.Ranged, AttackType.RangedTouch, AttackType.Touch };
                                                                         a.Bonus = 2;
                                                                         a.check_only_one_fact = true;
                                                                         a.Descriptor = ModifierDescriptor.UntypedStackable;
                                                                         a.only_from_caster = false;
                                                                         a.CheckedFacts = new BlueprintUnitFact[]
                                                                         {
                                                                             shaken, frightened, crushing_despair_buff, aura_of_despair_effect_buff
                                                                         };
                                                                     }),
                                                                 Helpers.Create<NewMechanics.DamageBonusAgainstAnyFactsOwner>(d =>
                                                                    {
                                                                        d.Bonus = 2;
                                                                        d.Descriptor = ModifierDescriptor.UntypedStackable;
                                                                        d.facts = new BlueprintUnitFact[]
                                                                         {
                                                                             shaken, frightened, crushing_despair_buff, aura_of_despair_effect_buff
                                                                         };
                                                                    }
                                                                    )
                                                                 );


            var despair_archetype = createPhantomArchetype("DespairPhantomArchetype",
                                                         "Despair",
                                                         true,
                                                         false,
                                                         true,
                                                         new StatType[] { StatType.SkillPersuasion, StatType.SkillStealth },
                                                         new LevelEntry[] { Helpers.LevelEntry(1, powerful_from_despair_phantom)},
                                                         new LevelEntry[] {Helpers.LevelEntry(1) }
                                                         );

            //touch of gracelesness, stricken heart, ray of exhaustion, crushing despair, suffocation, eyebite
            createPhantom("Despair",
                          "Despair",
                          "Some creatures die in such horrific ways, or live such pointless and senseless lives, that despair grips their very beings. Phantoms with this focus use misery as a weapon, inflicting the living with the gloom of the phantoms’ continued existence. Despair phantoms often appear twisted or wounded, showing the grisly circumstances of their demise. Their coloration tends to have a grayish or sickly green cast. When they speak, they do so in terrifying whispers or high-pitched screeches.\n"
                          + "Skills: The phantom gains a number of ranks in Persuasion and Stealth equal to its number of Hit Dice. While confined in the spiritualist’s consciousness, the phantom grants the spiritualist Skill Focus in each of these skills.\n"
                          + "Good Saves: Fortitude and Will.\n"
                          + "Power from Despair: The phantom gains a +2 bonus on attack and damage rolls against creatures that are shaken, frightened, panicked, cowering, or subject to effects such as aura of despair or crushing despair.",
                          despairing_shout.Icon,
                          despair_archetype,
                          miserable_strike, aura_of_despair, despairing_shout, inescapable_despair,
                          new StatType[] { StatType.SkillPersuasion, StatType.SkillStealth },
                          12, 14,
                          new BlueprintAbility[]
                          {
                              Witch.ill_omen,
                              NewSpells.stricken_heart,
                              library.Get<BlueprintAbility>("f492622e473d34747806bdb39356eb89"), //slow
                              library.Get<BlueprintAbility>("4baf4109145de4345861fe0f2209d903") //crushing despair
                          }
                          );
        }
    }
}

