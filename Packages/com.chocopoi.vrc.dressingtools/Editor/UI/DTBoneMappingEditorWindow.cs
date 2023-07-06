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
        public DTWearableBoneMappingMode wearableBoneMappingMode;
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

                // TODO: implement adding
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
                        // TODO: implement editing
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
                    // empty mapping placeholder
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

        private DTWearableBoneMappingMode ConvertIntToWearableBoneMappingMode(int wearableBoneMappingMode)
        {
            switch (wearableBoneMappingMode)
            {
                case 1:
                    return DTWearableBoneMappingMode.Override;
                case 2:
                    return DTWearableBoneMappingMode.Manual;
                default:
                case 0:
                    return DTWearableBoneMappingMode.Auto;
            }
        }

        public void OnGUI()
        {
            if (settings == null)
            {
                Close();
                return;
            }

            // Header
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("Avatar", settings.dresserSettings.targetAvatar, typeof(GameObject), true);
            settings.wearableBoneMappingMode = ConvertIntToWearableBoneMappingMode(EditorGUILayout.Popup("Mode", (int)settings.wearableBoneMappingMode, new string[] { "Auto", "Override", "Manual" }));
            EditorGUILayout.ObjectField("Wearable", settings.dresserSettings.targetWearable, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();

            if (settings.wearableBoneMappingMode == DTWearableBoneMappingMode.Auto)
            {
                EditorGUILayout.HelpBox("In auto mode, everything is controlled by dresser and its generated mappings. To edit mappings, either use Override or even Manual mode.", MessageType.Warning);
            }
            if (settings.wearableBoneMappingMode == DTWearableBoneMappingMode.Override)
            {
                EditorGUILayout.HelpBox("In override mode, the mappings here will override the ones generated by the dresser. It could be useful for fixing some minor bone mappings.", MessageType.Info);
            }
            if (settings.wearableBoneMappingMode == DTWearableBoneMappingMode.Manual)
            {
                EditorGUILayout.HelpBox("In manual mode, the dresser is ignored and the mappings defined here will be exactly the same when applied to other avatars/users. It might cause incompatibility issues so use with caution.", MessageType.Warning);
            }

            DTUtils.DrawHorizontalLine();

            // TODO: implement final result mapping preview
            GUILayout.Toolbar(0, new string[] { "Your Mappings", "Result Mappings" });

            EditorGUILayout.Separator();

            // Bone Mappings
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUI.BeginDisabledGroup(settings.wearableBoneMappingMode == DTWearableBoneMappingMode.Auto);
            DrawAvatarHierarchy(settings.dresserSettings.targetAvatar.transform, settings.dresserSettings.targetAvatar.transform);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}
