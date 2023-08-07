﻿using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [CustomModuleEditor(typeof(AnimationGenerationModule))]
    internal class AnimationGenerationModuleEditor : ModuleEditor, IAnimationGenerationModuleEditorView
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public event Action TargetAvatarOrWearableChange { add { _parentView.TargetAvatarOrWearableChange += value; } remove { _parentView.TargetAvatarOrWearableChange -= value; } }
        public event Action AvatarOnWearPresetChangeEvent;
        public event Action AvatarOnWearPresetSaveEvent;
        public event Action AvatarOnWearPresetDeleteEvent;
        public event Action AvatarOnWearToggleAddEvent;
        public event Action AvatarOnWearBlendshapeAddEvent;
        public event Action WearableOnWearChangeEvent;
        public event Action WearableOnWearSaveEvent;
        public event Action WearableOnWearDeleteEvent;
        public event Action WearableOnWearToggleAddEvent;
        public event Action WearableOnWearBlendshapeAddEvent;

        public bool ShowCannotRenderPresetWithoutTargetAvatarHelpBox { get; set; }
        public bool ShowCannotRenderPresetWithoutTargetWearableHelpBox { get; set; }
        public PresetData AvatarOnWearPresetData { get; set; }
        public PresetData WearableOnWearPresetData { get; set; }

        private AnimationGenerationModuleEditorPresenter _presenter;
        private IModuleEditorViewParent _parentView;
        private bool _foldoutAnimationGenerationAvatarOnWear;
        private bool _foldoutAnimationGenerationWearableOnWear;
        private bool _foldoutAvatarAnimationPresetToggles;
        private bool _foldoutAvatarAnimationPresetBlendshapes;
        private bool _foldoutWearableAnimationPresetToggles;
        private bool _foldoutWearableAnimationPresetBlendshapes;

        public AnimationGenerationModuleEditor(IModuleEditorViewParent parentView, DTWearableModuleBase target) : base(parentView, target)
        {
            _parentView = parentView;
            _presenter = new AnimationGenerationModuleEditorPresenter(this, parentView, (AnimationGenerationModule)target);

            ShowCannotRenderPresetWithoutTargetAvatarHelpBox = true;
            ShowCannotRenderPresetWithoutTargetWearableHelpBox = true;
            AvatarOnWearPresetData = new PresetData();
            WearableOnWearPresetData = new PresetData();

            _foldoutAnimationGenerationAvatarOnWear = false;
            _foldoutAnimationGenerationWearableOnWear = false;
            _foldoutAvatarAnimationPresetToggles = false;
            _foldoutAvatarAnimationPresetBlendshapes = false;
            _foldoutWearableAnimationPresetToggles = false;
            _foldoutWearableAnimationPresetBlendshapes = false;
        }

        private void DrawAnimationPresetToggles(List<ToggleData> toggles, Action addButtonOnClickedEvent, ref bool foldoutAnimationPresetToggles)
        {
            BeginFoldoutBox(ref foldoutAnimationPresetToggles, "Toggles");
            if (foldoutAnimationPresetToggles)
            {
                HelpBox("The object must be a child or grand-child of the root. Or it will not be selected.", MessageType.Info);

                Button("+ Add", addButtonOnClickedEvent, GUILayout.ExpandWidth(false));

                var copy = new List<ToggleData>(toggles);
                foreach (var toggle in copy)
                {
                    if (toggle.isInvalid)
                    {
                        HelpBox("The following GameObject is invalid.", MessageType.Error);
                    }
                    BeginHorizontal();
                    {
                        GameObjectField(ref toggle.gameObject, true, toggle.changeEvent);
                        Toggle(ref toggle.state, toggle.changeEvent);
                        Button("x", toggle.removeButtonClickEvent, GUILayout.ExpandWidth(false));
                    }
                    EndHorizontal();
                }
            }
            EndFoldoutBox();
        }

        private void DrawAnimationPresetBlendshapes(List<BlendshapeData> blendshapes, Action addButtonOnClickedEvent, ref bool foldoutAnimationPresetBlendshapes)
        {
            BeginFoldoutBox(ref foldoutAnimationPresetBlendshapes, "Blendshapes");
            if (foldoutAnimationPresetBlendshapes)
            {
                HelpBox("The object must be a child or grand-child of the root, and has a SkinnedMeshRenderer. Or it will not be selected.", MessageType.Info);

                Button("+ Add", addButtonOnClickedEvent, GUILayout.ExpandWidth(false));

                var copy = new List<BlendshapeData>(blendshapes);
                foreach (var blendshape in copy)
                {
                    if (blendshape.isInvalid)
                    {
                        HelpBox("The following GameObject is invalid.", MessageType.Error);
                    }
                    BeginHorizontal();
                    {
                        GameObjectField(ref blendshape.gameObject, true, blendshape.gameObjectFieldChangeEvent);

                        if (!blendshape.isInvalid)
                        {
                            Popup(ref blendshape.selectedBlendshapeIndex, blendshape.availableBlendshapeNames, blendshape.blendshapeNameChangeEvent);
                            Slider(ref blendshape.value, 0, 100, blendshape.sliderChangeEvent);
                        }
                        else
                        {
                            // empty placeholder
                            BeginDisabled(true);
                            {
                                var fakeInt = 0;
                                var fakeFloat = 0.0f;
                                Popup(ref fakeInt, new string[] { "---" });
                                Slider(ref fakeFloat, 0, 100);
                            }
                            EndDisabled();
                        }

                        Button("x", blendshape.removeButtonClickEvent, GUILayout.ExpandWidth(false));
                    }
                    EndHorizontal();
                }
            }
            EndFoldoutBox();
        }

        private void DrawAnimationPreset(PresetData presetData, Action changeEvent, Action saveEvent, Action deleteEvent, Action toggleAddEvent, Action blendshapeAddEvent, ref bool foldoutAnimationPresetToggles, ref bool foldoutAnimationPresetBlendshapes)
        {
            BeginHorizontal();
            {
                Popup("Saved Presets", ref presetData.selectedPresetIndex, presetData.savedPresetKeys, changeEvent);
                Button("Save", saveEvent, GUILayout.ExpandWidth(false));
                Button("Delete", deleteEvent, GUILayout.ExpandWidth(false));
            }
            EndHorizontal();

            Separator();

            DrawAnimationPresetToggles(presetData.toggles, toggleAddEvent, ref foldoutAnimationPresetToggles);
            DrawAnimationPresetBlendshapes(presetData.blendshapes, blendshapeAddEvent, ref foldoutAnimationPresetBlendshapes);
        }

        private void DrawAnimationGenerationAvatarOnWear()
        {
            BeginFoldoutBox(ref _foldoutAnimationGenerationAvatarOnWear, "Avatar Animation On Wear");
            if (_foldoutAnimationGenerationAvatarOnWear)
            {
                if (ShowCannotRenderPresetWithoutTargetAvatarHelpBox)
                {
                    HelpBox("Cannot render preset without a target avatar selected.", MessageType.Error);
                }
                else
                {
                    DrawAnimationPreset(AvatarOnWearPresetData, AvatarOnWearPresetChangeEvent, AvatarOnWearPresetSaveEvent, AvatarOnWearPresetDeleteEvent, AvatarOnWearToggleAddEvent, AvatarOnWearBlendshapeAddEvent, ref _foldoutAvatarAnimationPresetToggles, ref _foldoutAvatarAnimationPresetBlendshapes);
                }
            }
            EndFoldoutBox();
        }

        private void DrawAnimationGenerationWearableOnWear()
        {
            BeginFoldoutBox(ref _foldoutAnimationGenerationWearableOnWear, "Wearable Animation On Wear");
            if (_foldoutAnimationGenerationWearableOnWear)
            {
                if (ShowCannotRenderPresetWithoutTargetWearableHelpBox)
                {
                    HelpBox("Cannot render preset without a target wearable selected.", MessageType.Error);
                }
                else
                {
                    DrawAnimationPreset(WearableOnWearPresetData, WearableOnWearChangeEvent, WearableOnWearSaveEvent, WearableOnWearDeleteEvent, WearableOnWearToggleAddEvent, WearableOnWearBlendshapeAddEvent, ref _foldoutWearableAnimationPresetToggles, ref _foldoutWearableAnimationPresetBlendshapes);
                }
            }
            EndFoldoutBox();
        }

        public override void OnGUI()
        {
            var module = (AnimationGenerationModule)target;

            DrawAnimationGenerationAvatarOnWear();
            DrawAnimationGenerationWearableOnWear();
            // TODO: customizables
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}