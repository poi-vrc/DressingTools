/*
 * File: DTWearableEditor.cs
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

using Chocopoi.DressingFramework.Cabinet;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    [CustomEditor(typeof(DTWearable))]
    internal class DTWearableEditor : Editor
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public override void OnInspectorGUI()
        {
            // show the tool logo
            DTLogo.Show();

            var wearable = (DTWearable)target;

            wearable.WearableGameObject = (GameObject)EditorGUILayout.ObjectField(t._("wearable.inspector.settings.wearable"), wearable.WearableGameObject, typeof(GameObject), true);
            EditorGUILayout.Separator();

            if (GUILayout.Button(t._("common.inspector.btn.openInEditor"), GUILayout.Height(40)))
            {
                var cabinet = wearable.FindCabinetComponent();

                if (cabinet != null)
                {
                    var window = (DTMainEditorWindow)EditorWindow.GetWindow(typeof(DTMainEditorWindow));
                    window.titleContent = new GUIContent(t._("tool.name"));
                    window.Show();
                    window.StartDressing(cabinet.AvatarGameObject, wearable.WearableGameObject);
                }
                else
                {
                    EditorUtility.DisplayDialog(t._("tool.name"), t._("wearable.inspector.dialog.msg.unableToLocateCabinetToStartDressing"), t._("common.dialog.btn.ok"));
                }
            }
        }
    }
}
