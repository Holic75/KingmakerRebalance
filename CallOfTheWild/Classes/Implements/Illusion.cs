using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
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
        public BlueprintFeature createColorBeam()
        {
            var blinding_ray = library.Get<BlueprintAbility>("9b4d07751dd104243a94b495c571c9dd");

            var ability = library.CopyAndAdd(blinding_ray, prefix + "ColorBeam", "");
            ability.SpellResistance = true;
            ability.RemoveComponents<AbilityResourceLogic>();
            ability.RemoveComponents<ContextRankConfig>();
            ability.AddComponent(createClassScalingConfig(type: AbilityRankType.DamageDice));
            ability.AddComponent(resource.CreateResourceLogic());
            Common.addSpellDescriptor(ability, SpellDescriptor.MindAffecting);
            ability.SetNameDescription("Color Beam",
                                       "As a standard action, you can expend 1 point of mental focus to unleash a beam of cascading colors at any one target within 30 feet. Doing so requires a ranged touch attack. If the beam hits, the target is blinded for 1 round if it has a number of Hit Dice equal to or lower than your occultist level. A foe with a number of Hit Dice greater than your occultist level is instead dazzled for 1 round.\n"
                                       + "This is a mind-affecting illusion effect."
                                       );
            addFocusInvestmentCheck(ability, SpellSchool.Illusion);
            return Common.AbilityToFeature(ability, false);
        }


        public BlueprintFeature createTerror()
        {
            var effect = Helpers.CreateConditional(Helpers.Create<ContextConditionHitDice>(c => { c.HitDice = 0; c.AddSharedValue = true; c.SharedValue = AbilitySharedValue.StatBonus; }),
                                                   Helpers.Create<AooMechanics.ContextActionProvokeAttackOfOpportunityFromAnyoneExceptCaster>(a => a.max_units = 100));
            var ability = Helpers.CreateAbility(prefix + "TerrorAbility",
                                                "Terror",
                                                "As a standard action, by spending one point of mental focus, you can make a melee touch attack that causes a creature to be assailed by nightmares only it can see. The creature provokes an attack of opportunity from all your allies. Creatures with more Hit Dice than your occultist level are unaffected. This is a mind-affecting fear effect.",
                                                "",
                                                Helpers.GetIcon("d2aeac47450c76347aebbc02e4f463e0"), //fear
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(effect),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                Helpers.CreateDeliverTouch(),
                                                Common.createAbilitySpawnFx("49a8069c238b1a8429f2123654d4f45b", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                createClassScalingConfig(),
                                                Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default)), AbilitySharedValue.StatBonus)                                               
                                                );
            ability.setMiscAbilityParametersTouchHarmful();
            var ability_cast = Helpers.CreateTouchSpellCast(ability, resource);
            addFocusInvestmentCheck(ability, SpellSchool.Illusion);
            return Common.AbilityToFeature(ability_cast, false);
        }


        public BlueprintFeature createShadowBeast()
        {
            var wizard_spelllist = library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");
            var ability = Helpers.CreateAbility(prefix + "ShadowBeasts",
                                               "Shadow Beasts",
                                               "As a standard action, you can call forth one or more beasts made of shadow by expending 1 point of mental focus. This functions as shadow conjuration, but it can be used to duplicate only the effects of summon monster spells. Creatures created with this spell deal 60% of the normal damage to those that disbelieve the illusion, and their nondamaging effects have only a 60% chance of affecting disbelieving targets. This can be used to duplicate any summon monster spell up to summon monster V. For every 2 additional levels you possess beyond 9th, the maximum spell level you can duplicate with this ability increases by 1 (to a maximum of summon monster IX at 17th level). ",
                                               "",
                                               LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormOfSouls.png"),
                                               AbilityType.SpellLike,
                                               UnitCommand.CommandType.Standard,
                                               AbilityRange.Unlimited,
                                               Helpers.roundsPerLevelDuration,
                                               "Will disbelief",
                                               Helpers.CreateSpellComponent(SpellSchool.Illusion),
                                               Helpers.CreateSpellDescriptor(SpellDescriptor.Summoning | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60)
                                               );

            var spell_guids = new string[]
            {
                "8fd74eddd9b6c224693d9ab241f25e84",//summon monster 1
                "1724061e89c667045a6891179ee2e8e7",//summon elemental small
                "970c6db48ff0c6f43afc9dbb48780d03",//summon monster 2
                "5d61dde0020bbf54ba1521f7ca0229dc",//summon monster 3
                "e42b1dbff4262c6469a9ff0a6ce730e3",//summon elemental medium
                "7ed74a3ec8c458d4fb50b192fd7be6ef",//summon monster 4
                "89404dd71edc1aa42962824b44156fe5",//summon elemental large
                "630c8b85d9f07a64f917d79cb5905741",//summon monster 5
                "766ec978fa993034f86a372c8eb1fc10",//summon elemental huge
                "e740afbab0147944dab35d83faa0ae1c",//summon monster 6
                "8eb769e3b583f594faabe1cfdb0bb696",//summon elemental greater
                "ab167fd8203c1314bac6568932f1752f",//summon monster 7
                "8a7f8c1223bda1541b42fd0320cdbe2b",//summon elder elemental
                "d3ac756a229830243a72e84f3ab050d0",//summon monster 8
                "52b5df2a97df18242aec67610616ded0",//suumon monster 9
            };

            List<BlueprintAbility> abilities = new List<BlueprintAbility>();

            foreach (var id in spell_guids)
            {
                var spell = library.Get<BlueprintAbility>(id);
                var lvl = spell.GetComponents<SpellListComponent>().FirstOrDefault(f => f.SpellList == wizard_spelllist).SpellLevel;

                var spells = spell.HasVariants ? spell.Variants : new BlueprintAbility[] {spell };
                foreach (var s in spells)
                {
                    var sp = Common.convertToSpellLike(s, prefix, classes, stat, resource, archetypes: getArchetypeArray());
                    Common.addSpellDescriptor(sp, (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Shadow60);
                    sp.RemoveComponents<SpellComponent>();
                    sp.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Illusion));
                    sp.AddComponent(Helpers.Create<NewMechanics.AbilityShowIfHasClassLevels>(a => { a.character_classes = classes; a.level = lvl * 2 + 1; }));
                    sp.SetName("Shadow Beasts: " + sp.Name);
                    Common.unsetAsFullRoundAction(sp);
                    abilities.Add(sp);
                }
            }

            ability.AddComponent(Helpers.CreateAbilityVariants(ability, abilities));
            addFocusInvestmentCheck(ability, SpellSchool.Illusion);
            var feature = Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 9);
            return feature;
        }


        public BlueprintFeature createUnseen()
        {
            var greater_invisibility = library.Get<BlueprintAbility>("ecaa0def35b38f949bd1976a6c9539e0");

            var ability = Common.convertToSpellLikeVariants(greater_invisibility, prefix, classes, stat, resource, archetypes: getArchetypeArray(), self_only: true);
            ability.AddComponent(createClassScalingConfig(min: 10, max: 10));

            ability.SetNameDescription("Unseen",
                                       "As a standard action, you can expend 1 point of mental focus to become invisible, as greater invisibility. This effect lasts for 1 minute.\n"
                                       + "You can expend 2 points of mental focus instead of 1 to use this power on a willing adjacent creature instead of yourself.\n"
                                       + "You must be at least 7th level to select this focus power."
                                       );

            var ability2 = library.CopyAndAdd(ability, ability.name + "Others", "");
            ability2.Range = AbilityRange.Touch;
            ability2.setMiscAbilityParametersTouchFriendly();
            ability2.ReplaceComponent<AbilityResourceLogic>(a => a.Amount = 2);
            ability2.SetName("Unseen (Others)");
            var wrapper = Common.createVariantWrapper(prefix + "UnseenBase", "", ability, ability2);
            wrapper.AddComponent(Helpers.CreateSpellComponent(SpellSchool.Illusion));
            addFocusInvestmentCheck(wrapper, SpellSchool.Illusion);
            var feature = Common.AbilityToFeature(wrapper, false);
            addMinLevelPrerequisite(feature, 7);
            return feature;
        }


        public BlueprintFeature createBedevelingAura()
        {
            var buff = Helpers.CreateBuff(prefix + "BedevelingAuraEffectBuff",
                                          "Bedeveling Aura",
                                          "By expending one point of mental focus, you can emit a 30-foot aura that bedevils your enemies with phantasmal assailants. Enemies within this aura move at half speed, are unable to take attacks of opportunity, and are considered to be flanked. This is a mind-affecting effect. The aura lasts for a number of round equal to 1/2 your occultist level.\n"
                                          + "You must be at least 9th level to select this focus power.",
                                          "",
                                          Helpers.GetIcon("b48674cef2bff5e478a007cf57d8345b"), //remove curse
                                          null,
                                          Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.Slowed),
                                          Common.createAddCondition(Kingmaker.UnitLogic.UnitCondition.DisableAttacksOfOpportunity),
                                          Helpers.Create<FlankingMechanics.AlwaysFlanked>(),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting)
                                          );

            var area_buff = Common.createBuffAreaEffect(buff, 30.Feet(), Helpers.CreateConditionsCheckerOr(Helpers.Create<ContextConditionIsEnemy>()));
            area_buff.GetComponent<AddAreaEffect>().AreaEffect.Fx = Common.createPrefabLink("dfadb7fa26de0384d9d9a6dabb0bea72");
            area_buff.SetBuffFlags(0);

            var ability = Helpers.CreateAbility(prefix + "BedevelingAuraAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Personal,
                                                "1 round/2 levels",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(area_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
                                                createClassScalingConfig(ContextRankProgression.Div2),
                                                resource.CreateResourceLogic()
                                                );
            ability.setMiscAbilityParametersSelfOnly();

            addFocusInvestmentCheck(ability, SpellSchool.Illusion);
            var feature = Common.AbilityToFeature(ability, false);
            addMinLevelPrerequisite(feature, 9);
            return feature;
        }


        public BlueprintBuff createDistortion()
        {
            var property = ImplementMechanics.InvestedImplementFocusAmountProperty.createProperty(prefix + "DistortionProperty", "",
                                                                                                  createClassScalingConfig(ContextRankProgression.AsIs, type: AbilityRankType.StatBonus),
                                                                                                  false,
                                                                                                  SpellSchool.Illusion);
            var buff = Helpers.CreateBuff(prefix + "DistortionBuff",
                                          "Distortion",
                                          "The implement allows its bearer to distort his form and location, protecting him from harm. Any attack has a chance to miss the bearer equal to 1% per point of mental focus invested into the implement (up to 1% per occultist level).\n"
                                          + "This works independently of concealement effects. Creatures with see invisibility, true seeing, or similar abilities ignore the miss chance from this ability.",
                                          "",
                                          Helpers.GetIcon("903092f6488f9ce45a80943923576ab3"), //displacement
                                          null,
                                          Helpers.Create<NewMechanics.AutoMissChance>(c =>
                                             {
                                              c.value = Helpers.CreateContextValue(AbilityRankType.Default);
                                              c.illusion_effect = true;
                                                 c.attack_types = new AttackType[] { AttackType.Melee, AttackType.Ranged, AttackType.RangedTouch, AttackType.Touch };
                                             }
                                             ),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, ContextRankProgression.AsIs,
                                                                          customProperty: property)
                                          );
            return buff;
        }
    }
}
