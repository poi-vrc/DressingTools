/*
 * File: VRCAnimationRemapper.cs
 * Project: DressingTools
 * Created Date: Tuesday, September 5th 2023, 10:17:55 pm
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

#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Lib.Animations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal class VRCAnimationRemapper
    {
        private class AnimationClipContainer
        {
            public AnimationClip clip;
            public Action<AnimationClip> dispatchFunc;

            public AnimationClipContainer()
            {
                clip = null;
                dispatchFunc = null;
            }
        }

        private VRCAvatarDescriptor _avatarDescriptor;
        private IPathRemapper _pathRemapper;
        private List<AnimationClipContainer> _clipContainers;
        private Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorController> _animatorCopies;

        public VRCAnimationRemapper(VRCAvatarDescriptor avatarDescriptor, IPathRemapper pathRemapper)
        {
            _avatarDescriptor = avatarDescriptor;
            _pathRemapper = pathRemapper;
            _clipContainers = new List<AnimationClipContainer>();
            _animatorCopies = new Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorController>();

            // scan for existing animation clips
            ScanAnimLayers(_avatarDescriptor.baseAnimationLayers);
            ScanAnimLayers(_avatarDescriptor.specialAnimationLayers);
        }

        public void PerformRemapping()
        {
            foreach (var clipContainer in _clipContainers)
            {
                var oldClip = clipContainer.clip;
                var remapped = false;

                var newClip = new AnimationClip()
                {
                    name = oldClip.name,
                    legacy = oldClip.legacy,
                    frameRate = oldClip.frameRate,
                    localBounds = oldClip.localBounds,
                    wrapMode = oldClip.wrapMode
                };
                AnimationUtility.SetAnimationClipSettings(newClip, AnimationUtility.GetAnimationClipSettings(oldClip));

                var curveBindings = AnimationUtility.GetCurveBindings(oldClip);
                foreach (var curveBinding in curveBindings)
                {
                    var avoidContainerBones = curveBinding.type == typeof(Transform);
                    var newPath = _pathRemapper.Remap(curveBinding.path, avoidContainerBones);

                    remapped |= newPath != curveBinding.path;

                    newClip.SetCurve(newPath, curveBinding.type, curveBinding.propertyName, AnimationUtility.GetEditorCurve(oldClip, curveBinding));
                }

                var objRefBindings = AnimationUtility.GetObjectReferenceCurveBindings(oldClip);
                foreach (var objRefBinding in objRefBindings)
                {
                    var newPath = _pathRemapper.Remap(objRefBinding.path, false);

                    remapped |= newPath != objRefBinding.path;

                    var newObjRefBinding = objRefBinding;
                    newObjRefBinding.path = newPath;
                    AnimationUtility.SetObjectReferenceCurve(newClip, newObjRefBinding, AnimationUtility.GetObjectReferenceCurve(oldClip, objRefBinding));
                }

                if (remapped)
                {
                    AssetDatabase.CreateAsset(newClip, CabinetApplier.GeneratedAssetsPath + "/cpDT_VRC_Remapped_Clip_" + oldClip.name + ".anim");
                    clipContainer.dispatchFunc(newClip);
                }
            }

            ApplyAnimLayers(_avatarDescriptor.baseAnimationLayers);
            ApplyAnimLayers(_avatarDescriptor.specialAnimationLayers);
        }

        private void ApplyAnimLayers(VRCAvatarDescriptor.CustomAnimLayer[] animLayers)
        {
            for (var i = 0; i < animLayers.Length; i++)
            {
                if (_animatorCopies.TryGetValue(animLayers[i].type, out var copy))
                {
                    animLayers[i].isDefault = false;
                    animLayers[i].animatorController = copy;
                }
            }
        }

        private void ScanAnimLayers(VRCAvatarDescriptor.CustomAnimLayer[] animLayers)
        {
            foreach (var animLayer in animLayers)
            {
                if (animLayer.isDefault || animLayer.animatorController == null || !(animLayer.animatorController is AnimatorController))
                {
                    continue;
                }

                var controller = (AnimatorController)animLayer.animatorController;

                // create a copy for operations
                var copiedPath = string.Format("{0}/cpDT_VRC_Remapped_AnimLayer_{1}.controller", CabinetApplier.GeneratedAssetsPath, animLayer.type.ToString());
                var copiedController = DTEditorUtils.CopyAssetToPathAndImport<AnimatorController>(controller, copiedPath);
                _animatorCopies[animLayer.type] = copiedController;

                foreach (var layer in copiedController.layers)
                {
                    var stack = new Stack<AnimatorStateMachine>();
                    stack.Push(layer.stateMachine);

                    while (stack.Count > 0)
                    {
                        var stateMachine = stack.Pop();

                        foreach (var state in stateMachine.states)
                        {
                            ScanMotion(state.state.motion, (AnimationClip clip) => state.state.motion = clip);
                        }

                        foreach (var childStateMachine in stateMachine.stateMachines)
                        {
                            stack.Push(childStateMachine.stateMachine);
                        }
                    }
                }
            }
        }

        private void ScanMotion(Motion motion, Action<AnimationClip> dispatchFunc)
        {
            if (motion == null || VRCEditorUtils.IsProxyAnimation(motion))
            {
                return;
            }

            if (motion is AnimationClip clip)
            {
                _clipContainers.Add(new AnimationClipContainer()
                {
                    clip = clip,
                    dispatchFunc = dispatchFunc
                });
            }
            else if (motion is BlendTree tree)
            {
                for (var i = 0; i < tree.children.Length; i++)
                {
                    ScanMotion(motion, (AnimationClip newClip) => tree.children[i].motion = newClip);
                }
            }
        }
    }
}
#endif
