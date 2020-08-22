using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using static Kingmaker.Visual.CharacterSystem.BakedCharacter;

namespace CallOfTheWild.UnitViewMechanics
{

    [Harmony12.HarmonyPatch(typeof(DollRoom), "CreateAvatar")]
    class DollRoom_CreateAvatar_Patch
    {
        static bool Prefix(Character originalAvatar, string dollName, DollRoom __instance, ref Character __result, Transform ___m_CharacterPlaceholder)
        {
            try
            {
                Character character = new GameObject(string.Format("Doll [{0}]", dollName)).AddComponent<Character>();
                character.transform.localScale = originalAvatar.transform.localScale;
                character.IsInDollRoom = true;
                character.AnimatorPrefab = originalAvatar.AnimatorPrefab;
                character.Skeleton = originalAvatar.Skeleton;
                character.AnimationSet = originalAvatar.AnimationSet;
                Action<Character> callback = (Character c) => AccessTools.Method(__instance.GetType(), "OnCharacterUpdated").Invoke(__instance, new object[] { c });
                character.OnUpdated += callback;
                //Copy BakedCharacter
                character.BakedCharacter = originalAvatar.BakedCharacter;                
                character.CopyEquipmentFrom(originalAvatar);

                var inventory_scale = __instance?.Unit?.Blueprint?.GetComponent<ChangeUnitScaleForInventory>();
                if (inventory_scale != null)
                {
                    character.transform.localScale = originalAvatar.transform.localScale * inventory_scale.scale_factor;
                }
                character.Start();
                character.Animator.gameObject.AddComponent<UnitAnimationCallbackReceiver>();

                character.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                if (character.AnimationManager)
                {
                    character.AnimationManager.PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);
                    character.AnimationManager.IsInCombat = true;
                    character.AnimationManager.Tick();
                }
                character.transform.SetParent(___m_CharacterPlaceholder, false);
                character.Mirror = originalAvatar.Mirror;
                character.transform.localRotation = Quaternion.Euler(0f, (float)((!originalAvatar.Mirror) ? -45 : 45), 0f);
                __result = character;
            }
            catch (Exception ex)
            {
                Main.logger.Log(ex.ToString());
            }
            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(EntityDataBase), "AttachToViewOnLoad")]
    class EntityDataBase_AttachToViewOnLoad_Patch
    {
        static void Postfix(EntityDataBase __instance, EntityViewBase view)
        {
            if (view != null)
            {
                return;
            }

            var unit_entity_data = __instance as UnitEntityData;
            if (unit_entity_data == null)
            {
                return;
            }
            if (unit_entity_data.GetActivePolymorph() != null)
            {
                return;
            }
            var replace_view = unit_entity_data.Descriptor?.Get<UnitPartViewReplacement>()?.buff?.Blueprint.GetComponent<ReplaceUnitView>();
            if (replace_view != null)
            {
                Helpers.TryReplaceView(unit_entity_data.Descriptor, replace_view.prefab, replace_view.use_master_view);
            }
        }
    }


    public class UnitPartViewReplacement : UnitPart
    {
        [JsonProperty]
        public Fact buff;
    }

    public class ReplaceUnitView : OwnedGameLogicComponent<UnitDescriptor>, IAreaLoadingStagesHandler
    {
        public UnitViewLink prefab;
        public bool use_master_view = false;
        public void OnAreaLoadingComplete()
        {
            if (this.Owner.Unit.GetActivePolymorph() != null)
            {
                return;
            }
            this.OnFactActivate();
        }

        public void OnAreaScenesLoaded()
        {

        }

        public override void OnFactActivate()
        {
            //Main.logger.Log("Activate");
            this.Owner.Ensure<UnitPartViewReplacement>().buff = this.Fact;
            Helpers.TryReplaceView(this.Owner, prefab, use_master_view);
        }


        public override void OnFactDeactivate()
        {
            /*Main.logger.Log("Deactivate");
            this.Owner.Ensure<UnitPartViewReplacement>().buff = null;
            foreach (Buff buff in this.Owner.Buffs)
                buff.ClearParticleEffect();
            UnitEntityView view = this.Owner.Unit.View;
            this.Owner.Unit.AttachToViewOnLoad((EntityViewBase)null);
            this.Owner.Unit.View.transform.SetParent(view.transform.parent, false);
            this.Owner.Unit.View.transform.position = view.transform.position;
            this.Owner.Unit.View.transform.rotation = view.transform.rotation;*/
        }
    }


    public class ChangeUnitScaleForInventory : BlueprintComponent
    {
        public float scale_factor;

    }



    class Helpers
    {
        static public void TryReplaceView(UnitDescriptor Owner, UnitViewLink Prefab, bool use_master_view)
        {
            if (!Owner.Unit.View)
            {
                return;
            }
            UnitEntityView unitEntityView = null;
            if (!use_master_view)
            {
                unitEntityView = Prefab.Load(true);
            }
            else
            {//copy view from master
                var master = Owner.Unit.Descriptor.Master.Value;
                unitEntityView = master?.Descriptor.Doll.CreateUnitView(false);
                //Main.logger.Log("View: " + (unitEntityView != null).ToString());
                if (master != null)
                {
                    if (master?.UISettings.PortraitBlueprint != null)
                        Owner.Unit.Descriptor.UISettings.SetPortrait(master.UISettings.PortraitBlueprint);
                    else
                        Owner.Unit.Descriptor.UISettings.SetPortrait(master.UISettings.Portrait);
                    Owner.Unit.Descriptor.CustomPrefabGuid = master.Descriptor.CustomPrefabGuid;
                    Owner.Unit.Descriptor.Doll = master.Descriptor.Doll;
                    Owner.Unit.Descriptor.ForcceUseClassEquipment = true;
                    Owner.Unit.Descriptor.CustomGender = master.Descriptor.Gender;
                    Owner.Unit.Descriptor.CustomAsks = master.Descriptor.Asks;                   
                }
            }
            if (unitEntityView == null)
            {
                return;
            }
            foreach (Buff buff in Owner.Buffs)
            {
                buff.ClearParticleEffect();
            }
            UnitEntityView oldView = Owner.Unit.View;
            UnitEntityView newView = null;
            if (!use_master_view)
            {
                newView = UnityEngine.Object.Instantiate(unitEntityView).GetComponent<UnitEntityView>();
                newView.UniqueId = Owner.Unit.UniqueId;
            }
            else
            {
                newView = unitEntityView;
                //newView = UnityEngine.Object.Instantiate(unitEntityView).GetComponent<UnitEntityView>();
                newView.UniqueId = Owner.Unit.UniqueId;
                //newView = unitEntityView;
            }
            newView.transform.SetParent(oldView.transform.parent, false);
            newView.transform.position = oldView.transform.position;
            newView.transform.rotation = oldView.transform.rotation;
            newView.DisableSizeScaling = false;
            newView.Blueprint = Owner.Blueprint;
            if (!use_master_view)
            {
                var character = newView.GetComponent<Character>();
                if (character == null)
                {
                    character = newView.gameObject.AddComponent<Character>();
                    character.AnimatorPrefab = BlueprintRoot.Instance.CharGen.FemaleDoll.AnimatorPrefab;
                    character.BakedCharacter = CreateBakedCharacter(newView.gameObject);
                    //BlueprintRoot.Instance.CharGen.FemaleDoll
                    /*var drow = ResourcesLibrary.TryGetResource<UnitEntityView>("a65d9da806faa8f4ca078dfe942bf458", true);
                    CloneMonobehaviour(drow.GetComponentInChildren<Character>(), character);
                    character.BakedCharacter = CreateBakedCharacter(newView.gameObject);*/
                }
            }
            Owner.Unit.AttachToViewOnLoad(newView);
            Owner.Unit.Commands.InterruptAll((UnitCommand cmd) => !(cmd is UnitMoveTo));
            var selectionManager = Game.Instance.UI.SelectionManager;
            if (selectionManager != null)
            {
                selectionManager.ForceCreateMarks();
            }

            if (use_master_view)
            {
                var master = Owner.Unit.Descriptor.Master.Value;
                int? clothesPrimaryIndex = master.Descriptor.Doll?.ClothesPrimaryIndex;
                int primaryRampIndex = !clothesPrimaryIndex.HasValue ? -1 : clothesPrimaryIndex.Value;
                int? clothesSecondaryIndex = master.Descriptor.Doll?.ClothesSecondaryIndex;
                int secondaryRampIndex = !clothesSecondaryIndex.HasValue ? -1 : clothesSecondaryIndex.Value;
                var equipment_class = CallOfTheWild.Helpers.GetField<BlueprintCharacterClass>(master.View, "m_EquipmentClass");
                foreach (EquipmentEntity loadClothe in equipment_class.LoadClothes(master.Descriptor.Gender, master.Descriptor.Progression.Race))
                {
                    unitEntityView.CharacterAvatar.AddEquipmentEntity(loadClothe, false);
                    if (primaryRampIndex >= 0)
                        unitEntityView.CharacterAvatar.SetPrimaryRampIndex(loadClothe, primaryRampIndex, false);
                    if (secondaryRampIndex >= 0)
                        unitEntityView.CharacterAvatar.SetSecondaryRampIndex(loadClothe, secondaryRampIndex, false);
                }
                //unitEntityView.UpdateClassEquipment();
            }
            UnityEngine.Object.Destroy(oldView.gameObject);
        }


        static public void maybeAddCharacter(UnitEntityView original)
        {
            var character = original.GetComponent<Character>();
            if (character == null)
            {
                character = original.gameObject.AddComponent<Character>();
                //BlueprintRoot.Instance.CharGen.FemaleDoll
                var drow = ResourcesLibrary.TryGetResource<UnitEntityView>("a65d9da806faa8f4ca078dfe942bf458", true);
                CloneMonobehaviour(drow.GetComponentInChildren<Character>(), character);
                character.BakedCharacter = CreateBakedCharacter(original.gameObject);
            }
        }



        static BakedCharacter CreateBakedCharacter(GameObject source)
        {
            if (source == null)
            {
                Main.logger.Log("CreateBakedCharacter failed. Source is null");
                return null;
            }
            var renderers = source.GetComponentsInChildren<SkinnedMeshRenderer>();
            var descriptions = new List<RendererDescription>();
            foreach (var renderer in renderers)
            {
                var desc = new RendererDescription();
                desc.Mesh = renderer.sharedMesh;
                var bonesNames = renderer.bones.Select(t => t.name).ToArray();
                desc.Textures = new List<CharacterTextureDescription>()
                    {
                        new CharacterTextureDescription(CharacterTextureChannel.Diffuse, renderer.material.mainTexture as Texture2D)
                    };
                desc.Material = renderer.sharedMaterial;
                desc.RootBone = "Pelvis";
                desc.Bones = bonesNames;
                desc.Name = renderer.name;
                descriptions.Add(desc);
            }
            var bakedCharacter = ScriptableObject.CreateInstance<BakedCharacter>();
            bakedCharacter.RendererDescriptions = descriptions;
            bakedCharacter.name = source.name + "BakedCharacter";
            return bakedCharacter;
        }


        static void CloneMonobehaviour(MonoBehaviour source, MonoBehaviour target)
        {
            var type = source.GetType();
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                field.SetValue(target, field.GetValue(source));
            }
        }
    }
}
