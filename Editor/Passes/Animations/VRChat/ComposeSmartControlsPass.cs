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

#if DT_VRCSDK3A
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations.VRChat;
using Chocopoi.DressingFramework.Detail.DK.Passes;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Animations.Fluent;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.Passes.Menu;
using Chocopoi.DressingTools.Passes.Modifiers;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

namespace Chocopoi.DressingTools.Passes.Animations.VRChat
{
    // TODO: this currently depends on VRC because we still need to find a way to allow user to supply animator
    internal class ComposeSmartControlsPass : BuildPass
    {
        private const string LogLabel = "SmartControlComposer";
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public override BuildConstraint Constraint => InvokeAtStage(BuildStage.Transpose)
            .BeforePass<ComposeAndInstallMenuPass>()
            .BeforePass<ComposeAnimatorParametersPass>()
            .BeforePass<RemapAnimationsPass>()
            .BeforePass<GroupDynamicsPass>()
            .BeforePass<GroupDynamicsModifyAnimPass>()
            .Build();

        // for mocking in tests
        internal UI ui = new UnityUI();

        public override bool Invoke(Context ctx)
        {
            if (!ctx.AvatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDesc))
            {
                // not a VRC avatar
                return true;
            }

            // TODO: tmeporarily use the setting from here, there should be another component storing config later on
            if (!ctx.AvatarGameObject.TryGetComponent<DTCabinet>(out var cabinetComp) ||
                !CabinetConfigUtility.TryDeserialize(cabinetComp.ConfigJson, out var config))
            {
                // use default config if not exist
                config = new CabinetConfig();
            }

            // TODO: allow switching to another layer
            var fx = VRCAnimUtils.GetAvatarLayerAnimator(avatarDesc, VRCAvatarDescriptor.AnimLayerType.FX);
            var ctrlComps = ctx.AvatarGameObject.GetComponentsInChildren<DTSmartControl>(true);

            bool writeDefaults;
            if (config.animationWriteDefaultsMode == CabinetConfig.WriteDefaultsMode.Auto)
            {
                AnimUtils.GetWriteDefaultCounts(fx, out var onCount, out var offCount);
                ctx.Report.LogInfo(LogLabel, $"Write defaults count: {onCount} on {offCount} off");
                if (onCount != 0 && offCount != 0)
                {
                    ui.ShowInconsistentWriteDefaultsDialog(onCount, offCount);
                }
                writeDefaults = AnimUtils.DetermineWriteDefaultsByOnOffCounts(onCount, offCount);
                ctx.Report.LogInfo(LogLabel, $"Detected write defaults {(writeDefaults ? "on" : "off")}");
            }
            else
            {
                writeDefaults = config.animationWriteDefaultsMode == CabinetConfig.WriteDefaultsMode.On;
                ctx.Report.LogInfo(LogLabel, $"Explicitly specified to use write defaults {(writeDefaults ? "on" : "off")}");
            }

            var options = new AnimatorOptions()
            {
                rootTransform = ctx.AvatarGameObject.transform,
                context = ctx,
                writeDefaults = writeDefaults
            };

            // compose into animator layers and clips
            var composer = new SmartControlComposer(options, fx);
            foreach (var ctrl in ctrlComps)
            {
                composer.Compose(ctrl);
            }
            composer.Finish();

            EditorUtility.SetDirty(fx);

            return true;
        }

        internal interface UI
        {
            void ShowInconsistentWriteDefaultsDialog(int onCount, int offCount);
        }

        private class UnityUI : UI
        {
            public void ShowInconsistentWriteDefaultsDialog(int onCount, int offCount)
            {
                EditorUtility.DisplayDialog(t._("tool.name"), t._("passes.anims.vrc.composeSmartControls.dialog.msg.inconsistentWriteDefaults", onCount, offCount), t._("common.dialog.btn.ok"));
            }
        }
    }
}
#endif
