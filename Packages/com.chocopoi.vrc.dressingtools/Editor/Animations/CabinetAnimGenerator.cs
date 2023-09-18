/*
 * File: cabinetAnimGen.cs
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
using Chocopoi.DressingTools.Lib.Animations;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations
{
    internal class cabinetAnimGen
    {
        public const string LogLabel = "CabinetAnimGenerator";

        public static class MessageCode
        {
            // Warnings
            public const string IgnoredObjectHasNoSkinnedMeshRendererAttached = "cabinetAnimGen.msgCode.warn.ignoredObjectHasNoSkinnedMeshRendererAttached";
            public const string IgnoredObjectHasNoMeshAttached = "cabinetAnimGen.msgCode.warn.ignoredObjectHasNoMeshAttached";
            public const string IgnoredObjectHasNoSuchBlendshape = "cabinetAnimGen.msgCode.warn.ignoredObjectHasNoSuchBlendshape";
            public const string IgnoredAvatarToggleObjectNotFound = "cabinetAnimGen.msgCode.warn.ignoredAvatarToggleObjectNotFound";
            public const string IgnoredAvatarBlendshapeObjectNotFound = "cabinetAnimGen.msgCode.warn.ignoredAvatarBlendshapeObjectNotFound";
            public const string IgnoredCouldNotObtainAvatarBlendshapeOriginalValue = "cabinetAnimGen.msgCode.warn.ignoredCouldNotObtainAvatarBlendshapeOriginalValue";
            public const string IgnoredWearableToggleObjectNotFound = "cabinetAnimGen.msgCode.warn.ignoredWearableToggleObjectNotFound";
            public const string IgnoredWearableBlendshapeObjectNotFound = "cabinetAnimGen.msgCode.warn.ignoredWearableBlendshapeObjectNotFound";
            public const string IgnoredCouldNotObtainWearableBlendshapeOriginalValue = "cabinetAnimGen.msgCode.warn.ignoredCouldNotObtainWearableBlendshapeOriginalValue";
        }

        private DTReport _report;
        private GameObject _avatarObject;
        private GameObject _wearableObject;
        private CabinetAnimWearableModuleConfig _module;
        private List<IDynamicsProxy> _avatarDynamics;
        private List<IDynamicsProxy> _wearableDynamics;
        private IPathRemapper _pathRemapper;
        private bool _writeDefaults;

        public cabinetAnimGen(DTReport report, GameObject avatarObject, CabinetAnimWearableModuleConfig module, GameObject wearableObject, List<IDynamicsProxy> avatarDynamics, List<IDynamicsProxy> wearableDynamics, IPathRemapper pathRemapper, bool writeDefaults)
        {
            _report = report;
            _avatarObject = avatarObject;
            _module = module;
            _wearableObject = wearableObject;
            _avatarDynamics = avatarDynamics;
            _wearableDynamics = wearableDynamics;
            _pathRemapper = pathRemapper;
            _writeDefaults = writeDefaults;
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

        private Transform GetRemappedAvatarTransform(string avatarPath)
        {
            return _avatarObject.transform.Find(_pathRemapper.Remap(avatarPath));
        }

        private Transform GetRemappedWearableTransform(string wearablePath)
        {
            var basePath = AnimationUtils.GetRelativePath(_wearableObject.transform, _avatarObject.transform);
            return _avatarObject.transform.Find(_pathRemapper.Remap(basePath + "/" + wearablePath));
        }

        private void GenerateAvatarToggleAnimations(AnimationClip enableClip, AnimationClip disableClip, AnimationToggle[] toggles)
        {
            foreach (var toggle in toggles)
            {
                var obj = GetRemappedAvatarTransform(toggle.path);
                if (obj == null)
                {
                    DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredAvatarToggleObjectNotFound, toggle.path);
                }
                else
                {
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(enableClip, toggle.path, toggle.state);
                    if (!_writeDefaults)
                    {
                        AnimationUtils.SetSingleFrameGameObjectEnabledCurve(disableClip, toggle.path, obj.gameObject.activeSelf);
                    }
                }
            }
        }

        private void GenerateAvatarBlendshapeAnimations(AnimationClip enableClip, AnimationClip disableClip, AnimationBlendshapeValue[] blendshapes)
        {
            foreach (var blendshape in blendshapes)
            {
                var obj = GetRemappedAvatarTransform(blendshape.path);
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
                if (!_writeDefaults)
                {
                    // write the original value if not write defaults
                    AnimationUtils.SetSingleFrameBlendshapeCurve(disableClip, blendshape.path, blendshape.blendshapeName, originalValue);
                }
            }
        }

        private void GenerateWearableToggleAnimations(AnimationClip enableClip, AnimationClip disableClip, AnimationToggle[] toggles)
        {
            foreach (var toggle in toggles)
            {
                var obj = GetRemappedWearableTransform(toggle.path);
                if (obj == null)
                {
                    DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredWearableToggleObjectNotFound, toggle.path);
                }
                else
                {
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(enableClip, AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform), toggle.state);
                    if (!_writeDefaults)
                    {
                        AnimationUtils.SetSingleFrameGameObjectEnabledCurve(disableClip, AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform), obj.gameObject.activeSelf);
                    }
                }
            }
        }

        private void GenerateWearableBlendshapeAnimations(AnimationClip enableClip, AnimationClip disableClip, AnimationBlendshapeValue[] blendshapes)
        {
            foreach (var blendshape in blendshapes)
            {
                var obj = GetRemappedWearableTransform(blendshape.path);
                if (obj == null)
                {
                    DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredWearableBlendshapeObjectNotFound, _wearableObject.name, blendshape.path);
                    continue;
                }

                if (!TryGetBlendshapeValue(obj.gameObject, blendshape.blendshapeName, out var originalValue))
                {
                    DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredCouldNotObtainWearableBlendshapeOriginalValue, _wearableObject.name, blendshape.path);
                    continue;
                }

                AnimationUtils.SetSingleFrameBlendshapeCurve(enableClip, AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform), blendshape.blendshapeName, blendshape.value);
                if (!_writeDefaults)
                {
                    // write the original value if not write defaults
                    AnimationUtils.SetSingleFrameBlendshapeCurve(disableClip, AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform), blendshape.blendshapeName, originalValue);
                }
            }
        }

        public System.Tuple<AnimationClip, AnimationClip> GenerateWearAnimations()
        {
            var enableClip = new AnimationClip();
            var disableClip = new AnimationClip();

            // prevent unexpected behaviour
            if (!DTEditorUtils.IsGrandParent(_avatarObject.transform, _wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            // avatar toggles
            GenerateAvatarToggleAnimations(enableClip, disableClip, _module.avatarAnimationOnWear.toggles.ToArray());

            // wearable toggles
            GenerateWearableToggleAnimations(enableClip, disableClip, _module.wearableAnimationOnWear.toggles.ToArray());

            // dynamics
            var visitedDynamicsTransforms = new List<Transform>();
            foreach (var dynamics in _wearableDynamics)
            {
                if (!DTEditorUtils.IsGrandParent(_avatarObject.transform, dynamics.Transform))
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
                if (!_writeDefaults)
                {
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(disableClip, AnimationUtils.GetRelativePath(dynamics.Transform, _avatarObject.transform), false);
                }

                // mark as visited
                visitedDynamicsTransforms.Add(dynamics.Transform);
            }

            // avatar blendshapes
            GenerateAvatarBlendshapeAnimations(enableClip, disableClip, _module.avatarAnimationOnWear.blendshapes.ToArray());

            // wearable blendshapes
            GenerateWearableBlendshapeAnimations(enableClip, disableClip, _module.wearableAnimationOnWear.blendshapes.ToArray());

            return new System.Tuple<AnimationClip, AnimationClip>(enableClip, disableClip);
        }

        public Dictionary<WearableCustomizable, System.Tuple<AnimationClip, AnimationClip>> GenerateCustomizableToggleAnimations()
        {
            // prevent unexpected behaviour
            if (!DTEditorUtils.IsGrandParent(_avatarObject.transform, _wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            var dict = new Dictionary<WearableCustomizable, System.Tuple<AnimationClip, AnimationClip>>();

            foreach (var customizable in _module.wearableCustomizables)
            {
                var enableClip = new AnimationClip();
                var disableClip = new AnimationClip();

                // avatar required toggles
                GenerateAvatarToggleAnimations(enableClip, disableClip, customizable.avatarToggles.ToArray());

                // avatar required blendshapes
                GenerateAvatarBlendshapeAnimations(enableClip, disableClip, customizable.avatarBlendshapes.ToArray());

                if (customizable.type == WearableCustomizableType.Toggle)
                {
                    // wearable required blendshapes
                    GenerateWearableBlendshapeAnimations(enableClip, disableClip, customizable.wearableBlendshapes.ToArray());

                    // wearable toggle
                    GenerateWearableToggleAnimations(enableClip, disableClip, customizable.wearableToggles.ToArray());
                }
                else if (customizable.type == WearableCustomizableType.Blendshape)
                {
                    // wearable required toggle
                    GenerateWearableToggleAnimations(enableClip, disableClip, customizable.wearableToggles.ToArray());

                    // we only the toggles here, for radial blendshapes, we need to separate a layer to do that
                }

                dict.Add(customizable, new System.Tuple<AnimationClip, AnimationClip>(enableClip, disableClip));
            }

            return dict;
        }

        public Dictionary<WearableCustomizable, AnimationClip> GenerateCustomizableBlendshapeAnimations()
        {
            // prevent unexpected behaviour
            if (!DTEditorUtils.IsGrandParent(_avatarObject.transform, _wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            var dict = new Dictionary<WearableCustomizable, AnimationClip>();

            foreach (var customizable in _module.wearableCustomizables)
            {
                if (customizable.type == WearableCustomizableType.Blendshape)
                {
                    var clip = new AnimationClip();

                    foreach (var wearableBlendshape in customizable.wearableBlendshapes)
                    {
                        var obj = _wearableObject.transform.Find(wearableBlendshape.path);
                        if (obj == null)
                        {
                            DTReportUtils.LogWarnLocalized(_report, LogLabel, MessageCode.IgnoredWearableBlendshapeObjectNotFound, _wearableObject.name, wearableBlendshape.path);
                            continue;
                        }

                        AnimationUtils.SetLinearZeroToHundredBlendshapeCurve(clip, AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform), wearableBlendshape.blendshapeName);
                    }

                    dict.Add(customizable, clip);
                }
            }

            return dict;
        }
    }
}
