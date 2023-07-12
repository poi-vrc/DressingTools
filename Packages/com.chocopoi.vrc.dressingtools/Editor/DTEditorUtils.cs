using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DTEditorUtils
    {

        //Reference: https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/#post-3416790
        public static void DrawHorizontalLine(int i_height = 1)
        {
            EditorGUILayout.Separator();
            var rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Separator();
        }

        public static DTCabinet[] GetAllCabinets()
        {
            return Object.FindObjectsOfType<DTCabinet>();
        }

        public static void ReadOnlyTextField(string label, string text)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                EditorGUILayout.SelectableLabel(text, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            EditorGUILayout.EndHorizontal();
        }

        public static DTCabinet GetAvatarCabinet(GameObject avatar, bool createIfNotExists = false)
        {
            // Find DTContainer in the scene, if not, create one
            var container = Object.FindObjectOfType<DTContainer>();

            if (container == null)
            {
                if (!createIfNotExists)
                {
                    return null;
                }

                var gameObject = new GameObject("DressingTools");
                container = gameObject.AddComponent<DTContainer>();
            }

            // Find cabinets in the container
            DTCabinet[] cabinets = container.gameObject.GetComponentsInChildren<DTCabinet>();

            foreach (var cabinet in cabinets)
            {
                if (cabinet.avatarGameObject == avatar)
                {
                    return cabinet;
                }
            }

            if (!createIfNotExists)
            {
                return null;
            }

            // create new cabinet if not exist
            var cabinetGameObject = new GameObject("Cabinet_" + avatar.name);
            cabinetGameObject.transform.SetParent(container.transform);

            var newCabinet = cabinetGameObject.AddComponent<DTCabinet>();

            // TODO: read default config, scan for armature names?
            newCabinet.avatarGameObject = avatar;
            newCabinet.avatarArmatureName = "Armature";
            newCabinet.applierMode = DTCabinetApplierMode.LateApply;
            newCabinet.serializedApplierSettings = "{}";

            return newCabinet;
        }
    }
}
