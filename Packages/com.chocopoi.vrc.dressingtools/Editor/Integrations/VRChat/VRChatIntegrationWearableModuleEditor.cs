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

#if VRC_SDK_VRCSDK3
using System;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Integration.VRChat.Modules;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using UnityEditor;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    [ExcludeFromCodeCoverage]
    [CustomWearableModuleEditor(typeof(VRChatIntegrationWearableModuleProvider))]
    internal class VRChatIntegrationWearableModuleEditor : WearableModuleEditor, IVRChatIntegrationWearableModuleEditorView
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

        public event Action ConfigChange;

        public bool UseCustomCabinetToggleName { get => _useCustomCabinetToggleName; set => _useCustomCabinetToggleName = value; }
        public bool UseCabinetThumbnails { get => _useCabinetThumbnails; set => _useCabinetThumbnails = value; }
        public string CustomCabinetToggleName { get => _customCabinetToggleName; set => _customCabinetToggleName = value; }

        private IWearableModuleEditorViewParent _parentView;
        private VRChatIntegrationWearableModuleEditorPresenter _presenter;

        private bool _useCustomCabinetToggleName;
        private bool _useCabinetThumbnails;
        private string _customCabinetToggleName;

        public VRChatIntegrationWearableModuleEditor(IWearableModuleEditorViewParent parentView, VRChatIntegrationWearableModuleProvider provider, IModuleConfig target) : base(parentView, provider, target)
        {
            _parentView = parentView;
            _presenter = new VRChatIntegrationWearableModuleEditorPresenter(this, parentView, (VRChatIntegrationWearableModuleConfig)target);

            _useCustomCabinetToggleName = false;
        }

        public override void OnGUI()
        {
            ToggleLeft(t._("integrations.vrc.modules.integration.editor.toggle.useCustomCabinetToggleName"), ref _useCustomCabinetToggleName, ConfigChange);
            BeginDisabled(!_useCustomCabinetToggleName);
            {
                EditorGUI.indentLevel += 1;
                TextField(t._("integrations.vrc.modules.integration.editor.textField.customCabinetToggleName"), ref _customCabinetToggleName, ConfigChange);
                EditorGUI.indentLevel -= 1;
            }
            EndDisabled();
            ToggleLeft(t._("integrations.vrc.modules.integration.editor.toggle.useCabinetThumbnails"), ref _useCabinetThumbnails, ConfigChange);
        }

        public override bool IsValid() => _presenter.IsValid();
    }
}
#endif
