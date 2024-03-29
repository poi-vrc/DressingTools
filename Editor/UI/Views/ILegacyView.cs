/*
 * File: ILegacyView.cs
 * Project: DressingTools
 * Created Date: Wednesday, September 10th 2023, 2:45:04 pm
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
using Chocopoi.DressingTools.UI.Views.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal interface ILegacyView : IEditorView
    {
        event Action TargetAvatarOrWearableChange;
        event Action RenameClothesNameButtonClick;
        event Action ConfigChange;
        event Action CheckAndPreviewButtonClick;
        event Action TestNowButtonClick;
        event Action DressNowButtonClick;

        UpdateChecker.ParsedVersion CurrentVersion { get; set; }
        GameObject TargetAvatar { get; set; }
        GameObject TargetClothes { get; set; }
        string NewClothesName { get; set; }
        bool UseCustomArmatureName { get; set; }
        string AvatarArmatureObjectName { get; set; }
        string ClothesArmatureObjectName { get; set; }
        bool GroupBones { get; set; }
        bool GroupDynamics { get; set; }
        bool GroupDynamicsSeparateGameObjects { get; set; }
        string Prefix { get; set; }
        string Suffix { get; set; }
        bool PreventDuplicateNames { get; set; }
        int DynamicsOption { get; set; }
        ReportData ReportData { get; set; }

        bool ShowExistingWearableConfigIgnoreConfirmDialog();
        bool ShowDressConfirmDialog();
        void ShowCompletedDialog();
    }
}
