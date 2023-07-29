using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Newtonsoft.Json;
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

        private Dictionary<DTCabinetWearable, DTWearableConfig> deserializedConfigCache;

        public CabinetSubView(IMainView mainView, IMainPresenter mainPresenter)
        {
            cabinetPresenter = new CabinetPresenter(this);
            this.mainPresenter = mainPresenter;
            deserializedConfigCache = new Dictionary<DTCabinetWearable, DTWearableConfig>();
        }

        public void ResetAllDeserializedConfigCache()
        {
            deserializedConfigCache.Clear();
        }

        public void OnGUI()
        {
            // TODO: beautify UI, now it's so simplified for functionality development

            var cabinets = DTEditorUtils.GetAllCabinets();

            if (cabinets.Length == 0)
            {
                GUILayout.Label("There are no existing cabinets. Create one below for your avatar:");
                selectedCreateCabinetGameObject = (GameObject)EditorGUILayout.ObjectField("Avatar", selectedCreateCabinetGameObject, typeof(GameObject), true);
                if (GUILayout.Button("Create cabinet") && selectedCreateCabinetGameObject != null)
                {
                    DTEditorUtils.GetAvatarCabinet(selectedCreateCabinetGameObject, true);
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

            var cabinet = cabinets[selectedCabinetIndex];

            cabinet.avatarGameObject = (GameObject)EditorGUILayout.ObjectField("Avatar", cabinet.avatarGameObject, typeof(GameObject), true);
            cabinet.avatarArmatureName = EditorGUILayout.TextField("Armature Name", cabinet.avatarArmatureName);

            var wearablesToRemove = new List<DTCabinetWearable>();
            var wearables = cabinet.GetWearables();

            foreach (var wearable in wearables)
            {
                DTWearableConfig config = null;

                // TODO: when to clear deserialization cache?
                if (!deserializedConfigCache.ContainsKey(wearable))
                {
                    if (wearable.configJson != null)
                    {
                        config = JsonConvert.DeserializeObject<DTWearableConfig>(wearable.configJson);
                        deserializedConfigCache.Add(wearable, config);
                    }
                }
                else
                {
                    config = deserializedConfigCache[wearable];
                }

                if (config == null)
                {
                    EditorGUILayout.HelpBox("Unable to load one of the wearable configuration!", MessageType.Error);
                }
                else
                {
                    GUILayout.Label(string.Format("{0} ({1})", config.info.name, config.info.uuid));
                }
                if (GUILayout.Button("Remove"))
                {
                    wearablesToRemove.Add(wearable);
                }
            }

            // remove all pending wearable in list
            foreach (var wearable in wearablesToRemove)
            {
                DTEditorUtils.RemoveCabinetWearable(cabinet, wearable);
            }

            if (GUILayout.Button("Add Wearable"))
            {
                // start dressing
                mainPresenter.StartDressingWizard();
            }
        }
    }
}
