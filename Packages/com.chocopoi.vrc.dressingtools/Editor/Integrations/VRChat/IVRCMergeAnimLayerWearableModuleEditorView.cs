/*
 * File: IVRCMergeAnimLayerWearableModuleEditorView.cs
 * Project: DressingTools
 * Created Date: Tuesday, 29th Aug 2023, 02:53:11 pm
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

#if VRC_SDK_VRCSDK3
using System;
using Chocopoi.DressingFramework.UI;
using UnityEngine;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal interface IVRCMergeAnimLayerWearableModuleEditorView : IEditorView
    {
        event Action ConfigChange;

        string[] AnimLayerKeys { get; set; }
        int SelectedAnimLayerIndex { get; set; }
        int SelectedPathMode { get; set; }
        bool RemoveAnimatorAfterApply { get; set; }
        bool MatchLayerWriteDefaults { get; set; }
        GameObject AnimatorObject { get; set; }
        bool ShowNoTargetAvatarOrWearableHelpbox { get; set; }
        bool ShowAnimatorObjectPathNotFoundHelpbox { get; set; }
        bool ShowNoAnimatorHelpbox { get; set; }
        bool ShowNotInWearableHelpbox { get; set; }
    }
}
#endif
