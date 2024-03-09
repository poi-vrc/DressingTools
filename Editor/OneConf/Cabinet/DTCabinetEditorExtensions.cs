/*
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using Chocopoi.DressingFramework;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Cabinet
{
    internal static class DTCabinetEditorExtensions
    {
        public static bool AddWearable(this DTCabinet cabinet, WearableConfig wearableConfig, GameObject wearableGameObject)
        {
            var cabinetConfig = CabinetConfigUtility.Deserialize(cabinet.ConfigJson);
            var cabinetWearable = OneConfUtils.GetCabinetWearable(wearableGameObject);

            // if not exist, create a new component
            if (cabinetWearable == null)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(wearableGameObject) && PrefabUtility.GetPrefabInstanceStatus(wearableGameObject) == PrefabInstanceStatus.NotAPrefab)
                {
                    // if not in scene, we instantiate it with a prefab connection
                    wearableGameObject = (GameObject)PrefabUtility.InstantiatePrefab(wearableGameObject);
                }

                // parent to avatar if haven't yet
                if (!DKEditorUtils.IsGrandParent(cabinet.RootGameObject.transform, wearableGameObject.transform))
                {
                    wearableGameObject.transform.SetParent(cabinet.RootGameObject.transform);
                }

                // applying scalings
                if (!PrefabUtility.IsPartOfAnyPrefab(wearableGameObject))
                {
                    wearableConfig.ApplyAvatarConfigTransforms(cabinet.RootGameObject, wearableGameObject);
                }

                // add cabinet wearable component
                cabinetWearable = wearableGameObject.AddComponent<DTWearable>();
                cabinetWearable.RootGameObject = wearableGameObject;
            }

            wearableConfig.info.RefreshUpdatedTime();
            cabinetWearable.ConfigJson = WearableConfigUtility.Serialize(wearableConfig);

            // TODO: check config valid

            var handlers = ModuleManager.Instance.GetAllToolEventHandlers();
            foreach (var handler in handlers)
            {
                // TODO: dependency graph
                handler.OnAddWearableToCabinet(cabinetConfig, cabinet.RootGameObject, wearableConfig, wearableGameObject);
            }

            return true;
        }

        public static void RemoveWearable(this DTCabinet cabinet, DTWearable wearable)
        {
            var cabinetWearables = cabinet.RootGameObject.GetComponentsInChildren<DTWearable>();
            foreach (var cabinetWearable in cabinetWearables)
            {
                if (cabinetWearable == wearable)
                {
                    if (PrefabUtility.IsPartOfAnyPrefab(cabinetWearable.gameObject))
                    {
                        Debug.Log("[DressingFramework] Wearable is part of a prefab. Only the component is removed.");
                        Object.DestroyImmediate(cabinetWearable);
                    }
                    else
                    {
                        Object.DestroyImmediate(cabinetWearable.gameObject);
                    }
                    break;
                }
            }
        }
    }
}
