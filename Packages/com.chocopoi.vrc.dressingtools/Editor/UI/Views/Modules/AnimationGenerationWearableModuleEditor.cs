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
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [ExcludeFromCodeCoverage]
    [CustomWearableModuleEditor(typeof(AnimationGenerationWearableModuleProvider))]
    internal class AnimationGenerationWearableModuleEditor : WearableModuleEditor, IAnimationGenerationWearableModuleEditorView
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public event Action AvatarOnWearPresetChangeEvent;
        public event Action AvatarOnWearPresetSaveEvent;
        public event Action AvatarOnWearPresetDeleteEvent;
        public event Action AvatarOnWearToggleAddEvent;
        public event Action AvatarOnWearBlendshapeAddEvent;
        public event Action WearableOnWearPresetChangeEvent;
        public event Action WearableOnWearPresetSaveEvent;
        public event Action WearableOnWearPresetDeleteEvent;
        public event Action WearableOnWearToggleAddEvent;
        public event Action WearableOnWearBlendshapeAddEvent;
        public event Action AddCustomizableEvent;

        public bool ShowCannotRenderPresetWithoutTargetAvatarHelpBox { get; set; }
        public bool ShowCannotRenderPresetWithoutTargetWearableHelpBox { get; set; }
        public PresetViewData AvatarOnWearPresetData { get; set; }
        public PresetViewData WearableOnWearPresetData { get; set; }
        public List<CustomizableViewData> Customizables { get; set; }

        private AnimationGenerationWearableModuleEditorPresenter _presenter;
        private IWearableModuleEditorViewParent _parentView;
        private bool _foldoutAnimationGenerationAvatarOnWear;
        private bool _foldoutAnimationGenerationWearableOnWear;
        private bool _foldoutAvatarAnimationPresetToggles;
        private bool _foldoutAvatarAnimationPresetBlendshapes;
        private bool _foldoutWearableAnimationPresetToggles;
        private bool _foldoutWearableAnimationPresetBlendshapes;
        private bool _foldoutAnimationGenerationCustomizables;

        public AnimationGenerationWearableModuleEditor(IWearableModuleEditorViewParent parentView, WearableModuleProviderBase provider, IModuleConfig target) : base(parentView, provider, target)
        {
            _parentView = parentView;
            _presenter = new AnimationGenerationWearableModuleEditorPresenter(this, parentView, (AnimationGenerationWearableModuleConfig)target);

            ShowCannotRenderPresetWithoutTargetAvatarHelpBox = true;
            ShowCannotRenderPresetWithoutTargetWearableHelpBox = true;
            AvatarOnWearPresetData = new PresetViewData();
            WearableOnWearPresetData = new PresetViewData();
            Customizables = new List<CustomizableViewData>();

            _foldoutAnimationGenerationAvatarOnWear = false;
            _foldoutAnimationGenerationWearableOnWear = false;
            _foldoutAvatarAnimationPresetToggles = false;
            _foldoutAvatarAnimationPresetBlendshapes = false;
            _foldoutWearableAnimationPresetToggles = false;
            _foldoutWearableAnimationPresetBlendshapes = false;
        }

        private void DrawToggles(string title, List<ToggleData> toggles, List<ToggleSuggestionData> toggleSuggestions, Action addButtonOnClickedEvent, ref bool foldoutAnimationPresetToggles)
        {
            BeginFoldoutBox(ref foldoutAnimationPresetToggles, title);
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

        private void DrawBlendshapes(string title, List<BlendshapeData> blendshapes, List<SmrSuggestionData> smrSuggestions, Action addButtonOnClickedEvent, ref bool foldoutAnimationPresetBlendshapes, bool hideSlider = false)
        {
            BeginFoldoutBox(ref foldoutAnimationPresetBlendshapes, title);
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
                            if (!hideSlider) Slider(ref blendshape.value, 0, 100, blendshape.sliderChangeEvent);
                        }
                        else
                        {
                            // empty placeholder
                            BeginDisabled(true);
                            {
                                var fakeInt = 0;
                                var fakeFloat = 0.0f;
                                Popup(ref fakeInt, new string[] { "---" });
                                if (!hideSlider) Slider(ref fakeFloat, 0, 100);
                            }
                            EndDisabled();
                        }

                        Button("x", blendshape.removeButtonClickEvent, GUILayout.ExpandWidth(false));
                    }
                    EndHorizontal();
                }

                HorizontalLine();

                Label("Suggestions:");

                Separator();

                var smrSuggestionCopy = new List<SmrSuggestionData>(smrSuggestions);
                foreach (var smrSuggestion in smrSuggestionCopy)
                {
                    BeginHorizontal();
                    {
                        Button("+", smrSuggestion.addButtonClickEvent, GUILayout.ExpandWidth(false));
                        BeginDisabled(true);
                        {
                            GameObjectField(ref smrSuggestion.gameObject, true, null);
                        }
                        EndDisabled();
                    }
                    EndHorizontal();
                }
            }
            EndFoldoutBox();
        }

        private void DrawAnimationPreset(PresetViewData presetData, Action changeEvent, Action saveEvent, Action deleteEvent, Action toggleAddEvent, Action blendshapeAddEvent, ref bool foldoutAnimationPresetToggles, ref bool foldoutAnimationPresetBlendshapes)
        {
            BeginHorizontal();
            {
                Popup("Saved Presets", ref presetData.selectedPresetIndex, presetData.savedPresetKeys, changeEvent);
                Button("Save", saveEvent, GUILayout.ExpandWidth(false));
                Button("Delete", deleteEvent, GUILayout.ExpandWidth(false));
            }
            EndHorizontal();

            Separator();

            DrawToggles("Toggles", presetData.toggles, presetData.toggleSuggestions, toggleAddEvent, ref foldoutAnimationPresetToggles);
            DrawBlendshapes("Blendshapes", presetData.blendshapes, presetData.smrSuggestions, blendshapeAddEvent, ref foldoutAnimationPresetBlendshapes);
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
                    DrawAnimationPreset(WearableOnWearPresetData, WearableOnWearPresetChangeEvent, WearableOnWearPresetSaveEvent, WearableOnWearPresetDeleteEvent, WearableOnWearToggleAddEvent, WearableOnWearBlendshapeAddEvent, ref _foldoutWearableAnimationPresetToggles, ref _foldoutWearableAnimationPresetBlendshapes);
                }
            }
            EndFoldoutBox();
        }

        private void DrawCustomizable(CustomizableViewData customizable)
        {
            BeginFoldoutBoxWithButtonRight(ref customizable.foldout, customizable.name, "x Remove", customizable.removeButtonClickEvent);
            if (customizable.foldout)
            {
                TextField("Name", ref customizable.name, customizable.customizableSettingsChangeEvent);
                Popup("Type:", ref customizable.type, new string[] { "Toggle", "Blendshape" }, customizable.customizableSettingsChangeEvent);

                Separator();

                if (customizable.type == 0)
                {
                    // toggle mode
                    DrawToggles("Wearable Toggles", customizable.wearableToggles, customizable.wearableToggleSuggestions, customizable.addWearableToggleEvent, ref customizable.foldoutWearableToggles);

                    HorizontalLine();

                    DrawToggles("Avatar Toggles", customizable.avatarToggles, customizable.avatarToggleSuggestions, customizable.addAvatarToggleEvent, ref customizable.foldoutAvatarToggles);
                    DrawBlendshapes("Avatar Blendshapes", customizable.avatarBlendshapes, customizable.avatarSmrSuggestions, customizable.addAvatarBlendshapeEvent, ref customizable.foldoutAvatarBlendshapes);
                    DrawBlendshapes("Wearable Blendshapes", customizable.wearableBlendshapes, customizable.wearableSmrSuggestions, customizable.addWearableBlendshapeEvent, ref customizable.foldoutWearableBlendshapes);
                }
                else
                {
                    DrawBlendshapes("Wearable Blendshapes", customizable.wearableBlendshapes, customizable.wearableSmrSuggestions, customizable.addWearableBlendshapeEvent, ref customizable.foldoutWearableBlendshapes, true);

                    HorizontalLine();

                    // radial blendshape mode
                    DrawToggles("Avatar Toggles", customizable.avatarToggles, customizable.avatarToggleSuggestions, customizable.addAvatarToggleEvent, ref customizable.foldoutAvatarToggles);
                    DrawToggles("Wearable Toggles", customizable.wearableToggles, customizable.wearableToggleSuggestions, customizable.addWearableToggleEvent, ref customizable.foldoutWearableToggles);
                    DrawBlendshapes("Avatar Blendshapes", customizable.avatarBlendshapes, customizable.avatarSmrSuggestions, customizable.addAvatarBlendshapeEvent, ref customizable.foldoutAvatarBlendshapes);
                }
            }
            EndFoldoutBox();

        }

        private void DrawCustomizables()
        {
            BeginFoldoutBox(ref _foldoutAnimationGenerationCustomizables, "Customizables");
            if (_foldoutAnimationGenerationCustomizables)
            {
                if (ShowCannotRenderPresetWithoutTargetWearableHelpBox)
                {
                    HelpBox("Cannot render customizables without a target wearable selected.", MessageType.Error);
                }
                else
                {
                    BeginHorizontal();
                    {
                        Button("+ Add", AddCustomizableEvent, GUILayout.ExpandWidth(false));
                    }
                    EndHorizontal();

                    Separator();

                    var copy = new List<CustomizableViewData>(Customizables);
                    foreach (var customizable in copy)
                    {
                        DrawCustomizable(customizable);
                    }
                }
            }
            EndFoldoutBox();
        }

        public override void OnGUI()
        {
            var module = (AnimationGenerationWearableModuleConfig)target;

            DrawAnimationGenerationAvatarOnWear();
            DrawAnimationGenerationWearableOnWear();
            DrawCustomizables();
        }

        public override bool IsValid()
        {
            return true;
        }

        private class InputPresetNamingDialog : EditorWindow
        {
            public string PresetName { get; set; }
            public InputPresetNamingDialog()
            {
                titleContent = new GUIContent("DressingTools");
                position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            }

            public void OnGUI()
            {
                PresetName = EditorGUILayout.TextField("New Preset Name:", PresetName);
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Add"))
                    {
                        Close();
                    }
                    if (GUILayout.Button("Cancel"))
                    {
                        PresetName = null;
                        Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            public static InputPresetNamingDialog Create()
            {
                return CreateInstance<InputPresetNamingDialog>();
            }
        }

        public string ShowPresetNamingDialog()
        {
            var dialog = InputPresetNamingDialog.Create();
            dialog.ShowModalUtility();
            return dialog.PresetName;
        }

        public void ShowDuplicatedPresetNameDialog()
        {
            EditorUtility.DisplayDialog("DressingTools", "A preset with the same name exists.", "OK");
        }

        public bool ShowPresetDeleteConfirmDialog()
        {
            return EditorUtility.DisplayDialog("DressingTools", "Are you sure to remove this preset?", "Yes", "No");
        }
    }
}
