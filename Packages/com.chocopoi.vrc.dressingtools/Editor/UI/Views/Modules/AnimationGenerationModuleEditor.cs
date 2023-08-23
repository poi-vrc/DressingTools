/*
 * File: AnimationGenerationModuleEditor.cs
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Lib.Wearable.Modules.Providers;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [ExcludeFromCodeCoverage]
    [CustomModuleEditor(typeof(AnimationGenerationModuleProvider))]
    internal class AnimationGenerationModuleEditor : ModuleEditor, IAnimationGenerationModuleEditorView
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

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

        public AnimationGenerationModuleEditor(IModuleEditorViewParent parentView, ModuleProviderBase provider, IModuleConfig target) : base(parentView, provider, target)
        {
            _parentView = parentView;
            _presenter = new AnimationGenerationModuleEditorPresenter(this, parentView, (AnimationGenerationModuleConfig)target);

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

        private void DrawAnimationPresetToggles(List<ToggleData> toggles, List<ToggleSuggestionData> toggleSuggestions, Action addButtonOnClickedEvent, ref bool foldoutAnimationPresetToggles)
        {
            BeginFoldoutBox(ref foldoutAnimationPresetToggles, "Toggles");
            if (foldoutAnimationPresetToggles)
            {
                HelpBox("The object must be a child or grand-child of the root. Or it will not be selected.", MessageType.Info);

                BeginHorizontal();
                {
                    Button("+ Add", addButtonOnClickedEvent, GUILayout.ExpandWidth(false));
                }
                EndHorizontal();

                var toggleDataCopy = new List<ToggleData>(toggles);
                foreach (var toggle in toggleDataCopy)
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

                HorizontalLine();

                Label("Suggestions:");

                Separator();

                var toggleSuggestionCopy = new List<ToggleSuggestionData>(toggleSuggestions);
                foreach (var toggleSuggestion in toggleSuggestionCopy)
                {
                    BeginHorizontal();
                    {
                        Button("+", toggleSuggestion.addButtonClickEvent, GUILayout.ExpandWidth(false));
                        BeginDisabled(true);
                        {
                            GameObjectField(ref toggleSuggestion.gameObject, true, null);
                            Toggle(ref toggleSuggestion.state, null);
                        }
                        EndDisabled();
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

            DrawAnimationPresetToggles(presetData.toggles, presetData.toggleSuggestions, toggleAddEvent, ref foldoutAnimationPresetToggles);
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
            var module = (AnimationGenerationModuleConfig)target;

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
