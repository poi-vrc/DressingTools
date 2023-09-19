/*
 * File: ArmatureMappingModuleEditor.cs
 * Project: DressingTools
 * Created Date: Thursday, August 10th 2023, 12:27:04 am
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
using Chocopoi.DressingFramework.Dresser;
using Chocopoi.DressingFramework.Extensibility.Providers;
using Chocopoi.DressingFramework.UI;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [ExcludeFromCodeCoverage]
    [CustomWearableModuleEditor(typeof(ArmatureMappingWearableModuleProvider))]
    internal class ArmatureMappingWearableModuleEditor : WearableModuleEditor, IArmatureMappingWearableModuleEditorView
    {
        private static Localization.I18n t = Localization.I18n.Instance;

        public event Action DresserChange;
        public event Action ModuleSettingsChange;
        public event Action DresserSettingsChange;
        public event Action RegenerateMappingsButtonClick;
        public event Action ViewEditMappingsButtonClick;
        public event Action ViewReportButtonClick;

        public ReportData DresserReportData { get; set; }
        public DresserSettings DresserSettings { get; set; }
        public string[] AvailableDresserKeys { get; set; }
        public int SelectedDresserIndex { get => _selectedDresserIndex; set => _selectedDresserIndex = value; }
        public bool IsAvatarAssociatedWithCabinet { get; set; }
        public bool IsLoadCabinetConfigError { get; set; }
        public string AvatarArmatureName { get => _avatarArmatureName; set => _avatarArmatureName = value; }
        public bool RemoveExistingPrefixSuffix { get => _removeExistingPrefixSuffix; set => _removeExistingPrefixSuffix = value; }
        public bool GroupBones { get => _groupBones; set => _groupBones = value; }

        private ArmatureMappingWearableModuleEditorPresenter _presenter;
        private IWearableModuleEditorViewParent _parentView;
        private int _selectedDresserIndex;
        private string _avatarArmatureName;
        private bool _foldoutDresserReportLogEntries;
        private bool _removeExistingPrefixSuffix;
        private bool _groupBones;

        public ArmatureMappingWearableModuleEditor(IWearableModuleEditorViewParent parentView, WearableModuleProviderBase provider, IModuleConfig target) : base(parentView, provider, target)
        {
            _parentView = parentView;
            _presenter = new ArmatureMappingWearableModuleEditorPresenter(this, parentView, (ArmatureMappingWearableModuleConfig)target);
            _selectedDresserIndex = 0;
            _avatarArmatureName = null;
            _foldoutDresserReportLogEntries = true;
            _groupBones = true;
            _removeExistingPrefixSuffix = true;

            DresserReportData = null;
            DresserSettings = null;
        }

        public void StartMappingEditor(DTMappingEditorContainer container)
        {
            var boneMappingEditorWindow = (DTMappingEditorWindow)EditorWindow.GetWindow(typeof(DTMappingEditorWindow));

            boneMappingEditorWindow.SetContainer(container);
            boneMappingEditorWindow.titleContent = new GUIContent(t._("modules.wearable.armatureMapping.editor.title"));
            boneMappingEditorWindow.Show();
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
            ToggleLeft(t._("modules.wearable.armatureMapping.editor.toggle.removeExistingPrefixesAndSuffixes"), ref _removeExistingPrefixSuffix, ModuleSettingsChange);
            ToggleLeft(t._("modules.wearable.armatureMapping.editor.toggle.groupBones"), ref _groupBones, ModuleSettingsChange);

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

            BeginHorizontal();
            {
                Button(t._("modules.wearable.armatureMapping.editor.btn.regenerateMappings"), RegenerateMappingsButtonClick);
                Button(t._("modules.wearable.armatureMapping.editor.btn.viewEditMappings"), ViewEditMappingsButtonClick);
            }
            EndHorizontal();

            BeginHorizontal();
            {
                BeginDisabled(DresserReportData == null);
                {
                    Button(t._("modules.wearable.armatureMapping.editor.btn.viewReport"), ViewReportButtonClick);
                }
                EndDisabled();
                BeginDisabled(true);
                {
                    // TODO: handle test now
                    Button(t._("modules.wearable.armatureMapping.editor.btn.testNow"));
                }
                EndDisabled();
            }
            EndHorizontal();

            HorizontalLine();

            DrawDresserReportGUI();
        }

        public override bool IsValid() => _presenter.IsValid();
    }
}
