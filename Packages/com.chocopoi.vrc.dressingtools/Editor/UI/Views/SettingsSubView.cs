/*
 * File: SettingsSubView.cs
 * Project: DressingTools
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.UI;
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
    internal class SettingsSubView : ElementViewBase, ISettingsSubView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private const string PopupPlaceholder = "---";

        public event Action LanguageChanged;
        public event Action SettingsChanged;
        public event Action UpdaterCheckUpdateButtonClicked;
        public event Action ResetToDefaultsButtonClicked;

        public List<string> AvailableLanguageKeys { get; set; }
        public List<string> AvailableBranchKeys { get; set; }

        public string LanguageSelected { get; set; }

        public string CabinetDefaultsArmatureName { get; set; }
        public bool CabinetDefaultsGroupDynamics { get; set; }
        public bool CabinetDefaultsSeparateDynamics { get; set; }
        public bool CabinetDefaultsAnimWriteDefaults { get; set; }

        public string UpdaterCurrentVersion { get; set; }
        public bool UpdaterShowHelpboxUpdateNotChecked { get; set; }
        public string UpdaterDefaultBranch { get; set; }
        public string UpdaterSelectedBranch { get; set; }

        private IMainView _mainView;
        private SettingsPresenter _presenter;
        private PopupField<string> _languagePopup;
        private TextField _cabinetDefArmatureNameField;
        private Toggle _cabinetGroupDynamicsToggle;
        private Toggle _cabinetSeparateDynamicsToggle;
        private Toggle _cabinetAnimWriteDefToggle;
        private Label _updaterCurrentVerLabel;
        private VisualElement _updaterHelpboxContainer;
        private VisualElement _updaterBranchSelectionContainer;
        private Label _updaterDefBranchLabel;
        private PopupField<string> _updaterBranchPopup;

        public SettingsSubView(IMainView mainView)
        {
            _mainView = mainView;
            AvailableLanguageKeys = new List<string>() { PopupPlaceholder };
            AvailableBranchKeys = new List<string>() { PopupPlaceholder };

            LanguageSelected = null;

            CabinetDefaultsArmatureName = "Armature";
            CabinetDefaultsGroupDynamics = true;
            CabinetDefaultsSeparateDynamics = true;
            CabinetDefaultsAnimWriteDefaults = true;

            UpdaterCurrentVersion = "";
            UpdaterShowHelpboxUpdateNotChecked = true;
            UpdaterDefaultBranch = "";
            UpdaterSelectedBranch = null;

            _presenter = new SettingsPresenter(this);
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("SettingsSubView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("SettingsSubViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }
        }

        private void InitLanguagePopup()
        {
            var languagePopupContainer = Q<VisualElement>("language-popup-container").First();
            _languagePopup = new PopupField<string>(t._("settings.editor.i18n.popup.language"), AvailableLanguageKeys, 0);
            _languagePopup.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                if (evt.previousValue == null || evt.previousValue == PopupPlaceholder) return;
                LanguageSelected = evt.newValue;
                LanguageChanged?.Invoke();
            });
            languagePopupContainer.Add(_languagePopup);
        }

        private void InitCabinetDefaults()
        {
            _cabinetDefArmatureNameField = Q<TextField>("cabinet-def-armature-name-field").First();
            _cabinetDefArmatureNameField.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                var val = _cabinetDefArmatureNameField.value;
                if (string.IsNullOrEmpty(val)) return;
                CabinetDefaultsArmatureName = val;
                SettingsChanged?.Invoke();
            });
            _cabinetGroupDynamicsToggle = Q<Toggle>("cabinet-def-group-dynamics-toggle").First();
            _cabinetGroupDynamicsToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                CabinetDefaultsGroupDynamics = evt.newValue;
                SettingsChanged?.Invoke();
            });
            _cabinetSeparateDynamicsToggle = Q<Toggle>("cabinet-def-separate-dynamics-toggle").First();
            _cabinetSeparateDynamicsToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                CabinetDefaultsSeparateDynamics = evt.newValue;
                SettingsChanged?.Invoke();
            });
            _cabinetAnimWriteDefToggle = Q<Toggle>("cabinet-def-anim-write-defs-toggle").First();
            _cabinetAnimWriteDefToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                CabinetDefaultsAnimWriteDefaults = evt.newValue;
                SettingsChanged?.Invoke();
            });
        }

        private void InitUpdateChecker()
        {
            _updaterCurrentVerLabel = Q<Label>("updater-current-ver-label").First();
            _updaterHelpboxContainer = Q<VisualElement>("updater-helpbox-container").First();
            _updaterBranchSelectionContainer = Q<VisualElement>("updater-branch-selection-container").First();
            _updaterDefBranchLabel = Q<Label>("updater-def-branch-label").First();

            var updaterBranchPopupContainer = Q<VisualElement>("updater-branch-popup-container").First();
            _updaterBranchPopup = new PopupField<string>(t._("settings.editor.updateChecker.popup.branch"), AvailableBranchKeys, 0);
            _updaterBranchPopup.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                UpdaterSelectedBranch = evt.newValue;
                SettingsChanged?.Invoke();
            });
            updaterBranchPopupContainer.Add(_updaterBranchPopup);

            var updaterCheckUpdateBtn = Q<Button>("updater-check-update-btn").First();
            updaterCheckUpdateBtn.clicked += UpdaterCheckUpdateButtonClicked;

            var resetToDefaultsBtn = Q<Button>("reset-defaults-btn").First();
            resetToDefaultsBtn.clicked += ResetToDefaultsButtonClicked;
        }

        private void RepaintCabinetDefaults()
        {
            _cabinetDefArmatureNameField.value = CabinetDefaultsArmatureName;
            _cabinetGroupDynamicsToggle.value = CabinetDefaultsGroupDynamics;
            _cabinetSeparateDynamicsToggle.value = CabinetDefaultsSeparateDynamics;
            _cabinetAnimWriteDefToggle.value = CabinetDefaultsAnimWriteDefaults;
        }

        private void RepaintUpdateChecker()
        {
            _updaterCurrentVerLabel.text = UpdaterCurrentVersion;

            _updaterHelpboxContainer.Clear();
            if (UpdaterShowHelpboxUpdateNotChecked)
            {
                _updaterBranchSelectionContainer.style.display = DisplayStyle.None;
                _updaterHelpboxContainer.Add(CreateHelpBox(t._("settings.editor.updaterChecker.helpbox.msg.updateNotChecked"), UnityEditor.MessageType.Warning));
            }
            else
            {
                _updaterBranchSelectionContainer.style.display = DisplayStyle.Flex;
                _updaterBranchPopup.value = UpdaterSelectedBranch;
            }

            _updaterDefBranchLabel.text = UpdaterDefaultBranch;
        }

        public override void Repaint()
        {
            _languagePopup.value = LanguageSelected;

            RepaintCabinetDefaults();
            RepaintUpdateChecker();
        }

        public override void OnEnable()
        {
            InitVisualTree();
            InitLanguagePopup();
            InitCabinetDefaults();
            InitUpdateChecker();

            t.LocalizeElement(this);

            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        public void AskReloadWindow()
        {
            if (EditorUtility.DisplayDialog(t._("tool.name"), t._("settings.editor.dialog.msg.confirmUpdateLocaleReloadWindow"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no")))
            {
                var window = EditorWindow.GetWindow(typeof(DTMainEditorWindow));
                window.Close();

                DTMainEditorWindow.ShowWindow();
            }
        }
    }
}
