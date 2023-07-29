using System.Collections.Generic;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Modules
{
    [CustomModuleEditor(typeof(AnimationGenerationModule))]
    public class AnimationGenerationModuleEditor : ModuleEditor
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        private bool foldoutAnimationGenerationAvatarOnWear = false;

        private bool foldoutAnimationGenerationWearableOnWear = false;

        private bool foldoutAvatarAnimationPresetToggles = false;

        private bool foldoutAvatarAnimationPresetBlendshapes = false;

        private bool foldoutWearableAnimationPresetToggles = false;

        private bool foldoutWearableAnimationPresetBlendshapes = false;

        public AnimationGenerationModuleEditor(DTWearableModuleBase target, DTWearableConfig config) : base(target, config)
        {
        }

        private void DrawAnimationPresetToggles(Transform root, DTAnimationPreset preset, ref bool foldoutAnimationPresetToggles)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutAnimationPresetToggles = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAnimationPresetToggles, "Toggles");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutAnimationPresetToggles)
            {
                EditorGUILayout.HelpBox("The object must be a child or grand-child of the root. Or it will not be selected.", MessageType.Info);

                if (GUILayout.Button("+ Add", GUILayout.ExpandWidth(false)))
                {
                    var newArray = new DTAnimationToggle[preset.toggles.Length + 1];
                    preset.toggles.CopyTo(newArray, 0);
                    newArray[newArray.Length - 1] = new DTAnimationToggle();
                    preset.toggles = newArray;
                }

                var toRemove = new List<DTAnimationToggle>();

                foreach (var toggle in preset.toggles)
                {
                    EditorGUILayout.BeginHorizontal();

                    var lastObj = toggle.path != null ? root.Find(toggle.path)?.gameObject : null;
                    var newObj = (GameObject)EditorGUILayout.ObjectField(lastObj, typeof(GameObject), true);
                    if (lastObj != newObj && DTRuntimeUtils.IsGrandParent(root, newObj.transform))
                    {
                        // renew path if changed
                        toggle.path = DTRuntimeUtils.GetRelativePath(newObj.transform, root);
                    }

                    toggle.state = EditorGUILayout.Toggle(toggle.state);
                    if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                    {
                        toRemove.Add(toggle);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // remove the queued toggles
                foreach (var toggle in toRemove)
                {
                    var list = new List<DTAnimationToggle>(preset.toggles);
                    list.Remove(toggle);
                    preset.toggles = list.ToArray();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAnimationPresetBlendshapes(Transform root, DTAnimationPreset preset, ref bool foldoutAnimationPresetBlendshapes)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutAnimationPresetBlendshapes = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAnimationPresetBlendshapes, "Blendshapes");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutAnimationPresetBlendshapes)
            {
                EditorGUILayout.HelpBox("The object must be a child or grand-child of the root, and has a SkinnedMeshRenderer. Or it will not be selected.", MessageType.Info);

                if (GUILayout.Button("+ Add", GUILayout.ExpandWidth(false)))
                {
                    var newArray = new DTAnimationBlendshapeValue[preset.blendshapes.Length + 1];
                    preset.blendshapes.CopyTo(newArray, 0);
                    newArray[newArray.Length - 1] = new DTAnimationBlendshapeValue();
                    preset.blendshapes = newArray;
                }

                var toRemove = new List<DTAnimationBlendshapeValue>();

                foreach (var blendshape in preset.blendshapes)
                {
                    EditorGUILayout.BeginHorizontal();

                    var lastObj = blendshape.path != null ? root.Find(blendshape.path)?.gameObject : null;
                    var newObj = (GameObject)EditorGUILayout.ObjectField(lastObj, typeof(GameObject), true);
                    var mesh = newObj?.GetComponent<SkinnedMeshRenderer>()?.sharedMesh;
                    if (newObj != null && lastObj != newObj && DTRuntimeUtils.IsGrandParent(root, newObj.transform) && mesh != null)
                    {
                        // renew path if changed
                        blendshape.path = DTRuntimeUtils.GetRelativePath(newObj.transform, root);
                    }

                    if (mesh == null || mesh.blendShapeCount == 0)
                    {
                        // empty placeholder
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.Popup(0, new string[] { "---" });
                        EditorGUILayout.Slider(0, 0, 100);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        string[] names = new string[mesh.blendShapeCount];
                        for (var i = 0; i < names.Length; i++)
                        {
                            names[i] = mesh.GetBlendShapeName(i);
                        }

                        var selectedBlendshapeIndex = System.Array.IndexOf(names, blendshape.blendshapeName);
                        if (selectedBlendshapeIndex == -1)
                        {
                            selectedBlendshapeIndex = 0;
                        }

                        selectedBlendshapeIndex = EditorGUILayout.Popup(selectedBlendshapeIndex, names);
                        blendshape.blendshapeName = names[selectedBlendshapeIndex];
                        blendshape.value = EditorGUILayout.Slider(blendshape.value, 0, 100);
                    }

                    if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                    {
                        toRemove.Add(blendshape);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // remove the queued blendshapes
                foreach (var blendshape in toRemove)
                {
                    var list = new List<DTAnimationBlendshapeValue>(preset.blendshapes);
                    list.Remove(blendshape);
                    preset.blendshapes = list.ToArray();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAnimationPreset(Transform root, DTAnimationPreset preset, ref bool foldoutAnimationPresetToggles, ref bool foldoutAnimationPresetBlendshapes)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Popup("Saved Presets", 0, new string[] { "---" });
            GUILayout.Button("Save", GUILayout.ExpandWidth(false));
            GUILayout.Button("Delete", GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            DrawAnimationPresetToggles(root, preset, ref foldoutAnimationPresetToggles);
            DrawAnimationPresetBlendshapes(root, preset, ref foldoutAnimationPresetBlendshapes);
        }

        private void DrawAnimationGenerationAvatarOnWear(AnimationGenerationModule module, GameObject targetAvatar)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutAnimationGenerationAvatarOnWear = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAnimationGenerationAvatarOnWear, "Avatar Animation On Wear");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutAnimationGenerationAvatarOnWear)
            {
                if (targetAvatar != null)
                {
                    DrawAnimationPreset(targetAvatar.transform, module.avatarAnimationOnWear, ref foldoutAvatarAnimationPresetToggles, ref foldoutAvatarAnimationPresetBlendshapes);
                }
                else
                {
                    EditorGUILayout.HelpBox("Cannot render preset without a target avatar selected.", MessageType.Error);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAnimationGenerationWearableOnWear(AnimationGenerationModule module, GameObject targetWearable)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutAnimationGenerationWearableOnWear = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAnimationGenerationWearableOnWear, "Wearable Animation On Wear");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutAnimationGenerationWearableOnWear)
            {
                if (targetWearable != null)
                {
                    DrawAnimationPreset(targetWearable.transform, module.wearableAnimationOnWear, ref foldoutWearableAnimationPresetToggles, ref foldoutWearableAnimationPresetBlendshapes);
                }
                else
                {
                    EditorGUILayout.HelpBox("Cannot render preset without a target wearable selected.", MessageType.Error);
                }
            }
            EditorGUILayout.EndVertical();
        }

        public override bool OnGUI(GameObject avatarGameObject, GameObject wearableGameObject)
        {
            var module = (AnimationGenerationModule)target;

            DrawAnimationGenerationAvatarOnWear(module, avatarGameObject);
            DrawAnimationGenerationWearableOnWear(module, wearableGameObject);
            // TODO: customizables

            // TODO: detect modification
            return false;
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
