/*
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
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using UnityEditor;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [ExcludeFromCodeCoverage]
    [CustomWearableModuleEditor(typeof(ArmatureMappingWearableModuleProvider))]
    internal class ArmatureMappingWearableModuleEditor : WearableModuleEditorIMGUI, IArmatureMappingWearableModuleEditorView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action MappingModeChange;
        public event Action DresserChange;
        public event Action ModuleSettingsChange;
        public event Action DresserSettingsChange;
        public event Action RegenerateMappingsButtonClick;
        public event Action ViewEditMappingsButtonClick;

        public ReportData DresserReportData { get; set; }
        public string[] AvailableDresserKeys { get; set; }
        public int SelectedMappingMode { get => _selectedMappingMode; set => _selectedMappingMode = value; }
        public int SelectedDresserIndex { get => _selectedDresserIndex; set => _selectedDresserIndex = value; }
        public bool IsAvatarAssociatedWithCabinet { get; set; }
        public bool IsLoadCabinetConfigError { get; set; }
        public string AvatarArmatureName { get => _avatarArmatureName; set => _avatarArmatureName = value; }
        public bool RemoveExistingPrefixSuffix { get => _removeExistingPrefixSuffix; set => _removeExistingPrefixSuffix = value; }
        public bool GroupBones { get => _groupBones; set => _groupBones = value; }
        public ArmatureMappingWearableModuleProvider.DefaultDresserSettings DresserSettings { get; set; }
        public string WearableArmatureName { get => _wearableArmatureName; set => _wearableArmatureName = value; }

        private readonly ArmatureMappingWearableModuleEditorPresenter _presenter;
        private readonly IWearableModuleEditorViewParent _parentView;
        private int _selectedMappingMode;
        private int _selectedDresserIndex;
        private string _avatarArmatureName;
        private string _wearableArmatureName;
        private bool _foldoutDresserReportLogEntries;
        private bool _removeExistingPrefixSuffix;
        private bool _groupBones;

        public ArmatureMappingWearableModuleEditor(IWearableModuleEditorViewParent parentView, WearableModuleProvider provider, IModuleConfig target) : base(parentView, provider, target)
        {
            _parentView = parentView;
            _presenter = new ArmatureMappingWearableModuleEditorPresenter(this, parentView, (ArmatureMappingWearableModuleConfig)target);
            _selectedDresserIndex = 0;
            _avatarArmatureName = null;
            _wearableArmatureName = null;
            _foldoutDresserReportLogEntries = true;
            _groupBones = true;
            _removeExistingPrefixSuffix = true;

            DresserReportData = null;
            DresserSettings = null;
        }

        public void StartMappingEditor()
        {
            DTMappingEditorWindow.StartMappingEditor();
        }

        private void DrawDresserReportGUI()
        {
            if (DresserReportData != null)
            {
                //Result

                if (DresserReportData.errorMsgs.Count > 0)
                {
                    HelpBox(t._("report.editor.helpbox.resultError"), MessageType.Error);
                }
                else if (DresserReportData.warnMsgs.Count > 0)
                {
                    HelpBox(t._("report.editor.helpbox.resultWarn"), MessageType.Warning);
                }
                else
                {
                    HelpBox(t._("report.editor.helpbox.resultSuccess"), MessageType.Info);
                }

                Separator();

                BeginHorizontal();
                {
                    Label(t._("report.editor.label.errors", DresserReportData.errorMsgs.Count));
                    Label(t._("report.editor.label.warnings", DresserReportData.warnMsgs.Count));
                    Label(t._("report.editor.label.infos", DresserReportData.infoMsgs.Count));
                }
                EndHorizontal();

                BeginFoldoutBox(ref _foldoutDresserReportLogEntries, t._("report.editor.label.logs"));
                if (_foldoutDresserReportLogEntries)
                {
                    foreach (var msg in DresserReportData.errorMsgs)
                    {
                        EditorGUILayout.HelpBox(msg, MessageType.Error);
                    }

                    foreach (var msg in DresserReportData.warnMsgs)
                    {
                        EditorGUILayout.HelpBox(msg, MessageType.Warning);
                    }

                    foreach (var msg in DresserReportData.infoMsgs)
                    {
                        EditorGUILayout.HelpBox(msg, MessageType.Info);
                    }
                }
                EndFoldoutBox();
            }
            else
            {
                HelpBox(t._("modules.wearable.armatureMapping.editor.helpbox.noReportGenerated"), MessageType.Warning);
            }
        }

        public override void OnGUI()
        {
            ToggleLeft(t._("modules.wearable.armatureMapping.editor.toggle.removeExistingPrefixesAndSuffixes"), ref _removeExistingPrefixSuffix, ModuleSettingsChange);
            ToggleLeft(t._("modules.wearable.armatureMapping.editor.toggle.groupBones"), ref _groupBones, ModuleSettingsChange);

            Popup(t._("modules.wearable.armatureMapping.editor.popup.mappingMode"), ref _selectedMappingMode, new string[] {
                t._("modules.wearable.armatureMapping.editor.popup.mappingMode.auto"),
                t._("modules.wearable.armatureMapping.editor.popup.mappingMode.override"),
                t._("modules.wearable.armatureMapping.editor.popup.mappingMode.manual") }, MappingModeChange);

            Button(t._("modules.wearable.armatureMapping.editor.btn.viewEditMappings"), ViewEditMappingsButtonClick);

            // not manual mode
            if (_selectedMappingMode != 2)
            {
                HorizontalLine();

                // list all available dressers
                Popup(t._("modules.wearable.armatureMapping.editor.popup.dressers"), ref _selectedDresserIndex, AvailableDresserKeys, DresserChange);

                if (IsAvatarAssociatedWithCabinet)
                {
                    HelpBox(t._("modules.wearable.armatureMapping.editor.helpbox.assocatedWithCabinetUseEditorChangeArmatureName"), MessageType.Info);
                }
                if (IsLoadCabinetConfigError)
                {
                    HelpBox(t._("modules.wearable.armatureMapping.editor.helpbox.unableToLoadCabinetConfig"), MessageType.Error);
                }
                BeginDisabled(IsAvatarAssociatedWithCabinet);
                {
                    DelayedTextField(t._("modules.wearable.armatureMapping.editor.textField.avatarArmatureName"), ref _avatarArmatureName, ModuleSettingsChange);
                }
                EndDisabled();
                DelayedTextField(t._("modules.wearable.armatureMapping.editor.textField.wearableArmatureName"), ref _wearableArmatureName, ModuleSettingsChange);

                // TODO: the current way to draw dresser settings is not in MVP pattern
                if (DresserSettings != null)
                {
                    if (DresserSettings.DrawEditorGUI())
                    {
                        // raise dresser settings changed event
                        DresserSettingsChange?.Invoke();
                    }
                }
                else
                {
                    HelpBox(t._("modules.wearable.armatureMapping.editor.helpbox.unableToRenderDresserSettings"), MessageType.Error);
                }

                Separator();

                Button(t._("modules.wearable.armatureMapping.editor.btn.regenerateMappings"), RegenerateMappingsButtonClick);

                HorizontalLine();

                DrawDresserReportGUI();
            }
        }

        public override bool IsValid() => _presenter.IsValid();
    }
}
