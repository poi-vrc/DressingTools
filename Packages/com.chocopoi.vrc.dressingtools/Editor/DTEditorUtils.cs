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

using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules.Providers;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    internal class DTEditorUtils
    {
        //Reference: https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/#post-3416790
        public static void DrawHorizontalLine(int height = 1)
        {
            EditorGUILayout.Separator();
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Separator();
        }

        public static void ReadOnlyTextField(string label, string text)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                EditorGUILayout.SelectableLabel(text, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            EditorGUILayout.EndHorizontal();
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
                comp.AvatarGameObject = avatar;
                comp.AvatarArmatureName = "Armature";
            }

            return comp;
        }

        public static bool AddCabinetWearable(DTCabinet cabinet, WearableConfig config, GameObject wearableGameObject)
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
            wearableGameObject.transform.SetParent(cabinet.transform);

            // add cabinet wearable component
            var cabinetWearable = wearableGameObject.AddComponent<DTCabinetWearable>();

            cabinetWearable.wearableGameObject = wearableGameObject;
            cabinetWearable.configJson = config.Serialize().ToString(Formatting.None);

            // do provider hooks
            var providers = ModuleProviderLocator.Instance.GetAllProviders();
            foreach (var provider in providers)
            {
                var module = DTRuntimeUtils.FindWearableModule(config, provider.ModuleIdentifier);
                if (module == null)
                {
                    // config does not have such module
                    continue;
                }

                if (!provider.OnAddWearableToCabinet(cabinet, config, wearableGameObject, module))
                {
                    Debug.LogWarning("[DressingTools] [AddCabinetWearable] Error processing provider OnAddWearableToCabinet hook: " + provider.ModuleIdentifier);
                    return false;
                }
            }

            return true;
        }

        public static void RemoveCabinetWearable(DTCabinet cabinet, DTCabinetWearable wearable)
        {
            var cabinetWearables = cabinet.AvatarGameObject.GetComponentsInChildren<DTCabinetWearable>();
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

        public static void PrepareWearableConfig(WearableConfig config, GameObject targetAvatar, GameObject targetWearable)
        {
            config.Version = WearableConfig.CurrentConfigVersion;

            AddWearableMetaInfo(config, targetWearable);
            AddWearableTargetAvatarConfig(config, targetAvatar, targetWearable);
        }

        public static void AddWearableTargetAvatarConfig(WearableConfig config, GameObject targetAvatar, GameObject targetWearable)
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(targetAvatar);

            // try obtain armature name from cabinet
            if (cabinet == null)
            {
                // leave it empty
                config.AvatarConfig.armatureName = "";
            }
            else
            {
                config.AvatarConfig.armatureName = cabinet.AvatarArmatureName;
            }

            // can't do anything
            if (targetAvatar == null || targetWearable == null)
            {
                return;
            }

            config.AvatarConfig.name = targetAvatar.name;

            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(targetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            config.AvatarConfig.guids.Clear();
            if (!invalidAvatarPrefabGuid)
            {
                // TODO: multiple guids
                config.AvatarConfig.guids.Add(avatarPrefabGuid);
            }

            var deltaPos = targetWearable.transform.position - targetAvatar.transform.position;
            var deltaRotation = targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation);
            config.AvatarConfig.worldPosition = new AvatarConfigVector3(deltaPos);
            config.AvatarConfig.worldRotation = new AvatarConfigQuaternion(deltaRotation);
            config.AvatarConfig.avatarLossyScale = new AvatarConfigVector3(targetAvatar.transform.lossyScale);
            config.AvatarConfig.wearableLossyScale = new AvatarConfigVector3(targetWearable.transform.lossyScale);
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
    }
}
