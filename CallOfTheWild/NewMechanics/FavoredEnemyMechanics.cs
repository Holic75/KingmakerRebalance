using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.FavoredEnemyMechanics
{
    [AllowMultipleComponents]
    [ComponentName("Armor check penalty increase")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ACBonusAgainstFavoredEnemy : OwnedGameLogicComponent<UnitDescriptor>, ITargetRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleAttackRoll>, ITargetRulebookSubscriber
    {
        public ModifierDescriptor descriptor;
        public ContextValue value;

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            var unit_part_fe = this.Owner.Get<UnitPartFavoredEnemy>();
            if (unit_part_fe == null)
            {
                return;
            }

            foreach (var fe in unit_part_fe.Entries)
            {
                if ((fe.CheckedFeatures).Any<BlueprintUnitFact>((Func<BlueprintUnitFact, bool>)(p => evt.Initiator.Descriptor.HasFact(p))))
                {
                    evt.AddTemporaryModifier(evt.Target.Stats.AC.AddModifier(this.value.Calculate(this.Fact.MaybeContext) * this.Fact.GetRank(), (GameLogicComponent)this, this.descriptor));
                    break;
                }
            }
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddHalfFavoredEnemy : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintUnitFact[] CheckedFacts;

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartFavoredEnemy>().AddEntry(this.CheckedFacts, this.Fact, this.Fact.GetRank());
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartAbilityModifiers>().RemoveEntry(this.Fact);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class BlessedHunterTerrain : OwnedGameLogicComponent<UnitDescriptor>, IAreaLoadingStagesHandler, IGlobalSubscriber
    {
        public LootSetting[] Settings;
        private ModifiableValue.Modifier m_DamageModifier;
        private ModifiableValue.Modifier m_WillSaveModifier;
        private ModifiableValue.Modifier m_FortSaveModifier;
        private ModifiableValue.Modifier m_RefSaveModifier;

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            this.CheckSettings();
            this.Owner.Ensure<UnitPartFavoredTerrain>().AddEntry(this.Settings, this.Fact);
        }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            this.DeactivateModifier();
            this.Owner.Ensure<UnitPartFavoredTerrain>().RemoveEntry(this.Fact);
        }

        public void CheckSettings()
        {
            BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
            if (currentlyLoadedArea != null && ((IEnumerable<LootSetting>)this.Settings).Contains<LootSetting>(currentlyLoadedArea.LootSetting))
                this.ActivateModifier();
            else
                this.DeactivateModifier();
        }

        public void ActivateModifier()
        {
            if (this.m_DamageModifier == null)
                this.m_DamageModifier = this.Owner.Stats.AdditionalDamage.AddModifier(this.Fact.GetRank(), (GameLogicComponent)this, ModifierDescriptor.UntypedStackable);
            if (this.m_WillSaveModifier == null)
                this.m_WillSaveModifier = this.Owner.Stats.SaveWill.AddModifier(this.Fact.GetRank(), (GameLogicComponent)this, ModifierDescriptor.UntypedStackable);
            if (this.m_FortSaveModifier == null)
                this.m_FortSaveModifier = this.Owner.Stats.SaveFortitude.AddModifier(this.Fact.GetRank(), (GameLogicComponent)this, ModifierDescriptor.UntypedStackable);
            if (this.m_RefSaveModifier == null)
                this.m_RefSaveModifier = this.Owner.Stats.SaveReflex.AddModifier(this.Fact.GetRank(), (GameLogicComponent)this, ModifierDescriptor.UntypedStackable);
        }

        public void DeactivateModifier()
        {
            if (this.m_DamageModifier != null)
            {
                if (this.m_DamageModifier != null)
                    this.m_DamageModifier.Remove();
                this.m_DamageModifier = (ModifiableValue.Modifier)null;
            }
            if (this.m_FortSaveModifier != null)
            {
                if (this.m_FortSaveModifier != null)
                    this.m_FortSaveModifier.Remove();
                this.m_FortSaveModifier = (ModifiableValue.Modifier)null;
            }
            if (this.m_WillSaveModifier != null)
            {
                if (this.m_WillSaveModifier != null)
                    this.m_WillSaveModifier.Remove();
                this.m_WillSaveModifier = (ModifiableValue.Modifier)null;
            }
            if (this.m_RefSaveModifier != null)
            {
                if (this.m_RefSaveModifier != null)
                    this.m_RefSaveModifier.Remove();
                this.m_RefSaveModifier = (ModifiableValue.Modifier)null;
            }
        }

        public void OnAreaScenesLoaded()
        {
        }

        public void OnAreaLoadingComplete()
        {
            this.CheckSettings();
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityTargetSecondaryHandFree : BlueprintComponent, IAbilityTargetChecker
    {
        public bool not;
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            if (target?.Unit == null)
            {
                return false;
            }

            return not != (!target.Unit.Body.SecondaryHand.HasItem && !HoldingItemsMechanics.Helpers.has2hWeapon(target.Unit.Body.PrimaryHand));
        }

        public string GetReason()
        {
            return "Need free secondary hand.";
        }
    }

    public class AbilityTargetIsFavoredEnemy : BlueprintComponent, IAbilityTargetChecker
    {
        public bool not;
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target.Unit;

            if (unit == null || caster == null)
                return false;
            bool flag = false;
            foreach (UnitPartFavoredEnemy.FavoredEntry entry in caster.Ensure<UnitPartFavoredEnemy>().Entries)
            {
                flag = ((IEnumerable<BlueprintUnitFact>)entry.CheckedFeatures).Any<BlueprintUnitFact>((Func<BlueprintUnitFact, bool>)(p => unit.Descriptor.HasFact(p)));
                if (flag)
                    break;
            }
            return flag != not;
        }

        public string GetReason()
        {
            return $"Target must {(not ? "not " : "")}be favored enemy";
        }
    }
}
