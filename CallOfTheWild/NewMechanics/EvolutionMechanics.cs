using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
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
using Kingmaker.UnitLogic.FactLogic;
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
        [JsonProperty]
        List<BlueprintActivatableAbility> evolutions_selection_toggles = new List<BlueprintActivatableAbility>();
        [JsonProperty]
        Dictionary<string, BlueprintBuff> evolutions = new Dictionary<string, BlueprintBuff>(); //id of feature and reference to buff (might be null)
        Dictionary<string, BlueprintBuff> preselected_evolutions = new Dictionary<string, BlueprintBuff>(); //id of feature and reference to buff (might be null)

        [JsonProperty]
        BlueprintAbility activate_persistent_evolutions;
        [JsonProperty]
        bool persistent_evolutions_selected = false;

        public void initialize(BlueprintAbility activate_persistent_evolutions_ability)
        {
            activate_persistent_evolutions = activate_persistent_evolutions_ability;
        }

        public void addEvolutionSelections(BlueprintActivatableAbility[] selections)
        {
            evolutions_selection_toggles.AddRange(selections);
        }


        public void removeEvolutionSelections(BlueprintActivatableAbility[] selections)
        {
            foreach (var s in selections)
            {
                evolutions_selection_toggles.Remove(s);
            }
        }


        public void activateEvolutionSelection()
        {
            foreach (var s in evolutions_selection_toggles)
            {
                if (!this.Owner.HasFact(s))
                {
                    this.Owner.AddFact(s);
                }
            }


            unlockPersistentEvolutions();
        }


        public void deactivateEvolutionSelection()
        {
            foreach (var s in evolutions_selection_toggles)
            {
                this.Owner.RemoveFact(s);
            }
            lockPersistentEvolutions();
        }






        public void addEvolution(BlueprintFeature feature, BlueprintBuff buff = null)
        {
            evolutions[feature.AssetGuid] = buff;
        }


        public void addPreselectedEvolution(BlueprintFeature feature, BlueprintBuff buff = null)
        {
            preselected_evolutions[feature.AssetGuid] = buff;
        }


        public bool hasEvolution(BlueprintFeature feature)
        {
            return evolutions.ContainsKey(feature.AssetGuid);
        }


        public bool hasPreselectedEvolution(BlueprintFeature feature)
        {
            return preselected_evolutions.ContainsKey(feature.AssetGuid);
        }


        public bool removePreselectedEvolution(BlueprintFeature feature)
        {
            return preselected_evolutions.Remove(feature.AssetGuid);
        }


        public void removeEvolution(BlueprintFeature feature, BlueprintBuff buff)
        {
            if (evolutions.ContainsKey(feature.AssetGuid) && evolutions[feature.AssetGuid] == buff)
            {
                evolutions.Remove(feature.AssetGuid);
            }
        }


        public void lockPersistentEvolutions()
        {
            this.Owner.RemoveFact(activate_persistent_evolutions);
            persistent_evolutions_selected = true;
        }


        public void unlockPersistentEvolutions()
        {
            if (!this.Owner.HasFact(activate_persistent_evolutions))
            {
                this.Owner.AddFact(activate_persistent_evolutions);
            }
            persistent_evolutions_selected = false;
        }

        public bool persistentEvolutionsSelected()
        {
            return persistent_evolutions_selected;
        }


        public void removePersistentAndTemporaryEvolutions()
        {
            foreach (var kv in evolutions.ToArray())
            {
                if (kv.Value != null)
                {
                    this.Owner.Buffs.RemoveFact(kv.Value);
                    evolutions.Remove(kv.Key);
                }
            }
        }

    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddPreselectedEvolutions : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintBuff buff;
        public BlueprintFeature evolution;

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartEvolution>().addPreselectedEvolution(evolution, buff);
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartEvolution>().removePreselectedEvolution(evolution);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddEvolutionTogglesToEidolon : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintActivatableAbility[] toggles;

        public override void OnFactActivate()
        {
            this.Owner.Pet?.Ensure<UnitPartEvolution>().addEvolutionSelections(toggles);
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Pet?.Ensure<UnitPartEvolution>().removeEvolutionSelections(toggles);
        }
    }


    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddEvolutions : AddFacts
    {
        public BlueprintBuff buff;

        public override void OnFactActivate()
        {
            base.OnFactActivate();
            foreach (var f in this.Facts)
            {
                this.Owner.Ensure<UnitPartEvolution>().addEvolution(f as BlueprintFeature, buff);
            }
        }


        public override void OnFactDeactivate()
        {
            base.OnFactDeactivate();
            foreach (var f in this.Facts)
            {
                this.Owner.Ensure<UnitPartEvolution>().removeEvolution(f as BlueprintFeature, buff);
            }
        }
    }


    public class RestrictionHasEvolution : ActivatableAbilityRestriction
    {
        public bool not;
        public bool allow_preselected = true;
        public BlueprintFeature evolution;
        public override bool IsAvailable()
        {
            bool has_evolution = this.Owner.Ensure<UnitPartEvolution>().hasEvolution(evolution);

            if (allow_preselected)
            {
                has_evolution = has_evolution || this.Owner.Ensure<UnitPartEvolution>().hasPreselectedEvolution(evolution);
            }
            return has_evolution != not;
        }
    }


    public class RemoveEvolutionSelection : ContextAction
    {
        public override string GetCaption()
        {
            return "Deactivate persistent evolution selection.";
        }

        public override void RunAction()
        {
            this.Target.Unit?.Ensure<UnitPartEvolution>().deactivateEvolutionSelection();
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityShowIfEidolonHasEvolution : BlueprintComponent, IAbilityVisibilityProvider
    {
        public BlueprintFeature evolution;
        public bool not;

        public bool IsAbilityVisible(AbilityData ability)
        {
            if (ability.Caster.Pet == null)
            {
                return false;
            }

            return ability.Caster.Pet.Ensure<UnitPartEvolution>().hasEvolution(evolution) != not;
        }
    }


    [AllowedOn(typeof(BlueprintAbility))]
    [AllowMultipleComponents]
    public class AbilityCasterEidolonEvolutionsLocked : BlueprintComponent, IAbilityCasterChecker
    {
        public bool not = false;
        public bool CorrectCaster(UnitEntityData caster)
        {
            if (caster.Descriptor.Pet == null)
            {
                return false;
            }
            return caster.Descriptor.Pet.Ensure<UnitPartEvolution>().persistentEvolutionsSelected() != not;
        }

        public string GetReason()
        {
            return $"Eidolon evolutions must {(not ? "not " : "")}be activated.";
        }
    }


    [ComponentName("Add feature on class level")]
    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class RefreshEidolonEvolutionsOnLevelUp : OwnedGameLogicComponent<UnitDescriptor>, IUnitGainLevelHandler, IGlobalSubscriber
    {
        BlueprintCharacterClass character_class;

        public void HandleUnitGainLevel(UnitDescriptor unit, BlueprintCharacterClass @class)
        {
            if (character_class != @class)
            {
                return;
            }

            this.Owner.Pet?.Ensure<UnitPartEvolution>().removePersistentAndTemporaryEvolutions();
            this.Owner.Pet?.Ensure<UnitPartEvolution>().activateEvolutionSelection();
        }
    }
}
