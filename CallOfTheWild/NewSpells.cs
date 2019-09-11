using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class NewSpells
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintAbility shillelagh;
        static public BlueprintAbility flame_blade;
        static public BlueprintAbility virtuoso_performance;
        static public BlueprintAbility deadly_juggernaut;
        static public BlueprintAbility invisibility_purge;
        static public BlueprintAbility sanctuary;
        static public BlueprintBuff sanctuary_buff;

        static public BlueprintAbility command;
        static public BlueprintAbility fire_shield;
        static public Dictionary<DamageEnergyType, BlueprintBuff> fire_shield_buffs = new Dictionary<DamageEnergyType, BlueprintBuff>();
        static public Dictionary<DamageEnergyType, BlueprintAbility> fire_shield_variants = new Dictionary<DamageEnergyType, BlueprintAbility>();


        static public BlueprintAbility strong_jaw;


        static public BlueprintAbility contingency;

        static public void load()
        {
            createShillelagh();
            createFlameBlade();
            createVirtuosoPerformance();
            createDeadlyJuggernaut();
            createInvisibilityPurge();
            createSanctuary();
            createCommand();
            createFireShield();
            createContingency();
            createStrongJaw();
        }


        static void createStrongJaw()
        {
            var acid_maw = library.Get<BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67");
            var magic_fang = library.Get<BlueprintAbility>("403cf599412299a4f9d5d925c7b9fb33");

            var buff = Helpers.CreateBuff("StrongJawBuff",
                                          "Strong Jaw",
                                          "Laying a hand upon an allied creature’s jaw, claws, tentacles, or other natural weapons, you enhance the power of that creature’s natural attacks. Each natural attack that creature makes deals damage as if the creature were two sizes larger than it actually is. If the creature is already Gargantuan or Colossal-sized, double the amount of damage dealt by each of its natural attacks instead. This spell does not actually change the creature’s size; all of its statistics except the amount of damage dealt by its natural attacks remain unchanged.",
                                          "",
                                          acid_maw.Icon,
                                          null,
                                          Helpers.Create<NewMechanics.DoubleWeaponSize>(d => d.categories = new WeaponCategory[] { WeaponCategory.OtherNaturalWeapons, WeaponCategory.Bite, WeaponCategory.Claw, WeaponCategory.Gore, WeaponCategory.UnarmedStrike })
                                          );

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), true);
            strong_jaw = Helpers.CreateAbility("StrongJawAbility",
                                               buff.Name,
                                               buff.Description,
                                               "",
                                               buff.Icon,
                                               AbilityType.Spell,
                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                               AbilityRange.Touch,
                                               Helpers.minutesPerLevelDuration,
                                               "",
                                               Helpers.CreateRunActions(apply_buff),
                                               magic_fang.GetComponent<AbilitySpawnFx>(),
                                               Helpers.CreateSpellComponent(SpellSchool.Transmutation)
                                               );
            strong_jaw.AvailableMetamagic = magic_fang.AvailableMetamagic;
            strong_jaw.setMiscAbilityParametersTouchFriendly();
            strong_jaw.AddToSpellList(Helpers.druidSpellList, 4);
            strong_jaw.AddToSpellList(Helpers.rangerSpellList, 3);

            strong_jaw.AddSpellAndScroll("1cd597e316ac49941a568312de2be6ae");
        }


        static void createContingency()
        {
            var evocation = library.Get<BlueprintFeature>("c46512b796216b64899f26301241e4e6");
            var divination_buff = library.Get<BlueprintBuff>("6d338078b1a8cdc41bf3a39f65247161");

            var contingency_give_ability_buff = Helpers.CreateBuff("ContingencyGiveAbilityBuff",
                                                                   "",
                                                                   "",
                                                                   "",
                                                                   null,
                                                                   null);
            contingency_give_ability_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var action_remove_contingency_use = Helpers.CreateActionList(Common.createContextActionRemoveBuff(contingency_give_ability_buff));
            var contingency_store_buff = Helpers.CreateBuff("ContingencyBuff",
                                                            "Contingency",
                                                            "You can place another spell upon your person so that it comes into effect at some point in the future whenever you desire. You will need to spend another full round action to apply companion spell.\n"
                                                            + "The spell to be brought into effect by the contingency must be one that affects your person and be of a spell level no higher than one-third your caster level (rounded down, maximum 6th level).\n"
                                                            + "At any moment during spell duration you can release a companion spell as a free action. You can use only one contingency spell at a time; if a second is cast, the first one (if still active) is dispelled.",
                                                            "",
                                                            evocation.Icon,
                                                            divination_buff.FxOnStart,
                                                            Helpers.Create<SpellManipulationMechanics.FactStoreSpell>(f => f.actions_on_store = action_remove_contingency_use),
                                                            Helpers.CreateAddFactContextActions(Common.createContextActionApplyBuff(contingency_give_ability_buff, 
                                                                                                                                    Helpers.CreateContextDuration(),
                                                                                                                                    is_child: true, is_permanent: true)
                                                                                                                                    )
                                                            );
            contingency_store_buff.AddComponent(Helpers.Create<SpellManipulationMechanics.AddStoredSpellToCaption>(a => a.store_fact = contingency_store_buff));
            var release_action = Helpers.Create<SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = contingency_store_buff);
            var remove_buff = Common.createContextActionRemoveBuff(contingency_store_buff);

            var contingency_release = Helpers.CreateAbility("ContingencyReleaseAbility",
                                                            "Contingency: Release",
                                                            contingency_store_buff.Description,
                                                            "",
                                                            contingency_store_buff.Icon,
                                                            AbilityType.SpellLike,
                                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                            AbilityRange.Personal,
                                                            "",
                                                            "",
                                                            Helpers.CreateRunActions(release_action, remove_buff),
                                                            Helpers.Create<SpellManipulationMechanics.AbilityCasterHasSpellStoredInFact>(a => a.store_fact = contingency_store_buff)
                                                            );
            contingency_release.setMiscAbilityParametersSelfOnly();
            contingency_store_buff.AddComponent(Helpers.CreateAddFact(contingency_release));

            int max_variants = 6;
            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return  spell.Spellbook != null
                        && spell.SpellLevel <= (spell.Spellbook.CasterLevel / 3)
                        && spell.Blueprint.CanTargetSelf
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < max_variants)
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent)
                        && !SpellManipulationMechanics.FactStoreSpell.hasSpellStoredInFact(spell.Caster, contingency_store_buff);
            };



            for (int i = 0; i < max_variants; i++)
            {
                var contingency_use = Helpers.CreateAbility($"ContingencyStoreAbility{i}",
                                                            "Contingency: Store",
                                                            contingency_store_buff.Description,
                                                            "",
                                                            contingency_store_buff.Icon,
                                                            AbilityType.Spell,
                                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                            AbilityRange.Personal,
                                                            "1 day/level or until discharged",
                                                            "",
                                                            Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                            Helpers.Create<SpellManipulationMechanics.AbilityStoreSpellInFact>(s =>
                                                                                                                              { s.fact = contingency_store_buff;
                                                                                                                                  s.check_slot_predicate = check_slot_predicate;
                                                                                                                                  s.variant = i;
                                                                                                                              }
                                                                                                                              )
                                                            );
                contingency_give_ability_buff.AddComponent(Helpers.CreateAddFact(contingency_use));
                contingency_use.setMiscAbilityParametersSelfOnly();
                Common.setAsFullRoundAction(contingency_use);
            }

            var apply_buff = Common.createContextActionApplyBuff(contingency_store_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Days));
            contingency = Helpers.CreateAbility("ContingencyAbility",
                                                "Contingency",
                                                contingency_store_buff.Description,
                                                "",
                                                contingency_store_buff.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                "1 day/level or until discharged",
                                                "",
                                                Helpers.CreateRunActions(apply_buff),
                                                Helpers.CreateContextRankConfig(),
                                                Helpers.CreateSpellComponent(SpellSchool.Evocation)
                                                );
            contingency.setMiscAbilityParametersSelfOnly();
            Common.setAsFullRoundAction(contingency);
            contingency.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken;
            contingency.AddToSpellList(Helpers.wizardSpellList, 6);
            contingency.AddSpellAndScroll("beab337b352b5ac479698e2bbc08f4ce"); //circle of death
        }


        static void createFireShield()
        {
            var shield_of_dawn = library.Get<BlueprintAbility>("62888999171921e4dafb46de83f4d67d");
            var shield = library.Get<BlueprintAbility>("ef768022b0785eb43a18969903c537c4");
            var shield_of_dawn_buff = library.Get<BlueprintBuff>("07abad76e7b688242b56749cd25f5d3d");
            var shield_buff = library.Get<BlueprintBuff>("9c0fa9b438ada3f43864be8dd8b3e741");


            DamageEnergyType[] energy = new DamageEnergyType[] { DamageEnergyType.Fire, DamageEnergyType.Cold };
            SpellDescriptor[] descriptors = new SpellDescriptor[] { SpellDescriptor.Fire, SpellDescriptor.Cold };
            BlueprintBuff[] prototype_buffs = new BlueprintBuff[] { shield_of_dawn_buff, shield_buff };
            BlueprintAbility[] prototype_spells = new BlueprintAbility[] { shield_of_dawn, shield };
            string[] names = new string[] { "Warm Shield", "Chill Shield" };
            string[] descriptions = new string[] { "The flames are warm to the touch. You take only half damage from cold-based attacks. If such an attack allows a Reflex save for half damage, you take no damage on a successful saving throw.",
                                                   "The flames are cool to the touch. You take only half damage from fire-based attacks. If such an attack allows a Reflex save for half damage, you take no damage on a successful saving throw." };


            BlueprintAbility[] shields = new BlueprintAbility[energy.Length];

            for (int i = 0; i < shields.Length; i++)
            {
                var deal_damage = Helpers.CreateActionDealDamage(energy[i],
                                                                 Helpers.CreateContextDiceValue(DiceType.D6, Common.createSimpleContextValue(1), Helpers.CreateContextValue(AbilityRankType.DamageBonus))
                                                                 );

                var dmg_component = shield_of_dawn_buff.GetComponent<AddTargetAttackRollTrigger>().CreateCopy();
                dmg_component.ActionsOnAttacker = Helpers.CreateActionList(deal_damage);
                var buff = Helpers.CreateBuff($"FireShield{i + 1}Buff",
                                              names[i],
                                              descriptions[i],
                                              "",
                                              prototype_buffs[i].Icon,
                                              prototype_buffs[i].FxOnStart,
                                              Common.createAddEnergyDamageDurability(energy[i], 0.5f),
                                              Common.createEvasionAgainstDescriptor(descriptors[i], SavingThrowType.Fortitude),
                                              Common.createEvasionAgainstDescriptor(descriptors[i], SavingThrowType.Reflex),
                                              Common.createEvasionAgainstDescriptor(descriptors[i], SavingThrowType.Will),
                                              Helpers.CreateContextRankConfig(type: AbilityRankType.DamageBonus, max: 15),
                                              dmg_component,
                                              Helpers.CreateSpellDescriptor(descriptors[i])
                                              );

                fire_shield_buffs.Add(energy[i], buff);

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), true);
                shields[i] = Helpers.CreateAbility($"FireShield{i + 1}Ability",
                                                  buff.Name,
                                                  buff.Description,
                                                  "",
                                                  buff.Icon,
                                                  AbilityType.Spell,
                                                  Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                  AbilityRange.Personal,
                                                  Helpers.roundsPerLevelDuration,
                                                  Helpers.savingThrowNone,
                                                  Helpers.CreateRunActions(apply_buff),
                                                  prototype_spells[i].GetComponent<AbilitySpawnFx>(),
                                                  Helpers.CreateSpellDescriptor(descriptors[i]),
                                                  shield_of_dawn.GetComponent<SpellComponent>()
                                                  );
                shields[i].setMiscAbilityParametersSelfOnly();
                fire_shield_variants.Add(energy[i], shields[i]);
                shields[i].SpellResistance = true;
            }

            fire_shield = Helpers.CreateAbility("FireShieldAbility",
                                                "Fire Shield",
                                                "This spell wreathes you in flame and causes damage to each creature that attacks you in melee. The flames also protect you from either cold-based or fire-based attacks, depending on if you choose cool or warm flames for your fire shield.\n"
                                                + "Any creature striking you with its body or a hand - held weapon deals normal damage, but at the same time the attacker takes 1d6 points of damage + 1 point per caster level(maximum + 15). This damage is either cold damage (if you choose a chill shield) or fire damage (if you choose a warm shield). If the attacker has spell resistance, it applies to this effect. Creatures wielding melee weapons with reach are not subject to this damage if they attack you.",
                                                "",
                                                shield_of_dawn.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                Helpers.roundsPerLevelDuration,
                                                Helpers.savingThrowNone,
                                                shield_of_dawn.GetComponent<SpellComponent>());
            fire_shield.AvailableMetamagic = shield_of_dawn.AvailableMetamagic;
            fire_shield.setMiscAbilityParametersSelfOnly();
            fire_shield.SpellResistance = true;

            fire_shield.AddComponent(fire_shield.CreateAbilityVariants(shields));
            fire_shield.AddToSpellList(Helpers.wizardSpellList, 4);
            fire_shield.AddToSpellList(Helpers.magusSpellList, 4);
            fire_shield.AddToSpellList(Helpers.alchemistSpellList, 4);

            fire_shield.AddSpellAndScroll("8e0c81ac23fe75b4288c21ee57f55e3f"); // shield of dawn

            //replace 5th level spell in fire domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("881b2137a1779294c8956fe5b497cc35"), fire_shield, 5);
            //replace 4th level spell in sun domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("c85c8791ee13d4c4ea10d93c97a19afc"), fire_shield, 4);
        }


        static void createCommand()
        {
            var dominate_person = library.Get<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61");
            BlueprintBuff[] buffs = new BlueprintBuff[]{library.Get<BlueprintBuff>("9934fedff1b14994ea90205d189c8759"), //daze
                                                         library.Get<BlueprintBuff>("24cf3deb078d3df4d92ba24b176bda97"), //prone
                                                         library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf") //frightened
                                                        };
            string[] names = { "Halt", "Fall", "Run" };
            string[] descriptions = { "The subject stands in place for 1 round. It may not take any actions but is not considered helpless.",
                                      "On its turn, the subject falls to the ground and remains prone for 1 round. It may act normally while prone but takes any appropriate penalties.",
                                      "On its turn, the subject moves away from you as quickly as possible for 1 round. It may do nothing but move during its turn, and it provokes attacks of opportunity for this movement as normal." };

            List<BlueprintAbility> commands = new List<BlueprintAbility>();


            command = Helpers.CreateAbility("CommandSpellAbility",
                                            "Command",
                                            "You give the subject a single command, which it obeys to the best of its ability at its earliest opportunity.",
                                            "",
                                            dominate_person.Icon,
                                            AbilityType.Spell,
                                            Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                            AbilityRange.Close,
                                            Helpers.oneRoundDuration,
                                            Helpers.willNegates);

            command.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            command.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            command.CanTargetEnemies = true;
            command.CanTargetFriends = true;
            command.CanTargetSelf = false;
            command.CanTargetPoint = false;
            command.Animation = dominate_person.Animation;
            command.AnimationStyle = dominate_person.AnimationStyle;
            command.AddComponent(dominate_person.GetComponent<SpellDescriptorComponent>());
            command.AddComponent(dominate_person.GetComponent<SpellComponent>());
            command.AvailableMetamagic = Metamagic.Heighten | Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend;
            command.SpellResistance = true;



            for (int i = 0; i< buffs.Length; i++)
            {
                var variant_command = library.CopyAndAdd<BlueprintAbility>(command.AssetGuid, $"CommandSpell{i + 1}Ability", "");
                variant_command.SetDescription(descriptions[i]);
                variant_command.SetName($"Command ({names[i]})");

                var buff_action = Common.createContextSavedApplyBuff(buffs[i],
                                                                      Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds)
                                                                     );
                var buff_save = Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(buff_action));

                variant_command.AddComponent(Helpers.CreateRunActions(buff_save));
                variant_command.AddComponent(dominate_person.GetComponent<AbilitySpawnFx>());
                variant_command.AddComponent(dominate_person.GetComponent<AbilityTargetHasFact>());
                variant_command.AddComponents(dominate_person.GetComponents<AbilityTargetHasNoFactUnless>());
                commands.Add(variant_command);
            }

            command.AddComponent(command.CreateAbilityVariants(commands));
            command.AddToSpellList(Helpers.clericSpellList, 1);
            command.AddToSpellList(Helpers.inquisitorSpellList, 1);

            command.AddSpellAndScroll("f199f6e5026488c499042900b572eb7f"); //dominate person
        }




        static void createSanctuary()
        {
            var lesser_restoration = library.Get<BlueprintAbility>("e84fc922ccf952943b5240293669b171");
            var sancturay_logic = Helpers.Create<SanctuaryMechanics.Sanctuary>(c =>
                                                                         {
                                                                             c.save_type = SavingThrowType.Will;
                                                                             c.offensive_action_effect = SanctuaryMechanics.OffensiveActionEffect.REMOVE_FROM_OWNER;
                                                                         }
                                                                         );
            sanctuary_buff = library.CopyAndAdd<BlueprintBuff>("525f980cb29bc2240b93e953974cb325", "SanctuaryBuff", "");//invisibility

            sanctuary_buff.ComponentsArray = new BlueprintComponent[] { sancturay_logic };

            var apply_buff = Common.createContextActionApplyBuff(sanctuary_buff,
                                                                Helpers.CreateContextDuration(bonus: Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Rounds),
                                                                is_from_spell: true);

            sanctuary = Helpers.CreateAbility("SanctuaryAbility",
                                                "Sanctuary",
                                                "Any opponent attempting to directly attack the warded creature, even with a targeted spell, must attempt a Will save. If the save succeeds, the opponent can attack normally and is unaffected by that casting of the spell. If the save fails, the opponent can’t follow through with the attack, that part of its action is lost, and it can’t directly attack the warded creature for the duration of the spell. Those not attempting to attack the subject remain unaffected. This spell does not prevent the warded creature from being attacked or affected by area of effect spells. The subject cannot attack without breaking the spell but may use non-attack spells or otherwise act.",
                                                "",
                                                lesser_restoration.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.roundsPerLevelDuration,
                                                Helpers.willNegates,
                                                Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                Helpers.CreateRunActions(apply_buff)
                                                );

            sanctuary_buff.SetDescription(sanctuary.Description);
            sanctuary_buff.SetIcon(sanctuary.Icon);

            sanctuary.CanTargetSelf = true;
            sanctuary.CanTargetPoint = false;
            sanctuary.CanTargetFriends = true;
            sanctuary.CanTargetEnemies = false;
            sanctuary.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            sanctuary.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionTouch;
            sanctuary.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten;
            sanctuary.AddToSpellList(Helpers.clericSpellList, 1);
            sanctuary.AddToSpellList(Helpers.inquisitorSpellList, 1);

            sanctuary.AddSpellAndScroll("c0af0b5277e91e347ade3aa8994b0d17"); //invisibility

            //replace 1st spell in protection domain
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("b750650400d9d554b880dbf4c8347b24"), sanctuary, 1);
        }



        static internal void createShillelagh()
        {
            var boneshaker = library.Get<BlueprintAbility>("b7731c2b4fa1c9844a092329177be4c3");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var enchant_dice = Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponDamageChange>();
            enchant_dice.dice_formula = new DiceFormula(2, DiceType.D6);
            enchant_dice.bonus_damage = 0;
            enchant_dice.damage_type_description = null;
            var enchantment_size = Common.createWeaponEnchantment("ShillelaghEnchantment",
                                                                  "Shillelagh",
                                                                  "Your own non - magical club or quarterstaff becomes a weapon with a + 1 enhancement bonus on attack and damage rolls. A quarterstaff gains this enhancement for both ends of the weapon. It deals damage as if it were two size categories larger (a Small club or quarterstaff so transmuted deals 1d8 points of damage, a Medium 2d6, and a Large 3d6), +1 for its enhancement bonus. If you stop wielding it, the weapon loses magical properties.",
                                                                  "Shillelagh",
                                                                  "",
                                                                  "",
                                                                  0,
                                                                  null,
                                                                  enchant_dice);
            var enhantment1 = library.Get<BlueprintWeaponEnchantment>("d704f90f54f813043a525f304f6c0050");

            BlueprintWeaponType[] shillelagh_types = new BlueprintWeaponType[] {library.Get<BlueprintWeaponType>("26aa0672af2c7d84ba93bec37758c712"), // club
                                                                                library.Get<BlueprintWeaponType>("629736dabac7f9f4a819dc854eaed2d6")  // quarterstaff
                                                                               };
            var buff = Helpers.CreateBuff("ShillelaghBuff",
                                          enchantment_size.Name,
                                          enchantment_size.Description,
                                          "",
                                          boneshaker.Icon,
                                          null,
                                          Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), true, true, shillelagh_types, enchantment_size),
                                          Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), true, true, shillelagh_types, enhantment1)
                                          );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;


            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                                 Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes)
                                                                );

            shillelagh = Helpers.CreateAbility("ShillelaghAbility",
                                               buff.Name,
                                               buff.Description,
                                               "",
                                               buff.Icon,
                                               AbilityType.Spell,
                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                               AbilityRange.Personal,
                                               Helpers.minutesPerLevelDuration,
                                               "",
                                               bless_weapon.GetComponent<AbilitySpawnFx>(),
                                               bless_weapon.GetComponent<ContextRankConfig>(),
                                               Helpers.CreateRunActions(apply_buff),
                                               Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Transmutation),
                                               Common.createAbilityCasterMainWeaponCheck(shillelagh_types[0].Category, shillelagh_types[1].Category)
                                               );
            shillelagh.CanTargetSelf = true;
            shillelagh.CanTargetPoint = false;
            shillelagh.CanTargetFriends = false;
            shillelagh.CanTargetEnemies = false;
            shillelagh.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Omni;
            shillelagh.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;
            shillelagh.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten;
            shillelagh.AddToSpellList(Helpers.druidSpellList, 1);
            shillelagh.AddSpellAndScroll("98abe0fd52e9d7d49a4a94615acbbc60"); //boneshacker
        }


        static void createFlameBlade()
        {
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var flaming_enchatment = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");
            var scimitar_type = library.Get<BlueprintWeaponType>("d9fbec4637d71bd4ebc977628de3daf3");
            var immaterial = Helpers.Create<NewMechanics.EnchantmentMechanics.Immaterial>();
            BlueprintWeaponEnchantment[] flame_blade_enchantments = new BlueprintWeaponEnchantment[11];
            var fire_damage = Common.createEnergyDamageDescription(Kingmaker.Enums.Damage.DamageEnergyType.Fire);

            for (int i = 0; i < flame_blade_enchantments.Length; i++)
            {
                var flame_blade_enchant = Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponDamageChange>(w =>
                                                                                    {
                                                                                        w.bonus_damage = i;
                                                                                        w.dice_formula = new DiceFormula(1, DiceType.D8);
                                                                                        w.damage_type_description = fire_damage;
                                                                                    });
                flame_blade_enchantments[i] = Common.createWeaponEnchantment($"FlameBlade{i}Enchantment",
                                                                             "Flame Blade",
                                                                             "You transform a non-magical scimitar into a 3-foot-long, blazing beam of red-hot fire springs. Attacks with the flame blade are melee touch attacks. The blade deals 1d8 points of fire damage + 1 point per two caster levels (maximum +10). Since the blade is immaterial, your Strength modifier does not apply to the damage. If you stop wielding it, the weapon loses magical properties.",
                                                                             "",
                                                                             "",
                                                                             "",
                                                                             0,
                                                                             flaming_enchatment.WeaponFxPrefab,
                                                                             immaterial,
                                                                             flame_blade_enchant
                                                                             );
            }


            var empower = Common.createWeaponEnchantment("EmpowerWeaponEnchantment",
                                                         "Empowered",
                                                         "All variable, numeric effects of an empowered spell are increased by half including bonuses to those dice rolls.",
                                                         "",
                                                         "",
                                                         "",
                                                         0,
                                                         null,
                                                         Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponMetamagicDamage>(w => w.empower = true)
                                                         );
            var maximize = Common.createWeaponEnchantment("MaximizeWeaponEnchantment",
                                                         "Maximized",
                                                         "All variable, numeric effects of a spell are maximized.",
                                                         "",
                                                         "",
                                                         "",
                                                         0,
                                                         null,
                                                         Helpers.Create<NewMechanics.EnchantmentMechanics.WeaponMetamagicDamage>(w => w.maximize = true)
                                                         );


            var empower_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Empower,
                                                                                                  true, true,
                                                                                                  new BlueprintWeaponType[] { scimitar_type }, empower);

            var maximize_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Maximize,
                                                                                                  true, true,
                                                                                                  new BlueprintWeaponType[] { scimitar_type }, maximize);


            var buff = Helpers.CreateBuff("FlameBladeBuff",
                                            flame_blade_enchantments[0].Name,
                                            flame_blade_enchantments[0].Description,
                                            "",
                                            bless_weapon.Icon,
                                            null,
                                            Common.createBuffContextEnchantPrimaryHandWeapon(Helpers.CreateContextValue(AbilityRankType.DamageBonus), true, true,
                                                                                            new BlueprintWeaponType[] { scimitar_type }, flame_blade_enchantments),
                                            empower_buff,
                                            maximize_buff,
                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                            type: AbilityRankType.DamageBonus, stepLevel: 2)
                                            );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;

            flame_blade = library.CopyAndAdd<BlueprintAbility>(shillelagh.AssetGuid, "FlameBladeAbility", "");
            flame_blade.SetIcon(bless_weapon.Icon);
            flame_blade.SetName(buff.Name);
            flame_blade.SetDescription(buff.Description);

            flame_blade.ReplaceComponent<AbilityCasterMainWeaponCheck>(Common.createAbilityCasterMainWeaponCheck(scimitar_type.Category));
            flame_blade.ReplaceComponent<SpellComponent>(Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Evocation));

            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                    Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes)
                                                );
            flame_blade.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(apply_buff));
            flame_blade.AvailableMetamagic = flame_blade.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Empower | Kingmaker.UnitLogic.Abilities.Metamagic.Maximize;
            flame_blade.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Fire));

            flame_blade.AddToSpellList(Helpers.druidSpellList, 2);
            flame_blade.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502"); //bless weapon
        }


        static void createVirtuosoPerformance()
        {
            var performance_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            var inspire_competence = library.Get<BlueprintActivatableAbility>("430ab3bb57f2cfc46b7b3a68afd4f74e");
            var increase_group_size = Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.BardicPerformance);
            var consume_additional_resource = Helpers.Create<NewMechanics.ConsumeResourceIfAbilitiesFromGroupActivated>(c =>
                                                                                                                        {
                                                                                                                            c.group = ActivatableAbilityGroup.BardicPerformance;
                                                                                                                            c.num_abilities_activated = 2;
                                                                                                                            c.resource = performance_resource;
                                                                                                                        }
                                                                                                                        );
            var deactivate_performance = Helpers.Create<NewMechanics.DeactivateAbilityFromGroup>(c =>
                                                                                                    {
                                                                                                        c.group = ActivatableAbilityGroup.BardicPerformance;
                                                                                                        c.num_abilities_activated = 1;
                                                                                                    }
                                                                                                  );
            virtuoso_performance = library.CopyAndAdd<BlueprintAbility>("20b548bf09bb3ea4bafea78dcb4f3db6", "VirtuosoPerformanceAbility", ""); //echolocation
            virtuoso_performance.SetIcon(inspire_competence.Icon);
            virtuoso_performance.SetName("Virtuoso Performance");
            virtuoso_performance.SetDescription("While this spell is active, you may start a second bardic performance while maintaining another. Starting the second performance costs 2 rounds of bardic performance instead of 1. Maintaining both performances costs a total of 3 rounds of bardic performance for each round they are maintained. When this spell ends, one of the performances ends immediately.");
            virtuoso_performance.RemoveComponents<SpellListComponent>();
            virtuoso_performance.RemoveComponents<AbilityEffectRunAction>();
            virtuoso_performance.LocalizedDuration = Helpers.roundsPerLevelDuration;

            var buff = Helpers.CreateBuff("VirtuosoPerformanceBuff",
                                          virtuoso_performance.Name,
                                          virtuoso_performance.Description,
                                          "",
                                          virtuoso_performance.Icon,
                                          null,
                                          increase_group_size,
                                          Helpers.CreateAddFactContextActions(newRound: consume_additional_resource, deactivated: deactivate_performance)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                                 Helpers.CreateContextDuration(bonus: Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Rounds),
                                                                 is_from_spell: true);

            virtuoso_performance.AddComponent(Helpers.CreateRunActions(apply_buff));
            virtuoso_performance.AddToSpellList(Helpers.bardSpellList, 4);
            virtuoso_performance.AddSpellAndScroll("33770ff24b320e343bb767815f800fc4"); //echolocation
        }


        static internal void createDeadlyJuggernaut()
        {
            var sneak_attack = library.Get<BlueprintFeature>("df4f34f7cac73ab40986bc33f87b1a3c");
            var false_life = library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762");
            deadly_juggernaut = library.CopyAndAdd<BlueprintAbility>("779179912e6c6fe458fa4cfb90d96e10", "DeadlyJuggernautAbility", "");
            deadly_juggernaut.RemoveComponents<SpellListComponent>();
            deadly_juggernaut.ReplaceComponent<AbilitySpawnFx>(false_life.GetComponent<AbilitySpawnFx>());
            deadly_juggernaut.ReplaceComponent<SpellComponent>(false_life.GetComponent<SpellComponent>());

            deadly_juggernaut.SetIcon(sneak_attack.Icon);
            deadly_juggernaut.SetName("Deadly Juggernaut");
            deadly_juggernaut.SetDescription("With every enemy life you take, you become increasingly dangerous and difficult to stop. During the duration of the spell, you gain a cumulative +1 luck bonus on melee attack rolls, melee weapon damage rolls, Strength checks, and Strength-based skill checks as well as DR 2/— each time you reduce a qualifying opponent to 0 or few hit points (maximum +5 bonus and DR 10/—) with a melee attack.");
            deadly_juggernaut.RemoveComponents<AbilityEffectRunAction>();


            BlueprintBuff[] buffs = new BlueprintBuff[5];

            for (int i = 0; i < buffs.Length; i++)
            {
                int bonus = i + 1;
                buffs[i] = Helpers.CreateBuff($"DeadlyJuggernaut{i + 1}Buff",
                                              $"Deadly Juggernaut (+{i + 1})",
                                              deadly_juggernaut.Description,
                                              "",
                                              deadly_juggernaut.Icon,
                                              null,
                                              Common.createAttackTypeAttackBonus(Common.createSimpleContextValue(bonus), AttackTypeAttackBonus.WeaponRangeType.Melee, ModifierDescriptor.Luck),
                                              Helpers.CreateAddStatBonus(StatType.AdditionalDamage, bonus, ModifierDescriptor.Luck),
                                              Common.createAbilityScoreCheckBonus(Common.createSimpleContextValue(bonus), ModifierDescriptor.Luck, StatType.Strength),
                                              Helpers.CreateAddStatBonus(StatType.SkillAthletics, bonus, ModifierDescriptor.Luck),
                                              Common.createPhysicalDR(bonus * 2)
                                              );
            }

            var conditional = Helpers.CreateConditional(Helpers.CreateConditionHasBuff(buffs[0]),
                                                        new GameAction[] {Common.createContextActionApplyBuff(buffs[1], Helpers.CreateContextDuration(), is_from_spell: true, is_child: true, is_permanent: true),
                                                                           Common.createContextActionRemoveBuff(buffs[0])
                                                                          },
                                                        new GameAction[] { Common.createContextActionApplyBuff(buffs[0], Helpers.CreateContextDuration(), is_from_spell: true, is_child: true, is_permanent: true) }
                                                       );
            for (int i = 1; i < buffs.Length; i++)
            {
                GameAction[] if_true = null;
                if (i < buffs.Length - 1)
                {
                    if_true = new GameAction[] {Common.createContextActionRemoveBuff(buffs[i]),
                                                Common.createContextActionApplyBuff(buffs[i+1],
                                                                                    Helpers.CreateContextDuration(),
                                                                                    is_from_spell: true,
                                                                                    is_child: true,
                                                                                    is_permanent:true)
                                               };
                }
                conditional = Helpers.CreateConditional(Helpers.CreateConditionHasBuff(buffs[i]),
                                                        if_true,
                                                        new GameAction[] { conditional });

            }




            var on_kill = Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(conditional),
                                                                           reduce_hp_to_zero: true, 
                                                                           check_weapon_range_type: true, 
                                                                           on_initiator: true,
                                                                           range_type: AttackTypeAttackBonus.WeaponRangeType.Melee);

            var buff = Helpers.CreateBuff("DeadlyJuggernautBuff",
                                          deadly_juggernaut.Name,
                                          deadly_juggernaut.Description,
                                          "",
                                          deadly_juggernaut.Icon,
                                          null,
                                          on_kill);
            buff.Stacking = StackingType.Replace;
            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                     Helpers.CreateContextDuration(bonus: Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Minutes),
                                                     is_from_spell: true);
            deadly_juggernaut.AddComponent(Helpers.CreateRunActions(apply_buff));


            deadly_juggernaut.AddToSpellList(Helpers.clericSpellList, 3);
            deadly_juggernaut.AddToSpellList(Helpers.inquisitorSpellList, 3);
            deadly_juggernaut.AddToSpellList(Helpers.paladinSpellList, 3);
            deadly_juggernaut.AddSpellAndScroll("539ff89add7d8e4409ab92df30e6afee"); //lead_blades
        }


        static internal void createInvisibilityPurge()
        {
            var invisibility = library.Get<BlueprintBuff>("525f980cb29bc2240b93e953974cb325");
            var invisibility_greater = library.Get<BlueprintBuff>("e6b35473a237a6045969253beb09777c");
            var divination_area = library.Get<BlueprintAbilityAreaEffect>("4ba26a4641c911d4487e3f7f11bcf801");
            var area_effect = Helpers.Create<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect>();
            area_effect.name = "InvisibilityPurgeArea";
            area_effect.AffectEnemies = true;
            area_effect.AggroEnemies = true;
            area_effect.Size = 30.Feet();
            area_effect.Shape = AreaEffectShape.Cylinder;

            var remove_invisibility = new GameAction[] { Common.createContextActionRemoveBuff(invisibility), Common.createContextActionRemoveBuff(invisibility_greater) };

            area_effect.AddComponent(Helpers.CreateAreaEffectRunAction(unitEnter: remove_invisibility, round: remove_invisibility));
            area_effect.Fx = divination_area.Fx;
            library.AddAsset(area_effect, "");

            var see_invisibility = library.Get<BlueprintAbility>("30e5dc243f937fc4b95d2f8f4e1b7ff3");
            var buff = Helpers.CreateBuff("InvisibilityPurgeBuff",
                                          "Invisibility Purge",
                                          "You surround yourself with a sphere of power with a radius of 30 feet that negates all forms of invisibility.\n" +
                                          "Anything invisible becomes visible while in the area.",
                                          "",
                                          see_invisibility.Icon,
                                          null,
                                          Common.createAddAreaEffect(area_effect)
                                          );

            var apply_buff = Helpers.CreateApplyBuff(buff, 
                                                     Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), 
                                                     fromSpell: true, dispellable: true, asChild: true);
            invisibility_purge = Helpers.CreateAbility("InvisibilityPurgeAbility",
                                                       buff.Name,
                                                       buff.Description,
                                                       "",
                                                       buff.Icon,
                                                       AbilityType.Spell,
                                                       Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                       AbilityRange.Personal,
                                                       Helpers.minutesPerLevelDuration,
                                                       Helpers.savingThrowNone,
                                                       Helpers.CreateRunActions(apply_buff),
                                                       Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                       see_invisibility.GetComponent<AbilitySpawnFx>(),
                                                       see_invisibility.GetComponent<ContextRankConfig>()
                                                       );
            invisibility_purge.Animation = see_invisibility.Animation;
            invisibility_purge.AnimationStyle = see_invisibility.AnimationStyle;
            invisibility_purge.CanTargetSelf = true;
            invisibility_purge.CanTargetPoint = false;
            invisibility_purge.CanTargetEnemies = false;
            invisibility_purge.CanTargetFriends = false;
            invisibility_purge.AvailableMetamagic = see_invisibility.AvailableMetamagic;

            invisibility_purge.AddToSpellList(Helpers.inquisitorSpellList, 3);
            invisibility_purge.AddToSpellList(Helpers.clericSpellList, 3);

            invisibility_purge.AddSpellAndScroll("12f4ee72c02537244b5b2bacfa236bc7"); //see invisibility scroll
        }



    }
}
