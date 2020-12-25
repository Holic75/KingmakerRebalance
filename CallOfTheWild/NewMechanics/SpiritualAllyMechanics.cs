using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CallOfTheWild.SpiritualAllyMechanics
{

    public class UnitPartSpiritualWeaponTargetMark: UnitPart
    {
        [JsonProperty]
        public BlueprintBuff buff;
    }




    public class ContextActionSummonSpiritualAlly : ContextActionSpawnMonster
    {
        public BlueprintUnit Blueprint;
        public ContextValue LevelValue = new ContextValue();
        public ActionList AfterSpawn;
        public BlueprintSummonPool SummonPool;
        public ContextDurationValue DurationValue;
        public ContextDiceValue CountValue;
        public bool DoNotLinkToCaster;
        public bool IsDirectlyControllable;

        public WeaponCategory category;
        public bool use_deity_weapon;

        public BlueprintBuff attack_mark_buff;
        public string custom_name;

        public override void RunAction()
        {
            UnitEntityData maybeCaster = this.Context.MaybeCaster;
            if (maybeCaster == null)
            {
                UberDebug.LogError((UnityEngine.Object)this, (object)"Caster is missing", (object[])Array.Empty<object>());
            }
            else
            {
                Rounds duration = this.DurationValue.Calculate(this.Context);
                int count = this.CountValue.Calculate(this.Context);
                int level = this.LevelValue.Calculate(this.Context);
                Vector3 clampedPosition = ObstacleAnalyzer.GetNearestNode(this.Target.Point).clampedPosition;
                UnitEntityView unitEntityView = this.Blueprint.Prefab.Load(false);
                float radius = (UnityEngine.Object)unitEntityView != (UnityEngine.Object)null ? unitEntityView.Corpulence : 0.5f;
                FreePlaceSelector.PlaceSpawnPlaces(count, radius, clampedPosition);
                for (int index = 0; index < count; ++index)
                {
                    Vector3 relaxedPosition = FreePlaceSelector.GetRelaxedPosition(index, true);
                    UnitEntityData summonedUnit = this.Context.TriggerRule<RuleSummonUnit>(new RuleSummonUnit(maybeCaster, this.Blueprint, relaxedPosition, duration, level)
                    {
                        Context = this.Context,
                        DoNotLinkToCaster = this.DoNotLinkToCaster
                    }).SummonedUnit;
                    var weapon = getWeapon();
                    if (weapon != null)
                    {
                        ItemsCollection.DoWithoutEvents((Action)(() => summonedUnit.Body.PrimaryHand.InsertItem(weapon.CreateEntity())));
                    }

                    if (attack_mark_buff != null)
                    {
                        summonedUnit.Ensure<UnitPartSpiritualWeaponTargetMark>().buff = attack_mark_buff;
                    }
                    if ((UnityEngine.Object)this.SummonPool != (UnityEngine.Object)null)
                        GameHelper.RegisterUnitInSummonPool(this.SummonPool, summonedUnit);
                    using (this.Context.GetDataScope((TargetWrapper)summonedUnit))
                        this.AfterSpawn.Run();

                    summonedUnit.Descriptor.CustomName = custom_name;
                    EventBus.RaiseEvent<IUnitNameHandler>((Action<IUnitNameHandler>)(h => h.OnUnitNameChanged(summonedUnit)));
                }
            }
        }


        BlueprintItemWeapon getWeapon()
        {
            if (!use_deity_weapon)
            {
                return getDeafaultWeapon(category);
            }

            var deity_weapon = this.Context.MaybeCaster.Descriptor.Progression.Features.Enumerable.Where<Kingmaker.UnitLogic.Feature>(p => p.Blueprint == NewFeats.deity_favored_weapon).FirstOrDefault();
            
            if (deity_weapon == null || !deity_weapon.Param.WeaponCategory.HasValue)
            {
                if (this.Context.MaybeCaster.Descriptor.Alignment.Value.HasComponent(AlignmentComponent.Lawful))
                {
                    return getDeafaultWeapon(WeaponCategory.Longsword);
                }
                else if (this.Context.MaybeCaster.Descriptor.Alignment.Value.HasComponent(AlignmentComponent.Chaotic))
                {
                    return getDeafaultWeapon(WeaponCategory.Battleaxe);
                }
                else if (this.Context.MaybeCaster.Descriptor.Alignment.Value.HasComponent(AlignmentComponent.Good))
                {
                    return getDeafaultWeapon(WeaponCategory.Warhammer);
                }
                else if (this.Context.MaybeCaster.Descriptor.Alignment.Value.HasComponent(AlignmentComponent.Evil))
                {
                    return getDeafaultWeapon(WeaponCategory.Flail);
                }
                else //neutral
                {
                    return getDeafaultWeapon(WeaponCategory.Scimitar);
                }
            }
            else
            {
                return getDeafaultWeapon(deity_weapon.Param.WeaponCategory.Value);
            }
        }
 
        BlueprintItemWeapon getDeafaultWeapon(WeaponCategory category)
        {
            return SpiritualWeapons.category_weapon_map[category];
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class SpiritualWeaponBab : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintFeature alternative_feature;
        public ContextValue alternative_value;
        private ModifiableValue.Modifier m_Modifier;

        public override void OnTurnOn()
        {
            int num = this.Fact.MaybeContext.MaybeCaster.Descriptor.Stats.BaseAttackBonus.BaseValue; //ModifiedValue ?

            if (alternative_feature != null && this.Fact.MaybeContext.MaybeCaster.Descriptor.HasFact(alternative_feature))
            {
                num = alternative_value.Calculate(this.Fact.MaybeContext);
            }
            num = num - this.Owner.Stats.BaseAttackBonus.BaseValue;
            this.m_Modifier = this.Owner.Stats.BaseAttackBonus.AddModifier(num, (GameLogicComponent)this, ModifierDescriptor.None);
        }

        public override void OnTurnOff()
        {
            this.m_Modifier?.Remove();
            this.m_Modifier = (ModifiableValue.Modifier)null;
        }
    }


    public class TargetMarkedConsideration : Consideration
    {
        [Range(0.0f, 1f)]
        public float AliveScore;
        [Range(0.0f, 1f)]
        public float DeadScore;
        [Range(0.0f, 1f)]
        public float UnconsciousScore;

        public override float Score(DecisionContext context)
        {
            
            var mark = context.Unit.Get<UnitPartSpiritualWeaponTargetMark>()?.buff;
            if (mark == null)
            {
                return 0.5f;
            }
            var summoner = context.Unit?.Get<UnitPartSummonedMonster>()?.Summoner;
            if (context.Target.Unit == null || summoner == null)
            {
                return 0.5f;
            }

            if (context.Target.Unit.Buffs.Enumerable.Any(b => b.Blueprint == mark && b.Context.MaybeCaster == summoner))
            {
                return 1.0f;
            }
            else
            {
                return 0.001f;
            }
        }
    }
}
