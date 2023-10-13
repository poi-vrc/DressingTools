/*
 * File: CabinetAnimWearableModuleEditor.cs
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
using Chocopoi.DressingFramework.Extensibility.Plugin;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingFramework.UI;
using Chocopoi.DressingTools.Api.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [ExcludeFromCodeCoverage]
    [CustomWearableModuleEditor(typeof(CabinetAnimWearableModuleProvider))]
    internal class CabinetAnimWearableModuleEditor : WearableModuleEditorIMGUI, ICabinetAnimWearableModuleEditorView
    {
        private static I18nTranslator t = I18n.ToolTranslator;

        public event Action ConfigChange;
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

        public bool InvertAvatarToggleOriginalStates { get => _invertAvatarToggleOriginalStates; set => _invertAvatarToggleOriginalStates = value; }
        public bool InvertWearableToggleOriginalStates { get => _invertWearableToggleOriginalStates; set => _invertWearableToggleOriginalStates = value; }
        public bool SetWearableDynamicsInactive { get => _setWearableDynamicsInactive; set => _setWearableDynamicsInactive = value; }
        public bool ShowCannotRenderPresetWithoutTargetAvatarHelpBox { get; set; }
        public bool ShowCannotRenderPresetWithoutTargetWearableHelpBox { get; set; }
        public PresetViewData AvatarOnWearPresetData { get; set; }
        public PresetViewData WearableOnWearPresetData { get; set; }
        public List<CustomizableViewData> Customizables { get; set; }

        private CabinetAnimWearableModuleEditorPresenter _presenter;
        private IWearableModuleEditorViewParent _parentView;
        private bool _invertAvatarToggleOriginalStates;
        private bool _invertWearableToggleOriginalStates;
        private bool _setWearableDynamicsInactive;
        private bool _foldoutCabinetAnimAvatarOnWear;
        private bool _foldoutCabinetAnimWearableOnWear;
        private bool _foldoutAvatarAnimationPresetToggles;
        private bool _foldoutAvatarAnimationPresetBlendshapes;
        private bool _foldoutWearableAnimationPresetToggles;
        private bool _foldoutWearableAnimationPresetBlendshapes;
        private bool _foldoutCabinetAnimCustomizables;

        public CabinetAnimWearableModuleEditor(IWearableModuleEditorViewParent parentView, WearableModuleProviderBase provider, IModuleConfig target) : base(parentView, provider, target)
        {
            _parentView = parentView;
            _presenter = new CabinetAnimWearableModuleEditorPresenter(this, parentView, (CabinetAnimWearableModuleConfig)target);

            ShowCannotRenderPresetWithoutTargetAvatarHelpBox = true;
            ShowCannotRenderPresetWithoutTargetWearableHelpBox = true;
            AvatarOnWearPresetData = new PresetViewData();
            WearableOnWearPresetData = new PresetViewData();
            Customizables = new List<CustomizableViewData>();

            _foldoutCabinetAnimAvatarOnWear = false;
            _foldoutCabinetAnimWearableOnWear = false;
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
                HelpBox(t._("modules.wearable.cabinetAnim.editor.helpbox.objectMustBeAChildOrGrandChildOfRoot"), MessageType.Info);

                BeginHorizontal();
                {
                    Button(t._("modules.wearable.cabinetAnim.editor.btn.add"), addButtonOnClickedEvent, GUILayout.ExpandWidth(false));
                }
                EndHorizontal();

                var toggleDataCopy = new List<ToggleData>(toggles);
                foreach (var toggle in toggleDataCopy)
                {
                    if (toggle.isInvalid)
                    {
                        HelpBox(t._("modules.wearable.cabinetAnim.editor.helpbox.invalidGameObject"), MessageType.Error);
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

                Label(t._("modules.wearable.cabinetAnim.editor.label.suggestions"));

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
                HelpBox(t._("modules.wearable.cabinetAnim.editor.helpbox.objectMustBeAChildOrGrandChildOfRootAndHasSmr"), MessageType.Info);

                Button(t._("modules.wearable.cabinetAnim.editor.btn.add"), addButtonOnClickedEvent, GUILayout.ExpandWidth(false));

                var copy = new List<BlendshapeData>(blendshapes);
                foreach (var blendshape in copy)
                {
                    if (blendshape.isInvalid)
                    {
                        HelpBox(t._("modules.wearable.cabinetAnim.editor.helpbox.invalidGameObject"), MessageType.Error);
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

                Label(t._("modules.wearable.cabinetAnim.editor.label.suggestions"));

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
                Popup(t._("modules.wearable.cabinetAnim.editor.popup.savedPresets"), ref presetData.selectedPresetIndex, presetData.savedPresetKeys, changeEvent);
                Button(t._("modules.wearable.cabinetAnim.editor.btn.save"), saveEvent, GUILayout.ExpandWidth(false));
                Button(t._("modules.wearable.cabinetAnim.editor.btn.delete"), deleteEvent, GUILayout.ExpandWidth(false));
            }
            EndHorizontal();

            Separator();

            DrawToggles(t._("modules.wearable.cabinetAnim.editor.label.toggles"), presetData.toggles, presetData.toggleSuggestions, toggleAddEvent, ref foldoutAnimationPresetToggles);
            DrawBlendshapes(t._("modules.wearable.cabinetAnim.editor.label.blendshapes"), presetData.blendshapes, presetData.smrSuggestions, blendshapeAddEvent, ref foldoutAnimationPresetBlendshapes);
        }

        private void DrawCabinetAnimAvatarOnWear()
        {
            BeginFoldoutBox(ref _foldoutCabinetAnimAvatarOnWear, t._("modules.wearable.cabinetAnim.editor.foldout.avatarAnimOnWear"));
            if (_foldoutCabinetAnimAvatarOnWear)
            {
                if (ShowCannotRenderPresetWithoutTargetAvatarHelpBox)
                {
                    HelpBox(t._("modules.wearable.cabinetAnim.editor.helpbox.cannotRenderPresetWithoutTargetAvatar"), MessageType.Error);
                }
                else
                {
                    DrawAnimationPreset(AvatarOnWearPresetData, AvatarOnWearPresetChangeEvent, AvatarOnWearPresetSaveEvent, AvatarOnWearPresetDeleteEvent, AvatarOnWearToggleAddEvent, AvatarOnWearBlendshapeAddEvent, ref _foldoutAvatarAnimationPresetToggles, ref _foldoutAvatarAnimationPresetBlendshapes);
                }
            }
            EndFoldoutBox();
        }

        private void DrawCabinetAnimWearableOnWear()
        {
            BeginFoldoutBox(ref _foldoutCabinetAnimWearableOnWear, t._("modules.wearable.cabinetAnim.editor.foldout.wearableAnimOnWear"));
            if (_foldoutCabinetAnimWearableOnWear)
            {
                if (ShowCannotRenderPresetWithoutTargetWearableHelpBox)
                {
                    HelpBox(t._("modules.wearable.cabinetAnim.editor.helpbox.cannotRenderPresetWithoutTargetWearable"), MessageType.Error);
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
            BeginFoldoutBoxWithButtonRight(ref customizable.foldout, customizable.name, t._("modules.wearable.cabinetAnim.editor.btn.remove"), customizable.removeButtonClickEvent);
            if (customizable.foldout)
            {
                if (customizable.IsInvalid())
                {
                    HelpBox(t._("modules.wearable.cabinetAnim.editor.helpbox.invalidCustomizable"), MessageType.Error);
                }

                TextField(t._("modules.wearable.cabinetAnim.editor.label.name"), ref customizable.name, customizable.customizableSettingsChangeEvent);
                Popup(t._("modules.wearable.cabinetAnim.editor.popup.customizableType"), ref customizable.type, new string[] { t._("modules.wearable.cabinetAnim.editor.popup.customizableType.toggle"), t._("modules.wearable.cabinetAnim.editor.popup.customizableType.blendshape") }, customizable.customizableSettingsChangeEvent);

                Separator();

                if (customizable.type == 0)
                {
                    // toggle mode
                    DrawToggles(t._("modules.wearable.cabinetAnim.editor.foldout.customizableWearableToggles"), customizable.wearableToggles, customizable.wearableToggleSuggestions, customizable.addWearableToggleEvent, ref customizable.foldoutWearableToggles);

                    HorizontalLine();

                    DrawToggles(t._("modules.wearable.cabinetAnim.editor.foldout.customizableAvatarToggles"), customizable.avatarToggles, customizable.avatarToggleSuggestions, customizable.addAvatarToggleEvent, ref customizable.foldoutAvatarToggles);
                    DrawBlendshapes(t._("modules.wearable.cabinetAnim.editor.foldout.customizableAvatarBlendshapes"), customizable.avatarBlendshapes, customizable.avatarSmrSuggestions, customizable.addAvatarBlendshapeEvent, ref customizable.foldoutAvatarBlendshapes);
                    DrawBlendshapes(t._("modules.wearable.cabinetAnim.editor.foldout.customizableWearableBlendshapes"), customizable.wearableBlendshapes, customizable.wearableSmrSuggestions, customizable.addWearableBlendshapeEvent, ref customizable.foldoutWearableBlendshapes);
                }
                else
                {
                    // radial blendshape mode
                    DrawBlendshapes(t._("modules.wearable.cabinetAnim.editor.foldout.customizableWearableBlendshapes"), customizable.wearableBlendshapes, customizable.wearableSmrSuggestions, customizable.addWearableBlendshapeEvent, ref customizable.foldoutWearableBlendshapes, true);
                }
            }
            EndFoldoutBox();

        }

        private void DrawCustomizables()
        {
            BeginFoldoutBox(ref _foldoutCabinetAnimCustomizables, t._("modules.wearable.cabinetAnim.editor.foldout.customizables"));
            if (_foldoutCabinetAnimCustomizables)
            {
                if (ShowCannotRenderPresetWithoutTargetWearableHelpBox)
                {
                    HelpBox(t._("modules.wearable.cabinetAnim.editor.helpbox.cannotRenderCustomizablesWithoutTargetWearable"), MessageType.Error);
                }
                else
                {
                    BeginHorizontal();
                    {
                        Button(t._("modules.wearable.cabinetAnim.editor.btn.add"), AddCustomizableEvent, GUILayout.ExpandWidth(false));
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
            var module = (CabinetAnimWearableModuleConfig)Target;

            DrawCabinetAnimAvatarOnWear();
            DrawCabinetAnimWearableOnWear();
            DrawCustomizables();

            ToggleLeft(t._("modules.wearable.cabinetAnim.editor.toggle.invertAvatarToggleOriginalStates"), ref _invertAvatarToggleOriginalStates, ConfigChange);
            ToggleLeft(t._("modules.wearable.cabinetAnim.editor.toggle.invertWearableToggleOriginalStates"), ref _invertWearableToggleOriginalStates, ConfigChange);
            ToggleLeft(t._("modules.wearable.cabinetAnim.editor.toggle.setWearableDynamicsInactive"), ref _setWearableDynamicsInactive, ConfigChange);
        }

        private static bool IsPresetViewDataValid(PresetViewData viewData)
        {
            foreach (var toggle in viewData.toggles)
            {
                if (toggle.isInvalid) return false;
            }

            foreach (var blendshape in viewData.blendshapes)
            {
                if (blendshape.isInvalid) return false;
            }
            return true;
        }

        private bool IsCustomizablesValid()
        {
            foreach (var customizable in Customizables)
            {
                foreach (var toggle in customizable.avatarToggles)
                {
                    if (toggle.isInvalid) return false;
                }
                foreach (var toggle in customizable.wearableToggles)
                {
                    if (toggle.isInvalid) return false;
                }
                foreach (var blendshape in customizable.avatarBlendshapes)
                {
                    if (blendshape.isInvalid) return false;
                }
                foreach (var blendshape in customizable.wearableBlendshapes)
                {
                    if (blendshape.isInvalid) return false;
                }
            }
            return true;
        }

        public override bool IsValid()
        {
            return IsPresetViewDataValid(AvatarOnWearPresetData) && IsPresetViewDataValid(WearableOnWearPresetData) && IsCustomizablesValid();
        }

        private class InputPresetNamingDialog : EditorWindow
        {
            public string PresetName { get; set; }
            public InputPresetNamingDialog()
            {
                titleContent = new GUIContent(t._("tool.name"));
                position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            }

            public void OnGUI()
            {
                PresetName = EditorGUILayout.TextField(t._("modules.wearable.cabinetAnim.editor.presetNamingDialog.label.newPresetName"), PresetName);
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(t._("modules.wearable.cabinetAnim.editor.presetNamingDialog.btn.add")))
                    {
                        Close();
                    }
                    if (GUILayout.Button(t._("modules.wearable.cabinetAnim.editor.presetNamingDialog.btn.cancel")))
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
            EditorUtility.DisplayDialog(t._("tool.name"), t._("modules.wearable.cabinetAnim.editor.dialog.msg.duplicatePresetName"), t._("common.dialog.btn.ok"));
        }

        public bool ShowPresetDeleteConfirmDialog()
        {
            return EditorUtility.DisplayDialog(t._("tool.name"), t._("modules.wearable.cabinetAnim.editor.dialog.msg.removePresetConfirm"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no"));
        }
    }
}
