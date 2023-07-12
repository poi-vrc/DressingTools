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
        private static readonly Dictionary<string, IDTApplier> appliers = new Dictionary<string, IDTApplier>
        {
            { "Default", new DTDefaultApplier() }
        };

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

        private string GetApplierKeyByTypeName(string typeName)
        {
            foreach (var applier in appliers)
            {
                var type = applier.Value.GetType();
                if (type.Name == typeName || type.FullName == typeName)
                {
                    return applier.Key;
                }
            }
            return null;
        }

        private DTCabinetApplierMode ConvertIntToApplierMode(int applierMode)
        {
            switch (applierMode)
            {
                default:
                case 0:
                    return DTCabinetApplierMode.LateApplyFullyIsolated;
                case 1:
                    return DTCabinetApplierMode.LateApplyEmbedScripts;
                case 2:
                    return DTCabinetApplierMode.ApplyImmediately;
            }
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

            var cabinet = cabinets[selectedCabinetIndex];

            cabinet.avatarGameObject = (GameObject)EditorGUILayout.ObjectField("Avatar", cabinet.avatarGameObject, typeof(GameObject), true);
            cabinet.avatarArmatureName = EditorGUILayout.TextField("Armature Name", cabinet.avatarArmatureName);

            {
                // list all appliers
                string[] applierKeys = new string[appliers.Count];
                appliers.Keys.CopyTo(applierKeys, 0);
                string selectedApplierKey = GetApplierKeyByTypeName(cabinet.applierName);
                int selectedApplierIndex = EditorGUILayout.Popup("Appliers", selectedApplierKey != null ? Array.IndexOf(applierKeys, selectedApplierKey) : 0, applierKeys);
                string newSelectedApplierKey = applierKeys[selectedApplierIndex];
                var applier = appliers[newSelectedApplierKey];
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
                    if (applierSettings == null)
                    {
                        applierSettings = JsonConvert.DeserializeObject<DTDefaultApplierSettings>(cabinet.serializedApplierSettings ?? "{}");
                    }
                }

                // draw applier settings
                if (applierSettings.DrawEditorGUI())
                {
                    // serialize if modified
                    cabinet.serializedApplierSettings = JsonConvert.SerializeObject(applierSettings);
                }
            }

            cabinet.applierMode = ConvertIntToApplierMode(EditorGUILayout.Popup("Applier Mode", (int)cabinet.applierMode, new string[] { "Late apply (Fully isolated)", "Late apply (Embed scripts to avatar)", "Apply immediately" }));

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
                cabinet.wearables.Remove(wearable);
            }

            if (GUILayout.Button("Add Wearable"))
            {
                // start dressing
                mainPresenter.StartDressingWizard();
            }
        }
    }
}
