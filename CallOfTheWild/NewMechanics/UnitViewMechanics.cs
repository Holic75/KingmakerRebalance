using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
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

/*namespace CallOfTheWild.UnitViewMechanics
{

    public class UnitPartReplaceView : UnitPart
    {
        public BlueprintFact buff;
    }

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
                Main.DebugError(ex);
            }
            return false;
        }
    }


    [Harmony12.HarmonyPatch(typeof(UnitEntityView), "CreateView")]
    class UnitEntityView_CreateView_Patch
    {
        static bool Prefix(Character originalAvatar, string dollName, DollRoom __instance, ref Character __result, Transform ___m_CharacterPlaceholder)
        {
        }
    }


    public class ReplaceUnitViewOnApply: OwnedGameLogicComponent<UnitDescriptor>
    {
        public UnitViewLink prefab;

        public override void OnFactActivate()
        {
           this.Owner.Ensure<UnitPartReplaceView>().buff = this.Fact.Blueprint;
           Helpers.TryReplaceView(this.Owner, prefab);
        }


        public override void OnFactDeactivate()
        {
            this.Owner.Ensure<UnitPartReplaceView>().buff = null;
        }
    }



    class Helpers
    {
        static public UnitEntityView TryReplaceView(UnitDescriptor Owner, UnitViewLink Prefab)
        {
            if (!Owner.Unit.View)
            {
                Main.DebugLog("No View");
                return null;
            }
            UnitEntityView unitEntityView = Prefab.Load(true);
            if (unitEntityView == null)
            {
                Main.DebugLog("Could not load prefab");
                return null;
            }
            foreach (Buff buff in Owner.Buffs)
            {
                buff.ClearParticleEffect();
            }
            UnitEntityView oldView = Owner.Unit.View;
            UnitEntityView newView = UnityEngine.Object.Instantiate(unitEntityView).GetComponent<UnitEntityView>();
            newView.UniqueId = Owner.Unit.UniqueId;
            newView.transform.SetParent(oldView.transform.parent, false);
            newView.transform.position = oldView.transform.position;
            newView.transform.rotation = oldView.transform.rotation;
            newView.DisableSizeScaling = true;
            newView.Blueprint = Owner.Blueprint;
            var character = newView.GetComponent<Character>();
            if (character == null)
            {
                character = newView.gameObject.AddComponent<Character>();
                //BlueprintRoot.Instance.CharGen.FemaleDoll
                var drow = ResourcesLibrary.TryGetResource<UnitEntityView>("a65d9da806faa8f4ca078dfe942bf458", true);
                CloneMonobehaviour(drow.GetComponentInChildren<Character>(), character);
                character.BakedCharacter = CreateBakedCharacter(newView.gameObject);
            }
            Owner.Unit.AttachToViewOnLoad(newView);
            Owner.Unit.Commands.InterruptAll((UnitCommand cmd) => !(cmd is UnitMoveTo));
            var selectionManager = Game.Instance.UI.SelectionManager;
            if (selectionManager != null)
            {
                selectionManager.ForceCreateMarks();
            }

            UnityEngine.Object.Destroy(oldView.gameObject);
            return newView;
        }


        static BakedCharacter CreateBakedCharacter(GameObject source)
        {
            if (source == null)
            {
                Main.DebugLog("CreateBakedCharacter failed. Source is null");
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
}*/
