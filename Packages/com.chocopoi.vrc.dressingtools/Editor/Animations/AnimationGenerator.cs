﻿using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations
{
    public class AnimationGenerator
    {
        public const string LogLabel = "AnimationGenerator";

        public static class MessageCode
        {
            // Warnings
            public const string IgnoredObjectHasNoSkinnedMeshRendererAttached = "animationGenerator.msgCode.warn.ignoredObjectHasNoSkinnedMeshRendererAttached";
            public const string IgnoredObjectHasNoMeshAttached = "animationGenerator.msgCode.warn.ignoredObjectHasNoMeshAttached";
            public const string IgnoredObjectHasNoSuchBlendshape = "animationGenerator.msgCode.warn.ignoredObjectHasNoSuchBlendshape";
            public const string IgnoredAvatarToggleObjectNotFound = "animationGenerator.msgCode.warn.ignoredAvatarToggleObjectNotFound";
            public const string IgnoredAvatarBlendshapeObjectNotFound = "animationGenerator.msgCode.warn.ignoredAvatarBlendshapeObjectNotFound";
            public const string IgnoredCouldNotObtainAvatarBlendshapeOriginalValue = "animationGenerator.msgCode.warn.ignoredCouldNotObtainAvatarBlendshapeOriginalValue";
            public const string IgnoredWearableToggleObjectNotFound = "animationGenerator.msgCode.warn.ignoredWearableToggleObjectNotFound";
            public const string IgnoredWearableBlendshapeObjectNotFound = "animationGenerator.msgCode.warn.ignoredWearableBlendshapeObjectNotFound";
            public const string IgnoredCouldNotObtainWearableBlendshapeOriginalValue = "animationGenerator.msgCode.warn.ignoredCouldNotObtainWearableBlendshapeOriginalValue";
        }

        private DTReport report;

        private GameObject avatarObject;

        private GameObject wearableObject;

        private AnimationGenerationModule module;

        private List<IDynamicsProxy> wearableDynamics;

        public AnimationGenerator(DTReport report, GameObject avatarObject, AnimationGenerationModule module, GameObject wearableObject, List<IDynamicsProxy> wearableDynamics)
        {
            this.report = report;
            this.avatarObject = avatarObject;
            this.module = module;
            this.wearableObject = wearableObject;
            this.wearableDynamics = wearableDynamics;
        }

        private bool TryGetBlendshapeValue(GameObject obj, string blendshapeName, out float value)
        {
            value = -1.0f;

            SkinnedMeshRenderer smr;
            if ((smr = obj.GetComponent<SkinnedMeshRenderer>()) == null)
            {
                report.LogWarnLocalized(LogLabel, MessageCode.IgnoredObjectHasNoSkinnedMeshRendererAttached, obj.name);
                return false;
            }

            Mesh mesh;
            if ((mesh = smr.sharedMesh) == null)
            {
                report.LogWarnLocalized(LogLabel, MessageCode.IgnoredObjectHasNoMeshAttached, obj.name);
                return false;
            }

            int blendshapeIndex;
            if ((blendshapeIndex = mesh.GetBlendShapeIndex(blendshapeName)) == -1)
            {
                report.LogWarnLocalized(LogLabel, MessageCode.IgnoredObjectHasNoSuchBlendshape, obj.name, blendshapeName);
                return false;
            }

            value = smr.GetBlendShapeWeight(blendshapeIndex);
            return true;
        }

        private void GenerateAvatarToggleAnimations(AnimationClip enableClip, AnimationClip disableClip, DTAnimationToggle[] toggles, bool writeDefaults)
        {
            foreach (var toggle in toggles)
            {
                var obj = avatarObject.transform.Find(toggle.path);
                if (obj == null)
                {
                    report.LogWarnLocalized(LogLabel, MessageCode.IgnoredAvatarToggleObjectNotFound, toggle.path);
                }
                else
                {
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(enableClip, toggle.path, toggle.state);
                    if (!writeDefaults)
                    {
                        AnimationUtils.SetSingleFrameGameObjectEnabledCurve(disableClip, toggle.path, obj.gameObject.activeSelf);
                    }
                }
            }
        }

        private void GenerateAvatarBlendshapeAnimations(AnimationClip enableClip, AnimationClip disableClip, DTAnimationBlendshapeValue[] blendshapes, bool writeDefaults)
        {
            foreach (var blendshape in blendshapes)
            {
                var obj = avatarObject.transform.Find(blendshape.path);
                if (obj == null)
                {
                    report.LogWarnLocalized(LogLabel, MessageCode.IgnoredAvatarBlendshapeObjectNotFound, avatarObject.name, blendshape.path);
                    continue;
                }

                if (!TryGetBlendshapeValue(obj.gameObject, blendshape.blendshapeName, out var originalValue))
                {
                    report.LogWarnLocalized(LogLabel, MessageCode.IgnoredCouldNotObtainAvatarBlendshapeOriginalValue, avatarObject.name, blendshape.path);
                    continue;
                }

                AnimationUtils.SetSingleFrameBlendshapeCurve(enableClip, blendshape.path, blendshape.blendshapeName, blendshape.value);
                if (!writeDefaults)
                {
                    // write the original value if not write defaults
                    AnimationUtils.SetSingleFrameBlendshapeCurve(disableClip, blendshape.path, blendshape.blendshapeName, originalValue);
                }
            }
        }

        private void GenerateWearableToggleAnimations(AnimationClip enableClip, AnimationClip disableClip, DTAnimationToggle[] toggles, bool writeDefaults)
        {
            foreach (var toggle in toggles)
            {
                var obj = wearableObject.transform.Find(toggle.path);
                if (obj == null)
                {
                    report.LogWarnLocalized(LogLabel, MessageCode.IgnoredWearableToggleObjectNotFound, toggle.path);
                }
                else
                {
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(enableClip, AnimationUtils.GetRelativePath(obj.transform, avatarObject.transform), toggle.state);
                    if (!writeDefaults)
                    {
                        AnimationUtils.SetSingleFrameGameObjectEnabledCurve(disableClip, AnimationUtils.GetRelativePath(obj.transform, avatarObject.transform), obj.gameObject.activeSelf);
                    }
                }
            }
        }

        private void GenerateWearableBlendshapeAnimations(AnimationClip enableClip, AnimationClip disableClip, DTAnimationBlendshapeValue[] blendshapes, bool writeDefaults)
        {
            foreach (var blendshape in blendshapes)
            {
                var obj = wearableObject.transform.Find(blendshape.path);
                if (obj == null)
                {
                    report.LogWarnLocalized(LogLabel, MessageCode.IgnoredAvatarBlendshapeObjectNotFound, wearableObject.name, blendshape.path);
                }

                if (!TryGetBlendshapeValue(obj.gameObject, blendshape.blendshapeName, out var originalValue))
                {
                    report.LogWarnLocalized(LogLabel, MessageCode.IgnoredCouldNotObtainWearableBlendshapeOriginalValue, wearableObject.name, blendshape.path);
                    continue;
                }

                AnimationUtils.SetSingleFrameBlendshapeCurve(enableClip, AnimationUtils.GetRelativePath(obj.transform, avatarObject.transform), blendshape.blendshapeName, blendshape.value);
                if (!writeDefaults)
                {
                    // write the original value if not write defaults
                    AnimationUtils.SetSingleFrameBlendshapeCurve(disableClip, AnimationUtils.GetRelativePath(obj.transform, avatarObject.transform), blendshape.blendshapeName, originalValue);
                }
            }
        }

        public System.Tuple<AnimationClip, AnimationClip> GenerateWearAnimations(bool writeDefaults)
        {
            var enableClip = new AnimationClip();
            var disableClip = new AnimationClip();

            // prevent unexpected behaviour
            if (!DTRuntimeUtils.IsGrandParent(avatarObject.transform, wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            // avatar toggles
            GenerateAvatarToggleAnimations(enableClip, disableClip, module.avatarAnimationOnWear.toggles, writeDefaults);

            // wearable toggles
            GenerateWearableToggleAnimations(enableClip, disableClip, module.wearableAnimationOnWear.toggles, writeDefaults);

            // dynamics
            var visitedDynamicsTransforms = new List<Transform>();
            foreach (var dynamics in wearableDynamics)
            {
                if (!DTRuntimeUtils.IsGrandParent(avatarObject.transform, dynamics.Transform))
                {
                    throw new System.Exception(string.Format("Dynamics {0} is not inside avatar {1}, aborting", dynamics.Transform.name, avatarObject.name));
                }

                if (visitedDynamicsTransforms.Contains(dynamics.Transform))
                {
                    // skip duplicates since it's meaningless
                    continue;
                }

                // enable/disable dynamics object
                AnimationUtils.SetSingleFrameGameObjectEnabledCurve(enableClip, AnimationUtils.GetRelativePath(dynamics.Transform, avatarObject.transform), true);
                if (!writeDefaults)
                {
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(disableClip, AnimationUtils.GetRelativePath(dynamics.Transform, avatarObject.transform), false);
                }

                // mark as visited
                visitedDynamicsTransforms.Add(dynamics.Transform);
            }

            // avatar blendshapes
            GenerateAvatarBlendshapeAnimations(enableClip, disableClip, module.avatarAnimationOnWear.blendshapes, writeDefaults);

            // wearable blendshapes
            GenerateWearableBlendshapeAnimations(enableClip, disableClip, module.wearableAnimationOnWear.blendshapes, writeDefaults);

            return new System.Tuple<AnimationClip, AnimationClip>(enableClip, disableClip);
        }

        public Dictionary<DTWearableCustomizable, System.Tuple<AnimationClip, AnimationClip>> GenerateCustomizableAnimations(bool writeDefaults)
        {
            // prevent unexpected behaviour
            if (!DTRuntimeUtils.IsGrandParent(avatarObject.transform, wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            var dict = new Dictionary<DTWearableCustomizable, System.Tuple<AnimationClip, AnimationClip>>();

            foreach (var customizable in module.wearableCustomizables)
            {
                var enableClip = new AnimationClip();
                var disableClip = new AnimationClip();

                if (customizable.type == DTWearableCustomizableType.Toggle)
                {
                    // avatar required toggles
                    GenerateAvatarToggleAnimations(enableClip, disableClip, customizable.avatarRequiredToggles, writeDefaults);

                    // avatar required blendshapes
                    GenerateAvatarBlendshapeAnimations(enableClip, disableClip, customizable.avatarRequiredBlendshapes, writeDefaults);

                    // wearable required blendshapes
                    GenerateWearableBlendshapeAnimations(enableClip, disableClip, customizable.wearableBlendshapes, writeDefaults);

                    // wearable toggle
                    GenerateWearableToggleAnimations(enableClip, disableClip, customizable.wearableToggles, writeDefaults);
                }
                else if (customizable.type == DTWearableCustomizableType.Blendshape)
                {
                    // TODO: we need to create a curve from 0.0f to 100.0f to handle this type of customizable
                    throw new System.NotImplementedException();
                }

                dict.Add(customizable, new System.Tuple<AnimationClip, AnimationClip>(enableClip, disableClip));
            }

            return dict;
        }
    }
}
