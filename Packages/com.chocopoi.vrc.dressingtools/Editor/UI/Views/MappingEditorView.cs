using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.View
{
    internal class MappingEditorView : EditorViewBase, IMappingEditorView
    {
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
                                    Popup(ref boneMapping.mappingType, new string[] { "Do Nothing", "Move to Avatar Bone", "ParentConstraint to Avatar Bone", "IgnoreTransform on Dynamics", "Copy Avatar Data on Dynamics" }, boneMapping.MappingChange);
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
                HelpBox("In auto mode, everything is controlled by dresser and its generated mappings. To edit mappings, either use Override or even Manual mode.", MessageType.Warning);
            }
            if (mappingMode == 1)
            {
                HelpBox("In override mode, the mappings here will override the ones generated by the dresser. It could be useful for fixing some minor bone mappings.", MessageType.Info);
            }
            if (mappingMode == 2)
            {
                HelpBox("In manual mode, the dresser is ignored and the mappings defined here will be exactly the same when applied to other avatars/users. It might cause incompatibility issues so use with caution.", MessageType.Warning);
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
                        GameObjectField("Left: Avatar", ref _targetAvatar, true);
                    }
                    EndDisabled();
                    Popup("Mode", ref _selectedBoneMappingMode, new string[] { "Auto", "Override", "Manual" }, BoneMappingModeChange);
                    BeginDisabled(true);
                    {
                        GameObjectField("Right: Wearable", ref _targetWearable, true);
                    }
                    EndDisabled();
                }
                EndHorizontal();

                DrawMappingHeaderHelpBoxes(_selectedBoneMappingMode);

                HorizontalLine();

                // only display if in override mode
                if (_selectedBoneMappingMode == 1)
                {
                    Toolbar(ref _selectedBoneMappingDisplayMode, new string[] { "Your Mappings", "Resultant Mappings" }, BoneMappingDisplayModeChange);

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
                HelpBox("Bone mapping not available.", MessageType.Error);
                return;
            }

            DrawBoneMappingEditor();
        }
    }
}
