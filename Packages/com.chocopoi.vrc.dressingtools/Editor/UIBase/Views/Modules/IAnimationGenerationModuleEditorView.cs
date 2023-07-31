using System;
using System.Collections.Generic;
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
        public List<BlendshapeData> blendshapes;
        public string[] savedPresetKeys;
        public int selectedPresetIndex;

        public PresetData()
        {
            toggles = new List<ToggleData>();
            blendshapes = new List<BlendshapeData>();
            savedPresetKeys = new string[] { "---" };
            selectedPresetIndex = 0;
        }
    }

    internal interface IAnimationGenerationModuleEditorView : IEditorView
    {
        event Action TargetAvatarOrWearableChange;
        event Action AvatarOnWearPresetChangeEvent;
        event Action AvatarOnWearPresetSaveEvent;
        event Action AvatarOnWearPresetDeleteEvent;
        event Action AvatarOnWearToggleAddEvent;
        event Action AvatarOnWearBlendshapeAddEvent;
        event Action WearableOnWearChangeEvent;
        event Action WearableOnWearSaveEvent;
        event Action WearableOnWearDeleteEvent;
        event Action WearableOnWearToggleAddEvent;
        event Action WearableOnWearBlendshapeAddEvent;
        bool ShowCannotRenderPresetWithoutTargetAvatarHelpBox { get; set; }
        bool ShowCannotRenderPresetWithoutTargetWearableHelpBox { get; set; }
        PresetData AvatarOnWearPresetData { get; set; }
        PresetData WearableOnWearPresetData { get; set; }
    }
}
