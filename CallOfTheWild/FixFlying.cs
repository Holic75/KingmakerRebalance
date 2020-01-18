using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{

    class UnitPartFlying : AdditiveUnitPart
    {
        public bool isFlying()
        {
            return !buffs.Empty();
        }
    }



    [Harmony12.HarmonyPatch(typeof(RuleCalculateAC))]
    [Harmony12.HarmonyPatch("OnTrigger", Harmony12.MethodType.Normal)]
    class RuleCalculateAC__OnTrigger__FlyingFix
    {
        static bool Prefix(RuleCalculateAC __instance, RulebookEventContext context)
        {
            if (__instance.AttackType == Kingmaker.RuleSystem.AttackType.Melee || __instance.AttackType == Kingmaker.RuleSystem.AttackType.Touch)
            {
                var state = __instance.Target.Descriptor.State;
                bool target_unable_to_fly = !state.CanMove || __instance.IsTargetFlatFooted;

                bool target_fly = __instance.Target.Ensure<UnitPartFlying>().isFlying() && !target_unable_to_fly;
                bool attacker_fly = __instance.Initiator.Ensure<UnitPartFlying>().isFlying() && __instance.Initiator.Descriptor.State.CanMove;

                if (target_fly && !attacker_fly)
                {
                    __instance.AddBonus(FixFlying.fly_ac_bonus, FixFlying.flying_fact);
                }
            }
            return true;
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddFlying : OwnedGameLogicComponent<UnitDescriptor>, IUnitSubscriber
    {

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartFlying>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartFlying>().removeBuff(this.Fact);
        }
    }


    class FixFlying
    {
        static internal int fly_ac_bonus = 3;
        static LibraryScriptableObject library = Main.library;
        static internal Fact flying_fact;

        static internal void load()
        {
            createFlyingFact();
            fixFlyingCreatures();
            fixWingsBuff();
            fixGroundSpells();
        }


        static void fixGroundSpells()
        {
            //grease, entangles
            //tar pool, obsidian flow
            //spike stones, spike growth
            //tremor, ice slick, 

            //entangle is already set in vanilla
            var spells = new BlueprintAbility[]
            {
                library.Get<BlueprintAbility>("7d700cdf260d36e48bb7af3a8ca5031f"), //tar pool
                library.Get<BlueprintAbility>("e48638596c955a74c8a32dbc90b518c1"), //obsidian flow
                library.Get<BlueprintAbility>("d1afa8bc28c99104da7d784115552de5"), //spike stones
                library.Get<BlueprintAbility>("29b0f9026ad05e14789d84e867cc6dff") //spike growth
            };

            foreach (var s in spells)
            {
                s.AddComponent(Helpers.CreateSpellDescriptor(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Ground));
            }

            var areas = new BlueprintAbilityAreaEffect[]
            {
                library.Get<BlueprintAbilityAreaEffect>("d59890fb468915946bae085439bd0881"), //tar pool
                library.Get<BlueprintAbilityAreaEffect>("d64b08ae01012e34cbc55b3a27ea29b7"), //obsidian flow
                library.Get<BlueprintAbilityAreaEffect>("eca936a9e235875498d1e74ff7c09ecd"), //spike stones
                library.Get<BlueprintAbilityAreaEffect>("16e0e4c6a16f68c49832340b93706499"), //spike growth
                library.Get<BlueprintAbilityAreaEffect>("bcb6329cefc66da41b011299a43cc681") //entangle
             };

            foreach (var a in areas)
            {
                a.AddComponent(Helpers.CreateSpellDescriptor(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Ground));
            }
        }

        static void createFlyingFact()
        {
            var flying_unit_fact = Helpers.Create<BlueprintUnitFact>();

            flying_unit_fact.name = "FlyingUnitFact";
            flying_unit_fact.SetName("Flying");
            flying_unit_fact.SetDescription("");
            library.AddAsset(flying_unit_fact, "d0c3194ee46e4b55a6ea2140ebea4e05");
            flying_fact = new Fact(flying_unit_fact);
        }


        static void fixWingsBuff()
        {
            var airborne = library.Get<BlueprintFeature>("70cffb448c132fa409e49156d013b175");

            var wing_buffs = library.GetAllBlueprints().Where(b => b.name.Contains("BuffWings") && b is BlueprintBuff).Cast<BlueprintBuff>().ToList();
            wing_buffs.Add(library.Get<BlueprintBuff>("25699a90ed3299e438b6fd5548930809")); //angel wings
            wing_buffs.Add(library.Get<BlueprintBuff>("a19cda073f4c2b64ca1f8bf8fe285ece")); //black angel wings
            wing_buffs.Add(library.Get<BlueprintBuff>("4113178a8d5bf4841b8f15b1b39e004f")); //diabolic wings
            wing_buffs.Add(library.Get<BlueprintBuff>("775df52784e1d454cba0da8df5f4f59a")); //deva wings
            foreach (var wb in wing_buffs)
            {
                wb.ReplaceComponent<ACBonusAgainstAttacks>(Helpers.CreateAddFact(airborne));
            }
        }



        static void fixFlyingCreatures()
        {
            var airborne = library.Get<BlueprintFeature>("70cffb448c132fa409e49156d013b175");
            airborne.AddComponent(Common.createAddConditionImmunity(Kingmaker.UnitLogic.UnitCondition.DifficultTerrain));
            airborne.AddComponent(Common.createBuffDescriptorImmunity(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Ground));
            airborne.AddComponent(Common.createSpellImmunityToSpellDescriptor(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Ground));
            airborne.AddComponent(Helpers.Create<AddFlying>());
            airborne.AddComponent(Helpers.Create<ManeuverImmunity>(m => m.Type = CombatManeuver.Trip));
            var air_mastery = library.Get<BlueprintFeature>("be52ced7ae1c7354a8ee12d9bad47805");

            BlueprintUnitFact[] facts = new BlueprintUnitFact[]{
                                                                  library.Get<BlueprintBuff>("3689b69a30d6d7c48b90e28228fb7b7c"), //transmuter air elemental 1
                                                                  library.Get<BlueprintBuff>("2b2060036a20108448299f3ee2b14015"), //transmuter air elemental 2
                                                                  library.Get<BlueprintBuff>("2641f73f8d7864f4bba0bd6134018094"), //polymorph air elemental greater
                                                                  library.Get<BlueprintBuff>("933c5cd1113d2ef4a84f55660a744008"), //polymorph air elemental
                                                                  library.Get<BlueprintBuff>("70828d40058f2d3428bb767eb6e3e561"), //air elemental body 1
                                                                  library.Get<BlueprintBuff>("3af4d4bc55fa0ae4e851708d7395f1dd"), //air elemental body 2
                                                                  library.Get<BlueprintBuff>("db00581a03e6947419648dfba6aa03b2"), //air elemental body 3
                                                                  library.Get<BlueprintBuff>("ba06b8cff52da9e4d8432144ed6a6d19"), //air elemental body 4
                                                                  library.Get<BlueprintBuff>("dc1ef6f6d52b9fd49bc0696ab1a4f18b"), //air wildshape 1
                                                                  library.Get<BlueprintBuff>("65fdf187fea97c94b9cf4ff6746901a6"), //air wildshape 2
                                                                  library.Get<BlueprintBuff>("814bc75e74f969641bf110addf076ff9"), //air wildshape 3
                                                                  library.Get<BlueprintBuff>("eb52d24d6f60fc742b32fe943b919180"), //air wildshape 4
                                                                  library.Get<BlueprintBuff>("1a482859d9513e4418f57abcd396d315"), //wyvern

                                                                  library.Get<BlueprintBuff>("268fafac0a5b78c42a58bd9c1ae78bcf"), //black dragon I
                                                                  library.Get<BlueprintBuff>("b117bc8b41735924dba3fb23318f39ff"), //blue dragon I
                                                                  library.Get<BlueprintBuff>("17d330af03f5b3042a4417ab1d45e484"), //brass dragon I
                                                                  library.Get<BlueprintBuff>("1032d4ffb1c56444ca5bfce2c778614d"), //bronze dragon I
                                                                  library.Get<BlueprintBuff>("a4cc7169fb7e64a4a8f53bdc774341b1"), //copper dragon I
                                                                  library.Get<BlueprintBuff>("89669cfba3d9c15448c23b79dd604c41"), //gold dragon I
                                                                  library.Get<BlueprintBuff>("02611a12f38bed340920d1d427865917"), //green dragon buff I
                                                                  library.Get<BlueprintBuff>("294cbb3e1d547f341a5d7ec8500ffa44"), //red dragon I
                                                                  library.Get<BlueprintBuff>("feb2ab7613e563e45bcf9f7ffe4e05c6"), //silver dragon I
                                                                  library.Get<BlueprintBuff>("a6acd3ad1e9fa6c45998d43fd5dcd86d"), //white dragon buff I

                                                                  library.Get<BlueprintBuff>("9eb5ba8c396d2c74c8bfabd3f5e91050"), //black dragon II
                                                                  library.Get<BlueprintBuff>("cf8b4e861226e0545a6805036ab2a21b"), //blue dragon II
                                                                  library.Get<BlueprintBuff>("f7fdc15aa0219104a8b38c9891cac17b"), //brass dragon II
                                                                  library.Get<BlueprintBuff>("53e408cab2331bd48a3db846e531dfe8"), //bronze dragon II
                                                                  library.Get<BlueprintBuff>("799c8b6ae43c7d741ac7887c984f2aa2"), //copper dragon II
                                                                  library.Get<BlueprintBuff>("4300f60c00ecabc439deab11ce6d738a"), //gold dragon II
                                                                  library.Get<BlueprintBuff>("070543328d3e9af49bb514641c56911d"), //green dragon buff II
                                                                  library.Get<BlueprintBuff>("40a96969339f3c241b4d989910f255e1"), //red dragon II
                                                                  library.Get<BlueprintBuff>("16857109dafc2b94eafd1e888552ef76"), //silver dragon II
                                                                  library.Get<BlueprintBuff>("2652c61dff50a24479520c84005ede8b"), //white dragon buff II

                                                                  library.Get<BlueprintBuff>("c231e0cf7c203644d81e665d6115ae69"), //black dragon III
                                                                  library.Get<BlueprintBuff>("a4993affb4c4ad6429eca6daeb7b86a8"), //blue dragon III
                                                                  library.Get<BlueprintBuff>("8acd6ac6f89c73b4180191eb63768009"), //brass dragon III
                                                                  library.Get<BlueprintBuff>("1d3d388fd7b740842bde43dfb0aa56bb"), //bronze dragon III
                                                                  library.Get<BlueprintBuff>("c0e8f767f87ac354495865ce3dc3ee46"), //copper dragon III
                                                                  library.Get<BlueprintBuff>("ec6ad3612c4f0e340b54a11a0e78793d"), //gold dragon III
                                                                  library.Get<BlueprintBuff>("2d294863adf81f944a7558f7ae248448"), //green dragon buff III
                                                                  library.Get<BlueprintBuff>("782d09044e895fa44b9b6d9cca3a52b5"), //red dragon III
                                                                  library.Get<BlueprintBuff>("80babfb32011f384ea865d768857da79"), //silver dragon III
                                                                  library.Get<BlueprintBuff>("8dae421e48035a044a4b1a7b9208c5db"), //white dragon buff III
                                                                };
            for (int i = 0; i < facts.Length; i++)
            {
                facts[i].AddComponent(Helpers.CreateAddFact(airborne));
                if (i < 12)
                {
                    facts[i].AddComponent(Helpers.CreateAddFact(air_mastery));
                }
            }

            var airborne_types = new BlueprintUnitType[] {library.Get<BlueprintUnitType>("b012216cc6867354fb088d0c36968ea3"), //black dragon
                                                      library.Get<BlueprintUnitType>("f52b75839bb928242b6108df9d7f35a2"), //wyvern
                                                      library.Get<BlueprintUnitType>("284b6fd0b67688a4bb3ec28b15152d69"), //ankou
                                                      library.Get<BlueprintUnitType>("a574fa56e67623d41b538bdeae291fd5"), //gargoyle
                                                      library.Get<BlueprintUnitType>("535823c600a2c8f4a906063b7b949eb5"), //jabberwok
                                                      library.Get<BlueprintUnitType>("248c45d90c2f6e7429358fbe503e9c08") //manticore

                                                     };

            var units = library.GetAllBlueprints().OfType<BlueprintUnit>();
            foreach (var unit in units)
            {
                if (airborne_types.Contains(unit.Type))
                {
                    unit.AddFacts = unit.AddFacts.AddToArray(airborne);
                }
            }
        }

    }
}
