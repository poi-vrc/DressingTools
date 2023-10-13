/*
 * File: VRCEditorUtils.cs
 * Project: DressingTools
 * Created Date: Tuesday, September 5th 2023, 9:41:36 am
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
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal static class VRCEditorUtils
    {
        public static VRCExpressionParameters CopyAndReplaceExpressionParameters(VRCAvatarDescriptor avatarDescriptor, string path)
        {
            var expressionParameters = avatarDescriptor.expressionParameters;

            if (expressionParameters == null)
            {
                expressionParameters = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Expressions Menu/DefaultExpressionParameters.asset");
                if (expressionParameters == null)
                {
                    expressionParameters = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>("Assets/VRCSDK/Examples3/Animation/Controllers/Expressions Menu/DefaultExpressionParameters.asset");
                }

                if (expressionParameters == null)
                {
                    // we can't obtain the default asset
                    return null;
                }
            }

            var fileName = Path.GetFileNameWithoutExtension(path);

            // do not copy again if we have copied before
            if (expressionParameters.name == fileName)
            {
                return expressionParameters;
            }

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(expressionParameters), path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path);
            var copiedParams = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(path);
            avatarDescriptor.expressionParameters = copiedParams;
            return copiedParams;
        }

        public static VRCExpressionsMenu CopyAndReplaceExpressionMenu(VRCAvatarDescriptor avatarDescriptor, string path)
        {
            var expressionsMenu = avatarDescriptor.expressionsMenu;

            if (expressionsMenu == null)
            {
                expressionsMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Expressions Menu/DefaultExpressionsMenu.asset");
                if (expressionsMenu == null)
                {
                    expressionsMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>("Assets/VRCSDK/Examples3/Animation/Controllers/Expressions Menu/DefaultExpressionsMenu.asset");
                }

                if (expressionsMenu == null)
                {
                    // we can't obtain the default asset
                    return null;
                }
            }

            var fileName = Path.GetFileNameWithoutExtension(path);

            // do not copy again if we have copied before
            if (expressionsMenu.name == fileName)
            {
                return expressionsMenu;
            }

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(expressionsMenu), path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path);
            var copiedMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(path);
            avatarDescriptor.expressionsMenu = copiedMenu;
            return copiedMenu;
        }

        public static void FindAnimLayerArrayAndIndex(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType animLayerType, out VRCAvatarDescriptor.CustomAnimLayer[] layers, out int customAnimLayerIndex)
        {
            layers = null;
            customAnimLayerIndex = -1;

            // try find in base anim layers
            for (var i = 0; i < avatarDescriptor.baseAnimationLayers.Length; i++)
            {
                if (avatarDescriptor.baseAnimationLayers[i].type == animLayerType)
                {
                    layers = avatarDescriptor.baseAnimationLayers;
                    customAnimLayerIndex = i;
                    break;
                }
            }

            // try find in special layers
            if (layers == null)
            {
                for (var i = 0; i < avatarDescriptor.specialAnimationLayers.Length; i++)
                {
                    if (avatarDescriptor.specialAnimationLayers[i].type == animLayerType)
                    {
                        layers = avatarDescriptor.specialAnimationLayers;
                        customAnimLayerIndex = i;
                        break;
                    }
                }

                // not found
            }
        }

        public static AnimatorController CopyAndReplaceLayerAnimator(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType animLayerType, string path)
        {
            FindAnimLayerArrayAndIndex(avatarDescriptor, animLayerType, out var layers, out var customAnimLayerIndex);

            var animLayer = layers[customAnimLayerIndex];
            var animator = GetAnimLayerAnimator(animLayer);

            var fileName = Path.GetFileNameWithoutExtension(path);

            // do not copy again if we have copied before
            if (animator.name == fileName)
            {
                return animator;
            }

            // copy to our asset path
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(animator), path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path);

            // get back here
            var copiedAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            animLayer.animatorController = copiedAnimator;
            layers[customAnimLayerIndex] = animLayer;

            return copiedAnimator;
        }

        public static AnimatorController GetAnimLayerAnimator(VRCAvatarDescriptor.CustomAnimLayer animLayer)
        {
            return !animLayer.isDefault && animLayer.animatorController != null && animLayer.animatorController is AnimatorController controller ?
                 controller :
                 GetDefaultLayerAnimator(animLayer.type);
        }

        public static AnimatorController GetDefaultLayerAnimator(VRCAvatarDescriptor.AnimLayerType animLayerType)
        {
            string defaultControllerName = null;
            switch (animLayerType)
            {
                case VRCAvatarDescriptor.AnimLayerType.Base:
                    defaultControllerName = "LocomotionLayer";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Additive:
                    defaultControllerName = "IdleLayer";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Action:
                    defaultControllerName = "ActionLayer";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Gesture:
                    defaultControllerName = "HandsLayer";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.FX:
                    defaultControllerName = "FaceLayer";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Sitting:
                    defaultControllerName = "SittingLayer";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.IKPose:
                    defaultControllerName = "UtilityIKPose";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.TPose:
                    defaultControllerName = "UtilityTPose";
                    break;
            }

            if (defaultControllerName == null)
            {
                return null;
            }

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/Controllers/vrc_AvatarV3" + defaultControllerName + ".controller");
            if (controller == null)
            {
                controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3" + defaultControllerName + ".controller");
            }
            return controller;
        }

        public static bool IsProxyAnimation(Motion m)
        {
            if (m == null)
            {
                return false;
            }
            var animPath = AssetDatabase.GetAssetPath(m);
            return animPath != null && animPath != "" && animPath.Contains("/Animation/ProxyAnim/proxy");
        }
    }
}
#endif
