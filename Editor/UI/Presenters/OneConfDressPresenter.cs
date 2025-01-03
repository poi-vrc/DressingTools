﻿/*
 * File: DressingPresenter.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 1:22:09 am
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
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class OneConfDressPresenter
    {
        private readonly IOneConfDressSubView _view;

        public OneConfDressPresenter(IOneConfDressSubView view)
        {
            _view = view;
            _view.Load += OnLoad;
        }

        private void SubscribeEvents()
        {
            _view.Unload += OnUnload;
            _view.ForceUpdateView += OnForceUpdateView;
            _view.TargetAvatarChange += OnTargetAvatarChange;
            _view.TargetWearableChange += OnTargetAvatarOrWearableChange;
            _view.AddToCabinetButtonClick += OnAddToCabinetButtonClick;

            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void UnsubscribeEvents()
        {
            _view.Unload -= OnUnload;
            _view.ForceUpdateView -= OnForceUpdateView;
            _view.TargetAvatarChange -= OnTargetAvatarChange;
            _view.TargetWearableChange -= OnTargetAvatarOrWearableChange;
            _view.AddToCabinetButtonClick -= OnAddToCabinetButtonClick;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        private void OnHierarchyChanged()
        {
            UpdateView();
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            var cabinet = OneConfUtils.GetAvatarCabinet(_view.TargetAvatar);
            _view.DisableAddToCabinetButton = cabinet == null;
            _view.Repaint();
        }

        private void OnLoad()
        {
            SubscribeEvents();
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }

        private void CreateNewConfig()
        {
            if (_view.TargetAvatar == null)
            {
                return;
            }

            _view.Config = new WearableConfig();
            OneConfUtils.PrepareWearableConfig(_view.Config, _view.TargetAvatar, _view.TargetWearable);
            _view.ForceUpdateConfigView();
            _view.AutoSetup();
        }

        private void OnTargetAvatarChange()
        {
            _view.ResetWizardAndConfigView();
            OnTargetAvatarOrWearableChange();
        }

        private void OnTargetAvatarOrWearableChange()
        {
            // try find if the wearable has a config, if yes, use advanced mode for editing
            if (_view.TargetWearable != null)
            {
                var cabinetWearable = OneConfUtils.GetCabinetWearable(_view.TargetWearable);
                if (cabinetWearable != null)
                {
                    if (WearableConfigUtility.TryDeserialize(cabinetWearable.ConfigJson, out var config))
                    {
                        _view.Config = config;
                        _view.SelectedDressingMode = 1;
                        _view.ForceUpdateConfigView();
                    }
                    else
                    {
                        CreateNewConfig();
                    }
                }
                else
                {
                    CreateNewConfig();
                }
            }

            UpdateView();
        }

        private void OnAddToCabinetButtonClick()
        {
            if (!_view.IsConfigValid())
            {
                _view.ShowFixAllInvalidConfig();
                Debug.Log("[DressingTools] Invalid configuration. Cannot proceed adding to cabinet");
                return;
            }

            var cabinet = OneConfUtils.GetAvatarCabinet(_view.TargetAvatar, true);

            if (cabinet == null)
            {
                return;
            }

            if (!CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out var cabinetConfig))
            {
                // TODO: handle deserialization error
                Debug.Log("[DressingTools] Could not deserialize cabinet config");
                return;
            }

            // remove previews
            DTEditorUtils.CleanUpPreviewAvatars();
            DTEditorUtils.FocusGameObjectInSceneView(_view.TargetAvatar);

            _view.ApplyToConfig();

            if (cabinet.AddWearable(_view.Config, _view.TargetWearable))
            {
                // reset and return
                _view.ResetWizardAndConfigView();
                _view.SelectTab(0);

                _view.ForceUpdateCabinetSubView();
            }
            else
            {
                _view.ShowFixAllInvalidConfig();
            }
        }
    }
}
