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
using Chocopoi.DressingTools.Integration.VRChat.Modules;
using Chocopoi.DressingTools.Integrations.VRChat;
#endif
using Chocopoi.DressingFramework.Extensibility.Plugin;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Cabinet.Modules;
using Chocopoi.DressingTools.Wearable.Modules;

namespace Chocopoi.DressingTools.Plugin
{
    internal class DressingToolsPlugin : PluginBase
    {
        public const string PluginIdentifier = "com.chocopoi.vrc.dressingtools.plugin";

        public override string Identifier => PluginIdentifier;
        public override string FriendlyName => "DressingTools";
        public override ExecutionConstraint Constraint => ExecutionConstraint.Empty;

        public override void OnEnable()
        {
            RegisterCabinetModuleProvider(new CabinetAnimCabinetModuleProvider());

            RegisterWearableModuleProvider(new ArmatureMappingWearableModuleProvider());
            RegisterWearableModuleProvider(new BlendshapeSyncWearableModuleProvider());
            RegisterFrameworkEventHandler(new BlendshapeSyncWearableModuleProvider.EventAdapter());
            RegisterWearableModuleProvider(new CabinetAnimWearableModuleProvider());
            RegisterFrameworkEventHandler(new CabinetAnimWearableModuleProvider.EventAdapter());
            RegisterWearableModuleProvider(new MoveRootWearableModuleProvider());

#if VRC_SDK_VRCSDK3
            RegisterCabinetHook(new VRCScanAnimationsCabinetHook());
            RegisterWearableModuleProvider(new VRChatIntegrationWearableModuleProvider());
            RegisterWearableModuleProvider(new VRCMergeAnimLayerWearableModuleProvider());
#endif
        }

        public override void OnDisable()
        {
        }
    }
}
