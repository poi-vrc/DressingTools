/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.UI.Views.Modules;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Configurator.Modules
{
    internal class OneConfArmatureMappingModule : OneConfModuleBase, IArmatureMergingModule
    {
        public Transform TargetArmature
        {
            get
            {
                ReadCabinetConfig(out _, out var config);
                return string.IsNullOrEmpty(config.avatarArmatureName) ?
                    null :
                    OneConfUtils.GuessArmature(_avatarGameObject, config.avatarArmatureName);
            }
            set
            {
                WriteCabinetConfig((comp, config) =>
                {
                    config.avatarArmatureName =
                        value == null ?
                        "" :
                        AnimationUtils.GetRelativePath(value.transform, comp.transform);
                });
            }
        }
        public Transform SourceArmature
        {
            get
            {
                ReadWearableModule<ArmatureMappingWearableModuleConfig>(out var module);
                if (module == null) return null;
                return string.IsNullOrEmpty(module.wearableArmatureName) ?
                    null :
                    OneConfUtils.GuessArmature(_wearableComp.gameObject, module.wearableArmatureName);
            }
            set
            {
                WriteWearableModule<ArmatureMappingWearableModuleConfig>(module =>
                {
                    module.wearableArmatureName =
                        value == null ?
                        "" :
                        AnimationUtils.GetRelativePath(value.transform, _wearableComp.transform);
                });
            }
        }

        public OneConfArmatureMappingModule(GameObject avatarGameObject, DTWearable wearableComp)
        : base(avatarGameObject, wearableComp)
        {
        }

        public override VisualElement CreateView()
        {
            throw new NotImplementedException();
        }
    }
}
