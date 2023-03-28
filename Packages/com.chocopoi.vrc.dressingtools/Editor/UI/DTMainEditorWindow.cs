using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    public class DTMainEditorWindow : EditorWindow, IMainView
    {
        private IMainPresenter mainPresenter;

        private ICabinetSubView cabinetSubView;
        private IDressingSubView dressingSubView;
        private ISettingsSubView settingsSubView;

        protected int selectedTab;

        [MenuItem("Tools/chocopoi/DressingTools", false, 0)]
        public static void ShowWindow()
        {
            var window = (DTMainEditorWindow)GetWindow(typeof(DTMainEditorWindow));
            window.titleContent = new GUIContent("DressingTools");
            window.Show();
        }

        public DTMainEditorWindow()
        {
            mainPresenter = new MainPresenter(this);
            cabinetSubView = new CabinetSubView(this);
            dressingSubView = new DressingSubView(this);
            settingsSubView = new SettingsSubView(this);
        }

        public void OnGUI()
        {
            // show the tool logo
            DTLogo.Show();

            // tool tabs
            selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Cabinet", "Dressing", "Settings" });
            if (selectedTab == 0)
            {
                cabinetSubView.OnGUI();
            }
            else if (selectedTab == 1)
            {
                dressingSubView.OnGUI();
            }
            else if (selectedTab == 2)
            {
                settingsSubView.OnGUI();
            }
        }
    }
}
