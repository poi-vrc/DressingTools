/*
 * File: LegacyView.cs
 * Project: DressingTools
 * Created Date: Wednesday, September 10th 2023, 2:45:04 pm
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.View
{
    [ExcludeFromCodeCoverage]
    internal class LegacyView : EditorViewBase, ILegacyView
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

        public DressingToolsUpdater.ParsedVersion CurrentVersion { get; set; }

        public event Action TargetAvatarOrWearableChange;
        public event Action RenameClothesNameButtonClick;
        public event Action ConfigChange;
        public event Action CheckAndPreviewButtonClick;
        public event Action TestNowButtonClick;
        public event Action DressNowButtonClick;

        public GameObject TargetAvatar { get => _targetAvatar; set => _targetAvatar = value; }
        public GameObject TargetClothes { get => _targetClothes; set => _targetClothes = value; }
        public string NewClothesName { get => _newClothesName; set => _newClothesName = value; }
        public bool UseCustomArmatureName { get => _useCustomArmatureNames; set => _useCustomArmatureNames = value; }
        public string AvatarArmatureObjectName { get => _avatarArmatureObjectName; set => _avatarArmatureObjectName = value; }
        public string ClothesArmatureObjectName { get => _clothesArmatureObjectName; set => _clothesArmatureObjectName = value; }
        public bool GroupBones { get => _groupBones; set => _groupBones = value; }
        public bool GroupDynamics { get => _groupDynamics; set => _groupDynamics = value; }
        public bool GroupDynamicsSeparateGameObjects { get => _groupDynamicsSeparateGameObjects; set => _groupDynamicsSeparateGameObjects = value; }
        public bool RemoveExistingPrefixSuffix { get => _removeExistingPrefixSuffix; set => _removeExistingPrefixSuffix = value; }
        public int DynamicsOption { get => _dynamicsOption; set => _dynamicsOption = value; }
        public ReportData ReportData { get => _reportData; set => _reportData = value; }
        public bool ShowHasCabinetHelpbox { get; set; }

        private LegacyPresenter _presenter;
        private int _selectedMode;
        private Vector2 _scrollPos;
        private bool _foldoutReportLogEntries;
        private GameObject _targetAvatar;
        private GameObject _targetClothes;
        private string _newClothesName;
        private bool _useCustomArmatureNames;
        private string _avatarArmatureObjectName;
        private string _clothesArmatureObjectName;
        private bool _groupBones;
        private bool _groupDynamics;
        private bool _groupDynamicsSeparateGameObjects;
        private bool _removeExistingPrefixSuffix;
        private int _dynamicsOption;
        private ReportData _reportData;

        public LegacyView()
        {
            _presenter = new LegacyPresenter(this);

            _selectedMode = 0;
            _scrollPos = Vector2.zero;
            _foldoutReportLogEntries = true;
            _targetAvatar = null;
            _targetClothes = null;
            _newClothesName = "";
            _useCustomArmatureNames = false;
            _avatarArmatureObjectName = "Armature";
            _clothesArmatureObjectName = "Armature";
            _groupBones = true;
            _groupDynamics = true;
            _groupDynamicsSeparateGameObjects = true;
            _removeExistingPrefixSuffix = true;
            _dynamicsOption = 0;
            _reportData = null;
        }

        public bool ShowExistingWearableConfigIgnoreConfirmDialog()
        {
            return EditorUtility.DisplayDialog(t._("tool.name"), t._("legacy.editor.dialog.msg.existingWearableConfigIgnoreConfirm"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no"));
        }

        public bool ShowDressConfirmDialog()
        {
            return EditorUtility.DisplayDialog(t._("tool.name"), t._("legacy.editor.dialog.msg.dressConfirm"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no"));
        }

        public void ShowCompletedDialog()
        {
            EditorUtility.DisplayDialog(t._("tool.name"), t._("legacy.editor.dialog.msg.completed"), t._("common.dialog.btn.ok"));
        }

        private void DrawReportResult()
        {
            if (_reportData == null)
            {
                HelpBox(t._("legacy.editor.helpbox.noDressReportGenerated"), MessageType.Warning);
                return;
            }

            //Result

            if (_reportData.errorMsgs.Count > 0)
            {
                HelpBox(t._("report.editor.helpbox.resultError"), MessageType.Error);
            }
            else if (_reportData.warnMsgs.Count > 0)
            {
                HelpBox(t._("report.editor.helpbox.resultWarn"), MessageType.Warning);
            }
            else
            {
                HelpBox(t._("report.editor.helpbox.resultSuccess"), MessageType.Info);
            }
        }

        private void DrawReportDetails()
        {
            if (_reportData == null)
            {
                return;
            }

            BeginFoldoutBox(ref _foldoutReportLogEntries, t._("legacy.editor.foldout.label.logs"));
            if (_foldoutReportLogEntries)
            {
                foreach (var msg in _reportData.errorMsgs)
                {
                    HelpBox(msg, MessageType.Error);
                }

                foreach (var msg in _reportData.warnMsgs)
                {
                    HelpBox(msg, MessageType.Warning);
                }

                foreach (var msg in _reportData.infoMsgs)
                {
                    HelpBox(msg, MessageType.Info);
                }
            }
            EndFoldoutBox();
        }

        private void DrawCommonClothesConfig()
        {
            Label(t._("legacy.editor.label.setup"), EditorStyles.boldLabel);
            HelpBox(t._("legacy.editor.helpbox.moveClothesIntoPlace"), MessageType.Info);

            GameObjectField(t._("legacy.editor.gameObjectField.avatar"), ref _targetAvatar, true, TargetAvatarOrWearableChange);
            GameObjectField(t._("legacy.editor.gameObjectField.clothes"), ref _targetClothes, true, TargetAvatarOrWearableChange);

            BeginHorizontal();
            {
                BeginDisabled(_targetClothes == null);
                {
                    TextField(t._("legacy.editor.textField.newClothesName"), ref _newClothesName);
                    Button(t._("legacy.editor.btn.renameNewClothesName"), RenameClothesNameButtonClick, GUILayout.ExpandWidth(false));
                }
                EndDisabled();
            }
            EndHorizontal();

            ToggleLeft(t._("legacy.editor.toggle.useCustomArmatureNames"), ref _useCustomArmatureNames, ConfigChange);
            BeginDisabled(!_useCustomArmatureNames);
            {
                EditorGUI.indentLevel += 1;
                if (ShowHasCabinetHelpbox)
                {
                    HelpBox(t._("legacy.editor.helpbox.avatarAttachedToCabinet"), MessageType.Info);
                }
                BeginDisabled(ShowHasCabinetHelpbox);
                {
                    DelayedTextField(t._("legacy.editor.textField.avatarArmatureName"), ref _avatarArmatureObjectName, ConfigChange);
                }
                EndDisabled();
                DelayedTextField(t._("legacy.editor.textField.clothesArmatureName"), ref _clothesArmatureObjectName, ConfigChange);
                EditorGUI.indentLevel -= 1;
            }
            EndDisabled();
        }

        private void DrawSimple()
        {
            DrawCommonClothesConfig();

            // simple mode defaults
            _groupDynamics = true;
            _groupDynamicsSeparateGameObjects = true;
            _dynamicsOption = 0;

            Separator();

            Label(t._("legacy.editor.label.dynamicsAutoGroupedHandledInSimpleMode"));
        }

        private void DrawAdvanced()
        {
            DrawCommonClothesConfig();

            HorizontalLine();

            Label(t._("legacy.editor.label.groupingBonesAndDynamics"), EditorStyles.boldLabel);

            Separator();

            ToggleLeft(t._("legacy.editor.toggle.groupBones"), ref _groupBones, ConfigChange);
            if (ShowHasCabinetHelpbox)
            {
                HelpBox(t._("legacy.editor.helpbox.avatarAttachedToCabinet"), MessageType.Info);
            }
            BeginDisabled(ShowHasCabinetHelpbox);
            {
                ToggleLeft(t._("legacy.editor.toggle.groupDynamics"), ref _groupDynamics, ConfigChange);
                BeginDisabled(!_groupDynamics);
                {
                    EditorGUI.indentLevel += 1;
                    ToggleLeft(t._("legacy.editor.toggle.groupDynamicsSeparateGameObjects"), ref _groupDynamicsSeparateGameObjects, ConfigChange);
                    EditorGUI.indentLevel -= 1;
                }
                EndDisabled();
            }
            EndDisabled();

            HorizontalLine();

            Label(t._("legacy.editor.label.prefixesAndSuffixes"), EditorStyles.boldLabel);
            HelpBox(t._("legacy.editor.label.prefixesAndSuffixesAutoGenAndHandled"), MessageType.Info);

            ToggleLeft(t._("legacy.editor.toggle.removeExistingPrefixesAndSuffixes"), ref _removeExistingPrefixSuffix, ConfigChange);

            HorizontalLine();

            Label(t._("legacy.editor.label.dynamicsHandling"), EditorStyles.boldLabel);

            Separator();

            Popup(ref _dynamicsOption, new string[] {
                t._("legacy.editor.popup.dynamicsOption.removeDynamicsAndAddParentConstraint"),
                t._("legacy.editor.popup.dynamicsOption.keepDynamicsAndAddParentConstraintIfNeeded"),
                t._("legacy.editor.popup.dynamicsOption.removeDynamicsAndAddIgnoreTransform"),
                t._("legacy.editor.popup.dynamicsOption.copyAvatarDynamicsData"),
                t._("legacy.editor.popup.dynamicsOption.ignoreAllDynamics")
            });
        }

        private void DrawContent()
        {
            Toolbar(ref _selectedMode, new string[] { t._("legacy.editor.popup.mode.simple"), t._("legacy.editor.popup.mode.advanced") });

            HorizontalLine();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (_selectedMode == 0)
            {
                DrawSimple();
            }
            else
            {
                DrawAdvanced();
            }

            HorizontalLine();

            Label(t._("legacy.editor.label.checkAndDress"), EditorStyles.boldLabel);

            DrawReportResult();

            var checkBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16
            };
            var noReportOrHasErrors = _reportData == null || _reportData.errorMsgs.Count > 0;
            BeginHorizontal();
            {
                BeginDisabled(_targetClothes == null || _targetClothes.name == "");
                {
                    Button(t._("legacy.editor.btn.checkAndPreview"), checkBtnStyle, CheckAndPreviewButtonClick, GUILayout.Height(40));
                }
                EndDisabled();

                BeginDisabled(noReportOrHasErrors);
                {
                    Button(t._("legacy.editor.btn.testNow"), checkBtnStyle, TestNowButtonClick, GUILayout.Height(40));
                }
                EndDisabled();
            }
            EndHorizontal();

            BeginDisabled(noReportOrHasErrors);
            {
                Button(t._("legacy.editor.btn.dressNow"), checkBtnStyle, DressNowButtonClick, GUILayout.Height(40));
            }
            EndDisabled();

            Separator();

            DrawReportDetails();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            Label(t._("legacy.editor.label.legacyEditor"));
            DTLogo.Show();
            Separator();
            HelpBox(t._("legacy.editor.helpbox.legacyEditorWarning"), MessageType.Warning);
            HorizontalLine();
        }

        private void DrawFooter()
        {
            HorizontalLine();
            Label(t._("legacy.editor.label.toolVersion", CurrentVersion?.fullVersionString));
            // TODO: version checker
            EditorGUILayout.SelectableLabel("https://dressingtools.chocopoi.com");
        }

        public override void OnGUI()
        {
            DrawHeader();
            if (Application.isPlaying)
            {
                HelpBox(t._("legacy.editor.label.exitPlayMode"), MessageType.Warning);
            }
            else
            {
                DrawContent();
            }
            DrawFooter();
        }
    }
}
