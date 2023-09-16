/*
 * File: IWearableConfigView.cs
 * Project: DressingTools
 * Created Date: Wednesday, September 13th 2023, 9:36:26 pm
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General License for more details.
 * 
 * You should have received a copy of the GNU General License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal class WearableConfigModuleViewData
    {
        public WearableModuleEditor editor;
        public Action removeButtonOnClick;
    }

    internal interface IWearableConfigView : IEditorView, IWearableModuleEditorViewParent
    {
        event Action InfoNewThumbnailButtonClick;
        event Action CaptureThumbnailButtonClick;
        event Action CaptureCancelButtonClick;
        event Action CaptureSettingsChange;
        event Action ToolbarAutoSetupButtonClick;
        event Action ToolbarPreviewButtonClick;
        event Action AdvancedModuleAddButtonClick;
        event Action ModeChange;

        WearableConfig Config { get; set; }

        int SelectedMode { get; set; }

        Texture2D InfoThumbnail { get; set; }
        bool InfoUseCustomWearableName { get; set; }
        string InfoCustomWearableName { get; set; }
        string InfoUuid { get; set; }
        string InfoCreatedTime { get; set; }
        string InfoUpdatedTime { get; set; }
        string InfoAuthor { get; set; }
        string InfoDescription { get; set; }
        bool CaptureWearableOnly { get; set; }
        bool CaptureRemoveBackground { get; set; }

        bool SimpleUseArmatureMapping { get; set; }
        bool SimpleUseMoveRoot { get; set; }
        bool SimpleUseAnimationGeneration { get; set; }
        bool SimpleUseBlendshapeSync { get; set; }
        ArmatureMappingWearableModuleConfig SimpleArmatureMappingConfig { get; set; }
        MoveRootWearableModuleConfig SimpleMoveRootConfig { get; set; }
        AnimationGenerationWearableModuleConfig SimpleAnimationGenerationConfig { get; set; }
        BlendshapeSyncWearableModuleConfig SimpleBlendshapeSyncConfig { get; set; }

        List<string> AdvancedModuleNames { get; set; }
        GameObject AdvancedAvatarConfigGuidReference { get; set; }
        string AdvancedAvatarConfigGuid { get; set; }
        bool AdvancedAvatarConfigUseAvatarObjName { get; set; }
        string AdvancedAvatarConfigCustomName { get; set; }
        string AdvancedAvatarConfigArmatureName { get; set; }
        string AdvancedAvatarConfigDeltaWorldPos { get; set; }
        string AdvancedAvatarConfigDeltaWorldRot { get; set; }
        string AdvancedAvatarConfigAvatarLossyScale { get; set; }
        string AdvancedAvatarConfigWearableLossyScale { get; set; }
        string AdvancedSelectedModuleName { get; set; }
        List<WearableConfigModuleViewData> AdvancedModuleViewDataList { get; set; }

        bool ShowAvatarNoCabinetHelpBox { get; set; }
        bool ShowArmatureNotFoundHelpBox { get; set; }
        bool ShowArmatureGuessedHelpBox { get; set; }
        bool ShowCabinetConfigErrorHelpBox { get; set; }

        bool PreviewActive { get; }

        void Repaint();
        void RepaintSimpleMode();
        void RepaintAdvancedModeModules();
        void RepaintCapturePreview();
        void SwitchToInfoPanel();
        void SwitchToCapturePanel();
        void ShowModuleAddedBeforeDialog();
        bool ShowConfirmAutoSetupDialog();
    }
}
