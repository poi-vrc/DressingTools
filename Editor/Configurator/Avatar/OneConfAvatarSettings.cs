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
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Serialization;
using UnityEngine;

namespace Chocopoi.DressingTools.Configurator.Avatar
{
    internal class OneConfAvatarSettings : IAvatarSettings
    {
        public WriteDefaultsModes WriteDefaultsMode
        {
            get
            {
                ReadCabinetConfig(out _, out var config);
                // TODO: should explicit convert them one by one
                return (WriteDefaultsModes)config.animationWriteDefaultsMode;
            }
            set
            {
                WriteCabinetConfig((comp, config) =>
                {
                    // TODO: should explicit convert them one by one
                    config.animationWriteDefaultsMode = (CabinetConfig.WriteDefaultsMode)value;
                });
            }
        }

        private readonly GameObject _avatarGameObject;

        public OneConfAvatarSettings(GameObject avatarGameObject)
        {
            _avatarGameObject = avatarGameObject;
        }

        private void ReadCabinetConfig(out DTCabinet comp, out CabinetConfig config)
        {
            if (_avatarGameObject.TryGetComponent(out comp))
            {
                if (!CabinetConfigUtility.TryDeserialize(comp.ConfigJson, out config))
                {
                    config = new CabinetConfig();
                }
            }
            else
            {
                comp = null;
                config = new CabinetConfig();
            }
        }

        private void WriteCabinetConfig(Action<DTCabinet, CabinetConfig> func)
        {
            CabinetConfig config;
            if (_avatarGameObject.TryGetComponent<DTCabinet>(out var cabinetComp))
            {
                if (!CabinetConfigUtility.TryDeserialize(cabinetComp.ConfigJson, out config))
                {
                    config = new CabinetConfig();
                }
            }
            else
            {
                cabinetComp = _avatarGameObject.AddComponent<DTCabinet>();
                config = new CabinetConfig();
            }
            func?.Invoke(cabinetComp, config);
            cabinetComp.ConfigJson = CabinetConfigUtility.Serialize(config);
        }
    }
}
