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
using System.Linq;
using BestHTTP.Extensions;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Cabinet.Modules;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.Wearable;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chocopoi.DressingTools
{
    internal class DTEditorUtils
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;
        private const string BoneNameMappingsPath = "Packages/com.chocopoi.vrc.dressingtools/Resources/boneNameMappings.json";
        private const string PreviewAvatarNamePrefix = "DTPreview_";
        private const string ThumbnailRenderTextureGuid = "52c645a5f631e32439c8674dc6e491e2";
        private const string ThumbnailCameraLayer = "DT_Thumbnail";
        private const string ThumbnailCameraName = "DTTempThumbnailCamera";
        private const string ThumbnailWearableName = "DTTempThumbnailWearable";
        private const float ThumbnailCameraFov = 45.0f;
        private const int ThumbnailWidth = 128;
        private const int ThumbnailHeight = 128;
        private const int StartingUserLayer = 8;
        private const int MaxLayers = 32;

        private static RenderTexture s_thumbnailCameraRenderTexture = null;
        private static Dictionary<string, System.Type> s_reflectionTypeCache = new Dictionary<string, System.Type>();
        private static readonly System.Random Random = new System.Random();
        private static List<List<string>> s_boneNameMappings = null;

        //Reference: https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/#post-3416790
        public static void DrawHorizontalLine(int height = 1)
        {
            EditorGUILayout.Separator();
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Separator();
        }

        public static DTCabinet[] GetAllCabinets()
        {
            return Object.FindObjectsOfType<DTCabinet>();
        }

        public static DTWearable[] GetAllSceneWearables()
        {
            return Object.FindObjectsOfType<DTWearable>();
        }

        public static string GetGameObjectOriginalPrefabGuid(GameObject obj)
        {
            var assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj));
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            return guid;
        }

        public static DTCabinet GetAvatarCabinet(GameObject avatarGameObject, bool createIfNotExists = false)
        {
            if (avatarGameObject == null)
            {
                return null;
            }

            // loop through all cabinets and search
            var cabinets = GetAllCabinets();

            // no matter there are two occurance or not, we return the first found
            foreach (var cabinet in cabinets)
            {
                if (cabinet.AvatarGameObject == avatarGameObject)
                {
                    return cabinet;
                }
            }

            if (createIfNotExists)
            {
                // create new cabinet if not exist
                var comp = avatarGameObject.AddComponent<DTCabinet>();

                // TODO: read default config, scan for armature names?
                comp.AvatarGameObject = avatarGameObject;
                var config = new CabinetConfig();
                comp.configJson = config.Serialize();

                return comp;
            }

            return null;
        }

        public static DTWearable GetCabinetWearable(GameObject wearableGaneObject)
        {
            if (wearableGaneObject == null)
            {
                return null;
            }

            // loop through all scene wearables and search
            var wearables = GetAllSceneWearables();

            // no matter there are two occurance or not, we return the first found
            foreach (var sceneWearable in wearables)
            {
                if (sceneWearable.WearableGameObject == wearableGaneObject)
                {
                    return sceneWearable;
                }
            }

            return null;
        }

        public static DTWearable[] GetCabinetWearables(GameObject avatarGameObject)
        {
            if (avatarGameObject == null)
            {
                return new DTWearable[0];
            }
            return avatarGameObject.GetComponentsInChildren<DTWearable>();
        }

        public static void ApplyWearableTransforms(AvatarConfig avatarConfig, GameObject targetAvatar, GameObject targetWearable)
        {
            // check position delta and adjust
            {
                var wearableWorldPos = avatarConfig.worldPosition.ToVector3();
                if (targetWearable.transform.position - targetAvatar.transform.position != wearableWorldPos)
                {
                    Debug.LogFormat("[DressingTools] [AddCabinetWearable] Moved wearable world pos: {0}", wearableWorldPos.ToString());
                    targetWearable.transform.position += wearableWorldPos;
                }
            }

            // check rotation delta and adjust
            {
                var wearableWorldRot = avatarConfig.worldRotation.ToQuaternion();
                if (targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation) != wearableWorldRot)
                {
                    Debug.LogFormat("[DressingTools] [AddCabinetWearable] Moved wearable world rotation: {0}", wearableWorldRot.ToString());
                    targetWearable.transform.rotation *= wearableWorldRot;
                }
            }

            // apply avatar scale
            var lastAvatarParent = targetAvatar.transform.parent;
            var lastAvatarScale = Vector3.zero + targetAvatar.transform.localScale;
            if (lastAvatarParent != null)
            {
                // tricky workaround to apply lossy world scale is to unparent
                targetAvatar.transform.SetParent(null);
            }

            var avatarScaleVec = avatarConfig.avatarLossyScale.ToVector3();
            if (targetAvatar.transform.localScale != avatarScaleVec)
            {
                Debug.LogFormat("[DressingTools] [AddCabinetWearable] Adjusted avatar scale: {0}", avatarScaleVec.ToString());
                targetAvatar.transform.localScale = avatarScaleVec;
            }

            // apply wearable scale
            var lastWearableParent = targetWearable.transform.parent;
            var lastWearableScale = Vector3.zero + targetWearable.transform.localScale;
            if (lastWearableParent != null)
            {
                // tricky workaround to apply lossy world scale is to unparent
                targetWearable.transform.SetParent(null);
            }

            var wearableScaleVec = avatarConfig.wearableLossyScale.ToVector3();
            if (targetWearable.transform.localScale != wearableScaleVec)
            {
                Debug.LogFormat("[DressingTools] [AddCabinetWearable] Adjusted wearable scale: {0}", wearableScaleVec.ToString());
                targetWearable.transform.localScale = wearableScaleVec;
            }

            // restore avatar scale
            if (lastAvatarParent != null)
            {
                targetAvatar.transform.SetParent(lastAvatarParent);
            }
            targetAvatar.transform.localScale = lastAvatarScale;

            // restore wearable scale
            if (lastWearableParent != null)
            {
                targetWearable.transform.SetParent(lastWearableParent);
            }
            targetWearable.transform.localScale = lastWearableScale;
        }

        public static DTCabinet FindCabinetComponent(DTWearable wearable)
        {
            var p = wearable.transform.parent;
            DTCabinet cabinet = null;
            while (p != null)
            {
                if (p.TryGetComponent(out cabinet))
                {
                    break;
                }
                p = p.parent;
            }
            return cabinet;
        }

        public static bool AddCabinetWearable(CabinetConfig cabinetConfig, GameObject avatarGameObject, WearableConfig wearableConfig, GameObject wearableGameObject)
        {
            var cabinetWearable = GetCabinetWearable(wearableGameObject);

            // if not exist, create a new component
            if (cabinetWearable == null)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(wearableGameObject) && PrefabUtility.GetPrefabInstanceStatus(wearableGameObject) == PrefabInstanceStatus.NotAPrefab)
                {
                    // if not in scene, we instantiate it with a prefab connection
                    wearableGameObject = (GameObject)PrefabUtility.InstantiatePrefab(wearableGameObject);
                }

                // parent to avatar if haven't yet
                if (!IsGrandParent(avatarGameObject.transform, wearableGameObject.transform))
                {
                    wearableGameObject.transform.SetParent(avatarGameObject.transform);
                }

                // applying scalings
                ApplyWearableTransforms(wearableConfig.avatarConfig, avatarGameObject, wearableGameObject);

                // add cabinet wearable component
                cabinetWearable = wearableGameObject.AddComponent<DTWearable>();
                cabinetWearable.WearableGameObject = wearableGameObject;
            }

            wearableConfig.info.RefreshUpdatedTime();
            cabinetWearable.configJson = wearableConfig.Serialize();

            DoWearableModuleProviderCallbacks(wearableConfig.modules, (WearableModuleProviderBase provider, List<WearableModule> modules) =>
            {
                if (!provider.OnAddWearableToCabinet(cabinetConfig, avatarGameObject, wearableConfig, wearableGameObject, new ReadOnlyCollection<WearableModule>(modules)))
                {
                    Debug.LogWarning("[DressingTools] [AddCabinetWearable] Error processing provider OnAddWearableToCabinet hook: " + provider.ModuleIdentifier);
                    return false;
                }
                return true;
            });

            return true;
        }

        public static List<string> FindUnknownWearableModuleNames(List<WearableModule> modules)
        {
            var list = new List<string>();
            foreach (var module in modules)
            {
                var provider = WearableModuleProviderLocator.Instance.GetProvider(module.moduleName);
                if (provider == null)
                {
                    list.Add(module.moduleName);
                }
            }
            return list;
        }

        public static bool DoWearableModuleProviderCallbacks(List<WearableModule> modules, System.Func<WearableModuleProviderBase, List<WearableModule>, bool> callback)
        {
            var moduleByProvider = new Dictionary<WearableModuleProviderBase, List<WearableModule>>();

            // prepare module by provider
            foreach (var module in modules)
            {
                // locate the module provider
                var provider = WearableModuleProviderLocator.Instance.GetProvider(module.moduleName);

                if (provider == null)
                {
                    Debug.Log("[DressingTools] Missing wearable module provider detected: " + module.moduleName);
                    return false;
                }

                if (!moduleByProvider.TryGetValue(provider, out var groupedList))
                {
                    groupedList = new List<WearableModule>();
                    moduleByProvider[provider] = groupedList;
                }
                groupedList.Add(module);
            }

            // sort providers according to their call order
            var allProviders = WearableModuleProviderLocator.Instance.GetAllProviders();
            var sortedProviderList = new List<WearableModuleProviderBase>(allProviders);
            sortedProviderList.Sort((m1, m2) => m1.CallOrder.CompareTo(m2.CallOrder));

            // call provider callbacks
            foreach (var provider in allProviders)
            {
                if (!moduleByProvider.TryGetValue(provider, out var providerModules))
                {
                    // create a empty list
                    providerModules = new List<WearableModule>();
                }

                if (!callback.Invoke(provider, providerModules))
                {
                    return false;
                }
            }

            return true;
        }

        public static void RemoveCabinetWearable(DTCabinet cabinet, DTWearable wearable)
        {
            var cabinetWearables = cabinet.AvatarGameObject.GetComponentsInChildren<DTWearable>();
            foreach (var cabinetWearable in cabinetWearables)
            {
                if (cabinetWearable == wearable)
                {
                    if (!PrefabUtility.IsOutermostPrefabInstanceRoot(cabinetWearable.gameObject))
                    {
                        Debug.Log("Prefab is not outermost. Aborting");
                        return;
                    }
                    Object.DestroyImmediate(cabinetWearable.gameObject);
                    break;
                }
            }
        }

        public static void PrepareWearableConfig(WearableConfig wearableConfig, GameObject targetAvatar, GameObject targetWearable)
        {
            wearableConfig.version = WearableConfig.CurrentConfigVersion;

            AddWearableMetaInfo(wearableConfig, targetWearable);
            AddWearableTargetAvatarConfig(wearableConfig, targetAvatar, targetWearable);
        }

        public static void AddWearableTargetAvatarConfig(WearableConfig wearableConfig, GameObject targetAvatar, GameObject targetWearable)
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(targetAvatar);

            // try obtain armature name from cabinet
            if (cabinet == null)
            {
                // leave it empty
                wearableConfig.avatarConfig.armatureName = "";
            }
            else
            {
                if (CabinetConfig.TryDeserialize(cabinet.configJson, out var cabinetConfig))
                {
                    wearableConfig.avatarConfig.armatureName = cabinetConfig.avatarArmatureName;
                }
                else
                {
                    wearableConfig.avatarConfig.armatureName = "";
                }
            }

            // can't do anything
            if (targetAvatar == null || targetWearable == null)
            {
                return;
            }

            wearableConfig.avatarConfig.name = targetAvatar.name;

            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(targetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            wearableConfig.avatarConfig.guids.Clear();
            if (!invalidAvatarPrefabGuid)
            {
                // TODO: multiple guids
                wearableConfig.avatarConfig.guids.Add(avatarPrefabGuid);
            }

            var deltaPos = targetWearable.transform.position - targetAvatar.transform.position;
            var deltaRotation = targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation);
            wearableConfig.avatarConfig.worldPosition = new AvatarConfigVector3(deltaPos);
            wearableConfig.avatarConfig.worldRotation = new AvatarConfigQuaternion(deltaRotation);
            wearableConfig.avatarConfig.avatarLossyScale = new AvatarConfigVector3(targetAvatar.transform.lossyScale);
            wearableConfig.avatarConfig.wearableLossyScale = new AvatarConfigVector3(targetWearable.transform.lossyScale);
        }

        public static void AddWearableMetaInfo(WearableConfig config, GameObject targetWearable)
        {
            if (targetWearable == null)
            {
                return;
            }

            config.info.name = targetWearable.name;
            config.info.author = "";
            config.info.description = "";
        }

        public static System.Type FindType(string typeName)
        {
            // try getting from cache to avoid scanning the assemblies again
            if (s_reflectionTypeCache.ContainsKey(typeName))
            {
                return s_reflectionTypeCache[typeName];
            }

            // scan from assemblies and save to cache
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                {
                    s_reflectionTypeCache[typeName] = type;
                    return type;
                }
            }

            // no such type found
            return null;
        }

        public static void HandleBoneMappingOverrides(List<BoneMapping> generatedBoneMappings, List<BoneMapping> overrideBoneMappings)
        {
            foreach (var mappingOverride in overrideBoneMappings)
            {
                var matched = false;

                foreach (var originalMapping in generatedBoneMappings)
                {
                    // override on match
                    if (originalMapping.wearableBonePath == mappingOverride.wearableBonePath)
                    {
                        originalMapping.avatarBonePath = mappingOverride.avatarBonePath;
                        originalMapping.mappingType = mappingOverride.mappingType;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    // add mapping if not matched
                    generatedBoneMappings.Add(mappingOverride);
                }
            }
        }

        // TODO: copied from AvatarLib because of the Runtime/Editor assembly problem, create a Runtime one there soon?
        public static string GetRelativePath(Transform transform, Transform untilTransform = null, string prefix = "", string suffix = "")
        {
            string path = transform.name;
            while (true)
            {
                transform = transform.parent;

                if (transform.parent == null || (untilTransform != null && transform == untilTransform))
                {
                    break;
                }

                path = transform.name + "/" + path;
            }
            return prefix + path + suffix;
        }

        public static bool IsGrandParent(Transform grandParent, Transform grandChild)
        {
            var p = grandChild.parent;
            while (p != null)
            {
                if (p == grandParent)
                {
                    return true;
                }
                p = p.parent;
            }
            return false;
        }

        public static List<IDynamicsProxy> ScanDynamics(GameObject obj, bool doNotScanContainingWearables = false)
        {
            var dynamicsList = new List<IDynamicsProxy>();

            // TODO: replace by reading YAML

            // get the dynbone type
            var DynamicBoneType = FindType("DynamicBone");
            var PhysBoneType = FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

            // scan dynbones
            if (DynamicBoneType != null)
            {
                var dynBones = obj.GetComponentsInChildren(DynamicBoneType, true);
                foreach (var dynBone in dynBones)
                {
                    if (doNotScanContainingWearables && IsOriginatedFromAnyWearable(obj.transform, dynBone.transform))
                    {
                        continue;
                    }
                    dynamicsList.Add(new DynamicBoneProxy(dynBone));
                }
            }

            // scan physbones
            if (PhysBoneType != null)
            {
                var physBones = obj.GetComponentsInChildren(PhysBoneType, true);
                foreach (var physBone in physBones)
                {
                    if (doNotScanContainingWearables && IsOriginatedFromAnyWearable(obj.transform, physBone.transform))
                    {
                        continue;
                    }
                    dynamicsList.Add(new PhysBoneProxy(physBone));
                }
            }

            return dynamicsList;
        }

        public static IDynamicsProxy FindDynamicsWithRoot(List<IDynamicsProxy> avatarDynamics, Transform dynamicsRoot)
        {
            foreach (var bone in avatarDynamics)
            {
                if (bone.RootTransform == dynamicsRoot)
                {
                    return bone;
                }
            }
            return null;
        }

        public static bool IsDynamicsExists(List<IDynamicsProxy> avatarDynamics, Transform dynamicsRoot)
        {
            return FindDynamicsWithRoot(avatarDynamics, dynamicsRoot) != null;
        }

        private static void LoadBoneNameMappings()
        {
            try
            {
                var reader = new StreamReader(BoneNameMappingsPath);
                var json = reader.ReadToEnd();
                reader.Close();
                s_boneNameMappings = JsonConvert.DeserializeObject<List<List<string>>>(json);
            }
            catch (IOException e)
            {
                Debug.LogError(e);
            }
        }

        public static Transform GuessMatchingAvatarBone(Transform avatarBoneParent, string childBoneName)
        {
            // load bone name mappings if needed
            if (s_boneNameMappings == null)
            {
                LoadBoneNameMappings();
            }

            // trim the string
            childBoneName = childBoneName.Trim();

            // check if there is a prefix
            if (childBoneName.StartsWith("("))
            {
                //find the first closing bracket
                var prefixBracketEnd = childBoneName.IndexOf(")");
                if (prefixBracketEnd != -1 && prefixBracketEnd != childBoneName.Length - 1) //remove it if there is
                {
                    childBoneName = childBoneName.Substring(prefixBracketEnd + 1).Trim();
                }
            }

            // check if there is a suffix
            if (childBoneName.EndsWith(")"))
            {
                //find the first closing bracket
                var suffixBracketStart = childBoneName.LastIndexOf("(");
                if (suffixBracketStart != -1 && suffixBracketStart != 0) //remove it if there is
                {
                    childBoneName = childBoneName.Substring(0, suffixBracketStart).Trim();
                }
            }

            var exactMatchBoneTransform = avatarBoneParent.Find(childBoneName);
            if (exactMatchBoneTransform != null)
            {
                // exact match
                return exactMatchBoneTransform;
            }

            // try match it via the mapping list
            foreach (var boneNames in s_boneNameMappings)
            {
                if (boneNames.Contains(childBoneName))
                {
                    foreach (var boneName in boneNames)
                    {
                        var remappedBoneTransform = avatarBoneParent.Find(boneName);
                        if (remappedBoneTransform != null)
                        {
                            // found alternative bone name
                            return remappedBoneTransform;
                        }
                    }
                }
            }

            // match failure
            return null;
        }

        public static Transform GuessArmature(GameObject targetClothes, string armatureObjectName, bool rename = false)
        {
            var transforms = new List<Transform>();

            for (var i = 0; i < targetClothes.transform.childCount; i++)
            {
                var child = targetClothes.transform.GetChild(i);

                if (child.name.ToLower().Trim().Contains(armatureObjectName.ToLower()))
                {
                    transforms.Add(child);
                }
            }

            if (transforms.Count == 1)
            {
                if (rename)
                {
                    transforms[0].name = armatureObjectName;
                }
                return transforms[0];
            }
            else
            {
                return null;
            }
        }

        // referenced from: http://answers.unity3d.com/questions/458207/copy-a-component-at-runtime.html
        public static Component CopyComponent(Component originalComponent, GameObject destGameObject)
        {
            System.Type type = originalComponent.GetType();

            // get the destination component or add new
            var destComp = destGameObject.AddComponent(type);

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.IsStatic) continue;
                field.SetValue(destComp, field.GetValue(originalComponent));
            }

            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite || prop.Name == "name")
                {
                    continue;
                }
                prop.SetValue(destComp, prop.GetValue(originalComponent, null), null);
            }

            return destComp;
        }

        public static T FindWearableModuleConfig<T>(WearableConfig config) where T : IModuleConfig
        {
            foreach (var module in config.modules)
            {
                if (module.config is T moduleConfig)
                {
                    return moduleConfig;
                }
            }
            return default;
        }

        public static WearableModule FindWearableModule(WearableConfig config, string moduleName)
        {
            foreach (var module in config.modules)
            {
                if (moduleName == module.moduleName)
                {
                    return module;
                }
            }
            return null;
        }

        public static T FindCabinetModuleConfig<T>(CabinetConfig config) where T : IModuleConfig
        {
            foreach (var module in config.modules)
            {
                if (module.config is T moduleConfig)
                {
                    return moduleConfig;
                }
            }
            return default;
        }

        public static CabinetModule FindCabinetModule(CabinetConfig config, string moduleName)
        {
            foreach (var module in config.modules)
            {
                if (moduleName == module.moduleName)
                {
                    return module;
                }
            }
            return null;
        }

        public static bool IsOriginatedFromAnyWearable(Transform root, Transform transform)
        {
            var found = false;
            while (transform != null)
            {
                transform = transform.parent;
                if (transform == root || transform == null)
                {
                    break;
                }

                if (transform.TryGetComponent<DTWearable>(out var _))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        public static string RandomString(int length)
        {
            // i just copied from stackoverflow :D
            // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings?page=1&tab=scoredesc#tab-top
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static bool PreviewActive { get; private set; }

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

        public static void UpdatePreviewAvatar(GameObject targetAvatar, WearableConfig newWearableConfig, GameObject newWearable)
        {
            PreviewAvatar(targetAvatar, newWearable, out var previewAvatar, out var previewWearable);

            if (previewAvatar == null || previewWearable == null)
            {
                return;
            }

            var cabinet = GetAvatarCabinet(previewAvatar);

            if (cabinet == null)
            {
                return;
            }

            if (!CabinetConfig.TryDeserialize(cabinet.configJson, out var cabinetConfig))
            {
                Debug.LogError("[DressingTools] Unable to deserialize cabinet config for preview");
                return;
            }

            var report = new DTReport();
            var cabCtx = new ApplyCabinetContext()
            {
                report = report,
                cabinetConfig = cabinetConfig,
                avatarGameObject = previewAvatar,
                avatarDynamics = ScanDynamics(previewAvatar, true),
                wearableContexts = new Dictionary<DTWearable, ApplyWearableContext>()
            };

            var wearCtx = new ApplyWearableContext()
            {
                wearableConfig = newWearableConfig,
                wearableGameObject = previewWearable,
                wearableDynamics = ScanDynamics(previewWearable)
            };

            DoWearableModuleProviderCallbacks(newWearableConfig.modules, (WearableModuleProviderBase provider, List<WearableModule> modules) =>
            {
                if (!provider.OnPreviewWearable(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(modules)))
                {
                    Debug.LogError("[DressingTools] Error applying wearable in preview!");
                    return false;
                }
                return true;
            });
        }

        public static void PreviewAvatar(GameObject targetAvatar, GameObject targetWearable, out GameObject previewAvatar, out GameObject previewWearable)
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
            var path = IsGrandParent(targetAvatar.transform, targetWearable.transform) ?
                AnimationUtils.GetRelativePath(targetWearable.transform, targetAvatar.transform) :
                targetWearable.name;

            // return existing preview object if any
            if (previewAvatar != null)
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
            if (!IsGrandParent(targetAvatar.transform, targetWearable.transform))
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

        public static T CopyAssetToPathAndImport<T>(Object assetObj, string copiedPath) where T : Object
        {
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(assetObj), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);
            return AssetDatabase.LoadAssetAtPath<T>(copiedPath);
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

        public static string GetBase64FromTexture(Texture2D texture)
        {
            var buffer = texture.EncodeToPNG();

            var decompressedMs = new MemoryStream();
            using (var gzipStream = new GZipStream(decompressedMs, CompressionMode.Compress, true))
            {
                gzipStream.Write(buffer, 0, buffer.Length);
            }

            decompressedMs.Position = 0;
            var compressedBuffer = new byte[decompressedMs.Length];
            decompressedMs.Read(compressedBuffer, 0, compressedBuffer.Length);

            return System.Convert.ToBase64String(compressedBuffer);
        }

        public static Texture2D GetTextureFromBase64(string b64)
        {
            var compressedBuffer = System.Convert.FromBase64String(b64);

            var compressedMs = new MemoryStream();
            compressedMs.Write(compressedBuffer, 0, compressedBuffer.Length);

            byte[] decompressedBuffer;
            compressedMs.Position = 0;
            using (var gzipStream = new GZipStream(compressedMs, CompressionMode.Decompress, true))
            {
                using (var readerMs = new MemoryStream())
                {
                    gzipStream.CopyTo(readerMs);
                    decompressedBuffer = readerMs.ToArray();
                }
            }

            var texture = new Texture2D(ThumbnailWidth, ThumbnailHeight);
            texture.LoadImage(decompressedBuffer);
            return texture;
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
    }
}
