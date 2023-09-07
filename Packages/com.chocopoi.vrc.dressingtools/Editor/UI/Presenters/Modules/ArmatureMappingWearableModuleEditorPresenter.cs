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
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class ArmatureMappingWearableModuleEditorPresenter
    {
        private IArmatureMappingWearableModuleEditorView _view;
        private IWearableModuleEditorViewParent _parentView;
        private ArmatureMappingWearableModuleConfig _module;

        private DTReport _dresserReport = null;
        private DTMappingEditorContainer _mappingEditorContainer;
        private DTCabinet _cabinet;

        public ArmatureMappingWearableModuleEditorPresenter(IArmatureMappingWearableModuleEditorView view, IWearableModuleEditorViewParent parentView, ArmatureMappingWearableModuleConfig module)
        {
            _view = view;
            _parentView = parentView;
            _module = module;

            _mappingEditorContainer = new DTMappingEditorContainer();
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
            _view.ViewReportButtonClick += OnViewReportButtonClick;
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
            _view.ViewReportButtonClick -= OnViewReportButtonClick;
        }

        private void OnViewReportButtonClick()
        {
            if (_dresserReport != null)
            {
                ReportWindow.ShowWindow(_dresserReport);
            }
        }

        private void ResetMappingEditorContainer()
        {
            _mappingEditorContainer.targetAvatar = null;
            _mappingEditorContainer.targetWearable = null;
            _mappingEditorContainer.generatedBoneMappings = null;
            _mappingEditorContainer.boneMappingMode = BoneMappingMode.Auto;
        }

        private void GenerateDresserMappings()
        {
            // reset mapping editor container
            ResetMappingEditorContainer();
            _mappingEditorContainer.targetAvatar = _parentView.TargetAvatar;
            _mappingEditorContainer.targetWearable = _parentView.TargetWearable;

            // execute dresser
            var dresser = DresserRegistry.GetDresserByIndex(_view.SelectedDresserIndex);
            _dresserReport = dresser.Execute(_view.DresserSettings, out _mappingEditorContainer.generatedBoneMappings);

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
                if (logEntries.ContainsKey(DTReportLogType.Error))
                {
                    foreach (var logEntry in logEntries[DTReportLogType.Error])
                    {
                        _view.DresserReportData.errorMsgs.Add(logEntry.message);
                    }
                }

                _view.DresserReportData.warnMsgs.Clear();
                if (logEntries.ContainsKey(DTReportLogType.Warning))
                {
                    foreach (var logEntry in logEntries[DTReportLogType.Warning])
                    {
                        _view.DresserReportData.warnMsgs.Add(logEntry.message);
                    }
                }

                _view.DresserReportData.infoMsgs.Clear();
                if (logEntries.ContainsKey(DTReportLogType.Info))
                {
                    foreach (var logEntry in logEntries[DTReportLogType.Info])
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
            _cabinet = DTEditorUtils.GetAvatarCabinet(_parentView.TargetAvatar);
            if (_cabinet != null)
            {
                _view.IsAvatarAssociatedWithCabinet = true;

                if (CabinetConfig.TryDeserialize(_cabinet.configJson, out var cabinetConfig))
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
            _view.StartMappingEditor(_mappingEditorContainer);
        }

        public void UpdateView()
        {
            // list all available dressers
            _view.AvailableDresserKeys = DresserRegistry.GetAvailableDresserKeys();
            _view.SelectedDresserIndex = DresserRegistry.GetDresserKeyIndexByTypeName(_module.dresserName);
            if (_view.SelectedDresserIndex == -1)
            {
                _view.SelectedDresserIndex = 0;
            }

            _view.GroupBones = _module.groupBones;
            _view.RemoveExistingPrefixSuffix = _module.removeExistingPrefixSuffix;

            var regenerateMappingsNeeded = false;

            CheckCorrectDresserSettingsType();

            UpdateDresserSettings();

            UpdateDresserReport();

            if (regenerateMappingsNeeded)
            {
                regenerateMappingsNeeded = false;
                GenerateDresserMappings();
            }
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
            _module.boneMappingMode = _mappingEditorContainer.boneMappingMode;
            _module.boneMappings = _module.boneMappingMode != BoneMappingMode.Auto ? _mappingEditorContainer.generatedBoneMappings : new List<BoneMapping>();
        }

        public bool IsValid()
        {
            ApplySettings();
            return _dresserReport != null && !_dresserReport.HasLogType(DTReportLogType.Error) && _module.boneMappings != null;
        }
    }
}
