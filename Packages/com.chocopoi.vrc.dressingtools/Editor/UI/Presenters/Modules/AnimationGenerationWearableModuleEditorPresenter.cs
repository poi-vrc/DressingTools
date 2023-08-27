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

using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Cabinet.Modules;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Cabinet.Modules;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
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

            _parentView.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
        }

        private void UpdateSelectedPresetData(Dictionary<string, AnimationPreset> savedPresets, PresetData presetData)
        {
            if (presetData.selectedPresetIndex == 0)
            {
                return;
            }
            var key = presetData.savedPresetKeys[presetData.selectedPresetIndex];
            _module.avatarAnimationOnWear = new AnimationPreset(savedPresets[key]);
        }

        private void SavePreset(Dictionary<string, AnimationPreset> savedPresets, PresetData presetData, AnimationPreset presetToSave)
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
            _cabinet.configJson = _cabinetConfig.ToString();
            UpdateView();
        }

        private void DeletePreset(Dictionary<string, AnimationPreset> savedPresets, PresetData presetData)
        {
            if (presetData.selectedPresetIndex == 0)
            {
                return;
            }

            var key = presetData.savedPresetKeys[presetData.selectedPresetIndex];
            savedPresets.Remove(key);
            presetData.selectedPresetIndex = 0;

            // serialize to cabinet
            _cabinet.configJson = _cabinetConfig.ToString();
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

        private void UpdateAnimationPresetToggles(Transform root, AnimationPreset preset, List<ToggleData> toggleDataList)
        {
            toggleDataList.Clear();
            foreach (var toggle in preset.toggles)
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
                    }
                    else
                    {
                        toggleData.isInvalid = true;
                    }
                };
                toggleData.removeButtonClickEvent = () =>
                {
                    preset.toggles.Remove(toggle);
                    toggleDataList.Remove(toggleData);
                    UpdateAnimationGenerationAvatarOnWear();
                    UpdateAnimationGenerationWearableOnWear();
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

        private void UpdateAnimationPresetBlendshapes(Transform root, AnimationPreset preset, List<BlendshapeData> blendshapeDataList)
        {
            blendshapeDataList.Clear();
            foreach (var blendshape in preset.blendshapes)
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
                    }
                    else
                    {
                        blendshapeData.isInvalid = true;
                    }
                };
                blendshapeData.blendshapeNameChangeEvent = () => blendshape.blendshapeName = blendshapeData.availableBlendshapeNames[blendshapeData.selectedBlendshapeIndex];
                blendshapeData.sliderChangeEvent = () => blendshape.value = blendshapeData.value;
                blendshapeData.removeButtonClickEvent = () =>
                {
                    preset.blendshapes.Remove(blendshape);
                    blendshapeDataList.Remove(blendshapeData);
                };

                blendshapeDataList.Add(blendshapeData);
            }
        }

        private void UpdateAnimationPreset(Transform root, Dictionary<string, AnimationPreset> savedPresets, AnimationPreset preset, PresetData presetData)
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

            UpdateAnimationPresetToggles(root, preset, presetData.toggles);
            UpdateAnimationPresetBlendshapes(root, preset, presetData.blendshapes);
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

        private void UpdateAvatarOnWearToggleSuggestions(PresetData presetData)
        {
            var targetAvatar = _parentView.TargetAvatar;
            var targetWearable = _parentView.TargetWearable;

            presetData.toggleSuggestions.Clear();
            if (_cabinetConfig != null)
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
                            }
                        };
                        presetData.toggleSuggestions.Add(toggleSuggestion);
                    }
                }
            }
        }

        private void UpdateWearableOnWearToggleSuggestions(PresetData presetData)
        {
            var targetWearable = _parentView.TargetWearable;

            presetData.toggleSuggestions.Clear();
            var wearableTrans = targetWearable.transform;

            // TODO: we can't obtain wearable armature name here, listing everything at the root for now

            // iterate through childs
            for (var i = 0; i < wearableTrans.childCount; i++)
            {
                var childTrans = wearableTrans.GetChild(i);
                if (childTrans != targetWearable.transform && !IsGameObjectUsedInToggles(childTrans.gameObject, presetData.toggles))
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
                UpdateAnimationPreset(_parentView.TargetAvatar.transform, savedPresets, _module.avatarAnimationOnWear, _view.AvatarOnWearPresetData);
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
                UpdateAnimationPreset(_parentView.TargetWearable.transform, savedPresets, _module.wearableAnimationOnWear, _view.WearableOnWearPresetData);
                UpdateWearableOnWearToggleSuggestions(_view.WearableOnWearPresetData);
            }
            else
            {
                _view.ShowCannotRenderPresetWithoutTargetWearableHelpBox = true;
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
                        _cabinet.configJson = _cabinetConfig.ToString();
                    }
                }
            }
            else
            {
                _cabinetConfig = null;
            }

            UpdateAnimationGenerationAvatarOnWear();
            UpdateAnimationGenerationWearableOnWear();
            // TODO: customizables
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
