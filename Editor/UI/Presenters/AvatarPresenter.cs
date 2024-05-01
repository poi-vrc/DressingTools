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

using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class AvatarPresenter
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private IAvatarSubView _view;

        public AvatarPresenter(IAvatarSubView view)
        {
            _view = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.AddOutfitButtonClick += OnAddOutfitButtonClick;
            _view.ForceUpdateView += OnForceUpdateView;
            _view.SelectedAvatarChange += OnSelectedCabinetChange;
            _view.AvatarSettingsChange += OnAvatarSettingsChange;

            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.AddOutfitButtonClick -= OnAddOutfitButtonClick;
            _view.ForceUpdateView -= OnForceUpdateView;
            _view.SelectedAvatarChange -= OnSelectedCabinetChange;
            _view.AvatarSettingsChange -= OnAvatarSettingsChange;

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

        private void OnHierarchyChanged()
        {
            if (Application.isPlaying) return;
            UpdateView();
        }


        private void OnSelectedCabinetChange()
        {
            UpdateView();
        }

        private void OnAvatarSettingsChange()
        {
            var cabinets = OneConfUtils.GetAllCabinets();

            if (cabinets.Length == 0)
            {
                _view.ShowCreateCabinetBackButton = false;
                _view.ShowCreateCabinetPanel = true;
                return;
            }
            _view.ShowCreateCabinetBackButton = true;

            var cabinet = cabinets[_view.SelectedAvatarIndex];

            cabinet.RootGameObject = _view.CabinetAvatarGameObject;

            if (_cabinetConfig == null)
            {
                Debug.LogWarning("[DressingTools] Cabinet config is uninitialized from UI but cabinet settings changed.");
                return;
            }

            _cabinetConfig.avatarArmatureName = _view.CabinetAvatarArmatureName;
            _cabinetConfig.groupDynamics = _view.CabinetGroupDynamics;
            _cabinetConfig.groupDynamicsSeparateGameObjects = _view.CabinetGroupDynamicsSeparateGameObjects;
            _cabinetConfig.animationWriteDefaultsMode = (CabinetConfig.WriteDefaultsMode)_view.SettingsAnimationWriteDefaultsMode;

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
            cabAnimConfig.resetCustomizablesOnSwitch = _view.CabinetResetCustomizablesOnSwitch;

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
                    _view.SelectedAvatarIndex = i;
                    break;
                }
            }

            // update
            UpdateView();
        }

        private void UpdateCabinetSelectionDropdown(DTCabinet[] cabinets)
        {
            // cabinet selection dropdown
            _view.AvailableAvatarSelections.Clear();
            for (var i = 0; i < cabinets.Length; i++)
            {
                _view.AvailableAvatarSelections.Add(cabinets[i].RootGameObject != null ? cabinets[i].RootGameObject.name : t._("cabinet.editor.cabinetContent.popup.cabinetOptions.cabinetNameNoGameObjectAttached", i + 1));
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

            if (_view.SelectedAvatarIndex < 0 || _view.SelectedAvatarIndex >= cabinets.Length)
            {
                // invalid selected cabinet index, setting it back to 0
                _view.SelectedAvatarIndex = 0;
            }

            // clear views
            _view.InstalledOutfitPreviews.Clear();

            // update selected cabinet view
            var cabinet = cabinets[_view.SelectedAvatarIndex];

            // cabinet json is broken, ask user whether to make a new one or not
            if (!CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out _cabinetConfig) || !_cabinetConfig.IsValid())
            {
                Debug.LogWarning("[DressingTools] [CabinetPresenter] Unable to deserialize cabinet config or invalid configuration! Using new config instead");
                _cabinetConfig = new CabinetConfig();
                cabinet.ConfigJson = CabinetConfigUtility.Serialize(_cabinetConfig);
            }

            _view.CabinetAvatarGameObject = cabinet.RootGameObject;
            _view.CabinetAvatarArmatureName = _cabinetConfig.avatarArmatureName;
            _view.CabinetGroupDynamics = _cabinetConfig.groupDynamics;
            _view.CabinetGroupDynamicsSeparateGameObjects = _cabinetConfig.groupDynamicsSeparateGameObjects;
            _view.SettingsAnimationWriteDefaultsMode = (int)_cabinetConfig.animationWriteDefaultsMode;
            UpdateCabinetAnimationConfig();

            var wearables = OneConfUtils.GetCabinetWearables(cabinet.RootGameObject);

            foreach (var wearable in wearables)
            {
                var config = WearableConfigUtility.Deserialize(wearable.ConfigJson);
                _view.InstalledOutfitPreviews.Add(new OutfitPreview()
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

        private void OnAddOutfitButtonClick()
        {
            _view.StartDressing(_view.SelectedAvatarGameObject);
        }
    }
}
