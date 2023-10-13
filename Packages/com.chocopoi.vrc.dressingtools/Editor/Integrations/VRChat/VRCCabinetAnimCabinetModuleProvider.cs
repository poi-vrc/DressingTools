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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.AvatarLib.Expressions;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Context;
using Chocopoi.DressingFramework.Extensibility.Plugin;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingFramework.Wearable.Modules;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Integrations.VRChat;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Chocopoi.DressingTools.Api.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.Api.Integration.VRChat.Cabinet.Modules;
using Chocopoi.DressingFramework.Cabinet.Modules;
using Chocopoi.DressingFramework.Wearable;
using System;

namespace Chocopoi.DressingTools.Integration.VRChat.Modules
{
    [InitializeOnLoad]
    internal class VRCCabinetAnimCabinetModuleProvider : CabinetModuleProviderBase
    {
        private const string ExpressionParametersAssetName = "VRC_ExParams.asset";
        private const string ExpressionMenuAssetName = "VRC_ExMenu.asset";
        private const string AnimLayerAssetNamePrefix = "VRC_AnimLayer_";

        private static readonly I18nTranslator t = Localization.I18n.ToolTranslator;
        private const string LogLabel = "VRCCabinetAnimCabinetModuleProvider";

        [ExcludeFromCodeCoverage] public override string Identifier => VRCCabinetAnimCabinetModuleConfig.ModuleIdentifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("integrations.vrc.modules.cabinetAnim.friendlyName");
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;
        [ExcludeFromCodeCoverage] public override CabinetApplyConstraint Constraint => ApplyAtStage(CabinetApplyStage.Integration, CabinetHookStageRunOrder.Before).Build();

        public override IModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<VRCCabinetAnimCabinetModuleConfig>();

        public override IModuleConfig NewModuleConfig() => new VRCCabinetAnimCabinetModuleConfig();

        private static AnimatorStateTransition GetStateTransitionReference()
        {
            return new AnimatorStateTransition
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
        }

        private static void PrepareAnimatorAndExpressions(ApplyCabinetContext cabCtx, VRCAvatarDescriptor avatarDescriptor, out AnimatorController fxController, out VRCExpressionParameters exParams, out VRCExpressionsMenu exMenu, out List<VRCExpressionParameters.Parameter> parametersToAdd, out Dictionary<int, Motion> pairs)
        {
            // enable custom expressions and animation layers
            avatarDescriptor.customExpressions = true;
            avatarDescriptor.customizeAnimationLayers = true;

            // obtain FX layer
            var fxLayerPath = cabCtx.MakeUniqueAssetPath($"{AnimLayerAssetNamePrefix}FX.controller");
            fxController = VRCEditorUtils.CopyAndReplaceLayerAnimator(avatarDescriptor, VRCAvatarDescriptor.AnimLayerType.FX, fxLayerPath);

            AnimationUtils.RemoveAnimatorLayers(fxController, "^cpDT_Cabinet");
            AnimationUtils.RemoveAnimatorParameters(fxController, "^cpDT_Cabinet");

            AnimationUtils.AddAnimatorParameter(fxController, "cpDT_Cabinet", 0);

            EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.makingExMenuParamsCopy"), 0);
            var exParamsPath = cabCtx.MakeUniqueAssetPath(ExpressionParametersAssetName);
            exParams = VRCEditorUtils.CopyAndReplaceExpressionParameters(avatarDescriptor, exParamsPath);

            var exMenuPath = cabCtx.MakeUniqueAssetPath(ExpressionMenuAssetName);
            exMenu = VRCEditorUtils.CopyAndReplaceExpressionMenu(avatarDescriptor, exMenuPath);

            ExpressionMenuUtils.RemoveExpressionParameters(exParams, "^cpDT_Cabinet");

            parametersToAdd = new List<VRCExpressionParameters.Parameter>
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

            // create an empty clip
            var emptyClip = new AnimationClip();
            cabCtx.CreateUniqueAsset(emptyClip, "Cabinet_EmptyClip.anim");
            pairs = new Dictionary<int, Motion>
            {
                { 0, emptyClip }
            };
        }

