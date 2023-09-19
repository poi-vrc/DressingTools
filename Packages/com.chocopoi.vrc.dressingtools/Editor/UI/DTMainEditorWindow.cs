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
using Chocopoi.DressingFramework.Cabinet;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingFramework.Wearable;
using Chocopoi.DressingFramework.Wearable.Modules;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.UI.View;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    [ExcludeFromCodeCoverage]
    public class DTMainEditorWindow : EditorWindow
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

        private MainView _view;

        [MenuItem("Tools/chocopoi/DressingTools", false, 0)]
        public static void ShowWindow()
        {
            var window = (DTMainEditorWindow)GetWindow(typeof(DTMainEditorWindow));
            window.titleContent = new GUIContent(t._("tool.name"));
            window.Show();
        }

        public void SelectCabinet(DTCabinet cabinet) => _view.SelectCabinet(cabinet);

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
