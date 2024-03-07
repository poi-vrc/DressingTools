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
using UnityEditor.Animations;
using UnityEngine;

#if DT_VRCSDK3A
using Chocopoi.DressingFramework.Animations.VRChat;
using VRC.SDK3.Avatars.Components;
#endif

namespace Chocopoi.DressingTools.Animations
{
    internal static class AnimUtils
    {
#if DT_VRCSDK3A
        private static readonly string[] VRCInternalParameters = new string[] {
            "IsLocal",
            "Viseme",
            "Voice",
            "GestureLeft",
            "GestureRight",
            "GestureLeftWeight",
            "GestureRightWeight",
            "AngularY",
            "VelocityX",
            "VelocityY",
            "VelocityZ",
            "VelocityMagnitude",
            "Upright",
            "Grounded",
            "Seated",
            "AFK",
            "TrackingType",
            "VRMode",
            "MuteSelf",
            "InStation",
            "Earmuffs",
            "IsOnFriendsList",
            "AvatarVersion",
            "ScaleModified",
            "ScaleFactor",
            "ScaleFactorInverse",
            "EyeHeightAsMeters",
            "EyeHeightAsPercent",
        };

        private static void ScanVRCAvatarLayerAnimatorParameters(Dictionary<string, AnimatorControllerParameterType> parameters, VRCAvatarDescriptor.CustomAnimLayer[] customAnimLayers)
        {
            foreach (var customAnimLayer in customAnimLayers)
            {
                var animator = VRCAnimUtils.GetCustomAnimLayerAnimator(customAnimLayer);
                foreach (var animParam in animator.parameters)
                {
                    if (Array.IndexOf(VRCInternalParameters, animParam.name) != -1)
                    {
                        // skip vrc internal parameters
                        continue;
                    }

                    if (parameters.TryGetValue(animParam.name, out var type))
                    {
                        if (type != animParam.type)
                        {
                            // ignore it, otherwise the console will be full of messages
                            continue;
                        }
                    }
                    else
                    {
                        parameters[animParam.name] = animParam.type;
                    }
                }
            }
        }

        private static void ScanVRCAnimatorParameters(Dictionary<string, AnimatorControllerParameterType> parameters, GameObject avatarGameObject)
        {
            if (!avatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDesc))
            {
                return;
            }

            ScanVRCAvatarLayerAnimatorParameters(parameters, avatarDesc.baseAnimationLayers);
            ScanVRCAvatarLayerAnimatorParameters(parameters, avatarDesc.specialAnimationLayers);
        }
#endif

        public static Dictionary<string, AnimatorControllerParameterType> ScanAnimatorParameters(GameObject avatarGameObject)
        {
            var parameters = new Dictionary<string, AnimatorControllerParameterType>();
#if DT_VRCSDK3A
            ScanVRCAnimatorParameters(parameters, avatarGameObject);
#endif
            return parameters;
        }

        public static void GetWriteDefaultCounts(AnimatorController controller, out int onCount, out int offCount)
        {
            var stack = new Stack<AnimatorStateMachine>();

            foreach (var layer in controller.layers)
            {
                stack.Push(layer.stateMachine);
            }

            onCount = 0;
            offCount = 0;

            while (stack.Count > 0)
            {
                var stateMachine = stack.Pop();
                foreach (var state in stateMachine.states)
                {
                    if (state.state.writeDefaultValues)
                    {
                        onCount++;
                    }
                    else
                    {
                        offCount++;
                    }
                }

                foreach (var childAnimatorMachine in stateMachine.stateMachines)
                {
                    stack.Push(childAnimatorMachine.stateMachine);
                }
            }

            if (onCount != 0 && offCount != 0)
            {
                Debug.LogWarning($"[DressingTools] Inconsistent write defaults detected with {onCount} on and {offCount} off. Unexpected behaviour might occur. Using the one with more count.");
            }
        }

        public static bool DetermineWriteDefaultsByOnOffCounts(int onCount, int offCount)
        {
            // default to on if both counts are equal
            return onCount >= offCount;
        }

        public static bool DetectWriteDefaults(AnimatorController controller)
        {
            GetWriteDefaultCounts(controller, out var onCount, out var offCount);
            return DetermineWriteDefaultsByOnOffCounts(onCount, offCount);
        }
    }
}
