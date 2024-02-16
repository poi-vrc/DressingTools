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
using Chocopoi.DressingFramework.Detail.DK.Passes.VRChat;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Animations.Fluent;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Menu.Passes;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

namespace Chocopoi.DressingTools.Animations.Passes.VRChat
{
    // TODO: this currently depends on VRC because we still need to find a way to allow user to supply animator
    internal class ComposeSmartControlsPass : BuildPass
    {
        public override BuildConstraint Constraint => InvokeAtStage(BuildStage.Transpose)
            .BeforePass<ComposeAndInstallMenuPass>()
            .BeforePass<ApplyVRCExParamsPass>()
            .Build();

        public override bool Invoke(Context ctx)
        {
            if (!ctx.AvatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDesc))
            {
                // not a VRC avatar
                return true;
            }

            // TODO: allow switching to another layer
            var fx = VRCAnimUtils.GetAvatarLayerAnimator(avatarDesc, VRCAvatarDescriptor.AnimLayerType.FX);
            var ctrlComps = ctx.AvatarGameObject.GetComponentsInChildren<DTSmartControl>(true);

            var options = new AnimatorOptions()
            {
                rootTransform = ctx.AvatarGameObject.transform,
                context = ctx,
                writeDefaultsMode = AnimatorOptions.DetectWriteDefaultsMode(fx) // TODO: configurable
            };

            // compose into animator layers and clips
            var composer = new SmartControlComposer(options, fx);
            foreach (var ctrl in ctrlComps)
            {
                composer.Compose(ctrl);
            }

            EditorUtility.SetDirty(fx);

            return true;
        }
    }
}
#endif
