using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild.BuffMechanics
{
    public class UnitPartStoreBuff : AdditiveUnitPart
    {
        public void removeAllBuffsByBlueprint(BlueprintBuff buff)
        {
            var found_buffs =  buffs.FindAll(b => b.Blueprint == buff).OfType<Buff>().ToArray();
            buffs.RemoveAll(b => b.Blueprint == buff);
            foreach (var b in found_buffs)
            {
                b.Remove();
            }
            
        }
    }


    public class StoreBuff : BuffLogic
    {
        public override void OnFactActivate()
        {
            this.Buff.Context.MaybeCaster?.Ensure<UnitPartStoreBuff>().addBuff(this.Buff);
        }

        public override void OnFactDeactivate()
        {
            this.Buff.Context.MaybeCaster?.Get<UnitPartStoreBuff>()?.removeBuff(this.Buff);
        }
    }




    public class RemoveStoredBuffs : ContextAction
    {
        public BlueprintBuff buff;
        public override string GetCaption()
        {
            return "Remove unique buff";
        }


        public override void RunAction()
        {
            var part_store_buff = this.Target.Unit?.Get<UnitPartStoreBuff>();

            if (part_store_buff == null)
            {
                return;
            }

            part_store_buff.removeAllBuffsByBlueprint(buff);
        }
    }

    public class SuppressBuffsCorrect : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintBuff[] Buffs = new BlueprintBuff[0];
        public SpellSchool[] Schools;
        public SpellDescriptorWrapper Descriptor;

        public override void OnFactActivate()
        {
            var partBuffSuppress = this.Owner.Ensure<UnitPartBuffSuppressSaved>();
            if (this.Descriptor != SpellDescriptor.None)
                partBuffSuppress.Suppress((SpellDescriptor)this.Descriptor);
            if (!((IList<SpellSchool>)this.Schools).Empty<SpellSchool>())
                partBuffSuppress.Suppress(this.Schools);
            foreach (BlueprintBuff buff in this.Buffs)
                partBuffSuppress.Suppress(buff);
        }

        public override void OnFactDeactivate()
        {
            var partBuffSuppress = this.Owner.Get<UnitPartBuffSuppressSaved>();
            if (!(bool)((UnitPart)partBuffSuppress))
            {
                UberDebug.LogError((object)"UnitPartSuppressBuff is missing", (object[])Array.Empty<object>());
            }
            else
            {
                if (this.Descriptor != SpellDescriptor.None)
                    partBuffSuppress.Release((SpellDescriptor)this.Descriptor);
                if (!((IList<SpellSchool>)this.Schools).Empty<SpellSchool>())
                    partBuffSuppress.Release(this.Schools);
                foreach (BlueprintBuff buff in this.Buffs)
                    partBuffSuppress.Release(buff);
            }
        }
    }


    public class UnitPartBuffSuppressSaved : UnitPart
    {
        [JsonProperty]
        private readonly List<SpellDescriptor> m_SpellDescriptors = new List<SpellDescriptor>();
        [JsonProperty]
        private readonly List<BlueprintBuff> m_Buffs = new List<BlueprintBuff>();
        [JsonProperty]
        private readonly List<SpellSchool> m_SpellSchools = new List<SpellSchool>();

        private static IEnumerable<SpellDescriptor> GetValues(
          SpellDescriptor spellDescriptor)
        {
            return EnumUtils.GetValues<SpellDescriptor>().Where<SpellDescriptor>((Func<SpellDescriptor, bool>)(v =>
            {
                if (v != SpellDescriptor.None)
                    return (ulong)(spellDescriptor & v) > 0UL;
                return false;
            }));
        }

        public void Suppress(SpellSchool[] spellSchools)
        {
            foreach (SpellSchool spellSchool in spellSchools)
                this.m_SpellSchools.Add(spellSchool);
        }

        public void Suppress(SpellDescriptor spellDescriptor)
        {
            foreach (SpellDescriptor spellDescriptor1 in UnitPartBuffSuppressSaved.GetValues(spellDescriptor))
                this.m_SpellDescriptors.Add(spellDescriptor1);
            this.Update();
        }

        public void Suppress(BlueprintBuff buff)
        {
            this.m_Buffs.Add(buff);
            this.Update();
        }

        public void Release(SpellSchool[] spellSchools)
        {
            foreach (SpellSchool spellSchool in spellSchools)
                this.m_SpellSchools.Remove(spellSchool);
            this.Update();
            this.TryRemovePart();
        }

        public void Release(SpellDescriptor spellDescriptor)
        {
            foreach (SpellDescriptor spellDescriptor1 in UnitPartBuffSuppressSaved.GetValues(spellDescriptor))
                this.m_SpellDescriptors.Remove(spellDescriptor1);
            this.Update();
            this.TryRemovePart();
        }

        public void Release(BlueprintBuff buff)
        {
            this.m_Buffs.Remove(buff);
            this.Update();
            this.TryRemovePart();
        }

        private void TryRemovePart()
        {
            if (this.m_Buffs.Any<BlueprintBuff>() || this.m_SpellDescriptors.Any<SpellDescriptor>() || this.m_SpellSchools.Any<SpellSchool>())
                return;
            this.Owner.Remove<UnitPartBuffSuppress>();
        }

        public bool IsSuppressed(Buff buff)
        {
            if (!this.m_Buffs.Contains(buff.Blueprint) && !UnitPartBuffSuppressSaved.GetValues(buff.Context.SpellDescriptor).Any<SpellDescriptor>((Func<SpellDescriptor, bool>)(d => this.m_SpellDescriptors.Contains(d))))
                return this.m_SpellSchools.Contains(buff.Context.SpellSchool);
            return true;
        }

        private void Update()
        {
            foreach (Buff buff in this.Owner.Buffs)
            {
                bool flag = this.IsSuppressed(buff);
                if (buff.IsSuppressed != flag)
                {
                    if (flag && buff.Active)
                        buff.Deactivate();
                    buff.IsSuppressed = flag;
                    if (!flag && !buff.Active)
                        buff.Activate();
                }
            }
        }
    }



    public class RemoveUniqueBuff : ContextAction
    {
        public BlueprintBuff buff;
        public override string GetCaption()
        {
            return "Remove unique buff";
        }


        public override void RunAction()
        {
            var unit_part_unique_buff = this.Target.Unit?.Get<UnitPartUniqueBuffs>();
            if (unit_part_unique_buff == null)
            {
                return;
            }
            var buff_to_remove = unit_part_unique_buff.Buffs.Find(b => b.Blueprint == buff);
            if (buff_to_remove != null)
            {
                unit_part_unique_buff.RemoveBuff(buff_to_remove);
                buff_to_remove.Remove();
            }
        }
    }


    [Harmony12.HarmonyPatch(typeof(Buff))]
    [Harmony12.HarmonyPatch("Activate", Harmony12.MethodType.Normal)]
    class Buff_Activate_Patch
    {
        static bool Prefix(Buff __instance)
        {
            var partBuffSuppress = __instance.Owner.Get<UnitPartBuffSuppressSaved>();
            __instance.IsSuppressed = partBuffSuppress != null && partBuffSuppress.IsSuppressed(__instance);
            if (__instance.IsSuppressed)
                return false;

            return true;
        }
    }
}
