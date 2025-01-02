/*
 * File: OneConfDressSubView.cs
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
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf.Wearable;
using Chocopoi.DressingTools.UI.Presenters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class OneConfDressSubView : ElementView, IOneConfDressSubView, IOneConfWearableConfigViewParent
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action TargetAvatarChange { add => _mainView.AvatarSelectionChange += value; remove => _mainView.AvatarSelectionChange -= value; }
        public event Action TargetWearableChange;
        public event Action AddToCabinetButtonClick;
        public GameObject TargetAvatar { get => _mainView.SelectedAvatarGameObject; }
        public GameObject TargetWearable { get; set; }
        public WearableConfig Config { get; set; }
        public bool DisableAllButtons { get; set; }
        public bool DisableAddToCabinetButton { get; set; }
        public int SelectedDressingMode { get => _currentMode; set => _currentMode = value; }

        private readonly OneConfDressPresenter _presenter;
        private readonly IMainView _mainView;
        private readonly OneConfWearableConfigView _configView;
        private int _currentMode;
        private ObjectField _wearableObjectField;
        private Button _btnAddToCabinet;
        private VisualElement _helpboxContainer;
        private VisualElement _configViewContainer;


        public OneConfDressSubView(IMainView mainView)
        {
            _mainView = mainView;
            _presenter = new OneConfDressPresenter(this);

            TargetWearable = null;

            DisableAllButtons = true;
            DisableAddToCabinetButton = true;
            Config = new WearableConfig();

            _configView = new OneConfWearableConfigView(this);

            InitVisualTree();
            t.LocalizeElement(this);
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

        public void StartDressing(GameObject targetWearable = null)
        {
            ResetWizardAndConfigView();
            TargetWearable = targetWearable;
            TargetWearableChange?.Invoke();
            Repaint();
        }

        public void ResetWizardAndConfigView()
        {
            // reset parameters
            _currentMode = 0;
            TargetWearable = null;
            Config = new WearableConfig();

            // force update the config view
            _configView.RaiseForceUpdateViewEvent();

            Repaint();
        }

        // asks config view to apply the UI changes to the config instance
        public void ApplyToConfig() => _configView.ApplyToConfig();

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("OneConfDressSubView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("OneConfDressSubViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }

            _wearableObjectField = Q<ObjectField>("objectfield-wearable");
            _wearableObjectField.objectType = typeof(GameObject);
            _wearableObjectField.value = TargetWearable;
            _wearableObjectField.RegisterValueChangedCallback((ChangeEvent<UnityEngine.Object> evt) =>
            {
                TargetWearable = (GameObject)evt.newValue;
                TargetWearableChange?.Invoke();
            });

            _helpboxContainer = Q<VisualElement>("helpbox-container");

            _configViewContainer = Q<VisualElement>("config-view-container");
            _configViewContainer.Add(_configView);

            _btnAddToCabinet = Q<Button>("btn-add-to-cabinet");
            _btnAddToCabinet.clicked += () => AddToCabinetButtonClick?.Invoke();

            BindFoldoutHeaderWithContainer("foldout-setup", "setup-container");
        }

        public override void Repaint()
        {
            _wearableObjectField.value = TargetWearable;
            var isTargetAvatarWearableNull = TargetAvatar == null || TargetWearable == null;

            _helpboxContainer.Clear();
            if (isTargetAvatarWearableNull)
            {
                _configViewContainer.style.display = DisplayStyle.None;
                _btnAddToCabinet.style.display = DisplayStyle.None;
                _helpboxContainer.Add(CreateHelpBox(t._("editor.main.dress.helpbox.selectAvatarWearable"), MessageType.Error));
            }
            else if (!DKEditorUtils.IsGrandParent(TargetAvatar.transform, TargetWearable.transform))
            {
                _configViewContainer.style.display = DisplayStyle.None;
                _btnAddToCabinet.style.display = DisplayStyle.None;
                _helpboxContainer.Add(CreateHelpBox(t._("editor.main.dress.helpbox.wearableNotInsideOfAvatar"), MessageType.Error));
            }
            else
            {
                _configViewContainer.style.display = DisplayStyle.Flex;
                _btnAddToCabinet.style.display = DisplayStyle.Flex;
            }
        }

        public void ShowFixAllInvalidConfig()
        {
            EditorUtility.DisplayDialog(t._("tool.name"), t._("dressing.editor.dialog.msg.fixInvalidConfig"), t._("common.dialog.btn.ok"));
        }
    }
}
