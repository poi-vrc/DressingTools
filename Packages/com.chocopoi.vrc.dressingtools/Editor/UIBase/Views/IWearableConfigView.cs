using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.UI.Views.Modules;
using Chocopoi.DressingTools.Wearable;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal class ModuleData
    {
        public ModuleEditor editor;
        public Action removeButtonOnClickEvent;
    }

    internal interface IWearableConfigView : IEditorView, IModuleEditorViewParent
    {
        event Action TargetAvatarConfigChange;
        event Action MetaInfoChange;
        event Action AddModuleButtonClick;

        string[] AvailableModuleKeys { get; set; }
        int SelectedAvailableModule { get; set; }
        DTWearableConfig Config { get; }
        List<ModuleData> ModuleDataList { get; set; }
        bool ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox { get; set; }
        bool IsInvalidAvatarPrefabGuid { get; set; }
        string AvatarPrefabGuid { get; set; }
        GameObject GuidReferencePrefab { get; set; }
        bool TargetAvatarConfigUseAvatarObjectName { get; set; }
        string TargetAvatarConfigAvatarName { get; set; }
        string TargetAvatarConfigArmatureName { get; set; }
        string TargetAvatarConfigWorldPosition { get; set; }
        string TargetAvatarConfigWorldRotation { get; set; }
        string TargetAvatarConfigWorldAvatarLossyScale { get; set; }
        string TargetAvatarConfigWorldWearableLossyScale { get; set; }
        string ConfigUuid { get; set; }
        bool MetaInfoUseWearableObjectName { get; set; }
        string MetaInfoWearableName { get; set; }
        string MetaInfoAuthor { get; set; }
        string MetaInfoCreatedTime { get; set; }
        string MetaInfoUpdatedTime { get; set; }
        string MetaInfoDescription { get; set; }
    }
}
