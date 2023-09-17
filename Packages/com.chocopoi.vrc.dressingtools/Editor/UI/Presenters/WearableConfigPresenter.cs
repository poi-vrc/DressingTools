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
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class WearableConfigPresenter
    {
        private WearableModuleProviderBase[] s_moduleProviders = null;
        private static Dictionary<Type, Type> s_moduleEditorTypesCache = null;

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

            _view.InfoNewThumbnailButtonClick += OnInfoNewThumbnailButtonClick;
            _view.CaptureThumbnailButtonClick += OnCaptureThumbnailButtonClick;
            _view.CaptureCancelButtonClick += OnCaptureCancelButtonClick;
            _view.CaptureSettingsChange += OnCaptureSettingsChange;
            _view.ModeChange += OnModeChange;
            _view.AdvancedModuleAddButtonClick += OnAdvancedModuleAddButtonClick;
            _view.ToolbarPreviewButtonClick += OnPreviewButtonClick;
            _view.ToolbarAutoSetupButtonClick += OnToolbarAutoSetupButtonClick;
            _view.AvatarConfigChange += OnAvatarConfigChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;

            _view.InfoNewThumbnailButtonClick -= OnInfoNewThumbnailButtonClick;
            _view.CaptureThumbnailButtonClick -= OnCaptureThumbnailButtonClick;
            _view.CaptureCancelButtonClick -= OnCaptureCancelButtonClick;
            _view.CaptureSettingsChange -= OnCaptureSettingsChange;
            _view.ModeChange -= OnModeChange;
            _view.AdvancedModuleAddButtonClick -= OnAdvancedModuleAddButtonClick;
            _view.ToolbarPreviewButtonClick -= OnPreviewButtonClick;
            _view.ToolbarAutoSetupButtonClick -= OnToolbarAutoSetupButtonClick;
            _view.AvatarConfigChange -= OnAvatarConfigChange;
        }

        private void OnAvatarConfigChange()
        {
            ApplyAvatarConfig();
            UpdateAdvancedAvatarConfigView();
            _view.RepaintAdvancedModeAvatarConfig();
        }

        private void OnToolbarAutoSetupButtonClick()
        {
            if (_view.ShowConfirmAutoSetupDialog())
            {
                AutoSetup();
            }
        }


        private void OnPreviewButtonClick()
        {
            if (_view.PreviewActive)
            {
                DTEditorUtils.CleanUpPreviewAvatars();
                DTEditorUtils.FocusGameObjectInSceneView(_view.TargetAvatar);
            }
            else
            {
                UpdateAvatarPreview();
            }
        }

        private void OnAdvancedModuleAddButtonClick()
        {
            var index = _view.AdvancedModuleNames.IndexOf(_view.AdvancedSelectedModuleName);
            if (index == -1)
            {
                // invalid
                return;
            }

            if (index == 0)
            {
                // placeholder
                return;
            }

            var moduleProvider = s_moduleProviders[index - 1];

            if (!moduleProvider.AllowMultiple)
            {
                // check if any existing type
                foreach (var existingModule in _view.Config.modules)
                {
                    if (existingModule.moduleName == moduleProvider.ModuleIdentifier)
                    {
                        _view.ShowModuleAddedBeforeDialog();
                        return;
                    }
                }
            }

            _view.Config.modules.Add(new WearableModule()
            {
                moduleName = moduleProvider.ModuleIdentifier,
                config = moduleProvider.NewModuleConfig()
            });

            // update and repaint module views only
            UpdateAdvancedModulesView();
            _view.RepaintAdvancedModeModules();
        }

        private void OnModeChange()
        {
            if (_view.SelectedMode == 0)
            {
                UpdateSimpleView();
                _view.RepaintSimpleMode();
            }
            else if (_view.SelectedMode == 1)
            {
                ApplySimpleConfig();
                UpdateAdvancedModulesView();
                _view.RepaintAdvancedModeModules();
            }
        }

        private void OnInfoNewThumbnailButtonClick()
        {
            UpdateCaptureThumbnailPanel();
            _view.SwitchToCapturePanel();
        }

        private void UpdateCaptureThumbnailPanel()
        {
            if (_view.TargetWearable != null)
            {
                DTEditorUtils.PrepareWearableThumbnailCamera(_view.TargetWearable, _view.CaptureWearableOnly, _view.CaptureRemoveBackground, true, () => _view.RepaintCapturePreview());
            }
        }

        private void OnCaptureThumbnailButtonClick()
        {
            var texture = DTEditorUtils.GetThumbnailCameraPreview();
            _view.Config.info.thumbnail = DTEditorUtils.GetBase64FromTexture(texture);
            _view.InfoThumbnail = texture;
            ReturnToInfoPanel();
        }

        private void OnCaptureCancelButtonClick()
        {
            ReturnToInfoPanel();
        }

        private void ReturnToInfoPanel()
        {
            DTEditorUtils.CleanUpThumbnailObjects();
            if (_view.TargetAvatar != null)
            {
                DTEditorUtils.FocusGameObjectInSceneView(_view.TargetAvatar);
            }
            _view.Repaint();
            _view.SwitchToInfoPanel();
        }

        private void OnCaptureSettingsChange()
        {
            UpdateCaptureThumbnailPanel();
        }

        private void OnTargetAvatarOrWearableChange()
        {
            ApplyAvatarConfig();
            UpdateView();
        }

        private static void RemoveModuleIfExist(WearableConfig wearableConfig, string moduleName)
        {
            var module = DTEditorUtils.FindWearableModule(wearableConfig, moduleName);
            if (module != null)
            {
                wearableConfig.modules.Remove(module);
            }
        }

        private static void SetModuleConfig(WearableConfig wearableConfig, string moduleName, IModuleConfig moduleConfig)
        {
            var module = DTEditorUtils.FindWearableModule(wearableConfig, moduleName);
            if (module != null)
            {
                module.config = moduleConfig;
            }
            else
            {
                wearableConfig.modules.Add(new WearableModule()
                {
                    moduleName = moduleName,
                    config = moduleConfig,
                });
            }
        }

        private static void ApplySimpleModuleConfig(WearableConfig wearableConfig, string moduleName, IModuleConfig moduleConfig, bool enabled)
        {
            if (enabled)
            {
                SetModuleConfig(wearableConfig, moduleName, moduleConfig);
            }
            else
            {
                RemoveModuleIfExist(wearableConfig, moduleName);
            }
        }

        private void ApplySimpleConfig()
        {
            ApplySimpleModuleConfig(_view.Config, ArmatureMappingWearableModuleProvider.MODULE_IDENTIFIER, _view.SimpleArmatureMappingConfig, _view.SimpleUseArmatureMapping);
            ApplySimpleModuleConfig(_view.Config, MoveRootWearableModuleProvider.MODULE_IDENTIFIER, _view.SimpleMoveRootConfig, _view.SimpleUseMoveRoot);
            ApplySimpleModuleConfig(_view.Config, AnimationGenerationWearableModuleProvider.MODULE_IDENTIFIER, _view.SimpleAnimationGenerationConfig, _view.SimpleUseAnimationGeneration);
            ApplySimpleModuleConfig(_view.Config, BlendshapeSyncWearableModuleProvider.MODULE_IDENTIFIER, _view.SimpleBlendshapeSyncConfig, _view.SimpleUseBlendshapeSync);
        }

        private void ApplyWearableInfoConfig()
        {
            if (_view.TargetWearable == null)
            {
                return;
            }

            _view.Config.info.name = _view.InfoUseCustomWearableName ? _view.InfoCustomWearableName : _view.TargetWearable.name;
            _view.Config.info.author = _view.InfoAuthor;
            _view.Config.info.description = _view.InfoDescription;
        }

        private void ApplyAvatarConfig()
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar);

            // try obtain armature name from cabinet
            if (cabinet == null)
            {
                // leave it empty
                _view.Config.avatarConfig.armatureName = "";
            }
            else
            {
                if (CabinetConfig.TryDeserialize(cabinet.configJson, out var cabinetConfig))
                {
                    _view.Config.avatarConfig.armatureName = cabinetConfig.avatarArmatureName;
                }
                else
                {
                    _view.Config.avatarConfig.armatureName = "";
                }
            }

            // can't do anything
            if (_view.TargetAvatar == null || _view.TargetWearable == null)
            {
                return;
            }

            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(_view.AdvancedAvatarConfigGuidReference ?? _view.TargetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            _view.Config.avatarConfig.guids.Clear();
            if (!invalidAvatarPrefabGuid)
            {
                // TODO: multiple guids
                _view.Config.avatarConfig.guids.Add(avatarPrefabGuid);
            }

            var deltaPos = _view.TargetWearable.transform.position - _view.TargetAvatar.transform.position;
            var deltaRotation = _view.TargetWearable.transform.rotation * Quaternion.Inverse(_view.TargetAvatar.transform.rotation);
            _view.Config.avatarConfig.worldPosition = new AvatarConfigVector3(deltaPos);
            _view.Config.avatarConfig.worldRotation = new AvatarConfigQuaternion(deltaRotation);
            _view.Config.avatarConfig.avatarLossyScale = new AvatarConfigVector3(_view.TargetAvatar.transform.lossyScale);
            _view.Config.avatarConfig.wearableLossyScale = new AvatarConfigVector3(_view.TargetWearable.transform.lossyScale);
        }

        public void ApplyConfig()
        {
            ApplyWearableInfoConfig();

            if (_view.SelectedMode == 0)
            {
                ApplySimpleConfig();
            }
            else if (_view.SelectedMode == 1)
            {
                ApplyAvatarConfig();
            }
        }

        private void UpdateWearableInfoView()
        {
            if (_view.Config.info.thumbnail != null)
            {
                try
                {
                    _view.InfoThumbnail = DTEditorUtils.GetTextureFromBase64(_view.Config.info.thumbnail);
                }
                catch (Exception ex)
                {
                    _view.InfoThumbnail = null;
                    Debug.LogWarning("[DressingTools] Unable to load thumbnail from base64, ignoring: " + ex.Message);
                }
            }

            _view.InfoUseCustomWearableName = _view.TargetWearable != null ? (_view.TargetWearable.name != _view.Config.info.name) : true;
            _view.InfoCustomWearableName = _view.Config.info.name;

            _view.InfoUuid = _view.Config.info.uuid;
            _view.InfoAuthor = _view.Config.info.author;

            // attempts to parse and display the created time
            if (DateTime.TryParse(_view.Config.info.createdTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var createdTimeDt))
            {
                _view.InfoCreatedTime = createdTimeDt.ToLocalTime().ToString();
            }
            else
            {
                _view.InfoCreatedTime = "(Unable to parse date)";
            }

            // attempts to parse and display the updated time
            if (DateTime.TryParse(_view.Config.info.updatedTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var updatedTimeDt))
            {
                _view.InfoUpdatedTime = updatedTimeDt.ToLocalTime().ToString();
            }
            else
            {
                _view.InfoUpdatedTime = "(Unable to parse date)";
            }

            _view.InfoDescription = _view.Config.info.description;
        }

        private void UpdateSimpleView()
        {
            var armatureMappingModule = DTEditorUtils.FindWearableModule(_view.Config, ArmatureMappingWearableModuleProvider.MODULE_IDENTIFIER);
            _view.SimpleUseArmatureMapping = armatureMappingModule != null;
            _view.SimpleArmatureMappingConfig = armatureMappingModule != null ? (ArmatureMappingWearableModuleConfig)armatureMappingModule.config : new ArmatureMappingWearableModuleConfig();

            var moveRootModule = DTEditorUtils.FindWearableModule(_view.Config, MoveRootWearableModuleProvider.MODULE_IDENTIFIER);
            _view.SimpleUseMoveRoot = moveRootModule != null;
            _view.SimpleMoveRootConfig = moveRootModule != null ? (MoveRootWearableModuleConfig)moveRootModule.config : new MoveRootWearableModuleConfig();

            var animGenModule = DTEditorUtils.FindWearableModule(_view.Config, AnimationGenerationWearableModuleProvider.MODULE_IDENTIFIER);
            _view.SimpleUseAnimationGeneration = animGenModule != null;
            _view.SimpleAnimationGenerationConfig = animGenModule != null ? (AnimationGenerationWearableModuleConfig)animGenModule.config : new AnimationGenerationWearableModuleConfig();

            var blendshapeSyncModule = DTEditorUtils.FindWearableModule(_view.Config, BlendshapeSyncWearableModuleProvider.MODULE_IDENTIFIER);
            _view.SimpleUseBlendshapeSync = blendshapeSyncModule != null;
            _view.SimpleBlendshapeSyncConfig = blendshapeSyncModule != null ? (BlendshapeSyncWearableModuleConfig)blendshapeSyncModule.config : new BlendshapeSyncWearableModuleConfig();
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

        private void UpdateAdvancedModulesView()
        {
            if (s_moduleProviders == null)
            {
                s_moduleProviders = WearableModuleProviderLocator.Instance.GetAllProviders();
            }

            _view.AdvancedModuleNames.Clear();
            _view.AdvancedModuleNames.Add("---");
            foreach (var moduleProvider in s_moduleProviders)
            {
                _view.AdvancedModuleNames.Add($"{moduleProvider.FriendlyName}");
            }

            // call unload before we clear the list
            foreach (var moduleData in _view.AdvancedModuleViewDataList)
            {
                moduleData.editor.OnDisable();
            }
            _view.AdvancedModuleViewDataList.Clear();

            foreach (var module in _view.Config.modules)
            {
                var provider = WearableModuleProviderLocator.Instance.GetProvider(module.moduleName);

                if (provider == null)
                {
                    // TODO: display as unknown module in GUI
                    Debug.LogWarning("Unknown module detected: " + module.moduleName);
                    continue;
                }

                var moduleData = new WearableConfigModuleViewData()
                {
                    editor = CreateModuleEditor(provider, module.config),
                };
                moduleData.removeButtonOnClick = () =>
                {
                    moduleData.editor.OnDisable();
                    _view.Config.modules.Remove(module);
                    _view.AdvancedModuleViewDataList.Remove(moduleData);
                    UpdateAdvancedModulesView();
                    _view.RepaintAdvancedModeModules();
                };
                _view.AdvancedModuleViewDataList.Add(moduleData);

                // call enable
                moduleData.editor.OnEnable();
            }
        }

        private void UpdateAdvancedAvatarConfigView()
        {
            if (_view.TargetAvatar == null || _view.TargetWearable == null)
            {
                return;
            }

            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(_view.AdvancedAvatarConfigGuidReference ?? _view.TargetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            _view.AdvancedAvatarConfigGuid = invalidAvatarPrefabGuid ? null : avatarPrefabGuid;

            _view.AdvancedAvatarConfigUseAvatarObjName = _view.TargetAvatar.name == _view.Config.avatarConfig.name;
            _view.AdvancedAvatarConfigCustomName = _view.Config.avatarConfig.name;
            _view.AdvancedAvatarConfigDeltaWorldPos = _view.Config.avatarConfig.worldPosition.ToString();
            _view.AdvancedAvatarConfigDeltaWorldRot = _view.Config.avatarConfig.worldRotation.ToString();
            _view.AdvancedAvatarConfigAvatarLossyScale = _view.Config.avatarConfig.avatarLossyScale.ToString();
            _view.AdvancedAvatarConfigWearableLossyScale = _view.Config.avatarConfig.wearableLossyScale.ToString();
        }

        private void UpdateAdvancedView()
        {
            UpdateAdvancedModulesView();
            UpdateAdvancedAvatarConfigView();
        }

        private void UpdateView()
        {
            UpdateWearableInfoView();
            UpdateSimpleView();
            UpdateAdvancedView();
            _view.Repaint();
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }

        public void UpdateAvatarPreview()
        {
            ApplyConfig();
            DTEditorUtils.UpdatePreviewAvatar(_view.TargetAvatar, _view.Config, _view.TargetWearable);
        }

        private void AutoSetupMapping()
        {
            // cabinet
            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar);
            if (!CabinetConfig.TryDeserialize(cabinet.configJson, out var cabinetConfig))
            {
                _view.ShowCabinetConfigErrorHelpBox = true;
                return;
            }
            _view.ShowCabinetConfigErrorHelpBox = false;

            var armatureName = "Armature";
            if (cabinet != null)
            {
                armatureName = cabinetConfig.avatarArmatureName;
                _view.ShowAvatarNoCabinetHelpBox = false;
            }
            else
            {
                _view.ShowAvatarNoCabinetHelpBox = true;
            }

            // attempt to find wearable armature using avatar armature name
            var armature = DTEditorUtils.GuessArmature(_view.TargetWearable, armatureName);

            if (armature == null)
            {
                _view.ShowArmatureNotFoundHelpBox = true;
                _view.ShowArmatureGuessedHelpBox = false;
                RemoveModuleIfExist(_view.Config, ArmatureMappingWearableModuleProvider.MODULE_IDENTIFIER);
            }
            else
            {
                _view.ShowArmatureNotFoundHelpBox = false;
                _view.ShowArmatureGuessedHelpBox = armature.name != armatureName;

                var config = new ArmatureMappingWearableModuleConfig()
                {
                    dresserName = typeof(DefaultDresser).FullName,
                    wearableArmatureName = armatureName,
                    boneMappingMode = BoneMappingMode.Auto,
                    boneMappings = null,
                    serializedDresserConfig = JsonConvert.SerializeObject(new DefaultDresserSettings()),
                    removeExistingPrefixSuffix = true,
                    groupBones = true
                };
                SetModuleConfig(_view.Config, ArmatureMappingWearableModuleProvider.MODULE_IDENTIFIER, config);
            }
        }

        private string[] GetBlendshapeNames(SkinnedMeshRenderer smr)
        {
            if (smr.sharedMesh == null)
            {
                return new string[0];
            }

            var names = new List<string>();
            for (var i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                names.Add(smr.sharedMesh.GetBlendShapeName(i));
            }

            return names.ToArray();
        }

        private void AutoSetupAnimationGeneration()
        {
            // generate wearable toggles
            {
                var transforms = new List<Transform>();

                var armatureMappingModule = DTEditorUtils.FindWearableModuleConfig<ArmatureMappingWearableModuleConfig>(_view.Config);
                if (armatureMappingModule != null)
                {
                    // skip the armature if used armature mapping
                    var wearableArmature = DTEditorUtils.GuessArmature(_view.TargetWearable, armatureMappingModule.wearableArmatureName);
                    for (var i = 0; i < _view.TargetWearable.transform.childCount; i++)
                    {
                        // do not auto add wearable toggle if state is disabled initially
                        var child = _view.TargetWearable.transform.GetChild(i);
                        if (child != wearableArmature && child.gameObject.activeSelf)
                        {
                            transforms.Add(child);
                        }
                    }
                }
                else
                {
                    // add all
                    for (var i = 0; i < _view.TargetWearable.transform.childCount; i++)
                    {
                        // do not auto add wearable toggle if state is disabled initially
                        var child = _view.TargetWearable.transform.GetChild(i);
                        if (child.gameObject.activeSelf)
                        {
                            transforms.Add(child);
                        }
                    }
                }

                var config = new AnimationGenerationWearableModuleConfig();

                var toggles = config.wearableAnimationOnWear.toggles;
                var blendshapes = config.wearableAnimationOnWear.blendshapes;
                toggles.Clear();
                blendshapes.Clear();

                foreach (var trans in transforms)
                {
                    toggles.Add(new AnimationToggle()
                    {
                        path = DTEditorUtils.GetRelativePath(trans, _view.TargetWearable.transform),
                        state = true
                    });
                }

                SetModuleConfig(_view.Config, AnimationGenerationWearableModuleProvider.MODULE_IDENTIFIER, config);
            }

            // generate blendshape syncs
            {
                var config = new BlendshapeSyncWearableModuleConfig();

                // find all avatar blendshapes
                var avatarSmrs = _view.TargetAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var avatarSmrCache = new Dictionary<SkinnedMeshRenderer, string[]>();
                foreach (var avatarSmr in avatarSmrs)
                {
                    // transverse up to see if it is originated from ours or an existing wearable
                    if (DTEditorUtils.IsOriginatedFromAnyWearable(_view.TargetAvatar.transform, avatarSmr.transform) ||
                        DTEditorUtils.IsGrandParent(_view.TargetWearable.transform, avatarSmr.transform))
                    {
                        // skip this SMR
                        continue;
                    }

                    // add to cache
                    avatarSmrCache.Add(avatarSmr, GetBlendshapeNames(avatarSmr));
                }

                // pair wearable blendshape names with avatar
                var wearableSmrs = _view.TargetWearable.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var wearableSmr in wearableSmrs)
                {
                    var wearableBlendshapes = GetBlendshapeNames(wearableSmr);

                    if (wearableBlendshapes.Length == 0)
                    {
                        // skip no wearable blendshapes
                        continue;
                    }

                    // search if have pairs
                    var found = false;
                    foreach (var avatarSmr in avatarSmrs)
                    {
                        if (!avatarSmrCache.ContainsKey(avatarSmr))
                        {
                            continue;
                        }

                        var avatarBlendshapes = avatarSmrCache[avatarSmr];
                        foreach (var wearableBlendshape in wearableBlendshapes)
                        {
                            if (System.Array.IndexOf(avatarBlendshapes, wearableBlendshape) != -1)
                            {
                                config.blendshapeSyncs.Add(new AnimationBlendshapeSync()
                                {
                                    avatarBlendshapeName = wearableBlendshape,
                                    avatarFromValue = 0,
                                    avatarToValue = 100,
                                    wearableFromValue = 0,
                                    wearableToValue = 100,
                                    wearableBlendshapeName = wearableBlendshape,
                                    avatarPath = DTEditorUtils.GetRelativePath(avatarSmr.transform, _view.TargetAvatar.transform),
                                    wearablePath = DTEditorUtils.GetRelativePath(wearableSmr.transform, _view.TargetWearable.transform)
                                });
                                found = true;
                                break;
                            }
                        }

                        if (found)
                        {
                            // don't process the same blendshape anymore if found
                            break;
                        }
                    }
                }

                // enable module if count > 0
                if (config.blendshapeSyncs.Count > 0)
                {
                    SetModuleConfig(_view.Config, BlendshapeSyncWearableModuleProvider.MODULE_IDENTIFIER, config);
                }
                else
                {
                    RemoveModuleIfExist(_view.Config, BlendshapeSyncWearableModuleProvider.MODULE_IDENTIFIER);
                }
            }
        }

        public void AutoSetup()
        {
            if (_view.TargetAvatar == null || _view.TargetWearable == null)
            {
                return;
            }

            AutoSetupMapping();
            AutoSetupAnimationGeneration();

            UpdateView();
        }
    }
}
