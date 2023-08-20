/*
 * File: DTCabinetApplier.cs
 * Project: DressingTools
 * Created Date: Saturday, July 29th 2023, 10:31:11 am
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
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    internal class CabinetApplier
    {
        public const string LogLabel = "DTCabinetApplier";
        private const string DynamicsContainerName = "DT_Dynamics";

        public static class MessageCode
        {
            // Info
            public const string AdjustedWearablePositionFromDelta = "appliers.default.msgCode.info.adjustedWearablePositionFromDelta";
            public const string AdjustedWearableRotationFromDelta = "appliers.default.msgCode.info.adjustedWearableRotationFromDelta";
            public const string AdjustedAvatarScale = "appliers.default.msgCode.info.adjustedAvatarScale";
            public const string AdjustedWearableScale = "appliers.default.msgCode.info.adjustedWearableScale";

            // Error
            public const string UnableToDeserializeConfig = "appliers.default.msgCode.error.unableToDeserializeConfig";
            public const string ApplyingModuleHasErrors = "appliers.default.msgCode.error.applyingModuleHasErrors";
            public const string ApplyingWearableHasErrors = "appliers.default.msgCode.error.applyingWearableHasErrors";
            public const string IncompatibleConfigVersion = "appliers.default.msgCode.error.incompatibleConfigVersion";
            public const string ConfigMigrationFailed = "appliers.default.msgCode.error.configMigrationFailed";
        }

        private DTReport _report;

        private DTCabinet _cabinet;

        private List<IDynamicsProxy> _avatarDynamics;

        public CabinetApplier(DTReport report, DTCabinet cabinet)
        {
            _report = report;
            _cabinet = cabinet;
        }

        private void ApplyTransforms(AvatarConfig avatarConfig, GameObject targetWearable, out Transform lastAvatarParent, out Vector3 lastAvatarScale)
        {
            var targetAvatar = _cabinet.AvatarGameObject;

            // check position delta and adjust
            {
                var wearableWorldPos = avatarConfig.worldPosition.ToVector3();
                if (targetWearable.transform.position - targetAvatar.transform.position != wearableWorldPos)
                {
                    DTReportUtils.LogInfoLocalized(_report, LogLabel, MessageCode.AdjustedWearablePositionFromDelta, wearableWorldPos.ToString());
                    targetWearable.transform.position += wearableWorldPos;
                }
            }

            // check rotation delta and adjust
            {
                var wearableWorldRot = avatarConfig.worldRotation.ToQuaternion();
                if (targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation) != wearableWorldRot)
                {
                    DTReportUtils.LogInfoLocalized(_report, LogLabel, MessageCode.AdjustedWearableRotationFromDelta, wearableWorldRot.ToString());
                    targetWearable.transform.rotation *= wearableWorldRot;
                }
            }

            // apply avatar scale
            lastAvatarParent = _cabinet.AvatarGameObject.transform.parent;
            lastAvatarScale = Vector3.zero + targetAvatar.transform.localScale;
            if (lastAvatarParent != null)
            {
                // tricky workaround to apply lossy world scale is to unparent
                _cabinet.AvatarGameObject.transform.SetParent(null);
            }

            var avatarScaleVec = avatarConfig.avatarLossyScale.ToVector3();
            if (targetAvatar.transform.localScale != avatarScaleVec)
            {
                DTReportUtils.LogInfoLocalized(_report, LogLabel, MessageCode.AdjustedAvatarScale, avatarScaleVec.ToString());
                targetAvatar.transform.localScale = avatarScaleVec;
            }

            // apply wearable scale
            var wearableScaleVec = avatarConfig.wearableLossyScale.ToVector3();
            if (targetWearable.transform.localScale != wearableScaleVec)
            {
                DTReportUtils.LogInfoLocalized(_report, LogLabel, MessageCode.AdjustedWearableScale, wearableScaleVec.ToString());
                targetWearable.transform.localScale = wearableScaleVec;
            }
        }

        private void RollbackTransform(Transform lastAvatarParent, Vector3 lastAvatarScale)
        {
            // restore avatar scale
            if (lastAvatarParent != null)
            {
                _cabinet.AvatarGameObject.transform.SetParent(lastAvatarParent);
            }
            _cabinet.AvatarGameObject.transform.localScale = lastAvatarScale;
        }

        private void CopyDynamicsToContainer(IDynamicsProxy dynamics, GameObject dynamicsContainer)
        {
            // in case it does not have a root transform
            if (dynamics.RootTransform == null)
            {
                dynamics.RootTransform = dynamics.Transform;
            }

            // copy to dynamics container
            DTRuntimeUtils.CopyComponent(dynamics.Component, dynamicsContainer.gameObject);

            // destroy the original one
            Object.DestroyImmediate(dynamics.Component);
        }

        private void GroupDynamics(GameObject wearableGameObject, List<IDynamicsProxy> wearableDynamics)
        {
            // no need to group if no dynamics
            if (wearableDynamics.Count == 0)
            {
                return;
            }

            // create dynamics container (reuse if originally have)
            var dynamicsContainer = wearableGameObject.transform.Find(DynamicsContainerName);
            if (dynamicsContainer == null)
            {
                var obj = new GameObject(DynamicsContainerName);
                obj.transform.SetParent(wearableGameObject.transform);
                dynamicsContainer = obj.transform;
            }

            if (_cabinet.GroupDynamicsSeparateGameObjects)
            {
                // group them in separate GameObjects
                var addedNames = new Dictionary<string, int>();
                foreach (var dynamics in wearableDynamics)
                {
                    var name = dynamics.RootTransform.name;

                    // we might occur cases with dynamics' bone name are the same
                    if (!addedNames.TryGetValue(name, out int count))
                    {
                        count = 0;
                    }

                    // we don't add suffix for the first occurance
                    var containerName = count == 0 ? name : string.Format("{0}_{1}", name, count);
                    var container = new GameObject(containerName);
                    container.transform.SetParent(dynamicsContainer);

                    CopyDynamicsToContainer(dynamics, container);

                    addedNames[name] = ++count;
                }
            }
            else
            {
                // we just group them into a single GameObject
                foreach (var dynamics in wearableDynamics)
                {
                    CopyDynamicsToContainer(dynamics, dynamicsContainer.gameObject);
                }
            }
        }

        private bool ApplyWearable(WearableConfig config, GameObject wearableGameObject)
        {
            GameObject wearableObj;
            if (DTRuntimeUtils.IsGrandParent(_cabinet.AvatarGameObject.transform, wearableGameObject.transform))
            {
                wearableObj = wearableGameObject;
            }
            else
            {
                // instantiate wearable prefab and parent to avatar
                wearableObj = Object.Instantiate(wearableGameObject, _cabinet.AvatarGameObject.transform);
            }

            // scan for wearable dynamics
            var wearableDynamics = DTRuntimeUtils.ScanDynamics(wearableObj);

            // apply translation and scaling
            ApplyTransforms(config.targetAvatarConfig, wearableObj, out var lastAvatarParent, out var lastAvatarScale);

            // sort modules according to their apply order
            var modules = new List<WearableModuleBase>(config.modules);
            modules.Sort((m1, m2) => m1.ApplyOrder.CompareTo(m2.ApplyOrder));

            // do module apply
            foreach (var module in modules)
            {
                if (!module.Apply(_report, _cabinet, _avatarDynamics, config, wearableGameObject))
                {
                    DTReportUtils.LogErrorLocalized(_report, LogLabel, MessageCode.ApplyingModuleHasErrors);
                    return false;
                }
            }

            // group dynamics
            if (_cabinet.GroupDynamics)
            {
                GroupDynamics(wearableGameObject, wearableDynamics);
            }

            RollbackTransform(lastAvatarParent, lastAvatarScale);

            return true;
        }

        public void Execute()
        {
            // scan for avatar dynamics
            _avatarDynamics = DTRuntimeUtils.ScanDynamics(_cabinet.AvatarGameObject);
            var wearables = _cabinet.GetWearables();

            foreach (var wearable in wearables)
            {
                // deserialize the config
                var config = JsonConvert.DeserializeObject<WearableConfig>(wearable.configJson);

                // Migration
                if (config.configVersion > WearableConfig.CurrentConfigVersion)
                {
                    DTReportUtils.LogErrorLocalized(_report, LogLabel, MessageCode.IncompatibleConfigVersion);
                    break;
                }
                else if (config.configVersion < WearableConfig.CurrentConfigVersion)
                {
                    var result = WearableConfigMigrator.Migrate(wearable.configJson, out var migratedJson);
                    if (!result)
                    {
                        DTReportUtils.LogErrorLocalized(_report, LogLabel, MessageCode.ConfigMigrationFailed);
                        break;
                    }
                    wearable.configJson = migratedJson;
                    config = JsonConvert.DeserializeObject<WearableConfig>(migratedJson);
                }

                if (config == null)
                {
                    DTReportUtils.LogErrorLocalized(_report, LogLabel, MessageCode.UnableToDeserializeConfig);
                    continue;
                }

                if (!ApplyWearable(config, wearable.wearableGameObject))
                {
                    DTReportUtils.LogErrorLocalized(_report, LogLabel, MessageCode.ApplyingWearableHasErrors, config.info.name);
                    break;
                }
            }
        }
    }
}
