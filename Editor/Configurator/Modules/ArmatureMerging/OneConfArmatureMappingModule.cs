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

using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.UI.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.Configurator.Modules
{
    internal class OneConfArmatureMappingModule : IArmatureMergingModule
    {
        public Transform TargetArmature
        {
            get
            {
                if (!_avatarGameObject.TryGetComponent<DTCabinet>(out var cabinetComp) ||
                    !CabinetConfigUtility.TryDeserialize(cabinetComp.ConfigJson, out var cabinetConfig))
                {
                    cabinetConfig = new CabinetConfig();
                }
                if (string.IsNullOrEmpty(cabinetConfig.avatarArmatureName))
                {
                    return null;
                }
                return OneConfUtils.GuessArmature(_avatarGameObject, cabinetConfig.avatarArmatureName);
            }
            set
            {
                CabinetConfig cabinetConfig;
                if (_avatarGameObject.TryGetComponent<DTCabinet>(out var cabinetComp))
                {
                    if (!CabinetConfigUtility.TryDeserialize(cabinetComp.ConfigJson, out cabinetConfig))
                    {
                        cabinetConfig = new CabinetConfig();
                    }
                }
                else
                {
                    cabinetComp = _avatarGameObject.AddComponent<DTCabinet>();
                    cabinetConfig = new CabinetConfig();
                }
                cabinetConfig.avatarArmatureName =
                    value == null ?
                    "" :
                    AnimationUtils.GetRelativePath(value.transform, cabinetComp.transform);
                cabinetComp.ConfigJson = CabinetConfigUtility.Serialize(cabinetConfig);
            }
        }
        public Transform SourceArmature
        {
            get
            {
                if (!WearableConfigUtility.TryDeserialize(_wearableComp.ConfigJson, out var config))
                {
                    return null;
                }
                var module = config.FindModuleConfig<ArmatureMappingWearableModuleConfig>();
                if (module == null || string.IsNullOrEmpty(module.wearableArmatureName))
                {
                    return null;
                }
                return OneConfUtils.GuessArmature(_wearableComp.gameObject, module.wearableArmatureName);
            }
            set
            {
                if (!WearableConfigUtility.TryDeserialize(_wearableComp.ConfigJson, out var config))
                {
                    return;
                }
                var module = config.FindModuleConfig<ArmatureMappingWearableModuleConfig>();
                if (module == null)
                {
                    return;
                }
                module.wearableArmatureName =
                    value == null ?
                    "" :
                    AnimationUtils.GetRelativePath(value.transform, _wearableComp.transform);
                _wearableComp.ConfigJson = WearableConfigUtility.Serialize(config);
            }
        }

        private readonly GameObject _avatarGameObject;
        private readonly DTWearable _wearableComp;

        public OneConfArmatureMappingModule(GameObject avatarGameObject, DTWearable wearableComp)
        {
            _avatarGameObject = avatarGameObject;
            _wearableComp = wearableComp;
        }

        public ElementView CreateView()
        {
            throw new System.NotImplementedException();
        }
    }
}
