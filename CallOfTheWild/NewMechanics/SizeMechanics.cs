using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.SizeMechanics
{
    public class UnitPartSizeHp : UnitPart
    {
        [JsonProperty]
        private bool active = false;
        [JsonProperty]
        private int bonus_hp = 0;

        public void activate()
        {
            active = true;
            recalculateHp();
        }

        public void deactivate()
        {
            active = false;
            recalculateHp();
            
        }


        public void recalculateHp()
        {
            int new_bonus_hp = !active ? 0 : CalculateSizeBonus(this.Owner.State.Size) - CalculateSizeBonus(this.Owner.OriginalSize);
            this.Owner.Stats.HitPoints.BaseValue += (new_bonus_hp - bonus_hp);
            bonus_hp = new_bonus_hp;
        }


        private int CalculateSizeBonus(Size size)
        {
            switch (size)
            {
                case Size.Tiny:
                    return 0;
                case Size.Small:
                    return 10;
                case Size.Medium:
                    return 20;
                case Size.Large:
                    return 30;
                case Size.Huge:
                    return 40;
                case Size.Gargantuan:
                    return 60;
                case Size.Colossal:
                    return 80;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), (object)size, (string)null);
            }
        }
    }


    public class UnitPartSizeOverride : AdditiveUnitPart
    {
        public new void addBuff(Fact buff)
        {
            base.addBuff(buff);
            this.Owner?.Ensure<UnitPartSizeModifier>()?.Remove(null);
        }


        public new void removeBuff(Fact buff)
        {
            base.removeBuff(buff);
            this.Owner?.Ensure<UnitPartSizeModifier>()?.Remove(null);
        }


        public Size getSize()
        {
            if (buffs.Empty())
            {
                return this.Owner.OriginalSize;
            }
            else
            {                
                return buffs.Last().Blueprint.GetComponent<PermanentSizeOverride>().getSize();
            }
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class PermanentSizeOverride : OwnedGameLogicComponent<UnitDescriptor>
    {
        public Size size;
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartSizeOverride>().addBuff(this.Fact);
        }

        /*public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartSizeOverride>().addBuff(this.Fact);
            //this.Owner.State.Size = this.Owner.Ensure<UnitPartSizeOverride>().getSize();
        }*/


        /*public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartSizeOverride>().removeBuff(this.Fact);
            //this.Owner.State.Size = this.Owner.Ensure<UnitPartSizeOverride>().getSize();
        }*/

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartSizeOverride>().removeBuff(this.Fact);
        }

        public Size getSize()
        {
            return size;
        }
    }


    [AllowMultipleComponents]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddFeatureBasedOnOriginalSize : OwnedGameLogicComponent<UnitDescriptor>, IGlobalSubscriber
    {
        public BlueprintFeature small_feature;
        public BlueprintFeature medium_feature;
        [JsonProperty]
        private Fact m_AppliedFact;

        public override void OnFactActivate()
        {
            this.Apply();
        }

        public override void OnFactDeactivate()
        {
            this.Owner.RemoveFact(this.m_AppliedFact);
            this.m_AppliedFact = (Fact)null;
        }


        private void Apply()
        {
            this.Owner.RemoveFact(this.m_AppliedFact);
            this.m_AppliedFact = (Fact)null;

            if (this.Owner.OriginalSize == Size.Small)
            {
                if (small_feature != null)
                {
                    this.m_AppliedFact = this.Owner.AddFact(this.small_feature, null, null);
                }
            }
            else if (medium_feature != null)
            {
                this.m_AppliedFact = this.Owner.AddFact(this.medium_feature, null, null);
            }

        }
    }


    class PrerequisiteCharacterSize : Prerequisite
    {
        public Size value;
        public bool or_smaller;
        public bool or_larger;

        public override bool Check([CanBeNull] FeatureSelectionState selectionState, [NotNull] UnitDescriptor unit, [NotNull] LevelUpState state)
        {
            return CheckUnit(unit);
        }

        public bool CheckUnit(UnitDescriptor unit)
        {
            if (unit.OriginalSize == value)
                return true;

            if (or_smaller && unit.OriginalSize < value)
                return true;

            if (or_larger && unit.OriginalSize > value)
                return true;

            return false;
        }

        public override string GetUIText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string text = $"Size: {Kingmaker.Blueprints.Root.LocalizedTexts.Instance.Sizes.GetText(value)}";
            stringBuilder.Append(text);
            if (or_smaller)
                stringBuilder.Append(" or smaller");
            if (or_larger)
                stringBuilder.Append(" or larger");
            return stringBuilder.ToString();
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitPartSizeModifier), "UpdateSize")]
    class UnitPartSizeModifier_UpdateSize_Patch
    {
        static void Postfix(UnitPartSizeModifier __instance, List<Fact> ___m_SizeChangeFacts)
        {
            Main.TraceLog();
            Fact fact = ___m_SizeChangeFacts?.LastItem<Fact>();
            var part = __instance?.Owner?.Get<UnitPartSizeOverride>();
            if (fact == null && part != null)
            {
                __instance.Owner.State.Size = part.getSize();
            }

            __instance.Owner.Get<UnitPartSizeHp>()?.recalculateHp();
        }
    }


    [Harmony12.HarmonyPatch(typeof(ChangeUnitSize), "GetSize")]
    class ChangeUnitSize_GetSize_Patch
    {
        static void Postfix(ChangeUnitSize __instance, ref Size __result)
        {
            Main.TraceLog();
            var change_type = Helpers.GetField<int>(__instance, "m_Type");
            var part = __instance?.Owner?.Get<UnitPartSizeOverride>();
            if (change_type == 0 && part != null)
            {
                __result = __instance.Owner.Get<UnitPartSizeOverride>().getSize().Shift(__instance.SizeDelta);
            }
        }
    }




    public class UnitPartDoubleWeaponSize : AdditiveUnitPart
    {
        public void maybeApply(RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon?.Blueprint == null)
            {
                return;
            }

            bool res = false;
            foreach (var b in buffs)
            {
                b.CallComponents<DoubleWeaponSize>(a => res = a.apply(evt));
                {
                    if (res)
                    {
                        return;
                    }
                }
            }
        }
    }

    //double weapon damage after verything else
    [Harmony12.HarmonyPatch(typeof(RuleCalculateWeaponStats), "OnTrigger")]
    class RuleCalculateWeaponStats_OnTrigger_Patch
    {
        static bool Prefix(RuleCalculateWeaponStats __instance, RulebookEventContext context)
        {
            Main.TraceLog();
            __instance.Initiator?.Get<UnitPartDoubleWeaponSize>()?.maybeApply(__instance);
            return true;
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class DoubleWeaponSize : OwnedGameLogicComponent<UnitDescriptor>
    {
        public WeaponCategory[] categories;

        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartDoubleWeaponSize>().addBuff(this.Fact);
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartDoubleWeaponSize>().removeBuff(this.Fact);
        }

        public bool apply(RuleCalculateWeaponStats evt)
        {
            if (!categories.Empty() && !categories.Contains(evt.Weapon.Blueprint.Category))
            {
                return false;
            }

            if (evt.DoNotScaleDamage)
            {
                return false;
            }
            var wielder_size = evt.Initiator.Descriptor.State.Size;
            evt.DoNotScaleDamage = true;

            //scale weapon to the wielder size if need (note polymophs do not change their size, so their weapon dice is not supposed to scale)
            var base_weapon_dice = evt.Initiator.Body.IsPolymorphed ? evt.Weapon.Blueprint.Damage : evt.Weapon.Blueprint.ScaleDamage(wielder_size);
            DiceFormula baseDice = !evt.WeaponDamageDiceOverride.HasValue ? base_weapon_dice : (evt.Initiator.Body.IsPolymorphed ? evt.WeaponDamageDiceOverride.Value : WeaponDamageScaleTable.Scale(evt.WeaponDamageDiceOverride.Value, wielder_size));


            if (wielder_size == Size.Colossal || wielder_size == Size.Gargantuan)
            {
                //double damage dice
                DiceFormula double_damage = new DiceFormula(2 * baseDice.Rolls, baseDice.Dice);
                evt.WeaponDamageDiceOverride = new DiceFormula?(double_damage);
            }
            else
            {
                var new_dice = WeaponDamageScaleTable.Scale(baseDice, wielder_size + 2, wielder_size, evt.Weapon.Blueprint);
                if (new_dice == baseDice)
                {
                    //no scaling available
                    new_dice = new DiceFormula(2 * baseDice.Rolls, baseDice.Dice);
                }
                evt.WeaponDamageDiceOverride = new DiceFormula?(new_dice);
            }
            return true;
        }
    }



    [AllowedOn(typeof(BlueprintUnitFact))]
    public class CorrectSizeHp : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnFactActivate()
        {
            this.Owner.Ensure<UnitPartSizeHp>().activate();
        }

        public override void OnFactDeactivate()
        {
            this.Owner.Get<UnitPartSizeHp>()?.deactivate();
            this.Owner.Remove<UnitPartSizeHp>();
        }
    }
}
