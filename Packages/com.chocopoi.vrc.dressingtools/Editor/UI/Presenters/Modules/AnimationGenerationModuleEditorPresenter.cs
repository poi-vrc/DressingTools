using System.Collections.Generic;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class AnimationGenerationModuleEditorPresenter
    {
        private IAnimationGenerationModuleEditorView _view;
        private IModuleEditorViewParent _parentView;
        private AnimationGenerationModule _module;

        public AnimationGenerationModuleEditorPresenter(IAnimationGenerationModuleEditorView view, IModuleEditorViewParent parentView, AnimationGenerationModule module)
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

            _view.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;

            _view.AvatarOnWearToggleAddEvent += OnAvatarOnWearToggleAddEvent;
            _view.AvatarOnWearBlendshapeAddEvent += OnAvatarOnWearBlendshapeAddEvent;

            _view.WearableOnWearToggleAddEvent += OnWearableOnWearToggleAddEvent;
            _view.WearableOnWearBlendshapeAddEvent += OnWearableOnWearBlendshapeAddEvent;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;

            _view.AvatarOnWearToggleAddEvent -= OnAvatarOnWearToggleAddEvent;
            _view.AvatarOnWearBlendshapeAddEvent -= OnAvatarOnWearBlendshapeAddEvent;

            _view.WearableOnWearToggleAddEvent -= OnWearableOnWearToggleAddEvent;
            _view.WearableOnWearBlendshapeAddEvent -= OnWearableOnWearBlendshapeAddEvent;
        }

        private void OnTargetAvatarOrWearableChange()
        {
            UpdateView();
        }

        private void OnAvatarOnWearToggleAddEvent()
        {
            _module.avatarAnimationOnWear.toggles.Add(new DTAnimationToggle());
            UpdateAnimationGenerationAvatarOnWear();
        }

        private void OnAvatarOnWearBlendshapeAddEvent()
        {
            _module.avatarAnimationOnWear.blendshapes.Add(new DTAnimationBlendshapeValue());
            UpdateAnimationGenerationAvatarOnWear();
        }

        private void OnWearableOnWearToggleAddEvent()
        {
            _module.wearableAnimationOnWear.toggles.Add(new DTAnimationToggle());
            UpdateAnimationGenerationWearableOnWear();
        }

        private void OnWearableOnWearBlendshapeAddEvent()
        {
            _module.wearableAnimationOnWear.blendshapes.Add(new DTAnimationBlendshapeValue());
            UpdateAnimationGenerationWearableOnWear();
        }

        private void UpdateAnimationPresetToggles(Transform root, DTAnimationPreset preset, List<ToggleData> toggleDataList)
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
                    if (toggleData.gameObject != null && DTRuntimeUtils.IsGrandParent(root, toggleData.gameObject.transform))
                    {
                        // renew path if changed
                        toggleData.isInvalid = false;
                        toggle.path = DTRuntimeUtils.GetRelativePath(toggleData.gameObject.transform, root);
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

        private void UpdateAnimationPresetBlendshapes(Transform root, DTAnimationPreset preset, List<BlendshapeData> blendshapeDataList)
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
                    if (blendshapeData.gameObject != null && newMesh != null && newMesh.blendShapeCount > 0 && DTRuntimeUtils.IsGrandParent(root, blendshapeData.gameObject.transform))
                    {
                        // renew path if changed
                        blendshapeData.isInvalid = false;
                        blendshape.path = DTRuntimeUtils.GetRelativePath(blendshapeData.gameObject.transform, root);

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

        private void UpdateAnimationPreset(Transform root, DTAnimationPreset preset, PresetData presetData)
        {
            UpdateAnimationPresetToggles(root, preset, presetData.toggles);
            UpdateAnimationPresetBlendshapes(root, preset, presetData.blendshapes);
        }

        private void UpdateAnimationGenerationAvatarOnWear()
        {
            if (_parentView.TargetAvatar != null)
            {
                _view.ShowCannotRenderPresetWithoutTargetAvatarHelpBox = false;
                UpdateAnimationPreset(_parentView.TargetAvatar.transform, _module.avatarAnimationOnWear, _view.AvatarOnWearPresetData);
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
                _view.ShowCannotRenderPresetWithoutTargetWearableHelpBox = false;
                UpdateAnimationPreset(_parentView.TargetWearable.transform, _module.wearableAnimationOnWear, _view.WearableOnWearPresetData);
            }
            else
            {
                _view.ShowCannotRenderPresetWithoutTargetWearableHelpBox = true;
            }
        }

        private void UpdateView()
        {
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
