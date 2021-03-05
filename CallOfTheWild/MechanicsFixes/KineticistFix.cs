using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Class.Kineticist.Actions;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class KineticistFix
    {
        static LibraryScriptableObject library => Main.library;
        static Dictionary<string, (BlueprintActivatableAbility, BlueprintAbility)> blast_kinetic_blades_burn_map = new Dictionary<string, (BlueprintActivatableAbility, BlueprintAbility)>();
        static List<BlueprintActivatableAbility> substance_infusions = new List<BlueprintActivatableAbility>();

        public static BlueprintFeature blade_rush;
        public static BlueprintFeature blade_rush_swift;
        public static BlueprintFeature kinetic_whip;
        public static BlueprintBuff kinetic_whip_buff;

        public static BlueprintFeature suffocate;
        public static BlueprintFeature air_leap;
        public static BlueprintFeature wings_of_air;//+1/2 level to mobility
        public static BlueprintFeature cryokinetic_stasis;//?
        public static BlueprintFeature flame_jet; //dimension door
        public static BlueprintFeature flame_jet_greater; //move action dimension door and immunity to ground based effects
        public static BlueprintFeature purifying_flames;//
        public static BlueprintFeature windsight;//
        public static BlueprintFeature ice_path;//
        public static BlueprintFeature spark_of_life;
        static public BlueprintFeature cold_snap;
        static public BlueprintFeature smoke_storm;

        //heat wave
        //searing flame
        //from the ashes

        static public BlueprintFeature kinetic_invocation;
        static public BlueprintFeature kinetic_invocation_cloak_of_winds;
        static public BlueprintFeature kinetic_invocation_wind_walk;
        static public BlueprintFeature kinetic_invocation_blessing_of_salamander;
        static public BlueprintFeature kinetic_invocation_invigorate;
        static public BlueprintFeature kinetic_invocation_fluid_form;
        static public BlueprintFeature kinetic_invocation_sleet_storm;
        static public BlueprintFeature kinetic_invocation_slowing_mud;
        static public BlueprintFeatureSelection expanded_element_bonus_talent_selection;

        public static BlueprintFeature whip_hurricane;

        public static BlueprintFeature internal_buffer;
        public static BlueprintAbilityResource internal_buffer_resource;

        static BlueprintCharacterClass kineticist_class = library.Get<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");
        static BlueprintFeature kinetic_blade_infusion = library.Get<BlueprintFeature>("9ff81732daddb174aa8138ad1297c787");
        static BlueprintFeature whirlwind_infusion = library.Get<BlueprintFeature>("80fdf049d396c33408a805d9e21a42e1");

        static BlueprintFeatureSelection infusion_selection = library.Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
        static BlueprintArchetype kinetic_knight = library.Get<BlueprintArchetype>("7d61d9b2250260a45b18c5634524a8fb");
        static BlueprintProgression kineticist_progression = library.Get<BlueprintProgression>("b79e92dd495edd64e90fb483c504b8df");


        static BlueprintAbility blade_whirlwind_ability = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");
        static BlueprintAbility kinetic_whip_ability;
        static BlueprintAbility whip_hurricane_ability;
        static BlueprintAbility blade_rush_ability;
        static BlueprintAbility blade_rush_swift_ability;
        static public BlueprintBuff blade_rush_buff;

        static public BlueprintFeature kinetic_fist;
        static public BlueprintFeature energize_weapon;



        internal static void load(bool update_archetypes)
        {
            var kinetic_blade_infusion = library.Get<BlueprintFeature>("9ff81732daddb174aa8138ad1297c787");
            foreach (var c in kinetic_blade_infusion.GetComponents<AddFeatureIfHasFact>())
            {
                var add_facts = c.Feature.GetComponents<AddFeatureIfHasFact>().ToArray();
                blast_kinetic_blades_burn_map.Add(c.CheckedFact.AssetGuid, (add_facts[0].Feature as BlueprintActivatableAbility, add_facts[1].Feature as BlueprintAbility));
            }

            foreach (var infusion in infusion_selection.AllFeatures)
            {
                var comp = infusion.GetComponent<AddFacts>();
                if (comp != null)
                {
                    var ability = comp.Facts[0] as BlueprintActivatableAbility;
                    if (ability == null)
                    {
                        continue;
                    }
                    if (ability.Group == ActivatableAbilityGroup.SubstanceInfusion)
                    {
                        substance_infusions.Add(ability);
                    }
                }
            }

            fixDescriptors();
            //addWhirlwindInfusionToKineticistSelection();
            restoreKineticKnightinfusions();
            whirlwind_infusion.HideInUI = false;
            fixKineticBladeCost();
            fixKineticBladeCostForKineticKnight();
            fixBladeWhirlwindCost();
            fixKineticHealer();
            fixBladeWhirlwindRange();

            fixShroudOfWaterForKineticKnight();

            createBladeRush();
            createKineticWhip();
            createWhipHurricane();
            createKineticFist();
            createSparkOfLife();
            createAirLeapAndWingsOfAir();
            createFlameJetAndFlameJetGreater();
            createWindsight();
            createPurifyingFlames();
            createSuffocate();
            createIcePath();
            createColdSnap();
            createSmokeStorm();
            createKineticInvocation();
            createInternalBuffer();
            fixKineticistAbilitiesToBeSpelllike();
            Witch.infusion.AllFeatures = infusion_selection.AllFeatures;

            fixRepeatingElements();
            if (update_archetypes)
            {
                Main.logger.Log("Updating base kineticist archetypes");
                updateKineticistArchetypes();
            }
        }

        static void createKineticFist()
        {
            //we will need to create cost and burn ability and insert them via patches into kinetic blade processing functions (?)
            //TryRunKineticBladeActivationAction - make a prefix that will run a burn cost ability
            //TryHandleKineticBladeAttack -  use damage ability (if allowed)
            //we will add buff that will do the following:
            //on first attack it will use burn cost buff and apply damage marker buff
            //on all attacks it will use blast effect (OnEventDidTrigger(RuleAttackWithWeapon)
        }


        static void fixRepeatingElements()
        {
            var first_element_selection = library.Get<BlueprintFeatureSelection>("1f3a15a3ae8a5524ab8b97f469bf4e3d");
            var first_element_selection_kk = library.Get<BlueprintFeatureSelection>("b1f296f0bd16bc242ae35d0638df82eb");
            var second_element_selection = library.Get<BlueprintFeatureSelection>("4204bc10b3d5db440b1f52f0c375848b");
            var third_element_selection = library.Get<BlueprintFeatureSelection>("e2c1718828fc843479f18ab4d75ded86");

            second_element_selection.SetDescription("At 7th level, a kineticist learns to use another element or expands her understanding of her own element. She can choose any element, including her primary element. She gains one of that element’s simple blast wild talents that she does not already possess, if any. She also gains all composite blast wild talents whose prerequisites she meets, as well as the basic wild talent of her chosen expanded element.\n"
                                                    + "If the kineticist chooses to expand her understanding of an element she already has, she gains an additional utility wild talent or infusion of her choice from that element, as if from her infusion or wild talent class feature, as appropriate.\n"
                                                    + "At 15th level, the kineticist can either select a new element or expand her understanding of her original element. She can’t select the same element she selected at 7th level unless it is her primary element. She gains all the benefits from her new expanded element as listed above. However, if the kineticist selected her primary element as her expanded element at both 7th and 15th levels, her mastery of that element increases. For wild talents of her element, the kineticist gains a +1 bonus on attack rolls and damage rolls, as well as to caster level and DCs.");
            third_element_selection.SetDescription(second_element_selection.Description);

            expanded_element_bonus_talent_selection = Helpers.CreateFeatureSelection("BonusExpandedElementFeatureSelection",
                                                                                     second_element_selection.Name,
                                                                                     second_element_selection.Description,
                                                                                     "",
                                                                                     second_element_selection.Icon,
                                                                                     FeatureGroup.None);

            expanded_element_bonus_talent_selection.AllFeatures = new BlueprintFeature[] { infusion_selection, library.Get<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459") };
            for (int i = 0; i < 4; i++)
            {
                second_element_selection.AllFeatures[i].AddComponent(Helpers.Create<NewMechanics.addSelectionIfHasFacts>(a =>
                {
                    a.selection = expanded_element_bonus_talent_selection;
                    a.facts = new BlueprintUnitFact[] { first_element_selection.AllFeatures[i], first_element_selection_kk.AllFeatures[i] };
                }));

                third_element_selection.AllFeatures[i].AddComponent(Helpers.Create<NewMechanics.addSelectionIfHasFacts>(a =>
                {
                    a.selection = expanded_element_bonus_talent_selection;
                    a.facts = new BlueprintUnitFact[] { first_element_selection.AllFeatures[i], first_element_selection_kk.AllFeatures[i] };
                }));

                third_element_selection.AllFeatures[i].AddComponent(Helpers.Create<PrerequisiteMechanics.PrerequsiteOrAlternative>(p =>
                {
                    p.base_prerequsite = third_element_selection.AllFeatures[i].GetComponent<PrerequisiteNoFeature>();
                    p.alternative_prerequsite = Helpers.PrerequisiteFeaturesFromList(first_element_selection.AllFeatures[i],
                                                                                     first_element_selection_kk.AllFeatures[i]);
                }));
                third_element_selection.AllFeatures[i].RemoveComponents<PrerequisiteNoFeature>();


                var third_element_bonus = Helpers.CreateFeature("Bonus" + third_element_selection.AllFeatures[i].name,
                                                                third_element_selection.AllFeatures[i].Name,
                                                                third_element_selection.AllFeatures[i].Description,
                                                                "",
                                                                third_element_selection.AllFeatures[i].Icon,
                                                                FeatureGroup.None,
                                                                Helpers.Create<NewMechanics.ThirdElementKineticistBonus>(t => t.value = 1)
                                                                );
                third_element_bonus.HideInCharacterSheetAndLevelUp = true;
                third_element_selection.AllFeatures[i].AddComponents(Common.createAddFeatureIfHasFact(second_element_selection.AllFeatures[i], third_element_bonus));
            }
        }


        static void createSmokeStorm()
        {
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            var area = library.CopyAndAdd<BlueprintAbilityAreaEffect>(NewSpells.obscuring_mist_area.AssetGuid, "SmokeStormAbilityArea", "");

            var apply_buff = Common.createContextActionApplyBuff(sickened, Helpers.CreateContextDuration(1, DurationRate.Rounds, DiceType.D4, 1));
            var apply_saved = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, apply_buff));
            area.AddComponent(Helpers.CreateAreaEffectRunAction(round: Helpers.CreateConditional(Common.createContextConditionHasFacts(false, Common.undead, Common.construct, Common.elemental, sickened),
                                                                                                 null,
                                                                                                 apply_saved)
                                                                )
                             );
            area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Fire | SpellDescriptor.Sickened));
            
            var spawn_area = Common.createContextActionSpawnAreaEffect(area, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)));
            var ability = Helpers.CreateAbility("SmokeStormKineticistAbility",
                                                     "Smoke Storm",
                                                     "Element: fire\nType: utility\nLevel: 3\nBurn: 0\n"
                                                     + "You create a cloud of choking smoke, filling a 20-foot-radius spread and affecting vision like an obscuring mist. All creatures that begin their turns inside the area become sickened for 1d4+1 rounds (Fort negates).",
                                                     "",
                                                     sickened.Icon,
                                                     AbilityType.SpellLike,
                                                     UnitCommand.CommandType.Standard,
                                                     AbilityRange.Medium,
                                                     Helpers.roundsPerLevelDuration,
                                                     Helpers.fortNegates,
                                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Fire | SpellDescriptor.Sickened),
                                                     Helpers.CreateRunActions(spawn_area),
                                                     Common.createAbilityAoERadius(20.Feet(), TargetType.Any),
                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] {kineticist_class }),
                                                     Helpers.Create<AbilityKineticist>(a => { a.Amount = 0; a.WildTalentBurnCost = 0; }),
                                                     Common.createContextCalculateAbilityParamsBasedOnClass(kineticist_class, StatType.Constitution, true)
                                                     );
            ability.setMiscAbilityParametersRangedDirectional();

            smoke_storm = Common.AbilityToFeature(ability, false);
            smoke_storm.AddComponents(library.Get<BlueprintFeature>("14c699ccd0564e04a9587b1845d16014").GetComponent<PrerequisiteFeaturesFromList>());
            smoke_storm.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 6));

            area.AddComponent(Helpers.Create<UniqueAreaEffect>(a => a.Feature = smoke_storm));
            addWildTalent(smoke_storm);
        }


        static void createColdSnap()
        {
            var buff = Helpers.CreateBuff("ColdSnapBuff",
                                          "Cold Snap Effect",
                                          "Element: water \nType: utility\nLevel: 3\nBurn: 1\n"
                                          + "You chill your shroud of water and send the cold around you, creating an aura of numbing cold around yourself. Until the next time your burn is removed ou can begin or end the cold aura at will as a swift action. When aura is active, all creatures within 5 feet of you take a –4 penalty to Dexterity. You are immune to these effects, as are creatures that are immune to cold or having at least cold resistance 5.",
                                          "",
                                          LoadIcons.Image2Sprite.Create(@"AbilityIcons/StormOfSouls.png"),
                                          null,
                                          Helpers.CreateAddStatBonus(StatType.Dexterity, -4, Kingmaker.Enums.ModifierDescriptor.UntypedStackable)
                                          );

            var aura = Common.createBuffAreaEffect(buff, 7.Feet(),
                                                  Helpers.CreateConditionsCheckerAnd(Helpers.Create<NewMechanics.HasEnergyImmunityOrDR>(h =>
                                                                                     {
                                                                                         h.energy = Kingmaker.Enums.Damage.DamageEnergyType.Cold;
                                                                                         h.min_dr = 5;
                                                                                         h.Not = true;
                                                                                     }),
                                                                                     Common.createContextConditionIsCaster(not: true))
                                                  );

            aura.SetName("Cold Snap");

            var enable_buff = Helpers.CreateBuff("ColdSnapEnableBuff",
                                          "Activate Cold Snap",
                                          buff.Description,
                                          "",
                                          buff.Icon,
                                          null
                                          );
            enable_buff.SetBuffFlags(BuffFlags.RemoveOnRest);

            var toggle = Common.buffToToggle(buff, UnitCommand.CommandType.Swift, true, Common.createActivatableAbilityRestrictionHasFact(enable_buff));
            toggle.DeactivateIfOwnerDisabled = true;
            toggle.Buff = aura;
            toggle.SetName("Cold Snap");

            var enable_ability = Helpers.CreateAbility("ColdSnapEnableAbility",
                                                       enable_buff.Name,
                                                       enable_buff.Description,
                                                       "",
                                                       enable_buff.Icon,
                                                       AbilityType.SpellLike,
                                                       UnitCommand.CommandType.Standard,
                                                       AbilityRange.Personal,
                                                       "Until rest",
                                                       "",
                                                       Helpers.CreateRunActions(Common.createContextActionApplyBuff(enable_buff, Helpers.CreateContextDuration(), dispellable: true, is_permanent: true)),
                                                       Helpers.Create<AbilityKineticist>(a => { a.Amount = 1; a.WildTalentBurnCost = 1; }),
                                                       Common.createAbilityCasterHasNoFacts(enable_buff)
                                                       );
            enable_ability.setMiscAbilityParametersSelfOnly();

            cold_snap = Common.ActivatableAbilityToFeature(toggle);
            cold_snap.AddComponent(Helpers.CreateAddFact(enable_ability));
            cold_snap.AddComponents(Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("1ff5d6e76b7c2fa48be555b77d1ad8b2")), //cold adaptation
                                    Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("29ec36fa2a5b8b94ebce170bd369083a"))
                                    );//shroud of water
            cold_snap.AddComponents(library.Get<BlueprintFeature>("1d42456e6113739499e1bda025e0ba03").GetComponent<PrerequisiteFeaturesFromList>()); //from slick
            cold_snap.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 6));
            cold_snap.SetName("Cold Snap");
            addWildTalent(cold_snap);
        }


        static void createSuffocate()
        {
            var effect = Helpers.CreateConditionalSaved(null,
                                                         Common.createContextActionApplyBuff(NewSpells.fast_suffocation_buff, Helpers.CreateContextDuration(2, DurationRate.Rounds), is_from_spell: true)
                                                       );
            var suffocate_ability = Helpers.CreateAbility("SuffocateKineticistAbility",
                                    "Suffocate",
                                    "Element: air, water\nType: utility\nLevel: 6\nBurn: 1\n"
                                    + "You can accept 1 point of burn to expel the air from your target’s lungs. If you do so and the target fails its first Fortitude save, it becomes disabled and is reduced to 0 hit points, and on its second failed Fortitude save, it falls unconscious and is reduced to –1 hit points.",
                                    "",
                                    Helpers.GetIcon("e6f2fc5d73d88064583cb828801212f4"),
                                    AbilityType.SpellLike,
                                    UnitCommand.CommandType.Standard,
                                    AbilityRange.Medium,
                                    "2 rounds",
                                    Helpers.fortNegates,
                                    Helpers.CreateRunActions(SavingThrowType.Fortitude, effect),
                                    Common.createAbilityTargetHasFact(true, Common.undead),
                                    Common.createAbilityTargetHasFact(true, Common.construct),
                                    Common.createAbilityTargetHasFact(true, Common.elemental),
                                    Helpers.CreateSpellDescriptor(SpellDescriptor.Death),
                                    Helpers.CreateSpellComponent(SpellSchool.Necromancy),
                                    Common.createAbilitySpawnFx("cbfe312cb8e63e240a859efaad8e467c", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                    Common.createContextCalculateAbilityParamsBasedOnClass(kineticist_class, StatType.Constitution, true),
                                    Helpers.Create<AbilityKineticist>(a => { a.WildTalentBurnCost = 1; a.Amount = 1; })
                                    );
            suffocate_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            suffocate_ability.SpellResistance = true;
            suffocate = Common.AbilityToFeature(suffocate_ability, false);
            suffocate.AddComponents(library.Get<BlueprintFeature>("b182f559f572da342b54bece4404e4e7").GetComponent<PrerequisiteFeaturesFromList>().CreateCopy(p => p.Group = Prerequisite.GroupType.Any),
                                    library.Get<BlueprintFeature>("1d42456e6113739499e1bda025e0ba03").GetComponent<PrerequisiteFeaturesFromList>().CreateCopy(p => p.Group = Prerequisite.GroupType.Any),
                                    Helpers.PrerequisiteClassLevel(kineticist_class, 12)
                                    );
            addWildTalent(suffocate);
        }


        static void createKineticInvocation()
        {
            kinetic_invocation = Helpers.CreateFeature("KineticInvocationFeature",
                                                       "Kinetic Invocation",
                                                       "For each kineticist element to which you have access, you treat all spells associated with that element (see below) as utility wild talents of the listed level, which you can select as normal. Each has a burn cost of 1 unless otherwise noted, and any nonpermanent, non-instantaneous effects end when your burn is removed.\n"
                                                       + "Your caster level for a spell is equal to your kineticist level, and the save DC for any spell is equal to 10 + 1/2 kineticist level + your Constitution modifier.\n"
                                                       + "Air: Cloak of Winds (3rd), wind walk (6th).\n"
                                                       + "Earth: Slowing Mud (4th).\n"
                                                       + "Fire: Blessing of the Salamander (5th, self only), Invigorate (1st).\n"
                                                       + "Water: Fluid Form (6th), Sleet Storm (3rd).",
                                                       "",
                                                       Helpers.GetIcon("16fa59cc9a72a6043b566b49184f53fe"),
                                                       FeatureGroup.Feat,
                                                       Helpers.PrerequisiteClassLevel(kineticist_class, 1)
                                                       );

            kinetic_invocation_cloak_of_winds = createKineticInvocationTalent(NewSpells.cloak_of_winds, 3, SpellDescriptor.Electricity);
            kinetic_invocation_wind_walk = createKineticInvocationTalent(NewSpells.wind_walk, 6, SpellDescriptor.Electricity);

            kinetic_invocation_slowing_mud = createKineticInvocationTalent(library.Get<BlueprintAbility>("6b30813c3709fc44b92dc8fd8191f345"), 4, SpellDescriptor.Acid);

            kinetic_invocation_invigorate = createKineticInvocationTalent(NewSpells.invigorate, 1, SpellDescriptor.Fire);
            kinetic_invocation_blessing_of_salamander = createKineticInvocationTalent(library.Get<BlueprintAbility>("9256a86aec14ad14e9497f6b60e26f3f"), 5, SpellDescriptor.Fire, self_only: true);

            kinetic_invocation_fluid_form = createKineticInvocationTalent(NewSpells.fluid_form, 6, SpellDescriptor.Cold);
            kinetic_invocation_sleet_storm = createKineticInvocationTalent(NewSpells.sleet_storm, 3, SpellDescriptor.Cold);
            library.AddFeats(kinetic_invocation);
        }


        static BlueprintFeature createKineticInvocationTalent(BlueprintAbility spell, int level, SpellDescriptor descriptor, int burn_cost = 1, bool self_only = false)
        {

            var ability = Common.convertToKineticistTalent(spell, "KineticInvocation", burn_cost: burn_cost);
            if (self_only)
            {
                ability.setMiscAbilityParametersSelfOnly();
                ability.Range = AbilityRange.Personal;
            }

            ability.SetName(kinetic_invocation.Name + ": " + spell.Name);

            switch (descriptor)
            {
                case SpellDescriptor.Electricity:
                    ability.SetDescription($"Element: air\nType: utility\nLevel: {level}\nBurn: {burn_cost}\n" + ability.Description);
                    break;
                case SpellDescriptor.Cold:
                    ability.SetDescription($"Element: water\nType: utility\nLevel: {level}\nBurn: {burn_cost}\n" + ability.Description);
                    break;
                case SpellDescriptor.Fire:
                    ability.SetDescription($"Element: fire\nType: utility\nLevel: {level}\nBurn: {burn_cost}\n" + ability.Description);
                    break;
                case SpellDescriptor.Acid:
                    ability.SetDescription($"Element: earth\nType: utility\nLevel: {level}\nBurn: {burn_cost}\n" + ability.Description);
                    break;
                default:
                    break;
            }
            var feature = Common.AbilityToFeature(ability, false, "");
            if (level > 1)
            {
                feature.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, level * 2));
            }
            feature.Groups = new FeatureGroup[] { FeatureGroup.KineticWildTalent };
            feature.AddComponent(Helpers.PrerequisiteFeature(kinetic_invocation));

            switch (descriptor)
            {
                case SpellDescriptor.Electricity:
                    feature.AddComponent(library.Get<BlueprintFeature>("b182f559f572da342b54bece4404e4e7").GetComponent< PrerequisiteFeaturesFromList>()); //celerity
                    break;
                case SpellDescriptor.Cold:
                    feature.AddComponent(library.Get<BlueprintFeature>("1d42456e6113739499e1bda025e0ba03").GetComponent<PrerequisiteFeaturesFromList>()); //slick
                    break;
                case SpellDescriptor.Fire:
                    feature.AddComponent(library.Get<BlueprintFeature>("14c699ccd0564e04a9587b1845d16014").GetComponent<PrerequisiteFeaturesFromList>()); //fox fire
                    break;
                case SpellDescriptor.Acid:
                    feature.AddComponent(Helpers.PrerequisiteFeaturesFromList(library.Get<BlueprintFeature>("c6816ad80a3df9c4ea7d3b012b06bacd"),
                                                                              library.Get<BlueprintFeature>("956b65effbf37e5419c13100ab4385a3"),
                                                                              library.Get<BlueprintFeature>("c43d9c2d23e56fb428a4eb60da9ba1cb"),
                                                                              library.Get<BlueprintFeature>("d2a93ab18fcff8c419b03a2c3d573606")
                                                                             )
                                        ); 
                    break;
                default:
                    break;
            }

            addWildTalent(feature);
            return feature;
        }


        static void createIcePath()
        {
            var difficult_terrain = library.CopyAndAdd<BlueprintBuff>("1914ccc0f3da5b1439f0b90d90d05811", "IcePathDifficultTerrainBuff", "");
            var slick_area = library.CopyAndAdd<BlueprintAbilityAreaEffect>("eca936a9e235875498d1e74ff7c09ecd", "IcePathArea", ""); //spike stones
            slick_area.Size = 5.Feet();

            slick_area.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            slick_area.Fx.AssetId = "b6a8750499b0ec647ba68430e83bfc2f";
            slick_area.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = difficult_terrain);
            var apply_prone = Helpers.Create<ContextActionKnockdownTarget>();
            var area_effect = Helpers.CreateAreaEffectRunAction(//unitEnter: Common.createContextActionApplyBuff(difficult_terrain, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false),
                                                                //unitExit: Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = difficult_terrain),
                                                                unitMove: Common.createContextActionSkillCheck(StatType.SkillMobility, failure: Helpers.CreateActionList(apply_prone), custom_dc: 10));
            slick_area.ReplaceComponent<AbilityAreaEffectRunAction>(area_effect);
            slick_area.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Ground | SpellDescriptor.Cold));

            var ice_path_buff = Helpers.CreateBuff("IcePathBuff",
                                               "Ice Path",
                                               "Element: water\nType: utility\nLevel: 6\nBurn: 0\n"
                                               + "You freeze water vapor in the air, allowing you to travel above the ground as air walk by walking along the ice, and leaving a path of ice behind you that lasts for 1 round before it melts.",
                                               "",
                                               LoadIcons.Image2Sprite.Create(@"AbilityIcons/IcePath.png"),
                                               null,
                                               Common.createSpellImmunityToSpellDescriptor(SpellDescriptor.Ground),
                                               Common.createBuffDescriptorImmunity(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Ground),
                                               Common.createAddConditionImmunity(Kingmaker.UnitLogic.UnitCondition.DifficultTerrain),
                                               Helpers.CreateAddFact(FixFlying.pit_spell_immunity),
                                               Helpers.Create<UnitMoveMechanics.ActionOnUnitMoved>(a => a.actions = Helpers.CreateActionList(Common.createContextActionSpawnAreaEffect(slick_area, Helpers.CreateContextDuration(1))))
                                               );

            var toggle = Common.buffToToggle(ice_path_buff, UnitCommand.CommandType.Free, true);

            ice_path = Common.ActivatableAbilityToFeature(toggle, false);
            ice_path.Groups = new FeatureGroup[] { FeatureGroup.KineticWildTalent };
            ice_path.AddComponents(library.Get<BlueprintFeature>("1d42456e6113739499e1bda025e0ba03").GetComponent<PrerequisiteFeaturesFromList>()); //from slick
            ice_path.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 12));

            addWildTalent(ice_path);
        }


        static void createWindsight()
        {
            windsight = Helpers.CreateFeature("WindsightAirFeature",
                                     "Windsight",
                                     "Element: air\nType: utility\nLevel: 3\nBurn: 0\n"
                                     + "You can see through mist and fog (including fog cloud and similar magic).",
                                     "",
                                     Helpers.GetIcon("4cf3d0fae3239ec478f51e86f49161cb"),
                                     FeatureGroup.KineticWildTalent,
                                     Helpers.Create<ConcealementMechanics.IgnoreFogConcelement>(),
                                     library.Get<BlueprintFeature>("b182f559f572da342b54bece4404e4e7").GetComponent<PrerequisiteFeaturesFromList>(), //from celerity
                                     Helpers.PrerequisiteClassLevel(kineticist_class, 6)
                                     );

            addWildTalent(windsight);
        }


        static void createAirLeapAndWingsOfAir()
        {
            air_leap = Helpers.CreateFeature("AirLeapFeature",
                                             "Air's Leap",
                                             "Element: air\nType: utility\nLevel: 1\nBurn: 0\n"
                                             + "You receive a bonus to your mobility skill equal to half your kineticist level.",
                                             "",
                                             Helpers.GetIcon("0087fc2d64b6095478bc7b8d7d512caf"),
                                             FeatureGroup.KineticWildTalent,
                                             Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.SkillMobility, Kingmaker.Enums.ModifierDescriptor.UntypedStackable),
                                             Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { kineticist_class },
                                                                            progression: ContextRankProgression.Div2),
                                             library.Get<BlueprintFeature>("b182f559f572da342b54bece4404e4e7").GetComponent<PrerequisiteFeaturesFromList>(), //from celerity
                                             Helpers.PrerequisiteClassLevel(kineticist_class, 1)
                                             );

            wings_of_air = Helpers.CreateFeature("WingsOfAirFeature",
                                                 "Wings of Air",
                                                 "Element: air\nType: utility\nLevel: 3\nBurn: 0\n"
                                                 +"The air bends to your will, allowing you to soar to great heights. You are constantly under the effects of fly.",
                                                 "",
                                                 NewSpells.fly.Icon,
                                                 FeatureGroup.KineticWildTalent,
                                                 Common.createAuraFeatureComponent(NewSpells.fly_buff),
                                                 library.Get<BlueprintFeature>("b182f559f572da342b54bece4404e4e7").GetComponent<PrerequisiteFeaturesFromList>(), //from celerity
                                                 Helpers.PrerequisiteClassLevel(kineticist_class, 6),
                                                 Helpers.PrerequisiteFeature(air_leap)
                                                 );

            addWildTalent(air_leap);
            addWildTalent(wings_of_air);
        }


        static void createPurifyingFlames()
        {
            var neutralzie_poison = library.Get<BlueprintAbility>("e7240516af4241b42b2cd819929ea9da");
            var purifying_flames_ability = Common.convertToKineticistTalent(neutralzie_poison, "PurifyingFlames", burn_cost: 1);
            purifying_flames_ability.SetNameDescription("Purifying Flames",
                                                     "Element: fire\nType: utility\nLevel: 4\nBurn: 1\n"
                                                     + "You use fire to burn poison from within a creature’s veins. You must attempt a caster level check (1d20 + your kineticist level) against the DC of each poison affecting the target. Success means that the poison is neutralized, as neutralize poison. ");

            purifying_flames = Common.AbilityToFeature(purifying_flames_ability, false);

            purifying_flames.AddComponents(library.Get<BlueprintFeature>("14c699ccd0564e04a9587b1845d16014").GetComponent<PrerequisiteFeaturesFromList>(), //from fox fire
                                    Helpers.PrerequisiteClassLevel(kineticist_class, 8)
                                    );
            purifying_flames.Groups = new FeatureGroup[] { FeatureGroup.KineticWildTalent };


            addWildTalent(purifying_flames);
        }


        static void createFlameJetAndFlameJetGreater()
        {
            var dimension_door = library.Get<BlueprintAbility>("a9b8be9b87865744382f7c64e599aeb2");
            var flame_jet_ability = Common.convertToKineticistTalent(dimension_door, "FlameJet");
            flame_jet_ability.Range = AbilityRange.Custom;
            flame_jet_ability.CustomRange = 30.Feet();
            flame_jet_ability.SetNameDescriptionIcon("Flame Jet",
                                                     "Element: fire\nType: utility\nLevel: 3\nBurn: 0\n"
                                                     + "You shoot a burst of flame behind you as a standard action, propelling you up to 30 feet in a straight line, including into the air.",
                                                     LoadIcons.Image2Sprite.Create(@"AbilityIcons/FlameJet.png"));

            flame_jet_ability.ReplaceComponent<AbilityCustomDimensionDoor>(a =>
            {
                a.CasterDisappearProjectile = library.Get<BlueprintProjectile>("ecf79fc871f15074e95698a3fef47aee");
                a.CasterAppearProjectile = library.Get<BlueprintProjectile>("ecf79fc871f15074e95698a3fef47aee");
            });

            flame_jet = Common.AbilityToFeature(flame_jet_ability, false);

            flame_jet.AddComponents(library.Get<BlueprintFeature>("14c699ccd0564e04a9587b1845d16014").GetComponent<PrerequisiteFeaturesFromList>(), //from fox fire
                                    Helpers.PrerequisiteClassLevel(kineticist_class, 6)
                                    );
            flame_jet.Groups = new FeatureGroup[] { FeatureGroup.KineticWildTalent };

            flame_jet_greater = Helpers.CreateFeature("FlameJEtGreaterFeature",
                                                     "Flame Jet, Greater",
                                                     "Element: fire\nType: utility\nLevel: 5\nBurn: 0\n"
                                                     + "You can use flame jet as a move action and can emanate a mild jet of flame, allowing you to hover without spending an action (essentially a fly effect).",
                                                     "",
                                                     flame_jet.Icon,
                                                     FeatureGroup.KineticWildTalent,
                                                     Helpers.Create<TurnActionMechanics.MoveActionAbilityUse>(m => m.abilities = new BlueprintAbility[] {flame_jet_ability }),
                                                     Common.createAuraFeatureComponent(NewSpells.fly_buff),
                                                     library.Get<BlueprintFeature>("14c699ccd0564e04a9587b1845d16014").GetComponent<PrerequisiteFeaturesFromList>(), //from fox fire
                                                     Helpers.PrerequisiteClassLevel(kineticist_class, 10),
                                                     Helpers.PrerequisiteFeature(flame_jet)
                                                     );

            addWildTalent(flame_jet);
            addWildTalent(flame_jet_greater);
        }


        static void updateKineticistArchetypes()
        {
            //make dark elementalist recover 2 points of burn per soul power use
            var soul_power = library.Get<BlueprintAbility>("31a1e5b27cdb78f4094630610519981c");
            soul_power.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(Common.changeAction<ContextActionHealBurn>(a.Actions.Actions, c => c.Value = 3)));
            soul_power.SetDescription("A dark elementalist uses the souls of others to protect herself from the dangers of burn. She can't choose to accept burn if doing so would raise her total number of points of burn above 3. However, a number of times per day equal to her Intelligence modifier, as a full-round action she can gather up the souls of dead creatures with HD sum equal to or higher than her character level. When she does, all of her existing burn is unloaded into the departing souls, racking it with unspeakable torment, but reducing her current burn total to zero.\nA dark elementalist gains attack and damage bonuses from elemental overflow based on how many times that day she has successfully used soul power to rack souls, rather than based on her current burn total. For instance, a 9th-level dark elementalist who had used soul power three or more times during the course of the day would add a +3 bonus on attack rolls and a +6 bonus on damage rolls. A dark elementalist does not gain size bonuses to physical ability scores or a chance to ignore critical hits and sneak attacks from elemental overflow.\nThis ability alters burn and elemental overflow and replaces internal buffer.");

            var soul_power_feature = library.Get<BlueprintFeature>("42c5a9a8661db2f47aedf87fb8b27aaf");
            soul_power_feature.SetDescription(soul_power.Description);

            //make psychockineticist burn give only -1 to skills/saves per burn value
            var psychokineticist_burn = library.Get<BlueprintBuff>("a9e3e785ea41449499b6b5d3d22a0856");
            var context_rank_config = psychokineticist_burn.GetComponent<ContextRankConfig>();
            Helpers.SetField(context_rank_config, "m_StepLevel", 1);
            var psychikineticist_burn_feature = library.Get<BlueprintFeature>("f53404854de9fd04a9ff1959385edb71");
            psychikineticist_burn_feature.SetDescription("A psychokineticist's mind strains when he overtaxes himself. He takes a –1 penalty on Will saves, Wisdom checks, and Wisdom-based skill checks for each point of burn he has accepted, rather than taking nonlethal damage from burn. He can accept an amount of burn equal to his Wisdom modifier (rather than 3 + his Wisdom modifier). Otherwise, his burn works just like a normal kineticist's.\nThis ability alters burn.");
        }


        static void fixDescriptors()
        {
            var blast_descriptor_map = new Dictionary<BlueprintAbility, SpellDescriptor>
            {
                {library.Get<BlueprintAbility>("d663a8d40be1e57478f34d6477a67270"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water }, //water blast
                {library.Get<BlueprintAbility>("4e2e066dd4dc8de4d8281ed5b3f4acb6"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water | SpellDescriptor.Electricity }, //charged water
                {library.Get<BlueprintAbility>("7980e876b0749fc47ac49b9552e259c1"), SpellDescriptor.Cold }, //cold blast
                {library.Get<BlueprintAbility>("3baf01649a92ae640927b0f633db7c11"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water | SpellDescriptor.Fire }, //steam blast
                {library.Get<BlueprintAbility>("b93e1f0540a4fa3478a6b47ae3816f32"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth |  (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air}, //sandstorm
                {library.Get<BlueprintAbility>("9afdc3eeca49c594aa7bf00e8e9803ac"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air | SpellDescriptor.Fire }, //plasma
                {library.Get<BlueprintAbility>("e2610c88664e07343b4f3fb6336f210c"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth }, //mud
                {library.Get<BlueprintAbility>("8c25f52fce5113a4491229fd1265fc3c"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Earth | SpellDescriptor.Fire }, //magma blast
                {library.Get<BlueprintAbility>("403bcf42f08ca70498432cf62abee434"), SpellDescriptor.Cold | (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water}, //ice blast
                {library.Get<BlueprintAbility>("16617b8c20688e4438a803effeeee8a6"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Water | SpellDescriptor.Cold }, //blizzard blast
                {library.Get<BlueprintAbility>("0ab1552e2ebdacf44bb7b20f5393366d"), (SpellDescriptor)AdditionalSpellDescriptors.ExtraSpellDescriptor.Air }, //air blast
            };

            foreach (var kv in blast_descriptor_map)
            {
                Common.replaceSpellDescriptor(kv.Key, kv.Value);

                foreach (var v in kv.Key.Variants)
                {
                    Common.replaceSpellDescriptor(v, kv.Value);
                }
            }
        }

        static void fixKineticistAbilitiesToBeSpelllike()
        {
            var abilities = library.GetAllBlueprints().Where<BlueprintScriptableObject>(a => a is BlueprintAbility).ToArray().Cast<BlueprintAbility>().Where(b => b.GetComponent<AbilityKineticist>() != null).ToArray();

            var combat_abilities = new BlueprintAbility[] { blade_rush_ability, blade_rush_swift_ability, whip_hurricane_ability, blade_whirlwind_ability, kinetic_whip_ability };

            foreach (var ability in abilities)
            {
                if (!combat_abilities.Contains(ability))
                {
                    ability.Type = AbilityType.SpellLike;
                }
            }
        }





        static void createInternalBuffer()
        {
            internal_buffer_resource = Helpers.CreateAbilityResource("KineticistInternalBufferResource", "", "", "", null);
            internal_buffer_resource.SetIncreasedByLevelStartPlusDivStep(0, 6, 1, 5, 1, 0, 0.0f, new BlueprintCharacterClass[] { kineticist_class });

            var icon = library.Get<BlueprintActivatableAbility>("00b6d36e31548dc4ab0ac9d15e64a980").Icon; //healing judgment
            var spend_resource = Helpers.Create<NewMechanics.ContextActionSpendResource>(s => s.resource = internal_buffer_resource);
            var buff = Helpers.CreateBuff("InternalBufferBuff",
                                          "Internal Buffer",
                                          "At 6th level, a kineticist’s study of her body and the elemental forces that course through it allow her to form an internal buffer to store extra energy.\n" +
                                          "When she would otherwise accept burn, a kineticist can spend energy from her buffer to avoid accepting 1 point of burn. She can do it once per day. Kineticist gets an additional use of this ability at levels 11 and 16.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<KineticistMechanics.DecreaseWildTalentCostWithActionOnBurn>(a => a.actions = Helpers.CreateActionList(spend_resource))
                                          );

            var ability = Helpers.CreateActivatableAbility("InternalBufferActivatableAbility",
                                                           buff.Name,
                                                           buff.Description,
                                                           "",
                                                           buff.Icon,
                                                           buff,
                                                           AbilityActivationType.Immediately,
                                                           UnitCommand.CommandType.Free,
                                                           null,
                                                           Helpers.CreateActivatableResourceLogic(internal_buffer_resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                                                           );
            ability.DeactivateImmediately = true;

            internal_buffer = Common.ActivatableAbilityToFeature(ability, hide: false);
            internal_buffer.AddComponent(Helpers.CreateAddAbilityResource(internal_buffer_resource));
            kineticist_progression.LevelEntries[5].Features.Add(internal_buffer);

            var dark_elementalist_archetype = library.Get<BlueprintArchetype>("f12f18ae8842425489d29f302e69134c");
            dark_elementalist_archetype.RemoveFeatures = dark_elementalist_archetype.RemoveFeatures.AddToArray(Helpers.LevelEntry(6, internal_buffer));

            //Witch.infusion.AllFeatures = infusion_selection.AllFeatures;
        }

        static void fixShroudOfWaterForKineticKnight()
        {
            //make shroud of water give enchancement bonus to armor or shield for burn (as of now it is not capped unlike in pnp)
            var abilities = new BlueprintAbility[] { library.Get<BlueprintAbility>("d2603c237cf8d9e41b95b71e4cf0e692"), //armor
                                                     library.Get<BlueprintAbility>("78926d1c7c01f5245974c5da015d0641") }; //shield

            var buffs = new BlueprintBuff[] { library.Get<BlueprintBuff>("04d22f8c690781d4c8f61f0437cb91ef"), //armor
                                                     library.Get<BlueprintBuff>("1f1657d95529d8945964515ca44473aa") }; //shield

            var armor_feature = library.CopyAndAdd<BlueprintFeature>("1ff803cb49f63ea4185490fae2c43ca7", "ShroudOfWaterArmorKineticKnightEffectFeature", "");
            var shield_feature = library.CopyAndAdd<BlueprintFeature>("4d8feca11d6e29a499ae761b90eacdba", "ShroudOfWaterShieldKineticKnightEffectFeature", "");

            armor_feature.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: new  BlueprintCharacterClass[]{ kineticist_class },
                                                progression: ContextRankProgression.StartPlusDivStep, startLevel: - 10, stepLevel: 4, min: 4),
                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureRank, feature: armor_feature, type: Kingmaker.Enums.AbilityRankType.DamageDice),
                Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, Kingmaker.Enums.ModifierDescriptor.Armor),
                Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, Kingmaker.Enums.ModifierDescriptor.ArmorEnhancement, rankType: Kingmaker.Enums.AbilityRankType.DamageDice),
                Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[]{armor_feature })
            };


            shield_feature.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: new  BlueprintCharacterClass[]{ kineticist_class },
                                                progression: ContextRankProgression.StartPlusDivStep, startLevel: - 2, stepLevel: 4, min: 2),
                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureRank, feature: shield_feature, type: Kingmaker.Enums.AbilityRankType.DamageDice),
                Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, Kingmaker.Enums.ModifierDescriptor.Shield),
                Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, Kingmaker.Enums.ModifierDescriptor.ShieldEnhancement, rankType: Kingmaker.Enums.AbilityRankType.DamageDice),
                Helpers.Create<RecalculateOnFactsChange>(r => r.CheckedFacts = new BlueprintUnitFact[]{shield_feature })
            };

            var features = new BlueprintFeature[] { armor_feature, shield_feature };

            for (int i = 0; i < abilities.Length; i++)
            {
                var apply_buff = abilities[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff;
                var base_buff = apply_buff.Buff;

                var new_buff = library.CopyAndAdd<BlueprintBuff>(base_buff.AssetGuid, "KineticKnight" + base_buff.name, "");
                new_buff.ReplaceComponent<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { features[i] });
                var apply_new_buff = apply_buff.CreateCopy(a => a.Buff = new_buff);

                var action = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionHasArchetype>(c => c.archetype = kinetic_knight),
                                                        apply_new_buff,
                                                        apply_buff
                                                      );
                //abilities[i].ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
                //var remove = Common.createContextActionRemoveBuff(new_buff);
                //buffs[i].GetComponent<AddFactContextActions>().Deactivated.Actions = buffs[i].GetComponent<AddFactContextActions>().Deactivated.Actions.AddToArray(remove);
            }

            armor_feature = library.Get<BlueprintFeature>("1ff803cb49f63ea4185490fae2c43ca7");
            shield_feature = library.Get<BlueprintFeature>("4d8feca11d6e29a499ae761b90eacdba");
            var base_shroud_of_water = armor_feature.GetComponent<ShroudOfWater>();
            armor_feature.ReplaceComponent<ShroudOfWater>(Helpers.Create<NewMechanics.ShroudOfWater2>(s =>
            {
                s.BaseValue = base_shroud_of_water.BaseValue;
                s.Descriptor1 = base_shroud_of_water.Descriptor;
                s.Stat = base_shroud_of_water.Stat;
                s.UpgradeFeature = base_shroud_of_water.UpgradeFeature;
                s.Descriptor2 = ModifierDescriptor.ArmorEnhancement;
                s.kinetic_knight_archetype = kinetic_knight;
            }));


            base_shroud_of_water = shield_feature.GetComponent<ShroudOfWater>();
            shield_feature.ReplaceComponent<ShroudOfWater>(Helpers.Create<NewMechanics.ShroudOfWater2>(s =>
            {
                s.BaseValue = base_shroud_of_water.BaseValue;
                s.Descriptor1 = base_shroud_of_water.Descriptor;
                s.Stat = base_shroud_of_water.Stat;
                s.UpgradeFeature = base_shroud_of_water.UpgradeFeature;
                s.Descriptor2 = ModifierDescriptor.ShieldEnhancement;
                s.kinetic_knight_archetype = kinetic_knight;
            }));

            //update description
            var elemental_bastion = library.Get<BlueprintFeature>("82fbdd5eb5ac73b498c572cc71bda48f");
            elemental_bastion.SetDescription(elemental_bastion.Description + "\nNote:  If she has the shroud of water defense wild talent, whenever its bonus would be increased by accepting burn, she instead increases the enhancement bonus of her armor or shield by an equal amount.");
        }

        static void fixBladeWhirlwindRange()
        {
            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");
            //var range = blade_whirlwind.GetComponent<AbilityTargetsAround>();
            // Helpers.SetField(range, "m_Radius", 15.Feet());
            //var attack_in_range = Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionEngagedByCaster>(), blade_whirlwind.GetComponent<AbilityEffectRunAction>().Actions.Actions);
            // blade_whirlwind.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(attack_in_range));
            var attack_engaged = Helpers.Create<NewMechanics.ContextActionOnTargetsInReach>(c => c.actions = blade_whirlwind.GetComponent<AbilityEffectRunAction>().Actions);
            blade_whirlwind.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(attack_engaged));
            blade_whirlwind.RemoveComponents<AbilityTargetsAround>();
            blade_whirlwind.NeedEquipWeapons = true;
            blade_whirlwind.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Special;
            blade_whirlwind.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSpecialAttack;
            //blade_whirlwind.AddComponent(CallOfTheWild.Helpers.Create<NewMechanics.AttackAnimation>());
        }


        static void restoreKineticKnightinfusions()
        {
            infusion_selection.Obligatory = false;
            var level_entries = new List<LevelEntry>();

            foreach (var rf in kinetic_knight.RemoveFeatures)
            {
                var features = new List<BlueprintFeatureBase>();
                foreach (var f in rf.Features)
                {
                    if (f != infusion_selection || rf.Level == 3)
                    {
                        features.Add(f);
                    }
                }
                if (features.Count != 0)
                {
                    level_entries.Add(Helpers.LevelEntry(rf.Level, features.ToArray()));
                }
            }
            kinetic_knight.RemoveFeatures = level_entries.ToArray();
        }

        static void addWhirlwindInfusionToKineticistSelection()
        {
            infusion_selection.AllFeatures = infusion_selection.AllFeatures.AddToArray(whirlwind_infusion);
        }

        static void fixKineticHealer()
        {
            var sigil_of_creation_feature = library.Get<BlueprintFeature>("3e354532d3e41b548b883f7a67f27acf");
            //add option to offload burn to other target when using kinetic healer
            var healer_base = library.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c");
            healer_base.SetDescription("With a touch, you can heal a willing living creature of an amount of damage equal to your kinetic blast’s damage. Instead of paying the burn cost yourself, you can cause the recipient to take 1 point of burn. If you do so, the recipient takes 1 point of nonlethal damage per Hit Die kineticist possesses, as usual for burn; this damage can’t be healed by any means until the recipient takes a full night’s rest.");
            var healer_burn_self = library.CopyAndAdd<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c", "KineticHealerBurnSelfAbility", "");
            var healer_burn_other = library.CopyAndAdd<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c", "KineticHealerBurnOtherAbility", "");
            healer_burn_other.CanTargetSelf = false;
            healer_burn_other.ReplaceComponent<AbilityKineticist>(a => a.WildTalentBurnCost = 0);
            healer_burn_other.SetName(healer_burn_other.Name + ": Burn Offload");

            var burn_other_buff = Helpers.CreateBuff("BurnOtherBuff",
                                                     "Burn Offload",
                                                     "",
                                                     "",
                                                     healer_burn_other.Icon,
                                                     null,
                                                     Helpers.CreateAddContextStatBonus(Kingmaker.EntitySystem.Stats.StatType.DamageNonLethal, Kingmaker.Enums.ModifierDescriptor.UntypedStackable),
                                                     Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CharacterLevel)
                                                     );
            burn_other_buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.HiddenInUi);
            burn_other_buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Stack;
            var apply_burn = Common.createContextActionApplyBuff(burn_other_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);

            healer_burn_other.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(apply_burn)));



            healer_base.ComponentsArray = new BlueprintComponent[] { Helpers.CreateAbilityVariants(healer_base, healer_burn_self, healer_burn_other) };
            sigil_of_creation_feature.GetComponent<AutoMetamagic>().Abilities = (new BlueprintAbility[] { healer_burn_self, healer_burn_other }).ToList();
        }


        static BlueprintAbility[] getKineticBladesBurnArray()
        {
            var burns = new List<BlueprintAbility>();

            foreach (var c in blast_kinetic_blades_burn_map.Values)
            {
                burns.Add(c.Item2);
            }
            return burns.ToArray();
        }


        static void createKineticWhip()
        {
            var icon = library.Get<BlueprintAbility>("16e23c7a8ae53cc42a93066d19766404").Icon; //jolt
            var blade_enabled_buff = library.Get<BlueprintBuff>("426a9c079ee7ac34aa8e0054f2218074");
            var apply_blade = Common.createContextActionApplyBuff(blade_enabled_buff, Helpers.CreateContextDuration(), dispellable: false, is_child: true, is_permanent: true);
            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");

            kinetic_whip_buff = Helpers.CreateBuff("KineticWhipBuff",
                                                   "Kinetic Whip",
                                                   "You form a long tendril of energy or elemental matter. This functions as kinetic blade but counts as a reach weapon appropriate for your size. Unlike most reach weapons, the kinetic whip can also attack nearby creatures. The kinetic whip disappears at the beginning of your next turn, but in the intervening time, it threatens all squares within its reach, allowing you to make attacks of opportunity that deal the whip’s usual damage.",
                                                   "",
                                                   icon,
                                                   null,
                                                   Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.Reach, 4, Kingmaker.Enums.ModifierDescriptor.UntypedStackable),
                                                   Helpers.CreateAddFactContextActions(activated: apply_blade),
                                                   Helpers.Create<AddConditionImmunity>(a => a.Condition = Kingmaker.UnitLogic.UnitCondition.DisableAttacksOfOpportunity)
                                                   );

            var apply_whip = Common.createContextActionApplyBuff(kinetic_whip_buff, Helpers.CreateContextDuration(1), dispellable: false);
            kinetic_whip_ability = Helpers.CreateAbility("KineticWhipAbility",
                                                kinetic_whip_buff.Name,
                                                "Element: universal\nType: form infusion\nLevel: 3\nBurn: 2\nPrerequisites: kinetic blade\nAssociated Blasts: any\nSaving Throw: none\n" + kinetic_whip_buff.Description,
                                                "",
                                                icon,
                                                AbilityType.Special,
                                                UnitCommand.CommandType.Free,
                                                AbilityRange.Personal,
                                                Helpers.oneRoundDuration,
                                                "",
                                                Helpers.CreateRunActions(apply_whip),
                                                blade_whirlwind.GetComponent<AbilityCasterHasFacts>(),
                                                Helpers.Create<AbilityKineticist>(a =>
                                                                                    {
                                                                                        a.InfusionBurnCost = 2;
                                                                                    }
                                                                                 ),
                                                Common.createAbilityCasterHasNoFacts(kinetic_whip_buff)
                                                );

            kinetic_whip_ability.setMiscAbilityParametersSelfOnly();

            addBladeInfusionCostIncrease(kinetic_whip_ability);


            kinetic_whip = Common.AbilityToFeature(kinetic_whip_ability, false);
            kinetic_whip.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 6));
            kinetic_whip.AddComponent(Helpers.PrerequisiteFeature(kinetic_blade_infusion));
            infusion_selection.AllFeatures = infusion_selection.AllFeatures.AddToArray(kinetic_whip);

            kinetic_knight.AddFeatures = kinetic_knight.AddFeatures.AddToArray(Helpers.LevelEntry(5, kinetic_whip));
            kineticist_progression.UIGroups.Last().Features.Add(kinetic_whip);

            //remove whip when using blade whirlwind, blade dash and swift blade dash
            var remove_whip = Common.createContextActionRemoveBuff(kinetic_whip_buff);
            blade_whirlwind_ability.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(remove_whip))));
            blade_rush_ability.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(remove_whip))));
            blade_rush_swift_ability.AddComponent(Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(remove_whip))));

            addToMetakinesis(kinetic_whip_ability);
        }


        static void createSparkOfLife()
        {
            BlueprintAbility summon_elemental_medium = library.Get<BlueprintAbility>("e42b1dbff4262c6469a9ff0a6ce730e3");
            BlueprintAbility summon_elemental_large = library.Get<BlueprintAbility>("89404dd71edc1aa42962824b44156fe5");
            BlueprintAbility summon_elemental_huge = library.Get<BlueprintAbility>("766ec978fa993034f86a372c8eb1fc10");
            BlueprintAbility summon_elemental_greater = library.Get<BlueprintAbility>("8eb769e3b583f594faabe1cfdb0bb696");
            BlueprintAbility summon_elemental_elder = library.Get<BlueprintAbility>("8a7f8c1223bda1541b42fd0320cdbe2b");

            var elements = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("49e55e8f24e1ad24e910fefc0258adba"), //air
                library.Get<BlueprintFeature>("d945ac76fc6a06e44b890252824db30a"), //earth
                library.Get<BlueprintFeature>("fbed3ca8c0d89124ebb3299ccf68c439"), //fire
                library.Get<BlueprintFeature>("53a8c2f3543147b4d913c6de0c57c7e8"), //water
            };

            var summon_pool = library.CopyAndAdd<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138", "SparkOfLifeSummonPool", "");
            var spark_of_life_ability = Helpers.CreateAbility("SparkOfLifeAbilityBase",
                                                              "Spark of Life",
                                                              "Element: universal\nType: utility\nLevel: 5\nBurn: 0\n"
                                                              + "You breathe a semblance of life into elemental matter, which takes the form of a Medium elemental of any of your elements as if summoned by summon monster IV with a caster level equal to your kineticist level, except the elemental gains the mindless trait. Each round on your turn, you must take a move action to guide the elemental or it collapses back into its component element. By accepting 1 point of burn, you can pour a bit of your own sentience into the elemental, removing the mindless quality and allowing it to persist for 1 round per kineticist level without requiring any further actions. At 12th level, you can choose to form a Large elemental as if by summon monster V; at 14th level, you can choose to form a Huge elemental as if by summon monster VI; at 16th level, you can choose to form a greater elemental as if by summon monster VII; and at 18th level, you can choose to form an elder elemental as if by summon monster VIII.",
                                                              "",
                                                              Helpers.GetIcon("e42b1dbff4262c6469a9ff0a6ce730e3"),
                                                              AbilityType.SpellLike,
                                                              UnitCommand.CommandType.Standard,
                                                              AbilityRange.Close,
                                                              "",
                                                              ""
                                                              );
            var variants = Helpers.CreateAbilityVariants(spark_of_life_ability);

            spark_of_life_ability.setMiscAbilityParametersRangedDirectional();


            var buff = Helpers.CreateBuff("SparkOfLifeBuff",
                                          "Spark of Life: Concentration",
                                          spark_of_life_ability.Description,
                                          "",
                                          spark_of_life_ability.Icon,
                                          null,
                                          Helpers.CreateAddFactContextActions(activated: new GameAction[] {Common.apply_concnetration},
                                                                              newRound: new GameAction[] {Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Helpers.Create<NewMechanics.HasUnitsInSummonPoolFromCaster>(h => {h.SummonPool = summon_pool; h.Not = true; })
                                                                                                                                                                     ),
                                                                                                                                    Helpers.Create<ContextActionRemoveSelf>()
                                                                                                                                    ),
                                                                                                         },
                                                                              deactivated: new GameAction[] { Helpers.Create<NewMechanics.ContextActionClearSummonPoolFromCaster>(c => c.SummonPool = summon_pool) }
                                                                              )
                                          );



            var names = new string[] { "Air", "Earth", "Fire", "Water" };


            for (int i = 0; i < 4; i++)
            {
                var action = Common.createRunActionsDependingOnContextValue(
                     Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.StatBonus),
                     summon_elemental_medium.Variants[i].GetComponent<AbilityEffectRunAction>().Actions,
                     summon_elemental_large.Variants[i].GetComponent<AbilityEffectRunAction>().Actions,
                     summon_elemental_huge.Variants[i].GetComponent<AbilityEffectRunAction>().Actions,
                     summon_elemental_greater.Variants[i].GetComponent<AbilityEffectRunAction>().Actions,
                     summon_elemental_elder.Variants[i].GetComponent<AbilityEffectRunAction>().Actions
                     );
                var normal_ability = Helpers.CreateAbility($"SparkOfLife{names[i]}Ability",
                                                          $"Spark of Life ({names[i]}, Burn)",
                                                           spark_of_life_ability.Description,
                                                           "",
                                                           spark_of_life_ability.Icon,
                                                           AbilityType.SpellLike,
                                                           UnitCommand.CommandType.Standard,
                                                           AbilityRange.Close,
                                                           Helpers.roundsPerLevelDuration,
                                                           "",
                                                           Helpers.CreateRunActions(action),
                                                           summon_elemental_medium.Variants[i].GetComponent<SpellDescriptorComponent>(),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { kineticist_class }),
                                                           Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = elements[i]),
                                                           Helpers.Create<AbilityKineticist>(a => { a.Amount = 1; a.WildTalentBurnCost = 1; }),
                                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { kineticist_class },
                                                                                           progression: ContextRankProgression.DelayedStartPlusDivStep, type: Kingmaker.Enums.AbilityRankType.StatBonus,
                                                                                           startLevel: 10, stepLevel: 2, min: 1, max: 5)
                                                           );
                normal_ability.setMiscAbilityParametersRangedDirectional();
                var new_duration = Helpers.CreateContextDuration(1, Kingmaker.UnitLogic.Mechanics.DurationRate.Hours);
                var action2 = Common.createRunActionsDependingOnContextValue(
                             Helpers.CreateContextValue(Kingmaker.Enums.AbilityRankType.StatBonus),
                             Helpers.CreateActionList((summon_elemental_medium.Variants[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).CreateCopy(sm => { sm.SummonPool = summon_pool; sm.DurationValue = new_duration; })),
                             Helpers.CreateActionList((summon_elemental_large.Variants[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).CreateCopy(sm => { sm.SummonPool = summon_pool; sm.DurationValue = new_duration; })),
                             Helpers.CreateActionList((summon_elemental_huge.Variants[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).CreateCopy(sm => { sm.SummonPool = summon_pool; sm.DurationValue = new_duration; })),
                             Helpers.CreateActionList((summon_elemental_greater.Variants[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).CreateCopy(sm => { sm.SummonPool = summon_pool; sm.DurationValue = new_duration; })),
                             Helpers.CreateActionList((summon_elemental_elder.Variants[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnMonster).CreateCopy(sm => { sm.SummonPool = summon_pool; sm.DurationValue = new_duration; }))
                             );

                var concentration_ability = Helpers.CreateAbility($"SparkOfLife{names[i]}ConcentrationAbility",
                                          $"Spark of Life ({names[i]}, Concentration)",
                                           spark_of_life_ability.Description,
                                           "",
                                           spark_of_life_ability.Icon,
                                           AbilityType.SpellLike,
                                           UnitCommand.CommandType.Standard,
                                           AbilityRange.Close,
                                           "Concentration",
                                           "",
                                           Helpers.CreateRunActions(action2,
                                                                    Common.createContextActionApplyBuffToCaster(buff, new_duration, dispellable: false)
                                                                    ),
                                           summon_elemental_medium.Variants[i].GetComponent<SpellDescriptorComponent>(),
                                           Helpers.Create<AbilityShowIfCasterHasFact>(a => a.UnitFact = elements[i]),
                                           Helpers.Create<AbilityKineticist>(a => a.Amount = 1),
                                           Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { kineticist_class },
                                                                           progression: ContextRankProgression.DelayedStartPlusDivStep, type: Kingmaker.Enums.AbilityRankType.StatBonus,
                                                                           startLevel: 10, stepLevel: 2, min: 1, max: 5),
                                           Common.createAbilityCasterHasNoFacts(Common.concentration_buff)
                                           );
                concentration_ability.setMiscAbilityParametersRangedDirectional();
                Common.setAsFullRoundAction(concentration_ability);
                variants.Variants = variants.Variants.AddToArray(normal_ability, concentration_ability);
                spark_of_life_ability.AddComponent(variants);
            }

            var stop_concentrating = Helpers.CreateAbility("SparkOfLifeStopConcentratingAbility",
                                                           "Spark of life: Stop Concentrating",
                                                           spark_of_life_ability.Description,
                                                           "",
                                                           LoadIcons.Image2Sprite.Create(@"AbilityIcons/Unsummon.png"),
                                                           AbilityType.Special,
                                                           UnitCommand.CommandType.Free,
                                                           AbilityRange.Personal,
                                                           "",
                                                           "",
                                                           Helpers.CreateRunActions(Common.createContextActionRemoveBuff(buff)),
                                                           Common.createAbilityCasterHasFacts(buff)
                                                           );
            stop_concentrating.setMiscAbilityParametersSelfOnly();
            variants.Variants = variants.Variants.AddToArray(stop_concentrating);

            spark_of_life = Common.AbilityToFeature(spark_of_life_ability, false);
            spark_of_life.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 10));
            addWildTalent(spark_of_life);
        }


        static void addWildTalent(BlueprintFeature feature)
        {
            var wild_talent_selection = library.Get<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459");
            wild_talent_selection.AllFeatures = wild_talent_selection.AllFeatures.AddToArray(feature);
        }


        static void createWhipHurricane()
        {
            var icon = LoadIcons.Image2Sprite.Create(@"AbilityIcons/WhipHurricane.png");
            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");

            whip_hurricane_ability = library.CopyAndAdd<BlueprintAbility>(blade_whirlwind.AssetGuid, "WhipHurricaneAbility", "");
            whip_hurricane_ability.SetNameDescriptionIcon("Whip Hurricane",
                                                          "Element: universal\nType: form infusion\nLevel: 6\nBurn: 4\nPrerequisites: kinetic blade, kinetic whip, blade whirlwind\nAssociated Blasts: any\nSaving Throw: none\n"
                                                          + "As blade whirlwind, except it manifests a kinetic whip, and the whip lasts until the beginning of your next turn or until you use any form infusion that creates a blade or whip again, whichever comes first.",
                                                          icon);

            var add_whip = Common.createContextActionApplyBuff(kinetic_whip_buff, Helpers.CreateContextDuration(1), dispellable: false);
            whip_hurricane_ability.ReplaceComponent<AbilityExecuteActionOnCast>(a => a.Actions = Helpers.CreateActionList(Common.createContextActionOnContextCaster(add_whip)));
            whip_hurricane_ability.ReplaceComponent<AbilityKineticist>(a => a.InfusionBurnCost = 4);
            //whip_hurricane_ability.ReplaceComponent<AbilityTargetsAround>(a => Helpers.SetField(a, "m_Radius", 20.Feet()));
            addBladeInfusionCostIncrease(whip_hurricane_ability);


            whip_hurricane = Common.AbilityToFeature(whip_hurricane_ability, false);
            whip_hurricane.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 12));
            whip_hurricane.AddComponent(Helpers.PrerequisiteFeature(kinetic_blade_infusion));
            whip_hurricane.AddComponent(Helpers.PrerequisiteFeature(kinetic_whip));
            whip_hurricane.AddComponent(Helpers.PrerequisiteFeature(whirlwind_infusion));

            infusion_selection.AllFeatures = infusion_selection.AllFeatures.AddToArray(whip_hurricane);

            kinetic_knight.AddFeatures = kinetic_knight.AddFeatures.AddToArray(Helpers.LevelEntry(11, whip_hurricane));
            kineticist_progression.UIGroups.Last().Features.Add(whip_hurricane);

            addToMetakinesis(whip_hurricane_ability);
        }



        static void createBladeRush()
        {
            var icon = NewSpells.dimension_door_free.Icon; //freedom of movement

            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");
            blade_rush_buff = library.CopyAndAdd<BlueprintBuff>("f36da144a379d534cad8e21667079066", "BladeRushBuff", "9cf447963ba0408289d3d35e97a345a1");
            blade_rush_buff.ComponentsArray = new BlueprintComponent[]
            {
                Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.AdditionalAttackBonus, 2, Kingmaker.Enums.ModifierDescriptor.None),
                Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, -2, Kingmaker.Enums.ModifierDescriptor.None),
            };
            blade_rush_ability = Helpers.CreateAbility("BladeRushAbility",
                                                            "Blade Rush",
                                                            "Element: universal\nType: form infusion\nLevel: 2\nBurn: 2\nPrerequisites: kinetic blade\nAssociated Blasts: any\nSaving Throw: none\nYou use your element’s power to instantly move 25 feet in any direction, manifest a kinetic blade, and attack once. You gain a +2 bonus on the attack roll and take a –2 penalty to your AC until the start of your next turn. The movement doesn’t provoke attacks of opportunity, though activating blade rush does. If you have the kinetic whip infusion, you can manifest a kinetic whip instead of a kinetic blade at the end of your movement by increasing the burn cost of this infusion by 1. The blade or whip vanishes instantly after the rush.",
                                                            "",
                                                            icon,
                                                            AbilityType.Special,
                                                            UnitCommand.CommandType.Standard,
                                                            AbilityRange.Close,
                                                            "",
                                                            "",
                                                            Helpers.CreateRunActions(Common.createContextActionOnContextCaster(Common.createContextActionApplyBuff(blade_rush_buff, Helpers.CreateContextDuration(1), dispellable: false)),
                                                                                     Helpers.Create<ContextActionCastSpell>(c => c.Spell = NewSpells.dimension_door_free),
                                                                                     Common.createContextActionAttack()
                                                                                     ),
                                                            blade_whirlwind.GetComponent<AbilityCasterHasFacts>(),
                                                            Helpers.Create<AbilityKineticist>(a =>
                                                                                                {
                                                                                                    a.InfusionBurnCost = 2;
                                                                                                }
                                                                                             )
                                                            );
            blade_rush_buff.SetNameDescriptionIcon(blade_rush_ability);
            blade_rush_ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            blade_rush_ability.NeedEquipWeapons = true;

            addBladeInfusionCostIncrease(blade_rush_ability);

            blade_rush = Common.AbilityToFeature(blade_rush_ability, false);
            blade_rush.AddComponent(Helpers.PrerequisiteClassLevel(kineticist_class, 4));
            blade_rush.AddComponent(Helpers.PrerequisiteFeature(kinetic_blade_infusion));
            infusion_selection.AllFeatures = infusion_selection.AllFeatures.AddToArray(blade_rush);


            blade_rush_swift_ability = library.CopyAndAdd<BlueprintAbility>(blade_rush_ability.AssetGuid, "BladeRushSwift", "");
            blade_rush_swift_ability.ActionType = UnitCommand.CommandType.Swift;
            blade_rush_swift_ability.SetNameDescription("Blade Rush (Swift Action)",
                                                        "At 13th level as a swift action, she can accept 2 points of burn to unleash a kinetic blast with the blade rush infusion."
                                                        );
            addBladeInfusionCostIncrease(blade_rush_swift_ability);
            blade_rush_swift_ability.ReplaceComponent<AbilityKineticist>(a => a.WildTalentBurnCost = 2);

            blade_rush_swift = Common.AbilityToFeature(blade_rush_swift_ability, false, "");

            kinetic_knight.AddFeatures[2].Features.Add(blade_rush);
            kinetic_knight.AddFeatures = kinetic_knight.AddFeatures.AddToArray(Helpers.LevelEntry(13, blade_rush_swift));

            kineticist_progression.UIGroups[3].Features = kineticist_progression.UIGroups[3].Features.Take(4).ToList();
            kineticist_progression.UIGroups = kineticist_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(kinetic_blade_infusion, blade_rush, whirlwind_infusion, blade_rush_swift));

            addToMetakinesis(blade_rush_ability);
        }


        static void addToMetakinesis(BlueprintAbility ability)
        {
            var metakinesis_buffs = new BlueprintBuff[]
            {
                library.Get<BlueprintBuff>("f5f3aa17dd579ff49879923fb7bc2adb"), //empower
                library.Get<BlueprintBuff>("f8d0f7099e73c95499830ec0a93e2eeb"), //empower cheaper
                library.Get<BlueprintBuff>("870d7e67e97a68f439155bdf465ea191"), //maximized
                library.Get<BlueprintBuff>("b8f43f0040155c74abd1bc794dbec320"), //maximized cheaper
                library.Get<BlueprintBuff>("f690edc756b748e43bba232e0eabd004"), //quicken
                library.Get<BlueprintBuff>("c4b74e4448b81d04f9df89ed14c38a95"), //quicken cheaper
            };

            foreach (var b in metakinesis_buffs)
            {
                var comp = b.GetComponent<AutoMetamagic>();
                
                if (comp.Abilities.Contains(ability))
                {
                    continue;
                }
                comp.Abilities.Add(ability);

                var comp2 = b.GetComponent<AddKineticistBurnModifier>();
                if (comp2.AppliableTo.Contains(ability))
                {
                    continue;
                }
                comp2.AppliableTo = comp2.AppliableTo.AddToArray(ability);

            }
        }

        static void fixBladeWhirlwindCost()
        {
            //in vanilla blade whirlwind has cost which is independent of blast type and infusions (2)
            //it should be 3 + blast cost (either 0 or 2) + substance infusion cost
            var blade_whirlwind = library.Get<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65");

            blade_whirlwind.GetComponent<AbilityKineticist>().InfusionBurnCost = 3;

            addBladeInfusionCostIncrease(blade_whirlwind);
        }


        static void addBladeInfusionCostIncrease(BlueprintAbility blade_ability)
        {
            //add blade cost (if any)
            foreach (var blade_burn in blast_kinetic_blades_burn_map.Values)
            {
                var buff = blade_burn.Item1.Buff;
                var cost = blade_burn.Item2.GetComponent<AbilityKineticist>();
                if (cost.BlastBurnCost != 0)
                {
                    buff.AddComponent(Helpers.Create<AddKineticistBurnModifier>(a =>
                                                                                {
                                                                                    a.AppliableTo = new BlueprintAbility[] { blade_ability };
                                                                                    a.BurnType = KineticistBurnType.Blast;
                                                                                    a.Value = cost.BlastBurnCost;
                                                                                }
                                                                                                                        )
                                                                                );
                }
            }

            //add infusion cost and disallow using infusions with incompatible blades
            foreach (var infusion in substance_infusions)
            {
                //Main.logger.Log(infusion.name);
                var allowed_blasts = infusion.Buff.GetComponent<AddAreaDamageTrigger>()?.AbilityList;
                if (allowed_blasts == null)
                {
                    allowed_blasts = infusion.Buff.GetComponent<AddKineticistBurnModifier>().AppliableTo;
                }

                foreach (var blast_key in blast_kinetic_blades_burn_map.Keys)
                {
                    var blast_ability = library.Get<BlueprintAbility>(blast_key);
                    if (!allowed_blasts.Contains(blast_ability))
                    {
                        blast_kinetic_blades_burn_map[blast_key].Item1.AddComponent(Common.createActivatableAbilityRestrictionHasFact(infusion.Buff, not: true));
                    }
                }
                var infusion_burn = infusion.Buff.GetComponent<AddKineticistBurnModifier>();
                if (infusion_burn != null)
                {
                    infusion_burn.AppliableTo = infusion_burn.AppliableTo.AddToArray(blade_ability);
                }
            }
        }


        static void fixKineticBladeCostForKineticKnight()
        {
            var kinetic_elemental_blade = library.Get<BlueprintFeature>("22a6db57adc348b458cb224f3047b3b2");

            kinetic_elemental_blade.ReplaceComponent<AddKineticistBurnModifier>(a => a.AppliableTo = getKineticBladesBurnArray());
        }


        static void fixKineticBladeCost()
        {
            //apply infusions cost to compatible blades
            foreach (var infusion in substance_infusions)
            {
                var burn_modifier = infusion.Buff.GetComponent<AddKineticistBurnModifier>();
                if (burn_modifier != null)
                {
                    foreach (var blast in burn_modifier.AppliableTo)
                    {
                        var abilities_to_add = new List<BlueprintAbility>();
                        if (blast_kinetic_blades_burn_map.ContainsKey(blast.AssetGuid))
                        {
                            abilities_to_add.Add(blast_kinetic_blades_burn_map[blast.AssetGuid].Item2);
                        }
                        burn_modifier.AppliableTo = burn_modifier.AppliableTo.AddToArray(abilities_to_add.ToArray());
                    }
                }
            }

            //composite blade blasts should cost 3 (blade (1)  + composite blast (2))
            foreach (var burn in getKineticBladesBurnArray())
            {
                var cost = burn.GetComponent<AbilityKineticist>();
                if (cost.InfusionBurnCost == 2)
                {
                    cost.InfusionBurnCost = 1;
                    cost.BlastBurnCost = 2;
                }
            }

        }
    }

    //do not remove kinetic blade if we have kinetic whip active
    [Harmony12.HarmonyPatch(typeof(UnitPartKineticist))]
    [Harmony12.HarmonyPatch("RemoveBladeActivatedBuff", Harmony12.MethodType.Normal)]
    class UnitPartKineticist__RemoveBladeActivatedBuff__Patch
    {
        static bool Prefix(UnitPartKineticist __instance, ref AddKineticistPart ___m_Settings)
        {
            Main.TraceLog();
            if (__instance.Owner.Buffs.HasFact(KineticistFix.kinetic_whip_buff))
            {
                return false;
            }

            /*var blade_enabled_buff = __instance.Owner.Buffs.GetBuff(___m_Settings.BladeActivatedBuff);

            if (Main.settings.kinetic_blade_refresh_for_tb && __instance.Owner.Unit.IsInCombat && blade_enabled_buff != null)
            {//remove after 5 seconds to avoid paying cost for next round in the previous one, will not properly work in rt
                blade_enabled_buff.RemoveAfterDelay(new TimeSpan(0, 0, 5));
            }
            else
            {
                __instance.Owner.Buffs.RemoveFact(___m_Settings.BladeActivatedBuff);
            }*/

            __instance.Owner.Buffs.RemoveFact(___m_Settings.BladeActivatedBuff);
            return false;
        }
    }


    //avoid getting burn on the queued command if it has cooldown
    [Harmony12.HarmonyPatch(typeof(KineticistController))]
    [Harmony12.HarmonyPatch("TryRunKineticBladeActivationAction", Harmony12.MethodType.Normal)]
    class Patch_KineticistController_TryRunKineticBladeActivationAction_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var check_blade_activated = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("IsActivatingBladeNow"));

            codes[check_blade_activated] = new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_1); //cmd
            codes.Insert(check_blade_activated + 1, new Harmony12.CodeInstruction(System.Reflection.Emit.OpCodes.Call, new Func<UnitPartKineticist, UnitCommand, bool>(shouldReturnToQueue).Method));

            return codes.AsEnumerable();
        }

        private static bool shouldReturnToQueue(UnitPartKineticist kineticist, UnitCommand cmd)
        {
            Main.TraceLog();
            return kineticist.IsActivatingBladeNow || kineticist.Owner.Unit.CombatState.HasCooldownForCommand(cmd);
        }
    }


    //fix addAreaDamageTrigger to account for descriptors of ability that provoked it (for (greater) elemental fcous feat)
    [Harmony12.HarmonyPatch(typeof(AddAreaDamageTrigger))]
    [Harmony12.HarmonyPatch("RunAction", Harmony12.MethodType.Normal)]
    class AddAreaDamageTrigger__RunAction__Patch
    {
        static bool Prefix(AddAreaDamageTrigger __instance, UnitEntityData target, ref TimeSpan ___m_LastFrameTime, ref HashSet<UnitEntityData> ___m_AffectedThisFrame)
        {
            AbilityExecutionContext ability_context = null;

            TimeSpan gameTime = Game.Instance.TimeController.GameTime;
            if (gameTime != ___m_LastFrameTime)
            {
                ___m_LastFrameTime = gameTime;
                ___m_AffectedThisFrame.Clear();
            }

            if (!___m_AffectedThisFrame.Add(target) || !__instance.Actions.HasActions)
                return false;
            if (__instance.OwnerArea != null)
            {
                ability_context = __instance.OwnerArea.Context?.SourceAbilityContext;
                var original_blueprint = __instance.OwnerArea.Context?.AssociatedBlueprint;
                Helpers.SetField(__instance.OwnerArea.Context, "AssociatedBlueprint", ability_context?.AssociatedBlueprint);
                __instance.OwnerArea.Context?.RecalculateAbilityParams(); //will trigger RuleCalculate ability params since it normally has ContextAbilityParamsCalculator component

                using (__instance.OwnerArea.Context?.GetDataScope((TargetWrapper)target))
                    __instance.Actions.Run();

                Helpers.SetField(__instance.OwnerArea.Context, "AssociatedBlueprint", original_blueprint);
                __instance.OwnerArea.Context?.RecalculateAbilityParams();
            }
            else
            {
                ability_context = Helpers.GetMechanicsContext()?.SourceAbilityContext;
                var original_blueprint = __instance.Fact.MaybeContext?.AssociatedBlueprint;
                Helpers.SetField(__instance.Fact.MaybeContext, "AssociatedBlueprint", ability_context?.AssociatedBlueprint);
                __instance.Fact.MaybeContext?.RecalculateAbilityParams(); //will trigger RuleCalculate ability params since it normally has ContextAbilityParamsCalculator component
                (__instance.Fact as IFactContextOwner)?.RunActionInContext(__instance.Actions, (TargetWrapper)target);
                Helpers.SetField(__instance.Fact.MaybeContext, "AssociatedBlueprint", original_blueprint);
                __instance.Fact.MaybeContext?.RecalculateAbilityParams();
            }


            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(IncreaseSpellDescriptorDC))]
    [Harmony12.HarmonyPatch("OnEventAboutToTrigger", Harmony12.MethodType.Normal)]
    class IncreaseSpellDescriptorDC__OnEventAboutToTrigger__Patch
    {
        static bool Prefix(IncreaseSpellDescriptorDC __instance, RuleCalculateAbilityParams evt)
        {
            var ability_context = Helpers.GetMechanicsContext()?.SourceAbilityContext;
            SpellDescriptorComponent component = ability_context?.AssociatedBlueprint?.GetComponent<SpellDescriptorComponent>();
            component = component ?? evt.Spell?.GetComponent<SpellDescriptorComponent>();
            if ((component != null ? (component.Descriptor.HasAnyFlag((SpellDescriptor)__instance.Descriptor) ? 1 : 0) : 0) == 0)
                return false;
            evt.AddBonusDC(__instance.BonusDC);
            return false;
        }
    }
}
