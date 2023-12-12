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
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingFramework.Proxy;
using Chocopoi.DressingTools.Api.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.Localization;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations
{
    internal class CabinetAnimGenerator
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;
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

        private DKReport _report;
        private GameObject _avatarObject;
        private GameObject _wearableObject;
        private CabinetAnimWearableModuleConfig _module;
        private List<IDynamicsProxy> _avatarDynamics;
        private List<IDynamicsProxy> _wearableDynamics;
        private PathRemapper _pathRemapper;
        private bool _writeDefaults;

        public CabinetAnimGenerator(DKReport report, GameObject avatarObject, CabinetAnimWearableModuleConfig module, GameObject wearableObject, List<IDynamicsProxy> avatarDynamics, List<IDynamicsProxy> wearableDynamics, PathRemapper pathRemapper, bool writeDefaults)
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
                _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredObjectHasNoSkinnedMeshRendererAttached, obj.name);
                return false;
            }

            Mesh mesh;
            if ((mesh = smr.sharedMesh) == null)
            {
                _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredObjectHasNoMeshAttached, obj.name);
                return false;
            }

            int blendshapeIndex;
            if ((blendshapeIndex = mesh.GetBlendShapeIndex(blendshapeName)) == -1)
            {
                _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredObjectHasNoSuchBlendshape, obj.name, blendshapeName);
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

        private void GenerateAvatarToggleAnimations(AnimationClip enableClip, AnimationClip disableClip, CabinetAnimWearableModuleConfig.Toggle[] toggles)
        {
            foreach (var toggle in toggles)
            {
                var obj = GetRemappedAvatarTransform(toggle.path);
                if (obj == null)
                {
                    _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredAvatarToggleObjectNotFound, toggle.path);
                }
                else
                {
                    var remappedPath = AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform);
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(enableClip, remappedPath, toggle.state);
                    if (!_writeDefaults)
                    {
                        AnimationUtils.SetSingleFrameGameObjectEnabledCurve(disableClip, remappedPath, obj.gameObject.activeSelf);
                    }
                }
            }
        }

        private void GenerateAvatarBlendshapeAnimations(AnimationClip enableClip, AnimationClip disableClip, CabinetAnimWearableModuleConfig.BlendshapeValue[] blendshapes)
        {
            foreach (var blendshape in blendshapes)
            {
                var obj = GetRemappedAvatarTransform(blendshape.path);
                if (obj == null)
                {
                    _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredAvatarBlendshapeObjectNotFound, _avatarObject.name, blendshape.path);
                    continue;
                }

                if (!TryGetBlendshapeValue(obj.gameObject, blendshape.blendshapeName, out var originalValue))
                {
                    _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredCouldNotObtainAvatarBlendshapeOriginalValue, _avatarObject.name, blendshape.path);
                    continue;
                }

                var remappedPath = AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform);
                AnimationUtils.SetSingleFrameBlendshapeCurve(enableClip, remappedPath, blendshape.blendshapeName, blendshape.value);
                if (!_writeDefaults)
                {
                    // write the original value if not write defaults
                    AnimationUtils.SetSingleFrameBlendshapeCurve(disableClip, remappedPath, blendshape.blendshapeName, originalValue);
                }
            }
        }

        private void GenerateWearableToggleAnimations(AnimationClip enableClip, AnimationClip disableClip, CabinetAnimWearableModuleConfig.Toggle[] toggles)
        {
            foreach (var toggle in toggles)
            {
                var obj = GetRemappedWearableTransform(toggle.path);
                if (obj == null)
                {
                    _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredWearableToggleObjectNotFound, toggle.path);
                }
                else
                {
                    var remappedPath = AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform);
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(enableClip, remappedPath, toggle.state);
                    if (!_writeDefaults)
                    {
                        AnimationUtils.SetSingleFrameGameObjectEnabledCurve(disableClip, remappedPath, obj.gameObject.activeSelf);
                    }
                }
            }
        }

        private void GenerateWearableBlendshapeAnimations(AnimationClip enableClip, AnimationClip disableClip, CabinetAnimWearableModuleConfig.BlendshapeValue[] blendshapes)
        {
            foreach (var blendshape in blendshapes)
            {
                var obj = GetRemappedWearableTransform(blendshape.path);
                if (obj == null)
                {
                    _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredWearableBlendshapeObjectNotFound, _wearableObject.name, blendshape.path);
                    continue;
                }

                if (!TryGetBlendshapeValue(obj.gameObject, blendshape.blendshapeName, out var originalValue))
                {
                    _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredCouldNotObtainWearableBlendshapeOriginalValue, _wearableObject.name, blendshape.path);
                    continue;
                }

                var remappedPath = AnimationUtils.GetRelativePath(obj.transform, _avatarObject.transform);
                AnimationUtils.SetSingleFrameBlendshapeCurve(enableClip, remappedPath, blendshape.blendshapeName, blendshape.value);
                if (!_writeDefaults)
                {
                    // write the original value if not write defaults
                    AnimationUtils.SetSingleFrameBlendshapeCurve(disableClip, remappedPath, blendshape.blendshapeName, originalValue);
                }
            }
        }

        public System.Tuple<AnimationClip, AnimationClip> GenerateWearAnimations()
        {
            var enableClip = new AnimationClip();
            var disableClip = new AnimationClip();

            // prevent unexpected behaviour
            if (!DKEditorUtils.IsGrandParent(_avatarObject.transform, _wearableObject.transform))
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
                if (!DKEditorUtils.IsGrandParent(_avatarObject.transform, dynamics.Transform))
                {
                    throw new System.Exception(string.Format("Dynamics {0} is not inside avatar {1}, aborting", dynamics.Transform.name, _avatarObject.name));
                }

                if (visitedDynamicsTransforms.Contains(dynamics.Transform))
                {
                    // skip duplicates since it's meaningless
                    continue;
                }

                // enable/disable dynamics object
                var remappedPath = AnimationUtils.GetRelativePath(dynamics.Transform, _avatarObject.transform);
                AnimationUtils.SetSingleFrameComponentEnabledCurve(enableClip, remappedPath, dynamics.Component.GetType(), true);
                if (!_writeDefaults)
                {
                    AnimationUtils.SetSingleFrameComponentEnabledCurve(disableClip, remappedPath, dynamics.Component.GetType(), false);
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

        public Dictionary<CabinetAnimWearableModuleConfig.Customizable, System.Tuple<AnimationClip, AnimationClip>> GenerateCustomizableToggleAnimations()
        {
            // prevent unexpected behaviour
            if (!DKEditorUtils.IsGrandParent(_avatarObject.transform, _wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            var dict = new Dictionary<CabinetAnimWearableModuleConfig.Customizable, System.Tuple<AnimationClip, AnimationClip>>();

            foreach (var customizable in _module.wearableCustomizables)
            {
                var enableClip = new AnimationClip();
                var disableClip = new AnimationClip();

                // avatar required toggles
                GenerateAvatarToggleAnimations(enableClip, disableClip, customizable.avatarToggles.ToArray());

                // avatar required blendshapes
                GenerateAvatarBlendshapeAnimations(enableClip, disableClip, customizable.avatarBlendshapes.ToArray());

                if (customizable.type == CabinetAnimWearableModuleConfig.CustomizableType.Toggle)
                {
                    // wearable required blendshapes
                    GenerateWearableBlendshapeAnimations(enableClip, disableClip, customizable.wearableBlendshapes.ToArray());

                    // wearable toggle
                    GenerateWearableToggleAnimations(enableClip, disableClip, customizable.wearableToggles.ToArray());
                }

                dict.Add(customizable, new System.Tuple<AnimationClip, AnimationClip>(enableClip, disableClip));
            }

            return dict;
        }

        public Dictionary<CabinetAnimWearableModuleConfig.Customizable, AnimationClip> GenerateCustomizableBlendshapeAnimations()
        {
            // prevent unexpected behaviour
            if (!DKEditorUtils.IsGrandParent(_avatarObject.transform, _wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            var dict = new Dictionary<CabinetAnimWearableModuleConfig.Customizable, AnimationClip>();

            foreach (var customizable in _module.wearableCustomizables)
            {
                if (customizable.type == CabinetAnimWearableModuleConfig.CustomizableType.Blendshape)
                {
                    var clip = new AnimationClip();

                    foreach (var wearableBlendshape in customizable.wearableBlendshapes)
                    {
                        var obj = _wearableObject.transform.Find(wearableBlendshape.path);
                        if (obj == null)
                        {
                            _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredWearableBlendshapeObjectNotFound, _wearableObject.name, wearableBlendshape.path);
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
