/*
 * File: ICabinetAnimWearableModuleEditorView.cs
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
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
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

    internal class SmrSuggestionData
    {
        public GameObject gameObject;
        public Action addButtonClickEvent;

        public SmrSuggestionData()
        {
            gameObject = null;
        }
    }

    internal class PresetViewData
    {
        public List<ToggleData> toggles;
        public List<ToggleSuggestionData> toggleSuggestions;
        public List<BlendshapeData> blendshapes;
        public List<SmrSuggestionData> smrSuggestions;
        public string[] savedPresetKeys;
        public int selectedPresetIndex;

        public PresetViewData()
        {
            toggles = new List<ToggleData>();
            toggleSuggestions = new List<ToggleSuggestionData>();
            blendshapes = new List<BlendshapeData>();
            smrSuggestions = new List<SmrSuggestionData>();
            savedPresetKeys = new string[] { "---" };
            selectedPresetIndex = 0;
        }
    }


    internal class CustomizableViewData
    {
        public bool foldout;
        public string name;
        public int type;
        public float defaultValue;
        public Action customizableSettingsChangeEvent;
        public bool foldoutAvatarToggles;
        public List<ToggleData> avatarToggles;
        public List<ToggleSuggestionData> avatarToggleSuggestions;
        public Action addAvatarToggleEvent;
        public bool foldoutWearableToggles;
        public List<ToggleData> wearableToggles;
        public List<ToggleSuggestionData> wearableToggleSuggestions;
        public Action addWearableToggleEvent;
        public bool foldoutAvatarBlendshapes;
        public List<BlendshapeData> avatarBlendshapes;
        public List<SmrSuggestionData> avatarSmrSuggestions;
        public Action addAvatarBlendshapeEvent;
        public bool foldoutWearableBlendshapes;
        public List<BlendshapeData> wearableBlendshapes;
        public List<SmrSuggestionData> wearableSmrSuggestions;
        public Action addWearableBlendshapeEvent;
        public Action removeButtonClickEvent;

        public CustomizableViewData()
        {
            foldout = false;
            name = null;
            type = 0;
            defaultValue = 0;
            customizableSettingsChangeEvent = null;
            foldoutAvatarToggles = false;
            avatarToggles = new List<ToggleData>();
            avatarToggleSuggestions = new List<ToggleSuggestionData>();
            addAvatarToggleEvent = null;
            foldoutWearableToggles = false;
            wearableToggles = new List<ToggleData>();
            wearableToggleSuggestions = new List<ToggleSuggestionData>();
            addWearableToggleEvent = null;
            foldoutAvatarBlendshapes = false;
            avatarBlendshapes = new List<BlendshapeData>();
            avatarSmrSuggestions = new List<SmrSuggestionData>();
            addAvatarBlendshapeEvent = null;
            foldoutWearableBlendshapes = false;
            wearableBlendshapes = new List<BlendshapeData>();
            wearableSmrSuggestions = new List<SmrSuggestionData>();
            addWearableBlendshapeEvent = null;
            removeButtonClickEvent = null;
        }

        public bool IsInvalid()
        {
            var result = false;

            result |= name == null || name == "";

            foreach (var toggle in avatarToggles)
            {
                result |= toggle.isInvalid;
            }

            foreach (var toggle in wearableToggles)
            {
                result |= toggle.isInvalid;
            }

            foreach (var blendshape in avatarBlendshapes)
            {
                result |= blendshape.isInvalid;
            }

            foreach (var blendshape in wearableBlendshapes)
            {
                result |= blendshape.isInvalid;
            }

            return result;
        }
    }

    internal interface ICabinetAnimWearableModuleEditorView : IEditorView
    {
        event Action ConfigChange;
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
        event Action AddCustomizableEvent;

        bool InvertAvatarToggleOriginalStates { get; set; }
        bool InvertWearableToggleOriginalStates { get; set; }
        bool SetWearableDynamicsInactive { get; set; }
        bool ShowCannotRenderPresetWithoutTargetAvatarHelpBox { get; set; }
        bool ShowCannotRenderPresetWithoutTargetWearableHelpBox { get; set; }
        PresetViewData AvatarOnWearPresetData { get; set; }
        PresetViewData WearableOnWearPresetData { get; set; }
        List<CustomizableViewData> Customizables { get; set; }

        string ShowPresetNamingDialog();
        void ShowDuplicatedPresetNameDialog();
        bool ShowPresetDeleteConfirmDialog();
    }
}
