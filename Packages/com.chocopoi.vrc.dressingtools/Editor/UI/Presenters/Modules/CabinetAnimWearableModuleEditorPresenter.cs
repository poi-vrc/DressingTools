/*
 * File: CabinetAnimWearableModuleEditorPresenter.cs
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
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Cabinet;
using Chocopoi.DressingFramework.Cabinet.Modules;
using Chocopoi.DressingFramework.Cabinet.Modules.BuiltIn;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingFramework.UI;
using Chocopoi.DressingFramework.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class CabinetAnimWearableModuleEditorPresenter
    {
        private const string SavedPresetUnselectedPlaceholder = "---";

        private ICabinetAnimWearableModuleEditorView _view;
        private IWearableModuleEditorViewParent _parentView;
        private CabinetAnimWearableModuleConfig _module;
        private ICabinet _cabinet;
        private CabinetConfig _cabinetConfig;
        private CabinetAnimCabinetModuleConfig _moduleConfig;

        public CabinetAnimWearableModuleEditorPresenter(ICabinetAnimWearableModuleEditorView view, IWearableModuleEditorViewParent parentView, CabinetAnimWearableModuleConfig module)
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
            _module.wearableCustomizables.Add(new CabinetAnimWearableModuleConfig.Customizable()
            {
                name = "Customizable-" + DKRuntimeUtils.RandomString(6)
            });
            UpdateCustomizables();
        }

        private void UpdateSelectedPresetData(Dictionary<string, CabinetAnimWearableModuleConfig.Preset> savedPresets, PresetViewData presetData)
        {
            if (presetData.selectedPresetIndex == 0)
            {
                return;
            }
            var key = presetData.savedPresetKeys[presetData.selectedPresetIndex];
            _module.avatarAnimationOnWear = new CabinetAnimWearableModuleConfig.Preset(savedPresets[key]);
        }

        private void SavePreset(Dictionary<string, CabinetAnimWearableModuleConfig.Preset> savedPresets, PresetViewData presetData, CabinetAnimWearableModuleConfig.Preset presetToSave)
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
            _cabinet.ConfigJson = CabinetConfigUtility.Serialize(_cabinetConfig);
            UpdateView();
        }

        private void DeletePreset(Dictionary<string, CabinetAnimWearableModuleConfig.Preset> savedPresets, PresetViewData presetData)
        {
            if (presetData.selectedPresetIndex == 0)
            {
                return;
            }

            var key = presetData.savedPresetKeys[presetData.selectedPresetIndex];
            savedPresets.Remove(key);
            presetData.selectedPresetIndex = 0;

            // serialize to cabinet
            _cabinet.ConfigJson = CabinetConfigUtility.Serialize(_cabinetConfig);
            UpdateView();
        }

        private void OnAvatarOnWearPresetChangeEvent()
        {
            if (_cabinetConfig != null)
            {
                var agcm = _cabinetConfig.FindModuleConfig<CabinetAnimCabinetModuleConfig>();
                if (agcm != null)
                {
                    UpdateSelectedPresetData(agcm.savedAvatarPresets, _view.AvatarOnWearPresetData);
                    UpdateCabinetAnimAvatarOnWear();
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
                var agcm = _cabinetConfig.FindModuleConfig<CabinetAnimCabinetModuleConfig>();
                if (agcm != null)
                {
                    UpdateSelectedPresetData(agcm.savedWearablePresets, _view.WearableOnWearPresetData);
                    UpdateCabinetAnimWearableOnWear();
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
            _module.avatarAnimationOnWear.toggles.Add(new CabinetAnimWearableModuleConfig.Toggle());
            UpdateCabinetAnimAvatarOnWear();
        }

        private void OnAvatarOnWearBlendshapeAddEvent()
        {
            _module.avatarAnimationOnWear.blendshapes.Add(new CabinetAnimWearableModuleConfig.BlendshapeValue());
            UpdateCabinetAnimAvatarOnWear();
        }

        private void OnWearableOnWearToggleAddEvent()
        {
            _module.wearableAnimationOnWear.toggles.Add(new CabinetAnimWearableModuleConfig.Toggle());
            UpdateCabinetAnimWearableOnWear();
        }

        private void OnWearableOnWearBlendshapeAddEvent()
        {
            _module.wearableAnimationOnWear.blendshapes.Add(new CabinetAnimWearableModuleConfig.BlendshapeValue());
            UpdateCabinetAnimWearableOnWear();
        }

        private void UpdateToggles(Transform root, List<CabinetAnimWearableModuleConfig.Toggle> toggles, List<ToggleData> toggleDataList, Action updateView = null)
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
                    if (toggleData.gameObject != null && DKRuntimeUtils.IsGrandParent(root, toggleData.gameObject.transform))
                    {
                        // renew path if changed
                        toggleData.isInvalid = false;
                        toggle.path = AnimationUtils.GetRelativePath(toggleData.gameObject.transform, root);
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
            var names = new List<string>
            {
                "---"
            };

            if (mesh == null)
            {
                return names.ToArray();
            }

            for (var i = 0; i < mesh.blendShapeCount; i++)
            {
                names.Add(mesh.GetBlendShapeName(i));
            }

            return names.ToArray();
        }

        private void UpdateBlendshapes(Transform root, List<CabinetAnimWearableModuleConfig.BlendshapeValue> blendshapes, List<BlendshapeData> blendshapeDataList, Action updateView = null)
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
                    availableBlendshapeNames = blendshapeNames,
                    value = blendshape.value
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
                    if (blendshapeData.gameObject != null && newMesh != null && newMesh.blendShapeCount > 0 && DKRuntimeUtils.IsGrandParent(root, blendshapeData.gameObject.transform))
                    {
                        // renew path if changed
                        blendshapeData.isInvalid = false;
                        blendshape.path = AnimationUtils.GetRelativePath(blendshapeData.gameObject.transform, root);

                        // generate blendshape names
                        blendshapeData.availableBlendshapeNames = GetBlendshapeNames(newMesh);
                        blendshapeData.selectedBlendshapeIndex = 0;
                        blendshapeData.value = 0;

                        // blendshape.blendshapeName = blendshapeData.availableBlendshapeNames[blendshapeData.selectedBlendshapeIndex];
                        // blendshape.value = blendshapeData.value;
                        // _parentView.UpdateAvatarPreview();
                    }
                    else
                    {
                        blendshapeData.isInvalid = true;
                    }
                };
                blendshapeData.blendshapeNameChangeEvent = () =>
                {
                    if (blendshapeData.selectedBlendshapeIndex != 0)
                    {
                        blendshape.blendshapeName = blendshapeData.availableBlendshapeNames[blendshapeData.selectedBlendshapeIndex];
                        _parentView.UpdateAvatarPreview();
                    }
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

        private void UpdateAnimationPreset(Transform root, Dictionary<string, CabinetAnimWearableModuleConfig.Preset> savedPresets, CabinetAnimWearableModuleConfig.Preset preset, PresetViewData presetData, Action updateView)
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

        private bool IsGameObjectUsedInBlendshapes(GameObject go, List<BlendshapeData> blendshapeData)
        {
            foreach (var blendshape in blendshapeData)
            {
                if (blendshape.gameObject == go)
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateToggleAndSmrSuggestions(Transform root, List<CabinetAnimWearableModuleConfig.Toggle> realToggles, List<ToggleData> toggles,
             List<ToggleSuggestionData> toggleSuggestions, List<CabinetAnimWearableModuleConfig.BlendshapeValue> realBlendshapes, List<BlendshapeData> blendshapes,
              List<SmrSuggestionData> smrSuggestions, Action updateView, bool inverted, string armatureName = null, Transform skipTransform = null)
        {
            toggleSuggestions.Clear();
            smrSuggestions.Clear();

            // iterate through childs
            for (var i = 0; i < root.childCount; i++)
            {
                var childTrans = root.GetChild(i);

                if ((skipTransform == null || skipTransform != childTrans) &&
                    (armatureName == null || childTrans.name != armatureName))
                {
                    if (!IsGameObjectUsedInToggles(childTrans.gameObject, toggles))
                    {
                        var toggleSuggestion = new ToggleSuggestionData
                        {
                            gameObject = childTrans.gameObject,
                            state = childTrans.gameObject.activeSelf ^ inverted,
                            addButtonClickEvent = () =>
                            {
                                realToggles.Add(new CabinetAnimWearableModuleConfig.Toggle()
                                {
                                    path = AnimationUtils.GetRelativePath(childTrans, root),
                                    state = childTrans.gameObject.activeSelf ^ inverted
                                });
                                _parentView.UpdateAvatarPreview();
                                updateView.Invoke();
                            }
                        };
                        toggleSuggestions.Add(toggleSuggestion);
                    }

                    if (childTrans.TryGetComponent<SkinnedMeshRenderer>(out _))
                    {
                        var smrSuggestion = new SmrSuggestionData
                        {
                            gameObject = childTrans.gameObject,
                            addButtonClickEvent = () =>
                            {
                                realBlendshapes.Add(new CabinetAnimWearableModuleConfig.BlendshapeValue()
                                {
                                    path = AnimationUtils.GetRelativePath(childTrans, root),
                                    blendshapeName = "",
                                    value = 0
                                });
                                _parentView.UpdateAvatarPreview();
                                updateView.Invoke();
                            }
                        };
                        smrSuggestions.Add(smrSuggestion);
                    }
                }
            }
        }

        private void UpdateAvatarOnWearToggleAndSmrSuggestions(PresetViewData presetData)
        {
            var targetAvatar = _parentView.TargetAvatar;
            var targetWearable = _parentView.TargetWearable;

            if (targetAvatar != null && targetWearable != null && _cabinetConfig != null)
            {
                var armatureName = _cabinetConfig.avatarArmatureName;
                var avatarTrans = targetAvatar.transform;
                var wearableTrans = targetWearable.transform;

                UpdateToggleAndSmrSuggestions(
                    avatarTrans, _module.avatarAnimationOnWear.toggles, presetData.toggles, presetData.toggleSuggestions,
                     _module.avatarAnimationOnWear.blendshapes, presetData.blendshapes, presetData.smrSuggestions,
                      () => UpdateCabinetAnimAvatarOnWear(), true, armatureName, wearableTrans);
            }
        }

        private void UpdateWearableOnWearToggleAndSmrSuggestions(PresetViewData presetData)
        {
            var targetWearable = _parentView.TargetWearable;

            if (targetWearable != null)
            {
                var wearableTrans = targetWearable.transform;
                UpdateToggleAndSmrSuggestions(
                    wearableTrans, _module.wearableAnimationOnWear.toggles, presetData.toggles, presetData.toggleSuggestions,
                     _module.avatarAnimationOnWear.blendshapes, presetData.blendshapes, presetData.smrSuggestions,
                      () => UpdateCabinetAnimWearableOnWear(), false);
            }
        }

        private void UpdateCabinetAnimAvatarOnWear()
        {
            if (_parentView.TargetAvatar != null)
            {
                Dictionary<string, CabinetAnimWearableModuleConfig.Preset> savedPresets = null;
                if (_cabinetConfig != null)
                {
                    var agcm = _cabinetConfig.FindModuleConfig<CabinetAnimCabinetModuleConfig>();
                    if (agcm != null)
                    {
                        savedPresets = agcm.savedAvatarPresets;
                    }
                }

                _view.ShowCannotRenderPresetWithoutTargetAvatarHelpBox = false;
                UpdateAnimationPreset(_parentView.TargetAvatar.transform, savedPresets, _module.avatarAnimationOnWear, _view.AvatarOnWearPresetData, () => UpdateCabinetAnimAvatarOnWear());
                UpdateAvatarOnWearToggleAndSmrSuggestions(_view.AvatarOnWearPresetData);
            }
            else
            {
                _view.ShowCannotRenderPresetWithoutTargetAvatarHelpBox = true;
            }
        }

        private void UpdateCabinetAnimWearableOnWear()
        {
            if (_parentView.TargetWearable != null)
            {
                Dictionary<string, CabinetAnimWearableModuleConfig.Preset> savedPresets = null;
                if (_cabinetConfig != null)
                {
                    var agcm = _cabinetConfig.FindModuleConfig<CabinetAnimCabinetModuleConfig>();
                    if (agcm != null)
                    {
                        savedPresets = agcm.savedWearablePresets;
                    }
                }

                _view.ShowCannotRenderPresetWithoutTargetWearableHelpBox = false;
                UpdateAnimationPreset(_parentView.TargetWearable.transform, savedPresets, _module.wearableAnimationOnWear, _view.WearableOnWearPresetData, () => UpdateCabinetAnimWearableOnWear());
                UpdateWearableOnWearToggleAndSmrSuggestions(_view.WearableOnWearPresetData);
            }
            else
            {
                _view.ShowCannotRenderPresetWithoutTargetWearableHelpBox = true;
            }
        }

        private void UpdateCustomizableAvatarTogglesAndBlendshapes(CabinetAnimWearableModuleConfig.Customizable wearable, CustomizableViewData customizableData)
        {
            var targetAvatar = _parentView.TargetAvatar;
            var targetWearable = _parentView.TargetWearable;

            if (targetAvatar != null && targetWearable != null && _cabinetConfig != null)
            {
                var armatureName = _cabinetConfig.avatarArmatureName;
                var avatarTrans = targetAvatar.transform;
                var wearableTrans = targetWearable.transform;

                UpdateToggles(avatarTrans, wearable.avatarToggles, customizableData.avatarToggles);
                UpdateBlendshapes(_parentView.TargetAvatar.transform, wearable.avatarBlendshapes, customizableData.avatarBlendshapes);
                UpdateToggleAndSmrSuggestions(avatarTrans, wearable.avatarToggles, customizableData.avatarToggles, customizableData.avatarToggleSuggestions,
                    wearable.avatarBlendshapes, customizableData.avatarBlendshapes, customizableData.avatarSmrSuggestions,
                    () => UpdateCustomizableAvatarTogglesAndBlendshapes(wearable, customizableData), true, armatureName, wearableTrans);
            }
        }

        private void UpdateCustomizableWearableTogglesAndBlendshapes(CabinetAnimWearableModuleConfig.Customizable wearable, CustomizableViewData customizableData)
        {
            var targetAvatar = _parentView.TargetAvatar;
            var targetWearable = _parentView.TargetWearable;

            if (targetAvatar != null && targetWearable != null)
            {
                var wearableTrans = targetWearable.transform;

                UpdateToggles(wearableTrans, wearable.wearableToggles, customizableData.wearableToggles);
                UpdateBlendshapes(_parentView.TargetWearable.transform, wearable.wearableBlendshapes, customizableData.wearableBlendshapes);
                UpdateToggleAndSmrSuggestions(wearableTrans, wearable.wearableToggles, customizableData.wearableToggles, customizableData.wearableToggleSuggestions,
                    wearable.wearableBlendshapes, customizableData.wearableBlendshapes, customizableData.wearableSmrSuggestions,
                    () => UpdateCustomizableWearableTogglesAndBlendshapes(wearable, customizableData), true);
            }
        }

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
                    UpdateCustomizableAvatarTogglesAndBlendshapes(wearable, customizableData);
                };

                customizableData.customizableSettingsChangeEvent = () =>
                {
                    wearable.name = customizableData.name;
                    wearable.type = (CabinetAnimWearableModuleConfig.CustomizableType)customizableData.type;
                    wearable.defaultValue = customizableData.defaultValue;
                };

                customizableData.addAvatarToggleEvent = () =>
                {
                    wearable.avatarToggles.Add(new CabinetAnimWearableModuleConfig.Toggle());
                    UpdateCustomizableAvatarTogglesAndBlendshapes(wearable, customizableData);
                };
                customizableData.addAvatarBlendshapeEvent = () =>
                {
                    wearable.avatarBlendshapes.Add(new CabinetAnimWearableModuleConfig.BlendshapeValue());
                    UpdateCustomizableAvatarTogglesAndBlendshapes(wearable, customizableData);
                };
                customizableData.addWearableToggleEvent = () =>
                {
                    wearable.wearableToggles.Add(new CabinetAnimWearableModuleConfig.Toggle());
                    UpdateCustomizableWearableTogglesAndBlendshapes(wearable, customizableData);
                };
                customizableData.addWearableBlendshapeEvent = () =>
                {
                    wearable.wearableBlendshapes.Add(new CabinetAnimWearableModuleConfig.BlendshapeValue());
                    UpdateCustomizableWearableTogglesAndBlendshapes(wearable, customizableData);
                };

                // TODO: refactor, this is a dirty way to clear non-blendshape type customizable stuff
                if (wearable.type == CabinetAnimWearableModuleConfig.CustomizableType.Blendshape)
                {
                    wearable.avatarToggles.Clear();
                    wearable.avatarBlendshapes.Clear();
                    wearable.wearableToggles.Clear();
                }

                UpdateCustomizableAvatarTogglesAndBlendshapes(wearable, customizableData);
                UpdateCustomizableWearableTogglesAndBlendshapes(wearable, customizableData);

                _view.Customizables.Add(customizableData);
            }
        }

        private void UpdateView()
        {
            _cabinet = DKRuntimeUtils.GetAvatarCabinet(_parentView.TargetAvatar);
            if (_cabinet != null)
            {
                if (!CabinetConfigUtility.TryDeserialize(_cabinet.ConfigJson, out _cabinetConfig))
                {
                    Debug.LogError("[DressingTools] [CabinetAnimWearableModuleEditorPresenter] Unable to deserialize cabinet config!");
                }
                else
                {
                    _moduleConfig = _cabinetConfig.FindModuleConfig<CabinetAnimCabinetModuleConfig>();
                    if (_moduleConfig == null)
                    {
                        // add the cabinet module if not exist
                        _moduleConfig = new CabinetAnimCabinetModuleConfig();
                        _cabinetConfig.modules.Add(new CabinetModule()
                        {
                            moduleName = CabinetAnimCabinetModuleConfig.ModuleIdentifier,
                            config = _moduleConfig
                        });
                        _cabinet.ConfigJson = CabinetConfigUtility.Serialize(_cabinetConfig);
                    }
                }
            }
            else
            {
                _cabinetConfig = null;
            }

            UpdateCabinetAnimAvatarOnWear();
            UpdateCabinetAnimWearableOnWear();
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
