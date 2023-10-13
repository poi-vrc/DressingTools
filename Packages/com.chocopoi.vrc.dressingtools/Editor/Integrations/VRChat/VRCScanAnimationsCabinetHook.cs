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

#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Context;
using Chocopoi.DressingFramework.Extensibility.Plugin;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal class VRCScanAnimationsCabinetHook : CabinetHookBase
    {
        [ExcludeFromCodeCoverage]
        public override string FriendlyName => "VRChat Scan Animations";

        [ExcludeFromCodeCoverage]
        public override CabinetApplyConstraint Constraint =>
            ApplyAtStage(CabinetApplyStage.Analyzing, CabinetHookStageRunOrder.After)
                .Build();

        private Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorController> _animatorCopies = new Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorController>();

        private void ScanAnimLayers(ApplyCabinetContext cabCtx, VRCAvatarDescriptor.CustomAnimLayer[] animLayers)
        {
            var dtCabCtx = cabCtx.Extra<DKCabinetContext>();

            foreach (var animLayer in animLayers)
            {
                if (animLayer.isDefault || animLayer.animatorController == null || !(animLayer.animatorController is AnimatorController))
                {
                    continue;
                }

                var controller = (AnimatorController)animLayer.animatorController;

                // create a copy for operations
                var copiedPath = cabCtx.MakeUniqueAssetPath($"VRC_AnimLayer_{animLayer.type}.controller");
                var copiedController = DTEditorUtils.CopyAssetToPathAndImport<AnimatorController>(controller, copiedPath);
                _animatorCopies[animLayer.type] = copiedController;

                var visitedMotions = new HashSet<Motion>();

                foreach (var layer in copiedController.layers)
                {
                    var stack = new Stack<AnimatorStateMachine>();
                    stack.Push(layer.stateMachine);

                    while (stack.Count > 0)
                    {
                        var stateMachine = stack.Pop();

                        foreach (var state in stateMachine.states)
                        {
                            cabCtx.animationStore.RegisterMotion(state.state.motion, (AnimationClip clip) => state.state.motion = clip, (Motion m) => !VRCEditorUtils.IsProxyAnimation(m), visitedMotions);
                        }

                        foreach (var childStateMachine in stateMachine.stateMachines)
                        {
                            stack.Push(childStateMachine.stateMachine);
                        }
                    }
                }
            }
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

        public override bool Invoke(ApplyCabinetContext cabCtx)
        {
            // get the avatar descriptor
            if (!cabCtx.avatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDescriptor))
            {
                // not a vrc avatar
                return true;
            }

            _animatorCopies.Clear();
            ScanAnimLayers(cabCtx, avatarDescriptor.baseAnimationLayers);
            ScanAnimLayers(cabCtx, avatarDescriptor.specialAnimationLayers);
            ApplyAnimLayers(avatarDescriptor.baseAnimationLayers);
            ApplyAnimLayers(avatarDescriptor.specialAnimationLayers);
            // cabCtx.animationStore.Write += () =>
            // {
            // };

            return true;
        }
    }
}
#endif
