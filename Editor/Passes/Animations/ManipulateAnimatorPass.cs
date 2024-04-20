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

using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Components;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Localization;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorMerger = Chocopoi.DressingTools.Animations.AnimatorMerger;
#if DT_VRCSDK3A
using Chocopoi.DressingFramework.Animations.VRChat;
using VRC.SDK3.Avatars.Components;
#endif

namespace Chocopoi.DressingTools.Passes.Modifiers
{
    [ComponentPassFor(typeof(DTBlendshapeSync))]
    internal class ManipulateAnimatorPass : ComponentPass
    {
        public class MessageCode
        {
        }
        private const string LogLabel = "ManipulateAnimatorPass";

        private const string VRCGestureAvatarMaskGuid = "b2b8bad9583e56a46a3e21795e96ad92";
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .Build();

#if DT_VRCSDK3A
        private static AnimatorController PrepareVRCAnimLayerTarget(Context ctx, Dictionary<AnimatorController, bool> clonedStatus, Dictionary<AnimatorController, AnimatorMerger.WriteDefaultsMode> writeDefaults, bool matchTargetWriteDefaults, VRCAvatarDescriptor.AnimLayerType animLayerType)
        {
            if (!ctx.AvatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDesc))
            {
                // not a vrc avatar
                return null;
            }

            // TODO: becareful this might a default animator which is not cloned + having a same hashcode
            var layerAnimator = VRCAnimUtils.GetAvatarLayerAnimator(avatarDesc, animLayerType);
            if (layerAnimator == null)
            {
                return null;
            }

            clonedStatus[layerAnimator] = true;
            AnimUtils.GetWriteDefaultCounts(layerAnimator, out var onCount, out var offCount);
            if (!matchTargetWriteDefaults)
            {
                writeDefaults[layerAnimator] = AnimatorMerger.WriteDefaultsMode.DoNothing;
            }
            else
            {
                if (onCount == 0 && offCount > 0)
                {
                    writeDefaults[layerAnimator] = AnimatorMerger.WriteDefaultsMode.Off;
                }
                else if (onCount > 0 && offCount == 0)
                {
                    writeDefaults[layerAnimator] = AnimatorMerger.WriteDefaultsMode.On;
                }
                else
                {
                    writeDefaults[layerAnimator] = AnimatorMerger.WriteDefaultsMode.DoNothing;
                }
            }

            return layerAnimator;
        }

        // TODO: this function is temporarily until reference tracker is implemented
        private static VRCAvatarDescriptor.CustomAnimLayer[] GetVRCMergedLayers(Dictionary<AnimatorController, AnimatorMerger> mergers, VRCAvatarDescriptor.CustomAnimLayer[] animLayers)
        {
            // create a new array and shallow copy
            var output = new VRCAvatarDescriptor.CustomAnimLayer[animLayers.Length];
            animLayers.CopyTo(output, 0);

            for (var i = 0; i < output.Length; i++)
            {
                if (!(output[i].animatorController is AnimatorController animCtrl))
                {
                    continue;
                }

                // get the merger of such layer
                if (mergers.TryGetValue(animCtrl, out var merger))
                {
                    output[i].animatorController = merger.Merge();
                    output[i].isDefault = false;

                    // add avatar mask for default gesture layer
                    if (output[i].type == VRCAvatarDescriptor.AnimLayerType.Gesture && output[i].isDefault)
                    {
                        var maskPath = AssetDatabase.GUIDToAssetPath(VRCGestureAvatarMaskGuid);
                        if (maskPath == null || maskPath == "")
                        {
                            throw new System.Exception("Unable to find VRC gesture avatar mask by GUID!");
                        }
                        output[i].mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
                    }
                }
            }

            return output;
        }

        private void WriteToVRCAvatarDescriptor(Context ctx, Dictionary<AnimatorController, AnimatorMerger> mergers)
        {
            if (!ctx.AvatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDesc))
            {
                // not a vrc avatar
                return;
            }

            avatarDesc.customizeAnimationLayers = true;

            // write merge layers
            avatarDesc.baseAnimationLayers = GetVRCMergedLayers(mergers, avatarDesc.baseAnimationLayers);
            avatarDesc.specialAnimationLayers = GetVRCMergedLayers(mergers, avatarDesc.specialAnimationLayers);
        }
