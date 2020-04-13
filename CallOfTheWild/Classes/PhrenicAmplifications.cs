using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class PhrenicAmplificationsEngine
    {
        static LibraryScriptableObject library => Main.library;
        //intense focus ?
        //ongoing defense
        //relentness casting

        BlueprintAbilityResource resource;
        BlueprintSpellbook spellbook;
        BlueprintCharacterClass character_class;
        string name_prefix;

        public PhrenicAmplificationsEngine(BlueprintAbilityResource pool_resource, BlueprintSpellbook linked_spellbook, BlueprintCharacterClass linked_class, string asset_prefix)
        {
            resource = pool_resource;
            spellbook = linked_spellbook;
            character_class = linked_class;
            name_prefix = asset_prefix;
        }


        public BlueprintFeature createFocusedForce()
        {
            var buff = Helpers.CreateBuff(name_prefix + "FocusedForceBuff",
                                          "Focused Force",
                                          "When casting a force spell, the psychic can increase the spell’s damage by spending 1 point from her phrenic pool. Increase the die size for the spell’s damage by one step (from 1d4 to 1d6, 1d6 to 1d8, 1d8 to 1d10, or 1d10 to 1d12). This increases the size of each die rolled, so a spell that dealt 4d6+3 points of force damage would deal 4d8+3 points of force damage instead. This amplification can be linked only to spells that deal force damage, and only if that damage includes a die value. A spell that already uses d12s for damage can’t be amplified in this way.",
                                          "",
                                          Helpers.GetIcon("0a2f7c6aa81bc6548ac7780d8b70bcbc"),
                                          null,
                                          Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.spell_descriptor = SpellDescriptor.Force;
                                                                                                               s.resource = resource;
                                                                                                               s.spellbook = spellbook;                                                                                                             
                                                                                                               s.amount = 1; }),
                                          Helpers.Create<OnCastMechanics.SpellDamageDiceIncrease>(s => {s.spellbook = spellbook; s.SpellDescriptor = SpellDescriptor.Force; })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            return Common.ActivatableAbilityToFeature(toggle, false);
        }


        public BlueprintFeature createWillOfTheDead()
        {
            var will_of_the_dead = library.Get<BlueprintFeature>("1a5e7191279e7cd479b17a6ca438498c");
            var buff = Helpers.CreateBuff(name_prefix + "WillOfTheDeadBuff",
                                          "Will of the Dead",
                                          "Even undead creatures can be affected by the psychic’s mind-affecting spells. The psychic can spend 2 points from her phrenic pool to overcome an undead creature’s immunity to mind-affecting effects for the purposes of the linked spell. This ability functions even on mindless undead, but has no effect on creatures that aren’t undead. This amplification can be linked only to spells that have the mind-affecting descriptor.",
                                          "",
                                          will_of_the_dead.Icon,
                                          null,
                                          Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.resource = resource; s.spell_descriptor = SpellDescriptor.MindAffecting; s.amount = 2; }),
                                          Helpers.CreateAddFact(will_of_the_dead)
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                             Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 2; })
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            return Common.ActivatableAbilityToFeature(toggle, false);
        }


        public BlueprintFeature createMindsEye()
        {         
            var buff = Helpers.CreateBuff(name_prefix + "MindsEyeBuff",
                                          "Mind's Eye",
                                          "ome psychics train their visual and psychic senses, binding them together into a unified focus to better guide their ranged spells and place them with uncanny precision. While casting a spell that requires a ranged attack roll, the psychic can spend 2 points from her phrenic pool and gain a +4 insight bonus on the attack roll.",
                                          "",
                                          Helpers.GetIcon("3c08d842e802c3e4eb19d15496145709"), //uncanny dodge
                                          null,
                                          Helpers.Create<OnCastMechanics.RangedSpellAttackRollBonus>(s => { s.spellbook = spellbook; s.bonus = 4; s.descriptor = ModifierDescriptor.UntypedStackable; s.resource = resource; s.amount = 2; })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                             Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 2; })
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            return Common.ActivatableAbilityToFeature(toggle, false);
        }


        public BlueprintFeature createConjuredArmor()
        {
            var effect_buff = Helpers.CreateBuff(name_prefix + "ConjuredArmorEffectBuff",
                                          "Conjured Armor",
                                          "By spending 1 point from her phrenic pool, the psychic grants any creature she conjures or summons with the linked spell a +2 deflection bonus to AC. This bonus lasts for 1 round per caster level or until the creature disappears, whichever comes first. This amplification can be linked only to conjuration (calling) or conjuration (summoning) spells. The bonus increases to +3 at 8th level and to +4 at 15th level.",
                                          "",
                                          Helpers.GetIcon("38155ca9e4055bb48a89240a2055dcc3"), //augmented summoning
                                          null,
                                          Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, Kingmaker.Enums.ModifierDescriptor.Deflection, ContextValueType.Rank, AbilityRankType.Default),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Custom, AbilityRankType.Default,
                                                                          customProgression: new (int, int)[] { (7, 2), (14, 3), (20, 4) },
                                                                          classes: new BlueprintCharacterClass[] {character_class}
                                                                          )
                                          );

            var buff = Helpers.CreateBuff(name_prefix + "ConjuredArmorBuff",
                                          effect_buff.Name,
                                          effect_buff.Description,
                                          "",
                                          Helpers.GetIcon("38155ca9e4055bb48a89240a2055dcc3"), //augmented summoning
                                          null,
                                          Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.resource = resource; s.spellbook = spellbook; s.school = SpellSchool.Conjuration; s.spell_descriptor = SpellDescriptor.Summoning; s.amount = 1; }),
                                          Helpers.Create<OnCastMechanics.OnSpawnBuff>(s => { s.spellbook = spellbook; s.school = SpellSchool.Conjuration; s.spell_descriptor = SpellDescriptor.Summoning; s.buff = effect_buff; s.duration_value = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)); }),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { character_class })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            return Common.ActivatableAbilityToFeature(toggle, false);
        }


        public BlueprintFeature createBiokineticHealing()
        {
            var buff = Helpers.CreateBuff(name_prefix + "BiokineticHealingBuff",
                                          "Biokinetic Healing",
                                          "When the psychic casts a linked spell from the transmutation school, she can spend 1 point from her phrenic pool to regain 2 hit points per level of the linked spell.",
                                          "",
                                          Helpers.GetIcon("8d6073201e5395d458b8251386d72df1"), //lay on hands self
                                          null,
                                          Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.resource = resource; s.spellbook = spellbook; s.school = SpellSchool.Transmutation; s.amount = 1; }),
                                          Helpers.Create<OnCastMechanics.HealAfterSpellCast>(s => { s.spellbook = spellbook; s.school = SpellSchool.Transmutation; s.multiplier = 2; })
                                          );

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                             resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                             );
            toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
            return Common.ActivatableAbilityToFeature(toggle, false);          
        }





        public BlueprintFeature createDefensivePrognostication()
        {
            var toggles = new BlueprintActivatableAbility[2];

            for (int i = 0; i < 2; i++)
            {
                var effect_buff = Helpers.CreateBuff(name_prefix + $"DefensivePrognostication{i + 1}EffectBuff",
                                                      "Defensive Prognostication " + Common.roman_id[i + 1],
                                                      "When casting a divination spell, the psychic sees a glimmer of her future. By spending 1 point from her phrenic pool as she casts a divination spell, she gains a +2 insight bonus to AC for a number of rounds equal to the linked spell’s level. She can instead spend 2 points to increase the bonus to +4. This amplification can be linked only to divination spells.",
                                                      "",
                                                      Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //armor
                                                      null,
                                                      Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, (i+1)*2, Kingmaker.Enums.ModifierDescriptor.Insight)
                                                      );

                var buff = Helpers.CreateBuff(name_prefix + $"DefensivePrognostication{i+1}Buff",
                                              effect_buff.Name,
                                              effect_buff.Description,
                                              "",
                                              Helpers.GetIcon("9e1ad5d6f87d19e4d8883d63a6e35568"), //armor
                                              null,
                                              Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.resource = resource; s.spellbook = spellbook; s.school = SpellSchool.Divination; s.amount = i+1; }),
                                              Helpers.Create<OnCastMechanics.ApplyBuffAfterSpellCast>(s => { s.spellbook = spellbook; s.school = SpellSchool.Divination; s.buff = effect_buff; })
                                              );

                var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                                 resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                 );
                toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();
                if (i > 0)
                {
                    toggle.AddComponent(Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 2; }));
                }
                toggles[i] = toggle;
            }
            var feature = Helpers.CreateFeature(name_prefix + "DefensivePrognosticationFeature",
                                                "Defensive Prognostication",
                                                toggles[0].Description,
                                                "",
                                                toggles[0].Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFacts(toggles)
                                                );
            return feature;
        }


        public BlueprintFeature createOverpoweringMind()
        {
            var feature = Helpers.CreateFeature(name_prefix + "Overpowering MindFeature",
                              "Overpowering Mind",
                              "The psychic can spend 2 points from her phrenic pool to increase the save DC of the linked spell by 1. At 8th level, she can choose to instead spend 4 points to increase the DC by 2. At 15th level, she can choose to instead spend 6 points to increase the DC by 3. This amplification can be linked only to spells that have the mind - affecting descriptor.",
                              "",
                              Helpers.GetIcon("eabf94e4edc6e714cabd96aa69f8b207"), //mind fog
                              FeatureGroup.None
                              );
            for (int i = 0; i < 3; i++)
            {
                var buff = Helpers.CreateBuff(name_prefix + $"OverpoweringMind{i+1}Buff",
                                              "Overpowering Mind " + Common.roman_id[i+1],
                                              feature.Description,
                                              "",
                                              feature.Icon,
                                              null,
                                              Helpers.Create<NewMechanics.SpendResourceOnSpecificSpellCast>(s => { s.resource = resource; s.spellbook = spellbook; s.spell_descriptor = SpellDescriptor.MindAffecting; s.amount = 2*(i+1); }),
                                              Helpers.Create<NewMechanics.ContextIncreaseDescriptorSpellsDC>(c => { c.spellbook = spellbook; c.Descriptor = SpellDescriptor.MindAffecting; c.Value = i + 1; })
                                              );

                var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                                 resource.CreateActivatableResourceLogic(spendType: ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                 );
                toggle.AddComponent(Helpers.Create<ResourceMechanics.RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = 2 * (i+1); }));
                toggle.Group = ActivatableAbilityGroupExtension.PhrenicAmplification.ToActivatableAbilityGroup();

                if (i == 0)
                {
                    feature.AddComponent(Helpers.CreateAddFact(toggle));
                }
                else
                {
                    feature.AddComponent(Helpers.CreateAddFeatureOnClassLevel(Common.ActivatableAbilityToFeature(toggle), 1 + i*7, new BlueprintCharacterClass[] { character_class }));
                }
            }
            return feature;
        }
    }
}
