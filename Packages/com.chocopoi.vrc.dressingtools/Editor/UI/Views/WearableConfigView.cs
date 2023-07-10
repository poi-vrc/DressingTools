using System;
using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class WearableConfigViewContainer
    {
        public GameObject targetAvatar;
        public GameObject targetWearable;
        public DTWearableConfig config;
    }

    internal class WearableConfigView : IWearableConfigView
    {
        private static readonly I18n t = I18n.GetInstance();

        private WearableConfigPresenter wearableConfigPresenter;

        private WearableConfigViewContainer container;

        // used to detect changes and regenerate mappings
        private GameObject lastTargetAvatar;

        private GameObject lastTargetWearable;

        private int selectedWearableType;

        private string selectedDresserName = "Default";

        private DTDresserSettings dresserSettings = null;

        private DTReport dresserReport = null;

        private bool foldoutMetaInfo = false;

        private bool foldoutMapping = true;

        private bool foldoutGeneric = true;

        private bool foldoutDresserReportLogEntries = false;

        private bool foldoutAnimationGeneration = false;

        private bool foldoutAnimationGenerationAvatarOnWear = false;

        private bool foldoutAnimationGenerationWearableOnWear = false;

        private bool foldoutAnimationGenerationBlendshapeSync = false;

        private bool foldoutTargetAvatarConfigs = false;

        private bool foldoutAvatarAnimationPresetToggles = false;

        private bool foldoutAvatarAnimationPresetBlendshapes = false;

        private bool foldoutWearableAnimationPresetToggles = false;

        private bool foldoutWearableAnimationPresetBlendshapes = false;

        private bool regenerateMappingsNeeded = false;

        public WearableConfigView(WearableConfigViewContainer container)
        {
            wearableConfigPresenter = new WearableConfigPresenter(this);
            this.container = container;
        }

        private void DrawTypeGenericGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutGeneric = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutGeneric, "Move To Avatar");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutGeneric)
            {
                var root = container.targetAvatar?.transform;

                if (root != null)
                {
                    var lastObj = container.config.avatarPath != null ? root.Find(container.config.avatarPath)?.gameObject : null;
                    var newObj = (GameObject)EditorGUILayout.ObjectField("Move To", lastObj, typeof(GameObject), true);
                    if (lastObj != newObj && isGrandParent(root, newObj.transform))
                    {
                        // renew path if changed
                        container.config.avatarPath = AnimationUtils.GetRelativePath(newObj.transform, root);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Please select an avatar first.", MessageType.Error);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawDresserReportGUI()
        {
            if (dresserReport != null)
            {
                //Result

                switch (dresserReport.Result)
                {
                    case DTReportResult.InvalidSettings:
                        EditorGUILayout.HelpBox(t._("helpbox_error_check_result_invalid_settings"), MessageType.Error);
                        break;
                    case DTReportResult.Incompatible:
                        EditorGUILayout.HelpBox(t._("helpbox_error_check_result_incompatible"), MessageType.Error);
                        break;
                    case DTReportResult.Ok:
                        EditorGUILayout.HelpBox(t._("helpbox_info_check_result_ok"), MessageType.Info);
                        break;
                    case DTReportResult.Compatible:
                        EditorGUILayout.HelpBox(t._("helpbox_warn_check_result_compatible"), MessageType.Warning);
                        break;
                }

                EditorGUILayout.Separator();

                var logEntries = dresserReport.GetLogEntriesAsDictionary();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Errors: " + (logEntries.ContainsKey(DTReportLogType.Error) ? logEntries[DTReportLogType.Error].Count : 0));
                GUILayout.Label("Warnings: " + (logEntries.ContainsKey(DTReportLogType.Warning) ? logEntries[DTReportLogType.Warning].Count : 0));
                GUILayout.Label("Infos: " + (logEntries.ContainsKey(DTReportLogType.Info) ? logEntries[DTReportLogType.Info].Count : 0));
                EditorGUILayout.EndHorizontal();

                foldoutDresserReportLogEntries = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutDresserReportLogEntries, "Logs");
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (foldoutDresserReportLogEntries)
                {
                    if (logEntries.ContainsKey(DTReportLogType.Error))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Error])
                        {
                            EditorGUILayout.HelpBox(string.Format("({0}) {1}", logEntry.code.ToString("X4"), logEntry.message), MessageType.Error);
                        }
                    }

                    if (logEntries.ContainsKey(DTReportLogType.Warning))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Warning])
                        {
                            EditorGUILayout.HelpBox(string.Format("({0}) {1}", logEntry.code.ToString("X4"), logEntry.message), MessageType.Warning);
                        }
                    }

                    if (logEntries.ContainsKey(DTReportLogType.Info))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Info])
                        {
                            EditorGUILayout.HelpBox(string.Format("({0}) {1}", logEntry.code.ToString("X4"), logEntry.message), MessageType.Info);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_no_check_report"), MessageType.Warning);
            }
        }

        private void DrawTypeArmatureMappingFoldout()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutMapping = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutMapping, "Armature/Root Objects Mapping");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutMapping)
            {
                // list all available dressers
                string[] dresserKeys = wearableConfigPresenter.GetAvailableDresserKeys();
                var selectedDresserIndex = EditorGUILayout.Popup("Dressers", Array.IndexOf(dresserKeys, selectedDresserName), dresserKeys);

                if (dresserKeys[selectedDresserIndex] != selectedDresserName)
                {
                    // regenerate on dresser change
                    regenerateMappingsNeeded = true;
                }
                selectedDresserName = dresserKeys[selectedDresserIndex];

                var dresser = wearableConfigPresenter.GetDresserByName(selectedDresserName);

                // set the type name to config
                container.config.dresserName = dresser.GetType().FullName;

                // Initialize dresser settings
                if (dresser is DTDefaultDresser && !(dresserSettings is DTDefaultDresserSettings))
                {
                    dresserSettings = new DTDefaultDresserSettings
                    {
                        // TODO: constant defaults?
                        avatarArmatureName = "Armature",
                        wearableArmatureName = "Armature",
                        dynamicsOption = DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint
                    };
                }

                // draw the dresser settings GUI and regenerate if modified
                dresserSettings.targetAvatar = container.targetAvatar;
                dresserSettings.targetWearable = container.targetWearable;
                regenerateMappingsNeeded |= dresserSettings.DrawEditorGUI();

                EditorGUILayout.Separator();

                // generate bone mappings

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Regenerate Mappings"))
                {
                    regenerateMappingsNeeded = true;
                }
                if (GUILayout.Button("View/Edit Mappings"))
                {
                    wearableConfigPresenter.StartMappingEditor();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(dresserReport == null);
                if (GUILayout.Button("View Report"))
                {
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(true);
                if (GUILayout.Button("Test Now"))
                {
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                DTUtils.DrawHorizontalLine();

                DrawDresserReportGUI();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTypeArmatureGUI()
        {
            DrawTypeArmatureMappingFoldout();
        }

        private bool isGrandParent(Transform grandParent, Transform grandChild)
        {
            var p = grandChild.parent;
            while (p != null)
            {
                if (p == grandParent)
                {
                    return true;
                }
                p = p.parent;
            }
            return false;
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
                    if (lastObj != newObj && isGrandParent(root, newObj.transform))
                    {
                        // renew path if changed
                        toggle.path = AnimationUtils.GetRelativePath(newObj.transform, root);
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
                    if (newObj != null && lastObj != newObj && isGrandParent(root, newObj.transform) && mesh != null)
                    {
                        // renew path if changed
                        blendshape.path = AnimationUtils.GetRelativePath(newObj.transform, root);
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

                        var selectedBlendshapeIndex = Array.IndexOf(names, blendshape.blendshapeName);
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

        private void DrawAnimationGenerationAvatarOnWear()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutAnimationGenerationAvatarOnWear = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAnimationGenerationAvatarOnWear, "Avatar Animation On Wear");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutAnimationGenerationAvatarOnWear)
            {
                if (container.config.avatarAnimationOnWear == null)
                {
                    container.config.avatarAnimationOnWear = new DTAnimationPreset()
                    {
                        toggles = new DTAnimationToggle[0],
                        blendshapes = new DTAnimationBlendshapeValue[0]
                    };
                }

                if (container.targetAvatar != null)
                {
                    DrawAnimationPreset(container.targetAvatar.transform, container.config.avatarAnimationOnWear, ref foldoutAvatarAnimationPresetToggles, ref foldoutAvatarAnimationPresetBlendshapes);
                }
                else
                {
                    EditorGUILayout.HelpBox("Cannot render preset without a target avatar selected.", MessageType.Error);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAnimationGenerationWearableOnWear()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutAnimationGenerationWearableOnWear = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAnimationGenerationWearableOnWear, "Wearable Animation On Wear");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutAnimationGenerationWearableOnWear)
            {
                if (container.config.wearableAnimationOnWear == null)
                {
                    container.config.wearableAnimationOnWear = new DTAnimationPreset()
                    {
                        toggles = new DTAnimationToggle[0],
                        blendshapes = new DTAnimationBlendshapeValue[0]
                    };
                }

                if (container.targetWearable != null)
                {
                    DrawAnimationPreset(container.targetWearable.transform, container.config.wearableAnimationOnWear, ref foldoutWearableAnimationPresetToggles, ref foldoutWearableAnimationPresetBlendshapes);
                }
                else
                {
                    EditorGUILayout.HelpBox("Cannot render preset without a target wearable selected.", MessageType.Error);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAnimationGenerationBlendshapeSync()
        {
            var avatarRoot = container.targetAvatar?.transform;
            var wearableRoot = container.targetWearable?.transform;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutAnimationGenerationBlendshapeSync = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAnimationGenerationBlendshapeSync, "Blendshape Sync");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutAnimationGenerationBlendshapeSync)
            {
                if (container.config.blendshapeSyncs == null)
                {
                    container.config.blendshapeSyncs = new DTAnimationBlendshapeSync[0];
                }

                if (avatarRoot == null || wearableRoot == null)
                {
                    EditorGUILayout.HelpBox("Cannot render blendshape sync editor without a target avatar and a target wearble selected.", MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox("The object must be a child or grand-child of the root. Or it will not be selected.", MessageType.Info);

                    if (GUILayout.Button("+ Add", GUILayout.ExpandWidth(false)))
                    {
                        var newArray = new DTAnimationBlendshapeSync[container.config.blendshapeSyncs.Length + 1];
                        container.config.blendshapeSyncs.CopyTo(newArray, 0);
                        newArray[newArray.Length - 1] = new DTAnimationBlendshapeSync();
                        container.config.blendshapeSyncs = newArray;
                    }

                    var toRemove = new List<DTAnimationBlendshapeSync>();

                    foreach (var blendshapeSync in container.config.blendshapeSyncs)
                    {
                        EditorGUILayout.BeginHorizontal();

                        var lastAvatarObj = blendshapeSync.avatarPath != null ? avatarRoot.Find(blendshapeSync.avatarPath)?.gameObject : null;
                        GUILayout.Label("Avatar:");
                        var newAvatarObj = (GameObject)EditorGUILayout.ObjectField(lastAvatarObj, typeof(GameObject), true);
                        var avatarMesh = newAvatarObj?.GetComponent<SkinnedMeshRenderer>()?.sharedMesh;
                        if (lastAvatarObj != newAvatarObj && isGrandParent(avatarRoot, newAvatarObj.transform) && avatarMesh != null)
                        {
                            // renew path if changed
                            blendshapeSync.avatarPath = AnimationUtils.GetRelativePath(newAvatarObj.transform, avatarRoot);
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
                            var selectedAvatarBlendshapeIndex = Array.IndexOf(avatarBlendshapeNames, blendshapeSync.avatarBlendshapeName);
                            if (selectedAvatarBlendshapeIndex == -1)
                            {
                                selectedAvatarBlendshapeIndex = 0;
                            }
                            selectedAvatarBlendshapeIndex = EditorGUILayout.Popup(selectedAvatarBlendshapeIndex, avatarBlendshapeNames);
                            blendshapeSync.avatarBlendshapeName = avatarBlendshapeNames[selectedAvatarBlendshapeIndex];
                        }

                        var lastWearableObj = blendshapeSync.wearablePath != null ? wearableRoot.Find(blendshapeSync.wearablePath)?.gameObject : null;
                        GUILayout.Label("Wearable:");
                        var newWearableObj = (GameObject)EditorGUILayout.ObjectField(lastWearableObj, typeof(GameObject), true);
                        var wearableMesh = newWearableObj?.GetComponent<SkinnedMeshRenderer>()?.sharedMesh;
                        if (lastWearableObj != newWearableObj && isGrandParent(wearableRoot, newWearableObj.transform) && wearableMesh != null)
                        {
                            // renew path if changed
                            blendshapeSync.wearablePath = AnimationUtils.GetRelativePath(newWearableObj.transform, wearableRoot);
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
                            var selectedWearableBlendshapeIndex = Array.IndexOf(wearableBlendshapeNames, blendshapeSync.wearableBlendshapeName);
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
                        var list = new List<DTAnimationBlendshapeSync>(container.config.blendshapeSyncs);
                        list.Remove(blendshapeSync);
                        container.config.blendshapeSyncs = list.ToArray();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        private void DrawAnimationGenerationGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutAnimationGeneration = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAnimationGeneration, "Animation Generation");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutAnimationGeneration)
            {
                DrawAnimationGenerationAvatarOnWear();
                DrawAnimationGenerationWearableOnWear();
                DrawAnimationGenerationBlendshapeSync();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAvatarConfigsGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutTargetAvatarConfigs = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTargetAvatarConfigs, "Target Avatar Configurations");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutTargetAvatarConfigs)
            {
                EditorGUILayout.TextField("Name", "");
                EditorGUILayout.TextField("Armature Name", "");
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawMetaInfoGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutMetaInfo = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutMetaInfo, "Meta Information");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutMetaInfo)
            {
                EditorGUILayout.TextField("Name", "");
                EditorGUILayout.TextField("Author", "");
                GUILayout.Label("Description");
                EditorGUILayout.TextArea("");
            }
            EditorGUILayout.EndVertical();
        }

        public void OnGUI()
        {
            var newSelectedWearableType = EditorGUILayout.Popup("Wearable Type", selectedWearableType, new string[] { "Generic", "Armature-based" });

            if (newSelectedWearableType == 0) // Generic
            {
                DrawTypeGenericGUI();
            }
            else if (newSelectedWearableType == 1) // Armature-based
            {
                DrawTypeArmatureGUI();

                // detect object reference change or wearable type change
                if (newSelectedWearableType != selectedWearableType || lastTargetAvatar != container.targetAvatar || lastTargetWearable != container.targetWearable)
                {
                    // regenerate on object reference change
                    regenerateMappingsNeeded = true;
                    lastTargetAvatar = container.targetAvatar;
                    lastTargetWearable = container.targetWearable;
                }

                // regenerate on flag
                if (regenerateMappingsNeeded)
                {
                    var dresser = wearableConfigPresenter.GetDresserByName(selectedDresserName);
                    dresserReport = wearableConfigPresenter.GenerateDresserMappings(dresser, dresserSettings);
                    regenerateMappingsNeeded = false;
                }
            }
            selectedWearableType = newSelectedWearableType;

            DrawAnimationGenerationGUI();

            DrawAvatarConfigsGUI();

            DrawMetaInfoGUI();
        }
    }
}
