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
using Chocopoi.DressingTools.Cabinet;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal static class VRCEditorUtils
    {
        private const string ExpressionParametersAssetName = "cpDT_VRC_ExParams";
        private const string ExpressionMenuAssetName = "cpDT_VRC_ExMenu";
        private const string AnimLayerAssetNamePrefix = "cpDT_VRC_AnimLayer_";

        public static VRCExpressionParameters CopyAndReplaceExpressionParameters(VRCAvatarDescriptor avatarDescriptor)
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

            // do not copy again if we have copied before
            if (expressionParameters.name == ExpressionParametersAssetName)
            {
                return expressionParameters;
            }

            var copiedPath = string.Format("{0}/{1}.asset", CabinetApplier.GeneratedAssetsPath, ExpressionParametersAssetName);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(expressionParameters), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);
            var copiedParams = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(copiedPath);
            avatarDescriptor.expressionParameters = copiedParams;
            return copiedParams;
        }

        public static VRCExpressionsMenu CopyAndReplaceExpressionMenu(VRCAvatarDescriptor avatarDescriptor)
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

            // do not copy again if we have copied before
            if (expressionsMenu.name == ExpressionMenuAssetName)
            {
                return expressionsMenu;
            }

            var copiedPath = string.Format("{0}/{1}.asset", CabinetApplier.GeneratedAssetsPath, ExpressionMenuAssetName);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(expressionsMenu), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);
            var copiedMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(copiedPath);
            avatarDescriptor.expressionsMenu = copiedMenu;
            return copiedMenu;
        }

        public static AnimatorController CopyAndReplaceLayerAnimator(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType animLayerType)
        {
            var customAnimLayerIndex = -1;
            for (var i = 0; i < avatarDescriptor.baseAnimationLayers.Length; i++)
            {
                if (avatarDescriptor.baseAnimationLayers[i].type == animLayerType)
                {
                    customAnimLayerIndex = i;
                    break;
                }
            }

            if (customAnimLayerIndex == -1)
            {
                return null;
            }

            var animLayer = avatarDescriptor.baseAnimationLayers[customAnimLayerIndex];
            var animator = GetAnimLayerAnimator(animLayer);

            // do not copy again if we have copied before
            if (animator.name.StartsWith(AnimLayerAssetNamePrefix))
            {
                return animator;
            }

            // copy to our asset path
            var copiedPath = string.Format("{0}/{1}{2}.controller", CabinetApplier.GeneratedAssetsPath, AnimLayerAssetNamePrefix, animLayerType.ToString());
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(animator), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);

            // get back here
            var copiedAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(copiedPath);
            animLayer.animatorController = copiedAnimator;
            avatarDescriptor.baseAnimationLayers[customAnimLayerIndex] = animLayer;

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
    }
}
#endif
