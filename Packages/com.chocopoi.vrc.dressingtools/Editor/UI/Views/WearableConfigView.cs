using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class WearableConfigView : EditorViewBase, IWearableConfigView
    {
        private static readonly I18n t = I18n.GetInstance();

        public event Action ForceUpdateView;
        public event Action TargetAvatarOrWearableChange { add { viewParent_.TargetAvatarOrWearableChange += value; } remove { viewParent_.TargetAvatarOrWearableChange -= value; } }
        public event Action TargetAvatarConfigChange;
        public event Action MetaInfoChange;
        public event Action AddModuleButtonClick;

        public string[] AvailableModuleKeys { get; set; }
        public int SelectedAvailableModule { get => selectedAvailableModule_; set => selectedAvailableModule_ = value; }
        public GameObject TargetAvatar { get => viewParent_.TargetAvatar; }
        public GameObject TargetWearable { get => viewParent_.TargetWearable; }
        public DTWearableConfig Config { get => viewParent_.Config; }
        public List<ModuleData> ModuleDataList { get; set; }
        public bool ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox { get; set; }
        public bool IsInvalidAvatarPrefabGuid { get; set; }
        public string AvatarPrefabGuid { get; set; }
        public GameObject GuidReferencePrefab { get => guidReferencePrefab_; set => guidReferencePrefab_ = value; }
        public bool TargetAvatarConfigUseAvatarObjectName { get => targetAvatarConfigUseAvatarObjectName_; set => targetAvatarConfigUseAvatarObjectName_ = value; }
        public string TargetAvatarConfigAvatarName { get => targetAvatarConfigAvatarName_; set => targetAvatarConfigAvatarName_ = value; }
        public string TargetAvatarConfigArmatureName { get; set; }
        public string TargetAvatarConfigWorldPosition { get; set; }
        public string TargetAvatarConfigWorldRotation { get; set; }
        public string TargetAvatarConfigWorldAvatarLossyScale { get; set; }
        public string TargetAvatarConfigWorldWearableLossyScale { get; set; }
        public string ConfigUuid { get; set; }
        public bool MetaInfoUseWearableObjectName { get => metaInfoUseWearableObjectName_; set => metaInfoUseWearableObjectName_ = value; }
        public string MetaInfoWearableName { get => metaInfoWearableName_; set => metaInfoWearableName_ = value; }
        public string MetaInfoAuthor { get => metaInfoAuthor_; set => metaInfoAuthor_ = value; }
        public string MetaInfoCreatedTime { get; set; }
        public string MetaInfoUpdatedTime { get; set; }
        public string MetaInfoDescription { get => metaInfoDescription_; set => metaInfoDescription_ = value; }

        private WearableConfigPresenter presenter_;
        private IWearableConfigViewParent viewParent_;
        private int selectedAvailableModule_;
        private bool foldoutMetaInfo_;
        private bool foldoutTargetAvatarConfigs_;
        private GameObject guidReferencePrefab_;
        private bool targetAvatarConfigUseAvatarObjectName_;
        private string targetAvatarConfigAvatarName_;
        private bool metaInfoUseWearableObjectName_;
        private string metaInfoWearableName_;
        private string metaInfoAuthor_;
        private string metaInfoDescription_;

        public WearableConfigView(IWearableConfigViewParent viewParent)
        {
            viewParent_ = viewParent;
            presenter_ = new WearableConfigPresenter(this);

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

            selectedAvailableModule_ = 0;
            foldoutMetaInfo_ = false;
            foldoutTargetAvatarConfigs_ = false;
            guidReferencePrefab_ = null;
            targetAvatarConfigUseAvatarObjectName_ = false;
            targetAvatarConfigAvatarName_ = null;
            metaInfoUseWearableObjectName_ = false;
            metaInfoWearableName_ = null;
            metaInfoAuthor_ = null;
            metaInfoDescription_ = null;
        }

        public void RaiseForceUpdateView()
        {
            ForceUpdateView?.Invoke();
        }

        private void DrawModulesGUI()
        {
            BeginHorizontal();
            {
                Popup("Select Module:", ref selectedAvailableModule_, AvailableModuleKeys);
                Button("Add", AddModuleButtonClick, GUILayout.ExpandWidth(false));
            }
            EndHorizontal();

            var copy = new List<ModuleData>(ModuleDataList);
            foreach (var moduleData in copy)
            {
                BeginFoldoutBoxWithButtonRight(ref moduleData.editor.foldout, moduleData.editor.FriendlyName, "x Remove", moduleData.removeButtonOnClickEvent);
                if (moduleData.editor.foldout)
                {
                    moduleData.editor.OnGUI();
                }
                EndFoldoutBox();
            }
        }

        private void DrawAvatarConfigsGUI()
        {
            BeginFoldoutBox(ref foldoutTargetAvatarConfigs_, "Target Avatar Configuration");
            if (foldoutTargetAvatarConfigs_)
            {
                HelpBox("This allows other users to be able to find your configuration for their avatars and wearables once uploaded.", MessageType.Info);

                if (ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox)
                {
                    HelpBox("Target avatar and wearable cannot be empty to access this editor.", MessageType.Error);
                }
                else
                {
                    if (IsInvalidAvatarPrefabGuid)
                    {
                        HelpBox("Your avatar is unpacked and the GUID cannot be found automatically. To help other online users to find your configuration, drag your avatar original unpacked prefab here to get a GUID.", MessageType.Warning);
                    }
                    GameObjectField("GUID Reference Prefab", ref guidReferencePrefab_, true, TargetAvatarConfigChange);

                    ReadOnlyTextField("GUID", IsInvalidAvatarPrefabGuid ? "(Not available)" : AvatarPrefabGuid);

                    ToggleLeft("Use avatar object's name", ref targetAvatarConfigUseAvatarObjectName_, TargetAvatarConfigChange);
                    BeginDisabled(targetAvatarConfigUseAvatarObjectName_);
                    {
                        DelayedTextField("Name", ref targetAvatarConfigAvatarName_, TargetAvatarConfigChange);
                    }
                    EndDisabled();

                    ReadOnlyTextField("Armature Name", TargetAvatarConfigArmatureName);
                    ReadOnlyTextField("Delta World Position", TargetAvatarConfigWorldPosition);
                    ReadOnlyTextField("Delta World Rotation", TargetAvatarConfigWorldRotation);
                    ReadOnlyTextField("Avatar Lossy Scale", TargetAvatarConfigWorldAvatarLossyScale);
                    ReadOnlyTextField("Wearable Lossy Scale", TargetAvatarConfigWorldAvatarLossyScale);

                    HelpBox("If you modified the FBX or created the prefab on your own, the GUID will be unlikely the original one. If that is the case, please create a new avatar configuration and drag the original prefab here.", MessageType.Info);
                }
            }
            EndFoldoutBox();
        }

        private void DrawMetaInfoGUI()
        {
            BeginFoldoutBox(ref foldoutMetaInfo_, "Meta Information");
            if (foldoutMetaInfo_)
            {
                ReadOnlyTextField("UUID", ConfigUuid);

                ToggleLeft("Use wearable object's name", ref metaInfoUseWearableObjectName_, MetaInfoChange);
                BeginDisabled(metaInfoUseWearableObjectName_);
                {
                    DelayedTextField("Name", ref metaInfoWearableName_, MetaInfoChange);
                }
                EndDisabled();
                DelayedTextField("Author", ref metaInfoAuthor_, MetaInfoChange);

                ReadOnlyTextField("Created Time", MetaInfoCreatedTime);
                ReadOnlyTextField("Updated Time", MetaInfoUpdatedTime);

                Label("Description");
                TextArea(ref metaInfoDescription_, MetaInfoChange);
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

        public bool IsValid() => presenter_.IsValid();
    }
}
