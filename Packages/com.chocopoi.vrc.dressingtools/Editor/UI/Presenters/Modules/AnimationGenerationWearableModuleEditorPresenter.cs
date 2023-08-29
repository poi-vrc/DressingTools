/*
 * File: AnimationGenerationModuleEditorPresenter.cs
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
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Cabinet.Modules;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Cabinet.Modules;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class AnimationGenerationWearableModuleEditorPresenter
    {
        private const string SavedPresetUnselectedPlaceholder = "---";

        private IAnimationGenerationWearableModuleEditorView _view;
        private IWearableModuleEditorViewParent _parentView;
        private AnimationGenerationWearableModuleConfig _module;
        private DTCabinet _cabinet;
        private CabinetConfig _cabinetConfig;
        private AnimationGenerationCabinetModuleConfig _moduleConfig;

        public AnimationGenerationWearableModuleEditorPresenter(IAnimationGenerationWearableModuleEditorView view, IWearableModuleEditorViewParent parentView, AnimationGenerationWearableModuleConfig module)
        {
            _view = view;
            _parentView = parentView;
            _module = module;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ForceUpdateView += OnForceUpdateView;

            _view.AvatarOnWearToggleAddEvent += OnAvatarOnWearToggleAddEvent;
            _view.AvatarOnWearBlendshapeAddEvent += OnAvatarOnWearBlendshapeAddEvent;

            _view.WearableOnWearToggleAddEvent += OnWearableOnWearToggleAddEvent;
            _view.WearableOnWearBlendshapeAddEvent += OnWearableOnWearBlendshapeAddEvent;

            _view.AvatarOnWearPresetChangeEvent += OnAvatarOnWearPresetChangeEvent;
            _view.AvatarOnWearPresetSaveEvent += OnAvatarOnWearPresetSaveEvent;
            _view.AvatarOnWearPresetDeleteEvent += OnAvatarOnWearPresetDeleteEvent;

            _view.WearableOnWearPresetChangeEvent += OnWearableOnWearPresetChangeEvent;
            _view.WearableOnWearPresetSaveEvent += OnWearableOnWearPresetSaveEvent;
            _view.WearableOnWearPresetDeleteEvent += OnWearableOnWearPresetDeleteEvent;

            _view.AddCustomizableEvent += OnAddCustomizableEvent;

            _parentView.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;

            _view.AvatarOnWearToggleAddEvent -= OnAvatarOnWearToggleAddEvent;
            _view.AvatarOnWearBlendshapeAddEvent -= OnAvatarOnWearBlendshapeAddEvent;

            _view.WearableOnWearToggleAddEvent -= OnWearableOnWearToggleAddEvent;
            _view.WearableOnWearBlendshapeAddEvent -= OnWearableOnWearBlendshapeAddEvent;

            _view.AvatarOnWearPresetChangeEvent -= OnAvatarOnWearPresetChangeEvent;
            _view.AvatarOnWearPresetSaveEvent -= OnAvatarOnWearPresetSaveEvent;
            _view.AvatarOnWearPresetDeleteEvent -= OnAvatarOnWearPresetDeleteEvent;

            _view.WearableOnWearPresetChangeEvent -= OnWearableOnWearPresetChangeEvent;
            _view.WearableOnWearPresetSaveEvent -= OnWearableOnWearPresetSaveEvent;
            _view.WearableOnWearPresetDeleteEvent -= OnWearableOnWearPresetDeleteEvent;

            _view.AddCustomizableEvent -= OnAddCustomizableEvent;

            _parentView.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
        }

        private void OnAddCustomizableEvent()
        {
            _module.wearableCustomizables.Add(new WearableCustomizable()
            {
                name = "Customizable-" + DTEditorUtils.RandomString(6)
            });
            UpdateCustomizables();
        }

        private void UpdateSelectedPresetData(Dictionary<string, AnimationPreset> savedPresets, PresetViewData presetData)
        {
            if (presetData.selectedPresetIndex == 0)
            {
                return;
            }
            var key = presetData.savedPresetKeys[presetData.selectedPresetIndex];
            _module.avatarAnimationOnWear = new AnimationPreset(savedPresets[key]);
        }

        private void SavePreset(Dictionary<string, AnimationPreset> savedPresets, PresetViewData presetData, AnimationPreset presetToSave)
        {
            var presetName = _view.ShowPresetNamingDialog();

            if (presetName == null || presetName.Trim() == "")
            {
                // cancelled
                return;
            }

            if (savedPresets.ContainsKey(presetName))
            {
                _view.ShowDuplicatedPresetNameDialog();
                return;
            }

            savedPresets.Add(presetName, presetToSave);

            // serialize to cabinet
            _cabinet.configJson = _cabinetConfig.Serialize();
            UpdateView();
        }

        private void DeletePreset(Dictionary<string, AnimationPreset> savedPresets, PresetViewData presetData)
        {
            if (presetData.selectedPresetIndex == 0)
            {
                return;
            }

            var key = presetData.savedPresetKeys[presetData.selectedPresetIndex];
            savedPresets.Remove(key);
            presetData.selectedPresetIndex = 0;

            // serialize to cabinet
            _cabinet.configJson = _cabinetConfig.Serialize();
            UpdateView();
        }

        private void OnAvatarOnWearPresetChangeEvent()
        {
            if (_cabinetConfig != null)
            {
                var agcm = DTEditorUtils.FindCabinetModuleConfig<AnimationGenerationCabinetModuleConfig>(_cabinetConfig);
                if (agcm != null)
                {
                    UpdateSelectedPresetData(agcm.savedAvatarPresets, _view.AvatarOnWearPresetData);
                    UpdateAnimationGenerationAvatarOnWear();
                }
            }
        }

        private void OnAvatarOnWearPresetSaveEvent()
        {
            if (_cabinetConfig == null || _moduleConfig == null) return;
            SavePreset(_moduleConfig.savedAvatarPresets, _view.AvatarOnWearPresetData, _module.avatarAnimationOnWear);
        }

        private void OnAvatarOnWearPresetDeleteEvent()
        {
            if (_cabinetConfig == null || _moduleConfig == null) return;
            DeletePreset(_moduleConfig.savedAvatarPresets, _view.AvatarOnWearPresetData);
        }

        private void OnWearableOnWearPresetChangeEvent()
        {
            if (_cabinetConfig != null)
            {
                var agcm = DTEditorUtils.FindCabinetModuleConfig<AnimationGenerationCabinetModuleConfig>(_cabinetConfig);
                if (agcm != null)
                {
                    UpdateSelectedPresetData(agcm.savedWearablePresets, _view.WearableOnWearPresetData);
                    UpdateAnimationGenerationWearableOnWear();
                }
            }
        }

        private void OnWearableOnWearPresetSaveEvent()
        {
            if (_cabinetConfig == null || _moduleConfig == null) return;
            SavePreset(_moduleConfig.savedWearablePresets, _view.WearableOnWearPresetData, _module.wearableAnimationOnWear);
        }

        private void OnWearableOnWearPresetDeleteEvent()
        {
            if (_cabinetConfig == null || _moduleConfig == null) return;
            DeletePreset(_moduleConfig.savedWearablePresets, _view.WearableOnWearPresetData);
        }

        private void OnTargetAvatarOrWearableChange()
        {
            UpdateView();
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void OnAvatarOnWearToggleAddEvent()
        {
            _module.avatarAnimationOnWear.toggles.Add(new AnimationToggle());
            UpdateAnimationGenerationAvatarOnWear();
        }

        private void OnAvatarOnWearBlendshapeAddEvent()
        {
            _module.avatarAnimationOnWear.blendshapes.Add(new AnimationBlendshapeValue());
            UpdateAnimationGenerationAvatarOnWear();
        }

        private void OnWearableOnWearToggleAddEvent()
        {
            _module.wearableAnimationOnWear.toggles.Add(new AnimationToggle());
            UpdateAnimationGenerationWearableOnWear();
        }

        private void OnWearableOnWearBlendshapeAddEvent()
        {
            _module.wearableAnimationOnWear.blendshapes.Add(new AnimationBlendshapeValue());
            UpdateAnimationGenerationWearableOnWear();
        }

        private void UpdateToggles(Transform root, List<AnimationToggle> toggles, List<ToggleData> toggleDataList, Action updateView = null)
        {
            toggleDataList.Clear();
            foreach (var toggle in toggles)
            {
                var transform = toggle.path != null ? root.Find(toggle.path) : null;

                var toggleData = new ToggleData()
                {
                    isInvalid = transform == null,
                    gameObject = transform?.gameObject,
                    state = toggle.state,
                };
                toggleData.changeEvent = () =>
                {
                    if (toggleData.gameObject != null && DTEditorUtils.IsGrandParent(root, toggleData.gameObject.transform))
                    {
                        // renew path if changed
                        toggleData.isInvalid = false;
                        toggle.path = DTEditorUtils.GetRelativePath(toggleData.gameObject.transform, root);
                        toggle.state = toggleData.state;
                        _parentView.UpdateAvatarPreview();
                    }
                    else
                    {
                        toggleData.isInvalid = true;
                    }
                };
                toggleData.removeButtonClickEvent = () =>
                {
                    toggles.Remove(toggle);
                    toggleDataList.Remove(toggleData);
                    _parentView.UpdateAvatarPreview();
                    updateView?.Invoke();
                };

                toggleDataList.Add(toggleData);
            }
        }

        private string[] GetBlendshapeNames(Mesh mesh)
        {
            if (mesh == null)
            {
                return null;
            }

            string[] names = new string[mesh.blendShapeCount];
            for (var i = 0; i < names.Length; i++)
            {
                names[i] = mesh.GetBlendShapeName(i);
            }
            return names;
        }

        private void UpdateBlendshapes(Transform root, List<AnimationBlendshapeValue> blendshapes, List<BlendshapeData> blendshapeDataList, Action updateView = null)
        {
            blendshapeDataList.Clear();
            foreach (var blendshape in blendshapes)
            {
                var transform = blendshape.path != null ? root.Find(blendshape.path) : null;
                var smr = transform?.GetComponent<SkinnedMeshRenderer>();
                var mesh = smr != null ? smr.sharedMesh : null;

                var blendshapeNames = GetBlendshapeNames(mesh);
                var blendshapeData = new BlendshapeData()
                {
                    isInvalid = transform == null || mesh == null,
                    gameObject = transform?.gameObject,
                    availableBlendshapeNames = blendshapeNames
                };

                // select blendshape index
                if (blendshapeNames != null)
                {
                    var selectedBlendshapeIndex = System.Array.IndexOf(blendshapeData.availableBlendshapeNames, blendshape.blendshapeName);
                    if (selectedBlendshapeIndex == -1)
                    {
                        selectedBlendshapeIndex = 0;
                    }
                    blendshapeData.selectedBlendshapeIndex = selectedBlendshapeIndex;
                }

                blendshapeData.gameObjectFieldChangeEvent = () =>
                {
                    var newSmr = blendshapeData.gameObject?.GetComponent<SkinnedMeshRenderer>();
                    var newMesh = newSmr != null ? newSmr.sharedMesh : null;
                    if (blendshapeData.gameObject != null && newMesh != null && newMesh.blendShapeCount > 0 && DTEditorUtils.IsGrandParent(root, blendshapeData.gameObject.transform))
                    {
                        // renew path if changed
                        blendshapeData.isInvalid = false;
                        blendshape.path = DTEditorUtils.GetRelativePath(blendshapeData.gameObject.transform, root);

                        // generate blendshape names
                        blendshapeData.availableBlendshapeNames = GetBlendshapeNames(newMesh);
                        blendshapeData.selectedBlendshapeIndex = 0;
                        blendshapeData.value = 0;

                        blendshape.blendshapeName = blendshapeData.availableBlendshapeNames[blendshapeData.selectedBlendshapeIndex];
                        blendshape.value = blendshapeData.value;
                        _parentView.UpdateAvatarPreview();
                    }
                    else
                    {
                        blendshapeData.isInvalid = true;
                    }
                };
                blendshapeData.blendshapeNameChangeEvent = () =>
                {
                    blendshape.blendshapeName = blendshapeData.availableBlendshapeNames[blendshapeData.selectedBlendshapeIndex];
                    _parentView.UpdateAvatarPreview();
                };
                blendshapeData.sliderChangeEvent = () =>
                {
                    blendshape.value = blendshapeData.value;
                    _parentView.UpdateAvatarPreview();
                };
                blendshapeData.removeButtonClickEvent = () =>
                {
                    blendshapes.Remove(blendshape);
                    blendshapeDataList.Remove(blendshapeData);
                    _parentView.UpdateAvatarPreview();
                    updateView?.Invoke();
                };

                blendshapeDataList.Add(blendshapeData);
            }
        }

        private void UpdateAnimationPreset(Transform root, Dictionary<string, AnimationPreset> savedPresets, AnimationPreset preset, PresetViewData presetData, Action updateView)
        {
            if (savedPresets != null)
            {
                var savedPresetKeys = new string[savedPresets.Keys.Count + 1];
                savedPresetKeys[0] = SavedPresetUnselectedPlaceholder;
                savedPresets.Keys.CopyTo(savedPresetKeys, 1);
                presetData.savedPresetKeys = savedPresetKeys;
            }
            else
            {
                presetData.savedPresetKeys = new string[] { SavedPresetUnselectedPlaceholder };
            }

            UpdateToggles(root, preset.toggles, presetData.toggles, updateView);
            UpdateBlendshapes(root, preset.blendshapes, presetData.blendshapes, updateView);
        }

        private bool IsGameObjectUsedInToggles(GameObject go, List<ToggleData> toggleData)
        {
            foreach (var toggle in toggleData)
            {
                if (toggle.gameObject == go)
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateAvatarOnWearToggleSuggestions(PresetViewData presetData)
        {
            presetData.toggleSuggestions.Clear();

            var targetAvatar = _parentView.TargetAvatar;
            var targetWearable = _parentView.TargetWearable;

            if (targetAvatar != null && targetWearable != null && _cabinetConfig != null)
            {
                var armatureName = _cabinetConfig.avatarArmatureName;
                var avatarTrans = targetAvatar.transform;

                // iterate through childs
                for (var i = 0; i < avatarTrans.childCount; i++)
                {
                    var childTrans = avatarTrans.GetChild(i);
                    if (childTrans != targetWearable.transform && childTrans.name != armatureName && !IsGameObjectUsedInToggles(childTrans.gameObject, presetData.toggles))
                    {
                        var toggleSuggestion = new ToggleSuggestionData
                        {
                            gameObject = childTrans.gameObject,
                            state = !childTrans.gameObject.activeSelf,
                            addButtonClickEvent = () =>
                            {
                                _module.avatarAnimationOnWear.toggles.Add(new AnimationToggle()
                                {
                                    path = AnimationUtils.GetRelativePath(childTrans, avatarTrans),
                                    state = !childTrans.gameObject.activeSelf
                                });
                                UpdateAnimationGenerationAvatarOnWear();
                                _parentView.UpdateAvatarPreview();
                            }
                        };
                        presetData.toggleSuggestions.Add(toggleSuggestion);
                    }
                }
            }
        }

        private void UpdateWearableOnWearToggleSuggestions(PresetViewData presetData)
        {
            presetData.toggleSuggestions.Clear();

            var targetWearable = _parentView.TargetWearable;

            if (targetWearable != null)
            {
                return;
            }

            var wearableTrans = targetWearable.transform;

            // TODO: we can't obtain wearable armature name here, listing everything at the root for now

            // iterate through childs
            for (var i = 0; i < wearableTrans.childCount; i++)
            {
                var childTrans = wearableTrans.GetChild(i);
                if (!IsGameObjectUsedInToggles(childTrans.gameObject, presetData.toggles))
                {
                    var toggleSuggestion = new ToggleSuggestionData
                    {
                        gameObject = childTrans.gameObject,
                        state = childTrans.gameObject.activeSelf,
                        addButtonClickEvent = () =>
                        {
                            _module.wearableAnimationOnWear.toggles.Add(new AnimationToggle()
                            {
                                path = AnimationUtils.GetRelativePath(childTrans, wearableTrans),
                                state = !childTrans.gameObject.activeSelf
                            });
                            UpdateAnimationGenerationWearableOnWear();
                            _parentView.UpdateAvatarPreview();
                        }
                    };
                    presetData.toggleSuggestions.Add(toggleSuggestion);
                }
            }
        }

        private void UpdateAnimationGenerationAvatarOnWear()
        {
            if (_parentView.TargetAvatar != null)
            {
                Dictionary<string, AnimationPreset> savedPresets = null;
                if (_cabinetConfig != null)
                {
                    var agcm = DTEditorUtils.FindCabinetModuleConfig<AnimationGenerationCabinetModuleConfig>(_cabinetConfig);
                    if (agcm != null)
                    {
                        savedPresets = agcm.savedAvatarPresets;
                    }
                }

                _view.ShowCannotRenderPresetWithoutTargetAvatarHelpBox = false;
                UpdateAnimationPreset(_parentView.TargetAvatar.transform, savedPresets, _module.avatarAnimationOnWear, _view.AvatarOnWearPresetData, () => UpdateAnimationGenerationAvatarOnWear());
                UpdateAvatarOnWearToggleSuggestions(_view.AvatarOnWearPresetData);
            }
            else
            {
                _view.ShowCannotRenderPresetWithoutTargetAvatarHelpBox = true;
            }
        }

        private void UpdateAnimationGenerationWearableOnWear()
        {
            if (_parentView.TargetWearable != null)
            {
                Dictionary<string, AnimationPreset> savedPresets = null;
                if (_cabinetConfig != null)
                {
                    var agcm = DTEditorUtils.FindCabinetModuleConfig<AnimationGenerationCabinetModuleConfig>(_cabinetConfig);
                    if (agcm != null)
                    {
                        savedPresets = agcm.savedWearablePresets;
                    }
                }

                _view.ShowCannotRenderPresetWithoutTargetWearableHelpBox = false;
                UpdateAnimationPreset(_parentView.TargetWearable.transform, savedPresets, _module.wearableAnimationOnWear, _view.WearableOnWearPresetData, () => UpdateAnimationGenerationWearableOnWear());
                UpdateWearableOnWearToggleSuggestions(_view.WearableOnWearPresetData);
            }
            else
            {
                _view.ShowCannotRenderPresetWithoutTargetWearableHelpBox = true;
            }
        }

        private void UpdateCustomizableAvatarToggles(WearableCustomizable wearable, CustomizableViewData customizableData) => UpdateToggles(_parentView.TargetAvatar.transform, wearable.avatarToggles, customizableData.avatarToggles);
        private void UpdateCustomizableWearableToggles(WearableCustomizable wearable, CustomizableViewData customizableData) => UpdateToggles(_parentView.TargetWearable.transform, wearable.wearableToggles, customizableData.wearableToggles);
        private void UpdateCustomizableAvatarBlendshapes(WearableCustomizable wearable, CustomizableViewData customizableData) => UpdateBlendshapes(_parentView.TargetAvatar.transform, wearable.avatarBlendshapes, customizableData.avatarBlendshapes);
        private void UpdateCustomizableWearableBlendshapes(WearableCustomizable wearable, CustomizableViewData customizableData) => UpdateBlendshapes(_parentView.TargetWearable.transform, wearable.wearableBlendshapes, customizableData.wearableBlendshapes);

        private void UpdateCustomizables()
        {
            _view.Customizables.Clear();

            if (_parentView.TargetAvatar == null || _parentView.TargetWearable == null)
            {
                return;
            }

            foreach (var wearable in _module.wearableCustomizables)
            {
                var customizableData = new CustomizableViewData
                {
                    name = wearable.name,
                    type = (int)wearable.type,
                    defaultValue = wearable.defaultValue
                };

                customizableData.removeButtonClickEvent = () =>
                {
                    _module.wearableCustomizables.Remove(wearable);
                    _view.Customizables.Remove(customizableData);
                };

                customizableData.customizableSettingsChangeEvent = () =>
                {
                    wearable.name = customizableData.name;
                    wearable.type = (WearableCustomizableType)customizableData.type;
                    wearable.defaultValue = customizableData.defaultValue;
                };

                customizableData.addAvatarToggleEvent = () =>
                {
                    wearable.avatarToggles.Add(new AnimationToggle());
                    UpdateCustomizableAvatarToggles(wearable, customizableData);
                };
                customizableData.addAvatarBlendshapeEvent = () =>
                {
                    wearable.avatarBlendshapes.Add(new AnimationBlendshapeValue());
                    UpdateCustomizableAvatarBlendshapes(wearable, customizableData);
                };
                customizableData.addWearableToggleEvent = () =>
                {
                    wearable.wearableToggles.Add(new AnimationToggle());
                    UpdateCustomizableWearableToggles(wearable, customizableData);
                };
                customizableData.addWearableBlendshapeEvent = () =>
                {
                    wearable.wearableBlendshapes.Add(new AnimationBlendshapeValue());
                    UpdateCustomizableWearableBlendshapes(wearable, customizableData);
                };

                UpdateCustomizableAvatarToggles(wearable, customizableData);
                UpdateCustomizableAvatarBlendshapes(wearable, customizableData);
                UpdateCustomizableWearableToggles(wearable, customizableData);
                UpdateCustomizableWearableBlendshapes(wearable, customizableData);

                _view.Customizables.Add(customizableData);
            }
        }

        private void UpdateView()
        {
            _cabinet = DTEditorUtils.GetAvatarCabinet(_parentView.TargetAvatar);
            if (_cabinet != null)
            {
                if (!CabinetConfig.TryDeserialize(_cabinet.configJson, out _cabinetConfig))
                {
                    Debug.LogError("[DressingTools] [AnimationGenerationWearableModuleEditorPresenter] Unable to deserialize cabinet config!");
                }
                else
                {
                    _moduleConfig = DTEditorUtils.FindCabinetModuleConfig<AnimationGenerationCabinetModuleConfig>(_cabinetConfig);
                    if (_moduleConfig == null)
                    {
                        // add the cabinet module if not exist
                        _moduleConfig = new AnimationGenerationCabinetModuleConfig();
                        _cabinetConfig.modules.Add(new CabinetModule()
                        {
                            moduleName = AnimationGenerationCabinetModuleProvider.MODULE_IDENTIFIER,
                            config = _moduleConfig
                        });
                        _cabinet.configJson = _cabinetConfig.Serialize();
                    }
                }
            }
            else
            {
                _cabinetConfig = null;
            }

            UpdateAnimationGenerationAvatarOnWear();
            UpdateAnimationGenerationWearableOnWear();
            UpdateCustomizables();
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
