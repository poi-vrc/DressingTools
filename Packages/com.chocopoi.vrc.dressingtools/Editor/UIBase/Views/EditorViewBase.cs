/*
 * File: EditorViewBase.cs
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
using Chocopoi.DressingTools.UI;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase
{
    public abstract class EditorViewBase : IEditorView
    {
        public event Action ForceUpdateView;
        public event Action Load;
        public event Action Unload;

        public virtual void RaiseForceUpdateViewEvent()
        {
            ForceUpdateView?.Invoke();
        }

        public abstract void OnGUI();

        public virtual void OnEnable()
        {
            Load?.Invoke();
        }

        public virtual void OnDisable()
        {
            Unload?.Invoke();
        }

        //Reference: https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/#post-3416790
        public void HorizontalLine(int height = 1)
        {
            EditorGUILayout.Separator();
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Separator();
        }

        public void ReadOnlyTextField(string label, string text)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                EditorGUILayout.SelectableLabel(text, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            EditorGUILayout.EndHorizontal();
        }

        public void Label(string text, params GUILayoutOption[] options)
        {
            GUILayout.Label(text, options);
        }

        public void Button(string text, Action onClickEvent = null, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(text, options))
            {
                onClickEvent?.Invoke();
            }
        }

        public void TextField(string label, ref string text, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newValue = EditorGUILayout.TextField(label, text, options);
            var modified = text != newValue;
            text = newValue;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void DelayedTextField(string label, ref string text, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newValue = EditorGUILayout.DelayedTextField(label, text, options);
            var modified = text != newValue;
            text = newValue;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public UnityEngine.Object ObjectField(string label, UnityEngine.Object obj, Type objType, bool allowSceneObjs, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newObj = EditorGUILayout.ObjectField(label, obj, objType, allowSceneObjs, options);
            if (newObj != obj)
            {
                onChangeEvent?.Invoke();
            }
            return newObj;
        }

        public void GameObjectField(ref GameObject go, bool allowSceneObjs, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newObj = (GameObject)EditorGUILayout.ObjectField(go, typeof(GameObject), allowSceneObjs, options);
            var modified = newObj != go;
            go = newObj;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void GameObjectField(string label, ref GameObject go, bool allowSceneObjs, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newObj = (GameObject)EditorGUILayout.ObjectField(label, go, typeof(GameObject), allowSceneObjs, options);
            var modified = newObj != go;
            go = newObj;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void Toggle(string label, ref bool value, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newValue = EditorGUILayout.Toggle(label, value);
            var modified = newValue != value;
            value = newValue;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void Toggle(ref bool value, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newValue = EditorGUILayout.Toggle(value);
            var modified = newValue != value;
            value = newValue;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void ToggleLeft(string label, ref bool value, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newValue = EditorGUILayout.ToggleLeft(label, value);
            var modified = newValue != value;
            value = newValue;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void Slider(ref float value, float leftValue, float rightValue, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newValue = EditorGUILayout.Slider(value, leftValue, rightValue, options);
            var modified = newValue != value;
            value = newValue;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void Slider(string label, ref float value, float leftValue, float rightValue, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newValue = EditorGUILayout.Slider(label, value, leftValue, rightValue, options);
            var modified = newValue != value;
            value = newValue;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void Popup(ref int selectedIndex, string[] keys, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, keys, options);
            var modified = newSelectedIndex != selectedIndex;
            selectedIndex = newSelectedIndex;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void Popup(string label, ref int selectedIndex, string[] keys, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            int newSelectedIndex = EditorGUILayout.Popup(label, selectedIndex, keys, options);
            var modified = newSelectedIndex != selectedIndex;
            selectedIndex = newSelectedIndex;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void HelpBox(string message, MessageType type)
        {
            EditorGUILayout.HelpBox(message, type);
        }

        public void Foldout(ref bool foldout, string label)
        {
            foldout = EditorGUILayout.Foldout(foldout, label);
        }

        public void BeginFoldoutBox(ref bool foldout, string label)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void BeginFoldoutBoxWithButtonRight(ref bool foldout, string foldoutLabel, string btnLabel, Action btnOnClickEvent = null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, foldoutLabel);
            Button(btnLabel, btnOnClickEvent, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void EndFoldoutBox()
        {
            EditorGUILayout.EndVertical();
        }

        public void BeginHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
        }

        public void BeginHorizontal(GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(style, options);
        }

        public void EndHorizontal()
        {
            EditorGUILayout.EndHorizontal();
        }

        public void BeginVertical()
        {
            EditorGUILayout.BeginVertical();
        }

        public void BeginVertical(GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(style, options);
        }

        public void EndVertical()
        {
            EditorGUILayout.EndVertical();
        }

        public void BeginDisabled(bool disabled)
        {
            EditorGUI.BeginDisabledGroup(disabled);
        }

        public void EndDisabled()
        {
            EditorGUI.EndDisabledGroup();
        }

        public void Separator()
        {
            EditorGUILayout.Separator();
        }

        public void Toolbar(ref int selected, string[] keys, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newSelected = GUILayout.Toolbar(selected, keys, options);
            var modified = newSelected != selected;
            selected = newSelected;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }

        public void TextArea(ref string text, Action onChangeEvent = null, params GUILayoutOption[] options)
        {
            var newValue = EditorGUILayout.TextArea(text, options);
            var modified = newValue != text;
            text = newValue;
            if (modified)
            {
                onChangeEvent?.Invoke();
            }
        }
    }
}
