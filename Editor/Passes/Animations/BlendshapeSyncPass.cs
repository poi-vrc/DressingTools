/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Localization;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Passes.Modifiers
{
    [ComponentPassFor(typeof(DTBlendshapeSync))]
    internal class BlendshapeSyncPass : ComponentPass
    {
        public class MessageCode
        {
        }
        private const string LogLabel = "BlendshapeSyncPass";
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .Build();

        private static void FollowBlendshapeSyncValues(Context ctx, DTBlendshapeSync comp)
        {
            // follow blendshape sync
            foreach (var bs in comp.Entries)
            {
                var sourceSmrObj = ctx.AvatarGameObject.transform.Find(bs.SourcePath);
                if (sourceSmrObj == null)
                {
                    ctx.Report.LogWarn(LogLabel, "Blendshape sync avatar GameObject at path not found: " + bs.SourcePath);
                    continue;
                }

                if (!sourceSmrObj.TryGetComponent<SkinnedMeshRenderer>(out var sourceSmr) || sourceSmr.sharedMesh == null)
                {
                    ctx.Report.LogWarn(LogLabel, "Blendshape sync avatar GameObject at path does not have SkinnedMeshRenderer or Mesh attached: " + bs.SourcePath);
                    continue;
                }

                var sourceBsIndex = sourceSmr.sharedMesh.GetBlendShapeIndex(bs.SourceBlendshape);
                if (sourceBsIndex == -1)
                {
                    ctx.Report.LogWarn(LogLabel, "Blendshape sync avatar GameObject does not have blendshape: " + bs.SourceBlendshape);
                    continue;
                }

                if (bs.DestinationSkinnedMeshRenderer == null)
                {
                    ctx.Report.LogWarn(LogLabel, "Destination skinned mesh renderer is null, ignoring");
                    continue;
                }

                var destBsIndex = bs.DestinationSkinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(bs.DestinationBlendshape);
                if (destBsIndex == -1)
                {
                    ctx.Report.LogWarn(LogLabel, "Blendshape sync wearable GameObject does not have blendshape: " + bs.DestinationBlendshape);
                    continue;
                }

                // copy value from avatar to wearable
                bs.DestinationSkinnedMeshRenderer.SetBlendShapeWeight(destBsIndex, sourceSmr.GetBlendShapeWeight(sourceBsIndex));
            }
        }

        private static List<DTBlendshapeSync.Entry> FindSyncEntries(EditorCurveBinding curveBinding, List<DTBlendshapeSync.Entry> blendshapeSyncs)
        {
            return blendshapeSyncs.Where(bs =>
                curveBinding.path == bs.SourcePath &&
                curveBinding.propertyName == $"blendShape.{bs.SourceBlendshape}")
                .ToList();
        }

        private void SyncAnimationCurves(Context ctx, List<DTBlendshapeSync.Entry> allEntries)
        {
            // TODO: implement partial boundaries and inverts
            var animStore = ctx.Feature<AnimationStore>();

            foreach (var clipContainer in animStore.Clips)
            {
                var oldClip = clipContainer.newClip != null ? clipContainer.newClip : clipContainer.originalClip;
                var newClip = DTEditorUtils.CopyClip(oldClip);
                var modified = false;

                var curveBindings = AnimationUtility.GetCurveBindings(oldClip);
                foreach (var curveBinding in curveBindings)
                {
                    var matches = FindSyncEntries(curveBinding, allEntries);
                    foreach (var entry in matches)
                    {
                        if (entry.DestinationSkinnedMeshRenderer == null)
                        {
                            continue;
                        }
                        var path = AnimationUtils.GetRelativePath(entry.DestinationSkinnedMeshRenderer.transform, ctx.AvatarGameObject.transform);
                        newClip.SetCurve(path, curveBinding.type, $"blendShape.{entry.DestinationBlendshape}", AnimationUtility.GetEditorCurve(oldClip, curveBinding));
                        modified = true;
                    }
                }

                if (modified)
                {
                    clipContainer.newClip = newClip;
                }
            }
        }

        public override bool Invoke(Context ctx)
        {
            var comps = ctx.AvatarGameObject.GetComponentsInChildren<DTBlendshapeSync>(true);
            var allEntries = new List<DTBlendshapeSync.Entry>();
            foreach (var comp in comps)
            {
                FollowBlendshapeSyncValues(ctx, comp);
                allEntries.AddRange(comp.Entries);
            }
            SyncAnimationCurves(ctx, allEntries);
            return true;
        }

        public override bool Invoke(Context ctx, DTBaseComponent component, out List<DTBaseComponent> generatedComponents)
        {
            generatedComponents = new List<DTBaseComponent>();
            var bsSync = (DTBlendshapeSync)component;
            FollowBlendshapeSyncValues(ctx, bsSync);
            SyncAnimationCurves(ctx, bsSync.Entries);
            return true;
        }
    }
}
