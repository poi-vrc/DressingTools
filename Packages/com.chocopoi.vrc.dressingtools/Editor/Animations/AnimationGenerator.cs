/*
 * File: AnimationGenerator.cs
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
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations
{
    internal class AnimationGenerator
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

        private DTReport _report;

        private GameObject _avatarObject;

        private GameObject _wearableObject;

        private AnimationGenerationModuleConfig _module;

        private List<IDynamicsProxy> _wearableDynamics;

        public AnimationGenerator(DTReport report, GameObject avatarObject, AnimationGenerationModuleConfig module, GameObject wearableObject, List<IDynamicsProxy> wearableDynamics)
        {
            _report = report;
            _avatarObject = avatarObject;
            _module = module;
            _wearableObject = wearableObject;
            _wearableDynamics = wearableDynamics;
        }

        private bool TryGetBlendshapeValue(GameObject obj, string blendshapeName, out float value)
        {
            value = -1.0f;

            SkinnedMeshRenderer smr;
            if ((smr = obj.GetComponent<SkinnedMeshRenderer>()) == null)
            {
                DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredObjectHasNoSkinnedMeshRendererAttached, obj.name);
                return false;
            }

            Mesh mesh;
            if ((mesh = smr.sharedMesh) == null)
            {
                DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredObjectHasNoMeshAttached, obj.name);
                return false;
            }

            int blendshapeIndex;
            if ((blendshapeIndex = mesh.GetBlendShapeIndex(blendshapeName)) == -1)
            {
                DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredObjectHasNoSuchBlendshape, obj.name, blendshapeName);
                return false;
            }

            value = smr.GetBlendShapeWeight(blendshapeIndex);
            return true;
        }

        private void GenerateAvatarToggleAnimations(AnimationClip enableClip, AnimationClip disableClip, AnimationToggle[] toggles, bool writeDefaults)
        {
            foreach (var toggle in toggles)
            {
                var obj = _avatarObject.transform.Find(toggle.path);
                if (obj == null)
                {
                    DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredAvatarToggleObjectNotFound, toggle.path);
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

        private void GenerateAvatarBlendshapeAnimations(AnimationClip enableClip, AnimationClip disableClip, AnimationBlendshapeValue[] blendshapes, bool writeDefaults)
        {
            foreach (var blendshape in blendshapes)
            {
                var obj = _avatarObject.transform.Find(blendshape.path);
                if (obj == null)
                {
                    DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredAvatarBlendshapeObjectNotFound, _avatarObject.name, blendshape.path);
                    continue;
                }

                if (!TryGetBlendshapeValue(obj.gameObject, blendshape.blendshapeName, out var originalValue))
                {
                    DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredCouldNotObtainAvatarBlendshapeOriginalValue, _avatarObject.name, blendshape.path);
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

        private void GenerateWearableToggleAnimations(AnimationClip enableClip, AnimationClip disableClip, AnimationToggle[] toggles, bool writeDefaults)
        {
            foreach (var toggle in toggles)
            {
                var obj = _wearableObject.transform.Find(toggle.path);
                if (obj == null)
                {
                    DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredWearableToggleObjectNotFound, toggle.path);
                }
                else
                {
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(enableClip, AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform), toggle.state);
                    if (!writeDefaults)
                    {
                        AnimationUtils.SetSingleFrameGameObjectEnabledCurve(disableClip, AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform), obj.gameObject.activeSelf);
                    }
                }
            }
        }

        private void GenerateWearableBlendshapeAnimations(AnimationClip enableClip, AnimationClip disableClip, AnimationBlendshapeValue[] blendshapes, bool writeDefaults)
        {
            foreach (var blendshape in blendshapes)
            {
                var obj = _wearableObject.transform.Find(blendshape.path);
                if (obj == null)
                {
                    DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredAvatarBlendshapeObjectNotFound, _wearableObject.name, blendshape.path);
                }

                if (!TryGetBlendshapeValue(obj.gameObject, blendshape.blendshapeName, out var originalValue))
                {
                    DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredCouldNotObtainWearableBlendshapeOriginalValue, _wearableObject.name, blendshape.path);
                    continue;
                }

                AnimationUtils.SetSingleFrameBlendshapeCurve(enableClip, AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform), blendshape.blendshapeName, blendshape.value);
                if (!writeDefaults)
                {
                    // write the original value if not write defaults
                    AnimationUtils.SetSingleFrameBlendshapeCurve(disableClip, AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform), blendshape.blendshapeName, originalValue);
                }
            }
        }

        public System.Tuple<AnimationClip, AnimationClip> GenerateWearAnimations(bool writeDefaults)
        {
            var enableClip = new AnimationClip();
            var disableClip = new AnimationClip();

            // prevent unexpected behaviour
            if (!DTRuntimeUtils.IsGrandParent(_avatarObject.transform, _wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            // avatar toggles
            GenerateAvatarToggleAnimations(enableClip, disableClip, _module.avatarAnimationOnWear.toggles.ToArray(), writeDefaults);

            // wearable toggles
            GenerateWearableToggleAnimations(enableClip, disableClip, _module.wearableAnimationOnWear.toggles.ToArray(), writeDefaults);

            // dynamics
            var visitedDynamicsTransforms = new List<Transform>();
            foreach (var dynamics in _wearableDynamics)
            {
                if (!DTRuntimeUtils.IsGrandParent(_avatarObject.transform, dynamics.Transform))
                {
                    throw new System.Exception(string.Format("Dynamics {0} is not inside avatar {1}, aborting", dynamics.Transform.name, _avatarObject.name));
                }

                if (visitedDynamicsTransforms.Contains(dynamics.Transform))
                {
                    // skip duplicates since it's meaningless
                    continue;
                }

                // enable/disable dynamics object
                AnimationUtils.SetSingleFrameGameObjectEnabledCurve(enableClip, AnimationUtils.GetRelativePath(dynamics.Transform, _avatarObject.transform), true);
                if (!writeDefaults)
                {
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(disableClip, AnimationUtils.GetRelativePath(dynamics.Transform, _avatarObject.transform), false);
                }

                // mark as visited
                visitedDynamicsTransforms.Add(dynamics.Transform);
            }

            // avatar blendshapes
            GenerateAvatarBlendshapeAnimations(enableClip, disableClip, _module.avatarAnimationOnWear.blendshapes.ToArray(), writeDefaults);

            // wearable blendshapes
            GenerateWearableBlendshapeAnimations(enableClip, disableClip, _module.wearableAnimationOnWear.blendshapes.ToArray(), writeDefaults);

            return new System.Tuple<AnimationClip, AnimationClip>(enableClip, disableClip);
        }

        public Dictionary<WearableCustomizable, System.Tuple<AnimationClip, AnimationClip>> GenerateCustomizableAnimations(bool writeDefaults)
        {
            // prevent unexpected behaviour
            if (!DTRuntimeUtils.IsGrandParent(_avatarObject.transform, _wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            var dict = new Dictionary<WearableCustomizable, System.Tuple<AnimationClip, AnimationClip>>();

            foreach (var customizable in _module.wearableCustomizables)
            {
                var enableClip = new AnimationClip();
                var disableClip = new AnimationClip();

                if (customizable.type == WearableCustomizableType.Toggle)
                {
                    // avatar required toggles
                    GenerateAvatarToggleAnimations(enableClip, disableClip, customizable.avatarRequiredToggles.ToArray(), writeDefaults);

                    // avatar required blendshapes
                    GenerateAvatarBlendshapeAnimations(enableClip, disableClip, customizable.avatarRequiredBlendshapes.ToArray(), writeDefaults);

                    // wearable required blendshapes
                    GenerateWearableBlendshapeAnimations(enableClip, disableClip, customizable.wearableBlendshapes.ToArray(), writeDefaults);

                    // wearable toggle
                    GenerateWearableToggleAnimations(enableClip, disableClip, customizable.wearableToggles.ToArray(), writeDefaults);
                }
                else if (customizable.type == WearableCustomizableType.Blendshape)
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
