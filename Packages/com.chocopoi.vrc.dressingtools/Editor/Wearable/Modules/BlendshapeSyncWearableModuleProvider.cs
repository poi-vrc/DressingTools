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
using Chocopoi.DressingFramework.Cabinet;
using Chocopoi.DressingFramework.Context;
using Chocopoi.DressingFramework.Extensibility.Plugin;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingFramework.Wearable;
using Chocopoi.DressingFramework.Wearable.Modules;
using Chocopoi.DressingTools.Api.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.Localization;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    [InitializeOnLoad]
    internal class BlendshapeSyncWearableModuleProvider : WearableModuleProviderBase
    {
        internal class EventAdapter : FrameworkEventAdapter
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

        public override WearableApplyConstraint Constraint =>
            ApplyAtStage(CabinetApplyStage.Transpose)
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

        private static BlendshapeSyncWearableModuleConfig.BlendshapeSync FindBlendshapeSync(EditorCurveBinding curveBinding, List<BlendshapeSyncWearableModuleConfig.BlendshapeSync> blendshapeSyncs)
        {
            foreach (var blendshapeSync in blendshapeSyncs)
            {
                if (curveBinding.path == blendshapeSync.avatarPath && curveBinding.propertyName == "blendShape." + blendshapeSync.avatarBlendshapeName)
                {
                    return blendshapeSync;
                }
            }
            return null;
        }

        public override bool Invoke(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, ReadOnlyCollection<WearableModule> modules, bool isPreview)
        {
            if (modules.Count == 0) return true;

            FollowBlendshapeSyncValues(cabCtx.avatarGameObject, wearCtx.wearableGameObject, modules[0]);

            if (isPreview)
            {
                return true;
            }

            var bsm = (BlendshapeSyncWearableModuleConfig)modules[0].config;

            // TODO: implement partial boundaries and inverts

            foreach (var clipContainer in cabCtx.animationStore.Clips)
            {
                var oldClip = clipContainer.newClip != null ? clipContainer.newClip : clipContainer.originalClip;
                var newClip = DTEditorUtils.CopyClip(oldClip);
                var modified = false;

                var curveBindings = AnimationUtility.GetCurveBindings(oldClip);
                foreach (var curveBinding in curveBindings)
                {
                    var blendshapeSync = FindBlendshapeSync(curveBinding, bsm.blendshapeSyncs);
                    if (blendshapeSync != null)
                    {
                        var basePath = AnimationUtils.GetRelativePath(wearCtx.wearableGameObject.transform, cabCtx.avatarGameObject.transform);
                        newClip.SetCurve(basePath + "/" + blendshapeSync.wearablePath, curveBinding.type, "blendShape." + blendshapeSync.wearableBlendshapeName, AnimationUtility.GetEditorCurve(oldClip, curveBinding));
                        modified = true;
                    }
                }

                if (modified)
                {
                    clipContainer.newClip = newClip;
                }
            }

            return true;
        }
    }
}
