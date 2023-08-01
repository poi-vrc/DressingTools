using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [CustomModuleEditor(typeof(ArmatureMappingModule))]
    internal class ArmatureMappingModuleEditor : ModuleEditor, IArmatureMappingModuleEditorView
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public event Action TargetAvatarOrWearableChange { add { _configView.TargetAvatarOrWearableChange += value; } remove { _configView.TargetAvatarOrWearableChange -= value; } }
        public event Action DresserChange;
        public event Action AvatarArmatureNameChange;
        public event Action DresserSettingsChange;
        public event Action RegenerateMappingsButtonClick;
        public event Action ViewEditMappingsButtonClick;

        public ReportData DresserReportData { get; set; }
        public DTDresserSettings DresserSettings { get; set; }
        public string[] AvailableDresserKeys { get; set; }
        public int SelectedDresserIndex { get => _selectedDresserIndex; set => _selectedDresserIndex = value; }
        public bool IsAvatarAssociatedWithCabinet { get; set; }
        public string AvatarArmatureName { get => _avatarArmatureName; set => _avatarArmatureName = value; }

        private ArmatureMappingModuleEditorPresenter _presenter;
        private IWearableConfigView _configView;
        private int _selectedDresserIndex;
        private string _avatarArmatureName;
        private bool _foldoutDresserReportLogEntries;

        public ArmatureMappingModuleEditor(IWearableConfigView configView, DTWearableModuleBase target) : base(configView, target)
        {
            _configView = configView;
            _presenter = new ArmatureMappingModuleEditorPresenter(this, configView, (ArmatureMappingModule)target);
            _selectedDresserIndex = 0;
            _avatarArmatureName = null;
            _foldoutDresserReportLogEntries = true;

            DresserReportData = null;
            DresserSettings = null;
        }

        private void DrawDresserReportGUI()
        {
            if (DresserReportData != null)
            {
                //Result

                if (DresserReportData.errorMsgs.Count > 0)
                {
                    HelpBox(t._("helpbox_error_check_result_incompatible"), MessageType.Error);
                }
                else if (DresserReportData.warnMsgs.Count > 0)
                {
                    HelpBox(t._("helpbox_warn_check_result_compatible"), MessageType.Warning);
                }
                else
                {
                    HelpBox(t._("helpbox_info_check_result_ok"), MessageType.Info);
                }

                Separator();

                BeginHorizontal();
                {
                    Label("Errors: " + DresserReportData.errorMsgs.Count);
                    Label("Warnings: " + DresserReportData.warnMsgs.Count);
                    Label("Infos: " + DresserReportData.infoMsgs.Count);
                }
                EndHorizontal();

                BeginFoldoutBox(ref _foldoutDresserReportLogEntries, "Logs");
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
                HelpBox(t._("helpbox_warn_no_check_report"), MessageType.Warning);
            }
        }

        public override void OnGUI()
        {
            // list all available dressers
            Popup("Dressers", ref _selectedDresserIndex, AvailableDresserKeys, DresserChange);

            if (IsAvatarAssociatedWithCabinet)
            {
                HelpBox("The avatar is associated with a cabinet. To change the avatar Armature name, please use the cabinet editor.", MessageType.Info);
            }
            BeginDisabled(IsAvatarAssociatedWithCabinet);
            {
                DelayedTextField("Avatar Armature Name", ref _avatarArmatureName, AvatarArmatureNameChange);
            }
            EndDisabled();

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
                HelpBox("Unable to render dresser settings.", MessageType.Error);
            }

            Separator();

            BeginHorizontal();
            {
                Button("Regenerate Mappings", RegenerateMappingsButtonClick);
                Button("View/Edit Mappings", ViewEditMappingsButtonClick);
            }
            EndHorizontal();

            BeginHorizontal();
            {
                BeginDisabled(DresserReportData == null);
                {
                    // TODO: handle view report
                    Button("View Report");
                }
                EndDisabled();
                BeginDisabled(true);
                {
                    // TODO: handle test now
                    Button("Test Now");
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
