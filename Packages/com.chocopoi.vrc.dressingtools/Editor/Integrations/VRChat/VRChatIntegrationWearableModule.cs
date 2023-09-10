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
using Chocopoi.DressingTools.Integrations.VRChat;
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Wearable;
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
        public bool cabinetThumbnails;

        public VRChatIntegrationWearableModuleConfig()
        {
            customCabinetToggleName = null;
            cabinetThumbnails = true;
        }
    }

    [InitializeOnLoad]
    internal class VRChatIntegrationWearableModuleProvider : WearableModuleProviderBase
    {
        public const string MODULE_IDENTIFIER = "com.chocopoi.dressingtools.integrations.vrchat.wearable";

        private static readonly Localization.I18n t = Localization.I18n.Instance;
        private const string LogLabel = "VRChatIntegrationModuleProvider";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => MODULE_IDENTIFIER;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("integrations.vrc.modules.integration.friendlyName");
        [ExcludeFromCodeCoverage] public override int CallOrder => int.MaxValue;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static VRChatIntegrationWearableModuleProvider()
        {
            WearableModuleProviderLocator.Instance.Register(new VRChatIntegrationWearableModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<VRChatIntegrationWearableModuleConfig>();

        public override IModuleConfig NewModuleConfig() => new VRChatIntegrationWearableModuleConfig();

        public override bool OnAfterApplyCabinet(ApplyCabinetContext cabCtx)
        {
            // get the avatar descriptor
            if (!cabCtx.avatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDescriptor))
            {
                // not a vrc avatar
                return true;
            }

            var result = false;
            try
            {
                result = ApplyAnimationsAndMenu(cabCtx, avatarDescriptor);
                ApplyAnimationRemapping(cabCtx, avatarDescriptor);
            }
            catch (System.Exception ex)
            {
                cabCtx.report.LogException(LogLabel, ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return result;
        }

        private static void ApplyAnimationRemapping(ApplyCabinetContext cabCtx, VRCAvatarDescriptor avatarDescriptor)
        {
            EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.remappingAnimations"), 1.0f);
            var remapper = new VRCAnimationRemapper(avatarDescriptor, cabCtx.pathRemapper);
            remapper.PerformRemapping();
        }

        private static bool ApplyAnimationsAndMenu(ApplyCabinetContext cabCtx, VRCAvatarDescriptor avatarDescriptor)
        {
            // enable custom expressions and animation layers
            avatarDescriptor.customExpressions = true;
            avatarDescriptor.customizeAnimationLayers = true;

            EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.makingAnimatorCopy"), 0);
            // obtain FX layer
            var fxController = VRCEditorUtils.CopyAndReplaceLayerAnimator(avatarDescriptor, VRCAvatarDescriptor.AnimLayerType.FX);

            AnimationUtils.RemoveAnimatorLayers(fxController, "^cpDT_Cabinet");
            AnimationUtils.RemoveAnimatorParameters(fxController, "^cpDT_Cabinet");

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

            EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.makingExMenuParamsCopy"), 0);
            var exParams = VRCEditorUtils.CopyAndReplaceExpressionParameters(avatarDescriptor);
            var exMenu = VRCEditorUtils.CopyAndReplaceExpressionMenu(avatarDescriptor);

            ExpressionMenuUtils.RemoveExpressionParameters(exParams, "^cpDT_Cabinet");

            var parametersToAdd = new List<VRCExpressionParameters.Parameter>
            {
                new VRCExpressionParameters.Parameter()
                {
                    name = "cpDT_Cabinet",
                    valueType = VRCExpressionParameters.ValueType.Int,
                    defaultValue = 0,
                    networkSynced = true,
                    saved = true
                }
            };

            ExpressionMenuUtils.RemoveExpressionMenuControls(exMenu, "DT Cabinet");

            var baseSubMenu = new ExpressionMenuBuilder();
            baseSubMenu.AddToggle("Original", "cpDT_Cabinet", 0);
            baseSubMenu.CreateAsset(string.Format("{0}/cpDT_Cabinet.asset", CabinetApplier.GeneratedAssetsPath));

            // create an empty clip
            var emptyClip = new AnimationClip();
            AssetDatabase.CreateAsset(emptyClip, CabinetApplier.GeneratedAssetsPath + "/cpDT_EmptyClip.anim");
            var pairs = new Dictionary<int, Motion>
            {
                { 0, emptyClip }
            };

            // get wearables
            var wearables = DTEditorUtils.GetCabinetWearables(cabCtx.avatarGameObject);
            var cabinetMenu = baseSubMenu;
            var cabinetMenuIndex = 0;
            var originalLayerLength = fxController.layers.Length;

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

                EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.generatingWearableAnimations", config.info.name), i / (float)wearables.Length);

                // find the animation generation module
                var agm = DTEditorUtils.FindWearableModuleConfig<AnimationGenerationWearableModuleConfig>(config);
                if (agm == null)
                {
                    continue;
                }

                // add wearable thumbnail
                Texture2D icon = null;
                if (vrcm.cabinetThumbnails && config.info.thumbnail != null)
                {
                    icon = DTEditorUtils.GetTextureFromBase64(config.info.thumbnail);
                    icon.Compress(true);

                    // write into file
                    AssetDatabase.CreateAsset(icon, string.Format("{0}/cpDT_Cabinet_{1}_Icon.asset", CabinetApplier.GeneratedAssetsPath, i));
                }

                var animationGenerator = new AnimationGenerator(cabCtx.report, cabCtx.avatarGameObject, agm, wearables[i].wearableGameObject, cabCtx.avatarDynamics, wearCtx.wearableDynamics, cabCtx.pathRemapper, cabCtx.cabinetConfig.animationWriteDefaults);

                // TODO: merge disable clips and check for conflicts
                var wearAnimations = animationGenerator.GenerateWearAnimations();
                pairs.Add(i + 1, wearAnimations.Item1); // enable clip
                AssetDatabase.CreateAsset(wearAnimations.Item1, CabinetApplier.GeneratedAssetsPath + "/cpDT_" + wearables[i].name + ".anim");

                // add a new submenu if going to be full
                if (cabinetMenu.GetMenu().controls.Count == 7)
                {
                    var newSubMenu = new ExpressionMenuBuilder();
                    newSubMenu.CreateAsset(string.Format("{0}/cpDT_Cabinet_{1}.asset", CabinetApplier.GeneratedAssetsPath, ++cabinetMenuIndex));

                    cabinetMenu.AddSubMenu("Next Page", newSubMenu.GetMenu(), icon);
                    cabinetMenu = newSubMenu;
                }

                if (agm.wearableCustomizables.Count > 0)
                {
                    // Add a submenu for wearables that have customizables
                    var baseCustomizableSubMenu = new ExpressionMenuBuilder();
                    baseCustomizableSubMenu.CreateAsset(string.Format("{0}/cpDT_Cabinet_{1}_{2}.asset", CabinetApplier.GeneratedAssetsPath, cabinetMenuIndex, config.info.name));
                    cabinetMenu.AddSubMenu(vrcm.customCabinetToggleName ?? config.info.name, baseCustomizableSubMenu.GetMenu());

                    // add enable toggle
                    baseCustomizableSubMenu.AddToggle("Enable", "cpDT_Cabinet", i + 1);

                    var customizableMenu = baseCustomizableSubMenu;
                    var customizableMenuIndex = 0;

                    var customizableToggleAnimations = animationGenerator.GenerateCustomizableToggleAnimations();
                    var customizableBlendshapeAnimations = animationGenerator.GenerateCustomizableBlendshapeAnimations();

                    foreach (var wearableCustomizable in agm.wearableCustomizables)
                    {
                        // add a new submenu if going to be full
                        if (customizableMenu.GetMenu().controls.Count == 7)
                        {
                            var newSubMenu = new ExpressionMenuBuilder();
                            newSubMenu.CreateAsset(string.Format("{0}/cpDT_Cabinet_{1}_{2}_{3}.asset", CabinetApplier.GeneratedAssetsPath, cabinetMenuIndex, config.info.name, ++customizableMenuIndex));

                            customizableMenu.AddSubMenu("Next Page", newSubMenu.GetMenu());
                            customizableMenu = newSubMenu;
                        }

                        var parameterName = string.Format("cpDT_Cabinet_{0}_{1}_{2}_{3}_{4}", cabinetMenuIndex, config.info.name, customizableMenuIndex, wearableCustomizable.name, DTEditorUtils.RandomString(8));

                        if (wearableCustomizable.type == WearableCustomizableType.Toggle)
                        {
                            AnimationUtils.AddAnimatorParameter(fxController, parameterName, wearableCustomizable.defaultValue > 0);
                            parametersToAdd.Add(new VRCExpressionParameters.Parameter()
                            {
                                name = parameterName,
                                valueType = VRCExpressionParameters.ValueType.Bool,
                                defaultValue = wearableCustomizable.defaultValue,
                                networkSynced = true,
                                saved = true
                            });

                            var anims = customizableToggleAnimations[wearableCustomizable];
                            AssetDatabase.CreateAsset(anims.Item1, string.Format("{0}/{1}_On.anim", CabinetApplier.GeneratedAssetsPath, parameterName));
                            AssetDatabase.CreateAsset(anims.Item2, string.Format("{0}/{1}_Off.anim", CabinetApplier.GeneratedAssetsPath, parameterName));
                            AnimationUtils.GenerateSingleToggleLayer(fxController, parameterName, parameterName, anims.Item2, anims.Item1, cabCtx.cabinetConfig.animationWriteDefaults, false, null, refTransition);
                            customizableMenu.AddToggle(wearableCustomizable.name, parameterName, 1);
                        }
                        else if (wearableCustomizable.type == WearableCustomizableType.Blendshape)
                        {
                            AnimationUtils.AddAnimatorParameter(fxController, parameterName, wearableCustomizable.defaultValue);
                            parametersToAdd.Add(new VRCExpressionParameters.Parameter()
                            {
                                name = wearableCustomizable.name,
                                valueType = VRCExpressionParameters.ValueType.Float,
                                defaultValue = wearableCustomizable.defaultValue,
                                networkSynced = true,
                                saved = true
                            });

                            var toggleAnims = customizableToggleAnimations[wearableCustomizable];
                            AssetDatabase.CreateAsset(toggleAnims.Item1, string.Format("{0}/{1}_On.anim", CabinetApplier.GeneratedAssetsPath, parameterName));
                            AssetDatabase.CreateAsset(toggleAnims.Item2, string.Format("{0}/{1}_Off.anim", CabinetApplier.GeneratedAssetsPath, parameterName));
                            var blendshapeAnim = customizableBlendshapeAnimations[wearableCustomizable];
                            AssetDatabase.CreateAsset(blendshapeAnim, string.Format("{0}/{1}_MotionTime.anim", CabinetApplier.GeneratedAssetsPath, parameterName));
                            AnimationUtils.GenerateSingleToggleLayer(fxController, parameterName + "_Toggles", parameterName, toggleAnims.Item2, toggleAnims.Item1, cabCtx.cabinetConfig.animationWriteDefaults, false, null, refTransition);
                            AnimationUtils.GenerateSingleMotionTimeLayer(fxController, parameterName + "_Blendshapes", parameterName, blendshapeAnim, cabCtx.cabinetConfig.animationWriteDefaults);
                            customizableMenu.AddRadialPuppet(wearableCustomizable.name, parameterName);
                        }
                    }
                }
                else
                {
                    // Add toggle only
                    cabinetMenu.AddToggle(vrcm.customCabinetToggleName ?? config.info.name, "cpDT_Cabinet", i + 1, icon);
                }
            }

            // create cabinet layer
            AnimationUtils.GenerateAnyStateLayer(fxController, "cpDT_Cabinet", "cpDT_Cabinet", pairs, cabCtx.cabinetConfig.animationWriteDefaults, null, refTransition);

            // we push our cabinet layer to the top
            var layerList = new List<AnimatorControllerLayer>(fxController.layers);
            var cabinetLayer = layerList[layerList.Count - 1];
            layerList.Remove(cabinetLayer);
            layerList.Insert(originalLayerLength, cabinetLayer);
            fxController.layers = layerList.ToArray();

            EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.addingExParams"), 1.0f);

            try
            {
                ExpressionMenuUtils.AddExpressionParameters(exParams, parametersToAdd);
                ExpressionMenuUtils.AddSubMenu(exMenu, "DT Cabinet", baseSubMenu.GetMenu());
            }
            catch (ParameterOverflowException ex)
            {
                DTReportUtils.LogExceptionLocalized(cabCtx.report, LogLabel, ex, "integrations.vrc.msgCode.error.parameterOverFlow");
                return false;
            }

            EditorUtility.SetDirty(fxController);
            EditorUtility.SetDirty(exMenu);
            EditorUtility.SetDirty(exParams);

            EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.savingAssets"), 1.0f);
            AssetDatabase.SaveAssets();

            return true;
        }
    }
}
#endif
