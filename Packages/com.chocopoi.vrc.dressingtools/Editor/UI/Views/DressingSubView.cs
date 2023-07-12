using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class DressingSubView : IDressingSubView
    {
        private IMainPresenter mainPresenter;

        private DressingPresenter dressingPresenter;

        private WearableConfigViewContainer wearableConfigViewSettings;

        private WearableConfigView wearableConfigView;

        public DressingSubView(IMainView mainView, IMainPresenter mainPresenter)
        {
            this.mainPresenter = mainPresenter;
            dressingPresenter = new DressingPresenter(this);
            wearableConfigViewSettings = new WearableConfigViewContainer
            {
                config = new DTWearableConfig()
            };
            wearableConfigView = new WearableConfigView(wearableConfigViewSettings);
        }

        public void OnGUI()
        {
            // TODO: beautify UI
            wearableConfigViewSettings.targetAvatar = (GameObject)EditorGUILayout.ObjectField("Avatar", wearableConfigViewSettings.targetAvatar, typeof(GameObject), true);

            var cabinet = DTEditorUtils.GetAvatarCabinet(wearableConfigViewSettings.targetAvatar);
            if (cabinet == null)
            {
                EditorGUILayout.HelpBox("The selected avatar has no existing cabinet.", MessageType.Error);
            }

            wearableConfigViewSettings.targetWearable = (GameObject)EditorGUILayout.ObjectField("Wearable", wearableConfigViewSettings.targetWearable, typeof(GameObject), true);

            wearableConfigView.OnGUI();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!wearableConfigView.IsConfigReady());
            EditorGUI.BeginDisabledGroup(cabinet == null);
            if (GUILayout.Button("Add to cabinet"))
            {
                mainPresenter.AddToCabinet(cabinet, wearableConfigViewSettings.targetWearable, wearableConfigViewSettings.config);
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Save to file"))
            {

            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
    }
}
