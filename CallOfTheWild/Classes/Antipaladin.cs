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
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.ElementsSystem;

namespace CallOfTheWild
{
    public class Antipaladin
    {
        static internal LibraryScriptableObject library => Main.library;
        static internal bool test_mode = false;
        static public BlueprintCharacterClass antipaladin_class;
        static public BlueprintProgression antipaladin_progression;

        static public BlueprintFeatureSelection antipaladin_deity;
        static public BlueprintFeature antipaladin_proficiencies;
        static public BlueprintFeature touch_of_corruption;
        static public BlueprintFeature smite_good;
        static public BlueprintFeature smite_good_extra_use;
        static public BlueprintFeature unholy_resilence;
        static public BlueprintFeature aura_of_cowardice;
        static public BlueprintFeature plague_bringer;
        static public BlueprintFeature channel_negative_energy;
        static public BlueprintFeature[] fiendish_boon;
        static public BlueprintFeature aura_of_despair;
        static public BlueprintFeature aura_of_vengeance;
        static public BlueprintFeature aura_of_sin;
        static public BlueprintFeature aura_of_deparvity;
        static public BlueprintFeature tip_of_spear;

        static public BlueprintAbilityResource smite_resource;
        static public BlueprintAbilityResource touch_of_corruption_resource;


        static public BlueprintFeatureSelection cruelty;

        internal static void creatAntipaldinClass()
        {
            Main.logger.Log("Antipaladin class test mode: " + test_mode.ToString());
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var paladin_class = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");

            antipaladin_class = Helpers.Create<BlueprintCharacterClass>();
            antipaladin_class.name = "AntipaladinClass";
            library.AddAsset(antipaladin_class, "");

            antipaladin_class.LocalizedName = Helpers.CreateString("Antipaladin.Name", "Antipaladin");
            antipaladin_class.LocalizedDescription = Helpers.CreateString("Antipaladin.Description",
                                                                         "Although it is a rare occurrence, paladins do sometimes stray from the path of righteousness. Most of these wayward holy warriors seek out redemption and forgiveness for their misdeeds, regaining their powers through piety, charity, and powerful magic. Yet there are others, the dark and disturbed few, who turn actively to evil, courting the dark powers they once railed against in order to take vengeance on their former brothers. It’s said that those who climb the farthest have the farthest to fall, and antipaladins are living proof of this fact, their pride and hatred blinding them to the glory of their forsaken patrons.\n"
                                                                         + "Antipaladins become the antithesis of their former selves. They make pacts with fiends, take the lives of the innocent, and put nothing ahead of their personal power and wealth. Champions of evil, they often lead armies of evil creatures and work with other villains to bring ruin to the holy and tyranny to the weak. Not surprisingly, paladins stop at nothing to put an end to such nefarious antiheroes.\n"
                                                                         + "Role: Antipaladins are villains at their most dangerous. They care nothing for the lives of others and actively seek to bring death and destruction to ordered society. They rarely travel with those that they do not subjugate, unless as part of a ruse to bring ruin from within."
                                                                         );
            antipaladin_class.m_Icon = fighter_class.Icon;
            antipaladin_class.SkillPoints = paladin_class.SkillPoints;
            antipaladin_class.HitDie = DiceType.D10;
            antipaladin_class.BaseAttackBonus = paladin_class.BaseAttackBonus;
            antipaladin_class.FortitudeSave = paladin_class.FortitudeSave;
            antipaladin_class.ReflexSave = paladin_class.ReflexSave;
            antipaladin_class.WillSave = paladin_class.WillSave;
            antipaladin_class.Spellbook = createAntipaladinSpellbook();
            antipaladin_class.ClassSkills = paladin_class.ClassSkills;
            antipaladin_class.IsDivineCaster = true;
            antipaladin_class.IsArcaneCaster = false;
            antipaladin_class.StartingGold = paladin_class.StartingGold;
            antipaladin_class.PrimaryColor = paladin_class.PrimaryColor;
            antipaladin_class.SecondaryColor = paladin_class.SecondaryColor;
            antipaladin_class.RecommendedAttributes = paladin_class.RecommendedAttributes;
            antipaladin_class.NotRecommendedAttributes = paladin_class.NotRecommendedAttributes;
            antipaladin_class.EquipmentEntities = fighter_class.EquipmentEntities;
            antipaladin_class.MaleEquipmentEntities = fighter_class.MaleEquipmentEntities;
            antipaladin_class.FemaleEquipmentEntities = fighter_class.FemaleEquipmentEntities;
            antipaladin_class.ComponentsArray = paladin_class.ComponentsArray.ToArray();
            antipaladin_class.ReplaceComponent<PrerequisiteAlignment>(p => p.Alignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Evil);
            antipaladin_class.StartingItems = paladin_class.StartingItems;
            createAntipaladinProgression();
            antipaladin_class.Progression = antipaladin_progression;

            antipaladin_class.Archetypes = new BlueprintArchetype[] { }; //blighted myrmidon, insinuator, dread vanguard, knight of the sepulcher, iron tyrant
            Helpers.RegisterClass(antipaladin_class);
            //fix antipaldin feats
        }


