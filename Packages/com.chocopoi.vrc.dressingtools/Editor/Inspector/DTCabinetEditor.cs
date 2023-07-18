using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UI;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    [CustomEditor(typeof(DTCabinet))]
    public class DTCabinetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // show the tool logo
            DTLogo.Show();

            var cabinet = (DTCabinet)target;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Avatar", cabinet.avatarGameObject, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();

            if (GUILayout.Button("Refresh Cabinet"))
            {
                EditorUtility.DisplayProgressBar("DressingTools", "Refreshing cabinet...", 0);
                cabinet.RefreshCabinet();
                EditorUtility.ClearProgressBar();
            }

            if (GUILayout.Button("Open in Editor", GUILayout.Height(40)))
            {
                // TODO: select cabinet in editor
                var window = (DTMainEditorWindow)EditorWindow.GetWindow(typeof(DTMainEditorWindow));
                window.titleContent = new GUIContent("DressingTools");
                window.Show();
            }
        }
    }
}
