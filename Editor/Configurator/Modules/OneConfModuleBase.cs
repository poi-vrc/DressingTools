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
using Chocopoi.DressingTools.OneConf.Wearable;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Configurator.Modules
{
    internal abstract class OneConfModuleBase : IModule
    {
        protected readonly GameObject _avatarGameObject;
        protected readonly DTWearable _wearableComp;

        public OneConfModuleBase(GameObject avatarGameObject, DTWearable wearableComp)
        {
            _avatarGameObject = avatarGameObject;
            _wearableComp = wearableComp;
        }

        public abstract VisualElement CreateView();

        protected void ReadCabinetConfig(out DTCabinet comp, out CabinetConfig config)
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

        protected void WriteCabinetConfig(Action<DTCabinet, CabinetConfig> func)
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

        protected void ReadWearableConfig(out WearableConfig config)
        {
            if (!WearableConfigUtility.TryDeserialize(_wearableComp.ConfigJson, out config))
            {
                config = null;
            }
        }

        protected void ReadWearableModule<T>(out T module) where T : IModuleConfig
        {
            ReadWearableConfig(out var config);
            if (config == null)
            {
                module = default;
                return;
            }
            module = config.FindModuleConfig<T>();
        }

        protected void WriteWearableConfig(Action<WearableConfig> func)
        {
            if (!WearableConfigUtility.TryDeserialize(_wearableComp.ConfigJson, out var config))
            {
                return;
            }
            func?.Invoke(config);
            _wearableComp.ConfigJson = WearableConfigUtility.Serialize(config);
        }

        protected void WriteWearableModule<T>(Action<T> func) where T : IModuleConfig
        {
            WriteWearableConfig((config) =>
            {
                var module = config.FindModuleConfig<T>();
                if (module == null)
                {
                    return;
                }
                func?.Invoke(module);
            });
        }
    }
}
