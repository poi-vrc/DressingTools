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
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework.Detail.DK.Logging;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Standard;
using Chocopoi.DressingTools.Dresser.Tags;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;
using Chocopoi.DressingTools.Passes.Modifiers;
using Chocopoi.DressingTools.UI.Views.Modules;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using LogType = Chocopoi.DressingFramework.Logging.LogType;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class ArmatureMappingWearableModuleEditorPresenter
    {
        private const string OldDresserClassName = "Chocopoi.DressingTools.Dresser.Standard.DefaultDresser";

        private readonly IArmatureMappingWearableModuleEditorView _view;
        private readonly IWearableModuleEditorViewParent _parentView;
        private readonly ArmatureMappingWearableModuleConfig _module;

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

        private static List<BoneMapping> ConvertToOldMappings(Transform wearableRoot, List<ObjectMapping> objectMappings, List<ITag> tags)
        {
            // TODO: this is temporary for later migration to Object Mappings + Tags
            var output = new List<BoneMapping>();
            foreach (var objectMapping in objectMappings)
            {
                var boneMapping = new BoneMapping
                {
                    avatarBonePath = objectMapping.TargetPath,
                    wearableBonePath = AnimationUtils.GetRelativePath(objectMapping.SourceTransform.transform, wearableRoot)
                };

                if (objectMapping.Type == ObjectMapping.MappingType.MoveToBone)
                {
                    boneMapping.mappingType = BoneMappingType.MoveToBone;
                }
                else if (objectMapping.Type == ObjectMapping.MappingType.ParentConstraint)
                {
                    boneMapping.mappingType = BoneMappingType.ParentConstraint;
                }
                else if (objectMapping.Type == ObjectMapping.MappingType.IgnoreTransform)
                {
                    boneMapping.mappingType = BoneMappingType.IgnoreTransform;
                }

                output.Add(boneMapping);
            }

            foreach (var tag in tags)
            {
                var boneMapping = new BoneMapping()
                {
                    avatarBonePath = tag.TargetPath,
                    wearableBonePath = AnimationUtils.GetRelativePath(tag.SourceTransform.transform, wearableRoot)
                };

                if (tag is CopyDynamicsTag)
                {
                    boneMapping.mappingType = BoneMappingType.CopyDynamics;
                }

                output.Add(boneMapping);
            }
            return output;
        }

        private static StandardDresserSettings.DynamicsOptions ConvertToNewDynamicsOption(ArmatureMappingWearableModuleProvider.DefaultDresserSettings.DynamicsOptions dynamicsOptions)
        {
            if (dynamicsOptions == ArmatureMappingWearableModuleProvider.DefaultDresserSettings.DynamicsOptions.RemoveDynamicsAndUseParentConstraint)
            {
                return StandardDresserSettings.DynamicsOptions.RemoveDynamicsAndUseParentConstraint;
            }
            else if (dynamicsOptions == ArmatureMappingWearableModuleProvider.DefaultDresserSettings.DynamicsOptions.KeepDynamicsAndUseParentConstraintIfNecessary)
            {
                return StandardDresserSettings.DynamicsOptions.KeepDynamicsAndUseParentConstraintIfNecessary;
            }
            else if (dynamicsOptions == ArmatureMappingWearableModuleProvider.DefaultDresserSettings.DynamicsOptions.IgnoreTransform)
            {
                return StandardDresserSettings.DynamicsOptions.IgnoreTransform;
            }
            else if (dynamicsOptions == ArmatureMappingWearableModuleProvider.DefaultDresserSettings.DynamicsOptions.CopyDynamics)
            {
                return StandardDresserSettings.DynamicsOptions.CopyDynamics;
            }
            else if (dynamicsOptions == ArmatureMappingWearableModuleProvider.DefaultDresserSettings.DynamicsOptions.IgnoreAll)
            {
                return StandardDresserSettings.DynamicsOptions.IgnoreAll;
            }
            else
            {
                return StandardDresserSettings.DynamicsOptions.Auto;
            }
        }

        private void GenerateDresserMappings()
        {
            if (_parentView.TargetWearable == null || string.IsNullOrEmpty(_view.WearableArmatureName))
            {
                return;
            }

            var avatarArmature = OneConfUtils.GuessArmature(_parentView.TargetAvatar, _view.AvatarArmatureName);
            var wearableArmature = OneConfUtils.GuessArmature(_parentView.TargetWearable, _view.WearableArmatureName);

            // execute default dresser (dresser selection is ignored now)
            var dresser = new StandardDresser();
            var settings = new StandardDresserSettings()
            {
                SourceArmature = wearableArmature,
                TargetArmaturePath = avatarArmature != null ? avatarArmature.name : _view.AvatarArmatureName,
                DynamicsOption = ConvertToNewDynamicsOption(_view.DresserSettings.dynamicsOption)
            };
            _dresserReport = new DKReport();
            dresser.Execute(_dresserReport, _parentView.TargetAvatar, settings, out var objectMappings, out var tags);

            if (objectMappings != null && tags != null)
            {
                // TODO: this is temporary for later migration to Object Mappings + Tags
                var oldObjectMappings = ConvertToOldMappings(_parentView.TargetWearable.transform, objectMappings, tags);
                UpdateMappingEditorView(oldObjectMappings);
            }
            else
            {
                UpdateMappingEditorView(null);
            }

            ApplySettings();
            UpdateDresserReport();
        }

        private void InitializeDresserSettings()
        {
            _view.DresserSettings = JsonConvert.DeserializeObject<ArmatureMappingWearableModuleProvider.DefaultDresserSettings>(_module.serializedDresserConfig ?? "{}");
            _view.DresserSettings ??= new ArmatureMappingWearableModuleProvider.DefaultDresserSettings();
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

        private void UpdateDresserSettings()
        {
            _cabinet = OneConfUtils.GetAvatarCabinet(_parentView.TargetAvatar, true);
            if (_cabinet != null)
            {
                _view.IsAvatarAssociatedWithCabinet = true;

                if (CabinetConfigUtility.TryDeserialize(_cabinet.ConfigJson, out var cabinetConfig))
                {
                    _view.AvatarArmatureName = cabinetConfig.avatarArmatureName;
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
            GenerateDresserMappings();
        }

        private void OnModuleSettingsChange()
        {
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
            _view.AvailableDresserKeys = new string[] { "Default" };
            _view.SelectedDresserIndex = 0;

            _view.GroupBones = _module.groupBones;
            _view.RemoveExistingPrefixSuffix = _module.removeExistingPrefixSuffix;
            _view.WearableArmatureName = _module.wearableArmatureName;

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
            _module.dresserName = OldDresserClassName;
            _module.wearableArmatureName = _view.WearableArmatureName;
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
