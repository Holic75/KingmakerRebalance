using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    public partial class ImplementsEngine
    {
        public BlueprintFeature createServitor()
        {
            string[] summon_monster_guids = new string[] {"8fd74eddd9b6c224693d9ab241f25e84", "7ab27a0d547742741beb5d089f1c3852", "15b5efe371d47c944b58444e7b734ffb",
                                                          "efa433a38e9c7c14bb4e780f8a3fe559", "0964bf88b582bed41b74e79596c4f6d9", "02de4dd8add69aa42a3d1330b573e2ab",
                                                          "2920d48574933c24391fbb9e18f87bf5" };

            var summon_m7 = library.Get<BlueprintAbility>("2920d48574933c24391fbb9e18f87bf5");

            List<ActionList> summon_actions = new List<ActionList>();
            foreach (var s in summon_monster_guids)
            {
                summon_actions.Add(library.Get<BlueprintAbility>(s).GetComponent<AbilityEffectRunAction>().Actions);
            }

            var ability = library.CopyAndAdd<BlueprintAbility>(summon_m7.AssetGuid, prefix + "ServitorAbility", "");
            Common.unsetAsFullRoundAction(ability);
            ability.SetName("Servitor");
            ability.SetDescription("As a standard action, you can expend 1 point of mental focus to summon a servitor.\nThis ability functions as summon monster I, but you can use it only to summon a single creature, and the effect lasts for 1 minute. At 4th level and every 3 levels thereafter, the level of the summon monster spell increases by 1, to a maximum of summon monster VII at 19th level.");
            ability.RemoveComponents<SpellComponent>();
            ability.Type = AbilityType.SpellLike;
            var action = Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus), summon_actions.ToArray()));
            ability.ReplaceComponent<AbilityEffectRunAction>(action);
            ability.ReplaceComponent<ContextRankConfig>(Helpers.CreateContextRankConfig(min: 10, max: 10));
            ability.AddComponents(createClassScalingConfig(progression: ContextRankProgression.StartPlusDivStep, startLevel: 1, stepLevel: 3, type: AbilityRankType.StatBonus),
                                  resource.CreateResourceLogic());
            ability.LocalizedDuration = Helpers.oneMinuteDuration;
            ability.Parent = null;
            addFocusInvestmentCheck(ability, SpellSchool.Conjuration);

            return Common.AbilityToFeature(ability, false);
        }


        public BlueprintFeature createFleshMend()
        {
            var icon = Helpers.GetIcon("caae1dc6fcf7b37408686971ee27db13");//lay on hands

            var ability = Helpers.CreateAbility(prefix + "FleshMendAbility",
                                                "Flesh Mend",
                                                $"As a standard action, you can expend 1 point of mental focus to heal a living creature with a touch. The creature is healed an amount of damage equal to 1d{BalanceFixes.getDamageDieString(DiceType.D8)} + your occultist level. For every 4 occultist levels you possess beyond 3rd, the creature is healed an additional 1d{BalanceFixes.getDamageDieString(DiceType.D8)} points of damage, to a maximum of 5d{BalanceFixes.getDamageDieString(DiceType.D8)}+19 at 19th level. This has no effect on undead creatures. You must be at least 3rd level to select this focus power.",
                                                "",
                                                icon,
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(BalanceFixes.getDamageDie(DiceType.D8), Helpers.CreateContextValue(AbilityRankType.DamageDice), Helpers.CreateContextValue(AbilityRankType.DamageBonus)))),
                                                createClassScalingConfig(type: AbilityRankType.DamageBonus),
                                                createClassScalingConfig(type: AbilityRankType.DamageDice, progression: ContextRankProgression.StartPlusDivStep, startLevel: 3, stepLevel: 4),
                                                Common.createAbilitySpawnFx("224fb8fd952ec4d45b6d3436a77663d9", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Helpers.Create<AbilityUseOnRest>(a => a.Type = AbilityUseOnRestType.HealDamage),
                                                Common.createAbilityTargetHasFact(true, Common.undead, Common.construct, Common.elemental),
                                                resource.CreateResourceLogic(),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.Cure | SpellDescriptor.RestoreHP),
                                                Helpers.CreateSpellComponent(SpellSchool.Conjuration)
                                                );
            ability.setMiscAbilityParametersTouchFriendly();
            addFocusInvestmentCheck(ability, SpellSchool.Conjuration);

            var feature =  Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 3);
            return feature;
        }


        public BlueprintFeature createPsychicFog()
        {
            var fog_cloud = Common.convertToSpellLike(NewSpells.obscuring_mist, prefix, classes, stat, resource, archetypes: getArchetypeArray());
            var solid_fog = Common.convertToSpellLike(NewSpells.solid_fog, prefix, classes, stat, resource, archetypes: getArchetypeArray(), cost: 2);

            fog_cloud.SetName("Psychic Fog (Obscuring Mist)");
            solid_fog.SetName("Psychic Fog (Solid Fog)");
            solid_fog.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfHasClassLevels>(a => { a.level = 7; a.character_classes = classes; }));

            var wrapper = Common.createVariantWrapper(prefix + "PsychicFogBase", "", fog_cloud, solid_fog);
            wrapper.SetNameDescription("Psychic Fog",
                                       "As a standard action, you can expend 1 point of mental focus to create a cloud of fog. This fog lasts for 1 minute per occultist level you possess. It functions as fog cloud, except it can’t be dispersed by wind. At 7th level, You can expend 1 additional point of mental focus when creating this fog, causing it to become more tangible and function as solid fog.");

            addFocusInvestmentCheck(wrapper, SpellSchool.Conjuration);
            var feature = Common.AbilityToFeature(wrapper, false);
            return feature;
        }


        public BlueprintFeature createPurgeCorruption()
        {
            var neutralize_poison = library.Get<BlueprintAbility>("e7240516af4241b42b2cd819929ea9da");
            var remove_disease = library.Get<BlueprintAbility>("4093d5a0eb5cae94e909eb1e0e1a6b36");

            var ability_neutralize_poison = Common.convertToSpellLike(neutralize_poison, prefix, classes, stat, resource, archetypes: getArchetypeArray());
            var ability_remove_disease = Common.convertToSpellLike(remove_disease, prefix, classes, stat, resource, archetypes: getArchetypeArray());
            ability_neutralize_poison.SetName("Purge Corruption: " + ability_neutralize_poison.Name);
            ability_remove_disease.SetName("Purge Corruption: " + ability_remove_disease.Name);

            var wrapper = Common.createVariantWrapper(prefix + "PurgeCorruptionBase", "", ability_neutralize_poison, ability_remove_disease);
            wrapper.SetNameDescription("Purge Corruption", "As a standard action, you can expend 1 point of mental focus to draw out the corruption from a creature.\nThis ability functions as either neutralize poison or remove disease, using your occultist level as the caster level. Each use of this ability can cure only one poison or one disease. You must be at least 5th level to select this focus power.");

            addFocusInvestmentCheck(wrapper, SpellSchool.Conjuration);
            var feature = Common.AbilityToFeature(wrapper, false);
            addMinLevelPrerequisite(feature, 5);
            return feature;
        }


        public BlueprintFeature createSideStep()
        {
            var dimension_door = library.Get<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2");
            var description = "You can create a temporary fissure in space by expending 1 point of mental focus. You can use this ability as a move action. The fissure begins in any square you designate and allows you to teleport to any other square you can see within 10 feet per occultist level.\nYou must be at least 7th level to select this focus power.";

            var feature = Helpers.CreateFeature(prefix + "SideStepFeature",
                                          "Side Step",
                                          description,
                                          "",
                                          dimension_door.Icon,
                                          FeatureGroup.None
                                          );
           
            for (int i = 1; i <= 20; i++)
            {
                var ability = library.CopyAndAdd(dimension_door, prefix + $"SideStep{i * 5}Ability", "");
                ability.Parent = null;
                ability.ActionType = CommandType.Move;
                ability.Range = AbilityRange.Custom;
                ability.CustomRange = (i * 10).Feet();
                ability.Type = AbilityType.Supernatural;

                ability.SetNameDescription("Side Step", description);
                ability.AddComponent(resource.CreateResourceLogic());
                var feature_i = Common.AbilityToFeature(ability);
                int min_level = i == 1 ? 0 : i;
                int max_level = i == 20 ? 100 : i;

                addFocusInvestmentCheck(ability, SpellSchool.Conjuration);
                feature.AddComponent(createAddFeatureInLevelRange(feature_i, min_level, max_level));              
            }

            addMinLevelPrerequisite(feature, 7);
            return feature;
        }


        public BlueprintBuff createCastingFocus()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "CastingFocusProperty", "",
                                                                                                  createClassScalingConfig(ContextRankProgression.MultiplyByModifier, type: AbilityRankType.StatBonus, stepLevel: 2),//2 * lvl
                                                                                                  false,
                                                                                                  SpellSchool.Conjuration);
            var buff = Helpers.CreateBuff(prefix + "CastingFocusBuff",
                                          "Casting Focus",
                                          "The implement empowers the bearer’s ties to the worlds beyond, allowing his spells to maintain their power for a longer period of time. The bearer can add the implement as an additional focus component to any summoning conjuration spell he casts that has a duration measured in rounds per level. If he does so, he adds 1 to his caster level for every 2 points of mental focus stored in the implement (to a maximum bonus equal to your occultist level). This increase applies only when determining the duration of the spell. Apply this increase after other effects that adjust a spell’s duration, such as Extend Spell.",
                                          "",
                                          Helpers.GetIcon("38155ca9e4055bb48a89240a2055dcc3"), //augment summoning
                                          null,
                                          Helpers.Create<NewMechanics.AddContextValueToSummonDuration>(a => a.value = Helpers.CreateContextValue(AbilityRankType.Default)),
                                          Helpers.Create<OnCastMechanics.IncreaseBuffDurationForSchool>(i => { i.value = Helpers.CreateContextValue(AbilityRankType.Default); i.school = SpellSchool.Conjuration;  }),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.DivStep, stepLevel: 2,
                                                                          customProperty: property)                                        
                                          );
            return buff;
        }
    }
}
