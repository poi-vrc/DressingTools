/*
 * File: DTRuntimeUtils.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 12:21:25 am
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Proxy;
using Newtonsoft.Json;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    internal static class DTRuntimeUtils
    {
        private const string BoneNameMappingsPath = "Packages/com.chocopoi.vrc.dressingtools/Resources/boneNameMappings.json";

        private static Dictionary<string, System.Type> s_reflectionTypeCache = new Dictionary<string, System.Type>();

        private static List<List<string>> s_boneNameMappings = null;

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
            Type type = originalComponent.GetType();

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
