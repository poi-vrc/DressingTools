using Chocopoi.DressingTools.UI.View;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    public class DTMainEditorWindow : EditorWindow
    {
        private MainView _view;

        [MenuItem("Tools/chocopoi/DressingTools", false, 0)]
        public static void ShowWindow()
        {
            var window = (DTMainEditorWindow)GetWindow(typeof(DTMainEditorWindow));
            window.titleContent = new GUIContent("DressingTools");
            window.Show();
        }

        public DTMainEditorWindow()
        {
            _view = new MainView();
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
