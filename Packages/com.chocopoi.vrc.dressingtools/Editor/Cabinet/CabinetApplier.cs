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
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Lib.Wearable.Modules.Providers;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using Newtonsoft.Json.Linq;
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
            public const string AdjustedWearablePositionFromDelta = "cabinet.applier.msgCode.info.adjustedWearablePositionFromDelta";
            public const string AdjustedWearableRotationFromDelta = "cabinet.applier.msgCode.info.adjustedWearableRotationFromDelta";
            public const string AdjustedAvatarScale = "cabinet.applier.msgCode.info.adjustedAvatarScale";
            public const string AdjustedWearableScale = "cabinet.applier.msgCode.info.adjustedWearableScale";

            // Error
            public const string UnableToDeserializeConfig = "cabinet.applier.msgCode.error.unableToDeserializeConfig";
            public const string ApplyingModuleHasErrors = "cabinet.applier.msgCode.error.applyingModuleHasErrors";
            public const string ApplyingWearableHasErrors = "cabinet.applier.msgCode.error.applyingWearableHasErrors";
            public const string IncompatibleConfigVersion = "cabinet.applier.msgCode.error.incompatibleConfigVersion";
            public const string ConfigMigrationFailed = "cabinet.applier.msgCode.error.configMigrationFailed";
            public const string ModuleHasNoProviderAvailable = "cabinet.applier.msgCode.error.moduleHasNoProviderAvailable";
            public const string BeforeApplyCabinetProviderHookHasErrors = "cabinet.applier.msgCode.error.beforeApplyCabinetProviderHookHasErrors";
            public const string AfterApplyCabinetProviderHookHasErrors = "cabinet.applier.msgCode.error.afterApplyCabinetProviderHookHasErrors";
        }

        private ApplyCabinetContext _cabCtx;

        public CabinetApplier(DTReport report, DTCabinet cabinet)
        {
            _cabCtx = new ApplyCabinetContext()
            {
                report = report,
                cabinet = cabinet,
                avatarDynamics = new List<IDynamicsProxy>()
            };
        }

        private void ApplyTransforms(AvatarConfig avatarConfig, GameObject targetWearable, out Transform lastAvatarParent, out Vector3 lastAvatarScale)
        {
            var targetAvatar = _cabCtx.cabinet.avatarGameObject;

            // check position delta and adjust
            {
                var wearableWorldPos = avatarConfig.worldPosition.ToVector3();
                if (targetWearable.transform.position - targetAvatar.transform.position != wearableWorldPos)
                {
                    DTReportUtils.LogInfoLocalized(_cabCtx.report, LogLabel, MessageCode.AdjustedWearablePositionFromDelta, wearableWorldPos.ToString());
                    targetWearable.transform.position += wearableWorldPos;
                }
            }

            // check rotation delta and adjust
            {
                var wearableWorldRot = avatarConfig.worldRotation.ToQuaternion();
                if (targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation) != wearableWorldRot)
                {
                    DTReportUtils.LogInfoLocalized(_cabCtx.report, LogLabel, MessageCode.AdjustedWearableRotationFromDelta, wearableWorldRot.ToString());
                    targetWearable.transform.rotation *= wearableWorldRot;
                }
            }

            // apply avatar scale
            lastAvatarParent = _cabCtx.cabinet.avatarGameObject.transform.parent;
            lastAvatarScale = Vector3.zero + targetAvatar.transform.localScale;
            if (lastAvatarParent != null)
            {
                // tricky workaround to apply lossy world scale is to unparent
                _cabCtx.cabinet.avatarGameObject.transform.SetParent(null);
            }

            var avatarScaleVec = avatarConfig.avatarLossyScale.ToVector3();
            if (targetAvatar.transform.localScale != avatarScaleVec)
            {
                DTReportUtils.LogInfoLocalized(_cabCtx.report, LogLabel, MessageCode.AdjustedAvatarScale, avatarScaleVec.ToString());
                targetAvatar.transform.localScale = avatarScaleVec;
            }

            // apply wearable scale
            var wearableScaleVec = avatarConfig.wearableLossyScale.ToVector3();
            if (targetWearable.transform.localScale != wearableScaleVec)
            {
                DTReportUtils.LogInfoLocalized(_cabCtx.report, LogLabel, MessageCode.AdjustedWearableScale, wearableScaleVec.ToString());
                targetWearable.transform.localScale = wearableScaleVec;
            }
        }

        private void RollbackTransform(Transform lastAvatarParent, Vector3 lastAvatarScale)
        {
            // restore avatar scale
            if (lastAvatarParent != null)
            {
                _cabCtx.cabinet.avatarGameObject.transform.SetParent(lastAvatarParent);
            }
            _cabCtx.cabinet.avatarGameObject.transform.localScale = lastAvatarScale;
        }

        private void CopyDynamicsToContainer(IDynamicsProxy dynamics, GameObject dynamicsContainer)
        {
            // in our PhysBoneProxy, we return the current transform if rootTransform is null
            // so if we move it away, the controlling transform will be incorrect. so we are
            // setting the transform again here.
            if (dynamics is PhysBoneProxy && dynamics.RootTransform == dynamics.Transform)
            {
                dynamics.RootTransform = dynamics.Transform;
            }

            // copy to dynamics container
            DTEditorUtils.CopyComponent(dynamics.Component, dynamicsContainer);

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

            if (_cabCtx.cabinet.groupDynamicsSeparateGameObjects)
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

        private bool ApplyWearable(ApplyWearableContext wearCtx)
        {
            GameObject wearableObj;
            if (DTEditorUtils.IsGrandParent(_cabCtx.cabinet.avatarGameObject.transform, wearCtx.wearableGameObject.transform))
            {
                wearableObj = wearCtx.wearableGameObject;
            }
            else
            {
                // instantiate wearable prefab and parent to avatar
                wearableObj = Object.Instantiate(wearCtx.wearableGameObject, _cabCtx.cabinet.avatarGameObject.transform);
            }

            // apply translation and scaling
            ApplyTransforms(wearCtx.config.AvatarConfig, wearableObj, out var lastAvatarParent, out var lastAvatarScale);

            // sort modules according to their apply order
            var modules = new List<WearableModule>(wearCtx.config.Modules);
            modules.Sort((m1, m2) =>
            {
                var m1Provider = ModuleProviderLocator.Instance.GetProvider(m1.moduleName);
                var m2Provider = ModuleProviderLocator.Instance.GetProvider(m2.moduleName);

                if (m1Provider == null)
                {
                    return -1;
                }
                else if (m2Provider == null)
                {
                    return 1;
                }

                return m1Provider.ApplyOrder.CompareTo(m2Provider.ApplyOrder);
            });

            // do module apply
            foreach (var module in modules)
            {
                // locate the module provider
                var provider = ModuleProviderLocator.Instance.GetProvider(module.moduleName);

                if (provider == null)
                {
                    DTReportUtils.LogErrorLocalized(_cabCtx.report, LogLabel, MessageCode.ModuleHasNoProviderAvailable, module.moduleName);
                    return false;
                }

                if (!provider.OnApplyWearable(_cabCtx, wearCtx, module))
                {
                    DTReportUtils.LogErrorLocalized(_cabCtx.report, LogLabel, MessageCode.ApplyingModuleHasErrors);
                    return false;
                }
            }

            // group dynamics
            if (_cabCtx.cabinet.groupDynamics)
            {
                GroupDynamics(wearCtx.wearableGameObject, wearCtx.wearableDynamics);
            }

            RollbackTransform(lastAvatarParent, lastAvatarScale);

            return true;
        }

        private static bool DoBeforeApplyCabinetProviderHooks(ApplyCabinetContext ctx)
        {
            // do provider hooks
            var providers = ModuleProviderLocator.Instance.GetAllProviders();
            foreach (var provider in providers)
            {
                if (!provider.OnBeforeApplyCabinet(ctx))
                {
                    DTReportUtils.LogErrorLocalized(ctx.report, LogLabel, MessageCode.BeforeApplyCabinetProviderHookHasErrors, provider.ModuleIdentifier);
                    return false;
                }
            }
            return true;
        }

        private static bool DoAfterApplyCabinetProviderHooks(ApplyCabinetContext ctx)
        {
            // do provider hooks
            var providers = ModuleProviderLocator.Instance.GetAllProviders();
            foreach (var provider in providers)
            {
                if (!provider.OnAfterApplyCabinet(ctx))
                {
                    DTReportUtils.LogErrorLocalized(ctx.report, LogLabel, MessageCode.AfterApplyCabinetProviderHookHasErrors, provider.ModuleIdentifier);
                    return false;
                }
            }
            return true;
        }

        public void Execute()
        {
            // scan for avatar dynamics
            _cabCtx.avatarDynamics = DTEditorUtils.ScanDynamics(_cabCtx.cabinet.avatarGameObject, true);

            if (!DoBeforeApplyCabinetProviderHooks(_cabCtx))
            {
                return;
            }

            var wearables = DTEditorUtils.GetCabinetWearables(_cabCtx.cabinet);

            foreach (var wearable in wearables)
            {
                // deserialize the config
                WearableConfig config = null;
                try
                {
                    var jObject = JObject.Parse(wearable.configJson);
                    config = WearableConfig.Deserialize(jObject);
                }
                catch (System.Exception ex)
                {
                    DTReportUtils.LogExceptionLocalized(_cabCtx.report, LogLabel, ex);
                    DTReportUtils.LogErrorLocalized(_cabCtx.report, LogLabel, MessageCode.UnableToDeserializeConfig);
                    return;
                }

                if (config == null)
                {
                    DTReportUtils.LogErrorLocalized(_cabCtx.report, LogLabel, MessageCode.UnableToDeserializeConfig);
                    return;
                }

                var wearCtx = new ApplyWearableContext()
                {
                    config = config,
                    wearableGameObject = wearable.wearableGameObject,
                    wearableDynamics = DTEditorUtils.ScanDynamics(wearable.wearableGameObject, false)
                };

                if (!ApplyWearable(wearCtx))
                {
                    DTReportUtils.LogErrorLocalized(_cabCtx.report, LogLabel, MessageCode.ApplyingWearableHasErrors, config.Info.name);
                    return;
                }
            }

            if (!DoAfterApplyCabinetProviderHooks(_cabCtx))
            {
                return;
            }
        }
    }
}
