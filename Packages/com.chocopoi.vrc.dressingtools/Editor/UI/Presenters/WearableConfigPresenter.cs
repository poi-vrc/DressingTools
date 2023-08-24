/*
 * File: WearableConfigPresenter.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 9th 2023, 8:34:36 pm
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class WearableConfigPresenter
    {
        private static Dictionary<Type, Type> s_moduleEditorTypesCache = null;

        private IWearableConfigView _view;
        private WearableModuleProviderBase[] _moduleProviders;

        public WearableConfigPresenter(IWearableConfigView view)
        {
            _view = view;
            _moduleProviders = null;

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
                _view.Config.AvatarConfig.armatureName = "";
            }
            else
            {
                if (CabinetConfig.TryDeserialize(cabinet.configJson, out var cabinetConfig))
                {
                    _view.Config.AvatarConfig.armatureName = cabinetConfig.AvatarArmatureName;
                }
                else
                {
                    _view.Config.AvatarConfig.armatureName = "";
                }
            }

            // can't do anything
            if (_view.TargetAvatar == null || _view.TargetWearable == null)
            {
                return;
            }

            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(_view.GuidReferencePrefab ?? _view.TargetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            _view.Config.AvatarConfig.guids.Clear();
            if (!invalidAvatarPrefabGuid)
            {
                // TODO: multiple guids
                _view.Config.AvatarConfig.guids.Add(avatarPrefabGuid);
            }

            var deltaPos = _view.TargetWearable.transform.position - _view.TargetAvatar.transform.position;
            var deltaRotation = _view.TargetWearable.transform.rotation * Quaternion.Inverse(_view.TargetAvatar.transform.rotation);
            _view.Config.AvatarConfig.worldPosition = new AvatarConfigVector3(deltaPos);
            _view.Config.AvatarConfig.worldRotation = new AvatarConfigQuaternion(deltaRotation);
            _view.Config.AvatarConfig.avatarLossyScale = new AvatarConfigVector3(_view.TargetAvatar.transform.lossyScale);
            _view.Config.AvatarConfig.wearableLossyScale = new AvatarConfigVector3(_view.TargetWearable.transform.lossyScale);
        }

        private void OnMetaInfoChange()
        {
            ApplyMetaInfoChanges();
        }

        private void ApplyMetaInfoChanges()
        {
            _view.Config.Info.name = _view.MetaInfoWearableName;
            _view.Config.Info.author = _view.MetaInfoAuthor;
            _view.Config.Info.description = _view.MetaInfoDescription;
        }

        private void OnTargetAvatarOrWearableChange()
        {
            // reset the settings and read the avatar and wearable name
            _view.TargetAvatarConfigUseAvatarObjectName = true;
            if (_view.TargetAvatar != null)
            {
                _view.TargetAvatarConfigAvatarName = _view.Config.AvatarConfig.name = _view.TargetAvatar.name;
            }
            _view.MetaInfoUseWearableObjectName = true;
            if (_view.TargetWearable != null)
            {
                _view.MetaInfoWearableName = _view.Config.Info.name = _view.TargetWearable.name;
            }

            // apply guid, world pos changes
            ApplyTargetAvatarConfigChanges();

            // update view
            UpdateView();

            // force update module editors
            foreach (var moduleData in _view.ModuleDataList)
            {
                moduleData.editor.RaiseForceUpdateViewEvent();
            }
        }

        private void OnAddModuleButtonClick()
        {
            var provider = _moduleProviders[_view.SelectedAvailableModule];

            if (provider == null)
            {
                Debug.LogError("[DressingTools] The requested module provider type was not yet registered to locator!");
                return;
            }

            var newModuleConfig = provider.NewModuleConfig();
            if (!provider.AllowMultiple)
            {
                // check if any existing type
                foreach (var existingModule in _view.Config.Modules)
                {
                    if (existingModule.moduleName == provider.ModuleIdentifier)
                    {
                        EditorUtility.DisplayDialog("DressingTools", "This module has been added before and cannot have multiple ones.", "OK");
                        return;
                    }
                }
            }
            _view.Config.Modules.Add(new WearableModule()
            {
                moduleName = provider.ModuleIdentifier,
                config = newModuleConfig
            });

            // update module editors
            UpdateModulesView();
        }

        private WearableModuleEditor CreateModuleEditor(WearableModuleProviderBase provider, IModuleConfig module)
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
                        var attributes = type.GetCustomAttributes(typeof(CustomWearableModuleEditor), true);
                        foreach (CustomWearableModuleEditor attribute in attributes)
                        {
                            if (s_moduleEditorTypesCache.ContainsKey(attribute.ModuleProviderType))
                            {
                                Debug.LogWarning("There are more than one CustomModuleEditor pointing to the same module provider! Skipping: " + type.FullName);
                                continue;
                            }
                            s_moduleEditorTypesCache.Add(attribute.ModuleProviderType, type);
                        }
                    }
                }
            }

            // obtain from cache and create an editor instance
            if (s_moduleEditorTypesCache.TryGetValue(provider.GetType(), out var moduleEditorType))
            {
                return (WearableModuleEditor)Activator.CreateInstance(moduleEditorType, _view, provider, module);
            }

            // default module
            return new WearableModuleEditor(_view, provider, module);
        }

        private void UpdateModulesView()
        {
            // this will clear the list and causing foldout to be closed (default state is false)
            // TODO: do not clear but update necessary only?

            _moduleProviders = WearableModuleProviderLocator.Instance.GetAllProviders();
            var keys = new string[_moduleProviders.Length];
            for (var i = 0; i < _moduleProviders.Length; i++)
            {
                keys[i] = _moduleProviders[i].FriendlyName;
            }
            _view.AvailableModuleKeys = keys;

            // call unload before we clear the list
            foreach (var moduleData in _view.ModuleDataList)
            {
                moduleData.editor.OnDisable();
            }
            _view.ModuleDataList.Clear();

            foreach (var module in _view.Config.Modules)
            {
                var provider = WearableModuleProviderLocator.Instance.GetProvider(module.moduleName);

                if (provider == null)
                {
                    // TODO: display as unknown module in GUI
                    Debug.LogWarning("Unknown module detected: " + module.moduleName);
                    continue;
                }

                var moduleData = new ModuleData()
                {
                    editor = CreateModuleEditor(provider, module.config),
                };
                moduleData.removeButtonOnClickEvent = () =>
                {
                    moduleData.editor.OnDisable();
                    _view.Config.Modules.Remove(module);
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

            _view.TargetAvatarConfigUseAvatarObjectName = _view.TargetAvatar.name == _view.Config.AvatarConfig.name;
            _view.TargetAvatarConfigAvatarName = _view.Config.AvatarConfig.name;
            _view.TargetAvatarConfigWorldPosition = _view.Config.AvatarConfig.worldPosition.ToString();
            _view.TargetAvatarConfigWorldRotation = _view.Config.AvatarConfig.worldRotation.ToString();
            _view.TargetAvatarConfigWorldAvatarLossyScale = _view.Config.AvatarConfig.avatarLossyScale.ToString();
            _view.TargetAvatarConfigWorldWearableLossyScale = _view.Config.AvatarConfig.wearableLossyScale.ToString();
        }

        private void UpdateMetaInfoView()
        {
            _view.ConfigUuid = _view.Config.Info.uuid;

            if (_view.TargetWearable == null)
            {
                // can't do anything with empty wearable
                return;
            }

            // automatically unset this setting if name is not the same
            _view.MetaInfoUseWearableObjectName = _view.TargetWearable.name == _view.Config.Info.name;
            _view.MetaInfoWearableName = _view.Config.Info.name;
            _view.MetaInfoAuthor = _view.Config.Info.author;

            // attempts to parse and display the created time
            if (DateTime.TryParse(_view.Config.Info.createdTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var createdTimeDt))
            {
                _view.MetaInfoCreatedTime = createdTimeDt.ToLocalTime().ToString();
            }
            else
            {
                _view.MetaInfoCreatedTime = "(Unable to parse date)";
            }

            // attempts to parse and display the updated time
            if (DateTime.TryParse(_view.Config.Info.updatedTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var updatedTimeDt))
            {
                _view.MetaInfoUpdatedTime = updatedTimeDt.ToLocalTime().ToString();
            }
            else
            {
                _view.MetaInfoUpdatedTime = "(Unable to parse date)";
            }

            _view.MetaInfoDescription = _view.Config.Info.description;
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
            _view.Config.Version = WearableConfig.CurrentConfigVersion;

            // TODO: multiple GUIDs
            if (_view.GuidReferencePrefab != null || _view.TargetAvatar != null)
            {
                var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(_view.GuidReferencePrefab ?? _view.TargetAvatar);
                var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";
                if (invalidAvatarPrefabGuid)
                {
                    if (_view.Config.AvatarConfig.guids.Count > 0)
                    {
                        _view.Config.AvatarConfig.guids.Clear();
                    }
                }
                else
                {
                    if (_view.Config.AvatarConfig.guids.Count != 1)
                    {
                        _view.Config.AvatarConfig.guids.Clear();
                        _view.Config.AvatarConfig.guids.Add(avatarPrefabGuid);
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
