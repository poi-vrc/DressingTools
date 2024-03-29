/*
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
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Event;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.Passes.Modifiers;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Wearable.Modules
{
    [InitializeOnLoad]
    internal class BlendshapeSyncWearableModuleProvider : WearableModuleProvider
    {
        internal class EventAdapter : ToolEventAdapter
        {
            public override void OnAddWearableToCabinet(CabinetConfig cabinetConfig, GameObject avatarGameObject, WearableConfig wearableConfig, GameObject wearableGameObject)
            {
                var bsm = wearableConfig.FindModule(BlendshapeSyncWearableModuleConfig.ModuleIdentifier);
                if (bsm == null) return;
                FollowBlendshapeSyncValues(avatarGameObject, wearableGameObject, bsm);
            }
        }

        private static readonly I18nTranslator t = I18n.ToolTranslator;

        [ExcludeFromCodeCoverage] public override string Identifier => BlendshapeSyncWearableModuleConfig.ModuleIdentifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("modules.wearable.blendshapeSync.friendlyName");
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .BeforePass<BlendshapeSyncPass>()
                .Build();

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

        // TODO: remove this function
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

        public override bool Invoke(CabinetContext cabCtx, WearableContext wearCtx, ReadOnlyCollection<WearableModule> modules, bool isPreview)
        {
            if (modules.Count == 0) return true;

            FollowBlendshapeSyncValues(cabCtx.dkCtx.AvatarGameObject, wearCtx.wearableGameObject, modules[0]);

            if (isPreview)
            {
                return true;
            }

            var bsm = (BlendshapeSyncWearableModuleConfig)modules[0].config;
            var comp = wearCtx.wearableGameObject.AddComponent<DTBlendshapeSync>();
            foreach (var bs in bsm.blendshapeSyncs)
            {
                var wearableSmrObj = wearCtx.wearableGameObject.transform.Find(bs.wearablePath);
                if (wearableSmrObj == null ||
                    !wearableSmrObj.TryGetComponent<SkinnedMeshRenderer>(out var wearableSmr))
                {
                    continue;
                }

                comp.Entries.Add(new DTBlendshapeSync.Entry()
                {
                    SourcePath = bs.avatarPath,
                    SourceBlendshape = bs.avatarBlendshapeName,
                    DestinationSkinnedMeshRenderer = wearableSmr,
                    DestinationBlendshape = bs.wearableBlendshapeName
                });
            }

            return true;
        }
    }
}