        private static void AddCustomizable(ApplyCabinetContext cabCtx, WearableConfig config, int wearableIndex, AnimatorStateTransition refTransition, AnimatorController fxController, Dictionary<int, Motion> pairs, List<VRCExpressionParameters.Parameter> parametersToAdd, CabinetAnimWearableModuleConfig.Customizable wearableCustomizable, Dictionary<CabinetAnimWearableModuleConfig.Customizable, System.Tuple<AnimationClip, AnimationClip>> customizableToggleAnimations, Dictionary<CabinetAnimWearableModuleConfig.Customizable, AnimationClip> customizableBlendshapeAnimations, ref ExpressionMenuBuilder customizableMenu, ref int customizableMenuIndex)
        {
            // add a new submenu if going to be full
            if (customizableMenu.GetMenu().controls.Count == 7)
            {
                var newSubMenu = new ExpressionMenuBuilder();
                newSubMenu.CreateAsset(cabCtx.MakeUniqueAssetPath($"Cabinet_{wearableIndex}_{config.info.name}_Menu_{++customizableMenuIndex}.asset"));

                customizableMenu.AddSubMenu("Next Page", newSubMenu.GetMenu());
                customizableMenu = newSubMenu;
            }

            var uniqueName = $"Cabinet_{wearableIndex}_{config.info.name}_{wearableCustomizable.name}_{DKEditorUtils.RandomString(8)}";
            var parameterName = "cpDT_" + uniqueName;

            if (wearableCustomizable.type == CabinetAnimWearableModuleConfig.CustomizableType.Toggle)
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
                cabCtx.CreateUniqueAsset(anims.Item1, $"{uniqueName}_On.anim");
                cabCtx.CreateUniqueAsset(anims.Item2, $"{uniqueName}_Off.anim");
                AnimationUtils.GenerateSingleToggleLayer(fxController, parameterName, parameterName, anims.Item2, anims.Item1, cabCtx.cabinetConfig.animationWriteDefaults, false, null, refTransition);
                customizableMenu.AddToggle(wearableCustomizable.name, parameterName, 1);
            }
            else if (wearableCustomizable.type == CabinetAnimWearableModuleConfig.CustomizableType.Blendshape)
            {
                AnimationUtils.AddAnimatorParameter(fxController, parameterName, wearableCustomizable.defaultValue);
                parametersToAdd.Add(new VRCExpressionParameters.Parameter()
                {
                    name = parameterName,
                    valueType = VRCExpressionParameters.ValueType.Float,
                    defaultValue = wearableCustomizable.defaultValue,
                    networkSynced = true,
                    saved = true
                });

                var blendshapeAnim = customizableBlendshapeAnimations[wearableCustomizable];
                cabCtx.CreateUniqueAsset(blendshapeAnim, $"{uniqueName}_MotionTime.anim");
                AnimationUtils.GenerateSingleMotionTimeLayer(fxController, parameterName + "_Blendshapes", parameterName, blendshapeAnim, cabCtx.cabinetConfig.animationWriteDefaults);
                customizableMenu.AddRadialPuppet(wearableCustomizable.name, parameterName);
            }
        }

