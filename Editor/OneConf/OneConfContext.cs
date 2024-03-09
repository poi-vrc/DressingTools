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

using System.Collections.Generic;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.Dynamics.Proxy;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf
{
    internal class CabinetContext : IExtraContext
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public const string LogLabel = "CabinetApplier";

        public static class MessageCode
        {
            // Error
            public const string UnableToDeserializeCabinetConfig = "cabinet.applier.msgCode.error.unableToDeserializeCabinetConfig";
            public const string UnableToDeserializeWearableConfig = "cabinet.applier.msgCode.error.unableToDeserializeWearableConfig";
            public const string CabinetHookHasErrors = "cabinet.applier.msgCode.error.cabinetHookHasErrors";
            public const string WearableHookHasErrors = "cabinet.applier.msgCode.error.wearableHookHasErrors";
            public const string UnresolvedDependencies = "cabinet.applier.msgCode.error.unresolvedDependencies";
            public const string CyclicDependencies = "cabinet.applier.msgCode.error.cyclicDependencies";
            public const string ModuleHasNoProviderAvailable = "cabinet.applier.msgCode.error.moduleHasNoProviderAvailable";
        }

        /// <summary>
        /// Cabinet config
        /// </summary>
        public CabinetConfig cabinetConfig;

        /// <summary>
        /// All wearable contexts
        /// </summary>
        public Dictionary<DTWearable, WearableContext> wearableContexts;

        public List<IDynamicsProxy> avatarDynamics;

        public Context dkCtx;

        public CabinetContext()
        {
            cabinetConfig = null;
            wearableContexts = new Dictionary<DTWearable, WearableContext>();
        }

        private void InitWearableContexts()
        {
            var cabinet = OneConfUtils.GetAvatarCabinet(dkCtx.AvatarGameObject, false);

            // attempt to deserialize cabinet config
            if (cabinet != null)
            {
                try
                {
                    cabinetConfig = CabinetConfigUtility.Deserialize(cabinet.ConfigJson);
                }
                catch (System.Exception ex)
                {
                    dkCtx.Report.LogExceptionLocalized(t, LogLabel, ex);
                    dkCtx.Report.LogErrorLocalized(t, LogLabel, MessageCode.UnableToDeserializeCabinetConfig, dkCtx.AvatarGameObject.gameObject.name);
                    return;
                }
            }
            else
            {
                cabinetConfig = new CabinetConfig();
            }

            var wearables = OneConfUtils.GetCabinetWearables(dkCtx.AvatarGameObject);

            foreach (var wearable in wearables)
            {
                // deserialize the config
                WearableConfig wearableConfig = null;
                try
                {
                    wearableConfig = WearableConfigUtility.Deserialize(wearable.ConfigJson);
                }
                catch (System.Exception ex)
                {
                    dkCtx.Report.LogExceptionLocalized(t, LogLabel, ex);
                    dkCtx.Report.LogErrorLocalized(t, LogLabel, MessageCode.UnableToDeserializeWearableConfig, wearable.RootGameObject.name);
                    return;
                }

                if (wearableConfig == null)
                {
                    dkCtx.Report.LogErrorLocalized(t, LogLabel, MessageCode.UnableToDeserializeWearableConfig, wearable.RootGameObject.name);
                    return;
                }

                // detect unknown modules and report them
                var unknownModules = wearableConfig.FindUnknownModules();
                if (unknownModules.Count > 0)
                {
                    foreach (var module in unknownModules)
                    {
                        dkCtx.Report.LogErrorLocalized(t, LogLabel, MessageCode.ModuleHasNoProviderAvailable, module.moduleName);
                    }
                    return;
                }

                // clone if needed
                GameObject wearableObj;
                if (DKEditorUtils.IsGrandParent(dkCtx.AvatarGameObject.transform, wearable.RootGameObject.transform))
                {
                    wearableObj = wearable.RootGameObject;

                    if (PrefabUtility.GetPrefabAssetType(wearableObj) != PrefabAssetType.NotAPrefab)
                    {
                        throw new System.Exception("A wearable prefab is passed through cabinet applier!");
                    }
                }
                else
                {
                    // instantiate wearable prefab and parent to avatar
                    wearableObj = Object.Instantiate(wearable.RootGameObject, dkCtx.AvatarGameObject.transform);
                }

                var wearCtx = new WearableContext()
                {
                    wearableConfig = wearableConfig,
                    wearableGameObject = wearableObj,
                    wearableDynamics = DynamicsUtils.ScanDynamics(wearableObj)
                };

                wearableContexts[wearable] = wearCtx;
            }
        }

        public void OnEnable(Context ctx)
        {
            dkCtx = ctx;
            avatarDynamics = OneConfUtils.ScanAvatarOnlyDynamics(ctx.AvatarGameObject);
            InitWearableContexts();
        }

        public void OnDisable(Context ctx)
        {
            dkCtx = null;
        }

        public void CreateUniqueAsset(Object obj, string name)
        {
            dkCtx.CreateUniqueAsset(obj, name);
        }
    }

    /// <summary>
    /// Apply wearable context
    /// </summary>
    internal class WearableContext
    {
        /// <summary>
        /// Wearable config
        /// </summary>
        public WearableConfig wearableConfig;

        /// <summary>
        /// Wearable GameObject
        /// </summary>
        public GameObject wearableGameObject;

        /// <summary>
        /// Scanned wearable dynamics
        /// </summary>
        public List<IDynamicsProxy> wearableDynamics;

        private readonly string _randomString;

        /// <summary>
        /// Initialize a new wearable context
        /// </summary>
        public WearableContext()
        {
            _randomString = DKEditorUtils.RandomString(8);
        }

        /// <summary>
        /// Make the specified name unique to this cabinet wearable.
        /// Note: Only guarantee uniqueness of the same name against other wearables, not within the same wearable.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Unique name</returns>
        public string MakeUniqueName(string name)
        {
            return $"{wearableGameObject.name}_{_randomString}_{name}";
        }

        /// <summary>
        /// Make an asset with the specified name unique to this cabinet avatar and wearable
        /// Note: Only guarantee uniqueness of the same name against other wearables, not within the same wearable.
        /// </summary>
        /// <param name="cabCtx">Apply cabinet context</param>
        /// <param name="obj">Asset to save</param>
        /// <param name="name">Name</param>
        public void CreateUniqueAsset(CabinetContext cabCtx, Object obj, string name)
        {
            cabCtx.CreateUniqueAsset(obj, MakeUniqueName(name));
        }
    }
}
