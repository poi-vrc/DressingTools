using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class CabinetSubView : ICabinetSubView
    {
        private CabinetPresenter cabinetPresenter;

        private GameObject selectedCreateCabinetGameObject;

        public CabinetSubView(IMainView mainView)
        {
            cabinetPresenter = new CabinetPresenter(this);
        }

        public void OnGUI()
        {
            // TODO: beautify UI, now it's so simplified for functionality development
            GUILayout.Label("Cabinet");

            var cabinets = cabinetPresenter.GetCabinets();

            if (cabinets.Length == 0)
            {
                GUILayout.Label("There are no existing cabinets. Create one below for your avatar:");
                selectedCreateCabinetGameObject = (GameObject)EditorGUILayout.ObjectField("Avatar", selectedCreateCabinetGameObject, typeof(GameObject), true);
                if (GUILayout.Button("Create cabinet") && selectedCreateCabinetGameObject != null)
                {
                    cabinetPresenter.GetAvatarCabinet(selectedCreateCabinetGameObject);
                }
            }
            else
            {
                GUILayout.Label("Hello World");
            }
        }
    }
}
