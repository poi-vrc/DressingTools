/*
 * File: IAnimationGenerationModuleEditorView.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 9th 2023, 8:34:36 pm
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
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal class ToggleData
    {
        public bool isInvalid;
        public GameObject gameObject;
        public bool state;
        public Action changeEvent;
        public Action removeButtonClickEvent;

        public ToggleData()
        {
            isInvalid = true;
            gameObject = null;
            state = false;
        }
    }

    internal class ToggleSuggestionData
    {
        public GameObject gameObject;
        public bool state;
        public Action addButtonClickEvent;

        public ToggleSuggestionData()
        {
            gameObject = null;
            state = false;
        }
    }

    internal class BlendshapeData
    {
        public bool isInvalid;
        public GameObject gameObject;
        public string[] availableBlendshapeNames;
        public int selectedBlendshapeIndex;
        public float value;
        public Action gameObjectFieldChangeEvent;
        public Action blendshapeNameChangeEvent;
        public Action sliderChangeEvent;
        public Action removeButtonClickEvent;

        public BlendshapeData()
        {
            isInvalid = true;
            gameObject = null;
            availableBlendshapeNames = new string[] { "---" };
            selectedBlendshapeIndex = 0;
            value = 0;
        }
    }

    internal class PresetData
    {
        public List<ToggleData> toggles;
        public List<ToggleSuggestionData> toggleSuggestions;
        public List<BlendshapeData> blendshapes;
        public string[] savedPresetKeys;
        public int selectedPresetIndex;

        public PresetData()
        {
            toggles = new List<ToggleData>();
            toggleSuggestions = new List<ToggleSuggestionData>();
            blendshapes = new List<BlendshapeData>();
            savedPresetKeys = new string[] { "---" };
            selectedPresetIndex = 0;
        }
    }

    internal interface IAnimationGenerationWearableModuleEditorView : IEditorView
    {
        event Action AvatarOnWearPresetChangeEvent;
        event Action AvatarOnWearPresetSaveEvent;
        event Action AvatarOnWearPresetDeleteEvent;
        event Action AvatarOnWearToggleAddEvent;
        event Action AvatarOnWearBlendshapeAddEvent;
        event Action WearableOnWearPresetChangeEvent;
        event Action WearableOnWearPresetSaveEvent;
        event Action WearableOnWearPresetDeleteEvent;
        event Action WearableOnWearToggleAddEvent;
        event Action WearableOnWearBlendshapeAddEvent;
        bool ShowCannotRenderPresetWithoutTargetAvatarHelpBox { get; set; }
        bool ShowCannotRenderPresetWithoutTargetWearableHelpBox { get; set; }
        PresetData AvatarOnWearPresetData { get; set; }
        PresetData WearableOnWearPresetData { get; set; }

        string ShowPresetNamingDialog();
        void ShowDuplicatedPresetNameDialog();
        bool ShowPresetDeleteConfirmDialog();
    }
}
