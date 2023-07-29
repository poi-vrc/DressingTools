using System;
using System.Collections.Generic;
using System.Linq;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.UI.Modules;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
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

        private static Dictionary<Type, Type> moduleEditorTypesCache = null;

        private static List<Type> availableModulesCache = null;

        private static string[] availableModuleKeysCache = null;

        private WearableConfigPresenter wearableConfigPresenter;

        private WearableConfigViewContainer container;

        private Dictionary<DTWearableModuleBase, ModuleEditor> moduleEditors;

        private int selectedAvailableModule = 0;

        private bool foldoutMetaInfo = false;

        private bool foldoutTargetAvatarConfigs = false;

        private bool metaInfoUseWearableName = true;

        private bool targetAvatarConfigUseAvatarName = true;

        private GameObject guidReferencePrefab = null;

        public WearableConfigView(WearableConfigViewContainer container)
        {
            wearableConfigPresenter = new WearableConfigPresenter(this);
            this.container = container;
            moduleEditors = new Dictionary<DTWearableModuleBase, ModuleEditor>();

            // check if meta info name is customized
            if (container.targetWearable != null && container.config.info.name != container.targetWearable.name)
            {
                metaInfoUseWearableName = false;
            }

            // check if target avatar name is customized
            if (container.targetAvatar != null)
            {
                targetAvatarConfigUseAvatarName = container.config.targetAvatarConfig.name == container.targetAvatar.name;
            }
        }

        private void UpdateTargetAvatarConfig(DTCabinet cabinet)
        {
            if (container.targetAvatar == null || container.targetWearable == null)
            {
                // we cannot do anything with target avatar and wearable null
                return;
            }

            if (targetAvatarConfigUseAvatarName)
            {
                container.config.targetAvatarConfig.name = container.targetAvatar.name;
            }

            // try obtain armature name from cabinet
            if (cabinet == null)
            {
                // leave it empty
                container.config.targetAvatarConfig.armatureName = "";
            }
            else
            {
                container.config.targetAvatarConfig.armatureName = cabinet.avatarArmatureName;
            }

            var deltaPos = container.targetWearable.transform.position - container.targetAvatar.transform.position;
            var deltaRotation = container.targetWearable.transform.rotation * Quaternion.Inverse(container.targetAvatar.transform.rotation);
            container.config.targetAvatarConfig.worldPosition = new DTAvatarConfigVector3(deltaPos);
            container.config.targetAvatarConfig.worldRotation = new DTAvatarConfigQuaternion(deltaRotation);
            container.config.targetAvatarConfig.avatarLossyScale = new DTAvatarConfigVector3(container.targetAvatar.transform.lossyScale);
            container.config.targetAvatarConfig.wearableLossyScale = new DTAvatarConfigVector3(container.targetWearable.transform.lossyScale);
        }

        private ModuleEditor CreateModuleEditor(DTWearableModuleBase module)
        {
            // prepare cache if not yet
            if (moduleEditorTypesCache == null)
            {
                moduleEditorTypesCache = new Dictionary<Type, Type>();

                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        var attributes = type.GetCustomAttributes(typeof(CustomModuleEditor), true);
                        foreach (CustomModuleEditor attribute in attributes)
                        {
                            if (moduleEditorTypesCache.ContainsKey(attribute.ModuleType))
                            {
                                Debug.LogWarning("There are more than one CustomModuleEditor pointing to the same module! Skipping: " + type.FullName);
                                continue;
                            }
                            moduleEditorTypesCache.Add(attribute.ModuleType, type);
                        }
                    }
                }
            }

            // obtain from cache and create an editor instance
            if (moduleEditorTypesCache.TryGetValue(module.GetType(), out var moduleEditorType))
            {
                return (ModuleEditor)Activator.CreateInstance(moduleEditorType, module, container.config);

            }

            return null;
        }

        private static List<Type> GetAllAvailableModules()
        {
            return System.Reflection.Assembly.GetAssembly(typeof(DTWearableModuleBase))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DTWearableModuleBase)) && !t.IsAbstract)
                .ToList();
        }

        private void DrawModulesGUI()
        {
            if (availableModulesCache == null)
            {
                availableModulesCache = GetAllAvailableModules();
                availableModuleKeysCache = new string[availableModulesCache.Count];
                for (var i = 0; i < availableModulesCache.Count; i++)
                {
                    availableModuleKeysCache[i] = availableModulesCache[i].FullName;
                }
            }

            EditorGUILayout.BeginHorizontal();
            selectedAvailableModule = EditorGUILayout.Popup("Select Module: ", selectedAvailableModule, availableModuleKeysCache);
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
            {
                var newModule = (DTWearableModuleBase)Activator.CreateInstance(availableModulesCache[selectedAvailableModule]);
                if (!newModule.AllowMultiple)
                {
                    // check if any existing type
                    foreach (var existingModule in container.config.modules)
                    {
                        if (existingModule.GetType() == newModule.GetType())
                        {
                            EditorUtility.DisplayDialog("DressingTools", "This module has been added before and cannot have multiple ones.", "OK");
                            return;
                        }
                    }
                }
                container.config.modules.Add(newModule);
            }
            EditorGUILayout.EndHorizontal();

            var toRemove = new List<DTWearableModuleBase>();

            foreach (var module in container.config.modules)
            {
                if (!moduleEditors.TryGetValue(module, out var editor))
                {
                    editor = CreateModuleEditor(module);

                    if (editor == null)
                    {
                        // unable to initialize editor;
                        EditorGUILayout.HelpBox("No editor available for the module.", MessageType.Error);
                        if (GUILayout.Button("Remove"))
                        {
                            toRemove.Add(module);
                        }
                        continue;
                    }

                    moduleEditors.Add(module, editor);
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                editor.foldout = EditorGUILayout.BeginFoldoutHeaderGroup(editor.foldout, editor.FriendlyName);
                if (GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
                {
                    toRemove.Add(module);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (editor.foldout)
                {
                    if (editor.OnGUI(container.targetAvatar, container.targetWearable))
                    {
                        // TODO: what to do if modified
                    }
                }
                EditorGUILayout.EndVertical();
            }

            // remove modules
            foreach (var module in toRemove)
            {
                container.config.modules.Remove(module);
            }
        }

        private void DrawAvatarConfigsGUI(DTCabinet cabinet)
        {
            UpdateTargetAvatarConfig(cabinet);

            // GUI
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutTargetAvatarConfigs = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTargetAvatarConfigs, "Target Avatar Configuration");
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

                    var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(guidReferencePrefab ?? container.targetAvatar);
                    var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

                    if (invalidAvatarPrefabGuid)
                    {
                        EditorGUILayout.HelpBox("Your avatar is unpacked and the GUID cannot be found automatically. To help other online users to find your configuration, drag your avatar original unpacked prefab here to get a GUID.", MessageType.Warning);
                    }

                    DTEditorUtils.ReadOnlyTextField("GUID", invalidAvatarPrefabGuid ? "(Not available)" : avatarPrefabGuid);

                    targetAvatarConfigUseAvatarName = EditorGUILayout.ToggleLeft("Use avatar object's name", targetAvatarConfigUseAvatarName);
                    EditorGUI.BeginDisabledGroup(targetAvatarConfigUseAvatarName);
                    container.config.targetAvatarConfig.name = EditorGUILayout.TextField("Name", container.config.targetAvatarConfig.name);
                    EditorGUI.EndDisabledGroup();

                    DTEditorUtils.ReadOnlyTextField("Armature Name", container.config.targetAvatarConfig.armatureName);
                    DTEditorUtils.ReadOnlyTextField("Delta World Position", container.config.targetAvatarConfig.worldPosition.ToString());
                    DTEditorUtils.ReadOnlyTextField("Delta World Rotation", container.config.targetAvatarConfig.worldRotation.ToString());
                    DTEditorUtils.ReadOnlyTextField("Avatar Lossy Scale", container.config.targetAvatarConfig.avatarLossyScale.ToString());
                    DTEditorUtils.ReadOnlyTextField("Wearable Lossy Scale", container.config.targetAvatarConfig.wearableLossyScale.ToString());

                    EditorGUILayout.HelpBox("If you modified the FBX or created the prefab on your own, the GUID will be unlikely the original one. If that is the case, please create a new avatar configuration and drag the original prefab here.", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawMetaInfoGUI()
        {
            // write info name
            if (metaInfoUseWearableName)
            {
                container.config.info.name = container.targetWearable?.name;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutMetaInfo = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutMetaInfo, "Meta Information");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutMetaInfo)
            {
                DTEditorUtils.ReadOnlyTextField("UUID", container.config.info.uuid);

                metaInfoUseWearableName = EditorGUILayout.ToggleLeft("Use wearable object's name", metaInfoUseWearableName);
                EditorGUI.BeginDisabledGroup(metaInfoUseWearableName);
                container.config.info.name = EditorGUILayout.TextField("Name", container.config.info.name);
                EditorGUI.EndDisabledGroup();
                container.config.info.author = EditorGUILayout.TextField("Author", container.config.info.author);

                // attempts to parse and display the created time
                if (DateTime.TryParse(container.config.info.createdTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var createdTimeDt))
                {
                    DTEditorUtils.ReadOnlyTextField("Created Time", createdTimeDt.ToLocalTime().ToString());
                }
                else
                {
                    DTEditorUtils.ReadOnlyTextField("Created Time", "(Unable to parse date)");
                }

                // attempts to parse and display the updated time
                if (DateTime.TryParse(container.config.info.updatedTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var updatedTimeDt))
                {
                    DTEditorUtils.ReadOnlyTextField("Updated Time", updatedTimeDt.ToLocalTime().ToString());
                }
                else
                {
                    DTEditorUtils.ReadOnlyTextField("Updated Time", "(Unable to parse date)");
                }

                GUILayout.Label("Description");
                container.config.info.description = EditorGUILayout.TextArea(container.config.info.description);
            }
            EditorGUILayout.EndVertical();
        }

        public void OnGUI()
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(container.targetAvatar);

            DrawModulesGUI();
            DrawAvatarConfigsGUI(cabinet);
            DrawMetaInfoGUI();
        }

        public bool IsValid()
        {
            // prepare config
            container.config.configVersion = DTWearableConfig.CurrentConfigVersion;

            // TODO: multiple GUIDs
            if (guidReferencePrefab != null || container.targetAvatar != null)
            {
                var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(guidReferencePrefab ?? container.targetAvatar);
                var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";
                if (invalidAvatarPrefabGuid)
                {
                    if (container.config.targetAvatarConfig.guids.Length != 0)
                    {
                        container.config.targetAvatarConfig.guids = new string[0];
                    }
                }
                else
                {
                    if (container.config.targetAvatarConfig.guids.Length != 1)
                    {
                        container.config.targetAvatarConfig.guids = new string[1];
                    }
                    container.config.targetAvatarConfig.guids[0] = avatarPrefabGuid;
                }
            }

            var ready = true;

            foreach (var module in container.config.modules)
            {
                // ask the module editor that whether the module config is valid
                ready &= moduleEditors.TryGetValue(module, out var editor) && editor.IsValid();
            }

            return ready;
        }
    }
}
