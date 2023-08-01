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
        private static Dictionary<Type, Type> s_moduleEditorTypesCache = null;

        private static List<Type> s_availableModulesCache = null;

        private IWearableConfigView _view;

        public WearableConfigPresenter(IWearableConfigView view)
        {
            _view = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;
            _view.AddModuleButtonClick += OnAddModuleButtonClick;
            _view.TargetAvatarConfigChange += OnTargetAvatarConfigChange;
            _view.MetaInfoChange += OnMetaInfoChange;
            _view.ForceUpdateView += OnForceUpdateView;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
            _view.AddModuleButtonClick -= OnAddModuleButtonClick;
            _view.TargetAvatarConfigChange -= OnTargetAvatarConfigChange;
            _view.MetaInfoChange -= OnMetaInfoChange;
            _view.ForceUpdateView -= OnForceUpdateView;
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
            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar);

            // try obtain armature name from cabinet
            if (cabinet == null)
            {
                // leave it empty
                _view.Config.targetAvatarConfig.armatureName = "";
            }
            else
            {
                _view.Config.targetAvatarConfig.armatureName = cabinet.avatarArmatureName;
            }

            // can't do anything
            if (_view.TargetAvatar == null || _view.TargetWearable == null)
            {
                return;
            }

            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(_view.GuidReferencePrefab ?? _view.TargetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            _view.Config.targetAvatarConfig.guids.Clear();
            if (!invalidAvatarPrefabGuid)
            {
                // TODO: multiple guids
                _view.Config.targetAvatarConfig.guids.Add(avatarPrefabGuid);
            }

            var deltaPos = _view.TargetWearable.transform.position - _view.TargetAvatar.transform.position;
            var deltaRotation = _view.TargetWearable.transform.rotation * Quaternion.Inverse(_view.TargetAvatar.transform.rotation);
            _view.Config.targetAvatarConfig.worldPosition = new DTAvatarConfigVector3(deltaPos);
            _view.Config.targetAvatarConfig.worldRotation = new DTAvatarConfigQuaternion(deltaRotation);
            _view.Config.targetAvatarConfig.avatarLossyScale = new DTAvatarConfigVector3(_view.TargetAvatar.transform.lossyScale);
            _view.Config.targetAvatarConfig.wearableLossyScale = new DTAvatarConfigVector3(_view.TargetWearable.transform.lossyScale);
        }

        private void OnMetaInfoChange()
        {
            ApplyMetaInfoChanges();
        }

        private void ApplyMetaInfoChanges()
        {
            _view.Config.info.name = _view.MetaInfoWearableName;
            _view.Config.info.author = _view.MetaInfoAuthor;
            _view.Config.info.description = _view.MetaInfoDescription;
        }

        private void OnTargetAvatarOrWearableChange()
        {
            // reset the settings and read the avatar and wearable name
            _view.TargetAvatarConfigUseAvatarObjectName = true;
            if (_view.TargetAvatar != null)
            {
                _view.TargetAvatarConfigAvatarName = _view.Config.targetAvatarConfig.name = _view.TargetAvatar.name;
            }
            _view.MetaInfoUseWearableObjectName = true;
            if (_view.TargetWearable != null)
            {
                _view.MetaInfoWearableName = _view.Config.info.name = _view.TargetWearable.name;
            }

            // apply guid, world pos changes
            ApplyTargetAvatarConfigChanges();

            // update view
            UpdateView();
        }

        private void OnAddModuleButtonClick()
        {
            var newModule = (DTWearableModuleBase)Activator.CreateInstance(s_availableModulesCache[_view.SelectedAvailableModule]);
            if (!newModule.AllowMultiple)
            {
                // check if any existing type
                foreach (var existingModule in _view.Config.modules)
                {
                    if (existingModule.GetType() == newModule.GetType())
                    {
                        EditorUtility.DisplayDialog("DressingTools", "This module has been added before and cannot have multiple ones.", "OK");
                        return;
                    }
                }
            }
            _view.Config.modules.Add(newModule);

            // update module editors
            UpdateModulesView();
        }

        private ModuleEditor CreateModuleEditor(DTWearableModuleBase module)
        {
            // prepare cache if not yet
            if (s_moduleEditorTypesCache == null)
            {
                s_moduleEditorTypesCache = new Dictionary<Type, Type>();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        var attributes = type.GetCustomAttributes(typeof(CustomModuleEditor), true);
                        foreach (CustomModuleEditor attribute in attributes)
                        {
                            if (s_moduleEditorTypesCache.ContainsKey(attribute.ModuleType))
                            {
                                Debug.LogWarning("There are more than one CustomModuleEditor pointing to the same module! Skipping: " + type.FullName);
                                continue;
                            }
                            s_moduleEditorTypesCache.Add(attribute.ModuleType, type);
                        }
                    }
                }
            }

            // obtain from cache and create an editor instance
            if (s_moduleEditorTypesCache.TryGetValue(module.GetType(), out var moduleEditorType))
            {
                return (ModuleEditor)Activator.CreateInstance(moduleEditorType, _view, module);
            }

            // default module
            return new ModuleEditor(_view, module);
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

            if (s_availableModulesCache == null)
            {
                s_availableModulesCache = GetAllAvailableModules();
                var keys = new string[s_availableModulesCache.Count];
                for (var i = 0; i < s_availableModulesCache.Count; i++)
                {
                    keys[i] = s_availableModulesCache[i].FullName;
                }
                _view.AvailableModuleKeys = keys;
            }

            // call unload before we clear the list
            foreach (var moduleData in _view.ModuleDataList)
            {
                moduleData.editor.OnDisable();
            }
            _view.ModuleDataList.Clear();

            foreach (var module in _view.Config.modules)
            {
                var moduleData = new ModuleData()
                {
                    editor = CreateModuleEditor(module),
                };
                moduleData.removeButtonOnClickEvent = () =>
                {
                    moduleData.editor.OnDisable();
                    _view.Config.modules.Remove(module);
                    _view.ModuleDataList.Remove(moduleData);
                };
                _view.ModuleDataList.Add(moduleData);

                // call enable
                moduleData.editor.OnEnable();
            }

            // TODO: sort modules according to apply order?
        }

        private void UpdateAvatarConfigsView()
        {
            if (_view.TargetAvatar == null || _view.TargetWearable == null)
            {
                // we cannot do anything with target avatar and wearable null
                _view.ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox = true;
                return;
            }
            _view.ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox = false;

            // GUI Data
            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(_view.GuidReferencePrefab ?? _view.TargetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            _view.IsInvalidAvatarPrefabGuid = invalidAvatarPrefabGuid;
            _view.AvatarPrefabGuid = invalidAvatarPrefabGuid ? null : avatarPrefabGuid;

            _view.TargetAvatarConfigUseAvatarObjectName = _view.TargetAvatar.name == _view.Config.targetAvatarConfig.name;
            _view.TargetAvatarConfigAvatarName = _view.Config.targetAvatarConfig.name;
            _view.TargetAvatarConfigWorldPosition = _view.Config.targetAvatarConfig.worldPosition.ToString();
            _view.TargetAvatarConfigWorldRotation = _view.Config.targetAvatarConfig.worldRotation.ToString();
            _view.TargetAvatarConfigWorldAvatarLossyScale = _view.Config.targetAvatarConfig.avatarLossyScale.ToString();
            _view.TargetAvatarConfigWorldWearableLossyScale = _view.Config.targetAvatarConfig.wearableLossyScale.ToString();
        }

        private void UpdateMetaInfoView()
        {
            _view.ConfigUuid = _view.Config.info.uuid;

            if (_view.TargetWearable == null)
            {
                // can't do anything with empty wearable
                return;
            }

            // automatically unset this setting if name is not the same
            _view.MetaInfoUseWearableObjectName = _view.TargetWearable.name == _view.Config.info.name;
            _view.MetaInfoWearableName = _view.Config.info.name;
            _view.MetaInfoAuthor = _view.Config.info.author;

            // attempts to parse and display the created time
            if (DateTime.TryParse(_view.Config.info.createdTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var createdTimeDt))
            {
                _view.MetaInfoCreatedTime = createdTimeDt.ToLocalTime().ToString();
            }
            else
            {
                _view.MetaInfoCreatedTime = "(Unable to parse date)";
            }

            // attempts to parse and display the updated time
            if (DateTime.TryParse(_view.Config.info.updatedTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var updatedTimeDt))
            {
                _view.MetaInfoUpdatedTime = updatedTimeDt.ToLocalTime().ToString();
            }
            else
            {
                _view.MetaInfoUpdatedTime = "(Unable to parse date)";
            }

            _view.MetaInfoDescription = _view.Config.info.description;
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
            // prepare config
            _view.Config.configVersion = DTWearableConfig.CurrentConfigVersion;

            // TODO: multiple GUIDs
            if (_view.GuidReferencePrefab != null || _view.TargetAvatar != null)
            {
                var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(_view.GuidReferencePrefab ?? _view.TargetAvatar);
                var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";
                if (invalidAvatarPrefabGuid)
                {
                    if (_view.Config.targetAvatarConfig.guids.Count > 0)
                    {
                        _view.Config.targetAvatarConfig.guids.Clear();
                    }
                }
                else
                {
                    if (_view.Config.targetAvatarConfig.guids.Count != 1)
                    {
                        _view.Config.targetAvatarConfig.guids.Clear();
                        _view.Config.targetAvatarConfig.guids.Add(avatarPrefabGuid);
                    }
                }
            }

            var ready = true;

            foreach (var module in _view.ModuleDataList)
            {
                // ask the module editor that whether the module config is valid
                ready &= module.editor.IsValid();
            }

            return ready;
        }
    }
}
