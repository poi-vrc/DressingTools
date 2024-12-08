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

using System.Collections.ObjectModel;
using Chocopoi.DressingFramework.Components;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Components.OneConf;
// using Chocopoi.DressingTools.Configurator.Modules;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using UnityEngine;
using UnityEngine.UIElements;

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

        // public VisualElement CreateView()
        // {
        //     throw new System.NotImplementedException();
        // }

        // public List<IModule> GetModules()
        // {
        //     var modules = new List<IModule>();
        //     if (!WearableConfigUtility.TryDeserialize(_wearableComp.ConfigJson, out var config))
        //     {
        //         Debug.LogError("[DressingTools] Unable to deserialize OneConf wearable config, returning empty modules");
        //         return modules;
        //     }

        //     foreach (var oneConfModule in config.modules)
        //     {
        //         // TODO
        //         if (oneConfModule.moduleName == ArmatureMappingWearableModuleConfig.ModuleIdentifier &&
        //             oneConfModule.config is ArmatureMappingWearableModuleConfig)
        //         {
        //             modules.Add(new OneConfArmatureMappingModule(_avatarGameObject, _wearableComp));
        //         }
        //     }

        //     return modules;
        // }

        public void Preview(GameObject previewAvatarGameObject, GameObject previewOutfitGameObject)
        {
            if (previewAvatarGameObject == null || previewOutfitGameObject == null)
            {
                return;
            }

            var cabinet = OneConfUtils.GetAvatarCabinet(previewAvatarGameObject);
            if (cabinet == null)
            {
                return;
            }

            if (!CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out var cabinetConfig))
            {
                Debug.LogError("[DressingTools] Unable to deserialize cabinet config for preview");
                return;
            }

            if (!WearableConfigUtility.TryDeserialize(_wearableComp.ConfigJson, out var wearableConfig))
            {
                Debug.LogError("[DressingTools] Unable to deserialize OneConf wearable config for preview");
                return;
            }

            var dkCtx = new DKNativeContext(previewAvatarGameObject);
            var cabCtx = new CabinetContext
            {
                dkCtx = dkCtx,
                cabinetConfig = cabinetConfig
            };

            var wearCtx = new WearableContext
            {
                wearableConfig = wearableConfig,
                wearableGameObject = previewOutfitGameObject,
                wearableDynamics = DynamicsUtils.ScanDynamics(previewOutfitGameObject)
            };

            var providers = ModuleManager.Instance.GetAllWearableModuleProviders();

            foreach (var provider in providers)
            {
                if (!provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(wearableConfig.FindModules(provider.Identifier)), true))
                {
                    Debug.LogError("[DressingTools] Error applying wearable in preview!");
                    break;
                }
            }

            // remove all DK components
            var dkComps = previewAvatarGameObject.GetComponentsInChildren<DKBaseComponent>();
            foreach (var comp in dkComps)
            {
                Object.DestroyImmediate(comp);
            }
        }
    }
}