#endif

        private static AnimatorMerger PrepareTargetMerger(Context ctx, Dictionary<AnimatorController, AnimatorMerger> mergers, AnimatorController targetController)
        {
            if (mergers.TryGetValue(targetController, out var merger))
            {
                return merger;
            }

            // this is cloned initially
            merger = mergers[targetController] = new AnimatorMerger(ctx, $"{targetController}_Merged");
            merger.AddAnimator("", targetController);

            return merger;
        }

        private static bool TryGetSourceController(Transform avatarRoot, DTManipulateAnimator comp, out Transform relativeRoot, out AnimatorController controller)
        {
            relativeRoot = null;
            controller = null;
            if (comp.SourceType == DTManipulateAnimator.SourceTypes.AnimatorController)
            {
                if (comp.PathMode == DTManipulateAnimator.PathModes.Relative)
                {
                    relativeRoot = comp.SourceRelativeRoot != null ? comp.SourceRelativeRoot : comp.transform;
                }
                else
                {
                    relativeRoot = avatarRoot;
                }
                controller = comp.SourceController;
                return true;
            }
            else if (comp.SourceType == DTManipulateAnimator.SourceTypes.Animator)
            {
                var animator = comp.SourceAnimator;
                if (animator == null && !comp.TryGetComponent(out animator))
                {
                    return false;
                }

                if (!(animator.runtimeAnimatorController is AnimatorController animCtrl))
                {
                    return false;
                }
                relativeRoot = comp.PathMode == DTManipulateAnimator.PathModes.Relative ? animator.transform : avatarRoot;
                controller = animCtrl;
                return true;
            }
            return false;
        }

        public override bool Invoke(Context ctx)
        {
            var clonedStatus = new Dictionary<AnimatorController, bool>();
            var mergers = new Dictionary<AnimatorController, AnimatorMerger>();
            var writeDefaults = new Dictionary<AnimatorController, AnimatorMerger.WriteDefaultsMode>();

            var comps = ctx.AvatarGameObject.GetComponentsInChildren<DTManipulateAnimator>(true);
            foreach (var comp in comps)
            {
                // TODO: not implemented
                if (comp.TargetType != DTManipulateAnimator.TargetTypes.VRCAnimLayer)
                {
                    ctx.Report.LogWarn(LogLabel, "Target types other than VRCAnimLayer are not implemented, skipping");
                    continue;
                }
                if (comp.ManipulateMode != DTManipulateAnimator.ManipulateModes.Add)
                {
                    ctx.Report.LogWarn(LogLabel, "Manipulate modes other than Add are not implemented, skipping");
                    continue;
                }

                AnimatorController targetController;
#if DT_VRCSDK3A
                targetController = PrepareVRCAnimLayerTarget(ctx, clonedStatus, writeDefaults, comp.MatchTargetWriteDefaults, comp.VRCTargetLayer);
#else
                targetController = null;
#endif
                if (targetController == null)
                {
                    ctx.Report.LogWarn(LogLabel, $"Could not prepare target controller for {comp.name}, skipping");
                    continue;
                }

                if (!TryGetSourceController(ctx.AvatarGameObject.transform, comp, out var relativeRoot, out var sourceCtrl))
                {
                    ctx.Report.LogWarn(LogLabel, $"Could not obtain source controller for {comp.name}, skipping");
                    continue;
                }

                var merger = PrepareTargetMerger(ctx, mergers, targetController);
                var rebasePath = "";
                if (relativeRoot != ctx.AvatarGameObject.transform)
                {
                    rebasePath = AnimationUtils.GetRelativePath(relativeRoot, ctx.AvatarGameObject.transform);
                }

                merger.AddAnimator(rebasePath, sourceCtrl);

                if (comp.SourceType == DTManipulateAnimator.SourceTypes.Animator &&
                    comp.RemoveSourceAnimator)
                {
                    Object.DestroyImmediate(comp.SourceAnimator);
                }
            }

#if DT_VRCSDK3A
            WriteToVRCAvatarDescriptor(ctx, mergers);
#endif

            return true;
        }

        public override bool Invoke(Context ctx, DTBaseComponent component, out List<DTBaseComponent> generatedComponents)
        {
            // TODO: caution on GetAvatarLayerAnimator returning default asset which is not cloned
            Debug.LogWarning("[DressingTools] Component apply on ManipulateAnimatorPass is not implemented yet.");
            generatedComponents = new List<DTBaseComponent>();
            return true;
        }
    }
}
