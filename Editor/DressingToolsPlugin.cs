﻿/*
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
using Chocopoi.DressingTools.OneConf.Cabinet.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.Passes.Animations;
using Chocopoi.DressingTools.Passes.Menu;
using Chocopoi.DressingTools.Passes.Modifiers;
using Chocopoi.DressingTools.Passes.Optimization;
#if DT_VRCSDK3A
using Chocopoi.DressingTools.Passes.Animations.VRChat;
using Chocopoi.DressingTools.OneConf.Integration.VRChat.Modules;
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

            RegisterBuildPass(new BlendshapeSyncPass());
            RegisterBuildPass(new ArmatureMappingPass());
            RegisterBuildPass(new CopyDynamicsPass());
            RegisterBuildPass(new GroupDynamicsPass());
            RegisterBuildPass(new GroupDynamicsModifyAnimPass());
            RegisterBuildPass(new IgnoreDynamicsPass());
            RegisterBuildPass(new MoveRootPass());
            RegisterBuildPass(new ObjectMappingPass());
            RegisterBuildPass(new WardrobePass());

            RegisterBuildPass(new ComposeAndInstallMenuPass());
            RegisterBuildPass(new ComposeAnimatorParametersPass());
            RegisterBuildPass(new GCGameObjectsPass());

#if DT_VRCSDK3A
            RegisterBuildPass(new VRCMergeAnimLayerWearableModuleProvider());
            RegisterBuildPass(new ComposeSmartControlsPass());
#endif
        }

        public override void OnDisable()
        {
        }
    }
}
