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
using System.IO;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Cabinet.Modules;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Proxy;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    internal class DTEditorUtils
    {
        private const string BoneNameMappingsPath = "Packages/com.chocopoi.vrc.dressingtools/Resources/boneNameMappings.json";

        private static Dictionary<string, System.Type> s_reflectionTypeCache = new Dictionary<string, System.Type>();

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

        public static string GetGameObjectOriginalPrefabGuid(GameObject obj)
        {
            var assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj));
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            return guid;
        }

        public static DTCabinet GetAvatarCabinet(GameObject avatar, bool createIfNotExists = false)
        {
            if (avatar == null)
            {
                return null;
            }

            var comp = avatar.GetComponent<DTCabinet>();

            if (comp == null && createIfNotExists)
            {
                // create new cabinet if not exist
                comp = avatar.AddComponent<DTCabinet>();

                // TODO: read default config, scan for armature names?
                comp.avatarGameObject = avatar;
                var config = new CabinetConfig();
                comp.configJson = config.ToString();
            }

            return comp;
        }

        public static DTCabinetWearable[] GetCabinetWearables(GameObject avatarGameObject)
        {
            if (avatarGameObject == null)
            {
                return new DTCabinetWearable[0];
            }
            return avatarGameObject.GetComponentsInChildren<DTCabinetWearable>();
        }

        public static bool AddCabinetWearable(CabinetConfig cabinetConfig, GameObject avatarGameObject, WearableConfig wearableConfig, GameObject wearableGameObject)
        {
            // do not add if there's an existing component
            if (wearableGameObject.GetComponent<DTCabinetWearable>() != null)
            {
                return false;
            }

            if (PrefabUtility.IsPartOfAnyPrefab(wearableGameObject) && PrefabUtility.GetPrefabInstanceStatus(wearableGameObject) == PrefabInstanceStatus.NotAPrefab)
            {
                // if not in scene, we instantiate it with a prefab connection
                wearableGameObject = (GameObject)PrefabUtility.InstantiatePrefab(wearableGameObject);
            }

            // parent to avatar
            wearableGameObject.transform.SetParent(avatarGameObject.transform);

            // add cabinet wearable component
            var cabinetWearable = wearableGameObject.AddComponent<DTCabinetWearable>();

            cabinetWearable.wearableGameObject = wearableGameObject;
            cabinetWearable.configJson = wearableConfig.Serialize().ToString(Formatting.None);

            // do provider hooks
            var providers = WearableModuleProviderLocator.Instance.GetAllProviders();
            foreach (var provider in providers)
            {
                var module = FindWearableModule(wearableConfig, provider.ModuleIdentifier);
                if (module == null)
                {
                    // config does not have such module
                    continue;
                }

                if (!provider.OnAddWearableToCabinet(cabinetConfig, avatarGameObject, wearableConfig, wearableGameObject, module))
                {
                    Debug.LogWarning("[DressingTools] [AddCabinetWearable] Error processing provider OnAddWearableToCabinet hook: " + provider.ModuleIdentifier);
                    return false;
                }
            }

            return true;
        }

        public static void RemoveCabinetWearable(DTCabinet cabinet, DTCabinetWearable wearable)
        {
            var cabinetWearables = cabinet.avatarGameObject.GetComponentsInChildren<DTCabinetWearable>();
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
            wearableConfig.Version = WearableConfig.CurrentConfigVersion;

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
                wearableConfig.AvatarConfig.armatureName = "";
            }
            else
            {
                if (CabinetConfig.TryDeserialize(cabinet.configJson, out var cabinetConfig))
                {
                    wearableConfig.AvatarConfig.armatureName = cabinetConfig.AvatarArmatureName;
                }
                else
                {
                    wearableConfig.AvatarConfig.armatureName = "";
                }
            }

            // can't do anything
            if (targetAvatar == null || targetWearable == null)
            {
                return;
            }

            wearableConfig.AvatarConfig.name = targetAvatar.name;

            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(targetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            wearableConfig.AvatarConfig.guids.Clear();
            if (!invalidAvatarPrefabGuid)
            {
                // TODO: multiple guids
                wearableConfig.AvatarConfig.guids.Add(avatarPrefabGuid);
            }

            var deltaPos = targetWearable.transform.position - targetAvatar.transform.position;
            var deltaRotation = targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation);
            wearableConfig.AvatarConfig.worldPosition = new AvatarConfigVector3(deltaPos);
            wearableConfig.AvatarConfig.worldRotation = new AvatarConfigQuaternion(deltaRotation);
            wearableConfig.AvatarConfig.avatarLossyScale = new AvatarConfigVector3(targetAvatar.transform.lossyScale);
            wearableConfig.AvatarConfig.wearableLossyScale = new AvatarConfigVector3(targetWearable.transform.lossyScale);
        }

        public static void AddWearableMetaInfo(WearableConfig config, GameObject targetWearable)
        {
            if (targetWearable == null)
            {
                return;
            }

            config.Info.name = targetWearable.name;
            config.Info.author = "";
            config.Info.description = "";
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
            foreach (var module in config.Modules)
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
            foreach (var module in config.Modules)
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
            foreach (var module in config.Modules)
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
            foreach (var module in config.Modules)
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

                if (transform.TryGetComponent<DTCabinetWearable>(out var _))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }
}
