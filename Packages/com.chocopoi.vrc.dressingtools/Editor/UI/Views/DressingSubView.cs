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
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class DressingSubView : ElementViewBase, IDressingSubView, IWearableConfigViewParent
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

        public event Action TargetAvatarOrWearableChange;
        public event Action DoAddToCabinetEvent;
        public event Action DressingModeChange;
        public GameObject TargetAvatar { get; set; }
        public GameObject TargetWearable { get; set; }
        public WearableConfig Config { get; set; }
        public bool ShowAvatarNoExistingCabinetHelpbox { get; set; }
        public bool DisableAllButtons { get; set; }
        public bool DisableAddToCabinetButton { get; set; }
        public int SelectedDressingMode { get => _currentMode; set => _currentMode = value; }

        private DressingPresenter _presenter;
        private IMainView _mainView;
        private WearableSetupWizardView _wizardView;
        private WearableConfigView _configView;
        private int _currentMode;
        private Button[] _viewModeBtns;
        private ObjectField _avatarObjectField;
        private VisualElement _advancedContainer;
        private ObjectField _wearableObjectField;

        public DressingSubView(IMainView mainView)
        {
            _mainView = mainView;
            _presenter = new DressingPresenter(this);

            TargetAvatar = null;
            TargetWearable = null;

            ShowAvatarNoExistingCabinetHelpbox = true;
            DisableAllButtons = true;
            DisableAddToCabinetButton = true;
            Config = new WearableConfig();

            _wizardView = new WearableSetupWizardView(this);
            _configView = new WearableConfigView(this);
        }

        public bool ShowConfirmSwitchWizardModeDialog()
        {
            return EditorUtility.DisplayDialog(t._("tool.name"), t._("dressing.editor.dialog.msg.switchWizardModeConfirm"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no"));
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
            Repaint();
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

            Repaint();
        }

        public override void OnEnable()
        {
            InitVisualTree();
            BindViewModes();

            base.OnEnable();

            t.LocalizeElement(this);

            _configView.OnEnable();
            _wizardView.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _configView.OnDisable();
            _wizardView.OnDisable();
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("DressingSubView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("DressingSubViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }

            _avatarObjectField = Q<ObjectField>("objectfield-avatar").First();
            _avatarObjectField.objectType = typeof(GameObject);
            _avatarObjectField.value = TargetAvatar;
            _avatarObjectField.RegisterValueChangedCallback((ChangeEvent<UnityEngine.Object> evt) =>
            {
                TargetAvatar = (GameObject)evt.newValue;
                TargetAvatarOrWearableChange?.Invoke();
            });

            _wearableObjectField = Q<ObjectField>("objectfield-wearable").First();
            _wearableObjectField.objectType = typeof(GameObject);
            _wearableObjectField.value = TargetWearable;
            _wearableObjectField.RegisterValueChangedCallback((ChangeEvent<UnityEngine.Object> evt) =>
            {
                TargetWearable = (GameObject)evt.newValue;
                TargetAvatarOrWearableChange?.Invoke();
            });

            _advancedContainer = Q<VisualElement>("advanced-container").First();

            var wizardContainer = Q<VisualElement>("wizard-container").First();
            wizardContainer.Add(_wizardView);

            var configViewContainer = Q<VisualElement>("config-view-container").First();
            configViewContainer.Add(_configView);

            _wizardView.style.display = DisplayStyle.Flex;
            _advancedContainer.style.display = DisplayStyle.None;

            BindFoldoutHeaderWithContainer("foldout-setup", "setup-container");
        }

        private void BindViewModes()
        {
            var wizardBtn = Q<Button>("toolbar-btn-wizard");
            var advancedBtn = Q<Button>("toolbar-btn-advanced");

            _viewModeBtns = new Button[] { wizardBtn, advancedBtn };

            for (var i = 0; i < _viewModeBtns.Length; i++)
            {
                var viewModeIndex = i;
                _viewModeBtns[i].clicked += () =>
                {
                    if (_currentMode == viewModeIndex) return;
                    _currentMode = viewModeIndex;
                    DressingModeChange?.Invoke();
                    UpdateViewModes();
                };
                _viewModeBtns[i].EnableInClassList("active", viewModeIndex == _currentMode);
            }
        }

        private void UpdateViewModes()
        {
            for (var i = 0; i < _viewModeBtns.Length; i++)
            {
                _viewModeBtns[i].EnableInClassList("active", i == _currentMode);
            }

            if (_currentMode == 0)
            {
                _wizardView.style.display = DisplayStyle.Flex;
                _advancedContainer.style.display = DisplayStyle.None;
            }
            else if (_currentMode == 1)
            {
                _wizardView.style.display = DisplayStyle.None;
                _advancedContainer.style.display = DisplayStyle.Flex;
            }
        }

        public void Repaint()
        {
            _avatarObjectField.value = TargetAvatar;
            _wearableObjectField.value = TargetWearable;
            UpdateViewModes();
        }
    }
}
