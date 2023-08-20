/*
 * File: DTCabinetEditor.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 9th 2023, 11:38:52 pm
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

using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UI;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    [CustomEditor(typeof(DTCabinet))]
    internal class DTCabinetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // show the tool logo
            DTLogo.Show();

            var cabinet = (DTCabinet)target;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Avatar", cabinet.AvatarGameObject, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();

            if (GUILayout.Button("Open in Editor", GUILayout.Height(40)))
            {
                var window = (DTMainEditorWindow)EditorWindow.GetWindow(typeof(DTMainEditorWindow));
                window.titleContent = new GUIContent("DressingTools");
                window.Show();
                window.SelectCabinet(cabinet);
            }
        }
    }
}
