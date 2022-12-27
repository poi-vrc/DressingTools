using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    internal static class DTLogo
    {
        public static void Show()
        {
            // TODO: Design a logo for DT and show it here!
            var titleLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24
            };
            EditorGUILayout.LabelField("DressingTools2", titleLabelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(60));
        }
    }
}
