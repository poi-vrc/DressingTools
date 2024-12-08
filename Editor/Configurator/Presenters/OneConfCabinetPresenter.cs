/*
 * File: CabinetPresenter.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 9th 2023, 11:38:52 pm
 * Author: chocopoi (poi@chocopoi.com)
 * -----
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

using System.Collections.Generic;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Configurator.Views;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chocopoi.DressingTools.Configurator.Presenters
{
    internal class OneConfCabinetPresenter
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private IOneConfCabinetView _view;
        private GameObject _avatarGameObject;

        public OneConfCabinetPresenter(IOneConfCabinetView view, GameObject avatarGameObject)
        {
            _view = view;
            _avatarGameObject = avatarGameObject;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.SettingsChanged += OnSettingsChanged;
            _view.ForceUpdateView += OnForceUpdateView;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.SettingsChanged -= OnSettingsChanged;
            _view.ForceUpdateView -= OnForceUpdateView;
        }

        private void OnSettingsChanged()
        {
            var cabinet = OneConfUtils.GetAvatarCabinet(_avatarGameObject, true);
            if (!CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out var config))
            {
                config = new CabinetConfig();
            }

            config.avatarArmatureName = _view.ArmatureName;
            config.groupDynamics = _view.GroupDynamics;
            config.groupDynamicsSeparateGameObjects = _view.GroupDynamicsSeparate;

            var cabAnimConfig = config.FindModuleConfig<CabinetAnimCabinetModuleConfig>();
            if (cabAnimConfig == null)
            {
                cabAnimConfig = new CabinetAnimCabinetModuleConfig();
                config.modules.Add(new CabinetModule()
                {
                    config = cabAnimConfig,
                    moduleName = CabinetAnimCabinetModuleConfig.ModuleIdentifier
                });
            }

            cabAnimConfig.thumbnails = _view.UseThumbnails;
            cabAnimConfig.resetCustomizablesOnSwitch = _view.ResetCustomizablesOnSwitch;
            cabAnimConfig.menuInstallPath = _view.MenuInstallPathField;
            cabAnimConfig.menuItemName = _view.MenuItemNameField;
            cabAnimConfig.networkSynced = _view.NetworkSyncedToggle;
            cabAnimConfig.saved = _view.SavedToggle;

            cabinet.ConfigJson = CabinetConfigUtility.Serialize(config);
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            var cabinet = OneConfUtils.GetAvatarCabinet(_avatarGameObject);
            if (cabinet == null || !CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out var config))
            {
                config = new CabinetConfig();
            }

            _view.ArmatureName = config.avatarArmatureName;
            _view.GroupDynamics = config.groupDynamics;
            _view.GroupDynamicsSeparate = config.groupDynamicsSeparateGameObjects;

            var cabAnimConfig = config.FindModuleConfig<CabinetAnimCabinetModuleConfig>();
            if (cabAnimConfig == null)
            {
                cabAnimConfig = new CabinetAnimCabinetModuleConfig();
            }

            _view.UseThumbnails = cabAnimConfig.thumbnails;
            _view.ResetCustomizablesOnSwitch = cabAnimConfig.resetCustomizablesOnSwitch;
            _view.MenuInstallPathField = cabAnimConfig.menuInstallPath;
            _view.MenuItemNameField = cabAnimConfig.menuItemName;
            _view.NetworkSyncedToggle = cabAnimConfig.networkSynced;
            _view.SavedToggle = cabAnimConfig.saved;

            _view.Repaint();
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
