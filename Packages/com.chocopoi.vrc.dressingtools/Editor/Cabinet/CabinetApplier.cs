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
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    internal class CabinetApplier
    {
        public const string LogLabel = "DTCabinetApplier";

        public const string GeneratedAssetsFolderName = "_DTGeneratedAssets";
        public const string GeneratedAssetsPath = "Assets/" + GeneratedAssetsFolderName;

        private const string DynamicsContainerName = "DT_Dynamics";

        public static class MessageCode
        {
            // Error
            public const string UnableToDeserializeCabinetConfig = "cabinet.applier.msgCode.error.unableToDeserializeCabinetConfig";
            public const string UnableToDeserializeWearableConfig = "cabinet.applier.msgCode.error.unableToDeserializeWearableConfig";
            public const string ApplyingModuleHasErrors = "cabinet.applier.msgCode.error.applyingModuleHasErrors";
            public const string ApplyingWearableHasErrors = "cabinet.applier.msgCode.error.applyingWearableHasErrors";
            public const string IncompatibleConfigVersion = "cabinet.applier.msgCode.error.incompatibleConfigVersion";
            public const string ConfigMigrationFailed = "cabinet.applier.msgCode.error.configMigrationFailed";
            public const string ModuleHasNoProviderAvailable = "cabinet.applier.msgCode.error.moduleHasNoProviderAvailable";
            public const string BeforeApplyCabinetProviderHookHasErrors = "cabinet.applier.msgCode.error.beforeApplyCabinetProviderHookHasErrors";
            public const string AfterApplyCabinetProviderHookHasErrors = "cabinet.applier.msgCode.error.afterApplyCabinetProviderHookHasErrors";
        }

        private ApplyCabinetContext _cabCtx;
        private DTCabinet _cabinet;

        public CabinetApplier(DTReport report, DTCabinet cabinet)
        {
            _cabinet = cabinet;
            _cabCtx = new ApplyCabinetContext()
            {
                report = report,
                avatarGameObject = cabinet.avatarGameObject,
                cabinetConfig = null,
                avatarDynamics = new List<IDynamicsProxy>(),
                wearableContexts = new Dictionary<DTCabinetWearable, ApplyWearableContext>()
            };
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
            var newComp = DTEditorUtils.CopyComponent(dynamics.Component, dynamicsContainer);

            // destroy the original one
            Object.DestroyImmediate(dynamics.Component);

            // set the component in our proxy to the new component
            dynamics.Component = newComp;
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

            if (_cabCtx.cabinetConfig.groupDynamicsSeparateGameObjects)
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
            if (DTEditorUtils.IsGrandParent(_cabCtx.avatarGameObject.transform, wearCtx.wearableGameObject.transform))
            {
                wearableObj = wearCtx.wearableGameObject;
            }
            else
            {
                // instantiate wearable prefab and parent to avatar
                wearableObj = Object.Instantiate(wearCtx.wearableGameObject, _cabCtx.avatarGameObject.transform);
            }

            // sort modules according to their apply order
            var modules = new List<WearableModule>(wearCtx.wearableConfig.modules);
            modules.Sort((m1, m2) =>
            {
                var m1Provider = WearableModuleProviderLocator.Instance.GetProvider(m1.moduleName);
                var m2Provider = WearableModuleProviderLocator.Instance.GetProvider(m2.moduleName);

                if (m1Provider == null)
                {
                    return -1;
                }
                else if (m2Provider == null)
                {
                    return 1;
                }

                return m1Provider.CallOrder.CompareTo(m2Provider.CallOrder);
            });

            // do module apply
            foreach (var module in modules)
            {
                // locate the module provider
                var provider = WearableModuleProviderLocator.Instance.GetProvider(module.moduleName);

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
            if (_cabCtx.cabinetConfig.groupDynamics)
            {
                GroupDynamics(wearCtx.wearableGameObject, wearCtx.wearableDynamics);
            }

            return true;
        }

        private static bool DoBeforeApplyCabinetProviderHooks(ApplyCabinetContext ctx, List<WearableModuleProviderBase> wearableModuleProviders)
        {
            // do provider hooks
            foreach (var provider in wearableModuleProviders)
            {
                if (!provider.OnBeforeApplyCabinet(ctx))
                {
                    DTReportUtils.LogErrorLocalized(ctx.report, LogLabel, MessageCode.BeforeApplyCabinetProviderHookHasErrors, provider.ModuleIdentifier);
                    return false;
                }
            }
            return true;
        }

        private static bool DoAfterApplyCabinetProviderHooks(ApplyCabinetContext ctx, List<WearableModuleProviderBase> wearableModuleProviders)
        {
            // do provider hooks
            foreach (var provider in wearableModuleProviders)
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
            // attempt to deserialize cabinet config
            try
            {
                _cabCtx.cabinetConfig = CabinetConfig.Deserialize(_cabinet.configJson);
            }
            catch (System.Exception ex)
            {
                DTReportUtils.LogExceptionLocalized(_cabCtx.report, LogLabel, ex);
                DTReportUtils.LogErrorLocalized(_cabCtx.report, LogLabel, MessageCode.UnableToDeserializeCabinetConfig);
                return;
            }

            // sort providers
            var wearableModuleProviders = new List<WearableModuleProviderBase>(WearableModuleProviderLocator.Instance.GetAllProviders());
            wearableModuleProviders.Sort((p1, p2) => p1.CallOrder.CompareTo(p2.CallOrder));

            // remove previous generated files
            AssetDatabase.DeleteAsset(GeneratedAssetsPath);
            AssetDatabase.CreateFolder("Assets", GeneratedAssetsFolderName);

            // scan for avatar dynamics
            _cabCtx.avatarDynamics = DTEditorUtils.ScanDynamics(_cabCtx.avatarGameObject, true);

            if (!DoBeforeApplyCabinetProviderHooks(_cabCtx, wearableModuleProviders))
            {
                return;
            }

            var wearables = DTEditorUtils.GetCabinetWearables(_cabCtx.avatarGameObject);

            foreach (var wearable in wearables)
            {
                // deserialize the config
                WearableConfig wearableConfig = null;
                try
                {
                    wearableConfig = WearableConfig.Deserialize(wearable.configJson);
                }
                catch (System.Exception ex)
                {
                    DTReportUtils.LogExceptionLocalized(_cabCtx.report, LogLabel, ex);
                    DTReportUtils.LogErrorLocalized(_cabCtx.report, LogLabel, MessageCode.UnableToDeserializeWearableConfig);
                    return;
                }

                if (wearableConfig == null)
                {
                    DTReportUtils.LogErrorLocalized(_cabCtx.report, LogLabel, MessageCode.UnableToDeserializeWearableConfig);
                    return;
                }

                var wearCtx = new ApplyWearableContext()
                {
                    wearableConfig = wearableConfig,
                    wearableGameObject = wearable.wearableGameObject,
                    wearableDynamics = DTEditorUtils.ScanDynamics(wearable.wearableGameObject, false)
                };
                _cabCtx.wearableContexts[wearable] = wearCtx;

                if (!ApplyWearable(wearCtx))
                {
                    DTReportUtils.LogErrorLocalized(_cabCtx.report, LogLabel, MessageCode.ApplyingWearableHasErrors, wearableConfig.info.name);
                    return;
                }
            }

            if (!DoAfterApplyCabinetProviderHooks(_cabCtx, wearableModuleProviders))
            {
                return;
            }

            // remove all DT components
            var dtComps = _cabCtx.avatarGameObject.GetComponentsInChildren<DTBaseComponent>();
            foreach (var comp in dtComps)
            {
                Object.DestroyImmediate(comp);
            }
        }
    }
}
