/*
 * File: VRCMergeAnimLayerWearableModule.cs
 * Project: DressingTools
 * Created Date: Tuesday, 29th Aug 2023, 02:53:11 pm
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
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Serialization;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Newtonsoft.Json.Linq;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Integrations.VRChat;
using UnityEditor.Animations;
using System.Collections.ObjectModel;
using UnityEngine;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Cabinet;
using VRC.SDKBase;

namespace Chocopoi.DressingTools.Integration.VRChat.Modules
{
    internal class VRCMergeAnimLayerWearableModuleConfig : IModuleConfig
    {
        public enum AnimLayer
        {
            Base = 0,
            Additive = 1,
            Gesture = 2,
            Action = 3,
            FX = 4,
            Sitting = 5,
            TPose = 6,
            IKPose = 7
        }

        public enum PathMode
        {
            Relative = 0,
            Absolute = 1
        }

        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        public SerializationVersion version;
        public AnimLayer animLayer;
        public PathMode pathMode;
        public bool removeAnimatorAfterApply;
        public bool matchLayerWriteDefaults;
        public string animatorPath;

        public VRCMergeAnimLayerWearableModuleConfig()
        {
            version = CurrentConfigVersion;
            animLayer = AnimLayer.FX;
            pathMode = PathMode.Relative;
            removeAnimatorAfterApply = true;
            matchLayerWriteDefaults = true;
            animatorPath = "";
        }

        public static VRCAvatarDescriptor.AnimLayerType? ToAnimLayerType(AnimLayer type)
        {
            switch (type)
            {
                case AnimLayer.Base:
                    return VRCAvatarDescriptor.AnimLayerType.Base;
                case AnimLayer.Additive:
                    return VRCAvatarDescriptor.AnimLayerType.Additive;
                case AnimLayer.Gesture:
                    return VRCAvatarDescriptor.AnimLayerType.Gesture;
                case AnimLayer.Action:
                    return VRCAvatarDescriptor.AnimLayerType.Action;
                case AnimLayer.FX:
                    return VRCAvatarDescriptor.AnimLayerType.FX;
                case AnimLayer.Sitting:
                    return VRCAvatarDescriptor.AnimLayerType.Sitting;
                case AnimLayer.TPose:
                    return VRCAvatarDescriptor.AnimLayerType.TPose;
                case AnimLayer.IKPose:
                    return VRCAvatarDescriptor.AnimLayerType.IKPose;
            }
            return null;
        }
    }

    [InitializeOnLoad]
    internal class VRCMergeAnimLayerWearableModuleProvider : WearableModuleProviderBase
    {
        public class MessageCode
        {
            // Errors
            public const string AnimatorPathNotFound = "integrations.vrc.modules.mergeAnimLayer.msgCode.error.animatorPathNotFound";
            public const string AnimatorNotFound = "integrations.vrc.modules.mergeAnimLayer.msgCode.error.animatorNotFound";
            public const string NoSuchAnimLayer = "integrations.vrc.modules.mergeAnimLayer.msgCode.error.noSuchAnimLayer";
        }
        private static readonly Localization.I18n t = Localization.I18n.Instance;
        private const string LogLabel = "VRCMergeAnimLayer";
        public const string MODULE_IDENTIFIER = "com.chocopoi.dressingtools.integration.vrchat.wearable.merge-anim-layer";

        private const string GestureAvatarMaskGuid = "b2b8bad9583e56a46a3e21795e96ad92";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => MODULE_IDENTIFIER;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("integrations.vrc.modules.mergeAnimLayer.friendlyName");
        [ExcludeFromCodeCoverage] public override int CallOrder => 6;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => true;

        static VRCMergeAnimLayerWearableModuleProvider()
        {
            WearableModuleProviderLocator.Instance.Register(new VRCMergeAnimLayerWearableModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject)
        {
            // TODO: do schema check

            var version = jObject["version"].ToObject<SerializationVersion>();
            if (version.Major > VRCMergeAnimLayerWearableModuleConfig.CurrentConfigVersion.Major)
            {
                throw new System.Exception("Incompatible VRCMergeAnimLayerWearableModuleConfig version: " + version.Major + " > " + VRCMergeAnimLayerWearableModuleConfig.CurrentConfigVersion.Major);
            }

            return jObject.ToObject<VRCMergeAnimLayerWearableModuleConfig>();
        }

        public override IModuleConfig NewModuleConfig() => new VRCMergeAnimLayerWearableModuleConfig();

        private void ObtainOriginalLayerAnimators(Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorController> originalLayerAnimators, VRCAvatarDescriptor.CustomAnimLayer[] animLayers)
        {
            foreach (var animLayer in animLayers)
            {
                originalLayerAnimators[animLayer.type] = VRCEditorUtils.GetAnimLayerAnimator(animLayer);
            }
        }

        private static void DetectLayerWriteDefaultsMode(Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorController> originalLayerAnimators, Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorMerger.WriteDefaultsMode> writeDefaultModes)
        {
            foreach (KeyValuePair<VRCAvatarDescriptor.AnimLayerType, AnimatorController> pair in originalLayerAnimators)
            {
                var stack = new Stack<AnimatorStateMachine>();

                foreach (var layer in pair.Value.layers)
                {
                    stack.Push(layer.stateMachine);
                }

                var writeDefaultsOn = false;
                var writeDefaultsOff = false;

                while (stack.Count > 0)
                {
                    var stateMachine = stack.Pop();
                    foreach (var state in stateMachine.states)
                    {
                        if (state.state.writeDefaultValues)
                        {
                            writeDefaultsOn = true;
                        }
                        else
                        {
                            writeDefaultsOff = true;
                        }
                    }

                    foreach (var childAnimatorMachine in stateMachine.stateMachines)
                    {
                        stack.Push(childAnimatorMachine.stateMachine);
                    }
                }

                if (writeDefaultsOn && writeDefaultsOff)
                {
                    writeDefaultModes[pair.Key] = AnimatorMerger.WriteDefaultsMode.DoNothing;
                }
                else
                {
                    writeDefaultModes[pair.Key] = writeDefaultsOn ? AnimatorMerger.WriteDefaultsMode.On : AnimatorMerger.WriteDefaultsMode.Off;
                }
            }
        }

        private static VRCAvatarDescriptor.CustomAnimLayer[] GetMergedLayers(Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorMerger> mergers, VRCAvatarDescriptor.CustomAnimLayer[] animLayers)
        {
            // create a new array and shallow copy
            var output = new VRCAvatarDescriptor.CustomAnimLayer[animLayers.Length];
            animLayers.CopyTo(output, 0);

            for (var i = 0; i < output.Length; i++)
            {
                // get the merger of such layer
                if (mergers.TryGetValue(output[i].type, out var merger))
                {
                    output[i].animatorController = merger.Merge();
                    output[i].isDefault = false;

                    // add avatar mask for default gesture layer
                    if (output[i].type == VRCAvatarDescriptor.AnimLayerType.Gesture && output[i].isDefault)
                    {
                        var maskPath = AssetDatabase.GUIDToAssetPath(GestureAvatarMaskGuid);
                        if (maskPath == null || maskPath == "")
                        {
                            throw new System.Exception("Unable to find VRC gesture avatar mask by GUID!");
                        }
                        output[i].mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
                    }
                }
            }

            return output;
        }

        public override bool OnApplyWearable(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, ReadOnlyCollection<WearableModule> modules)
        {
            if (modules.Count == 0)
            {
                // no any merge anim layer module
                return true;
            }

            if (!cabCtx.avatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDescriptor))
            {
                // not a VRC avatar
                return true;
            }

            // prepare
            var originalLayerAnimators = new Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorController>();
            var mergerByLayer = new Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorMerger>();
            var writeDefaultModes = new Dictionary<VRCAvatarDescriptor.AnimLayerType, AnimatorMerger.WriteDefaultsMode>();

            ObtainOriginalLayerAnimators(originalLayerAnimators, avatarDescriptor.baseAnimationLayers);
            ObtainOriginalLayerAnimators(originalLayerAnimators, avatarDescriptor.specialAnimationLayers);
            DetectLayerWriteDefaultsMode(originalLayerAnimators, writeDefaultModes);

            foreach (var module in modules)
            {
                var malm = (VRCMergeAnimLayerWearableModuleConfig)module.config;

                // get animator transform
                Transform animatorTransform;
                if (malm.animatorPath == "")
                {
                    // the wearable root contains the animator
                    animatorTransform = wearCtx.wearableGameObject.transform;
                }
                else
                {
                    animatorTransform = wearCtx.wearableGameObject.transform.Find(malm.animatorPath);
                    if (animatorTransform == null)
                    {
                        DTReportUtils.LogErrorLocalized(cabCtx.report, LogLabel, MessageCode.AnimatorPathNotFound, malm.animatorPath);
                        continue;
                    }
                }

                // get animator component
                if (!animatorTransform.TryGetComponent<Animator>(out var animator))
                {
                    DTReportUtils.LogErrorLocalized(cabCtx.report, LogLabel, MessageCode.AnimatorNotFound, animatorTransform.name);
                    continue;
                }

                // rebase path
                var rebasePath = malm.pathMode == VRCMergeAnimLayerWearableModuleConfig.PathMode.Absolute ?
                                "" :
                                AnimationUtils.GetRelativePath(animatorTransform, cabCtx.avatarGameObject.transform);

                var vrcAnimLayer = VRCMergeAnimLayerWearableModuleConfig.ToAnimLayerType(malm.animLayer);
                if (!vrcAnimLayer.HasValue)
                {
                    DTReportUtils.LogErrorLocalized(cabCtx.report, LogLabel, MessageCode.NoSuchAnimLayer);
                    continue;
                }

                // create merger if haven't
                if (!mergerByLayer.TryGetValue(vrcAnimLayer.Value, out var merger))
                {
                    merger = new AnimatorMerger($"{CabinetApplier.GeneratedAssetsPath}/cpDT_VRC_Merged_{malm.animLayer.ToString()}.controller");
                    mergerByLayer[vrcAnimLayer.Value] = merger;

                    // add root controller
                    if (originalLayerAnimators.TryGetValue(vrcAnimLayer.Value, out var animatorController))
                    {
                        merger.AddAnimator("", animatorController, AnimatorMerger.WriteDefaultsMode.DoNothing);
                    }
                }

                // perform merge
                merger.AddAnimator(rebasePath, (AnimatorController)animator.runtimeAnimatorController, writeDefaultModes[vrcAnimLayer.Value]);

                // remove animator after merging
                if (malm.removeAnimatorAfterApply)
                {
                    Object.DestroyImmediate(animator);
                }
            }

            avatarDescriptor.customizeAnimationLayers = true;

            // write merge layers
            avatarDescriptor.baseAnimationLayers = GetMergedLayers(mergerByLayer, avatarDescriptor.baseAnimationLayers);
            avatarDescriptor.specialAnimationLayers = GetMergedLayers(mergerByLayer, avatarDescriptor.specialAnimationLayers);

            return true;
        }
    }
}
#endif
