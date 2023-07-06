using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class CabinetSubView : ICabinetSubView
    {
        private CabinetPresenter cabinetPresenter;

        private IMainPresenter mainPresenter;

        private GameObject selectedCreateCabinetGameObject;

        private int selectedCabinetIndex;

        public CabinetSubView(IMainView mainView, IMainPresenter mainPresenter)
        {
            cabinetPresenter = new CabinetPresenter(this);
            this.mainPresenter = mainPresenter;
        }

        public void OnGUI()
        {
            // TODO: beautify UI, now it's so simplified for functionality development

            var cabinets = DTUtils.GetAllCabinets();

            if (cabinets.Length == 0)
            {
                GUILayout.Label("There are no existing cabinets. Create one below for your avatar:");
                selectedCreateCabinetGameObject = (GameObject)EditorGUILayout.ObjectField("Avatar", selectedCreateCabinetGameObject, typeof(GameObject), true);
                if (GUILayout.Button("Create cabinet") && selectedCreateCabinetGameObject != null)
                {
                    DTUtils.GetAvatarCabinet(selectedCreateCabinetGameObject, true);
                }
                return;
            }

            // create dropdown menu for cabinet selection
            string[] cabinetOptions = new string[cabinets.Length];
            for (var i = 0; i < cabinets.Length; i++)
            {
                cabinetOptions[i] = cabinets[i].avatarGameObject != null ? cabinets[i].avatarGameObject.name : string.Format("Cabinet {0} (No GameObject Attached)", i + 1);
            }
            selectedCabinetIndex = EditorGUILayout.Popup("Cabinet", selectedCabinetIndex, cabinetOptions);

            var selectedCabinet = cabinets[selectedCabinetIndex];

            selectedCabinet.avatarGameObject = (GameObject)EditorGUILayout.ObjectField("Avatar", selectedCabinet.avatarGameObject, typeof(GameObject), true);
            selectedCabinet.avatarArmatureName = EditorGUILayout.TextField("Armature Name", selectedCabinet.avatarArmatureName);

            var wearablesToRemove = new List<DTCabinetWearable>();

            foreach (var wearable in selectedCabinet.wearables)
            {
                GUILayout.Label(string.Format("{0} ({1})", wearable.info.name, wearable.info.uuid));
                if (GUILayout.Button("Remove"))
                {
                    wearablesToRemove.Add(wearable);
                }
            }

            // remove all pending wearable in list
            foreach (var wearable in wearablesToRemove)
            {
                selectedCabinet.wearables.Remove(wearable);
            }

            if (GUILayout.Button("Add Wearable"))
            {
                // start dressing
                mainPresenter.StartDressingWizard();
            }
        }
    }
}
