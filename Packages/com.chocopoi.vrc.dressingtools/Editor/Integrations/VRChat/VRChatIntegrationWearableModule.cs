/*
 * File: VRChatIntegrationModule.cs
 * Project: DressingTools
 * Created Date: Saturday, July 29th 2023, 10:31:11 am
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.AvatarLib.Expressions;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Chocopoi.DressingTools.Integration.VRChat.Modules
{
    internal class VRChatIntegrationWearableModuleConfig : IModuleConfig
    {
        public string customCabinetToggleName;

        public VRChatIntegrationWearableModuleConfig()
        {
            customCabinetToggleName = null;
        }
    }

    [InitializeOnLoad]
    internal class VRChatIntegrationWearableModuleProvider : WearableModuleProviderBase
    {
        public const string MODULE_IDENTIFIER = "com.chocopoi.dressingtools.integrations.vrchat.wearable";

        private const string LogLabel = "VRChatIntegrationModuleProvider";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => MODULE_IDENTIFIER;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Integration: VRChat";
        [ExcludeFromCodeCoverage] public override int CallOrder => int.MaxValue;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static VRChatIntegrationWearableModuleProvider()
        {
            WearableModuleProviderLocator.Instance.Register(new VRChatIntegrationWearableModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<VRChatIntegrationWearableModuleConfig>();

        public override IModuleConfig NewModuleConfig() => new VRChatIntegrationWearableModuleConfig();

        public override bool OnAfterApplyCabinet(ApplyCabinetContext ctx)
        {
            var result = false;
            try
            {
                result = ApplyAnimationsAndMenu(ctx);
            }
            catch (System.Exception ex)
            {
                ctx.report.LogException(LogLabel, ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return result;
        }

        public static bool ApplyAnimationsAndMenu(ApplyCabinetContext cabCtx)
        {
            // get the avatar descriptor
            if (!cabCtx.avatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDescriptor))
            {
                // not a vrc avatar
                return true;
            }

            EditorUtility.DisplayProgressBar("DressingTools", "Generating animations...", 0);

            // enable custom expressions and animation layers
            avatarDescriptor.customExpressions = true;
            avatarDescriptor.customizeAnimationLayers = true;

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
                DTReportUtils.LogExceptionLocalized(cabCtx.report, LogLabel, ex, "integrations.vrc.msgCode.error.parameterOverFlow");
                return false;
            }

            ExpressionMenuUtils.RemoveExpressionMenuControls(exMenu, "DT Cabinet");

            var subMenu = new ExpressionMenuBuilder(exMenu)
                .BeginNewSubMenu("DT Cabinet");

            subMenu.AddToggle("Original", "cpDT_Cabinet", 0);

            // create an empty clip
            var emptyClip = new AnimationClip();
            AssetDatabase.CreateAsset(emptyClip, CabinetApplier.GeneratedAssetsPath + "/cpDT_EmptyClip.anim");
            var pairs = new Dictionary<int, Motion>
            {
                { 0, emptyClip }
            };

            // get wearables
            var wearables = DTEditorUtils.GetCabinetWearables(cabCtx.avatarGameObject);

            for (var i = 0; i < wearables.Length; i++)
            {
                // obtain the wearable context
                var wearCtx = cabCtx.wearableContexts[wearables[i]];
                var config = wearCtx.wearableConfig;

                // obtain module
                var vrcm = DTEditorUtils.FindWearableModuleConfig<VRChatIntegrationWearableModuleConfig>(config);
                if (vrcm == null)
                {
                    // use default settings if no module
                    vrcm = new VRChatIntegrationWearableModuleConfig();
                }

                EditorUtility.DisplayProgressBar("DressingTools", "Generating animations for " + config.Info.name + "...", i / (float)wearables.Length * 100);

                // find the animation generation module
                var agm = DTEditorUtils.FindWearableModuleConfig<AnimationGenerationWearableModuleConfig>(config);
                if (agm == null)
                {
                    continue;
                }

                var animationGenerator = new AnimationGenerator(cabCtx.report, cabCtx.avatarGameObject, agm, wearables[i].wearableGameObject, cabCtx.avatarDynamics, wearCtx.wearableDynamics, cabCtx.cabinetConfig.AnimationWriteDefaults);

                // TODO: merge disable clips and check for conflicts
                var wearAnimations = animationGenerator.GenerateWearAnimations();
                pairs.Add(i + 1, wearAnimations.Item1); // enable clip
                AssetDatabase.CreateAsset(wearAnimations.Item1, CabinetApplier.GeneratedAssetsPath + "/cpDT_" + wearables[i].name + ".anim");

                // generate expression menu
                subMenu.AddToggle(vrcm.customCabinetToggleName ?? config.Info.name, "cpDT_Cabinet", i + 1);
            }

            AnimationUtils.GenerateAnyStateLayer(fxController, "cpDT_Cabinet", "cpDT_Cabinet", pairs, cabCtx.cabinetConfig.AnimationWriteDefaults, null, refTransition);

            EditorUtility.DisplayProgressBar("DressingTools", "Generating expression menu...", 0);
            subMenu.CreateAsset(CabinetApplier.GeneratedAssetsPath + "/cpDT_Cabinet.asset")
                .EndNewSubMenu();

            EditorUtility.SetDirty(fxController);
            EditorUtility.SetDirty(exMenu);
            EditorUtility.SetDirty(exParams);

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();

            return true;
        }

        private static AnimatorController GetDefaultLayerAnimator(VRCAvatarDescriptor.AnimLayerType animLayerType)
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

        private static VRCExpressionParameters CopyAndReplaceExpressionParameters(VRCAvatarDescriptor avatarDescriptor)
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

            var copiedPath = string.Format("{0}/cpDT_ExParams.asset", CabinetApplier.GeneratedAssetsPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(expressionParameters), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);
            var copiedParams = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(copiedPath);
            avatarDescriptor.expressionParameters = copiedParams;
            return copiedParams;
        }

        private static VRCExpressionsMenu CopyAndReplaceExpressionMenu(VRCAvatarDescriptor avatarDescriptor)
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

            var copiedPath = string.Format("{0}/cpDT_ExMenu.asset", CabinetApplier.GeneratedAssetsPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(expressionsMenu), copiedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(copiedPath);
            var copiedMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(copiedPath);
            avatarDescriptor.expressionsMenu = copiedMenu;
            return copiedMenu;
        }

        private static AnimatorController CopyAndReplaceLayerAnimator(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType animLayerType)
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
            var copiedPath = string.Format("{0}/cpDT_{1}.controller", CabinetApplier.GeneratedAssetsPath, animLayerType.ToString());
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
