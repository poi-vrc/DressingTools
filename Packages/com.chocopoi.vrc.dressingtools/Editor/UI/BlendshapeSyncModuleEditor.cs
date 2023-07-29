using System.Collections.Generic;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Modules
{
    [CustomModuleEditor(typeof(BlendshapeSyncModule))]
    public class BlendshapeSyncModuleEditor : ModuleEditor
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public BlendshapeSyncModuleEditor(DTWearableModuleBase target, DTWearableConfig config) : base(target, config)
        {
        }

        public override bool OnGUI(GameObject avatarGameObject, GameObject wearableGameObject)
        {
            var module = (BlendshapeSyncModule)target;

            if (avatarGameObject == null || wearableGameObject == null)
            {
                EditorGUILayout.HelpBox("Cannot render blendshape sync editor without a target avatar and a target wearble selected.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("The object must be a child or grand-child of the root. Or it will not be selected.", MessageType.Info);

                if (GUILayout.Button("+ Add", GUILayout.ExpandWidth(false)))
                {
                    var newArray = new DTAnimationBlendshapeSync[module.blendshapeSyncs.Length + 1];
                    module.blendshapeSyncs.CopyTo(newArray, 0);
                    newArray[newArray.Length - 1] = new DTAnimationBlendshapeSync();
                    module.blendshapeSyncs = newArray;
                }

                var toRemove = new List<DTAnimationBlendshapeSync>();

                foreach (var blendshapeSync in module.blendshapeSyncs)
                {
                    EditorGUILayout.BeginHorizontal();

                    var lastAvatarObj = blendshapeSync.avatarPath != null ? avatarGameObject.transform.Find(blendshapeSync.avatarPath)?.gameObject : null;
                    GUILayout.Label("Avatar:");
                    var newAvatarObj = (GameObject)EditorGUILayout.ObjectField(lastAvatarObj, typeof(GameObject), true);
                    var avatarMesh = newAvatarObj?.GetComponent<SkinnedMeshRenderer>()?.sharedMesh;
                    if (lastAvatarObj != newAvatarObj && DTRuntimeUtils.IsGrandParent(avatarGameObject.transform, newAvatarObj.transform) && avatarMesh != null)
                    {
                        // renew path if changed
                        blendshapeSync.avatarPath = DTRuntimeUtils.GetRelativePath(newAvatarObj.transform, avatarGameObject.transform);
                    }

                    if (avatarMesh == null || avatarMesh.blendShapeCount == 0)
                    {
                        // empty placeholder
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.Popup(0, new string[] { "---" });
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        string[] avatarBlendshapeNames = new string[avatarMesh.blendShapeCount];
                        for (var i = 0; i < avatarBlendshapeNames.Length; i++)
                        {
                            avatarBlendshapeNames[i] = avatarMesh.GetBlendShapeName(i);
                        }
                        var selectedAvatarBlendshapeIndex = System.Array.IndexOf(avatarBlendshapeNames, blendshapeSync.avatarBlendshapeName);
                        if (selectedAvatarBlendshapeIndex == -1)
                        {
                            selectedAvatarBlendshapeIndex = 0;
                        }
                        selectedAvatarBlendshapeIndex = EditorGUILayout.Popup(selectedAvatarBlendshapeIndex, avatarBlendshapeNames);
                        blendshapeSync.avatarBlendshapeName = avatarBlendshapeNames[selectedAvatarBlendshapeIndex];
                    }

                    var lastWearableObj = blendshapeSync.wearablePath != null ? wearableGameObject.transform.Find(blendshapeSync.wearablePath)?.gameObject : null;
                    GUILayout.Label("Wearable:");
                    var newWearableObj = (GameObject)EditorGUILayout.ObjectField(lastWearableObj, typeof(GameObject), true);
                    var wearableMesh = newWearableObj?.GetComponent<SkinnedMeshRenderer>()?.sharedMesh;
                    if (lastWearableObj != newWearableObj && DTRuntimeUtils.IsGrandParent(wearableGameObject.transform, newWearableObj.transform) && wearableMesh != null)
                    {
                        // renew path if changed
                        blendshapeSync.wearablePath = DTRuntimeUtils.GetRelativePath(newWearableObj.transform, wearableGameObject.transform);
                    }

                    if (wearableMesh == null || wearableMesh.blendShapeCount == 0)
                    {
                        // empty placeholder
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.Popup(0, new string[] { "---" });
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        string[] wearableBlendshapeNames = new string[wearableMesh.blendShapeCount];
                        for (var i = 0; i < wearableBlendshapeNames.Length; i++)
                        {
                            wearableBlendshapeNames[i] = wearableMesh.GetBlendShapeName(i);
                        }
                        var selectedWearableBlendshapeIndex = System.Array.IndexOf(wearableBlendshapeNames, blendshapeSync.wearableBlendshapeName);
                        if (selectedWearableBlendshapeIndex == -1)
                        {
                            selectedWearableBlendshapeIndex = 0;
                        }
                        selectedWearableBlendshapeIndex = EditorGUILayout.Popup(selectedWearableBlendshapeIndex, wearableBlendshapeNames);
                        blendshapeSync.wearableBlendshapeName = wearableBlendshapeNames[selectedWearableBlendshapeIndex];
                    }

                    // TODO: custom boundaries, now simply just invert 0-100 to 100-0

                    var lastInvertedBoundaries = blendshapeSync.avatarFromValue == 0 && blendshapeSync.avatarToValue == 100 && blendshapeSync.wearableFromValue == 100 && blendshapeSync.wearableToValue == 0;
                    var newInvertedBoundaries = GUILayout.Toggle(lastInvertedBoundaries, "Inverted");

                    if (newInvertedBoundaries)
                    {
                        blendshapeSync.avatarFromValue = 0;
                        blendshapeSync.avatarToValue = 100;
                        blendshapeSync.wearableFromValue = 100;
                        blendshapeSync.wearableToValue = 0;
                    }
                    else
                    {
                        blendshapeSync.avatarFromValue = 0;
                        blendshapeSync.avatarToValue = 100;
                        blendshapeSync.wearableFromValue = 0;
                        blendshapeSync.wearableToValue = 100;
                    }

                    if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                    {
                        toRemove.Add(blendshapeSync);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // remove the queued toggles
                foreach (var blendshapeSync in toRemove)
                {
                    var list = new List<DTAnimationBlendshapeSync>(module.blendshapeSyncs);
                    list.Remove(blendshapeSync);
                    module.blendshapeSyncs = list.ToArray();
                }
            }

            return false;
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
