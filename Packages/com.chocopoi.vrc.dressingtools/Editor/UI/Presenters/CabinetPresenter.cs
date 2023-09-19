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
using System.Collections.Generic;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class CabinetPresenter
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

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
            DTEditorUtils.GetAvatarCabinet(_view.CreateCabinetAvatarGameObject, true);
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
            UpdateView();
        }


        private void OnSelectedCabinetChange()
        {
            UpdateView();
        }

        private void OnCabinetSettingsChange()
        {
            var cabinets = DTEditorUtils.GetAllCabinets();

            if (cabinets.Length == 0)
            {
                _view.ShowCreateCabinetBackButton = false;
                _view.ShowCreateCabinetPanel = true;
                return;
            }
            _view.ShowCreateCabinetBackButton = true;

            var cabinet = cabinets[_view.SelectedCabinetIndex];

            cabinet.AvatarGameObject = _view.CabinetAvatarGameObject;

            if (_cabinetConfig == null)
            {
                _cabinetConfig = new CabinetConfig();
            }
            _cabinetConfig.avatarArmatureName = _view.CabinetAvatarArmatureName;
            _cabinetConfig.groupDynamics = _view.CabinetGroupDynamics;
            _cabinetConfig.groupDynamicsSeparateGameObjects = _view.CabinetGroupDynamicsSeparateGameObjects;
            _cabinetConfig.animationWriteDefaults = _view.CabinetAnimationWriteDefaults;
            cabinet.configJson = _cabinetConfig.Serialize();
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        public void SelectCabinet(DTCabinet cabinet)
        {
            var cabinets = DTEditorUtils.GetAllCabinets();

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
                _view.AvailableCabinetSelections.Add(cabinets[i].AvatarGameObject != null ? cabinets[i].AvatarGameObject.name : t._("cabinet.editor.cabinetContent.popup.cabinetOptions.cabinetNameNoGameObjectAttached", i + 1));
            }
        }

        private void UpdateCabinetContentView()
        {
            var cabinets = DTEditorUtils.GetAllCabinets();

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
            if (!CabinetConfig.TryDeserialize(cabinet.configJson, out _cabinetConfig))
            {
                // TODO: ask user
                Debug.LogError("[DressingTools] [CabinetPresenter] Unable to deserialize cabinet config!");
                return;
            }

            _view.CabinetAvatarGameObject = cabinet.AvatarGameObject;
            _view.CabinetAvatarArmatureName = _cabinetConfig.avatarArmatureName;
            _view.CabinetGroupDynamics = _cabinetConfig.groupDynamics;
            _view.CabinetGroupDynamicsSeparateGameObjects = _cabinetConfig.groupDynamicsSeparateGameObjects;
            _view.CabinetAnimationWriteDefaults = _cabinetConfig.animationWriteDefaults;

            // list cabinet modules
            _view.CabinetModulePreviews.Clear();
            foreach (var cabinetModule in _cabinetConfig.modules)
            {
                var preview = new CabinetModulePreview()
                {
                    name = cabinetModule.moduleName,
                };
                preview.RemoveButtonClick = () =>
                {
                    _cabinetConfig.modules.Remove(cabinetModule);
                    _view.CabinetModulePreviews.Remove(preview);
                    cabinet.configJson = _cabinetConfig.Serialize();
                    _view.Repaint();
                };
                _view.CabinetModulePreviews.Add(preview);
            }

            var wearables = DTEditorUtils.GetCabinetWearables(cabinet.AvatarGameObject);

            foreach (var wearable in wearables)
            {
                var config = WearableConfig.Deserialize(wearable.configJson);
                _view.InstalledWearablePreviews.Add(new WearablePreview()
                {
                    name = config != null ?
                        config.info.name :
                        t._("cabinet.editor.cabinetContent.wearablePreview.name.unableToLoadConfiguration"),
                    thumbnail = config != null && config.info.thumbnail != null ?
                        DTEditorUtils.GetTextureFromBase64(config.info.thumbnail) :
                        null,
                    RemoveButtonClick = () =>
                    {
                        DTEditorUtils.RemoveCabinetWearable(cabinet, wearable);
                        UpdateView();
                    },
                    EditButtonClick = () =>
                    {
                        _view.StartDressing(cabinet.AvatarGameObject, wearable.WearableGameObject);
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

        private void OnAddWearableButtonClick()
        {
            var cabinets = DTEditorUtils.GetAllCabinets();

            if (cabinets.Length == 0 || _view.SelectedCabinetIndex < 0 || _view.SelectedCabinetIndex >= cabinets.Length)
            {
                return;
            }

            var cabinet = cabinets[_view.SelectedCabinetIndex];
            _view.StartDressing(cabinet.AvatarGameObject);
        }
    }
}
