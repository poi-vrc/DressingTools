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
        private static void ScanVRCAvatarLayerAnimatorParameters(Dictionary<string, AnimatorControllerParameterType> parameters, VRCAvatarDescriptor.CustomAnimLayer[] customAnimLayers)
        {
            foreach (var customAnimLayer in customAnimLayers)
            {
                var animator = VRCAnimUtils.GetCustomAnimLayerAnimator(customAnimLayer);
                foreach (var animParam in animator.parameters)
                {
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
    }
}
