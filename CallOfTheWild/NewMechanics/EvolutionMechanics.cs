using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.EvolutionMechanics
{
    public class UnitPartEvolution : UnitPart
    {
        public class EvolutionEntry
        {
            public Fact buff;
            public int cost;

            public EvolutionEntry(Fact parent_buff, int evolution_cost)
            {
                buff = parent_buff;
                cost = evolution_cost;
            }


            public EvolutionEntry()
            {
                buff = null;
                cost = 0;
            }
        }

        [JsonProperty]
        Dictionary<string, EvolutionEntry> temporary_evolutions = new Dictionary<string, EvolutionEntry>(); //id of feature and reference to buff (might be null)
        Dictionary<string, EvolutionEntry> permanent_evolutions = new Dictionary<string, EvolutionEntry>(); //id of feature and reference to buff (null)
        Dictionary<string, EvolutionEntry> short_duration_evolutions = new Dictionary<string, EvolutionEntry>();
        [JsonProperty]
        Dictionary<Fact, int> evolution_points = new Dictionary<Fact, int>();
        private int evolution_points_spent = 0;


        public int getNumEvolutionPoints()
        {
            int amount = 0;
            foreach (var kv in evolution_points)
            {
                kv.Key.CallComponents<IIncreaseEvolutionPool>(i => { amount += i.getAmount(); });
            }  
            return amount - evolution_points_spent;
        }

        public void increaseNumberOfEvolutionPoints(Fact fact)
        {
            int amount = 0;
            fact.CallComponents<IIncreaseEvolutionPool>(i => { amount += i.getAmount(); });
            evolution_points[fact] = amount;
        }

        public void removeEvolutionPointsIncrease(Fact fact)
        {
            evolution_points.Remove(fact);
        }

        public void addTemporaryEvolution(BlueprintFeature feature, int cost, Fact buff = null)
        {
            removeShortDurationEvolution2Internal(feature);
            removeTemporaryEvolution(feature, buff);
            temporary_evolutions[feature.AssetGuid] = new EvolutionEntry(buff, cost);
            evolution_points_spent += temporary_evolutions[feature.AssetGuid].cost;
        }


        public void addShortDurationEvolution(BlueprintFeature feature, Fact buff = null)
        {
            removeShortDurationEvolution(feature, buff);
            short_duration_evolutions[feature.AssetGuid] = new EvolutionEntry(buff, 0);      
        }


        public void addPermanentEvolution(BlueprintFeature feature)
        {
            removeShortDurationEvolution2Internal(feature);
            permanent_evolutions[feature.AssetGuid] = null;
        }


        public bool hasEvolution(BlueprintFeature feature)
        {
            return temporary_evolutions.ContainsKey(feature.AssetGuid) || permanent_evolutions.ContainsKey(feature.AssetGuid) || short_duration_evolutions.ContainsKey(feature.AssetGuid);
        }


        public bool hasPermanentEvolution(BlueprintFeature feature)
        {
            return permanent_evolutions.ContainsKey(feature.AssetGuid);
        }


        private void removeTemporaryEvolutionInternal(string feature_id, Fact buff)
        {
            if (temporary_evolutions.ContainsKey(feature_id) && temporary_evolutions[feature_id].buff == buff)
            {
                evolution_points_spent -= temporary_evolutions[feature_id].cost;
                temporary_evolutions.Remove(feature_id);
            }
        }


        private void removeShortDurationEvolutionInternal(string feature_id, Fact buff)
        {
            if (short_duration_evolutions.ContainsKey(feature_id) && short_duration_evolutions[feature_id].buff == buff)
            {
                short_duration_evolutions.Remove(feature_id);
            }
        }


        public void removeShortDurationEvolution(BlueprintFeature feature, Fact buff)
        {
            removeShortDurationEvolutionInternal(feature.AssetGuid, buff);
        }


        public void removeTemporaryEvolution(BlueprintFeature feature, Fact buff)
        {
            removeTemporaryEvolutionInternal(feature.AssetGuid, buff);
        }


        public void removePermanentEvolution(BlueprintFeature feature)
        {
            permanent_evolutions.Remove(feature.AssetGuid);
        }


        private void removeShortDurationEvolution2Internal(BlueprintFeature feature)
        {
            foreach (var kv in short_duration_evolutions.ToArray())
            {
                if (kv.Value.buff != null && kv.Key == feature.AssetGuid)
                {
                    this.Owner.RemoveFact(kv.Value.buff);
                }
            }
            short_duration_evolutions.Remove(feature.AssetGuid);
        }

        public void removeTemporaryEvolutions()
        {
            foreach (var kv in temporary_evolutions.ToArray())
            {                
                if (kv.Value.buff != null)
                {
                    this.Owner.RemoveFact(kv.Value.buff);
                }
            }
        }
    }

    public class UnitPartSelfEvolution : UnitPartEvolution
    {

    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddTemporaryEvolution : AddFeatureToCompanion
    {
        public int cost = 0;

        public override void OnFactActivate()
        {
            base.OnFactActivate();
            this.Owner.Ensure<UnitPartEvolution>().addTemporaryEvolution(Feature, cost, this.Fact);
        }

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            this.Owner.Ensure<UnitPartEvolution>().addTemporaryEvolution(Feature, cost, this.Fact);
        }

        public override void OnTurnOff()
        {
            base.OnFactDeactivate();
            this.Owner.Ensure<UnitPartEvolution>().removeTemporaryEvolution(Feature, this.Fact);
        }


        public override void OnFactDeactivate()
        {
            base.OnFactDeactivate();
            this.Owner.Ensure<UnitPartEvolution>().removeTemporaryEvolution(Feature, this.Fact);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddShortDurationEvolution : AddFeatureToCompanion
    {

        public override void OnFactActivate()
        {
            base.OnFactActivate();
            this.Owner.Ensure<UnitPartEvolution>().addShortDurationEvolution(Feature, this.Fact);
        }

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            this.Owner.Ensure<UnitPartEvolution>().addShortDurationEvolution(Feature, this.Fact);
        }

        public override void OnTurnOff()
        {
            base.OnFactDeactivate();
            this.Owner.Ensure<UnitPartEvolution>().removeShortDurationEvolution(Feature, this.Fact);
        }


        public override void OnFactDeactivate()
        {
            base.OnFactDeactivate();
            this.Owner.Ensure<UnitPartEvolution>().removeShortDurationEvolution(Feature, this.Fact);
        }
    }


    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddTemporarySelfEvolution : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
    {
        public BlueprintFeature Feature;
        public int cost;
        [JsonProperty]
        private Fact m_AppliedFact;

        public override void OnFactActivate()
        {
            this.Apply();
            this.Owner.Ensure<UnitPartSelfEvolution>().addTemporaryEvolution(Feature, cost, this.Fact);
        }

        public override void OnTurnOn()
        {
            this.OnFactActivate();
        }

        public override void OnTurnOff()
        {
            this.OnFactDeactivate();
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartSelfEvolution>().removeTemporaryEvolution(Feature, this.Fact);
            this.Owner.RemoveFact(this.m_AppliedFact);
            this.m_AppliedFact = null;
        }

        public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
        {
            this.Apply();
        }

        private void Apply()
        {
            if (this.m_AppliedFact != null)
                return;
            this.m_AppliedFact = this.Owner.AddFact(this.Feature, (MechanicsContext)null, (FeatureParam)null);
        }
    }


    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddShortDurationSelfEvolution : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
    {
        public BlueprintFeature Feature;
        [JsonProperty]
        private Fact m_AppliedFact;

        public override void OnFactActivate()
        {
            this.Apply();
            this.Owner.Ensure<UnitPartSelfEvolution>().addShortDurationEvolution(Feature, this.Fact);
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartSelfEvolution>().removeShortDurationEvolution(Feature, this.Fact);
            this.Owner.RemoveFact(this.m_AppliedFact);
            this.m_AppliedFact = null;
        }

        public override void OnTurnOn()
        {
            this.OnFactActivate();
        }

        public override void OnTurnOff()
        {
            this.OnFactDeactivate();
        }


        public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
        {
            this.Apply();
        }

        private void Apply()
        {
            if (this.m_AppliedFact != null)
                return;
            if (this.Owner.Progression.Features.HasFact((BlueprintFact)this.Feature))
            {
                return;
            }
            this.m_AppliedFact = this.Owner.AddFact(this.Feature, (MechanicsContext)null, (FeatureParam)null);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddPermanentEvolution : AddFeatureToCompanion
    {
        public override void OnFactActivate()
        {
            base.OnFactActivate();
            this.Owner.Ensure<UnitPartEvolution>().addPermanentEvolution(Feature);
        }


        public override void OnTurnOn()
        {
            base.OnTurnOn();
            this.Owner.Ensure<UnitPartEvolution>().addPermanentEvolution(Feature);
        }

        public override void OnTurnOff()
        {
            base.OnFactDeactivate();
            this.Owner.Ensure<UnitPartEvolution>().removePermanentEvolution(Feature);
        }

        public override void OnFactDeactivate()
        {
            base.OnFactDeactivate();
            this.Owner.Ensure<UnitPartEvolution>().removePermanentEvolution(Feature);
        }
    }


    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddPermanentSelfEvolution : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
    {
        public BlueprintFeature Feature;
        public int cost;
        [JsonProperty]
        private Fact m_AppliedFact;

        public override void OnFactActivate()
        {
            this.Apply();
            this.Owner.Ensure<UnitPartSelfEvolution>().addPermanentEvolution(Feature);
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartSelfEvolution>().removePermanentEvolution(Feature);
            this.Owner.RemoveFact(this.m_AppliedFact);
            this.m_AppliedFact = null;           
        }

        public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
        {
            this.Apply();
        }

        private void Apply()
        {
            if (this.m_AppliedFact != null)
                return;
            this.m_AppliedFact = this.Owner.AddFact(this.Feature, (MechanicsContext)null, (FeatureParam)null);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddFakeEvolution : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintFeature Feature;
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartEvolution>().addPermanentEvolution(Feature);
        }


        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartEvolution>().addPermanentEvolution(Feature);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartEvolution>().removePermanentEvolution(Feature);
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartEvolution>().removePermanentEvolution(Feature);
        }
    }


    interface IIncreaseEvolutionPool
    {
       int getAmount();
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class IncreaseEvolutionPool : OwnedGameLogicComponent<UnitDescriptor>, IIncreaseEvolutionPool
    {
        public ContextValue amount;

        public int getAmount()
        {
            return amount.Calculate(this.Fact.MaybeContext)*this.Fact.GetRank();
        }

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartEvolution>().increaseNumberOfEvolutionPoints(this.Fact);
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartEvolution>().removeEvolutionPointsIncrease(this.Fact);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class IncreaseSelfEvolutionPool : OwnedGameLogicComponent<UnitDescriptor>, IIncreaseEvolutionPool
    {
        public ContextValue amount;
        public int getAmount()
        {
            return amount.Calculate(this.Fact.MaybeContext);
        }

        public override void OnFactActivate()
        {
            this.Owner.Ensure < UnitPartSelfEvolution>().increaseNumberOfEvolutionPoints(this.Fact);
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartSelfEvolution>().removeEvolutionPointsIncrease(this.Fact);
        }
    }



    [AllowMultipleComponents]
    public class PrerequisiteEnoughEvolutionPoints : Prerequisite
    {
        public int amount;
        public BlueprintFeature feature;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
                return unit.Ensure<UnitPartEvolution>().getNumEvolutionPoints() >= amount || unit.Progression.Features.HasFact((BlueprintFact)this.feature);
        }

        public override string GetUIText()
        {
            return $"At least {amount} unused evolution point{(amount == 1 ? "" : "s")}.";
        }
    }


    [AllowMultipleComponents]
    public class PrerequisiteEnoughSelfEvolutionPoints : Prerequisite
    {
        public int amount;
        public BlueprintFeature feature;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            return unit.Ensure<UnitPartSelfEvolution>().getNumEvolutionPoints() >= amount || unit.Progression.Features.HasFact((BlueprintFact)this.feature);
        }

        public override string GetUIText()
        {
            return $"At least {amount} unused diverted evolution point {(amount == 1 ? "" : "s")}.";
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityShowIfHasEvolution : BlueprintComponent, IAbilityVisibilityProvider
    {
        public BlueprintFeature evolution;
        public bool not;

        public bool IsAbilityVisible(AbilityData ability)
        {
            if (ability.Caster.Pet == null)
            {
                return false;
            }

            return ability.Caster.Ensure<UnitPartEvolution>().hasEvolution(evolution) != not;
        }
    }

    [AllowedOn(typeof(BlueprintAbility))]
    public class PrerequisiteEvolution : Prerequisite
    {
        public BlueprintFeature evolution;
        public bool not;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            return unit.Ensure<UnitPartEvolution>().hasEvolution(evolution) != not; ;
        }

        public override string GetUIText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if ((UnityEngine.Object)this.evolution == (UnityEngine.Object)null)
            {
                UberDebug.LogError((object)("Empty Feature field in prerequisite component: " + this.name), (object[])Array.Empty<object>());
            }
            else
            {
                if (string.IsNullOrEmpty(this.evolution.Name))
                    UberDebug.LogError((object)string.Format("{0} has no Display Name", (object)this.evolution.name), (object[])Array.Empty<object>());
                stringBuilder.Append(this.evolution.Name);
            }
            if (!not)
            {
                return "Has Evolution: " + stringBuilder.ToString();
            }
            else
            {
                return "No Evolution: " + stringBuilder.ToString();
            }
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    public class PrerequisiteSelfEvolution : Prerequisite
    {
        public BlueprintFeature evolution;
        public bool not;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            return unit.Ensure<UnitPartSelfEvolution>().hasEvolution(evolution) != not; ;
        }

        public override string GetUIText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if ((UnityEngine.Object)this.evolution == (UnityEngine.Object)null)
            {
                UberDebug.LogError((object)("Empty Feature field in prerequisite component: " + this.name), (object[])Array.Empty<object>());
            }
            else
            {
                if (string.IsNullOrEmpty(this.evolution.Name))
                    UberDebug.LogError((object)string.Format("{0} has no Display Name", (object)this.evolution.name), (object[])Array.Empty<object>());
                stringBuilder.Append(this.evolution.Name);
            }
            if (!not)
            {
                return "Has Personal Evolution: " + stringBuilder.ToString();
            }
            else
            {
                return "No Personal Evolution: " + stringBuilder.ToString();
            }
        }
    }



    [AllowedOn(typeof(BlueprintAbility))]
    public class PrerequisiteNoPermanentEvolution : Prerequisite
    {
        public BlueprintFeature evolution;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            return !unit.Ensure<UnitPartEvolution>().hasPermanentEvolution(evolution);
        }

        public override string GetUIText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if ((UnityEngine.Object)this.evolution == (UnityEngine.Object)null)
            {
                UberDebug.LogError((object)("Empty Feature field in prerequisite component: " + this.name), (object[])Array.Empty<object>());
            }
            else
            {
                if (string.IsNullOrEmpty(this.evolution.Name))
                    UberDebug.LogError((object)string.Format("{0} has no Display Name", (object)this.evolution.name), (object[])Array.Empty<object>());
                stringBuilder.Append(this.evolution.Name);
            }
            return "No Permanent Evolution: " + stringBuilder.ToString();
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    public class PrerequisiteNoPermanentSelfEvolution : Prerequisite
    {
        public BlueprintFeature evolution;

        public override bool Check(
          FeatureSelectionState selectionState,
          UnitDescriptor unit,
          LevelUpState state)
        {
            return !unit.Ensure<UnitPartSelfEvolution>().hasPermanentEvolution(evolution);
        }

        public override string GetUIText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if ((UnityEngine.Object)this.evolution == (UnityEngine.Object)null)
            {
                UberDebug.LogError((object)("Empty Feature field in prerequisite component: " + this.name), (object[])Array.Empty<object>());
            }
            else
            {
                if (string.IsNullOrEmpty(this.evolution.Name))
                    UberDebug.LogError((object)string.Format("{0} has no Display Name", (object)this.evolution.name), (object[])Array.Empty<object>());
                stringBuilder.Append(this.evolution.Name);
            }
            return "No Permanent Personal Evolution: " + stringBuilder.ToString();
        }
    }



    public abstract class ComponentAppliedOnceOnLevelUp : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        public override void OnFactActivate()
        {
            try
            {

                // If we're in the level-up UI, apply the component
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {
                    Apply(levelUp.State);
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }

        // Optionally remove this fact to free some memory; useful if the fact is already applied
        // and there is no reason to track its overall rank.
        protected virtual bool RemoveAfterLevelUp => false;

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {

        }

        protected abstract void Apply(LevelUpState state);
    }

    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class RefreshEvolutionsOnLevelUp : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        public override void OnFactActivate()
        {
            try
            {
                // If we're in the level-up UI, apply the component
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {                   
                    this.Owner.Ensure<UnitPartEvolution>().removeTemporaryEvolutions();
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {

        }
    }


    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class RefreshSelfEvolutionsOnLevelUp : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        public override void OnFactActivate()
        {
            try
            {
                // If we're in the level-up UI, apply the component
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {
                    this.Owner.Ensure<UnitPartSelfEvolution>().removeTemporaryEvolutions();
                }
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {

        }
    }


    public class addEvolutionSelection : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        public BlueprintFeatureSelection selection;

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
        }

        public override void OnFactActivate()
        {
            try
            {
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {
                    if (this.Owner.Ensure<UnitPartEvolution>().getNumEvolutionPoints() > 0)
                    {
                        int index = levelUp.State.Selections.Count<FeatureSelectionState>((Func<FeatureSelectionState, bool>)(s => s.Selection == selection));
                        FeatureSelectionState featureSelectionState = new FeatureSelectionState(null, null, selection, index, 0);
                        levelUp.State.Selections.Add(featureSelectionState);
                    }
                }
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }


    public class addSelfEvolutionSelection : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        public BlueprintFeatureSelection selection;

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
        }

        public override void OnFactActivate()
        {
            try
            {
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {
                    if (this.Owner.Ensure<UnitPartSelfEvolution>().getNumEvolutionPoints() > 0)
                    {
                        int index = levelUp.State.Selections.Count<FeatureSelectionState>((Func<FeatureSelectionState, bool>)(s => s.Selection == selection));
                        FeatureSelectionState featureSelectionState = new FeatureSelectionState(null, null, selection, index, 0);
                        levelUp.State.Selections.Add(featureSelectionState);
                    }
                }
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }


    public class addSelection : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        public BlueprintFeatureSelection selection;

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
        }

        public override void OnFactActivate()
        {
            try
            {
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {
                    int index = levelUp.State.Selections.Count<FeatureSelectionState>((Func<FeatureSelectionState, bool>)(s => s.Selection == selection));
                    FeatureSelectionState featureSelectionState = new FeatureSelectionState(null, null, selection, index, 0);
                    levelUp.State.Selections.Add(featureSelectionState);
                }
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }


    public class ShareSpellbooksWithCompanion : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnFactActivate()
        {
            var pet = this.Owner.Pet;
            if (pet == null)
            {
                return;
            }

            var spellbooks = Helpers.GetField<Dictionary<BlueprintSpellbook, Spellbook>>(pet.Descriptor, "m_Spellbooks");

            foreach (var sb in this.Owner.Spellbooks)
            {
                spellbooks[sb.Blueprint] = sb;
            }
        }

        public override void OnTurnOn()
        {
            OnFactActivate();
        }

        public override void OnTurnOff()
        {
            OnFactDeactivate();
        }

        public override void OnFactDeactivate()
        {
            var pet = this.Owner.Pet;
            if (pet == null)
            {
                return;
            }

            var spellbooks = Helpers.GetField<Dictionary<BlueprintSpellbook, Spellbook>>(pet.Descriptor, "m_Spellbooks");

            foreach (var sb in this.Owner.Spellbooks)
            {
                spellbooks.Remove(sb.Blueprint);
            }
        }

    }
}
