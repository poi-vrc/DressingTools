/*
 * File: MappingEditorView.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 12:21:25 am
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
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.View
{
    [ExcludeFromCodeCoverage]
    internal class MappingEditorView : IMGUIViewBase, IMappingEditorView
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

        public event Action BoneMappingModeChange;
        public event Action BoneMappingDisplayModeChange;

        public GameObject TargetAvatar { get => _targetAvatar; set => _targetAvatar = value; }
        public GameObject TargetWearable { get => _targetWearable; set => _targetWearable = value; }
        public int SelectedBoneMappingMode { get => _selectedBoneMappingMode; set => _selectedBoneMappingMode = value; }
        public int SelectedBoneMappingDisplayMode { get => _selectedBoneMappingDisplayMode; set => _selectedBoneMappingDisplayMode = value; }
        public List<ViewAvatarHierachyNode> AvatarHierachyNodes { get; set; }
        public bool ShowBoneMappingNotAvailableHelpbox { get; set; }

        private MappingEditorPresenter _presenter;
        private GameObject _targetAvatar;
        private GameObject _targetWearable;
        private int _selectedBoneMappingMode;
        private int _selectedBoneMappingDisplayMode;
        private Vector2 _scrollPos;

        public MappingEditorView()
        {
            _presenter = new MappingEditorPresenter(this);

            TargetAvatar = null;
            TargetWearable = null;
            SelectedBoneMappingMode = 0;
            SelectedBoneMappingDisplayMode = 0;
            AvatarHierachyNodes = new List<ViewAvatarHierachyNode>();
            ShowBoneMappingNotAvailableHelpbox = true;
        }

        public void SetContainer(DTMappingEditorContainer container) => _presenter.SetContainer(container);

        private void DrawAvatarHierarchy(List<ViewAvatarHierachyNode> nodes)
        {
            var nodeCopy = new List<ViewAvatarHierachyNode>(nodes);
            foreach (var node in nodeCopy)
            {
                BeginHorizontal();
                {
                    Foldout(ref node.foldout, node.avatarObjectTransform.name);

                    BeginDisabled(true);
                    {
                        EditorGUILayout.ObjectField(node.avatarObjectTransform.gameObject, typeof(GameObject), true, GUILayout.ExpandWidth(false));
                    }
                    EndDisabled();

                    Button("+", node.AddMappingButtonClick, GUILayout.ExpandWidth(false));

                    // backup and set indent level to zero
                    var lastIndentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;

                    BeginVertical();
                    {
                        if (node.wearableMappings.Count > 0)
                        {
                            var mappingsCopy = new List<ViewBoneMapping>(node.wearableMappings);
                            foreach (var boneMapping in mappingsCopy)
                            {
                                if (boneMapping.isInvalid)
                                {
                                    HelpBox("Invalid mapping below:", MessageType.Error);
                                }
                                BeginHorizontal();
                                {
                                    Popup(ref boneMapping.mappingType, new string[] {
                                        t._("mapping.editor.popup.mappingType.doNothing"),
                                        t._("mapping.editor.popup.mappingType.moveToAvatarBone"),
                                        t._("mapping.editor.popup.mappingType.parentConstraintToAvatarBone"),
                                        t._("mapping.editor.popup.mappingType.ignoreTransformOnDynamics"),
                                        t._("mapping.editor.popup.mappingType.copyAvatarDynamicsData")
                                        }, boneMapping.MappingChange);
                                    GameObjectField(ref boneMapping.wearableObject, true, boneMapping.MappingChange);
                                    Button("x", boneMapping.RemoveMappingButtonClick, GUILayout.ExpandWidth(false));
                                }
                                EndHorizontal();
                            }
                        }
                        else
                        {
                            // empty mapping placeholder
                            BeginDisabled(true);
                            {
                                BeginHorizontal();
                                {
                                    EditorGUILayout.Popup(0, new string[] { "---" });
                                    EditorGUILayout.ObjectField(null, typeof(GameObject), true);
                                    GUILayout.Button("x", GUILayout.ExpandWidth(false));
                                }
                                EndHorizontal();
                            }
                            EndDisabled();
                        }
                    }
                    EndVertical();

                    EditorGUI.indentLevel = lastIndentLevel;
                }
                EndHorizontal();

                if (node.foldout)
                {
                    EditorGUI.indentLevel += 1;

                    // render childs
                    DrawAvatarHierarchy(node.childs);

                    EditorGUI.indentLevel -= 1;
                }
            }
        }

        private void DrawMappingHeaderHelpBoxes(int mappingMode)
        {
            if (mappingMode == 0)
            {
                HelpBox(t._("mapping.editor.helpbox.autoModeExplanation"), MessageType.Warning);
            }
            if (mappingMode == 1)
            {
                HelpBox(t._("mapping.editor.helpbox.overrideModeExplanation"), MessageType.Info);
            }
            if (mappingMode == 2)
            {
                HelpBox(t._("mapping.editor.helpbox.manualModeExplanation"), MessageType.Warning);
            }
        }

        private void DrawBoneMappingEditor()
        {
            BeginVertical();
            {
                BeginHorizontal();
                {
                    BeginDisabled(true);
                    {
                        GameObjectField(t._("mapping.editor.gameObjectField.leftAvatar"), ref _targetAvatar, true);
                    }
                    EndDisabled();
                    Popup(t._("mapping.editor.popup.mappingMode"), ref _selectedBoneMappingMode, new string[] {
                        t._("mapping.editor.popup.mappingMode.auto"),
                        t._("mapping.editor.popup.mappingMode.override"),
                        t._("mapping.editor.popup.mappingMode.manual")
                        }, BoneMappingModeChange);
                    BeginDisabled(true);
                    {
                        GameObjectField(t._("mapping.editor.gameObjectField.rightWearable"), ref _targetWearable, true);
                    }
                    EndDisabled();
                }
                EndHorizontal();

                DrawMappingHeaderHelpBoxes(_selectedBoneMappingMode);

                HorizontalLine();

                // only display if in override mode
                if (_selectedBoneMappingMode == 1)
                {
                    Toolbar(ref _selectedBoneMappingDisplayMode, new string[] { t._("mapping.editor.displayModes.yourMappings"), t._("mapping.editor.displayModes.resultantMappings") }, BoneMappingDisplayModeChange);

                    Separator();
                }

                // scroll view
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

                // disable on Auto mode
                BeginDisabled(_selectedBoneMappingMode == 0 || (_selectedBoneMappingMode == 1 && _selectedBoneMappingDisplayMode == 1));
                {
                    DrawAvatarHierarchy(AvatarHierachyNodes);
                }
                EndDisabled();

                EditorGUILayout.EndScrollView();
            }
            EndVertical();
        }

        public override void OnGUI()
        {
            if (ShowBoneMappingNotAvailableHelpbox)
            {
                HelpBox(t._("mapping.editor.helpbox.boneMappingsNotAvailable"), MessageType.Error);
                return;
            }

            DrawBoneMappingEditor();
        }
    }
}
