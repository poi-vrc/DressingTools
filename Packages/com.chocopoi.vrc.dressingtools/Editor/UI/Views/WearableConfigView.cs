﻿using System;
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

        private bool metaInfoUseWearableName = true;

        private bool targetAvatarConfigUseAvatarName = true;

        private GameObject guidReferencePrefab = null;

        public WearableConfigView(WearableConfigViewContainer container)
        {
            wearableConfigPresenter = new WearableConfigPresenter(this);
            this.container = container;

            // check if meta info name is customized
            if (container.targetWearable != null && container.config.info.name != container.targetWearable.name)
            {
                metaInfoUseWearableName = false;
            }

            // check if target avatar name is customized
            if (container.targetAvatar != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(container.targetAvatar));
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (guid != null && guid != "")
                {
                    var avatarConfig = FindAvatarConfigByGuid(guid);
                    if (avatarConfig != null)
                    {
                        targetAvatarConfigUseAvatarName = avatarConfig.name == container.targetAvatar.name;
                    }
                }
            }
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

        private DTAvatarConfig FindAvatarConfigByGuid(string guid)
        {
            foreach (var avatarConfig in container.config.targetAvatarConfigs)
            {
                if (avatarConfig.guid == guid)
                {
                    return avatarConfig;
                }
            }
            return null;
        }

        private void DrawAvatarConfigsGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutTargetAvatarConfigs = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTargetAvatarConfigs, "Target Avatar Configurations");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutTargetAvatarConfigs)
            {
                EditorGUILayout.HelpBox("This allows other users to be able to find your configuration for their avatars and wearables once uploaded.", MessageType.Info);

                if (container.targetAvatar == null || container.targetWearable == null)
                {
                    EditorGUILayout.HelpBox("Target avatar and wearable cannot be empty to access this editor.", MessageType.Error);
                }
                else
                {
                    guidReferencePrefab = (GameObject)EditorGUILayout.ObjectField("GUID Reference Prefab", guidReferencePrefab, typeof(GameObject), true);

                    var assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(guidReferencePrefab ?? container.targetAvatar));
                    var guid = AssetDatabase.AssetPathToGUID(assetPath);
                    var invalidGuid = guid == null || guid == "";
                    if (invalidGuid)
                    {
                        EditorGUILayout.HelpBox("Your avatar is unpacked and the GUID cannot be found automatically. To help other online users to find your configuration, drag your avatar original unpacked prefab here to get a GUID.", MessageType.Warning);
                    }
                    else
                    {
                        DTUtils.ReadOnlyTextField("GUID", guid);

                        var avatarConfig = FindAvatarConfigByGuid(guid);

                        if (avatarConfig == null)
                        {
                            avatarConfig = new DTAvatarConfig()
                            {
                                guid = guid
                            };
                            var newArray = new DTAvatarConfig[container.config.targetAvatarConfigs.Length + 1];
                            container.config.targetAvatarConfigs.CopyTo(newArray, 0);
                            container.config.targetAvatarConfigs = newArray;
                            newArray[newArray.Length - 1] = avatarConfig;
                        }

                        targetAvatarConfigUseAvatarName = EditorGUILayout.ToggleLeft("Use avatar object's name", targetAvatarConfigUseAvatarName);
                        if (targetAvatarConfigUseAvatarName)
                        {
                            container.targetAvatar.name = container.targetAvatar.name;
                        }
                        EditorGUI.BeginDisabledGroup(targetAvatarConfigUseAvatarName);
                        container.targetAvatar.name = EditorGUILayout.TextField("Name", container.targetAvatar.name);
                        EditorGUI.EndDisabledGroup();

                        // try obtain armature name from cabinet
                        var cabinet = DTUtils.GetAvatarCabinet(container.targetAvatar);
                        if (cabinet == null)
                        {
                            // attempt to get from dress settings if in armature mode
                            if (selectedWearableType == 1 && dresserSettings != null)
                            {
                                avatarConfig.armatureName = dresserSettings.avatarArmatureName;
                                DTUtils.ReadOnlyTextField("Armature Name", avatarConfig.armatureName);
                            }
                            else
                            {
                                // leave it empty
                                avatarConfig.armatureName = "";
                            }
                        }
                        else
                        {
                            avatarConfig.armatureName = cabinet.avatarArmatureName;
                            DTUtils.ReadOnlyTextField("Armature Name", avatarConfig.armatureName);
                        }

                        var deltaPos = container.targetWearable.transform.position - container.targetAvatar.transform.position;
                        var deltaRotation = container.targetWearable.transform.rotation * Quaternion.Inverse(container.targetAvatar.transform.rotation);
                        avatarConfig.worldPosition = new DTAvatarConfigVector3(deltaPos);
                        avatarConfig.worldRotation = new DTAvatarConfigQuaternion(deltaRotation);
                        avatarConfig.avatarLossyScale = new DTAvatarConfigVector3(container.targetAvatar.transform.lossyScale);
                        avatarConfig.wearableLossyScale = new DTAvatarConfigVector3(container.targetWearable.transform.lossyScale);
                        DTUtils.ReadOnlyTextField("Delta World Position", deltaPos.ToString());
                        DTUtils.ReadOnlyTextField("Delta World Rotation", deltaRotation.ToString());
                        DTUtils.ReadOnlyTextField("Avatar Lossy Scale", container.targetAvatar.transform.localScale.ToString());
                        DTUtils.ReadOnlyTextField("Wearable Lossy Scale", container.targetWearable.transform.localScale.ToString());

                        EditorGUILayout.HelpBox("If you modified the FBX or created the prefab on your own, the GUID will be unlikely the original one. If that is the case, please create a new avatar configuration and drag the original prefab here.", MessageType.Info);
                    }
                }
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
                DTUtils.ReadOnlyTextField("UUID", container.config.info.uuid);

                metaInfoUseWearableName = EditorGUILayout.ToggleLeft("Use wearable object's name", metaInfoUseWearableName);
                if (metaInfoUseWearableName)
                {
                    container.config.info.name = container.targetWearable?.name;
                }
                EditorGUI.BeginDisabledGroup(metaInfoUseWearableName);
                container.config.info.name = EditorGUILayout.TextField("Name", container.config.info.name);
                EditorGUI.EndDisabledGroup();
                container.config.info.author = EditorGUILayout.TextField("Author", container.config.info.author);

                // attempts to parse and display the created time
                if (DateTime.TryParse(container.config.info.createdTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var createdTimeDt))
                {
                    DTUtils.ReadOnlyTextField("Created Time", createdTimeDt.ToLocalTime().ToString());
                }
                else
                {
                    DTUtils.ReadOnlyTextField("Created Time", "(Unable to parse date)");
                }

                // attempts to parse and display the updated time
                if (DateTime.TryParse(container.config.info.updatedTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var updatedTimeDt))
                {
                    DTUtils.ReadOnlyTextField("Updated Time", updatedTimeDt.ToLocalTime().ToString());
                }
                else
                {
                    DTUtils.ReadOnlyTextField("Updated Time", "(Unable to parse date)");
                }

                GUILayout.Label("Description");
                container.config.info.description = EditorGUILayout.TextArea(container.config.info.description);
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

        public void PrepareConfig()
        {
            container.config.configVersion = DTWearableConfig.CurrentConfigVersion;

            // update values from mapping editor container
            var mappingEditorContainer = wearableConfigPresenter.GetMappingEditorContainer();
            container.config.boneMappingMode = mappingEditorContainer.boneMappingMode;
            container.config.boneMappings = container.config.boneMappingMode != DTWearableMappingMode.Auto ? mappingEditorContainer.boneMappings?.ToArray() : new DTBoneMapping[0];
            container.config.objectMappingMode = mappingEditorContainer.objectMappingMode;
            container.config.objectMappings = container.config.objectMappingMode != DTWearableMappingMode.Auto ? mappingEditorContainer.objectMappings?.ToArray() : new DTObjectMapping[0];
        }

        public bool IsConfigReady()
        {
            // prepare the config first
            PrepareConfig();

            var ready = true;

            if (selectedWearableType == 1)
            {
                // armature mode
                ready &= dresserReport != null && (dresserReport.Result == DTReportResult.Ok || dresserReport.Result == DTReportResult.Compatible) && container.config.boneMappings != null && container.config.objectMappings != null;
            }

            return ready;
        }
    }
}
