#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.AvatarLib.Expressions;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Chocopoi.DressingTools.Integrations.VRC
{
    internal class GenerateAnimationsHook : IBuildDTCabinetHook
    {
        private DTCabinet cabinet;

        public GenerateAnimationsHook(DTCabinet cabinet)
        {
            this.cabinet = cabinet;
        }

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            EditorUtility.DisplayProgressBar("DressingTools", "Generating animations...", 0);

            var wearables = cabinet.GetWearables();
            var report = new DTReport();

            var emptyClip = new AnimationClip();
            AssetDatabase.CreateAsset(emptyClip, "Assets/_DTTemporaryAssets/EmptyClip.anim");
            var pairs = new Dictionary<int, Motion>
            {
                { 0, emptyClip }
            };

            for (var i = 0; i < wearables.Length; i++)
            {
                EditorUtility.DisplayProgressBar("DressingTools", "Generating animations for " + wearables[i].config.info.name + "...", i / (float)wearables.Length * 100);
                var wearableDynamics = DTRuntimeUtils.ScanDynamics(wearables[i].wearableGameObject);
                var animationGenerator = new AnimationGenerator(report, avatarGameObject, wearables[i].config, wearables[i].wearableGameObject, wearableDynamics);

                // TODO: write defaults settings
                var wearAnimations = animationGenerator.GenerateWearAnimations(true);
                pairs.Add(i + 1, wearAnimations.Item1); // enable clip
                AssetDatabase.CreateAsset(wearAnimations.Item1, "Assets/_DTTemporaryAssets/cpDT_" + wearables[i].name + ".anim");
            }

            // get the avatar descriptor
            var avatarDescriptor = avatarGameObject.GetComponent<VRCAvatarDescriptor>();

            // obtain FX layer
            var fxController = GetLayerAnimator(avatarDescriptor, VRCAvatarDescriptor.AnimLayerType.FX);

            // TODO: these operations are just temporary for testing, need rework to do exception catching etc
            
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
                hasExitTime = true,
                hasFixedDuration = true,
                isExit = false,
                mute = false,
                offset = 0,
                orderedInterruption = true,
                solo = false,
                conditions = new AnimatorCondition[] { }
            };
            
            AnimationUtils.GenerateAnyStateLayer(fxController, "cpDT_Cabinet", "cpDT_Cabinet", pairs, true, null, refTransition);
            
            ExpressionMenuUtils.RemoveExpressionParameters(avatarDescriptor.expressionParameters, "^cpDT_Cabinet");
            ExpressionMenuUtils.AddExpressionParameters(avatarDescriptor.expressionParameters, new VRCExpressionParameters.Parameter[]
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
            
            ExpressionMenuUtils.RemoveExpressionMenuControls(avatarDescriptor.expressionsMenu, "DT Cabinet");
            
            EditorUtility.DisplayProgressBar("DressingTools", "Generating expression menu...", 0);
            var subMenu = new ExpressionMenuBuilder(avatarDescriptor.expressionsMenu)
                .BeginNewSubMenu("DT Cabinet");
            
            subMenu.AddToggle("Original", "cpDT_Cabinet", 0);
            for (var i = 0; i < wearables.Length; i++)
            {
                subMenu.AddToggle(wearables[i].config.info.name, "cpDT_Cabinet", i + 1);
            }
            
            subMenu.CreateAsset("Assets/_DTTemporaryAssets/cpDT_Cabinet.asset")
                .EndNewSubMenu();
            
            AssetDatabase.SaveAssets();

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

        private AnimatorController GetLayerAnimator(VRCAvatarDescriptor avatarDescriptor, VRCAvatarDescriptor.AnimLayerType animLayerType)
        {
            VRCAvatarDescriptor.CustomAnimLayer? nullableAnimLayer = null;

            foreach (var layer in avatarDescriptor.baseAnimationLayers)
            {
                if (layer.type == animLayerType)
                {
                    nullableAnimLayer = layer;
                }
            }

            if (nullableAnimLayer == null)
            {
                return null;
            }

            var animLayer = nullableAnimLayer.Value;

            if (!animLayer.isDefault && animLayer.animatorController != null)
            {
                return (AnimatorController)animLayer.animatorController;
            }

            return GetDefaultLayerAnimator(animLayerType);
        }
    }
}
#endif
