using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.Reporting;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Reporting
{
    public class MissingScriptsChecker
    {
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
                EditorGUILayout.HelpBox("The following GameObjects contain missing scripts, which will affect VRChat uploading and DressingTools. If it's located at \"Armature\" or prefab root, it is most likely some script-embedding tool put some scripts in it and safe to remove.", MessageType.Warning);
                if (DressingUtils.FindType("DynamicBone") == null)
                {
                    EditorGUILayout.HelpBox("We cannot detect if it is DynamicBones if it's not installed, which allow us to determine whether those missing scripts are safe to remove or not. Try installing DynamicBones if the avatar/clothes use them.", MessageType.Error);
                }
                EditorGUILayout.Separator();
                if (GUILayout.Button("Remove All Missing Scripts (Caution)") && EditorUtility.DisplayDialog("DressingTools", "Are you sure?", "Yes", "No"))
                {
                    int count = 0;
                    foreach (var obj in missingScripts)
                    {
                        count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                    }
                    if (missingScripts.Count == count)
                    {
                        EditorUtility.DisplayDialog("DressingTools", "Successfully removed " + count + " missing script(s).", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("DressingTools", "Unable to remove all scripts. Only removed " + count + " missing script(s).", "OK");
                    }
                    missingScripts = new List<GameObject>();
                    Close();
                }
                EditorGUILayout.LabelField("Click on the icons to locate them:", EditorStyles.boldLabel);
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
