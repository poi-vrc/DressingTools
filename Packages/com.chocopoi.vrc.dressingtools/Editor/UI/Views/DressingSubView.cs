/*
 * File: DressingSubView.cs
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class DressingSubView : EditorViewBase, IDressingSubView, IWearableConfigViewParent
    {
        public event Action TargetAvatarOrWearableChange;
        public event Action DoAddToCabinetEvent;
        public event Action DressingModeChange;
        public GameObject TargetAvatar { get => _targetAvatar; set => _targetAvatar = value; }
        public GameObject TargetWearable { get => _targetWearable; set => _targetWearable = value; }
        public WearableConfig Config { get; set; }
        public bool ShowAvatarNoExistingCabinetHelpbox { get; set; }
        public bool DisableAllButtons { get; set; }
        public bool DisableAddToCabinetButton { get; set; }
        public int SelectedDressingMode { get => _currentMode; set => _currentMode = value; }

        private DressingPresenter _presenter;
        private IMainView _mainView;
        private GameObject _targetAvatar;
        private GameObject _targetWearable;
        private WearableSetupWizardView _wizardView;
        private WearableConfigView _configView;
        private int _currentMode;

        public DressingSubView(IMainView mainView)
        {
            _mainView = mainView;
            _presenter = new DressingPresenter(this);

            _targetAvatar = null;
            _targetWearable = null;

            ShowAvatarNoExistingCabinetHelpbox = true;
            DisableAllButtons = true;
            DisableAddToCabinetButton = true;
            Config = new WearableConfig();

            _wizardView = new WearableSetupWizardView(this);
            _configView = new WearableConfigView(this);
        }

        public void WizardGenerateConfig()
        {
            _wizardView.GenerateConfig();
            _configView.RaiseForceUpdateViewEvent();
        }

        public bool IsConfigValid()
        {
            return _currentMode == 0 ? _wizardView.IsValid() : _configView.IsValid();
        }

        public void SelectTab(int selectedTab)
        {
            _mainView.SelectedTab = selectedTab;
        }

        public void ForceUpdateCabinetSubView()
        {
            _mainView.ForceUpdateCabinetSubView();
        }

        public void RaiseDoAddToCabinetEvent()
        {
            DoAddToCabinetEvent?.Invoke();
        }

        public void StartSetupWizard(GameObject targetAvatar, GameObject targetWearable = null)
        {
            ResetWizardAndConfigView();
            TargetAvatar = targetAvatar;
            TargetWearable = targetWearable;
            TargetAvatarOrWearableChange?.Invoke();
        }

        public void ResetWizardAndConfigView()
        {
            // reset parameters
            _currentMode = 0;
            TargetAvatar = null;
            TargetWearable = null;
            Config = new WearableConfig();

            _wizardView.CurrentStep = 0;
            _wizardView.RaiseForceUpdateViewEvent();

            // force update the config view
            _configView.RaiseForceUpdateViewEvent();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _configView.OnEnable();
            _wizardView.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _configView.OnDisable();
            _wizardView.OnDisable();
        }

        public override void OnGUI()
        {
            GameObjectField("Avatar", ref _targetAvatar, true, TargetAvatarOrWearableChange);

            if (ShowAvatarNoExistingCabinetHelpbox)
            {
                HelpBox("The selected avatar has no existing cabinet.", MessageType.Error);
            }

            GameObjectField("Wearable", ref _targetWearable, true, TargetAvatarOrWearableChange);

            HorizontalLine();

            BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                // TODO: ask wizard to write back data to here on mode change
                Toolbar(ref _currentMode, new string[] { "Wizard", "Advanced" }, DressingModeChange);
            }
            EndHorizontal();

            Separator();

            if (_currentMode == 0)
            {
                // Wizard mode
                _wizardView.OnGUI();
            }
            else if (_currentMode == 1)
            {
                // Fully-custom advanced mode
                _configView.OnGUI();

                BeginHorizontal();
                {
                    BeginDisabled(!_configView.IsValid());
                    {
                        BeginDisabled(DisableAddToCabinetButton);
                        {
                            Button("Add to cabinet", DoAddToCabinetEvent);
                        }
                        EndDisabled();

                        Button("Save to file");
                    }
                    EndDisabled();
                }
                EndHorizontal();
            }
        }
    }
}
