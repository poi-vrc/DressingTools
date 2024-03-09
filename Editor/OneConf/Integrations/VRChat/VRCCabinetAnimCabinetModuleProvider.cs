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

#if DT_VRCSDK3A
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Animations.VRChat;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Animations.Fluent;
using Chocopoi.DressingTools.OneConf.Animations;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules.Integrations.VRChat;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Passes;
using Newtonsoft.Json.Linq;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

namespace Chocopoi.DressingTools.OneConf.Integration.VRChat.Modules
{
    [InitializeOnLoad]
    internal class VRCCabinetAnimCabinetModuleProvider : CabinetModuleProvider
    {
        private static readonly I18nTranslator t = Localization.I18n.ToolTranslator;

        [ExcludeFromCodeCoverage] public override string Identifier => VRCCabinetAnimCabinetModuleConfig.ModuleIdentifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("integrations.vrc.modules.cabinetAnim.friendlyName");
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;
        [ExcludeFromCodeCoverage]
        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Generation)
                .AfterPass<GroupDynamicsWearablePass>()
                .Build();

        public override IModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<VRCCabinetAnimCabinetModuleConfig>();

        public override IModuleConfig NewModuleConfig() => new VRCCabinetAnimCabinetModuleConfig();

        public override bool Invoke(CabinetContext cabCtx, ReadOnlyCollection<CabinetModule> modules, bool isPreview)
        {
            if (isPreview) return true;

            // get the avatar descriptor
            if (!cabCtx.dkCtx.AvatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDescriptor))
            {
                // not a vrc avatar
                return true;
            }

            // obtain module
            var vrcm = modules.Count == 0 ?
                new VRCCabinetAnimCabinetModuleConfig() :
                (VRCCabinetAnimCabinetModuleConfig)modules[0].config;

            var fxController = VRCAnimUtils.GetAvatarLayerAnimator(avatarDescriptor, VRCAvatarDescriptor.AnimLayerType.FX);

            // get wearables
            var wearables = OneConfUtils.GetCabinetWearables(cabCtx.dkCtx.AvatarGameObject);

            if (wearables.Length > 0)
            {
                var cac = new CabinetAnimComposer(cabCtx.dkCtx, fxController, cabCtx.dkCtx.AvatarGameObject, vrcm.cabinetThumbnails);

                for (var i = 0; i < wearables.Length; i++)
                {
                    // obtain the wearable context
                    var wearCtx = cabCtx.wearableContexts[wearables[i]];
                    var config = wearCtx.wearableConfig;

                    cac.AddWearable(wearCtx.wearableGameObject, config, wearCtx.wearableDynamics);
                }

                cac.Compose();
            }

            EditorUtility.SetDirty(fxController);

            return true;
        }
    }
}
#endif
