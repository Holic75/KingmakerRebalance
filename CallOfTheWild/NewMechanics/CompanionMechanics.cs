using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CallOfTheWild.CompanionMechanics
{

    public class ChangeCompanionAlignmentToMasters : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            var pet = this.Owner?.Pet;
            if (pet == null || pet.Descriptor.Alignment.Value == this.Owner.Alignment.Value)
            {
                return;
            }
            pet.Descriptor.Alignment.Set(this.Owner.Alignment.Value);
        }
    }

    public class ChangeCompanionAlignment : OwnedGameLogicComponent<UnitDescriptor>
    {
        public Alignment alignment;
        public override void OnTurnOn()
        {
            var pet = this.Owner?.Pet;
            if (pet == null || pet.Descriptor.Alignment.Value == alignment)
            {
                return;
            }

            pet.Descriptor.Alignment.Set(alignment);
        }
    }

    public class AddOutgoingDamageTriggerOnAttackerOfPetOrMaster : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleDealDamage>, IInitiatorRulebookHandler<RuleDealStatDamage>, IInitiatorRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber, IRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDrainEnergy>
    {
        public ActionList Actions;
        public bool TriggerOnStatDamageOrEnergyDrain;
        public bool reduce_below0;
        public int min_dmg = 1;
        public bool on_self = false;

        public bool only_from_weapon = false;
        public bool only_from_spell = false;
        public bool consider_damage_type = false;

        public DamageEnergyType[] energy_types;
        public PhysicalDamageForm[] physical_types;

        private void RunAction(TargetWrapper target)
        {
            if (!this.Actions.HasActions)
                return;
            (this.Fact as IFactContextOwner)?.RunActionInContext(this.Actions, target);
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (!checkPetOrMaster(evt.Target))
            {
                return;
            }
            var spellbook = Helpers.GetMechanicsContext()?.SourceAbilityContext?.Ability.Spellbook;
            if (only_from_spell && spellbook == null)
            {
                return;
            }
            if (only_from_weapon && !(evt.Reason?.Rule is RuleAttackWithWeapon))
            {
                return;
            }

            int received_damage = 0;


            if (!consider_damage_type)
            {
                received_damage = evt.Damage;
            }
            else
            {
                foreach (var d in evt.Calculate.CalculatedDamage)
                {
                    var energy_damage = (d.Source as EnergyDamage);
                    var physical_damage = (d.Source as PhysicalDamage);

                    if (energy_damage != null && energy_types.Contains(energy_damage.EnergyType))
                    {
                        received_damage += d.FinalValue;
                    }
                    if (physical_damage != null && physical_damage.Form.HasValue && physical_types.Contains(physical_damage.Form.Value))
                    {
                        received_damage += d.FinalValue;
                    }
                }
            }

            if (received_damage < min_dmg)
            {
                return;
            }

            if ((evt.Target.Damage + evt.Target.HPLeft < 0 || evt.Target.HPLeft > 0) && reduce_below0)
            {
                return;
            }

            this.RunAction(on_self ? evt.Target : evt.Initiator);

        }

        public void OnEventAboutToTrigger(RuleDealStatDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleDealStatDamage evt)
        {
            if (!checkPetOrMaster(evt.Target))
            {
                return;
            }
            if (!this.TriggerOnStatDamageOrEnergyDrain)
                return;

            if (reduce_below0 && !evt.Target.Descriptor.State.MarkedForDeath)
            {
                return;
            }
            this.RunAction(on_self ? evt.Target : evt.Initiator);
        }

        public void OnEventAboutToTrigger(RuleDrainEnergy evt)
        {
        }

        public void OnEventDidTrigger(RuleDrainEnergy evt)
        {
            if (!checkPetOrMaster(evt.Target))
            {
                return;
            }
            if (!this.TriggerOnStatDamageOrEnergyDrain)
                return;
            if (reduce_below0 && !evt.Target.Descriptor.State.MarkedForDeath)
            {
                return;
            }
            this.RunAction(on_self ? evt.Target : evt.Initiator);
        }

        private bool checkPetOrMaster(UnitEntityData unit)
        {
            if (this.Fact.MaybeContext?.MaybeCaster == null || unit == null)
            {
                return false;
            }
            if (unit == this.Fact.MaybeContext?.MaybeCaster)
            {
                return true;
            }

            if (unit.Descriptor.IsPet && unit.Descriptor.Master.Value == this.Fact.MaybeContext.MaybeCaster)
            {
                return true;
            }

            if (this.Fact.MaybeContext.MaybeCaster.Descriptor.IsPet && this.Fact.MaybeContext.MaybeCaster.Descriptor.Master.Value == unit)
            {
                return true;
            }

            return false;
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class MultiAttack : RuleInitiatorLogicComponent<RuleCalculateAttacksCount>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
    {
        public override void OnEventAboutToTrigger(RuleCalculateAttacksCount evt)
        {
            if (evt.Initiator.Descriptor.HasFact(Wildshape.no_multi_attack))
            {
                return;
            }
            if (!evt.Initiator.Body.HandsAreEnabled)
            {
                return;
            }
            if (evt.Initiator.Body.PrimaryHand.MaybeWeapon == null || !evt.Initiator.Body.PrimaryHand.MaybeWeapon.Blueprint.IsNatural || evt.Initiator.Body.PrimaryHand.MaybeWeapon.Blueprint.IsUnarmed)
                return;

            if ((bool)evt.Initiator.Descriptor.State.Features.IterativeNaturalAttacks)
            {
                return;
            }

            if (countNumberOfAttacks(evt.Initiator) < 3)
            {
                ++evt.PrimaryHand.PenalizedAttacks;
            }
        }

        public override void OnEventDidTrigger(RuleCalculateAttacksCount evt)
        {
        }

        public int countNumberOfAttacks(UnitEntityData unit)
        {
            int num_attacks = 0;
            if (unit.Body.HandsAreEnabled)
            {
                if (unit.Body.PrimaryHand.MaybeWeapon != null && unit.Body.PrimaryHand.MaybeWeapon.Blueprint.IsNatural && !unit.Body.PrimaryHand.MaybeWeapon.Blueprint.IsUnarmed)
                {
                    num_attacks++;
                    if (unit.Descriptor.HasFact(Evolutions.extra_attack))
                    {
                        num_attacks++;
                    }
                    if (unit.Descriptor.HasFact(Evolutions.extra_attack2))
                    {
                        num_attacks++;
                    }
                    if (unit.Descriptor.HasFact(Evolutions.extra_attack_serpentine))
                    {
                        num_attacks++;
                    }
                }
                if (unit.Body.SecondaryHand.MaybeWeapon != null && unit.Body.SecondaryHand.MaybeWeapon.Blueprint.IsNatural && !unit.Body.SecondaryHand.MaybeWeapon.Blueprint.IsUnarmed)
                {
                    num_attacks++;
                    if (unit.Descriptor.HasFact(Evolutions.extra_off_hand_attack))
                    {
                        num_attacks++;
                    }
                    if (unit.Descriptor.HasFact(Evolutions.extra_off_hand_attack2))
                    {
                        num_attacks++;
                    }
                    if (unit.Descriptor.HasFact(Evolutions.extra_off_hand_attack_serpentine))
                    {
                        num_attacks++;
                    }
                }
            }

            num_attacks += unit.Body.AdditionalLimbs.Where(w => w.MaybeWeapon != null && w.MaybeWeapon.Blueprint.IsNatural).Count();

            if (unit.Descriptor.HasFact(Wildshape.mutated_shape_buff))
            {
                num_attacks++;
            }
            return num_attacks;
        }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt.Initiator.Descriptor.HasFact(Wildshape.no_multi_attack))
            {
                return;
            }
            if ((bool)evt.Initiator.Descriptor.State.Features.IterativeNaturalAttacks)
            {
                return;
            }

            if (countNumberOfAttacks(evt.Initiator) < 3)
            {
                return;
            }
            if (evt.Weapon.IsSecondary)
            {
                evt.AddBonus(3, this.Fact);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {

        }
    }

    public class UnitPartUnsummonedCompanion : UnitPart
    {
        static public int min_hp => -1;
        [JsonProperty]
        private int companion_hp = min_hp;


        public bool active()
        {
            return companion_hp > min_hp;
        }

        public void activate()
        {
            if (this.Owner.Pet == null)
            {
                return;
            }
            if (active())
            {
                return;
            }
            if (this.Owner.Pet.Descriptor.HPLeft <= min_hp)
            {
                return;
            }
            companion_hp = this.Owner.Pet.Descriptor.HPLeft;
            if (Game.Instance.Player.Difficulty.DeathDoorCondition && !this.Owner.Pet.Descriptor.State.HasCondition(UnitCondition.DeathDoor))
            {
                this.Owner.Pet.Descriptor.State.AddCondition(UnitCondition.DeathDoor);
            }
            this.Owner.Pet.Descriptor.State.MarkedForDeath = true;
            this.Owner.Pet.Descriptor.State.IsUntargetable.Retain();
            //Main.logger.Log("HP:" + companion_hp.ToString());
        }

        public void deactivate()
        {
            if (!active())
            {
                return;
            }
            if (this.Owner.Pet == null)
            {
                return;
            }
            if (this.Owner.Pet.Descriptor.State.IsDead)
            {
                //Main.logger.Log("Ressurection HP:" + companion_hp.ToString());
                //Main.logger.Log("Max HP:" + this.Owner.Pet.MaxHP.ToString());
                float health_part = ((float)this.companion_hp) / this.Owner.Pet.MaxHP;
                this.Owner.Pet.Descriptor.Resurrect(health_part, true);
                //this.Owner.Pet.Descriptor.AddBuff(BlueprintRoot.Instance.SystemMechanics.ResurrectionBuff, null, new TimeSpan?(1.Rounds().Seconds));
            }
            this.Owner.Pet.Descriptor.State.IsUntargetable.Release();
            companion_hp = min_hp;
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ContextActionUnsummonCompanion : ContextAction
    {
        public override string GetCaption()
        {
            return "";
        }

        public override void RunAction()
        {
            this.Target?.Unit?.Ensure<UnitPartUnsummonedCompanion>().activate();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ContextActionSummonCompanion : ContextAction
    {
        public override string GetCaption()
        {
            return "";
        }

        public override void RunAction()
        {
            this.Target?.Unit?.Ensure<UnitPartUnsummonedCompanion>().deactivate();
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityCasterCompanionCanBeUnsummoned : BlueprintComponent, IAbilityCasterChecker
    {
        public bool CorrectCaster(UnitEntityData caster)
        {
            return caster.Descriptor.Pet != null && caster.Descriptor.Pet.Descriptor.HPLeft > UnitPartUnsummonedCompanion.min_hp;
        }

        public string GetReason()
        {
            return "Companion is not disabled";
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityCasterCompanionUnsummoned : BlueprintComponent, IAbilityCasterChecker
    {
        public bool not = false;
        public bool CorrectCaster(UnitEntityData caster)
        {
            var unsummon_part = caster.Get<UnitPartUnsummonedCompanion>();
            return (unsummon_part != null && unsummon_part.active()) != not;
        }

        public string GetReason()
        {
            return "Companion is alive";
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityCasterCompanionHasFact : BlueprintComponent, IAbilityCasterChecker
    {
        public bool not = false;
        public BlueprintUnitFact fact;
        public bool CorrectCaster(UnitEntityData caster)
        {
            if (caster?.Descriptor.Pet == null)
            {
                return false;
            }
            return caster.Descriptor.Pet.Descriptor.HasFact(fact) != not;
        }

        public string GetReason()
        {
            return "Companion " + (not ? "has" : "does not have ") + fact.Name;
        }
    }

    public class ActivatableAbilityCompanionUnsummoned : ActivatableAbilityRestriction
    {
        public bool not;
        public override bool IsAvailable()
        {
            var unsummon_part = Owner.Get<UnitPartUnsummonedCompanion>();
            return (unsummon_part != null && unsummon_part.active()) != not;
        }
    }


    public class ActivatableAbilityCompanionUnsummonedOrNoFeature : ActivatableAbilityRestriction
    {
        public bool not;
        public BlueprintFeature feature;
        public override bool IsAvailable()
        {
            var unsummon_part = Owner.Get<UnitPartUnsummonedCompanion>();
            return ((unsummon_part != null && unsummon_part.active()) || (!Owner.HasFact(feature))) != not;
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class ContextActionPetIsAlive : ContextCondition
    {
        
        protected override bool CheckCondition()
        {
            var pet = this.Context.MaybeCaster?.Descriptor?.Pet;
            return pet != null && !pet.Descriptor.State.IsDead;
        }

        protected override string GetConditionCaption()
        {
            return "";
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class ContextConditionIsPet : ContextCondition
    {

        protected override bool CheckCondition()
        {
            return this.Target.Unit?.Descriptor?.Master.Value == this.Context.MaybeCaster;
        }

        protected override string GetConditionCaption()
        {
            return "";
        }
    }


    public class CompanionWithinRange : ActivatableAbilityRestriction
    {
        public Feet range;
        public override bool IsAvailable()
        {
            var pet = this.Owner?.Pet;

            if (pet == null || pet.Descriptor.State.IsDead)
            {
                return false;
            }
 
            return  this.Owner.Unit.IsUnitInRange(pet.Position, range.Meters);
        }
    }


    public class CompanionHasFactRestriction : ActivatableAbilityRestriction
    {
        public BlueprintFeature fact;
        public override bool IsAvailable()
        {
            var pet = this.Owner?.Pet;

            if (pet == null)
            {
                return false;
            }

            return pet.Descriptor.HasFact(fact);
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityCasterCompanionDeadOrSummonPoolEmpty : BlueprintComponent, IAbilityCasterChecker
    {
        public BlueprintSummonPool SummonPool;
        public bool not;
        public bool CorrectCaster(UnitEntityData caster)
        {
            var pet = caster?.Descriptor?.Pet;
            return not != ((pet == null || pet.Descriptor.State.IsDead) || GameHelper.GetSummonPool(this.SummonPool).Count <= 0);
        }

        public string GetReason()
        {
            return $"Companion is {(not ? "dead" : "alive")}";
        }
    }






    public class TransferDamageAfterThresholdToPet : RuleTargetLogicComponent<RuleDealDamage>
    {
        public int threshold = 1;
        public override void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public override void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt.Target.Damage < 0)
            {
                return;
            }
            var pet = evt?.Target?.Descriptor?.Pet;
            if (pet == null)
            {
                return;
            }

            /*if (evt.Target.HPLeft <= 0)
            {
                return;
            }*/

            if (pet.Descriptor.State.IsDead)
            {
                return;
            }


            int max_can_transfer = pet.HPLeft + pet.Stats.Constitution - 1;
            int max_need_transfer = threshold - evt.Target.HPLeft;

            int transfer_damage = Math.Min(max_can_transfer, max_need_transfer);
            transfer_damage = Math.Min(transfer_damage, evt.Target.Damage);
            if (transfer_damage <= 0)
            {
                return;
            }
            evt.Target.Damage -= transfer_damage;
            var damage_bundle = new DamageBundle(new DirectDamage(new DiceFormula(transfer_damage, DiceType.One), 0));
            var rule = this.Fact.MaybeContext.TriggerRule(new RuleDealDamage(evt.Target, pet, damage_bundle));

        }
    }


    public class TransferDamageToMaster : RuleTargetLogicComponent<RuleDealDamage>
    {
        public override void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public override void OnEventDidTrigger(RuleDealDamage evt)
        {
            var master = evt?.Target?.Descriptor?.Master.Value;
            if (master == null)
            {
                return;
            }


            if (master.Descriptor.State.IsDead)
            {
                return;
            }

            //Main.logger.Log($"{evt.Target.Damage}, {evt.Target.HPLeft}");
            int max_can_transfer = master.HPLeft - 1;
            int max_need_transfer = (-evt.Target.HPLeft) - evt.Target.Stats.Constitution + 1;


            int transfer_damage = Math.Min(max_can_transfer, max_need_transfer);
            transfer_damage = Math.Min(transfer_damage, evt.Target.Damage);
            if (transfer_damage <= 0)
            {
                return;
            }

            evt.Target.Damage -= transfer_damage;
            var damage_bundle = new DamageBundle(new DirectDamage(new DiceFormula(transfer_damage, DiceType.One), 0));
            var rule = this.Fact.MaybeContext.TriggerRule(new RuleDealDamage(evt.Target, master, damage_bundle));
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class GrabFeaturesFromCompanion : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintFeature[] Features;
        [JsonProperty]
        private List<Fact> m_AppliedFacts = new List<Fact>();


        public override void OnFactActivate()
        {
            this.m_AppliedFacts.Clear();
            //base.OnTurnOn();
            UnitEntityData pet = this.Owner.Pet;
            if (pet == null)
                return;

            foreach (BlueprintFeature feature in this.Features)
            {
                if (pet.Descriptor.Progression.Features.HasFact((BlueprintFact)feature) && !this.Owner.HasFact((BlueprintUnitFact)feature))
                {
                    var added_fact = this.Owner.Progression.Features.AddFact((BlueprintFact)feature, null);
                    if (added_fact != null)
                    {
                        m_AppliedFacts.Add(added_fact);
                    }
                }
            }
        }

        public override void OnFactDeactivate()
        {
            //base.OnTurnOff();
            this.m_AppliedFacts.ForEach(new Action<Fact>((this.Owner).RemoveFact));
            this.m_AppliedFacts.Clear();
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SetPhysicalStatsToAnimalCompanionStats : OwnedGameLogicComponent<UnitDescriptor>
    {
        [JsonProperty]
        private readonly List<ModifiableValue.Modifier> m_AppliedModifiers = new List<ModifiableValue.Modifier>();

        public override void OnFactActivate()
        {
            UnitEntityData pet = this.Owner.Pet;
            if (pet == null)
                return;

            var stats = new StatType[] { StatType.Strength, StatType.Dexterity, StatType.Constitution };
            m_AppliedModifiers.Clear();
            foreach (var s in stats)
            {
                int bonus = pet.Stats.GetStat(s).BaseValue + pet.Stats.GetStat(s).GetDescriptorBonus(ModifierDescriptor.Racial);
                bonus -= this.Owner.Stats.GetStat(s).BaseValue + this.Owner.Stats.GetStat(s).GetDescriptorBonus(ModifierDescriptor.Racial);
                this.m_AppliedModifiers.Add(this.Owner.Stats.GetStat(s).AddModifier(bonus, (GameLogicComponent)this, ModifierDescriptor.UntypedStackable));
            }
        }


        public override void OnFactDeactivate()
        {
            this.m_AppliedModifiers.ForEach((Action<ModifiableValue.Modifier>)(m => m.Remove()));
            m_AppliedModifiers.Clear();
        }
    }

    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityTargetSelfOrMaster : BlueprintComponent, IAbilityTargetChecker
    {
        public bool Inverted;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target.Unit;
            
            return (target.Unit == caster || caster.Descriptor.Master.Value == target.Unit) != Inverted;
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityTargetHasFactUnlessPet : BlueprintComponent, IAbilityTargetChecker
    {
        public BlueprintUnitFact[] CheckedFacts;
        public bool Inverted;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target.Unit;
            if (unit == null)
                return false;
            bool flag = false;
            foreach (BlueprintUnitFact checkedFact in this.CheckedFacts)
            {
                flag = unit.Descriptor.HasFact(checkedFact);
                if (flag)
                    break;
            }
            return (flag != this.Inverted) || (caster.Descriptor.Pet != null && caster.Descriptor.Pet == target.Unit);
        }
    }


    public class UnitPartSharedSlot : AdditiveUnitPart
    {
        public bool worksFor<T>(int slot_id = 0)
        {
            foreach (var b in buffs)
            {
                var comp = b.Blueprint.GetComponent<SharedSlotComponent>();
                if (comp == null)
                {
                    continue;
                }
                if (comp.slot_type == typeof(T) && comp.slot_id == slot_id)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SharedSlotComponent: OwnedGameLogicComponent<UnitDescriptor>
    {
        public Type slot_type;
        public int slot_id = 0;

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartSharedSlot>().addBuff(this.Fact);
        }


        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartSharedSlot>().removeBuff(this.Fact);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class  SharedSlots: OwnedGameLogicComponent<UnitDescriptor>, IUnitEquipmentHandler, IGlobalSubscriber
    {
        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        {
            if (slot.Owner != this.Owner)
                return;

            var pet = Owner.Pet;
            if (pet == null)
            {
                return;
            }


            bool unequipped = slot.MaybeItem == null;
            bool equipped = previousItem == null;
                 
            if (slot == Owner.Body.Head)
            {
                if (unequipped && pet.Body.Head.Disabled)
                {
                    pet.Body.Head.ReleaseDeactivateFlag();
                }
                if (equipped && !canShareSlot<BlueprintItemEquipmentHead>())
                {
                    pet.Body.Head.RetainDeactivateFlag();
                }
            }

            if (slot == Owner.Body.Neck)
            {
                if (unequipped && pet.Body.Neck.Disabled)
                {
                    pet.Body.Neck.ReleaseDeactivateFlag();
                }
                if (equipped && !canShareSlot<BlueprintItemEquipmentNeck>())
                {
                    pet.Body.Neck.RetainDeactivateFlag();
                }
            }

            if (slot == Owner.Body.Shoulders)
            {
                if (unequipped && pet.Body.Shoulders.Disabled)
                {
                    pet.Body.Shoulders.ReleaseDeactivateFlag();
                }
                if (equipped && !canShareSlot<BlueprintItemEquipmentShoulders>())
                {
                    pet.Body.Shoulders.RetainDeactivateFlag();
                }
            }

            if (slot == Owner.Body.Wrist)
            {
                if (unequipped && pet.Body.Wrist.Disabled)
                {
                    pet.Body.Wrist.ReleaseDeactivateFlag();
                }
                if (equipped && !canShareSlot<BlueprintItemEquipmentWrist>())
                {
                    pet.Body.Wrist.RetainDeactivateFlag();
                }
            }


            if (slot == Owner.Body.Gloves)
            {
                if (unequipped && pet.Body.Gloves.Disabled)
                {
                    pet.Body.Gloves.ReleaseDeactivateFlag();
                }
                if (equipped && !canShareSlot<BlueprintItemEquipmentGloves>())
                {
                    pet.Body.Gloves.RetainDeactivateFlag();
                }
            }


            if (slot == Owner.Body.Ring1)
            {
                if (unequipped && pet.Body.Ring1.Disabled)
                {
                    pet.Body.Ring1.ReleaseDeactivateFlag();
                }
                if (equipped && !canShareSlot<BlueprintItemEquipmentRing>(1))
                {
                    pet.Body.Ring1.RetainDeactivateFlag();
                }
            }


            if (slot == Owner.Body.Ring2)
            {
                if (unequipped && pet.Body.Ring2.Disabled)
                {
                    pet.Body.Ring2.ReleaseDeactivateFlag();
                }
                if (equipped && !canShareSlot<BlueprintItemEquipmentRing>(2))
                {
                    pet.Body.Ring2.RetainDeactivateFlag();
                }
            }


            if (slot == Owner.Body.Feet)
            {
                if (unequipped && pet.Body.Feet.Disabled)
                {
                    pet.Body.Feet.ReleaseDeactivateFlag();
                }
                if (equipped && !canShareSlot<BlueprintItemEquipmentFeet>())
                {
                    pet.Body.Feet.RetainDeactivateFlag();
                }
            }


            if (slot == Owner.Body.Belt)
            {
                if (unequipped && pet.Body.Belt.Disabled)
                {
                    pet.Body.Belt.ReleaseDeactivateFlag();
                }
                if (equipped && !canShareSlot<BlueprintItemEquipmentBelt>())
                {
                    pet.Body.Belt.RetainDeactivateFlag();
                }
            }


            bool canShareSlot<T>(int slot_id = 0)
            {
                var share_part =  Owner.Pet.Get<UnitPartSharedSlot>();
                if (share_part == null)
                {
                    return false;
                }

                return share_part.worksFor<T>(slot_id);
            }
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ShareFeaturesWithCompanion2 : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintFeature[] Features;
        List<Fact> applied_facts = new List<Fact>();
        public override void OnTurnOn()
        {
            base.OnTurnOn();
            UnitEntityData pet = this.Owner.Pet;
            if (pet == null)
                return;
            foreach (BlueprintFeature feature in this.Features)
            {
                if (!pet.Descriptor.Progression.Features.HasFact(feature) && this.Owner.HasFact(feature))
                   applied_facts.Add(pet.Descriptor.Progression.Features.AddFact(feature, null));
            }
        }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            UnitEntityData pet = this.Owner.Pet;
            foreach (var f in this.applied_facts)
                pet?.Descriptor.Progression.Features.RemoveFact(f);
            applied_facts.Clear();
        }
    }


    [ComponentName("Add feature to companion with check if it has feature")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class AddFeatureToCompanion2 : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintFeature Feature;
        [JsonProperty]
        private Fact m_AppliedFact = null;

        public override void OnFactActivate()
        {
            this.TryAdd();
        }

        public override void OnFactDeactivate()
        {
            this.TryRemove();
        }

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            this.TryAdd();
        }

        private void TryAdd()
        {
            if (m_AppliedFact != null)
            {
                return;
            }
            if (this.Owner.Pet == null || this.Owner.Pet.Descriptor.Progression.Features.HasFact((BlueprintFact)this.Feature))
                return;
            m_AppliedFact = this.Owner.Pet.Descriptor.Progression.Features.AddFact((BlueprintFact)this.Feature, (MechanicsContext)null);
        }

        private void TryRemove()
        {
            if (this.Owner.Pet == null || m_AppliedFact == null)
                return;
            this.Owner.Pet.Descriptor.Progression.Features.RemoveFact(m_AppliedFact);
            m_AppliedFact = null;
        }
    }


    [ComponentName("Add feature on class level")]
    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SetAnimalCompanionRankToCharacterLevel : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler, IGlobalSubscriber
    {
        public BlueprintFeature rank_feature;
        public int level_diff;
        public BlueprintFeature boon_companion = Main.library.Get<BlueprintFeature>("8fc01f06eab4dd946baa5bc658cac556");
        [JsonProperty]
        private int m_AppliedRank;

        public override void OnFactActivate()
        {
            this.Apply();
        }

        public override void OnFactDeactivate()
        {
            for (; this.m_AppliedRank > 0; --this.m_AppliedRank)
                this.Owner.RemoveFact((BlueprintUnitFact)this.rank_feature);
        }

        private void Apply()
        {
            int actual_diff = level_diff;
            if (boon_companion != null && this.Owner.HasFact(boon_companion))
            {
                actual_diff = 0;
            }
            int rank = this.Owner.GetFact((BlueprintUnitFact)this.rank_feature).GetRank();
            int character_level = this.Owner.Progression.CharacterLevel;
            for (int i = rank; i < character_level + actual_diff; i++)
            {
                this.Owner.AddFact((BlueprintUnitFact)this.rank_feature, null);
                ++this.m_AppliedRank;
            }

            rank = this.Owner.GetFact((BlueprintUnitFact)this.rank_feature).GetRank();
            for (int i = rank; (i > character_level + actual_diff && m_AppliedRank > 0); i--)
            {
                this.Owner.RemoveFact((BlueprintUnitFact)this.rank_feature);
                --this.m_AppliedRank;
            }
            var add_pet = this.Owner.Progression.Features.Enumerable.Where(f => f?.Blueprint.GetComponent<AddPet>() != null).FirstOrDefault()?.Blueprint.GetComponent<AddPet>();
        }

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
            this.Apply();
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityShowIfMasterKnowsSpellAndPetHAsSufficientStat : BlueprintComponent, IAbilityVisibilityProvider
    {
        public BlueprintAbility spell;
        public BlueprintCharacterClass master_class;
        public StatType stat;
        public int min_stat;

        public bool IsAbilityVisible(AbilityData ability)
        {

            var master = ability.Caster?.Master.Value;
            if (master == null)
            {
                return false;
            }

            if (ability.Caster.Stats.GetStat<ModifiableValueAttributeStat>(stat).ModifiedValue < min_stat)
            {
                return false;
            }

            var master_spellbook = master.Descriptor.GetSpellbook(master_class);
            if (master_spellbook == null)
            {
                return false;
            }

            return master_spellbook.IsKnown(spell);

        }
    }





    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class FavoredTerrainBonusFromMaster : OwnedGameLogicComponent<UnitDescriptor>, IAreaLoadingStagesHandler, IGlobalSubscriber
    {
        private ModifiableValue.Modifier m_InitiativeModifier;
        private ModifiableValue.Modifier m_PerceptionModifier;
        private ModifiableValue.Modifier m_StealthModifier;
        private ModifiableValue.Modifier m_LoreNatureModifier;

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            this.CheckSettings();
        }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            this.DeactivateModifier();
        }

        public void CheckSettings()
        {
            this.DeactivateModifier();
            var unit_part_favored_terrain = this.Owner.Master.Value?.Get<UnitPartFavoredTerrain>();
            if (unit_part_favored_terrain == null)
            {
                return;
            }
            BlueprintArea current_area = Game.Instance.CurrentlyLoadedArea;
            if (current_area == null)
            {
                return;
            }

            foreach (var ft in unit_part_favored_terrain.Entries)
            {
                if (ft.Settings.Contains<LootSetting>(current_area.LootSetting))
                {
                    this.ActivateModifier(ft.Source.GetRank() * 2, ft.Source);
                    break;
                }
            }                
        }

        public void ActivateModifier(int val, Fact source)
        {
            if (this.m_InitiativeModifier == null)
                this.m_InitiativeModifier = this.Owner.Stats.Initiative.AddModifier(val, source, (string)null, ModifierDescriptor.None);
            if (this.m_PerceptionModifier == null)
                this.m_PerceptionModifier = this.Owner.Stats.SkillPerception.AddModifier(val, source, (string)null, ModifierDescriptor.None);
            if (this.m_StealthModifier == null)
                this.m_StealthModifier = this.Owner.Stats.SkillStealth.AddModifier(val, source, (string)null, ModifierDescriptor.None);
            if (this.m_LoreNatureModifier != null)
                return;
            this.m_LoreNatureModifier = this.Owner.Stats.SkillLoreNature.AddModifier(val, source, (string)null, ModifierDescriptor.None);
        }

        public void DeactivateModifier()
        {
            if (this.m_InitiativeModifier != null)
            {
                if (this.m_InitiativeModifier != null)
                    this.m_InitiativeModifier.Remove();
                this.m_InitiativeModifier = (ModifiableValue.Modifier)null;
            }
            if (this.m_PerceptionModifier != null)
            {
                if (this.m_PerceptionModifier != null)
                    this.m_PerceptionModifier.Remove();
                this.m_PerceptionModifier = (ModifiableValue.Modifier)null;
            }
            if (this.m_StealthModifier != null)
            {
                if (this.m_StealthModifier != null)
                    this.m_StealthModifier.Remove();
                this.m_StealthModifier = (ModifiableValue.Modifier)null;
            }
            if (this.m_LoreNatureModifier == null)
                return;
            if (this.m_LoreNatureModifier != null)
                this.m_LoreNatureModifier.Remove();
            this.m_LoreNatureModifier = (ModifiableValue.Modifier)null;
        }

        public void OnAreaScenesLoaded()
        {
        }

        public void OnAreaLoadingComplete()
        {
            this.CheckSettings();
        }
    }


    public class CustomLevelProgression : BlueprintComponent
    {
        public int[] rank_to_level = new int[21]
                                                    {
                                                            0,
                                                            1,
                                                            2,
                                                            3,
                                                            4,
                                                            5,
                                                            6,
                                                            7,
                                                            8,
                                                            9,
                                                            10,
                                                            11,
                                                            12,
                                                            13,
                                                            14,
                                                            15,
                                                            16,
                                                            17,
                                                            18,
                                                            19,
                                                            20,
                                                    };

        public int getLevel(AddPet add_pet_component)
        {
            if (add_pet_component.LevelRank == null)
                return 1;
            int? rank = add_pet_component.Owner.GetFact(add_pet_component.LevelRank)?.GetRank();
            int index = Mathf.Min(20, !rank.HasValue ? 0 : rank.Value);
            return rank_to_level[index];
        }

    }
}
