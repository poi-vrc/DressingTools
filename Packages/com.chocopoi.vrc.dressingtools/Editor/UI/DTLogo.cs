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
