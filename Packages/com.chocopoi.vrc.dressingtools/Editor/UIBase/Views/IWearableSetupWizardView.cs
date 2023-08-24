/*
 * File: IWearableSetupWizardView.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 1:22:09 am
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
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UI.Views.Modules;
using Chocopoi.DressingTools.Wearable.Modules;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal interface IWearableSetupWizardView : IEditorView, IWearableModuleEditorViewParent
    {
        event Action PreviousButtonClick;
        event Action NextButtonClick;

        WearableConfig Config { get; set; }
        ArmatureMappingWearableModuleConfig ArmatureMappingModuleConfig { get; set; }
        MoveRootWearableModuleConfig MoveRootModuleConfig { get; set; }
        AnimationGenerationWearableModuleConfig AnimationGenerationModuleConfig { get; set; }
        BlendshapeSyncWearableModuleConfig BlendshapeSyncModuleConfig { get; set; }
        ArmatureMappingWearableModuleEditor ArmatureMappingModuleEditor { get; set; }
        MoveRootWearableModuleEditor MoveRootModuleEditor { get; set; }
        AnimationGenerationWearableModuleEditor AnimationGenerationModuleEditor { get; set; }
        BlendshapeSyncWearableModuleEditor BlendshapeSyncModuleEditor { get; set; }
        bool UseArmatureMapping { get; set; }
        bool UseMoveRoot { get; set; }
        bool UseAnimationGeneration { get; set; }
        bool UseBlendshapeSync { get; set; }
        int CurrentStep { get; set; }
        bool ShowAvatarNoCabinetHelpBox { get; set; }
        bool ShowArmatureNotFoundHelpBox { get; set; }
        bool ShowArmatureGuessedHelpBox { get; set; }
        bool ShowCabinetConfigErrorHelpBox { get; set; }

        void GenerateConfig();
        void RaiseDoAddToCabinetEvent();
        bool IsValid();
    }
}
