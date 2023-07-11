using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Applier;
using Chocopoi.DressingTools.Applier.Default;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UI;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    [CustomEditor(typeof(DTCabinet))]
    public class DTCabinetEditor : Editor
    {
        private static readonly Dictionary<string, IDTApplier> appliers = new Dictionary<string, IDTApplier>
        {
            { "Default", new DTDefaultApplier() }
        };

        private DTApplierSettings applierSettings = null;

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

        public override void OnInspectorGUI()
        {
            // show the tool logo
            DTLogo.Show();

            var cabinet = (DTCabinet)target;

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
        }
    }
}
