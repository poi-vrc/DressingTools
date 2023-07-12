using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    public class DTMappingEditorContainer
    {
        public DTDresserSettings dresserSettings;
        public DTWearableMappingMode boneMappingMode;
        public List<DTBoneMapping> boneMappings;
        public DTWearableMappingMode objectMappingMode;
        public List<DTObjectMapping> objectMappings;
    }

    public class DTMappingEditorWindow : EditorWindow
    {
        private DTMappingEditorContainer container;

        private Vector2 scrollPos;

        private Vector2 leftScrollPos;
        private Vector2 rightScrollPos;

        private int selectedBoneObjectEditor;

        public DTMappingEditorWindow()
        {
        }

        public void SetSettings(DTMappingEditorContainer container)
        {
            this.container = container;
        }

        private List<DTBoneMapping> GetAvatarBoneMapping(Transform avatarRoot, Transform targetAvatarBone)
        {
            var path = AnimationUtils.GetRelativePath(targetAvatarBone, avatarRoot);

            var boneMappings = new List<DTBoneMapping>();

            foreach (var boneMapping in container.boneMappings)
            {
                if (boneMapping.avatarBonePath == path)
                {
                    boneMappings.Add(boneMapping);
                }
            }

            return boneMappings;
        }

        private DTObjectMapping GetWearableObjectMapping(Transform wearableRoot, Transform targetWearableObject)
        {
            var path = AnimationUtils.GetRelativePath(targetWearableObject, wearableRoot);

            foreach (var objectMapping in container.objectMappings)
            {
                if (objectMapping.wearableObjectPath == path)
                {
                    return objectMapping;
                }
            }

            return null;
        }

        public void DrawWearableHierarchy(Transform wearableRoot, Transform parent)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (child.name == container.dresserSettings.wearableArmatureName)
                {
                    // skips rendering the Armature object
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(child.gameObject, typeof(GameObject), true);
                GUILayout.Label("-->", GUILayout.ExpandWidth(false));

                // backup and set indent level to zero
                var lastIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var objectMapping = GetWearableObjectMapping(wearableRoot, child);

                if (objectMapping != null)
                {
                    // the avatar root itself
                    if (objectMapping.avatarObjectPath == "" || objectMapping.avatarObjectPath == ".")
                    {
                        EditorGUILayout.ObjectField(container.dresserSettings.targetAvatar, typeof(GameObject), true);
                    }
                    else
                    {
                        EditorGUILayout.ObjectField(container.dresserSettings.targetAvatar.transform.Find(objectMapping.avatarObjectPath)?.gameObject, typeof(GameObject), true);
                    }
                }
                else
                {
                    EditorGUILayout.ObjectField(null, typeof(GameObject), true);
                }

                // restore to the previous indent level
                EditorGUI.indentLevel = lastIndentLevel;

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                DrawWearableHierarchy(wearableRoot, child);
                EditorGUI.indentLevel -= 1;
            }
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
                        EditorGUILayout.Popup((int)boneMapping.mappingType, new string[] { "Do Nothing", "Move to Avatar Bone", "ParentConstraint to Avatar Bone", "IgnoreTransform on Dynamics", "Copy Avatar Data on Dynamics" });
                        EditorGUILayout.ObjectField(container.dresserSettings.targetWearable.transform.Find(boneMapping.wearableBonePath)?.gameObject, typeof(GameObject), true);
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

        private DTWearableMappingMode ConvertIntToWearableMappingMode(int wearableMappingMode)
        {
            switch (wearableMappingMode)
            {
                case 1:
                    return DTWearableMappingMode.Override;
                case 2:
                    return DTWearableMappingMode.Manual;
                default:
                case 0:
                    return DTWearableMappingMode.Auto;
            }
        }

        private void DrawBoneObjectEditorSwitch()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Mapping Editor:");
            selectedBoneObjectEditor = GUILayout.Toolbar(selectedBoneObjectEditor, new string[] { "Armature", "Root Objects" });
            GUILayout.EndHorizontal();
        }

        private void DrawMappingHeaderHelpBoxes(DTWearableMappingMode mappingMode)
        {
            if (mappingMode == DTWearableMappingMode.Auto)
            {
                EditorGUILayout.HelpBox("In auto mode, everything is controlled by dresser and its generated mappings. To edit mappings, either use Override or even Manual mode.", MessageType.Warning);
            }
            if (mappingMode == DTWearableMappingMode.Override)
            {
                EditorGUILayout.HelpBox("In override mode, the mappings here will override the ones generated by the dresser. It could be useful for fixing some minor bone mappings.", MessageType.Info);
            }
            if (mappingMode == DTWearableMappingMode.Manual)
            {
                EditorGUILayout.HelpBox("In manual mode, the dresser is ignored and the mappings defined here will be exactly the same when applied to other avatars/users. It might cause incompatibility issues so use with caution.", MessageType.Warning);
            }
        }

        private void DrawBoneMappingEditor()
        {
            // Header
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Left: Avatar", container.dresserSettings.targetAvatar, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();
            container.boneMappingMode = ConvertIntToWearableMappingMode(EditorGUILayout.Popup("Mode", (int)container.boneMappingMode, new string[] { "Auto", "Override", "Manual" }));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Right: Wearable", container.dresserSettings.targetWearable, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            DrawMappingHeaderHelpBoxes(container.boneMappingMode);

            DTEditorUtils.DrawHorizontalLine();

            // TODO: implement final result mapping preview
            GUILayout.Toolbar(0, new string[] { "Your Mappings", "Result Mappings" });

            EditorGUILayout.Separator();

            // Bone Mappings
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUI.BeginDisabledGroup(container.boneMappingMode == DTWearableMappingMode.Auto);
            DrawAvatarHierarchy(container.dresserSettings.targetAvatar.transform, container.dresserSettings.targetAvatar.transform);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawObjectMappingEditor()
        {
            // Header
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Left: Wearable", container.dresserSettings.targetWearable, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();
            container.objectMappingMode = ConvertIntToWearableMappingMode(EditorGUILayout.Popup("Mode", (int)container.objectMappingMode, new string[] { "Auto", "Override", "Manual" }));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Right: Avatar", container.dresserSettings.targetAvatar, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            DrawMappingHeaderHelpBoxes(container.objectMappingMode);

            DTEditorUtils.DrawHorizontalLine();

            // TODO: implement final result mapping preview
            GUILayout.Toolbar(0, new string[] { "Your Mappings", "Result Mappings" });

            EditorGUILayout.Separator();

            // Object Mappings
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUI.BeginDisabledGroup(container.objectMappingMode == DTWearableMappingMode.Auto);
            DrawWearableHierarchy(container.dresserSettings.targetWearable.transform, container.dresserSettings.targetWearable.transform);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        public void OnGUI()
        {
            if (container == null)
            {
                Close();
                return;
            }

            if (container.dresserSettings == null || container.boneMappings == null || container.objectMappings == null)
            {
                EditorGUILayout.HelpBox("Bone and object mapping not available.", MessageType.Error);
                return;
            }

            // Bone/Object editor switch
            DrawBoneObjectEditorSwitch();

            // Bone editor
            if (selectedBoneObjectEditor == 0)
            {
                DrawBoneMappingEditor();
            }
            else
            {
                DrawObjectMappingEditor();
            }

            EditorGUILayout.Separator();
        }
    }
}
