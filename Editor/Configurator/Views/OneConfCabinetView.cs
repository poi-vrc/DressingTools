/*
 * File: CabinetSubView.cs
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Configurator.Presenters;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Configurator.Views
{
    [ExcludeFromCodeCoverage]
    internal class OneConfCabinetView : ElementView, IOneConfCabinetView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action SettingsChanged;

        public string ArmatureName { get => _armatureNameField.value; set => _armatureNameField.value = value; }
        public bool GroupDynamics { get => _groupDynToggle.value; set => _groupDynToggle.value = value; }
        public bool GroupDynamicsSeparate { get => _groupDynSepToggle.value; set => _groupDynSepToggle.value = value; }
        public bool UseThumbnails { get => _useThumbnailsToggle.value; set => _useThumbnailsToggle.value = value; }
        public bool ResetCustomizablesOnSwitch { get => _resetCustomizablesOnSwitchToggle.value; set => _resetCustomizablesOnSwitchToggle.value = value; }
        public string MenuInstallPathField { get => _installPathField.value; set => _installPathField.value = value; }
        public string MenuItemNameField { get => _itemNameField.value; set => _itemNameField.value = value; }
        public bool NetworkSyncedToggle { get => _networkSyncedToggle.value; set => _networkSyncedToggle.value = value; }
        public bool SavedToggle { get => _savedToggle.value; set => _savedToggle.value = value; }

        private readonly OneConfCabinetPresenter _presenter;
        private TextField _armatureNameField;
        private Toggle _groupDynToggle;
        private Toggle _groupDynSepToggle;
        private Toggle _useThumbnailsToggle;
        private Toggle _resetCustomizablesOnSwitchToggle;
        private TextField _installPathField;
        private TextField _itemNameField;
        private Toggle _networkSyncedToggle;
        private Toggle _savedToggle;

        public OneConfCabinetView(GameObject avatarGameObject)
        {
            _presenter = new OneConfCabinetPresenter(this, avatarGameObject);
            InitVisualTree();
            t.LocalizeElement(this);
        }

        private void InitVisualTree()
        {
            _armatureNameField = new TextField(t._("editor.main.avatar.settings.oneConf.textField.avatarArmatureName"));
            _armatureNameField.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_armatureNameField);
            _groupDynToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.groupDynamics"));
            _groupDynToggle.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_groupDynToggle);
            _groupDynSepToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.groupDynamicsSeparate"));
            _groupDynSepToggle.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_groupDynSepToggle);

            _useThumbnailsToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.useThumbnailsAsMenuIcons"));
            _useThumbnailsToggle.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_useThumbnailsToggle);
            _resetCustomizablesOnSwitchToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.resetCustomizablesOnSwitch"));
            _resetCustomizablesOnSwitchToggle.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_resetCustomizablesOnSwitchToggle);

            Add(new IMGUIContainer(() => EditorGUILayout.HelpBox(t._("editor.main.avatar.settings.oneConf.helpbox.installPathDescription"), MessageType.Info)));

            _installPathField = new TextField(t._("editor.main.avatar.settings.oneConf.textField.menuInstallPath"));
            _installPathField.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_installPathField);
            _itemNameField = new TextField(t._("editor.main.avatar.settings.oneConf.textField.menuItemName"));
            _itemNameField.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_itemNameField);

            _networkSyncedToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.networkSynced"));
            _networkSyncedToggle.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_networkSyncedToggle);
            _savedToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.saved"));
            _savedToggle.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_savedToggle);
        }

        public override void Repaint()
        {
            _armatureNameField.value = ArmatureName;
            _groupDynToggle.value = GroupDynamics;
            _groupDynSepToggle.value = GroupDynamicsSeparate;
            _useThumbnailsToggle.value = UseThumbnails;
            _resetCustomizablesOnSwitchToggle.value = ResetCustomizablesOnSwitch;
            _installPathField.value = MenuInstallPathField;
            _itemNameField.value = MenuItemNameField;
            _networkSyncedToggle.value = NetworkSyncedToggle;
            _savedToggle.value = SavedToggle;
        }
    }
}
