using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.Reporting;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Reporting
{
    public class MissingScriptsChecker
    {
        private static Translation.I18n t = Translation.I18n.GetInstance();

        public static void ScanGameObject(GameObject gameObject, List<GameObject> missingScriptObjects)
        {
            Component[] components = gameObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    missingScriptObjects.Add(gameObject);
                    break;
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                ScanGameObject(child.gameObject, missingScriptObjects);
            }
        }

        // Temporary solution to the annoying missing script error until v2
        private class MissingScriptEditorWindow : EditorWindow
        {
            public List<GameObject> missingScripts = new List<GameObject>();

            public void OnGUI()
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_gameobjects_contain_missing_scripts"), MessageType.Warning);
                if (DressingUtils.FindType("DynamicBone") == null)
                {
                    EditorGUILayout.HelpBox(t._("helpbox_warn_missing_scripts_cannot_detect_if_dynamicbones_not_installed"), MessageType.Warning);
                }
                EditorGUILayout.Separator();
                if (GUILayout.Button(t._("button_remove_all_missing_scripts")) && EditorUtility.DisplayDialog("DressingTools", t._("dialog_dress_confirmation_content"), t._("dialog_button_yes"), t._("dialog_button_no")))
                {
                    int count = 0;
                    foreach (var obj in missingScripts)
                    {
                        count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                    }
                    if (missingScripts.Count == count)
                    {
                        EditorUtility.DisplayDialog("DressingTools", t._("dialog_missing_scripts_removal_success", count), "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("DressingTools", t._("dialog_missing_scripts_removal_failure", count), "OK");
                    }
                    missingScripts = new List<GameObject>();
                    Close();
                }
                EditorGUILayout.LabelField(t._("label_missing_scripts_click_to_locate"), EditorStyles.boldLabel);
                DressingUtils.DrawHorizontalLine();
                foreach (var obj in missingScripts)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                    EditorGUI.EndDisabledGroup();
                }
            }
        }

        private static void ShowMissingScriptWindow(List<GameObject> list)
        {
            var window = (MissingScriptEditorWindow)EditorWindow.GetWindow(typeof(MissingScriptEditorWindow));
            window.titleContent = new GUIContent("DressingTools Warning");
            window.missingScripts = list;
            window.Show();
        }

        public static bool Check(GameObject avatar, GameObject clothes)
        {
            bool dynBoneInstalled = DressingUtils.FindType("DynamicBone") != null;

            //scan missing scripts
            var allObjects = new List<GameObject>();
            ScanGameObject(avatar, allObjects);
            ScanGameObject(clothes, allObjects);

            if (allObjects.Count > 0)
            {
                ShowMissingScriptWindow(allObjects);
            }

            return dynBoneInstalled || allObjects.Count == 0;
        }
    }
}
