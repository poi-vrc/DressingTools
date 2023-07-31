using System;
using System.Collections.Generic;
using System.Linq;
using Chocopoi.DressingTools.UI.Views.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class WearableConfigPresenter
    {
        private static Dictionary<Type, Type> moduleEditorTypesCache = null;

        private static List<Type> availableModulesCache = null;

        private IWearableConfigView view_;

        public WearableConfigPresenter(IWearableConfigView view)
        {
            view_ = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            view_.Load += OnLoad;
            view_.Unload += OnUnload;

            view_.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;
            view_.AddModuleButtonClick += OnAddModuleButtonClick;
            view_.TargetAvatarConfigChange += OnTargetAvatarConfigChange;
            view_.MetaInfoChange += OnMetaInfoChange;
            view_.ForceUpdateView += OnForceUpdateView;
        }

        private void UnsubscribeEvents()
        {
            view_.Load -= OnLoad;
            view_.Unload -= OnUnload;

            view_.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
            view_.AddModuleButtonClick -= OnAddModuleButtonClick;
            view_.TargetAvatarConfigChange -= OnTargetAvatarConfigChange;
            view_.MetaInfoChange -= OnMetaInfoChange;
            view_.ForceUpdateView -= OnForceUpdateView;
        }

        private void OnForceUpdateView()
        {
            UpdateModulesView();
            UpdateView();
        }

        private void OnTargetAvatarConfigChange()
        {
            ApplyTargetAvatarConfigChanges();
            UpdateView();
        }

        private void ApplyTargetAvatarConfigChanges()
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(view_.TargetAvatar);

            // try obtain armature name from cabinet
            if (cabinet == null)
            {
                // leave it empty
                view_.Config.targetAvatarConfig.armatureName = "";
            }
            else
            {
                view_.Config.targetAvatarConfig.armatureName = cabinet.avatarArmatureName;
            }

            // can't do anything
            if (view_.TargetAvatar == null || view_.TargetWearable == null)
            {
                return;
            }

            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(view_.GuidReferencePrefab ?? view_.TargetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            view_.Config.targetAvatarConfig.guids.Clear();
            if (!invalidAvatarPrefabGuid)
            {
                // TODO: multiple guids
                view_.Config.targetAvatarConfig.guids.Add(avatarPrefabGuid);
            }

            var deltaPos = view_.TargetWearable.transform.position - view_.TargetAvatar.transform.position;
            var deltaRotation = view_.TargetWearable.transform.rotation * Quaternion.Inverse(view_.TargetAvatar.transform.rotation);
            view_.Config.targetAvatarConfig.worldPosition = new DTAvatarConfigVector3(deltaPos);
            view_.Config.targetAvatarConfig.worldRotation = new DTAvatarConfigQuaternion(deltaRotation);
            view_.Config.targetAvatarConfig.avatarLossyScale = new DTAvatarConfigVector3(view_.TargetAvatar.transform.lossyScale);
            view_.Config.targetAvatarConfig.wearableLossyScale = new DTAvatarConfigVector3(view_.TargetWearable.transform.lossyScale);
        }

        private void OnMetaInfoChange()
        {
            ApplyMetaInfoChanges();
        }

        private void ApplyMetaInfoChanges()
        {
            view_.Config.info.name = view_.MetaInfoWearableName;
            view_.Config.info.author = view_.MetaInfoAuthor;
            view_.Config.info.description = view_.MetaInfoDescription;
        }

        private void OnTargetAvatarOrWearableChange()
        {
            // reset the settings and read the avatar and wearable name
            view_.TargetAvatarConfigUseAvatarObjectName = true;
            if (view_.TargetAvatar != null)
            {
                view_.TargetAvatarConfigAvatarName = view_.Config.targetAvatarConfig.name = view_.TargetAvatar.name;
            }
            view_.MetaInfoUseWearableObjectName = true;
            if (view_.TargetWearable != null)
            {
                view_.MetaInfoWearableName = view_.Config.info.name = view_.TargetWearable.name;
            }

            // apply guid, world pos changes
            ApplyTargetAvatarConfigChanges();

            // update view
            UpdateView();
        }

        private void OnAddModuleButtonClick()
        {
            var newModule = (DTWearableModuleBase)Activator.CreateInstance(availableModulesCache[view_.SelectedAvailableModule]);
            if (!newModule.AllowMultiple)
            {
                // check if any existing type
                foreach (var existingModule in view_.Config.modules)
                {
                    if (existingModule.GetType() == newModule.GetType())
                    {
                        EditorUtility.DisplayDialog("DressingTools", "This module has been added before and cannot have multiple ones.", "OK");
                        return;
                    }
                }
            }
            view_.Config.modules.Add(newModule);

            // update module editors
            UpdateModulesView();
        }

        private ModuleEditor CreateModuleEditor(DTWearableModuleBase module)
        {
            // prepare cache if not yet
            if (moduleEditorTypesCache == null)
            {
                moduleEditorTypesCache = new Dictionary<Type, Type>();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

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
                return (ModuleEditor)Activator.CreateInstance(moduleEditorType, view_, module);
            }

            // default module
            return new ModuleEditor(view_, module);
        }

        private static List<Type> GetAllAvailableModules()
        {
            return System.Reflection.Assembly.GetAssembly(typeof(DTWearableModuleBase))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DTWearableModuleBase)) && !t.IsAbstract)
                .ToList();
        }

        private void UpdateModulesView()
        {
            // this will clear the list and causing foldout to be closed (default state is false)
            // TODO: do not clear but update necessary only?

            if (availableModulesCache == null)
            {
                availableModulesCache = GetAllAvailableModules();
                var keys = new string[availableModulesCache.Count];
                for (var i = 0; i < availableModulesCache.Count; i++)
                {
                    keys[i] = availableModulesCache[i].FullName;
                }
                view_.AvailableModuleKeys = keys;
            }

            // call unload before we clear the list
            foreach (var moduleData in view_.ModuleDataList)
            {
                moduleData.editor.OnDisable();
            }
            view_.ModuleDataList.Clear();

            foreach (var module in view_.Config.modules)
            {
                var moduleData = new ModuleData()
                {
                    editor = CreateModuleEditor(module),
                };
                moduleData.removeButtonOnClickEvent = () =>
                {
                    moduleData.editor.OnDisable();
                    view_.Config.modules.Remove(module);
                    view_.ModuleDataList.Remove(moduleData);
                };
                view_.ModuleDataList.Add(moduleData);

                // call enable
                moduleData.editor.OnEnable();
            }

            // TODO: sort modules according to apply order?
        }

        private void UpdateAvatarConfigsView()
        {
            if (view_.TargetAvatar == null || view_.TargetWearable == null)
            {
                // we cannot do anything with target avatar and wearable null
                view_.ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox = true;
                return;
            }
            view_.ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox = false;

            // GUI Data
            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(view_.GuidReferencePrefab ?? view_.TargetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            view_.IsInvalidAvatarPrefabGuid = invalidAvatarPrefabGuid;
            view_.AvatarPrefabGuid = invalidAvatarPrefabGuid ? null : avatarPrefabGuid;

            view_.TargetAvatarConfigUseAvatarObjectName = view_.TargetAvatar.name == view_.Config.targetAvatarConfig.name;
            view_.TargetAvatarConfigAvatarName = view_.Config.targetAvatarConfig.name;
            view_.TargetAvatarConfigWorldPosition = view_.Config.targetAvatarConfig.worldPosition.ToString();
            view_.TargetAvatarConfigWorldRotation = view_.Config.targetAvatarConfig.worldRotation.ToString();
            view_.TargetAvatarConfigWorldAvatarLossyScale = view_.Config.targetAvatarConfig.avatarLossyScale.ToString();
            view_.TargetAvatarConfigWorldWearableLossyScale = view_.Config.targetAvatarConfig.wearableLossyScale.ToString();
        }

        private void UpdateMetaInfoView()
        {
            view_.ConfigUuid = view_.Config.info.uuid;

            if (view_.TargetWearable == null)
            {
                // can't do anything with empty wearable
                return;
            }

            // automatically unset this setting if name is not the same
            view_.MetaInfoUseWearableObjectName = view_.TargetWearable.name == view_.Config.info.name;
            view_.MetaInfoWearableName = view_.Config.info.name;
            view_.MetaInfoAuthor = view_.Config.info.author;

            // attempts to parse and display the created time
            if (DateTime.TryParse(view_.Config.info.createdTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var createdTimeDt))
            {
                view_.MetaInfoCreatedTime = createdTimeDt.ToLocalTime().ToString();
            }
            else
            {
                view_.MetaInfoCreatedTime = "(Unable to parse date)";
            }

            // attempts to parse and display the updated time
            if (DateTime.TryParse(view_.Config.info.updatedTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var updatedTimeDt))
            {
                view_.MetaInfoUpdatedTime = updatedTimeDt.ToLocalTime().ToString();
            }
            else
            {
                view_.MetaInfoUpdatedTime = "(Unable to parse date)";
            }

            view_.MetaInfoDescription = view_.Config.info.description;
        }

        private void UpdateView()
        {
            // updates the view according to the config

            // it will not update the module editors to avoid foldout reset
            UpdateAvatarConfigsView();
            UpdateMetaInfoView();
        }

        private void OnLoad()
        {
            OnForceUpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }

        public bool IsValid()
        {
            return true;
        }
    }
}
