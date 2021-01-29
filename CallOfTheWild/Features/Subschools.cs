using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
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
    class Subschools
    {
        static LibraryScriptableObject library => Main.library;
        static public BlueprintProgression admixture;
        static public BlueprintProgression teleportation;
        static public BlueprintProgression undead;
        static public BlueprintProgression phantasm;
        //static public BlueprintProgression controller;
        static public BlueprintProgression prophecy;
        //static public BlueprintProgression banishment;
        static public BlueprintProgression enhancement;
        static public BlueprintProgression arcane_crafter;

        static public BlueprintProgression evocation;
        static public BlueprintProgression transmutation;
        static public BlueprintProgression divination;
        static public BlueprintProgression conjuration;
        //static public BlueprintProgression enchantment;
        static public BlueprintProgression necromancy;
        //static public BlueprintProgression abjuration;
        static public BlueprintProgression illusion;

        static public BlueprintProgression universalist;

        static public BlueprintAbility augment;
        static public BlueprintAbility inspiring_prediciton_ability;
        static public BlueprintAbility terror_ability_cast;
        static public List<BlueprintActivatableAbility> versatile_evocation = new List<BlueprintActivatableAbility>();
        static public BlueprintAbility bolster;
        static public BlueprintAbility command_undead;

        static public BlueprintFeatureSelection school_selection = library.Get<BlueprintFeatureSelection>("5f838049069f1ac4d804ce0862ab5110");

        static BlueprintCharacterClass[] getWizardArray()
        {
            return new BlueprintCharacterClass[] { library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e") };
        }

        static public BlueprintFeatureSelection opposition_school_selection = library.Get<BlueprintFeatureSelection>("6c29030e9fea36949877c43a6f94ff31");


        static internal void load()
        {
            createAdmixture();
            createTeleportation();
            createEnhancement();
            createProphecy();
            createPhantasm();
            createArcaneCrafter();
            createUndead();
        }


        static void createUndead()
        {
            necromancy = library.Get<BlueprintProgression>("e9450978cc9feeb468fb8ee3a90607e3");
            var resource2 = library.Get<BlueprintAbilityResource>("16394ff73175b5745957750040a2c99e"); //turn undead resource

            var necromancer_command_undead = ChannelEnergyEngine.createCommandUndead("Necromancer",
                                                                     "Command Undead",
                                                                     "As a standard action, you can enslave one undead creature within close range. Undead creature receives a Will save to negate the effect. The DC for this Will save is equal to 10 + 1/2 your wizard level + your Charisma modifier. Undead that fail its saves fall under your control, obeying your commands to the best of their ability, as if under the effects of control undead. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.",
                                                                     StatType.Charisma, getWizardArray(), resource2);
            necromancer_command_undead.RemoveComponents<AddAbilityResources>();
            command_undead = necromancer_command_undead.GetComponent<AddFeatureIfHasFact>().Feature as BlueprintAbility;
            var base_feature = library.CopyAndAdd<BlueprintFeature>("927707dce06627d4f880c90b5575125f", "UndeadSchoolBaseFeature", "");
            var resource = Helpers.CreateAbilityResource("UndeadSchoolBaseAbilityResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Intelligence);

            var buff = Helpers.CreateBuff("UndeadBolsterBuff",
                                          "Bolster",
                                          "As a standard action, you can touch an undead creature and infuse it with negative energy. It gains a +1 profane bonus on all attack rolls and saving throws, as well as 1 temporary hit point per Hit Die and a +2 bonus to its turn resistance. The bonus on attack rolls and saving throws increases by +1 for every 5 wizard levels you possess. These bonuses last for a number of rounds equal to 1/2 your wizard level (minimum 1 round). The bonuses and temporary hit points are immediately dispelled if the creature is within the area of a consecrate spell. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.",
                                          "",
                                          NewSpells.desecrate.Icon,
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.Profane),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.Profane),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.Profane),
                                          Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.Profane),
                                          Helpers.Create<TemporaryHitPointsPerLevel>(t =>
                                          {
                                              t.Descriptor = ModifierDescriptor.Profane;
                                              t.HitPointsPerLevel = 1;
                                              t.RemoveWhenHitPointsEnd = true;
                                              t.Value = 1;
                                          }),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getWizardArray(), progression: ContextRankProgression.OnePlusDivStep, stepLevel: 5),
                                          Helpers.Create<SavingThrowBonusAgainstSpecificSpells>(c =>
                                          {
                                                c.Spells = new BlueprintAbility[0];
                                                c.Value = 2;
                                                c.BypassFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("3d8e38c9ed54931469281ab0cec506e9") }; //sun domain
                                          }
                                          )
                                          );
            ChannelEnergyEngine.addChannelResitance(buff);

            NewSpells.consecrate_area.AddComponent(Helpers.CreateAreaEffectRunAction(unitEnter: Common.createContextActionRemoveBuff(buff),
                                                                                     round: Common.createContextActionRemoveBuff(buff)));

            var ability = Helpers.CreateAbility("UndeadSchoolBaseAbility",
                                                buff.Name,
                                                buff.Description,
                                                "",
                                                buff.Icon,
                                                AbilityType.SpellLike,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "1 round/2 levels",
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplySpellBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)))),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getWizardArray(), progression: ContextRankProgression.Div2, min: 1),
                                                Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                resource.CreateResourceLogic(),
                                                Common.createAbilityTargetHasFact(false, Common.undead)
                                                );
            ability.setMiscAbilityParametersTouchFriendly();
            base_feature.SetIcon(buff.Icon);
            base_feature.ReplaceComponent<AddFacts>(a => a.Facts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] { ability, necromancer_command_undead });
            base_feature.ReplaceComponent<AddAbilityResources>(a => a.Resource = resource);
            base_feature.ReplaceComponent<ReplaceAbilitiesStat>(r => r.Ability = new BlueprintAbility[] { ability });
            base_feature.ReplaceComponent<ReplaceCasterLevelOfAbility>(r => r.Spell = ability);
            base_feature.SetNameDescription("Focused School — Undead",
                                      "The dread and feared necromancer commands undead and uses the foul power of unlife against his enemies.\n"
                                      + necromancer_command_undead.Name + ": " + necromancer_command_undead.Description +"\n"
                                      + ability.Name + ": " + ability.Description);

            undead = library.CopyAndAdd(necromancy, "UndeadSchoolProgression", "");
            undead.SetNameDescription(base_feature.Name,
                                      base_feature.Description + "\n"
                                      + "Life Sight: At 8th level, you gain blindsight to a range of 10 feet for a number of rounds per day equal to your wizard level. This ability only allows you to detect living creatures and undead creatures. This sight also tells you whether a creature is living or undead. Constructs and other creatures that are neither living nor undead cannot be seen with this ability. The range of this ability increases by 10 feet at 12th level, and by an additional 10 feet for every four levels beyond 12th. These rounds do not need to be consecutive.");
            undead.LevelEntries = undead.LevelEntries.ToList().ToArray();
            undead.LevelEntries[0] = Helpers.LevelEntry(1, undead.LevelEntries[0].Features.Skip(1).ToArray().AddToArray(base_feature));
            addToSchoolSelection(undead, necromancy);
            bolster = ability;
        }


        static void createArcaneCrafter()
        {
            universalist = library.Get<BlueprintProgression>("0933849149cfc9244ac05d6a5b57fd80");

            var metamagics = library.GetAllBlueprints().OfType<BlueprintFeature>().Where(b => b.Groups.Contains(FeatureGroup.WizardFeat) && (b.GetComponent<AddMetamagicFeat>() != null)).ToArray();

            var metacharge = Helpers.CreateFeatureSelection("MetachargeFeature",
                                                            "Metacharge",
                                                            "As an Arcane crafter, you gain a bonus metamagic feat at 3rd level. You must still meet all prerequisites for this bonus feat, including caster level minimums.",
                                                            "",
                                                            null,
                                                            FeatureGroup.None);
            metacharge.AllFeatures = metamagics;

            arcane_crafter = library.CopyAndAdd(universalist, "ArcaneCrafterSchoolProgression", "");
            arcane_crafter.SetNameDescription("Arcane Crafter",
                            "Wizards who do not specialize (known as as universalists) have the most diversity of all arcane spellcasters.\n"
                            + metacharge.Name + ": " + metacharge.Description + "\n"
                            + "Metamagic Mastery: At 8th level, you can apply any one metamagic feat that you know to a spell you are about to cast. This does not alter the level of the spell or the casting time. You can use this ability once per day at 8th level and one additional time per day for every two wizard levels you possess beyond 8th. Any time you use this ability to apply a metamagic feat that increases the spell level by more than 1, you must use an additional daily usage for each level above 1 that the feat adds to the spell. Even though this ability does not modify the spell’s actual level, you cannot use this ability to cast a spell whose modified spell level would be above the level of the highest-level spell that you are capable of casting.");
            arcane_crafter.LevelEntries = arcane_crafter.LevelEntries.ToList().ToArray();
            arcane_crafter.LevelEntries[0] = Helpers.LevelEntry(3, metacharge);
            addToSchoolSelection(arcane_crafter, universalist);
        }


        static void createPhantasm()
        {
        
            illusion = library.Get<BlueprintProgression>("24d5402c0c1de48468b563f6174c6256");

            var resource = Helpers.CreateAbilityResource("PhantasmSchoolBaseAbilityResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Intelligence);

            var terror_effect = Helpers.CreateConditional(Helpers.Create<ContextConditionHitDice>(c => { c.HitDice = 0; c.AddSharedValue = true; c.SharedValue = AbilitySharedValue.StatBonus; }),
                                       Helpers.Create<AooMechanics.ContextActionProvokeAttackOfOpportunityFromAnyoneExceptCaster>(a => a.max_units = 100));
            var terror_ability = Helpers.CreateAbility("PhantasmSchoolTerrorAbility",
                                                "Terror",
                                                "As a standard action, you can make a melee touch attack that causes a creature to be assailed by nightmares only it can see. The creature provokes an attack of opportunity from you or an ally of your choice. Creatures with more Hit Dice than your wizard level are unaffected. This is a mind-affecting fear effect. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.",
                                                "",
                                                Helpers.GetIcon("d2aeac47450c76347aebbc02e4f463e0"), //fear
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "",
                                                "",
                                                Helpers.CreateRunActions(terror_effect),
                                                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting),
                                                Helpers.CreateDeliverTouch(),
                                                Common.createAbilitySpawnFx("49a8069c238b1a8429f2123654d4f45b", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getWizardArray()),
                                                Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default)), AbilitySharedValue.StatBonus)
                                                );
            terror_ability.setMiscAbilityParametersTouchHarmful();
            terror_ability_cast = Helpers.CreateTouchSpellCast(terror_ability, resource);

            var base_feature = Common.AbilityToFeature(terror_ability_cast, false);
            base_feature.AddComponent(resource.CreateAddAbilityResource());

            var resource2 = Helpers.CreateAbilityResource("PhantasmSchoolGreaterAbilityResource", "", "", "", null);
            resource2.SetIncreasedByLevel(0, 1, getWizardArray());
            var buff = Helpers.CreateBuff("PhantamsBedevelingAuraEffectBuff",
                                          "Bedeveling Aura",
                                          "At 8th level, you can emit a 30-foot aura that bedevils your enemies with phantasmal assailants. Enemies within this aura move at half speed, are unable to take attacks of opportunity, and are considered to be flanked. This is a mind-affecting effect. You can use this ability for a number of rounds per day equal to your wizard level. These rounds do not need to be consecutive.",
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

            var toggle = Common.buffToToggle(buff, CommandType.Standard, false, resource2.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.NewRound));
            toggle.Buff = area_buff;

            var greater_feature = Common.ActivatableAbilityToFeature(toggle, false);
            greater_feature.AddComponent(resource2.CreateAddAbilityResource());

            phantasm = library.CopyAndAdd(illusion, "SpecialisationSchoolPhantamsProgression", "");
            phantasm.SetNameDescription("Focused School — Phantasm",
                                        "Illusionists use magic to weave confounding images, figments, and phantoms to baffle and vex their foes.\n"
                                        + terror_ability.Name + ": " + terror_ability.Description +"\n"
                                        + toggle.Name + ": " + toggle.Description);
            phantasm.LevelEntries = new LevelEntry[]
            {
                Helpers.LevelEntry(1, base_feature,  opposition_school_selection, opposition_school_selection),
                Helpers.LevelEntry(8, greater_feature),
            };
            addToSchoolSelection(phantasm, illusion);
        }


        static void createProphecy()
        {
            var description = "A number of times per day equal to 3 + your Intelligence modifier, you can predict an ally’s success, bolstering others’ resolve. As a swift action, you can shout an inspiring prediction, granting each ally within 50 feet who can hear you a +4 luck bonus on her next attack roll, saving throw, or skill check.";
            var description2 = "A number of times per day equal to your Intelligence modifier, you can publicly declare that your next spell is guided by prophecy. When you do, the next spell you cast has a 20% chance of fizzling (1–20 on a d%). If the spell does not fail, treat the spell as if it had been modified by the Empower Spell feat, even if you do not have that feat. At 12th level, the chance that the spell fizzles is reduced to 15% (1–15 on a d%). At 16th level, the chance is reduced to 10% (1–10 on a d%).";
            
            divination = library.Get<BlueprintProgression>("d7d18ce5c24bd324d96173fdc3309646");
            var base_feature = library.Get<BlueprintFeature>("54d21b3221ea82a4d90d5a91b7872f3d");

            var resource = Helpers.CreateAbilityResource("ProphecySchoolBaseAbilityResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Intelligence);

            var lesser_buff = Helpers.CreateBuff("ProphecySchoolBaseBuff",
                                                 "Inspiring Prediction",
                                                 description,
                                                 "",
                                                 Helpers.GetIcon("ec931b882e806ce42906597e5585c13f"), //guidance
                                                 null,
                                                 Helpers.Create<BuffAllSavesBonus>(b => { b.Descriptor = ModifierDescriptor.Luck; b.Value = 4; }),
                                                 Helpers.Create<BuffAllSkillsBonus>(b => { b.Descriptor = ModifierDescriptor.Luck; b.Value = 4; }),
                                                 Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 4, ModifierDescriptor.Luck));
            var remove_self = Helpers.CreateActionList(Common.createContextActionRemoveBuff(lesser_buff));
            lesser_buff.AddComponents(Common.createAddInitiatorAttackRollTrigger2(remove_self, only_hit: false, on_initiator: true),
                                      Helpers.Create<AddInitiatorSkillRollTrigger>(a => a.Action = remove_self),
                                      Helpers.Create<AddInitiatorPartySkillRollTrigger>(a => a.Action = remove_self),
                                      Helpers.Create<AddInitiatorSavingThrowTrigger>(a => a.Action = remove_self)
                                      );

            var lesser_ability = Helpers.CreateAbility("ProphecySchoolBaseAbility",
                                                lesser_buff.Name,
                                                lesser_buff.Description,
                                                "",
                                                lesser_buff.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Swift,
                                                AbilityRange.Personal,
                                                Helpers.oneMinuteDuration,
                                                "",
                                                Helpers.CreateRunActions(Common.createContextActionApplyBuff(lesser_buff, Helpers.CreateContextDuration(10), dispellable: false)),
                                                Helpers.CreateAbilityTargetsAround(30.Feet(), Kingmaker.UnitLogic.Abilities.Components.TargetType.Ally, spreadSpeed: 17.Feet()),
                                                Common.createAbilitySpawnFx("c388856d0e8855f429a83ccba67944ba",
                                                                                anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                resource.CreateResourceLogic()
                                                );
            lesser_ability.setMiscAbilityParametersSelfOnly();
            inspiring_prediciton_ability = lesser_ability;
            var inspiring_predicition_feature = Helpers.CreateFeature("ProphecySchoolSchoolBaseFeature",
                                                      "Focused School — Prophecy",
                                                      "Diviners are masters of remote viewing, prophecies, and using magic to explore the world.\n"
                                                      + $"Inspiring Prediction: {description}\n"
                                                      + "Diviner's Fortune: When you activate this school power, you can touch any creature as a standard action to give it an insight bonus on all of its attack rolls, skill checks, ability checks, and saving throws equal to 1/2 your wizard level (minimum +1) for 1 round. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.\n"
                                                      + $"In Accordance with the Prophecy: {description2}",
                                                      "",
                                                      lesser_ability.Icon,
                                                      FeatureGroup.None,
                                                      base_feature.ComponentsArray.Take(4).ToArray()
                                                      );
            inspiring_predicition_feature.AddComponents(resource.CreateAddAbilityResource(), Helpers.CreateAddFacts(lesser_ability));


            var resource2 = Helpers.CreateAbilityResource("ProphecySchoolGreaterAbilityResource", "", "", "", null);
            resource2.SetIncreasedByStat(0, StatType.Intelligence);

            var greater_buff = Helpers.CreateBuff("ProphecySchoolGreaterBuff",
                                     "In Accordance with the Prophecy",
                                     description2,
                                     "",
                                     Helpers.GetIcon("ef16771cb05d1344989519e87f25b3c5"), //divine power
                                     null,
                                     Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSpellDescriptor>(a => { a.Metamagic = Metamagic.Empower; a.resource = resource2; a.amount = 1; }),
                                     Helpers.Create<SpellFailureMechanics.SpellFailureChance>(s => s.chance = Helpers.CreateContextValue(AbilityRankType.Default)),
                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getWizardArray(), progression: ContextRankProgression.Custom,
                                                                     customProgression: new (int, int)[] { (11, 20), (15, 15), (20, 10) })
                                     );

            var greater_ability = Helpers.CreateActivatableAbility("ProphecySchoolGreaterAbility",
                                                        greater_buff.Name,
                                                        greater_buff.Description,
                                                        "",
                                                        greater_buff.Icon,
                                                        greater_buff,
                                                        AbilityActivationType.Immediately,
                                                        CommandType.Free,
                                                        null,
                                                        resource2.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                        );
            greater_ability.DeactivateImmediately = true;
            var greater_feature = Common.ActivatableAbilityToFeature(greater_ability, false);
            greater_feature.AddComponent(resource2.CreateAddAbilityResource());

            prophecy = library.CopyAndAdd(divination, "SpecialisationSchoolProphecyProgression", "");
            prophecy.SetNameDescription(inspiring_predicition_feature);
            prophecy.LevelEntries = new LevelEntry[]
            {
                Helpers.LevelEntry(1, inspiring_predicition_feature,  opposition_school_selection, opposition_school_selection),
                Helpers.LevelEntry(8, greater_feature),
            };
            addToSchoolSelection(prophecy, divination);
        }

        static void createEnhancement()
        {
            transmutation = library.Get<BlueprintProgression>("b6a604dab356ac34788abf4ad79449ec");
            //also fix hide in ui flags to make level ui more consistent with other schools
            var base_feature = library.Get<BlueprintFeature>("c459c8200e666ef4c990873d3e501b91");
            base_feature.HideInUI = false;
            var change_shape_feature = library.Get<BlueprintFeature>("aeb56418768235640a3ee858d5ee05e8");
            change_shape_feature.HideInUI = false;
            change_shape_feature.SetNameDescription("Change Shape",
                                                    "At 8th level, you can change your shape for a number of rounds per day equal to your wizard level. These rounds do not need to be consecutive. This ability otherwise functions like beast shape II or elemental body I. At 12th level, this ability functions like beast shape III or elemental body II.");
            var stats = new StatType[]
            {
                StatType.Strength,
                StatType.Dexterity,
                StatType.Constitution,
                StatType.Intelligence,
                StatType.Wisdom,
                StatType.Charisma,
                StatType.AC,
            };

            var resource = Helpers.CreateAbilityResource("EnhancmeentSchoolBaseAbilityResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Intelligence);
            var resource2 = Helpers.CreateAbilityResource("EnhancmeentSchoolGreaterAbilityResource", "", "", "", null);
            resource2.SetIncreasedByLevel(0, 1, getWizardArray());

            var description = "As a standard action, you can touch a creature and grant it either a +2 enhancement bonus to a single ability score of your choice or a +1 bonus to natural armor that stacks with any natural armor the creature might possess. At 10th level, the enhancement bonus to one ability score increases to +4. The natural armor bonus increases by +1 for every five wizard levels you possess, to a maximum of +5 at 20th level. This augmentation lasts a number of rounds equal to 1/2 your wizard level (minimum 1 round). You can use this ability a number of times per day equal to 3 + your Intelligence modifier.";
            var description2 = "At 8th level, as a swift action you can grant yourself an enhancement bonus to a single ability score equal to 1/2 your wizard level (maximum +10) for one round. You may use this ability for a number of times per day equal to your wizard level.";
            var augment_feature = Helpers.CreateFeature("EnhancementSchoolSchoolBaseFeature",
                                                      "Focused School — Enhancement",
                                                      "Transmuters use magic to change the world around them.\n"
                                                      +"Physical Enhancement: You gain a +1 enhancement bonus to one physical ability score (Strength, Dexterity, or Constitution). This bonus increases by +1 for every five wizard levels you possess to a maximum of +5 at 20th level. At 20th level, this bonus applies to two physical ability scores of your choice.\n"
                                                      + $"Augment: {description}\n"
                                                      + $"Perfection of Self: {description2}",
                                                      "",
                                                      Helpers.GetIcon("a970537ea2da20e42ae709c0bb8f793f"), //touch of law
                                                      FeatureGroup.None,
                                                      resource.CreateAddAbilityResource(),
                                                      base_feature.ComponentsArray[0],
                                                      Helpers.CreateAddFact(library.Get<BlueprintFeature>("93919f8ce64dc5a4cbf058a486a44a1b")) //physical enchantment
                                                      );

            var base_abilities = new List<BlueprintAbility>();
            foreach (var s in stats)
            {
                BlueprintBuff buff = null;
                if (s == StatType.AC)
                {
                    buff = Helpers.CreateBuff(s.ToString() + "EnhancementAugmentBuff",
                                              "Augment: Natural Armor",
                                              description,
                                              "",
                                              augment_feature.Icon,
                                              Common.createPrefabLink("d6a7e564d0ee8b640b40c221d116e7ed"), //tocuh of good
                                              Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.NaturalArmor),
                                              Helpers.CreateContextRankConfig(Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.ClassLevel,
                                                                             classes: getWizardArray(), progression: ContextRankProgression.OnePlusDivStep,
                                                                             stepLevel: 5)
                                             );
                }
                else
                {
                    buff = Helpers.CreateBuff(s.ToString() + "EnhancementAugmentBuff",
                                              "Augment: " + s.ToString(),
                                              description,
                                              "",
                                              augment_feature.Icon,
                                              Common.createPrefabLink("d6a7e564d0ee8b640b40c221d116e7ed"), //tocuh of good
                                              Helpers.CreateAddContextStatBonus(s, ModifierDescriptor.Enhancement, multiplier: 2),
                                              Helpers.CreateContextRankConfig(Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.ClassLevel,
                                                                             classes: getWizardArray(), progression: ContextRankProgression.OnePlusDivStep,
                                                                             stepLevel: 10, max: 2)
                                             );
                }

                var ability = Helpers.CreateAbility(s.ToString() + "EnhancementAugmentAbility",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Supernatural,
                                                   Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                   AbilityRange.Touch,
                                                   "1 round/ 2 levels",
                                                   "",
                                                   Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), dispellable: false)),
                                                   Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getWizardArray(), progression: ContextRankProgression.Div2, min: 1),
                                                   resource.CreateResourceLogic()
                                                   );
                ability.setMiscAbilityParametersTouchFriendly();
                base_abilities.Add(ability);
            }

            var base_ability = Common.createVariantWrapper("EnhancementAugmentAbilityBase", "", base_abilities.ToArray());
            base_ability.SetNameDescription("Augment", description);
            augment = base_ability;

            augment_feature.AddComponent(Helpers.CreateAddFact(base_ability));

            var perfection_feature = Helpers.CreateFeature("EnhancementSchoolSchoolGreaterFeature",
                                                      "Perfection of Self",
                                                      description2,
                                                      "",
                                                      Helpers.GetIcon("c3a8f31778c3980498d8f00c980be5f5"), //guidance
                                                      FeatureGroup.None,
                                                      resource2.CreateAddAbilityResource()
                                                      );

            var greater_abilities = new List<BlueprintAbility>();
            foreach (var s in stats)
            {
                if (s == StatType.AC)
                {
                    continue;
                }

                var buff = Helpers.CreateBuff(s.ToString() + "EnhancementOerfectionOfSelfBuff",
                                              "Perfection of Self: " + s.ToString(),
                                              description2,
                                              "",
                                              perfection_feature.Icon,
                                              null,
                                              Helpers.CreateAddContextStatBonus(s, ModifierDescriptor.Enhancement),
                                              Helpers.CreateContextRankConfig(Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.ClassLevel,
                                                                             classes: getWizardArray(), progression: ContextRankProgression.Div2)
                                             );
                

                var ability = Helpers.CreateAbility(s.ToString() + "EnhancementOerfectionOfSelfAbility",
                                                   buff.Name,
                                                   buff.Description,
                                                   "",
                                                   buff.Icon,
                                                   AbilityType.Supernatural,
                                                   CommandType.Swift,
                                                   AbilityRange.Personal,
                                                   Helpers.oneRoundDuration,
                                                   "",
                                                   Helpers.CreateRunActions(Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                   resource2.CreateResourceLogic(),
                                                   Common.createAbilitySpawnFx("c388856d0e8855f429a83ccba67944ba", anchor: AbilitySpawnFxAnchor.SelectedTarget)
                                                   );
                ability.setMiscAbilityParametersSelfOnly();
                greater_abilities.Add(ability);
            }


            var greater_ability = Common.createVariantWrapper("EnhancementPerfectionOfSelfAbilityBase", "", greater_abilities.ToArray());
            greater_ability.SetNameDescription(perfection_feature.Name, perfection_feature.Description);

            perfection_feature.AddComponent(Helpers.CreateAddFact(greater_ability));

            enhancement = library.CopyAndAdd(transmutation, "SpecialisationSchoolEnhancementProgression", "");
            enhancement.SetNameDescription(augment_feature);
            enhancement.LevelEntries = new LevelEntry[]
            {
                Helpers.LevelEntry(1, augment_feature, library.Get<BlueprintFeature>("93919f8ce64dc5a4cbf058a486a44a1b"), opposition_school_selection, opposition_school_selection),
                Helpers.LevelEntry(8, perfection_feature),
                enhancement.LevelEntries[2]
            };
            addToSchoolSelection(enhancement, transmutation);
        }


        static void createTeleportation()
        {
            conjuration = library.Get<BlueprintProgression>("567801abe990faf4080df566fadcd038");

            var base_feature = library.Get<BlueprintFeature>("cee0f7edbd874a042952ee150f878b84");

            var dimension_door = library.Get<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2");

            var resource = Helpers.CreateAbilityResource("TeleportationSchoolBaseAbilityResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Intelligence);

            var description = "At 1st level, you can teleport to a nearby space as a swift action as if using dimension door. This movement does not provoke an attack of opportunity. You must be able to see the space that you are moving into. You cannot take other creatures with you when you use this ability (except for familiars). You can move 5 feet for every two wizard levels you possess (minimum 5 feet). You can use this ability a number of times per day equal to 3 + your Intelligence modifier.";
            var shift_feature = Helpers.CreateFeature("TeleprotationSchoolBaseFeature",
                                                      "Focused School — Teleportation",
                                                      "The conjurer focuses on the study of summoning monsters and magic alike to bend to his will.\n"
                                                      + "Summoner's Charm: Whenever you cast a conjuration (summoning) spell, increase the duration by a number of rounds equal to 1/2 your wizard level (minimum 1). This increase is not doubled by Extend Spell.\n"
                                                      + $"Shift: {description}\n"
                                                      + "Dimensional Steps: At 8th level, you can use this ability to teleport up to 30 feet as a move action. You can use this ability a total number of times per day equal to your wizard level.",
                                                      "",
                                                      dimension_door.Icon,
                                                      FeatureGroup.None,
                                                      resource.CreateAddAbilityResource(),
                                                      base_feature.ComponentsArray[0],
                                                      base_feature.ComponentsArray[1]
                                                      );

            for (int i = 1; i <= 10; i++)
            {
                var ability = library.CopyAndAdd(dimension_door, $"TeleprotationSchoolBase{i * 5}Ability", "");
                ability.Parent = null;
                ability.ActionType = CommandType.Swift;
                ability.Range = AbilityRange.Custom;
                ability.CustomRange = (i * 5).Feet();
                ability.Type = AbilityType.Supernatural;

                ability.SetNameDescription("Shift", description);
                ability.AddComponent(resource.CreateResourceLogic());
                var feature = Common.AbilityToFeature(ability);
                int min_level = i == 1 ? 0 : 2 * i;
                int max_level = i == 10 ? 100 : 2 * i + 1;

                
                shift_feature.AddComponent(Helpers.Create<LevelUpMechanics.AddFeatureOnClassLevelRange>(a => { a.min_level = min_level; a.max_level = max_level; a.classes = getWizardArray(); a.Feature = feature; }));
            }


            teleportation = library.CopyAndAdd(conjuration, "SpecialisationSchoolTeleportationProgression", "");
            teleportation.SetNameDescription(shift_feature);
            teleportation.LevelEntries = new LevelEntry[]
            {
                Helpers.LevelEntry(1, shift_feature, opposition_school_selection, opposition_school_selection),
                conjuration.LevelEntries[1]
            };
            addToSchoolSelection(teleportation, conjuration);
        }



        static void createAdmixture()
        {
            evocation = library.Get<BlueprintProgression>("f8019b7724d72a241a97157bc37f1c3b");

            var level1_feature = evocation.LevelEntries[0].Features[0];
            var resource = Helpers.CreateAbilityResource("AdmixtureSchoolBaseAbilityResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Intelligence);

            var metamagics = new Metamagic[] {(Metamagic)MetamagicFeats.MetamagicExtender.ElementalAcid,
                                               (Metamagic)MetamagicFeats.MetamagicExtender.ElementalElectricity,
                                               (Metamagic)MetamagicFeats.MetamagicExtender.ElementalFire,
                                               (Metamagic)MetamagicFeats.MetamagicExtender.ElementalCold };

            var energy = new DamageEnergyType[] { DamageEnergyType.Acid, DamageEnergyType.Electricity, DamageEnergyType.Fire, DamageEnergyType.Cold };
            var names = new string[] { "Acid", "Electricity", "Fire", "Cold" };
            var acid_icon = library.Get<BlueprintFeature>("52135eada006e9045a848cd659749608").Icon;
            var fire_icon = library.Get<BlueprintFeature>("13bdf8d542811ac4ca228a53aa108145").Icon;
            var elec_icon = library.Get<BlueprintFeature>("d439691f37d17804890bd9c263ae1e80").Icon;
            var cold_icon = library.Get<BlueprintFeature>("2ed9d8bf76412ba4a8afe38fa9925fca").Icon;

            var icons = new UnityEngine.Sprite[] { acid_icon, elec_icon, fire_icon, cold_icon };
            var description1 = "When you cast an evocation spell that does acid, cold, electricity, or fire damage, you may change the damage dealt to one of the other four energy types. This changes the descriptor of the spell to match the new energy type. Any non-damaging effects remain unchanged. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.";
            var description2 = "At 8th level, you can emit a 30-foot aura that transforms magical energy. Choose an energy type to transform it into. Any magical source of energy with a caster level equal to or less than your wizard level is altered to the chosen energy type. This includes supernatural effects from creatures with Hit Dice no greater than your caster level. For example, you could transform a white dragon’s frigid breath weapon (a supernatural ability), but not a fire elemental’s fiery touch (an extraordinary ability). If an effect lies only partially within your aura, only the portions within the aura are transformed. You can use this ability for a number of rounds per day equal to your wizard level. The rounds do not need to be consecutive.";
            var versatile_evocation_feature = Helpers.CreateFeature("VersatileEvocationFeature",
                                                                    "Focused School — Admixture",
                                                                    "Evokers revel in the raw power of magic, and can use it to create and destroy with shocking ease.\n"
                                                                    +"Intense Spells: Whenever you cast an evocation spell that deals hit point damage, add 1/2 your wizard level to the damage (minimum +1). This bonus only applies once to a spell, not once per missile or ray, and cannot be split between multiple missiles or rays. This damage is of the same type as the spell. At 20th level, whenever you cast an evocation spell, you can roll twice to penetrate a creature's spell resistance and take the better result.\n"
                                                                    + $"Versatile Evocation: {description1}\n"
                                                                    + $"Elemental Manipulation: {description2}",
                                                                    "",
                                                                    fire_icon,
                                                                    FeatureGroup.None,
                                                                    resource.CreateAddAbilityResource(),
                                                                    level1_feature.ComponentsArray[0].CreateCopy(), //school
                                                                    level1_feature.ComponentsArray[2].CreateCopy()
                                                                    );


            
            for (int i = 0; i < metamagics.Length; i++)
            {
                var buff = Helpers.CreateBuff(names[i] + "VersatileEvocationBuff",
                                               "Versatile Metamagic: " + names[i],
                                               description1,
                                               "",
                                               icons[i],
                                               null,
                                               Helpers.Create<NewMechanics.MetamagicMechanics.MetamagicOnSchool>(m => { m.amount = 1; m.resource = resource; m.Metamagic = metamagics[i]; m.school = SpellSchool.Evocation; })
                                               );

                var toggle = Helpers.CreateActivatableAbility(names[i] + "VersatileEvocationToggleAbility",
                                                               buff.Name,
                                                               description1,
                                                               "",
                                                               icons[i],
                                                               buff,
                                                               Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                               null,
                                                               resource.CreateActivatableResourceLogic(Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                               );
                toggle.DeactivateImmediately = true;
                toggle.Group = ActivatableAbilityGroupExtension.VersatileEvocation.ToActivatableAbilityGroup();
                versatile_evocation_feature.AddComponent(Helpers.CreateAddFact(toggle));
                versatile_evocation.Add(toggle);
            }



            var icons2 = new UnityEngine.Sprite[] { Helpers.GetIcon("435222be97067a447b2b40d3c58a058e"),
                                                  Helpers.GetIcon("96ca3143601d6b242802655336620d91"),
                                                   Helpers.GetIcon("cdb106d53c65bbc4086183d54c3b97c7"),
                                                   Helpers.GetIcon("7ef096fdc8394e149a9e8dced7576fee")
                                                  };

            var resource2 = Helpers.CreateAbilityResource("AdmixtureSchoolGreaterAbilityResource", "", "", "", null);
            resource2.SetIncreasedByLevel(0, 1, getWizardArray());

            var elemental_manipulation_feature = Helpers.CreateFeature("ElementalManipulationFeature",
                                                                "Elemental Manipulation",
                                                                description2,
                                                                "",
                                                                icons2[2],
                                                                FeatureGroup.None,
                                                                resource2.CreateAddAbilityResource());

            for (int i = 0; i < metamagics.Length; i++)
            {
                var buff = Helpers.CreateBuff(names[i] + "ElemenatlManipulationEffectBuff",
                                               "Elemental Manipulation: " + names[i],
                                               description2,
                                               "",
                                               icons[i],
                                               null,
                                               Helpers.Create<SpellManipulationMechanics.ChangeElementalDamage>(c =>{ c.Element = energy[i]; c.max_level = Helpers.CreateContextValue(AbilityRankType.Default); }),
                                               Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getWizardArray())
                                               );

                var buff_area_effect =  Common.createBuffAreaEffect(buff, 30.Feet(), Helpers.CreateConditionsCheckerOr());
                var toggle = Helpers.CreateActivatableAbility(names[i] + "ElementalManipulationToggleAbility",
                                                               buff.Name,
                                                               description2,
                                                               "",
                                                               icons[i],
                                                               buff_area_effect,
                                                               Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                                                               null,
                                                               resource2.CreateActivatableResourceLogic(Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic.ResourceSpendType.NewRound)
                                                               );

                toggle.Group = ActivatableAbilityGroupExtension.ElementalManipulation.ToActivatableAbilityGroup();
                elemental_manipulation_feature.AddComponent(Helpers.CreateAddFact(toggle));
            }


            admixture = library.CopyAndAdd(evocation, "SpecialisationSchoolAdmixtureProgression", "");
            admixture.LevelEntries = new LevelEntry[]
            {
                Helpers.LevelEntry(1, versatile_evocation_feature, opposition_school_selection, opposition_school_selection),
                Helpers.LevelEntry(8, elemental_manipulation_feature),
                evocation.LevelEntries[2],
            };
            admixture.SetNameDescription(versatile_evocation_feature.Name, versatile_evocation_feature.Description);

            addToSchoolSelection(admixture, evocation);
            
        }     
        

        static void addToSchoolSelection(BlueprintFeature subschool, BlueprintFeature school)
        {
            school_selection.AllFeatures = school_selection.AllFeatures.AddToArray(subschool);
            school.AddComponent(Helpers.Create<NewMechanics.FeatureReplacement>(f => f.replacement_feature = subschool)); //dodge
        }
    }
}
