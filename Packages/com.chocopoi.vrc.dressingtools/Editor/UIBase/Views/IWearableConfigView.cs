﻿/*
 * File: IWearableConfigView.cs
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
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal class ModuleData
    {
        public WearableModuleEditor editor;
        public Action removeButtonOnClickEvent;
    }

    internal interface IWearableConfigView : IEditorView, IWearableModuleEditorViewParent
    {
        event Action TargetAvatarConfigChange;
        event Action MetaInfoChange;
        event Action AddModuleButtonClick;

        string[] AvailableModuleKeys { get; set; }
        int SelectedAvailableModule { get; set; }
        WearableConfig Config { get; }
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