        public static BlueprintCharacterClass[] getAntipaladinArray()
        {
            return new BlueprintCharacterClass[] { antipaladin_class };
        }

        static void createAntipaladinProgression()
        {

            antipaladin_proficiencies = library.CopyAndAdd<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb", "", "AntipaladinProficiencies");
            antipaladin_proficiencies.SetNameDescription("Antipaladin Proficiencies",
                                                         "Antipaladins are proficient with all simple and martial weapons, with all types of armor (heavy, medium, and light), and with shields (except tower shields).");
            unholy_resilence = library.CopyAndAdd<BlueprintFeature>("8a5b5e272e5c34e41aa8b4facbb746d3", "UnholyResilence", ""); //from divine grace
            unholy_resilence.SetNameDescription("Unholy Resilience",
                                                "At 2nd level, an antipaladin gains a bonus equal to his Charisma bonus (if any) on all saving throws.");

            plague_bringer = library.CopyAndAdd<BlueprintFeature>("41d1d0de15e672349bf4262a5acf06ce", "UnholyResilence", ""); //from divine health
            plague_bringer.SetNameDescription("Plague Bringer",
                                              "At 3rd level, the powers of darkness make an antipaladin a beacon of corruption and disease. An antipaladin does not take any damage or take any penalty from diseases. He can still contract diseases and spread them to others, but he is otherwise immune to their effects.");
            plague_bringer.ComponentsArray = new BlueprintComponent[] { Helpers.Create<SuppressBuffs>(s => s.Descriptor = SpellDescriptor.Disease) };


            createAntipaladinDeitySelection();
            createSmiteGood();
            createChannelEnergy();

            antipaladin_progression = Helpers.CreateProgression("AntpladinProgression",
                                                              antipaladin_class.Name,
                                                              antipaladin_class.Description,
                                                              "",
                                                              antipaladin_class.Icon,
                                                              FeatureGroup.None);

            antipaladin_progression.Classes = getAntipaladinArray();



            antipaladin_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, antipaladin_proficiencies, antipaladin_deity, smite_good,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")), // touch calculate feature                                                                                      
                                                                    Helpers.LevelEntry(2, touch_of_corruption, unholy_resilence),
                                                                    Helpers.LevelEntry(3, aura_of_cowardice, plague_bringer, cruelty),
                                                                    Helpers.LevelEntry(4, smite_good_extra_use, channel_negative_energy),
                                                                    Helpers.LevelEntry(5, fiendish_boon[0]),
                                                                    Helpers.LevelEntry(6, cruelty),
                                                                    Helpers.LevelEntry(7, smite_good_extra_use),
                                                                    Helpers.LevelEntry(8, aura_of_despair, fiendish_boon[1]),
                                                                    Helpers.LevelEntry(9, cruelty),
                                                                    Helpers.LevelEntry(10, smite_good_extra_use),
                                                                    Helpers.LevelEntry(11, aura_of_vengeance, fiendish_boon[2]),
                                                                    Helpers.LevelEntry(12, cruelty),
                                                                    Helpers.LevelEntry(13, smite_good_extra_use),
                                                                    Helpers.LevelEntry(14, aura_of_sin, fiendish_boon[3]),
                                                                    Helpers.LevelEntry(15, cruelty),
                                                                    Helpers.LevelEntry(16, smite_good_extra_use),
                                                                    Helpers.LevelEntry(17, aura_of_deparvity, fiendish_boon[4]),
                                                                    Helpers.LevelEntry(18, cruelty),
                                                                    Helpers.LevelEntry(19, smite_good_extra_use),
                                                                    Helpers.LevelEntry(20, tip_of_spear, fiendish_boon[5])
                                                                    };

