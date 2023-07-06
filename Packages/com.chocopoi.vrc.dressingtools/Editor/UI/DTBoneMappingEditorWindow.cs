using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    public class DTBoneMappingEditorSettings
    {
        public GameObject targetAvatar;
        public GameObject targetWearable;
        public List<DTBoneMapping> boneMappings;
    }

    public class DTBoneMappingEditorWindow : EditorWindow
    {
        public DTBoneMappingEditorSettings settings;

        private Vector2 scrollPos;

        public DTBoneMappingEditorWindow()
        {
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("Avatar", null, typeof(GameObject), true);
            EditorGUILayout.ObjectField("Wearable", null, typeof(GameObject), true);
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true);
            GUILayout.BeginHorizontal();

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
