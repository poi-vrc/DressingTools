/*
 * File: DTLogo.cs
 * Project: DressingTools
 * Created Date: Saturday, July 22nd 2023, 12:36:56 am
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

using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    internal static class DTLogo
    {
        private const string LogoGuid = "7f5e245331889a94bb6e4a077cbd97a6";
        private static readonly Texture2D LogoTexture;
        private static readonly GUIStyle LogoStyle;

        static DTLogo()
        {
            var path = AssetDatabase.GUIDToAssetPath(LogoGuid);
            LogoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (LogoTexture == null)
            {
                Debug.Log("[DressingTools] Unable to load logo banner texture!");
                return;
            }

            var ratio = LogoTexture.width / (float)LogoTexture.height;
            LogoStyle = new GUIStyle()
            {
                imagePosition = ImagePosition.ImageOnly,
                stretchWidth = false,
                stretchHeight = false,
                fixedWidth = 64 * ratio,
                fixedHeight = 64,
            };
        }

        public static void Show()
        {
            if (LogoTexture == null)
            {
                var titleLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 24
                };
                EditorGUILayout.LabelField("DressingTools2", titleLabelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(60));
                return;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            var ratio = 64 / (float)LogoTexture.height;
            var width = LogoTexture.width * ratio;
            var rect = GUILayoutUtility.GetRect(width, 64);
            GUI.DrawTexture(rect, LogoTexture, ScaleMode.ScaleToFit);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
        }
    }
}
