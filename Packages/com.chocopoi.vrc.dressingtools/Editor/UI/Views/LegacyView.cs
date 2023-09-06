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
        public bool DressNowConfirm { get => _dressNowConfirm; set => _dressNowConfirm = value; }
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
        private bool _dressNowConfirm;

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
            _dressNowConfirm = false;
        }

        private void DrawReportResult()
        {
            if (_reportData == null)
            {
                HelpBox("No dress report generated.", MessageType.Warning);
                return;
            }

            //Result

            if (_reportData.errorMsgs.Count > 0)
            {
                HelpBox("Error(s) occurred during bone mappings generation.", MessageType.Error);
            }
            else if (_reportData.warnMsgs.Count > 0)
            {
                HelpBox("Warning(s) occurred during bone mappings generation.", MessageType.Warning);
            }
            else
            {
                HelpBox("Bone mappings generation is successful.", MessageType.Info);
            }
        }

        private void DrawReportDetails()
        {
            if (_reportData == null)
            {
                return;
            }

            BeginFoldoutBox(ref _foldoutReportLogEntries, "Logs");
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
            Label("Setup", EditorStyles.boldLabel);
            HelpBox("Move clothes into place", MessageType.Info);

            GameObjectField("Avatar", ref _targetAvatar, true, TargetAvatarOrWearableChange);
            GameObjectField("Clothes", ref _targetClothes, true, TargetAvatarOrWearableChange);

            BeginHorizontal();
            {
                BeginDisabled(_targetClothes == null);
                {
                    TextField("New Clothes", ref _newClothesName);
                    Button("Rename", RenameClothesNameButtonClick, GUILayout.ExpandWidth(false));
                }
                EndDisabled();
            }
            EndHorizontal();

            ToggleLeft("Use custom armature names", ref _useCustomArmatureNames, ConfigChange);
            BeginDisabled(!_useCustomArmatureNames);
            {
                EditorGUI.indentLevel += 1;
                if (ShowHasCabinetHelpbox)
                {
                    HelpBox("The avatar is attached with a cabinet. Change this settings from the cabinet settings.", MessageType.Info);
                }
                BeginDisabled(ShowHasCabinetHelpbox);
                {
                    DelayedTextField("Avatar Armature Name", ref _avatarArmatureObjectName, ConfigChange);
                }
                EndDisabled();
                DelayedTextField("Clothes Armature Name", ref _clothesArmatureObjectName, ConfigChange);
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

            Label("Dynamics are automatically grouped and handled in simple mode.");
        }

        private void DrawAdvanced()
        {
            DrawCommonClothesConfig();

            HorizontalLine();

            Label("Grouping bones and dynamics", EditorStyles.boldLabel);

            Separator();

            ToggleLeft("Group bones", ref _groupBones, ConfigChange);
            if (ShowHasCabinetHelpbox)
            {
                HelpBox("The avatar is attached with a cabinet. Change this settings from the cabinet settings.", MessageType.Info);
            }
            BeginDisabled(ShowHasCabinetHelpbox);
            {
                ToggleLeft("Group dynamics", ref _groupDynamics, ConfigChange);
                BeginDisabled(!_groupDynamics);
                {
                    EditorGUI.indentLevel += 1;
                    ToggleLeft("Separate dynamics into different objects", ref _groupDynamicsSeparateGameObjects, ConfigChange);
                    EditorGUI.indentLevel -= 1;
                }
                EndDisabled();
            }
            EndDisabled();

            HorizontalLine();

            Label("Prefixes and suffixes", EditorStyles.boldLabel);
            HelpBox("Prefixes and suffixes are automatically generated and handled internally in v2.", MessageType.Info);

            ToggleLeft("Remove existing prefixes and suffixes", ref _removeExistingPrefixSuffix, ConfigChange);

            HorizontalLine();

            Label("Dynamics handling", EditorStyles.boldLabel);

            Separator();

            Popup(ref _dynamicsOption, new string[] {
                "Remove wearable dynamics and ParentConstraint",
                "Keep wearable dynamics and ParentConstraint if needed",
                "Remove wearable dynamics and IgnoreTransform",
                "Copy avatar dynamics data to wearable",
                "Ignore all dynamics"
            });
        }

        private void DrawContent()
        {
            Toolbar(ref _selectedMode, new string[] { "Simple", "Advanced" });

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

            Label("Check and Dress", EditorStyles.boldLabel);

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
                    Button("Check and Preview", checkBtnStyle, CheckAndPreviewButtonClick, GUILayout.Height(40));
                }
                EndDisabled();

                BeginDisabled(noReportOrHasErrors);
                {
                    Button("Test Now", checkBtnStyle, TestNowButtonClick, GUILayout.Height(40));
                }
                EndDisabled();
            }
            EndHorizontal();

            BeginDisabled(noReportOrHasErrors);
            {
                ToggleLeft("I have confirmed", ref _dressNowConfirm);
            }
            EndDisabled();

            BeginDisabled(noReportOrHasErrors || !_dressNowConfirm);
            {
                Button("Dress Now", checkBtnStyle, DressNowButtonClick, GUILayout.Height(40));
            }
            EndDisabled();

            Separator();

            DrawReportDetails();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            Label("Legacy Editor");
            DTLogo.Show();
            Separator();
            HelpBox("This legacy editor lacks many v2 features. You should use the new editor instead.", MessageType.Warning);
            HorizontalLine();
        }

        private void DrawFooter()
        {
            HorizontalLine();
            Label("Version: " + CurrentVersion?.fullVersionString);
            // TODO: version checker
            EditorGUILayout.SelectableLabel("https://dressingtools.chocopoi.com");
        }

        public override void OnGUI()
        {
            DrawHeader();
            if (Application.isPlaying)
            {
                HelpBox("Please exit play mode.", MessageType.Warning);
            }
            else
            {
                DrawContent();
            }
            DrawFooter();
        }
    }
}
