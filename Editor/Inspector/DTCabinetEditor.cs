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

using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Api.Cabinet;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    [ExcludeFromCodeCoverage]
    [CustomEditor(typeof(DTCabinet))]
    internal class DTCabinetEditor : Editor
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public override void OnInspectorGUI()
        {
            // show the tool logo
            DTLogo.Show();

            var cabinet = (DTCabinet)target;

            cabinet.AvatarGameObject = (GameObject)EditorGUILayout.ObjectField(t._("cabinet.inspector.settings.avatar"), cabinet.AvatarGameObject, typeof(GameObject), true);
            EditorGUILayout.Separator();

            if (GUILayout.Button(t._("common.inspector.btn.openInEditor"), GUILayout.Height(40)))
            {
                var window = (DTMainEditorWindow)EditorWindow.GetWindow(typeof(DTMainEditorWindow));
                window.titleContent = new GUIContent(t._("tool.name"));
                window.Show();
                window.SelectCabinet(cabinet);
            }
        }
    }
}
