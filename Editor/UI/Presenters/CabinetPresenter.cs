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

using System;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class CabinetPresenter
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private ICabinetSubView _view;
        private CabinetConfig _cabinetConfig;

        public CabinetPresenter(ICabinetSubView view)
        {
            _view = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.AddWearableButtonClick += OnAddWearableButtonClick;
            _view.ForceUpdateView += OnForceUpdateView;
            _view.SelectedCabinetChange += OnSelectedCabinetChange;
            _view.CabinetSettingsChange += OnCabinetSettingsChange;
            _view.ToolbarCreateCabinetButtonClick += OnToolbarCreateCabinetButtonClick;
            _view.CreateCabinetStartButtonClick += OnCreateCabinetStartButtonClick;
            _view.CreateCabinetBackButtonClick += OnCreateCabinetBackButtonClick;

            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.AddWearableButtonClick -= OnAddWearableButtonClick;
            _view.ForceUpdateView -= OnForceUpdateView;
            _view.SelectedCabinetChange -= OnSelectedCabinetChange;
            _view.CabinetSettingsChange -= OnCabinetSettingsChange;
            _view.ToolbarCreateCabinetButtonClick -= OnToolbarCreateCabinetButtonClick;
            _view.CreateCabinetStartButtonClick -= OnCreateCabinetStartButtonClick;
            _view.CreateCabinetBackButtonClick -= OnCreateCabinetBackButtonClick;

            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                UpdateView();
            }
        }

        private void OnCreateCabinetBackButtonClick()
        {
            _view.ShowCreateCabinetPanel = false;
            UpdateView();
        }

        private void OnCreateCabinetStartButtonClick()
        {
            if (_view.CreateCabinetAvatarGameObject == null)
            {
                return;
            }
            OneConfUtils.GetAvatarCabinet(_view.CreateCabinetAvatarGameObject, true);
            _view.ShowCreateCabinetPanel = false;
            UpdateView();
        }

        private void OnToolbarCreateCabinetButtonClick()
        {
            _view.ShowCreateCabinetPanel = true;
            _view.ShowCreateCabinetBackButton = true;
            _view.CreateCabinetAvatarGameObject = null;

            UpdateView();
        }

        private void OnHierarchyChanged()
        {
            if (Application.isPlaying) return;
            UpdateView();
        }


        private void OnSelectedCabinetChange()
        {
            UpdateView();
        }

        private void OnCabinetSettingsChange()
        {
            var cabinets = OneConfUtils.GetAllCabinets();

            if (cabinets.Length == 0)
            {
                _view.ShowCreateCabinetBackButton = false;
                _view.ShowCreateCabinetPanel = true;
                return;
            }
            _view.ShowCreateCabinetBackButton = true;

            var cabinet = cabinets[_view.SelectedCabinetIndex];

            cabinet.RootGameObject = _view.CabinetAvatarGameObject;

            if (_cabinetConfig == null)
            {
                _cabinetConfig = new CabinetConfig();
            }
            _cabinetConfig.avatarArmatureName = _view.CabinetAvatarArmatureName;
            _cabinetConfig.groupDynamics = _view.CabinetGroupDynamics;
            _cabinetConfig.groupDynamicsSeparateGameObjects = _view.CabinetGroupDynamicsSeparateGameObjects;
            _cabinetConfig.animationWriteDefaultsMode = (CabinetConfig.WriteDefaultsMode)_view.CabinetAnimationWriteDefaultsMode;

            var cabAnimConfig = _cabinetConfig.FindModuleConfig<CabinetAnimCabinetModuleConfig>();
            if (cabAnimConfig == null)
            {
                cabAnimConfig = new CabinetAnimCabinetModuleConfig();
                _cabinetConfig.modules.Add(new CabinetModule()
                {
                    config = cabAnimConfig,
                    moduleName = CabinetAnimCabinetModuleConfig.ModuleIdentifier
                });
            }

            cabAnimConfig.thumbnails = _view.CabinetUseThumbnailsAsMenuIcons;
            cabAnimConfig.menuInstallPath = _view.CabinetMenuInstallPath;
            cabAnimConfig.menuItemName = _view.CabinetMenuItemName;
            cabAnimConfig.networkSynced = _view.CabinetNetworkSynced;
            cabAnimConfig.saved = _view.CabinetSaved;

            cabinet.ConfigJson = CabinetConfigUtility.Serialize(_cabinetConfig);
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        public void SelectCabinet(DTCabinet cabinet)
        {
            var cabinets = OneConfUtils.GetAllCabinets();

            if (cabinets.Length == 0)
            {
                _view.ShowCreateCabinetBackButton = false;
                _view.ShowCreateCabinetPanel = true;
                return;
            }
            _view.ShowCreateCabinetBackButton = true;
            _view.ShowCreateCabinetPanel = false;

            // refresh the keys first
            UpdateCabinetSelectionDropdown(cabinets);

            // find matching index
            for (var i = 0; i < cabinets.Length; i++)
            {
                if (cabinets[i] == cabinet)
                {
                    _view.SelectedCabinetIndex = i;
                    break;
                }
            }

            // update
            UpdateView();
        }

        private void UpdateCabinetSelectionDropdown(DTCabinet[] cabinets)
        {
            // cabinet selection dropdown
            _view.AvailableCabinetSelections.Clear();
            for (var i = 0; i < cabinets.Length; i++)
            {
                _view.AvailableCabinetSelections.Add(cabinets[i].RootGameObject != null ? cabinets[i].RootGameObject.name : t._("cabinet.editor.cabinetContent.popup.cabinetOptions.cabinetNameNoGameObjectAttached", i + 1));
            }
        }

        private void UpdateCabinetContentView()
        {
            var cabinets = OneConfUtils.GetAllCabinets();

            if (cabinets.Length == 0)
            {
                _view.ShowCreateCabinetBackButton = false;
                _view.ShowCreateCabinetPanel = true;
                return;
            }
            _view.ShowCreateCabinetBackButton = true;

            UpdateCabinetSelectionDropdown(cabinets);

            if (_view.SelectedCabinetIndex < 0 || _view.SelectedCabinetIndex >= cabinets.Length)
            {
                // invalid selected cabinet index, setting it back to 0
                _view.SelectedCabinetIndex = 0;
            }

            // clear views
            _view.InstalledWearablePreviews.Clear();

            // update selected cabinet view
            var cabinet = cabinets[_view.SelectedCabinetIndex];

            // cabinet json is broken, ask user whether to make a new one or not
            if (!CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out _cabinetConfig))
            {
                // TODO: ask user
                Debug.LogError("[DressingTools] [CabinetPresenter] Unable to deserialize cabinet config!");
                return;
            }

            _view.CabinetAvatarGameObject = cabinet.RootGameObject;
            _view.CabinetAvatarArmatureName = _cabinetConfig.avatarArmatureName;
            _view.CabinetGroupDynamics = _cabinetConfig.groupDynamics;
            _view.CabinetGroupDynamicsSeparateGameObjects = _cabinetConfig.groupDynamicsSeparateGameObjects;
            _view.CabinetAnimationWriteDefaultsMode = (int)_cabinetConfig.animationWriteDefaultsMode;
            UpdateCabinetAnimationConfig();

            var wearables = OneConfUtils.GetCabinetWearables(cabinet.RootGameObject);

            foreach (var wearable in wearables)
            {
                var config = WearableConfigUtility.Deserialize(wearable.ConfigJson);
                _view.InstalledWearablePreviews.Add(new WearablePreview()
                {
                    name = config != null ?
                        config.info.name :
                        t._("cabinet.editor.cabinetContent.wearablePreview.name.unableToLoadConfiguration"),
                    thumbnail = config != null && config.info.thumbnail != null ?
                        OneConfUtils.GetTextureFromBase64(config.info.thumbnail) :
                        null,
                    RemoveButtonClick = () =>
                    {
                        if (wearable is DTWearable dtWearable)
                        {
                            cabinet.RemoveWearable(dtWearable);
                            UpdateView();
                        }
                        else
                        {
                            Debug.LogWarning("[DressingTools] Removing non-DressingTools wearable is not currently supported");
                        }
                    },
                    EditButtonClick = () =>
                    {
                        _view.StartDressing(cabinet.RootGameObject, wearable.RootGameObject);
                    }
                });
            }
        }

        private void UpdateCabinetAnimationConfig()
        {
            var cabAnimConfig = _cabinetConfig.FindModuleConfig<CabinetAnimCabinetModuleConfig>() ?? new CabinetAnimCabinetModuleConfig();
            _view.CabinetUseThumbnailsAsMenuIcons = cabAnimConfig.thumbnails;
            _view.CabinetMenuInstallPath = cabAnimConfig.menuInstallPath;
            _view.CabinetMenuItemName = cabAnimConfig.menuItemName;
            _view.CabinetNetworkSynced = cabAnimConfig.networkSynced;
            _view.CabinetSaved = cabAnimConfig.saved;
        }

        private void UpdateView()
        {
            UpdateCabinetContentView();
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

        private void OnAddWearableButtonClick()
        {
            var cabinets = OneConfUtils.GetAllCabinets();

            if (cabinets.Length == 0 || _view.SelectedCabinetIndex < 0 || _view.SelectedCabinetIndex >= cabinets.Length)
            {
                return;
            }

            var cabinet = cabinets[_view.SelectedCabinetIndex];
            _view.StartDressing(cabinet.RootGameObject);
        }
    }
}