            antipaladin_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { antipaladin_proficiencies, antipaladin_deity};
            antipaladin_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(fiendish_boon.AddToArray(unholy_resilence, plague_bringer)),
                                                                Helpers.CreateUIGroup(smite_good, smite_good_extra_use),
                                                                Helpers.CreateUIGroup(aura_of_cowardice, aura_of_despair, aura_of_vengeance, aura_of_sin, aura_of_deparvity, tip_of_spear),
                                                                Helpers.CreateUIGroup(touch_of_corruption, channel_negative_energy, cruelty),
                                                           };
        }


        static void createTouchOfCorruption()
        {
            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            //6
            var dazed = Common.dazed_non_mind_affecting;
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            //iseased
            //9
            //cursed
            var exahusted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");
            var frightened = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");
            var nauseted = library.Get<BlueprintBuff>("956331dba5125ef48afe41875a00ca0e");
            var poisoned = library.Get<BlueprintBuff>("ba1ae42c58e228c4da28328ea6b4ae34");

            //12
            var blinded = library.Get<BlueprintBuff>("0ec36e7596a4928489d2049e1e1c76a7");
            var deafened = Common.deafened;




            var paladin = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            touch_of_corruption_resource = library.Get<BlueprintAbilityResource>("9dedf41d995ff4446a181f143c3db98c");
            ClassToProgression.addClassToResource(antipaladin_class, new BlueprintArchetype[0], touch_of_corruption_resource, paladin);

            var dice = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageDice), 0);
            var heal_action = Common.createContextActionHealTarget(dice);
            var damage_undead_action = Helpers.CreateActionDealDamage(DamageEnergyType.PositiveEnergy, dice);
            var damage_living_action = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, dice);
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                          type: AbilityRankType.DamageDice, classes: getAntipaladinArray());

            var deaths_embrace_living = library.Get<BlueprintFeature>("fd7c08ccd3c7773458eb9613db3e93ad");


            var fervor_negative_ability_others = Helpers.CreateAbility("WarpriestFervorNegativeTouchOthersAbility",
                                                            "Fervor (Negative Energy) Others",
                                                            "At 2nd level, a warpriest can draw upon the power of his faith to heal wounds or harm foes. This ability can be used a number of times per day equal to 1/2 his warpriest level + his Wisdom modifier. By expending one use of this ability, an evil warpriest (or one who worships an evil deity) can touch a living creature and deal to it 1d6 points of damage, plus an additional 1d6 points of damage for every 3 warpriest levels he possesses above 2nd (to a maximum of 7d6 at 20th level). Using this ability is a standard action (unless the warpriest targets himself, in which case it’s a swift action). Alternatively, the warpriest can use this ability to heal an undead creature for same amount of damage he would otherwise deal with a melee touch attack. Using fervor in this way is a standard action that provokes an attack of opportunity. Living creatures do not receive a saving throw against this damage. This counts as negative energy.",
                                                            "",
                                                            inflict_light_wounds.Icon,
                                                            AbilityType.Supernatural,
                                                            CommandType.Standard,
                                                            AbilityRange.Touch,
                                                            "",
                                                            Helpers.savingThrowNone,
                                                            Helpers.CreateRunActions(Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(),
                                                                                                                                                 Common.createContextConditionHasFact(deaths_embrace_living)
                                                                                                                                                ),
                                                                                                               heal_action,
                                                                                                               damage_living_action)),
                                                            inflict_light_wounds.GetComponent<AbilityDeliverTouch>(),
                                                            inflict_light_wounds.GetComponent<AbilitySpawnFx>(),
                                                            context_rank_config,
                                                            context_rank_config_fcb,
                                                            Helpers.Create<AbilityUseOnRest>(c => c.Type = AbilityUseOnRestType.HealUndead)
                                                            );
            fervor_negative_ability_others.CanTargetFriends = true;
            fervor_negative_ability_others.CanTargetEnemies = true;
            fervor_negative_ability_others.CanTargetSelf = false;
            fervor_negative_ability_others.CanTargetPoint = false;
            fervor_negative_ability_others.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            fervor_negative_ability_others.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            fervor_negative_ability_others.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            fervor_negative_ability_others.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;

            var fervor_negative_ability_others_sticky = Helpers.CreateTouchSpellCast(fervor_negative_ability_others, "WarpriestFervorNegativeOthersAbility", "");
            fervor_negative_ability_others_sticky.AddComponents(Common.createAbilityTargetHasFact(true, construct_type),
                                                                Helpers.CreateResourceLogic(warpriest_fervor_resource));

            var fervor_negative_ability_self = library.CopyAndAdd<BlueprintAbility>(fervor_negative_ability_others, "WarpriestFervorNegativeSelfAbility", "");
            fervor_negative_ability_self.SetName("Fervor (Negative Energy) Self");
            fervor_negative_ability_self.Range = AbilityRange.Personal;
            fervor_negative_ability_self.CanTargetSelf = true;
            fervor_negative_ability_self.CanTargetFriends = false;
            fervor_negative_ability_self.CanTargetEnemies = false;
            fervor_negative_ability_self.ActionType = CommandType.Swift;
            fervor_negative_ability_self.AddComponent(Helpers.CreateResourceLogic(warpriest_fervor_resource));

            fervor_negative = Helpers.CreateFeature("WarpriestFervorNegative",
                                                    "Fervor (Negative Energy)",
                                                    fervor_negative_ability_others.Description + "\n" + fervor_swift_cast_ability.Description,
                                                    "",
                                                    fervor_negative_ability_others.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddFacts(fervor_negative_ability_self, fervor_negative_ability_others_sticky, fervor_swift_cast_ability)
                                                    );
        }


        static void createChannelEnergy()
        {
            var negative_energy_feature = library.Get<BlueprintFeature>("3adb2c906e031ee41a01bfc1d5fb7eea");
            var context_rank_config = Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray(), progression: ContextRankProgression.OnePlusDiv2);
            var dc_scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(getAntipaladinArray(), StatType.Charisma);
            negative_energy_feature = Helpers.CreateFeature("AntipaladinChannelNegativeEnergyFeature",
                                                            "Channel Negative Energy",
                                                            "When an antipaladin reaches 4th level, he gains the supernatural ability to channel negative energy like a cleric. Using this ability consumes two uses of his touch of corruption ability. An antipaladin uses his level as his effective cleric level when channeling negative energy. This is a Charisma-based ability.",
                                                            "",
                                                            negative_energy_feature.Icon,
                                                            FeatureGroup.None);

            var harm_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHarm,
                                                                      "AntipaladinChannelEnergyHarmLiving",
                                                                      "",
                                                                      "Channeling negative energy causes a burst that damages all living creatures in a 30 - foot radius centered on the antipaladin. The amount of damage inflicted is equal to 1d6 points of damage plus 1d6 points of damage for every two antipaladin levels beyond 1st (3d6 at 5th, and so on). Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1 / 2 the antipaladin's level + the antipaladin's Charisma modifier.",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(touch_of_corruption_resource, amount: 2));
            var heal_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHeal,
                                                                        "AntipaladinChannelEnergyHarmUndead",
                                                                        "",
                                                                        "Channeling negative energy causes a burst that heals all undead creatures in a 30-foot radius centered on the antipaladin. The amount of damage healed is equal to 1d6 points of damage plus 1d6 points of damage for every two antipaladin levels beyond 1st (3d6 at 5th, and so on).",
                                                                        "",
                                                                        context_rank_config,
                                                                        dc_scaling,
                                                                        Helpers.CreateResourceLogic(touch_of_corruption_resource, amount: 2));

            var harm_living_base = Common.createVariantWrapper("AntipaladinNegativeHarmBase", "", harm_living);
            var heal_undead_base = Common.createVariantWrapper("AntipaladinNegativeHealBase", "", heal_undead);

            ChannelEnergyEngine.storeChannel(harm_living, negative_energy_feature, ChannelEnergyEngine.ChannelType.NegativeHarm);
            ChannelEnergyEngine.storeChannel(heal_undead, negative_energy_feature, ChannelEnergyEngine.ChannelType.NegativeHeal);

            negative_energy_feature.AddComponent(Helpers.CreateAddFacts(harm_living_base, heal_undead_base));
        }

        static void createSmiteGood()
        {
            var resource = library.Get<BlueprintAbilityResource>("b4274c5bb0bf2ad4190eb7c44859048b");
            tip_of_spear = Helpers.CreateFeature("TipOfTheSpearFeature",
                                                 "Tip of the Spear",
                                                 "At 20th level, the antipaladin tears through heroes and rival villains alike. The antipaladin gains three additional uses of smite good per day and can smite foes regardless of their alignment.",
                                                 "",
                                                 LoadIcons.Image2Sprite.Create(@"AbilityIcons/TipOfTheSpear.png"),
                                                 FeatureGroup.None,
                                                 resource.CreateIncreaseResourceAmount(3)
                                                 );

            smite_good = Common.createSmite("AntipaladinSmiteGood",
                                            "Smite Good",
                                            "Once per day, an antipaladin can call out to the dark powers to crush the forces of good. As a swift action, the antipaladin chooses one target within sight to smite. If this target is good, the antipaladin adds his Charisma bonus (if any) on his attack rolls and adds his antipaladin level on all damage rolls made against the target of his smite, smite good attacks automatically bypass any DR the creature might possess.\n"
                                            + "In addition, while smite good is in effect, the antipaladin gains a deflection bonus equal to his Charisma modifier (if any) to his AC against attacks made by the target of the smite. If the antipaladin targets a creature that is not good, the smite is wasted with no effect.\n"
                                            + "The smite evil lasts until the target dies or the paladin selects a new target. At 4th level, and at every three levels thereafter, the paladin may smite evil one additional time per day.",
                                            "",
                                            "",
                                            library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce").Icon, //unholy
                                            getAntipaladinArray(),
                                            Helpers.Create<NewMechanics.ContextConditionAlignmentUnlessCasterHasFact>(c => { c.Alignment = AlignmentComponent.Good; c.fact = tip_of_spear; })
                                            );

            smite_good_extra_use = library.CopyAndAdd<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137", "SmiteGoodAdditionalUse", "");
            smite_good.SetNameDescriptionIcon("Smite Good — Additional Use",
                                              smite_good.Description,
                                              smite_good.Icon);

            smite_resource = resource;
        }


        static void createAntipaladinDeitySelection()
        {
            antipaladin_deity = library.CopyAndAdd<BlueprintFeatureSelection>("a7c8b73528d34c2479b4bd638503da1d", "AntipaladinDeitySelection", "");
            antipaladin_deity.AllFeatures = new BlueprintFeature[0];

            var deities = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4").AllFeatures;

            var allow_evil = library.Get<BlueprintFeature>("351235ac5fc2b7e47801f63d117b656c");

            antipaladin_deity.AllFeatures = deities.Where(d => d.GetComponents<AddFacts>().Aggregate(false, (val, af) => val = val || af.Facts.Contains(allow_evil))).ToArray();
            antipaladin_deity.Features = antipaladin_deity.AllFeatures;
        }


        static BlueprintSpellbook createAntipaladinSpellbook()
        {
            var paladin_spellook = library.Get<BlueprintSpellbook>("bce4989b070ce924b986bf346f59e885");
            var antipaladin_spellbook = library.CopyAndAdd(paladin_spellook, "AntipaladinSpellbook", "");
            antipaladin_spellbook.Name = antipaladin_class.LocalizedName;
            antipaladin_spellbook.CharacterClass = antipaladin_class;          
            antipaladin_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            antipaladin_spellbook.SpellList.name = "AntipaladinSpelllist";
            library.AddAsset(antipaladin_spellbook.SpellList, "");
            antipaladin_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < antipaladin_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                antipaladin_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "8bc64d869456b004b9db255cdd1ea734", 1), //bane
                new Common.SpellId( "b7731c2b4fa1c9844a092329177be4c3", 1), //boneshaker
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( NewSpells.command.AssetGuid, 1),
                new Common.SpellId( "fbdd8c455ac4cde4a9a3e18c84af9485", 1), //doom
                new Common.SpellId( "e5af3674bb241f14b9a9f6b0c7dc3d27", 1), //inflict light wounds
                new Common.SpellId( "433b1faf4d02cc34abb0ade5ceda47c4", 1), //protection from alignment
                new Common.SpellId( NewSpells.savage_maw.AssetGuid, 1), 
                //shadow claws
                //touch of blindness
                new Common.SpellId( "8fd74eddd9b6c224693d9ab241f25e84", 1), //summon monster I
                
                new Common.SpellId( NewSpells.blade_tutor.AssetGuid, 2), //acid arrow
                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 2), //blindness
                new Common.SpellId( "4c3d08935262b6544ae97599b3a9556d", 2), //bulls's strength
                new Common.SpellId( NewSpells.desecrate.AssetGuid, 2),
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagles splendor
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 2), //hold person
                new Common.SpellId( NewSpells.inflict_pain.AssetGuid, 2),
                new Common.SpellId( "89940cde01689fb46946b2f8cd7b66b7", 2), //invisibility
                new Common.SpellId( "c9198d9dfd2515d4ba98335b57bb66c7", 2), //litany of eloqeunce
                new Common.SpellId( "16f7754287811724abe1e0ead88f74ca", 2), //litany of entanglement
                new Common.SpellId( "dee3074b2fbfb064b80b973f9b56319e", 2), //pernicious poison
                new Common.SpellId( "82962a820ebc0e7408b8582fdc3f4c0c", 2), //sense vitals
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( "1724061e89c667045a6891179ee2e8e7", 2), //summon monster 2
                new Common.SpellId( NewSpells.touch_of_blood_letting.AssetGuid, 2),              
                new Common.SpellId( NewSpells.vine_strike.AssetGuid, 2),
                
                new Common.SpellId( NewSpells.accursed_glare.AssetGuid, 3), 
                new Common.SpellId( "4b76d32feb089ad4499c3a1ce8e1ac27", 3), //animate dead
                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 3), //bestow curse
                new Common.SpellId( "48e2744846ed04b4580be1a3343a5d3d", 3), //contagion
                new Common.SpellId( NewSpells.deadly_juggernaut.AssetGuid, 3),
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //diplacement instead of shield of darkness
                new Common.SpellId( "65f0b63c45ea82a4f8b8325768a3832d", 3), //inflict moderate wounds             
                //second wind
                new Common.SpellId( "5d61dde0020bbf54ba1521f7ca0229dc", 3), //summon monster 3
                new Common.SpellId( "8a28a811ca5d20d49a863e832c31cce1", 3), //vampiric touch

                //banishing blade        
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( NewSpells.inflict_pain_mass.AssetGuid, 4),
                new Common.SpellId( "bd5da98859cf2b3418f6d68ea66cabbe", 4), //inflict serious wounds
                new Common.SpellId( "ecaa0def35b38f949bd1976a6c9539e0", 4), //invisibility greater
                new Common.SpellId( "435e73bcff18f304293484f9511b4672", 4), //lithany of madness
                new Common.SpellId( "4fbd47525382517419c66fb548fe9a67", 4), //slay living
                new Common.SpellId( "b56521d58f996cd4299dab3f38d5fe31", 4), //profane nimbus
                new Common.SpellId( "9047cb1797639924487ec0ad566a3fea", 4), //resounding blow
                new Common.SpellId( "7ed74a3ec8c458d4fb50b192fd7be6ef", 4), //summon monster 4
                //unholy sword
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(antipaladin_spellbook.SpellList, spell_id.level);
            }

            return antipaladin_spellbook;
        }
    }
}
