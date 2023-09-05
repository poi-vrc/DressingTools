/*
 * File: BlendshapeSyncModule.cs
 * Project: DressingTools
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Serialization;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    internal class BlendshapeSyncWearableModuleConfig : IModuleConfig
    {
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        public SerializationVersion version;
        public List<AnimationBlendshapeSync> blendshapeSyncs; // blendshapes to sync from avatar to wearables

        public BlendshapeSyncWearableModuleConfig()
        {
            version = CurrentConfigVersion;
            blendshapeSyncs = new List<AnimationBlendshapeSync>();
        }
    }

    [InitializeOnLoad]
    internal class BlendshapeSyncWearableModuleProvider : WearableModuleProviderBase
    {
        public const string MODULE_IDENTIFIER = "com.chocopoi.dressingtools.built-in.wearable.blendshape-sync";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => MODULE_IDENTIFIER;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Blendshape Sync";
        [ExcludeFromCodeCoverage] public override int CallOrder => 6;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static BlendshapeSyncWearableModuleProvider()
        {
            WearableModuleProviderLocator.Instance.Register(new BlendshapeSyncWearableModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject)
        {
            // TODO: do schema check

            var version = jObject["version"].ToObject<SerializationVersion>();
            if (version.Major > BlendshapeSyncWearableModuleConfig.CurrentConfigVersion.Major)
            {
                throw new System.Exception("Incompatible BlendshapeSyncWearableModuleConfig version: " + version.Major + " > " + BlendshapeSyncWearableModuleConfig.CurrentConfigVersion.Major);
            }

            return jObject.ToObject<BlendshapeSyncWearableModuleConfig>();
        }

        public override IModuleConfig NewModuleConfig() => new BlendshapeSyncWearableModuleConfig();

        private static void FollowBlendshapeSyncValues(GameObject avatarGameObject, GameObject wearableGameObject, WearableModule module)
        {
            var bsm = (BlendshapeSyncWearableModuleConfig)module.config;

            // follow blendshape sync
            foreach (var bs in bsm.blendshapeSyncs)
            {
                var avatarSmrObj = avatarGameObject.transform.Find(bs.avatarPath);
                if (avatarSmrObj == null)
                {
                    Debug.LogWarning("[DressingTools] [BlendshapeSyncProvider] Blendshape sync avatar GameObject at path not found: " + bs.avatarPath);
                    continue;
                }

                var avatarSmr = avatarSmrObj.GetComponent<SkinnedMeshRenderer>();
                if (avatarSmr == null || avatarSmr.sharedMesh == null)
                {
                    Debug.LogWarning("[DressingTools] [BlendshapeSyncProvider] Blendshape sync avatar GameObject at path does not have SkinnedMeshRenderer or Mesh attached: " + bs.avatarPath);
                    continue;
                }

                var avatarBlendshapeIndex = avatarSmr.sharedMesh.GetBlendShapeIndex(bs.avatarBlendshapeName);
                if (avatarBlendshapeIndex == -1)
                {
                    Debug.LogWarning("[DressingTools] [BlendshapeSyncProvider] Blendshape sync avatar GameObject does not have blendshape: " + bs.avatarBlendshapeName);
                    continue;
                }

                var wearableSmrObj = wearableGameObject.transform.Find(bs.wearablePath);
                if (wearableSmrObj == null)
                {
                    Debug.LogWarning("[DressingTools] [BlendshapeSyncProvider] Blendshape sync wearable GameObject at path not found: " + bs.wearablePath);
                    continue;
                }

                var wearableSmr = wearableSmrObj.GetComponent<SkinnedMeshRenderer>();
                if (wearableSmr == null)
                {
                    Debug.LogWarning("[DressingTools] [BlendshapeSyncProvider] Blendshape sync wearable GameObject at path does not have SkinnedMeshRenderer or Mesh attached: " + bs.avatarPath);
                    continue;
                }

                var wearableBlendshapeIndex = wearableSmr.sharedMesh.GetBlendShapeIndex(bs.wearableBlendshapeName);
                if (wearableBlendshapeIndex == -1)
                {
                    Debug.LogWarning("[DressingTools] [BlendshapeSyncProvider] Blendshape sync wearable GameObject does not have blendshape: " + bs.wearableBlendshapeName);
                    continue;
                }

                // copy value from avatar to wearable
                wearableSmr.SetBlendShapeWeight(wearableBlendshapeIndex, avatarSmr.GetBlendShapeWeight(avatarBlendshapeIndex));
            }
        }

        public override bool OnAddWearableToCabinet(CabinetConfig cabinetConfig, GameObject avatarGameObject, WearableConfig wearableConfig, GameObject wearableGameObject, ReadOnlyCollection<WearableModule> modules)
        {
            if (modules.Count == 0)
            {
                return true;
            }

            FollowBlendshapeSyncValues(avatarGameObject, wearableGameObject, modules[0]);
            return true;
        }

        public override bool OnAfterApplyCabinet(ApplyCabinetContext cabCtx)
        {
            var wearables = DTEditorUtils.GetCabinetWearables(cabCtx.avatarGameObject);

            foreach (var wearable in wearables)
            {
                var wearCtx = cabCtx.wearableContexts[wearable];
                var config = wearCtx.wearableConfig;
                var module = DTEditorUtils.FindWearableModule(config, MODULE_IDENTIFIER);

                if (module == null)
                {
                    // no blendshape sync module, skipping
                    continue;
                }

                FollowBlendshapeSyncValues(cabCtx.avatarGameObject, wearable.wearableGameObject, module);
            }
            return true;
        }

        public override bool OnPreviewWearable(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, ReadOnlyCollection<WearableModule> modules)
        {
            if (modules.Count == 0)
            {
                return true;
            }

            FollowBlendshapeSyncValues(cabCtx.avatarGameObject, wearCtx.wearableGameObject, modules[0]);

            return true;
        }
    }
}
