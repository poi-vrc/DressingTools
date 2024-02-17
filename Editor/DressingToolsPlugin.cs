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

using Chocopoi.DressingFramework.Extensibility;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Wearable.Hooks;
using Chocopoi.DressingTools.Menu.Passes;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;

#if DT_VRCSDK3A
using Chocopoi.DressingTools.OneConf.Integration.VRChat.Modules;
using Chocopoi.DressingTools.Animations.Passes.VRChat;
#endif

namespace Chocopoi.DressingTools
{
    internal class DressingToolsPlugin : Plugin
    {
        public override string FriendlyName => "DressingTools";
        public override PluginConstraint Constraint => PluginConstraint.Empty;

        public override void OnEnable()
        {
            RegisterBuildPass(new CabinetAnimCabinetModuleProvider());

            RegisterBuildPass(new ArmatureMappingWearableModuleProvider());
            RegisterBuildPass(new BlendshapeSyncWearableModuleProvider());
            RegisterBuildPass(new CabinetAnimWearableModuleProvider());
            RegisterBuildPass(new MoveRootWearableModuleProvider());
            RegisterBuildPass(new ComposeAndInstallMenuPass());
            RegisterBuildPass(new GroupDynamicsWearablePass());

#if DT_VRCSDK3A
            RegisterBuildPass(new VRCCabinetAnimCabinetModuleProvider());
            RegisterBuildPass(new VRCMergeAnimLayerWearableModuleProvider());
            RegisterBuildPass(new ComposeSmartControlsPass());
#endif
        }

        public override void OnDisable()
        {
        }
    }
}
