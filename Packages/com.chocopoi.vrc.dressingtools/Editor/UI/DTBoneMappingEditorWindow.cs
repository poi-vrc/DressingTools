using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    public class DTBoneMappingEditorSettings
    {
        public DTDresserSettings dresserSettings;
        public List<DTBoneMapping> boneMappings;
    }

    public class DTBoneMappingEditorWindow : EditorWindow
    {
        private DTBoneMappingEditorSettings settings;

        private Vector2 scrollPos;

        private Vector2 leftScrollPos;
        private Vector2 rightScrollPos;

        public DTBoneMappingEditorWindow()
        {
        }

        public void SetSettings(DTBoneMappingEditorSettings settings)
        {
            this.settings = settings;
        }

        private List<DTBoneMapping> GetAvatarBoneMapping(Transform avatarRoot, Transform targetAvatarBone)
        {
            var path = AnimationUtils.GetRelativePath(targetAvatarBone, avatarRoot);

            var boneMappings = new List<DTBoneMapping>();

            foreach (var boneMapping in settings.boneMappings)
            {
                if (boneMapping.avatarBonePath == path)
                {
                    boneMappings.Add(boneMapping);
                }
            }

            return boneMappings;
        }

        public void DrawAvatarHierarchy(Transform avatarRoot, Transform parent)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(child.gameObject, typeof(GameObject), true);

                GUILayout.Button("+", GUILayout.ExpandWidth(false));

                // backup and set indent level to zero
                var lastIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                EditorGUILayout.BeginVertical();

                var boneMappings = GetAvatarBoneMapping(avatarRoot, child);

                if (boneMappings.Count > 0)
                {
                    foreach (var boneMapping in boneMappings)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Popup((int)boneMapping.mappingType, new string[] { "Do Nothing Usually", "Move to Avatar Bone Usually", "ParentConstraint to Avatar Bone Usually" });
                        EditorGUILayout.Popup((int)boneMapping.dynamicsBindingType, new string[] { "Do Nothing on Dynamics", "ParentConstraint on Dynamics", "IgnoreTransform on Dynamics", "Copy Avatar Data on Dynamics" });
                        EditorGUILayout.ObjectField(settings.dresserSettings.targetWearable.transform.Find(boneMapping.wearableBonePath)?.gameObject, typeof(GameObject), true);
                        GUILayout.Button("x", GUILayout.ExpandWidth(false));
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Popup(0, new string[] { "---" });
                    EditorGUILayout.Popup(0, new string[] { "---" });
                    EditorGUILayout.ObjectField(null, typeof(GameObject), true);
                    GUILayout.Button("x", GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUILayout.EndVertical();
                // restore to the previous indent level
                EditorGUI.indentLevel = lastIndentLevel;

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                DrawAvatarHierarchy(avatarRoot, child);
                EditorGUI.indentLevel -= 1;
            }
        }

        public void OnGUI()
        {
            if (settings == null)
            {
                Close();
                return;
            }

            // Bone Mappings
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Left: avatar bones
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("Avatar", settings.dresserSettings.targetAvatar, typeof(GameObject), true);
            EditorGUILayout.ObjectField("Wearable", settings.dresserSettings.targetWearable, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();
            DrawAvatarHierarchy(settings.dresserSettings.targetAvatar.transform, settings.dresserSettings.targetAvatar.transform);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }
    }
}
