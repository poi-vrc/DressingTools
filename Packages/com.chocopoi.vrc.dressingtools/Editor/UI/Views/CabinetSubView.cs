using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Applier;
using Chocopoi.DressingTools.Applier.Default;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
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

        private DTApplierSettings applierSettings = null;

        public CabinetSubView(IMainView mainView, IMainPresenter mainPresenter)
        {
            cabinetPresenter = new CabinetPresenter(this);
            this.mainPresenter = mainPresenter;
        }

        private DTCabinetApplierMode ConvertIntToApplierMode(int applierMode)
        {
            switch (applierMode)
            {
                default:
                case 0:
                    return DTCabinetApplierMode.LateApply;
                case 1:
                    return DTCabinetApplierMode.ApplyImmediately;
            }
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

            var refreshCabinetNeeded = false;

            {
                // list all appliers
                string[] applierKeys = DTCabinet.GetApplierKeys();
                string selectedApplierKey = DTCabinet.GetApplierKeyByTypeName(cabinet.applierName);
                int selectedApplierIndex = EditorGUILayout.Popup("Appliers", selectedApplierKey != null ? Array.IndexOf(applierKeys, selectedApplierKey) : 0, applierKeys);
                string newSelectedApplierKey = applierKeys[selectedApplierIndex];
                var applier = DTCabinet.GetApplierByKey(newSelectedApplierKey);
                cabinet.applierName = applier.GetType().FullName;
                if (newSelectedApplierKey != selectedApplierKey)
                {
                    // wipe old settings if applier is changed
                    applierSettings = null;
                    cabinet.serializedApplierSettings = null;
                }

                // initialize applier settings
                if (applier is DTDefaultApplier)
                {
                    if (applierSettings == null || !(applierSettings is DTDefaultApplierSettings))
                    {
                        applierSettings = applier.DeserializeSettings(cabinet.serializedApplierSettings ?? "{}");
                        if (applierSettings == null)
                        {
                            applierSettings = new DTDefaultApplierSettings();
                        }
                        refreshCabinetNeeded = true;
                    }
                }

                // draw applier settings
                if (applierSettings.DrawEditorGUI())
                {
                    // serialize if modified
                    cabinet.serializedApplierSettings = JsonConvert.SerializeObject(applierSettings);
                    refreshCabinetNeeded = true;
                }
            }

            var newApplierMode = ConvertIntToApplierMode(EditorGUILayout.Popup("Applier Mode", (int)cabinet.applierMode, new string[] { "Late apply", "Apply immediately" }));
            if (newApplierMode != cabinet.applierMode)
            {
                refreshCabinetNeeded = true;
            }
            cabinet.applierMode = newApplierMode;

            var wearablesToRemove = new List<DTCabinetWearable>();

            foreach (var wearable in cabinet.wearables)
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
                cabinet.RemoveWearable(wearable);
            }

            // refresh the cabinet if any removed
            if (wearablesToRemove.Count > 0)
            {
                refreshCabinetNeeded = true;
            }

            if (GUILayout.Button("Add Wearable"))
            {
                // start dressing
                mainPresenter.StartDressingWizard();
            }

            if (refreshCabinetNeeded)
            {
                // refresh cabinet if needed
                EditorUtility.DisplayProgressBar("DressingTools", "Refreshing cabinet...", 0);
                cabinet.RefreshCabinet();
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
