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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.OneConf.Animations;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Serialization;
using Newtonsoft.Json.Linq;
using UnityEditor;
#if DT_VRCSDK3A
using VRC.SDK3.Avatars.Components;
using Chocopoi.DressingFramework.Animations.VRChat;
#endif

namespace Chocopoi.DressingTools.OneConf.Cabinet.Modules
{
    [InitializeOnLoad]
    internal class CabinetAnimCabinetModuleProvider : CabinetModuleProvider
    {
        [ExcludeFromCodeCoverage] public override string Identifier => CabinetAnimCabinetModuleConfig.ModuleIdentifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Cabinet Animation";
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;
        [ExcludeFromCodeCoverage]
        public override BuildConstraint Constraint => InvokeAtStage(BuildStage.Generation).Build();

        public override IModuleConfig DeserializeModuleConfig(JObject jObject)
        {
            // TODO: do schema check

            var version = jObject["version"].ToObject<SerializationVersion>();
            if (version.Major > CabinetAnimCabinetModuleConfig.CurrentConfigVersion.Major)
            {
                throw new System.Exception("Incompatible CabinetAnimCabinetModule version: " + version.Major + " > " + CabinetAnimCabinetModuleConfig.CurrentConfigVersion.Major);
            }

            return jObject.ToObject<CabinetAnimCabinetModuleConfig>();
        }

        public override IModuleConfig NewModuleConfig() => new CabinetAnimCabinetModuleConfig();

#if DT_VRCSDK3A
        private void InvokeForVRC(CabinetContext cabCtx, CabinetAnimCabinetModuleConfig cabm, VRCAvatarDescriptor avatarDescriptor)
        {
            // get the avatar descriptor
            var fxController = VRCAnimUtils.GetAvatarLayerAnimator(avatarDescriptor, VRCAvatarDescriptor.AnimLayerType.FX);

            // get wearables
            var wearables = OneConfUtils.GetCabinetWearables(cabCtx.dkCtx.AvatarGameObject);

            var cac = new CabinetAnimComposer(cabCtx.dkCtx, fxController, cabCtx.dkCtx.AvatarGameObject, cabm);

            for (var i = 0; i < wearables.Length; i++)
            {
                // obtain the wearable context
                var wearCtx = cabCtx.wearableContexts[wearables[i]];
                var config = wearCtx.wearableConfig;

                cac.AddWearable(wearCtx.wearableGameObject, config, wearCtx.wearableDynamics);
            }

            cac.Compose();

            EditorUtility.SetDirty(fxController);
        }
#endif

        public override bool Invoke(CabinetContext cabCtx, ReadOnlyCollection<CabinetModule> modules, bool isPreview)
        {
            if (isPreview) return true;

            // obtain module
            var cabm = modules.Count == 0 ?
                new CabinetAnimCabinetModuleConfig() :
                (CabinetAnimCabinetModuleConfig)modules[0].config;

#if DT_VRCSDK3A
            if (cabCtx.dkCtx.AvatarGameObject.TryGetComponent<VRCAvatarDescriptor>(out var avatarDescriptor))
            {
                InvokeForVRC(cabCtx, cabm, avatarDescriptor);
                return true;
            }
#endif

            return true;
        }
    }
}
