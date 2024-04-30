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

using System.Collections.Generic;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Configurator.Modules;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.UI.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.Configurator.Cabinet
{
    internal class OneConfConfigurableOutfit : IConfigurableOutfit
    {
        public Transform RootTransform
        {
            get => _wearableComp.RootGameObject.transform;
        }
        public string Name
        {
            get => WearableConfigUtility.TryDeserialize(_wearableComp.ConfigJson, out var config) ?
                config.info.name :
                "(Error)";

        }
        public Texture2D Icon
        {
            get => WearableConfigUtility.TryDeserialize(_wearableComp.ConfigJson, out var config) ?
                (string.IsNullOrEmpty(config.info.thumbnail) ?
                    null :
                    OneConfUtils.GetTextureFromBase64(config.info.thumbnail)) :
                null;
        }

        private readonly GameObject _avatarGameObject;
        private readonly DTWearable _wearableComp;

        public OneConfConfigurableOutfit(GameObject avatarGameObject, DTWearable wearableComp)
        {
            _avatarGameObject = avatarGameObject;
            _wearableComp = wearableComp;
        }

        public ElementView CreateView()
        {
            throw new System.NotImplementedException();
        }

        public List<IModule> GetModules()
        {
            var modules = new List<IModule>();
            if (!WearableConfigUtility.TryDeserialize(_wearableComp.ConfigJson, out var config))
            {
                Debug.LogError("[DressingTools] Unable to deserialize OneConf wearable config, returning empty modules");
                return modules;
            }

            foreach (var oneConfModule in config.modules)
            {
                // TODO
                if (oneConfModule.moduleName == ArmatureMappingWearableModuleConfig.ModuleIdentifier &&
                    oneConfModule.config is ArmatureMappingWearableModuleConfig)
                {
                    modules.Add(new OneConfArmatureMappingModule(_avatarGameObject, _wearableComp));
                }
            }

            return modules;
        }
    }
}
