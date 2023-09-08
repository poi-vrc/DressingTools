/*
 * File: WearableConfigView.cs
 * Project: DressingTools
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
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
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class WearableConfigView : IMGUIViewBase, IWearableConfigView
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

        public event Action TargetAvatarOrWearableChange { add { _viewParent.TargetAvatarOrWearableChange += value; } remove { _viewParent.TargetAvatarOrWearableChange -= value; } }
        public event Action TargetAvatarConfigChange;
        public event Action MetaInfoChange;
        public event Action AddModuleButtonClick;

        public string[] AvailableModuleKeys { get; set; }
        public int SelectedAvailableModule { get => _selectedAvailableModule; set => _selectedAvailableModule = value; }
        public GameObject TargetAvatar { get => _viewParent.TargetAvatar; }
        public GameObject TargetWearable { get => _viewParent.TargetWearable; }
        public WearableConfig Config { get => _viewParent.Config; }
        public List<ModuleData> ModuleDataList { get; set; }
        public bool ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox { get; set; }
        public bool IsInvalidAvatarPrefabGuid { get; set; }
        public string AvatarPrefabGuid { get; set; }
        public GameObject GuidReferencePrefab { get => _guidReferencePrefab; set => _guidReferencePrefab = value; }
        public bool TargetAvatarConfigUseAvatarObjectName { get => _targetAvatarConfigUseAvatarObjectName; set => _targetAvatarConfigUseAvatarObjectName = value; }
        public string TargetAvatarConfigAvatarName { get => _targetAvatarConfigAvatarName; set => _targetAvatarConfigAvatarName = value; }
        public string TargetAvatarConfigArmatureName { get; set; }
        public string TargetAvatarConfigWorldPosition { get; set; }
        public string TargetAvatarConfigWorldRotation { get; set; }
        public string TargetAvatarConfigWorldAvatarLossyScale { get; set; }
        public string TargetAvatarConfigWorldWearableLossyScale { get; set; }
        public string ConfigUuid { get; set; }
        public bool MetaInfoUseWearableObjectName { get => _metaInfoUseWearableObjectName; set => _metaInfoUseWearableObjectName = value; }
        public string MetaInfoWearableName { get => _metaInfoWearableName; set => _metaInfoWearableName = value; }
        public string MetaInfoAuthor { get => _metaInfoAuthor; set => _metaInfoAuthor = value; }
        public string MetaInfoCreatedTime { get; set; }
        public string MetaInfoUpdatedTime { get; set; }
        public string MetaInfoDescription { get => _metaInfoDescription; set => _metaInfoDescription = value; }

        private WearableConfigPresenter _presenter;
        private IWearableConfigViewParent _viewParent;
        private int _selectedAvailableModule;
        private bool _foldoutMetaInfo;
        private bool _foldoutTargetAvatarConfigs;
        private GameObject _guidReferencePrefab;
        private bool _targetAvatarConfigUseAvatarObjectName;
        private string _targetAvatarConfigAvatarName;
        private bool _metaInfoUseWearableObjectName;
        private string _metaInfoWearableName;
        private string _metaInfoAuthor;
        private string _metaInfoDescription;

        public WearableConfigView(IWearableConfigViewParent viewParent)
        {
            _viewParent = viewParent;
            _presenter = new WearableConfigPresenter(this);

            AvailableModuleKeys = new string[0];
            ModuleDataList = new List<ModuleData>();
            ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox = true;
            IsInvalidAvatarPrefabGuid = true;
            AvatarPrefabGuid = null;
            TargetAvatarConfigArmatureName = null;
            TargetAvatarConfigWorldPosition = null;
            TargetAvatarConfigWorldRotation = null;
            TargetAvatarConfigWorldAvatarLossyScale = null;
            TargetAvatarConfigWorldWearableLossyScale = null;
            ConfigUuid = null;
            MetaInfoCreatedTime = null;
            MetaInfoUpdatedTime = null;

            _selectedAvailableModule = 0;
            _foldoutMetaInfo = false;
            _foldoutTargetAvatarConfigs = false;
            _guidReferencePrefab = null;
            _targetAvatarConfigUseAvatarObjectName = false;
            _targetAvatarConfigAvatarName = null;
            _metaInfoUseWearableObjectName = false;
            _metaInfoWearableName = null;
            _metaInfoAuthor = null;
            _metaInfoDescription = null;
        }

        public void ShowModuleAddedBeforeDialog()
        {
            EditorUtility.DisplayDialog(t._("tool.name"), t._("dressing.wearableConfig.dialog.msg.moduleAddedBeforeCannotMultiple"), t._("common.dialog.btn.ok"));
        }

        private void DrawModulesGUI()
        {
            BeginHorizontal();
            {
                Popup(t._("dressing.wearableConfig.popup.selectModule"), ref _selectedAvailableModule, AvailableModuleKeys);
                Button(t._("dressing.wearableConfig.btn.add"), AddModuleButtonClick, GUILayout.ExpandWidth(false));
            }
            EndHorizontal();

            var copy = new List<ModuleData>(ModuleDataList);
            foreach (var moduleData in copy)
            {
                BeginFoldoutBoxWithButtonRight(ref moduleData.editor.foldout, moduleData.editor.FriendlyName, t._("dressing.wearableConfig.btn.remove"), moduleData.removeButtonOnClickEvent);
                if (moduleData.editor.foldout)
                {
                    moduleData.editor.OnGUI();
                }
                EndFoldoutBox();
            }
        }

        private void DrawAvatarConfigsGUI()
        {
            BeginFoldoutBox(ref _foldoutTargetAvatarConfigs, t._("dressing.wearableConfig.foldout.avatarConfig"));
            if (_foldoutTargetAvatarConfigs)
            {
                if (ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox)
                {
                    HelpBox(t._("dressing.wearableConfig.helpbox.noTargetOrWearableSelected"), MessageType.Error);
                }
                else
                {
                    HelpBox(t._("dressing.wearableConfig.helpbox.guidTip"), MessageType.Info);

                    if (IsInvalidAvatarPrefabGuid)
                    {
                        HelpBox(t._("dressing.wearableConfig.helpbox.unableToFindGuidFromUnpackedAvatar"), MessageType.Warning);
                    }
                    GameObjectField(t._("dressing.wearableConfig.gameObjectField.guidRefPrefab"), ref _guidReferencePrefab, true, TargetAvatarConfigChange);

                    ReadOnlyTextField(t._("dressing.wearableConfig.readOnlyTextField.guid"), IsInvalidAvatarPrefabGuid ? "(Not available)" : AvatarPrefabGuid);

                    ToggleLeft(t._("dressing.wearableConfig.toggle.useAvatarObjectName"), ref _targetAvatarConfigUseAvatarObjectName, TargetAvatarConfigChange);
                    BeginDisabled(_targetAvatarConfigUseAvatarObjectName);
                    {
                        DelayedTextField(t._("dressing.wearableConfig.textField.customAvatarObjectName"), ref _targetAvatarConfigAvatarName, TargetAvatarConfigChange);
                    }
                    EndDisabled();

                    ReadOnlyTextField(t._("dressing.wearableConfig.readOnlyTextField.armatureName"), TargetAvatarConfigArmatureName);
                    ReadOnlyTextField(t._("dressing.wearableConfig.readOnlyTextField.deltaWorldPos"), TargetAvatarConfigWorldPosition);
                    ReadOnlyTextField(t._("dressing.wearableConfig.readOnlyTextField.deltaWorldRot"), TargetAvatarConfigWorldRotation);
                    ReadOnlyTextField(t._("dressing.wearableConfig.readOnlyTextField.avatarLossyScale"), TargetAvatarConfigWorldAvatarLossyScale);
                    ReadOnlyTextField(t._("dressing.wearableConfig.readOnlyTextField.wearableLossyScale"), TargetAvatarConfigWorldAvatarLossyScale);

                    // TODO: multiple GUIDs
                    // HelpBox("If you modified the FBX or created the prefab on your own, the GUID will be unlikely the original one. If that is the case, please create a new avatar configuration and drag the original prefab here.", MessageType.Info);
                }
            }
            EndFoldoutBox();
        }

        private void DrawMetaInfoGUI()
        {
            BeginFoldoutBox(ref _foldoutMetaInfo, t._("dressing.wearableConfig.foldout.metaInfo"));
            if (_foldoutMetaInfo)
            {
                ReadOnlyTextField(t._("dressing.wearableConfig.readOnlyTextField.uuid"), ConfigUuid);

                ToggleLeft(t._("dressing.wearableConfig.toggle.useWearableObjectName"), ref _metaInfoUseWearableObjectName, MetaInfoChange);
                BeginDisabled(_metaInfoUseWearableObjectName);
                {
                    DelayedTextField(t._("dressing.wearableConfig.textField.customWearableObjectName"), ref _metaInfoWearableName, MetaInfoChange);
                }
                EndDisabled();
                DelayedTextField(t._("dressing.wearableConfig.textField.author"), ref _metaInfoAuthor, MetaInfoChange);

                ReadOnlyTextField(t._("dressing.wearableConfig.readOnlyTextField.createdTime"), MetaInfoCreatedTime);
                ReadOnlyTextField(t._("dressing.wearableConfig.readOnlyTextField.updatedTime"), MetaInfoUpdatedTime);

                Label(t._("dressing.wearableConfig.label.description"));
                TextArea(ref _metaInfoDescription, MetaInfoChange);
            }
            EndFoldoutBox();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            foreach (var moduleData in ModuleDataList)
            {
                moduleData.editor.OnEnable();
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            foreach (var moduleData in ModuleDataList)
            {
                moduleData.editor.OnDisable();
            }
        }

        public override void OnGUI()
        {
            DrawModulesGUI();
            DrawAvatarConfigsGUI();
            DrawMetaInfoGUI();
        }

        public bool IsValid() => _presenter.IsValid();

        public void UpdateAvatarPreview()
        {
            throw new NotImplementedException();
        }
    }
}
