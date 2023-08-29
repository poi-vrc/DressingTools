/*
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

using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class DressingPresenter
    {
        private IDressingSubView _view;

        public DressingPresenter(IDressingSubView view)
        {
            _view = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ForceUpdateView += OnForceUpdateView;
            _view.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;
            _view.DoAddToCabinetEvent += OnAddToCabinetButtonClick;
            _view.DressingModeChange += OnDressingModeChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
            _view.DoAddToCabinetEvent -= OnAddToCabinetButtonClick;
            _view.DressingModeChange -= OnDressingModeChange;
        }

        private void OnDressingModeChange()
        {
            // ask if really switch back to wizard mode
            if (_view.SelectedDressingMode == 0 && !EditorUtility.DisplayDialog("DressingTools", "Switching back to wizard mode will do auto-setup and wipe your existing configuration here.\nAre you sure?", "Yes", "No"))
            {
                _view.SelectedDressingMode = 1;
                return;
            }

            // generate config
            _view.WizardGenerateConfig();
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar);
            var cabinetIsNull = cabinet == null;
            _view.ShowAvatarNoExistingCabinetHelpbox = cabinetIsNull;
            _view.DisableAddToCabinetButton = cabinetIsNull;
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }

        private void OnTargetAvatarOrWearableChange()
        {
            UpdateView();
        }

        private void OnAddToCabinetButtonClick()
        {
            Debug.Log("Add");
            if (!_view.IsConfigValid())
            {
                Debug.Log("[DressingTools] Invalid configuration. Cannot proceed adding to cabinet");
                return;
            }

            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar);

            if (cabinet == null)
            {
                return;
            }

            if (!CabinetConfig.TryDeserialize(cabinet.configJson, out var cabinetConfig))
            {
                // TODO: handle deserialization error
                Debug.Log("[DressingTools] Could not deserialize cabinet config");
                return;
            }

            DTEditorUtils.AddCabinetWearable(cabinetConfig, _view.TargetAvatar, _view.Config, _view.TargetWearable);

            // remove previews
            DTEditorUtils.CleanUpPreviewAvatars();
            DTEditorUtils.FocusGameObjectInSceneView(_view.TargetAvatar);

            // reset and return
            _view.ResetWizardAndConfigView();
            _view.SelectTab(0);

            _view.ForceUpdateCabinetSubView();
        }
    }
}
