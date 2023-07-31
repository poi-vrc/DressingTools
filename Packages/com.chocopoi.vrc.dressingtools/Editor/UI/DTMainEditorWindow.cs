using Chocopoi.DressingTools.UI.View;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    public class DTMainEditorWindow : EditorWindow
    {
        private MainView view_;

        [MenuItem("Tools/chocopoi/DressingTools", false, 0)]
        public static void ShowWindow()
        {
            var window = (DTMainEditorWindow)GetWindow(typeof(DTMainEditorWindow));
            window.titleContent = new GUIContent("DressingTools");
            window.Show();
        }

        public DTMainEditorWindow()
        {
            view_ = new MainView();
        }

        public void OnEnable()
        {
            view_.OnEnable();
        }

        public void OnDisable()
        {
            view_.OnDisable();
        }

        public void OnGUI()
        {
            view_.OnGUI();
        }
    }
}