        private static void AddCustomizableMenu(ApplyCabinetContext cabCtx, WearableConfig config, CabinetAnimWearableModuleConfig agm, int wearableIndex, AnimatorStateTransition refTransition, AnimatorController fxController, Dictionary<int, Motion> pairs, List<VRCExpressionParameters.Parameter> parametersToAdd, CabinetAnimGenerator animationGenerator, ExpressionMenuBuilder cabinetMenu)
        {
            // Add a submenu for wearables that have customizables
            var baseCustomizableSubMenu = new ExpressionMenuBuilder();
            baseCustomizableSubMenu.CreateAsset(cabCtx.MakeUniqueAssetPath($"Cabinet_{wearableIndex}_{config.info.name}_Menu.asset"));
            cabinetMenu.AddSubMenu(config.info.name, baseCustomizableSubMenu.GetMenu());

            // add enable toggle
            baseCustomizableSubMenu.AddToggle("Enable", "cpDT_Cabinet", wearableIndex + 1);

            var customizableMenu = baseCustomizableSubMenu;
            var customizableMenuIndex = 0;

            var customizableToggleAnimations = animationGenerator.GenerateCustomizableToggleAnimations();
            var customizableBlendshapeAnimations = animationGenerator.GenerateCustomizableBlendshapeAnimations();

            foreach (var wearableCustomizable in agm.wearableCustomizables)
            {
                AddCustomizable(cabCtx, config, wearableIndex, refTransition, fxController, pairs, parametersToAdd, wearableCustomizable, customizableToggleAnimations, customizableBlendshapeAnimations, ref customizableMenu, ref customizableMenuIndex);
            }
        }

        private static void ApplyWearableAnimationsAndMenus(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, VRCCabinetAnimCabinetModuleConfig vrcm, CabinetAnimWearableModuleConfig agm, int wearableIndex, AnimatorStateTransition refTransition, AnimatorController fxController, Dictionary<int, Motion> pairs, List<VRCExpressionParameters.Parameter> parametersToAdd, ref ExpressionMenuBuilder cabinetMenu, ref int cabinetMenuIndex)
        {
            var config = wearCtx.wearableConfig;

            // add wearable thumbnail
            Texture2D icon = null;
            if (vrcm.cabinetThumbnails && config.info.thumbnail != null)
            {
                icon = DTEditorUtils.GetTextureFromBase64(config.info.thumbnail);
                icon.Compress(true);

                // write into file
                cabCtx.CreateUniqueAsset(icon, $"Cabinet_{wearableIndex}_Icon.asset");
            }

            var animationGenerator = new CabinetAnimGenerator(cabCtx.report, cabCtx.avatarGameObject, agm, wearCtx.wearableGameObject, cabCtx.avatarDynamics, wearCtx.wearableDynamics, cabCtx.pathRemapper, cabCtx.cabinetConfig.animationWriteDefaults);

            // TODO: merge disable clips and check for conflicts
            var wearAnimations = animationGenerator.GenerateWearAnimations();
            pairs.Add(wearableIndex + 1, wearAnimations.Item1); // enable clip
            cabCtx.CreateUniqueAsset(wearAnimations.Item1, $"Cabinet_{wearableIndex}_{config.info.name}_Enable.anim");

            // add a new submenu if going to be full
            if (cabinetMenu.GetMenu().controls.Count == 7)
            {
                var newSubMenu = new ExpressionMenuBuilder();
                newSubMenu.CreateAsset(cabCtx.MakeUniqueAssetPath($"Cabinet_Menu_{++cabinetMenuIndex}.asset"));

                cabinetMenu.AddSubMenu("Next Page", newSubMenu.GetMenu(), icon);
                cabinetMenu = newSubMenu;
            }

            if (agm.wearableCustomizables.Count > 0)
            {
                AddCustomizableMenu(cabCtx, config, agm, wearableIndex, refTransition, fxController, pairs, parametersToAdd, animationGenerator, cabinetMenu);
            }
            else
            {
                // Add toggle only
                cabinetMenu.AddToggle(config.info.name, "cpDT_Cabinet", wearableIndex + 1, icon);
            }
        }

        private static void PushCabinetLayerToTop(AnimatorController fxController, int originalLayerLength)
        {
            var layerList = new List<AnimatorControllerLayer>(fxController.layers);
            var cabinetLayer = layerList[layerList.Count - 1];
            layerList.Remove(cabinetLayer);
            layerList.Insert(originalLayerLength, cabinetLayer);
            fxController.layers = layerList.ToArray();
        }

