/*
 * File: DTLegacyEditorWindow.cs
 * Project: DressingTools
 * Created Date: Wednesday, September 10th 2023, 2:45:04 pm
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
using Chocopoi.DressingTools.UI.View;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    [ExcludeFromCodeCoverage]
    public class DTLegacyEditorWindow : EditorWindow
    {
        private LegacyView _view;

        [MenuItem("Tools/chocopoi/Legacy Editor", false, 0)]
        static void ShowWindow()
        {
            var window = (DTLegacyEditorWindow)GetWindow(typeof(DTLegacyEditorWindow));
            window.titleContent = new GUIContent("DressingTools Legacy");
            window.Show();
        }

        public DTLegacyEditorWindow()
        {
            _view = new LegacyView();
        }

        public void OnEnable()
        {
            _view.OnEnable();
        }

        public void OnDisable()
        {
            _view.OnDisable();
        }

        public void OnGUI()
        {
            _view.OnGUI();
        }
    }
}
