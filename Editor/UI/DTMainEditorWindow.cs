/*
 * File: DTMainEditorWindow.cs
 * Project: DressingTools
 * Created Date: Thursday, August 10th 2023, 12:27:04 am
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

using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    [ExcludeFromCodeCoverage]
    internal class DTMainEditorWindow : EditorWindow
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private MainView _view;

        [MenuItem("Tools/chocopoi/Reload Translations", false, 0)]
        public static void ReloadTranslations()
        {
            I18n.ReloadTranslations();
        }

        [MenuItem("Tools/chocopoi/DressingTools", false, 0)]
        public static void ShowWindow()
        {
            var window = (DTMainEditorWindow)GetWindow(typeof(DTMainEditorWindow));
            window.titleContent = new GUIContent(t._("tool.name"));
            window.Show();
        }

        public void SelectAvatar(GameObject avatarGameObject) => _view.SelectAvatar(avatarGameObject);

        public void StartDressing(GameObject avatarGameObject, GameObject wearableGameObject)
        {
            _view.SelectedTab = 1;
            _view.StartDressing(avatarGameObject, wearableGameObject);
        }

        public void OnEnable()
        {
            _view = new MainView();
            rootVisualElement.Add(_view);
            _view.OnEnable();
        }

        public void OnDisable()
        {
            _view.OnDisable();
        }
    }
}
