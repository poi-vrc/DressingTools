using System;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class ArmatureMappingModuleEditorPresenter
    {
        private IArmatureMappingModuleEditorView view_;
        private IWearableConfigView configView_;
        private ArmatureMappingModule module_;

        private DTReport dresserReport_ = null;
        private DTMappingEditorContainer mappingEditorContainer_;
        private DTCabinet cabinet_;

        public ArmatureMappingModuleEditorPresenter(IArmatureMappingModuleEditorView view, IWearableConfigView configView, ArmatureMappingModule module)
        {
            view_ = view;
            configView_ = configView;
            module_ = module;

            mappingEditorContainer_ = new DTMappingEditorContainer();
            ResetMappingEditorContainer();

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            view_.Load += OnLoad;
            view_.Unload += OnUnload;

            view_.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;
            view_.DresserChange += OnDresserChange;
            view_.AvatarArmatureNameChange += OnAvatarArmatureNameChange;
            view_.DresserSettingsChange += OnDresserSettingsChange;
            view_.RegenerateMappingsButtonClick += OnRegenerateMappingsButtonClick;
            view_.ViewEditMappingsButtonClick += OnViewEditMappingsButtonClick;
        }

        private void UnsubscribeEvents()
        {
            view_.Load -= OnLoad;
            view_.Unload -= OnUnload;

            view_.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
            view_.DresserChange -= OnDresserChange;
            view_.AvatarArmatureNameChange -= OnAvatarArmatureNameChange;
            view_.DresserSettingsChange -= OnDresserSettingsChange;
            view_.RegenerateMappingsButtonClick -= OnRegenerateMappingsButtonClick;
            view_.ViewEditMappingsButtonClick -= OnViewEditMappingsButtonClick;
        }

        private void ResetMappingEditorContainer()
        {
            mappingEditorContainer_.dresserSettings = null;
            mappingEditorContainer_.boneMappings = null;
            mappingEditorContainer_.boneMappingMode = DTBoneMappingMode.Auto;
        }

        private void GenerateDresserMappings()
        {
            // reset mapping editor container
            ResetMappingEditorContainer();
            mappingEditorContainer_.dresserSettings = view_.DresserSettings;

            // execute dresser
            var dresser = DresserRegistry.GetDresserByIndex(view_.SelectedDresserIndex);
            dresserReport_ = dresser.Execute(view_.DresserSettings, out mappingEditorContainer_.boneMappings);

            UpdateDresserReport();
        }

        public void StartMappingEditor()
        {
            var boneMappingEditorWindow = (DTMappingEditorWindow)EditorWindow.GetWindow(typeof(DTMappingEditorWindow));

            boneMappingEditorWindow.SetSettings(mappingEditorContainer_);
            boneMappingEditorWindow.titleContent = new GUIContent("DT Mapping Editor");
            boneMappingEditorWindow.Show();
        }

        private void InitializeDresserSettings()
        {
            var dresser = DresserRegistry.GetDresserByName(view_.AvailableDresserKeys[view_.SelectedDresserIndex]);
            view_.DresserSettings = dresser.DeserializeSettings(module_.serializedDresserConfig ?? "{}");
            if (view_.DresserSettings == null)
            {
                view_.DresserSettings = dresser.NewSettings();
            }
        }

        private void UpdateDresserReport()
        {
            if (dresserReport_ != null)
            {
                view_.DresserReportData = new ReportData();
                var logEntries = dresserReport_.GetLogEntriesAsDictionary();

                view_.DresserReportData.errorMsgs.Clear();
                if (logEntries.ContainsKey(DTReportLogType.Error))
                {
                    foreach (var logEntry in logEntries[DTReportLogType.Error])
                    {
                        view_.DresserReportData.errorMsgs.Add(logEntry.message);
                    }
                }

                view_.DresserReportData.warnMsgs.Clear();
                if (logEntries.ContainsKey(DTReportLogType.Warning))
                {
                    foreach (var logEntry in logEntries[DTReportLogType.Warning])
                    {
                        view_.DresserReportData.warnMsgs.Add(logEntry.message);
                    }
                }

                view_.DresserReportData.infoMsgs.Clear();
                if (logEntries.ContainsKey(DTReportLogType.Info))
                {
                    foreach (var logEntry in logEntries[DTReportLogType.Info])
                    {
                        view_.DresserReportData.infoMsgs.Add(logEntry.message);
                    }
                }
            }
            else
            {
                view_.DresserReportData = null;
            }
        }

        private void CheckCorrectDresserSettingsType()
        {
            var dresser = DresserRegistry.GetDresserByIndex(view_.SelectedDresserIndex);

            // reinitialize dresser settings if not correct type
            if (dresser is DTDefaultDresser && !(view_.DresserSettings is DTDefaultDresserSettings))
            {
                InitializeDresserSettings();
            }
        }

        private void UpdateDresserSettings()
        {
            view_.DresserSettings.targetAvatar = configView_.TargetAvatar;
            view_.DresserSettings.targetWearable = configView_.TargetWearable;
            cabinet_ = DTEditorUtils.GetAvatarCabinet(configView_.TargetAvatar);
            if (cabinet_ != null)
            {
                view_.IsAvatarAssociatedWithCabinet = true;
                view_.AvatarArmatureName = cabinet_.avatarArmatureName;
                view_.DresserSettings.avatarArmatureName = cabinet_.avatarArmatureName;
            }
            else
            {
                view_.IsAvatarAssociatedWithCabinet = false;
            }
        }

        private void OnTargetAvatarOrWearableChange()
        {
            UpdateDresserSettings();
            GenerateDresserMappings();
        }

        private void OnDresserChange()
        {
            CheckCorrectDresserSettingsType();
            GenerateDresserMappings();
        }

        private void OnAvatarArmatureNameChange()
        {
            view_.DresserSettings.avatarArmatureName = view_.AvatarArmatureName;
            GenerateDresserMappings();
        }

        private void OnDresserSettingsChange()
        {
            // serialize if modified
            module_.serializedDresserConfig = JsonConvert.SerializeObject(view_.DresserSettings);
            GenerateDresserMappings();
        }

        private void OnRegenerateMappingsButtonClick()
        {
            GenerateDresserMappings();
        }

        private void OnViewEditMappingsButtonClick()
        {
            StartMappingEditor();
        }

        public void UpdateView()
        {
            // list all available dressers
            view_.AvailableDresserKeys = DresserRegistry.GetAvailableDresserKeys();
            view_.SelectedDresserIndex = DresserRegistry.GetDresserKeyIndexByTypeName(module_.dresserName);
            if (view_.SelectedDresserIndex == -1)
            {
                view_.SelectedDresserIndex = 0;
            }

            var regenerateMappingsNeeded = false;

            // initial dresser settings if null
            if (view_.DresserSettings == null)
            {
                InitializeDresserSettings();
            }
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

        public bool IsValid()
        {
            var dresser = DresserRegistry.GetDresserByIndex(view_.SelectedDresserIndex);

            module_.dresserName = dresser.GetType().FullName;

            // copy wearable armature name from dresser settings and serialize dresser settings
            if (view_.DresserSettings != null)
            {
                module_.wearableArmatureName = view_.DresserSettings.wearableArmatureName;
            }
            module_.serializedDresserConfig = JsonConvert.SerializeObject(view_.DresserSettings);

            // update values from mapping editor container
            module_.boneMappingMode = mappingEditorContainer_.boneMappingMode;
            module_.boneMappings = module_.boneMappingMode != DTBoneMappingMode.Auto ? mappingEditorContainer_.boneMappings?.ToArray() : new DTBoneMapping[0];

            return dresserReport_ != null && !dresserReport_.HasLogType(DTReportLogType.Error) && module_.boneMappings != null;
        }
    }
}
