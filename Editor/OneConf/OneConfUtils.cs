/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.Dynamics.Proxy;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf
{
    internal static class OneConfUtils
    {
        private const string BoneNameMappingsPath = "Packages/com.chocopoi.vrc.dressingtools/Resources/BoneNameMappings.json";
        private static List<List<string>> s_boneNameMappings = null;

        public static DTWearable[] GetAllSceneWearables()
        {
            // TODO: check for IWearable instead sticking to DT
            return Object.FindObjectsOfType<DTWearable>();
        }

        public static DTWearable GetCabinetWearable(GameObject wearableGameObject)
        {
            if (wearableGameObject == null)
            {
                return null;
            }

            // loop through all scene wearables and search
            var wearables = GetAllSceneWearables();

            // no matter there are two occurance or not, we return the first found
            foreach (var sceneWearable in wearables)
            {
                if (sceneWearable.RootGameObject == wearableGameObject)
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

        public static DTCabinet[] GetAllCabinets()
        {
            // TODO: check for ICabinet instead sticking to DT
            return Object.FindObjectsOfType<DTCabinet>();
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
                if (cabinet.RootGameObject == avatarGameObject)
                {
                    return cabinet;
                }
            }

            if (createIfNotExists)
            {
                // create new cabinet if not exist
                var comp = avatarGameObject.AddComponent<DTCabinet>();

                // TODO: read default config, scan for armature names?
                comp.RootGameObject = avatarGameObject;
                var config = new CabinetConfig();
                comp.ConfigJson = JsonConvert.SerializeObject(config);

                return comp;
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

        public static List<IDynamicsProxy> ScanAvatarOnlyDynamics(GameObject avatarRoot)
        {
            return DynamicsUtils.ScanDynamics(avatarRoot, comp => !IsOriginatedFromAnyWearable(avatarRoot.transform, comp.transform));
        }

        public static string GetGameObjectOriginalPrefabGuid(GameObject obj)
        {
            var assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj));
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            return guid;
        }

        public static void PrepareWearableConfig(WearableConfig wearableConfig, GameObject targetAvatar, GameObject targetWearable)
        {
            wearableConfig.version = WearableConfig.CurrentConfigVersion;

            AddWearableMetaInfo(wearableConfig, targetWearable);
            AddWearableTargetAvatarConfig(wearableConfig, targetAvatar, targetWearable);
        }

        public static void AddWearableTargetAvatarConfig(WearableConfig wearableConfig, GameObject targetAvatar, GameObject targetWearable)
        {
            var cabinet = OneConfUtils.GetAvatarCabinet(targetAvatar);

            // try obtain armature name from cabinet
            if (cabinet == null)
            {
                // leave it empty
                wearableConfig.avatarConfig.armatureName = "";
            }
            else
            {
                if (CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out var cabinetConfig))
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

            var avatarPrefabGuid = GetGameObjectOriginalPrefabGuid(targetAvatar);
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

        private static void LoadBoneNameMappings()
        {
            try
            {
                var reader = new StreamReader(BoneNameMappingsPath);
                var json = reader.ReadToEnd();
                reader.Close();
                var jObj = JObject.Parse(json);
                s_boneNameMappings = jObj["mappings"].ToObject<List<List<string>>>();
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

            var texture = new Texture2D(DTEditorUtils.ThumbnailWidth, DTEditorUtils.ThumbnailHeight);
            texture.LoadImage(decompressedBuffer);
            return texture;
        }

    }
}
