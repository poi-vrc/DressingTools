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

#if DT_VRCSDK3A
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.Integrations.VRChat;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Integration.VRChat.Modules
{
    [InitializeOnLoad]
    internal class VRCMergeAnimLayerWearableModuleProvider : WearableModuleProvider
    {
        public class MessageCode
        {
        }
        private const string LogLabel = "VRCMergeAnimLayerWearableModuleProvider";
        private static readonly I18nTranslator t = Localization.I18n.ToolTranslator;

        [ExcludeFromCodeCoverage] public override string Identifier => VRCMergeAnimLayerWearableModuleConfig.ModuleIdentifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("integrations.vrc.modules.mergeAnimLayer.friendlyName");
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => true;
        [ExcludeFromCodeCoverage] public override BuildConstraint Constraint => InvokeAtStage(BuildStage.Transpose).Build();

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

        public override bool Invoke(CabinetContext cabCtx, WearableContext wearCtx, ReadOnlyCollection<WearableModule> modules, bool isPreview)
        {
            if (isPreview) return true;

            if (modules.Count == 0)
            {
                // no any merge anim layer module
                return true;
            }

            foreach (var module in modules)
            {
                var config = (VRCMergeAnimLayerWearableModuleConfig)module.config;

                if (string.IsNullOrEmpty(config.animatorPath))
                {
                    cabCtx.dkCtx.Report.LogWarn(LogLabel, $"VRC merge animator path specified is empty, skipping: {wearCtx.wearableConfig.info.name}");
                    continue;
                }

                var animatorTrans = wearCtx.wearableGameObject.transform.Find(config.animatorPath);
                if (animatorTrans == null || !animatorTrans.TryGetComponent<Animator>(out var animator))
                {
                    cabCtx.dkCtx.Report.LogWarn(LogLabel, $"VRC merge animator not found at path \"{config.animatorPath}\", skipping: {wearCtx.wearableConfig.info.name}");
                    continue;
                }

                var comp = wearCtx.wearableGameObject.AddComponent<DTManipulateAnimator>();
                comp.ManipulateMode = DTManipulateAnimator.ManipulateModes.Add;
                comp.TargetType = DTManipulateAnimator.TargetTypes.VRCAnimLayer;
                comp.VRCTargetLayer = VRCMergeAnimLayerWearableModuleConfig.ToAnimLayerType(config.animLayer).Value;
                comp.PathMode = (DTManipulateAnimator.PathModes)config.pathMode;
                comp.SourceType = DTManipulateAnimator.SourceTypes.Animator;
                comp.SourceAnimator = animator;
            }

            return true;
        }
    }
}
#endif
