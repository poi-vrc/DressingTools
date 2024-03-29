/*
 * File: DTEditorUtils.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 1:22:09 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Components;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Components.Generic;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    internal class DTEditorUtils
    {
        private static RenderTexture s_thumbnailCameraRenderTexture = null;
        private static readonly System.Random Random = new System.Random();

        public static bool PreviewActive { get; private set; }

        public const int ThumbnailWidth = 128;
        public const int ThumbnailHeight = 128;

        private const string PreviewAvatarNamePrefix = "DTPreview_";
        private const string ThumbnailRenderTextureGuid = "52c645a5f631e32439c8674dc6e491e2";
        private const string ThumbnailCameraLayer = "DT_Thumbnail";
        private const string ThumbnailCameraName = "DTTempThumbnailCamera";
        private const string ThumbnailWearableName = "DTTempThumbnailWearable";
        private const float ThumbnailCameraFov = 45.0f;
        private const int StartingUserLayer = 8;
        private const int MaxLayers = 32;

        public static void CleanUpPreviewAvatars()
        {
            PreviewActive = false;
            // remove all existing preview objects;
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj != null && obj.name.StartsWith(PreviewAvatarNamePrefix))
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }

        private static WearableModuleProvider GetWearableModuleProviderByIdentifier(List<WearableModuleProvider> providers, string identifier)
        {
            foreach (var provider in providers)
            {
                if (provider.Identifier == identifier)
                {
                    return provider;
                }
            }
            return null;
        }

        public static void UpdatePreviewAvatar(GameObject targetAvatar, WearableConfig newWearableConfig, GameObject newWearable, bool forceRecreate = false)
        {
            PreviewAvatar(targetAvatar, newWearable, out var previewAvatar, out var previewWearable, forceRecreate);

            if (previewAvatar == null || previewWearable == null)
            {
                return;
            }

            var cabinet = OneConfUtils.GetAvatarCabinet(targetAvatar);

            if (cabinet == null)
            {
                return;
            }

            if (!CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out var cabinetConfig))
            {
                Debug.LogError("[DressingTools] Unable to deserialize cabinet config for preview");
                return;
            }

            var dkCtx = new DKNativeContext(previewAvatar);
            var cabCtx = new CabinetContext
            {
                dkCtx = dkCtx,
                cabinetConfig = cabinetConfig
            };

            var wearCtx = new WearableContext
            {
                wearableConfig = newWearableConfig,
                wearableGameObject = previewWearable,
                wearableDynamics = DynamicsUtils.ScanDynamics(previewWearable)
            };

            var providers = ModuleManager.Instance.GetAllWearableModuleProviders();

            foreach (var provider in providers)
            {
                if (!provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(newWearableConfig.FindModules(provider.Identifier)), true))
                {
                    Debug.LogError("[DressingTools] Error applying wearable in preview!");
                    break;
                }
            }

            // remove all DK components
            var dkComps = previewAvatar.GetComponentsInChildren<DKBaseComponent>();
            foreach (var comp in dkComps)
            {
                Object.DestroyImmediate(comp);
            }
        }

        public static void PreviewAvatar(GameObject targetAvatar, GameObject targetWearable, out GameObject previewAvatar, out GameObject previewWearable, bool forceRecreate = false)
        {
            if (targetAvatar == null || targetWearable == null)
            {
                CleanUpPreviewAvatars();
                PreviewActive = false;
                previewAvatar = null;
                previewWearable = null;
                return;
            }

            var objName = PreviewAvatarNamePrefix + targetAvatar.name;
            previewAvatar = GameObject.Find(objName);

            // find path of wearable
            var path = DKEditorUtils.IsGrandParent(targetAvatar.transform, targetWearable.transform) ?
                AnimationUtils.GetRelativePath(targetWearable.transform, targetAvatar.transform) :
                targetWearable.name;

            // return existing preview object if any
            if (previewAvatar != null && !forceRecreate)
            {
                var wearableTransform = previewAvatar.transform.Find(path);

                // valid preview
                if (wearableTransform != null)
                {
                    PreviewActive = true;
                    previewWearable = wearableTransform.gameObject;
                    return;
                }

                // recreate the preview
            }

            // clean up and recreate
            CleanUpPreviewAvatars();

            // create a copy of the avatar and wearable
            previewAvatar = Object.Instantiate(targetAvatar);
            previewAvatar.name = objName;

            var newPos = previewAvatar.transform.position;
            newPos.x -= 20;
            previewAvatar.transform.position = newPos;

            // if wearable is not inside avatar, we instantiate a new copy
            if (!DKEditorUtils.IsGrandParent(targetAvatar.transform, targetWearable.transform))
            {
                previewWearable = Object.Instantiate(targetWearable);
                previewWearable.transform.position = newPos;
                previewWearable.transform.SetParent(previewAvatar.transform);
            }
            else
            {
                previewWearable = previewAvatar.transform.Find(path).gameObject;
            }

            // select in sceneview
            FocusGameObjectInSceneView(previewAvatar);

            PreviewActive = true;
        }

        public static void FocusGameObjectInSceneView(GameObject go)
        {
            Selection.activeGameObject = go;
            SceneView.FrameLastActiveSceneView();
        }

        public static Texture2D GetThumbnailCameraPreview()
        {
            if (s_thumbnailCameraRenderTexture == null)
            {
                s_thumbnailCameraRenderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(AssetDatabase.GUIDToAssetPath(ThumbnailRenderTextureGuid));
            }

            Texture2D tex = new Texture2D(ThumbnailWidth, ThumbnailHeight, TextureFormat.ARGB32, false);
            RenderTexture.active = s_thumbnailCameraRenderTexture;
            tex.ReadPixels(new Rect(0, 0, s_thumbnailCameraRenderTexture.width, s_thumbnailCameraRenderTexture.height), 0, 0);
            tex.Apply();
            return tex;
        }

        public static void PrepareWearableThumbnailCamera(GameObject targetWearable, bool wearableOnly, bool removeBackground, bool focus = true, System.Action positionUpdate = null)
        {
            CleanUpThumbnailObjects();

            // render texture
            var renderTexturePath = AssetDatabase.GUIDToAssetPath(ThumbnailRenderTextureGuid);
            var renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(renderTexturePath);

            var addedLayer = PrepareWearableThumbnailCameraLayer();
            if (!addedLayer)
            {
                Debug.LogWarning("[DressingTools] Unable to add thumbnail camera layer, maybe it's full? Now camera will not make wearable visible only");
            }

            // prepare clone
            if (wearableOnly)
            {
                targetWearable = Object.Instantiate(targetWearable);
                targetWearable.name = ThumbnailWearableName;
                var newClonePos = targetWearable.transform.position;
                newClonePos.x -= 30.0f;
                targetWearable.transform.position = newClonePos;
            }

            // create new camera object
            var cameraObj = new GameObject(ThumbnailCameraName);
            var newCamPos = targetWearable.transform.position;
            newCamPos.y += 0.8f;
            newCamPos.z += 1.3f;
            cameraObj.transform.SetPositionAndRotation(newCamPos, Quaternion.Euler(0, 180, 0));

            // prepare camera
            var camera = cameraObj.AddComponent<Camera>();
            camera.targetTexture = renderTexture;
            if (removeBackground)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0, 0, 0, 0);
            }
            camera.fieldOfView = ThumbnailCameraFov;
            if (wearableOnly && addedLayer)
            {
                camera.cullingMask = LayerMask.GetMask(ThumbnailCameraLayer);
                RecursiveSetLayer(targetWearable, LayerMask.NameToLayer(ThumbnailCameraLayer));
            }

            if (focus)
            {
                SceneView.lastActiveSceneView.LookAt(newCamPos, Quaternion.Euler(0, 180, 0));
                SceneView.lastActiveSceneView.AlignViewToObject(cameraObj.transform);
                var follower = cameraObj.AddComponent<SceneViewFollower>();
                if (positionUpdate != null)
                {
                    follower.PositionUpdate += positionUpdate;
                }
            }
        }

        private static void RecursiveSetLayer(GameObject obj, int layerIndex)
        {
            // only change if layer is default
            if (obj.layer == 0)
            {
                obj.layer = layerIndex;
            }

            for (var i = 0; i < obj.transform.childCount; i++)
            {
                var child = obj.transform.GetChild(i);
                RecursiveSetLayer(child.gameObject, layerIndex);
            }
        }

        public static void CleanUpThumbnailObjects()
        {
            // remove existing camera
            var existingCamObj = GameObject.Find(ThumbnailCameraName);
            if (existingCamObj != null)
            {
                Object.DestroyImmediate(existingCamObj);
            }

            // remove existing dummy
            var existingDummy = GameObject.Find(ThumbnailWearableName);
            if (existingDummy != null)
            {
                Object.DestroyImmediate(existingDummy);
            }
        }

        // referenced from: https://forum.unity.com/threads/create-tags-and-layers-in-the-editor-using-script-both-edit-and-runtime-modes.732119/
        public static bool PrepareWearableThumbnailCameraLayer()
        {
            if (!HasCullingLayer(ThumbnailCameraLayer))
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
                var so = new SerializedObject(assets);
                var layers = so.FindProperty("layers");

                // we want to be as latest as possible (because we are just a fancy feature)
                for (var i = MaxLayers - 1; i >= StartingUserLayer; i--)
                {
                    var layer = layers.GetArrayElementAtIndex(i);
                    if (layer.stringValue == "")
                    {
                        layer.stringValue = ThumbnailCameraLayer;
                        so.ApplyModifiedPropertiesWithoutUndo();
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool CleanUpCullingLayers()
        {
            if (HasCullingLayer(ThumbnailCameraLayer))
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
                var so = new SerializedObject(assets);
                var layers = so.FindProperty("layers");
                for (var i = StartingUserLayer; i < MaxLayers; i++)
                {
                    var layer = layers.GetArrayElementAtIndex(i);
                    if (layer.stringValue == ThumbnailCameraLayer)
                    {
                        so.ApplyModifiedPropertiesWithoutUndo();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasCullingLayer(string layerName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            var so = new SerializedObject(assets);
            var layers = so.FindProperty("layers");
            for (var i = 0; i < MaxLayers; i++)
            {
                var elem = layers.GetArrayElementAtIndex(i);
                if (elem.stringValue.Equals(layerName))
                {
                    return true;
                }
            }
            return false;
        }

        public static AnimationClip CopyClip(AnimationClip oldClip)
        {
            var newClip = new AnimationClip()
            {
                name = oldClip.name,
                legacy = oldClip.legacy,
                frameRate = oldClip.frameRate,
                localBounds = oldClip.localBounds,
                wrapMode = oldClip.wrapMode
            };
            AnimationUtility.SetAnimationClipSettings(newClip, AnimationUtility.GetAnimationClipSettings(oldClip));

            var curveBindings = AnimationUtility.GetCurveBindings(oldClip);
            foreach (var curveBinding in curveBindings)
            {
                newClip.SetCurve(curveBinding.path, curveBinding.type, curveBinding.propertyName, AnimationUtility.GetEditorCurve(oldClip, curveBinding));
            }

            var objRefBindings = AnimationUtility.GetObjectReferenceCurveBindings(oldClip);
            foreach (var objRefBinding in objRefBindings)
            {
                AnimationUtility.SetObjectReferenceCurve(newClip, objRefBinding, AnimationUtility.GetObjectReferenceCurve(oldClip, objRefBinding));
            }

            return newClip;
        }
    }
}
