/*
 * File: GenerateAnimationsHook.cs
 * Project: DressingTools
 * Created Date: Thursday, August 10th 2023, 11:42:41 pm
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
using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.AvatarLib.Expressions;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Integration.VRChat.Modules;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal class GenerateAnimationsHook : IBuildDTCabinetHook
    {
        private const string LogLabel = "GenerateAnimationHook";

        private DTReport _report;

        private DTCabinet _cabinet;

        public GenerateAnimationsHook(DTReport report, DTCabinet cabinet)
        {
            _report = report;
            _cabinet = cabinet;
        }

        public bool OnPreprocessAvatar()
        {
            EditorUtility.DisplayProgressBar("DressingTools", "Generating animations...", 0);

            // get the avatar descriptor
            var avatarDescriptor = _cabinet.AvatarGameObject.GetComponent<VRCAvatarDescriptor>();

            // obtain FX layer
            var fxController = CopyAndReplaceLayerAnimator(avatarDescriptor, VRCAvatarDescriptor.AnimLayerType.FX);

            EditorUtility.DisplayProgressBar("DressingTools", "Removing old animator layers and parameters...", 0);
            AnimationUtils.RemoveAnimatorLayers(fxController, "^cpDT_Cabinet");
            AnimationUtils.RemoveAnimatorParameters(fxController, "^cpDT_Cabinet");

            EditorUtility.DisplayProgressBar("DressingTools", "Writing animator...", 0);
            AnimationUtils.AddAnimatorParameter(fxController, "cpDT_Cabinet", 0);
            var refTransition = new AnimatorStateTransition
            {
                canTransitionToSelf = false,
                duration = 0,
                exitTime = 0,
                hasExitTime = false,
                hasFixedDuration = true,
                isExit = false,
                mute = false,
                offset = 0,
                orderedInterruption = true,
                solo = false,
                conditions = new AnimatorCondition[] { }
            };

            var exParams = CopyAndReplaceExpressionParameters(avatarDescriptor);
            var exMenu = CopyAndReplaceExpressionMenu(avatarDescriptor);

            ExpressionMenuUtils.RemoveExpressionParameters(exParams, "^cpDT_Cabinet");

            try
            {
                ExpressionMenuUtils.AddExpressionParameters(exParams, new VRCExpressionParameters.Parameter[]
                {
                new VRCExpressionParameters.Parameter()
                {
                    name = "cpDT_Cabinet",
                    valueType = VRCExpressionParameters.ValueType.Int,
                    defaultValue = 0,
                    networkSynced = true,
                    saved = true
                }
                });
            }
            catch (ParameterOverflowException ex)
            {
                DTReportUtils.LogExceptionLocalized(_report, LogLabel, ex, "integrations.vrc.msgCode.error.parameterOverFlow");
                return false;
            }

            ExpressionMenuUtils.RemoveExpressionMenuControls(exMenu, "DT Cabinet");

            var subMenu = new ExpressionMenuBuilder(exMenu)
                .BeginNewSubMenu("DT Cabinet");

            subMenu.AddToggle("Original", "cpDT_Cabinet", 0);

            // create an empty clip
            var emptyClip = new AnimationClip();
            AssetDatabase.CreateAsset(emptyClip, BuildDTCabinetCallback.GeneratedAssetsPath + "/cpDT_EmptyClip.anim");
            var pairs = new Dictionary<int, Motion>
            {
                { 0, emptyClip }
            };

            // get wearables
            var wearables = _cabinet.GetWearables();

            for (var i = 0; i < wearables.Length; i++)
            {
                var config = WearableConfig.Deserialize(wearables[i].configJson);
                if (config == null)
                {
                    if (!EditorUtility.DisplayDialog("DressingTools", "Unable to load configuration for one of the wearables. It will not be dressed.\nDo you want to continue?", "Yes", "No"))
                    {
                        return false;
                    }
                    continue;
                }

                // obtain module
                var vrcm = DTRuntimeUtils.FindWearableModuleConfig<VRChatIntegrationModuleConfig>(config);
                if (vrcm == null)
                {
                    // use default settings if no module
                    vrcm = new VRChatIntegrationModuleConfig();
                }

                EditorUtility.DisplayProgressBar("DressingTools", "Generating animations for " + config.Info.name + "...", i / (float)wearables.Length * 100);
                var wearableDynamics = DTRuntimeUtils.ScanDynamics(wearables[i].wearableGameObject, false);

                // find the animation generation module
                var agm = DTRuntimeUtils.FindWearableModuleConfig<AnimationGenerationModuleConfig>(config);
                if (agm == null)
                {
                    Debug.Log("[DressingTools] [BuildDTCabinetCallback] [GenerateAnimationHook] " + config.Info.name + " has no AnimationGenerationModule, skipping this wearable generation");
                    continue;
                }

                var animationGenerator = new AnimationGenerator(_report, _cabinet.AvatarGameObject, agm, wearables[i].wearableGameObject, wearableDynamics);

                // TODO: merge disable clips and check for conflicts
                var wearAnimations = animationGenerator.GenerateWearAnimations(_cabinet.AnimationGenerationWriteDefaults);
                pairs.Add(i + 1, wearAnimations.Item1); // enable clip
                AssetDatabase.CreateAsset(wearAnimations.Item1, BuildDTCabinetCallback.GeneratedAssetsPath + "/cpDT_" + wearables[i].name + ".anim");

                // generate expression menu
                subMenu.AddToggle(vrcm.customCabinetToggleName ?? config.Info.name, "cpDT_Cabinet", i + 1);
            }

            AnimationUtils.GenerateAnyStateLayer(fxController, "cpDT_Cabinet", "cpDT_Cabinet", pairs, _cabinet.AnimationGenerationWriteDefaults, null, refTransition);

            EditorUtility.DisplayProgressBar("DressingTools", "Generating expression menu...", 0);
            subMenu.CreateAsset(BuildDTCabinetCallback.GeneratedAssetsPath + "/cpDT_Cabinet.asset")
                .EndNewSubMenu();

            EditorUtility.SetDirty(fxController);
            EditorUtility.SetDirty(exMenu);
            EditorUtility.SetDirty(exParams);

            return true;
        }

        private AnimatorController GetDefaultLayerAnimator(VRCAvatarDescriptor.AnimLayerType animLayerType)
        {
            string defaultControllerName = null;
            switch (animLayerType)
            {
                case VRCAvatarDescriptor.AnimLayerType.Base:
                    defaultControllerName = "Locomotion";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Additive:
                    defaultControllerName = "Idle";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Action:
                    defaultControllerName = "Action";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.Gesture:
                    defaultControllerName = "Hands";
                    break;
                case VRCAvatarDescriptor.AnimLayerType.FX:
                    defaultControllerName = "Face";
                    break;
            }

            if (defaultControllerName == null)
            {
                return null;
            }

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/Controllers/vrc_AvatarV3" + defaultControllerName + "Layer.controller");
            if (controller == null)
            {
                controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3" + defaultControllerName + "Layer.controller");
            }
            return controller;
        }

        private VRCExpressionParameters CopyAndReplaceExpressionParameters(VRCAvatarDescriptor avatarDescriptor)
        {
            var copiedPath = string.Format("{0}/cpDT_ExParams.asset", BuildDTCabinetCallback.GeneratedAssetsPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(avatarDescriptor.expressionParameters), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);
            var copiedParams = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(copiedPath);
            avatarDescriptor.expressionParameters = copiedParams;
            return copiedParams;
        }

        private VRCExpressionsMenu CopyAndReplaceExpressionMenu(VRCAvatarDescriptor avatarDescriptor)
        {
            var copiedPath = string.Format("{0}/cpDT_ExMenu.asset", BuildDTCabinetCallback.GeneratedAssetsPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(avatarDescriptor.expressionsMenu), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);
            var copiedMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(copiedPath);
            avatarDescriptor.expressionsMenu = copiedMenu;
            return copiedMenu;
        }

        private AnimatorController CopyAndReplaceLayerAnimator(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType animLayerType)
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
            var animator = !animLayer.isDefault && animLayer.animatorController != null ? (AnimatorController)animLayer.animatorController : GetDefaultLayerAnimator(animLayerType);

            // copy to our asset path
            var copiedPath = string.Format("{0}/cpDT_{1}.controller", BuildDTCabinetCallback.GeneratedAssetsPath, animLayerType.ToString());
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
    }
}
#endif
