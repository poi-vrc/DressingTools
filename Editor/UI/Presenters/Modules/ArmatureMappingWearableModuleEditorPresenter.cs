/*
 * File: ArmatureMappingModuleEditorPresenter.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 12:21:25 am
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

using System.Collections.Generic;
using Chocopoi.DressingFramework.Detail.DK.Logging;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;
using Chocopoi.DressingTools.UI.Views.Modules;
using Newtonsoft.Json;
using UnityEditor;
using LogType = Chocopoi.DressingFramework.Logging.LogType;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class ArmatureMappingWearableModuleEditorPresenter
    {
        private IArmatureMappingWearableModuleEditorView _view;
        private IWearableModuleEditorViewParent _parentView;
        private ArmatureMappingWearableModuleConfig _module;

        private DKReport _dresserReport = null;
        private DTCabinet _cabinet;

        public ArmatureMappingWearableModuleEditorPresenter(IArmatureMappingWearableModuleEditorView view, IWearableModuleEditorViewParent parentView, ArmatureMappingWearableModuleConfig module)
        {
            _view = view;
            _parentView = parentView;
            _module = module;

            ResetMappingEditorContainer();

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ForceUpdateView += OnForceUpdateView;
            _view.DresserChange += OnDresserChange;
            _view.ModuleSettingsChange += OnModuleSettingsChange;
            _view.DresserSettingsChange += OnDresserSettingsChange;
            _view.RegenerateMappingsButtonClick += OnRegenerateMappingsButtonClick;
            _view.ViewEditMappingsButtonClick += OnViewEditMappingsButtonClick;
            _view.MappingModeChange += OnMappingModeChange;
            DTMappingEditorWindow.Data.MappingEditorChanged += OnMappingEditorDataChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.DresserChange -= OnDresserChange;
            _view.ModuleSettingsChange -= OnModuleSettingsChange;
            _view.DresserSettingsChange -= OnDresserSettingsChange;
            _view.RegenerateMappingsButtonClick -= OnRegenerateMappingsButtonClick;
            _view.ViewEditMappingsButtonClick -= OnViewEditMappingsButtonClick;
            _view.MappingModeChange -= OnMappingModeChange;
            DTMappingEditorWindow.Data.MappingEditorChanged -= OnMappingEditorDataChange;
        }

        private void OnMappingEditorDataChange()
        {
            _module.boneMappingMode = DTMappingEditorWindow.Data.boneMappingMode;
            _module.boneMappings = _module.boneMappingMode != BoneMappingMode.Auto ? DTMappingEditorWindow.Data.outputBoneMappings : new List<BoneMapping>();
            UpdateView();
        }

        private void OnMappingModeChange()
        {
            _module.boneMappingMode = DTMappingEditorWindow.Data.boneMappingMode = (BoneMappingMode)_view.SelectedMappingMode;
            UpdateMappingEditorView(DTMappingEditorWindow.Data.generatedBoneMappings);
        }

        private void ResetMappingEditorContainer()
        {
            DTMappingEditorWindow.Data.Reset();
        }

        private void UpdateMappingEditorView(List<BoneMapping> generatedMappings)
        {
            // update data
            DTMappingEditorWindow.Data.targetAvatar = _parentView.TargetAvatar;
            DTMappingEditorWindow.Data.targetWearable = _parentView.TargetWearable;
            DTMappingEditorWindow.Data.boneMappingMode = _module.boneMappingMode;
            DTMappingEditorWindow.Data.outputBoneMappings = _module.boneMappings;
            DTMappingEditorWindow.Data.generatedBoneMappings = generatedMappings;

            // force update if opened
            if (EditorWindow.HasOpenInstances<DTMappingEditorWindow>())
            {
                var window = EditorWindow.GetWindow<DTMappingEditorWindow>();
                window.GetView().RaiseForceUpdateViewEvent();
            }
        }

        private void GenerateDresserMappings()
        {
            // execute dresser
            var dresser = DresserRegistry.GetDresserByIndex(_view.SelectedDresserIndex);
            _dresserReport = dresser.Execute(_view.DresserSettings, out var generatedMappings);
            UpdateMappingEditorView(generatedMappings);

            ApplySettings();
            UpdateDresserReport();
        }

        private void InitializeDresserSettings()
        {
            var dresser = DresserRegistry.GetDresserByName(_view.AvailableDresserKeys != null ? _view.AvailableDresserKeys[_view.SelectedDresserIndex] : "Default Dresser");
            _view.DresserSettings = dresser.DeserializeSettings(_module.serializedDresserConfig ?? "{}");
            if (_view.DresserSettings == null)
            {
                _view.DresserSettings = dresser.NewSettings();
            }
        }

        private void UpdateDresserReport()
        {
            if (_dresserReport != null)
            {
                _view.DresserReportData = new ReportData();
                var logEntries = _dresserReport.GetLogEntriesAsDictionary();

                _view.DresserReportData.errorMsgs.Clear();
                if (logEntries.ContainsKey(LogType.Error))
                {
                    foreach (var logEntry in logEntries[LogType.Error])
                    {
                        _view.DresserReportData.errorMsgs.Add(logEntry.message);
                    }
                }

                _view.DresserReportData.warnMsgs.Clear();
                if (logEntries.ContainsKey(LogType.Warning))
                {
                    foreach (var logEntry in logEntries[LogType.Warning])
                    {
                        _view.DresserReportData.warnMsgs.Add(logEntry.message);
                    }
                }

                _view.DresserReportData.infoMsgs.Clear();
                if (logEntries.ContainsKey(LogType.Info))
                {
                    foreach (var logEntry in logEntries[LogType.Info])
                    {
                        _view.DresserReportData.infoMsgs.Add(logEntry.message);
                    }
                }
            }
            else
            {
                _view.DresserReportData = null;
            }
        }

        private void CheckCorrectDresserSettingsType()
        {
            var dresser = DresserRegistry.GetDresserByIndex(_view.SelectedDresserIndex);

            // reinitialize dresser settings if not correct type
            if (dresser is DefaultDresser && !(_view.DresserSettings is DefaultDresserSettings))
            {
                InitializeDresserSettings();
            }
        }

        private void UpdateDresserSettings()
        {
            _view.DresserSettings.targetAvatar = _parentView.TargetAvatar;
            _view.DresserSettings.targetWearable = _parentView.TargetWearable;
            _cabinet = OneConfUtils.GetAvatarCabinet(_parentView.TargetAvatar);
            if (_cabinet != null)
            {
                _view.IsAvatarAssociatedWithCabinet = true;

                if (CabinetConfigUtility.TryDeserialize(_cabinet.ConfigJson, out var cabinetConfig))
                {
                    _view.AvatarArmatureName = cabinetConfig.avatarArmatureName;
                    _view.DresserSettings.avatarArmatureName = cabinetConfig.avatarArmatureName;
                    _view.IsLoadCabinetConfigError = false;
                }
                else
                {
                    _view.IsLoadCabinetConfigError = true;
                }
            }
            else
            {
                _view.IsAvatarAssociatedWithCabinet = false;
            }
        }

        private void OnForceUpdateView()
        {
            if (_view.DresserSettings == null)
            {
                InitializeDresserSettings();
            }
            UpdateDresserSettings();
            GenerateDresserMappings();
        }

        private void OnDresserChange()
        {
            CheckCorrectDresserSettingsType();
            GenerateDresserMappings();
        }

        private void OnModuleSettingsChange()
        {
            _view.DresserSettings.avatarArmatureName = _view.AvatarArmatureName;
            _module.groupBones = _view.GroupBones;
            _module.removeExistingPrefixSuffix = _view.RemoveExistingPrefixSuffix;
            GenerateDresserMappings();
        }

        private void OnDresserSettingsChange()
        {
            // serialize if modified
            _module.serializedDresserConfig = JsonConvert.SerializeObject(_view.DresserSettings);
            GenerateDresserMappings();
        }

        private void OnRegenerateMappingsButtonClick()
        {
            GenerateDresserMappings();
        }

        private void OnViewEditMappingsButtonClick()
        {
            _view.StartMappingEditor();
        }

        public void UpdateView()
        {
            _view.SelectedMappingMode = (int)_module.boneMappingMode;

            // list all available dressers
            _view.AvailableDresserKeys = DresserRegistry.GetAvailableDresserKeys();
            _view.SelectedDresserIndex = DresserRegistry.GetDresserKeyIndexByTypeName(_module.dresserName);
            if (_view.SelectedDresserIndex == -1)
            {
                _view.SelectedDresserIndex = 0;
            }

            _view.GroupBones = _module.groupBones;
            _view.RemoveExistingPrefixSuffix = _module.removeExistingPrefixSuffix;

            CheckCorrectDresserSettingsType();
            UpdateDresserSettings();
            UpdateDresserReport();
            UpdateMappingEditorView(DTMappingEditorWindow.Data.generatedBoneMappings);
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }

        private void ApplySettings()
        {
            var dresser = DresserRegistry.GetDresserByIndex(_view.SelectedDresserIndex);
            _module.dresserName = dresser.GetType().FullName;

            // copy wearable armature name from dresser settings and serialize dresser settings
            if (_view.DresserSettings != null)
            {
                _module.wearableArmatureName = _view.DresserSettings.wearableArmatureName;
            }
            _module.serializedDresserConfig = JsonConvert.SerializeObject(_view.DresserSettings);

            // update values from mapping editor container
            _module.boneMappingMode = DTMappingEditorWindow.Data.boneMappingMode;
            _module.boneMappings = _module.boneMappingMode != BoneMappingMode.Auto ? DTMappingEditorWindow.Data.outputBoneMappings : new List<BoneMapping>();
        }

        public bool IsValid()
        {
            ApplySettings();
            var validate = true;
            if (_module.boneMappingMode != BoneMappingMode.Manual)
            {
                validate &= _dresserReport != null && !_dresserReport.HasLogType(LogType.Error);
            }
            return validate && _module.boneMappings != null;
        }
    }
}