        private static bool TryAddExMenusAndParams(ApplyCabinetContext cabCtx, VRCExpressionParameters exParams, VRCExpressionsMenu exMenu, ExpressionMenuBuilder baseSubMenu, List<VRCExpressionParameters.Parameter> parametersToAdd)
        {
            try
            {
                ExpressionMenuUtils.AddExpressionParameters(exParams, parametersToAdd);
                ExpressionMenuUtils.AddSubMenu(exMenu, "DT Cabinet", baseSubMenu.GetMenu());
                return true;
            }
            catch (ParameterOverflowException ex)
            {
                cabCtx.report.LogExceptionLocalized(t, LogLabel, ex, "integrations.vrc.msgCode.error.parameterOverFlow");
                return false;
            }
        }

        private static bool ApplyAnimationsAndMenu(ApplyCabinetContext cabCtx, VRCCabinetAnimCabinetModuleConfig vrcm, VRCAvatarDescriptor avatarDescriptor)
        {
            EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.makingAnimatorCopy"), 0);

            PrepareAnimatorAndExpressions(cabCtx, avatarDescriptor, out var fxController, out var exParams, out var exMenu, out var parametersToAdd, out var pairs);
            var refTransition = GetStateTransitionReference();

            var baseSubMenu = new ExpressionMenuBuilder();
            baseSubMenu.AddToggle("Original", "cpDT_Cabinet", 0);
            baseSubMenu.CreateAsset(cabCtx.MakeUniqueAssetPath("Cabinet_Menu.asset"));

            // get wearables
            var wearables = DKEditorUtils.GetCabinetWearables(cabCtx.avatarGameObject);
            var cabinetMenu = baseSubMenu;
            var cabinetMenuIndex = 0;
            var originalLayerLength = fxController.layers.Length;

            for (var i = 0; i < wearables.Length; i++)
            {
                // obtain the wearable context
                var wearCtx = cabCtx.wearableContexts[wearables[i]];
                var config = wearCtx.wearableConfig;

                EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.generatingWearableAnimations", config.info.name), i / (float)wearables.Length);

                // find the animation generation module
                var agm = config.FindModuleConfig<CabinetAnimWearableModuleConfig>();
                if (agm == null)
                {
                    continue;
                }

                ApplyWearableAnimationsAndMenus(cabCtx, wearCtx, vrcm, agm, i, refTransition, fxController, pairs, parametersToAdd, ref cabinetMenu, ref cabinetMenuIndex);
            }

            // create cabinet layer
            AnimationUtils.GenerateAnyStateLayer(fxController, "cpDT_Cabinet", "cpDT_Cabinet", pairs, cabCtx.cabinetConfig.animationWriteDefaults, null, refTransition);

            // we push our cabinet layer to the top
            PushCabinetLayerToTop(fxController, originalLayerLength);

            EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.addingExParams"), 1.0f);

            if (!TryAddExMenusAndParams(cabCtx, exParams, exMenu, baseSubMenu, parametersToAdd)) return false;

            EditorUtility.SetDirty(fxController);
            EditorUtility.SetDirty(exMenu);
            EditorUtility.SetDirty(exParams);

            EditorUtility.DisplayProgressBar(t._("tool.name"), t._("integrations.vrc.progressBar.msg.savingAssets"), 1.0f);
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();

            return true;
        }

        public override bool Invoke(ApplyCabinetContext cabCtx, ReadOnlyCollection<CabinetModule> modules, bool isPreview)
        {
            if (isPreview) return true;

            // get the avatar descriptor
            if (!cabCtx.avatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDescriptor))
            {
                // not a vrc avatar
                return true;
            }

            // obtain module
            var vrcm = modules.Count == 0 ?
                new VRCCabinetAnimCabinetModuleConfig() :
                (VRCCabinetAnimCabinetModuleConfig)modules[0].config;

            return ApplyAnimationsAndMenu(cabCtx, vrcm, avatarDescriptor);
        }
    }
}
#endif
