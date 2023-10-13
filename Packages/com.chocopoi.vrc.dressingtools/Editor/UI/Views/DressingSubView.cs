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
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.UI;
using Chocopoi.DressingFramework.Wearable;
using Chocopoi.DressingTools.Localization;
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
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action TargetAvatarOrWearableChange;
        public event Action AddToCabinetButtonClick;
        public GameObject TargetAvatar { get; set; }
        public GameObject TargetWearable { get; set; }
        public WearableConfig Config { get; set; }
        public bool DisableAllButtons { get; set; }
        public bool DisableAddToCabinetButton { get; set; }
        public int SelectedDressingMode { get => _currentMode; set => _currentMode = value; }

        private DressingPresenter _presenter;
        private IMainView _mainView;
        private WearableConfigView _configView;
        private int _currentMode;
        private Button[] _viewModeBtns;
        private ObjectField _avatarObjectField;
        private VisualElement _advancedContainer;
        private ObjectField _wearableObjectField;
        private Button _btnAddToCabinet;

        public DressingSubView(IMainView mainView)
        {
            _mainView = mainView;
            _presenter = new DressingPresenter(this);

            TargetAvatar = null;
            TargetWearable = null;

            DisableAllButtons = true;
            DisableAddToCabinetButton = true;
            Config = new WearableConfig();

            _configView = new WearableConfigView(this);
        }

        public void ForceUpdateConfigView()
        {
            _configView.RaiseForceUpdateViewEvent();
        }

        public void AutoSetup()
        {
            _configView.AutoSetup();
        }

        public bool IsConfigValid()
        {
            return _configView.IsValid();
        }

        public void SelectTab(int selectedTab)
        {
            _mainView.SelectedTab = selectedTab;
        }

        public void ForceUpdateCabinetSubView()
        {
            _mainView.ForceUpdateCabinetSubView();
        }

        public void StartDressing(GameObject targetAvatar, GameObject targetWearable = null)
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

            // force update the config view
            _configView.RaiseForceUpdateViewEvent();

            Repaint();
        }

        // asks config view to apply the UI changes to the config instance
        public void ApplyToConfig() => _configView.ApplyToConfig();

        public override void OnEnable()
        {
            InitVisualTree();
            t.LocalizeElement(this);
            _configView.OnEnable();
            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _configView.OnDisable();
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

            var configViewContainer = Q<VisualElement>("config-view-container").First();
            configViewContainer.Add(_configView);

            _btnAddToCabinet = Q<Button>("btn-add-to-cabinet").First();
            _btnAddToCabinet.clicked += AddToCabinetButtonClick;

            BindFoldoutHeaderWithContainer("foldout-setup", "setup-container");
        }

        public override void Repaint()
        {
            _avatarObjectField.value = TargetAvatar;
            _wearableObjectField.value = TargetWearable;
        }

        public void ShowFixAllInvalidConfig()
        {
            EditorUtility.DisplayDialog(t._("tool.name"), t._("dressing.editor.dialog.msg.fixInvalidConfig"), t._("common.dialog.btn.ok"));
        }
    }
}
