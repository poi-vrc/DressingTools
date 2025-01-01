/*
 * Copyright (c) 2024 chocopoi
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
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Elements;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
#if DT_VRCSDK3A
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace Chocopoi.DressingTools.Inspector.Views
{
    [ExcludeFromCodeCoverage]
    internal class MenuInstallView : ElementView, IMenuInstallView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public DTMenuInstall Target { get; set; }
#if DT_VRCSDK3A
        public VRCExpressionsMenu VRCSourceMenu { get => (VRCExpressionsMenu)_vrcSourceMenuObjField.value; set => _vrcSourceMenuObjField.value = value; }
#endif
        public string InstallPath { get => _installPathField.value; set => _installPathField.value = value; }
        public bool HasMenuGroupComponent { get; set; }

        public event Action SettingsChanged;
        private readonly MenuInstallPresenter _presenter;
        private VisualElement _menuGroupControlledHelpbox;
        private ObjectField _vrcSourceMenuObjField;
        private TextField _installPathField;

        public MenuInstallView()
        {
            _presenter = new MenuInstallPresenter(this);
            InitVisualTree();
            t.LocalizeElement(this);
        }

        private void InitVisualTree()
        {
            _menuGroupControlledHelpbox = CreateHelpBox(t._("inspector.menu.install.helpbox.menuGroupControlled"), UnityEditor.MessageType.Info);
            Add(_menuGroupControlledHelpbox);

#if DT_VRCSDK3A
            _vrcSourceMenuObjField = new ObjectField(t._("inspector.menu.install.objectField.sourceVrcMenu"))
            {
                objectType = typeof(VRCExpressionsMenu)
            };
            _vrcSourceMenuObjField.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_vrcSourceMenuObjField);
#endif
            Add(CreateHelpBox(t._("inspector.menu.install.helpbox.installPathDescription"), UnityEditor.MessageType.Info));
            _installPathField = new TextField(t._("inspector.menu.install.textField.installPath"));
            _installPathField.RegisterValueChangedCallback(evt => SettingsChanged?.Invoke());
            Add(_installPathField);
        }

        public override void Repaint()
        {
            _menuGroupControlledHelpbox.style.display = HasMenuGroupComponent ? DisplayStyle.Flex : DisplayStyle.None;
#if DT_VRCSDK3A
            _vrcSourceMenuObjField.SetEnabled(!HasMenuGroupComponent);
#endif 
        }
    }
}
