/*
 * File: VRChatIntegrationModuleEditor.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 23th 2023, 7:56:36 pm
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
using Chocopoi.DressingTools.Integration.VRChat.Modules;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using UnityEditor;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    [ExcludeFromCodeCoverage]
    [CustomModuleEditor(typeof(VRChatIntegrationModuleProvider))]
    internal class VRChatIntegrationModuleEditor : ModuleEditor, IVRChatIntegrationModuleEditorView
    {
        public event Action ConfigChange;

        public bool UseCustomCabinetToggleName { get => _useCustomCabinetToggleName; set => _useCustomCabinetToggleName = value; }
        public string CustomCabinetToggleName { get => _customCabinetToggleName; set => _customCabinetToggleName = value; }

        private IModuleEditorViewParent _parentView;
        private VRChatIntegrationModuleEditorPresenter _presenter;

        private bool _useCustomCabinetToggleName;
        private string _customCabinetToggleName;

        public VRChatIntegrationModuleEditor(IModuleEditorViewParent parentView, VRChatIntegrationModuleProvider provider, IModuleConfig target) : base(parentView, provider, target)
        {
            _parentView = parentView;
            _presenter = new VRChatIntegrationModuleEditorPresenter(this, parentView, (VRChatIntegrationModuleConfig)target);

            _useCustomCabinetToggleName = false;
        }

        public override void OnGUI()
        {
            ToggleLeft("Use custom cabinet toggle name", ref _useCustomCabinetToggleName, ConfigChange);
            BeginDisabled(!_useCustomCabinetToggleName);
            {
                EditorGUI.indentLevel += 1;
                TextField("Toggle Name", ref _customCabinetToggleName, ConfigChange);
                EditorGUI.indentLevel -= 1;
            }
            EndDisabled();
        }

        public override bool IsValid() => _presenter.IsValid();
    }
}
